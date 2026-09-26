---
id: T260
title: Committee admin pages: a stale can-no-longer-sit warning after a panel save, and the revocation button stays disabled until its reason loses focus
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T260 — Committee admin pages: a stale can-no-longer-sit warning after a panel save, and the revocation button stays disabled until its reason loses focus

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the committee chain 2 browser check.

## Symptom

1. **After a panel save.** Save removes a member who can no longer sit and shows "Panel members updated.", but the
   yellow "1 member … can no longer sit on it… Saving takes that member off the panel." warning stays until a reload
   (panels 1 and 3 on dev). This is from T237.
2. **Revocation.** On `/admin/entrustment-decisions`, "Confirm revocation" stays disabled until the reason box loses
   focus: the binding updates on change, not on input. A click straight after typing hits a disabled button. This
   predates the chain.

## What to build

1. Re-read the panel after a save, so the warning reflects it.
2. Bind the reason with `:event="oninput"`, or enable the button on input. bUnit for both.

## Verification

- [ ] Both, with bUnit tests.

## Related

T237, T182.

Notes, 2026-09-25 (committee chain 3's reviews):
- `ReviewDetail` keeps offering the chair's controls after a refused click, until a reload. The page returns on error
  without re-reading (the T142 design); re-read the review after a refusal (T256 review).
- On `PanelEdit`, choosing a member as chair leaves the stored chair offered unselected under Members, so Save drops
  them or refuses with the quorum message. Say so in the line, or move the stored chair to Members (T257 review).
- `PanelsList` does not say why New panel is missing for a panel administrator who can create nothing. Reuse
  `PanelEdit`'s NoneCreatable through `DecisionPanelFormOptionsDto` (T245 review).

## Notes

- **T295 replay, 2026-09-26 (C30).** Notes, 2026-09-26 (T295 replay, Steps 2.22 and 2.23, F-2.22a):
- `/committee/panels/{id}` never names the panel it edits. The tab reads "Decision Panel", the header "Decision panel" and the cards "Decides for" and "Update members". "Paed Annual Review Panel" appears nowhere, nor do its scope, institution or speciality (design/baseline/act-2/2.22-1-panel-saved.png, design/baseline/states/panel-edit--saved.png). An administrator with several panels cannot tell which one is open. `PanelEdit.razor` reads `panel.Name`, `Scope`, `InstitutionId` and `SpecialityId` into `_model` (:326-336), but renders the Panel name, Scope, Institution and Speciality fields only for a new panel (:115-173). `DecisionPanelDetailDto` (CommitteeDecisionDtos.cs:35-43) carries no institution or speciality name. This widens the task by one item, which the replay rated medium; P3 still fits, since nothing wrong is saved. Name the panel in the page header (the panel's name as the title, with a subtitle such as "Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital"), or add a read-only details list above Update members. Carry the institution and speciality names on the DTO. The tab title's wording belongs to T190. Verification: a bUnit test that the page for an existing panel shows its name, scope, institution and speciality, and a browser check that Step 2.22's page names Paed Annual Review Panel.
- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26 (states/review-detail--chair-cannot-act.png).** Start review is offered while no chair can act. On a scheduled formative review whose chair (Dr Zulu) Prof Mbatha had locked out, Dr Naidoo, a member, was offered Start review above #chair-cannot-act-note (ReviewDetail.razor:139-154, :197-202). StartCommitteeReviewCommandHandler (StartCommitteeReview.cs:42-83) asks DemandStartableReviewAsync and the trainee check, but never whether the chair may sit (PanelSeat.SittingAt, T256). A start freezes the evidence snapshot and the agenda (:81). Afterwards nothing can move the review until a panel administrator seats a chair: on a formative review only the chair can Close it (ReviewDetail.razor:158), and on a summative review only the chair can record. Evidence filed in between is left out of the snapshot. Decide, and record which. The recommended option is to refuse Start while the chair cannot act: the handler and the page read one predicate, and the page shows Start disabled with the note as its reason (T107's pattern). The alternative is to keep Start and say at the button that the review will wait for a chair and that the evidence is frozen now. T256's As built decided nothing about Start. Verify with a handler test (the refusal writes nothing, given the audit trap) and a bUnit test for the button.
