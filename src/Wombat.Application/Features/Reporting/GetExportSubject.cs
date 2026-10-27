using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Users;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Reporting;

/// <summary>
/// Whose portfolio Export portfolio exports, read before its form (T379, flow 13, M4, N1, F6; Spec 2's row): the person's
/// name, where and on what they train, and the two days the form opens on.
/// </summary>
/// <param name="TraineeUserId">
/// The staff route's user id (<c>/portfolio/export/{TraineeUserId}</c>); null for the caller's own export
/// (<c>/portfolio/export</c>).
/// </param>
/// <remarks>
/// <para>
/// Null, and so the one Unavailable page, for every refused cause, so the page never offers what the command would refuse
/// and an id confirms nothing (Q19): a caller the export command's own check refuses
/// (<see cref="TraineeScopeResolver.MayReadAsync" />); an id no account holds (an erased person's pseudonym among them); and
/// at the own route, a caller with no trainee profile, whose export has no programme to hold (F6, R3-E19). An Administrator
/// is never refused here, as the command never refuses one.
/// </para>
/// <para>
/// The programme read is the preferred profile's (<see cref="TraineeScopeResolver.PreferredProfiles" />), the one the
/// command's scope and the PDF read. On the staff route a person the caller may export but who holds no profile (only an
/// Administrator reaches one) is answered with the name alone and a null <see cref="ExportSubjectDto.ProfileId" />: the
/// page then draws the trail without the programme's crumb (check build note 6).
/// </para>
/// </remarks>
public sealed record GetExportSubjectQuery(string? TraineeUserId, ClaimsPrincipal Principal) : IRequest<ExportSubjectDto?>;

public sealed class GetExportSubjectQueryValidator : AbstractValidator<GetExportSubjectQuery>
{
    public GetExportSubjectQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
    }
}

/// <summary>The person whose portfolio is exported, as the export page names them (T379, M4).</summary>
/// <param name="TraineeUserId">Their user id, which the export command takes.</param>
/// <param name="Name">The name as stored, "Lerato Molefe": the staff page's h1, trail and hints.</param>
/// <param name="InstitutionName">Where the preferred profile trains; null with no profile.</param>
/// <param name="CurriculumName">The curriculum's name, "Paediatric EPA Curriculum"; null with no profile.</param>
/// <param name="CurriculumVersion">The curriculum's version, "11.1"; null with no profile.</param>
/// <param name="TrainingYear">
/// The training year today, or on the last day of an ended programme (<see cref="EndedProgrammeDto.TrainingYearAtEnd" />);
/// null before the programme starts and with no profile.
/// </param>
/// <param name="ProgrammeStartDate">The programme start, where the form's From opens; null with no profile.</param>
/// <param name="LastDay">
/// The last day of an ended programme (<see cref="EndedProgrammes.Of" />), where the form's To opens; null while it runs.
/// </param>
/// <param name="ProfileId">
/// The preferred profile's id, which the staff trail's middle crumb links to (M4); null when the person holds none.
/// </param>
public sealed record ExportSubjectDto(
    string TraineeUserId,
    string Name,
    string? InstitutionName,
    string? CurriculumName,
    string? CurriculumVersion,
    int? TrainingYear,
    DateOnly? ProgrammeStartDate,
    DateOnly? LastDay,
    int? ProfileId);

public sealed class GetExportSubjectQueryHandler : IRequestHandler<GetExportSubjectQuery, ExportSubjectDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _clock;

    public GetExportSubjectQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users, TimeProvider clock)
    {
        _dbContext = dbContext;
        _users = users;
        _clock = clock;
    }

    public async Task<ExportSubjectDto?> Handle(GetExportSubjectQuery request, CancellationToken cancellationToken)
    {
        var own = request.TraineeUserId is null;
        var traineeUserId = own
            ? request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            : request.TraineeUserId;
        if (string.IsNullOrWhiteSpace(traineeUserId))
        {
            return null;
        }

        // The export command's own check, first, so nothing is read about a person the caller may not export (T379, F1).
        if (!await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, traineeUserId, cancellationToken))
        {
            return null;
        }

        var profile = await TraineeScopeResolver.PreferredProfiles(_dbContext)
            .AsNoTracking()
            .Where(candidate => candidate.UserId == traineeUserId)
            .SingleOrDefaultAsync(cancellationToken);

        // F6: the own export with no profile has no subject; nor does an id no account holds.
        if (profile is null && own)
        {
            return null;
        }

        var contacts = await _users.GetContactsAsync([traineeUserId], cancellationToken);
        if (contacts is null || !contacts.TryGetValue(traineeUserId, out var contact))
        {
            return null;
        }

        var fullName = $"{contact.FirstName} {contact.LastName}".Trim();
        var name = fullName.Length == 0 ? contact.Email : fullName;

        if (profile is null)
        {
            return new ExportSubjectDto(traineeUserId, name, null, null, null, null, null, null, null);
        }

        var curriculum = await _dbContext.Set<Curriculum>()
            .AsNoTracking()
            .Where(candidate => candidate.Id == profile.CurriculumId)
            .Select(candidate => new { candidate.Name, candidate.Version })
            .SingleOrDefaultAsync(cancellationToken);
        var institutionName = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(institution => institution.Id == profile.InstitutionId)
            .Select(institution => institution.Name)
            .SingleOrDefaultAsync(cancellationToken);

        // The end, read once (build-contracts § 0): the last day and the year the programme ended in; a running programme's
        // year is today's on the South African calendar (T325).
        var ended = EndedProgrammes.Of(profile);
        var trainingYear = ended is not null ? ended.TrainingYearAtEnd : profile.GetStage(QuotaCalendar.Today(_clock));

        return new ExportSubjectDto(
            traineeUserId,
            name,
            institutionName,
            curriculum?.Name,
            curriculum?.Version,
            trainingYear,
            profile.ProgrammeStartDate,
            ended?.EndedOn,
            profile.Id);
    }
}
