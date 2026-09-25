using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(user => new { user.IsActive, user.Role });
    }
}
