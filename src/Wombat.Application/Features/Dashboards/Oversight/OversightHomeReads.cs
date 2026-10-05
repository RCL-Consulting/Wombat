using System.Security.Claims;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Programme;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Application.Features.Programme.Waiting;

namespace Wombat.Application.Features.Dashboards.Oversight;

/// <summary>
/// A Home card that previews Programme trainees (T358, flow 06; R2-Home): its first five registrars, in the list's order,
/// and how many the list holds, which "3 more in Programme trainees." and the count badge read.
/// </summary>
/// <param name="Rows">The first <see cref="ListProgrammeTraineesQueryHandler.HomeRows" /> registrars.</param>
/// <param name="Total">Every registrar the card's list holds.</param>
public sealed record RegistrarsCardDto(IReadOnlyList<ProgrammeTraineeRowDto> Rows, int Total)
{
    /// <summary>A card with nobody on it: no scope to read, or nobody in it.</summary>
    public static readonly RegistrarsCardDto Empty = new([], 0);

    /// <summary>How many the list holds past the rows shown.</summary>
    public int Beyond => Math.Max(0, Total - Rows.Count);
}

/// <summary>
/// The reads the four oversight Homes are made of (T358, flow 06, lane B; Q1, Q3, Q5, E3, E4, E5): each card is the first
/// rows of the list it previews, read by that list's own reader in the scope of the role whose Home it is, so a card and
/// the page its foot opens cannot disagree.
/// </summary>
/// <remarks>
/// A Home reads as its own role (E4): the Committee member's Home as Committee member, and so on, never the union of the
/// roles held. Where that role gives no scope (<see cref="ProgrammeScope.ResolveAsync" />: someone in the programme as a
/// trainee, T185; no institution; a claimed speciality that names none), every card reads empty, as the programme pages
/// read "not found". Each read here sits behind the dashboard's one query, so a failure is Home's one load error.
/// </remarks>
internal static class OversightHomeReads
{
    /// <summary>The scope's current registrars, the first five, and the scope's Targets by EPA, from one roster read.</summary>
    public static async Task<(RegistrarsCardDto Registrars, CurriculumCoverage Coverage)> RosterAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ProgrammeScopeDto? scope,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            // Nobody to name, but the empty coverage still names the semester it is empty for.
            return (RegistrarsCardDto.Empty, await CurriculumCoverageReader.ReadAsync(dbContext, users, [], asOf, cancellationToken));
        }

        var read = await ProgrammeRosterReader.ReadAsync(dbContext, users, scope, new ProgrammeTraineesFilter(), asOf, cancellationToken);
        return (Card(read), read.Coverage);
    }

    /// <summary>
    /// The Coordinator's Nothing filed in 30 days: Programme trainees filtered Nothing filed (E5, the later of admission and
    /// today − 30), its first five.
    /// </summary>
    public static async Task<RegistrarsCardDto> NothingFiledAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ProgrammeScopeDto? scope,
        DateOnly asOf,
        CancellationToken cancellationToken)
    {
        if (scope is null)
        {
            return RegistrarsCardDto.Empty;
        }

        var read = await ProgrammeRosterReader.ReadAsync(
            dbContext, users, scope, new ProgrammeTraineesFilter(NothingFiled: true), asOf, cancellationToken);
        return Card(read);
    }

    /// <summary>
    /// Waiting for assessors' first page of five (Q3, E3, E4): what waits for a named assessor in the scope, oldest first,
    /// with every count the page's heading reads. Null where the role gives no scope.
    /// </summary>
    public static async Task<WaitingForAssessorsDto?> WaitingAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        IReminderRecipients recipients,
        TimeProvider clock,
        DashboardThresholds thresholds,
        ClaimsPrincipal principal,
        ProgrammeScopeDto? scope,
        CancellationToken cancellationToken)
        => scope is null
            ? null
            : await WaitingForAssessorsReader.ReadAsync(
                dbContext,
                users,
                recipients,
                clock,
                thresholds,
                principal,
                scope,
                new WaitingForAssessorsFilter(),
                page: 1,
                pageSize: ListWaitingForAssessorsQueryHandler.HomeRows,
                cancellationToken);

    private static RegistrarsCardDto Card(ProgrammeRosterRead read)
        => new(read.Rows.Take(ListProgrammeTraineesQueryHandler.HomeRows).ToList(), read.Rows.Count);
}
