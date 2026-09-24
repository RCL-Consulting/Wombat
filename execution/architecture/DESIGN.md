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
│ │ gradient     │ │ article.content.px-4                 ││    │
│ │              │ │   @Body                              ││    │
│ │              │ │                                      ││    │
│ └──────────────┴─────────────────────────────────────────┘    │
└───────────────────────────────────────────────────────────────┘
```

- Above `641px`: flex-direction row, sidebar `250px` sticky full-height, top-row sticky.
- At or below `640.98px`: flex-direction column, nav collapses behind a `.navbar-toggler` checkbox.
- `top-row` right-aligns an "About" / profile / logout cluster. On narrow screens it justify-betweens.
- Below the layout, a fixed `#blazor-error-ui` banner renders on hub errors with a Reload + dismiss affordance. Copy ClinicAssist verbatim.

Copy `MainLayout.razor.css` from ClinicAssist with the class names unchanged. Only the gradient tokens differ.

## The NavMenu

- Brand row at the top with `Wombat` wordmark (no icon until a logo exists).
- Nav items are `<NavLink class="nav-link">` inside `<div class="nav-item px-3">`, wrapped in `<AuthorizeView>` blocks so each item only renders for authorized roles.
- Active and hover states are in `NavMenu.razor.css`: `rgba(255,255,255,0.37)` for active, `rgba(255,255,255,0.1)` for hover, `#d7d7d7` default text.
- Icons are inline SVG background-images on a sized `<span>` (the "CSS background-image SVG" pattern from ClinicAssist) — **no Bootstrap Icons font**. Put one CSS rule per icon under `NavMenu.razor.css`.
- Logout is a `<form action="/account/logout" method="post">` with an `AntiforgeryToken`, rendered as a full-width `.nav-logout-button`.
- Mobile: a checkbox-backed `.navbar-toggler` controls visibility. No JavaScript.

The nav item list is role-driven. The initial set:

| Role                        | Items                                                    |
|-----------------------------|----------------------------------------------------------|
| Everyone (authenticated)    | Home, My Account, Logout                                 |
| Trainee                     | Activities, My Activities, MSF Reports, Committee Reviews, My Progress |
| PendingTrainee              | Activities, My Activities                                |
| Assessor                    | Activity Inbox, Recent Activities                        |
| Coordinator                 | Invitations, Stalled Activities                          |
| SpecialityAdmin / SubSpec.  | Programme Trainees, Curriculum, STAR Review Queue        |
| InstitutionalAdmin          | Institution, Specialities, Users                         |
| Administrator               | Institutions, Invitations, Users, Activity Types, System |
| CommitteeMember             | Programme Trainees (read-only)                           |

MSF Reports, Committee Reviews and My Progress sit in their own Trainee-only `AuthorizeView`, outside the block
shared with PendingTrainee, because their pages do not admit a pending trainee (T141). A link goes in the shared
block only if its page admits PendingTrainee; `NavMenuAuthorizationTests` checks every link a trainee or pending
trainee is offered against its page's `[Authorize]`.

New items go in this table and then in `NavMenu.razor`, not anywhere else.

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

## Table system

Two classes, two uses:

- `.clinic-table` — the canonical list table. Header uses `--header-bg`, rows separate with `--border-color`, hover uses `--hover-bg`. Wrap every table in `<div class="table-container shadow">` for the rounded surface + horizontal scroll on narrow screens.
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

A grid whose every row is always a set of inputs, such as a scale's rungs on `EntrustmentScaleEdit`, is a different
pattern and keeps its controls in the cells. Each of them still needs an accessible name; a column header does not
give one.

**Many columns.** A table's narrowest width is its columns' longest words plus 2 × `--space-md` of padding per column.
Nine columns spend 288px on padding alone. The curriculum items table needed 1018px against the 907px its container has
at 1280px, and nothing in it was too wide. `.clinic-table--compact` halves the cell padding (a spanning cell keeps the
full padding, because it holds a form). That brings the table to 874px. Reach for it on a table of eight or more
columns, and measure a table at 1280px against the longest real values before adding a column.

```css
.clinic-table--compact /* cell padding --space-sm; a td[colspan] keeps --space-md (T176) */
.is-editing            /* on a clinic-table row: the item open for editing and its form row below (T176) */
```

A table **grouped by one column** puts each group in its own `<tbody>`, and the group's first row opens with
`<th scope="rowgroup" rowspan="n">` naming it; `app.css` aligns that label to the top of its group. No new class. The
committee review's evidence snapshot is one table per EPA, grouped by instrument (T167). A card that holds such a table
is an `article.detail-card--compact` whose `<h4>` it names with `aria-labelledby`.

## Form system

```css
.form-container   /* surface card with padding, border, shadow */
.form-grid        /* grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap 1.5rem */
.form-grid--wide  /* minmax(350px, 1fr) — wide sections that must not truncate */
.form-group       /* flex-column, gap .5rem, label on top of input */
.full-width       /* grid-column: 1 / -1 */

.form-control     /* padding .75rem, border --border-color, radius 6px */
.form-select      /* native <select> styled with a chevron data-URI */
.form-select-sm   /* compact variant */

.form-actions     /* flex, justify-end, gap .75rem, padded-top, top border */
.scale-choices    /* one radio per line for a rating scale's points, lowest first (T205) */
.workflow-action-reasons /* list under a workflow action row: why a disabled action cannot be taken (T107) */
.stage-minima     /* two-column grid: training year | rung picker, one row per year (T125 curriculum minima) */
```

