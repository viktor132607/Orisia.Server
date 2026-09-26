using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Orisia.Server.Common.Requests.Groups;
using Orisia.Server.Common.Responses.Groups;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class GroupService(IGroupRepository groupRepository) : IGroupService
{
    private static readonly Dictionary<char, string> BulgarianTransliteration = new()
    {
        ['а']="a", ['б']="b", ['в']="v", ['г']="g", ['д']="d", ['е']="e", ['ж']="zh",
        ['з']="z", ['и']="i", ['й']="y", ['к']="k", ['л']="l", ['м']="m", ['н']="n",
        ['о']="o", ['п']="p", ['р']="r", ['с']="s", ['т']="t", ['у']="u", ['ф']="f",
        ['х']="h", ['ц']="ts", ['ч']="ch", ['ш']="sh", ['щ']="sht", ['ъ']="a", ['ь']="y",
        ['ю']="yu", ['я']="ya"
    };

    public async Task<IEnumerable<GroupResponse>> GetPublicAsync() =>
        (await groupRepository.GetPublicAsync()).Select(Map);

    public async Task<GroupResponse> GetPublicBySlugAsync(string slug)
    {
        DanceGroup? group = await groupRepository.GetBySlugAsync(NormalizeSlug(slug), true);
        if (group is null) throw new AppException("Group not found.").SetStatusCode(404);
        return Map(group);
    }

    public async Task<IEnumerable<GroupResponse>> GetAdminAsync() =>
        (await groupRepository.GetAdminAsync()).Select(Map);

    public async Task<GroupResponse> GetAdminByIdAsync(Guid id)
    {
        DanceGroup? group = await groupRepository.GetWithSchedulesAsync(id);
        if (group is null) throw new AppException("Group not found.").SetStatusCode(404);
        return Map(group);
    }

    public async Task<GroupResponse> CreateAsync(CreateGroupRequest request)
    {
        string slug = BuildSlug(request.Slug, request.NameEn, request.NameBg);
        if (await groupRepository.SlugExistsAsync(slug))
            throw new AppException("A group with this slug already exists.").SetStatusCode(409);

        DanceGroup group = new()
        {
            Slug = slug,
            NameBg = request.NameBg.Trim(),
            NameEn = request.NameEn.Trim(),
            DescriptionBg = request.DescriptionBg.Trim(),
            DescriptionEn = request.DescriptionEn.Trim(),
            Location = NormalizeOptional(request.Location),
            Active = request.Active,
            SortOrder = request.SortOrder,
            Schedules = BuildSchedules(request.Schedules)
        };

        DanceGroup? created = await groupRepository.AddAsync(group);
        if (created is null) throw new AppException("Group could not be created.").SetStatusCode(500);
        return await GetAdminByIdAsync(created.Id);
    }

    public async Task<GroupResponse> UpdateAsync(Guid id, UpdateGroupRequest request)
    {
        _ = await GetAdminByIdAsync(id);
        string slug = BuildSlug(request.Slug, request.NameEn, request.NameBg);
        if (await groupRepository.SlugExistsAsync(slug, id))
            throw new AppException("A group with this slug already exists.").SetStatusCode(409);

        DanceGroup update = new()
        {
            Id = id,
            Slug = slug,
            NameBg = request.NameBg.Trim(),
            NameEn = request.NameEn.Trim(),
            DescriptionBg = request.DescriptionBg.Trim(),
            DescriptionEn = request.DescriptionEn.Trim(),
            Location = NormalizeOptional(request.Location),
            Active = request.Active,
            SortOrder = request.SortOrder
        };

        DanceGroup? saved = await groupRepository.UpdateWithSchedulesAsync(
            update,
            BuildSchedules(request.Schedules));

        if (saved is null) throw new AppException("Group not found.").SetStatusCode(404);
        return Map(saved);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _ = await GetAdminByIdAsync(id);
        return await groupRepository.DeleteAsync(id);
    }

    private static List<DanceGroupSchedule> BuildSchedules(IEnumerable<GroupScheduleRequest> schedules)
    {
        List<DanceGroupSchedule> result = [];
        HashSet<string> unique = [];

        foreach (GroupScheduleRequest schedule in schedules)
        {
            if (!TimeOnly.TryParseExact(
                    schedule.StartTime,
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out TimeOnly startTime))
            {
                throw new AppException("Invalid group start time.").SetStatusCode(400);
            }

            string formattedTime = startTime.ToString("HH:mm", CultureInfo.InvariantCulture);\n            string key = $"{schedule.DayOfWeek}:{formattedTime}";
            if (!unique.Add(key))
                throw new AppException("Duplicate group schedule.").SetStatusCode(400);

            result.Add(new DanceGroupSchedule
            {
                DayOfWeek = (DayOfWeek)schedule.DayOfWeek,
                StartTime = startTime,
                DurationMinutes = schedule.DurationMinutes
            });
        }

        if (result.Count == 0)
            throw new AppException("A group must have at least one weekly schedule.").SetStatusCode(400);

        return result;
    }

    private static GroupResponse Map(DanceGroup group) => new()
    {
        Id = group.Id,
        Slug = group.Slug,
        NameBg = group.NameBg,
        NameEn = group.NameEn,
        DescriptionBg = group.DescriptionBg,
        DescriptionEn = group.DescriptionEn,
        Location = group.Location,
        Active = group.Active,
        SortOrder = group.SortOrder,
        Schedules = group.Schedules
            .Where(item => !item.IsDeleted)
            .OrderBy(item => ((int)item.DayOfWeek + 6) % 7)
            .ThenBy(item => item.StartTime)
            .Select(item => new GroupScheduleResponse
            {
                Id = item.Id,
                DayOfWeek = (int)item.DayOfWeek,
                StartTime = item.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                DurationMinutes = item.DurationMinutes
            })
            .ToArray(),
        CreatedOn = group.CreatedOn,
        ModifiedOn = group.ModifiedOn
    };

    private static string BuildSlug(string? requestedSlug, string nameEn, string nameBg)
    {
        string source = !string.IsNullOrWhiteSpace(requestedSlug)
            ? requestedSlug
            : !string.IsNullOrWhiteSpace(nameEn) ? nameEn : nameBg;

        string slug = NormalizeSlug(source);
        if (string.IsNullOrWhiteSpace(slug))
            throw new AppException("A valid slug could not be generated.").SetStatusCode(400);
        return slug;
    }

    private static string NormalizeSlug(string value)
    {
        string lower = value.Trim().ToLowerInvariant();
        StringBuilder transliterated = new();

        foreach (char character in lower)
            transliterated.Append(BulgarianTransliteration.TryGetValue(character, out string? replacement) ? replacement : character);

        string normalized = transliterated.ToString().Normalize(NormalizationForm.FormD);
        StringBuilder ascii = new();
        foreach (char character in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                ascii.Append(character);

        string slug = Regex.Replace(ascii.ToString(), "[^a-z0-9]+", "-");
        return Regex.Replace(slug, "-{2,}", "-").Trim('-');
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
