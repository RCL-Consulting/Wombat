---
id: T298
title: The committee's Targets card orders tied trainees by user id, and the trainee's Upcoming deadlines card reads a field no instrument has
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-10-05
completed: 2026-10-05
---

# T298 — The committee's Targets card orders tied trainees by user id, and the trainee's Upcoming deadlines card reads a field no instrument has

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. One is an order no reader can follow among equal rows. The other is a card that promises deadlines and is always empty, so it tells a trainee nothing.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-2.33a).

## Symptom

- Step 2.33: when every registrar has met the same share, the committee dashboard's 'Targets this period' lists them as Anele Dlamini, Lerato Molefe, Sipho Ndlovu, Nomsa Mahlangu, Pieter du Plessis (all 0/10 · 0/5). That is neither surname nor first-name order. Naidoo and Botha see the same order (Step 2.35; design/baseline/act-2/2.33-1-zulu-committee-home.png).
- Step 3.50: the Trainee dashboard's 'Upcoming deadlines' reads 'No deadlines in the next 14 days.' (design/baseline/act-3/3.50-1-dlamini-dashboard.png), and it can never read anything else. states.md § States no local replay reaches and coverage.md list 'Upcoming deadlines with a row' as unreachable. The replay cannot prove the negative; the code does.

## Root cause

- CurriculumCoverage.cs:241-244 orders trainees by the share of targets met, then by TraineeUserId, which is a GUID. The primary order is deliberate: fewest met first, the trainees a committee needs to see. The tie-break is not. The coverage reader has no names. GetCommitteeMemberDashboardSummaryQuery.cs:70-81 resolves them afterwards and keeps the order, and it is the only consumer of CurriculumCoverage.Trainees.
- GetTraineeDashboardSummaryQuery.cs:111-148 scans each unfinished activity's DataJson for a property whose name contains 'due_date'. No schema under src/Wombat.Infrastructure/Activities/Seeds declares one, the KGK Teaching Session Log the runbook builds has none, and nothing else writes one. TraineeDashboardSummaryDto.cs:14,50 carries the result and TraineeDashboard.razor:138-155 renders it.

## What to build

- Break the committee card's ties by name. Keep the share met first, as now. Then order by surname and first name, case-insensitively, as ListTraineesForSpeciality and ListMsfCampaignSubjects do. Use the user id only for identical names. Names exist only once UserDisplayNames resolves them, so either order in the committee query or give CurriculumCoverage the names. Leave the EPA list's order alone.
- Decide the deadlines card. Either remove it, with its DataJson scan, UpcomingDeadlineItem and the DTO field, or give it a source the product already holds, such as:
  - the current quota window's end for a target not yet met (QuotaCalendar, TraineeQuotaProgress);
  - a committee review scheduled for the trainee;
  - a STAR nearing expiry (the data behind EntrustmentDecisionExpiringReminderEmail).
  Removing it is acceptable; a card that can never fill is not. Either way, update states.md, coverage.md and Step 3.50, and T280's deadline-link item if the card goes.

## Verification

- [x] A CurriculumCoverage or committee dashboard handler test: three trainees at the same share come back in surname order, and a lower share comes first whatever the name. (`CommitteeMemberDashboard_OrdersEqualSharesBySurnameThenFirstName_AndAFewerShareFirstWhateverTheName`: eight trainees, ids that sort the other way, two of one name; fails on the old code.)
- [x] Browser, Step 2.33: all five at 0/10 read Anele Dlamini, Pieter du Plessis, Nomsa Mahlangu, Lerato Molefe, Sipho Ndlovu for Zulu, Naidoo and Botha. (2026-10-05, Dr Zulu and Dr van Rensburg on `wombat_scenario_t290`, restored from `scenario-t355-post-act2`; Naidoo and Botha read the same query.)
- [x] The deadlines card, if removed: bUnit shows TraineeDashboard renders no 'Upcoming deadlines', and the query no longer reads DataJson. If kept: a handler test gives it a row from the chosen source, and a browser check at Step 3.50 shows that row. (Removed by T355, flow 05, `b020c942`: Q3.)
- [x] states.md, coverage.md and Step 3.50 are updated to match. (Flow 05 for the card; Step 2.33's Expect names the order, 2026-10-05.)

## Related

T130 (the coverage reader), T238 (the committee card's trainees), T280 (the trainee dashboard's deadline links), D15 (a different 'deadline': the late-filing warning). Runbook steps 2.33, 2.35 and 3.50. Author's suspect at 3.50: 'Upcoming deadlines can never show anything'.

- **2026-10-05 (T358's Q8, before flow 06's build).** The tie-break is in the committee query, not in `CurriculumCoverage`: it reads `GetContactsAsync` (first and last name apart; "Pieter du Plessis" cannot be split) and orders share, surname, first name, id. Landed with T290's committee item.
