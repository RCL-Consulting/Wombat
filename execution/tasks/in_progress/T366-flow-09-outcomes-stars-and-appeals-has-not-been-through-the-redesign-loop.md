---
id: T366
title: Flow 09, outcomes, STARs and appeals, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-10-14
started: 2026-10-14
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

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [ ] The canvas is created, with the mark copied in before round 1 — its link.
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/09-outcomes-stars-appeals/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 09) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 09, the baseline is re-captured, and
  flow 09's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

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
