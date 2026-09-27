using Microsoft.EntityFrameworkCore;
using Orisia.Server.Core.Enums;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Seed;

/// <summary>Initial public content from verified Orisia announcements; never overwrites editorial changes.</summary>
public static class OrisiaContentSeeder
{
    private const string DefaultContentImageUrl = "/events/za-galya.webp";

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
                MediaUrl = DefaultContentImageUrl,
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
                MediaUrl = DefaultContentImageUrl,
                PublishedAt = DateTime.UtcNow
            }
        ];

        foreach (Post post in posts)
        {
            if (!await db.Posts.AnyAsync(item => item.Slug == post.Slug && !item.IsDeleted, cancellationToken))
                db.Posts.Add(post);
        }

        List<Post> newsWithoutImage = await db.Posts
            .Where(item => item.Type == PostType.News && !item.IsDeleted && (item.MediaUrl == null || item.MediaUrl == ""))
            .ToListAsync(cancellationToken);

        foreach (Post post in newsWithoutImage)
            post.MediaUrl = DefaultContentImageUrl;

        Event[] events =
        [
            new()
            {
                Slug = "blagotvoritelen-koncert-za-galya-2026",
                TitleBg = "Благотворителен концерт „За Галя“",
                TitleEn = "Charity concert “For Galya”",
                DescriptionBg = "На 27 септември различни хора, таланти и светове се събират на една сцена с една обща цел — да бъдем до Галя.\n\nГаля е преподавател и творец, който е давал знание, музика и изкуство на другите. Сега е наш ред да я подкрепим.\n\nНа сцената: Михаел Лашев — авторска музика; Адриана Витанова; Веселина Няголова — български фолклор; „Сладките на Светлозара Савова“; Даскало за фолклор „Орисия“ — хоротека за малки и големи; аниматорска агенция „БАМ-БАМ“ — специална детска програма с игри и забавления.\n\nЕлате с децата, приятелите и близките си. Нека превърнем този неделен следобед в среща, която има значение.\n\nЕдна сцена. Много сърца. Една кауза — за Галя.",
                DescriptionEn = "On 27 September, different people, talents and worlds come together on one stage for one shared cause — to support Galya.\n\nGalya is a teacher and artist who has shared knowledge, music and creativity with others. Now it is our turn to stand by her.\n\nOn stage: Mihael Lashev with original music; Adriana Vitanova; Veselina Nyagolova with Bulgarian folklore; Sladkite na Svetlozara Savova; ORISIA Folklore School with an open horoteka for children and adults; and BAM-BAM animation agency with a special children’s programme of games and entertainment.\n\nCome with your children, friends and family. Let us turn this Sunday afternoon into a gathering that matters.\n\nOne stage. Many hearts. One cause — for Galya.",
                // 16:00 in Ruse on 27 September 2026 is 13:00 UTC (EEST).
                StartAt = new DateTime(2026, 9, 27, 13, 0, 0, DateTimeKind.Utc),
                EventType = EventType.Performance,
                Location = "Сцената на Ruse Stage, Русе",
                MediaType = EventMediaType.Image,
                MediaUrl = "/events/za-galya.webp",
                Featured = true,
                Status = PublicationStatus.Published
            },
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
                MediaType = EventMediaType.Image,
                MediaUrl = DefaultContentImageUrl,
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
                MediaType = EventMediaType.Image,
                MediaUrl = DefaultContentImageUrl,
                Status = PublicationStatus.Published
            }
        ];

        foreach (Event item in events)
        {
            if (!await db.Events.AnyAsync(existing => existing.Slug == item.Slug && !existing.IsDeleted, cancellationToken))
                db.Events.Add(item);
        }

        List<Event> eventsWithoutImage = await db.Events
            .Where(item => !item.IsDeleted && (item.MediaType == EventMediaType.None || (item.MediaType == EventMediaType.Image && (item.MediaUrl == null || item.MediaUrl == ""))))
            .ToListAsync(cancellationToken);

        foreach (Event item in eventsWithoutImage)
        {
            item.MediaType = EventMediaType.Image;
            item.MediaUrl = DefaultContentImageUrl;
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
