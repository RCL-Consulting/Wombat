# Flow 17: the build sweep (T398, build-lanes.md § "The sweep")

Measured 2026-11-10 in Chrome (Playwright, headless, viewport height 800; 844 at 768 and below) against `t398` published
to :5183 on `wombat_scenario_t398_sweep`, a restore of the T394 replay's end state (`recovery/scenario-t394-post-actA.dump`).
Signed in as Prof Mbatha (`mbatha@kgk.wombat.local`), Dr Kruger (`kruger@cmsa.wombat.local`) and devadmin. At each width:
every `.table-container`, its table's `scrollWidth` against the container's `clientWidth`, and the table's narrowest
(`width: min-content`) against the container, which is the spare the canvas reports; the card (the tabpanel) and every
`.detail-card`/`.form-container`, `scrollWidth` against `clientWidth`; the tab bar (tabs, rows, `scrollWidth` against
`clientWidth`); each field row (its buttons on one line, beside or under the label, never over it, the label not clipped);
the page's `documentElement.scrollWidth` against its `clientWidth`; below 641px every visible control in `main` under
44px. Pass: nothing wider than its box, one row of tabs, page = its own width, no control under 44px below 641px.

**First pass: 2 faults.**
- **The writer's tab bar at 641px**: from 641px the sidebar takes 306px, so the five tabs had 335px for the 416px their
  labels need (C1's equal tracks applied only below 641px); the page scrolled sideways by 57px at 641 (698 in 641), up
  to about 720px. A reader's four tabs fit (335 in 335).
- **Mini-CEX (Paediatrics)'s "Moves, as saved"** (a reader's Workflow): its four moves need 578px at their narrowest
  (six columns of single words, 32px of padding each), and from 901 to 935px the card gave 543 to 577 (1px scan); 925
  was 578 in 567. KGK's two moves need 486 and fit.
- A third, synthetic: a count with its word ("2 problems", R1) in each of four tabs took the writer's bar to 710px, past
  the column at 640px and from 641 to 1000px (85 to 16px over). Not a state this database holds; measured by adding the
  builder's own badge markup to the tabs and taking it out again.

**Fixes** (`app.css`, DESIGN.md amended):
- C1 and R1 (lane C, under Builder and in "Flow 17: the builder"): the tracks and the count's word go by the page's
  column, not the viewport. `.activity-type-page` is a size container (`builder-page`); the tabs take equal tracks where
  the column is under 34rem (544px: below 641px, as before, and from 641 to about 850px), and a count's word is visually
  hidden where it is under 46rem (736px: below 641px, as before, and up to about 1040px). Above those, as built. So at 768
  the writer's tabs are now equal tracks (92px each), where the boards drew them as wide as their labels.
- The moves table (lane B's `.workflow-readout`) joins the catalogue's 1024px stack in flow 16's shared part (in all seven
  selector lists), so it is a table from 1025px, where it has 667px for its 578.

**Second pass, below: 0 fails.** Then a 1px scan of the tab bar from 360 to 1300px on both types, as rendered and with a
"2 problems" count added to one, two, three and four tabs: 0 fails (one row, inside the column, page unscrolled, every tab
44px). And a 1px scan of the builder's tables from 901 to 1200px on Mini-CEX (Paediatrics) Workflow and Details and KGK
Workflow and Publish: 0 fails; the moves table's smallest spare, at its narrowest, is 89px (1025px), the versions table's
260px.

