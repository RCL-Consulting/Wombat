---
id: T220
title: Activity pages, lists and dashboards show workflow state and move keys, not their labels
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T220 — Activity pages, lists and dashboards show workflow state and move keys, not their labels

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Three seed states differ from their key: `submitted` is "Awaiting supervisor", "Awaiting review"
or "Awaiting discussion". Elsewhere the key and label differ only in case.
**Surfaced:** 2026-09-25, the T189 review (the display half of its finding 1).

## Symptom

- `ActivityView.razor` prints `CurrentState` in its header ("State: {key}") and its summary. Its history prints
  `TransitionKey` and "FromState → ToState".
- `MyActivities`, `ActivityInbox` and `TraineeDashboard` print the state key.

T189 made refusals and notices use labels, so the pages disagree with their own notices.

## What to build

Carry the state and move labels (`Workflow.StateLabel` and the move labels, from the pinned version) in the activity DTOs,
and print them everywhere a key is shown today.

## Verification

- [x] Every surface above shows labels. bUnit, and a browser check on a clinical audit ("Awaiting supervisor").

## Related

T189, T172.

---

## As built — 2026-09-25 (`a3ae666`)

Workflow states and moves are printed by their labels from the activity's pinned version. This covers `ActivityView`
(header, summary and history), My activities, the Inbox, the trainee and assessor dashboards, the PDF and the NewActivity
notice. The key is used only where a pinned workflow no longer declares it.

The committee evidence snapshot freezes the source state's label (`CommitteeEvidenceItem.SourceStateLabel`, migration
`T220_CommitteeEvidenceSourceStateLabel`). Snapshots taken before it fall back to the key; W-007 would allow a backfill,
and none was needed.

Browser on dev (scripted Chrome, master `ec58d2e`; `pg_dump` first, at `recovery/pre-g2-migrations.dump`): clinical audit #41:
- **Submit** read "Submitted. It is now Awaiting supervisor."
- **The page** reads "State: Awaiting supervisor". Its history reads "Submit | Draft → Awaiting supervisor", then "Sign
  Off | Awaiting supervisor → Signed off".
- **The lists.** My activities, the dashboard badge and the assessor's inbox read the labels.
- **The PDF** prints "State: Signed off".
- **Review 10's evidence picker** reads "Clinical Audit (Paediatrics) #41 · Unrated · 2026-09-20 · Signed off".
- **Not run:** the "accepted" state; no offered type has one.

**Filed from the review:** [T250] (committee review states print enum names).
