---
id: T280
title: Accessibility leftovers: trainee dashboard links named only by instrument, the alert's dismiss button named "×", and the review PDF leaving out a review under appeal
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T280 — Accessibility leftovers: trainee dashboard links named only by instrument, the alert's dismiss button named "×", and the review PDF leaving out a review under appeal

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T239 and T250 reviews.

## Symptom and what to build

- **Trainee dashboard links.** The inbox, recent and deadline links in `TraineeDashboard.razor` are named only by
  `ActivityTypeName`. Carry the EPA or date on the dashboard DTO, and name each link uniquely.
- **The alert's dismiss button** in `Alert.razor` is named "×". Give it `aria-label="Dismiss"`.
- **The portfolio PDF leaves out a review under appeal.** `PortfolioPdfService` exports only Ratified or Final reviews,
  so a review under appeal drops out of the PDF until the appeal is resolved (since `6faa3fd5`). Decide, and state
  which: include it, marked "under appeal".

## Verification

- [ ] Each, with bUnit or PDF text tests.

## Related

T239, T250, T212.

Notes, 2026-09-25 (committee chain 3's browser check):
- `/msf/campaigns` has 22 duplicate accessible names (e.g. "View campaign: the campaign for Demo Trainee (Default MSF,
  closing 2026-10-09)", and Manage, View report and Withdraw likewise). `CampaignsList.razor` does not use
  `RowNames.Distinct`, which DESIGN.md § Table system (T239) requires.
- The assessor dashboard's Recent decisions names links "<instrument> for <trainee>", so several rows for one trainee
  share a name (6 × "Mini-CEX (Paediatrics) for Demo Trainee" on dev). Add the date or EPA.

## Notes

- **T295 replay, 2026-09-26 (C71).** Note, 2026-09-26 (the T295 replay, step A.7.13, F-A.7.13a): confirmed on review 7 (`/committee/reviews/7`). In Chromium's accessibility tree, the dismiss button on the "Sampling concentration warnings" alert is named "×" (screenshot `design/baseline/act-A/A.7.13-1-zulu-review7-top.png`). `Alert.razor:8` is the only "×" button under Components, and `ReviewDetail.razor:68` is the only caller that sets `Dismissible`, so the one-line fix this task names covers it. This does not widen the task.
- **T295 replay, 2026-09-26 (C72).** Note, 2026-09-26 (the T295 replay): three more controls or regions with no name or state. This widens the task.
- **The pager's page-size select has no accessible name** (step 4.1, F-4.1a, Decisions Due; screenshot `design/baseline/act-4/4.1-1-smit-decisions-due-outstanding.png`, at the foot of the list). `PagerControls.razor:10` labels it with a bare `<span>Per page:</span>`, so a screen reader announces an unnamed combobox on every paged list. Use a `<label for>` with an id unique to each pager instance, or `aria-label="Rows per page"`. T264 changes the same component to keep the focus, so whichever task lands first should do both.
- **The builder's tab bar exposes no selected state** (step A.7.9, F-A.7.9a, `/admin/activity-types/23`; screenshot `design/baseline/act-A/A.7.9-5-builder-top.png`). `ActivityTypeEdit.razor:62-66` renders plain buttons, and only `.is-active` marks the current one. Either apply the tabs pattern (`role=tablist`/`tab`/`tabpanel`, `aria-selected`, `aria-controls`, arrow keys) or put `aria-pressed`/`aria-current` on the active button. Record the choice in DESIGN.md § Builder layout, which is silent on ARIA today.
- **Review 7's cards are named inconsistently** (step A.7.13, F-A.7.13b). Walking the page by regions reaches 3 of its 9 cards. `ReviewDetail.razor:116, 207, 352, 705, 731 and 1491` are `section.detail-card` elements with no `aria-labelledby`, while 553, 1502 and 1833 have one. Each unnamed card has an `<h3>`, so this is consistency rather than a WCAG failure: give each section `aria-labelledby` pointing at its heading.
bUnit for each.
- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26 (the top of states/review-detail--ratify-blocked.png, --chair-cannot-act.png, --no-longer-decided.png).** Besides its name, the dismiss button is misplaced. Alert.razor:6-9 puts the content and the button in `.actions-cell`, which wraps (app.css:307-311). The sampling warnings' content is a block that fills the row, so the tiny bordered × wraps under it, to the bottom left, not the top right where a dismiss control is looked for. Give a dismissible alert its own layout (the content, and the button at the top right, beside the first line), with the aria-label 'Dismiss' this task already asks for. Record it in DESIGN § Alerts. Verify with bUnit on the markup and a retake of review 7's top.
- **T297 re-check, 2026-09-26 (F-3.12b).** A dashboard card with an `Href` wraps its whole content in one `<a>` (`DashboardCard.razor:10-24`), and the Trainee dashboard puts links inside it: the Activity inbox card's row links and "Open inbox →" (`TraineeDashboard.razor:90`), and My authorisations' "View authorisations →" (`:166`). A link inside a link is invalid, and a screen reader announces the card as one link. Since T011; T297's row links now sit inside it (`design/baseline/act-3/3.12-1-ndlovu-home-declined.png`).
- **Wombat design system build, 2026-09-27.** Four more, found while documenting the components: `ConfirmDialog` has no accessible name; `TabBar` has no tab roles, so the active tab shows by colour alone; the pager's page-size select has no label; and `PasswordToggleButton` does nothing on the static sign-in page (no circuit) and sits off-centre. Recorded in the design system's component READMEs (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18).
