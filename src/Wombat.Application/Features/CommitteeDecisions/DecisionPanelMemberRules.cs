using FluentValidation;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// The one set of rules a panel's member list is held to, on create and on update alike (T165, D46):
/// <see cref="DecisionPanelComposition" />, stated as validation messages.
/// </summary>
internal static class DecisionPanelMemberRules
{
    /// <summary>
    /// A named member at most once each, exactly one chair, and at least <see cref="CommitteeReview.Quorum" /> members.
    /// </summary>
    /// <remarks>
    /// Stops at the first rule that fails, so a list that names nobody is told that, not also that it has no chair.
    /// </remarks>
    public static IRuleBuilderOptions<T, IReadOnlyList<DecisionPanelMemberInput>> MustBeAPanelsMembers<T>(
        this IRuleBuilderInitial<T, IReadOnlyList<DecisionPanelMemberInput>> rule)
        => rule
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(members => members.All(member => member is not null && !string.IsNullOrWhiteSpace(member.UserId)))
            .WithMessage("Every panel member must be named.")
            .Must(members => members.All(member => Enum.IsDefined(member.Role)))
            .WithMessage("Every panel member must be the chair, a member or an external member.")
            .Must(members => DecisionPanelComposition.NamesEachMemberOnce(members.Select(member => member.UserId)))
            .WithMessage(DecisionPanelComposition.MemberListedTwice)
            .Must(members => DecisionPanelComposition.HasOneChair(members.Select(member => member.Role)))
            .WithMessage(DecisionPanelComposition.ExactlyOneChair)
            .Must(members => DecisionPanelComposition.HoldsAQuorum(members.Select(member => member.UserId)))
            .WithMessage(DecisionPanelComposition.TooFewMembers);
}
