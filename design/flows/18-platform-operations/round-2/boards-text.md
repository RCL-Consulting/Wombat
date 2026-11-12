# Flow 18 round 2: the text of the boards that stand

Generated from `round-2/R2-*.dc.html` (T400, build-contracts § 0) by a stdlib script, run with `python -I`, as flow 17's was: style, script, svg, head and helmet blocks stripped. Each board opens with its label strip: its `<title>` as `Label:`, then each `cv-chip` of its `cv-chips` strip after `>`. Kept: headings, labels, aria-labels, placeholders, input values, alt text, `role`, `[checked]`, `[disabled]`, `[aria-disabled]`, `[selected]`, `[aria-selected]`, `[aria-haspopup]`, `[aria-current]`, `[aria-required]`, `[tabindex]`; an `anno` chip after `>`; a link is marked `[link]`, a button `[button]`. The order is by id: R2-0-spec, R2-0-spec-2 and R2-0-steps (for reference only: round 3's Spec is restated whole), then the 25 standing ids that round 3 did not redraw, with their twins: I, J, R, L, E, each at 1280 then its other widths (the widest first). Design data: a label strip, a chip, a `cv-note`, a caption saying Illustration, FIXTURES, DATA, NEW or PROPOSED is never a shipped word. A table row is one `| a | b |` line; a `dt` opens with `**`.

