using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Epas;

namespace Wombat.Infrastructure.Persistence.Configurations;

/// <summary>
/// The College's instrument vocabulary (T122). Referenced by key from <c>ActivityTypes.WbaToolKey</c> and from inside
/// <c>CurriculumItems.PermittedToolsJson</c>, with no foreign key from either: the rows are inserted by
/// <c>PaediatricCatalogueSeeder</c> after migrations run, and a jsonb array cannot carry one anyway.
/// </summary>
public sealed class WbaToolConfiguration : IEntityTypeConfiguration<WbaTool>
{
    public void Configure(EntityTypeBuilder<WbaTool> builder)
    {
        builder.ToTable("WbaTools");
        builder.Property(entity => entity.Key).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(2000);
        builder.HasIndex(entity => entity.Key).IsUnique();
    }
}
