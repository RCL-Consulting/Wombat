Wombat is a work-based assessment tool for medical specialists. It is built around EPAs (Entrustable Professional Activities): registrars file observed work, consultants rate it, and a committee reads the evidence each period and issues STARs (Statements of Awarded Responsibility). A national College owns the catalogue of EPAs, entrustment ladders and curriculum versions; an institution adopts it and admits its registrars against it. The product is a dense, form-and-table web application for clinicians and programme staff, and its look is quiet on purpose: white cards on a pale grey page, one blue for actions, a navy-to-violet sidebar, Source Sans 3 for the words, and the words doing the work.

## What this system records

Wombat's GUI is being restructured (decision W-008): the shell, the navigation, the role dashboards, the page shapes and the task flows may all change, and the emails and PDFs are in scope. Each flow is designed in Claude Design on this system, built in Razor and `app.css`, and replayed before it counts as done.

**Flow 01, the shell, is redesigned and built** (T335, 2026-09-27, commit `b347e11c`). This system now records the language as it ships from that build. Flow 01 changed two kinds of thing:

- **The frame**, which is new: the sidebar with the acting role's head and switch, the navigation per acting role, the account row as the top bar, the phone bar and its CSS-only menu, breadcrumbs from the owner table, Home's header, the system pages (Access denied, Page not found, the error page), the reconnect dialog and the error bar (components MainLayout, MainLayoutPhone, NavMenu, PageHeader, Breadcrumbs, ActingRoleSwitchAlert, DashboardCard's frame, AccessDenied, NotFound, ErrorPage, ReferenceBlock, ReconnectModal, ErrorBar).
- **What every page inherits**, which every page now has: the round-3 token sheet with T322's contrast fix (tokens.json), Source Sans 3 and the line heights, 36px buttons with an outline filled by the surface, alerts and badges with body text on the tint, the validation icon and stripe, the page header's spacing, the tab title "<Page> · Wombat", and sentence case in every title, heading and nav label.

**Everything else still carries the pre-restructure language**: the structure and copy of the page bodies of flows 02 to 20, and the emails and PDFs. In the redesign's order: 02 sign-in and account (AuthLayout, My account), 03 filing an activity, 04 the assessor's inbox, 05 the trainee's progress, 06 programme oversight, 07 the review sitting, 08 the committee calendar, 09 outcomes, STARs and appeals, 10 an MSF campaign, 11 onboarding, 12 people admin, 13 graduation and exit, 14 data rights, 15 an institution adopting the catalogue, 16 the College's catalogue, 17 the activity type builder, 18 platform operations, 19 emails, 20 PDFs. Home's cards are kept as they were, less the duplicates flow 01 dropped; they belong to later flows. A redesign may replace any of it, but must keep the invariants under "Building a design Wombat can ship".

## Who uses it

Ten roles. A person may hold several, and **acts in one at a time**: the acting role chooses what the frame shows (the navigation, Home's dashboard, the "Acting as" head), and nothing else. **Access is the union of the roles held** and never changes with the acting role: a page any held role admits opens whatever the role, following a link never switches it, and Access denied is shown only for a page none of them opens.

| Role (as the product names it) | Comes to Wombat to |
|---|---|
| Administrator | keep the platform, the Colleges and the scales; create institutions; run jobs; read the audit trail |
| College admin | keep specialities, EPAs and curriculum versions; write the College's activity types |
| Institutional admin | adopt the curriculum; invite, admit and manage people; build activity types |
| Speciality admin, Sub-speciality admin | watch the programme; form panels; schedule reviews; keep the STAR register |
| Coordinator | run MSF (multi-source feedback) campaigns; chase stalled requests; schedule reviews; decide data-rights requests |
| Committee member | sit reviews; stage and ratify STARs; resolve appeals |
| Assessor | rate, decline, return and discuss registrars' work |
| Trainee | file activities; read progress, decisions and STARs; appeal; export the portfolio |
| Pending trainee | wait to be admitted; start logging activities |

- **The acting role** is the one stored with the account while the person holds it, else the first they hold in this order: Administrator, College admin, Institutional admin, Speciality admin, Sub-speciality admin, Committee member, Coordinator, Assessor, Trainee, Pending trainee. It is remembered per account, across sign-ins, never in the browser.
- **Switching** is one link, `/dashboard/switch/{role}?returnUrl=<local path>` (W-010), so an email can switch too: the sidebar's "Switch to …" (two roles) or "Change role" (three or more), and the result's "Switch back to …". The landing page says "You are now acting as Assessor." once, and takes the focus.
- **A former trainee** holds no role but keeps a read-only record: no "Acting as" head, and My progress among the personal links.
- **Home with no role** (a graduate, or an account whose last role was removed) has no subtitle, no header action and no dashboard: one full-width emphasised card. A graduate holding the `trainee_record` claim gets "Your training record" (`book`): "You hold no role at the moment. Your training record is kept, read-only: your progress in each period, and your portfolio to export.", with "Open My progress →". Anyone else gets "No role assigned" (`info`): "Your account holds no role at the moment, so there is nothing to show you here. An administrator can give you one.", with no link. It is the graduate's landing page, which flow 13 designs.
- Anonymous visitors register from an invitation, answer an MSF questionnaire from an emailed link, or verify an exported PDF.

## Voice and content

Wombat writes plainly, for busy clinicians, in full sentences. No emoji, no exclamation marks, no marketing.

- **Sentence case everywhere, the nav included.** The first word is capitalised, and after it only acronyms and proper nouns: EPA, EPAs, MSF, SSO, STAR, CPSA, PDF, Mini-CEX, DOPS, and College where it names the College. "Activity inbox", "My committee reviews", "Create curriculum", "Review and release", "Confirm revocation". A role is its label: "Committee member", never "CommitteeMember". **One exception ships:** a workflow move's label is its key in title case (`WorkflowTransition.LabelFor`: `sign_off` is "Sign Off", `return` "Return"), on its button, in its disabled reason and in the activity's history, because the workflow DSL declares no label. Flows 03 and 04 decide whether moves get sentence-case labels; until then quote a move as it ships.
- **A page is named once, in the same words, wherever it is named.** Its nav label, its `<h1>`, its breadcrumb and the stem of its tab are one string: "Activity inbox" in the sidebar, the heading and the trail, and "Activity inbox · Wombat" on the tab. The one exception is a record's own crumb, which PageHeader's `CurrentCrumb` may set to the record's name or id: a data-rights request's crumb is its id under the h1 "Data rights request", and the College admin on a College's specialities reads Home › Specialities › {College name} under the h1 "Specialities". The tab is always "<Page> · Wombat" (a middle dot, a space each side), and "Error: <Page> · Wombat" after a refused post. Home is "Home", with "{acting role} · Semester 2, 2026" under it (nothing, for someone with no role).
- **People by name, as stored, never by id and never with a title.** "Nomsa Mahlangu", "Thandi Zulu", "Pieter Smit": the account's first and last name, with no "Dr" or "Prof", or the email when there is no name. Only the audit log, data-rights records and the anonymous verification page show ids. The examples in this system use the scenario's cast (Nomsa Mahlangu and Sipho Ndlovu, first-year registrars; Anele Dlamini, Pieter du Plessis; Thandi Zulu, a consultant who sits on the committee and assesses; Mohammed Patel and Fatima Khumalo, assessors; Pieter Smit, the coordinator), never an invented name.
- **States and moves by their label, never their key.** "Requested", "Awaiting supervisor", not `submitted`. A move is named as its button is ("Sign Off", in the title case moves still ship with). A badge's words are the state; its tint only repeats it.
- **A figure is a count against a target for a named window.** "2 of 3 this semester", "1 of 1 in 2026", "4 of 9 trainees met this period's target". Never a lifetime total or a mean percentage. Say "training year 3" for a stage and "Semester 2, 2026" for a quota window, never a bare "year". Where no target applies, say why ("no target (your programme ended part-way through)"), with no fraction or bar.
- **A refusal is shown in its own words.** Pages print a caught refusal through `RefusalText.Of(exception)`, never a log-shaped "Validation failed: -- Members: …". A refusal that arrives by URL travels as a code, and the page chooses the sentence: "Your session has ended. Please sign in again." A load failure says nothing changed and offers the read again: "**Could not load your Home.** Nothing has changed. Try again, or come back in a few minutes." **Today only Home meets this** (DashboardFrame). Every list and detail page's load failure is StatePanel's `LoadError`, printed as the page gives it (often `exception.Message`, sometimes `RefusalText.Of`), with no Try again (StatePanel, Known gaps). Flows 02 to 18 should design their load failures on Home's pattern.
- **Say what an action will do, and what it cannot undo.** A confirmation names its object and its consequence: "Deactivate this EPA?" / "It can no longer be chosen for a new activity, and it stops being a target on every progress page and dashboard …"
- **A disabled action says why, in visible text, starting with its name.** "Sign Off: Needs Overall level, which you cannot fill in here." (the move's name as it ships, in title case). A tooltip alone is not enough.
- **A row with no action says why.** "Set by the College." "Staged below." "Editing below." Never a blank cell.
- **Every row action names its row.** The visible label stays short ("Edit"); its accessible name carries the row: "Edit PAED-001 — Providing paediatric emergency care to children"; "Mini-CEX (Paediatrics) for Nomsa Mahlangu".
- **An EPA is "Code — Title"**, with "(no longer in use)" in muted text beside one not in force.
- **Out of scope is "not found".** A record in another institution is never "forbidden"; it does not exist for that reader. Access denied names neither the page nor who may open it: "None of your roles (Committee member, Assessor) opens this page."
- **Dates are calendar dates; times are South African time, with the zone**: "2026-09-26 15:14 SAST".
- **The way back is "Go to Home".** Empty states name what is missing and what would change it: "Nothing is waiting for your rating." "Nothing here yet" / "There is no data to show yet."

## The shell

The frame every signed-in page sits in (MainLayout; DESIGN.md § Layout grid, § The NavMenu, § Acting role, § Breadcrumbs).

- **One banner.** A `<header>` that is the sidebar from 641px: the 56px brand cell (the mark at 32px and the Fraunces wordmark, the whole cell the link home), the role head ("Acting as" and the role, then the switch), and the navigation, whose list alone scrolls. Its last child, the account row, is fixed across the top of the page as a 56px light top bar: the person's name (a link to My account, cut at 28 characters) and one Sign out.
- **Below 641px** the header is the phone bar (the brand, "Acting as" and the role, Menu), and Menu opens the menu in the page's place, the account row pinned at its foot. The toggle is CSS only (a hidden checkbox and its label), because the shell must work with no circuit (see "Building a design Wombat can ship"); every navigation closes it.
- **The navigation is the acting role's**, Home first, flat up to eight links and grouped under headings above that (the Administrator's 17, the Institutional admin's 16), then My progress (for anyone with a trainee record) and My data rights under a rule. It never links to an unbuilt page, and holds no Sign out or My account.
- **At most one item is lit** (the owner table, `NavOwners`): a list lights itself (`aria-current="page"`), a page under a list lights that list for the acting role (`aria-current="true"`), and a page with no owner for the acting role lights nothing. Lit is a `nav-active-bg` fill, weight 600 and a 3px white bar. On My account the top bar's name is the current page.
- **Breadcrumbs follow the owner table** (NavMenu lists it): Home › the owning list › the page, the crumbs before the page's links in `link-color`, the page's own crumb in body text at weight 600; Home › the page where the acting role has no owner. None on Home, on a list the menu offers, on My account and the sign-in pages, and on the system pages; Change password alone of the account pages has one, Home › My account › Change password. Below 641px the trail folds to one 44px link to its parent, at weight 600, led by an 18px back chevron.
- **The page header** is the page's one `<h1>`, a subtitle and the actions on the heading's baseline, over a 2px `secondary-color` rule; the page starts 24px under it.
- **Dark chrome** (the sidebar, the phone bar, the signed-out bar) paints only with the `--nav-*` tokens and the gradient, and every control on it takes `nav-focus-ring`.

## Visual foundations

### Colour

- Set the page on `background-color` and every card, form, table and dialog on `surface-color`. Body text is `text-color`; secondary text, help and labels are `muted-text`; links are `link-color`, darkening to `link-hover`.
- `secondary-color` is the one action colour: the primary button, outline buttons, the rule under the page header, the active tab, progress fills, the focus ring, the info edge. `primary-color` is ink for emphasis (the dashboard figure, the account card's wordmark). Do not swap them.
- **Words on a filled button are `on-fill`**, never `surface-color`: dark mode will change one and not the other.
- Hairlines are `border-color` (decorative, 1.30:1); a control's edge is `input-border` (3.80:1). Two exceptions ship and are not the pattern: the activity type builder's unselected tab (`.tab-bar-tab`, pre-restructure, flow 17) is edged in `border-color`, and a clickable card (`.detail-card--interactive`) keeps the card's hairline and is told by its lift and pointer cursor. A new control takes `input-border`. Table heads and neutral badges sit on `header-bg`; rows hover to `hover-bg`.
- **Status comes in four semantic pairs**, a colour and its tint: `success-color`/`success-bg`, `warning-color`/`warning-bg`, `danger-color`/`danger-bg`, `secondary-color`/`info-bg`. **A semantic colour is never words on its own tint**: an alert's and a badge's words are `text-color` on the tint, and the kind's colour is the edge, the icon, the dot or the fill (T322). The one semantic colour used as words is `danger-color`, for a refusal on a light ground (a field's message).
- **Dark chrome** is the brand's one strong surface: a 180deg gradient from `sidebar-gradient-start` to `sidebar-gradient-end`, reached at 70% (the phone bar is the start as a solid; the signed-out bar the gradient at 90deg). On it: items at rest `nav-text`; the current and hovered item, the role's name and the bar's controls `nav-text-strong`; group headings, "Acting as" and the switch's edge `nav-group-label`; the current item's fill `nav-active-bg`, a hover `nav-hover-bg`, the brand cell `nav-brand-bg`, rules `nav-divider`. A translucent token is valid only over the ground it was measured on. `nav-text` on `nav-active-bg` is forbidden.
- A dialog's backdrop is `scrim`.
- One theme, Light: `color-scheme: light`. Dark mode is later (W-011).
- **No colour outside `:root`.** Every colour is a token first; the only exceptions are quoted SVG data URIs (the select's chevron, the alerts' icon masks) and a system colour in a Windows contrast theme. `Design/StylesheetRuleTests` fails on any other.

### Type

- **Source Sans 3** is the body face (`body`: "Source Sans 3", "Segoe UI", system-ui, sans-serif), self-hosted from its variable upright and italic WOFF2 files (weights 200 to 900). Body text is 1rem at line height 1.5; every control inherits the whole font (T328); figures in a table are tabular.
- **Fraunces** (`display`) sets the word "Wombat" and nothing else: weight 500, `font-optical-sizing: auto`, `font-variation-settings: "SOFT" 0, "WONK" 1` (`--font-display-settings`). `wordmark` 1.6rem in the sidebar and on the signed-out bar (at every width there), `wordmark-phone` 1.4rem on the phone bar, `wordmark-large` 2.4rem on the sign-in and MSF respondent cards.
- **Headings** are all weight 600 at line height 1.25: `h1` 1.5rem (1.375rem below 641px; one per page, from PageHeader, never larger), `h2` 1.25rem, a dashboard card's title and `h3` 1.1rem, `h4` 1rem, `h5` 0.9rem. The sign-in card's title is `account-title` (2rem). An empty state's title and a section's `legend` are not heading elements: `panel-title`, 1.1rem/600 at the body's 1.5.
- **Text at body size:** a field's `label` is unstyled body text (1rem/400); a field-level group's legend reads the same. A table head is `table-head` (1rem/600 on `header-bg`); a details list's term is `term` (1rem/500 in `muted-text`), the only weight 500. The body face's weights are 400, 500, 600 and 700.
- **The shell's text:** a nav label 0.9375rem (600 when current), a group heading 0.8125rem/600, "Acting as" 0.8125rem/600 and the role 1.0625rem/600, the switch and Change role's options 0.9375rem, the phone Menu toggle 1rem/600, the signed-out bar's Sign in 0.95rem/600. The trail is 0.9rem: `crumb` links in `link-color`, `crumb-current` 600 in `text-color`, `crumb-back` the phone's one 600 link.
- **Small text steps down:** `subtitle` 0.9rem (help, the trail's separators, the pager), `small` 0.875rem (a field's refusal), `text-sm` 0.85rem (meta lines, a field's warning), `badge-label` 0.75rem/600. Buttons are 0.95rem/600, `.btn-sm` 0.85rem/600, `.btn-xs` 0.7rem; `.form-select-sm` is 0.85rem/400.
- A dashboard figure is `metric`: 2rem/700 in `primary-color`, line height 1. Stored text shown verbatim (`.code-block`), and a failure's reference, are `code` in `mono` (Consolas) at 0.875rem. A bare `<code>` is unstyled (app.css has no `code` rule): the 20 or so that pages write render in the browser's generic monospace at its reduced size, not in `code`.

### Space, radii, elevation and motion

- Prefer the `--space-*` scale: `space-xs` 4px, `space-sm` 8px, `space-md` 16px, `space-lg` 24px, `space-xl` 32px, `space-2xl` 48px. A field's parts are `space-sm` apart; grid gaps and card padding are `space-lg`; forms pad by `space-xl`. Draw the layout between blocks on it.
- **The shell is not built on the scale alone.** Three steps off it, written as literals because `:root` has no token for them, recur inside rows and controls (tokens.json's spacing note lists every one): **10px** in the nav rows and Change role's options (sides and icon gap), the role head's top and the brand's mark-to-word gap; **12px** for the header's rule under the subtitle, the padding of an alert, the error bar and a phone's reference block, `.form-actions`' gap, the system pages' icon and panel gaps, a control's sides, and the Menu toggle's and a phone nav row's sides; **6px** between crumbs, above the role switch, around the personal links' rule, and a control's top and bottom. A design may use 6, 10 and 12px inside a row or a control, as the shell does.
- Radii are five tokens that grow with the thing: `radius-sm` 4px (nav rows, skeletons, bars), `radius-md` 6px (controls, buttons, alerts), `radius-lg` 8px (cards), `radius-xl` 12px (tables, forms, dialogs), `radius-pill` (badges, tabs). A status dot is 50%.
- Elevation is shallow and three tokens: `shadow-raised` (cards, forms, the signed-out cards, and a table in DataTable or in a hand-written `.table-container` that adds `.shadow`; nine hand-written ones do not, and sit flat), `shadow-dialog` (the reconnect and confirm dialogs), `shadow-bar` (the error bar). A clickable card lifts 2px to `0 4px 12px` of `shadow-color`, the one shadow not named.
- `motion-fast` (150ms) is every transition in app.css and the shell's rows: a button's dim, a card's lift, a nav row's, the switch's and Change role's hover. Skeletons pulse over 1.2s. The reconnect dialog runs its own clock: a 0.3s delay, then a 1.5s slide and a 0.5s fade in, a 0.5s fade out, a 1s spinner and a 1.2s bar. **Under `prefers-reduced-motion: reduce` nothing moves**: every transition goes to 0, every animation jumps to its end, the skeleton is a still `header-bg` block and the reconnect dialog is simply there.

### States

- **Focus** is a 2px solid ring at a 2px offset on every `:focus-visible` element, from one rule: `focus-ring` (the action blue) on a light ground, `nav-focus-ring` (white) on dark chrome. The h1 that Blazor focuses after navigation, and the reconnect dialog's focused heading, show none.
- **Disabled** is 65% opacity. A button whose own action is running is not disabled: it keeps the focus and says so with `aria-disabled="true"`, with the progress cursor. Every other button that action would race is disabled.
- **An invalid field** is marked twice: a `danger-color` border and a 4px inset stripe down its left, so the state is not colour alone; its message carries the circle-alert icon. A contrast theme turns the stripe into a border.
- A row open for editing and the form row under it share a 4px `secondary-color` stripe.
- **The current nav item** is told by its bar and weight, not its fill; a contrast theme draws the bar as a border.

### Layout

- **The shell:** a 250px sidebar (`sidebar-width`) beside the page from 641px, the 56px top bar over the page, and the page in `article` with a side gutter at every width (32px left and 24px right from 641px, 16px on a phone). Below 641px the phone bar and its menu.
- **Grids** ask for their minimum or the whole container, whichever is less: `minmax(min(250px, 100%), 1fr)`, never a bare `minmax(250px, 1fr)`. Forms use `.form-grid` (250px), filters `.search-grid` (220px). The dashboard grid counts its tracks: three from 1100px, two from 901px, one at 900px and below.
- A detail page is `.details-grid`, 1fr 2fr, one column at 900px and below.
- A control is 38px, a button 36px (a small one 28px); every target on a phone's shell is 44px.
- Every page works at 390px with no sideways scroll; a wide table scrolls inside its `.table-container`.

## Contrast

Every pair the design system paints meets WCAG 2.1 AA: text 4.5:1; a control's edge, the focus ring and a meaningful mark (an icon that means something, a status dot, a progress fill) 3:1. `Design/ContrastTests` computes every figure from `app.css`'s `:root`, reads the pairs the shell's own stylesheets paint, and fails when a pair falls short or a figure in DESIGN.md differs from its computation.

**The method.** WCAG 2.1 relative luminance on the token values. A translucent colour is blended over the ground it actually sits on, unrounded, before it is measured. The sidebar's gradient is interpolated in sRGB, as a browser interpolates a gradient of `rgb()` colours, and sampled at 21 points from its start to its end; the worst sample must pass, and the tables give the start, the middle and the end (the worst is the start in every row). A figure here is the token pair's, not a picture's.

The tables are DESIGN.md § Contrast's, every row of them; that section is the source.

Page (each pair on a solid ground):

| Foreground / ground | Needs | Ratio |
|---|---|---|
| `text-color` on surface · background · header-bg | 4.5 | 12.63 · 11.99 · 11.36 |
| `text-color` on the success · warning · danger · info tints | 4.5 | 11.23 · 11.89 · 11.81 · 11.84 |
| `muted-text` on surface · background · header-bg | 4.5 | 5.09 · 4.83 · 4.57 |
| `muted-text` on the success · warning · danger · info tints | 4.5 | 4.52 · 4.79 · 4.75 · 4.77 |
| `link-color` on surface · background · header-bg | 4.5 | 6.70 · 6.36 · 6.03 |
| `link-color` on the success · warning · danger · info tints | 4.5 | 5.96 · 6.31 · 6.27 · 6.28 |
| `link-hover` on surface | 4.5 | 8.91 |
| `primary-color` on surface | 4.5 | 10.98 |
| `on-fill` on secondary · success · danger (filled buttons) | 4.5 | 4.86 · 5.88 · 5.95 |
| `secondary-color` (an outline button's label and edge, filled with the surface) on surface · background | 4.5 | 4.86 · 4.61 |
| `danger-color` (a field's refusal) on surface · background | 4.5 | 5.95 · 5.65 |
| `input-border` on surface · background · header-bg | 3 | 3.80 · 3.60 · 3.42 |
| `input-border` on the success · warning · danger · info tints | 3 | 3.38 · 3.58 · 3.55 · 3.56 |
| `focus-ring` on surface · background · header-bg | 3 | 4.86 · 4.61 · 4.37 |
| `focus-ring` on the success · warning · danger · info tints | 3 | 4.32 · 4.57 · 4.54 · 4.55 |
| Tint edges and icons: success · warning · danger · info (`secondary-color`) on their tints | 3 | 5.23 · 5.43 · 5.56 · 4.55 |
| Neutral badge: text · edge on header-bg | 4.5 · 3 | 11.36 · 3.42 |
| Status dots on surface: success · warning · danger | 3 | 5.88 · 5.77 · 5.95 |
| Progress fill on its hover-bg track: secondary · success (complete) | 3 | 4.61 · 5.58 |
| Reconnect bar: `secondary-color` on its header-bg track | 3 | 4.37 |
| Error bar: `text-color` · the warning edge and icon on warning-bg | 4.5 · 3 | 11.89 · 5.43 |
| `border-color` on surface (a hairline; see Colour for the two controls edged in it) | none | 1.30 |

Dark chrome (the gradient's start · middle · end, a translucent token blended over it first). The phone bar is the start as a solid, the phone menu's foot the end, and the signed-out bar the same gradient at 90deg, so the gradient rows hold for it too:

| Foreground / ground | Needs | Ratio |
|---|---|---|
| `nav-text` on the gradient | 4.5 | 9.78 · 11.09 · 11.25 |
| `nav-text-strong` on the gradient (the brand, the role name, the signed-out bar's text) | 4.5 | 14.07 · 15.96 · 16.19 |
| `nav-group-label` on the gradient (group headings, "Acting as", the switch's and the toggle's edges) | 4.5 | 8.00 · 9.08 · 9.21 |
| `nav-text-strong` on `nav-active-bg` over the gradient (the current item) | 4.5 | 5.27 · 5.73 · 6.08 |
| `nav-text` on `nav-active-bg` over the gradient | 4.5 | 3.66 · 3.98 · 4.22: forbidden |
| `nav-text-strong` on `nav-hover-bg` over the gradient (a hovered item) | 4.5 | 10.62 · 12.06 · 12.64 |
| `nav-focus-ring` on the gradient | 3 | 14.07 · 15.96 · 16.19 |
| `nav-focus-ring` on the current item | 3 | 5.27 · 5.73 · 6.08 |
| `nav-text-strong` on `nav-brand-bg` over the gradient's start (the brand cell) | 4.5 | 17.52 |
| Phone bar (the start, solid): `nav-text-strong` · `nav-group-label` · `nav-focus-ring` | 4.5 · 4.5 · 3 | 14.07 · 8.00 · 14.07 |
| Phone menu's foot (the end, solid): `nav-text-strong` · `nav-group-label` · `nav-focus-ring` | 4.5 · 3 · 3 | 16.19 · 9.21 · 16.19 |

Before T322, closed in flow 01: the semantic colours as words on their tints were 2.42 to 3.57:1, white on the success and danger buttons 2.87 and 3.82:1, the ring 2.99:1 on the page, an input's edge 1.49:1, and the ok and warn dots 2.87 and 2.57:1.

## Fonts and their licences

Both faces are self-hosted: the CSP's `font-src` is `'self'`, so there is no font host and no Google Fonts link. Both are under the **SIL Open Font License 1.1**, served as files beside the application and never compiled into it, which W-011 accepts as aggregation beside the AGPL-3.0 code. Each licence sits beside its files (the Fonts asset group).

- **Source Sans 3**: Adobe's release 3.052R, its variable upright and italic WOFF2 files, byte for byte. "Copyright 2010-2022 Adobe, with Reserved Font Name 'Source'": a subset, converted or otherwise changed file is a Modified Version and may not be served under that name, so the files are never trimmed or converted; a newer release is vendored whole. `Design/TypographyTests` pins each file's SHA-256.
- **Fraunces**: "Copyright 2018 The Fraunces Project Authors"; it reserves no name.
- A new face comes with its licence file beside it. W-011 accepts the OFL for a font served as a separate file; anything else is a new decision.

## Iconography

- Icons are Lucide line icons (MIT), one SVG per icon in `wwwroot/icons/`, rendered by `Icon.razor` as `<svg class="icon" width="16" height="16" aria-hidden="true"><use href="/icons/{name}.svg#i"/></svg>`. The `.icon` class draws them: no fill, `currentColor` stroke 2px, round caps and joins, so an icon takes its colour from the text around it.
- The set is 54 icons (the Icons group); 49 are named by a page. Name a new one by its Lucide name, copy Lucide's file, and give its root `id="i"`.
- **Thirteen older files are simplified redrawings, not Lucide's paths**, from before flow 01 (T010): `home` (a roof and walls with no door: the nav's Home and every "Go to Home"), `user` (the account row, a nav item), `inbox` (the Assessor's Activity inbox), `search` (Page not found), `info` (its stroke is `M12 10v6`, where the info alert's mask draws Lucide's `M12 16v-4`), `settings` (spokes, not Lucide's gear), `calendar` (a 16-tall body, where `calendar-check` is Lucide's 18), `users`, `book`, `file-text`, `pencil`, `trash`, and `alert-triangle` (a plain triangle; `triangle-alert` is Lucide's). Match a glyph against Lucide, not against these; the flow-01 icons are Lucide's own.
- Sizes: 16px by default (buttons, the account row), 18px in a dashboard card's title and for the phone trail's back chevron, 14px for the chevron between crumbs, 20px in the nav, the Menu toggle and the error bar, 24px before a system page's heading.
- The alerts' and a field's refusal icons are drawn by app.css (a masked `::before`), and the reconnect dialog inlines its own paths; no page writes those.
- The Bootstrap Icons font is not loaded: `<i class="bi bi-*">` renders nothing. Never use icon fonts, emoji or pictures as icons.
- Icons are decorative (`aria-hidden`): the words beside them carry the meaning.

## The mark

The Wombat mark is a white wombat face with a navy cross for a nose, rising from a dark burrow mound on a blue disc (`wombat-mark.svg`, a 64-unit square with transparent corners). It is multi-colour: show it as an `<img alt="">` beside the wordmark, never as an `Icon` glyph, and never recolour it. The lockup is the mark plus the Fraunces wordmark: 32px and `wordmark` in white in the sidebar's brand cell and on the signed-out bar, 28px and `wordmark-phone` on the phone bar, 56px and `wordmark-large` in `primary-color` on the sign-in and MSF respondent cards (the other account pages carry no lockup). **Below 641px the signed-out bar pairs a 28px mark with the 1.6rem `wordmark`**, 10px apart, beside a 44px Sign in: the 28px rule is not scoped to the sidebar, while the 1.4rem one is. DESIGN.md § Logo & brand assets says the signed-out bar's mark is 32px, so this is probably a slip in the code (scoping it to `.sidebar .brand-mark` would keep 32px and 1.6rem); until it is settled, this is what ships. `wombat-tile.svg` is the full-bleed square the app-icon PNGs are rendered from. `favicon.svg` is an older drawing (eyes, a triangle nose) that still serves as the browser favicon.

## Building a design Wombat can ship

Every design must stay buildable in this stack:

- **Blazor Server** (.NET 10, Razor components). A signed-in page is interactive: every click is a server round trip, so prefer explicit actions to per-keystroke behaviour.
- **Static pages.** For a signed-out visitor, sign-in, register, forgot password, link account, access denied, not found, the error page, the MSF questionnaire (`/msf/respond`) and portfolio verification (`/portfolio/verify`) are plain server-rendered HTML with form posts: no client behaviour beyond a small script module, no live validation, a CSS-only toggle. Three stay static when the person is signed in (`[ExcludeFromInteractiveRouting]`): `/Error` and `/portfolio/verify`, drawn inside the full signed-in shell, and `/msf/respond`, in the sign-in layout.
- **The signed-in shell must work with no circuit.** On `/Error` and `/portfolio/verify`, and on every page before its circuit connects, the frame is static HTML. So every control in the frame is a link, a form post, a `<details>` or a CSS toggle: the phone menu is a checkbox and its label, Change role is a native `<details>`, and every role switch is a full-load link. No `@onclick` menu, popover or live control in the frame.
- **A strict CSP.** Fonts, scripts, styles and images come from the site itself (images may be `data:` URIs). Self-hosted fonts only, WOFF2 files served beside the app under the OFL (W-011); no Google Fonts, no CDN scripts or styles, no inline scripts or `onclick`, no third-party calls or avatars.
- **No CSS framework.** No Bootstrap, Tailwind, MudBlazor, Radzen or jQuery. Every class a page names is defined in `app.css` (or a layout's `.razor.css`); a new class is written there first.
- **Tokens first.** No colour outside `:root`; prefer the `--space-*` scale for spacing (the shell's 6, 10 and 12px steps are the literals it allows, inside a row or a control); radii, shadows and durations from their tokens; no inline `<style>` in a page.
- **Keep the invariants:** every nav link opens a page that admits the acting role, and no two share a label; one `<h1>` per page; a page's nav label, heading, crumb and tab stem are the same words (a record's own crumb may be its name or id, through PageHeader's `CurrentCrumb`); the focus moves to an action's result; row actions are named per row; field help is linked by `aria-describedby`; an invalid field is not shown by colour alone; static pages stay static; WCAG 2.1 AA throughout, with targets at least 24px (44px in the phone's shell); reduced motion stops everything.
- **Keep the framework's names**: `#blazor-error-ui` with `.reload` and `.dismiss`, `#components-reconnect-modal` and its `components-reconnect-*` classes, `FocusOnNavigate`'s h1.

Check a design at 1280×800 and at 390×844.

## Not synced

- **Uploaded by the integrator, not synced as files:** the asset groups' files. The Icons group holds 25 of the 54 icons; the 29 flow 01 added are uploaded from `src/Wombat.Web/wwwroot/icons/`. The Fonts group holds nothing yet: its two licences are in `assets/Fonts/` (`SourceSans3-OFL.txt`, `Fraunces-OFL.txt`, copies of `wwwroot/fonts/`'s). The three WOFF2 files tokens.json's `type.fonts` names are in `fonts/` here, byte for byte `wwwroot/fonts/`'s (the SHA-256s `Design/TypographyTests` pins), and are published at `fonts/` with the system; without them the compiled `@font-face` rules 404 and every preview falls back to Segoe UI and Georgia.
- **Held outside tokens.json:** `color-scheme: light` and `--font-display-settings` ("SOFT" 0, "WONK" 1), which tokens.json has no shape for, are carried in `components/bundle.css`'s `:root`. `--motion-fast` is the `duration` family and `--sidebar-width` the `size` family.
- **Components not built:** the trajectory chart, the entrustment standing and MSF coverage panels, the EPA target coverage list, `EpaLabel`, the activity form and detail renderers, and the password toggle on its own (it appears in AuthLayout). DashboardFrame is described with DashboardCard. Previews are static renditions of the Razor markup, with the real class names and copy, styled by `components/bundle.css` (app.css and the four layout stylesheets, their isolation scope dropped); there is no JavaScript bundle, because the components are Razor, not React.
- **Not carried:** `favicon.ico` (the store takes no .ico), `site.webmanifest`, `wombat.js`, `js/*.js` and `ReconnectModal.razor.js`.
