# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-24/25 (Opus): the EPA stream, built

The operator: "Not deploying yet, we need to get the EPA stream completed", then "push when they land and keep going".

### Done

- **The stream as planned in EPA-PROGRAMME § 4 is built.** § 2A is re-baselined and lists only what is left.
- **About 150 tasks were closed.** Each was implemented in a worktree, adversarially reviewed and fixed, squash-merged
  (one commit per task), suite-green, browser-checked on dev with scripted Chrome and the seeded accounts, and pushed.
- **Master = origin.** The last full run was 7,467 tests green.
- **The last checks of the session** found one real defect: a release after a withdraw committed evidence. It was fixed
  in `317670c`, test-first and mutation-checked.
- **Written up:**
  - the College message, drafted in `knowledge/college-message-2026-09.md` (12 questions, each with Wombat's interim
    answer);
  - STATE, rewritten;
  - DOMAIN.md, HANDOVER.md and CLAUDE.md, each corrected where the work showed them wrong.
- **T159 done:** the runbook is retargeted, and Acts 1–2 replayed cleanly on a fresh, separate database (`wombat_t159`,
  operator-approved). The snapshot is `recovery/t159-post-act2.dump`, and the replay's small gaps are filed as T291.

### For the operator

- **T157, the deploy:**
  - `pg_dump` production first.
  - Confirm `Email__SmtpHost` is set; without it, mail bodies with links are logged (T157's note).
  - Expect many migrations. T130, T219 and T281 empty the progress table, and the bootstrapper refills it.
- **New P2s "before real users"** (neither is exploitable today; there is no SSO provider and no real user):
  - T288: SSO group mappings across institutions;
  - T289: the assessor-profile form.
- **Defaults adopted on recommendation,** any of which can be overruled. All are in EPA-PROGRAMME § 3 or the task files:
  - D33 part 1, D34, D35, D38(a), D44–D50;
  - T131 O1–O8, T237, T240, T242 (c), T258, T268, T273, T281 and T284.
  - T249 records one choice as the operator's: the PDF prints group counts.
- **Waiting on the College:** T139 (§ 3F question 10) and T170 (question 11). Deferred: T146, T152, T153 and T171.
- **T128:** the backup destination. It is still blocked on the operator.

### Dev state

- **The dev trainee (profile 1) is ended,** with a last day of 2026-09-20, from T281's browser check. The permission
  classifier refused the agent's SQL restore. To restore it, run this, then restart the app so the bootstrapper refills
  progress:
  - `UPDATE "TraineeProfiles" SET "IsActive"=true,"DeactivatedOn"=NULL,"CompletedOn"=NULL WHERE "Id"=1;`
  - `DELETE FROM "CurriculumItemProgresses";`
- **Snapshots:** `recovery/pre-*.dump`, one before each migration batch, and `t159-post-act2.dump`. The dev app runs on
  its own database; `wombat_t159` holds the replay.
- **Dev accounts:** trainee, assessor, committee, committee2, coordinator, instadmin and collegeadmin (DevUserSeeder).
  The passwords of users created in the session are in `pwd_DO_NOT_COMMIT.txt` only.
- **Dev runs PostgreSQL 16; production runs 18** (T275).

### Traps

- **Merging lanes:**
  - Squash per task with `cherry-pick -n`, then compare HEAD with `git merge-tree --write-tree` of the branch.
  - Review fixes often sit inside a lane's merge commits, so take the full merge's version of any file that conflicts.
  - Regenerate a lane's migration whose timestamp sorts before master's newest.
- **Tests and commits:**
  - Run the suites in `.claude/worktrees/verify-master` while the dev app runs; it locks the per-project Release output.
  - Write commit messages to a file (`git commit -F`).
  - Never commit on master while a merge agent works.
- **Credentials:** agents may not read the admin credential; use the seeded accounts and say which steps need an
  Administrator.
- **The audit trap still bites.** A throw after staging commits the staged rows unless it is a refused save (T201).
