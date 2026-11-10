---
id: T398
title: Flow 17, build and publish an activity type, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-11-09
started: 2026-11-09
---

# T398 — Flow 17, build and publish an activity type, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 17 is the seventeenth flow of the restructure (W-008, BRIEF § 8): an institution's
administrator, the College admin or the Administrator reads, builds and publishes an activity type (its form, workflow
and credit rules), and every form a registrar files (flow 03) and an assessor rates (flow 04) is drawn from it. BRIEF § 4
names the builder one of the three dense pages to lay out deliberately.
**Surfaced:** 2026-11-09, after T394 (flow 16) closed; the operator said "flow 17".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T394 needed.

## Symptom

The activity-type pages (`/admin/activity-types`, `/admin/activity-types/new`, `/admin/activity-types/{ActivityTypeId:int}`:
`ActivityTypesList.razor`, `ActivityTypeEdit.razor`) have not been designed for the restructure. Brief:
`design/flows/17-activity-type-builder.md`, written 2026-09-26, before flows 01–16; T300 (D52) changed the pages after it
was written, and flows 01–16 changed the shell, the forms, the dialogs and the tables it inherits, so the brief must be
restated against what is there now. Known faults it takes up: whatever the restated brief finds still open (T324's
F17 part; § 7's B11), and what the earlier replays left for this flow (A.7.9's builder at 390; T280).

## Root cause

Not a defect with one cause: the pages predate the design system and the brief's requirements (§ Known problems).

## What to build

Run the loop as flow 16 ran it (`design/flows/16-college-catalogue/`, BRIEF § 11 "Flow 15" and "Flow 16"):
1. **Restate the brief** in flow 16's layout, from read-only audits of the code and the captures; check the pages table
   against the code (the file each route renders, `@page`), including what flows 01–16 already changed; run
   `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 17`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 17") on the Wombat design system (version 49), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** wireframes first (the brief's Mode), then fidelity; pick; a review of the boards against the code and
   the runbook; the operator's decisions; one correction round (its own canvas if over about 200 boards); check it
   item by item.
6. **Build** in waves (flow 16's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 16's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; sweep new tables 900–1100px against their container; after any width change, read every
   column of every list it reaches at 1280 for each role, and sweep 390–1440; re-take the flow's states.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [x] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link. (https://claude.ai/artifact/AQNS1QZDaUNizG7dzKPSGs; round 3 on https://claude.ai/artifact/Cpyw4vwX7UodgJUdKLF7Zw)
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/17-activity-type-builder/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 17) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 17, the baseline is re-captured, and
  flow 17's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T394 (flow 16), T392 (flow 15); T300, D52; T324; BRIEF § 4, § 6–8, § 11; CUSTOMIZATION.md.

## Notes

