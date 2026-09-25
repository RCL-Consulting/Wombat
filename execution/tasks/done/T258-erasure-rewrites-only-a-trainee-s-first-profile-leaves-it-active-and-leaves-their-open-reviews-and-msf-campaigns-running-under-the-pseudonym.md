---
id: T258
title: Erasure rewrites only a trainee's first profile, leaves it active, and leaves their open reviews and MSF campaigns running under the pseudonym
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T258 — Erasure rewrites only a trainee's first profile, leaves it active, and leaves their open reviews and MSF campaigns running under the pseudonym

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. An erased person's records stay live: a panel can still ratify their review, and a release writes
activities for the pseudonym.
**Surfaced:** 2026-09-25, the T238 review (out of scope). Read from the code, not run.

## Symptom

`ErasureExecutor`:
- rewrites only the **first** trainee profile and the **first** assessor profile, so a trainee's other profiles stay
  under their original id;
- leaves the rewritten profile **active**;
- moves the trainee's open committee reviews to the pseudonym, where the panel can still start, record and ratify them
  (`DemandTraineeAtPanelInstitutionAsync` finds the pseudonym's profile);
- moves their MSF campaigns to the pseudonym, where the coordinator can still invite, open, close and release them
  (`IsSubjectInScopeAsync`). A release then writes activities for the pseudonym.

T238 keeps a pseudonym out of new scope. It does not end what was already open.

## What to build

In erasure itself:
- Rewrite every profile of the user, and deactivate them (T209's `DeactivatedOn` = the erasure day).
- End what is open: withdraw open or draft MSF campaigns, anonymising their invitations as a withdraw does. Close or
  withdraw open committee reviews with a recorded reason. Refuse a pending STAR stage.
- Keep what is settled: ratified STARs and released reports, under the pseudonym.

Record the rule in CUSTOMIZATION.md or DOMAIN.md § data rights.

## Verification

- [x] After erasure, no profile under the original id remains, every pseudonymised profile is inactive, and no open
      review or campaign remains. Postgres test with the real `ErasureExecutor`.

## Related

T238, T026 (data rights), T207, T209.

---

## As built — 2026-09-25 (`9770042`)

Erasure (`ErasureExecutor`) pseudonymises every trainee and assessor profile of the person and deactivates them
(`DeactivatedOn` = the erasure day). Then:
- **Open MSF campaigns** (draft, open, closed or under review) are withdrawn, and their invitations anonymised.
- **Open committee reviews** are withdrawn with a recorded reason (migration `T258_CommitteeReviewWithdrawn`: state
  Withdrawn, `WithdrawnOn`, `WithdrawalReason`), and their staged decisions are dropped.
- **A review under appeal** stays answerable.
- **Settled records** (ratified STARs, released reports) stay, under the pseudonym.

The request's completion and the erasure are one transaction, and a concurrency conflict reaches the approver as a
readable refusal. Withdrawn reviews read as such on every card. Postgres tests run the real executor.

**Decision for the operator:** a trainee may request erasure whatever state their reviews are in, because approval
withdraws the open ones (CUSTOMIZATION.md § 5).

Browser on dev (scripted Chrome, master `f19417d`; `pg_dump -n public` first, at `recovery/pre-t258-migration.dump`): a new trainee, `erase-me@wombat.local`, was invited, registered from the emailed link and admitted, so that the dev
trainee was not erased.
- **Open records:** reviews 17 (Scheduled), 18 (formative) and 19 (started), and draft campaign 22 with one invitee.
- **The request:** submitted with review 17 open, with no refusal. The coordinator approved it, and it read Completed.
- **Reviews 17–19** read "Withdrawn · None: the review was withdrawn", each with the Withdrawn row, the reason and no
  actions. Review 17's cards read as the steps expect.
- **Campaign 22** reads Withdrawn, with its address removed.
- **SQL.** Profile 3 is `deleted_user_cc1c5436`, inactive, with `DeactivatedOn` 2026-09-25. Nothing remains under the
  original id, and nothing is open under the pseudonym. The dev trainee's reviews and campaigns are unchanged.

**Filed from the review:** [T276] (in-flight activities, nominee ids, concurrency, and two leftover addresses) and a note
on [T264] (approval has no ConfirmDialog). The 30-minute session window is noted on [T279].
