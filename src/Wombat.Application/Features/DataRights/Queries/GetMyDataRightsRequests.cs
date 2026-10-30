using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.DataRights.Queries;

/// <summary>
/// My data rights (T387, S3, S10, T319): the requester's own requests, newest first, each with its answer (the decision,
/// its day and its note, never who decided), and the requester's institution by name.
/// </summary>
public sealed record GetMyDataRightsRequestsQuery(
    ClaimsPrincipal Principal) : IRequest<MyDataRightsDto>;

public sealed class GetMyDataRightsRequestsQueryValidator : AbstractValidator<GetMyDataRightsRequestsQuery>
{
    public GetMyDataRightsRequestsQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetMyDataRightsRequestsQueryHandler : IRequestHandler<GetMyDataRightsRequestsQuery, MyDataRightsDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMyDataRightsRequestsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MyDataRightsDto> Handle(GetMyDataRightsRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        var requests = await _dbContext.Set<DataRightsRequest>()
            .AsNoTracking()
            .Where(r => r.RequesterUserId == userId)
            .OrderByDescending(r => r.RequestedOn)
            .ToListAsync(cancellationToken);

        // The records corrected on each Rectification, by one grouped read of the requests' corrections (T387, S10, M09:
        // "Corrected: Account › Last name."), never one read a request. Each record once, in the order first recorded: the
        // answer says what was corrected, not how many times.
        var rectificationIds = requests
            .Where(r => r.Type == DataRightsRequestType.Rectification)
            .Select(r => r.Id)
            .ToArray();
        var correctedRecords = rectificationIds.Length == 0
            ? new Dictionary<Guid, IReadOnlyList<string>>()
            : (await _dbContext.Set<DataRightsRectification>()
                .AsNoTracking()
                .Where(c => rectificationIds.Contains(c.RequestId))
                .OrderBy(c => c.AppliedOn)
                .ThenBy(c => c.Id)
                .Select(c => new { c.RequestId, c.TargetType })
                .ToListAsync(cancellationToken))
                .GroupBy(c => c.RequestId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<string>)group.Select(c => c.TargetType).Distinct(StringComparer.Ordinal).ToArray());

        // Where the caller is, by name, for "Staff at ‹institution› decide it" (S3); null for an account set to none (M18).
        // The claim, as Submit stamps a new request from it, so the sentence names where the next request will go.
        var institutionName = request.Principal.GetInstitutionId() is { } institutionId
            ? await _dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(institution => institution.Id == institutionId)
                .Select(institution => institution.Name)
                .SingleOrDefaultAsync(cancellationToken)
            : null;

        return new MyDataRightsDto(
            institutionName,
            requests
                .Select(r => new MyDataRightsRequestDto(
                    r.Id,
                    r.RequestedOn,
                    r.Type,
                    r.Status,
                    r.DecidedOn,
                    r.DecisionNote,
                    r.CompletedOn,
                    correctedRecords.GetValueOrDefault(r.Id) ?? []))
                .ToArray());
    }
}
