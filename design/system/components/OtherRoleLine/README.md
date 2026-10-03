# OtherRoleLine

The other-role line (`Components/Shared/OtherRoleLine.razor`): on Home, one line about the work waiting in the Activity inbox, for someone who holds Assessor and is acting in another role, with one link that opens the oldest. Flow 04 (T350, 2026-09-30, 06aa51d7; note 7, Q3, E4, C12) made it; until then a consultant acting as Committee member learnt of waiting work only by switching role.

## What the consumer provides

`<OtherRoleLine />`, placed by `Home.razor` between PageHeader and the acting role's dashboard. It reads for itself (`ListWaitingForYouQuery`, the inbox's own read, so the line and the inbox cannot disagree; T297) and takes the acting role from the cascade. It draws nothing:

- for someone who does not hold Assessor, or who is acting as Assessor (their Home has the "Waiting for you" card);
- when nothing waits;
- when its read fails: the failure is logged, never shown. The line is a courtesy beside the dashboard; a Home error is DashboardFrame's to say (C12).

## Markup

`section.alert.other-role-line` with `alert-warning` when any of it is overdue, `alert-info` otherwise, named `aria-label="Waiting for you in the Activity inbox"`, 24px above the dashboard. Not a live region: it arrives with the page, and a screen reader finds it by its name. Inside, an `.alert-row`: the words (`span.alert-row-text`), then one `a.btn.btn-outline.btn-sm` to `/activities/{id}` of the oldest.

## The words (`WaitingWords.OtherRoleLine`, `OtherRoleAction`)

- One, overdue: "1 activity waits for you in the Activity inbox, and it is overdue: Mini-CEX (Paediatrics) · PAED-004 · 2026-09-30, from Nomsa Mahlangu, waiting 8 days." · **Open it**
- Several: "3 activities wait for you in the Activity inbox; 1 is overdue. The oldest: …, from …, waiting 8 days." (the "; n is/are overdue" clause only when any is) · **Open the oldest**
- One, not overdue: "1 activity waits for you in the Activity inbox: …, from Anele Dlamini, waiting less than a day." · **Open it**

The link's accessible name carries its row: "Open it: Mini-CEX (Paediatrics) · PAED-004 · 2026-09-30, from Nomsa Mahlangu".

**Below 641px** the row stacks: the words, then the link at the line's width and 44px.

## Rules (DESIGN.md § Dashboard page, R1)

- **No switch in the line.** It opens the activity, which every role may open; the sidebar's switch is the one way to change role. Opened from the line, the activity page lights nothing and its trail is Home › the activity.
- It names no role: the read counts every arm that is not the author's, not only the assessor's.
- One line, never a card: it is about another role's work, not this Home's.
- `Home.razor` places it once, above whichever dashboard the acting role draws; it draws only for a holder of Assessor acting as another role. Flow 04 owns its words.

## Contrast

The words `text-color` on `warning-bg` 11.89:1 or on `info-bg` 11.84:1; the edge and icon 5.43:1 (warning) or 4.55:1 (info); the link as an outline button, `secondary-color` 4.86:1 on its surface fill.
