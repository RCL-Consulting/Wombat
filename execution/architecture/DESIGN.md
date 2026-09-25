# Design system

This file is the visual contract for the Wombat rewrite. It exists because the first pass at T010 said "copy ClinicAssist" without enumerating what that actually means, and the current `Wombat.Web/wwwroot/app.css` is still the 37-line Blazor default — raw `<h1>` + `<table class="table">` — which is nowhere near the reference.

The design system is **ported structurally from ClinicAssist** (same token names, same component class names, same spacing scale, same layout grid) with a **new Wombat palette**. That means `t010` ships the card system, the button system, the form system, the pager, the validation summary, etc. with the same class names ClinicAssist uses, so muscle memory transfers and the Razor pages look identical in shape. Only the colours differ.

Read this once at the start of any task that renders HTML (T010, T011, T019, and any new UI task). When a task says "use `.detail-card`" it means the class defined here.

## Files that own the design system

```
src/Wombat.Web/
├── wwwroot/
│   ├── app.css                                   ← global design system (sections below)
│   ├── icons/                                    ← inline-SVG sprite files, one per icon
│   └── lib/…                                     ← bootstrap grid only, if needed; no Bootstrap components
└── Components/
    ├── Layout/
    │   ├── MainLayout.razor                      ← .page > .sidebar/main shell
    │   ├── MainLayout.razor.css                  ← sidebar width, top-row, stickiness
    │   ├── NavMenu.razor                         ← brand + AuthorizeView nav items
    │   └── NavMenu.razor.css                     ← sidebar-gradient, nav-link hover/active
    └── Shared/
        ├── Icon.razor                            ← <Icon Name="check" /> renders inline SVG
        ├── PageHeader.razor                      ← <h1> + subtitle + right-hand action slot
        ├── Breadcrumbs.razor
        ├── DataTable.razor                       ← generic list shell using .clinic-table
        ├── FormField.razor                       ← <label> + input slot + validation message
        ├── ConfirmDialog.razor
        ├── ActionResult.razor                    ← .action-result region; takes the focus when an action is done (T234)
        ├── InFlight.cs                           ← aria-disabled for a button whose own action runs (T234 review)
        ├── EpaLabel.razor                        ← "Code — Title", "(no longer in use)" muted beside it when not in force (T255)
        ├── PagerControls.razor                   ← .pager .pager-actions .pager-page-size
        ├── StatePanel.razor                      ← empty / loading / error state shells
        ├── DashboardCard.razor                   ← <DashboardCard Title=…> wraps .detail-card
        └── Skeleton.razor                        ← <div class="skeleton" />
```

`app.css` is the one global stylesheet. Component-scoped `.razor.css` files exist where a component has styles that do not belong in `app.css` (`MainLayout.razor.css`, `NavMenu.razor.css`). **No other CSS framework.** No MudBlazor, no Radzen, no Bootstrap components (the grid file is optional — `.form-grid` below is native CSS grid and does not need it).

`Components/App.razor` links every first-party stylesheet and script through `@Assets["…"]` (`href="@Assets["app.css"]"`), which resolves to the content-hashed URL (`app.<hash>.css`) that `MapStaticAssets` serves with an immutable year-long cache. An edit changes the URL, so a deploy never pairs a new page with a cached old stylesheet. A new stylesheet or script is linked the same way, never by a bare path; `Hosting/AppAssetUrlTests` fails on a bare one (T175).

## Design tokens

Defined at `:root` in `app.css`. These are the **only** colours and spacings allowed — no raw hex in component files except inside SVG data URIs.

```css
:root {
  /* ── Brand palette (Wombat) ──────────────────────────
     Refined navy/blue identity (T089). Swapping the palette means editing
     this one block — no other file encodes a raw colour. */
  --primary-color:    #2c3e50;   /* deep slate-navy ink (headings, sidebar-safe) */
  --secondary-color:  #2d6cdf;   /* action / link / focus ring — 4.86:1 white-on-button (AA) */
  --accent-color:     #3498db;   /* lighter brand blue for soft accents / highlights */
  --success-color:    #27ae60;   /* semantic, rarely tweaked */
  --danger-color:     #e74c3c;
  --warning-color:    #fd7e14;
  --info-color:       #3498db;

  /* Surfaces */
  --background-color: #f8f9fa;
  --surface-color:    #ffffff;
  --text-color:       #333333;
  --muted-text:       #6c757d;
  --link-color:       #0b5cab;
  --border-color:     #dee2e6;
  --input-border:     #ced4da;
  --shadow-color:     rgba(0, 0, 0, 0.1);
  --focus-ring:       #3498db;

  /* Semantic backgrounds (for alerts / rows) */
  --danger-bg:        #fff5f5;
  --success-bg:       #e8f5e9;
  --warning-bg:       #fff8e1;
  --info-bg:          #f3f8ff;
  --hover-bg:         #f8f9fa;
  --header-bg:        #f1f3f5;

  /* Sidebar gradient (navy → violet) */
  --sidebar-gradient-start: rgb(5, 39, 103);
  --sidebar-gradient-end:   #3a0647;

  /* Spacing scale ── use these, not raw rem values */
  --space-xs:  0.25rem;
  --space-sm:  0.5rem;
  --space-md:  1rem;
  --space-lg:  1.5rem;
  --space-xl:  2rem;
  --space-2xl: 3rem;
}
```

**Rules:**

- Add a new token before hard-coding a colour or a spacing value anywhere else.
- `primary-color` is ink / heading accent. `secondary-color` is for actions — buttons, links, focus rings. Do not mix them up.
- The sidebar gradient tokens are used by `NavMenu.razor.css` only.
- If the brand palette changes, only `:root` changes.

## Logo & brand assets (T089)

The Wombat mark is a stylised wombat face (white, on a navy→blue gradient disc) emerging from a darker
"burrow" mound. Source-of-truth SVGs live in `wwwroot/brand/`:

- `wombat-mark.svg` — the round mark, transparent corners. Used in the nav brand lockup, the login card,
  and as `favicon.svg`. This is the canonical vector logo; render it as an `<img>` (it is multi-colour, so
  it is **not** an `Icon.razor` glyph and does not use `currentColor`).
- `wombat-tile.svg` — a full-bleed, opaque square variant (same face, no transparent corners) used only to
  rasterise the OS app-icon tiles.

Raster fallbacks (generated from `wombat-tile.svg`, in `wwwroot/`): `favicon.ico` (16/32/48),
`apple-touch-icon.png` (180), `icon-192.png` / `icon-512.png` (PWA, `purpose: any maskable`). The head links
+ `site.webmanifest` + `theme-color` (`#2c3e50`) are wired in `Components/App.razor`.

**Lockup:** mark + `Wombat` wordmark, `font-weight: 700`, `letter-spacing: -0.01em`. White on the sidebar
gradient (`.navbar-brand`); `--primary-color` on light (`.account-brand`). To re-skin, regenerate the four
raster files from the tile SVG and keep the mark/tile colours in sync with `:root`.

## Typography

```css
body {
  background-color: var(--background-color);
  color: var(--text-color);
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

h1 { font-size: 1.5rem;  font-weight: 600; margin-bottom: var(--space-md); }
h2 { font-size: 1.25rem; font-weight: 600; margin-bottom: var(--space-sm); }
h3 { font-size: 1.1rem;  font-weight: 600; margin-bottom: var(--space-sm); }
h4 { font-size: 1rem;    font-weight: 600; margin-bottom: var(--space-xs); }
h5 { font-size: 0.9rem;  font-weight: 600; margin-bottom: var(--space-xs); }

.page-subtitle { font-size: 0.9rem; color: var(--muted-text); margin: var(--space-xs) 0 0; }
```

Page `<h1>` is `1.5rem`, not bigger. Pages use `PageHeader` (section below) to keep the subtitle + action slot consistent. Do not render a lone `<h1>` in a page — reach for `PageHeader`.

## Layout grid (the shell)

The whole app lives inside a two-pane flex shell defined by `MainLayout.razor` + `MainLayout.razor.css`.

```
┌───────────────────────────────────────────────────────────────┐
│ .page                                                         │
│ ┌──────────────┬─────────────────────────────────────────┐    │
│ │ .sidebar     │ main                                    │    │
│ │ (NavMenu)    │ ┌──────────────────────────────────────┐│    │
│ │ 250px        │ │ .top-row (user menu, sign-out)       ││    │
│ │ sticky       │ │ sticky, 3.5rem                       ││    │
│ │ full-height  │ ├──────────────────────────────────────┤│    │
│ │ gradient     │ │ article.content                      ││    │
│ │              │ │   @Body                              ││    │
│ │              │ │                                      ││    │
│ └──────────────┴─────────────────────────────────────────┘    │
└───────────────────────────────────────────────────────────────┘
```

- Above `641px`: flex-direction row, sidebar `250px` sticky full-height, top-row sticky.
- At or below `640.98px`: flex-direction column, nav collapses behind a `.navbar-toggler` checkbox.
- **The page has a side gutter at every width** (T226): `.top-row` and `article` pad their sides by `--space-md` (16px)
  outside any media query, and the `min-width: 641px` rule widens that to 2rem and 1.5rem. Until T226 the phone width
  had none, and a page's header and cards ran from the screen's edge to its edge (`/msf/campaigns/{id}` at 390px).
  ClinicAssist gets its gutter from Bootstrap's `px-4`, which Wombat does not load. `Design/NarrowLayoutTests` pins it.
- `top-row` right-aligns an "About" / profile / logout cluster. On narrow screens it justify-betweens.
- Below the layout, a fixed `#blazor-error-ui` banner renders on hub errors with a Reload + dismiss affordance. Copy ClinicAssist verbatim.

Copy `MainLayout.razor.css` from ClinicAssist with the class names unchanged. Only the gradient tokens differ.

## The NavMenu

- Brand row at the top: the mark (`/brand/wombat-mark.svg`, an `<img>`, § Logo & brand assets) and the `Wombat`
  wordmark.
- Nav items are `<NavLink class="nav-link">` inside `<div class="nav-item px-3">`. Home, My Account, Data Rights and
  Logout are written out; the role links come from `Sections` in `NavMenu.razor`'s `@code`, a list of (roles, links)
  rendered inside the signed-in `<AuthorizeView>` (T178). Each page is declared once there, so it has one label and one
  icon wherever it is offered.
- Active and hover states are in `NavMenu.razor.css`: `rgba(255,255,255,0.37)` for active, `rgba(255,255,255,0.1)` for hover, `#d7d7d7` default text.
- Each item's icon is the shared `<Icon Name="…" />` (§ Icons), sized and spaced by `NavMenu.razor.css`'s
  `.nav-item ::deep .icon` rule — **no Bootstrap Icons font**. A role link names its icon in its `Sections` entry.
- Logout is a `<form action="/account/logout" method="post">` with an `AntiforgeryToken`, rendered as a full-width `.nav-logout-button`.
- Mobile: a checkbox-backed `.navbar-toggler` controls visibility. No JavaScript.

The nav item list is role-driven. Each row is what that role alone sees, in menu order; `…` in the "Everyone signed in"
row is where the role's own links go. `NavMenuAuthorizationTests` renders the nav for every row and fails when the two
differ, so a change to the nav is a change to this table (T178).

| Role                 | Items |
|----------------------|-------|
| Signed out           | Home, Sign in |
| Everyone signed in   | Home, My Account, Data Rights, …, Logout |
| Trainee              | Activities, My Activities, MSF Reports, My Committee Reviews, My Progress, Export Portfolio |
| PendingTrainee       | Activities, My Activities |
| Assessor             | Activity Inbox, Recent Activities |
| Coordinator          | Data Rights Requests, MSF Campaigns, Committee Reviews, Decisions Due, Stalled Activities |
| CommitteeMember      | Programme Trainees, Decision Panels, Committee Reviews |
| SpecialityAdmin      | Programme Trainees, Decision Panels, Committee Reviews, STAR Review Queue, Decisions Due |
| SubSpecialityAdmin   | Programme Trainees, Decision Panels, Committee Reviews, STAR Review Queue, Decisions Due |
| CollegeAdmin         | Specialities, EPAs, Curricula |
| InstitutionalAdmin   | Curriculum Adoptions, EPAs, Curricula, Activity Types, Entrustment Scales, Trainees, Assessors, Invitations, Users, SSO Mappings, Audit Log, Decision Panels, Committee Reviews, Decisions Due |
| Administrator        | Colleges, EPAs, Curricula, Institutions, Invitations, Users, Activity Types, Entrustment Scales, Scheduled Jobs, SSO Mappings, Audit Log, Decision Panels, Committee Reviews, Decisions Due, Data Rights Requests, System |

Every link opens a page that admits the role offering it. `NavMenuAuthorizationTests` judges each rendered link against
its page's `[Authorize]` through the app's own policies, for every role in `WombatRoles.All`. So the Coordinator is not
offered Invitations and the speciality admins are not offered Curricula: those pages refuse them (T178).

A user holding several roles sees each of their roles' rows in the table's order, and a page an earlier row already
offered is not repeated. `NavMenuAuthorizationTests` checks that for every pair of roles and for all of them at once.
No two links share a label, so the union never shows the same name twice. That is why the trainee's own reviews are
"My Committee Reviews" beside the staff's "Committee Reviews", and the queue of other people's requests is "Data Rights
Requests" beside everyone's own "Data Rights".

Recent Activities, Stalled Activities, Programme Trainees, STAR Review Queue and System still open the
`/placeholder/{Feature}` stub. `PlaceholderPage.Headings` lists exactly the features the nav links there, so a feature
that gains a real page loses its placeholder entry when its link moves. Any other `/placeholder/…` address is Page not
found, with status 404 (`NavigationManager.NotFound()`), not a "Coming soon" for a page that exists.

Decisions Due (`/committee/decisions-due`, T131 slice 6) is offered to the roles that schedule committee reviews and to
no other: a CommitteeMember is not offered it, and its page does not admit one. `NavMenuAuthorizationTests` checks both.

MSF Reports, My Committee Reviews, My Progress and Export Portfolio sit in their own Trainee-only section, apart from
the one shared with PendingTrainee, because their pages do not admit a pending trainee (T141). A link goes in the
shared section only if its page admits PendingTrainee.

My Progress is also offered to someone who holds a trainee profile without the Trainee role (T252): completing a
programme removes the role, and a graduate still reads their own record, as their portfolio export prints it. That
section is keyed on a claim, not a role, so it is not a row of the table: `trainee_record`
(`WombatClaimTypes.TraineeRecord`), which sign-in issues to anyone holding a trainee profile, current or ended. It sits
straight after the Trainee section, so a Trainee, who holds the claim too, sees My Progress once, where their row puts
it. The page's policy, `TraineeOrFormerTrainee`, admits the same two: the role or the claim. `NavMenuAuthorizationTests`
checks that a holder of the claim alone is offered the link and admitted, and that a PendingTrainee without it is
neither. The graduate's other trainee pages (MSF Reports, My Committee Reviews) still require the role.

MSF coverage (`/msf/coverage`, T210) is not a nav item. It is a planning aid for the campaigns, so it is reached from
the MSF campaign list's header, an outline "MSF coverage" link (`#msf-coverage-link`) beside New campaign, and its own
header links back ("Back to campaigns"). The link is not offered to someone who holds Trainee, whom the page shows no
programme (`GetMsfProgrammeCoverageQuery.ShowsNoProgrammeTo`).

New items go in this table and then in `NavMenu.razor`'s `Sections`, not anywhere else.

## Button system

Class order is **`.btn .btn-sm .btn-{variant} [spacing utilities]`**. The sizing comes before the variant.

```css
.btn            /* padding .5rem 1rem, radius 6px, cursor pointer, hover filter:brightness(.95) */
.btn:focus-visible /* 2px --focus-ring outline, offset 2px */

.btn-primary    /* bg --secondary-color, white text */
.btn-success    /* bg --success-color, white text */
.btn-danger     /* bg --danger-color, white text */
.btn-outline    /* transparent bg, border+color --secondary-color */

.btn-sm         /* padding .2rem .6rem, font .8rem */
.btn-xs         /* padding .15rem .4rem, font .7rem */
```

**Rules:**

- Never use Bootstrap's `.btn-outline-primary` / `.btn-outline-success` / `.btn-outline-danger`. Use `.btn-outline` and let the tokens drive the colour.
- Row-level list actions (Edit, Delete) are `.btn .btn-sm .btn-outline`, wrapped in `<div class="actions-cell">` for a flex-gap cluster.
- The primary page action is `.btn .btn-primary` and lives in the `PageHeader` action slot.
- Destructive actions open a `ConfirmDialog` first; the red `.btn-danger` only appears inside the dialog's footer.
- A destructive row action on a list (Withdraw on the MSF campaign list, T206) is one `ConfirmDialog` for the page,
  whose body names the row it was opened from and says what cannot be undone. The row's button carries an `aria-label`
  that starts with its visible label and names the row ("Withdraw the campaign for …"): a column of identical
  "Withdraw" buttons is otherwise indistinguishable to a screen reader. It is disabled while the action is in flight.
  Its result is an `Alert` with `Role="status"` (done) or `Role="alert"` (refused) inside a `.action-result` region
  (`tabindex="-1"`), which takes the focus in `OnAfterRenderAsync`, after the dialog has closed: the row's button may be
  gone, and while the modal is open nothing outside it can take the focus (T206 review).
- The same holds on a form page whose confirmed action removes the button that opened the dialog: Deactivate on the EPA
  page is offered only on an active EPA, so once it deactivates, its result takes the focus (T196 review). A refused
  deactivation leaves its button, and the dialog hands the focus back to it.
