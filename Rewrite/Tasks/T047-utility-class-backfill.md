# T047 — Backfill Bootstrap-style utility classes in app.css

Follow-up item #3 from the post-T046 backlog: "Remaining Bootstrap utility drift in AuditDetail + RequestDetail. `text-sm`, `text-muted`, `mt-4`, `mt-1` still undefined. Cosmetic — doesn't break structural rendering. Either define the utilities (trivial — one-line rules each) or strip them from the razor files. Deferred from T043."

## Scope check

Backlog called out two files. Full grep surfaced seven:

- `Components/Pages/Profile/DataRights.razor` (user-facing)
- `Components/Pages/Admin/DataRights/RequestDetail.razor`, `RequestsList.razor`
- `Components/Pages/Admin/Audit/AuditDetail.razor`, `AuditList.razor`
- `Components/Pages/Admin/Jobs/ScheduledJobsList.razor`, `ScheduledJobRunsList.razor`

And the scope includes a fifth orphan: `text-danger` (one usage on `ScheduledJobRunsList.razor:69`).

## Decision

**Define, don't strip.** These are small Bootstrap-convention utility names (text-sm, text-muted, mt-1, mt-4, text-danger), used consistently across seven files, and the `app.css` utility section already has sibling utilities (`.mb-3`, `.mt-3`, `.shadow`, `.text-center`, `.font-mono`, `.list-unstyled`, `.muted`, `.visually-hidden`). Adding five one-line rules is the cheapest fix and preserves author intent — the class names already read idiomatically to anyone coming from Bootstrap or Tailwind. Stripping would touch every file and force readers to decide what markup a dangling `text-muted` was meant to achieve.

## Changes

Appended to the Utilities section of `src/Wombat.Web/wwwroot/app.css`, matching the existing literal-rem convention for margins and the existing token-backed convention for colors:

```css
.mt-1 { margin-top: 0.25rem; }
.mt-4 { margin-top: 1.5rem; }

.muted,
.text-muted { color: var(--muted-text); }

.text-sm { font-size: 0.85rem; }
.text-danger { color: var(--danger-color); }
```

Notes:

- `.muted` already existed for the same purpose. Combined selector keeps both names working without duplicating the rule — `text-muted` is the Bootstrap-familiar form that every referenced file uses; `.muted` is the existing Wombat form used on dashboards (e.g. `<p class="muted">`).
- Margin values follow the Bootstrap scale (1 = 0.25rem, 4 = 1.5rem) to match the existing `.mt-3` / `.mb-3`. Spacing tokens exist (`--space-xs` = 0.25rem, `--space-lg` = 1.5rem), but the existing `.mt-3` / `.mb-3` both use literal `1rem` rather than `var(--space-md)`, so sticking with literals here matches the section's conventions. A separate cleanup could migrate all four to tokens in one pass if that's wanted.
- Font sizes: 0.85rem matches the pager/badge/muted-list conventions already scattered through `app.css`. Bootstrap's `.small` is `0.875em`; close enough.

## Verification

Navigated to `/admin/audit/<failed-entry>` (one of the pre-fix draft-create failures from T046 logged a `CreateActivityCommand` failure entry). The detail page now renders all four referenced utilities correctly:

- **`text-sm text-muted mt-1`** on the error-message line under "Failed" in the Event card — renders smaller (0.85rem), muted grey, with a 0.25rem top offset.
- **`mt-4`** on the `Payload (raw JSON)` detail-card — visibly separated from the Event/Actor grid above by 1.5rem.
- The rest of the page (details-grid + detail-card + detail-list from T043) still composes correctly.

Bonus finding: the Payload JSON in that failed entry shows `"principal": "[PRINCIPAL]"` — T045's audit-serializer cycle fix visible in persisted audit data.

## Out of scope

- Migrating `.mt-3` / `.mb-3` to use `var(--space-*)` tokens. Out of scope; same convention as the existing section.
- Defining every Bootstrap utility preemptively. Only the classes actually referenced in razor files were added.
- No tests added — pure CSS addition with zero code path impact.

## Definition of done

- Every referenced `text-sm`, `text-muted`, `mt-1`, `mt-4`, `text-danger` in `src/Wombat.Web/Components/Pages/**/*.razor` resolves to a defined CSS rule.
- Spot-verification: one page rendering each of the four classes visually applies the expected styling.
- Build clean, Web tests pass.

## Files touched

- `src/Wombat.Web/wwwroot/app.css`
- `Rewrite/Tasks/T047-utility-class-backfill.md` (this file)
- `Rewrite/current_state.md`
