# ReviewSitting

A committee review, sat (`/committee/reviews/{ReviewId}`, `Pages/CommitteeDecisions/ReviewDetail.razor`; its reading in `Review/ReviewSitting.cs`, every sentence in `Review/ReviewWords.cs`): **the agenda is the page**. One column under the header: the sitting bar, the notes, the agenda with one line open at a time, the decision, then the evidence the committee reads. Flow 07 (T361, 2026-10-12, 44de180b, with 59bb72fd, 6ad6c57a, 6b149fff and 14378856 from its replay; D53; round 3's R2-Spec; round 3 items 7–31; E1–E9) rebuilt it. Until then it was one long page of cards in a details grid, with Defer and Reinstate on each line and the chair's controls in a card of their own.

The page is a host. What it draws is in `Review/`: SittingBar, AgendaTable with its open line (AgendaLineForm or EvidenceReader), the foot form (AgendaLineForm), DecisionForm. It holds the state they share: the review and its sibling reads, what the chair has typed (`DecisionDraft`, a `LineDraft` per open or kept line, the foot form's), the one result on the page and where it sits, and the five dialogs (ConfirmDialog).

## Who reads it

Committee member, Coordinator, Administrator, Institutional admin and both speciality admins (`[Authorize(Roles = …)]`); the read ladder decides which reviews. The trainee reads theirs on My committee reviews, never here. In the menu it is under Committee reviews for each of those roles (`NavOwners`). An id that names nothing and one out of reach get the same answer (T194).

## The header (the title rule; round 3 item 19, E6)

- **The h1 and the tab**: the trainee and the period, "Lerato Molefe · 2026 S2" ("Lerato Molefe · 2026 S2 · Wombat"); "Committee review" while it is read and when the read failed. The trail is Home › Committee reviews › the h1.
- **The subtitle**: "Annual progression review · Paed Annual Review Panel · Training year 4": the type ("Formative check-in" for a formative review, whatever its stored type; T309 item 2), the panel, and the training year at the end of the review's period.
- **"Lerato Molefe's progress"** under it (`a.review-progress-link`, a link alone on its line, 24px; 44px below 641px), through PageHeader's `SubtitleContent`, to flow 06's registrar page (`/programme/trainees/{ProfileId}`): only for the four roles that page admits (`ProgrammeReadAs`), and only while the trainee trains at the panel's institution.

## The sections, in order (DESIGN.md § Detail page, "The committee review has no details-grid")

`div.review-page`, a flex column 24px apart; each section a `detail-card` (`section`, `aria-labelledby`) with its `h2.review-section-title` (1.15rem/600, the card title's size, one level under the h1, A.7.13):

| Section | What |
|---|---|
| The sitting (SittingBar) | the state, the count, the next act; a visually hidden h2. The page's result sits under it for Start (formative), Ratify and Close. |
| Before the committee decides (`#notes`) | only when it has something to say: "Sampling figures are incomplete" (a danger alert, not dismissible, each cause its own sentence, T135); "Sampling concentration warnings" (h3), one line per EPA, "PAED-010 · 1 rating · 1 assessor · 1 source. Fewer than three distinct assessors across this EPA's evidence.", "(no longer in use)" after a paused code (T273), and "Not counted: …" under them; an EPA whose STAR was revoked mid-review (a warning, T235) with "Stage PAED-003" for the chair, which chooses it in the foot form. |
| The decision (DecisionForm) | **above the agenda once recorded**, below it until then. |
| Agenda (AgendaTable) | the spine: one line per EPA, one open at a time; the foot form under it for the chair. |
| Entrustment against Annexure A | EntrustmentStandingPanel, its EPA names moving the focus to the charts. |
| Multi-source feedback by EPA | the MSF coverage panel (`MsfCoveragePanel`, not recorded here). |
| Evidence snapshot (`#evidence-snapshot`) | "Frozen when the review started, 2026-10-05 · 9 items" (muted); an h3 per EPA (`.snapshot-group-heading`, "PAED-001 — Providing paediatric emergency care to children"), each a stacked table (Item, the row header, a link to flow 03's page with the review's way back, ReviewReturnLink; Assessor; Rating; Encounter; State), each row `id="snap-{itemId}"`; then "Not about a single EPA" and "Frozen before lines named their EPA" as lists. T173's info alert first when a campaign closed in the window is not in the snapshot. |
| Rating trajectory by EPA | a `section.list-section.full-width`, one TrajectoryChart per EPA rated in the window at h3; "Back to PAED-001's line" in a card's head while that line is open. |
| Appeals (`#appeals`, `.review-appeals`) | the appeal body's resolve form under appeal (Outcome, and for Remitted the replacement category, rationale, conditions and Present), else who resolves it; the appeals lodged, "No appeal has been lodged against this review." |

**A formative review** has no agenda and no decision: the bar (its count "Formative check-in · no decision recorded" once closed; its act Close review…), the notes, the snapshot, the standing, the MSF coverage and the trajectories.

## The page's one result (round 3 item 15; T234, T260)

One `ActionResult` (`#review-result`), drawn **beside the control it answers** and taking the focus once drawn: "Review started." atop the agenda; "Pending entrustment decision staged." or "… removed." in a row under its line (`tr.agenda-result-row`), or under the foot form (a removal on a line no longer on the agenda: atop the agenda); a stage's refusal at its form's top ("Not staged. An expiry date must be after the issue date."); "Decision recorded." or "Not recorded. Give the committee's rationale." in the Decision section; "Decision ratified." and "Formative review closed." under the bar; "Appeal resolved." in Appeals. A success is green (`alert-success`, `role="status"`), a refusal red (`alert-danger`, `role="alert"`), printed through `RefusalText.Of`. One action at a time: while one is on its way every other act is disabled, and the pressed one keeps the focus, `aria-disabled`.

The review is **read again after every action**, done or refused (T260), and on Refresh: the review with its agenda, then the staged decisions, the sampling report, the MSF count and the standing, each apart; the trajectories, the MSF coverage and the curriculum only when the state changed.

## States (board 8; T329)

- **Loading**: the header "Committee review", an always-present `role="status"` that says "Loading the review…", a `div.review-page[aria-busy=true]` of skeletons, no action.
- **Load failed**: StatePanel's danger alert, "The review could not be loaded. Nothing on it has changed.", with Try again answering into the h1. Never the exception's text: it goes to the log.
- **Not found** (an unknown id, or one out of reach): flow 01's Page not found in the page's place (PageHeader's `search` icon, the crumb "Not found"), "The committee review could not be found among the reviews you can view." and "Back to Committee reviews" (`a.btn.btn-primary`).
- **A section's own read** fails in its section and the rest stands: "The agenda could not be read. The rest of the review stands.", "The staged decisions could not be read. The agenda shows the ones last read.", "The rating trajectories could not be loaded. The rest of the review stands." (warnings, `role` none); the standing's and the MSF coverage's own words; in an open line, "Standing could not be loaded." once in its place.
- **Opened at a snapshot line** (`#snap-12`, the activity page's way back; 59bb72fd): the lines are drawn only once the review is read, after the navigation, so the page takes the focus at that line itself.

## Focus clearance and stacking (DESIGN.md § Stacking)

The page's focus targets (its `h2`/`h3`/`h4` with `tabindex`, the result region, `tr[id^=snap-]`, `li[id^=snap-]`) carry `scroll-margin-top: calc(56px + var(--sitting-bar-h, 0px) + 16px)`, and from 641px `html` reserves `scroll-padding-top: calc(56px + var(--sitting-bar-h, 0px))`, so nothing that takes the focus lands under the top bar or the chair's stuck bar (WCAG 2.4.11). The sidebar is `z-index: 10` (MainLayout), the chair's bar 1, dialogs in the top layer.

## Narrow widths

- **Below 641px** every bar is static and stacked (SittingBar), and every target is 44px: the progress link, the bar's buttons, a line's Stage and Remove…, a tick's whole row (`.form-check` and its label at 2.75rem, the box 1.25rem), Read, "Show the other N snapshot lines", the reader's Back, "Rating chart for …" and "Back to …'s line", Record decision… (the row's width), the staging form's and the appeal's buttons, and a snapshot line's item link (`#evidence-snapshot tbody th a`, 6b149fff: it was a 21px target).
- **The agenda stacks under 760px of its own width** (a container query, not the viewport: AgendaTable).
- **The snapshot's tables** are `clinic-table--stack`: below 641px each line a block, the item heading it.

## Rules (DESIGN.md § Committee agenda, § The chair's controls, § Detail page)

- **No deferral anywhere** (D53): every line left unstaged once the decision is recorded reads "Not decided at this sitting", with the decision's one optional sentence.
- **Each control is offered to exactly the people its handler lets use it**, in the states it takes it (`CallerChairs`, `CallerMayStart`, `CallerResolvesAppeals`, `TraineeElsewhere`, `ChairCannotAct`); a reader reads who may act, never a control that would be refused.
- **No live following** (T362): a reader keeps up by Refresh.
- **No pronoun for a person** (round-3-check 3): names, "the chair", "the registrar", "the trainee". Dates ISO; times "SAST".

## Contrast

No new colour token; every pair is one the Spec measured: text 12.63:1, muted 5.09:1, links 6.70:1, the action blue 4.86:1, the input border 3.80:1 (the reader's bar's stripe); the badges as Badge's.

## Known gaps

- **What the page has no data for** (the T361 handoff): the ratified graduation summary; a chair's line's earlier STAR; a decided line's level and expiry on the agenda; who can start when the chair cannot act.
- Readers follow the sitting only by Refresh (T362 proposes live following).
- The appeals section is the pre-restructure form (flow 09's): `EditForm` with DataAnnotations, its Present ticks like the decision's.
