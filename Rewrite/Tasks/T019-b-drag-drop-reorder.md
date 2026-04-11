# T019-b — Drag-and-drop reordering in the builder

**Phase:** Phase-2 follow-up (post-launch iteration)
**Depends on:** T019
**Blocks:** nothing

## Goal

Replace the up/down button reorder in the Form tab with drag-and-drop for sections and for fields within and across sections. Keep up/down buttons as a keyboard-accessible fallback. The underlying schema JSON does not change — this is a UI-only enhancement.

## What to do

1. Add SortableJS via a static `.js` file in `wwwroot/lib/sortable/` (MIT, ~8KB minified, no npm chain). Do **not** pull in a Blazor wrapper package; the raw interop is 20 lines and avoids a version-lock problem.
2. Write `wwwroot/js/builder-sortable.js` that exposes `window.wombatBuilder.initSortable(containerId, dotNetRef, groupName)` and calls back into `.NET` with `(fromIndex, toIndex, fromGroup, toGroup)` on drop.
3. In `FormTab.razor`, initialize Sortable on the section list and on each section's field list. Sections belong to group `sections`; all field lists share the group `fields` so fields can be dragged between sections.
4. On drop, call a `MoveSection` or `MoveField` method on the Razor component. The component mutates the staging schema and re-renders. Blazor's reactive render takes care of the DOM update.
5. Up/down buttons remain in the UI, unchanged. They are the keyboard path.
6. Touch-device support: SortableJS handles touch out of the box. Verify on iPad Safari and on a touchscreen Windows laptop; these are the two failure modes institutions will hit.
7. Drag ghost styling: use SortableJS's `ghostClass`, `chosenClass`, `dragClass` options with CSS in `Wombat.Web.styles.css` matching the site's drag affordance.
8. After drop, the edited staging schema is dirty — surface the "unsaved changes" indicator that T019 already shows.

## Files created

- `src/Wombat.Web/wwwroot/lib/sortable/Sortable.min.js` (vendor file, pinned version, committed)
- `src/Wombat.Web/wwwroot/js/builder-sortable.js`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/FormTab.razor.cs` (partial class, drag callbacks)
- `tests/Wombat.Web.Tests/Activities/BuilderDragDropTests.cs` (component-level — simulate the JS callback method)

## Verification

- [ ] `dotnet build` clean.
- [ ] Drag a field up and down within a section; confirm the schema updates and preview reflects the new order.
- [ ] Drag a field between sections; confirm both sections update and the field's `key` is preserved.
- [ ] Drag a section up or down; confirm the order persists through Save draft.
- [ ] Up/down buttons still work and are still keyboard-reachable (tab order, arrow key activation).
- [ ] Touch test on iPad or Android tablet (or the browser devtools touch emulator as a minimum).
- [ ] Screen-reader test: drag-and-drop is announced via an `aria-live` region when a move completes.

## Notes & gotchas

- Do not let SortableJS own state. It reports drop positions; the component owns the schema. After every drop, the component computes the new order and re-renders. SortableJS's DOM mutation is cosmetic and gets overwritten by the Blazor re-render — that's fine.
- Cross-section drag is the feature most likely to produce a confusing UX. Require a visible hover-highlight on the target section so users understand where the field will land.
- If a field's `show_if` references a field that is now *after* it in evaluation order (due to a drag), the validator should warn on save. `show_if` evaluation walks the schema top-to-bottom and a forward reference is ambiguous.
