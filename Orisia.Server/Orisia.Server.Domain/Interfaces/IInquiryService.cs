using Orisia.Server.Common.Requests.Inquiries;
using Orisia.Server.Common.Responses.Inquiries;
using Orisia.Server.Core.Enums;

namespace Orisia.Server.Domain.Interfaces;

public interface IInquiryService
{
    Task<ContactInquiryResponse> SubmitAsync(CreateInquiryRequest request);

    Task<IEnumerable<ContactInquiryResponse>> GetAdminAsync(
        InquiryStatus? status = null);

    Task<ContactInquiryResponse> GetAdminByIdAsync(Guid id);
    Task<ContactInquiryResponse> MarkReadAsync(Guid id);
    Task<ContactInquiryResponse> AnswerAsync(Guid id, AnswerInquiryRequest request);
    Task<ContactInquiryResponse> ArchiveAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
