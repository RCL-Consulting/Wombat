---
id: T369
title: Flow 10, the MSF campaign, has not been through the redesign loop
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-10-15
started: 2026-10-15
completed: 2026-10-19
---

# T369 — Flow 10, the MSF campaign, has not been through the redesign loop

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 10 is the tenth flow of the restructure (W-008, BRIEF § 8): a Coordinator runs a
registrar's multi-source feedback campaign, colleagues answer anonymously on `/msf/respond` (the only page people without
an account see), and the registrar reads the released report. Anonymity must hold, and a released report is evidence.
**Surfaced:** 2026-10-15, after T290, D54 and D55 closed; the operator said "yes" to starting flow 10.
**Model:** Opus. Design rounds, a review, a build and an acceptance replay, as T335–T366 needed.

## Symptom

The MSF pages (the campaign list and set-up, a campaign's page with its nominees and release, `/msf/respond`, and the
registrar's released report) have not been designed for the restructure. Brief: `design/flows/10-msf-campaign.md`,
written 2026-09-26, before flows 01–09. Known faults it takes up include T270 (MSF page polish) and the MSF halves of
T329 and T272; the audits in step 1 list the rest.

## What to build

Run the loop as flow 09 ran it (`design/flows/09-outcomes-stars-appeals/`, BRIEF § 11 "Flow 08" and "Flow 09"):
1. **Restate the brief** in flow 09's layout, from read-only audits of the code and the captures; check the pages table
   against the code, including what flows 01–09 and D55 already changed; run `check_flow_completeness.py` and
   `check_verbatim_steps.py`.
2. **Stage** (`stage_upload.ps1 -Flow 10`); write § 1 + § 8 as ONE paste file.
3. **A new canvas** ("Wombat 10") on the Wombat design system (version 23), with `tokens.css` written by hand.
4. **Copy the mark** into it before round 1.
5. **Rounds:** structure (or straight to fidelity, as the brief says), pick, fidelity; a review of the boards against
   the code and the runbook; the operator's decisions; one correction round; check it item by item.
6. **Build** in waves (flow 09's build lanes as template), integrate, review sized to the change, one fix pass, squash;
   DESIGN.md amended with its banner.
7. **Replay** the whole runbook on a fresh database (flow 09's `replay-brief.md`); keep capture names;
   `check_baseline_paths.py`; re-take the flow's states on scratch copies.
8. **Re-sync the design system** and republish (read the live files and compare with HEAD first).
9. **Lessons** into BRIEF § 11, only what is new.

## Verification

- [x] The brief is restated, and `check_flow_completeness.py` exits 0 — its commit. `ded43219` (step 1's note).
- [x] The canvas is created, with `tokens.css` and the mark copied in before round 1 — its link.
  https://claude.ai/artifact/CPUXS577gX2yo79AryKmVE (step 3–4's note).
- [x] A chosen design with its states, reviewed, and the decisions recorded — `design/flows/10-msf-campaign/`.
  Round 3 checked final (`e825e2e2`, `round-3-check.md`).
- [x] Built, with all six suites green and DESIGN.md amended (its banner lists flow 10) — the squash commit and counts.
  `68327c9d`: Domain 821, Application 3879, Infrastructure 1029, Architecture 52, Web 3837, Integration 491.
- [x] The whole runbook replays on a fresh database with no regression from flow 10, the baseline is re-captured, and
  flow 10's states are re-taken, with `check_baseline_paths.py` at `missing 0` — the Actual lines.
  2026-10-19 on `wombat_scenario_t369` (`652749d0` and its parent): 331 steps, 281 with no gap, no flow 10
  regression; states 62 rows hold, 5 refusal rows given their exact words; missing 0.
- [x] The design system is re-synced and republished — its version number. Version 25 (1792413020-e2bb), from
  `1f639c60`.

## Related

T366 (flow 09), T364 (flow 08), T162 (the system-managed MSF type), T153, T270, T329, T272, T334, D9, D55; BRIEF § 6,
§ 8, § 11.

## Notes

- **2026-10-15, step 3–4 (ahead of step 1's commit).** Canvas "Wombat 10":
  https://claude.ai/artifact/CPUXS577gX2yo79AryKmVE, created from the Design type, empty boards; design system v23
  (1791997100-4a21) installed: `tokens.json`, `components/bundle.css`, the three fonts copied from the system, and
  `tokens.css` written by hand (flow 09's, whose 54 properties match app.css's `:root`; `--shadow-raised` resolves to
  the same value). The mark copied in as `/_blob/d5b5890c1eb0efaad13d0d347d1ebf4c`.
- **2026-10-15, step 1.** Restated from read-only audits at `cc80d5b3` (one agent): flow 09's layout; Mode kept (F:
  one proposal for the list, the campaign page, coverage and `/msf/respond`, 2–3 variations for the coordinator's report
  and the registrar's MSF reports). T331 and T322 dropped (done in flow 01); new observed defects in § 5 (no confirm on
  Close/Release, no `<h1>` on `/msf/respond`, the 1280 cut on My MSF reports, the list's 390 scroll, …). The `states/`
  captures for these pages are old-frame (2026-09-26); the step captures are T366's. Operator questions Q7–Q11 in § 6
  (Q10: a campaign outliving its registrar's programme, D54/D55, inferred). Completeness exit 0, verbatim 424/0 (118
  stale steps in other briefs re-synced), baseline missing 0. The mark is written into § 1 and `round-1-ask.txt`.
- **2026-10-15, round 1** (canvas 1792060099-c3ec, `round-1/` saved; assessed in `round-1-assessment.md`): the proposal
  P1–P6 and dialogs, report variations RA/RB/RC, the registrar's MA/MB. Recommended: RA (with RB's group header, RC's
  Released one-liner), MA (`/msf/my-reports/{id}`, MB-390's alert reworded "could not be found"). Corrections: DTO
  additions (list, campaign page, report), Responded 3 at 3.38 is 0, placeholder EPA titles; anonymity holds. Round 2's
  ask must require a spec and a steps board. The attachments arrived. Q7–Q11 to the operator.
- **2026-10-15, the operator's round 1 decisions:** RA + MA; Q7 header link, no Administrator item; Q8 a release mail
  (T367's path); Q9 nobody sees the spread; Q10 warn and keep the release; Q11 fold T270, T325 and the MSF halves of
  T329, T280, T324, T323 into the build. Recorded in `round-1-assessment.md` § 9. `round-2-ask.txt` written (323 lines;
  spec and steps boards). Flagged PROPOSED in it: no release mail when her programme has ended (D54: the link would land
  on a page she cannot open); "Closed by itself" inferred from the close's date.
- **2026-10-15, round 2** (canvas 1792061893-64a5, 159 boards in `round-2/`; drawn without its screenshots) reviewed
  from three sides (code, cast, design system; merged in `round-2-review.md`: M1–M10, S1–S17, 13 nits). The operator
  took every recommendation on F1–F11: F1 an Open report shows no results until it closes; F2 a group floor of 3 (the
  cast gains a third Nurse and Consultant; `Distribution` dropped); F5 `ClosedByUserId` stored (a migration); F3 no
  Responses column on her list; F4 no release mail once her programme has ended; F6–F11 as recommended. Round 3 is
  `round-3-ask.txt`, with six screenshots re-attached.
- **2026-10-15, round 3** (canvas 1792071007-7a95, 188 boards in `round-3/`) checked item by item (one Sonnet agent,
  `round-3-check.md`): 44 done, 8 partly, 1 not done; no anonymity leak; **final, no fourth round.** For the build: R8a/R9
  thresholds drawn 5 / 3 / 2 (the page shows the campaign's own); the Spec's class table and `Distribution`'s removal;
  K10's body is `MsfCampaignText.WithdrawBody` as built; R8b's reason links the campaign page; PAED-015's tool list.
- **2026-10-15, step 6 begun.** Plan `build-lanes.md`, `build-contracts.md` (`6e842de3`): § 0 on `t369`, lanes K
  (campaign) and R (report, the one migration `T369_MsfCampaignClosedBy`) in wave 1, T (her reports, respondent, coverage)
  in wave 2, then the runbook lane. **F2's cast (the operator):** Sister Thandi Nkosi (`nkosi@kgk.wombat.local`, Nurse,
  Above expectations, "Escalates early and keeps the nursing notes in step") and Dr Johan van Wyk
  (`vanwyk@kgk.wombat.local`, Consultant, Above expectations, "Thorough on ward rounds; presents concisely"); both answer
  in step 3.42.
- **2026-10-15, step 6 done** (`68327c9d`, squashed from `t369`): § 0 (`dae634f0`), lanes R, K, T, the runbook lane;
  three Sonnet reviews (`build-review.md`: no high; R1 refusals classified as flow 09's, D2 the list hand-written to
  stack at 768, D3 no pronoun); the operator kept R7 (per-group responded counts while open); one fix pass. **For the
  replay:** stub-mail counts +3; the release mail's log names no recipient (T282); the D1 margins at 1280 and 390;
  3.34–3.50's Actuals and Gaps (T270, T325) are stale until rewritten.
- **2026-10-19, step 7 done.** Brief `d3379233`; published from `e598c490`; fresh `wombat_scenario_t369`. Acts 1–6 and
  the appendix: 31/19, 44/38, 60/58, 50/45, 29/23, 42/36, 75/62; every other Gap a known task, or the panel's member
  order (4.25, 4.41, 4.42, 5.8: **T370 proposed**). Flow 10's checks (D1 margins, dialogs' focus, `<h1>`, 390, the
  refusals' `role="alert"`, anonymity) pass. Old Gaps closed: T270, T325, F-3.50a, T329 (2.20), T260 (2.22), T290
  (2.37). T368 did not recur (5 of 5). States: 62 rows hold, 5 corrected; 3.43's stand-in SQL fixed (it needs
  `DeliveryLinkSelector`). Proposed: **T371** (comments in submission order, anonymity: needs the operator) and
  **T372** (an empty band on the campaign page). Scratch DBs `wombat_scenario_t369_st0..st3`, `_s515`.
- **2026-10-19, steps 8 and 9 done.** The live design system matched HEAD (174 files, three CRLF-only). Re-synced
  (`da19e284`: 7 records in a new MSF group, 16 revised); the re-sync found three page faults, fixed (`e4b480e2`: the
  campaign form's cards 24px apart, `.msf-form-cards`; the respondent's legend 1.1rem/600; two "Please"s; Web 3837 green;
  checked in the browser at 1280 and 390, the two states re-taken), and one kept (MSF coverage's long date, by
  `QuotaText.Iso`'s note). One Sonnet review (F1 must-fix, the preview's form class; F2–F8; `be67b0a7`). The two icons
  uploaded and in the index; published as **version 25** (1792413020-e2bb, `1f639c60`). Lessons in BRIEF § 11 "Flow 10"
  (`324c6abf`); the replay brief keeps scratch files out of the repo (`a81b0b73`).
