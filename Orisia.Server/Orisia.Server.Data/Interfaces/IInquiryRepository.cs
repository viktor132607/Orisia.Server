using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Interfaces;

public interface IInquiryRepository : IRepository<ContactInquiry>
{
    Task<IEnumerable<ContactInquiry>> GetForAdminAsync(InquiryStatus? status = null);
    Task<ContactInquiry?> GetWithAnswererAsync(Guid id);
}