- **A button is never disabled by its own action while that action runs** (T234; T225 review). It has the focus, and a
  browser drops the focus of a button it disables, to the page body: until T234, Save on the EPA page and the action
  buttons of 25 other pages left `document.activeElement` on BODY. The action's in-flight flag, set before its first
  `await`, makes a second press send nothing (T202); the button may say what it is doing ("Saving..."). While it runs
  the button carries `aria-disabled="true"` (`InFlight.AriaDisabled(running)`, Components/Shared; T234 review), so a
  screen reader hears that it is unavailable: a changed label on the focused button is often not read.
  `.btn[aria-disabled]` looks unavailable and keeps its focus ring. Every other button the action would race is disabled
  while it runs. Where one flag serves several buttons, the page records which one was pressed (`OtherInFlight(action)`
  and `Running(action)` on the user page, the activity type builder, the jobs list and the committee review page; the
  row's id on the invitations, SSO mappings and data-rights pages). A `ConfirmDialog`'s confirm button is held to the
  same rule while its `OnConfirm` runs, and its Cancel is disabled then.
- **An action that is done moves the focus to its result** (T234), an `Alert` in an `.action-result` region
  (`tabindex="-1"`) where the page reports the result (at the top of a form page; under the button on the rebuild page,
  under the form on the export check, under the table on the jobs list), once the page has rendered it. A status that
  arrives on its own is often not read, and what the action changed may have taken its button away (a cleared choice, a
  closed row, a form rebuilt with what was stored, a swapped button). The shared `ActionResult` component
  (Components/Shared) is the region: the page puts its result alerts inside and calls `FocusAfterRender()` when the
  action is done. The older pages (EPA, trainee profile, curriculum items, the MSF campaign pages) hand-roll the same
  region with `OnAfterRenderAsync`. An action that leaves the page (a create, a clone) moves no focus: the next page has
  it.
- **An action that is a page load has its result arrive with the page** (T265): a form the browser sends itself, whose
  answer is the page again. Change password posts to an endpoint that sends the browser back with what happened; Verify
  on the export check is a GET form. The region is still `ActionResult`, given `FocusOnLoad` when the page is that
  answer, done or refused: it renders `autofocus`, which a static page needs, and focuses itself once a circuit has
  rendered, since that render replaces the HTML the browser focused (after the router's `FocusOnNavigate` has put it on
  the heading). The export check sends `check=1` with the hash, so a check opened from a link, which nobody pressed for,
  moves no focus.
- **A refused action leaves the focus where it was**: on its button, still there and enabled, or in the field Enter was
  pressed in. The refusal is a `danger` `Alert`, `role="alert"`, read at once, and the operator's next step is usually to
  correct a field and press again. Where the refusal itself leaves the pressed button gone or disabled, its result takes
  the focus too (the user page's Reset password, whose password is cleared either way). A refusal shown beside its button
  (the curriculum item editor's Add and Save, the panel page's Save committee) is read there.
- The exceptions are deliberate: the trainee profile's Deactivate and Mark complete move the focus to their result done
  or refused (below); the MSF campaign page keeps it on Open after a refused open, for the retry, and puts it in the email
  field after an invitee is added ("The MSF campaign page", below); the curriculum item editor moves it to the Add form's
  empty state after the Add that took the last EPA (§ Card system); and a certificate download (My authorisations) keeps
  it on the button pressed: the page shows no result of its own, and the browser's download says what it did.
  `ActionFocusTests` (bUnit) holds each action in its `Scenarios` table to the rule (running, done, refused), and the
  pages with their own tests (EPA, trainee profile, curriculum items, panel, new activity) are held there. A new action
  gets a scenario. A page-load action is not one, having no button whose action runs in a circuit: its page's own tests
  hold it (`Account/ChangePasswordPageTests`, `Portfolio/VerifyExportPageTests`).
- A destructive action on a form page that acts on a field (the trainee profile's Deactivate and Mark complete, which
  each record the "Last day in the programme", T209 review) is an `.btn-outline` in the form's actions row whose
  `ConfirmDialog` names the value it will record and says it cannot be changed afterwards. Its result goes to the page's
  `.action-result` region, which takes the focus once it has answered, done or refused. A refusal's `Alert` has an `Id`
  that the field names with `aria-describedby` (after its help, `FieldHelp.DescribedBy`) while the refusal stands; a
  refusal of anything else on the page (Save) carries no such id, and the field does not name it. A date field bounded by
  "today" defaults to today on the South African calendar (`QuotaCalendar.Today(TimeProvider)`), never the server's
  `DateTime.Today`. It carries no `max`: the field sits inside the page's `EditForm`, and the browser would then refuse
  Save over a field Save does not send. The server's refusal is the rule.

## Table system

Two classes, two uses:

- `.clinic-table` — the canonical list table. Header uses `--header-bg`, rows separate with `--border-color`, hover uses `--hover-bg`. Wrap every table in `<div class="table-container shadow">` for the rounded surface + horizontal scroll on narrow screens.
  Every `<table>` is one, bar a data table only a screen reader reads (the trajectory chart's `.visually-hidden`), and
  carries no class app.css does not define: the entrustment decisions list was `class="data-table"` until T226, unstyled
  and 133px wider than the screen at 390px. `Design/NarrowLayoutTests` scans every page.
- `.table` — do not use. It renders as Bootstrap defaults. Delete every existing occurrence.

A list page is always:

```
PageHeader (title + subtitle + primary action slot)
search-container (search-grid with labelled .search-input fields)
table-container
  clinic-table
    thead (sticky, header-bg)
    tbody (rows)
PagerControls
```

`DataTable.razor` wraps this and takes column definitions via a `RenderFragment<TItem>`-per-column pattern. T010 builds the shell; T011 and T019 consume it.

**Editing a row in place** (T176, the curriculum items page). An item of a list, opened for editing, does not put its
controls in its own row's cells. A select squeezed into a column shows only a fragment of its label ("CPSA", "Cho…").
Instead:

- The item's row stays read-only, so what is stored stays in view. Its actions cell says "Editing below".
- Every control goes in a second row whose one cell spans every column (`colspan` = the header count). The cell holds
  a `<fieldset>` whose `<legend>` names the item ("Edit PAED-001"). Inside it: a `.form-grid` of `<FormField>`s, the
  same fields in the same order as the page's Add form, each with its `<label for>` and the same help text. Then a
  refusal `Alert` with `role="alert"`, and a `.form-actions` row holding the status that explains a disabled Save,
  Cancel, and Save. On the curriculum items page, `CurriculumItemsEditLayoutTests` holds the two forms to each other.
- Both rows carry `.is-editing`: a `--secondary-color` stripe down the left ties them together. They take no hover
  tint: neither is a list row while the edit is open, and tinting a whole form as the pointer crosses it is noise.
- The form row is as wide as the table, so Save sits at the table's right edge. **The read-only table must fit its
  container**, or Save is off-screen however the form is laid out.

**A row the caller cannot change** (T211). A list offers a row action only where the command behind it would accept it,
and a link only to a page that admits the caller and lets them act there. The page reads that from the query, never
from the caller's roles: each row carries the flag the command's own rule sets (`CurriculumItemDto.CanEdit`,
`CurriculumDto.CanEditCurriculum`, both from `CurriculumAdminScope`), and a page-level link is shown to those the target
page's policy admits (`IAuthorizationService` with that policy's name). A row with no action is not left blank: its
actions cell says why in a `.muted` span ("Set by the College"). Where whole groups of rows are read-only to the
caller, a standing `Alert` (`Role=""`) at the top says what they may and may not change there. An institution's own
item on a shared national list is marked under its title, `.muted .text-sm`, as the entrustment standing panel marks an
institution's own EPA. It names the owner to a caller whose reads span institutions ("Groote Schuur Hospital's own
item": an Administrator, T222), and says "Your institution's own item" to anyone else, every local item they read being
their own. The query decides which (`CurriculumItemDto.OwningInstitutionName`, cut by
`CurriculumAdminScope.NamesItemOwners`), never the page from the caller's roles.

**A column of actions that no row offers is not rendered** (T226). The committee agenda's Action column is the chair's
(T213), and it is shown while some line offers Stage, Defer or Reinstate (`ActionOn(line)`, the predicates the commands
demand). A decided or ratified review's lines offer nothing, bar a Defer on a line that keeps it from being ratified, so
the column was a row of blank cells there; now it is left out. A line with no action beside one that has is not left
blank either, as above: its cell says why in a `.muted` span. In progress that is a staged line, "Staged below", where
its decision is removed; on a decided review, "Fixed with the recorded decision" (T226 review). A cell's buttons sit in a
`div.actions-cell` inside the `<td>`, never `td.actions-cell`: a table cell made a flex box is no longer a table cell,
and its border no longer meets its row's.

**Row actions on the curriculum items page** (T222) follow § Button system: Edit and Remove are `.btn .btn-sm
.btn-outline`, named by the item's EPA (`aria-label="Edit PAED-001"`, `"Remove PAED-001"`), and, where the row names its
owner, by whose item it is (`"Edit PAED-099 (Groote Schuur Hospital's own item)"`). Since T223 two institutions may each
hold an item of their own on one EPA, and an Administrator reads both rows, so the EPA alone no longer names one button;
the edit row's legend and the Remove result name the item the same way. Remove opens the page's one
`ConfirmDialog`, whose result takes the focus in an `.action-result` region once the dialog has closed. The dialog says
whose trainees the item stops measuring: a national item's in every institution that has adopted the curriculum, an
institution's own item only that institution's, named as the row names its owner (T222 review). **An actions
column's header is a `.visually-hidden` "Actions", never an empty `<th>`**: a header names the cells under it. A column of
buttons that fits in its content's width needs no `.col-actions`; the curriculum items table has no room for its 12rem.

**Every list's row actions name their rows** (T239). Until T239, 24 pages headed their actions column with an empty
`<th>`, and most offered a column of identical "Edit", "View" or "Open" buttons. Each row's button, and each link styled
as one, carries an `aria-label` that contains its visible label, and starts with it where the sentence allows, and names
the row in the page's words: "View the 2026 S2 review before General CCC", "Open Mini-CEX for Thandi Nkosi, PAED-003,
encounter date 2026-09-01", "Edit PAED-001 — Take a history", "Run now: msf-campaign-auto-close"; an "Up" reads "Move
field Notes up" (WCAG 2.5.3 asks that the name contain the label, so a speech-input user can say what they see). A plain
link whose own text is the row's words ("Mini-CEX") needs none. Where the list's key column is unique by rule (an
institution's, a College's or a scale's name, a job's key), the page names the row by it. Where it is not (a person's
name, a panel's, a review's period), `RowNames.Distinct` (Components/Shared) adds tie-breakers only to the rows that
would otherwise share a name, each repeating a column the row shows where it can: the email, a review's type, an
activity's state. A tie-breaker that says the same of every row sharing a name is not added to them, and rows that every
tie-breaker leaves alike are numbered in list order, "(1 of 2)", so no two names on one list are ever the same (T239
review: two drafts saved in one minute, or two panels of one name, read the same until then). `ReviewRowNames` and
`ActivityRowNames` are the committee's and the activity lists' wordings. The same holds outside tables: a user's roles
("Remove the Assessor role"), a review's staged decisions ("Remove the staged decision on PAED-003"), the builder's
sections and fields ("Move field Notes up", "Add field to section Request"), and each STAR's "Download certificate for
PAED-003". The edit row's Cancel and Save on the curriculum items page name the item too. `Design/RowActionMarkupTests`
scans every page: no empty `<th>`, no `td.actions-cell`, and no button or `.btn` link in a `Row` fragment or a `tbody`
without a name of its own, which is an `aria-label` computed from the row (a fixed "Detail" names every row alike) that
contains the visible label. A column no row offers an action in is not rendered: the entrustment scales list shows its
Edit and Delete column to an Administrator only, and the decision panels list its Edit column only to a caller who may
manage one of its panels.

**A picker offers exactly what the command it feeds accepts, and is asked again after every command that changes that**
(T195, T222). The curriculum item editor's EPA pickers leave out every EPA the curriculum already holds for the same
trainees, apart from the edited item's own, because the Add and Update commands refuse it (T223): a national item holds
its EPA for every institution, and an institution's own item holds it for that institution alone, so institution B's
picker still offers an EPA that institution A has as its own item, and the College's does not. The EPA field's help says
so, and so does the College's empty state: an EPA can be missing from its picker because an institution has it as its own
item, which the College does not read, and only that institution frees it. It names no institution and no EPA. After
each Add, Save and Remove the page asks for both pickers again (the edit row's while one is open). A choice the new
answer no longer offers moves to one it does: the Add form's to the first EPA on offer, the edit row's back to the item's
stored EPA. **A refusal reads the page again too**
(T222 review): the likeliest one comes from a page that is out of date (an EPA added, or an item removed, in another tab),
so the page reads the curriculum and both pickers again and keeps the refusal. An edit row whose item has gone closes, and
its refusal moves to the `.action-result` region, which takes the focus; a refused Add that leaves nothing to add puts its
refusal at the head of the empty state (§ Card system).

**An EPA that is not in force** is marked beside its title in a `.muted` span, on the same line: "(inactive: not in
force)" on the curriculum editor, which admins read, and "(no longer in use)", the words the activity's own EPA
picker uses (`EpaOptionLabel`), on the surfaces listed here. The shared `EpaLabel` prints it: an activity's EPA cell on
My activities and the Inbox (through `ActivityEpaLabel`, T231), the rating trajectory headings on My progress and the
committee review page, and the EPA on the admin's entrustment decisions list, its revoke confirmation and My
authorisations (T255). It renders no element of its own, so a heading's text, and its accessible name, carries the
mark. A page that names an EPA passes the flag its DTO carries (`EpaInForce`); the parameter is required. No other
surface marks it yet: the committee review page's sampling concentration list, its staged STARs (which say "No longer
fits" instead) and its evidence snapshot headings (frozen at Start) still print a bare "Code — Title".

A grid whose every row is always a set of inputs, such as a scale's levels on `EntrustmentScaleEdit`, is a different
pattern and keeps its controls in the cells. Each of them still needs an accessible name; a column header does not
give one. Name each by its column and its row, in the words the page uses: `aria-label="Label, level 3"`, and a row's
buttons `aria-label="Move level 3 up"` and `"Remove level 3"` (T198). The table is
`clinic-table clinic-table--compact clinic-table--inputs`: the inputs' own padding spaces the row, and the row's text and
buttons centre on its inputs. Its rows take no hover tint, for the reason `.is-editing` rows take none. Its ordinal is
`.col-fit`, as narrow as its content. Its buttons are `.col-actions`, whose header is a `.visually-hidden` "Actions": the
column asks for 12rem, three small buttons on one line, and gets it before the inputs grow. Where the card is narrow the
buttons wrap, and the table scrolls only once they are stacked. The inputs share the rest. Not `.col-fit` for buttons:
at 1% the column shrinks to its widest button and stacks them. And not buttons held on one line: the scale editor then
scrolled sideways, with Remove out of view, at 390px and near 700 and 1000px (T198 review).

**A narrow grid of inputs stacks its buttons** (T226). The 12rem is claimed before the inputs grow, so in a narrow table
the inputs were left their headers' width: Label 39px and Description 83px on the scale editor at 390px, from 641 to
700px and from 941 to 1000px. So the grid's `.table-container` is a size container (`container: inputs-grid /
inline-size`, selected by `:has(> .clinic-table--inputs)`), and under 44rem its buttons' column is `width: 1%` after
all, the buttons stacked, and its cells' sides pad by `--space-xs`, since the inputs' own padding spaces them. A container
query, not a media query: the card's width is not the viewport's (two thirds of the row at 1000px, all of it at 900px).

**Why 44rem.** The buttons go on one line only where each input then still has the buttons' own 12rem: Label is 201px
in a 705px table, and 191px in a 688px one. The first threshold, 36rem, put them on one line at 1280px (a 581px table)
with Label 131px, too narrow for "Indirect supervision", and narrower than stacking gives at 1100px (152px). It was also
5px from the edge: a classic 15px scrollbar made the table 571px and stacked the buttons again (T226 review). At 44rem,
1280, 1366 and 1440px stack with or without a scrollbar (tables 571 to 688px, 16px or more under), and 1536 and 1920px
keep one line (742px and up, 38px or more over).

The scale editor's Label and Description inputs, measured in headless Chrome on a static copy of the page. Headless
Chrome draws no scrollbar; with a classic 15px one the table is about 10px narrower at two-column widths and 15px at
one-column widths, and each input about 5 to 9px.

| Width | Table | Buttons | Label / Description |
|---|---|---|---|
| 390px | 306px | stacked | 63 / 102 |
| 641px | 283px | stacked | 50 / 92 |
| 700px | 342px | stacked | 84 / 117 |
| 800px | 442px | stacked | 141 / 160 |
| 1000px | 395px | stacked | 114 / 140 |
| 1280px | 581px | stacked | 220 / 220 |
| 1440px | 688px | stacked | 274 / 274 |
| 1536px | 752px | one line | 227 / 227 |
| 1920px | 1008px | one line | 355 / 355 |

The rows are three buttons high below 44rem. At 641px, the narrowest two-column width, Label is still only 50px (42px
with a scrollbar): the table's narrowest is its headers' words and one button, which now fits the 283px the card has
there, where it was 11px wider. Anything wider needs a stacked layout per level, which is a redesign.

**Many columns.** A table's narrowest width is its columns' longest words plus 2 × `--space-md` of padding per column.
Nine columns spend 288px on padding alone. The curriculum items table needed 1018px against the 907px its container has
at 1280px, and nothing in it was too wide. `.clinic-table--compact` halves the cell padding (a spanning cell keeps the
full padding, because it holds a form). That brought the table to 874px. Reach for it on a table of eight or more
columns, and on a grid of inputs.

A column of long text (a title, a list of names) is `.col-wrap`, on its header and every one of its cells. When the table
is short of room it gives way first: its lines wrap, then a long word breaks inside itself (`overflow-wrap: anywhere`),
down to a floor, and only then does the table scroll sideways. So its longest word no longer sets the table's narrowest
width; the floor does. The header sets the floor, 4rem of text (5rem with `.col-wrap--wide`). Without one the column
fell to a letter a line, 30px wide at 1024px (T198 review). The header also asks for a share of the table, 14% (20%
with `--wide`). A share, not a length: a length would hold the column there on a wide screen while every other column
grew.

That is a trade, not a free gain. A static render of the curriculum items table (the fifteen v11.1 items; T198), whose
EPA column is `--wide` and whose Tools column is not:
- Its narrowest width is 794px, down from 883px, so it has 113px to spare at 1280px (a 907px container), up from 24 in
  the same render (T176 measured 33 on the live page).
- No word breaks at 1280px, but with 5px to spare: the EPA column is 168px, and "neurodevelopmental" needs 163. Below
  about 1272px that word breaks, where the old table fitted down to about 1257px and then scrolled.
- By about 1168px both columns are at their floors (96px and 80px with padding), and many words are broken. Below that
  the table scrolls.
- At 1440px every column is within 10px of its old width. At 1920px the EPA and Tools columns are narrower than they
  were, 309px and 216px against 374px and 276px: they hold their share, and the other columns take the rest.

Measure a table at 1280px against the longest real values before adding a column.

```css
.clinic-table--compact /* cell padding --space-sm; a td[colspan] keeps --space-md (T176) */
.clinic-table--inputs  /* a grid whose every row is a set of inputs: cells centre vertically (T198) */
.is-editing            /* on a clinic-table row: the item open for editing and its form row below (T176) */
.col-wrap              /* th and td of a long-text column: overflow-wrap anywhere; the th asks for 14%, floor 4rem (T198) */
.col-wrap--wide        /* with .col-wrap on the th: asks for 20%, floor 5rem (T198) */
.col-fit               /* th and td of a column as narrow as its content, on one line: an ordinal, a code (T198) */
.col-actions           /* th and td of a row's buttons: asks for 12rem, one line of three; they wrap when short (T198) */
@container inputs-grid /* under 44rem, a .clinic-table--inputs stacks its buttons (width 1%) and pads cells by xs (T226) */
```

No inline `style="width:…"` on a table's cells. A column's width is one of the classes above.

A table **grouped by one column** puts each group in its own `<tbody>`, and the group's first row opens with
`<th scope="rowgroup" rowspan="n">` naming it; `app.css` aligns that label to the top of its group. No new class. The
committee review's evidence snapshot is one table per EPA, grouped by instrument (T167). A card that holds such a table
is an `article.detail-card--compact` whose `<h4>` it names with `aria-labelledby`.

## Form system

```css
.form-container   /* surface card with padding, border, shadow */
.form-grid        /* grid-template-columns: repeat(auto-fit, minmax(min(250px, 100%), 1fr)); gap 1.5rem */
.form-grid--wide  /* minmax(min(350px, 100%), 1fr) — wide sections that must not truncate */
.form-group       /* flex-column, gap .5rem, label on top of input */
.full-width       /* grid-column: 1 / -1 */

.form-control     /* padding .75rem, border --input-border, radius 6px */
.form-select      /* native <select> styled with a chevron data-URI */
.form-select-sm   /* compact variant */
.form-control.invalid, .form-select.invalid, textarea.invalid, …[aria-invalid="true"]
                  /* an invalid control: --danger-color border + a 4px left stripe (T236; with .input-validation-error) */

.form-actions     /* flex, justify-end, gap .75rem, padded-top, top border */
.scale-choices    /* one radio per line for a rating scale's points, lowest first (T205) */
.workflow-action-reasons /* list under a workflow action row: why a disabled action cannot be taken (T107) */
.stage-minima     /* two-column grid: training year | rung picker, one row per year (T125 curriculum minima) */
```

**Rules:**

- Every form is inside a `.form-container`. Every form's submit/cancel cluster is a `.form-actions` row at the bottom.
- **An auto-fit grid's column asks for its width or the whole container, whichever is less**: `minmax(min(350px,
  100%), 1fr)`, never a bare `minmax(350px, 1fr)` (T226). A bare one is 350px in a container with less room: the scale
  editor's Name input stuck out of its card at 390px, and out of the one-third column of `.details-grid` at every width
  that has two columns, and the EPA page's fields ran 48px past its form's card at 641px. The same holds for
  `.search-grid`, `.check-grid` and `.dashboard-grid`; `Design/NarrowLayoutTests` scans app.css for any other.
- `<FormField>` wraps a `<label for="…">` + input slot + `.validation-message` target. Its help text carries the id
  `FieldHelp.Id(InputId)` (`{InputId}-help`). The input in the slot is the caller's markup, which a component cannot
  change, so **a caller that gives a field `HelpText` names it on the input**:
  `aria-describedby="@FieldHelp.DescribedBy("x", helpText)"`, or `@FieldHelp.Id("x")` when the help is constant.
  `DescribedBy` lists the help first, then any other region that describes the input (a warning, a refusal), and is
  null when there is nothing, so the attribute is left off. `FieldHelp` is its own class because every page also imports
  the schema's `FormField`. A literal id passed to it is the field's own `InputId`. `FieldHelp.Id` keeps only
  characters an id list can carry (`FieldHelp.IdPart`), because an activity form's input id is a free-text field key;
  build any other id that is named in an id list from a key the same way. `FormFieldHelpTextLinkTests` scans every
  caller (T193; T205 did the MSF comment boxes).
- **Help goes in `HelpText`, never into the slot.** A bare `<small class="muted">` or `<p class="form-hint">` under the
  input is read by no screen reader. A line that does belong in the slot (the curriculum editor's "Suggested because …")
  has an id, and the control names it after its help. A group's help (a `<fieldset>` of checkboxes or rows) has an id
  the fieldset names. The same scan fails on help written into a slot (T193).
- Inputs default to `.form-control`. Selects use `.form-select` (never native unstyled).
- **An option of a `<select multiple>` rendered from data is keyed by what it offers**: `<option @key="candidate.UserId"
  value="@candidate.UserId" selected="@isSelected">` (T257). Unkeyed, Blazor reuses the option elements by position when
  the list changes (the panel form's Members list leaves out the chair), and a browser ignores a change to the `selected`
  attribute of an option the user has clicked, so the element reused for the next person shows them selected when the
  form does not hold them, and the next click sends them. `Design/MultiSelectOptionKeyTests` scans every page.
- Validation summaries render as `.validation-summary-errors` (red panel) at the top of the form. Per-field errors render as `.validation-message` under the field.
- **An invalid field shows it on the control**, not only in the message under it (T236). Blazor's inputs carry `.invalid`
  and `aria-invalid="true"` while the `EditContext` holds a message for them. A control a page marks by hand, where the
  page can tell the server will refuse what is typed (T192's encounter date, T125's rung picker, T205's MSF comment),
  carries `.input-validation-error` and `aria-invalid="true"`. On a `.form-control`, a `.form-select` or a `textarea`,
  `app.css` gives every one of them a `--danger-color` border and a 4px stripe down the left edge: the 1px border and a
  3px inset shadow. The stripe is the cue that is not colour (WCAG 1.4.1); a shadow, not a wider border, so the text does
  not move as a field turns invalid while it is typed in. In a contrast theme (`@media (forced-colors: active)`), which
  drops shadows and paints every border one colour, the stripe is a 4px left border instead. The rule comes after
  `.form-control` and `.form-select`, which are as specific as `.input-validation-error`. A checkbox or radio is not
  marked, since a native one takes no border; its message says it. There is no field CSS class provider: Blazor's own
  class names are the ones styled, so a page marks a field by giving it a validation message, and a hand-made control by
  the two markers above, never by a style of its own. `Design/InvalidFieldStyleTests` pins the rule, and its
  `ShowsInvalid(element)` matches a rendered control against the rule's own selectors, which a page test uses to show a
  refused field is one the rule styles (the agenda deferral reason, the activity form's encounter date).
- **A field the server refused is marked, not only named** (T263). An activity form's refusal names each field by its
  label in words, and carries the fields' schema keys beside the message (`ActivityFieldsRefusedException.FieldKeys`;
  CUSTOMIZATION.md § Keys are for logic). The page hands them to `ActivityForm` as `RefusedFieldKeys`, with the id of the
  `Alert` that shows the refusal as `RefusalId`. Each control named takes the two markers above and names the alert in
  its `aria-describedby`, after its help text and any notice (`FieldHelp.DescribedBy`). A multi-choice group's fieldset
  names the alert, and its checkboxes are not marked. A locked field the refusal names is marked too, so the actor sees
  which field stopped the move. The page holds the keys with the alert and clears both on its next action, so a field
  is marked exactly while the alert that names it is shown. The three places are `/activities/new`'s refused create
  (`NewActivity.RefusalAlertId`), a refused move on the activity's page (`ActivityView.ActionRefusalAlertId`) and the
  notice of a submit refused straight after the create (`ActivityView.NoticeAlertId`, the keys riding on
  `ActivityNotice.RefusedFieldKeys`). `Activities/RefusedFieldMarkingTests` holds all three.
- Multi-step forms get `<fieldset>` with a styled `<legend>` — both reset in the CSS.
- Checkbox: `<div class="form-check">` wrapping a `.form-check-input` + `<label>`.
- A group of checkboxes is a `<fieldset>` with a `<legend>`, the checkboxes inside a `.check-grid` (columns of
  `.form-check`), every checkbox with its own unique id. **Not** a `<FormField>`: its `<label for>` would point at
  no single input. When two groups share a page (an inline edit row and an Add form), prefix their ids differently
  (`edit-tool-{key}` / `add-tool-{key}`, T122's curriculum tool list).
- In `ActivityForm`, a field that is not one control is a `fieldset.form-group` among the section's `<FormField>`s:
  a multi-choice field, and a field with no control to name (the file placeholder, an unsupported type) (T177).
  `.form-group` makes its legend read as that field's label, not as the section title. Help text goes inside the
  fieldset, which names it with `aria-describedby`. The checkbox groups on the MSF campaign form (T147) and the
  curriculum item forms (T122) are plain fieldsets with a section-size legend.
- An option is shown by its label, never by the key it stores (T191): `<option value="picu">PICU</option>`, and a
  checkbox's id is built from the key, checking it stores the key, and its `<label>` shows the words. A schema option
  is a bare string only when the string reads as words ("1"); a key is written
  `{ "value": "admission_notes", "label": "Admission notes" }`, and the builder's Options box takes it as
  `admission_notes | Admission notes`. The box holds one option per line, and a comma is part of the option, never a
  separator. The portfolio export prints the label too, except on a scale, where the rung the College prints comes
  first (T100). A field that stores an id prints as what the id names, never as the id (T199,
  `PortfolioFieldReferences`): an `epa` field as its picker labels it ("PAED-003 — Title", "(no longer in use)" after
  one not in force), a `user` field as the person's name (never their address; "Unknown person" when the account is
  gone), and a campaign's evidence record's `campaign_id` as the MSF section of the same export heads that campaign,
  "Annual MSF (Campaign #3)".
- A rating scale answered by choosing one point is a `fieldset.form-group` whose `<legend>` is the question (with the
  required `*`), holding a `.scale-choices` list of `.form-check` rows: one radio each, lowest point first, each with its
  own id and `<label for>`, a point's description as a `<small>` in its label (T205, the MSF questionnaire). A list, not
  a `<select>`: every point's label stays in view. Not a `.check-grid`: an ordered scale reads down, not in columns.
- Sensitive inputs (password, passphrase): wrap in `.password-wrapper` and use `PasswordToggleButton.razor` to show/hide.
- A workflow action the actor may take but cannot complete (T107) is a **disabled** button, never a hidden one. Its
  reason is visible text in a `.workflow-action-reasons` list below the row, starting with the action's name, and the
  button points at it with `aria-describedby`. A tooltip alone is not enough.
- An action that can be taken but has a consequence worth knowing first (Start review while MSF campaigns from the
  window are unreleased, T173) says so in a `<span class="muted">` beside the button in its `.form-actions` row, and
  the button points at it with `aria-describedby`. Say it at the button even when a notice elsewhere on the page says
  it too: on a narrow screen the notice can be several cards away.

## Card system

```css
.detail-card                  /* surface, padding var(--space-lg), border+shadow */
.detail-card--compact         /* padding var(--space-md) */
.detail-card--header          /* padded header strip only */
.detail-card--interactive     /* hover: translateY(-2px), bigger shadow, cursor pointer */
.detail-card--empty           /* dashed border, centred muted text, var(--space-xl) */
.detail-card--empty-compact   /* dashed border, centred, var(--space-md) */
.detail-card--empty-compact[tabindex]:focus-visible /* focus ring, for an empty state the page moves the focus to (T222) */
.detail-card--emphasis        /* left 4px solid accent stripe, --secondary-color */
.detail-card--warning         /* left 4px solid accent stripe, --warning-color */
```

- `.detail-card h3` has a bottom border and primary colour — gives the card a titled strip.
- Cards are the building block of dashboards and detail pages. Anywhere you want to group fields inside a page, reach for a card.
- The empty-state card uses a dashed border so it reads as "nothing yet" at a glance.
- **A form with nothing left to offer is replaced by its empty state** (T222). When a form's one required picker would
  be empty (the curriculum item editor's Add form, once every EPA the item could name is already on the curriculum), the
  form is not rendered with an empty select and a live submit button. Under the form's heading, a
  `div.detail-card.detail-card--empty-compact` says that nothing is left, in the words of what the form would have made,
  and what would have to change for there to be something. It has `tabindex="-1"`: when the action that emptied the
  picker took its own button away with the form (the last Add), the page moves the focus to it in `OnAfterRenderAsync`,
  and it starts with that action's result ("Curriculum item added.", or a refusal in `.text-danger`), so the focus reads
  the result out where the operator is. A status alert that arrives already filled just as the focus moves may not be
  read at all (T222 review). The page's usual result alert still shows at the top.

## Dashboard layout grid

Dashboards use one shared grid so every role page looks like the same product.

```css
.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(320px, 100%), 1fr));
  gap: var(--space-lg);
  align-items: start;
}

