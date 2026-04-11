using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Epas;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class EntrustmentScaleConfiguration : IEntityTypeConfiguration<EntrustmentScale>
{
    public void Configure(EntityTypeBuilder<EntrustmentScale> builder)
    {
        builder.ToTable("EntrustmentScales");
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(2000);
        builder.HasIndex(entity => entity.Name).IsUnique();

        builder.HasMany(entity => entity.Levels)
            .WithOne(entity => entity.Scale)
            .HasForeignKey(entity => entity.ScaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
