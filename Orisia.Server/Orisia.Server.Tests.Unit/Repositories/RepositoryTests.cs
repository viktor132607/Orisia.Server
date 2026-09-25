using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Pages;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.PaginationAndFiltering;
using Orisia.Server.Data.Repositories;
using Xunit;

namespace Orisia.Server.Tests.Unit.Repositories;

public class RepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly Repository<User> _repository;

    public RepositoryTests()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "RepositoryTests-" + Guid.NewGuid())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new Repository<User>(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        User user = CreateUser("one@example.com");
        User? result = await _repository.AddAsync(user);

        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.False(result.IsDeleted);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeSoftDeletedEntities()
    {
        await _repository.AddAsync(CreateUser("active@example.com"));
        await _repository.AddAsync(CreateUser("deleted@example.com", true));

        IEnumerable<User> result = await _repository.GetAllAsync();

        Assert.Single(result);
        Assert.Equal("active@example.com", result.Single().Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForSoftDeletedEntity()
    {
        User user = CreateUser("deleted@example.com", true);
        await _repository.AddAsync(user);

        Assert.Null(await _repository.GetByIdAsync(user.Id));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteEntity()
    {
        User user = CreateUser("delete@example.com");
        await _repository.AddAsync(user);

        Assert.True(await _repository.DeleteAsync(user.Id));
        Assert.Null(await _repository.GetByIdAsync(user.Id));
        Assert.True((await _context.Users.FindAsync(user.Id))!.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        Assert.False(await _repository.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntityAndPreserveCreatedOn()
    {
        User user = CreateUser("before@example.com");
        await _repository.AddAsync(user);
        DateTime createdOn = user.CreatedOn;

        User update = new()
        {
            Id = user.Id,
            Email = "after@example.com",
            Names = "Updated",
            Phone = "123",
            PasswordHash = "hash"
        };

        User? result = await _repository.UpdateAsync(update);

        Assert.NotNull(result);
        Assert.Equal("after@example.com", result.Email);
        Assert.Equal(createdOn, result.CreatedOn);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        Assert.Null(await _repository.UpdateAsync(CreateUser("missing@example.com")));
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterSortAndPaginateWithoutDeletedRows()
    {
        await _repository.AddAsync(CreateUser("b@example.com"));
        await _repository.AddAsync(CreateUser("a@example.com"));
        await _repository.AddAsync(CreateUser("deleted@example.com", true));

        Filter<User> filter = new()
        {
            Predicate = user => user.Email.Contains("@example.com"),
            SortBy = nameof(User.Email),
            PageNumber = 1,
            PageSize = 1
        };

        Paginated<User> result = await _repository.SearchAsync(filter);

        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("a@example.com", result.Items.Single().Email);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnAllMatchingRows_WhenPaginationIsNotRequested()
    {
        await _repository.AddAsync(CreateUser("a@example.com"));
        await _repository.AddAsync(CreateUser("b@example.com"));

        Paginated<User> result = await _repository.SearchAsync(new Filter<User>
        {
            Predicate = user => user.Email.EndsWith("@example.com")
        });

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
    }

    [Fact]
    public async Task NullPayloads_ShouldReturnNull()
    {
        Assert.Null(await _repository.AddAsync(null));
        Assert.Null(await _repository.UpdateAsync(null));
    }

    private static User CreateUser(string email, bool isDeleted = false) => new()
    {
        Email = email,
        Names = "Test User",
        Phone = "0000000000",
        PasswordHash = "hash",
        IsDeleted = isDeleted
    };
}
