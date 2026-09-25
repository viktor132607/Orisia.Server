using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Orisia.Server.Common.Options;
using Orisia.Server.Common.Responses.Media;
using Orisia.Server.Core.Exceptions;
using DataMedia = Orisia.Server.Data.Entities.Media;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Media;

namespace Orisia.Server.Domain.Services;

public class MediaService(
    IMediaRepository mediaRepository,
    IMediaStorage mediaStorage,
    IImageProcessor imageProcessor,
    IAuthService authService,
    IOptions<MediaStorageOptions> options) : IMediaService
{
    private static readonly IReadOnlyDictionary<string, string> AllowedTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
            ["image/gif"] = ".gif"
        };

    private readonly MediaStorageOptions _options = options.Value;

    public async Task<MediaResponse> UploadAsync(
        MediaUploadInput input,
        CancellationToken cancellationToken = default)
    {
        ValidateInput(input);

        await using MemoryStream content = new();
        await input.Content.CopyToAsync(content, cancellationToken);

        if (content.Length == 0 || content.Length != input.Length)
        {
            throw new AppException("The uploaded file is incomplete.").SetStatusCode(400);
        }

        content.Position = 0;
        ImageProcessingResult image;

        try
        {
            image = await imageProcessor.ProcessAsync(content, cancellationToken);
        }
        catch (Exception ex) when (ex is not AppException)
        {
            throw new AppException("The uploaded file is not a valid supported image.")
                .SetStatusCode(400);
        }

        if (!AllowedTypes.TryGetValue(image.DetectedMimeType, out string? detectedExtension)
            || !MimeTypesEquivalent(input.ContentType, image.DetectedMimeType))
        {
            throw new AppException("The file content does not match its declared MIME type.")
                .SetStatusCode(400);
        }

        string requestedExtension = Path.GetExtension(input.OriginalFileName).ToLowerInvariant();
        if (!ExtensionMatches(requestedExtension, detectedExtension))
        {
            throw new AppException("The file extension does not match the uploaded image type.")
                .SetStatusCode(400);
        }

        content.Position = 0;
        string sha256 = Convert.ToHexString(
            SHA256.HashData(content.ToArray()))
            .ToLowerInvariant();

        string? storageKey = null;
        string? thumbnailStorageKey = null;

        try
        {
            content.Position = 0;
            storageKey = await mediaStorage.SaveAsync(
                content,
                detectedExtension,
                cancellationToken);

            await using MemoryStream thumbnail = new(image.ThumbnailBytes, writable: false);
            thumbnailStorageKey = await mediaStorage.SaveAsync(
                thumbnail,
                ".webp",
                cancellationToken);

            Guid? uploaderId = await GetCurrentUserIdAsync();

            DataMedia entity = new()
            {
                OriginalFileName = SanitizeFileName(input.OriginalFileName),
                StorageKey = storageKey,
                ThumbnailStorageKey = thumbnailStorageKey,
                MimeType = image.DetectedMimeType,
                Extension = detectedExtension,
                SizeBytes = input.Length,
                Sha256 = sha256,
                Width = image.Width,
                Height = image.Height,
                AltBg = NormalizeOptional(input.AltBg),
                AltEn = NormalizeOptional(input.AltEn),
                UploadedById = uploaderId
            };

            DataMedia? created = await mediaRepository.AddAsync(entity);
            if (created is null)
            {
                throw new AppException("Media could not be saved.").SetStatusCode(500);
            }

            return await GetByIdAsync(created.Id);
        }
        catch
        {
            if (thumbnailStorageKey is not null)
            {
                await mediaStorage.DeleteAsync(thumbnailStorageKey, cancellationToken);
            }

            if (storageKey is not null)
            {
                await mediaStorage.DeleteAsync(storageKey, cancellationToken);
            }

            throw;
        }
    }

    public async Task<IEnumerable<MediaResponse>> GetAllAsync()
    {
        IEnumerable<DataMedia> items = await mediaRepository.GetAllWithUploaderAsync();
        return items.Select(Map);
    }

    public async Task<MediaResponse> GetByIdAsync(Guid id)
    {
        DataMedia? item = await mediaRepository.GetWithUploaderAsync(id);
        if (item is null)
        {
            throw new AppException("Media not found.").SetStatusCode(404);
        }

        return Map(item);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        DataMedia? item = await mediaRepository.GetWithUploaderAsync(id);
        if (item is null)
        {
            throw new AppException("Media not found.").SetStatusCode(404);
        }

        if (await mediaRepository.IsInUseAsync(id))
        {
            throw new AppException("Media is currently used as a cover and cannot be deleted.")
                .SetStatusCode(409);
        }

        bool deleted = await mediaRepository.DeleteAsync(id);
        if (!deleted)
        {
            return false;
        }

        if (item.ThumbnailStorageKey is not null)
        {
            await mediaStorage.DeleteAsync(item.ThumbnailStorageKey, cancellationToken);
        }

        await mediaStorage.DeleteAsync(item.StorageKey, cancellationToken);
        return true;
    }

    private MediaResponse Map(DataMedia item)
    {
        return new MediaResponse
        {
            Id = item.Id,
            OriginalFileName = item.OriginalFileName,
            MimeType = item.MimeType,
            Extension = item.Extension,
            SizeBytes = item.SizeBytes,
            Sha256 = item.Sha256,
            Width = item.Width,
            Height = item.Height,
            AltBg = item.AltBg,
            AltEn = item.AltEn,
            Url = mediaStorage.GetPublicUrl(item.StorageKey),
            ThumbnailUrl = item.ThumbnailStorageKey is null
                ? null
                : mediaStorage.GetPublicUrl(item.ThumbnailStorageKey),
            UploadedById = item.UploadedById,
            UploadedByName = item.UploadedBy?.Names,
            CreatedOn = item.CreatedOn
        };
    }

    private void ValidateInput(MediaUploadInput input)
    {
        if (input.Content is null || !input.Content.CanRead)
        {
            throw new AppException("A readable file is required.").SetStatusCode(400);
        }

        if (input.Length <= 0)
        {
            throw new AppException("The uploaded file is empty.").SetStatusCode(400);
        }

        if (input.Length > _options.MaxFileSizeBytes)
        {
            throw new AppException(
                $"The uploaded file exceeds the maximum size of {_options.MaxFileSizeBytes} bytes.")
                .SetStatusCode(413);
        }

        if (!AllowedTypes.ContainsKey(input.ContentType))
        {
            throw new AppException(
                "Unsupported media type. Allowed: JPEG, PNG, WebP and GIF.")
                .SetStatusCode(415);
        }

        if (string.IsNullOrWhiteSpace(input.OriginalFileName))
        {
            throw new AppException("File name is required.").SetStatusCode(400);
        }
    }

    private async Task<Guid?> GetCurrentUserIdAsync()
    {
        string? id = await authService.GetCurrentUserId();
        return Guid.TryParse(id, out Guid userId) ? userId : null;
    }

    private static bool MimeTypesEquivalent(string declared, string detected)
    {
        return declared.Equals(detected, StringComparison.OrdinalIgnoreCase)
            || (
                declared.Equals("image/jpg", StringComparison.OrdinalIgnoreCase)
                && detected.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ExtensionMatches(string requested, string detected)
    {
        return requested.Equals(detected, StringComparison.OrdinalIgnoreCase)
            || (
                requested.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                && detected.Equals(".jpg", StringComparison.OrdinalIgnoreCase));
    }

    private static string SanitizeFileName(string fileName)
    {
        string value = Path.GetFileName(fileName).Trim();
        return value.Length <= 255 ? value : value[..255];
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
