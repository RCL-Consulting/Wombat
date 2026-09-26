---
id: T307
title: Resolving an appeal takes an outcome nobody chose: the Outcome select opens on Dismissed, Upheld leaves the decision standing like Dismissed, and a remit cannot record conditions
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-09-26
completed: 2026-09-26
---

# T307 — Resolving an appeal takes an outcome nobody chose: the Outcome select opens on Dismissed, Upheld leaves the decision standing like Dismissed, and a remit cannot record conditions

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Resolving is irreversible: the review goes Final and nothing reopens it. The form pre-selects the outcome worst for the appellant, the two non-remit outcomes do the same thing under opposite-sounding names, and a remitted replacement silently loses its conditions (the remediation plan).
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-4.45a, F-4.45b).

## Symptom

Step 4.45, as Dr van Rensburg (external member) on Dr Mahlangu's review under appeal. The Appeals card's Outcome select opens on "Dismissed", with options Upheld, Dismissed and Remitted and no "Select an outcome…". Pressing Resolve appeal without touching it dismisses the appeal and closes the review. Choosing Remitted reveals Replacement category ("Select a category…"), Replacement rationale and Present, and no Replacement conditions field. So in Steps 4.46-4.47 the chair's remit to "Satisfactory with Observations" cannot record its observations, although the first-time Decision form has a Conditions box. Screenshots: design/baseline/act-4/4.45-1-vanrensburg-remit-form.png and design/baseline/states/review-detail--appeal-form.png.

Found in code while verifying, not by the replay: "Upheld" and "Dismissed" both close the review with the appealed decision still current, and the select prints the enum names with no help text. A resolver who picks Upheld, meaning the trainee's appeal succeeds, leaves the Inadequate Progress decision in force.

## Root cause

- `ReviewDetail.razor:2048-2051`: `AppealResolutionModel.Outcome` is a non-nullable `CommitteeAppealOutcome` initialised to `Dismissed`, so `[Required]` never fires. The select at `:742-748` loops over `Enum.GetValues<CommitteeAppealOutcome>()` with no empty option and prints `@outcome` raw.
- `ReviewDetail.razor:750-778`: the Remitted branch renders no input for `_appeal.RemittedConditions` (`:2055`), which `ResolveAppealAsync` still sends (`:1263`), always null. `ResolveAppealCommand.RemittedConditions` (`ResolveAppeal.cs:31`, validated at `:59`) and `CommitteeReview.ResolveAppeal(remittedConditions)` (`CommitteeReview.cs:731`, passed to `CommitteeDecision.Create` at `:767`) accept conditions.
- `CommitteeReview.cs:746`: only Remitted builds a replacement. Upheld and Dismissed both run `appeal.Resolve` (`:777`) and set `Final` (`:784-785`). `CommitteeAppealOutcome.cs` has no doc, and no decision in EPA-PROGRAMME § 3, DOMAIN.md or DESIGN.md says what Upheld means.
- `ResolveAppealCommandValidator` has no `IsInEnum` on `Outcome`, and `CommitteeAppeal.Resolve` stores any value.
- DESIGN.md:1555's "Every category select opens on 'Select a category…'" covers category selects only, so the Outcome select was never held to it.

## What to build

1. **Outcome opens empty.** Make `AppealResolutionModel.Outcome` nullable with `[Required(ErrorMessage = "Choose an outcome.")]`, and make the first option `<option value="">Select an outcome…</option>`. Label each option in words from `CommitteeDecisionWording` (an `AppealOutcomeLabel`), with help text on the select saying what each outcome does to the decision. Add `RuleFor(Outcome).IsInEnum()` to the validator.
2. **Remit records conditions.** Under Remitted, add an optional "Replacement conditions" `InputTextArea` (`#appeal-conditions`) bound to `_appeal.RemittedConditions`, on progression and entrustment-only remits alike, as the Decision form's Conditions box is. Send it only for Remitted.
3. **Decide what Upheld means,** and record it in EPA-PROGRAMME § 3 as a D-number adopted on recommendation. Recommended default: the engine has two effects, so offer two outcomes, "Dismissed: the decision stands" and "Remitted: the appeal body replaces the decision", and remove Upheld from the enum and the select. The alternative is that Upheld requires a replacement, as Remitted does. Compatibility is not a constraint: rewrite stored Upheld rows or re-seed. Add the question to the College message (§ 3F) if the operator wants the College's word.
4. **DESIGN.md:** extend the "opens on Select a…" rule to the appeal Outcome select, and describe the remit's Conditions field.
5. Update runbook Steps 4.45-4.47's Expect once fixed. The replay itself never fixes.

