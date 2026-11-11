---
id: T400
title: Flow 18, the operator keeps the platform running, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-11-11
started: 2026-11-11
---

# T400 — Flow 18, the operator keeps the platform running, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 18 is the eighteenth flow of the restructure (W-008, BRIEF § 8) and the last flow of
screens: the platform operator (devadmin) reads the Administrator's Home and the health page, creates and keeps
institutions, runs, pauses and reads the scheduled jobs, reads the audit trail and rebuilds progress; an institution's
administrator reads their own institution's audit trail. The health card is the operator's only warning that mail is not
configured (T157, T327).
**Surfaced:** 2026-11-11, after T398 (flow 17) closed; the operator said "flow 18 next".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T398 needed.

## Symptom

The operator's pages (`/`, the Administrator's Home; `/admin/institutions` (+`new`, `{Id:int}`), `/admin/jobs` (+`runs`),
`/admin/audit` (+`{Id:guid}`), `/admin/curriculum-progress`, `/admin/users/{UserId}`, `/placeholder/{Feature}`; also
`/health`) have not been designed for the restructure. Brief: `design/flows/18-platform-operations.md`, written before
flows 01–17; flows 01–17 changed the shell, the forms, the dialogs and the tables it inherits (flow 15 the rebuild page,
flow 12 the user page, flow 16 the 1150px stack), so the brief must be restated against what is there now. Known faults
it takes up: whatever the restated brief finds still open (T327, the System health stubs; the brief's Mode says the
System page must become a real health page or lose its nav item), and what the earlier replays left for this flow
(A.2.10 and A.6.1–A.6.2's known Gaps).

## Root cause

Not a defect with one cause: the pages predate the design system and the brief's requirements (§ Known problems).

## What to build

Run the loop as flow 17 ran it (`design/flows/17-activity-type-builder/`, BRIEF § 11 "Flow 16" and "Flow 17"):
1. **Restate the brief** in flow 17's layout, from read-only audits of the code and the captures; check the pages table
   against the code (the file each route renders, `@page`), including what flows 01–17 already changed; run
   `check_flow_completeness.py` and `check_verbatim_steps.py`. The brief says "He" of devadmin: use they/them.
2. **Stage** (`stage_upload.ps1 -Flow 18`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 18") on the Wombat design system (version 52), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** wireframes first (the brief's Mode), then fidelity; pick; a review of the boards against the code and
   the runbook; the operator's decisions; one correction round (its own canvas if over about 200 boards); check it
   item by item.
6. **Build** in waves (flow 17's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 17's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; sweep new tables 900–1100px against their container; after any width change, read every
   column of every list it reaches at 1280 for each role, and sweep 390–1440; re-take the flow's states. Start replay
   apps detached; act prompts forbid reading an earlier flow's `replay-pw.env`.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [ ] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link.
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/18-platform-operations/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 18) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 18, the baseline is re-captured, and
  flow 18's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T398 (flow 17), T394 (flow 16); T157, T302, T327; BRIEF § 4, § 6–8, § 11; INFRASTRUCTURE.md, HANDOVER.md.

## Notes

- **2026-11-11.** Filed and started on the operator's word ("yes, flow 18 next"); master pushed first (`70657d6c`).
- **2026-11-11, steps 3–4 (ahead of step 1, as T398).** Canvas "Wombat 18": https://claude.ai/artifact/FE7WSRNTmSp88bvS4dZRGV,
  created from the Design type, empty boards; design system v52 (1794377277-5687) installed: `tokens.json`,
  `components/bundle.css` and the three fonts copied from the system, `tokens.css` by hand (flow 17's, header updated;
  its 54 properties match app.css's `:root`, `--shadow-raised` resolved). The mark copied in as
  `/_blob/b3b9ce265597292edc272896bd70a693`. Canvas scratch:
  `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\5ae5486c-8726-4e89-ae35-c60429e28267\scratchpad\c18`.
- **2026-11-11, steps 1–2** (one Opus agent, `337a6675`): brief restated in flow 17's layout (they/them throughout);
  `check_flow_completeness.py` exit 0; `check_verbatim_steps.py` 0 problems for flow 18; `check_baseline_paths.py`
  missing 0; staged 30 captures (all opened: payloads `[REDACTED]`, no link, token or password), published here into the
  canvas as `project/ref/baseline/…` (version 4); `round-1-ask.txt` (636 lines). Gone: the System stub and its nav item,
  the Maintenance card, ShortCode's required text, T302's Deactivate, the institution's and Curriculum progress's failure
  words, nine jobs (eight since T379), 4.38's failed entries. Remain: T327, T157, T324, T325/T384, T326, T286, T277, T328,
  T264, T323, T329's kind, T378, T114, T274; new: the jobs page's result below the table, "dispatched." for a finished
  run, Run now not refusing a disabled job, the jobs table wrapping at 1280, Back links beside the trail, the audit
  entry's h3 with no h2 and its empty state for an unknown id; the Api host's `/health` checks no database (unfiled).
  BRIEF § 8's row no longer lists `/placeholder/{Feature}`. **Waiting:** the operator's Q1–Q5 (before round 1), then
  Q6–Q12 (brief § 6). The old frame's `states/` captures are marked "read their bodies only".

- **2026-11-11, Q1–Q5 taken as recommended ("yes"):** Q1 Home's health card holds everything, no System item (round 1
  draws both); Q2 T327's mail half folded in ("Not configured: mail is written to the log, not sent." a warning, nothing
  sent in 24 hours a failure; the Production startup refusal stays with T157); Q3 "All 8 jobs last succeeded." or the
  jobs named as failed, never run, disabled or overdue (overdue: no run within an hour of its scheduled time), linking to
  Scheduled jobs; Q4 every account but an erased one, locked said apart, one query; Q5 the institution record headed by
  its name, Status read out, Deactivate…/Reactivate… behind dialogs in place of the Active box, no Back link, a new list
  subtitle, T378's field-tied refusals. Next: the operator sends `round-1-ask.txt` and says "done".
- **2026-11-11, round 1** (31 boards, `f18r1.css` and `canvas.json` in `round-1/`, `243ae294`; the operator sent the ask
  and said "done"): A Home holds it all, B a System page, C needs you first (wireframes); the health lines; institutions,
  jobs and run history, the audit log and an entry at fidelity. **Assessed** (one Opus agent, `32efa559`): pick A, nothing
  from B or C; M1 Q4's count (25 counted the erased account), M2 Q5's Reactivate asks nothing and the subtitle's words,
  M3 Q2's mail sentence, M4 the Steps board (own account is 3 + 1; 14 of 23 Expects change, not 18); 11 shoulds, 14 nits;
  Q13–Q17. Two slips here: the ask went out listing Q1–Q5 as open (written before the decisions; not amended, flow 17's
  lesson again), and Q2 was put to the operator as "nothing sent in 24 hours" where the brief says a drop (Q13).
- **2026-11-11, Q13–Q17 taken as recommended ("yes"):** pick A, nothing from B or C; Q13 mail fails on a mail dropped in
  24 hours (the brief's rule; not "nothing sent"); Q14 no breakdown lines, Q4's one figure plus locked; Q15 audit names
  read live from the user id, never stored; Q16 Q5 kept, a task proposed for the College, speciality and sub-speciality
  records to follow it; Q17 one "Institution unavailable" for the Administrator and an institution's own admin (1.23's
  Expect changes). Round 2's ask: one Opus agent, every decision stated as settled.
- **2026-11-11, round 2's ask** (one Opus agent, `76caf6e6`): `round-2-ask.txt`, 942 lines; A and screens 3–5 at full
  fidelity, ~85 states / ~175 boards (R2-H Home 11, R2-I institutions 22, R2-J jobs 12, R2-R run history 11, R2-L audit
  log 15, R2-E entry 11; 390 twins, seven 768 twins); every decision stated as settled; M1–M4, S1–S11, N1–N14 and the
  brief's two corrections folded in; Steps: 14 of 23 Expects change, plus 1.23 (Q17). Provisional (PROPOSED): T1–T7 Q6–Q12
  at the brief's recommendations (T6: institutions, jobs, audit stack from 1150px, run history from 1024px), T8–T24 words.
  Inferred, not checked: three stored audit refusals, 1.1's account count drawn as 9. Waiting: the operator sends it.
- **2026-11-11, round 2** (173 boards, `f18r2.css` and `canvas.json` in `round-2/`, `8063c80b`; the operator sent the ask
  and said "Done"). **Reviewed** (three Sonnet agents: code `844b21e1`, cast `19abf3d7`, ds `ba2f2d3a`; merged here,
  `round-2-review.md`): M1 the Deactivate/Reactivate dialogs in ConfirmDialog's form mode, M2 the pager's order, M3 Q15's
  name read (an erased person falls back to the id; `DataRightsPeople.StaffNamesAsync` gives the pseudonym), M4 A.5.5
  also changes (Q17), M5 A.2.10's 390 count; K2–K12, C3–C7, S1–S10, 22 nits. **Waiting:** the operator's Q18 (mail
  health persisted, recommended) and T1–T24, then the round-3 correction ask.
- **2026-11-11, Q18 and T1–T24 taken as recommended ("yes"):** Q18 each mail's outcome (sent or dropped, its time, the
  refusal's words; no address or body) persisted in a small table pruned by the retention job, the 24 hours read from it,
  "Nothing sent since ‹time›" for the empty state; T1–T24 (Q6–Q12 and words) confirmed as drawn. Round 3's ask: one Opus
  agent.
- **2026-11-11, round 3's ask** (one Opus agent, `62f0c352`; `round-3-ask.txt`, 509 lines): 57 items (M1–M5, K2–K20,
  C3–C13, S1–S10, N1–N7, Q18, T1–T24, A1–A3); ~128 boards (120 redrawn, R3-H12 new for Q18's "Nothing sent since", the
  Spec, Steps and check). Wombat 18 holds 204 boards, so a new canvas: "Wombat 18 · Round 3"
  https://claude.ai/artifact/CWTWqELyB63LfwvP7cgyJV, created here with design system v52 (tokens.css by hand), the mark
  `/_blob/c6a099e64c4d721c4870011edee399ca` (filled into the ask), `project/ref/baseline/` and round 2's 171 boards as
  `project/r2/R2-….ref.html` with `f18r2.css`. Provisional (PROPOSED): Q18's details (an `EmailOutcome` table written by
  both hosts, 90 days kept by scheduled-job-run-retention, "Not read" for Mail when the database is down), and words and
  markup for K2–K17, S1, S4. Kabelo Sithole's role checked (README.md:139). Waiting: the operator sends it.
