using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Seed;

public static class CategorySeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        Category[] categories =
        [
            new()
            {
                Name = "Running",
                ImageUri = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1200&q=80",
            },
            new()
            {
                Name = "Football",
                ImageUri = "https://images.unsplash.com/photo-1574629810360-7efbbe195018?auto=format&fit=crop&w=1200&q=80",
            },
            new()
            {
                Name = "Fitness & Training",
                ImageUri = "https://images.unsplash.com/photo-1517836357463-d25dfeac3438?auto=format&fit=crop&w=1200&q=80",
            },
            new()
            {
                Name = "Outdoor",
                ImageUri = "https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=1200&q=80",
            },
            new()
            {
                Name = "Cycling",
                ImageUri = "https://images.unsplash.com/photo-1541625602330-2277a4c46182?auto=format&fit=crop&w=1200&q=80",
            },
            new()
            {
                Name = "Tennis",
                ImageUri = "https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?auto=format&fit=crop&w=1200&q=80",
            },
            new()
            {
                Name = "Basketball",
                ImageUri = "https://images.unsplash.com/photo-1546519638-68e109498ffc?auto=format&fit=crop&w=1200&q=80",
            },
            new()
            {
                Name = "Accessories",
                ImageUri = "https://images.unsplash.com/photo-1517837016564-bfc5f76c00a3?auto=format&fit=crop&w=1200&q=80",
            },
        ];

        HashSet<string> existingCategoryNames = db.Categories
            .Where(category => !string.IsNullOrWhiteSpace(category.Name))
            .Select(category => category.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Category[] categoriesToAdd = categories
            .Where(category => !existingCategoryNames.Contains(category.Name))
            .ToArray();

        if (categoriesToAdd.Length == 0)
        {
            return;
        }

        db.Categories.AddRange(categoriesToAdd);
        await db.SaveChangesAsync();
    }
}
