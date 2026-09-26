---
id: T302
title: An InstitutionalAdmin can deactivate her own institution, or reactivate one an Administrator deactivated, with the Active box on its edit page
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-09-26
completed: 2026-09-26
---

# T302 — An InstitutionalAdmin can deactivate her own institution, or reactivate one an Administrator deactivated, with the Active box on its edit page

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High. It is an authorization bypass of a rule T056 made the Administrator's alone. Unticking the box turns off her institution's onboarding: an inactive institution issues no invitations. Re-ticking it undoes an Administrator's deactivation and restarts onboarding there.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-1.23b, F-A.6.3a).

## Symptom

Prof Mbatha (InstitutionalAdmin, KGK) opens KGK's record at /admin/institutions/2 by typing its address.
- **Deactivate is refused.** Pressing it gives 'Only global administrators may deactivate institutions.' (Step A.6.3; design/baseline/act-A/A.6.3-2-deactivate-refused.png).
- **The Active box is not.** She unticks Active, presses Save, and reads 'Institution saved.'. The database then holds KGK inactive (`SELECT "Id","ContactEmail","IsActive" FROM "Institutions"` gave `2|hod.paediatrics@kgk.wombat.local|f`; design/baseline/act-A/A.6.3-3-untick-active-saved.png). devadmin had to tick it again (A.6.3-5-devadmin-reactivates-kgk.png).
- **Step 1.23 shows the box to her on first sight** (design/baseline/act-1/1.23-1-kgk-own-record.png).
- **What an inactive KGK does:** every invitation she issues is refused with 'The selected institution was not found.'.
- **The reverse, from the code:** she can open and reactivate her institution after an Administrator has deactivated it.

## Root cause

- `UpdateInstitutionCommandHandler.cs:20` asks only `CanAccessInstitution(request.Id)`. `:31` then writes `institution.IsActive = request.IsActive` for any in-scope caller.
- `DeactivateInstitutionCommandHandler.cs:20-23` refuses anyone who is not `IsAdministrator()`, as T056 decided: 'CreateInstitution and DeactivateInstitution are restricted to Administrator outright'. The update path was never held to that rule.
- `InstitutionEdit.razor:52-60` renders the Active box to every caller on an existing record, and `:159` sends it.
- `GetInstitutionByIdQueryHandler.cs:20-28` does not filter on `IsActive`, so an inactive institution still opens for its admin.
- `InvitationRules.cs:81-85` is the one reader of `Institution.IsActive`.
- The only test, `InstitutionalAdminScopeTests.cs:188-196`, covers another institution's record, not her own `IsActive`.

## What to build

**The active state is the Administrator's alone, in the command, not only on the page.**
- **The command.** Take `IsActive` out of `UpdateInstitutionCommand`, and add an Administrator-only `ReactivateInstitutionCommand` beside `DeactivateInstitutionCommand`. Each state change is then one command with one rule (compatibility is not a constraint).
- **The fallback.** If a separate command is not wanted, `UpdateInstitutionCommand` refuses a change to `IsActive` from a non-Administrator with Deactivate's wording, before any field is written (the audit trap).
- **Her other edits stay.** An InstitutionalAdmin keeps editing her own institution's name, short code and contact email.
- **The page.** It shows Status as text to an InstitutionalAdmin, and shows the Active and Deactivate/Reactivate controls only to those the Administrator policy admits (`IAuthorizationService`, DESIGN.md § Table system, T211).
- **Related work.** The Deactivate button and the Back and Cancel links on the same page are T291's item 7, and Deactivate's missing confirmation is T264. Land them together if convenient.

## Verification

- [x] An InstitutionalAdmin cannot change KGK's state through the update — met by construction: the update carries no
  state (`TheUpdate_CarriesNoActiveState_SoNoUpdateCanChangeIt`, by reflection), and
  `HerUpdateOfTheInstitutionAnAdministratorDeactivated_LeavesItInactive`. The audit-trap check is
  `AnInstitutionalAdminWhoDeactivatesHerOwnInstitution_IsRefused_AndAfterTheAuditSave_ItIsUnchanged`.
- [x] She cannot reactivate; an Administrator can deactivate and reactivate —
  `AnInstitutionalAdmin_CannotReactivateHerInactiveInstitution`, `AnAdministrator_DeactivatesAndReactivatesIt`.
- [x] She still saves her institution's name, short code and contact email —
  `AnInstitutionalAdmin_StillSavesHerOwnInstitutionsNameShortCodeAndContactEmail`.
- [x] bUnit: Status as text for her, the Active box and Deactivate for an Administrator — `InstitutionEditStatusTests`
  (12 cases, 11 failed before), with the policy, not the role, deciding (after review).
- [x] Browser, 2026-09-26: Step A.6.3 (rewritten for the new page) as Prof Mbatha on a post-Act-6 copy: "Institution
  saved.", Status "Active" as text, no box, no Deactivate, Back to home and Cancel to `/`, and SQL
  `2|hod.paediatrics@kgk.wombat.local|t`. Step 1.23 on a post-Act-1 copy: no Active box. As devadmin (Step 1.6's
  Expect), the box and Deactivate remain, behind a dialog that names KGK.

## As built — 2026-09-26 (`41be531`)

- `IsActive` is out of `UpdateInstitutionCommand`; an Administrator-only `ReactivateInstitutionCommand` sits beside
  `DeactivateInstitutionCommand`, so each state change is one command with one rule.
- The page decides by the `Administrator` policy through `IAuthorizationService` (T211). An Administrator keeps the
  Active box: Save sends the update, then Deactivate or Reactivate only if the user changed the box (after review, so a
  concurrent change is not undone), and unticking asks first. Everyone else sees Status as text, "Set by a global
  administrator.".
- Landed with it, on the same page: T291 item 7 (Back and Cancel lead home for a non-Administrator; Deactivate only for
  an Administrator) and T264's institution part (Deactivate as an outline button behind a named ConfirmDialog). Both
  tasks carry a note of what remains.
- Refusals use `RefusalText.Of`. No migration.
- Runbook: Step A.6.3 rewritten for the new page; its stale SQL comment dropped. Baseline: 11 captures re-taken, and the
  ones whose state no longer exists hold the new page under the same names (BRIEF § 10).

## Related

T056 (Deactivate and Create are the Administrator's), T211 (DESIGN.md:369), T291 (note item 7: Deactivate, Back and Cancel on the same page), T264 (Deactivate's confirmation), T272 (this handler's 'already exists' wording). Runbook Steps 1.23 and A.6.3.
