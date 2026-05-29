# T068 — Trainees cannot create any activity (admin-only read guard on the schema query)

**Status:** in_progress (found + fixing during Act 3 play-through, 2026-05-29)
**Severity:** High — blocks the entire Act 3 operational loop (submit → rate → complete → credit).
No trainee can create any activity of any type.
**Surface:** `src/Wombat.Application/Features/Activities/Queries/GetActivityTypeEditor/GetActivityTypeEditorQuery.cs`
(consumed by `src/Wombat.Web/Components/Pages/Activities/NewActivity.razor`).

## Symptom

Trainee signs in → `/activities/new` → selects a published activity type (e.g. Mini-CEX).
The `@bind:after="LoadSelectedTypeAsync"` handler fires `GetActivityTypeEditorQuery`, which throws
`InvalidOperationException: The activity type could not be found.` → Blazor circuit terminates
("An unhandled error has occurred." banner). The schema-driven form never renders, so **no draft
can be created** and the rest of the activity lifecycle is unreachable.

Server stack (NewActivity.razor:70 → GetActivityTypeEditorQuery.cs:99).

## Root cause

`NewActivity.razor` (the trainee's create page) reuses the **admin builder's**
`GetActivityTypeEditorQuery`. That handler calls `CanReadAsync`, which returns true only for
`Administrator` or a scope-matched `InstitutionalAdmin`; for any other principal it returns false,
and the handler converts a false read into the same "could not be found" throw (line 99). A Trainee
(or Assessor, Coordinator, etc.) therefore always fails, and because the Razor handler doesn't
catch it, the circuit dies.

The dropdown itself is populated by a different, correctly-scoped query (`ListActivityTypesQuery`),
which is why the type list shows but selecting one crashes.

## Fix

A *published, active* activity type is a form template — reading its schema is not sensitive and is
required for anyone who can log an activity against it. Add a branch at the top of `CanReadAsync`:

```csharp
if (activityType.IsActive && activityType.Version > 0)
{
    return true;
}
```

Drafts (`Version == 0`) and inactive types remain admin/institutional-admin-only via the existing
branches below, so the builder's draft-editing semantics are unchanged. `CreateDraftAsync`
(the Save-draft/Submit path) has no admin gate of its own, so this single change unblocks the full
create flow.

## Verification

- Build clean.
- Browser: Dlamini (Trainee) → `/activities/new` → select Mini-CEX → full 12-field schema form
  renders, no circuit crash; Save draft creates `/activities/{id}`; Submit advances to Submitted.
- Admin builder draft editing still works (drafts still admin-only).
- Then: Act 3 Phases 3.A–3.I proceed end-to-end.
