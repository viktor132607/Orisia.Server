using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Orisia.Server.Common.Requests.Horoteka;
using Orisia.Server.Common.Responses.Horoteka;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class DanceService(
    IDanceRepository danceRepository,
    IMediaRepository mediaRepository,
    IMediaStorage mediaStorage) : IDanceService
{
    private static readonly Dictionary<char, string> BulgarianTransliteration = new()
    {
        ['а']="a", ['б']="b", ['в']="v", ['г']="g", ['д']="d", ['е']="e", ['ж']="zh",
        ['з']="z", ['и']="i", ['й']="y", ['к']="k", ['л']="l", ['м']="m", ['н']="n",
        ['о']="o", ['п']="p", ['р']="r", ['с']="s", ['т']="t", ['у']="u", ['ф']="f",
        ['х']="h", ['ц']="ts", ['ч']="ch", ['ш']="sh", ['щ']="sht", ['ъ']="a", ['ь']="y",
        ['ю']="yu", ['я']="ya"
    };

    public async Task<IEnumerable<DanceResponse>> GetPublicAsync(string? region = null)
    {
        IEnumerable<Dance> dances = await danceRepository.GetPublicAsync(region);
        return dances.Select(Map);
    }

    public async Task<DanceResponse> GetPublicBySlugAsync(string slug)
    {
        Dance? dance = await danceRepository.GetBySlugAsync(NormalizeSlug(slug), true);
        if (dance is null)
        {
            throw new AppException("Dance not found.").SetStatusCode(404);
        }

        return Map(dance);
    }

    public async Task<IEnumerable<DanceResponse>> GetAdminAsync()
    {
        IEnumerable<Dance> dances = await danceRepository.GetAdminAsync();
        return dances.Select(Map);
    }

    public async Task<DanceResponse> GetAdminByIdAsync(Guid id)
    {
        Dance? dance = await danceRepository.GetWithThumbnailAsync(id);
        if (dance is null)
        {
            throw new AppException("Dance not found.").SetStatusCode(404);
        }

        return Map(dance);
    }

    public async Task<DanceResponse> CreateAsync(CreateDanceRequest request)
    {
        string slug = BuildSlug(request.Slug, request.TitleEn, request.TitleBg);

        if (await danceRepository.SlugExistsAsync(slug))
        {
            throw new AppException("A dance with this slug already exists.").SetStatusCode(409);
        }

        await ValidateThumbnailAsync(request.ThumbnailMediaId);

        Dance dance = new()
        {
            Slug = slug,
            TitleBg = request.TitleBg.Trim(),
            TitleEn = request.TitleEn.Trim(),
            DescriptionBg = request.DescriptionBg.Trim(),
            DescriptionEn = request.DescriptionEn.Trim(),
            Region = NormalizeOptional(request.Region),
            Rhythm = NormalizeOptional(request.Rhythm),
            VideoUrl = NormalizeOptional(request.VideoUrl),
            ThumbnailMediaId = request.ThumbnailMediaId,
            DurationSeconds = request.DurationSeconds,
            SortOrder = request.SortOrder,
            Active = request.Active
        };

        Dance? created = await danceRepository.AddAsync(dance);
        if (created is null)
        {
            throw new AppException("Dance could not be created.").SetStatusCode(500);
        }

        return await GetAdminByIdAsync(created.Id);
    }

    public async Task<DanceResponse> UpdateAsync(Guid id, UpdateDanceRequest request)
    {
        Dance? existing = await danceRepository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new AppException("Dance not found.").SetStatusCode(404);
        }

        string slug = BuildSlug(request.Slug, request.TitleEn, request.TitleBg);
        if (await danceRepository.SlugExistsAsync(slug, id))
        {
            throw new AppException("A dance with this slug already exists.").SetStatusCode(409);
        }

        await ValidateThumbnailAsync(request.ThumbnailMediaId);

        Dance updated = new()
        {
            Id = existing.Id,
            Slug = slug,
            TitleBg = request.TitleBg.Trim(),
            TitleEn = request.TitleEn.Trim(),
            DescriptionBg = request.DescriptionBg.Trim(),
            DescriptionEn = request.DescriptionEn.Trim(),
            Region = NormalizeOptional(request.Region),
            Rhythm = NormalizeOptional(request.Rhythm),
            VideoUrl = NormalizeOptional(request.VideoUrl),
            ThumbnailMediaId = request.ThumbnailMediaId,
            DurationSeconds = request.DurationSeconds,
            SortOrder = request.SortOrder,
            Active = request.Active
        };

        _ = await danceRepository.UpdateAsync(updated)
            ?? throw new AppException("Dance not found.").SetStatusCode(404);

        return await GetAdminByIdAsync(id);
    }

    public async Task ReorderAsync(ReorderDancesRequest request)
    {
        Dictionary<Guid, int> order = new();

        foreach (DanceOrderItem item in request.Items)
        {
            if (!order.TryAdd(item.DanceId, item.SortOrder))
            {
                throw new AppException("Duplicate dance id in reorder request.").SetStatusCode(400);
            }
        }

        try
        {
            await danceRepository.ReorderAsync(order);
        }
        catch (InvalidOperationException)
        {
            throw new AppException("One or more dances were not found.").SetStatusCode(400);
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _ = await GetAdminByIdAsync(id);
        return await danceRepository.DeleteAsync(id);
    }

    private async Task ValidateThumbnailAsync(Guid? mediaId)
    {
        if (!mediaId.HasValue)
        {
            return;
        }

        if (await mediaRepository.GetWithUploaderAsync(mediaId.Value) is null)
        {
            throw new AppException("Thumbnail media not found.").SetStatusCode(404);
        }
    }

    private DanceResponse Map(Dance dance)
    {
        return new DanceResponse
        {
            Id = dance.Id,
            Slug = dance.Slug,
            TitleBg = dance.TitleBg,
            TitleEn = dance.TitleEn,
            DescriptionBg = dance.DescriptionBg,
            DescriptionEn = dance.DescriptionEn,
            Region = dance.Region,
            Rhythm = dance.Rhythm,
            VideoUrl = dance.VideoUrl,
            ThumbnailMediaId = dance.ThumbnailMediaId,
            ThumbnailUrl = dance.ThumbnailMedia is null || dance.ThumbnailMedia.IsDeleted
                ? null
                : mediaStorage.GetPublicUrl(
                    dance.ThumbnailMedia.ThumbnailStorageKey
                    ?? dance.ThumbnailMedia.StorageKey),
            DurationSeconds = dance.DurationSeconds,
            SortOrder = dance.SortOrder,
            Active = dance.Active,
            CreatedOn = dance.CreatedOn,
            ModifiedOn = dance.ModifiedOn
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
