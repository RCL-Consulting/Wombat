using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// The registrar the Admit page admits (T373, flow 11, M01–M15): who they are, where and as what they were invited, when
/// they registered, and the curriculum the admission offers (F1). Null for an id no registrar waiting in the caller's scope
/// has: an unknown id, an account that holds no PendingTrainee, and one outside the caller's institution read alike (M08),
/// so the page says nothing about another institution's account.
/// </summary>
public sealed record GetAdmissionCandidateQuery(string UserId, ClaimsPrincipal Principal) : IRequest<AdmissionCandidateDto?>;

/// <summary>Whether a registrar can be admitted at all, before anything is chosen (M04, M04b; check rule 3).</summary>
public enum AdmissionStanding
{
    /// <summary>A curriculum is offered: the form is drawn.</summary>
    Ready,

    /// <summary>The institution has adopted curricula, but none for the invited sub-speciality (check rule 3).</summary>
    NothingAdoptedForInvitedScope,

    /// <summary>The institution has adopted no curriculum at all.</summary>
    NothingAdopted,

    /// <summary>The registrar belongs to no institution (M04b): only an Administrator reaches one.</summary>
    NoInstitution
}

/// <param name="InstitutionName">The registrar's institution; null for one with none (M04b).</param>
/// <param name="InvitedAs">The invited scope in the pages' words (<see cref="InvitationScopeWords.Disciplines" />), "Paediatrics / Paediatrics"; null with none (S5).</param>
/// <param name="RegisteredOn">The used invitation's day on the South African calendar; null without one (an SSO registrar).</param>
/// <param name="HasInvitedScope">Invited to a sub-speciality: the admission keeps it, and offers its adopted curriculum alone (F1).</param>
/// <param name="Curricula">What the admission accepts: the curriculum adopted for the invited sub-speciality, or, invited to none, the institution's adopted curricula (M14).</param>
/// <param name="IsDeactivated">The account is deactivated: the admission is refused (S6, M13).</param>
/// <param name="Today">Today on the South African calendar, the start's default.</param>
public sealed record AdmissionCandidateDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    string? InstitutionName,
    string? InvitedAs,
    DateOnly? RegisteredOn,
    bool HasInvitedScope,
    IReadOnlyList<CurriculumChoiceDto> Curricula,
    AdmissionStanding Standing,
    bool IsDeactivated,
    DateOnly Today)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public sealed class GetAdmissionCandidateQueryHandler : IRequestHandler<GetAdmissionCandidateQuery, AdmissionCandidateDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _clock;

    public GetAdmissionCandidateQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users, TimeProvider? clock = null)
    {
        _dbContext = dbContext;
        _users = users;
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<AdmissionCandidateDto?> Handle(GetAdmissionCandidateQuery request, CancellationToken cancellationToken)
    {
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

        var user = string.IsNullOrWhiteSpace(request.UserId)
            ? null
            : await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (!AdmissionScope.IsWaitingInScope(user, request.Principal))
        {
            return null;
        }

        var names = await RegistrarFacts.ScopeNamesAsync(_dbContext, [user!], cancellationToken);
        var used = await RegistrarFacts.UsedInvitationAsync(_dbContext, user!.Email, cancellationToken);
        var institutionName = user.InstitutionId is { } institutionId
            ? names.Institutions.GetValueOrDefault(institutionId)
            : null;

        IReadOnlyList<CurriculumChoiceDto> curricula = [];
        var standing = AdmissionStanding.NoInstitution;
        if (user.InstitutionId is { } at)
        {
            curricula = await CurriculumChoices.ListAsync(AdmissionCurricula.Offered(_dbContext, at, user), cancellationToken);
            standing = curricula.Count > 0
                ? AdmissionStanding.Ready
                : AdmissionCurricula.HasInvitedScope(user)
                    && await TraineeAdoptionResolver.ActiveAdoptions(_dbContext, at).AnyAsync(cancellationToken)
                        ? AdmissionStanding.NothingAdoptedForInvitedScope
                        : AdmissionStanding.NothingAdopted;
        }

        return new AdmissionCandidateDto(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            institutionName,
            names.InvitedAs(user),
            used?.UsedOn is { } usedOn ? ProgrammeCalendar.DateOf(usedOn) : null,
            AdmissionCurricula.HasInvitedScope(user),
            curricula,
            standing,
            user.IsDeactivated,
            QuotaCalendar.Today(_clock));
    }
}

/// <summary>Who the admission pages and the admission itself may act on (T373, flow 11).</summary>
internal static class AdmissionScope
{
    /// <summary>
    /// A registrar waiting for admission (PendingTrainee) whom the caller may admit: an Administrator any, an
    /// Institutional admin one of their own institution. A registrar of no institution is the Administrator's alone, as
    /// the Waiting list shows them (<see cref="ListPendingTraineesQuery" />): until T373 an Institutional admin who held
    /// the id could admit such a registrar into their own institution.
    /// </summary>
    public static bool IsWaitingInScope(UserIdentityDetails? user, ClaimsPrincipal principal)
        => user is not null
            && user.Roles.Contains(WombatRoles.PendingTrainee, StringComparer.Ordinal)
            && IsInScope(user, principal);

    public static bool IsInScope(UserIdentityDetails user, ClaimsPrincipal principal)
        => principal.IsAdministrator()
            || (user.InstitutionId is { } institutionId && principal.CanAccessInstitution(institutionId));
}

/// <summary>
/// The curricula an admission offers (T373, flow 11, F1): the one rule the Admit page's read and the command share, so the
/// page offers only what the command accepts (T304's rule, narrowed).
/// </summary>
internal static class AdmissionCurricula
{
    /// <summary>The registrar was invited to a sub-speciality, which the admission keeps (F1).</summary>
    public static bool HasInvitedScope(UserIdentityDetails user) => user.SubSpecialityIds.Count > 0;

