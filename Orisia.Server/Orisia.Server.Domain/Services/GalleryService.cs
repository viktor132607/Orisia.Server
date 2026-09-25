using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Orisia.Server.Common.Requests.Gallery;
using Orisia.Server.Common.Responses.Gallery;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class GalleryService(
    IGalleryRepository galleryRepository,
    IMediaRepository mediaRepository,
    IMediaStorage mediaStorage) : IGalleryService
{
    private static readonly Dictionary<char, string> BulgarianTransliteration = new()
    {
        ['а']="a", ['б']="b", ['в']="v", ['г']="g", ['д']="d", ['е']="e", ['ж']="zh",
        ['з']="z", ['и']="i", ['й']="y", ['к']="k", ['л']="l", ['м']="m", ['н']="n",
        ['о']="o", ['п']="p", ['р']="r", ['с']="s", ['т']="t", ['у']="u", ['ф']="f",
        ['х']="h", ['ц']="ts", ['ч']="ch", ['ш']="sh", ['щ']="sht", ['ъ']="a", ['ь']="y",
        ['ю']="yu", ['я']="ya"
    };

    public async Task<IEnumerable<GalleryAlbumResponse>> GetPublicAlbumsAsync(bool? featured = null)
    {
        IEnumerable<GalleryAlbum> albums = await galleryRepository.GetAlbumsAsync(true, featured);
        return albums.Select(album => MapAlbum(album, publicOnly: true));
    }

    public async Task<GalleryAlbumResponse> GetPublicAlbumBySlugAsync(string slug)
    {
        GalleryAlbum? album = await galleryRepository.GetAlbumBySlugAsync(NormalizeSlug(slug), true);
        if (album is null)
        {
            throw new AppException("Gallery album not found.").SetStatusCode(404);
        }

        return MapAlbum(album, publicOnly: true);
    }

    public async Task<IEnumerable<GalleryAlbumResponse>> GetAdminAlbumsAsync()
    {
        IEnumerable<GalleryAlbum> albums = await galleryRepository.GetAlbumsAsync(false);
        return albums.Select(album => MapAlbum(album, publicOnly: false));
    }

    public async Task<GalleryAlbumResponse> GetAdminAlbumByIdAsync(Guid id)
    {
        GalleryAlbum? album = await galleryRepository.GetAlbumByIdAsync(id);
        if (album is null)
        {
            throw new AppException("Gallery album not found.").SetStatusCode(404);
        }

        return MapAlbum(album, publicOnly: false);
    }

    public async Task<GalleryAlbumResponse> CreateAlbumAsync(CreateGalleryAlbumRequest request)
    {
        string slug = BuildSlug(request.Slug, request.TitleEn, request.TitleBg);

        if (await galleryRepository.SlugExistsAsync(slug))
        {
            throw new AppException("A gallery album with this slug already exists.").SetStatusCode(409);
        }

        await ValidateCoverAsync(request.CoverMediaId);

        GalleryAlbum album = new()
        {
            Slug = slug,
            TitleBg = request.TitleBg.Trim(),
            TitleEn = request.TitleEn.Trim(),
            DescriptionBg = NormalizeOptional(request.DescriptionBg),
            DescriptionEn = NormalizeOptional(request.DescriptionEn),
            CoverMediaId = request.CoverMediaId,
            Active = request.Active,
            Featured = request.Featured,
            SortOrder = request.SortOrder
        };

        GalleryAlbum created = await galleryRepository.AddAlbumAsync(album);
        return await GetAdminAlbumByIdAsync(created.Id);
    }

    public async Task<GalleryAlbumResponse> UpdateAlbumAsync(Guid id, UpdateGalleryAlbumRequest request)
    {
        GalleryAlbum? existing = await galleryRepository.GetAlbumByIdAsync(id);
        if (existing is null)
        {
            throw new AppException("Gallery album not found.").SetStatusCode(404);
        }

        string slug = BuildSlug(request.Slug, request.TitleEn, request.TitleBg);
        if (await galleryRepository.SlugExistsAsync(slug, id))
        {
            throw new AppException("A gallery album with this slug already exists.").SetStatusCode(409);
        }

        await ValidateCoverAsync(request.CoverMediaId);

        GalleryAlbum updated = new()
        {
            Id = existing.Id,
            Slug = slug,
            TitleBg = request.TitleBg.Trim(),
            TitleEn = request.TitleEn.Trim(),
            DescriptionBg = NormalizeOptional(request.DescriptionBg),
            DescriptionEn = NormalizeOptional(request.DescriptionEn),
            CoverMediaId = request.CoverMediaId,
            Active = request.Active,
            Featured = request.Featured,
            SortOrder = request.SortOrder
        };

        _ = await galleryRepository.UpdateAlbumAsync(updated)
            ?? throw new AppException("Gallery album not found.").SetStatusCode(404);

        return await GetAdminAlbumByIdAsync(id);
    }

    public async Task<bool> DeleteAlbumAsync(Guid id)
    {
        _ = await GetAdminAlbumByIdAsync(id);
        return await galleryRepository.DeleteAlbumAsync(id);
    }

    public async Task<IReadOnlyCollection<GalleryMediaResponse>> AddMediaAsync(
        Guid albumId,
        AddGalleryMediaRequest request)
    {
        _ = await RequireAlbumAsync(albumId);

        if (request.Items.Count == 0)
        {
            throw new AppException("At least one media item is required.").SetStatusCode(400);
        }

        Guid[] duplicateRequestIds = request.Items
            .GroupBy(item => item.MediaId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateRequestIds.Length > 0)
        {
            throw new AppException("The same media item cannot be added twice in one request.")
                .SetStatusCode(400);
        }

        int nextSortOrder = await galleryRepository.GetNextSortOrderAsync(albumId);
        List<GalleryMedia> links = [];

        foreach (GalleryMediaInput input in request.Items)
        {
            Media? media = await mediaRepository.GetWithUploaderAsync(input.MediaId);
            if (media is null)
            {
                throw new AppException($"Media {input.MediaId} not found.").SetStatusCode(404);
            }

            if (await galleryRepository.MediaLinkExistsAsync(albumId, input.MediaId))
            {
                throw new AppException("Media is already part of this album.").SetStatusCode(409);
            }

            links.Add(new GalleryMedia
            {
                GalleryAlbumId = albumId,
                MediaId = input.MediaId,
                CaptionBg = NormalizeOptional(input.CaptionBg),
                CaptionEn = NormalizeOptional(input.CaptionEn),
                SortOrder = input.SortOrder ?? nextSortOrder++,
                Active = input.Active
            });
        }

        IReadOnlyCollection<GalleryMedia> created = await galleryRepository.AddMediaAsync(links);

        List<GalleryMediaResponse> result = [];
        foreach (GalleryMedia link in created)
        {
            GalleryMedia? loaded = await galleryRepository.GetGalleryMediaAsync(link.Id);
            if (loaded is not null)
            {
                result.Add(MapMedia(loaded));
            }
        }

        return result
            .OrderBy(item => item.SortOrder)
            .ToArray();
    }

    public async Task<GalleryMediaResponse> UpdateMediaAsync(
        Guid galleryMediaId,
        UpdateGalleryMediaRequest request)
    {
        GalleryMedia? existing = await galleryRepository.GetGalleryMediaAsync(galleryMediaId);
        if (existing is null)
        {
            throw new AppException("Gallery media item not found.").SetStatusCode(404);
        }

        GalleryMedia updated = new()
        {
            Id = existing.Id,
            GalleryAlbumId = existing.GalleryAlbumId,
            MediaId = existing.MediaId,
            CaptionBg = NormalizeOptional(request.CaptionBg),
            CaptionEn = NormalizeOptional(request.CaptionEn),
            SortOrder = request.SortOrder,
            Active = request.Active
        };

        _ = await galleryRepository.UpdateGalleryMediaAsync(updated)
            ?? throw new AppException("Gallery media item not found.").SetStatusCode(404);

        GalleryMedia loaded = await galleryRepository.GetGalleryMediaAsync(galleryMediaId)
            ?? throw new AppException("Gallery media item not found.").SetStatusCode(404);

        return MapMedia(loaded);
    }

    public async Task<GalleryMediaResponse> MoveMediaAsync(
        Guid galleryMediaId,
        MoveGalleryMediaRequest request)
    {
        GalleryMedia? existing = await galleryRepository.GetGalleryMediaAsync(galleryMediaId);
        if (existing is null)
        {
            throw new AppException("Gallery media item not found.").SetStatusCode(404);
        }

        _ = await RequireAlbumAsync(request.TargetAlbumId);

        if (existing.GalleryAlbumId != request.TargetAlbumId
            && await galleryRepository.MediaLinkExistsAsync(request.TargetAlbumId, existing.MediaId))
        {
            throw new AppException("Media is already part of the target album.").SetStatusCode(409);
        }

        int sortOrder = request.SortOrder
            ?? await galleryRepository.GetNextSortOrderAsync(request.TargetAlbumId);

        GalleryMedia updated = new()
        {
            Id = existing.Id,
            GalleryAlbumId = request.TargetAlbumId,
            MediaId = existing.MediaId,
            CaptionBg = existing.CaptionBg,
            CaptionEn = existing.CaptionEn,
            SortOrder = sortOrder,
            Active = existing.Active
        };

        _ = await galleryRepository.UpdateGalleryMediaAsync(updated)
            ?? throw new AppException("Gallery media item not found.").SetStatusCode(404);

        GalleryMedia loaded = await galleryRepository.GetGalleryMediaAsync(galleryMediaId)
            ?? throw new AppException("Gallery media item not found.").SetStatusCode(404);

        return MapMedia(loaded);
    }

    public async Task ReorderMediaAsync(Guid albumId, ReorderGalleryMediaRequest request)
    {
        _ = await RequireAlbumAsync(albumId);

        Dictionary<Guid, int> order = new();
        foreach (GalleryMediaOrderItem item in request.Items)
        {
            if (!order.TryAdd(item.GalleryMediaId, item.SortOrder))
            {
                throw new AppException("Duplicate gallery media id in reorder request.").SetStatusCode(400);
            }
        }

        try
        {
            await galleryRepository.ReorderAsync(albumId, order);
        }
        catch (InvalidOperationException)
        {
            throw new AppException("One or more gallery media items do not belong to this album.")
                .SetStatusCode(400);
        }
    }

    public async Task<bool> DeleteMediaAsync(Guid galleryMediaId)
    {
        GalleryMedia? existing = await galleryRepository.GetGalleryMediaAsync(galleryMediaId);
        if (existing is null)
        {
            throw new AppException("Gallery media item not found.").SetStatusCode(404);
        }

        return await galleryRepository.DeleteGalleryMediaAsync(galleryMediaId);
    }

    private async Task<GalleryAlbum> RequireAlbumAsync(Guid id)
    {
        return await galleryRepository.GetAlbumByIdAsync(id)
            ?? throw new AppException("Gallery album not found.").SetStatusCode(404);
    }

    private async Task ValidateCoverAsync(Guid? mediaId)
    {
        if (!mediaId.HasValue)
        {
            return;
        }

        if (await mediaRepository.GetWithUploaderAsync(mediaId.Value) is null)
        {
            throw new AppException("Cover media not found.").SetStatusCode(404);
        }
    }

    private GalleryAlbumResponse MapAlbum(GalleryAlbum album, bool publicOnly)
    {
        IEnumerable<GalleryMedia> items = album.Items
            .Where(item => !item.IsDeleted);

        if (publicOnly)
        {
            items = items.Where(item => item.Active && item.Media is { IsDeleted: false });
        }

        return new GalleryAlbumResponse
        {
            Id = album.Id,
            Slug = album.Slug,
            TitleBg = album.TitleBg,
            TitleEn = album.TitleEn,
            DescriptionBg = album.DescriptionBg,
            DescriptionEn = album.DescriptionEn,
            CoverMediaId = album.CoverMediaId,
            CoverUrl = album.CoverMedia is null || album.CoverMedia.IsDeleted
                ? null
                : mediaStorage.GetPublicUrl(album.CoverMedia.StorageKey),
            CoverThumbnailUrl = album.CoverMedia?.ThumbnailStorageKey is null || album.CoverMedia.IsDeleted
                ? null
                : mediaStorage.GetPublicUrl(album.CoverMedia.ThumbnailStorageKey),
            Active = album.Active,
            Featured = album.Featured,
            SortOrder = album.SortOrder,
            Items = items
                .OrderBy(item => item.SortOrder)
                .Select(MapMedia)
                .ToArray(),
            CreatedOn = album.CreatedOn,
            ModifiedOn = album.ModifiedOn
        };
    }

    private GalleryMediaResponse MapMedia(GalleryMedia item)
    {
        Media media = item.Media
            ?? throw new InvalidOperationException("Gallery media relation was not loaded.");

        return new GalleryMediaResponse
        {
            Id = item.Id,
            MediaId = item.MediaId,
            Url = mediaStorage.GetPublicUrl(media.StorageKey),
            ThumbnailUrl = media.ThumbnailStorageKey is null
                ? null
                : mediaStorage.GetPublicUrl(media.ThumbnailStorageKey),
            AltBg = media.AltBg,
            AltEn = media.AltEn,
            CaptionBg = item.CaptionBg,
            CaptionEn = item.CaptionEn,
            SortOrder = item.SortOrder,
            Active = item.Active
        };
    }

    private static string BuildSlug(string? requestedSlug, string titleEn, string titleBg)
    {
        string source = !string.IsNullOrWhiteSpace(requestedSlug)
            ? requestedSlug
            : !string.IsNullOrWhiteSpace(titleEn)
                ? titleEn
                : titleBg;

        string slug = NormalizeSlug(source);
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new AppException("A valid slug could not be generated.").SetStatusCode(400);
        }

        return slug;
    }

    private static string NormalizeSlug(string value)
    {
        string lower = value.Trim().ToLowerInvariant();
        StringBuilder transliterated = new();

        foreach (char character in lower)
        {
            transliterated.Append(
                BulgarianTransliteration.TryGetValue(character, out string? replacement)
                    ? replacement
                    : character);
        }

        string normalized = transliterated.ToString().Normalize(NormalizationForm.FormD);
        StringBuilder ascii = new();

        foreach (char character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                ascii.Append(character);
            }
        }

        string slug = Regex.Replace(ascii.ToString(), "[^a-z0-9]+", "-");
        slug = Regex.Replace(slug, "-{2,}", "-").Trim('-');
        return slug.Length <= 180 ? slug : slug[..180].TrimEnd('-');
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
