# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## In progress 2026-09-24 (Opus) — finishing the EPA stream (the operator: "Not deploying yet")

- Housekeeping committed (`47a9aad`, `54c9805`): T118 and T104 closed by finding, T158–T171 filed, and
  `EPA-PROGRAMME.md` § 2 rewritten as the live queue, with § 4 "Order to finish the stream".
- **Shipped on master since the morning (not pushed), each browser-verified on dev where reachable.** The core of the
  EPA stream is done:
  - T131 (all six slices: D38 evidence, cadence, routing, agenda, entrustment-only reviews, the decisions-due page).
  - T165 (quorum, D46), T166 (year targets and exit readiness), T167 (the evidence snapshot names each line).
  - T168 (MSF coverage) and T186 (coverage read from the evidence rows).
  - T154 (clinical audit and portfolio review; D34 closed, D45) and T164 (learner feedback, D35 closed).
  - T205 (the MSF respondent page), T206, T207, T163 and T214 (MSF privacy and links).
  - T160 (the D15 date bounds), T137, T135 (D44), T113, T182 and T183 (security), and 40-odd smaller tasks.
  - See `git log` and each task's "As built".
- **Master suites:** 4,806 tests (Domain 595, Application 2184, Infrastructure 885, Architecture 33, Web 997,
  Integration 112), all green.
- **Running:**
  - The committee chain: T215 (the planner reads STARs), then T213, T212, T216 and T194.
  - The browser check of polish batch F1 (T191, T197, T192, T189, T193).
- **Decisions adopted on recommendation**, all in EPA-PROGRAMME § 3; the operator may overrule any of them:
  - D33 part 1; D34 (a link); D35 (an MSF kind); D38(a); D44; D45; D46 (quorum 2, no Administrator bypass).
  - D47 (an interim 5-point MSF scale).
  - T131's O1–O8 defaults; T160's credit-bearing scope; T174 (seeds pin on create only).
- **Left for the operator or the College:**
  - T139 (WindowMonths; § 3F question 10), T170 (self-assessment; question 11), T209 (partial-end rule; question 4).
  - T146, T152 and T171, which are not needed for v11.1.
  - § 3F now holds 12 College questions.
- **Browser checks:**
  - The Playwright MCP server disconnected; agents drive a scripted Chrome instead (`npm i playwright`, channel
    `chrome`).
  - Agents may not read the admin credential, so the DevUserSeeder accounts are used: trainee, assessor, committee,
    committee2, coordinator and instadmin.
  - Email is caught by a local SMTP sink on port 25.
- **Dev DB:**
  - reviews 1–5 and panels 1–2 (Neonatal CCC);
  - MSF campaigns 4–9;
  - activities up to about 35;
  - institution 2 and coordinator.t113b.

## Earlier today (Opus) — T102, T105, T120, T149, T099 and W-007 shipped

The full handoff is in `execution/log/2026-09-24-t102-t149-handoff.md`. These traps still apply:

- **Label only what is STORED** (`ActivityForm.StoredDataJson`), because `GetUserOptionAsync` is unscoped by design.
- **A new seed's transitions declare `validation`;** copy `mini_cex_cpsa`.
- **Pinned seed lists live in five test files.** A new seed fails them by design; update them deliberately.
- **Restoring a file with `cp -p` keeps its mtime**, so MSBuild skips the rebuild. Touch it after.
- The audit trap, the `--no-build` rule, and "stop only `Wombat.Web.exe`" all still apply.
- Suites at the start of this session: Domain 356, Application 895, Infrastructure 582, Architecture 28, Web 288
  (2149). Integration: 22 of 23 (T140).
