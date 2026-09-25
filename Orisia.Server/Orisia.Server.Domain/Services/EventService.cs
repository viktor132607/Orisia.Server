using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Orisia.Server.Common.Requests.Events;
using Orisia.Server.Common.Responses.Events;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;
using Orisia.Server.Domain.Validation;

namespace Orisia.Server.Domain.Services;

public class EventService(IEventRepository eventRepository) : IEventService
{
    private static readonly Dictionary<char, string> BulgarianTransliteration = new()
    {
        ['а']="a", ['б']="b", ['в']="v", ['г']="g", ['д']="d", ['е']="e", ['ж']="zh",
        ['з']="z", ['и']="i", ['й']="y", ['к']="k", ['л']="l", ['м']="m", ['н']="n",
        ['о']="o", ['п']="p", ['р']="r", ['с']="s", ['т']="t", ['у']="u", ['ф']="f",
        ['х']="h", ['ц']="ts", ['ч']="ch", ['ш']="sh", ['щ']="sht", ['ъ']="a", ['ь']="y",
        ['ю']="yu", ['я']="ya"
    };

    public async Task<IEnumerable<EventResponse>> GetPublishedAsync(
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        EventType? type = null,
        bool? featured = null)
    {
        DateTime? fromUtc = from?.UtcDateTime;
        DateTime? toUtc = to?.UtcDateTime;

        if (fromUtc.HasValue && toUtc.HasValue && fromUtc > toUtc)
        {
            throw new AppException("From date cannot be after to date.").SetStatusCode(400);
        }

        IEnumerable<Event> events = await eventRepository.GetPublishedAsync(fromUtc, toUtc, type, featured);
        return events.Select(Map);
    }

    public async Task<IEnumerable<EventResponse>> GetUpcomingAsync(EventType? type = null, int take = 10)
    {
        ValidateTake(take);
        IEnumerable<Event> events = await eventRepository.GetUpcomingAsync(DateTime.UtcNow, type, take);
        return events.Select(Map);
    }

    public async Task<IEnumerable<EventResponse>> GetPastAsync(EventType? type = null, int take = 10)
    {
        ValidateTake(take);
        IEnumerable<Event> events = await eventRepository.GetPastAsync(DateTime.UtcNow, type, take);
        return events.Select(Map);
    }

    public async Task<EventResponse> GetPublishedBySlugAsync(string slug)
    {
        Event? item = await eventRepository.GetBySlugAsync(NormalizeSlug(slug));
        if (item is null || item.Status != PublicationStatus.Published)
        {
            throw new AppException("Event not found.").SetStatusCode(404);
        }

        return Map(item);
    }

    public async Task<IEnumerable<EventResponse>> GetAdminAsync(
        EventType? type = null,
        PublicationStatus? status = null)
    {
        IEnumerable<Event> events = await eventRepository.GetForAdminAsync(type, status);
        return events.Select(Map);
    }

    public async Task<EventResponse> GetAdminByIdAsync(Guid id)
    {
        Event? item = await eventRepository.GetByIdAsync(id);
        if (item is null)
        {
            throw new AppException("Event not found.").SetStatusCode(404);
        }

        return Map(item);
    }

    public async Task<EventResponse> CreateAsync(CreateEventRequest request)
    {
        ValidateDates(request.StartAt, request.EndAt);

        string slug = BuildSlug(request.Slug, request.TitleEn, request.TitleBg);
        if (await eventRepository.SlugExistsAsync(slug))
        {
            throw new AppException("An event with this slug already exists.").SetStatusCode(409);
        }

        Event item = new()
        {
            Slug = slug,
            TitleBg = request.TitleBg.Trim(),
            TitleEn = request.TitleEn.Trim(),
            DescriptionBg = request.DescriptionBg.Trim(),
            DescriptionEn = request.DescriptionEn.Trim(),
            StartAt = request.StartAt.UtcDateTime,
            EndAt = request.EndAt?.UtcDateTime,
            AllDay = request.AllDay,
            EventType = request.EventType,
            Location = NormalizeOptional(request.Location),
            CoverMediaId = request.CoverMediaId,
            Featured = request.Featured,
            Status = PublicationStatus.Draft,
            RecurrenceRule = RecurrenceRuleValidator.NormalizeAndValidate(request.RecurrenceRule)
        };

        Event? created = await eventRepository.AddAsync(item);
        if (created is null)
        {
            throw new AppException("Event could not be created.").SetStatusCode(500);
        }

        return Map(created);
    }

    public async Task<EventResponse> UpdateAsync(Guid id, UpdateEventRequest request)
    {
        Event? existing = await eventRepository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new AppException("Event not found.").SetStatusCode(404);
        }