.dashboard-span-2 { grid-column: span 2; }
.dashboard-span-3 { grid-column: span 3; }
```

Dashboard widget classes (added in T011):

```css
.dashboard-metric         /* flex column, gap xs */
.dashboard-metric-value   /* 2rem bold, --primary-color */
.dashboard-metric-label   /* 0.9rem, --muted-text */

.progress-bar             /* 0.5rem tall, rounded, --hover-bg background */
.progress-bar-fill        /* fills parent height, --secondary-color */
.progress-bar-fill.is-complete  /* --success-color */

.progress-row             /* one titled figure + bar + meta line; margin-bottom sm (T130) */
.progress-row-head        /* flex, space-between, baseline: label left, "n of m" right */
.progress-row-meta        /* muted, 0.85rem, margin-top xs: the explanatory line under a figure */
.progress-group-title     /* heading that splits a page of progress cards into groups ("Each semester", "Once a year") */
```

`.progress-row-*` was promoted in T130 from the inline `display:flex;justify-content:space-between` row that had spread
across six progress surfaces, past the four-dashboard threshold below. Use it for any "label · n of m · bar" figure.
Give every `.progress-bar` `role="progressbar"` with `aria-valuemin`, `aria-valuemax`, `aria-valuenow` and an
`aria-label` that states the figure in words. A bar is decoration without them.

Curriculum progress figures (T130) are always **a count against a target for a named window**: "2 of 3 this semester",
"1 of 1 in 2026", "4 of 9 trainees met". Never a lifetime total, and never a mean percentage across trainees. Say
"training year N" for `TraineeProfile.GetStage` and "Semester S, YYYY" / "YYYY academic year" for a quota window:
the two are different concepts (D17), and the bare word "year" beside both is ambiguous. When a target is waived
under the College's D14 rule, show the count and the date targets start, never a fraction and bar. When the programme's
end waives it (D49, T209: it ended in the window before the window's last month, or the window is after the end), show
the count and why ("the programme ended part-way through", "after the programme ended"), never a fraction, a bar, "short"
or "targets start with": the targets did not start later, they stopped.

The three staff dashboards' target cards (CommitteeMember, SpecialityAdmin and SubSpecialityAdmin, one
`EpaTargetCoverageList`) count the same trainees: the current trainees in the caller's scope
(`TraineeScopeResolver.KeepCurrentAsync`, T238), an active profile on an account that still holds Trainee and that an
administrator has not locked (T268). So an erased trainee's pseudonym, a profile that outlived its Trainee role and a
locked trainee are in no "n of m", and the admins' "Trainees in programme" tile counts as active the trainees their card
reads. None of them is counted as inactive either; the admin trainees list is where such a profile is seen, and ended.

Each dashboard card is a `<DashboardCard>` — a shared component that wraps `.detail-card` and adds `Title`, `Icon` (Lucide name), `Href` (turns it into `.detail-card--interactive`), `Emphasis` / `Warning` (left stripe variants), and `Span` (1/2/3, the `.dashboard-span-*` modifiers). Reach for `<DashboardCard>` first; drop to raw `<div class="detail-card">` only when the card does not have a titled strip. Below `~900px` the `.dashboard-grid` auto-fit collapses everything to a single column.

T011 mandates this grid for every role dashboard — do not hand-roll a different one per role.

## Builder layout

T019 introduces a small builder-specific extension to the shared system:

```css
.tab-bar          /* horizontal tab strip with pill-like tab buttons */
.tab-bar-tab      /* inactive tab button */
.tab-bar-tab.is-active

