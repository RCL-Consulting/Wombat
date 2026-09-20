# Knowledge — what the project knows

The slow-changing half of the workspace. **You do not read this folder at session start** —
`CLAUDE.md` imports `execution/STATE.md` and `execution/HANDOFF.md`, and the SessionStart hook
injects the active task. Come here when you need a contract.

Its companion is **`execution/architecture/`** — how the system is *built*
(`ARCHITECTURE.md`, `DESIGN.md`, `CUSTOMIZATION.md`, `INFRASTRUCTURE.md`). This folder is what
the project *knows*: the domain, the programme, the runbooks, the closed plans.

## Where to go

1. **Writing Razor?** `../architecture/DESIGN.md`, first, every time.
2. **Touching the activity platform?** `../architecture/CUSTOMIZATION.md`, then `DOMAIN.md`
   for what the words mean.
3. **Adding a project reference or a handler?** `../architecture/ARCHITECTURE.md` — the
   boundaries are test-enforced.
4. **Operating the live service?** `HANDOVER.md`, then `../architecture/INFRASTRUCTURE.md`.
5. **Replaying a scenario?** `scenario-paediatrics.md`; its findings sections are superseded
   history and say so.
6. **Wondering why something is the way it is?** Product decisions `D1`–`D38` are in
   `EPA-PROGRAMME.md` § 3; process decisions `W-nnn` are in `../DECISIONS.md`.

## Document map

| File | Purpose |
|---|---|
| `DOMAIN.md` | What EPAs, WBAs, STAR and the role hierarchy actually mean. |
| `EPA-PROGRAMME.md` | The CPSA Paediatric v11.1 catalogue programme, its phases, and product decisions **D1–D38**. |
| `HANDOVER.md` | Running the live service: what is deployed, config, deploys, backups, logs, known limitations. The T016 deliverable. |
| `WORKFLOW.md` | Git branching and verification levels. **Its session-handoff protocol is superseded** by the harness — see the banner at the top of that file. |
| `PLAN.md` | The original rewrite plan. T001–T027, complete June 2026. Historical. |
| `scenario-*.md` | Replay runbooks that produce the scenario test corpus. |
| `practical-plan.md`, `gui-review-plan.md` | Closed post-rewrite plans, kept as record. |
| `book-fidelity-plan.md` | Superseded by `practical-plan.md`; kept only because `EPA Book/critique.md` cites it. **Do not execute tasks from it.** |

## How these docs got here

They began as `Rewrite/`, the plan for rebuilding Wombat on the ClinicAssist.NET architecture.
**That rewrite finished in June 2026** (T001–T027; see `PLAN.md` § Status) and the application
has been deployed since 2026-06-19. Everything since — the activity platform, the EPA v11.1
catalogue, security hardening, the operational work — is ordinary product work.

On 2026-09-20 the folder was renamed `Programme/`, because the old name had outlived what it
described, and then folded into this workspace so that the project keeps everything it knows
in one place. See **W-004** and **W-005** in `../DECISIONS.md`. Paths in
`../log/_pre_harness_current_state.md` and in completed task files still read `Rewrite/` or
`Programme/`; they were correct when written and are left alone.

## Reference material

Neither reference tree is vendored into this repo any more — both were deleted on 2026-09-20.

- **ClinicAssist.NET** — the reference architecture to copy from. Live working copy at `C:\Users\Renier\ClinicAssist.NET`. Treat as read-only. When in doubt about "how should X be structured", look there first.
- **The old Wombat source** — in this repo's own history at commit `55a92c6`, the parent of the scaffold commit `c843421`. Use `git show 55a92c6:<path>` for one file, or `git worktree add ../wombat-old 55a92c6` for the whole tree.

## Scope discipline

This plan deliberately excludes:

- Any attempt to migrate data from the old Wombat. There are no real users, so there is no data to migrate.
- Any attempt to keep the old Wombat running alongside the new one. The old code is reference only.
- Any feature not present in the current Wombat, unless `DOMAIN.md` flags it as a correctness fix. New features are added *after* parity is reached.

If an agent session is tempted to do any of the above, it should stop and add a task file instead of silently expanding scope.
