using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Infrastructure.Persistence.Configurations.CommitteeDecisions;

/// <summary>
/// Who was present when one committee decision was taken (T165). One row per member per decision. No foreign key to the
/// panel's members: the attendance is a record of the sitting and must survive a later change to the panel.
/// </summary>
public sealed class CommitteeDecisionAttendeeConfiguration : IEntityTypeConfiguration<CommitteeDecisionAttendee>
{
    public void Configure(EntityTypeBuilder<CommitteeDecisionAttendee> builder)
    {
        builder.ToTable("CommitteeDecisionAttendees");
        builder.Property(entity => entity.UserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(entity => new { entity.DecisionId, entity.UserId }).IsUnique();
    }
}
