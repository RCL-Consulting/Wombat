---
id: T191
title: Choice options in some seeds are bare keys, so the form shows admission_notes instead of a label
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

**Severity:** Low. The form reads as a database, not a clinical form.
**Surfaced:** 2026-09-24, the T177 browser check (the CCA's "Documentation reviewed").

## Symptom

`cca_cpsa/schema.json`'s `documents_reviewed` options are bare strings (`admission_notes`, `progress_notes`, …), and
the form shows them verbatim. Other seeds may do the same.

## What to build

Survey every seed's `choice`/`multichoice` options, and give them labels in whatever option shape the DSL supports
(check `FormSchemaParser`). The refresher republishes changed seeds. Stored values must not change: keep the keys and
change only the labels.

## Verification

- [ ] No seeded option renders a snake_case key. Test over every seed schema.

## Related

T120 (the CCA seed), T177.
