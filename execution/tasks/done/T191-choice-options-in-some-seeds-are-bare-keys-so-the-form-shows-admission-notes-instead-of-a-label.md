---
id: T191
title: Choice options in some seeds are bare keys, so the form shows admission_notes instead of a label
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T191 — Choice options in some seeds are bare keys, so the form shows admission_notes instead of a label

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

- [x] No seeded option renders a snake_case key. Test over every seed schema.

## Related

T120 (the CCA seed), T177.

---

## As built — 2026-09-25 (`5a29004`)

Choice and multichoice options may carry a label (`{ "value", "label" }`, `FieldOption`; Parse + Serialize +
round-trip fixture). Stored values are unchanged. Every seed's options are labelled. The builder's Options box is one
option per line, written `value | Label`. The parser refuses a `|` or a line break in a value, a line break in a label,
and a value offered twice, naming the field. The PDF prints a scale's rung first, then the option's label.

Browser on dev (scripted Chrome, `2561f07`): 
- **Boots.** The first boot republished exactly the 15 changed seeds, the second none.
- **The form.** A new CCA shows "Admission notes …" and "PICU".
- **Storage.** Activity 35 stores `admission_notes` and `picu`, and its page shows the labels.

CUSTOMIZATION.md's option form is updated.
