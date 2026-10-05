# Wombat GUI redesign: the master brief

For the operator running the redesign in Claude Design, and for Claude Code implementing its result (T332). Written
2026-09-26. Revised on 2026-09-27 for the operator's decision (W-008), the pilot (T335) and the emails and PDFs
(T336): § 0 and § 11 are new, § 4 is rewritten, and § 2, § 3, § 5 (its DESIGN.md citations), § 8, § 9 and § 10
changed.

- **Per-flow briefs:** `design/flows/NN-<slug>.md`, one per flow, ready to paste (§ 8).
- **The pilot's step-by-step:** `design/pilot/README.md`.
- **Screenshots:** `design/baseline/`, which is gitignored (§ 10).
- **The upload set:** `design/tools/stage_upload.ps1 -Flow NN` stages it into `design/upload/`, which is gitignored
  (§ 3.1).
- **Citations:**
  - Every claim about Wombat names a file, a task or a runbook step.
  - `DESIGN.md` is cited by section, not by line, because every flow that lands now amends it (W-008).
  - Every claim about Claude Design names its source in brackets, keyed in § 2.0. Each was re-fetched and checked on
    2026-09-26.
  - Where a claim is inferred rather than documented, it says **(inference)**.

---

## 0. Status (2026-09-27)

| | |
|---|---|
| Decided | **Restructure, with UX in scope** (W-008). The operator: *"restructure because I want it to look at UX also, include mails and pdf"*. The redesign may change the shell, the navigation, the role dashboards, the page shapes and the task flows themselves, not only the look. § 4 says what that means for Wombat, and what stays invariant. |
| Scope | 80 page templates (flows 01–18), the 15 email templates (flow 19) and the PDFs (flow 20). The PDFs are the portfolio, the STAR certificate and the data-export summary (§ 8). |
| Pilot done | Flow 01, the shell, went first. It was taken from Claude Design through Razor to a green replay (T335): designed in three rounds, built as `b347e11c`, replayed on a fresh database with no regression, and put into the design system (`064cde00`). What it taught is § 11, and this brief is corrected by it. Flow 02 is next (§ 8). |
| `DESIGN.md` | Its structural lock is lifted for the redesign. Everything else in it still binds whatever has not been redesigned, and each flow that lands amends it with its tests (the banner at its top; § 9). |
| Decided | **The design system:** Wombat's own, built from the code (W-009): https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18, its source in `design/system/`. |
| Decided | **The aesthetic direction** was given in the pilot's first message (pilot step C). Flow 01's canvas and the design system now carry it; later flows follow them. |

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
| CollegeAdmin | Dr Anton Kruger (CPSA) | keep specialities, EPAs and curriculum versions, and write the College's activity types | F16, F17 (since T300, D52) |
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

