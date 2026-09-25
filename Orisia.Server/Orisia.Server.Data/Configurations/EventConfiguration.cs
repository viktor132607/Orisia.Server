using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
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

        builder.Property(item => item.Location)
            .HasMaxLength(300);

        builder.Property(item => item.RecurrenceRule)
            .HasMaxLength(500);

        builder.HasIndex(item => item.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(item => new { item.Status, item.StartAt });
        builder.HasIndex(item => new { item.EventType, item.Status, item.StartAt });
        builder.HasIndex(item => new { item.Featured, item.Status, item.StartAt });
    }
}
