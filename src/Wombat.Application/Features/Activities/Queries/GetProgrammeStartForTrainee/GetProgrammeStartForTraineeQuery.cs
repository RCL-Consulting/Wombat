using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;

namespace Wombat.Application.Features.Activities.Queries.GetProgrammeStartForTrainee;

/// <summary>
/// The day a trainee's programme started, as the encounter-date bound reads it (T192): the <c>ProgrammeStartDate</c> of
/// their preferred profile (<see cref="TraineeScopeResolver.PreferredProfiles" />), the profile credit and the write
/// path's <c>EncounterDateGate</c> pick. Null when the trainee holds no profile, or when the caller may not read about
/// them.
/// </summary>
/// <remarks>
/// <para>
/// For the activity form's hint and nothing else. On a type that can credit, an encounter dated before this day is
/// refused (T160), so while such a date is typed the form says so instead of warning that the filing will be recorded as
/// late, which would tell the author it "can still be filed". The server stays the rule: a caller who gets null here
/// sees no hint, and the refusal still comes.
/// </para>
/// <para>
/// Read through <see cref="TraineeScopeResolver.MayReadAsync" />, as every other read about a trainee is (T113). A
/// refusal answers null, the answer for a trainee with no profile, so that asking about an id confirms nothing. The
/// author of a filing is the trainee on every page that files one today, and a trainee reads their own start.
/// </para>
/// </remarks>
public sealed record GetProgrammeStartForTraineeQuery(
    string TraineeUserId,
    ClaimsPrincipal Principal) : IRequest<DateOnly?>;

public sealed class GetProgrammeStartForTraineeQueryValidator : AbstractValidator<GetProgrammeStartForTraineeQuery>
{
    public GetProgrammeStartForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetProgrammeStartForTraineeQueryHandler
    : IRequestHandler<GetProgrammeStartForTraineeQuery, DateOnly?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetProgrammeStartForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DateOnly?> Handle(GetProgrammeStartForTraineeQuery request, CancellationToken cancellationToken)
    {
        var traineeUserId = request.TraineeUserId.Trim();

        if (!await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, traineeUserId, cancellationToken))
        {
            return null;
        }

        // CreditTargetResolver.PickProfileAsync's pick, which the gate reads the bound from: a different profile here
        // would hint at one start while the server refused by another.
        return await TraineeScopeResolver.PreferredProfiles(_dbContext)
            .AsNoTracking()
            .Where(profile => profile.UserId == traineeUserId)
            .Select(profile => (DateOnly?)profile.ProgrammeStartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
