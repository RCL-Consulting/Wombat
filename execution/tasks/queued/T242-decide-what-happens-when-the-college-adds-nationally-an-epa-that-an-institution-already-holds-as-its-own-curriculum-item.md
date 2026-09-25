---
id: T242
title: Decide what happens when the College adds nationally an EPA that an institution already holds as its own curriculum item
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T242 — Decide what happens when the College adds nationally an EPA that an institution already holds as its own curriculum item

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The College stays blocked, and says why.
**Surfaced:** 2026-09-25, the T223 review (finding 3).

## The question

National and local items cannot share an EPA on one curriculum, which T223's exclusion constraint enforces. So an
institution's own item on a national EPA blocks the College from adding that EPA nationally. When the College adds it,
is the institution's item:
- (a) removed, and its trainees measured by the national item;
- (b) absorbed, so the national item takes the local one's place and its credit history; or
- (c) the add keeps being refused until the institution removes its own item, which is today's behaviour?

**Recommendation:** (c), with the refusal naming the institution. The College owns the national list, but deleting an
institution's configuration from under it is not the College's to do silently. With no real users (W-007), (a) would be
cheap if the operator prefers it.

## Verification

- [ ] The decision is recorded in EPA-PROGRAMME § 3D. If it changes behaviour, a handler test.

## Related

T223, T222, T211, T195.
