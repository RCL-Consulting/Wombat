using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.DataRights;

namespace Wombat.Infrastructure.Persistence.Configurations.DataRights;

public sealed class DataRightsRequestConfiguration : IEntityTypeConfiguration<DataRightsRequest>
{
    public void Configure(EntityTypeBuilder<DataRightsRequest> builder)
    {
        builder.ToTable("DataRightsRequests");

        // T112: the list path filters on this on every page load. No FK — the column is a snapshot of
        // where the requester was when they asked, and restructuring the institution tree must not
        // cascade into, or be blocked by, a historical statutory request.
        builder.HasIndex(e => e.InstitutionId);

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("uuid")
            .ValueGeneratedNever();

        builder.Property(e => e.RequesterUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(e => e.RequesterDisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.RequestedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(e => e.Type)
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(e => e.Reason)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(e => e.DecisionNote)
            .HasMaxLength(4000);

        builder.Property(e => e.DecidedByUserId)
            .HasMaxLength(450);

        builder.Property(e => e.DecidedOn)
            .HasColumnType("timestamp with time zone");

        builder.Property(e => e.CompletedOn)
            .HasColumnType("timestamp with time zone");

        builder.HasMany(e => e.Rectifications)
            .WithOne(r => r.Request)
            .HasForeignKey(r => r.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optimistic concurrency on the request, on Postgres's own xmin (no column, no write path), as CommitteeReview does
        // (T387, F8). An approval, and above all an erasure, is decided on a status read moments earlier: without a token a
        // withdrawal saved in between would be overwritten by the approval, and the person erased on a request they had
        // taken back. Whichever of two saves comes second is refused, and the handler reads the row again.
        builder.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

        builder.HasIndex(e => e.RequesterUserId);
        builder.HasIndex(e => new { e.Status, e.RequestedOn });
    }
}
