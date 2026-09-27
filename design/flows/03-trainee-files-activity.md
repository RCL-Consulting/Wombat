# F03 A registrar asks for an assessment, or logs one, and follows it to credit

**Who and why:** a registrar files observed work (25 observations a semester, Annexure B's total:
`act-1-setup.md:410`, Step 1.18) and follows each piece to Completed and credited. It is the product's most frequent
write, and progress, reviews and STARs all rest on it. This flow designs the activity renderer and the activity page once. F04
(the assessor) and F07 (the chair reading evidence) extend that design.

| | |
|---|---|
| Mode | **Wireframe first** for the activity renderer: field types, sections, the workflow action bar and the history. Then fidelity. Design the renderer, not 22 instruments (BRIEF.md § 5.2, the jsonb forms). |
| Viewports | Desktop 1280×800 and phone 390×844 |
| Variations | 2–3 |
| People | Trainee: Dr Anele Dlamini (year 3), Dr Sipho Ndlovu and Dr Nomsa Mahlangu (year 1), Dr Pieter du Plessis (year 2), Dr Lerato Molefe (year 4) |
| Runbook steps | 30, pasted verbatim in § 8 |
| Pages (`coverage.md` templates) | `/activities/new`, `/activities/{ActivityId:int}`, `/activities/mine`, `/activities/inbox`. It ends on `/portfolio/progress` (F05) and the trainee's Activity inbox card on `/` (F05 owns the dashboard). |
| Components drawn at runtime | `Components/Shared/Activities/ActivityForm.razor`, `ActivityDetail.razor`, `ActivityWorkflowActions.razor`; pages `Components/Pages/Activities/NewActivity.razor`, `ActivityView.razor`, `MyActivities.razor`, `ActivityInbox.razor` |
| Held | Nothing. T297's 3 images were re-captured on 2026-09-26 (§ 4) |
| Depends on | F01's tokens, the shell, and its loading, load-error and validation styles |

**How to run this thread** (BRIEF.md § 2.3):
1. Brief F01 first. Open a new Claude Design thread for this flow alone.
2. Paste § 1, and attach the key screenshots in § 4 with it. Attach the seed JSON files listed in § 4 too.
3. Paste § 8, the runbook steps, as the next message.
4. Attach the state screenshots in § 4 as the chat asks for them.
5. Pick one renderer wireframe, then ask for fidelity on New activity, the activity page and My activities.
6. Answer § 6's questions in the chat, one sentence per decision. Export the chosen artboards into
   `design/flows/03-trainee-files-activity/`.

---

## 1. The ask (paste this first)

