using Microsoft.EntityFrameworkCore;
using Orisia.Server.Data;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Repositories;
using Xunit;

namespace Orisia.Server.Tests.Unit.Repositories;

public class GalleryRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly GalleryRepository _repository;

    public GalleryRepositoryTests()
    {
        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("GalleryRepositoryTests-" + Guid.NewGuid())
                .Options;

        _context = new ApplicationDbContext(options);
        _repository = new GalleryRepository(_context);
    }

    [Fact]
    public async Task GetAlbumsAsync_PublicOnly_ShouldReturnOnlyActiveAlbums()
    {
        _context.GalleryAlbums.AddRange(
            CreateAlbum("active", true),
            CreateAlbum("inactive", false));

        await _context.SaveChangesAsync();

        IEnumerable<GalleryAlbum> result = await _repository.GetAlbumsAsync(true);

        GalleryAlbum only = Assert.Single(result);
        Assert.Equal("active", only.Slug);
    }

    [Fact]
    public async Task ReorderAsync_ShouldUpdateSortOrder()
    {
        GalleryAlbum album = CreateAlbum("album", true);
        Media media = CreateMedia();

        GalleryMedia first = new()
        {
            GalleryAlbumId = album.Id,
            MediaId = media.Id,
            SortOrder = 0
        };

        GalleryMedia second = new()
        {
            GalleryAlbumId = album.Id,
            MediaId = Guid.NewGuid(),
            SortOrder = 1
        };

        Media media2 = CreateMedia(second.MediaId);

        _context.AddRange(album, media, media2, first, second);
        await _context.SaveChangesAsync();

        await _repository.ReorderAsync(album.Id, new Dictionary<Guid, int>
        {
            [first.Id] = 10,
            [second.Id] = 20
        });

        Assert.Equal(10, (await _context.GalleryMedia.FindAsync(first.Id))!.SortOrder);
        Assert.Equal(20, (await _context.GalleryMedia.FindAsync(second.Id))!.SortOrder);
    }

    private static GalleryAlbum CreateAlbum(string slug, bool active)
    {
        return new GalleryAlbum
        {
            Slug = slug,
            TitleBg = slug,
            TitleEn = slug,
            Active = active
        };
    }

    private static Media CreateMedia(Guid? id = null)
    {
        return new Media
        {
            Id = id ?? Guid.NewGuid(),
            OriginalFileName = "photo.jpg",
            StorageKey = Guid.NewGuid().ToString("N") + ".jpg",
            MimeType = "image/jpeg",
            Extension = ".jpg",
            Sha256 = new string('a', 64)
        };
    }
}
