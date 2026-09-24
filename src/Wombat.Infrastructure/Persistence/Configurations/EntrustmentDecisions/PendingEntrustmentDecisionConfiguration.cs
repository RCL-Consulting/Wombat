using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.EntrustmentDecisions;

public sealed class PendingEntrustmentDecisionConfiguration : IEntityTypeConfiguration<PendingEntrustmentDecision>
{
    public void Configure(EntityTypeBuilder<PendingEntrustmentDecision> builder)
    {
        builder.ToTable("PendingEntrustmentDecisions");

        builder.Property(entity => entity.Rationale).HasMaxLength(4000).IsRequired();
        // T131: the ids of the snapshot lines the decision rests on, and nothing else; the ratify handler builds each
        // STAR's links from the rows themselves. Read through EvidenceItemIds, which is computed and not a column.
        builder.Property(entity => entity.EvidenceItemIdsJson).HasColumnType("jsonb").IsRequired();
        builder.Ignore(entity => entity.EvidenceItemIds);
        builder.Property(entity => entity.StagedByUserId).HasMaxLength(450).IsRequired();

        builder.Property(entity => entity.IssuedOn).HasColumnType("date");
        builder.Property(entity => entity.ExpiresOn).HasColumnType("date");

        builder.HasOne(entity => entity.Review)
            .WithMany()
            .HasForeignKey(entity => entity.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(entity => entity.Epa)
            .WithMany()
            .HasForeignKey(entity => entity.EpaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.AuthorisedLevel)
            .WithMany()
            .HasForeignKey(entity => entity.AuthorisedLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        // T131: one staged decision per EPA at a review. Ratifying issues one STAR per staged decision and supersedes the
        // trainee's active one on its EPA, so two would supersede each other in an order nobody chose. The staging handler
        // refuses a second by name; this holds when two chairs stage at once.
        builder.HasIndex(entity => new { entity.ReviewId, entity.EpaId }).IsUnique();
    }
}