######## BOARD R2-0-spec.dc.html
Label: R2-0-spec — round 2 Spec (1 of 2)
### R2-0-spec · Flow 18 · Round 2 · Spec (1 of 2)
Structure A at fidelity, and screens 3–5, on the page "Round 2". Round 1's 31 boards are on the page "Round 1", untouched. Every board links ds/wombat/tokens.css, ds/wombat/components/bundle.css, then f18r2.css; no board carries an inline style; no wireframe class.
#### 1 · What I opened, and what I could not
The 30 captures, all opened, read again this round from this canvas's project files (project/ref/baseline/): each decoded as a PNG, its bytes identical (sha256) to the copies round 1 viewed one by one:
- act-1/1.6-1-institutions-one.png 1280x800
- act-1/1.6-2-kgk-created.png 1280x800
- act-1/1.6-4-create-empty-save.png 1280x800
- act-3/3.55-2-audit-failures.png 1280x985
- act-3/3.56-1-audit-entry.png 1280x800
- act-3/3.56-2-back-to-log.png 1280x4725
- act-3/3.57-1-devadmin-audit.png 1280x4725
- act-3/3.57-2-msf-submission-detail.png 1280x800
- act-3/3.57-3-naidoo-transition-detail.png 1280x800
- act-6/6.38-2-curriculum-progress-page.png 1280x800
- act-6/6.38-3-rebuild-dialog.png 1280x800
- act-6/6.38-4-rebuilt.png 1280x847
- act-A/A.2.1-1-scheduled-jobs.png 1280x1092
- act-A/A.2.10-1-digest-key.png 1280x844
- act-A/A.2.10-2-partial-key-nothing.png 1280x844
- act-A/A.2.10-3-failed-none.png 1280x844
- act-A/A.2.10-4-from-today.png 1280x2365
- act-A/A.2.2-1-auto-close-disabled.png 1280x1166
- act-A/A.2.4-1-digest-dispatched.png 1280x1092
- act-A/A.5.2-1-institutions-access-denied.png 1280x800
- act-A/A.6.1-2-kgk-saved.png 1280x800
- act-A/A.6.2-1-system-health.png 1280x800
- act-A/A.6.9-1-own-account.png 1280x800
- act-A/A.7.11-1-home-390.png 390x844
- act-A/A.7.11-3-jobs-390.png 390x2258
- act-A/A.7.11-4-runs-390.png 390x3499
- act-A/A.7.11-5-institutions-390.png 390x844
- states/audit-detail--not-found.png 1265x812
- states/institution-edit--deactivated.png 1280x800
- states/scheduled-jobs-list--loading.png 1265x812
Round 1's boards and f18r1.css, all opened: project/canvas.json (its 31 entries) and the source of each board and of f18r1.css as published in this session (version 8, unchanged since): Spec.dc.html, Steps.dc.html, h-lines.dc.html, wf-a.dc.html, wf-b.dc.html, wf-c.dc.html, i-list.dc.html, i-create.dc.html, i-record.dc.html, i-deactivate.dc.html, i-inactive.dc.html, i-states.dc.html, i-list-390.dc.html, i-record-390.dc.html, j-disabled.dc.html, j-ran.dc.html, j-illus.dc.html, j-runs.dc.html, j-runs-job.dc.html, j-states.dc.html, j-jobs-390.dc.html, j-runs-390.dc.html, a-log.dc.html, a-fail.dc.html, a-admin.dc.html, a-entry-fail.dc.html, a-entry-payload.dc.html, a-entry-large.dc.html, a-states.dc.html, a-log-390.dc.html, a-entry-390.dc.html, f18r1.css.
Could not read: none. The two 2026-09-26 states/ captures are on the old frame: their bodies only.
#### 2 · The decisions, and the boards that draw them
| Q | Decision | Boards |
| Q1 | Home's card holds everything; no System page, route or item | R2-H01–H11 |
| Q2 | Mail line: "Not configured" + "Mail is written to the log, not sent."; IEmailHealth | R2-H01, H03, H04, H08 |
| Q3 | Jobs line: all succeeded, or names failed / never run / disabled / overdue jobs | R2-H01, H02, H05–H07, H09 |
| Q4 | 24 accounts (every but the erased one), locked apart | R2-H01, H02, H10 |
| Q5 | The record headed by its name; Status section; Deactivate… / Reactivate… asking; Q5's subtitle; T378 | R2-I01–I22 |
| Q6 · PROPOSED (THIS ASK), T1 | Run history: select, Status, SAST days, Show/Clear, names, PagerControls 50, error whole | R2-R01–R11 |
| Q7 · PROPOSED (THIS ASK), T2 | SAST everywhere; schedules in words + cron; stored UTC under the entry's time | every board |
| Q8 · PROPOSED (THIS ASK), T3 | The audit filters then Show; window fixed and stated; address; live names; PagerControls | R2-L01–L15 |
| Q9 · PROPOSED (THIS ASK), T4 | Entry: What happened, Who, What it was done to, Payload; Unavailable | R2-E01–E11 |
| Q10 · PROPOSED (THIS ASK), T5 | Results above the list, focused, in true words; disabled Run now with its reason | R2-J02–J10 |
| Q11 · PROPOSED (THIS ASK), T6 | At 390 health first; every list stacks; 44 px | every -390 twin |
| Q12 · PROPOSED (THIS ASK), T7 | What the build folds in | this board, § 3 |
| Q13 | A drop in 24 h is Failing; no mail sent is not a state | R2-H04, H09 |
| Q14 | No breakdown on Accounts | R2-H01 |
| Q15 | Names read live; an erased person by the pseudonym | R2-L*, R2-E09 |
| Q16 | Q5 holds for institutions only (T401 later) | none drawn |
| Q17 | One Unavailable for an institution, both roles | R2-I19, I20 |
#### 3 · The corrections, each with its boards
| Item | Done | Boards |
| M1 | Accounts 24, "None is locked.", no breakdown; .accounts-breakdown gone | R2-H01–H10 |
| M2 | (a) Reactivate… asks (dialog, opens on Cancel, result focused); (b) Q5's subtitle | R2-I14–I16; R2-I01–I06 |
| M3 | Q2's sentence, then the PROPOSED second line | R2-H01–H10 |
| M4 | Own account via Users (3 + 1 / 4 + 1); 14 + 1.23 Expects change; the extra rows | R2-0-steps |
| S1 | No B or C part anywhere | all |
| S2 | Every PROPOSED item listed | § 6 |
| S3 | The house load-error sentence with Try again (refresh-cw), nothing under it | R2-I05, I18, J12, R11, L15, E11 |
| S4 | Database down: Mail as it is, jobs and Accounts Not read | R2-H08 |
| S5 | Overdue in Q3's sense | R2-H07, H09 |
| S6 | Unavailable for both roles | R2-I19, I20 |
| S7 | Unavailable as § Alerts draws it (.detail-card--empty, .state-panel-copy, .form-actions, no icon) | R2-I19, I20, E08 |
| S8 | Each list in the stack its columns need; swept 900–1100 against its container; columns read at 1280 per role | R2-0-spec-2 § 9 |
| S9 | The DATA table's costly half; Ran by names and descriptions are not DATA | § 7 |
| S10 | Every state round 1 left out is a board | § 5 |
| S11 | KGK's contact at A.7.11 hod.paediatrics@kgk.wombat.local | R2-I03 |
| N1 | No gendered pronoun anywhere; "they" | all |
| N2 | Subtitle "KGK · Active" / "DEMO · Inactive"; results by name | R2-I11–I22 |
| N3 | The field help PROPOSED (T12) | R2-I07–I13 |
| N4 | One "Status": the h2 | R2-I11–I16 |
| N5 | Running amber (accepted); Overdue .badge-overdue; tints by class | R2-J05, J06, R07, H07 |
| N6 | Brackets the code holds filled: the crash error, "No subject", David Naidoo, the three refusals | R2-J08, L02, E01–E03 |
| N7 | Run now refused now moves the focus to the result (today it stays on the button) | R2-J06 |
| N8 | PagerControls with "Per page:" 10/20/50/100, set to 50 | R2-R*, R2-L* |
| N9 | No "Error: None"; Running once; "No runs match these filters."; descriptions rewritten | R2-R*, R2-J* |
| N10 | The audit rows drop Subject and the address; both stay on the entry | R2-L*, R2-E* |
| N11 | The payload region named by aria-labelledby its h2 | R2-E02, E03, E06, E07, E09 |
| N12 | Detail's name keeps the date: "Detail: Login at 2026-11-10 20:24:10" | R2-L* |
| N13 | Both stack classes; every caption a name | every list |
| N14 | states.md:1093 "Nine jobs" → eight | R2-0-steps |
#### 4 · Illustrations (states the replay never reaches)
R2-H03, R2-H04, R2-H05, R2-H06, R2-H07, R2-H08, R2-H09, R2-H10, R2-I06, R2-I09, R2-I10, R2-I19, R2-I22, R2-J05, R2-J06, R2-J07, R2-J08, R2-J09, R2-J10, R2-R06, R2-R07, R2-R08, R2-R09, R2-L08, R2-E06, R2-E07, R2-E09. Each says so in its canvas title and as an orange chip on its face; none draws two states that cannot hold at once.
#### 5 · The boards
170 boards in six rows: R2-H01–H11, R2-I01–I22, R2-J01–J12, R2-R01–R11, R2-L01–L15, R2-E01–E11, each at 1280 and 390 (R2-I03 and R2-R05 at 390 only), with 768 twins of H01, I02, I12, J01, R01, L01, L05 and E03; and R2-0-spec, R2-0-spec-2 and R2-0-steps. The canvas holds 31 + 173 boards.
#### 6 · PROPOSED (THIS ASK)
| T | What | Boards |
| T1 | Q6 as settled; "Run history", "Ran by", Finished folded. Alternative: "Job run history", "Triggered by" | R2-R* |
| T2 | Q7 as settled | all |
| T3 | Q8 as settled. Alternative: a Person filter by name (Q15 makes it possible) | R2-L* |
| T4 | Q9's section names. Alternative: Event, Person, Subject | R2-E* |
| T5 | Q10's words. Alternative: round 1's "‹key› ran and succeeded. It started at …" | R2-J* |
| T6 | Q11; the stacks (Institutions, Scheduled jobs, Audit log 1150; Run history 1024) | all 390, 768 |
| T7 | Q12 | Spec |
| T8 | Health in words, a badge a line, no dot, fixed order | R2-H* |
| T9 | Home's partial answer with the database down | R2-H08 |
| T10 | Unavailable's trail and link by the caller's way in | R2-I19, I20 |
| T11 | The institution's results by name; Reactivate's dialog (NEW words) | R2-I11–I16 |
| T12 | Create subtitle; field help; Status sentences; "KGK · Active" | R2-I07–I17 |
| T13 | The health lines' words beyond Q2 and Q3 | R2-H* |
| T14 | Accounts: title, "24" / "accounts", the sentence, the locked line, "Users" | R2-H* |
| T15 | The new subtitles and descriptions | R2-J*, R*, L*, E* |
| T16 | "None while disabled", "Not run yet", "Running…", Duration "—" | R2-J*, R07 |
| T17 | Count lines and empty states | R2-R*, L* |
| T18 | Run history's filters in the address | R2-R09, H05 |
| T19 | The Institution filter, and the institution under the person (devadmin) | R2-L05–L08 |
| T20 | An actor-less row: "MSF respondent (no account)"; the entry's sentence; "No signed-in person" (NEW) for any other | R2-L05, L06, E02 |
| T21 | The window line, on Show and on arrival (NEW) | R2-L* |
| T22 | RunScheduledJobNowCommand refuses a disabled job (NEW refusal) | R2-J09 |
| T23 | The failures' words (NEW) | R2-J10, I22 |
| T24 | Created off the record; the list's Created a day | R2-I* |
| Also | Running kept amber (as built); run history's filters in the address; the audit address carrying the resolved window, and the entry's trail carrying it | R2-J06, R09, L13 |
#### 7 · The changes the boards need (code), each PROPOSED; decided ones say their Q
| Where | Change |
| GetAdministratorDashboardSummaryQuery | A partial answer when the database does not answer (S4, T9: today the probe is caught at :31–41 but the ten ListUsersInRoleAsync calls at :44–52 throw and DashboardFrame replaces both cards); one count, every account but an erased one, by a join to DataRightsErasureRecord, and the locked count (Q4); the mail state from IEmailHealth (Q2); each job's key, enabled, last status, time, first error line, and overdue computed from its cron (Q3). DATA |
| IEmailHealth | NEW (T327, Q2, Q13): configured or not, the queue's depth, the last sent, the last dropped and its refusal, the drops in 24 h; written by the mail worker and LoggingEmailSender; the Web host's only. DATA |
| AdministratorDashboard | The two cards; the status dots go; DESIGN.md § Status dots retired. |
| ScheduledJobDto, GetScheduledJobStatusQueryHandler | The last run's error, finish and runner (S9, DATA); the schedule in words and SAST from the cron (a formatter, T2); each job class's Description without its UTC clause (code, T15). |
| RunScheduledJobNowCommand | Returns the run: status, start, finish, error (today IRequest with no answer, :8; DispatchNowAsync swallows a failure into the row, ScheduledJobDispatcher.cs:25, :54–60). Refuses a disabled job (T22). DATA |
| ScheduledJobsList | The result above the list, focused (today after the table, :79–84); the columns; Run now aria-disabled with its reason (today disabled, :64); Running…; the house load error (today ex.Message, :117) and a loading line; the 1150 px stack. |
| GetScheduledJobRunsQuery | Page and PageSize with a total (today Take(200), :43–45); From and To as South African days (today UTC days, :31–41, :54, :60); the runner's name by GetDisplayNamesAsync (an existing read). DATA: the total and paging |
| ScheduledJobRunsList | "Run history" (h1, tab, NavOwners' crumb); the select (today free text per keystroke, :23); Show and Clear filters; the address (T18); PagerControls; the count line; the empty states; the house load error; no Back to jobs (:9–12). |
| InstitutionsList | Q5's subtitle (today :8); the columns; StackWide in the 1150 px stack (today no Stack, :20); the house load error with Try again (today exception.Message, :63) and a loading line. |
| InstitutionCreate | T378's refusals (today DataAnnotations ValidationMessages, :30–47); the mark line; the subtitle; no Back (:18); the created result by the one-shot token (today forceLoad, :79). |
| InstitutionEdit (Administrator) | h1 and tab the name (today "Edit institution", :212); the subtitle (:214–216); no Back (:187–188); the Status section with Deactivate… and Reactivate… (ReactivateInstitutionCommand exists) behind dialogs; no Active box (:86–97), no Save-with-unticked dialog (:128–129); the results by name (T11); Unavailable for null, both roles (today /not-found, :102–105; Q17). |
| ListAuditEntriesQuery | From and To resolved and passed in, never recomputed (today UtcNow − 24h, :20–21); the Institution filter for the Administrator (pinned for an institution's administrator, :41); the institution's name per row (today an id, AuditEntryDto.cs:15) DATA; the person's live name (Q15). |
| AuditList | The filters' order and labels; Show and Clear filters after them (today Apply filters in the header, :8–15); the address; the window line; PagerControls (today hand-built, :121–134), the focus on the h2 (today lost, :165); the columns (no Subject, no address: N10); the 1150 px stack; the house load error. |
| AuditDetail | h2 sections (today h3 cards, :26, :43, :54, :74); the institution by name (today raw ids, :60, :64) DATA; the payload laid out, an indented serialisation keeping the stored key order, in its region (today one pre, :71–77); Unavailable for null (today an empty card, :20–21); the trail's crumb carrying the list's address (PageHeader's custom Trail, as SpecialityEdit.razor:28); no Back to log (:10). |
#### 8 · Messages by kind, with their words
| Kind | Words | Status |
| Health line | The lines on R2-H01–H10 (§ FIXTURES "Health lines") | Q2/Q3 decided; the rest PROPOSED T13 |
| Result | "Kgosi Kgari Teaching Hospital created."; "… saved. Its contact email is now paediatrics@kgk.wombat.local."; "Demo Institution deactivated. A new invitation can no longer name it."; "Demo Institution reactivated. A new invitation can name it again." | T11 (flow 12/16's), replacing "Institution saved." / "Institution deactivated." / "Institution saved and reactivated." |
| Result | "Disabled msf-campaign-auto-close. The scheduler will not run it until you enable it."; "Enabled … Its next run is 2026-11-10 22:00 SAST."; "Ran weekly-coordinator-digest: Succeeded at 2026-11-10 21:39 SAST."; "Ran ‹key›: Failed at … : ‹the error as stored›." | T5, replacing "Job '…' disabled." / "dispatched." |
| Refusal | The four create refusals (built); "Job 'audit-log-retention' is already running, …" (built) | built |
| Refusal | "Job 'msf-campaign-auto-close' is disabled, so it was not run. Enable it, then run it." | NEW, T22 |
| Failure | "msf-campaign-auto-close could not be disabled. Nothing has changed. Try again, or come back in a few minutes."; "Could not deactivate / reactivate the institution. …" | NEW, T23; "Could not save the institution. …" built |
| Question | "Deactivate Demo Institution?" (built); "Reactivate Demo Institution?" "A new invitation can name it again. Its people and their records are as they were." | NEW, T11 |
| Load / error | "Loading the …." / "Could not load the …. Nothing has changed. Try again, or come back in a few minutes." | the house sentence (S3), replacing round 1's |
| Empty | "No runs match" / "The filters are kept above. Clear them to see all 21."; "No job has run yet" (round 1's); "No actions match" / "Nothing was recorded …" (round 1's) | NEW / T17 |
| Notice | The window lines (T21); "Enable it to run it." (T5) | PROPOSED / NEW |
| Unavailable | "Institution unavailable" / "This institution does not exist, or you cannot open it."; "Audit entry unavailable" / "This audit entry does not exist, or you cannot open it." | Q17, T4 |

######## BOARD R2-0-spec-2.dc.html
Label: R2-0-spec-2 — round 2 Spec (2 of 2)
### R2-0-spec-2 · Round 2 · Spec (2 of 2)
#### 9 · Classes
| Row | Class | Kind | Why |
| R1 | .health-list, .health-line, .health-line-name, .health-line-text | NEW (T8) | Home's lines: name and badge beside the sentence; one column under a 30rem list (container query). Round 1's R1, accepted. |
| R2 | .code-block--region | NEW (Q9) | A .code-block modifier: max-height 28rem, overflow auto, its focus ring. Round 1's .payload-block. |
| C1 | .institutions-page, .jobs-page, .audit-page in the 1150 px stack; .job-runs-page in the 1024 px stack | CHANGED (T6) | Scope classes beside .people-page with no rule but the stack; written out in f18r2.css because the canvas cannot edit bundle.css. |
| reuses | .page, .sidebar, .brand, .nav-*, .account-row (the shell); .breadcrumbs; .header-container, .page-subtitle, .actions-cell; .dashboard-grid, .dashboard-span-2, .detail-card, .dashboard-card-title, .dashboard-card-skeleton, .dashboard-metric(-value, -label); .people-page, .record-page, .user-page, .user-section; .search-container, .people-filter, .search-grid, .search-field, .filter-actions, .field-help; .table-container, .clinic-table, .clinic-table--stack, .clinic-table--stack-wide, .col-wrap, .col-fit--wide, .person-cell, .nowrap; .pager, .pager-info, .pager-page-size(-label, -select), .pager-actions; .form-container, .form-grid, .form-group, .form-control, .form-select, .form-actions, .required-mark, .validation-message; .action-result, .alert(-success, -warning, -danger, -row, -row-text); .badge and its tints; .loading-line, .skeleton, .skel-title, .skel-line, .skel-short; .detail-card--empty, .state-panel-title, .state-panel-copy; .detail-list; .code-block; .dialog-card, .dialog-scrim, .dialog-form, .dialog-actions; .page-title-with-icon, .page-title-icon--danger, .system-panel (Access denied only); .muted, .text-sm, .visually-hidden; .icon | shipped | as shipped, never redeclared |
| gone | .accounts-breakdown, .list-head, .list-count, .list-window, .action-reason, .job-desc, .job-key, .run-when, .nw, .run-error, .payload-block, .payload-note, .audit-entry-page, .stored-utc, .institution-status, round 1's R9, every .cv-wf | — | each a shipped class's job, or retired |
| K1–K5 | .cv-chips, .cv-chip(--illus, --new, --proposed, --data); .cv-note; .cv-focus; .cv-scrim, .cv-dialog; .cv-sheet, .cv-cols | canvas only | never shipped |
Two choices without a new rule: the disabled Run now's reason is a span.muted.text-sm (a p's own margins would open the actions cell's row), and a run's error and a row's refusal sit under the badge after a <br> in a span.text-sm (inside .person-cell, a badge would take the cell's muted span rule). Institutions' Name keeps the row's th scope="row", painted as a header as on Users and EPAs.
#### 10 · Widths: the 900–1100 sweep, against each container, and every column at 1280
Measured in Chromium on this canvas's own files (table scrollWidth / container clientWidth, px), and every board at 1280, 1100, 768 and 390: no page and no table scrolls sideways at any of them; at 390 every button, input and select in a page is 44 px. Round 1's arithmetic held: 974 px at 1280 (972 measured inside the border), 717 at 1025.
| List | Sweep: width: layout table/container | Each column at 1280: width, most lines of a value |
| Institutions (1150) | 900: stacked 592/592 · 950: stacked 642/642 · 1000: stacked 692/692 · 1024: stacked 716/716 · 1025: stacked 717/717 · 1050: stacked 742/742 · 1100: stacked 792/792 · 1150: stacked 842/842 · 1151: table 843/843 · 1200: table 892/892 · 1280: table 972/972 | Name 258 px, 2 lines; Short code 125 px, 1 line; Contact email 258 px, 2 lines; Status 99 px, 1 line; Created 144 px, 1 line; Actions 87 px, 1 line |
| Scheduled jobs (1150) | 900: stacked 592/592 · 950: stacked 642/642 · 1000: stacked 692/692 · 1024: stacked 716/716 · 1025: stacked 717/717 · 1050: stacked 742/742 · 1100: stacked 792/792 · 1150: stacked 842/842 · 1151: table 843/843 · 1200: table 892/892 · 1280: table 972/972 | Job 350 px, 4 lines; Schedule 145 px, 4 lines; Last run 171 px, 3 lines; Next run 171 px, 2 lines; Actions 135 px, 2 lines |
| Run history (1024) | 900: stacked 592/592 · 950: stacked 642/642 · 1000: stacked 692/692 · 1024: stacked 716/716 · 1025: table 717/717 · 1050: table 742/742 · 1100: table 792/792 · 1150: table 842/842 · 1151: table 843/843 · 1200: table 892/892 · 1280: table 972/972 | Job 288 px, 1 line; Started 270 px, 1 line; Duration 98 px, 1 line; Status 125 px, 1 line; Ran by 191 px, 1 line |
| Audit log, Prof Mbatha (1150) | 900: stacked 592/592 · 950: stacked 642/642 · 1000: stacked 692/692 · 1024: stacked 716/716 · 1025: stacked 717/717 · 1050: stacked 742/742 · 1100: stacked 792/792 · 1150: stacked 842/842 · 1151: table 843/843 · 1200: table 892/892 · 1280: table 972/972 | When 242 px, 2 lines; Person 145 px, 2 lines; Category 142 px, 1 line; Action 248 px, 2 lines; Result 103 px, 1 line; Detail 92 px, 1 line |
| Audit log, devadmin (1150) | 900: stacked 592/592 · 950: stacked 642/642 · 1000: stacked 692/692 · 1024: stacked 716/716 · 1025: stacked 717/717 · 1050: stacked 742/742 · 1100: stacked 792/792 · 1150: stacked 842/842 · 1151: table 843/843 · 1200: table 892/892 · 1280: table 972/972 | When 218 px, 2 lines; Person 213 px, 3 lines; Category 142 px, 1 line; Action 205 px, 2 lines; Result 103 px, 1 line; Detail 92 px, 1 line |
| Institutions, the 200-character name | 900: stacked 592/592 · 950: stacked 642/642 · 1000: stacked 692/692 · 1024: stacked 716/716 · 1025: stacked 717/717 · 1050: stacked 742/742 · 1100: stacked 792/792 · 1150: stacked 842/842 · 1151: table 843/843 · 1200: table 892/892 · 1280: table 972/972 | Name 245 px, 8 lines; Short code 124 px, 1 line; Contact email 245 px, 2 lines; Status 107 px, 1 line; Created 165 px, 1 line; Actions 86 px, 1 line |
Read at 1280, each role: no value runs past two lines beside an empty band. Run history first overflowed at 1025–1050 (752 in 717) while Started was one unbreakable line; each time now breaks only between its day and its hour (.nowrap on each part), which is what fits it in the 1024 px stack. The audit log's Action took only 144 px at 1280 (three lines) beside a one-line When while it carried .col-wrap's 14% header; it now asks no share and gets 205–248 px, two lines at most. A Job description on Scheduled jobs runs to four lines in 350 px beside two-line times: prose, the row's tallest cell. On devadmin's rows the institution under the person takes two lines ("Kgosi Kgari Teaching Hospital"). Institutions' contact email takes two lines at 1280, three at 1151. At 100 institutions the list is 100 rows on one page, by name; a pager would come with T401's records, not now.
#### 11 · Badges (BadgeFor, no new tint)
| Words | Class |
| OK, Success, Succeeded, Active | .badge-completed |
| Failing, Failed, Unreachable | .badge-declined |
| Not configured, Disabled, Running | .badge-accepted (Running as built, N5) |
| Overdue | .badge-overdue |
| Inactive | .badge-withdrawn |
| Not read | .badge-draft |
#### 12 · 44 px at 390, and 768
.people-page (the lists) and .record-page / .user-page (the records) make every button, input and select 44 px; .filter-actions stacks Show and Clear filters full width; StackWide's row buttons are 44 px from 900 px down (and from 1150 / 1024 in the C1 stacks); .dialog-card .dialog-actions stacks both dialogs' buttons, the action on top, Cancel first in the DOM; a header action takes the row. At 768 every list is stacked (all four stacks are wider than 768), Home is the 2-column grid with System health spanning both.
#### 13 · DESIGN.md text each change needs
| Section | Change |
| § Status dots | Retired: Home's three dots were its only user; a state is a word on a badge. |
| § Dashboard page | The Administrator's Home: System health (span 2, three lines in words, fixed order) and Accounts (one figure, the locked line). |
| § Table system | Institutions, Scheduled jobs and the Audit log join the 1150 px stack; Run history the 1024 px stack; a time breaks only between its day and its hour. |
| § Pager | Run history and the Audit log on PagerControls, 50 a page, "Per page:", the focus on the list's h2 after a turn. |
| § Page-level patterns | A record headed by its name, its Status in a section of its own; an entry a .user-page of h2 sections; Unavailable gains Institution and Audit entry, the trail and link by the caller's way in; no Back links. |
| § Button system | Deactivate… and Reactivate… each asking (outline, aria-haspopup); Run now aria-disabled with its reason as text; a result above the list. |
| § Form system | The create page's refusals under their fields, focused, described by them first. |
#### 14 · Tokens and contrast
Every colour an existing token. Measured: text on each badge tint 11.23–11.89:1; muted text on white 5.09, on --header-bg 4.57, on the success tint 4.52, warning 4.79, danger 4.75, info 4.77 (the reason beside Run now, the window line and the card's muted line pass on every ground they sit on); the badge edges (success 5.88, warning and .badge-overdue 5.77, danger 5.95, --input-border 3.80 on white); the focus ring 4.86:1 on white and 4.37 on --header-bg (the payload region's ring); the link 6.70.
#### 15 · LATER, NOT BUILT
- T274: what a run did, stored with it (the Status cell leaves room).
- T157: refusing to start in Production with no mail server.
- T114 items 2–3: bounding a payload's size.
- The Api host's /health, which checks nothing (to be filed).
- T401: the College's records to Q5's pattern.
- A by-name Person filter (T3).
#### 16 · Edge cases
| Edge | Board |
| Every health line failing | R2-H09 |
| The database down | R2-H08 |
| No runs | R2-R08 |
| Thousands of runs, paged (about 2,800 held) | R2-R06 |
| A run still Running | R2-R07 |
| A 2,000-character error | R2-R07 |
| A payload of several KB | R2-E06 |
| An actor-less row | R2-L05, L06 (T20) |
| An entry with no subject / with one | R2-E01 / R2-E07 |
| No contact email; a 200-character name | R2-I06 |
| An institution's administrator whose institution is inactive reads the log | words only: unchanged (their account, role and rows are kept) |
| An erased person in old rows | R2-E09 (Q15) |
| Two Administrators running the same job | R2-J06 |

######## BOARD R2-0-steps.dc.html
Label: R2-0-steps — round 2 Steps
### R2-0-steps · Round 2 · Steps
Counting rule. A journey counts presses + fields. A press is a click on a button, link, tab or checkbox; a field is a text typed, a date set or a choice made in a select or a radio group, and is not also a press; a menu link is a press, and at 390 opening the menu is one more; a dialog's confirm is a press; a page the app goes to by itself is no press; reading and scrolling are 0.
#### The 23 steps, and 1.23
| Step | Board | Expect | What it will say once built |
| 1.1 | R2-H02 | changes | "System health, which shows the database connection as healthy" → "System health, which reads Database OK, Mail Not configured ('Mail is written to the log, not sent.') and Scheduled jobs OK, naming weekly-coordinator-digest as not run yet"; "Users across institutions, which counts the seeded accounts (8, or 9 where the bootstrap Administrator exists)" → "Accounts, which counts every account but an erased one (8, or 9 …), with 'None is locked.'". The menu, Home lit and the top bar unchanged. |
| 1.6 | R2-I01, I07, I08, I11, I02 | changes | Do unchanged. The row unchanged, "offering Edit only"; the form gains "* marks a field you must fill in." and each refusal focused and described; "headed 'Edit institution', with the browser tab reading 'Edit institution · Wombat'" → "headed 'Kgosi Kgari Teaching Hospital', the tab 'Kgosi Kgari Teaching Hospital · Wombat', the result 'Kgosi Kgari Teaching Hospital created.' focused"; "Active is ticked and Deactivate is offered" → "Status reads Active, 'A new invitation can name it.', with Deactivate…"; "created today" reads 2026-11-10. |
| 1.11 | R2-H02 | changes | "The dashboard's count of registered users is two higher" → "Accounts reads two higher (11)". |
| 3.32 | R2-J04 (words) | unchanged | "The job's last run updates." holds; the result now reads "Ran assessor-pending-nudge: Succeeded at 2026-11-10 19:15 SAST." above the list. |
| 3.55 | R2-L01–L03 | changes | Do "Set From (UTC) … and apply" → "Set From (SAST) to the sitting's start, 18:58, and press Show"; the two filters end "and press Show". Expect: "50 to a page" unchanged; "each with its actor and OK" → "each with the person by name and Success"; "each FAILED" → "each Failed, its refusal under the badge"; 166, 4 and 19 unchanged. |
| 3.56 | R2-E01, L13 | changes | "The Event card shows …" → "What happened shows the SAST time to the millisecond with the stored UTC under it, Command, CreateActivityCommand, Kgosi Kgari Teaching Hospital, and Failed with 'Date observed: The date cannot be after today (2026-11-10).'"; "The Actor card shows Dr Ndlovu's user id, display name and IP address" → "Who shows Sipho Ndlovu by name, the user id and ::/48"; "No Payload card" → "no Payload section"; "'Back to log' returns to the list" → "the trail's Audit log returns to Failures only, page 1, the focus on the Detail left". |
| 3.57 | R2-L05, E02, E03 | changes | Do "From (UTC)" → "From (SAST) 19:00, Show". "whose actor is 'system'" → "whose Person reads 'MSF respondent (no account)' and 'No institution'"; "names no user and no display name, only a truncated address" → "Who reads 'No person: an MSF respondent answers by link, with no account.', User id None, ::/48"; the payloads unchanged in content, one property a line. 174 and the eight submissions unchanged. |
| 4.38 | R2-L09, L10, E04, E05 | changes | "five OK rows by Dr Zulu" → "five Success rows by Thandi Zulu"; "the Event card and the Actor card only" → "What happened, Who and What it was done to only, no Payload"; the revocation's actor "Refilwe Mokoena". |
| 6.38 | — | unchanged | Flow 15's page; no board. |
| A.2.1 | R2-J01 | changes | "its cron schedule (UTC), with its last run, that run's status badge and its next run in the server's local time" → "its schedule in words in SAST with the cron under it, its last run in SAST with its badge under it, and its next run in SAST"; "Each has Disable and Run now" unchanged; "reads '—' for its last run and status" → "reads 'Not run yet'"; the eight keys unchanged. |
| A.2.2 | R2-J02, J03 | changes | "Disabling says 'Job 'msf-campaign-auto-close' disabled.'" → "'Disabled msf-campaign-auto-close. The scheduler will not run it until you enable it.' above the list, focused"; "'—' for its next run" → "'None while disabled'"; "its Run now is unavailable" gains "with 'Enable it to run it.' beside it"; enabled again: "'Enabled msf-campaign-auto-close. Its next run is 2026-11-10 22:00 SAST.'". |
| A.2.4 | R2-J04 | changes | "'Job 'weekly-coordinator-digest' dispatched.'" → "'Ran weekly-coordinator-digest: Succeeded at 2026-11-10 21:39 SAST.'"; the rest unchanged. |
| A.2.6 | — | unchanged | Its Expect reads the log. |
| A.2.8 | — | unchanged | Its Expect reads the log. |
| A.2.9 | — | unchanged | "Each run reads Succeeded" holds. |
| A.2.10 | R2-R01–R04 | changes | Do → "Choose weekly-coordinator-digest in Job and press Show; choose All jobs and Failed and press Show; choose All and set From to today and press Show" (the "digest" sub-step goes). Expect: "each with its key, start, finish, duration, status badge, who triggered it and any error" → "each with its job, its start in SAST, its duration, its status badge (a failed run's error under it) and who ran it"; "'scheduler'" → "'Scheduler'"; "Filter is pressed" → "Show is pressed"; "'No runs found'" → "'No runs match these filters.'"; "a UTC day" → "a South African day"; gains "the pager reads 'Showing 1–21 of 21'; 10 by hand, 11 by the scheduler". |
| A.5.2 | R2-I21 | unchanged | Flow 01's Access denied. |
| A.5.13 | — | unchanged | No System item (Q1). |
| A.6.1 | R2-I02, I12 | changes | "creation time" → "the day it was created"; "Saving says 'Institution saved.'" → "'Kgosi Kgari Teaching Hospital saved. Its contact email is now paediatrics@kgk.wombat.local.'"; "offered the Status box and Deactivate" → "Status reads Active, with Deactivate…, in a section of its own; no Active box". |
| A.6.2 | R2-H01 | changes | Rewritten: "Database reads OK; Mail reads Not configured, 'Mail is written to the log, not sent.'; Scheduled jobs reads OK, 'All 8 jobs last succeeded.'; each a word on a badge, no dot, no task id. Accounts reads 24, every account but an erased one, and 'None is locked.'" |
| A.6.9 | — | unchanged | Flow 12's page; no board. |
| A.6.10 | — | unchanged | /health. |
| A.7.11 | R2-I03, R05, J01-390, H01-390 | changes | "The jobs table scrolls inside its container, with each Run now and toggle reachable" → "Each job is a stacked block, its state, next run, Run now and Disable in view, each button 44 px; Run history's runs and Institutions' rows stack likewise (KGK's contact hod.paediatrics@kgk.wombat.local); nothing scrolls sideways"; "The run-history filters stack" unchanged. |
| 1.23 (Q17) | R2-I20 | changes | "The Demo Institution's id sends ‹Prof Mbatha› to Page not found, not 'You cannot open this page'" → "… to 'Institution unavailable', 'This institution does not exist, or you cannot open it.', with Go to Home; never 'You cannot open this page'" (act-1-setup.md:709, :722). |
Changed: 14 of the 23 (1.1, 1.6, 1.11, 3.55, 3.56, 3.57, 4.38, A.2.1, A.2.2, A.2.4, A.2.10, A.6.1, A.6.2, A.7.11), and 1.23. Unchanged: 9 (3.32, 6.38, A.2.6, A.2.8, A.2.9, A.5.2, A.5.13, A.6.9, A.6.10).
#### states.md rows
| Line | Change |
| 141 | "System health (with the job status), Users across institutions" → the two cards in words |
| 888 | "An administrator's edit" → the record by its name |
| 889 | "Institution saved." → T11's |
| 898 | "Institution deactivated." → T11's |
| 899 | Page not found → Institution unavailable (Q17) |
| 1086 | "No entries" → "No actions match" |
| 1091 | "Audit entry not found" → "Audit entry unavailable" |
| 1093 | "Nine jobs" → eight (stale today, N14) |
| 1095 | "Dispatched" → the ran result |
| 1099 | "the full key" → the Job select |
| 1100 | "No runs" → "No runs match these filters." |
Also rows (M4 c): A.2.1's "—" → "Not run yet"; A.2.10's "finish" (folded into Started and Duration); 3.57's "system" on the rows; 4.38's "OK".
#### Runbook words to search (BRIEF.md § 9 item 7)
| Words | Step |
| "Apply filters", "apply" | 3.55 |
| "Back to log" | 3.56 |
| "Back to jobs" | A.2.10 (none in its Expect; the page's) |
| "Edit institution" | 1.6 |
| "registered users" | 1.11 |
| "dispatched" | A.2.4; states.md:1095 |
| "When (UTC)", "From (UTC)" | 3.55, 3.57 |
| "Display name" | 3.56 |
| "Users across institutions" | 1.1, A.6.2 |
| "Status box" | A.6.1 |
| "scheduler" lower-case | A.2.10 |
| "No runs found" | A.2.10 |
| "Page not found" | 1.23 |
#### Journeys: presses + fields, 1280 / 390, today's beside
| Journey | Round 2 | Today |
| 1.1 read Home | 0 + 0 / 0 + 0 | 0 + 0 |
| A.6.2 read health and accounts | 0 + 0 / 0 + 0 | 0 + 0 (nothing true to read) |
| A failing mail line found | 0 + 0 (acting on it is outside Wombat) | never (nothing says it) |
| A failed job found and run again (Illustration) | Home's Scheduled jobs, Run now: 2 + 0 / 2 + 0 | 2 + 0 / 3 + 0 (the menu), found only by reading a badge |
| 1.6 | Institutions, Create institution, Save (empty), Save, the trail: 5 + 3 (390: 6 + 3) | 5 + 3 (Back for the trail) |
| A.6.1 | Institutions, Edit, Save; the email: 3 + 1 (390: 4 + 1) | 3 + 1 |
| Deactivate and reactivate | Deactivate…, Deactivate, Reactivate…, Reactivate: 4 + 0 | Deactivate, Deactivate, the box, Save: 4 + 0 |
| A.2.2 | Disable, Run now (nothing sent), Enable: 3 + 0 | 2 + 0 (Run now not pressable) |
| A.2.10 | Run history, Show ×3; Job, Job and Status, Status and From: 4 + 5 (390: 5 + 5) | Run history, Filter ×4; the key ×3, Status, From: 5 + 6 |
| 3.55 | Audit log, Show ×3; From, Result, Result and Action: 4 + 4 (390: 5 + 4) | 4 + 4 (Apply) |
| 3.56 and back | Detail, the trail: 2 + 0, the rows kept | 2 + 0, filters lost (3.56-2: 325 rows): 2 + 2 more to find the row |
| Curriculum progress | 2 + 0 / 3 + 0 | the same (flow 15's) |
| Their own account (A.6.9's page) | Users, Show, the row; "devadmin": 3 + 1 / 4 + 1 | the same |

######## BOARD R2-I01.dc.html
Label: Institutions · Wombat
> R2-I01
> 1.6, before the create: one row
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[link] Create institution
[tabindex="-1"]
Institutions
| Name | Short code | Contact email | Status | Created | Actions |
| Demo Institution | DEMO | admin@demo.local | Active | 2026-11-10 | [link] Edit Demo Institution |
[aria-label="Board notes"]
Ordered by name. No pager: institutions are few; at 100 the list would be 100 rows on one page, read by name (say so; not drawn).

######## BOARD R2-I01-390.dc.html
Label: Institutions · Wombat
> R2-I01
> 1.6, before the create: one row
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[link] Create institution
[tabindex="-1"]
Institutions
| Name | Short code | Contact email | Status | Created | Actions |
| Demo Institution | DEMO | admin@demo.local | Active | 2026-11-10 | [link] Edit Demo Institution |
[aria-label="Board notes"]
Ordered by name. No pager: institutions are few; at 100 the list would be 100 rows on one page, read by name (say so; not drawn).

######## BOARD R2-I02.dc.html
Label: Institutions · Wombat
> R2-I02
> A.6.1: two rows
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[link] Create institution
[tabindex="-1"]
Institutions
| Name | Short code | Contact email | Status | Created | Actions |
| Demo Institution | DEMO | admin@demo.local | Active | 2026-11-10 | [link] Edit Demo Institution |
| Kgosi Kgari Teaching Hospital | KGK | paeds-admin@kgk.wombat.local | Active | 2026-11-10 | [link] Edit Kgosi Kgari Teaching Hospital |
[aria-label="Board notes"]
Stacked from 1150 px down (C1). The sweep and the column read at 1280 are on R2-0-spec-2.

######## BOARD R2-I02-768.dc.html
Label: Institutions · Wombat
> R2-I02
> A.6.1: two rows
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[link] Create institution
[tabindex="-1"]
Institutions
| Name | Short code | Contact email | Status | Created | Actions |
| Demo Institution | DEMO | admin@demo.local | Active | 2026-11-10 | [link] Edit Demo Institution |
| Kgosi Kgari Teaching Hospital | KGK | paeds-admin@kgk.wombat.local | Active | 2026-11-10 | [link] Edit Kgosi Kgari Teaching Hospital |
[aria-label="Board notes"]
Stacked from 1150 px down (C1). The sweep and the column read at 1280 are on R2-0-spec-2.

######## BOARD R2-I02-390.dc.html
Label: Institutions · Wombat
> R2-I02
> A.6.1: two rows
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[link] Create institution
[tabindex="-1"]
Institutions
| Name | Short code | Contact email | Status | Created | Actions |
| Demo Institution | DEMO | admin@demo.local | Active | 2026-11-10 | [link] Edit Demo Institution |
| Kgosi Kgari Teaching Hospital | KGK | paeds-admin@kgk.wombat.local | Active | 2026-11-10 | [link] Edit Kgosi Kgari Teaching Hospital |
[aria-label="Board notes"]
Stacked from 1150 px down (C1). The sweep and the column read at 1280 are on R2-0-spec-2.

######## BOARD R2-I03-390.dc.html
Label: Institutions · Wombat
> R2-I03
> A.7.11 on a phone
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[link] Create institution
[tabindex="-1"]
Institutions
| Name | Short code | Contact email | Status | Created | Actions |
| Demo Institution | DEMO | admin@demo.local | Active | 2026-11-10 | [link] Edit Demo Institution |
| Kgosi Kgari Teaching Hospital | KGK | hod.paediatrics@kgk.wombat.local | Active | 2026-11-10 | [link] Edit Kgosi Kgari Teaching Hospital |
[aria-label="Board notes"]
KGK's contact set at A.6.3 (S11). Each row a block: Short code, Contact email, Status, Created and Edit in view; Edit 44 px.

######## BOARD R2-I04.dc.html
Label: Institutions · Wombat
> R2-I04
> The list loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[tabindex="-1"]
[role="status"] Loading the institutions.
[aria-label="Board notes"]
Create institution appears once the rows have loaded.

######## BOARD R2-I04-390.dc.html
Label: Institutions · Wombat
> R2-I04
> The list loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[tabindex="-1"]
[role="status"] Loading the institutions.
[aria-label="Board notes"]
Create institution appears once the rows have loaded.

######## BOARD R2-I05.dc.html
Label: Institutions · Wombat
> R2-I05
> The list's load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[tabindex="-1"]
[role="alert"] Could not load the institutions. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
[aria-label="Board notes"]
Try again focuses the h1 once the list reads (S3). Nothing under the alert.

######## BOARD R2-I05-390.dc.html
Label: Institutions · Wombat
> R2-I05
> The list's load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[tabindex="-1"]
[role="alert"] Could not load the institutions. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
[aria-label="Board notes"]
Try again focuses the h1 once the list reads (S3). Nothing under the alert.

######## BOARD R2-I06.dc.html
Label: Institutions · Wombat
> R2-I06
> A 200-character name, no contact email, Inactive
> Illustration
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[link] Create institution
[tabindex="-1"]
Institutions
| Name | Short code | Contact email | Status | Created | Actions |
| Demo Institution | DEMO | admin@demo.local | Active | 2026-11-10 | [link] Edit Demo Institution |
| Kgosi Kgari Teaching Hospital | KGK | paeds-admin@kgk.wombat.local | Active | 2026-11-10 | [link] Edit Kgosi Kgari Teaching Hospital |
| ‹An institution's name of 200 characters: it wraps inside its cell, breaking a word only where one word alone is wider than the cell, and never widens the table at 1280, 1100, 768 or 390 px› | ‹CODE› | None | Inactive | ‹YYYY-MM-DD› | [link] Edit ‹An institution's name of 200 characters: it wraps inside its cell, breaking a word only where one word alone is wider than the cell, and never widens the table at 1280, 1100, 768 or 390 px› |
[aria-label="Board notes"]
The name wraps in its own cell; "None" where there is no contact email; Inactive on .badge-withdrawn.

######## BOARD R2-I06-390.dc.html
Label: Institutions · Wombat
> R2-I06
> A 200-character name, no contact email, Inactive
> Illustration
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institutions
### [tabindex="-1"] Institutions
The institutions that hold Wombat's people and records.
[link] Create institution
[tabindex="-1"]
Institutions
| Name | Short code | Contact email | Status | Created | Actions |
| Demo Institution | DEMO | admin@demo.local | Active | 2026-11-10 | [link] Edit Demo Institution |
| Kgosi Kgari Teaching Hospital | KGK | paeds-admin@kgk.wombat.local | Active | 2026-11-10 | [link] Edit Kgosi Kgari Teaching Hospital |
| ‹An institution's name of 200 characters: it wraps inside its cell, breaking a word only where one word alone is wider than the cell, and never widens the table at 1280, 1100, 768 or 390 px› | ‹CODE› | None | Inactive | ‹YYYY-MM-DD› | [link] Edit ‹An institution's name of 200 characters: it wraps inside its cell, breaking a word only where one word alone is wider than the cell, and never widens the table at 1280, 1100, 768 or 390 px› |
[aria-label="Board notes"]
The name wraps in its own cell; "None" where there is no contact email; Inactive on .badge-withdrawn.

######## BOARD R2-I17.dc.html
Label: Institution · Wombat
> R2-I17
> The record loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Institutions
-
- [aria-current="page"] Institution
### [tabindex="-1"] Institution
[role="status"] Loading the institution.
[aria-label="Board notes"]
"Institution" while loading, as built; no Save, no Status action until it reads.

######## BOARD R2-I17-390.dc.html
Label: Institution · Wombat
> R2-I17
> The record loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Institutions
-
- [aria-current="page"] Institution
### [tabindex="-1"] Institution
[role="status"] Loading the institution.
[aria-label="Board notes"]
"Institution" while loading, as built; no Save, no Status action until it reads.

######## BOARD R2-I18.dc.html
Label: Institution · Wombat
> R2-I18
> The record's load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Institutions
-
- [aria-current="page"] Institution
### [tabindex="-1"] Institution
[role="alert"] Could not load the institution. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
[aria-label="Board notes"]

######## BOARD R2-I18-390.dc.html
Label: Institution · Wombat
> R2-I18
> The record's load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Institutions
-
- [aria-current="page"] Institution
### [tabindex="-1"] Institution
[role="alert"] Could not load the institution. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
[aria-label="Board notes"]

######## BOARD R2-I19.dc.html
Label: Institution unavailable · Wombat
> R2-I19
> Unavailable: /admin/institutions/99
> Illustration
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Institutions
-
- [aria-current="page"] Institution unavailable
### [tabindex="-1"] Institution unavailable
This institution does not exist, or you cannot open it.
[link] Go to Institutions
[aria-label="Board notes"]
Q17: an unknown id and one outside the caller's scope read the same; no id shown, never which cause.

######## BOARD R2-I19-390.dc.html
Label: Institution unavailable · Wombat
> R2-I19
> Unavailable: /admin/institutions/99
> Illustration
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] [aria-current="page"] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Institutions
-
- [aria-current="page"] Institution unavailable
### [tabindex="-1"] Institution unavailable
This institution does not exist, or you cannot open it.
[link] Go to Institutions
[aria-label="Board notes"]
Q17: an unknown id and one outside the caller's scope read the same; no id shown, never which cause.

######## BOARD R2-I20.dc.html
Label: Institution unavailable · Wombat
> R2-I20
> 1.23: Prof Mbatha types the Demo Institution's id
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institution unavailable
### [tabindex="-1"] Institution unavailable
This institution does not exist, or you cannot open it.
[link] Go to Home
[aria-label="Board notes"]
Q17, T10: their trail has no Institutions crumb, and the link is Go to Home. 1.23's Expect and states.md:899 change (R2-0-steps).

######## BOARD R2-I20-390.dc.html
Label: Institution unavailable · Wombat
> R2-I20
> 1.23: Prof Mbatha types the Demo Institution's id
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Institution unavailable
### [tabindex="-1"] Institution unavailable
This institution does not exist, or you cannot open it.
[link] Go to Home
[aria-label="Board notes"]
Q17, T10: their trail has no Institutions crumb, and the link is Go to Home. 1.23's Expect and states.md:899 change (R2-0-steps).

######## BOARD R2-I21.dc.html
Label: Access denied · Wombat
> R2-I21
> A.5.2: Prof Mbatha types /admin/institutions
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### You cannot open this page
Your role (Institutional admin) does not open this page.
If you need it for your work, ask the platform administrator.
[link] Go to Home
[aria-label="Board notes"]
Flow 01's Access denied, reused unchanged; nothing lit. /admin/jobs reads the same words.

######## BOARD R2-I21-390.dc.html
Label: Access denied · Wombat
> R2-I21
> A.5.2: Prof Mbatha types /admin/institutions
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
### You cannot open this page
Your role (Institutional admin) does not open this page.
If you need it for your work, ask the platform administrator.
[link] Go to Home
[aria-label="Board notes"]
Flow 01's Access denied, reused unchanged; nothing lit. /admin/jobs reads the same words.

######## BOARD R2-J11.dc.html
Label: Scheduled jobs · Wombat
> R2-J11
> Loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Scheduled jobs
### [tabindex="-1"] Scheduled jobs
The eight jobs Wombat runs on a schedule. Run one now, disable or enable one, or read every run in Run history.
[link] Run history
[tabindex="-1"]
[role="status"] Loading the scheduled jobs.
[aria-label="Board notes"]
The body of states/scheduled-jobs-list--loading.png, in the house words; Run history stays in the header (it needs no data).

######## BOARD R2-J11-390.dc.html
Label: Scheduled jobs · Wombat
> R2-J11
> Loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Scheduled jobs
### [tabindex="-1"] Scheduled jobs
The eight jobs Wombat runs on a schedule. Run one now, disable or enable one, or read every run in Run history.
[link] Run history
[tabindex="-1"]
[role="status"] Loading the scheduled jobs.
[aria-label="Board notes"]
The body of states/scheduled-jobs-list--loading.png, in the house words; Run history stays in the header (it needs no data).

######## BOARD R2-J12.dc.html
Label: Scheduled jobs · Wombat
> R2-J12
> The load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Scheduled jobs
### [tabindex="-1"] Scheduled jobs
The eight jobs Wombat runs on a schedule. Run one now, disable or enable one, or read every run in Run history.
[link] Run history
[tabindex="-1"]
[role="alert"] Could not load the scheduled jobs. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
[aria-label="Board notes"]

######## BOARD R2-J12-390.dc.html
Label: Scheduled jobs · Wombat
> R2-J12
> The load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Scheduled jobs
### [tabindex="-1"] Scheduled jobs
The eight jobs Wombat runs on a schedule. Run one now, disable or enable one, or read every run in Run history.
[link] Run history
[tabindex="-1"]
[role="alert"] Could not load the scheduled jobs. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
[aria-label="Board notes"]

######## BOARD R2-R03.dc.html
Label: Run history · Wombat
> R2-R03
> A.2.10: Status Failed, Show
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Scheduled jobs
-
- [aria-current="page"] Run history
### [tabindex="-1"] Run history
Every run of the eight jobs, newest first, kept for 90 days.
[aria-label="Filter the runs"] [role="search"]
Job
[selected] All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
entrustment-decision-expiry
msf-campaign-auto-close
msf-invitation-expiry-reminder
scheduled-job-run-retention
weekly-coordinator-digest
Status
All
Running
Succeeded
[selected] Failed
From (a South African day)
To (a South African day)
[button] Show
[button] Clear filters
#### [tabindex="-1"] Runs
[role="status"] [tabindex="-1"] No runs match these filters.
No runs match
The filters are kept above. Clear them to see all 21.
[button] Clear the filters
[aria-label="Board notes"]
"Clear the filters" clears them and moves the focus to the first filter, Job.

######## BOARD R2-R03-390.dc.html
Label: Run history · Wombat
> R2-R03
> A.2.10: Status Failed, Show
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Scheduled jobs
-
- [aria-current="page"] Run history
### [tabindex="-1"] Run history
Every run of the eight jobs, newest first, kept for 90 days.
[aria-label="Filter the runs"] [role="search"]
Job
[selected] All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
entrustment-decision-expiry
msf-campaign-auto-close
msf-invitation-expiry-reminder
scheduled-job-run-retention
weekly-coordinator-digest
Status
All
Running
Succeeded
[selected] Failed
From (a South African day)
To (a South African day)
[button] Show
[button] Clear filters
#### [tabindex="-1"] Runs
[role="status"] [tabindex="-1"] No runs match these filters.
No runs match
The filters are kept above. Clear them to see all 21.
[button] Clear the filters
[aria-label="Board notes"]
"Clear the filters" clears them and moves the focus to the first filter, Job.

######## BOARD R2-R08.dc.html
Label: Run history · Wombat
> R2-R08
> No runs at all
> Illustration
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Scheduled jobs
-
- [aria-current="page"] Run history
### [tabindex="-1"] Run history
Every run of the eight jobs, newest first, kept for 90 days.
[aria-label="Filter the runs"] [role="search"]
Job
[selected] All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
entrustment-decision-expiry
msf-campaign-auto-close
msf-invitation-expiry-reminder
scheduled-job-run-retention
weekly-coordinator-digest
Status
[selected] All
Running
Succeeded
Failed
From (a South African day)
To (a South African day)
[button] Show
#### [tabindex="-1"] Runs
No job has run yet
The scheduler runs each job at its time; Scheduled jobs shows when each runs next.
[aria-label="Board notes"]
A new install. No count line, no table, no pager.

######## BOARD R2-R08-390.dc.html
Label: Run history · Wombat
> R2-R08
> No runs at all
> Illustration
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Scheduled jobs
-
- [aria-current="page"] Run history
### [tabindex="-1"] Run history
Every run of the eight jobs, newest first, kept for 90 days.
[aria-label="Filter the runs"] [role="search"]
Job
[selected] All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
entrustment-decision-expiry
msf-campaign-auto-close
msf-invitation-expiry-reminder
scheduled-job-run-retention
weekly-coordinator-digest
Status
[selected] All
Running
Succeeded
Failed
From (a South African day)
To (a South African day)
[button] Show
#### [tabindex="-1"] Runs
No job has run yet
The scheduler runs each job at its time; Scheduled jobs shows when each runs next.
[aria-label="Board notes"]
A new install. No count line, no table, no pager.

######## BOARD R2-R10.dc.html
Label: Run history · Wombat
> R2-R10
> Loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Scheduled jobs
-
- [aria-current="page"] Run history
### [tabindex="-1"] Run history
Every run of the eight jobs, newest first, kept for 90 days.
[aria-label="Filter the runs"] [role="search"]
Job
[selected] All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
entrustment-decision-expiry
msf-campaign-auto-close
msf-invitation-expiry-reminder
scheduled-job-run-retention
weekly-coordinator-digest
Status
[selected] All
Running
Succeeded
Failed
From (a South African day)
To (a South African day)
[button] [aria-disabled="true"] Show
#### [tabindex="-1"] Runs
[role="status"] Loading the run history.
[aria-label="Board notes"]
The filters stay drawn; Show waits for the first read.

######## BOARD R2-R10-390.dc.html
Label: Run history · Wombat
> R2-R10
> Loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Scheduled jobs
-
- [aria-current="page"] Run history
### [tabindex="-1"] Run history
Every run of the eight jobs, newest first, kept for 90 days.
[aria-label="Filter the runs"] [role="search"]
Job
[selected] All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
entrustment-decision-expiry
msf-campaign-auto-close
msf-invitation-expiry-reminder
scheduled-job-run-retention
weekly-coordinator-digest
Status
[selected] All
Running
Succeeded
Failed
From (a South African day)
To (a South African day)
[button] [aria-disabled="true"] Show
#### [tabindex="-1"] Runs
[role="status"] Loading the run history.
[aria-label="Board notes"]
The filters stay drawn; Show waits for the first read.

######## BOARD R2-R11.dc.html
Label: Run history · Wombat
> R2-R11
> The load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Scheduled jobs
-
- [aria-current="page"] Run history
### [tabindex="-1"] Run history
Every run of the eight jobs, newest first, kept for 90 days.
[aria-label="Filter the runs"] [role="search"]
Job
[selected] All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
entrustment-decision-expiry
msf-campaign-auto-close
msf-invitation-expiry-reminder
scheduled-job-run-retention
weekly-coordinator-digest
Status
[selected] All
Running
Succeeded
Failed
From (a South African day)
To (a South African day)
[button] Show
#### [tabindex="-1"] Runs
[role="alert"] Could not load the run history. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
[aria-label="Board notes"]
The filters stay; nothing under the alert.

######## BOARD R2-R11-390.dc.html
Label: Run history · Wombat
> R2-R11
> The load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] [aria-current="page"] Scheduled jobs
-
[link] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Scheduled jobs
-
- [aria-current="page"] Run history
### [tabindex="-1"] Run history
Every run of the eight jobs, newest first, kept for 90 days.
[aria-label="Filter the runs"] [role="search"]
Job
[selected] All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
entrustment-decision-expiry
msf-campaign-auto-close
msf-invitation-expiry-reminder
scheduled-job-run-retention
weekly-coordinator-digest
Status
[selected] All
Running
Succeeded
Failed
From (a South African day)
To (a South African day)
[button] Show
#### [tabindex="-1"] Runs
[role="alert"] Could not load the run history. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
[aria-label="Board notes"]
The filters stay; nothing under the alert.

######## BOARD R2-L14.dc.html
Label: Audit log · Wombat
> R2-L14
> Loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action at Kgosi Kgari Teaching Hospital, newest first.
[aria-label="Filter the audit log"] [role="search"]
Category
[selected] All categories
Command
Authentication
Permission
Committee
MSF
Export
Data rights
Job
Activity type
Action
Part of a name: "Msf" finds the MSF commands.
Person's user id
Subject type
From (SAST)
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] [aria-disabled="true"] Show
#### [tabindex="-1"] Actions
[role="status"] Loading the audit log.
[aria-label="Board notes"]
The filters stay drawn; Show waits for the first read.

######## BOARD R2-L14-390.dc.html
Label: Audit log · Wombat
> R2-L14
> Loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action at Kgosi Kgari Teaching Hospital, newest first.
[aria-label="Filter the audit log"] [role="search"]
Category
[selected] All categories
Command
Authentication
Permission
Committee
MSF
Export
Data rights
Job
Activity type
Action
Part of a name: "Msf" finds the MSF commands.
Person's user id
Subject type
From (SAST)
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] [aria-disabled="true"] Show
#### [tabindex="-1"] Actions
[role="status"] Loading the audit log.
[aria-label="Board notes"]
The filters stay drawn; Show waits for the first read.

######## BOARD R2-L15.dc.html
Label: Audit log · Wombat
> R2-L15
> The load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action at Kgosi Kgari Teaching Hospital, newest first.
[aria-label="Filter the audit log"] [role="search"]
Category
[selected] All categories
Command
Authentication
Permission
Committee
MSF
Export
Data rights
Job
Activity type
Action
Part of a name: "Msf" finds the MSF commands.
Person's user id
Subject type
From (SAST)
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] Show
#### [tabindex="-1"] Actions
[role="alert"] Could not load the audit log. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
[aria-label="Board notes"]

######## BOARD R2-L15-390.dc.html
Label: Audit log · Wombat
> R2-L15
> The load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action at Kgosi Kgari Teaching Hospital, newest first.
[aria-label="Filter the audit log"] [role="search"]
Category
[selected] All categories
Command
Authentication
Permission
Committee
MSF
Export
Data rights
Job
Activity type
Action
Part of a name: "Msf" finds the MSF commands.
Person's user id
Subject type
From (SAST)
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] Show
#### [tabindex="-1"] Actions
[role="alert"] Could not load the audit log. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
[aria-label="Board notes"]

