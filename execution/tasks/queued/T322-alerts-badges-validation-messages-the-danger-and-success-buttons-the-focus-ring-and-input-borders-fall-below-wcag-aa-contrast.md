---
id: T322
title: Alerts, badges, validation messages, the danger and success buttons, the focus ring and input borders fall below WCAG AA contrast
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T322 — Alerts, badges, validation messages, the danger and success buttons, the focus ring and input borders fall below WCAG AA contrast

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High. This is text a reader must read: every success, warning and danger alert, every accepted, completed and declined badge, and every validation message. It measures 2.42–3.82:1 against a 4.5:1 minimum, and warning text is barely half. The Lock out and Approve-style buttons carry white text at 2.87–3.82:1. Input borders at 1.49:1 do not show where a field is. It is system-wide because the fault is in the tokens, not on a page.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-A.7.14a).

## Symptom

Runbook step A.7.14 measured the rendered pages, and the ratios match the tokens exactly:
- **Success alert, `.badge-completed`:** #27ae60 on #e8f5e9, 2.55:1. See the profile's "saved" alert, `design/baseline/act-A/A.7.14-2-profile-saved-success-alert.png`.
- **`.alert-warning`, `.badge-accepted`:** #fd7e14 on #fff8e1, 2.42:1.
- **`.alert-danger`, `.badge-declined`:** #e74c3c on #fff5f5, 3.57:1. See Change password's mismatch alert, `A.7.14-3-change-password-mismatch-danger-alert.png`.
- **`.validation-message` on white:** 3.82:1.
- **White text on buttons:** on `.btn-danger` (the user page's Lock out, `A.7.14-4-patel-lockout-button-focused.png`) 3.82:1, and on `.btn-success` 2.87:1.
- **The focus ring on the page background:** 2.99:1.
- **Input borders:** 1.49:1 on white, 1.42:1 on the page background.

What passes: card text 12.63, muted text 4.83/5.09, links 6.70, the primary button 4.86, the ring on white 3.15. To check whether the defect is still there, look at any success or warning alert: its text is pale green or orange on a pale tint of the same colour.

## Root cause

`src/Wombat.Web/wwwroot/app.css` `:root`, lines 19-21 (`--success-color` rgb(39 174 96), `--danger-color` rgb(231 76 60), `--warning-color` rgb(253 126 20)), 28 (`--input-border` rgb(206 212 218)) and 30 (`--focus-ring` rgb(52 152 219)). These tokens are used:
- as text on their own tints in `.alert-danger/-success/-warning` (645-661) and `.badge-accepted/-completed/-declined` (971-984);
- as text on white in `.validation-message` (686-689), `.validation-summary-errors` (704-711) and `.text-danger` (1267);
- as the background under white text in `.btn-success` and `.btn-danger` (375-383).

T086 darkened only `--muted-text`. T166 (`.badge-standing-*`, about line 986) and T160 (`.field-warning`, about line 691) each worked around the same shortfall locally, with body text on the tint, and their comments say why, but the tokens were never fixed.

One trap for the fix: the sidebar has no focus rule of its own (`NavMenu.razor.css`), so nav links use the same `a:focus-visible` ring. The current ring is 4.46:1 on the sidebar's rgb(5 39 103), but a ring dark enough for the page (for example `--secondary-color`, 4.61:1 on the page background) is only 2.90:1 there.

## What to build

Every colour pair in the design system meets WCAG 2.1 AA: text 4.5:1, and control boundaries and the focus indicator 3:1 (1.4.3, 1.4.11). The pairs are enforced by a test that reads the tokens.
- **Semantic text.** Follow the precedent `.badge-standing-*` and `.field-warning` already set: alerts and status badges take body text (`--text-color`, 11+:1 on every tint) on their tint, and the semantic colour stays as border or stripe. Alternatively, add darker text tokens. Measured candidates:
  - `--danger-text` rgb(176 42 31): 6.14 on its tint, 6.57 on white;
  - `--success-text` rgb(25 111 61): 5.52 on its tint;
  - `--warning-text` rgb(138 70 0): 6.69 on its tint.
  `.validation-message`, `.validation-summary-errors` and `.text-danger` need a danger text colour of at least 4.5:1 on white and on the page background either way.
- **Buttons.** Darken the fills of `.btn-danger` and `.btn-success` so white text reaches 4.5:1. For example, rgb(176 42 31) gives 6.57 and rgb(30 126 52) gives 5.14. Darkening `--danger-color` and `--success-color` themselves is acceptable, provided the invalid-field stripe (T236) and status dots still read.
- **Focus ring.** Content ring of at least 3:1 on `--surface-color` and `--background-color` (for example `--secondary-color`: 4.86 and 4.61). Give the sidebar its own ring token that keeps 3:1 on both ends of its gradient (the current light blue gives 4.46 and 5.14), scoped in `NavMenu.razor.css`.
- **Input border.** At least 3:1 on white and on the page background. For example, rgb(134 142 150) gives 3.32 and 3.15.
- **DESIGN.md.** In § tokens (line 69 and on), list every pair with its ratio. Remove the "falls short of 4.5:1" caveats that no longer apply, and keep the rule that no colour is set outside `:root`.

## Verification

- [ ] A new `Design/ContrastTests` in Wombat.Web.Tests parses app.css's `:root` and each rule's colour and background. It asserts at least 4.5:1 for: every `.alert-*` and `.badge-*` text on its background; `.validation-message`, `.validation-summary-errors` and `.text-danger` on `--surface-color` and `--background-color`; and white on `.btn-primary`, `.btn-success` and `.btn-danger`. It asserts at least 3:1 for: the focus ring on `--surface-color` and `--background-color`; the sidebar ring on both gradient ends; and `--input-border` on both surfaces. Mutation check: put back one old token and the test fails.
- [ ] Browser, runbook step A.7.14: repeat the replay's contrast measurement on the profile's success alert, Change password's danger alert, the user page's Lock out button (focused), a validation message and an input. Every pair passes. Retake `design/baseline/act-A/A.7.14-2..4`.
- [ ] Browser, runbook steps A.7.5 and A.7.9: Tab through the sidebar nav and then page content. The ring is visible and at least 3:1 on both.
- [ ] DESIGN.md's token section records the pairs and the sidebar ring token, and grep finds no hex or rgb colour outside `:root` in app.css.

## Related

T086 (the muted-text half of the same audit), T166 (`.badge-standing-*`), T160 (`.field-warning`), T236 (invalid-field stripe on `--danger-color`), runbook step A.7.14, DESIGN.md § tokens and § Accessibility.

## Notes

- **Wombat design system build, 2026-09-27.** Two more pairs below the 3:1 a meaningful mark needs: the ok and warn status dots against white (2.87:1 and 2.57:1), and a complete progress bar's green fill on its track (2.73:1). All the failing pairs are kept as they are and flagged in the design system's token notes and brand README.
