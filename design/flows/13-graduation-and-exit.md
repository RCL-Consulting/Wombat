# F13 The programme ends: the portfolio, its verification, completion, withdrawal and the graduate's record

Dr Lerato Molefe graduates and Dr Pieter du Plessis leaves; staff export and close their programmes, and a stranger
checks the PDF. The record that leaves Wombat has to be right, reachable and verifiable.

Part of the GUI redesign briefed in `design/BRIEF.md` (T332). Written 2026-09-26.

| | |
|---|---|
| **People** | Trainee Dr Lerato Molefe, who becomes a former trainee at Step 5.17 (no role, a `trainee_record` claim: BRIEF § 1); Trainee Dr Pieter du Plessis, who leaves and keeps the Trainee role; InstitutionalAdmin Prof Nolwazi Mbatha; Coordinator Mr Pieter Smit; Dr David Naidoo as Assessor (5.25); an anonymous verifier |
| **Frequency and stakes** | Once per registrar after four years, plus leavers. Very high: the PDF and the STAR certificates are the record that leaves Wombat (T310, citing DOMAIN.md:78) |
| **Mode** | **Wireframe first** for the former trainee's home and nav (a new dashboard, BRIEF § 7 B1) and for the staff export's entry points (B2). **Fidelity** for Export, Verify and the completion dialogs |
| **Pages** (`coverage.md` templates) | `/portfolio/export`, `/portfolio/export/{TraineeUserId}`, `/portfolio/verify`, `/admin/trainees`, `/admin/trainees/edit`, `/admin/users/{UserId}`, `/portfolio/progress`, `/`, `/committee/my-reviews`, `/portfolio/authorisations`, `/msf/my-reports`, `/activities/new`, `/activities/mine`, `/activities/{ActivityId:int}`, `/committee/reviews` |
| **Held screenshots** | Two, under T303 (§ Attach). Brief from the step text for those screens until they are re-captured. T297's three were re-captured on 2026-09-26 |

