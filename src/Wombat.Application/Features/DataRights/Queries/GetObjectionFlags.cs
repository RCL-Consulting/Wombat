using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.DataRights.Queries;

/// <summary>The person's reminder-email preference (T387: the optional-processing flag is gone, T318).</summary>
public sealed record ObjectionFlagsDto(
    bool OptOutOfDigestEmails);

public sealed record GetObjectionFlagsQuery(
    ClaimsPrincipal Principal) : IRequest<ObjectionFlagsDto>;

/// <summary>
/// Handler lives in Infrastructure because it reads WombatIdentityUser directly.
/// </summary>
public interface IObjectionFlagReader
{
    Task<ObjectionFlagsDto> GetAsync(string userId, CancellationToken cancellationToken);
}

public sealed class GetObjectionFlagsQueryHandler : IRequestHandler<GetObjectionFlagsQuery, ObjectionFlagsDto>
{
    private readonly IObjectionFlagReader _flagReader;

    public GetObjectionFlagsQueryHandler(IObjectionFlagReader flagReader)
    {
        _flagReader = flagReader;
    }

    public async Task<ObjectionFlagsDto> Handle(GetObjectionFlagsQuery request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        return await _flagReader.GetAsync(userId, cancellationToken);
    }
}
