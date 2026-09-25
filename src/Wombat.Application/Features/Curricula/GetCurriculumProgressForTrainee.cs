using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Trainee;

namespace Wombat.Application.Features.Curricula;

/// <summary>
/// Full curriculum-credit view for a trainee's own portfolio progress page (T130): every curriculum item in the
/// trainee's curriculum, each read against its target for the window containing today (a semester or an
/// academic year), with the College's D14 exemption applied, and the previous window's result. Items with no
/// credit yet are included. The trainee dashboard shows a summary of the same read model via
/// <see cref="GetTraineeDashboardSummaryQuery"/>.
/// </summary>
/// <remarks>
/// A trainee whose programme has ended (completed or withdrawn, and not since admitted to another) gets the programme
/// they ended on, read as on its last day with every period back to its start, and marked as ended
/// (<see cref="TraineeCurriculumProgressSummaryDto.Ended" />, T252): <see cref="TraineeQuotaProgressReader.ReadAsync" />.
/// </remarks>
/// <param name="Principal">
/// The caller. Null comes back for anyone who may not read about this trainee
/// (<see cref="TraineeScopeResolver.MayReadAsync" />), exactly as it does for a trainee with no profile at all, so the
/// answer never confirms that the id names somebody. Until T113 this query answered on the caller-supplied id alone.
/// It sits before <paramref name="AsOf" /> because the date must keep its default and the caller must not have one.
/// </param>
/// <param name="AsOf">
/// The day to read progress for. Defaults to today in South Africa; tests pin it. An ended programme is read as on its
/// last day when that is earlier.
/// </param>
public sealed record GetCurriculumProgressForTraineeQuery(
    string TraineeUserId,
    ClaimsPrincipal Principal,
    DateOnly? AsOf = null)
    : IRequest<TraineeCurriculumProgressSummaryDto?>;

public sealed class GetCurriculumProgressForTraineeQueryValidator
    : AbstractValidator<GetCurriculumProgressForTraineeQuery>
{
    public GetCurriculumProgressForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetCurriculumProgressForTraineeQueryHandler
    : IRequestHandler<GetCurriculumProgressForTraineeQuery, TraineeCurriculumProgressSummaryDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCurriculumProgressForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TraineeCurriculumProgressSummaryDto?> Handle(
        GetCurriculumProgressForTraineeQuery request, CancellationToken cancellationToken)
    {
        // Trimmed once, so the id that is authorised is the id that is read.
        var traineeUserId = request.TraineeUserId.Trim();

        if (!await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, traineeUserId, cancellationToken))
        {
            return null;
        }

        return await TraineeQuotaProgressReader.ReadAsync(
            _dbContext,
            traineeUserId,
            request.AsOf ?? QuotaCalendar.Today(),
            cancellationToken);
    }
}
