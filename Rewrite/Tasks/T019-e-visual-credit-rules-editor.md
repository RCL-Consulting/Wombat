# T019-e — Visual credit-rules editor

**Phase:** Phase-2 follow-up
**Depends on:** T018, T019
**Blocks:** nothing

## Goal

Replace the JSON editor in the Credit tab with a guided form that produces the same `CreditRulesJson` the JSON editor writes by hand. The engine (T018) is unchanged. Admins who do not want to read JSON can compose credit rules by pointing at fields in the form schema and setting thresholds.

## What to do

1. **Directive list**: the main view is a list of credit directives, each rendered as a card. Add / remove / reorder (up/down) controls.
2. **New directive wizard** — three short screens, each backed by a dropdown sourced from the current form schema and curriculum structure:
   - **What curriculum item does this activity count toward?** Options: "The CurriculumItem matching an EPA chosen in field X" (pick an `epa` field) / "A specific CurriculumItem" (pick one from the speciality's curriculum) / "Any CurriculumItem whose tags match tags field X".
   - **How much does it count?** A number with sensible default 1. Option: "once per activity" / "once per value of field X" (for repeatable sections when T019-c ships) / "proportional to field X" (with a scale factor).
   - **Under what minimum level?** Pick a `likert` or `number` field; set a minimum value. Optional — if not set, the activity counts regardless of rating.
3. **Directive summary**: each saved directive renders as a plain-English sentence on the card: *"Counts 1 toward the CurriculumItem matching the EPA in `epa_id`, if `reasoning` is at level 3 or above."* Click to edit, click × to delete.
4. **Live validation**: every directive is run through the T018 validator on save. Errors surface inline on the card.
5. **Test against sample activity**: same button the v1 Credit tab has, unchanged. Applies the current directive set to a synthetic activity and shows which curriculum items would be credited.
6. **JSON view toggle**: "View JSON" modal for power users. Read-only by default; an "Edit as JSON" button opens the v1 editor in a side panel for advanced cases the wizard can't express.
7. **Round-trip**: any directive set editable in the visual tool round-trips through JSON unchanged. Directives created by hand in JSON that the wizard can render are opened in the wizard on next load; directives that use constructs the wizard doesn't understand are shown as a "JSON-only" card with a note.
8. **Schema coupling**: when a field the directive depends on is removed from the form schema, surface a broken-reference warning and highlight the affected directive. This reuses the T019 schema-change diff logic.

## Files created

- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/CreditTabVisual.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/CreditDirectiveCard.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/NewCreditDirectiveWizard.razor`
- `src/Wombat.Application/Activities/Credit/CreditDirectiveSummarizer.cs` (converts a directive to plain-English)
- `tests/Wombat.Application.Tests/Activities/Credit/CreditDirectiveSummarizerTests.cs`
- `tests/Wombat.Web.Tests/Activities/CreditTabVisualTests.cs`

## Verification

- [ ] Build a Mini-CEX with the visual wizard so it credits the EPA selected in the `epa_id` field, minimum level 3 on `reasoning`. Publish. Create an activity, fill it in, submit. Confirm credit applies as expected.
- [ ] Open the same schema in the JSON view; confirm the generated JSON matches a hand-written reference.
- [ ] Open a hand-written JSON credit-rule set and confirm directives the wizard understands render as cards. Use a construct the wizard does not understand (e.g. a custom match expression); confirm it shows as "JSON-only" with a note.
- [ ] Delete a field that a directive depends on; confirm the directive card shows the broken-reference warning and publishing is blocked until fixed.
- [ ] Round-trip: a visual-edited directive, saved, reloaded, shown in the wizard again with identical values.

## Notes & gotchas

- The plain-English summarizer is the feature that sells this task. Credit rules are the hardest part of the builder to reason about — seeing the rule stated in English reduces the chance an admin ships the wrong one by an order of magnitude. Spend the time to get the sentences right.
- Do not allow the wizard to create credit rules that would be impossible in the engine (e.g. `minimum_level_field` pointing at a `text` field). Validate as you go.
- The wizard is a view over the same JSON. Never introduce a divergent internal representation. Read the JSON in, mutate via wizard actions, write the JSON out — the wizard has no state of its own.
- "Proportional to field X" in step 2 is a stretch feature. If it doesn't fit cleanly in the engine, cut it.
