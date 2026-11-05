using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Persistence;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Colleges;

/// <summary>
/// Who already holds a College's name or short code (T394, flow 16, lane B; S10, N7, T272): the create and the update ask
/// before they stage anything, so a taken value is refused under its field naming its holder, and the unique index's
/// refusal is read the same way only when another save won the race in between.
/// </summary>
/// <remarks>
/// <para>
/// The compare is ordinal, as the unique indexes compare (N7): "cpsa" is not "CPSA". A College's name is national and not
/// secret, so naming the holder names no one out of the Administrator's scope (the audit row records the sentence).
/// </para>
/// <para>
/// This assumes the database's collation is case-sensitive, as PostgreSQL's default is
/// (<c>Create_WithTheSameShortCodeInAnotherCase_IsNotRefusedByTheCheck</c> pins it). Under a case-insensitive collation the
/// query would return "cpsa" for "CPSA", the ordinal filter would drop it, the unique index (in that collation) would then
/// refuse the save, and the race's read-back would find no ordinal holder and rethrow the database's refusal, so the page
/// would say "could not be saved" for a plain duplicate (T394 fix pass, code N-3). A change of collation changes this
/// compare with it.
/// </para>
/// </remarks>
internal static class CollegeHolders
{
    /// <summary>The words of the field refusals and the validators (R1, Spec-2 § 9's N1).</summary>
    public const string NameRequired = "Enter a name.";
    public const string ShortCodeRequired = "Enter a short code.";
    public const string NameTooLong = "A name is at most 200 characters.";

    /// <summary>NEW (T394, lane B): no board words the short code's cap; it reads as the name's does.</summary>
    public const string ShortCodeTooLong = "A short code is at most 32 characters.";

    public const string DescriptionTooLong = "A description is at most 1,000 characters.";

    public static string NameTaken(string name) => $"{name} is already a College's name. Choose another.";

    public static string ShortCodeTaken(string shortCode, string holder)
        => $"{shortCode} is already the short code of {holder}. Choose another.";

    /// <summary>
    /// The refusal for a name or short code another College holds, the name first (the form's order), or null when
    /// neither is taken. Read with no tracking, so a read after a refused save sees the store, not this context's staging.
    /// </summary>
    public static async Task<FieldRefusalException?> RefusalAsync(
        IApplicationDbContext dbContext,
        int? exceptId,
        string name,
        string shortCode,
        Exception? inner,
        CancellationToken cancellationToken)
    {
        var holders = await dbContext.Set<College>()
            .AsNoTracking()
            .Where(college => (exceptId == null || college.Id != exceptId) && (college.Name == name || college.ShortCode == shortCode))
            .Select(college => new { college.Name, college.ShortCode })
            .ToListAsync(cancellationToken);

        // Ordinal, in memory too: the in-memory provider and a case-insensitive collation would otherwise disagree with the
        // index about what is taken.
        if (holders.FirstOrDefault(holder => string.Equals(holder.Name, name, StringComparison.Ordinal)) is not null)
        {
            return new FieldRefusalException(nameof(CreateCollegeCommand.Name), NameTaken(name), inner);
        }

        if (holders.FirstOrDefault(holder => string.Equals(holder.ShortCode, shortCode, StringComparison.Ordinal)) is { } code)
        {
            return new FieldRefusalException(nameof(CreateCollegeCommand.ShortCode), ShortCodeTaken(shortCode, code.Name), inner);
        }

        return null;
    }

    /// <summary>
    /// Saves, reading the unique index's refusal as a taken value only when a holder is there now (the race); any other
    /// refused save is rethrown as it is, for the page's fixed sentence (T272).
    /// </summary>
    public static async Task SaveAsync(
        IApplicationDbContext dbContext,
        int? exceptId,
        string name,
        string shortCode,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (PostgresErrors.IsUniqueViolation(exception))
        {
            if (await RefusalAsync(dbContext, exceptId, name, shortCode, exception, cancellationToken) is { } raced)
            {
                throw raced;
            }

            throw;
        }
    }
}
