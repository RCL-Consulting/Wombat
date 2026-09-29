# F14 A person exercises data rights, and staff decide them

A registrar asks for a copy, access, a correction or erasure of her data, and a coordinator or the Administrator decides
it. These are legal rights, and an erasure cannot be undone, so each page must say exactly what happened and what will.

Part of the GUI redesign briefed in `design/BRIEF.md` (T332). Written 2026-09-26.

| | |
|---|---|
| **People** | Trainees Dr Anele Dlamini (an export), Dr Pieter du Plessis (access, withdrawn), Dr Nomsa Mahlangu (a correction, rejected) and Dr Sipho Ndlovu (erasure); Coordinator Mr Pieter Smit, who decides KGK's requests; Administrator `devadmin`, who sees every institution's; InstitutionalAdmin Prof Nolwazi Mbatha, who reads the aftermath. Every user reaches the digest opt-out (A.2.3) |
| **Frequency and stakes** | Rare. High: legal rights (POPIA, per CUSTOMIZATION.md § 5), and an erasure is irreversible |
| **Mode** | **Straight to fidelity** for the requester's page and the queue (the Form, List and Detail page shapes hold: BRIEF § 4). **Wireframe first** for the new Correction card on a request (BRIEF § 7 B3) |
| **Pages** (`coverage.md` templates) | `/account/data-rights`, `/admin/data-rights`, `/admin/data-rights/{Id:guid}`, `/not-found`, `/account/login`, `/admin/users`, `/admin/trainees`, `/committee/reviews`, `/committee/reviews/{ReviewId:int}`; endpoints `/account/data-rights/download/{id:guid}` and `/account/session-ended` |
| **Held screenshots** | None. No group-1 task changes this flow's pages. Two Home captures from its phone steps, formerly held under T297, were re-captured on 2026-09-26 (§ Attach) |

