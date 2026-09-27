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

- [ ] Set-up complete: the design system exists in Claude Design; `design/pilot/README.md` names every step, upload and
  check; flow 01 asks for a restructure — read.
- [ ] A chosen design for the shell, with its states — the Claude Design artifact's link recorded here.
- [ ] Implemented; suites green; `DESIGN.md` amended with a dated note — commit.
- [ ] Flow 01's runbook steps replay green on a fresh database; baseline re-captured — the replay's Actual lines.
- [ ] `BRIEF.md` corrected by the pilot's lessons — a dated "Pilot findings" section.

## Related

W-008, T332 (the brief), T336 (emails and PDFs), T293–T295 (the runbook and baseline).
