using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class ContactInquiryConfiguration : IEntityTypeConfiguration<ContactInquiry>
{
    public void Configure(EntityTypeBuilder<ContactInquiry> builder)
    {
        builder.Property(item => item.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(item => item.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(item => item.Phone)
            .HasMaxLength(50);

        builder.Property(item => item.Subject)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(item => item.Message)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(item => item.AnswerText)
            .HasMaxLength(5000);

        builder.HasIndex(item => new { item.Status, item.CreatedOn });
        builder.HasIndex(item => item.Email);
        builder.HasIndex(item => item.AnsweredById);

        builder.HasOne(item => item.AnsweredBy)
            .WithMany()
            .HasForeignKey(item => item.AnsweredById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
