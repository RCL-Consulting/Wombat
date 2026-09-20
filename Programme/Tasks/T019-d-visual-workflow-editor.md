# T019-d — Visual workflow editor

**Phase:** Phase-2 follow-up
**Depends on:** T018, T019
**Blocks:** nothing

## Goal

Replace the JSON editor in the Workflow tab with a drag-and-connect state machine designer. Admins who are not comfortable with JSON can compose workflows visually; the underlying JSON representation and the T018 engine are unchanged, so existing workflows keep working and can be viewed interchangeably in the visual editor or as JSON.

## What to do

1. **Canvas**: a scrollable/zoomable SVG or HTML-absolute-positioned surface with state nodes and transition arrows. Prefer HTML with `position: absolute` over SVG for easier interop with Blazor event handling; use SVG only for the arrows themselves.
2. **State node**: a rounded rectangle with the state label, a small "terminal" badge if `terminal: true`, and an edit affordance. Draggable to reposition. Position is persisted in a per-workflow layout object on the `ActivityType` row (new nullable `WorkflowLayoutJson` column) — purely cosmetic, not part of the workflow semantics.
3. **Create state**: a "+ Add state" button creates a new node at the canvas centre. Properties panel (right side) edits label, key, terminal flag.
4. **Create transition**: drag from a state's output handle to another state's input handle. The handle is a small circle on the edge of the node that appears on hover.
5. **Transition properties panel**: clicking an arrow opens a properties panel with key, actor rule (same UI as T019's Form tab field editor — dropdowns for role / subject / role+scope), `requires_note`, `required_fields` (multi-select of field keys from the form schema).
6. **Delete**: selecting a node or arrow and pressing Delete (or a button) removes it. Deleting a state with incoming transitions prompts confirmation.
7. **Auto-layout fallback**: a "Tidy layout" button arranges states along rows using a simple topological-sort-then-wrap layout. Not as good as a real graph library, much smaller than one. Good enough for 5–15 state workflows.
8. **JSON view toggle**: a "View JSON" button opens a read-only modal showing the equivalent JSON. Copy-paste is enabled for power users who want to hand-edit (the JSON tab from T019 remains accessible as an "Advanced" view behind a toggle).
9. **Live validation**: the T018 workflow validator runs on every edit. Errors surface as red badges on the offending states/transitions and in a validation summary bar at the bottom of the canvas.
10. **Undo/redo**: a simple 50-step undo stack at the component level. Not per-keystroke — one entry per discrete action (create state, delete arrow, edit property).
11. **Migration**: existing workflows edited in v1 JSON render correctly in the visual editor. Workflows without a saved layout get the auto-layout on first open and the resulting layout is saved on the next edit.

## Files created

- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/WorkflowVisualEditor.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/WorkflowVisualEditor.razor.cs`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/StateNode.razor`
- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/TransitionArrow.razor`
- `src/Wombat.Web/wwwroot/js/workflow-canvas.js` (pointer events and connection drag)
- `src/Wombat.Infrastructure/Persistence/Migrations/*WorkflowLayout.cs`
- `tests/Wombat.Web.Tests/Activities/WorkflowVisualEditorTests.cs`

## Verification

- [ ] Build a workflow from scratch visually: 5 states, 6 transitions, two actor rules. Publish. Run an activity through it end-to-end.
- [ ] Open an existing workflow built in v1 JSON. Confirm it renders. Edit a transition's actor rule visually. Confirm the JSON round-trips unchanged except for the edited field.
- [ ] Delete a state with incoming transitions; confirm the prompt, confirm the arrows are removed with the state.
- [ ] Validation: create an unreachable state, confirm the red badge. Fix it, confirm the badge clears.
- [ ] Layout persistence: move states around, save draft, reload, confirm positions preserved.
- [ ] "View JSON" shows the same JSON the textarea editor would.

## Notes & gotchas

- Resist adding a graph library (dagre, cytoscape, jsplumb). They pull in 50–200KB each and mostly exist to solve harder problems than a 10-state workflow needs. The hand-rolled canvas is 300 lines of JS.
- `WorkflowLayoutJson` is cosmetic only. Workflows are still stored and validated through the existing `WorkflowJson` column. Deleting the layout has no semantic effect.
- When the form schema changes, transitions that reference removed fields must be flagged. The existing v1 validator already does this — reuse it.
- Pointer events, not mouse events. Touch-capable institutions exist.
- Zoom/pan is nice but not essential. If a workflow has more than 20 states it probably shouldn't; that's a smell. The editor does not need to support arbitrarily large canvases in v1.
