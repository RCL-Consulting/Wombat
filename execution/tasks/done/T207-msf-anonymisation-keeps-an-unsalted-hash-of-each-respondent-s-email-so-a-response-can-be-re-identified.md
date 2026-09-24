---
id: T207
title: MSF 'anonymisation' keeps an unsalted hash of each respondent's email, so a response can be re-identified
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T207 — MSF 'anonymisation' keeps an unsalted hash of each respondent's email, so a response can be re-identified

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

- [x] No unsalted respondent hash is stored after close, withdraw or auto-close. Tests plus a read-only SQL check on
      dev.

## Related

T184, T202, T026 (the pseudonym salt).

---

## As built — 2026-09-25 (`f816096`)

`RespondentEmailHash` is dropped. Nothing read it, so there was nothing to re-key. `MsfInvitation.Anonymize` nulls the
address and keeps nothing derived from it. The migration `T207_DropMsfRespondentEmailHash` nulls every stored hash
before the drop. A no-trace test runs close, auto-close and withdraw through the app and searches every table
(including audit and job-run rows) for the address or any of its digests.

Browser/SQL on dev (scripted Chrome, `150417e`):  `MsfInvitations` has no hash column (no `%EmailHash%` column anywhere), and 0 anonymised rows keep an address.
CUSTOMIZATION.md updated.
