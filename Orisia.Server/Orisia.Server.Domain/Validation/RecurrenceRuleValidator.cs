using System.Globalization;
using Orisia.Server.Core.Exceptions;

namespace Orisia.Server.Domain.Validation;

public static class RecurrenceRuleValidator
{
    private static readonly HashSet<string> AllowedKeys =
    [
        "FREQ", "INTERVAL", "COUNT", "UNTIL", "BYDAY", "BYMONTHDAY", "BYMONTH"
    ];

    private static readonly HashSet<string> Frequencies =
    [
        "DAILY", "WEEKLY", "MONTHLY", "YEARLY"
    ];

    private static readonly HashSet<string> WeekDays =
    [
        "MO", "TU", "WE", "TH", "FR", "SA", "SU"
    ];

    public static string? NormalizeAndValidate(string? recurrenceRule)
    {
        if (string.IsNullOrWhiteSpace(recurrenceRule))
        {
            return null;
        }

        string normalized = recurrenceRule.Trim().ToUpperInvariant();
        if (normalized.StartsWith("RRULE:", StringComparison.Ordinal))
        {
            normalized = normalized["RRULE:".Length..];
        }

        Dictionary<string, string> parts = Parse(normalized);

        if (!parts.TryGetValue("FREQ", out string? frequency) || !Frequencies.Contains(frequency))
        {
            throw Invalid("FREQ is required and must be DAILY, WEEKLY, MONTHLY or YEARLY.");
        }

        ValidatePositiveInteger(parts, "INTERVAL");
        ValidatePositiveInteger(parts, "COUNT");

        if (parts.ContainsKey("COUNT") && parts.ContainsKey("UNTIL"))
        {
            throw Invalid("COUNT and UNTIL cannot be used together.");
        }

        if (parts.TryGetValue("UNTIL", out string? until))
        {
            bool validUntil =
                DateTime.TryParseExact(
                    until,
                    ["yyyyMMdd'T'HHmmss'Z'", "yyyyMMdd"],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out _);

            if (!validUntil)
            {
                throw Invalid("UNTIL must use yyyyMMdd or yyyyMMddTHHmmssZ format.");
            }
        }

        if (parts.TryGetValue("BYDAY", out string? byDay))
        {
            string[] days = byDay.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (days.Length == 0 || days.Any(day => !WeekDays.Contains(day)))
            {
                throw Invalid("BYDAY contains an unsupported weekday.");
            }
        }

        if (parts.TryGetValue("BYMONTHDAY", out string? byMonthDay))
        {
            ValidateIntegerList(
                byMonthDay,
                value => value is >= -31 and <= 31 && value != 0,
                "BYMONTHDAY values must be between -31 and 31, excluding 0.");
        }

        if (parts.TryGetValue("BYMONTH", out string? byMonth))
        {
            ValidateIntegerList(
                byMonth,
                value => value is >= 1 and <= 12,
                "BYMONTH values must be between 1 and 12.");
        }

        return string.Join(
            ';',
            parts.Select(pair => $"{pair.Key}={pair.Value}"));
    }

    private static Dictionary<string, string> Parse(string rule)
    {
        Dictionary<string, string> parts = new(StringComparer.Ordinal);

        foreach (string segment in rule.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] pair = segment.Split('=', 2, StringSplitOptions.TrimEntries);
            if (pair.Length != 2 || string.IsNullOrWhiteSpace(pair[0]) || string.IsNullOrWhiteSpace(pair[1]))
            {
                throw Invalid("Recurrence rule contains an invalid segment.");
            }

            string key = pair[0];
            string value = pair[1];

            if (!AllowedKeys.Contains(key))
            {
                throw Invalid($"Unsupported recurrence key: {key}.");
            }

            if (!parts.TryAdd(key, value))
            {
                throw Invalid($"Duplicate recurrence key: {key}.");
            }
        }

        return parts;
    }

    private static void ValidatePositiveInteger(
        IReadOnlyDictionary<string, string> parts,
        string key)
    {
        if (!parts.TryGetValue(key, out string? raw))
        {
            return;
        }

        if (!int.TryParse(raw, out int value) || value <= 0)
        {
            throw Invalid($"{key} must be a positive integer.");
        }
    }

    private static void ValidateIntegerList(
        string raw,
        Func<int, bool> predicate,
        string error)
    {
        string[] values = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (values.Length == 0
            || values.Any(value => !int.TryParse(value, out int parsed) || !predicate(parsed)))
        {
            throw Invalid(error);
        }
    }

    private static AppException Invalid(string message)
    {
        return new AppException($"Invalid recurrence rule. {message}").SetStatusCode(400);
    }
}
