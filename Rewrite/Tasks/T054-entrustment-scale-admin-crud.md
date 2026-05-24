# T054 — Admin CRUD for `EntrustmentScale` + `EntrustmentLevel`

Closes the "only true feature gap" surfaced by the Act 1 audit / play-through (Step 1.7). Before this task, the `EntrustmentScale` entity existed in the domain + had read-only access from `FormEdit` and `ReviewDetail` via `GetEntrustmentScalesListQuery`, but the only writer in the whole codebase was `DataSeeder`. Phase 1.C of the Paediatrics scenario had to fall back to "reuse the seeded global scale" because there was no admin surface.

## Shipped

### Application layer — MediatR commands + queries

Three new commands in `src/Wombat.Application/Features/Epas/Commands/`:

- **`CreateEntrustmentScale/`** — `CreateEntrustmentScaleCommand`, validator, handler. Accepts name, description, and a list of `EntrustmentLevelInput(Order, Label, Description?)`. Validator enforces ≥2 levels, contiguous `Order` from 1, unique labels per scale, label ≤200 chars, description ≤2000 chars. Handler rejects duplicate scale names (case-sensitive, trimmed).

- **`UpdateEntrustmentScale/`** — `UpdateEntrustmentScaleCommand` carrying `IReadOnlyList<EntrustmentLevelUpdate(Id?, Order, Label, Description?)>`. Handler diffs against current state: levels with an `Id` get updated in place, levels with `Id == null` get inserted, and levels not present in the input get checked for referential integrity (`PendingEntrustmentDecision` + `EntrustmentDecision` references) before deletion. If any referenced level would be removed, the whole update throws — admins must revoke referencing decisions first.

- **`DeleteEntrustmentScale/`** — single-id command, marked `[NoValidator]`. Handler walks four reference tables (`AssessmentForm.ScaleId`, `MsfQuestion.ScaleId`, `PendingEntrustmentDecision.AuthorisedLevelId`, `EntrustmentDecision.AuthorisedLevelId`) and throws with a specific message identifying the blocker. When nothing references the scale, all child levels are removed first, then the scale itself.

One new query: `GetEntrustmentScaleByIdQuery` appended to `Features/Epas/GetEpas.cs` alongside the existing `GetEntrustmentScalesListQuery` — same projection shape, returns `EntrustmentScaleDto?`.

### Web layer — two Razor pages

- **`Components/Pages/Admin/EntrustmentScales/EntrustmentScalesList.razor`** — `/admin/entrustment-scales`. Standard admin list following the institutions pattern: `StatePanel` + `DataTable` over `EntrustmentScaleDto`. Columns: Name / Description / Levels (count) / Edit + Delete buttons. Delete dispatches `DeleteEntrustmentScaleCommand` and surfaces the handler's `InvalidOperationException.Message` inline so the user sees exactly which downstream entity is blocking deletion.

- **`Components/Pages/Admin/EntrustmentScales/EntrustmentScaleEdit.razor`** — `/admin/entrustment-scales/new` and `/admin/entrustment-scales/{Id:int}`. Two-card layout: "Scale details" (name + description) above "Levels" (table of rows with per-row Up/Down/Remove buttons, an Add-level button that auto-numbers the next `Order`). Single Save button submits the whole thing through `CreateEntrustmentScaleCommand` or `UpdateEntrustmentScaleCommand`. After Create, navigates to `/admin/entrustment-scales/{id}` so refresh-to-blank-form can't lose context (matches the T055 pattern from `ActivityTypeEdit`). `Order` is derived from row position via the internal `Reorder()` helper after any move/remove — the user never edits the Order column directly.

### Nav menu

`Components/Layout/NavMenu.razor` gets one new entry between Activity Types and Scheduled Jobs, using the `award` icon.

### Tests

`tests/Wombat.Application.Tests/Features/Epas/EntrustmentScaleAdminHandlerTests.cs` — 5 happy-path + guard-path tests:
- `Create_PersistsScaleWithOrderedLevels`
- `Create_TrimsNameAndRejectsDuplicate`
- `Update_AddsRenamesAndRemovesLevels`
- `Delete_RemovesScaleWhenUnused`
- `Delete_RejectsScaleReferencedByAssessmentForm`

Brings Application test count from 169 → 174.

## Out of scope (documented compromises)

