using Microsoft.AspNetCore.Identity;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.API.Helpers
{
    public class DummyEmailSender : IEmailSender<User>
    {
        public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
        {
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
        {
            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
        {
            return Task.CompletedTask;
        }
    }
}