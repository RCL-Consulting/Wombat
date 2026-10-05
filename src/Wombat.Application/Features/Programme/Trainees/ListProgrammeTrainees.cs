using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;

namespace Wombat.Application.Features.Programme.Trainees;

/// <summary>
/// Programme trainees (<c>/programme/trainees</c>; T358, flow 06, Q1): the current registrars in the scope of the role the
/// page reads as (<paramref name="ActingRole" />, D2, E4), filtered and paged. Null when that role gives no scope
/// (<see cref="ProgrammeScope.ResolveAsync" />): the page then says "Page not found".
/// </summary>
/// <param name="ActingRole">The role read as, one of <see cref="ProgrammeScope.RosterRoles" />.</param>
/// <param name="ShortOnEpaId">Only registrars short on this EPA this window (<c>?short=</c>).</param>
/// <param name="TrainingYear">Only registrars in this training year (<c>?year=</c>).</param>
/// <param name="NothingFiled">Only registrars with nothing filed in 30 days (<c>?filed=true</c>, E5).</param>
/// <param name="Page">1-based; a page past the last reads the last.</param>
/// <param name="PageSize">20 by default (R2-Trainees t6).</param>
/// <param name="AsOf">The day to read for; today on the South African calendar by default. Tests pin it.</param>
public sealed record ListProgrammeTraineesQuery(
    ClaimsPrincipal Principal,
    string ActingRole,
    int? ShortOnEpaId = null,
    int? TrainingYear = null,
    bool NothingFiled = false,
    int Page = 1,
    int PageSize = 20,
    DateOnly? AsOf = null) : IRequest<ProgrammeTraineesDto?>;

public sealed class ListProgrammeTraineesQueryValidator : AbstractValidator<ListProgrammeTraineesQuery>
{
    public ListProgrammeTraineesQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class ListProgrammeTraineesQueryHandler : IRequestHandler<ListProgrammeTraineesQuery, ProgrammeTraineesDto?>
{
    /// <summary>How many registrars Home's Registrars and Nothing filed cards show before "3 more in Programme trainees.".</summary>
    public const int HomeRows = 5;

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _clock;

    public ListProgrammeTraineesQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users, TimeProvider clock)
    {
        _dbContext = dbContext;
        _users = users;
        _clock = clock;
    }

    public async Task<ProgrammeTraineesDto?> Handle(ListProgrammeTraineesQuery request, CancellationToken cancellationToken)
    {
        var scope = await ProgrammeScope.ResolveAsync(_dbContext, request.Principal, request.ActingRole, cancellationToken);
        if (scope is null)
        {
            return null;
        }

        var read = await ProgrammeRosterReader.ReadAsync(
            _dbContext,
            _users,
            scope,
            new ProgrammeTraineesFilter(request.ShortOnEpaId, request.TrainingYear, request.NothingFiled),
            request.AsOf ?? QuotaCalendar.Today(_clock),
            cancellationToken);

        var pageSize = Math.Max(1, request.PageSize);
        var lastPage = Math.Max(1, (read.Rows.Count + pageSize - 1) / pageSize);
        var pageNumber = Math.Clamp(request.Page, 1, lastPage);

        return new ProgrammeTraineesDto(
            read,
            read.Rows.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            read.Rows.Count,
            pageNumber,
            pageSize);
    }
}
