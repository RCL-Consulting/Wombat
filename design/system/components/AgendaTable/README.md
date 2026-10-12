# AgendaTable

The agenda, the committee review's spine (`Pages/CommitteeDecisions/Review/AgendaTable.razor`, its words in `ReviewWords` and `CommitteeAgendaText`): one line per EPA the review is there to decide, the chair's lines after them. One line opens at a time, in place, to stage a decision; a staged decision is said in its line; once the decision is recorded every line left unstaged reads "Not decided at this sitting". Flow 07 (T361, 2026-10-12, 44de180b, with 14378856; D53; R2-Spec "Agenda line"; round 3 items 17, 20, 24, 25, 27, 30; E1, E9) rebuilt it. Until then each line had Defer and Reinstate, and the staging form was one card under the table.

## What the consumer provides

```razor
<AgendaTable Review="review" Agenda="_agenda" Pending="_pending" ChairStages="@ChairStages"
             ChairActsOnDecided="@(ChairMayAct && review.State == CommitteeReviewState.Decided)"
             OpenEpaId="_openEpaId" DraftEpaIds="_drafts.Keys" OpenContent="OpenLine"
             Result="@ResultRegion" ResultAtTop="@(_resultPlace == AgendaPlace)" ResultEpaId="@ResultLine"
             Foot="@(ChairStages ? FootForm : null)" AgendaError="_agendaFailed" PendingError="_pendingFailed"
             Busy="@(_inFlight is not null)" OnStage="OpenLineFor" OnRemove="OpenRemoveAsync" />
```

