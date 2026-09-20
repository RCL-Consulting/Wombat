---
id: T125
title: "An administrator sets a curriculum minimum by typing a bare integer, with no idea what it means on the ladder"
status: queued
priority: P2
created: 2026-09-19
---
# T125 — An administrator sets a curriculum minimum by typing a bare integer, with no idea what it means on the ladder

**Status:** open
**Surfaced:** 2026-09-19, while fixing [T100]. Filed separately because it is not the defect [T100]
describes.
**Severity:** Medium. It is an **unguided write**, not a mislabelled read, which makes it worse than the
display defects it was filed alongside: a wrong minimum here silently changes what every trainee on the
curriculum has to reach.

## What [T100] said, and why it was wrong

[T100] listed `Admin/Curricula/CurriculumItemsEdit.razor:74` and `:113` among its "bare ordinal, no label"
**display** sites. Re-reading the file (line numbers had drifted) there are four sites, and only one of
them is a display:

| Site | Shape | What it is |
|---|---|---|
| `:86` | display | The read-mode "Minimum level" cell. A genuine [T100] bare-ordinal display |
| `:60`, `:124` | **`<input type="number">`** | The add-row and edit-row minimum. Free text |
| `:70`, `:139` | **`<textarea>`** | The per-stage minimum map, as raw JSON. The values inside are rung ordinals |

So this is not a formatting change. Threading a label to `:86` is worth doing and is cheap, but it leaves
the administrator still typing `4` into `:60` and still hand-authoring `{"1":3,"2":4}` into `:70` against a
ladder they cannot see.

## Why it matters more than a label

`CurriculumItem.MinimumLevelOrder` is the target every trainee on the curriculum is measured against, and
`CreditApplier` compares achieved ordinals to it. Since [T109] the item is also **pinned to a scale**
(`CurriculumItem.ScaleId`), which means the integer typed here is an assertion about a specific ladder —
and the two ladders in the product disagree about what a number means:

| Typed | On the O-R Scale (5 rungs) | On CPSA v11.1 (6 rungs) |
|---|---|---|
| `4` | Independent | **3b** |
| `5` | Supervises others | **4** |

An administrator who has worked with the legacy ladder and types `4` meaning "independent practice" has,
on a v11.1 curriculum, just set the target to **3b** — two rungs below what they meant, on the EPA whose
whole point is the year-4 exit standard. Nothing on the page tells them.

The adjacent cell at `:85` already prints `@(item.ScaleName ?? "Not pinned")`, so the page knows *which*
ladder. It just never says what its rungs are called.

## The fix

The page already has everything it needs. `_scales` is loaded at `:166` and populated at `:195` via
`GetEntrustmentScalesListQuery`, and a scale `<select>` is already rendered at `:62-69` and `:128-134`.

1. **`:60` and `:124` become rung `<select>`s** whose options are the selected scale's levels — value the
   `Order`, label the rung, exactly as `ActivityReferenceDataService.GetEntrustmentScaleLevelOptionsAsync`
   now does for the WBA picker ([T100] tier 1).
2. **When the item is unpinned, keep the number input.** There is no ladder to offer rungs from, and
   [T109] documents unpinned as a permanent and meaningful state, not a migration artefact. This is the
   same permissive fallback [T108] and [T123] d3 both insist on.
3. **Changing the scale must re-render the rung options**, and should say plainly what it is doing to the
   stored ordinals — it does not remap them, so an item re-pinned from the O-R Scale to CPSA keeps
   `MinimumLevelOrder = 4` and silently changes its meaning from "Independent" to "3b". That is D24's
   ordinal-remap hazard, on a page an admin can reach today.
4. **The stage map at `:70`/`:139` needs a shape, not a textarea.** One row per training year with a rung
   picker is the obvious answer; the helper text at `:140` currently explains the JSON shape, which is the
   tell that the control is wrong.
5. `:86` gets the rung label — that part is plain [T100], and
   `Wombat.Application/Features/Epas/EntrustmentRungLabels.cs` is the resolver to use. It needs
   `CurriculumItemDto` to carry a label beside the `ScaleId`/`ScaleName` it already has
   (`CurriculumDto.cs:18-30`), which is built in **three** places that must move together:
   `CurriculumMappings.cs:38`, `GetCurricula.cs:75` and `GetCurricula.cs:111`.

## Verification

- Setting a minimum on a curriculum-3 item offers `1, 2, 3a, 3b, 4, 5` and stores the ordinal.
- Setting a minimum on an unpinned item still accepts a typed integer.
- Re-pinning an item to a different ladder does not silently change what its stored minimum means without
  saying so.
- The read-mode cell shows the rung, not the ordinal.

## Related

Split out of [T100], whose tiers 1–3 shipped 2026-09-19. Depends on nothing. The re-pin hazard in step 3
is the same one D24 weighs for [T104]. Blocks nothing, but it is the last surface where a rung ordinal is
still written by hand.
