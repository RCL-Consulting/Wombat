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

        // Remitted with no replacement rationale and no attendance -> validator must reject before the handler. (Whether
        // the replacement needs a category is the review's type, which the domain judges: T131 slice 5.)
        var command = new ResolveAppealCommand(
            ReviewId: 1,
            Outcome: CommitteeAppealOutcome.Remitted,
            RemittedCategory: null,
            RemittedRationale: null,
            RemittedConditions: null,
            PresentUserIds: null,
            Principal: new ClaimsPrincipal(new ClaimsIdentity()));

        var act = async () => await behavior.Handle(
            command,
            () => { handlerCalled = true; return Task.FromResult<CommitteeReviewDetailDto>(null!); },
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        handlerCalled.Should().BeFalse();
    }

    /// <summary>
    /// An outcome the enum does not define is refused before the handler, so nothing is resolved (T307). 1 was Upheld,
    /// which T307 removed (D51): a caller still sending it resolves nothing.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(99)]
    public async Task Handle_AnUndefinedOutcome_IsRefused_AndTheHandlerIsNotCalled(int outcome)
    {
        var behavior = CreateBehavior();
        var handlerCalled = false;

        var command = new ResolveAppealCommand(
            ReviewId: 1,
            Outcome: (CommitteeAppealOutcome)outcome,
            RemittedCategory: null,
            RemittedRationale: null,
            RemittedConditions: null,
            PresentUserIds: null,
            Principal: new ClaimsPrincipal(new ClaimsIdentity()));

        var act = async () => await behavior.Handle(
            command,
            () => { handlerCalled = true; return Task.FromResult<CommitteeReviewDetailDto>(null!); },
            CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().ContainSingle(error => error.PropertyName == nameof(ResolveAppealCommand.Outcome));
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
            PresentUserIds: null,
            Principal: new ClaimsPrincipal(new ClaimsIdentity()));

        await behavior.Handle(
            command,
            () => { handlerCalled = true; return Task.FromResult<CommitteeReviewDetailDto>(null!); },
            CancellationToken.None);

        handlerCalled.Should().BeTrue();
    }
}
