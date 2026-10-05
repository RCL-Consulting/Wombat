# NavMenu

The navigation panel: the acting role's head and switch, then that role's links (flat up to eight, grouped above), then the person's own links under a rule, with at most one item lit.

Flow 01 of the restructure (T335, b347e11c) rebuilt it. Until then it was the union of every role held, in Title Case, with My Account, Data Rights and Logout written out, and five placeholder stubs.

## What the consumer provides

Nothing: MainLayout renders it in its header. What it offers is `NavItems.For(acting role, claims)` (`src/Wombat.Web/Navigation/NavItems.cs`), which MainLayout builds and cascades; each page is declared there once, with one label, one Lucide icon and the page it opens. Which item is lit is `NavOwners.Lit` (the owner table), from the page `Routes` cascades.

## Structure (DESIGN.md § The NavMenu)

`nav.nav-panel[aria-label="Main"]` (`nav-text`, a flex column whose list alone scrolls):

- **The role head** `div.role-head` (a `nav-divider` rule under it): `p.role-now` with `span.role-now-label` "Acting as" (0.8125rem/600, `nav-group-label`) and `span.role-now-name`, the role's sentence-case label (1.0625rem/600, `nav-text-strong`), one line where they fit, the role under the label where not, never cut.
  - **Two roles held:** one link `a.role-switch` "Switch to {label}" with an `arrow-left-right` icon (32px, a `nav-group-label` edge, `radius-md`).
  - **Three or more:** a native `details.role-change` whose `summary.role-switch` reads "Change role", listing `a.role-option` "Switch to {label}" for each other role held, in precedence order. No script, so it works on a static page.
  - Every switch is `/dashboard/switch/{role}` with no return address, so it lands on the new role's Home, and carries `data-enhance-nav="false"`: a full page load.
  - **No role** (a former trainee): no head and no switch; the list starts under the brand cell.
- **The list** `div.nav-list` (scrolls; no tabindex): `ul.nav-items` of `li > NavItemLink`, rows 4px apart.
  - Up to eight of the acting role's links, Home counted, one flat list; **the personal links are not counted** (flow 06, T358, E2: until then they were, and the Coordinator's eight would have been grouped). More (the Administrator's 16, the Institutional admin's 15), Home, then groups: `p.nav-group-heading` (0.8125rem/600, `nav-group-label`, sentence case), a `<p>` and not a heading element, naming its `ul.nav-items` through `aria-labelledby`.
  - **The personal links** last, in `ul.nav-items.nav-personal` under a `nav-divider` rule: My progress (to anyone holding the Trainee role or a trainee record, whatever the acting role) and My data rights (everyone).
- **NavItemLink** `a.nav-link`: a 20px icon and `span.nav-label`, 0.9375rem, 28px tall (44px and 1rem below 641px), `radius-sm`, `nav-text`. Hover: `nav-hover-bg`, `nav-text-strong`. Lit: the class `active`, `nav-active-bg`, `nav-text-strong`, weight 600 and a 3px `nav-text-strong` bar inset on its left.

## Who is offered what (NavItems.cs; DESIGN.md's table, which NavMenuAuthorizationTests holds)

| Acting role | Links |
|---|---|
| Administrator | Home; Platform: Scheduled jobs, Audit log, SSO mappings, Data rights requests; Organisations: Institutions, Colleges; People: Users, Invitations; Catalogue: EPAs, Curricula, Activity types, Entrustment scales; Reviews: Decisions due, Committee reviews, Decision panels |
| Institutional admin | Home; People: Invitations, Trainees, Assessors, Users; Curriculum: Curriculum adoptions, Curricula, EPAs, Activity types, Entrustment scales; Reviews: Decisions due, Committee reviews, Decision panels; Access and audit: SSO mappings, Audit log |
| College admin | Home, Specialities, EPAs, Curricula, Activity types |
| Speciality admin, Sub-speciality admin | Home, Programme trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels, Entrustment decisions |
| Committee member | Home, Programme trainees, Committee reviews, Decision panels |
| Coordinator | Home, Programme trainees, Waiting for assessors, Decisions due, MSF campaigns, Committee reviews, Decision panels, Data rights requests |
| Assessor | Home, Activity inbox |
| Trainee | Home, Log an activity, My activities, MSF reports, My committee reviews, Export portfolio |
| Pending trainee | Home, Log an activity, My activities |
| No role | Home |

