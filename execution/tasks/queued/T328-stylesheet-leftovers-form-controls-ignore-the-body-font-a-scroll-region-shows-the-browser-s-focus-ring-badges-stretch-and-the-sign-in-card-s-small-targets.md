---
id: T328
title: Stylesheet leftovers: form controls ignore the body font, a scroll region shows the browser's focus ring, badges stretch, and the sign-in card's small targets
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T328 — Stylesheet leftovers: form controls ignore the body font, a scroll region shows the browser's focus ring, badges stretch, and the sign-in card's small targets

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Each item is visible polish or a small target, and none blocks a task. The 23px password toggle does fail WCAG 2.5.8, because it sits over the input and so cannot claim the spacing exception. The 21px account link fails only T086's own at-least-24px rule, since it stands clear of Sign out.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-A.7.2a, F-A.7.9b, F-A.7.12a, F-A.7.3a, F-A.7.3b, F-A.7.5a, F-A.7.5b).

## Symptom

**In full:** Stylesheet leftovers: form controls ignore the body font, a scroll region shows the browser's focus ring, dashboard badges stretch, the password toggle is 23px and sits high on the sign-in card, the account link is 21px, and two lists put Apply filters before their filters.

- **Step A.7.3:** textareas render in monospace and selects in Arial while the body is Segoe UI (`#dr-reason`, `#dr-type` on `/account/data-rights`, `design/baseline/states/data-rights--narrow.png`; the activity form's feedback fields too). On the dashboards' Recent activities and Recent decisions, a "Completed" badge stretches into a tall pill beside a link that wraps (`states/home--narrow-trainee.png`, `home--narrow-assessor.png`).
- **Step A.7.5:** Decisions Due's scrollable summary region, focused by keyboard, shows the browser's 1px auto outline rather than the design's focus ring (`act-A/A.7.5-3-decisions-due-summary-scrolled.png`). At 390px the data-rights queue's "Apply filters" sits at y 225, above Type (364), Status (460) and the table (561), so Tab reaches it before the filters it applies (`states/data-rights-requests--narrow.png`). The audit log does the same.
- **Steps A.7.9 and A.7.12:** the password "Show" toggle is 45×23px on the user page's Reset password and on the sign-in card, and on the sign-in card it sits at the field's top edge, not centred (`states/login--narrow.png`).
- **Step A.7.2:** at 390px the top row's account-address link is 21px tall on every page.

## Root cause

- **Fonts.** app.css has no `font-family` rule for `button`, `input`, `select` or `textarea`, and `.form-control` (445-453) and `.form-select` (455-470) set none, so the browser's defaults apply.
- **Region focus.** The `:focus-visible` list at app.css:348-354 covers `.btn`, `.form-control`, `.form-select`, `.search-input` and `a`, but not a focusable region. `DecisionsDue.razor:79` and `ProgrammeCoverage.razor:59,104` are `tabindex="0" role="region"`.
- **Badges.** Eight dashboard list rows are inline `style="display:flex;justify-content:space-between"` (`TraineeDashboard.razor:101,125`; `AssessorDashboard`, `CoordinatorDashboard` and `InstitutionalAdminDashboard`). `align-items` defaults to stretch, so the `.badge` (inline-block, app.css:953-959) takes the row's height.
- **Password toggle.** `PasswordToggleButton.razor:3` with app.css:1165-1174: padding 0.25rem on the browser's button font gives 23px, and the toggle is placed at `top: 50%` of `.password-wrapper`. On `Login.razor:33`, `Register.razor:60,67` and `LinkExternalLogin.razor:39` the wrapper also holds the `<label>`, so 50% falls near the input's top edge. `ChangePassword.razor:45` and `UserDetail.razor:120` wrap only the input, and there it is centred. The toggle also exposes no state or target: Change password has three buttons all named "Show" (found by code read).
- **Account link.** `MainLayout.razor:12`'s link gets no height from `.top-row ::deep a` (MainLayout.razor.css:33-39). T086's finding (2) named this link at 21px, and its fix lifted only `.btn`.
- **Apply filters.** `RequestsList.razor:8-15` and `AuditList.razor:8-15` put it in `PageHeader`'s Actions, before their search container. `EntrustmentDecisions/Index.razor:39-41` puts it in `FormActions` after the filters.

## What to build

- **Base font rule:** `button, input, select, textarea { font-family: inherit; }`. The builder's JSON textareas keep their own monospace (DESIGN.md:2087).
- **Region focus:** a `:focus-visible` rule for a focusable region (`.table-container[tabindex]`, or `[tabindex]:focus-visible` generally) with the `--focus-ring` outline, as DESIGN § Accessibility requires.
- **Dashboard rows:** a `.list-row` class (flex, space-between, a gap, `align-items: center` or `baseline`) replaces the eight inline styles. A badge keeps its pill (`align-self: center`, `white-space: nowrap`).
- **Password toggle:** at least 1.75rem tall (T086's floor) and centred on its input on all six pages, with `.password-wrapper` holding only the input and the toggle and the label moved outside it, as Change password already does. Give the toggle `aria-pressed` and `aria-controls` naming its input, so three "Show" buttons on one page can be told apart.
- **Account link:** at least 1.75rem tall (`inline-flex`, `align-items: center`, `min-height`).
- **Apply filters:** on the data-rights queue and the audit log, the button goes after the filters, in the search container's `FormActions`, as on the entrustment decisions list. Reading and Tab order become filters, then Apply, then the table.

## Verification

- [ ] Design tests (`DesignSystemSmokeTests` or `NarrowLayoutTests`) assert that app.css has the font-inherit rule, the region `:focus-visible` rule, the `.list-row` rule, and a `min-height` of at least 1.75rem on `.password-toggle-btn` and the top-row link. A grep test finds no inline `display:flex` style in `Components/Pages/Dashboards`.
- [ ] bUnit: the `.password-wrapper` on Login, Register and LinkExternalLogin contains no `<label>`. `PasswordToggleButton` renders `aria-pressed` and `aria-controls`. `RequestsList` and `AuditList` render Apply filters after their last filter.
- [ ] Browser at 390px, runbook steps A.7.2, A.7.3, A.7.5, A.7.9 and A.7.12:
- the replay's target check finds every button and link at least 24px tall;
- textareas and selects render in Segoe UI;
- Decisions Due's region shows the `--focus-ring` outline;
- dashboard badges keep their pill;
- the sign-in toggle is centred;
- the data-rights queue's Tab order is Type, Status, Apply filters, then the table.
Retake the cited screenshots.

## As built in T335 (`b347e11c`, 2026-09-27)

- **Done in T335:**
  - controls take `font: inherit`;
  - one universal `:focus-visible` ring covers a scroll region;
  - badges keep their pill (`flex: none`);
  - the password toggle is 28 px and centred;
  - the top-bar account link is 32 px.
  `StylesheetRuleTests` and `TypographyTests` hold these.
- **Still open:**
  - the dashboards' inline `display:flex` rows (`.list-row`);
  - the label inside `.password-wrapper`, and `aria-pressed` / `aria-controls` on `PasswordToggleButton`;
  - Apply filters' place in the Tab order.
- The 390 px browser items belong to T335's step G replay.

## Related

T086 (its finding 2 named the account link), T193 (password fields), T226, T266, DESIGN § Accessibility and § Page-level patterns (List page), T277 (the audit log's filter window, same page), runbook steps A.7.2, A.7.3, A.7.5, A.7.9 and A.7.12.

## Notes

- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26: page-chrome spacing, from the same stylesheet.** (1) Header actions differ in height. A <button class="btn"> ('Schedule review', ReviewsSchedule.razor:16) is shorter than an <a class="btn"> ('New campaign', CampaignsList.razor:21; 'Run history', ScheduledJobsList.razor:10). The button keeps the browser's control font, and with it that font's 'normal' line height; the link inherits Segoe UI (states/reviews-schedule--loading.png, campaigns-list--loading.png, scheduled-jobs-list--loading.png). This task's font-inherit rule should make them equal. Measure both after it, and use `font: inherit` if the line height still differs. (2) The header actions sit against the top bar's bottom edge. `.header-container` aligns to flex-start (app.css:117-126), the h1 keeps the browser's top margin, and the actions cell has none. Align the actions with the title. (3) On /msf/reports/{id}, the Supervision level label sits straight under the Narrative textarea, and the Coordinator actions card touches the first respondent-group card below it (states/campaign-report--blocked.png). The two FormFields (CampaignReport.razor:110-124) are bare `.form-group`s outside a `.form-grid`, and `.form-group` has no margin (app.css:422-426). The page's `.details-grid` (:33) is followed by standalone `.detail-card` sections (:146), and neither has a margin (app.css:751-757, 1086-1090). Give stacked form groups and stacked cards the design's gap: a rule, not a per-page fix. Verify with a design test for each rule, and retake the three screenshots. The sweep's other point, no active nav item on /msf/reports and /msf/coverage, is filed as its own task.
- **T332 brief, 2026-09-26.** Three more: `app.css:623` uses `var(--text-muted)`, a token `:root` does not define (only the `.text-muted` class exists, line 1259), so that rule falls back to the inherited colour; the nav items render underlined in the captures; and in full-page captures the sidebar's gradient stops short of the page's end.
- **T297 re-check, 2026-09-26.** Two more: the Coordinator's Stalled requests row breaks its date over two lines at 1280 px ("18" over "Sept"; F-3.30b, `act-3/3.30-1-smit-stalled-requests.png`); and a dashboard card's icon is fixed, so "Awaiting your review" and "Stalled requests" show the alert triangle even when they read "Nothing is awaiting your review." (F-2.36a). Only the Warning styling follows the data. A convention for the redesign rather than a T297 regression: the card it replaced did the same.
- **Wombat design system build, 2026-09-27.** DESIGN.md describes a `.detail-card h3` rule that `app.css` does not have, and the `Breadcrumbs` component is used by no page.
