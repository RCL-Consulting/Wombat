# Wombat Programme — Index

This folder holds **the contracts that change rarely**: the domain model, the architecture and
design rules, the customization model, the infrastructure and handover docs, the plans, and the
scenario runbooks.

**Live state is not here.** It moved to `execution/` on 2026-09-20, where the `rcl-harness`
script bounds and enforces it:

| Was | Is now |
|---|---|
| `Programme/current_state.md` (3,574 lines, read every session) | `execution/STATE.md` (≤60 lines) + `execution/HANDOFF.md` (≤80), both imported automatically |
| `Programme/Tasks/T0xx-*.md` | `execution/tasks/{queued,in_progress,blocked,done}/` |
| — | `execution/DECISIONS.md`, `execution/DASHBOARD.md` (generated), `execution/log/` |

The old handoff is archived at `execution/log/_pre_harness_current_state.md`. It is history, not
state. See the **Execution workspace** section at the end of `CLAUDE.md` for the commands.

## How to use this folder

You do not read this folder at session start — `CLAUDE.md` imports the state you need. Come here
when you need a contract:

1. **Writing Razor?** `DESIGN.md`, first, every time.
2. **Touching the activity platform?** `CUSTOMIZATION.md`, then `DOMAIN.md` for what the words mean.
3. **Adding a project reference or a handler?** `ARCHITECTURE.md` — the boundaries are test-enforced.
4. **Operating the live service?** `HANDOVER.md`, then `INFRASTRUCTURE.md`.
5. **Replaying a scenario?** `scenario-paediatrics.md`; note its findings sections are superseded history.

It began life as `Rewrite/`, the plan for rebuilding Wombat on the ClinicAssist.NET architecture. **That rewrite finished in June 2026** (T001–T027; see `PLAN.md` § Status) and the application has been deployed since 2026-06-19. Everything since — the activity platform, the EPA v11.1 catalogue, security hardening, the operational work — is ordinary product work tracked the same way. The folder was renamed on 2026-09-20 because the old name had outlived what it described.

## Document map

| File | Purpose |
|---|---|
| `PLAN.md` | Master plan. Phases, task list, progress checkboxes. |
| `DOMAIN.md` | What EPAs, WBAs, STAR and the role hierarchy actually mean. Corrects misunderstandings in the current Wombat model. |
| `ARCHITECTURE.md` | Clean Architecture / CQRS layout, conventions, non-negotiables. |
| `DESIGN.md` | The canonical UI/design-system contract: tokens, layout grid, buttons, tables, forms, cards, dashboards, alerts, skeletons, icons, and the `app.css` section order. **Any task that writes Razor must read this first.** |
| `CUSTOMIZATION.md` | The Activity platform: the jsonb schema, workflow and credit DSLs, and where the line between platform and hardcoded sits. |
| `EPA-PROGRAMME.md` | The CPSA Paediatric v11.1 catalogue programme, its phases and its numbered decisions. |
| `WORKFLOW.md` | Git branching and verification levels. **Its session-handoff protocol is superseded** by the harness — see the banner at the top of that file. |
| `INFRASTRUCTURE.md` | Linode server layout, deployment, secrets, backups. |
| `HANDOVER.md` | Running the live service: what is deployed, config, deploys, backups, logs, known limitations. The T016 deliverable. |
| `scenario-*.md` | Replay runbooks that produce the scenario test corpus. |
| `practical-plan.md`, `gui-review-plan.md` | Closed post-rewrite plans, kept as record. |
| `book-fidelity-plan.md` | Superseded by `practical-plan.md`; kept only because `EPA Book/critique.md` cites it. **Do not execute tasks from it.** |

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
