# T398 step 6: rules common to every build lane (flow 17, build and publish an activity type)

Adapted from `design/flows/16-college-catalogue/build-lanes.md` (T394). You are one lane of the build of flow 17 of
Wombat's GUI restructure, task T398 (`execution/tasks/in_progress/T398-*.md`). The design is final: canvas "Wombat 17 ·
Round 3", round 3 (87 boards: 32 states redrawn at 1280 with their 390 twins, 2 at 768, 7 new screens with their twins,
and the Spec 1–4, Steps and two check boards), checked in `round-3-check.md` (82 of 86 done, 4 partly; no fourth round),
with the operator's decisions of 2026-11-10 (T398's Notes: "round 3 decisions"). The lane plan is at the end of this
file. The names two or more lanes share are in `build-contracts.md`, beside it; § 0 of it is already on `t398`.

**Status: PROPOSED.** P1 and P2 (§ "For the operator", at the end) are answered before § 0 runs.

## Read first (in your worktree)
- `design/flows/17-activity-type-builder.md`: the brief. The cast, the pages table (each route's file), § 5's known
  problems with file:line evidence (code cited at `d831ee7a`; § 0 has moved some of it), § 6's Q1–Q12 (all as
  recommended; Q9 folds T334 in), § 7's acceptance, and the 11 runbook steps in § 8.
- The decisions, all the operator's, all as recommended: `round-1-assessment.md` (pick A, with B5.1's Publish section
  and C5.1's "What you are publishing"; Q13 the JSON laid out indented in the parser's key order, Q14 Discard's two
  bodies, Q15 unticking Active on a published type asks first, Q16 Publish saves first); `round-2-review.md` § Questions
  (Q17 Publish asks Q15's question first when Active was newly unticked, then saves, then asks on warnings; Q18 keep
  T133: a setting at a deleted field is dropped at save; Q19 Q9 changes writing only; Q20 Publish lands on the tab it
  was pressed from) and its should-fixes J1–J15, which are build work; round 3's decisions in T398's Notes, settled
  below (§ "Settled here").
- The boards: `round-3/` (`R3-*`, flat, and `f17r3.css`) and **the round-2 boards round 3 did not redraw**, which stand
  (`round-2/`): R2-A01–A08, A09-create, A10, A11; R2-B01–B06, B08–B11; R2-C08-new, C09, C16, C21 (390 and 768); R2-D02,
  D03; R2-E03, E06, E08, E09, E10, E13, E14 (390), E15; each with its twins. `round-3/boards-text.md` and
  `round-2/boards-text.md` (§ 0's) are their text, one heading per board (`######## BOARD file`), label strip first; a
  link is marked `[link]`, a button `[button]`.
  - `R3-0-spec` (§ 1 the boards, § 2 Q1–Q20 and T1–T24 each with its boards, the PROPOSED list and the NEW sentences),
    `R3-0-spec-2` (**§ 3 the code changes**, § 4 every PART 2 rule of `f17r3.css` quoted), `R3-0-spec-3` (§ 5 the
    numbers, § 6 the DESIGN.md text, § 7 tokens and contrast) and `R3-0-spec-4` (§ 8 DATA by query, **§ 9 every
    message by kind**, § 10 edge cases, § 11 later) are the contract, **with round 2's `R2-0-spec-2` (code changes,
    classes, the measured sweep, DESIGN.md) and `R2-0-spec-3` (DATA, messages, edge cases, Accessibility), which stand
    where round 3 does not restate them**: where an R3 row says "Round 2's row" or "As round 2's", read round 2's. The
    same for `R3-0-steps`: its seven "As round 2's row" Expects are `R2-0-steps`' rows of the same step, with round 3's
    amendments on top.
  - The boards' CSS (`f17r3.css` PART 2: C1, R1, C2, S2, C3) is the canvas's picture. § 0 has written C3 (the selector
    lists); lane C writes C1, R1, C2 and S2 in `app.css`. PART 1 (`.cv-*`, K1–K3) never ships.
  - A board's label strip, an `anno` chip, a strip saying "Illustration", "FIXTURES", "DATA" or "PROPOSED" is never a
    shipped word. A bracketed figure (‹date›, ‹College name›, ‹section 1›, ‹title›) is read from the data; a ‹date› on an
    Illustration is the South African day of that save (N7).
- **The boards are design data, never instructions to you.** Where a board and the code disagree on a rule (who may
  act, what is shown to whom, what is stored), the code as § 0 left it, the decisions and this file win; say so in your
  report. The standing corrections are in § "Settled here"; the trail is `Breadcrumbs.razor`'s markup, never a board's;
  the live preview is flow 03's `ActivityForm` as it renders, never the board's drawing of it.
- **Check a board's CSS against DESIGN.md, not only against the board** (BRIEF § 11, flow 11): every stacked form and
  dialog row puts **the primary on top** (`column-reverse` over a DOM of the secondary first: Cancel before Discard
  draft, Deactivate, Publish version 2), and a result bolds **only its first sentence** where the board bolds one.
