using System.Globalization;
using Orisia.Server.Data.Entities;
using Orisia.Server.Domain.Validation;

namespace Orisia.Server.Domain.Calendar;

public static class RecurrenceExpander
{
    private const int MaxGeneratedOccurrences = 10000;
    private const int MaxScannedDays = 36600;

    public static IReadOnlyList<EventOccurrence> Expand(Event item, DateTime fromUtc, DateTime toUtc)
    {
        if (fromUtc > toUtc)
        {
            return Array.Empty<EventOccurrence>();
        }

        if (string.IsNullOrWhiteSpace(item.RecurrenceRule))
        {
            return Overlaps(item.StartAt, item.EndAt, fromUtc, toUtc)
                ? [new EventOccurrence(item.StartAt, item.EndAt)]
                : Array.Empty<EventOccurrence>();
        }

        string normalized = RecurrenceRuleValidator.NormalizeAndValidate(item.RecurrenceRule)!;
        Dictionary<string, string> parts = Parse(normalized);

        string frequency = parts["FREQ"];
        int interval = ReadPositiveInt(parts, "INTERVAL") ?? 1;
        int? count = ReadPositiveInt(parts, "COUNT");
        DateTime? until = ReadUntil(parts);
        HashSet<DayOfWeek>? byDays = ReadByDays(parts);
        HashSet<int>? byMonthDays = ReadIntSet(parts, "BYMONTHDAY");
        HashSet<int>? byMonths = ReadIntSet(parts, "BYMONTH");

        TimeSpan? duration = item.EndAt.HasValue
            ? item.EndAt.Value - item.StartAt
            : null;

        DateTime startDate = item.StartAt.Date;
        DateTime finalDate = toUtc.Date;

        if (until.HasValue && until.Value < finalDate)
        {
            finalDate = until.Value.Date;
        }

        int scannedDays = (int)Math.Min(
            (finalDate - startDate).TotalDays,
            MaxScannedDays);

        if (scannedDays < 0)
        {
            return Array.Empty<EventOccurrence>();
        }

        List<EventOccurrence> result = [];
        int occurrenceNumber = 0;

        for (int dayOffset = 0; dayOffset <= scannedDays; dayOffset++)
        {
            DateTime candidateDate = startDate.AddDays(dayOffset);
            DateTime candidate = DateTime.SpecifyKind(
                candidateDate.Add(item.StartAt.TimeOfDay),
                DateTimeKind.Utc);

            if (candidate < item.StartAt)
            {
                continue;
            }

            if (until.HasValue && candidate > until.Value)
            {
                break;
            }

            if (!MatchesFrequency(
                    frequency,
                    interval,
                    item.StartAt,
                    candidate,
                    byDays,
                    byMonthDays,
                    byMonths))
            {
                continue;
            }

            occurrenceNumber++;
            if (count.HasValue && occurrenceNumber > count.Value)
            {
                break;
            }

            DateTime? occurrenceEnd = duration.HasValue
                ? candidate.Add(duration.Value)
                : null;

            if (!Overlaps(candidate, occurrenceEnd, fromUtc, toUtc))
            {
                continue;
            }

            result.Add(new EventOccurrence(candidate, occurrenceEnd));

            if (result.Count >= MaxGeneratedOccurrences)
            {
                break;
            }
        }

        return result;
    }

    private static bool MatchesFrequency(
        string frequency,
        int interval,
        DateTime start,
        DateTime candidate,
        HashSet<DayOfWeek>? byDays,
        HashSet<int>? byMonthDays,
        HashSet<int>? byMonths)
    {
        if (byMonths is not null && !byMonths.Contains(candidate.Month))
        {
            return false;
        }

        if (byMonthDays is not null && !MatchesMonthDay(candidate, byMonthDays))
        {
            return false;
        }

        if (byDays is not null && !byDays.Contains(candidate.DayOfWeek))
        {
            return false;
        }

        return frequency switch
        {
            "DAILY" => MatchesDaily(start, candidate, interval),
            "WEEKLY" => MatchesWeekly(start, candidate, interval, byDays),
            "MONTHLY" => MatchesMonthly(start, candidate, interval, byDays, byMonthDays),
            "YEARLY" => MatchesYearly(start, candidate, interval, byDays, byMonthDays, byMonths),
            _ => false
        };
    }

    private static bool MatchesDaily(DateTime start, DateTime candidate, int interval)
    {
        int days = (candidate.Date - start.Date).Days;
        return days >= 0 && days % interval == 0;
    }

