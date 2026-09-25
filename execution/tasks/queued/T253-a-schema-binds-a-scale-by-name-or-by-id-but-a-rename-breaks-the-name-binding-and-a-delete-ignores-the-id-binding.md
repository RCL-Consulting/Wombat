---
id: T253
title: A schema binds a scale by name or by id, but a rename breaks the name binding and a delete ignores the id binding
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T253 — A schema binds a scale by name or by id, but a rename breaks the name binding and a delete ignores the id binding

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Either way, a published type's rating fields silently lose their ladder. The rung picker empties
and credit falls back to comparing bare ordinals, the silent break T109 guards against.
**Surfaced:** 2026-09-25, the T229 review (finding 3) and the T232 review (out of scope, Medium). Both predate them.

## Symptom

- **Rename.** Every `*_cpsa` seed binds its scale fields by the scale's exact name ("CPSA Paediatric Entrustment Scale
  v11.1"), and the generic seeds bind "O-R Scale". Renaming the scale on the scale editor unbinds every such field.
  `CreditApplier.ResolveScaleIdAsync`, `ActivityReferenceDataService` and `EntrustmentRungLabels.LoadForScaleKeysAsync`
  look a key up by id, else by name.
- **Delete.** The activity-type builder writes `scale_key` as the scale's **id** (`ActivityTypeEdit.razor` loads
  `GetEntrustmentScaleOptionsAsync`, whose value is the id). `EntrustmentScaleReferences.ThrowIfNamedByAPublishedSchemaAsync`
  compares keys only to the scale's name. So deleting a scale a UI-built type binds by id succeeds.

## What to build

One binding rule, used by both directions:
- **Seeds bind by the scale's `SeedKey`** (T221 gave the v11.1 scale `cpsa:scale:v11.1`; T229 keyed the demo scale),
  for example `"scale_key": "seed:cpsa:scale:v11.1"`. The three resolvers accept it. The refresher republishes the
  seeds (W-007).
- **The delete check** refuses a scale any published schema binds by id, name or seed key, and names the type.
- **The rename check** refuses nothing once seeds bind by key, but warns if a published schema still binds by the old
  name.

## Verification

- [ ] Renaming the v11.1 scale leaves every CPSA field bound. Test.
- [ ] Deleting a scale a published schema binds by id is refused, naming the type. Handler test (the audit trap).

## Related

T229, T232, T221, T109, T174.
