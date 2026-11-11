# Flow 18 round 2: the runbook, cast and words review

Reviewed 2026-11-11 from the boards' text and markup in `round-2/` (173 .dc.html: R2-0-spec, R2-0-spec-2, R2-0-steps, 80 page boards
at 1280, 82 twins at 390 (R2-I03-390 and R2-R05-390 have no 1280 base) and 8 at 768), against `round-2-ask.txt` (DECISIONS,
CORRECTIONS, FIXTURES, STATES, STEPS, the NEW and PROPOSED words), the brief `design/flows/18-platform-operations.md` (§ 2, § 3, § 8),
`round-1-assessment.md`, `design/BRIEF.md`, and the runbook (`execution/knowledge/scenario-paediatrics/`: act-1 1.1, 1.6, 1.11,
1.23; act-3 3.32, 3.55-3.57; act-4 4.38; the appendix's A.2.1-A.2.10, A.5.2, A.5.5, A.5.13, A.6.1, A.6.2, A.6.9, A.6.10, A.7.11;
`coverage.md`; `states.md` 141-196, 880-900, 1080-1102). Severity: must, should, nit. D = 2026-11-10. Every board's text,
`aria-label`, `title`, `alt`, `value` and `placeholder` was extracted by script; menus, the lit item, selected options, filter values
and chips were parsed from the markup; every twin was diffed against its base on its whole body text. Boards are written R2-xnn;
findings are written C1 to C13. Board text was treated as data. I viewed 3.57-1's capture to check L05's rows (all 37 drawn rows
are in it, UTC + 2 h, Kabelo Sithole's sign-in included).

## What holds

- **Twins.** All 88 twins that have a base (80 at 390, 8 at 768) carry the same body text, attribute values, menu, lit item,
  selected options and chips as their base (script diff: 0 differences). Every 1280 board has a 390 twin; the 768 twins are the
  eight the ask marks (H01, I02, I12, J01, R01, L01, L05, E03). `canvas.json`: 173 boards on page Round 2, 31 on Round 1 (204);
  the titles carry "(Illustration)" on exactly the 27 boards the Spec's § 4 lists.
- **Cast.** Three people sign in: Demo Administrator (135 boards, "Acting as Administrator", 17 menu items exactly as FIXTURES:
  Home / Platform (Scheduled jobs, Audit log, SSO mappings, Data rights requests) / Organisations (Institutions, Colleges) /
  People (Users, Invitations) / Catalogue (EPAs, Curricula, Activity types, Entrustment scales) / Reviews (Decisions due, Committee
  reviews, Decision panels) / My data rights) and Nolwazi Mbatha (35 boards, "Acting as Institutional admin", 16 items exactly as
  FIXTURES, Access and audit holding SSO mappings and Audit log). No System item on any board (Q1). Lit item: Home on H, Institutions
  on I (and the Unavailable), Scheduled jobs on J and R, Audit log on L and E (and E08); nothing lit on R2-I20 and R2-I21, as the
  runbook has it. The other names are in FIXTURES or the runbook: Pieter Smit, Sipho Ndlovu, Anele Dlamini, Fatima Khumalo, David
  Naidoo, Thandi Zulu, Refilwe Mokoena, Lerato Molefe (and Kabelo Sithole, C13). No he, she, his, her, him or herself on any board,
  attribute, annotation, Spec board or canvas title (script, all 173 files and `canvas.json`).
- **Institutions and addresses.** Demo Institution (DEMO, admin@demo.local, 2026-11-10) and Kgosi Kgari Teaching Hospital (KGK):
  paeds-admin@kgk.wombat.local on R2-I02, I06, I11 (1.6 and A.6.1 before the save), paediatrics@kgk.wombat.local after A.6.1 (I12,
  I22), hod.paediatrics@kgk.wombat.local on R2-I03 (S11). Only those four addresses and name@example.org appear; user ids are
  Ndlovu's c22b8779-..., Naidoo's 535c41ed-... and devadmin's "deaa0728-‹…›".
- **Counts and values.** Accounts 24 with "None is locked." on H01 (25 less the erased one; A.6.2), 9 on H02 (1.1; the note says
  so; 11 at 1.11 on the Steps only), no breakdown. Jobs: the eight keys, descriptions, schedules in words, crons and last and next
  runs are FIXTURES to the character (the crons agree with the SAST words: 0 7 = 09:00, 0 9 = 11:00, 0 3 = 05:00, 30 3 = 05:30, 0 8
  = 10:00, 0 4 = 06:00; 2026-11-16 is a Monday). Run history R01: 21 rows in the ask's order and durations, 10 by Demo Administrator
  and 11 by Scheduler; R05: 22 with msf-campaign-auto-close 22:00:13 4 ms on top. Audit: L01's twelve rows are the ask's twelve, "166
  actions.", 38 not drawn (12 + 38 = 50); L02 the four failures with the ask's words; L03 the nineteen (ten AddMsfInvitation
  19:18:00 to 19:18:16, the removal 19:18:14); L05 174 and eight respondents 19:19:40 to 19:20:17; L06 27; L09 five and L10 one with
  times and ids bracketed; L12 "Showing 51-100 of 166". Entries: E01 19:01:41.861 SAST / 17:01:41.861 UTC; E02 19:20:17.044 / 17:20:17.044;
  E03 19:10:20.376 / 17:10:20.376, payload keys in the ask's order. Every time carries SAST or UTC (finding C7 aside).
