# T067 — Activity-type builder crashes on Add field / section actions (loop-variable capture)

**Status:** in_progress (found + fixing during Act 3 play-through, 2026-05-29)
**Severity:** High — blocks building any multi-field activity-type schema through the visual builder.
**Surface:** `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/ActivityTypeEdit.razor`

## Symptom

On `/admin/activity-types/{id}` → Form tab, clicking **Add field** (or any section-level
action button: Edit / Up / Down / Delete on a section) throws:

```
System.ArgumentOutOfRangeException: Index was out of range. Must be non-negative and
less than the size of the collection. (Parameter 'index')
   at System.Collections.Generic.List`1.get_Item(Int32 index)
   at ...ActivityTypeEdit.AddField(Int32 sectionIndex) ActivityTypeEdit.razor:line 511
```

The Blazor circuit terminates ("An unhandled error has occurred." banner with Retry/Resume);
all subsequent edits silently no-op against the dead circuit.

Reproduced deterministically on a freshly-loaded type with **zero prior edits** (id 13),
first click. Independent of any field state.

## Root cause

The section list renders with an indexed `for` loop:

```razor
@for (var sectionIndex = 0; sectionIndex < _builderSchema.Sections.Count; sectionIndex++)
{
    var formSection = _builderSchema.Sections[sectionIndex];
    ...
    <button @onclick="() => AddField(sectionIndex)">Add field</button>
}
```

C# `for` declares a **single** `sectionIndex` shared by every lambda. The `@onclick`
delegates capture it by reference; they execute *after* the loop, when
`sectionIndex == Sections.Count` (1 for a single-section type) → `Sections[1]` is out of range.

The field-level loop uses `.Select((value, index) => …)` inside a `foreach`, so `field.index`
is captured per-iteration and is **not** affected — only the outer section index is the bug.

## Fix

Capture the section index into a per-iteration local inside the loop body and use it in all
deferred `@onclick` lambdas (`SelectSection`, `MoveSection`, `DeleteSection`, `SelectField`,
`MoveField`, `DeleteField`, `AddField`). Render-time uses (`disabled`, `formSection`) are
unaffected but switched to the local for consistency.

## Verification

- Build clean.
- Browser: Add field on a fresh type adds a 2nd field with no circuit crash; section
  Up/Down/Delete operate on the correct section.
- Then: full Mini-CEX schema (3 sections / 12 fields) builds end-to-end → Act 3 unblocked.
