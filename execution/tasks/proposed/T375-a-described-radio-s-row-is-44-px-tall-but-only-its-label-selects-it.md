---
id: T375
title: A described radio's row is 44 px tall but only its label selects it
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-20
---

# T375 — A described radio's row is 44 px tall but only its label selects it

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A touch target that looks 44 px but answers on 24 px: a missed tap at 390 px, nothing wrong recorded.
**Surfaced:** 2026-10-20, T373's states pass (Invite a person at 390 px).

## Symptom

On Invite a person (`/admin/invitations/new`, the role radios) and Schedule review (Review type), each choice is a row
with `min-height` 44 px holding the radio, its label and a help line. A click or tap on the help line, or on the row
below the 24 px label, does not choose the radio.

## Root cause

`.form-check.review-type-choice` is a `div`; only the `<input>` and its `<label for>` take the click. The help `<p>`
is a sibling, tied to the input by `aria-describedby` only (`InviteAPerson.razor:99`, `ScheduleReview.razor:157`).

## What to build

Make the whole row choose the radio: wrap the row in the label (keeping the help as the description), or stretch the
label's hit area over the row. Keep the accessible name the label alone, as T364's build review D7 asked.

## Verification

- [ ] At 390 px, a tap on each row's help line chooses that radio, on both pages — a Playwright check.
- [ ] Each radio's accessible name is its label only, its description the help — the accessibility tree.
- [ ] Web suite green.

## Related

T373 (flow 11), T364 (flow 08, Review type); `app.css` `.review-type-choice`.

## Notes
