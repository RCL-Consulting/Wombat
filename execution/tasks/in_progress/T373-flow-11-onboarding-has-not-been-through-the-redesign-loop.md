---
id: T373
title: Flow 11, onboarding, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-10-19
started: 2026-10-19
---

# T373 — Flow 11, onboarding, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 11 is the eleventh flow of the restructure (W-008, BRIEF § 8): an admin invites each person
with a role and scope, the invitee registers from the emailed link (the account card, static), and a registrar waits as
PendingTrainee until the InstitutionalAdmin admits them with their programme dates. Every user meets it once, and a role
and scope are access control.
**Surfaced:** 2026-10-19, after T369 (flow 10) closed; the operator said "carry on".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T369 needed.

## Symptom

The onboarding pages (Invitations, Register, the pending registrar's pages, the admission form) have not been designed
for the restructure. Brief: `design/flows/11-onboarding.md`, written 2026-09-26, before flows 01–10. Known faults it takes
up include T264 (revoke and refused-issue states), T306 (training year on admission), T304 (only the adopted curriculum),
T315 (the token in the address bar) and T317 (a refused registration drops the names); the audits in step 1 list the
rest, including what D55 already changed for PendingTrainee.

## What to build

Run the loop as flow 10 ran it (`design/flows/10-msf-campaign/`, BRIEF § 11 "Flow 09" and "Flow 10"):
1. **Restate the brief** in flow 10's layout, from read-only audits of the code and the captures; check the pages table
   against the code, including what flows 01–10, D54 and D55 already changed; run `check_flow_completeness.py` and
   `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 11`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 11") on the Wombat design system (version 25), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** structure where the brief says wireframe first, fidelity otherwise; pick; fidelity; a review of the boards
   against the code and the runbook; the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (flow 10's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 10's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; re-take the flow's states on scratch copies.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [ ] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link.
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/11-onboarding/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 11) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 11, the baseline is re-captured, and
  flow 11's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T369 (flow 10), T366 (flow 09), T264, T286, T289, T303, T304, T306, T315, T317, D54, D55; BRIEF § 6, § 8, § 11.

## Notes
- **2026-10-19, steps 3–4 (ahead of step 1's commit).** Canvas "Wombat 11":
  https://claude.ai/artifact/SFn6dDmJnoww2fhYrQD416, created from the Design type, empty boards; design system v25
  (1792413020-e2bb) installed: `tokens.json`, `components/bundle.css`, the three fonts copied from the system, and
  `tokens.css` by hand (flow 10's; its 54 properties match app.css's `:root`). The mark copied in as
  `/_blob/b3cd1c9ecfc87765138aec8743e383bc`.
- **2026-10-19, steps 1–2.** Restated from read-only audits at `a1a9b120` (one agent, `11a77b0c`): flow 10's layout;
  Mode kept but for the pending registrar's pages, now ONE proposal at fidelity (D55 and T141 left nothing to vary);
  Invitations 2–3 variations at fidelity, Register one proposal, Trainees and admission 2–3 wireframes. Dropped as done:
  T303, T304, D55/T141's picker and menu, flow 01's and 02's parts, T285, T290, T306's dashboard half. ~20 observed
  defects added (§ 5). Operator questions Q1–Q14 in § 6, each with a recommendation. Completeness exit 0, verbatim
  424/0 (130 stale steps in other briefs re-synced), baseline missing 0. Staged: `design/upload/`, 119 files; every
  capture with a live link left out. Mark written in (`9f81d865`). **Next:** the operator sends `round-1-ask.txt` to
  the canvas with § 4's key screenshots. Note for step 7: states.md's `new-activity--pending-trainee` row predates D55.
- **2026-10-19, round 1** (canvas 1792417019-5603, 53 boards in `round-1/`; assessed in `round-1-assessment.md`):
  Invitations A/B/C, Register R, the pending Home H, Trainees W1–W3, Spec and Steps. Recommended: B (a page of its
  own) and W1 (admission a page); R and H confirmed with corrections. Must-fix: "Sent" on replay rows (the log sender
  reports none), H3 names Molefe for Mahlangu, Z1's "stays active" (accepting revokes the others, T061), a future start
  is "Not started", the T315 cookie must be Lax, and the T286 and Q10 refusals leak across institutions. Decisions
  Q1–Q18 (§ 6) to the operator.
- **2026-10-19, round 2** (canvas 1792419753-ee69, 192 boards in `round-2/`) reviewed from three sides (code, cast,
  design system; merged in `round-2-review.md`: M1–M11, S1–S24, 14 nits). F1–F10 to the operator. The gap behind
  T374 (an existing account onto a programme) stays proposed, outside this flow.
- **2026-10-19, round 2's decisions.** The operator took F1–F10 as recommended (`round-2-review.md` § The operator's
  decisions). Round 3's ask written (`round-3-ask.txt`: F1–F10, M1–M11, S1–S24, the Steps board, the nits, the Spec's
  DATA list). **Next:** the operator sends it to the canvas; then save round 3 and check it item by item.
- **2026-10-20, round 3** (canvas 1792460548-2e63, 203 boards in `round-3/`): checked item by item by one Sonnet agent,
  rendered in Chromium (`round-3-check.md`): F 9/1, M 10/1, S 22/2 done/partly, none not done; 0 px sideways on every
  board at every width; no invented data. Two must-fixes (the Invitee header's `.col-wrap`; M12's name before a read)
  and five partlies taken into the build as rules (the check's last section). **Final.** **Next:** step 6, the build.
- **2026-10-20, step 6, built** (`69a17c70`, squashed from `t373`). The operator's word: push, drop the scratch DBs, T371
  no, proceed. One planner (Opus: `build-lanes.md`, `build-contracts.md`, `round-3/boards-text.md`, and § 0 with the one
  migration `T373_InvitationIssuedAsRole`), lanes I, G and A in one wave (Opus), the runbook lane (Opus), three Sonnet
  reviews (`build-review-code.md`, `-pages.md`, `-runbook.md`, merged in `build-review.md`: no must-fix, 15
  should-fix), one fix pass (Opus; every finding held and was fixed; B1 a new card "Another invitation opened"; B2
  admission in one transaction; R3 native `disabled` kept under T234's rule). Suites: Domain 829, Application 3980,
  Infrastructure 1029, Architecture 52, Web 3980, Integration 507. Lanes I and A could not finish their own suites (the
  machine ran low on memory with three lanes testing): run suites one at a time. For step 7: the clipboard permission
  in the Playwright context (README § The browser); invitation stubs are tagged `invitation` only (no `role:`), so
  match a link to a person by the mail's text; 5 more stub mails from admissions; check 901, 1000 and 1100 px for words
  split in Delivery and Curriculum; flow 13's Completed table still scrolls in its container at 390. Branches `t373*`
  and their worktrees kept. **Next:** step 7, the whole-runbook replay on a fresh database.
