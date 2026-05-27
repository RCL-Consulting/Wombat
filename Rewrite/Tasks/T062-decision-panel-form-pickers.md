# T062 — Decision Panel form pickers (Institution / Speciality / user-id replacement)

`src/Wombat.Web/Components/Pages/CommitteeDecisions/PanelEdit.razor` uses raw integer inputs for Institution / Speciality and raw GUID textareas for Chair / Member / External user ids. A real operator must run a DB query (or, in the 2026-05-27 Act 2 play-through, `psql` directly) to find the right IDs. Finding **A2-10**.

The fix follows the pattern T053 established on `ActivityTypeEdit.razor`: replace primitive inputs with scope-aware `<select>` and multi-select widgets backed by lookup queries.

## Change

### Institution / Speciality (numeric inputs → pickers)

Replace `InputNumber` for `panel-institution` and `panel-speciality` with `<select>` populated from `GetInstitutionsListQuery` and `GetSpecialitiesListQuery` (already used by T053). When caller is InstitutionalAdmin, the lists are scope-filtered automatically.

Both fields hide / show based on `_model.Scope`:
- `DecisionPanelScope.Institution` → show Institution, hide Speciality.
- `DecisionPanelScope.Speciality` → show both; Speciality combobox is keyed on the selected Institution.

### Chair user id (single GUID text → picker)

Add a single-select `<select>` listing all users with `CommitteeMember` role. Source: a new `ListUsersInRoleQuery(role, principal)` (extend `IUserAdministrationService.ListUsersInRoleAsync` to be scope-aware) OR reuse the new query that ships with **T061** if T061 lands first. Label format: `$"{FirstName} {LastName} ({Email})"`.

### Member user ids / External user ids (GUID textareas → multi-select)

Multi-select widget — either a native `<select multiple>` or a checkbox grid (the codebase doesn't appear to have a tag-input component yet; check before adding a dependency). Same data source as the chair picker.

Members must be users with `CommitteeMember` role; Externals can be users with `CommitteeMember` role from outside the institution OR a separate "external" flag. Check `DecisionPanelMemberRole.External` semantics in `Wombat.Domain/CommitteeDecisions/` before deciding.

### Pre-fill on edit

Existing edit page (`/committee/panels/{id}`) already loads the `DecisionPanel` and pre-fills the textboxes by GUID. After the picker swap, pre-select the corresponding `<option>` entries.

## Tests

- bUnit smoke test for the new pickers — `PanelEdit.razor` renders with the correct options for a seeded user set.
- Scope-guard test: InstitutionalAdmin only sees users from own institution in the picker.
- Round-trip test: create a panel via the form, reload edit page, every pre-filled value matches what was submitted.

## Browser verification

Sign in as Administrator. Visit `/committee/panels/new`. Verify:
- Institution dropdown shows all institutions (Demo + KGK).
- Speciality dropdown enables after Institution is picked, lists the institution's specialities.
- Chair picker shows all CommitteeMember users (Zulu, Naidoo, Botha, van Rensburg, plus DevUserSeeder's `committee@wombat.local`).
- Members + External multi-selects show the same options minus the chosen Chair.

Sign in as Mbatha (InstitutionalAdmin) once **T063** has widened the auth: Institution dropdown shows only KGK; Speciality only Paediatrics; user pickers show only KGK CommitteeMember users.

## Out of scope

- Drag-to-reorder members (cosmetic; the `Members` list is unordered semantically).
- Bulk-add by CSV / email paste — power-user feature; defer.
- Cross-institution external-examiner provisioning — a future T0XX should make the External user role come from anywhere; for now externals must already be Wombat users.

## Definition of done

- Build clean, all suites pass.
- Round-trip test passes.
- Browser-verified as both Administrator and (after T063) InstitutionalAdmin.
- Scenario doc Act 2 finding A2-10 marked closed.

## Files touched

- `src/Wombat.Web/Components/Pages/CommitteeDecisions/PanelEdit.razor`
- Possibly `src/Wombat.Application/Features/CommitteeDecisions/*` — extend the supporting queries for the new pickers.
- `src/Wombat.Application/Common/Interfaces/IUserAdministrationService.cs` if a scope-aware `ListUsersInRoleAsync` is needed.
- `tests/Wombat.Web.Tests/*` and `tests/Wombat.Application.Tests/*` — new tests.
- `Rewrite/Tasks/T062-decision-panel-form-pickers.md` (this file).
- `Rewrite/scenario-paediatrics.md` — Act 2 Step 2.8 Actual/Gap update.
- `Rewrite/current_state.md`.

## Estimate

~2–3 hours. **Sonnet.** Cleaner if T061 has landed first (reuses its `ListUsersByRole` query).
