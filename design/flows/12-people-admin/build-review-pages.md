# T376 build review: the pages, the design system and accessibility

Reviewer: read-only, by reading markup, C# and CSS (no app run, no test run, no browser; every measurement is from CSS and
markup). Diff `a95e1daa..c4592070` on `t376`. Checked against `round-3/boards-text.md`, `round-2-review.md` (M6-M10, F9-F11),
`round-3-check.md` ("Left" 1-4) and `build-lanes.md` / `build-contracts.md`. Read: `UsersList`, `UserDetail`,
`AssessorTrainingSection`, `AssessorsList`, `AssessorProfileEdit`, `InstitutionEdit`, `InstitutionCreate`, `GroupMappings`,
`InstitutionalAdminDashboard`, their `*Words`/`*Text.cs`, the 402 new `app.css` lines and the DESIGN.md diff.

Result: **1 must-fix, 8 should-fix, 7 nits.** What I checked and found right comes after the findings.

## Findings

### P1 (must-fix): DESIGN.md's banner does not name flow 12
- `execution/architecture/DESIGN.md:25-58` ("Redesigned so far") ends at flow 11 (`... section order (three Onboarding
  sections))`). Flow 12 is named in the body (`:737`, `:1096`, `:1491`, `:3484`, `:3534`, `:3929`, `:4206`, `:4858`) but not in
  the list the banner says every redesigned flow adds itself to ("It then adds itself to the list below, with its date and
  commit").
- Fix: append "flow 12, people admin (2026-10-23, T376: § Button system (a destructive action is an outline trigger), § Form
  system (a form in a note panel, the assessor training form), § Table system (Users' table), § Page-level patterns (Users,
  Assessors, the user page, her institution's record, SSO mappings, her Home), the owner table's rows, and § app.css section
  order (five People admin sections))" plus the commit once there is one.

### P2 (should-fix): Users' Institution and Scope cells are `.col-wrap` with no header floor, so words can split at 901-1100px
- `UsersList.razor:158` (`td ... class="col-wrap"`) and `:176`; the matching `<th>` at `:144` and `:148` carry no `.col-wrap`.
  `.clinic-table .col-wrap` is `overflow-wrap: anywhere` (`app.css:363`), whose floor lives on `th.col-wrap` (`min-width: 4rem`,
  `:367`). Flow 11's check found exactly this: `anywhere` lets the column fall to one letter when the header gives no floor, and
  "Paediatrics / Paediatrics" or "Kgosi Kgari Teaching Hospital" splits between 901 and 1100px, where the table is stacked no
  longer and the other columns are fixed by their words. (Inferred from the CSS; not measured.)
- DESIGN.md `:1096` ("the column never shrinks to split a word between 901 and 1100px") is true for the Name column only.
- Fix: put `col-wrap` on the two `<th scope="col">` as flow 11's Delivery does (`app.css:6309` has the narrow-desktop
  precedent), or drop `col-wrap` from the two `td`s (Assessors does not use it: `AssessorsList.razor:110,112`). Then replay at
  901, 1000, 1100 and 390.

### P3 (should-fix): the Users row-header rule reaches SSO mappings, whose group ids may be longer than a column
- `app.css:6709` (`.people-page th[scope="row"] .person-cell span { overflow-wrap: break-word }`) is scoped to `.people-page`,
  which `GroupMappings.razor:25` also uses; its row header is the group name over `mapping.ExternalGroupId` (`:327`). An
  identity-provider group id (an Entra object id is 36 characters, no break opportunity) now cannot shrink below its length, so
  the 4-5 column table is wider than its card between 901 and 1100px and scrolls sideways, instead of breaking the id inside
  itself as `.person-cell span`'s `anywhere` did.
- Fix: scope the rule to Users (a `people-users` modifier on the table or `.people-filter + .list-section`), or give the
  mappings row header `overflow-wrap: anywhere` back for the id. Decide which break the boards want for an id (O03 draws ids short).

