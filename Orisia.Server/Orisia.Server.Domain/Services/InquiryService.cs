using Orisia.Server.Common.Requests.Inquiries;
using Orisia.Server.Common.Responses.Inquiries;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class InquiryService(
    IInquiryRepository inquiryRepository,
    IAuthService authService,
    IEmailNotificationService emailNotificationService) : IInquiryService
{
    public async Task<ContactInquiryResponse> SubmitAsync(CreateInquiryRequest request)
    {
        ContactInquiry inquiry = new()
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = NormalizeOptional(request.Phone),
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Status = InquiryStatus.New
        };

        ContactInquiry? created = await inquiryRepository.AddAsync(inquiry);
        if (created is null)
        {
            throw new AppException("Inquiry could not be submitted.").SetStatusCode(500);
        }

        await emailNotificationService.SendContactInquiryReceivedAsync(created);

        return Map(created);
    }

    public async Task<IEnumerable<ContactInquiryResponse>> GetAdminAsync(
        InquiryStatus? status = null)
    {
        IEnumerable<ContactInquiry> inquiries =
            await inquiryRepository.GetForAdminAsync(status);

        return inquiries.Select(Map);
    }

    public async Task<ContactInquiryResponse> GetAdminByIdAsync(Guid id)
    {
        ContactInquiry? inquiry = await inquiryRepository.GetWithAnswererAsync(id);
        if (inquiry is null)
        {
            throw new AppException("Inquiry not found.").SetStatusCode(404);
        }

        return Map(inquiry);
    }

    public async Task<ContactInquiryResponse> MarkReadAsync(Guid id)
    {
        ContactInquiry existing = await RequireAsync(id);

        if (existing.Status == InquiryStatus.Archived)
        {
            throw new AppException("Archived inquiries cannot be marked as read.")
                .SetStatusCode(409);
        }

        if (existing.Status == InquiryStatus.Answered)
        {
            return await GetAdminByIdAsync(id);
        }

        ContactInquiry updated = Clone(existing);
        updated.Status = InquiryStatus.Read;
        updated.ReadAt ??= DateTime.UtcNow;

        _ = await inquiryRepository.UpdateAsync(updated)
            ?? throw new AppException("Inquiry not found.").SetStatusCode(404);

        return await GetAdminByIdAsync(id);
    }

    public async Task<ContactInquiryResponse> AnswerAsync(
        Guid id,
        AnswerInquiryRequest request)
    {
        ContactInquiry existing = await RequireAsync(id);

        if (existing.Status == InquiryStatus.Archived)
        {
            throw new AppException("Archived inquiries cannot be answered.")
                .SetStatusCode(409);
        }

        Guid answeredById = await GetRequiredCurrentUserIdAsync();

        ContactInquiry updated = Clone(existing);
        updated.Status = InquiryStatus.Answered;
        updated.ReadAt ??= DateTime.UtcNow;
        updated.AnswerText = request.Answer.Trim();
        updated.AnsweredAt = DateTime.UtcNow;
        updated.AnsweredById = answeredById;
        updated.ArchivedAt = null;

        ContactInquiry? saved = await inquiryRepository.UpdateAsync(updated);
        if (saved is null)
        {
            throw new AppException("Inquiry not found.").SetStatusCode(404);
        }

        await emailNotificationService.SendContactInquiryAnswerAsync(saved);

        return await GetAdminByIdAsync(id);
    }

    public async Task<ContactInquiryResponse> ArchiveAsync(Guid id)
    {
        ContactInquiry existing = await RequireAsync(id);

        if (existing.Status == InquiryStatus.Archived)
        {
            return await GetAdminByIdAsync(id);
        }

        ContactInquiry updated = Clone(existing);
        updated.Status = InquiryStatus.Archived;
        updated.ArchivedAt = DateTime.UtcNow;

        _ = await inquiryRepository.UpdateAsync(updated)
            ?? throw new AppException("Inquiry not found.").SetStatusCode(404);

        return await GetAdminByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _ = await RequireAsync(id);
        return await inquiryRepository.DeleteAsync(id);
    }

    private async Task<ContactInquiry> RequireAsync(Guid id)
    {
        return await inquiryRepository.GetByIdAsync(id)
            ?? throw new AppException("Inquiry not found.").SetStatusCode(404);
    }

    private async Task<Guid> GetRequiredCurrentUserIdAsync()
    {
        string? raw = await authService.GetCurrentUserId();
        if (!Guid.TryParse(raw, out Guid userId))
        {
            throw new AppException("Unauthorized.").SetStatusCode(401);
        }

        return userId;
    }

    private static ContactInquiry Clone(ContactInquiry item)
    {
        return new ContactInquiry
        {
            Id = item.Id,
            Name = item.Name,
            Email = item.Email,
            Phone = item.Phone,
            Subject = item.Subject,
            Message = item.Message,
            Status = item.Status,
            ReadAt = item.ReadAt,
            AnswerText = item.AnswerText,
            AnsweredAt = item.AnsweredAt,
            AnsweredById = item.AnsweredById,
            ArchivedAt = item.ArchivedAt
        };
    }

    private static ContactInquiryResponse Map(ContactInquiry item)
    {
        return new ContactInquiryResponse
        {
            Id = item.Id,
            Name = item.Name,
            Email = item.Email,
            Phone = item.Phone,
            Subject = item.Subject,
            Message = item.Message,
            Status = item.Status,
            CreatedOn = item.CreatedOn,
            ModifiedOn = item.ModifiedOn,
            ReadAt = item.ReadAt,
            AnswerText = item.AnswerText,
            AnsweredAt = item.AnsweredAt,
            AnsweredById = item.AnsweredById,
            AnsweredByName = item.AnsweredBy?.Names,
            ArchivedAt = item.ArchivedAt
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
