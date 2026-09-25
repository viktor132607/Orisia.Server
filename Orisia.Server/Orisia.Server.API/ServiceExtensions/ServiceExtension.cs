using Orisia.Server.API.Services;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Data.Repositories;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Services;

namespace Orisia.Server.API.ServiceExtensions;

public static class ServiceExtension
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IGdprService, GdprService>();
        services.AddTransient<IPostService, PostService>();
        services.AddSingleton<IPasswordResetTokenStore, MemoryPasswordResetTokenStore>();
        services.AddSingleton<IEmailNotificationService, ConsoleEmailNotificationService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IEventRepository, EventRepository>();

        return services;
    }
}
