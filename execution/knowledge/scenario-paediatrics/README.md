# Wombat end-to-end scenario: Paediatrics at Kgosi Kgari

A time-phased runbook that walks a realistic South African paediatric training programme through Wombat, from an empty
install to a registrar's graduation and beyond. It has three jobs:

1. **The test corpus.** Every step has an expected outcome, and replaying it catches integration gaps that unit tests
   miss. The rows it produces are the scenario data dev and production hold (CLAUDE.md § "Nothing is live").
2. **The journey catalogue.** Every page Wombat serves is played by some step, or listed in `coverage.md` with the
   reason it is not. `coverage.md` indexes every job each role does. That index is what a GUI redesign is briefed from,
   and replaying a redesigned flow's steps is its acceptance check.
3. **Training material, later.** The steps are written as intent and outcome, so they survive a redesign and read as
   "how do I…" for each role.

Rewritten 2026-09-26 (T293) from the single file that T091 and T159 had re-baselined. That file, with every recorded
`Actual:` and `Gap:` line since May 2026, is kept whole at `execution/log/scenario-paediatrics-history-2026-09.md`.

## Files

| File | What it holds |
|---|---|
| `README.md` | This: how to play, the step format, the cast and the world. |
| `act-1-setup.md` | Day 0. The Administrator creates KGK and invites the CollegeAdmin and Prof Mbatha; the seeded national catalogue is checked; KGK adopts it and builds one activity type. |
| `act-2-onboarding.md` | Day 1. Staff and registrars are invited, register, get their roles and profiles, and are admitted; the review panel is formed; each role's first sign-in. |
| `act-3-operations.md` | Months 1–6. Workplace-based assessments filed, rated, declined, returned and re-submitted; procedure logs; an MSF campaign from set-up to release; stalled work; progress and dashboards; the audit trail. |
| `act-4-annual-review.md` | Month 12. Reviews scheduled and sat; STARs staged, decided and ratified; an appeal lodged and resolved; entrustment decisions and what is due. |
| `act-5-graduation.md` | Year 4. The final review, the portfolio PDF and its verification, the programme's end, and the graduate's read-only record. |
| `act-6-catalogue.md` | After graduation. The College and KGK maintain the catalogue: a College, speciality, EPA, curriculum and scale created or edited, an EPA paused and restored, local additions. |
| `appendix-cross-cutting.md` | Data rights, scheduled jobs, SSO, account self-service, system pages, platform operations, narrow viewports and accessibility. |
| `coverage.md` | Every routable page → the steps that play it; what is not played and why; the role-by-job index. |
| `states.md` | For each page, the states a user can meet and how to reach each one from a step's database state. |

## How to play

- **Linear.** Start at Act 1 and work forward. Each act assumes the ones before it completed. Act 6 and the appendix run
  last because they change shared state (a paused EPA, an erased account).
- **A fresh database.** Create an empty PostgreSQL database and point `ConnectionStrings__DefaultConnection` at it.
  Then start `Wombat.Web` with `ASPNETCORE_ENVIRONMENT=Development`. The migrations run at start-up, then the seeders:
  - `DataSeeder` creates the Demo College and Demo Institution with their Internal Medicine catalogue.
  - `PaediatricCatalogueSeeder` creates the CPSA v11.1 catalogue that Act 1 checks, with its `*_cpsa` instruments.
  - `DevUserSeeder` creates the dev accounts at the Demo Institution, and `devadmin@wombat.local`, the Administrator
    this runbook signs in as on dev (T292).
  - `AdminSeeder` creates the bootstrap Administrator, but only where it is configured.

  The Demo College, the Demo Institution and their dev accounts are ignored throughout.
- **`tools/scenario-replay.ps1`** does all of this beside the dev app without printing the database password: `create`
  an empty `wombat_scenario*` database, `publish` the app away from the dev app's locked output, `start` it on its own
  port (5180 by default), run `sql` checks, and `dump` or `restore` a snapshot.
- **Mail.** `appsettings.Development.json` sends mail to an SMTP server on `localhost:25` and builds every emailed link
  from `Wombat:BaseUrl`, `http://localhost:5080`. So `start` overrides both on the command line: `Email:SmtpHost` to
  empty, which writes each email's body to the log (`.scenario-app/<db>.log`, a "Stub email" line with the subject and
  the text), and `Wombat:BaseUrl` to the replay's own address. That is where the registration and MSF links come from,
  and where a step's "is emailed" is checked. The log sender is also what lets an invitation read "Not delivered" and
  be resent (Step 2.26); an SMTP server that accepts the mail never gets there.
