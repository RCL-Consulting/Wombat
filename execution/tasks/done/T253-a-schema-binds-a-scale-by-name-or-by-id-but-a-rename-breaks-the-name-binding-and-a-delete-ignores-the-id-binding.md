---
id: T253
title: A schema binds a scale by name or by id, but a rename breaks the name binding and a delete ignores the id binding
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] Renaming the v11.1 scale leaves every CPSA field bound. Test.
- [x] Deleting a scale a published schema binds by id is refused, naming the type. Handler test (the audit trap).

## Related

T229, T232, T221, T109, T174.

---

## As built — 2026-09-25 (`6dc4d54`, D25 superseded)

One scale-binding rule: a field's `scale_key` is an id, a seed key (`seed:cpsa:scale:v11.1`, `seed:demo:scale:o-r`) or a
name, resolved by one resolver. Seeds bind by seed key.
- **The migration.** The data-only migration `T253_ScaleKeysBindByIdOrSeedKey` rewrote every stored name binding to a
  seed key, or to the id where the scale has no key. It covered published versions, current copies and drafts.
- **Delete.** A scale that any published schema binds by id, seed key or name cannot be deleted; the refusal names the
  type.
- **Publish.** A draft whose field binds no scale is not published.
- **Rename.** A rename warns if a published form still binds by the old name, and a name of digits or starting `seed:`
  is refused.
- **A refused scale save commits nothing** (the audit trap).

Handler, Postgres (the migration's SQL, and a round-trip against a fresh boot) and seed tests.

Browser on dev (scripted Chrome, master `36b0661`; `pg_dump -n public` first, at `recovery/pre-t253-migration.dump`):
- **Bindings, before → after:**
  - published versions: "CPSA … v11.1" ×19 → `seed:cpsa:scale:v11.1` ×19, and "O-R Scale" ×58 → `seed:demo:scale:o-r` ×58;
  - current copies: 8 → 8 and 23 → 23.
  - No name binding remains.
- **The seed refresher** republished nothing: "0 republished, 22 unchanged".
- **A new Mini-CEX (Paediatrics) on PAED-001** (activity 49): the assessor's picker offered 1, 2, 3a, 3b, 4 and 5.
  Completed at 3a, it was credited (progress 10/10).
- **Older completed Mini-CEX** 16 and 7 still read "3a" and "3b".
- **Not run:** the rename, delete and publish refusals (Administrator-only). They are covered by handler tests.

**Filed from the review:** [T271] (the delete and publish race; the builder's "Select…") and [T264] (the scales list's
Delete).
