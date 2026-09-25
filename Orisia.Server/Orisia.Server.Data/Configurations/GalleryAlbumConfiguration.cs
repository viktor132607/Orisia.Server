using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class GalleryAlbumConfiguration : IEntityTypeConfiguration<GalleryAlbum>
{
    public void Configure(EntityTypeBuilder<GalleryAlbum> builder)
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
            .HasMaxLength(1000);

        builder.Property(item => item.DescriptionEn)
            .HasMaxLength(1000);

        builder.HasIndex(item => item.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(item => new { item.Active, item.Featured, item.SortOrder });

        builder.HasOne(item => item.CoverMedia)
            .WithMany()
            .HasForeignKey(item => item.CoverMediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(item => item.Items)
            .WithOne(item => item.GalleryAlbum)
            .HasForeignKey(item => item.GalleryAlbumId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
