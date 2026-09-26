using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Orisia.Server.Common.Options;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.API.Services;

public sealed class EmailNotificationService(
    HttpClient http,
    IOptions<EmailOptions> options,
    IWebHostEnvironment environment,
    ILogger<EmailNotificationService> logger) : IEmailNotificationService
{
    public Task SendPasswordResetAsync(User user, string resetLink) => SendAsync(
        user.Email, "Orisia — password reset",
        $"<p>Reset your password:</p><p><a href=\"{WebUtility.HtmlEncode(resetLink)}\">Reset password</a></p>");

    public Task SendContactInquiryReceivedAsync(ContactInquiry inquiry) =>
        string.IsNullOrWhiteSpace(options.Value.InquiryRecipient)
            ? Task.CompletedTask
            : SendAsync(options.Value.InquiryRecipient, "Orisia — new inquiry",
                $"<p>{WebUtility.HtmlEncode(inquiry.Name)} ({WebUtility.HtmlEncode(inquiry.Email)})</p><h2>{WebUtility.HtmlEncode(inquiry.Subject)}</h2><p>{WebUtility.HtmlEncode(inquiry.Message)}</p>");

    public Task SendContactInquiryAnswerAsync(ContactInquiry inquiry) => SendAsync(
        inquiry.Email, "Orisia — reply to your inquiry",
        $"<h2>{WebUtility.HtmlEncode(inquiry.Subject)}</h2><p>{WebUtility.HtmlEncode(inquiry.AnswerText).Replace("\n", "<br>")}</p>");

    private async Task SendAsync(string to, string subject, string html)
    {
        var settings = options.Value;
        if (settings.DeliveryMode.Equals("Console", StringComparison.OrdinalIgnoreCase) && environment.IsDevelopment())
        {
            logger.LogInformation("Development email to {Recipient}, subject {Subject}.", to, subject);
            return;
        }
        if (!settings.DeliveryMode.Equals("Resend", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(settings.ResendApiKey))
            throw new AppException("Email delivery is not configured.").SetStatusCode(503);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ResendApiKey);
        request.Content = JsonContent.Create(new { from = $"{settings.SenderName} <{settings.SenderEmail}>", to = new[] { to }, subject, html });
        using var response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Email provider rejected delivery with HTTP {StatusCode}.", (int)response.StatusCode);
            throw new AppException("Email delivery failed. Try again later.").SetStatusCode(502);
        }
    }
}
