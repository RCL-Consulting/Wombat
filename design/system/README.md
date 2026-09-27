Wombat is a work-based assessment tool for medical specialists. It is built around EPAs (Entrustable Professional Activities): registrars file observed work, consultants rate it, and a committee reads the evidence each period and issues STARs (Statements of Awarded Responsibility). A national College owns the catalogue of EPAs, entrustment ladders and curriculum versions; an institution adopts it and admits its registrars against it. The product is a dense, form-and-table web application for clinicians and programme staff, and its look is quiet on purpose: white cards on a pale grey page, one blue for actions, a navy-to-violet sidebar, and the words doing the work.

## This system records today's language

Wombat's GUI is being restructured (decision W-008, 2026-09-27): the shell, the navigation, the role dashboards, the page shapes and the task flows may all change, and the emails and PDFs are in scope. Nothing has been redesigned yet; the pilot is flow 01, the shell. Treat everything here as the starting point, not the destination:

- The tokens, classes and components below are what `app.css` and the Razor components ship today, exactly.
- Whatever a redesign has not reached keeps this contract, its class names and the tests that pin them.
- A redesign may replace any of it, but it must keep the invariants listed under "Building a design Wombat can ship".

## Who uses it

Ten roles. A person may hold several and sees each role's links in turn.

| Role | Comes to Wombat to |
|---|---|
| Administrator | keep the platform, the Colleges and the scales; create institutions; run jobs; read the audit trail |
| CollegeAdmin | keep specialities, EPAs and curriculum versions; write the College's activity types |
| InstitutionalAdmin | adopt the curriculum; invite, admit and manage people; build activity types |
| SpecialityAdmin, SubSpecialityAdmin | watch the programme; form panels; schedule reviews; keep the STAR register |
| Coordinator | run MSF (multi-source feedback) campaigns; chase stalled work; schedule reviews; decide data-rights requests |
| CommitteeMember | sit reviews; stage and ratify STARs; resolve appeals |
| Assessor | rate, decline, return and discuss registrars' work |
| Trainee | file activities; read progress, decisions and STARs; appeal; export the portfolio |
| PendingTrainee | wait to be admitted |

A former trainee holds no role but keeps a read-only record. Anonymous visitors register from an invitation, answer an MSF questionnaire from an emailed link, or verify an exported PDF.

## Voice and content

Wombat writes plainly, for busy clinicians, in full sentences. No emoji, no exclamation marks, no marketing. Headings, buttons and messages are sentence case ("Create curriculum", "Review and release", "Confirm revocation"). Nav links are the exception: they are Title Case page names ("Activity Inbox", "My Committee Reviews").

- **People by name, never by id.** "Thandi Nkosi", not a user id. Only the audit log, data-rights records and the anonymous verification page show ids.
- **States and moves by their label, never their key.** "Awaiting supervisor", not `submitted`. A move is named as its button is ("Sign Off"). A badge's words are the state; its tint only repeats it.
- **A figure is a count against a target for a named window.** "2 of 3 this semester", "1 of 1 in 2026", "4 of 9 trainees met this period's target". Never a lifetime total or a mean percentage. Say "training year 3" for a stage and "Semester 2, 2026" for a quota window, never a bare "year". Where no target applies, say why ("no target (your programme ended part-way through)"), with no fraction or bar.
- **A refusal is shown in its own words.** Pages print a caught refusal through `RefusalText.Of(exception)`, never a log-shaped "Validation failed: -- Members: …". A refusal that arrives by URL travels as a code, and the page chooses the sentence: "Your session has ended. Please sign in again."
- **Say what an action will do, and what it cannot undo.** A confirmation names its object and its consequence: "Deactivate this EPA?" / "It can no longer be chosen for a new activity, and it stops being a target on every progress page and dashboard …"
- **A disabled action says why, in visible text, starting with its name.** "Sign Off: Needs Overall level, which you cannot fill in here." A tooltip alone is not enough.
- **A row with no action says why.** "Set by the College." "Staged below." "Editing below." Never a blank cell.
- **Every row action names its row.** The visible label stays short ("Edit"); its accessible name carries the row: "Edit PAED-001 — Providing paediatric emergency care to children". "Open Mini-CEX for Thandi Nkosi, PAED-003, encounter date 2026-09-01".
- **An EPA is "Code — Title"**, with "(no longer in use)" in muted text beside one not in force.
- **Out of scope is "not found".** A record in another institution is never "forbidden"; it does not exist for that reader.
- **Dates are calendar dates; times are South African time**, shown with the zone.
- **Empty states name what is missing and what would change it.** "No pending items." "Nothing here yet" / "There is no data to show yet."

