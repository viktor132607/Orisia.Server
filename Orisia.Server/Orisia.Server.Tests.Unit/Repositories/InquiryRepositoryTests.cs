using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Repositories;
using Xunit;

namespace Orisia.Server.Tests.Unit.Repositories;

public class InquiryRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly InquiryRepository _repository;

    public InquiryRepositoryTests()
    {
        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("InquiryRepositoryTests-" + Guid.NewGuid())
                .Options;

        _context = new ApplicationDbContext(options);
        _repository = new InquiryRepository(_context);
    }

    [Fact]
    public async Task GetForAdminAsync_ShouldFilterStatusAndIgnoreDeleted()
    {
        _context.ContactInquiries.AddRange(
            CreateInquiry("new", InquiryStatus.New),
            CreateInquiry("read", InquiryStatus.Read),
            CreateInquiry("deleted", InquiryStatus.New, true));

        await _context.SaveChangesAsync();

        IEnumerable<ContactInquiry> result =
            await _repository.GetForAdminAsync(InquiryStatus.New);

        ContactInquiry only = Assert.Single(result);
        Assert.Equal("new", only.Subject);
    }

    [Fact]
    public async Task GetForAdminAsync_ShouldPlaceArchivedAfterActive()
    {
        _context.ContactInquiries.AddRange(
            CreateInquiry("archived", InquiryStatus.Archived),
            CreateInquiry("active", InquiryStatus.New));

        await _context.SaveChangesAsync();

        List<ContactInquiry> result =
            (await _repository.GetForAdminAsync()).ToList();

        Assert.Equal("active", result[0].Subject);
        Assert.Equal("archived", result[1].Subject);
    }

    private static ContactInquiry CreateInquiry(
        string subject,
        InquiryStatus status,
        bool deleted = false)
    {
        return new ContactInquiry
        {
            Name = "Name",
            Email = "mail@example.com",
            Subject = subject,
            Message = "Message",
            Status = status,
            IsDeleted = deleted
        };
    }
}
