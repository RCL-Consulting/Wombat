---
id: T376
title: Flow 12, people admin, has not been through the redesign loop
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-10-23
started: 2026-10-23
---

# T376 — Flow 12, people admin, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 12 is the twelfth flow of the restructure (W-008, BRIEF § 8): the InstitutionalAdmin keeps her
institution's people (users, a second role, assessor training, a password reset, a lock and a reactivation, her own
institution's record). Role grants and locks are access control.
**Surfaced:** 2026-10-23, after T373 (flow 11) closed; the operator said "push, then start flow 12".
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T373 needed.

## Symptom

The people-admin pages (Users, the user page with its roles, scope, invitations, lock and reset, Assessors and the
assessor profile, the institution's record, SSO, the InstitutionalAdmin dashboard) have not been designed for the
restructure. Brief: `design/flows/12-people-admin.md`, written 2026-09-26, before flows 01–11. Known faults it takes up
include T264 (red in-row actions with no confirmation), T286, T288 and T289 (scope leaks in SSO mappings and the assessor
profile), T291, T317, T323, T324 and T326; the audits in step 1 list the rest, including what flow 11 (T373) and D55
already changed.

## What to build

Run the loop as flow 11 ran it (`design/flows/11-onboarding/`, BRIEF § 11 "Flow 10" and "Flow 11"):
1. **Restate the brief** in flow 11's layout, from read-only audits of the code and the captures; check the pages table
   against the code, including what flows 01–11, D54 and D55 already changed; run `check_flow_completeness.py` and
   `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 12`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 12") on the Wombat design system (version 29), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** structure where the brief says wireframe first, fidelity otherwise; pick; fidelity; a review of the boards
   against the code and the runbook; the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (flow 11's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 10's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; re-take the flow's states on scratch copies.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [ ] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit.
- [ ] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link.
- [ ] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/12-people-admin/`.
- [ ] Built, with all six suites green and DESIGN.md amended (its banner lists flow 12) — the squash commit and counts.
- [ ] The whole runbook replays on a fresh database with no regression from flow 12, the baseline is re-captured, and
  flow 12's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
- [ ] The design system is re-synced and republished — its version number.

## Related

T373 (flow 11), T369 (flow 10), T264, T286, T288, T289, T291, T317, T323, T324, T326, T056, D54, D55; BRIEF § 6, § 8,
§ 11.

## Notes
- **2026-10-23, steps 3–4 (ahead of step 1's commit).** Canvas "Wombat 12":
  https://claude.ai/artifact/EUfPe9d3zgnGxvQ9LeSzz8, created from the Design type, empty boards; design system v29
  (1792508645-23ab) installed: `tokens.json`, `components/bundle.css`, the three fonts copied from the system, and
  `tokens.css` by hand (flow 11's, header updated; its 54 properties match app.css's `:root`). The mark copied in as
  `/_blob/b8532f490e278114d4c3b43f9a0533f6`.
- **2026-10-23, steps 1–2.** Restated from read-only audits at `98584921` (one Opus agent): flow 11's layout; Mode F/W
  (wireframes first for the user page with assessor training, Q1–Q2, and for her Home, Q3/Q7/Q8; Users at fidelity in
  2–3 variations; one proposal each for her institution's record and SSO mappings). Dropped as done: T302, T303,
  T278/T290, T317's reset half, T321, T322, flow 01's Home. § 5: 17 rows (9 tasks, 8 new defects); § 6: Q1–Q16, each
  with a recommendation (Q11, Q12, Q16: T288, T333 and T289 land before the build). Completeness exit 0, verbatim
  424/0 (115 stale steps in flows 01–07 and 11 re-synced), baseline missing 0. Staged: `design/upload/`, 123 files;
  the four in `crop-first/` (A.4.5-0/1/2, `user-detail--pending-invitations`) opened: no link, password masked, they go
  as they are. Inferred, unreplayed: the user page's Pending invitations card can no longer show anything since flow 11;
  Add role grants no scope. **Next:** the operator sends `round-1-ask.txt` to the canvas with § 4's key screenshots.
- **2026-10-23, round 1** (canvas 1792747971-2e98, 44 boards in `round-1/`, `62f6ec41`; assessed in
  `round-1-assessment.md`): picks S1 (sections), H1 (cards, waiting line), UA (one table, filters); I and O confirmed
  with corrections. 4 must-fixes (C1 scope is per person not per role, checked: `WombatIdentityUser` SpecialityScopes;
  C2 role words not flow 11's; C3 du Plessis has My progress; C4 H2's invented counts), 13 should, 8 nits. New operator
  questions Q17–Q22. Q20 checked and proposed as **T377** (no code writes a Manual role assignment, so SSO can strip a
  hand-given role). The Spec board says no capture arrived: re-attach for round 2.
