# DecisionForm

The committee's decision (`Pages/CommitteeDecisions/Review/DecisionForm.razor`, its typed values in the page's `DecisionDraft`): the Decision section of the committee review. For the chair while the review is in progress, the form that records the decision, with who is present and whether that is a quorum said as it is ticked; for everyone else, and once recorded, the decision itself. Below the agenda until it is recorded, then above it. Flow 07 (T361, 2026-10-12, 44de180b; D53; R2-Spec "Decision form"; round 3 items 7, 16, 21; E1, E4; T324) rebuilt it. Until then Record was a plain submit, natively disabled short of a quorum, and a lost category or rationale came back as the validator's property names.

## What the consumer provides

```razor
<DecisionForm Review="_review" Agenda="_agenda" Pending="_pending" Draft="_decision"
              OffersForm="@(ChairMayAct && _review.State == CommitteeReviewState.InProgress)"
              Result="@ResultAt(DecisionPlace)" OnRecord="OpenRecordAsync" OnTicked="StateHasChanged"
              Running="@(_inFlight == RecordAction)" Focus="FocusByIdAsync" />
```

- `Review` (required), `Agenda`, `Pending` (the staged decisions Record fixes), `Draft` (required; kept across every re-read, so a refusal or a Refresh never loses a word, T324).
- `OffersForm`: the chair, while the review is in progress (T213).
- `Result`: the page's result region when it is this section's.
- `OnRecord`: Record decision… pressed while available: the page checks the fields and opens the dialog. `OnTicked`: a tick changed, so the chair's sitting bar recounts Present. `Running`, `Focus`.

## Markup

`section.detail-card#decision`, `h2#decision-heading.review-section-title` "Decision" (`tabindex="-1"`: the sitting bar's "Go to the decision" moves the focus here), then:

- The result, when it is this section's: "Decision recorded." (success), or a refusal before any dialog, "Not recorded. Choose a progression category. Give the committee's rationale." (danger, taking the focus).
- **An entrustment-only review** (`p.muted#entrustment-only-note`): "This review decides entrustment only. Its decision is the entrustment decisions staged above, and it records no progression category: the trainee's progression is decided at a progression review." ("… staged on the agenda below, …" once decided).
- **The chair rated all of this evidence** (a warning, `h3` and a sentence, T165): "Every rated record in the evidence snapshot was rated by Thandi Zulu, who chairs this panel. A committee decision is never a single assessor's: say in the rationale how the other members weighed it." Not a refusal.

**The form** (`OffersForm`), a `.form-grid`:

- **Category** (a progression review only; required): a select opening on "Select a category…": Satisfactory Progress, Satisfactory with Observations, … (`CommitteeDecisionWording.CategoryLabel`).
- **Rationale** (required, full width).
- **About the EPAs not decided (optional)** (full width), help "The registrar sees this once the review is ratified.": the decision's one sentence (`UndecidedNote`, D53), shown under every line left unstaged.
- **Conditions** (full width).
- **Present** (`fieldset.form-group.full-width`, required), help "Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other."; a `.check-grid` of every member who may sit, "Thandi Zulu (chair)" ticked and locked, the others' ticks free.
- O8's sitting-order warning under the grid where another panel still owes a decision (`p.field-warning#sitting-order-warning`): "PAED-004 is decided by <the other panel> and not yet decided for 2026 S2. A progression decision recorded now is taken without it." A warning, never a refusal.
- `div.decision-actions` (a rule above, 32px over it, 16px under it; a wrapping row): **Record decision…** (`btn-primary#record-decision`), and beside it `div.decision-actions-text`: **the quorum line** (`p.quorum-line#quorum-line`, `aria-live="polite"`, the page's one live region besides the results): "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other.", "4 present: the chair and 3 others. The quorum is met.", or, where the panel cannot sit a quorum, its reason (`PanelShortfall`), which ticking cannot cure (7); then the reason or the hint.

**Otherwise**: "No decision has been recorded yet." ("No decision was recorded before the review was withdrawn."); or each decision recorded, an `article.decision-record` with its `h3` the outcome ("Satisfactory Progress"; an entrustment-only review's own words), "Replaced on appeal by the decision above." where it was, the rationale, "About the EPAs not decided: To be decided at the final review.", "Conditions: …", "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)" ("Nobody was recorded as present for this decision.").

## Record decision…

- **Never natively disabled** (16). It is `aria-disabled="true"` with `.is-unavailable` while the quorum falls short (named by the quorum line) or while an entrustment-only review has nothing staged (E4: "This review decides entrustment only, so stage at least one decision before recording it.", `p.move-reasons#record-reason`); pressed then, it moves the focus to the reason.
- **Available**, the hint beside it says what Recording does (`p.muted#record-hint`, not live): "Recording fixes the 3 staged decisions, and records the 12 EPAs not staged as not decided." ("Recording records the committee's decision." with neither).
- **Pressed**: a missing category or rationale is refused at the top before any dialog (T324), each field marked with `aria-invalid` and its own message ("Choose a progression category.", "Give the committee's rationale."), everything typed and ticked kept. Else the ConfirmDialog: "Record Satisfactory Progress for Lerato Molefe?" ("Record the entrustment decisions for Lerato Molefe?"), "Fixes 3 staged decisions: PAED-001 at 5, PAED-010 at 4 (expires 2026-10-25), PAED-012 at 5.", "Records 12 EPAs as not decided: PAED-002, PAED-003, … and PAED-015." (every unstaged line, due and optional alike, E1), "Present: …", "Afterwards only a decision that no longer fits the trainee's curriculum can be removed.", **Not yet** and **Record decision**. While it runs Record keeps the focus, `aria-disabled` (T234).
- **Recorded**: the draft is cleared, the section moves above the agenda, "Decision recorded." in it, taking the focus.

## Narrow widths

Below 641px Record decision… is 44px and the row's width, the quorum line and its reason under it; each Present tick's row is 2.75rem, the whole label its target (the box 1.25rem).

## Rules (DESIGN.md § Committee agenda, "The decision")

- **No deferral** (D53): the one sentence about the EPAs not decided is the decision's, optional, and the registrar reads it once ratified.
- **The quorum is said as it is ticked**, and Record says why it cannot be pressed; nothing is natively disabled.
- "Outcome Deferred" is a progression category, recorded like any other; it is not a deferral.

## Contrast

The quorum line `text-color` 12.63:1; the hint `muted-text` 5.09:1; the reason as `move-reasons`; Record `on-fill` on `secondary-color` 4.86:1.

## Known gaps

- What is typed and ticked lives in the circuit until Record: leaving the page loses it.
