using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class GalleryMediaConfiguration : IEntityTypeConfiguration<GalleryMedia>
{
    public void Configure(EntityTypeBuilder<GalleryMedia> builder)
    {
        builder.Property(item => item.CaptionBg)
            .HasMaxLength(500);

        builder.Property(item => item.CaptionEn)
            .HasMaxLength(500);

        builder.HasIndex(item => new { item.GalleryAlbumId, item.SortOrder });
        builder.HasIndex(item => new { item.GalleryAlbumId, item.MediaId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasOne(item => item.Media)
            .WithMany()
            .HasForeignKey(item => item.MediaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
