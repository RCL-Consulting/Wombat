# F06 Watching the programme: who is behind, and what has stalled

**Who and why:** the committee, the programme director, the sub-speciality lead and the coordinator look each week at
who is behind this period's targets and which requests have waited too long for an assessor. It is the programme's
early warning, caught before the committee sits (flow 07). Today it is four Home cards that link nowhere but one, no
page of the programme's registrars, no page of stalled work, and no way to chase a request but a nightly mail.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **W** in BRIEF.md § 8: the four oversight Homes and the pages that were "Coming soon" (B7) are expected to change a lot, so round 1 asks for 2–3 structural variations. |
| Mode | **Structure first** (BRIEF.md § 2.3, as § 11 corrected it). Round 1: 2–3 structural variations as wireframes of the four Homes and of whatever pages each variation adds (a programme roster, a list of what waits for a reviewer, a staff view of one registrar), each with its reasoning and its step counts. Round 2, after the pick: fidelity with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844 |
| Variations | 2–3 in round 1; the chosen one in round 2 |
| People | CommitteeMember: Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha (each also an Assessor), Dr John van Rensburg (external, CommitteeMember only). SpecialityAdmin: Dr Refilwe Mokoena. SubSpecialityAdmin: Dr Kabelo Sithole. Coordinator: Mr Pieter Smit (`README.md` § Cast). The registrars they watch: Dr Molefe (graduates in Act 5), Dr Dlamini, Dr du Plessis (withdraws in Act 5), Dr Mahlangu, Dr Ndlovu (erased in the appendix) |
| Runbook steps | 20, pasted verbatim in § 8. Steps 3.32 and A.2.8 are the Administrator running the reminder jobs on `/admin/jobs` (flow 18's page), seen here for what they chase. Step 3.33's committee Home carries flow 04's other-role line. |
| Pages (`coverage.md` templates) | `/` (the four role dashboards: `Components/Pages/Dashboards/CommitteeMemberDashboard.razor`, `SpecialityAdminDashboard.razor`, `SubSpecialityAdminDashboard.razor`, `CoordinatorDashboard.razor`, inside `Home.razor` and `DashboardFrame`), `/dashboard/switch/{role}`, `/not-found` (the deleted placeholders' addresses), `/committee/panels` (the Coordinator's, reached only by address; flow 08's page), `/admin/entrustment-decisions` (the speciality admins', reached only by address; flow 09's page). The steps also pass through `/activities/{ActivityId:int}` (a stalled row's link; flow 03's page), `/committee/decisions-due` and `/committee/reviews` (flow 08's), `/committee/reviews/{ReviewId:int}` (flow 07's), `/msf/campaigns` (flow 10's), `/admin/data-rights` (flow 14's) and `/admin/jobs` (flow 18's). Checked at `ef65c336`. |
| Menus (`NavItems.cs`) | CommitteeMember (`:151`): Home, Committee reviews, Decision panels. SpecialityAdmin and SubSpecialityAdmin (`:149–150`): Home, Decisions due, Committee reviews, Decision panels. Coordinator (`:154`): Home, Decisions due, MSF campaigns, Committee reviews, Data rights requests. Assessor (`:155`): Home, Activity inbox. Then My data rights under the rule (`:189–192`). DESIGN.md's nav table matches (`DESIGN.md:520–532`). |
| Components drawn at runtime | `RoleDashboard<T>` (`Components/Shared/RoleDashboard.cs`) in `DashboardFrame`; `DashboardCard`; `Shared/EpaTargetCoverageList.razor` (the committee's and the admins' per-EPA card); `.list-row` (the Coordinator's rows); `.dashboard-metric` (the admins' counts); `Shared/OtherRoleLine.razor` (flow 04's line, above every dashboard of an Assessor holder); `ActingRoleSwitchAlert`. Queries: `Application/Features/Dashboards/{CommitteeMember,SpecialityAdmin,SubSpecialityAdmin,Coordinator}/Get…DashboardSummaryQuery.cs`, `Features/Curricula/Quota/CurriculumCoverage.cs`, `Features/Activities/Services/ActivityWaiting.cs` |
| Held | None: T297 landed (`done/`), and every capture § 4 names was re-taken by the T355 replay on 2026-10-04, on the shell as built |
| Depends on | Flow 01, built (`b347e11c`): the shell, Home's frame, the acting role and its switch, `DashboardFrame`'s loading and load error, Not found. Flow 04, built (`06aa51d7`): the list vocabulary (rows named per row, the count as words, "Waiting N days", the Overdue badge, `WaitingList`), the other-role line. Flow 05, built (`b020c942`): the progress words ("n of m" over its caption, `ProgressWords`), ISO dates on screens (D1), an EPA's name as its own link. The design system is **version 15**, re-synced to flow 05 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 06`. Open each screenshot. Leave out anything in
   `design/upload/crop-first/`, or crop its link first. Flow 05's boards (§ 4) are not staged by the script.
2. **Start a new canvas** from the main app's Design page, on the Wombat design system v15
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not an earlier flow's canvas. Flow 06's canvas: https://claude.ai/artifact/97WQWEuzMYHcKngPnNdHkV (created 2026-10-05).
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`). Put the returned `/_blob/` URL in § 1 and in
   `design/flows/06-programme-oversight/round-1-ask.txt`, where they read `/_blob/d0ffe547ebb511d3f58145477434f9aa`.
4. **Send § 1 and § 8 as ONE message** (`design/flows/06-programme-oversight/round-1-ask.txt`), with the key
   screenshots in § 4. The canvas draws round 1 from the first message and does not wait for a second (§ 11, flow 02).
   Read `project/canvas.json`'s version before and after: a send can draw nothing.
5. **Pick one structure,** say why in a sentence, and answer § 6's questions one sentence each; questions 8–10 are the
   operator's. A question whose answer rests on an open task (T290, T298) names it, and the task lands before the
   build (§ 11, flow 05). Ask for round 2.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/05-trainee-progress/round-2-review.md`
   did: one reviewer on the code, one on the cast; the reviewer on the code checks the ask too (§ 11, flow 03). Put the
   decisions to the operator, then ask for one correction round.
7. **Record every round** in `design/flows/06-programme-oversight/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 06 — Watching the programme: who is behind, and what has stalled

This is flow 06 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox) and 05 (a registrar reads where she stands)
  are designed and built; these pages sit inside the shell. Structure first, then fidelity: round 1 is 2–3 structural
  variations as wireframes of the four oversight Homes and of the pages each variation adds, and I pick one before
  round 2. The runbook steps follow below in this same message. The brand mark is at /_blob/d0ffe547ebb511d3f58145477434f9aa: use it, never a drawn
  disc.

GOAL: Programme staff open Wombat once a week and learn, in under a minute and on a phone if need be:
  - which registrars are behind this period's targets, by how much, and on which EPAs;
  - which EPAs the programme as a whole is short on;
  - which requests have waited too long for an assessor, whose they are, and how to chase them;
  - from any of these, the one registrar or the one request behind the figure.
  Nothing else warns the programme before the committee sits; a registrar found behind there is found too late.
  What is wrong today (the attached screenshots show each; all on the shell as built, 2026-10-04):
  - No card on any of the four Homes leads anywhere, except the Coordinator's stalled rows. A committee member reads
    "semester 0/10 · yearly 0/5" beside each registrar and cannot open that registrar: no staff page shows one
    registrar's targets or standing outside a committee review (flow 07), and My progress admits only the registrar.
  - The registrar rows say "semester 1/10 · yearly 0/5" and the admins' tile "5 active / 0 inactive": slashes, where
    every other figure in Wombat is "n of m" for a named window ("1 of 10 EPAs met this semester", flow 05). The
    semester and its months are said twice on the committee Home, once per card, and again in Home's subtitle.
  - The per-EPA card is one component with two titles: "Targets met by EPA" (committee) and "Curriculum coverage —
    Semester 2, 2026" (admins). Its rows link nowhere. Its figure sits beside a two-line EPA title, so "0 of 5 met"
    wraps to two lines at 1280 and "0 of 2 met" to three at 390; registrar names wrap the same way ("Pieter du /
    Plessis") in the committee's narrow card.
  - Exemption is worded twice: " · 1 exempt" on one card, " · 1 trainee exempt this period" on the other.
  - Registrars with equal figures are ordered by an internal id, not by name (T298).
  - The external member, Dr van Rensburg, reads "No trainees have targets this period." and "No curriculum targets for
    these trainees.", though he sits on the panel that judges all five: the card lists only the registrars of the
    member's own sub-specialities, and he holds none (T290).
  - The speciality admins' "Pending reviews: 2 activities awaiting review" links nowhere: no page lists what it counts.
  - The Coordinator's "Stalled requests" lists what waits for a reviewer and has not moved for 7 days, oldest first,
    each "Mini-CEX (Paediatrics) — Nomsa Mahlangu" and the day it last moved, "26 Sept" (a day-month form found nowhere
    else since flow 05's ISO dates, wrapping to two lines). It never says how long a request has waited, though the
    assessor's own Home says "Waiting 8 days". Each link is named by instrument and trainee only, so two requests of
    one type from one registrar share a name. It stops at 10 rows and says nothing of the rest.
  - Nothing chases a stalled request but the nightly mail to its assessor after 5 days (Step 3.32); the card waits 7.
    No page sends a reminder now, and nobody may change a request's named assessor: the registrar can only cancel it
    and file again.
  - "Invitations nearing expiry" prints the role's internal key, "(Trainee)" today and "(CommitteeMember)" for
    another, and the day as "expires 29 Sept".
  - The three pages the menus once linked as "Coming soon" were deleted with their nav items (flow 01): Programme
    trainees (committee, both speciality admins), Stalled activities (Coordinator) and STAR review queue (both
    speciality admins). Each address is Page not found.
  - Two pages admit a role whose menu has no item for them: Decision panels admits the Coordinator, and Entrustment
    decisions (the programme's register of STARs, flow 09's page) admits both speciality admins, who reach it only by
    typing its address; its trail then reads Home › Entrustment decisions with nothing lit.
  - A weekly mail already sends the Coordinator a roster no page shows: the registrars with nothing logged in 30 days,
    MSF campaigns awaiting review, this week's reviews (WeeklyCoordinatorDigestJob).

AUDIENCE:
  - CommitteeMember: Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha: senior paediatric consultants who also
    assess, so their sessions open as Committee member with "Switch to Assessor", and flow 04's line ("1 activity waits
    for you in the Activity inbox …", with Open it) above their committee cards when work waits. Dr John van Rensburg:
    external examiner from another university, Committee member only.
  - SpecialityAdmin: Dr Refilwe Mokoena (programme director). SubSpecialityAdmin: Dr Kabelo Sithole (sub-speciality
    training lead). They run the programme, form panels and schedule reviews.
  - Coordinator: Mr Pieter Smit (programme administrator, not a clinician). He runs feedback campaigns, chases stalled
    work, schedules reviews and keeps the data-rights queue.
  - Kgosi Kgari Teaching Hospital (KGK), Paediatrics, on the College's (CPSA) curriculum 11.1: five registrars, Dr
    Lerato Molefe (training year 4), Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr Nomsa Mahlangu and Dr Sipho
    Ndlovu (1). By the story's end two programmes have ended (Molefe graduated, du Plessis withdrew) and one account
    is erased (Ndlovu): two current registrars. Desktop 1280×800 and phone 390×844.

WHAT THE HOMES CARRY TODAY (real figures; they are the data to draw with):
  - Every Home: h1 "Home", subtitle "<Role> · Semester 2, 2026", no header action for these four roles; the sidebar's
    "Acting as <Role>", with "Switch to <Role>" for a holder of two; after a switch an info alert "You are now acting as
    Committee member."; loading and load error drawn by the frame (flow 01's: "Could not load your Home. Nothing has
    changed. Try again, or come back in a few minutes." with Try again, no cards).
  - Committee member (Step 3.52, Dr Zulu):
    - "Targets this period": "Semester 2, 2026 · July to November", then each current registrar of the member's
      sub-specialities, fewest met first: Pieter du Plessis, Nomsa Mahlangu, Sipho Ndlovu "semester 0/10 · yearly 0/5";
      Anele Dlamini, Lerato Molefe "semester 1/10 · yearly 0/5". Empty: "No trainees have targets this period."
    - "Targets met by EPA": the same semester line, then PAED-001 to PAED-015, each "PAED-001 — Providing paediatric
      emergency care to children (3 per semester)", "2 of 5 met" and a bar. Empty: "No curriculum targets for these
      trainees." After Act 6 KGK-001, the institution's own EPA, leads the list: "KGK-001 — Running a paediatric
      outreach clinic at a district hospital (1 per academic year)".
  - Speciality admin and Sub-speciality admin (Step 3.53; the two are identical, scoped to the speciality or the
    sub-speciality):
    - "Pending reviews": "2" over "activities awaiting review" ("activity" for one); no link.
    - "Trainees in programme": "5" over "active / 0 inactive"; by the story's end "2 active / 3 inactive" (every ended
      programme and the erased account count as inactive).
    - "Curriculum coverage — Semester 2, 2026": the same list as the committee's "Targets met by EPA".
  - Coordinator (Step 3.30, Mr Smit):
    - "Stalled requests", a warning card when it has rows: "Portfolio and Logbook Review (Paediatrics) — Pieter du
      Plessis · 26 Sept", "Mini-CEX (Paediatrics) — Nomsa Mahlangu · 26 Sept", the type a link to the activity.
      Empty: "No stalled requests."
    - "Invitations nearing expiry", three days ahead: "expiring@kgk.wombat.local (Trainee) · expires 29 Sept". Empty:
      "No invitations expiring soon."
    - "Quick action": the button "Start an MSF campaign".
  - Data volumes: none (Step 2.32 and 2.33: no stalled request; five registrars all at 0); typical (Step 3.52: five
    registrars, PAED-001 "2 of 5 met"); heavy (16 EPAs with KGK-001; more than 10 stalled requests; a registrar exempt
    this period; an EPA title of about 120 characters, "PAED-015 — Teaching and applying evidence-based care
    responsibly and ethically in clinical decision-making and research"); the story's end (two current registrars,
    three ended or erased).

THREE DIFFERENT "PER"S THE DESIGN MUST KEEP APART (as flow 05 kept them):
  - Observation targets, per window (Annexure B): ten EPAs have a target per semester, five per academic year.
    Semester 2 is July to November; December counts into it.
  - Decision cadence: when the committee decides the EPA (each semester for six, each academic year for the rest).
  - Level targets, per training year (Annexure A), and the exit level.
  A registrar's figure is "n of m" for a named window ("1 of 10 EPAs met this semester", "0 of 5 EPAs met in 2026");
  a programme's is "n of m registrars" for a named window ("2 of 5 registrars met PAED-001's target this semester").
  Never a percentage, never "n / m", never the bare word "year" beside both a training year and an academic year.

SCREENS, in order (today's names; a variation may merge, split, rename or drop them):
  1. Home for the Committee member (/): who is behind, which EPAs the programme is short on; with flow 04's line above
     it for an Assessor holder; the external member's view.
  2. Home for the Speciality admin and the Sub-speciality admin (/): the same question for the programme they run,
     and what waits for a reviewer in it.
  3. Home for the Coordinator (/): what has stalled and how to chase it; invitations nearing expiry; his one quick
     action.
  4. Programme trainees (PROPOSED, a new page; today a deleted stub): the programme's registrars, each with training
     year, this period's semester and yearly figures, and shortfalls; or no page.
  5. One registrar, as staff read her (PROPOSED, a new page or a reuse): her targets, standing and evidence, wherever a
     roster row or a Home row leads; or say why a row leads to an existing page (a committee review, flow 07's) or
     nowhere.
  6. What waits for a reviewer (PROPOSED, a new page; today a deleted stub, "Stalled activities"): every request
     waiting for its assessor in the reader's scope, how long it has waited, with stalled ones marked; the target of
     the admins' "Pending reviews" and the Coordinator's "Stalled requests"; triage (remind now, reassign) as PROPOSED
     actions; or no page.
  7. Entrustment decisions in the speciality admins' menu (today reached by address; flow 09 designs the page's body;
     this flow decides only whether the menus link it, and under what label, replacing "STAR review queue").

WHAT FLOW 06 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell and Home's frame (flow 01: "Home", the role and semester under it, header actions, the frame's
    loading and load error, the acting-role switch and its alert); the other-role line's words, place and Open it
    (flow 04); flow 04's list vocabulary (rows named per row, counts as words, "Waiting N days", the Overdue badge);
    flow 05's progress words ("n of m" over its caption, ISO dates, an EPA's name as its own link) and its pages, which
    admit only the registrar; the bodies of Decisions due and Decision panels (flow 08's), of a committee review (flow
    07's) and of Entrustment decisions (flow 09's).
  May, for these four roles: every card on their Homes, its title, order, rows and links; EpaTargetCoverageList's one
    title and row shape (it draws both the committee's and the admins' card); the menus of these four roles (a new
    item, a renamed one; each item must admit every role whose menu offers it); new pages, each with its owning list.
  Say which of these each variation changes, and why.

STEPS: 2.32, 2.33, 2.35, 2.37, 2.38, 3.30, 3.31, 3.32, 3.33, 3.52, 3.53, 3.54, A.2.8, A.5.10, A.5.11, A.5.12, A.7.5,
  A.7.6, A.7.7, A.7.8. Pasted verbatim below (Role / Route / Do / Expect). In them, D is the day the story is replayed.
  Where an Expect quotes today's wording, that is today's page, not a requirement on the new one.

STATES TO SHOW:
  Each Home: empty (nobody to show; nothing stalled); typical; heavy (16 EPAs; more than 10 stalled; an exempt
    registrar; the 120-character title); the story's end (two current registrars, three ended); loading; load error
    (the frame's); 390 px.
  Committee member: a holder of two roles, before and after the switch, and with flow 04's line above the cards (one
    waiting; one overdue); the external member, who after T290 sees the institution's current registrars (the empty
    card remains only for an institution with none).
  Coordinator: invitations nearing expiry with rows, the role by its label.
  Each new page, if designed: empty, typical, heavy, a filter with no match ("no match" is not "empty"), loading,
    load error (a sentence and Try again, never an exception's text), 390 px, and a row out of the reader's scope
    (not shown; a typed address to it is "not found").
  Triage, if designed (PROPOSED): the action, its confirmation, its result alert with focus moved to it, and its
    refusal (the assessor has no email address; the request moved meanwhile).

REQUIREMENTS FROM KNOWN DEFECTS:
  - T290: a committee member with no sub-speciality (the external member) sees the institution's current registrars.
  - T298: registrars tied on a figure are ordered by surname, then first name. Fewest met stays first.
  - T280: every link on these Homes is named by more than its instrument, so no two share a name (flow 03's "Type ·
    EPA · date"); a card is not one link around other links.
  - T325: every date is South African, in one form for people to read (ISO, flow 05's D1), and "today" is South
    African.
  - T351: "waiting N days" counts from the request's last move, which any save resets today; draw the figure as
    flow 04 draws it, and do not promise more precision than that.
  - T328: badges keep their pill beside a wrapping row; a scrollable region shows the design's focus ring; every
    target ≥ 24 px, 44 px on a phone.
  - B7 and B8 (BRIEF.md § 7): each deleted page is designed or left out, and every page these roles may open is
    linked from their menu or a card, or the brief says why not (the Coordinator's Decision panels; the speciality
    admins' Entrustment decisions).
  - The progress figures rule (DESIGN.md, flow 05): "n of m" for a named window; never "n / m"; one wording for
    exemption.
  - Content (BRIEF.md § 6): sentence case; states, types and roles by their labels ("Trainee", never a key); people by
    name as stored, never an address where a name exists; no system words.
  - Done since the old brief, keep them: Stalled requests lists everything waiting for a reviewer (requested ones
    included), oldest first, each row linking to its activity and naming the trainee (T297); "Pending reviews" counts
    in the singular for one and does not open the admin's own empty inbox (T297); badge and alert contrast (T322);
    the placeholders are gone and their addresses are Page not found (T335).

QUESTIONS THE DESIGN MUST ANSWER:
  1. Programme trainees: a page (who it admits: the committee, both speciality admins, the Coordinator?), a fuller
     Home card, or nothing? Does the committee's "Targets this period" become its preview?
  2. One registrar, as staff read her: a new staff page (her targets, standing against Annexure A, rating trajectory,
     evidence), the committee review page (which exists only once a review is scheduled), or no link from a roster row?
     Flow 05's EntrustmentStandingPanel and TrajectoryChart already draw for staff on the review page.
  3. What waits for a reviewer: one page serving both the admins' "Pending reviews" and the Coordinator's "Stalled
     requests" (all waiting, stalled marked), two pages, or Home cards alone? What is "stalled": 7 days, the nudge's 5,
     or flow 04's Overdue?
  4. Triage: is "Send a reminder now" in scope, and "Reassign to another assessor"? Neither is built. Today nobody may
     change a request's named assessor once filed (the request section is the registrar's, and the assessor's field is
     locked while it waits). A reassignment changes the activity platform's rules, not only a page; who may do it is
     undecided. Mark each PROPOSED.
  5. The per-EPA card: one title for both Homes; do its rows link (to what, given My progress admits only the
     registrar)? Where does the figure sit at 390 px?
  6. The Coordinator's Home: is "Quick action" a card, a header action (flow 01 gives Home one header action per role
     at most), or gone? Does the weekly digest's roster (nothing logged in 30 days) belong on a page?
  7. Entrustment decisions: a menu item for both speciality admins, under that label (replacing "STAR review queue";
     "STAR" is the Statement of Awarded Responsibility)? Decision panels: a menu item for the Coordinator?
  8. (The operator's) T290 and T298 are open backend tasks the design's figures depend on. Land them first, as their
     own task, before the build (flow 05's T304), or fold them into the build?
  9. (The operator's) If triage is chosen, build it in this flow, or design it here and file it as its own task?
  10. (The operator's) What does "inactive" count: ended programmes, erased accounts, or neither? Today every profile
     not active counts, so a programme's graduates accumulate there.

CONSTRAINTS:
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
  Since flow 03 also: ActivityStatus, ActivityAbout, ActivityForm, ActivityWorkflowActions, SubmitCheck,
  RefusalSummary, ActivityHistory, NeedsYouList, InstrumentPicker, and DataTable's stacked rows (Stack). Since flow 04:
  the rung picker, WaitingList, WayOn, the Overdue badge and the other-role line. Since flow 05: the progress index
  (an EPA per row, its name its own link), the figure over its caption, EntrustmentStandingPanel's stacked rows, and
  TrajectoryChart's two drawings (wide and 322 px) with its visible table.
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

ASK:
  - Round 1: 2–3 STRUCTURAL variations as wireframes of the four Homes and of the pages each variation adds, keeps or
    drops (screens 4–7). Each covers the pages and their order (or what replaces them), the menus of the four roles,
    the steps of each journey (the committee's weekly look from sign-in to the registrar furthest behind; the
    programme director's from her pending count to the oldest waiting request; the Coordinator's from a stalled
    request to chasing it; the external member's first look; each on a phone), and where each figure, mark and failure
    shows. Give each its reasoning and its step count per journey, at 1280 and 390 px. No colour or type choices yet.
    Stop and wait for my pick.
  - Round 2: the chosen structure at full fidelity on the design system's tokens, every screen and state above, at
    1280 and 390 px.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (DESIGN.md § Dashboard page and its dashboard link rule; § Dashboard layout grid and its progress figures
    rule; § The NavMenu and its nav table; the owner table, where Entrustment decisions has no owning list yet).
  - Mark PROPOSED every action that is not built (a reminder, a reassignment) and every page that does not exist
    (Programme trainees, a staff view of one registrar, the waiting list).
  - Flag edge cases: nobody to show; every target met; a registrar exempt this period; the 120-character title; 16
    EPAs; more than 10 stalled; a request waiting on an assessor who has been deactivated; a registrar whose programme
    ended this week; a person holding two of these roles.
  - Run an accessibility review against WCAG 2.1 AA: every link named for its registrar, EPA or activity; figures read
    as words, never a bare bar; targets ≥ 24 px (44 at 390); focus after Try again and after any triage action.

ATTACHED: act-3/3.52-1-zulu-committee-home.png, states/home--committee-line-one.png, act-2/2.37-1-vanrensburg-home.png,
  act-3/3.53-1-mokoena-home.png, act-3/3.30-1-smit-stalled-requests.png, states/home--coordinator-expiring.png,
  act-A/A.7.6-1-home-390.png, act-A/A.7.7-1-home-390.png, act-A/A.7.5-1-home-390.png,
  act-A/A.7.7-5-entrustment-decisions-390.png, act-3/3.31-1-stalled-activities-404.png; flow 05's boards, for what one registrar
  reads today: R3-E-typical (one EPA's page), R3-P-standing (My progress with STARs) and R3-T-committee (the trajectory
  as the review page draws it), each at 1280 and 390, with flow05-r3.css.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 2.32 | `/account/login` → `/` → `/admin/invitations` → `/access-denied` → `/committee/panels` → `/committee/panels/new` → `/access-denied` | Mr Smit, first sign-in | "No stalled requests.", "No invitations expiring soon.", Start an MSF campaign; a menu with no Decision panels, though `/committee/panels` admits him (B8) |
| 2.33 | `/account/login` → `/` | Dr Zulu | five registrars at "semester 0/10 · yearly 0/5", every EPA "0 of 5 met"; "Switch to Assessor" |
| 2.35 | `/account/login` → `/` → `/committee/panels` | Dr Naidoo, Dr Botha | the same Home; the panel they sit on (flow 08's page) |
| 2.37 | `/account/login` → `/` → `/committee/panels` | Dr van Rensburg | "No trainees have targets this period." (T290): he should see the institution's registrars |
| 2.38 | `/` | Dr Mokoena, Dr Sithole | Pending reviews 0 with no link, "5 active / 0 inactive", coverage "0 of 5 met" per EPA |
| 3.30 | `/` | Mr Smit | both stalled requests, oldest first, each linked and naming the trainee, "26 Sept"; no "waiting N days" |
| 3.31 | `/` → `/not-found` | Mr Smit | no page of stalled requests in his menu; the old stub's address is Page not found (404) |
| 3.32 | `/admin/jobs` | devadmin (flow 18) | the nightly reminder: "Hi Thandi" and "Hi Mohammed", each request waiting 8 days; the only chase today |
| 3.33 | `/` → `/dashboard/switch/{role}` → `/` → `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Zulu | flow 04's line above Targets this period ("… it is overdue …", Open it); after she completes it, Mr Smit's card lists only the portfolio review |
| 3.52 | `/` → `/dashboard/switch/{role}` → `/` | Dr Zulu | back to Committee member; ties ordered by an internal id (T298); PAED-001 "2 of 5 met"; no link on either card; no Programme trainees |
| 3.53 | `/` | Dr Mokoena | "2 activities awaiting review", no link; no Entrustment decisions or Programme trainees in her menu |
| 3.54 | `/` | Dr Sithole | the same three cards and figures, the same menu |
| A.2.8 | `/admin/jobs` | devadmin (flow 18) | the draft reminder (15 days) and the assessor reminder (6 days) each log whom they reminded and skipped |
| A.5.10 | `/` | Mr Smit | the portfolio review still stalled, its row linked; no stalled-work page |
| A.5.11 | `/` | Dr Botha | her menu offers no Programme trainees |
| A.5.12 | `/` | Dr Mokoena | her menu offers neither Programme trainees nor STAR review queue |
| A.7.5 | `/` → `/msf/campaigns` → `/msf/campaigns/{CampaignId:int}` → `/committee/decisions-due` → `/admin/data-rights` | Mr Smit at 390 px | Home's three cards stacked, the stalled row's date wrapping; his other pages are flows 08's, 10's and 14's |
| A.7.6 | `/` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu at 390 px | Home's two cards stacked, two current registrars, "0 of 2 met" wrapping to three lines; the review page is flow 07's |
| A.7.7 | `/` → `/committee/panels` → `/committee/decisions-due` → `/admin/entrustment-decisions` | Dr Mokoena at 390 px | "3 activities awaiting review", "2 active / 3 inactive", the coverage stacked with every figure on three lines; Entrustment decisions by address only, trail Home › Entrustment decisions |
| A.7.8 | `/` → `/committee/reviews` → `/committee/decisions-due` | Dr Sithole at 390 px | as Dr Mokoena's; the bar's "Acting as" over "Sub-speciality admin" on two lines |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Home and the role dashboards. The
step captures were re-taken by the T355 replay on 2026-10-04 and the `states/` ones by the T335 states capture on
2026-09-27, all on the shell as built; the cards have not changed since T297. Where a state has both, the step capture
is the one to brief from.

| Page | State | Screenshot | What it shows |
|---|---|---|---|
| Home (Committee member) | Two roles, all zeros | `act-2/2.33-1-zulu-committee-home.png` (`states/home--committee-member.png`) | five registrars at 0; "Switch to Assessor" |
| Home (Committee member) | With figures, after the switch | `act-3/3.52-1-zulu-committee-home.png` (`states/home--committee-member-figures.png`) | the switch alert; names wrapping; "0 of 5 met" wrapping beside two-line titles |
| Home (Committee member) | Flow 04's line above the cards | `states/home--committee-line-one.png`, `--committee-line-overdue.png`, `--committee-line-several.png` | the info or warning line with Open it, then the committee cards (flow 04's line; this flow's cards) |
| Home (Committee member) | Nobody to show (T290) | `act-2/2.37-1-vanrensburg-home.png` (`states/home--committee-member-empty.png`) | both cards empty for the external member |
| Home (Committee member) | Narrow, the story's end | `act-A/A.7.6-1-home-390.png` (`states/home--narrow-committee-member.png`) | two registrars; KGK-001 first; figures on three lines |
| Home (Speciality admin) | All zeros | `act-2/2.38-1-mokoena-home.png` (`states/home--speciality-admin.png`) | Pending reviews 0, "5 active / 0 inactive", coverage at 0 |
| Home (Speciality admin) | With figures | `act-3/3.53-1-mokoena-home.png` (`states/home--speciality-admin-figures.png`) | "2 activities awaiting review", no link |
| Home (Sub-speciality admin) | With figures | `act-3/3.54-1-sithole-home.png` (`states/home--sub-speciality-admin.png`) | the same as Dr Mokoena's |
| Home (Speciality admin) | Narrow, the story's end | `act-A/A.7.7-1-home-390.png` (`states/home--narrow-speciality-admin.png`) | "3 activities awaiting review", "2 active / 3 inactive", 16 EPAs, each figure on three lines |
| Home (Sub-speciality admin) | Narrow | `act-A/A.7.8-1-home-390.png` (`states/home--narrow-sub-speciality-admin.png`) | as Dr Mokoena's |
| Home (Coordinator) | Nothing waiting | `act-2/2.32-1-smit-home.png` (`states/home--coordinator-empty.png`) | "No stalled requests.", "No invitations expiring soon.", Start an MSF campaign |
| Home (Coordinator) | Stalled requests | `act-3/3.30-1-smit-stalled-requests.png` (`states/home--coordinator-stalled.png`) | two linked rows, "26 Sept" wrapping |
| Home (Coordinator) | One stalled | `act-A/A.5.10-1-stalled-requests-card.png` | the portfolio review alone |
| Home (Coordinator) | Invitations nearing expiry | `states/home--coordinator-expiring.png` | "expiring@kgk.wombat.local (Trainee)", "expires 29 Sept" |
| Home (Coordinator) | Narrow | `act-A/A.7.5-1-home-390.png` (`states/home--narrow-coordinator.png`) | three cards stacked |
| Home (any) | Loading, load error | `states/home--loading.png`, `states/home--load-error.png` | the frame's skeleton; its alert and Try again (flow 01's) |
| The deleted pages | Page not found | `act-3/3.31-1-stalled-activities-404.png` | `/placeholder/stalled-activities`, 404 |
| Entrustment decisions | By address, narrow | `act-A/A.7.7-5-entrustment-decisions-390.png` | trail one Home link, nothing lit; the table scrolling in its box (flow 09's page) |
| Menus | Committee member, Speciality admin | `act-A/A.5.11-1-botha-menu.png`, `act-A/A.5.12-1-mokoena-menu.png` | no Programme trainees, no Entrustment decisions |

Not captured, so describe them in words (`states.md`; BRIEF § 10):
- **More than 10 stalled requests.** The card stops at 10 and says nothing (`GetCoordinatorDashboardSummaryQuery.cs:21`,
  `:78`); no cast member reaches more than two.
- **A registrar exempt this period** (" · 1 exempt" on the committee card, `CommitteeMemberDashboard.razor:13`;
  " · 1 trainee exempt this period" on the coverage list, `EpaTargetCoverageList.razor:14`): every start is 15 January
  (D42), so no cast member is exempt.
- **"all exempt" on an EPA row** (`EpaTargetCoverageList.razor:20–35`).
- **A request waiting on a deactivated assessor:** the nudge skips such a nominee and says so in its summary (D50);
  nothing on a Home says it.
- The new pages, if designed: none exists, so every state is drawn from the description.

**Belongs to other flows:** the Assessor's Home after a switch (`home--assessor-*`) and the other-role line's words are
flow 04's; the InstitutionalAdmin's Home is flow 12's and the Administrator's flow 18's; Decisions due and Decision
panels are flow 08's; the review page is flow 07's; Entrustment decisions' body is flow 09's; the reminder mails are
flow 19's.

## 4. Attach

Paths are relative to `design/baseline/`. Every key screenshot below was opened and checked on 2026-10-04.

**Key screenshots (attach these first, with § 1):**
1. `act-3/3.52-1-zulu-committee-home.png`: the committee Home after the switch: "semester 0/10 · yearly 0/5", names
   wrapping, every EPA row's figure beside a two-line title, no link anywhere
2. `states/home--committee-line-one.png`: flow 04's line above the committee cards (Dr Naidoo)
3. `act-2/2.37-1-vanrensburg-home.png`: the external member's empty cards (T290)
4. `act-3/3.53-1-mokoena-home.png`: the speciality admin's three cards: "2 activities awaiting review" with no link,
   "5 active / 0 inactive", "Curriculum coverage — Semester 2, 2026"
5. `act-3/3.30-1-smit-stalled-requests.png`: the Coordinator's two stalled rows, "26 Sept" wrapping
6. `states/home--coordinator-expiring.png`: an invitation nearing expiry, "(Trainee)", "expires 29 Sept"
7. `act-A/A.7.6-1-home-390.png`: the committee Home at 390 at the story's end, two registrars, KGK-001 first
8. `act-A/A.7.7-1-home-390.png`: the speciality admin's Home at 390, 16 EPAs, every figure on three lines
9. `act-A/A.7.5-1-home-390.png`: the Coordinator's Home at 390
10. `act-A/A.7.7-5-entrustment-decisions-390.png`: Entrustment decisions reached by address
11. `act-3/3.31-1-stalled-activities-404.png`: a deleted stub's address, Page not found

Also attach **flow 05's boards** for what one registrar reads today, from
`design/flows/05-trainee-progress/round-3/project/`: `R3-E-typical-1280.dc.html` and `-390` (one EPA's page, Dr Dlamini's
PAED-001), `R3-P-standing-1280.dc.html` and `-390` (My progress with STARs, the Entrustment panel), and
`R3-T-committee-1280.dc.html` and `-390` (the trajectory over a review window, as the review page draws it), with
`flow05-r3.css`, which they load beside the design system's bundle. Stage them by hand in `design/upload/flow05-boards/`; the
staging script does not stage them.

**States (attach as the chat asks):** every row of § 3 not already attached above, and
`act-2/2.38-2-sithole-home.png`, `act-A/A.5.10-1-stalled-requests-card.png`, `act-A/A.7.8-1-home-390.png`,
`act-A/A.7.7-2-menu-open-390.png`, `act-A/A.7.5-4-decisions-due-390.png` (a focusable scroll region with the design's
ring, flow 08's page).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `ef65c336`;
`W/` is `src/Wombat.Web/`, `A/` is `src/Wombat.Application/`.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T290** (P3; the committee item Medium) | The committee card takes profiles whose curriculum's sub-speciality is in the member's claims (`A/Features/Dashboards/CommitteeMember/GetCommitteeMemberDashboardSummaryQuery.cs:34`, `:53–57`); the external member holds none, so both cards are empty (`W/Components/Pages/Dashboards/CommitteeMemberDashboard.razor:16`). Design the cards for the institution's current registrars; "nobody to show" only for an institution with none. Backend: question 8. | `act-2/2.37-1-vanrensburg-home.png`; Step 2.37 |
| **T298** (P3) | Ties are ordered by share met, then `TraineeUserId`, a GUID (`A/Features/Curricula/Quota/CurriculumCoverage.cs:241–244`); the committee query keeps that order (`GetCommitteeMemberDashboardSummaryQuery.cs:70–81`). Fewest met first, then surname and first name. No test pins a tie (`CurriculumCoverageTests.cs:213`). T298's other half, Upcoming deadlines, went with flow 05. | `act-3/3.52-1-zulu-committee-home.png` |
| **T280** (P3) | Each stalled row's link is the type, its accessible name "{Type} for {Trainee}" (`CoordinatorDashboard.razor:23–28`), so two of one type from one registrar share a name. | `act-3/3.30-1-smit-stalled-requests.png` |
| **T325** (P3) | The stalled day and an invitation's expiry print `d MMM` (`CoordinatorDashboard.razor:27`, `:49`), from a UTC timestamp and a UTC "today" (`GetCoordinatorDashboardSummaryQuery.cs:42–43`). Flow 05's screens print ISO dates (D1, DESIGN.md:1346). | `act-3/3.30-1-smit-stalled-requests.png`, `states/home--coordinator-expiring.png` |
| **T351** (P3) | `UpdatedOn` is the clock of the stall, the nudge and the assessor's "Waiting N days", and any save resets it. | T351's file |
| **T328** (P3; its dashboard parts landed) | `.list-row` (`W/wwwroot/app.css:1837–1843`; DESIGN.md:1381–1383) and the focus ring on scroll regions (DESIGN.md:2589–2592) exist; the committee's registrar rows still use `li.progress-row-head`, not `.list-row` (`CommitteeMemberDashboard.razor:23`), and `.progress-row-head` lets its figure wrap (`app.css:1919–1925`). | `act-A/A.7.7-1-home-390.png` |
| **B7** (BRIEF § 7) | Programme trainees, Stalled activities and STAR review queue were deleted with their nav items (T335); `NavMenuAuthorizationTests.NoPlaceholderPage_Exists` (`:136`) pins it, and `NavItems.cs:69–71` leaves them to flows 06 and 09. A nav change is a DESIGN.md change: `DesignMdNavTable_MatchesTheMenu_ForEveryActingRole` (`:320`) parses the table (DESIGN.md:520–532). No intent document exists for any of them. | `act-3/3.31-1-stalled-activities-404.png` |
| **B8** (BRIEF § 7) | `/committee/panels` admits the Coordinator (`W/Components/Pages/CommitteeDecisions/PanelsList.razor:2`), whose menu has no item (`NavItems.cs:154`). `/admin/entrustment-decisions` admits both speciality admins (`Admin/EntrustmentDecisions/Index.razor:2`), whose menus have none, and has no owning list (DESIGN.md:589). | Steps 2.32, A.7.7; `act-A/A.7.7-5-entrustment-decisions-390.png` |
| Not built: a staff roster | `/admin/trainees` admits only the Administrator and the InstitutionalAdmin (`Admin/Trainees/PendingTraineesList.razor:2`). The nearest lists by registrar are Decisions due (not the committee: `DecisionsDue.razor:2`), MSF coverage (Coordinator and Administrator, `MultiSourceFeedback/ProgrammeCoverage.razor:2`) and Committee reviews' Trainee column. `WeeklyCoordinatorDigestJob` mails a roster no page shows (nothing logged in 30 days). | Steps 3.52, A.5.11 |
| Not built: one registrar for staff | `/portfolio/progress` and `/portfolio/progress/{EpaId}` are `TraineeOrFormerTrainee` and take no registrar (`Portfolio/MyProgress.razor:4`, `Portfolio/EpaProgress.razor:3`). Staff read one registrar's standing only on a committee review (`CommitteeDecisions/ReviewDetail.razor:707–722`, `:1544–1546`). `/portfolio/export/{TraineeUserId}` admits anyone who oversees her (`A/Features/Reporting/ExportPortfolio.cs:94–110`) and nothing links it (B2, flow 13's). | — |
| Not built: a waiting list, triage | Pending reviews has no link and its comment says "no page lists the backlog yet" (`SpecialityAdminDashboard.razor:9–11`). Stalled requests: `AwaitsReviewer` and 7 days (`GetCoordinatorDashboardSummaryQuery.cs:41`, `:50–59`; `A/Common/Options/DashboardThresholds.cs:8`), at most 10 (`:21`, `:78`), no "more". The nudge: daily 09:00 UTC, 5 days (`src/Wombat.Infrastructure/Scheduling/Jobs/AssessorPendingNudgeJob.cs:43–58`). No reminder or reassignment action exists. In `requested` the Mini-CEX's state is editable by its nominee (`Activities/Seeds/mini_cex_cpsa/workflow.json:10–12`) and its assessor field by the registrar (the section's default, `schema.json:7–23`); writing needs both (`FieldPermissionEvaluator.cs:18`, `:80`), so nobody may change it; the registrar can only cancel and file again. A reassignment is an activity-platform change. | Steps 3.31, 3.32, A.5.10 |
| Observed, not filed | One component, two titles: "Targets met by EPA" (`CommitteeMemberDashboard.razor:33`) and "Curriculum coverage — {semester}" (`SpecialityAdminDashboard.razor:33`, `:45–46`; the sub-speciality twin). Two exemption wordings (`CommitteeMemberDashboard.razor:13`; `EpaTargetCoverageList.razor:14`). The committee card's "semester n/m · yearly n/m" (`:47–61`) and the admins' "active / {m} inactive" (`SpecialityAdminDashboard.razor:26`) break "never n / m" (DESIGN.md:1344); neither uses `ProgressWords`. "Inactive" counts every profile not active (`GetSpecialityAdminDashboardSummaryQuery.cs:86`). The speciality admin's pending count is scoped by the activity's speciality stamp, her trainee count by sub-speciality profile (`:58–77`). An invitation's role prints `TargetRole`, the key (`CoordinatorDashboard.razor:47`; `Domain/Invitations/Invitation.cs:17`). No card sets a named section (`HeadingId`, DESIGN.md:3374–3378) or a count badge in words. | `act-3/3.53-1-mokoena-home.png`, `states/home--coordinator-expiring.png` |

**Dropped from the old brief (fixed since):** Stalled requests reading one literal state and its unlinked rows (T297);
"1 activities in review" and "Review queue →" opening the admin's empty inbox (T297); "Welcome, <email>", "Viewing as"
and "Switch view" (T335's frame); badge and alert contrast on the warning card (T322); dashboard rows stretching badges
(T335's `.list-row`); the browser's focus ring on scroll regions (T335).

## 6. Questions the design must answer

1. **Programme trainees: a page, a fuller card, or nothing?** If a page, who it admits (the committee, both speciality
   admins, the Coordinator), its owning list and nav label, and whether the committee's "Targets this period" becomes
   its preview.
2. **One registrar, as staff read her.** A new staff route (it would need a registrar id and a scope check like
   `TraineeScopeResolver.MayReadAsync`), the committee review page (only once a review exists), or no link. Flow 05's
   `EntrustmentStandingPanel` and `TrajectoryChart` already draw for staff on the review page.
3. **What waits for a reviewer.** One page for both "Pending reviews" and "Stalled requests", two, or Home cards alone;
   and what "stalled" means: the card's 7 days, the nudge's 5, or flow 04's Overdue.
4. **Triage.** "Send a reminder now" and "Reassign to another assessor", each PROPOSED; a reassignment changes the
   activity platform's write rules (§ 5), and who may do it is undecided.
5. **The per-EPA card:** one title; whether its rows link, and to what; its figure at 390 px.
6. **The Coordinator's Quick action** (a card, Home's one header action, or gone), and whether the weekly digest's
   roster belongs on a page.
7. **Menus:** Entrustment decisions for both speciality admins, under that label (replacing "STAR review queue");
   Decision panels for the Coordinator.
8. **(The operator's) T290 and T298 first,** as their own task before the build (flow 05's T304), or folded into it?
9. **(The operator's) Triage, if chosen:** built in this flow, or designed here and filed as its own task?
10. **(The operator's) "Inactive":** ended programmes, erased accounts, or neither?

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the 20 steps** on a fresh database, playing the acts up to each one (`README.md` § How to play): 2.32, 2.33,
  2.35, 2.37 and 2.38; Act 3's 3.30, 3.31, 3.32, 3.33, 3.52, 3.53 and 3.54; the appendix's A.2.8, A.5.10–A.5.12 and
  A.7.5–A.7.8. Every Expect must hold. **Rewrite the steps that change:** a page built where a stub was rewrites 3.31,
  3.52, 3.53, A.5.10–A.5.12 and A.7.7, whose Expects say the menu offers no such page; every step that quotes these
  cards' words (2.32, 2.33, 2.37, 2.38, 3.30, 3.52–3.54, A.7.5–A.7.8, and 4.3's "no Entrustment decisions in the menu")
  is updated in the same task (BRIEF.md § 9 item 7). A new page or action needs a step (`Scenario/ScenarioRunbookTests`).
- **Checks the design adds:** 2.37, Dr van Rensburg sees the registrars (T290); 3.52, ties in surname order (T298);
  3.30, every row named per row and dated in one form (T280, T325); 3.53, Pending reviews leads to a page that lists
  what it counts, or the brief says why not; at 390 px no page scrolls sideways, no figure wraps beside its name, and
  every target is 44 px.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include:
  - `Dashboards/WaitingCardsTests`, `HomeFrameTests` and `AssessorHomeTests`;
  - `Navigation/NavMenuAuthorizationTests` (the nav table, the placeholder check) and `DashboardLinkAuthorizationTests`;
  - `Progress/QuotaProgressRenderingTests` (EpaTargetCoverageList);
  - `Design/NarrowLayoutTests` and `DefinedClassTests`; `Accessibility/RowNamesTests`;
  - `Scenario/`;
  - the Application suite's `Features/Dashboards/` and `CurriculumCoverageTests`.
- **Re-capture** the states in § 3 and the steps' own captures, keeping their names. Compare them with the chosen
  artboards.
- **Browser check** at 1280 and 390 px as each of the four roles, and as the external member.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as flow 06's build left it (T358, 2026-10-05); § 2 records what each step showed before it. The
build added Steps 3.54a–3.54c, A.5.14–A.5.16 and A.7.8a: the round 3 boards (`R2-Steps`) number them 3.55–3.57,
A.5.14–A.5.16 and A.7.9, but the runbook's own 3.55–3.57 (the audit trail) and A.7.9 (Prof Mbatha's phone) already hold
those numbers, so the new ones take a letter after the step they follow, as Step 6.14a did. `D` is the replay day.

```text
Step 2.32 — Mr Smit, Coordinator (act-2-onboarding.md:562)
Role: Coordinator — Mr Pieter Smit
Route: /account/login → / → /admin/invitations → /access-denied → /committee/panels → /committee/panels/new → /access-denied
Do: Sign in and read the dashboard and nav. Type the invitations address. Open Decision panels from the menu, then type
  the new-panel address.
Expect: Home reads "Coordinator · Semester N, YYYY", with the header action "Start an MSF campaign"
  (`/msf/campaigns/new`), where a Quick action card was (T358, Q6). Its cards, in order:
  - "Waiting for assessors", with no badge: "Nothing is waiting for an assessor.", with Open Waiting for assessors;
  - "Nothing filed in 30 days", its rule line "Current registrars with nothing filed (a draft is not filed) in the last
    30 days. A registrar admitted less than 30 days ago is not listed.", then "Every current registrar has filed
    something in the last 30 days.", with Open in Programme trainees: the registrars were admitted today, and a
    registrar admitted less than 30 days ago is not listed (E5);
  - "Invitations nearing expiry": "No invitations expiring soon."
  The sidebar reads "Acting as Coordinator" over Home, Programme trainees, Waiting for assessors, Decisions due, MSF
  campaigns, Committee reviews, Decision panels and Data rights requests, one flat list (E2), then My data rights. It
  has no Invitations, and the invitations page reads "You cannot open this page", "Your role (Coordinator) does not open
  this page." and "If you need it for your work, ask your institution's Wombat administrator.", with Go to Home (T178,
  T335). Decision panels lists the panel with no New panel and no Edit column, and Decision panels is lit there (T358,
  C10). The panel form reads "You cannot open this page" in the same words: the Coordinator reads panels, and edits none
  (D4).

Step 2.33 — Dr Zulu's first view: committee member (act-2-onboarding.md:590)
Role: CommitteeMember + Assessor — Dr Thandi Zulu
Route: /account/login → /
Do: Sign in and read the dashboard and nav.
Expect: Home reads "Committee member · Semester N, YYYY" under its heading. The sidebar reads "Acting as Committee
  member", with "Switch to Assessor" under it: Dr Zulu holds both, and the precedence opens Dr Zulu's sessions as a
  Committee member. The cards (T358, Q1, Q5):
  - "Registrars", badged "5 registrars", under the rule line "Fewest met first, then by surname.": Anele Dlamini, Pieter
    du Plessis, Nomsa Mahlangu, Lerato Molefe and Sipho Ndlovu, all tied at nothing met, so by surname, then first name
    (T298). Each name is a link to the registrar's page, over the training year ("Training year 3"), then "0 of 10" over
    "EPAs met this semester" and "0 of 5" over "EPAs met in YYYY". Nobody is exempt: a 15 January start counts from the
    boundary (D42). Its foot is Open Programme trainees.
  - "Targets by EPA", under "Fewest registrars met first. Semester N, YYYY ends on <the semester's last day, ISO>.":
    PAED-001 to PAED-015 in code order, each EPA's name a link to Programme trainees filtered Short on it, over its
    cadence ("3 per semester", "1 per academic year"), then "0 of 5" over "registrars met this semester" or "registrars
    met in YYYY" (T130's count, never a percentage). It has no foot.
  The menu is the Committee member's alone, never the union of Dr Zulu's roles: Home, Programme trainees, Committee
  reviews and Decision panels, then My data rights.

Step 2.35 — Dr Naidoo and Dr Botha (act-2-onboarding.md:643)
Role: CommitteeMember + Assessor — Dr David Naidoo and Dr Sarah Botha
Route: /account/login → / → /committee/panels
Do: Each signs in, reads the dashboard and opens Decision panels.
Expect: Dr Naidoo and Dr Botha each see what Dr Zulu saw in Step 2.33: the same menu, the five registrars on Registrars
  in its order, and "0 of 5" for every EPA on Targets by EPA. Decision panels lists the panel both sit on, with no New
  panel and no Edit.

Step 2.37 — Dr van Rensburg, external committee member (act-2-onboarding.md:669)
Role: CommitteeMember — Dr John van Rensburg
Route: /account/login → / → /committee/panels
Do: Sign in, read the dashboard and open Decision panels.
Expect: Dr van Rensburg sees "Committee member · Semester N, YYYY" on Home, and no switch: Dr van Rensburg holds one
  role. "Registrars", badged "5 registrars", lists KGK's five current registrars in Step 2.33's order, and "Targets by
  EPA" reads "0 of 5" for each EPA: a committee member reads every current registrar at the member's institution,
  whatever the member's own sub-specialities, and Dr van Rensburg holds none (T290). The sidebar reads "Acting as
  Committee member" with no switch, over Home, Programme trainees, Committee reviews and Decision panels, then My data
  rights. Decision panels lists the panel Dr van Rensburg sits on as external member, with no New panel and no Edit.

Step 2.38 — Dr Mokoena's and Dr Sithole's dashboards (act-2-onboarding.md:690)
Role: SpecialityAdmin and SubSpecialityAdmin — Dr Refilwe Mokoena and Dr Kabelo Sithole
Route: /
Do: Each opens Home, now that the registrars are admitted.
Expect: Each sees "Speciality admin · Semester N, YYYY" or "Sub-speciality admin · Semester N, YYYY" on Home. The cards,
  read for Paediatrics (T358, Q3): "Waiting for assessors", with no badge, reads "Nothing is waiting for an assessor.",
  with Open Waiting for assessors; "Registrars" lists the five as Step 2.33's card does, badged "5 registrars", with
  Open Programme trainees; "Targets by EPA" lists PAED-001 to PAED-015, each "0 of 5". There is no Pending reviews, no
  Trainees in programme and no "inactive" anywhere (Q10). The sidebar reads "Acting as Speciality admin" or "Acting as
  Sub-speciality admin", over Home, Programme trainees, Waiting for assessors, Decisions due, Committee reviews,
  Decision panels and Entrustment decisions, then My data rights.

Step 3.30 — Mr Smit's dashboard shows the stalled requests (act-3-operations.md:752)
Role: Coordinator — Mr Pieter Smit
Route: /
Do: Once Dr Mahlangu's Mini-CEX and Dr du Plessis's portfolio review have waited more than seven days, open the
  dashboard.
Expect: Home's "Waiting for assessors" is in the warning stripe, badged "3 waiting, 2 overdue", under the rule line
  "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5." (T358, Q3). It lists every request
  whose next move names one person, oldest first, each row its link ("Type · EPA · date", then "from <registrar>"), its
  state's badge with Overdue beside it, "With <assessor>" on its own line, then the wait (E6):
  - "Portfolio and Logbook Review (Paediatrics) · PAED-015 · `D−1`", from Pieter du Plessis: Awaiting review, Overdue,
    "With Mohammed Patel", "Waiting 8 days";
  - "Mini-CEX (Paediatrics) · PAED-004 · `D−3`", from Nomsa Mahlangu: Requested, Overdue, "With Thandi Zulu", "Waiting 8
    days";
  - "Case-Based Discussion (Paediatrics) · PAED-002 · `D−5`", from Pieter du Plessis: Requested, "With Fatima Khumalo",
    "Waiting less than a day" (filed in this sitting, Step 3.21, and not aged).
  Each link opens the activity's page, `/activities/{id}` (T297), and no date on the card is written "26 Sept" (T325).
  Its foot is Open Waiting for assessors. "Nothing filed in 30 days" reads "Every current registrar has filed something
  in the last 30 days." and "Invitations nearing expiry" "No invitations expiring soon.". "Start an MSF campaign" is
  Home's header action; there is no Quick action card.

Step 3.31 — Mr Smit looks for a page of stalled requests (act-3-operations.md:799)
Role: Coordinator — Mr Pieter Smit
Route: / → /programme/waiting → /not-found
Do: Open Waiting for assessors from the menu. On the Mini-CEX's row choose Send a reminder, read the dialog, then choose
  Send the reminder. Then type the address the menu once linked, `/placeholder/stalled-activities`.
Expect: The menu reads Home, Programme trainees, Waiting for assessors, Decisions due, MSF campaigns, Committee reviews,
  Decision panels and Data rights requests, then My data rights, and Waiting for assessors is lit on its page (T358,
  Q7). The page is headed "Waiting for assessors", its subtitle "Requests at Kgosi Kgari Teaching Hospital whose next
  move names an assessor, supervisor or reviewer, read as Coordinator. Your own requests are not listed." (E4), over the
  filters Waiting (All) and With (Anyone), and Show. The list's heading reads "3 waiting, 2 overdue", its rule line
  "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5. Waiting counts from the last move:
  any save restarts it.", and the table's columns are Activity, With, State and Waiting: Step 3.30's three rows in its
  order, each Waiting cell "8 days" or "Less than a day" over "since … SAST", then Send a reminder.
  On the Mini-CEX's row Send a reminder opens a dialog titled "Send Thandi Zulu a reminder?", which says "Thandi Zulu
  gets one email, "Activities awaiting your assessment", listing this request: Mini-CEX (Paediatrics) from Nomsa
  Mahlangu — waiting 8 days. It moves nothing: the request stays Requested, its wait is not restarted, and Nomsa
  Mahlangu is not told." and opens on Don't send (C4). After Send the reminder, above the table and with the focus:
  "Reminder sent to Thandi Zulu. It lists Mini-CEX (Paediatrics) · PAED-004 · `D−3`, from Nomsa Mahlangu, waiting 8
  days. The request is still Requested; its wait is unchanged." The list is read again: the Mini-CEX's row reads
  "Reminded `D` by Pieter Smit" where its button was, still Requested, Overdue and "8 days"; the other two keep their
  buttons.
  The log holds one stub mail (tags reminder, assessor-reminder) "Activities awaiting your assessment", "Hi Thandi",
  listing "Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 8 days". Nomsa Mahlangu is sent nothing. No page offers
  Reassign (C5). The old address is "Page not found" with status 404: the placeholder page went with the stubs (T335,
  flow 01).

Step 3.32 — The daily nudge reminds the assessors (act-3-operations.md:830)
Role: Administrator — devadmin@wombat.local
Route: /admin/jobs
Do: Run `assessor-pending-nudge` now.
Expect: The job's last run updates. The application log then holds three stub emails "Activities awaiting your
  assessment", naming no address (T282), each greeting its assessor by first name: Step 3.31's reminder (tags reminder,
  assessor-reminder), "Hi Thandi", and the nudge's two (tags nudge, assessor-pending):
  - "Hi Thandi", listing Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 8 days;
  - "Hi Mohammed", listing Portfolio and Logbook Review (Paediatrics) from Pieter du Plessis — waiting 8 days.
  The nudge still mails a request reminded today: a reminder moves nothing (T358, C4). The run's summary line reads
  "assessors nudged 2 (activities 2)" and skips nobody. Nothing else has waited five days, so nobody else is nudged: Dr
  du Plessis's CBD has waited less than a day.

Step 3.33 — Dr Zulu switches to her assessor view and completes the stalled Mini-CEX (act-3-operations.md:851)
Role: Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Her sessions open acting as a Committee member; choose Switch to Assessor in the sidebar. From "Waiting for you",
  open Dr Mahlangu's Mini-CEX (the Activity inbox lists it too), rate it 3a with feedback, and complete it.
Expect: The sidebar first reads "Acting as Committee member", with "Switch to Assessor" under it, and Home "Committee
  member · Semester N, YYYY" under its heading. Under the header, above the committee cards (Registrars and Targets by
  EPA), a warning line reads "1 activity waits for you in the Activity inbox, and it is overdue: Mini-CEX (Paediatrics)
  · PAED-004 · `D−3`, from Nomsa Mahlangu, waiting 8 days.", with Open it (T350). After the switch Home reads "You are
  now acting as Assessor." in an info alert under the header, and "Assessor · Semester N, YYYY", with no such line; the
  sidebar reads "Acting as Assessor" over Home and Activity inbox, and "Waiting for you", in the warning stripe, is
  badged "1 waiting, 1 overdue": this Mini-CEX, "Mini-CEX (Paediatrics) · PAED-004 · `D−3`", from Nomsa Mahlangu,
  Requested with Overdue beside it, "Waiting 8 days": it has waited past the assessor's seven days since Step 3.30 aged
  it. The inbox lists it with "8 days" over "since `D−8` … SAST"; the page's status card reads "Your move. Nomsa
  Mahlangu asked you on `D−8` … SAST.", the same moment. Step 3.31's reminder has not moved it. Dr Zulu rates it on the
  rung picker. After Complete the result reads "Completed. Nothing else waits for you.", with Go to Home, and Home then
  reads "Nothing is waiting for you." The Mini-CEX is Completed, credited "1 item". Year 1's minimum on PAED-004 is 3a.
  Nothing of Dr Mahlangu's waits for an assessor any more: the Mini-CEX has left Waiting for assessors and Mr Smit's
  Home card.

Step 3.52 — Dr Zulu's committee dashboard (act-3-operations.md:1243)
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → /
Do: Choose Switch to Committee member in the sidebar and read the dashboard and the menu.
Expect:
  - Home opens acting as Assessor, Dr Zulu's choice from Step 3.33, kept with the account. After the switch an info
    alert reads "You are now acting as Committee member.", the sidebar "Acting as Committee member", and Home's subtitle
    "Committee member · Semester N, YYYY".
  - "Registrars", badged "5 registrars", fewest met first, then by surname: Pieter du Plessis, Nomsa Mahlangu and Sipho
    Ndlovu, each "0 of 10" over "EPAs met this semester" and "0 of 5" over "EPAs met in YYYY"; then Anele Dlamini and
    Lerato Molefe, each "1 of 10" and "0 of 5" (T298 at each tie).
  - "Targets by EPA", fewest registrars met first, then by code: PAED-002 to PAED-015, each "0 of 5", then PAED-001 ("3
    per semester") last, "2 of 5" over "registrars met this semester".
  - Each registrar's name is a link to the registrar's page, and each EPA's name a link to Programme trainees filtered
    Short on it, its figure in words hidden after it. No card is one link around its rows (T280).
  - The menu is Home, Programme trainees, Committee reviews and Decision panels, then My data rights.

Step 3.53 — Dr Mokoena's dashboard (act-3-operations.md:1270)
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /
Do: Read the dashboard and the menu.
Expect:
  - "Waiting for assessors", in the warning stripe, badged "2 waiting, 1 overdue": Dr du Plessis's portfolio review
    (Awaiting review, Overdue, "With Mohammed Patel", "Waiting 8 days"), then the CBD (Requested, "With Fatima Khumalo",
    "Waiting less than a day"), each a link to its activity, with Open Waiting for assessors (T358, Q3).
  - "Registrars" and "Targets by EPA" read as Step 3.52's, for Paediatrics: the same five, badged "5 registrars", and
    PAED-001 "2 of 5" last.
  - No Pending reviews, no Trainees in programme and no "inactive" (Q10).
  - The menu is Home, Programme trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels and
    Entrustment decisions, then My data rights. The STAR review queue is not restored: Entrustment decisions takes its
    place.

Step 3.54 — Dr Sithole's dashboard (act-3-operations.md:1294)
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /
Do: Read the dashboard.
Expect: The same three cards and figures as Dr Mokoena's, read for Paediatrics: "2 waiting, 1 overdue", Registrars
  badged "5 registrars", and PAED-001 "2 of 5" last on Targets by EPA. Dr Sithole's menu reads as Dr Mokoena's, under
  "Acting as Sub-speciality admin".

Step 3.54a — Dr Zulu reads who is short on PAED-002 (act-3-operations.md:1306)
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /programme/trainees
Do: On Home, choose PAED-002 on Targets by EPA. Then choose Clear filters.
Expect: Programme trainees opens with Short on set to PAED-002 and Programme trainees lit (T358, Q1). Its subtitle reads
  "Current registrars at Kgosi Kgari Teaching Hospital, read as Committee member · Semester N, YYYY". The list's
  heading, with the focus, reads "5 of 5 current registrars are short on PAED-002", its rule line "PAED-002 — <its
  title>, 3 per semester. Furthest from its target first, then fewest EPAs met, then by surname.", and its columns are
  Registrar, Training year, PAED-002, This semester, In YYYY and Last filed. Pieter du Plessis, Anele Dlamini and Lerato
  Molefe read "0 of 3 this semester" over "3 more by <the semester's last day, ISO>", then Nomsa Mahlangu and Sipho
  Ndlovu "1 of 3 this semester" over "2 more by …". Each registrar's name is a link to the registrar's page.
  Clear filters shows the whole list, headed "5 current registrars", under "Fewest met first, then by surname. Semester
  N, YYYY ends on <its last day>.", its columns Registrar, Training year, This semester, In YYYY, Furthest short and
  Last filed. Furthest short reads "PAED-002, PAED-003, PAED-005" for Anele Dlamini and "PAED-001, PAED-003, PAED-005"
  for Nomsa Mahlangu, each over "0 of 3 each this semester" (D11). Nothing on the page reads "n / m", a percentage or
  the bare word "year" (Q10).

Step 3.54b — Dr Zulu opens Dr Mahlangu's page (act-3-operations.md:1327)
Role: CommitteeMember — Dr Thandi Zulu
Route: /programme/trainees → /programme/trainees/{ProfileId:int}
Do: On Programme trainees, choose Nomsa Mahlangu, and read the page to its foot.
Expect: The registrar page's h1 reads "Nomsa Mahlangu", its tab "Nomsa Mahlangu · Wombat", its subtitle "Training year 1
  · Semester N, YYYY · Kgosi Kgari Teaching Hospital, Paediatrics"; the trail reads Home › Programme trainees › Nomsa
  Mahlangu, and Programme trainees is lit (T358, C2, C9). Its sections, in order, each headed:
  - This period: "0 of 10" over "EPAs met this semester" and "0 of 5" over "EPAs met in YYYY", then "Semester N, YYYY
    ends on <its last day>. Training year 1 sets the minimum level each encounter is judged against.";
  - EPAs: the index, in code order, each EPA's name as text, not a link (review 7), among them PAED-001 "0 of 3 this
    semester", and PAED-002 and PAED-004 "1 of 3 this semester";
  - Entrustment against Annexure A: the standing panel, PAED-004's latest rating "3a · Encounter `D−3`" and PAED-002's
    "3a · Encounter `D−8`"; each EPA charted on this page names a link to its chart below;
  - Rating trajectories: a chart for each EPA rated in the YYYY academic year (PAED-002's DOPS and PAED-004's Mini-CEX,
    both 3a), each headed by its own h3, with no Today rule (D8);
  - Waiting for assessors: "Nothing of Nomsa Mahlangu's waits for an assessor.";
  - Committee reviews: "No review is scheduled for Nomsa Mahlangu."
  Dr Zulu, a Committee member, is offered no Send a reminder. No word on the page names Nomsa Mahlangu by a pronoun
  (round-3-check 1).

Step 3.54c — Dr Mokoena filters Waiting for assessors (act-3-operations.md:1349)
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /programme/waiting
Do: Open Waiting for assessors from the menu. Choose With: Mohammed Patel and, before Show, look at the list; then Show.
  Then choose Waiting: Overdue only and With: Fatima Khumalo, and Show. Last, choose Clear filters.
Expect: The subtitle reads "Requests in Paediatrics whose next move names an assessor, supervisor or reviewer, read as
  Speciality admin. Your own requests are not listed." (E4), and the heading "2 waiting, 1 overdue": Step 3.53's two
  rows, each with Send a reminder. The With filter offers Anyone, Fatima Khumalo and Mohammed Patel, each waiting row's
  assessor by surname. Nothing on the page changes until Show (round 3 item 31). After Show the address carries the
  filter (`?with=<id>`, D3), Clear filters is offered, and the heading takes the focus: "1 waiting, 1 overdue, with
  Mohammed Patel", over the portfolio review alone. After the second Show the heading reads "0 of 2 waiting", and under
  it "No request matches these filters.", "Overdue only, with Fatima Khumalo." and Clear filters (review 33). Clear
  filters returns the whole list, "2 waiting, 1 overdue". No reminder is sent.

Step A.2.8 — devadmin runs the two reminders against aged work (appendix-cross-cutting.md:398)
Role: Administrator — devadmin
Route: /admin/jobs
Do: Age the draft by 15 days and the request by 6 (see the Note). Then run activity-draft-nudge and
  assessor-pending-nudge now, and read the log.
Expect: The log holds a stub mail "You have draft activities waiting" beginning "Hi Nomsa,", listing "Mini-CEX
  (Paediatrics) — draft for 15 days". It also holds "Activities awaiting your assessment", beginning "Hi Fatima,",
  which lists "Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 6 days". Each job logs one line counting whom it
  reminded and whom it skipped, and why (T151, T240).

Step A.5.10 — Mr Smit's stalled requests (appendix-cross-cutting.md:804)
Role: Coordinator — Mr Pieter Smit
Route: / → /programme/waiting
Do: Read the dashboard's "Waiting for assessors" card, then open Waiting for assessors from its foot.
Expect: The card, in the warning stripe, is badged "3 waiting, 1 overdue", and lists, oldest first, each row a link to
  its activity's page (T297):
  - Dr du Plessis's portfolio review, Awaiting review, Overdue, "With Mohammed Patel", "Waiting 8 days": Dr du Plessis
    withdrew at Step 5.27, and the request still waits (round-3-check, r6);
  - Dr Mahlangu's Mini-CEX of Step A.2.7, encounter `D−2`, Requested, not overdue, "With Fatima Khumalo", "Waiting 6
    days" (aged at Step A.2.8);
  - Dr du Plessis's CBD of Step 3.21, Requested, "With Fatima Khumalo".
  Waiting for assessors lists the same three in that order under "3 waiting, 1 overdue", each with Send a reminder (none
  is sent here). Nothing offers Reassign (C5).

Step A.5.11 — Dr Botha's menu offers no Programme trainees (appendix-cross-cutting.md:826)
Role: CommitteeMember — Dr Sarah Botha
Route: /
Do: Read the menu.
Expect: "Acting as Committee member", with "Switch to Assessor" under it, over Home, Programme trainees, Committee
  reviews and Decision panels, then My data rights. There is no Waiting for assessors: chasing work is not a Committee
  member's (T358, Q7).

Step A.5.12 — Dr Mokoena's menu offers no Programme trainees or STAR review queue (appendix-cross-cutting.md:837)
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /
Do: Read the menu.
Expect: Home, Programme trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels and
  Entrustment decisions, then My data rights (T358, R2-Menus). The STAR review queue is not restored: Entrustment
  decisions, the programme's register of STARs, takes its place (DESIGN.md § The NavMenu).

Step A.5.14 — Registrar pages Dr Zulu may not read, by address (appendix-cross-cutting.md:859)
Role: CommitteeMember — Dr Thandi Zulu
Route: /programme/trainees/{ProfileId:int} → /programme/trainees/{ProfileId:int}
Do: Type the registrar page's address with the trainee profile id of the dev trainee `trainee@wombat.local`, at the Demo
  Institution; then with Dr Ndlovu's, whose account was erased in § A.1.
Expect: Each is flow 01's Page not found, word for word: the h1 "Page not found", the tab "Page not found · Wombat",
  "There is no page at this address.", "Check the address, or start again from Home." and Go to Home, with no trail and
  nothing in the menu lit. Neither page confirms that the profile exists (T358, C2; CLAUDE.md: 404, not 403), and
  nothing of either registrar is read.

Step A.5.15 — A reminder Wombat cannot send (appendix-cross-cutting.md:876)
Role: Coordinator and Administrator — Mr Pieter Smit and devadmin
Route: /programme/waiting → /admin/users/{UserId} → /programme/waiting → /admin/users/{UserId} → /programme/waiting
Do: On a scratch database (the Note), Mr Smit opens Waiting for assessors and leaves it open. devadmin locks Dr Khumalo
  out on Dr Khumalo's account page (Lock out user). Mr Smit, on the list as read before the lock, chooses Send a
  reminder on Dr du Plessis's CBD, then Send the reminder.
  devadmin then reactivates Dr Khumalo (Reactivate user), and Dr Patel's email address is cleared (the Note). Mr Smit,
  on the list as read before, chooses Send a reminder on the portfolio review, then Send the reminder.
Expect: The first answer stands above the table and takes the focus: "Not sent. Fatima Khumalo's account is deactivated,
  so Wombat sends Fatima Khumalo no email. The request still waits." The list is read again, and both of Dr Khumalo's
  rows read "No reminder: Fatima Khumalo's account is deactivated." where the button was (E3).
  The second, in the same place and with the focus: "Not sent. Mohammed Patel has no email address in Wombat. Ask your
  institutional admin to add one.", and the portfolio review's row then reads "No reminder: Mohammed Patel has no email
  address in Wombat.".
  Neither request moves: each keeps its state and its wait, and no "Reminded …" line is recorded. The log gains no
  reminder mail. A reminder is never refused because an assessor opted out of digest emails (E1).

Step A.5.16 — Mr Smit looks for registrars with nothing filed (appendix-cross-cutting.md:900)
Role: Coordinator — Mr Pieter Smit
Route: / → /programme/trainees
Do: On Home, choose Open in Programme trainees under "Nothing filed in 30 days".
Expect: Home's "Nothing filed in 30 days" reads "Every current registrar has filed something in the last 30 days.": the
  two current registrars, Dr Dlamini and Dr Mahlangu, were admitted in this sitting, and a registrar admitted less than
  30 days ago is not listed (E5). Programme trainees opens with Nothing filed in 30 days ticked and Programme trainees lit. Nobody
  matches, so the heading reads "0 of 2 current registrars", and under it "No registrar matches these filters.",
  "Nothing filed in 30 days." and Clear filters (review 33).
  A registrar is listed only once admitted at least 30 days before and with nothing filed since `D−30`: a draft is not
  filed, and a recorded MSF is. The heading then takes the count's own form, singular or plural: "1 of 2 current
  registrars has filed nothing in 30 days", "2 of 2 current registrars have filed nothing in 30 days" (D9).

Step A.7.5 — Mr Smit on his phone (appendix-cross-cutting.md:1202)
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions due and the data-rights queue.
Expect: Home's header action, Start an MSF campaign, is a row of its own below the header's rule, 44px tall and the
  page's width, and the cards stack in one column, each row's link a 44px block (T358, R2-Home k6). The campaign page
  keeps its gutter (T226), and its invitee table (counts by respondent group: the page never lists an address) scrolls
  within its card. Decisions due's summary scrolls sideways, and a keyboard can scroll it, as a labelled, focusable
  region (DESIGN.md's decisions-due contract). The queue's filters stack above its table.

Step A.7.6 — Dr Zulu on her phone (appendix-cross-cutting.md:1222)
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: At 390 px, open Committee reviews and Dr Molefe's final review.
Expect: Home's Registrars and Targets by EPA cards are one column: each registrar's name a 44px block with the training
  year and the figures under it, each EPA's name a 44px block with its cadence and its "n of 2" under it (T358, round 3
  item 21). The review's cards stack. The evidence tables scroll inside their containers. Each EPA's trajectory chart is
  a card of its own (its section is not a card around it), drawn at 322 px, never scaled, its rungs and months 12 px,
  and fits its named region, "Rating chart for PAED-001" and the like (E1), with its table stacked under it. Nothing
  scrolls sideways (T166, T355).

Step A.7.7 — Dr Mokoena on her phone (appendix-cross-cutting.md:1244)
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /committee/panels → /committee/decisions-due → /admin/entrustment-decisions
Do: At 390 px, open the dashboard, then Decision panels, Decisions due and Entrustment decisions from the menu.
Expect: Home stacks Waiting for assessors, badged "3 waiting, 1 overdue", Registrars, badged "2 registrars", and Targets
  by EPA, KGK-001 first among the EPAs neither registrar has met, by code (T358). The panels list and the decisions list
  scroll inside their containers (T226). Entrustment decisions is in the menu, lit on its page, with no trail: it is a
  list the menu offers (R2-Menus m5).

Step A.7.8 — Dr Sithole on his phone (appendix-cross-cutting.md:1263)
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: / → /committee/reviews → /committee/decisions-due
Do: At 390 px, open his dashboard, then Committee reviews and Decisions due from the menu.
Expect: As for Dr Mokoena, scoped to Dr Sithole's sub-speciality: Home stacks the same three cards. The folded bar reads
  "Acting as" over "Sub-speciality admin", in two lines: the bar grows, and the role is never cut. Dr Sithole's menu
  reads as Dr Mokoena's (A.5.12), each row 44px.

Step A.7.8a — Programme trainees, a registrar's page and Waiting for assessors on a phone and a tablet (appendix-cross-cutting.md:1276)
Role: CommitteeMember and Coordinator — Dr Thandi Zulu and Mr Pieter Smit
Route: /programme/trainees → /programme/trainees/{ProfileId:int} → /programme/waiting → /
Do: At 390 px, Dr Zulu opens Programme trainees from the menu, then Dr Mahlangu's page; Mr Smit opens Waiting for
  assessors from the menu. Then each opens the same pages, and Home, at 768 px.
Expect: At 390 px both lists' tables stack, a block per row: each registrar's name and each activity's link a 44px
  block, every other cell under its column's name. The filter bar's fields stack, and Show and Clear filters are each
  44px tall and the row's width; so is each Send a reminder (round 3 items 22, 25). On Dr Mahlangu's page the sections
  stack, the trail is one 44px link back to Programme trainees, Dr Mahlangu's Mini-CEX of Step A.2.7 is listed under
  Waiting for assessors with no Send a reminder for Dr Zulu, and each trajectory chart is drawn at 322 px, never scaled,
  in its own named region.
  At 768 px Home's Registrars and Targets by EPA rows are one column for Dr Zulu, each figure under its name, and Mr
  Smit's cards stack in one column (round 3 item 21; the grid has one track at 900 px and below). Nothing scrolls
  sideways at either width.
```