######## BOARD R2-E01.dc.html
Label: Audit entry · Wombat
> R2-E01
> 3.56: Sipho Ndlovu's future-date create
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry
### [tabindex="-1"] Audit entry
CreateActivityCommand by Sipho Ndlovu, 2026-11-10 19:01:41 SAST · Failed
#### What happened
**When
2026-11-10 19:01:41.861 SASTStored as 2026-11-10 17:01:41.861 UTC
**Category
Command
**Action
CreateActivityCommand
**Institution
Kgosi Kgari Teaching Hospital
**Result
FailedDate observed: The date cannot be after today (2026-11-10).
#### Who
**Name
Sipho Ndlovu
**User id
c22b8779-798b-4229-a0b8-b73a3d16999c
**Address
::/48Cut to its first 48 bits when stored.
#### What it was done to
No subject
[aria-label="Board notes"]
Prof Mbatha reads no Payload section: only the Administrator does. The trail's Audit log returns to Failures only, page 1 (R2-L13).

######## BOARD R2-E01-390.dc.html
Label: Audit entry · Wombat
> R2-E01
> 3.56: Sipho Ndlovu's future-date create
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry
### [tabindex="-1"] Audit entry
CreateActivityCommand by Sipho Ndlovu, 2026-11-10 19:01:41 SAST · Failed
#### What happened
**When
2026-11-10 19:01:41.861 SASTStored as 2026-11-10 17:01:41.861 UTC
**Category
Command
**Action
CreateActivityCommand
**Institution
Kgosi Kgari Teaching Hospital
**Result
FailedDate observed: The date cannot be after today (2026-11-10).
#### Who
**Name
Sipho Ndlovu
**User id
c22b8779-798b-4229-a0b8-b73a3d16999c
**Address
::/48Cut to its first 48 bits when stored.
#### What it was done to
No subject
[aria-label="Board notes"]
Prof Mbatha reads no Payload section: only the Administrator does. The trail's Audit log returns to Failures only, page 1 (R2-L13).

