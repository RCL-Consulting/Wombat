using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Programme.Trainees;

/// <summary>
/// One registrar by trainee-profile id, for the registrar page (<c>/programme/trainees/{ProfileId:int}</c>; T358, flow 06,
/// C2, review 6), read as <paramref name="ActingRole" />. The page then sends flow 05's queries by the returned user id.
/// </summary>
/// <remarks>
/// <para>
/// Null, and so "Page not found", unless all of these hold, so an id never confirms anything (D7's reasoning):
/// </para>
/// <list type="bullet">
/// <item>the role gives a scope (<see cref="ProgrammeScope.ResolveAsync" />);</item>
/// <item>the profile is in it (<see cref="ProgrammeScope.Profiles" />: the institution, and for the two admins the
/// sub-speciality);</item>
/// <item>it is the user's preferred profile (<see cref="TraineeScopeResolver.PreferredProfiles" />). flow 05's reads take a
/// user id and read that profile, so an older profile's address would show the newer programme (review 6);</item>
/// <item>an account still holds the user id. An erased profile keeps its institution under a pseudonym no account holds
/// (<c>ErasureExecutor</c>), and naming it would print the pseudonym.</item>
/// </list>
/// <para>
/// An ended programme opens, read-only (r6): <see cref="ProgrammeTraineeDto.Ended" /> says how, and the training year and
/// the semester are those of the last day, as My progress reads an ended programme (T252). So does a locked account's: a
/// lock takes a registrar off the lists of current registrars (T268), not out of the staff's reach.
/// </para>
/// </remarks>
/// <param name="AsOf">The day to read for; today on the South African calendar by default. Tests pin it.</param>
public sealed record GetProgrammeTraineeQuery(ClaimsPrincipal Principal, string ActingRole, int ProfileId, DateOnly? AsOf = null)
    : IRequest<ProgrammeTraineeDto?>;

public sealed class GetProgrammeTraineeQueryValidator : AbstractValidator<GetProgrammeTraineeQuery>
{
    public GetProgrammeTraineeQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetProgrammeTraineeQueryHandler : IRequestHandler<GetProgrammeTraineeQuery, ProgrammeTraineeDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _clock;

    public GetProgrammeTraineeQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users, TimeProvider clock)
    {
        _dbContext = dbContext;
        _users = users;
        _clock = clock;
    }

    public async Task<ProgrammeTraineeDto?> Handle(GetProgrammeTraineeQuery request, CancellationToken cancellationToken)
    {
        var scope = await ProgrammeScope.ResolveAsync(_dbContext, request.Principal, request.ActingRole, cancellationToken);
        if (scope is null)
        {
            return null;
        }

        var preferred = TraineeScopeResolver.PreferredProfiles(_dbContext).Select(profile => profile.Id);
        var profile = await ProgrammeScope.Profiles(_dbContext, scope)
            .AsNoTracking()
            .Where(candidate => candidate.Id == request.ProfileId && preferred.Contains(candidate.Id))
            .SingleOrDefaultAsync(cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var contacts = await _users.GetContactsAsync([profile.UserId], cancellationToken);
        if (contacts is null || !contacts.TryGetValue(profile.UserId, out var contact))
        {
            return null;
        }

        var subSpecialityName = await _dbContext.Set<Curriculum>()
            .AsNoTracking()
            .Where(curriculum => curriculum.Id == profile.CurriculumId)
            .Select(curriculum => curriculum.SubSpeciality.Name)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;
        var institutionName = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(institution => institution.Id == profile.InstitutionId)
            .Select(institution => institution.Name)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        // An ended programme is read as on its last day, never later (T252, D49); a running one today.
        var today = request.AsOf ?? QuotaCalendar.Today(_clock);
        var day = profile.IsActive || profile.EndedOn is not { } ended || ended >= today ? today : ended;
        var name = $"{contact.FirstName} {contact.LastName}".Trim();

        return new ProgrammeTraineeDto(
            profile.Id,
            profile.UserId,
            name.Length == 0 ? profile.UserId : name,
            profile.GetStage(day),
            institutionName,
            subSpecialityName,
            QuotaText.SemesterName(AcademicPeriod.Containing(day)),
            profile.IsActive ? null : new ProgrammeEndDto(profile.CompletedOn is not null, profile.EndedOn, today),
            scope);
    }
}
