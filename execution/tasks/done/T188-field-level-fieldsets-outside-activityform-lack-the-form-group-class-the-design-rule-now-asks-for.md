---
id: T188
title: Field-level fieldsets outside ActivityForm lack the form-group class the design rule now asks for
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T188 — Field-level fieldsets outside ActivityForm lack the form-group class the design rule now asks for

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

- [x] Each page renders its group like a field. Browser.

## Related

T177, T147, T122, T125.

---

## As built — 2026-09-25 (`0a9a4cb`)

Every field-level fieldset renders as a form group: a legend at the label's size, help at the help's size, and a nested
rule running the legend's full height. DESIGN.md's rule covers every field-level fieldset, and a scan test enforces it.
"Evidence it rests on" is marked required.

Browser on dev (scripted Chrome, master `e22d58b`; `pg_dump -n public` first, at `recovery/pre-t283-t281-migrations.dump`): the fieldsets on the campaign, curriculum-item and staging forms read at the label's size, with grey help. Nested
groups are named by their legend, and section legends keep their weight.
