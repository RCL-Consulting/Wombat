using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Scheduling;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class ScheduledJobDefinitionConfiguration : IEntityTypeConfiguration<ScheduledJobDefinition>
{
    public void Configure(EntityTypeBuilder<ScheduledJobDefinition> builder)
    {
        builder.ToTable("ScheduledJobDefinitions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Key).HasMaxLength(100).IsRequired();
        builder.Property(e => e.CronExpression).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired();
        builder.HasIndex(e => e.Key).IsUnique();
    }
}
