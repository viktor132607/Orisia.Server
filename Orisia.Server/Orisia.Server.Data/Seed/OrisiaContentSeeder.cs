using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Seed;

/// <summary>Initial public content from verified Orisia announcements; never overwrites editorial changes.</summary>
public static class OrisiaContentSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        // https://www.facebook.com/orisiyaruse/posts/pfbid02P1R7armQVGgFez8hJgCQMu8pDR5idkfwFG2rZEKt2njHeVTsFB1DBT7tXoPwkvcil
        // https://obshtinaruse.bg/sandrovo-posreshta-xiv-izdanie-na-folklornia-festival-sandrovo-pee-i-tantsuva
        Post[] posts =
        [
            new()
            {
                Slug = "nova-grupa-nachinaeshti-oktomvri-2026",
                Type = PostType.News,
                TitleBg = "Нова група за начинаещи от 12 октомври",
                TitleEn = "New beginners' group starting 12 October",
                ExcerptBg = "Народни танци за начинаещи в Русе — понеделник и сряда от 19:40 ч.",
                ExcerptEn = "Beginner folk dances in Ruse — Mondays and Wednesdays at 19:40.",
                BodyBg = "Даскало за фолклор „Орисия“ открива нова група за начинаещи на 12 октомври 2026 г. Занятията са в понеделник и сряда от 19:40 ч. в залата на бул. Родина 80, на гърба на боулинг залата в Русе. За записване и допълнителна информация се свържете с нас чрез страницата ни във Facebook.",
                BodyEn = "ORISIA Folklore School is opening a new beginners' group on 12 October 2026. Classes are on Mondays and Wednesdays at 19:40 at 80 Rodina Boulevard, behind the bowling hall in Ruse. Contact us through our Facebook page for registration and more information.",
                Status = PublicationStatus.Published,
                Featured = true,
                PublishedAt = DateTime.UtcNow
            },
            new()
            {
                Slug = "orisiya-sandrovo-pee-i-tancuva-2026",
                Type = PostType.News,
                TitleBg = "„Орисия“ на фестивала „Сандрово пее и танцува“",
                TitleEn = "ORISIA at the Sandrovo Sings and Dances festival",
                ExcerptBg = "Школата участва с демонстрации в XIV издание на фолклорния фестивал в Сандрово.",
                ExcerptEn = "The school took part in the 14th folklore festival in Sandrovo with dance demonstrations.",
                BodyBg = "На 4 юли 2026 г. в село Сандрово се проведе XIV фолклорен фестивал „Сандрово пее и танцува“. Във вечерната програма Даскало за фолклор „Орисия“ — Русе представи демонстрации на български народни танци.",
                BodyEn = "The 14th Sandrovo Sings and Dances folklore festival took place on 4 July 2026. ORISIA Folklore School from Ruse presented Bulgarian folk dance demonstrations during the evening programme.",
                Status = PublicationStatus.Published,
                PublishedAt = DateTime.UtcNow
            }
        ];

        foreach (Post post in posts)
        {
            if (!await db.Posts.AnyAsync(item => item.Slug == post.Slug && !item.IsDeleted, cancellationToken))
                db.Posts.Add(post);
        }

        Event[] events =
        [
            new()
            {
                Slug = "nachalo-na-grupa-za-nachinaeshti-2026",
                TitleBg = "Начало на новата група за начинаещи",
                TitleEn = "New beginners' group begins",
                DescriptionBg = "Първо занятие на новата група на „Орисия“. Редовните занятия са всеки понеделник и сряда от 19:40 ч. За записване вижте страницата ни във Facebook.",
                DescriptionEn = "The first class of ORISIA's new beginners' group. Regular classes are every Monday and Wednesday at 19:40. Visit our Facebook page to register.",
                // 19:40 in Ruse on 12 October is 16:40 UTC (summer time).
                StartAt = new DateTime(2026, 10, 12, 16, 40, 0, DateTimeKind.Utc),
                EventType = EventType.Rehearsal,
                Location = "гр. Русе, бул. Родина 80 (на гърба на боулинг залата)",
                Featured = true,
                Status = PublicationStatus.Published
            },
            new()
            {
                Slug = "sandrovo-pee-i-tancuva-2026",
                TitleBg = "Фестивал „Сандрово пее и танцува“",
                TitleEn = "Sandrovo Sings and Dances festival",
                DescriptionBg = "XIV издание на фолклорния фестивал в Сандрово с демонстрации на Даскало за фолклор „Орисия“ — Русе във вечерната програма.",
                DescriptionEn = "The 14th folklore festival in Sandrovo, with dance demonstrations by ORISIA Folklore School from Ruse in the evening programme.",
                StartAt = new DateTime(2026, 7, 4, 0, 0, 0, DateTimeKind.Utc),
                AllDay = true,
                EventType = EventType.Festival,
                Location = "с. Сандрово, община Русе",
                Status = PublicationStatus.Published
            }
        ];

        foreach (Event item in events)
        {
            if (!await db.Events.AnyAsync(existing => existing.Slug == item.Slug && !existing.IsDeleted, cancellationToken))
                db.Events.Add(item);
        }

        GalleryAlbum[] albums =
        [
            new()
            {
                Slug = "orisiya-na-horoto",
                TitleBg = "На хорото с „Орисия“",
                TitleEn = "Dancing with ORISIA",
                DescriptionBg = "Място за снимки от заниманията на Даскало за фолклор „Орисия“. Снимките ще бъдат добавени след качване в галерията.",
                DescriptionEn = "A space for photos from ORISIA Folklore School's classes. Photos will appear once uploaded to the gallery.",
                Active = true,
                Featured = true,
                SortOrder = 0
            },
            new()
            {
                Slug = "sandrovo-pee-i-tancuva-2026",
                TitleBg = "„Сандрово пее и танцува“ 2026",
                TitleEn = "Sandrovo Sings and Dances 2026",
                DescriptionBg = "Място за снимки от участието на „Орисия“ на фестивала на 4 юли 2026 г. Снимките ще бъдат добавени след качване в галерията.",
                DescriptionEn = "A space for photos from ORISIA's appearance at the festival on 4 July 2026. Photos will appear once uploaded to the gallery.",
                Active = true,
                SortOrder = 1
            }
        ];

        foreach (GalleryAlbum album in albums)
        {
            if (!await db.GalleryAlbums.AnyAsync(existing => existing.Slug == album.Slug && !existing.IsDeleted, cancellationToken))
                db.GalleryAlbums.Add(album);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
