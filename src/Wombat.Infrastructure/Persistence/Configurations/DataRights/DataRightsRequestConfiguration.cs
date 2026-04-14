using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.DataRights;

namespace Wombat.Infrastructure.Persistence.Configurations.DataRights;

public sealed class DataRightsRequestConfiguration : IEntityTypeConfiguration<DataRightsRequest>
{
    public void Configure(EntityTypeBuilder<DataRightsRequest> builder)
    {
        builder.ToTable("DataRightsRequests");

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

        builder.HasIndex(e => e.RequesterUserId);
        builder.HasIndex(e => new { e.Status, e.RequestedOn });
    }
}