- **No `IsActive` soft-delete on `EntrustmentScale`.** The original triage suggested soft-delete to honour "existing forms/reviews reference scales by FK", but that would require a schema change + hand-written migration + Designer file + ModelSnapshot update. The simpler referential-integrity-check-on-delete handles the same risk: scales referenced by `AssessmentForm`, `MsfQuestion`, or via any `EntrustmentLevel` by `PendingEntrustmentDecision` / `EntrustmentDecision` cannot be deleted. If product later wants the "archive but keep visible to historical references" semantics, an `IsActive` column can land as a follow-up.
- **No InstitutionalAdmin scoping.** Both pages are gated on `[Authorize(Policy = "Administrator")]` — same as every other admin surface. When T056 lands and InstitutionalAdmin gets institution-scoped powers, this page will need an `InstitutionId` filter and either a per-institution scope on `EntrustmentScale` (schema change) or a scoping convention (e.g. naming prefix). Out of scope here; noted in T056's brief.
- **No cascading transitions for `Order` collisions during edit.** The validator catches a non-contiguous order at submit time; the UI's `Reorder()` keeps orders contiguous as the user clicks Up/Down/Remove, so this is enforced both client- and server-side.
- **No DataSeeder change.** The seeded "O-R Scale" remains. The Paediatrics scenario can now build its own (Step 1.7), but doesn't have to.

## Browser verification

`http://localhost:5080/admin/entrustment-scales` as Administrator:

| Action | Result |
|---|---|
| Visit list | Seeded "O-R Scale" rendered with 5 levels + Edit / Delete buttons; nav has new "Entrustment Scales" entry between Activity Types and Scheduled Jobs |
| Click `Create scale`, fill name + description, click `Add level` 3 times (to get 5 rows), fill all labels + descriptions, click `Save` | New scale persisted at `/admin/entrustment-scales/2`; URL flips on first save; all 5 levels round-trip with correct Order |
| Rename level 4 from "Unsupervised" to "Independent", click `Save` | Status banner "Entrustment scale saved."; label updates in place |
| Click `Delete` on the new scale (no Forms reference it) | Status banner "Entrustment scale deleted."; list collapses to just the O-R Scale |
| Nav-menu entry rendering | Reads "Entrustment Scales", uses `award` icon, sits in the order documented |

## Build and test status at completion

- `dotnet build Wombat.sln -c Release` — 0 warnings, 0 errors.
- Application: 174/174 pass (was 169; +5 from this task).
- Architecture: 19/19 pass.
- Web: 38/38 pass.
- Infrastructure: not run locally (known parallel-flaky).

## Files touched

**Added:**
- `src/Wombat.Application/Features/Epas/Commands/CreateEntrustmentScale/CreateEntrustmentScaleCommand.cs`
- `src/Wombat.Application/Features/Epas/Commands/CreateEntrustmentScale/CreateEntrustmentScaleCommandValidator.cs`
- `src/Wombat.Application/Features/Epas/Commands/CreateEntrustmentScale/CreateEntrustmentScaleCommandHandler.cs`
- `src/Wombat.Application/Features/Epas/Commands/UpdateEntrustmentScale/UpdateEntrustmentScaleCommand.cs`
- `src/Wombat.Application/Features/Epas/Commands/UpdateEntrustmentScale/UpdateEntrustmentScaleCommandValidator.cs`
- `src/Wombat.Application/Features/Epas/Commands/UpdateEntrustmentScale/UpdateEntrustmentScaleCommandHandler.cs`
- `src/Wombat.Application/Features/Epas/Commands/DeleteEntrustmentScale/DeleteEntrustmentScaleCommand.cs`
- `src/Wombat.Application/Features/Epas/Commands/DeleteEntrustmentScale/DeleteEntrustmentScaleCommandHandler.cs`
- `src/Wombat.Web/Components/Pages/Admin/EntrustmentScales/EntrustmentScalesList.razor`
- `src/Wombat.Web/Components/Pages/Admin/EntrustmentScales/EntrustmentScaleEdit.razor`
- `tests/Wombat.Application.Tests/Features/Epas/EntrustmentScaleAdminHandlerTests.cs`
- `Rewrite/Tasks/T054-entrustment-scale-admin-crud.md` (this file)

**Modified:**
- `src/Wombat.Application/Features/Epas/GetEpas.cs` (added `GetEntrustmentScaleByIdQuery` + handler)
- `src/Wombat.Web/Components/Layout/NavMenu.razor` (nav entry)
- `Rewrite/current_state.md`

## Follow-ups (after T054)

- **Restore Step 1.7's original prescription in the scenario.** `Rewrite/scenario-paediatrics.md` Step 1.7 currently says "reuse seeded scale; capability gap tracked as T054". Now that T054 has shipped, swap the workaround for the canonical create-scale steps and clear the "Open" gap line.
- **T056 InstitutionalAdmin scoping** — when it lands, give this page the same `InstitutionalAdmin`-aware treatment as the others.