```text
FLOW 03 — File the right assessment, name the right assessor, and follow it to credit

GOAL: A registrar files the right instrument on the right EPA and date, names a permitted assessor, submits, and
  follows it to Completed and credited. She recovers from refusals, from a request the assessor declines, and from a
  reflection returned for more work.
  What is wrong today (the attached screenshots show each):
  - A Submit or Complete that succeeds says nothing, and the focus drops to the page body.
  - A refused move with a note (Decline, Return) closes its note panel, and the note is lost.
  - After a refusal names the date, typing a late date there shows no late-filing warning.
  - At 390 px the form sits three cards deep, its inputs are 193 px wide in a 343 px column, and the chosen EPA and
    assessor are cut off in their selects.
  - Textareas render in monospace.
  - New activity has no skeleton while it loads, and a failed read ends the whole session with "An unhandled error has
    occurred. Reload".
  - The activity page's summary shows the form's version number, and does not say whose activity it is.
  - Nothing emails the assessor when a request is filed, but another page says a request to assess is emailed.

AUDIENCE: Trainee (registrar); desktop 1280×800 and phone 390×844. Cast: Dr Anele Dlamini (year 3), Dr Sipho Ndlovu
  and Dr Nomsa Mahlangu (year 1), Dr Pieter du Plessis (year 2), Dr Lerato Molefe (year 4). Assessors named in the
  story: Dr Naidoo, Dr Khumalo, Dr Botha, Dr Patel, Dr Zulu.

WHAT THE FORMS ARE: every activity form is drawn at runtime from a JSON schema, so design the renderer's vocabulary,
  not one form (three schemas are attached as examples):
  - Field types: text, long text, number, date, choice, multi-choice, scale (the entrustment ladder), EPA, user (a
    person picker), file. The builder can also make Likert, procedure reference and signature fields.
  - Each field has a label, may be required (marked *), and may carry help text, which is linked to the input.
  - Fields sit in sections. In each state a section is either open to the viewer or locked (read-only), depending on
    who they are: in a Mini-CEX, the trainee fills in Request; the assessor fills in Entrustment and Feedback.
  - The workflow offers moves as buttons (Save draft, Submit, Cancel; for the assessor, Complete and Decline). Some
    moves need a note (Decline, Return), which opens a note field before the move is applied.
  - The history is a table of moves: action, from state → to state, actor, when, credit ("1 item" or "—"), note. A
    late filing adds "Filed 20 days after the encounter".
  - Three instrument shapes to design for:
    rated (Mini-CEX: Request → Entrustment on the six-rung ladder 1, 2, 3a, 3b, 4, 5 → Feedback;
      Draft → Requested → Completed, or Declined, or Cancelled);
    unrated (Reflective Exercise: the trainee's reflection, then the supervisor's Discussion; Draft → Awaiting
      discussion → Discussed, or Returned to Draft, or Cancelled);
    self-logged (KGK Teaching Session Log: one section, no rating, Draft → Logged or Cancelled, credits nothing).

SCREENS, in order:
  1. New activity: choose the instrument, then the EPA, the encounter date and the assessor, then the instrument's
     form. Show it for a rated (Mini-CEX), an unrated (Reflective Exercise) and a self-logged (Teaching Session Log)
     instrument, with the late-filing warning and the two date refusals.
  2. The activity page, for its subject (the registrar): draft saved; submit refused; saved as a draft but not
     submitted; requested; submitted late; declined (with the assessor's reason); returned; discussed; logged;
     cancelled; awaiting review; completed and credited.
  3. The result of a move: "Submitted. It is now Requested." in a result region that takes the focus.
  4. My activities: drafts, a mixed list, empty, loading.
  5. The registrar's view of the activity inbox: work returned to her.
  6. Every screen at 390 px.

  Words the pages use today (the runbook quotes them):
  - "Draft saved. It has not been submitted."
  - "Submitted. It is now Requested." / "… Awaiting discussion." / "… Logged." / "… Awaiting review."
  - "Saved as a draft, but not submitted: … Fix the fields below and submit again."
  - "Presenting problem: A value is required."
  - "Nothing was saved. Date observed: The date cannot be after today (<date>)."
  - "This date is before the trainee's programme started (<date>), and will not be accepted."
  - "This encounter was 20 days ago. It can still be filed, but a filing more than 14 days after the encounter is
    recorded as late."
  - "Activity unavailable. The requested activity could not be loaded."
  - "No activities yet"

STEPS: 2.42, 2.43, 3.1, 3.2, 3.3, 3.6, 3.7, 3.8, 3.9, 3.10, 3.12, 3.14, 3.16, 3.18, 3.19, 3.20, 3.21, 3.22, 3.23,
  3.25, 3.27, 3.29, 5.24, 6.16, 6.20, 6.27, 6.37, A.2.7, A.6.6, A.7.1. Pasted verbatim in the next message (Role /
  Route / Do / Expect). Where an Expect quotes today's wording, that is today's page, not a requirement on the new one.

STATES TO SHOW:
  New activity: no type chosen; Mini-CEX; Reflective Exercise; Teaching Session Log; refused (future date); hint
    (before the programme); refused (before the programme); late-filing warning; the EPA picker without a paused EPA;
    loading; narrow.
  Activity page (the registrar's view): draft saved; refused submit; saved as a draft but not submitted; requested;
    submitted late; declined; returned; discussed; logged; cancelled; awaiting review; completed; loading; load error;
    narrow.
  My activities: a draft; a mixed list (logged and cancelled); empty; loading; load error; narrow.
  Activity inbox: the registrar's returned work.
  Data volumes: an instrument picker of 11 items; an EPA picker of 1 to 17 items with long names ("PAED-015 —
    Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research");
    My activities with 0, 3 and 40 rows; a history of 2 and 6 rows.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T299: a move that succeeds reports its outcome in a result region and moves the focus there. A refused move keeps
    its note panel open, with the note as typed, and names the note field. A button is not disabled by its own action
    (it is aria-disabled while it runs). A value changed after a refusal drops its refusal mark, and the form's own
    hints (the late-filing warning included) show again.
  - T323: at 390 px the form is one card deep, inputs are at least about 290 px wide, and the chosen EPA and assessor
    are not cut off in their selects.
  - T329: New activity shows its header and a skeleton from the first render, and a failed read shows an alert on the
    page; it never ends the session.
  - T320: no screen may say or imply that the assessor is emailed. Nothing sends one; the inbox is the only notice.
  - T328: textareas and selects use the body font, not monospace or Arial.
  - T325: every time in the history is South African time with its zone ("2026-09-26 12:14 SAST").
  - T324: states and types by their labels; people by name.
  - T190: an activity page's tab title names its type (today every one is just "Activity").
  - T280: a dashboard link to an activity is named by more than its instrument (the EPA or the date too).
  - T297 (being fixed): the registrar's Activity inbox card on Home lists exactly what the inbox page lists.
  - Observed, not filed: the summary card shows "Version: 1" (the form's schema version) and never names the registrar
    whose activity it is.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Is choosing the instrument a first step of its own (a picker of 11 instruments), or one field among the rest?
  2. How does the form show which fields the assessor, not the registrar, fills in? Today those sections are shown
     greyed and locked.
  3. Where does the history (each move, who, when, days after the encounter) sit, and how much of it shows by default?
  4. How are the College's labels shown: the entrustment rungs (1, 2, 3a, 3b, 4, 5) and the instruments (Mini-CEX,
     CbD, DOPS, …)?
  5. A move shown disabled with its reason (for example, a required field its mover cannot write): show the move
     greyed with the reason beside it, or hide it and explain?

CONSTRAINTS:
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side behaviour beyond a
  ≤10-line script module; no live validation; the phone nav toggle is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence.
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token or a NEW token with its value; spacing on the scale xs 4, sm 8, md 16, lg 24, xl 32,
  2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, primary action),
  Breadcrumbs, DataTable (.clinic-table in .table-container, PagerControls), FormField / FormActions (.form-container,
  .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid), StatePanel (loading / empty / error),
  Skeleton, Alert (success / info / warning / danger), ActionResult, ConfirmDialog (native <dialog>), badges (five tints),
  Icon, TrajectoryChart (hand-drawn SVG, no chart library).
Design these framework states explicitly (no source file shows them): the active nav item; field validation (invalid
  border plus a stripe, not colour alone; message under the field; the summary); the reconnect dialog (rejoining,
  retrying, failed, paused, resume-failed); the in-app error bar; access denied; not found; session ended.
Content: people by name, states and types by label, times in South African time with the zone shown; an out-of-scope
  record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (sidebar included); targets ≥ 24 px; focus
  moves to an action's result; every page works at 390 px with no sideways scroll.
Viewports: 1280×800 and 390×844.

ASK:
  - 2–3 variations as wireframes first: New activity and the activity page, each for the rated and the unrated
    instrument, at 1280 and 390 px. After I pick one, full fidelity for every screen and state above.
  - Design the activity page once, with role and state variants: flow 04 (the assessor) and flow 07 (the committee
    chair reading evidence) will extend it. Keep one vocabulary for sections, locked sections, the action bar, the
    note panel, the result region and the history.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    breaks.
  - Flag edge cases: a 17-item EPA picker with long names, a long presenting problem, a history of 6 rows at 390 px, a
    form with 8 long-text fields.
  - Run an accessibility review against WCAG 2.1 AA: labels and required marks, help text linked to its field, the
    refusal named by its field, focus after each move, and keyboard-only filing.

ATTACHED: act-2/2.42-1-molefe-new-activity.png, states/new-activity--mini-cex.png, act-3/3.23-1-dops-picker.png,
  states/activity-view--refused-submit.png, states/new-activity--late-warning.png,
  states/activity-view--submitted-late.png, states/activity-view--declined.png, states/activity-view--returned.png,
  states/new-activity--teaching-log.png, act-A/A.7.1-3-submitted-requested.png,
  act-A/A.7.1-4-late-filing-warning-fresh-form.png, states/new-activity--narrow.png, states/activity-view--narrow.png;
  seed files mini_cex_cpsa/{schema,workflow,credit}.json and reflective_exercise_cpsa/{schema,workflow,credit}.json;
  the Teaching Session Log's definition (pasted as text).
```

---

## 2. The journey

