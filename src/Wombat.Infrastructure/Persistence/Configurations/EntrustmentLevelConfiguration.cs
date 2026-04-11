using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Epas;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class EntrustmentLevelConfiguration : IEntityTypeConfiguration<EntrustmentLevel>
{
    public void Configure(EntityTypeBuilder<EntrustmentLevel> builder)
    {
        builder.ToTable("EntrustmentLevels");
        builder.Property(entity => entity.Label).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(2000);
        builder.HasIndex(entity => new { entity.ScaleId, entity.Order }).IsUnique();
    }
}
