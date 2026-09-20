# T074 — Coordinator "Stalled requests" panel never matched (F-3G-1)

> **✅ FIXED 2026-05-30 (Opus) — commit `361fc6b`.**

**Status:** closed.
**Severity:** Medium — the coordinator's primary triage surface was dead.
**Surface:** `src/Wombat.Application/Features/Dashboards/Coordinator/GetCoordinatorDashboardSummaryQuery.cs`.

## Symptom (found in Act 3 Phase 3.G)

A Mini-CEX submitted by Mahlangu and left in `submitted` (then backdated 15 days in the DB to
simulate staleness) never appeared in Dr Smit's "Stalled requests" panel, even after the
`assessor-pending-nudge` job was triggered manually. The panel rendered but always showed
"No stalled requests."

## Root cause

The query filtered `a.CurrentState == "requested"` — **a state no activity workflow defines.** The
schema-driven model uses `draft -> submitted -> rated -> completed` (and variants); there is no
"requested" state anywhere. So the predicate matched zero rows by construction — the panel was dead
code. Secondarily it compared `a.CreatedOn` (draft-creation time) rather than `a.UpdatedOn` (the
time the activity actually entered `submitted`), which is the timestamp the sibling
`AssessorPendingNudgeJob` correctly uses.

## Fix

```csharp
.Where(a => a.CurrentState == "submitted" && a.UpdatedOn < stallCutoff);
```

with the projection + ordering switched from `CreatedOn` to `UpdatedOn`, and the DTO field
`StalledRequestItem.RequestedOn` renamed to `SubmittedOn` (UI label follows). Threshold unchanged:
`DashboardThresholds.CoordinatorStallDays` (default 7). Added a regression test
(`RecentlySubmittedOrNonSubmitted_AreNotStalled`) asserting that a recently-submitted activity, an
old draft, and an old completed activity are all excluded.

## Verification

- Unit: `CoordinatorDashboardQueryTests` — `WithStalledActivities_ReturnsThem` (now seeds a
  `submitted`, ~10-day-old activity) + the new negative test. Application suite green.
- The same backdated-activity scenario from Phase 3.G would now surface in the panel.

## Note

There is no dedicated "submitted-at" timestamp column; `UpdatedOn` is the proxy (it advances on each
transition). That's correct for the stalled-submission case because a `submitted` activity's last
transition *is* its submission.
