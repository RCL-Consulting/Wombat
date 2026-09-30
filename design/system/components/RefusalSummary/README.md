# RefusalSummary

A refusal that names fields, said once at the head of the work (`Components/Shared/Activities/RefusalSummary.razor`): a bold title that says what did not happen and what state the work is still in, a line that says what to do, and one line a field in the refusal's own words, each a link to that field's input. The same component on Log an activity, on the activity's page and in its note panel. Flow 03 (T342, 2026-09-29, 725237ee; C8, T263) made it; until then a refusal was a danger Alert printing the server's message whole, and only at the head.

## What the consumer provides

```razor
<RefusalSummary @ref="_summary" Title="Nothing was saved." Text="Everything you typed is kept below." Lines="_refusalLines" />
```

- `Title` (required) and `Text`: the words, from `RefusalWords` on the activity's page and fixed on Log an activity.
- `Lines`: the refusal cut one field a line (`FieldRefusals.Split`, at each field's label): each `RefusalLine` holds the field's key, the line's words and the field's own part. A line about no field is plain text.
- `Id`: `refusal-summary` by default (every refused field names it); the note panel's is `note-summary`.
- `AutoFocus`: for a refusal that arrives with the page (a submit refused straight after the create, handed over from Log an activity), so it wins over FocusOnNavigate's h1.
- The page calls `FocusAsync()` once it has drawn the summary.

## The words

| Where | Title | Text |
|---|---|---|
| Log an activity, a refused create | Nothing was saved. | Everything you typed is kept below. |
| The activity page, a refused move | Not submitted. It is still a draft. ("Not declined.", "Not completed.": a move that ends the activity reads as the state it would have ended in; "Return was not made." for one with no word) | Fix the field below and submit again. / Fix the 2 fields below and submit again. |
| The activity page, a refused Save draft | Not saved. It is still a draft. | Fix the field below and save again. |
| The activity page, the submit refused straight after the create | Saved as a draft, but not submitted. | Fix the 6 fields below and submit again. |
| The note panel | Not declined. | (none) |

Then the lines: "Date observed: The date cannot be after today (2026-09-30).", "Presenting problem: A value is required.", "Reason for Sipho Ndlovu: Decline requires a note."

## Markup

`div.validation-summary-errors.error-summary[role=alert][tabindex=-1]` with the id: the danger summary of § Validation (body text on `danger-bg`, a `danger-color` edge thickened to 4px, the circle-alert icon drawn by the stylesheet), then `<strong>` the title, a space and the text, then `ul.refusal-summary-list` (4px above, 1.1rem indent), each `li` holding `a[href="#<key>-in"]`. In a column that spaces its blocks with a gap (`.form-column`, the note panel) it has no margin of its own.

## Focus

- **The summary takes the focus once drawn** (and by `autofocus` on arrival), so a screen reader starts there; `role="alert"` announces it too.
- **A link moves the focus to its field's input**, never to the field's wrapper (C8). The `href` is only for a page with no circuit: the app's `<base href="/">` resolves a bare fragment against the site's root, so a followed link would navigate to Home and lose what was typed. Pressed, the link's handler prevents that (`@onclick:preventDefault`) and calls `PageFocus.FocusByIdAsync` (wombat.js's `wombat.focusById`), which makes a target that cannot take the focus of itself (a fieldset, a read-out) focusable first.
- Each refused field names the summary last in its `aria-describedby`, after its help and its own message, so the refusal is read with the field, not only announced once.

## Rules (DESIGN.md § Alerts, validation, empty states, "An action's outcome on a record page")

- A refusal that names fields is this summary. **A refusal that names no field** is a danger Alert above the page's cards, and the focus stays on the button pressed.
- The lines are the server's own words, cut at each field's label; never a notice's sentence rewritten.
- The summary and the marks are cleared together on the page's next action, so a field is marked exactly while the summary that names it is shown; a field changed since drops its own marks at once (ActivityForm).

## Contrast

Words `text-color` on `danger-bg`, 11.81:1; the links `link-color` on the tint, 6.27:1; the edge and icon `danger-color`, 5.56:1.

## Known gaps

- **The summary keeps a corrected field's line until the next submit** (Step 3.10). Once the registrar changes a refused date, the field drops its marks and shows its new hint (the late warning, say), but the summary above still reads the last refusal ("Date observed: The date cannot be before your programme started (2026-01-15)."), so for a moment the page says two things of one field. The operator kept it as the default: the summary reports the last submit, and the next action clears it.
