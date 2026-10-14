# DecisionHistory

A committee review's recorded decisions, newest first (`Pages/CommitteeDecisions/Review/DecisionHistory.razor`; its words in `ReviewWords.cs` and `CommitteeDecisionWording`): **the one history both review pages draw**. Flow 09 (T366, 2026-10-14, 49e7cf5e; contract § 1; T308) lifted it out of DecisionForm's read branch, so the committee's page (DecisionForm renders it, unchanged for its readers) and the trainee's own page (MyReview, with `ForTrainee`) say a decision the same way. Until then the trainee's page drew its own, and showed only the newest decision, never that an appeal had replaced one.

## What the consumer provides

`<DecisionHistory Review="review" ForTrainee="true" />`

- `Review` (required): the review whose decisions are drawn, newest first, as its DTO orders them.
- `ForTrainee`: the trainee's own page. An entrustment-only review's decision then says, under its heading, what such a review decides; the committee's page says it once above the history (DecisionForm).

## Markup

- **No decision**: `p.muted#no-decision-note`, "No decision has been recorded yet." ("No decision was recorded before the review was withdrawn.").
- **A withdrawn review's decision**: `p.muted#withdrawn-decision-note` above the history.
- **The history**: `div.decision-history`, a flex column 24px apart (`space-lg`); a 1px `border-color` hairline and 24px between two decisions (`.decision-record + .decision-record`).
- **Each decision**: `article.decision-record#decision-{id}`, named by its `h3`, the outcome: "Satisfactory with Observations", "Inadequate Progress — Additional Training", "Entrustment decisions only". Under it, its paragraphs 8px apart, the last with none:
  - with `ForTrainee` on an entrustment-only review, `p.decision-note` (muted): "This review decided entrustment only, so it records no progression outcome."; on the current decision, when an appeal remitted it, "The appeal was remitted: this decision is the appeal body's, and the entrustment decisions this review issued stand unless one is revoked.", and, when the agenda is shown, "What it decided on each EPA is on its agenda below.";
  - on a decision an appeal replaced, `p.decision-replaced` (muted): "Replaced on appeal by the decision above.";
  - the rationale;
  - "About the EPAs not decided: Not enough observed evidence yet this year." (the decision's `UndecidedNote`, D53; a remit carries the first decision's, E5);
  - "Conditions: Two observed Mini-CEX and one DOPS before the next review." (T307);
  - "Present: Thandi Zulu (chair), Sarah Botha", no full stop (the build review's Present line), or `p.decision-note` "Nobody was recorded as present for this decision.".

## Rules (DESIGN.md § Entrustment-only reviews, "The trainee's reviews"; § Committee agenda, "The decision")

- **Newest first**: the replacement heads the history and the decision it replaced follows, marked.
- **Two h3s never read the same**: an entrustment-only decision replaced on appeal is headed "Entrustment decisions only, replaced on appeal".
- **No deferral** (D53): the sentence about the EPAs not decided is the decision's own, optional; "Outcome Deferred" is a progression category like any other.
- No pronoun for a person: the panel by its members' names, the appeal body as "the appeal body".

## Contrast

No new colour token. The rationale and lines `text-color` 12.63:1, the notes and the replaced line `muted-text` 5.09:1 on the surface; the hairline is a separator, not a boundary.

## Known gaps

- None the replay met (4.41, 4.42, 4.47, 4.48, 4.51).
