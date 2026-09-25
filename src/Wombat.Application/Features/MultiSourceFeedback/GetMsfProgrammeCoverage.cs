using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// Per programme, per EPA and per semester, how many of the programme's trainees a released multi-source feedback campaign
/// covered: "n of m trainees". For a coordinator planning campaigns. (T210)
/// </summary>
/// <remarks>
/// <para>
/// <b>It is the trainees' own cards, counted.</b> Each trainee's coverage is read by the rule their grid is read by
/// (<see cref="MsfSemesterCoverage" />, T168 and T186): an EPA of their list is covered in a semester when a released MSF
/// campaign about them closed in it and its evidence rows carry the EPA. A trainee counts as covered for an EPA in a
/// semester exactly when their card (<see cref="GetMsfCoverageForTraineeQuery" />) says so. Nothing here is a target or a
/// shortfall, for the reason the card gives none: the College's cadence is not confirmed from Annexure B (D8, D9).
/// </para>
/// <para>
/// <b>Whose trainees.</b> Those whose record the caller may read (<see cref="TraineeScopeResolver.ReadableAsync" />, T113's
/// ladder): a Coordinator's own institution's, everyone's for a global Administrator. Someone who holds Trainee reads only
/// their own record whatever other role they hold (T185's rung), and their own card is on My progress, so they are shown
/// no programme. Of those, the trainees on a programme now: current trainees (<see cref="TraineeScopeResolver.WhichAreCurrentAsync" />,
/// T238), whose profile is active and whose account still holds Trainee. A trainee who has completed or left is on no
/// programme to plan a campaign for, and an erased trainee's pseudonym, whose profile stays active under an id no account
/// holds, is no trainee to count in "n of m".
/// </para>
/// <para>
/// <b>What a programme is.</b> A curriculum as one institution follows it: its trainees there, and its EPAs in force, the
/// College's and that institution's own (<see cref="MsfSemesterCoverage.OnListOf" />). A curriculum row is shared by every
/// adopting institution, so an Administrator sees one programme per institution that follows it.
/// </para>
/// <para>
/// <b>Which semesters.</b> The one containing the day read as today, and the one before it: what each trainee's progress
/// page shows. A trainee counts in a semester only once their programme has started by its last day (31 December for
/// semester 2, D40), the rule by which their own card leaves out the earlier semester (<see cref="MsfSemesterCoverage.CountsIn" />,
/// the one implementation both read).
/// </para>
/// </remarks>
/// <param name="AsOf">The day read as today. Defaults to today in South Africa. The page leaves it null; it pins the
/// calendar in a test.</param>
public sealed record GetMsfProgrammeCoverageQuery(ClaimsPrincipal Principal, DateOnly? AsOf = null)
    : IRequest<MsfProgrammeCoverageDto>
{
    /// <summary>
    /// Whether this caller is shown no programme, whatever else they hold: anyone who holds Trainee
    /// (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185's rung), who reads no other trainee's record. The handler
    /// answers them with no programme, and the page says why instead of asking.
    /// </summary>
    public static bool ShowsNoProgrammeTo(ClaimsPrincipal principal) => TraineeScopeResolver.ActsAsTrainee(principal);
}

