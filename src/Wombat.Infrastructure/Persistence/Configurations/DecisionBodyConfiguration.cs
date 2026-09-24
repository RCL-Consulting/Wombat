using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Curricula;

namespace Wombat.Infrastructure.Persistence.Configurations;

/// <summary>
/// The College's decision bodies (T131, Decision 2). Keyed by <c>Key</c>, which is what <c>CurriculumItems.DecisionBodyKey</c>
/// references with a restricting foreign key.
/// </summary>
/// <remarks>
/// Unlike <c>WbaTools</c>, this vocabulary can carry a foreign key: the T131 migration inserts the row it stamps items with
/// before it stamps them, and <c>PaediatricCatalogueSeeder</c> inserts the vocabulary before it creates any item.
/// </remarks>
public sealed class DecisionBodyConfiguration : IEntityTypeConfiguration<DecisionBody>
{
    public void Configure(EntityTypeBuilder<DecisionBody> builder)
    {
        builder.ToTable("DecisionBodies");
        builder.HasKey(entity => entity.Key);
        builder.Property(entity => entity.Key).HasMaxLength(DecisionBody.KeyMaxLength);
        builder.Property(entity => entity.Name).HasMaxLength(200).IsRequired();
    }
}
