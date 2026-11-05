---
id: T394
title: Flow 16, the College keeps the national catalogue, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-11-04
started: 2026-11-04
---

# T394 — Flow 16, the College keeps the national catalogue, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 16 is the sixteenth flow of the restructure (W-008, BRIEF § 8): the College's administrator
keeps the specialities, sub-specialities, EPAs and curriculum versions, and the Administrator keeps the Colleges and the
entrustment scales. A target, its period or an EPA's pause is read live in every adopting institution, including closed
periods, so each page must say whose page it is and how far a change reaches before it is saved.
**Surfaced:** 2026-11-04, after T392 (flow 15) closed; the operator said "start the next flow".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T392 needed.

## Symptom

The catalogue pages as the College and the Administrator see them (21 templates: Colleges, specialities,
sub-specialities, entrustment scales, EPAs, curricula and a curriculum's items, `/access-denied`) have not been designed
for the restructure. Brief: `design/flows/16-college-catalogue.md`, written 2026-09-26, before flows 01–15; flow 15
rebuilt the EPA list, the curriculum list and the items page for an institution, so the brief must be restated against
what is there now. Known faults it takes up: T323's item-editor half (at 390px), T393 (CurriculumEdit and
EntrustmentScaleEdit print exception text), T291 (Back to colleges offered to the College), T324, T326, T331 (the
nav lights nothing on the College's specialities), and what T392's replay left unfiled for this flow: CurriculumEdit's
validator messages twice and unlinked, its 36–40px controls at 390px, no result after a create or a clone; the EPA
list's Create EPA under the long subtitle; three EPA list states on the old column widths (`epas-list--college`,
`--institution`, `--inactive`).

## Root cause

Not a defect with one cause: the pages predate the design system and the brief's requirements (§ Known problems).

## What to build

Run the loop as flow 15 ran it (`design/flows/15-institution-adopts-catalogue/`, BRIEF § 11 "Flow 14" and "Flow 15"):
1. **Restate the brief** in flow 15's layout, from read-only audits of the code and the captures; check the pages table
   against the code (the file each route renders, `@page`), including what flows 01–15 already changed; run
   `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 16`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 16") on the Wombat design system (version 45), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** fidelity in two parts (the brief's Mode); pick; a review of the boards against the code and the runbook;
   the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (flow 15's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 15's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; sweep new tables 900–1100px against their container; re-take the flow's states.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [ ] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link.
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/16-college-catalogue/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 16) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 16, the baseline is re-captured, and
  flow 16's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T392 (flow 15), T387 (flow 14); T323, T393, T291, T324, T326, T331; BRIEF § 6, § 8, § 11; CUSTOMIZATION.md.

## Notes

- **2026-11-04.** Filed and started on the operator's word ("start the next flow"). The flow 14 scratch DBs
  (`wombat_scenario_t387_s10`, `_s20`, `_s21`, `_s22`, `_s515`) were dropped first; the replays' end-state DBs
  (`wombat_scenario_t290`…`_t392`) kept.
- **2026-11-04, steps 3–4 (ahead of step 1, as T392).** Canvas "Wombat 16": https://claude.ai/artifact/AqaH447KL91WCTbjQFJnWf,
  created from the Design type, empty boards; design system v45 (1793670418-6412) installed: `tokens.json`,
  `components/bundle.css` and the three fonts copied from the system, and `tokens.css` by hand (flow 15's, header
  updated; its 54 properties match app.css's `:root`, `--shadow-raised` resolved). The mark copied in as
  `/_blob/ce366cc30297c5a90255d5d0a88fa6b5`. Steps 1–2 run in one Opus agent. Canvas scratch:
  `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\28969ed0-c979-487c-82e2-606d8331c3a3\scratchpad\c16`.
- **2026-11-04, steps 1–2** (one Opus agent, `999b7fc9`): brief restated in flow 15's layout (33 steps; pages table
  names every template's file); `check_flow_completeness.py` exit 0; `check_verbatim_steps.py` 0 problems for flow 16
  (it always exits 0); `check_baseline_paths.py` missing 0; staged 146 files (70 captures); `round-1-ask.txt` written,
  the mark's URL filled in here. Gone: T323's item editor, T331, T301's start and adoption refusal, T329's EPA-list
  skeleton, T272's CreateEpa, the empty "Items (0)" table. Remain: T291 item 2 (+ the Specialities redirect for a
  College admin with no College), T264, T324, T393, T272 (College handlers, part 1), T326 (Neonatology), T301's
  "Active" box, CurriculumEdit's doubled messages and missing result. Not measured: the 36–38px controls; Create EPA
  under the subtitle for the Administrator or at 768. **Waiting:** the operator's Q1 (before round 1), Q3–Q5, Q7–Q10,
  Q12, Q13 (brief § 6).
