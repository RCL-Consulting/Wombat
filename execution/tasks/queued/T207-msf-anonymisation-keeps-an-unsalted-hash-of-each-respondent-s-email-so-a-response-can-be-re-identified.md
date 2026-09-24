---
id: T207
title: MSF 'anonymisation' keeps an unsalted hash of each respondent's email, so a response can be re-identified
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium: privacy. Anyone who knows the invited addresses can re-identify each response from the database.
**Surfaced:** 2026-09-24, the T202 review (finding 5).

## Symptom

`MsfInvitation.cs:43-44` stores an unsalted SHA-256 of the upper-cased email when anonymising, and nothing reads
`RespondentEmailHash`.

## What to build

Drop the hash if nothing needs it (grep). Otherwise key it with `Wombat__PseudonymSalt` (HMAC), as other pseudonyms are
keyed. Write a migration that clears existing hashes, W-007.

## Verification

- [ ] No unsalted respondent hash is stored after close, withdraw or auto-close. Tests plus a read-only SQL check on
      dev.

## Related

T184, T202, T026 (the pseudonym salt).
