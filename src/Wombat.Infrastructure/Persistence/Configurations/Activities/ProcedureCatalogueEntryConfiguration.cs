using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Activities;

namespace Wombat.Infrastructure.Persistence.Configurations.Activities;

public sealed class ProcedureCatalogueEntryConfiguration : IEntityTypeConfiguration<ProcedureCatalogueEntry>
{
    public void Configure(EntityTypeBuilder<ProcedureCatalogueEntry> builder)
    {
        builder.ToTable("ProcedureCatalogueEntries");
        builder.Property(entity => entity.Key).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Category).HasMaxLength(100).IsRequired();

        builder.HasIndex(entity => entity.Key).IsUnique();
        builder.HasIndex(entity => new { entity.Category, entity.Name });
    }
}
