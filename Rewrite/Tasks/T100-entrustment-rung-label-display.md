# T100 — Entrustment levels render as "Order. Label", which prints "3. 3a" under the v11.1 ladder

**Status:** open
**Surfaced:** 2026-09-17 (anticipated by T098's trap list, now enumerated site by site).
**Severity:** Medium — cosmetic, but it is wrong on the clinician-facing form, the STAR certificate and
the portfolio PDF, i.e. every place a rung is read by a human.

## Symptom

The CPSA v11.1 ladder has six rungs at `Order` 1-6 labelled `1, 2, 3a, 3b, 4, 5`. Every display site that
concatenates the ordinal with the label therefore prints nonsense:

| Stored Order | College rung | Rendered today |
|---|---|---|
| 3 | 3a | `3. 3a` |
| 4 | 3b | `4. 3b` |
| 5 | 4  | `5. 4` |
| 6 | 5  | `6. 5` |

The ordinal was a useful prefix while `Order` and `Label` agreed (the old 5-rung "O-R Scale"). Under any
scale whose labels are not 1..N it is actively misleading — a reader cannot tell which number is the rung.

## Sites

**Six print `"{Order}. {Label}"`:**

- `src/Wombat.Infrastructure/Activities/ActivityReferenceDataService.cs:166-168` — the WBA rating picker.
  `new ActivityCatalogueOption(level.Order.ToString(), level.Order + ". " + level.Label)`. Consumed by
  `ActivityForm.razor:281`, which feeds both the editable `<select>` and the read-only one
  (`ActivityForm.razor:63-78`) — so the same string appears on the entry form and the detail view.
- `src/Wombat.Web/Components/Pages/CommitteeDecisions/ReviewDetail.razor:165` — STAR authorised-level picker.
- `src/Wombat.Web/Components/Pages/Admin/EntrustmentDecisions/Index.razor:73`
- `src/Wombat.Web/Components/Pages/Portfolio/MyAuthorisations.razor:28`
- `src/Wombat.Infrastructure/Reporting/EntrustmentCertificatePdfService.cs:105` — the STAR certificate.
- `src/Wombat.Infrastructure/Reporting/EntrustmentSummaryComponent.cs:54` — the portfolio PDF.

One site already diverges and prints the label alone: `ReviewDetail.razor:198`.

**A further set prints the bare `Order` with no label at all**, which reads wrong for the same reason
(order 5 is the College's rung "4"):

- `Components/Pages/Dashboards/TraineeDashboard.razor:41` and `Components/Pages/Portfolio/MyProgress.razor:35`
  — "Minimum level @item.EffectiveMinimumLevelOrder".
- `TrajectoryChart.razor:155` (y-axis ticks), `:45` (tooltip `Rating @point.Rating`), `:69` (the
  screen-reader table's Rating cell).
- `Components/Pages/Admin/Curricula/CurriculumItemsEdit.razor:74` and `:113` (bare `MinimumLevelOrder`,
  plus the raw stage-map JSON at `:75`/`:117`).

**One site prints the stored `DataJson` integer with no scale mapping whatsoever:**
`src/Wombat.Infrastructure/Reporting/ActivitiesSectionComponent.cs:115-125` via `GetFieldValue` at
`:161-168` — a portfolio PDF prints "Supervision required for this encounter: 5".

**Correct by construction** (separate columns, no concatenation): `Admin/EntrustmentScales/EntrustmentScaleEdit.razor:74`.
**Out of scope** (not `EntrustmentLevel`-backed): MSF answers, averaged as bare ints at
`MsfAggregationService.cs:41-42`.

## Fix

One shared formatter, then the call sites. The label alone is the correct default — the College's own
document refers to rungs as "3a", never as "level 4". Where an ordering hint genuinely helps (the admin
scale editor), keep the ordinal in its own column rather than concatenated.

The bare-ordinal sites need a **label** threaded to them, which means carrying the label on the DTO
(`CurriculumItemDto`, `TraineeDashboardSummaryDto`, the trajectory point) rather than formatting an int
that arrives without its scale. The `ActivitiesSectionComponent` PDF site needs the activity's pinned
schema `scale_key` resolved to a scale before it can print anything meaningful — that one is a genuine
piece of work, not a formatting change, and may be split out.

## Verification

- A trainee's progress page shows the target as `3b`, not `4` or `4. 3b`.
- The rating picker on a CPSA Mini-CEX lists `1, 2, 3a, 3b, 4, 5` — six entries, no ordinal prefixes.
- The STAR certificate and the portfolio PDF print rung labels.
- The 5-rung "O-R Scale" still reads correctly everywhere (labels there are `1`..`5`, so the change is
  invisible on the old scale — which is the regression risk worth a test).

## Related

Lands on the form that [T070] rebuilds, so do it after T070 or expect a merge. Listed in
`T098-epa-v11-adoption.md` trap 5 and phase 2.