- **2026-11-04, decisions.** The operator took brief § 6's Q1–Q14 as recommended ("yes"): Q1 one Specialities page per
  College (round 1 draws it beside today's lists and a two-pane variant); Q3 recorded as **D56**, Q8 as **D57**
  (EPA-PROGRAMME § 3). The round-1 ask already builds on the recommendations; no change to it. **Next:** the operator
  sends `design/flows/16-college-catalogue/round-1-ask.txt` with its ATTACHED captures (staged in `design/upload/`) to
  the canvas's chat, and says "done"; then assess round 1 (one Opus agent, as T392's `round-1-assessment.md`).
- **2026-11-04, round 1** (50 boards and `f16r1.css` in `round-1/`, `b486bb38`; the operator sent the ask and said
  "done"): A1–A3 the tree as wireframes (the canvas recommends A2, one page per College), B1/B2 Colleges, C1/C2 scales,
  each at 1280 and 390, plus Spec and Steps. Assessment running (one Opus agent → `round-1-assessment.md`).
- **2026-11-04, round 1 assessed** (one Opus agent, `83a42356`): picks A2 (Q1), B1 (D56), C1 (Q11, 6.8) with C2's
  textarea; M1–M8 (D57, Q12, Q7, Q11, Q6 contradicted; B2's Reactivate; the Steps board's 6.7/1.15 and missed Expects;
  two "Edit Paediatrics"); 18 part-1 steps (the ask's 19 a miscount); no capture reached the canvas. Q15–Q17 for the
  operator. Q16 checked here (`UpdateEntrustmentScaleCommandHandler.cs:61-84` guards removals only): proposed as T395.
- **2026-11-04, picks.** The operator took A2, B1 and C1 (with C2's textarea) and Q15–Q17 as recommended ("yes"):
  Q15 no count in the deactivation dialog (D56's words); Q16 Up, Down and Remove offered only on a scale nothing uses,
  the handler refusing the same (T395, still proposed: the design draws it, the build may fold it in if the operator
  promotes it); Q17 Home's card keeps Q12's four links. **Next:** round 2's ask (part 1 corrected, part 2 at fidelity).
- **2026-11-04, round 2's ask** (one Opus agent, `c969d474`): `round-2-ask.txt`, 1,119 lines; part 1 (A2, B1, C1)
  corrected at fidelity, part 2 at fidelity with one variation (EV1 reach in the pause dialog only, EV2 also a standing
  line on a national EPA's page); all 70 of § 4's captures to attach, re-staged (`design/upload/baseline/`, 70 found).
  T1–T12 settled provisionally in the ask for round 2's review; T11 (a curriculum's sub-speciality editable after items
  or an adoption) drawn as built. **Next:** the operator sends it and says "done".
- **2026-11-04, round 2** (306 boards and `f16r2.css` in `round-2/`, `19f3eac5`; the operator sent the ask and said
  "done"; the canvas read all 70 captures from `design/upload/baseline/`, none as attachments). Three Sonnet reviews
  running (code, cast, design system → `round-2-review-{code,cast,ds}.md`); the merge into `round-2-review.md` follows.
- **2026-11-04, round 2 reviewed** (three Sonnet agents: code `d4f98c5c`, cast `2c9161ef`, ds `7bb27d42`; merged here,
  `6e7af73a`, `round-2-review.md`): M1–M5 checked against the boards and the code (Q6/Q16 order; the CNSA scale missing
  from four selects; F18's "Items (15)"; required marks; accessible names); the shoulds and nits carried. Q18–Q21 for the
  operator (the refusals' order, a curriculum's sub-speciality fixed once used, header actions in app.css, EV2).
- **2026-11-04, Q18–Q21** taken as recommended ("yes"): Q6's checks before Q16's on a removal; a curriculum's
  sub-speciality fixed once it has an item or an adoption (the command refuses); header actions fixed in app.css in
  this build; EV2. **Next:** round 3's ask.
- **2026-11-04, round 3's ask** (one Opus agent, `84f211f4`, `round-3-ask.txt`, 406 lines): 51 corrections (M1–M5,
  S1–S21, N1–N21, Q18–Q21) and T1–T8 on Steps; 99 boards redrawn at 1280 (+99 at 390, 11 at 768, 5 at 390 only); 10
  new (R3-C11/C13 on the CNSA scale as read at 6.6, R3-D20 Q19's refusal, four sweep-edge boards); EV1 dropped. Its own
  calls, for the check: Q19's read-out on every redrawn 11.1/11.2 page; F18's two counts (16 rows, the College's 15);
  the Illustration chip rule; stacking breakpoints (1024; the items 1100) recommended, not decided.
- **2026-11-04, round 3 drawn** on a second canvas (the operator: "it required a new canvas"): "Wombat 16 · Round 3"
  https://claude.ai/artifact/W1R56z3QGxfK2GPMEcp3c9. Saving to `round-3/` (one Sonnet agent).
- **2026-11-04, round 3 saved** (230 boards, `f16r3.css`, `ae5e007b`; design system installed on the second canvas,
  `tokens.css` present, its record's version null). The canvas reports page overflow 0 from 768 to 1280; the check
  (one Sonnet agent → `round-3-check.md`) judges the tables against their containers.
- **2026-11-04, round 3 checked** (one Sonnet agent, `e2977baa`, `round-3-check.md`): 59 rows, 55 done, 4 partly (S1 the
  sweep table proves no headroom; S15 and N1 words named not worded; T8 Steps cites two R2 boards), 0 not done; no
  regression; the check board overclaims M4's hidden "required" on rung labels and counts 40 captures for 39. Widths by
  arithmetic: the four tables fit above 1024 with 50–120 px spare, the items above 1100 with 50–100 px. Decisions 1–5
  put to the operator before step 6.
- **2026-11-04, round 3's decisions 1–5** taken as recommended ("yes"): (1) a browser sweep of the four tables at
  900–1100 px against their containers, in the build, before the replay; (2) the items' thin margin above 1100 accepted
  if (1) confirms it; (3) S15/N1's words and T8's stale references settled in the build plan, no round 4; (4) A22's
  fixture cut to the 2,000 cap; (5) Q19's refusal names its cause (it has items, or the adopting institution by name).
  **Next:** step 6.
- **2026-11-04, step 6 planned** (one Opus planner): plan and contracts on master `b0717eb2`; § 0 on `t394` `9f25092c`
  (worktree `.claude/worktrees/t394`, rebased onto the plan): CatalogueReach and its words, CurriculumDto's Reach,
  NationalItemCount, HasNationalEpas, SubSpecialityFixed, EpaDto.Reach, EntrustmentScaleUses and InUse, CatalogueLinks'
  flow 16 tokens, D57's policy and menu, Q20's ≥1024 half, the catalogue CSS; no migration; two RegisterInvitationTests
  that read the clock (red on master since 2026-11-03) given a fixed clock. Suites 836/4347/1035/52/4533/520 green.
  Lanes A tree, B admin (Colleges, scales, Q18, T395's guard), C curriculum, D EPAs and items, W runbook; the sweep is
  the integrator's (`sweep.md`). P1–P3 put to the operator.
- **2026-11-05, P1–P3 approved** ("yes"): Q20's ≥1024 half only; "in use" is the delete's seven uses (adding a rung and
  editing a label stay allowed); no reach line for an institution's own EPA. The wave runs: lanes A–D, each an Opus
  agent in `.claude/worktrees/t394-<lane>` cut from `t394` at `9f25092c`.
- **2026-11-05, lane D done** (`t394-items`: `a81feb4c`, `c51d296d`; suites 836/4353/1035/52/4559/520): adds three
  init members to § 0's `EpaDto.cs` (EpaListDto.CollegeName, EpaPageDto.CollegeName, ReadsEveryCollege), to accept at
  the merge; EV2's line wherever its rule holds (also behind the pause dialog and on the Administrator's page); the
  required line and aria-required reach flow 15's institution forms too (one component); R3-F14's picker after Q10
  differs from the board; no College subtitle while loading or after a failed read. For the review.
- **2026-11-05, lane C done** (`t394-curriculum`: `1d795130`; suites 836/4362/1035/52/4556/521): Q19 refused in
  `UpdateCurriculumCommand` before any assignment (`CurriculumSubSpecialityFixedException`, causes HoldsItems first,
  then Adopted with the adopters named, superseded included); a duplicate only on a real unique violation; the clone's
  refusal key is "Version" (contracts said "NewVersion"); the cloned result read back from the source. Merge risk: its
  tests build lane A's and B's DTOs positionally. Words on no board listed in its report (for the review).
- **2026-11-05, lane A done** (`t394-tree`: `69f24e97`, `703a1537`; suites 836/4364/1035/52/4590/523): GetCollegeTreeQuery;
  the default ladder at create; name refusals pre-checked in all four commands (a speciality no longer calls every
  refused save a duplicate); the old sub-specialities address redirects to the tree's section; Home's four links.
  Departs from the boards: an Unavailable page's last crumb is its h1; the Status help is `p.page-subtitle`
  (FieldGroupTests). `PageTitleTests`' exemption note for MySpecialitiesRedirect left stale. For the review.
- **2026-11-05, lane B done** (`t394-admin`: `fc3158d4`, `59c175b4`; suites 836/4364/1035/52/4564/520): College
  refusals name the holder; the scale save refuses in Q18's order (name, unknown ids, Q6 pinned items by label, ended
  records, decisions, then Q16 in use), checked by level id; **T395's guard** proved by
  `EntrustmentScaleReorderGuardTests` (9 tests); the delete refusal names its sub-speciality holders. Mended outside
  its list: one `EndedCurriculumListTests` assertion.
- **2026-11-05, the lanes merged into `t394`** (`54c25bc8` A, `d4c75428` B, `1091aa85` C, `bb251ebd` D): only DESIGN.md's
  section-order lane lines conflicted (each lane filled its own; resolved by taking the filled line). Suites running.
- **2026-11-05, suites on the merge and the sweep.** On `t394`: build 0 errors; 836/4402/1035/52/4670/524 green, after
  fixing a flaky lane A test (`ed7635a4`: `NotContain("99")` met ConfirmDialog's `confirm-<Guid>` ids, 2 of 3 runs).
  **The sweep** (`sweep.md`, `0e6ff53a`): `t394` on :5182 against `wombat_scenario_t394_sweep` (restored from
  `scenario-t392-post-actA`); 8 pages and callers at 900–1100 px (items to 1150); first pass the Administrator's
  Curricula 756/717 px at 1025 and 742 at 1050, so `.curricula-page` moved to the 1100px stack; second pass 0 fails;
  decision 2 accepted (spare 0). Not measured: Colleges' heavy five (R3-B16). `scenario-replay.ps1 stop` again missed
  the app (stopped by id after reading its command line). Lane W running (`t394-runbook`, cut at `ed7635a4`).
- **2026-11-05, lane W merged** (`t394-runbook` `2e29f5df`, merged `770c0b27`; Web 4670): steps 1.2–1.4, 1.8,
  1.12–1.18, 6.1–6.8, 6.10–6.14, 6.17, 6.23, 6.25–6.26, 6.28–6.31, A.7.10; F-1.12a, F-6.1a, F-6.8a, F-6.11a, F-6.13a
  closed; Q6's rung refusal listed as unreachable from the UI; verbatim 0 problems, completeness exit 0. Its baseline
  check's "4 missing" (and 3164 in a worktree) is because the captures are not in a worktree: rerun on master after
  the squash (master: missing 0). Two Sonnet reviews running (code; pages and runbook).
- **2026-11-05, the build reviewed** (code `179d8bfc`: 0 must, 4 should, 8 nit; pages and runbook `60b06f21`: 1 must, 5
  should, 11 nit, ~38 words on no board; merged `build-review.md` on t394): M1 checked here (A.7.10's "1 registrar"
  on 11.2, but Dr Ndlovu's erasure at A.1.12 ends the only one: the runbook is wrong, the build right). F1 (the fix
  pass's scope; S-1's check-then-save race to its own task) and F2 (the words) put to the operator.
- **2026-11-05, F1 and F2 taken** ("yes"): the fix pass (one Opus agent on `t394`) takes M1, should-fixes 1–8 and every
  nit; the words on no board accepted; S-1 proposed as **T396** (the guards' check-then-save race).
- **2026-11-05, fix pass done** (one Opus agent; `2c9d70ea`..`624333f4` on t394; suites 836/4411/1035/52/4675/528;
  verbatim 0 problems, completeness ok): M1, S1–S8 and the nits but N2, N7, N11 (no change asked), N9 (T390's) and
  N-8's second half (the @using is used). F2: only "..." → "…" and "its registrar stays on it" (one registrar).
  **New defect** found by its PostgreSQL tests: reordering an unused scale's rungs fails (EF circular dependency on the
  unique (scale, order) index), reported as on master too: proposed as **T397**; the operator's call whether it is fixed
  before the squash.
