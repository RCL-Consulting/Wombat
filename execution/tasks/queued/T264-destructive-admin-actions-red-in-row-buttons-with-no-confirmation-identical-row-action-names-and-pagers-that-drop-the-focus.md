---
id: T264
title: Destructive admin actions: red in-row buttons with no confirmation, identical row-action names, and pagers that drop the focus
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T264 — Destructive admin actions: red in-row buttons with no confirmation, identical row-action names, and pagers that drop the focus

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is one click from a deactivation, a lock-out or a delete, and screen-reader users hear "Revoke,
Revoke, Revoke".
**Surfaced:** 2026-09-25, the T234 review (and its implementer's list).

## Symptom

- **No ConfirmDialog on a red button:**
  - Deactivate on the college, institution, speciality and sub-speciality pages;
  - Lock out, Remove and Revoke all on the user page;
  - Revoke on invitations;
  - Delete on SSO mappings;
  - Delete on the entrustment scales list;
  - the Revoke panel on entrustment decisions, which is a card, not a ConfirmDialog.
- **Identical row-action names with no `aria-label`:** Revoke, Delete, Withdraw, the user page's Remove, and the review
  page's Remove.
- **Pagers drop the focus on a page change:** `AuditList` (it nulls `_result`, which removes the pager),
  `RequestsList`, and `PagerControls` (Next and Previous disable themselves at either end).
- **Discard changes on the activity page** disables itself when pressed.

## What to build

Apply DESIGN.md's destructive-action pattern (T222, T206): a ConfirmDialog naming the target, a `btn-outline` in-row
trigger, and the focus moved to the result. Give each row action an `aria-label` naming its row. Keep pagers mounted,
and keep the focus on the pressed control or move it to the list heading. bUnit for each page.

## Verification

- [ ] Each listed action asks first and names its target, and each pager keeps the focus. bUnit.

## Related

T234, T222, T206, T239.
