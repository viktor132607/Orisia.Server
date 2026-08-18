using Orisia.Server.Data.Entities;

namespace Orisia.Server.Domain.Interfaces;

public interface IEmailNotificationService
{
    Task SendPasswordResetAsync(User user, string resetLink);

    Task SendOrderConfirmationAsync(User user, Order order, string paymentMethod, string deliveryMethod);

    Task SendOrderStatusChangedAsync(User user, Order order);
}