### P4 (should-fix): the sign-in removal dialog says a password still works when the person may have none
- `UserPageText.cs:243-244`: `SignInRemoveBody` is "‹name› can no longer sign in through ‹provider›. Signing in with the email
  and a password still works." for every person. `UserDetail.razor` offers the live Remove… whenever it is not the last way in
  (`lastWayIn = !HasLocalPassword && count == 1`, `:~960`), so a person with no password and two institutional sign-ins reaches the
  dialog, and the sentence is false. Its result (`SignInRemovedRest`, `:253-257`) already branches on `hasPassword`.
- Fix: branch the body the same way ("‹name› still signs in through their other institutional sign-in.") and add a test case.

### P5 (should-fix): about a dozen sentences are neither on a board nor marked NEW
- Not in `boards-text.md`, `build-lanes.md` or `build-contracts.md` (grep, 0 hits each): `UserPageText.cs:34` "That did not go
  through. ...", `:53-58` "Not changed here.", `:61` "... has no institution, so no role can be added here: ...",
  `:217` "Could not check for requests waiting for ‹name›. ...", `:~60` "... has adopted nothing this role can be scoped to yet, so it
  cannot be added. Adopt a curriculum first."; `UsersListWords.cs:145-147` "No accounts yet" / "Nobody has an account here yet. ...".
  (The Users empty card is labelled "no board" in code; the others are not labelled.)
- Fix: list them as NEW (with the reason there is no board) in `build-contracts.md`, or draw them. Words not on a board are what
  round 4, if any, has to see.

### P6 (should-fix): Assessors' rows-per-page change moves the focus to the heading, not to the select
- `AssessorsList.razor:193-212` (`ChangePageSizeAsync` -> `AfterTurn` -> `_heading.FocusAsync()`). `UsersList.razor:361-367` keeps the
  focus on the select ("T350 build review, A1"): a size change re-reads in place, and moving the focus off the control the person
  is using, mid-adjustment, drops the keyboard user's place.
- Fix: for a size change skip `AfterTurn`'s heading focus (only a page turn takes it).

