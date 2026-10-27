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
| `badge-accepted` | `warning-bg` / `warning-color` | in hand, or wanting attention again |
| `badge-completed` | `success-bg` / `success-color` | done or succeeded |
| `badge-declined` | `danger-bg` / `danger-color` | refused, failed, missed or withdrawn |

Each status is mapped onto these in C#: `ActivityState(key, isFinished)` (done is a terminal state of the pinned workflow, never a key's name; `declined`, `rejected`, `cancelled` red; `submitted`, `requested` blue; `accepted` amber; anything else grey), `AgendaLine`, `AgendaElsewhere`, `DecisionDue`, `MsfCampaign`, `EntrustmentDecision`, `Appeal` (flow 09), `DataRightsRequest`, `JobRun` and `AuditResult`. DESIGN.md § Badges holds each table.

**Locked out** (flow 12, T376, K25; Users, UserPage): the administrative lock only, never a sign-in throttle: `span.badge.badge-declined.status-locked`, the `lock` icon (14px on Users, 16px in the who line) and the words "Locked out", `.badge.status-locked` giving them a `space-xs` gap. Active is a plain word, not a badge.

**Not decided is amber** (flow 07, T361, D53): with no deferral, a line left unstaged at a ratified sitting reads "Not decided at this sitting" on the review's agenda (`AgendaLine`) and the trainee's own reviews, "Not decided" on Decisions due (`DecisionDue`, its column and its status, "Not decided at review #30. A later sitting for 2026 S2 plans it again.") and on another panel's line (`AgendaElsewhere`): `badge-accepted`, the tint the deferral it replaced had, wanting attention again, not a failure. Until flow 07 an agenda line's Not decided was `badge-declined` and Deferred amber. Decisions due shows Not decided only where the line was due at that sitting or its window has ended (E3); an optional sitting's Not decided does not count. A committee review's state badge (`ReviewSitting.StateBadge`): Scheduled grey, In progress blue, Decided and Under appeal amber, Ratified and Closed (final) green, Withdrawn red.

**An MSF campaign's state** (flow 10, T369; `BadgeFor.MsfCampaign`, its words `MsfCampaignText.State`): Draft grey (`badge-draft`), Open blue (`badge-submitted`), Closed and Under review amber (`badge-accepted`: the coordinator's to review), Released green (`badge-completed`), Withdrawn red (`badge-declined`); "Under review", never the enum's "UnderReview". One badge on the campaign list, the campaign page's and the report's status cards (MsfCampaigns, MsfCampaignPage, MsfReport).

**Scheduled is grey on both committee lists** (flow 08, T364, round 3 S7): Decisions due's Scheduled (`DecisionDue`) was `badge-submitted` and is now `badge-draft`, the tint a scheduled review's state has on Committee reviews and the review's own sitting bar (`ReviewSitting.StateBadge`), so one state reads in one colour wherever it shows (BadgeFor.cs: "one colour on both lists"). Decisions due's other badges are unchanged: Decided green, Not decided amber, Missed and Revoked: re-decide red, and Not scheduled, Due by year end, Partial period and As opportunity allows grey (DecisionsDue).

**An appeal's badge** (flow 09, T366; `BadgeFor.Appeal`, DESIGN.md § Badges): **Open amber** (`badge-accepted`), the tint of a review under appeal, waiting on the appeal body; **Dismissed and Remitted both green** (`badge-completed`), an appeal heard and closed. Neither outcome is a failure (D51: there is no Upheld), so the badge's word says which it was. It heads each appeal's entry on both review pages (AppealEntry), beside "Your appeal" or "Appeal by Nomsa Mahlangu". Flow 09 also put the review's state badge on the trainee's own list and page (MyCommitteeReviews, MyReview), where State was words; "Expires in 20 days" beside a STAR's expiry is amber (`BadgeFor.State(BadgeState.Accepted)`, MyAuthorisations), and a STAR's status on My authorisations and the register is `EntrustmentDecision`'s: Active green, Expired amber, Revoked red, Superseded grey (StarRegister).

