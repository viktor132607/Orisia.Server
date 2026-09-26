using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Orisia.Server.API.Services;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public sealed class AdminBootstrapperTests
{
    private static ApplicationDbContext CreateDb() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task CreatesOneAdminAndDoesNotResetExistingPassword()
    {
        await using var db = CreateDb();
        await AdminBootstrapper.SeedAsync(db, " ADMIN@example.test ", "StrongBootstrapPassword123!");
        await AdminBootstrapper.SeedAsync(db, "other@example.test", "DifferentPassword123!");
        var user = Assert.Single(await db.Users.ToListAsync());
        Assert.Equal("admin@example.test", user.Email);
        Assert.Equal("Admin", user.Role);
        Assert.Equal(PasswordVerificationResult.Success, new PasswordHasher<User>()
            .VerifyHashedPassword(user, user.PasswordHash, "StrongBootstrapPassword123!"));
    }

    [Fact]
    public async Task DoesNotSeedWithoutExplicitConfiguration()
    {
        await using var db = CreateDb();
        await AdminBootstrapper.SeedAsync(db, null, null);
        Assert.Empty(await db.Users.ToListAsync());
    }

    [Theory]
    [InlineData("bad-email", "StrongBootstrapPassword123!")]
    [InlineData("admin@example.test", "short")]
    [InlineData("admin@example.test", null)]
    public async Task RejectsInvalidConfiguration(string email, string? password)
    {
        await using var db = CreateDb();
        await Assert.ThrowsAsync<InvalidOperationException>(() => AdminBootstrapper.SeedAsync(db, email, password));
        Assert.Empty(await db.Users.ToListAsync());
    }

    [Fact]
    public async Task DoesNotPromoteAnExistingUser()
    {
        await using var db = CreateDb();
        db.Users.Add(new User { Email = "user@example.test", Names = "User", Phone = "", PasswordHash = "test", Role = "User" });
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => AdminBootstrapper.SeedAsync(db, "user@example.test", "StrongBootstrapPassword123!"));
        Assert.Equal("User", (await db.Users.SingleAsync()).Role);
    }
}
