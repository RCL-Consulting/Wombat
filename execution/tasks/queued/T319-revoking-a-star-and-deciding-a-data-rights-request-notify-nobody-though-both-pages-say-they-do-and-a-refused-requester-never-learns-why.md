---
id: T319
title: Revoking a STAR and deciding a data-rights request notify nobody, though both pages say they do, and a refused requester never learns why
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T319 — Revoking a STAR and deciding a data-rights request notify nobody, though both pages say they do, and a refused requester never learns why

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Two pages make an explicit promise that the handlers behind them do not keep. The first concerns the most consequential thing written about a trainee, the withdrawal of a STAR. The trainee is not told, and the STAR's card simply disappears from her page. The second concerns a data-rights request: its requester is promised a notice and never gets one. A refused requester cannot even read the reason the reviewer was required to write. For a correction request, POPIA s 24 expects the data subject to be told what was done (inferred; the operator should confirm). This is not a permission hole and no data is wrongly changed, so it is not High.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-4.36a, F-A.1.2a, F-A.1.9a).

## Symptom

- **The revocation (Step 4.36).** On /admin/entrustment-decisions, Revoke opens a section that reads "Revocation is immediate and irreversible. The trainee is notified." (design/baseline/states/entrustment-decisions--revoke-form.png). After "Entrustment decision for PAED-002 revoked.", no mail reaches Dr Dlamini: the SMTP sink held 89 files before and after, and the log has no mail line.
- **What the trainee sees (Step 4.42).** On /portfolio/authorisations the revoked PAED-002 card is simply gone (design/baseline/states/my-authorisations--after-revocation.png). She has no mail, no "revoked" entry, and no way to reach the revoked certificate, although the certificate carries the date, the reviser and the reason (Step 4.37), and she may download it.
- **The data-rights promise (Steps A.1.2, A.1.3, A.1.8, A.1.12).** Submitting on /account/data-rights says "Request submitted. You will be notified when it is reviewed." (design/baseline/states/data-rights--submitted.png). No mail followed Smit's approval (A.1.3), his rejection (A.1.8) or devadmin's erasure (A.1.12); the sink stayed at 90 throughout.
- **The refusal's reason (Step A.1.9).** Dr Mahlangu's rejected rectification reads only "Rejected" in Your requests, with no note, no decision date and nothing else to open (design/baseline/states/data-rights--rejected.png, design/baseline/act-A/A.1.9-1-mahlangu-reads-rejected.png). The reviewer had to write a note to reject it, and only the admin queue can read that note.

## Root cause

- **Revocation.** RevokeEntrustmentDecision.cs:35-78: the handler holds only IApplicationDbContext, and lines 74-75 revoke and save. There is no IEmailSender and no revocation template in Common/Email/Templates. It is the only caller of EntrustmentDecision.Revoke. The sentence at Index.razor:106 was written against a notice that was never built.
- **Where a trainee reads a revocation.** MyAuthorisations.razor lists GetActiveDecisionsForTraineeQuery, which filters Status == Active (GetActiveDecisionsForTrainee.cs:49). Revoked and expired STARs therefore have no place on the trainee's side, although DownloadEntrustmentCertificate.cs:68-73 would serve them to her (TraineeScopeResolver.MayReadAsync).
- **Data-rights decisions.** ApproveDataRightsRequest.cs:42-52 and RejectDataRightsRequest.cs:37-40 inject no sender, and each decision is a mutate-and-save (Approve :80-113, Reject :56-57). No data-rights template exists. DataRights.razor:262 promises the notice anyway.
- **The requester's view.** GetMyDataRightsRequests.cs:38-43 projects into DataRightsRequestSummaryDto (DataRightsDtos.cs:18-23): id, name, date, type and status only. The note and the decision date never reach the requester's page.

## What to build

Keep both promises: send the notices. Dropping the sentences instead would leave a trainee who lost a STAR, and a refused requester, finding out by accident.

**One way to send a one-off notice, shared with the sibling notifications task.** A notice is not a digest, so:
- It is sent only after the save commits, never before. A refused command mails nobody. This follows the audit trap (T201): nothing that can fail runs after the mutation.
- It goes to the account through IUserAdministrationService.GetByIdAsync.
- It skips a deactivated account (UserIdentityDetails.IsDeactivated), as T274 asks of the expiry notices, but not an account that opted out of digest emails. DataRights.razor:161-165 already says one-off mail still arrives.
- A failed send is logged and never fails the command.
- A notice carries no free text written about the person: no revocation reason, no decision note. Mail is outside Wombat's access control, and T101 redacted these texts even from the audit log. The mail names what happened, when and by whom, and links to where it can be read.

