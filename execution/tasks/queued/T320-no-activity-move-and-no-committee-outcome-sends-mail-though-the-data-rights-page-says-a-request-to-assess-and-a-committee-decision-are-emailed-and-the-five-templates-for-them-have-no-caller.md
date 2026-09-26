---
id: T320
title: No activity move and no committee outcome sends mail, though the data-rights page says a request to assess and a committee decision are emailed, and the five templates for them have no caller
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T320 — No activity move and no committee outcome sends mail, though the data-rights page says a request to assess and a committee decision are emailed, and the five templates for them have no caller

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Every user is told on /account/data-rights that a request to assess and a committee or entrustment decision are emailed, and neither ever is. So:
- an assessor learns of a request only by opening the inbox, or from the nudge after five days;
- a trainee is never told that an assessment was completed or declined, or that a review was ratified and STARs issued;
- an appeal body is never told that an appeal was lodged.
The work is not lost, since every page and dashboard shows it. That is why this is Medium, not High. Five dead templates, one of them in the retired "STAR reflection" sense, would also mislead whoever builds this next.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-4.27a).

## Symptom

- **Activity moves send nothing.**
  - Step 3.3: Dr Dlamini submits her Mini-CEX and it reads Requested (design/baseline/act-3/3.3-1-requested.png). Dr Naidoo gets no mail and finds the request only in his inbox.
  - Completions, declines and returns in Acts 3, 5 and 6 (5.24, A.2.7, 6.16, 6.18, A.7.2) mail nobody either.
- **Committee outcomes send nothing.**
  - Step 4.27: ratifying Dr Molefe's review issues STARs #1 to #3 (design/baseline/act-4/4.27-1-zulu-ratified-stars-issued.png), and she is told nothing.
  - Steps 4.43 to 4.48: an appeal is lodged, noted and remitted, and neither the appeal body nor the trainee gets mail.
  - The SMTP sink stayed at 89 through all of Act 4. At the end of the run its 96 files were invitations, MSF feedback requests, nudges, digests, draft reminders and one graduation mail, and none concerned a move, a ratification or an appeal.
- **The promise.** /account/data-rights's digest help text reads: "Email about one particular thing is still sent, such as a request to assess, an invitation to give feedback, or a committee or entrustment decision." It is visible in design/baseline/states/data-rights--rejected.png.
- **What the runbook records.** Steps 3.3 and 5.24 record today's behaviour ("Wombat mails nobody when an activity moves"), and coverage.md files it as "Undecided". The README's own example step 3.4 expects "Dr Dlamini is emailed that it was completed."

## Root cause

- **Dead templates.** T012 (b1173faa) built five templates in src/Wombat.Application/Common/Email/Templates/ and wired none of them: AssessmentRequestedEmail, AssessmentAcceptedEmail, AssessmentCompletedEmail, AssessmentDeclinedEmail and StarDecisionEmail. grep finds no reference outside their own files. Its note called assessment emails "cosmetic".
- **No sender on these paths.** IEmailSender is injected only in IssueInvitation, ResendInvitation, OpenMsfCampaign, ResendMsfLinks, CompleteTraineeProfile and the five scheduled jobs.
  - ActivityService.TransitionAsync (src/Wombat.Infrastructure/Activities/ActivityService.cs:367) has none.
  - RatifyCommitteeDecision.cs:160, LodgeAppeal.cs:59 and ResolveAppeal.cs:119 save and return.
  - There is no INotificationHandler anywhere in src.
- **A promise made without the feature.** T240 wrote the help text (DataRights.razor:161-165) describing mail that does not exist.
- **The design was planned, never decided.** CUSTOMIZATION.md § 7 (line 610) plans for "which events trigger notifications" to come from the workflow definition. No D-number decides it, and coverage.md:116 says "Undecided".
- **The templates are wrong as well as unused.**
  - StarDecisionEmail says "Your STAR reflection has been approved/declined". That is the pre-rewrite meaning. A STAR is now a Statement of Awarded Responsibility (T296), issued by ratification and never declined.
  - AssessmentAcceptedEmail names a state that no CPSA workflow has (coverage.md; Step 3.51).
  - Each template takes a hard-coded vocabulary ("assessment request"), not the pinned workflow's labels.

## What to build

**1. Decide first, and record the decision** as a D-number in EPA-PROGRAMME § 3, with what was rejected. The recommendation is below.

**Activity moves.** Derive the recipients from the pinned workflow, with no new DSL property:
- A move whose new state has outgoing transitions for a `field:` actor mails the person that field names. Example: submit, Draft → Requested, mails the assessor in `assessor_user_id`.
- A move made by someone other than the subject mails the subject. Examples: complete, decline, return, sign-off.
- A move the subject makes themselves (submit, cancel) does not mail the subject.
- `role:` and `scope:` actors are never mailed. A role is many people, and the inbox and the nudge serve them.
- **Rejected alternative:** a per-transition `notify` property in the workflow DSL. It gives an institution control, but it needs Parse and Serialize, a SeedRoundTripTests case, builder UI and re-authored seeds (CLAUDE.md, "Parse is not enough"). Choose it only if the operator wants institutions to tune notifications.

