namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// What a refusal says when a committee review changed between being read and the save. (T213 review)
/// </summary>
/// <remarks>
/// The chair's actions at an in-progress or decided review each move the review's concurrency token (its <c>xmin</c>,
/// <c>CommitteeReviewConfiguration</c>): staging, editing and removing a staged decision mark the review modified, and
/// recording and ratifying change its state. So any one of them can be what refused another, and each refusal names them
/// all, in one wording. Before the T213 review each handler listed its own subset. Deferring and reinstating an agenda
/// line moved it too until there was no deferral (T361, D53).
/// </remarks>
internal static class CommitteeReviewChanged
{
    /// <summary>Everything that moves the token, as the middle of a refusal: "…: {this}. Nothing was …".</summary>
    internal const string WhatChanges =
        "a decision was staged, edited or removed, or the committee's decision was recorded or ratified";
}