######## BOARD R2-E04.dc.html
Label: Audit entry · Wombat
> R2-E04
> 4.38: Thandi Zulu's decision entry
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry
### [tabindex="-1"] Audit entry
RecordCommitteeDecisionCommand by Thandi Zulu, 2026-11-10 ‹hh:mm:ss› SAST · Success
#### What happened
**When
2026-11-10 ‹hh:mm:ss.fff› SASTStored as 2026-11-10 ‹hh:mm:ss.fff› UTC
**Category
‹category›
**Action
RecordCommitteeDecisionCommand
**Institution
Kgosi Kgari Teaching Hospital
**Result
Success
#### Who
**Name
Thandi Zulu
**User id
‹…›
**Address
‹…›
#### What it was done to
**Type
‹…›
**Id
‹…›
[aria-label="Board notes"]
What happened, Who and What it was done to only: the stored summary's Rationale, Conditions and UndecidedNote are the Administrator's payload.

######## BOARD R2-E04-390.dc.html
Label: Audit entry · Wombat
> R2-E04
> 4.38: Thandi Zulu's decision entry
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry
### [tabindex="-1"] Audit entry
RecordCommitteeDecisionCommand by Thandi Zulu, 2026-11-10 ‹hh:mm:ss› SAST · Success
#### What happened
**When
2026-11-10 ‹hh:mm:ss.fff› SASTStored as 2026-11-10 ‹hh:mm:ss.fff› UTC
**Category
‹category›
**Action
RecordCommitteeDecisionCommand
**Institution
Kgosi Kgari Teaching Hospital
**Result
Success
#### Who
**Name
Thandi Zulu
**User id
‹…›
**Address
‹…›
#### What it was done to
**Type
‹…›
**Id
‹…›
[aria-label="Board notes"]
What happened, Who and What it was done to only: the stored summary's Rationale, Conditions and UndecidedNote are the Administrator's payload.

