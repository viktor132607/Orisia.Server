using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.API.Services;

public static class AdminBootstrapper
{
    public static async Task SeedAsync(ApplicationDbContext db, string? email, string? password)
    {
        if (await db.Users.AnyAsync(u => !u.IsDeleted && u.IsActive && u.Role == Roles.Admin)) return;
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password)) return;
        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email.Trim()) || string.IsNullOrWhiteSpace(password) || password.Length < 16)
            throw new InvalidOperationException("Bootstrap admin requires a valid email and a password of at least 16 characters.");
        var normalized = email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == normalized))
            throw new InvalidOperationException("Bootstrap email already belongs to an account. Existing accounts are not promoted automatically.");
        var user = new User { Email = normalized, Names = "Orisia Administrator", Phone = "", PasswordHash = "", Role = Roles.Admin, IsActive = true };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