Then the personal links. Icons: home, circle-plus, list, message-square, file-text, download, inbox, calendar-check, scale, stethoscope, list-checks, book-open, book-check, clipboard-list, gauge, building, graduation-cap, users, mail, user-round, user-check, clock, history, key, shield-check, trending-up, lock, award.

**Flow 06** (T358, 2026-10-05; R2-Menus; Q7, C10) gave the four roles that watch the programme its pages: Programme trainees (`users`) for all four; Waiting for assessors (`clock`) for the three that chase it, not the Committee member, whose work is not chasing; Entrustment decisions (`award`) for both speciality admins, who reached it until then only by address; and Decision panels for the Coordinator, whom the list admitted with no item (Step 2.32), read-only as before. The Coordinator's eight stay flat.

## The current item (NavOwners.cs; R2-Rules § 3)

- **A list lights itself**, with `aria-current="page"`.
- **A page under a list lights its owner** for the acting role, with `aria-current="true"`: an activity lights Activity inbox for an Assessor and My activities for a Trainee; an audit entry lights Audit log; a College's specialities light Specialities for the College admin and Colleges for the Administrator.
- **Where the acting role has no owner, nothing is lit.** A page outside the acting role's links still opens (access is the union of the roles held), with nothing lit. "Lights Home" is retired.
- **Nothing is lit** on My account (the account row's name carries `aria-current="page"`), Change password, the sign-in pages, the anonymous static pages and the system pages.
- The row's cue is its bar and its weight; the fill is only 2.67:1 against a plain row.

### The owner table (`NavOwners.Table`; DESIGN.md § The NavMenu)

Every page that is not an item's own, and the list it sits under for each acting role. That list is lit (`aria-current="true"`) and the page's trail is Home › the list › the page. A role the row does not name has no owner there: nothing is lit and the trail is Home › the page. A page goes in this table, and a new one is placed here before it is drawn.

| Page | Lit item, by acting role |
|---|---|
| An activity (`/activities/{id}`) | Assessor: Activity inbox · Trainee, Pending trainee: My activities · Speciality admin, Sub-speciality admin, Coordinator: Waiting for assessors, however the activity was reached (flow 06) |
| A registrar's page (`/programme/trainees/{id}`, flow 06) | Committee member, Speciality admin, Sub-speciality admin, Coordinator: Programme trainees; its trail Home › Programme trainees › the registrar's name |
| A committee review | Committee member, Coordinator, Speciality admin, Sub-speciality admin, Institutional admin, Administrator: Committee reviews (a trainee reads theirs on My committee reviews; this page does not admit them) |
| A decision panel | Administrator, Institutional admin, Speciality admin, Sub-speciality admin: Decision panels. Not the Coordinator: the panel's page does not admit the role, and an owner is named only for a role the page admits, so the Coordinator reads the panels on the list, which lights itself (D4) |
| An MSF campaign, an MSF report (`/msf/reports/{id}`), MSF coverage (`/msf/coverage`) | Coordinator: MSF campaigns |
| A data-rights request | Coordinator, Administrator: Data rights requests (its own crumb is the request's id) |
| A College's specialities and sub-specialities, and their edit pages | College admin: Specialities · Administrator: Colleges. On a College's specialities the College is in the trail: Home › Specialities › {College name} for the College admin (the College is the page's own crumb), Home › Colleges › {College name} › Specialities for the Administrator |
| A College | Administrator: Colleges |
| An institution | Administrator: Institutions |
| An EPA; an activity type; a curriculum's items | Administrator, College admin, Institutional admin: EPAs; Activity types; Curricula |
| A curriculum | Administrator, College admin: Curricula |
| Curriculum progress (`/admin/curriculum-progress`) | Administrator: Curricula (linked from Curricula) |
| An entrustment scale | Administrator: Entrustment scales |
| A user | Administrator, Institutional admin: Users |
| A trainee's profile; an assessor's profile | Institutional admin: Trainees; Assessors |
| An audit entry | Administrator, Institutional admin: Audit log |
| A scheduled job's run history | Administrator: Scheduled jobs |
| My authorisations | Trainee: My progress |
| An EPA's page (`/portfolio/progress/{id}`, flow 05) | Every acting role, and none: My progress (a page under a personal link; below) |

**A page under a personal link** (flow 05, T355, E3; `NavOwners.UnderAPersonalLink`, `IsPersonal`): an entry that names no role, under My progress or My data rights, is owned whatever the acting role, and for a graduate who holds none, since the menu offers the link whatever the role. It is lit only where the person is offered the link. An entry that names roles keeps them, whatever item it is under: My authorisations is under My progress for the Trainee alone.

**Entrustment decisions** is not in the table: for both speciality admins it is a list the menu offers, which lights itself (flow 06, R2-Menus m5); for every other role it admits, the Institutional admin's Home link included, it is under no list until flow 09 places it.

A list that is an item of the acting role's menu lights itself and draws no trail. A list the acting role's menu does not offer (opened through another role held) lights nothing, with the trail Home › the list.

### Outside the rule (`NavOwners.Outside`)

These light nothing and draw no trail, but one:

- **My account** (`/account/profile`): the top bar's name is the current item (`aria-current="page"`).
- **Change password**: nothing lit; the one trail here, Home › My account › Change password.
- **The sign-in pages**, in the sign-in layout: Sign in, Sign out, Register, Forgotten password, Link account, and the MSF questionnaire (`/msf/respond`).
- **Portfolio verification** (`/portfolio/verify`), the anonymous static page anyone holding a portfolio PDF opens.
- **The system pages**: Access denied, Page not found, the error page.

**A page that draws Page not found in its own place lights nothing** (flow 06, T358, build review D1): a registrar, a roster or an EPA that is not the caller's to read is no page at all, so its owner is not lit either. The page tells the shell through its header (PageHeader's `Page="typeof(NotFound)"`), which hands it to the `PageDrawn` that `Routes` cascades; NavMenu lights by the page drawn before the routed one, and lights again when the header tells it (the header renders after the menu). The word is held against the route it was said on, so the next navigation forgets it. `ActiveNavItemTests` mounts the menu beside Programme trainees, a programme trainee and an EPA page drawn as not found, and asserts nothing is lit.

## Rules

- **One acting role at a time, never the union.** The acting role chooses only what the frame shows; following a link never switches it.
- **A nav label is its page's `<h1>`, its breadcrumb and the stem of its tab**, in sentence case: "Activity inbox" and "Activity inbox · Wombat".
- **Every link opens a page that admits the acting role, and no two links share a name** ("My committee reviews" beside "Committee reviews"; "My data rights" beside "Data rights requests").
- **No link to an unbuilt page**, no Sign out and no My account (the account row has both).
- Every colour is a `--nav-*` token and every ring `nav-focus-ring`. A new item goes in `NavItems.cs` and DESIGN.md's table; a new page under a list goes in the owner table.

## Contrast

`nav-text` 9.78 to 11.25:1 on the gradient; a hovered row 10.62:1 and up; the current row's words 5.27:1 on `nav-active-bg` (`nav-text` there would be 3.66:1, so it is forbidden); group headings and "Acting as" 8.00:1; the ring 14.07:1 on the gradient and 5.27:1 on the current row.

## Known gaps

- Flow 06 built Programme trainees, and Waiting for assessors in Stalled activities' place. The STAR review queue is not restored: Entrustment decisions, the programme's register of STARs, takes its place under its own label for both speciality admins. Recent activities and System were dropped.
