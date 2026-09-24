# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-24 (Opus) — T102, T105, T120, T149, T099, and W-007

Five tasks shipped (four P1s); nine follow-ups filed. The previous handoff (T122) is in
`execution/log/2026-09-23-t122-handoff.md`.

### Done

- **[T102] — a `user` field names only an eligible nominee.** Every `user` field, plus any field a `field:` rule
  names, may name only an active holder of the field's `role` (default Assessor). They must be at the activity's
  stamped institution, never the subject, with no Administrator bypass. A changed value is judged on every write;
  an unchanged one only at the author's hand-on (shared with T122). The picker runs the same query, and labels only
  STORED values. `UpdateDraftAsync` is deleted.
  - Pushed as `cbc6ca2`, after a 5-lens design critique and 3 review rounds. Round 1 caught a label leak.
  - Browser, activity 16: forged ids were refused; a stale assessor was refused at the trainee's submit; the
    assessor's own completion was not re-judged.
- **[T105] — a transition declares how much of the form it checks.** `validation` is `all` (the default: every visible
  required field), `owned` (the required fields the mover can write before moving) or `draft` (formats only).
  `requires_fields` adds under every value. `owned` was needed: with honest `required` flags on the assessor's fields, a
  whole-schema check at the trainee's submit would refuse every submit.
  - All 12 seeds with transitions were re-authored; the first boot republished 12 and the second none.
  - Browser, activities 17 and 18: a half-filled draft cancels; a decline needs a note and no ratings; complete names
    all four missing ratings. Master `0a767c2`, not pushed.
- **[T120] — four instruments seeded.**
  - Rated: `cca_cpsa`, `rca_cpsa` and `chart_stimulated_recall_cpsa`.
  - Unrated: `reflective_exercise_cpsa`, which credits nothing (D6, D7). The trainee writes it; a named supervisor or
    mentor records the discussion or returns it.
  - Browser, activity 19: a CCA against PAED-001 credited 1 item at 3a. It charts as "Case analysis". The CCA EPA
    picker offered exactly PAED-001, 002, 003, 004 and 006.
  - Boots: the first inserted 4 types; the second changed nothing.
  - Clinical audit and portfolio review were split to **[T154]** (the operator's call: defer).
- **[T149] — the SSO link endpoint and SSO sign-in, hardened.** Linking reads the provider, subject and email from the
  external cookie, never the form. It refuses SSO-only accounts and Administrators, checks the password with lockout,
  and is rate-limited. SSO sign-in refuses a deactivated account (not a brute-force lockout), an Administrator, and an
  account of another institution. Erasure and an institution move drop external logins.
  - One review: no major defect; its minor findings are fixed. 14 handler tests and a PostgreSQL erasure test,
    all mutation-checked; on dev, curl showed the 11th link post rate-limited.
- **[T099] — closed by finding.** Dev was fixed on 2026-09-17, and a fresh database is fixed by T130's `DevUserSeeder`.
  A read-only production query showed production at the 16 September build (last migration T096), with one user and no
  catalogue, so there is nobody to re-scope. The deploy is **[T157] (P1)**.
- **W-007: every open task carries a "compatibility is not a constraint" preamble**, from `tasks/_template.md`. The
  operator asked for it to be durable at the point of work. CLAUDE.md § Nothing is live points at it.

### Filed, not fixed

- **[T155] P2: SSO sign-in rewrites an account's email from an unverified claim**, and can re-key it onto another
  person's address. From the T149 review.
- [T156] P3: login hardening leftovers (a /24 throttle that counts successes, enumeration, a sliding external cookie).
- [T150]–[T154] P3: T102 follow-ups (sampling, nudges, cross-institution, leavers); T154 audit and portfolio review.

### Next

1. **[T157] — the production deploy (operator, or Sonnet with your go-ahead).** `pg_dump`, then `deploy/deploy.ps1`.
   Production holds only the admin, so the risk is low but the migrations are many.
2. **[T155] — Model: Opus.** Security, P2.
3. **[T125], then [T135] — Opus.** The EPA track's ready work. T105 and T120 add no migration;
   the refresher republishes 12 types and the seeders create 4.

### Traps

- **Label only what is STORED** (`ActivityForm.StoredDataJson`): `GetUserOptionAsync` is unscoped by design.
- **A new seed's transitions declare `validation`;** copy `mini_cex_cpsa`. Forgetting it gives `all`, which refuses
  cancel and decline on a half-filled form.
- **Pinned seed lists live in five test files** (CpsaWbaSeedTests, SeedPublishabilityTests, SeedScaleKeyTests,
  PaediatricCatalogueToolSeedTests, WbaToolAllowListPostgresTests). A new seed fails them by design. Update them
  deliberately; never loosen them. T122's migration snapshots are permanent: never edit them.
- **Restoring a file with `cp -p` keeps its mtime** and MSBuild skips the rebuild. Touch it after.
- Test worktrees `.claude/worktrees/wf_8c171d32-88b-*` (6) and the eight T122 worktree branches are merged
  leftovers; remove them when convenient.
- The audit trap, the `--no-build` rule, and "stop only `Wombat.Web.exe`" all still apply.

### Verification status

- Suites, no `--no-build`: Domain **356**, Application **895**, Infrastructure **582**, Architecture **28**, Web
  **288**, for **2149** in all, up from 1555 at session start. Integration: 22 of 23; the failure is T140.
- Release build clean; `harness.py lint` clean. T102, T105, T120 and T149 are pushed; T099 is squash-merged on master,
  **not pushed**. Dev DB: activities 16–19 are this session's browser checks.