- `Review` (required), `Agenda` (the review's agenda, null when it could not be read), `Pending` (the staged decisions, each shown in its line).
- `ChairStages`: the chair, while the review is in progress (Stage and Remove…). `ChairActsOnDecided`: the chair on a decided review, Remove… on a staged decision that no longer fits the trainee's curriculum (T165).
- `OpenEpaId` and `OpenContent`: the open line and what is drawn in the row under it (AgendaLineForm, or EvidenceReader in its place). `DraftEpaIds`: the lines holding a kept draft (E9).
- `Result`, `ResultAtTop`, `ResultEpaId`: the page's one result region, atop the table ("Review started.") or in a row under a line.
- `Foot`: the foot form, "Stage a decision on an EPA not on this agenda" (AgendaLineForm with an EPA select), drawn whether or not the agenda was read.
- `AgendaError`, `PendingError`, `Busy`, `OnStage`, `OnRemove`. `AgendaTable.StageId(epaId)` and `RemoveId(epaId)` name a line's buttons, where Cancel returns the focus.

## Markup

`section.detail-card#agenda`, `h2#agenda-heading.review-section-title` "Agenda", then, in order:

- The result atop the table, when it is "Review started.".
- A failed read, a warning each: "The agenda could not be read. The rest of the review stands.", "The staged decisions could not be read. The agenda shows the ones last read.".
- **What the lines mean now** (`p.muted#agenda-intro`): decided, "Fixed when the committee's decision was recorded: ratifying issues exactly these." (with "Only a decision that no longer fits the trainee's curriculum can still be removed." where one does); ratified, "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each one." or "Nothing was staged when the review was ratified, so ratifying it issued no STAR."; withdrawn, "Nothing is pending: the review was withdrawn, so it issues no STAR, and anything staged at it was removed.".
- "Already decided in this window, so not on the agenda: PAED-001 and PAED-012." (`#agenda-decided-in-window`, muted, T215).
- **The table**: `div.table-container.agenda-table-container > table.clinic-table.agenda-table[role=table]`, its caption "15 EPAs for 2026 S2 · 3 staged" while the review is in progress ("· none staged" before the first), "15 EPAs for 2026 S2" otherwise. Its columns:
  - **EPA**, the row header: `span.agenda-epa-code` (weight 600, a block; "(no longer in use)" after a paused EPA's code, `span.paused-mark`, T273) over `span.agenda-epa-title` (muted 0.9rem, wrapping).
  - **Window**: "2026 S2" for a semester EPA, "2026" for an annual one.
  - **State**: the badge (`BadgeFor.AgendaLine`), then each sentence a `span.agenda-line-detail` (a block, 0.9rem, 4px above it, a space before it so a stacked line reads as one text).
  - **Evidence**: "3 items", "1 item".
  - **Action** (`td.agenda-line-action > div.actions-cell`): the chair's alone, and only while some line offers something (T213, T226); anyone else reads four columns, never an empty fifth.
- Each line `tr.agenda-line#agenda-line-{EpaId}`; the open line takes `.is-editing`, and the row under it, `tr.is-editing.agenda-form-row#agenda-form-{EpaId}` (one cell spanning the table, padded 16px), holds the form; the two share `tr.is-editing`'s stripe. A result row is `tr.agenda-result-row` (padded 16px). Neither the form row nor the result row takes the row hover.
- "Decided by another panel" (h3), a read-only list: each EPA ("PAED-001 — Providing paediatric emergency care to children", bold), the panel that decides it and the window, and its badge (`BadgeFor.AgendaElsewhere`: Not yet decided, On the agenda of an open review, Decided, Not decided, Missed).
- The foot form, in `div.agenda-foot#agenda-foot` (a rule above, 24px each side of it).

## The states and their words

| Badge (`CommitteeAgendaText.StatusLabel`) | Tint | Under it |
|---|---|---|
| Due at this sitting | `badge-draft` | nothing: the badge says it |
| Due by year end | `badge-draft` | "Optional here; decided by the year's last sitting." |
| Partial period | `badge-draft` | "The trainee joined part-way through the window: optional, never missed." |
| As opportunity allows | `badge-draft` | "Decided as opportunity allows: optional." ("Added by the chair." on a chair's line) |
| Staged | `badge-submitted` | the decision in round 3 item 30's words (below) |
| Decided | `badge-completed` | "STAR #41." |
| Decided elsewhere | `badge-completed` | "Another sitting has decided it in this window, so it need not be decided here." (+ " A decision staged on it decides it again." in progress, "Ratifying the review records that." once decided); "Another sitting had decided it in this window when this review settled its agenda." |
| Not decided at this sitting | `badge-accepted` | the decision's `UndecidedNote` ("To be decided at the final review."); "The review was withdrawn before deciding it." |

**A staged line** says: "Added by the chair." (a chair's line), "Level 5 · issued 2026-10-05 · no expiry." ("· expires 2026-10-25."), "Rests on 2 items of the snapshot: Mini-CEX (Paediatrics) #12; Case-Based Discussion (Paediatrics) #13.", the hints the form gave ("Only one item is named. The College's rule is that an entrustment decision is never taken from a single form.", "None of the named items is about PAED-002.", "None of the named items was finished when the review started …"), and "No longer fits: …" (T167). One that names no evidence: "Names no evidence, so the review cannot be ratified with it. Remove it and stage it again, naming the evidence." (T213).

## The Action column

- **Stage** (`btn btn-sm btn-outline`, named "Stage a decision on PAED-001", `aria-expanded`, `aria-controls` the form row when open): on a line still due and unstaged at a review in progress. It opens the line, keeping any other line's typed form as its draft, and the focus moves to Authorised level.
- **Draft kept** (E9): a closed line holding a draft shows `span.agenda-draft-kept` "Draft kept" (muted 0.85rem) before its Stage, now named "Stage a decision on PAED-001 (draft kept)"; it reopens the line as typed. A draft lasts until it is staged, cancelled, or the page is left: the page holds it, in no storage.
- **Remove…** (`btn btn-sm btn-outline`, named "Remove the staged decision on PAED-012"): on a staged line, behind ConfirmDialog ("Keep it" safe, "Remove the decision" in danger).
- Otherwise, muted: "Nothing to do at this sitting" in progress, "Fixed with the recorded decision" once decided (T211).

## Stacking (round 3 item 20; 14378856)

`.agenda-table-container` is a query container (`container: agenda / inline-size`). **Under 760px of its own width**, not the viewport's, each line stacks into a block of its cells: the table, its caption, its row groups, rows and cells all `display: block` (the caption a block too, else it shrank to one word a line); each line padded 8px 16px with a rule under it; the header row visually hidden but read, and each cell names its column as the hidden header would (`td[data-label]::before`, "State: ", muted 600, its alternative text empty so a screen reader hears the cell's own role). The markup carries its table roles explicitly, so a stacked line keeps them. The open line's stripe is the row's own edge (`inset 4px 0 0 secondary-color` on `.agenda-line.is-editing` and the form row), its first cell's dropped. The actions align left. Below 641px a line's buttons are 44px.

## Rules (DESIGN.md § Committee agenda)

- **No deferral** (D53): nothing is asked of a line but staging; every line left unstaged once recorded is "Not decided at this sitting".
- One line open at a time; a result sits under its line and takes the focus.
- The three kinds of "still due" share one badge and differ by label; Not decided is amber, an outcome the committee chose, not a failure.

## Contrast

The code `text-color` 12.63:1, the title and the muted lines `muted-text` 5.09:1, the open line's stripe `secondary-color` 4.86:1 on the surface; the badges as Badge's.

## Known gaps

- A decided line names its STAR by number ("STAR #41."), not its level and expiry; a chair's line does not show the EPA's earlier STAR (the page has no data for either).
