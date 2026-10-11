---
id: T125
title: "An administrator sets a curriculum minimum by typing a bare integer, with no idea what it means on the ladder"
status: done
priority: P2
created: 2026-09-19
started: 2026-09-24
completed: 2026-09-24
---
# T125 — An administrator sets a curriculum minimum by typing a bare integer, with no idea what it means on the ladder

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

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

## Update 2026-09-24 — EPA-stream survey

**[T136] is folded in.** It is the same page (`CurriculumItemsEdit.razor`) and the same handler
(`UpdateCurriculumItemCommandHandler`), and T136 closes when this task lands. Under rule 1 below, the page can no longer
submit a scale change whose ordinals do not fit, so what remains of T136 is its message wording and tests, listed
here.

Observed at `431e69e`: T136's premise ("the row closes, no message") is contradicted by the code. The handler refuses before mutating (`ManageCurriculumItems.cs:229-231`). The page closes the row only on success
and otherwise shows `_actionError` as an alert at the top of the page (`CurriculumItemsEdit.razor:23-26`), which is
easy to miss. Reproduce it in the browser before relying on either reading.

Line numbers above have drifted (observed at `431e69e`):

| What | Where now |
|---|---|
| Minimum input | `:75` (edit row), `:173` (add form) |
| Per-stage map | `:85`, `:188`, a single-line `InputText`, not a textarea; help text `:189` |
| Read-mode cells | `:100` (minimum), `:102` (raw JSON) |
| `_scales` loaded | `:267` |
| Add-form default | `:365-372`: `MinimumLevelOrder = 4` with no scale, which is 3b on v11.1 |

**The plan: the survey's recommendations, adopted.**

1. **A scale change resets the minima.** When the scale changes, every minimum picker (flat and per-year) resets to an
   empty, required "choose a rung", and Save is disabled until each is re-picked on the new ladder. Never clamp and
   never remap: the stored ordinal is not carried across. This replaces step 3's "say plainly what it is doing".
   *Rejected:* keeping the ordinals behind a "was Independent on O-R, is 3b on CPSA" confirm, because one click still
   changes what a number means. Also rejected: clamping, which this file and T136 both refuse.
2. **The per-stage editor** shows one row per key already in the map, plus add-year and remove-year controls. The
   curriculum has no programme length (`Curriculum.cs:5-14`) and `GetStage` is uncapped (`TraineeProfile.cs:66-76`),
   so fixed rows could drop a year-5 key. **A key the editor does not render round-trips untouched.** The editor
   serialises to the JSON the command already accepts; `NormalizeStageOverridesJson` and the validator do not change.
3. **Read-mode labels are resolved on the page** from the loaded `_scales` levels (`EntrustmentScaleDto.Levels`),
   falling back to the ordinal when the item is unpinned or the value is off the ladder. This replaces step 5's
   `CurriculumItemDto` change, so the positional DTO keeps its three construction sites and the bUnit helper.
4. **From T136: the refusal names the scale and the field.** `CurriculumMappings.EnsureScaleCanExpressMinimaAsync`
   (`CurriculumMappings.cs:89-122`) should say, for example, "Minimum level 6 (rung 5) is not a rung on O-R Scale, which
   has 5" or "Year 4 minimum …". Show it next to the edited row, in an inline sub-row like the Tools one, not only in the
   top alert. Keep the check before the first mutation. The tests that match `*requires level N*`
   (`CurriculumItemScalePinTests.cs:88,104`; `EntrustmentScaleAdminHandlerTests.cs:347`) change their text but must
   still assert the refusal.
5. [T109]'s server-side refusal stays. The pickers are guidance only.

Left to the implementer: the add form's default scale, either the siblings' shared scale or
`SubSpeciality.DefaultEntrustmentScaleId`, in place of "unpinned, 4". [T139] edits the same page (`WindowMonths` at
`:48`, `:86`, `:103`, `:192-195`); sequence the two so the page is reworked once.

**Verification, added (the last four come from T136):**

- [x] Changing the scale empties the minimum pickers and disables Save until each is re-picked. bUnit test.
- [x] A per-stage key the editor does not render survives a save. bUnit or handler test.
- [x] Browser, on dev (read DESIGN.md first): a v11.1 item offers 1/2/3a/3b/4/5. Re-pin PAED-001 to the O-R Scale
      and back.
- [x] Update refuses an incompatible scale-and-ordinals combination. Handler test on the Update path; today only Add
      is covered (`CurriculumItemScalePinTests.cs:76-117`).
- [x] Update succeeds when scale, flat minimum and stage minima change together. Handler test.
- [x] The refusal names the scale and the offending value, next to the row. bUnit test.
- [x] Full suite green, no `--no-build`.

---

## As built — 2026-09-24 (with [T136])

- **Rung pickers** (`RungPicker.razor`) for the flat and the per-year minima, labelled from the pinned scale's levels.
  They are numeric when the item is unpinned, and a stored value that is not a rung shows as "N (not a rung on X)".
- **A scale change empties every picker** and disables Save. An always-present `role="status"` region explains why,
  and Save points at it with `aria-describedby`. Minima picked on a scale come back if the admin returns to that
  scale within the same edit. Nothing is ever carried to a different scale.
- **Per-year editor** (`StageMinimaEditor.razor`, `StageMinimaDraft.cs`): one row per stored year, add the lowest
  missing year, remove a year. Entries that are not a year and a rung are listed verbatim and block Save until removed;
  of a year spelled twice, the last spelling (the one credit reads) is shown.
- **The refusal** (`CurriculumMappings.EnsureScaleCanExpressMinimaAsync`) names the scale, the field and the value, and
  the previous ladder's rung in brackets. It still runs before any mutation (the test now saves and clears the tracker,
  as `AuditPipelineBehavior` would) and is shown beside the row. Read mode prints rung labels. The add form suggests
  the siblings' shared scale, else the sub-speciality default, and says why.
- **Evidence.**
  - Tests: Application +4, Web +33 (on master: Application 919, Web 355, all five suites green). 29 mutants across
    the two rounds, all killed.
  - Browser on dev: all six checks pass.
    - PAED-001 offers exactly 1/2/3a/3b/4/5.
    - Re-pinned to O-R, the pickers emptied and Save was off with the reason. Picked and saved; the SQL showed
      `ScaleId=1` and the chosen minima.
    - Re-pinned back to v11.1 and re-picked. The row is byte-identical to the before-snapshot, and progress rows are
      unchanged.
    - Cancel saves nothing.
    - Accessibility: labels present, and `aria-describedby` points at the status.
- **Filed:** [T174] (the seeder re-pins an unpinned seeded item at every boot, the review's M1), [T175] (`app.css`
  has no version in its URL, seen in this browser run), [T176] (the edit row is cramped, and three inputs have no name).