######## BOARD R2-E05.dc.html
Label: Audit entry · Wombat
> R2-E05
> 4.38: Refilwe Mokoena's revocation
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry
### [tabindex="-1"] Audit entry
RevokeEntrustmentDecisionCommand by Refilwe Mokoena, 2026-11-10 ‹hh:mm:ss› SAST · Success
#### What happened
**When
2026-11-10 ‹hh:mm:ss.fff› SASTStored as 2026-11-10 ‹hh:mm:ss.fff› UTC
**Category
‹category›
**Action
RevokeEntrustmentDecisionCommand
**Institution
Kgosi Kgari Teaching Hospital
**Result
Success
#### Who
**Name
Refilwe Mokoena
**User id
‹…›
**Address
‹…›
#### What it was done to
**Type
‹…›
**Id
‹…›
[aria-label="Board notes"]

######## BOARD R2-E05-390.dc.html
Label: Audit entry · Wombat
> R2-E05
> 4.38: Refilwe Mokoena's revocation
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry
### [tabindex="-1"] Audit entry
RevokeEntrustmentDecisionCommand by Refilwe Mokoena, 2026-11-10 ‹hh:mm:ss› SAST · Success
#### What happened
**When
2026-11-10 ‹hh:mm:ss.fff› SASTStored as 2026-11-10 ‹hh:mm:ss.fff› UTC
**Category
‹category›
**Action
RevokeEntrustmentDecisionCommand
**Institution
Kgosi Kgari Teaching Hospital
**Result
Success
#### Who
**Name
Refilwe Mokoena
**User id
‹…›
**Address
‹…›
#### What it was done to
**Type
‹…›
**Id
‹…›
[aria-label="Board notes"]

