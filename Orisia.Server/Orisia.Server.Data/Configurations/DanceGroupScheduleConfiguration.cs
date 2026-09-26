using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class DanceGroupScheduleConfiguration : IEntityTypeConfiguration<DanceGroupSchedule>
{
    public void Configure(EntityTypeBuilder<DanceGroupSchedule> builder)
    {
        builder.Property(item => item.StartTime).HasColumnType("time without time zone");

        builder.HasIndex(item => new { item.DanceGroupId, item.DayOfWeek, item.StartTime })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
