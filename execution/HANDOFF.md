# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## In progress 2026-09-24 (Opus) — finishing the EPA stream (the operator: "Not deploying yet")

- Housekeeping committed (`47a9aad`, `54c9805`): T118 and T104 closed by finding, T158–T171 filed, and
  `EPA-PROGRAMME.md` § 2 rewritten as the live queue, with § 4 "Order to finish the stream".
- **Shipped on master (not pushed), each browser-verified on dev:**
  - T127 + T143 + T148: create once, then navigate with a notice.
  - T138: released MSF only.
  - T107: a disabled action with its reason.
  - T125 + T136: rung pickers.
  - T137: EPA stamp and the list columns; its migration is applied on dev.
  - T135 + T150: sampling and trajectory, D44.
  - T113: trainee and MSF scope.
  - The smalls: T141, T144, T147 and T140 (integration is green).
- Master suites: Domain 398, Application 1112, Infrastructure 660, Architecture 31, Web 400, Integration 29.
- **Running in workflow worktrees:**
  - T160 (encounter-date bounds; migration lane). T162, T145 and T163 follow it, one migration at a time.
  - The second small batch: T175 + T180, T172, T177, T174.
  - The P1 security pair: T182, T183.
- **Filed today:** T172–T186. The P1s are T182 and T183 (cross-institution committee and entrustment admin).
- **Dev DB additions** from the browser checks:
  - activities 20–24;
  - institution 2 and coordinator.t113b@wombat.local (password not recorded; reset it if needed);
  - decision panel 1, committee review 1;
  - activity 23, a stuck `msf_cpsa` draft (T162).

## Earlier today (Opus) — T102, T105, T120, T149, T099 and W-007 shipped

The full handoff is in `execution/log/2026-09-24-t102-t149-handoff.md`. These traps still apply:

- **Label only what is STORED** (`ActivityForm.StoredDataJson`), because `GetUserOptionAsync` is unscoped by design.
- **A new seed's transitions declare `validation`;** copy `mini_cex_cpsa`.
- **Pinned seed lists live in five test files.** A new seed fails them by design; update them deliberately.
- **Restoring a file with `cp -p` keeps its mtime**, so MSBuild skips the rebuild. Touch it after.
- The audit trap, the `--no-build` rule, and "stop only `Wombat.Web.exe`" all still apply.
- Suites at the start of this session: Domain 356, Application 895, Infrastructure 582, Architecture 28, Web 288
  (2149). Integration: 22 of 23 (T140).
