# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## In progress 2026-09-24 (Opus) — finishing the EPA stream (the operator: "Not deploying yet")

- Housekeeping committed (`47a9aad`, `54c9805`): T118 and T104 closed by finding, T158–T171 filed, and
  `EPA-PROGRAMME.md` § 2 rewritten as the live queue, with § 4 "Order to finish the stream".
- **Shipped on master (not pushed), each browser-verified on dev:** T127, T143, T148, T138, T107, T125, T136, T137,
  T135, T150, T113, T141, T144, T147, T140, T175, T180, T172, T177, T174, T160, T142, T173, T161, T158, T176, T182, T183.
  - D44 was decided (T135). T160 added the credit-bearing scope.
  - Migrations: T137, T160, T174 (data only) and T182, all applied on dev.
- **Master suites:** 3145 tests (Domain 428, Application 1410, Infrastructure 671, Architecture 31, Web 566, Integration
  39).
- **Running:**
  - Migration lane: T167, then T162.
  - T169 and T184.
  - T131's design workflow: three proposals, judged, then synthesized.
- **Next:**
  - T166 and T168 (committee views), T154 (clinical audit + portfolio review on D34/shape recommendations), T164 (D35),
    T165 (quorum), then T131 per its design.
  - Then the smalls T186–T199 and T145/T163 (migration lane).
- **Adopted on standing recommendations** (the operator may overrule; listed in EPA-PROGRAMME):
  - D33 part 1; D44; D38(a) for T131.
  - T160's credit-bearing scope; T174's "seeds pin on create only".
- **Dev DB additions:**
  - activities 20–31;
  - institution 2 with coordinator.t113b (password not recorded);
  - panel 1 and review 1;
  - MSF campaign 4 (UnderReview);
  - activity 23 (a stuck msf draft; T162 removes it).

## Earlier today (Opus) — T102, T105, T120, T149, T099 and W-007 shipped

The full handoff is in `execution/log/2026-09-24-t102-t149-handoff.md`. These traps still apply:

- **Label only what is STORED** (`ActivityForm.StoredDataJson`), because `GetUserOptionAsync` is unscoped by design.
- **A new seed's transitions declare `validation`;** copy `mini_cex_cpsa`.
- **Pinned seed lists live in five test files.** A new seed fails them by design; update them deliberately.
- **Restoring a file with `cp -p` keeps its mtime**, so MSBuild skips the rebuild. Touch it after.
- The audit trap, the `--no-build` rule, and "stop only `Wombat.Web.exe`" all still apply.
- Suites at the start of this session: Domain 356, Application 895, Infrastructure 582, Architecture 28, Web 288
  (2149). Integration: 22 of 23 (T140).