**Revocation.**
- Add EntrustmentDecisionRevokedEmail, modelled on EntrustmentDecisionExpiredEmail: the EPA code and title, the level, the date revoked and who revoked it, by name. It says the reason is on the certificate and links to /portfolio/authorisations.
- RevokeEntrustmentDecisionCommandHandler sends it to decision.TraineeUserId after the save.
- My authorisations gains a "No longer in force" section listing her revoked and expired STARs, each with its status, the date and a Download of its certificate, which already states the reason. That way the notice points somewhere and the card no longer just vanishes. **Recommendation:** list revoked and expired only; superseded STARs stay off the page, because another STAR replaced them.
- Keep Index.razor:106's sentence, now true. Consider "The trainee is emailed."

**Data-rights decisions.**
- Add one DataRightsDecisionEmail: the request's type, the date made, and Approved, Completed or Rejected, with a link to /account/data-rights. It carries no note, since the note is read on the page.
- Approve and Reject send it after their save.
- **Erasure** is the exception: once the executor has run, the requester can no longer sign in (Step A.1.13). So read the address before ExecuteAsync, and send a confirmation that the erasure was carried out, with no link, after it commits. Nothing stores the address; T282 already logs mail by reference, not address.
- The requester's own list (GetMyDataRightsRequests, DataRightsRequestSummaryDto) carries DecidedOn and DecisionNote, and DataRights.razor shows them for a decided request, for example as a row beneath it or a "Decision" cell.
- **Recommendation:** do not show the decider. The admin pages keep the decider's id on purpose (DESIGN.md § Page shapes, "Five pages keep ids"), but on the requester's page an id means nothing. The note is the institution's answer, not a person's.
- Keep DataRights.razor:262's sentence, now true.

Rectification that is approved stays Approved, because applying it is not built (coverage.md; T112). Its notice therefore says Approved. That is not this task's to finish.

## Verification

- [ ] RevokeEntrustmentDecision with a recording IEmailSender mails the trainee exactly once, after the save, naming the EPA, the level and the reviser; the body contains no part of the reason. Application test.
- [ ] A refused revocation (out of scope, unknown id, already revoked) mails nobody; a deactivated trainee is skipped and the revocation still stands. Application tests.
- [ ] A throwing sender does not fail the revocation or either data-rights decision; the saved state stands and the failure is logged. Application test.
- [ ] My authorisations lists a revoked STAR under "No longer in force" with its status, date and a named Download of its certificate; an active STAR stays in the main list. bUnit.
- [ ] Approve and Reject each mail the requester exactly once, after the save, with no note text; an erasure's approval mails the pre-erasure address once after the erasure commits, with no link. Application tests.
- [ ] GetMyDataRightsRequests returns DecidedOn and DecisionNote for the caller's own decided requests; DataRights.razor shows them; a submitted request shows neither. Application test + bUnit.
- [ ] Browser, the runbook against an SMTP sink: Step 4.36, the sink gains one mail to Dr Dlamini naming PAED-002; Step 4.42, My authorisations shows PAED-002 as revoked with its certificate; Steps A.1.2/A.1.3, one mail to Dr Dlamini; A.1.8/A.1.9, one mail to Dr Mahlangu and her row shows the note; A.1.12, one confirmation to Dr Ndlovu's former address. The runbook's Expect lines for 4.36, 4.42, A.1.3, A.1.8, A.1.9 and A.1.12 are updated to say so, and screenshots re-captured into design/baseline/.

## Related

The sibling task holds activity-move and committee-outcome mail (C59) and shares the one-off notice path; whichever lands first builds it. T274 skips deactivated accounts for expiry notices, and this task uses the same rule. T101 redacts the revocation reason and the decision notes. T112 covers data-rights scope and the unbuilt rectification apply. T240 wrote the help text saying one-off mail is still sent. T258 and T276 cover erasure. T282 logs mail by reference. Runbook: Steps 4.36, 4.37, 4.42, A.1.2, A.1.3, A.1.8, A.1.9, A.1.12, A.1.13. Findings F-4.36a, F-A.1.2a, F-A.1.9a.
