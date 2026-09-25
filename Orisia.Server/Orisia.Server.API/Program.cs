using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Orisia.Server.API.Configuration;
using Orisia.Server.API.Middlewares;
using Orisia.Server.API.ServiceExtensions;
using Orisia.Server.Common.Options;
using Orisia.Server.Data;
using Orisia.Server.Data.Helpers;
using Orisia.Server.Domain.Authentication;
using Orisia.Server.Core.StaticClasses;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateOnStart();
builder.Services.Configure<ClientAppOptions>(builder.Configuration.GetSection(ClientAppOptions.SectionName));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));
builder.Services.Configure<DevelopmentOptions>(builder.Configuration.GetSection(DevelopmentOptions.SectionName));
builder.Services.Configure<MediaStorageOptions>(builder.Configuration.GetSection(MediaStorageOptions.SectionName));
builder.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

JwtOptions jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? throw new InvalidOperationException("JWT configuration is missing.");
JwtSecurityConfiguration.ValidateOrThrow(jwtOptions);

CorsOptions corsOptions = builder.Configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>() ?? new CorsOptions();

builder.Services.AddMemoryCache();
builder.Services.AddOpenApi();
builder.Services.AddCustomServices();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

ResolvedDatabaseConnection resolvedDatabaseConnection;

try
{
    resolvedDatabaseConnection = DatabaseConnectionStringResolver.Resolve(builder.Configuration, builder.Environment);
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Database configuration error: {ex.Message}");
    throw;
}

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql(resolvedDatabaseConnection.ConnectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = JwtSecurityConfiguration.CreateTokenValidationParameters(jwtOptions);
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                string? userIdValue = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdValue, out Guid userId))
                {
                    context.Fail("Invalid user identifier.");
                    return;
                }

                ApplicationDbContext db = context.HttpContext.RequestServices
                    .GetRequiredService<ApplicationDbContext>();

                bool active = await db.Users.AnyAsync(user =>
                    user.Id == userId
                    && !user.IsDeleted
                    && user.IsActive);

                if (!active)
                {
                    context.Fail("User account is inactive.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole(Roles.Admin));

    options.AddPolicy(AuthorizationPolicies.ContentManagement, policy =>
        policy.RequireRole(Roles.Admin, Roles.Editor));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ConfiguredOrigins", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy
                .SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri))
                    {
                        return false;
                    }

                    return uri.Host == "localhost"
                        || uri.Host == "127.0.0.1"
                        || uri.Host.StartsWith("192.168.")
                        || uri.Host.StartsWith("10.")
                        || uri.Host.StartsWith("172.");
                })
                .AllowAnyHeader()
                .AllowAnyMethod();

            return;
        }

        if (corsOptions.AllowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy.WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

WebApplication app = builder.Build();

app.Logger.LogInformation(
    "PostgreSQL connection resolved from configuration key {DatabaseConnectionSource}.",
    resolvedDatabaseConnection.SourceKey);

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseStaticFiles();
app.UseCors("ConfiguredOrigins");

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTheme(ScalarTheme.Moon)
        .WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl);
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    try
    {
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IOptions<DevelopmentOptions> developmentOptionsAccessor = scope.ServiceProvider.GetRequiredService<IOptions<DevelopmentOptions>>();
        DevelopmentOptions developmentOptions = developmentOptionsAccessor.Value;

        if (app.Environment.IsDevelopment() && developmentOptions.ResetDatabaseOnStart)
        {
            await DatabaseUtils.TruncateAllTablesSafeAsync(db);
        }

        app.Logger.LogInformation("Applying Entity Framework Core migrations.");
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(
            ex,
            "Database startup failed while applying migrations using configuration key {DatabaseConnectionSource}.",
            resolvedDatabaseConnection.SourceKey);
        throw;
    }
}

await DbInitializer.SeedAsync(app.Services);

app.Run();
