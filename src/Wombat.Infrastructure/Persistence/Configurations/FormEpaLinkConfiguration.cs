using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Forms;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class FormEpaLinkConfiguration : IEntityTypeConfiguration<FormEpaLink>
{
    public void Configure(EntityTypeBuilder<FormEpaLink> builder)
    {
        builder.ToTable("FormEpaLinks");
        builder.HasIndex(entity => new { entity.FormId, entity.EpaId }).IsUnique();

        builder.HasOne(entity => entity.Epa)
            .WithMany(entity => entity.FormLinks)
            .HasForeignKey(entity => entity.EpaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
