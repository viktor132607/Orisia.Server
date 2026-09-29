using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Npgsql;
using Scalar.AspNetCore;
using Serilog;
using Orisia.Server.API.Configuration;
using Orisia.Server.API.Infrastructure;
using Orisia.Server.API.Middlewares;
using Orisia.Server.API.ServiceExtensions;
using Orisia.Server.API.Services;
using Orisia.Server.Common.Options;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Data;
using Orisia.Server.Data.Helpers;
using Orisia.Server.Data.Seed;
using Orisia.Server.Domain.Authentication;
using Orisia.Server.Domain.Interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Orisia.Server.API")
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

string? sentryDsn =
    builder.Configuration["Sentry:Dsn"] ??
    Environment.GetEnvironmentVariable("SENTRY_DSN");

if (!string.IsNullOrWhiteSpace(sentryDsn))
{
    builder.WebHost.UseSentry(options =>
    {
        options.Dsn = sentryDsn;
        options.SendDefaultPii = false;
        options.AttachStacktrace = true;
        options.TracesSampleRate = 0.05;
        options.Environment = builder.Environment.EnvironmentName;
    });
}

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateOnStart();

builder.Services.Configure<ClientAppOptions>(
    builder.Configuration.GetSection(ClientAppOptions.SectionName));
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));
builder.Services.PostConfigure<EmailOptions>(options =>
{
    if (!builder.Environment.IsDevelopment() &&
        options.DeliveryMode.Equals("Console", StringComparison.OrdinalIgnoreCase))
    {
        options.DeliveryMode = "Disabled";
    }
});
builder.Services.Configure<CorsOptions>(
    builder.Configuration.GetSection(CorsOptions.SectionName));
builder.Services.Configure<DevelopmentOptions>(
    builder.Configuration.GetSection(DevelopmentOptions.SectionName));
builder.Services.Configure<MediaStorageOptions>(
    builder.Configuration.GetSection(MediaStorageOptions.SectionName));

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

JwtOptions jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

JwtSecurityConfiguration.ValidateOrThrow(jwtOptions);

CorsOptions corsOptions = builder.Configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>()
    ?? new CorsOptions();

builder.Services.AddMemoryCache();
builder.Services.AddOpenApi();
builder.Services.AddCustomServices();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                statusCode = StatusCodes.Status429TooManyRequests,
                code = "RateLimitExceeded",
                message = "Too many requests.",
                traceId = context.HttpContext.TraceIdentifier
            },
            cancellationToken);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetRateLimitPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetRateLimitPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("inquiry", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetRateLimitPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("upload", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            GetRateLimitPartitionKey(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

ResolvedDatabaseConnection resolvedDatabaseConnection =
    DatabaseConnectionStringResolver.Resolve(
        builder.Configuration,
        builder.Environment);

builder.Services.AddSingleton(sp =>
    new PostgresAdvisoryLock(
        resolvedDatabaseConnection.ConnectionString,
        sp.GetRequiredService<ILogger<PostgresAdvisoryLock>>()));

builder.Services.AddScoped<DatabaseSessionContextInterceptor>();

builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(resolvedDatabaseConnection.ConnectionString);
    options.AddInterceptors(
        serviceProvider.GetRequiredService<DatabaseSessionContextInterceptor>());
});

builder.Services.AddSingleton(
    new DatabaseBackupConnection(
        resolvedDatabaseConnection.ConnectionString));

builder.Services.AddSingleton<IDatabaseBackupOperationLock, DistributedDatabaseBackupOperationLock>();
builder.Services.AddSingleton<IPasswordResetTokenStore>(_ =>
    new PostgresPasswordResetTokenStore(
        resolvedDatabaseConnection.ConnectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            JwtSecurityConfiguration.CreateTokenValidationParameters(jwtOptions);

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                string? userIdValue =
                    context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!Guid.TryParse(userIdValue, out Guid userId))
                {
                    context.Fail("Invalid user identifier.");
                    return;
                }

                ApplicationDbContext db =
                    context.HttpContext.RequestServices
                        .GetRequiredService<ApplicationDbContext>();

                string? tokenRole =
                    context.Principal?.FindFirst(ClaimTypes.Role)?.Value;

                bool active = await db.Users.AnyAsync(user =>
                    user.Id == userId &&
                    !user.IsDeleted &&
                    user.IsActive &&
                    user.Role == tokenRole);

                if (!active)
                {
                    context.Fail("User account is inactive.");
                }
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthorizationPolicies.AdminOnly,
        policy => policy.RequireRole(Roles.Admin));

    options.AddPolicy(
        AuthorizationPolicies.ContentManagement,
        policy => policy.RequireRole(Roles.Admin, Roles.Editor));
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

        string[] origins = corsOptions.AllowedOrigins.Length > 0
            ? corsOptions.AllowedOrigins
            : ["https://orisia-client-zgwt.onrender.com"];

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

WebApplication app = builder.Build();

app.Logger.LogInformation(
    "PostgreSQL connection resolved from configuration key {DatabaseConnectionSource}.",
    resolvedDatabaseConnection.SourceKey);

app.UseForwardedHeaders();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);

        string? userId =
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            diagnosticContext.Set("UserId", userId);
        }
    };
});

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

