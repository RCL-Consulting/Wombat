# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-21 (Opus) — T121: MSF leaves evidence behind

One application task shipped, one pre-existing defect fixed on the way, two filed.

### Done

- **[T121]** — a released MSF campaign now writes one terminal `msf_cpsa` activity per covered EPA into
  the trainee's ordinary activity, portfolio and committee path. **Browser-verified end to end on dev**,
  three times over: scope stamped from the subject, no respondent data in `DataJson`, `ObservedOn` from
  the real close date, a dropped EPA reported as dropped.
- **Per D8, nothing is credited.** `"counts_for": []`, so no `CurriculumItemProgress` row moves and
  `CreditedItemCount` stays null — T108's "never evaluated". The task file's Verification section assumed
  the opposite; it is rewritten, with an **As built** section listing every divergence from the design.
- **D11 lives in `MsfAggregationService.BuildReport`**, so the disabled button and the server refusal
  cannot disagree. Browser-verified blocked and unblocked.
- **Found and fixed on the way: the portfolio export threw for any trainee with a released MSF
  campaign.** `PortfolioPdfService` never included `Responses.Invitation`, which
  `MsfAggregationService` dereferences on its first line. Invisible because the only two portfolio tests
  inject a `ThrowingMsfAggregationService`. Guard **verified to fail against the unfixed code**.
- **An adversarial review of the diff (6 reviewers, 3 refuters each) found six real defects, all
  fixed** and all written up on the task file. The three that mattered: the scope check was on create,
  not on **release**, which is the half that writes; a failed fan-out would have **committed the release
  anyway** via the audit catch; and the reviewer's ordinal was a bare number box, so rung "4" stored
  rung "3b" (the T100 trap).

### Filed, not fixed

- **[T137] P2** — N EPAs give N rows on `/activities/mine` reading `Type / State / Updated` and nothing
  else. Not MSF-specific: `ActivitySummaryDto` has no EPA and no date, and `Activity.EpaId` is written by
  nothing.
- **[T138] P3** — the committee evidence snapshot has no state filter on MSF campaigns, so a draft, open
  or **withdrawn** campaign is shown to a panel as evidence.
- **[T135] updated** — `msf_cpsa` is a live instance of its defect 2, and folding it into
  `WithheldRatedActivities` would be the wrong fix.

### Next

1. **[T130] (the annual quota)** is the most visible gap left and is unblocked. [T121] writes
   `ObservedOn` from the real close date precisely so phase 3 can bucket it.
2. **[T120]** (the ten remaining v11.1 tools) and **[T122]** (enforce the EPA→tool mapping).
3. **Ask the two residual College questions** — the "clinical observed interaction" merge scope, and
   whether June is semester 1 or 2. Neither blocks starting; both block finishing.
4. **[T128]** — still blocked on you: off-host destination + `age` key holder.

### Traps

- **`AuditWriter` shares the request's scoped `IApplicationDbContext` and calls `SaveChangesAsync`, and
  `AuditPipelineBehavior` writes an audit row from its `catch`.** Any exception thrown while the context
  holds a half-finished mutation therefore COMMITS that mutation on the way out. This is not MSF-specific
  and it is not written down anywhere else: **any handler that mutates then validates is committing on
  failure.** Worth a sweep.
- **`msf_cpsa` is in the rated set (nine seeds, not eight)**, unavoidably while D10's optional ordinal
  exists: `Schema_DeclaresARatedFieldExactlyWhenItCarriesAScale` is a biconditional. It is invisible on
  both surfaces that read that set, because neither finds an `assessor_user_id`. Recorded against D10.
- **Do not add `msf_cpsa` to `CpsaWbaSeedTests.SeedKeys`** — its theories assume the
  request → assess → feedback shape. `MsfSeedTests` is its guard; a comment says so there.
- **`ClaimsPrincipalExtensions.CanAccessInstitution` returns false for a Coordinator**, whatever their
  institution: it admits only Administrator and InstitutionalAdmin.
- **`ClaimsPrincipal.IsInRole` is the BCL instance method**, not the extension, so it reads the
  identity's `RoleClaimType`. A test principal built without it matches `role:Coordinator` for nobody.
- **The dev trainee is now on the paediatric curriculum** (`TraineeProfiles.CurriculumId` 1 → 2). Dev
  holds MSF campaigns 1–3 and activities 1–6. Campaign 1 was released through the pre-fix number box, so
  its level 4 means rung "3b"; 2 and 3 went through the rung picker and are right.
- Hooks hard-code `C:\dev\rcl_execution\bin\harness.py` (`main`). Never pass `--no-build`.

### Verification status

- `dotnet build Wombat.sln -c Release` — **0 warnings, 0 errors**.
- Suites green, no `--no-build`: Domain **78**, Application **559**, Infrastructure **269**,
  Architecture **23**, Web **111** — **1040 total**, up from 1009. Integration is Docker-gated and was
  not run; its MSF flow test was rewritten for the new command shapes and compiles.
- Two migrations, both `dotnet ef`-generated and **applied to the dev database**. Each carries one
  commented hand-edit: `MinimumRespondentCategories` back-fills as 2 not 0, and EF's
  `AddColumn("xmin")` was **removed** — `xmin` is a Postgres system column, so adding it fails.
- The portfolio-export guard was verified to fail against the unfixed code.
- `harness.py lint` clean. Not re-verified: `drift-check.sh`, `restore-rehearsal.sh`.
