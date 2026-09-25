using Microsoft.Extensions.Options;
using Moq;
using Orisia.Server.Common.Options;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Media;
using Orisia.Server.Domain.Services;
using Xunit;

namespace Orisia.Server.Tests.Unit.Services;

public class MediaServiceTests
{
    private readonly Mock<IMediaRepository> _repository = new();
    private readonly Mock<IMediaStorage> _storage = new();
    private readonly Mock<IImageProcessor> _processor = new();
    private readonly Mock<IAuthService> _auth = new();
    private readonly MediaService _service;

    public MediaServiceTests()
    {
        _service = new MediaService(
            _repository.Object,
            _storage.Object,
            _processor.Object,
            _auth.Object,
            Options.Create(new MediaStorageOptions
            {
                MaxFileSizeBytes = 1024
            }));
    }

    [Fact]
    public async Task UploadAsync_ShouldStoreOriginalThumbnailAndMetadata()
    {
        byte[] bytes = [1, 2, 3, 4];
        Guid userId = Guid.NewGuid();

        _processor
            .Setup(x => x.ProcessAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageProcessingResult(
                "image/png",
                1200,
                800,
                [9, 8, 7]));

        _storage
            .SetupSequence(x => x.SaveAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("2026/09/original.png")
            .ReturnsAsync("2026/09/thumb.webp");

        _storage
            .Setup(x => x.GetPublicUrl(It.IsAny<string>()))
            .Returns((string key) => "/uploads/media/" + key);

        _auth.Setup(x => x.GetCurrentUserId()).ReturnsAsync(userId.ToString());

        Media? saved = null;
        _repository
            .Setup(x => x.AddAsync(It.IsAny<Media>()))
            .ReturnsAsync((Media item) =>
            {
                saved = item;
                return item;
            });

        _repository
            .Setup(x => x.GetWithUploaderAsync(It.IsAny<Guid>()))
            .ReturnsAsync(() => saved);

        await using MemoryStream stream = new(bytes);

        var result = await _service.UploadAsync(new MediaUploadInput(
            stream,
            "photo.png",
            "image/png",
            bytes.Length,
            "Снимка",
            "Photo"));

        Assert.Equal("image/png", result.MimeType);
        Assert.Equal(1200, result.Width);
        Assert.Equal(800, result.Height);
        Assert.NotNull(result.ThumbnailUrl);
        Assert.Equal(userId, result.UploadedById);
        Assert.NotNull(saved);
        Assert.Equal(64, saved!.Sha256.Length);
    }

    [Fact]
    public async Task UploadAsync_ShouldRejectFileAboveConfiguredLimit()
    {
        await using MemoryStream stream = new(new byte[2]);

        await Assert.ThrowsAsync<AppException>(() =>
            _service.UploadAsync(new MediaUploadInput(
                stream,
                "huge.jpg",
                "image/jpeg",
                2048,
                null,
                null)));
    }

    [Fact]
    public async Task UploadAsync_ShouldRejectMimeMismatch()
    {
        byte[] bytes = [1, 2, 3];

        _processor
            .Setup(x => x.ProcessAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageProcessingResult(
                "image/png",
                100,
                100,
                [4]));

        await using MemoryStream stream = new(bytes);

        await Assert.ThrowsAsync<AppException>(() =>
            _service.UploadAsync(new MediaUploadInput(
                stream,
                "fake.jpg",
                "image/jpeg",
                bytes.Length,
                null,
                null)));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRejectMediaUsedAsCover()
    {
        Guid id = Guid.NewGuid();

        _repository
            .Setup(x => x.GetWithUploaderAsync(id))
            .ReturnsAsync(new Media
            {
                Id = id,
                OriginalFileName = "photo.png",
                StorageKey = "photo.png",
                MimeType = "image/png",
                Extension = ".png",
                Sha256 = new string('a', 64)
            });

        _repository.Setup(x => x.IsInUseAsync(id)).ReturnsAsync(true);

        await Assert.ThrowsAsync<AppException>(() => _service.DeleteAsync(id));

        _storage.Verify(
            x => x.DeleteAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
