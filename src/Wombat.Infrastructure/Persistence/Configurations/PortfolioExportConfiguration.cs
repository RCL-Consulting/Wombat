using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Reporting;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class PortfolioExportConfiguration : IEntityTypeConfiguration<PortfolioExport>
{
    public void Configure(EntityTypeBuilder<PortfolioExport> builder)
    {
        builder.ToTable("PortfolioExports");
        builder.Property(entity => entity.TraineeUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.ExportedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.ContentHash).HasMaxLength(64).IsRequired();
        builder.Property(entity => entity.FileName).HasMaxLength(200).IsRequired();
        builder.Property(entity => entity.FilterFromDate).HasColumnType("date");
        builder.Property(entity => entity.FilterToDate).HasColumnType("date");

        builder.HasIndex(entity => entity.ContentHash);
        builder.HasIndex(entity => entity.TraineeUserId);
    }
}
