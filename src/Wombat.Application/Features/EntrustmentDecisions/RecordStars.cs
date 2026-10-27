using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.EntrustmentDecisions;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// A trainee record's STARs, as flow 13's pages count them (T379, F6 and S8; build-contracts § 0).
/// </summary>
/// <param name="AnyIssued">
/// Whether the person was ever issued a STAR: any ratified decision, in force or not (F6). A pending one is not a decision
/// yet (<c>PendingEntrustmentDecision</c>). Where it is true, My authorisations has something to list, in its "no longer
/// in force" group if nowhere else, so the record's link to it shows (Home's card, My progress's "Your record:" line) and
/// the ended profile's account sentence names the certificates.
/// </param>
/// <param name="InForce">
/// How many EPAs of the record carry a STAR in force: on an ended programme, as the programme left them (T312,
/// <see cref="EntrustmentStandingDto.StarsAsEndedOn" />). Home's count: "Your 15 STARs in force, each with its certificate
/// to download." Zero with <see cref="AnyIssued" /> true is PROPOSED's "None of your STARs is in force now; My
/// authorisations lists them."
/// </param>
public sealed record RecordStarsDto(bool AnyIssued, int InForce);

/// <summary>
/// Reads <see cref="RecordStarsDto" /> for one person: the former trainee's Home and My progress's record line (lane R), and
/// the ended profile's account sentence (lane P), so the three cannot disagree on whether a STAR exists.
/// </summary>
/// <remarks>
/// It checks no scope: its callers do, as <see cref="EntrustmentStandingReader" />'s do. <see cref="RecordStarsDto.InForce" />
/// is the standing's own count (the EPAs whose standing row has a decision), read through
/// <see cref="EntrustmentStandingReader.ReadAsync" /> on the person's preferred profile without the latest ratings, so it is
/// the number the standing panel and the PDF show for the same record.
/// </remarks>
public static class RecordStars
{
    /// <param name="principal">Who is reading; the standing's reader asks it, though no rating is read here.</param>
    /// <param name="asOf">Today on the South African calendar.</param>
    public static async Task<RecordStarsDto> ReadAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        string traineeUserId,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(traineeUserId);

        var anyIssued = await dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .AnyAsync(decision => decision.TraineeUserId == traineeUserId, cancellationToken);
        if (!anyIssued)
        {
            return new RecordStarsDto(false, 0);
        }

        var standing = await EntrustmentStandingReader.ReadAsync(
            dbContext, principal, traineeUserId, asOf, withLatestRatings: false, cancellationToken);
        var inForce = standing?.Epas.Count(epa => epa.Decision is not null) ?? 0;

        return new RecordStarsDto(true, inForce);
    }
}
