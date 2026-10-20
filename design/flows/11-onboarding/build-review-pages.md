# T373 build review: the pages, the design system and accessibility

Reviewer: read-only, by reading markup, C# and CSS (no app run, no test run; measurements are from CSS and markup, not from
a browser). Diff `69a7df20..4a58fbe7` on `t373`. Checked against the accepted boards (`round-3/boards-text.md`, the Spec's
two boards), `round-3-check.md` "Taken into the build" (rules 1 to 7) and `build-lanes.md`.

Result: **0 must-fix, 4 should-fix, 9 nits.** Most of what the brief lists holds; the list of what I checked and found
right comes after the findings, so the integrator need not re-check it.

## Findings

### P1 (should-fix): the OneTimeLink's live region is `display: none` while empty, so a copy result may never be announced
- `src/Wombat.Web/wwwroot/app.css:6357` (`.otl-said:empty { display: none; }`), element at
  `OneTimeLink.razor:26` (`<p id="otl-said" class="otl-said field-help" aria-live="polite">`).
- Boards N06, N06b (S10): "Link copied." / "Could not copy. ..." are said "in the same live region". The boards draw the line in
  place; none hides it.
- A live region has to be in the accessibility tree before its text changes. An element removed with `display: none` is not,
  and text arriving in it as it becomes visible is announced unreliably (often not at all) by screen readers. The button's
  own label changes to "Copied", but that is not announced either, since the button is already focused. So the one thing
  S10 asks the region to do can fail for exactly the people it is for. (Inferred from the CSS and how live regions work;
  not tested with a screen reader.)
- Fix: do not hide the empty region with `display: none`. Either drop the `:empty` rule and let the empty `<p>` take its
  `gap` (the `.otl` column has `gap: var(--space-sm)`, so use `margin-block: calc(var(--space-sm) * -1)` or give the empty
  one `height: 0`), or hide it the visually-hidden way (the `.visually-hidden` clip pattern) while empty and restore it when it has
  text. Keep `aria-live="polite"` and the id.

### P2 (should-fix): after a successful Try again on Invitations, Invite a person and Admit, the focus falls to the page body
- `InvitationsList.razor:56`, `InviteAPerson.razor:47`, `AdmitRegistrar.razor:45`: `<StatePanel ... OnRetry=...>` with no
  `FocusAfterRetry`. Contrast `PendingTraineesList.razor:31`, which passes one (the Waiting h2).
- Boards L16, N23, M11 draw only the failed case ("focus on it after a failed retry"), and `StatePanel` does that. But on a
  retry that loads, the Try again button and its alert are gone and nothing takes the focus (DESIGN.md `:4373`: "a retry that
  loads moves the focus to the ... heading"). BRIEF § 6 A5 (focus after every action).
- Fix: pass `FocusAfterRetry`, as Trainees does: a `tabindex="-1"` `@ref` on the first section's h2 for Invitations (the Active
  invitations heading), the form's first field or the Invite-form `h1`'s region for Invite a person (an `ElementReference` on the
  Email input, `FocusAsync`), and the "Who is being admitted" h2 for Admit.

### P3 (should-fix): Admit's `NotPending` refusal swaps the page for the not-found panel and puts the focus nowhere
- `AdmitRegistrar.razor:438-442` (`case TraineeAdmissionRefusal.NotPending: _candidate = null; _notFound = true; return;`).
  The other refusals either focus a field (`_focusField`, `:446`) or the result (`_result?.FocusAfterRender()`, `:467`).
- Board M08 (the panel) and Spec M3/S9. The button the person pressed (Admit ‹name›) is gone with the form, the h1 and the tab
  title change to "Registrar not found", and the focus drops to the body. A stale tab's second Admit by another admin is the way
  in, but it is a refusal the page words, and "focus after every action" is the brief's rule.
- Fix: after setting `_notFound`, focus the not-found panel's heading or its "Go to Trainees" link (give the panel's `<p>` or the
  link an id and call `PageFocus.FocusByIdAsync` from `OnAfterRenderAsync` via `_focusField`, which already exists).

### P4 (should-fix): Trainees keeps `?admitted=` result on screen after a same-page navigation
- `PendingTraineesList.razor:52-55,270-308`: `Admitted` and `Back` are read once, in `OnInitializedAsync`/`LoadAsync`. The
  router re-uses the component for `/admin/trainees` after `/admin/trainees?admitted=7` (menu "Trainees", the breadcrumb, the
  browser back), sets `Admitted` to null and does not re-run `OnInitializedAsync`, so the success Alert for the admission
  stays (and its `FocusOnLoad` has fired already).
- Board T02 to T04 (S8: "what Admit did", once). Not a data fault, but a stale "Lerato Molefe admitted ..." stays above a table that
  has since changed (another admission, a Cancel back from Admit, a refresh by Retry).
