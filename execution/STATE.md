# State — wombat

Cap: 60 lines. `harness.py lint` enforces it; `harness.py trim` moves the overflow into `log/`.
This is a reset point, not a diary.

## Focus

Wombat is **deployed but not in service**: scenario data only. **The EPA v11.1 stream is built**, and since 2026-09-26
the runbook is a replayed journey catalogue with a GUI screenshot baseline, the groundwork for a Claude Design
redesign. Left: the operator's deploy, the College's answers, 17 P2s "before real users", and P3 polish.

## Now

- **The runbook** (`knowledge/scenario-paediatrics/`, T293): 324 steps playing all 80 page templates; `coverage.md`
  indexes every role's jobs, `states.md` 577 states; T294's test holds it to the routes. **Replayed** (T295,
  `wombat_scenario`): all outcome checks matched; baseline in gitignored `design/baseline/` (1,250 screenshots).
- **Open: 73 queued (1 P1, 17 P2, 55 P3), 1 blocked.**
  - **P1:** T157, the deploy, which waits for the operator.
  - **P2, from the replay:** T297, T302–T305, T307, T311–T313, T315, T316, T319, T320, T322 and T329. **Earlier:** T288
    and T289.
  - **The rest are P3:** polish, accessibility, and design debt; T297–T331 added 20.
- **Gated on the College or the operator:** T139, T170, T146, T152, T153, T171 (`knowledge/college-message-2026-09.md`).

## Blockers

- **T128:** the off-host backup destination. It needs one operator decision: where the encrypted copy goes, and who
  holds the `age` private key.

## Next

- **T157, the deploy (operator).**
  - Take a `pg_dump` first. The migrations since the 16 September build are many, and T130, T219 and T281 empty the
    progress table for the bootstrapper to refill.
  - Check that production sets `Email__SmtpHost` (T157's note).
- Send the College the § 3F questions, and decide T128.
- **The GUI redesign:** brief Claude Design from `coverage.md`'s journeys, `states.md` and `design/baseline/`, and decide
  how much of `DESIGN.md`'s frozen structure may change (a reskin or a restructure). Fix the P2s before real users.

## Open questions

- **Defaults adopted on recommendation (the operator may overrule).** All are in EPA-PROGRAMME § 3:
  - D33 part 1, D34, D35, D38(a), D44–D47, and D48–D50;
  - T131's O1–O8, T237 (no Trainee in any panel seat), T240 (the draft reminder is a digest);
  - T258 (erasure may be requested whatever the reviews' state), T268 (a locked account is not current);
  - T273, T281 (no credit after the last day) and T284 (opening a draft is new work).
- **Dev database:** snapshots are in `recovery/pre-*.dump`; the replay's are `recovery/scenario-post-act*.dump`. Dev runs
  PostgreSQL 16, production 18 (T275).

## Files to open first

- `CLAUDE.md`: conventions, footguns, and the "nothing is live" section.
- `execution/knowledge/EPA-PROGRAMME.md`: § 2A (what is left), § 3 (decisions D1–D50), and § 3F (College asks).
- `execution/architecture/DESIGN.md`: mandatory before any Razor work.
- `execution/knowledge/scenario-paediatrics/README.md` and `coverage.md`: the journeys, and how to replay them.
- `deploy/verify/drift-check.sh`: run before trusting any claim about the server.

## Recent

- **2026-09-24/25:** the stream and its follow-ups shipped in about 20 batches. See `git log` and each "As built".
- **2026-09-26:** T292–T296 (dev Administrator, runbook rewritten, guarded, replayed; docs); findings T297–T331.
