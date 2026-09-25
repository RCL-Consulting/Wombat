using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

public sealed class CommitteeEvidenceConfiguration : IEntityTypeConfiguration<CommitteeEvidence>
{
    public void Configure(EntityTypeBuilder<CommitteeEvidence> builder)
    {
        builder.ToTable("CommitteeEvidenceItems");
        builder.Property(entity => entity.SourceLabel).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.Summary).HasMaxLength(4000).IsRequired();
        builder.HasIndex(entity => new { entity.ReviewId, entity.SourceType });

        // T167: what each line is about, frozen at Start. Lengths follow the columns they are copied from (Epas.Code and
        // Title, ActivityTypes.WbaToolKey, WbaTools.Name, EntrustmentLevels.Label, Activities.CurrentState). No foreign
        // key on EpaId, for the reason ActivityId has none: the snapshot must outlive, and never block, a change to the
        // catalogue.
        builder.Property(entity => entity.EpaCode).HasMaxLength(64);
        builder.Property(entity => entity.EpaTitle).HasMaxLength(200);
        builder.Property(entity => entity.InstrumentKey).HasMaxLength(64);
        builder.Property(entity => entity.InstrumentName).HasMaxLength(200);
        builder.Property(entity => entity.RatingLabel).HasMaxLength(200);
        builder.Property(entity => entity.SourceState).HasMaxLength(100);

        // T220: the state's label, frozen with it. Unbounded, because it is copied from a workflow's jsonb, where a
        // builder may write a label of any length, and a long label must not stop a review from starting.
        builder.Property(entity => entity.SourceStateLabel);

        // T165: the assessor the line's version names, frozen at Start. A user id, like every other in the schema.
        builder.Property(entity => entity.AssessorUserId).HasMaxLength(450);
    }
}
