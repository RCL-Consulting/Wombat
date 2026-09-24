# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## In progress 2026-09-24 (Opus) — finishing the EPA stream (the operator: "Not deploying yet")

- Housekeeping committed (`47a9aad`, `54c9805`): T118 and T104 closed by finding, T158–T171 filed, and
  `EPA-PROGRAMME.md` § 2 rewritten as the live queue, with § 4 "Order to finish the stream".
- Running now: T127 with T143 and T148 on branch `t127-new-activity-submit-flow` (create once, then navigate). T138 and
  T107 are in workflow worktrees; each is squash-merged when it lands.

## Earlier today (Opus) — T102, T105, T120, T149, T099 and W-007 shipped

The full handoff is in `execution/log/2026-09-24-t102-t149-handoff.md`. These traps still apply:

- **Label only what is STORED** (`ActivityForm.StoredDataJson`), because `GetUserOptionAsync` is unscoped by design.
- **A new seed's transitions declare `validation`;** copy `mini_cex_cpsa`.
- **Pinned seed lists live in five test files.** A new seed fails them by design; update them deliberately.
- **Restoring a file with `cp -p` keeps its mtime**, so MSBuild skips the rebuild. Touch it after.
- The audit trap, the `--no-build` rule, and "stop only `Wombat.Web.exe`" all still apply.
- Suites at the start of this session: Domain 356, Application 895, Infrastructure 582, Architecture 28, Web 288
  (2149). Integration: 22 of 23 (T140).
