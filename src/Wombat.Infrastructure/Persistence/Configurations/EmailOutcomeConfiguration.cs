using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Email;

namespace Wombat.Infrastructure.Persistence.Configurations;

/// <summary>
/// The mail outcomes Home's Mail line reads (T400, flow 18, Q18, K3): the moment, the kind (stored as its number, as a
/// job run's status is) and the redacted refusal; never an address, a subject, a body, a reference or a tag. Read by
/// moment (the last 24 hours, the last sent) and by kind and moment (the last drop).
/// </summary>
public sealed class EmailOutcomeConfiguration : IEntityTypeConfiguration<EmailOutcome>
{
    public void Configure(EntityTypeBuilder<EmailOutcome> builder)
    {
        builder.ToTable("EmailOutcomes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Refusal).HasMaxLength(EmailOutcome.RefusalMaxLength);
        builder.HasIndex(e => e.OccurredAt);
        builder.HasIndex(e => new { e.Kind, e.OccurredAt });
    }
}
