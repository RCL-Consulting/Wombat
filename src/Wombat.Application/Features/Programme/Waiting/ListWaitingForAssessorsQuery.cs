using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;

namespace Wombat.Application.Features.Programme.Waiting;

/// <summary>
/// Waiting for assessors (T358, flow 06; Q3, E3, E4): what waits for a named assessor in the programme of the role read
/// as (<paramref name="ActingRole" />, <c>ProgrammeReadAs.RoleFor</c>), oldest first, one page of it. The page, Home's
/// card (<see cref="ListWaitingForAssessorsQueryHandler.HomeRows" />) and the registrar page's section
/// (<paramref name="SubjectUserId" /> set) send this one query (E4).
/// </summary>
/// <remarks>
/// Admits a <see cref="ProgrammeScope.WaitingRoles" /> role; with <paramref name="SubjectUserId" /> set, any
/// <see cref="ProgrammeScope.RosterRoles" /> role, so a Committee member reads one registrar's waiting requests (with
/// <see cref="WaitingForAssessorsDto.MayRemind" /> false). Null for any other role, and wherever the scope is
/// (<see cref="ProgrammeScope.ResolveAsync" />): the page reads as not found, never as a refusal.
/// </remarks>
public sealed record ListWaitingForAssessorsQuery(
    ClaimsPrincipal Principal,
    string ActingRole,
    bool OverdueOnly = false,
    string? WithUserId = null,
    string? SubjectUserId = null,
    int Page = 1,
    int PageSize = 20) : IRequest<WaitingForAssessorsDto?>;

/// <summary>
/// One page of 1 to 100 rows, from the first (T358, build review R3), as Programme trainees' query is bounded
/// (<c>ListProgrammeTraineesQueryValidator</c>).
/// </summary>
public sealed class ListWaitingForAssessorsQueryValidator : AbstractValidator<ListWaitingForAssessorsQuery>
{
    public ListWaitingForAssessorsQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class ListWaitingForAssessorsQueryHandler : IRequestHandler<ListWaitingForAssessorsQuery, WaitingForAssessorsDto?>
{
    /// <summary>How many rows a Home's Waiting for assessors card asks for: five, then "n more wait in …" (R2-Home).</summary>
    public const int HomeRows = 5;

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly IReminderRecipients _recipients;
    private readonly DashboardThresholds _thresholds;
    private readonly TimeProvider _clock;

    public ListWaitingForAssessorsQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        IReminderRecipients recipients,
        IOptions<DashboardThresholds> thresholds,
        TimeProvider clock)
    {
        _dbContext = dbContext;
        _users = users;
        _recipients = recipients;
        _thresholds = thresholds.Value;
        _clock = clock;
    }

    public async Task<WaitingForAssessorsDto?> Handle(ListWaitingForAssessorsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var admitted = request.SubjectUserId is null ? ProgrammeScope.WaitingRoles : ProgrammeScope.RosterRoles;
        if (!admitted.Contains(request.ActingRole, StringComparer.Ordinal))
        {
            return null;
        }

        var scope = await ProgrammeScope.ResolveAsync(_dbContext, request.Principal, request.ActingRole, cancellationToken);
        if (scope is null)
        {
            return null;
        }

        return await WaitingForAssessorsReader.ReadAsync(
            _dbContext,
            _users,
            _recipients,
            _clock,
            _thresholds,
            request.Principal,
            scope,
            new WaitingForAssessorsFilter(request.OverdueOnly, request.WithUserId, request.SubjectUserId),
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
