# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-24 (Opus) — T102: a `user` field names only an eligible nominee

One P1 shipped; five follow-ups filed. The previous handoff (T122) is in `execution/log/2026-09-23-t122-handoff.md`.

### Done

- **[T102] fixes 2 and 3.** A *nominee field* is every `user` field plus every field a `field:` rule names. It may
  name only an active holder of the field's `role` (default `Assessor`) at the **activity's stamped institution**,
  never the subject, with no Administrator bypass. The rules:
  - a **changed** value is judged on every write, including a withdrawal;
  - an **unchanged** one only at the author's hand-on (D20's clause, now shared with T122 as `UnchangedFieldsHandedOn`);
  - the picker (`GetNomineeOptionsAsync`) runs the same query (`NomineeDirectory`);
  - save and publish refuse duplicate keys, a `field:` rule naming a non-user field, and options on a user field;
  - `UpdateActivityDraftCommand`/`UpdateDraftAsync` are **deleted** (no caller; they bypassed every guard).
- **Browser-verified on dev** (activity 16):
  - a forged option value is refused at create, and no draft is left behind;
  - a draft whose assessor then lost the role is **refused at the trainee's submit, by name**;
  - re-picking passes;
  - the new assessor lost the role too and **still completed and credited** (not re-judged on their own move);
  - a forged admin id is refused as "that person", and its name is never shown;
  - the builder's "Names a" select works;
  - roles were restored afterwards.
- **Review:** a 6-reader map, a 5-lens design critique (21 of 24 upheld: added the hand-on clause, "deactivated"
  rather than "locked out", the publish checks), then 3 implementation rounds with 3 refuters per finding.
  - Round 1 found a **label leak** (the picker looked up the working copy's id, so it showed any user's name and
    email), a Release-build break and a remount DbContext race.
  - Round 2 found only minor issues: the "Select…" display after a refusal, and the speciality admin roles.
  - Round 3 found wording only. All fixed. D23 closed.
- **Tests:** 274 added by six worktree agents, plus about 20 in round 2. Every area was mutation-checked; all mutants
  were killed.

### Filed, not fixed

- **[T149] P1: the SSO link endpoint is an unthrottled password oracle.** It binds the provider/subject from the form,
  not the cookie, and SSO sign-in ignores lockout. Confirmed in code (`Program.cs:358`,
  `ExternalLoginHandler.cs:123`), not in a browser.
- [T150] P3: sampling counts assessors on drafts and cancels. [T151] P3: nudges reach deactivated or opted-out
  nominees. [T152] P3: a supervisor from another institution cannot be named. [T153] P3: a trainee who left an
  institution still files there and is shown its staff.

### Next

1. **[T120] — Model: Opus.** The ten tools; each seed declares its `WbaToolKey`, and may declare `"role"` on a user
   field. See the T122 and T102 notes at the top of T120.
2. **[T149] — Model: Opus.** Security; small but needs care.
3. **[T148] — Sonnet.** Small and user-visible.
4. **Production deploy of T130 + T122 + T102:** take a `pg_dump` first. T102 has no migration.

### Traps

- **Label only what is STORED.** `ActivityForm.StoredDataJson` exists because the working copy can hold anyone's id;
  `GetUserOptionAsync` is unscoped by design. Never pass it a working-copy value.
- **Eligibility is the activity's, never the caller's.** Institution = the stamp; a null stamp admits nobody.
- **Don't re-judge stored nominees** except at the author's hand-on: it strands in-flight work (the T122 lesson).
- **Restoring a file with `cp -p` keeps its old mtime, and MSBuild then skips the rebuild.** Touch it after, or tests
  run stale DLLs.
- Six test worktrees under `.claude/worktrees/` (branches `worktree-wf_8c171d32-88b-*`) are merged into the task
  branch; remove them when convenient. The eight T122 worktree branches are still there too.
- The audit trap, the `--no-build` rule, and "stop only `Wombat.Web.exe`" all still apply.

### Verification status

- Suites, no `--no-build`: Domain **349**, Application **872**, Infrastructure **455**, Architecture **28**, Web
  **276**, for **1980** in all, up from 1555. Integration: 21 of 22, including 6 new PostgreSQL nominee tests
  (`DateTimeOffset.MaxValue` round-trips as `infinity`); the failure is the known MSF fixture, T140.
- Release build clean; `has-pending-model-changes` clean; `harness.py lint` clean. Branch `t102-user-field-validation`,
  squash-merged to master. **Not pushed.**