## Verification

- [x] bUnit: the Outcome opens on "Select an outcome…" and an empty resolve says "Choose an outcome." and sends nothing —
  `ReviewDetailAppealFormTests.TheOutcome_OpensOnSelectAnOutcome_AndSaysEachOutcomeInWords`,
  `ResolvingWithoutAnOutcome_SaysChooseAnOutcome_AndSendsNothing` (mutation: the Dismissed default restored, caught); the
  focus moves to the select only after the message renders (after review).
- [x] bUnit: Remitted shows Replacement conditions and sends them; Dismissed sends none —
  `Remitting_OffersReplacementConditions_AndSendsWhatIsWritten`, `Dismissing_SendsNoReplacement_EvenWhatWasWrittenForARemit`.
- [x] Application: a remit's conditions are stored on the replacement and read back —
  `ResolveAppeal_ARemitWithConditions_StoresThemOnTheReplacement_AndTheReviewReturnsThem`.
- [x] Validator and handler: an undefined outcome is refused and nothing is resolved —
  `ValidationBehaviorTests.Handle_AnUndefinedOutcome_IsRefused_AndTheHandlerIsNotCalled`,
  `CommitteeQuorumHandlerTests.AnUndefinedOutcome_IsRefused_AndNothingIsResolved`.
- [x] D51 in EPA-PROGRAMME § 3D; `AppealOutcomeTests` pins what each outcome does; `AppealOutcomeMigrationPostgresTests`
  rehearses the migration on PostgreSQL (an Upheld row becomes Dismissed; the check constraint refuses the old value).
- [x] Browser, 2026-09-26: Act 4 replayed from the post-Act-3 snapshot (`wombat_scenario_rc307a`). At 4.45 the Outcome
  opens on "Select an outcome…" with no Upheld, and Remitted reveals Replacement conditions; at 4.47 the remit with
  conditions closes the review and the replacement's card reads its conditions and who sat (SQL confirms). A Dismissed
  appeal played on a post-Act-4 copy. The migration ran on the replay corpus at start-up.

## As built — 2026-09-26 (`d03732d`)

- `AppealOutcome` is Dismissed (2) and Remitted (3); Upheld is gone (**D51**, adopted on recommendation). Migration
  `20260926191415_T307_AppealOutcomesDismissedOrRemitted` rewrites stored Upheld as Dismissed, what the engine had done
  with them, then adds `CK_CommitteeAppeals_Outcome`. An undefined outcome is refused by the validator, by
  `CommitteeReview.ResolveAppeal` before any change (the audit trap) and by `CommitteeAppeal.Resolve`.
- The form opens on "Select an outcome…", says each outcome in words, shows Replacement conditions with Remitted, sends
  a remit's fields only with Remitted, and puts its one message under the select. The Appeals list names outcomes
  through `CommitteeDecisionWording`.
- Runbook: 4.45 and 4.47 restated for the new form; the re-check's lines on 4.45–4.48. Baseline: 9 captures re-taken;
  `review-detail--appeal-upheld` no longer exists (kept on disk, marked in the brief).
- Seen in the re-check, noted: the clipped Outcome select (F-4.45c, T323); a refusal shown far from its card (F-4.46a,
  T299).

## Related

T165 (a remit records its own quorate sitting), T213 (the appeal body resolves), T131 slice 5 (the entrustment-only remit), T022 (named the three outcomes), T264 (confirmation on destructive actions), the My Committee Reviews task drafted alongside (the trainee should see the remit's conditions and outcome). DESIGN.md:1555. Runbook Steps 4.45, 4.46 and 4.47. Findings F-4.45a and F-4.45b.
