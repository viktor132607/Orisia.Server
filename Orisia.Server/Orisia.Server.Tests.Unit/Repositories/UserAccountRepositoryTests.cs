using Microsoft.EntityFrameworkCore;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Repositories;
using Xunit;

namespace Orisia.Server.Tests.Unit.Repositories;

public class UserAccountRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserAccountRepositoryTests()
    {
        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("UserAccountRepositoryTests-" + Guid.NewGuid())
                .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task IsEmailAlreadyUsed_ShouldNormalizeEmail()
    {
        _context.Users.Add(CreateUser("user@example.com"));
        await _context.SaveChangesAsync();

        Assert.True(await _repository.IsEmailAlreadyUsed(" USER@EXAMPLE.COM "));
    }

    [Fact]
    public async Task IsEmailAlreadyUsedByOtherUser_ShouldExcludeCurrentUser()
    {
        User user = CreateUser("user@example.com");
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        Assert.False(await _repository.IsEmailAlreadyUsedByOtherUser(
            "USER@EXAMPLE.COM",
            user.Id));
    }

    private static User CreateUser(string email)
    {
        return new User
        {
            Email = email,
            Names = "User",
            Phone = "123",
            PasswordHash = "hash"
        };
    }
}
