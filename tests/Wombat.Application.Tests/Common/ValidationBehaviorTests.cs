using System.Security.Claims;
using FluentAssertions;
using FluentValidation;
using Wombat.Application.Common.Behaviours;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Tests.Common;

public sealed class ValidationBehaviorTests
{
    // Uses the real ResolveAppealCommandValidator (the validator that, before T088, was registered
    // but never executed — letting the bad Remitted request reach the domain layer in the replay).
    private static ValidationBehavior<ResolveAppealCommand, CommitteeReviewDetailDto> CreateBehavior()
        => new([new ResolveAppealCommandValidator()]);

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationExceptionAndDoesNotCallHandler()
    {
        var behavior = CreateBehavior();
        var handlerCalled = false;

        // Remitted with no replacement category/rationale -> validator must reject before the handler.
        var command = new ResolveAppealCommand(
            ReviewId: 1,
            Outcome: CommitteeAppealOutcome.Remitted,
            RemittedCategory: null,
            RemittedRationale: null,
            RemittedConditions: null,
            Principal: new ClaimsPrincipal(new ClaimsIdentity()));

        var act = async () => await behavior.Handle(
            command,
            () => { handlerCalled = true; return Task.FromResult<CommitteeReviewDetailDto>(null!); },
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        handlerCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsHandler()
    {
        var behavior = CreateBehavior();
        var handlerCalled = false;

        // Dismissed outcome has no replacement requirement; ReviewId > 0; principal present.
        var command = new ResolveAppealCommand(
            ReviewId: 1,
            Outcome: CommitteeAppealOutcome.Dismissed,
            RemittedCategory: null,
            RemittedRationale: null,
            RemittedConditions: null,
            Principal: new ClaimsPrincipal(new ClaimsIdentity()));

        await behavior.Handle(
            command,
            () => { handlerCalled = true; return Task.FromResult<CommitteeReviewDetailDto>(null!); },
            CancellationToken.None);

        handlerCalled.Should().BeTrue();
    }
}
