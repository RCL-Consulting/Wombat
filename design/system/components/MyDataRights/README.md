# MyDataRights

My data rights (`/account/data-rights`, `Pages/Profile/DataRights.razor`; its words `MyDataRightsWords.cs`, the words it shares with the staff pages `Components/Shared/DataRightsWords.cs`, a type's and a status's labels `Wombat.Application/Features/DataRights/DataRightsLabels.cs`; the reads `GetMyDataRightsRequestsQuery`, `GetObjectionFlagsQuery`; the commands `SubmitDataRightsRequestCommand`, `WithdrawDataRightsRequestCommand`, `UpdateObjectionFlagsCommand`): **what a person has asked about their own data, and the answers; a new request; and the one preference left, the reminder emails.** Flow 14 (T387, 2026-10-30, e8eea1d2 with 5b5aab80; boards R3-M01 to M18; Q4 to Q8, Q14, Q20, Q24, Q27, F7, M2, M11, M13, M18, N3, N4, N12, N17, S24) rebuilt it in flow 12's column of sections. Until then it printed a request's type and status by their enum names, showed no answer, offered an optional-processing box (gone, Q6, T318) and printed an exception's message on a failure.

## Who reads it

Everyone signed in (`[Authorize]`): a personal link, under the menu's divider for every role, a former trainee and a pending registrar among them (NavMenu). It is a list page, so it lights itself and has no trail. Its own requests only; who decided is never shown (Q5, T319).

## The page

