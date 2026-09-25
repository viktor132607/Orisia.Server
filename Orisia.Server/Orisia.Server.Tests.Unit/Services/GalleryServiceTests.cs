using Moq;
using Orisia.Server.Common.Requests.Gallery;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class GalleryServiceTests
{
    private readonly Mock<IGalleryRepository> _gallery = new();
    private readonly Mock<IMediaRepository> _media = new();
    private readonly Mock<IMediaStorage> _storage = new();
    private readonly GalleryService _service;

    public GalleryServiceTests()
    {
        _storage.Setup(x => x.GetPublicUrl(It.IsAny<string>()))
            .Returns((string key) => "/uploads/media/" + key);

        _service = new GalleryService(
            _gallery.Object,
            _media.Object,
            _storage.Object);
    }

    [Fact]
    public async Task CreateAlbumAsync_ShouldGenerateSlugAndValidateCover()
    {
        Guid mediaId = Guid.NewGuid();

        _gallery.Setup(x => x.SlugExistsAsync("folkloren-sabor", null))
            .ReturnsAsync(false);

        _media.Setup(x => x.GetWithUploaderAsync(mediaId))
            .ReturnsAsync(CreateMedia(mediaId));

        GalleryAlbum? saved = null;
        _gallery.Setup(x => x.AddAlbumAsync(It.IsAny<GalleryAlbum>()))
            .ReturnsAsync((GalleryAlbum album) =>
            {
                saved = album;
                return album;
            });

        _gallery.Setup(x => x.GetAlbumByIdAsync(It.IsAny<Guid>(), false))
            .ReturnsAsync(() => saved);

        var result = await _service.CreateAlbumAsync(new CreateGalleryAlbumRequest
        {
            TitleBg = "Фолклорен събор",
            TitleEn = "",
            CoverMediaId = mediaId,
            Featured = true
        });

        Assert.Equal("folkloren-sabor", result.Slug);
        Assert.Equal(mediaId, result.CoverMediaId);
        Assert.True(result.Featured);
    }

    [Fact]
    public async Task AddMediaAsync_ShouldRejectDuplicateMediaInsideAlbum()
    {
        Guid albumId = Guid.NewGuid();
        Guid mediaId = Guid.NewGuid();

        _gallery.Setup(x => x.GetAlbumByIdAsync(albumId, false))
            .ReturnsAsync(CreateAlbum(albumId));
        _media.Setup(x => x.GetWithUploaderAsync(mediaId))
            .ReturnsAsync(CreateMedia(mediaId));
        _gallery.Setup(x => x.MediaLinkExistsAsync(albumId, mediaId))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<AppException>(() =>
            _service.AddMediaAsync(albumId, new AddGalleryMediaRequest
            {
                Items =
                [
                    new GalleryMediaInput { MediaId = mediaId }
                ]
            }));
    }

    [Fact]
    public async Task MoveMediaAsync_ShouldRejectDuplicateInTargetAlbum()
    {
        Guid linkId = Guid.NewGuid();
        Guid sourceAlbumId = Guid.NewGuid();
        Guid targetAlbumId = Guid.NewGuid();
        Guid mediaId = Guid.NewGuid();

        _gallery.Setup(x => x.GetGalleryMediaAsync(linkId, false))
            .ReturnsAsync(new GalleryMedia
            {
                Id = linkId,
                GalleryAlbumId = sourceAlbumId,
                MediaId = mediaId,
                Media = CreateMedia(mediaId)
            });

        _gallery.Setup(x => x.GetAlbumByIdAsync(targetAlbumId, false))
            .ReturnsAsync(CreateAlbum(targetAlbumId));

        _gallery.Setup(x => x.MediaLinkExistsAsync(targetAlbumId, mediaId))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<AppException>(() =>
            _service.MoveMediaAsync(linkId, new MoveGalleryMediaRequest
            {
                TargetAlbumId = targetAlbumId
            }));
    }

    [Fact]
    public async Task GetPublicAlbumBySlugAsync_ShouldHideInactiveItems()
    {
        Guid albumId = Guid.NewGuid();

        GalleryAlbum album = CreateAlbum(albumId);
        album.Items =
        [
            new GalleryMedia
            {
                GalleryAlbumId = albumId,
                MediaId = Guid.NewGuid(),
                Active = true,
                SortOrder = 0,
                Media = CreateMedia(Guid.NewGuid())
            },
            new GalleryMedia
            {
                GalleryAlbumId = albumId,
                MediaId = Guid.NewGuid(),
                Active = false,
                SortOrder = 1,
                Media = CreateMedia(Guid.NewGuid())
            }
        ];

        _gallery.Setup(x => x.GetAlbumBySlugAsync("album", true))
            .ReturnsAsync(album);

        var result = await _service.GetPublicAlbumBySlugAsync("album");

        Assert.Single(result.Items);
    }

    private static GalleryAlbum CreateAlbum(Guid id)
    {
        return new GalleryAlbum
        {
            Id = id,
            Slug = "album",
            TitleBg = "Албум",
            TitleEn = "Album",
            Active = true
        };
    }

    private static Media CreateMedia(Guid id)
    {
        return new Media
        {
            Id = id,
            OriginalFileName = "photo.jpg",
            StorageKey = $"{id:N}.jpg",
            ThumbnailStorageKey = $"{id:N}.webp",
            MimeType = "image/jpeg",
            Extension = ".jpg",
            Sha256 = new string('a', 64)
        };
    }
}
