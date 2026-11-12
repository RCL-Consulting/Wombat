# Flow 18: the build sweep (T400, build-lanes.md § "The sweep")

Measured 2026-11-11 in Chrome (Playwright, headless, viewport height 800; 844 at 768 and below) against `t400` (`e2ad955f`,
then the fix below) published to :5184 on `wombat_scenario_t400_sweep`, a restore of the T398 replay's end state
(`recovery/scenario-t398-post-actA.dump`); the start migrated it (`T400_EmailOutcomes`). Signed in as devadmin
(`devadmin@wombat.local`, Administrator) and Prof Mbatha (`mbatha@kgk.wombat.local`, KGK's institutional admin). At each
width: every `.table-container`, its table's `scrollWidth` against the container's `clientWidth`, and the table's narrowest
(`width: min-content`) against the container, which is the spare; every `.detail-card`/`.form-container`, `scrollWidth`
against `clientWidth`; the page's `documentElement.scrollWidth` against its `clientWidth`; below 641px every visible
control in `main` under 44px. Pass: nothing wider than its box, the page its own width, no control under 44px below 641px.

**States, reached on the scratch database only.** Before the sweep: Run now on activity-draft-nudge ("Ran
activity-draft-nudge: Succeeded at 2026-11-11 21:33 SAST.", the result focused), Disable on msf-campaign-auto-close
("Disabled msf-campaign-auto-close. The scheduler will not run it until you enable it."), and a failed run of
weekly-coordinator-digest inserted by SQL with a 330-character error (an Npgsql connection failure with a long unbroken
type name), so Home read Scheduled jobs Failed with its "Its runs" button, Scheduled jobs showed the error under Last run,
and Run history under Status. After the sweep, Run now on weekly-coordinator-digest gave Scheduled jobs a state with no
error, measured at 1151, 1280 and 1440. The start had run all eight jobs (overdue), so Run history held 29 runs, then 31.
The audit log is read in a fixed window, `?from=2026-11-10T17:00&to=2026-11-11T00:00` (the T398 replay's acts), so a page
of 50 holds the replay's failures for both roles; the default 24-hour window slid past them while the sweep ran.

**First pass: nothing sideways, nothing under 44px; 4 column faults at 1280** (BRIEF § 11, flows 15–16: a value past two
lines beside an empty band; a one-word value broken):
- **Scheduled jobs, with a failed run** (devadmin): the error's max-content took Last run to 407px, leaving Job 210px
  (a key over 2 lines, a description over 3–4) and Schedule 110px ("Daily at 09:00 SAST" over 2 lines, "cron 0 7 * * *,
  UTC" over 3); on the seven rows with no error, Last run held its day, hour and badge in 375px, an empty band beside them.
- **Run history, with a failed run** (devadmin): Status 463px; Job 146px, so every key broke at its hyphens,
  "weekly-coordinator-digest" over 3 lines, beside a Status cell holding one badge.
- **The audit log, both roles**: Action is `.col-wrap`, whose header's 14% share held it at 144px, so
  "UpdateCurrentUserProfileCommand" and "RunScheduledJobNowCommand" broke over 3 lines; with a failure on the page Result
  took 326px (devadmin) and 345px (Prof Mbatha) on every row.
- **The audit log with Success only** (`&result=success`, both roles): the names still broke over 3 lines in Action's
  144px beside When's 235px (devadmin) and 259px (Prof Mbatha), one line each with a 60–90px band.

**Fixes** (`app.css`, lane B's and lane C's sections; DESIGN.md § Table system and the section index amended):
- Lane B: while a row shows an error, its column asks for a share (`:has()` on the table, the header by its place):
  Scheduled jobs' Last run 25%, Run history's Status 30%, as round 3 drew them (R3-J08's 241px, R3-R07's 440px of 972).
- Lane C: the audit's Action header asks for no share (`width: auto`; the 7rem floor and the break kept), and Result asks
  for 20% while a row shows a failure's words.
- A share is not a floor: every narrowest table is unchanged (536, 573, 539, 710 and 699px), so the stacks' edges stand.
  Web.Tests 4950 passed (its own build); republished and measured again.

**Second pass, the tables below: 0 fails** at every width, both roles; the columns at 1280 after them.

**The smallest spare** (a table's narrowest against its container, laid out as a table): Institutions 180px, Scheduled
jobs 307px, the audit log 133px (devadmin) and 144px (Prof Mbatha), each at 1151px; Run history 144px (all jobs) and 178px
(one job) at 1025px. Stacked (to 1150px; Run history to 1024px), a stacked table's narrowest is 125–167px. The canvas found
the natural tables overflowing at 843–945px (R3-0-spec-3 § 10); nothing laid out as a table overflows above its stack's
edge.

**Home (devadmin), S2 as measured.** The health list is 739–743px from 1095 to 1099 (two tracks, the card spanning both:
wide); 471.3px at 1100 (three tracks), rising 0.67px a pixel to 479.3px at 1112 (narrow: one column, the badge beside the
name); 480.0px at 1113 (wide) and 484.7px at 1120. So narrow from 1100 to 1112 and wide from 1113, as the build plan says.
The grid: one track at 900, two from 901 to 1099, three from 1100. At 641 and 768 the list is narrow (285 and 412px, one
track), at 640 wide (558px), at 390 narrow (308px). The footers' buttons at 390 and 640: "Its runs" (named "Its runs:
weekly-coordinator-digest, failed"), Scheduled jobs and Users, each 44px.

**An entry's payload** (devadmin, `01a1c615-b64e-7395-a5c6-097f9f83c97f`, the largest the database holds: a refused
activity type save, 1,974 characters stored): the region is 480px to its padding (`max-height: 28rem`, 448px of content,
`overflow: auto`), its content 704px (1280) to 1607px (390), so it scrolls inside itself at every width and never
sideways (its `scrollWidth` = `clientWidth`); the page unscrolled (`scrollY` 0), 1504px long at 1280 and 1780px at 390.
Prof Mbatha's entry (her newest) has no Payload section, the Administrator's alone, as built.

**For the review, not widths:**
- The payload region is `content-box`, so 28rem is its content and the box is 482px (30rem with its 1rem padding and
  border). The Spec says "capped at 28rem"; either reading holds, but say which. *Settled by the fix pass (N2): 28rem of
  content, as DESIGN.md's audit entry now says.*
- Disable on Scheduled jobs acted at once, with no dialog. Check that R3-J02 draws none.
- At 1280 after the fix two command names still break once ("UpdateCurrentUserProfileCommand",
  "CreateOrUpdateAssessorProfileCommand", 2 lines in 196–278px), and with an error on the page Run history's
  "msf-invitation-expiry-reminder" takes 2 lines: what round 3 accepted for devadmin's Action (237px, 2 lines) and R3-R07's
  Job (2 lines). devadmin's Person is the name over the institution, each up to 2 lines ("Kgosi Kgari Teaching Hospital").
- Run history at 1025px with an error on the page: Status is 215px and the error takes 15 lines. As R3-R07 says, the
  error is the row's point.

**Not measured**: a 2,000-character error (the inserted one is 330); Institutions with more than its 2 rows; the
institution record's Deactivate dialog and its refusal states.

## Every column at 1280, after the fix

Each column's width in px, and each value that takes more than one line (from its text's line boxes; a badge alone in its
cell reads as one line). An error or a failure's words are the row's point and may run long.

| List, state | Columns (width) | Values over one line |
|---|---|---|
| Institutions (devadmin, 2 rows) | Name 279 · Short code 116 · Contact email 279 · Status 95 · Created 115 · Actions 87 | none |
| Scheduled jobs, a failed run (devadmin, 8 rows) | Job 308 · Schedule 128 · Last run 243 · Next run 132 · Actions 162 | descriptions 2–3 lines (the key 1); each schedule's words and its cron 2 lines each; "None while disabled" 2; the error 11 |
| Scheduled jobs, no error (devadmin, 8 rows) | Job 367 · Schedule 139 · Last run 143 · Next run 141 · Actions 183 | descriptions 2 lines; each schedule's words and its cron 2 lines each; "None while disabled" 2 |
| Run history, all jobs, a failed run (devadmin, 31 rows) | Job 233 · Started 194 · Duration 92 · Status 292 · Ran by 162 | "msf-invitation-expiry-reminder" 2 (3 rows); "Demo Administrator" 2 (11 rows); Started breaks only between its day and its hour; the error 9 |
| Run history, one job (devadmin, 4 rows) | Job 230 · Started 274 · Duration 92 · Status 149 · Ran by 227 | none |
| The audit log, the replay's window (devadmin, 50 rows) | When 163 · Person 166 · Category 131 · Action 228 · Result 194 · Detail 88 | When between its day and its hour; "Kgosi Kgari Teaching Hospital" 2 (45 rows), "Demo Administrator" 2 (2 rows); "UpdateCurrentUserProfileCommand" 2, "CreateOrUpdateAssessorProfileCommand" 2 (2 rows); a refusal 8, "Account locked after repeated failed sign-in attempts." 3 |
| The audit log, Success only (devadmin) | When 187 · Person 192 · Category 131 · Action 279 · Result 94 · Detail 88 | "Kgosi Kgari Teaching Hospital" 2 (45 rows); "CreateOrUpdateAssessorProfileCommand" 2 (2 rows) |
| The audit log, the replay's window (Prof Mbatha, 50 rows) | When 173 · Person 135 · Category 131 · Action 250 · Result 194 · Detail 88 | When between its day and its hour; "Nolwazi Mbatha" 2 (14 rows), "Refilwe Mokoena" 2 (4), "Mohammed Patel" 2 (5); "UpdateCurrentUserProfileCommand" 2, "CreateOrUpdateAssessorProfileCommand" 2 (2 rows); the failures as devadmin's |
| The audit log, Success only (Prof Mbatha) | When 200 · Person 148 · Category 131 · Action 310 · Result 94 · Detail 88 | "Mohammed Patel" 2 (4 rows); "CreateOrUpdateAssessorProfileCommand" 2 (2 rows) |

Before the fix (the first pass, the same states): Scheduled jobs Job 210, Schedule 110, Last run 407; Run history Job 146
(a key over 3 lines), Status 463; the audit log Action 144 (names over 3 lines) for both roles, Result 326 (devadmin) and
345 (Prof Mbatha); with Success only Action 144 (3 lines) beside When 235 (devadmin) and 259 (Prof Mbatha).

## Institutions, as devadmin (/admin/institutions, 2 rows)

| Width | Laid out | Table | Container | Spare | Narrowest table | Spare at narrowest | Page | Under 44px |
|---|---|---|---|---|---|---|---|---|
| 900 | stacked | 592 | 592 | 0 | 144 | 448 | ok | - |
| 925 | stacked | 617 | 617 | 0 | 144 | 473 | ok | - |
| 950 | stacked | 642 | 642 | 0 | 144 | 498 | ok | - |
| 975 | stacked | 667 | 667 | 0 | 144 | 523 | ok | - |
| 1000 | stacked | 692 | 692 | 0 | 144 | 548 | ok | - |
| 1024 | stacked | 716 | 716 | 0 | 144 | 572 | ok | - |
| 1025 | stacked | 717 | 717 | 0 | 167 | 550 | ok | - |
| 1050 | stacked | 742 | 742 | 0 | 167 | 575 | ok | - |
| 1075 | stacked | 767 | 767 | 0 | 167 | 600 | ok | - |
| 1100 | stacked | 792 | 792 | 0 | 167 | 625 | ok | - |
| 1125 | stacked | 817 | 817 | 0 | 167 | 650 | ok | - |
| 1150 | stacked | 842 | 842 | 0 | 167 | 675 | ok | - |
| 1151 | table | 843 | 843 | 0 | 663 | 180 | ok | - |
| 1200 | table | 892 | 892 | 0 | 663 | 229 | ok | - |
| 1280 | table | 972 | 972 | 0 | 663 | 309 | ok | - |
| 1440 | table | 1132 | 1132 | 0 | 663 | 469 | ok | - |
| 390 | stacked | 356 | 356 | 0 | 144 | 212 | ok | 0 |
| 640 | stacked | 606 | 606 | 0 | 144 | 462 | ok | 0 |
| 641 | stacked | 333 | 333 | 0 | 144 | 189 | ok | - |
| 768 | stacked | 460 | 460 | 0 | 144 | 316 | ok | - |

## Scheduled jobs, as devadmin (/admin/jobs, 8 rows)

| Width | Laid out | Table | Container | Spare | Narrowest table | Spare at narrowest | Page | Under 44px |
|---|---|---|---|---|---|---|---|---|
| 900 | stacked | 592 | 592 | 0 | 128 | 464 | ok | - |
| 925 | stacked | 617 | 617 | 0 | 128 | 489 | ok | - |
| 950 | stacked | 642 | 642 | 0 | 128 | 514 | ok | - |
| 975 | stacked | 667 | 667 | 0 | 128 | 539 | ok | - |
| 1000 | stacked | 692 | 692 | 0 | 128 | 564 | ok | - |
| 1024 | stacked | 716 | 716 | 0 | 128 | 588 | ok | - |
| 1025 | stacked | 717 | 717 | 0 | 128 | 589 | ok | - |
| 1050 | stacked | 742 | 742 | 0 | 128 | 614 | ok | - |
| 1075 | stacked | 767 | 767 | 0 | 128 | 639 | ok | - |
| 1100 | stacked | 792 | 792 | 0 | 128 | 664 | ok | - |
| 1125 | stacked | 817 | 817 | 0 | 128 | 689 | ok | - |
| 1150 | stacked | 842 | 842 | 0 | 128 | 714 | ok | - |
| 1151 | table | 843 | 843 | 0 | 536 | 307 | ok | - |
| 1200 | table | 892 | 892 | 0 | 536 | 356 | ok | - |
| 1280 | table | 972 | 972 | 0 | 536 | 436 | ok | - |
| 1440 | table | 1132 | 1132 | 0 | 536 | 596 | ok | - |
| 390 | stacked | 356 | 356 | 0 | 128 | 228 | ok | 0 |
| 640 | stacked | 606 | 606 | 0 | 128 | 478 | ok | 0 |
| 641 | stacked | 333 | 333 | 0 | 128 | 205 | ok | - |
| 768 | stacked | 460 | 460 | 0 | 128 | 332 | ok | - |

## Run history, as devadmin, all jobs (/admin/jobs/runs, 31 rows)

| Width | Laid out | Table | Container | Spare | Narrowest table | Spare at narrowest | Page | Under 44px |
|---|---|---|---|---|---|---|---|---|
| 900 | stacked | 592 | 592 | 0 | 125 | 467 | ok | - |
| 925 | stacked | 617 | 617 | 0 | 125 | 492 | ok | - |
| 950 | stacked | 642 | 642 | 0 | 125 | 517 | ok | - |
| 975 | stacked | 667 | 667 | 0 | 125 | 542 | ok | - |
| 1000 | stacked | 692 | 692 | 0 | 125 | 567 | ok | - |
| 1024 | stacked | 716 | 716 | 0 | 125 | 591 | ok | - |
| 1025 | table | 717 | 717 | 0 | 573 | 144 | ok | - |
| 1050 | table | 742 | 742 | 0 | 573 | 169 | ok | - |
| 1075 | table | 767 | 767 | 0 | 573 | 194 | ok | - |
| 1100 | table | 792 | 792 | 0 | 573 | 219 | ok | - |
| 1125 | table | 817 | 817 | 0 | 573 | 244 | ok | - |
| 1150 | table | 842 | 842 | 0 | 573 | 269 | ok | - |
| 1151 | table | 843 | 843 | 0 | 573 | 270 | ok | - |
| 1200 | table | 892 | 892 | 0 | 573 | 319 | ok | - |
| 1280 | table | 972 | 972 | 0 | 573 | 399 | ok | - |
| 1440 | table | 1132 | 1132 | 0 | 573 | 559 | ok | - |
| 390 | stacked | 356 | 356 | 0 | 125 | 231 | ok | 0 |
| 640 | stacked | 606 | 606 | 0 | 125 | 481 | ok | 0 |
| 641 | stacked | 333 | 333 | 0 | 125 | 208 | ok | - |
| 768 | stacked | 460 | 460 | 0 | 125 | 335 | ok | - |

## Run history, as devadmin, one job (/admin/jobs/runs?job=activity-draft-nudge, 4 rows)

| Width | Laid out | Table | Container | Spare | Narrowest table | Spare at narrowest | Page | Under 44px |
|---|---|---|---|---|---|---|---|---|
| 900 | stacked | 592 | 592 | 0 | 125 | 467 | ok | - |
| 925 | stacked | 617 | 617 | 0 | 125 | 492 | ok | - |
| 950 | stacked | 642 | 642 | 0 | 125 | 517 | ok | - |
| 975 | stacked | 667 | 667 | 0 | 125 | 542 | ok | - |
| 1000 | stacked | 692 | 692 | 0 | 125 | 567 | ok | - |
| 1024 | stacked | 716 | 716 | 0 | 125 | 591 | ok | - |
| 1025 | table | 717 | 717 | 0 | 539 | 178 | ok | - |
| 1050 | table | 742 | 742 | 0 | 539 | 203 | ok | - |
| 1075 | table | 767 | 767 | 0 | 539 | 228 | ok | - |
| 1100 | table | 792 | 792 | 0 | 539 | 253 | ok | - |
| 1125 | table | 817 | 817 | 0 | 539 | 278 | ok | - |
| 1150 | table | 842 | 842 | 0 | 539 | 303 | ok | - |
| 1151 | table | 843 | 843 | 0 | 539 | 304 | ok | - |
| 1200 | table | 892 | 892 | 0 | 539 | 353 | ok | - |
| 1280 | table | 972 | 972 | 0 | 539 | 433 | ok | - |
| 1440 | table | 1132 | 1132 | 0 | 539 | 593 | ok | - |
| 390 | stacked | 356 | 356 | 0 | 125 | 231 | ok | 0 |
| 640 | stacked | 606 | 606 | 0 | 125 | 481 | ok | 0 |
| 641 | stacked | 333 | 333 | 0 | 125 | 208 | ok | - |
| 768 | stacked | 460 | 460 | 0 | 125 | 335 | ok | - |

## The audit log, as devadmin (/admin/audit?from=2026-11-10T17:00&to=2026-11-11T00:00, a page of 50)

| Width | Laid out | Table | Container | Spare | Narrowest table | Spare at narrowest | Page | Under 44px |
|---|---|---|---|---|---|---|---|---|
| 900 | stacked | 592 | 592 | 0 | 131 | 461 | ok | - |
| 925 | stacked | 617 | 617 | 0 | 131 | 486 | ok | - |
| 950 | stacked | 642 | 642 | 0 | 131 | 511 | ok | - |
| 975 | stacked | 667 | 667 | 0 | 131 | 536 | ok | - |
| 1000 | stacked | 692 | 692 | 0 | 131 | 561 | ok | - |
| 1024 | stacked | 716 | 716 | 0 | 131 | 585 | ok | - |
| 1025 | stacked | 717 | 717 | 0 | 131 | 586 | ok | - |
| 1050 | stacked | 742 | 742 | 0 | 131 | 611 | ok | - |
| 1075 | stacked | 767 | 767 | 0 | 131 | 636 | ok | - |
| 1100 | stacked | 792 | 792 | 0 | 131 | 661 | ok | - |
| 1125 | stacked | 817 | 817 | 0 | 131 | 686 | ok | - |
| 1150 | stacked | 842 | 842 | 0 | 131 | 711 | ok | - |
| 1151 | table | 843 | 843 | 0 | 710 | 133 | ok | - |
| 1200 | table | 892 | 892 | 0 | 710 | 182 | ok | - |
| 1280 | table | 972 | 972 | 0 | 710 | 262 | ok | - |
| 1440 | table | 1132 | 1132 | 0 | 710 | 422 | ok | - |
| 390 | stacked | 356 | 356 | 0 | 131 | 225 | ok | 0 |
| 640 | stacked | 606 | 606 | 0 | 131 | 475 | ok | 0 |
| 641 | stacked | 333 | 333 | 0 | 131 | 202 | ok | - |
| 768 | stacked | 460 | 460 | 0 | 131 | 329 | ok | - |

## The audit log, as Prof Mbatha (/admin/audit?from=2026-11-10T17:00&to=2026-11-11T00:00, a page of 50)

| Width | Laid out | Table | Container | Spare | Narrowest table | Spare at narrowest | Page | Under 44px |
|---|---|---|---|---|---|---|---|---|
| 900 | stacked | 592 | 592 | 0 | 131 | 461 | ok | - |
| 925 | stacked | 617 | 617 | 0 | 131 | 486 | ok | - |
| 950 | stacked | 642 | 642 | 0 | 131 | 511 | ok | - |
| 975 | stacked | 667 | 667 | 0 | 131 | 536 | ok | - |
| 1000 | stacked | 692 | 692 | 0 | 131 | 561 | ok | - |
| 1024 | stacked | 716 | 716 | 0 | 131 | 585 | ok | - |
| 1025 | stacked | 717 | 717 | 0 | 131 | 586 | ok | - |
| 1050 | stacked | 742 | 742 | 0 | 131 | 611 | ok | - |
| 1075 | stacked | 767 | 767 | 0 | 131 | 636 | ok | - |
| 1100 | stacked | 792 | 792 | 0 | 131 | 661 | ok | - |
| 1125 | stacked | 817 | 817 | 0 | 131 | 686 | ok | - |
| 1150 | stacked | 842 | 842 | 0 | 131 | 711 | ok | - |
| 1151 | table | 843 | 843 | 0 | 699 | 144 | ok | - |
| 1200 | table | 892 | 892 | 0 | 699 | 193 | ok | - |
| 1280 | table | 972 | 972 | 0 | 699 | 273 | ok | - |
| 1440 | table | 1132 | 1132 | 0 | 699 | 433 | ok | - |
| 390 | stacked | 356 | 356 | 0 | 131 | 225 | ok | 0 |
| 640 | stacked | 606 | 606 | 0 | 131 | 475 | ok | 0 |
| 641 | stacked | 333 | 333 | 0 | 131 | 202 | ok | - |
| 768 | stacked | 460 | 460 | 0 | 131 | 329 | ok | - |

## Home, as devadmin (/)

| Width | Cards (scrollWidth / clientWidth) | Grid tracks | Health list | Lines (columns each) | Page | Under 44px |
|---|---|---|---|---|---|---|
| 900 | 2 fit | 1 | 544 | 2 2 2 (wide) | ok | - |
| 901 | 2 fit | 2 | 545 | 2 2 2 (wide) | ok | - |
| 925 | 2 fit | 2 | 569 | 2 2 2 (wide) | ok | - |
| 950 | 2 fit | 2 | 594 | 2 2 2 (wide) | ok | - |
| 975 | 2 fit | 2 | 619 | 2 2 2 (wide) | ok | - |
| 1000 | 2 fit | 2 | 644 | 2 2 2 (wide) | ok | - |
| 1025 | 2 fit | 2 | 669 | 2 2 2 (wide) | ok | - |
| 1050 | 2 fit | 2 | 694 | 2 2 2 (wide) | ok | - |
| 1075 | 2 fit | 2 | 719 | 2 2 2 (wide) | ok | - |
| 1095 | 2 fit | 2 | 739 | 2 2 2 (wide) | ok | - |
| 1096 | 2 fit | 2 | 740 | 2 2 2 (wide) | ok | - |
| 1097 | 2 fit | 2 | 741 | 2 2 2 (wide) | ok | - |
| 1098 | 2 fit | 2 | 742 | 2 2 2 (wide) | ok | - |
| 1099 | 2 fit | 2 | 743 | 2 2 2 (wide) | ok | - |
| 1100 | 2 fit | 3 | 471.3 | 1 1 1 (narrow) | ok | - |
| 1101 | 2 fit | 3 | 472 | 1 1 1 (narrow) | ok | - |
| 1102 | 2 fit | 3 | 472.7 | 1 1 1 (narrow) | ok | - |
| 1103 | 2 fit | 3 | 473.3 | 1 1 1 (narrow) | ok | - |
| 1104 | 2 fit | 3 | 474 | 1 1 1 (narrow) | ok | - |
| 1105 | 2 fit | 3 | 474.7 | 1 1 1 (narrow) | ok | - |
| 1106 | 2 fit | 3 | 475.3 | 1 1 1 (narrow) | ok | - |
| 1107 | 2 fit | 3 | 476 | 1 1 1 (narrow) | ok | - |
| 1108 | 2 fit | 3 | 476.7 | 1 1 1 (narrow) | ok | - |
| 1109 | 2 fit | 3 | 477.3 | 1 1 1 (narrow) | ok | - |
| 1110 | 2 fit | 3 | 478 | 1 1 1 (narrow) | ok | - |
| 1111 | 2 fit | 3 | 478.7 | 1 1 1 (narrow) | ok | - |
| 1112 | 2 fit | 3 | 479.3 | 1 1 1 (narrow) | ok | - |
| 1113 | 2 fit | 3 | 480 | 2 2 2 (wide) | ok | - |
| 1114 | 2 fit | 3 | 480.7 | 2 2 2 (wide) | ok | - |
| 1115 | 2 fit | 3 | 481.3 | 2 2 2 (wide) | ok | - |
| 1116 | 2 fit | 3 | 482 | 2 2 2 (wide) | ok | - |
| 1117 | 2 fit | 3 | 482.7 | 2 2 2 (wide) | ok | - |
| 1118 | 2 fit | 3 | 483.3 | 2 2 2 (wide) | ok | - |
| 1119 | 2 fit | 3 | 484 | 2 2 2 (wide) | ok | - |
| 1120 | 2 fit | 3 | 484.7 | 2 2 2 (wide) | ok | - |
| 1125 | 2 fit | 3 | 488 | 2 2 2 (wide) | ok | - |
| 1150 | 2 fit | 3 | 504.7 | 2 2 2 (wide) | ok | - |
| 1151 | 2 fit | 3 | 505.3 | 2 2 2 (wide) | ok | - |
| 1280 | 2 fit | 3 | 591.3 | 2 2 2 (wide) | ok | - |
| 390 | 2 fit | 1 | 308 | 1 1 1 (narrow) | ok | 0 |
| 640 | 2 fit | 1 | 558 | 2 2 2 (wide) | ok | 0 |
| 641 | 2 fit | 1 | 285 | 1 1 1 (narrow) | ok | - |
| 768 | 2 fit | 1 | 412 | 1 1 1 (narrow) | ok | - |

## Home, as Prof Mbatha (/)

| Width | Cards (scrollWidth / clientWidth) | Page | Under 44px |
|---|---|---|---|
| 900 | 3 fit | ok | - |
| 925 | 3 fit | ok | - |
| 950 | 3 fit | ok | - |
| 975 | 3 fit | ok | - |
| 1000 | 3 fit | ok | - |
| 1024 | 3 fit | ok | - |
| 1025 | 3 fit | ok | - |
| 1050 | 3 fit | ok | - |
| 1075 | 3 fit | ok | - |
| 1100 | 3 fit | ok | - |
| 1125 | 3 fit | ok | - |
| 1150 | 3 fit | ok | - |
| 1151 | 3 fit | ok | - |
| 1280 | 3 fit | ok | - |
| 390 | 3 fit | ok | 0 |
| 640 | 3 fit | ok | 0 |
| 641 | 3 fit | ok | - |
| 768 | 3 fit | ok | - |

## The institution record, as devadmin (/admin/institutions/2)

| Width | Cards (scrollWidth / clientWidth) | Page | Under 44px |
|---|---|---|---|
| 900 | 1 fit | ok | - |
| 925 | 1 fit | ok | - |
| 950 | 1 fit | ok | - |
| 975 | 1 fit | ok | - |
| 1000 | 1 fit | ok | - |
| 1024 | 1 fit | ok | - |
| 1025 | 1 fit | ok | - |
| 1050 | 1 fit | ok | - |
| 1075 | 1 fit | ok | - |
| 1100 | 1 fit | ok | - |
| 1125 | 1 fit | ok | - |
| 1150 | 1 fit | ok | - |
| 1151 | 1 fit | ok | - |
| 1280 | 1 fit | ok | - |
| 390 | 1 fit | ok | 0 |
| 640 | 1 fit | ok | 0 |
| 641 | 1 fit | ok | - |
| 768 | 1 fit | ok | - |

## The institution record, as Prof Mbatha (/admin/institutions/2)

| Width | Cards (scrollWidth / clientWidth) | Page | Under 44px |
|---|---|---|---|
| 900 | 1 fit | ok | - |
| 925 | 1 fit | ok | - |
| 950 | 1 fit | ok | - |
| 975 | 1 fit | ok | - |
| 1000 | 1 fit | ok | - |
| 1024 | 1 fit | ok | - |
| 1025 | 1 fit | ok | - |
| 1050 | 1 fit | ok | - |
| 1075 | 1 fit | ok | - |
| 1100 | 1 fit | ok | - |
| 1125 | 1 fit | ok | - |
| 1150 | 1 fit | ok | - |
| 1151 | 1 fit | ok | - |
| 1280 | 1 fit | ok | - |
| 390 | 1 fit | ok | 0 |
| 640 | 1 fit | ok | 0 |
| 641 | 1 fit | ok | - |
| 768 | 1 fit | ok | - |

## Create institution, as devadmin (/admin/institutions/new)

| Width | Cards (scrollWidth / clientWidth) | Page | Under 44px |
|---|---|---|---|
| 900 | 1 fit | ok | - |
| 925 | 1 fit | ok | - |
| 950 | 1 fit | ok | - |
| 975 | 1 fit | ok | - |
| 1000 | 1 fit | ok | - |
| 1024 | 1 fit | ok | - |
| 1025 | 1 fit | ok | - |
| 1050 | 1 fit | ok | - |
| 1075 | 1 fit | ok | - |
| 1100 | 1 fit | ok | - |
| 1125 | 1 fit | ok | - |
| 1150 | 1 fit | ok | - |
| 1151 | 1 fit | ok | - |
| 1280 | 1 fit | ok | - |
| 390 | 1 fit | ok | 0 |
| 640 | 1 fit | ok | 0 |
| 641 | 1 fit | ok | - |
| 768 | 1 fit | ok | - |

## An audit entry with the largest payload, as devadmin

| Width | Cards (scrollWidth / clientWidth) | Payload region (height / content; max-height; sideways) | Page | Under 44px |
|---|---|---|---|---|
| 900 | 4 fit | 480 / 977; 448px; no | ok | - |
| 925 | 4 fit | 480 / 914; 448px; no | ok | - |
| 950 | 4 fit | 480 / 914; 448px; no | ok | - |
| 975 | 4 fit | 480 / 872; 448px; no | ok | - |
| 1000 | 4 fit | 480 / 851; 448px; no | ok | - |
| 1024 | 4 fit | 480 / 830; 448px; no | ok | - |
| 1025 | 4 fit | 480 / 830; 448px; no | ok | - |
| 1050 | 4 fit | 480 / 809; 448px; no | ok | - |
| 1075 | 4 fit | 480 / 788; 448px; no | ok | - |
| 1100 | 4 fit | 480 / 767; 448px; no | ok | - |
| 1125 | 4 fit | 480 / 767; 448px; no | ok | - |
| 1150 | 4 fit | 480 / 767; 448px; no | ok | - |
| 1151 | 4 fit | 480 / 767; 448px; no | ok | - |
| 1280 | 4 fit | 480 / 704; 448px; no | ok | - |
| 390 | 4 fit | 480 / 1607; 448px; no | ok | 0 |
| 640 | 4 fit | 480 / 935; 448px; no | ok | 0 |
| 641 | 4 fit | 480 / 1670; 448px; no | ok | - |
| 768 | 4 fit | 480 / 1229; 448px; no | ok | - |

## An audit entry, as Prof Mbatha

| Width | Cards (scrollWidth / clientWidth) | Page | Under 44px |
|---|---|---|---|
| 900 | 3 fit | ok | - |
| 925 | 3 fit | ok | - |
| 950 | 3 fit | ok | - |
| 975 | 3 fit | ok | - |
| 1000 | 3 fit | ok | - |
| 1024 | 3 fit | ok | - |
| 1025 | 3 fit | ok | - |
| 1050 | 3 fit | ok | - |
| 1075 | 3 fit | ok | - |
| 1100 | 3 fit | ok | - |
| 1125 | 3 fit | ok | - |
| 1150 | 3 fit | ok | - |
| 1151 | 3 fit | ok | - |
| 1280 | 3 fit | ok | - |
| 390 | 3 fit | ok | 0 |
| 640 | 3 fit | ok | 0 |
| 641 | 3 fit | ok | - |
| 768 | 3 fit | ok | - |

