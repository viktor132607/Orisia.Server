using Orisia.Server.Data.Interfaces;
using Orisia.Server.Data.Repositories;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Services;
using Orisia.Server.API.Services;

namespace Orisia.Server.API.ServiceExtensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            // SERVICES
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IWishlistService, WishlistService>();
            services.AddTransient<IReviewService, ReviewService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IGdprService, GdprService>();
            services.AddSingleton<IPasswordResetTokenStore, MemoryPasswordResetTokenStore>();
            services.AddSingleton<IEmailNotificationService, ConsoleEmailNotificationService>();

            // REPOS
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IWishlistRepository, WishlistRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();

            return services;
        }
    }
}
