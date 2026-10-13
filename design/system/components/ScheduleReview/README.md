# ScheduleReview

Schedule a review (`/committee/reviews/new`, `Pages/CommitteeDecisions/ScheduleReview.razor`; its words in `ScheduleWords.cs`, the agenda's in `CommitteeAgendaText.cs`, the types' in `CommitteeDecisionWording`): **put a trainee before a panel for a period**, with the agenda the review will carry beside the form. Flow 08 (T364, 2026-10-13, 622c5ad1; boards R2-4; Q1, Q3; round 3 F1, F3, F4, F6, M5, M9, S1, S4, S6; C1, C4, C6, C7, C11; T309 items 1 and 3, T324, T338) made it a page of its own. Until then it was a card over the Committee reviews list, with text fields for ISO dates, a "Formative only" tick beside a Review type select, and a preview that could not say a seat was already held.

## Who reads it

Coordinator, Administrator, Institutional admin and both speciality admins (`[Authorize(Roles = …)]`), the five that schedule; not the Committee member. It is **owned by Committee reviews** (the owner table, Q1): it lights Committee reviews, and its trail is Home › Committee reviews › Schedule a review whichever page's Schedule opened it (NavMenu, Breadcrumbs). It is reached from Committee reviews' header action, from Decisions due's Schedule on a row and "Schedule a review" on its trainee line.

## The page

`div.committee-calendar`. The header: "Schedule a review", "Put a trainee before a panel for a period. The review opens once it is created." Then one `form.form-container` named "Schedule a review", its content a **`.schedule-grid`**: the fields (`.schedule-fields`, a column 24px apart), the refusal summary and the actions on the left; **the Agenda aside** (`.agenda-preview`) on the right from 1100px, in a 22rem track, the dashboard grid's line; below 1100px the aside goes under the fields, before the summary and the actions (M9).

- **Filled from the query** `?panel=&trainee=&period=&return=` (`ReviewLinks.Schedule`): each value is taken only where the page offers it, so the link authorizes nothing. Opened from Committee reviews with no query, Panel, Trainee and Period are empty.
- **Panel** ("Choose a panel"; help "Only the panels with a trainee you can schedule are listed.", or "No panel has a trainee you can schedule a review for. …").
- **Trainee**, disabled until a panel is chosen ("Choose a panel first", help "Choose a panel first. The list holds the trainees at its institution you can schedule.", C4); then current registrars with their training year, "Lerato Molefe · Training year 4", help "Current registrars at the panel's institution whose programme you oversee, with their training year." (5.29).
- **Period**: the six periods Decisions due offers (M5), "2026 S2 · 1 Jul to 31 Dec 2026"; its help "The semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so the annual EPAs a semester-2 sitting decides are judged on the whole year.", **led by "2026 S1 has ended. A review can still sit for it: sittings are often held after the period ends."** when the period chosen has ended (F1); a semester-2 period adds the window line under it, "The College's semester 2 ends in November; its window runs to 31 December, so December's work counts." (F6), which the select names.
- **Evidence window** (`fieldset.form-group`, legend "Evidence window"): From and To in **the browser's date input** (`InputDate`, F3), side by side while they fit (`.field-pair`, auto-fit 12rem tracks). Filled by Period, still editable.
- **Scheduled on**, a date input, today by default; help "The day the review is put on the schedule."
- **Review type** (`fieldset.form-group`, legend "Review type"; DESIGN.md § Form system, "A choice of one among a few kinds … is radios"): a `.scale-choices` list of `.form-check.review-type-choice` rows, each a radio named by the kind alone, "Annual progression review", "Pre-graduation review", "Entrustment-only review", then **"Formative check-in"** (T309 item 3: the old tick folded in), each with its help under the label as a `small.page-subtitle.review-type-help`, set in from the radio's edge, which the radio names with `aria-describedby` (build review D7): "The semester-2 sitting decides the trainee's progression for the year.", "The final review before the programme ends: it decides the trainee's progression, and whether they may graduate.", "Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress check-ins. It takes no seat for the period." Only the kinds the handler accepts for the panel and period are offered (`CommitteeReviewTypes.Allowed`), the default **checked as the page opens** (F4), so the form never refuses a missing type; a kind chosen by hand is kept while a change still offers it (D4).

## The Agenda aside: advice, never a gate (C6)

`section.agenda-preview` (`info-bg`, a 1px `border-color` edge, `radius-lg`), headed by an h2 "Agenda". It holds **one narrow live region**, `.agenda-live` (`aria-live="polite"`, no `role="alert"` inside it, S6), with only the words that change; the optional lines (`ul.agenda-lines`) and the notes (`.muted`) sit outside it, so a choice is announced once. In order of precedence it says:

