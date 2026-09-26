# Wombat GUI redesign: the master brief

For the operator running the redesign in Claude Design, and for Claude Code implementing its result (T332). Written
2026-09-26.

- **Per-flow briefs:** `design/flows/NN-<slug>.md`, one per flow, ready to paste (§ 8).
- **Screenshots:** `design/baseline/`, which is gitignored (§ 10).
- **Citations:**
  - Every claim about Wombat names a file, a task or a runbook step.
  - Every claim about Claude Design names its source in brackets, keyed in § 2.0. Each was re-fetched and checked on
    2026-09-26.
  - Where a claim is inferred rather than documented, it says **(inference)**.

---

## 1. What Wombat is and who uses it

Wombat is a work-based assessment tool for specialist registrars. It is built on EPAs (Entrustable Professional
Activities).
- **Who owns what.** A national College owns the catalogue: EPAs, entrustment ladders and curriculum versions. An
  institution adopts it and admits its registrars against it.
- **The core loop.** Registrars file observed work, and consultants rate it. A committee reads the evidence each period
  and issues STARs (Statements of Awarded Responsibility).
- **The story the screenshots come from.** Paediatrics (CPSA, catalogue v11.1) at Kgosi Kgari Teaching Hospital
  (`execution/knowledge/scenario-paediatrics/README.md` § The world, § Cast). All data is scenario data; no real person
  is in it (CLAUDE.md § Nothing is live).

Every job each role does is indexed in `execution/knowledge/scenario-paediatrics/coverage.md` § Journeys by role.

| Role | Who in the story | Comes to Wombat to | Flows |
|---|---|---|---|
| Administrator | devadmin (the platform operator) | keep the platform, the Colleges and the scales; create institutions; run jobs; read the audit trail | F18, F16, F11, F14, F15, F17 |
| CollegeAdmin | Dr Anton Kruger (CPSA) | keep specialities, EPAs and curriculum versions | F16; F17 once T300 lands |
| InstitutionalAdmin | Prof Nolwazi Mbatha (KGK) | adopt the curriculum; invite, admit and manage people; build activity types | F11, F12, F15, F17, F08, F13, F09 (the STAR register), F18 (the audit log) |
| SpecialityAdmin, SubSpecialityAdmin | Dr Refilwe Mokoena, Dr Kabelo Sithole | watch the programme; form panels; schedule reviews; keep the STAR register | F06, F08, F09 |
| Coordinator | Mr Pieter Smit | run MSF campaigns; chase stalled work; schedule reviews; decide data-rights requests | F06, F08, F10, F13, F14 |
| CommitteeMember | Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha, Dr John van Rensburg (external) | sit reviews, stage and ratify STARs, resolve appeals | F06, F07, F08, F09 |
| Assessor | Dr Mohammed Patel, Dr Fatima Khumalo (and the three committee consultants) | rate, decline, return and discuss registrars' work | F04 |
| Trainee | Dr Molefe (year 4), Dr Dlamini (3), Dr du Plessis (2), Dr Mahlangu and Dr Ndlovu (1) | file activities; read progress, decisions and STARs; appeal; export the portfolio | F03, F05, F09, F10, F13, F14 |
| PendingTrainee | each registrar between registering and admission | wait to be admitted | F11 |
| Former trainee (no role, a `trainee_record` claim) | Dr Molefe after step 5.17 | keep a read-only record | F13 |
| Anonymous | invitees, MSF respondents, a portfolio verifier | register, answer an MSF questionnaire, verify a PDF | F11, F10, F13, F02 |

There are **10 roles**, as defined in `src/Wombat.Domain/Identity/WombatRoles.cs`. Older text says nine; T296 corrected
it.

## 2. How to run this redesign

### 2.0 Sources for the Claude Design claims

The research behind every claim below, with each source's verdict after a skeptic re-fetched it, is in
`design/research/claude-design-2026-09-26.json`. `design/tools/` holds the checks this brief was held to: every
screenshot it names exists, every runbook step and page is in a flow, and every quoted step matches the runbook.

| Key | Source |
|---|---|
| [launch] | anthropic.com/news/claude-design-anthropic-labs |
| [setup] | support.claude.com/en/articles/14604397-set-up-your-design-system-in-claude-design |
| [start] | support.claude.com/en/articles/14604416-get-started-with-claude-design |
| [admin] | support.claude.com/en/articles/14604406-claude-design-admin-guide-for-team-and-enterprise-plans |
| [academy] | academy.claude.com/tutorials/using-claude-design-for-prototypes-and-ux |
| [parrott] | claude.com/blog/how-the-product-designer-who-built-claude-design-uses-it-to-explore-ideas-before-building-them (24 July 2026) |
| [cmds], [w34], [artifacts] | code.claude.com/docs/en/commands; …/whats-new/2026-w34; …/artifacts |
| [onbrand] | claude.com/blog/claude-design-stays-on-brand-for-daily-work |
| [relnotes] | support.claude.com/en/articles/12138966-release-notes (16 September 2026 entry) |
| [cli] | the installed Claude Code 2.1.282 binary's own description of `/design-sync` |
| Secondary | builder.io/blog/claude-design (a competitor); dev.to/bilelsalemdev (a tutorial); wmedia.es/en/tips/claude-code-design-sync-your-components |

### 2.1 What Claude Design will and will not do for Wombat

- **It will not find the flows.**
  - The user walks it through each journey: "Walk Claude through the journey and it will generate each screen in
    context" [academy].
  - No source says it maps routes, pages, roles or journeys from a repo.
  - This brief and `design/flows/` supply them.
- **It will not use Wombat's components.**
  - `/design-sync` converts only a React design system: "Convert your repo's React design system" [cmds]; "If your
    design system already exists as React components, run /design-sync" [setup]; "Push a React design system" [cli].
  - Wombat's components are Razor, so the design system it holds is visual only: tokens, type, icons and brand.
  - The component vocabulary goes into every brief by name (§ 5.1).
- **Without a design system its output is generic.** The admin guide says so: "functional but generic output"
  [admin]. So set the design system up first (§ 2.3).
- **Left undirected, it picks the look itself.** "Left undirected, Claude picks one of its favorite aesthetics"
  [parrott]. The operator states the direction (§ 4).
- **What comes back is a visual spec, not code Wombat can use.**
  - The handoff bundle is "design files, the chat and a README" [academy]. A secondary source says the design files are
    HTML/CSS/JS [dev.to].
  - Anthropic's own advice: "If you're shipping production software, stick with Claude Code" [parrott].
  - Claude Code rebuilds each design in Razor and `app.css` (§ 9).
- **Expect lower fidelity.** Anthropic reports "pretty high fidelity" when it can fetch your components [parrott]. A
  competitor estimated "maybe 50%-75% at best" [builder.io]; that was in April 2026, before the June update, and it is
  an estimate, not a measurement. Wombat can neither sync its components nor run a preview, so expect the lower figure
  (inference).

