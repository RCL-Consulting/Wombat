using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Activities;

namespace Wombat.Infrastructure.Persistence.Configurations.Activities;

public sealed class ActivityTransitionConfiguration : IEntityTypeConfiguration<ActivityTransition>
{
    public void Configure(EntityTypeBuilder<ActivityTransition> builder)
    {
        builder.ToTable("ActivityTransitions");
        builder.Property(entity => entity.FromState).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.ToState).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.TransitionKey).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.ActorUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.Note).HasMaxLength(4000);
        builder.Property(entity => entity.SnapshotJson).HasColumnType("jsonb").IsRequired();
        builder.Property(entity => entity.OccurredOn).HasColumnType("timestamp with time zone");

        // Nullable on purpose — null means "credit was never evaluated for this move", which is the
        // honest value for every row written before T108 and for every transition on an activity type
        // that credits nothing by design.
        builder.Property(entity => entity.CreditedItemCount);
        builder.HasIndex(entity => new { entity.ActivityId, entity.OccurredOn });
    }
}