1. A check-in: "A formative review carries no agenda: it decides no EPA."
2. **Already held** before Create (Q3, T309 item 1), by the create's own seat check (`GetSchedulingSeatQuery`): a warning alert, "**Already held.** Review #1 already puts Lerato Molefe before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-13). A trainee has one binding review for each period before the panels that decide the same EPAs, until it is ratified: open that review, or ratify it before scheduling another." and "A formative check-in takes no seat: choose Formative check-in under Review type to schedule one."
3. The preview failed: a warning alert, "The agenda could not be read just now." with Try again (`refresh-cw`), and "The review can still be created: its agenda is planned when you press Create review." (T338, T272).
4. Between a choice and the reads returning: "Reading the agenda." (`.muted`, build review D2); before anything is chosen: "Choose a panel, a trainee and a period to see which EPAs the review will be there to decide."
5. Nothing to decide (S1): "No EPA will be on the agenda." (`.agenda-count`) with the handler's own sentence.
6. Else the count, `.agenda-count` (weight 600), "15 EPAs will be on the agenda for 2026 S2.", what is due, "PAED-001, PAED-002, … and PAED-015 are due at this sitting.", then the optional lines, "PAED-008 — Evaluating and managing neurodevelopmental and behavioural presentations in children · As opportunity allows", and the notes.

## The actions

- **While the seat is held, Create is not offered** (Q3): the actions are the reason as a `p.move-reasons.move-reasons--beside`, "Create review is not offered: review #1 already holds 2026 S2 for Lerato Molefe.", Cancel, and "Open review #1" (`btn-primary`, named "Open review #1 of Lerato Molefe"). Choosing Formative check-in offers Create again.
- **Cancel goes back where the page came from** (C4), read only through `ReviewLinks.ScheduleReturn`: Decisions due with its filters, or Committee reviews; a forged `return=` goes to Committee reviews.
- **Create review** runs as § Button system says: "Creating…" with `.is-running` and `aria-disabled`, never disabled, a second press sending nothing; **Cancel is a disabled button meanwhile**, not a followable link (S6); a visually hidden status says "Creating the review.". Created, the page goes to the review (ReviewSitting), whose h1 takes the focus.

## Refusals (C7, T324, S4)

A summary above the actions (`.validation-summary-errors.error-summary`, `role="alert"`, focused once drawn): "**The review was not created.** Everything you chose is kept below.", each line "Field: message" linking to its field, and the same message beside the field. The page's own checks are Wombat's words: "Choose a panel.", "Choose a trainee.", "Choose the period the review sits for.", "Choose the day the window starts.", "Choose the day the window ends.", "Choose the day the review is scheduled on.", "The window ends before it starts. Choose a day on or after 2026-01-01.". A handler's own refusal (`CommitteeRefusedException`) shows its words; **the race on a held seat** (a second tab) re-reads the seat, so the aside turns to Already held, and the summary adds a short "Open review #1" (S4); any other failure is "The review could not be created just now. Nothing has changed. Try again, or come back in a few minutes.", logged (T338).

## States (C11)

- **Loading**: the header at once, the status "Loading what you can schedule.", the form's shape (`aria-busy`): skeletons for the fields, the Agenda heading, and Cancel; no Create.
- **Load failed**: "Could not load what you can schedule. Nothing has changed. Try again, or come back in a few minutes." with Try again.
- **Someone the command refuses outright** (who also holds Trainee, T216): the empty card "No review can be scheduled", with the trainee note and "Open My committee reviews"; no form. A role that schedules nothing reads "Your role does not schedule committee reviews.".

## Narrow widths

Below 1100px the aside goes under the fields. Below 641px every control and radio row is 44px, the actions stack **with the primary on top**, each 44px and the row's width (`column-reverse`), the held seat's reason first above the buttons it explains (`order: 1` in the reversed column, build review D1), and the aside's padding is a step smaller.

## Contrast

No new colour token. In the aside, body text 11.84:1 and its notes (`muted-text`) 4.77:1 on `info-bg` (app.css's own figures); the aside's `border-color` edge (1.22:1) is decoration, its heading naming the region; the Already held alert's words 11.89:1 on `warning-bg`, its edge 5.43:1. The review type's help (`muted-text`) 5.09:1 on the surface.

## Known gaps

- **T365**: at a semester-1 sitting the aside labels an EPA decided as opportunity allows "Due by year end" (the replay's 4.2: PAED-008, 009 and 013), where Decisions due reads "As opportunity allows" for the same period.
