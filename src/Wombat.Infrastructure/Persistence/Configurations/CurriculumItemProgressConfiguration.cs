using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Domain.Curricula;

namespace Wombat.Infrastructure.Persistence.Configurations;

public sealed class CurriculumItemProgressConfiguration : IEntityTypeConfiguration<CurriculumItemProgress>
{
    /// <summary>
    /// Named explicitly because EF's generated name would run to 80 characters and Postgres truncates
    /// identifiers at 63, so the name in the database would not be the name anyone searches for.
    /// </summary>
    public const string NaturalKeyIndexName = "UX_CurriculumItemProgresses_Item_Trainee_Period";

    public void Configure(EntityTypeBuilder<CurriculumItemProgress> builder)
    {
        builder.ToTable("CurriculumItemProgresses", table =>
        {
            // One row per semester (T130). The in-memory provider the unit suites run on enforces neither
            // constraint; only Postgres does, so the integration test asserts both.
            table.HasCheckConstraint("CK_CurriculumItemProgresses_Semester", "\"Semester\" IN (1, 2)");
            table.HasCheckConstraint("CK_CurriculumItemProgresses_AcademicYear", "\"AcademicYear\" BETWEEN 1 AND 9999");
        });
        builder.Property(entity => entity.TraineeUserId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.LastUpdated).HasColumnType("timestamp with time zone");
        builder.Property(entity => entity.LastObservedOn).HasColumnType("date");
        builder.Property(entity => entity.CreditedActivityKeysJson).HasColumnType("jsonb").IsRequired();

        // The natural key (CurriculumItemProgressKey). A row is the bucket for one semester, and the semester
        // is always the one containing the encounter date, so this is unique by construction on the write path.
        builder.HasIndex(entity => new { entity.CurriculumItemId, entity.TraineeUserId, entity.AcademicYear, entity.Semester })
            .IsUnique()
            .HasDatabaseName(NaturalKeyIndexName);
        builder.HasIndex(entity => entity.TraineeUserId);

        // Optimistic concurrency, using Postgres's own xmin, which costs no column and no write path. A progress
        // row holds a running total, so two writers who both read it and both add one would otherwise save the
        // same number and lose a credit. The same applies to a rebuild that writes absolute values over a row a
        // live completion has just incremented. With the token, the second writer's whole save rolls back
        // instead.
        builder.Property<uint>("xmin").HasColumnName("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

        builder.HasOne(entity => entity.CurriculumItem)
            .WithMany()
            .HasForeignKey(entity => entity.CurriculumItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // The scale this row's stored tally was computed on (T109). Shadow FK — the progress row has no
        // reason to navigate to a scale, but the reference must not be allowed to dangle.
        builder.HasOne<Wombat.Domain.Epas.EntrustmentScale>()
            .WithMany()
            .HasForeignKey(entity => entity.MinimumLevelScaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => entity.MinimumLevelScaleId);
    }
}