.builder-two-col  /* admin builder split view: editor rail + live preview */
```

- `.tab-bar` sits directly below `PageHeader` inside the surrounding `.form-container`.
- `.builder-two-col` is `grid-template-columns: minmax(320px, 1fr) minmax(400px, 1.4fr)` with the normal `var(--space-lg)` gap and collapses to a single column below `900px`.
- The left column uses stacked `.detail-card` sections and field rows. The right column is always the live preview rendered by the shared `ActivityForm.razor`.
- New builder affordances still reuse the existing button, card, form, alert, and validation classes. The builder does not get its own parallel design language.

## Alerts, validation, empty states

```css
.alert                       /* padding 1rem, radius 6px, margin-bottom 1.5rem */
.alert-danger                /* bg --danger-bg, text --danger-color, border --danger-color */
.alert-success               /* bg --success-bg */
.alert-warning               /* bg --warning-bg */
.alert-info                  /* bg --info-bg */

.validation-message          /* inline field error, --danger-color, .85rem */
.validation-summary-errors   /* form-level error panel */
.input-validation-error      /* a hand-marked invalid input: --danger-color border + 4px left stripe, as Blazor's .invalid (§ Form system, T236) */
.field-warning               /* inline NON-blocking warning under a field, input accepted as typed: --warning-bg, --warning-color left stripe, body text, .85rem (T160 late filing) */
```

- `<Alert>` is announced by its kind unless the caller names a `Role` (T193). `danger` is `role="alert"`, so a refusal
  that appears after Submit is read at once; `warning` and `success` are `role="status"`, because nearly every one
  reports what an action did; `info` has none. Name one where the kind's default is wrong:
  - a refusal shown as a warning is `Role="alert"` (the MSF page's refused link; the activity page's refused-submit
    notice, which arrives with the page already filled, where a `status` is often not read);
  - a warning or success that is **standing page content**, there on every visit (a banner, "credited nothing", a
    suppressed category), is `Role=""`, which renders none, or it is read out each time the page finishes loading.
  - Never hand-write `<div class="alert …">`: it skips the default.
- **A refusal is shown in its own words** (T213). A page puts a caught exception in its error `Alert` through
  `RefusalText.Of(exception)` (Components/Shared), never `exception.Message` directly: a command's validator refuses
  with a FluentValidation `ValidationException`, whose message is written for a log ("Validation failed: -- Members: …
  Severity: Error"), and the helper shows each failure's message once instead. Every other refusal's message is a
  sentence already and is shown as it is. The committee pages use it throughout; other pages still print
  `exception.Message` and move to it when next touched.
- **An alert already on the page when it loads is not reliably announced**, whatever its role. When a page reloads with
  a refusal and puts focus in a field (sign-in, link account), give the `Alert` an `Id` and have the field name it with
  `aria-describedby`, so the refusal is read with the field. A refusal that names fields gives its `Alert` an `Id` the
  same way, and every field it names points at it (the activity pages, T263, § Form system).
- `StatePanel.razor` renders three canonical states: loading (skeletons), error (`.alert .alert-danger`), empty (`.detail-card--empty` + optional CTA).
- Every list page handles all three states explicitly. **No more "Loading…" plain text** — that pattern is dead.
- A field's warning and its predicted refusal never show together. When the page can tell the server will refuse what is
  typed (an encounter date before the trainee's programme started, T192), the field says so as a `.validation-message`
  in place of any `.field-warning`, whose "can still be filed" would contradict it. Both sit in one `role="status"`
  region under the field, present before anything is typed, which the input names with `aria-describedby`, after its
  help text (`FieldHelp.DescribedBy`, § Form system). While the refusal is predicted the input also carries
  `aria-invalid="true"` and `.input-validation-error`; a warning alone marks nothing, since what it warns of is
  accepted. It is a hint: the server's refusal stays the rule. A refusal the page did not predict counts the same
  (T263): while a refusal on show names the field, the `.field-warning` is left out, since the page's start date was
  missing or out of date and the alert says why the date was not accepted.

## Skeleton loaders

```css
.skeleton {
  background: linear-gradient(90deg, var(--hover-bg), var(--border-color), var(--hover-bg));
  background-size: 200% 100%;
  animation: skeleton-pulse 1.2s ease-in-out infinite;
  border-radius: 4px;
  min-height: 1rem;
}