### P7 (should-fix): GroupMappings' Try again has no `FocusAfterRetry`
- `GroupMappings.razor:47` (`<StatePanel ... OnRetry="LoadAsync">`). A retry that loads removes the alert and the button and the
  focus falls to the body (flow 11's P2; Users, Assessors, the user page and the record page all pass one).
- Fix: `FocusAfterRetry` to the h2 (`sso-add-heading` with `tabindex="-1"`, or the not-set-up card's h2).

### P8 (should-fix): `.dashboard-grid .detail-list` is not scoped to her Home and changes the Trainee's
- `app.css:6758-6761` sets `grid-template-columns: minmax(0, max-content) minmax(0, 1fr)` for every `.detail-list` in a
  `.dashboard-grid`; the base is `minmax(8rem, max-content) 1fr` (`:1984`) and `TraineeDashboard.razor` uses `.detail-list` too.
  The 8rem floor on the label column is gone for the Trainee's cards, and the `dd` rule (`:6764`) breaks values anywhere there.
- Fix: scope to her card (`#home-card-institution .detail-list`, or a class on her `dl`) or to `.dashboard-grid--institution`,
  and re-take the Trainee's Home at 390 if it stays.

### P9 (should-fix): two results can stand on the user page at once
- `AssessorTrainingSection.razor` keeps `_saved`/`_refusal` until its own next action; the page clears its result when the section
  acts (`OnActed="ClearResult"`, `UserDetail.razor:71`) but nothing tells the section when the page acts (`ClearResult()` at
  `:687, :844, :1100`). Save training, then Add a role (or Lock out) leaves "Training saved for ..." above a second, focused result.
  The page's header comment and DESIGN.md `:3534` say "one on the page at a time".
- Fix: a parameter the page bumps on `ClearResult` (e.g. `ResultVersion`) that the section reads in `OnParametersSet` to clear
  `_saved`/`_refusal`, or an `ActionResult` owned by the page and passed down.

### P10 (nit): AssessorTrainingSection focuses a heading that cannot take focus
- `AssessorTrainingSection.razor:265` (`FocusAfterRetryAsync` returns `HeadingId` for the caller's own page) but the h2 at `:35`
  and `:63` has no `tabindex="-1"`, so the call does nothing. Add `tabindex="-1"`.

### P11 (nit): `AssessorProfileEdit`'s Try again has no focus target
- `AssessorProfileEdit.razor:32`. After a retry that resolves to "User unavailable" the focus is on the body. Pass
  `FocusAfterRetry` to the empty card (give `StatePanel`'s empty title an id) or accept it as a legacy route. Low traffic.

### P12 (nit): Users hides the pager when the matches fit one page, with no way back from a large page size
- `UsersList.razor:196`, `AssessorsList.razor:129`: `> PageSize` is what F9 and the Left 2 note say, and it is right for the
  default. With `size=100` in the address and 30 matches the pager, and with it the Rows per page select, is gone, so the person
  cannot return to 20 except through the address. Fine for a first release; say so in the handoff, or draw the select alone.

### P13 (nit): "Open Trainees" in a role row is an inline link, under 44px at 390
- `UserDetail.razor:~411` (`<span class="role-row-note">... <a href=...>Open Trainees</a></span>`). The shared phone rule
  (`app.css`, `.user-page .btn`) covers buttons, not links; WCAG 2.5.8 excepts an inline link in a sentence, so this is not a
  failure, but it is the page's only link under 44px at 390. Make it `btn btn-outline btn-sm` or leave and record it.

### P14 (nit): the training form's date help is a `<small class="page-subtitle">`, everything else a `<p>`
- `AssessorTrainingSection.razor:~209` (`<small id=CompletedHelpId class="page-subtitle">`). Use `<p class="field-help">` as the
  other fields' help does.

### P15 (nit): the Detail page sample in DESIGN.md still shows a filled Delete in the header
- `DESIGN.md:3516` (`<button class="btn btn-danger btn-sm" ...>Delete</button>`). `:737` supersedes it in words, but the code
  sample under § Detail page is what a developer copies. Change it to `btn-outline` or add "(see § Button system:
  a destructive trigger is outline)" under it.

### P16 (nit): `InstitutionCreate` keeps a visible empty `ActionResult` and `FormField` markup
- `InstitutionCreate.razor:22-27`. Left "as built (flow 18's)" by design; the empty region leaves a 24px gap under the header
  because it sits outside `.record-page`. Harmless; flow 18 will take it.

## Checked and right (no action)

- **Words.** The Users, Assessors, user page, training, record, SSO and Home sentences equal the boards where a board exists
  (grepped ~40 strings in `boards-text.md`: Locking out the user prevents sign-in immediately, Keep the role, Keep access,
  Keep the sign-in, Remove sign-in, Reactivate user, Open Users, Open Curriculum adoptions, Add mapping, Mapping added, Read only:
  its provider is no longer set up, Institutional sign-in is not set up, Nobody holds the Assessor role yet, Training saved for,
  Choose the training status, Type the qualifications, ...). The page says "they" throughout. Heading counts, hidden live
  count and the No match card are generated from `UsersListWords` and read as the boards.
- **Headings.** One h1 per page (`PageHeader`; the record page and `AssessorProfileEdit` too). h2 sections (People, Roles and
  scope, Assessor training, Sign-in and access, Add a mapping, the mappings, the not-set-up card), h3 only for the three
  `.section-part`s of Sign-in and access. Titles: `Users`, the person's name or `User`, `Institution` before the read and on a
  failure, `SSO mappings`, `Assessors`.
- **Tables.** `StackWide` with explicit roles, `Caption` on all four tables, `th scope="col"` / `th scope="row"`; the empty
  Actions header is `visually-hidden`. Row actions are named per row and begin with the visible word (Manage, Record/Edit training
  for, Delete the mapping of ..., Remove ‹role› from ‹name›, Remove the ‹provider› sign-in from ‹name›, Lock out ‹name›), with
  `RowNames.Distinct` adding the address when two share a name.
- **Destructive actions.** Every trigger is `btn-outline` (`UserDetail.razor:~395, ~900, ~944`, `GroupMappings.razor:340`,
  `InstitutionEdit.razor:121`); `btn-danger` appears only through `ConfirmDialog`'s `DangerAction`. Dialogs open on the safe
  button (Cancel is first in the DOM, so the native dialog focuses it; form mode adds `autofocus`); its words say what is kept
  (Keep the role, Keep the sign-in, Keep access, Keep the mapping). The last way in is `aria-disabled` + `.is-unavailable` with
  `aria-describedby`, never `disabled`.
- **Focus.** Add a role... focuses the first radio; Cancel returns to the trigger; a refusal focuses its field with no summary
  (Role -> first radio, selects, `add-role-panel-error`); results are `ActionResult` + `FocusOnLoad` (page) or
  `FocusAfterRender` (section, SSO, record); Remove/Lock return to their trigger or the result; Show/Clear/page turn focus the
  People heading; the size change keeps the select (Users); `?section=training` focuses the first radio once, or the absence
  sentence.
- **ARIA wiring.** `aria-expanded` always, `aria-controls` only while the panel exists (M10); radios named by their label, help
  in a sibling `p` and in `aria-describedby`; `aria-invalid` on each radio/input and never on the fieldset (`.choice-group.is-invalid`
  carries the group); required marks `aria-hidden` with a `visually-hidden` "required" where the legend/label has none; every
  `aria-describedby` target is rendered under the same condition.
- **CSS.** No raw hex or `rgb()` in the 402 new lines (grep); no `style=` in any of the nine pages (grep); no `bi bi-`; every class
  the nine pages write is defined in `app.css` (scripted over the `class="..."` literals: 0 undefined; the C#-built
  `input-validation-error`, `is-invalid`, `choice-group.is-invalid` exist). Tokens used (`--space-*`, `--border-color`,
  `--muted-text`, `--link-color`, `--link-hover`) exist.
- **Section duplication.** The five new sections do not repeat each other: `.people-page .action-result:empty` appears once
  (`app.css:~6590`, with `.record-page`'s), lane T's `.user-section .action-result:empty` is a different selector, and lane I's
  and lane U's blocks add no second copy. The `.people-page > *` / `.user-page > *` / `.record-page > *` resets are one each.
- **Phone rules.** Below 641px: `.people-page .btn`, `.user-page .btn`, `.record-page .btn` and their inputs are 44px; forms
  stack `column-reverse` with Cancel first in the DOM, so the primary is on top and first in neither tab order nor reading order
  is lost (flow 11's lesson); the password field's Show keeps its width (M6: the full width is only `.section-part > .btn` and
  `.form-actions .btn`); radio labels are the 44px target (`.user-page .choice-group .form-check label`, S17); the dialog
  buttons come from flow 11's M8; Home's metric links and footer links are 44px (the footer through `app.css:2802`).
- **F9.** Both pagers are guarded by `> PageSize` (`UsersList.razor:196` on `MatchCount`, `AssessorsList.razor:129` on
  `TotalCount`), not `> 10`; DESIGN.md `:3496-3507` and `:3484` say so.
- **Left 3 on the name cell.** `th scope="row"` has no `.col-wrap` on Users, Assessors or the mappings; the address is
  `InvitationText.Address` (a `<wbr>` after the @). Only P2 and P3 above remain.
- **Components.** Results are `ActionResult`, load errors `StatePanel`, skeletons raw `div.skeleton`; standing notices are
  `Alert Role=""`; the waiting request in the dialog is an `Alert` with no role (S19, S23).
- **Not run.** No suite, no `DefinedClassTests`, no browser. The class check above is a script over the literals, not the test.
