# T019-f — Multi-condition visibility

**Phase:** Phase-2 follow-up
**Depends on:** T017, T019
**Blocks:** nothing

## Goal

Extend the `show_if` DSL from the single-condition v1 shape to support ANDs, ORs, and grouping. Backwards compatible: any single-condition rule from v1 continues to parse and evaluate identically.

## What to do

### DSL

1. A `show_if` value can be either:
   - The v1 shape: `{ "field": "...", "op": "...", "value": ... }` (still valid).
   - A group: `{ "all_of": [ ... ] }` or `{ "any_of": [ ... ] }`, where children are either v1 conditions or nested groups. One level of nesting is enough — the evaluator handles arbitrary depth, but the builder UI only produces up to two levels.
2. The schema validator rejects groups of mixed shapes, empty groups, and groups nested more than three levels deep.
3. A separate `hide_if` is **not** added. `show_if: { any_of: ... }` plus negating operators covers the cases.

### Evaluator

4. Extend `ConditionEvaluator` in T018's activity engine to recursively evaluate groups. `all_of` is AND; `any_of` is OR; v1 condition shape is a leaf.
5. Short-circuit evaluation. A hidden field's value is still omitted from submission payloads (same rule as v1).
6. Unit tests cover every leaf operator crossed with every group shape.

### Builder UI

7. The `show_if` editor in the FieldEditor grows a "+ Add another condition" button.
8. When a second condition is added, the editor becomes a group with a top-level `all_of` / `any_of` toggle. A "+ Add group" button adds a nested child group (up to two total levels visible in the UI).
9. Each condition row keeps the v1 three-dropdown shape (Field, Operator, Value).
10. The plain-English summary under the editor renders the composite rule: *"Show this field when (procedure equals 'central_line') AND (complication is set)."*
11. When a field is deleted from the schema, any `show_if` that references it is flagged — the builder shows a broken-reference warning on publish.

### Migration

12. Schemas written in v1 continue to parse and render. They are not rewritten. Editing a v1 `show_if` in the builder upgrades it to the group shape only if the admin adds a second condition.

## Files touched

- `src/Wombat.Domain/Activities/Schema/ShowIf.cs` (new union type)
- `src/Wombat.Application/Activities/Schema/ConditionEvaluator.cs`
- `src/Wombat.Application/Activities/Schema/SchemaValidator.cs`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/FieldEditors/ShowIfEditor.razor`
- `src/Wombat.Web/Components/Shared/Activities/ActivityForm.razor` (uses the new evaluator)
- `tests/Wombat.Application.Tests/Activities/Schema/ConditionEvaluatorTests.cs`

## Verification

- [ ] v1 single-condition schemas still parse and evaluate identically (regression test).
- [ ] A schema with a three-deep nested group evaluates correctly.
- [ ] Deleting a referenced field produces a broken-reference warning.
- [ ] Builder UI produces valid JSON for every combination the user can construct.
- [ ] Plain-English summary is accurate for a sample of compound rules.

## Notes & gotchas

- The UI is the hard part. Composition UIs for boolean expressions are notoriously confusing. Keep the depth limited (two visible levels), keep the operators visible and obvious, and lean on the plain-English summary to reassure the admin they built what they meant to build.
- Resist adding new operators in this task. The set stays as: `equals`, `not_equals`, `is_set`, `is_not_set`, `greater_than`, `less_than`. New operators need their own small tasks because each is a leaf evaluator + a UI affordance + tests.
- Forward references are still not allowed. A field's `show_if` may only reference fields that appear earlier in the schema evaluation order.
