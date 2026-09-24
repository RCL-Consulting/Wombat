# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## In progress 2026-09-24 (Opus) — finishing the EPA stream (the operator: "Not deploying yet")

- Housekeeping committed (`47a9aad`, `54c9805`): T118 and T104 closed by finding, T158–T171 filed, and
  `EPA-PROGRAMME.md` § 2 rewritten as the live queue, with § 4 "Order to finish the stream".
- Shipped on master (not pushed): **T127 + T143 + T148** `5fbc8c2` (create once, then navigate with a notice; the
  server checks a create that is itself the filing), **T138** `29aad11` (released MSF only, windowed by the close day),
  **T107** `6934294` + `5f5e097` (disabled action with its reason; browser, activity 23). Filed T172, T173. All five
  suites green on master: 2,221.
- Running in workflow worktrees, squash-merged as each lands: T125 (+T136), T137, T135 (+T150), T113, and the smalls
  T141, T144, T147 and T140. T145, T162 and T163 wait for T137's migration to merge, so the snapshots do not collide.
- Dev DB: activities 20–23 are this session's browser checks. 23 is a stuck trainee-created `msf_cpsa` draft (T162).

## Earlier today (Opus) — T102, T105, T120, T149, T099 and W-007 shipped

The full handoff is in `execution/log/2026-09-24-t102-t149-handoff.md`. These traps still apply:

- **Label only what is STORED** (`ActivityForm.StoredDataJson`), because `GetUserOptionAsync` is unscoped by design.
- **A new seed's transitions declare `validation`;** copy `mini_cex_cpsa`.
- **Pinned seed lists live in five test files.** A new seed fails them by design; update them deliberately.
- **Restoring a file with `cp -p` keeps its mtime**, so MSBuild skips the rebuild. Touch it after.
- The audit trap, the `--no-build` rule, and "stop only `Wombat.Web.exe`" all still apply.
- Suites at the start of this session: Domain 356, Application 895, Infrastructure 582, Architecture 28, Web 288
  (2149). Integration: 22 of 23 (T140).