- **Downloads.** The browser a replay drives may be the operator's own, where a download manager can take over a
  download before it reaches the browser, so a missing file proves nothing. Check a download from inside the page
  instead: `fetch` its link with the page's cookies and read the status, the `Content-Type`, the
  `Content-Disposition` and the size; for a hash, run `crypto.subtle.digest('SHA-256', …)` over the body. Only a bad
  response is a finding. On the operator's machine the download manager saves to `%USERPROFILE%\Downloads\Compressed`,
  where a file's contents can be checked afterwards.
- **Passwords.** Accounts the cast registers get passwords chosen at replay time. They are written to the gitignored
  `pwd_DO_NOT_COMMIT.txt` and nowhere else, never into this runbook: the same steps are played on the production host.
- **The clock.** Wombat reads the real clock. The story's dates are narrative only. The current semester, a registrar's
  training year, the encounter-date bounds (T160), late filing (D15) and "overdue" all read today's date. So every
  date a step types is written relative to the replay day `D`, for example "an encounter on `D−10`". The January
  intakes are written from `J`, the latest 15 January on or before `D`: Molefe starts on `J−3y`, so she is in her
  fourth year on any replay day, and her programme ends on `J+1y−1d` (Act 2). Where the story
  needs a date the product would refuse (a future encounter, one before the programme start), the step says so and
  expects the refusal.
- **Snapshots.** After each act, `pg_dump -Fc` the database to `recovery/scenario-post-act<N>.dump`, so a later act
  can be replayed without the earlier ones.
- **Screenshots.** A replay captures each step's outcome into `design/baseline/<act>/<step>-<n>-<slug>.png`
  (gitignored), and `states.md`'s states into `design/baseline/states/`. That is the redesign's visual baseline.

## The step format

```
### Step 3.4 — Dr Naidoo rates Dr Dlamini's Mini-CEX
Role: Assessor — Dr David Naidoo
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Dlamini's Mini-CEX from the inbox, rate every item on the six-rung ladder, add feedback, and complete it.
Expect: The activity reads Completed and is read-only to both of them. Dr Dlamini is emailed that it was completed.
  Her progress for the EPA it credits counts one more this semester (checked in Step 3.6).
Actual:
Gap:
```

- **`Role`** is the Wombat role acting, then the person. For an anonymous visitor, write `Anonymous — <who>`; for work
  the system does, `System — <job or trigger>`.
- **`Route`** lists the pages visited, in order, separated by ` → `. Each is written **exactly as its `@page` directive
  declares it**, parameters and constraints included (`/committee/reviews/{ReviewId:int}`), or as a mapped endpoint
  (`/dashboard/switch/{role}`). A step with no page writes `n/a`. T294's test holds every route here to a real page or
  endpoint, and every page to at least one step.
- **`Do`** is the intent: what the person sets out to do and with what data. Name fields, options and values by what
  they mean ("the six-rung ladder", "the encounter date `D−10`"), never by position or control ("the third button").
