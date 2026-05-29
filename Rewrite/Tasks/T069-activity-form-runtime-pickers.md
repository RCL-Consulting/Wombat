# T069 — ActivityForm runtime renderer lacks EPA / User / Scale pickers

**Status:** ✅ SHIPPED 2026-05-30 (Opus). See "Resolution" below.
**Severity:** High — schema-driven activity forms are not usable by real trainees.
**Surface:** `src/Wombat.Web/Components/Shared/Activities/ActivityForm.razor`

## Resolution (2026-05-30)

Chosen approach: **resolve Scale by key at runtime + add a Scale-picker to the builder** (EPA/User
pickers were unconditional). Changes:

1. **`IActivityReferenceDataService` extended** (+impl `ActivityReferenceDataService`) with four
   principal-aware methods: `GetEpaOptionsAsync` (active EPAs scoped to the caller's sub-speciality,
   plus sub-specialities under their speciality claims; Administrator sees all; Value=EPA id,
   Label="Code — Title"), `GetAssessorOptionsAsync` (assessor users scoped to the caller's
   institution via `IUserAdministrationService`; Administrator all; Value=user id), and
   `GetEntrustmentScaleOptionsAsync` / `GetEntrustmentScaleLevelOptionsAsync(scaleKey)` (scale list
   for the builder; level list resolved by scale id **or** name; Value=level order, Label="order.
   label").
2. **`ActivityForm.razor`** now injects `AuthenticationStateProvider`, loads options per field type
   (Epa→EPA select, User→assessor select, Scale→level select via `field.ScaleKey`, else
   `CatalogueKey`), and renders `Epa`/`User` as `<select>` (alongside `Choice`). A `Scale` field
   renders a level `<select>` when its scale resolves, else **falls back to the number input** (so
   pre-existing schemas with no scale binding keep working). Stored primitives are unchanged (EPA id
   int, user id string, level order int) → `CreditApplier` + actor-DSL binding unaffected.
3. **Builder field editor** (`ActivityTypeEdit.razor`) shows an **Entrustment scale** `<select>` for
   Scale-typed fields, bound to `selectedField.ScaleKey` (round-trips via existing `scale_key`
   serialisation). Scale list loaded via the reference-data service.

**Live-verified** (`wombat_t002_verify`): as Dlamini, `/activities/new` Mini-CEX shows an EPA
dropdown (15 scoped PAED EPAs), an Assessor dropdown (5 KGK assessors), and the unbound Scale fields
correctly fall back to number inputs. As admin, the builder shows the Entrustment-scale picker for
Scale fields; binding "Paed General Entrustment Scale" makes the live preview's overall-level field
render the 5 levels. Tests: +6 (Infrastructure 7, Web 41); Application 249, Domain 45, Architecture 19.

**Remaining debt (small follow-up):** existing published schemas need re-binding (open the type, set
each Scale field's scale, republish) to get labelled level dropdowns at runtime. Also, the
EPA-trajectory parser (T072) still reads the rating by literal field name (`overall`|`overall_level`)
rather than from the schema — a fully schema-aware field-role resolution remains open.

---

## Original report

## Symptom

On the trainee `/activities/new` form (and any `ActivityForm` render), fields declared in the
builder as rich types degrade to raw inputs:

| Field type | Rendered as | Should be |
|---|---|---|
| `Epa` | `<input type=number>` | scoped `<select>` of EPAs (code — title) |
| `User` | `<input type=text>` | scoped `<select>` of eligible users (e.g. assessors) |
| `Scale`/`Likert`/`Rating` | `<input type=number>` | the entrustment-level selector (label per level) |
| `Choice` | `<select>` ✓ | (already correct) |

A trainee filling a Mini-CEX must therefore type a raw EPA **id** (`2`) and a raw assessor **GUID**
by hand, and rating fields take a bare number with no level labels. Verified end-to-end during the
Act 3 play (the lifecycle works, but only because the operator hand-entered ids/GUIDs).

Root cause: the `@switch (field.Type)` in `ActivityForm.razor` groups
`Number/Epa/Likert/Scale/Rating` onto one number input and `Text/User/Signature/ProcedureRef` onto
one text input.

## Fix sketch

- Add `Epa` and `User` cases rendering a `<select>` from scoped lookups (extend
  `IActivityReferenceDataService` or add scoped queries). **The EPA lookup must be scoped to the
  trainee's speciality/sub-speciality** — the activity-type and assessor lists are already scoped;
  the EPA option source must match (compare T056 scope guards).
- Add a `Scale` case rendering the entrustment-scale levels (Order as value, Label as text),
  resolved from the field's `CatalogueKey` or the institution default scale.
- Keep storing the same primitive the engine reads (EPA id int, user id string, level Order int) so
  `CreditApplier` / actor-DSL binding are unaffected.

## Verification

Trainee Mini-CEX form shows an EPA dropdown (Paed EPAs only), an Assessor dropdown (KGK assessors),
and 5-level rating selectors; submitting still credits correctly (re-run Act 3 Phase 3.A–3.C from
the `act3-schema-built` snapshot).
