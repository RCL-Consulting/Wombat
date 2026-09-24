---
id: T188
title: Field-level fieldsets outside ActivityForm lack the form-group class the design rule now asks for
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low, visual consistency only.
**Surfaced:** 2026-09-24, the T177 review (finding 2).

## Symptom

`CampaignEdit.razor:93`, `CurriculumItemsEdit.razor:256` and `StageMinimaEditor`'s fieldset in the Add form are
`fieldset.full-width` groups sitting among `FormField`s, without `form-group`. T177 scoped DESIGN.md's rule to
ActivityForm to avoid unrelated page work.

## What to build

Give them `form-group`, widen the DESIGN.md rule to cover every field-level fieldset, and check each page in the
browser.

## Verification

- [ ] Each page renders its group like a field. Browser.

## Related

T177, T147, T122, T125.