**Committee outcomes.**
- Ratification mails the trainee: the review's outcome, and each STAR issued, by EPA and level.
- A lodged appeal mails the appeal body's chair (PanelSeat.AppealBodyAt).
- A resolved appeal (upheld, dismissed or remitted) mails the trainee.
- Revocation belongs to the sibling task.
- Scheduling a review is left out; it is not a decision.

**2. Wire it through the sibling task's one-off notice path:**
- mail only after the save commits, so a move refused by the tool, nominee or encounter-date gate mails nobody (T201);
- skip a deactivated account, but not the digest opt-out;
- a failed send never fails the move;
- no free text in the mail: no form data, notes, rationale or appeal reason (T101 redacts these even from the audit log). The mail names the event, the activity type, the EPA code and who acted, and links to the page.

For activities, compute the recipients among the reads before ApplyTransition, and send after SaveChanges. Resolve "who acts next" through ActorFieldRules. AssessorPendingNudgeJob parses `field:` actors on its own (HasFieldUserActor, GetFieldName); move it onto the same helper so the nudge and the notice cannot disagree about who the nominee is.

**3. Templates.**
- Delete the five dead templates.
- Replace the four activity templates with one activity-move template that prints the pinned workflow's state and move labels, never keys (DESIGN.md, "A workflow state or move is shown by its label", T220).
- Replace StarDecisionEmail with a review-outcome template and an appeal template in current vocabulary.
- PasswordResetEmail is also uncalled, but it belongs to the unbuilt self-service reset (coverage.md; A.4.4). Leave it to that work, or delete it here and say so.

**4. Make the help text and the runbook true.**
- DataRights.razor:161-165's help text must name what is actually sent (DataRightsDigestOptOutTests pins it).
- Rewrite the runbook's Expect lines that say nothing is sent: 3.3, 5.24, A.2.7, A.7.2, plus 4.27 and 4.43 to 4.48.
- Change coverage.md:116 from "Undecided" to the decision.

## Verification

- [ ] The decision is recorded as a D-number in EPA-PROGRAMME § 3, with the rejected alternative.
- [ ] Submitting a Mini-CEX (Paediatrics) mails the named assessor exactly once, after the save; complete and decline each mail the subject once, and the decline mail contains no part of the note; cancel by the subject mails nobody; a `role:` actor is never mailed. Infrastructure tests against ActivityService with a recording sender.
- [ ] A move refused by ToolPermissionGate, NomineeGate or EncounterDateGate mails nobody; a deactivated nominee is skipped and the move stands; a throwing sender does not fail the move. Infrastructure tests.
- [ ] The nudge job and the move notice name the same nominee for every seeded workflow (one shared resolver). Test over the seed catalogue.
- [ ] Ratify mails the trainee once, naming each STAR issued; LodgeAppeal mails the appeal body's chair once; ResolveAppeal mails the trainee once; a refused ratify (quorum, not chair) mails nobody. Application tests.
- [ ] grep finds a caller in src for every template under Common/Email/Templates (StarDecisionEmail and the four Assessment* templates are gone); no template prints a state key.
- [ ] DataRightsDigestOptOutTests pins help text that names only mail that is actually sent.
- [ ] Browser, the runbook against an SMTP sink: Step 3.3, one mail to Dr Naidoo naming the Mini-CEX and PAED-001; the completing step in Act 3, one mail to Dr Dlamini; Step 4.27, one mail to Dr Molefe naming STARs #1–#3; Steps 4.43 and 4.47, one mail each to the appeal body's chair and to Dr Mahlangu. The Expect lines of 3.3, 5.24, A.2.7, A.7.2, 4.27 and 4.43–4.48 and coverage.md:116 are updated, and screenshots re-captured into design/baseline/.

## Related

The sibling task (C57 and C58: revocation and data-rights notices) builds the shared one-off notice path. T012 created the dead templates. T240 wrote the help text. T151, T274 and D50 cover the recipient rules for the periodic mail. T201 covers the audit trap. T101 covers redacted texts. T220 shows labels, not keys. T296 settled STAR's meaning. D46 covers panels and the appeal body. CUSTOMIZATION.md § 7 plans workflow-driven notifications. Runbook: Steps 3.3, 3.51, 4.27, 4.42–4.48, 5.24, 6.16, 6.18, A.2.7, A.7.2; README example step 3.4; coverage.md:116. Finding F-4.27a; the author and reviewer suspects in Acts 3, 4 and 6.
