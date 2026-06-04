# T085 — TrajectoryChart has no text/tabular fallback for screen readers

**Status:** 🟠 OPEN — found 2026-06-04 in Appendix A.4.3. **MODERATE** (WCAG 2.1 SC 1.1.1).

## Problem (F-A4-1)

`TrajectoryChart.razor` renders an inline `<svg role="img" aria-label="Rating trajectory for {EPA}">`
with a per-point `<title>` (date · rating). There is **no text or tabular equivalent** of the plotted
data. A screen reader announces only the chart's *label* (which EPA), not the actual observation
values/dates — and SVG `<title>`-per-`<circle>` support across screen readers is inconsistent. So the
information conveyed by the chart (the rating trajectory) is not available as text → WCAG 1.1.1
(Non-text Content).

**Verified (2026-06-04)** on `/committee/reviews/4`: two trajectory charts (PAED-001, PAED-010), each
`role=img` + aria-label, `hasTableSibling=false` (no adjacent data table).

## Fix outline

Alongside each `TrajectoryChart`, render a `visually-hidden` data table (or a `<figure>`+`<figcaption>`
with a `<table>`) listing each observation's date, rating, and source — or set a richer `aria-label`
that summarises the series (e.g. "PAED-001: 2 observations — 12 Apr 2026 rated 2, 03 May 2026 rated 3").
A visually-hidden table is the more robust, WCAG-clean option and reuses the existing `.visually-hidden`
utility.

## Notes

Heading structure on the review page is otherwise sound (H1 → H3 section cards → H4 items; minor H1→H3
skip with no H2 — cosmetic). Required-field markers ARE announced (FormField renders a `.visually-hidden`
"required"). This task is scoped to the chart fallback only.
