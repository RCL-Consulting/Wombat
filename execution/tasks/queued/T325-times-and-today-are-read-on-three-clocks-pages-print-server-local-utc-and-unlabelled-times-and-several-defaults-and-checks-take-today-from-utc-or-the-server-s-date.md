---
id: T325
title: Times and "today" are read on three clocks: pages print server-local, UTC and unlabelled times, and several defaults and checks take today from UTC or the server's date
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T325 — Times and "today" are read on three clocks: pages print server-local, UTC and unlabelled times, and several defaults and checks take today from UTC or the server's date

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Printed times are off by two hours or carry no zone. "Today" is off by a day only between 22:00 and 24:00 UTC, which the replay did not cross. One behavioural case: an MSF respondent link is accepted for up to two hours after its South African last day.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-3.47b, F-A.1.2c).

## Symptom

- **Step 3.47:** one release shows as "Released 2026-09-26 12:53" on the trainee's MSF list (server-local, no zone) and "Recorded on the portfolio: 2026-09-26 10:53 UTC" on the coordinator's report (`design/baseline/act-3/3.47-1-my-msf-reports.png`, `3.46-1-report-released.png`).
- **Step 3.32:** `/admin/jobs` shows local run times beside "daily at 09:00 UTC" schedules (`act-3/3.32-1-nudge-run.png`).
- **Step A.1.2:** the requester's "Your requests" read 13:14 at 15:14 SAST. Between 22:00 and 24:00 UTC it names the previous South African day.
- **Found by code read:** "today" is not the South African date:
  - the STAR staging form's "Issued on" default;
  - My authorisations' "Expires in N days";
  - Export Portfolio's default range;
  - a new curriculum's effective date;
  - a new MSF campaign's open and close dates.
  Each would be a day behind between 22:00 and 24:00 UTC.

## Root cause

- **Printed times.** Fourteen renderings call `.ToLocalTime()`, which is the server's zone: dev is SAST, and INFRASTRUCTURE.md sets none for the box. They are `ActivityInbox.razor:34`, `ActivityView.razor:159`, `CollegesList.razor:37`, `InstitutionsList.razor:37`, `InvitationsList.razor:127,153`, `ScheduledJobRunsList.razor:66-67`, `ScheduledJobsList.razor:39,50`, `UserDetail.razor:188`, `MyMsfReports.razor:39,45` and `ActivityRowNames.cs:29`. `CampaignReport.razor:75` and `VerifyExport.razor:57` print labelled UTC. `DataRights.razor:108,137`, `RequestsList.razor:59` and `RequestDetail.razor:41,54` print the stored UTC with no zone. DESIGN has no rule for printed timestamps.
- **"Today" in Web.** `ReviewDetail.razor:2083` (`DateOnly.FromDateTime(DateTime.UtcNow)`), `MyAuthorisations.razor:39,132` (UtcNow), `ExportPortfolio.razor:55-56`, `CurriculumEdit.razor:166,168` and `CampaignEdit.razor:1194,1197` (`DateTime.Today`), and `CurriculumItemsEdit.razor:221-222,375-376` (UtcNow's year).
- **"Today" in Application.** `MsfCampaignRules.cs:704` (link expiry judged on the UTC date), `GetTraineeDashboardSummaryQuery.cs:111-112`, `GetCoordinatorDashboardSummaryQuery.cs:38-39`, `GetUserByIdQuery.cs:58`, and `AdoptCurriculum.cs:77`, which stores `AdoptedOn` as the UTC date.

DESIGN.md:326 requires `QuotaCalendar.Today(TimeProvider)` (`QuotaProgress.cs:13-20`, over `ProgrammeCalendar.DateOf`). Nothing enforces it.

## What to build

- **One rule, recorded in DESIGN § Page shapes:** a person reads a time in South African time, with the zone shown ("2026-09-26 15:14 SAST"). Every rendering goes through one helper over `ProgrammeCalendar` (UTC+2 fixed; South Africa has no DST), never `ToLocalTime()` and never the raw UTC value. Job schedules are described in the same zone. The data-rights and audit pages may keep the stored value where it is the record, but labelled. The MSF report and the verify page match what the PDF prints.
- **Every "today":** reads `QuotaCalendar.Today(TimeProvider)`, with the `TimeProvider` injected into Web pages and Application handlers, so a test can pin 22:30 UTC as `ActivityService` already does (T160).
- **A guard test:** fails on `DateTime.Today`, `DateTime.Now`, `DateOnly.FromDateTime(DateTime.UtcNow)` or `.ToLocalTime()` anywhere under `src/`, with a short commented allow-list for audit timestamps and the like.

## Verification

- [ ] The guard test is green. Mutation check: add one `.ToLocalTime()` and it fails.
- [ ] Tests with `TimeProvider` pinned at 22:30 UTC each read the South African date:
- the staging form's Issued on default;
- My authorisations' "Expires in N days";
- Export Portfolio's defaults;
- the MSF campaign's default dates;
- the curriculum's effective date;
- a respondent link on its last day (refused once the South African day has passed);
- `AdoptedOn`.
- [ ] bUnit: `MyMsfReports`, `ScheduledJobsList`, `ScheduledJobRunsList` and the data-rights pages print times in the one labelled form.
- [ ] Browser, runbook steps 3.46-3.47, 3.32 and A.1.2: the same release time reads the same on the trainee's list and the coordinator's report, `/admin/jobs` times match their schedule's zone, and "Your requests" shows the South African time, labelled.

## Related

DESIGN.md:326 (the "today" rule), T160 (`TimeProvider` in `ActivityService`), T209, T252, `ProgrammeCalendar`, `QuotaCalendar`, CLAUDE.md § EncounterDateGate (22:30 UTC is already tomorrow), runbook steps 3.32, 3.46, 3.47, 4.17, 4.38, 5.9 and A.1.2.