- **2026-11-09.** Filed and started on the operator's word ("flow 17").
- **2026-11-09, steps 3–4 (ahead of step 1, as T394).** Canvas "Wombat 17": https://claude.ai/artifact/AQNS1QZDaUNizG7dzKPSGs,
  created from the Design type, empty boards; design system v49 (1794239792-3119) installed: `tokens.json`,
  `components/bundle.css` and the three fonts copied from the system, and `tokens.css` by hand (flow 16's, header
  updated; its 54 properties match app.css's `:root`, `--shadow-raised` resolved). The mark copied in as
  `/_blob/a89fec160a8a8c0b20116ccc3634e0bc`. Steps 1–2 run in one Opus agent. Canvas scratch:
  `C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\5ae5486c-8726-4e89-ae35-c60429e28267\scratchpad\c17`.
- **2026-11-09, steps 1–2** (one Opus agent, `7d8ce207`): brief restated in flow 16's layout; `check_flow_completeness.py`
  exit 0; `check_verbatim_steps.py` 0 problems for flow 17; `check_baseline_paths.py` missing 0; staged 106 files (30
  captures, all opened) and the nine seed files by hand in `design/upload/seeds/`; `round-1-ask.txt` (610 lines), the
  mark's URL filled in here. Gone: T322, T329's builder half, T291 item 5, T190, T266, B11. Remain: T334, T324's builder
  half, T271, T291 items 3 and 6, T326, T280, Delete/Discard without asking, load errors printing exception text; and
  new, unfiled: field rows' buttons over their labels at 1280, the list not stacking at 390, editors below the list,
  refusals naming no tab, Publish's reason only a tooltip, JSON re-rendered on one line, "v0", no h2, Back to list,
  search per keystroke, no version history, an institution seeing every College's types. **Waiting:** the operator's
  Q1, Q2, Q8 (before round 1), then Q3–Q12 (brief § 6).
- **2026-11-10, decisions.** The operator took brief § 6's Q1–Q12 as recommended (sent the ask and said "done"): Q1
  (a) the tabs as a real tabs pattern, Details first; Q2 the JSON stays the editor with a read in words beside it, no
  diagram; Q8 Delete an outline button that asks nothing, Discard draft… through a ConfirmDialog, Publish asks only on
  warnings with its reason as text; Q9 T334 folded in; Q10 the fold-in list as written. No new D-row: Q9 is D52's T334
  note, Q8 follows D56's "asks first" pattern.
- **2026-11-10, round 1** (17 boards, `f17r1.css` and `canvas.json` in `round-1/`): Spec (Main) and Steps; A tabs, B
  one page of h2 sections, C a stepped path, each as list, read, new form, workflow/credit and publish. Steps board's
  totals 1.26–1.30: today 31 + 29; A 30/32; B 27/29; C 29/31 (1280/390). Assessment: one Opus agent →
  `round-1-assessment.md`.
- **2026-11-10, round 1 assessed** (one Opus agent, `ab13dd3d`): pick A (Q1), with B5.1's Publish section as A's
  panel and C5.1's "What you are publishing"; M1–M7 (Q12 subtitles, Q7/Q11 list, Q9 notice, Discard's untrue body,
  the missing Publish panel, B/C against Q1/Q2/Q8, the Steps board's double count: today 22 + 29); 14 shoulds, 12 nits.
  Q13–Q16 for the operator. No capture or seed reached the canvas: round 2 must upload them into the canvas's files.
- **2026-11-10, picks.** The operator took A (with B5.1's Publish section and C5.1's first-publish summary) and Q13–Q16
  as recommended ("yes"): Q13 the JSON not kept as typed, laid out indented in the parser's order; Q14 two Discard
  bodies, the details stay; Q15 unticking Active on a published type asks first; Q16 Publish saves first. The 29
  captures and nine seeds published into the canvas as project files under `project/ref/` (version 7). Round 2's ask:
  one Opus agent → `round-2-ask.txt`.
- **2026-11-10, round 2's ask** (one Opus agent, `c73fbcf4`; capture count corrected here): `round-2-ask.txt`, 941
  lines; A at full fidelity, Publish a fifth writer-only tab, ~65 states / ~145 boards (R2-A list 12, R2-B read 11,
  R2-C form 21, R2-D workflow/credit 5, R2-E publish/discard/deactivate 15); T1–T24 settled provisionally for round 2's
  review; readings to confirm: a reader sees no JSON (Q2), the Publish tab as part of the pick (M5).
- **2026-11-10, round 2** (145 boards, `f17r2.css` and `canvas.json` in `round-2/`; the operator sent the ask and said
  "done"): every reference file opened (30 captures, 9 seeds, said on R2-0-spec); A at fidelity, families A list, B
  read, C new type and form, D workflow/credit, E publish/discard/deactivate; T4 measured and failed (round 1's outline
  kept, S2); C1 `.tab-bar` restyled, S1 `.tab-problem`, C2/C3 changed. Reviews running (three Sonnet: code, cast, ds).
- **2026-11-10, round 2 reviewed** (three Sonnet agents: code `4adda465`, cast `5dc373c7`, ds `747fd118`; merged here,
  `round-2-review.md`): M1 `aria-required` and the star's line (39 boards), M2 names without their visible words (the
  problem tab, C19), M3 the Preview radio counted as a press (390: 22 + 31), M4 E02 on Details not Credit, M5 five
  states.md rows; build should-fixes J1–J15 (RefusalSummary across tabs, refusals as a list of places, refusal vs
  failure, Publish not inert). **Waiting:** the operator's Q17–Q20, then the round-3 correction ask.