## Visual foundations

### Colour

- Set the page on `background-color` and every card, form, table and dialog on `surface-color`. Body text is `text-color`; secondary text, help and labels are `muted-text`; links are `link-color`.
- `secondary-color` is the one action colour: the primary button, outline buttons, the rule under the page title, the active tab, progress fills, focus-adjacent stripes. `primary-color` is ink for emphasis (the dashboard figure, the account wordmark). Do not swap them (DESIGN.md § Design tokens).
- Hairlines are `border-color` (decorative, 1.30:1); control edges are `input-border`. Table heads sit on `header-bg`; rows hover to `hover-bg`.
- Status comes in four semantic pairs, a colour and its tint: `success-color`/`success-bg`, `warning-color`/`warning-bg`, `danger-color`/`danger-bg`, `secondary-color`/`info-bg`. Put `text-color` on a tint; use the semantic colour as a border, stripe or dot. The source's alerts and state badges still put the semantic colour on its own tint, which fails AA (see Contrast).
- The sidebar is the brand's one strong surface: a 180deg gradient from `sidebar-gradient-start` to `sidebar-gradient-end`, reached at 70%. Its item text is rgb(215 215 215), white when active; active items sit on white at 37%, hovered on white at 10%, and the brand row on black at 40%. Those four values are written in NavMenu.razor.css, not in :root.
- `accent-color` and `info-color` are declared but read by nothing.
- There is one theme, Light. Wombat has no dark mode and no `prefers-color-scheme` rule.
- Never write a colour outside `:root`: a new colour is a new token first. The source breaks this in five places (the auth page's gradient, the dialog backdrop at black 35%, the nav values above, and the error bar's `lightyellow`).

### Type

- Body text is the system stack `body`: "Segoe UI", Tahoma, Geneva, Verdana, sans-serif. Segoe UI ships only with Windows; other systems fall back down the stack. It cannot be self-hosted, so it has no font file here.
- Fraunces (`display`, the one shipped face, variable 100–900, OFL) sets the word "Wombat" in the lockup and nothing else: weight 500, `font-optical-sizing: auto`, `font-variation-settings: "SOFT" 0, "WONK" 1`. Use `wordmark` in the sidebar and `wordmark-large` on account cards.
- Headings are all weight 600: `h1` 1.5rem (one per page, from PageHeader, never larger), `h2` 1.25rem, `h3` 1.1rem (card titles), `h4` 1rem, `h5` 0.9rem. The sign-in card's title is `account-title` (2rem).
- Small text steps down: `subtitle` 0.9rem (help text, pager info), `small` 0.85rem (validation messages, meta lines), `badge-label` 0.75rem bold. Buttons are `button` 0.95rem/500, `button-sm` 0.8rem, `button-xs` 0.7rem.
- A dashboard figure is `metric`: 2rem bold in `primary-color`. Stored text shown verbatim is `code` in Consolas.
- app.css sets no line-height; the browser's `normal` applies.

### Space, radii and elevation