- **`Expect`** is the outcome a person could check: what the page shows, what is now true, who is told, and what is
  refused. Quote on-screen words only where the wording is itself a product decision (for example the "n of m trainees
  met this period's target" rule, or a refusal's text), and say so. Expectations come from the product's decisions
  (EPA-PROGRAMME § 3, the task files' As-built sections) and are checked against the code. Where the code does something
  else, the expectation says what was decided, and the replay records the difference as a `Gap`.
- **`Actual`** and **`Gap`** are empty until the step is played. A replay writes
  `Actual (<date>, <replay>): …` and `Gap: none` or the finding with its task id.
- A continuation line is indented two spaces. A step may carry one optional `Note:` line for a precondition, a caveat
  or a pointer to a decision.

Steps are numbered `<act>.<n>` in play order; the appendix numbers `A.<section>.<n>`. Phases (`## Phase 3.B — …`)
group steps that serve one purpose. Each act file opens with its **scenario date, who acts, why, its starting state and
its goal**, and closes with its **outcome state** (with SQL checks where a count proves it) and a **handoff** to the
next act. It carries no findings section: findings become tasks.

## Findings

- **The runbook is wrong** (a route, an expectation the product changed, a step that cannot be played as written): fix
  the step in place and say so in `Gap`.
- **The application is wrong**: record it in `Gap` and file a task (`harness.py task new`), or add it to an open task
  it belongs to. Never fix application code during a replay. The replay's job is to find, not to fix.
- **Not built**: a feature no page offers yet (the nav links to no unbuilt page), or a flow that needs something the
  environment lacks (an SSO provider),
  is listed in `coverage.md` with its reason. It is not played.

## Cast

All hypothetical. All would fit in one real South African teaching hospital's paediatric department. Every address is
on a `wombat.local` domain so that no email can reach a real person.

| Wombat role | Real-world role | Person | Account | Acts |
|---|---|---|---|---|
| Administrator (global) | Platform operator | — | `devadmin@wombat.local` on dev (T292); the bootstrap admin elsewhere | 1, 6, appendix |
| CollegeAdmin (CPSA) | CMSA College registrar | Dr Anton Kruger | `kruger@cmsa.wombat.local` | 1, 6 |
| InstitutionalAdmin (KGK) | Head of Department | Prof Nolwazi Mbatha | `mbatha@kgk.wombat.local` | 1, 2, 4, 5, 6 |
| SpecialityAdmin (KGK, Paediatrics) | Programme director | Dr Refilwe Mokoena | `mokoena@kgk.wombat.local` | 2, 3, 4 |
| SubSpecialityAdmin (KGK, Paediatrics) | Sub-speciality training lead | Dr Kabelo Sithole | `sithole@kgk.wombat.local` | 2, 3, 4 |
| Coordinator | Programme coordinator | Mr Pieter Smit | `smit@kgk.wombat.local` | 2, 3, 4, 5, appendix |
| CommitteeMember + Assessor | Senior consultant, panel chair | Dr Thandi Zulu | `zulu@kgk.wombat.local` | 2, 3, 4, 5 |
| CommitteeMember + Assessor | Senior consultants | Dr David Naidoo, Dr Sarah Botha | `naidoo@…`, `botha@kgk.wombat.local` | 2, 3, 4, 5 |
| Assessor | Consultants | Dr Mohammed Patel, Dr Fatima Khumalo | `patel@…`, `khumalo@kgk.wombat.local` | 2, 3 |
| CommitteeMember (external) | External examiner, Stellenbosch | Dr John van Rensburg | `vanrensburg@sun.wombat.local` | 2, 4 |
| Trainee, year 4 (Jan 2023 cohort) | Final-year registrar | Dr Lerato Molefe | `molefe@kgk.wombat.local` | 2–5; a former trainee after Act 5 |
| Trainee, year 3 (Jan 2024) | Registrar | Dr Anele Dlamini | `dlamini@kgk.wombat.local` | 2–4 |
| Trainee, year 2 (Jan 2025) | Registrar | Dr Pieter du Plessis | `duplessis@kgk.wombat.local` | 2–4 |
| Trainee, year 1 (Jan 2026) | Registrars | Dr Nomsa Mahlangu, Dr Sipho Ndlovu | `mahlangu@…`, `ndlovu@kgk.wombat.local` | 2–4 |
| PendingTrainee | Each registrar between registration and admission | — | — | 2 |
| Anonymous — MSF respondents | Colleagues rating a registrar | named in Act 3 | a link in an email, no account | 3 |
| Anonymous — a verifier | Someone checking a portfolio PDF | — | no account | 5 |

The Administrator appears only where nobody else may act: creating KGK, inviting the CollegeAdmin and Prof Mbatha,
the platform's own pages, and the catalogue's College-level records.

## The world

- **The College** (owns the catalogue): the College of Paediatricians of South Africa (CPSA), a constituent College of
  the CMSA. It owns the Paediatrics speciality, its Paediatrics sub-speciality, the six-rung `CPSA Paediatric
  Entrustment Scale v11.1` (`1`, `2`, `3a`, `3b`, `4`, `5`), the 15 EPAs PAED-001 to PAED-015 of catalogue version 11.1,
  and the `Paediatric EPA Curriculum` version 11.1. `PaediatricCatalogueSeeder` creates all of it.
- **The institution** (adopts the catalogue): Kgosi Kgari Teaching Hospital (KGK), Mahikeng, North West. It adopts
  curriculum 11.1 and admits its registrars against the adoption.
- **The programme**: four years, two registrars admitted each January. The five in the cast are the 2023 to 2026
  intakes, each admitted on 15 January, which counts as a start on the semester boundary (D42).
- **The story's calendar**: Act 1 is Monday 12 January 2026, Act 4's sitting is December 2026, and Act 5 is late
  December 2026, the last month of Dr Molefe's four years. Act 6 is early 2027. It is narrative only (see "The clock").

## Terms

- **EPA**: Entrustable Professional Activity, a unit of work a registrar must be trusted to perform unsupervised.
- **Curriculum item**: an EPA's requirement in one curriculum: a target per period, the level to reach, the tools that
  may evidence it, and how often it is decided.
- **Activity type**: a schema-driven instrument (form, workflow and credit rules) on the Activity platform. An
  **activity** is one filing of it: "Dr Dlamini's Mini-CEX on `D−10`".
- **WBA**: a workplace-based assessment, the activities an assessor rates (Mini-CEX, CbD, DOPS and the rest).
- **MSF**: multi-source feedback, a campaign in which colleagues rate a registrar anonymously.
- **STAR**: a Statement of Awarded Responsibility, the committee's formal entrustment of a registrar with an EPA at a
  level.
- **Entrustment decision**: the recorded decision, from a committee sitting, that a registrar may perform an EPA at a
  level, with its expiry.
