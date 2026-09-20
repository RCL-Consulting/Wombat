# T095 — Two minor observations from the Acts 3–4 replay (Coordinator panel name; new-activity Submit)

**Status:** DONE (2026-06-16, Opus) — both surfaced during the T091 Acts 3–4 replay; fixed + verified.

## 1. Coordinator "Stalled requests" panel showed the trainee's raw UserId GUID
**Symptom (Act 3.G):** the Coordinator dashboard "Stalled requests" panel listed
`Mini-CEX (Paediatrics) — 22e35b71-06d2-4ac9-…` — the raw `SubjectUserId` GUID instead of a name.
**Root cause:** `GetCoordinatorDashboardSummaryQueryHandler` projected `a.SubjectUserId` straight into
`StalledRequestItem.SubjectName` (the DTO field is named `SubjectName` but was being fed the id).
**Fix:** the handler now injects `IUserAdministrationService` and resolves each stalled subject's display
name (First + Last, falling back to email, then the id) — names cached per id, ≤10 rows. +1 assertion in
`CoordinatorDashboardQueryTests.WithStalledActivities_ReturnsThem` (`SubjectName == "Test Trainee"`); the
4 existing tests gained a stub `IUserAdministrationService`.

## 2. `/activities/new` "Submit" failed for activity types whose first transition isn't keyed "submit"
**Symptom (Act 3.D, seen in the audit log):** clicking **Submit** on the new-activity page for a
`procedure_log` (or MSF) created the draft but recorded a **FAILED** `TransitionActivityCommand` — the page
hard-coded the transition key `"submit"`, which only exists for the 4-state assessor types; procedure_log
uses `"log"` and MSF uses `"open"`. The trainee had to open the activity and use the detail-page button.
**Fix:** `NewActivity.razor` now resolves the first transition out of the workflow's **initial state** that
the current user is allowed to take (via `WorkflowParser` + `IWorkflowEvaluator`, the same pattern
`ActivityWorkflowActions` uses) instead of the hard-coded key. Falls back to "Draft created. Open the
activity to take the next action." when no transition is available.
**Live-verified:** Ndlovu created a procedure_log via `/activities/new` → **Submit** → activity reached
`logged`; audit shows `TransitionActivityCommand` **Success=true** (previously FAILED).

## Tests / build
Application **314**, Architecture 19, Web 39, Domain 45 — all green; Release build clean (0 warnings).
