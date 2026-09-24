using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>A panel an EPA's decision routes to.</summary>
public sealed record CommitteeRoutedPanelDto(int Id, string Name);

/// <summary>Who decides one EPA for a trainee of one programme at the institution.</summary>
/// <param name="DecisionBodyKey">The College committee the curriculum item gives the EPA to, or null for none.</param>
/// <param name="DecisionBodyName">That committee's name.</param>
/// <param name="Panels">
/// The panels the EPA's decision routes to (<see cref="DecisionRouting.RoutesTo(string?, DecisionPanel, TraineeScope?, IEnumerable{DecisionPanel})" />).
/// Empty when no panel at the institution covers the programme.
/// </param>
/// <param name="FallsBackToGeneral">
/// True when the College gives the EPA to a named committee and no panel at the institution sits as it for this
/// programme, so the general panels decide it: "no neonatal panel: routed to general".
/// </param>
public sealed record CommitteeRoutingLineDto(
    int EpaId,
    string EpaCode,
    string EpaTitle,
    string? DecisionBodyKey,
    string? DecisionBodyName,
    IReadOnlyList<CommitteeRoutedPanelDto> Panels,
    bool FallsBackToGeneral);

/// <summary>One programme (curriculum version) followed at the institution, and who decides each of its EPAs.</summary>
public sealed record CommitteeRoutingProgrammeDto(
    int CurriculumId,
    string CurriculumName,
    string CurriculumVersion,
    string? SpecialityName,
    IReadOnlyList<CommitteeRoutingLineDto> Lines);

/// <summary>Who decides each EPA at one institution. (T131 slice 3)</summary>
public sealed record CommitteeRoutingDto(
    int InstitutionId,
    string InstitutionName,
    IReadOnlyList<CommitteeRoutingProgrammeDto> Programmes);

/// <summary>
/// Who decides each EPA at an institution: for every programme followed there, which panel takes each EPA's
/// entrustment decision, and where the College names a committee no panel sits as, that the general panels take it.
/// (T131 slice 3)
/// </summary>
/// <remarks>
/// <para>
/// An institution's committee arrangement, so it is scoped like one (the T056 family). A global Administrator must name
/// the institution; anyone else reads their own, and naming another institution returns null, as an institution that
/// does not exist does (a 404 that confirms nothing). Only the roles that work with the committee read it: an
/// InstitutionalAdmin, a Speciality or SubSpecialityAdmin, a Coordinator or a CommitteeMember. Anyone else gets null.
/// </para>
/// <para>
/// The programmes are the curricula the institution has adopted and those its trainees follow, the latter covering a
/// trainee still on an older version. Each programme's items are the national core and this institution's own local
/// items, never another institution's (a curriculum row is shared by every institution that adopted it), and only items
/// in force (<see cref="CurriculumItemsInForce" />). Each line asks <see cref="DecisionRouting" /> of every panel at the
/// institution, for a trainee of that programme, so the card says exactly what routing does.
/// </para>
/// </remarks>
public sealed record GetCommitteeRoutingQuery(int? InstitutionId, ClaimsPrincipal Principal)
    : IRequest<CommitteeRoutingDto?>;

public sealed class GetCommitteeRoutingQueryHandler : IRequestHandler<GetCommitteeRoutingQuery, CommitteeRoutingDto?>
{
    /// <summary>The refusal a global Administrator gets for not naming an institution.</summary>
    internal const string AdministratorMustNameInstitution = "Choose the institution whose committee routing to show.";

    private readonly IApplicationDbContext _dbContext;

