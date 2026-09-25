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
}
