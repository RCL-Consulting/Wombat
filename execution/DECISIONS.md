# Decisions — wombat

Append-only. A decision is worth a row when reversing it would cost real work.
Record the rejected option and why: that reasoning is what rots first.

> **This is not the only decision register, and the split is deliberate.**
>
> | Register | Prefix | Holds |
> |---|---|---|
> | **this file** | `W-nnn` | **How the project is run** — tooling, workspace layout, process. |
> | `execution/knowledge/EPA-PROGRAMME.md` § 3 | `D1`–`D38` | **What the product does** — the CPSA v11.1 catalogue's domain decisions, cited by task files as `[T123] D1a`. |
>
> The prefixes differ because `D-001` beside `D38` is a trap: the same letter, two
> unrelated sequences, no way to tell from a citation which register you are in. A product
> decision goes in `EPA-PROGRAMME.md`; a decision about how we work goes here.

Status: `Accepted` | `Superseded by W-NNN` | `Reversed`

| ID | Date | Title | Status | Rationale, and what was rejected | Related |
|---|---|---|---|---|---|
| W-001 | 2026-09-20 | Adopt the rcl-harness execution workspace | Accepted | `CLAUDE.md` mandated reading a 296 KB / 3,574-line `current_state.md` at every session start — ~75k tokens before a line of code. That file was append-only with nothing bounding it, and one session added 340 lines to it. The harness bounds STATE at 60 lines and HANDOFF at 80, enforces both with `lint`, and blocks a Stop that changed task state without writing a handoff. Measured result: **301,455 → 7,924 bytes, 38× smaller.** *Rejected:* hand-trimming `current_state.md` into `Programme/log/` — same one-off gain, but nothing would stop it regrowing, which is exactly how it got to 3,574 lines. | `execution/log/_pre_harness_current_state.md`, `C:\dev\AGENTIC_STACK_PLAN.md` |
| W-002 | 2026-09-20 | Task lanes derived from PLAN/practical-plan/git, not from the `**Status:**` lines | Accepted | The status lines were unreliable: 26 files said open or in-progress, but 7 (T067, T068, T089, T091, T101, T112, T119) had shipping commits, and the 6 `T019-b…g` follow-ups carried no status at all while `PLAN.md` had them unchecked. Lanes were assigned from `PLAN.md` checkboxes, `practical-plan.md`'s progress table, and commit subjects beginning with the task id. *Rejected:* trusting the in-file status, which would have parked seven finished tasks in the queue. **These lanes are a first pass and want a spot-check;** `harness.py task done <id>` keeps them honest from here. | `execution/DASHBOARD.md` |
| W-003 | 2026-09-20 | v2 YAML frontmatter on all 115 task files, rather than the harness's v1 bullet format | Accepted | `harness.py` reads a v1 `- **Status:**` *list bullet*; Wombat's files carry a bare `**Status:**`, so every file warned. Frontmatter derives only what the file already says — id from the filename, title from the H1, status from the lane, priority from `**Severity:**`, model and created date where stated. *Rejected:* adding a `- ` prefix to 59 files, which would have satisfied a legacy reader while leaving 56 files with no status and `task done` unable to write them. | all of `execution/tasks/` |
| W-004 | 2026-09-20 | `Programme/` keeps the slow-changing contracts; only live state moved | **Superseded by W-005** | The doctrine files (DOMAIN, ARCHITECTURE, DESIGN, CUSTOMIZATION, INFRASTRUCTURE, HANDOVER, EPA-PROGRAMME, runbooks) are heavily cross-referenced, and 180 of those references had just been rewritten the same day for the `Rewrite/` → `Programme/` rename. *Rejected, for now:* folding them into `execution/knowledge/` and `execution/architecture/` — defensible, but it would churn every cross-reference twice in one day for no gain in session-start cost, which is what this work was for. Left as an open question in STATE. | `execution/knowledge/README.md` |
| W-005 | 2026-09-20 | Everything moves under `execution/`, in the harness's standard layout | Accepted | Reverses W-004 on the operator's call: *"I prefer the standard: `execution/knowledge/`"*. The doctrine now sits in `execution/architecture/` (ARCHITECTURE, DESIGN, CUSTOMIZATION, INFRASTRUCTURE — how the system is built) and `execution/knowledge/` (DOMAIN, EPA-PROGRAMME, HANDOVER, WORKFLOW, PLAN, runbooks, closed plans — what the project knows); the two June scratch files went to `log/`. `Programme/` is gone. **W-004's reasoning was about cost, not correctness** — it weighed a third path-churn in one day against a layout question, and that trade is the operator's to make, not mine to settle by deferring. The churn was 40 live references in 17 files, less than half the 180 the rename cost, because the harness had already absorbed the task register. *Rejected:* a flat `knowledge/` holding all 17 files — `architecture/` exists in the same standard and the build-versus-know split is real. | `execution/knowledge/README.md`, W-004 |
