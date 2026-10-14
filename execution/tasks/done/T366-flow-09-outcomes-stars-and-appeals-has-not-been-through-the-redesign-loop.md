---
id: T366
title: Flow 09, outcomes, STARs and appeals, has not been through the redesign loop
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-10-14
started: 2026-10-14
completed: 2026-10-14
---

# T366 — Flow 09, outcomes, STARs and appeals, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 09 is the ninth flow of the restructure (W-008, BRIEF § 8): what a committee's
decisions become: STARs, the trainee's authorisations, the STAR register, a revocation, and an appeal.
**Surfaced:** 2026-10-14, after flow 08 closed (T364); the operator said "lets go".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T364 needed.

## Symptom

The outcome pages (`/portfolio/authorisations`, `/committee/my-reviews`, the review page's appeal and outcome cards,
`/admin/entrustment-decisions`, `/committee/decisions-due` as the place an outcome lands, `/access-denied`) have not been
designed for the restructure. Brief: `design/flows/09-outcomes-stars-appeals.md`. BRIEF § 8 marks the flow F/W. Known
faults the flow takes up: the quorum refusal shows only at the head of the review page (`act-4/4.46-1`, BRIEF § 6), and
the chosen outcome is clipped at 1280 px once Remitted opens its second column (F-4.45c).

## What to build

Run the loop as flow 08 ran it (`design/flows/08-committee-calendar/`, BRIEF § 11 "Flow 07" and "Flow 08"):
1. **Restate the brief** in flow 08's layout, from read-only audits of the code and the captures; check the pages table
   against the code, including what flows 07 and 08 already changed on the review page and Decisions due; run
   `check_flow_completeness.py` and `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 09`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 09") on the Wombat design system (version 20).
4. **Copy the mark** into it before round 1.
5. **Rounds:** structure, pick, fidelity; a review of the boards against the code and the runbook; the operator's
   decisions; one correction round; check it item by item.
