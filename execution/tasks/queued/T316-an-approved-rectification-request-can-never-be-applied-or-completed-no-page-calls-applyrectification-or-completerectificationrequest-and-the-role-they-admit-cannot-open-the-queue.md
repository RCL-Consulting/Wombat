---
id: T316
title: An approved rectification request can never be applied or completed: no page calls ApplyRectification or CompleteRectificationRequest, and the role they admit cannot open the queue
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T316 — An approved rectification request can never be applied or completed: no page calls ApplyRectification or CompleteRectificationRequest, and the role they admit cannot open the queue

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A data subject's right to have inaccurate data corrected, which the product offers on its own form, cannot be carried through in Wombat. The request stays Approved for good, and nothing records what was corrected. No data is wrongly changed.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings from the code read).

## Symptom

A trainee submits a Rectification on /account/data-rights (runbook Step A.1.7). If the Coordinator approves it on /admin/data-rights/{id} instead of rejecting it, the request reads Approved and the page offers nothing more. The Action card appears only for Submitted and Under review. The requester's own page shows Approved with no action. Nothing records the correction or completes the request, so it stays Approved indefinitely. Step A.1.8's Note records this. It was not played, because that step rejects the request; the replay confirmed it from code (suspects-verdicts: author and reviewer at A.1.8).

## Root cause

- ApproveDataRightsRequest.cs:107-110 leaves a Rectification in Approved, 'applied manually by admin via ApplyRectificationCommand'.
- ApplyRectificationCommand (ApplyRectification.cs) and CompleteRectificationRequestCommand (CompleteRectificationRequest.cs) have no caller in Wombat.Web or Wombat.Api, and no test. A grep of src and tests finds them only in their own files and in that comment. No query reads DataRightsRectification rows either.
- RequestDetail.razor:65-86 offers actions only while the request is Submitted or UnderReview.
- The commands admit a different role from the queue. DataRightsAuthorization.cs:31 sets RectificationRoles = SpecialityAdmin (plus Administrator), while /admin/data-rights and its detail admit Administrator and Coordinator (RequestsList.razor:2, RequestDetail.razor:2). The Coordinator who approves cannot apply, and the SpecialityAdmin who may apply cannot open the request, so adding a button alone would not close this.
- ApplyRectification records a correction (target type and id, the before and after as JSON) and marks it applied. It changes no data itself.
- T112's 'Still open' left two points for when a UI is built: who the data-rights officer is, and that SpecialityAdmin rectification is scoped to the institution but not the speciality.

## What to build

- Decide who carries a rectification through, and record it in EPA-PROGRAMME § 3 or DECISIONS. **Recommendation:** whoever may approve the request (the review role in scope, and Administrator). One person then approves, corrects and completes. `RectificationRoles` folds into `ReviewRoles`, which also settles T112's speciality-scope point by removing the separate role.
- On /admin/data-rights/{id}, an Approved Rectification shows a Correction card to that role. The card lists the corrections recorded so far and offers a form that records one through ApplyRectificationCommand: which record, the value before and the value after. The data itself is corrected on the page that owns it (the trainee profile, the account's name), and the card links there. "Complete request" calls CompleteRectificationRequestCommand and is refused until at least one correction is recorded. Once complete, the Decision card shows Completed on.
- The requester's /account/data-rights reads Completed once done. The promised notification is F-A.1.2a and is triaged separately.
- Tests for both handlers, which have none today.
- A runbook step plays an approved rectification end to end: approve, correct on the owning page, record, complete. A.1.8 keeps its rejection, because D42 decides that one on its merits.

## Verification

- [ ] Application tests: ApplyRectification and CompleteRectificationRequest refuse a caller out of scope, a request that is not a rectification, a request that is not Approved, and a completion with nothing recorded. A request carried through reads Completed with its corrections.
- [ ] bUnit (RequestDetail): the Correction card shows only for an Approved Rectification and only to a role that may rectify; Complete request is refused until one correction is recorded.
- [ ] The decision on who rectifies is recorded, and T112's two open rectification points are marked closed by it.
- [ ] Browser check on a replay: a Coordinator (or the decided role) approves a rectification, corrects the record on its own page, records it and completes it; the requester's /account/data-rights reads Completed.

## Related

T112 (Still open), T084, T258; D42 (the reason A.1.8 rejects); runbook Steps A.1.7, A.1.8, A.1.9; F-A.1.2a (the notification promised at submission, triaged separately).
