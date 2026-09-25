using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;

namespace Orisia.Server.Data.Repositories;

public class InquiryRepository(ApplicationDbContext context)
    : Repository<ContactInquiry>(context), IInquiryRepository
{
    public async Task<IEnumerable<ContactInquiry>> GetForAdminAsync(
        InquiryStatus? status = null)
    {
        IQueryable<ContactInquiry> query = Context.ContactInquiries
            .AsNoTracking()
            .Include(item => item.AnsweredBy)
            .Where(item => !item.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(item => item.Status == status.Value);
        }

        return await query
            .OrderBy(item => item.Status == InquiryStatus.Archived)
            .ThenByDescending(item => item.CreatedOn)
            .ToListAsync();
    }

    public async Task<ContactInquiry?> GetWithAnswererAsync(Guid id)
    {
        return await Context.ContactInquiries
            .AsNoTracking()
            .Include(item => item.AnsweredBy)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
    }
}
