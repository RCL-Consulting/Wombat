# NewMsfCampaign

New MSF campaign (`/msf/campaigns/new`, `Pages/MultiSourceFeedback/NewCampaign.razor`; its words in `MsfCampaignText.cs`): **the create form, a page of its own**, four cards in a column of the form's measure, creating a draft that emails nobody. Flow 10 (T369, 2026-10-19, 68327c9d; boards R3-C1 to C8; round 3 F2, F6, C1–C8, S5, S12, M4, M9; T224, T248, T284, T324, T325, T329; build review D5) split it from the campaign's page. Until then the create form sat on `/msf/campaigns/{id}` above a campaign that did not exist yet, a group's minimum could be 1 or 2, the window's days were the server's, a refusal printed a model property's name, and a failed read of the templates ended the circuit.

## Who reads it

Coordinator and Administrator. Owned by MSF campaigns for the Coordinator (NavMenu): the trail Home › MSF campaigns › New MSF campaign, MSF campaigns lit. Someone who holds Trainee, or a Coordinator at no institution, is shown only the standing warning (`#msf-runs-no-campaigns`, `Role=""`) in the words the create and template commands refuse them in, and nothing is read for them (T224, T248): no form, no Questionnaire card, no trainee list.

## The page

The header: "New MSF campaign" (h1 and tab), the subtitle "One campaign for one current trainee: its questionnaire, its window, its thresholds and the EPAs it is evidence for. Creating it makes a draft; nobody is emailed until you open it." Then, in `.msf-page.msf-form-column` (52rem): "* marks a field you must fill in." (`p.field-help`) and four `.form-container` cards (padded 24px), each titled by an `h2.msf-card-title`, some with a muted `small` after the title:

1. **Questionnaire** ("Written once, used by every campaign that names it."). The result of Add template in an `ActionResult` (focused): "**Template created.** Default MSF is chosen as this campaign's template." (C2). **Template** (required mark, `aria-required`): each template with its kind, "Default MSF (Multi-source feedback)", its questions under it as help ("Rates the trainee's overall professional performance. · What should the trainee keep doing or improve?"); the first template is chosen, so a create needs no pick where there is one. It joins the campaign's form by `form="campaign-form"`. Under it **the fold**, `details.template-fold` with a `plus` and "Write a new questionnaire", **outside the campaign's form** (S5): Kind (radios in `.choice-stack`: Multi-source feedback, Learner feedback), Template name, Scale question ("Answered on five points, from Well below expectations to Well above expectations."), Comment question, and Add template (`type="button"`, its own handler). **No template yet** (C1): the fold is open and pre-filled, the select reads "No template yet" with "Write the first one below; it is chosen here once it is added." and is natively disabled (the one exception to `aria-disabled` on these forms: a select whose one option is a placeholder offers nothing to choose or to explain; build review D5).
2. **Trainee and window**: **Trainee** ("Choose a trainee", then "Lerato Molefe (molefe@kgk.wombat.local)"; "Current trainees you can run a campaign for.", exactly those the create accepts, never the caller, T238, T248); **Opens on**, today ("Today, on the South African calendar.", T325) and **Closes on**, two weeks later ("The respondents' last day to answer, on the South African calendar."), both the browser's date input.
3. **Thresholds** ("Nothing is shown of a respondent group that falls short, so no answer can be traced to its author."): Minimum responses (8 by default; "Responses needed in all before the report can be released."), **Minimum responses in a respondent group** (3; "A respondent group with fewer is suppressed: its rating and comments are never shown. At least 3 for multi-source feedback.", **refused below 3 for either kind**, "Enter a number from 3 to 100.", F2), Minimum respondent groups reporting (2; "… Two is the College's answer: eight peer doctors alone are not multi-source feedback."; on learner feedback `aria-disabled` and read-only at 1, "Learner feedback has one respondent group, so its groups-reporting minimum is fixed at 1.", M4).
4. **Evidence**: `fieldset.form-group.choice-row.choice-group` (S12), its legend "Evidence for these EPAs" with the mark and a hidden "required", its help ("One campaign covers many EPAs and runs once a semester. Each one selected here becomes a separate evidence record on the trainee's portfolio when the report is released."; "Select a trainee first." until one is), **"2 of 15 ticked"** (a bold `role="status"` line), and the boxes in two columns (`.epa-checks`), each "PAED-010 — Leading and operating within a clinical team". Then `.form-actions`: Cancel (outline, back to the list) and **Create campaign** (primary); with no template it is `aria-disabled`, no submit, and "**Create campaign:** write a questionnaire first." in `.move-reasons`.

## A refused create (M9; RefusalSummary)

`div.validation-summary-errors.error-summary`, `role="alert"`, focused: "**Not created.** Fix the field below and create the campaign again." ("Fix the 2 fields below …"), each line a link that moves the focus to its field (the EPA group's to its first box, never the legend), never navigating. Each field's own message under it (`.validation-message`, `aria-invalid`, after its help in `aria-describedby`): "Choose the trainee this campaign is about.", "Enter a last day on or after the day the campaign opens.", "Select at least one EPA this campaign is evidence for." (the group's invalid stripe, `.choice-group.is-invalid`). A handler's own refusal is the summary's text; anything else one fixed sentence, logged (build review R1).

## States (C6, T329)

- **Loading**: the required line, then four skeletons, said once ("Loading the templates and trainees.").
- **Load failed**: the fixed-word load error with Try again (StatePanel); the circuit stays up.

## Narrow widths

Below 641px every control and radio row is 44px, the form's actions stack the page's width with Create campaign on top, the cards pad 16px. The EPA boxes go to one column when two do not fit. Nothing scrolls sideways at 390.

## Contrast

No new colour token. Labels and text 12.63:1; help and the cards' muted titles 5.09:1; the required mark as FormField's; the fold's summary and Cancel 4.86:1.

## Known gaps

- **Found re-syncing:** the three cards inside the campaign's form (Trainee and window, Thresholds, Evidence) touch, with no space between them: `.msf-page`'s 24px gap spaces its children, and the three are children of `form#campaign-form`, which has no gap of its own (`campaign-edit--new.png`). Not filed.
