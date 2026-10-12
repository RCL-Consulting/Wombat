# SittingBar

The sitting bar (`Pages/CommitteeDecisions/Review/SittingBar.razor`, drawn from `ReviewSitting.Bar`): directly under the committee review's header, where the review stands and the one act that comes next. The chair's bar sticks under the top bar while the chair works down the agenda; a reader's scrolls away and says who may act. Flow 07 (T361, 2026-10-12, 44de180b; R2-Spec "SittingBar"; round 3 items 14, 16, 21, 28; E2, E7, E8) made it; until then the state was a line in the review's details and the chair's controls a card of their own.

## What the consumer provides

```razor
<SittingBar View="@ReviewSitting.Bar(review, agenda, pending, present)" AsOf="@AsOf" Busy="@(_inFlight is not null)"
            Refreshing="@(_inFlight == RefreshAction)" OnStart="OpenStartAsync" OnGoToDecision="GoToDecisionAsync"
            OnRatify="OpenRatifyAsync" OnClose="OpenCloseAsync" OnRefresh="RefreshAsync" Focus="FocusByIdAsync" />
```

- `View` (required): a `SittingBarView`: `Chairs` (the chair's bar), the state label and its badge class (`ReviewSitting.StateBadge`), `Count`, `Mode`, `Present`, `Sentence`, the `Act` (None, Start, GoToDecision, Ratify, Close), `ActReason` (why the act is unavailable) and `Refresh`.
- `AsOf`: a reader's "As of 14:02 SAST", with what the last Refresh found.
- `Busy`: an action runs, so every act here would race it and is disabled; `Refreshing`: Refresh's own read runs.
- `OnStart`, `OnGoToDecision`, `OnRatify`, `OnClose`, `OnRefresh`, and `Focus` (moves the focus to an element by id).

## Markup

`section.sitting-bar#sitting-bar`, named "The sitting" by a visually hidden `h2#sitting-heading`: a surface card with a 4px left stripe, `radius-lg`, `shadow-raised`, padded 8px 16px, a wrapping flex row 8px by 16px apart.

- `div.sitting-bar-line`: the state badge (`.badge.sitting-bar-state`: Scheduled grey, In progress blue, Decided and Under appeal amber, Ratified and Closed (final) green, Withdrawn red; Badge), the count (`.sitting-bar-count`, weight 600), the mode (`.sitting-bar-mode`, muted) and Present (`.sitting-bar-present`, muted).
- `p.sitting-bar-sentence#sitting-sentence` (a reader's bar, a line of its own).
- `div.sitting-bar-actions` at the row's right (`margin-left: auto`): a reader's "As of …" (`p.sitting-bar-as-of#sitting-as-of`, muted, `role="status"`) and Refresh (`btn-sm btn-outline`, `refresh-cw`), then the act.

## The words (`ReviewWords`)

| Part | Words |
|---|---|
| Count | before Start "15 EPAs on the agenda"; in progress "None staged", "3 staged"; decided "Satisfactory Progress · 3 decisions to issue" ("· no decision to issue"); ratified, under appeal or closed "Satisfactory Progress · 3 STARs issued" ("· no STAR issued"); a closed formative review "Formative check-in · no decision recorded" |
| Mode (before Start; a formative review in progress) | "Summative · evidence 2026-01-01 to 2026-12-31", "Formative: an interim check-in, no binding decision · evidence …"; in progress, "Formative: an interim check-in, no binding decision" |
| Present (the chair's bar, in progress, from the decision form's ticks; not live) | "Present: the chair · one more member needed", "4 present · quorum met", "Present: the chair · no other member may sit" |
| The act | **Start review…** (`btn-primary`), **Go to the decision** (`btn-outline`; moves the focus to the Decision heading), **Ratify…** (`btn-primary`), **Close review…** (`btn-primary`; a formative review) |
| Sentence (a reader's bar) | "Only the panel's chair, Thandi Zulu, can stage entrustment decisions and record the committee's decision."; "… can ratify the committee's decision." ("… can remove a staged decision that no longer fits the trainee's curriculum and ratify …" where one does); "… can close this review."; to a reader not offered Start, "Only the coordinators of the panel's institution, and those of its members who are active committee members there, can start this review."; the trainee elsewhere or the chair unable to act, in those checks' own sentences; "Withdrawn on 2026-10-04. …" |
| As of (a reader's, while the review can still change) | "As of 14:02 SAST", "As of 2026-10-04 14:02 SAST" when read on another day (E7); after Refresh "As of 14:31 SAST. Now Decided.", "… Now 4 staged.", "… No change."; a Refresh that failed, "The review could not be read again; it is shown as last read." |

## The two bars

- **The chair's bar** (`.sitting-bar`, `View.Chairs`): drawn when the chair may act now (`CallerChairs`, no `TraineeElsewhere`) and the review is scheduled, in progress or decided with an act to offer. Its stripe is the action blue (`secondary-color`). **From 641px it sticks** under the 56px top bar (`position: sticky; top: 56px; z-index: 1`, under the sidebar's 10, above the page's tables; DESIGN.md § Stacking), and `js/sticky-offset.js` measures its height into `--sitting-bar-h` on `<html>`, which `scroll-padding-top` and every focus target's `scroll-margin-top` reserve. From 641 to 1023px it keeps one line and Present drops (E8).
- **A reader's bar** (`.sitting-bar.sitting-bar--reader`): static under the title, it scrolls away and reserves nothing. Its stripe is `input-border`, or the action blue where the reader may start the review (`.sitting-bar--starts`, E2: a coordinator, a member who may sit, an Administrator). It holds the sentence and, while the review is scheduled, in progress or decided, As of and Refresh.

## Behaviour

- **Refresh** reads the review again in place, keeps the scroll, and keeps the focus on itself: its own read never disables it (`aria-disabled` while it runs, T234); the As of line, a status, says what changed (21). There is no live following (T362).
- **Ratify short of a quorum** (`ActReason`, the review's `QuorumShortfall`): `aria-disabled="true"` with `.is-unavailable`, named by its reason (`aria-describedby="ratify-reason"`) beside it (`p.move-reasons.move-reasons--beside#ratify-reason`), never natively disabled (16); pressed, it moves the focus to the reason.
- Start, Ratify and Close each open a ConfirmDialog (its safe button "Not yet"); the answer sits under the bar ("Decision ratified.", "Formative review closed.") or atop the agenda ("Review started."), taking the focus (ReviewSitting).
- Re-drawn after every action from the review read again (T260).

## Narrow widths

Below 641px every bar is static and stacked: a column, its actions a full-width column, each button 44px.

## Rules (DESIGN.md § Committee agenda, "The sitting bar"; § Stacking)

- One act at a time, the next one; the chair's other acts are in the agenda's lines and the decision form.
- A reader is told who can act, never shown a control that would be refused.
- A sticky page bar takes `z-index: 1`; nothing in `app.css` takes 10 or more (`Design/StylesheetRuleTests`).

## Contrast

The stripe `secondary-color` 4.86:1 (the chair's, a starter's), `input-border` 3.80:1 (a reader's) on the surface; the count `text-color` 12.63:1; the mode, Present and As of `muted-text` 5.09:1; the badges as Badge's.

## Known gaps

- Who can start a scheduled review when the chair cannot act is not said: the bar says the chair cannot act, and Start is withheld from everyone (Q11).
