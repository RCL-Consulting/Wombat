using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Forms;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class FormCriterionConfiguration : IEntityTypeConfiguration<FormCriterion>
{
    public void Configure(EntityTypeBuilder<FormCriterion> builder)
    {
        builder.ToTable("FormCriteria");
        builder.Property(entity => entity.Prompt).HasMaxLength(500).IsRequired();
        builder.Property(entity => entity.HelpText).HasMaxLength(2000);
        builder.HasIndex(entity => new { entity.FormId, entity.Order }).IsUnique();
    }
}