The header: h1 and tab "My data rights", subtitle "Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails." No header action. The column is `<div class="user-page data-rights-page">` (flow 12's `.user-page`: a flex column of `section.detail-card.user-section`s 24px apart, each an h2), three sections in this order (Q4):

- **Your requests** (`h2#dr-requests-heading`):
  - None yet: `.detail-card.detail-card--empty`, "No requests yet." (`.state-panel-title`) over "You have not asked for your data, a correction or erasure. Make a request below." (`.state-panel-copy`).
  - Else a `DataTable` with `StackWide` and `Class="data-rights-table"` (DataTable), captioned "Your data rights requests, newest first". Columns: **Submitted**, the row header (`th scope="row"`), the moment in SAST to the minute ("2026-10-30 20:10 SAST", `DataRightsWords.When`); **Type** by label ("Access", "Export", "Rectification", "Erasure"); **Status**, a badge (`BadgeFor.DataRightsRequest`: Submitted blue, Approved amber, Completed green, Rejected red, Withdrawn grey `badge-withdrawn`; Badge); **Answer** (`td.answer-cell`, `MyDataRightsWords.Answer`); and an unlabelled actions column (`.visually-hidden` "Actions").
  - **The answer** (Q5, T319): "Waiting for a decision."; "You withdrew it before a decision."; "Rejected on 2026-10-30: “‹note›”."; "Approved on 2026-10-30: “‹note›”." (an Access, Export or Erasure, which complete as they are approved, keeps this answer once Completed); an approved Rectification adds "Being corrected; it reads Completed once done."; a completed one, "Completed on 2026-10-30. Corrected: Account › Last name." (each record by its label, `DataRightsWords.Record`). The day is the South African day; a note in curly quotes, and a note that ends its own sentence takes no second full stop ("“Identity confirmed; export released to the data subject.”").
  - **Withdraw** (outline, small) only while Submitted (Q14), no dialog; **Download** (primary, small, `download`) on a Completed Access or Export, a link to `/account/data-rights/download/{id}`. Each named for its row (T239, N4, N12): "Withdraw the Export request made 2026-10-30 20:10 SAST", "Download the export of the Export request made …", "Download the copy of your data for the Access request made …"; to the second ("2026-10-28 00:01:07 SAST") where two of one type share the minute, and numbered where the second still leaves them alike (`RowNames.Distinct`).
- **Make a request** (`h2#dr-make-heading`):
  - The intro, naming who decides (M18): "Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. Staff at Kgosi Kgari Teaching Hospital decide it; the answer shows under Your requests." ("… The Wombat administrator decides it; …" for a person with no institution).
  - **Request type \***, flow 11's described radios: `fieldset.form-group.choice-group.choice-help-stack`, legend "Request type" with `span.required-mark` " \*" and a `.visually-hidden` " required"; the four offered types (`DataRightsLabels.OfferedTypes`; never Objection, Q8), **none chosen as the page opens**, each `input#dr-type-‹Type›` described by its `p.page-subtitle.choice-help#dr-type-‹Type›-help`: Access "Everything Wombat holds about you, as a download, once the request is approved."; Export "The same download, to take your data elsewhere: machine-readable data with a PDF summary."; Rectification "Something about you is recorded wrongly. Say what it is and what it should be."; Erasure "Your account and personal data are removed; your training records are kept under a name that identifies nobody. Once approved it cannot be undone, and you can no longer sign in."
  - **Reason \*** (`textarea#dr-reason`, four rows, `aria-required`, no `maxlength`), then its counter `p.field-help#dr-reason-count` "0 of 4,000 characters", counted in the browser as it is typed (`wwwroot/js/review-appeal.js`'s `count`, which writes into Blazor's own text node, so a box Blazor empties after a submit reads "0 of 4,000 characters" again: 5b5aab80); a count past 4,000 shows the excess.
  - The next step, `p.field-help`: "What happens next: staff at Kgosi Kgari Teaching Hospital read it and approve or reject it with a note, and you are emailed when it is decided. While it waits you can withdraw it under Your requests."
  - `.form-actions` **Submit request** (primary). No `<form>` element: the button's own handler sends.
- **Reminder emails** (`h2#dr-reminders-heading`, Q24): "Wombat processes what your training programme needs (your record, assessments and what regulation requires), and that cannot be objected to. You can stop its regular reminders."; one checkbox `input#dr-reminders` "Stop the regular reminder emails", described by `p.field-help#dr-reminders-help`: "Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such as an invitation to give feedback, a coordinator's reminder about one activity, or a notice that an entrustment decision is about to expire or has expired." (the 5 is `DashboardThresholds.AssessorNudgeDays`); `.form-actions` **Save** (primary).

## Refusals before a send (M02, N17)

Submit checks the page first; nothing is sent while either is refused, the values are kept, and the first refused field takes the focus (the first radio, else the reason):

- No type: "Choose the request type." as `div.validation-message#dr-type-msg[role="alert"]` under the legend, the fieldset `.is-invalid` and `aria-describedby` it, each radio `aria-invalid`. Choosing a type drops it.
- No reason: "Please provide a reason for your request."; past 4,000 characters: "Give your reason in 4,000 characters or fewer." As `div.validation-message#dr-reason-msg[role="alert"]` under the box, the box `.input-validation-error` and `aria-invalid`, described by the refusal first, then the counter (`dr-reason-msg dr-reason-count`, N17). A changed reason drops it.
- The validator's own refusal of Type or Reason lands under its field the same way.

## Results (M11, M13)

One result on the page at a time, in the section whose action it answers, a fresh `ActionResult` (`FocusOnLoad`) that takes the focus as it is drawn; no h2 takes the focus. Success is a success alert, a failure a danger alert; none is bold.

- Under **Your requests**: "Request submitted. You will be notified when it is reviewed." (the type and reason cleared, the list read again); "Request withdrawn."; "This request was decided before it could be withdrawn. Nothing was withdrawn." (the row read again, showing the decision); "The request could not be withdrawn just now. Nothing has changed. Try again, or come back in a few minutes."
- Under **Make a request**: "The request could not be sent just now. Nothing was sent. Try again, or come back in a few minutes." (type and reason kept).
- Under **Reminder emails**: "Reminder emails stopped." / "Reminder emails will be sent again.", or, when the save changed nothing (N3), "Nothing changed: reminder emails are still stopped." / "Nothing changed: reminder emails are still sent."; failed, "The reminder emails could not be saved just now. Nothing has changed. Try again, or come back in a few minutes."

The button pressed is never disabled while it runs (`aria-disabled="true"`, a second press sends nothing, T234); the other actions are natively disabled meanwhile.

## States

- **Loading**: `p.loading-line[role="status"]` "Loading your data rights." over three skeleton cards (`aria-hidden`).
- **Load failed**: "Could not load your data rights. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`), which focuses the h1 once the read succeeds.

## Narrow widths

Your requests stacks from 900px (`.clinic-table--stack-wide`): the Submitted moment the block's first line, with no rule under it (`.data-rights-table.clinic-table--stack-wide tbody th[scope="row"]`, review P7), then "Type:", "Status:" and "Answer:" lines; the answer wraps inside its cell, never widening the table (`.answer-cell`, `overflow-wrap: anywhere`). Below 641px (`.data-rights-page`): a stacked row's Withdraw or Download takes the row's width; the reminder checkbox's label is a 44px row (outside a `.choice-group`, so `.user-page`'s radio rule does not reach it); `.user-page`'s rules give the rest (every control 44px, a described radio's label a 44px row, Submit request and Save the row's width). Measured on the replay at 390, 768 and 1280: nothing sideways (A.7.3, A.7.5).

## Contrast

No new colour token. The help, counter and choice help in `muted-text`, 5.09:1 on the surface.

## Known gaps

- The loading column has no `aria-busy` (the queue's and a request's have one); noted by the states pass, not filed.
- A failed submit or save takes about a minute (two 30-second timeouts) with nothing said meanwhile; noted by the states pass, not filed.
- The opt-out does not reach every scheduled mail (T389, proposed); a name changed on My account reverts at the next institutional sign-in (T388, proposed).
