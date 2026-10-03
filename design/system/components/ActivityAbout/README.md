# ActivityAbout

The activity page's About card (`Components/Shared/Activities/ActivityAbout.razor`): who, what EPA, when, and what it credits, as a details list, in the details grid's narrow column beside the form. It also holds what the old summary card carried: the form's version where a move's reason names it, and the credit warnings. Flow 03 (T342, 2026-09-29, 725237ee; C13) made it; "(no longer in use)" beside a paused EPA came in daaf6386, its space before it in a341b44e.

## What the consumer provides

`<ActivityAbout Rows="@model.About()" />`

- `Rows` (required): `ActivityAboutRow(Label, Value, Warnings, Id)` with an optional `Marker`, worked out by `ActivityPageModel.About`.
- `Class` and `IdSuffix` (flow 04, T350; round 1, E3; round 2, E2): where the page draws About twice. For a reader with a section to fill who is not the author, the page draws it `.only-wide` in its place and again `.only-narrow` after the sections and the bar, so on a phone About comes under the bar; the second copy passes `IdSuffix="-narrow"`, which every id it carries (its title's, each row's) takes, so no id is on the page twice. Every other reader keeps the one About in its place.

## Markup

`section.detail-card.activity-about` (aligned to the top of its grid row), named by its `h2.activity-card-title` "About" (1.1rem, 16px above the list), then `dl.details-list`: each row a `div` of `dt` (the term, `muted-text` at weight 500) and `dd`. In the `dd`: the value; the marker after it, a space and `span.muted` "(no longer in use)"; then each warning as a `p.field-warning`, 4px apart, **with no role**: standing content, there on every visit (T193).

## The rows

| Row | Value |
|---|---|
| Registrar | The person it is about: "Sipho Ndlovu". |
| The nominee, by its field's label | "Assessor", "Supervisor or mentor", "Reviewer": "Fatima Khumalo". |
| EPA | "PAED-002 — Managing common paediatric presentations"; an EPA not in force now adds " (no longer in use)" in muted words (T231, D48). |
| Encounter | "2026-09-10", or "not recorded (created 2026-09-30)" where nobody stated one (`EncounterDate.Label`, T161). Its `dd` is `activity-encounter-date`. |
| Filed | The filing's day: "2026-09-30", or "2026-09-30, 20 days after the encounter (late)" (D15). The filing is the author's first move out of the first state that leads on, or the create where the create files it. |
| Form | "Version 3", only where a move's unavailable reason names the form's version (T107). |
| Credit | "None until it is completed" (the terminal state's label, lower-cased); "None: a reflective exercise credits nothing"; "None" on a closed record; "1 item" on a done one. |

**An author's own draft, never filed**, has only Registrar, Started ("2026-09-30"), Encounter and Credit: its form says the rest.

## The credit warnings (under Credit, on a done record)

- A paused EPA (D48): "This activity's EPA is paused: its credit waits."
- It credited nothing (T108, T281): "It counted towards no curriculum requirement. The EPA it was recorded against is most likely not part of this trainee's curriculum, or was not in use at the time, or the encounter is dated after the trainee's programme ended. The record is kept. If it should have counted, raise it with the programme administrator."
- Across scales (T109): "It counted towards the required number of observations, but not towards the supervision level. The rating was recorded on a different entrustment scale from the one this trainee's curriculum requirement uses, so the two levels are not comparable. Please raise it with the programme administrator: the curriculum item's scale may need to be corrected."

## Rules (DESIGN.md § Page-level patterns, "Record page with a workflow")

- The nominee is named by its field's label, as the College wrote it.
- An EPA is "Code — Title", with "(no longer in use)" in muted words beside one not in force, as every other page marks it.
- A warning here is standing content, never an alert.

## Contrast

Terms `muted-text`, 5.09:1 on the surface; the marker the same; a warning's words `text-color` on `warning-bg`, 11.89:1, its stripe `warning-color` 5.43:1.

## Known gaps

- The across-scales warning still says "Please", which the voice drops (T109's words, carried over).
- About's date row is always "Encounter", where the boards named it per type ("Case or incident", "Delivered", "Period") (T347).
- Credit builds a noun from the type's name ("a portfolio and logbook review") (T347).
