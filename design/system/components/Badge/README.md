# Badge

A small pill that shows a state in words, tinted by one of five colours that only repeat what the words say; `BadgeFor` is the one place a badge's class is chosen.

## What the consumer provides

`<span class="badge @BadgeFor.MsfCampaign(campaign.State)">@campaign.StateLabel</span>`

- The class, always from a `BadgeFor` method, never a `badge-` class written in the page (`Design/DefinedClassTests` fails on one).
- The words: the state's label, never its key ("Awaiting supervisor", not `submitted`).

## The five state tints (`BadgeFor.State`)

| Class | Tint | Means |
|---|---|---|
| `badge-draft` | grey: `muted-text` on `hover-bg` | not started, nothing wrong yet, or no longer in force |
| `badge-submitted` | blue: `secondary-color` on `info-bg` | handed on, waiting on someone |
| `badge-accepted` | amber: `warning-color` on `warning-bg` | in hand, or wanting attention again |
| `badge-completed` | green: `success-color` on `success-bg` | done or succeeded |
| `badge-declined` | red: `danger-color` on `danger-bg` | refused, failed, missed or withdrawn |

Each status is mapped onto these in C#: `ActivityState(key, isFinished)` (done is a terminal state of the pinned workflow, never a key's name; `declined`, `rejected`, `cancelled` are red; `submitted`, `requested` blue; `accepted` amber; anything else grey), `AgendaLine`, `AgendaElsewhere`, `DecisionDue`, `MsfCampaign` (Draft grey, Open blue, Closed and Under review amber, Released green, Withdrawn red), `EntrustmentDecision` (Active green, Expired amber, Revoked red, Superseded grey), `DataRightsRequest`, `JobRun` and `AuditResult`. DESIGN.md § Badges holds each table.

## The standing badges (`BadgeFor.Standing`)

`badge-standing-met`, `-below` and `-none`: a level against a target ("At or above", "Below", "No decision", "Not comparable"), the only badges that are a comparison. Body text (`text-color`) on the tint with a semantic border, because the semantic colours on their own tints fall short.

## Rules (DESIGN.md § Badges)

- A category or a type (an audit category, a request's type) is not a state, so it is not a badge; its words stand alone.
- A new status maps onto the five; app.css defines no other state class.
- `.badge`: 0.75rem, weight 600, `radius-pill`, padding 0.15rem 0.5rem.

## Contrast

Passes: `badge-draft` 4.83:1, `badge-submitted` 4.55:1, every standing badge (11:1 and up). **Fail, kept as the source has it (T322):** `badge-accepted` 2.42:1, `badge-completed` 2.55:1, `badge-declined` 3.57:1.
