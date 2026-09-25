using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.Property(item => item.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(item => item.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(item => item.ThumbnailStorageKey)
            .HasMaxLength(500);

        builder.Property(item => item.MimeType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.Extension)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(item => item.Sha256)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(item => item.AltBg)
            .HasMaxLength(300);

        builder.Property(item => item.AltEn)
            .HasMaxLength(300);

        builder.HasIndex(item => item.StorageKey).IsUnique();
        builder.HasIndex(item => item.Sha256);
        builder.HasIndex(item => item.CreatedOn);

        builder.HasOne(item => item.UploadedBy)
            .WithMany(user => user.MediaUploads)
            .HasForeignKey(item => item.UploadedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
