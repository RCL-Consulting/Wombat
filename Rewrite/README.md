# Wombat Rewrite — Plan Index

This folder is the canonical plan for rewriting Wombat on top of the same architecture as ClinicAssist.NET. It is written to survive multi-session agentic coding: any future session can bootstrap by reading `current_state.md`, then the task file it names, and resuming work without losing continuity.

## How to use this folder

1. Start every session by reading `current_state.md`. It names the active task, blockers, and the last verified commit.
2. Open the task file it points to under `Tasks/`. Each task file has its own Definition of Done, file list, and verification steps.
3. Work only on that task. When done, update `current_state.md` and check the box in `PLAN.md`.
4. If the task has to branch (unexpected work), add a new task file rather than mutating the old one — the plan is append-only so git history stays useful.

## Document map

| File | Purpose |
|---|---|
| `PLAN.md` | Master plan. Phases, task list, progress checkboxes. |
| `DOMAIN.md` | What EPAs, WBAs, STAR and the role hierarchy actually mean. Corrects misunderstandings in the current Wombat model. |
| `ARCHITECTURE.md` | Clean Architecture / CQRS layout, conventions, non-negotiables. |
| `DESIGN.md` | The canonical UI/design-system contract: tokens, layout grid, buttons, tables, forms, cards, dashboards, alerts, skeletons, icons, and the `app.css` section order. **Any task that writes Razor must read this first.** |
| `WORKFLOW.md` | Git branching, session handoff, verification protocol, agent prompts. |
| `INFRASTRUCTURE.md` | Linode server layout, deployment, secrets, backups. |
| `current_state.md` | Live state. Updated at the end of every session. |
| `Tasks/T0xx-*.md` | Individual task files. One file per unit of work. |

## Reference material (read-only)

- `../ClinicAssist.NET_ref_DO_NOT_COMMIT/` — the reference architecture to copy from. Treat as read-only. When in doubt about "how should X be structured", look there first.
- `../ClinicAssist.NET_ref_DO_NOT_COMMIT/SOURCE_MAP.md` — quick orientation for the reference project.
- `../SOURCE_MAP.md` — source map of the current Wombat code. Useful when porting domain logic; will be deleted once the rewrite lands.

## Scope discipline

This plan deliberately excludes:

- Any attempt to migrate data from the old Wombat. There are no real users, so there is no data to migrate.
- Any attempt to keep the old Wombat running alongside the new one. The old code is reference only.
- Any feature not present in the current Wombat, unless `DOMAIN.md` flags it as a correctness fix. New features are added *after* parity is reached.

If an agent session is tempted to do any of the above, it should stop and add a task file instead of silently expanding scope.
