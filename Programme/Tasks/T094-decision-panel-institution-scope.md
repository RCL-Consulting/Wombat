# T094 — Speciality-scoped DecisionPanel loses its InstitutionId (InstitutionalAdmin can't schedule on it)

**Status:** DONE (2026-06-15, Opus) — surfaced during the T091 Act-4 replay; fixed + tested.

## Symptom
During the Act-4 replay, Mbatha (InstitutionalAdmin) scheduled committee reviews at
`/committee/reviews`. The **Panel dropdown was empty** — her own panel "Paed Annual Review
Panel 2026" (created in Act 2, Speciality-scoped) did not appear, so she could not schedule
any reviews. The whole Act-4 committee cycle was blocked.

## Root cause
`CreateDecisionPanelCommandHandler` persisted `InstitutionId` **only** for `Institution`-scoped
panels (`InstitutionId = request.Scope == DecisionPanelScope.Institution ? request.InstitutionId : null`),
forcing it to `null` for `Speciality`-scoped panels — directly contradicting the handler's own
comment that "the panel carries its own institution regardless of scope" (post-T091, the speciality
is a *national* catalogue entry and can no longer source the institution). The panel-create form
(`PanelEdit.razor`) shows **no institution picker** for Speciality scope, so the request also carried
no `InstitutionId`. Result: the panel was stored with `InstitutionId = NULL`.

`ListDecisionPanelsQueryHandler` filters an InstitutionalAdmin's panels by
`panel.InstitutionId == scopedInstitutionId`, so a NULL-institution panel is invisible to its own
creator. This is exactly the "speciality-scoped DecisionPanels must carry their own InstitutionId"
provisional risk flagged in `CLAUDE.md` (T091 P2).

## Fix
`src/Wombat.Application/Features/CommitteeDecisions/CreateDecisionPanel.cs`:
1. Persist the **resolved** institution for **all** scopes (`InstitutionId = resolvedInstitutionId`),
   not just Institution scope.
2. `ResolveInstitutionIdAsync`: when the request supplies no `InstitutionId` **and** the caller is an
   InstitutionalAdmin (not a global Administrator), pin the panel to the caller's own institution
   (`principal.GetInstitutionId()`). An explicit request value is still honoured and validated by the
   existing `CanAccessInstitution` guard — so the prior "InstitutionA admin passes InstitutionB ⇒
   reject" behaviour is unchanged.

A global Administrator still supplies the institution explicitly via the form (Institution scope) and
is unaffected for Speciality scope (Administrators see all panels regardless).

## Test
`tests/Wombat.Application.Tests/Features/CommitteeDecisions/PanelScopeGuardTests.cs`:
`Create_InstitutionalAdmin_SpecialityPanel_PinsToOwnInstitution` — a Speciality-scoped panel created by
an InstitutionA admin with `InstitutionId: null` now persists `InstitutionId == InstitutionA`. The
existing `Create_InstitutionalAdmin_CannotCreateSpecialityPanelOutsideInstitution` (explicit foreign id
⇒ reject) still passes. **Application 313 → 314 green; Architecture 19; Release build clean (0 warnings).**

## Replay note
The existing panel in `t091-act3-complete`/`t091-act4-complete` was created **before** this fix, so it
still carried `InstitutionId = NULL`. It was **backfilled** to `InstitutionId = 2` via SQL to unblock the
Act-4 replay (the value it should have had). Fresh panels created after this fix get it automatically.
