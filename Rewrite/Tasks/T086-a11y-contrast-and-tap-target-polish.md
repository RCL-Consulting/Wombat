# T086 — A11y polish: muted-text contrast + small tap targets

**Status:** 🟡 OPEN — found 2026-06-04 in Appendix A.4.4 / A.4.1. **LOW** (WCAG AA polish).

## Findings

**(1) Muted secondary text marginally fails AA contrast (WCAG 1.4.3).**
The muted token `#6c757d` (rgb 108,117,125) on the page background `#f8f9fa` (rgb 248,249,250) measures
**4.45:1** — just under the AA 4.5:1 threshold for normal text. The same token on **white** card
surfaces measures 4.69:1 (passes). Affects `.page-subtitle` (and any muted text placed directly on the
page background). Measured live 2026-06-04.

**(2) Some standalone controls below the AA minimum target size (WCAG 2.5.8, AA 2.2 → 24×24px).**
At 360px width, several controls render 21–23px tall: the "Wombat" brand link (23px), the profile/email
link (21px), "Sign out" (23px), and table-row "Disable" buttons (23px). Primary action buttons (Run now,
Run history, Create) are ~38px (pass 24, fail the AAA 44px of 2.5.5).

## Fix outline

- Darken the muted token slightly (e.g. `#686f76`/`#6a7077` → ≥4.5:1 on `#f8f9fa`) **or** restrict
  `.page-subtitle`/muted text to white surfaces. Keep it routed through the CSS custom property (no raw
  hex outside `:root`, per DESIGN.md).
- Bump small standalone controls (icon/text buttons, compact table-action buttons) to `min-height:24px`
  (ideally ~32px) and ensure adequate hit padding.

## Minor UX note (not a11y, optional)

The `/admin/jobs/runs` and `/admin/data-rights` list pages accept `?key=` / `?status=` query strings but
**do not pre-filter on load** — filtering only applies via the on-page Filter form. Tiny inconsistency;
wire the query string into the initial filter state if convenient.