| Step | Page | The registrar does | She must be able to see |
|---|---|---|---|
| 2.42 | `/activities/new` | Dr Molefe reads the instrument picker | the eleven instruments, each once, by name; which are rated |
| 2.43 | `/activities/new` | opens a Mini-CEX by link and reads the pickers | the sections (Request, Entrustment, Feedback), Save draft and Submit; only EPAs whose tool list names the Mini-CEX; only her institution's active assessors |
| 3.1 | `/` → `/activities/new` | Dr Dlamini starts a Mini-CEX from the dashboard and saves a draft | only Request is open; the draft's page with "Draft saved. It has not been submitted." |
| 3.2 | `/activities/mine` → `/activities/{ActivityId:int}` | finds the draft and submits it with a field missing | the draft in My activities; the refusal naming the field, and the field marked; still a draft |
| 3.3 | `/activities/{ActivityId:int}` | fills in the field and submits | Requested; the page now read-only to her, Cancel the only move; the Submit row in the history |
| 3.6 | `/activities/mine` → `/activities/{ActivityId:int}` | opens the completed Mini-CEX | Completed, credited "1 item"; the rung and the assessor's feedback; no move left |
| 3.7 | `/portfolio/progress` | opens My progress (F05's page) | the Mini-CEX counted against this semester's target |
| 3.8 | `/activities/new` | Dr Ndlovu types a future date and submits | "Nothing was saved. …"; the date field marked; everything typed kept |
| 3.9 | `/activities/new` | types a date before his programme started | the hint as he types, then the refusal |
| 3.10 | `/activities/new` → `/activities/{ActivityId:int}` | types the real date, 20 days ago, and submits | "Submitted. It is now Requested."; "Filed 20 days after the encounter" in the history |
| 3.12 | `/` → `/activities/{ActivityId:int}` → `/activities/new` | reads the decline, then files the encounter again, naming another assessor | Declined, with the assessor's note and no move; the late-filing warning on the new form |
| 3.14 | `/activities/new` → `/activities/{ActivityId:int}` | files a Reflective Exercise | only the EPAs its list names; no Entrustment section; Discussion locked; no late warning (it credits nothing) |
| 3.16 | `/` → `/activities/inbox` → `/activities/{ActivityId:int}` | finds the returned reflection in his inbox, expands it, submits again | the return and its note; his fields open again; Awaiting discussion after the submit |
| 3.18 | `/activities/new` → `/activities/{ActivityId:int}` | Dr du Plessis logs a teaching session | one section, no rating, all 15 EPAs; "Submitted. It is now Logged." |
| 3.19 | `/activities/new` | logs two more, one from before his programme | both Logged; the early date accepted with no hint |
| 3.20 | `/activities/new` → `/activities/{ActivityId:int}` → `/activities/mine` | starts the wrong instrument, saves, cancels | the draft saved with required fields empty; Cancel with no note; Cancelled in My activities |
| 3.21 | `/activities/new` | asks for a CBD | Requested |
| 3.22 | `/activities/new` | asks for a portfolio review over a period | one EPA offered; no rating; Review locked; "Awaiting review" |
| 3.23 | `/activities/new` | Dr Mahlangu files a DOPS and looks for PAED-010 | the EPA picker follows the instrument's tool list (PAED-010 not offered) |
| 3.25 | `/activities/new` → `/activities/mine` | Dr Molefe files six assessments | six Requested rows in My activities |
| 3.27 | `/activities/new` | Dr Dlamini files three more | each Requested |
| 3.29 | `/activities/new` | Dr Mahlangu asks Dr Zulu for a Mini-CEX, which is left unrated | Requested (it stalls: F06 and F04 pick it up) |
| 5.24 | `/activities/new` | Dr du Plessis asks for a Mini-CEX on his last shift | Requested; no email is sent |
| 6.16 | `/activities/new` | Dr Dlamini files on PAED-012 | nine EPAs for the Mini-CEX |
| 6.20 | `/activities/new` | Dr Mahlangu opens the EPA picker while PAED-012 is paused | a paused EPA is not offered |
| 6.27 | `/portfolio/progress` → `/activities/new` | Dr Dlamini, after KGK adds a local EPA | the local EPA offered for Direct Observation |
| 6.37 | `/portfolio/progress` → `/activities/new` | Dr Ndlovu, moved to curriculum 11.2 | the new version's EPAs offered |
| A.2.7 | `/activities/new` | Dr Mahlangu saves one draft and submits one request | the two notices |
| A.6.6 | `/activities/new` | Dr Dlamini opens the assessor list while Dr Patel is locked | a locked assessor is not offered; never herself |
| A.7.1 | `/activities/new` → `/activities/{ActivityId:int}` | Dr Dlamini files with the keyboard alone | focus order; labels and help read out; the focus ring on every control; the late warning announced; focus never left on the page body |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Activities.

| Page | State | Screenshot | What it shows |
|---|---|---|---|
| New activity | No type chosen | `states/new-activity--blank.png` | the type select alone |
| New activity | A rated form | `states/new-activity--mini-cex.png` | Request open; Entrustment and Feedback locked |
| New activity | An unrated form | `states/new-activity--reflective.png` | no Entrustment section; Discussion locked |
| New activity | A self-logged form | `states/new-activity--teaching-log.png` | one section, no rating |
| New activity | Refused: a future date | `states/new-activity--refused-future.png` | "Nothing was saved. …", the date field marked |
| New activity | Hint: before the programme | `states/new-activity--before-programme.png` | the hint under the date as it is typed |
| New activity | Refused: before the programme | `states/new-activity--refused-before-programme.png` | |
| New activity | Late-filing warning | `states/new-activity--late-warning.png` | "This encounter was 20 days ago. …" |
| New activity | EPA picker without a paused EPA | `states/new-activity--paused-epa.png` | |
| New activity | Loading | `states/new-activity--loading.png` | the header and an empty type select; no skeleton (T329) |
| New activity | Narrow | `states/new-activity--narrow.png` | |
| Activity page | Draft saved | `states/activity-view--draft-saved.png` | "Draft saved. It has not been submitted." |
| Activity page | Refused submit | `states/activity-view--refused-submit.png` | "Presenting problem: A value is required.", the field marked |
| Activity page | Saved as a draft, not submitted | `states/activity-view--not-submitted.png` | the empty Submit from New activity |
| Activity page | Requested | `states/activity-view--requested.png` | read-only to her; Cancel only |
| Activity page | Submitted late | `states/activity-view--submitted-late.png` | "Submitted. It is now Requested."; "Filed 20 days after the encounter" |
| Activity page | Declined | `states/activity-view--declined.png` | the assessor's note in the history; no move |
| Activity page | Returned | `states/activity-view--returned.png` | back to Draft, his fields open |
| Activity page | Discussed | `states/activity-view--discussed.png` | finished, read-only |
| Activity page | Logged | `states/activity-view--logged.png` | |
| Activity page | Cancelled | `states/activity-view--cancelled.png` | |
| Activity page | Awaiting review | `states/activity-view--awaiting-review.png` | the portfolio review |
| Activity page | Completed | `states/activity-view--completed.png` | the rung and the feedback; credited "1 item" |
| Activity page | Loading | `states/activity-view--loading.png` | |
| Activity page | Load error | `states/activity-view--load-error.png` | |
| Activity page | Narrow | `states/activity-view--narrow.png` | three cards deep, 193 px inputs (T323) |
| My activities | A draft | `states/my-activities--draft.png` | |
| My activities | Logged and cancelled | `states/my-activities--mixed.png` | |
| My activities | Nothing filed | `states/my-activities--empty.png` | "No activities yet" |
| My activities | Loading | `states/my-activities--loading.png` | |
| My activities | Load error | `states/my-activities--load-error.png` | |
| My activities | Narrow | `states/my-activities--narrow.png` | |
| Activity inbox | A registrar's returned work | `states/activity-inbox--trainee.png` | |
| Home | Work returned | `states/home--trainee-returned.png` | the Activity inbox card lists the reflection as Draft, as `/activities/inbox` does (re-captured after T297, 2026-09-26) |

Not captured, so describe them in words:
- **A move shown disabled with its reason** (T107). No seeded or KGK workflow leaves a required field its mover cannot
  write (`states.md` § States no local replay reaches). Question 5.
- **"Filed. It is now …"**, the notice for a type whose creation is its filing. Only the Demo generic types are born
  requested or terminal (the same section).
- **A re-submission after a return, on a type that credits.** No seeded type both credits and returns
  (`coverage.md` § Flows and states not played). In that case, the second Submit would not record lateness again:
  only the first filing does (CLAUDE.md § Activity platform, `EncounterDateGate`).
- **"Awaiting admission".** A pending trainee's New activity (`states/new-activity--pending-trainee.png`) belongs to
  F11.

## 4. Attach

Paths are relative to `design/baseline/`. Every one below was checked with `ls` on 2026-09-26.

**Key screenshots (attach these first, with § 1):**
1. `act-2/2.42-1-molefe-new-activity.png`
2. `states/new-activity--mini-cex.png`
3. `act-3/3.23-1-dops-picker.png`
4. `states/activity-view--refused-submit.png`
5. `states/new-activity--late-warning.png`
6. `states/activity-view--submitted-late.png`
7. `states/activity-view--declined.png`
8. `states/activity-view--returned.png`
9. `states/new-activity--teaching-log.png`
10. `act-A/A.7.1-3-submitted-requested.png`
11. `act-A/A.7.1-4-late-filing-warning-fresh-form.png`
12. `states/new-activity--narrow.png`
13. `states/activity-view--narrow.png`

**The forms as data** (BRIEF.md § 3.1; research `blazor_specific_risks[4]` and
`best_inputs_for_existing_app_redesign[5]`):
- `src/Wombat.Infrastructure/Activities/Seeds/mini_cex_cpsa/schema.json`, `workflow.json`, `credit.json` (rated).
- `src/Wombat.Infrastructure/Activities/Seeds/reflective_exercise_cpsa/schema.json`, `workflow.json`, `credit.json`
  (unrated).
- **The self-logged shape: paste the KGK Teaching Session Log's definition instead of `teaching_session`.** BRIEF.md
  § 3.1 names `teaching_session`, but that seed is a generic starter type ("Teaching log with basic acceptance
  workflow", `Seeds/README.md`) whose workflow is draft → submitted → accepted by a SpecialityAdmin
  (`teaching_session/workflow.json`), so it is reviewed, not self-logged. The story's log is built in
  the builder (Act 1, steps 1.26–1.29) and exists only in the database. Paste this text, taken from those steps:

  ```text
  KGK Teaching Session Log (kgk_teaching_log), scoped to KGK; not a WBA instrument;
    credits nothing ({"counts_for": []}).
  Form: one section, "Teaching session":
    Topic (text, required); EPA (EPA, required); Date delivered (date, required, the encounter date);
    Audience (choice, required: Interns, Medical students, Nursing staff, Registrars);
    Learning objectives (long text, required); Supervising consultant (user, optional).
  Workflow: draft → logged (Log; the whole form is checked) or draft → cancelled (Cancel; formats only); both by the
    registrar or whoever filed it. Logged is final.
  ```

**States (attach as the chat asks):** every row of § 3 not already attached above.

**Re-captured after T297 landed (2026-09-26); attach as the chat asks:** `act-3/3.12-1-ndlovu-home-declined.png` (the
Activity inbox card empty beside the declined request on Recent activities) and `act-3/3.16-1-ndlovu-home-returned.png`
(the card listing the returned reflection as Draft). `states/home--trainee-returned.png` too: it was taken from the
end-of-Act-3 snapshot with the reflection set back to Draft by SQL, and shows the same cards as `3.16-1` (BRIEF § 10).

**Also available** (step captures): every instrument in the picker, `act-2/2.42-2-…` to `act-2/2.42-12-…`; the
decline and return from the registrar's side, `act-3/3.12-2-declined-view.png` and `act-3/3.16-3-returned-view.png`.

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists.

| Task | What it means for the design | Evidence (under `design/baseline/`) |
|---|---|---|
| **T299** | A result region under the header, taking the focus after every move. A note panel that stays open on a refusal, with its text, and a named note field. A pressed button that stays focusable (`aria-disabled`) while it runs. A refusal mark that clears when its value changes, letting the form's own hints (the late warning) come back. | `act-A/A.7.1-3-submitted-requested.png`, `states/activity-view--decline-refused.png`, `act-A/A.7.1-4-late-filing-warning-fresh-form.png` |
| **T323** | One card deep at 390 px; inputs at least about 290 px wide; selects that show the chosen EPA and assessor. The history table must fit or reflow (its Actor column is cut off at 390 px in `act-A/A.7.2-1-patel-completes-on-phone.png`). | `states/activity-view--narrow.png`, `states/new-activity--narrow.png` |
| **T329** | New activity: the header and a skeleton from the first render; an alert, not the in-app error bar, when a read fails. | `states/new-activity--loading.png`, `states/shell--error-banner.png` |
| **T320** | No copy promises an email to the assessor. The inbox is the only notice, so the design should make "who has it now" visible on the page. | `states/data-rights--rejected.png` (the promise, on another page) |
| **T328** | Textareas and selects in the body font. | `act-A/A.7.2-1-patel-completes-on-phone.png` (monospace feedback) |
| **T325** | History times in SAST with the zone. | `states/activity-view--to-rate.png` ("2026-09-26 12:14", no zone) |
| **T324** | Labels, not codes, everywhere on the page. | BRIEF.md § 6 A8 |
| **T190** | The tab title names the activity type. | T190's symptom |
| **T280** | Links to an activity named by instrument plus EPA or date, so two rows never share a name. | T280's symptom |
| **T297** (group 1, landed in 7bf8ea7) | The Home inbox card and `/activities/inbox` list the same things. | § 4: `act-3/3.12-1-…`, `act-3/3.16-1-…` and `states/home--trainee-returned.png`, all re-captured after the fix |
| **T322** | Alert, badge and validation colours from F01's tokens. | `act-A/A.7.14-3-change-password-mismatch-danger-alert.png` |
| Observed, not filed | The summary card lists Type, Encounter date, State and Version (`ActivityView.razor:73–79`), not the registrar or the assessor. "Version" is the pinned form's schema version. The Trainee's nav item that opens New activity is labelled "Activities" (DESIGN.md:196; `NavMenu.razor:101`). | `states/activity-view--to-rate.png`, `states/new-activity--mini-cex.png` |

## 6. Questions the design must answer

1. **Is instrument choice a first step of its own, or one field among the rest?** The picker holds 11 instruments
   (Step 2.42; `act-2/2.42-1-…` to `2.42-12-…`), and the rest of the form depends on the choice.
2. **How does the form show which fields the assessor fills in?** In a Mini-CEX the assessor's Entrustment and
   Feedback sections are read-only to the registrar (Step 3.1). Today they show as greyed inputs.
3. **Where does the history sit, and how much of it shows by default?** Each move has an actor, a time, a credit and a
   note, and a late filing adds "Filed N days after the encounter" (Step 3.10). The decline's reason lives only in the
   history today (Step 3.12).
4. **How are the College's labels shown?** Rungs 1, 2, 3a, 3b, 4 and 5 (D32, Step 3.5) and the instruments (Mini-CEX,
   CbD, DOPS…). Does the rung show its descriptor beside its label?
5. **A move disabled with its reason** (`states.md` § States no local replay reaches): grey it with the reason, or
   hide it and explain?

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the 30 steps** on a fresh database, playing the acts up to each one (`README.md` § How to play): Act 2's
  2.42 and 2.43; Act 3's 3.1–3.29; 5.24; Act 6's 6.16, 6.20, 6.27 and 6.37; and the appendix's A.2.7, A.6.6 and A.7.1.
  Every Expect must hold. The steps quote the notices word for word (3.1, 3.8–3.10, 3.12, 3.14, 3.18, 3.22, A.2.7,
  A.7.1): update them in the same task if the wording changes (BRIEF.md § 9 item 7). After T297 lands, 3.12's Expect is
  T297's rewritten one.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`). They
  must include:
  - every file in `Activities/`, notably `RuntimeRendererTests`, `ActivityFormLabelTests`, `ActivityFormHelpTextTests`,
    `LateFilingWarningTests`, `RefusedFieldMarkingTests`, `NewActivitySubmitFlowTests`, `ActivityWorkflowActionsTests`,
    `EpaPickerScopeTests` and `NomineePickerTests`;
  - `BuilderPreviewParityTests`: the builder's preview uses the same renderer, so a renderer change shows in F17;
  - `Accessibility/ActionFocusTests`, `FormFieldHelpTextLinkTests` and `RowNamesTests`;
  - `Design/InvalidFieldStyleTests`, `FieldGroupTests`, `NarrowLayoutTests`, `RowActionMarkupTests` and
    `DefinedClassTests`;
  - `Scenario/`.
- **Re-capture** the 34 states in § 3 and the steps' own captures. Compare them with
  the chosen artboards.
- **Browser check** at 1280 and 390 px as a registrar: file a Mini-CEX, a Reflective Exercise and a Teaching Session
  Log with the keyboard alone (Step A.7.1's checks), and read each outcome with a screen reader.

## 8. The runbook steps, verbatim (paste this second)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as it is today. `D` is the replay day and `J` the programme anchor (`act-3-operations.md`
§ The clock in this act).

```text
Step 2.42 — The instruments a KGK registrar is offered (act-2-onboarding.md:715)
Role: Trainee — Dr Lerato Molefe
Route: /activities/new
Do: Open the Activity type select and read its options. Choose each in turn to see what it rates, and save nothing.
Expect: Eleven options besides "Select…", no name twice: Case-Based Discussion, Chart-Stimulated Recall, Clinical Audit,
  Clinical Case Analysis, Direct Observation, DOPS, Mini-CEX, Portfolio and Logbook Review, Random Case Analysis and
  Reflective Exercise, each "(Paediatrics)", and `KGK Teaching Session Log`. Seven of them rate on `1 2 3a 3b 4 5`:
  CBD, Chart-Stimulated Recall, CCA, Direct Observation, DOPS, Mini-CEX and RCA. Multi-Source Feedback and Learner
  Feedback (Paediatrics) are not offered, because only the system writes them (T162, T164). Nor are the Demo types,
  which belong to another discipline.

Step 2.43 — The Mini-CEX link and whom it may name (act-2-onboarding.md:730)
Role: Trainee — Dr Lerato Molefe
Route: /activities/new
Do: Open `/activities/new?type=mini_cex_cpsa`. Read the EPA and Assessor pickers, then leave without saving.
Expect: The Mini-CEX (Paediatrics) is already chosen, with the sections Request, Entrustment and Feedback, and Save
  draft and Submit. The EPA picker offers the nine EPAs whose Annexure A list names the Mini-CEX: PAED-001, 002, 003,
  004, 006, 007, 008, 012 and 013 (T122). The Assessor picker offers exactly Botha, Khumalo, Naidoo, Patel and Zulu, each
  with their email. It does not offer van Rensburg, Molefe herself or any Demo account (T102). Nothing is saved.

Step 3.1 — Dr Dlamini starts a Mini-CEX and saves it as a draft (act-3-operations.md:85)
Role: Trainee — Dr Anele Dlamini
Route: / → /activities/new
Do: From the dashboard, log an activity and choose Mini-CEX (Paediatrics). Fill in the EPA PAED-001, the assessor Dr
  David Naidoo, the date observed `D−10`, the setting Emergency unit and the complexity Moderate. Leave the presenting
  problem empty, and save the draft.
Expect: The type select offers eleven types, and no name appears twice: the ten hand-filed `*_cpsa` instruments and KGK
  Teaching Session Log. Multi-Source Feedback and Learner Feedback are not offered (T162). The form has three sections:
  Request, Entrustment and Feedback. Only Request can be filled in; the other two are locked. The EPA picker offers the
  nine EPAs whose list names the Mini-CEX: PAED-001 to 004, 006 to 008, 012 and 013. It does not offer PAED-010 (T122).
  The Assessor picker offers exactly KGK's active Assessors, by name and address: Dr Botha, Dr Khumalo, Dr Naidoo,
  Dr Patel and Dr Zulu. It never offers Dr Dlamini or another registrar, nor Prof Mbatha, Mr Smit, Dr Mokoena,
  Dr Sithole or Dr van Rensburg (T102). No lateness warning shows for `D−10`. Saving opens the activity's page, State:
  Draft, with "Draft saved. It has not been submitted." (T127).

Step 3.2 — Dr Dlamini reopens the draft, and a submit with a field missing is refused (act-3-operations.md:105)
Role: Trainee — Dr Anele Dlamini
Route: /activities/mine → /activities/{ActivityId:int}
Do: Find the draft in My Activities and open it. Press Submit without filling in the presenting problem.
Expect: My Activities lists the draft as Mini-CEX (Paediatrics), PAED-001 with its title, encounter date `D−10`, Draft,
  credited "—". On the draft's page, the Request fields are open. The page offers Submit and Cancel, and Discard changes
  stays disabled until something is typed. The history has one row: Create (Draft → Draft), by Dr Dlamini. The submit
  is refused with "Presenting problem: A value is required.", the field is marked (T263), and the activity stays a
  draft.

Step 3.3 — Dr Dlamini completes the request and submits it (act-3-operations.md:120)
Role: Trainee — Dr Anele Dlamini
Route: /activities/{ActivityId:int}
Do: Type the presenting problem "Two-year-old with bronchiolitis and rising work of breathing" and submit.
Expect: State: Requested. The page is now read-only to her, and Cancel is the only action. The history adds Submit
  (Draft → Requested). It carries no lateness note, because ten days is on time (D15). No email is sent: Wombat mails
  nobody when an activity moves, so Dr Naidoo learns of the request from his inbox.

Step 3.6 — Dr Dlamini sees the completed Mini-CEX in My Activities (act-3-operations.md:163)
Role: Trainee — Dr Anele Dlamini
Route: /activities/mine → /activities/{ActivityId:int}
Do: Open My Activities, then the Mini-CEX.
Expect: The row reads Completed, credited "1 item". The page is read-only. It shows rung 4 and Dr Naidoo's feedback, and
  offers no action. The history holds Create, Submit and Complete, with Dr Naidoo as the actor of the Complete.

Step 3.7 — Dr Dlamini's progress counts the Mini-CEX against this semester's target (act-3-operations.md:174)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress
Do: Open My Progress.
Expect: The "This period" card reads:
  - "Semester 2, 2026 · July to November";
  - semester targets "0 of 10 EPAs met this semester" and yearly targets "0 of 5 EPAs met in 2026";
  - training year 3;
  - multi-source feedback "0 of 15 EPAs covered…".
  The PAED-001 card reads "1 of 3 this semester", with a bar, then "2 more by 30 November 2026. At the minimum level when
  observed: 1 of 1. Last encounter date: `D−10`." Below that come "Target: 3 per semester (6 a year). Minimum now 4."
  and "Semester 1, 2026: 0 of 3, 3 short". The Entrustment section shows no STAR yet. The trajectory charts PAED-001:
  1 observation from 1 distinct assessor.

Step 3.8 — A future encounter date is refused (act-3-operations.md:198)
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new
Do: Start a Mini-CEX (Paediatrics) for an encounter on the ward. Fill in the EPA PAED-002, the assessor Dr Fatima
  Khumalo, the setting Ward, the presenting problem "Nine-month-old with gastroenteritis and moderate dehydration" and
  the complexity Low. By mistake, type the date observed as `D+1`. Submit.
Expect: The submit is refused with "Nothing was saved. Date observed: The date cannot be after today (`D`)." The date
  field is marked and names the alert, and everything typed is kept (T160, T263). No activity exists.

Step 3.9 — A date before the programme started is refused (act-3-operations.md:211)
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new
Do: Change the date observed to `J−1d`, the day before his programme started (2026-01-14 on a replay in 2026), and
  submit again.
Expect: As the date is typed, the field is marked, and below it reads "This date is before the trainee's programme
  started (`J`), and will not be accepted." The submit is refused with "Nothing was saved. Date observed: The date
  cannot be before the trainee's programme started (`J`)." A Mini-CEX can credit, so its date is held to the programme
  start (T160, T192). No activity exists.

Step 3.10 — A late filing is warned about and recorded, never refused (act-3-operations.md:226)
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new → /activities/{ActivityId:int}
Do: Change the date observed to `D−20`, the day the encounter happened, and submit.
Expect: No lateness warning shows yet. The field still carries the last refusal's mark, which stays until the next action
  (T263), and a refused date is never called fileable. Step 3.12's fresh form shows the warning. The submit succeeds:
  "Submitted. It is now Requested." In the history, the Submit row's time carries "Filed 20 days after the encounter"
  (D15, T160).

Step 3.12 — Dr Ndlovu reads the decline and files the encounter again, naming Dr Botha (act-3-operations.md:255)
Role: Trainee — Dr Sipho Ndlovu
Route: / → /activities/{ActivityId:int} → /activities/new → /activities/{ActivityId:int}
Do: Open the declined Mini-CEX from the dashboard's Recent activities. Then file the same encounter as a new Mini-CEX,
  `D−20`, PAED-002, naming Dr Sarah Botha, and submit.
Expect: Recent activities lists the Mini-CEX with a red Declined badge. The Activity inbox card does not list it, as the
  inbox it opens does not: nothing can move a declined request on (T297). No mail tells him of the decline (T320), so
  Recent activities, while it is among his five newest, and My Activities are where he finds it. Its page reads
  State: Declined, offers no action, and shows Dr Khumalo's note in the history. The re-filing is a new activity. As
  `D−20` is typed, the field warns: "This encounter was 20 days ago. It can still be filed, but a filing more than 14
  days after the encounter is recorded as late." Its own Submit row records "Filed 20 days after the encounter". It
  ends Requested, and the Activity inbox card lists it, as the inbox does: he may still cancel it.

Step 3.14 — Dr Ndlovu submits a reflective exercise to Dr Botha (act-3-operations.md:296)
Role: Trainee — Dr Sipho Ndlovu
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Reflective Exercise (Paediatrics) on a critical incident. Fill in the EPA PAED-001, the supervisor
  Dr Botha and the date of the incident `D−20`. For what happened, write "A three-year-old in septic shock waited
  90 minutes for antibiotics after triage". Add the analysis, what he learned and a one-line plan, then submit.
Expect: The EPA picker offers only PAED-001, 003, 008 and 014, the EPAs whose list names the reflective exercise (D45).
  The form has no Entrustment section, because the exercise is unrated (D6), and its Discussion section is locked. No
  lateness warning shows for `D−20`: a reflective exercise credits nothing, so its filing is late for nobody (T160).
  The submit reads "Submitted. It is now Awaiting discussion.", and the Submit row carries no lateness note.

Step 3.16 — Dr Ndlovu finds the returned reflection in his inbox and submits it again (act-3-operations.md:325)
Role: Trainee — Dr Sipho Ndlovu
Route: / → /activities/inbox → /activities/{ActivityId:int}
Do: From the dashboard's Activity inbox card, open the inbox, then the reflection. Expand "What I will do differently":
  "Start the sepsis bundle at triage; read the paediatric sepsis guideline before my next take". Submit again.
Expect: The dashboard card lists the reflection as Draft. The inbox page lists it and nothing else of his. His fields are
  open again, with Submit and Cancel. After the submit, the state is Awaiting discussion. The history reads Create,
  Submit, Return and Submit, and neither Submit row carries a lateness note. Home's Activity inbox card then lists the
  reflection as Awaiting discussion, as the inbox does (he may still cancel it), and not the declined Mini-CEX (T297).

Step 3.18 — Dr du Plessis logs a teaching session (act-3-operations.md:360)
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: Choose KGK Teaching Session Log. Fill in the topic "Recognising the sick child: a triage refresher", the EPA
  PAED-015, the date delivered `D−6`, the audience Interns and the learning objectives. Submit.
Expect: The form is Act 1's: one section, Teaching session, with six fields, the first five required and Supervising
  consultant optional, and no rating. The EPA picker offers all 15 PAED EPAs: the type is not a College instrument, so
  no tool list binds it (D21). The submit reads "Submitted. It is now Logged." The Log row is credited "—", and no
  lateness is recorded.

Step 3.19 — Dr du Plessis logs two more sessions, one from before his programme (act-3-operations.md:376)
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: Log two more sessions:
  - "Neonatal jaundice for fourth-year students": PAED-004, `D−13`, Medical students.
  - "Oral rehydration for nursing staff", taught as a medical officer before admission: PAED-015, `J−1y−56d`
    (20 November 2024 on a replay in 2026), Nursing staff.
Expect: Both sessions are Logged. `J−1y−56d` is before his programme started (`J−1y`), and it is accepted with no
  hint. A type that credits nothing is held only to "not after today" (T160).

Step 3.20 — Dr du Plessis abandons a draft started on the wrong instrument (act-3-operations.md:390)
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int} → /activities/mine
Do: Start a DOPS (Paediatrics) by mistake. Choose only the EPA PAED-002 and save the draft. Then cancel it from its page.
Expect: The draft saves with its other required fields empty, because saving a draft checks formats only (T105). Cancel
  asks for no note and moves the draft to Cancelled, with nothing offered after it. My Activities lists it as Cancelled,
  credited "—", beside the three Logged sessions.

Step 3.21 — Dr du Plessis asks Dr Khumalo for a CBD (act-3-operations.md:403)
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Case-Based Discussion (Paediatrics). Fill in the EPA PAED-002, the assessor Dr Khumalo, the date `D−5`, the
  case discussed (a toddler with a first febrile seizure) and the focus Clinical reasoning. Submit.
Expect: The CBD ends Requested. It stays that way to the end of the act; Step 3.51 reads it from Dr Khumalo's side.

Step 3.22 — Dr du Plessis asks Dr Patel to review his portfolio (act-3-operations.md:413)
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Portfolio and Logbook Review (Paediatrics). Fill in the EPA PAED-015, the reviewer Dr Patel, the review
  period `D−60` to `D−1`, and the note for the reviewer "Three teaching sessions logged; please review them with my
  logbook". Leave the export's file name empty, since it is optional. Submit.
Expect: The EPA picker offers PAED-015 only, the one EPA whose list names the portfolio review (D45). The form has no
  rating, and its Review section is locked. The submit reads "Submitted. It is now Awaiting review." The encounter date
  is the period's last day, `D−1`. The review stays Awaiting review to the end of the act.

Step 3.23 — Dr Mahlangu files a DOPS, and the EPA picker follows the tool lists (act-3-operations.md:427)
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/{ActivityId:int}
Do: File a DOPS (Paediatrics) for a lumbar puncture on a four-month-old with suspected meningitis. First look for
  PAED-010 in the EPA picker. Then choose PAED-002, the assessor Dr Patel, the date `D−8`, the procedure "Lumbar
  puncture", the setting Ward and the complexity Moderate. Submit.
Expect: The EPA picker offers the eight EPAs whose list names the DOPS, PAED-001 to PAED-008. It does not offer
  PAED-010, whose list is Direct observation and MSF only (T122); the old runbook filed this DOPS there. The DOPS ends
  Requested.

Step 3.25 — Dr Molefe files six workplace-based assessments (act-3-operations.md:463)
Role: Trainee — Dr Lerato Molefe
Route: /activities/new → /activities/{ActivityId:int} → /activities/mine
Do: File and submit each of these, filling in every Request field. The clinical context below is the presenting
  problem, the procedure, the case discussed or the context:
  - Mini-CEX, PAED-001, Dr Zulu, `D−12`, Emergency unit, complexity High: status epilepticus in a five-year-old.
  - CBD, PAED-001, Dr Naidoo, `D−11`, focus Escalation and safety: a febrile neutropenic child.
  - DOPS, PAED-001, Dr Patel, `D−9`, Emergency unit, complexity High: intraosseous access in a shocked infant.
  - Mini-CEX, PAED-012, Dr Botha, `D−7`, Ward, complexity Moderate: telling parents of a new diagnosis of type 1
    diabetes.
  - CBD, PAED-012, Dr Khumalo, `D−5`, focus Ethics and consent: a transfusion refused on religious grounds.
  - Direct Observation, PAED-010, Dr Zulu, `D−4`, activity observed Ward round: leading the Monday ward round.
Expect: Each ends Requested, with no lateness warning, since all six are within 14 days. My Activities lists the six as
  Requested, credited "—". The Direct Observation's EPA picker offers PAED-010, because its list names Direct
  observation. The Mini-CEX and DOPS pickers do not offer it.

Step 3.27 — Dr Dlamini files three more (act-3-operations.md:501)
Role: Trainee — Dr Anele Dlamini
Route: /activities/new → /activities/{ActivityId:int}
Do: File and submit:
  - CBD, PAED-001, Dr Zulu, `D−9`, focus Management plan: diabetic ketoacidosis in a nine-year-old.
  - Clinical Case Analysis, PAED-001, Dr Botha, `D−6`, case "Anaphylaxis, 7-year-old, emergency unit", documents
    reviewed Admission notes and Progress notes, setting Emergency unit, and the reasoning discussed: why adrenaline
    was repeated before the second-line drugs.
  - Mini-CEX, PAED-004, Dr Khumalo, `D−2`, Neonatal unit, complexity Moderate: a term neonate with respiratory
    distress.
Expect: Each ends Requested.

Step 3.29 — Dr Mahlangu asks Dr Zulu for a Mini-CEX, which is left unrated (act-3-operations.md:532)
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Mini-CEX (Paediatrics): PAED-004, Dr Zulu, `D−3`, Neonatal unit, "Term neonate with jaundice on day three",
  complexity Low. Submit. Dr Zulu does not act on it.
Expect: The Mini-CEX ends Requested.

Step 5.24 — Dr du Plessis asks for a Mini-CEX on a shift after his post ended (act-5-graduation.md:575)
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Mini-CEX (Paediatrics) on PAED-002 with:
  - encounter date `D−1`;
  - Dr Naidoo as the assessor;
  - a clinical setting, a presenting problem and a case complexity.
  Submit it.
Expect: The EPA list offers PAED-002, because its tool list includes the Mini-CEX. There is no late-filing warning. The
  activity reads Requested and appears in Dr Naidoo's inbox. No email is sent: Wombat mails nobody when an activity
  moves (Step 3.3).

Step 6.16 — Dr Dlamini files a Mini-CEX on PAED-012 (act-6-catalogue.md:407)
Role: Trainee — Dr Anele Dlamini
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Mini-CEX (Paediatrics) and submit it:
  - EPA: PAED-012;
  - assessor: Dr Patel;
  - encounter date: `D`;
  - setting: Outpatient clinic;
  - presenting problem: "Counselling the parents of a toddler newly diagnosed with type 1 diabetes";
  - complexity: Moderate.
  Note the activity's id from its address.
Expect: The EPA picker offers nine EPAs for the Mini-CEX, PAED-012 among them. Once submitted, the activity reads
  Requested and is in Dr Patel's inbox.

Step 6.20 — Dr Mahlangu's picker leaves PAED-012 out too (act-6-catalogue.md:498)
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new
Do: Start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without saving.
Expect: Eight EPAs: PAED-001 to PAED-004, PAED-006 to PAED-008, and PAED-013. PAED-012 is not offered.

Step 6.27 — Dr Dlamini is measured against KGK-001 (act-6-catalogue.md:660)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /activities/new
Do: Read her progress. Then start a Direct Observation (Paediatrics) and open its EPA picker. Leave without saving.
Expect: A KGK-001 card appears under "Once a year", reading 0 of 1 for the current year, and the yearly targets line
  counts one more EPA. Its line for the year before reads "0 of 1, 1 short": a target is read live, into periods that
  have already closed, as the items page warned. The Direct Observation picker offers ten EPAs, KGK-001 among them.

Step 6.37 — Dr Ndlovu on 11.2, before progress is rebuilt (act-6-catalogue.md:853)
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress → /activities/new
Do: Read the page. Then start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without saving.
Expect:
  - **The cards.** Seventeen: 11.2's 16 national items (PAED-016 once a year, and PAED-011 at 2 per academic year,
    among them) and KGK-001.
  - **The counts.** His encounters before the move are counted in 11.1's tallies, which this page no longer reads. Until
    Step 6.38, each card shows only what 11.2 holds for him, which is nothing yet.
  - **The picker.** The Mini-CEX picker offers ten EPAs, PAED-016 among them.

Step A.2.7 — Dr Mahlangu leaves a draft and a request waiting (appendix-cross-cutting.md:385)
Role: Trainee — Dr Nomsa Mahlangu
Route: /activities/new → /activities/{ActivityId:int} → /activities/new → /activities/{ActivityId:int}
Do: File two Mini-CEX (Paediatrics) activities, each with every request field filled, an EPA the list offers, and Dr
  Khumalo as assessor.
  - The first: the encounter on `D−3`, then Save draft.
  - The second: the encounter on `D−2`, then Submit.
Expect: The first reads "Draft saved. It has not been submitted." The second reads "Submitted. It is now Requested.",
  and is in Dr Khumalo's inbox. No email is sent: Wombat mails nobody when an activity moves (Step 3.3).

Step A.6.6 — Dr Patel cannot be named as an assessor while locked (appendix-cross-cutting.md:960)
Role: Trainee — Dr Anele Dlamini
Route: /activities/new
Do: Start a Mini-CEX (Paediatrics) and open its assessor list. Then leave without saving.
Expect: KGK's other assessors are listed (Dr Zulu, Dr Naidoo, Dr Botha and Dr Khumalo), but not Dr Patel, and never
  herself (T102). Nothing is saved.

Step A.7.1 — Dr Dlamini files a Mini-CEX with the keyboard alone (appendix-cross-cutting.md:1039)
Role: Trainee — Dr Anele Dlamini
Route: /activities/new → /activities/{ActivityId:int}
Do: At desktop width, using only Tab, Shift+Tab, the arrow keys, Space and Enter, file a Mini-CEX (Paediatrics):
  - choose the type and press Submit with the form empty;
  - on the draft that opens, choose an EPA the list offers, Dr Patel as assessor, the encounter on `D−20`, the setting
    Ward, a presenting problem and the complexity Moderate;
  - then submit it.
Expect: Focus follows the page's order: activity type, the request fields, Save draft, Submit. Each control is
  labelled, and says it is required. Its help text is read with it (T193).
  - The focus ring is visible on every control, and never on the page heading (T048).
  - Only EPAs whose tool list names the Mini-CEX are offered (T122).
  - The empty Submit keeps the activity as a draft and opens it: "Saved as a draft, but not submitted: … Fix the fields
    below and submit again." The fields the refusal names are marked (T127, T263), and the focus is never left on the
    page body (T234).
  - Typing `D−20` announces the late-filing warning ("This encounter was 20 days ago. It can still be filed, …") from
    its live region (D15, T160).
  - Submitted, it reads Requested, and its history records the filing as 20 days after the encounter.
```
