# Flow 13 round 2: the cast and runbook review

Reviewed 2026-10-27 from the boards' text and markup in `round-2/project/` (R2-0-spec, -spec-2, -spec-3, R2-0-steps, -steps-2;
77 boards at 1280, 78 at 390, 8 at 768), against `round-2-ask.txt` (FIXTURES, DECISIONS, CORRECTIONS), the brief § 1, and the
runbook (`execution/knowledge/scenario-paediatrics/`: README, `act-5-graduation.md` every step with its T376 Actual lines,
`act-4-annual-review.md` 4.30, `act-6-catalogue.md` 6.21, 6.34, 6.36, 6.40, 6.41, `act-2-onboarding.md` 2.28 to 2.31, 2.39,
2.40, `appendix-cross-cutting.md` A.1.6, A.1.14, A.2.1, A.2.9, A.4.6, A.7.4, A.7.9, A.7.12, `states.md`) and the seed
`paediatric-epa-v11.1.json`. I opened the captures in `design/baseline/act-5/` (5.20-1, 5.20-2, 5.28-3, 5.8-3, 5.27-3, 5.26-2,
5.10-1, 5.17-1, 5.13-1, 5.27-1, 5.21-1). D = 2026-10-26, J = 2026-01-15. Severity: M must-fix, S should-fix, N nit.

## What holds

- **Every person, address, role, id and date checks out.** Molefe (`molefe@`, user id `2b5abc2f-9e37-4714-bb7a-8e48fa11f699`,
  profile 2, start 2023-01-15, expected 2027-01-14, year 4, completed 2026-10-26), du Plessis (`duplessis@`, profile 4, start
  2025-01-15, expected 2029-01-14, year 2, last day 2026-10-24, no STAR), Mbatha, Smit, Naidoo, Mokoena, Sithole and Dlamini
  are spelled and scoped as FIXTURES and the README have them. No title and no pronoun on any page body; role labels, never
  keys. Only dates 2023-01-15, 2025-01-15, 2026-10-24, -25, -26, 2027-01-14 and 2029-01-14 appear as data, plus the two
  typed illustration days (2026-11-02 in E11, 2026-06-30 in P17).
- **Menus** equal FIXTURES: Mbatha's (E04 to E06, P*, C*), Smit's (X01 to X04, E06), Naidoo's as Committee member (X05),
  Mokoena's and Sithole's (X06, X07), Dlamini's Trainee menu (E01, E09), and the former trainee's Home, My progress, Export
  portfolio, My data rights (Q2). The Administrator's menu on X08 equals Step 1.1's.
- **Hashes, files, times.** Both 64-digit hashes in V02, V03, V07, V08, V10 are character-for-character FIXTURES'; V03 is the
  upper-case form of V02's; V09's spaced value joins to the same hash (checked); V06's value is the 12-character prefix.
  `portfolio-aba6ce7943c2.pdf` and `portfolio-344dd52bdded.pdf`; 22:51, 22:52 and 22:56 SAST are 20:51, 20:52 and 20:56 UTC;
  "Exported 3 times, the last 22:52" (5.13) and "Once" (5.23); trainee and exporter both `2b5abc2f-…`; filter 2023-01-15 to
  2026-10-26. The ids are never named (DESIGN's anonymous rule).
- **Counts and lists.** Trainees: 4 current at 5.17 (Dlamini, du Plessis, Mahlangu, Ndlovu), 3 at 5.27, 5 before (C03, in
  surname order), 1 then 2 ended, latest end first (Molefe 2026-10-26 before du Plessis 2026-10-24, the reverse of the old
  list in capture 5.27-3, as the design intends); training year 4 and 2 "when it ended". 15 STAR cards (R08) in code order; 15
  STARs on Home (H01), none for du Plessis (H02, R09). C04's 20 rows are 2 named plus 18 brackets, "Showing 1-20 of 40".
- **Reviews and decisions.** Molefe's last decision "Graduate (programme complete), Pre-graduation review, ratified
  2026-10-26" (5.5, 5.6; P01 to P07, P19); du Plessis's "Satisfactory with Observations, Annual progression review, ratified
  2026-10-26" (4.30; P08 to P10, M7). The dialog's "1 activity, observed 2026-10-25, stops counting." is activity 24 (5.24 to
  5.28).
- **The record.** PAED-010 and PAED-012 "Target: 3 per semester (6 a year). Minimum when your programme ended: 5." equals the
  5.20-1 capture word for word (seed: 6 a year, 3 a semester, exit order 6 = label 5); PAED-002 "3 per semester ... 3b" in year
  2 equals 5.26 and 5.28; the three titles equal the seed; R02's "no target (your programme ended part-way through) · 0
  recorded" equals 5.28.