**Rules:**

- Every form is inside a `.form-container`. Every form's submit/cancel cluster is a `.form-actions` row at the bottom.
- `<FormField>` wraps a `<label for="…">` + input slot + `.validation-message` target. Its help text carries the id
  `FormField.HelpTextId(InputId)` (`{InputId}-help`); the input in the slot is the caller's, so the caller names it
  with `aria-describedby` (T205, the MSF comment boxes).
- Inputs default to `.form-control`. Selects use `.form-select` (never native unstyled).
- Validation summaries render as `.validation-summary-errors` (red panel) at the top of the form. Per-field errors render as `.validation-message` under the field.
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
.detail-card--emphasis        /* left 4px solid accent stripe, --secondary-color */
.detail-card--warning         /* left 4px solid accent stripe, --warning-color */
```

- `.detail-card h3` has a bottom border and primary colour — gives the card a titled strip.
- Cards are the building block of dashboards and detail pages. Anywhere you want to group fields inside a page, reach for a card.
- The empty-state card uses a dashed border so it reads as "nothing yet" at a glance.

## Dashboard layout grid

Dashboards use one shared grid so every role page looks like the same product.

```css
.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
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
under the College's D14 rule, show the count and the date targets start, never a fraction and bar.

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
.input-validation-error      /* adds --danger-color border to an input */
.field-warning               /* inline NON-blocking warning under a field, input accepted as typed: --warning-bg, --warning-color left stripe, body text, .85rem (T160 late filing) */
```

- `StatePanel.razor` renders three canonical states: loading (skeletons), error (`.alert .alert-danger`), empty (`.detail-card--empty` + optional CTA).
- Every list page handles all three states explicitly. **No more "Loading…" plain text** — that pattern is dead.

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

Used on activity state indicators in dashboard list cards and activity tables.

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
period judges that period's year, not the next one. A target the curriculum sets no level for (no per-stage map, or a
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
- Required fields show a visual `*` plus `aria-required="true"`.
- `.visually-hidden` is available for screen-reader-only copy.
- `:focus-visible` uses `--focus-ring`. Never remove focus outlines without replacing them.
- Up/down reorder buttons (T019) are keyboard-focusable `<button type="button">` with `aria-label="Move field up"`.
- `<fieldset>` is reset to no border/padding and its `<legend>` styled as a heading — this is the semantic grouping for multi-field clusters.

## Icons

- One `Icon.razor` wraps `<svg>` + `<use href="/icons/{name}.svg#i" />` or inline path data.
- Icons live under `src/Wombat.Web/wwwroot/icons/` as individual SVG files, copied from Lucide (MIT licensed, compatible with AGPL-3.0).
- Nav icons are the exception — they use CSS background-image data URIs (`NavMenu.razor.css`) so hovering repaints the whole strip cheaply.
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
- **Role dashboards** live under `Components/Pages/Dashboards/` (`AdministratorDashboard`, `InstitutionalAdminDashboard`, `SpecialityAdminDashboard`, `SubSpecialityAdminDashboard`, `CommitteeMemberDashboard`, `CoordinatorDashboard`, `AssessorDashboard`, `TraineeDashboard`). Each one is a child component — **no `@page` directive**, **no `<PageTitle>`**, **no `<PageHeader>`**. Adding any of those would duplicate Home's header.
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

### Account / auth page

```
<div class="account-form-container">
  <h2>Sign in</h2>
  @if (Error is not null) { <div class="alert alert-danger">@Error</div> }
  <form method="post" action="/account/login/submit">
    <div class="mb-3"> label + .form-control </div>
    …
    <button type="submit" class="btn btn-primary">Sign in</button>
  </form>
</div>
```

`.account-form-container` is a 400px centred card with a wide top margin — the shape ClinicAssist uses for its login/register/change-password pages.

### Anonymous static page (the MSF respondent page, T205)

A page for a stranger holding an emailed link, who never signs in: `/msf/respond`, where an MSF respondent answers. It
is the account page's shape, widened for a questionnaire, and it is the one page that is **not interactive**:

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

- `App.razor` renders `Routes` and `HeadOutlet` with no render mode for a page marked `[ExcludeFromInteractiveRouting]`,
  and `InteractiveServer` for every other. Static, the page opens no circuit, so the Blazor hub's fallback policy (T181)
  cannot stop it, the link's rate limit sees the submit (an HTTP post, not a SignalR message), and it works without
  WebSockets or script.
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
.table-container, .clinic-table, .clinic-table tr.is-editing, .clinic-table--compact, .actions-cell

/* ── Buttons ───────────────────────────────────────── */
.btn, .btn-{variant}, .btn-sm, .btn-xs, .btn-outline

/* ── Forms ─────────────────────────────────────────── */
.form-container, .form-grid, .form-group, .full-width, .form-control, .form-select, .form-select-sm, .form-check, .check-grid, .scale-choices, .form-actions, .account-form-container(--wide)

/* ── Alerts ────────────────────────────────────────── */
.alert, .alert-{kind}, .error-summary

/* ── Validation ────────────────────────────────────── */
.validation-message, .validation-summary-errors, .input-validation-error, .field-warning

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
