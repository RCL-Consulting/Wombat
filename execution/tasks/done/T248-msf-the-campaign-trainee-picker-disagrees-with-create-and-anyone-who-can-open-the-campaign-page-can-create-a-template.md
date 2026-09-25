---
id: T248
title: MSF: the campaign trainee picker disagrees with create, and anyone who can open the campaign page can create a template
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T248 — MSF: the campaign trainee picker disagrees with create, and anyone who can open the campaign page can create a template

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T224 review.

## Symptom

- `ListTraineesForSpecialityQuery` lists every profile at the caller's institution, inactive ones included, and checks
  no role. Create refuses some of them (trainee-first, a current profile elsewhere).
- `CreateMsfTemplateCommand` takes no principal, so anyone who can open the campaign page can create a template,
  including someone who holds Trainee (T224's rule).

## What to build

- Build the picker from `TraineeScopeResolver.PreferredProfiles` and the rule create uses: one predicate, as T102 did
  for nominees. This overlaps [T238]'s erased and stale profiles, so share its rule.
- `CreateMsfTemplateCommand` takes the principal and applies `MsfCampaignRules.RunsNoCampaigns` before any write.

## Verification

- [x] The picker offers exactly whom create accepts. Handler tests.
- [x] A Trainee holder cannot create a template. Handler test (the audit trap).

## Related

T224, T238, T113, T102.

Note, 2026-09-25 (the G2 browser check), needs confirmation: as Trainee plus Coordinator, `/msf/reports/14` (their own
released campaign) showed the coordinator's report page. It had a Coordinator actions card with an editable Narrative
box and the supervision-level list, no action button, and no T224 warning. The subject should get the trainee's view
(`/msf/my-reports/{id}`), or the T224 warning, never the coordinator's card. Check whether a learner-feedback campaign
would show teaching contexts there.

---

## As built — 2026-09-25 (`fd5bde2`)

The campaign trainee picker offers exactly whom create accepts: current trainees at the caller's institution (T238's
rule, `ListMsfCampaignSubjectsQuery`, contacts from `GetContactsAsync`). Creating a template needs the right to run
campaigns (`RunsNoCampaigns` and the coordinator's institution), checked before any write. Handler, Postgres and bUnit
tests.

Browser on dev (scripted Chrome, master `8e00e68`):
- **The picker** offered only Demo Trainee, and a quick template was created.
- **A stale form.** With the profile deactivated by SQL, it was refused: "A multi-source feedback campaign can only be run
  for a trainee in a programme at your own institution." No campaign was created, and the profile was restored. The same
  held with the Trainee role removed through instadmin's UI (then re-added).
- **A Coordinator with no institution** (collegeadmin plus Coordinator, by SQL, then removed) saw only "…run by a
  coordinator at an institution, or by an administrator."
- **A Trainee plus Coordinator** saw only "You hold the Trainee role, so you cannot run multi-source feedback campaigns…".

**Filed from the review:** [T268] (a locked trainee is not current) and [T269] (the subject's own report).