**A programme's end** (flow 13, T379, F10; `BadgeFor.ProgrammeOutcome(completed)`, DESIGN.md § Badges): **Completed green** (`badge-completed`) and, for a programme ended without completing it, **grey under its own name**, `badge-withdrawn` (the neutral tint, `header-bg` ground and `input-border` edge, as `badge-draft`, whose class says draft): "Withdrawn" on Trainees' Completed and closed (Trainees), "Inactive" in an ended profile's Status (TraineeProfile). The day follows the badge as plain words ("Completed 2026-10-27", "Inactive Left the programme: 2026-10-25"). A running programme's "Active" is a plain word, no badge. An erased person's profile reads as Withdrawn on the erasure day.

## The standing badges (`BadgeFor.Standing`)

`badge-standing-met`, `-below` and `-none`: a level against a target ("At or above", "Below", "No decision", "Not comparable"), the only badges that are a comparison. They keep their own class names and are painted as the state badge of their tint (completed, accepted, draft). Since flow 05 (T355) they show in three more places beside the standing panel: My progress's index (each EPA's STAR against the training year, the badge then its level, `span.verdict-level`), the EPA page's Entrustment (beside the STAR's level, `div.star-level`), and Home's My authorisations card in words only (`StandingWords.YearLine`, no badge).

## Overdue (`BadgeFor.Overdue`)

`badge-overdue`, "Overdue", painted as `badge-accepted` (`warning-bg`, its `warning-color` edge): a waiting activity that has waited `AssessorDueDays` (7) days or more (flow 04, T350, note 14). It stands **beside** the state's badge, never in its place, the two grouped in `span.needs-you-badges` (4px apart, wrapping): "Requested" "Overdue". Until flow 04 the Assessor's Home replaced the state's badge with an amber `badge-accepted` reading "Overdue", so an overdue row never said what state it waited in. See WaitingList.

## A count

A DashboardCard's `Count` shows as a `badge-submitted` after the title ("Needs you 2"), read as words where `CountWords` is given (", 2 items"). Its `BadgeWords` (flow 04) is the count in words as the badge itself, to the eye and to a screen reader alike: "2 waiting, 1 overdue". A list section's heading carries its count the same way: the inbox's "1 waiting, 1 overdue" (`badge-submitted`) and "3 decisions" (`badge-draft`), the registrar page's "1 waiting, 1 overdue".

**Work waiting on someone is `badge-submitted`; a count of people is `badge-draft`** (flow 06, T358, D10; DashboardCard's `BadgeTone`): "5 registrars" on the Registrars and Nothing filed in 30 days cards, since people wait on nobody.

**"Exempt this period"** (flow 06, review 9) is a `badge-draft`, `BadgeFor.State(Draft)`: a registrar to whom no target applies this period, always beside why, in words ("Starts on 2027-01-01", "Started part-way through the period"), on Programme trainees and Home's Registrars (RegistrarRoster).

## Look

`.badge`: inline-flex, 0.75rem/600, line height 1.5, padding 1px 10px, `radius-pill`, a 1px `input-border` edge unless the tint sets its own, `text-color` words, one line. In a flex row it keeps its shape (`align-self: center`, `flex: none`) rather than stretch to a wrapping row's height (T328).

## Rules

- A category or a type (an audit category, a request's type) is not a state, so it is not a badge; its words stand alone. Flow 05 breaks this once (T357): an institution's own EPA carries a neutral badge, "Kgosi Kgari Teaching Hospital's own" (`ProgressWords.LocalBadge`, `BadgeFor.State(Draft)`), in My progress's index and the EPA page's h1, where the standing panel marks the same EPA as text under its name (a gap, recorded with EpaProgressTable).
- A new status maps onto the five; app.css defines no other state class.

## Contrast

Words 11.23 to 11.89:1 on every tint (11.36:1 on the neutral). Edges: 5.23 (success), 5.43 (warning), 5.56 (danger), 4.55 (info), 3.42:1 (neutral, on `header-bg`). Until T335 a state's colour was its words on its own tint (2.42 to 3.57:1).
