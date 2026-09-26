using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Seed;

/// <summary>Explicitly opt-in demo content; never overwrites existing content.</summary>
public static class CmsDemoSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (!await db.Posts.AnyAsync(p => p.Slug == "demo-orisia"))
            db.Posts.Add(new Post {
                Slug = "demo-orisia", TitleBg = "Демо публикация", TitleEn = "Demo post",
                BodyBg = "Примерно съдържание за тестова среда.", BodyEn = "Sample content for a test environment.",
                Status = PublicationStatus.Published, PublishedAt = DateTime.UtcNow });
        if (!await db.Events.AnyAsync(e => e.Slug == "demo-rehearsal"))
            db.Events.Add(new Event {
                Slug = "demo-rehearsal", TitleBg = "Демо репетиция", TitleEn = "Demo rehearsal",
                DescriptionBg = "Примерно събитие.", DescriptionEn = "Sample event.",
                StartAt = DateTime.UtcNow.Date.AddDays(7).AddHours(15),
                EndAt = DateTime.UtcNow.Date.AddDays(7).AddHours(17), Status = PublicationStatus.Published });
        if (!await db.Dances.AnyAsync(d => d.Slug == "demo-pravo-horo"))
            db.Dances.Add(new Dance {
                Slug = "demo-pravo-horo", TitleBg = "Демо право хоро", TitleEn = "Demo Pravo horo",
                DescriptionBg = "Примерен запис в хоротеката.", DescriptionEn = "Sample dance library entry.", Rhythm = "2/4" });
        if (!await db.GalleryAlbums.AnyAsync(a => a.Slug == "demo-gallery"))
            db.GalleryAlbums.Add(new GalleryAlbum {
                Slug = "demo-gallery", TitleBg = "Демо галерия", TitleEn = "Demo gallery", Active = true });
        await db.SaveChangesAsync();
    }
}
