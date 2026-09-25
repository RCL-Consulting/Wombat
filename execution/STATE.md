# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service**: scenario data only, no real trainees. **The CPSA Paediatric EPA v11.1
stream is built** (EPA-PROGRAMME § 2A, re-baselined 2026-09-25). What remains is the operator's deploy, the College's
answers, and a P3 polish backlog.

## Now

- **2026-09-24/25: about 150 tasks shipped.** Each was reviewed, fixed, merged, suite-green, browser-checked on dev and
  pushed. Master = origin; the last full run was 7,465 tests green.
- **Open: 50 queued, 1 blocked.**
  - **P1:** T157, the deploy, which waits for the operator.
  - **P2, merged and in the final browser check:** T278, T279, T282 and T285.
  - **New P2s, "before real users":** T288 (SSO group mappings across institutions; no provider is configured) and T289
    (the assessor-profile form).
  - **The rest are P3:** polish, accessibility, and design debt.
- **Gated on the College or the operator:** T139, T170, T146, T152, T153 and T171. The College message (12 questions,
  each with Wombat's interim answer) is drafted in `knowledge/college-message-2026-09.md`.
- **T159:** the paediatric runbook is retargeted onto the seeded catalogue. The replay on a fresh dev database is next.

## Blockers

- **T128:** the off-host backup destination. It needs one operator decision: where the encrypted copy goes, and who
  holds the `age` private key.

## Next

- **Replay T159 on a fresh dev database:** `pg_dump` first, then drop, migrate and seed. The runbook's steps 1.11–1.12
  need an Administrator. The planned stand-in is an SQL insert of KGK, with instadmin moved there briefly to invite
  Mbatha.
- **T157, the deploy (operator).**
  - Take a `pg_dump` first. The migrations since the 16 September build are many, and T130, T219 and T281 empty the
    progress table for the bootstrapper to refill.
  - Check that production sets `Email__SmtpHost` (T157's note).
- Send the College the § 3F questions, and decide T128.

## Open questions

- **Defaults adopted on recommendation (the operator may overrule).** All are in EPA-PROGRAMME § 3:
  - D33 part 1, D34, D35, D38(a), D44–D47, and D48–D50;
  - T131's O1–O8, T237 (no Trainee in any panel seat), T240 (the draft reminder is a digest);
  - T258 (erasure may be requested whatever the reviews' state), T268 (a locked account is not current);
  - T273, T281 (no credit after the last day) and T284 (opening a draft is new work).
- **Dev database:** snapshots are in `recovery/pre-*.dump`. Dev runs PostgreSQL 16, production 18 (T275).

## Files to open first

- `CLAUDE.md`: conventions, footguns, and the "nothing is live" section.
- `execution/knowledge/EPA-PROGRAMME.md`: § 2A (what is left), § 3 (decisions D1–D50), and § 3F (College asks).
- `execution/architecture/DESIGN.md`: mandatory before any Razor work.
- `deploy/verify/drift-check.sh`: run before trusting any claim about the server.

## Recent

- **2026-09-24/25:** the stream and its follow-ups shipped in about 20 batches. See `git log` and each "As built".