**How to run this thread** (BRIEF § 2.3, step 3; source keys are BRIEF § 2.0's):
1. Open a new thread in the Wombat design system. Attach the KEY SCREENSHOTS (§ Attach, first list).
2. Paste **The ask** below. Then paste **The runbook steps, verbatim** (the last section) as the next message: "Walk
   Claude through the journey and it will generate each screen in context" [academy].
3. Add the other screenshots in § Attach as the chat asks for them.
4. Round 1 is wireframes ("Wireframe first when fidelity doesn't matter" [parrott]). Pick one, then ask for round 2.
5. Write every decision into the chat as a sentence; the chat travels in the handoff bundle [academy]. Export the chosen
   artboards to `design/flows/13-graduation-and-exit/` before moving on: there is no version history [start].

## The ask

Paste this block as the thread's first message.

```text
FLOW 13 — The programme ends: I export my portfolio and anyone can check it; my programme is closed; I keep my record.

GOAL: Let a registrar, and the staff who oversee her, export her portfolio as a PDF. Let anyone who receives the PDF
  check, without an account, that Wombat produced it. Let the InstitutionalAdmin record how a programme ended: Mark
  complete (a graduation, which removes the Trainee role) or Deactivate (a withdrawal, which keeps it). Let the graduate
  or the leaver keep a read-only record, in which an encounter observed after the last day counts towards nothing.
  What is wrong today:
  - the graduate lands on "No role assigned … An administrator can give you one.", loses My authorisations (the only
    page that downloads her STAR certificates) and has no Export portfolio link, although the page admits her;
  - the staff export is reached only by typing a user id into the address, and never says whose portfolio it exports;
  - a completed or withdrawn profile still offers Save profile, so the archived record can be re-dated;
  - the verify page tells a genuine PDF it is unknown when its hash is typed in upper case, and picks one of several
    identical exports by chance.

AUDIENCE: Dr Lerato Molefe (Trainee, year 4; after completion a former trainee with no role but a trainee record).
  Dr Pieter du Plessis (Trainee, year 2; withdraws, keeps the Trainee role). Prof Nolwazi Mbatha (InstitutionalAdmin).
  Mr Pieter Smit (Coordinator). Dr David Naidoo (Assessor). A credentialing officer at her next hospital (anonymous, no
  account). Once per registrar after four years, plus leavers; the stakes are very high, because the PDF and the STAR
  certificates are the record that leaves Wombat. Desktop 1280×800 and phone 390×844.

LAYOUT: Export, the trainee profile and My progress sit in the signed-in shell (sidebar nav, top row). Verify is a
  signed-out, static page in the same shell with only Home and Sign in. The former trainee's home is a Dashboard page:
  cards in a grid under the page's one h1.

CONTENT (use these real scenario values):
  - Kgosi Kgari Teaching Hospital (KGK); Paediatric EPA Curriculum 11.1; Paediatrics / Paediatrics.
  - Dr Molefe: programme started 15 January 2023, completed 26 September 2026, training year 4; 15 STARs, one per EPA
    PAED-001 to PAED-015, each at its exit level (nine at 5, six at 4), issued 26 September 2026, no expiry.
  - Export: From 2023-01-15 To 2026-09-26; file portfolio-68a170344cf3.pdf; exported three times (by her, by Prof
    Mbatha, by Mr Smit), byte-identical; her export as a graduate is portfolio-3db2ef8e7d2b.pdf.
  - Verify: a 64-digit lower-case SHA-256 (68a17034…ec9c); trainee and exporter shown as user ids
    (59ba7d01-561f-4831-9f35-b85305e31354), never names; "Filter: 2023-01-15 to 2026-09-26".
  - Dr du Plessis: started 15 January 2025, last day 2026-09-24, Withdrawn; a Mini-CEX on PAED-002 observed 2026-09-25,
    Completed, credited None.

SCREENS, in order:
  1. /portfolio/export and /portfolio/export/{TraineeUserId} — Export portfolio. States: her own before export
     ("Export your portfolio", the last twelve months filled in); exported; staff, with the trainee named in the header
     ("Export Lerato Molefe's portfolio"); refused (one message and no form, the same for an unknown id and for a
     trainee the caller does not oversee); 390 px. Also the NEW entry points: an "Export portfolio" action on the
     committee review page (/committee/reviews/{ReviewId:int}) for the trainee under review, and on the trainee profile
     page (/admin/trainees/edit), each named for the person.
  2. /portfolio/verify — Verify a portfolio export (static, anonymous, a GET form checked as the page renders). States:
     blank; verified ("This PDF was first generated by Wombat on …, exported N times, last exported on …", trainee and
     exporter as ids, the filter, the file name); no match (a tampered copy); empty (spaces only: "Enter the content
     hash to check."); not a hash ("A SHA-256 hash is 64 hexadecimal characters; the file name carries only the first
     12."); check failed ("The export could not be checked just now."); 390 px with the ids wrapping.
  3. /admin/trainees/edit and /admin/trainees — End a programme. States: the Mark complete dialog (names the person and
     the day; the Trainee role is removed, a graduation email is sent, encounters after that day count towards nothing,
     none of it can be undone); a future day refused ("The completion date cannot be after today (2026-09-26).", read
     with the field, the typed day kept); completed; the Deactivate dialog (names the last day; the profile cannot be
     made active again); deactivated; the Trainees list's "Completed & closed profiles" section ("Completed
     2026-09-26", "Withdrawn 2026-09-24"); the ended profile as a read-only record: curriculum, programme start and
     expected completion as a details list, no inputs, no Save profile, Back to trainees kept.
  4. / and the nav — The former trainee's home (NEW dashboard, keyed on the trainee record, not a role): "You completed
     your programme on 26 September 2026, so no target applies to you any more." with links to My progress, My
     authorisations and Export portfolio; her nav (Home, My Account, Data Rights, My Progress, Export Portfolio); the
     withdrawn trainee's home (he keeps the Trainee dashboard; its Curriculum targets card reads "Your programme ended
     on 24 September 2026, so no target applies to you any more." and points to My progress); both at 390 px.
  5. /portfolio/progress, /activities/mine, /activities/{ActivityId:int} — The record. My progress while the programme
     runs with the exit rule met; completed (notice "You completed your programme on 26 September 2026. This page is
     your record of it and is read-only …", a "Your programme" card, each EPA's periods newest first, the last period
     "no target (your programme ended part-way through) · n recorded", no progress bars, the standing, the
     trajectories); withdrawn (the same, "Ended"); 390 px. My Activities with the after-the-end Mini-CEX credited None,
     and its page warning that it counted towards no curriculum requirement because it was observed after the programme
     ended.

STEPS: 5.9–5.29, 6.21, 6.40, 6.41, A.7.4 and A.7.12 (26 steps), pasted verbatim in my next message (Role / Route / Do /
  Expect). Walk them in order; each Expect is what the screen must let the person see or do.

STATES TO SHOW, beyond those above: loading (a skeleton under the page header) and load error on Export, the trainee
  profile and My progress; access denied as the graduate meets it on My Committee Reviews and MSF Reports, which stay
  role-only; session ended (her open tab signs out when her role is removed). Data volumes: one export of a file and
  three identical exports; a Completed & closed list of 2 and of 40; a record with 15 EPA cards and with 17.

REQUIREMENTS FROM KNOWN DEFECTS (each is testable):
  - T311: a user with no role and a trainee record gets a former-trainee home, never "No role assigned". It carries the
    ended line and links to My progress, My authorisations and Export portfolio. Her nav offers My Progress and Export
    Portfolio. My authorisations admits her and its Download works. MSF Reports and My Committee Reviews stay role-only.
    A graduate who later holds a role (for example Assessor) gets that role's dashboard. "No role assigned" stays for an
    account with neither a role nor a record.
  - T311: the graduation email says only that the programme was marked complete on that day and that her record stays
    in Wombat (My progress, her STAR certificates, the portfolio export), with a link to My progress. It drops "Your
    committee has ratified your final entrustment decisions": Mark complete never checks that.
  - T314: the staff export names the trainee in its header; the trainee herself reads "Export your portfolio". It is
    linked from the committee review page and the trainee profile page, offered only to someone the export admits, and
    the link is named for the person. An unknown id and an out-of-scope id get one identical refusal, with no form.
  - T305: a completed or withdrawn profile is read-only (details list, no inputs, no Save profile).
  - T313: verify ignores letter case; a value that is not 64 hexadecimal digits gets its own message; several exports
    of one file answer with the first generation, the count and the last export; ids stay ids. T287: the field's
    placeholder does not say "from the PDF footer", because the PDF prints no hash.
  - T312: an ended record is frozen at its last day. An EPA the College pauses afterwards stays on it, headed "(no
    longer in use)"; an item the institution adds afterwards does not appear on it.
  - T310: the STAR certificate names the institution whose panel issued it (the PDF, not a screen).
  - Every dialog names its target, its trigger is an outline button, and the result takes the focus; a refusal keeps
    what was typed. People by name everywhere except on Verify, which shows ids so as to disclose no name. Every time
    carries its zone, and "today" is the South African date (T325: Export's default range, the completion's "cannot be
    after today"). At 390 px nothing scrolls sideways.

QUESTIONS THE DESIGN MUST ANSWER:
  1. What does a graduate come back for, and what does her home lead with: the record, the certificates, or the export?
  2. May a graduate file an activity about herself? It is undecided. Today /activities/new admits her, and the withdrawn
     trainee's dashboard still offers Log an activity and Request an assessment. Do not assume either answer: show the
     home without a filing action, and say where one would sit if it is allowed.
  3. Verifying a STAR certificate is not built: /portfolio/verify checks portfolio exports only. Leave it out, or show
     it as a clearly marked future state?

CONSTRAINTS:
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

ASK:
  - Round 1, WIREFRAME, 2–3 variations: screen 4 (the former trainee's home and nav, desktop and 390 px) and the staff
    export entry points on the review page and the trainee profile page. Stop for my pick.
  - Round 2, FULL FIDELITY, 2–3 variations: screens 1, 2, 3 and 5, in the variation I picked.
  - Name every design-system component you use and mark anything else NEW. Say which Wombat rule a variation breaks.
  - Show the empty, error and loading states and the data volumes above. Review each screen for WCAG 2.1 AA: contrast,
    focus, targets of at least 24 px, and colour never the only signal.
  - Ask me before assuming anything the steps leave open.

ATTACHED: states/export-portfolio--own.png, states/export-portfolio--staff.png,
  act-5/5.11-1-portfolio-summary-stars.png (the PDF's summary page), states/verify-export--verified.png,
  states/verify-export--no-match.png, states/trainee-profile-edit--complete-dialog.png,
  act-5/5.18-1-graduation-email.png, states/home--no-role.png, act-5/5.22-2-molefe-authorisations-denied.png,
  states/my-progress--completed.png, states/my-progress--withdrawn.png, act-5/5.21-1-molefe-home-graduate.png.
  These show today's pages, defects included; they are not the target.
```

## The journey

One line per step, in play order: who does what, and what the screen must let them see. The full steps are in the last
section.

| Step | Page | Who does what | What they must be able to see |
|---|---|---|---|
| 5.9 | `/portfolio/export` | Dr Molefe opens Export Portfolio from the menu, sets `J−3y` to `D` and exports | The last twelve months filled in; "Portfolio exported successfully …"; a `portfolio-<12 hex>.pdf` download |
| 5.10 | `/admin/users` → `/admin/users/{UserId}` → `/portfolio/export/{TraineeUserId}` | Prof Mbatha exports Dr Molefe's portfolio | Today she types the id. After T314: a link from a named trainee, and a header naming Dr Molefe |
| 5.11 | none (the PDF) | Prof Mbatha reads the PDF | Not a screen. Its summary page is `act-5/5.11-1-portfolio-summary-stars.png`; brief the PDF separately if wanted |
| 5.12 | `/portfolio/export/{TraineeUserId}` | Mr Smit reproduces the export | The trainee named; after T314, a link from the review page he already has open (5.2) |
| 5.13 | `/portfolio/verify` | The verifier, signed out, enters the file's SHA-256 | "Export verified": when it was generated, the trainee and exporter as ids, the filter and the file name |
| 5.14 | `/portfolio/verify` | The verifier checks a tampered copy, then presses Verify on spaces | "No matching export found …"; "Enter the content hash to check."; never an error page |
| 5.15 | `/portfolio/progress` | Dr Molefe reads her progress while the programme runs, and leaves the tab open | This period's targets and bars; the exit rule met; the trajectories |
| 5.16 | `/admin/trainees` → `/admin/trainees/edit` | Prof Mbatha tries to record `J+1y−1d` as the graduation day | The dialog naming the day and every consequence; the refusal read with the field; the profile still Active |
| 5.17 | `/admin/trainees/edit` → `/admin/trainees` → `/admin/users/{UserId}` | Prof Mbatha marks the programme complete on `D` | The result; Status Completed; the end controls gone; the profile read-only (T305); "Completed & closed profiles"; the user page with no roles |
| 5.18 | none (the email) | The system emails Dr Molefe | Only what is true (T311) |
| 5.19 | `/portfolio/progress` → `/account/session-ended` → `/account/login` | Dr Molefe returns to her open tab | "Your session has ended. Please sign in again." |
| 5.20 | `/account/login` → `/portfolio/progress` | Dr Molefe signs in again | The read-only record: notice, "Your programme" card, periods newest first, no bars, the standing, the trajectories |
| 5.21 | `/` | Dr Molefe reads Home and the menu | Today "No role assigned". After T311, the former-trainee home and its three links |
| 5.22 | `/committee/my-reviews`, `/portfolio/authorisations`, `/msf/my-reports` → `/access-denied`; `/activities/new` | Dr Molefe types the trainee pages' addresses | Access denied on My Committee Reviews and MSF Reports; after T311, My authorisations admits her; the filing question (Q2) |
| 5.23 | `/portfolio/export` → `/portfolio/verify` | Dr Molefe exports as a graduate and verifies the new file | Export in her menu (T311); a new file that says she completed the programme; both files verify |
| 5.24 | `/activities/new` → `/activities/{ActivityId:int}` | Dr du Plessis files a Mini-CEX dated `D−1`, after his post ended | Requested, with no late-filing warning |
| 5.25 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Naidoo rates it `3b` and completes it | Completed, read-only, credited "1 item" |
| 5.26 | `/portfolio/progress` | Dr du Plessis reads PAED-002 | This period's count includes it, as the latest encounter |
| 5.27 | `/admin/trainees` → `/admin/trainees/edit` → `/admin/trainees` | Prof Mbatha deactivates his profile with last day `D−2` | The dialog naming the day; the result; Inactive, "Left the programme"; "Withdrawn" in the closed list; the credit taken back in the same save |
| 5.28 | `/` → `/portfolio/progress` → `/activities/mine` | Dr du Plessis reads his record | Home's ended line and link; My progress read-only, "Ended"; the Mini-CEX Completed, credited None |
| 5.29 | `/committee/reviews` | Mr Smit opens Schedule review | Only the current trainees are offered |
| 6.21 | `/portfolio/progress` | Dr Molefe reads her record while PAED-012 is paused | Today PAED-012 drops out. After T312 it stays, headed "(no longer in use)" |
| 6.40 | `/portfolio/progress` | Dr Molefe reads her record after the rebuild | Today a KGK-001 card appears with shortfalls. After T312 her record is as she graduated |
| 6.41 | `/portfolio/progress` → `/activities/mine` | Dr du Plessis reads his record after the rebuild | Still ended and read-only; the Mini-CEX still credited None |
| A.7.4 | `/` → `/portfolio/progress` | Dr Molefe at 390 px | Home says she completed her programme and points to My progress; the record fits the width |
| A.7.12 | `/account/login` → `/account/forgot-password` → `/portfolio/verify` | The verifier at 390 px | The verify result fits the width, the ids wrapping |

## States to design

28 captures, from `execution/knowledge/scenario-paediatrics/states.md`. The line number is the row in `states.md`.
**HELD** means the capture predates a group-1 fix and must be re-captured before it is briefed from (BRIEF § 10).

| Page slug | State | Screenshot (`design/baseline/…`) | Who | How it is reached (`states.md` line) |
|---|---|---|---|---|
| `export-portfolio` | Own, before export | `states/export-portfolio--own.png` | Dr Molefe | At Step 5.9, before Export: the last twelve months filled in. (:310) |
| `export-portfolio` | Exported | `states/export-portfolio--exported.png` | Dr Molefe | At Step 5.9. (:311) |
| `export-portfolio` | Staff, by address | `states/export-portfolio--staff.png` | Prof Mbatha | At Step 5.10, before Export. (:312) |
| `export-portfolio` | Refused | `states/export-portfolio--refused.png` | Dr Dlamini | Scratch (post-appendix): type Dr Molefe's export address, press Export: "You are not authorized to export this portfolio." (:313) |
| `export-portfolio` | Narrow | `states/export-portfolio--narrow.png` | Dr Molefe | At Step 5.23, at 390 px, before Export. No change. (:314) |
| `verify-export` | Blank | `states/verify-export--blank.png` | Anonymous | At Step 5.13, before Verify. (:315) |
| `verify-export` | Verified | `states/verify-export--verified.png` | Anonymous | At Step 5.13. (:316) |
| `verify-export` | No match | `states/verify-export--no-match.png` | Anonymous | At Step 5.14, the tampered copy. (:317) |
| `verify-export` | Empty hash | `states/verify-export--empty.png` | Anonymous | At Step 5.14, spaces only. (:318) |
| `verify-export` | Could not check | `states/verify-export--check-failed.png` | Anonymous | Hold a read, open `/portfolio/verify?hash=0&check=1`, wait 35 s: "The export could not be checked just now." No change. (:319) |
| `verify-export` | Narrow | `states/verify-export--narrow.png` | Anonymous | At Step A.7.12. (:320) |
| `trainee-profile-edit` | Mark complete dialog | `states/trainee-profile-edit--complete-dialog.png` | Prof Mbatha | At Step 5.16. (:677) |
| `trainee-profile-edit` | A future day refused | `states/trainee-profile-edit--future-refused.png` | Prof Mbatha | At Step 5.16. (:678) |
| `trainee-profile-edit` | Completed | `states/trainee-profile-edit--completed.png` | Prof Mbatha | At Step 5.17. (:679) |
| `trainee-profile-edit` | Deactivate dialog | `states/trainee-profile-edit--deactivate-dialog.png` | Prof Mbatha | At Step 5.27. (:680) |
| `trainee-profile-edit` | Deactivated | `states/trainee-profile-edit--deactivated.png` | Prof Mbatha | At Step 5.27. (:681) |
| `pending-trainees-list` | Completed and closed | `states/pending-trainees-list--closed.png` | Prof Mbatha | At Step 5.27. (:671) |
| `user-detail` | No roles | `states/user-detail--no-roles.png` **HELD (T303)** | Prof Mbatha | At Step 5.17. (:648) |
| `home` | No role assigned | `states/home--no-role.png` | Dr Molefe (former trainee) | At Step 5.21: the code shows the "No role assigned" card (the step's expectation is the decided wording; see its note). (:136) |
| `home` | Trainee whose programme ended | `states/home--trainee-ended.png` (re-captured after T297, 2026-09-26) | Dr du Plessis | At Step 5.28: "Your programme ended on …, so no target applies to you any more." (:135) |
| `home` | Narrow: former trainee | `states/home--narrow-former-trainee.png` | Dr Molefe | At Step A.7.4. (:140) |
| `my-progress` | Exit rule met | `states/my-progress--exit-met.png` | Dr Molefe | At Step 5.15. (:292) |
| `my-progress` | Completed programme, read-only | `states/my-progress--completed.png` | Dr Molefe (former trainee) | At Step 5.20. (:293) |
| `my-progress` | Withdrawn, read-only | `states/my-progress--withdrawn.png` | Dr du Plessis | At Step 5.28. (:294) |
| `my-progress` | Narrow, former trainee | `states/my-progress--narrow-former.png` | Dr Molefe | At Step A.7.4. (:303) |
| `activity-view` | Credited nothing, after the last day | `states/activity-view--after-programme-end.png` | Dr du Plessis | After Step 5.27, his `D−1` Mini-CEX: the same warning, credit None. (:264) |
| `my-activities` | Credited None after leaving | `states/my-activities--credited-none.png` | Dr du Plessis | At Step 5.28. (:272) |
| `reviews-schedule` | Current trainees only | `states/reviews-schedule--current-only.png` | Mr Smit | At Step 5.29, the trainee list open. (:353) |

Three states have no capture, because they do not exist yet. Design them from the requirements:
- **The former trainee's home** (T311; BRIEF § 7 B1). `states/home--no-role.png` is what she sees today.
- **Verify, "not a hash"**, and **verified with several exports** (T313).
- **The ended profile as a read-only record** (T305; BRIEF § 7 B12). `states/trainee-profile-edit--completed.png` shows
  today's editable form.

## Attach

Every path is under `design/baseline/` and was checked to exist on 2026-09-26. Open each before uploading (BRIEF
§ 3.3). None of these shows a registration link or a password.

**First, with the ask (the KEY SCREENSHOTS):**
- `states/export-portfolio--own.png`
- `states/export-portfolio--staff.png`
- `act-5/5.11-1-portfolio-summary-stars.png`
- `states/verify-export--verified.png`
- `states/verify-export--no-match.png`
- `states/trainee-profile-edit--complete-dialog.png`
- `act-5/5.18-1-graduation-email.png`
- `states/home--no-role.png`
- `act-5/5.22-2-molefe-authorisations-denied.png`
- `states/my-progress--completed.png`
- `states/my-progress--withdrawn.png`
- `act-5/5.21-1-molefe-home-graduate.png`

**Then, as the chat asks, by screen:**
- Export: `states/export-portfolio--exported.png`, `states/export-portfolio--refused.png`,
  `states/export-portfolio--narrow.png`, `act-5/5.10-1-mbatha-staff-export.png`,
  `act-5/5.12-1-smit-reproduces-export.png`, `act-5/5.23-1-graduate-exported.png`,
  `act-5/5.11-2-portfolio-committee-decisions.png`.
- Verify: `states/verify-export--blank.png`, `states/verify-export--empty.png`,
  `states/verify-export--check-failed.png`, `states/verify-export--narrow.png`,
  `act-5/5.13-2-verifier-uppercase-hash.png` (the upper-case defect), `act-5/5.23-2-graduate-file-verified.png`,
  `act-A/A.7.12-3-verify-molefe.png`.
- Ending a programme: `states/trainee-profile-edit--future-refused.png`, `states/trainee-profile-edit--completed.png`,
  `states/trainee-profile-edit--deactivate-dialog.png`, `states/trainee-profile-edit--deactivated.png`,
  `states/pending-trainees-list--closed.png`, `act-5/5.17-1-mbatha-marked-complete.png`,
  `act-5/5.17-2-mbatha-trainees-completed.png`.
- The graduate: `states/home--narrow-former-trainee.png`, `act-A/A.7.4-1-molefe-home-no-role.png`,
  `act-5/5.19-1-molefe-session-ended.png`, `act-5/5.22-1-molefe-my-reviews-denied.png`,
  `act-5/5.22-3-molefe-msf-reports-denied.png`, `act-5/5.22-4-molefe-new-activity-opens.png`.
- The record: `states/my-progress--exit-met.png`, `states/my-progress--narrow-former.png`,
  `act-A/A.7.4-2-molefe-record.png`, `states/activity-view--after-programme-end.png`,
  `states/my-activities--credited-none.png`, `states/reviews-schedule--current-only.png`.
- Only as evidence of T312's drift, never as a target: `act-6/6.21-1-molefe-record-paused.png`,
  `act-6/6.40-2-molefe-kgk001-card.png`, `act-6/6.41-1-duplessis-record-after-rebuild.png`.

**HELD: do not attach until re-captured after the group-1 fix lands** (BRIEF § 10):
- T303 (Add role offers Trainee): `act-5/5.17-3-mbatha-user-no-roles.png`, `states/user-detail--no-roles.png`.

**Re-captured after T297 landed (2026-09-26), so no longer held:** `states/home--trainee-ended.png`,
`act-5/5.28-1-duplessis-home-ended.png` (both from the end-of-Act-5 snapshot) and
`act-A/A.4.6-1-trainee-dashboard-ended.png` (the end of the appendix). The ended trainee does get the trainee dashboard,
and T297 changed it: its Activity inbox card now lists his requested CBD and his portfolio review awaiting review, the
two rows his inbox lists (Step 5.28's re-check).

## Known problems this design must solve

| Task | Priority | What it means for the design | Evidence |
|---|---|---|---|
| T311 | P2 | A former-trainee dashboard keyed on the `trainee_record` claim, never "No role assigned" (`Home.razor:61-70` today). Her nav offers My Progress and Export Portfolio (`NavMenu.razor:146` today offers My Progress only). My authorisations admits her (`MyAuthorisations.razor:2` today is Trainee-only). MSF Reports and My Committee Reviews stay role-only (DESIGN.md:236). The graduation email drops the ratification claim and links to My progress (`GraduationEmail.cs`) | `states/home--no-role.png`, `act-5/5.21-1-molefe-home-graduate.png`, `act-5/5.22-2-molefe-authorisations-denied.png`, `act-5/5.18-1-graduation-email.png` |
| T314 | P3 | The staff export's header names the trainee ("Export Lerato Molefe's portfolio"), and the page is linked from the committee review page and the trainee profile page, only for a caller the export admits (BRIEF § 7 B2). An unknown id and an out-of-scope id get one refusal with no form. DESIGN.md:1701-1702 already says no input takes a raw user id | `states/export-portfolio--staff.png`, `act-5/5.10-1-mbatha-staff-export.png` |
| T305 | P2 | A completed or withdrawn profile is a details list with no inputs and no Save profile; the command refuses a save ("This programme ended on <date>; its record is archived and cannot be changed."). BRIEF § 7 B12 | `states/trainee-profile-edit--completed.png`, `states/trainee-profile-edit--deactivated.png` |
| T313 | P2 | Verify: case-insensitive; "not a hash" message; "first generated on …, exported N times, last exported on …"; ids stay ids. Verification records are kept for the life of the record (the 90-day cleanup goes) | `act-5/5.13-2-verifier-uppercase-hash.png`, `states/verify-export--verified.png` |
| T287 | P3 | The hash field's placeholder "Enter the hash from the PDF footer" is false: the PDF prints no hash (F-5.13b). T313 may take it | `states/verify-export--blank.png` |
| T312 | P2 | An ended record reads the EPA list and targets it ended with. A later pause shows "(no longer in use)" and keeps the EPA; a later local item does not appear. The PDF's "Progress per EPA" follows | `act-6/6.21-1-molefe-record-paused.png`, `act-6/6.40-2-molefe-kgk001-card.png` |
| T310 | P3 | The certificate names the issuing panel's institution. A PDF, not a screen: note it if the PDF is briefed | none (code read) |
| T325 | P3 | Export's default range and "today" read the South African date; Verify prints a labelled time (today "2026-09-26 12:09 UTC") matching what the PDF prints (T325's rule) | `states/verify-export--verified.png` |
| T323, T328 | P3 | At 390 px the verify result's ids wrap (A.7.12 found this holds); controls use the body font (BRIEF § 6 A3, A4) | `states/verify-export--narrow.png` |
| T153 | P3 | Filing after leaving is undecided (Q2); do not design it in or out | `act-5/5.22-4-molefe-new-activity-opens.png` |

## Questions the design must answer

1. **What does a graduate come back for, and what does her home lead with?** What she keeps: My progress as a read-only
   record (5.20), 15 STAR certificates (5.8, after T311), and the portfolio export (5.23), which the graduation email
   promises (5.18). Which one leads, and how does the home say the programme is over?
2. **May a graduate file an activity about herself?** Undecided: `coverage.md` § Flows and states not played, "A
   graduate filing an activity about herself … A decision first"; T153 is the related task. Today `/activities/new`
   admits her (5.22), and the withdrawn trainee's dashboard still offers Log an activity and Request an assessment (5.28
   Actual). The design must not assume either answer.
3. **STAR certificate verification is not built.** `/portfolio/verify` checks exports only, and a certificate's hash
   is recorded nowhere (`coverage.md` § Flows and states not played; BRIEF § 7 B10). Leave it out, or show it as a
   marked future state?

## Notes

- `/portfolio/verify` is static and excluded from interactive routing. Its Verify is a GET form, checked as the page
  renders: it loads `/portfolio/verify?hash=…&check=1` (`Components/App.razor` remarks; DESIGN.md:1921-1922; T265).
  Nothing on it can react before a full page load.
- The PDF is not a screen. `act-5/5.11-1-portfolio-summary-stars.png` and
  `act-5/5.11-2-portfolio-committee-decisions.png` show its summary and committee pages; Step 5.11 lists its sections.
  Brief it separately if wanted.
- Session ended (5.19) and Access denied (5.22) are framework states designed in F01 and F02 (BRIEF § 5.1). This flow
  only has to show them in context.

## Acceptance

After Claude Code builds the chosen design (BRIEF § 9), the flow is done when:
- **The steps replay on a fresh database.** Play Acts 1–4, then **5.9–5.29**, then Act 6 through **6.21**, **6.40** and
  **6.41**, then the appendix's **A.7.4** and **A.7.12** (`README.md` § How to play; `tools/scenario-replay.ps1
  create|publish|start wombat_scenario_f13`). Every Expect holds. Where the design changes wording, the steps' Expect
  lines change in the same task (BRIEF § 9, item 7). Expect to rewrite 5.10 and 5.12 (the named export and its links),
  5.13 (verify's answer), 5.17 (the read-only profile), 5.18 (the email), 5.21–5.23 (home, nav, My authorisations,
  Export in the menu), and 6.21 and 6.40 once T312 lands.
- **The tests pass:** `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`, including
  `Navigation/NavMenuAuthorizationTests` (the graduate's nav), `DashboardLinkAuthorizationTests` (every link on the
  former-trainee card opens a page that admits her), `Portfolio/VerifyExportPageTests`, and T294's guard in
  `tests/Wombat.Web.Tests/Scenario/`.
- **The baseline is re-captured:** the 28 states above and the step captures of 5.9–5.29, 6.21, 6.40, 6.41, A.7.4 and
  A.7.12, compared with the chosen artboards.
- **The browser check passes** at 1280 and 390 px for each person in the flow: Dr Molefe as trainee and as former
  trainee, Dr du Plessis, Prof Mbatha, Mr Smit, Dr Naidoo, and signed out on Verify.

## The runbook steps, verbatim

Paste this as the second message. It is each step's Role, Route, Do and Expect, copied from
`execution/knowledge/scenario-paediatrics/` (`act-5-graduation.md`, `act-6-catalogue.md`, `appendix-cross-cutting.md`).
`D` is the replay day and `J` the latest 15 January on or before it: on 2026-09-26, `D` is 2026-09-26 and `J−3y` is
2023-01-15.

```text
Step 5.9 — Dr Molefe exports her portfolio (act-5-graduation.md)
Role: Trainee — Dr Lerato Molefe
Route: /portfolio/export
Do: Open Export Portfolio from the menu. Set the from date to her programme start, `J−3y`, and the to date to `D`, and
  export the PDF.
Expect: The page opens with the last twelve months filled in. After the export it says "Portfolio exported
  successfully. The download should start automatically.", and a file named `portfolio-<12 hex digits>.pdf` downloads.
  Record the file name: Steps 5.10 and 5.12 must produce the same one.

Step 5.10 — Prof Mbatha exports Dr Molefe's portfolio by its address (act-5-graduation.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId} → /portfolio/export/{TraineeUserId}
Do: Find Dr Molefe in Users and open her. Take her user id from the page's address, then type the export address with
  that id. Set the dates to `J−3y` and `D`, and export.
Expect: The export succeeds as in Step 5.9. The downloaded file has the same name as Dr Molefe's and is identical to it
  byte for byte. Compare the two with `fc /b`, or by their SHA-256.

Step 5.11 — Prof Mbatha reads the portfolio (act-5-graduation.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: n/a
Do: Open the PDF from Step 5.10 and read it through.
Expect: Every page carries the same header and footer:
  - header: Kgosi Kgari Teaching Hospital, "Portfolio Export", Lerato Molefe, Paediatric EPA Curriculum, "Paediatrics —
    Paediatrics" (speciality and sub-speciality), and "Period: `J−3y` to `D`";
  - footer: "Page n of m | Generated by Wombat", with no generation time.
  The sections follow in this order:
  - a Summary, with activities by type and the number of committee reviews;
  - "Statements of Awarded Responsibility (STARs)", reading "15 active entrustment decisions on record." and giving each
    EPA's rung by its label;
  - "Progress per EPA", read on `D` in training year 4;
  - "Committee Decisions", listing each ratified review. The final one reads Pre-graduation review, Ratified, "Graduate
    (programme complete)", with its rationale and the four members present by name;
  - her activities by type, with rungs shown by their labels and people by their names;
  - the MSF campaign;
  - "Appendix — Audit Trail".

Step 5.12 — Mr Smit reproduces the export (act-5-graduation.md)
Role: Coordinator — Mr Pieter Smit
Route: /portfolio/export/{TraineeUserId}
Do: Type the export address with Dr Molefe's user id, which Prof Mbatha sends him. Set the same two dates, and export.
Expect: The export is allowed, because a coordinator of her institution oversees her (T079, T101). The file has the
  same name as those from Steps 5.9 and 5.10, and is identical to both byte for byte.

Step 5.13 — A verifier checks the genuine PDF (act-5-graduation.md)
Role: Anonymous — a credentialing officer at Dr Molefe's new hospital
Route: /portfolio/verify
Do: In a private window, without signing in, open the verification page. Compute the SHA-256 of the PDF that Dr Molefe
  sent (`certutil -hashfile <file> SHA256`, or `sha256sum`, both of which print it in lower case), enter it and press
  Verify. Do the same for Mr Smit's copy.
Expect: The page works signed out, as a static page. Verifying loads the page's own address with the hash in it. The
  page reads "Export verified" and shows:
  - when Wombat generated the file, in UTC;
  - the trainee and the exporter as user ids, never as names (DESIGN: the anonymous page must not disclose a name);
  - "Filter: `J−3y` to `D`";
  - the file name that all three downloads carry.
  Mr Smit's copy has the same SHA-256 and gets the same answer.

Step 5.14 — The verifier checks a tampered copy (act-5-graduation.md)
Role: Anonymous — a credentialing officer at Dr Molefe's new hospital
Route: /portfolio/verify
Do: Change one byte in a copy of the PDF, compute the copy's SHA-256 and verify it. Then press Verify with only spaces
  in the field.
Expect: The tampered copy gets "No matching export found. This hash does not correspond to any PDF generated by this
  system." The press with only spaces gets "Enter the content hash to check." Neither leads to an error page.

Step 5.15 — Dr Molefe leaves My progress open (act-5-graduation.md)
Role: Trainee — Dr Lerato Molefe
Route: /account/login → /account/login/submit → / → /portfolio/progress
Do: In her own browser, sign in and open My progress. Leave the tab open through Step 5.19.
Expect: The page shows her programme as still running:
  - "This period", and each EPA's target for it, with progress bars;
  - on each EPA's card, the period before it;
  - "Entrustment against Annexure A", with the exit rule met as in Step 5.6;
  - her rating trajectories.
  Nothing says that her programme has ended.

Step 5.16 — Prof Mbatha tries to record the story's graduation day (act-5-graduation.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Molefe's profile from Active profiles. Set the last day in the programme to the story's graduation day,
  `J+1y−1d`. Press Mark complete and confirm.
Expect: The last-day field opens on `D`. Among the rest, its help says that:
  - Mark complete records the graduation day, archives the profile and removes the Trainee role;
  - the day cannot be after today;
  - the day cannot be changed afterwards.
  The confirmation names `J+1y−1d` (as yyyy-MM-dd) as her graduation day. It says that the Trainee role will be
  removed, a graduation email will be sent, encounters after that day will count towards nothing, and none of this can
  be undone. Once
  confirmed, the request is refused with "The completion date cannot be after today (`D`)." (`D` shown as yyyy-MM-dd),
  which is read with the field. The profile is still Active.

Step 5.17 — Prof Mbatha marks Dr Molefe's programme complete on `D` (act-5-graduation.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees/edit → /admin/trainees → /admin/users/{UserId}
Do: Set the last day to `D`, press Mark complete and confirm. Then go back to Trainees, and open Dr Molefe in Users.
Expect: The page says "Trainee marked complete. The Trainee role has been removed and a graduation email sent."
  - The summary reads Status Completed, Completed `D`.
  - The last-day field, Deactivate and Mark complete are gone.
  - On Trainees she is no longer under Active profiles. She is listed under Completed & closed profiles with the outcome
    "Completed `D`".
  - Her user page says "This user has no roles."
  - No credit is taken back, because nothing of hers is observed after `D` (T281; Step 5.27 shows a take-back).

Step 5.18 — Dr Molefe is emailed (act-5-graduation.md)
Role: System — the graduation email sent by Mark complete
Route: n/a
Do: Read the application log for the mail sent in Step 5.17.
Expect: There is one email to `molefe@kgk.wombat.local` with the subject "Congratulations on completing Paediatric EPA
  Curriculum". It congratulates her on completing the programme as of `D`, and says that her committee has ratified
  her final entrustment decisions and that her portfolio is available in Wombat.

Step 5.19 — Dr Molefe's open session ends (act-5-graduation.md)
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress → /account/session-ended → /account/login
Do: Go back to the tab left open at Step 5.15 and wait up to a minute.
Expect: Removing her role changed her account's security stamp. The session check, which runs once a minute, ends the
  session: the tab reloads to the sign-in page, which says "Your session has ended. Please sign in again." Nothing
  she had open still acts as a Trainee.

Step 5.20 — Dr Molefe signs in again and reads her record (act-5-graduation.md)
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /account/login → /account/login/submit → /portfolio/progress
Do: Sign in from the page that Step 5.19 left her on. She is returned to My progress. Read it.
Expect: Her trainee record admits her to the page (T252, `TraineeOrFormerTrainee`).
  - An information notice reads "You completed your programme on <`D` as a long date>. This page is your record of it
    and is read-only: no target applies to you any more.", followed by D49's rule.
  - A "Your programme" card gives her start date, Completed `D`, and training year 4 as the year the programme ended.
  - Each EPA's card lists its periods, newest first. The period holding `D` reads "no target (your programme ended
    part-way through) · n recorded", and the earlier periods read as met or short.
  - There are no progress bars.
  - The standing reads the year her programme ended in, with the exit rule met.
  - Her trajectories are still shown.

Step 5.21 — What her home page and menu offer a graduate (act-5-graduation.md)
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /
Do: Open Home and read the menu.
Expect: The menu offers Home, My Account, Data Rights, My Progress and Logout, and nothing else (DESIGN § The NavMenu,
  T252). Home says "You completed your programme on <`D` as a long date>, so no target applies to you any more." and
  points her to My progress, read-only, as the trainee dashboard's Curriculum targets card does for an ended programme
  (T252 As built: "Home says the same").

Step 5.22 — What a graduate can no longer open (act-5-graduation.md)
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /committee/my-reviews → /access-denied → /portfolio/authorisations → /access-denied → /msf/my-reports → /access-denied → /activities/new
Do: Type the addresses of My Committee Reviews, My authorisations and MSF Reports in turn. Then type the address of the
  new-activity page.
Expect: Each of the first three shows Access denied: those pages still require the Trainee role (DESIGN § The NavMenu).
  Nothing offers her an activity to file: there is no menu link and no dashboard action. The new-activity page asks
  only that she is signed in, so it opens when she types its address. She files nothing.

Step 5.23 — Dr Molefe exports her portfolio as a graduate (act-5-graduation.md)
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/export → /portfolio/verify
Do: Type the export address, set the dates to `J−3y` and `D`, and export. Then verify the new file's SHA-256
  on the verification page.
Expect: The export page is not in her menu, but it admits her, because the portfolio is her own (`MayReadAsync`). The
  file has a new name. Its "Progress per EPA" now says "The trainee completed the programme on <`D`>", so the content
  differs from Steps 5.9 to 5.12. The new file verifies, with the same trainee, the same filter and its own
  file name. The earlier file still verifies too.

Step 5.24 — Dr du Plessis asks for a Mini-CEX on a shift after his post ended (act-5-graduation.md)
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/{ActivityId:int}
Do: File a Mini-CEX (Paediatrics) on PAED-002 with:
  - encounter date `D−1`;
  - Dr Naidoo as the assessor;
  - a clinical setting, a presenting problem and a case complexity.
  Submit it.
Expect: The EPA list offers PAED-002, because its tool list includes the Mini-CEX. There is no late-filing warning. The
  activity reads Requested and appears in Dr Naidoo's inbox. No email is sent: Wombat mails nobody when an activity
  moves (Step 3.3).

Step 5.25 — Dr Naidoo rates it (act-5-graduation.md)
Role: Assessor — Dr David Naidoo
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr du Plessis's Mini-CEX from the inbox and rate it `3b`, PAED-002's minimum in training year 2. Write what
  was done well, what to develop and the agreed plan, and complete it.
Expect: The activity reads Completed and is read-only to both of them. It credits PAED-002 in the semester that holds
  `D−1` (checked in Step 5.26), and My Activities reads it credited "1 item". Nobody is emailed.

Step 5.26 — Dr du Plessis sees it counted (act-5-graduation.md)
Role: Trainee — Dr Pieter du Plessis
Route: /portfolio/progress
Do: Open My progress and read PAED-002's card for this period.
Expect: This period's count for PAED-002 includes the `D−1` Mini-CEX, shown as the latest encounter. Record the count
  as n.

Step 5.27 — Prof Mbatha records his withdrawal (act-5-graduation.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Open Dr du Plessis's profile and set the last day in the programme to `D−2`. Press Deactivate and confirm.
Expect: The confirmation names `D−2` as his last day. It says that encounters after that day will count towards
  nothing, including any already counted, and that neither the day nor the profile can be changed back. After
  confirming:
  - the page says "Trainee profile deactivated. Their last day in the programme is recorded as `D−2`.";
  - the summary reads Status Inactive, "Left the programme: `D−2`";
  - on Trainees he is listed under Completed & closed profiles, with the outcome "Withdrawn `D−2`";
  - in the same save, the credit from the `D−1` Mini-CEX is taken back (T281);
  - he keeps the Trainee role and is not emailed.

Step 5.28 — Dr du Plessis's record after he left (act-5-graduation.md)
Role: Trainee — Dr Pieter du Plessis
Route: /account/login → /account/login/submit → / → /portfolio/progress → /activities/mine
Do: Sign in, then read Home, My progress and My Activities.
Expect: Each page records that his programme ended on `D−2`:
  - Home's Curriculum targets card says "Your programme ended on <`D−2` as a long date>, so no target applies to you any
    more.", and points to My progress.
  - My progress opens with "Your programme ended on <`D−2`>. This page is your record of it and is read-only…". Its
    "Your programme" card reads Ended `D−2`. PAED-002's period holding `D−2` reads "no target (your programme ended
    part-way through) · n−1 recorded".
  - My Activities still lists the `D−1` Mini-CEX as Completed, now credited "None": the take-back's replay re-stamped
    its completion (`ProgrammeEndCredit`). The filing is kept, but it counts towards nothing.

Step 5.29 — Neither ended programme can be put before the panel (act-5-graduation.md)
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews
Do: Open Schedule review, choose the Paed Annual Review Panel and read the trainee list.
Expect: The list offers only Dr Dlamini, Dr Mahlangu and Dr Ndlovu. A graduate and a registrar who has withdrawn are not
  current trainees (T238). He closes the form without scheduling anything.

Step 6.21 — Dr Molefe's record during the pause (act-6-catalogue.md)
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress
Do: Read her record.
Expect: The record opens with its notice that she completed her programme and that the page is read-only.
  - It lists no PAED-012 card while PAED-012 is paused, as every progress page leaves out an EPA not in force (T158).
  - Her "Entrustment against Annexure A" panel has no PAED-012 row either: the standing reads only items in force.
  - Any rating trajectory she has on PAED-012 still charts, headed "(no longer in use)" (T255).
  - PAED-006's card carries the title corrected in Step 6.14. Her other cards and periods are as Act 5 left them.

Step 6.40 — Dr Molefe's record after the rebuild (act-6-catalogue.md)
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress
Do: Read her record, card by card, against what she read in Act 5 (Step 5.20).
Expect:
  - Her fifteen national cards are back, PAED-012 among them, and each period reads what Act 5 recorded: the rebuild
    replays a completed profile's evidence as it was (Act 5's handoff).
  - PAED-006's card carries the corrected title. There is no PAED-016 card: her profile is on 11.1.
  - A KGK-001 card has appeared under "Once a year", counting none of her encounters: each closed year of her
    programme reads "0 of 1, 1 short". KGK's own item on 11.1 is read live, into a record that has ended.

