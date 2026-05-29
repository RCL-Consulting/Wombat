# T069 — ActivityForm runtime renderer lacks EPA / User / Scale pickers

**Status:** open (found during Act 3 play-through, 2026-05-29)
**Severity:** High — schema-driven activity forms are not usable by real trainees.
**Surface:** `src/Wombat.Web/Components/Shared/Activities/ActivityForm.razor`

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
