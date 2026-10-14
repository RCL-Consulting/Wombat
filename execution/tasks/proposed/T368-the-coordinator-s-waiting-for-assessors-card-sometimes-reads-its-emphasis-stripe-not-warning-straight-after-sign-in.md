---
id: T368
title: The Coordinator's Waiting for assessors card sometimes reads its emphasis stripe, not warning, straight after sign-in
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-14
---

# T368 — The Coordinator's Waiting for assessors card sometimes reads its emphasis stripe, not warning, straight after sign-in

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low, unconfirmed: a stripe colour only; the badge's words ("3 waiting, 1 overdue") are right every time.
**Surfaced:** 2026-10-14, T366's replay, Step A.5.10 (appendix agent; classified new-defect, needs confirmation).

## Why

The appendix replay saw the Coordinator's Home card "Waiting for assessors" carry `detail-card--emphasis` instead of
`detail-card--warning` in 7 of 9 reads taken straight after sign-in through a heading-filtered Playwright locator, and
once in A.7.5's Home read. Every one of 15 probes that polled the DOM or waited first read warning, so it may be a read
taken mid-render (the card drawn before its overdue count is known). Not reproduced on demand. Flow 06's card, not flow
09's.

## Symptom

Possibly: for a moment after the Home loads, the card's stripe is the emphasis colour though one request is overdue.

## Root cause

Not yet known. Check whether the card renders with a default tone before its read completes, and whether that state
is ever left on screen.

## What to build

Decide after the cause: if the card draws a tone before it knows, draw none (or the skeleton) until it does.

## Verification

- [ ] The cause stated from the code, and the card's first render checked with the read delayed (bUnit) — a test.
