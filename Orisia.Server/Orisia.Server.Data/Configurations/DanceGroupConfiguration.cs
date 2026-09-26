using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class DanceGroupConfiguration : IEntityTypeConfiguration<DanceGroup>
{
    public void Configure(EntityTypeBuilder<DanceGroup> builder)
    {
        builder.Property(item => item.Slug).HasMaxLength(180).IsRequired();
        builder.Property(item => item.NameBg).HasMaxLength(250).IsRequired();
        builder.Property(item => item.NameEn).HasMaxLength(250).IsRequired();
        builder.Property(item => item.DescriptionBg).IsRequired();
        builder.Property(item => item.DescriptionEn).IsRequired();
        builder.Property(item => item.Location).HasMaxLength(300);

        builder.HasIndex(item => item.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(item => new { item.Active, item.SortOrder });

        builder.HasMany(item => item.Schedules)
            .WithOne(item => item.DanceGroup)
            .HasForeignKey(item => item.DanceGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