    public GetCommitteeRoutingQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeRoutingDto?> Handle(GetCommitteeRoutingQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        var institutionId = ResolveInstitution(request);
        if (institutionId is not int id)
        {
            return null;
        }

        var institutionName = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(institution => institution.Id == id)
            .Select(institution => institution.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (institutionName is null)
        {
            return null;
        }

        var curriculumIds = await ProgrammeCurriculumIdsAsync(id, cancellationToken);
        var programmes = curriculumIds.Count == 0
            ? []
            : await BuildProgrammesAsync(id, curriculumIds, cancellationToken);

        return new CommitteeRoutingDto(id, institutionName, programmes);
    }

    /// <summary>The institution asked about, or null when the caller may not read it.</summary>
    private static int? ResolveInstitution(GetCommitteeRoutingQuery request)
    {
        var principal = request.Principal;

        if (principal.IsAdministrator())
        {
            return request.InstitutionId ?? throw new InvalidOperationException(AdministratorMustNameInstitution);
        }

        if (!WorksWithTheCommittee(principal) || principal.GetInstitutionId() is not int own)
        {
            return null;
        }

        return request.InstitutionId is null || request.InstitutionId == own ? own : null;
    }

    private static bool WorksWithTheCommittee(ClaimsPrincipal principal)
        => principal.IsInstitutionalAdmin() ||
           principal.IsInRole(WombatRoles.SpecialityAdmin) ||
           principal.IsInRole(WombatRoles.SubSpecialityAdmin) ||
           principal.IsInRole(WombatRoles.Coordinator) ||
           principal.IsInRole(WombatRoles.CommitteeMember);

    /// <summary>The curricula the institution has adopted, and those its trainees' preferred profiles follow.</summary>
    private async Task<IReadOnlyCollection<int>> ProgrammeCurriculumIdsAsync(int institutionId, CancellationToken cancellationToken)
    {
        var adopted = await _dbContext.Set<InstitutionCurriculumAdoption>()
            .AsNoTracking()
            .Where(adoption => adoption.InstitutionId == institutionId && adoption.IsActive)
            .Select(adoption => adoption.CurriculumId)
            .ToListAsync(cancellationToken);

        var followed = await TraineeScopeResolver.PreferredProfiles(_dbContext)
            .Where(profile => profile.InstitutionId == institutionId)
            .Select(profile => profile.CurriculumId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return adopted.Concat(followed).ToHashSet();
    }

    private async Task<IReadOnlyList<CommitteeRoutingProgrammeDto>> BuildProgrammesAsync(
        int institutionId,
        IReadOnlyCollection<int> curriculumIds,
        CancellationToken cancellationToken)
    {
        // One level at a time, as TraineeScopeResolver does: a required navigation in one projection is an INNER join.
        var curricula = await _dbContext.Set<Curriculum>()
            .AsNoTracking()
            .Where(curriculum => curriculumIds.Contains(curriculum.Id))
            .Select(curriculum => new { curriculum.Id, curriculum.Name, curriculum.Version, curriculum.SubSpecialityId })
            .ToListAsync(cancellationToken);

        var subSpecialityIds = curricula.Select(curriculum => curriculum.SubSpecialityId).Distinct().ToArray();
        var specialityBySubSpeciality = await _dbContext.Set<SubSpeciality>()
            .AsNoTracking()
            .Where(subSpeciality => subSpecialityIds.Contains(subSpeciality.Id))
            .ToDictionaryAsync(subSpeciality => subSpeciality.Id, subSpeciality => subSpeciality.SpecialityId, cancellationToken);

        var specialityIds = specialityBySubSpeciality.Values.Distinct().ToArray();
        var specialityNames = await _dbContext.Set<Speciality>()
            .AsNoTracking()
            .Where(speciality => specialityIds.Contains(speciality.Id))
            .ToDictionaryAsync(speciality => speciality.Id, speciality => speciality.Name, cancellationToken);

        // The national core and this institution's own local items; never another institution's, whose rows share the
        // curriculum.
        var items = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .InForce()
            .Where(item => curriculumIds.Contains(item.CurriculumId) &&
                           (item.OwningInstitutionId == null || item.OwningInstitutionId == institutionId))
            .Select(item => new { item.CurriculumId, item.EpaId, item.Epa.Code, item.Epa.Title, item.DecisionBodyKey })
            .ToListAsync(cancellationToken);

        var bodyNames = await _dbContext.Set<DecisionBody>()
            .AsNoTracking()
            .ToDictionaryAsync(body => body.Key, body => body.Name, StringComparer.Ordinal, cancellationToken);

        var panels = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .Where(panel => panel.InstitutionId == institutionId)
            .OrderBy(panel => panel.Name)
            .ThenBy(panel => panel.Id)
            .ToListAsync(cancellationToken);

        return curricula
            .Select(curriculum =>
            {
                int? specialityId = specialityBySubSpeciality.TryGetValue(curriculum.SubSpecialityId, out var found)
                    ? found
                    : null;
                var trainee = new TraineeScope(institutionId, specialityId, curriculum.SubSpecialityId);

                var lines = items
                    .Where(item => item.CurriculumId == curriculum.Id)
                    .OrderBy(item => item.Code, StringComparer.Ordinal)
                    .ThenBy(item => item.EpaId)
                    .Select(item =>
                    {
                        var bodyKey = DecisionBody.NormalizeKey(item.DecisionBodyKey);
                        return new CommitteeRoutingLineDto(
                            item.EpaId,
                            item.Code,
                            item.Title,
                            bodyKey,
                            bodyKey is not null && bodyNames.TryGetValue(bodyKey, out var bodyName) ? bodyName : null,
                            panels
                                .Where(panel => DecisionRouting.RoutesTo(bodyKey, panel, trainee, panels))
                                .Select(panel => new CommitteeRoutedPanelDto(panel.Id, panel.Name))
                                .ToArray(),
                            bodyKey is not null && DecisionRouting.BodyPanelFor(bodyKey, trainee, panels) is null);
                    })
                    .ToArray();

                return new CommitteeRoutingProgrammeDto(
                    curriculum.Id,
                    curriculum.Name,
                    curriculum.Version,
                    specialityId is int known && specialityNames.TryGetValue(known, out var name) ? name : null,
                    lines);
            })
            .OrderBy(programme => programme.CurriculumName, StringComparer.CurrentCulture)
            .ThenBy(programme => programme.CurriculumVersion, StringComparer.Ordinal)
            .ThenBy(programme => programme.CurriculumId)
            .ToArray();
    }
}
