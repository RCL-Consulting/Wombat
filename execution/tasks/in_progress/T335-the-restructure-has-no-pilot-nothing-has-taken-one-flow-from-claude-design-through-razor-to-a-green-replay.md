---
id: T335
title: The restructure has no pilot: nothing has taken one flow from Claude Design through Razor to a green replay
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-09-27
started: 2026-09-27
---

# T335 — The restructure has no pilot: nothing has taken one flow from Claude Design through Razor to a green replay

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. W-008 chose a restructure of 80 page templates, 15 emails and the PDFs. The path from a Claude Design
canvas to Razor components under `DESIGN.md`, the CSP and the Blazor render modes has never been walked, and the brief
rests on assumptions about the tool it could not verify (BRIEF § 2.2: whether `/design` reads the repo's design system;
upload formats; how much fidelity survives the translation). Running all 18 flows on those assumptions would repeat any
mistake 18 times.
**Surfaced:** 2026-09-27, the operator's choice of a restructure and "set up the pilot".

## What to build

A pilot on `design/flows/01-shell.md` (the shell: sign-in landing, navigation, role switching, the system states), which
every page uses:
1. **Set-up:** the design system chosen and in Claude Design; the flow brief restated for a restructure; the upload set
   staged from tracked files only; `design/pilot/README.md`, the step-by-step.
2. **Design:** 2–3 structural variations as wireframes, then the chosen one at fidelity, with its states (empty, error,
   loading, reconnect, narrow).
3. **Build:** Claude Code implements the chosen design in Razor and `app.css`, amending `DESIGN.md` and its Design tests
   deliberately (W-008).
4. **Accept:** replay flow 01's runbook steps; T294's guard, bUnit and the suites green; the baseline re-captured.
5. **Learn:** correct `BRIEF.md` and the flow template with what the pilot taught, before the next flow.

## Verification

- [x] Set-up complete: the design system exists in Claude Design; `design/pilot/README.md` names every step, upload and
  check; flow 01 asks for a restructure — 2026-09-27: Wombat's design system built from the code (W-009,
  https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18, source in `design/system/`); the pilot README's steps A–H each have
  a done-signal, and a critic ran every command it names; flow 01 asks first for 2–3 structural wireframes;
  `design/tools/stage_upload.ps1 -Flow 01` stages 78 files (3.4 MB) from tracked files and the listed screenshots,
  holding back the one capture that shows a registration link.
- [x] A chosen design for the shell, with its states — the Claude Design artifact's link recorded here — 2026-09-27:
  https://claude.ai/artifact/R86QvLEyyfKx98fT4MENcD ("Wombat · Flow 01 · The shell"), variation A with C's breadcrumbs,
  round 3 accepted after the round-2 review's 30 fixes and D1–D11 (W-010, W-011). The boards are tracked in
  `design/flows/01-shell/round-3/`, and the record is `design/flows/01-shell/README.md`.
- [x] Implemented; suites green; `DESIGN.md` amended with a dated note — commit — 2026-09-27: `b347e11c`, one squash of
  branch `t335`. Six lanes (tokens and type, the acting role, reconnect and the error bar, titles, the shell, Home and the
  failure pages) were built test-first in worktrees, then merged. A four-sided review followed (security, fidelity,
  render modes and accessibility, runbook and docs: 3 medium findings, the rest low). Two fix lanes and a last pass
  followed that review. All six suites pass: Domain 791, Application 3,392, Infrastructure 992, Architecture 49,
  Web 2,549, Integration 453 (8,226), with 0 warnings. DESIGN.md's banner reads "Redesigned so far: flow 01".
  Each amended section carries "(2026-09-27, T335, flow 01)". The lanes' headless-Chrome checks, at 1280 and 390, are
  the evidence, not the replay: that is step G.
- [ ] Flow 01's runbook steps replay green on a fresh database; baseline re-captured — the replay's Actual lines.
- [ ] `BRIEF.md` corrected by the pilot's lessons — a dated "Pilot findings" section.

## Related

W-008, T332 (the brief), T336 (emails and PDFs), T293–T295 (the runbook and baseline).
