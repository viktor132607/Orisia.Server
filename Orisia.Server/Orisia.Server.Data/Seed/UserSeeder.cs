using Microsoft.AspNetCore.Identity;
using Orisia.Server.Core.StaticClasses;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Seed;

public static class UserSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        PasswordHasher<User> hasher = new();

        List<User> users =
        [
            CreateUser("admin@orisia.bg", "Elena Petrova", "0888123400", Roles.Admin, "Admin123!", hasher),
            CreateUser("editor@orisia.bg", "Orisia Editor", "0888123401", Roles.Editor, "Editor123!", hasher),
            CreateUser("martin.georgiev@orisia.bg", "Martin Georgiev", "0888123402", Roles.User, "User01!", hasher),
            CreateUser("maria.dimitrova@orisia.bg", "Maria Dimitrova", "0888123403", Roles.User, "User02!", hasher)
        ];

        HashSet<string> existingEmails = db.Users
            .Where(user => !string.IsNullOrWhiteSpace(user.Email))
            .Select(user => user.Email)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        List<User> usersToAdd = users
            .Where(user => !existingEmails.Contains(user.Email))
            .ToList();

        if (usersToAdd.Count == 0)
        {
            return;
        }

        await db.Users.AddRangeAsync(usersToAdd);
        await db.SaveChangesAsync();
    }

    private static User CreateUser(
        string email,
        string names,
        string phone,
        string role,
        string password,
        PasswordHasher<User> hasher)
    {
        User user = new()
        {
            Email = email,
            PasswordHash = "temporaryPasswordHash",
            Names = names,
            Phone = phone,
            Role = role
        };

        user.PasswordHash = hasher.HashPassword(user, password);
        return user;
    }
}
