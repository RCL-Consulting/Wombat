# T004 — EPAs, Curricula, and Assessment Forms

**Phase:** 1 — Domain & data
**Depends on:** T003
**Blocks:** T007, T009, T011

## Goal

Model EPAs, Curricula (correctly — see DOMAIN.md), CurriculumItems, and Assessment Forms. Ship CRUD handlers and admin pages for all of them. This is the heart of the competency framework; take the time to get it right.

## What to do

1. In `Wombat.Domain/Epas/`:
   - `Epa` — Id, SubSpecialityId, Code, Title, Description, RequiredKnowledgeSkills (text), IsActive, CreatedOn.
   - `EntrustmentScale` — Id, Name, Description. A scale is a reusable definition (e.g. "O-R Scale" with levels 1–5).
   - `EntrustmentLevel` — Id, ScaleId, Order (1..N), Label, Description. Child of `EntrustmentScale`.
2. In `Wombat.Domain/Curricula/`:
   - `Curriculum` — Id, SubSpecialityId, Name, Version, EffectiveFrom (`DateOnly`), EffectiveTo (`DateOnly?`), IsActive. Aggregate root.
   - `CurriculumItem` — Id, CurriculumId, EpaId, RequiredCount, MinimumLevelOrder, WindowMonths, Weight (nullable double). Child of Curriculum. This is what Wombat previously called `EPACurriculum`. Rename with intent.
3. In `Wombat.Domain/Forms/`:
   - `AssessmentForm` — Id, Name, InstitutionId?, SpecialityId?, SubSpecialityId?, ScaleId (which EntrustmentScale it uses), CanDelete, IsActive.
   - `FormCriterion` — Id, FormId, Order, Prompt, HelpText, IsRequired. Replaces `OptionCriterion`.
   - `FormEpaLink` — Id, FormId, EpaId. Replaces `EPAForm`. This says "this form can be used to assess this EPA".
4. EF configurations for all of the above. Keys, indexes, cascade rules. Use `DateOnly` for effective dates.
5. CQRS handlers in `Wombat.Application/Features/Epas/`, `Wombat.Application/Features/Curricula/`, `Wombat.Application/Features/Forms/`:
   - For EPAs: Create, Update, Deactivate, ListForSubSpeciality, GetById.
   - For Curricula: Create, Update (metadata only), AddItem, RemoveItem, UpdateItem, List, GetById, **CloneAsNewVersion** (creates a v2 of a curriculum — useful because curricula change over time and you don't want to edit a live one).
   - For Forms: Create, Update, Deactivate, AddCriterion, RemoveCriterion, LinkEpa, UnlinkEpa, List, GetById.
6. Blazor pages under `Wombat.Web/Components/Pages/Admin/Epas/`, `.../Admin/Curricula/`, `.../Admin/Forms/`. List + edit for each. The curriculum editor needs a sub-page to manage items (add/remove EPAs with their requirements).
7. EF migration: `EpasAndCurricula`. Include `Designer.cs`.
8. Seed: one default `EntrustmentScale` ("O-R Scale", levels 1–5 with standard labels). One sample EPA and one sample curriculum under the demo SubSpeciality.

## Files created

- `src/Wombat.Domain/Epas/{Epa,EntrustmentScale,EntrustmentLevel}.cs`
- `src/Wombat.Domain/Curricula/{Curriculum,CurriculumItem}.cs`
- `src/Wombat.Domain/Forms/{AssessmentForm,FormCriterion,FormEpaLink}.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/{Epa,EntrustmentScale,Curriculum,CurriculumItem,AssessmentForm,FormCriterion,FormEpaLink}Configuration.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*EpasAndCurricula.cs`
- `src/Wombat.Application/Features/{Epas,Curricula,Forms}/**`
- `src/Wombat.Web/Components/Pages/Admin/{Epas,Curricula,Forms}/**`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` — unit tests for `Curriculum.CloneAsNewVersion()` and for validators.
- [ ] Manual: create an EPA, create a form linked to it, create a curriculum, add the EPA as a curriculum item with RequiredCount=3 and MinimumLevelOrder=4. Reload and confirm persistence.
- [ ] Cloning a curriculum creates a new row with version incremented and all items copied.

## Notes & gotchas

- `MinimumLevelOrder` is an **order** within the scale, not a level ID. That way cloning a curriculum across scales is possible (though discouraged).
- Curriculum `Version` is a string (e.g. "2024.1"), not an int. Let the user version however they like.
- A curriculum cannot be edited in place once it has trainees attached to it. The editor must enforce this and redirect to the clone flow. This is a domain-level constraint; enforce it in the handler, not only the UI.
- Do not create an `Options` / `OptionSet` abstraction like the old Wombat. The new model uses `EntrustmentScale` + `EntrustmentLevel` directly. The old abstraction was too generic and caused more confusion than it saved.
