using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class CommitteeAppealConfiguration : IEntityTypeConfiguration<CommitteeAppeal>
{
    public void Configure(EntityTypeBuilder<CommitteeAppeal> builder)
    {
        // An open appeal has no outcome; a resolved one was Dismissed (2) or Remitted (3). 1 was Upheld, which did what
        // Dismissed does under a name that says the appeal succeeded; T307 rewrote the stored ones as Dismissed and removed
        // it (D51), and the domain and the validator refuse it.
        builder.ToTable("CommitteeAppeals", table => table.HasCheckConstraint(
            "CK_CommitteeAppeals_Outcome", "\"Outcome\" IS NULL OR \"Outcome\" IN (2, 3)"));
        builder.Property(entity => entity.LodgedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.ResolvedByUserId).HasMaxLength(450);
        builder.Property(entity => entity.Reason).HasMaxLength(4000).IsRequired();
        builder.HasIndex(entity => new { entity.ReviewId, entity.LodgedOn });
    }
}
