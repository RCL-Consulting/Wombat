---
id: T165
title: A committee decision can be taken by one person: a one-member panel validates, and the chair alone ratifies
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T165 — One person can take a committee entrustment decision end to end

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium-High. The College defines the summative entrustment decision as a committee's, taken "never by a
single assessor and never from a single form". Wombat lets one person take it end to end, and the record cannot show
that anyone else was involved.
**Surfaced:** 2026-09-24, the EPA-stream survey, gap "committee of several people". No task covers it: [T131] covers
routing, cadence and the agenda only.

## Symptom

Observed at `431e69e`:

- **A one-member panel is valid.** `CreateDecisionPanel.cs:26` and `UpdateDecisionPanel.cs:22` require only
  `Members.NotEmpty()`. Nothing requires a Chair (`CreateDecisionPanel.cs:20-37`), and nothing requires the members to
  be distinct.
- **The chair alone ratifies, and ratifying issues every staged STAR.** `CommitteeReview.Ratify`
  (`CommitteeReview.cs:97-117`) stores only `RatifiedByUserId`. `RatifyCommitteeDecision.cs:43-58` then turns every
  staged `PendingEntrustmentDecision` into an issued decision on that one user's action. There is no attendance,
  quorum or concurrence record anywhere.
- **An Administrator who is not on the panel passes the chair check.** There are two copies of `DemandChairAccess`,
  both with the bypass: `CommitteeDecisionAuthorization.cs:143-158` (used by record, ratify and close) and
  `EntrustmentDecisionAuthorization.cs:14-19` (used by stage, issue and remove).
- Nothing stops the chair from also being the trainee's only assessor. This is inferred from the absence of any check.

The source: `tasks/done/T098-data/page-8-wba-tools.json:57` (page 4's governance statement). The frequency text for
EPAs 2-15 in `epa-detail.json` says "taken by Clinical Competency Committee (multiple staff members)".

## What to build

1. **Panel composition.** A panel needs at least two distinct members and exactly one Chair. Enforce it on create and
   update.
2. **Who took the decision.** Record the members present when a review is decided and ratified. Refuse ratification
   unless the attendance includes the chair and meets the panel's quorum. Recommendation: attendance recorded at the
   sitting, because a CCC decides in a meeting. The alternative is a second member's concurrence step after the chair
   records. The quorum number is an operator decision. "Multiple" means at least two; recommend a per-panel quorum with
   2 as the floor.
3. **The Administrator bypass.** Decide whether a global Administrator who is not on the panel may record, ratify or
   issue. Recommendation: no, for the actions that take a decision; keep read access. Fold the two
   `DemandChairAccess` copies into one while in there.
4. **Assessor overlap.** Show it on the review page when a panel member is also one of the trainee's assessors in the
   evidence window. Flag it; do not refuse.

Every new check runs before the first mutation (the audit trap). A refused ratify must issue no STAR.

## Verification

- [x] A panel with one member, with two entries for the same user, or with no Chair is refused on create and on update.
      Validator tests.
- [x] Ratify is refused unless recorded attendance includes the chair and meets the quorum, and a refused ratify issues
      no STAR. Application tests.
- [x] A ratified review shows who attended, on the review page and in the portfolio export. Test, and in the browser.
- [x] The Administrator-bypass decision is recorded here and tested on the consolidated `DemandChairAccess`.
- [x] Browser, on dev: stage a STAR, ratify with two attendees, and see both named.
- [x] Full suite green, no `--no-build`.

## Related

[T131] (routing, cadence and agenda; this task is its sibling, not its scope), [T166] (year-target view), [T167]
(evidence snapshot), D38 (evidence basis), [T063], [T094] (earlier panel authorization work). The scenario runbook's
Act 4 panel must follow: re-seed it under W-007 rather than design around it.

---

## As built — 2026-09-24 (D46)

A panel holds each member once, exactly one Chair, and at least two members, each an active CommitteeMember at the
panel's institution (`DecisionPanelMemberRules`).
- **Recording** records who was present, on the decision itself: at least two, the recording chair among them. A
  remitted appeal records its own quorate sitting.
- **Ratifying** needs that quorum, and only the Chair ratifies.
- **One chair check, no Administrator bypass**, also on appeals.
- **Staged STARs are fixed once the decision is recorded.** A STAR that no longer fits can still be removed.
- **The chair as only assessor** is warned about on the review, from the snapshot's frozen assessor ids.
- **Erasure** replaces a member's id in attendance and in the snapshot's assessor lines.

Merged with T131 slices 1–2 as `02a60db`. The migration was regenerated as `20260924194915_T165_CommitteeDecisionQuorum`.

Browser on dev (scripted Chrome, `5f9db98`, a local SMTP sink capturing the mail): 
- A panel with only a chair is refused, for both an edit and a new panel. As the InstitutionalAdmin, committee2 was
  added to panel 1.
- Review 3: scheduled by the Coordinator and started by the chair, with PAED-001 staged on two snapshot lines. In the
  "Present" group the chair is ticked and locked.
  - The chair alone was refused: "A committee decision needs at least two panel members present …".
  - With committee2, the decision recorded ("Present: Demo Committee (chair), Demo Committee Two") and was ratified.
    STAR 3 issued.
- committee2 is refused Record ("Only the panel's chair can do this.") and Ratify.

**Filed ([T213]):** the races and wording left by the merge. Also added there: two refusals show the raw validator
format, and a non-chair is shown controls that then refuse.
