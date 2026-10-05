using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Activities;

namespace Wombat.Infrastructure.Persistence.Configurations.Activities;

/// <summary>A reminder about a waiting request (T358, flow 06; Q4, C4).</summary>
public sealed class ActivityReminderConfiguration : IEntityTypeConfiguration<ActivityReminder>
{
    public void Configure(EntityTypeBuilder<ActivityReminder> builder)
    {
        builder.ToTable("ActivityReminders");
        builder.Property(entity => entity.SentByUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.AssessorUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.SentOn).HasColumnType("timestamp with time zone");

        // A key without a navigation on either side: the reminder goes with its activity, and a save that adds one never
        // reaches the activity row, whose UpdatedOn the wait counts from (C4c).
        builder.HasOne<Activity>()
            .WithMany()
            .HasForeignKey(entity => entity.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        // The same-day block (round 3's settled rule): one reminder per request per South African day, whoever sends it.
        // The command checks first; two senders in the same instant meet this index, and the loser answers "already
        // reminded today" (D6).
        builder.HasIndex(entity => new { entity.ActivityId, entity.SentOnDay }).IsUnique();

        // The last reminder of each row the waiting list reads.
        builder.HasIndex(entity => new { entity.ActivityId, entity.SentOn });
    }
}