- Space on the `--space-*` scale only: `space-xs` 4px, `space-sm` 8px, `space-md` 16px, `space-lg` 24px, `space-xl` 32px, `space-2xl` 48px. A field's parts are `space-sm` apart; grid gaps and card padding are `space-lg`; forms pad by `space-xl`.
- Radii grow with the thing: `radius-4` for bars and skeletons, `radius-6` for controls and alerts, `radius-8` for cards, `radius-12` for tables, forms and dialogs, `radius-pill` for badges and tabs. app.css writes these as literals; there are no radius tokens in :root.
- Elevation is shallow: cards rest on `detail-card-shadow`, tables take `shadow-utility`, account cards `account-card-shadow`. A clickable card lifts 2px to `interactive-hover-shadow`. All use `shadow-color`.

### Borders, states and motion

- Focus is a 2px solid `focus-ring` outline, offset 2px, on every `:focus-visible` button, input and link. The h1 that Blazor focuses after navigation shows no ring.
- Disabled is 65% opacity. A button whose own action is running is not disabled: it keeps the focus and says so with `aria-disabled="true"` (`InFlight.AriaDisabled`), with the progress cursor. Every other button that action would race is disabled.
- An invalid field is marked twice: a `danger-color` border and a 4px stripe down its left (`invalid-stripe`), so the state is not colour alone; forced-colors mode turns the stripe into a border.
- A row open for editing and the form row under it share a `secondary-color` stripe (`editing-stripe`).
- Motion is small and functional: buttons dim to brightness 0.95 on hover (0.15s), interactive cards lift 2px (0.15s), skeletons pulse over 1.2s. There is no `prefers-reduced-motion` rule yet.

### Layout

- The shell: a 250px sticky sidebar beside the main column from 641px up (`sidebar-width`, `breakpoint-wide`); a sticky 3.5rem top row holding the user's name and Sign out; the page in an `article` with a side gutter at every width (16px on a phone, 2rem left and 1.5rem right from 641px). Below 641px the sidebar stacks on top and the nav folds behind a CSS-only toggler.
- Grids are auto-fit, and a column asks for its minimum or the whole container, whichever is less: `minmax(min(250px, 100%), 1fr)`, never a bare `minmax(250px, 1fr)`. Forms use `.form-grid` (250px), dashboards `.dashboard-grid` (320px), filters `.search-grid` (220px).
- A detail page is `.details-grid`, 1fr 2fr, one column at 900px and below.
- Every page works at 390px with no sideways scroll; a wide table scrolls inside its `.table-container`.

## Contrast: the pairs that fail AA today

These are measured from the tokens (task T322). They are kept exactly as the source has them and flagged here and in each token's note; a design should fix them, not copy them.

| Where | Pair | Ratio | Needs |
|---|---|---|---|
| `.alert-warning`, `.badge-accepted` | `warning-color` on `warning-bg` | 2.42:1 | 4.5:1 |
| `.alert-success`, `.badge-completed` | `success-color` on `success-bg` | 2.55:1 | 4.5:1 |
| `.alert-danger`, `.badge-declined`, `.validation-summary-errors` | `danger-color` on `danger-bg` | 3.57:1 | 4.5:1 |
| `.validation-message`, `.text-danger` | `danger-color` on `surface-color` / `background-color` | 3.82 / 3.62:1 | 4.5:1 |
| `.btn-success` label | `surface-color` on `success-color` | 2.87:1 | 4.5:1 |
| `.btn-danger` label | `surface-color` on `danger-color` | 3.82:1 | 4.5:1 |
| Focus ring on the page | `focus-ring` on `background-color` | 2.99:1 | 3:1 |
| Input borders | `input-border` on `surface-color` / `background-color` | 1.49 / 1.42:1 | 3:1 |
| `.status-dot.ok`, `.status-dot.warn` | `success-color` / `warning-color` on `surface-color` | 2.87 / 2.57:1 | 3:1 |
| A complete progress bar | `success-color` on the `hover-bg` track | 2.73:1 | 3:1 |

What passes: `text-color` 12.63:1 on white and above 11:1 on every tint, `muted-text` 4.57 to 5.09:1, `link-color` 6.70:1, the primary button 4.86:1, `.alert-info` and `.badge-submitted` 4.55:1, the focus ring 3.15:1 on white and 4.46:1 or more on the sidebar. T322's proposed fix: body text on every tint (as `.badge-standing-*` and `.field-warning` already do), darker button fills, `secondary-color` as the ring, and an input border of at least 3:1.