MediaStorageOptions mediaOptions =
    app.Services.GetRequiredService<IOptions<MediaStorageOptions>>().Value;

string mediaRoot = Path.GetFullPath(
    Path.IsPathRooted(mediaOptions.RootPath)
        ? mediaOptions.RootPath
        : Path.Combine(app.Environment.ContentRootPath, mediaOptions.RootPath));

Directory.CreateDirectory(mediaRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaRoot),
    RequestPath = "/" + mediaOptions.PublicBasePath.Trim('/')
});
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("ConfiguredOrigins");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl);
    });
}

app.MapControllers();

using (IServiceScope scope = app.Services.CreateScope())
{
    try
    {
        ApplicationDbContext db =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        IOptions<DevelopmentOptions> developmentOptionsAccessor =
            scope.ServiceProvider.GetRequiredService<IOptions<DevelopmentOptions>>();

        DevelopmentOptions developmentOptions =
            developmentOptionsAccessor.Value;

        app.Logger.LogInformation("Applying Entity Framework Core migrations.");

        const int maxMigrationAttempts = 5;
        for (int attempt = 1; attempt <= maxMigrationAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                break;
            }
            catch (Exception exception)
                when (attempt < maxMigrationAttempts &&
                      (exception is NpgsqlException || exception is TimeoutException))
            {
                TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                app.Logger.LogWarning(
                    exception,
                    "Transient database startup failure. Attempt {Attempt}/{MaxAttempts}; retrying in {DelaySeconds}s.",
                    attempt,
                    maxMigrationAttempts,
                    delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }

        await AdminBootstrapper.SeedAsync(
            db,
            builder.Configuration["BootstrapAdmin:Email"],
            builder.Configuration["BootstrapAdmin:Password"]);

        if (app.Environment.IsDevelopment() &&
            developmentOptions.ResetDatabaseOnStart)
        {
            await DatabaseUtils.TruncateAllTablesSafeAsync(db);
        }

        if (builder.Configuration.GetValue<bool?>("Seed:ContentData") ?? true)
        {
            await OrisiaContentSeeder.SeedAsync(db);
        }
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

if (app.Environment.IsDevelopment() ||
    builder.Configuration.GetValue<bool>("Seed:DemoData"))
{
    await DbInitializer.SeedAsync(app.Services);
}

app.MapGet("/health/live", () => Results.Ok(new { status = "live" }))
    .AllowAnonymous();

app.MapGet(
        "/health/ready",
        async (ApplicationDbContext db, CancellationToken cancellationToken) =>
            await db.Database.CanConnectAsync(cancellationToken)
                ? Results.Ok(new { status = "ready", database = true })
                : Results.Json(
                    new { status = "unready", database = false },
                    statusCode: StatusCodes.Status503ServiceUnavailable))
    .AllowAnonymous();

app.Run();

static string GetRateLimitPartitionKey(HttpContext context) =>
    context.User.FindFirstValue(ClaimTypes.NameIdentifier)
    ?? context.Connection.RemoteIpAddress?.ToString()
    ?? "unknown";