- **Illustration chips.** The 15 boards with the chip (H03, H04, H07, X08, E16, E17, E18, P07, P16, P17, P18, P19, C04, C05,
  R10) are exactly Spec 3's list and the ask's; the chip and the title both say it; none is a state the replay reaches. No
  replay-reached state (H01, H02, E01 to E08, V02 to V07, P01 to P05, P08 to P11, C01, C02, R01 to R04, R07 to R09) carries
  one. H04 is not played either: `states.md` offers it as an optional scratch only.
- **390 and 768 twins.** All 78 boards at 390 and the 8 at 768 were diffed against their 1280 twins on body text and on every
  `value`, `href`, `aria-label`, `alt` and `title` attribute: no line differs (H08 has no 1280 twin, as asked). The eight 768
  boards are those the ask marks.
- **Steps board.** 26 rows (5.9 to 5.29 with 5.24 to 5.26 as one, 6.21, 6.40, 6.41, A.7.4, A.7.12). Every journey count
  recomputes: Mbatha today 4 + 3 (390: 5 + 3), built 4 + 0 (5 + 0); Smit today 1 + 2, built 4 + 0; her export 1 + 2, built 2 + 0
  (3 + 0 by Menu); verify 1 + 2; complete 4 + 0; withdrawal 4 + 1. A4's line is present. A.2.1 "eight jobs" and A.2.9 "five
  runs" are right arithmetic (nine and six today).

## Findings

