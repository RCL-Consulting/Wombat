# Flow 17 build: the review

Two Sonnet reviews of the merged `t398` at `68e127d8` (§ 0, lanes A, B, C and W, the sweep), 2026-11-10: **code**
(`build-review-code.md`: § 0 and lane A; 1 must, 3 should, 5 nits) and **pages and runbook** (`build-review-pages.md`:
lanes B, C and W; 0 must, 4 should, 12 nits; 31 groups of NEW words). The two files are kept as written; this one merges
them. Each must was checked by the integrator before it was carried. Suites at `68e127d8`: 895/4459/1035/52/4806/532.

## What holds
- T334/Q19 widens nothing: `MayWrite` is the old rule behind `!systemManaged`, `MayRead` the old read rule, every caller
  updated (code). P2 assigns nothing before a refusal; the compact `Serialize` and the seeds' round trip are unchanged;
  the editor read's null is handled by both callers; seeded versions read "Wombat's catalogue" (code).
- Steps 1.24–1.31, 6.14a, A.7.9, A.7.14 and `states.md` :902–:920 match what the code renders, every count traced; flow
  03's filing page and summary unchanged; the tab keys prevent only arrows, Home and End (pages).
- The sweep's two departures (equal tab tracks under a 34rem column; the count's word clipped under 46rem, still in the
  name) and every lane departure the integrator accepted: accepted (pages). Lane W's four suspected faults: three are
  right as built; the fourth (an Active-only save leaves a draft) is by design and wants a `states.md` row.

## Must-fix
| # | From | Defect | Checked | Fix |
|---|---|---|---|---|
| M1 | code M-1 | The preview's EPAs are computed by the type's own scope for every reader; `MayRead` admits any active published type, so institution A's admin opening B's type sees B's own EPAs. | `GetActivityTypeEditorQuery.cs:222`; `ActivityTypeAdminScope.cs:75` | Compute the preview's EPAs by the caller's scope (the type's for a writer); a test of A reading B's type. |

## Should-fix (the fix pass)
- **Code S-1.** The nominee-role refusal lists labels but the parser accepts keys only: accept the labels too, or word the
  list as the keys a writer types (`FormSchemaParser.cs:404-407`).
- **Code S-2.** A reader gets `DraftSavedByName` and the draft's `DraftDisplayFieldsJson`: give a reader neither (the
  published version's display fields).
- **Code S-3.** A system-managed type that already holds a draft can neither be discarded (T334) nor refreshed (the
  refresher skips a draft in flight). **For the operator: F1.**
- **Pages S1.** The list's Actions column is 224px for one button: `col-actions--single` (`ActivityTypesList.razor:112,125`;
  `app.css:6577`); re-read the 1280 columns after it.
- **Pages S2.** A section's key and title inputs carry no `aria-invalid`/`aria-describedby` for the refusal in the head.
- **Pages S3.** Pin in `NarrowLayoutTests`: C1, R1, S2, the `builder-page` container and `.workflow-readout` in the
  1024px stack.
- **Pages S4.** DESIGN.md: the list page (search with Show and Clear, count line, own-first order, no-match vs empty, Set
  by, the release line, the subtitle by role) and the read-out components.
- **Runbook.** A `states.md` row "Not yet captured (T398)" for a published type saved with only Active changed.

## Nits
Code's five and pages' twelve stay in their files; the fix pass takes them as a list, and § 5 of `build-contracts.md`
gets the NEW words (pages' list of 31 groups).

## For the operator (answered 2026-11-10: F1 as recommended)
- **F1. A system-managed type with a draft in flight.** T334 refuses every write to MSF and Learner Feedback, Discard
  included, and the seed refresher will not touch a type with a draft, so such a draft could only be cleared in the
  database. No builder path can make one now (T334), so it only arises from a draft saved before this build.
  *Recommend:* Discard stays allowed on a system-managed type that holds a draft, for the roles that could write it
  before T334: it only drops staged work and never changes what is published. The alternative, the refresher discarding
  it, would silently throw away someone's work.
