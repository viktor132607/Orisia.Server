using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class DanceConfiguration : IEntityTypeConfiguration<Dance>
{
    public void Configure(EntityTypeBuilder<Dance> builder)
    {
        builder.Property(item => item.Slug)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(item => item.TitleBg)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(item => item.TitleEn)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(item => item.DescriptionBg)
            .IsRequired();

        builder.Property(item => item.DescriptionEn)
            .IsRequired();

        builder.Property(item => item.Region)
            .HasMaxLength(120);

        builder.Property(item => item.Rhythm)
            .HasMaxLength(120);

        builder.Property(item => item.VideoUrl)
            .HasMaxLength(1000);

        builder.HasIndex(item => item.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(item => new { item.Active, item.SortOrder });
        builder.HasIndex(item => item.Region);

        builder.HasOne(item => item.ThumbnailMedia)
            .WithMany()
            .HasForeignKey(item => item.ThumbnailMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
