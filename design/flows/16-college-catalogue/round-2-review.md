# Flow 16 round 2: the review

Round 2 (`round-2/`, 306 boards: the Spec, Spec-2 and Steps, the page boards at 1280 with their 390 and 768 twins;
`f16r2.css`) drew part 1 as picked (A the tree, specialities, sub-specialities and Home; B Colleges; C the scales) and
part 2 (D curricula, E EPAs, F a curriculum's items and an item's page), with one variation pair, EV1/EV2 (the reach on a
national EPA's page). The canvas read all 70 captures from `design/upload/baseline/` (none arrived as attachments).

Reviewed 2026-11-04 from three sides, each a Sonnet agent: **the code** (`round-2-review-code.md`, `d4f98c5c`: K1–K13,
N1–N11), **the cast and words** (`round-2-review-cast.md`, `2c9161ef`: C1–C16; checklist 119 states, 114 done, 5 partly,
0 missing; corrections 29, 27 done, 2 partly) and **the design system** (`round-2-review-ds.md`, `7bb27d42`: M1–M2,
S1–S5, N1–N4). The three files are kept as they are; this one merges them. Every must-fix below was checked by the
integrator against the boards or the code (the evidence column) before it was carried. Severity: M must, S should,
N nit.

## What holds

- The twins match their 1280 boards on text and actions (code: 156 compared, 0 differences; cast: 162).
- All 33 steps are rows on the Steps board and their counts hold (code), except three counts (C4).
- D56, D57, Q7 and the picks hold in the boards; the cast, dates and counts are the runbook's otherwise; no gendered
  word, id or code name in a product string.
- Fixed since flow 15 (ds): all 18 dialogs match `ConfirmDialog`; no `td.actions-cell`; load errors wrapped; loading
  lines carry `role="status"`.

## Must-fix

| # | From | Defect | Checked | Fix |
|---|---|---|---|---|
| M1 | K1, C3 | Q6 and Q16 cannot both be built as the Spec states them. Every case that raises Q6's per-item rung refusal is already "a scale in use", so Q16's refusal wins unless the order is stated, and C11/C13 are dead boards (round 1's M5 undone). C11's premise is false: the CPSA ladder is pinned from the seed, so Remove was never offered there. | `UpdateEntrustmentScaleCommandHandler.cs:61-84` guards removals only; `PaediatricCatalogueSeeder.cs:970` pins `MinimumLevelOrder` from the seed | Q18 below. |
| M2 | C1 | The Default entrustment scale select omits CNSA Neurology Entrustment Scale on A06, A08, A15, A16a (and twins), which exists from 6.5; A14a has it. Steps 6.12 should say the select offers three. | grep: 0 matches on the four, 2 on A14a | Add the option; correct 6.12's row. |
| M3 | C2, K (should) | F18 heads "Items (15)" over 16 rows (the runbook reads "Items (16)"), and is labelled "after 6.26" while showing Add item, which exists only from 6.28. The Administrator's "Items (n)" is not the national count. | F18: one "Items (15)", 16 body rows | "Items (16)"; label it after 6.28; say the count is the College's items. |
| M4 | ds M1 | 95 files draw `required-mark` with `aria-hidden` and nothing else: no `aria-required="true"` on the control and no "* marks a field you must fill in." line. | `DESIGN.md:1292`, `:1473`; `R2-A06` has 0 `aria-required` | Draw both on every form with a required field. |
| M5 | ds M2 | Four links' `aria-label`s do not contain their visible words (WCAG 2.5.3): "Items (n)", "Create sub-speciality", "Edit this version", C08's "Open Adult Neurology". | ds review's evidence | Start each label with its visible words. |

## Should-fix (carried to round 3)

- **Layout.** S2 (ds): the 900–1100 sweep skipped 901–949 and 1025–1049; by layout arithmetic (not measured) Colleges
  overflow up to 74 px at 901, Curricula about 37 px, the tree about 3 px, the items up to 27 px at 1025–1051. Stack or
  fold them (flow 15's lesson). S3 (ds): header actions wrap under the subtitle on four pages, an app.css defect (Q20).
- **Duplicates and markup.** S1 the result region nested twice (71 files); S4 `.catalogue-record` the fourth copy of one
  rule, use one `.user-page .form-check`; S5 the levels table has no `th scope="row"`.
- **Code (K).** C08's "Open Adult Neurology" has no ids behind it; B17's 2,000-character description passes the
  1,000 cap; Q14's "no national EPA" branch has no data source; E04's row order and KGK-001's College cell differ from
  the query; F16's "as built" words are not the built words; T11 (Q19); an Administrator with no College claim gets the
  "ask the platform administrator" words; K10 the one-shot token vocabulary unlisted (add "Entrustment scale created."
  and the tree's four results to `CatalogueLinks`); K11 "Put rung 5 back" needs removed rungs kept until Save; K12 most
  NEW words listed by reference, not by words; K13 the reach as a rule (reuse `RegistrarCount`'s, by
  `InstitutionId` + `CurriculumId` + `IsActive`; say the sentence for an own EPA and for a superseded adoption).
- **Cast (C).** C4 Steps counts for 6.2, 6.6 and 6.30 off the stated rule; C5 Steps gaps (the unlisted `states.md`
  rows); C6 three statements disagree on whether Create EPA wraps at 1280; C7 the other Delete refusals unworded on the
  Spec; C8 the Illustration chip applied unevenly.

Nits: code N1–N11, cast eight, ds N1–N4 (breadcrumb markup vs `Breadcrumbs.razor`, "‹…›" printed as values, the
Unavailable markup, Save's `aria-haspopup`). Carried as one list in round 3's ask.

## Questions for the operator

- **Q18 (M1). The order of the rung refusals.** *Recommendation:* on a save that removes a rung, the handler runs Q6's
  checks first (pinned items and ended records, named by EPA, curriculum and rung label, up to three then "and ‹N›
  more"); Q16's refusal covers the rest (a reorder, a removal no item needs, on a scale in use). The page offers Up,
  Down and Remove only while the scale is unused, so Q6's words show only when another save pinned the scale after the
  page was read: C11 is relabelled to that premise (a scale not yet pinned when the page was read, never the CPSA ladder, which is pinned from the seed), marked Illustration if no runbook step reaches it. C12 is the
  page read again, with Q16's words.
- **Q19 (K8, T11). A curriculum's sub-speciality.** *Recommendation:* a read-out once the version has an item or an
  adoption, and `UpdateCurriculumCommand` refuses the move then; today it rewrites `Curriculum.SubSpecialityId` and
  leaves KGK's adoption on the old discipline (`UpdateCurriculum.cs:74`, `InstitutionCurriculumAdoption.cs:24-26`).
- **Q20 (ds S3). Header actions that wrap under the subtitle.** *Recommendation:* fix it in app.css in this flow's
  build (actions beside the title down to 1024, under it below), since four of these pages meet it and flow 15's EPA
  list did too.
- **Q21 (EV). The reach on a national EPA's page.** *Recommendation:* EV2, as the Spec recommends: the goal says the
  College's administrator is told how far an EPA's correction reaches before saving, and EV1 tells it only on a pause.
</content>
</invoke>
