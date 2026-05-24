# T053 — Activity-type Metadata: context-aware Scope Id picker

The activity-type Metadata tab forced the operator to type a raw integer into a `Scope Id` spinbutton, with no indication of which integer corresponded to which speciality/institution. To find it, they had to navigate to another tab to read the entity's `{id}` from the URL bar. Identified in the 2026-05-24 Playwright audit as a UX-friction item.

## Change

`src/Wombat.Web/Components/Pages/Admin/ActivityTypes/ActivityTypeEdit.razor` — Metadata tab only.

### Markup
- Scope `<select>` was switched from `@bind="_scope"` to `value="@_scope" @onchange="OnScopeChanged"` so we can clear `_scopeIdText` whenever the selected scope changes (prevents an institution id from carrying over when the user flips to Speciality, etc.).
- The numeric `<input type="number">` for Scope Id was replaced with a `<select>` whose options come from a context-aware projection. The whole `FormField` block is now wrapped in `@if (_scope != ActivityScope.Global)` — when scope is Global, Scope Id is meaningless and the field hides entirely.
- The field's label and placeholder also flip per scope: "Institution" / "Speciality" / "Sub-speciality" instead of the generic "Scope Id".

### Code-behind
- Added three lookup lists (`_institutions`, `_specialities`, `_subSpecialities`) loaded via `GetInstitutionsListQuery` / `GetSpecialitiesListQuery` / `GetSubSpecialitiesListQuery` in `OnParametersSetAsync`. Same pattern as `EpaEdit` and `CurriculumEdit`.
- New computed property `ScopeIdOptions` projects each list to a `ScopeIdOption(int Id, string Label)` record per the active scope:
  - `Institution` → flat list, label = `institution.Name`
  - `Speciality` → joined to institution, label = `$"{institution.Name} / {speciality.Name}"`
  - `SubSpeciality` → joined through speciality + institution, label = `$"{institution.Name} / {speciality.Name} / {subSpeciality.Name}"` (matches the EPA edit and curriculum edit triple-path convention).
- `OnScopeChanged` parses the new scope value, sets `_scope`, and clears `_scopeIdText`. The cleared state is correctly represented by the empty-string `<option value="">Select X</option>` so validation can catch unselected required scopes.

The underlying storage variable `_scopeIdText` is unchanged so `SaveDraftAsync` keeps using `ParseNullableInt(_scopeIdText)` exactly as before. No backend / DTO / command changes needed.

## Browser verification

Logged in as Administrator. Visited `/admin/activity-types/new`, opened the Metadata tab, and clicked through every scope:

| Scope | Field visibility | Label | Options (sorted) |
|---|---|---|---|
| Global | hidden | n/a | n/a |
| Institution | shown | "Institution" | Demo Institution, Kgosi Kgari Teaching Hospital |
| Speciality | shown | "Speciality" | Demo Institution / General Medicine; Kgosi Kgari Teaching Hospital / Paediatrics |
| SubSpeciality | shown | "Sub-speciality" | Demo Institution / General Medicine / General Internal Medicine; Kgosi Kgari Teaching Hospital / Paediatrics / General Paediatrics |

Switching from one scope to another correctly clears the selection (the placeholder "Select X" becomes the active option). Re-selecting Global hides the field again.

Loaded the existing `mini_cex_paed` activity type (id=11, Scope=Speciality, ScopeId=2) and confirmed the picker pre-selects `Kgosi Kgari Teaching Hospital / Paediatrics` — round-trips cleanly.

## Out of scope

- **InstitutionalAdmin-aware filtering.** When T056 lands and InstitutionalAdmin can edit activity types, the picker should filter to the user's own institution. Today this is moot because only Administrator can edit. Add the filter in T056.
- **Cascading dropdowns** (institution → speciality → sub-speciality). The flat triple-path label is consistent with the EPA / Curriculum edit pages and is faster for users who already know the path. Cascading is a separate UX choice; not changed here.
- **Backend validation that the chosen ScopeId actually exists at the chosen Scope.** The existing `SaveActivityTypeDraftCommand` validator presumably already covers this; not investigated. Defer to T056 if any gaps surface.

## Definition of done

- Build clean.
- 38/38 Web tests pass.
- Browser-verified across all four scope values + round-trip on a persisted entity.

## Files touched

- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/ActivityTypeEdit.razor`
- `Rewrite/Tasks/T053-activity-type-scope-id-picker.md` (this file)
- `Rewrite/current_state.md`