## Iconography

- Icons are Lucide line icons (MIT), one SVG per icon in `wwwroot/icons/`, rendered by `Icon.razor` as `<svg class="icon" width="16" height="16" aria-hidden="true"><use href="/icons/{name}.svg#i"/></svg>`. The `.icon` class draws them: no fill, `currentColor` stroke 2px, round caps and joins. So an icon takes its colour from the text around it.
- The set is 25 icons (the Icons group); 20 are in use. Name a new one by its Lucide name, and give its root `id="i"`.
- The Bootstrap Icons font is not loaded: `<i class="bi bi-*">` renders nothing. Never use icon fonts, emoji or pictures as icons.
- Icons are decorative (`aria-hidden`): the words beside them carry the meaning. Sizes: 16px by default, 18px in a card title, 20px in the nav.

## The mark

The Wombat mark is a white wombat face with a navy cross for a nose, rising from a dark burrow mound on a blue disc (`wombat-mark.svg`, 64-unit square, transparent corners). It is multi-colour: show it as an `<img>`, never as an `Icon` glyph, and never recolour it. The lockup is the mark plus the Fraunces wordmark: 40px mark and `wordmark` in white on the sidebar; 56px mark and `wordmark-large` in `primary-color` on an account card. `wombat-tile.svg` is the full-bleed square the app-icon PNGs are rendered from. `favicon.svg` is an older drawing (eyes, a triangle nose) that still serves as the browser favicon.

## Building a design Wombat can ship

Every design must stay buildable in this stack:

- **Blazor Server** (.NET 10, Razor components). A signed-in page is interactive: every click is a server round trip, so prefer explicit actions to per-keystroke behaviour.
- **Static pages for signed-out visitors.** Sign-in, register, forgot password, link account, access denied, not found, the MSF questionnaire (`/msf/respond`) and portfolio verification (`/portfolio/verify`) are plain server-rendered HTML with form posts: no client behaviour beyond a small script module, no live validation, a CSS-only nav toggle.
- **A strict CSP.** Fonts, scripts, styles and images come from the site itself (images may be `data:` URIs). Self-hosted fonts only, as woff2 with a GPLv3-compatible licence; no Google Fonts, no CDN scripts or styles, no inline scripts or `onclick`, no third-party calls or avatars.
- **No CSS framework.** No Bootstrap, Tailwind, MudBlazor, Radzen or jQuery. Every class a page names is defined in `app.css` (or a layout's `.razor.css`); a new class is written there first.
- **Tokens only.** No raw hex or rgb outside `:root`; spacing on the `--space-*` scale; no inline `<style>` in a page.
- **Keep the invariants:** every nav link opens a page that admits the role, and no two share a label; one `<h1>` per page; the focus moves to an action's result; row actions are named per row; field help is linked by `aria-describedby`; an invalid field is not shown by colour alone; static pages stay static; WCAG 2.1 AA throughout, with targets at least 24px.
- **Design the framework's own states too**, which no page file shows: the active nav item, field validation (border, stripe, message, summary), the reconnect dialog, the in-app error bar, access denied, not found and session ended.

Check a design at 1280×800 and at 390×844.

## Not synced

- Tokens not placed: `--font-display-settings` ("SOFT" 0, "WONK" 1), which has no token shape, is carried in `components/bundle.css`. Radii, shadows and the `size` family are app.css's literal values, not :root properties, named here by value.
- Components not built: the trajectory chart, the entrustment standing and MSF coverage panels, the EPA target coverage list, `EpaLabel`, the activity form and detail renderers, the reconnect dialog and the password toggle on its own (it appears in AuthLayout). Previews are static renditions of the Razor markup styled by `components/bundle.css`; there is no JavaScript bundle, because the components are Razor, not React.
- Not carried: `favicon.ico` (the store takes no .ico), `site.webmanifest`, `wombat.js` and `js/*.js`.
