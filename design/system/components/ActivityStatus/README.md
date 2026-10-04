# ActivityStatus

"Who has it now" (`Components/Shared/Activities/ActivityStatus.razor`): the activity page's first card. The state's badge and one headline, a quoted note when the last move carried one, a body, a meta line, and the page's one action for that state; its stripe says whose move it is. Flow 03 (T342, 2026-09-29, 725237ee; Q3) made it; it replaces the old summary card and the header's "State:" line. Flow 05 (T355, 2026-10-04, b020c942; C5, E5) added the done card's count line and pointed Open My progress at the EPA's page.

## What the consumer provides

`<ActivityStatus View="@model.Status(_ratedLabel, _countLine)" Busy="@(_running is not null)" OnCancel="CancelFromStatusAsync" />`

- `View` (required): an `ActivityStatusView`, worked out by `ActivityPageModel.Status` from the detail's holder (`ActivityHolderDto`: the author, a person, a role, done or closed) and the viewer. Every headline ends with a full stop (C11). Its `ActionAriaLabel` (flow 05, T355, E5) is the action's name where its words alone do not say where it goes.
- The count line (flow 05, C5): for a done record's registrar the page reads `GetActivityCountLineQuery` as part of its own load and passes it to `Status`; null for anyone else, for an unfinished record, and when the read fails (logged; the page never fails for it).
- `OnCancel`: the card's Cancel was pressed; the page opens its ConfirmDialog. `Busy`: a move is running, so Cancel, which would race it, is disabled.

## Markup

`section.detail-card.activity-status` plus its tone, named by its label, "Who has it now" (`aria-labelledby="activity-status-label"`; flow 04, nit A10: until then it was named by the whole headline), 24px above the details grid:

- `.activity-status-main` (a column, 8px apart, `flex: 1 1 26rem`): `p#activity-status-label.activity-status-label` "Who has it now" (muted, 0.85rem/600); `p.activity-status-head`, the badge (`BadgeFor.ActivityState`) then `span.activity-status-headline` (1.1rem/600, line height 1.35); a `blockquote.activity-status-quote` (the note as its writer wrote it, keeping its lines, on `header-bg` with a 4px `input-border` stripe, `radius-sm`; its writer in `cite`, muted 0.85rem, not italic); `p.activity-status-body`; `p.activity-status-meta` (muted, 0.9rem).
- `.activity-status-action`, beside the main column (under it where there is no room), holding the one action.

**The stripe** (4px, the card's left edge; its frame is every card's hairline): the viewer's move `.detail-card--emphasis` (`secondary-color`); someone else's `.activity-status--others` (`input-border`); done `.activity-status--done` (`success-color`); declined `.activity-status--declined` (`danger-color`).

## The words, by holder

