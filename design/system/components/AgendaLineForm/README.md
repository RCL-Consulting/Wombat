# AgendaLineForm

One staging form (`Pages/CommitteeDecisions/Review/AgendaLineForm.razor`, its typed values in the page's `LineDraft`): the chair stages a pending entrustment decision on one EPA, inside the agenda line its Stage opened, or at the agenda's foot as "Stage a decision on an EPA not on this agenda" with an EPA select. What is typed outlives the form: a line closed while another opens keeps it ("Draft kept"), and Read's reader can take the form's place and Back bring it back as typed. Flow 07 (T361, 2026-10-12, 44de180b, with 6ad6c57a and 14378856; R2-Spec "AgendaLineForm"; round 3 items 8, 11, 16, 20, 26, 30; C2, C3, E9; T324) made it. Until then staging was one form under the agenda, its evidence a flat list of checkboxes.

## What the consumer provides

```razor
<AgendaLineForm Draft="draft" IdPrefix="stage-1" Heading="Stage a decision on PAED-001"
                StarEpas="_starEpas" Scales="_scales" EvidenceItems="_review.EvidenceItems"
                Standing="StandingLine" HasChart="HasChart" OffersRead="true" Result="@ResultAt(FormPlace(1))"
                OnRead="Read" OnChartJump="JumpToChartAsync" OnStage="() => StageAsync(draft, foot: false)"
                OnCancel="() => CancelLine(1)" Busy="@Busy" Running="@Running" Focus="FocusByIdAsync" />
```

- `Draft` (required): the `LineDraft` the page holds: the EPA, the rung, Issued on (today on the South African calendar when it opened, T325; the foot form's too, 6ad6c57a, which read 0001-01-01), Expires on, the rationale, the ticked lines in the order ticked, whether the other lines are shown, and the last refusal with its fields.
- `IdPrefix` (required): every id's start, "stage-1" for PAED-001's line, "stage-0" for the foot form, so an open line and the foot form on one EPA never share an id.
- `Heading` (required), `ChoosesEpa` (the foot form: an EPA select over the trainee's curriculum).
- `StarEpas` (the curriculum, each EPA with its ladder, StarCurriculum), `Scales`, `EvidenceItems` (the frozen snapshot).
- `Standing` (the standing line for an EPA), `HasChart` (whether the page draws a chart for it), `OffersRead` (inside an open line).
- `Result`: the page's result region when the last refusal is this form's, at its top.
- `OnRead`, `OnChartJump`, `OnStage`, `OnCancel` (none on the foot form), `Busy`, `Running`, `Focus`. `ChartJumpId(prefix, epaId)` and `ReadId(prefix, evidenceId)` name the links Back and "Back to …'s line" return the focus to.

## Markup

`div.agenda-form[role=group]` named by its heading, a column 16px apart:

1. `h3.agenda-form-heading` (1.05rem/600, `tabindex="-1"`): "Stage a decision on PAED-001", or "Stage a decision on an EPA not on this agenda".
2. The result, when it is this form's: "Not staged. An expiry date must be after the issue date.", taking the focus.
3. **The standing in one line** (`p.agenda-form-standing`, muted): "Year 4 target 5 · no STAR yet · 3 ratings in the review window · MSF: None released yet" ("STAR at 4", "MSF: Covered"); each part read apart, and a part whose read failed is replaced once by "Standing could not be loaded." (11, 26).
4. **"Rating chart for PAED-001"** (`a.chart-jump`, a link alone on its line, weight 600, 24px), only where the page draws that chart: it moves the focus to the chart's heading, where "Back to PAED-001's line" returns it (C2; TrajectoryChart's `HeadContent`).
5. A `.form-grid`:
   - **EPA** (the foot form): a select, "Choose an EPA…", each "PAED-001 — Providing paediatric emergency care to children", help "The EPAs on the trainee's curriculum.". Choosing one clears a rung not on its ladder.
   - **Evidence it rests on** (`fieldset.form-group.full-width.snapshot-pick`, required): this EPA's lines first, by instrument A–Z; the others behind **"Show the other 6 snapshot lines"** (`btn btn-sm btn-outline .snapshot-pick-more`, `aria-expanded`), grouped by EPA (`div.snapshot-pick-group`, weight 600: "PAED-010 — Leading and operating within a clinical team", "Not about a single EPA", "Frozen before lines named their EPA"). Each line `div.snapshot-pick-line`: a tick (`.form-check`, its whole label the target: "Mini-CEX (Paediatrics) #12 · Thandi Zulu" and, muted, " Rated 5 · 2026-09-23 · Completed"; "2026-09-23 (filed)" where the encounter date was not declared; "Not recorded" for a rated instrument nobody rated, "Unrated" for one that rates nothing; a campaign report its state alone) and, in an open line, **Read** (`btn btn-sm btn-outline`, named "Read Mini-CEX (Paediatrics) #12"). Only lines a decision may rest on are offered (never a supervisor report, D38); with none, "The evidence snapshot holds no line a decision can rest on, so no entrustment decision can be staged at this review.". **The hints** under the ticks (`p.field-warning`, said, never enforced, not live; D38, O1): "Only one item is named. The College's rule is that an entrustment decision is never taken from a single form.", "None of the named items is about PAED-002.", "None of the named items was finished when the review started (each shows the state it was in), so the decision would rest on no completed assessment.".
   - **Authorised level** (required): a select of the EPA's ladder's rungs, grouped by scale (`optgroup`), "Choose a rung…" ("Choose an EPA first…" on the foot form before one is chosen); help "Rungs of CPSA Paediatric Entrustment Scale v11.1, the ladder this EPA is assessed on." ("This EPA is on no ladder, so any scale's rung may be chosen.").
   - **Issued on** (required, a date), **Expires on (optional)** (a date), **Rationale** (required, full width).
6. `div.form-actions` (left-aligned, no top margin): **Stage pending decision** (`btn-primary`), and **Cancel** (`btn-outline`) on a line.

## Behaviour

- **Stage pending decision is never natively disabled** (16). Until a line is ticked it is `aria-disabled="true"` with `.is-unavailable`, named by its reason beside it (`p.move-reasons.move-reasons--beside`, "Name at least one item of the evidence snapshot."); pressed then, it moves the focus to the reason. While its own stage runs it is `aria-disabled` and keeps the focus (T234).
- **What the form can tell is refused before anything is sent** (T324): each field marked with `aria-invalid` and its own message under it (`div.validation-message`: "Choose the EPA the decision is on.", "Choose the authorised level.", "An expiry date must be after the issue date.", "Give the decision's rationale."), and the summary at the form's top, "Not staged. Choose the authorised level. Give the decision's rationale.", taking the focus. The server's refusals arrive the same way; everything typed and ticked is kept.
- **Staged**: the line closes to Staged (AgendaTable), its draft gone, "Pending entrustment decision staged." under it; the foot form opens again on today, and its result sits under the chair's new line.
- **Cancel** discards the draft, closes the line and returns the focus to its Stage.
- **Read** (C3) swaps the form for EvidenceReader; Back restores it as typed and returns the focus to that Read.

## Narrow widths

The form is one column inside its line. Below 641px: Stage pending decision and Cancel, "Show the other N snapshot lines", Read and "Rating chart for …" are 44px; each tick's row is 2.75rem with the whole label its target (the box 1.25rem), and the label wraps its detail under its name with a 0.25em gap (14378856: a flex label had dropped the space, "Default MSF #1Released").

## Rules (DESIGN.md § Committee agenda, "The line's form")

- A decision rests on named evidence from the frozen snapshot; the hints say what the College's rules would question and never refuse.
- One field, one message; a summary at the top says what did not happen.
- The draft lives in the circuit only: no navigation, no storage.

## Contrast

The standing line and the ticks' detail `muted-text` 5.09:1; the hints `field-warning` as FormField's; Stage pending decision `on-fill` on `secondary-color` 4.86:1, unavailable as Button's `.is-unavailable`.

## Known gaps

- None found in the re-sync. The page holds a draft only while it stays open: leaving the page loses every kept draft, by design (E9).