**`DESIGN.md` line numbers in flows 02–18 are stale.** They were taken at commit `58bea95f` (2026-09-26). T300, T302,
T303 and T307 have since added 39 lines, and W-008's banner another 16. Read such a citation with
`git show 58bea95f:execution/architecture/DESIGN.md`, or find the passage by its section. This brief and flow 01 now
cite sections.

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
| How Wombat gets in | The session reads `app.css`, `DESIGN.md`, the runbook and the PNGs where they are. The artifact skill "looks for an existing design system in your project" [artifacts]; that `/design` does the same is an inference. | Link or upload a codebase; "Claude reads the components and styles" [setup]. **The repo is on GitHub since 2026-10-05** (`origin` is `github.com/RCL-Consulting/Wombat`, public), so linking it is possible; untested. **Do not link `github.com/reniercloete/Wombat`:** it holds only the pre-rewrite app (its `master` is `1f14260c`, a 2024 commit in this repo's early history; CLAUDE.md § Reference folders), so it would import the old app's look. Until linking is tried, upload the staged folder in § 3.1. |
| What it produces | Artboards on one canvas, published as a Design artifact, with PNG or PDF export per artboard [cmds]. Then "pick an artboard, and tell Claude which option to implement" [w34]. Since 16 September 2026 Claude Design runs inside Claude Code conversations [onbrand; relnotes]. One research sweep quotes the release notes as adding "on-canvas editing and importing your design system"; the re-check confirmed only the in-conversation part. | Variations [start]. Exports: .zip, PDF, PPTX, Google Slides, standalone HTML. Handoff: "Send to local coding agent" [start]. |
| What it needs | v2.1.234 or later [w34] (v2.1.265 per [cmds; artifacts]); 2.1.282 is installed. A claude.ai login: [cmds] says so for `/design-sync`; one research sweep read it for `/design` too, not re-verified. | A paid plan; it is a research preview [launch]. It is off by default on Enterprise [admin]. |
| Size | Nothing to upload. | "Consider linking very large repositories from Claude Code to avoid lag or browser issues" [start]. |

Hand off to a **local** Claude Code session either way. "Send to Claude Code Web" needs the repo reachable from the
web, and it is not (inference).

### 2.3 Order of work

0. **Decided: restructure, with UX in scope** (W-008, § 4). Two things are still the operator's to settle:
   - **the aesthetic direction,** in a sentence or two, given with the design system. "Do the thinking before you
     prompt" [parrott].
   - **the design system** itself, which is pilot step A.
1. **Set up the design system** (`design/pilot/README.md` step A). Done: Wombat's own, built from the code (W-009).
   - **Path 1: build a Wombat design system from the code.**
     - Give it the staged set (§ 3.1) and 6–10 screenshots as "real examples, not just specs" [setup]. For example:
       `states/home--administrator.png`, `states/activity-inbox--assessor.png`, `states/new-activity--mini-cex.png`,
       `states/activity-view--requested.png`, `states/login--blank.png`, `states/shell--nav-open.png` and
       `states/review-detail--staged.png`.
     - Paste § 5.1 into the setup chat, framed as constraints.
     - Name the components, which is what [start] advises ("mention it by name").
     - **Check what it extracted:** the palette equals `app.css` `:root`; Fraunces is used on the wordmark only; the
       icons are Lucide line icons; no Google Fonts or CDN is used.
     - If it misses, try again with other assets (a research sweep's paraphrase of [setup], not a quotation). "Design
       system import is only as good as its source" [start].
   - **Path 2: use the existing "RCL Consulting" design system.** The pilot README lists what it brings and what it
     lacks for Wombat.
   - **Either way, today's tokens fail WCAG AA** (§ 6 A1). The pilot's token sheet fixes them, and the design system is
     then updated from it (step 6).
2. **Run the pilot: flow 01, the shell, end to end** (T335; `design/pilot/README.md` steps B–G):
   - 2–3 structural variations as wireframes;
   - a pick, then fidelity with its states;
   - the Claude Code build, with `DESIGN.md` and its tests amended;
   - a green replay of the flow's steps on a fresh database, and a re-captured baseline.
3. **Put the lessons into the brief** (pilot step H).
   - Write § 11, "Pilot findings", dated.
   - Correct § 2.4, § 5, § 9 and the flow files by what the pilot found.
   - Do this before flow 02 is briefed.
4. **Then brief the flows in § 8's order, one thread per flow.** The brief template is § 2.4, and each flow file
   already follows it.
   - Stage the flow's upload set with `design/tools/stage_upload.ps1 -Flow NN` (§ 3.1). Attach its KEY SCREENSHOTS
     first, and add STATES as the chat asks for them.
   - **Send § 1 and § 8 as one message.** The canvas draws round 1 from the first message; it does not wait for the
     steps (§ 11, flow 02). Prefer new-shell captures (`act-*/`) to pre-shell `states/` ones in the key screenshots.
   - **Get the structure first, then fidelity, then a review round before the build** (§ 11: flow 01 needed one).
     - Each flow asks first for 2–3 structural variations as wireframes. A structural variation covers the flow's
       pages and their order, the steps of its journeys, and where each outcome and refusal shows.
     - Then comes a pick, then fidelity ("Wireframe first when fidelity doesn't matter" [parrott]).
     - A flow marked F in § 8 is expected to change little in structure, so its wireframe round may be a single
       proposal to confirm (inference; the pilot tests it).
     - Flow 01 is restated this way already. Flows 02–18 are restated as each comes up, by what the pilot teaches.
   - Ask for 2–3 variations [start].
   - Ask for the empty, error and loading states and for different data volumes: "Flag edge cases" [academy].
   - Ask for an accessibility review [start], with § 6 A1 as the check.
   - Answer its clarifying questions in the chat, and write each decision as a sentence. The chat travels in the
     handoff bundle [academy].
   - **Export the chosen artboards into `design/flows/NN-<slug>/` before moving on.** There is no version history
     [start].
   - **Flows 19 (emails) and 20 (PDFs)** need only F01's tokens and type. They can run beside the screen flows once the
     pilot lands; W-008 rejected leaving them until after.
   - **Before round 1, copy the mark into the flow's canvas.** A canvas does not copy a design system's logos (§ 11):
     `Artifact publish` with `asset: true`, `from_url` = the design system and `asset_ids` = the mark's id. Then tell the
     canvas the copy's `/_blob/` URL.
   - **Review the chosen round before building** from the boards' text (`Artifact read` returns each `.dc.html`):
     unwritten or contradicting rules, states that cannot occur, copy, and the tokens. Put the decisions to the operator,
     then ask for one correction round.
5. **Hand off (§ 9),** one Claude Code task per flow. Each flow lands with its `DESIGN.md` amendment, its tests and a
   green replay of its steps.
6. **After every flow lands, re-sync the design system from the code and republish it** (`design/system/`, then the
   Design System artifact: uploads first, the changed `project/` files in one publish, the index last). Flow 01 showed
   that it goes stale at once (§ 11); the next flow starts only after it.

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
ASK: first 2–3 STRUCTURAL variations as wireframes (pages and their order, steps per journey, where outcomes and
  refusals show), each with its reasoning and its step count; after the pick, fidelity with the states; name every
  design-system component used and mark anything else NEW; say which DESIGN.md rule a variation changes.
ATTACHED: <file list>
```

**The shell is a given for flows 02–20.** Flow 01 designed and built it: the sidebar per acting role, the top bar, the
phone menu, the active item, breadcrumbs, Home's frame, the system pages, the reconnect dialog and the error bar
(DESIGN.md § The NavMenu, § Layout grid, § System pages). A later flow designs page bodies inside it. Its CONSTRAINTS
paste the digest as restated on 2026-09-27 (§ 5.1). Its screenshots of Home, the nav and the failure pages predate the
shell, so where a brief quotes "Welcome", "Viewing as" or a Title Case nav, the build is flow 01's. The brief is
restated when its flow comes up.

The ASK line was restated for W-008 on 2026-09-27. Flow 01's ask follows it. Flows 02–18 still carry the old line,
"2–3 variations; <wireframe | full fidelity>", until each is restated (§ 2.3 step 4).

Their Mode rows are from before W-008 too. Where one says "straight to fidelity" because a page shape "holds" or
"stays", citing § 4 (flows 02, 08, 10, 14, 15 and 16), it cites the old recommendation, which W-008 replaced. Read it
as § 8's F mark: a structure expected to change little, still to be confirmed in a structural round. Flow 17's
citations of "§ 2.3 step 3" and of § 4's "three dense pages" point at text since rewritten. Each is corrected when its
flow is restated.

## 3. What to link and upload, and what never to

**The rule:** only tracked files and screenshots you have opened go to Claude Design.
- **Never upload or link the working tree.** That means neither the repo root nor `src/Wombat.Web` as a folder.
- **The same applies to a Claude Code `/design` session.** What it reads goes to the model, so point it at the files
  below (inference).

### 3.1 Stage the upload set with the script

Run it from the repo root. It empties `design/upload/` (gitignored) and stages one thread's files there:

```powershell
pwsh design/tools/stage_upload.ps1 -Flow 01          # a flow: the design-system set, the brief, the flow, its Attach list
pwsh design/tools/stage_upload.ps1 -DesignSystem     # setting up a design system: the set and § 2.3's example screenshots
pwsh design/tools/stage_upload.ps1 -Flow 01 -WithLayout   # also the shell's Razor and CSS (Components/Layout)
```

**What it does:**
- **It copies only tracked files,** as `git ls-files` lists them, from the working tree. It refuses a flow brief that is
  not tracked yet.
- **It copies screenshots by name** from `design/baseline/`: the ones the flow's Attach section lists, leaving out any
  "Do not attach" paragraph's, or § 2.3's examples.
- **It refuses** anything under `recovery/` or `.scenario-app/`, any `*.dump`, `pwd_DO_NOT_COMMIT.txt`,
  `appsettings*.json`, user secrets, `bin/` and `obj/`. It also refuses to run if `design/upload/` is not gitignored.
- **It keeps back what may show a one-time link.** Any file whose name holds invit, issued, resent, resend, reset or
  MsfExpiryReminder goes into `design/upload/crop-first/` with a warning, never beside the rest (§ 3.3).
- **It prints every file it staged,** grouped, with the total size.

`-Flow 01` staged 78 files, 3.38 MB, on 2026-09-27:
- 3 brief files: `DESIGN.md`, this brief and the flow;
- 30 design-system files;
- 44 screenshots;
- 1 file kept back in `crop-first/`: `act-1/1.5-2-kruger-invited.png`.

| What | Path | Why |
|---|---|---|
| Tokens and every component style | `src/Wombat.Web/wwwroot/app.css` (tokens on `:root`, lines 11–45) | The design system's source |
| Display face | `wwwroot/fonts/fraunces-var.woff2` (OFL) | The wordmark |
| Brand | `wwwroot/brand/wombat-mark.svg`, `wombat-tile.svg`, `wwwroot/favicon.svg` | The logo. Bring it; do not ask for a new one (the research sweep found no image-generation model; not re-verified) |
| Icons | `wwwroot/icons/*.svg` (25 Lucide icons, 20 in use) | The icon set |
| Contract | `execution/architecture/DESIGN.md` | Mostly behaviour and wording, not look. Its banner says the structural lock is lifted (W-008). Say which passages are stale (§ 5.5). The documented inputs do not name `.md` [setup], so if it is refused, paste § 5 instead. |
| The brief | `design/BRIEF.md` and `design/flows/NN-*.md` | What to design, and the constraints |
| Shell, with `-WithLayout` | `Components/Layout/MainLayout.razor(.css)`, `NavMenu.razor(.css)`, `AuthLayout.razor`, `ReconnectModal.razor(.css, .js)` | Today's frame, and the framework hooks it must keep (§ 5.3). Optional: a restructure should not copy today's markup. |
| Not staged | `Components/Shared/` (36 tracked files: 26 `.razor`, 10 `.cs`) | How well Razor is read is undocumented. [academy] names CSS modules, Tailwind and styled-components, and implies React only through its mention of hooks. § 5.1 names the components instead. |

**Per flow, also:**
- paste its runbook steps as text (each flow's last section);
- for F03 and F17, add three seed folders from `src/Wombat.Infrastructure/Activities/Seeds/`: `mini_cex_cpsa`,
  `reflective_exercise_cpsa` and `teaching_session`. Each holds `schema.json`, `workflow.json` and `credit.json`. The
  script does not stage them.

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
  - Observed: `act-1/1.5-2-kruger-invited.png`, `act-1/1.7-1-mbatha-invited.png`,
    `act-2/2.16-1-registrars-invited.png`, `states/invitations-list--issued.png`,
    `states/invitations-list--being-sent.png` and `states/invitations-list--resent.png` (flows 11 and 19).
  - Likely the same: the other act-1 and act-2 `*-invited`, `*-issued` and `*-resent` captures.
  - Crop the link out, or leave the image out.
  - `stage_upload.ps1` keeps every invitations capture back in `design/upload/crop-first/`, including the empty and
    loading ones. That is deliberately more than the captures observed to show a link.
- **Emails carry one-time links too** (T336, flow 19). `InvitationEmail`, `MsfInvitationEmail`,
  `MsfExpiryReminderEmail` and `PasswordResetEmail` each hold a registration, respondent or reset link.
  - The script judges by file name alone, so it keeps these back only while each capture under `mail/` is named for
    its template. On 2026-09-27 they are, so all four are kept back.
  - T336 replaced every token in `design/baseline/mail/` with a placeholder of the same length
    (`PLACEHOLDER-TOKEN-xxx…`), so these captures show no live link (observed). Open each one, then move it back from
    `crop-first/`.
  - The other emails' links are ordinary page addresses, but open each one first.
- **Other captures checked:** `act-A/A.4.5-2-reset.png` shows no password (observed). The script keeps it back anyway,
  for its name.

## 4. The decision: RESTRUCTURE, with UX in scope (W-008)

**Decided on 2026-09-27** (`execution/DECISIONS.md` W-008). The operator: *"restructure because I want it to look at
UX also, include mails and pdf"*.
- **The redesign may change how Wombat works for each person, not only how it looks.** It covers the shell, the
  navigation, the role dashboards, the page shapes and the task flows themselves.
- **It adds the emails and the PDFs** (§ 8, flows 19 and 20).
- ***Rejected:* a reskin,** which keeps the structure and changes the look. It could not fix what the replay found wrong
  in the frame (§ 4.3).

This section used to recommend a middle course: restructure the frame, and reskin the pages. The operator went further,
so the page shapes are open too. The evidence is kept below, because it is what the restructure must answer (§ 4.3)
and carry (§ 4.4).

### 4.1 What "UX in scope" means for Wombat

Each flow's design answers every row for its people. These rows are what a structural variation is judged on (pilot
step D).

| Concern | What the design must decide | Wombat today (evidence) |
|---|---|---|
| **Information architecture and navigation, per role** | What each role's navigation holds and how it is grouped. The navigation of a person with several roles. Which item a sub-page lights. How a page now reached only by address gets a link. | One flat list per role, in the order of DESIGN.md § The NavMenu's table. That is 6 links for an Assessor, 10 for a Trainee, 18 for an InstitutionalAdmin and 20 for the Administrator; a multi-role user sees the union. Five items are "Coming soon". Seven pages have no link for a role they admit (`coverage.md` § Reached only by address). Some pages light no item (T331). |
| **What each role lands on, and what it must do from there** | Each role's landing page and its first action. How a two-role person chooses the role they act in. | Every role lands on `/`, Home, which shows one role's dashboard at a time. `Navigation/DashboardPriority.cs` chooses it, and a line on Home alone switches it, through `/dashboard/switch/{role}` (Steps 2.33 and 2.34). Switching changes the dashboard but not the nav (Step 2.34: "The nav is unchanged"). A graduate lands on "No role assigned" (T311). |
| **The number of steps in the core journeys** | For each journey in § 4.2: the pages and actions from landing to done, stated per variation beside today's count. | § 4.2, from `coverage.md` § Journeys by role and the steps' Route lines. |
| **Where an action's outcome and a refusal appear** | Where every action's result shows, and where a refusal shows, in view of the control that caused it. | A result region takes the focus (`Accessibility/ActionFocusTests`; § 6 A5). But the review page's quorum refusal shows only at the head of the page, out of view of the card that caused it (`act-4/4.46-1-zulu-remit-quorum-refused.png`; flow 09). A refused move must keep its typed note (T299). |
| **Empty, loading, error and reconnect states** | Each page's first render, skeleton, load error, empty and no-match states. The reconnect dialog, the in-app error bar, access denied, not found and the error page. | The header is missing until the first read ends, and an alert sits above an empty state (T329; § 6 A6). Access denied can render inside a second layout (T321). The reconnect dialog shows two messages at once (T330). |
| **Narrow viewports** | The 390 px layout of every page, and of the navigation. | The nav folds behind a CSS-only toggle; open, it pushes the page down (`states/shell--nav-open.png`). The item editor is cut off, and the trajectory charts' labels are about 5 px (T323; § 6 A3; Steps A.7.1–A.7.13). |
| **Forms and pickers offer only what the command accepts** | Each picker's options and each form's fields, taken from what the handler accepts from this caller. A read-only view where the caller cannot write. | Some pickers narrow already. In Step 3.1, the EPA picker offers the nine EPAs whose tool list names the Mini-CEX (T122), and the Assessor picker offers only KGK's active assessors (T102). Other forms offered what the command then refused; those are fixed or being fixed (T300, T302, T303, T291, T304, T301; § 6 A11). |

### 4.2 The core journeys, and how many steps they take today

"Route today" is the Route line of the runbook steps named. Each arrow is a page change. A structural variation states
its own count for the same journey, with the same data.

| Role | Journey (`coverage.md` § Journeys by role) | Steps | Route today | What the count hides |
|---|---|---|---|---|
| Trainee | Ask a consultant for a Mini-CEX: save a draft, then submit it. This is the most frequent job: 25 observations per registrar per semester (`act-1-setup.md:410`, Step 1.18). | 3.1, 3.2, 3.3 | `/` → `/activities/new` (save the draft), then `/activities/mine` → `/activities/{ActivityId:int}` (reopen it and submit) | The draft is found again through My Activities. Two of the form's three sections are locked until the state allows them. |
| Assessor | Rate a Mini-CEX a registrar sent me | 3.5 | `/activities/inbox` → `/activities/{ActivityId:int}` | Two pages. A two-role consultant first switches the dashboard to Assessor to see the Assessor cards (Step 3.33), though the inbox is in the nav either way. |
| Coordinator | See which requests have stalled, and chase one | 3.30, 3.31, A.5.10 | `/` (the "Stalled requests" card) → `/placeholder/{Feature}` | A dead end: "no reminder and no reassignment" (Step 3.31). |
| Coordinator | Set up an MSF campaign and invite its respondents | 3.34, 3.35, 3.36 | `/` → `/msf/campaigns` → `/msf/campaigns/new` → `/msf/campaigns/{CampaignId:int}` | The questionnaire is created first, then the campaign (Step 3.34). |
| SpecialityAdmin | Schedule a registrar's review from what is due | 4.9 | `/committee/decisions-due` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Three pages. The Schedule link fills a form on another page. |
| CommitteeMember (chair) | Start a review, stage STARs, record the decision, ratify | 4.16, 4.17, 4.23, 4.27 | `/committee/reviews/{ReviewId:int}` throughout, leaving it for each piece of evidence: review → `/activities/{ActivityId:int}` → review (Step 4.17) | One long page carries the whole sitting (flow 07). |
| InstitutionalAdmin | From registration to admission | 2.28, 2.29 | `/admin/invitations` → `/admin/users` → `/admin/users/{UserId}` → `/admin/trainees` to find who is pending; then `/admin/trainees` → `/admin/trainees/edit` → `/admin/trainees` for each registrar | Four pages to find who is waiting. Then three per admission, done four times in Step 2.29. |
| CollegeAdmin | Publish a new curriculum version | 6.29, 6.30, 6.31 | Clone: `/admin/curricula` → `/admin/curricula/{Id:int}/items` → `/admin/curricula/{Id:int}` → `/admin/curricula`. Edit: `/admin/curricula` → `/admin/curricula/{Id:int}/items`. Publish: `/admin/curricula` → `/admin/curricula/{Id:int}` → `/admin/curricula`. | Each of the three moves starts again from the list. |
| Former trainee | Read my record | 5.20, 5.21 | `/account/login` → `/portfolio/progress`; Home reads "No role assigned" | Home offers her nothing (T311; § 7 B1). |

### 4.3 The evidence behind the decision

**The frame is wrong in structure** (the 2026-09-26 analysis):
- **The nav is long and flat.**
  - The Administrator sees 20 links: 16 role links plus Home, My Account, Data Rights and Logout. The InstitutionalAdmin
    sees 18 (DESIGN.md § The NavMenu, its table). A multi-role user sees the union.
  - There is no grouping.
  - Five links open "Coming soon" stubs (DESIGN.md § The NavMenu, the paragraph on `/placeholder/{Feature}`).
  - Seven pages a role is admitted to have no link in that role's nav (`coverage.md` § Reached only by address).
  - The nav lights the wrong item or none (T331).
  - Sign out appears twice: in the top row and as the nav's Logout (`MainLayout.razor`, `NavMenu.razor`).
- **The dashboards are wrong in what they show, not how they look.**
  - Five cards counted the wrong thing (T297, since fixed).
  - One card has no data source (T298).
  - A graduate lands on "No role assigned" (T311; `Home.razor:66`).
  - System health is two stubs labelled with task ids (T327).
  - Four of the five placeholders are the list behind a dashboard card: Recent Activities, Stalled Activities, Programme
    Trainees and System (§ 7 B7).

**The page shapes' own defects would fit in today's shapes.** They are presentation and state problems:
- contrast (T322);
- widths (T323);
- labels (T324);
- copy (T326);
- the stylesheet (T328);
- loading and error states (T329).

That was the case for keeping the shapes, and it still holds for those defects. But W-008 puts the journeys in scope.
So a shape may now change for a UX reason, such as a journey's step count (§ 4.2), even where its defects do not
require it.

**The cost the pilot measures.** Claude Design cannot build with Razor components (§ 2.1). So a new page shape comes
back as a visual spec, and Claude Code rebuilds it across 80 page templates (inference). The pilot (T335) is the first
measure of what that costs and how much fidelity survives.

### 4.4 What stays invariant

Every variation, in every flow, keeps these. The first eight rows are the old decision table's "Survives either
choice" row.

| Invariant | Pinned by |
|---|---|
| Every nav link opens a page that admits the role, and no two links share a label | `Navigation/NavMenuAuthorizationTests` |
| One `<h1>` per page, from `PageHeader` | No test as such. `Routes.razor`'s `FocusOnNavigate Selector="h1"` depends on it (§ 5.2). |
| Focus moves to an action's result | `Accessibility/ActionFocusTests` |
| Row actions are named per row | `Design/RowActionMarkupTests`, `Accessibility/RowNamesTests` |
| Field help is linked | `Accessibility/FormFieldHelpTextLinkTests` |
| An invalid field is not shown by colour alone | `Design/InvalidFieldStyleTests` |
| Static pages stay static: signed-out pages, `/msf/respond`, `/portfolio/verify` | `Components/App.razor` (`PageRenderMode`); `Hosting/MsfRespondPageHostingTests`, `VerifyExportPageHostingTests` |
| The CSP | `Security/SecurityHeadersMiddleware.cs`; `Hosting/AppAssetUrlTests` |
| The sign-in cookie is written only by a form post | DESIGN.md § Account / auth page; § 5.2 |
| An out-of-scope record is "not found", never "forbidden" | CLAUDE.md § InstitutionalAdmin scope-aware powers; Step A.5.5 |

### 4.5 What the restructure changes in `DESIGN.md` and the tests

- **`DESIGN.md`.** Its structural lock is lifted: the banner at its top, dated 2026-09-27, names the three passages.
  Each flow rewrites the sections it redesigns, in the same task as its Razor (§ 9). For the shell and the tokens (flow
  01), those are:
  - § Design tokens and § Typography;
  - § Layout grid (the shell), § The NavMenu and § Dashboard layout grid;
  - § Page-level patterns › Dashboard page.
- **The tests to edit, as each flow reaches them:**
  - the value pins: `Design/FieldGroupTests` (the border), `InvalidFieldStyleTests`, `TableColumnClassTests` and
    `NarrowLayoutTests`;
  - T322's planned `Design/ContrastTests`, which is new;
  - `Navigation/NavMenuAuthorizationTests`, which parses DESIGN.md's nav table, so a nav change is a DESIGN.md change;
  - `DashboardLinkAuthorizationTests`, and the placeholder check (`PlaceholderPage.Headings`, in
    `NavMenuAuthorizationTests`);
  - `PageShapeSmokeTests` and `DesignSystemSmokeTests`, which pin class names;
  - the page layout tests (`Admin/ActivityTypeBuilderLayoutTests`, `CurriculumItemsEditLayoutTests` and others);
  - `NarrowLayoutTests`' shell pins (`@media (min-width: 641px)` and the gutter).

  `Design/*` is 12 files and 2,509 lines.
- **The runbook's Expect lines.** They quote the screen. By a keyword search, 59 steps' Role, Route, Do or Expect lines
  mention the nav, the role switch, Sign out, a menu or "Coming soon". Fifteen of them are among flow 01's 21 steps;
  its other six (A.5.1, A.5.3 and A.5.5–A.5.8) use none of the keywords. A shell change is checked against all 59 and
  those six, 65 steps (pilot step F).

### 4.6 Two visual decisions, still open

- **The body font.** The stack starts with Segoe UI, which only Windows has (`app.css:50`), so other systems fall back
  to Tahoma, Geneva or Verdana. The choice is to keep it, or to name a self-hosted, GPLv3-compatible woff2.
- **Dark mode and reduced motion.** `app.css` has neither (no `prefers-color-scheme` or `prefers-reduced-motion`
  rule), so both are opt-in.

The "RCL Consulting" design system, if chosen, answers both in part: Jost for text, and a dark theme. Its
reduced-motion rule covers only its own animation (pilot step A).

## 5. Constraints every design must meet to be buildable

### 5.1 The constraints digest (paste verbatim into every brief)

```
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5; restated after the flow 01 pilot, 2026-09-27)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  the error page, /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side
  behaviour beyond a small script module; no live validation; the phone menu is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence (Source Sans 3 and Fraunces, OFL, ship).
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token (the design system's tokens.json) or a NEW token with its value; prefer the spacing scale
  xs 4, sm 8, md 16, lg 24, xl 32, 2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
The shell is designed and built (flow 01): design the page body inside it, not a new frame. The sidebar shows the
  acting role's navigation (one role at a time; access is the union of the roles held), grouped above eight links, with
  My progress and My data rights under a rule; the top bar holds the person's name and Sign out; below 641 px a phone
  bar with a CSS-only menu. A new page names its owning list (its nav item lights, its breadcrumb follows it) and its
  title "<Page> · Wombat", the same words as its h1 and its nav label, in sentence case.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, the trail from the
  owning list, an optional icon, header actions), DataTable (.clinic-table in .table-container, PagerControls),
  FormField / FormActions (.form-container, .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid,
  loading and load error), StatePanel (loading / empty / error), Skeleton, Alert (success / info / warning / danger,
  text on the tint), ActionResult, ConfirmDialog (native <dialog>), badges (five tints), Icon, TrajectoryChart
  (hand-drawn SVG, no chart library), ReferenceBlock.
Already designed by flow 01, reuse them: the active nav item, the acting-role switch and its result alert, access
  denied, not found, the error page, the reconnect dialog and the in-app error bar. Still to design where a flow meets
  them: field validation (invalid border plus a stripe, not colour alone; message under the field; the summary) and
  session ended.
Content: sentence case everywhere; people by name as stored (no titles); states and types by label; times in South
  African time with the zone shown; an out-of-scope record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (the sidebar's white ring included); targets
  ≥ 24 px, 44 px on a phone; focus moves to an action's result; every page works at 390 px with no sideways scroll;
  reduced motion honoured.
Viewports: 1280×800 and 390×844.
```

### 5.2 The technical constraints, with their sources

| Constraint | Source |
|---|---|
| **Two render modes.** A signed-out visitor's pages and `[ExcludeFromInteractiveRouting]` pages are static; everything else is `InteractiveServer`. | `Components/App.razor` (`PageRenderMode`) |
| On a static page, `@onclick`, `@bind`, `OnAfterRenderAsync` and JS interop do nothing. Behaviour there is a link, a form post to an endpoint, or `wwwroot/wombat.js`. | DESIGN.md § Account / auth page ("A visitor who has not signed in gets static pages") |
| **The sign-in cookie is written only by an HTTP POST.** Sign-in, register, link, sign-out and change password stay real `<form method="post">` elements. | DESIGN.md § Account / auth page ("The sign-in cookie is written only in an HTTP request") |
| `/msf/respond` is static for everyone: its link's rate limit must see every request. `/portfolio/verify` is a GET form checked as the page renders. | `App.razor` remarks; T205, T265 |
| **CSP:** `default-src 'self'; script-src 'self' 'nonce-…'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'self'` | `src/Wombat.Web/Security/SecurityHeadersMiddleware.cs:46–58` |
| Claude's design output is allowed Google Fonts and CDN scripts, including the Tailwind CDN and jQuery. All of these must be stripped. | [artifacts], per the research verdict |
| Every script is same-origin or carries the nonce. Every first-party CSS and JS file is linked through `@Assets[…]`. | `Hosting/AppAssetUrlTests` (T175); DESIGN.md § Files that own the design system |
| **Every class a page names is defined** in `app.css` or the component's `.razor.css`. So utility classes (`px-4`, `flex`) cannot be used unless they are written into `app.css`. | `Design/DefinedClassTests` (T266) |
| Icons render as `<svg class="icon"><use href="/icons/{Name}.svg#i"/></svg>`. A new icon is a Lucide SVG whose root has `id="i"`. There is no Bootstrap Icons font. | `Components/Shared/Icon.razor`; DESIGN.md § Icons |
| Dependencies must be GPLv3-compatible; the repo is AGPL-3.0. JS goes in small modules under `wwwroot/js/`. Modals are native `<dialog>` elements opened by `wwwroot/js/dialog.js`, on interactive pages only. | CLAUDE.md § Key technical choices; DESIGN.md § Non-negotiables; `ConfirmDialog.razor` |
| Exactly one `<h1>` per page, from `PageHeader`: `FocusOnNavigate Selector="h1"` targets it. Dashboards add none. | `Routes.razor`; DESIGN.md § Page-level patterns › Dashboard page |
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

Change any of these only together with DESIGN.md and the test named. Under W-008 each is open to the flow that
redesigns it (§ 4.5, § 9).

| Rule | DESIGN.md section | Pinned by |
|---|---|---|
| Structure "ported from ClinicAssist", with fixed class names (`.clinic-table`, `.detail-card`, `.btn-*`…) | The opening; § Non-negotiables; § Historical context. Lifted for the redesign by the banner (W-008). | `DesignSystemSmokeTests`, `PageShapeSmokeTests` |
| Page shapes: List, Detail, Form, Dashboard, Account, Anonymous | § Page-level patterns | `PageShapeSmokeTests` (the static and account shapes are technical) |
| Shell values: 250px sidebar, sticky top row, 641px breakpoint, 16px gutter | § Layout grid (the shell) | `NarrowLayoutTests` |
| Nav order, grouping and labels; the five placeholders | § The NavMenu | `NavMenuAuthorizationTests` (parses the table) |
| Badge tints come from `BadgeFor` only, with a state→tint table | § Badges | `DefinedClassTests.OnlyBadgeFor_NamesABadgeClass`, `BadgeForStatusTableTests` |
| Buttons: `.btn-outline` only; the primary action goes in the `PageHeader` slot; `.btn-danger` only in a dialog footer | § Button system | Not tested. There are 15 uses of `.btn-danger` outside `ConfirmDialog.razor`. |
| Colours only in `:root`; spacing only on `--space-*`; no inline `<style>` in pages | § Design tokens; § Non-negotiables; CLAUDE.md | Not tested. There are 34 `style="` attributes in 14 files. |

### 5.5 Where DESIGN.md is stale (do not copy these)

- **The opening:** it describes a "37-line" `app.css`. The file is 1,414 lines.
- **§ Historical context:** "The palette is still TBD". The palette was set in T089.
- **The token block** gives `--muted-text #6c757d`. The value is rgb(104 111 119) (T086). The block also omits
  `--font-display`.
- **The lockup:** "font-weight 700" (§ Logo & brand assets). The wordmark is Fraunces 500 at 1.7rem
  (`NavMenu.razor.css`).
- **`.account-form-container`:** 400px (§ Account / auth page). The value is 30rem.
- **The List-page template** uses `shadow-sm mb-4`, which is undefined, so copying it fails `DefinedClassTests`.
- **`wwwroot/lib/`** (§ Files that own the design system) does not exist.
- **Defects in the stylesheets themselves:**
  - `var(--text-muted)` at `app.css:631` names an undefined token;
  - nav items render underlined (`act-1/1.5-2-kruger-invited.png`): `NavMenu.razor.css` sets `text-decoration: none`
    on the brand (line 38) but not on `.nav-link`;
  - raw colours sit outside `:root`: the auth gradient (`app.css:57–58`), the dialog backdrop (`app.css:1316`),
    `NavMenu.razor.css`, and `#blazor-error-ui` in `lightyellow` (`MainLayout.razor.css:95`).

## 6. Requirements every design must meet

Each requirement is testable. It is listed with its task and the evidence before the fix; paths are under
`design/baseline/`.

| # | Requirement | Tasks | Evidence |
|---|---|---|---|
| A1 | **Contrast in the tokens.** Text is 4.5:1; control borders and the focus ring are 3:1 (WCAG 1.4.3, 1.4.11). The sidebar needs its own focus-ring token that reaches 3:1 on both ends of its gradient. See the failing pairs below. | T322 (P2) | `act-A/A.7.14-1-login.png`, `act-A/A.7.14-3-success-alert.png`, `act-A/A.7.14-5-danger-button.png`, `act-A/A.7.14-7-review-7-badges.png` (re-taken after T335's tokens landed T322: every pair now passes, Step A.7.14's Actual) |
| A2 | **Colour is never the only signal.** A status is written in words; a dot may sit beside it as a secondary cue, hidden from screen readers. | T327 | `act-A/A.6.2-1-system-health.png` |
| A3 | **At 390 px nothing scrolls sideways.** The item editor fits with Save in view; activity inputs are ≥ ~290 px; chart text is ≥ 11 px or a table replaces the chart. **At 1280 px** long emails and GUIDs wrap, and wide tables show their actions. | T323 | `act-A/A.7.10-5-item-editor-390.png`, `states/activity-view--narrow.png`, `states/review-detail--narrow.png` (its trajectory charts, at 375 px), `states/invitations-list--check-address.png`, `states/user-detail--pending-invitations.png` |
| A4 | **Targets, fonts, focus and order.** Every target is ≥ 24 px; controls use the body font; a scroll region shows the design's focus ring; filters come before Apply and the table in tab order; each password toggle exposes `aria-pressed` and names its input; badges keep their pill shape; header actions share one height. | T328 | `states/login--narrow.png`, `states/data-rights--narrow.png`, `act-A/A.7.5-4-decisions-due-390.png` (re-taken after T335: the region now shows the design's ring), `states/home--narrow-trainee.png` |
| A5 | **Every action reports its outcome** in a result region that takes the focus. A refused move keeps its typed note. A button is not disabled by its own action. A stale refusal mark goes when its value changes. Destructive actions confirm and name the target, and their trigger is an outline button. | T299, T264 | `act-A/A.7.1-4-submitted-requested.png`, `states/activity-view--decline-refused.png`, `act-6/6.8-1-delete-refused.png` |
| A6 | **Every page designs its loading, load-error and not-found states.** The header shows from the first render; a skeleton shows while loading; the alert shows with no empty state under it; no action is offered before the record has loaded. | T329 (P2) | `states/activity-view--loading.png`, `states/epas-list--load-error.png`, `states/panels-list--loading.png`, `states/activity-type-edit--not-found.png` |
| A7 | **Failure screens.** A designed error page carries a request id, signed in or out. Access denied is drawn once. The reconnect dialog shows one message per state. | T321, T330 | `act-A/A.5.8-2-error-signed-out.png`, `states/shell--reconnect-retrying.png`; Access denied drawn twice has no capture now (Back to institutions, the link that reached it, went with T302) |
| A8 | **People by name, states and types by label, fields by their own label.** Ids stay only in the audit log and an audit entry, the data-rights list and a request, and `/portfolio/verify`. | T324 | `states/scheduled-job-runs-list--loaded.png`, `act-1/1.27-2-form-saved-preview.png`, `states/institution-edit--invalid.png`, `states/entrustment-scale-edit--rung-refused.png`; Home greeted by email, not name, until T335; its header now reads "Home" over the role and period, with no greeting (`act-A/A.5.3-1-forged-switch-trainee-view.png`) |
| A9 | **One clock, labelled.** Every time is South African time with its zone ("2026-09-26 15:14 SAST"), and every "today" is the South African date. | T325 | `act-3/3.47-1-molefe-report.png` against `act-3/3.46-1-report-released.png`; `act-3/3.32-1-nudge-run.png` |
| A10 | **Copy says what the page does for this viewer**, and "no match" is not "empty". | T326 | `act-1/1.22-1-scales-read-only.png`, `states/activity-types-list--no-match.png`, `states/curriculum-items-edit--no-items.png`, `states/group-mappings--no-provider.png` |
| A11 | **Offer only what the caller can do.** Pickers offer exactly what the command accepts; records the caller cannot write get read-only views. | T300, T302, T303, T291, T304, T301 | § 10 (pre-fix) |
| A12 | **The nav marks where you are,** never two items at once. | T331 | `states/campaign-report--loading.png`, `states/specialities-list--loading.png` |
| A13 | **Operations pages report the truth.** Counts say what they count; the run filter is a select; lists page with "Showing 1–50 of N". | T327, T277 | `act-A/A.2.10-2-partial-key-nothing.png` |
| A14 | **One page-title pattern, and unique accessible names.** This covers the alert's dismiss button (today "×"), the pager's size select, the builder's selected tab, and dashboard links. | T190, T280 | `act-A/A.7.13-1-review-7.png`, `act-A/A.7.9-6-builder-390.png` |

**A1's failing pairs before T335** (T322, measured at step A.7.14 by the T295 replay; the T335 replay measured
every pair passing):

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
| B4 | **Training year** beyond My progress. | T306; `act-2/2.29-3-four-admitted.png` | A column on Active profiles, a line on the admit form, and the trainee's targets card (F05, F11). |
| B5 | **The trainee's view of her decisions and appeals.** | T308; `states/my-reviews--appealed.png` | A decision history shared with the committee's review page, and an Appeals block (F09). |
| B6 | **STARs no longer in force.** | T319 (P2); `states/my-authorisations--after-revocation.png` | A "No longer in force" section; the data-rights decision note (F09, F14). |
| B7 | **The five pages that were "Coming soon".** T335 deleted the placeholder page and its five nav items, so each address is Page not found now. | coverage.md § Flows and states not played | For each, design the page or leave it out: Recent Activities (F04); Stalled Activities with triage, Programme Trainees, and STAR Review Queue (perhaps simply "Entrustment decisions") (F06); System (F18). Their contents are inferred: no intent document exists. |
| B8 | **Pages reached only by address:** `/admin/entrustment-decisions` (speciality admins), `/admin/institutions/{id}` (her own), `/committee/panels` (Coordinator), `/portfolio/authorisations` (not in the nav), `/account/logout-confirm`, `/Error`. | coverage.md § Reached only by address | A link, or a reason for none (F09, F12, F08, F02, F01). |
| B9 | **The appeal outcome form.** | T307 (group 1, landed in d03732d; D51), T309 | Built and re-captured on 2026-09-26: Outcome opens on "Select an outcome…" and says each outcome in words, with help; "Choose an outcome." if none is chosen; an optional "Replacement conditions" on a remit; Upheld is gone (D51), leaving Dismissed and Remitted (F09). The scheduling preview warns when the seat is held (F08, T309). |
| B10 | **Implied but not built:** self-service password reset, rebuilding one trainee's progress, STAR certificate verification, and the builder's visual workflow and credit editors. | coverage.md; T019-b…g | Show as future, or leave out; not assumed built (F02, F15, F13, F17). |
| B11 | **The College's builder and a read-only builder.** | T300 (group 1, landed in 1e154ab; D52) | Built and re-captured on 2026-09-26: Activity Types in the CollegeAdmin's nav; the notice "Set by the College that owns Paediatrics. …"; View or Edit per row; Scope offering only the caller's scopes. Redesign it from `states/activity-type-edit--college-instrument.png` and act-6 `6.14a-*` (F17). |
| B12 | **An ended trainee profile** as a read-only record. | T305 (P2); `states/trainee-profile-edit--completed.png` | Details list, no Save (F13). |
| B13 | **The Upcoming deadlines card.** | T298 | Remove it, or give it a real source (F05). |

## 8. The flow index

The flows are in briefing order. The order is the shared frame first, then frequency × stakes. Frequency comes from
the programme's calendar: 25 observations per registrar per semester (`act-1-setup.md:410`, Step 1.18); a review
each semester or year; one MSF campaign per period. Stakes come from task severity. "Held" names a group-1 task whose screenshots must be
re-captured before briefing (§ 10).

**Mode.** Under W-008 every flow starts with a structural round (§ 2.3 step 4). W marks a flow whose structure is
expected to change a lot; F marks one expected to change little, whose structural round may be a single proposal to
confirm. The marks were set before W-008, for the reskin question; the pilot tests them (§ 11).

**Flows 19 and 20** have no page template and no nav. They are the outputs the screens send and print, added by W-008
and written for T336. Their baseline goes into `design/baseline/mail/` and `design/baseline/pdf/`.

| # | File | Flow | People | Pages (coverage.md templates) | Steps | Mode | Held |
|---|---|---|---|---|---|---|---|
| 01 | `flows/01-shell.md` | The shared frame: nav, Home frame, role switch, system states | everyone | `/`, `/access-denied`, `/not-found`, `/Error`, `/placeholder/{Feature}` | 21 | W | — (T297 re-captured 2026-09-26) |
| 02 | `flows/02-sign-in-and-account.md` | Getting in, staying in, one's own account | everyone, anonymous | `/account/login`, `logout`, `logout-confirm`, `profile`, `change-password`, `forgot-password`, `link-external` | 19 | F | — |
| 03 | `flows/03-trainee-files-activity.md` | A registrar asks for an assessment or logs one | Trainee | `/activities/new`, `/activities/{ActivityId:int}`, `/activities/mine`, `/activities/inbox` | 30 | W | — (T297 re-captured 2026-09-26) |
| 04 | `flows/04-assessor-inbox.md` | An assessor works their inbox | Assessor | `/`, `/activities/inbox`, `/activities/{ActivityId:int}` | 17 | W | — (T297 re-captured 2026-09-26) |
| 05 | `flows/05-trainee-progress.md` | A registrar reads where they stand | Trainee | `/`, `/portfolio/progress` (+`{EpaId}`, T355), `/activities/mine` | 17 | W | — (T297 re-captured 2026-09-26) |
| 06 | `flows/06-programme-oversight.md` | Who is behind, and what has stalled | CommitteeMember, Speciality/SubSpecialityAdmin, Coordinator | `/`, `/dashboard/switch/{role}`, `/not-found`, `/committee/panels`, `/admin/entrustment-decisions` | 20 | W | — (T355 re-captured 2026-10-04) |
| 07 | `flows/07-review-sitting.md` | The committee sits and ratifies | CommitteeMember | `/committee/reviews/{ReviewId:int}`, `/activities/{ActivityId:int}`, `/committee/reviews`, `/committee/panels` | 26 | W | — |
| 08 | `flows/08-committee-calendar.md` | Panels, what is due, the schedule | InstitutionalAdmin, Speciality/SubSpecialityAdmin, Coordinator, CommitteeMember | `/committee/panels` (+`new`, `{PanelId}`), `/committee/decisions-due`, `/committee/reviews`, `/committee/reviews/{ReviewId:int}`, `/access-denied` | 30 | F/W | — |
| 09 | `flows/09-outcomes-stars-appeals.md` | STARs, the register, a revocation, an appeal | Trainee, CommitteeMember, speciality admins, InstitutionalAdmin | `/portfolio/authorisations`, `/committee/my-reviews`, `/committee/reviews/{ReviewId:int}`, `/admin/entrustment-decisions`, `/committee/decisions-due`, `/access-denied` | 17 | F/W | — (T307 re-captured 2026-09-26) |
| 10 | `flows/10-msf-campaign.md` | An MSF campaign from set-up to release | Coordinator, anonymous respondents, Trainee | `/msf/campaigns` (+`new`, `{CampaignId}`), `/msf/respond`, `/msf/reports/{CampaignId:int}`, `/msf/my-reports` (+`{CampaignId}`), `/msf/coverage` | 18 | F | — |
| 11 | `flows/11-onboarding.md` | From invitation to admission | Administrator, InstitutionalAdmin, invitees, PendingTrainee | `/admin/invitations`, `/account/register`, `/`, `/admin/users` (+`{UserId}`), `/admin/trainees` (+`edit`), `/account/profile`, `/activities/mine`, `/activities/new`, `/access-denied` | 29 | F/W | — (T297 and T303 re-captured 2026-09-26) |
| 12 | `flows/12-people-admin.md` | Users, roles, assessors, locks and resets | InstitutionalAdmin | `/admin/users` (+`{UserId}`), `/admin/assessors` (+`edit`), `/admin/institutions/{Id:int}`, `/admin/sso/group-mappings`, `/` | 15 | F/W | — (T302 and T303 re-captured 2026-09-26) |
| 13 | `flows/13-graduation-and-exit.md` | Portfolio, verification, completion, the graduate's record | Trainee → former trainee, InstitutionalAdmin, Coordinator, verifier | `/portfolio/export` (+`{TraineeUserId}`), `/portfolio/verify`, `/admin/trainees` (+`edit`), `/admin/users/{UserId}`, `/portfolio/progress`, `/`, `/committee/my-reviews`, `/portfolio/authorisations`, `/msf/my-reports`, `/activities/*`, `/committee/reviews` | 26 | W | — (T297 and T303 re-captured 2026-09-26) |
| 14 | `flows/14-data-rights.md` | Data rights, requested and decided | Trainee, Coordinator, Administrator | `/account/data-rights`, `/admin/data-rights` (+`{Id:guid}`), `/not-found`, `/account/login`, `/admin/users`, `/admin/trainees`, `/committee/reviews` (+`{ReviewId}`) | 19 | F/W | — |
| 15 | `flows/15-institution-adopts-catalogue.md` | Adopt, add local items, move to a new version | InstitutionalAdmin, Administrator | `/admin/curricula` (+`{Id}/items`), `/admin/epas` (+`new`, `{Id}`), `/admin/adoptions`, `/admin/entrustment-scales`, `/admin/entrustment-decisions`, `/admin/trainees/edit`, `/admin/curriculum-progress`, `/portfolio/progress` | 16 | F | — |
| 16 | `flows/16-college-catalogue.md` | The College keeps the national catalogue | CollegeAdmin, Administrator | `/admin/colleges/**`, `/admin/specialities/**`, `/admin/entrustment-scales/**`, `/admin/epas/**`, `/admin/curricula/**`, `/access-denied` (21 templates) | 33 | F | — (T300's dashboard re-captured 2026-09-26; Dr Kruger's other 1280 px sidebars are pre-fix) |
| 17 | `flows/17-activity-type-builder.md` | Build and publish an activity type | InstitutionalAdmin, Administrator, CollegeAdmin (T300) | `/admin/activity-types` (+`new`, `{ActivityTypeId}`) | 11 | W | — (T300 re-captured 2026-09-26) |
| 18 | `flows/18-platform-operations.md` | The operator keeps the platform running | Administrator; InstitutionalAdmin (audit) | `/`, `/admin/institutions/**`, `/admin/jobs` (+`runs`), `/admin/audit` (+`{Id:guid}`), `/admin/curriculum-progress`, `/admin/users/{UserId}`, `/placeholder/{Feature}` | 23 | W | — (T302 re-captured 2026-09-26) |
| 19 | `flows/19-emails.md` | Emails: what each mail tells its reader, and where its link lands | each mail's recipient: invitees, registrars, assessors, the Coordinator, MSF respondents, anyone resetting a password | None. The 15 templates in `src/Wombat.Application/Common/Email/Templates/`, on `EmailTemplateBase.cs` | 20 | W | — (baseline captured 2026-09-27, T336) |
| 20 | `flows/20-pdfs.md` | PDFs: the portfolio, the STAR certificate, the data-export summary | Trainee, former trainee, staff who export, verifiers | None. `Infrastructure/Reporting/PortfolioPdfService.cs` and its section components; `EntrustmentCertificatePdfService.cs`; the `portfolio-summary.pdf` in a data-rights export (`Infrastructure/DataRights/AccessReportBuilder.cs:198`) | 11 | W | — (baseline captured 2026-09-27, T336) |

Coverage was checked by script on 2026-09-26 against the runbook, `coverage.md` and `design/baseline/states/`. The
checks cover flows 01–18:
- every one of the 324 runbook steps is in at least one flow (325 on 2026-09-27, with T300's Step 6.14a; still all
  placed);
- all 80 page templates are placed;
- all 578 state captures are placed;
- every role in `coverage.md` § Journeys by role is served (§ 1).

Re-run on 2026-09-27 over all 20 flows: every baseline path the brief and the flows name exists, `mail/` and `pdf/`
included (`check_baseline_paths.py` exits 1 otherwise; the three captures § 10 lists as never taken are reported
apart); 416 quoted steps match the runbook; every flow's ask passes.

Every group-3 task (the presentation debt) is a requirement in § 6 or a screen in § 7. The five pages that were
"Coming soon" until T335 are B7.

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
2. **Markup.** Map every element to a component or a class. A new class is defined in `app.css`, or
   `DefinedClassTests` fails. A new component goes in `Components/Shared/` and into § 5.1's list.
   - Where the design keeps a pattern, keep its class names: `.clinic-table` in `.table-container`; `.form-container`,
     `.form-grid` and `.form-actions`; `StatePanel` on every list; `.dashboard-grid`.
   - Where it replaces a pattern (W-008), the replacement is deliberate: the DESIGN.md section and the tests that pin
     the old names change with it (item 6).
3. **Icons.** Add any missing Lucide SVG to `wwwroot/icons` with `id="i"`, and render it through `<Icon Name=…>`.
4. **Strip what the handoff brings.** Remove CDN fonts and scripts, utility classes, inline `<style>` and hex literals.
   Any script becomes a `wwwroot/js` module linked through `@Assets`.
5. **Static pages.** Build them without interactivity, and keep the framework hooks: `EditForm` and `ValidationMessage`,
   `NavLink`, the reconnect class names, and `#blazor-error-ui`.
6. **Amend `DESIGN.md` and its tests, in the same task (W-008).**
   - Rewrite every DESIGN.md section the flow redesigned, so that the contract describes what was built. For the
     sections each flow is likely to touch, see § 4.5 and § 5.4.
   - Change the tests that pin those sections in the same commit. Never delete a test to make room: replace what it
     pinned with what the new design guarantees.
   - Add the flow to the banner's "Redesigned so far" list at the top of DESIGN.md, with its date and commit.
   - Keep § 4.4's invariants.
7. **Wording.** If the redesign changes on-screen wording, update the runbook's `Expect:` lines in the same task. The
   steps quote the screen, and many steps outside the flow quote the frame (§ 4.5). Search the whole runbook, not just
   the flow's own steps.
8. **Emails and PDFs (flows 19 and 20).** Build them in the email templates and the QuestPDF components, not in
   `app.css`. They share the tokens' values, not the stylesheet. Keep each email's plain-text twin. Keep the portfolio's
   integrity footer (`Reporting/IntegrityFooterComponent.cs`) and what `/portfolio/verify` checks against it.
   - The emails are checked by `tests/Wombat.Application.Tests`, for example
     `Features/MultiSourceFeedback/MsfInvitationEmailTests`.
   - The PDFs are checked by `tests/Wombat.Infrastructure.Tests/Reporting/`.
   - Flows 19 and 20 name the rest.

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
  - When a flow's steps span the acts, as flow 01's do (Act 2 to the appendix), that means the whole runbook in order. A
    copy of a `recovery/scenario-post-act*.dump` is not a fresh database. It is for re-capturing a state that
    `states.md` marks "Scratch (post-actN)", never for the acceptance replay (pilot step G).
  - The whole runbook took about five hours for flow 01: seven agents played one act each, in order, then three
    captured the states (§ 11). Each agent drove Chrome with the Playwright library; the Playwright MCP is not needed.
  - A replay agent writes each step's Actual and Gap lines and nothing else. It classes each gap as a regression, an open
    task still failing, a wrong runbook line, a new defect, or not played.
- **The baseline is re-captured.** The flow's step and state screenshots are taken again into `design/baseline/`
  (`states.md` § How to capture) and compared with the chosen artboards.
  - A re-taken capture keeps its file name (the runbook README's Screenshots rule).
  - After the replay, `design/tools/check_baseline_paths.py` must report `missing 0`. Check that each citation still
    shows what its sentence says: a path can exist and show something else (§ 11).
- **The browser check passes** for each role in the flow, at 1280 and at 390.
- **`DESIGN.md` says what was built,** and its banner lists the flow (item 6).

For flows 19 and 20, "the steps replay" means the steps that send or print the output. Their captures are retaken into
`design/baseline/mail/` and `pdf/` (T336).

## 10. Status of the baseline

- **What it holds.** `design/baseline/` (gitignored, `.gitignore:427`) held 1,250 PNGs from the T295 replay of
  2026-09-26 (before T335):
  - 672 step captures (before T335): act-1 53, act-2 134, act-3 131, act-4 56, act-5 58, act-6 85 and act-A 155,
    named `<step>-<n>-<slug>.png`;
  - 578 state captures (before T335), `states/<page>--<state>.png`.
- **The T335 replay** (step G, 2026-09-27) re-took every step capture from a fresh replay of the whole runbook, at each
  step's own moment, on the restructured shell: 737 now (act-1 62, act-2 129, act-3 141, act-4 85, act-5 60, act-6 98
  and act-A 162). It renumbered or dropped some, so a step's `-<n>` may differ from the one quoted before it; the
  brief and the flows cite the new names. It re-took 65 states too, flow 01's (`shell--*`, `home--*`,
  `access-denied--*`, `error--*`, `not-found--*`); the other states are still the T295 captures. The placeholder
  pages went with T335, so `states/placeholder--*.png` stay on disk as the old stubs only.
- **The emails and PDFs** (T336, 2026-09-27; flows 19 and 20 say where each came from):
  - `mail/`: all 15 templates and 3 variants, each as `<Template>.png` (the HTML body at 600 px), `.html` and `.txt`.
    Six templates come from the replay's SMTP sink (seven captures, with the resent MSF link). The other nine were
    rendered with the story's data: the six that nothing sends, and three whose sender the story never triggers. Every
    one-time token is a same-length placeholder.
  - `pdf/`: 6 PDFs and their 22 pages at 110 dpi, from a copy of `scenario-post-actA`. They are Dr Molefe's portfolio
    (her own export and the staff export, byte-identical), three STAR certificates (Active, Superseded and Revoked),
    and Dr Dlamini's data-export summary.
- **How it matches `states.md`.** `states.md` named 578 captures (before T335):
  - Two named captures were not taken: `login--sso-<code>` (no provider is configured) and `my-progress--december` (a
    December replay only).
  - Two more are named only in the "how to reach" column and were not taken either: `change-password--throttled`
    (`states.md:218`) and `epa-edit--local-reactivated` (`states.md:536`). F02 and F15 describe them in words.
  - Two files on disk are named only in `states.md`'s prose: `review-detail--appeal-upheld` and
    `review-detail--decided-elsewhere`. `review-detail--appeal-upheld` shows an outcome that no longer exists: T307 removed
    Upheld (D51). The file is kept on disk as the record of the old form; do not brief from it.

**Pre-fix (group 1).** These tasks are being fixed now. Until each fix lands and its pages are re-captured, do not brief
from these images: the fix changes what the page shows. Each flow file repeats its own list under PRE-FIX.

| Task | Change | Screenshots to re-capture |
|---|---|---|
| T297 (in progress) | Five dashboard cards read literal state keys: the Assessor, Trainee, Coordinator, SpecialityAdmin and SubSpecialityAdmin homes change | Every capture of those five homes. By role (the planner's list, extended by persona; the extension is inferred): act-2 `2.8-2`, `2.9-3`, `2.10-4`, `2.10-6`, `2.10-7`, `2.31-2`, `2.32-1`, `2.34-1`, `2.36-1`, `2.36-2`, `2.38-1`, `2.38-2`, `2.39-1`, `2.40-1`, `2.40-3`, `2.40-5`, `2.40-7`; act-3 `3.12-1`, `3.16-1`, `3.24-1`, `3.30-1`, `3.33-2`, `3.50-1`, `3.51-1`, `3.53-1`, `3.54-1`; act-4 `4.3-1`; act-5 `5.28-1` (ended trainee, inferred); act-A `A.4.6-1` (ended trainee, inferred), `A.4.7-2`, `A.5.3-1`, `A.5.10-1`, `A.6.8-1`, `A.7.3-1`, `A.7.5-1`, `A.7.7-1`, `A.7.8-1`; states `home--assessor-empty`, `--assessor-pending`, `--assessor-decisions`, `--assessor-switched`, `home--coordinator-empty`, `--coordinator-stalled`, `--coordinator-expiring`, `home--speciality-admin`, `--speciality-admin-figures`, `home--sub-speciality-admin`, `home--trainee`, `--trainee-first`, `--trainee-returned`, `--trainee-ended` (inferred), `home--narrow-assessor`, `--narrow-coordinator`, `--narrow-speciality-admin`, `--narrow-sub-speciality-admin`, `--narrow-trainee`. **Re-captured on 2026-09-26** after T297 landed (7bf8ea7), from a fresh replay of Act 3 and a post-actA copy: act-3 `3.12-1`, `3.16-1`, `3.24-1`, `3.30-1`, `3.33-2`, `3.50-1`, `3.51-1`, `3.53-1`, `3.54-1`; act-A `A.6.8-1` (then numbered A.6.8-2). Not re-captured: Step 3.53's capture of the inbox, because the Pending reviews card no longer links to the inbox it showed, so no step reaches it; the T335 replay removed the file. **Every other capture in this row was re-captured on 2026-09-26 too**, each from the end-of-act snapshot of the act it belongs to, so it shows the end of that act, not the step's exact mid-act moment: act-2 (all 17) from `scenario-post-act2`, so Dr Mokoena's and Dr Sithole's Act 2 Homes already count the 5 admitted registrars; act-4 `4.3-1` from post-act4; act-5 `5.28-1` and `home--trainee-ended` from post-act5; the eight other act-A captures, the five `home--narrow-*` and `home--coordinator-expiring` (with states.md's scratch invitation) from post-actA. The other states come from the act their states.md row names: `--assessor-empty`, `--assessor-switched`, `--coordinator-empty`, `--speciality-admin` and `--trainee-first` from post-act2; `--assessor-decisions`, `--speciality-admin-figures`, `--sub-speciality-admin` and `--trainee` from post-act3; `--assessor-pending`, `--coordinator-stalled` and `--trainee-returned` from post-act3 with two SQL stand-ins for the mid-act moment (Dr Mahlangu's Mini-CEX, activity 21, set back to requested and aged 8 days; Dr Ndlovu's reflection, activity 4, set back to Draft). Nothing in this row is held now. **Re-taken again on 2026-09-27** by the T335 replay (step G), a fresh replay of the whole runbook: every step capture in this row now shows its step's own moment, on the restructured shell, so the end-of-act caveats above hold for none of them (Dr Mokoena's and Dr Sithole's Act 2 Homes count no registrars yet). The Home states were re-taken that day too, for flow 01. At that replay Step 3.30's ageing was refused, so `3.30-1` reads "No stalled requests."; the stalled request is on `A.5.10-1`, after A.5.10's stand-in ageing. |
| T300 | Builder: read-only mode, narrowed Scope, View or Edit per row; Activity Types in the CollegeAdmin's nav | act-1 `1.24-1`, `1.25-1`…`1.25-5`, `1.26-1`, `1.26-2`, `1.31-1`, `1.31-2`; act-A `A.7.9-6`; states `activity-types-list--*` (6), `activity-type-edit--college-instrument`, `--new`, `--loading`, `--not-found`, `--metadata`; Dr Kruger's dashboard (`home--college-admin`, `home--narrow-college-admin`, act-6 `6.10-1`, act-A `A.7.10-1`). In Dr Kruger's other 1280 px step captures the sidebar was pre-fix until the T335 replay re-took them on 2026-09-27; in his other state captures it still is: brief those from the page, not the nav (F16). The 390 px `A.7.10-2` to `A.7.10-5` fold the nav, so T300 does not change them (observed on `A.7.10-5`). **Re-captured on 2026-09-26** after T300 landed (1e154ab, D52), all 26. From a copy of `scenario-post-act1`, as Prof Mbatha: act-1 `1.24-1`, `1.25-1` to `1.25-5`, `1.26-1`, `1.26-2`, `1.31-1` and `1.31-2`, and the eleven states; as Dr Kruger, `home--college-admin`. From a copy of `scenario-post-act6`: act-6 `6.10-1`. From a post-actA copy: act-A `A.7.9-6`, `A.7.10-1` and `home--narrow-college-admin`. What changed (observed): the College rows offer View; Mini-CEX (Paediatrics) opens read-only, headed by its name, under the standing notice "Set by the College that owns Paediatrics. You can read this activity type here, but not change it.", with View on each section and field, the metadata as text, and the workflow and credit as code blocks; a new type's Scope offers Institution with KGK only; the loading and not-found states read "Activity type" with Back to list only; the list's loading state offers no New activity type; Dr Kruger's sidebar ends with Activity Types. Caveats: on the end-of-Act-1 copy KGK Teaching Session Log already exists, so the list captures show 23 rows (Step 1.31's moment), and Step 1.26 was replayed with a second type, "KGK Teaching Session Log (T300 re-check)", whose name is in `1.26-2`, `activity-type-edit--metadata` and the `activity-types-list--draft` row. The T335 replay (2026-09-27) re-took the step captures at their own moments, so the caveat now holds for the states only: `1.26-2` shows the story's own "KGK Teaching Session Log". At 390 px Dr Kruger's nav is folded, so `A.7.10-1` and `home--narrow-college-admin` look as before. Six new captures, not counted above, record the new Step 6.14a as Dr Kruger: act-6 `6.14a-1` to `6.14a-6` (his list with Edit on the College's twelve; `msf_cpsa` open for editing, which T334 will make read-only; Scope with Speciality and Sub-speciality; the saved draft; the list with it). Still visible, still open: no "Draft saved." after a first save (T291 item 3), and the raw seed key in the read-only field view (T271, `1.25-5`). Nothing in this row is held now, except the sidebar in Dr Kruger's older state captures. |
| T302 (in progress) | Institution page: Status as text for an InstitutionalAdmin, with no box and no Deactivate; Deactivate and Reactivate as commands for an Administrator, who keeps the Active box (Save sends a changed box as the state's own command, and unticking asks first) and a Deactivate behind a confirmation. Back and Cancel lead home for anyone but an Administrator (T291 item 7) | act-1 `1.23-1`; act-A `A.6.3-1`, `A.6.3-2` and Step A.6.3's other captures (these states cease to exist); states `institution-edit--own`, `--deactivate-refused`, `--administrator`, `--deactivated`, `--saved`, `--narrow`. **Re-captured on 2026-09-26** after T302 landed (41be531): `1.23-1`, `institution-edit--own` and `--administrator` from a post-act1 copy; `A.6.3-1`, `--saved`, `--narrow` (Step A.6.1 replayed first) and `--deactivated` (the Demo Institution) from a post-act6 copy. The state that ceased keeps its file name and now holds the new page: `institution-edit--deactivate-refused` is her full page after the save. **Re-taken by the T335 replay (2026-09-27):** Step A.6.3 is now two captures, `A.6.3-1-contact-saved` (the form after her save: Status "Active" as text, "Set by a global administrator.", Cancel and Save only) and `A.6.3-2-cancel-home` (her Home, where "Back to home" and Cancel lead); its other captures are gone. Also re-captured, because the fix changed them: act-1 `1.6-2` and act-A `A.6.1-2` (held by flow 18), and `1.23-4` (the create form she can still open, T291 item 8, now with "Back to home"). Back to institutions → Access denied no longer exists (T302): her links lead home, so no step reaches it, and the T335 replay removed both captures of it (act-1 1.23-2 and act-A A.6.3-4). T321's nested layout has no capture now; flow 01 designs Access denied. |
| T303 | User page: Trainee "System-managed", never under Add role | act-2 `2.28-3`; act-5 `5.17-3`; act-A `A.7.9-3`; states `user-detail--pending-trainee`, `--no-roles`, `--reset-refused`, `--reset`, `--narrow`; probably `--other` and `--role-added` (inferred). **Re-captured on 2026-09-26** after T303 landed (4824d62). Every user page an administrator manages changed: Add role never offers Trainee and carries a help line under its select ("Trainee is not offered: …"), and a held Trainee reads "System-managed" with no Remove (observed). `2.28-3`, `--pending-trainee`, `--other` and `--role-added` (with `2.13-1` and `2.13-2`) come from a fresh replay of Act 2 Steps 2.1 to 2.28 from `scenario-post-act1`, so at the steps' own moments; `5.17-3` and `--no-roles` from `scenario-post-act5`; `A.7.9-3`, `--narrow`, `--reset-refused` and `--reset` (with `A.4.5-1` and `A.4.5-2`) from a post-actA copy. Also re-captured there, because the help line changed them though the row did not name them: act-A `A.6.4-1`, `A.6.4-2` and `A.6.7-1`, and states `user-detail--locked`, `--reactivated` and `--pending-invitations` (Dr Patel locked and reactivated again, and states.md's scratch invitation to Dr Botha). Not re-taken: `--own`, `--administrator-own` and `--unavailable`, which have no Add role, and `--loading`, a skeleton. Nothing in this row is held now. The captures still show each role's name running into "System-managed" or its Remove with no gap ("PendingTraineeSystem-managed"; F-2.28b, which widens T323's Remove item). |
| T307 | Appeal card: Outcome opens empty; replacement conditions; Upheld removed (D51) | act-4 `4.45-2`, `4.46-1`, `4.47-1`; states `review-detail--appeal-form`, `--appeal-member`, `--remit-refused`, `--remitted`, `--appeal-dismissed`, `--appeal-upheld` (ceased to exist). **Re-captured on 2026-09-26** after T307 landed (d03732d, D51), from a fresh replay of Act 4 Steps 4.1 to 4.48 on a copy of `scenario-post-act3`, so at the steps' own moments: `4.45-2`, `4.46-1`, `4.47-1`, `--appeal-form`, `--appeal-member`, `--remit-refused` and `--remitted`; also act-4 `4.44-1`, which flow 09 held with `--appeal-member`. `--appeal-dismissed` comes from a copy of `scenario-post-act4`, as `states.md` says (Dr Dlamini appeals; Dr Zulu resolves it Dismissed). What changed (observed): the Outcome opens on "Select an outcome…", with "Dismissed: the decision stands" and "Remitted: the appeal body replaces the decision" and help under it; Remitted adds an optional "Replacement conditions" box; the replacement's card reads "Conditions: …"; the Appeals list says the outcome in words ("(Remitted)", "(Dismissed)"). `review-detail--appeal-upheld` is not re-taken: there is no Upheld. It stays on disk as the old form's record; do not brief from it. One new capture, not counted above: act-4 `4.45-1-vanrensburg-choose-an-outcome` (the T335 replay numbers it first), the refusal "Choose an outcome." under the empty select. Still visible: at 1280 px the chosen outcome is clipped in the half-width select once Remitted opens the second column (`4.45-2`, F-4.45c), and the quorum refusal shows only at the head of the page, out of view of the card (`4.46-1` shows the form after it, F-4.46a). Nothing in this row is held now. |

**Other backend fixes that will change what a page shows.** These do not hold a brief, but re-capture after they land:
- T304: curriculum pickers, and a save that reports replayed credit;
- T305: an ended profile becomes read-only;
- T312: an ended record freezes its EPA list;
- T313: the verify messages;
- T319: "No longer in force", and decision notes;
- T320: data-rights help text;
- T329: loading and error states.

## 11. Pilot findings

Written 2026-09-27 at the end of the flow 01 pilot (T335). Flow 01 is designed (canvas
https://claude.ai/artifact/R86QvLEyyfKx98fT4MENcD, round 3), built (`b347e11c`), replayed (step G) and in the design
system (`064cde00`). Each finding says what was observed and what it changed. "Observed" means seen in this pilot;
"inference" means reasoned, not tested.

**Running Claude Design**
- **Start from the main app's Design page or from Claude Code, never the standalone claude.ai/design.** Observed: the
  design system made as an Artifact appears in the main app's Design page and in `/design`, not in the standalone
  homepage, which keeps a separate, older store. *Changed:* `design/pilot/README.md` step C (`355317ce`).
- **A canvas does not copy a design system's logos.** Observed: a Design canvas installs the system's `tokens.json` and
  files, but not the logos, icons and pictures it keeps by id. Flow 01's canvas drew a CSS disc for the mark through
  three rounds, and Claude Design later flagged it as a placeholder. Copying the mark into the canvas's asset store
  fixed it: `Artifact publish` with `asset: true`, `from_url` = the design system, and `asset_ids` = the mark's id.
  After that, one message had the boards use the copy's `/_blob/` URL.
  *Changed:* do this before round 1 of every flow. Pilot README step C.
- **The design system goes stale the moment a flow lands.** Observed: after flow 01 it still showed the old nav (Title
  Case, the union of roles), the old tokens and none of the new components. It was re-synced from the code, checked by
  two reviewers (23 mismatches, all fixed), and republished as versions 3 and 4 with 29 new icons.
  *Changed:* § 2.3 step 6 is no longer a precaution but a step of every flow's landing. Flow 02 starts only after it.
- **A canvas board is readable text.** Observed: `Artifact read` on the canvas returns each `.dc.html` board. So a
  round can be checked item by item against the message that asked for it, on its text and its markup. Round 3's 30
  fixes were checked this way, before anything was built.
- **The boards use Google Fonts and inline hex.** Observed on every board. They are a picture, not code; the build
  rebuilds them under the CSP. Unchanged from § 9's rule; confirmed.

**The rounds**
- **Structure first worked.** Observed: round 1's three structural variations (a grouped sidebar with one acting role;
  top-bar sections; Home as the map) made the pick a one-line decision: variation A, with C's breadcrumbs.
- **Plan a review round between fidelity and the build.** Observed: round 2 at fidelity was accepted with changes after
  a four-lens review (tokens, shell, states, build). That review found:
  - 22 should-fixes;
  - rules left unwritten or contradicting each other (the switch rule; the active item);
  - states that cannot occur (a signed-out Not found reached any way but typing);
  - eleven decisions for the operator, D1–D11, seven of them needed before the build. Two became project rules (W-010,
    W-011).

  Round 3 applied 30 fixes. *Changed:* § 2.3 step 4 now reads structure, pick, fidelity, review, correction round,
  build.
- **Check a recommendation against its own claims.** Observed: D1, as recommended, was "remembered across sign-ins" in
  a cookie "deleted at sign-out", which cannot both hold. The canvas's own rule text ("stored with the account") was
  right, and the build followed it. *Changed:* nothing in the brief; a caution for the reviewer.
- **The canvas can correct the reviewer.** Observed: the review's swatch for the active item was the blend at the old
  alpha. Round 3's token sheet gave the right one, and a hand check confirmed it. Keep the numbers checkable on both
  sides; the build's `ContrastTests` computes every pair.
- **Boards can disagree with each other.** Observed:
  - The owner table gives a list page the trail "Home › the list", while the list board draws none.
  - The Sign out button is 28 px on one board and 32 px on another.

  The integrator chose, and recorded the reading in DESIGN.md. *Changed:* nothing; expect it and record it.

**The build (step F)**
- **Parallel lanes, then dependent lanes.** Observed:
  - Four independent lanes ran at once in worktrees: tokens and type, the acting role, the reconnect dialog, and page
    titles.
  - Two lanes that needed them followed: the shell, then Home with the failure pages.
  - Then one integration branch, a four-lens review, two fix lanes, a last pass, and one squash (`b347e11c`: 238
    files, +17,748 / −3,068; 7,725 → 8,226 tests, all green).

  Merge friction came from shared files: DESIGN.md sections, runbook Expects, `app.css` `:root`, and two test helpers
  named `StyleSheet.cs` and `Stylesheet.cs`, which are one file on Windows. *Changed:* a lane brief names the shared
  helpers and the files each lane owns. Every merged state runs all six suites. Two tests that passed in their own lanes
  failed only after the merge, each asserting the other lane's old copy.
- **The review found what the lanes could not.** Observed: after all six suites were green, the review found three
  medium defects:
  - the error page's rerun ran the sign-in check again, so a database outage showed a bare 500;
  - the reconnect dialog lost focus when its state changed under it;
  - runbook steps sent a Committee member to an inbox her menu no longer offers.

  It also found about 40 low ones. *Changed:* keep the review between the green merge and the squash, with one agreed
  stopping line: one fix pass, then file the rest.
- **No browser tool is needed.** Observed: the Playwright MCP was not connected all session. The lanes checked their
  work in headless Chrome over the DevTools protocol, and the replay drove Chrome with the Playwright library
  (`channel: 'chrome'`), sign-in passwords taken from environment variables. *Changed:* § 9's replay no longer waits
  on the MCP.

**The replay (step G)**
- **The whole runbook replays in about five hours.** Observed: seven agents played the acts in order on a fresh database
  (`wombat_scenario_t335`), and three captured flow 01's states from the new snapshots.
  - 243 of 325 steps had no gap.
  - None of the other 82 steps failed because of flow 01:
    - 78 fail only on open tasks their Gap lines already cited (92 gaps in all);
    - one had a wrong Expect, now corrected;
    - three were not played (3.30, 3.32, and 3.33's Overdue badge). The session's permission rules refused the ageing
      `UPDATE` their Note prescribes. The switch in 3.33 was played, and the Overdue badge was seen at A.7.2.

  *Changed:* none yet. Ageing needs a command the replay tool owns, or an operator-run statement (T337).
- **A replay must keep the capture names.** Observed: re-taken captures were renamed and renumbered, which broke 101
  citations in the flow briefs and this brief. A script that repointed them by step and number made it worse: where a
  step's captures were renumbered, a citation landed on a different picture, and the path checker cannot see that.
  Every repointed citation was then checked against its image, and the dead ones were reworded, mostly the deleted
  placeholder pages. *Changed:* the runbook README's Screenshots rule now says to keep an existing capture's file name
  when the state is the same. `check_baseline_paths.py` runs after every replay.
- **The error page after a real failure is not reachable by the replay.** Observed: it is served outside Development
  only, and `tools/scenario-replay.ps1` starts the app in Development. It is covered by `ErrorPageFlowTests` and by a
  headless-Chrome probe on a test host. *Changed:* states.md marks it so. A non-Development `start` option is filed
  (T337).
- **State recipes are hypotheses until replayed.** Observed: four were corrected.
  - `Blazor.pauseCircuit()` works only on a page that has not reconnected.
  - The "attempt started" listener must bind the IPv6 loopback too.
  - One width was worded loosely.
  - One timeout is 60 s, not 65 s.

**Flow 02 (T339, 2026-09-28): what the second flow added**

Flow 02 ran the whole loop in one day: brief, three rounds, a review, the build (`f50dffb2`), the replay (246 of 325
steps with no gap) and the re-sync. Its record is `design/flows/02-sign-in-and-account/`. Only what flow 01 did not
already teach is here.

- **The canvas does not wait for the second message.** Observed: it drew round 1 (29 boards) from § 1 alone; § 8 went
  in afterwards as a check, and added one board and three corrections. *Changed:* § 2.3 step 4 sends § 1 and § 8
  together.
- **A sent message can draw nothing.** Observed: the first send of round 2 saved no boards; the canvas's version was
  unchanged. *Changed:* nothing in the brief; read `project/canvas.json`'s version after each round before reviewing.
- **An F flow's single proposal worked.** Observed: round 1 was one structure with options only at the brief's forks
  (three), and the operator decided all four questions in one exchange. Inference: an F flow can skip the 2–3
  variations.
- **The canvas does not know the code; the review must.** Observed: round 2 drew a link date Wombat does not store,
  five password rules where Identity enforces six, and cast members with roles the runbook does not give them. The
  four-sided review caught each by reading the code and the runbook. *Changed:* nothing; keep one reviewer on the code
  and one on the cast.
- **Lanes that share a contract run in waves.** Observed: the words and codes (lane A) and My account (lane C) ran in
  parallel on agreed literals; the pages that render both (lane B) ran after their merge. Every merge was clean.
  *Changed:* flow 02's `build-lanes.md` is the template: name the contract strings in both briefs.
- **A test can pass by modelling what cannot happen.** Observed: the build's review found another site could still
  sign a person out. The sign-out test posted "another site's form" with the victim's cookie, which a browser never
  sends cross-site (the cookie is SameSite=Lax). Likewise a stylesheet test read only exact selectors, so a rule that
  lost the cascade passed (`Stylesheet.Cascaded` now resolves it). *Changed:* the reviewers' brief asks whether each
  test models a state that can occur.
- **The main checkout cannot build while the dev app runs.** Observed: the app on :5080 locks `bin/x64/Release`. Run a
  suite from a throwaway worktree (`git worktree add … HEAD`) after committing locally.
- **Keeping capture names held.** Observed: the replay kept every existing name; `check_baseline_paths.py` read missing
  0 with no repointing, where flow 01's replay broke 101 citations.

**Flow 03 (T342, 2026-09-28 to 09-30): what the third flow added**

Flow 03 ran the loop over three days: a W flow's three structures, three rounds, a four-sided review, the build
(`725237ee`), the replay (254 of 325 steps with no gap) and the re-sync (design system version 10). Its record is
`design/flows/03-trainee-files-activity/`. Only what flows 01 and 02 did not already teach is here.

- **The integrator's own ask can be wrong.** Observed: round 2's ask carried a correction (2) that the code refutes:
  the portfolio review has a named reviewer. The four-sided review caught it. *Changed:* nothing; the reviewer on the
  code checks the ask as well as the boards.
- **A replay can die mid-act, and the per-act dumps are the recovery.** Observed: a machine shutdown killed act 3's
  agent after about 45 of its 57 steps; it had written no Actual line, and its database was half-played. The post-act-2
  dump was restored into a new database (the tool never drops one) and act 3 was replayed from its start; acts 1–2
  stood. *Changed:* the replay brief has each agent write its Actual lines per phase, and the integrator commits each
  act as it lands.
- **A password changed mid-replay strands the earlier snapshots.** Observed: the appendix changed two cast members'
  passwords, and act 2's for Dr du Plessis was kept nowhere, so the states agents could not sign him in on the
  post-act-2 snapshot; they copied his hash across by SQL. *Changed:* the replay brief keeps every password a cast
  member ever had (`WB_PW_<SURNAME>_<ACT>`), never overwriting one.
- **The states capture finds what the replay does not.** Observed: the replay's steps passed, but the states rows,
  which look at every transient state, found a refusal shown twice under one field and a missing space in the replay's
  own fix. *Changed:* republish before the states capture so the replay's fixes are in it, and re-take any state a
  later fix changes.
- **Check the bundle against app.css on every re-sync.** Observed: flow 02's re-sync had lost the `:root` extras'
  closing brace, nesting the whole bundle in `:root`; flow 03's reviewer found it by comparing the bundle's app.css part
  byte for byte. *Changed:* nothing; keep that check in the re-sync's review.
- **One implementer and one Sonnet review were enough for the re-sync.** Observed: 40 files, two must-fixes (both the
  upload's wording) and four nits, all fixed before the publish. CLAUDE.md § Multi-agent workflows now sizes this.

**Flow 04 (T350, 2026-09-30 to 10-03): what the fourth flow added**

Flow 04 ran the loop in two sessions: three structures, three rounds (A picked), a four-sided review, the build
(`06aa51d7`), the replay (325 steps, 249 with no gap, no regression), the states (34 rows, all hold) and the re-sync
(design system version 13). Its record is `design/flows/04-assessor-inbox/`. Only what flows 01–03 did not already
teach is here.

- **A dead replay can resume in place.** Observed: the session ended during act 2's real hour's wait (Step 2.26). The
  plan was to restore post-act1 and replay the act. But the agent had written its lines per phase through 2.25, and the
  database stood exactly there (checked by SQL against the lines), so act 2 resumed at 2.26 three days later and the
  wait was already over. *Changed:* before restoring, check whether the database matches the last written step; restore
  only if it does not, or if a step's open browser session is needed (2.31's tab was re-opened and said so).
- **Copied scripts carry stale assumptions.** Observed: an act agent reused an earlier replay's scripts. Their date
  helper filed one extra request (#24, then cancelled), so every later id is one higher than the last replay's. It was
  harmless because the runbook keys by email, not id. The repo also moved (`C:\dev\Wombat`), which broke every absolute
  path in the brief. *Changed:* the replay brief says to compute every date from D, and it names the new path.
- **An agent may skip a claim to protect something that needs no protecting.** Observed: act 6's agent skipped two
  restart claims (6.2, 6.14) "to keep the log". But `scenario-replay.ps1 start` keeps the old log beside the new one.
  The integrator restarted the app and played both claims. *Changed:* the replay brief says to play every claim,
  restarts included.
- **The states capture can reuse the replay's captures, after opening each.** Observed: 17 of the 34 states were the
  replay's own images, each opened and checked clause by clause. The other 17 were taken fresh. Some captures could not
  be reused: a viewport-only shot of a page longer than 800px, and five taken while scrolled, which draw the fixed
  sidebar part-way down the image. *Changed:* the replay brief says to take full-page shots from the top.
- **Ageing a row can make an impossible record.** Observed: Step 3.30 ages a request eight days back, but its encounter
  is three days back, so About reads "Filed" five days before "Encounter". Three states show it. *Changed:* T353.
- **Compare the live design system with the repo before replacing it.** Observed: 3 of the 30 published files differed
  from HEAD. All three were the build commit's own edits, never published, not someone's edit on the page. *Changed:*
  the re-sync reads each published file and compares it with HEAD before the publish.
- **The re-sync's reviewer checks DESIGN.md too.** Observed: the re-sync copied DESIGN.md's "The committee Home places
  it" (the other-role line). The code places it in `Home.razor` for every role. The Sonnet reviewer, reading the code,
  caught it, and DESIGN.md was corrected with the design system. *Changed:* nothing; keep the reviewer on the code.

**Flow 05 (T355, 2026-10-03 to 10-04): what the fifth flow added**

Flow 05 ran the whole loop in one session: a W flow's three structures (V1 picked), three rounds, a four-sided review,
the build (`b020c942`), the replay (256 of 325 steps with no gap), the states (36 of 37) and the re-sync (design system
version 15). Its record is `design/flows/05-trainee-progress/`. Only what flows 01–04 did not already teach is here.

- **A defect the design depends on lands first, as its own task.** Observed: round 1's Q9 (draw only the rebuilt state
  after a curriculum move) was true only once T304 replayed credit on a move. The operator chose T304 first, with
  T305 folded in, since both changed one handler. Each was reviewed against its own risk, and a partial replay checked
  them before the UI build began. *Changed:* nothing; a question whose answer rests on an open task names it, and the
  task lands before step 6.
- **The replay finds layout regressions the build review cannot.** Observed: four reviewers read the code and passed
  the committee page's chart. The replay at 1280 then found it in the details grid's narrow column (4.15), and a second
  look found it still a card inside a card, the figure 874 px against a 900 drawing. At 390, A.7.3 found a 2 px
  sideways scroll. Each was fixed by the integrator, republished onto the same database, and re-checked by the next
  act's agent before it played on. *Changed:* nothing; this is the replay's job, and the next act re-checks a fix
  first.
- **A first fix can be partial: measure the container, not the class.** Observed: `full-width` made the section span
  the grid, but the chart still drew its narrow form, because its parent card ate 50 px. *Changed:* nothing; a layout
  re-check measures the element the rule depends on (here, the container query's container).
- **Test the press, not the href.** Observed: the build review's one high finding was the integrator's own wiring. A
  bare `#trajectory-1` link resolves against `<base href="/">` and sends the reader to Home, and the test checked only
  the href string. *Changed:* an in-page link focuses its target through `PageFocus` (as `RefusalSummary` does), and
  its test presses it and asserts the page did not move.
- **A real hour's wait survives in short background waits.** Observed: flow 04's act 2 agent died in one long wait.
  This replay's act 2 and act 3 agents each waited a real hour in short background waits, playing read-only steps
  meanwhile and saying so, and both finished. *Changed:* the act prompts say how to wait.
- **A page taller than about 16,384 px cannot be one screenshot.** Observed: Chromium repeated the header part-way down
  an 18,013 px committee page. *Changed:* capture such a page in parts and stitch them (the states agent did).

**What the brief keeps as it was**
- The flows' order (§ 8), the invariants (§ 4.4) and the acceptance check (§ 9) held. The digest (§ 5.1) is restated
  for what flow 01 fixed: the shell, its components and the states it already designed. The brief template (§ 2.4)
  gains the shell as a given.
