# Flow 16: the build sweep (T394 step 6, round 3 decisions 1 and 2)

Measured 2026-11-05 in Chrome (Playwright, headless, viewport height 800) against `t394` published to :5182 on
`wombat_scenario_t394_sweep`, a restore of the T392 replay end state (`recovery/scenario-t392-post-actA.dump`). At each
width, every `.table-container`: its table's `scrollWidth` against the container's `clientWidth`; and the page's
`documentElement.scrollWidth` against its `clientWidth`. Pass: table ≤ container, page = its own width.

**First pass: 1 page failed.** The Administrator's Curricula (four rows, five columns) was 756 px in a 717 px container at
1025 and 742 px at 1050, above its 1024 px stack edge. **Fix:** `.curricula-page` moved from the 1024 px stack to the
1100 px one the items use (app.css, flow 16's shared part; DESIGN.md's section-order line). **Second pass, below: 0 fails.**
Decision 2: the items' smallest spare between 1101 and 1150 px is 0 (the table fills its container exactly), accepted.
Not measured: Colleges' heavy five (R3-B16), which needs a scratch copy with two more Colleges.


## Colleges, as devadmin (/admin/colleges, 3 rows)

| Width | Table | Container | Spare | Page |
|---|---|---|---|---|
| 900 | 592 | 592 | 0 | ok |
| 925 | 617 | 617 | 0 | ok |
| 950 | 642 | 642 | 0 | ok |
| 975 | 667 | 667 | 0 | ok |
| 1000 | 692 | 692 | 0 | ok |
| 1025 | 717 | 717 | 0 | ok |
| 1050 | 742 | 742 | 0 | ok |
| 1075 | 767 | 767 | 0 | ok |
| 1100 | 792 | 792 | 0 | ok |

## Curricula, as devadmin (/admin/curricula, 4 rows)

| Width | Table | Container | Spare | Page |
|---|---|---|---|---|
| 900 | 592 | 592 | 0 | ok |
| 925 | 617 | 617 | 0 | ok |
| 950 | 642 | 642 | 0 | ok |
| 975 | 667 | 667 | 0 | ok |
| 1000 | 692 | 692 | 0 | ok |
| 1025 | 717 | 717 | 0 | ok |
| 1050 | 742 | 742 | 0 | ok |
| 1075 | 767 | 767 | 0 | ok |
| 1100 | 792 | 792 | 0 | ok |

## Items 11.1, as devadmin (/admin/curricula/2/items, 16 rows)

| Width | Table | Container | Spare | Page |
|---|---|---|---|---|
| 900 | 592 | 592 | 0 | ok |
| 925 | 617 | 617 | 0 | ok |
| 950 | 642 | 642 | 0 | ok |
| 975 | 667 | 667 | 0 | ok |
| 1000 | 692 | 692 | 0 | ok |
| 1025 | 717 | 717 | 0 | ok |
| 1050 | 742 | 742 | 0 | ok |
| 1075 | 767 | 767 | 0 | ok |
| 1100 | 792 | 792 | 0 | ok |
| 1101 | 793 | 793 | 0 | ok |
| 1125 | 817 | 817 | 0 | ok |
| 1150 | 842 | 842 | 0 | ok |

## EPAs, as devadmin (/admin/epas, 18 rows)

| Width | Table | Container | Spare | Page |
|---|---|---|---|---|
| 900 | 592 | 592 | 0 | ok |
| 925 | 617 | 617 | 0 | ok |
| 950 | 642 | 642 | 0 | ok |
| 975 | 667 | 667 | 0 | ok |
| 1000 | 692 | 692 | 0 | ok |
| 1025 | 717 | 717 | 0 | ok |
| 1050 | 742 | 742 | 0 | ok |
| 1075 | 767 | 767 | 0 | ok |
| 1100 | 792 | 792 | 0 | ok |

## Tree, as Dr Kruger (/admin/colleges/2/specialities, 2 rows)

| Width | Table | Container | Spare | Page |
|---|---|---|---|---|
| 900 | 592 | 592 | 0 | ok |
| 925 | 617 | 617 | 0 | ok |
| 950 | 642 | 642 | 0 | ok |
| 975 | 667 | 667 | 0 | ok |
| 1000 | 692 | 692 | 0 | ok |
| 1025 | 717 | 717 | 0 | ok |
| 1050 | 742 | 742 | 0 | ok |
| 1075 | 767 | 767 | 0 | ok |
| 1100 | 792 | 792 | 0 | ok |

## Curricula, as Dr Kruger (/admin/curricula, 3 rows)

| Width | Table | Container | Spare | Page |
|---|---|---|---|---|
| 900 | 592 | 592 | 0 | ok |
| 925 | 617 | 617 | 0 | ok |
| 950 | 642 | 642 | 0 | ok |
| 975 | 667 | 667 | 0 | ok |
| 1000 | 692 | 692 | 0 | ok |
| 1025 | 717 | 717 | 0 | ok |
| 1050 | 742 | 742 | 0 | ok |
| 1075 | 767 | 767 | 0 | ok |
| 1100 | 792 | 792 | 0 | ok |

## Items 11.1, as Dr Kruger (/admin/curricula/2/items, 15 rows)

| Width | Table | Container | Spare | Page |
|---|---|---|---|---|
| 900 | 592 | 592 | 0 | ok |
| 925 | 617 | 617 | 0 | ok |
| 950 | 642 | 642 | 0 | ok |
| 975 | 667 | 667 | 0 | ok |
| 1000 | 692 | 692 | 0 | ok |
| 1025 | 717 | 717 | 0 | ok |
| 1050 | 742 | 742 | 0 | ok |
| 1075 | 767 | 767 | 0 | ok |
| 1100 | 792 | 792 | 0 | ok |
| 1101 | 793 | 793 | 0 | ok |
| 1125 | 817 | 817 | 0 | ok |
| 1150 | 842 | 842 | 0 | ok |

## EPAs, as Dr Kruger (/admin/epas, 16 rows)

| Width | Table | Container | Spare | Page |
|---|---|---|---|---|
| 900 | 592 | 592 | 0 | ok |
| 925 | 617 | 617 | 0 | ok |
| 950 | 642 | 642 | 0 | ok |
| 975 | 667 | 667 | 0 | ok |
| 1000 | 692 | 692 | 0 | ok |
| 1025 | 717 | 717 | 0 | ok |
| 1050 | 742 | 742 | 0 | ok |
| 1075 | 767 | 767 | 0 | ok |
| 1100 | 792 | 792 | 0 | ok |
