# Flow 18 round 3: the text of every board

Generated from `round-3/R3-*.dc.html` (T400, build-contracts § 0) by a stdlib script, run with `python -I`, as flow 17's was: style, script, svg, head and helmet blocks stripped. Each board opens with its label strip: its `<title>` as `Label:`, then each `cv-chip` of its `cv-chips` strip after `>`. Kept: headings, labels, aria-labels, placeholders, input values, alt text, `role`, `[checked]`, `[disabled]`, `[aria-disabled]`, `[selected]`, `[aria-selected]`, `[aria-haspopup]`, `[aria-current]`, `[aria-required]`, `[tabindex]`; an `anno` chip after `>`; a link is marked `[link]`, a button `[button]`. The order is by id: the Spec (1 to 3), Steps and the two check boards, then H, I, J, R, L, E, each at 1280 then its other widths (the widest first). Design data: a label strip, a chip, a `cv-note`, a caption saying Illustration, FIXTURES, DATA, NEW or PROPOSED is never a shipped word. A table row is one `| a | b |` line; a `dt` opens with `**`.

######## BOARD R3-0-spec.dc.html
Label: R3-0-spec — round 3 Spec (1 of 3)
### R3-0-spec · Flow 18 · Round 3 · Spec (1 of 3)
The correction round on round 2 (H Home, I the institutions, J the scheduled jobs, R run history, L the audit log, E an audit entry), on its own canvas, "Wombat 18 · Round 3", page "Round 3". Round 2's Spec boards (R2-0-spec, R2-0-spec-2) restated whole here with round 3's items; nothing is "as round 2". Every board links ds/wombat/tokens.css, ds/wombat/components/bundle.css, then f18r3.css; no inline style; the mark is /_blob/c6a099e64c4d721c4870011edee399ca.
#### 1 · The reference files, and where round 3 sits
As the check board (R3-0-check § A) says them: arrived and opened, project/r2/ (170 page boards and R2-0-spec, R2-0-spec-2, R2-0-steps, each as R2-….ref.html) and project/r2/f18r2.css; the design system v52 under ds/wombat/ (tokens.css, components/bundle.css, tokens.json, the three faces); the 30 captures under project/ref/baseline/, of which the 23 this round names were viewed. The mark is in this canvas's asset store (image/svg+xml, 1,328 bytes); it is referenced, its bytes not fetched. Could not read: none.
Round 3 is drawn on this canvas because "Wombat 18" holds 204 boards, at a canvas's ceiling. Each R3 board is drawn fresh from its R2 file, carrying everything no item changes. The R2 boards not redrawn stand on "Wombat 18": R2-I01, I02 (with 390 and 768), I03 (390 alone), I04, I05, I06, I17, I18, I19, I20, I21; J11, J12; R03, R08, R10, R11; L14, L15; E01, E04, E05, E08, E10, E11 (25 ids, 50 screens; the message counts 24 ids). Where an item supersedes their markup the build follows the item, not the standing board: S9 (.institutions-page on the list alone, so R2-I17–I20's class is inert there); N1 and N2 (h2.list-section-title, a count line with no role="status") on R2-R03, R08, R10, R11, L14, L15; C12 (names as stored) in the annotations of R2-E01, E08, I20, I21, which keep their "Prof Mbatha".
#### 2 · The decisions, all decided, and the boards that draw them
| Q | Decision | Boards |
| Q1 | Home's card holds everything; no System page, route or item | R3-H01–H12 |
| Q2 | Mail line: "Not configured" + "Mail is written to the log, not sent."; IEmailHealth keeps only "configured" and the queue's depth (Q18) | R3-H01, H03, H04, H08, H12 |
| Q3 | Jobs line: all succeeded, or names failed / never run / disabled / overdue jobs | R3-H01, H02, H05–H07, H09 |
| Q4 | 24 accounts (every but the erased one), locked apart; counted by K2's method | R3-H01, H02, H10 |
| Q5 | The record headed by its name; Status section; Deactivate… / Reactivate… asking; Q5's subtitle; T378 | R2-I01–I06, I17–I21; R3-I07–I16, I22 |
| Q6 (by T1) | Run history: select, Status, SAST days, Show/Clear, names, PagerControls 50, error whole | R3-R01, R02, R04, R05, R06, R07, R09; R2-R03, R08, R10, R11 |
| Q7 (by T2) | SAST everywhere; schedules in words + cron; stored UTC under the entry's time | every board |
| Q8 (by T3) | The audit filters then Show; window fixed and stated; address; live names; PagerControls | R3-L01–L13; R2-L14, L15 |
| Q9 (by T4) | Entry: What happened, Who, What it was done to, Payload; Unavailable | R3-E02, E03, E06, E07, E09; R2-E01, E04, E05, E08, E10, E11 |
| Q10 (by T5) | Results above the list, focused, in true words; disabled Run now with its reason | R3-J02–J10 |
| Q11 (by T6) | At 390 health first; every list stacks; 44 px | every -390 twin |
| Q12 (by T7) | What the build folds in | § 7 |
| Q13 | A drop in 24 h is Failing; no mail sent is not a state | R3-H04, H09, H12 |
| Q14 | No breakdown on Accounts | R3-H01 |
| Q15 | Names read live; an erased person by the pseudonym; by M3's read | R3-L*, R3-E09 |
| Q16 | Q5 holds for institutions only (T401 later) | none drawn |
| Q17 | One Unavailable for an institution, both roles | R2-I19, I20; Steps 1.23, A.5.5 |
| Q18 (added 2026-11-11) | Mail health kept, not counted in-process: each mail's outcome persisted (sent or dropped, its time, the refusal's words; never an address, subject or body); the 24 hours and last sent read from the table; "Nothing sent since ‹time›" the empty state; pruned by the existing retention job | R3-H03, H04, H08, H09, H12; R3-J01–J10 (the description) |
##### T1–T24, Decided 2026-11-11
| T | What | Boards | Changed in round 3 |
| T1 | Q6 as settled; "Run history", "Ran by", Finished folded. Not taken: "Job run history", "Triggered by" | R3-R*, R2-R03, R08, R10, R11 | — |
| T2 | Q7 as settled | all | C7: the Detail name carries the zone |
| T3 | Q8 as settled. Not taken: a Person filter by name | R3-L* | — |
| T4 | Q9's section names. Not taken: round 1's Event, Person, Subject | R3-E*, R2-E* | S4: the Payload section unnamed, its region named once |
| T5 | Q10's words. Not taken: round 1's "‹key› ran and succeeded. It started at …" | R3-J* | K5: a job due at once reads "Enabled ‹key›. It runs at the next check, within 30 seconds." |
| T6 | Q11; the stacks (Institutions, Scheduled jobs, Audit log 1150; Run history 1024) | all 390, 768 | S8: the reason is readability; S9: a selector-list addition |
| T7 | Q12 | Spec | — |
| T8 | Health in words, a badge a line, no dot, fixed order | R3-H* | S1: the links in the cards' footers |
| T9 | Home's partial answer with the database down | R3-H08 | K11: a per-part DTO; Q18: a mail server set reads Not read |
| T10 | Unavailable's trail and link by the caller's way in | R2-I19, I20 | — |
| T11 | The institution's results by name; Reactivate's dialog | R3-I11–I16 | M1: the dialogs in action mode |
| T12 | Create subtitle; field help; Status sentences; "KGK · Active" | R3-I07–I16, R2-I17 | — |
| T13 | The health lines' words beyond Q2 and Q3 | R3-H* | Q18: the empty form "Nothing sent since ‹…›"; K3: the drop with no refusal |
| T14 | Accounts: title, "24" / "accounts", the sentence, the locked line, "Users" | R3-H* | S1: "Users" a footer button |
| T15 | The new subtitles and descriptions | R3-J*, R*, L*, E* | Q18: scheduled-job-run-retention's description |
| T16 | "None while disabled", "Not run yet", "Running…", Duration "—" | R3-J*, R07 | — |
| T17 | Count lines and empty states | R3-R*, L* | K16: the audit empty body; K19: the singulars |
| T18 | Run history's filters in the address | R3-R09, H05 | — |
| T19 | The Institution filter, and the institution under the person (devadmin) | R3-L05–L08 | K15: a three-kind filter parameter |
| T20 | An actor-less row: "MSF respondent (no account)"; the entry's sentence; "No signed-in person" for any other | R3-L05, L06, E02 | K8: ActorKind; "No signed-in person" reaches the failed sign-ins |
| T21 | The window line, on Show and on arrival | R3-L* | — |
| T22 | RunScheduledJobNowCommand refuses a disabled job | R3-J09 | K6: one typed refusal, under the job's lock |
| T23 | The failures' words | R3-J10, I22 | K6: anything not a refusal reads T23's words, never ex.Message |
| T24 | Created off the record; the list's Created a day | R3-I*, R2-I01, I02 | — |
Round 2's corrections changed by round 3: S4 ("Mail as it is" with the database down) is amended by Q18 (a mail server set reads Not read); N12 (the Detail name) gains the zone by C7; N11 (the payload region named by its h2) is kept, by S4. M3's read and S8's wording are decided (§ 7, § 10).
#### 3 · This round's items, each with its boards
| Id | Item | Boards / place |
| M1 | ConfirmDialog's action mode: dialog.dialog-scrim.dialog-card (+ .cv-dialog) > div.form-container > h2, p, div.form-actions > Cancel (outline, autofocus), the action | R3-I13, I15 (+390) |
| M2 | PagerControls' order and markup: div.pager-info, div.pager-actions, div.pager-page-size; no span | R3-R01, R02, R04, R05-390, R06, R07, R09; R3-L01–L10, L12, L13 |
| M3 | Names through DataRightsPeople.StaffNamesAsync with the erasure records | § 7, § 17; R3-E09 |
| M4 | A.5.5 changes under Q17 | Steps |
| M5 | A.2.10 starts on Scheduled jobs: 4 + 5 at both widths | Steps |
| K2 | CountAccountsAsync on IUserAdministrationService | § 7, § 17 |
| K3 | Outcomes at SendWithRetryAsync's returns; drops with no refusal; the queue in-process | § 7, § 9, § 16 |
| K4 | IsDue shared; Running neither failing nor overdue; an unreadable cron's sentence | § 7, § 9, § 16; R3-H07 note |
| K5 | Next run by the host's rule; "At the next check, within 30 seconds" | § 7, § 9, § 16; R3-J03 note |
| K6 | DispatchNowAsync returns the run; ScheduledJobRefusal; T23 for anything else; TriggeredBy the caller | § 7 |
| K7 | One time helper in Web; South African days; the cron fallback | § 7 |
| K8 | ActorKind in the audit DTO | § 7, § 17 |
| K9 | The address sentence by its prefix (48 or 24 bits) | § 9, § 16 |
| K10 | A Failed row's stored text; "The action failed." | § 7, § 9, § 16 |
| K11 | The per-part DTO; how long R3-H08's state can be seen | § 7, § 16; R3-H08 note |
| K12 | InstitutionEdit's /not-found call at :246-250 | § 7 |
| K13 | CatalogueLinks.InstitutionCreated(id) | § 7 |
| K14 | No maxlength (and with S6 no required); the two built refusals | R3-I07–I16, I22; § 9 |
| K15 | The Institution filter's three kinds; names by lookup; the Category map | § 7, § 17 |
| K16 | The audit empty body true of any filter | R3-L11; Steps (states.md:1086) |
| K17 | Status pressed with unsaved typing | § 16; Steps |
| K18 | Run history's "kept for 90 days." kept | § 9 |
| K19 | A count line's singular | § 9 |
| K20 | Mail outcomes from whichever host sends | § 7, § 15 |
| C3 | "dispatched" in the Actuals | Steps |
| C4 | The search rows completed; "Back to institutions" | Steps |
| C5 | Every NEW and decided word quoted | § 9 |
| C6 | One Illustration rule | § 4; the chips |
| C7 | The Detail name with " SAST" | R3-L01–L10, L12, L13 |
| C8 | Today's A.2.10 listed field by field | Steps |
| C9 | Round 2's S8 points to § 10 | § 3 (the table below) |
| C10 | 1.1's 9 drawn, 1.11's 11 words only | Steps |
| C11 | R3-L05's note on devadmin's own rows | R3-L05 (+390, 768) |
| C12 | "Nolwazi Mbatha" in annotations and titles | R3-L01, L04 and every board drawn; Spec, Steps, check |
| C13 | Kabelo Sithole among the rows | § 8 |
| S1 | Home's links as footer buttons; DashboardCard's markup | R3-H01–H12 |
| S2 | One column at 768; two 901–1099; three from 1100 | § 12, § 13; measured |
| S3 | f18r3.css R2 without its focus block | f18r3.css; § 9 |
| S4 | The payload region named once | R3-E02, E03, E06, E07, E09 |
| S5 | Retiring the status dots: four places | § 7, § 13, § 14 |
| S6 | aria-required on every starred control; no required | R3-I07–I16, I22 |
| S7 | Job a row header on Scheduled jobs and Run history | R3-J01–J10; R3-R01, R02, R04, R05-390, R06, R07, R09 |
| S8 | The stacks kept as a readability choice | § 10 |
| S9 | The page classes join the selector lists; .institutions-page on the list alone | f18r3.css C1; § 9 |
| S10 | DataTable's shadow and roles, not drawn | § 9, § 13 |
| N1 | h2.list-section-title tabindex="-1" | R3-R01, R02, R04, R05-390, R06, R07, R09; R3-L01–L13 |
| N2 | The count line p[tabindex="-1"], no role="status" | the same boards |
| N3 | Disabled and Not configured stay .badge-accepted | § 11 |
| N4 | Badge edges on their own tints | § 14 |
| N5 | ContrastTests pins muted on --success-bg | § 7, § 14 |
| N6 | No bracket in an id, for or aria-* value | every R3 board (checked) |
| N7 | The health line's hidden colon | § 9 |
| A1 | Steps cite R3 ids | Steps |
| A2 | Twins carry the 1280 board's words and markup | every twin |
| A3 | One h1, no inline style, no undefined class, no raw colour, ids clean, references resolve, names start with their words | every board |
##### Round 2's corrections, kept (C9: S8 now points to § 10)
| Item | Done | Boards |
| M1 | Accounts 24, "None is locked.", no breakdown; .accounts-breakdown gone | R3-H01–H10 |
| M2 | (a) Reactivate… asks; (b) Q5's subtitle | R3-I14–I16; R2-I01–I06 |
| M3 | Q2's sentence, then the second line | R3-H01–H10 |
| M4 | Own account via Users; 14 + 1.23 Expects change; the extra rows | Steps |
| S1 | No B or C part anywhere | all |
| S2 | Every PROPOSED item listed | § 6 |
| S3 | The house load-error sentence with Try again, nothing under it | R2-I05, I18, J12, R11, L15, E11 |
| S4 | Database down: jobs and Accounts Not read; Mail as it is while Not configured (amended by Q18) | R3-H08 |
| S5 | Overdue in Q3's sense (the host's rule, K4) | R3-H07, H09 |
| S6 | Unavailable for both roles | R2-I19, I20 |
| S7 | Unavailable as § Alerts draws it | R2-I19, I20, E08 |
| S8 | Each list in the stack its columns need; the sweep; columns at 1280 | § 10 (C9) |
| S9 | Superseded by M3 ("Ran by" names come from DataRightsPeople, a DATA row) | § 17 |
| S10 | Every state round 1 left out is a board | § 5 |
| S11 | KGK's contact at A.7.11 hod.paediatrics@kgk.wombat.local | R2-I03 |
| N1 | No gendered pronoun; "they" | all |
| N2 | Subtitle "KGK · Active" / "DEMO · Inactive"; results by name | R3-I11–I16, I22 |
| N3 | The field help (decided, T12) | R3-I07–I16 |
| N4 | One "Status": the h2 | R3-I11–I16 |
| N5 | Running amber; Overdue .badge-overdue | R3-J05, J06, R07, H07 |
| N6 | Brackets the code holds filled | R3-J08, L02, E02, E03; R2-E01 |
| N7 | Run now refused moves the focus to the result | R3-J06 |
| N8 | PagerControls with "Per page:" 10/20/50/100, set to 50 (order by M2) | R3-R*, R3-L* |
| N9 | No "Error: None"; Running once; "No runs match these filters." | R3-R*, R3-J* |
| N10 | The audit rows drop Subject and the address | R3-L*, R3-E* |
| N11 | The payload region named by aria-labelledby its h2 (kept by S4) | R3-E02, E03, E06, E07, E09 |
| N12 | Detail's name keeps the date and, by C7, the zone: "Detail: Login at 2026-11-10 20:24:10 SAST" | R3-L* |
| N13 | Both stack classes; every caption a name | every list |
| N14 | states.md:1093 "Nine jobs" → eight | Steps |
#### 4 · Illustrations: one rule (C6)
The orange chip, and "Illustration" in the canvas title, mark every state no runbook step and no states.md row reaches, EXCEPT a loading state and a load error, which carry none: every page has them, and the replay's states pass reaches them by holding a read or failing one. So the six load errors (R2-I05, I18, J12, R11, L15, E11) and the seven loadings (R2-H11 now R3-H11, R2-I04, I17, J11, R10, L14, E10) carry none. None draws two states that cannot hold at once.
The 28: R3-H03, H04, H05, H06, H07, H08, H09, H10, H12, I09, I10, I22, J05, J06, J07, J08, J09, J10, R06, R07, R09, L08, E06, E07, E09, and the standing R2-I06, I19 and R08. Round 2's 27 stand by the rule; R3-H12 adds one.
#### 5 · The boards
Redrawn (120 screens): 56 at 1280 — R3-H01–H11, I07–I16, I22, J01–J10, R01, R02, R04, R06, R07, R09, L01–L13, E02, E03, E06, E07, E09 — their 56 twins at 390, R3-R05 at 390 alone, and 7 at 768 (H01, I12, J01, R01, L01, L05, E03). New (2): R3-H12 at 1280 and 390. Sheets: R3-0-check, R3-0-check-2, R3-0-spec, -spec-2, -spec-3, R3-0-steps. Standing on "Wombat 18" (50 screens): the list in § 1.
#### 6 · PROPOSED (THIS ASK)
This message's own settlements, which the operator has not yet seen. T1–T24 are decided and are no longer here.
| Item | What | Boards / place |
| K2 | The method name CountAccountsAsync on IUserAdministrationService | § 7 |
| K3 (c) | A mail abandoned as the host stops, or whose last try failed in the backoff wait, recorded dropped with no refusal; the Failing line then has no refusal clause | § 9, § 16 |
| K4 | A Running last run is neither failing nor overdue; an unparsable cron's sentence, badge Failing | § 9, § 16; R3-H07 note |
| K5 | "At the next check, within 30 seconds"; "Enabled ‹key›. It runs at the next check, within 30 seconds." | § 9; R3-J03 note |
| K6 | The typed refusal ScheduledJobRefusal, in Application, carrying its sentence | § 7 |
| K7 | The time helper in Web, replacing ErrorPages.cs and ActivityListWords.cs's copies; the fallback "cron ‹expr›, UTC" with no words line | § 7 |
| K8 | ActorKind: Person, MsfRespondent, None | § 7, § 17 |
| K10 | AuditPipelineBehavior stores a refusal's words and, for any other exception, "The action failed." | § 7, § 9 |
| K15 | A filter parameter of three kinds (all, none, one id) | § 7 |
| K16 | "Nothing matches these filters between these times. Widen From and To, or clear the filters." | R3-L11 (chip) |
| K17 | Unsaved typing stays in the fields when the status changes | § 16 |
| K20 | Home reads both hosts' mail through the table | § 7, § 15 |
| S1 | The links as buttons in the cards' footers | R3-H01–H10, H12 (chip) |
| S4 | The Payload section unnamed; the region named once | R3-E02, E03, E06, E07, E09 (markup only, no chip) |
| N2 | The count line p[tabindex="-1"], not a live region | the list boards (markup only, no chip) |
| Q18 | EmailOutcome (Domain; its id, the moment in UTC, Sent or Dropped, the refusal or none, redacted by EmailLog.Redact, bounded at 2,000 characters) with a migration and its Designer file; written by either host at K3's points; LoggingEmailSender writes none; scheduled-job-run-retention deletes rows older than 90 days, and its description says so; "since ‹time›" is the window's start, now less 90 days; a mail server set and the database down reads Not read | R3-H03, H04, H08, H09, H12; R3-J01–J10 (chip) |
| NEW words | K3's drop with no refusal; K4's cron sentence; K5's two; K9's 24-bit sentence; K10's "The action failed."; K16's body; Q18's description (each quoted in § 9 beside what it replaces) | § 9 |

