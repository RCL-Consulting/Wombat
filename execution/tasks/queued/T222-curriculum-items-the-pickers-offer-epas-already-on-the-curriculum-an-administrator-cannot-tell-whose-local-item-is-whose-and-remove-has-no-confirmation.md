---
id: T222
title: Curriculum items: the pickers offer EPAs already on the curriculum, an Administrator cannot tell whose local item is whose, and Remove has no confirmation
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T222 — Curriculum items: the pickers offer EPAs already on the curriculum, an Administrator cannot tell whose local item is whose, and Remove has no confirmation

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Each is a dead end or an unlabelled action. None writes anything wrong.
**Surfaced:** 2026-09-25, the T211 review (findings 2, 4 and 5). All predate T211.

## Symptom

1. **Duplicate EPAs in the pickers.** `CurriculumItemEpas.ListAsync` (`ListCurriculumItemEpaOptionsQuery`) offers, on Add
   and on Edit, EPAs that are already items on the curriculum. The Add command then refuses each one with "already
   contains". On curriculum 2 every PAED EPA is already an item, so the Add picker offers nothing that can be added.
2. **Whose item?** To an Administrator, every local item reads "The institution's own item"
   (`CurriculumItemsEdit.razor`). `CurriculumItem` has no navigation to its institution.
3. **The actions cell breaks DESIGN.md.** Remove is a red in-row button with no `ConfirmDialog`. Edit and Remove have
   no `aria-label` naming the EPA. The actions column header is an empty `<th>`.

## What to build

1. Leave out of both pickers any EPA the curriculum already holds for the same owner, apart from the edited item's own.
   Give the Add form an empty state ("Every EPA … is already on this curriculum"), and refresh the picker after each
   Add, Save and Remove. This changes T195's tested picker contract (`CurriculumItemEpaOwnershipTests` counts a
   duplicate as accepted), so update that test deliberately.
2. Name the owning institution on a local item for a caller who can see more than one institution's items.
3. A `ConfirmDialog` for Remove, `aria-label`s naming the EPA, and a `.visually-hidden` "Actions" header.

## Verification

- [ ] The Add picker on curriculum 2 offers only EPAs not yet on it, and says so when there are none. bUnit and handler
      tests; browser.
- [ ] An Administrator sees the owning institution on each local item. bUnit.
- [ ] Remove asks for confirmation. Edit and Remove are named. bUnit.

## Related

T211, T195, T198.
