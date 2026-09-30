# InstrumentPicker

What Log an activity (`/activities/new`, `NewActivity.razor`) shows before a type is chosen: every activity type the caller may file, as links, in a card a group by what filing one means. Flow 03 (T342, 2026-09-29, 725237ee; Q1) made it: until then the page opened on a select of every type and the form under it.

## What the consumer provides

`<InstrumentPicker Types="_activityTypes" />`

- `Types` (required): the types `ListActivityTypesQuery` returned, already filtered by scope and ladder (T111, T123), so a type the caller may not file is never here. Each carries its `Shape` (`ActivityTypeShape`: `Rated`, `DiscussedOrReviewed`, `LoggedByYou`, read from the published form) and `CreditsNothing` (an empty `counts_for`).

The page heads it with PageHeader: "Log an activity", subtitle "Choose what you are filing. Each opens its own form." Its StatePanel draws three 8rem skeletons while the list loads (a visually hidden status says "Loading what you can file.") and, on a failed read, "Could not load what you can file. Nothing has changed. Try again, or come back in a few minutes." with Try again.

## The groups

In this order, each only when it holds a type; within a group, the types by name:

| Heading (`h2`) | Its line |
|---|---|
| Rated by an assessor | "You name an assessor. They rate the encounter on 1, 2, 3a, 3b, 4, 5 and write feedback. Completed, it counts towards the EPA." |
| Discussed or reviewed, not rated | "You name a supervisor or reviewer, who discusses or reviews it with you. No rating." |
| Logged by you | "Only you fill it in. It is logged at once, and credits nothing." (", and credits nothing" only when every type in the group credits nothing) |

Beside the heading, muted, its count: "1 type", "7 types".

## Markup

- `div.stack-list` of `section.detail-card.instrument-group`, each named by its `h2` (`aria-labelledby`, ids `group-rated`, `group-discussed`, `group-logged`).
- `.instrument-group-head`: the heading and the count on one baseline, wrapping; then the line, `p.text-muted`.
- `ul.instrument-picker`: an auto-fill grid of `minmax(min(18rem, 100%), 1fr)`, 8px apart. Each `li` holds `a.instrument-link` to `/activities/new?type=<key>`: the type's name, then a 16px `chevron-right` at the far end. The link is 44px (2.75rem) at every width, padded 8px by 12px, `radius-md`, edged `input-border`, `link-color` at weight 600 on the surface, no underline until the pointer is on it (then `hover-bg` and the underline).

## Choosing a type

A link keeps the page (the same route, a new `?type`), so FocusOnNavigate does not run: once the new address has loaded, the page's h1 takes the focus (`PageFocus.FocusHeadingAsync`, wombat.js's `wombat.focusHeading`). The form's subtitle is then "Mini-CEX (Paediatrics) · Choose another type" (PageHeader's `SubtitleContent`), the last part a link back to the picker, which does the same.

## Empty

With no type the caller may file: a `.detail-card--empty`, "Nothing can be filed yet." · "No activity type is open to you. If you expected one, ask your programme administrator which ones your programme uses."

## Rules (DESIGN.md § Form system, "The activity form")

- A type the system writes (Multi-Source Feedback, Learner Feedback) is never offered (T162, T164); nor is one of another discipline.
- The link is the type's name as the College wrote it, never its key.
- The group is decided by the form, not by the type's name: a builder's new type lands in the group its shape says.

## Contrast

The links' words `link-color` on the surface, 6.70:1, their edge `input-border` 3.80:1; the count and line `muted-text`, 5.09:1.

## Known gaps

- The boards counted a group as "(7)"; the build says "7 types" (T347).
