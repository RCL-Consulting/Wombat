---
id: T141
title: A trainee cannot reach My progress from the navigation
status: queued
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-23
---

# T141 — A trainee cannot reach My progress from the navigation

**Severity:** Low. The page exists and works; nothing in the navigation leads to it.
**Surfaced:** 2026-09-23, during [T130]'s browser verification.

## Symptom

The trainee navigation's "My Curriculum" links to `/placeholder/my-curriculum`. `/portfolio/progress`, the page a
registrar opens to see what is expected of them this period, is reachable only through the dashboard card.

## Root cause

The nav item predates the page. The Trainee block is shared with PendingTrainee (`NavMenu.razor:39`), and
`/portfolio/progress` is `[Authorize(Roles = "Trainee")]`, so pointing the shared item at it would refuse a pending
trainee.

## What to build

Point "My Curriculum" at `/portfolio/progress` for Trainees only (split the item out of the shared block), and update
the nav table in `execution/architecture/DESIGN.md` in the same change.

## Verification

- [ ] A Trainee reaches the page in one nav click; a PendingTrainee does not see the item — checked by
      `NavMenuAuthorizationTests` and in the browser

## Related

[T130]; `NavMenu.razor:64-69`; DESIGN.md § The NavMenu.
