using Moq;
using Orisia.Server.Common.Requests.Inquiries;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class InquiryServiceTests
{
    private readonly Mock<IInquiryRepository> _inquiries = new();
    private readonly Mock<IAuthService> _auth = new();
    private readonly Mock<IEmailNotificationService> _email = new();
    private readonly InquiryService _service;

    public InquiryServiceTests()
    {
        _service = new InquiryService(
            _inquiries.Object,
            _auth.Object,
            _email.Object);
    }

    [Fact]
    public async Task SubmitAsync_ShouldNormalizeDataAndSendNotification()
    {
        ContactInquiry? saved = null;

        _inquiries.Setup(x => x.AddAsync(It.IsAny<ContactInquiry>()))
            .ReturnsAsync((ContactInquiry inquiry) =>
            {
                saved = inquiry;
                return inquiry;
            });

        var result = await _service.SubmitAsync(new CreateInquiryRequest
        {
            Name = " Viktor ",
            Email = " VIKTOR@EXAMPLE.COM ",
            Phone = " +359 88 123 4567 ",
            Subject = " Запитване ",
            Message = " Това е достатъчно дълго съобщение. "
        });

        Assert.Equal(InquiryStatus.New, result.Status);
        Assert.Equal("viktor@example.com", result.Email);
        Assert.Equal("+359 88 123 4567", result.Phone);
        Assert.NotNull(saved);

        _email.Verify(
            x => x.SendContactInquiryReceivedAsync(saved!),
            Times.Once);
    }

    [Fact]
    public async Task MarkReadAsync_ShouldSetStatusAndTimestamp()
    {
        Guid id = Guid.NewGuid();
        ContactInquiry inquiry = CreateInquiry(id, InquiryStatus.New);

        _inquiries.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(inquiry);
        _inquiries.Setup(x => x.UpdateAsync(It.IsAny<ContactInquiry>()))
            .ReturnsAsync((ContactInquiry item) =>
            {
                inquiry.Status = item.Status;
                inquiry.ReadAt = item.ReadAt;
                return item;
            });
        _inquiries.Setup(x => x.GetWithAnswererAsync(id))
            .ReturnsAsync(() => inquiry);

        var result = await _service.MarkReadAsync(id);

        Assert.Equal(InquiryStatus.Read, result.Status);
        Assert.NotNull(result.ReadAt);
    }

    [Fact]
    public async Task AnswerAsync_ShouldSetAnswerMetadataAndNotifySender()
    {
        Guid id = Guid.NewGuid();
        Guid adminId = Guid.NewGuid();
        ContactInquiry inquiry = CreateInquiry(id, InquiryStatus.Read);

        _auth.Setup(x => x.GetCurrentUserId()).ReturnsAsync(adminId.ToString());
        _inquiries.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(inquiry);
        _inquiries.Setup(x => x.UpdateAsync(It.IsAny<ContactInquiry>()))
            .ReturnsAsync((ContactInquiry item) =>
            {
                inquiry.Status = item.Status;
                inquiry.AnswerText = item.AnswerText;
                inquiry.AnsweredAt = item.AnsweredAt;
                inquiry.AnsweredById = item.AnsweredById;
                inquiry.ReadAt = item.ReadAt;
                return item;
            });
        _inquiries.Setup(x => x.GetWithAnswererAsync(id))
            .ReturnsAsync(() => inquiry);

        var result = await _service.AnswerAsync(id, new AnswerInquiryRequest
        {
            Answer = " Благодарим за запитването. "
        });

        Assert.Equal(InquiryStatus.Answered, result.Status);
        Assert.Equal("Благодарим за запитването.", result.AnswerText);
        Assert.Equal(adminId, result.AnsweredById);
        Assert.NotNull(result.AnsweredAt);

        _email.Verify(
            x => x.SendContactInquiryAnswerAsync(It.Is<ContactInquiry>(
                item => item.Id == id && item.Status == InquiryStatus.Answered)),
            Times.Once);
    }

    [Fact]
    public async Task AnswerAsync_ShouldRejectArchivedInquiry()
    {
        Guid id = Guid.NewGuid();

        _inquiries.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(CreateInquiry(id, InquiryStatus.Archived));

        await Assert.ThrowsAsync<AppException>(() =>
            _service.AnswerAsync(id, new AnswerInquiryRequest
            {
                Answer = "Answer"
            }));
    }

    private static ContactInquiry CreateInquiry(Guid id, InquiryStatus status)
    {
        return new ContactInquiry
        {
            Id = id,
            Name = "Viktor",
            Email = "viktor@example.com",
            Subject = "Subject",
            Message = "Message content",
            Status = status
        };
    }
}