- **2026-11-10, Q17–Q20 taken as recommended ("yes"):** Q17 Publish asks Q15's question first when Active was newly
  unticked, then saves, then asks on warnings; Q18 keep T133 (a pointer at a deleted field is dropped at save; C18
  redrawn as "None" with the Publish warning, the NEW refusal dropped); Q19 Q9 changes writing only, an inactive
  system-managed type stays readable; Q20 Publish lands on the tab it was pressed from. Round 3's ask: one Opus agent.
- **2026-11-10, round 3's ask** (one Opus agent, `19e5cab2`; `round-3-ask.txt`, 479 lines): 89 items (M1–M5, S1–S29,
  N1–N24, Q17–Q20, T1–T8); 87 boards, so a new canvas: "Wombat 17 · Round 3" https://claude.ai/artifact/Cpyw4vwX7UodgJUdKLF7Zw,
  created here with design system v49 (tokens.css by hand, bundle.css and fonts from Wombat 17, tokens.json from the
  system), the mark `/_blob/1464be2bd2c027ed86bec42ee064aa32`, `project/ref/` and round 2's 145 boards as
  `project/r2/R2-….ref.html` (not `.dc.html`, which would show as boards) with `f17r2.css`; the ask amended to match.
  Provisional in the ask (PROPOSED): C19's folds without `aria-expanded`, the tab's count as a number at 390, Publish's
  dialog `btn-primary`, Active's help shortened, a College with no speciality's subtitle, E02b.
- **2026-11-10, round 3** (87 boards, `684119ce`) checked item by item (one Sonnet, `round-3-check.md`): 86 rows, done
  82, partly 4 (S2's two unnamed margin resets; C3 says six lists/nine rules, app.css has seven; Spec rows not restated;
  seven Steps rows "as round 2's"), not done 0. One regression: the in-place editor is `<li role="group">` (19 files).
  Steps arithmetic and the five states.md rows hold. Three stale line citations. Pixel claims not measured. Waiting:
  the operator's decisions (carry the fixes into the build, no round 4; keep the seven PROPOSED settlements).
- **2026-11-10, round 3 decisions.** The operator said "yes": no round 4 (the check's corrections go into the build:
  the editor `li` without `role="group"`, seven selector lists, the two margin rules named, the dropped Spec/Steps rows
  read from round 2, three line citations corrected); the seven PROPOSED settlements kept. Next: step 6, the build.
- **2026-11-10, build plan** (one Opus agent, `af28d571`): § 0, lanes A (below Web), B (the list, read-outs), C (the
  builder), the sweep, W, two Sonnet reviews, one fix pass; no migration. The operator said "yes" to P1 (only the two
  pages tell a refusal from a failure; T399 proposed for the rest) and P2 (every tab's problems reported at once).
- **2026-11-10, § 0** (one Opus agent, `d3c98db5` on `t398`; notes for the lanes `08d1bbda`): shared shapes, T334/Q19
  rule (one test inverted on purpose), DTO members, refusal shape, scale seed keys, CatalogueLinks, ActivityTypeWords,
  read-out stubs, C3 in seven lists. Suites 836/4411/1035/52/4675/530 → 840/4419/1035/52/4703/530, 0 warnings, no
  migration. Lanes A, B, C cut from `08d1bbda` and running.
- **2026-11-10, lanes merged** on `t398` (A `562aae67` `ab9ed40d`; B `69653e7f`; C `1778a84e` `599f3841`; conflicts: the
  builder page taken from C, DESIGN.md B's and C's lines; one fix, C's unreachable fallback places). Build 0 warnings;
  suites 895/4459/1035/52/4806/532. The sweep running (one Opus agent, scratch `wombat_scenario_t398_sweep`). Lane
  notes for W and § 5 in the scratchpad `b398/lane-{a,b,c}.md`.