        ValidateDates(request.StartAt, request.EndAt);

        string slug = BuildSlug(request.Slug, request.TitleEn, request.TitleBg);
        if (await eventRepository.SlugExistsAsync(slug, id))
        {
            throw new AppException("An event with this slug already exists.").SetStatusCode(409);
        }

        Event updated = new()
        {
            Id = existing.Id,
            Slug = slug,
            TitleBg = request.TitleBg.Trim(),
            TitleEn = request.TitleEn.Trim(),
            DescriptionBg = request.DescriptionBg.Trim(),
            DescriptionEn = request.DescriptionEn.Trim(),
            StartAt = request.StartAt.UtcDateTime,
            EndAt = request.EndAt?.UtcDateTime,
            AllDay = request.AllDay,
            EventType = request.EventType,
            Location = NormalizeOptional(request.Location),
            CoverMediaId = request.CoverMediaId,
            Featured = request.Featured,
            Status = existing.Status,
            RecurrenceRule = RecurrenceRuleValidator.NormalizeAndValidate(request.RecurrenceRule)
        };

        Event? saved = await eventRepository.UpdateAsync(updated);
        if (saved is null)
        {
            throw new AppException("Event not found.").SetStatusCode(404);
        }

        return Map(saved);
    }

    public Task<EventResponse> PublishAsync(Guid id)
    {
        return ChangeStatusAsync(id, PublicationStatus.Published);
    }

    public Task<EventResponse> UnpublishAsync(Guid id)
    {
        return ChangeStatusAsync(id, PublicationStatus.Draft);
    }

    public Task<EventResponse> ArchiveAsync(Guid id)
    {
        return ChangeStatusAsync(id, PublicationStatus.Archived);
    }

    public async Task<EventResponse> SetFeaturedAsync(Guid id, bool featured)
    {
        Event? item = await eventRepository.GetByIdAsync(id);
        if (item is null)
        {
            throw new AppException("Event not found.").SetStatusCode(404);
        }

        Event updated = Clone(item);
        updated.Featured = featured;

        Event? saved = await eventRepository.UpdateAsync(updated);
        if (saved is null)
        {
            throw new AppException("Event not found.").SetStatusCode(404);
        }

        return Map(saved);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _ = await GetAdminByIdAsync(id);
        return await eventRepository.DeleteAsync(id);
    }

    private async Task<EventResponse> ChangeStatusAsync(Guid id, PublicationStatus status)
    {
        Event? item = await eventRepository.GetByIdAsync(id);
        if (item is null)
        {
            throw new AppException("Event not found.").SetStatusCode(404);
        }

        Event updated = Clone(item);
        updated.Status = status;

        Event? saved = await eventRepository.UpdateAsync(updated);
        if (saved is null)
        {
            throw new AppException("Event not found.").SetStatusCode(404);
        }

        return Map(saved);
    }

    private static Event Clone(Event item)
    {
        return new Event
        {
            Id = item.Id,
            Slug = item.Slug,
            TitleBg = item.TitleBg,
            TitleEn = item.TitleEn,
            DescriptionBg = item.DescriptionBg,
            DescriptionEn = item.DescriptionEn,
            StartAt = item.StartAt,
            EndAt = item.EndAt,
            AllDay = item.AllDay,
            EventType = item.EventType,
            Location = item.Location,
            CoverMediaId = item.CoverMediaId,
            Featured = item.Featured,
            Status = item.Status,
            RecurrenceRule = item.RecurrenceRule
        };
    }

    private static EventResponse Map(Event item)
    {
        return new EventResponse
        {
            Id = item.Id,
            Slug = item.Slug,
            TitleBg = item.TitleBg,
            TitleEn = item.TitleEn,
            DescriptionBg = item.DescriptionBg,
            DescriptionEn = item.DescriptionEn,
            StartAt = item.StartAt,
            EndAt = item.EndAt,
            AllDay = item.AllDay,
            EventType = item.EventType,
            Location = item.Location,
            CoverMediaId = item.CoverMediaId,
            Featured = item.Featured,
            Status = item.Status,
            RecurrenceRule = item.RecurrenceRule,
            CreatedOn = item.CreatedOn,
            ModifiedOn = item.ModifiedOn
        };
    }

    private static void ValidateDates(DateTimeOffset startAt, DateTimeOffset? endAt)
    {
        if (endAt.HasValue && endAt.Value < startAt)
        {
            throw new AppException("Event end date cannot be before start date.").SetStatusCode(400);
        }
    }

    private static void ValidateTake(int take)
    {
        if (take is < 1 or > 100)
        {
            throw new AppException("Take must be between 1 and 100.").SetStatusCode(400);
        }
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
