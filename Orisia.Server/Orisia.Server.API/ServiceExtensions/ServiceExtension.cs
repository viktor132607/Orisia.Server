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
        services.AddSingleton<IDatabaseBackupOperationLock, DatabaseBackupOperationLock>();
        services.AddSingleton<IDatabaseBackupArchiveValidator, DatabaseBackupArchiveValidator>();
        services.AddSingleton<IDatabaseBackupFileStore, DatabaseBackupFileStore>();
        services.AddSingleton<IPostgresProcessRunner, PostgresProcessRunner>();
        services.AddSingleton<IPostgresBackupTool, PostgresBackupTool>();
        services.AddSingleton<IDatabaseBackupService, DatabaseBackupService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IGdprService, GdprService>();
        services.AddTransient<IPostService, PostService>();
        services.AddTransient<IEventService, EventService>();
        services.AddTransient<ICalendarService, CalendarService>();
        services.AddTransient<IFeedService, FeedService>();
        services.AddTransient<IMediaService, MediaService>();
        services.AddTransient<IGalleryService, GalleryService>();
        services.AddTransient<IReviewService, ReviewService>();
        services.AddTransient<IDanceService, DanceService>();
        services.AddTransient<IInquiryService, InquiryService>();
        services.AddTransient<IAdminDashboardService, AdminDashboardService>();
        services.AddSingleton<IPasswordResetTokenStore, MemoryPasswordResetTokenStore>();
        services.AddHttpClient<IEmailNotificationService, EmailNotificationService>(http => http.Timeout = TimeSpan.FromSeconds(30));
        services.AddSingleton<IMediaStorage, LocalMediaStorage>();
        services.AddSingleton<IImageProcessor, SkiaSharpImageProcessor>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IGalleryRepository, GalleryRepository>();
        services.AddScoped<ISiteReviewRepository, SiteReviewRepository>();
        services.AddScoped<IDanceRepository, DanceRepository>();
        services.AddScoped<IInquiryRepository, InquiryRepository>();

        return services;
    }
}
