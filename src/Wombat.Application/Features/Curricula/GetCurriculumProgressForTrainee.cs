using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;
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
/// <param name="AsOf">The day to read progress for. Defaults to today in South Africa; tests pin it.</param>
public sealed record GetCurriculumProgressForTraineeQuery(string TraineeUserId, DateOnly? AsOf = null)
    : IRequest<TraineeCurriculumProgressSummaryDto?>;

public sealed class GetCurriculumProgressForTraineeQueryValidator
    : AbstractValidator<GetCurriculumProgressForTraineeQuery>
{
    public GetCurriculumProgressForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
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

    public Task<TraineeCurriculumProgressSummaryDto?> Handle(
        GetCurriculumProgressForTraineeQuery request, CancellationToken cancellationToken)
        => TraineeQuotaProgressReader.ReadAsync(
            _dbContext,
            request.TraineeUserId.Trim(),
            request.AsOf ?? QuotaCalendar.Today(),
            cancellationToken);
}