**The smallest spare** (a table's narrowest against its container, laid out as a table):
- The list: Prof Mbatha 46px, Dr Kruger 55px, devadmin 40px, each at 1025px (the canvas said about 35 and 30). Stacked to
  1024px, a stacked row's narrowest is 254px.
- The builder: KGK's moves 181px and versions 398px at 1025; Mini-CEX's moves 89px at 1025. The card and the bar are 0
  everywhere (they fill their column); the editor rail and preview stand one above the other to 1115px and side by side
  from 1116 (C2's container), with nothing over.
- Field rows: every row's buttons on one line, under the label (S2 lays a row out as a column: label, meta, buttons), never
  over it, no label clipped; a label takes two lines at 390 and 641 on Mini-CEX (Paediatrics), one elsewhere.

**Every column at 1280** (the table at the end): no name past two lines (one name takes two, "Portfolio and Logbook Review
(Paediatrics)", for all three), no one-word piece broken, no band beside a name (0px). Not a width fault, for the review:
the action column is 224px for one 60px Edit or View (round 2 called it the table's slack), while Set by wraps its scope
line, "Speciality · Paediatrics" or "Speciality · General Medicine", to two lines on nearly every row at all three widths,
and Dr Kruger's and devadmin's Published and Draft wrap "Not published" and "Saved 2026-11-05" to two lines.

**After the fix pass (pages review S1; inferred from the CSS, not measured).** The list's Actions column now carries
`col-actions--single` (`width: auto`) in place of `.col-actions`' 12rem, so it asks for its one button's width (about
92px with the cell's 32px of padding) where it held 224px. Under the auto table layout the 132px it gives back goes to the
columns whose content wraps, in proportion to what each still wants: at 1280 (a 972px table) Set by's scope line
("Speciality · Paediatrics", "Speciality · General Medicine") and Dr Kruger's and devadmin's "Not published" and "Saved
2026-11-05" should come back to one line on most rows, and the one two-line name may stay two. At 1025 the table's
narrowest falls by the same 132px (no column is held at 12rem any longer), so the spare grows from 40 to 55px to about
170 to 190px; the 1024px stack is unchanged. The next replay (step 7) measures every column at 1280 again.

Not measured: a real refusal state (the counts were synthetic, above); the list's and builder's other tabs at 900-1150
(Details and Credit were measured at 390, 640, 641 and 768 only; the brief names Form, Workflow and Publish); Dr Kruger
and devadmin on the builder (the brief names Prof Mbatha).

## The list, as Prof Mbatha (/admin/activity-types, 23 rows)

| Width | Laid out | Table | Container | Spare | Narrowest table | Spare at narrowest | Page | Under 44px |
|---|---|---|---|---|---|---|---|---|
| 900 | stacked | 592 | 592 | 0 | 254 | 338 | ok | - |
| 925 | stacked | 617 | 617 | 0 | 254 | 363 | ok | - |
| 950 | stacked | 642 | 642 | 0 | 254 | 388 | ok | - |
| 975 | stacked | 667 | 667 | 0 | 254 | 413 | ok | - |
| 1000 | stacked | 692 | 692 | 0 | 254 | 438 | ok | - |
| 1025 | table | 717 | 717 | 0 | 671 | 46 | ok | - |
| 1050 | table | 742 | 742 | 0 | 671 | 71 | ok | - |
| 1075 | table | 767 | 767 | 0 | 671 | 96 | ok | - |
| 1100 | table | 792 | 792 | 0 | 671 | 121 | ok | - |
| 390 | stacked | 356 | 356 | 0 | 254 | 102 | ok | 0 |
| 640 | stacked | 606 | 606 | 0 | 254 | 352 | ok | 0 |
| 641 | stacked | 333 | 333 | 0 | 254 | 79 | ok | - |
| 768 | stacked | 460 | 460 | 0 | 254 | 206 | ok | - |

## The list, as Dr Kruger (/admin/activity-types, 23 rows)

| Width | Laid out | Table | Container | Spare | Narrowest table | Spare at narrowest | Page | Under 44px |
|---|---|---|---|---|---|---|---|---|
| 900 | stacked | 592 | 592 | 0 | 254 | 338 | ok | - |
| 925 | stacked | 617 | 617 | 0 | 254 | 363 | ok | - |
| 950 | stacked | 642 | 642 | 0 | 254 | 388 | ok | - |
| 975 | stacked | 667 | 667 | 0 | 254 | 413 | ok | - |
| 1000 | stacked | 692 | 692 | 0 | 254 | 438 | ok | - |
| 1025 | table | 717 | 717 | 0 | 662 | 55 | ok | - |
| 1050 | table | 742 | 742 | 0 | 662 | 80 | ok | - |
| 1075 | table | 767 | 767 | 0 | 662 | 105 | ok | - |
| 1100 | table | 792 | 792 | 0 | 662 | 130 | ok | - |
| 390 | stacked | 356 | 356 | 0 | 254 | 102 | ok | 0 |
| 640 | stacked | 606 | 606 | 0 | 254 | 352 | ok | 0 |
| 641 | stacked | 333 | 333 | 0 | 254 | 79 | ok | - |
| 768 | stacked | 460 | 460 | 0 | 254 | 206 | ok | - |

## The list, as devadmin (/admin/activity-types, 24 rows)

| Width | Laid out | Table | Container | Spare | Narrowest table | Spare at narrowest | Page | Under 44px |
|---|---|---|---|---|---|---|---|---|
| 900 | stacked | 592 | 592 | 0 | 254 | 338 | ok | - |
| 925 | stacked | 617 | 617 | 0 | 254 | 363 | ok | - |
| 950 | stacked | 642 | 642 | 0 | 254 | 388 | ok | - |
| 975 | stacked | 667 | 667 | 0 | 254 | 413 | ok | - |
| 1000 | stacked | 692 | 692 | 0 | 254 | 438 | ok | - |
| 1025 | table | 717 | 717 | 0 | 677 | 40 | ok | - |
| 1050 | table | 742 | 742 | 0 | 677 | 65 | ok | - |
| 1075 | table | 767 | 767 | 0 | 677 | 90 | ok | - |
| 1100 | table | 792 | 792 | 0 | 677 | 115 | ok | - |
| 390 | stacked | 356 | 356 | 0 | 254 | 102 | ok | 0 |
| 640 | stacked | 606 | 606 | 0 | 254 | 352 | ok | 0 |
| 641 | stacked | 333 | 333 | 0 | 254 | 79 | ok | - |
| 768 | stacked | 460 | 460 | 0 | 254 | 206 | ok | - |

## The builder, KGK Teaching Session Log, Form (Prof Mbatha, a writer)

| Width | Card | Tables (laid out: table / container; narrowest) | Tabs (n, rows, bar / room) | Field rows (buttons on one line; beside / under the label; over it) | Page | Under 44px |
|---|---|---|---|---|---|---|
| 900 | 592 / 592 | none | 5, 1, 594 / 594 | 6 of 6; 0 / 6; 0 | ok | - |
| 925 | 617 / 617 | none | 5, 1, 619 / 619 | 6 of 6; 0 / 6; 0 | ok | - |
| 950 | 642 / 642 | none | 5, 1, 644 / 644 | 6 of 6; 0 / 6; 0 | ok | - |
| 975 | 667 / 667 | none | 5, 1, 669 / 669 | 6 of 6; 0 / 6; 0 | ok | - |
| 1000 | 692 / 692 | none | 5, 1, 694 / 694 | 6 of 6; 0 / 6; 0 | ok | - |
| 1025 | 717 / 717 | none | 5, 1, 719 / 719 | 6 of 6; 0 / 6; 0 | ok | - |
| 1050 | 742 / 742 | none | 5, 1, 744 / 744 | 6 of 6; 0 / 6; 0 | ok | - |
| 1075 | 767 / 767 | none | 5, 1, 769 / 769 | 6 of 6; 0 / 6; 0 | ok | - |
| 1100 | 792 / 792 | none | 5, 1, 794 / 794 | 6 of 6; 0 / 6; 0 | ok | - |
| 1115 | 807 / 807 | none | 5, 1, 809 / 809 | 6 of 6; 0 / 6; 0 | ok | - |
| 1116 | 808 / 808 | none | 5, 1, 810 / 810 | 6 of 6; 0 / 6; 0 | ok | - |
| 1125 | 817 / 817 | none | 5, 1, 819 / 819 | 6 of 6; 0 / 6; 0 | ok | - |
| 1150 | 842 / 842 | none | 5, 1, 844 / 844 | 6 of 6; 0 / 6; 0 | ok | - |
| 390 | 356 / 356 | none | 5, 1, 358 / 358 | 6 of 6; 0 / 6; 0 | ok | 0 |
| 640 | 606 / 606 | none | 5, 1, 608 / 608 | 6 of 6; 0 / 6; 0 | ok | 0 |
| 641 | 333 / 333 | none | 5, 1, 335 / 335 | 6 of 6; 0 / 6; 0 | ok | - |
| 768 | 460 / 460 | none | 5, 1, 462 / 462 | 6 of 6; 0 / 6; 0 | ok | - |

## The builder, KGK Teaching Session Log, Workflow (Prof Mbatha, a writer)

| Width | Card | Tables (laid out: table / container; narrowest) | Tabs (n, rows, bar / room) | Field rows (buttons on one line; beside / under the label; over it) | Page | Under 44px |
|---|---|---|---|---|---|---|
| 900 | 592 / 592 | Moves, as saved: stacked 542 / 542; 98 | 5, 1, 594 / 594 | none shown | ok | - |
| 925 | 617 / 617 | Moves, as saved: stacked 567 / 567; 98 | 5, 1, 619 / 619 | none shown | ok | - |
| 950 | 642 / 642 | Moves, as saved: stacked 592 / 592; 98 | 5, 1, 644 / 644 | none shown | ok | - |
| 975 | 667 / 667 | Moves, as saved: stacked 617 / 617; 98 | 5, 1, 669 / 669 | none shown | ok | - |
| 1000 | 692 / 692 | Moves, as saved: stacked 642 / 642; 98 | 5, 1, 694 / 694 | none shown | ok | - |
| 1025 | 717 / 717 | Moves, as saved: table 667 / 667; 486 | 5, 1, 719 / 719 | none shown | ok | - |
| 1050 | 742 / 742 | Moves, as saved: table 692 / 692; 486 | 5, 1, 744 / 744 | none shown | ok | - |
| 1075 | 767 / 767 | Moves, as saved: table 717 / 717; 486 | 5, 1, 769 / 769 | none shown | ok | - |
| 1100 | 792 / 792 | Moves, as saved: table 742 / 742; 486 | 5, 1, 794 / 794 | none shown | ok | - |
| 1115 | 807 / 807 | Moves, as saved: table 757 / 757; 486 | 5, 1, 809 / 809 | none shown | ok | - |
| 1116 | 808 / 808 | Moves, as saved: table 758 / 758; 486 | 5, 1, 810 / 810 | none shown | ok | - |
| 1125 | 817 / 817 | Moves, as saved: table 767 / 767; 486 | 5, 1, 819 / 819 | none shown | ok | - |
| 1150 | 842 / 842 | Moves, as saved: table 792 / 792; 486 | 5, 1, 844 / 844 | none shown | ok | - |
| 390 | 356 / 356 | Moves, as saved: stacked 306 / 306; 98 | 5, 1, 358 / 358 | none shown | ok | 0 |
| 640 | 606 / 606 | Moves, as saved: stacked 556 / 556; 98 | 5, 1, 608 / 608 | none shown | ok | 0 |
| 641 | 333 / 333 | Moves, as saved: stacked 283 / 283; 98 | 5, 1, 335 / 335 | none shown | ok | - |
| 768 | 460 / 460 | Moves, as saved: stacked 410 / 410; 98 | 5, 1, 462 / 462 | none shown | ok | - |

## The builder, KGK Teaching Session Log, Publish (Prof Mbatha, a writer)

| Width | Card | Tables (laid out: table / container; narrowest) | Tabs (n, rows, bar / room) | Field rows (buttons on one line; beside / under the label; over it) | Page | Under 44px |
|---|---|---|---|---|---|---|
| 900 | 592 / 592 | Published versions: stacked 542 / 542; 105 | 5, 1, 594 / 594 | none shown | ok | - |
| 925 | 617 / 617 | Published versions: table 567 / 567; 269 | 5, 1, 619 / 619 | none shown | ok | - |
| 950 | 642 / 642 | Published versions: table 592 / 592; 269 | 5, 1, 644 / 644 | none shown | ok | - |
| 975 | 667 / 667 | Published versions: table 617 / 617; 269 | 5, 1, 669 / 669 | none shown | ok | - |
| 1000 | 692 / 692 | Published versions: table 642 / 642; 269 | 5, 1, 694 / 694 | none shown | ok | - |
| 1025 | 717 / 717 | Published versions: table 667 / 667; 269 | 5, 1, 719 / 719 | none shown | ok | - |
| 1050 | 742 / 742 | Published versions: table 692 / 692; 269 | 5, 1, 744 / 744 | none shown | ok | - |
| 1075 | 767 / 767 | Published versions: table 717 / 717; 269 | 5, 1, 769 / 769 | none shown | ok | - |
| 1100 | 792 / 792 | Published versions: table 742 / 742; 269 | 5, 1, 794 / 794 | none shown | ok | - |
| 1115 | 807 / 807 | Published versions: table 757 / 757; 269 | 5, 1, 809 / 809 | none shown | ok | - |
| 1116 | 808 / 808 | Published versions: table 758 / 758; 269 | 5, 1, 810 / 810 | none shown | ok | - |
| 1125 | 817 / 817 | Published versions: table 767 / 767; 269 | 5, 1, 819 / 819 | none shown | ok | - |
| 1150 | 842 / 842 | Published versions: table 792 / 792; 269 | 5, 1, 844 / 844 | none shown | ok | - |
| 390 | 356 / 356 | Published versions: stacked 306 / 306; 105 | 5, 1, 358 / 358 | none shown | ok | 0 |
| 640 | 606 / 606 | Published versions: stacked 556 / 556; 105 | 5, 1, 608 / 608 | none shown | ok | 0 |
| 641 | 333 / 333 | Published versions: stacked 283 / 283; 105 | 5, 1, 335 / 335 | none shown | ok | - |
| 768 | 460 / 460 | Published versions: stacked 410 / 410; 105 | 5, 1, 462 / 462 | none shown | ok | - |

## The builder, KGK Teaching Session Log, Details (Prof Mbatha, a writer)

| Width | Card | Tables (laid out: table / container; narrowest) | Tabs (n, rows, bar / room) | Field rows (buttons on one line; beside / under the label; over it) | Page | Under 44px |
|---|---|---|---|---|---|---|
| 390 | 356 / 356 | none | 5, 1, 358 / 358 | none shown | ok | 0 |
| 640 | 606 / 606 | none | 5, 1, 608 / 608 | none shown | ok | 0 |
| 641 | 333 / 333 | none | 5, 1, 335 / 335 | none shown | ok | - |
| 768 | 460 / 460 | none | 5, 1, 462 / 462 | none shown | ok | - |

## The builder, Mini-CEX (Paediatrics), Form (Prof Mbatha, a reader)

| Width | Card | Tables (laid out: table / container; narrowest) | Tabs (n, rows, bar / room) | Field rows (buttons on one line; beside / under the label; over it) | Page | Under 44px |
|---|---|---|---|---|---|---|
| 900 | 592 / 592 | none | 4, 1, 594 / 594 | 10 of 10; 0 / 10; 0 | ok | - |
| 925 | 617 / 617 | none | 4, 1, 619 / 619 | 10 of 10; 0 / 10; 0 | ok | - |
| 950 | 642 / 642 | none | 4, 1, 644 / 644 | 10 of 10; 0 / 10; 0 | ok | - |
| 975 | 667 / 667 | none | 4, 1, 669 / 669 | 10 of 10; 0 / 10; 0 | ok | - |
| 1000 | 692 / 692 | none | 4, 1, 694 / 694 | 10 of 10; 0 / 10; 0 | ok | - |
| 1025 | 717 / 717 | none | 4, 1, 719 / 719 | 10 of 10; 0 / 10; 0 | ok | - |
| 1050 | 742 / 742 | none | 4, 1, 744 / 744 | 10 of 10; 0 / 10; 0 | ok | - |
| 1075 | 767 / 767 | none | 4, 1, 769 / 769 | 10 of 10; 0 / 10; 0 | ok | - |
| 1100 | 792 / 792 | none | 4, 1, 794 / 794 | 10 of 10; 0 / 10; 0 | ok | - |
| 1115 | 807 / 807 | none | 4, 1, 809 / 809 | 10 of 10; 0 / 10; 0 | ok | - |
| 1116 | 808 / 808 | none | 4, 1, 810 / 810 | 10 of 10; 0 / 10; 0 | ok | - |
| 1125 | 817 / 817 | none | 4, 1, 819 / 819 | 10 of 10; 0 / 10; 0 | ok | - |
| 1150 | 842 / 842 | none | 4, 1, 844 / 844 | 10 of 10; 0 / 10; 0 | ok | - |
| 390 | 356 / 356 | none | 4, 1, 358 / 358 | 10 of 10; 0 / 10; 0 | ok | 0 |
| 640 | 606 / 606 | none | 4, 1, 608 / 608 | 10 of 10; 0 / 10; 0 | ok | 0 |
| 641 | 333 / 333 | none | 4, 1, 335 / 335 | 10 of 10; 0 / 10; 0 | ok | - |
| 768 | 460 / 460 | none | 4, 1, 462 / 462 | 10 of 10; 0 / 10; 0 | ok | - |

## The builder, Mini-CEX (Paediatrics), Workflow (Prof Mbatha, a reader)

| Width | Card | Tables (laid out: table / container; narrowest) | Tabs (n, rows, bar / room) | Field rows (buttons on one line; beside / under the label; over it) | Page | Under 44px |
|---|---|---|---|---|---|---|
| 900 | 592 / 592 | Moves, as saved: stacked 542 / 542; 106 | 4, 1, 594 / 594 | none shown | ok | - |
| 925 | 617 / 617 | Moves, as saved: stacked 567 / 567; 106 | 4, 1, 619 / 619 | none shown | ok | - |
| 950 | 642 / 642 | Moves, as saved: stacked 592 / 592; 106 | 4, 1, 644 / 644 | none shown | ok | - |
| 975 | 667 / 667 | Moves, as saved: stacked 617 / 617; 106 | 4, 1, 669 / 669 | none shown | ok | - |
| 1000 | 692 / 692 | Moves, as saved: stacked 642 / 642; 106 | 4, 1, 694 / 694 | none shown | ok | - |
| 1025 | 717 / 717 | Moves, as saved: table 667 / 667; 578 | 4, 1, 719 / 719 | none shown | ok | - |
| 1050 | 742 / 742 | Moves, as saved: table 692 / 692; 578 | 4, 1, 744 / 744 | none shown | ok | - |
| 1075 | 767 / 767 | Moves, as saved: table 717 / 717; 578 | 4, 1, 769 / 769 | none shown | ok | - |
| 1100 | 792 / 792 | Moves, as saved: table 742 / 742; 578 | 4, 1, 794 / 794 | none shown | ok | - |
| 1115 | 807 / 807 | Moves, as saved: table 757 / 757; 578 | 4, 1, 809 / 809 | none shown | ok | - |
| 1116 | 808 / 808 | Moves, as saved: table 758 / 758; 578 | 4, 1, 810 / 810 | none shown | ok | - |
| 1125 | 817 / 817 | Moves, as saved: table 767 / 767; 578 | 4, 1, 819 / 819 | none shown | ok | - |
| 1150 | 842 / 842 | Moves, as saved: table 792 / 792; 578 | 4, 1, 844 / 844 | none shown | ok | - |
| 390 | 356 / 356 | Moves, as saved: stacked 306 / 306; 106 | 4, 1, 358 / 358 | none shown | ok | 0 |
| 640 | 606 / 606 | Moves, as saved: stacked 556 / 556; 106 | 4, 1, 608 / 608 | none shown | ok | 0 |
| 641 | 333 / 333 | Moves, as saved: stacked 283 / 283; 106 | 4, 1, 335 / 335 | none shown | ok | - |
| 768 | 460 / 460 | Moves, as saved: stacked 410 / 410; 106 | 4, 1, 462 / 462 | none shown | ok | - |

## The builder, Mini-CEX (Paediatrics), Details (Prof Mbatha, a reader)

| Width | Card | Tables (laid out: table / container; narrowest) | Tabs (n, rows, bar / room) | Field rows (buttons on one line; beside / under the label; over it) | Page | Under 44px |
|---|---|---|---|---|---|---|
| 390 | 356 / 356 | Published versions: stacked 306 / 306; 105 | 4, 1, 358 / 358 | none shown | ok | 0 |
| 640 | 606 / 606 | Published versions: stacked 556 / 556; 105 | 4, 1, 608 / 608 | none shown | ok | 0 |
| 641 | 333 / 333 | Published versions: stacked 283 / 283; 105 | 4, 1, 335 / 335 | none shown | ok | - |
| 768 | 460 / 460 | Published versions: stacked 410 / 410; 105 | 4, 1, 462 / 462 | none shown | ok | - |

## Every column of the list at 1280

| Person | Column | Width | Most lines in a cell | Name's lines | One-word pieces broken | Band beside the widest line |
|---|---|---|---|---|---|---|
| Prof Mbatha | Name | 309 | 4 | 2 | none | 0 |
| Prof Mbatha | Set by | 195 | 4 | - | none | 0 |
| Prof Mbatha | Published | 101 | 1 | - | none | 8 |
| Prof Mbatha | Draft | 68 | 1 | - | none | 0 |
| Prof Mbatha | Status | 75 | 1 | - | none | 2 |
| Prof Mbatha | Actions | 224 | 2 | - | none | 0 |
| Dr Kruger | Name | 295 | 4 | 2 | none | 0 |
| Dr Kruger | Set by | 148 | 4 | - | none | 0 |
| Dr Kruger | Published | 115 | 2 | - | none | 16 |
| Dr Kruger | Draft | 115 | 2 | - | none | 2 |
| Dr Kruger | Status | 75 | 1 | - | none | 2 |
| Dr Kruger | Actions | 224 | 2 | - | none | 0 |
| devadmin | Name | 290 | 4 | 2 | none | 0 |
| devadmin | Set by | 159 | 4 | - | none | 0 |
| devadmin | Published | 113 | 2 | - | none | 14 |
| devadmin | Draft | 111 | 2 | - | none | 5 |
| devadmin | Status | 75 | 1 | - | none | 2 |
| devadmin | Actions | 224 | 2 | - | none | 0 |
