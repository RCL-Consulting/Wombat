using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Queries.ListDecidedByYou;

/// <summary>
/// What the caller decided, newest first, a page at a time (T350, round 1, Q1; note 6): the Activity inbox's "Decided by
/// you". The read is <see cref="DecidedByYou" />, whose first page of five is the Assessor's Home "Recent decisions".
/// </summary>
/// <param name="Page">The page to serve, from 1; brought within the list's pages, so a page past the end is the last.</param>
/// <param name="PageSize">Rows per page, 20 by default; brought within 1 and <see cref="DecidedByYou.MaxPageSize" />.</param>
public sealed record ListDecidedByYouQuery(ClaimsPrincipal Principal, int Page = 1, int PageSize = DecidedByYou.DefaultPageSize)
    : IRequest<ActivityListPageDto>;

public sealed class ListDecidedByYouQueryHandler : IRequestHandler<ListDecidedByYouQuery, ActivityListPageDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public ListDecidedByYouQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public Task<ActivityListPageDto> Handle(ListDecidedByYouQuery request, CancellationToken cancellationToken)
        => DecidedByYou.ReadAsync(_dbContext, _users, request.Principal, request.Page, request.PageSize, cancellationToken);
}
