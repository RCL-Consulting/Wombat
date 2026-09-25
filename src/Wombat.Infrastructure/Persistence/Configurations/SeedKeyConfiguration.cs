using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wombat.Infrastructure.Persistence.Configurations;

/// <summary>
/// The <c>SeedKey</c> column a seeder finds its own rows by (T221), configured the same way on every table that has one.
/// </summary>
/// <remarks>
/// Unique where it is set, so one key names one row, and null for every row an administrator makes. Postgres would allow
/// many nulls in a plain unique index too; the filter says so outright and keeps the index to the handful of seeded rows.
/// </remarks>
internal static class SeedKeyConfiguration
{
    public const string PropertyName = "SeedKey";

    public const int MaxLength = 64;

    public static void HasSeedKey<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        builder.Property<string?>(PropertyName).HasMaxLength(MaxLength);
        builder.HasIndex(PropertyName)
            .IsUnique()
            .HasFilter($"\"{PropertyName}\" IS NOT NULL");
    }
}
