---
id: T244
title: bUnit tests that wait with the default one-second timeout fail when the machine is busy
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T244 — bUnit tests that wait with the default one-second timeout fail when the machine is busy

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Occasional red runs erode trust in the suite.
**Surfaced:** 2026-09-25. `CurriculumItemsRemoveTests.ASecondConfirmWhileTheFirstRemoveRuns_SendsNothing_AndRemoveIsOffMeanwhile(heldAt: Command)`
(T222) and `DecisionsDuePageTests.WhileTheQueryRuns_ThePageShowsSkeletons` (T194 run) each failed once under load and
passed on a rerun.

## What to build

Find why each needs more than a second. Where the test waits for a render that genuinely follows an awaited task, give
`WaitForAssertion`/`WaitForState` an explicit timeout in one shared constant (e.g. 5s), in the Web test base class. Do
not hide a real race: first check each flaky test for an unawaited task in the component.

## Verification

- [x] The Web suite passes 10 runs in a row while the Integration suite runs beside it.

## Related

T227, T222, T194.

Note, 2026-09-25 (the T210 run): `MsfInvitationAddressOnceTests.AnAddressTheCampaignAlreadyInvites_TypedAnyWay_IsRefused_AndNothingIsWritten(typed: "peer-9@example.test")`
(T228, Application) failed once in seven full runs, and passed alone. The error was not captured. It is an Application
test, not bUnit, so look for shared state.

---

## As built — 2026-09-25 (`2a2e67e`)

`WombatTestContext` carries one explicit `AsyncWorkTimeout`, used by every bUnit wait that follows real async work. The
flaky tests were each fixed at their cause.

**Evidence.**
- The implementer ran 10 Web suites while Integration and Application loops ran beside them: 0 failures.
- With 1.5 s slow-down probes, each fixed wait needed the timeout.
- In the reviewer's loaded run (20 busy loops on 16 cores), the old waits failed 4 of 4 full Web runs, on exactly the
  wait that was fixed.
- Stated plainly: the unfixed code also passed 10 of 10 under ordinary load, so the verification item alone could not
  tell them apart. The probes and the loaded run are the real evidence.

**Filed from the review:** [T262] (audit paging tie-break). The 53300 connection limit seen under parallel runs is
[T241].