6. **Build** in waves (flow 08's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 08's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; re-take the flow's states on scratch copies.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [x] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
  `0058adf3`.
- [x] The canvas is created, with the mark copied in before round 1 — its link.
  https://claude.ai/artifact/5LfLPdcXJ2iRX6GSm2bbKP (`47f2850d`).
- [x] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/09-outcomes-stars-appeals/`.
  B; `round-2-review.md` (`4da096d1`), `round-3-check.md` (`13057800`).
- [x] Built, with all six suites green and DESIGN.md amended (its banner lists flow 09) — the squash commit and counts.
  `49e7cf5e`: Domain 815, Application 3809, Infrastructure 1023, Architecture 52, Web 3735, Integration 491.
- [x] The whole runbook replays on a fresh database with no regression from flow 09, the baseline is re-captured, and
  flow 09's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
  `wombat_scenario_t366` (2026-10-14): acts 1–6 and the appendix (`60840a5d`..`4506f65e`), 331 steps, 273 with no gap,
  no regression-t366; states 57 rows, 56 hold, 2 corrected (`851d398f`); `check_baseline_paths.py` missing 0.
- [x] The design system is re-synced and republished — its version number.
  Version 23 (1791997100-4a21), from `e87e9627`, `9ba80586`, `63dd5d36`; the live v21 matched HEAD first.

## Related

T364 (flow 08), T361 (flow 07), T307, T309, D51, D53; BRIEF § 6, § 8, § 11.

## Notes

- **2026-10-14, step 1.** Restated from two read-only audits at `adfa3700` (the code; the captures, the key ones
  opened). T308 and T319 are unbuilt (the trainee's review view has no history or Appeals block; My authorisations lists
  Active only); the revoke's "The trainee is notified." is false. Completeness exit 0, verbatim 424 steps 0 problems
  (153 stale step pointers in flows 01–07 re-synced), baseline missing 0. `round-1-ask.txt` is § 1 + § 8 as one paste,
  with `/_blob/MARK` until the mark is copied in.
- **2026-10-14, steps 2–4.** Staged with `stage_upload.ps1 -Flow 09` (106 files, 10.3 MB); the staged copies of
  `4.44-1`, `4.45-3` and `review-detail--appeal-dismissed` cropped to the Appeals section, `4.47-1` split into `-top`
  and `-appeals` (each opened). Canvas "Wombat 09": https://claude.ai/artifact/5LfLPdcXJ2iRX6GSm2bbKP, design system v20
  (1791917863-8f48) installed, the mark copied in as `/_blob/19eee4d64e6ebc1818aa6f9007654872` and written into § 1 and
  `round-1-ask.txt`. Next: the operator sends `round-1-ask.txt` with the attachments (step 5).
- **2026-10-14, round 1 picked: B** (`58f8dc87`, assessed in `round-1-assessment.md`, `71e02aa2`): a review page of its
  own at `/committee/my-reviews/{id}`, one column at every width; A's inline resolve form and unchanged access denied;
  C's "What happens next" and "What will be recorded" wording. The operator said "go ahead" on every recommendation:
  Q7 send the revocation mail (no reason, the reviser by name); Q8 revoked and expired listed, superseded off, the
  reason shown on her page; Q9 no appeal deadline (§ 3F question 13 added), the reason's readers named truthfully;
  Q10 no menu item; Q11 **T367** (T319's revocation half) lands before the build, T308 and these pages' halves of
  T260, T264, T324, T325, T328, T329, T272 fold in. Round 1 drew invented fixture data: re-attach and name the folder.
- **2026-10-14, round 2** (canvas 1791962299-ac6d, `round-2/` at `1221c005`) reviewed from three sides (code, cast,
  design system; `round-2-review.md`, `4da096d1`): 11 must-fixes, 13 should-fixes. The operator said "yes" to F1–F9:
  newest first; the expired STAR a states row only; Revoke always enabled, a blank reason refused; "Download
  certificate for PAED-001"; "Go to the appeal" via PageFocus; dialogs return focus to the opener; T279 kept (C13 a
  Coordinator holding the External seat); `/committee/my-reviews/{id}` the review's own trainee only, ratified or later;
  Show. Superseded reads "Superseded by STAR #18, issued …" (no stored date). Round 3 is `round-3-ask.txt`.
- **2026-10-14, round 3** (canvas 1791966935-56f8, `round-3/` at `4c0912ac`) checked item by item
  (`round-3-check.md`, `13057800`): F1–F9, M1–M11, S1–S13 and the nits all done; **the design is final**. The canvas had
  shipped without its tokens stylesheet (the operator saw bland previews), fixed before the check. Carried to the build:
  the Outcome select at 390 (radios the fallback), F5's 390 saving measured in the replay. Next: T367, then the build.
- **2026-10-14, step 6 started.** `build-lanes.md` and `build-contracts.md` (`cc4256e6`); integration branch `t366`
  (§ 0, `ReviewLinks.MyReview`, `1737db34`). Wave 1: lane T (her review: the list, `/committee/my-reviews/{id}`,
  `DecisionHistory`, `AppealEntry`) on `t366-trainee`, lane S (My authorisations, the register, `ConfirmDialog`) on
  `t366-stars`. Wave 2: lane C (the committee's Appeals section, the jump) after T merges. Then the runbook lane, three
  Sonnet reviewers, one fix pass, the squash. A fresh restore failed NuGetAudit on `System.Security.Cryptography.Pkcs`
  10.0.0 (GHSA-846f-f764-76mx high, GHSA-2fmv-rv2c-59x7, published 2026-10-13, via MailKit): pinned 10.0.13 on master
  (`a7f790b3`) before the lanes were cut.
- **2026-10-14, step 6 done** (`49e7cf5e`, squashed from `t366`): lanes T (`t366-trainee`), S (`t366-stars`), C
  (`t366-committee`, wave 2), the runbook lane (`t366-runbook`), three Sonnet reviews (`build-review.md`: no high; R1/D3
  exception text on Resolve, D1 Try again's focus, D2 the revoke box), one fix pass (`t366-fix`, 14 commits). Kept: D8
  (native disabled while busy), D13 ("they" for the appeal body). **For the replay:** D14 the script module under the
  CSP, D15/G7 the focus after Remit/Dismiss, D16 the radios' targets at 1280/768, D6's Escape in a browser, F5's 390
  saving, T367's 4.36 mail in the log. Branches `t366*` kept locally. Next: step 7, the replay.
- **2026-10-14, step 7 done.** Replay on `wombat_scenario_t366` (published from `49e7cf5e`), one agent per act:
  act 1 31/19, 2 44/35, 3 60/56, 4 50/48, 5 29/23, 6 42/32, A 75/60 (steps / no gap); every other gap an open task;
  no regression from flow 09. Closed by the replay: F-4.36a, F-4.37a, F-4.43a, F-4.45c, F-4.48a, T260's entrustment
  half, T308, T323; T367 done (`5c183bb9`). Browser checks all pass: D14 (the module 200, no CSP violation), D6 (Escape
  to Lodge appeal), D15/G7 ("Appeal resolved." focused), D16 (radios 24 px at 1280/768, 44 at 390), F5 (the jump saves
  7,129 px at 1280, 13,033 at 390). States: 57 rows, 56 hold, 2 corrected (`851d398f`). Proposed T368 (A.5.10: the
  Coordinator's card stripe, unconfirmed). Observed, not filed: the revoke dialog keeps "Give a reason for the
  revocation." until the next press; the certificate's evidence summary prints UTC times. Dumps
  `recovery/scenario-t366-post-act{1..6,A}`, `-pre-a515-A`; scratch DBs `wombat_scenario_t366_s515`, `_s515b`,
  `_st1`..`_st3` (drop on the operator's word). Next: step 8, the design system.
- **2026-10-14, steps 8 and 9 done.** The live design system matched HEAD; re-synced (`e87e9627`: 7 records added, 13
  revised), one fault it found fixed (`9ba80586`: My committee reviews' View 44 px on a phone), one Sonnet review (no
  must-fix; six wording fixes, `63dd5d36`), published as version 23 (1791997100-4a21; version 22 put the index beside
  `project/`, corrected in the next call). Lessons in BRIEF § 11 "Flow 09" (`c9d33455`).
