using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserRoleAssignmentConfiguration : IEntityTypeConfiguration<UserRoleAssignment>
{
    public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
    {
        builder.ToTable("UserRoleAssignments");
        builder.Property(entity => entity.UserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.Role).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Source).HasConversion<string>().HasMaxLength(20);
        builder.Property(entity => entity.ProviderKey).HasMaxLength(100);

        builder.HasIndex(entity => new { entity.UserId, entity.Role, entity.Source }).IsUnique();
        builder.HasIndex(entity => entity.UserId);
    }
}
