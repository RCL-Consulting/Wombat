# Flow 18 round 3: the check

Round 3 (canvas "Wombat 18 · Round 3", saved flat in `round-3/`: 128 boards + `f18r3.css` + `canvas.json`) was checked item by item against
`round-3-ask.txt`, 2026-11-11. The ask holds **57 items**: M1-M5 (5), K2-K20 (19), C3-C13 (11), S1-S10 (10), N1-N7 (7), Q18 (1), T1-T24
(counted as one row), A1-A3 (3). Its stylesheet block, the sweep and the boards-whole duties add rows below that are not among the 57. The
canvas's own check boards (R3-0-check, check-2) were not trusted: each claim was re-derived from the boards' markup and text, and the
figures that could be recomputed from files were.

Method. Python stdlib scripts (no network) in
`C:\Users\Renier\AppData\Local\Temp\claude\67560d182d14635f\5ae5486c-8726-4e89-ae35-c60429e28267\scratchpad\chk18` (`lib.py` the parser,
`chk.py` the per-board summary, `twins.py`, `r2diff.py`/`r2d2-4.py` the R2 to R3 diffs, `main.py`/`doc.py` a board's outline). Over all 128 files:
the inventory against the ask's list and against round 2's widths; head links, `h1`, `<style>` bodies, inline `style=`, `required`/`maxlength`,
raw hex, brackets as control values, ids with spaces or brackets, duplicate ids, every `for`/`aria-*` reference, empty `aria-describedby`, the mark,
gendered words, and every class against `app.css` + `design/system/components/bundle.css` + `f18r3.css`; each 390 and 768 twin against its 1280
board (markup and words, `$preview` size and `<title>` removed); each R3 board against its R2 file as a token diff (so a change is told from a
regression); the dialogs, pagers, footers, row headers, Detail names, count lines, landmark names, S6 stars and chips; `f18r3.css` against
`f18r2.css`; every PART 2 rule against the Spec's quote; the Steps and sweep arithmetic; every contrast figure recomputed from `app.css` tokens;
and every line citation into `src/`, `tests/`, `app.css`, `DESIGN.md` and the runbook opened.

**Not rendered, not measured.** This check rendered no board (the boards link `ds/wombat/…`, which `round-3/` does not hold; no browser run).
Every pixel figure (the home cards' track counts and 28/44 px buttons, the pager's left/top, the dialogs' 448 x 280.6 and 256.6, the 366
renders' scrollWidth, the 657 controls under 44 px, the payload region's 480/1250, the columns' widths and line counts, the "rendered" check
of S7) is the canvas's own and is marked "reported" below. Where an independent route existed it was taken and is said.

## Verdict

**57 rows: done 55, partly 2, not done 0.** The two partly rows are **S2** (Spec § 12 and the check's own measurement say the health list is narrow
from 1100 to 1112 px and wide from 1113; the R3-H01 notes, on all three widths, still say "narrow layout until 1115px") and **C5** (several NEW
sentences are quoted with their kind and status but not "beside what it replaces", and do not say they replace nothing). No board the ask lists is
missing and no unlisted board was drawn. No regression of a round-2 must. Three line citations are stale (below). Round 3 is fit to build from
once the S2 note and the C5 cells are fixed.

## Inventory

| What | Ask | Found |
|---|---|---|
| Redrawn at 1280 | 56 | 56 (H01-H11, I07-I16, I22, J01-J10, R01, R02, R04, R06, R07, R09, L01-L13, E02, E03, E06, E07, E09) |
| Their 390 twins, plus R3-R05 at 390 alone | 56 + 1 | 57 |
| Redrawn at 768 | 7 | 7 (H01, I12, J01, R01, L01, L05, E03) |
| New (R3-H12, 1280 and 390) | 2 | 2 |
| Check, Spec, Steps | about 6 | 6 (R3-0-check, -check-2, -spec, -spec-2, -spec-3, R3-0-steps) |
| Total boards | about 128 | 128; no missing, no extra, by name; `canvas.json` lists exactly these 128 |
| Widths | as round 2 drew each | for every base the set of widths equals round 2's (H12 against R2-H03: 1280 and 390) |
| Standing on "Wombat 18" | 24 boards, 50 screens | 25 ids (the ask's own list), 50 screens counted in `round-2/`; the Spec says "the message counts 24 ids", which is right |
| `f18r3.css`, `canvas.json` | project file | both present |

Over all 128: every head links `ds/wombat/tokens.css`, `components/bundle.css`, `f18r3.css`, in that order (0 exceptions, no `/_blob/` stylesheet);
every `<style>` is `body{margin:0}`; no inline `style=` (0); one `h1` on every page board; no `required` attribute and no `maxlength` (0); no raw
hex; no duplicate id and no id holding a space or bracket; **0 unresolved** `for`/`aria-labelledby`/`aria-describedby`/`aria-controls`; **0**
`aria-describedby=""`; no bracket as a control's value; the brand mark is `/_blob/c6a099e64c4d721c4870011edee399ca` on all 122 page boards and
`b3b9ce…` (round 2's) on none; no gendered pronoun in any file or canvas title (0); no `Prof Mbatha` on a page board (the four sheets quote it only
where they say it is kept or banned). Every class used exists in `app.css`, `bundle.css` or `f18r3.css` (129 classes, 0 unknown). **All 64
twins are identical to their 1280 board** (A2; R3-R05-390 has none). No `button`/`a` carries `aria-label`; in 711 elements with a visually hidden
part the visible words come first (A3). `.dialog-form`, `.dialog-actions`, `.institutions-page`, `.status-dot` appear on no board.

## Must fixes (M1-M5)

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| M1 | Done | R3-I13 and R3-I15 (+390): `dialog.dialog-card.dialog-scrim.cv-dialog` with `aria-labelledby="dlg-t"` and `aria-describedby="dlg-b"`; `div.form-container > h2, p, div.form-actions > button.btn.btn-outline[autofocus]` "Cancel" then "Deactivate" `btn-danger` / "Reactivate" `btn-primary`. Matches `ConfirmDialog.razor:52-80` (action mode: `form-container`, `FormActions`, Cancel `autofocus`, then the action). Words unchanged from R2 (token diff). Spec § 12 quotes `.dialog-card .form-actions` and `app.css:6375` (opened: the 640.98 px block, `column-reverse`). | Card sizes (448 x 280.6 / 256.6; 358 at 390; action above Cancel, 44 px) are the canvas's render. |
| M2 | Done | All 40 pagers (R01, R02, R04, R05-390, R06, R07, R09, L01-L10, L12, L13 at every width) have one signature: `div.pager > div.pager-info, div.pager-actions (Previous, Next: btn btn-outline btn-sm, aria-disabled="true" at an end, none disabled), div.pager-page-size (label.pager-page-size-label for="per-page", select.form-select.form-select-sm.pager-page-size-select 10/20/50/100, 50 selected)`; 0 `span`. Same order and markup as `PagerControls.razor:23-37`. R3-L11 has no pager (empty). Spec § 13 § Pager says the order. | Left/top positions and the Tab order are the canvas's render (the Tab order is the DOM's, which is as drawn). |
| M3 | Done | Spec-2 § 7 "DataRightsPeople (M3)" and § 17: names through `StaffNamesAsync` (`DataRightsPeople.cs:69` is exactly that signature, `internal` to Application), the erasure records, an erased person by pseudonym, a blank live name never falling back to `ActorDisplay`, the stored display only with no user id; round 2's S9 "existing read" moved to DATA. R3-E09: Who is `‹the pseudonym the erasure gave›`, note names the read. | None. |
| M4 | Done | Steps A.5.5 row: Expect rewritten to "Institution unavailable ... Go to Home; never 'You cannot open this page'", Actual (`:1009-1010`) holds, board R2-I20 (standing); Changed line "14 of the 23 (...), and 1.23 and A.5.5"; "Page not found" search row "1.23; A.5.5". | None. |
| M5 | Done | Steps A.2.10: starts "On Scheduled jobs (A.2.9's page) press Run history"; 4 + 5 at both widths; recount row says why. | None. |

## K items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| K2 | Done | Spec-2 § 7 `IUserAdministrationService (K2)` row: `CountAccountsAsync`, Infrastructure, accounts no erasure record names and the locked among them, an erased account in neither (`UserDeactivation.cs:3-13` opened: lockout rules), handler stops looping `WombatRoles.All` (`GetAdministratorDashboardSummaryQuery.cs:44-52` is that loop), no Identity type. § 17 repeats. | None. |
| K3 | Done | § 7 `EmailOutcome` row: (a) 24 hours and last sent from the table; (b) written at `EmailWorker.cs:150` (Delivered) and `:160` (Dropped), not through `ReportAsync`, which returns early without a `DeliveryKey` (`:195-198`); (c) `AbandonAsync` (`:104`) and the backoff-wait return (`:179`) dropped with no refusal, PROPOSED; refusal through `EmailLog.Redact`, 2,000 characters; (d) R3-H12; (e) queue in-process. | None. |
| K4 | Done | § 7 ScheduledJobHost row: `IsDue` extracted (`ScheduledJobHost.cs:110-119`, opened), Running neither failing nor overdue (lock `:80-85`), unparsable cron sentence in § 9 "Health line, NEW" (badge Failing). R3-H07's note says Running is not overdue. | None. |
| K5 | Done | § 7: next occurrence after the last run's start; "At the next check, within 30 seconds" and "Enabled ‹key›. It runs at the next check, within 30 seconds." (§ 9, NEW, words only); R3-J03 note "True as drawn (K5)". `GetScheduledJobStatusQueryHandler.cs:38-49` is the after-now read the row names. | None. |
| K6 | Done | § 7 `IScheduledJobDispatcher` row: `DispatchNowAsync` returns the run (`IScheduledJobDispatcher.cs:5` returns `Task`), `ScheduledJobRefusal` for both refusals (`ScheduledJobDispatcher.cs:66-68` is `AlreadyRunning`), T23's words for anything else, `ex.Message` at `ScheduledJobsList.razor:149`/`:179`, `"unknown"` at `:171`; `TriggeredBy` the caller. R3-J04, J06, J07, J09, J10 changed only by the Q18 chip/description. | None. |
| K7 | Done | § 7 "The time helper": Web, replacing `ErrorPages.cs:51, :136` and `ActivityListWords.cs:13, :94` (all four lines hold the fixed +2 h offset), SA day and UTC range (`GetScheduledJobRunsQueryHandler.cs:33, :39` are the UTC-day lines), filters read as SAST (`AuditList.razor:185-186` tag UTC), cron fallback "cron ‹expr›, UTC". | None. |
| K8 | Done | § 7 "The audit DTO (K8)" and § 17: `ActorKind`, `SubmitMsfResponseCommand` with no user id is `MsfRespondent`, "No signed-in person" for every other row, a failed sign-in included (`AuditPipelineBehavior.cs:148-156` is `ActorOf` / `IAnonymousAuditedCommand`). | None. |
| K9 | Done | § 9 "Address": 48-bit (round 1's) and 24-bit (NEW, words only), `HttpAuditContextProvider.cs:62-83` is `TruncateIp` (/24 and /48); § 16 "An IPv4 address". | None. |
| K10 | Done | § 7 `AuditPipelineBehavior`: stores a refusal's words, else "The action failed.", text to the log (`:104` area stores `ex.Message` today); § 9 "Audit row text, NEW"; § 16. The three inferred refusals stay inferred. | None. |
| K11 | Done | § 7 and § 17: per-part DTO; § 16 "The database down" adds the circuit-outlives-outage sentence; R3-H08 note repeats it. | The cite `DependencyInjection.cs:4-6` is stale (see Stale citations). |
| K12 | Done | § 7 `InstitutionEdit` row: "InstitutionEdit.razor:246-250" is the `/not-found` call, ":102-105" the Status read-out for a non-Administrator (both opened and right). | None. |
| K13 | Done | § 7 `InstitutionCreate` row: `InstitutionCreated(id)`, the record reads the token once and strips it (`WithoutTokens`). | `CatalogueLinks.cs:38` is the `CreatedToken` constant; `:83-103` does hold `CollegeCreated` and `TreeCreated`. The cite is loose, not wrong. |
| K14 | Done | R3-I07-I16, I22 (+390): no `maxlength`, no `required` (script). § 9 "Refusal" lists "Keep the name to 200 characters. The record was not saved." and the short-code one, "built; words only". | None. |
| K15 | Done | § 7 `ListAuditEntriesQuery`: three-kind filter (all, none, one id), institution name a lookup, Category labels a map. | The ask's examples (Msf to "MSF", DataRights to "Data rights", ActivityType to "Activity type", nine labels) are not restated. |
| K16 | Done | R3-L11 (+390): empty body "Nothing matches these filters between these times. Widen From and To, or clear the filters." with the PROPOSED, NEW chip and a note that it is true of any filter; Steps `states.md` 1086 row gives it. | None. |
| K17 | Done | § 16 "Deactivate… or Reactivate… pressed with unsaved typing"; Steps K17 line; words only. | None. |
| K18 | Done | § 9 Subtitle row: "kept for 90 days." kept, with the reason. | None. |
| K19 | Done | § 9 "Count line": "21 runs." / "1 run."; "166 actions." / "1 action." R3-L10 draws "1 action." | None. |
| K20 | Done | § 7 `EmailOutcome` row ("Written by whichever host sends ... so Home reads both"), § 15 ("The Api host's mail now reads on Home through the table"); the "Web host's only" is gone. | The ask cites `DependencyInjection.cs:16-25`; that file's lines 16-25 are `using` lines (the SmtpHost branch is `Wombat.Infrastructure/DependencyInjection.cs:100`). The Spec repeats the stale cite. |

## C items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| C3 | Done | Steps search row "dispatched" names A.2.4's Expect (`appendix-cross-cutting.md:572`) and Actual (`:577`), A.2.6 `:607`, A.2.8 `:649`, A.2.9 `:670`, 3.32 `act-3-operations.md:840`, `states.md:1095`; all six lines opened and hold "dispatched"; the ":577 is A.2.4's" correction made. | None. |
| C4 | Done | Search rows: "When (UTC)" 3.56 `:1582`; "From (UTC)" 3.55 `:1551, :1562`, 3.57 `:1591`; "Display name" `:1583`, `:1600`; "Edit institution" 1.6 `:215, :225`, A.6.1 `:1235`, 1.23 `:711`; "Users across institutions" 1.1 `:58`, 1.11 `:347`, A.6.2 `:1245`; "registered users" `:345`, `:1251`; new "Back to institutions" `:1236`. All 21 lines opened and hold their words. | None. |
| C5 | **Partly** | § 9 "Messages by kind" quotes every group the ask lists (subtitles by role, the eight descriptions, schedule words, health lines whole, "Not read…", "Three jobs need you:", field helps, both Status sentences, Accounts lines, count lines, empty states, window lines, filter labels and helps, the three "Go to", "Stored as ‹…› UTC") with a Kind and a Status column. Description (Q18) and K16's body say what they replace. | K3's drop-with-no-refusal, K4's cron sentence, K5's two, K9's 24-bit sentence, K10's "The action failed.", K19's singulars and Q18's empty form are NEW but carry no "replaces ..." cell and do not say "replaces nothing". Spec-1 § 6 says "each quoted in § 9 beside what it replaces". |
| C6 | Done | Spec § 4: one rule, loading and load errors carry none, the 28 listed. Chips: 25 R3 boards carry the Illustration chip and canvas title (H03-H10, H12, I09, I10, I22, J05-J10, R06, R07, R09, L08, E06, E07, E09), exactly the ask's list, and the 3 standing add to 28; no board outside the list carries one; no chip/title mismatch. H11 (loading) has none. | None. |
| C7 | Done | Every Detail on R3-L01-L10, L12, L13 at all widths ends "SAST": 323 of 323 (re-counted: 36+8+38+24+111+54+8+18+10+2+6+8). R3-L01 note quotes "Detail: Login at 2026-11-10 20:24:10 SAST"; § 9 N12 restated with the zone. | None. |
| C8 | Done | Steps recount table: today's A.2.10 "the key, the key, the key and Status, Status and From" = 1+1+2+2 = 6, 5 presses, 5 + 6 holds. | None. |
| C9 | Done | Spec-1 round 2's corrections table: S8 "§ 10 (C9)". | None. |
| C10 | Done | Steps 1.1: "1.1's 9 is drawn on R3-H02; 1.11's 11 is words only"; R3-H02 reads 9. | None. |
| C11 | Done | R3-L05 (+390, +768) note: "devadmin's own RunScheduledJobNowCommand rows are among the rows not drawn, on this page or a later one (3.32's run, 19:15:54 SAST, is after From, so it may fall on page 1)". | None. |
| C12 | Done | R3-L01 chip "3.55: Nolwazi Mbatha from 18:58 SAST", R3-L01 and L04 notes; no `Prof` on any page board; Spec § 1 says R2-E01, E08, I20, I21 keep theirs; Steps keeps the runbook's quotation. | None. |
| C13 | Done | Spec-2 § 8: "Seen in the rows ... Kabelo Sithole, Kgosi Kgari Teaching Hospital, a Login at 2026-11-10 19:20:46 SAST on R3-L05". | None. |

## S items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| S1 | Done | All 11 boards with cards plus H12 (+390, H01-768): each card `section.detail-card[aria-labelledby]` > `h2.dashboard-card-title` (icon + `<span>`) > `div.dashboard-card-body`; links in `div.dashboard-card-footer` as `a.btn.btn-sm.btn-outline`. System health: "Its runs" (hidden ": entrustment-decision-expiry, failed") then "Scheduled jobs" on H05 and H09; "Scheduled jobs" alone elsewhere; none on H08 (Not read). Accounts "Users". No link in any health sentence. H11: each card is h2 + body with `.dashboard-card-skeleton[aria-hidden]` (2 skeletons), `aria-busy`, no footer. PROPOSED chip on H01-H10 and H12 only. | H08's Accounts card reads Not read and has no "Users" footer (the Spec says "No footer on a card that reads Not read"); the ask's "Accounts' footer: Users" does not except it. Footer heights are the canvas's. |
| S2 | **Partly** | R3-H01-768 keeps one column (R2 drew it so); Spec § 12 and § 3 say one column at 900 and below (`app.css:2283-2290`, opened: `max-width: 900px`, one track, spans reset), two tracks 901-1099 (`app.css:1822-1828`, 1099.98), three from 1100; the round-2 "2-column grid with System health spanning both" is gone. Spec § 12 also says the list is narrow from 1100 to 1112 and wide from 1113 (480.0 px), "not 1115 as the message estimated", which agrees with the check board's per-pixel table. | The note on R3-H01, R3-H01-390 and R3-H01-768 still reads "from 1100 to 1114px the health list is under 30rem ... narrow layout until 1115px (S2)", which the Spec and the measurement contradict. Spec § 13's Dashboard-page row gives the tracks but not the narrow band, though the ask says the DESIGN.md row "says the same". |
| S3 | Done | `f18r3.css` R2 is `.code-block--region { max-height: 28rem; overflow: auto; }` only; the `:focus-visible` block is gone and the comment says the ring is the page's. `app.css:512` is the global `:focus-visible` ring. Spec § 9 R2 row says so. | None. |
| S4 | Done | R3-E02, E03 (+768), E06, E07, E09: the Payload `section.user-section` has no `aria-labelledby`; `pre.code-block.code-block--region[tabindex=0][role=region]` has `aria-labelledby` the h2 "Payload". Landmarks in order on each: What happened, Who, What it was done to, Payload (no name twice). Spec-1 § 6 lists it with no chip; notes on E03 and E06 say it. | None. |
| S5 | Done | Spec-2 § 7 "Retiring the status dots": `app.css` `.status-dot` + `.ok/.warn/.err` (opened `:2110-2131`), used only by `AdministratorDashboard.razor:16-22` (opened: three dots), `ContrastTests.cs:319` (opened: resolves `.status-dot.{state}`), `DESIGN.md:324` (opened: "Status dots on surface …"), `DefinedClassTests` checked. § 13 and § 14 repeat. | None. |
| S6 | Done | R3-I07-I16, I22: Name and Short code labels "Name * required", `.required-mark[aria-hidden=true]`, hidden "required" (1 each), controls `aria-required="true"`, no `required`; "* marks a field you must fill in." on every one. | None. |
| S7 | Done | `th scope="row"` replaces the first `td` on every tbody row of R3-J01-J10 (8 rows each) and R3-R01 (21), R02 (2), R04 (21), R05-390 (22), R06 (3), R07 (2), R09 (1) (td-to-th pairs counted in the diff); on Scheduled jobs the th holds `.person-cell` (key + description), on Run history the key alone. Audit rows keep none (L01 has 0 row headers). `.jobs-page` and `.job-runs-page` do sit on `div.people-page` (R3-J01, R3-R01). Spec § 9 says so and that it was checked by rendering. | The two rules it cites (`app.css:6799`, `:6847`) are word-break rules for `.person-cell` content inside a row header (`.group-id`, `span`), not the rules that paint a row header; on R3-R01 the th holds no `.person-cell`. The "painted by" claim and the rendering are not verified here. |
| S8 | Done | Spec-3 § 10: "S8, decided. The 1150 px stack is kept ... as a readability choice, not as an overflow fix", with the natural-overflow widths (843, 855, 880, 886, 907, 945) and the cost avoided (Institutions' Contact email over three lines at 1151; a Scheduled jobs description over four). The ask's 11 widths are all tabled for J01, J08, R01, R07, L01, L05; the container = viewport less 308 re-derived (592, 642, 692, 716, 717, 742, 792, 842, 843, 892, 972), each spare re-subtracted, and each overflow viewport = min-content + 308 less 1 (536 to 843, 548 to 855, 573 to 880, 579 to 886, 600 to 907, 638 to 945: all consistent). | The min-content widths and line counts themselves are the canvas's render. The columns table shows a 4-line Schedule cell and 3-line Job/Person cells; the Spec explains none sits beside an empty band. |
| S9 | Done | `f18r3.css` C1 is a stand-in under a comment; counts **7 + 7** re-derived from `app.css`: the 1150 px block (7708-7769) holds 7 rule lists (lines 7711, 7725, 7734, 7743, 7751, 7757, 7766), the 1024 px block (7633-7702) holds 7 (7637, 7652, 7662, 7673, 7682, 7689, 7699); `bundle.css` 7551/7626 are the same blocks 82 lines earlier. The stand-in has 7 rules per block. No record board carries `.institutions-page` (0); Spec § 9 says the build puts it on the list alone; the alternative (DataTable `Class`) recorded, not taken. | None. |
| S10 | Done | Spec-2 § 7 DataTable row and § 9 row: shadow and the stacked roles, boards draw neither, "a 1 px shadow, not a defect". | None. |

## N items

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| N1 | Done | `h2.list-section-title[tabindex=-1]` ("Runs", "Actions") on all 14 R and 13 L base boards at every width (42 of each); no other focus-target h2. Spec-1 § 1 names the superseded standing R03, R08, R10, R11, L14, L15. | None. |
| N2 | Done | The count line is `p[tabindex=-1]` with no `role="status"` on every R/L board including L11; the only `role="status"` on the redrawn boards are the Alert results on I11, I12, I14, I16, J02-J04. Spec-1 § 6 lists it as markup-only, no chip. | None. |
| N3 | Done | Spec-3 § 11: Disabled and Not configured stay `.badge-accepted`; the `.badge-draft` preference recorded, not taken. | None. |
| N4 | Done | Spec-3 § 14: success 5.23, warning and overdue 5.43, danger 5.56 (beside 5.88, 5.77, 5.95); `--input-border` on `--header-bg` 3.42 (3.80). Recomputed from `app.css` tokens: 5.229, 5.427, 5.565; 5.882, 5.766, 5.953; 3.416, 3.799. `DESIGN.md:322` carries the same. | None. |
| N5 | Done | § 14: muted on success tint 4.52 (0.02 to spare); recomputed 4.5216. `ContrastTests.cs:88` includes `--success-bg` in the muted-text tints. | None. |
| N6 | Done | 0 brackets in an id, `for` or `aria-*` on all 128; R3-I10 had three brackets as input values in round 2; now empty with a note naming them (A3). | None. |
| N7 | Done | § 9 "The hidden colon (N7)" row. The health name's hidden ":" is on every health line. | None. |

## Q18 and T1-T24

| Item | Verdict | Evidence | What remains |
|---|---|---|---|
| Q18 | Done | R3-H03 note ("read from the table"), H04 and H09 notes (refusal as stored, redacted), H08 (Mail Not configured, rule stated incl. the Not read form), **R3-H12** (+390; Illustration; Mail OK, "Nothing sent since ‹YYYY-MM-DD hh:mm› SAST; none waiting; none dropped in the last 24 hours."; jobs OK "All 8 jobs last succeeded."; Accounts 24, "None is locked."; footers; the note: no mail sent is not a state). Retention description "Deletes job run records and mail outcomes older than 90 days." on J01-J10 with PROPOSED chip. Spec has the DATA row (§ 17), the code rows (§ 7: entity, migration + Designer, writes, read, `IEmailHealth`, retention job, tests), § 16 edge rows, § 6, § 8. Steps: "Q18 adds no step". | None. |
| T1-T24 | Done | Spec-1 § 2: heading "Decided 2026-11-11", each T with its boards and a "Changed in round 3" cell (T5 by K5, T13 by Q18 and K3, T15 by Q18, T20 by K8, T6 by S8/S9, T4 by S4, T2 by C7); alternatives recorded as not taken (T1, T3, T4, T5). The PROPOSED chips for T items are gone from R3-E02, J09 and L08 (token diff shows each removed); the full PROPOSED-chip inventory is S1, Q18 (since, description) and K16 only. NEW chips (I15, J09, J10, L04) stay as the ask says. | None. |

## A items and the sweep

| Item | Verdict | Evidence |
|---|---|---|
| A1 | Done | Steps cites R3 ids for every redrawn board (1.1 R3-H02; 1.6 R2-I01, R3-I07, I08, I11, R2-I02; 1.11; 3.32; 3.55; 3.56 R2-E01, R3-L13; 3.57 R3-L05, E02, E03; 4.38 R3-L09, L10, R2-E04, E05; A.2.1; A.2.2; A.2.4; A.2.10 R3-R01, R02, R2-R03, R3-R04; A.6.1 R2-I02, R3-I12; A.6.2; A.7.11 R2-I03, R3-R05, J01-390, H01-390) as the ask lists; standing ids keep R2. No redrawn board is cited as R2- in any note. |
| A2 | Done | 64 of 64 twins identical (above). |
| A3 | Done | Heads, h1, styles, classes, ids, references, no empty describedby, names: 0 exceptions (above). |
| Sweep tables | Reported | Home cards (R3-H01, H09 at 9 widths), pager (R01, L01 at 3), dialogs (4 rows), lists (6 boards x 11 widths + overflow widths + columns), payload (R3-E06 480/1250; landmarks), 44 px (657 controls), sideways (366 renders): all present on check-2 and Spec-3 and shaped as the ask asks. Re-derived here: the lists' arithmetic and the 7 + 7 lists; the H01 per-pixel table is internally consistent (471.3 + 0.667 per px, 480.0 at 1113). The 44 px count by markup: 112 `.form-control` and 63 `.form-select` on the 58 390-boards agree with the canvas, `btn` 494 less one Sign out per board = 436 agrees, and 46 breadcrumb links are the parent crumbs (the other 23 breadcrumb links are the `breadcrumb-ancestor` crumbs that `app.css:3416` hides at 390). Nothing measured by rendering was reproduced. |

## The stylesheet

`f18r3.css` against `f18r2.css`: identical except the header comment, the R1 label, the R2 comment, **R2's `.code-block--region:focus-visible` block removed (S3)**, and the C1 comment (stand-in, 7 + 7 lists). PART 1 (`.cv-*`) is byte-identical. PART 2 has **12 top-level rules; each is quoted verbatim in Spec-2 § 9** (checked by whitespace-normalised text). No new rule, no raw colour, no new token; no board relies on a rule that is not shipped or in the file. Class table: 48 of the 129 classes used are not named in § 9 as written; all are shell classes (`nav-*`, `brand-*`, `phone-role*`, `role-*`, `sign-out*`, `breadcrumb-*`), badge tints and button modifiers covered by "`.badge` and its tints", "`.nav-*`", "the shell" or "`.breadcrumbs`", and `.cv-chip--*` modifiers covered by "`.cv-chip(--illus, --new, --proposed, --data)`" (a nit, no action).

## Steps arithmetic

Counting rule restated once at the top of R3-0-steps, as the ask asks; the board is restated whole (no "as round 2's row"; the 24 rows are 23 steps plus A.5.5, then 1.23). Diff against R2-0-steps: only the items (R3 ids, M4, M5, C3, C4, C8, C10, K16, K17, Q18, Q18's description in A.2.1, the states.md line 141 and 1086, the search table).

| Journey | Rule applied | Board | Verdict |
|---|---|---|---|
| A.2.10 | presses Run history + Show x3 = 4; fields Job (1), Job and Status (2), Status and From (2) = 5; no menu at 390 (Run history is a header action on Scheduled jobs) | 4 + 5 at 1280 and 390 | Correct (M5); today 5 presses (Run history, Filter x4) + 1+1+2+2 = 6 fields = 5 + 6 as C8 says |
| 1.6 | Institutions, Create, Save, Save, the trail = 5 presses (+1 menu at 390 = 6); 3 fields | 5 + 3 (390: 6 + 3) | Correct |
| A.6.1 | Institutions, Edit, Save = 3 (390: 4); email = 1 | 3 + 1 (390: 4 + 1) | Correct |
| 3.55 | Audit log, Show x3 = 4 (390: 5); From, Result, Result and Action = 1+1+2 = 4 | 4 + 4 (390: 5 + 4) | Correct |
| Deactivate and reactivate | 4 confirms/presses, 0 fields | 4 + 0 | Correct |
| A.2.2 | Disable, Run now, Enable | 3 + 0 | Correct |
| Failed job found and run | Home footer "Scheduled jobs", Run now; no menu at 390 (Home is the landing page); today +1 at 390 | 2 + 0 / 2 + 0; today 2 + 0 / 3 + 0 | Correct; the footer button is the same one press |
| 3.56 and back; own account | Detail, the trail = 2; Users, Show, the row = 3 + "devadmin" 1 (390: 4) | 2 + 0; 3 + 1 / 4 + 1 | Correct |

"14 of the 23" changed = 1.1, 1.6, 1.11, 3.55, 3.56, 3.57, 4.38, A.2.1, A.2.2, A.2.4, A.2.10, A.6.1, A.6.2, A.7.11 (14 counted); unchanged 9 = 3.32, 6.38, A.2.6, A.2.8, A.2.9, A.5.2, A.5.13, A.6.9, A.6.10 (9 counted); 14 + 9 = 23; 1.23 and A.5.5 are the extra two. "One recount differs from round 2: A.2.10 at 390" is true (the only changed figure in the journeys table).

## Regressions

| Round-2 must | Result |
|---|---|
| ConfirmDialog action-mode markup | Holds, now on `form-container` / `form-actions` (M1); no form-mode class left. |
| Pager order | `info, actions, page-size`, no span, on all 40 pagers (M2). |
| Q15's pseudonym | R3-E09 Who is still the bracket `‹the pseudonym the erasure gave›`; note now names the read (M3). |
| A.5.5 | Rewritten as M4 asks; board R2-I20 stands; Changed line and search row updated. |
| A.2.10's count | 4 + 5 at both widths (M5); today's 5 + 6 unchanged. |
| Anything outside an item | The R2 to R3 token diff of all 57 base boards and R3-R05-390 shows only: brand-mark and id strings, S1's cards/footers and chip, Q18's notes/description/H12 words, S6/K14 attributes and the hidden "required", I10's three bracket values emptied (A3/N6), the dialogs' wrappers (M1), pager markup (M2), h2/count-line attributes (N1/N2), SAST on Detail names (C7), Nolwazi Mbatha (C12), C11's note, `th scope="row"` (S7), K16's body, S4's section, M3's note, removed T chips. No word lost or changed outside an item. |

## Stale citations

Of the line citations opened (about 80 across `src/`, `tests/`, `app.css`, `DESIGN.md` and the runbook), all hold except these:

1. `DependencyInjection.cs:4-6` (the security stamp revalidated, T279; Spec-3 § 16, R3-H08 and its 390 twin) points at `using` lines of `Wombat.Infrastructure/DependencyInjection.cs`. The registration is `DependencyInjection.cs:90` (`SessionRevalidation.Configure`) and `SessionRevalidation.cs:46-49`. Carried from the ask.
2. `DependencyInjection.cs:16-25` (both hosts start the mail worker when `SmtpHost` is set; Spec-2 § 7) points at `using` lines. The `SmtpHost` read is `Wombat.Infrastructure/DependencyInjection.cs:100`. Carried from the ask.
3. `CatalogueLinks.cs:38` (Spec-2 § 7, `InstitutionCreate`) is the `CreatedToken` constant, not `CollegeCreated`; `CollegeCreated` is `:84` and `TreeCreated` `:103` (inside the cited `:83-103`).
4. `app.css:6799` and `:6847` (Spec-2 § 9) are `overflow-wrap` rules for `.person-cell` content in a row header, cited as what "paints" the row header (see S7).

Held and re-checked: `app.css:512`, `:1822` and `:2283` (grid), `:2111-2131`, `:2916`, `:6375`, `:6925`, `:7633-7702`, `:7708-7769`; `DESIGN.md:322-324`; `ConfirmDialog.razor:52-80`; `AssessorDashboard.razor:39-41`; `PagerControls.razor:23-37`; `ContrastTests.cs:88, :319`; `DataRightsPeople.cs:69`; `ErasureExecutor.cs:440-452`; `UserDeactivation.cs:3-13`; `EmailWorker.cs:104, :150, :160, :179, :195-198`; `ScheduledJobHost.cs:80-85, :110-119, :212`; `ScheduledJobDispatcher.cs:66-68`; `ScheduledJobsList.razor:64, :79-84, :149, :171, :179`; `GetAdministratorDashboardSummaryQuery.cs:44-52`; `GetScheduledJobRunsQueryHandler.cs:33, :39`; `GetScheduledJobStatusQueryHandler.cs:38-49`; `HttpAuditContextProvider.cs:62-83`; `IScheduledJobDispatcher.cs:5`; `InstitutionEdit.razor:102-105, :246-250`; `AuditList.razor:185-186`; `AuditPipelineBehavior.cs:148-156`; `ErrorPages.cs:51, :136`; `ActivityListWords.cs:13, :94`; `AdministratorDashboard.razor:16-22`; and every runbook line in the Steps search table (`act-1-setup.md` 58, 215, 225, 345, 347, 709, 711, 722; `act-3-operations.md` 840, 1551, 1562, 1582, 1583, 1591, 1600; `appendix-cross-cutting.md` 572, 577, 607, 649, 670, 998-1011, 1235, 1236, 1245, 1251; `states.md` 141, 888, 889, 898, 899, 1086, 1091, 1093, 1095, 1099, 1100).

## Could not check

- Everything the canvas says it rendered: track counts, footer heights, pager coordinates and Tab order, dialog sizes, the 366 scrollWidths, the 657 controls at 44 px, the payload region's clientHeight/scrollHeight, the lists' min-content widths and columns' line counts, and S7's "painted by" rendering. The faces (Source Sans 3, Fraunces) and the design system v52 under `ds/wombat/` are not in `round-3/`; `design/system/components/bundle.css` (the repo copy, 82 lines behind `app.css` in the two stack blocks) was used for the class check.
- That the 30 captures opened and were viewed (the check board lists them; the images were not reopened).
- That the mark `/_blob/c6a099…` resolves to the right asset (referenced on every board; the bytes are not on disk).

## For the operator

1. **S2:** change the three R3-H01 notes (1280, 390, 768) to the measured band (narrow 1100-1112 px, wide from 1113 px) and add that band to Spec § 13's Dashboard-page row.
2. **C5:** give the NEW sentences of K3, K4, K5, K9, K10, K19 and Q18 a "replaces nothing" or "replaces ..." cell in Spec-2 § 9.
3. Correct the three stale `DependencyInjection.cs` / `CatalogueLinks.cs` cites and reword S7's "painted by" cite (Spec-2 § 7, § 9; Spec-3 § 16; R3-H08 notes).
4. Decide whether R3-H08's Accounts card, which reads Not read, should keep a "Users" footer (the ask does not except it; the Spec does).
5. The PROPOSED settlements (S1's footers, S4, N2, K2-K10, K15-K17, K20, Q18's entity/prune/since/Not read, the NEW words) stand as drawn and listed on Spec-1 § 6; none is a defect.