- **2026-11-11, round 3** (128 boards on "Wombat 18 · Round 3", `69f4f1e3`; the operator sent the ask and said "done")
  checked item by item (one Sonnet, `95a568be`, `round-3-check.md`): 57 rows, done 55, partly 2 (S2: R3-H01's notes say
  "narrow until 1115" where the Spec's measured band is 1100–1112, and Spec § 13 omits it; C5: seven new sentences lack a
  "replaces" cell), not done 0. No regression: the round-2 musts hold, all 128 boards present, 64 twins identical, f18r3.css
  differs only as the items say, Steps arithmetic right. Four loose citations (`DependencyInjection.cs:4-6`/`:16-25` are 90
  and 100; `CatalogueLinks.cs:38`; `app.css:6799`/`:6847`). Pixel figures not rendered. Waiting: the operator's decisions
  (no round 4, the fixes carried into the build; the round-3 PROPOSED settlements kept).
- **2026-11-11, round 3 decisions.** The operator said "yes to both": no round 4 (S2's band, C5's "replaces" cells and the
  four loose citations go into the build); the round-3 PROPOSED settlements kept (Q18's `EmailOutcome` table written by
  both hosts, 90 days, Mail "Not read" with the database down; the code names and words; Home's links in the card footers).
  Next: step 6, the build plan (one Opus agent).
