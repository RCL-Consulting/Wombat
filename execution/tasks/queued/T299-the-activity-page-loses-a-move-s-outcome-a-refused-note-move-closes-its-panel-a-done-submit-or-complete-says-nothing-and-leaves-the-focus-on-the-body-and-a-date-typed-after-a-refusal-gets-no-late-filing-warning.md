---
id: T299
title: The activity page loses a move's outcome: a refused note move closes its panel, a done Submit or Complete says nothing and leaves the focus on the body, and a date typed after a refusal gets no late-filing warning
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T299 — The activity page loses a move's outcome: a refused note move closes its panel, a done Submit or Complete says nothing and leaves the focus on the body, and a date typed after a refusal gets no late-filing warning

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Submit and Complete are the product's most used moves, and after each one a keyboard or screen-reader user gets no result and loses the focus. An assessor who types a decline note and is refused for another reason loses the note. The late-filing miss is Low: the lateness is still recorded; only the warning is lost.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-3.11a, F-A.7.1a, F-A.7.1b).

## Symptom

- Step 3.11: Dr Khumalo pressed Decline, left the note empty and pressed Apply. The alert read 'Decline requires a note.' and the state stayed Requested, but the note panel closed. The note field the alert names was gone, and Decline had to be pressed again (design/baseline/states/activity-view--decline-refused.png). A note typed for a move refused for any other reason is thrown away the same way.
- Step A.7.1, keyboard only: the empty Submit on /activities/new opened draft 28 under its refusal notice, with document.activeElement on BODY. On the draft, Submit moved it to Requested and the Submit button went with the old state. No result message was shown, and the focus was on BODY again (design/baseline/act-A/A.7.1-3-submitted-requested.png). Dr Patel's Complete at A.7.2 ends the same way.
- Step A.7.1: draft 28's refusal had named the encounter date as missing. Typing 2026-09-06 (D−20) there showed no late-filing warning, and the date was filed 20 days late unwarned; the history row reads 'Filed 20 days after the encounter'. On a fresh /activities/new form the same date warns 'This encounter was 20 days ago. It can still be filed, …' (act-A/A.7.1-4-late-filing-warning-fresh-form.png).

## Root cause

- ActivityWorkflowActions.razor:101-110: ConfirmSelectedTransitionAsync awaits OnTransitionRequested, then calls ClearSelection() whatever happened. The EventCallback carries no outcome, so the component cannot tell a refusal from a done move. OnParametersSet (:72-82) already closes the panel once the move is no longer offered, which covers the done case.
- ActivityWorkflowActions.razor:10 and :43 render the pressed button and Apply with disabled="@Busy" while the move runs, so the browser drops their focus to the body. T234 fixed this on 25 other pages (DESIGN.md § Button system). ActivityView.HandleTransitionAsync already refuses re-entry through _busy (:355-424).
- ActivityView.razor:355-424: a done move only reloads (LoadAsync, :410). No result is shown and the focus does not move, although the pressed button has gone with the old state. DESIGN.md § Accessibility says an action whose button is gone moves the focus to its result.
- ActivityView.razor:14-18 renders the page's only h1 (PageHeader) inside @if (_activity is not null). Routes.razor's <FocusOnNavigate Selector="h1"> therefore finds nothing when the page is reached by in-circuit navigation (from /activities/new, the inbox or a dashboard), and the focus stays on BODY. No other page puts its PageHeader in such a branch.
- ActivityForm.razor:161 hides the late-filing notice while the field's key is in RefusedFieldKeys. ActivityView and NewActivity hold those keys until the page's next action (ActivityView.razor:278 sets them from the notice, :384 clears them; NewActivity.razor:230, 256). T263 made the suppression deliberate, so that a refused date is never also called fileable. But the key stays after the author replaces the refused value (here an empty one), so a value the server never saw gets no warning. T263's As built records the mark clearing when the field was filled; the code clears it only at the next action.

## What to build

- A refused move keeps its note panel open, with the note as typed. A refusal about the note marks the textarea (aria-invalid, and aria-describedby pointing at the refusal alert), as T263 marks a refused field. Let the page return the outcome to ActivityWorkflowActions, for example through an OnTransitionRequested that reports whether the move was done, and clear the selection only after a done move. A check before sending (Apply with a required note empty says so at the field, with no round trip) is welcome, but the server stays the authority.
- The pressed button and Apply are not disabled by their own move. While it runs they carry aria-disabled (InFlight.AriaDisabled), the other actions are disabled, and _busy already refuses re-entry (DESIGN.md § Button system).
- A done move reports its result in an ActionResult region at the top of the page ('Submitted. It is now Requested.', 'Completed.', in the pinned workflow's labels, T220). The focus moves to it (FocusAfterRender), since the pressed button has gone.
- The page's h1 is present from the first render: PageHeader sits outside the loaded branch, with a neutral title ('Activity') until the activity arrives. FocusOnNavigate then has a heading however the page is reached.
- A refusal's mark, and the lateness suppression with it, hold only while the field keeps the value the server refused. Keep the refused values beside the keys when the refusal is shown, on both NewActivity and ActivityView. Once the author changes a field's value, drop that field's mark and let the form's own hints speak for the new value: the late-filing warning (D15, T160) and the pre-programme-start hint (T192). A refused value left in place is still never called fileable (T263). Update ActivityForm's RefusedFieldKeys remarks and DESIGN.md § Alerts to say so.

## Verification

- [ ] bUnit (ActivityWorkflowActions): a refused Decline leaves the panel open with the typed note, and a done Decline closes it.
- [ ] bUnit (ActivityView): a Decline refused for its note marks the textarea and names the alert.
- [ ] bUnit (ActivityView): while a move runs, the pressed button carries aria-disabled and is not disabled. After a done Submit, an ActionResult alert names the new state and takes the focus (VerifyFocusAsyncInvoke, as ActionFocusTests do).
- [ ] bUnit: ActivityView renders an h1 before its activity has loaded.
- [ ] bUnit (ActivityForm): with observed_on refused while empty, typing a date 20 days before the filing day shows the late-filing warning and drops the mark. With the refused value unchanged, neither changes, so T263's case still holds.
- [ ] Browser, keyboard only: Step 3.11 (an empty Apply keeps the panel open; the note is typed there and Apply declines). Step A.7.1 (the focus is never on BODY, after the empty Submit or after Submit, and typing D−20 on draft 28 announces the warning). Step A.7.2 (Complete's result takes the focus).

## Related

T234 (the focus rule), T263 (refused fields marked; the lateness suppression), T127 (the notice handed over from /activities/new), T193 (alerts), T107, T160 and D15 (the late-filing warning), T192, T220. Runbook steps 3.11, A.7.1 and A.7.2.

## Notes

- **T295 replay, 2026-09-26 (sweep).** T295 states sweep, 2026-09-26 (observed). The missing h1 also shows in the page's loading and load-error states. The skeleton and the danger alert sit flush under the top bar with no header, subtitle or way back (design/baseline/states/activity-view--loading.png, activity-view--load-error.png), and the 'Activity unavailable' empty state (ActivityView.razor:68) has none either. The cause is the one this task gives (ActivityView.razor:14-18). The planned fix, PageHeader outside the loaded branch with a neutral title until the activity arrives, covers all three states. Add the loading and load-error screenshots to the browser check. This does not widen the task.
