# T043 — Define orphan CSS helper classes

Follow-up from the GUI review sequence (T037–T042). During the cluster audits, several class names were found referenced in `.razor` files but **never defined** in `app.css` or any `.razor.css` isolation file. They rendered as browser-default HTML, which works but looks unstyled.

## Scope

Two categories.

**Category 1 — Define new utilities in `app.css`** for names that express real design intent:

| Class | Markup pattern | Used in |
|---|---|---|
| `.details-list` | `<dl>` with `<div><dt>…</dt><dd>…</dd></div>` rows | ReviewDetail, MyAuthorisations |
| `.detail-list` | `<dl>` with flat sibling `<dt>` / `<dd>` | AuditDetail, RequestDetail |
| `.stack-list` | `<div>` wrapping a vertical stack of cards | ReviewDetail |

Both dl classes are kept because the two files use structurally-different markup (div-wrapped rows vs flat siblings). Forcing one onto the other would require refactoring all 5 admin `<dl>` sites with conditional `@if` branches — high churn for low value.

**Category 2 — Swap to existing defined utilities:**

| From (orphan) | To (defined) | Count |
|---|---|---|
| `stack-card` | `detail-card detail-card--compact` | 3 |
| `detail-grid` | `details-grid` | 4 |
| `detail-section` | `detail-card` | 5 |

All three swaps land on utilities already in `app.css` and already used elsewhere in the codebase (ActivityTypeEdit and others use `detail-card detail-card--compact` for sub-cards; `details-grid` + `detail-card` is the standard detail-page layout).

## Out of scope

- Bootstrap utility drift in AuditDetail + RequestDetail: `text-sm`, `text-muted`, `mt-4`, `mt-1` are undefined but cosmetic — structural rendering is unaffected. Flag as a separate task if a consistency pass is wanted later.
- `plain-list` was swapped to `list-unstyled` in T039, so not in this task.

## Definition of done

- All 12 orphan usages (counted across the 5 affected files) either map to a defined class or have been swapped.
- Zero razor file drift for the 2 admin detail files beyond the mechanical swaps.
- Build clean, tests green.
- Browser-verify at least one file from each affected cluster (committee, portfolio, admin) to confirm the layout looks right with the new CSS.

## Files touched

- `src/Wombat.Web/wwwroot/app.css` (+3 CSS blocks)
- `src/Wombat.Web/Components/Pages/CommitteeDecisions/ReviewDetail.razor` (stack-card × 2)
- `src/Wombat.Web/Components/Pages/CommitteeDecisions/MyReviews.razor` (stack-card × 1)
- `src/Wombat.Web/Components/Pages/Admin/Audit/AuditDetail.razor` (detail-grid, detail-section × 4)
- `src/Wombat.Web/Components/Pages/Admin/DataRights/RequestDetail.razor` (detail-grid, detail-section × 3)
