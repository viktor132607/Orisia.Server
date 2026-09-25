using Moq;
using Orisia.Server.Common.Requests.Horoteka;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class DanceServiceTests
{
    private readonly Mock<IDanceRepository> _dances = new();
    private readonly Mock<IMediaRepository> _media = new();
    private readonly Mock<IMediaStorage> _storage = new();
    private readonly DanceService _service;

    public DanceServiceTests()
    {
        _storage
            .Setup(x => x.GetPublicUrl(It.IsAny<string>()))
            .Returns((string key) => "/uploads/media/" + key);

        _service = new DanceService(
            _dances.Object,
            _media.Object,
            _storage.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldGenerateBulgarianSlugAndValidateThumbnail()
    {
        Guid thumbnailId = Guid.NewGuid();

        _dances.Setup(x => x.SlugExistsAsync("pravo-horo", null))
            .ReturnsAsync(false);

        _media.Setup(x => x.GetWithUploaderAsync(thumbnailId))
            .ReturnsAsync(CreateMedia(thumbnailId));

        Dance? saved = null;
        _dances.Setup(x => x.AddAsync(It.IsAny<Dance>()))
            .ReturnsAsync((Dance dance) =>
            {
                saved = dance;
                return dance;
            });

        _dances.Setup(x => x.GetWithThumbnailAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() =>
            {
                if (saved is not null)
                {
                    saved.ThumbnailMedia = CreateMedia(thumbnailId);
                }

                return saved;
            });

        var result = await _service.CreateAsync(new CreateDanceRequest
        {
            TitleBg = "Право хоро",
            TitleEn = "",
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            Region = "Шопска",
            Rhythm = "2/4",
            ThumbnailMediaId = thumbnailId,
            DurationSeconds = 180
        });

        Assert.Equal("pravo-horo", result.Slug);
        Assert.Equal("Шопска", result.Region);
        Assert.Equal(180, result.DurationSeconds);
        Assert.NotNull(result.ThumbnailUrl);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRejectDuplicateSlug()
    {
        Guid id = Guid.NewGuid();

        _dances.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(CreateDance(id));

        _dances.Setup(x => x.SlugExistsAsync("duplicate", id))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<AppException>(() =>
            _service.UpdateAsync(id, new UpdateDanceRequest
            {
                Slug = "duplicate",
                TitleBg = "Хоро",
                TitleEn = "Dance",
                DescriptionBg = "Описание",
                DescriptionEn = "Description"
            }));
    }

    [Fact]
    public async Task ReorderAsync_ShouldRejectDuplicateIds()
    {
        Guid id = Guid.NewGuid();

        await Assert.ThrowsAsync<AppException>(() =>
            _service.ReorderAsync(new ReorderDancesRequest
            {
                Items =
                [
                    new DanceOrderItem { DanceId = id, SortOrder = 0 },
                    new DanceOrderItem { DanceId = id, SortOrder = 1 }
                ]
            }));
    }

    private static Dance CreateDance(Guid id)
    {
        return new Dance
        {
            Id = id,
            Slug = "dance",
            TitleBg = "Хоро",
            TitleEn = "Dance",
            DescriptionBg = "Описание",
            DescriptionEn = "Description",
            Active = true
        };
    }

    private static Media CreateMedia(Guid id)
    {
        return new Media
        {
            Id = id,
            OriginalFileName = "thumb.jpg",
            StorageKey = $"{id:N}.jpg",
            ThumbnailStorageKey = $"{id:N}.webp",
            MimeType = "image/jpeg",
            Extension = ".jpg",
            Sha256 = new string('a', 64)
        };
    }
}
