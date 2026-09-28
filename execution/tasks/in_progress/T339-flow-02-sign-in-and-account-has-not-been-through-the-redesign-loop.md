---
id: T339
title: Flow 02, sign-in and account, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-09-28
started: 2026-09-28
---

# T339 — Flow 02, sign-in and account, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 02 is the second flow of the restructure (W-008, BRIEF § 8). Everyone passes through
these pages each session, and every refusal on the way in is access control.
**Surfaced:** 2026-09-27, the flow 01 pilot closing (T335). BRIEF § 11 says how to run it.
**Model:** Opus. Design review, a multi-lane build and an acceptance replay, as T335 needed.

## Symptom

The sign-in card, register, forgot and reset password, link account, sign out, My account and Change password still
wear the pre-restructure language inside the new shell. Their brief (`design/flows/02-sign-in-and-account.md`) predates
the pilot: its ASK line is the old "straight to fidelity" one (BRIEF § 2.4), and its screenshots show the old shell.

## What to build

Run flow 01's loop, as BRIEF § 2.3 now describes it and § 11 corrected it:
1. **Restate the brief.**
   - Rewrite its ASK line to the structure-first wording flow 01 used (`design/flows/01-shell.md` § 1).
   - It is marked F (little structural change expected), so round 1 may be one structural proposal to confirm, not
     three.
   - Check its pages table against the code. T335 added `POST /account/profile/submit`, and My account's name edits now
     post and re-issue the sign-in.
   - Drop the Title Case, "Welcome" and "Viewing as" wording; the shell is flow 01's (§ 2.4).
   - Run `design/tools/check_flow_completeness.py`.
2. **Stage the upload set:** `design/tools/stage_upload.ps1 -Flow 02` (BRIEF § 3). Crop any capture that shows a
   registration link.
3. **A new canvas** from the main app's Design page (not claude.ai/design), on the Wombat design system version 4
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). The operator creates it and gives its link.
4. **Copy the mark into it before round 1:** `Artifact publish`, `url` = the new canvas, `asset: true`, `from_url` = the
   design system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`. Put the returned `/_blob/` URL into round 1's
   message.
5. **Rounds.**
   - Structure (or one proposal), then the pick, then fidelity with its states.
   - Then a review round: read the boards with `Artifact read`, as `design/flows/01-shell/round-2-review.md` did.
   - The operator's decisions, then one correction round.
   - Record everything under `design/flows/02-sign-in-and-account/`.
6. **Build (step F)** in worktree lanes, following T335's pattern.
   - Reuse T335's lane rules (`design/pilot/build-lanes.md`) and its reviewers' brief (`design/pilot/review-lanes.md`).
     Name the shared test helpers up front (BRIEF § 11).
   - Integrate, review, fix, and land one squash.
   - Amend DESIGN.md with its banner, and change its tests deliberately.
7. **Replay (step G).** Flow 02's steps span the acts, so the whole runbook runs on a fresh database. Use T335's
   Workflow script, `design/pilot/replay-workflow.js`; its header says what to change. Drive the browser with the
   Playwright library installed in a scratch folder (BRIEF § 9). Keep capture names. Afterwards run `check_baseline_paths.py`, and check citations
   against the images.
8. **Re-sync the design system** from the code and republish it: uploads first, the index last (BRIEF § 2.3 step 6).
9. **Lessons into BRIEF § 11**, only where flow 02 teaches something new.

## Verification

- [x] The brief is restated, and `check_flow_completeness.py` exits 0 — the commit. `ca3b19ff` (2026-09-28); completeness exit 0, verbatim steps 0 problems, baseline paths missing 0. Upload set staged: 109 files, none in `crop-first/`.
- [x] The canvas is created, with the mark copied in before round 1 — its link here. "Wombat 02", https://claude.ai/artifact/5BnSniM2QS8NouqWS6wsPB (2026-09-28); the mark copied as `/_blob/a6c690a50a59fa44f668ab96ac894576` (source asset `16c4e619…`, sha256 `db68eb6f…`), before round 1.
- [x] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/02-sign-in-and-account/`. Rounds 1–3 (2026-09-28): round 1 one structure, Q1–Q4 and T287 decided; round 2 at fidelity, reviewed from four sides (`round-2-review.md`), E1–E11 decided; round 3 checked item by item (`round-3-check.md`). T340 filed.
- [x] Built, with all six suites green and DESIGN.md amended (its banner lists flow 02) — the commit. `f50dffb2` (2026-09-28): three lanes, a four-sided review, two fix lanes and the runbook lane, squashed; Domain 791, Application 3400, Infrastructure 1010, Architecture 49, Web 2720, Integration 480, all green; build 0 warnings; `check_baseline_paths.py` missing 0. T341 filed from the review.
- [x] The whole runbook replays on a fresh database with no regression from flow 02, and the baseline is re-captured,
  with `check_baseline_paths.py` at `missing 0` — the Actual lines. `6da0cdff` (2026-09-28): `wombat_scenario_t339`
  against `f50dffb2`, 246 of 325 steps with no gap, 81 gaps on open tasks already cited; the one flow 02 regression
  (A.7.12, Remember me at 390 px) fixed in `6735fa98` and re-checked in Chrome; one new defect, not flow 02's, noted on
  T328. The Account and sign-in states captured (63 rows, 76 files); `check_baseline_paths.py` missing 0; flow 02's key
  captures opened and checked against their citations.
- [x] The design system is re-synced and republished — its version number. Version 7 (2026-09-28; https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18): `56628efb` and `e33b563b`; 4 components added (SignInCard, PasswordField, PasswordRules, MyAccount), 7 revised, 4 icons uploaded (58), the index sent last. Lessons: BRIEF § 11 "Flow 02" (`937fe19c`).

## Related

W-008, W-009, T335 (the pilot; BRIEF § 11), T317 (account leftovers: registration names, GET `/account/logout`, the
reset form), T315 (the token in the address bar), T324 (validation messages).
