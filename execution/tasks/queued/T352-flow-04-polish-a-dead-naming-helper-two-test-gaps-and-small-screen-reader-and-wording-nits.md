---
id: T352
title: Flow 04 polish: a dead naming helper, two test gaps, and small screen-reader and wording nits
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-30
---

# T352 — Flow 04 polish: a dead naming helper, two test gaps, and small screen-reader and wording nits

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Code hygiene, test honesty and screen-reader nits on flow 04's pages; nothing refuses or loses work.
**Surfaced:** 2026-09-30, T350 step 6, the build's four-sided review (`design/flows/04-assessor-inbox/build-review.md`:
G3, G4, G5, R6, A6, A7, A8, A9). Observed in the code at `b5c0e837`.

## Symptom

1. **Dead helper (G3).** `ActivityRowNames.For(..., withSubject: true)` and its "updated … SAST" tie-breaker lost their
   only caller when the inbox moved to `AssessorRowNames`; `RowNamesTests` still exercises it, and its doc ("Whether the
   list is an inbox") is stale.
2. **A widened assertion (G4).** `ActivityListProjectionTests` (~268) asserts `Lookups` contains the trainee and the
   departed trainee and not "someone-else", so any other extra id passes; its comment says the assessor is looked up,
   which is not asserted.
3. **Lost page-level coverage (G5).** The removed inbox cases of `ActivityListColumnsTests` pinned the subject's name and
   the id fallback for a subject with no directory entry; `ActivityInboxTests` has none, so a departed registrar reads
   "from departed-trainee" (a raw id) with nothing deciding whether it should.
4. **One-letter and mixed-case words in move labels (R6).** `WorkflowTransition.LabelFor` lowers `return_to_A` to
   "Return to a" and `resubmit_PDFs` to "Resubmit pdfs"; a double separator leaves a double space. Builder-only.
5. **Descriptors read twice in browse mode (A6).** The rung picker's descriptor spans are `visually-hidden`, so a screen
   reader reading down hears each after the row and again in "What each rung means".
6. **Loading statuses born filled (A7).** The inbox's and `DashboardFrame`'s `role="status"` lines hold "Loading …" at
   the first render and in the prerendered HTML, so they are unreliably announced; their comments say otherwise.
7. **Headings inside `<summary>` (A8).** The request fold's section heading is in its summary, which several screen
   reader and browser pairs flatten, so on a phone those sections are missing from the heading list.
8. **Quick arrowing flicker (A9).** Three quick Right presses on the rung row replay three renders in order; the check
   mark and descriptor trail the focus for a moment. Mention only.

## What to build

1. Delete the `withSubject` arm (or the method) and its tests.
2. `Lookups.Should().BeEquivalentTo([TraineeId, "departed-trainee", AssessorId])`.
3. Add an `ActivityInboxTests` row whose `SubjectName` is a user id and decide the words (e.g. "from a former
   registrar").
4. Split on separators with `RemoveEmptyEntries`; keep a one-letter capital and any word with a capital after its first
   letter as written.
5. `<span hidden id=…>` for the descriptors (`aria-describedby` still resolves hidden text); update the tests.
6. Start each status empty and fill it after the first render, or rely on `aria-busy`; correct the comments.
7. Decide: accept (the summary's name still says the title and Show), or move the heading out of the summary.
8. No change unless a replay shows it matters.

## Verification

- [ ] 1–5: each by a unit or bUnit test that fails before the change (mutation-checked).
- [ ] 6: a bUnit test that the status is empty in the first render's markup and filled after.
- [ ] 7: the decision recorded in DESIGN.md § the request fold, with a test if the markup changes.

## Related

T350 (flow 04), `design/flows/04-assessor-inbox/build-review.md`; T349 (flow 03's polish).
