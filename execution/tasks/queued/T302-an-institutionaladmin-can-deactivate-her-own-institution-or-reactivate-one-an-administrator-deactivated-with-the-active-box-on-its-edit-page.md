---
id: T302
title: An InstitutionalAdmin can deactivate her own institution, or reactivate one an Administrator deactivated, with the Active box on its edit page
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
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

- [ ] Handler test: an InstitutionalAdmin of KGK who sends KGK's update with IsActive false is refused. After the audit pipeline's save, KGK is still active and its name, short code and contact email are unchanged.
- [ ] Handler test: an InstitutionalAdmin cannot reactivate her inactive institution. An Administrator can deactivate and reactivate it.
- [ ] Handler test: an InstitutionalAdmin still saves her own institution's name, short code and contact email.
- [ ] bUnit: InstitutionEdit shows an InstitutionalAdmin Status as text, with no Active box and no Deactivate. It shows an Administrator both.
- [ ] Browser, runbook Step A.6.3 replayed as Prof Mbatha: nothing on the page lets her change KGK's status, and SQL shows KGK IsActive true afterwards. Step 1.23: no Active box is offered.

## Related

T056 (Deactivate and Create are the Administrator's), T211 (DESIGN.md:369), T291 (note item 7: Deactivate, Back and Cancel on the same page), T264 (Deactivate's confirmation), T272 (this handler's 'already exists' wording). Runbook Steps 1.23 and A.6.3.