public sealed class GetMsfProgrammeCoverageQueryValidator : AbstractValidator<GetMsfProgrammeCoverageQuery>
{
    public GetMsfProgrammeCoverageQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetMsfProgrammeCoverageQueryHandler
    : IRequestHandler<GetMsfProgrammeCoverageQuery, MsfProgrammeCoverageDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetMsfProgrammeCoverageQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<MsfProgrammeCoverageDto> Handle(GetMsfProgrammeCoverageQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        var today = request.AsOf ?? QuotaCalendar.Today();
        var current = AcademicPeriod.Containing(today);
        IReadOnlyList<AcademicPeriod> periods = current.Previous() is { } previous ? [previous, current] : [current];

        var periodDtos = periods
            .Select(period => new MsfProgrammeCoveragePeriodDto(
                period.Year,
                period.Semester,
                QuotaText.SemesterName(period),
                QuotaText.Months(period),
                period.Start,
                period.End,
                HasEnded: period.End < today))
            .ToArray();

        // A programme view is about other trainees, and someone who holds Trainee reads nobody else's record (T185).
        if (GetMsfProgrammeCoverageQuery.ShowsNoProgrammeTo(request.Principal))
        {
            return new MsfProgrammeCoverageDto(today, periodDtos, []);
        }

        var readable = await TraineeScopeResolver.ReadableAsync(_dbContext, request.Principal, cancellationToken);
        if (readable.Count == 0)
        {
            return new MsfProgrammeCoverageDto(today, periodDtos, []);
        }

        // Of the readable trainees, the current ones (T238): the rule every list of the programme's trainees keeps to.
        var readableIds = (await TraineeScopeResolver.WhichAreCurrentAsync(_dbContext, _users, readable.Keys, cancellationToken))
            .ToArray();
        if (readableIds.Length == 0)
        {
            return new MsfProgrammeCoverageDto(today, periodDtos, []);
        }

        var profiles = await TraineeScopeResolver.ActiveProfiles(_dbContext)
            .AsNoTracking()
            .Where(profile => readableIds.Contains(profile.UserId))
            .Select(profile => new
            {
                profile.UserId,
                profile.InstitutionId,
                profile.CurriculumId,
                profile.ProgrammeStartDate
            })
            .ToListAsync(cancellationToken);

        if (profiles.Count == 0)
        {
            return new MsfProgrammeCoverageDto(today, periodDtos, []);
        }

        var institutionIds = profiles.Select(profile => profile.InstitutionId).Distinct().ToArray();
        var institutionNames = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(institution => institutionIds.Contains(institution.Id))
            .ToDictionaryAsync(institution => institution.Id, institution => institution.Name, cancellationToken);

        var curriculumIds = profiles.Select(profile => profile.CurriculumId).Distinct().ToArray();
        var curricula = await _dbContext.Set<Curriculum>()
            .AsNoTracking()
            .Where(curriculum => curriculumIds.Contains(curriculum.Id))
            .Select(curriculum => new { curriculum.Id, curriculum.Name, curriculum.Version })
            .ToDictionaryAsync(curriculum => curriculum.Id, cancellationToken);

        // The one rule each trainee's own card is read by, for every trainee at once.
        var covered = (await MsfSemesterCoverage.ReadAsync(
                _dbContext, profiles.Select(profile => profile.UserId).ToArray(), periods, cancellationToken))
            .Select(entry => (entry.SubjectUserId, entry.EpaId, entry.Period))
            .ToHashSet();

        var names = await UserDisplayNames.ResolveAsync(_users, profiles.Select(profile => profile.UserId), cancellationToken);

        // Every item on the programmes' curricula in one read, however many programmes: a superset of each programme's list,
        // since a list never leaves its curriculum. Each programme's list is then drawn from it by the one definition of a
        // trainee's list (OnListOf, in memory), so this read is a bound on what is fetched, not a second rule.
        var itemsOnCurricula = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Include(item => item.Epa)
            .Where(item => curriculumIds.Contains(item.CurriculumId))
            .ToListAsync(cancellationToken);

        var programmes = new List<MsfProgrammeDto>();
        foreach (var programme in profiles.GroupBy(profile => (profile.InstitutionId, profile.CurriculumId)))
        {
            var (institutionId, curriculumId) = programme.Key;

            // A programme's trainees share one list.
            var items = itemsOnCurricula
                .AsQueryable()
                .OnListOf(curriculumId, institutionId)
                .Select(item => new
                {
                    item.Id,
                    item.EpaId,
                    EpaCode = item.Epa.Code,
                    EpaTitle = item.Epa.Title,
                    IsLocal = item.OwningInstitutionId != null
                })
                .AsEnumerable()
                .OrderBy(item => item.EpaCode, StringComparer.Ordinal)
                .ThenBy(item => item.Id)
                .ToList();

            var trainees = programme
                .Select(profile => new
                {
                    profile.UserId,
                    Name = names.NameOf(profile.UserId),
                    profile.ProgrammeStartDate
                })
                .OrderBy(trainee => trainee.Name, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(trainee => trainee.UserId, StringComparer.Ordinal)
                .ToList();

            // Who counts in each semester: the trainees whose programme had started by its last day, the rule by which
            // each one's own card leaves out the earlier semester.
            var started = periods.ToDictionary(
                period => period,
                period => trainees.Where(trainee => MsfSemesterCoverage.CountsIn(trainee.ProgrammeStartDate, period)).ToList());

            var epas = items
                .Select(item => new MsfProgrammeEpaDto(
                    item.Id,
                    item.EpaId,
                    item.EpaCode,
                    item.EpaTitle,
                    item.IsLocal,
                    periods
                        .Select(period => new MsfProgrammeEpaPeriodDto(
                            period.Year,
                            period.Semester,
                            TraineesCovered: started[period].Count(trainee => covered.Contains((trainee.UserId, item.EpaId, period))),
                            Trainees: started[period].Count))
                        .ToArray()))
                .ToArray();

            var traineeDtos = trainees
                .Select(trainee => new MsfProgrammeTraineeDto(
                    trainee.UserId,
                    trainee.Name,
                    trainee.ProgrammeStartDate,
                    periods
                        .Select(period => MsfSemesterCoverage.CountsIn(trainee.ProgrammeStartDate, period)
                            ? new MsfProgrammeTraineePeriodDto(
                                period.Year,
                                period.Semester,
                                HadStarted: true,
                                EpasCovered: items.Count(item => covered.Contains((trainee.UserId, item.EpaId, period))))
                            : new MsfProgrammeTraineePeriodDto(period.Year, period.Semester, HadStarted: false, EpasCovered: 0))
                        .ToArray()))
                .ToArray();

            var curriculum = curricula.GetValueOrDefault(curriculumId);
            programmes.Add(new MsfProgrammeDto(
                institutionId,
                institutionNames.GetValueOrDefault(institutionId) ?? $"Institution {institutionId}",
                curriculumId,
                curriculum?.Name ?? $"Curriculum {curriculumId}",
                curriculum?.Version ?? string.Empty,
                periods
                    .Select(period => new MsfProgrammePeriodDto(period.Year, period.Semester, started[period].Count))
                    .ToArray(),
                epas,
                traineeDtos));
        }

        return new MsfProgrammeCoverageDto(
            today,
            periodDtos,
            programmes
                .OrderBy(programme => programme.InstitutionName, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(programme => programme.CurriculumName, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(programme => programme.CurriculumVersion, StringComparer.Ordinal)
                .ThenBy(programme => programme.CurriculumId)
                .ToArray());
    }
}

/// <summary>
/// The MSF coverage of every programme whose trainees the caller may read (<see cref="GetMsfProgrammeCoverageQuery" />,
/// T210): none for a caller who reads no other trainee.
/// </summary>
/// <param name="AsOf">The day read as today.</param>
/// <param name="Periods">The semesters read, oldest first: the one before today's, and today's.</param>
/// <param name="Programmes">By institution name, then curriculum.</param>
public sealed record MsfProgrammeCoverageDto(
    DateOnly AsOf,
    IReadOnlyList<MsfProgrammeCoveragePeriodDto> Periods,
    IReadOnlyList<MsfProgrammeDto> Programmes);

/// <summary>A semester read, as a reader names it.</summary>
/// <param name="Name">"Semester 1, 2026".</param>
/// <param name="Months">"January to June", on the College's calendar.</param>
/// <param name="End">The last day a campaign's close is placed in, and the day by which a trainee must have started to count.</param>
/// <param name="HasEnded">
/// <see cref="End" /> is behind the day read as today. A campaign that closed in it can still be released, so an ended
/// semester's count can still grow, and nothing may word it as final.
/// </param>
public sealed record MsfProgrammeCoveragePeriodDto(
    int Year,
    int Semester,
    string Name,
    string Months,
    DateOnly Start,
    DateOnly End,
    bool HasEnded);

/// <summary>A curriculum as one institution follows it, with its trainees there.</summary>
/// <param name="Periods">How many of its trainees count in each semester, in the order of <see cref="MsfProgrammeCoverageDto.Periods" />.</param>
/// <param name="Epas">One row per curriculum item on the programme's list, in EPA code order.</param>
/// <param name="Trainees">One row per trainee on the programme now, by name.</param>
public sealed record MsfProgrammeDto(
    int InstitutionId,
    string InstitutionName,
    int CurriculumId,
    string CurriculumName,
    string CurriculumVersion,
    IReadOnlyList<MsfProgrammePeriodDto> Periods,
    IReadOnlyList<MsfProgrammeEpaDto> Epas,
    IReadOnlyList<MsfProgrammeTraineeDto> Trainees)
{
    /// <summary>How many of the programme's trainees count in one semester, or null when the semester is not read.</summary>
    public MsfProgrammePeriodDto? For(int year, int semester)
        => Periods.FirstOrDefault(period => period.Year == year && period.Semester == semester);
}

/// <summary>One semester of a programme.</summary>
/// <param name="Trainees">The programme's trainees whose programme had started by the semester's last day.</param>
public sealed record MsfProgrammePeriodDto(int Year, int Semester, int Trainees);

/// <summary>One EPA of a programme, with its count in each semester.</summary>
/// <param name="IsLocal">The item is the institution's own, not one of the College's.</param>
public sealed record MsfProgrammeEpaDto(
    int CurriculumItemId,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    bool IsLocal,
    IReadOnlyList<MsfProgrammeEpaPeriodDto> Periods)
{
    /// <summary>This EPA's count in one semester, or null when the semester is not read.</summary>
    public MsfProgrammeEpaPeriodDto? For(int year, int semester)
        => Periods.FirstOrDefault(period => period.Year == year && period.Semester == semester);
}

/// <summary>One EPA in one semester: of the trainees who count in it, how many a released campaign covered.</summary>
public sealed record MsfProgrammeEpaPeriodDto(int Year, int Semester, int TraineesCovered, int Trainees);

/// <summary>One trainee of a programme, with how many of its EPAs were covered for them in each semester.</summary>
public sealed record MsfProgrammeTraineeDto(
    string UserId,
    string Name,
    DateOnly ProgrammeStartDate,
    IReadOnlyList<MsfProgrammeTraineePeriodDto> Periods)
{
    /// <summary>This trainee's count in one semester, or null when the semester is not read.</summary>
    public MsfProgrammeTraineePeriodDto? For(int year, int semester)
        => Periods.FirstOrDefault(period => period.Year == year && period.Semester == semester);
}

/// <summary>One trainee in one semester.</summary>
/// <param name="HadStarted">Their programme had started by the semester's last day, so they count in it.</param>
/// <param name="EpasCovered">
/// How many of the programme's EPAs a released campaign covered for them in it, their card's count; 0 when they had not
/// started, as they count in no EPA's figure for the semester.
/// </param>
public sealed record MsfProgrammeTraineePeriodDto(int Year, int Semester, bool HadStarted, int EpasCovered);
