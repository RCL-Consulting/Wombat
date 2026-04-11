# T019-c — Nested and repeatable sections

**Phase:** Phase-2 follow-up
**Depends on:** T017, T018, T019
**Blocks:** nothing

## Goal

Extend the schema DSL and the builder and renderer to support one additional level of section nesting and, more importantly, **repeatable sections** — groups of fields that recur N times within a single activity, like PDSA cycles in a QI project, off-service rotations in a logbook, co-authors on a research publication, or procedure attempts in a DOPS with multiple takes.

## What to do

### Schema changes

1. A `Section` gains an optional `repeatable` block:
   ```
   {
     "key": "pdsa",
     "title": "PDSA cycle",
     "repeatable": { "min": 1, "max": 5, "label_template": "Cycle {index}" },
     "fields": [ ... ]
   }
   ```
2. A `Section` may contain `sections: [...]` in addition to `fields`. At most one level of nesting — a sub-section may not itself contain sub-sections. Enforce in the schema validator.
3. Activity data for a repeatable section is stored as a JSON array under the section's key. Each element is an object of field values. Non-repeatable sections continue to store field values flat in the top-level data object.
4. Field keys inside a repeatable section are scoped to that section. A `value` field inside a `pdsa` repeatable lives at `data.pdsa[i].value`.
5. `show_if` inside a repeatable section may reference siblings in the same instance (`data.pdsa[i].value` via the shorthand `value`) or top-level fields. References to other repeatable instances are **not** supported.

### Credit rules

6. Credit rules gain a `for_each` directive that iterates a repeatable section:
   ```
   {
     "counts_for": [
       { "for_each": "pdsa", "match": { "curriculum_item_match": {...} }, "amount": 1 }
     ]
   }
   ```
7. Each iteration contributes `amount` to the matched curriculum item. Must be tested against MIN/MAX progress rules from T018.

### Builder

8. The Form tab grows a "Make repeatable" toggle on section cards. When on, reveals min/max/label template inputs.
9. Sub-sections appear as nested cards inside their parent with a visually clear indent. A parent section with sub-sections cannot also be repeatable — pick one.
10. Field reorder and drag-drop (T019-b) respect the nesting: a field cannot be dragged out of a repeatable group into a flat group and vice versa without acknowledgment, because the data shape differs.

### Renderer

11. `ActivityForm.razor` detects repeatable sections and renders them as a vertical stack of instances with "Add another" (respecting max) and "Remove" (respecting min) controls.
12. Each instance is rendered with the section's fields using the existing field renderers.
13. Validation errors inside a repeatable section are scoped to the offending instance and surfaced inline.

### Migration story

14. Schemas created under T019 have no repeatable sections. Existing `ActivityType` rows remain compatible — the new `repeatable` key is optional and defaulted-absent. No migration of existing activity data is required.
15. The T023 PDF exporter gains a repeatable-section renderer: a numbered list of instance blocks using the section's `label_template`.

## Files touched

- `src/Wombat.Domain/Activities/Schema/FormSection.cs` (adds `Repeatable`, `Sections`)
- `src/Wombat.Application/Activities/Schema/SchemaValidator.cs`
- `src/Wombat.Application/Activities/Credit/CreditApplier.cs` (for_each)
- `src/Wombat.Web/Components/Shared/Activities/ActivityForm.razor`
- `src/Wombat.Web/Components/Shared/Activities/RepeatableSectionRenderer.razor` (new)
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/FormTab.razor`
- `src/Wombat.Infrastructure/Portfolio/PortfolioPdfService.cs` (T023 renderer extension)
- `tests/Wombat.Application.Tests/Activities/Schema/RepeatableSectionTests.cs`
- `tests/Wombat.Application.Tests/Activities/Credit/ForEachCreditTests.cs`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` green.
- [ ] Build a QI activity with three PDSA cycles in the builder. Publish. Create an activity, add two instances, submit. Confirm data shape, credit rules that use `for_each` apply correctly, PDF renders both instances.
- [ ] Validation rejects three levels of nesting.
- [ ] Validation rejects a repeatable section that also has sub-sections.
- [ ] Schema change warning: toggling a section to repeatable after publish is flagged incompatible (stored data shape changes).

## Notes & gotchas

- Repeatable sections double the complexity of every schema consumer. Keep them as simple as possible: no conditional repeatability ("repeat N times where N comes from another field"), no nested repeatables, no drag-drop reorder of instances (just add/remove from the end).
- Credit rules with `for_each` must be idempotent over re-submissions — the credit applier is called on state transitions, and an activity with five instances must not credit 5× on every transition.
- The T020 QI seed should be updated to use repeatable PDSA sections once this task ships, replacing the three pre-numbered sub-section workaround.
- Backwards compatibility: the v1 `show_if` single-condition DSL still parses. Do not take this task as an excuse to expand `show_if` — that is T019-f.
