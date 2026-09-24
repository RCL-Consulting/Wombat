# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## In progress 2026-09-24 (Opus) — finishing the EPA stream (the operator: "Not deploying yet")

- Housekeeping committed (`47a9aad`, `54c9805`): T118 and T104 closed by finding, T158–T171 filed, and
  `EPA-PROGRAMME.md` § 2 rewritten as the live queue, with § 4 "Order to finish the stream".
- **Shipped on master (not pushed), each browser-verified on dev** (T184's MSF checks ran under T204):
  - T127, T143, T148, T138, T107, T125, T136, T137, T135, T150, T113, T141, T144, T147, T140, T175, T180, T172.
  - T177, T174, T160, T142, T173, T161, T158, T176, T182, T183, T169, T184, T167, T162, T204, T154, T200–T203.
- **Decisions adopted on recommendation** (all recorded in EPA-PROGRAMME § 3):
  - D33 part 1; D34 (link); D38(a); D44; D45 (unrated instruments are gated by their evidence EPA).
  - T160's credit-bearing scope; T174's "seeds pin on create only".
- **Master suites:** Domain 442, Application 1533, Infrastructure 767, Architecture 33, Web 593, Integration 52.
- **Running:**
  - Lane A: T131 slices 1–2.
  - Lane B: T165 (quorum, proposed D46), then T164 (learner feedback, D35).
  - T166, then T168 (committee views).
  - **T205 (P1: MSF respondents have no page)**, then T206.
- **Next:** T131 slices 3–6 (routing, agenda, entrustment-only reviews, the decisions-due page); T207 (MSF hash); T145,
  T163 and the P3 smalls. Then the College message (§ 3F, now 12 questions) and T157's deploy, which waits for your
  go-ahead.
- **Browser checks:**
  - The Playwright MCP server disconnected. Agents drive a scripted Chrome instead: `npm i playwright` in a scratch
    folder, with channel `chrome`.
  - Agents may not read the admin credential, so they use the DevUserSeeder accounts; there is now a dev Coordinator.
- **Dev DB additions:**
  - activities 20–34;
  - institution 2 and coordinator.t113b;
  - panel 1 and reviews 1–2;
  - MSF campaigns 4 and 5 (UnderReview);
  - types 20 and 21.

## Earlier today (Opus) — T102, T105, T120, T149, T099 and W-007 shipped

The full handoff is in `execution/log/2026-09-24-t102-t149-handoff.md`. These traps still apply:

- **Label only what is STORED** (`ActivityForm.StoredDataJson`), because `GetUserOptionAsync` is unscoped by design.
- **A new seed's transitions declare `validation`;** copy `mini_cex_cpsa`.
- **Pinned seed lists live in five test files.** A new seed fails them by design; update them deliberately.
- **Restoring a file with `cp -p` keeps its mtime**, so MSBuild skips the rebuild. Touch it after.
- The audit trap, the `--no-build` rule, and "stop only `Wombat.Web.exe`" all still apply.
- Suites at the start of this session: Domain 356, Application 895, Infrastructure 582, Architecture 28, Web 288
  (2149). Integration: 22 of 23 (T140).
