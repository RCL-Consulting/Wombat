---
id: T379
title: Flow 13, graduation and exit, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-10-27
started: 2026-10-27
---

# T379 — Flow 13, graduation and exit, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 13 is the thirteenth flow of the restructure (W-008, BRIEF § 8): a registrar's programme ends
(completion or withdrawal), the portfolio PDF is exported and verified, and the former trainee keeps their record. The
PDF and the STAR certificates are the record that leaves Wombat (T310), so it has to be right, reachable and verifiable.
**Surfaced:** 2026-10-27, after T376 (flow 12) closed; the operator said "push and lets start flow 13".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T376 needed.

## Symptom

The pages that end a programme and serve the record afterwards (Export portfolio and the staff export, Verify, Trainees'
completion and withdrawal, the user page's no-role state, the former trainee's Home and menu, My progress's ended
record, My authorisations and the other pages a graduate keeps) have not been designed for the restructure. Brief:
`design/flows/13-graduation-and-exit.md`, written 2026-09-26, before flows 01–12 and before D54/D55 (T290: a withdrawn
registrar holds no role either). Known faults it takes up include T311 (a graduate's Home, menu and STAR certificates),
T312 (a completed record rewritten by a later catalogue change), T313 (Verify's case-sensitive hash and 90-day window),
T314 (the staff export names no trainee), T287's verify item (the placeholder names a hash the footer does not print) and T305 (no correction path for an archived
record); the audits in step 1 list the rest, including what flows 05, 06, 11, 12, D54 and D55 already changed.

## What to build

Run the loop as flow 12 ran it (`design/flows/12-people-admin/`, BRIEF § 11 "Flow 11" and "Flow 12"):
1. **Restate the brief** in flow 12's layout, from read-only audits of the code and the captures; check the pages table
   against the code (the file each route renders, `@page`: BRIEF § 11 "Flow 12"), including what flows 01–12, D54 and
   D55 already changed; run `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 13`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 13") on the Wombat design system (version 33), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** structure where the brief says wireframe first, fidelity otherwise; pick; fidelity; a review of the boards
   against the code and the runbook; the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (flow 12's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 12's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; re-take the flow's states on scratch copies.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [x] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit. (this session's steps 1–2 commit; exit 0)
- [x] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link. (https://claude.ai/artifact/SujH2iB2snNe7VANE4SxdK)
- [x] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/13-graduation-and-exit/`. (round 3 final, `round-3-check.md`; decisions in the brief's § 6, `round-1-assessment.md` § 10 and `round-2-review.md`)
- [x] Built, with all six suites green and DESIGN.md amended (its banner lists flow 13) — the squash commit and counts. (`5cc10c66`: Domain 836, Application 4201, Infrastructure 1035, Architecture 52, Web 4259, Integration 512)
- [x] The whole runbook replays on a fresh database with no regression from flow 13, the baseline is re-captured, and
  flow 13's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines. (acts `95fa4119`–`8ef4b774`: 331 steps, 286 clean, no regression; states 64 rows `1f64aa17`, `0881302a`; missing 0)
- [ ] The design system is re-synced and republished — its version number.

## Related

T376 (flow 12), T373 (flow 11), T311, T312, T313, T314, T287, T305, T310, T290, T252, D54, D55; BRIEF § 6, § 8, § 11.

## Notes
- **2026-10-27, steps 3–4 (ahead of step 1's commit).** Canvas "Wombat 13":
  https://claude.ai/artifact/SujH2iB2snNe7VANE4SxdK, created from the Design type, empty boards; design system v33
  (1793082623-7a3a) installed: `tokens.json`, `components/bundle.css`, the three fonts copied from the system, and
  `tokens.css` by hand (flow 12's, header updated; its 54 properties match app.css's `:root`, `--shadow-raised` resolved).
  The mark copied in as `/_blob/f4e05985da677926678d5d52de67cf4f`. Steps 1–2 run in one Opus agent.
- **2026-10-27, steps 1–2** (one Opus agent, then checked here: MyAuthorisations is `Roles = "Trainee"`, the graduation
  mail claims a ratification): restated in flow 12's layout at `82b108a6`; Mode W/F (wireframes first for her Home and
  menu and for the staff export's entry points; fidelity in 2–3 variations for Export, Verify and the end of a
  programme; one proposal each for the closed list and the ended record pages). Dropped as done: T305 (its correction
  path is Q14), D54/T290 (the old "du Plessis keeps Trainee"), D55, T281, T252, T297, T303, T238; T310 moves to flow
  20. § 5: T311, T314, T313 with T287's verify item, T312, T325, T329, T280, T323, T166's gate, and four new defects
  (the closed rows' Edit, the profile's end inside Save's form, the unlinked My activities and Export, the staff
  export's entry points). § 6: Q1–Q16 with recommendations. Completeness exit 0; verbatim 424/0 (128 stale pointers in
  flows 01–07 re-synced); baseline missing 0. Staged: `design/upload/`, 122 files, none to crop. Inferred, unreplayed:
  `TraineeDashboard`'s ended branch is unreachable since D54. **Next:** the operator's answers to Q1–Q16, then round 1.
- **2026-10-27, the operator took Q1–Q16 as recommended** (recorded in the brief's § 6). So T312 lands before the build
  (Q10), as T288/T289/T333 did for flow 12; the build folds in T311, T314, T313 with T287's verify item (the cleanup
  job removed, A.2.1 and A.2.9 changed), the graduation mail's wording (Q12) and these pages' halves of T325, T329,
  T280 and T323; T310 goes to flow 20; T374 stays out. **Next:** the operator sends `round-1-ask.txt` to the canvas
  with § 4's key screenshots.
- **2026-10-27, round 1** (canvas 1793087531-266b, 27 boards in `round-1/`; assessed in `round-1-assessment.md`, one
  Opus agent, `5e13f015`): picks S1-A (one record card; `Main.dc.html` is its 1280 board), S2-A without the user-page
  link, Export V1, Verify V1, the end of a programme V1; S6 and S7 confirmed with corrections. 7 must-fixes (M1 Q2's
  menu undrawn; M2 S2-A's user-page link against Q4; M3 the profile's h1 "‹name›'s programme"; M4 S7's links belong
  under My progress's notice; M5 the Steps board misses 5.18, A.2.1, A.2.9, 5.8, A.1.6, A.4.6; M6 PAED-010/012 are three
  a semester, checked in the seed; M7 du Plessis's last decision is Satisfactory with Observations, checked in Act 4).
  Q17–Q27 for the operator. No board says whether the attachments arrived: re-attach § 4's twelve for round 2.
- **2026-10-27, round 2's ask** (`round-2-ask.txt`, 393 lines, ~165 boards; one Opus agent): the picks at fidelity on
  v33, every state at 1280 and 390 (768 for the tables), every § 7 item keyed to its boards, Q1–Q27 as decided, T312
  first, flow 12's round-2 faults listed. Both full hashes and the last export's time (22:52 SAST) checked against the
  replay's `PortfolioExports` rows. NEW words of the ask's own (E12, E15, P06, P15, Mbatha's export trail) are marked
  for the round-2 review. **Next:** the operator sends it with § 4's twelve key screenshots.
- **2026-10-27, round 2** (168 boards in `round-2/`, `14f88413`; the canvas could not open the captures, so it
  bracketed what only they hold): reviewed from the code, the cast and the design system, merged (`round-2-review.md`,
  `c8a61ccb`: 5 must, 24 should, 16 nits; K1/C2, K2, K4 and D2 checked here: `MayReadAsync` admits an Administrator,
  both end commands are `IRequest`, `GetExportSubjectQuery` does not exist, DESIGN.md 827–832 is the built refusal
  that Q7 changes). **The operator took F1–F11 as recommended**; T312 grows to cover R05, R06 and P18 (F3).
  **Next:** `round-3-ask.txt`.
- **2026-10-27, round 3's ask** (`round-3-ask.txt`, 347 lines, ~192 boards, a 73-item checklist; one Opus agent): every
  round-2 board redrawn as R3- with f13r3.css, 12 new states (X09, X10, E19, E20, P20–P25, C06, C07), brackets filled
  from the cast review. NEW words for the operator to see on the boards: the not-a-hash refusals (S10), "Choose the
  last day." (S3), the Assessor sentence (S5), no STAR in force (S8), "will stop counting" (M2). F9 drops Q6's h2 on
  Verify's answer (the result region). Captures 5.26-2 and 5.8-3 copied into `design/upload/baseline/act-5/`.
  **Next:** the operator sends it with the captures.
- **2026-10-27, round 3** (192 boards and `f13r3.css` in `round-3/`, `10f32323`; the captures did not reach the canvas
  in any round, so brackets stay only where a capture alone holds the value): checked item by item (`round-3-check.md`,
  one Sonnet agent): 73 rows, 70 done, 3 partly (S17 eleven seeded EPA titles left bracketed on R08; T10 A.1.6 and
  A.4.6 share one Steps clause; A4 17 px at 901 on R03/R06 from flow 05's `.details-list`, **T380 proposed**), 0 not
  done, 0 regressed. **Final.** Build notes in the check: the capture-bound brackets (the build reads the real values),
  GetExportSubjectQuery's null ProfileId case, the coverage.md rows, which f13r3.css rules move to app.css. **Steps 3–5
  done. Next: step 6, the build, after T312 (grown by F3).**
- **2026-10-27, step 6, the plan and § 0** (one Opus planner, `t379` from `3443d894`, § 0 `08a024dd`: `build-lanes.md`,
  `build-contracts.md`, `round-3/boards-text.md`; the end commands return `ProgrammeEndResult(TakenBack)`; shared
  readers, `MayExport` on three DTOs, `NavItems.PersonalFor`, `BadgeFor.ProgrammeOutcome`, the `.details-list` fix that
  closes T380's cause; migration `T379_PortfolioExportCleanupRetired`, data only). Six suites green in Release (Domain
  835, Application 4144, Infrastructure 1034, Architecture 52, Web 4158, Integration 511). The planner found master did
  not build in Release (T312's named tuple literals, CS8123): fixed on master, `232e7701`. **The operator approved the
  PROPOSED words as drawn and the wave** ("yes"): lanes R (record), E (export), V (verify), P (end) in parallel, then
  the runbook lane, three Sonnet reviews, one fix pass.
- **2026-10-27, step 6, built** (`5cc10c66`, squashed from `t379`, tree identical). Lanes R `544d5f97`, E `51fd84de`,
  V `4d378576`, P (four commits to `4b1c48bb`); merged with one fix (a duplicate test constant); § 6's NEW words filled
  from the lanes' reports; the runbook lane (`6ca1b6cd`, `95d3f765`: 5.8–5.28, 2.30, 6.34, 6.36, A.1.6, A.1.14, A.2.1,
  A.2.9, A.4.6, A.7.4, A.7.9, A.7.12; eight gaps closed; coverage and ~50 states rows); three Sonnet reviews
  (`build-review-{code,pages,runbook}.md`, merged in `build-review.md`: 0 must, 10 should, 21 nits); one fix pass
  (`a3337d26`, `cedbb8ea`, `a1ce056e`: every S fixed; P1 "Your record:" under the notice via `EndedRecord.AfterNotice`,
  P2 Spec 3's empty words kept, P4 the profile form refuses under each field). Left with reasons in `build-review.md`.
  For step 7: the browser checks of P1's position and P4's refusals; 5.13's "Exported 3 times" and SAST times; T312's
  6.21, 6.40, 6.41; the derived values on R02/R04/R07; Users' and the closed list at 901–1100 px. Branches `t379*`,
  `t312` and their worktrees kept. **Next:** step 7, the replay, on the operator's word.
- **2026-10-27/28, step 7, the acts** (fresh `wombat_scenario_t379`; D = 2026-10-27, the appendix 2026-10-28; brief
  `acf78faa`; one Sonnet agent an act): 331 steps, 286 with no gap, **no flow 13 regression**. Act 1 31/21 (`95fa4119`),
  Act 2 44/43 (`19095d1b`; 2.28's Expect corrected: the empty Completed and closed list), Act 3 60/56 (`03955e00`;
  3.22–3.24 played out of order by the agent, 3.24 not-played, seen right at 3.25–3.26), Act 4 50/44 (`c6ad1e41`),
  Act 5 29/27 (`35c2420e`), Act 6 42/33 (`2badf653`; **T312's 6.21, 6.40, 6.41 confirmed in the browser**), the
  appendix 75/62 (`8ef4b774`). Every flow 13 browser check held: "Your record:" under the notice; the last day refused
  on the press under the field with the focus; dialogs' safe button first, primary on top at 390; Verify in any case and
  spacing, "Exported 3 times, the last on … SAST", "Once"; 44 px at 390; nothing sideways at 390, 901, 1000, 1100;
  results take the focus; eight scheduled jobs. Closed as seen: T311, T313, T314, T287's verify item, T312. New:
  **T382** (an undated activity "observed after your last day"), **T385** (a date wraps at its hyphen on My activities at
  901/1000 px), **T384** (run history's From as a UTC day), **T383** (an MSF test fails between SAST and UTC midnight);
  and one flow 13 fault **fixed**: an erased trainee's review offered a dead-end export (`920f778c`, with a test).
  Dumps `recovery/scenario-t379-post-act{1..6,A}`, `-pre-a515-A`; scratch DB `wombat_scenario_t379_s515` kept.
  **Next:** the states pass (flow 13's rows in `states.md`), then step 8.
- **2026-10-28, step 7, the states** (two Sonnet agents in turn, the app republished with `920f778c` first): the
  profile, closed list and header actions, 24 rows (`1f64aa17`: 1 corrected, no defect; scratch `_s10`–`_s12`); Export,
  Verify, Home, My progress, My authorisations, My activities, the EPA page, 41 rows (`0881302a`: 2 corrected; scratch
  `_s20`, `_s21`). Every refusal under its field with the focus, every result focused, nothing sideways at 390 with
  44 px controls. Found: T382 again (the undated DOPS); **T386 proposed** (Home's retry focus lands on the card);
  T385 extended (Export's help splits a date at 390; a blue "until" in the 390 capture only, needs confirmation). One
  row left uncaptured: `trainee-profile-edit--end-failed` (needs a forced failure). The 42 other "Not yet captured
  (T379)" marks stripped. Some scratch captures show 2026-10-28 where the replay shows 2026-10-27. **Step 7 done.
  Next:** step 8.
