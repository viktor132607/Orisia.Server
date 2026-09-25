using Microsoft.EntityFrameworkCore;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Repositories;
using Xunit;

namespace Orisia.Server.Tests.Unit.Repositories;

public class DanceRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly DanceRepository _repository;

    public DanceRepositoryTests()
    {
        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("DanceRepositoryTests-" + Guid.NewGuid())
                .Options;

        _context = new ApplicationDbContext(options);
        _repository = new DanceRepository(_context);
    }

    [Fact]
    public async Task GetPublicAsync_ShouldHideInactiveDeletedAndFilterRegion()
    {
        _context.Dances.AddRange(
            CreateDance("active", "Шопска", true),
            CreateDance("inactive", "Шопска", false),
            CreateDance("other", "Тракийска", true),
            CreateDance("deleted", "Шопска", true, true));

        await _context.SaveChangesAsync();

        IEnumerable<Dance> result = await _repository.GetPublicAsync("Шопска");

        Dance only = Assert.Single(result);
        Assert.Equal("active", only.Slug);
    }

    [Fact]
    public async Task ReorderAsync_ShouldUpdateSortOrder()
    {
        Dance first = CreateDance("first", null, true);
        Dance second = CreateDance("second", null, true);

        _context.Dances.AddRange(first, second);
        await _context.SaveChangesAsync();

        await _repository.ReorderAsync(new Dictionary<Guid, int>
        {
            [first.Id] = 10,
            [second.Id] = 20
        });

        Assert.Equal(10, (await _context.Dances.FindAsync(first.Id))!.SortOrder);
        Assert.Equal(20, (await _context.Dances.FindAsync(second.Id))!.SortOrder);
    }

    private static Dance CreateDance(
        string slug,
        string? region,
        bool active,
        bool deleted = false)
    {
        return new Dance
        {
            Slug = slug,
            TitleBg = slug,
            TitleEn = slug,
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            Region = region,
            Active = active,
            IsDeleted = deleted
        };
    }
}