@keyframes skeleton-pulse {
  0%   { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
```

`Skeleton.razor` renders `<div class="skeleton" style="width:@Width;height:@Height">`. Dashboards and list pages render a handful of skeletons while `IScopedSender.Send(...)` resolves. The viewport should not shift when real data lands.

## Badges

```css
.badge           /* inline pill, 0.75rem, bold, 999px radius */
.badge-draft     /* --hover-bg bg, --muted-text text */
.badge-submitted /* --info-bg bg, --secondary-color text */
.badge-accepted  /* --warning-bg bg, --warning-color text */
.badge-completed /* --success-bg bg, --success-color text */
.badge-declined  /* --danger-bg bg, --danger-color text */
```

Used on activity state indicators in dashboard list cards and activity tables. The class is the state's key and the
text is its label (T220): `<span class="badge badge-submitted">Awaiting supervisor</span>`.

A status that is not an activity state is mapped onto these five in C#, never written as `badge-{status}`: app.css
defines no other. The entrustment decisions list did that, and all four of its STAR statuses were untinted pills (T226
review). There, Active is `badge-completed`, Expired `badge-accepted` (the EPA wants deciding again), Revoked
`badge-declined` and Superseded `badge-draft`.

```css
.badge-standing-met    /* --success-bg ground, --success-color border, body text (T166) */
.badge-standing-below  /* --warning-bg ground, --warning-color border, body text */
.badge-standing-none   /* --hover-bg ground, --border-color border, body text: no decision, or not comparable */
```

A verdict of a level against a target: `EntrustmentStandingPanel`'s "At or above", "Below", "No decision" and "Not
comparable" (T166). The words are the verdict and the tint repeats it. The text is body colour, not the semantic
colour on its own tint, which falls short of 4.5:1 at 0.75rem. Every badge above is a state; these are the only ones
that are a comparison.

**Entrustment standing** (T166). `Components/Shared/EntrustmentStandingPanel.razor` is the one rendering of
`GetEntrustmentStandingForTraineeQuery`. The committee review page shows it as a full-width card (`.detail-card
.full-width`) after its first pair of cards. The trainee's My progress page shows it as a section with its own `<h2>`,
and says it to "you" (`Self`). It opens with a sentence naming the training year and a `details-list` of two lines:
the year-target counts, and the exit rule counted by exit level with the EPAs short of it named. Then a
`.clinic-table--compact` of six columns: EPA, year target, STAR decision, against target, exit level, latest rating.
A level on another ladder, or on an unpinned item, is "Not comparable" with the reason under it, never a verdict. The
panel says the exit rule gates nothing, directly under the rule. Each page loads the panel in its own `try`, so a
failure shows in the panel and never replaces the page.

What the panel must not claim. On the review page the year is read for the review period's last day once that has
passed (`ReviewPeriodTo`), and the opening sentence says so and that decisions are today's: a review held after its
period judges that period's year, not the next one. On the trainee's own page once their programme has ended, the year is
read for its last day (`ProgrammeEnd`), and the sentence says the programme ended, or was completed, then, in that year,
rather than "you are in" a year the start date would put them in today (T252). A profile ended before Wombat recorded the
day (T209) is read on today, and the sentence says the day was not recorded and the year is counted from the start date
to today, never "you are in" it. A target the curriculum sets no level for (no per-stage map, or a
year past the ones it names) is the exit level standing in, and its cell says "Exit level; no year N level set" rather
than passing it off as Annexure A's. An institution's own EPA is a row, marked under its title, but the exit rule is
the College's and counts only the College's EPAs. For someone else, "nothing to show" does not say "no curriculum":
the same null is what a caller outside the trainee's oversight gets. The exit rule is one sentence built in C#, because
Razor drops a space standing alone before an expression and ran the sentences together.

**Multi-source feedback coverage** (T168). `Components/Shared/MsfCoveragePanel.razor` renders
`GetMsfCoverageForTraineeQuery` on the committee review page. It is a full-width card directly after the standing card,
wherever that sits, so the pair of cards below keeps its pair. It has three parts:

- An opening built as one C# string. It says what covered means, that MSF counts towards no target, and that it is read
  live beside a frozen snapshot, in the review's own terms: on a Scheduled review, that nothing is frozen yet; in
  progress, that a campaign released since Start is not in the snapshot; once decided (or a formative review closed),
  that the card may differ from what the panel saw. Only when the review period starts or ends inside a semester does
  it add that a semester is read whole.
- A `details-list` giving each semester's "n of m EPAs covered".
- A `.clinic-table--compact` with each EPA as a row header (`th scope="row"`) and one column per semester. A cell says
  "Covered", with the campaign and the day it closed, or "None released" in muted text ("None released yet" while the
  semester runs).

It takes no badge and no warning tint. Coverage is not a verdict against a target, and there is no shortfall to show
until Annexure B's cadence is confirmed. The trainee's My progress page says the same thing as one line per quota card,
newest semester first, and gives this semester's count in the "This period" card. Both pages use the words
`MsfCoverageText` builds. Every sentence says a released campaign *covering* the EPA closed in the semester, never a
campaign "about" it (D9). An uncovered semester that has ended is never worded as final ("…that closed in the semester
has been released"): a campaign is placed by the day it closed, so one closed in June and released in July covers
semester 1 only from its release.

**My progress once the programme has ended** (T252). A trainee with no current programme, completed or withdrawn, sees
the one they ended on, read as on its last day, never "No curriculum items assigned yet". The page opens with an
`<Alert Kind="info">` (standing content, so no role) saying that the programme ended and on which day, "You completed
your programme on …" or "Your programme ended on …", that the page is their record and read-only, and D49's rule, as the
PDF states it. The "This period" card becomes "Your programme": started, completed or ended, and the training year it
ended in. Each quota card keeps its heading and target line, and lists every period in a `details-list`, newest first,
from the one it ended in back to the start: "Semester 2, 2026: no target (your programme ended part-way through) · 1
recorded", "Semester 1, 2026: 3 of 3, met; 2 at the minimum level when observed". A period not yet closed by today
(`QuotaWindowDto.HasClosedBy`, through 31 December for semester 2, D40) is "2 of 3 so far", or "3 of 3 so far, met",
never "more by": nothing is owed. There is no bar, no D14 alert and no December notice. The period lines are the
portfolio PDF's (`PeriodLine`, which reads the same closing rule), in the page's "your", and each card keeps its MSF
line; a coverage read that fails says so above the cards, as on the running view. The opening sentence ("You completed
your programme on …") is `QuotaText.ProgrammeEnded`, which the standing panel and the trainee dashboard's Curriculum
targets card also use; that card says the programme ended and points here.

**The programme's MSF coverage** (T210, `/msf/coverage`, Coordinator and Administrator, as the campaign pages). Per
programme, EPA and semester, how many of the programme's trainees were covered: "1 of 3 trainees covered". A programme
is a curriculum as one institution follows it (`MsfProgrammeCoverageText.ProgrammeName`, "Paediatric EPA Curriculum 11.1
at Demo Institution"), so an Administrator sees one per institution. The trainees are those the caller may read about
(`TraineeScopeResolver.ReadableAsync`, the set form of T113's ladder) who are on the programme now: current trainees
(`TraineeScopeResolver.WhichAreCurrentAsync`, T238), an active profile on an account that still holds Trainee and that
an administrator has not locked (T268), so an erased trainee's pseudonym, a profile that outlived its Trainee role and a
locked trainee are not counted. The semesters are today's and the one before, as on the trainee's progress page. Each
trainee is counted from their own card: the counts are read by `MsfSemesterCoverage`, the one rule
`GetMsfCoverageForTraineeQuery` reads too. The page holds:

- A `.muted` opening (`#msf-coverage-intro`, one C# string): what covered means in D9's words, that each trainee is
  counted from their own progress page, that MSF counts towards no target so an uncovered trainee is no shortfall, that a
  trainee counts in a semester once their programme has started by its last day, and that an ended semester can still
  gain a campaign released later.
- One `section.detail-card` a programme, `aria-labelledby` its `<h2>`. A `details-list` gives each semester's trainees
  ("3 trainees, whose programme had started by 30 June 2026"). Then two tables, each under an `<h3>` ("By EPA", "By
  trainee"), each in a `.table-container` that is `tabindex="0"` and `role="region"`, as the decisions-due summary is,
  since neither holds anything focusable. Its `aria-labelledby` names its `<h3>` and then the programme's `<h2>`, so that
  where several programmes render (an Administrator's view, or an institution running two curricula) each region is
  named for whose it is ("By EPA Paediatric EPA Curriculum 11.1 at Demo Institution"), not one of several "By EPA"s.
  Both are `.clinic-table--compact` with a row header per row and a column per semester.
  - **By EPA**: "n of m trainees covered", bold when n is above 0 and muted when it is 0; "No trainee had started" where
    m is 0, never "0 of 0". An institution's own EPA is marked under its title, as on the trainee's grid.
  - **By trainee**: "6 of 15 EPAs covered", the trainee's own count for the semester (for the current one, the count
    their My progress "This period" line gives), or "Not started".
- Like the trainee's grid it takes no badge, no warning tint and no bar: nothing here is a verdict against a target.
- Someone who holds Trainee (T185's rung) reads no other trainee's record: the page opens with a standing `Alert`
  (`Kind="warning"`, `Role=""`, `#msf-coverage-trainee`) saying so and that their own coverage is on My progress, and asks
  for nothing. Nobody to show is the `StatePanel`'s empty card; a failed read is its error.

**The MSF campaign page** (T217, `/msf/campaigns/{id}`). Its campaign card shows the campaign as it is now and offers
only what its state allows. It is read again after every action on the page, refused or not: a refusal is likeliest
because the campaign changed elsewhere. The Quick template card is only on the create page, `/msf/campaigns/new`: a
campaign that exists already has its questionnaire, and its page reads no template list (T225). The card holds:

- A `details-list`: Trainee (by name), Template (with its kind), State (a badge) and Response window. Then one sentence
  saying what the state means and what comes next: an open campaign names its respondents' last day to respond and says
  that Close campaign is on its report. It says each respondent "is emailed" a link, never "has been emailed": the mail
  leaves after the open, and whether it arrived is what the next lines say (T251).
- **What became of the links, on an open campaign, counted and never whose** (T251). The mail worker reports each link's
  mail sent or dropped onto its invitation, and a link nothing was heard of for an hour (the host crashed, or stopped
  mid-send) counts as dropped (`MsfInvitation.LinkNotDelivered`, `LinkBeingSent`). Links not delivered are a `warning` `Alert`
  (`#msf-links-not-delivered`, `Role=""`: standing content, there on every visit until they are resent) reading "2 links
  were not delivered. Resend sends each of these respondents a new link; this page never says who they are."
  (`MsfCampaignText.LinksNotDelivered`). Links still being sent are a `.muted` line (`#msf-links-being-sent`) that says
  how many and to reload the page to see whether they arrived. A number, never a row or an address, for the reason the
  invitees are counted by group: an undelivered respondent is one who has not answered. Nothing is shown when every link
  was sent, and nothing on any other state.
- **Invitees, counted by respondent group, never listed once the campaign has opened.** An `<h4>`, a `.muted` line
  saying the page never lists who was invited or which of them responded ("Once the campaign opens, …" on a draft), and
  promising nothing more: a campaign whose category threshold is one shows a one-person group's answers on its report.
  Then a `.clinic-table` with a row header per group (by its label, "Peer doctor", never the key) and Invited, then
  Responded once the campaign has opened, with an "All groups" `tfoot` row when there are two groups or more. Not a row
  per invitee, even without its address: the coordinator added the rows and knows which is whom, so a responded mark on
  one row names the author of the comment that has just appeared on the report. A group's count adds little to the
  report, which already shows the total and a card for each group that has responded. Until release it does add the
  exact count of a group below the category threshold, which the report hides; that names nobody, and with one such group
  it is already the total less the others. (T217 review)
- **A draft's addresses, for Remove** (T247). On a draft that invites anyone, an `<h4>` "Addresses invited", a `.muted`
  line saying they are listed only while the campaign is a draft, and a `.clinic-table` with a row per invitee in the
  order they were added: the address as the row header (`.col-wrap`, so a long one wraps rather than widening the
  table), the group by its label, the teaching context on learner feedback, and a `.visually-hidden` "Actions" header
  over each row's Remove (`.btn .btn-sm .btn-outline`, `aria-label` "Remove peer-1@example.test (Peer doctor)"). This is
  not the row per invitee the counts refuse to be: a draft's invitees hold no working link and have given no response,
  so a row says nothing about anyone's answers. It does show the addresses to whoever runs the campaign, any coordinator
  at the trainee's institution, not only the one who typed them; that is accepted (T247 review). The query lists them
  only while the campaign row is a draft (`MsfCampaignSetupDto.DraftInvitees`), and the page asks the state too; once
  the campaign opens, the list is gone. Remove opens the page's second `ConfirmDialog`, which names the invitee, says
  that they hold no working link and will not be sent one, and that they can be added again while the campaign is a
  draft. "No working link", never "no link": an open that failed or was refused at its save may have mailed one. Its result, done
  (`Role="status"`) or refused (`Role="alert"`), takes the focus in the `.action-result` region once the dialog has
  closed: the row's button is gone after a remove, and after a refusal the campaign is read again and may have opened
  elsewhere. The counts and the list are read again with it, so a group loses the invitee at once, and removing the only
  invitee disables Open campaign with its reason. While a remove is in flight, Open, Withdraw campaign, Add invitee and
  every Remove are disabled (the focus is in the dialog), and a second confirm sends nothing.
- The invitee form, only on a draft.
- One `.form-actions` row, with nothing a state does not allow: a draft has Withdraw campaign and Open campaign; an
  open campaign has Withdraw campaign and a "View report" link, where it is closed, and, first, "Resend 2 links"
  (`.btn .btn-outline`, `#msf-resend-links`) while any link was not delivered, whose `aria-describedby` names the
  warning above (T251); a closed one under review has Withdraw campaign and a "Review and release" link; a released one
  "View report"; a withdrawn one has no row. Withdraw is where the campaign list offers it (T206): on every campaign not
  yet released (`MsfCampaignRules.IsWithdrawable`, which the command asks too). Withdrawing a closed campaign is the
  decision never to release its report, which whoever may release it may take (T199; until then a campaign nobody would
  release stayed under review for good). Both pages use the same `ConfirmDialog` wording (`MsfCampaignText`). For a
  closed campaign, the dialog and the result say that its report will never be released. They do not mention links or
  addresses, which closing already removed, and its state note ends "If it should never be released, withdraw it here."
  The command is sent the state the dialog was worded by (`WithdrawMsfCampaignCommand.ConfirmedState`), and refuses a
  withdraw confirmed while the page showed the campaign as a draft or open once it has closed
  (`MsfCampaignRules.WithdrawingForgoesRelease`, `WithdrawMsfCampaignCommandHandler.ClosedSinceShown`): a page loaded
  before the auto-close job ran would otherwise withdraw, never to be released, a campaign whose coordinator was asked
  only to stop its links (T199 review). Both pages read the campaign again after the refusal, so the next dialog asks in
  the closed campaign's words. On a draft that invites nobody,
  Open campaign is shown **disabled** with its reason (the T107 pattern above, T225): no click handler, and
  `aria-describedby="msf-open-reason"`, a `.workflow-action-reasons` list below the row reading "Open campaign: add at
  least one invitee first…" (`MsfCampaignText.OpenNeedsInvitees`). The handler's refusal of a campaign with no invitee
  stays the rule; until T225 the button was enabled and the refusal came only after the press.
- An action's result is an `Alert` in an `.action-result` region that takes the focus once it has answered, whenever
  the button that sent it is gone: after an open or a withdraw, and after an invitee add refused because the campaign
  was opened or withdrawn elsewhere (its form is gone). A refused open on a campaign that is still a draft leaves the
  focus on Open, for the retry; an add on a draft leaves it in the form. An open reports "Campaign opened; links are
  being sent." (`MsfCampaignText.CampaignOpened`, T251): the mail server answers after the request, so the open cannot say
  the links arrived, and its "could not all be sent" refusal is only for a mail the queue would not take. A resend reports
  "2 new links are being sent." and moves the focus to its result (its button is gone once no link awaits resending); a
  refused resend that leaves the button (a hand-off that failed) leaves the focus on it for the retry, and one that finds
  the campaign closed elsewhere moves the focus to the refusal, beside the campaign read again. Resend is never disabled
  by its own resend (T234): it carries `aria-disabled` and reads "Resending links…" while it runs, and a second press
  sends nothing; Withdraw campaign is disabled then. An address the campaign already invites, in any
  capitals, is refused the same way (T228): a `role="alert"` refusal naming no address, with the address left typed to
  correct and nobody counted. While an open is in flight, Withdraw campaign and Add invitee are disabled, and Open reads
  "Opening campaign…" but stays enabled and a second click on it sends nothing (T225 review): it holds the focus, and a
  browser drops the focus of a button it disables, to the page (observed in Chrome), so a refused open would otherwise
  leave the focus nowhere. While a withdraw is in flight, Open, Withdraw campaign and Add invitee are disabled (the focus
  is in the dialog). While an invitee add is in flight,
  Open and Withdraw campaign are disabled and a second Enter or click on Add invitee sends nothing (T228 review); Add
  invitee itself stays enabled, because it may hold the focus that a refusal on a draft leaves in place. A refusal is kept
  when the campaign cannot then be read again: the card says it could not be loaded, and the refusal says why the action
  was not taken.

Nobody runs a campaign about themselves, and nobody who holds Trainee runs one at all (T224, T185's trainee rung;
`MsfCampaignRules.IsKeptFromCampaignsAbout`). The create form's Trainee picker never offers the caller. It offers
exactly the trainees the create accepts (`ListMsfCampaignSubjectsQuery`, T238): the current trainees at the caller's
institution, each once, by the rule committee scheduling reads (an active profile on an account that still holds Trainee
and that an administrator has not locked, T268). So it offers no graduate, no trainee who has withdrawn or moved away,
no profile that outlived its Trainee role, no trainee whose account an administrator has locked, and no erased trainee's
pseudonym, and a crafted create for any of them is refused before anything is written, in the words a trainee elsewhere
gets (an Administrator is told "…only be run for a trainee in a programme now…"). A campaign already created stays one
its coordinator can close, release or withdraw after its trainee's programme ends. The picker and the create ask one
predicate (`MsfCampaignRules.MayStartCampaignAbout`, T248), and the page lists no trainee on a campaign's own page,
which has no create form. The campaign page answers a campaign about the caller as it answers an id that names nothing
(its "Campaign unavailable" card), the campaign list leaves those campaigns out, and the report is theirs only as the
released trainee's copy, on their own page (T269, below).

Someone who holds Trainee is told so, whatever role brought them there (`MsfCampaignRules.RunsNoCampaigns`; § A row
the caller cannot change). The campaign list and the campaign page open with a standing `Alert` (`Kind="warning"`,
`Role=""`, `#msf-runs-no-campaigns`) in the words a create refusal gives (`MsfCampaignRules.TraineeRunsNoCampaigns`).
The list then offers no "New campaign" and no empty card, and asks for nothing. The campaign page shows no create card
and lists no trainee; on a campaign's own page the alert sits above its "Campaign unavailable" card. The create page
shows them no Quick template card either, and reads no template list: creating a template is running campaigns, and the
template command refuses them in the alert's words (`MsfCampaignRules.EnsureRunsCampaigns`, T248; until then the card
stayed, since a template is about no trainee, and the command took no caller to refuse). A Coordinator at no
institution (an Administrator can add the role to such an account) runs no campaign either
(`MsfCampaignRules.RunsCampaigns`): the campaign page gives them the same standing alert in the words the create and
template commands refuse them in (`MsfCampaignRules.RunsCampaignsRoles`), and neither form (T248 review). The MSF
Campaigns nav link stays, as Committee Reviews and Decisions due stay for the same people (T185); the page it opens
now says why nothing is there. (T224 review)

**The MSF campaign list's row links** (T225, `/msf/campaigns`) are named by what their page lets the coordinator do in
the row's state, in the campaign page's words (`MsfCampaignText.CampaignLinkLabel`, `ReportLinkLabel`), never "Edit" in
every state:

| Campaign state | Campaign page link | Report link |
|---|---|---|
| Draft | Manage | none |
| Open | Manage | View report |
| Closed, Under review | View campaign | Review and release |
| Released | View campaign | View report |
| Withdrawn | View campaign | none |

A draft has no responses to report and a withdrawn campaign is never released, so neither row links to a report, as
the campaign page offers none there. Each link's `aria-label` starts with its visible label and names the row
("Manage: the campaign for Sipho Dlamini (Annual MSF, closing 2029-03-21)", `MsfCampaignText.RowLinkName`), as the row's
Withdraw button does (T206).

**The MSF report page** (`/msf/reports/{id}`) names each respondent group by its label ("Peer doctor", never
"PeerDoctor"), as its card headings, as the trainee's copy (`/msf/my-reports`) and as the portfolio PDF print it
(`MsfRespondentCategories.Describe`, T225). Close campaign and Release to trainee put their result in an
`.action-result` region above the report, which takes the focus when the button that acted is gone, as on the campaign
page; a load failure is the `StatePanel`'s, including a read that fails after an action that was taken, and a refusal
never replaces the report. Neither can be sent twice at once, and neither is disabled while it runs (T225 review, as Add
invitee): the page refuses the second press, and the button pressed keeps the focus, so a refusal that leaves it (a
close refused on a campaign still open, a release refused on one still ready) leaves the focus on it for the retry.
Release to trainee is disabled only on a report not ready for release; a refused release that finds it no longer ready
moves the focus to the result. The warning that says why (a campaign under review short of responses or groups) is
standing content (`Role=""`) and ends "If it should never be released, withdraw it on its campaign page.", the last
words a link to `/msf/campaigns/{id}` (`#msf-withdraw-elsewhere`), where Withdraw is (T199 review). A release whose save
a withdraw from another tab beat is refused in `ReleaseMsfCampaignCommandHandler.CampaignChanged`'s words, never EF's
row counts; the report read again shows the campaign withdrawn, with no form (T199 review). After a refused close or release the report is read again, keeping the narrative and
level typed, so a refusal names no other page: "If it is still open, close it again."
(`CloseMsfCampaignCommandHandler.CampaignChanged`, worded as T217's open and withdraw refusals). Only an open campaign
closes (T246): a report loaded while the campaign was open still offers Close after another tab or the auto-close job
has closed it, and that close is refused in the campaign's words ("Only open campaigns can be closed.",
`MsfCampaign.OnlyOpenCanBeClosed`), as the campaign page's refused open is (T217). Its close date stands, since it
places the campaign's semester, committee window and evidence date. The report read again shows it as it is now, Close
is gone, and the refusal takes the focus. A refused close or release is shown through `RefusalText.Of`. The Coordinator
actions card holds the narrative and level fields only while the campaign can still be closed or released (Open, Under
review). Otherwise (released, withdrawn, a draft) it shows them as stored, as `<p><strong>Narrative:</strong> …</p>` and
the level by its rung's label ("Not given", "Not stated" when empty), with no form (T246 review): a tab that typed a
narrative and then found the campaign released elsewhere kept its own text in an editable box beside "State: Released",
where the narrative the trainee was given belonged.

The campaign's own subject never reads the report on this page (T269). A trainee who also coordinates, or an
Administrator who is the subject, is admitted to it by role, and the handler admits the subject to the released report.
So the page sends them to their own copy, `/msf/my-reports/{id}`, replacing its address in the history so that Back does
not return to it, and renders nothing of the report meanwhile, only the `StatePanel`'s skeleton. That is the navigation
inside a circuit. A page load (the address bar, a bookmark, a link from outside the app) is the page's prerender, where
the same `NavigateTo` is the server's redirect, a 302 to `/msf/my-reports/{id}` with nothing of the report in it, because
`Wombat.Web.csproj` sets `BlazorDisableThrowNavigationException`. No page links the subject here (the campaign list and
the campaign page leave out a campaign about the caller), so the page load is the usual way in; `MsfReportPageFlowTests`
covers it and `SubjectReadsOwnReportTests` the circuit. Whether a copy is the caller's is the handler's answer
(`MsfCampaignAggregateReportDto.IsSubjectsCopy`), the same answer that leaves the teaching contexts unnamed (T164). The
trainee's page shows only a copy marked that way, so neither page names a context to the subject. The flag says which
page shows a copy, never what a copy may name: the portfolio PDF's copy is not marked, and the trainee reads it. The
trainee's page admits Coordinator as well as Trainee and Administrator, so that every role this page admits can open the
page it is sent to. The nav still offers MSF Reports to Trainee alone.

The state's words are `MsfCampaignText.State` ("Under review", never the enum's "UnderReview"), which the campaign list
and the report print too. Its badge is `MsfCampaignText.StateBadge`:

| Campaign state | Badge |
|---|---|
| Draft | `badge-draft` |
| Open | `badge-submitted` |
| Closed, Under review | `badge-accepted` |
| Released | `badge-completed` |
| Withdrawn | `badge-declined` |

**The trainee's copies of a released report** (`/msf/my-reports` and the portfolio PDF's feedback section,
`MsfSectionComponent`) print nothing of a respondent group below the category threshold: not its label, its answers or
its own count (T249). The PDF used to print "Nurse: 1 responses" above "Below minimum threshold", an exact count under a
group's name, which is what the threshold hides. When the PDF leaves any group out, it prints one line, however many
were left out: "Respondent groups with fewer than N responses are not shown, to protect the respondents' anonymity." N
is the campaign's category threshold. The line explains why the printed groups add up to less than the total, and it
names none of them. A count of one is singular: "1 response", "(1 rating)", and every number prints the same on any
host (invariant culture, so an export is the same bytes, T078).

The two copies differ in one respect. The web copy prints the total (in its list) and no group's count. The PDF prints
the total and each printed group's count ("Consultant: 3 responses"), and a required rating's "(3 ratings)" is the same
figure. So the total less the printed counts is the hidden groups' count together, and with one hidden group it is that
group's own; if the template allows only one other group, it also says which. That says at most who took part, never
what anyone answered, and it is accepted, as the campaign page accepts it for the coordinator (T217 review above) (T249
review). Dropping the group and rating counts from the PDF would match the web copy exactly; that is the operator's
call, not made here, since T249 asked for the counts to be kept and pluralised. The coordinator's report page is not
changed: it still shows a suppressed group's card, without its answers.

**Committee agenda** (T131 slice 4). The committee review page's Agenda card is full width, directly above the
"Pending entrustment decisions" card, so a chair reads what is due and stages it in one place. A formative review has
no Agenda card. The card holds:

- A muted sentence naming the period ("2026 S1") and saying that a line marked Due must be staged or deferred before
  ratify.
- On a review in progress, the EPAs routed to this panel and due whose window lost its decision while the review sat,
  and which the agenda does not hold (T235): an EPA Start left off because a STAR decided it, whose STAR has since been
  revoked. They are named in one sentence of their own in a warning `Alert` (`#agenda-no-longer-decided`) directly
  under the opening sentence, above the table: "PAED-003 is no longer decided in its window: the STAR that decided it
  has been revoked. It is not on this agenda; stage a decision on it to decide it at this sitting." (plural: "PAED-003
  and PAED-006 are no longer decided in their windows: the STARs that decided them have been revoked. They are not on
  this agenda; stage a decision on each to decide it at this sitting."). The alert stands on the page until the chair
  acts, so it takes `Role=""` and is not read out on every load. The chair gets one `.btn-outline.btn-sm` per EPA in an
  `.actions-cell` row under the sentence, "Stage PAED-003", named by its own text (no `aria-label`, so the name holds the
  visible words), which chooses the EPA in the staging form and moves the focus there, as a line's Stage does; staging it adds the chair's line, and
  the EPA is named no longer. Anyone else reads the sentence alone. The EPA is never added to the agenda by itself: the
  agenda was planned at Start and the decision recorded at this sitting settles it (D46). The sentence is not shown
  before Start, which plans the EPA, nor once the decision is recorded, when nothing more can be staged and the
  decisions-due page reads it as "Revoked: re-decide".
- A `.clinic-table` with five columns: EPA (the row header), window ("2026 S1" for a semester, "2026" for an annual EPA),
  state, evidence count, and action. The action column is the chair's alone (T213): anyone else reads four columns, not
  an empty fifth. The caption counts the lines and the ones still to stage or defer. The state cell
  holds the badge and, under it, a `.muted` line saying what the state means for this sitting: the deferral's reason,
  the STAR a decided line names, or that an optional line is optional and why.
- A line whose window another sitting has decided since it was planned reads "Decided elsewhere" (T235): a late
  semester-1 sitting ratifying an annual EPA that the semester-2 review holds as closing. Whether the window is decided is
  `CommitteeAgendaStatus.IsDecided`, the decisions-due page's and the planner's one predicate, read on the line's own
  window whenever the agenda is read and when the decision is recorded and ratified. Such a line is optional: it is not
  counted in the caption, and neither Record nor Ratify is disabled for it. While it is still due its muted line says
  "Another sitting has decided it in this window, so it need not be decided here." On a review in progress, the one
  state that takes a staged decision, it adds "A decision staged on it decides it again.", and the line offers Stage and
  Defer as any line still due does. On a decided review awaiting ratify it adds "Ratifying the review records that.",
  and on a scheduled review nothing. A review whose trainee has moved to another institution reads none of its lines
  this way, since that would say what the new institution decided (T182); a speciality panel that no longer covers the
  trainee at the same institution still does. Recording the decision settles it
  (`DecidedElsewhere` is stored, D46), and so does ratify for one decided since; its muted line then says "Another sitting
  had decided it in this window when this review settled its agenda." A settled line stays so if the other sitting's
  STAR is later revoked: the decisions-due page shows that, and the next sitting plans the EPA again. The trainee's own
  page says "Decided at another sitting in this window."
- Each line offers only what its state allows. A Due line that is not staged offers Stage and Defer. Stage chooses the
  EPA in the staging form below and moves the focus there. A deferred line offers Reinstate. The others offer nothing.
  All of it only while the review is in progress: recording the decision fixes the agenda with the staged STARs (T165).
  On a decided review the one action left is Defer on a closing line that blocks Ratify, which happens only when the
  STAR staged on it was removed afterwards because it no longer fits the trainee's curriculum.
- A deferral opens a `<fieldset>` form under the table, with its `<legend>` naming the EPA ("Defer PAED-002"), a
  required reason, and Cancel and Defer in a `.form-actions` row. The focus moves to the reason once the form is
  rendered, as Stage moves it to the EPA. The trainee sees the reason once the review is ratified (below). An empty
  reason is refused in the form with "Say why the committee is deferring the decision." as a `.validation-message` under
  the box (T212). Its region (`#deferral-reason-message`) is there before anything is refused, and the box names it with
  `aria-describedby` after its help. A refused submit moves the focus back to the box, so the message is read with it.
- The EPAs routed to this panel that a STAR already decided in their window, so the planner left them off, as one
  `.muted` sentence (`#agenda-decided-in-window`) in the scheduling preview's words: "Already decided in this window, so
  not on the agenda: PAED-001 and PAED-006." (T215). A STAR decides its window whether or not an agenda line records it,
  by the decisions-due page's rule. An EPA the review holds a line for is not named: its line says what this sitting did
  with it, and the decisions-due page says where the decision stands now. An empty agenda on a scheduled review says
  that Start adds every EPA due in its period and not already decided in its window.
- The EPAs another panel sitting as a College committee decides for the period ("Decided by another panel"), as a
  read-only list: the panel, the window, and a badge. While one is not yet decided, the progression Decision card shows
  a `.field-warning` above Record decision. That is a warning, never a refusal (O8).

The badges. Each says the state in words, and the three kinds of "still due" share one badge and differ by label:

| State | Badge |
|---|---|
| Due, Due by year end, Partial period, As opportunity allows | `badge-draft` |
| Staged | `badge-submitted` |
| Decided, Decided elsewhere | `badge-completed` |
| Deferred | `badge-accepted` |
| Not decided | `badge-declined` |

Another panel's decision uses the same badges: Not yet decided is `badge-draft`, On the agenda of an open review is
`badge-submitted`, Decided is `badge-completed`, Deferred is `badge-accepted`, and Missed is `badge-declined`. "Missed"
is computed when the page is read, never stored. It means the window has ended with nothing decided, deferred, or on an
open review's agenda. An EPA decided as opportunity allows, or in a partial period, is never missed.

Record decision is a disabled button while a closing line is neither staged, deferred nor decided elsewhere (the T107
pattern above), and so is Ratify. Each one's `.workflow-action-reasons` line names the lines, in the refusal's own words
(`CommitteeAgendaDto.RatifyBlockedReason`). Where more than one reason stands, the line gives the first the handler
would refuse with: the panel's seats before the agenda for Record, the decision's quorum before the agenda for Ratify.
The agenda is read again after every action on the page. A failure is a warning `Alert` in the card, never red beside
the action's success, and the agenda last read stays in view. Every sentence is built in C# (`CommitteeAgendaText`),
which the scheduling form's preview shares.

The "Pending entrustment decisions" list is read again after every action that can change it (T212): Stage, Remove, and
the review's own actions (Start, Record, Ratify, Close, Resolve). Ratifying issues the staged decisions as STARs and
clears the list, which the page used to go on showing until a reload. Defer and Reinstate do not read it: a line with a
decision staged on it cannot be deferred, and neither touches a staged row. A failed read is a warning `Alert` in that
card, as the agenda's is, and the list last read stays in view. An empty list says why it is empty
(`#no-pending-note`). Before the review is ratified it says that nothing has been staged. Once it is ratified, the
sentence depends on the agenda. With a line ratifying decided, it says that ratifying issued what was staged as STARs
and the agenda names each one; staging always leaves the EPA a line, so that is exact. With none, which ratify allows
when every line was optional or deferred (or a progression review's agenda is empty), it says that nothing was staged when the review was ratified, so it issued no
STAR. Where the agenda could not be read again straight after Ratify, the lines staged in the agenda last read count as
issued, since ratifying issues every staged decision.

**The chair's controls** (T213). The review page offers each control to exactly the people its handler lets use it,
in exactly the states its handler takes it, by what `GetCommitteeReviewByIdQuery` says the caller may do:
`CallerChairs`, `CallerResolvesAppeals` and `CallerMayStart`, computed by the predicates the handlers demand
(`CommitteeDecisionAuthorization.Chairs`, `ResolvesAppeals` and `WorksOnPanel`), and `TraineeElsewhere`, the trainee check
every chair's action and Start also demand (`CommitteeTraineeScope`). Start is offered on a scheduled review to a member
of its panel, a coordinator of the panel's institution or an Administrator (T194); an institutional, speciality or
sub-speciality administrator who reads the review without a seat (T218) is not offered it, and `#chair-actions-note`
reads "Only the panel's members, and the coordinators of its institution, can start this review." None of the three
flags is ever true for someone who holds Trainee, whatever seat or role they hold beside it (T185's rung, asked by
`WorksOnPanel` and by the seat predicates since the T194 review): a trainee seated on the panel that reviews them before
they held Trainee reads their own review once it is ratified, and is offered neither the chair's controls nor the
resolve form on it. Since T237 no one who holds Trainee is seated at all (see **The panel form**). The chair alone is
offered the decision form and Record decision (while the review is in progress), Ratify and its reason (once decided),
Close review, the staging form, Remove, and the agenda's Stage, Defer and Reinstate, and only while the chair may sit at
the review now (`PanelSeat.SittingAt`, T237's one rule, which `CommitteeDecisionAuthorization.Chairs` asks and every
chair's action demands, T256): an active committee member at the panel's institution who is not a trainee, and never the
trainee under review. A chair who has since lost the CommitteeMember role, moved institution, been deactivated or been
given Trainee is offered none of them. In their place, while a chair's action is open or to come (scheduled, in progress
or decided), a standing warning `Alert` with no
role (`#chair-cannot-act-note`, the query's `ChairCannotAct`) says why: to the chair, the sentence each click would be
refused with ("You chair this panel but cannot take the chair's actions now: only an active committee member at the
panel's institution who is not a trainee can, and never the trainee under review. A panel administrator must seat a
chair who can."); to every other reader, the same naming the chair ("The panel's chair, Thandi Zulu, cannot take the
chair's actions now: …"). `#chair-actions-note` then says nothing about the chair's actions, which it would name the
chair as able to take; on a scheduled review it still says who can start it. The appeal body alone (the chair or an external member) is offered the
resolve-appeal form, and only while the review is under appeal: a form is never shown with empty fields and no button.
It is offered only to a member of the appeal body who may sit on the panel now (`PanelSeat.AppealBodyAt`, the list the
note below names, T237), which the resolve handler demands too; a chair or external member who has since lost the
CommitteeMember role, moved institution or been deactivated reads the note instead, which does not name them.
These are hidden from everyone else, not disabled: T107's disabled button is for an action the reader may take but
cannot complete yet, and nobody else may take these. Instead a `.muted` sentence in the Review card
(`#chair-actions-note`) names the chair and what only the chair can do in the review's state ("Only the panel's chair,
Thandi Zulu, can ratify the committee's decision."; on a decided review it also names removing a staged decision that no
longer fits and deferring a line that keeps the review from being ratified, where either is open), and under appeal the
Appeals card names who resolves it (`#appeal-body-note`: "Only the appeal body can resolve the appeal: the panel's
chair, Thandi Zulu, and its external member, Anna Botha."). It names only those who can act, the query's `AppealBody`
(T237): the chair and external members who may sit on the panel now (`PanelSeat`), never someone who holds Trainee, and
the same for every reader, the trainee on their own appeal included. A chair who cannot act is left out ("…: the
panel's external member, Anna Botha."), and where no one can, it says so: "Only the appeal body, the panel's chair or an
external member, can resolve the appeal, and none of them can act now. A panel administrator must seat a chair who
can." A Decision card with no decision and no form says "No
decision has been recorded yet." (`#no-decision-note`) rather than stand as a bare heading. When the review's trainee no
longer trains at the panel's institution, Start and the chair's controls are offered to nobody (a global Administrator
excepted, whom the check does not refuse), no `#chair-actions-note` is shown, and a warning `Alert` with no role
(`#trainee-elsewhere-note`) says what those actions would refuse with. What the review holds (its agenda, staged
decisions, evidence and decisions) is shown to every reader as before. The remit form ticks and locks the chair and the
caller, who is the resolver because only the appeal body is offered the form. A staged decision that names no evidence
(only a row written some other way can) says, while the review is in progress, that the chair can remove it and stage it
again; once the decision is recorded it says what the ratify refusal says, that it is fixed and an administrator must
look into it (D46).

**A refusal never says whether an id exists** (T194). Every committee command and every read of a review authorises
before it looks at anything else, and gives an id that names nothing the same sentence as one out of the caller's reach,
before any state check. Each gate has one sentence, which the page prints as it is (`RefusalText.Of`): "The committee
review could not be found among the reviews you can view." (the review page and its sibling reads, a trainee's own review
before ratification included), "…you can start.", "…you chair." (every chair's action), "…whose appeals you resolve.",
and "…among your own ratified reviews." (lodging an appeal). A panel id gets its own gate's sentence ("You can only
manage panels in your institution.", or the scheduling refusal). Only who may act at all is said before the lookup,
because it says nothing about the id: "Only trainees can lodge appeals.", "You are not allowed to manage committee
panels.", "You hold the Trainee role, so you cannot create or change a decision panel, including one that reviews you."
(T256, to someone who holds Trainee beside a role that manages panels), "Only an institutional administrator can say
which College committee a panel sits as.", or T216's scheduling refusals. The seat refusals come after the one refusal,
and only to the seat's holder, who can read the review: "You sit on this panel's appeal body but cannot resolve its
appeals now: …" (T237) and "You chair this panel but cannot take the chair's actions now: …" (T256).

**The trainee's own reviews** (`/committee/my-reviews`). The list's Period column names the period the review sat for,
then its evidence window in a `.muted` span, as the schedule does: "2026 S2 · 2026-01-01 to 2026-12-31" (T212). The
window alone read the same for a year's two sittings, since a semester-2 sitting's window is the whole year. The detail
says the same on two lines, "Sits for" and "Evidence window", as the committee's review page does. A ratified review's
detail shows its agenda read-only, as an
`article.detail-card--compact` headed "Agenda": a muted sentence naming the period, then a `.clinic-table` of three
columns, EPA (the row header), window and outcome. The outcome cell holds the same badge as the chair's table and, under
it, a `.muted` line: the committee's reason for a deferral (O6), the STAR a decided line names, or that it was not
decided at this review. No actions. Nothing is shown before ratify; the read ladder gives a trainee nothing sooner.

**Scheduling a review.** The form asks for the period the review sits for: a select of semesters around today, labelled
with the period itself, "2026 S2 · 1 Jul to 31 Dec 2026". Choosing one fills the evidence window, which stays editable,
from the start of the period's academic year to the period's end: 2026-01-01 to 2026-12-31 for 2026 S2, because the
annual EPAs a semester-2 sitting closes are judged on the whole year. Under the fields, a full-width section previews
the agenda. It says how many EPAs will be on it and which must be decided, lists the optional ones with their status,
says which EPAs another panel decides ("schedule them separately"), and says which are already decided in the window.
Only the opening sentences sit in a `role="status"` region (`#agenda-preview-summary`), so a screen reader hears what a
new choice changed, not every line again. Where a STAR already decided every EPA due, the opening sentence says so
itself ("Every EPA this panel decides that is due for 2026 S2 is already decided in its window…"), not that none is due,
since the note naming them is outside the live region (T215). The Trainee select offers exactly the trainees the
scheduling handler would accept on the chosen panel (T182), and the Panel select offers exactly the panels on which that
list is not empty (`ListDecisionPanelsQuery` with `ForScheduling`, T194): not a panel the caller sits on at another
institution, not a speciality panel of a speciality they do not administer, and not a panel whose only trainee in reach
is not a current trainee. The Trainee select, the Panel select and the handler all read one rule for who that is
(`TraineeScopeResolver.ResolveCurrentAsync` and its set form, T238): an active profile on an account that still holds
Trainee and that an administrator has not locked (T268; a lockout after wrong passwords, which lifts itself, does not
count). So an erased trainee's pseudonym, a trainee whose programme has ended, one who no longer holds Trainee and one
whose account an administrator has locked are offered nowhere, and a typed or crafted request for one gets the
scheduling refusal before anything is written (an Administrator is told "Only a trainee in a programme now can be put
before a panel…"). When it offers none, its help text says so: "No panel has a trainee you can schedule a review for. A
panel is listed once a trainee it reviews is in a programme you oversee."

**The panel form** (`/committee/panels/new`, T194). A new panel's Scope and Speciality selects offer what creating the
panel would accept from the caller, read from `GetDecisionPanelFormOptionsQuery`, never from roles: an institutional
administrator both scopes and the specialities their institution has adopted a curriculum in; a speciality or
sub-speciality administrator the Speciality scope and only those of their own specialities their institution has adopted
(T245); an Administrator both scopes and, once they have chosen the institution above, the specialities it has adopted
(the query asked again with `InstitutionId` whenever the institution changes, T245 review). A speciality chosen before the
institution changed that the institution now chosen has not adopted is unchosen. Adopted is one predicate
(`AdoptedSpecialities.IdsAt`), the one the institutional administrator's speciality list reads, asked at the panel's
institution, and panel create refuses any other speciality from everyone: "Your institution has adopted no curriculum in
this speciality, so a panel for it would have no trainee to review.", or to an Administrator "The institution you chose
has adopted no curriculum in this speciality, so a panel for it would have no trainee to review.", which the form,
offering none, never draws. So that a speciality is not simply missing, the Speciality select's help text says why: "The
panel runs at your institution. Only the specialities it has adopted a curriculum in are listed."; to an Administrator
"Choose the institution first: only the specialities it has adopted a curriculum in are listed." until they have, then
"Only the specialities the chosen institution has adopted a curriculum in are listed." Speciality is selected first where
it is offered. Where nothing is, the page shows no form: a `detail-card--empty` card (`#panel-none-creatable`) headed "Create
panel" says in a `.muted` paragraph that they cannot create a panel, and who can: "…A speciality or sub-speciality
administrator creates panels for their own speciality at their own institution, once the institution has adopted a
curriculum in it. An institutional administrator creates the institution's panels." Every seat's picker (Chair, Members, External members) offers
exactly the people the panel may seat, `ListPanelMemberCandidatesQuery` by `PanelSeat`, the rule panel create and
update enforce: active committee members at the panel's institution who do not hold Trainee (T165, T237). The Members
help text ends "Only active committee members at the panel's institution who are not trainees are listed.", and the
External members help text says the same of its list and that external members sit with the chair on the appeal body. A
stored
member who can no longer sit is not listed and not sent back: a standing warning `Alert` (`Role=""`) above the form
says so before the save ("1 member of this panel can no longer sit on it, so is not listed below: only an active
committee member at its institution who is not a trainee can. Saving takes that member off the panel."). A stored chair
who cannot sit leaves the Chair select on "Select chair", so the form cannot be saved until another chair is chosen:
Save shows "Choose a chair." in the validation summary. The Members and External members pickers leave the chair out,
and choosing as chair someone already selected in either takes them off that selection too (T257): until then the
selection kept them, unseen, and the save was refused with "A panel member is listed more than once.". A polite line
under the Chair select says so ("Priya Naidoo is the chair now, so is no longer selected under Members."): a
`role="status"` region (`#panel-chair-note`, `.muted`) present before any chair is chosen, which the select names with
`aria-describedby` after its help while the line says something. Choosing another chair, or none, puts the chair before
back in the selection choosing them took them from, and the line says that too, after what the new choice took, if
anything ("Priya Naidoo is selected under Members again."). So a chair chosen by mistake takes nobody off, nor does
moving down the Chair select with the arrow keys, which in Chrome and Edge on Windows chooses each person passed. The
line is emptied when a choice takes nobody off and puts nobody back, when the institution changes, when the panel is
read again, and when the save succeeds; after any of these nobody is put back, so a stored chair is not moved to Members
when another is chosen: they are offered there again, unselected. Each option in the Members and External members
selects is keyed by the person it offers (§ Form system).

**Nobody who holds Trainee administers a panel** (T256). Panel create and update refuse someone who holds Trainee beside
a role that manages panels (the Administrator's included, T185's rung) before anything is read, the panel's read gives
them nothing, and the pickers offer them nobody: a trainee does not choose who sits on their review. So the panel form,
for a new panel (`/committee/panels/new`) and an existing one (`/committee/panels/{id}`) alike, reads
`GetDecisionPanelFormOptionsQuery` first and, where its `TraineeNote` is set, shows no form, no "Decides for" card and
nothing about the panel: a `detail-card--empty` card (`#panel-trainee-note`) headed "Create panel" or "Update members"
says in a `.muted` paragraph "You hold the Trainee role, so you cannot create or change a decision panel, including one
that reviews you.". The panel list (`/committee/panels`) offers **New panel** only when creating a panel would be
accepted (`MayCreateAny`) and **Edit** only on a row whose form would open (`DecisionPanelSummaryDto.CallerMayManage`,
the rule panel update and the form's read demand), so a committee member, a coordinator, or a speciality administrator
looking at another speciality's panel is offered neither either. To someone who holds Trainee beside a role that manages
panels it also says why, in a standing info `Alert` (no role) above the list (`#panels-trainee-note`), with the same
sentence; the list of panels and who decides each EPA stay in view. Its empty card says "Create a panel before
scheduling reviews." only to someone who may create one; otherwise "No decision panel runs at your institution yet.", or
to a global Administrator, who belongs to no institution and lists every panel, "No decision panel has been created yet."

**Who is offered scheduling** (T216). The page reads it from `GetCommitteeReviewsAccessQuery`, never from the caller's
roles: `MaySchedule` is the rule the scheduling command and the agenda preview demand first. A caller it refuses, a
committee member or anyone who also holds Trainee, is not offered "Schedule review", and a link that names a panel
opens no form (only a typed or saved one: Decisions due gives such a caller no Schedule link). The subtitle then reads "Open existing
committee reviews.". Someone who holds Trainee, whatever other role admits them, the Administrator's included (T185's
trainee rung), is also listed no review, their own included, so the list's empty card says why: its title is "No
reviews to show", its body is the query's `TraineeNote` ("You hold the Trainee role, so you cannot schedule a committee
review or preview its agenda, and this page lists no one's reviews. Your own are on My committee reviews once they are
ratified.", or, beside a committee member's role alone, "…so this page lists no one's committee reviews…"), and its
action is a `.btn-outline` link, "Open My committee reviews". A committee member who holds no Trainee role keeps the
list of their panels' reviews; their empty card says those appear once they are scheduled.

**Who is listed which reviews** (T218). The list is exactly the reviews whose Open link will open, by the review's own
read ladder in its set form (`CommitteeDecisionAuthorization.ReadableReviewsAsync`, which the review page and every
sibling read demand through `DemandReviewAccessAsync`): a coordinator or institutional administrator every review a panel
of their institution holds; a speciality or sub-speciality administrator the reviews of their own speciality's or
sub-speciality's trainees at their institution (T182's scope, read back); a committee member the reviews of the panels
they sit on, wherever; a global Administrator every review. Someone who holds Trainee is listed none (above), not even
their own ratified review, which opens for them and is on My committee reviews: the one place the list is narrower than
the review. So a scheduler finds the review they have just scheduled, and the Decisions due page links a review exactly
where it would open. A speciality administrator's reach follows the trainee's programme now, as every read about the
trainee does, not the programme they were reviewed in: when a trainee moves from Surgery to Paediatrics at the same
institution, their earlier reviews there pass to the Paediatrics administrator and leave the Surgery one
(`CommitteeDecisionAuthorization.InSchedulingReach` says why the review is not stamped instead). The list's empty card
never says that no review exists, since reviews out of the caller's reach may, and does not say which reviews are in
reach, which differs by role: to a caller who may schedule, its title is "No reviews yet" and its body "The reviews you
can open appear here once they are scheduled. Use Schedule review to put a trainee before a panel." It never says
"Schedule the first committee review".

**Entrustment-only reviews** (T131 slice 5). A review before a panel sitting as a College committee (the neonatal CCC)
decides entrustment only: its decision is the STARs staged at it, and it records no progression category. A general
panel's semester-1 sitting may be one too. Every page says so in the same words, built in C#
(`CommitteeDecisionWording`, which the portfolio PDF shares):

- **The type** reads "Annual progression review", "Pre-graduation review" or "Entrustment-only review" on the review
  card, and "Annual progression", "Pre-graduation" or "Entrustment only" in a list column.
- **The outcome** of a recorded decision is its category in words ("Satisfactory with Observations"), never the enum's
  name. An entrustment-only review's reads "Entrustment decisions only": as the decision's heading on the review page,
  in the Decision column of the schedule and of the trainee's reviews, as the heading of the trainee's current decision,
  and on the portfolio PDF's Decision line. It never reads "Pending" once the decision is recorded.
- **The Decision card** of an entrustment-only review has no Category field. In its place, above the form, a `.muted`
  paragraph (`#entrustment-only-note`) says the decision is the entrustment decisions staged below and records no
  progression category; once the review is ratified it says instead that the decision is the entrustment decisions the
  review issued, each named on its agenda. Rationale, conditions and who was present are asked as on any review. O8's
  sitting-order warning is not shown: it is about a progression decision, and this review takes none. While nothing is
  on its agenda, Record decision is shown disabled with the reason (the T107 pattern, `#record-reason`): its decision is
  what the agenda holds, and staging a decision puts the EPA there.
- **Every category select opens on "Select a category…"**, the first-time decision's as well as a remitted appeal's
  replacement, so the page never shows a category the chair did not choose. Leaving it there on a progression review is
  refused by the handler, whose reason the page shows ("… records a progression category. Choose one.").
- **A remitted appeal** on an entrustment-only review has no Replacement category either. A `.muted` line
  (`#remit-entrustment-only-note`) says why, and says what the remit does not do: the replacement changes no entrustment
  decision, the STARs the review issued stand, and one is changed by revoking it and re-deciding the EPA at a new review.
- **The trainee's reviews.** Under the heading of an entrustment-only review's decision, a `.muted` line says "This
  review decided entrustment only, so it records no progression outcome."; adds, when an appeal remitted the decision,
  that the decision is the appeal body's and the review's STARs stand unless one is revoked; and adds "What it decided on
  each EPA is on its agenda below." when the agenda is shown. The detail names the type.
- **Scheduling.** The Review type select offers only the types the handler accepts (`CommitteeReviewTypes.Allowed`):
  before a College committee, only "Entrustment-only review", with help text naming the committee; before a general
  panel, all three in semester 1 and the two progression types in semester 2. The semester-1 help text says when to
  choose entrustment-only (the sitting decides STARs, not progression) and that the semester-2 sitting always decides
  progression; it does not say semester 1 never does. Choosing a panel takes its default type; choosing a period keeps
  the chosen type while the period allows it, and takes the default otherwise. Where the panel decides no EPA on the
  trainee's curriculum, the agenda preview says so first, since an entrustment-only review there is refused.
- **The panel's College committee** ("Decides for" on the panel form) cannot change while a review before the panel is
  scheduled, in progress or awaiting ratification: what the review decides was fixed from it. The select's help text
  says so, and a refused save names the open review in the card's `Alert`.

**Decisions due** (T131 slice 6). `/committee/decisions-due` is a list page for the roles that schedule reviews
(Coordinator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Administrator). It lists, per current trainee the
caller administers or coordinates at their institution (an active profile on an account that still holds Trainee and
that an administrator has not locked, the rule scheduling reads, T238, T268), each EPA due for an entrustment decision
in a period and where the decision stands, so every Schedule link it offers is one the scheduling handler accepts. Every
sentence is built in C# (`DecisionsDueText`). A caller who also holds Trainee is shown nobody's decisions, whatever
other role admits them (T185's trainee rung): the page shows its empty card.

- **Filters**, in the `search-container`: Institution (an Administrator only, who belongs to none: nothing is read until
  one is chosen, and `#due-choose-institution` says so), Period (the scheduling form's semesters, the current one by
  default), Status ("Outstanding: not yet decided" by default, then "Every status", then each status), and EPA.
- **The opening**: a `.muted` sentence (`#due-intro`) naming the period, how many trainees and where, and what Missed
  means.
- **By EPA**: a `.detail-card` holding a `.clinic-table--compact` of eight columns: EPA (the row header), Due, Decided,
  Scheduled, Deferred, To schedule, Missed and Optional. Its caption says what the last three count. The filters do not
  narrow it: it is the whole period at a glance. Its `.table-container` (`#due-summary-scroll`) is `tabindex="0"`,
  `role="region"` and labelled by the card's heading, so a keyboard can scroll it at 390px: it holds nothing focusable.
- **The count**: a `.muted` line (`#due-count`, `role="status"`), "4 of 15 decisions due in 2026 S2 shown.", which a
  screen reader hears again whenever the Status or EPA filter changes it.
- **The list**: a `.clinic-table` of five columns: Trainee (the row header, by name), EPA, window ("2026 S1", or "2026"
  for an annual EPA), status, and action. The status cell holds the badge and, under it, a `.muted` line saying what it
  means: the STAR and review for a decision, the open review holding it, the review that deferred it, or why it is
  optional. `PagerControls` pages it, 20 rows to start. When the filters leave nothing, a `.detail-card--empty` says so.
- **Actions.** "Open review" where the status names a review and the caller passes that review's read ladder. "Schedule"
  on every row neither decided nor scheduled: it opens `/committee/reviews` with the panel the EPA routes to for the
  trainee, the trainee and the period a sitting that decides the window sits for, the window's last semester
  (`?panel=…&trainee=…&period=2026-2`): semester 2 for an annual EPA, whichever semester the page shows (Decision 5).
  The scheduling form takes each value only where it offers it, so the link authorizes nothing. Where no panel the
  caller can schedule before decides the EPA, the cell says so. Each link's `aria-label` names the trainee and the EPA.
- **An open review holding the seat.** A trainee has one open binding review per period in each seat (the general
  committee, or a College committee), and scheduling a second is refused. Where one is already open for the row's
  schedule period before the panel's seat, the row offers "Open review #N" (where the caller may open it) instead of
  Schedule, and its `.muted` line says "Review #N is open for 2026 S2 before General CCC: decide it there, or ratify that
  review before scheduling another." A line deferred on that same open review reads "Deferred at review #N, which is
  still open: its chair can reinstate it there", beside its one "Open review" link.
- **States**: `StatePanel`'s skeletons while loading, its danger `Alert` on a failure, and its empty card ("Nothing due
  for 2026 S2") when no trainee owes a decision.

| Status | Badge |
|---|---|
| Decided | `badge-completed` |
| Scheduled | `badge-submitted` |
| Deferred | `badge-accepted` |
| Not scheduled, Due by year end, Partial period, As opportunity allows | `badge-draft` |
| Missed, Revoked: re-decide | `badge-declined` |

Decided means a STAR from a sitting for a period in the window that still decides it, whether or not an agenda line
records it: Active; Expired, since expiry is informational (Decision 10); or Superseded, by a STAR from another window or
by one that decides this window in turn. A revoked one reads "Revoked: re-decide", and so does one superseded inside the
window by a STAR since revoked. Deferred holds only while no later sitting has decided the EPA since, in sitting order
(the period a review sat for, then the order reviews were scheduled in), so a deferral that a later, since-revoked
decision overtook reads "Revoked: re-decide". Missed is computed when the page is read, as on the agenda: the window has
ended with nothing decided, deferred or on an open review's agenda, and an EPA decided as opportunity allows, or in a
partial period, is never missed. The agenda planner, its "routed elsewhere" list and its "Already decided in this
window" sentence read the same rule (`CommitteeAgendaStatus.IsDecided`), so a sitting never plans as due an EPA this page
calls decided (T215). An open review's own lines are read against it too, so a line this page calls decided reads
"Decided elsewhere" on the review and never holds back its Record or Ratify; and "Revoked: re-decide" is
`CommitteeAgendaStatus.HasLostItsDecision`, the predicate a review in progress names an EPA off its agenda by (T235).

## Status dots

```css
.status-dot      /* 0.6rem circle, inline-block, margin-right sm */
.status-dot.ok   /* --success-color */
.status-dot.warn /* --warning-color */
.status-dot.err  /* --danger-color */
```

Used in the Administrator dashboard system-health card to show service status at a glance.

## Pager

```css
.pager                 /* flex, gap 1rem, margin-top 1rem */
.pager-info            /* muted, .9rem — "Showing 1–20 of 137" */
.pager-actions         /* flex, gap .5rem — prev/next buttons */
.pager-page-size       /* inline-flex, gap .4rem — "Per page: [20]" */
.pager-page-size-label /* muted, .85rem */
.pager-page-size-select/* width auto, compact */
```

`PagerControls.razor` is the one component for pagination. Use it on every list that can grow.

## Accessibility

- Every form field has a `<label for="…">` tying to the input's `id`.
- A field's help text has an id, and its input names it with `aria-describedby`, help first (§ Form system, T193).
- A sign-in field says what it holds: the email is `autocomplete="username"` and the password
  `autocomplete="current-password"`. A password being set is `autocomplete="new-password"`: register, change password,
  and an administrator's reset, where the browser would otherwise offer the administrator's own password (T193).
- A refused sign-in (`?error=`) is named by the email and password fields' `aria-describedby`, and a refused link by
  the password field's: the page reloads with focus in the field, and the alert alone is not announced (§ Alerts,
  validation, empty states). A refused password change is named by each of its three fields; the page reloads with the
  focus on the refusal (T265).
- Required fields show a visual `*` plus `aria-required="true"`.
- `.visually-hidden` is available for screen-reader-only copy.
- `:focus-visible` uses `--focus-ring`. Never remove focus outlines without replacing them.
- Never leave the focus on the page body. A button is not disabled by its own action, and says it is unavailable with
  `aria-disabled` while that action runs; an action that is done moves the focus to its result, and one whose button is
  gone does too (§ Button system, T234).
- An invalid field is marked by a stripe as well as the danger colour, and stays marked in a contrast theme (WCAG 1.4.1,
  § Form system, T236). Its focus ring is the usual outline, outside the stripe.
- Up/down reorder buttons (T019) are keyboard-focusable `<button type="button">` with `aria-label="Move field up"`.
- `<fieldset>` is reset to no border/padding and its `<legend>` styled as a heading — this is the semantic grouping for multi-field clusters.

## Icons

- One `Icon.razor` wraps `<svg>` + `<use href="/icons/{name}.svg#i" />` or inline path data.
- Icons live under `src/Wombat.Web/wwwroot/icons/` as individual SVG files, copied from Lucide (MIT licensed, compatible with AGPL-3.0).
- The nav uses the same `Icon.razor`; `NavMenu.razor.css` only sizes and spaces it (§ The NavMenu).
- **Do not load a Bootstrap Icons font.** ClinicAssist tried and the `<i class="bi bi-*">` approach renders nothing without the font, silently. Repeating that mistake is not on the table.

## Page-level patterns

Every page in `Wombat.Web` follows one of these shapes. Pick one at the top of the file and stick to it.

**A person is shown by name, never by user id** (T142). The name is a field on the page's DTO, filled by the query
that serves the page with one `UserDisplayNames.ResolveAsync` call for every id it lists. Razor never looks a name up,
nothing looks one up per row, and a mapper that other handlers share stays lookup-free. The id is shown only where no
user by that id exists, or the user has no name on record. Plain text, not `<code>`: a name is not an identifier. A
filter that narrows a list to one person takes what the list shows, a name, not an id.

Five pages keep ids on purpose. The audit log and an audit entry (`/admin/audit`), and the data-rights request list and
a request (`/admin/data-rights`), because the id is the record there. The anonymous portfolio verification page
(`/portfolio/verify`), because it must not disclose a name. No input takes a raw user id: the committee review schedule
form's trainee is a picker since T182.

**A workflow state or move is shown by its label, never by its key** (T220). The activity page's header, summary and
history, the activity lists, the dashboards' badges, the committee's evidence snapshot and the portfolio PDF all print
the label the query carried from the activity's pinned workflow (`CurrentStateLabel`, `FromStateLabel`, `ToStateLabel`,
`TransitionLabel`, `FinalStateLabel`, `SourceStateLabel`), so a page names a state as the refusals and notices on it do
(T189): "Awaiting supervisor", not `submitted`. A move is named as its button is ("Sign Off"). Razor never works a label
out. The key appears only as a badge's colour class (`badge-@item.CurrentState`), and as text only where the pinned
workflow cannot name it.

**A committee review's state is shown by its label, never by the enum's name** (T250). The schedule, the review page,
the trainee's own reviews and the portfolio PDF print one label per state, `CommitteeDecisionWording.StateLabel`, which
the review DTOs carry as `StateLabel`: "Scheduled", "In progress", "Decided", "Ratified", "Under appeal" and "Closed".
`Final` reads "Closed", since a review reaches it when a formative review is closed or when an appeal is resolved. A
refusal that names a review's state mid-sentence uses `StateInSentence`, which is built from the same labels ("decided,
not yet ratified"). The data-rights JSON export keeps the stored value, because it is the record.

### List page

```
<PageTitle>…</PageTitle>

<PageHeader Title="…" Subtitle="…">
  <Actions>
    <a class="btn btn-primary" href="/…">Create …</a>
  </Actions>
</PageHeader>

<div class="search-container shadow-sm mb-4">
  <div class="search-grid">
    <div class="search-field">…</div>
  </div>
</div>

@if (IsLoading)            { <Skeleton … /> × N }
else if (LoadError is not null) { <Alert Kind="danger" /> }
else if (Items.Count == 0)      { <DetailCard Empty CTA /> }
else
{
  <div class="table-container shadow">
    <table class="clinic-table"> … </table>
  </div>
  <PagerControls … />
}
```

### Detail page

```
<PageHeader Title="@item.Title">
  <Actions>
    <a class="btn btn-outline btn-sm" href="…">Edit</a>
    <button class="btn btn-danger btn-sm" @onclick="…">Delete</button>
  </Actions>
</PageHeader>

<div class="details-grid">                        <!-- 1fr 2fr @ >= 900px, 1fr @ narrow -->
  <aside class="detail-card"> summary </aside>
  <section class="detail-card"> main body </section>
</div>
```

### Form page

```
<PageHeader Title="…" />

<ActionResult @ref="_result">          @* what Save did; takes the focus once it is done (§ Button system, T234) *@
  <Alert Kind="success">…</Alert>
  <Alert Kind="danger">…</Alert>
</ActionResult>

<div class="form-container">
  <EditForm … >
    <ValidationSummary class="validation-summary-errors" />

    <div class="form-grid">
      <FormField Label="…"> <InputText class="form-control" /> </FormField>
      …
      <FormField Label="…" FullWidth="true"> <InputTextArea class="form-control" /> </FormField>
    </div>

    <div class="form-actions">
      <a class="btn btn-outline" href="…">Cancel</a>
      <button type="submit" class="btn btn-primary">Save</button>
    </div>
  </EditForm>
</div>
```

### Dashboard page

Dashboards are a composition, not a standalone page pattern.

- **`Home.razor`** is the one routed page at `/`. It owns the `<PageHeader Title="Welcome, {name}" Subtitle="Viewing as {role}" />`, resolves the active role (via a cookie plus `DashboardPriority.Order`), renders the "You also act as … Switch view:" link row if the user holds multiple roles, and picks one of the role dashboards to render inside.
- **Role dashboards** live under `Components/Pages/Dashboards/` (`AdministratorDashboard`, `CollegeAdminDashboard`, `InstitutionalAdminDashboard`, `SpecialityAdminDashboard`, `SubSpecialityAdminDashboard`, `CommitteeMemberDashboard`, `CoordinatorDashboard`, `AssessorDashboard`, `TraineeDashboard`). Each one is a child component — **no `@page` directive**, **no `<PageTitle>`**, **no `<PageHeader>`**. Adding any of those would duplicate Home's header.
- **Every role has its own case in Home's switch**, and the dashboard it picks admits the role (T261). Until then a
  CollegeAdmin fell through to the trainee's dashboard: "No curriculum assigned yet", beside links to pages that refuse
  a CollegeAdmin. PendingTrainee shares the trainee's dashboard, which branches on it. A signed-in user who holds **no
  role** (an administrator removed the last one) is not given a role's dashboard either: Home shows no "Viewing as"
  subtitle and one "No role assigned" card with no link, since no page is theirs to open.
- **Each dashboard names the roles it is for in `[Authorize(Roles = …)]`.** Blazor enforces `[Authorize]` on a routed
  page only, so on a dashboard it is a statement the test reads, not a gate. A dashboard with none, or a bare
  `[Authorize]`, would be judged as admitting every signed-in user; the test refuses both.
- **Every link on a dashboard opens a page that admits the dashboard's role** (T261), the nav's rule (§ The NavMenu).
  `DashboardLinkAuthorizationTests` renders Home for a holder of each role and judges every link on it with the nav
  test's `PageAccess.PageFor` and `RefusalOf`. It also checks that each dashboard's renders draw every `href` it can
  draw: those its own source declares, and those declared by every Wombat.Web component it names and by the components
  those name (the shared `EpaTargetCoverageList`, say). So a link in a branch the test does not reach, in the dashboard
  or in a component it uses, fails until the test's `DashboardSender` fills that branch; the sender fills every list a
  dashboard shows. An `href` that only passes on the component's own parameter (`DashboardCard`'s `href="@Href"`) is
  judged where the caller sets `Href`, and an SVG `<use href>` is no link. A
  link to a page the role cannot open is removed or pointed at one it can: the Coordinator starts an MSF campaign rather
  than issuing an invitation, and the InstitutionalAdmin's quick links are Users, Invitations, Curriculum adoptions and
  Entrustment decisions, not the Administrator's Institutions or the College's Specialities.
- **`/dashboard/switch/{role}`** (defined in `Program.cs`) is a minimal-API endpoint that writes the preferred-role cookie and 302s back to `/`. It never renders UI directly. Every way a user reaches a dashboard goes through Home.

So a dashboard `.razor` file looks like this:

```razor
@using Wombat.Application.Features.Dashboards.{RoleName}
@attribute [Authorize(Roles = "{RoleName}")]
@rendermode InteractiveServer
@inject IScopedSender Sender
@inject AuthenticationStateProvider AuthenticationStateProvider

<StatePanel IsLoading="_loading" LoadError="@_error">
  @if (_vm is not null)
  {
    <div class="dashboard-grid">
      <DashboardCard Title="…" Icon="…"> … </DashboardCard>
      <DashboardCard Title="…" Icon="…" Emphasis="true"> … </DashboardCard>
      <DashboardCard Title="…" Icon="…" Span="2"> … </DashboardCard>
    </div>
  }
</StatePanel>
```

Inline `style="..."` is acceptable inside a dashboard's list items — `style="display:flex;justify-content:space-between;padding:var(--space-xs) 0"` for a badge-row, `style="width:@percent%"` for a progress bar fill — because these are per-instance layout values, not a reusable utility. Keep them token-backed (`var(--space-*)`, never raw px). If the same inline-style pattern starts appearing in four or more dashboards, promote it to a named utility in `app.css`.

Every dashboard uses `.dashboard-grid` + `DashboardCard` + the `.dashboard-metric` / `.progress-bar` / `.status-dot` primitives. Role-specific content lives inside the cards; the grid and card shapes do not.

A dashboard that reads nothing (the CollegeAdmin's, a card of links to the national catalogue it authors) has no query,
no `StatePanel` and no `@rendermode`: the grid and its cards are the whole file.

### Account / auth page

```
<div class="account-form-container">
  <h2>Sign in</h2>
  @if (Error is not null) { <Alert Kind="danger" Id="login-error">@Error</Alert> }   @* role="alert" by default *@
  <form method="post" action="/account/login/submit">
    <div class="mb-3"> label + .form-control with its autocomplete token, aria-describedby="login-error" on a refusal </div>
    …
    <button type="submit" class="btn btn-primary">Sign in</button>
  </form>
</div>
```

`.account-form-container` is a 400px centred card with a wide top margin — the shape ClinicAssist uses for its login/register/change-password pages.

**A visitor who has not signed in gets static pages** (T181). `App.razor` gives them no render mode, so no page they
reach opens a circuit: the Blazor hub stays behind the fallback policy, because inside a circuit navigation never meets
an endpoint's policy. So everything on an `[AllowAnonymous]` page must work as plain HTML: links, form posts to a
minimal-API endpoint (as sign-in, register and forgot-password do), and `wombat.js` for any behaviour. `@onclick`,
`@bind`, `OnAfterRenderAsync` and JS interop do nothing there.

The same page is **interactive for a signed-in visitor** unless it carries `[ExcludeFromInteractiveRouting]`. So a page
that posts back to itself (`@formname` + `[SupplyParameterFromForm]`) runs two ways. Signed out, the submit is an HTTP
post that the pipeline sees. Signed in, it is an `@onsubmit` event in the circuit: there is no post, so
`[SupplyParameterFromForm]` binds nothing, the cascaded `HttpContext` is null, and no per-request middleware (a rate
limit) or response status applies to it. Such a page carries `[ExcludeFromInteractiveRouting]` as well, as `/msf/respond`
does, or it posts to an endpoint instead.

Two predate the rule and are dead signed out: `PasswordToggleButton` (sign-in, register, link) and the register page's
`OnAfterRenderAsync` that clears the invitation token from the address. Neither ever worked signed out: the hub has
always refused an anonymous circuit. `/portfolio/verify`'s Verify button was the third. Since T265 that page carries
`[ExcludeFromInteractiveRouting]` and its form is a GET: Verify loads `/portfolio/verify?hash=…&check=1`, and the check
is made as the page renders, the same for every visitor. A check that fails says so on the page.

**The sign-in cookie is written only in an HTTP request** (T265). A circuit's response started when the page first
loaded, so `SignInManager` there throws "Headers are read-only". Sign-in, register, link-account, sign-out and change
password each post a form to an endpoint in `Program.cs`, which writes the cookie and redirects. Change password stays
an interactive page, for its Show buttons. Its form, a plain `<form method="post" action="/account/change-password/submit">`
with `<AntiforgeryToken />`, is posted by the browser, which the circuit does not intercept. The endpoint changes the
password and issues the cookie again with the new security stamp, in one request. A separate "refresh the cookie"
endpoint would hand the new stamp to any cookie still inside the stamp validator's interval, a stolen one included. The
redirect back carries `?status=updated`, or a code for each refusal (`?error=PasswordMismatch`), never the words.
The page chooses the words (`ChangePasswordOutcome`), so a crafted link cannot put its own text in the page's alert.

An endpoint that issues the cookie again checks the session first (T265 review). The stamp validator looks at a cookie
once in thirty minutes, so until then a session that has already ended (an administrator's lock, a password changed in
another browser, a change of roles) still reaches the endpoint. `SignInManager.ValidateSecurityStampAsync` refuses it,
and the endpoint signs it out rather than hand it the new stamp, which would keep it alive past the lock. A password
such an endpoint checks is checked as the sign-in page checks one: `CheckPasswordSignInAsync` with
`lockoutOnFailure: true`, under the sign-in throttle (`LoginRateLimitPolicy`), whose refusal goes back to the form the
post came from. An account that signs in through its institution (`AllowLocalPassword` false) has no password to
change. A fault before the change says the password could not be changed; a fault after it, in issuing the cookie,
signs the user out and says the password was changed.

### Anonymous static page (the MSF respondent page, T205)

A page for a stranger holding an emailed link, who never signs in: `/msf/respond`, where an MSF respondent answers. It
is the account page's shape, widened for a questionnaire, and it is the one page that is **never interactive**, signed
in or not:

```razor
@page "/msf/respond"
@attribute [AllowAnonymous]
@attribute [ExcludeFromInteractiveRouting]              @* static HTML: App.razor gives it no render mode *@
@attribute [EnableRateLimiting(MsfRespondRateLimit.PolicyName)]
@attribute [RequireAntiforgeryToken(required: false)]  @* the link is the post's authority *@
@layout Layout.AuthLayout

<PageTitle>@PageTitleText</PageTitle>                   @* one per state; "Error: …" after a refused post *@
<div class="account-form-container account-form-container--wide shadow">
  brand lockup, <h2>, the state's Alert
  <div id="msf-error" class="error-summary" tabindex="-1" autofocus>  @* only after a refused post *@
    <Alert Kind="danger" Role="alert">…</Alert>
  </div>
  <form method="post" @formname="…" @onsubmit="…" data-submit-once>
    … fields, only while the questionnaire is shown …
  </form>
</div>
```

- `App.razor` renders `Routes` and `HeadOutlet` with no render mode for a page marked `[ExcludeFromInteractiveRouting]`
  and for a visitor who has not signed in (T181), and `InteractiveServer` for a signed-in user's every other page. The
  attribute is what keeps the page static for a respondent who happens to be signed in. Static, the page opens no
  circuit, the link's rate limit sees the submit (an HTTP post, not a SignalR message), and it works without WebSockets
  or script. `Hosting/MsfRespondPageHostingTests` loads it signed in for that reason.
- Fields are plain inputs named `Prefix.Key[id]`, read by `[SupplyParameterFromForm]`. Give them `@onchange`/`@oninput`
  too: bUnit renders the page interactively, and the handlers are how its tests fill the form.
- The `<form>` stays in the tree when it holds nothing. Blazor answers a post whose named form is not on the page with a
  bare 400, and a link can die between loading the page and posting it.
- `data-submit-once` (`wombat.js`) drops a second submit while the first is on its way: the link takes one response.
- A refusal is the page's own state, with the status the Api would answer (`MsfResponseRefusals.Describe`), set as the
  response starts: .NET 10 writes no body for a page that ends its render at 404.
- Every state has its own `<PageTitle>` ("Feedback link expired - Wombat", "Thank you - Wombat"), and a refused post's
  starts "Error:". A refused post reloads the whole page, so its summary is an `.error-summary` that takes the focus as
  the page loads (`tabindex="-1"` + `autofocus`), and the question the refusal names points at it: the rating's
  fieldset with `aria-describedby`, a comment box with `aria-invalid`, `.input-validation-error` and
  `aria-describedby` (its help text, then the summary). Only that question.
- No antiforgery check and no token in the form: whoever holds the link can post without a browser, so the check
  protected nothing, and a browser that lost the cookie had its answers answered with a bare 400.
- The page sets `Referrer-Policy: no-referrer` (the app's own policy sent its address, token and all, as the Referer of
  every asset it loads), `X-Robots-Tag: noindex, nofollow` and `Cache-Control: no-cache, no-store`, in every state.
- Declare what the post can leave out. The form mapper sets a dictionary the post carried no key for to null, whatever
  it was initialised to: a radio group with nothing chosen posts nothing. Reset each to empty before the page reads it.
- It says what the product guarantees and no more. MSF anonymity runs one way: the respondent sees their own
  questionnaire and nothing of anyone else's answers, counts or thresholds. The comment boxes' help text says a comment
  may reach the trainee word for word, which the promise that name and email are hidden does not cover.

```css
.account-form-container--wide  /* 44rem card for a questionnaire; less padding at ≤640px */
.error-summary                 /* a refused post's summary, focused on load; a focus ring when focused (T205) */
```

## app.css section order

`app.css` is one file, but it has mandatory sections and section headers. Keep them in this order so two sessions don't re-sort and conflict.

```css
/* ── Design tokens ─────────────────────────────────── */
:root { … }

/* ── Base ──────────────────────────────────────────── */
body, h1..h5, .page-subtitle

/* ── Header + search ──────────────────────────────── */
.header-container, .search-container, .search-input, .search-grid, .search-field, .search-hint

/* ── Tables ────────────────────────────────────────── */
.table-container, .clinic-table, .clinic-table tr.is-editing, .clinic-table--compact, .col-wrap, .col-fit, .col-actions, .clinic-table--inputs (+ @container inputs-grid), .actions-cell

/* ── Buttons ───────────────────────────────────────── */
.btn, .btn-{variant}, .btn-sm, .btn-xs, .btn-outline

/* ── Forms ─────────────────────────────────────────── */
.form-container, .form-grid, .form-group, .full-width, .form-control, .form-select, .form-select-sm, .form-check, .check-grid, .scale-choices, .form-actions, .account-form-container(--wide)

/* ── Alerts ────────────────────────────────────────── */
.alert, .alert-{kind}, .error-summary

/* ── Validation ────────────────────────────────────── */
.validation-message, .validation-summary-errors, .input-validation-error (+ .invalid, [aria-invalid="true"], forced colours), .field-warning

/* ── Cards ─────────────────────────────────────────── */
.detail-card, .detail-card--{variant}

/* ── Dashboard grid ───────────────────────────────── */
.dashboard-grid, .dashboard-span-{N}

/* ── Details grid ─────────────────────────────────── */
.details-grid (+ responsive)

/* ── Pager ─────────────────────────────────────────── */
.pager, .pager-info, .pager-actions, .pager-page-size, .pager-page-size-label, .pager-page-size-select

/* ── Password toggle ──────────────────────────────── */
.password-wrapper, .password-toggle-btn

/* ── State panels / skeletons ─────────────────────── */
.state-panel-title, .state-panel-copy, .skeleton, @keyframes skeleton-pulse

/* ── Utilities ─────────────────────────────────────── */
.shadow, .text-center, .mb-3, .visually-hidden

/* ── Accessibility ────────────────────────────────── */
fieldset, fieldset legend
```

When a new section is needed (say `/* ── Badges ── */`), add its heading in alphabetical-ish order inside the existing block and keep the rest of the file untouched.

## Non-negotiables

- One `app.css`. One design system. Component-scoped `.razor.css` only for the layout shell and NavMenu.
- No raw hex colours outside `:root`. No raw spacing outside the `--space-*` scale.
- No `<table class="table">`. Use `.clinic-table` wrapped in `.table-container`.
- Every list page renders loading / empty / error explicitly via `StatePanel` or equivalent.
- Every form page uses `.form-container` + `.form-grid` + `.form-actions`.
- Every dashboard page uses `.dashboard-grid`.
- `IScopedSender` only in interactive components.
- No Bootstrap Icons font. Inline SVG or CSS background-image data URIs.
- No MudBlazor, no Radzen, no jQuery. If a component needs JS, write a 10-line module under `wwwroot/js/` and import it with `IJSRuntime`.
- **Ask before widening the contract.** New component classes and new tokens are fine; silently deleting or renaming existing ones is not.

## Where this lives in the task graph

- **T010** ships the shell, `app.css` (every section above), `MainLayout`, `NavMenu`, and every `Components/Shared/*` component referenced here. Definition of Done for T010 includes "the list, detail, form, dashboard, and account page patterns above each render cleanly with stub data". Until T010 is done, the rest of the web surface has nothing to lean on.
- **T011** consumes `dashboard-grid` and `detail-card` only. No new global classes.
- **T019** consumes `detail-card`, `form-container`, `form-grid`, `form-actions`, `.btn`, `.clinic-table`, `.validation-summary-errors`. The builder's "Form tab" uses `.detail-card` for each section card and `.detail-card--interactive` for the hoverable add-field affordance. The JSON tabs use a `<textarea class="form-control" style="font-family: monospace">` inside a `.form-container`.
- Future UI tasks: open this file first. If a pattern you need isn't here, add it here in the same PR as the feature and reference the new section from the task file.

## Reference material

Two read-only reference trees informed this design system. Both were deleted from the worktree on 2026-09-20, long after T010 landed and `app.css` outgrew them. Where they went:

- **ClinicAssist.NET** — the gold standard the token names, class names, spacing scale and layout grid were ported from. Live working copy at `C:\Users\Renier\ClinicAssist.NET`; the files that mattered were `src/ClinicAssist.Web/wwwroot/app.css`, `Components/Layout/MainLayout.razor.css` and `Components/Layout/NavMenu.razor.css`. Copy structure, not palette.
- **The old Wombat** — its GUI was never a gold standard (Bootstrap-coupled tables, ad-hoc inline styles, no design-token discipline) but it **did ship and work**, and its `Views/` folder is the record of which pages users actually navigated for the Wombat-specific workflows ClinicAssist has no equivalent of (EPA management, curriculum items, activity builder, committee views). Recover it from this repo's history: `git worktree add ../wombat-old 55a92c6`.

Neither is needed to work on the design system now — `app.css` is the contract, and this file describes it in full.

## Historical context

The original ClinicAssist `app.css` was ~570 lines by the time its product shipped. Wombat's current one is 37. Every UI task that doesn't explicitly think about the design system leaks hand-rolled styles into its own `.razor` file, and six months later nothing matches. Porting the ClinicAssist skeleton in one go under T010 is cheaper than fighting that drift task by task.

The palette is still TBD. Everything else — token names, class names, spacing scale, layout grid — is locked to the ClinicAssist shape on purpose. Change the colours in `:root`, keep everything else.
