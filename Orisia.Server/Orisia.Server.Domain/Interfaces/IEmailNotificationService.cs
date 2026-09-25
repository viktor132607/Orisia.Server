using Orisia.Server.Data.Entities;

namespace Orisia.Server.Domain.Interfaces;

public interface IEmailNotificationService
{
    Task SendPasswordResetAsync(User user, string resetLink);
    Task SendContactInquiryReceivedAsync(ContactInquiry inquiry);
    Task SendContactInquiryAnswerAsync(ContactInquiry inquiry);
}
