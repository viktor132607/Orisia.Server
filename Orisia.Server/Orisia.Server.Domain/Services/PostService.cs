using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Orisia.Server.Common.Requests.Posts;
using Orisia.Server.Common.Responses.Posts;
using Orisia.Server.Core.Enums;
using Orisia.Server.Core.Exceptions;
using Orisia.Server.Data.Entities;
using Orisia.Server.Data.Interfaces;
using Orisia.Server.Domain.Interfaces;

namespace Orisia.Server.Domain.Services;

public class PostService(
    IPostRepository postRepository,
    IAuthService authService) : IPostService
{
    private static readonly Dictionary<char, string> BulgarianTransliteration = new()
    {
        ['а']="a", ['б']="b", ['в']="v", ['г']="g", ['д']="d", ['е']="e", ['ж']="zh",
        ['з']="z", ['и']="i", ['й']="y", ['к']="k", ['л']="l", ['м']="m", ['н']="n",
        ['о']="o", ['п']="p", ['р']="r", ['с']="s", ['т']="t", ['у']="u", ['ф']="f",
        ['х']="h", ['ц']="ts", ['ч']="ch", ['ш']="sh", ['щ']="sht", ['ъ']="a", ['ь']="y",
        ['ю']="yu", ['я']="ya"
    };

    public async Task<IEnumerable<PostResponse>> GetPublishedAsync(
        PostType? type = null,
        bool? featured = null,
        int? take = null)
    {
        if (take is <= 0)
        {
            throw new AppException("Take must be greater than zero.").SetStatusCode(400);
        }

        IEnumerable<Post> posts = await postRepository.GetPublishedAsync(type, featured, take);
        return posts.Select(Map);
    }

    public async Task<PostResponse> GetPublishedBySlugAsync(string slug)
    {
        Post? post = await postRepository.GetBySlugAsync(NormalizeSlug(slug));

        if (post is null
            || post.Status != PublicationStatus.Published
            || !post.PublishedAt.HasValue
            || post.PublishedAt.Value > DateTime.UtcNow)
        {
            throw new AppException("Post not found.").SetStatusCode(404);
        }

        return Map(post);
    }

    public async Task<IEnumerable<PostResponse>> GetAdminAsync(
        PostType? type = null,
        PublicationStatus? status = null)
    {
        IEnumerable<Post> posts = await postRepository.GetForAdminAsync(type, status);
        return posts.Select(Map);
    }

    public async Task<PostResponse> GetAdminByIdAsync(Guid id)
    {
        Post? post = await postRepository.GetByIdWithAuthorAsync(id);
        if (post is null)
        {
            throw new AppException("Post not found.").SetStatusCode(404);
        }

        return Map(post);
    }

    public async Task<PostResponse> CreateAsync(CreatePostRequest request)
    {
        Guid authorId = await GetCurrentUserIdAsync();
        string slug = BuildSlug(request.Slug, request.TitleEn, request.TitleBg);

        if (await postRepository.SlugExistsAsync(slug))
        {
            throw new AppException("A post with this slug already exists.").SetStatusCode(409);
        }

        Post post = new()
        {
            Slug = slug,
            Type = request.Type,
            Status = PublicationStatus.Draft,
            TitleBg = request.TitleBg.Trim(),
            TitleEn = request.TitleEn.Trim(),
            BodyBg = request.BodyBg.Trim(),
            BodyEn = request.BodyEn.Trim(),
            ExcerptBg = NormalizeOptional(request.ExcerptBg),
            ExcerptEn = NormalizeOptional(request.ExcerptEn),
            CoverMediaId = request.CoverMediaId,
            Featured = request.Featured,
            AuthorId = authorId
        };

        Post? created = await postRepository.AddAsync(post);
        if (created is null)
        {
            throw new AppException("Post could not be created.").SetStatusCode(500);
        }

        return await GetAdminByIdAsync(created.Id);
    }

    public async Task<PostResponse> UpdateAsync(Guid id, UpdatePostRequest request)
    {
        Post? existing = await postRepository.GetByIdAsync(id);
        if (existing is null)
        {
            throw new AppException("Post not found.").SetStatusCode(404);
        }

        string slug = BuildSlug(request.Slug, request.TitleEn, request.TitleBg);
        if (await postRepository.SlugExistsAsync(slug, id))
        {
            throw new AppException("A post with this slug already exists.").SetStatusCode(409);
        }

        Post updated = new()
        {
            Id = existing.Id,
            Slug = slug,
            Type = request.Type,
            Status = existing.Status,
            TitleBg = request.TitleBg.Trim(),
            TitleEn = request.TitleEn.Trim(),
            BodyBg = request.BodyBg.Trim(),
            BodyEn = request.BodyEn.Trim(),
            ExcerptBg = NormalizeOptional(request.ExcerptBg),
            ExcerptEn = NormalizeOptional(request.ExcerptEn),
            CoverMediaId = request.CoverMediaId,
            Featured = request.Featured,
            PublishedAt = existing.PublishedAt,
            AuthorId = existing.AuthorId
        };

        Post? saved = await postRepository.UpdateAsync(updated);
        if (saved is null)
        {
            throw new AppException("Post not found.").SetStatusCode(404);
        }

        return await GetAdminByIdAsync(saved.Id);
    }

    public Task<PostResponse> PublishAsync(Guid id)
    {
        return ChangeStatusAsync(id, PublicationStatus.Published);
    }

    public Task<PostResponse> ArchiveAsync(Guid id)
    {
        return ChangeStatusAsync(id, PublicationStatus.Archived);
    }

    public async Task<PostResponse> SetFeaturedAsync(Guid id, bool featured)
    {
        Post? post = await postRepository.GetByIdAsync(id);
        if (post is null)
        {
            throw new AppException("Post not found.").SetStatusCode(404);
        }

        Post updated = CloneForUpdate(post);
        updated.Featured = featured;

        Post? saved = await postRepository.UpdateAsync(updated);
        if (saved is null)
        {
            throw new AppException("Post not found.").SetStatusCode(404);
        }

        return await GetAdminByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _ = await GetAdminByIdAsync(id);
        return await postRepository.DeleteAsync(id);
    }

    private async Task<PostResponse> ChangeStatusAsync(Guid id, PublicationStatus status)
    {
        Post? post = await postRepository.GetByIdAsync(id);
        if (post is null)
        {
            throw new AppException("Post not found.").SetStatusCode(404);
        }

        Post updated = CloneForUpdate(post);
        updated.Status = status;
        updated.PublishedAt = status == PublicationStatus.Published
            ? post.PublishedAt ?? DateTime.UtcNow
            : null;

        Post? saved = await postRepository.UpdateAsync(updated);
        if (saved is null)
        {
            throw new AppException("Post not found.").SetStatusCode(404);
        }

        return await GetAdminByIdAsync(id);
    }

    private async Task<Guid> GetCurrentUserIdAsync()
    {
        string? currentUserId = await authService.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId) || !Guid.TryParse(currentUserId, out Guid authorId))
        {
            throw new AppException("Unauthorized.").SetStatusCode(401);
        }

        return authorId;
    }

    private static Post CloneForUpdate(Post post)
    {
        return new Post
        {
            Id = post.Id,
            Slug = post.Slug,
            Type = post.Type,
            Status = post.Status,
            TitleBg = post.TitleBg,
            TitleEn = post.TitleEn,
            BodyBg = post.BodyBg,
            BodyEn = post.BodyEn,
            ExcerptBg = post.ExcerptBg,
            ExcerptEn = post.ExcerptEn,
            CoverMediaId = post.CoverMediaId,
            Featured = post.Featured,
            PublishedAt = post.PublishedAt,
            AuthorId = post.AuthorId
        };
    }

    private static PostResponse Map(Post post)
    {
        return new PostResponse
        {
            Id = post.Id,
            Slug = post.Slug,
            Type = post.Type,
            Status = post.Status,
            TitleBg = post.TitleBg,
            TitleEn = post.TitleEn,
            BodyBg = post.BodyBg,
            BodyEn = post.BodyEn,
            ExcerptBg = post.ExcerptBg,
            ExcerptEn = post.ExcerptEn,
            CoverMediaId = post.CoverMediaId,
            Featured = post.Featured,
            PublishedAt = post.PublishedAt,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.Names,
            CreatedOn = post.CreatedOn,
            ModifiedOn = post.ModifiedOn
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

        string normalized = transliterated
            .ToString()
            .Normalize(NormalizationForm.FormD);

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