######## BOARD R2-E08.dc.html
Label: Audit entry unavailable · Wombat
> R2-E08
> Prof Mbatha types devadmin's RunScheduledJobNowCommand entry
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry unavailable
### [tabindex="-1"] Audit entry unavailable
This audit entry does not exist, or you cannot open it.
[link] Go to Audit log
[aria-label="Board notes"]
states.md:1091. An unknown id reads the same; never which cause (Q17's rule, the entry's words).

######## BOARD R2-E08-390.dc.html
Label: Audit entry unavailable · Wombat
> R2-E08
> Prof Mbatha types devadmin's RunScheduledJobNowCommand entry
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asInstitutional admin
MenuClose
[aria-label="Main"]
Acting asInstitutional admin
-
[link] Home
-
People
-
[link] Invitations
-
[link] Trainees
-
[link] Assessors
-
[link] Users
-
Curriculum
-
[link] Curricula
-
[link] EPAs
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] Entrustment decisions
-
Access and audit
-
[link] SSO mappings
-
[link] [aria-current="page"] Audit log
-
[link] My data rights
[link] Nolwazi Mbatha
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry unavailable
### [tabindex="-1"] Audit entry unavailable
This audit entry does not exist, or you cannot open it.
[link] Go to Audit log
[aria-label="Board notes"]
states.md:1091. An unknown id reads the same; never which cause (Q17's rule, the entry's words).

