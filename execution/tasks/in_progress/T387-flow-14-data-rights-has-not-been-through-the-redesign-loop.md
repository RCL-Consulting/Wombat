---
id: T387
title: Flow 14, data rights, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-10-30
started: 2026-10-30
---

# T387 — Flow 14, data rights, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 14 is the fourteenth flow of the restructure (W-008, BRIEF § 8): a person makes a data-rights
request (export, access, correction, erasure) and staff decide it. These are legal rights (POPIA) and an erasure cannot
be undone, so each page must say exactly what happened and what will.
**Surfaced:** 2026-10-30, after T379 (flow 13) closed; the operator said "start flow 14".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T379 needed.

## Symptom

The data-rights pages (My data rights, the Data rights requests queue, a request's page) and the erasure's aftermath
have not been designed for the restructure. Brief: `design/flows/14-data-rights.md`, written 2026-09-26, before flows
01–13. Known faults it takes up include T316 (an approved rectification can never be applied or completed), T319 (a
decision notifies nobody; a refused requester never sees the note), T320's digest help, T318 (an opt-out nothing
reads), T324 (enum names), T264 (an erasure approved without a confirmation), T323, T328, T325 and T329's rule; the
audits in step 1 list the rest, including what flows 01–13 already changed.

## What to build

Run the loop as flow 13 ran it (`design/flows/13-graduation-and-exit/`, BRIEF § 11 "Flow 12" and "Flow 13"):
1. **Restate the brief** in flow 13's layout, from read-only audits of the code and the captures; check the pages table
   against the code (the file each route renders, `@page`), including what flows 01–13 already changed; run
   `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 14`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 14") on the Wombat design system (version 37), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** structure where the brief says wireframe first, fidelity otherwise; pick; fidelity; a review of the boards
   against the code and the runbook; the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (flow 13's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 13's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; re-take the flow's states on scratch copies.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [x] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit. (`2bc97e30`; exit 0)
- [x] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link. (https://claude.ai/artifact/FrvjJMZLwRd2Y3AMDEJeGs)
- [x] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/14-data-rights/`. (round 3 final, `round-3-check.md`; decisions in the brief's § 6, `round-1-assessment.md` § 10 and `round-2-review.md`)
- [x] Built, with all six suites green and DESIGN.md amended (its banner lists flow 14) — the squash commit and counts. (`e8eea1d2`: Domain 836, Application 4279, Infrastructure 1035, Architecture 52, Web 4369, Integration 516)
- [ ] The whole runbook replays on a fresh database with no regression from flow 14, the baseline is re-captured, and
  flow 14's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T379 (flow 13), T376 (flow 12), T316, T319, T320, T318, T324, T264, T323, T328, T325, T329, T276, T258, T112;
BRIEF § 6, § 8, § 11; CUSTOMIZATION.md § 5.

## Notes
- **2026-10-30, steps 3–4 (ahead of step 1's commit).** Canvas "Wombat 14":
  https://claude.ai/artifact/FrvjJMZLwRd2Y3AMDEJeGs, created from the Design type, empty boards; design system v37
  (1793144981-e3ba) installed: `tokens.json`, `components/bundle.css`, the three fonts copied from the system, and
  `tokens.css` by hand (flow 13's, header updated; its 54 properties match app.css's `:root`, `--shadow-raised`
  resolved). The mark copied in as `/_blob/616538c681cf4475edf7c466864e408d`. Steps 1–2 run in one Opus agent.
- **2026-10-30, steps 1–2** (one Opus agent, `2bc97e30`; checked here: `DataRightsAuthorization.cs:29,32`
  ReviewRoles = Coordinator, RectificationRoles = SpecialityAdmin; `/admin/trainees/edit` is
  `AdministratorOrInstitutionalAdmin`, so the approving Coordinator cannot open the profile): restated in flow 13's
  layout; Mode F/W (wireframes first for My data rights and for a request's page with its correction; fidelity for the
  queue and the decision with the erasure dialog). Dropped as done: T239, T240, T234, T279, T258, the monospace
  textareas; content re-based on T379's replay (`deleted_user_0ae1d64b`). § 5: T316, T319, T318, T320's help
  sentence, T324, T264, T323, T328, T325, T329, T280, T276's request half, T112's wording. § 6: Q1–Q16 with
  recommendations. New, unfiled: UnderReview never set; the domain lets a decided request be withdrawn; a failed
  export PDF is silently dropped from the zip; refusals at the page top; Back to list repeats the trail; (inferred) an
  erased account in the Administrator's users list with an empty name. Completeness exit 0; verbatim 424/0; baseline
  missing 0. Staged 113 files, none to crop. `round-1-ask.txt` carries the mark's URL. **Next:** the operator's
  answers to Q1–Q16, then round 1.
- **2026-10-30, round 1** (18 boards in `round-1/`, `42366e44`; the operator sent the ask and said "done"; the boards
  follow Q1–Q16's recommendations, taken as recommended, to confirm): three structures for My data rights (A one page,
  B a form page, C cards), three for a request and its correction (A in its section, B a form page, C a status card),
  Queue V1/V2 and Decide V1/V2 at fidelity, NotFound-Kept, Spec, Steps. Assessed in `round-1-assessment.md` (one Opus agent, `2881b938`; checked here: `CanAct` never refuses the requester
  deciding their own request; `ApplyRectificationCommand` takes TargetType, a Guid and JSON values): picks My data
  rights A, Request A (with B's fixed list of records), Queue V1, Decide V1; the kept request confirmed; not found
  redrawn. 6 must-fixes (M1 not found as the request's own Unavailable page; M2 the digest help names mail nobody sends;
  M3 Q5's notice undrawn and AccountNotices' "by whom"; M4 gendered words; M5 the correction's record as free text, no
  recorder stored, no page changes an email; M6 the Steps board misses A.1.8a, A.2.4/A.2.6, the outcome). Q17–Q28 for
  the operator. No sign the captures arrived: re-attach § 4's twelve for round 2.
- **2026-10-30, the operator took Q1–Q28 as recommended** ("as recommended") and the round-1 picks. Round 2's ask
  (`round-2-ask.txt`) and the decisions written into the brief's § 6: one Opus agent.
- **2026-10-30, round 2's ask** (`round-2-ask.txt`, 509 lines, ~137 boards, 9 checks + 25 corrections M1–M6,
  S1–S12, N1–N7; decisions in the brief's § 6 and the assessment's § 10; one Opus agent, `61a92b31`). Checked here:
  `TargetType` is a free string (≤200) with no list in the code, and `TargetId` is a Guid while `TraineeProfile.Id` is an
  int, so the fixed list of records (Q26) needs the build to change the target's shape. NEW words are listed in the
  ask's NEW block for the round-2 review. A.1.8a's reason, note and times are brackets. **Next:** the operator sends it
  with the twelve act-A captures.
- **2026-10-30, round 2** (138 boards in `round-2/`, `a9904964`): reviewed by three Sonnet agents
  (code 2/15/8, cast 2/5/5, ds 3/6/8; checklist 7/9, corrections 23/25), the two linked stylesheets saved as
  `round-2/f14r2.css` and `f14r2-canvas.css`, merged by one Opus agent (`round-2-review.md`, `e1227d44`: 6 must,
  24 should, 18 nits). Checked here: `ErasureExecutor.cs:425-427` rewrites no data-rights request. F1–F10 for the
  operator.
- **2026-10-30, the operator took F1–F10 as recommended.** F6 and F7's follow-ups proposed: **T388** (a name set on
  My account reverts at the next institutional sign-in) and **T389** (a locked trainee is still emailed by the
  entrustment expiry job; narrower than the review's wording: the opt-out is rightly not honoured there). Round 3's
  ask: one Opus agent.
- **2026-10-30, round 3's ask** (`round-3-ask.txt`, 459 lines, ~148 boards, a 61-item checklist; `f14r3.css` written
  out in the ask for the canvas to save as `project/f14r3.css`; F1–F10 recorded in `round-2-review.md` and the brief's
  § 6; one Opus agent, `0c6f3882`). New states C11, D20, D21, D22, K02. Checked against the T379 database: two
  Administrators (admin@wombat.local, devadmin), so the last-Administrator refusal is an illustration; Mahlangu's
  profile id 5; the annual review #5. Inferred, to confirm: F6's radios disabled; whether Complete request sends a mail.
- **2026-10-30, round 3** (149 boards and `f14r3.css` in `round-3/`, `d47ab4ea`): checked item by item
  (`round-3-check.md`, one Sonnet agent, `10d92875`): 61 rows, 59 done, 2 partly (M1 C08/C09/C11 carry no
  before/after-save chip; A4 the render report as bullets, not a table), 0 not done, 0 regressed. The Spec reports 408
  controls at 390 none under 44 px and no sideways scroll at any width (not re-measured here). Unlisted: `h2#yr-h` lost
  its tabindex on the M boards; the build keeps it if the pager or a result focuses that heading. **Final. Steps 3–5
  done. Next: step 6, the build, on the operator's word.**
- **2026-10-30, step 6, the plan and § 0** (one Opus planner, `t387` from `c47bc6e0`, worktree
  `.claude/worktrees/agent-a0cbe10cd07555864`): `a9f28c08` boards-text, `eb4bded8` § 0, `7a155e37` build-lanes and
  contracts. Six suites green in Release (Domain 836, Application 4214, Infrastructure 1035, Architecture 52, Web
  4262, Integration 512). § 0: Q1/Q21 in `DataRightsAuthorization`, UnderReview removed, migration
  `T387_DataRightsFlow14` (read here: drops TargetId and OptOutOfOptionalProcessing, AppliedOn required,
  AppliedByUserId added, Status 1→0; `xmin` mapped with no DDL, as T121), the page DTOs, mails, F3's rewrite. Lanes D
  (decide and erase), K (corrections and reads), M (My data rights), P (the staff pages), then W (runbook), two
  Sonnet reviews, one fix pass. Ten PROPOSED items for the operator.
- **2026-10-30, step 6, the wave.** The operator approved the ten PROPOSED items and the wave ("yes"); recorded at the
  top of `build-lanes.md` (`5f18d3f4` on `t387`). Lanes D (`t387-decide`), K (`t387-reads`), M (`t387-mine`) and P
  (`t387-staff`), each in `.claude/worktrees/<branch>` from `5f18d3f4`, one Opus agent each, in parallel. Then the merge,
  lane W (runbook), two Sonnet reviews, one fix pass.
- **2026-10-30, step 6, the lanes** (all six suites green in each): D `71edf520`, `5cc523fb` (Application +28,
  Integration +3; the caller's scope checked before Withdrawn, kept: Withdrawn first would tell an out-of-scope
  Coordinator a foreign request's state, T056); K `fe3ca854` (Application +33; Apply does not itself refuse the name
  records for an institutional sign-in, only the page disables them; a correction recorded while Complete runs in
  another tab is not caught by xmin); M `5f616806` (Web +32; no browser check); P `d6324690` (Web +57; the queue stacks
  from 1100 px; no browser check). Merged into `t387` (`54ce6df2`): two conflicts resolved (DataTable's `Class` kept
  once with P's `ExtraClasses`; DESIGN.md's section-order lines, one per lane). Release build 0 errors; six suites green
  on the merge (Domain 836, Application 4275, Infrastructure 1035, Architecture 52, Web 4351, Integration 515). Lane W
  done (`17972410`, `4822d46f`, `374b55c9`: every changed step, new A.1.8a, the outcome at 5 requests and 11 stub mails,
  coverage, 42 states rows "Not yet captured (T387)"; verbatim 425/0, completeness ok, baseline missing 0, Web 4351),
  merged `4f9f94b6`. Code review (`build-review-code.md`, `71190f07`, Sonnet): 0 must, 3 should, 8 nits; checked here,
  S1: `ErasureExecutor.cs:377-402` leaves the request's Reason, DecisionNote and the corrections' values. **The operator
  decided S1** (2026-10-30): the erasure scrubs the person's own text (the reason, every correction's before/after
  values) to "Erased with the person's data on ‹day›."; the staff's decision note is kept. Pages and runbook reviews
  (Sonnet, `4cf8c56f`): pages 1 must (P1 DESIGN.md's banner), 4 should, 6 nits; runbook 0 must, 4 should, 5 nits
  (A.1.8a plays in order; the outcome's counts hold). One Opus fix pass (`3b9b1644`, `61b8304c`, `250c647f`,
  `c3693cc5`; `build-review.md` merges the three): every must and should fixed (S1 as decided; S2 Apply carries xmin;
  S3 eleven page tests; P1/P3 DESIGN.md's banner and amendments; P2 the queue reloads in place; P4 the note's counter
  and length refusal; P5 ConfirmDialog's action mode opens on Cancel, app-wide; R1–R4 the recipes); nits left with
  reasons in `build-review.md`. Six suites green (Domain 836, Application 4279, Infrastructure 1035, Architecture 52,
  Web 4369, Integration 516); verbatim 425/0; completeness ok. **Squashed onto master `e8eea1d2`** (tree identical to
  `t387` outside the task files). For step 7's browser checks: the dialog's focus on Cancel with a long forecast; the
  page-size select keeping focus; the note's counter; no rule under a stacked row header at ≤900 px; the new states'
  SQL recipes (unrun). Branches `t387*` and their worktrees kept. **Next:** step 7, the replay, on the operator's word.
