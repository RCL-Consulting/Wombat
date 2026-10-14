---
id: T367
title: A revoked STAR reaches its trainee: the revocation mail, and My authorisations reads revoked and expired STARs (T319's revocation half)
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-10-14
started: 2026-10-14
---

# T367 — A revoked STAR reaches its trainee: the revocation mail, and My authorisations reads revoked and expired STARs (T319's revocation half)

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium (T319's). The register says "The trainee is notified." and nothing is sent; a revoked STAR
vanishes from the trainee's page.
**Surfaced:** 2026-10-14, flow 09 round 1 (T366). The operator took the recommendations ("go ahead"): send the mail
(§ 6 Q7), list revoked and expired STARs with the reason on the page (Q8), and land this half of T319 first, before
flow 09's build, because the design rests on it (Q11; BRIEF § 11 flow 05).

## Symptom

T319 § Symptom, its revocation half (Steps 4.36, 4.42): no mail after a revoke; the card simply disappears.

## Root cause

T319 § Root cause: `RevokeEntrustmentDecision.cs:35-78` has no sender; `GetActiveDecisionsForTrainee.cs:49` filters
`Status == Active`.

## What to build

T319 § What to build, "One way to send a one-off notice" and "Revocation", with these decisions:
- The one-off notice path (after the commit; to the account; skips a deactivated account, not a digest opt-out; a
  failed send is logged and never fails the command; no free text about the person). T319's data-rights half reuses it.
- `EntrustmentDecisionRevokedEmail`: EPA code and title, level, date revoked, the reviser **by name**; no reason; a link
  to `/portfolio/authorisations`. Sent by `RevokeEntrustmentDecisionCommandHandler` after the save.
- The trainee's query widened to revoked and expired STARs (superseded left off), each with status, date, the reviser
  by name, **the revocation reason** (shown on her page, under her own access control: the operator's Q8), and its
  certificate. The page's layout is flow 09's (T366); this task supplies the query and DTO, with at most a plain
  interim section.
- The register's sentence stays and becomes true; its wording is flow 09's.

## Verification

- [ ] A handler test: a revoke sends one `EntrustmentDecisionRevokedEmail` to the trainee after the save, none on a
  refused revoke, none to a deactivated account, and a failed send does not fail the command.
- [ ] A query test: the trainee's STARs include revoked and expired (with reason, reviser name, date) and not superseded.
- [ ] The six suites green; Step 4.36's mail seen in the replay app's log.

## Related

T319 (its data-rights half stays there), T366 (flow 09), T101, T201, T274, T282.

## Notes
- **2026-10-14, built** (`e9b29081`). `AccountNotices` (Application, `Common/Email`) is the shared one-off notice path:
  after the save, to the account, a deactivated one skipped, a failed send logged (`CancellationToken.None`, as an
  invitation's), no free text. `EntrustmentDecisionRevokedEmail`: EPA, level, the South African day, the reviser by name
  (none when the account has no name), a link to `/portfolio/authorisations`; never the reason.
  `GetActiveDecisionsForTraineeQuery` renamed `GetDecisionsForTraineeQuery`: Active, Revoked, Expired, not Superseded;
  `RevokedByName` on the DTO (filled by this query only). My authorisations: the in-force cards, then a plain interim
  "No longer in force" (status, the day and reviser, the reason). Tests: `Revoke_MailsTheTraineeOnce…`,
  `Revoke_Refused_MailsNobody`, `Revoke_OfADeactivatedTrainee…`, `Revoke_WhoseMailFails…`,
  `GetDecisionsForTrainee_ReadsActiveRevokedAndExpired_NotSuperseded`. Six suites green: 814 / 3786 / 1022 / 52 / 3636
  / 491. One Sonnet review: no defects (both hosts resolve `AccountNotices`; nothing after the save can throw).
  **Open:** Step 4.36's mail in the replay app's log, seen in flow 09's replay (T366 step 7); then `task done`.