| # | Sev | Board | Finding |
|---|---|---|---|
| C1 | M | R2-V02 (and its 390 twin), R2-0-steps row A.7.12 | **A.7.12's twin is the wrong board.** V02 is titled "5.13 verified, three exports (the 390 twin is A.7.12)" and the Steps row says A.7.12 is "R2-V02 at 390". A.7.12's Actual verifies "Dr Molefe's newest export hash": "generated 2026-10-26 20:56 UTC", `344dd52b…`, once, file `portfolio-344dd52bdded.pdf`. That is R2-V07's content (22:56 SAST, "Once"), not V02's (22:51, "3 times, the last 22:52"). Retitle V02's twin or make V07 at 390 the A.7.12 board; fix the Steps row and V02's board subtitle. |
| C2 | M | R2-X08, R2-0-spec-3 A3 | **The refused-export Illustration rests on a false guard.** X08 and A3 say "an Administrator opens the review page, holds no institution, so is refused the export". `TraineeScopeResolver.MayReadAsync` returns true for `IsAdministrator()` (line 446), and `ExportPortfolioCommandHandler.DemandExportAccessAsync` admits "a global Administrator". So the Administrator IS offered "Export Lerato Molefe's portfolio" and the board draws a state that cannot hold. Per A3's own fallback: say on the Spec that no caller who can open the three pages is refused, and redraw X08 as the guard's Illustration with a non-Administrator (or drop the Administrator claim and name no role). If the board stays an Administrator's, add the action. Fixture: devadmin's top bar reads "Demo Administrator" (Step 1.1), so `‹name as stored›` has a value. |
| C3 | S | R2-R10 | An Illustration that draws two states that cannot hold at once (the ask forbids it): 17 EPA cards (‹PAED-016›, ‹PAED-017›) over a standing that still reads "15 of 15 EPAs at their exit level … (level 5: 9 of 9 · level 4: 6 of 6)". With 17 cards the standing reads "of 17" or the extra two are outside the College's rule. Bracket the standing's counts or say which. |
| C4 | S | R2-0-steps-2 row 5.8 | 5.8 is marked "Unchanged", but its **Note** ("My authorisations requires the Trainee role, so this is the last point at which she can download a certificate from that page (see Step 5.22)") is false once Q3 lands, and Step 5.22's Gap F-5.22a closes. Say "Expect unchanged; Note rewritten". |
| C5 | S | R2-0-steps-2 "Then the steps that open the profile" | The row groups 2.30, 2.39, 2.40, 6.34, 6.36, A.7.9 and says their Expect "is not in hand here". Checked: **2.39 and 2.40 do not open the profile** (they are the Trainee's Home and My progress, `/portfolio/progress`); both stay unchanged (their sidebar "Export portfolio" is Dlamini-style Trainee menu). The steps that do open it: 2.30 (Ndlovu, `edit?id=6`), 6.34 (Dlamini, id 3), 6.36 (Ndlovu), A.7.9 (Dlamini's profile and the closed list), each an *active* profile, so each gains the h1 "‹name›'s programme", no Back to trainees, the End this programme section and the header action; their quoted words ("Trainee profile saved.", the curriculum picker, the dates) stay. A.7.9's Expect changes in two lines: "Completed and closed (flow 13's list, 475 px) scrolls in its own container, each Edit 44 px" becomes stacked, View 44 px; "the trainee profile form stacks its four fields" becomes three fields plus the last day and the two triggers. |
| C6 | S | R2-0-steps-2 rows A.2.1, A.2.9 | Counts only. The build must also rewrite A.2.1's "The nine jobs" list and its Actual ("All nine had run"), A.2.9's title "runs the other six jobs", its bullet for `portfolio-export-cleanup`, its Note about the 90-day verify loss, and its Gap F-A.2.9a (T313 then closed). |
| C7 | S | R2-0-steps row 6.21, 6.40, 6.41 | One-line Expects. After T312: 6.21's Do ("open PAED-012's page by its address: her record does not link it while it is paused") and bullets (no PAED-012 card; no standing row) invert, and the Actual's "14 at or above … of 14 EPAs / level 5: 8 of 8" becomes 15 of 15 / 9 of 9; the Gap F-6.21a closes. 6.40's bullet "PAED-012 among them / A KGK-001 card has appeared" and standing "of 16 EPAs" become 15 EPAs, no KGK-001; 6.41's "16 with no decision, of 16 EPAs" becomes 15 of 15; F-6.40a closes. Also R05 omits PAED-006's corrected title (6.14), which 6.21's Expect names. |
| C8 | S | R2-R01, R05, R03, R04, R06, R07, R08, X08, Spec 1 | **Brackets the runbook or the captures fill** (see the next section). Spec 1's "the captures did not arrive" is true for the canvas, but the values exist, so the brackets and the "capture 5.20-2 did not arrive, so not drawn" notes on R03, R04 and R06 can be replaced. |
| C9 | N | R2-R07 | The Mini-CEX row has no "to David Naidoo" sub-line (5.28 Actual and capture 5.28-3 have it); the table is drawn with one row of seven. |
| C10 | N | R2-E09 to E15, H05, H06, P06, P12 to P15, V08 to V10, X05 to X07, X03, X04 | Not played by any step (they are the build's states) and not marked Illustration. The ask lists them unmarked, so this is as asked; each needs a `states.md` row when built. X03 and X04 have no replay step either (A4 names them). |
| C11 | N | R2-E16, E17, E18, P16, P17, P19, C04 | Illustrations mix a fixture's value with a bracketed person: E17's "‹registrar›'s programme started on 2023-01-15" (Molefe's start under another registrar), E18 and P19 keep `molefe@kgk.wombat.local` and "Training year 4" under a 60-character name, E16 gives Molefe about 200 activities (the replay holds 8), C04's 38 bracketed people all read "Paediatric EPA Curriculum (11.1) Paediatrics". Bracket the date, address and curriculum where the person is a bracket. |
| C12 | N | board subtitles of E01, E03, E04, E07, E08, H03, P01, P03, R01 to R06, R09 | Canvas notes use "she", "her", "his" ("Reached from her programme's header action", "Flow 05's standing for him"). Page bodies are clean; the product stores no pronoun, so say "Lerato Molefe's", or "their". |
| C13 | N | R2-P05, P11, R2-0-steps rows 5.17, 5.27, A.1.14 | The summary card loses its "Name:" row and the "Trainee summary" heading (capture 5.17-1 has both); say so on the Steps row. A.1.14's Expect never describes the closed list (only its Actual does): the build adds the line, and the Actual's order (du Plessis, then Molefe) flips to latest-first at A.1.14, 5.27, 6.34. |

## Steps whose Expect (or Do, Note, Gap) the build must rewrite

- **Act 5:** 5.9 (opening dates, result words); 5.10 (Route, Do, Expect, the Note "No page links to the staff export", Gap
  F-5.10a); 5.12 (Route, Do, the Note, Gap); 5.13 (answer in SAST not UTC, "Exported n times, the last …", first exporter,
  upper case now verifies; Note and Gaps F-5.13a, F-5.13b); 5.14 (the third sentence); 5.16 (no dialog: refused at the press);
  5.17 (dialog title, result focus, page title, summary order, closed list columns and View, no Back to trainees); 5.18 (Q12);
  5.20 (the record line; PAED-001's Open My authorisations); 5.21 (whole Expect, the Note, Gap F-5.21a); 5.22 (first of the
  four opens; Gap F-5.22a); 5.23 (Export in her menu; To on her last day; Gap F-5.23a); 5.27 (dialog title and sentences,
  result words and count, summary order, closed list); 5.28 (Home's card words, menu, My activities' subtitle and note row);
  5.8 (Note only, C4). Unchanged: 5.1 to 5.7, 5.11, 5.15, 5.19, 5.24 to 5.26, 5.29.
- **Act 6:** 6.21, 6.40, 6.41 (C7); 2.30, 6.34, 6.36 only as to the profile's title, trail and sections (C5).
- **Appendix:** A.1.6 and A.4.6 (Home's card words and menu); A.1.14 (closed list line, C13); A.2.1, A.2.9 (C6); A.7.4 (Home,
  menu with Export portfolio, Note, Gap F-A.7.4a closes); A.7.9 (C5); A.7.12 (C1). `coverage.md` rows at lines 143, 223, 323,
  385, 494 ("no page links to it") also change.

## Brackets the runbook or the captures could fill

| Board | Bracket | Value and source |
|---|---|---|
| R2-R01, R05 | PAED-010 Semester 2, 2026 `‹n› recorded` | 1 (capture 5.20-1; "Direct observation 2026-10-22" in its Latest rating) |
| R2-R01, R05 | PAED-012 Semester 2, 2026 `‹n› recorded` | 2 (capture 5.20-1; 6.21 Actual charts two ratings, 2026-10-19 and 10-21) |
| R2-R01, R05 | `‹as 5.20 records it›`, `‹earlier semesters›` | "0 of 3, 3 short" for Semester 1, 2026 and every earlier semester down to Semester 1, 2023 (5.20 Actual; capture) |
| R2-R01, R05, R10 | standing `in training year 4 ‹…›` | ", so the targets shown are that year's. STAR decisions and ratings are shown as they stand today. Each target is the level the curriculum sets for training year 4: Annexure A's, for the College's EPAs. The exit level is the level each EPA must reach to finish the programme. A level is compared only on the ladder the curriculum sets for that EPA." (capture 5.20-1); the row above the exit rule reads "15 at or above · 0 below · 0 with no decision, of 15 EPAs" |
| R2-R03 | title `‹title›` | "Providing paediatric emergency care to children" (seed; capture 5.20-2) |
| R2-R03, R06 | `‹target› · Decided each semester · Exit level ‹level›` | "3 a semester · Decided each semester · Exit level 5" for PAED-001, PAED-002 and PAED-012 (captures 5.20-2, 5.26-2; seed) |
| R2-R03, R06 | Entrustment `‹level›`, `Exit level ‹level and its line›` | "5" with "At or above"; Issued 2026-10-26; Exit level "5 · reached" (5.20 Actual; capture 5.20-2) |
| R2-R03 | Observations "not drawn" | Semester 2, 2026 "no target (your programme ended part-way through) · 3 recorded", then Semesters 1, 2026 back to 1, 2023 "0 of 3, 3 short"; "Training year 4: level 5, the minimum each encounter is judged against and your STAR's target."; the two MSF lines; trajectory "3 ratings in the 2026 academic year, from Thandi Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below." (capture 5.20-2) |
| R2-R04 | `Exit level ‹level›` | 5 (capture 5.26-2) |
| R2-R04 | Entrustment `‹flow 05's words where no STAR is on record›` | "No STAR yet." (5.26 Actual; capture 5.26-2) |
| R2-R02, R04 | `‹earlier semesters›` | Semester 1, 2026 "0 of 3, 3 short" (capture 5.26-2); the earlier ones are not in a capture I opened |
| R2-R07 | `All activities (‹n›)` | 7 (capture 5.28-3: DOPS Cancelled, Mini-CEX Completed, Portfolio and Logbook Review Awaiting review, Case-Based Discussion Requested, three Teaching Session Logs) |
| R2-R07 / Spec 2 | "‹today's, capture 5.28-3 not received›" subtitle | "Everything you have filed or logged, newest encounter first." (capture 5.28-3) |
| R2-R08 | `‹title›` on 13 cards, `‹5 or 4›` on 15, `‹rationale›` on 15 | Titles from the seed. Level 5 on PAED-001 to 007, 010, 012; 4 on 008, 009, 011, 013, 014, 015 (5.4 Actual). Rationale: PAED-001 "Leads resuscitation and emergency care without prompting; ready for unsupervised practice."; PAED-012 "Breaks difficult news clearly and kindly; consistent across settings."; the other thirteen "PAED-0nn entrusted at its exit level; the panel recommends completion." (capture 5.8-3) |
| R2-X08 | `‹name as stored›` | "Demo Administrator" (Step 1.1), but see C2 |
| R2-H04 | `‹name as stored›` | None played; if the `states.md` scratch is run, John van Rensburg |
| Not fillable | H03, H07, P16 to P19, E17, E18, C04, C05, R10's `‹…›`, V04's `5c171ec5‹…›` | Illustrations or truncated by the runbook (only `5c171ec5…` is recorded) |
