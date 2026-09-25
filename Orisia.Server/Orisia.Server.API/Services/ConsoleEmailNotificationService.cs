using Microsoft.Extensions.Logging;
using Orisia.Server.Data.Entities;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Services;

public class ConsoleEmailNotificationService(
    ILogger<ConsoleEmailNotificationService> logger) : IEmailNotificationService
{
    public Task SendPasswordResetAsync(User user, string resetLink)
    {
        logger.LogInformation(
            "Password reset requested for {Email}. Reset link: {ResetLink}",
            user.Email,
            resetLink);

        return Task.CompletedTask;
    }

    public Task SendContactInquiryReceivedAsync(ContactInquiry inquiry)
    {
        logger.LogInformation(
            "New contact inquiry {InquiryId} received from {Email} with subject {Subject}.",
            inquiry.Id,
            inquiry.Email,
            inquiry.Subject);

        return Task.CompletedTask;
    }

    public Task SendContactInquiryAnswerAsync(ContactInquiry inquiry)
    {
        logger.LogInformation(
            "Contact inquiry {InquiryId} answer notification prepared for {Email}.",
            inquiry.Id,
            inquiry.Email);

        return Task.CompletedTask;
    }
}