    private static bool MatchesWeekly(
        DateTime start,
        DateTime candidate,
        int interval,
        HashSet<DayOfWeek>? byDays)
    {
        DateTime startWeek = StartOfWeek(start.Date);
        DateTime candidateWeek = StartOfWeek(candidate.Date);
        int weeks = (candidateWeek - startWeek).Days / 7;

        if (weeks < 0 || weeks % interval != 0)
        {
            return false;
        }

        return byDays is not null
            ? byDays.Contains(candidate.DayOfWeek)
            : candidate.DayOfWeek == start.DayOfWeek;
    }

    private static bool MatchesMonthly(
        DateTime start,
        DateTime candidate,
        int interval,
        HashSet<DayOfWeek>? byDays,
        HashSet<int>? byMonthDays)
    {
        int months = ((candidate.Year - start.Year) * 12) + candidate.Month - start.Month;
        if (months < 0 || months % interval != 0)
        {
            return false;
        }

        if (byDays is null && byMonthDays is null)
        {
            return candidate.Day == start.Day;
        }

        return true;
    }

    private static bool MatchesYearly(
        DateTime start,
        DateTime candidate,
        int interval,
        HashSet<DayOfWeek>? byDays,
        HashSet<int>? byMonthDays,
        HashSet<int>? byMonths)
    {
        int years = candidate.Year - start.Year;
        if (years < 0 || years % interval != 0)
        {
            return false;
        }

        if (byMonths is null && candidate.Month != start.Month)
        {
            return false;
        }

        if (byDays is null && byMonthDays is null && candidate.Day != start.Day)
        {
            return false;
        }

        return true;
    }

    private static bool MatchesMonthDay(DateTime candidate, HashSet<int> values)
    {
        int daysInMonth = DateTime.DaysInMonth(candidate.Year, candidate.Month);

        return values.Any(value =>
            value > 0
                ? candidate.Day == value
                : candidate.Day == daysInMonth + value + 1);
    }

    private static bool Overlaps(
        DateTime start,
        DateTime? end,
        DateTime from,
        DateTime to)
    {
        DateTime effectiveEnd = end ?? start;
        return effectiveEnd >= from && start <= to;
    }

    private static DateTime StartOfWeek(DateTime value)
    {
        int offset = ((int)value.DayOfWeek + 6) % 7;
        return value.AddDays(-offset);
    }

    private static Dictionary<string, string> Parse(string rule)
    {
        return rule
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment => segment.Split('=', 2, StringSplitOptions.TrimEntries))
            .ToDictionary(pair => pair[0], pair => pair[1], StringComparer.Ordinal);
    }

    private static int? ReadPositiveInt(
        IReadOnlyDictionary<string, string> parts,
        string key)
    {
        return parts.TryGetValue(key, out string? raw)
            ? int.Parse(raw, CultureInfo.InvariantCulture)
            : null;
    }

    private static DateTime? ReadUntil(IReadOnlyDictionary<string, string> parts)
    {
        if (!parts.TryGetValue("UNTIL", out string? raw))
        {
            return null;
        }

        if (DateTime.TryParseExact(
                raw,
                "yyyyMMdd'T'HHmmss'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTime timestamp))
        {
            return DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
        }

        DateTime date = DateTime.ParseExact(
            raw,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);

        return DateTime.SpecifyKind(
            date.Date.AddDays(1).AddTicks(-1),
            DateTimeKind.Utc);
    }

    private static HashSet<DayOfWeek>? ReadByDays(
        IReadOnlyDictionary<string, string> parts)
    {
        if (!parts.TryGetValue("BYDAY", out string? raw))
        {
            return null;
        }

        Dictionary<string, DayOfWeek> map = new()
        {
            ["MO"] = DayOfWeek.Monday,
            ["TU"] = DayOfWeek.Tuesday,
            ["WE"] = DayOfWeek.Wednesday,
            ["TH"] = DayOfWeek.Thursday,
            ["FR"] = DayOfWeek.Friday,
            ["SA"] = DayOfWeek.Saturday,
            ["SU"] = DayOfWeek.Sunday
        };

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => map[value])
            .ToHashSet();
    }

    private static HashSet<int>? ReadIntSet(
        IReadOnlyDictionary<string, string> parts,
        string key)
    {
        if (!parts.TryGetValue(key, out string? raw))
        {
            return null;
        }

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.Parse(value, CultureInfo.InvariantCulture))
            .ToHashSet();
    }
}
