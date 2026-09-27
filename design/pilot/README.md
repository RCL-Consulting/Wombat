# The pilot: flow 01 from Claude Design to a green replay (T335)

This is the operator's step-by-step for the pilot, from nothing to an accepted flow. Written 2026-09-27.

**Why a pilot.** W-008 chose a restructure with UX in scope. It covers 80 page templates, 15 emails and the PDFs
(`design/BRIEF.md` § 0, § 4). Nobody has yet taken a flow from a Claude Design canvas to Razor under `DESIGN.md`, the
CSP and the Blazor render modes. The brief also rests on claims about the tool that it could not verify (BRIEF § 2.1,
§ 2.2). Running all the flows on those claims would repeat any mistake 20 times. So flow 01, the shell, goes first, end
to end. What it teaches corrects the brief before flow 02 is briefed (T335).

**What never leaves the machine.** Never write a password or a registration token into a tracked file. That covers
this README, the task file, the runbook's Actual lines and the brief. Passwords chosen during a replay go into
`pwd_DO_NOT_COMMIT.txt` only (`execution/knowledge/scenario-paediatrics/README.md` § How to play). BRIEF § 3.2 lists
what is never uploaded.

| Step | Who | Done when | Status |
|---|---|---|---|
| A. Choose and set up the design system | Operator; Claude | The design system holds Wombat's tokens, type, icons and brand, and passes its checks; the choice is recorded | **Done 2026-09-27** (W-009): Wombat's own, built from the code. The aesthetic direction was given at step C |
| B. Stage the upload set | Operator | `stage_upload.ps1 -Flow 01` exits 0; every screenshot has been opened; the crop-first file is cropped or left out | Ready |
| C. Start the canvas | Operator | A Design artifact holds round 1's 2–3 structural variations; its link is in T335 | Done 2026-09-27 (canvas https://claude.ai/artifact/R86QvLEyyfKx98fT4MENcD; A picked; round 3 accepted) |
| D. Review the structural variations | Operator | Each variation is scored against the checklist, in the chat | Done 2026-09-27 (canvas https://claude.ai/artifact/R86QvLEyyfKx98fT4MENcD; A picked; round 3 accepted) |
| E. Choose, then fidelity and states | Operator | The chosen frame's artboards and token sheet are in `design/flows/01-shell/`, and § 6's nine answers are in the chat | Done 2026-09-27 (canvas https://claude.ai/artifact/R86QvLEyyfKx98fT4MENcD; A picked; round 3 accepted) |
| F. Hand off to Claude Code | Claude Code | Built; `DESIGN.md` amended; suites green; committed | Built on branch t335; squash pending |
| G. Acceptance | Claude Code; operator | Flow 01's steps replay green on a fresh database; the baseline is re-captured | Waits on F |
| H. Lessons into the brief | Claude Code; operator | BRIEF § 11 is written, dated, and flow 02 is restated | Waits on G |

---

## A. Choose and set up the design system

**Chosen on 2026-09-27: path 1, Wombat's own design system, built from the code** (W-009). It is built:
**https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18** (private).
- **What it holds:** 79 tokens from `app.css` `:root` and the layout CSS (24 colours, 6 spacing, 6 radius, 7 shadow, 16
  size), 20 type styles in 4 groups, 18 components with guidelines and live previews (named as BRIEF § 5.1 lists them,
  class names unchanged), and the wombat marks, 25 Lucide icons and favicons copied from `wwwroot`. One theme, light:
  Wombat has no dark theme today.
- **Its source is tracked** in `design/system/` (tokens, README, component guidelines and previews; the assets and the
  font are the `wwwroot` originals). Re-sync from the code when `app.css` changes, as the type's `from-code.md` says.
- **Known to fail, kept and flagged:** every pair T322 lists, plus the ok and warn status dots and the complete progress
  bar. The design system's README says flow 01's token sheet replaces them; nobody designs from the failing values.
- **Rejected:** path 2 below (RCL Consulting's), a blend of the two, and designing with no system. See W-009.

The rest of this step is kept as the record of how it was done, and for a re-build.

### Path 1: build a Wombat design system from the code

1. **Stage the set:** `pwsh design/tools/stage_upload.ps1 -DesignSystem`. It stages 39 files, 1.8 MB, into
   `design/upload/`:
   - `app.css`, the font, the brand SVGs and the 25 icons;
   - `DESIGN.md` and the brief;
   - BRIEF § 2.3's seven example screenshots.
2. **Fill the design system.**
   - **In a Claude Code session in this repo,** say: *"Fill the Wombat design system at
     https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18 from the code. Read only the files in design/upload/. The tokens
     are app.css :root (lines 11–45); the wordmark face is Fraunces; the icons are the Lucide SVGs in wwwroot/icons;
     the brand is wwwroot/brand. Name the components exactly as design/BRIEF.md § 5.1 lists them. Treat § 5.1 as
     constraints."*
   - **Or at claude.ai/design,** upload `design/upload/` without `crop-first/` [setup]. Do not link the repository
     (BRIEF § 2.2).
3. **Check what it extracted** (BRIEF § 2.3 step 1):
   - the palette equals `app.css` `:root`;
   - Fraunces is used on the wordmark only;
   - the icons are Lucide line icons;
   - no Google Fonts or CDN is used;
   - the component names are § 5.1's.
4. **Mark what is known to fail.** Today's tokens fail WCAG AA in eight pairs (BRIEF § 6 A1). Note in the design
   system's README that flow 01's token sheet replaces them, so that nobody designs from the failing values.

**Done when** the Wombat design system shows the palette, the type, the icons and the brand, and passes the five
checks, and its link is recorded in T335.

### Path 2: use the existing "RCL Consulting" design system

**What it brings,** observed from its `project/README.md` on 2026-09-27 (its `tokens.json` was not read):
- **Colour:** semantic tokens over four swatches (ink, ivory, teal, copper), with a light and a dark theme.
- **Type:** four families. Cinzel (capitals only) for display, Cormorant Garamond for headlines, Jost for text, and IBM
  Plex Mono for values that must align.
- **Shape:** square shapes; sections set off by hairlines and led by an eyebrow; no drop shadows.
- **Motion:** one animation, the mark's "pivot sweep", which honours `prefers-reduced-motion`.
- **Components:** six, for a website and an app's navigation.

**What it would mean for Wombat.** Each of these is a decision, not a detail:
- **Brand.** Wombat would look like an RCL product. Decide whether the Wombat mark and the Fraunces wordmark stay (BRIEF
  § 3.1 says to bring the logo, not ask for a new one).
- **Fonts.**
  - Each face must be self-hosted as a woff2 in `wwwroot/fonts`, and linked through `@Assets` (BRIEF § 5.1, § 5.2).
    IBM Plex Mono is held there as `.ttf`, so it needs converting.
  - Each licence must be checked as GPLv3-compatible (CLAUDE.md § Key technical choices). All four are usually
    distributed under the SIL OFL, as Fraunces is (inference; confirm from each font's own licence).
- **Components.** None of its six maps to Wombat's vocabulary: DataTable, FormField, StatePanel, Alert, badges and the
  others in BRIEF § 5.1. So § 5.1 still rules, and tables, forms, badges and alerts are new to it.
- **Icons.** Its README says no icon set is defined yet. Wombat keeps Lucide.
- **Status colours.** Its README names no success, warning, danger or info colours. Wombat needs all four, for alerts
  and badges, and T322's eight failing pairs are exactly these (BRIEF § 6 A1). Flow 01's token sheet has to add them.
- **Flow 01's question 7.** It answers the question its own way: Jost replaces the Segoe UI stack, and a dark theme
  comes in. Its reduced-motion rule covers its own animation only, so Wombat's skeleton pulse still needs one.

**Not chosen (W-009).** Kept as the record of what it would have meant.

---

## B. Stage the upload set

```powershell
pwsh design/tools/stage_upload.ps1 -Flow 01
```

- **What it stages** (on 2026-09-27): 78 files, 3.38 MB, in `design/upload/`:
  - 3 brief files: `DESIGN.md`, `BRIEF.md` and `flows/01-shell.md`;
  - 30 design-system files;
  - 44 screenshots from `design/baseline/`, which are the flow's § 4 Attach list.
- **What it keeps back:** one file, in `design/upload/crop-first/`. That is `act-1/1.5-2-kruger-invited.png`, which
  shows a one-time registration link (BRIEF § 3.3). It is attached only to show the underlined nav, so crop the link
  out or leave it out.
- **Open every screenshot before you upload it** (BRIEF § 3).
- **Upload `design/upload/` without `crop-first/`,** and nothing else: never the repo root, never `src/Wombat.Web`.
- **What the script refuses:** anything under `recovery/` or `.scenario-app/`, `*.dump`, `pwd_DO_NOT_COMMIT.txt`,
  `appsettings*.json`, user secrets, `bin/` and `obj/`. It refuses a flow brief that is not tracked. It stops if
  `design/upload/` is not gitignored.

**Done when** the script exits 0 with no "not found" warning, and every screenshot has been opened.

---

## C. Start the canvas

**Where:** a Claude Code session in this repo is recommended (BRIEF § 2.2). There, run `/design`, or ask Claude to
create a Design artifact. Or start it in the main claude.ai app: **Design → Make something new → Design**, choosing the
Wombat design system.

**Not the standalone homepage (claude.ai/design) for now** (observed 2026-09-27). Its design-system picker lists only
its own design-system projects and the built-in presets, not the artifact-based design systems, so Wombat does not
appear there. The main app's Design page says "Claude Design lives here now. New Slides and Design projects are created
as artifacts", and the Design System type's own instructions call the standalone app's systems a separate, older store
(a system "migrated from the standalone version"). Wombat was built as an artifact, so it lives on the main app's
Design page and in Claude Code, where a Design artifact can name it by its link.

**First message** (in Claude Code):

```text
Create a Design artifact for Wombat flow 01, the shell. Use the design system https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18.
Read only the files in design/upload/, not the rest of the repo.
The brief is design/upload/design/flows/01-shell.md: § 1 is the ask and § 8 is the runbook steps; follow § 1 exactly.
Leave out design/upload/crop-first/ unless I say a file there has been cropped.
This is round 1: 2–3 STRUCTURAL variations as wireframes, each showing a–h in § 1, with its reasoning and its click
counts. No colour or type choices yet. Stop after round 1 and wait for my pick.
```

**In the main claude.ai app's Design page** (the standalone homepage would not offer Wombat; see above):
1. Paste flow 01 § 1 (the ask), and attach the 14 key screenshots in flow 01 § 4: number 13, `1.5-2`, only once its
   link is cropped out.
2. Paste § 8 as the next message.
3. Attach § 4's landing and state captures as the chat asks for them.

**Done when** a Design artifact holds 2–3 structural variations, and its link is recorded in T335.

---

## D. Review the structural variations

Score each variation against every row. A row a variation fails is a question for the chat, not a reason to drop the
variation on sight.

| # | Check | Passes when | Source |
|---|---|---|---|
| 1 | **Navigation per role** | The Administrator's (20 today), the InstitutionalAdmin's (18), the Trainee's (10) and the Assessor's (6) are written out, with the grouping explained | BRIEF § 4.1; flow 01 § 1 a |
| 2 | **Nothing lost, nothing mislabelled** | Every link of today's (flow 01 § 1, NAV CONTENT) is kept, moved or dropped with a reason. Every link opens a page that admits the role, and no two links share a label. | BRIEF § 4.4; `NavMenuAuthorizationTests` |
| 3 | **The five "Coming soon" items** | Each is designed by a named later flow, or dropped | Flow 01 question 2; BRIEF § 7 B7 |
| 4 | **Several roles** | Dr Zulu's navigation is shown, and so is where she sees the role she is acting in | BRIEF § 4.1; flow 01 § 1 b |
| 5 | **Role switching** | Where it lives, and what it changes (the landing, the navigation or both), are stated. A role she does not hold is never offered. | Flow 01 § 1 d; Steps 2.34, A.5.3 |
| 6 | **Top bar** | Identity by name, not email; one Sign out | T324; flow 01 question 4 |
| 7 | **Landing and clicks** | Each role's landing is shown, with the clicks to its most frequent job beside today's (a Trainee 1, an Assessor 2) | BRIEF § 4.2; flow 01 § 1 e |
| 8 | **Outcomes and refusals** | A result and a refusal each show in view of the control that caused it, and the focus moves to the result | BRIEF § 4.1, § 6 A5; flow 01 question 9 |
| 9 | **System states** | Loading, load error, access denied (drawn once), not found, the error page with a request id, reconnect (one message per state), the error bar | BRIEF § 6 A6, A7 |
| 10 | **Where you are** | The active item has a rule, including for a page reached from several lists, and never two items are lit | BRIEF § 6 A12; T331; flow 01 question 5 |
| 11 | **390 px** | The navigation folds with a CSS-only toggle; nothing scrolls sideways; a 20-link navigation works on an 800 px-tall screen | BRIEF § 5.1, § 6 A3 |
| 12 | **Buildable** | Static pages stay static; no per-keystroke behaviour; no script beyond a ≤10-line module; Lucide icons only; nothing from a CDN | BRIEF § 5.1, § 5.2 |
| 13 | **Targets and order** | Targets are at least 24 px, and the tab order follows the reading order. Contrast figures come at fidelity, in round 2. | BRIEF § 6 A1, A4 |
| 14 | **Reasoning** | What the variation is best at, what it costs, which roles it serves worst, and which DESIGN.md rule and test it changes | Flow 01 § 1 h; BRIEF § 4.5 |

**Done when** each variation is scored, and the notes are in the chat as sentences, because the chat travels in the
handoff bundle [academy].

---

## E. Choose, then fidelity and states

1. **Pick one,** and give the reason in one sentence in the chat.
2. **Ask for round 2:** flow 01 § 1 SCREENS 1–7 and STATES TO SHOW, at 1280 and 390 px, ending with the token sheet.
   The sheet carries a contrast figure for every pair, including T322's eight (BRIEF § 6 A1).
3. **Answer flow 01 § 6's nine questions** in the chat, one sentence per decision.
4. **Ask for the accessibility review** against WCAG 2.1 AA [start].
5. **Export the chosen artboards** (PNG per artboard [cmds]) into `design/flows/01-shell/`. There is no version history
   [start]. They are scenario data, so they may be committed with T335. Open each first.
6. **Update the design system** from the token sheet (BRIEF § 2.3 step 6).

**Done when** `design/flows/01-shell/` holds every screen and state of the chosen frame and its token sheet, the nine
answers are in the chat, and the artifact's link is in T335.

---

## F. Hand off to Claude Code

**Bringing the design back:** pick the artboards and tell Claude to implement them [w34]. From claude.ai/design, use
"Handoff to Claude Code", then "Send to local coding agent" [start]. Save the bundle under `design/flows/01-shell/`.

**The implementation prompt, as sent** (in a Claude Code session in this repo). The `[As built: …]` notes, added after
the build, mark where what was built differs from what was asked.

```text
Implement flow 01, the shell, from the chosen design: <Design artifact link>, artboards and bundle in
design/flows/01-shell/, decisions in its chat. The task is T335. Follow design/BRIEF.md § 9, CLAUDE.md, and the
banner at the top of execution/architecture/DESIGN.md (W-008).

Razor:
- Components/Layout/MainLayout.razor and .razor.css, NavMenu.razor and .razor.css, AuthLayout.razor,
  ReconnectModal.razor, .razor.css and .razor.js.
- Components/Pages/Home.razor, and the frame of the nine Components/Pages/Dashboards/*.razor (the cards' contents
  belong to later flows).
- Components/Pages/AccessDenied.razor, NotFound.razor, Error.razor, Placeholder/PlaceholderPage.razor.
  [As built: PlaceholderPage is deleted, and its /placeholder/{Feature} route with it.]
- Components/Routes.razor and Components/App.razor, only if the frame needs it.
- Role switching and landing: the /dashboard/switch/{role} endpoint in Program.cs, and Navigation/DashboardPriority.cs.
  [As built: the switch is Navigation/ActingRoleSwitch.cs, mapped in Program.cs.]
- Any NEW component in Components/Shared/, added to BRIEF § 5.1's list.

Styles:
- Every colour is a token on app.css :root, with the token sheet's values; T322's pairs must pass.
- A new class is defined in app.css, or in the layout's own .razor.css.
- No utility classes, no inline <style>, no hex outside :root.

Fonts and scripts:
- A new typeface is a self-hosted woff2 in wwwroot/fonts with a GPLv3-compatible licence, linked through @Assets.
- A script is a module of 10 lines or fewer in wwwroot/js, linked through @Assets.
- Nothing from a CDN. The CSP in Security/SecurityHeadersMiddleware.cs does not change.

Render modes and framework hooks:
- Signed-out pages, /msf/respond and /portfolio/verify stay static, and the phone nav toggle stays CSS-only.
- Sign-in, sign-out and the other cookie writes stay form posts.
- Keep NavLink's active class, #blazor-error-ui and its .reload, the components-reconnect-* classes, EditForm and
  ValidationMessage.
  [As built: NavItemLink replaced NavLink, and writes its own active class.]

DESIGN.md:
- Rewrite the sections the design changes: § Design tokens, § Typography, § Layout grid (the shell), § The NavMenu
  (its role table), § Dashboard layout grid, § Page-level patterns › Dashboard page, and § Alerts, validation, empty
  states and § Accessibility where they change.
- Add flow 01 to the banner's "Redesigned so far" list, with the date and the commit.

Tests, in tests/Wombat.Web.Tests:
- Update Navigation/NavMenuAuthorizationTests.cs (it parses DESIGN.md's nav table), DashboardLinkAuthorizationTests.cs
  and PageAccess.cs.
- Update Dashboards/DashboardCardTests.cs, DashboardPriorityTests.cs, WaitingCardsTests.cs and
  AssessorDashboardSubjectNameTests.cs.
- Update Design/NarrowLayoutTests.cs (the shell's gutter and breakpoint), DesignSystemSmokeTests.cs,
  PageShapeSmokeTests.cs, DefinedClassTests.cs, AlertRoleTests.cs, BadgeForStatusTableTests.cs,
  InvalidFieldStyleTests.cs and FieldGroupTests.cs.
- Add Design/ContrastTests.cs for T322's pairs.
- Check Hosting/AppHeadTests.cs (the page title, T190), AppAssetUrlTests.cs and BlazorEndpointAccessTests.cs.
- Keep Accessibility/* green, and Scenario/ (T294's guard).
- Replace what a test pinned with what the new design guarantees. Never delete a test to make room.

Runbook:
- Every step whose wording the frame changes gets its Expect updated in the same task. Find the candidates with:
  rg -n -i "viewing as|switch view|\bnav\b|sidebar|coming soon|logout|sign out|menu" execution/knowledge/scenario-paediatrics/
  A keyword search of the Role, Route, Do and Expect lines on 2026-09-27 found 59 steps. 15 of them are among flow
  01's 21 steps; check the other six (A.5.1, A.5.3, A.5.5–A.5.8) as well, 65 steps in all.

Run:
- dotnet build Wombat.sln -c Release.
- dotnet test on Wombat.Web.Tests and Wombat.Architecture.Tests, never with --no-build.
- Commit once, for the task.
```

**Done when** the suites are green (quote the counts), `DESIGN.md` is amended and its banner lists flow 01, and the
work is committed.

---

## G. Acceptance

The acceptance is BRIEF § 9, on a fresh database. Flow 01's steps run from Act 2 to the appendix, so the replay is the
whole runbook in order. A copy of a `recovery/scenario-post-act*.dump` is not a fresh database. Use one only to
re-capture a state that `states.md` marks "Scratch (post-actN)", never on the replay's own database.

1. **A fresh database and the published app**, beside the dev app:
   ```powershell
   tools/scenario-replay.ps1 create wombat_scenario_f01
   tools/scenario-replay.ps1 publish
   tools/scenario-replay.ps1 start wombat_scenario_f01
   ```
   The app listens on http://localhost:5180. Each email's body goes to `.scenario-app/wombat_scenario_f01.log`, which is
   never uploaded.
2. **Play the runbook** as `execution/knowledge/scenario-paediatrics/README.md` § How to play says: Acts 1 to 6, then
   the appendix through A.7.14. Write each step's `Actual (<date>, T335 replay, wombat_scenario_f01): …` line.
   - Every Expect must hold: flow 01's 21 steps and every other step that quotes the frame (step F's search).
   - Passwords go into `pwd_DO_NOT_COMMIT.txt` only.
3. **Re-capture the baseline** into `design/baseline/`, as `states.md` § How to capture says. That covers the rows of
   states.md § Shell and framework, § Home and the role dashboards, and § System pages, the landing captures (§ 4) and
   the steps' own captures.
4. **Compare** each capture with its chosen artboard, and list the differences in T335.
5. **Browser check** at 1280 and at 390 px for every row of DESIGN.md's nav table, and for a two-role user. Check the
   reconnect dialog by suspending the app (`states.md` § Shell and framework).
6. **Stop the app:** `tools/scenario-replay.ps1 stop`.

**Done when** the 21 steps' Actual lines hold, the baseline is re-captured, and T335's fourth box is ticked with them.

---

## H. Lessons into the brief

**Write BRIEF § 11, "Pilot findings",** dated. Each finding says what it changed in the brief or the flow files. Record
at least:

| Topic | The question the pilot answers |
|---|---|
| Design system | Which path, what imported well and what did not, and the checks' results |
| Upload set | What was missing or superfluous; whether `.md` and PNG uploads worked; any limit on attachments |
| Structural round | Were a–h and step D's checklist enough? Did the variations differ in structure, or only in look? |
| Fidelity | How much survived from artboard to Razor, per screen (the 50–75% estimate, BRIEF § 2.1) |
| Handoff | What the bundle held, and what Claude Code had to strip (CDN, utility classes, inline styles) |
| Build | The files and tests touched, against step F's list; which pins broke; the `DESIGN.md` sections rewritten |
| Replay | How many Expect lines changed, against the 59 predicted; what the full replay cost; whether later flows may accept on less (a decision to record, not assume) |
| Template | What changes in BRIEF § 2.4, and in flows 02–18. Their ASK line still reads "2–3 variations; <wireframe \| full fidelity>", and their `DESIGN.md` line numbers are stale (BRIEF § 2.0). |

**Then:**
- correct BRIEF § 2.4, § 5 and § 9 by the findings;
- restate flow 02 as flow 01 was restated;
- mark T335 done with `python <harness>/bin/harness.py task done T335` (CLAUDE.md § Driving the harness).

**Done when** BRIEF § 11 is written and dated, flow 02 is restated, and T335's last box is ticked.
