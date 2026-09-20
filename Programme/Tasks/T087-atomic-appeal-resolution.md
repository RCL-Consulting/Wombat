# T087 — Atomic committee-appeal resolution (F-4F-1)

**Status:** ✅ Done (2026-06-07)
**Found by:** clean v2 replay, Act 4 Phase 4.F.

## Problem (F-4F-1)

`CommitteeReview.ResolveAppeal` mutated the appeal **before** validating the Remitted-replacement
requirement. When a `Remitted` outcome was submitted without the replacement decision
(`remittedCategory` / `remittedRationale`), the sequence was:

1. `appeal.Resolve(outcome, …)` set the appeal's `Outcome` + `ResolvedOn`.
2. The "must record the replacement decision" guard then threw.

This left a **stranded state**: the appeal was marked resolved, the review stayed `UnderAppeal`, no
replacement decision existed, and the UI refused any retry ("There is no open appeal to resolve.").
Mirrors T084 (non-atomic erasure approve).

Repro in the replay: on the appeal form, selecting `Remitted` reveals the replacement fields
(`appeal-category` / `appeal-rationale`); submitting before filling them tripped the bug. (Note: the
form's `decision-category`/`decision-rationale` belong to the record-decision form, not the appeal
resolution — only `appeal-*` feed `RemittedCategory`/`RemittedRationale`.)

## Fix

`src/Wombat.Domain/CommitteeDecisions/CommitteeReview.cs` — moved the Remitted replacement-decision
guard **above** `appeal.Resolve(...)`, so all preconditions are checked before any mutation. The method
is now all-or-nothing: a bad request throws with zero state change; the appeal stays open and the review
stays `UnderAppeal` so the operator can retry.

## Tests

`tests/Wombat.Domain.Tests/CommitteeDecisions/CommitteeReviewTests.cs` —
`ResolveAppeal_RemittedWithoutReplacement_ThrowsAndLeavesAppealUnresolved`: asserts the throw leaves
`appeal.ResolvedOn`/`Outcome` null, review `UnderAppeal`, one decision; and that a subsequent correct
resolution then succeeds (review `Final`, two decisions). Domain 49→50; Application 278 (unchanged).

## Related (not fixed here — separate ticket)

FluentValidation validators are registered (`AddValidatorsFromAssembly`) but **no `ValidationBehavior`**
is in the MediatR pipeline, so command validators (incl. `ResolveAppealCommandValidator`) never run —
the domain guard is the only defense. Wiring a global validation behavior is cross-cutting and risks
surfacing latent validation across all commands; track separately.
