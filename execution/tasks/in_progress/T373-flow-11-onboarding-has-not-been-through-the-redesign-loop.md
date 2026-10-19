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
