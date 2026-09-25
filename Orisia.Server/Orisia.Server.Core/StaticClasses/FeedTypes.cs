using Orisia.Server.Core.Enums;

namespace Orisia.Server.Core.StaticClasses;

public static class FeedTypes
{
    public const string News = "news";
    public const string Report = "report";
    public const string Photos = "photos";
    public const string Blog = "blog";
    public const string Group = "group";
    public const string Schedule = "schedule";
    public const string Event = "event";

    public static readonly IReadOnlyCollection<string> All =
    [
        News,
        Report,
        Photos,
        Blog,
        Group,
        Schedule,
        Event
    ];

    public static bool IsValid(string? type)
    {
        return type is null || All.Contains(type, StringComparer.OrdinalIgnoreCase);
    }

    public static PostType? ToPostType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type) || type.Equals(Event, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return type.Trim().ToLowerInvariant() switch
        {
            News => PostType.News,
            Report => PostType.Report,
            Photos => PostType.Photos,
            Blog => PostType.Blog,
            Group => PostType.Group,
            Schedule => PostType.Schedule,
            _ => null
        };
    }

    public static string FromPostType(PostType type)
    {
        return type switch
        {
            PostType.News => News,
            PostType.Report => Report,
            PostType.Photos => Photos,
            PostType.Blog => Blog,
            PostType.Group => Group,
            PostType.Schedule => Schedule,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