- Fix: implement `OnParametersSetAsync` to reload or at least clear `_result`/`_focusBack` when `Admitted`/`Back` change
  (compare with the last seen values), or `Navigation.NavigateTo(OnboardingLinks.Trainees, replace: true)` after reading the
  id so that the address no longer carries it. (Inferred from Blazor's routing behaviour; not run.)

### P5 (nit): Admit writes a class that does not exist, `is-invalid`, on its inputs
- `AdmitRegistrar.razor:270` (`$"{baseClass} is-invalid"`). The system's invalid-field classes are `.input-validation-error` and
  `[aria-invalid="true"]` (`app.css:1355-1361`); `is-invalid` is defined only as `.choice-group.is-invalid` (`app.css:4687`).
  The red edge still shows, because the same inputs carry `aria-invalid="true"`, so this is a dead class, and
  `DefinedClassTests` does not see it (it is built in C#). Invite a person uses `.input-validation-error` (`InviteAPerson.razor:605`).
- Fix: use `input-validation-error`, as Invite a person does.

### P6 (nit): the boards draw `aria-label="Result"` on every ActionResult region; the build has none
- 12 boards (L07, L10, N03, N06, N06b, N09, N21, ...). `ActionResult.razor` renders a `tabindex="-1"` `div` with no name and no
  role, which is the component's existing contract everywhere else, so the build is consistent with the system. A focused
  unnamed `div` is read by its content. Either add `role="region"`/`aria-label` in `ActionResult` for all pages (a system change,
  not this flow's) or note the board as canvas shorthand. No change needed here.

### P7 (nit): "Cancel" or "Back to invitations" on Invite a person
- `InviteAPerson.razor:116`. Spec 2's "Words changed" lists `"Cancel"` becoming `"Back to invitations" (Invite a person)`, but
  every Invite board (N01 to N26) draws `Cancel`. The build follows the boards. Say which wins; as built, Cancel is fine and
  links to Invitations.

### P8 (nit): standing info alerts carry `role="status"` at page load
- `Register.razor:43` (the "This invitation is for ..." notice) and `InviteAPerson.razor:67` (the prefill notice). `Alert`'s
  doc says standing page content takes an empty `Role` (a status present on arrival is not reliably read). An info alert has no
  role by default, so `Role="status"` adds a role that does nothing useful on load. Harmless; drop it or leave it.

### P9 (nit): the T290 note's alert has a role on one page and none on the others
- `InvitationsList.razor:27` and `InviteAPerson.razor:26` use `Role="note"`; `PendingTraineesList.razor:25` and
  `AdmitRegistrar.razor:28` do not. Pick one (an info alert with no role is what the system does elsewhere).

### P10 (nit): Resend still carries `aria-disabled` and native `disabled` on its sibling rows
- `InvitationsList.razor:158-165`. Spec 2 (Form system): "Resend's aria-disabled and the old link Alert text are gone with the
  OneTimeLink." `build-lanes.md` keeps T234's rule (the pressed button stays enabled with `aria-disabled`) and the
  native `disabled` on the other rows is the existing pattern while a resend runs. If the Spec sentence meant removing it, it
  is not removed; if it meant the old Alert text, it is. Worth a one-line decision; I left it as a nit.

### P11 (nit): `ActiveCaptionAt` can print "At ." for an Institutional admin
- `InvitationsList.razor:250-252`: `_invitations[0].InstitutionName ?? string.Empty` gives "At . Not delivered first, ..." if
  the first row carries no institution name (a College-scoped row cannot reach an Institutional admin, so this needs a data fault).
  Guard by falling back to `ActiveCaptionEverywhere`'s shape without the place, or skip the "At ..." clause when empty.

### P12 (nit): dates are formatted with the current culture on four Trainees/Admit lines
- `PendingTraineesList.razor:125,185,191`, `AdmitRegistrar.razor:138` use `.ToString("yyyy-MM-dd")` (and `AdmissionText.cs:63-66,130,133,136`
  uses `{x:yyyy-MM-dd}`) with no `CultureInfo.InvariantCulture`; `InvitationText.Iso` does it right. The host has no culture
  set, so it is the process culture; on a non-Gregorian one these print another calendar's year. Reuse `InvitationText.Iso`
  (or an `AdmissionText.Iso`) for all of them.

### P13 (nit): M8 does not reach the form-mode ConfirmDialog's buttons
- `app.css:6240-6246` (`.dialog-card .form-actions ...`). ConfirmDialog's form mode (My account's Remove of a sign-in) uses
  `.dialog-actions`, not `.form-actions` (`ConfirmDialog.razor` form branch), which has its own column-reverse rule
  (`app.css:1130`) but no 44px/width rule outside `.my-account`. Spec 2 asks for the rule "in one place"; the action mode, the only one Flow
  11 uses, is covered. Add `.dialog-card .dialog-actions .btn` to the same rule if "general" is meant to be literal.

## Checked and right (no action)

- **Words.** The list, Invite a person, Register, Trainees, Admit and the wait card use the boards' sentences, including rule 1
  (the Invitee row header has no `.col-wrap`; Delivery has it), rule 2 (`Admit a registrar` in the h1, tab and trail for the T290
  note, loading and a failed read; the name only after the read), rule 3 (M04's two sentences), rule 4 (the lead names no discipline
  without a speciality), rule 5 and 6 (Waiting keeps Institution for the Administrator; Current trainees puts the institution under
  the name), rule 7 (Invited by with the issue day, M15 once). The ISO dates and "yyyy-MM-dd HH:mm SAST" come from
  `ActivityMoments.When`; Issued splits date over time.
- **States.** Every board state has code: the list (empty, loading, load error with Try again, the T290 note, Not delivered /
  Check the address note rows, resend, revoke with dialog and result, Expired with Invite again); Invite a person (all N states,
  prefill from a revoked and an expired invitation, field refusals, N26, the next-person form); Register (the form, G04 to G06
  refusals, all eleven cards, G18 with the sign-out form); Trainees (waiting, current, the result, cancel focus back, admin
  view); Admit (M01 to M15); the wait card (names its parts only when the data has them).
- **Required marks.** Invite a person and Admit: `span.required-mark` `aria-hidden`, `aria-required="true"`, the legend's
  hidden "required", "* marks a field you must fill in." above the first field, no "(optional)" anywhere (grep), and help text that
  says what empty does. Register has no marks (Q6), native `required`, no `novalidate`, no `maxlength`.
- **Headings and titles.** One h1 per page (PageHeader; Register's own); h2 sections, h3 only inside the wait card; the dialog's
  h2 inside its own dialog. `<PageTitle>` on all five pages, Register's per card, Admit's per name.
- **Focus.** Issue and Resend focus the ActionResult; Revoke focuses it after the dialog closes; a field refusal focuses the field
  (Email, Role's first radio for N25, a select); Admit's Cancel returns to the registrar's Admit… (`?back=`), and the admission
  result takes the focus as Trainees loads; ConfirmDialog returns to its opener on Cancel (its own script).
- **ids and descriptions.** Every `aria-describedby` target is rendered under the same condition as its reference (`MessageIdIf`,
  `DescribedBy`, `{field}-error`, `otl-help`/`otl-said`, the note row's `invitation-note-{id}`). Radios are named by their label
  alone, the description in a sibling `p` (S17). `dl.form-group`/`dd.readonly-value` for the one institution and the one
  curriculum (S18). The Training year is an `<output for>` under a real `<label for>`.
- **Components.** Results are `ActionResult` (no `.result-block`); load errors are `StatePanel`'s Alert with Try again; skeletons
  are raw `div.skeleton.skel-*`, not `Skeleton` (no inline `style=` on any of the five pages; the only one touched files carry,
  `TraineeDashboard.razor:126`'s progress bar, and `DashboardCard`'s link style, predate this flow).
- **CSS.** No raw hex or `rgb()` outside `:root` in the new sections; every token used exists (`--warning-bg`,
  `--warning-color`, `--success-color`, `--input-border`, `--muted-text`, `--font-mono`, `--space-*`, `--radius-sm`); every class
  the pages use is defined (checked by grep: `.history-note--warning`, `.delivery*`, `.otl*`, `.inv-*`, `.wait-*`, `.ty-output`,
  `.col-actions--single`, `.col-wrap--curriculum`, `.skel-*`, `.loading-line`, plus the reused system classes). No `<i class="bi">`,
  no `→` (the `arrow-right` and `hourglass` icons exist in `wwwroot/icons`), no inline script or `onclick`, nothing from a third
  party.
- **Tables.** Both lists use `StackWide` with explicit roles; the note row is a second `<tr>` handled by the existing
  `tr.has-note` / `td.history-note` stack-wide rules; no `width: 1%`; the only `nowrap` is `.inv-when`, which the check (carried
  2) asks for; the floors are `.col-wrap` `min-width`s as `build-lanes` allows.
- **General phone rules.** M8: the dialog is in the top layer outside `.inv-page`, and `.dialog-card` is its own class, so
  `.dialog-card .form-actions` reaches it; Cancel is first in the DOM and last on screen (column-reverse), 44px, full width. M9:
  `.header-container .actions-cell` full width, buttons `flex: 1 1 auto` and 44px; `.inv-page .btn` 44px. The removed
  `.committee-calendar` and `.msf-page` copies are covered (`msf` and the calendar use ConfirmDialog's action mode and
  `PageHeader`).
- **Tests not run** (told not to run the suites); nothing here relies on them.