    /// <summary>
    /// The institution's active adoptions for the registrar's invited sub-speciality; for a registrar invited to none
    /// (an SSO registrar, M14), every active adoption of the institution, within the invited speciality when there is one.
    /// Until T373 the admission offered every active adoption to everyone, and moved the registrar's scope to whichever was
    /// chosen (<c>AdmitTrainee.cs:129–134</c> at <c>69a7df20</c>).
    /// </summary>
    public static IQueryable<Curriculum> Offered(IApplicationDbContext dbContext, int institutionId, UserIdentityDetails user)
    {
        var adoptions = TraineeAdoptionResolver.ActiveAdoptions(dbContext, institutionId);
        if (HasInvitedScope(user))
        {
            var subSpecialityIds = user.SubSpecialityIds.ToArray();
            adoptions = adoptions.Where(adoption => subSpecialityIds.Contains(adoption.SubSpecialityId));
        }

        var adopted = adoptions.Select(adoption => adoption.CurriculumId);
        var curricula = dbContext.Set<Curriculum>().Where(curriculum => adopted.Contains(curriculum.Id));
        if (!HasInvitedScope(user) && user.SpecialityIds.Count > 0)
        {
            var specialityIds = user.SpecialityIds.ToArray();
            curricula = curricula.Where(curriculum => specialityIds.Contains(curriculum.SubSpeciality.SpecialityId));
        }

        return curricula;
    }
}

/// <summary>
/// What the onboarding pages say about a registrar beside their account (T373, flow 11, build-contracts § 3): the names of
/// their institution and invited scope, and the invitation they registered from. Read once per page, by the query that
/// serves it.
/// </summary>
internal static class RegistrarFacts
{
    /// <summary>The names of the users' institutions, specialities and sub-specialities, each read once.</summary>
    public static async Task<ScopeNames> ScopeNamesAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<UserIdentityDetails> users,
        CancellationToken cancellationToken)
    {
        var institutionIds = users.Where(user => user.InstitutionId.HasValue).Select(user => user.InstitutionId!.Value).Distinct().ToArray();
        var specialityIds = users.SelectMany(user => user.SpecialityIds).Distinct().ToArray();
        var subSpecialityIds = users.SelectMany(user => user.SubSpecialityIds).Distinct().ToArray();

        var institutions = await dbContext.Set<Institution>().AsNoTracking()
            .Where(entity => institutionIds.Contains(entity.Id))
            .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);
        var subSpecialities = await dbContext.Set<SubSpeciality>().AsNoTracking()
            .Where(entity => subSpecialityIds.Contains(entity.Id))
            .Select(entity => new { entity.Id, entity.Name, entity.SpecialityId })
            .ToDictionaryAsync(entity => entity.Id, entity => (entity.Name, entity.SpecialityId), cancellationToken);

        // A sub-speciality is named under its own speciality, read too where the account's scope does not list it.
        specialityIds = [.. specialityIds.Union(subSpecialities.Values.Select(sub => sub.SpecialityId))];
        var specialities = await dbContext.Set<Speciality>().AsNoTracking()
            .Where(entity => specialityIds.Contains(entity.Id))
            .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);

        return new ScopeNames(institutions, specialities, subSpecialities);
    }

    /// <summary>
    /// The invitation the registrar registered from: the latest used one to their address, whatever its spelling (S4,
    /// <see cref="InvitationAddress.ToAddress" />). Null for a registrar who registered from none (an SSO registrar).
    /// </summary>
    public static Task<Invitation?> UsedInvitationAsync(IApplicationDbContext dbContext, string email, CancellationToken cancellationToken)
        => string.IsNullOrWhiteSpace(email)
            ? Task.FromResult<Invitation?>(null)
            : dbContext.Set<Invitation>().AsNoTracking()
                .ToAddress(email)
                .Where(invitation => invitation.UsedOn != null)
                .OrderByDescending(invitation => invitation.UsedOn)
                .ThenByDescending(invitation => invitation.Id)
                .FirstOrDefaultAsync(cancellationToken);
}

/// <summary>The names <see cref="RegistrarFacts.ScopeNamesAsync" /> read.</summary>
internal sealed record ScopeNames(
    IReadOnlyDictionary<int, string> Institutions,
    IReadOnlyDictionary<int, string> Specialities,
    IReadOnlyDictionary<int, (string Name, int SpecialityId)> SubSpecialities)
{
    /// <summary>
    /// The registrar's invited sub-speciality and its speciality, by name; with no sub-speciality, the speciality alone;
    /// with neither, nulls. A registrar holds one of each from an invitation; of several, the lowest id is named.
    /// </summary>
    public (string? Speciality, string? SubSpeciality) Of(UserIdentityDetails user)
    {
        foreach (var subSpecialityId in user.SubSpecialityIds.Order())
        {
            if (SubSpecialities.TryGetValue(subSpecialityId, out var sub))
            {
                return (Specialities.GetValueOrDefault(sub.SpecialityId) ?? SpecialityOf(user), sub.Name);
            }
        }

        return (SpecialityOf(user), null);
    }

    /// <summary>"Paediatrics / Paediatrics", or null with no invited scope (S5).</summary>
    public string? InvitedAs(UserIdentityDetails user)
    {
        var (speciality, subSpeciality) = Of(user);
        return InvitationScopeWords.Disciplines(speciality, subSpeciality);
    }

    private string? SpecialityOf(UserIdentityDetails user)
        => user.SpecialityIds.Order().Select(id => Specialities.GetValueOrDefault(id)).FirstOrDefault(name => name is not null);
}