Step 6.41 — Dr du Plessis's record after the rebuild (act-6-catalogue.md)
Role: Trainee — Dr Pieter du Plessis
Route: /portfolio/progress → /activities/mine
Do: Read his record and his activities.
Expect:
  - My progress still opens with the notice that his programme ended on the day Act 5 recorded, and is read-only.
  - PAED-002's period holding that day still reads "no target (your programme ended part-way through)", with the
    count Act 5 Step 5.28 read: the Mini-CEX observed after his last day still counts towards nothing (T281).
  - A KGK-001 card has appeared on his record too, as on Dr Molefe's.
  - My Activities lists that Mini-CEX as Completed, with Credited None.

Step A.7.4 — Dr Molefe's record on her phone (appendix-cross-cutting.md)
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: / → /portfolio/progress
Do: At 390 px, sign in, read Home, and open My Progress from the folded nav.
Expect: Home says that she completed her programme and points her to My progress, as at Step 5.21 (T252). My Progress
  shows her read-only record of past periods (T252), its cards fitting the width.

Step A.7.12 — The anonymous pages on a phone (appendix-cross-cutting.md)
Role: Anonymous — a verifier
Route: /account/login → /account/forgot-password → /portfolio/verify
Do: At 390 px, open the sign-in page and the forgot-password page. Then verify Dr Molefe's portfolio PDF from Act 5 by
  its hash.
Expect: The sign-in card fits the width, and its fields and buttons are easy to tap. The verify page's result fits
  the width.
```
