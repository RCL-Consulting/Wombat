---
id: T295
title: The runbook has not been replayed end to end since v11.1, and no screenshot baseline of the GUI exists
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-09-26
completed: 2026-09-26
---

# T295 — The runbook has not been replayed end to end since v11.1, and no screenshot baseline of the GUI exists

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A runbook nobody has played is a hypothesis. The redesign needs a visual baseline of every page and
state, and none exists: 12 ad hoc PNGs in the gitignored `.playwright-mcp/`.
**Surfaced:** 2026-09-26, with T293.

## Symptom

Only Acts 1–2 were replayed since v11.1 (T159, 2026-09-25); Acts 3–5 were last played in June against the old world,
and the appendix in early June. Nothing captures what each page looks like.

## What to build

- `tools/scenario-replay.ps1`: create a replay database beside dev, publish and start the app against it on its own
  port, run SQL checks, dump and restore snapshots, without printing the database password.
- Replay every act and the appendix of the T293 runbook, in order, on a fresh database, with scripted Chrome as the
  seeded and invited accounts (devadmin for the Administrator, T292). Each step gets its `Actual` and `Gap` lines. A
  runbook error is fixed in place; an application defect is filed as a task or added to the open task it belongs to,
  and never fixed during the replay.
- A snapshot after each act (`recovery/scenario-post-act<N>.dump`).
- Screenshots of every step's outcome (`design/baseline/<act>/`) and of every state in `states.md`
  (`design/baseline/states/`), gitignored.

## Verification

- [x] Every step of every act has a dated `Actual` and a `Gap` line — 324 of 324; `grep -c "^Actual:$"` over the seven
  files returns 0 for each. Steps 2.26–2.27, which the first run's mail setup could not reach, were played again on a
  scratch copy with the log sender and are recorded so.
- [x] Snapshots exist for each act — `recovery/scenario-post-act{1,2,3,4,5,6,A}.dump`, and `scenario-act6-post-6D.dump`.
- [x] `design/baseline/` holds a screenshot per played step and per reachable state — 323 of 324 steps (A.6.10 is a
  `curl` of `/health`, with no page); 576 of 577 states (`my-progress--december` shows only when the replay day is in
  December, and `states.md` now says so).
- [x] Every Gap that names an application defect cites a task id — 128 finding ids carry `, T<nnn>`; the other four
  carry `not a defect` and the decision (D17 twice, T112, T213). `grep -oE "\[F-[^],]+\]"` finds none bare.

## As built — 2026-09-26

**The tool.** `tools/scenario-replay.ps1`: `create` an empty `wombat_scenario*` database, `publish` the app to
`.scenario-app/bin` (Debug, away from the dev app's locked `bin/x64/Release`), `start` it on its own port, `stop`,
`sql` (through a file, so Windows PowerShell 5.1 cannot strip the quotes of `"Table"` identifiers), `dump` and
`restore`. `start` overrides `Wombat:BaseUrl` and `Wombat:MsfRespondUrl` to its own port and `Email:SmtpHost` to empty:
`appsettings.Development.json` points links at :5080 and mail at `localhost:25`, which the first run found the hard way
(its first part started a Python SMTP sink on port 25 and read links off :5080; the sink is stopped).

**The replay.** Thirteen sequential parts in one scripted Chrome, on `wombat_scenario`, created fresh that morning,
signing in as the cast and as `devadmin` (T292). 324 of 324 steps played: 220 as expected, 104 with a gap. 17 steps
were corrected in place where the runbook was wrong. Three SQL writes, each one the runbook prescribes (3.30's ageing,
3.43's failed delivery, A.2.8's elapsed time); nothing else was stood in for. Every act's outcome SQL matched. The part
playing 4.A–4.D stopped at 4.6, and the next part played 4.6–4.27 itself before its own steps.
- The browser runs the operator's download manager, which takes over a download: the replay checked downloads from
  inside the page (A.1.5's zip was also checked in `Downloads\Compressed`). README § Downloads says how.

**The states sweep.** 109 states the replay did not reach were captured on scratch databases restored from the act
snapshots (`wombat_scenario_states` on :5181 from post-actA, `wombat_scenario_states4` on :5182 from post-act4): held
reads for loading and error states, a suspended or stopped app for the reconnect modal, deactivations, expired links.
Five `states.md` recipes were wrong and are corrected.

**The findings.** 132 from the replay, the 116 suspects T293's authors read from the code (87 confirmed, 4 refuted),
and 28 from the sweep were clustered, verified against the code by one agent per area, and filed:
- **35 new tasks, T297–T331: 15 P2 and 20 P3.** The P2s include T302 (an InstitutionalAdmin can deactivate her own
  institution), T303 (Add role makes a Trainee with no profile), T304–T305 (profile saves re-admit or rewrite an ended
  record), T311–T313 (a graduate's record and the portfolio's verification), T315 (the invitation token stays in the
  address bar), T319–T320 (promised emails never sent), T322 (contrast) and T329 (reads that crash the circuit).
- **46 notes on open tasks**, 29 from the replay and 17 from the sweep (T291, T272, T264, T286, T287, T289, T290, T323,
  T328 and others).
- **Four findings are not defects** and say why in their Gap lines.

**The baseline.** `design/baseline/` (gitignored): 672 step screenshots under `act-*` and 578 under `states/`.

## Related

T292, T293, T294, T159, T291.