######## BOARD R3-0-spec-2.dc.html
Label: R3-0-spec-2 — round 3 Spec (2 of 3)
### R3-0-spec-2 · Flow 18 · Round 3 · Spec (2 of 3)
#### 7 · The changes the boards need (code)
Round 2's rows, each amended; decided ones say their Q or T; this round's settlements are PROPOSED (§ 6).
| Where | Change |
| GetAdministratorDashboardSummaryQuery | A partial answer when the database does not answer (T9): each part (accounts, jobs, mail's table read) read and caught on its own, with a per-part read or not-read and the moment the database failed — a new DTO shape; today AdministratorDashboardSummaryDto holds DatabaseHealthy and TotalUserCount only (K11). The accounts by K2's method; the handler stops looping WombatRoles.All (GetAdministratorDashboardSummaryQuery.cs:44-52) and reads no Identity type. Mail from the EmailOutcome table and IEmailHealth (Q18). Each job's key, enabled, last status, time, first error line; "Overdue" by the shared IsDue (K4). DATA |
| IUserAdministrationService (K2) | One new method, PROPOSED name CountAccountsAsync, implemented in Infrastructure (it sees the Identity users and DataRightsErasureRecord): the accounts no erasure record names, and the locked among them (a lockout still running). An erased account is in neither figure (its LockoutEnd is MaxValue, UserDeactivation.cs:3-13). IApplicationDbContext exposes Domain entities only; the architecture tests hold it. DATA |
| EmailOutcome (Q18, K3) | NEW Domain entity: id, the moment in UTC, Sent or Dropped, the refusal or none (through EmailLog.Redact, which quotes addresses; at most 2,000 characters). A migration; a hand-written one carries its Designer file (CLAUDE.md). Written at SendWithRetryAsync's two returns (EmailWorker.cs:150 sent, :160 dropped) for every mail, not through ReportAsync, which returns early for a mail with no DeliveryKey (:195-198); a mail abandoned as the host stops (AbandonAsync, :104) and one whose last try failed in the backoff wait (:179) recorded dropped with no refusal (PROPOSED). Written by whichever host sends: both the Web and the Api host start the worker when SmtpHost is set (DependencyInjection.cs:16-25), so Home reads both (K20). LoggingEmailSender writes none. |
| IEmailHealth (Q2, Q18) | Keeps only "configured" (from configuration) and the queue's depth (EmailQueue's Channel, in-process, read from the host that draws Home). The 24 hours and "last sent" are read from the table, so a restart no longer resets them. Round 2's "the Web host's only" goes (K20). |
| ScheduledJobRunRetentionJob (Q18) | Also deletes EmailOutcome rows older than 90 days; its Description "Deletes job run records and mail outcomes older than 90 days." |
| Tests (Q18) | A sent and a dropped mail recorded; a drop with no refusal; the prune; a restart that keeps the 24 hours. |
| ScheduledJobHost / the dashboard query (K4) | IsDue (ScheduledJobHost.cs:110-119) extracted into one shared function the host and the dashboard query both call; a job whose last run is Running is neither failing nor overdue (it holds its lock, :80-85); a stored cron that does not parse reads its own sentence (§ 9). |
| ScheduledJobDto, GetScheduledJobStatusQueryHandler | The last run's error, finish and runner (DATA); the schedule in words and SAST (K7); each job's Description without its UTC clause (T15). Next run by the host's rule, the next occurrence after the last run's start (:110-119), not after now (GetScheduledJobStatusQueryHandler.cs:38-49); a job due as soon as it is enabled reads "At the next check, within 30 seconds" (K5). |
| IScheduledJobDispatcher, RunScheduledJobNowCommand (K6) | DispatchNowAsync (IScheduledJobDispatcher.cs:5) returns the run (status, start, finish, error), with its implementation and tests. The handler reads the job's definition and refuses a disabled job under the job's lock, so a Disable between the check and the run is honoured. Both refusals (already running, ScheduledJobDispatcher.cs:66-68; disabled, T22) are one typed refusal, PROPOSED name ScheduledJobRefusal (Application, carrying its sentence), shown as a warning Alert in its words; anything else (a key not in the registry, a database failure) is a failure in T23's fixed words, its cause to the log, never ex.Message (today ScheduledJobsList.razor:149, :179). TriggeredBy is the caller's id from the command, never the literal "unknown" (:171). DATA |
| ScheduledJobsList | The result above the list, focused (today after the table, :79-84); Job the row's header (S7); Run now aria-disabled with its reason (today disabled, :64); Running…; the house load error and a loading line; the 1150 px stack. |
| The time helper (K7) | PROPOSED: one helper in Web, beside and replacing the two private fixed-offset copies (ErrorPages.cs:51, :136; ActivityListWords.cs:13, :94); a .razor file may not name Domain's ProgrammeCalendar. A time with seconds or milliseconds and " SAST"; a South African day; a day's UTC range [00:00 SAST, the next 00:00 SAST) for run history's From and To (today UTC days, GetScheduledJobRunsQueryHandler.cs:33, :39); the audit filters' datetime-local read as SAST and converted to UTC (today tagged UTC, AuditList.razor:185-186). The schedule's words cover the jobs' three shapes; any other cron reads "cron ‹expr›, UTC", no words line. |
| GetScheduledJobRunsQuery | Page and PageSize with a total (today Take(200)); From and To as South African days (K7); Ran by through DataRightsPeople.StaffNamesAsync (M3; round 2's "an existing read" goes). DATA |
| ScheduledJobRunsList | "Run history"; the select; Show and Clear filters; the address (T18); PagerControls (M2's order); h2.list-section-title (N1); the count line p[tabindex="-1"] (N2); Job the row's header (S7); the empty states; the house load error; no Back to jobs. |
| InstitutionsList | Q5's subtitle; the columns; StackWide in the 1150 px stack (.institutions-page on the list alone, S9); the house load error and a loading line. |
| InstitutionCreate | T378's refusals; the mark line; aria-required on Name and Short code, no required, no maxlength (S6, K14); the subtitle; no Back; lands on the record through CatalogueLinks.InstitutionCreated(id) (K13: CatalogueLinks.cs:38 and :83-103 hold CollegeCreated and TreeCreated only); the record reads the token once and strips it (WithoutTokens). |
| InstitutionEdit (Administrator) | h1 and tab the name; the subtitle; no Back; the Status section with Deactivate… and Reactivate… in ConfirmDialog's action mode (M1; ConfirmDialog.razor:52-80, not form mode :32-48); no Active box; the results by name (T11); aria-required (S6); Unavailable for null, both roles, in place of the /not-found call at InstitutionEdit.razor:246-250 (K12; ":102-105" is the Status read-out for a non-Administrator). |
| ListAuditEntriesQuery | From and To resolved and passed in; the Institution filter as a parameter of three kinds, all, none, one id, which an int? cannot say (K15, PROPOSED); the institution's name per row a lookup (DATA); the Category labels a map (K15); names through DataRightsPeople.StaffNamesAsync (M3); ActorKind (K8). |
| DataRightsPeople (M3) | StaffNamesAsync (src/Wombat.Application/Features/DataRights/Queries/DataRightsPeople.cs:69) is internal to Application, so ListAuditEntriesQueryHandler, GetAuditEntryByIdQueryHandler and GetScheduledJobRunsQueryHandler call it, or a shared reader of it moved beside it, with the erasure records of the ids read (DataRightsErasures): an erased person reads as their pseudonym, a living one by the live "First Last", an account with no name by its id. A blank live name never falls back to the row's stored ActorDisplay, which keeps the real name (ErasureExecutor.cs clears the account's names, :440-452, and rewrites no audit row); the stored display is used only where a row has no user id (Q15). |
| The audit DTO (K8) | PROPOSED field ActorKind (Person, MsfRespondent, None), computed in the query from the row's user id and its Action: "SubmitMsfResponseCommand" with no user id is MsfRespondent (the row carries no flag; IAnonymousAuditedCommand, AuditPipelineBehavior.cs:148-156). "No signed-in person" (T20) on every other row with no user id, a failed sign-in included. |
| AuditPipelineBehavior (K10) | PROPOSED: at :104 stores a refusal's words (a FieldRefusalException's, a workflow refusal's, a ValidationException's validator messages reduced as RefusalText.Of reduces them on a page, the typed refusals' sentences) and, for any exception that is no refusal, "The action failed.", its text to the application log. The four drawn refusals read as drawn; the three round 2 marked inferred stay inferred, and the build says on its record where the stored text differs. |
| AuditList | The filters' order and labels; Show and Clear filters after them; the address; the window line; PagerControls (M2), the focus on h2.list-section-title after a turn, on the count line after Show (N1, N2); the Detail name with " SAST" (C7); the 1150 px stack; the house load error. |
| AuditDetail | h2 sections; the institution by name (DATA); the payload laid out in its region, named once by its h2 (S4); the address sentence by its prefix (K9); Unavailable for null; the trail carrying the list's address; no Back to log. |
| AdministratorDashboard | The two cards in DashboardCard's markup: h2 with the title in a span, .dashboard-card-body, the links in .dashboard-card-footer as a.btn.btn-sm.btn-outline (as AssessorDashboard.razor:39-41), the skeleton while loading (S1). |
| Retiring the status dots (S5) | One change, four places: app.css's .status-dot and .status-dot.ok/.warn/.err (app.css:2111-2131, used only by AdministratorDashboard.razor:16-22) removed; ContrastTests.cs:319 (tests/Wombat.Web.Tests/Design/), which resolves .status-dot.{state}, deleted or rewritten; DESIGN.md:324's row "Status dots on surface …" removed; DefinedClassTests checked for the class. |
| ContrastTests (N5) | Pins --muted-text on --success-bg (4.52:1, 0.02 to spare). |
| DataTable (S10) | The build draws div.table-container.shadow and, on a stacked table, role="table" / "rowgroup" / "row" / "columnheader" / "cell"; the boards draw neither (flows 16 and 17's convention), so the replay compares a 1 px shadow, not a defect. |
| app.css: f18r3.css R1 | NEW: .health-list, .health-line, .health-line-name, .health-line-text and the 30rem container query (quoted in § 9). |
| app.css: f18r3.css R2 | NEW: ".code-block--region { max-height: 28rem; overflow: auto; }" only; its ring is the page's (app.css:512). |
| app.css: f18r3.css C1 | CHANGED: ".institutions-page", ".jobs-page" and ".audit-page" added to each of the 7 selector lists of the 1150 px stack block (app.css:7708-7769); ".job-runs-page" to each of the 7 of the 1024 px block (app.css:7633-7702). Counted from bundle.css (its lines 7626-7687 and 7551-7620; bundle.css runs 82 lines behind app.css). No declaration is written. |
#### 8 · FIXTURES
Round 2's, unchanged: the T398 replay (wombat_scenario_t398); D = 2026-11-10; every time SAST = the stored UTC + 2 h. Use FIXTURES only; a value not given is a bracket ‹…›, never a control's value.
- devadmin, display name "Demo Administrator", Administrator, Global; "Acting as Administrator". Nolwazi Mbatha, Institutional admin, Kgosi Kgari Teaching Hospital (KGK); "Acting as Institutional admin". In annotations and canvas titles a person is named as stored, no title (C12).
- Seen in the rows: as round 2, with Kabelo Sithole, Kgosi Kgari Teaching Hospital, a Login at 2026-11-10 19:20:46 SAST on R3-L05 (from 3.57-1; the runbook's Sub-speciality admin) (C13).
- Accounts: 24 at A.6.2 (25 less Sipho Ndlovu's erased one); 9 at 1.1; 11 at 1.11.
- The eight jobs, by key, as R3-J01 draws them; scheduled-job-run-retention's description "Deletes job run records and mail outcomes older than 90 days." (Q18, replacing "Deletes job run records older than 90 days.").
- The mail lines (badge, then the sentence): Not configured "Mail is written to the log, not sent." then "To send mail, set Email__SmtpHost on the server and restart Wombat." / OK "Mail is sent. Last sent ‹YYYY-MM-DD hh:mm› SAST; none waiting; none dropped in the last 24 hours." (read from the table) / OK, nothing sent in what the table holds "Nothing sent since ‹YYYY-MM-DD hh:mm› SAST; none waiting; none dropped in the last 24 hours." (R3-H12) / Failing "‹n› mails were dropped in the last 24 hours, the last at ‹YYYY-MM-DD hh:mm› SAST: ‹the mail server's refusal›. The application log names each one." / Failing, the last drop with no refusal "‹n› mails were dropped in the last 24 hours, the last at ‹YYYY-MM-DD hh:mm› SAST. The application log names each one." (NEW, words only) / Not read, a mail server set and the database down "Not read: the database did not answer."
- The Detail button's name: "Detail: ‹action› at ‹YYYY-MM-DD hh:mm:ss› SAST", so "Detail: Login at 2026-11-10 20:24:10 SAST" (C7).
- Run history at A.2.10: 21 runs, 10 by hand (Ran by Demo Administrator) and 11 by the Scheduler; 22 at A.7.11.
- The audit log: 166 KGK rows from 18:58 SAST (3.55); 174 for devadmin from 19:00 SAST (3.57); the four KGK failures at 19:02:02, 19:01:45, 19:01:41 and 19:00:06 SAST.
- CAPTURE-ONLY: empty. No value rests on a capture alone.
#### 9 · Classes, and every message with its words
| Row | Class | Kind | Why, and the rule |
| f18r3.css R1 | .health-list, .health-line, .health-line-name, .health-line-text | NEW (T8) | Home's lines: name and badge beside the sentence; one column under a 30rem list (container query). As round 2 wrote it; S1 moves the links into the footers, so R1 gains nothing..health-list { container: health / inline-size; display: flex; flex-direction: column; list-style: none; margin: 0; padding: 0; } .health-line { border-top: 1px solid var(--border-color); display: grid; gap: var(--space-xs) var(--space-md); grid-template-columns: 10rem minmax(0, 1fr); padding: var(--space-md) 0; } .health-line:first-child { border-top: 0; padding-top: var(--space-xs); } .health-line:last-child { padding-bottom: 0; } .health-line-name { align-items: flex-start; display: flex; flex-direction: column; font-weight: 600; gap: var(--space-xs); } .health-line-text { display: flex; flex-direction: column; gap: var(--space-xs); min-width: 0; overflow-wrap: anywhere; } .health-line-text p, .health-line-text ul { margin: 0; } .health-line-text ul { padding-left: var(--space-lg); } @container health (width < 30rem) { .health-line { grid-template-columns: minmax(0, 1fr); } .health-line-name { align-items: center; flex-direction: row; flex-wrap: wrap; gap: var(--space-sm); } } |
| f18r3.css R2 | .code-block--region | NEW (Q9), CHANGED by S3 | A .code-block modifier: a ceiling and its own scroll. Its ring is the page's: app.css:512's global :focus-visible draws it, so the block that redeclared it goes..code-block--region { max-height: 28rem; overflow: auto; } |
| f18r3.css C1 | .institutions-page, .jobs-page, .audit-page in the 1150 px stack; .job-runs-page in the 1024 px stack | CHANGED (T6), a selector-list addition (S9) | The build adds the page classes to each existing selector list (7 in each block, counted) and writes no declaration; the alternative, DataTable's Class parameter carrying one table modifier, is recorded and not taken. .institutions-page goes on the list only: no record board this round draws carries it (R3-I07–I16, I22: div.record-page alone); the standing R2-I17–I20 keep it, inert, and the build puts it on the list alone. The canvas cannot edit bundle.css, so round 2's written-out rules stand in, unchanged:@media (max-width: 1150px) { .institutions-page .clinic-table--stack-wide thead, .jobs-page .clinic-table--stack-wide thead, .audit-page .clinic-table--stack-wide thead { border: 0; clip: rect(0, 0, 0, 0); height: 1px; margin: -1px; overflow: hidden; padding: 0; position: absolute; white-space: nowrap; width: 1px; } .institutions-page .clinic-table--stack-wide tbody tr, .jobs-page .clinic-table--stack-wide tbody tr, .audit-page .clinic-table--stack-wide tbody tr { border-bottom: 1px solid var(--border-color); display: grid; gap: var(--space-xs); padding: var(--space-sm) var(--space-md); } .institutions-page .clinic-table--stack-wide tbody tr:last-child, .jobs-page .clinic-table--stack-wide tbody tr:last-child, .audit-page .clinic-table--stack-wide tbody tr:last-child { border-bottom: 0; } .institutions-page .clinic-table--stack-wide td, .institutions-page .clinic-table--stack-wide tbody th, .jobs-page .clinic-table--stack-wide td, .jobs-page .clinic-table--stack-wide tbody th, .audit-page .clinic-table--stack-wide td, .audit-page .clinic-table--stack-wide tbody th { border: 0; padding: 0; width: auto; } .institutions-page .clinic-table--stack-wide th[scope="row"], .jobs-page .clinic-table--stack-wide th[scope="row"], .audit-page .clinic-table--stack-wide th[scope="row"] { display: block; } .institutions-page .clinic-table--stack-wide td[data-label]::before, .jobs-page .clinic-table--stack-wide td[data-label]::before, .audit-page .clinic-table--stack-wide td[data-label]::before { color: var(--muted-text); content: attr(data-label) ": "; content: attr(data-label) ": " / ""; font-weight: 600; } .institutions-page .clinic-table--stack-wide .btn, .jobs-page .clinic-table--stack-wide .btn, .audit-page .clinic-table--stack-wide .btn { min-height: 2.75rem; } } @media (max-width: 1024px) { .job-runs-page .clinic-table--stack-wide thead { border: 0; clip: rect(0, 0, 0, 0); height: 1px; margin: -1px; overflow: hidden; padding: 0; position: absolute; white-space: nowrap; width: 1px; } .job-runs-page .clinic-table--stack-wide tbody tr { border-bottom: 1px solid var(--border-color); display: grid; gap: var(--space-xs); padding: var(--space-sm) var(--space-md); } .job-runs-page .clinic-table--stack-wide tbody tr:last-child { border-bottom: 0; } .job-runs-page .clinic-table--stack-wide td, .job-runs-page .clinic-table--stack-wide tbody th { border: 0; padding: 0; width: auto; } .job-runs-page .clinic-table--stack-wide th[scope="row"] { display: block; } .job-runs-page .clinic-table--stack-wide td[data-label]::before { color: var(--muted-text); content: attr(data-label) ": "; content: attr(data-label) ": " / ""; font-weight: 600; } .job-runs-page .clinic-table--stack-wide .btn { min-height: 2.75rem; } } |
| reused, named | .dashboard-card-body, .dashboard-card-footer, .dashboard-card-skeleton (Home, S1); .form-container, .form-actions (the dialogs, M1); .pager-info, .pager-actions, .pager-page-size, .pager-page-size-label, .pager-page-size-select (M2); .list-section-title (N1) | shipped | as shipped, never redeclared |
| reused | .page, .sidebar, .brand, .nav-*, .account-row (the shell); .breadcrumbs; .header-container, .page-subtitle, .actions-cell; .dashboard-grid, .dashboard-span-2, .detail-card, .dashboard-card-title, .dashboard-metric(-value, -label); .people-page, .record-page, .user-page, .user-section; .search-container, .people-filter, .search-grid, .search-field, .filter-actions, .field-help; .table-container, .clinic-table, .clinic-table--stack, .clinic-table--stack-wide, .col-wrap, .col-fit--wide, .person-cell, .nowrap; .pager; .form-grid, .form-group, .form-control, .form-select, .required-mark, .validation-message; .action-result, .alert(-success, -warning, -danger, -row, -row-text); .badge and its tints; .loading-line, .skeleton, .skel-line, .skel-short; .detail-card--empty, .state-panel-title, .state-panel-copy; .detail-list; .code-block; .dialog-card, .dialog-scrim; .muted, .text-sm, .visually-hidden; .icon | shipped | as shipped |
| gone from every board this round draws | .dialog-form, .dialog-actions; .institutions-page on a record; required and maxlength; round 2's gone list (.accounts-breakdown, .list-head, .list-count, .list-window, .action-reason, .job-desc, .job-key, .run-when, .nw, .run-error, .payload-block, .payload-note, .audit-entry-page, .stored-utc, .institution-status, round 1's R9, every .cv-wf) | — | each a shipped class's job, or retired |
| K1–K6 | .cv-chips, .cv-chip(--illus, --new, --proposed, --data); .cv-note; .cv-focus; .cv-scrim, .cv-dialog; .cv-sheet, .cv-cols | canvas only (PART 1) | never shipped |
S7's row headers need no new rule: .jobs-page and .job-runs-page sit on div.people-page, so app.css's existing ".people-page th[scope="row"] .person-cell" rules (app.css:6799 and :6847; bundle.css 6717 and 6765) paint the key and its description, and .clinic-table th gives the header ground. Checked on R3-J01 and R3-R01 by rendering. S10: the build's DataTable draws div.table-container.shadow and, stacked, the table roles; the boards draw neither.
##### Messages by kind, with their words (C5)
| Kind | Words | Status |
| Subtitle | Scheduled jobs: "The eight jobs Wombat runs on a schedule. Run one now, disable or enable one, or read every run in Run history." Run history: "Every run of the eight jobs, newest first, kept for 90 days." (kept, K18: the retention job keeps that promise, and Home's jobs line says when that job is disabled, overdue or failing). The audit log: "Every consequential action at Kgosi Kgari Teaching Hospital, newest first." (an institution's administrator) / "Every consequential action across the institutions, and those with no institution, newest first." (the Administrator). An entry: "‹action› by ‹person›, ‹YYYY-MM-DD hh:mm:ss› SAST · ‹result›". Create institution: "A new institution is active from the moment it is saved, so its people can be invited." Institutions: "The institutions that hold Wombat's people and records." | decided T15, T12 (round 2's NEW) |
| Job descriptions | activity-draft-nudge "Reminds trainees about draft activities older than 14 days."; assessor-pending-nudge "Nudges assessors about activities waiting for their assessment for more than 5 days."; audit-log-retention "Archives audit log entries older than 2 years into the archive table."; entrustment-decision-expiry "Expires entrustment decisions past their expiry date and sends 30-day expiry reminders."; msf-campaign-auto-close "Closes open MSF campaigns whose window has ended and anonymises their respondents."; msf-invitation-expiry-reminder "Reminds MSF respondents who have not responded, once, two days before their last day to respond."; scheduled-job-run-retention "Deletes job run records and mail outcomes older than 90 days." (NEW, Q18, PROPOSED, replacing "Deletes job run records older than 90 days."); weekly-coordinator-digest "Sends weekly digest emails to Coordinators." | decided T15; one this round's |
| Schedule | "Daily at 09:00 SAST", "Hourly, on the hour", "Mondays at 10:00 SAST", each with "cron ‹expr›, UTC" under it; any other cron "cron ‹expr›, UTC" alone (K7) | decided T2; the fallback PROPOSED |
| Health line | Database: "The database answers." / "The database did not answer at ‹hh:mm› SAST, so Wombat can read and save nothing, and /health says Unhealthy." Mail: the § 8 lines. Jobs: "All 8 jobs last succeeded." / "7 jobs last succeeded. weekly-coordinator-digest has not run yet: its first run is 2026-11-16 10:00 SAST." / "entrustment-decision-expiry failed at ‹YYYY-MM-DD hh:mm› SAST: ‹the first line of its error›." / "msf-campaign-auto-close is disabled, so the scheduler does not run it. The other 7 last succeeded." / "audit-log-retention has not run since it was due at ‹YYYY-MM-DD hh:mm› SAST: the scheduler did not start it. The application log says why; Run now runs it." / "Three jobs need you:" with a sentence each / "Not read: the database did not answer." | Q2, Q3 decided; the rest decided T13 |
| Health line, NEW | "‹key›'s schedule cannot be read (cron ‹cron›), so the scheduler does not run it. The application log says why." (badge Failing; K4, words only). The Failing mail line with no refusal clause (K3, § 8). "Nothing sent since ‹YYYY-MM-DD hh:mm› SAST; none waiting; none dropped in the last 24 hours." (Q18, R3-H12) | this round's NEW / PROPOSED; Q18's decided |
| Accounts | "24" / "accounts"; "Every account Wombat holds but an erased one."; "None is locked." / "3 are locked and cannot sign in now."; the footer button "Users" | decided T14 |
| Footer buttons | "Scheduled jobs"; "Its runs" (visually hidden ": entrustment-decision-expiry, failed"); "Users" | decided T13/T14 words; their place PROPOSED (S1) |
| Field help | "* marks a field you must fill in."; "Up to 200 characters."; "Up to 32 characters."; "Optional." | decided T12 |
| Status | "A new invitation can name it." (Active) / "No new invitation can name it. Its people, their records and the invitations already sent are kept." (Inactive) | decided T12 |
| Result | "Kgosi Kgari Teaching Hospital created."; "Kgosi Kgari Teaching Hospital saved. Its contact email is now paediatrics@kgk.wombat.local."; "Demo Institution deactivated. A new invitation can no longer name it."; "Demo Institution reactivated. A new invitation can name it again." | decided T11, replacing "Institution saved." / "Institution deactivated." / "Institution saved and reactivated." |
| Result | "Disabled msf-campaign-auto-close. The scheduler will not run it until you enable it."; "Enabled msf-campaign-auto-close. Its next run is 2026-11-10 22:00 SAST."; "Ran weekly-coordinator-digest: Succeeded at 2026-11-10 21:39 SAST."; "Ran ‹key›: Failed at ‹YYYY-MM-DD hh:mm› SAST: ‹the error as stored›." | decided T5, replacing "Job '…' disabled." / "dispatched." |
| Result / Next run, NEW | "Enabled ‹key›. It runs at the next check, within 30 seconds." and Next run "At the next check, within 30 seconds" (K5, words only), beside the enabled result above | PROPOSED |
| Refusal | The create refusals (built): "Type the institution's name. The record was not saved."; "Type a short code. The record was not saved."; "An institution with the same name or short code already exists."; "Enter a valid email address, for example name@example.org."; and, unreachable by typing only while maxlength stood, "Keep the name to 200 characters. The record was not saved." and "Keep the short code to 32 characters. The record was not saved." (UpdateInstitutionCommandValidator.cs; K14, words only). "Job 'audit-log-retention' is already running, so it was not started again. Wait for that run to finish, then run it again if it is still needed." (built) | built |
| Refusal | "Job 'msf-campaign-auto-close' is disabled, so it was not run. Enable it, then run it." | decided T22 |
| Failure | "msf-campaign-auto-close could not be disabled. Nothing has changed. Try again, or come back in a few minutes."; "Could not deactivate / reactivate the institution. Nothing has changed. Try again, or come back in a few minutes."; "Could not save the institution. Nothing has changed. Try again, or come back in a few minutes." (built) | decided T23 |
| Audit row text, NEW | "The action failed." (a Failed row whose exception is no refusal; K10) | PROPOSED |
| Question | "Deactivate Demo Institution?" "No new invitation can be issued for it until it is reactivated. Its people, their records and the invitations already sent are kept. You can reactivate it on this page." (built); "Reactivate Demo Institution?" "A new invitation can name it again. Its people and their records are as they were." | built; decided T11 |
| Load / error | "Loading the institutions." / "Loading the scheduled jobs." / "Loading the run history." / "Loading this audit entry."; "Could not load the ‹…›. Nothing has changed. Try again, or come back in a few minutes." with Try again | round 2's (the house sentence) |
| Count line | "21 runs." / "1 run."; "166 actions." / "1 action." (K19); "No runs match these filters." / "No actions match these filters." | decided T17; the singular words only |
| Empty | "No runs match" / "The filters are kept above. Clear them to see all 21."; "No job has run yet" / "The scheduler runs each job at its time; Scheduled jobs shows when each runs next."; "No actions match" / "Nothing matches these filters between these times. Widen From and To, or clear the filters." (NEW, K16, replacing "Nothing was recorded between these times. Widen From and To, or clear the filters."); "Clear the filters" | decided T17; K16 PROPOSED |
| Window line | "Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window." / "Read from ‹D−1 hh:mm› SAST to ‹hh:mm› SAST: the last 24 hours, when you opened the page. Turning a page keeps this window." | decided T21 |
| Filter labels | "Job", "Status", "From (a South African day)", "To (a South African day)"; "Category", "Action" with "Part of a name: "Msf" finds the MSF commands.", "Person's user id", "Subject type", "From (SAST)", "To (SAST)" with "Empty: up to when you press Show.", "Result", "Institution" (devadmin); "Show", "Clear filters"; "Per page:" | decided T1, T3 |
| Notice | "Enable it to run it."; "Running…"; "None while disabled"; "Not run yet"; Duration "—" | decided T5, T16 |
| Unavailable | "Institution unavailable" / "This institution does not exist, or you cannot open it." with "Go to Institutions" or "Go to Home"; "Audit entry unavailable" / "This audit entry does not exist, or you cannot open it." with "Go to Audit log" | Q17, T4, T10 |
| Entry | "Stored as ‹YYYY-MM-DD hh:mm:ss.fff› UTC"; "No person: an MSF respondent answers by link, with no account."; "MSF respondent (no account)"; "No signed-in person" (every other row with no user id, a failed sign-in included, K8); "No institution"; "No subject"; "As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]"." | decided T4, T20 |
| Address | "Cut to its first 48 bits when stored." beside a …/48 (round 1's); "Cut to its first 24 bits when stored." beside an IPv4 …/24 (NEW, K9, words only; HttpAuditContextProvider.cs:62-83) | decided; K9 PROPOSED words |
| The hidden colon (N7) | A health line's name reads "Database: OK" through a visually hidden ":" between the name and its badge: the one place a hidden colon stands in for a label | recorded, no change |

######## BOARD R3-0-spec-3.dc.html
Label: R3-0-spec-3 — round 3 Spec (3 of 3)
### R3-0-spec-3 · Flow 18 · Round 3 · Spec (3 of 3)
#### 10 · Widths
Round 2's sweep printed "stacked 592/592" at every width: the table's box against its container, which a width:100% box always matches. Round 3 measures the CONTENT: each table's min-content width (width: min-content, its columns' content) against its .table-container's clientWidth, in Chromium on this canvas's files, on the system's own faces (Source Sans 3 and Fraunces loaded). A stacked table's min-content is its widest stacked value; a stacked row never scrolls.
| List | Sweep: width: layout min-content/container (px) |
| Scheduled jobs, R3-J01 (1150) | 900: stacked 128/592 · 950: stacked 128/642 · 1000: stacked 128/692 · 1024: stacked 128/716 · 1025: stacked 128/717 · 1050: stacked 128/742 · 1100: stacked 128/792 · 1150: stacked 128/842 · 1151: table 536/843 · 1200: table 536/892 · 1280: table 536/972 |
| Scheduled jobs, the crash error, R3-J08 (1150) | 900: stacked 128/592 · 950: stacked 128/642 · 1000: stacked 128/692 · 1024: stacked 128/716 · 1025: stacked 128/717 · 1050: stacked 128/742 · 1100: stacked 128/792 · 1150: stacked 128/842 · 1151: table 548/843 · 1200: table 548/892 · 1280: table 548/972 |
| Run history, R3-R01 (1024) | 900: stacked 125/592 · 950: stacked 125/642 · 1000: stacked 125/692 · 1024: stacked 125/716 · 1025: table 573/717 · 1050: table 573/742 · 1100: table 573/792 · 1150: table 573/842 · 1151: table 573/843 · 1200: table 573/892 · 1280: table 573/972 |
| Run history, the 2,000-character error, R3-R07 (1024) | 900: stacked 143/592 · 950: stacked 143/642 · 1000: stacked 143/692 · 1024: stacked 143/716 · 1025: table 579/717 · 1050: table 579/742 · 1100: table 579/792 · 1150: table 579/842 · 1151: table 579/843 · 1200: table 579/892 · 1280: table 579/972 |
| Audit log, Nolwazi Mbatha, R3-L01 (1150) | 900: stacked 131/592 · 950: stacked 131/642 · 1000: stacked 131/692 · 1024: stacked 131/716 · 1025: stacked 131/717 · 1050: stacked 131/742 · 1100: stacked 131/792 · 1150: stacked 131/842 · 1151: table 600/843 · 1200: table 600/892 · 1280: table 600/972 |
| Audit log, devadmin, R3-L05 (1150) | 900: stacked 131/592 · 950: stacked 131/642 · 1000: stacked 131/692 · 1024: stacked 131/716 · 1025: stacked 131/717 · 1050: stacked 131/742 · 1100: stacked 131/792 · 1150: stacked 131/842 · 1151: table 638/843 · 1200: table 638/892 · 1280: table 638/972 |
With the stack off (a scratch copy, no board), each natural table first overflows its container at a viewport of: Scheduled jobs 843 px (855 with the crash error); Run history 880 px (886 with the long error); the audit log 907 px for Nolwazi Mbatha and 945 px for devadmin. (The ds review measured about 862–885, 899–904, 926 and 967 on a fallback face; Job as a row header and the real faces move them.)
S8, decided. The 1150 px stack is kept for Institutions, Scheduled jobs and the audit log, and the 1024 px stack for Run history, as a readability choice, not as an overflow fix. Above 945 px no natural table here overflows, so from there to 1150 (and from 887 to 1024 for Run history) the stack is chosen for reading, not needed for room; below those widths, down to 901 px, the stack also keeps the audit log (907–945 px) and the job lists in their containers, and from 900 px down the .clinic-table--stack rule takes over. What the 1150 and 1024 starts avoid is the cells' lines just above them: at 1151 px Institutions' Contact email runs over three lines (round 2's reading; Institutions is not redrawn) and a Scheduled jobs description over four beside short cells. Flows 16 and 17 chose the same for their lists.
| List at 1280 | Each column: width, the most lines a value takes |
| Scheduled jobs (R3-J01) | Job 392 px, 3 lines; Schedule 151 px, 4 lines; Last run 146 px, 3 lines; Next run 144 px, 2 lines; Actions 138 px, 2 lines |
| Scheduled jobs, crash error (R3-J08) | Job 329 px, 4 lines; Schedule 138 px, 4 lines; Last run 241 px, 5 lines; Next run 135 px, 2 lines; Actions 129 px, 2 lines |
| Run history (R3-R01) | Job 299 px, 1 line; Started 245 px, 1 line; Duration 92 px, 1 line; Status 133 px, 1 line; Ran by 203 px, 1 line |
| Run history, long error (R3-R07) | Job 145 px, 2 lines; Started 162 px, 2 lines; Duration 92 px, 1 line; Status 440 px, 6 lines; Ran by 133 px, 2 lines |
| Audit log, Nolwazi Mbatha (R3-L01) | When 220 px, 1 line; Person 151 px, 1 line; Category 143 px, 1 line; Action 267 px, 1 line; Result 103 px, 1 line; Detail 88 px, 1 line |
| Audit log, devadmin (R3-L05) | When 198 px, 2 lines; Person 223 px, 3 lines; Category 131 px, 1 line; Action 237 px, 2 lines; Result 94 px, 1 line; Detail 88 px, 1 line |
Read at 1280 with Job a row header (S7): no value runs past two lines beside an empty band but where prose or an error is the row's point. On Scheduled jobs the Job cell (key and description, 3 lines) sits beside the schedule's words and cron (4 lines in 151 px) and the last run's day, hour and badge (3): the row's height is set by the schedule, with no empty band beside it. On R3-J08 the crash error under its badge takes 5 lines beside 4-line cells. On R3-R07 the 2,000-character error takes the Status column's 440 px over its 6 lines while the other cells take 2: the error is the row's reason, drawn whole (T1). The audit rows take 1 line each for Nolwazi Mbatha; for devadmin the person and the institution under the name take 3 ("Kgosi Kgari Teaching Hospital" wraps), the other cells 2 at most.
#### 11 · Badges (BadgeFor, no new tint)
| Words | Class |
| OK, Success, Succeeded, Active | .badge-completed |
| Failing, Failed, Unreachable | .badge-declined |
| Not configured, Disabled, Running | .badge-accepted (Running as built) |
| Overdue | .badge-overdue |
| Inactive | .badge-withdrawn |
| Not read | .badge-draft |
N3: Disabled and Not configured stay .badge-accepted, round 2's badges, decided with T8 and T13; the review's preference for .badge-draft is recorded, not taken.
#### 12 · 44 px at 390, and 768
.people-page (the lists) and .record-page / .user-page (the records) make every button, input and select 44 px; .filter-actions stacks Show and Clear filters full width; StackWide's row buttons are 44 px from 900 px down (and from 1150 / 1024 in the C1 stacks); .dialog-card .form-actions stacks both dialogs' buttons, the action on top, Cancel first in the DOM (app.css:6375; M1); Home's footer buttons are 44 px under 641 px (.dashboard-card-footer .btn, app.css:2916) and the footer wraps (app.css:6925) (S1); a header action takes the row. At 768 every list is stacked (all four stacks are wider than 768) and Home is ONE column: app.css:2288 sets one track and resets the spans at 900 px and below; two tracks from 901 to 1099 px, three from 1100 (S2; round 2's "the 2-column grid with System health spanning both" goes). From 1100 px the health list sits in the two-track span; measured, it is under 30rem (480 px) from 1100 to 1112 px (471.3 to 479.3 px), so its lines take the narrow layout there, and the wide layout from 1113 px (480.0 px) — not 1115 as the message estimated.
#### 13 · DESIGN.md text each change needs
| Section | Change |
| § Status dots | Retired: Home's three dots were its only user; a state is a word on a badge. Its contrast row (DESIGN.md:324) removed with it (S5). |
| § Dashboard page | The Administrator's Home: System health (span 2, three lines in words, fixed order) and Accounts (one figure, the locked line), each card DashboardCard's markup with its links as outline buttons in .dashboard-card-footer (S1). The grid: one column to 900 px, two tracks 901–1099, three from 1100 (S2). |
| § Table system | Institutions, Scheduled jobs and the Audit log join the 1150 px stack; Run history the 1024 px stack — a readability choice, not an overflow fix (S8); a time breaks only between its day and its hour; Job is the row's header on the job lists (S7); the build's DataTable draws .shadow and the stacked roles, the boards do not (S10). |
| § Pager | Run history and the Audit log on PagerControls in its order: info, Previous and Next, Per page (M2); 50 a page; the focus on the list's h2.list-section-title after a turn. |
| § Dialogs | A dialog that does not post is ConfirmDialog's action mode: div.form-container > h2, p, div.form-actions, Cancel autofocused and first in the DOM; form mode is for a dialog holding a real form that posts (M1). |
| § Page-level patterns | A record headed by its name, its Status in a section of its own; an entry a .user-page of h2 sections, its payload region named once by its h2, the section unnamed (S4); a list's count line takes the focus after Show and is not a live region (N2); Unavailable gains Institution and Audit entry; no Back links. |
| § Button system | Deactivate… and Reactivate… each asking (outline, aria-haspopup); Run now aria-disabled with its reason as text; a result above the list. |
| § Form system | The create page's refusals under their fields, focused, described by them first; a starred control aria-required="true", never required (S6). |
#### 14 · Tokens and contrast
Every colour an existing token. Measured: text on each badge tint 11.23–11.89:1; muted text on white 5.09, on --header-bg 4.57, on the success tint 4.52 (0.02 to spare; ContrastTests pins it, N5), warning 4.79, danger 4.75, info 4.77; the badge edges on their own tints (N4): success 5.23, warning and .badge-overdue 5.43, danger 5.56 (beside 5.88, 5.77, 5.95 on white); --input-border on --header-bg 3.42 (3.80 on white); the focus ring 4.86:1 on white and 4.37 on --header-bg; the link 6.70. The status dots' row goes with the dots (S5).
#### 15 · LATER, NOT BUILT
- T274: what a run did, stored with it.
- T157: refusing to start in Production with no mail server.
- T114 items 2–3: bounding a payload's size.
- The Api host's /health, which checks nothing (to be filed). The Api host's mail now reads on Home through the table (K20).
- T401: the College's records to Q5's pattern.
- A by-name Person filter (T3).
#### 16 · Edge cases
| Edge | Board or words |
| Every health line failing | R3-H09 |
| The database down | R3-H08; reachable only in a circuit that outlives the outage by up to a minute (the security stamp revalidated, DependencyInjection.cs:4-6, T279; a fresh request refused before Home draws) (K11) |
| A mail server set and the database down | words only: the mail line Not read, "Not read: the database did not answer." (Q18) |
| A mail server set, nothing sent in the table's 90 days | R3-H12 (Q18) |
| A drop with no refusal | words only: the Failing line without its refusal clause (K3) |
| A restart | words only: the 24 hours and "last sent" survive, read from the table (Q18) |
| A long Running run | words only: neither failing nor overdue on Home; Running on its row (K4) |
| A stored cron that does not parse | words only: Failing, "‹key›'s schedule cannot be read (cron ‹cron›), so the scheduler does not run it. The application log says why." (K4) |
| A job due at once when enabled | words only: Next run "At the next check, within 30 seconds" and its result (K5); R3-J03 is not one |
| An IPv4 address | words only: "Cut to its first 24 bits when stored." (K9) |
| A Failed row whose exception is no refusal | words only: "The action failed." (K10) |
| Deactivate… or Reactivate… pressed with unsaved typing | words only: the typed values stay in the fields, unsaved; only the status changes (K17) |
| No runs | R2-R08 |
| Thousands of runs, paged (about 2,800 held) | R3-R06 |
| A run still Running; a 2,000-character error | R3-R07 |
| A payload of several KB | R3-E06 |
| An actor-less row | R3-L05, L06 (T20, K8) |
| An entry with no subject / with one | R2-E01 / R3-E07 |
| No contact email; a 200-character name | R2-I06 |
| An institution's administrator whose institution is inactive reads the log | words only: unchanged |
| An erased person in old rows | R3-E09 (Q15, M3) |
| Two Administrators running the same job | R3-J06 |
#### 17 · DATA by query
| Query | What it must return that it does not today |
| GetAdministratorDashboardSummaryQuery | Per part (accounts, jobs, mail) read or not read, and the moment the database failed (K11); the accounts and the locked by CountAccountsAsync (K2); the mail's 24 hours, last sent and last drop (with its refusal or none) from EmailOutcome (Q18), configured and the queue's depth from IEmailHealth; each job's key, enabled, last status, time, first error line and IsDue (K4). |
| EmailOutcome (Q18) | The new table and its reads: the outcomes of the last 24 hours, the last Sent, the last Dropped, the window's start (now less 90 days) for "since ‹time›". |
| GetScheduledJobStatusQuery | The last run's error, finish and runner; Next run by the host's rule (K5). |
| RunScheduledJobNowCommand | The run (status, start, finish, error) or a ScheduledJobRefusal (K6). |
| GetScheduledJobRunsQuery | A page and a total; South African days; Ran by through DataRightsPeople.StaffNamesAsync with the erasure records (M3; round 2's S9 "an existing read" moved here). |
| ListAuditEntriesQuery | The resolved window; the Institution filter of three kinds (K15); the institution's name per row (a lookup); the Category label (a map); each Person through StaffNamesAsync (M3); ActorKind (K8). |
| GetAuditEntryByIdQuery | Who through StaffNamesAsync with the erasure records (M3); the subject's institution and speciality by name; ActorKind (K8). |

######## BOARD R3-0-steps.dc.html
Label: R3-0-steps — round 3 Steps
### R3-0-steps · Flow 18 · Round 3 · Steps
Round 2's Steps board restated whole (from project/r2/R2-0-steps.ref.html), with round 3's M4, M5, C3, C4, C8, C10, K16's states.md line, K17's line and A1. A row cites the board this round redrew by its R3 id; a board that stands on "Wombat 18" keeps its R2 id (A1). Runbook quotations keep their own words ("Dr", "Prof").
Counting rule (round 2's, unchanged, said once). A journey counts presses + fields. A press is a click on a button, link, tab or checkbox; a field is a text typed, a date set or a choice made in a select or a radio group, and is not also a press; a menu link is a press, and at 390 opening the menu is one more; a dialog's confirm is a press; a page the app goes to by itself is no press; reading and scrolling are 0.
#### The 23 steps, 1.23 and A.5.5
| Step | Board | Expect | What it will say once built |
| 1.1 | R3-H02 | changes | "System health, which shows the database connection as healthy" → "System health, which reads Database OK, Mail Not configured ('Mail is written to the log, not sent.') and Scheduled jobs OK, naming weekly-coordinator-digest as not run yet"; "Users across institutions, which counts the seeded accounts (8, or 9 where the bootstrap Administrator exists)" → "Accounts, which counts every account but an erased one (8, or 9 …), with 'None is locked.'". The menu, Home lit and the top bar unchanged. The links are buttons in the cards' footers (S1: "Scheduled jobs", "Users"). 1.1's 9 is drawn on R3-H02; 1.11's 11 is words only (C10). |
| 1.6 | R2-I01, R3-I07, R3-I08, R3-I11, R2-I02 | changes | Do unchanged. The row unchanged, "offering Edit only"; the form gains "* marks a field you must fill in." and each refusal focused and described; "headed 'Edit institution', with the browser tab reading 'Edit institution · Wombat'" → "headed 'Kgosi Kgari Teaching Hospital', the tab 'Kgosi Kgari Teaching Hospital · Wombat', the result 'Kgosi Kgari Teaching Hospital created.' focused"; "Active is ticked and Deactivate is offered" → "Status reads Active, 'A new invitation can name it.', with Deactivate…"; "created today" reads 2026-11-10. |
| 1.11 | R3-H02 (words) | changes | "The dashboard's count of registered users is two higher" → "Accounts reads two higher (11)". Words only: no board draws 11 (C10). |
| 3.32 | R3-J04 (words) | unchanged | "The job's last run updates." holds; the result now reads "Ran assessor-pending-nudge: Succeeded at 2026-11-10 19:15 SAST." above the list. Its Actual's "dispatched" (act-3-operations.md:840) is on the search table (C3). |
| 3.55 | R3-L01–L03 | changes | Do "Set From (UTC) … and apply" → "Set From (SAST) to the sitting's start, 18:58, and press Show"; the two filters end "and press Show". Expect: "50 to a page" unchanged; "each with its actor and OK" → "each with the person by name and Success"; "each FAILED" → "each Failed, its refusal under the badge"; 166, 4 and 19 unchanged. |
| 3.56 | R2-E01, R3-L13 | changes | "The Event card shows …" → "What happened shows the SAST time to the millisecond with the stored UTC under it, Command, CreateActivityCommand, Kgosi Kgari Teaching Hospital, and Failed with 'Date observed: The date cannot be after today (2026-11-10).'"; "The Actor card shows Dr Ndlovu's user id, display name and IP address" → "Who shows Sipho Ndlovu by name, the user id and ::/48"; "No Payload card" → "no Payload section"; "'Back to log' returns to the list" → "the trail's Audit log returns to Failures only, page 1, the focus on the Detail left". |
| 3.57 | R3-L05, R3-E02, R3-E03 | changes | Do "From (UTC)" → "From (SAST) 19:00, Show". "whose actor is 'system'" → "whose Person reads 'MSF respondent (no account)' and 'No institution'"; "names no user and no display name, only a truncated address" → "Who reads 'No person: an MSF respondent answers by link, with no account.', User id None, ::/48"; the payloads unchanged in content, one property a line. 174 and the eight submissions unchanged. |
| 4.38 | R3-L09, R3-L10, R2-E04, R2-E05 | changes | "five OK rows by Dr Zulu" → "five Success rows by Thandi Zulu"; "the Event card and the Actor card only" → "What happened, Who and What it was done to only, no Payload"; the revocation's actor "Refilwe Mokoena". |
| 6.38 | — | unchanged | Flow 15's page; no board. |
| A.2.1 | R3-J01 | changes | "its cron schedule (UTC), with its last run, that run's status badge and its next run in the server's local time" → "its schedule in words in SAST with the cron under it, its last run in SAST with its badge under it, and its next run in SAST"; "Each has Disable and Run now" unchanged; "reads '—' for its last run and status" → "reads 'Not run yet'"; the eight keys unchanged; scheduled-job-run-retention reads "Deletes job run records and mail outcomes older than 90 days." (Q18). |
| A.2.2 | R3-J02, R3-J03 | changes | "Disabling says 'Job 'msf-campaign-auto-close' disabled.'" → "'Disabled msf-campaign-auto-close. The scheduler will not run it until you enable it.' above the list, focused"; "'—' for its next run" → "'None while disabled'"; "its Run now is unavailable" gains "with 'Enable it to run it.' beside it"; enabled again: "'Enabled msf-campaign-auto-close. Its next run is 2026-11-10 22:00 SAST.'" (true by the host's rule, K5). |
| A.2.4 | R3-J04 | changes | "'Job 'weekly-coordinator-digest' dispatched.'" → "'Ran weekly-coordinator-digest: Succeeded at 2026-11-10 21:39 SAST.'"; the rest unchanged. Its Actual's "dispatched" (appendix-cross-cutting.md:577) is on the search table (C3). |
| A.2.6 | — | unchanged | Its Expect reads the log. Its Actual's "dispatched" (:607) is on the search table (C3). |
| A.2.8 | — | unchanged | Its Expect reads the log. Its Actual's "dispatched" (:649) is on the search table (C3). |
| A.2.9 | — | unchanged | "Each run reads Succeeded" holds. Its Actual's "dispatched" (:670) is on the search table (C3). |
| A.2.10 | R3-R01, R3-R02, R2-R03, R3-R04 | changes | Do → "On Scheduled jobs (A.2.9's page) press Run history; choose weekly-coordinator-digest in Job and press Show; choose All jobs and Failed and press Show; choose All and set From to today and press Show" (the "digest" sub-step goes). Expect: "each with its key, start, finish, duration, status badge, who triggered it and any error" → "each with its job, its start in SAST, its duration, its status badge (a failed run's error under it) and who ran it"; "'scheduler'" → "'Scheduler'"; "Filter is pressed" → "Show is pressed"; "'No runs found'" → "'No runs match these filters.'"; "a UTC day" → "a South African day"; gains "the pager reads 'Showing 1–21 of 21'; 10 by hand, 11 by the scheduler". |
| A.5.2 | R2-I21 | unchanged | Flow 01's Access denied. |
| A.5.5 | R2-I20 (standing) | changes | Expect "The institution shows Page not found, not 'You cannot open this page'" (appendix-cross-cutting.md:1003) → "The institution shows 'Institution unavailable', 'This institution does not exist, or you cannot open it.', with Go to Home; never 'You cannot open this page'". Its Actual's "as /admin/institutions/999 does" (:1009-1010) holds, the same Unavailable; the user half unchanged (M4, Q17). |
| A.5.13 | — | unchanged | No System item (Q1). |
| A.6.1 | R2-I02, R3-I12 | changes | "creation time" → "the day it was created"; "Saving says 'Institution saved.'" → "'Kgosi Kgari Teaching Hospital saved. Its contact email is now paediatrics@kgk.wombat.local.'"; "offered the Status box and Deactivate" → "Status reads Active, with Deactivate…, in a section of its own; no Active box". |
| A.6.2 | R3-H01 | changes | Rewritten: "Database reads OK; Mail reads Not configured, 'Mail is written to the log, not sent.'; Scheduled jobs reads OK, 'All 8 jobs last succeeded.'; each a word on a badge, no dot, no task id. Accounts reads 24, every account but an erased one, and 'None is locked.'; Scheduled jobs and Users are buttons in the cards' footers." |
| A.6.9 | — | unchanged | Flow 12's page; no board. |
| A.6.10 | — | unchanged | /health. |
| A.7.11 | R2-I03, R3-R05, R3-J01-390, R3-H01-390 | changes | "The jobs table scrolls inside its container, with each Run now and toggle reachable" → "Each job is a stacked block, its state, next run, Run now and Disable in view, each button 44 px; Run history's runs and Institutions' rows stack likewise (KGK's contact hod.paediatrics@kgk.wombat.local); nothing scrolls sideways"; "The run-history filters stack" unchanged. |
| 1.23 (Q17) | R2-I20 | changes | "The Demo Institution's id sends ‹Prof Mbatha› to Page not found, not 'You cannot open this page'" → "… to 'Institution unavailable', 'This institution does not exist, or you cannot open it.', with Go to Home; never 'You cannot open this page'" (act-1-setup.md:709, :722). The runbook's quotation keeps its own words. |
Changed: 14 of the 23 (1.1, 1.6, 1.11, 3.55, 3.56, 3.57, 4.38, A.2.1, A.2.2, A.2.4, A.2.10, A.6.1, A.6.2, A.7.11), and 1.23 and A.5.5. Unchanged: 9 (3.32, 6.38, A.2.6, A.2.8, A.2.9, A.5.2, A.5.13, A.6.9, A.6.10).
K17 (PROPOSED, words only; no step reaches it): Deactivate… or Reactivate… pressed with unsaved typing in the fields: the typed values stay in the fields, unsaved; only the status changes.
Q18 adds no step: the replay has no mail server, so the mail line reads Not configured throughout.
#### states.md rows
| Line | Change |
| 141 | "System health (with the job status), Users across institutions" → the two cards in words, their links in the cards' footers |
| 888 | "An administrator's edit" → the record by its name |
| 889 | "Institution saved." → T11's |
| 898 | "Institution deactivated." → T11's |
| 899 | Page not found → Institution unavailable (Q17) |
| 1086 | "No entries" → "No actions match", its body "Nothing matches these filters between these times. Widen From and To, or clear the filters." (K16) |
| 1091 | "Audit entry not found" → "Audit entry unavailable" |
| 1093 | "Nine jobs" → eight (stale today, N14) |
| 1095 | "Dispatched" → the ran result |
| 1099 | "the full key" → the Job select |
| 1100 | "No runs" → "No runs match these filters." |
Also rows (round 2's M4 c): A.2.1's "—" → "Not run yet"; A.2.10's "finish" (folded into Started and Duration); 3.57's "system" on the rows; 4.38's "OK".
#### Runbook words to search (BRIEF.md § 9 item 7)
| Words | Step (file:line where known) |
| "Apply filters", "apply" | 3.55 |
| "Back to log" | 3.56 |
| "Back to jobs" | A.2.10 (none in its Expect; the page's) |
| "Back to institutions" | A.6.1 (appendix-cross-cutting.md:1236) (C4) |
| "Edit institution" | 1.6 (act-1-setup.md:215, :225); A.6.1 (appendix-cross-cutting.md:1235); 1.23's Note (act-1-setup.md:711) |
| "registered users" | 1.11 (act-1-setup.md:345); A.6.2's Actual (appendix-cross-cutting.md:1251) |
| "dispatched" | A.2.4's Expect (appendix-cross-cutting.md:572) and Actual (:577); A.2.6's Actual (:607); A.2.8's (:649); A.2.9's (:670); 3.32's Actual (act-3-operations.md:840); states.md:1095. The Expects hold (round 2's M4 b); the runbook lane edits the Actuals when it replays (C3; the review's ":577 as A.2.6's" is A.2.4's) |
| "When (UTC)" | 3.56 (act-3-operations.md:1582) |
| "From (UTC)" | 3.55 (:1551, :1562); 3.57 (:1591) |
| "Display name" | 3.56 (:1583); 3.57 (:1600) |
| "Users across institutions" | 1.1 (act-1-setup.md:58); 1.11's Actual (:347); A.6.2 (appendix-cross-cutting.md:1245) |
| "Status box" | A.6.1 |
| "scheduler" lower-case | A.2.10 |
| "No runs found" | A.2.10 |
| "Page not found" | 1.23; A.5.5 |
#### Journeys: presses + fields, 1280 / 390, today's beside
Each journey recounted under the rule above. One recount differs from round 2: A.2.10 at 390 (M5).
| Journey | Round 3 | Today | Recount |
| 1.1 read Home | 0 + 0 / 0 + 0 | 0 + 0 | unchanged |
| A.6.2 read health and accounts | 0 + 0 / 0 + 0 | 0 + 0 (nothing true to read) | unchanged |
| A failing mail line found | 0 + 0 (acting on it is outside Wombat) | never (nothing says it) | unchanged |
| A failed job found and run again (Illustration) | Home's Scheduled jobs (a footer button), Run now: 2 + 0 / 2 + 0 | 2 + 0 / 3 + 0 (the menu), found only by reading a badge | unchanged: the footer button is the same one press |
| 1.6 | Institutions, Create institution, Save (empty), Save, the trail: 5 + 3 (390: 6 + 3, the menu) | 5 + 3 (Back for the trail) | unchanged |
| A.6.1 | Institutions, Edit, Save; the email: 3 + 1 (390: 4 + 1, the menu) | 3 + 1 | unchanged |
| Deactivate and reactivate | Deactivate…, Deactivate, Reactivate…, Reactivate: 4 + 0 | Deactivate, Deactivate, the box, Save: 4 + 0 | unchanged |
| A.2.2 | Disable, Run now (nothing sent), Enable: 3 + 0 | 2 + 0 (Run now not pressable) | unchanged |
| A.2.10 | From Scheduled jobs (A.2.9's page): Run history, Show ×3; Job, Job and Status, Status and From: 4 + 5 / 4 + 5 | Run history, Filter ×4; the key, the key, the key and Status, Status and From: 5 + 6 | RECOUNTED (M5): round 2 wrote "390: 5 + 5"; the journey starts on Scheduled jobs and Run history is a header action there, so no menu is opened at 390: 4 + 5 at both. Today's journey listed as its fields are (C8) |
| 3.55 | Audit log, Show ×3; From, Result, Result and Action: 4 + 4 (390: 5 + 4, the menu) | 4 + 4 (Apply) | unchanged |
| 3.56 and back | Detail, the trail: 2 + 0, the rows kept | 2 + 0, filters lost (3.56-2: 325 rows): 2 + 2 more to find the row | unchanged |
| Curriculum progress | 2 + 0 / 3 + 0 | the same (flow 15's) | unchanged |
| Their own account (A.6.9's page) | Users, Show, the row; "devadmin": 3 + 1 / 4 + 1 | the same | unchanged |

######## BOARD R3-0-check.dc.html
Label: R3-0-check — round 3 Check (1 of 2)
### R3-0-check · Flow 18 · Round 3 · Check (1 of 2)
Checked two ways, and each line says which. Markup: a script over every R3 board's source (the 122 page boards and the sheets). Rendering: Chromium (Playwright) on a harness copy of each board served from this canvas's own project files — the runtime's wrapper dropped (x-dc as display: contents, helmet's style kept), the stylesheets in the board's order. Faces: Source Sans 3 (upright) and Fraunces loaded from ds/wombat/fonts; the Source Sans 3 italic face is declared but no board asks for italic, so the browser did not fetch it; code uses the system's mono stack (no file). A number that was not measured says so.
#### A · The reference files
Arrived and opened: project/r2/ — all 173 files (170 page boards, R2-0-spec, R2-0-spec-2, R2-0-steps), each read as text; project/r2/f18r2.css, read and copied whole into f18r3.css; ds/wombat/tokens.css, ds/wombat/components/bundle.css, ds/wombat/tokens.json and the three faces (SourceSans3VF-Upright, SourceSans3VF-Italic, fraunces-var), read; the 30 captures under project/ref/baseline/, each decoded as a PNG (sizes below). The mark: /_blob/c6a099e64c4d721c4870011edee399ca is in this canvas's asset store (image/svg+xml, 1,328 bytes, sha256 db68eb6f…); every R3 page board references it; its bytes were not fetched (the asset is listed, not served to a file read here). No round 2 mark (/_blob/b3b9…) on any R3 board.
Could not read: none.
| Capture | Size | Viewed for |
| act-1/1.6-1-institutions-one.png | 1280x800 | opened (decoded), not viewed: no board this round draws from it |
| act-1/1.6-2-kgk-created.png | 1280x800 | R3-I11 |
| act-1/1.6-4-create-empty-save.png | 1280x800 | R3-I08 |
| act-3/3.55-2-audit-failures.png | 1280x985 | R3-L02 |
| act-3/3.56-1-audit-entry.png | 1280x800 | opened (decoded), not viewed: no board this round draws from it |
| act-3/3.56-2-back-to-log.png | 1280x4725 | R3-L13 (top 1,100 px viewed at full width) |
| act-3/3.57-1-devadmin-audit.png | 1280x4725 | R3-L05 (top 1,300 px viewed at full width) |
| act-3/3.57-2-msf-submission-detail.png | 1280x800 | R3-E02 |
| act-3/3.57-3-naidoo-transition-detail.png | 1280x800 | R3-E03 |
| act-6/6.38-2-curriculum-progress-page.png | 1280x800 | opened (decoded), not viewed: no board this round draws from it |
| act-6/6.38-3-rebuild-dialog.png | 1280x800 | opened (decoded), not viewed: no board this round draws from it |
| act-6/6.38-4-rebuilt.png | 1280x847 | opened (decoded), not viewed: no board this round draws from it |
| act-A/A.2.1-1-scheduled-jobs.png | 1280x1092 | R3-J01 |
| act-A/A.2.10-1-digest-key.png | 1280x844 | R3-R01 |
| act-A/A.2.10-2-partial-key-nothing.png | 1280x844 | opened (decoded), not viewed: no board this round draws from it |
| act-A/A.2.10-3-failed-none.png | 1280x844 | R3-R02 |
| act-A/A.2.10-4-from-today.png | 1280x2365 | R3-R04 (top 1,300 px viewed at full width) |
| act-A/A.2.2-1-auto-close-disabled.png | 1280x1166 | R3-J02 |
| act-A/A.2.4-1-digest-dispatched.png | 1280x1092 | R3-J04 |
| act-A/A.5.2-1-institutions-access-denied.png | 1280x800 | opened (decoded), not viewed: no board this round draws from it |
| act-A/A.6.1-2-kgk-saved.png | 1280x800 | R3-I12 |
| act-A/A.6.2-1-system-health.png | 1280x800 | R3-H01 |
| act-A/A.6.9-1-own-account.png | 1280x800 | opened (decoded), not viewed: no board this round draws from it |
| act-A/A.7.11-1-home-390.png | 390x844 | R3-H01-390 |
| act-A/A.7.11-3-jobs-390.png | 390x2258 | R3-J01-390 (top 1,200 px) |
| act-A/A.7.11-4-runs-390.png | 390x3499 | R3-R05-390 (top 1,400 px) |
| act-A/A.7.11-5-institutions-390.png | 390x844 | opened (decoded), not viewed: no board this round draws from it |
| states/audit-detail--not-found.png | 1265x812 | opened (decoded), not viewed: no board this round draws from it |
| states/institution-edit--deactivated.png | 1280x800 | R3-I14 (its body only) |
| states/scheduled-jobs-list--loading.png | 1265x812 | opened (decoded), not viewed: no board this round draws from it |
Words a capture shows that a board contradicts: none beyond the changes the boards draw on purpose (decided): "Edit institution", "Institution saved.", "Institution deactivated.", "Back to institutions", the Status checkbox; "Job '…' disabled.", the Enabled column, cron alone; "Job run history", "Back to jobs", "Job key", "Filter", "Triggered by" as a user id, "No runs found"; "Apply filters", "(UTC)", Actor as an address, Subject, "OK"/"FAILED"; "Payload (raw JSON)", "Back to log", Event / Actor cards. Values checked equal: KGK's first contact paeds-admin@kgk.wombat.local (1.6-2 ↔ R3-I11); paediatrics@kgk.wombat.local (A.6.1-2 ↔ R3-I12); the four failures at 19:02:02, 19:01:45, 19:01:41, 19:00:06 SAST (3.55-2, 17:xx UTC ↔ R3-L02); weekly-coordinator-digest's 21:39 (A.2.4-1 ↔ R3-J04); the 21 runs' starts and durations (A.2.10-4 ↔ R3-R04); David Naidoo's payload, activity 13, user 535c41ed-ffb9-449f-bab9-709da9a0ed77, 19:10:20.376 SAST (3.57-3 ↔ R3-E03); the MSF submission's token and answers redacted, 19:20:17.044 SAST (3.57-2 ↔ R3-E02); 3.57-1's first rows (Demo Administrator's Login at 20:24:19 SAST ↔ R3-L05). 3.57-1 and 3.56-2 show some actors as addresses ("mbatha@kgk.wombat.local", "molefe@kgk.wombat.local"); R3-L05 reads each by the live name (Q15, M3), as decided. A.7.11-4 shows a 22:00:13 msf-campaign-auto-close run, the 22nd run R3-R05-390 counts.
#### B · Item by item
| Id | Done | Drawn on | Checked by |
| M1 | done | R3-I13, I15 (+390) | markup: div.form-container > h2, p, div.form-actions; Cancel btn-outline autofocus first; no .dialog-form/.dialog-actions on any board (script). Rendered: § D3. Spec § 12 line replaced. |
| M2 | done | R3-R01, R02, R04, R05-390, R06, R07, R09; R3-L01–L10, L12, L13 (L11 has no pager: nothing matches) | markup: div.pager > div.pager-info, div.pager-actions (Previous, Next, aria-disabled at an end, never disabled), div.pager-page-size (label "Per page:", the select 10/20/50/100, 50 selected); no span (script, every pager). Rendered: § D2. Spec § 13 § Pager. |
| M3 | done | Spec § 7, § 17; R3-E09 (note names the read; Who stays the bracket) | read |
| M4 | done | Steps (A.5.5 row; Changed line; "Page not found" search row) | read |
| M5 | done | Steps (A.2.10 Do starts on Scheduled jobs; 4 + 5 at both) | read |
| K2 | done | Spec § 7, § 17 | read |
| K3 | done | Spec § 7, § 8, § 9, § 16 | read |
| K4 | done | Spec § 7, § 9, § 16; R3-H07 note | read |
| K5 | done | Spec § 7, § 9, § 16; R3-J03 note | read |
| K6 | done | Spec § 7, § 17 (R3-J04, J06, J07, J09, J10 as round 2 drew them) | read |
| K7 | done | Spec § 7, § 9 | read |
| K8 | done | Spec § 7, § 9, § 17 | read |
| K9 | done | Spec § 9, § 16 (boards keep the 48-bit sentence beside ::/48) | read |
| K10 | done | Spec § 7, § 9, § 16 | read |
| K11 | done | Spec § 7, § 16, § 17; R3-H08 note | read |
| K12 | done | Spec § 7 (InstitutionEdit row) | read |
| K13 | done | Spec § 7 (InstitutionCreate row) | read |
| K14 | done | R3-I07–I16, I22 (+390, I12-768): no maxlength (script); Spec § 9 (the two built refusals) | markup |
| K15 | done | Spec § 7, § 17 | read |
| K16 | done | R3-L11 (+390): the body and its PROPOSED chip; Steps (states.md:1086) | markup |
| K17 | done | Spec § 16; Steps | read |
| K18 | done | Spec § 9 | read |
| K19 | done | Spec § 9 | read |
| K20 | done | Spec § 7, § 15 | read |
| C3 | done | Steps (search row "dispatched"; the rows of 3.32, A.2.4, A.2.6, A.2.8, A.2.9) | read |
| C4 | done | Steps (search rows completed; "Back to institutions" added) | read |
| C5 | done | Spec § 9 (messages by kind, quoted, each with its status; NEW beside what it replaces) | read |
| C6 | done | Spec § 4 (the rule, the 28, why loading and load errors carry none); the chips on every R3 board match the list (script) | markup |
| C7 | done | R3-L01–L10, L12, L13 at every width: 323 Detail names, each ending " SAST" (script) | markup |
| C8 | done | Steps (today's A.2.10 listed field by field) | read |
| C9 | done | Spec § 3 (round 2's S8 → § 10) | read |
| C10 | done | Steps (1.1 and 1.11 rows) | read |
| C11 | done | R3-L05, L05-390, L05-768 (note) | markup |
| C12 | done | R3-L01, L04 chips and every annotation and title this round writes ("Prof Mbatha" on no R3 board, script); Spec § 1 names the standing R2-E01, E08, I20, I21 | markup |
| C13 | done | Spec § 8 | read |
| S1 | done | R3-H01–H12 (+390, H01-768) | markup: each card section named by its h2, the title in a span, .dashboard-card-body, the links as a.btn.btn-sm.btn-outline in .dashboard-card-footer at the body's end; no link in a health sentence; H08 (Not read) and H11 (loading) have no footer on the cards that read nothing. Rendered: § D1. |
| S2 | done | Spec § 12, § 13; measured § D1 | rendered |
| S3 | done | f18r3.css R2 (no focus block); Spec § 9 | read |
| S4 | done | R3-E02, E03 (+768), E06, E07, E09 (+390) | markup + the landmark list, § D5 |
| S5 | done | Spec § 7, § 13, § 14 | read |
| S6 | done | R3-I07–I16, I22: aria-required="true" on every starred control, no required="" (script); the label carries a visually hidden " required" | markup |
| S7 | done | R3-J01–J10, R3-R01, R02, R04, R05-390, R06, R07, R09: every tbody row opens with th scope="row" (script) | markup + rendered (§ D4: the .people-page rules paint it; checked on R3-J01 and R3-R01) |
| S8 | done | Spec § 10 | rendered |
| S9 | done | f18r3.css C1 (7 + 7 lists counted); no record board carries .institutions-page (script); Spec § 1, § 9 | markup |
| S10 | done | Spec § 7, § 9, § 13 | read |
| N1 | done | R3-R01, R02, R04, R05-390, R06, R07, R09; R3-L01–L13: h2.list-section-title tabindex="-1" (script) | markup |
| N2 | done | the same boards and R3-L11: the count line p[tabindex="-1"], no role="status" (script) | markup |
| N3 | done | Spec § 11 | read |
| N4 | done | Spec § 14 (figures recomputed from the tokens) | computed |
| N5 | done | Spec § 7, § 14 | read |
| N6 | done | every R3 board: no bracket in an id, for or aria-* value (script). Found and fixed on the way: R3-I10 drew three brackets as input values (round 2's markup); the inputs are now empty and the note names what they hold | markup |
| N7 | done | Spec § 9 | read |
| Q18 | done | R3-H03, H04, H08, H09 (notes), R3-H12 (new, 1280 and 390), R3-J01–J10 (the description and its chip); Spec § 2, § 6, § 7, § 8, § 16, § 17 | markup |
| T1–T24 | done | Spec § 2 (Decided 2026-11-11, boards, changes); the T chips gone from R3-E02, J09, L08 (no PROPOSED chip names a T item, script) | markup |
| A1 | done | Steps | read |
| A2 | done | every twin: the source equals its 1280 board's but for $preview (script; R3-R05-390 has no 1280 board) | markup |
| A3 | done | every R3 board (script): one h1; no inline style; no undefined class (every class found in bundle.css or f18r3.css); no raw colour; ids hold no bracket or space; no aria-describedby=""; every for, aria-labelledby, aria-describedby and aria-controls resolves; every name starts with its visible words | markup |
Not done: none.

######## BOARD R3-0-check-2.dc.html
Label: R3-0-check-2 — round 3 Check (2 of 2)
### R3-0-check-2 · Flow 18 · Round 3 · Check (2 of 2)
#### C · The stylesheet
| Row | State | Count / note |
| f18r3.css (project file at project/f18r3.css) | f18r2.css copied whole; PART 1 (.cv-*, K1–K6) unchanged | written as a file of the project, not an asset |
| f18r3.css R1 | as round 2 wrote it | .health-list, .health-line, .health-line-name, .health-line-text, the 30rem container query |
| f18r3.css R2 | CHANGED (S3) | ".code-block--region { max-height: 28rem; overflow: auto; }" only; the :focus-visible block gone; app.css:512 (bundle.css 430) draws the ring |
| f18r3.css C1 | a STAND-IN (S9), round 2's rules unchanged under a comment that says so | selector lists counted from bundle.css: the 1024 px block (bundle.css 7551–7620 = app.css 7633–7702) holds 7; the 1150 px block (bundle.css 7626–7687 = app.css 7708–7769) holds 7. The build adds ".institutions-page", ".jobs-page", ".audit-page" to each of the 1150 block's 7 and ".job-runs-page" to each of the 1024 block's 7 |
| Heads | checked (script) | every R3 board links ds/wombat/tokens.css, ds/wombat/components/bundle.css, f18r3.css, in that order; no /_blob/ stylesheet; each <style> holds body{margin:0} only; no inline style; no NEW rule but R1 and R2. The canvas changed no rule to make a board render. |
#### D · The sweep (rendered)
##### D1 · The home cards (S1, S2)
| Board @ width | Tracks | Health list against 30rem (480 px) | Footer buttons: height (px) |
| R3-H01 @ 1280 | 3 | 591.3 px: wide | Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H01 @ 1115 | 3 | 481.3 px: wide | Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H01 @ 1114 | 3 | 480.7 px: wide | Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H01 @ 1100 | 3 | 471.3 px: narrow | Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H01 @ 1099 | 2 | 743 px: wide | Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H01 @ 901 | 2 | 545 px: wide | Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H01 @ 900 | 1 | 544 px: wide | Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H01 @ 768 | 1 | 412 px: narrow | Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H01 @ 390 | 1 | 308 px: narrow | Scheduled jobs 44 (one row, unclipped); Users 44 (one row, unclipped) |
| R3-H09 @ 1280 | 3 | 591.3 px: wide | Its runs 28, Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H09 @ 1115 | 3 | 481.3 px: wide | Its runs 28, Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H09 @ 1114 | 3 | 480.7 px: wide | Its runs 28, Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H09 @ 1100 | 3 | 471.3 px: narrow | Its runs 28, Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H09 @ 1099 | 2 | 743 px: wide | Its runs 28, Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H09 @ 901 | 2 | 545 px: wide | Its runs 28, Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H09 @ 900 | 1 | 544 px: wide | Its runs 28, Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H09 @ 768 | 1 | 412 px: narrow | Its runs 28, Scheduled jobs 28 (one row, unclipped); Users 28 (one row, unclipped) |
| R3-H09 @ 390 | 1 | 308 px: narrow | Its runs 44, Scheduled jobs 44 (one row, unclipped); Users 44 (one row, unclipped) |
The health list is under 30rem from 1100 to 1112 px and 480.0 px at 1113 (narrow lines 1100–1112, wide from 1113; measured each pixel 1100–1115: 1100: 471.3, 1101: 472, 1102: 472.7, 1103: 473.3, 1104: 474, 1105: 474.7, 1106: 475.3, 1107: 476, 1108: 476.7, 1109: 477.3, 1110: 478, 1111: 478.7, 1112: 479.3, 1113: 480, 1114: 480.7, 1115: 481.3). At 900 px and below one column; at 768 the list is 412 px (narrow lines); at 390, 308 px. Every footer wraps without clipping a button (none needed a second row); 44 px at 390, 28 px above 640.
##### D2 · The pager (M2)
| Board @ width | DOM order: (left, top within the pager) | Tab order | spans |
| R01 @ 1280 | pager-info (282, 3) → pager-actions (414, 0) → pager-page-size (559, 0) | Previous, Next, Per page select | 0 |
| R01 @ 768 | pager-info (282, 3) → pager-actions (414, 0) → pager-page-size (559, 0) | Previous, Next, Per page select | 0 |
| R01 @ 390 | pager-info (16, 11) → pager-actions (148, 0) → pager-page-size (16, 60) | Previous, Next, Per page select | 0 |
| L01 @ 1280 | pager-info (282, 3) → pager-actions (421, 0) → pager-page-size (566, 0) | Previous, Next, Per page select | 0 |
| L01 @ 768 | pager-info (282, 3) → pager-actions (421, 0) → pager-page-size (566, 0) | Previous, Next, Per page select | 0 |
| L01 @ 390 | pager-info (16, 11) → pager-actions (155, 0) → pager-page-size (16, 60) | Previous, Next, Per page select | 0 |
At 1280 and 768 the three parts sit on one row, info first; at 390 Per page wraps under the info and the buttons. The Tab order is the DOM's: Previous, Next, the Per page select.
##### D3 · The dialogs (M1)
| Board @ width | Card w × h (px) | Buttons |
| I13 @ 1280 | 448 × 280.6 | Cancel (top 356, 36 px, autofocus) then Deactivate (top 356, 36 px); side by side |
| I13 @ 390 | 358 × 368.6 | Cancel (top 436, 44 px, autofocus) then Deactivate (top 380, 44 px); the action above Cancel |
| I15 @ 1280 | 448 × 256.6 | Cancel (top 332, 36 px, autofocus) then Reactivate (top 332, 36 px); side by side |
| I15 @ 390 | 358 × 320.6 | Cancel (top 388, 44 px, autofocus) then Reactivate (top 332, 44 px); the action above Cancel |
Round 2's form-mode markup drew 448 × 270; the ds review measured the action-mode component at 448 × 305. Round 3's action-mode markup measures 448 × 280.6 (Deactivate) and 448 × 256.6 (Reactivate) on these faces: the difference from 305 is the body's length and the face, not the structure. At 390 the action is above Cancel, Cancel first in the DOM, each 44 px.
##### D4 · The lists (S7, S8)
| Board | width: layout min-content/container (spare) | Natural table first overflows at |
| R3-J01 | 900: stacked 128/592 (+464) · 950: stacked 128/642 (+514) · 1000: stacked 128/692 (+564) · 1024: stacked 128/716 (+588) · 1025: stacked 128/717 (+589) · 1050: stacked 128/742 (+614) · 1100: stacked 128/792 (+664) · 1150: stacked 128/842 (+714) · 1151: table 536/843 (+307) · 1200: table 536/892 (+356) · 1280: table 536/972 (+436) | 843 px |
| R3-J08 | 900: stacked 128/592 (+464) · 950: stacked 128/642 (+514) · 1000: stacked 128/692 (+564) · 1024: stacked 128/716 (+588) · 1025: stacked 128/717 (+589) · 1050: stacked 128/742 (+614) · 1100: stacked 128/792 (+664) · 1150: stacked 128/842 (+714) · 1151: table 548/843 (+295) · 1200: table 548/892 (+344) · 1280: table 548/972 (+424) | 855 px |
| R3-R01 | 900: stacked 125/592 (+467) · 950: stacked 125/642 (+517) · 1000: stacked 125/692 (+567) · 1024: stacked 125/716 (+591) · 1025: table 573/717 (+144) · 1050: table 573/742 (+169) · 1100: table 573/792 (+219) · 1150: table 573/842 (+269) · 1151: table 573/843 (+270) · 1200: table 573/892 (+319) · 1280: table 573/972 (+399) | 880 px |
| R3-R07 | 900: stacked 143/592 (+449) · 950: stacked 143/642 (+499) · 1000: stacked 143/692 (+549) · 1024: stacked 143/716 (+573) · 1025: table 579/717 (+138) · 1050: table 579/742 (+163) · 1100: table 579/792 (+213) · 1150: table 579/842 (+263) · 1151: table 579/843 (+264) · 1200: table 579/892 (+313) · 1280: table 579/972 (+393) | 886 px |
| R3-L01 | 900: stacked 131/592 (+461) · 950: stacked 131/642 (+511) · 1000: stacked 131/692 (+561) · 1024: stacked 131/716 (+585) · 1025: stacked 131/717 (+586) · 1050: stacked 131/742 (+611) · 1100: stacked 131/792 (+661) · 1150: stacked 131/842 (+711) · 1151: table 600/843 (+243) · 1200: table 600/892 (+292) · 1280: table 600/972 (+372) | 907 px |
| R3-L05 | 900: stacked 131/592 (+461) · 950: stacked 131/642 (+511) · 1000: stacked 131/692 (+561) · 1024: stacked 131/716 (+585) · 1025: stacked 131/717 (+586) · 1050: stacked 131/742 (+611) · 1100: stacked 131/792 (+661) · 1150: stacked 131/842 (+711) · 1151: table 638/843 (+205) · 1200: table 638/892 (+254) · 1280: table 638/972 (+334) | 945 px |
Each list is stacked at and below its query (1150; 1024 for Run history) and a table above it, with room to spare at every width measured. Every column at 1280, with Job a row header, is on R3-0-spec-3 § 10; no value runs past two lines beside an empty band but the prose and errors named there.
##### D5 · The payload (S4)
R3-E06: the region's clientHeight 480 px, scrollHeight 1250 px at 1280; 480 / 1250 at 390: it scrolls inside itself; the page does not scroll sideways.
| Board | Named landmarks and regions, in order |
| R3-E02 | banner · navigation "Main" · main · navigation "Breadcrumb" · region "What happened" · region "Who" · region "What it was done to" · region "Payload" · complementary "Board notes" |
| R3-E03 | banner · navigation "Main" · main · navigation "Breadcrumb" · region "What happened" · region "Who" · region "What it was done to" · region "Payload" · complementary "Board notes" |
| R3-E06 | banner · navigation "Main" · main · navigation "Breadcrumb" · region "What happened" · region "Who" · region "What it was done to" · region "Payload" · complementary "Board notes" |
| R3-E07 | banner · navigation "Main" · main · navigation "Breadcrumb" · region "What happened" · region "Who" · region "What it was done to" · region "Payload" · complementary "Board notes" |
| R3-E09 | banner · navigation "Main" · main · navigation "Breadcrumb" · region "What happened" · region "Who" · region "What it was done to" · region "Payload" · complementary "Board notes" |
"Payload" is named once on each: the region; the section carries no name. ("Board notes" is the canvas's note, never shipped.)
#### E · The 44 px check (rendered)
At 390, on all 58 boards drawn at 390 (the 57 redrawn twins and R3-H12-390), every visible .btn, .form-control, .form-select, the pager's controls, the breadcrumb's links, every row action and the footers' links: 657 controls measured (46 a breadcrumb link, 436 btn, 112 form-control, 63 form-select). Under 44 px: 0.
#### F · Nothing scrolls sideways (rendered)
366 renders: each of the 122 page boards at 1280, 768 and 390. The document's scrollWidth never exceeds its width (0 exceptions), and no box inside a page scrolls sideways; the payload region scrolls vertically only.
#### G · The counts
|  | This message | Drawn |
| Redrawn screens | 120 (56 at 1280, their 56 twins at 390, R2-R05's at 390 alone, 7 at 768) | 120: 56 + 57 + 7 |
| New screens | 2 (R3-H12 at 1280 and 390) | 2 |
| Check, Spec and Steps boards | about 6 | 6: R3-0-check, R3-0-check-2, R3-0-spec, R3-0-spec-2, R3-0-spec-3, R3-0-steps |
| Total on this canvas | about 128 | 128 |
| Standing on "Wombat 18" | 24 boards, 50 screens | 25 ids by the message's own list (I01, I02, I03, I04, I05, I06, I17, I18, I19, I20, I21; J11, J12; R03, R08, R10, R11; L14, L15; E01, E04, E05, E08, E10, E11), 50 screens: the message counts one id short |
#### H · Check before you stop
| Check | Result | By |
| The check board first, saying which reference files opened | yes: R3-0-check is first on the page; § A | — |
| Heads link tokens.css, bundle.css, f18r3.css in that order | all 128 | markup |
| One h1 a board; no inline style; no undefined class; no raw colour | all 128 | markup |
| No .dialog-form or .dialog-actions; the dialogs in action mode, Cancel autofocused | yes | markup, rendered |
| Every pager info, actions, page size, no span | yes | markup, rendered |
| No required="" and no maxlength; aria-required="true" on every starred control | yes | markup |
| .institutions-page on no record board this round draws | yes | markup |
| Job a th scope="row" on every jobs and runs table | yes | markup |
| h2.list-section-title on every list; no role="status" on a count line | yes | markup |
| Home's links in .dashboard-card-footer as outline buttons; no link in a health sentence; every card with .dashboard-card-body and its title in a span | yes | markup |
| The Payload region named once | yes | markup, landmark list |
| Every Detail name ending in "SAST" | 323 of 323 | markup |
| R3-H12 drawn; scheduled-job-run-retention's description Q18's | yes | markup |
| No PROPOSED chip for a T item; the Illustration chip by C6's rule | yes (28 on the list; 25 R3 ids carry it, the 3 standing ones on "Wombat 18") | markup |
| "Nolwazi Mbatha", never "Prof Mbatha", in every annotation and title this round writes | yes (runbook quotations on the Steps keep theirs) | markup |
| No aria-describedby=""; no bracket as a control's value; no id holding a bracket or a space | yes (R3-I10 fixed, N6) | markup |
| Every 390 control 44 px | 657 measured, 0 under | rendered |
| Nothing scrolls sideways at 1280, 768 or 390 but the payload's region | 366 renders, 0 exceptions | rendered |
| No gendered pronoun on any board, annotations included | none found | markup |
| Every bracket ‹…› still a bracket | yes: none filled | markup |
| Every 390 and 768 twin with the same words as its 1280 board | yes (0 mismatches) | markup |
| f18r3.css saved as a project file | yes | — |
| The mark from /_blob/c6a099e64c4d721c4870011edee399ca | every page board | markup |

######## BOARD R3-H01.dc.html
Label: Home · Wombat
> R3-H01
> A.6.2 as replayed
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
A.6.2: 25 accounts less Sipho Ndlovu's erased one = 24; none locked (Q4). No dot, no task id (T327).
Order fixed: Database, Mail, Scheduled jobs (T8). At 390 System health comes first, one column; under a 30rem card the name and badge sit over the sentence (R1).
At 768 Home is one column (app.css:2288 sets one track and resets the spans at 900px and below); two tracks from 901 to 1099px, three from 1100; from 1100 to 1114px the health list is under 30rem, so its lines take the narrow layout until 1115px (S2).

######## BOARD R3-H01-768.dc.html
Label: Home · Wombat
> R3-H01
> A.6.2 as replayed
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
A.6.2: 25 accounts less Sipho Ndlovu's erased one = 24; none locked (Q4). No dot, no task id (T327).
Order fixed: Database, Mail, Scheduled jobs (T8). At 390 System health comes first, one column; under a 30rem card the name and badge sit over the sentence (R1).
At 768 Home is one column (app.css:2288 sets one track and resets the spans at 900px and below); two tracks from 901 to 1099px, three from 1100; from 1100 to 1114px the health list is under 30rem, so its lines take the narrow layout until 1115px (S2).

######## BOARD R3-H01-390.dc.html
Label: Home · Wombat
> R3-H01
> A.6.2 as replayed
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
A.6.2: 25 accounts less Sipho Ndlovu's erased one = 24; none locked (Q4). No dot, no task id (T327).
Order fixed: Database, Mail, Scheduled jobs (T8). At 390 System health comes first, one column; under a 30rem card the name and badge sit over the sentence (R1).
At 768 Home is one column (app.css:2288 sets one track and resets the spans at 900px and below); two tracks from 901 to 1099px, three from 1100; from 1100 to 1114px the health list is under 30rem, so its lines take the narrow layout until 1115px (S2).

######## BOARD R3-H02.dc.html
Label: Home · Wombat
> R3-H02
> 1.1, the first sign-in
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:OK
7 jobs last succeeded. weekly-coordinator-digest has not run yet: its first run is 2026-11-16 10:00 SAST.
[link] Scheduled jobs
#### Accounts
9accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
1.1: 9 accounts, the bootstrap Administrator among them (the brief's § 2; the Expect's "8, or 9"). At 1.11 the card reads 11 (words only, on the Steps).
A job never run is named and is not a problem: the badge stays OK (T13).

######## BOARD R3-H02-390.dc.html
Label: Home · Wombat
> R3-H02
> 1.1, the first sign-in
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:OK
7 jobs last succeeded. weekly-coordinator-digest has not run yet: its first run is 2026-11-16 10:00 SAST.
[link] Scheduled jobs
#### Accounts
9accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
1.1: 9 accounts, the bootstrap Administrator among them (the brief's § 2; the Expect's "8, or 9"). At 1.11 the card reads 11 (words only, on the Steps).
A job never run is named and is not a problem: the badge stays OK (T13).

######## BOARD R3-H03.dc.html
Label: Home · Wombat
> R3-H03
> Every line healthy, a mail server set
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:OK
Mail is sent. Last sent ‹YYYY-MM-DD hh:mm› SAST; none waiting; none dropped in the last 24 hours.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
No mail server is set in the replay, so this state is never reached there.
Q18: the line is read from the mail outcomes table, so a restart keeps "Last sent" and the 24 hours; "none waiting" is the queue's depth, read in-process from the host that draws Home.

######## BOARD R3-H03-390.dc.html
Label: Home · Wombat
> R3-H03
> Every line healthy, a mail server set
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:OK
Mail is sent. Last sent ‹YYYY-MM-DD hh:mm› SAST; none waiting; none dropped in the last 24 hours.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
No mail server is set in the replay, so this state is never reached there.
Q18: the line is read from the mail outcomes table, so a restart keeps "Last sent" and the 24 hours; "none waiting" is the queue's depth, read in-process from the host that draws Home.

######## BOARD R3-H04.dc.html
Label: Home · Wombat
> R3-H04
> A mail dropped in the last 24 hours
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Failing
‹n› mails were dropped in the last 24 hours, the last at ‹YYYY-MM-DD hh:mm› SAST: ‹the mail server's refusal›. The application log names each one.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
Q13: a drop in 24 hours is Failing; no mail sent is not a state. The refusal as stored, redacted (EmailLog.Redact), read from the mail outcomes table (Q18). A drop with no refusal (a mail abandoned as the host stops, or one whose last try failed in the backoff wait) reads without the refusal clause (K3, words only on the Spec).

######## BOARD R3-H04-390.dc.html
Label: Home · Wombat
> R3-H04
> A mail dropped in the last 24 hours
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Failing
‹n› mails were dropped in the last 24 hours, the last at ‹YYYY-MM-DD hh:mm› SAST: ‹the mail server's refusal›. The application log names each one.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
Q13: a drop in 24 hours is Failing; no mail sent is not a state. The refusal as stored, redacted (EmailLog.Redact), read from the mail outcomes table (Q18). A drop with no refusal (a mail abandoned as the host stops, or one whose last try failed in the backoff wait) reads without the refusal clause (K3, words only on the Spec).

######## BOARD R3-H05.dc.html
Label: Home · Wombat
> R3-H05
> A job failed
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:Failing
entrustment-decision-expiry failed at ‹YYYY-MM-DD hh:mm› SAST: ‹the first line of its error›.
[link] Its runs: entrustment-decision-expiry, failed
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
Its runs opens Run history at ?job=entrustment-decision-expiry&status=Failed (T18, R3-R09). Scheduled jobs, then Run now: 2 presses at 1280 and 390.

######## BOARD R3-H05-390.dc.html
Label: Home · Wombat
> R3-H05
> A job failed
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:Failing
entrustment-decision-expiry failed at ‹YYYY-MM-DD hh:mm› SAST: ‹the first line of its error›.
[link] Its runs: entrustment-decision-expiry, failed
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
Its runs opens Run history at ?job=entrustment-decision-expiry&status=Failed (T18, R3-R09). Scheduled jobs, then Run now: 2 presses at 1280 and 390.

######## BOARD R3-H06.dc.html
Label: Home · Wombat
> R3-H06
> A job disabled
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:Disabled
msf-campaign-auto-close is disabled, so the scheduler does not run it. The other 7 last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
A.2.2's state; Home is not opened while the job is disabled in the replay.

######## BOARD R3-H06-390.dc.html
Label: Home · Wombat
> R3-H06
> A job disabled
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:Disabled
msf-campaign-auto-close is disabled, so the scheduler does not run it. The other 7 last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
A.2.2's state; Home is not opened while the job is disabled in the replay.

######## BOARD R3-H07.dc.html
Label: Home · Wombat
> R3-H07
> A job overdue
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:Overdue
audit-log-retention has not run since it was due at ‹YYYY-MM-DD hh:mm› SAST: the scheduler did not start it. The application log says why; Run now runs it.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
Overdue (Q3, S5): an enabled job over an hour past its last scheduled time while Home is read; the host catches missed runs up every 30 s, so the scheduler did not start it. .badge-overdue (N5).
"Overdue" is the host's own rule (IsDue, ScheduledJobHost.cs:110-119), one shared function the host and the dashboard query both call, never re-derived from the cron. A job whose last run is Running is neither failing nor overdue: it holds its lock (K4).

######## BOARD R3-H07-390.dc.html
Label: Home · Wombat
> R3-H07
> A job overdue
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:Overdue
audit-log-retention has not run since it was due at ‹YYYY-MM-DD hh:mm› SAST: the scheduler did not start it. The application log says why; Run now runs it.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
Overdue (Q3, S5): an enabled job over an hour past its last scheduled time while Home is read; the host catches missed runs up every 30 s, so the scheduler did not start it. .badge-overdue (N5).
"Overdue" is the host's own rule (IsDue, ScheduledJobHost.cs:110-119), one shared function the host and the dashboard query both call, never re-derived from the cron. A job whose last run is Running is neither failing nor overdue: it holds its lock (K4).

######## BOARD R3-H08.dc.html
Label: Home · Wombat
> R3-H08
> The database down
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:Unreachable
The database did not answer at ‹hh:mm› SAST, so Wombat can read and save nothing, and /health says Unhealthy.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:Not read
Not read: the database did not answer.
#### Accounts
Not read: the database did not answer.
[aria-label="Board notes"]
Round 2's S4 as amended by Q18, and T9: the summary answers what it can, each part (accounts, jobs, the mail table) read and caught on its own (K11). Mail reads Not configured here because dev has no mail server, which is configuration; with a mail server set and the database down, the mail line reads Not read, "Not read: the database did not answer." (Q18).
Reachable only in a circuit that outlives the outage by up to a minute: the security stamp is revalidated against the database (DependencyInjection.cs:4-6, T279) and a fresh request is refused before Home draws (K11). No footer on a card that reads Not read. DATA: the per-part answer.

######## BOARD R3-H08-390.dc.html
Label: Home · Wombat
> R3-H08
> The database down
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:Unreachable
The database did not answer at ‹hh:mm› SAST, so Wombat can read and save nothing, and /health says Unhealthy.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:Not read
Not read: the database did not answer.
#### Accounts
Not read: the database did not answer.
[aria-label="Board notes"]
Round 2's S4 as amended by Q18, and T9: the summary answers what it can, each part (accounts, jobs, the mail table) read and caught on its own (K11). Mail reads Not configured here because dev has no mail server, which is configuration; with a mail server set and the database down, the mail line reads Not read, "Not read: the database did not answer." (Q18).
Reachable only in a circuit that outlives the outage by up to a minute: the security stamp is revalidated against the database (DependencyInjection.cs:4-6, T279) and a fresh request is refused before Home draws (K11). No footer on a card that reads Not read. DATA: the per-part answer.

######## BOARD R3-H09.dc.html
Label: Home · Wombat
> R3-H09
> Every line failing
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Failing
‹n› mails were dropped in the last 24 hours, the last at ‹YYYY-MM-DD hh:mm› SAST: ‹the mail server's refusal›. The application log names each one.
- Scheduled jobs:Failing
Three jobs need you:
- entrustment-decision-expiry failed at ‹YYYY-MM-DD hh:mm› SAST: ‹the first line of its error›.
- audit-log-retention has not run since it was due at ‹YYYY-MM-DD hh:mm› SAST: the scheduler did not start it. The application log says why; Run now runs it.
- msf-campaign-auto-close is disabled, so the scheduler does not run it.
[link] Its runs: entrustment-decision-expiry, failed
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
The badge takes the worst word (Failing, then Overdue, then Disabled); each job its own sentence under "Three jobs need you:".
The mail refusal as stored, redacted (EmailLog.Redact), read from the mail outcomes table (Q18).

######## BOARD R3-H09-390.dc.html
Label: Home · Wombat
> R3-H09
> Every line failing
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Failing
‹n› mails were dropped in the last 24 hours, the last at ‹YYYY-MM-DD hh:mm› SAST: ‹the mail server's refusal›. The application log names each one.
- Scheduled jobs:Failing
Three jobs need you:
- entrustment-decision-expiry failed at ‹YYYY-MM-DD hh:mm› SAST: ‹the first line of its error›.
- audit-log-retention has not run since it was due at ‹YYYY-MM-DD hh:mm› SAST: the scheduler did not start it. The application log says why; Run now runs it.
- msf-campaign-auto-close is disabled, so the scheduler does not run it.
[link] Its runs: entrustment-decision-expiry, failed
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
The badge takes the worst word (Failing, then Overdue, then Disabled); each job its own sentence under "Three jobs need you:".
The mail refusal as stored, redacted (EmailLog.Redact), read from the mail outcomes table (Q18).

######## BOARD R3-H10.dc.html
Label: Home · Wombat
> R3-H10
> Three accounts locked
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
3 are locked and cannot sign in now.
[link] Users
[aria-label="Board notes"]
Locked: a lockout still running, an administrator's or a sign-in lockout; an erased account is neither counted nor locked (T14).

######## BOARD R3-H10-390.dc.html
Label: Home · Wombat
> R3-H10
> Three accounts locked
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:Not configured
Mail is written to the log, not sent.
To send mail, set Email__SmtpHost on the server and restart Wombat.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
3 are locked and cannot sign in now.
[link] Users
[aria-label="Board notes"]
Locked: a lockout still running, an administrator's or a sign-in lockout; an erased account is neither counted nor locked (T14).

######## BOARD R3-H11.dc.html
Label: Home · Wombat
> R3-H11
> Home loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
#### Accounts
[aria-label="Board notes"]
DashboardFrame's loading as built: the header and the two cards' titles at once, each card its h2 and its body holding .dashboard-card-skeleton (aria-hidden), no footer until the read is in (S1).

######## BOARD R3-H11-390.dc.html
Label: Home · Wombat
> R3-H11
> Home loading
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
#### Accounts
[aria-label="Board notes"]
DashboardFrame's loading as built: the header and the two cards' titles at once, each card its h2 and its body holding .dashboard-card-skeleton (aria-hidden), no footer until the read is in (S1).

######## BOARD R3-H12.dc.html
Label: Home · Wombat
> R3-H12
> A mail server set, nothing sent in what the table holds
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
> PROPOSED (THIS ASK): "since ‹time›" is the start of the table’s 90 days (Q18)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:OK
Nothing sent since ‹YYYY-MM-DD hh:mm› SAST; none waiting; none dropped in the last 24 hours.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
No mail sent is not a state (Q13), so the badge is OK. The mail line is read from the mail outcomes table (Q18): nothing sent in the 90 days it holds, so ‹time› reads 90 days before the moment Home is read. No mail server is set in the replay, so this state is never reached there.

######## BOARD R3-H12-390.dc.html
Label: Home · Wombat
> R3-H12
> A mail server set, nothing sent in what the table holds
> Illustration
> PROPOSED (THIS ASK): the links in the cards’ footers (S1)
> PROPOSED (THIS ASK): "since ‹time›" is the start of the table’s 90 days (Q18)
(checkbox) [aria-label="Menu"]
[link] Wombat
Acting asAdministrator
MenuClose
[aria-label="Main"]
Acting asAdministrator
-
[link] [aria-current="page"] Home
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
### [tabindex="-1"] Home
Administrator · Semester 2, 2026
#### System health
- Database:OK
The database answers.
- Mail:OK
Nothing sent since ‹YYYY-MM-DD hh:mm› SAST; none waiting; none dropped in the last 24 hours.
- Scheduled jobs:OK
All 8 jobs last succeeded.
[link] Scheduled jobs
#### Accounts
24accounts
Every account Wombat holds but an erased one.
None is locked.
[link] Users
[aria-label="Board notes"]
No mail sent is not a state (Q13), so the badge is OK. The mail line is read from the mail outcomes table (Q18): nothing sent in the 90 days it holds, so ‹time› reads 90 days before the moment Home is read. No mail server is set in the replay, so this state is never reached there.

######## BOARD R3-I07.dc.html
Label: Create institution · Wombat
> R3-I07
> 1.6: Create institution, empty
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
- [aria-current="page"] Create institution
### [tabindex="-1"] Create institution
A new institution is active from the moment it is saved, so its people can be invited.
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[aria-required="true"]
Up to 200 characters.
Short code * required
[aria-required="true"]
Up to 32 characters.
Contact email
Optional.
[link] Cancel
[button] Save
[aria-label="Board notes"]
Cancel goes to the list. The field help is decided (T12, 2026-11-11).

######## BOARD R3-I07-390.dc.html
Label: Create institution · Wombat
> R3-I07
> 1.6: Create institution, empty
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
- [aria-current="page"] Create institution
### [tabindex="-1"] Create institution
A new institution is active from the moment it is saved, so its people can be invited.
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[aria-required="true"]
Up to 200 characters.
Short code * required
[aria-required="true"]
Up to 32 characters.
Contact email
Optional.
[link] Cancel
[button] Save
[aria-label="Board notes"]
Cancel goes to the list. The field help is decided (T12, 2026-11-11).

######## BOARD R3-I08.dc.html
Label: Create institution · Wombat
> R3-I08
> 1.6: saved empty
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
- [aria-current="page"] Create institution
### [tabindex="-1"] Create institution
A new institution is active from the moment it is saved, so its people can be invited.
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[aria-required="true"]
Type the institution's name. The record was not saved.
Up to 200 characters.
Short code * required
[aria-required="true"]
Type a short code. The record was not saved.
Up to 32 characters.
Contact email
Optional.
[link] Cancel
[button] Save
[aria-label="Board notes"]
Each refusal under its field, first in its aria-describedby; Name, the first refused field, takes the focus (T378). At 390 the form stacks; Save on top, Cancel first in the DOM (column-reverse, 44 px).

######## BOARD R3-I08-390.dc.html
Label: Create institution · Wombat
> R3-I08
> 1.6: saved empty
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
- [aria-current="page"] Create institution
### [tabindex="-1"] Create institution
A new institution is active from the moment it is saved, so its people can be invited.
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[aria-required="true"]
Type the institution's name. The record was not saved.
Up to 200 characters.
Short code * required
[aria-required="true"]
Type a short code. The record was not saved.
Up to 32 characters.
Contact email
Optional.
[link] Cancel
[button] Save
[aria-label="Board notes"]
Each refusal under its field, first in its aria-describedby; Name, the first refused field, takes the focus (T378). At 390 the form stacks; Save on top, Cancel first in the DOM (column-reverse, 44 px).

######## BOARD R3-I09.dc.html
Label: Create institution · Wombat
> R3-I09
> A duplicate name or short code
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
- [aria-current="page"] Create institution
### [tabindex="-1"] Create institution
A new institution is active from the moment it is saved, so its people can be invited.
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[value="Kgosi Kgari Teaching Hospital"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="KGK"] [aria-required="true"]
An institution with the same name or short code already exists.
Up to 32 characters.
Contact email
Optional.
[link] Cancel
[button] Save
[aria-label="Board notes"]
The refusal names no institution (flow 12's I08), under Short code.

######## BOARD R3-I09-390.dc.html
Label: Create institution · Wombat
> R3-I09
> A duplicate name or short code
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
- [aria-current="page"] Create institution
### [tabindex="-1"] Create institution
A new institution is active from the moment it is saved, so its people can be invited.
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[value="Kgosi Kgari Teaching Hospital"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="KGK"] [aria-required="true"]
An institution with the same name or short code already exists.
Up to 32 characters.
Contact email
Optional.
[link] Cancel
[button] Save
[aria-label="Board notes"]
The refusal names no institution (flow 12's I08), under Short code.

######## BOARD R3-I10.dc.html
Label: Create institution · Wombat
> R3-I10
> A malformed address
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
- [aria-current="page"] Create institution
### [tabindex="-1"] Create institution
A new institution is active from the moment it is saved, so its people can be invited.
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[aria-required="true"]
Up to 200 characters.
Short code * required
[aria-required="true"]
Up to 32 characters.
Contact email
Enter a valid email address, for example name@example.org.
Optional.
[link] Cancel
[button] Save
[aria-label="Board notes"]
Flow 12's I04 words, under Contact email.
The fields hold what was typed: ‹a new name›, ‹CODE› and ‹an address with no domain›. A bracket is never a control's value, so the board draws them empty and names them here (A3).

######## BOARD R3-I10-390.dc.html
Label: Create institution · Wombat
> R3-I10
> A malformed address
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
- [aria-current="page"] Create institution
### [tabindex="-1"] Create institution
A new institution is active from the moment it is saved, so its people can be invited.
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[aria-required="true"]
Up to 200 characters.
Short code * required
[aria-required="true"]
Up to 32 characters.
Contact email
Enter a valid email address, for example name@example.org.
Optional.
[link] Cancel
[button] Save
[aria-label="Board notes"]
Flow 12's I04 words, under Contact email.
The fields hold what was typed: ‹a new name›, ‹CODE› and ‹an address with no domain›. A bracket is never a control's value, so the board draws them empty and names them here (A3).

######## BOARD R3-I11.dc.html
Label: Kgosi Kgari Teaching Hospital · Wombat
> R3-I11
> 1.6 saved: KGK's record
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
- [aria-current="page"] Kgosi Kgari Teaching Hospital
### [tabindex="-1"] Kgosi Kgari Teaching Hospital
KGK · Active
[tabindex="-1"]
[role="status"] Kgosi Kgari Teaching Hospital created.
* marks a field you must fill in.
Name * required
[value="Kgosi Kgari Teaching Hospital"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="KGK"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="paeds-admin@kgk.wombat.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
Save on the create page lands here with the one-shot result (CatalogueLinks' token, flow 15). h1 and tab the name; no Created on the record (T24).

######## BOARD R3-I11-390.dc.html
Label: Kgosi Kgari Teaching Hospital · Wombat
> R3-I11
> 1.6 saved: KGK's record
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
- [aria-current="page"] Kgosi Kgari Teaching Hospital
### [tabindex="-1"] Kgosi Kgari Teaching Hospital
KGK · Active
[tabindex="-1"]
[role="status"] Kgosi Kgari Teaching Hospital created.
* marks a field you must fill in.
Name * required
[value="Kgosi Kgari Teaching Hospital"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="KGK"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="paeds-admin@kgk.wombat.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
Save on the create page lands here with the one-shot result (CatalogueLinks' token, flow 15). h1 and tab the name; no Created on the record (T24).

######## BOARD R3-I12.dc.html
Label: Kgosi Kgari Teaching Hospital · Wombat
> R3-I12
> A.6.1 saved
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
- [aria-current="page"] Kgosi Kgari Teaching Hospital
### [tabindex="-1"] Kgosi Kgari Teaching Hospital
KGK · Active
[tabindex="-1"]
[role="status"] Kgosi Kgari Teaching Hospital saved. Its contact email is now paediatrics@kgk.wombat.local.
* marks a field you must fill in.
Name * required
[value="Kgosi Kgari Teaching Hospital"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="KGK"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="paediatrics@kgk.wombat.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
Flow 12's result words for an institution's administrator, now the Administrator's too (T11).

######## BOARD R3-I12-768.dc.html
Label: Kgosi Kgari Teaching Hospital · Wombat
> R3-I12
> A.6.1 saved
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
- [aria-current="page"] Kgosi Kgari Teaching Hospital
### [tabindex="-1"] Kgosi Kgari Teaching Hospital
KGK · Active
[tabindex="-1"]
[role="status"] Kgosi Kgari Teaching Hospital saved. Its contact email is now paediatrics@kgk.wombat.local.
* marks a field you must fill in.
Name * required
[value="Kgosi Kgari Teaching Hospital"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="KGK"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="paediatrics@kgk.wombat.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
Flow 12's result words for an institution's administrator, now the Administrator's too (T11).

######## BOARD R3-I12-390.dc.html
Label: Kgosi Kgari Teaching Hospital · Wombat
> R3-I12
> A.6.1 saved
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
- [aria-current="page"] Kgosi Kgari Teaching Hospital
### [tabindex="-1"] Kgosi Kgari Teaching Hospital
KGK · Active
[tabindex="-1"]
[role="status"] Kgosi Kgari Teaching Hospital saved. Its contact email is now paediatrics@kgk.wombat.local.
* marks a field you must fill in.
Name * required
[value="Kgosi Kgari Teaching Hospital"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="KGK"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="paediatrics@kgk.wombat.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
Flow 12's result words for an institution's administrator, now the Administrator's too (T11).

######## BOARD R3-I13.dc.html
Label: Demo Institution · Wombat
> R3-I13
> Deactivate… asked
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
- [aria-current="page"] Demo Institution
### [tabindex="-1"] Demo Institution
DEMO · Active
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[value="Demo Institution"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="DEMO"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="admin@demo.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
Opens on Cancel; Cancel and Escape return to Deactivate…; Deactivate moves the focus to the result (R3-I14). At 390 the buttons take the row, the action on top.
#### Deactivate Demo Institution?
No new invitation can be issued for it until it is reactivated. Its people, their records and the invitations already sent are kept. You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R3-I13-390.dc.html
Label: Demo Institution · Wombat
> R3-I13
> Deactivate… asked
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
- [aria-current="page"] Demo Institution
### [tabindex="-1"] Demo Institution
DEMO · Active
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[value="Demo Institution"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="DEMO"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="admin@demo.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
Opens on Cancel; Cancel and Escape return to Deactivate…; Deactivate moves the focus to the result (R3-I14). At 390 the buttons take the row, the action on top.
#### Deactivate Demo Institution?
No new invitation can be issued for it until it is reactivated. Its people, their records and the invitations already sent are kept. You can reactivate it on this page.
[button] Cancel
[button] Deactivate

######## BOARD R3-I14.dc.html
Label: Demo Institution · Wombat
> R3-I14
> Deactivated
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
- [aria-current="page"] Demo Institution
### [tabindex="-1"] Demo Institution
DEMO · Inactive
[tabindex="-1"]
[role="status"] Demo Institution deactivated. A new invitation can no longer name it.
* marks a field you must fill in.
Name * required
[value="Demo Institution"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="DEMO"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="admin@demo.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Inactive No new invitation can name it. Its people, their records and the invitations already sent are kept.
[button] [aria-haspopup="dialog"] Reactivate…
[aria-label="Board notes"]
Status reads Inactive and offers Reactivate…, never Deactivate….

######## BOARD R3-I14-390.dc.html
Label: Demo Institution · Wombat
> R3-I14
> Deactivated
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
- [aria-current="page"] Demo Institution
### [tabindex="-1"] Demo Institution
DEMO · Inactive
[tabindex="-1"]
[role="status"] Demo Institution deactivated. A new invitation can no longer name it.
* marks a field you must fill in.
Name * required
[value="Demo Institution"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="DEMO"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="admin@demo.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Inactive No new invitation can name it. Its people, their records and the invitations already sent are kept.
[button] [aria-haspopup="dialog"] Reactivate…
[aria-label="Board notes"]
Status reads Inactive and offers Reactivate…, never Deactivate….

######## BOARD R3-I15.dc.html
Label: Demo Institution · Wombat
> R3-I15
> Reactivate… asked
> NEW: the dialog's words (T11)
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
- [aria-current="page"] Demo Institution
### [tabindex="-1"] Demo Institution
DEMO · Inactive
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[value="Demo Institution"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="DEMO"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="admin@demo.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Inactive No new invitation can name it. Its people, their records and the invitations already sent are kept.
[button] [aria-haspopup="dialog"] Reactivate…
[aria-label="Board notes"]
Reactivate… asks first (M2), opening on Cancel.
#### Reactivate Demo Institution?
A new invitation can name it again. Its people and their records are as they were.
[button] Cancel
[button] Reactivate

######## BOARD R3-I15-390.dc.html
Label: Demo Institution · Wombat
> R3-I15
> Reactivate… asked
> NEW: the dialog's words (T11)
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
- [aria-current="page"] Demo Institution
### [tabindex="-1"] Demo Institution
DEMO · Inactive
[tabindex="-1"]
* marks a field you must fill in.
Name * required
[value="Demo Institution"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="DEMO"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="admin@demo.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Inactive No new invitation can name it. Its people, their records and the invitations already sent are kept.
[button] [aria-haspopup="dialog"] Reactivate…
[aria-label="Board notes"]
Reactivate… asks first (M2), opening on Cancel.
#### Reactivate Demo Institution?
A new invitation can name it again. Its people and their records are as they were.
[button] Cancel
[button] Reactivate

######## BOARD R3-I16.dc.html
Label: Demo Institution · Wombat
> R3-I16
> Reactivated
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
- [aria-current="page"] Demo Institution
### [tabindex="-1"] Demo Institution
DEMO · Active
[tabindex="-1"]
[role="status"] Demo Institution reactivated. A new invitation can name it again.
* marks a field you must fill in.
Name * required
[value="Demo Institution"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="DEMO"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="admin@demo.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
Status reads Active with Deactivate… again.

######## BOARD R3-I16-390.dc.html
Label: Demo Institution · Wombat
> R3-I16
> Reactivated
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
- [aria-current="page"] Demo Institution
### [tabindex="-1"] Demo Institution
DEMO · Active
[tabindex="-1"]
[role="status"] Demo Institution reactivated. A new invitation can name it again.
* marks a field you must fill in.
Name * required
[value="Demo Institution"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="DEMO"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="admin@demo.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
Status reads Active with Deactivate… again.

######## BOARD R3-I22.dc.html
Label: Kgosi Kgari Teaching Hospital · Wombat
> R3-I22
> A save that failed
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
- [aria-current="page"] Kgosi Kgari Teaching Hospital
### [tabindex="-1"] Kgosi Kgari Teaching Hospital
KGK · Active
[tabindex="-1"]
[role="alert"] Could not save the institution. Nothing has changed. Try again, or come back in a few minutes.
* marks a field you must fill in.
Name * required
[value="Kgosi Kgari Teaching Hospital"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="KGK"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="paediatrics@kgk.wombat.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
As built (InstitutionEdit.razor's SaveFailed); the typed values stay in the fields.

######## BOARD R3-I22-390.dc.html
Label: Kgosi Kgari Teaching Hospital · Wombat
> R3-I22
> A save that failed
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
- [aria-current="page"] Kgosi Kgari Teaching Hospital
### [tabindex="-1"] Kgosi Kgari Teaching Hospital
KGK · Active
[tabindex="-1"]
[role="alert"] Could not save the institution. Nothing has changed. Try again, or come back in a few minutes.
* marks a field you must fill in.
Name * required
[value="Kgosi Kgari Teaching Hospital"] [aria-required="true"]
Up to 200 characters.
Short code * required
[value="KGK"] [aria-required="true"]
Up to 32 characters.
Contact email
[value="paediatrics@kgk.wombat.local"]
Optional.
[link] Cancel
[button] Save
#### Status
Active A new invitation can name it.
[button] [aria-haspopup="dialog"] Deactivate…
[aria-label="Board notes"]
As built (InstitutionEdit.razor's SaveFailed); the typed values stay in the fields.

######## BOARD R3-J01.dc.html
Label: Scheduled jobs · Wombat
> R3-J01
> A.2.1: the eight jobs
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
Ordered by key. Schedules in words in SAST, the cron under each (T2). The column headed Enabled goes. Stacked from 1150 px (C1); the sweep is on R3-0-spec-2.
Job is the row's header, th scope="row", painted by .people-page's existing rules (S7).

######## BOARD R3-J01-768.dc.html
Label: Scheduled jobs · Wombat
> R3-J01
> A.2.1: the eight jobs
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
Ordered by key. Schedules in words in SAST, the cron under each (T2). The column headed Enabled goes. Stacked from 1150 px (C1); the sweep is on R3-0-spec-2.
Job is the row's header, th scope="row", painted by .people-page's existing rules (S7).

######## BOARD R3-J01-390.dc.html
Label: Scheduled jobs · Wombat
> R3-J01
> A.2.1: the eight jobs
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
Ordered by key. Schedules in words in SAST, the cron under each (T2). The column headed Enabled goes. Stacked from 1150 px (C1); the sweep is on R3-0-spec-2.
Job is the row's header, th scope="row", painted by .people-page's existing rules (S7).

######## BOARD R3-J02.dc.html
Label: Scheduled jobs · Wombat
> R3-J02
> A.2.2: disabled
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="status"] Disabled msf-campaign-auto-close. The scheduler will not run it until you enable it.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | None while disabled | [button] [aria-disabled="true"] Run now: msf-campaign-auto-close [button] Enable msf-campaign-auto-closeEnable it to run it. |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
The result above the list, focused (T5). Run now stays in the tab order, aria-disabled, described by "Enable it to run it."; a press does nothing.
The reason is a span, not a p: a p's own margins would open the actions cell's row (no new rule).

######## BOARD R3-J02-390.dc.html
Label: Scheduled jobs · Wombat
> R3-J02
> A.2.2: disabled
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="status"] Disabled msf-campaign-auto-close. The scheduler will not run it until you enable it.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | None while disabled | [button] [aria-disabled="true"] Run now: msf-campaign-auto-close [button] Enable msf-campaign-auto-closeEnable it to run it. |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
The result above the list, focused (T5). Run now stays in the tab order, aria-disabled, described by "Enable it to run it."; a press does nothing.
The reason is a span, not a p: a p's own margins would open the actions cell's row (no new rule).

######## BOARD R3-J03.dc.html
Label: Scheduled jobs · Wombat
> R3-J03
> A.2.2: enabled again
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="status"] Enabled msf-campaign-auto-close. Its next run is 2026-11-10 22:00 SAST.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
True as drawn (K5): msf-campaign-auto-close was disabled for less than an hour, so the host's next occurrence after the last run's start (21:00 SAST) is 22:00 SAST. A job due as soon as it is enabled reads Next run "At the next check, within 30 seconds" and "Enabled ‹key›. It runs at the next check, within 30 seconds." (PROPOSED, words only, on the Spec).

######## BOARD R3-J03-390.dc.html
Label: Scheduled jobs · Wombat
> R3-J03
> A.2.2: enabled again
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="status"] Enabled msf-campaign-auto-close. Its next run is 2026-11-10 22:00 SAST.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
True as drawn (K5): msf-campaign-auto-close was disabled for less than an hour, so the host's next occurrence after the last run's start (21:00 SAST) is 22:00 SAST. A job due as soon as it is enabled reads Next run "At the next check, within 30 seconds" and "Enabled ‹key›. It runs at the next check, within 30 seconds." (PROPOSED, words only, on the Spec).

######## BOARD R3-J04.dc.html
Label: Scheduled jobs · Wombat
> R3-J04
> A.2.4: ran
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="status"] Ran weekly-coordinator-digest: Succeeded at 2026-11-10 21:39 SAST.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | 2026-11-10 21:39 SASTSucceeded | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
The run has ended when the result shows (Q10). 3.32's reads "Ran assessor-pending-nudge: Succeeded at 2026-11-10 19:15 SAST."

######## BOARD R3-J04-390.dc.html
Label: Scheduled jobs · Wombat
> R3-J04
> A.2.4: ran
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="status"] Ran weekly-coordinator-digest: Succeeded at 2026-11-10 21:39 SAST.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | 2026-11-10 21:39 SASTSucceeded | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
The run has ended when the result shows (Q10). 3.32's reads "Ran assessor-pending-nudge: Succeeded at 2026-11-10 19:15 SAST."

######## BOARD R3-J05.dc.html
Label: Scheduled jobs · Wombat
> R3-J05
> Run now waiting
> Illustration
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] [aria-disabled="true"] Running…: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
"Running…" on that button, aria-disabled; the other row actions send nothing while it waits (T234); the result replaces it when the run ends (T16).

######## BOARD R3-J05-390.dc.html
Label: Scheduled jobs · Wombat
> R3-J05
> Run now waiting
> Illustration
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] [aria-disabled="true"] Running…: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
"Running…" on that button, aria-disabled; the other row actions send nothing while it waits (T234); the result replaces it when the run ends (T16).

######## BOARD R3-J06.dc.html
Label: Scheduled jobs · Wombat
> R3-J06
> Run now refused while that job runs
> Illustration
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="alert"] Job 'audit-log-retention' is already running, so it was not started again. Wait for that run to finish, then run it again if it is still needed.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | ‹YYYY-MM-DD hh:mm› SASTRunning | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
Two Administrators, or the scheduler, running the same job. The refusal now takes the focus in the result region (N7; today it stays on the button). Running stays amber (badge-accepted, N5).

######## BOARD R3-J06-390.dc.html
Label: Scheduled jobs · Wombat
> R3-J06
> Run now refused while that job runs
> Illustration
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="alert"] Job 'audit-log-retention' is already running, so it was not started again. Wait for that run to finish, then run it again if it is still needed.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | ‹YYYY-MM-DD hh:mm› SASTRunning | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
Two Administrators, or the scheduler, running the same job. The refusal now takes the focus in the result region (N7; today it stays on the button). Running stays amber (badge-accepted, N5).

######## BOARD R3-J07.dc.html
Label: Scheduled jobs · Wombat
> R3-J07
> Run now ended in failure
> Illustration
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="alert"] Ran entrustment-decision-expiry: Failed at ‹YYYY-MM-DD hh:mm› SAST: ‹the error as stored›.
[link] Its runs: entrustment-decision-expiry, failed
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | ‹YYYY-MM-DD hh:mm› SASTFailed‹the error as stored› | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
DATA: RunScheduledJobNowCommand returns its run (status, start, finish, error).

######## BOARD R3-J07-390.dc.html
Label: Scheduled jobs · Wombat
> R3-J07
> Run now ended in failure
> Illustration
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="alert"] Ran entrustment-decision-expiry: Failed at ‹YYYY-MM-DD hh:mm› SAST: ‹the error as stored›.
[link] Its runs: entrustment-decision-expiry, failed
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | ‹YYYY-MM-DD hh:mm› SASTFailed‹the error as stored› | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
DATA: RunScheduledJobNowCommand returns its run (status, start, finish, error).

######## BOARD R3-J08.dc.html
Label: Scheduled jobs · Wombat
> R3-J08
> A failed last run in its row
> Illustration
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | ‹YYYY-MM-DD hh:mm› SASTFailedMarked as failed during startup reconciliation — process likely crashed. | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
A run left Running at a crash is failed at the next start with this error (ScheduledJobHost.cs:212). DATA: ScheduledJobDto carries the last run's error.

######## BOARD R3-J08-390.dc.html
Label: Scheduled jobs · Wombat
> R3-J08
> A failed last run in its row
> Illustration
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | ‹YYYY-MM-DD hh:mm› SASTFailedMarked as failed during startup reconciliation — process likely crashed. | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
A run left Running at a crash is failed at the next start with this error (ScheduledJobHost.cs:212). DATA: ScheduledJobDto carries the last run's error.

######## BOARD R3-J09.dc.html
Label: Scheduled jobs · Wombat
> R3-J09
> Run now on a disabled job, from a stale page
> Illustration
> NEW: the refusal (T22)
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="alert"] Job 'msf-campaign-auto-close' is disabled, so it was not run. Enable it, then run it.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | None while disabled | [button] [aria-disabled="true"] Run now: msf-campaign-auto-close [button] Enable msf-campaign-auto-closeEnable it to run it. |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
The page re-reads after the refusal, so the row now reads disabled.

######## BOARD R3-J09-390.dc.html
Label: Scheduled jobs · Wombat
> R3-J09
> Run now on a disabled job, from a stale page
> Illustration
> NEW: the refusal (T22)
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="alert"] Job 'msf-campaign-auto-close' is disabled, so it was not run. Enable it, then run it.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | None while disabled | [button] [aria-disabled="true"] Run now: msf-campaign-auto-close [button] Enable msf-campaign-auto-closeEnable it to run it. |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
The page re-reads after the refusal, so the row now reads disabled.

######## BOARD R3-J10.dc.html
Label: Scheduled jobs · Wombat
> R3-J10
> Disable failed
> Illustration
> NEW: the failure's words (T23)
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="alert"] msf-campaign-auto-close could not be disabled. Nothing has changed. Try again, or come back in a few minutes.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
The cause goes to the log; the row is as it was.

######## BOARD R3-J10-390.dc.html
Label: Scheduled jobs · Wombat
> R3-J10
> Disable failed
> Illustration
> NEW: the failure's words (T23)
> PROPOSED (THIS ASK), NEW: scheduled-job-run-retention’s description (Q18)
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
[role="alert"] msf-campaign-auto-close could not be disabled. Nothing has changed. Try again, or come back in a few minutes.
Scheduled jobs
| Job | Schedule | Last run | Next run | Actions |
| activity-draft-nudgeReminds trainees about draft activities older than 14 days. | Daily at 09:00 SASTcron 0 7 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 09:00 SAST | [button] Run now: activity-draft-nudge [button] Disable activity-draft-nudge |
| assessor-pending-nudgeNudges assessors about activities waiting for their assessment for more than 5 days. | Daily at 11:00 SASTcron 0 9 * * *, UTC | 2026-11-10 19:15 SASTSucceeded | 2026-11-11 11:00 SAST | [button] Run now: assessor-pending-nudge [button] Disable assessor-pending-nudge |
| audit-log-retentionArchives audit log entries older than 2 years into the archive table. | Daily at 05:00 SASTcron 0 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:00 SAST | [button] Run now: audit-log-retention [button] Disable audit-log-retention |
| entrustment-decision-expiryExpires entrustment decisions past their expiry date and sends 30-day expiry reminders. | Daily at 05:30 SASTcron 30 3 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 05:30 SAST | [button] Run now: entrustment-decision-expiry [button] Disable entrustment-decision-expiry |
| msf-campaign-auto-closeCloses open MSF campaigns whose window has ended and anonymises their respondents. | Hourly, on the hourcron 0 * * * *, UTC | 2026-11-10 21:00 SASTSucceeded | 2026-11-10 22:00 SAST | [button] Run now: msf-campaign-auto-close [button] Disable msf-campaign-auto-close |
| msf-invitation-expiry-reminderReminds MSF respondents who have not responded, once, two days before their last day to respond. | Daily at 10:00 SASTcron 0 8 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 10:00 SAST | [button] Run now: msf-invitation-expiry-reminder [button] Disable msf-invitation-expiry-reminder |
| scheduled-job-run-retentionDeletes job run records and mail outcomes older than 90 days. | Daily at 06:00 SASTcron 0 4 * * *, UTC | 2026-11-10 17:00 SASTSucceeded | 2026-11-11 06:00 SAST | [button] Run now: scheduled-job-run-retention [button] Disable scheduled-job-run-retention |
| weekly-coordinator-digestSends weekly digest emails to Coordinators. | Mondays at 10:00 SASTcron 0 8 * * 1, UTC | Not run yet | 2026-11-16 10:00 SAST | [button] Run now: weekly-coordinator-digest [button] Disable weekly-coordinator-digest |
[aria-label="Board notes"]
The cause goes to the log; the row is as it was.

######## BOARD R3-R01.dc.html
Label: Run history · Wombat
> R3-R01
> A.2.10: 21 runs
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
[tabindex="-1"] 21 runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| scheduled-job-run-retention | 2026-11-10 21:41:44 SAST | 7 ms | Succeeded | Demo Administrator |
| audit-log-retention | 2026-11-10 21:41:39 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-invitation-expiry-reminder | 2026-11-10 21:41:34 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:41:28 SAST | 14 ms | Succeeded | Demo Administrator |
| entrustment-decision-expiry | 2026-11-10 21:41:23 SAST | 29 ms | Succeeded | Demo Administrator |
| assessor-pending-nudge | 2026-11-10 21:41:07 SAST | 23 ms | Succeeded | Demo Administrator |
| activity-draft-nudge | 2026-11-10 21:41:02 SAST | 13 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:58 SAST | 25 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:40 SAST | 68 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:00:03 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 20:00:02 SAST | 35 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 19:15:54 SAST | 12 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 19:00:00 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 18:00:28 SAST | 35 ms | Succeeded | Scheduler |
| entrustment-decision-expiry | 2026-11-10 17:00:54 SAST | 100 ms | Succeeded | Scheduler |
| scheduled-job-run-retention | 2026-11-10 17:00:54 SAST | 24 ms | Succeeded | Scheduler |
| audit-log-retention | 2026-11-10 17:00:54 SAST | 67 ms | Succeeded | Scheduler |
| msf-invitation-expiry-reminder | 2026-11-10 17:00:54 SAST | 89 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 17:00:54 SAST | 102 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 17:00:54 SAST | 70 ms | Succeeded | Scheduler |
| activity-draft-nudge | 2026-11-10 17:00:54 SAST | 80 ms | Succeeded | Scheduler |
Showing 1–21 of 21
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
10 by hand, Ran by Demo Administrator; 11 by the Scheduler (the brief's 11/10 corrected). Stacked from 1024 px (C1).
Job is the row's header (S7). The pager in PagerControls' order: info, Previous and Next, Per page (M2).

######## BOARD R3-R01-768.dc.html
Label: Run history · Wombat
> R3-R01
> A.2.10: 21 runs
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
[tabindex="-1"] 21 runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| scheduled-job-run-retention | 2026-11-10 21:41:44 SAST | 7 ms | Succeeded | Demo Administrator |
| audit-log-retention | 2026-11-10 21:41:39 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-invitation-expiry-reminder | 2026-11-10 21:41:34 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:41:28 SAST | 14 ms | Succeeded | Demo Administrator |
| entrustment-decision-expiry | 2026-11-10 21:41:23 SAST | 29 ms | Succeeded | Demo Administrator |
| assessor-pending-nudge | 2026-11-10 21:41:07 SAST | 23 ms | Succeeded | Demo Administrator |
| activity-draft-nudge | 2026-11-10 21:41:02 SAST | 13 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:58 SAST | 25 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:40 SAST | 68 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:00:03 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 20:00:02 SAST | 35 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 19:15:54 SAST | 12 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 19:00:00 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 18:00:28 SAST | 35 ms | Succeeded | Scheduler |
| entrustment-decision-expiry | 2026-11-10 17:00:54 SAST | 100 ms | Succeeded | Scheduler |
| scheduled-job-run-retention | 2026-11-10 17:00:54 SAST | 24 ms | Succeeded | Scheduler |
| audit-log-retention | 2026-11-10 17:00:54 SAST | 67 ms | Succeeded | Scheduler |
| msf-invitation-expiry-reminder | 2026-11-10 17:00:54 SAST | 89 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 17:00:54 SAST | 102 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 17:00:54 SAST | 70 ms | Succeeded | Scheduler |
| activity-draft-nudge | 2026-11-10 17:00:54 SAST | 80 ms | Succeeded | Scheduler |
Showing 1–21 of 21
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
10 by hand, Ran by Demo Administrator; 11 by the Scheduler (the brief's 11/10 corrected). Stacked from 1024 px (C1).
Job is the row's header (S7). The pager in PagerControls' order: info, Previous and Next, Per page (M2).

######## BOARD R3-R01-390.dc.html
Label: Run history · Wombat
> R3-R01
> A.2.10: 21 runs
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
[tabindex="-1"] 21 runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| scheduled-job-run-retention | 2026-11-10 21:41:44 SAST | 7 ms | Succeeded | Demo Administrator |
| audit-log-retention | 2026-11-10 21:41:39 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-invitation-expiry-reminder | 2026-11-10 21:41:34 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:41:28 SAST | 14 ms | Succeeded | Demo Administrator |
| entrustment-decision-expiry | 2026-11-10 21:41:23 SAST | 29 ms | Succeeded | Demo Administrator |
| assessor-pending-nudge | 2026-11-10 21:41:07 SAST | 23 ms | Succeeded | Demo Administrator |
| activity-draft-nudge | 2026-11-10 21:41:02 SAST | 13 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:58 SAST | 25 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:40 SAST | 68 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:00:03 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 20:00:02 SAST | 35 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 19:15:54 SAST | 12 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 19:00:00 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 18:00:28 SAST | 35 ms | Succeeded | Scheduler |
| entrustment-decision-expiry | 2026-11-10 17:00:54 SAST | 100 ms | Succeeded | Scheduler |
| scheduled-job-run-retention | 2026-11-10 17:00:54 SAST | 24 ms | Succeeded | Scheduler |
| audit-log-retention | 2026-11-10 17:00:54 SAST | 67 ms | Succeeded | Scheduler |
| msf-invitation-expiry-reminder | 2026-11-10 17:00:54 SAST | 89 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 17:00:54 SAST | 102 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 17:00:54 SAST | 70 ms | Succeeded | Scheduler |
| activity-draft-nudge | 2026-11-10 17:00:54 SAST | 80 ms | Succeeded | Scheduler |
Showing 1–21 of 21
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
10 by hand, Ran by Demo Administrator; 11 by the Scheduler (the brief's 11/10 corrected). Stacked from 1024 px (C1).
Job is the row's header (S7). The pager in PagerControls' order: info, Previous and Next, Per page (M2).

######## BOARD R3-R02.dc.html
Label: Run history · Wombat
> R3-R02
> A.2.10: Job weekly-coordinator-digest, Show
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
All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
entrustment-decision-expiry
msf-campaign-auto-close
msf-invitation-expiry-reminder
scheduled-job-run-retention
[selected] weekly-coordinator-digest
Status
[selected] All
Running
Succeeded
Failed
From (a South African day)
To (a South African day)
[button] Show
[button] Clear filters
#### [tabindex="-1"] Runs
[tabindex="-1"] 2 runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| weekly-coordinator-digest | 2026-11-10 21:39:58 SAST | 25 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:40 SAST | 68 ms | Succeeded | Demo Administrator |
Showing 1–2 of 2
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Show moves the focus to the count line. A select of the eight keys cannot miss, so the "digest" sub-step goes.

######## BOARD R3-R02-390.dc.html
Label: Run history · Wombat
> R3-R02
> A.2.10: Job weekly-coordinator-digest, Show
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
All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
entrustment-decision-expiry
msf-campaign-auto-close
msf-invitation-expiry-reminder
scheduled-job-run-retention
[selected] weekly-coordinator-digest
Status
[selected] All
Running
Succeeded
Failed
From (a South African day)
To (a South African day)
[button] Show
[button] Clear filters
#### [tabindex="-1"] Runs
[tabindex="-1"] 2 runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| weekly-coordinator-digest | 2026-11-10 21:39:58 SAST | 25 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:40 SAST | 68 ms | Succeeded | Demo Administrator |
Showing 1–2 of 2
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Show moves the focus to the count line. A select of the eight keys cannot miss, so the "digest" sub-step goes.

######## BOARD R3-R04.dc.html
Label: Run history · Wombat
> R3-R04
> A.2.10: From 2026-11-10, Show
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
[value="2026-11-10"]
To (a South African day)
[button] Show
[button] Clear filters
#### [tabindex="-1"] Runs
[tabindex="-1"] 21 runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| scheduled-job-run-retention | 2026-11-10 21:41:44 SAST | 7 ms | Succeeded | Demo Administrator |
| audit-log-retention | 2026-11-10 21:41:39 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-invitation-expiry-reminder | 2026-11-10 21:41:34 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:41:28 SAST | 14 ms | Succeeded | Demo Administrator |
| entrustment-decision-expiry | 2026-11-10 21:41:23 SAST | 29 ms | Succeeded | Demo Administrator |
| assessor-pending-nudge | 2026-11-10 21:41:07 SAST | 23 ms | Succeeded | Demo Administrator |
| activity-draft-nudge | 2026-11-10 21:41:02 SAST | 13 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:58 SAST | 25 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:40 SAST | 68 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:00:03 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 20:00:02 SAST | 35 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 19:15:54 SAST | 12 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 19:00:00 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 18:00:28 SAST | 35 ms | Succeeded | Scheduler |
| entrustment-decision-expiry | 2026-11-10 17:00:54 SAST | 100 ms | Succeeded | Scheduler |
| scheduled-job-run-retention | 2026-11-10 17:00:54 SAST | 24 ms | Succeeded | Scheduler |
| audit-log-retention | 2026-11-10 17:00:54 SAST | 67 ms | Succeeded | Scheduler |
| msf-invitation-expiry-reminder | 2026-11-10 17:00:54 SAST | 89 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 17:00:54 SAST | 102 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 17:00:54 SAST | 70 ms | Succeeded | Scheduler |
| activity-draft-nudge | 2026-11-10 17:00:54 SAST | 80 ms | Succeeded | Scheduler |
Showing 1–21 of 21
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
A South African day: 2026-11-10 00:00 to 24:00 SAST.

######## BOARD R3-R04-390.dc.html
Label: Run history · Wombat
> R3-R04
> A.2.10: From 2026-11-10, Show
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
[value="2026-11-10"]
To (a South African day)
[button] Show
[button] Clear filters
#### [tabindex="-1"] Runs
[tabindex="-1"] 21 runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| scheduled-job-run-retention | 2026-11-10 21:41:44 SAST | 7 ms | Succeeded | Demo Administrator |
| audit-log-retention | 2026-11-10 21:41:39 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-invitation-expiry-reminder | 2026-11-10 21:41:34 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:41:28 SAST | 14 ms | Succeeded | Demo Administrator |
| entrustment-decision-expiry | 2026-11-10 21:41:23 SAST | 29 ms | Succeeded | Demo Administrator |
| assessor-pending-nudge | 2026-11-10 21:41:07 SAST | 23 ms | Succeeded | Demo Administrator |
| activity-draft-nudge | 2026-11-10 21:41:02 SAST | 13 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:58 SAST | 25 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:40 SAST | 68 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:00:03 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 20:00:02 SAST | 35 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 19:15:54 SAST | 12 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 19:00:00 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 18:00:28 SAST | 35 ms | Succeeded | Scheduler |
| entrustment-decision-expiry | 2026-11-10 17:00:54 SAST | 100 ms | Succeeded | Scheduler |
| scheduled-job-run-retention | 2026-11-10 17:00:54 SAST | 24 ms | Succeeded | Scheduler |
| audit-log-retention | 2026-11-10 17:00:54 SAST | 67 ms | Succeeded | Scheduler |
| msf-invitation-expiry-reminder | 2026-11-10 17:00:54 SAST | 89 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 17:00:54 SAST | 102 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 17:00:54 SAST | 70 ms | Succeeded | Scheduler |
| activity-draft-nudge | 2026-11-10 17:00:54 SAST | 80 ms | Succeeded | Scheduler |
Showing 1–21 of 21
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
A South African day: 2026-11-10 00:00 to 24:00 SAST.

######## BOARD R3-R05-390.dc.html
Label: Run history · Wombat
> R3-R05
> A.7.11: 22 runs on a phone
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
[tabindex="-1"] 22 runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| msf-campaign-auto-close | 2026-11-10 22:00:13 SAST | 4 ms | Succeeded | Scheduler |
| scheduled-job-run-retention | 2026-11-10 21:41:44 SAST | 7 ms | Succeeded | Demo Administrator |
| audit-log-retention | 2026-11-10 21:41:39 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-invitation-expiry-reminder | 2026-11-10 21:41:34 SAST | 20 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:41:28 SAST | 14 ms | Succeeded | Demo Administrator |
| entrustment-decision-expiry | 2026-11-10 21:41:23 SAST | 29 ms | Succeeded | Demo Administrator |
| assessor-pending-nudge | 2026-11-10 21:41:07 SAST | 23 ms | Succeeded | Demo Administrator |
| activity-draft-nudge | 2026-11-10 21:41:02 SAST | 13 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:58 SAST | 25 ms | Succeeded | Demo Administrator |
| weekly-coordinator-digest | 2026-11-10 21:39:40 SAST | 68 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 21:00:03 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 20:00:02 SAST | 35 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 19:15:54 SAST | 12 ms | Succeeded | Demo Administrator |
| msf-campaign-auto-close | 2026-11-10 19:00:00 SAST | 29 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 18:00:28 SAST | 35 ms | Succeeded | Scheduler |
| entrustment-decision-expiry | 2026-11-10 17:00:54 SAST | 100 ms | Succeeded | Scheduler |
| scheduled-job-run-retention | 2026-11-10 17:00:54 SAST | 24 ms | Succeeded | Scheduler |
| audit-log-retention | 2026-11-10 17:00:54 SAST | 67 ms | Succeeded | Scheduler |
| msf-invitation-expiry-reminder | 2026-11-10 17:00:54 SAST | 89 ms | Succeeded | Scheduler |
| assessor-pending-nudge | 2026-11-10 17:00:54 SAST | 102 ms | Succeeded | Scheduler |
| msf-campaign-auto-close | 2026-11-10 17:00:54 SAST | 70 ms | Succeeded | Scheduler |
| activity-draft-nudge | 2026-11-10 17:00:54 SAST | 80 ms | Succeeded | Scheduler |
Showing 1–22 of 22
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
A.7.11-4 adds msf-campaign-auto-close at 22:00:13 by the Scheduler, 4 ms. Filters stack; each run a block; nothing scrolls sideways.

######## BOARD R3-R06.dc.html
Label: Run history · Wombat
> R3-R06
> Page 2 past 50
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
[tabindex="-1"] ‹n› runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| ‹key› | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | ‹n› ms | Succeeded | Scheduler |
| ‹key› | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | ‹n› ms | Succeeded | Scheduler |
| ‹key› | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | ‹n› ms | Succeeded | Scheduler |
‹47 more rows of page 2, not drawn›
Showing 51–100 of ‹n›
[button] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Thousands of runs: about 2,800 held at once (one job hourly, 90 days). Next and Previous move the focus to h2 "Runs" (T1). DATA: a page and a total (today Take(200)).

######## BOARD R3-R06-390.dc.html
Label: Run history · Wombat
> R3-R06
> Page 2 past 50
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
[tabindex="-1"] ‹n› runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| ‹key› | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | ‹n› ms | Succeeded | Scheduler |
| ‹key› | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | ‹n› ms | Succeeded | Scheduler |
| ‹key› | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | ‹n› ms | Succeeded | Scheduler |
‹47 more rows of page 2, not drawn›
Showing 51–100 of ‹n›
[button] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Thousands of runs: about 2,800 held at once (one job hourly, 90 days). Next and Previous move the focus to h2 "Runs" (T1). DATA: a page and a total (today Take(200)).

######## BOARD R3-R07.dc.html
Label: Run history · Wombat
> R3-R07
> A run still Running; a 2,000-character error
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
[tabindex="-1"] ‹n› runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| audit-log-retention | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | — | Running | Scheduler |
| entrustment-decision-expiry | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | ‹n› ms | Failed‹the error as stored, shown whole up to its 2,000 characters and wrapping under its badge at every width: this bracket stands for a long refusal from a mail server or a database, with its own words, a stack line or two and an inner exception, never cut at 80 characters with a tooltip and never widening the table› | Demo Administrator |
Showing 1–2 of ‹n›
[button] [aria-disabled="true"] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Duration "—" while it runs; "Running" once, the badge (N9). No "Error: None" on a run that did not fail.

######## BOARD R3-R07-390.dc.html
Label: Run history · Wombat
> R3-R07
> A run still Running; a 2,000-character error
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
[tabindex="-1"] ‹n› runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| audit-log-retention | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | — | Running | Scheduler |
| entrustment-decision-expiry | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | ‹n› ms | Failed‹the error as stored, shown whole up to its 2,000 characters and wrapping under its badge at every width: this bracket stands for a long refusal from a mail server or a database, with its own words, a stack line or two and an inner exception, never cut at 80 characters with a tooltip and never widening the table› | Demo Administrator |
Showing 1–2 of ‹n›
[button] [aria-disabled="true"] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Duration "—" while it runs; "Running" once, the badge (N9). No "Error: None" on a run that did not fail.

######## BOARD R3-R09.dc.html
Label: Run history · Wombat
> R3-R09
> From Home's Its runs
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
All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
[selected] entrustment-decision-expiry
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
[tabindex="-1"] ‹n› runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| entrustment-decision-expiry | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | ‹n› ms | Failed‹the error as stored› | Scheduler |
Showing 1–‹n› of ‹n›
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
The address ?job=entrustment-decision-expiry&status=Failed sets Job and Status (T18).

######## BOARD R3-R09-390.dc.html
Label: Run history · Wombat
> R3-R09
> From Home's Its runs
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
All jobs
activity-draft-nudge
assessor-pending-nudge
audit-log-retention
[selected] entrustment-decision-expiry
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
[tabindex="-1"] ‹n› runs.
Runs, newest first
| Job | Started | Duration | Status | Ran by |
| entrustment-decision-expiry | ‹YYYY-MM-DD› ‹hh:mm:ss› SAST | ‹n› ms | Failed‹the error as stored› | Scheduler |
Showing 1–‹n› of ‹n›
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
The address ?job=entrustment-decision-expiry&status=Failed sets Job and Status (T18).

######## BOARD R3-L01.dc.html
Label: Audit log · Wombat
> R3-L01
> 3.55: Nolwazi Mbatha from 18:58 SAST
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
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 166 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:24:10 SAST | Nolwazi Mbatha | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:10 SAST |
| 2026-11-10 20:22:59 SAST | Nolwazi Mbatha | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:59 SAST |
| 2026-11-10 20:22:29 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:29 SAST |
| 2026-11-10 20:21:44 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:44 SAST |
| 2026-11-10 20:21:43 SAST | Lerato Molefe | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:21:43 SAST |
| 2026-11-10 20:21:29 SAST | Lerato Molefe | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:29 SAST |
| 2026-11-10 20:21:20 SAST | Lerato Molefe | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:20 SAST |
| 2026-11-10 20:21:12 SAST | Pieter Smit | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:21:05 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:05 SAST |
| 2026-11-10 20:20:55 SAST | Pieter Smit | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:48 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:48 SAST |
| 2026-11-10 20:20:19 SAST | Pieter Smit | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
‹38 more rows of this page, not drawn›
Showing 1–50 of 166
[button] [aria-disabled="true"] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
166 KGK rows: 58 Login, 25 Logout, 23 CreateActivityCommand, 40 TransitionActivityCommand, 19 MSF, 1 SendActivityReminderCommand. Nolwazi Mbatha's own sign-in the newest.
Show sits after the filters (T328); the person by the live name (Q15). Six columns; stacked from 1150 px (C1).
Each Detail's name carries the zone: "Detail: Login at 2026-11-10 20:24:10 SAST" (C7). The pager in PagerControls' order (M2).

######## BOARD R3-L01-768.dc.html
Label: Audit log · Wombat
> R3-L01
> 3.55: Nolwazi Mbatha from 18:58 SAST
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
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 166 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:24:10 SAST | Nolwazi Mbatha | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:10 SAST |
| 2026-11-10 20:22:59 SAST | Nolwazi Mbatha | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:59 SAST |
| 2026-11-10 20:22:29 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:29 SAST |
| 2026-11-10 20:21:44 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:44 SAST |
| 2026-11-10 20:21:43 SAST | Lerato Molefe | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:21:43 SAST |
| 2026-11-10 20:21:29 SAST | Lerato Molefe | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:29 SAST |
| 2026-11-10 20:21:20 SAST | Lerato Molefe | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:20 SAST |
| 2026-11-10 20:21:12 SAST | Pieter Smit | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:21:05 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:05 SAST |
| 2026-11-10 20:20:55 SAST | Pieter Smit | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:48 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:48 SAST |
| 2026-11-10 20:20:19 SAST | Pieter Smit | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
‹38 more rows of this page, not drawn›
Showing 1–50 of 166
[button] [aria-disabled="true"] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
166 KGK rows: 58 Login, 25 Logout, 23 CreateActivityCommand, 40 TransitionActivityCommand, 19 MSF, 1 SendActivityReminderCommand. Nolwazi Mbatha's own sign-in the newest.
Show sits after the filters (T328); the person by the live name (Q15). Six columns; stacked from 1150 px (C1).
Each Detail's name carries the zone: "Detail: Login at 2026-11-10 20:24:10 SAST" (C7). The pager in PagerControls' order (M2).

######## BOARD R3-L01-390.dc.html
Label: Audit log · Wombat
> R3-L01
> 3.55: Nolwazi Mbatha from 18:58 SAST
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
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 166 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:24:10 SAST | Nolwazi Mbatha | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:10 SAST |
| 2026-11-10 20:22:59 SAST | Nolwazi Mbatha | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:59 SAST |
| 2026-11-10 20:22:29 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:29 SAST |
| 2026-11-10 20:21:44 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:44 SAST |
| 2026-11-10 20:21:43 SAST | Lerato Molefe | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:21:43 SAST |
| 2026-11-10 20:21:29 SAST | Lerato Molefe | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:29 SAST |
| 2026-11-10 20:21:20 SAST | Lerato Molefe | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:20 SAST |
| 2026-11-10 20:21:12 SAST | Pieter Smit | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:21:05 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:05 SAST |
| 2026-11-10 20:20:55 SAST | Pieter Smit | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:48 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:48 SAST |
| 2026-11-10 20:20:19 SAST | Pieter Smit | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
‹38 more rows of this page, not drawn›
Showing 1–50 of 166
[button] [aria-disabled="true"] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
166 KGK rows: 58 Login, 25 Logout, 23 CreateActivityCommand, 40 TransitionActivityCommand, 19 MSF, 1 SendActivityReminderCommand. Nolwazi Mbatha's own sign-in the newest.
Show sits after the filters (T328); the person by the live name (Q15). Six columns; stacked from 1150 px (C1).
Each Detail's name carries the zone: "Detail: Login at 2026-11-10 20:24:10 SAST" (C7). The pager in PagerControls' order (M2).

######## BOARD R3-L02.dc.html
Label: Audit log · Wombat
> R3-L02
> 3.55: Failures only
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
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
All results
Success only
[selected] Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 4 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 19:02:02 SAST | Fatima Khumalo | Command | TransitionActivityCommand | FailedDecline requires a note. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:02:02 SAST |
| 2026-11-10 19:01:45 SAST | Sipho Ndlovu | Command | CreateActivityCommand | FailedDate observed: The date cannot be before your programme started (2026-01-15). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:45 SAST |
| 2026-11-10 19:01:41 SAST | Sipho Ndlovu | Command | CreateActivityCommand | FailedDate observed: The date cannot be after today (2026-11-10). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:41 SAST |
| 2026-11-10 19:00:06 SAST | Anele Dlamini | Command | TransitionActivityCommand | FailedPresenting problem: A value is required. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:00:06 SAST |
Showing 1–4 of 4
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Each refusal under its Failed badge. 19:01:41's words are observed (3.56-1); the other three are the steps' own words, inferred to be the stored text (the build reads it).

######## BOARD R3-L02-390.dc.html
Label: Audit log · Wombat
> R3-L02
> 3.55: Failures only
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
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
All results
Success only
[selected] Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 4 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 19:02:02 SAST | Fatima Khumalo | Command | TransitionActivityCommand | FailedDecline requires a note. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:02:02 SAST |
| 2026-11-10 19:01:45 SAST | Sipho Ndlovu | Command | CreateActivityCommand | FailedDate observed: The date cannot be before your programme started (2026-01-15). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:45 SAST |
| 2026-11-10 19:01:41 SAST | Sipho Ndlovu | Command | CreateActivityCommand | FailedDate observed: The date cannot be after today (2026-11-10). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:41 SAST |
| 2026-11-10 19:00:06 SAST | Anele Dlamini | Command | TransitionActivityCommand | FailedPresenting problem: A value is required. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:00:06 SAST |
Showing 1–4 of 4
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Each refusal under its Failed badge. 19:01:41's words are observed (3.56-1); the other three are the steps' own words, inferred to be the stored text (the build reads it).

######## BOARD R3-L03.dc.html
Label: Audit log · Wombat
> R3-L03
> 3.55: Action "Msf"
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
[value="Msf"]
Part of a name: "Msf" finds the MSF commands.
Person's user id
Subject type
From (SAST)
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 19 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:21:12 SAST | Pieter Smit | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:20:55 SAST | Pieter Smit | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:19 SAST | Pieter Smit | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
| 2026-11-10 19:18:49 SAST | Pieter Smit | Command | WithdrawMsfCampaignCommand | Success | [link] Detail: WithdrawMsfCampaignCommand at 2026-11-10 19:18:49 SAST |
| 2026-11-10 19:18:40 SAST | Pieter Smit | Command | CreateMsfCampaignCommand | Success | [link] Detail: CreateMsfCampaignCommand at 2026-11-10 19:18:40 SAST |
| 2026-11-10 19:18:22 SAST | Pieter Smit | Command | OpenMsfCampaignCommand | Success | [link] Detail: OpenMsfCampaignCommand at 2026-11-10 19:18:22 SAST |
| 2026-11-10 19:18:16 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:16 SAST |
| 2026-11-10 19:18:14 SAST | Pieter Smit | Command | RemoveMsfInvitationCommand | Success | [link] Detail: RemoveMsfInvitationCommand at 2026-11-10 19:18:14 SAST |
| 2026-11-10 19:18:10 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:10 SAST |
| 2026-11-10 19:18:09 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:09 SAST |
| 2026-11-10 19:18:08 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:08 SAST |
| 2026-11-10 19:18:06 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:06 SAST |
| 2026-11-10 19:18:05 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:05 SAST |
| 2026-11-10 19:18:04 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:04 SAST |
| 2026-11-10 19:18:02 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:02 SAST |
| 2026-11-10 19:18:01 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:01 SAST |
| 2026-11-10 19:18:00 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:00 SAST |
| 2026-11-10 19:17:47 SAST | Pieter Smit | Command | CreateMsfCampaignCommand | Success | [link] Detail: CreateMsfCampaignCommand at 2026-11-10 19:17:47 SAST |
| 2026-11-10 19:17:31 SAST | Pieter Smit | Command | CreateMsfTemplateCommand | Success | [link] Detail: CreateMsfTemplateCommand at 2026-11-10 19:17:31 SAST |
Showing 1–19 of 19
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Pieter Smit's nineteen: the template, two campaigns, ten invitations, one removal, the open, the withdrawal, the resend, the close and the release. No respondent's row: those have no institution.

######## BOARD R3-L03-390.dc.html
Label: Audit log · Wombat
> R3-L03
> 3.55: Action "Msf"
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
[value="Msf"]
Part of a name: "Msf" finds the MSF commands.
Person's user id
Subject type
From (SAST)
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 19 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:21:12 SAST | Pieter Smit | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:20:55 SAST | Pieter Smit | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:19 SAST | Pieter Smit | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
| 2026-11-10 19:18:49 SAST | Pieter Smit | Command | WithdrawMsfCampaignCommand | Success | [link] Detail: WithdrawMsfCampaignCommand at 2026-11-10 19:18:49 SAST |
| 2026-11-10 19:18:40 SAST | Pieter Smit | Command | CreateMsfCampaignCommand | Success | [link] Detail: CreateMsfCampaignCommand at 2026-11-10 19:18:40 SAST |
| 2026-11-10 19:18:22 SAST | Pieter Smit | Command | OpenMsfCampaignCommand | Success | [link] Detail: OpenMsfCampaignCommand at 2026-11-10 19:18:22 SAST |
| 2026-11-10 19:18:16 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:16 SAST |
| 2026-11-10 19:18:14 SAST | Pieter Smit | Command | RemoveMsfInvitationCommand | Success | [link] Detail: RemoveMsfInvitationCommand at 2026-11-10 19:18:14 SAST |
| 2026-11-10 19:18:10 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:10 SAST |
| 2026-11-10 19:18:09 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:09 SAST |
| 2026-11-10 19:18:08 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:08 SAST |
| 2026-11-10 19:18:06 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:06 SAST |
| 2026-11-10 19:18:05 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:05 SAST |
| 2026-11-10 19:18:04 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:04 SAST |
| 2026-11-10 19:18:02 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:02 SAST |
| 2026-11-10 19:18:01 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:01 SAST |
| 2026-11-10 19:18:00 SAST | Pieter Smit | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:00 SAST |
| 2026-11-10 19:17:47 SAST | Pieter Smit | Command | CreateMsfCampaignCommand | Success | [link] Detail: CreateMsfCampaignCommand at 2026-11-10 19:17:47 SAST |
| 2026-11-10 19:17:31 SAST | Pieter Smit | Command | CreateMsfTemplateCommand | Success | [link] Detail: CreateMsfTemplateCommand at 2026-11-10 19:17:31 SAST |
Showing 1–19 of 19
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Pieter Smit's nineteen: the template, two campaigns, ten invitations, one removal, the open, the withdrawal, the resend, the close and the release. No respondent's row: those have no institution.

######## BOARD R3-L04.dc.html
Label: Audit log · Wombat
> R3-L04
> Nolwazi Mbatha's first arrival: the last 24 hours
> NEW: the arrival window line (T21)
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
[tabindex="-1"] ‹n› actions.
Read from ‹D−1 hh:mm› SAST to ‹hh:mm› SAST: the last 24 hours, when you opened the page. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:24:10 SAST | Nolwazi Mbatha | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:10 SAST |
| 2026-11-10 20:22:59 SAST | Nolwazi Mbatha | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:59 SAST |
| 2026-11-10 20:22:29 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:29 SAST |
| 2026-11-10 20:21:44 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:44 SAST |
| 2026-11-10 20:21:43 SAST | Lerato Molefe | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:21:43 SAST |
| 2026-11-10 20:21:29 SAST | Lerato Molefe | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:29 SAST |
| 2026-11-10 20:21:20 SAST | Lerato Molefe | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:20 SAST |
| 2026-11-10 20:21:12 SAST | Pieter Smit | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:21:05 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:05 SAST |
| 2026-11-10 20:20:55 SAST | Pieter Smit | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:48 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:48 SAST |
| 2026-11-10 20:20:19 SAST | Pieter Smit | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
‹38 more rows of this page, not drawn›
Showing 1–50 of ‹n›
[button] [aria-disabled="true"] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
The window is fixed when the page loads; turning a page neither repeats nor misses a row (T277).

######## BOARD R3-L04-390.dc.html
Label: Audit log · Wombat
> R3-L04
> Nolwazi Mbatha's first arrival: the last 24 hours
> NEW: the arrival window line (T21)
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
[tabindex="-1"] ‹n› actions.
Read from ‹D−1 hh:mm› SAST to ‹hh:mm› SAST: the last 24 hours, when you opened the page. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:24:10 SAST | Nolwazi Mbatha | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:10 SAST |
| 2026-11-10 20:22:59 SAST | Nolwazi Mbatha | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:59 SAST |
| 2026-11-10 20:22:29 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:29 SAST |
| 2026-11-10 20:21:44 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:44 SAST |
| 2026-11-10 20:21:43 SAST | Lerato Molefe | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:21:43 SAST |
| 2026-11-10 20:21:29 SAST | Lerato Molefe | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:29 SAST |
| 2026-11-10 20:21:20 SAST | Lerato Molefe | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:20 SAST |
| 2026-11-10 20:21:12 SAST | Pieter Smit | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:21:05 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:05 SAST |
| 2026-11-10 20:20:55 SAST | Pieter Smit | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:48 SAST | Pieter Smit | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:48 SAST |
| 2026-11-10 20:20:19 SAST | Pieter Smit | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
‹38 more rows of this page, not drawn›
Showing 1–50 of ‹n›
[button] [aria-disabled="true"] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
The window is fixed when the page loads; turning a page neither repeats nor misses a row (T277).

######## BOARD R3-L05.dc.html
Label: Audit log · Wombat
> R3-L05
> 3.57: devadmin from 19:00 SAST
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
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action across the institutions, and those with no institution, newest first.
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
[value="2026-11-10T19:00"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
Institution
[selected] All institutions
No institution
Demo Institution
Kgosi Kgari Teaching Hospital
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 174 actions.
Read from 2026-11-10 19:00 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:24:19 SAST | Demo AdministratorNo institution | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:19 SAST |
| 2026-11-10 20:24:19 SAST | Nolwazi MbathaKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:24:19 SAST |
| 2026-11-10 20:24:10 SAST | Nolwazi MbathaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:10 SAST |
| 2026-11-10 20:22:59 SAST | Nolwazi MbathaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:59 SAST |
| 2026-11-10 20:22:29 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:29 SAST |
| 2026-11-10 20:21:44 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:44 SAST |
| 2026-11-10 20:21:43 SAST | Lerato MolefeKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:21:43 SAST |
| 2026-11-10 20:21:29 SAST | Lerato MolefeKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:29 SAST |
| 2026-11-10 20:21:20 SAST | Lerato MolefeKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:20 SAST |
| 2026-11-10 20:21:12 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:21:05 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:05 SAST |
| 2026-11-10 20:20:55 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:48 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:48 SAST |
| 2026-11-10 20:20:19 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
| 2026-11-10 20:20:16 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:16 SAST |
| 2026-11-10 19:21:43 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:21:43 SAST |
| 2026-11-10 19:20:58 SAST | Refilwe MokoenaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:58 SAST |
| 2026-11-10 19:20:57 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:57 SAST |
| 2026-11-10 19:20:49 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:49 SAST |
| 2026-11-10 19:20:46 SAST | Kabelo SitholeKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:46 SAST |
| 2026-11-10 19:20:46 SAST | Refilwe MokoenaKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:46 SAST |
| 2026-11-10 19:20:44 SAST | Refilwe MokoenaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:44 SAST |
| 2026-11-10 19:20:44 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:44 SAST |
| 2026-11-10 19:20:39 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:39 SAST |
| 2026-11-10 19:20:39 SAST | Fatima KhumaloKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:39 SAST |
| 2026-11-10 19:20:35 SAST | Fatima KhumaloKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:35 SAST |
| 2026-11-10 19:20:35 SAST | Anele DlaminiKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:35 SAST |
| 2026-11-10 19:20:31 SAST | Anele DlaminiKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:31 SAST |
| 2026-11-10 19:20:17 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:17 SAST |
| 2026-11-10 19:20:13 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:13 SAST |
| 2026-11-10 19:20:09 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:09 SAST |
| 2026-11-10 19:20:05 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:05 SAST |
| 2026-11-10 19:20:01 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:01 SAST |
| 2026-11-10 19:19:52 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:19:52 SAST |
| 2026-11-10 19:19:48 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:48 SAST |
| 2026-11-10 19:19:44 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:44 SAST |
| 2026-11-10 19:19:40 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:40 SAST |
‹13 more rows of this page, not drawn›
Showing 1–50 of 174
[button] [aria-disabled="true"] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Rows 1–37 of page 1 from 3.57-1 (UTC + 2 h), each person by the live name; the institution under the name (T19), no Institution column. devadmin's own RunScheduledJobNowCommand rows are among the rows not drawn, on this page or a later one (3.32's run, 19:15:54 SAST, is after From, so it may fall on page 1).

######## BOARD R3-L05-768.dc.html
Label: Audit log · Wombat
> R3-L05
> 3.57: devadmin from 19:00 SAST
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
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action across the institutions, and those with no institution, newest first.
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
[value="2026-11-10T19:00"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
Institution
[selected] All institutions
No institution
Demo Institution
Kgosi Kgari Teaching Hospital
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 174 actions.
Read from 2026-11-10 19:00 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:24:19 SAST | Demo AdministratorNo institution | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:19 SAST |
| 2026-11-10 20:24:19 SAST | Nolwazi MbathaKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:24:19 SAST |
| 2026-11-10 20:24:10 SAST | Nolwazi MbathaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:10 SAST |
| 2026-11-10 20:22:59 SAST | Nolwazi MbathaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:59 SAST |
| 2026-11-10 20:22:29 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:29 SAST |
| 2026-11-10 20:21:44 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:44 SAST |
| 2026-11-10 20:21:43 SAST | Lerato MolefeKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:21:43 SAST |
| 2026-11-10 20:21:29 SAST | Lerato MolefeKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:29 SAST |
| 2026-11-10 20:21:20 SAST | Lerato MolefeKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:20 SAST |
| 2026-11-10 20:21:12 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:21:05 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:05 SAST |
| 2026-11-10 20:20:55 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:48 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:48 SAST |
| 2026-11-10 20:20:19 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
| 2026-11-10 20:20:16 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:16 SAST |
| 2026-11-10 19:21:43 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:21:43 SAST |
| 2026-11-10 19:20:58 SAST | Refilwe MokoenaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:58 SAST |
| 2026-11-10 19:20:57 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:57 SAST |
| 2026-11-10 19:20:49 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:49 SAST |
| 2026-11-10 19:20:46 SAST | Kabelo SitholeKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:46 SAST |
| 2026-11-10 19:20:46 SAST | Refilwe MokoenaKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:46 SAST |
| 2026-11-10 19:20:44 SAST | Refilwe MokoenaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:44 SAST |
| 2026-11-10 19:20:44 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:44 SAST |
| 2026-11-10 19:20:39 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:39 SAST |
| 2026-11-10 19:20:39 SAST | Fatima KhumaloKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:39 SAST |
| 2026-11-10 19:20:35 SAST | Fatima KhumaloKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:35 SAST |
| 2026-11-10 19:20:35 SAST | Anele DlaminiKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:35 SAST |
| 2026-11-10 19:20:31 SAST | Anele DlaminiKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:31 SAST |
| 2026-11-10 19:20:17 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:17 SAST |
| 2026-11-10 19:20:13 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:13 SAST |
| 2026-11-10 19:20:09 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:09 SAST |
| 2026-11-10 19:20:05 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:05 SAST |
| 2026-11-10 19:20:01 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:01 SAST |
| 2026-11-10 19:19:52 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:19:52 SAST |
| 2026-11-10 19:19:48 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:48 SAST |
| 2026-11-10 19:19:44 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:44 SAST |
| 2026-11-10 19:19:40 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:40 SAST |
‹13 more rows of this page, not drawn›
Showing 1–50 of 174
[button] [aria-disabled="true"] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Rows 1–37 of page 1 from 3.57-1 (UTC + 2 h), each person by the live name; the institution under the name (T19), no Institution column. devadmin's own RunScheduledJobNowCommand rows are among the rows not drawn, on this page or a later one (3.32's run, 19:15:54 SAST, is after From, so it may fall on page 1).

######## BOARD R3-L05-390.dc.html
Label: Audit log · Wombat
> R3-L05
> 3.57: devadmin from 19:00 SAST
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
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action across the institutions, and those with no institution, newest first.
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
[value="2026-11-10T19:00"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
Institution
[selected] All institutions
No institution
Demo Institution
Kgosi Kgari Teaching Hospital
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 174 actions.
Read from 2026-11-10 19:00 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:24:19 SAST | Demo AdministratorNo institution | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:19 SAST |
| 2026-11-10 20:24:19 SAST | Nolwazi MbathaKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:24:19 SAST |
| 2026-11-10 20:24:10 SAST | Nolwazi MbathaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:10 SAST |
| 2026-11-10 20:22:59 SAST | Nolwazi MbathaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:59 SAST |
| 2026-11-10 20:22:29 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:22:29 SAST |
| 2026-11-10 20:21:44 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:44 SAST |
| 2026-11-10 20:21:43 SAST | Lerato MolefeKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 20:21:43 SAST |
| 2026-11-10 20:21:29 SAST | Lerato MolefeKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:29 SAST |
| 2026-11-10 20:21:20 SAST | Lerato MolefeKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:20 SAST |
| 2026-11-10 20:21:12 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:21:05 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:21:05 SAST |
| 2026-11-10 20:20:55 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:48 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:48 SAST |
| 2026-11-10 20:20:19 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
| 2026-11-10 20:20:16 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:20:16 SAST |
| 2026-11-10 19:21:43 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:21:43 SAST |
| 2026-11-10 19:20:58 SAST | Refilwe MokoenaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:58 SAST |
| 2026-11-10 19:20:57 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:57 SAST |
| 2026-11-10 19:20:49 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:49 SAST |
| 2026-11-10 19:20:46 SAST | Kabelo SitholeKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:46 SAST |
| 2026-11-10 19:20:46 SAST | Refilwe MokoenaKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:46 SAST |
| 2026-11-10 19:20:44 SAST | Refilwe MokoenaKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:44 SAST |
| 2026-11-10 19:20:44 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:44 SAST |
| 2026-11-10 19:20:39 SAST | Thandi ZuluKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:39 SAST |
| 2026-11-10 19:20:39 SAST | Fatima KhumaloKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:39 SAST |
| 2026-11-10 19:20:35 SAST | Fatima KhumaloKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:35 SAST |
| 2026-11-10 19:20:35 SAST | Anele DlaminiKgosi Kgari Teaching Hospital | Authentication | Logout | Success | [link] Detail: Logout at 2026-11-10 19:20:35 SAST |
| 2026-11-10 19:20:31 SAST | Anele DlaminiKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:20:31 SAST |
| 2026-11-10 19:20:17 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:17 SAST |
| 2026-11-10 19:20:13 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:13 SAST |
| 2026-11-10 19:20:09 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:09 SAST |
| 2026-11-10 19:20:05 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:05 SAST |
| 2026-11-10 19:20:01 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:01 SAST |
| 2026-11-10 19:19:52 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 19:19:52 SAST |
| 2026-11-10 19:19:48 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:48 SAST |
| 2026-11-10 19:19:44 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:44 SAST |
| 2026-11-10 19:19:40 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:40 SAST |
‹13 more rows of this page, not drawn›
Showing 1–50 of 174
[button] [aria-disabled="true"] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Rows 1–37 of page 1 from 3.57-1 (UTC + 2 h), each person by the live name; the institution under the name (T19), no Institution column. devadmin's own RunScheduledJobNowCommand rows are among the rows not drawn, on this page or a later one (3.32's run, 19:15:54 SAST, is after From, so it may fall on page 1).

######## BOARD R3-L06.dc.html
Label: Audit log · Wombat
> R3-L06
> 3.57: devadmin's "Msf"
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
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action across the institutions, and those with no institution, newest first.
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
[value="Msf"]
Part of a name: "Msf" finds the MSF commands.
Person's user id
Subject type
From (SAST)
[value="2026-11-10T19:00"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
Institution
[selected] All institutions
No institution
Demo Institution
Kgosi Kgari Teaching Hospital
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 27 actions.
Read from 2026-11-10 19:00 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:21:12 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:20:55 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:19 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
| 2026-11-10 19:20:17 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:17 SAST |
| 2026-11-10 19:20:13 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:13 SAST |
| 2026-11-10 19:20:09 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:09 SAST |
| 2026-11-10 19:20:05 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:05 SAST |
| 2026-11-10 19:20:01 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:01 SAST |
| 2026-11-10 19:19:48 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:48 SAST |
| 2026-11-10 19:19:44 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:44 SAST |
| 2026-11-10 19:19:40 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:40 SAST |
| 2026-11-10 19:18:49 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | WithdrawMsfCampaignCommand | Success | [link] Detail: WithdrawMsfCampaignCommand at 2026-11-10 19:18:49 SAST |
| 2026-11-10 19:18:40 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CreateMsfCampaignCommand | Success | [link] Detail: CreateMsfCampaignCommand at 2026-11-10 19:18:40 SAST |
| 2026-11-10 19:18:22 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | OpenMsfCampaignCommand | Success | [link] Detail: OpenMsfCampaignCommand at 2026-11-10 19:18:22 SAST |
| 2026-11-10 19:18:16 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:16 SAST |
| 2026-11-10 19:18:14 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | RemoveMsfInvitationCommand | Success | [link] Detail: RemoveMsfInvitationCommand at 2026-11-10 19:18:14 SAST |
| 2026-11-10 19:18:10 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:10 SAST |
| 2026-11-10 19:18:09 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:09 SAST |
| 2026-11-10 19:18:08 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:08 SAST |
| 2026-11-10 19:18:06 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:06 SAST |
| 2026-11-10 19:18:05 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:05 SAST |
| 2026-11-10 19:18:04 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:04 SAST |
| 2026-11-10 19:18:02 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:02 SAST |
| 2026-11-10 19:18:01 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:01 SAST |
| 2026-11-10 19:18:00 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:00 SAST |
| 2026-11-10 19:17:47 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CreateMsfCampaignCommand | Success | [link] Detail: CreateMsfCampaignCommand at 2026-11-10 19:17:47 SAST |
| 2026-11-10 19:17:31 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CreateMsfTemplateCommand | Success | [link] Detail: CreateMsfTemplateCommand at 2026-11-10 19:17:31 SAST |
Showing 1–27 of 27
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Pieter Smit's 19 at Kgosi Kgari Teaching Hospital and the 8 submissions with No institution.

######## BOARD R3-L06-390.dc.html
Label: Audit log · Wombat
> R3-L06
> 3.57: devadmin's "Msf"
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
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action across the institutions, and those with no institution, newest first.
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
[value="Msf"]
Part of a name: "Msf" finds the MSF commands.
Person's user id
Subject type
From (SAST)
[value="2026-11-10T19:00"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
Institution
[selected] All institutions
No institution
Demo Institution
Kgosi Kgari Teaching Hospital
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 27 actions.
Read from 2026-11-10 19:00 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:21:12 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | ReleaseMsfCampaignCommand | Success | [link] Detail: ReleaseMsfCampaignCommand at 2026-11-10 20:21:12 SAST |
| 2026-11-10 20:20:55 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CloseMsfCampaignCommand | Success | [link] Detail: CloseMsfCampaignCommand at 2026-11-10 20:20:55 SAST |
| 2026-11-10 20:20:19 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | ResendMsfLinksCommand | Success | [link] Detail: ResendMsfLinksCommand at 2026-11-10 20:20:19 SAST |
| 2026-11-10 19:20:17 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:17 SAST |
| 2026-11-10 19:20:13 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:13 SAST |
| 2026-11-10 19:20:09 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:09 SAST |
| 2026-11-10 19:20:05 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:05 SAST |
| 2026-11-10 19:20:01 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:01 SAST |
| 2026-11-10 19:19:48 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:48 SAST |
| 2026-11-10 19:19:44 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:44 SAST |
| 2026-11-10 19:19:40 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:40 SAST |
| 2026-11-10 19:18:49 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | WithdrawMsfCampaignCommand | Success | [link] Detail: WithdrawMsfCampaignCommand at 2026-11-10 19:18:49 SAST |
| 2026-11-10 19:18:40 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CreateMsfCampaignCommand | Success | [link] Detail: CreateMsfCampaignCommand at 2026-11-10 19:18:40 SAST |
| 2026-11-10 19:18:22 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | OpenMsfCampaignCommand | Success | [link] Detail: OpenMsfCampaignCommand at 2026-11-10 19:18:22 SAST |
| 2026-11-10 19:18:16 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:16 SAST |
| 2026-11-10 19:18:14 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | RemoveMsfInvitationCommand | Success | [link] Detail: RemoveMsfInvitationCommand at 2026-11-10 19:18:14 SAST |
| 2026-11-10 19:18:10 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:10 SAST |
| 2026-11-10 19:18:09 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:09 SAST |
| 2026-11-10 19:18:08 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:08 SAST |
| 2026-11-10 19:18:06 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:06 SAST |
| 2026-11-10 19:18:05 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:05 SAST |
| 2026-11-10 19:18:04 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:04 SAST |
| 2026-11-10 19:18:02 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:02 SAST |
| 2026-11-10 19:18:01 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:01 SAST |
| 2026-11-10 19:18:00 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | AddMsfInvitationCommand | Success | [link] Detail: AddMsfInvitationCommand at 2026-11-10 19:18:00 SAST |
| 2026-11-10 19:17:47 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CreateMsfCampaignCommand | Success | [link] Detail: CreateMsfCampaignCommand at 2026-11-10 19:17:47 SAST |
| 2026-11-10 19:17:31 SAST | Pieter SmitKgosi Kgari Teaching Hospital | Command | CreateMsfTemplateCommand | Success | [link] Detail: CreateMsfTemplateCommand at 2026-11-10 19:17:31 SAST |
Showing 1–27 of 27
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Pieter Smit's 19 at Kgosi Kgari Teaching Hospital and the 8 submissions with No institution.

######## BOARD R3-L07.dc.html
Label: Audit log · Wombat
> R3-L07
> 3.57: devadmin, Failures only
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
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action across the institutions, and those with no institution, newest first.
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
[value="2026-11-10T19:00"]
To (SAST)
Empty: up to when you press Show.
Result
All results
Success only
[selected] Failures only
Institution
[selected] All institutions
No institution
Demo Institution
Kgosi Kgari Teaching Hospital
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] ‹n› actions.
Read from 2026-11-10 19:00 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 19:02:02 SAST | Fatima KhumaloKgosi Kgari Teaching Hospital | Command | TransitionActivityCommand | FailedDecline requires a note. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:02:02 SAST |
| 2026-11-10 19:01:45 SAST | Sipho NdlovuKgosi Kgari Teaching Hospital | Command | CreateActivityCommand | FailedDate observed: The date cannot be before your programme started (2026-01-15). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:45 SAST |
| 2026-11-10 19:01:41 SAST | Sipho NdlovuKgosi Kgari Teaching Hospital | Command | CreateActivityCommand | FailedDate observed: The date cannot be after today (2026-11-10). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:41 SAST |
| 2026-11-10 19:00:06 SAST | Anele DlaminiKgosi Kgari Teaching Hospital | Command | TransitionActivityCommand | FailedPresenting problem: A value is required. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:00:06 SAST |
‹any other failed rows, from other institutions or none›
Showing 1–‹n› of ‹n›
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
3.57-0 is not attached: the four KGK failures are known, any others are not.

######## BOARD R3-L07-390.dc.html
Label: Audit log · Wombat
> R3-L07
> 3.57: devadmin, Failures only
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
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action across the institutions, and those with no institution, newest first.
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
[value="2026-11-10T19:00"]
To (SAST)
Empty: up to when you press Show.
Result
All results
Success only
[selected] Failures only
Institution
[selected] All institutions
No institution
Demo Institution
Kgosi Kgari Teaching Hospital
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] ‹n› actions.
Read from 2026-11-10 19:00 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 19:02:02 SAST | Fatima KhumaloKgosi Kgari Teaching Hospital | Command | TransitionActivityCommand | FailedDecline requires a note. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:02:02 SAST |
| 2026-11-10 19:01:45 SAST | Sipho NdlovuKgosi Kgari Teaching Hospital | Command | CreateActivityCommand | FailedDate observed: The date cannot be before your programme started (2026-01-15). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:45 SAST |
| 2026-11-10 19:01:41 SAST | Sipho NdlovuKgosi Kgari Teaching Hospital | Command | CreateActivityCommand | FailedDate observed: The date cannot be after today (2026-11-10). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:41 SAST |
| 2026-11-10 19:00:06 SAST | Anele DlaminiKgosi Kgari Teaching Hospital | Command | TransitionActivityCommand | FailedPresenting problem: A value is required. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:00:06 SAST |
‹any other failed rows, from other institutions or none›
Showing 1–‹n› of ‹n›
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
3.57-0 is not attached: the four KGK failures are known, any others are not.

######## BOARD R3-L08.dc.html
Label: Audit log · Wombat
> R3-L08
> Institution: No institution
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
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action across the institutions, and those with no institution, newest first.
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
[value="2026-11-10T19:00"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
Institution
All institutions
[selected] No institution
Demo Institution
Kgosi Kgari Teaching Hospital
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] ‹n› actions.
Read from 2026-11-10 19:00 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:24:19 SAST | Demo AdministratorNo institution | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:19 SAST |
| 2026-11-10 19:20:17 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:17 SAST |
| 2026-11-10 19:20:13 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:13 SAST |
| 2026-11-10 19:20:09 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:09 SAST |
| 2026-11-10 19:20:05 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:05 SAST |
| 2026-11-10 19:20:01 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:01 SAST |
| 2026-11-10 19:19:48 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:48 SAST |
| 2026-11-10 19:19:44 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:44 SAST |
| 2026-11-10 19:19:40 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:40 SAST |
‹devadmin's own RunScheduledJobNowCommand rows and any others with no institution›
Showing 1–‹n› of ‹n›
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
The query ignores this filter for an institution's administrator, pinned to theirs.

######## BOARD R3-L08-390.dc.html
Label: Audit log · Wombat
> R3-L08
> Institution: No institution
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
- [aria-current="page"] Audit log
### [tabindex="-1"] Audit log
Every consequential action across the institutions, and those with no institution, newest first.
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
[value="2026-11-10T19:00"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
Institution
All institutions
[selected] No institution
Demo Institution
Kgosi Kgari Teaching Hospital
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] ‹n› actions.
Read from 2026-11-10 19:00 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 20:24:19 SAST | Demo AdministratorNo institution | Authentication | Login | Success | [link] Detail: Login at 2026-11-10 20:24:19 SAST |
| 2026-11-10 19:20:17 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:17 SAST |
| 2026-11-10 19:20:13 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:13 SAST |
| 2026-11-10 19:20:09 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:09 SAST |
| 2026-11-10 19:20:05 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:05 SAST |
| 2026-11-10 19:20:01 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:20:01 SAST |
| 2026-11-10 19:19:48 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:48 SAST |
| 2026-11-10 19:19:44 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:44 SAST |
| 2026-11-10 19:19:40 SAST | MSF respondent (no account)No institution | Command | SubmitMsfResponseCommand | Success | [link] Detail: SubmitMsfResponseCommand at 2026-11-10 19:19:40 SAST |
‹devadmin's own RunScheduledJobNowCommand rows and any others with no institution›
Showing 1–‹n› of ‹n›
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
The query ignores this filter for an institution's administrator, pinned to theirs.

######## BOARD R3-L09.dc.html
Label: Audit log · Wombat
> R3-L09
> 4.38: RecordCommitteeDecisionCommand
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
[value="RecordCommitteeDecisionCommand"]
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
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 5 actions.
‹the window, as pressed›
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 ‹hh:mm:ss› SAST | Thandi Zulu | ‹category› | RecordCommitteeDecisionCommand | Success | [link] Detail: RecordCommitteeDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | Thandi Zulu | ‹category› | RecordCommitteeDecisionCommand | Success | [link] Detail: RecordCommitteeDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | Thandi Zulu | ‹category› | RecordCommitteeDecisionCommand | Success | [link] Detail: RecordCommitteeDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | Thandi Zulu | ‹category› | RecordCommitteeDecisionCommand | Success | [link] Detail: RecordCommitteeDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | Thandi Zulu | ‹category› | RecordCommitteeDecisionCommand | Success | [link] Detail: RecordCommitteeDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
Showing 1–5 of 5
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Five Success rows by Thandi Zulu, one per review; no Failed row (4.23 and 4.24 are refused on the page before anything is sent).

######## BOARD R3-L09-390.dc.html
Label: Audit log · Wombat
> R3-L09
> 4.38: RecordCommitteeDecisionCommand
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
[value="RecordCommitteeDecisionCommand"]
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
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 5 actions.
‹the window, as pressed›
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 ‹hh:mm:ss› SAST | Thandi Zulu | ‹category› | RecordCommitteeDecisionCommand | Success | [link] Detail: RecordCommitteeDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | Thandi Zulu | ‹category› | RecordCommitteeDecisionCommand | Success | [link] Detail: RecordCommitteeDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | Thandi Zulu | ‹category› | RecordCommitteeDecisionCommand | Success | [link] Detail: RecordCommitteeDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | Thandi Zulu | ‹category› | RecordCommitteeDecisionCommand | Success | [link] Detail: RecordCommitteeDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | Thandi Zulu | ‹category› | RecordCommitteeDecisionCommand | Success | [link] Detail: RecordCommitteeDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
Showing 1–5 of 5
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
Five Success rows by Thandi Zulu, one per review; no Failed row (4.23 and 4.24 are refused on the page before anything is sent).

######## BOARD R3-L10.dc.html
Label: Audit log · Wombat
> R3-L10
> 4.38: RevokeEntrustmentDecisionCommand
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
[value="RevokeEntrustmentDecisionCommand"]
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
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 1 action.
‹the window, as pressed›
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 ‹hh:mm:ss› SAST | Refilwe Mokoena | ‹category› | RevokeEntrustmentDecisionCommand | Success | [link] Detail: RevokeEntrustmentDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
Showing 1–1 of 1
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
"RevokeEntrustmentDecisionCommand" breaks inside its cell where it must (Action, .col-wrap).

######## BOARD R3-L10-390.dc.html
Label: Audit log · Wombat
> R3-L10
> 4.38: RevokeEntrustmentDecisionCommand
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
[value="RevokeEntrustmentDecisionCommand"]
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
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 1 action.
‹the window, as pressed›
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 ‹hh:mm:ss› SAST | Refilwe Mokoena | ‹category› | RevokeEntrustmentDecisionCommand | Success | [link] Detail: RevokeEntrustmentDecisionCommand at 2026-11-10 ‹hh:mm:ss› SAST |
Showing 1–1 of 1
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
"RevokeEntrustmentDecisionCommand" breaks inside its cell where it must (Action, .col-wrap).

######## BOARD R3-L11.dc.html
Label: Audit log · Wombat
> R3-L11
> Nothing matches (NoSuchCommand)
> PROPOSED (THIS ASK), NEW: the empty state’s body (K16)
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
[value="NoSuchCommand"]
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
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] No actions match these filters.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
No actions match
Nothing matches these filters between these times. Widen From and To, or clear the filters.
[button] Clear the filters
[aria-label="Board notes"]
states.md:1086. "Clear the filters" moves the focus to Category, the first filter.
The body is true of any filter (K16): here the window holds rows and Action matches none.

######## BOARD R3-L11-390.dc.html
Label: Audit log · Wombat
> R3-L11
> Nothing matches (NoSuchCommand)
> PROPOSED (THIS ASK), NEW: the empty state’s body (K16)
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
[value="NoSuchCommand"]
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
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] No actions match these filters.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
No actions match
Nothing matches these filters between these times. Widen From and To, or clear the filters.
[button] Clear the filters
[aria-label="Board notes"]
states.md:1086. "Clear the filters" moves the focus to Category, the first filter.
The body is true of any filter (K16): here the window holds rows and Action matches none.

######## BOARD R3-L12.dc.html
Label: Audit log · Wombat
> R3-L12
> Page 2 of 166
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
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 166 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 ‹hh:mm:ss› SAST | ‹name› | ‹category› | ‹action› | Success | [link] Detail: ‹action› at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | ‹name› | ‹category› | ‹action› | Success | [link] Detail: ‹action› at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | ‹name› | ‹category› | ‹action› | Success | [link] Detail: ‹action› at 2026-11-10 ‹hh:mm:ss› SAST |
‹47 more rows of this page, not drawn›
Showing 51–100 of 166
[button] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
The window unchanged: a row recorded since Show is not on page 2 (T277). The focus on h2 "Actions" (T264).

######## BOARD R3-L12-390.dc.html
Label: Audit log · Wombat
> R3-L12
> Page 2 of 166
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
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
[selected] All results
Success only
Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 166 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 ‹hh:mm:ss› SAST | ‹name› | ‹category› | ‹action› | Success | [link] Detail: ‹action› at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | ‹name› | ‹category› | ‹action› | Success | [link] Detail: ‹action› at 2026-11-10 ‹hh:mm:ss› SAST |
| 2026-11-10 ‹hh:mm:ss› SAST | ‹name› | ‹category› | ‹action› | Success | [link] Detail: ‹action› at 2026-11-10 ‹hh:mm:ss› SAST |
‹47 more rows of this page, not drawn›
Showing 51–100 of 166
[button] Previous
[button] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
The window unchanged: a row recorded since Show is not on page 2 (T277). The focus on h2 "Actions" (T264).

######## BOARD R3-L13.dc.html
Label: Audit log · Wombat
> R3-L13
> 3.56: back from an entry by the trail
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
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
All results
Success only
[selected] Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 4 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 19:02:02 SAST | Fatima Khumalo | Command | TransitionActivityCommand | FailedDecline requires a note. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:02:02 SAST |
| 2026-11-10 19:01:45 SAST | Sipho Ndlovu | Command | CreateActivityCommand | FailedDate observed: The date cannot be before your programme started (2026-01-15). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:45 SAST |
| 2026-11-10 19:01:41 SAST | Sipho Ndlovu | Command | CreateActivityCommand | FailedDate observed: The date cannot be after today (2026-11-10). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:41 SAST |
| 2026-11-10 19:00:06 SAST | Anele Dlamini | Command | TransitionActivityCommand | FailedPresenting problem: A value is required. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:00:06 SAST |
Showing 1–4 of 4
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
The trail's Audit log carries the rows' address (filters, resolved window, page): Failures only, page 1, the focus on the Detail left (T277). Today 3.56-2 drops the filters (325 rows).

######## BOARD R3-L13-390.dc.html
Label: Audit log · Wombat
> R3-L13
> 3.56: back from an entry by the trail
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
[value="2026-11-10T18:58"]
To (SAST)
Empty: up to when you press Show.
Result
All results
Success only
[selected] Failures only
[button] Show
[button] Clear filters
#### [tabindex="-1"] Actions
[tabindex="-1"] 4 actions.
Read from 2026-11-10 18:58 SAST to ‹hh:mm› SAST, when you pressed Show. Turning a page keeps this window.
Actions, newest first
| When | Person | Category | Action | Result | Detail |
| 2026-11-10 19:02:02 SAST | Fatima Khumalo | Command | TransitionActivityCommand | FailedDecline requires a note. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:02:02 SAST |
| 2026-11-10 19:01:45 SAST | Sipho Ndlovu | Command | CreateActivityCommand | FailedDate observed: The date cannot be before your programme started (2026-01-15). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:45 SAST |
| 2026-11-10 19:01:41 SAST | Sipho Ndlovu | Command | CreateActivityCommand | FailedDate observed: The date cannot be after today (2026-11-10). | [link] Detail: CreateActivityCommand at 2026-11-10 19:01:41 SAST |
| 2026-11-10 19:00:06 SAST | Anele Dlamini | Command | TransitionActivityCommand | FailedPresenting problem: A value is required. | [link] Detail: TransitionActivityCommand at 2026-11-10 19:00:06 SAST |
Showing 1–4 of 4
[button] [aria-disabled="true"] Previous
[button] [aria-disabled="true"] Next
Per page:
10
20
[selected] 50
100
[aria-label="Board notes"]
The trail's Audit log carries the rows' address (filters, resolved window, page): Failures only, page 1, the focus on the Detail left (T277). Today 3.56-2 drops the filters (325 rows).

######## BOARD R3-E02.dc.html
Label: Audit entry · Wombat
> R3-E02
> 3.57: a respondent's submission
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
SubmitMsfResponseCommand by MSF respondent (no account), 2026-11-10 19:20:17 SAST · Success
#### What happened
**When
2026-11-10 19:20:17.044 SASTStored as 2026-11-10 17:20:17.044 UTC
**Category
Command
**Action
SubmitMsfResponseCommand
**Institution
No institution
**Result
Success
#### Who
**Name
No person: an MSF respondent answers by link, with no account.
**User id
None
**Address
::/48Cut to its first 48 bits when stored.
#### What it was done to
No subject
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { "token": "[REDACTED]", "answers": "[REDACTED]" }
[aria-label="Board notes"]

######## BOARD R3-E02-390.dc.html
Label: Audit entry · Wombat
> R3-E02
> 3.57: a respondent's submission
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
SubmitMsfResponseCommand by MSF respondent (no account), 2026-11-10 19:20:17 SAST · Success
#### What happened
**When
2026-11-10 19:20:17.044 SASTStored as 2026-11-10 17:20:17.044 UTC
**Category
Command
**Action
SubmitMsfResponseCommand
**Institution
No institution
**Result
Success
#### Who
**Name
No person: an MSF respondent answers by link, with no account.
**User id
None
**Address
::/48Cut to its first 48 bits when stored.
#### What it was done to
No subject
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { "token": "[REDACTED]", "answers": "[REDACTED]" }
[aria-label="Board notes"]

######## BOARD R3-E03.dc.html
Label: Audit entry · Wombat
> R3-E03
> 3.57: David Naidoo's complete of activity 13
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
TransitionActivityCommand by David Naidoo, 2026-11-10 19:10:20 SAST · Success
#### What happened
**When
2026-11-10 19:10:20.376 SASTStored as 2026-11-10 17:10:20.376 UTC
**Category
Command
**Action
TransitionActivityCommand
**Institution
Kgosi Kgari Teaching Hospital
**Result
Success
#### Who
**Name
David Naidoo
**User id
535c41ed-ffb9-449f-bab9-709da9a0ed77
**Address
::/48Cut to its first 48 bits when stored.
#### What it was done to
No subject
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { "note": "[REDACTED]", "principal": "[PRINCIPAL]", "activityId": 13, "actorUserId": "535c41ed-ffb9-449f-bab9-709da9a0ed77", "dataPatchJson": "[REDACTED]", "transitionKey": "complete" }
[aria-label="Board notes"]
The payload in jsonb's stored key order, two-space indent, a property a line; its own region, focusable, named once, by h2 "Payload" (R2, N11); the section itself carries no name (S4).

######## BOARD R3-E03-768.dc.html
Label: Audit entry · Wombat
> R3-E03
> 3.57: David Naidoo's complete of activity 13
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
TransitionActivityCommand by David Naidoo, 2026-11-10 19:10:20 SAST · Success
#### What happened
**When
2026-11-10 19:10:20.376 SASTStored as 2026-11-10 17:10:20.376 UTC
**Category
Command
**Action
TransitionActivityCommand
**Institution
Kgosi Kgari Teaching Hospital
**Result
Success
#### Who
**Name
David Naidoo
**User id
535c41ed-ffb9-449f-bab9-709da9a0ed77
**Address
::/48Cut to its first 48 bits when stored.
#### What it was done to
No subject
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { "note": "[REDACTED]", "principal": "[PRINCIPAL]", "activityId": 13, "actorUserId": "535c41ed-ffb9-449f-bab9-709da9a0ed77", "dataPatchJson": "[REDACTED]", "transitionKey": "complete" }
[aria-label="Board notes"]
The payload in jsonb's stored key order, two-space indent, a property a line; its own region, focusable, named once, by h2 "Payload" (R2, N11); the section itself carries no name (S4).

######## BOARD R3-E03-390.dc.html
Label: Audit entry · Wombat
> R3-E03
> 3.57: David Naidoo's complete of activity 13
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
TransitionActivityCommand by David Naidoo, 2026-11-10 19:10:20 SAST · Success
#### What happened
**When
2026-11-10 19:10:20.376 SASTStored as 2026-11-10 17:10:20.376 UTC
**Category
Command
**Action
TransitionActivityCommand
**Institution
Kgosi Kgari Teaching Hospital
**Result
Success
#### Who
**Name
David Naidoo
**User id
535c41ed-ffb9-449f-bab9-709da9a0ed77
**Address
::/48Cut to its first 48 bits when stored.
#### What it was done to
No subject
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { "note": "[REDACTED]", "principal": "[PRINCIPAL]", "activityId": 13, "actorUserId": "535c41ed-ffb9-449f-bab9-709da9a0ed77", "dataPatchJson": "[REDACTED]", "transitionKey": "complete" }
[aria-label="Board notes"]
The payload in jsonb's stored key order, two-space indent, a property a line; its own region, focusable, named once, by h2 "Payload" (R2, N11); the section itself carries no name (S4).

######## BOARD R3-E06.dc.html
Label: Audit entry · Wombat
> R3-E06
> A large payload
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
‹the command's name› by Demo Administrator, ‹YYYY-MM-DD hh:mm:ss› SAST · Success
#### What happened
**When
‹YYYY-MM-DD hh:mm:ss.fff› SASTStored as ‹…› UTC
**Category
Activity type
**Action
‹the command's name›
**Institution
‹…›
**Result
Success
#### Who
**Name
Demo Administrator
**User id
deaa0728-‹…›
**Address
‹…›
#### What it was done to
**Type
‹…›
**Id
‹…›
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹a whole schema document›": { "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, } }
[aria-label="Board notes"]
Several KB of whole schema documents scroll inside the region past 28rem; the page does not (T114 item 1; bounding the size, items 2–3, is later).
The Payload section carries no name; the region is named once, by its h2 "Payload" (S4).

######## BOARD R3-E06-390.dc.html
Label: Audit entry · Wombat
> R3-E06
> A large payload
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
‹the command's name› by Demo Administrator, ‹YYYY-MM-DD hh:mm:ss› SAST · Success
#### What happened
**When
‹YYYY-MM-DD hh:mm:ss.fff› SASTStored as ‹…› UTC
**Category
Activity type
**Action
‹the command's name›
**Institution
‹…›
**Result
Success
#### Who
**Name
Demo Administrator
**User id
deaa0728-‹…›
**Address
‹…›
#### What it was done to
**Type
‹…›
**Id
‹…›
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹a whole schema document›": { "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, "‹property›": ‹value as stored›, } }
[aria-label="Board notes"]
Several KB of whole schema documents scroll inside the region past 28rem; the page does not (T114 item 1; bounding the size, items 2–3, is later).
The Payload section carries no name; the region is named once, by its h2 "Payload" (S4).

######## BOARD R3-E07.dc.html
Label: Audit entry · Wombat
> R3-E07
> An entry with a subject
> Illustration
> DATA: the subject's institution and speciality by name
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
‹action› by ‹name›, ‹YYYY-MM-DD hh:mm:ss› SAST · Success
#### What happened
**When
‹YYYY-MM-DD hh:mm:ss.fff› SASTStored as ‹…› UTC
**Category
‹category›
**Action
‹action›
**Institution
‹institution›
**Result
Success
#### Who
**Name
‹name›
**User id
‹…›
**Address
‹…›
#### What it was done to
**Type
‹…›
**Id
‹…›
**Institution
‹its institution by name›
**Speciality
‹its speciality by name›
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { ‹the payload as stored› }
[aria-label="Board notes"]

######## BOARD R3-E07-390.dc.html
Label: Audit entry · Wombat
> R3-E07
> An entry with a subject
> Illustration
> DATA: the subject's institution and speciality by name
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
‹action› by ‹name›, ‹YYYY-MM-DD hh:mm:ss› SAST · Success
#### What happened
**When
‹YYYY-MM-DD hh:mm:ss.fff› SASTStored as ‹…› UTC
**Category
‹category›
**Action
‹action›
**Institution
‹institution›
**Result
Success
#### Who
**Name
‹name›
**User id
‹…›
**Address
‹…›
#### What it was done to
**Type
‹…›
**Id
‹…›
**Institution
‹its institution by name›
**Speciality
‹its speciality by name›
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { ‹the payload as stored› }
[aria-label="Board notes"]

######## BOARD R3-E09.dc.html
Label: Audit entry · Wombat
> R3-E09
> Sipho Ndlovu's entry after the erasure
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
CreateActivityCommand by ‹the pseudonym the erasure gave›, 2026-11-10 19:01:41 SAST · Failed
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
‹the pseudonym the erasure gave›
**User id
c22b8779-798b-4229-a0b8-b73a3d16999c
**Address
::/48Cut to its first 48 bits when stored.
#### What it was done to
No subject
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { ‹the payload as stored› }
[aria-label="Board notes"]
M3 (Q15 by the right read): Who is read through DataRightsPeople.StaffNamesAsync with the erasure records of the ids read (DataRightsErasures), so an erased person reads as their pseudonym; a blank live name never falls back to the row's stored ActorDisplay, which keeps the real name. The pseudonym stays a bracket: no step gives it.

######## BOARD R3-E09-390.dc.html
Label: Audit entry · Wombat
> R3-E09
> Sipho Ndlovu's entry after the erasure
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
CreateActivityCommand by ‹the pseudonym the erasure gave›, 2026-11-10 19:01:41 SAST · Failed
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
‹the pseudonym the erasure gave›
**User id
c22b8779-798b-4229-a0b8-b73a3d16999c
**Address
::/48Cut to its first 48 bits when stored.
#### What it was done to
No subject
#### Payload
As stored, laid out one property a line: secrets read "[REDACTED]" and the signed-in person "[PRINCIPAL]".
[role="region"] [tabindex="0"] { ‹the payload as stored› }
[aria-label="Board notes"]
M3 (Q15 by the right read): Who is read through DataRightsPeople.StaffNamesAsync with the erasure records of the ids read (DataRightsErasures), so an erased person reads as their pseudonym; a blank live name never falls back to the row's stored ActorDisplay, which keeps the real name. The pseudonym stays a bracket: no step gives it.
