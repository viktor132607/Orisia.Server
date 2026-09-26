using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Orisia.Server.API.Services;
using Orisia.Server.Common.Options;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public sealed class EmailNotificationServiceTests
{
    private static User User() => new() { Email = "user@example.test", Names = "User", Phone = "", PasswordHash = "test" };

    [Fact]
    public async Task ResendUsesBearerKeyAndEscapesHtml()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var service = Create(handler, "Resend", "Production");
        await service.SendPasswordResetAsync(User(), "https://example.test/reset?token=a&x=b");
        Assert.Equal("https://api.resend.com/emails", handler.Url);
        Assert.Equal("Bearer test-key", handler.Authorization);
        Assert.Contains("user@example.test", handler.Body);
        Assert.Contains("amp;", handler.Body);
    }

    [Fact]
    public async Task ProductionConsoleModeCannotPretendToSend()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK);
        await Assert.ThrowsAsync<AppException>(() => Create(handler, "Console", "Production")
            .SendPasswordResetAsync(User(), "https://example.test/reset"));
        Assert.Null(handler.Url);
    }

    [Fact]
    public async Task FailedDeliveryIsReported()
    {
        await Assert.ThrowsAsync<AppException>(() => Create(new RecordingHandler(HttpStatusCode.BadRequest), "Resend", "Production")
            .SendPasswordResetAsync(User(), "https://example.test/reset"));
    }

    private static EmailNotificationService Create(RecordingHandler handler, string mode, string environmentName)
    {
        var env = new Mock<IWebHostEnvironment>();
        env.SetupGet(e => e.EnvironmentName).Returns(environmentName);
        return new EmailNotificationService(new HttpClient(handler), Options.Create(new EmailOptions {
            DeliveryMode = mode, ResendApiKey = "test-key", SenderEmail = "sender@example.test"
        }), env.Object, NullLogger<EmailNotificationService>.Instance);
    }

    private sealed class RecordingHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public string? Url { get; private set; }
        public string? Authorization { get; private set; }
        public string Body { get; private set; } = "";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Url = request.RequestUri!.ToString();
            Authorization = request.Headers.Authorization?.ToString();
            Body = await request.Content!.ReadAsStringAsync(ct);
            return new HttpResponseMessage(status);
        }
    }
}
