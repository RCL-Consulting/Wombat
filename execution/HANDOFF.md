# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-24 (Opus) — T102, T105, T120, and W-007

Three tasks shipped (two P1s); six follow-ups filed. The previous handoff (T122) is in
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
- **W-007: every open task carries a "compatibility is not a constraint" preamble**, from `tasks/_template.md`. The
  operator asked for it to be durable at the point of work. CLAUDE.md § Nothing is live points at it.

### Filed, not fixed

- **[T149] P1: the SSO link endpoint is an unthrottled password oracle.** It binds the provider/subject from the form,
  and SSO sign-in ignores lockout. Confirmed in code (`Program.cs:358`, `ExternalLoginHandler.cs:123`).
- [T150]–[T153] P3 (T102 follow-ups): sampling counts drafts and cancels; nudges reach deactivated or opted-out
  nominees; no cross-institution supervisor; a trainee who left still files at the old institution.
- [T154] P3: clinical audit and portfolio review (D34 and a design).

### Next

1. **[T149] — Model: Opus.** Security; small but needs care.
2. **[T099] — Model: Opus.** The last other P1.
3. **[T148] — Sonnet.** Small and user-visible.
4. **Production deploy of T130 + T122 + T102 + T105 + T120:** take a `pg_dump` first. T105 and T120 add no migration;
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

- Suites, no `--no-build`: Domain **356**, Application **895**, Infrastructure **568**, Architecture **28**, Web
  **288**, for **2135** in all, up from 1555 at session start. Integration: 21 of 22; the failure is T140.
- Release build clean; `harness.py lint` clean. T102 is pushed; T105 and T120 are squash-merged on master, **not
  pushed**. Dev DB: activities 16–19 are this session's browser checks.
