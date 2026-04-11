# Agentic Workflow

This document describes how a coding agent should run a session against this plan. Humans running the plan by hand should also follow it — the discipline is what makes the plan resumable.

## Session protocol

A session is one continuous block of work by a single agent (or human). Sessions are expected to be short (an hour to a few hours) and are not expected to carry knowledge between them except through files.

### Start of session

1. Read `current_state.md`. It tells you:
   - Which task is active.
   - What was done in the previous session.
   - What commit (or branch) is the last known-good state.
   - Any blockers or open questions.
2. Open the task file named in `current_state.md`.
3. Skim `PLAN.md` just enough to see where this task sits in the larger dependency graph.
4. If the task references code in `../ClinicAssist.NET_ref_DO_NOT_COMMIT/`, open those files and **read them before writing anything**.
5. Check out the task's working branch (naming below). If it doesn't exist, create it from `main`.
6. Run `dotnet build` to confirm the baseline is green.
7. Announce your plan for the session in a single paragraph. Stick to it.

### During the session

- Do not wander into other tasks. If you find something that needs doing elsewhere, add it as a new task file in `Tasks/` with status `pending` and keep going.
- Commit often. Small commits, meaningful messages. Each commit should leave the build green when possible; squash at the end if you like.
- When the agent needs a decision it can't make (naming, ambiguous domain rule), write the question into `current_state.md` under "Open questions" and make the most reasonable choice; flag the TODO in code and in the task file.
- If the agent is stuck for more than ~15 minutes on the same error, stop and write what it tried into `current_state.md`. A fresh session will often unstick it.

### End of session

1. Run verification for the active task (see the task file's verification section).
2. Commit any uncommitted work. Push the branch.
3. Update `current_state.md`:
   - Mark the task as `in_progress` (if unfinished) or add a "handoff note" to the next session.
   - Note the last commit SHA.
   - List anything the next session needs to know.
4. If the task is complete:
   - Tick its box in `PLAN.md`.
   - Append a short entry to `current_state.md` history.
   - Name the next task in the "Active task" field.
5. Do not merge to `main` at the end of a session unless the task is complete and verified. Unfinished work stays on the branch.

## Branching

- `main` is always green: latest `dotnet build` and `dotnet test` passing.
- One branch per task: `task/T00X-short-slug` (e.g. `task/T007-assessment-aggregate`).
- Merge via fast-forward or squash; no merge commits on `main`.
- Branches are short-lived. If a branch is open for more than three sessions, something is wrong — split the task or escalate.
- The `Rewrite/` plan files are edited on `main` directly when updating `current_state.md` or ticking `PLAN.md`. Otherwise plan edits go on a `plan/*` branch.

## Commit messages

Format:

```
T00X: short imperative summary

Optional body explaining why, not what.
```

Examples:

```
T001: scaffold five projects with Directory.Build.props
T007: Assessment aggregate state machine
T015: systemd unit and Caddy site file for wombat.example
```

## Verification levels

Every task has a verification section. Three levels, pick the highest that applies:

- **Build** — `dotnet build` clean.
- **Test** — `dotnet test` green for the relevant test project(s).
- **Manual** — a running instance of the app and a scripted click-through. Used for UI tasks.

A task is not done until the highest applicable level passes. If a manual check is required, record the date it was done in `current_state.md`.

## Agent prompt template

When launching a subagent to execute a task, use this template:

> You are resuming work on the Wombat rewrite. Before doing anything else, read `C:\Users\Renier\Wombat\Rewrite\current_state.md`, then read the task file it names, then read `C:\Users\Renier\Wombat\Rewrite\ARCHITECTURE.md` and `C:\Users\Renier\Wombat\Rewrite\DOMAIN.md` to refresh conventions. Then, and only then, begin work on the active task. When you finish or get blocked, update `current_state.md` per `WORKFLOW.md` and stop. Do not exceed the scope of the active task.

That prompt is deliberately terse. The task file itself carries the detail.

## When a task goes wrong

- **Wrong approach discovered mid-task.** Commit what you have to the branch, revert on a new branch, and add a note to the task file's "Lessons" section. Do not force through a broken design.
- **Task turns out to depend on something earlier.** Mark the task `blocked`, update `current_state.md` with the dependency, and either work on the dependency (if it's small) or switch to it as the active task.
- **Task is bigger than one session.** Split it. Add `T00Xa`, `T00Xb`, etc. Original task file stays as an index.

## When the plan itself goes wrong

If `DOMAIN.md` or `ARCHITECTURE.md` is wrong about something and the rewrite can't proceed without correcting it, stop coding, fix the plan, commit the plan change with message `plan: correct X because Y`, and then resume. The plan is the source of truth, but the source of truth has to be true.

## Multi-session handoff checklist

Every session must leave the next session able to answer:

- [ ] What am I working on? (`current_state.md` → Active task)
- [ ] What did the last session do? (`current_state.md` → Last session notes)
- [ ] Is the build green? (last commit SHA; if unsure, just run `dotnet build`)
- [ ] What do I need to read before I start? (task file + ARCHITECTURE + DOMAIN)
- [ ] Is there anything the previous session gave up on? (Open questions / Blockers)

If any of those is unclear after reading `current_state.md`, the previous session failed the handoff. Fix the handoff before writing code.
