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
        builder.HasIndex(entity => new { entity.ActivityId, entity.OccurredOn });
    }
}
