---
id: T280
title: Accessibility leftovers: trainee dashboard links named only by instrument, the alert's dismiss button named "×", and the review PDF leaving out a review under appeal
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T280 — Accessibility leftovers: trainee dashboard links named only by instrument, the alert's dismiss button named "×", and the review PDF leaving out a review under appeal

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T239 and T250 reviews.

## Symptom and what to build

- **Trainee dashboard links.** The inbox, recent and deadline links in `TraineeDashboard.razor` are named only by
  `ActivityTypeName`. Carry the EPA or date on the dashboard DTO, and name each link uniquely.
- **The alert's dismiss button** in `Alert.razor` is named "×". Give it `aria-label="Dismiss"`.
- **The portfolio PDF leaves out a review under appeal.** `PortfolioPdfService` exports only Ratified or Final reviews,
  so a review under appeal drops out of the PDF until the appeal is resolved (since `6faa3fd5`). Decide, and state
  which: include it, marked "under appeal".

## Verification

- [ ] Each, with bUnit or PDF text tests.

## Related

T239, T250, T212.
