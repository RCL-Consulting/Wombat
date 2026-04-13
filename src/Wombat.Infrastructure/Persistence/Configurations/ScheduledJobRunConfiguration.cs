using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Scheduling;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class ScheduledJobRunConfiguration : IEntityTypeConfiguration<ScheduledJobRun>
{
    public void Configure(EntityTypeBuilder<ScheduledJobRun> builder)
    {
        builder.ToTable("ScheduledJobRuns");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Key).HasMaxLength(100).IsRequired();
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.Property(e => e.TriggeredBy).HasMaxLength(450);
        builder.HasIndex(e => e.Key);
        builder.HasIndex(e => e.StartedAt);
        builder.HasIndex(e => new { e.Key, e.StartedAt });
    }
}