### 2.2 Where to run it

| | `/design` in a Claude Code session in this repo (**recommended**, inference) | claude.ai/design |
|---|---|---|
| How Wombat gets in | The session reads `app.css`, `DESIGN.md`, the runbook and the PNGs where they are. The artifact skill "looks for an existing design system in your project" [artifacts]; that `/design` does the same is an inference. | Link or upload a codebase; "Claude reads the components and styles" [setup]. **Linking the repo is not possible as it stands:** `origin` is `ssh://renier@rcl.co.za:10648/…` (`git remote -v`), not GitHub. **Do not link `github.com/reniercloete/Wombat`:** it holds only the pre-rewrite app (its `master` is `1f14260c`, a 2024 commit in this repo's early history; CLAUDE.md § Reference folders), so it would import the old app's look. Upload the staged folder in § 3.1. |
| What it produces | Artboards on one canvas, published as a Design artifact, with PNG or PDF export per artboard [cmds]. Then "pick an artboard, and tell Claude which option to implement" [w34]. Since 16 September 2026 Claude Design runs inside Claude Code conversations [onbrand; relnotes]. One research sweep quotes the release notes as adding "on-canvas editing and importing your design system"; the re-check confirmed only the in-conversation part. | Variations [start]. Exports: .zip, PDF, PPTX, Google Slides, standalone HTML. Handoff: "Send to local coding agent" [start]. |
| What it needs | v2.1.234 or later [w34] (v2.1.265 per [cmds; artifacts]); 2.1.282 is installed. A claude.ai login: [cmds] says so for `/design-sync`; one research sweep read it for `/design` too, not re-verified. | A paid plan; it is a research preview [launch]. It is off by default on Enterprise [admin]. |
| Size | Nothing to upload. | "Consider linking very large repositories from Claude Code to avoid lag or browser issues" [start]. |

Hand off to a **local** Claude Code session either way. "Send to Claude Code Web" needs the repo reachable from the
web, and it is not (inference).

### 2.3 Order of work

0. **Decide § 4** (reskin or restructure) and state the aesthetic direction in a sentence or two.
   - Record the decision as a `W-nnn` entry in `execution/DECISIONS.md`.
   - "Do the thinking before you prompt" [parrott].
1. **Set up the design system.**
   - Give it the staged set (§ 3.1) and 6–10 screenshots as "real examples, not just specs" [setup], for example
     `states/home--administrator.png`, `states/activity-inbox--assessor.png`, `states/new-activity--mini-cex.png`,
     `states/activity-view--requested.png`, `states/login--blank.png`, `states/shell--nav-open.png` and
     `states/review-detail--staged.png`.
   - Paste § 5.1 into the setup chat, framed as constraints.
   - Name the components, which is what [start] advises ("mention it by name").
   - **Check what it extracted:**
     - the palette equals `app.css` `:root`;
     - Fraunces is used on the wordmark only;
     - the icons are Lucide line icons;
     - no Google Fonts or CDN is used.
   - If it misses, try again with other assets (a research sweep's paraphrase of [setup], not a quotation). "Design
     system import is only as good as its source" [start].
2. **Brief F01 (the shell) first.** It settles the tokens and the frame that every later flow sits in.
3. **Brief the flows in order (§ 8), one thread per flow.** The brief template is § 2.4, and each flow file already
   follows it.
   - Attach the flow's KEY SCREENSHOTS. Add STATES as the chat asks for them.
   - **Structural changes get a wireframe first, then a pick, then fidelity** ("Wireframe first when fidelity doesn't
     matter" [parrott]). Structural means the shell, the nav, dashboards and page shapes. Recolour, type and density go
     straight to fidelity.
   - Ask for 2–3 variations [start].
   - Ask for the empty, error and loading states and for different data volumes: "Flag edge cases" [academy].
   - Ask for an accessibility review [start], with § 6 A1 as the check.
   - Answer its clarifying questions in the chat, and write each decision as a sentence. The chat travels in the
     handoff bundle [academy].
   - **Export the chosen artboards into `design/flows/NN-<slug>/` before moving on.** There is no version history
     [start].
4. **Hand off (§ 9),** one Claude Code task per flow.
5. **After any `app.css` or `DESIGN.md` change, re-import the design system.** This is a precaution: the claim that an
   import is a snapshot rests on one uncited blog.

### 2.4 The brief template (every `design/flows/*.md` follows it)

```
FLOW NN — <the job, as the person's goal>
GOAL: what these screens must let the person do, and what is wrong today.
AUDIENCE: <role>, <persona>; desktop 1280×800 and phone 390×844.
SCREENS, in order: 1. <route> — <purpose>; 2. …
STEPS: <runbook step ids>, pasted verbatim below (Role / Route / Do / Expect).
STATES TO SHOW: loading, empty, error, refused ("Nothing was saved. …"), access denied, not found, narrow; data volumes
  none / typical / heavy.
REQUIREMENTS FROM KNOWN DEFECTS: <Txxx: testable requirement>.
QUESTIONS THE DESIGN MUST ANSWER: …
CONSTRAINTS: <§ 5.1, verbatim>.
ASK: 2–3 variations; <wireframe | full fidelity>; name every design-system component used and mark anything else NEW;
  say which DESIGN.md rule a variation breaks.
ATTACHED: <file list>
```

## 3. What to link and upload, and what never to

**The rule:** only tracked files and screenshots you have opened go to Claude Design.
- **Never upload or link the working tree.** That means neither the repo root nor `src/Wombat.Web` as a folder.
- **The same applies to a Claude Code `/design` session.** What it reads goes to the model, so point it at the files
  below (inference).

### 3.1 The design system: stage tracked files only

Run this from the repo root. It copies 47 tracked files (about 470 KB); it was dry-run on 2026-09-26.

```powershell
$dst = Join-Path $env:TEMP 'wombat-design-system'
if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }
git ls-files src/Wombat.Web/wwwroot src/Wombat.Web/Components/Layout execution/architecture/DESIGN.md |
  ForEach-Object { $to = Join-Path $dst $_; New-Item -ItemType Directory -Force (Split-Path $to) | Out-Null; Copy-Item -LiteralPath $_ $to }
```

| What | Path | Why |
|---|---|---|
| Tokens and every component style | `src/Wombat.Web/wwwroot/app.css` (tokens on `:root`, lines 11–45) | The design system's source |
| Display face | `wwwroot/fonts/fraunces-var.woff2` (OFL) | The wordmark |
| Brand | `wwwroot/brand/wombat-mark.svg`, `wombat-tile.svg`, `wwwroot/favicon.svg` | The logo. Bring it; do not ask for a new one (the research sweep found no image-generation model; not re-verified) |
| Icons | `wwwroot/icons/*.svg` (25 Lucide icons, 20 in use) | The icon set |
| Shell | `Components/Layout/MainLayout.razor(.css)`, `NavMenu.razor(.css)`, `AuthLayout.razor`, `ReconnectModal.razor(.css)` | The frame |
| Contract | `execution/architecture/DESIGN.md` | Mostly behaviour and wording, not look. Say which passages are stale (§ 5.5). The documented inputs do not name `.md` [setup], so if it is refused, paste § 5 instead. |
| Optional | `Components/Shared/` (36 tracked files: 26 `.razor`, 10 `.cs`) | How well Razor is read is undocumented. [academy] names CSS modules, Tailwind and styled-components, and implies React only through its mention of hooks. |

**Per flow, attach:**
- the flow's screenshots from `design/baseline/`;
- its runbook steps, pasted as text;
- for F03 and F17, three seed folders from `src/Wombat.Infrastructure/Activities/Seeds/`: `mini_cex_cpsa`,
  `reflective_exercise_cpsa` and `teaching_session`. Each holds `schema.json`, `workflow.json` and `credit.json`.

### 3.2 Never upload, link or paste

| Path | What it holds |
|---|---|
| `pwd_DO_NOT_COMMIT.txt` | Session passwords |
| `recovery/` | pg_dump snapshots of the dev and replay databases |
| `publish/`, `.scenario-app/` | Published binaries and replay logs (email bodies with live links) |
| `src/*/bin/`, `src/*/obj/`, `src/Wombat.Web/dev-server.log`, `*.csproj.user` | Build output with copies of every `appsettings*.json`; logs |
| `src/Wombat.Web/appsettings*.json` | `appsettings.Development.json` is tracked and carries the dev seed-admin password |
| `.claude/`, `.vs/`, `.dotnet/`, `execution/.dotnet/`, `.playwright-mcp/`, `src/Wombat.Web/.playwright-mcp/` | Local settings, full worktree copies, tool caches |
| `EPA Book/`, `EPA version 11.1.docx` | A third party's copyrighted book; the College's draft |
| `deploy/`, `execution/` (except `DESIGN.md`) | Server layout; project history |
| User secrets (`%APPDATA%\Microsoft\UserSecrets`) | The connection string |

Also never paste the cast's or the admin's passwords, a connection string, `Wombat__PseudonymSalt`, or server or SSH
details.

### 3.3 Screenshots

The screenshots are scenario data, so they are shareable, but **open each one first**.

- **Invitation captures show a one-time registration link.** Every capture of `/admin/invitations` taken just after
  Issue or Resend shows the full `/account/register?token=…` link.
  - Observed: `act-1/1.5-2-kruger-invited.png` and `states/invitations-list--resent.png`.
  - Likely the same: `states/invitations-list--issued.png` (`states.md`: "Issued, with its link") and the act-1 and
    act-2 `*-invited`, `*-issued` and `*-resent` captures.
  - Crop the link out, or leave the image out.
- **Other captures checked:** `act-A/A.4.5-2-reset.png` shows no password (observed).

## 4. The decision to make first: RESKIN or RESTRUCTURE

| | **RESKIN** | **RESTRUCTURE** |
|---|---|---|
| Changes | The visual language: palette, type, radii, shadows, density, the sidebar's look, badge and alert styles | The same, plus the shell, the navigation, the dashboards and the page shapes |
| Keeps | DESIGN.md's structure: its own lock reads "Change the colours in `:root`, keep everything else" (line 2103, and lines 5 and 2081) | Only the invariants in the last row of this table |
| DESIGN.md | Rewrite § Design tokens, § Typography and the look of each component section | Retire the ClinicAssist structural lock (lines 5, 2081 and 2103) by a recorded decision, then rewrite § Layout grid, § The NavMenu, § Dashboard layout grid and § Page-level patterns |
| Tests to edit | Only the pins on values that change: `Design/FieldGroupTests` (border), `InvalidFieldStyleTests`, `TableColumnClassTests` and `NarrowLayoutTests`. Add T322's planned `Design/ContrastTests`. | All of the RESKIN column, plus `Navigation/NavMenuAuthorizationTests`, which parses DESIGN.md's nav table, so a nav change is a DESIGN.md change; `DashboardLinkAuthorizationTests`; the placeholder test; `PageShapeSmokeTests` and `DesignSystemSmokeTests`, which pin class names; the page layout tests (`Admin/ActivityTypeBuilderLayoutTests`, `CurriculumItemsEditLayoutTests` and others); and `NarrowLayoutTests`' shell pins (`@media (min-width: 641px)` and the gutter). `Design/*` is 12 files and 2,509 lines. |
| Survives either choice | Every nav link opens a page that admits the role, and no two share a label. One `<h1>` per page. Focus moves to an action's result. Row actions are named per row. Field help is linked. An invalid field is not shown by colour alone. Static pages stay static. The CSP. | |

### Recommendation: restructure the frame, reskin the pages

**Restructure** the shell, the navigation and the role dashboards, and design the missing screens that hang off them
(§ 7). **Reskin** the List, Detail, Form, Account and Anonymous page shapes, keeping the component vocabulary and its
class names. Three dense pages get layouts designed deliberately inside their shapes: the review page (F07), the
activity page (F03, F04 and F07), and the curriculum item editor and builder (F16, F17).

**Why the frame needs restructuring:**
- **The nav is long and flat.**
  - The Administrator sees 20 links: 16 role links plus Home, My Account, Data Rights and Logout. The InstitutionalAdmin
    sees 18 (DESIGN.md:192–205). A multi-role user sees the union.
  - There is no grouping.
  - Five links open "Coming soon" stubs (DESIGN.md:217–220).
  - Seven pages a role is admitted to have no link in that role's nav (`coverage.md` § Reached only by address).
  - The nav lights the wrong item or none (T331).
  - Sign out appears twice: in the top row and as the nav's Logout (`MainLayout.razor`, `NavMenu.razor`).
- **The dashboards are wrong in what they show, not how they look.**
  - Five cards count the wrong thing (T297).
  - One card has no data source (T298).
  - A graduate lands on "No role assigned" (T311; `Home.razor:66`).
  - System health is two stubs labelled with task ids (T327).
  - Four of the five placeholders are the list behind a dashboard card: Recent Activities, Stalled Activities, Programme
    Trainees and System (§ 7 B7).

**Why the page shapes can stay:**
- **Their defects fit within the shapes.** They are presentation and state problems, each fixable inside the current
  shapes:
  - contrast (T322);
  - widths (T323);
  - labels (T324);
  - copy (T326);
  - stylesheet (T328);
  - loading and error states (T329).
- **The shapes carry pinned accessibility guarantees:** `Accessibility/ActionFocusTests`, `FormFieldHelpTextLinkTests`,
  `Design/RowActionMarkupTests` and `AlertRoleTests`.
- **Claude Code would rebuild new shapes from pictures.** Claude Design cannot build with Razor components (§ 2.1), so
  new page shapes would come back as a visual spec that Claude Code rebuilds across 80 page templates (inference).

**Two visual decisions to make either way:**
- **The body font.** The stack starts with Segoe UI, which only Windows has (`app.css:50`), so other systems fall back
  to Tahoma, Geneva or Verdana. The choice is to keep it, or to name a self-hosted, GPLv3-compatible woff2.
- **Dark mode and reduced motion.** `app.css` has neither (no `prefers-color-scheme` or `prefers-reduced-motion`
  rule), so both are opt-in.

## 5. Constraints every design must meet to be buildable

### 5.1 The constraints digest (paste verbatim into every brief)

```
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
```

### 5.2 The technical constraints, with their sources

| Constraint | Source |
|---|---|
| **Two render modes.** A signed-out visitor's pages and `[ExcludeFromInteractiveRouting]` pages are static; everything else is `InteractiveServer`. | `Components/App.razor` (`PageRenderMode`) |
| On a static page, `@onclick`, `@bind`, `OnAfterRenderAsync` and JS interop do nothing. Behaviour there is a link, a form post to an endpoint, or `wwwroot/wombat.js`. | DESIGN.md:1887–1891 |
| **The sign-in cookie is written only by an HTTP POST.** Sign-in, register, link, sign-out and change password stay real `<form method="post">` elements. | DESIGN.md:1925–1933 |
| `/msf/respond` is static for everyone: its link's rate limit must see every request. `/portfolio/verify` is a GET form checked as the page renders. | `App.razor` remarks; T205, T265 |
| **CSP:** `default-src 'self'; script-src 'self' 'nonce-…'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'self'` | `src/Wombat.Web/Security/SecurityHeadersMiddleware.cs:46–58` |
| Claude's design output is allowed Google Fonts and CDN scripts, including the Tailwind CDN and jQuery. All of these must be stripped. | [artifacts], per the research verdict |
| Every script is same-origin or carries the nonce. Every first-party CSS and JS file is linked through `@Assets[…]`. | `Hosting/AppAssetUrlTests` (T175); DESIGN.md:41 |
| **Every class a page names is defined** in `app.css` or the component's `.razor.css`. So utility classes (`px-4`, `flex`) cannot be used unless they are written into `app.css`. | `Design/DefinedClassTests` (T266) |
| Icons render as `<svg class="icon"><use href="/icons/{Name}.svg#i"/></svg>`. A new icon is a Lucide SVG whose root has `id="i"`. There is no Bootstrap Icons font. | `Components/Shared/Icon.razor`; DESIGN.md:1682 |
| Dependencies must be GPLv3-compatible; the repo is AGPL-3.0. JS goes in small modules under `wwwroot/js/`. Modals are native `<dialog>` elements opened by `wwwroot/js/dialog.js`, on interactive pages only. | CLAUDE.md § Key technical choices; DESIGN.md:2080; `ConfirmDialog.razor` |
| Exactly one `<h1>` per page, from `PageHeader`: `FocusOnNavigate Selector="h1"` targets it. Dashboards add none. | `Routes.razor`; DESIGN.md:1806 |
| The activity forms are drawn at runtime from jsonb schemas (22 seeded types). A design is for the renderer's field, section and state vocabulary, not for one form. | `Components/Shared/Activities/ActivityForm.razor`, `ActivityDetail.razor`; research `blazor_specific_risks[4]` |
| The nav depends on role, and the data on institution scope. An out-of-scope id is a 404, not a 403. | `NavMenuAuthorizationTests`; CLAUDE.md § InstitutionalAdmin scope-aware powers |

### 5.3 Framework-owned markup: design it, and keep its names

None of this appears in any `.razor` file a design tool reads (research `blazor_specific_risks[2]`):
- `NavLink`'s `active` class;
- the input classes `valid`, `invalid` and `modified`, and `aria-invalid`;
- `.validation-message` and the ValidationSummary list;
- `#blazor-error-ui` and its `.reload` link (`MainLayout.razor:30`);
- `#components-reconnect-modal` and the `components-reconnect-*` state classes (`Layout/ReconnectModal.razor.css`);
- the reconnect wording (T330).

### 5.4 DESIGN.md's policy rules, which are open to deliberate change

Change any of these only together with DESIGN.md and the test named.

| Rule | DESIGN.md | Pinned by |
|---|---|---|
| Structure "ported from ClinicAssist", with fixed class names (`.clinic-table`, `.detail-card`, `.btn-*`…) | 5, 2081, 2103 | `DesignSystemSmokeTests`, `PageShapeSmokeTests` |
| Page shapes: List, Detail, Form, Dashboard, Account, Anonymous | 1689–2001 | `PageShapeSmokeTests` (the static and account shapes are technical) |
| Shell values: 250px sidebar, sticky top row, 641px breakpoint, 16px gutter | 139–168 | `NarrowLayoutTests` |
| Nav order, grouping and labels; the five placeholders | 170–243 | `NavMenuAuthorizationTests` (parses the table) |
| Badge tints come from `BadgeFor` only, with a state→tint table | 847–1634 | `DefinedClassTests.OnlyBadgeFor_NamesABadgeClass`, `BadgeForStatusTableTests` |
| Buttons: `.btn-outline` only; the primary action goes in the `PageHeader` slot; `.btn-danger` only in a dialog footer | 245–267 | Not tested. There are 15 uses of `.btn-danger` outside `ConfirmDialog.razor`. |
| Colours only in `:root`; spacing only on `--space-*`; no inline `<style>` in pages | 45, 2073; CLAUDE.md | Not tested. There are 34 `style="` attributes in 14 files. |

### 5.5 Where DESIGN.md is stale (do not copy these)

- **Line 3:** it describes a "37-line" `app.css`. The file is 1,406 lines.
- **Line 2103:** "The palette is still TBD". The palette was set in T089.
- **The token block** gives `--muted-text #6c757d`. The value is rgb(104 111 119) (T086). The block also omits
  `--font-display`.
- **The lockup:** "font-weight 700" (line 115). The wordmark is Fraunces 500 at 1.7rem (`NavMenu.razor.css`).
- **`.account-form-container`:** 400px (line 1870). The value is 30rem.
- **The List-page template** uses `shadow-sm mb-4`, which is undefined, so copying it fails `DefinedClassTests`.
- **`wwwroot/lib/`** (line 16) does not exist.
- **Defects in the stylesheets themselves:**
  - `var(--text-muted)` at `app.css:623` names an undefined token;
  - nav items render underlined (`act-1/1.5-2-kruger-invited.png`): `NavMenu.razor.css` sets `text-decoration: none`
    on the brand (line 38) but not on `.nav-link`;
  - raw colours sit outside `:root`: the auth gradient (`app.css:57–58`), the dialog backdrop (`app.css:1309`),
    `NavMenu.razor.css`, and `#blazor-error-ui` in `lightyellow` (`MainLayout.razor.css:95`).

## 6. Requirements every design must meet

Each requirement is testable. It is listed with its task and the evidence before the fix; paths are under
`design/baseline/`.

| # | Requirement | Tasks | Evidence |
|---|---|---|---|
| A1 | **Contrast in the tokens.** Text is 4.5:1; control borders and the focus ring are 3:1 (WCAG 1.4.3, 1.4.11). The sidebar needs its own focus-ring token that reaches 3:1 on both ends of its gradient. See the failing pairs below. | T322 (P2) | `act-A/A.7.14-1-signin-contrast.png`, `act-A/A.7.14-2-profile-saved-success-alert.png`, `act-A/A.7.14-4-patel-lockout-button-focused.png`, `act-A/A.7.14-6-reviews-badges.png` |
| A2 | **Colour is never the only signal.** A status is written in words; a dot may sit beside it as a secondary cue, hidden from screen readers. | T327 | `act-A/A.6.2-1-system-health.png` |
| A3 | **At 390 px nothing scrolls sideways.** The item editor fits with Save in view; activity inputs are ≥ ~290 px; chart text is ≥ 11 px or a table replaces the chart. **At 1280 px** long emails and GUIDs wrap, and wide tables show their actions. | T323 | `act-A/A.7.10-5-kruger-item-editor-viewport.png`, `states/activity-view--narrow.png`, `act-A/A.7.6-2-zulu-trajectory.png`, `states/invitations-list--check-address.png`, `states/user-detail--pending-invitations.png` |
| A4 | **Targets, fonts, focus and order.** Every target is ≥ 24 px; controls use the body font; a scroll region shows the design's focus ring; filters come before Apply and the table in tab order; each password toggle exposes `aria-pressed` and names its input; badges keep their pill shape; header actions share one height. | T328 | `states/login--narrow.png`, `states/data-rights--narrow.png`, `act-A/A.7.5-3-decisions-due-summary-scrolled.png`, `states/home--narrow-trainee.png` |
| A5 | **Every action reports its outcome** in a result region that takes the focus. A refused move keeps its typed note. A button is not disabled by its own action. A stale refusal mark goes when its value changes. Destructive actions confirm and name the target, and their trigger is an outline button. | T299, T264 | `act-A/A.7.1-3-submitted-requested.png`, `states/activity-view--decline-refused.png`, `act-6/6.8-1-delete-refused.png` |
| A6 | **Every page designs its loading, load-error and not-found states.** The header shows from the first render; a skeleton shows while loading; the alert shows with no empty state under it; no action is offered before the record has loaded. | T329 (P2) | `states/activity-view--loading.png`, `states/epas-list--load-error.png`, `states/panels-list--loading.png`, `states/activity-type-edit--not-found.png` |
| A7 | **Failure screens.** A designed error page carries a request id, signed in or out. Access denied is drawn once. The reconnect dialog shows one message per state. | T321, T330 | `act-A/A.6.3-4-back-to-institutions-denied.png`, `act-A/A.5.8-2-error-signed-out.png`, `states/shell--reconnect-retrying.png` |
| A8 | **People by name, states and types by label, fields by their own label.** Ids stay only in the audit log and an audit entry, the data-rights list and a request, and `/portfolio/verify`. | T324 | `states/scheduled-job-runs-list--loaded.png`, `act-1/1.27-2-form-saved-preview.png`, `states/institution-edit--invalid.png`, `states/entrustment-scale-edit--rung-refused.png`; Home greets by email, not name (`act-A/A.5.3-1-forged-switch-trainee-view.png`; observed, not filed) |
| A9 | **One clock, labelled.** Every time is South African time with its zone ("2026-09-26 15:14 SAST"), and every "today" is the South African date. | T325 | `act-3/3.47-1-my-msf-reports.png` against `act-3/3.46-1-report-released.png`; `act-3/3.32-1-nudge-run.png` |
| A10 | **Copy says what the page does for this viewer**, and "no match" is not "empty". | T326 | `act-1/1.22-1-scales-read-only.png`, `states/activity-types-list--no-match.png`, `states/curriculum-items-edit--no-items.png`, `states/group-mappings--no-provider.png` |
| A11 | **Offer only what the caller can do.** Pickers offer exactly what the command accepts; records the caller cannot write get read-only views. | T300, T302, T303, T291, T304, T301 | § 10 (pre-fix) |
| A12 | **The nav marks where you are,** never two items at once. | T331 | `states/campaign-report--loading.png`, `states/specialities-list--loading.png` |
| A13 | **Operations pages report the truth.** Counts say what they count; the run filter is a select; lists page with "Showing 1–50 of N". | T327, T277 | `act-A/A.2.10-2-partial-key-nothing.png` |
| A14 | **One page-title pattern, and unique accessible names.** This covers the alert's dismiss button (today "×"), the pager's size select, the builder's selected tab, and dashboard links. | T190, T280 | `act-A/A.7.13-1-zulu-review7-top.png`, `act-A/A.7.9-5-builder-top.png` |

**A1's failing pairs** (T322, measured at step A.7.14):

| Pair | Ratio |
|---|---|
| Success text on its tint | 2.55 |
| Warning text on its tint | 2.42 |
| Danger text on its tint | 3.57 |
| Validation text on white | 3.82 |
| White on the danger button | 3.82 |
| White on the success button | 2.87 |
| The focus ring on the page | 2.99 |
| The input border | 1.49 |

## 7. Screens that do not exist yet and must be decided

| # | Missing | Source | What the design must decide |
|---|---|---|---|
| B1 | **The graduate's home.** Today she lands on "No role assigned". | T311 (P2); `states/home--no-role.png`, `act-5/5.22-2-molefe-authorisations-denied.png` | A former-trainee dashboard: "You completed your programme on …", with My progress, My authorisations and Export portfolio. Her nav, and which pages admit her (F13). |
| B2 | **Staff portfolio export.** It is reached only by typing `/portfolio/export/{userId}` and never names the trainee. | T314; `states/export-portfolio--staff.png` | A header naming the trainee, and links from the review page and the trainee profile page (F13). |
| B3 | **Applying an approved correction.** No page does it. | T316 (P2); `states/data-rights-request--approved.png` | Who rectifies, then a Correction card on the request (F14). |
| B4 | **Training year** beyond My progress. | T306; `act-2/2.29-2-four-admitted.png` | A column on Active profiles, a line on the admit form, and the trainee's targets card (F05, F11). |
| B5 | **The trainee's view of her decisions and appeals.** | T308; `states/my-reviews--appealed.png` | A decision history shared with the committee's review page, and an Appeals block (F09). |
| B6 | **STARs no longer in force.** | T319 (P2); `states/my-authorisations--after-revocation.png` | A "No longer in force" section; the data-rights decision note (F09, F14). |
| B7 | **The five "Coming soon" items** (`NavMenu.razor:108,113,114,116,132`). | coverage.md § Flows and states not played | For each, design it or drop the nav item: Recent Activities (F04); Stalled Activities with triage, Programme Trainees, and STAR Review Queue (perhaps simply "Entrustment decisions") (F06); System (F18). Their contents are inferred: no intent document exists. |
| B8 | **Pages reached only by address:** `/admin/entrustment-decisions` (speciality admins), `/admin/institutions/{id}` (her own), `/committee/panels` (Coordinator), `/portfolio/authorisations` (not in the nav), `/account/logout-confirm`, `/Error`. | coverage.md § Reached only by address | A link, or a reason for none (F09, F12, F08, F02, F01). |
| B9 | **The appeal outcome form.** | T307 (group 1), T309 | Outcome opens empty; "Replacement conditions" on remit; "Upheld" may go (F09). The scheduling preview warns when the seat is held (F08). |
| B10 | **Implied but not built:** self-service password reset, rebuilding one trainee's progress, STAR certificate verification, and the builder's visual workflow and credit editors. | coverage.md; T019-b…g | Show as future, or leave out; not assumed built (F02, F15, F13, F17). |
| B11 | **The College's builder and a read-only builder.** | T300 (group 1) | Activity Types in the CollegeAdmin's nav; a "Set by the College" notice; View or Edit per row (F17). |
| B12 | **An ended trainee profile** as a read-only record. | T305 (P2); `states/trainee-profile-edit--completed.png` | Details list, no Save (F13). |
| B13 | **The Upcoming deadlines card.** | T298 | Remove it, or give it a real source (F05). |

## 8. The flow index

The flows are in briefing order. The order is the shared frame first, then frequency × stakes. Frequency comes from
the programme's calendar: 25 observations per registrar per semester (`act-1-setup.md:399`); a review each semester or
year; one MSF campaign per period. Stakes come from task severity. Mode is W for wireframe first, F for straight to
fidelity. "Held" names a group-1 task whose screenshots must be re-captured before briefing (§ 10).

| # | File | Flow | People | Pages (coverage.md templates) | Steps | Mode | Held |
|---|---|---|---|---|---|---|---|
| 01 | `flows/01-shell.md` | The shared frame: nav, Home frame, role switch, system states | everyone | `/`, `/access-denied`, `/not-found`, `/Error`, `/placeholder/{Feature}` | 21 | W | — (T297 re-captured 2026-09-26) |
| 02 | `flows/02-sign-in-and-account.md` | Getting in, staying in, one's own account | everyone, anonymous | `/account/login`, `logout-confirm`, `profile`, `change-password`, `forgot-password`, `link-external` | 19 | F | — |
| 03 | `flows/03-trainee-files-activity.md` | A registrar asks for an assessment or logs one | Trainee | `/activities/new`, `/activities/{ActivityId:int}`, `/activities/mine`, `/activities/inbox` | 30 | W | — (T297 re-captured 2026-09-26) |
| 04 | `flows/04-assessor-inbox.md` | An assessor works their inbox | Assessor | `/`, `/activities/inbox`, `/activities/{ActivityId:int}`, `/placeholder/{Feature}` | 17 | W | — (T297 re-captured 2026-09-26) |
| 05 | `flows/05-trainee-progress.md` | A registrar reads where they stand | Trainee | `/`, `/portfolio/progress`, `/activities/mine` | 17 | W | — (T297 re-captured 2026-09-26) |
| 06 | `flows/06-programme-oversight.md` | Who is behind, and what has stalled | CommitteeMember, Speciality/SubSpecialityAdmin, Coordinator | `/`, `/placeholder/{Feature}`, `/activities/inbox`, `/committee/panels` | 18 | W | — (T297 re-captured 2026-09-26) |
| 07 | `flows/07-review-sitting.md` | The committee sits and ratifies | CommitteeMember | `/committee/reviews/{ReviewId:int}`, `/activities/{ActivityId:int}`, `/committee/reviews`, `/committee/panels` | 26 | W | — |
| 08 | `flows/08-committee-calendar.md` | Panels, what is due, the schedule | InstitutionalAdmin, Speciality/SubSpecialityAdmin, Coordinator, CommitteeMember | `/committee/panels` (+`new`, `{PanelId}`), `/committee/decisions-due`, `/committee/reviews`, `/committee/reviews/{ReviewId:int}`, `/access-denied` | 30 | F/W | — |
| 09 | `flows/09-outcomes-stars-appeals.md` | STARs, the register, a revocation, an appeal | Trainee, CommitteeMember, speciality admins, InstitutionalAdmin | `/portfolio/authorisations`, `/committee/my-reviews`, `/committee/reviews/{ReviewId:int}`, `/admin/entrustment-decisions`, `/committee/decisions-due`, `/access-denied` | 17 | F/W | T307 |
| 10 | `flows/10-msf-campaign.md` | An MSF campaign from set-up to release | Coordinator, anonymous respondents, Trainee | `/msf/campaigns` (+`new`, `{CampaignId}`), `/msf/respond`, `/msf/reports/{CampaignId:int}`, `/msf/my-reports` (+`{CampaignId}`), `/msf/coverage` | 18 | F | — |
| 11 | `flows/11-onboarding.md` | From invitation to admission | Administrator, InstitutionalAdmin, invitees, PendingTrainee | `/admin/invitations`, `/account/register`, `/`, `/admin/users` (+`{UserId}`), `/admin/trainees` (+`edit`), `/account/profile`, `/activities/mine`, `/activities/new`, `/access-denied` | 29 | F/W | T303 (T297 re-captured 2026-09-26) |
| 12 | `flows/12-people-admin.md` | Users, roles, assessors, locks and resets | InstitutionalAdmin | `/admin/users` (+`{UserId}`), `/admin/assessors` (+`edit`), `/admin/institutions/{Id:int}`, `/admin/sso/group-mappings`, `/` | 15 | F/W | T303 (T302 re-captured 2026-09-26) |
| 13 | `flows/13-graduation-and-exit.md` | Portfolio, verification, completion, the graduate's record | Trainee → former trainee, InstitutionalAdmin, Coordinator, verifier | `/portfolio/export` (+`{TraineeUserId}`), `/portfolio/verify`, `/admin/trainees` (+`edit`), `/admin/users/{UserId}`, `/portfolio/progress`, `/`, `/committee/my-reviews`, `/portfolio/authorisations`, `/msf/my-reports`, `/activities/*`, `/committee/reviews` | 26 | W | T303 (T297 re-captured 2026-09-26) |
| 14 | `flows/14-data-rights.md` | Data rights, requested and decided | Trainee, Coordinator, Administrator | `/account/data-rights`, `/admin/data-rights` (+`{Id:guid}`), `/not-found`, `/account/login`, `/admin/users`, `/admin/trainees`, `/committee/reviews` (+`{ReviewId}`) | 19 | F/W | — |
| 15 | `flows/15-institution-adopts-catalogue.md` | Adopt, add local items, move to a new version | InstitutionalAdmin, Administrator | `/admin/curricula` (+`{Id}/items`), `/admin/epas` (+`new`, `{Id}`), `/admin/adoptions`, `/admin/entrustment-scales`, `/admin/entrustment-decisions`, `/admin/trainees/edit`, `/admin/curriculum-progress`, `/portfolio/progress` | 16 | F | — |
| 16 | `flows/16-college-catalogue.md` | The College keeps the national catalogue | CollegeAdmin, Administrator | `/admin/colleges/**`, `/admin/specialities/**`, `/admin/entrustment-scales/**`, `/admin/epas/**`, `/admin/curricula/**`, `/access-denied` (21 templates) | 33 | F | T300 (sidebar) |
| 17 | `flows/17-activity-type-builder.md` | Build and publish an activity type | InstitutionalAdmin, Administrator (CollegeAdmin after T300) | `/admin/activity-types` (+`new`, `{ActivityTypeId}`) | 10 | W | T300 |
| 18 | `flows/18-platform-operations.md` | The operator keeps the platform running | Administrator; InstitutionalAdmin (audit) | `/`, `/admin/institutions/**`, `/admin/jobs` (+`runs`), `/admin/audit` (+`{Id:guid}`), `/admin/curriculum-progress`, `/admin/users/{UserId}`, `/placeholder/{Feature}` | 23 | W | — (T302 re-captured 2026-09-26) |

Coverage was checked by script on 2026-09-26 against the runbook, `coverage.md` and `design/baseline/states/`:
- every one of the 324 runbook steps is in at least one flow;
- all 80 page templates are placed;
- all 578 state captures are placed;
- every role in `coverage.md` § Journeys by role is served (§ 1).

Every group-3 task (the presentation debt) is a requirement in § 6 or a screen in § 7. The five "Coming soon" items are
B7.

| Task | Where | Flows |
|---|---|---|
| T299 | A5 | F03, F04 |
| T306 | B4 | F05, F11 |
| T308 | B5 | F09 |
| T311 | B1 | F13 |
| T314 | B2 | F13 |
| T316 | B3 | F14 |
| T321 | A7 | F01, F12 |
| T322 | A1 | F01 (the tokens); also named in F02, F04, F06, F07, F10, F12 |
| T323 | A3 | F03, F04, F05, F07, F11, F12, F14, F16 |
| T324 | A8 | F01, F02, F09, F11, F12, F14, F16, F17, F18 |
| T325 | A9 | F01, F07, F09, F10, F14, F18 |
| T326 | A10 | F12, F15, F16, F17, F18 |
| T327 | A2, A13 | F18 |
| T328 | A4 | F01, F02, F03, F05, F06, F08, F14 |
| T330 | A7 | F01 |
| T331 | A12 | F01, F10, F16 |

## 9. The handoff and the acceptance check

**Bringing the design back:**
- **From claude.ai/design:** Export, then "Handoff to Claude Code", then "Send to local coding agent" [start]. The
  bundle holds the design files, the chat and a README [academy].
- **From `/design`:** pick the artboard and tell Claude to implement it [w34].
- **Either way, save the bundle or artboards** under `design/flows/NN-<slug>/`.

**Claude Code implements each flow as one task** (`harness.py task new`), committed per task (CLAUDE.md § Task
management):
1. **Colours.** Map every colour to a `:root` token. A new token goes into `:root` and DESIGN.md § Design tokens in the
   same change.
2. **Markup.** Map every element to an existing component or class. A new class is defined in `app.css`, or
   `DefinedClassTests` fails. Keep these patterns:
   - `.clinic-table` in `.table-container`;
   - `.form-container`, `.form-grid` and `.form-actions`;
   - `StatePanel` on every list;
   - `.dashboard-grid`.
3. **Icons.** Add any missing Lucide SVG to `wwwroot/icons` with `id="i"`, and render it through `<Icon Name=…>`.
4. **Strip what the handoff brings.** Remove CDN fonts and scripts, utility classes, inline `<style>` and hex literals.
   Any script becomes a `wwwroot/js` module linked through `@Assets`.
5. **Static pages.** Build them without interactivity, and keep the framework hooks: `EditForm` and `ValidationMessage`,
   `NavLink`, the reconnect class names, and `#blazor-error-ui`.
6. **A deliberate break of a DESIGN.md rule.** Amend DESIGN.md and the tests that pin it (§ 4, § 5.4) in the same task,
   citing the operator's decision.
7. **Wording.** If the redesign changes on-screen wording, update the runbook's `Expect:` lines in the same task. The
   steps quote the screen.

**A flow is done when all of these hold:**
- **The tests pass.** `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj` is green (never with `--no-build`:
  CLAUDE.md § Testing). That run includes the Design, Navigation, Accessibility and Hosting tests, and **T294's
  guard**, `tests/Wombat.Web.Tests/Scenario/`, which fails if a page has no step, a step names a route that does not
  exist, or a step breaks the format.
- **The steps replay.** The flow's runbook steps replay on a fresh database:
  - create the database with `tools/scenario-replay.ps1 create wombat_scenario_<flow>`;
  - publish with `tools/scenario-replay.ps1 publish`;
  - start the app with `tools/scenario-replay.ps1 start wombat_scenario_<flow>`;
  - play the acts up to and through the flow's steps (`README.md` § How to play). Every Expect must hold.
- **The baseline is re-captured.** The flow's step and state screenshots are taken again into `design/baseline/`
  (`states.md` § How to capture) and compared with the chosen artboards.
- **The browser check passes** for each role in the flow, at 1280 and at 390.

## 10. Status of the baseline

- **What it holds.** `design/baseline/` (gitignored, `.gitignore:427`) holds 1,250 PNGs from the T295 replay of
  2026-09-26:
  - 672 step captures: act-1 53, act-2 134, act-3 131, act-4 56, act-5 58, act-6 85 and act-A 155, named
    `<step>-<n>-<slug>.png`;
  - 578 state captures, `states/<page>--<state>.png`.
- **How it matches `states.md`.** `states.md` names 578 captures:
  - Two named captures were not taken: `login--sso-<code>` (no provider is configured) and `my-progress--december` (a
    December replay only).
  - Two more are named only in the "how to reach" column and were not taken either: `change-password--throttled`
    (`states.md:218`) and `epa-edit--local-reactivated` (`states.md:536`). F02 and F15 describe them in words.
  - Two files on disk are named only in `states.md`'s prose: `review-detail--appeal-upheld` and
    `review-detail--decided-elsewhere`.

**Pre-fix (group 1).** These tasks are being fixed now. Until each fix lands and its pages are re-captured, do not brief
from these images: the fix changes what the page shows. Each flow file repeats its own list under PRE-FIX.

| Task | Change | Screenshots to re-capture |
|---|---|---|
| T297 (in progress) | Five dashboard cards read literal state keys: the Assessor, Trainee, Coordinator, SpecialityAdmin and SubSpecialityAdmin homes change | Every capture of those five homes. By role (the planner's list, extended by persona; the extension is inferred): act-2 `2.8-2`, `2.9-3`, `2.10-4`, `2.10-6`, `2.10-7`, `2.31-2`, `2.32-1`, `2.34-1`, `2.36-1`, `2.36-2`, `2.38-1`, `2.38-2`, `2.39-1`, `2.40-1`, `2.40-3`, `2.40-5`, `2.40-7`; act-3 `3.12-1`, `3.16-1`, `3.24-1`, `3.30-1`, `3.33-2`, `3.50-1`, `3.51-1`, `3.53-1`, `3.53-2`, `3.54-1`; act-4 `4.3-1`; act-5 `5.28-1` (ended trainee, inferred); act-A `A.4.6-1` (ended trainee, inferred), `A.4.7-1`, `A.5.3-1`, `A.5.10-1`, `A.6.8-2`, `A.7.3-1`, `A.7.5-1`, `A.7.7-1`, `A.7.8-1`; states `home--assessor-empty`, `--assessor-pending`, `--assessor-decisions`, `--assessor-switched`, `home--coordinator-empty`, `--coordinator-stalled`, `--coordinator-expiring`, `home--speciality-admin`, `--speciality-admin-figures`, `home--sub-speciality-admin`, `home--trainee`, `--trainee-first`, `--trainee-returned`, `--trainee-ended` (inferred), `home--narrow-assessor`, `--narrow-coordinator`, `--narrow-speciality-admin`, `--narrow-sub-speciality-admin`, `--narrow-trainee`. **Re-captured on 2026-09-26** after T297 landed (7bf8ea7), from a fresh replay of Act 3 and a post-actA copy: act-3 `3.12-1`, `3.16-1`, `3.24-1`, `3.30-1`, `3.33-2`, `3.50-1`, `3.51-1`, `3.53-1`, `3.54-1`; act-A `A.6.8-2`. Not re-captured: act-3 `3.53-2`, because the Pending reviews card no longer links to the inbox it showed, so no step reaches it; do not attach it. **Every other capture in this row was re-captured on 2026-09-26 too**, each from the end-of-act snapshot of the act it belongs to, so it shows the end of that act, not the step's exact mid-act moment: act-2 (all 17) from `scenario-post-act2`, so Dr Mokoena's and Dr Sithole's Act 2 Homes already count the 5 admitted registrars; act-4 `4.3-1` from post-act4; act-5 `5.28-1` and `home--trainee-ended` from post-act5; the eight other act-A captures, the five `home--narrow-*` and `home--coordinator-expiring` (with states.md's scratch invitation) from post-actA. The other states come from the act their states.md row names: `--assessor-empty`, `--assessor-switched`, `--coordinator-empty`, `--speciality-admin` and `--trainee-first` from post-act2; `--assessor-decisions`, `--speciality-admin-figures`, `--sub-speciality-admin` and `--trainee` from post-act3; `--assessor-pending`, `--coordinator-stalled` and `--trainee-returned` from post-act3 with two SQL stand-ins for the mid-act moment (Dr Mahlangu's Mini-CEX, activity 21, set back to requested and aged 8 days; Dr Ndlovu's reflection, activity 4, set back to Draft). Nothing in this row is held now; `3.53-2` is retired. |
| T300 | Builder: read-only mode, narrowed Scope, View or Edit per row; Activity Types in the CollegeAdmin's nav | act-1 `1.24-1`, `1.25-1`…`1.25-5`, `1.26-1`, `1.26-2`, `1.31-1`, `1.31-2`; act-A `A.7.9-6`; states `activity-types-list--*` (6), `activity-type-edit--college-instrument`, `--new`, `--loading`, `--not-found`, `--metadata`; Dr Kruger's dashboard (`home--college-admin`, `home--narrow-college-admin`, act-6 `6.10-1`, act-A `A.7.10-1`). In every other 1280 px capture of Dr Kruger the sidebar is pre-fix: brief from the page, not the nav (F16). The 390 px `A.7.10-2` to `A.7.10-5` fold the nav, so T300 does not change them (observed on `A.7.10-5`). |
| T302 (in progress) | Institution page: Status as text for an InstitutionalAdmin, with no box and no Deactivate; Deactivate and Reactivate as commands for an Administrator, who keeps the Active box (Save sends a changed box as the state's own command, and unticking asks first) and a Deactivate behind a confirmation. Back and Cancel lead home for anyone but an Administrator (T291 item 7) | act-1 `1.23-1`; act-A `A.6.3-1`, `A.6.3-2`, `A.6.3-3` (these states cease to exist), `A.6.3-5`; states `institution-edit--own`, `--deactivate-refused`, `--administrator`, `--deactivated`, `--saved`, `--narrow`. **Re-captured on 2026-09-26** after T302 landed (41be531): `1.23-1`, `institution-edit--own` and `--administrator` from a post-act1 copy; `A.6.3-1`, `--saved`, `--narrow` (Step A.6.1 replayed first) and `--deactivated` (the Demo Institution) from a post-act6 copy. The states that ceased keep their file names and now hold the new page: `A.6.3-2-deactivate-refused` is the form after her save (Status "Active" as text, "Set by a global administrator.", Cancel and Save only; an element capture), `A.6.3-3-untick-active-saved` is the same page with "Back to home" focused, `A.6.3-5-devadmin-reactivates-kgk` is her Home, where "Back to home" and Cancel lead, and `institution-edit--deactivate-refused` is her full page after the save. Also re-captured, because the fix changed them: act-1 `1.6-2` and act-A `A.6.1-2` (held by flow 18), and `1.23-4` (the create form she can still open, T291 item 8, now with "Back to home"). No step reaches act-1 `1.23-2-back-link-access-denied` or act-A `A.6.3-4-back-to-institutions-denied` any more, because her links lead home: do not attach them as her page. `A.6.3-4` remains the evidence of T321's nested layout, which is still open (not re-checked here). |
| T303 | User page: Trainee "System-managed", never under Add role | act-2 `2.28-3`; act-5 `5.17-3`; act-A `A.7.9-3`; states `user-detail--pending-trainee`, `--no-roles`, `--reset-refused`, `--reset`, `--narrow`; probably `--other` and `--role-added` (inferred) |
| T307 | Appeal card: Outcome opens empty; replacement conditions; Upheld probably removed | act-4 `4.45-1`, `4.46-1`, `4.47-1`; states `review-detail--appeal-form`, `--appeal-member`, `--remit-refused`, `--remitted`, `--appeal-dismissed`, `--appeal-upheld` (may disappear) |

**Other backend fixes that will change what a page shows.** These do not hold a brief, but re-capture after they land:
- T304: curriculum pickers, and a save that reports replayed credit;
- T305: an ended profile becomes read-only;
- T312: an ended record freezes its EPA list;
- T313: the verify messages;
- T319: "No longer in force", and decision notes;
- T320: data-rights help text;
- T329: loading and error states.
