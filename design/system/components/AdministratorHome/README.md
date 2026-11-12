# AdministratorHome

The Administrator's Home (`Pages/Dashboards/AdministratorDashboard.razor`, drawn by Home for the acting role Administrator; its words `AdministratorHomeWords.cs`; the read `GetAdministratorDashboardSummaryQuery`, `AdministratorDashboardSummaryDto`; the mail half `IEmailHealth` and the `EmailOutcomes` table; the jobs' rule `Wombat.Application/Scheduling/JobSchedule.cs`; the addresses `JobLinks`, `PeopleLinks`): **two cards, System health and Accounts, each saying in words what holds and what needs the Administrator.** Flow 18 (T400, 2026-11-11, `34b5077e`, with `a7d14b19`: each time in a sentence one unit; boards R3-H01 to H12; Q1 to Q4, Q13, Q14, Q18, T8, T9, T13, T14, S1, S2, S5, K3, K4, K11) rebuilt it. Until then System health was three status dots, two of them stubs that always showed `warn` labelled with task ids ("Email queue (T012)", "Last nightly job (T024)"), and a "Users across institutions" figure counted distinct ids across the roles. There is no System page (Q1): Home is the one place the platform's health is read.

## Who reads it

The Administrator (`[Authorize(Roles = "Administrator")]`; the query refuses anyone else, "Only a Wombat administrator may read the system's health.", audit text only). Home's header is every Home's: "Home", "Administrator · Semester 2, 2026", no header action (DashboardCard).

## The page

One read behind `DashboardFrame` (its loading and its failure with Try again are every Home's), then `.dashboard-grid` with two `DashboardCard`s, each a `section` named by its `h2`:

- **System health** (`settings`, `#card-health`, `Span="2"`): `ul.health-list`, three `li.health-line` in fixed order, **Database, Mail, Scheduled jobs** (T8). Each line is `span.health-line-name` (the name, a visually hidden ":" so it reads "Database: OK", N7, then its badge) beside `div.health-line-text`, its sentences as paragraphs and, when several jobs need the Administrator, a list under "Three jobs need you:".
  - **The state is a word on a badge, never a dot** (S5, T327): OK `badge-completed`; Failing and Unreachable `badge-declined`; Not configured and Disabled `badge-accepted` (N3); Overdue `badge-overdue`; Not read `badge-draft`. The jobs' badge takes the worst word: Failing, then Overdue, then Disabled.
  - **Database**: "The database answers." or, Unreachable, "The database did not answer at 21:39 SAST, so Wombat can read and save nothing, and /health says Unhealthy."
  - **Mail** reads the mail outcomes table (Q18: one row a mail finished with, its kind and moment and a refusal's words, never an address or a subject; kept 90 days), so a restart keeps it: "Mail is sent. Last sent 2026-11-12 08:37 SAST; none waiting; none dropped in the last 24 hours." ("2 waiting" while the queue holds mail); "Nothing sent since ‹moment› SAST; …" when nothing was sent in the 90 days; Failing, "1 mail was dropped in the last 24 hours, the last at ‹moment› SAST: ‹the refusal›. The application log names each one." (no refusal clause for a mail abandoned as the host stopped, K3); Not configured, "Mail is written to the log, not sent." and "To send mail, set Email__SmtpHost on the server and restart Wombat.", which needs no database and so still answers when it is down.
  - **Scheduled jobs** judges each job by the scheduler's own rule (`JobSchedule`, K4, K5; a job still running is neither failing nor overdue): "All 8 jobs last succeeded."; "7 jobs last succeeded. ‹key› has not run yet: its first run is ‹moment› SAST."; one job, its sentence ("entrustment-decision-expiry failed at 2026-11-12 05:30 SAST: ‹its error›.", "‹key› has not run since it was due at ‹moment› SAST: the scheduler did not start it. The application log says why; Run now runs it.", "‹key› is disabled, so the scheduler does not run it. The other 7 last succeeded.", "‹key›'s schedule cannot be read (cron ‹cron›), so the scheduler does not run it. The application log says why."); several, "Three jobs need you:" (the count in words to ten) over one sentence each; no job stored, Not configured, "No scheduled job is stored yet: the scheduler stores them as it starts."
  - **A time in a sentence never breaks** (`a7d14b19`; the replay found "2026-11-" over "12 08:37 SAST"): each "2026-11-12 08:37 SAST" or "21:39 SAST" is a `span.nowrap` inside its paragraph (`AdministratorHomeWords.Runs`), so the paragraph still reads as one sentence.
  - **No sentence holds a link, and no line is a live region** (S1): the card's links are outline buttons in its `.dashboard-card-footer`, **Its runs** (Run history filtered to the one failed job's failed runs, `JobLinks.RunsOf(key, "Failed")`, with a visually hidden ": ‹key›, failed") only while exactly one job has failed, then **Scheduled jobs**. No footer while the jobs line reads Not read (R3-H08).
- **Accounts** (`users`, `#card-accounts`, `Emphasis`): `.dashboard-metric` "24" over "accounts" ("account" at one), `p.muted.text-sm` "Every account Wombat holds but an erased one.", then "None is locked.", "1 is locked and cannot sign in now." or "‹n› are locked and cannot sign in now." (an administrator's lock or a sign-in lockout still running; Q4, T14); no breakdown by role (Q14). **Users** in its footer.

## Each part on its own (K11, T9)

The read answers per part: with the database down the Database line reads Unreachable with the moment, and every part that needs the database (the mail outcomes, the jobs, the accounts) reads **Not read**, "Not read: the database did not answer.", with no footer on its card or line. The frame's failure (one danger Alert, no cards) is left for a read that fails whole.

## Narrow widths

`.dashboard-grid`: one column to 900px; two tracks from 901 to 1099px; three from 1100px, System health spanning two (S2). The list is a `health` size container: under 30rem each line's name and badge sit on one row over the sentences, which is so from 1100 to 1112px (the card spanning two of three tracks) and, from 900px down, wherever the card (the column's width there) is under 30rem, as at 390; from 1113px, from 901 to 1099px, and at 900px down while the card allows, each line is the name over its badge in a 10rem column beside the sentences. Below 641px the footers' buttons are 44px and the footer wraps.

## Contrast

No new colour token. A line's rule is `border-color` (a hairline); the badges are the five tints (Badge); the Accounts note `muted-text`, 5.09:1 on the surface.

## Known gaps

- No page fault found re-syncing. The queue depth is the host's that draws Home: the API host's queue is not counted (K20; both hosts write the outcomes table, so the 24 hours and the last drop are both hosts').
