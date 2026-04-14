using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Persistence.Configurations.Identity;

public sealed class SsoGroupRoleMappingConfiguration : IEntityTypeConfiguration<SsoGroupRoleMapping>
{
    public void Configure(EntityTypeBuilder<SsoGroupRoleMapping> builder)
    {
        builder.ToTable("SsoGroupRoleMappings");
        builder.Property(entity => entity.ProviderKey).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.ExternalGroupId).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.ExternalGroupDisplayName).HasMaxLength(256).IsRequired();
        builder.Property(entity => entity.WombatRole).HasMaxLength(64).IsRequired();

        builder.HasIndex(entity => new { entity.ProviderKey, entity.ExternalGroupId, entity.WombatRole }).IsUnique();
    }
}