**How to run this thread** (BRIEF § 2.3, step 3; source keys are BRIEF § 2.0's):
1. **Decide T316's question first** and write the answer into the chat: who carries an approved correction through. The
   recommendation is whoever may approve the request (the reviewing role in scope, and the Administrator), so that one
   person approves, corrects and completes (T316 § What to build). Screen 4 depends on it.
2. Open a new thread in the Wombat design system. Attach the KEY SCREENSHOTS (§ Attach, first list).
3. Paste **The ask** below. Then paste **The runbook steps, verbatim** (the last section) as the next message: "Walk
   Claude through the journey and it will generate each screen in context" [academy].
4. Add the other screenshots in § Attach as the chat asks for them.
5. Screens 1–3 and 5 go straight to fidelity. Screen 4 is a wireframe first ("Wireframe first when fidelity doesn't
   matter" [parrott]); pick one, then ask for its fidelity.
6. Write every decision into the chat as a sentence; the chat travels in the handoff bundle [academy]. Export the chosen
   artboards to `design/flows/14-data-rights/` before moving on: there is no version history [start].

## The ask

Paste this block as the thread's first message.

```text
FLOW 14 — I ask Wombat for my data, a correction or erasure, and see the answer; staff decide my request.

GOAL: Let any signed-in person set their processing preferences (today: opting out of digest emails) and make a
  request: an export (a download of everything held), access, a correction (rectification) or erasure. They can
  withdraw a request, download a completed export, and read the decision. Let a coordinator (his own institution's
  requests) or the Administrator (every institution's) find a request, read it and decide it with a note. An approved
  erasure ends the person's session and replaces them with a pseudonym everywhere. What is wrong today:
  - an approved correction can never be carried through: no page records the correction or completes the request, so
    it stays Approved for good;
  - the pages print code names ("UnderReview"), and the Type filter offers "Objection", which no one can submit;
  - the page promises emails nobody sends ("You will be notified when it is reviewed."; the digest help says a request
    to assess and a committee decision are emailed), and a rejected requester never sees the note or the date;
  - approving an erasure asks nothing first, although it cannot be undone.

AUDIENCE: Trainees Dr Anele Dlamini, Dr Pieter du Plessis, Dr Nomsa Mahlangu and Dr Sipho Ndlovu (any user can be a
  requester). Coordinator Mr Pieter Smit and the Administrator decide. InstitutionalAdmin Prof Nolwazi Mbatha reads
  what an erasure left. Rare, and high stakes: these are legal rights, and an erasure is irreversible. Desktop
  1280×800 and phone 390×844.

LAYOUT: All pages sit in the signed-in shell. The requester's page is a form and a list on one page (today three parts:
  processing preferences and a request form side by side, then "Your requests" as a table). The queue is a List page:
  filters, then Apply, then the table and its pager. A request is a Detail page: a Request card, then a Decision card or
  an Action card (decision note, Approve, Reject).

CONTENT (use these real scenario values; the institution is Kgosi Kgari Teaching Hospital, KGK):
  - Dr Dlamini, Export, reason "A copy of my training record for my own files.", submitted 2026-09-26 15:14 SAST;
    approved with the note "Identity confirmed; export released to the data subject."; it reads Completed at once and
    downloads data-export-20260926-131859.zip (data-export.json and portfolio-summary.pdf).
  - Dr du Plessis, Access, "Everything held about me.", then withdrawn.
  - Dr Mahlangu, Rectification, "My programme start is recorded as 15 January 2026; I started work on 19 January.";
    rejected with the note "The whole January 2026 intake is admitted on 15 January, the semester boundary (D42).
    Nothing to correct."
  - Dr Ndlovu, Erasure, "I am leaving the programme and want my personal data removed."; approved by the Administrator
    with "Identity confirmed. Programme records are kept under a pseudonym."; he becomes deleted_user_622063bc.
  - Ids stay visible on the queue and on a request, because the id is the record there: a request id such as
    01a0dde1-8ee6-79a8-ba4b-955f41042d17, the requester's user id, and "Decided by" as a user id. Design them to
    wrap, not to hide.
  - For the Correction card, an illustrative approved correction (not in the story): the requester's last name on her
    account, "Mahlngu" before, "Mahlangu" after.

SCREENS, in order:
  1. /account/data-rights — Your data rights (the requester). States: nothing requested ("No requests"); preferences
     saved; reason missing (refused, nothing sent); submitted (the row offers a Withdraw named for it); completed (a
     Download named for the request); withdrawn; rejected, now showing the decision date and the decision note (not
     who decided); loading; 390 px, with "Your requests" scrolling inside its container and each row's action
     reachable.
  2. /admin/data-rights — Data rights requests (the queue). States: the coordinator's (his institution only); nothing
     matches; three requests, newest first, each with a status badge; the Administrator's (every institution);
     loading; 390 px. The filters (Type, Status) come before Apply, and Apply before the table, in reading and tab
     order. Each row's Review action is named for the row.
  3. /admin/data-rights/{Id:guid} — A request. States: submitted (the Action card); decision note required ("A decision
     note is required."); approved (the Decision card: decided by, decided on, the note, completed on); withdrawn (no
     decision offered); rejected; the NEW erasure confirmation dialog (below); erasure approved; not found (one state
     for an unknown id and for another institution's request: never "not authorized"); 390 px.
  4. NEW — the Correction card on an approved Rectification, shown to whoever may carry it through. It lists the
     corrections recorded so far; records one (which record, the value before, the value after); links to the page that
     owns the data (the trainee profile, the account's name), where the data itself is corrected; and offers Complete
     request, refused until at least one correction is recorded. Once complete, the Decision card shows Completed on,
     and the requester's page reads Completed.
  5. The aftermath of an erasure: the requester's open tab goes to the sign-in page ("Your session has ended. Please
     sign in again."), and signing in again is refused as for any unknown address; the users list and the trainees
     list no longer show him; Committee reviews lists his reviews under the pseudonym deleted_user_…, the open one
     Withdrawn with its reason; a download address that is not yours reads Page not found.

STEPS: A.1.1–A.1.14, A.2.3, A.2.5, A.5.7, A.7.3 and A.7.5 (19 steps), pasted verbatim in my next message (Role / Route /
  Do / Expect). Walk them in order; each Expect is what the screen must let the person see or do.

STATES TO SHOW, beyond those above: the load-error alert on each of the three pages (and no empty state under it);
  access denied for a trainee who types the queue's address. Data volumes: no requests, three, and fifty across four
  institutions (the pager's "Showing 1–50 of N"); a reason and a note of one line and of a long paragraph.

REQUIREMENTS FROM KNOWN DEFECTS (each is testable):
  - T316: an approved Rectification shows the Correction card (screen 4) to whoever approves it. Complete request is
    refused until a correction is recorded. Then the requester's page reads Completed.
  - T324: types and statuses print as labels ("Under review", not "UnderReview") on the filters, every badge and every
    row name. The Type filter offers exactly the four types the form offers (Access, Export, Rectification, Erasure),
    not Objection.
  - T319: the requester sees, on a decided request, the decision date and the decision note, but not who decided. The
    decision notice email carries no note; it links to the page.
  - T319 and T320: the page promises only mail that is sent. "You will be notified when it is reviewed." stays only
    because T319 sends the notice. The digest help names only the one-off emails that exist.
  - T318: the "Opt out of optional processing" box goes, because nothing reads it. The preferences card says what can
    be opted out of (the three digest reminders) and that processing the programme needs cannot be objected to. Each
    action shows one result: a refusal never sits beside an earlier "Processing preferences saved."
  - T264: approving an erasure asks first, in a dialog that counts the open reviews and campaigns it will withdraw and
    says it cannot be undone. The trigger is an outline button; the dialog's confirm is the danger button.
  - T323: long addresses and ids wrap inside the Request and Decision cards at 1280 px (today they are clipped).
  - T328: Apply comes after its filters in tab order; textareas and selects use the body font (the Reason field is
    monospace today).
  - T325: every time carries its zone, "2026-09-26 15:14 SAST" (today the stored UTC prints with no zone, so 13:14 at
    15:14 SAST).
  - Every action's result takes the focus. A refusal keeps the typed reason or note. An out-of-scope request is "not
    found", never "forbidden".

QUESTIONS THE DESIGN MUST ANSWER:
  1. How is an erasure's finality made unmistakable before approval? What goes: the account (replaced by a pseudonym,
     its sign-in refused, its open session ended within a minute), every open committee review (withdrawn, with a
     reason), every MSF campaign about the person not yet released (withdrawn), a still-active trainee profile (ended
     that day). What is kept, under the pseudonym: ratified reviews, a review under appeal, issued STARs, released
     feedback reports, activities, progress and audit entries. The request itself keeps the requester's address.
     Known leftovers (T276): an activity still in its workflow can later credit the pseudonym, and user ids inside
     activity data are not rewritten.
  2. Does the requester's page keep preferences and requests on one page, or separate them?

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
  - FULL FIDELITY, 2–3 variations: screens 1, 2, 3 and 5.
  - WIREFRAME, 2–3 variations: screen 4, the Correction card (NEW). Stop for my pick, then give it full fidelity.
  - Name every design-system component you use and mark anything else NEW. Say which Wombat rule a variation breaks.
  - Show the empty, error and loading states and the data volumes above. Review each screen for WCAG 2.1 AA: contrast,
    focus, targets of at least 24 px, and colour never the only signal (every status is written in words).
  - Ask me before assuming anything the steps leave open.

ATTACHED: states/data-rights--submitted.png, states/data-rights--completed.png,
  states/data-rights-requests--coordinator.png, states/data-rights-request--note-required.png,
  states/data-rights-request--erasure.png, states/data-rights-request--approved.png,
  act-A/A.1.7-1-mahlangu-rectification-submitted.png, act-A/A.1.13-1-ndlovu-session-ended.png,
  states/reviews-schedule--pseudonym.png, states/data-rights-requests--narrow.png, states/data-rights--rejected.png.
  These show today's pages, defects included; they are not the target.
```

## The journey

One line per step, in play order: who does what, and what the screen must let them see. The full steps are in the last
section.

| Step | Page | Who does what | What they must be able to see |
|---|---|---|---|
| A.1.1 | `/` → `/account/data-rights` | Dr Dlamini reads the page and saves a preference | Preferences, a request form and "Your requests" ("No requests"); the digest opt-out's help naming the three reminders it stops; "Processing preferences saved." with the focus on it |
| A.1.2 | `/account/data-rights` | She submits an Export with no reason, then with one | The refusal "Please provide a reason for your request.", nothing sent; then the row: today, Export, Submitted, a Withdraw named for it, no Download |
| A.1.3 | `/admin/data-rights` → `/admin/data-rights/{Id:guid}` | Mr Smit filters the queue to Export and Submitted, applies, reviews her request, approves with no note, then with one | Only KGK's requests; filters that act only when applied; the request's details; "A decision note is required."; then Completed and the Decision card |
| A.1.4 | `/account/data-rights/download/{id:guid}` | Mr Smit opens the download address of the request he approved | Page not found, with nothing saying the request exists |
| A.1.5 | `/account/data-rights` → `/account/data-rights/download/{id:guid}` | Dr Dlamini downloads her export | Completed, and a Download named for the request |
| A.1.6 | `/account/data-rights` | Dr du Plessis asks for access, then withdraws it | Submitted with its Withdraw; then "Request withdrawn.", the focus on it, the row Withdrawn and offering nothing |
| A.1.7 | `/account/data-rights` | Dr Mahlangu asks for a correction | Submitted, with a Withdraw |
| A.1.8 | `/admin/data-rights` → `/admin/data-rights/{Id:guid}` | Mr Smit clears the filters, opens the withdrawn request, then rejects Dr Mahlangu's with a note | Three requests, newest first, with status badges; the withdrawn one offering no decision; "Request rejected." with the focus on it |
| A.1.9 | `/account/data-rights` | Dr Mahlangu reads the decision | Rejected, nothing offered. Today no note and no date; after T319 both |
| A.1.10 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit schedules a formative check-in for Dr Ndlovu | An open review for the erasure to end (a setup step; the page is F08's) |
| A.1.11 | `/account/data-rights` | Dr Ndlovu requests erasure | Accepted although a review is scheduled; Submitted, with a Withdraw |
| A.1.12 | `/admin/data-rights` → `/admin/data-rights/{Id:guid}` | `devadmin` filters to Erasure and approves with a note | Every institution's requests; after T264 a confirmation first; then Completed at once, with the Decision card |
| A.1.13 | `/account/session-ended` → `/account/login` | Dr Ndlovu's open tab, then a sign-in attempt | "Your session has ended. Please sign in again."; then "Invalid email or password.", with no mention of an erasure |
| A.1.14 | `/admin/users` → `/admin/trainees` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Prof Mbatha looks for Dr Ndlovu | No Dr Ndlovu among users or trainees; his reviews under `deleted_user_…`; the check-in Withdrawn with its reason and no action |
| A.2.3 | `/account/data-rights` | Mr Smit opts out of digest emails | "Processing preferences saved." |
| A.2.5 | `/account/data-rights` | Mr Smit opts back in | "Processing preferences saved." |
| A.5.7 | `/admin/data-rights/{Id:guid}` | Mr Smit opens a request id that is not his | One not-found state. Today "You are not authorized …" above "Not found" |
| A.7.3 | `/` → `/portfolio/progress` → `/activities/mine` → `/account/data-rights` | Dr Dlamini at 390 px | "Your requests" scrolls inside its container; each row's action stays reachable |
| A.7.5 | `/` → `/msf/campaigns` → … → `/admin/data-rights` | Mr Smit at 390 px | The queue's filters stack above its table, with Apply after them |

## States to design

27 captures, from `execution/knowledge/scenario-paediatrics/states.md`. The line number is the row in `states.md`. None
is held.

| Page slug | State | Screenshot (`design/baseline/…`) | Who | How it is reached (`states.md` line) |
|---|---|---|---|---|
| `data-rights` | Nothing requested | `states/data-rights--empty.png` | Dr Dlamini | At Step A.1.1, before saving: "No requests". (:220) |
| `data-rights` | Preferences saved | `states/data-rights--saved.png` | Dr Dlamini | At Step A.1.1. (:221) |
| `data-rights` | No reason given | `states/data-rights--reason-missing.png` | Dr Dlamini | At Step A.1.2, the first submit. (:222) |
| `data-rights` | Submitted | `states/data-rights--submitted.png` | Dr Dlamini | At Step A.1.2: the row with Withdraw. (:223) |
| `data-rights` | Completed, with Download | `states/data-rights--completed.png` | Dr Dlamini | At Step A.1.5. (:224) |
| `data-rights` | Withdrawn | `states/data-rights--withdrawn.png` | Dr du Plessis | At Step A.1.6. (:225) |
| `data-rights` | Rejected | `states/data-rights--rejected.png` | Dr Mahlangu | At Step A.1.9. (:226) |
| `data-rights` | Loading | `states/data-rights--loading.png` | Dr Dlamini | Hold a read, then Data Rights in the nav. No change. (:227) |
| `data-rights` | Narrow | `states/data-rights--narrow.png` | Dr Dlamini | At Step A.7.3. (:228) |
| `data-rights-requests` | A coordinator's queue | `states/data-rights-requests--coordinator.png` | Mr Smit | At Step A.1.3, filters applied. (:716) |
| `data-rights-requests` | Nothing matches | `states/data-rights-requests--empty.png` | Mr Smit | At Step A.1.3, before approving: type Erasure, applied: "No requests". No change. (:717) |
| `data-rights-requests` | Three requests | `states/data-rights-requests--three.png` | Mr Smit | At Step A.1.8. (:718) |
| `data-rights-requests` | Every institution | `states/data-rights-requests--administrator.png` | devadmin | At Step A.1.12. (:719) |
| `data-rights-requests` | Loading | `states/data-rights-requests--loading.png` | Mr Smit | Hold a read, then Data Rights Requests in the nav. No change. (:720) |
| `data-rights-requests` | Narrow | `states/data-rights-requests--narrow.png` | Mr Smit | At Step A.7.5. (:721) |
| `data-rights-request` | Submitted | `states/data-rights-request--submitted.png` | Mr Smit | At Step A.1.3, before approving. (:722) |
| `data-rights-request` | No decision note | `states/data-rights-request--note-required.png` | Mr Smit | At Step A.1.3: "A decision note is required." (:723) |
| `data-rights-request` | Approved | `states/data-rights-request--approved.png` | Mr Smit | At Step A.1.3. (:724) |
| `data-rights-request` | Withdrawn | `states/data-rights-request--withdrawn.png` | Mr Smit | At Step A.1.8. (:725) |
| `data-rights-request` | Rejected | `states/data-rights-request--rejected.png` | Mr Smit | At Step A.1.8. (:726) |
| `data-rights-request` | Erasure approved | `states/data-rights-request--erasure.png` | devadmin | At Step A.1.12. (:727) |
| `data-rights-request` | Not his | `states/data-rights-request--not-found.png` | Mr Smit | At Step A.5.7. (:728) |
| `data-rights-request` | Narrow | `states/data-rights-request--narrow.png` | Mr Smit | At Step A.1.8, at 390 px. No change. (:729) |
| `not-found` | From a data-rights download that is not his | `states/not-found--download.png` | Mr Smit | At Step A.1.4. (:159) |
| `users-list` | After an erasure | `states/users-list--after-erasure.png` | Prof Mbatha | At Step A.1.14. (:641) |
| `reviews-schedule` | After an erasure | `states/reviews-schedule--pseudonym.png` | Prof Mbatha | At Step A.1.14: `deleted_user_…`, one Withdrawn. (:354) |
| `review-detail` | Withdrawn by an erasure | `states/review-detail--withdrawn.png` | Prof Mbatha | At Step A.1.14. (:385) |

Four states have no capture, because they do not exist yet. Design them from the requirements:
- **The Correction card**: empty, one correction recorded, Complete refused, completed (T316; BRIEF § 7 B3).
  `states/data-rights-request--approved.png` shows an approved request with no further action; an approved
  Rectification looks the same today.
- **The erasure confirmation dialog** (T264's note of 2026-09-25).
- **A rejected request showing its decision date and note** to the requester (T319). `states/data-rights--rejected.png`
  shows today's bare "Rejected".
- **A request's load error** (BRIEF § 6 A6).

## Attach

Every path is under `design/baseline/` and was checked to exist on 2026-09-26. Open each before uploading (BRIEF
§ 3.3). They show scenario addresses and ids only; none shows a password or a registration link.

**First, with the ask (the KEY SCREENSHOTS):**
- `states/data-rights--submitted.png`
- `states/data-rights--completed.png`
- `states/data-rights-requests--coordinator.png`
- `states/data-rights-request--note-required.png`
- `states/data-rights-request--erasure.png`
- `states/data-rights-request--approved.png`
- `act-A/A.1.7-1-mahlangu-rectification-submitted.png`
- `act-A/A.1.13-1-ndlovu-session-ended.png`
- `states/reviews-schedule--pseudonym.png`
- `states/data-rights-requests--narrow.png`
- `states/data-rights--rejected.png`

**Then, as the chat asks, by screen:**
- The requester: `states/data-rights--empty.png`, `states/data-rights--saved.png`,
  `states/data-rights--reason-missing.png`, `states/data-rights--withdrawn.png`, `states/data-rights--loading.png`,
  `states/data-rights--narrow.png`, `act-A/A.1.1-1-dlamini-preferences-saved.png`,
  `act-A/A.1.2-1-export-no-reason-refused.png`, `act-A/A.1.2-2-export-submitted.png`,
  `act-A/A.1.5-1-dlamini-completed-download.png`, `act-A/A.1.6-1-duplessis-access-submitted.png`,
  `act-A/A.1.6-2-duplessis-withdrawn.png`, `act-A/A.1.9-1-mahlangu-reads-rejected.png`,
  `act-A/A.1.11-1-ndlovu-erasure-submitted.png`, `act-A/A.2.3-1-smit-digest-opt-out.png`,
  `act-A/A.2.5-1-smit-digest-opt-in.png`, `act-A/A.7.3-5-data-rights-390.png`.
- The queue: `states/data-rights-requests--empty.png`, `states/data-rights-requests--three.png`,
  `states/data-rights-requests--administrator.png`, `states/data-rights-requests--loading.png`,
  `act-A/A.1.3-1-smit-queue-filtered.png`, `act-A/A.1.8-1-smit-queue-three.png`,
  `act-A/A.1.12-1-devadmin-queue-erasure.png`, `act-A/A.1.12-3-queue-after-erasure.png`,
  `act-A/A.7.5-5-data-rights-390.png`.
- A request: `states/data-rights-request--submitted.png`, `states/data-rights-request--withdrawn.png`,
  `states/data-rights-request--rejected.png`, `states/data-rights-request--not-found.png`,
  `states/data-rights-request--narrow.png`, `act-A/A.1.3-2-export-approved-completed.png`,
  `act-A/A.1.8-2-mahlangu-rectification-rejected.png`, `act-A/A.1.8-3-duplessis-withdrawn-detail.png`,
  `act-A/A.1.12-2-erasure-approved.png`, `act-A/A.5.7-1-request-not-found.png`.
- The aftermath: `states/not-found--download.png`, `states/users-list--after-erasure.png`,
  `states/review-detail--withdrawn.png`, `act-A/A.1.4-1-smit-download-not-found.png`,
  `act-A/A.1.13-2-ndlovu-signin-refused.png`, `act-A/A.1.14-1-users-no-ndlovu.png`,
  `act-A/A.1.14-2-trainees-no-ndlovu.png`, `act-A/A.1.14-3-reviews-pseudonym.png`,
  `act-A/A.1.14-4-checkin-withdrawn.png`.

**Do not attach** `act-A/A.7.3-1-home-390.png` or `act-A/A.7.5-1-home-390.png`. They are Home captures from this
flow's phone steps, not pages this flow designs (re-captured after T297 on 2026-09-26; BRIEF § 10).

## Known problems this design must solve

| Task | Priority | What it means for the design | Evidence |
|---|---|---|---|
| T316 | P2 | Decide who rectifies (recommended: whoever approves; `RectificationRoles` folds into `ReviewRoles`, since today the Coordinator who approves cannot apply and the SpecialityAdmin who may apply cannot open the request). Then the Correction card; then the requester reads Completed. A new runbook step plays it end to end | `states/data-rights-request--approved.png`; `coverage.md` § Flows and states not played ("Applying and completing an approved data-rights rectification") |
| T324 | P3 | One label helper for type and status on filters, badges and row names (`RequestsList.razor:25-27,35-37` print enum names today). The Type filter offers the four submittable types | `states/data-rights-requests--coordinator.png` |
| T319 | P2 | The requester's list carries the decision date and note (`DataRightsRequestSummaryDto` has neither today); not the decider. A decision notice is sent after the save, with no note; an erasure sends a confirmation, read before the erasure runs | `states/data-rights--rejected.png`, `act-A/A.1.9-1-mahlangu-reads-rejected.png` |
| T320 | P2 | The digest help (`DataRights.razor:161-165`) names only the one-off mail that exists, once T320 decides which activity and committee mails to send | `states/data-rights--rejected.png` (the help text is visible) |
| T318 | P3 | Remove the optional-processing opt-out and its column; reword the card; one result per action | `states/data-rights--saved.png`, `states/data-rights--reason-missing.png` |
| T264 | P3 | The erasure's approval asks first, counting what it will withdraw and saying it cannot be undone (T264 note, 2026-09-25). Destructive triggers are outline buttons | `states/data-rights-request--erasure.png` |
| T323 | P3 | At 1280 px the Request card is about 315 px wide and clips the address and the user id once the Decision card sits beside it | `states/data-rights-request--submitted.png`, `--approved.png`, `--erasure.png` |
| T328 | P3 | "Apply filters" is a page-header action today, so it comes before the filters in reading and tab order; at 390 px it sits above them. Textareas render in monospace | `states/data-rights-requests--narrow.png`, `act-A/A.7.5-5-data-rights-390.png` |
| T325 | P3 | `DataRights.razor:108,137`, `RequestsList.razor:59` and `RequestDetail.razor:41,54` print stored UTC with no zone. T325's rule: SAST with the zone; these pages may keep the stored value where it is the record, but labelled | `act-A/A.1.2-2-export-submitted.png` |
| T276 | P3 | Erasure leftovers the confirmation should not hide (Q1). The request keeping the erased address is F-A.1.12a, "unsure" | `states/data-rights-requests--administrator.png` |
| T329 (BRIEF § 6 A6) | P2 | A6 applies T329's rule to every page: a skeleton while loading, the alert on a load error with no empty state under it, and no action offered before the record has loaded | `states/data-rights--loading.png`, `states/data-rights-requests--loading.png` |

## Questions the design must answer

1. **How is an erasure's finality made unmistakable before approval?** What goes and what is kept is in
   `execution/architecture/CUSTOMIZATION.md` § 5 ("Erasure ends what is open about the person and keeps what is
   settled", T258), summarised in the ask. The approval and the erasure are one transaction (A.1.12). T264's note asks
   the dialog to count the open reviews (including other institutions' and College panels') and campaigns it will
   withdraw. T276 lists what the erasure does not yet reach.
2. **Does the requester's page separate preferences from requests, or keep them on one page?** Today they share one
   page in three parts (A.1.1; `states/data-rights--empty.png`). The digest opt-out is the only preference once T318
   removes the other, and every user holds it, staff included (A.2.3).

## Notes

- **Ids stay visible on purpose** on the data-rights list and on a request, because the id is the record there
  (DESIGN.md:1699-1702; BRIEF § 6 A8, one of its exceptions). Design them to wrap, not to hide. On the requester's own
  page an id means nothing (T319 § What to build).
- **The download endpoint** (`/account/data-rights/download/{id:guid}`) answers the data subject or a global
  Administrator only. Anyone else gets Page not found, with nothing saying the request exists (A.1.4; T112).
- **Session ended** (A.1.13) and **Page not found** (A.1.4) are framework states designed in F01 and F02 (BRIEF § 5.1).
  This flow only shows them in context.
- **The withdrawn review** (A.1.14) still renders the live "Entrustment against Annexure A" card for the pseudonym. The
  replay noted it but did not file it (A.1.14 Gap); the review page itself is F07's.

## Acceptance

After Claude Code builds the chosen design (BRIEF § 9), the flow is done when:
- **The steps replay on a fresh database.** Play Acts 1–6, then the appendix's **A.1.1–A.1.14**, **A.2.3**, **A.2.5**,
  **A.5.7**, **A.7.3** and **A.7.5** (`README.md` § How to play; `tools/scenario-replay.ps1 create|publish|start
  wombat_scenario_f14`). Every Expect holds. Where the design changes wording, the steps' Expect lines change in the
  same task (BRIEF § 9, item 7). Expect to rewrite A.1.1 (no optional-processing box), A.1.2 (the promise), A.1.3 and
  A.1.8 (labels), A.1.9 (the note and date shown), A.1.12 (the confirmation) and A.5.7 (one not-found state). T316 adds
  a new step that approves, corrects on the owning page, records the correction and completes the request; A.1.8 keeps
  its rejection.
- **The tests pass:** `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`, including
  `Privacy/DataRightsDigestOptOutTests` (which pins the digest help), the `Accessibility` and `Design` tests, and T294's
  guard in `tests/Wombat.Web.Tests/Scenario/`.
- **The baseline is re-captured:** the 27 states above and the step captures of the 19 steps, compared with the chosen
  artboards.
- **The browser check passes** at 1280 and 390 px for each person in the flow: a trainee requester, Mr Smit, `devadmin`
  and Prof Mbatha.

## The runbook steps, verbatim

Paste this as the second message. It is each step's Role, Route, Do and Expect, copied from
`execution/knowledge/scenario-paediatrics/appendix-cross-cutting.md`. `D` is the replay day.

```text
Step A.1.1 — Dr Dlamini reads her data rights and sets a preference (appendix-cross-cutting.md)
Role: Trainee — Dr Anele Dlamini
Route: / → /account/data-rights
Do: Open My data rights, under the rule at the foot of her menu. Read the page, tick "Opt out of optional processing",
  and save her preferences.
Expect: The page has three parts: processing preferences, a request form, and "Your requests", which is empty ("No
  requests"). The digest opt-out's help names the three reminders it stops (the weekly coordinator digest, the 14-day
  draft reminder and the 5-day assessment reminder), and says that email about one particular thing is still sent
  (T240). Saving says "Processing preferences saved." and the focus moves to that message (T234).

Step A.1.2 — Dr Dlamini requests an export of her data (appendix-cross-cutting.md)
Role: Trainee — Dr Anele Dlamini
Route: /account/data-rights
Do: Choose the request type Export and submit it with no reason. Then submit it again with the reason "A copy of my
  training record for my own files."
Expect: The form offers four types: Access, Export, Rectification and Erasure. With no reason the request is refused
  ("Please provide a reason for your request.") and nothing is sent. The second submit says the request was submitted
  and that she will be notified when it is reviewed. "Your requests" lists it: today, Export, Submitted, with a
  Withdraw button named for it and no Download.

Step A.1.3 — Mr Smit approves the export from KGK's queue (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights → /admin/data-rights/{Id:guid}
Do: Open Data rights requests from the menu. Narrow it to type Export and status Submitted, apply the filters, and
  review Dr Dlamini's request. Press Approve with no decision note. Then approve it with the note "Identity confirmed;
  export released to the data subject."
Expect: The queue holds only KGK's requests (T112), and the filters change it only when they are applied. On the
  request's page Data rights requests stays lit, and the trail reads Home › Data rights requests › the request's id
  once it has loaded (R2-Rules § 3; DESIGN.md's owner table).
  - The row names the requester by the address she signs in with, and its Review action is named for the row (T239).
  - The detail page shows the requester, her user id, when it was submitted, its type, its status and her reason.
  - Approving with no note is refused: "A decision note is required."
  - Once approved, the request reads Completed at once, because an export is built when it is downloaded. A Decision
    card appears (decided by, as a user id; decided on; the note; completed on), and Approve and Reject are gone.

Step A.1.4 — Mr Smit cannot download what he approved (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights/download/{id:guid}
Do: Open the download address of the request he has just approved. Its id is in the detail page's address.
Expect: Page not found, with status 404. Nothing on the page says that the request exists. The reviewer approves; only
  the data subject, or a global Administrator, collects the bundle (T112).

Step A.1.5 — Dr Dlamini downloads her export (appendix-cross-cutting.md)
Role: Trainee — Dr Anele Dlamini
Route: /account/data-rights → /account/data-rights/download/{id:guid}
Do: Reload My data rights and download the completed request.
Expect: The row reads Completed and offers a Download named for the request.
  - The browser saves `data-export-<date>-<time>.zip`.
  - It holds `data-export.json`: schema version 2, with her profile, every activity she is the subject or author of,
    her committee reviews, the MSF campaigns about her, her progress by semester, her newest 500 audit entries and her
    portfolio exports.
  - It also holds `portfolio-summary.pdf`.
  - No email is sent.

Step A.1.6 — Dr du Plessis asks for access, then withdraws the request (appendix-cross-cutting.md)
Role: Trainee — Dr Pieter du Plessis
Route: /account/data-rights
Do: Submit an Access request with the reason "Everything held about me." Then withdraw it from "Your requests".
Expect: The request lists as Submitted, with a Withdraw named for its type and time. Once withdrawn, the page says
  "Request withdrawn.", the focus moves to that message, and the row reads Withdrawn and offers nothing.

Step A.1.7 — Dr Mahlangu asks for a correction (appendix-cross-cutting.md)
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Submit a Rectification request with the reason "My programme start is recorded as 15 January 2026; I started work
  on 19 January."
Expect: The request lists as Submitted, with a Withdraw.

Step A.1.8 — Mr Smit reads the queue and rejects the correction (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights → /admin/data-rights/{Id:guid} → /admin/data-rights/{Id:guid}
Do: Clear both filters and apply them. Open Dr du Plessis's withdrawn request, then Dr Mahlangu's. Reject hers with the
  note "The whole January 2026 intake is admitted on 15 January, the semester boundary (D42). Nothing to correct."
Expect: The queue lists all three KGK requests, newest first, each with its status badge.
  - The withdrawn request's page shows the request and offers no decision.
  - Dr Mahlangu's reads Rejected once rejected, with its Decision card, and the focus moves to "Request rejected."

Step A.1.9 — Dr Mahlangu reads the decision (appendix-cross-cutting.md)
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Reload My data rights and read her request.
Expect: The request reads Rejected and offers nothing: no Withdraw, no Download. The page shows neither the decision
  note nor who decided, and no email told her of the decision.

Step A.1.10 — Mr Smit schedules a formative check-in for Dr Ndlovu (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule a review of Dr Ndlovu before the Paed Annual Review Panel for the period holding `D`, Scheduled on `D`,
  with Formative only ticked. Note the review's id from its address.
Expect: The review opens with Mode "Formative (interim check-in, no binding decision)" and State Scheduled, as at Step
  4.49. It is open, so the erasure at A.1.12 has a review to end.

Step A.1.11 — Dr Ndlovu requests erasure (appendix-cross-cutting.md)
Role: Trainee — Dr Sipho Ndlovu
Route: /account/data-rights
Do: Submit an Erasure request with the reason "I am leaving the programme and want my personal data removed." Stay
  signed in in this browser.
Expect: The request is accepted although a review of him is scheduled: no review's state refuses an erasure request
  (T258). It lists as Submitted, with a Withdraw.

Step A.1.12 — devadmin approves the erasure (appendix-cross-cutting.md)
Role: Administrator — devadmin
Route: /admin/data-rights → /admin/data-rights/{Id:guid}
Do: Narrow the queue to Erasure and apply the filter, review Dr Ndlovu's request, and approve it with the note "Identity
  confirmed. Programme records are kept under a pseudonym."
Expect: The Administrator's queue holds every institution's requests.
  - Once approved, the request reads Completed at once, with its Decision card. The approval and the erasure are one
    transaction (T258).
  - No confirmation is asked before this irreversible action (noted on T264).
  - The queue and the request still name the requester by the address he signed in with: the erasure keeps the request
    as it was submitted (`ErasureExecutor`).

Step A.1.13 — Dr Ndlovu's open session ends, and he cannot sign in again (appendix-cross-cutting.md)
Role: Trainee — Dr Sipho Ndlovu (the browser left signed in at A.1.11)
Route: /account/session-ended → /account/login
Do: Wait on any page for up to a minute. Then sign in with his old address and password.
Expect: The tab leaves for the sign-in page by itself, which says "Your session has ended. Sign in again." (T279), an
  information notice. Signing in is refused in the words an unknown address gets, "Invalid email or password.", with
  no mention of an erasure (T156). No email is sent to him.

Step A.1.14 — What the erasure left, as KGK sees it (appendix-cross-cutting.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/trainees → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Look for Dr Ndlovu among KGK's users, its trainees and its committee reviews. Open the check-in from A.1.10.
Expect: No Dr Ndlovu remains among KGK's users or trainees (T026, T258).
  - Committee reviews lists his reviews under a pseudonym, `deleted_user_…`, that names nobody: Act 4's ratified
    review and closed check-in as they were, and A.1.10's check-in as Withdrawn.
  - The check-in reads Withdrawn, dated today, with the reason "Withdrawn because the trainee's personal data was erased
    at their request. Nothing more is decided at this review.", and offers no action.
  - His activities and progress stay under the same pseudonym; the account itself is checked in the outcome's SQL.

Step A.2.3 — Mr Smit opts out of digest emails (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Tick "Opt out of digest emails" and save his preferences.
Expect: "Processing preferences saved."

Step A.2.5 — Mr Smit opts back in (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Untick "Opt out of digest emails" and save.
Expect: "Processing preferences saved."

Step A.5.7 — A data-rights request that is not his, by id (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights/{Id:guid}
Do: Open the address of Dr Mahlangu's request with its last digit changed.
Expect: The refusal "You are not authorized to access this data-rights request." and the empty state "Not found". An
  unknown id and another institution's request read the same, so the page does not tell them apart (T112).

Step A.7.3 — Dr Dlamini on her phone (appendix-cross-cutting.md)
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/progress → /activities/mine → /account/data-rights
Do: At 390 px, sign in and open her dashboard, My progress, My activities and My data rights from the menu.
Expect: The dashboard's cards stack. On My progress, each EPA's figures and trajectory fit the width. My activities'
  rows stack, each cell but the link and the state labelled by its column (T342), and the table of "Your requests"
  scrolls inside its container; each row's link or action stays reachable.

Step A.7.5 — Mr Smit on his phone (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions due and the data-rights queue.
Expect: The campaign page keeps its gutter (T226), and its invitee table (counts by respondent group: the page never
  lists an address) scrolls within its card. Decisions due's summary scrolls sideways, and a keyboard can scroll it, as
  a labelled, focusable region (DESIGN.md's decisions-due contract). The queue's filters stack above its table.
```
