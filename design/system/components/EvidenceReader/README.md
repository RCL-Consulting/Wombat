# EvidenceReader

Read, in place (`Pages/CommitteeDecisions/Review/EvidenceReader.razor`): inside an open agenda line, the activity a snapshot line names, **as it is now**, takes the staging form's place, so the chair reads the evidence without leaving the sitting or losing what is typed. Flow 03's cards, read-only, with no action and no move. Flow 07 (T361, 2026-10-12, 44de180b; R2-Spec "EvidenceReader"; C3, round 3 item 8; review D1) made it. Until then the snapshot's links opened the activity page, and the way back was the browser's.

## What the consumer provides

```razor
<EvidenceReader Line="reading" EpaCode="PAED-001" IdPrefix="stage-1" OnBack="BackFromReader" />
```

- `Line` (required): the frozen snapshot line Read was pressed on.
- `EpaCode` (required): the open line's EPA, which Back names.
- `IdPrefix` (required): the open line's form's prefix; `EvidenceReader.HeadingIdFor(prefix)` names the heading the page focuses.
- `OnBack`: the page restores the form as typed (its `LineDraft`) and moves the focus to the Read that was pressed.

It reads the activity itself (`GetActivityByIdQuery`, as the reader; the rated field's rungs through `IActivityReferenceDataService`), once per line.

## Markup

`div.evidence-reader[role=group]` named by its heading, a column 16px apart:

1. **"Back to PAED-001"** (`button.btn.btn-sm.btn-outline.evidence-reader-back`, `arrow-left`), at the start.
2. `h3.evidence-reader-heading` (1.05rem/600, `tabindex="-1"`): "Mini-CEX (Paediatrics) #12 · PAED-001" (the line alone where it names no EPA). It takes the focus when the reader opens.
3. **Moved since Start** (`p.evidence-reader-moved`, weight 600), only where the activity's state is not the one frozen: "Now Completed. Frozen at Start as Awaiting review." The snapshot keeps the state at Start, and the form's hints read that.
4. **Flow 03's cards, read-only**: ActivityStatus with its action dropped; ActivityAbout at `HeadingLevel="4"`; the form through `ActivityDetail` at `SectionHeadingLevel="4"` (each section's head at h4 under the reader's h3, its margin kept: `.evidence-reader .form-section-head h4`). Locked sections closed as the record is over; "Filled in by …" attributions; the rung row from the ladder that was read.

## States

- **Reading**: `p.muted[role=status]` "Reading the activity…".
- **Failed**: a warning, "The activity could not be read. Nothing on the review has changed." (the exception to the log, T329).
- **Not readable** (no activity, or none this account may open): `p.muted` "This activity does not exist, or it cannot be opened from this account.".

## Rules (DESIGN.md § Committee agenda, "Read, in place")

- **No action and no move, even to its assessor**: the reader is for reading; the activity page is one link away in the snapshot.
- **Back restores the form exactly as typed**, the focus on the Read pressed; nothing typed is lost by reading.
- Headings stay one level apart: the page's h2 Agenda, the form's or the reader's h3, the activity's sections at h4.

## Narrow widths

Below 641px Back is a 44px button; the cards are flow 03's own phone layout (their actions dropped).

## Contrast

As ActivityStatus's, ActivityAbout's and ActivityForm's; the moved line `text-color` 12.63:1 at 600.

## Known gaps

- None found in the re-sync.