######## BOARD R2-E10.dc.html
Label: Audit entry · Wombat
> R2-E10
> Loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] [aria-current="page"] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry
### [tabindex="-1"] Audit entry
[role="status"] Loading this audit entry.

######## BOARD R2-E10-390.dc.html
Label: Audit entry · Wombat
> R2-E10
> Loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] [aria-current="page"] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry
### [tabindex="-1"] Audit entry
[role="status"] Loading this audit entry.

######## BOARD R2-E11.dc.html
Label: Audit entry · Wombat
> R2-E11
> The load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] [aria-current="page"] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry
### [tabindex="-1"] Audit entry
[role="alert"] Could not load this audit entry. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again

######## BOARD R2-E11-390.dc.html
Label: Audit entry · Wombat
> R2-E11
> The load error
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] Home
-
Platform
-
[link] Scheduled jobs
-
[link] [aria-current="page"] Audit log
-
[link] SSO mappings
-
[link] Data rights requests
-
Organisations
-
[link] Institutions
-
[link] Colleges
-
People
-
[link] Users
-
[link] Invitations
-
Catalogue
-
[link] EPAs
-
[link] Curricula
-
[link] Activity types
-
[link] Entrustment scales
-
Reviews
-
[link] Decisions due
-
[link] Committee reviews
-
[link] Decision panels
-
[link] My data rights
[link] Demo Administrator
[button] Sign out
[aria-label="Breadcrumb"]
-
[link] Home
-
-
[link] Audit log
-
- [aria-current="page"] Audit entry
### [tabindex="-1"] Audit entry
[role="alert"] Could not load this audit entry. Nothing has changed. Try again, or come back in a few minutes.
[button] Try again