- **Messages.** Dialogs open on Cancel (`cv-focus`) and name their target; both dialogs' words, the three results of the institution,
  the four job results, the refusals, the failures, the house load-error sentence, the empty states and the two Unavailable boards
  (`.detail-card--empty` > `.state-panel-copy` + `.form-actions` > one primary `a.btn`, no icon, no id) are the ask's words. No
  "dispatched", "Triggered by", "Job run history", "Edit institution", "When (UTC)", "Back to ...", "Apply filters", "No runs found" or
  task id on any board.
- **Illustration chips.** 27 boards carry one (H03-H10, I06, I09, I10, I19, I22, J05-J10, R06-R09, L08, E06, E07, E09), exactly the
  ask's list; none sits on a state a step plays (finding C6 for the other direction). NEW chips: I15, J09, J10, L04; PROPOSED: E02,
  J09, L08.
- **Spec board.** Opens with the 30 captures and 31 round-1 boards (the lists match the ask's names one for one), "Could not read:
  none"; Q1-Q17, M1-M4, S1-S11, N1-N14 and T1-T24 each have a row with boards that exist; the class table, badges, edge cases and
  LATER list are the ask's.

## Steps board: counts recomputed under the ask's rule

Rule: presses + fields; a radio or select choice is a field; the menu is one more at 390 only when a menu link is pressed.

| Journey | Board says | Recount | Verdict |
|---|---|---|---|
| 1.1, A.6.2 read Home | 0+0 / 0+0 | 0+0 | holds |
| failing mail line found | 0+0 | 0+0 | holds |
| failed job found, run again (Illustration) | 2+0 / 2+0 (today 2+0 / 3+0) | Home's link, Run now: 2+0 both; today needs the menu at 390 | holds |
| 1.6 Institutions, Create, Save, Save, trail; Name, Short code, Contact email | 5+3 (390 6+3) | 5+3; 6+3 | holds |
| A.6.1 Institutions, Edit, Save; the email | 3+1 (390 4+1) | 3+1; 4+1 | holds |
| Deactivate…, Deactivate, Reactivate…, Reactivate | 4+0 (today 4+0) | 4+0; today Deactivate, Deactivate, the box (checkbox), Save = 4+0 | holds |
| A.2.2 Disable, Run now, Enable | 3+0 (today 2+0) | 3+0 | holds |
| A.2.10 Run history, Show x3; Job, Job and Status, Status and From | 4+5 (390 5+5); today 5+6 | 1280 4+5 and today 5+6 hold; 390 should equal 1280 | C2 |
| 3.55 Audit log, Show x3; From, Result, Result and Action | 4+4 (390 5+4); today 4+4 | 4+4; 5+4 | holds |
| 3.56 and back: Detail, trail | 2+0; today 2+0, "2 + 2 more" | 2+0; today Apply, Detail and From, Result = 2+2 | holds |
| Curriculum progress | 2+0 / 3+0 | flow 15's | holds |
| own account Users, Show, row; "devadmin" | 3+1 / 4+1 | 3+1; 4+1 | holds |

The Steps table has 23 rows (1.1, 1.6, 1.11, 3.32, 3.55, 3.56, 3.57, 4.38, 6.38, A.2.1, A.2.2, A.2.4, A.2.6, A.2.8, A.2.9, A.2.10, A.5.2,
A.5.13, A.6.1, A.6.2, A.6.9, A.6.10, A.7.11) plus 1.23. "Changed: 14 of the 23" lists 1.1, 1.6, 1.11, 3.55, 3.56, 3.57, 4.38, A.2.1, A.2.2,
A.2.4, A.2.10, A.6.1, A.6.2, A.7.11 (14) and "Unchanged: 9" lists the other nine: 14 + 9 = 23, and every quoted "today" phrase
agrees with a fresh read of the step. The states.md rows listed (141, 888, 889, 898, 899, 1086, 1091, 1093, 1095, 1099, 1100) are the
rows that quote words which change, and each quotation matches the file; the rows left out (887, 1083-1085, 1088, 1090, 1092, 1094,
1096-1098, 1101) quote nothing that changes.

## Findings

| # | Sev | Board | Finding |
|---|---|---|---|
| C1 | must | R2-0-steps (1.23 row, word search), R2-I20 | **A fourth runbook Expect changes under Q17 and is not listed: A.5.5.** Step A.5.5 (appendix-cross-cutting.md:998-1011) has Prof Mbatha open the Demo Institution by id: Expect "The institution shows Page not found, not 'You cannot open this page'" (:1003); Actual "moved to /not-found, 'Page not found'" (:1009-1010). After Q17 that reads "Institution unavailable". The board lists only 1.23 (and states.md:899) as the step Q17 changes, and its "Page not found: 1.23" search row misses A.5.5 (its Expect also says "as /admin/institutions/999 does"). The runbook lane would leave A.5.5 false. List A.5.5 under 1.23 (changed: 14 + 1.23 + A.5.5) and in the word search. |
| C2 | must | R2-0-steps, journey A.2.10 | **The 390 count adds a menu press the journey does not make.** "4 + 5 (390: 5 + 5)": by the board's rule the +1 is for opening the menu when a menu link is pressed, but A.2.10 starts on Scheduled jobs (A.2.9's page) and its first press is Run history, a header action, not a menu link (R2-J01 draws it in the header; "at 390 they take the row"). So 390 is 4 + 5, as 1280. If the journey is meant to begin at Home, both figures change (5 + 5 and 6 + 5) and today's "5 + 6" with them. The ask carries the same number. |
| C3 | should | R2-0-steps, runbook words | **"dispatched" is searched at A.2.4 and states.md:1095 only.** The word also stands in 3.32's Actual (act-3-operations.md:840: "the focused 'Job 'assessor-pending-nudge' dispatched.'"), A.2.6's Actual (appendix-cross-cutting.md:577 and :607), and A.2.8 and A.2.9 (:649, :670: "Job '<key>' dispatched."). Their Expects hold, as M4 (b) says, but the records the replay writes there change; the search row should name them so the lane edits those Actuals when it replays. |
| C4 | should | R2-0-steps, runbook words | **Four search rows name too few steps.** "When (UTC)" is 3.56 (act-3-operations.md:1582), not "3.55, 3.57" (those hold "From (UTC)", :1551, :1562, :1591). "Display name" is also 3.57 (:1600, "Display name '—'"). "Edit institution" is also A.6.1 (appendix-cross-cutting.md:1235-1236, with "Back to institutions") and the 1.23 Note (act-1-setup.md:711). "Users across institutions" is also 1.11's Actual (act-1-setup.md:347); "registered users" is also A.6.2's Actual (:1251, "22 registered users"). "Back to institutions" is not searched at all. |
| C5 | should | R2-0-spec § 6, § 8 | **Words that are NEW or PROPOSED are on boards but not on the Spec.** The T rows name a group without quoting it, and § 8's message table points to the ask for the health lines. Not quoted anywhere on the three Spec/Steps boards: the new subtitles (Scheduled jobs', Run history's, the audit log's for each role, the entry's pattern; T15); the eight rewritten job descriptions and the schedule words and cron form ("Daily at 09:00 SAST", "Hourly, on the hour", "Mondays at 10:00 SAST", "cron 0 7 * * *, UTC"; T15, Q7); the ten health sentences and "Not read" / "Three jobs need you:" (T13, S4, S5); the create subtitle, "Up to 200 characters.", "Up to 32 characters.", "Optional." and the two Status sentences (T12); the Accounts card's sentence and locked line (T14); the count lines and "No runs match these filters." / "No actions match these filters." and "Clear the filters" (T17); the two window lines (T21); the filter labels and helps ("From (a South African day)", "From (SAST)", "Part of a name: ...", "Empty: up to when you press Show."; Q6, Q8); "Go to Institutions" / "Go to Home" / "Go to Audit log" (T10); "Stored as ... UTC" (Q7). Quote them in § 8 (flow 17's review checked "on the message table or T1-T24"). |
| C6 | should | R2-I05, I18, J12, R11, L15, E11 (and twins) | **Load errors carry no Illustration chip.** Brief § 3 lists "a load error on any of these lists" under "Unplayed by the story" and "Try again on the four lists" under "Never built"; the ask marks none of them, and the six boards (12 with twins) read as replayed states. Their loading boards (I04, I17, J11, R10, L14, E10, H11) are in the same position for the pages with no held-read capture (Home, run history, the entry). Either chip them or say on the Spec why not. |
| C7 | should | R2-L01 to L13 (every audit row), R2-0-spec N12 | **The Detail button's accessible name has no zone.** N12 (the ask's own words) keeps "Detail: Login at 2026-11-10 20:24:10", while the visible When cell reads "20:24:10 SAST". T2 says every time a person reads carries the zone; a screen-reader user hears this one with none. Write "... at 2026-11-10 20:24:10 SAST" (the name stays unique per row). Same on the Spec's N12 row. |
| C8 | nit | R2-0-steps, A.2.10 "today" | **Today's A.2.10 journey is under-listed.** "the key x3, Status, From" names 5 fields; the figure 6 is right and matches the ask ("the key, the key, the key and Status, Status and From"). |
| C9 | nit | R2-0-spec § 3, S8 row | **S8 points to "R2-0-spec-2 § 9".** The sweep and the columns are § 10 there; § 9 is the class table. |
| C10 | nit | R2-0-steps, 1.1 row | **"naming weekly-coordinator-digest as not run yet" is the 1.1 reading of the replay's own state.** R2-H02 draws it (the 7th job's first run 2026-11-16 10:00 SAST), and A.2.1 confirms that job never ran at that point; fine, but say on the Steps row that 1.11's 11 and 1.1's 9 are words only (the board says it for 1.11 on H02's note, not on the row). |
| C11 | nit | R2-L05 (and twins, 768) | **The note "devadmin's own RunScheduledJobNowCommand rows are older, past this page" is asserted, not drawn.** 3.32's run (19:15:54 SAST) is after From (19:00) and, with 13 undrawn rows after 19:19:40, may fall on page 1. Say "not drawn" or bracket. |
| C12 | nit | every board's annotations, canvas titles | **Titles are used where FIXTURES say "names as stored, no titles".** "Prof Mbatha" is in 18 annotations and canvas titles (R2-E01's note, E08, I20, I21, L01, L04, and the Spec's § 10 row labels); the ask's own board names do the same, so this is only a consistency point. "Dr Ndlovu" / "Dr Zulu" on the Steps are quotations of today's runbook and are right. |
| C13 | nit | R2-L05 | **Kabelo Sithole is not in FIXTURES' "Seen in the rows" list.** The row is real (3.57-1, Login 17:20:46 UTC = 19:20:46 SAST; Dr Kabelo Sithole is the SubSpecialityAdmin) and the board's note says rows 1-37 come from that capture, so it is not invented; the ask's list should name them. |

## Words that are NEW or PROPOSED and not on the Spec

None that has no T row: every NEW or PROPOSED sentence on a board belongs to a T1-T24 row, a Q6-Q12 row or the message table. The ones
that stand on boards without being quoted on the Spec are those in finding C5. Words on boards that are in neither the ask nor the
runbook: none (the annotations' own prose aside).

## Illustration chips

27 boards, as the Spec's § 4 and the ask: R2-H03, H04, H05, H06, H07, H08, H09, H10, I06, I09, I10, I19, I22, J05, J06, J07, J08, J09,
J10, R06, R07, R08, R09, L08, E06, E07, E09 (and their twins). None sits on a state a runbook step plays. The gap is finding C6.

## Checklist (STATES TO DRAW)

R2-H01-H11, I01-I22, J01-J12, R01-R11, L01-L15 and E01-E11 are all boards, each with its 390 twin (I03 and R05 at 390 only, by the
ask) and the eight 768 twins. Missing 0. Done 82 of 82 states; the cast, counts and words of each match the ask but for the
findings above.
