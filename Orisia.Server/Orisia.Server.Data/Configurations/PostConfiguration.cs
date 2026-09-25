using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.Property(post => post.Slug)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(post => post.TitleBg)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(post => post.TitleEn)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(post => post.ExcerptBg)
            .HasMaxLength(500);

        builder.Property(post => post.ExcerptEn)
            .HasMaxLength(500);

        builder.Property(post => post.SeoTitleBg)
            .HasMaxLength(70);

        builder.Property(post => post.SeoTitleEn)
            .HasMaxLength(70);

        builder.Property(post => post.SeoDescriptionBg)
            .HasMaxLength(180);

        builder.Property(post => post.SeoDescriptionEn)
            .HasMaxLength(180);

        builder.HasIndex(post => post.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(post => new { post.Type, post.Status, post.PublishedAt });
        builder.HasIndex(post => new { post.Featured, post.Status });

        builder.HasOne(post => post.Author)
            .WithMany(user => user.Posts)
            .HasForeignKey(post => post.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(post => post.CoverMedia)
            .WithMany()
            .HasForeignKey(post => post.CoverMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