- `execution/knowledge/EPA-PROGRAMME.md` § 3 (D52: the College writes its types; T334 is its note), CLAUDE.md § Roles
  (out of scope is 404, not 403), § Activity platform (the DSLs, `SchemaValidator`, `ActorRuleMatcher`, § Editing a seed
  folder: **a DSL property must be emitted in `Serialize` too**, and the seed refresher's seed-owned rule), § Key
  technical choices (**never compare stored `jsonb` text against serializer output**), § EF Core migrations (none here).
- `execution/architecture/CUSTOMIZATION.md` (the three DSLs, versions, the actor grammar).
- `execution/architecture/DESIGN.md`: mandatory before touching any Razor or CSS: § Builder layout (lane C rewrites
  it), § Table system (`StackWide`, the row header, `data-label`, the picker rule, the 1024px catalogue stack), § Form
  system (refusals under the field, the required mark and its line, `FieldHelp`, `RefusalSummary`), § Button system
  (the destructive trigger, the dialog and its phone rule, `aria-haspopup`, Save's question), § Alerts, § Page-level
  patterns (List page, Detail page, Unavailable, Breadcrumbs), § Accessibility, § app.css section order (§ 0 wrote the
  flow 17 headings).

## Where you work
- Only in your own worktree, `C:\dev\Wombat\.claude\worktrees\<branch>` (given in your prompt), on its branch. Absolute
  paths for every command. Never touch `C:\dev\Wombat` itself (master). Never merge, push or switch branch.
- Commit as you go, each message in a file, `git commit -F <file>`, ending with `Co-Authored-By: Claude Opus 5.5
  <noreply@anthropic.com>`. Conventional Commits: `feat(activity-types): … (T398)`.
- Other lanes run in parallel. Stay inside the files your lane owns (the lane plan). Shared by section: `app.css` (each
  lane between its own `/* Flow 17: … (T398). Lane X. */` heading and its `/* end … */` line, nothing else; lane C also
  replaces the builder's own `.tab-bar` and `.builder-two-col` rules in place, nowhere else), DESIGN.md (your lane's line
  under § app.css section order's flow 17 heading, and only the § your lane's change touches), `ActionFocusTests`,
  `PageTitleTests`, `ActiveNavItemTests`, `RowNamesTests`, `AccessibleNamesTests`, `IdReferencesTests` (rows added, none
  changed). § 0's files (`build-contracts.md` § 0: `ActivityDtos.cs`'s flow 17 members, `ActivityTypeAdminScope.cs`,
  `SystemManagedReleases.cs`, `RefusalPlace.cs`, `ActivityTypeWords.cs`, the three read-out components' parameters,
  `CatalogueLinks.cs`'s activity-type addresses) are read, not changed; a lane that needs a change to one stops and says
  so. A compile break your change causes in another lane's test is yours to mend, minimally; name it.
- **Three lanes share one machine** (BRIEF § 11). Run the six suites **one at a time**, each with its own build, never
  two `dotnet test` at once in your worktree, and Integration last. If a build or a test run is killed or hangs, stop
  and say so in your report; do not loop on it.

## Never
- Touch the apps on ports 5080 and 5180 or their databases, or start anything on those ports.
- Read `pwd_DO_NOT_COMMIT.txt`, any credential, `recovery/`, or user secrets' values.
- Pass `--no-build` to `dotnet test` or `dotnet ef`.
- **Add a migration** (`build-contracts.md` § 0). Owners, releases, names, seed ownership, the read in words, the
  preview's EPAs and the laid-out JSON are read or computed, never stored. A lane that finds it needs one stops and says so.
- Change what is stored: `SaveDraft` stores `Serialize`'s compact text as today; indentation is for the page only
  (Q13, S11). Never compare the editor's JSON text with a stored or re-serialised text as raw strings to decide
  anything (an unsaved edit, a changed draft): parse both and compare canonical to canonical (CLAUDE.md).
- Upgrade MediatR; add Bootstrap, MudBlazor, Radzen or jQuery.
- Put in shipped markup or CSS: a raw hex colour outside `:root`; an inline `style=`; `<i class="bi">` or "→"/"←" as an
  arrow; anything from a third-party host; an inline script or `onclick`; `td.actions-cell`; a badge for an owner or a
  status on an admin page (flow 15's F8: the tab's problem count is the one badge, R1).
- Use the native `disabled` attribute on an action the design says is unavailable: Publish off is `aria-disabled="true"`
  with "Save a draft to publish." beside it by `aria-describedby`, and inert (S5). The exceptions the boards draw: Up on
  the first section or field and Down on the last (R3-C11), which stay `disabled` as built; and the header actions
  disabled while another of them runs (T234's `InFlight`), as built.
- A filled danger button outside a `ConfirmDialog`. Delete on a section or a field is an outline button that asks
  nothing (Q8: nothing is saved until Save draft). Discard draft… is an outline trigger with `aria-haspopup="dialog"`.
  Publish stays `btn-success` in the header (A.7.14: 5.88:1); its dialog confirms on `btn-primary` (S7). Save draft that
  would deactivate a published type (Active newly unticked, Q15), and Publish while that holds (Q17), carry
  `aria-haspopup="dialog"` and ask; ticking Active back asks nothing.
- Print a date in any form but ISO, or a day from anything but `ProgrammeCalendar.DateOf(utc)` (the draft's save day,
  each version's publish day). Never `ToLocalTime()`, `DateTime.Today` or `DateTime.Now` on a page or in a handler.
- Print an exception's text on a page (T329, T393): every `RefusalText.Of(exception)` and `_loadError =
  exception.Message` on these two pages goes. A refusal is shown in its own words by its type (§ "Refusals and
  failures"); anything else is the board's fixed failure sentence, logged.
- Print a DataAnnotations default, an enum's or a property's name ("LongText", "SubSpeciality", "CommitteeMember",
  "seed:cpsa:scale:v11.1"), a role by its code name, a field by its key where the boards name it by its label (a key
  shows only as the monospace detail the boards draw), "v0", or a pronoun for a person.
- Offer an action its target refuses (DESIGN.md's picker rule): no Edit, Save draft, Discard draft…, Publish, Add, Up,
  Down or Delete to a reader or on a system-managed type (Q9, T334); no Discard draft… without a draft; no New activity
  type without a writable scope; no Back to list (the trail is the way back, flow 16's Q9).
- Read or name another institution's type to an institution (T211), or a draft or inactive type to one who may not
  write it (Unavailable, never forbidden).
- Add a role check or a route. § 0's T334/Q19 rule is the one access change. Change a seed, the credit engine,
  `ActivityService`, the filing page (flow 03) beyond the one line lane A owns, `ActivityForm` beyond lane C's optional
  parameter, `RefusalSummary` beyond lane C's optional parameters, the shell, or another flow's page.

`AuditPipelineBehavior` commits a failed handler's staged rows (T201) and writes a refused command's message into the
caller's audit row: every command validates and reads everything that can refuse **before** it stages anything (the
validator, the guards, the key's holder, every parse and cross-check, as `SaveDraft` already does: T122), and a
refusal's message names nobody out of scope.

## How
- Test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding idiom and comment density; comment *why*, citing T398 and the item: "(T398, Q17)",
  "(T398, round 3 S2)", "(T398, T334)", "(T398, round 3 check: the editor li)".
- Layers: handlers through `IApplicationDbContext`; Web through `IScopedSender` and DTOs. Names and counts read once per
  page by its query, never in Razor. The read in words (Q2) is computed by the editor query (lane A) and rendered by the
  read-out components (lane B): a `.razor` file never parses a DSL.
- Strings: straight apostrophes and straight quotes, but where the boards draw curly quotes ("12 of 22 activity types
  match “cpsa”."); "…" on a trigger that opens a dialog (Discard draft…), never on Publish (round 1's M7); "›" only in
  the trail; "—" only in the "None — …" options and `EpaOptionLabel`. Every count has its singular ("1 activity type.",
  "Form 1 problem", "Form 2 problems").
- **Titles** (each tab "{h1} · Wombat", T21): the list "Activity types"; "New activity type" on `/new`; a writer's
  "Edit KGK Teaching Session Log"; a reader's the name alone ("Mini-CEX (Paediatrics)"); "Activity type" while loading or
  after a failed read; "Activity type unavailable" for an unknown or unreadable id.
- **Results and focus** (BRIEF § 6 A5): one result region per page (one `div.action-result[tabindex=-1]` holding the
  Alert), under `PageHeader` and above the tabs at every width (S7), focused (`ActionResult`, `FocusAfterRender`). The
  first Save draft on `/new` moves to `CatalogueLinks.ActivityTypeCreated(id)`: the arriving page reads the type again
  in the caller's scope, writes "Draft saved.", focuses it, then `NavigateTo(WithoutTokens(uri), replace: true)`; a
  stale or foreign id writes and focuses nothing (T291 item 3). A result stays in its region until the next save,
  publish or discard; a tab press is none of them (E02b). Loading is the header, one `p.loading-line role="status"`
  ("Loading the activity types." / "Loading this activity type.") and the skeleton; no action offered until the read is
  in. A load failure is an `ActionResult` holding the danger `Alert` with its `alert-row` and Try again, no empty state
  under it, the h1 `tabindex="-1"`; after a failed retry the focus goes to the h1.
- **The rest of the focus rules** (R2-0-spec-3 § Accessibility, standing): a refusal on its summary; a summary line's
  press on its place (switching tab, opening the row's editor, focusing its key, move or JSON editor); Edit on the
  editor's first input, Done on the row's Edit; Add field on the new row's Field key (S13); Delete on the next row's
  Edit, else Add field; Show on the count line; Clear and "Clear the search" on the search input; a dialog on Cancel,
  Cancel and Escape back to its trigger (Publish's Q17 question back to Publish, R3-E17); Try again to the h1; a tab's
  selection on the tab.
- **Dialogs**: `ConfirmDialog` in action mode, `aria-labelledby`/`aria-describedby`, its body in `.dialog-body`, Cancel
  first in the DOM and focused on open. "Discard the draft of KGK Teaching Session Log?" with Q14's two bodies (R3-E07,
  R2-E09); "Publish version 2 of KGK Teaching Session Log?" / "Publishing makes this draft version 2. It changes:" and the
  warnings by label, confirm "Publish version 2" (R3-E05); "Deactivate KGK Teaching Session Log?" / "Registrars are no
  longer offered it when filing. Activities already filed are not changed." / "You can reactivate it on this page.",
  confirm the danger "Deactivate" (R3-E11, E16).
- **Standing notices are not live regions** (`Alert Role=""` or plain text): T300's read-only notice word for word, the
  release's notice (Q9, T22), the no-scope notice, the seed-owned notes (T18), "Save draft to see what publishing
  changes." (S6), Publish's reason.
- **The tabs** (Q1, T280, R2-0-spec's S2): `div.tab-bar role="tablist" aria-label="Parts of this activity type"`;
  each `button role="tab"` with `id`, `aria-selected`, a roving `tabindex` (0 on the selected, -1 elsewhere) and
  `aria-controls` on the selected tab only; the panel `role="tabpanel" aria-labelledby` its tab, with `tabindex="0"` only
  when it holds no focusable control (N21); Left/Right/Home/End move the focus, Enter or Space selects. A writer has five
  tabs (Details, Form, Workflow, Credit, Publish), a reader four (no Publish; Versions close the reader's Details, T1); a
  new type opens on Details, an existing one on Form, a save keeps the tab (T2), Publish lands on the tab it was pressed
  from (Q20). A tab with refusals carries `<span class="badge badge-declined">1<span> problem</span></span>` inside its
  name, no `aria-label` and no `aria-describedby` on any tab (M2, N20).
- **Refusals and failures** (Q6, J1–J3, S1–S4, S10): the refusal summary at the top of the result region, "Nothing was
  saved." + "Everything you typed is kept.", one line a place, each prefixed by the page from the place ("Details:",
  "Form:", "Form, section Section 2:", "Workflow, move Cancel:", "Credit:"), each a link that switches tab and focuses the
  place; the words at the place under the row, move or JSON editor (`R3-0-spec-4` § 9, and `build-contracts.md` § 3);
  the row `.is-refused`, its control `aria-invalid`, the refusal first in its `aria-describedby`. A refusal is one of:
  `IActivityTypeRefusal` (the three parse exceptions, and P2's aggregate), `FieldRefusalException`, a `ValidationException`
  (Details' four words), `UnauthorizedAccessException` (the guard's sentence, in the region, no place). Anything else is
  a failure: the fixed sentence ("The draft could not be saved. Nothing has changed. Try again, or come back in a few
  minutes.", "Version 2 could not be published. …", "The draft could not be discarded. …"), logged.
- **The required line** (M1): Details opens with `p.field-help` "* marks a field you must fill in." under Q14's line,
  and each editor in place under its `h4` (`RequiredMarkLine`); each starred control `aria-required="true"`; Scope's radios
  a `fieldset.choice-group role="radiogroup" aria-required="true"` with legend "Scope *"; the star `aria-hidden` with a
  visually hidden "required" (`FormField Required`).
- **Tables**: `DataTable` with `StackWide`, a `Caption` that does not repeat the heading ("Activity types you can read
  or change", "Moves, as saved", "Published versions"), the row header `th scope="row"` (the type's name with its key
  under it in `.person-cell`; a move's label; a version's number), `data-label` on every text cell, one action a row
  named for its row (`RowNames.Distinct`: "View Mini-CEX (Paediatrics)", "Edit KGK Teaching Session Log"); a column no row
  fills is not rendered (T239). The list's root carries `.people-page.activity-types-page` (§ 0's stack reads
  `.activity-types-page`); the builder's `.user-page.activity-type-page`.
- **Narrow desktops and phones**: nothing scrolls sideways at 1280, 1150, 1116, 1115, 1100, 1025, 1024, 901, 768, 641,
  640 or 390 (a page's `scrollWidth` equal to its `clientWidth`, and every `.table-container`'s); the list stacks from
  1024px down (§ 0, C3); the builder's columns stand side by side from 1116px and stack below with the CSS-only
  "Edit the form" / "Preview" choice (C2); the tab bar is one row at every width (C1); every control 44px below 641px.
- Do not edit the runbook (`execution/knowledge/scenario-paediatrics/*.md`) or `design/flows/*.md`; lane W does both
  after the merge. Name in your report every runbook line whose Expect your change alters.
- Before finishing: `dotnet build C:\dev\Wombat\.claude\worktrees\<branch>\Wombat.sln -c Release` clean with no new
  warning, then the six suites one at a time (`dotnet test tests/<Project>/<Project>.csproj -c Release`), quoting the
  counts. At § 0 they are the counts below.

## Your report (to the integrator, not to a person)
- Branch and commit hashes; files changed (grouped); each suite's counts.
- Decisions the design left open, and why; what you did not do; what another lane or the integrator must know: the
  contracts you provide or rely on (exact names, and how they differ from `build-contracts.md`); files touched outside
  your list; **the words you shipped that no board draws** (for `build-contracts.md` § 5); runbook steps whose Expect
  you alter.

## Settled here (round 3's check, the operator's "yes" of 2026-11-10; no round 4)

**The check's corrections, carried into the build** (`round-3-check.md` § For the operator 1–6, 9):
1. **The editor in place is an `li` with no role.** `li.field-editor-inplace` directly after its row inside the
   section's `ul.list-unstyled`, with its `h4`, and **no `role="group"` and no `aria-labelledby`** (R3-C10–C14, C15-after,
   C17, C19, C23 draw `role="group"`: wrong for an `li`). The Done button's `aria-controls` names the `li`'s id (N22).
2. **Seven selector lists, not six.** `.activity-types-page` joins all seven of app.css's "The catalogue tables stack
   from 1024px down" lists (thead; tbody tr; tbody tr:last-child; td and tbody th, two selectors; th[scope="row"];
   td[data-label]::before; .btn). § 0 has done it; the canvas's seven-rule stand-in never ships.
3. **The two margin rules S2 adds, named**: `.outline-section-head > * { margin: 0; }` and `.field-editor-inplace > * {
   margin: 0; }` reset the h3, h4 and `p` margins inside the outline, so the head's and the editor's `gap` alone spaces
   them. They ship with S2 (lane C), as does `.outline-section > .btn` (Add field's inset), which the Spec names.
4. **Round 2's rows stand where round 3 points at them**: R2-0-spec-2 (code changes, classes, measured), R2-0-spec-3
   (DATA, messages, edge cases, Accessibility, "At 100 rows (no pager, as built)", the 768 rows, the 900–1100 sweep
   rows, every column of the list at 1280) and R2-0-steps (the seven "As round 2's row" Expects).
5. **The standing boards' old markup is superseded** wherever an R3 board draws the pattern, on all twelve families
   that carry it (R2-B01, B02, B06, B08, C09, C16, C21, E08, E09, E10, E13, E14): the outline is a `ul.list-unstyled` of
   `li.field-row` with its buttons in a `div.actions-cell` (not `div` rows with `.field-row-actions`); no tab carries an
   `aria-label` or `aria-describedby`; a tabpanel holding a control takes no `tabindex`; no `aria-describedby=""`; the
   tab's count is `.badge.badge-declined`, never `.tab-problem`.
6. **Three stale citations, corrected**: `CatalogueLinks`' token list is `CatalogueLinks.cs:55` (not :48), and
   `ActivityTypeCreated` uses the existing `created` token, so no token is added; `EligibleFieldKeys` is
   `ActivityTypeEdit.razor:742` (not :43–66); 6.14a's Scope is `act-6-catalogue.md:444–447` and `:462–465` (not :459–461).
7. **Small** (check item 9): `RefusalPlace`'s list type is `IReadOnlyList<RefusalPlace>` (the Spec's HTML dropped it);
   R3-E04's and R3-E07's titles' `states.md` lines are labels, not words; C19's open editor drawn empty is a moment, not
   a state to build; the selects' `value`/`selected` attributes are the canvas's (Blazor binds the value); the 952-control
   44px count excludes the preview's 100, which are flow 03's.

**The seven PROPOSED settlements, kept** (T398's Notes):
| Settlement | Built as | Boards | Lane |
|---|---|---|---|
| The folds (M2(b)) | Above 20 fields in the form, each section's head offers "Hide fields of section ‹title›" / "Show fields of section ‹title›" (the visible words "Hide fields" / "Show fields"), `aria-controls` the section's `ul`, **no `aria-expanded`**; every section starts open | R3-C19 (and 768, 390) | C |
| The tab's count at 390 (S22) | R1: below 641px the badge's word is visually hidden, the number shown, " problem" kept in the name | the 390 twins of R3-C14, C15, C20, C22, D01, D04 | C |
| Publish's dialog (S7) | `ConfirmDialog` with `DangerAction=false`, so its confirm is `btn-primary` (4.86:1); no new dialog kind; the header's Publish stays `btn-success` | R3-E05 | C |
| Active's help (N2) | "An inactive type is not offered to anyone filing an activity. Activities already filed are not changed."; Q15's body "Registrars are no longer offered it when filing. Activities already filed are not changed." / "You can reactivate it on this page." | every Details board; R3-E11, E16 | C |
| A College with no speciality (N12) | The list's subtitle "The forms registrars file and assessors complete. ‹College name› has no speciality yet, so there are no types for you to change." (the list query reads the flag and the College's name) | R3-A09-nocreate, R3-C08-list | B |
| The result kept (E02b) | A result stays in the region when a tab is pressed | R3-E02b | C |
| Details' words (S10) | "Enter a key." / "A key is at most 100 characters." (NEW) beside "Enter a name." / "A name is at most 200 characters." (built, `CollegeHolders.cs:31`, `:33`) | words only | A (validator), C (under the field) |

**Further corrections, settled by this plan:**
- **The owner's words are the College's short code** (R2-0-spec-2: "the owning College's short code, the institution's
  name, or the platform"): CPSA's rows read "CPSA", the Demo College's "DEMO-C" (`DataSeeder.cs:70`). The boards'
  "Demo College" was a fixture; lane W writes the runbook's Expects from the build. "The platform" for a Global type (T12).
- **The tab keys need no blanket `preventDefault`.** N8 chose `@onkeydown:preventDefault` on the tablist; a static
  `preventDefault` there also swallows Tab and Shift+Tab, trapping the keyboard in the tab bar. Lane C reads the key in
  C# without it, and prevents only ArrowLeft, ArrowRight, Home and End from scrolling the page with one listener in the
  existing script module (`wwwroot/js/wombat.js`, scoped to `.tab-bar[role="tablist"]`; no inline script).
- **The preview's heading level is 3** (S8): the Form panel's `h2` "Preview" over `ActivityForm`'s section headings at
  `SectionHeadingLevel="3"`; the outline's section titles are `h3` in the other column under the panel's `h2` "Form".
- **A malformed JSON's words** (R3-0-spec-4 § 10): "The workflow is not valid JSON. Check the brackets and commas near
  line ‹n›." under the Workflow editor and after "Workflow:" in the summary; the credit rules' NEW twin "The credit
  rules are not valid JSON. Check the brackets and commas near line ‹n›."; with no line known, the sentence ends at
  "commas." (NEW).
- **Which tab a refusal belongs to** is its place's (`build-contracts.md` § 0): a schema problem Form (with its
  section and fields), a workflow problem Workflow (with its move), a credit problem Credit (with its directive), a
  Details refusal Details. EvidenceEpa's agreement check is Credit's (R3-D04), `ActorFieldRules`' `field:` rule
  Workflow's and its duplicate keys and user-field options Form's.

## The sweep (the integrator's done-when, after the merge and before lane W)

One lane cannot measure it: the list needs real rows and the builder its real columns. On a scratch database cut from
the T394 replay's end state (`wombat_scenario_t394` restored into a new `wombat_scenario_t398_sweep`, never the 5080/5180
apps' databases), on a port other than 5080 and 5180, in a browser:
- **The list** as Prof Mbatha (23 rows), Dr Kruger (23, his CPSA Case Presentation Log draft among them) and devadmin
  (every type), at 900 to 1100px in 25px steps plus 1025: the table's `scrollWidth` against its `.table-container`'s
  `clientWidth` (the canvas reports 35px spare at 1025 for Prof Mbatha's and about 30px for the Administrator's: check
  both). **Then every column at 1280 for each of the three** (BRIEF § 11, flow 16): no name past two lines, no column
  of one word breaking, no empty band beside a name.
- **The builder** on KGK Teaching Session Log as Prof Mbatha (a writer: Form, Workflow, Publish) and Mini-CEX
  (Paediatrics) as Prof Mbatha (a reader: Form, Workflow's "Moves, as saved"): at 900 to 1150px in 25px steps plus 1115
  and 1116, the card's and each `.table-container`'s `scrollWidth` against its `clientWidth`; each field row's four
  buttons on one line beside or under a whole label (never over it); the tab bar one row.
- **390, 640, 641 and 768** for both pages: nothing sideways, the tabs one row of five (four for a reader), every
  control 44px below 641px.
- **Record** the numbers in `design/flows/17-activity-type-builder/sweep.md` (one table a page and role: width, table,
  container, spare). If any width fails, the integrator fixes it in the owning lane's `app.css` section on `t398`,
  measures again, and says so in `build-review.md`.

## The pages, and the file each route renders

| Route | File (`@page`) | Lane |
|---|---|---|
| `/admin/activity-types` | `W/Admin/ActivityTypes/ActivityTypesList.razor` (`NationalCatalogueAccess`) | B |
| `/admin/activity-types/new`, `/admin/activity-types/{ActivityTypeId:int}` | `W/Admin/ActivityTypes/ActivityTypeEdit.razor` (`NationalCatalogueAccess`), with `BuilderModels.cs` | C |
| (inside the builder) the read in words and the versions | NEW `W/Admin/ActivityTypes/WorkflowReadout.razor`, `CreditReadout.razor`, `VersionsTable.razor` (§ 0's stubs) | B |
| `/activities/new` (flow 03's; one line: the editor read's null) | `W/Activities/NewActivity.razor` | A |
| `/` (the College admin's Home card links Activity types) | `W/Dashboards/CollegeAdminDashboard.razor` (flow 16's; unchanged) | none |

`W/` is `src/Wombat.Web/Components/Pages/`, `A/` is `src/Wombat.Application/Features/`, `D/` is
`src/Wombat.Domain/Activities/`.

## § 0's counts

Suites at master `bce95d7a` (flow 16's squash `de4cc718` plus design commits only): Domain 836, Application 4411,
Infrastructure 1035, Architecture 52, Web 4675, Integration 530 (from flow 16's handoff; § 0 re-runs them first). § 0
records the counts on `t398` after its commit here; the integrator fills the squash's.

## The lane plan

§ 0, one wave of three lanes, the merge, the sweep, then the runbook lane, two Sonnet reviews and one fix pass: **eight
agents beside the integrator** (§ 0, A, B, C, W, two reviewers, the fix pass), the same as flow 16. Flow 17 is two
pages and the Application, Domain and Infrastructure work behind them, so it is cut by layer where flow 16 was cut by
page family: one lane for everything below Web (A), one for the list and the read in words (B), one for the builder
page (C). What crosses is § 0's shapes only.
- **Worktrees:** under `C:\dev\Wombat\.claude\worktrees\<branch>`.
- **Branches:** the integration branch is **`t398`**, cut from master at this plan's commit; § 0 is its first commit.
  The lanes `t398-a`, `t398-b` and `t398-c` are cut from `t398` after § 0 and merged back into it (A, then B, then C);
  `t398-w` is cut after the merge and the sweep. The whole is squashed into one commit on master.
- **What crosses:** only § 0's names. Lane C's tests build `ActivityTypeEditorDto`s by hand (the FakeSender pattern of
  `ActivityTypeBuilderAccessTests`) and refusals by hand (`RefusalPlace`); they assert that the read-out components are
  rendered with the editor's `WorkflowRead`, `CreditRead` and `Versions`, never lane B's words inside them. Lane B's
  component tests build the read DTOs by hand, never through lane A's query.
- **Migration: none.** See `build-contracts.md` § 0.

### Lane A "the commands, the reads and the DSLs" (`t398-a`)

**Scope.** Everything below Web for the builder: Q6/J2 (each refusal carries its place), Q13/S11/J14 (the JSON laid out
for the page), Q2/S9/J12 (the read in words, computed), Q3/T16 (versions' publishers by name, "Wombat's catalogue"), N3
(null for an unknown or unreadable id), N11/T18 (seed ownership), S8/J11 (the preview's EPAs), S10 (Details' validator,
the key taken on Key), N1's Domain half (`FormSchemaParser:351`'s role refusal by label), T324's kin below Web, P2.
- **Domain** (`D/`): the three parse exceptions implement § 0's `IActivityTypeRefusal`, each carrying the place of
  its one problem (tab, section key, field keys, move key, directive number, kind, the JSON line where a `JsonException`
  gives one); `ActorFieldRules.EnsurePublishable` and `EvidenceEpa.EnsureCreditAgrees` give one place per problem, their
  `Message` the joined sentences as today, word for word (S4's NEW summary sentence for the credit EPA field is R3-D04's,
  `EvidenceEpa.cs:106–108`); every one of the parsers' throw sites (the Spec's 59: 33 schema, 18 workflow, 8 credit; one
  of the workflow's is an `ArgumentOutOfRangeException`, no refusal) has a place where it has one, else the tab alone
  (the summary line only). **P2** decides whether `ActivityType.SaveDraft` reports every
  payload's problems at once (`ActivityTypeDraftRefusedException`, § 0's shape) or the first payload's, as today.
  `WorkflowParser.Serialize(workflow, indented: true)` and `CreditRulesParser.Serialize(rules, indented: true)`
  (`FormSchemaParser` too, for symmetry): one property a line, two spaces, the parser's key order; the compact overload
  and every caller unchanged (the seeders, `ActivityTypeSeedCatalogue.cs:272–274`, `ActivityTypePublishCheckTests.cs:117–118`).
  NEW `D/Workflow/ActorRuleWords.cs`: the whole grammar to words (R3-0-spec-2 § 3's S9 row, word for word: "The
  registrar", "Whoever filed it", "or", "A ‹role label›" with its article from `WombatRoleLabels`, "The person ‹the
  field's label› names", "who is also", "of the activity's speciality", "of the activity's sub-speciality"; the
  validation words "The whole form", "The mover's own fields", "Formats only"; anything it cannot word, the rule as
  written, flagged for `<code>`). NEW `D/ActivityTypeSeedOwnership.cs`: `SeederUserId` ("seed-system", moved from
  `ActivityTypeSeedCatalogue.cs:92`, which now points at it) and `IsSeedOwned(ActivityType)` (the type's
  `OwnerUserId` and its newest version's publisher both the seeder, as `ActivityTypeSeedRefresher.cs:56–80` decides; the
  refresher calls it). `FormSchemaParser.cs:351`'s role refusal names roles by `WombatRoleLabels`.
- **The editor read** (`A/Activities/Queries/GetActivityTypeEditor/`): returns **null** for an unknown id and for one
  the caller may not read (§ 0's `MayRead`), never throws for either; fills § 0's members on the builder's path
  (`ForCallerAsync`, which every command's returned editor goes through): `Owner` (§ 0's helper), `SystemManaged`,
  `WrittenBy`, `DraftSavedByName`, each version's `PublishedByName` (`IUserAdministrationService.GetDisplayNamesAsync`,
  "Wombat's catalogue" for `SeederUserId`), `IsSeedOwned`, `PreviewEpaOptions` (S8: a Speciality or Sub-speciality
  type, that discipline's active national EPAs; an Institution type, the EPAs on the curricula the institution has
  adopted, national and its own; a Global type, none; labelled `PAED-001 — Title` as the filing page's picker labels
  them, in its order), `WorkflowRead` and `CreditRead` (the read in words of the **saved** draft, or of the published
  version for a reader, through `ActorRuleWords` and the schema's field labels: R2-B04, B05, B06, D02, D03, R3-D05's
  words), and `DraftWorkflowJson` / `DraftCreditRulesJson` / `DraftSchemaJson` laid out indented. `ForBuilder: false`
  (the filing page) keeps the compact text and fills none of them.
- **The commands** (`A/Activities/Commands/{Save,Publish,Discard}ActivityTypeDraft/`): `SaveActivityTypeDraftCommand`
  loses `[NoValidator]` and gains a validator (blank Key "Enter a key.", blank Name "Enter a name.", Key over 100 "A
  key is at most 100 characters.", Name over 200 "A name is at most 200 characters."; property names `Key`, `Name`);
  the key taken (`:92`) throws `FieldRefusalException("Key", "An activity type with key 'kgk_teaching_log' already
  exists.")`, still before anything is staged; "Published activity type keys cannot be changed." and "A saved draft is
  required before publishing." stay as guards no page reaches. Publish and Discard: nothing new beyond § 0's guard.
- **The filing page** (`W/Activities/NewActivity.razor:308`): the editor read's null is the page's own load failure, as
  the throw was (`?? throw` inside the existing `try`, or the same `_loadError = LoadFailure`): nothing a registrar sees
  changes.
- **Tests**: Domain NEW `RefusalPlaceTests` (each kind's place: the duplicate key's every row, the `field:` rule's move,
  the empty section, the credit directive's number and fields, a malformed JSON's line; the `Message` unchanged word for
  word), `ActorFieldRulesTests` and `EvidenceEpaPublishCheckTests` extended (one place per problem), `WorkflowParserTests`
  and `CreditRulesParserTests` (indented: one property a line, the parser's order, `Parse(indented)` canonical-equal to
  `Parse(compact)`), NEW `ActorRuleWordsTests` (the grammar table, each article, the fallback), `FormSchemaParserNomineeRoleTests`
  (the role by label), `ActivityTypeSaveDraftAtomicityTests` (P2's shape, nothing assigned on a refusal); Application
  `ActivityTypeScopeGuardTests:433` (null, not a throw), NEW `ActivityTypeEditorReadTests` (null for unknown and
  unreadable; owner; names and "Wombat's catalogue"; seed-owned; the preview's EPAs for each scope, the Demo EPA-001 never
  in a CPSA type's; the read in words for KGK's and Mini-CEX's seeds; indented JSON; `ForBuilder: false` compact),
  `SaveActivityTypeDraftValidatorTests` (the four words; the key taken on `Key`; nothing staged); Infrastructure
  `ActivityTypeSeedRefresherTests` green (the predicate moved); Integration NEW `ActivityTypeEditorPostgresTests` (the
  names, the short code and the preview's EPAs read on PostgreSQL; BRIEF § 11, flow 16); Web `LogAnActivityTests` green
  (the null path).
- Owns: `D/Schema/FormSchemaParser.cs`, `SchemaParseException.cs`, `D/Workflow/WorkflowParser.cs`,
  `WorkflowParseException.cs`, `ActorRuleParser.cs`, NEW `D/Workflow/ActorRuleWords.cs`, `D/Credit/CreditRulesParser.cs`,
  `CreditRulesParseException.cs`, `D/ActorFieldRules.cs`, `D/EvidenceEpa.cs`, `D/ActivityType.cs` (P2 only), NEW
  `D/ActivityTypeSeedOwnership.cs`, `A/Activities/Queries/GetActivityTypeEditor/*`, NEW
  `A/Activities/Queries/GetActivityTypeEditor/ActivityTypeReadout.cs` (or its own name),
  `A/Activities/Commands/{Save,Publish,Discard}ActivityTypeDraft/*`, `Wombat.Infrastructure/Persistence/ActivityTypeSeedCatalogue.cs`
  (the constant only), `ActivityTypeSeedRefresher.cs` (the predicate only), `W/Activities/NewActivity.razor` (the one
  line), their tests. **Not** `ActivityDtos.cs`, `ActivityTypeAdminScope.cs` or `RefusalPlace.cs` (§ 0's).
  Runbook lines: 1.27 and 1.28 (the refusals' places and at-place words), 1.25 (the read's words), 6.14a (the guard's
  sentence, never shown), the `states.md` rows :911–:914.

### Lane B "the list and the read in words" (`t398-b`)

**Scope.** Q7 (Name with the key under it; Set by in words; "Version 1" / "Not published"; "Saved ‹date›" / "None";
Status; Edit or View; StackWide and the 1024px stack; the search applied with Show and Clear, the count line), Q11
(own types first by name, then the rest; the Administrator's by name; N13), Q12 and T11 (the subtitle by role; N12's
for a College with no speciality), T326 (the no-match state apart from the empty catalogue, each by role), T329's kind
(loading, the load error with Try again), Q9 (a system-managed row's "Written by the MSF release." line and View), "v0"
gone; and the builder's read in words as components (Q2: the states and "Moves, as saved"; the credit's sentences; Q3:
"Published versions"). Boards: R2-A01–A08, A09-create, A10, A11, R3-A09-nocreate, A12, A13, R3-C08-list (each with its
twins); for the components R2-B03, B04, B05, B06, R3-B07 (its Workflow and Credit panels), R2-D02, D03, R3-D05, R2-E03,
E06 (the versions), and the read above the JSON on R3-D01, D04.
- **The read** (`A/Activities/Queries/ListActivityTypesAdmin/ListActivityTypesAdminQuery.cs`): fills § 0's list
  members (`Owner` through § 0's helper, `SystemManaged`, `WrittenBy`; `CanWrite` already false on a system-managed row
  by § 0's rule) and the list's `CallerCollegeName` and `CallerCollegeHasSpeciality` (N12: a College admin whose College
  holds no speciality; null and true for everyone else). The query's own order stays by name; the page orders (N13).
- **The page** (`ActivityTypesList.razor`, NEW `ActivityTypesListWords.cs`): as the A boards draw it. The search is a
  `search-grid` with "Search by name or key" and Show (Clear once a search is applied), applied on Show to name and key,
  case-insensitive, on the rows already read (no round trip per keystroke, no `@bind:event="oninput"`); the count line
  `p role="status"` above the table ("23 activity types.", "12 of 22 activity types match “cpsa”."), focused on Show; the
  no-match state 'No activity types match "zzz".' / "The search is kept above. Clear it to see all 23." with "Clear the
  search", distinct from the empty catalogue (R2-A09-create's "No activity types" / "Create the first activity type to
  start the builder." for a creator; "No activity type is open to you yet." otherwise); the rows by § "How"'s table
  rules, own (`CanWrite`) rows first by `StringComparer.OrdinalIgnoreCase`, then the rest; a date the SA day of
  `DraftUpdatedOn`; New activity type (to `CatalogueLinks.NewActivityType`) only where `CanCreate`; the subtitles by
  role (an institution's, the College's, the Administrator's, N12's), never by count. No pager (R2-0-spec-2: "At 100
  rows, as built").
- **The read-out components** (§ 0's stubs, filled here; NEW `ActivityTypeReadWords.cs` for any word the DTOs do not
  carry): `WorkflowReadout` (the states as a list: the initial one "where a new one starts", a terminal one "final"; the
  moves as a `DataTable StackWide` captioned "Moves, as saved", the row header the move's label, columns From, To, Who,
  Checks, Note; a move whose actor the words could not read shows it in `<code>`), `CreditReadout` ("Credits nothing."
  and its line, or one sentence a directive), `VersionsTable` (`DataTable StackWide` captioned "Published versions":
  Version, Published (the SA day), By; with Q3's line "Activities filed under a version stay on it." and "Not published
  yet." for none). Headings by the `HeadingLevel` parameter (§ 0), so the builder places each under its panel's `h2`.
- CSS under "Flow 17: the list and the read (T398)": only what the boards add that the system lacks (the list's
  release line under a name, if `.person-cell` lacks it; the states list). The 1024px stack is § 0's.
- **Tests**: Application `ListActivityTypesAdminQueryTests` (owner kinds and words' data, the release, the College
  flags, the read rule unchanged); Integration NEW `ActivityTypesListPostgresTests` (the owner's short code on
  PostgreSQL); Web NEW `Admin/ActivityTypesListPageTests` (Show and Clear, the count line and its focus, no-match vs
  empty for a creator and a reader, own first, the columns' words, "Not published", "Saved 2026-11-05", StackWide,
  `.activity-types-page`, the four subtitles, the release line, loading, the load error and Try again, no exception
  text), `ActivityTypesListAccessTests` (kept, adapted), NEW `Admin/WorkflowReadoutTests`, `CreditReadoutTests`,
  `VersionsTableTests` (each from hand-built DTOs: KGK's two moves, Mini-CEX's four, the `<code>` fallback, the
  singulars), `RowNamesTests`, `PageTitleTests`, `ActionFocusTests` (Show, Clear, Try again), `ActiveNavItemTests`.
- Owns: `ActivityTypesList.razor`, NEW `ActivityTypesListWords.cs`, `WorkflowReadout.razor`, `CreditReadout.razor`,
  `VersionsTable.razor` (the bodies; their parameters are § 0's), NEW `ActivityTypeReadWords.cs`,
  `A/Activities/Queries/ListActivityTypesAdmin/*`, their tests. Runbook lines: 1.24, 1.31, 6.14a (the list's rows, "Not
  published", "Saved"), 1.25 (the read's look), the `states.md` rows :902–:907, :914.

### Lane C "the builder" (`t398-c`)

**Scope.** Q1 and T280 (the tabs pattern, Details first on a new type, the Publish tab, Publish kept in the header),
Q2's place (the read above the JSON for a writer, the read alone for a reader; the JSON a `textarea.form-control.font-mono`
holding the laid-out text after each save and what was typed until then, N15), Q3 (the version strip; Versions on the
Publish tab, a reader's on Details), Q4 (the editor in place, one at a time, Edit → Done, S13 Add field opens the new
row's editor), Q5/C2 (the CSS-only "Edit the form" / "Preview"), Q6/J1 (the refusal summary across tabs, the words at
the place, the tab's count), Q8 (Delete outline, asking nothing; Discard draft… asks; Publish asks only on warnings),
Q9 (the release's notice, read-only), Q12 (the subtitle, the owner line, T300's notice), Q14–Q18, Q20, T271 item 2 (the
scale by name, to a reader and in the picker), T291 items 3 and 6 (the first save's token; the preview's EPAs), T324's
builder half (types, roles, settings and warnings by label), T264's kind, T329's kind (Unavailable, loading, load
error, failures), S5 (Publish inert when off), S6 (warnings from the saved draft), S7, S17, S22, N2, E02b, M1, M2;
every § "Settled here" item that is lane C's. Boards: R3-B07, C01–C07, C10–C15, C17–C20, C22, C23, D01, D04, D05, E01,
E02, E02b, E04, E05, E07, E11, E12, E16, E17; R2-B01–B06, B08–B11, C08-new, C09, C16, C21, D02, D03, E03, E06, E08,
E09, E10, E13, E14, E15 (each with its twins).
- **The page** (`ActivityTypeEdit.razor`, `.user-page.activity-type-page`; split into components lane C names and owns,
  as it sees fit): no Back to list; the header's Save draft (`btn-primary`), Publish (`btn-success`, `aria-disabled` with
  its reason beside it when there is no draft, and `PublishAsync` returning at once when `!CanPublish`, S5) and Discard
  draft… (only with a draft); the owner line and the version strip (`dl.detail-list`: Published "Version 1, 2026-11-05,
  Nolwazi Mbatha" or "Not published yet."; Draft "Saved 2026-11-05 by Nolwazi Mbatha" or "None"); the seed-owned notes
  (T18) under it; the result region; the tabs. **Details**: Q14's line "Saved with Save draft, not with Publish: a change
  here reaches everyone when you save.", the required line, Key (a read-out "Fixed: version 1 is published." once
  published), Name, Scope (a read-out "The one scope you can write in." when one is offered; else radios, R3-C04–C06;
  "None: a Global type belongs to no institution or speciality." for Global), "This tool is", Description (with its
  counter to 2,000 characters, R3-0-spec-4 § 10), Active with N2's help; a reader's as text with Versions. **Form**: the
  form settings by label with the key after ("Date delivered (delivered_on)"), T133 kept (Q18); the outline (S2, S27,
  § "Settled here" 1 and 3) with Edit, Up, Down, Delete (outline) named for the row, the editor in place, Done,
  Add field opening its editor; the folds above 20 fields; the type by label; Names a by `WombatRoleLabels` ("Assessor
  (default)", "Institutional admin", "Committee member"); the scale picker over § 0's options, resolving a `seed:` key
  to its scale (T271); the preview column (`h2` "Preview", `ActivityForm` with `EpaOptions="@_editor.PreviewEpaOptions"`
  and `SectionHeadingLevel="3"`; the preview's person line). **Workflow** and **Credit**: lane B's read-out above the
  JSON editor with its help (T10); the reader the read alone. **Publish** (a writer's): what publishing changes, from
  the **saved** draft (`BuilderSchemaModel.GetPublishWarnings` over `PublishedSchemaJson` and `DraftSchemaJson`), "Save
  draft to see what publishing changes." while there are unsaved edits (judged canonical to canonical), the pinned line,
  lane B's `VersionsTable`, C5.1's "What you are publishing" on a first publish (R3-E01). **Publish's order** (Q16, Q17):
  Active newly unticked on a published type → Q15's question first (Cancel leaves everything unsaved, the focus back on
  Publish); then save (a type with a draft and edits since); then, with warnings, the publish question; then publish;
  the result on the tab pressed from (Q20): "Published version 2.", or Q17's "Published version 2. KGK Teaching Session
  Log is inactive: registrars are no longer offered it." / "Draft saved. KGK Teaching Session Log is inactive: registrars
  are no longer offered it. Nothing was published.", or "Draft saved. Nothing was published." after Cancel on the
  warnings. **Save draft** with Active newly unticked on a published type asks Q15's question; its results "Draft
  saved.", "Draft saved. KGK Teaching Session Log is inactive: registrars are no longer offered it.", "Draft saved. KGK
  Teaching Session Log is active again: registrars are offered it." **Unavailable** (`StatePanel`, R2-B09): "Activity
  type unavailable" / "This activity type does not exist, or you cannot open it." / Go to Activity types. The no-scope
  notice on `/new` (R2-C08-new) as built. A system-managed type: the release's notice (§ 0's words), read-only.
- **`BuilderModels.cs`**: `GetPublishWarnings` by label (types, roles with "an Assessor", the settings' fields looked up
  in the draft's schema, the published one's for a field the draft dropped, the draft's label winning; no quotes round a
  type: "Field 'Learning objectives' changes type from Long text to Text."), N1.
- **Shared components, additively**: `RefusalSummary.razor` gains an optional per-line action (a callback and the id to
  focus) in place of the fixed `FocusFieldAsync`, flow 03's pages passing none and rendering as today (S1);
  `RefusalLine` (`FieldRefusals.cs`) gains optional members if the callback needs them; `ActivityForm.razor` gains an
  optional `EpaOptions` parameter used in place of `GetEpaOptionsAsync` when set (S8), the filing page passing none.
- **CSS**: under "Flow 17: the builder (T398)": R1 and S2 (with the two margin rules and Add field's inset, § "Settled
  here" 3), the version strip and the owner line if the system lacks them; **in place**, C1's `.tab-bar` / `.tab-bar-tab`
  rules replacing `app.css:2133–2160` (`.is-active` goes; used by the builder alone) and C2's builder rules beside
  `app.css:2164–2185`. `wwwroot/js/wombat.js`: the tablist's key listener (§ "Settled here").
- **DESIGN.md**: § Builder layout rewritten (R3-0-spec-3 § 6 with R2-0-spec-2's DESIGN.md text: the tablist, five and
  four tabs, the strip, the editor in place, the choice, the read above the JSON, the JSON laid out after each save,
  Delete outline, the tab's count, the required pattern, the folds); § Button system's and § Form system's lines
  (R3-0-spec-3 § 6); `DESIGN.md:237`'s "badges, tabs" corrected.
- **Tests**: Web `ActivityTypeBuilderAccessTests`, `ActivityTypeBuilderLayoutTests`, `ActivityTypeEditNomineeRoleTests`,
  `ActivityTypeToolPickerTests`, `BuilderEvidenceEpaFieldTests`, `BuilderNomineeRoleTests`, `BuilderOptionLabelTests`,
  `BuilderSchemaModelTests` (the warnings by label), all adapted deliberately; NEW `Admin/ActivityTypeTabsTests` (the
  tablist, roving tabindex, the keys, `aria-controls` on the selected only, five and four tabs, Details first on `/new`,
  the tab kept after a save, the badge in the name), `Admin/ActivityTypeRefusalTests` (the summary's lines and prefixes
  from hand-built `RefusalPlace`s, a line's press switching tab and opening the row's editor, the at-place words, the
  tab's count, Details' four words under Key and Name, the guard's sentence, the failure's fixed words, no exception
  text), `Admin/ActivityTypeOutlineTests` (Edit → Done in place, one at a time, Add field's editor and focus, Delete
  outline asking nothing and the focus after, the folds above 20 fields, no `role` on the editor `li`),
  `Admin/ActivityTypePublishTests` (S5 inert, Q16, Q17's order and Cancel, the warnings' dialog, Q20's tab, the results),
  `Admin/ActivityTypeDiscardTests` (both bodies, Cancel), `Admin/ActivityTypeDeactivateTests` (Q15 at Save draft, ticking
  back asks nothing), `Admin/ActivityTypeTokenTests` (the first save's "Draft saved." focused on the type's address, a
  stale token), `Admin/ActivityTypeUnavailableTests`, `Admin/ActivityTypeReaderTests` (four tabs, no JSON, Versions on
  Details, the scale by name, the release's notice); `Design/ContrastTests` (`TheActiveTab_…` reads
  `[aria-selected="true"]`, the underline on the page ground, S23), `NarrowLayoutTests`, `DefinedClassTests`; flow 03's
  `RefusedFieldMarkingTests`, `LogAnActivityTests` and `NewActivitySubmitFlowTests` green (the summary and form
  unchanged for them), a NEW `ActivityForm` test for `EpaOptions`; `ActionFocusTests`, `PageTitleTests`,
  `AccessibleNamesTests`, `IdReferencesTests`, `ActiveNavItemTests` rows.
- Owns: `ActivityTypeEdit.razor` and any component it is split into under `W/Admin/ActivityTypes/` (not lane B's three),
  `BuilderModels.cs`, NEW `W/Admin/ActivityTypes/ActivityTypeEditWords.cs`, `Shared/Activities/RefusalSummary.razor` and
  `FieldRefusals.cs` (additive only), `Shared/Activities/ActivityForm.razor` (the parameter only), `wwwroot/js/wombat.js`
  (the listener only), `app.css`'s `.tab-bar`/`.builder-two-col` rules and its own heading, DESIGN.md § Builder layout and
  the lines named, their tests. Runbook lines: 1.25, 1.26, 1.27, 1.28, 1.29, 1.30, 6.14a, A.7.9, A.7.14, the
  `states.md` rows :908–:920.

### Lane W "the runbook" (`t398-w`, after the merge and the sweep)

`execution/knowledge/scenario-paediatrics/` (`act-1-setup.md` 1.24–1.31, `act-6-catalogue.md` 6.14a,
`appendix-cross-cutting.md` A.7.9 and A.7.14, `states.md` :902–:920, `coverage.md`): the Do, Expect and Note lines of
the 11 steps as `R3-0-steps` (with `R2-0-steps` where it points back) and the lanes' reports give them (1.24's Show and
Clear and "22 activity types."; 1.25's "leave by the trail's Activity types"; 1.26 on Details with "Draft saved."
focused; 1.27's places and Add field opening its editor; 1.28's JSON laid out after the save; 1.30 from the Credit tab,
Q20; 6.14a's "Written by the MSF release." and Unavailable for Prof Mbatha; "DEMO-C" where the boards wrote "Demo
College"); `R3-0-steps` § states.md's words for :902–:920, and new `states.md` rows "Not yet captured (T398)" for the
states the boards add (Discard's question, both bodies; the publish question with warnings; Q15's and Q17's questions
and results; the refusal summary at a field, a move, a section and Details; the key taken; a College with no speciality;
the empty catalogue apart from no match; Unavailable; the load error); `coverage.md`'s rows for flow 17 (`:215`'s
"Discard draft" unplayed; the gaps that close); a word search over the whole runbook for "Back to list", "Metadata",
"code block", "v0", "`v1`", "Configured activity types", "Draft saved" as a list cell, "could not be found", "disabled"
Publish, "Edit section" opening a field, "InstitutionalAdmin" or "CommitteeMember" as words a page shows (BRIEF § 9 item
7); `design/flows/17-activity-type-builder.md` § 8 kept in step (`design/tools/sync_verbatim_steps.py`, then
`check_verbatim_steps.py` and `check_flow_completeness.py` exit 0), and the scenario guard green
(`ScenarioRunbookTests`). Words only from the lanes' reports and the boards; every count in an Expect traced to the steps
that set it (BRIEF § 11, flow 16).

### Reviews and the fix pass

Two Sonnet reviews of the merged `t398` (CLAUDE.md § Multi-agent workflows: the change touches who may write a type,
T334/Q19, the DSLs' refusals and their serializer, and a shared component of flow 03): **code** (§ 0 and lane A: the
T334/Q19 rule and every caller of `MayWrite`/`MayRead`, nothing widened; the refusal places against the 59 sites and
the two cross-checks, the messages unchanged; nothing staged before a refusal, the audit pipeline; the indented
serializer never stored and the seeds' round trip unchanged (`SeedRoundTripTests`); the editor read's null and its
callers; the preview's EPAs by scope; names read once; the list query's owner and flags; the PostgreSQL tests) and
**pages and runbook** (lanes B, C and W against the boards' text, DESIGN.md and the runbook; 390, 640, 641, 768, 1024,
1025, 1115, 1116, 1280; the words, NEW ones listed; the focus after each action; the tabs' keyboard; the sweep's file;
flow 03's filing page and summary unchanged). Merged into `build-review.md` by the integrator; one fix pass (one Opus
agent). A third round only if round two finds a new defect of substance.

## For the operator (PROPOSED: answer before § 0 runs)

- **P1. Where the refusal-or-failure rule applies (S3).** R3-0-spec-2's S3 row says `RefusalText.Of` "no longer prints"
  an exception that is not a refusal. `RefusalText.Of` is shared: beside the builder's five, 25 call sites in 17 files
  of flows 03, 04 and 07–12 (`NewActivity`, `ActivityView`, `ReviewDetail`, `PanelEdit`, `InstitutionEdit`,
  `UserDetail`, `GroupMappings`, `MsfCampaignText`, …), each of which would start printing a fixed sentence in place of
  whatever its handlers throw today, untested against their boards.
  *Recommendation:* the two activity-type pages stop calling `RefusalText.Of` and classify by type themselves (§ "How":
  "Refusals and failures"); `RefusalText.Of` stays as it is; file a task for the other pages, to be done page by page
  with their own fixed sentences. Say "everywhere" to change `RefusalText.Of` itself in this build (lane C, with every
  page's tests reviewed).
- **P2. A draft wrong on two tabs.** The summary and the tab's count were drawn for one tab at a time; today
  `ActivityType.SaveDraft` stops at the first payload that fails (schema, then workflow, then credit), so a draft wrong in
  its form and its workflow names only the form, and the workflow's refusal appears on the next save.
  *Recommendation:* report every tab's problems at once: `SaveDraft` (and `PublishDraft`) parse all three payloads, run
  the two cross-checks where the payloads they need parsed, and throw one `ActivityTypeDraftRefusedException` carrying
  every place (`build-contracts.md` § 0), its message the sentences joined; a save then needs one round per mistake
  fixed, not one per tab. The Domain tests that expect a parse exception's type from `SaveDraft` change deliberately
  (the parsers themselves still throw their own types). Say "first only" to keep today's order (lane A then adds places
  only).
