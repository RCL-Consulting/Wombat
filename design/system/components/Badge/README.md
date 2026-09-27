# Badge

A small pill that shows a state in words: body text on one of five tints, the tint's colour on its 1px edge; the tint and edge only repeat what the words say, and `BadgeFor` is the one place a badge's class is chosen.

## What the consumer provides

`<span class="badge @BadgeFor.MsfCampaign(campaign.State)">@campaign.StateLabel</span>`

- The class, always from a `BadgeFor` method, never a `badge-` class written in the page (`Design/DefinedClassTests` fails on one).
- The words: the state's label, never its key ("Awaiting supervisor", "Requested", not `submitted`).

## The five tints (`BadgeFor.State`)

| Class | Ground / edge | Means |
|---|---|---|
| `badge-draft` | `header-bg` / `input-border` | neutral: not started, nothing wrong yet, a state an institution's own workflow names, or no longer in force |
| `badge-submitted` | `info-bg` / `secondary-color` | handed on, waiting on someone |
| `badge-accepted` | `warning-bg` / `warning-color` | in hand, or wanting attention again ("Overdue") |
| `badge-completed` | `success-bg` / `success-color` | done or succeeded |
| `badge-declined` | `danger-bg` / `danger-color` | refused, failed, missed or withdrawn |

Each status is mapped onto these in C#: `ActivityState(key, isFinished)` (done is a terminal state of the pinned workflow, never a key's name; `declined`, `rejected`, `cancelled` red; `submitted`, `requested` blue; `accepted` amber; anything else grey), `AgendaLine`, `AgendaElsewhere`, `DecisionDue`, `MsfCampaign`, `EntrustmentDecision`, `DataRightsRequest`, `JobRun` and `AuditResult`. DESIGN.md § Badges holds each table.

## The standing badges (`BadgeFor.Standing`)

`badge-standing-met`, `-below` and `-none`: a level against a target ("At or above", "Below", "No decision", "Not comparable"), the only badges that are a comparison. They keep their own class names and are painted as the state badge of their tint (completed, accepted, draft).

## A count

A DashboardCard's `Count` shows as a `badge-submitted` after the title: "Waiting for your rating 2".

## Look

`.badge`: inline-flex, 0.75rem/600, line height 1.5, padding 1px 10px, `radius-pill`, a 1px `input-border` edge unless the tint sets its own, `text-color` words, one line. In a flex row it keeps its shape (`align-self: center`, `flex: none`) rather than stretch to a wrapping row's height (T328).

## Rules

- A category or a type (an audit category, a request's type) is not a state, so it is not a badge; its words stand alone.
- A new status maps onto the five; app.css defines no other state class.

## Contrast

Words 11.23 to 11.89:1 on every tint (11.36:1 on the neutral). Edges: 5.23 (success), 5.43 (warning), 5.56 (danger), 4.55 (info), 3.42:1 (neutral, on `header-bg`). Until T335 a state's colour was its words on its own tint (2.42 to 3.57:1).