| Holder, viewer | Headline | Body | Action |
|---|---|---|---|
| The author's draft, to the author | With you. Not submitted yet. | Finish the request and submit it. It is in nobody's inbox until you submit it. | (the bar's) |
| Returned, to the author | With you. Sarah Botha returned it on 2026-09-30 08:03 SAST. (her note quoted) | Change your reflection and submit it again. | (the bar's) |
| Returned, to anyone else | With Sipho Ndlovu. Sarah Botha returned it on 2026-09-30 08:03 SAST. | It is in nobody's inbox until Sipho Ndlovu submits it. | none |
| A person, to that person | Your move. Anele Dlamini asked you on 2026-09-30 07:58 SAST. | Complete it, or decline it with a note Anele Dlamini will read. | (the bar's) |
| A person, to the author | With David Naidoo since 2026-09-30 07:58 SAST. | Nothing for you to do. You can cancel the request until David Naidoo acts on it. | Cancel request… (quiet) |
| A role, nobody named | Waiting for <state label>. | To the author: "Nothing for you to do. You can cancel the request until someone acts on it." where Cancel is offered, else "Nothing for you to do." | Cancel request…, where offered |
| Declined, to its registrar | Closed. Fatima Khumalo declined it on 2026-09-30 08:01 SAST. (her reason quoted) | It credits nothing, and nothing more can happen to it. To be assessed on this encounter, file it again and name someone else. | File it again, to someone else |
| A draft cancelled before filing | Closed. You cancelled it on … | It was never submitted, and it credits nothing. | none |
| Done, to its registrar | Done. David Naidoo completed it on 2026-09-30 08:00 SAST. | Rated 4. Credited 1 item to PAED-001. PAED-001: 1 of 3 this semester. | Open My progress |
| Done, its own move (a log) | Done. Logged on … | A KGK teaching session log credits nothing, and nobody else acts on it. | none |
| Done, discussed | Done. Sarah Botha recorded the discussion on … | A reflective exercise credits nothing. Nothing more happens to it. | none |

- The viewer's own moves are verb phrases by the move's key (`ActivityPageModel.MovePhrase`: "complete it", "record the discussion", "sign it off"; a builder's own move "act on it"), never the label with " it" after it. "Record the discussion, or return it with a note Sipho Ndlovu will read."
- The mover is "You" to the viewer: "Done. You completed it on …".
- **A late filing** adds the meta line while someone holds it: "Filed 20 days after the encounter: recorded as late."
- **Credit** on a done record: "It counted towards no curriculum requirement." when it credited nothing; "Its credit to PAED-012 waits while the EPA is paused." when its EPA is paused and the completion credited nothing (one credited before the pause reads as credited).
- **The count line** (flow 05, T355, C5; `CountLineWords.ForCard`), after the credit sentence, only where it credited an item: what the credit made of the EPA's count in the activity's own window, in My progress's words: "PAED-001: 1 of 3 this semester.", "PAED-001: 3 of 3 this semester, met.", "PAED-008: no target in 2026 · 0 recorded."; for a window already past, "PAED-001, Semester 1, 2026: 1 of 3, 2 short.". A failed read leaves flow 03's words alone.
- Every time is South African with its zone (`ActivityMoments.When`).

## The action

- **File it again, to someone else** (E1, E5): a `.btn-primary` link with a 16px `copy` icon to `/activities/new?from={id}`, only on a declined request and only for its registrar.
- **Open My progress**: a `.btn-outline` link, on a done record, for its registrar. Its words and place are flow 03's; since flow 05 (E5; Q7) it lands on the EPA's own page, `/portfolio/progress/{EpaId}`, when the count line named the EPA, or when the EPA is paused (its page still opens), and its accessible name says so: "Open My progress at PAED-001". With no count line it goes to `/portfolio/progress` and is named by its words.
- **Cancel request…**: `.btn .btn-quiet .btn-quiet--danger`, while someone else holds the request, or a role does (C11; `CancelOnCard`), and only for whom the server offers it. It opens the page's ConfirmDialog ("Cancel this request?"), which hands the focus back here on Keep the request. On a draft, or a request the viewer holds, Cancel is last in the bar instead (ActivityWorkflowActions).

Below 641px the action takes the card's width and each button is 44px.

## Rules (DESIGN.md § Page-level patterns, "Record page with a workflow")

- One card answers who has it now; the page header carries no "State:" line.
- To the rater it shows no year minimum and no semester count, before or after rating (flow 04, Q7).
- A draft is not private (C2): never "nobody can see it". It is in nobody's inbox.
- The quote is the note as written, never rewritten.

## Contrast

The label and meta `muted-text`, 5.09:1; the quote `text-color` on `header-bg`, 11.36:1, its `cite` `muted-text` 4.57:1 there; Cancel request… `danger-color` on the surface, 5.95:1; the stripes each 3:1 or more on the page (R3-Spec § 5).

## Known gaps

- The completed card's count, the semester's figure the flow 03 board had, is built by flow 05 as its own sentence ("Credited 1 item to PAED-001. PAED-001: 1 of 3 this semester.", where the board ran the two together with a colon); the T355 replay found T346 no longer occurs, and its task file is still in `queued/`.
- The done body builds a noun from the type's name ("a portfolio and logbook review"), where the boards named the instrument ("a portfolio review") (T347).
