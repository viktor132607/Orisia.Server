using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class SiteReviewConfiguration : IEntityTypeConfiguration<SiteReview>
{
    public void Configure(EntityTypeBuilder<SiteReview> builder)
    {
        builder.Property(item => item.AuthorName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.Content)
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasIndex(item => new { item.Status, item.Featured, item.CreatedOn });
        builder.HasIndex(item => item.UserId);

        builder.HasOne(item => item.User)
            .WithMany(user => user.SiteReviews)
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(item => item.ModeratedBy)
            .WithMany()
            .HasForeignKey(item => item.ModeratedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
