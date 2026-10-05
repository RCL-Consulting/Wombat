---
id: T359
title: Staff cannot reassign a waiting request to another assessor
status: queued
priority: P3
owner: agent
depends_on: [T358]
created: 2026-10-05
---

# T359 — Staff cannot reassign a waiting request to another assessor

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A request stuck on an absent or deactivated assessor can only be cancelled by the registrar and
filed again; nothing is lost, but the chase is the registrar's, not the programme's.
**Surfaced:** 2026-10-05, flow 06 round 1 (T358): the operator chose to build "Send a reminder" in flow 06 and to file
"Reassign" as its own task (question 9).

## Symptom

On Waiting for assessors (flow 06), a speciality admin or the Coordinator can remind a request's named assessor but
cannot hand the request to another assessor. Nobody can.

## Root cause

A request's assessor is a nominee field written by the registrar. While the request waits (for the Mini-CEX, state
`requested`), the state is editable by `field:assessor_user_id` and the field by its section's default,
`subject|creator` (`src/Wombat.Infrastructure/Activities/Seeds/mini_cex_cpsa/workflow.json:10–12`, `schema.json:7–23`).
Writing needs both (`FieldPermissionEvaluator.cs:18`, `:80`), and the assessor is never the subject (NomineeGate), so
in that state nobody may write the field. The only way round is cancel (`workflow.json:51–59`) and File again.

## What to build

Undecided; decide first:
- who may reassign (both speciality admins? the Coordinator? the institutional admin?), and on which states;
- whether it is a workflow transition (declared per type, so the activity platform's DSL and the seeds change) or a
  staff-only field write outside the workflow;
- that the new nominee passes `NomineeGate` (an active holder of the field's role at the stamped institution);
- what the history records, whether the registrar and the old and new assessors are told, and whether the wait
  restarts.
Then a row action on Waiting for assessors, beside Send a reminder (flow 06's place for it).

## Verification

- [ ] A speciality admin reassigns Dr Mahlangu's requested Mini-CEX from Dr Zulu to Dr Khumalo; the activity's history
  records it and Dr Khumalo's inbox lists it — checked by a runbook step and a replay.
- [ ] A reassignment to someone `NomineeGate` refuses is refused with a sentence — checked by a test.
- [ ] All six suites green.

## Related

T358 (flow 06), D50 (whom the nudge skips), NomineeGate, `FieldPermissionEvaluator`, CLAUDE.md § Activity platform.

## Notes

- **From flow 06's build review (R1, 2026-10-05).** A reminder's history and the same-day block are keyed by request, not by the nominee it was sent to (`WaitingForAssessorsReader`: `LastReminder`, `RemindedToday`; `SendActivityReminderCommand`; the unique index `IX_ActivityReminders_ActivityId_SentOnDay`). Unreachable today, since nobody may change a waiting request's assessor. A reassignment makes it real: after one, the new assessor would read "Reminded … by …" for a mail the old one got, and could not be reminded until the next South African day. Scope both to the current holder (`ActivityReminder.AssessorUserId`), or decide the rule stays per request, when building this task.
