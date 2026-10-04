# Flow 05 · The build's review (2026-10-04)

Four read-only reviewers on Sonnet (`review-lanes.md`) read `t355` at `be9d5ef0`: all five lanes merged, the committee
page's links wired, the runbook-and-docs lane merged, all six suites green (Domain 810, Application 3,660,
Infrastructure 1,011, Architecture 52, Web 3,211, Integration 484). Their lenses:
- **R**: correctness and security (the back end);
- **D**: the pages against the design and DESIGN.md;
- **A**: accessibility and focus;
- **G**: regressions outside flow 05, and how honest the tests are.

One high (A1, the integrator's own wiring), four medium (G1, G2, R1 with D4, and G3 counted with them), and the rest
low. No reviewer found a read that reaches another trainee's data by naming an id; Recent decisions follows E6's rule;
the counts, December, D48's pause and T325's "today" hold; every traced state's words match the boards and the Spec.

## The fix pass (stopping line: every high and medium, and every low that changes what a user sees, hears or can reach)

Branch `t355-fix`, from `be9d5ef0`.

**High**
- **A1.** The committee page's standing-panel EPA names linked to bare fragments (`#trajectory-<EpaId>`), which the
  page's `<base href="/">` resolves to `/#…`: a press navigated to Home, losing unsaved input, and the chart was never
  reached. The integrator's test checked the href string, not the press. Fix: an in-page link focuses the chart through
  the existing `PageFocus` helper (as `RefusalSummary`, T342 A1), keeping the href with no circuit.

**Medium**
- **G1.** A review window longer than a year (the pre-graduation review, 2023-01-15 to 2026-12-31) crowded 48 month and
  eight semester labels into the chart. D2 and the Spec never considered such a window. Fix: ticks and labels by the
  window's length.
- **G2.** Nothing tested that the committee page gives the chart its window, heading level, summary and no Today line.
- **G3.** The committee page's empty chart sentence was false once D2 bounded the read: it now names the window.
- **R1, D4.** "Activities on this EPA" stopped at 100 rows while its caption said "Every".

**Low, fixed** (they change what a user sees or reaches): R2 (the minimum before the programme start), R3 ("against the
minimum then" where credit makes no level judgement; its parity test now goes through credit), R4 (no hard-coded
"declined"), R5 and R6 (the chart's off-ladder step and right edge), R7 with G5 (E3's owner rule only for entries under
a personal link), D1 (the paused completed card lands on its EPA's page, as the board), D2 (the stacked row header
wraps at 390), A2 (Home's "Open My activities" at 44 px), A3 (an empty STAR cell reads "No decision"), G4 (the Credit
link after a curriculum move), G6 (CUSTOMIZATION.md), G7 (a test's name), G8 (a weak assertion).

**Deferred to the squash:** D3, DESIGN.md's banner gains the squash's hash.

The fix pass's commits and counts are recorded with the squash.
