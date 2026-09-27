# F09 What the committee issued: STARs, the register, a revocation and an appeal

This flow covers four jobs. Registrars read the committee's decision, their STARs and their certificates, and may appeal.
The panel's appeal body resolves an appeal. Programme staff keep the STAR register and revoke a STAR issued in error. It
is flow 09 of 18 (`design/BRIEF.md` § 8).

| | |
|---|---|
| **Mode** | Straight to fidelity for the STAR register (screen 4) and My authorisations (screen 1). Wireframe first for the trainee's review page (screen 2), whose decision history and Appeals block are new (BRIEF § 7 B5), and for the committee's Appeals card (screen 3), which shares that history. |
| **Viewports** | Desktop 1280×800 and phone 390×844. |
| **Held** | Nothing held. T307 landed in d03732d (D51), and its appeal captures were re-taken on 2026-09-26 (§ Attach; BRIEF § 10). `states/review-detail--appeal-upheld.png` shows an outcome that no longer exists: do not attach it. |
| **Frequency** | After each sitting, which is a review each semester or year (BRIEF § 8). Appeals are rare. |
| **Stakes** | High. Resolving an appeal is irreversible: "the review goes Final and nothing reopens it" (T307). The certificate is the medico-legal record, and it leaves Wombat (T310, citing DOMAIN.md:78). |
| **People** | Trainees: Dr Lerato Molefe, Dr Anele Dlamini, Dr Nomsa Mahlangu and Dr Sipho Ndlovu. The committee: Dr Thandi Zulu (chair), Dr David Naidoo and Dr John van Rensburg (external). Dr Refilwe Mokoena (SpecialityAdmin) revokes. Dr Kabelo Sithole (SubSpecialityAdmin) and Prof Nolwazi Mbatha (InstitutionalAdmin) read the register (`scenario-paediatrics/README.md` § Cast). |

**How to use this brief**

1. Open a new Claude Design thread for this flow. The design system must be set up first, and flow 01 settled (BRIEF
   § 2.3, steps 1–3).
2. Paste the block under **The ask**, then the block under **Appendix: the runbook steps**.
3. Attach the images listed under **Attach**, key screenshots first. Open each one before you upload it (BRIEF § 3.3).
4. Answer Claude's questions in the chat, each as a sentence. The chat travels in the handoff bundle [academy].
5. Export the chosen artboards to `design/flows/09-outcomes-stars-appeals/` before moving on. There is no version
   history [start].

## The ask

```
FLOW 09 — Read what the committee issued, appeal it, resolve the appeal, and keep the STAR register

DIRECTION: <paste the aesthetic direction recorded as W-nnn in execution/DECISIONS.md (design/BRIEF.md § 2.3 step 0)>
DESIGN SYSTEM: Wombat, as set up for this project:
  - the :root tokens of app.css;
  - Fraunces on the wordmark only;
  - Lucide line icons;
  - the components named in CONSTRAINTS.
  These screens sit inside the shell settled in flow 01 (sidebar nav and top row). Do not redesign the shell here.

GOAL: Registrars read their committee decision, their STARs (Statements of Awarded Responsibility, each the
  committee's entrustment of an EPA at a level) and their certificates, and may appeal a decision. The panel's chair
  or external member resolves an appeal. A replacement decision needs a quorum. Programme staff read the STAR
  register and revoke a STAR issued in error. What is wrong today:
  - The appellant never sees her appeal, the decision it replaced, or any decision's conditions.
  - After she lodges an appeal, the list row still reads Ratified while the detail reads Under appeal.
  - A revoked STAR vanishes from the trainee's page. The revoke confirmation promises a notice that nobody sends.
  - The appeal's Outcome select opened on Dismissed, so resolving without choosing dismissed the appeal. T307 fixed it:
    it opens on "Select an outcome…", a remit records its conditions, and there is no Upheld (D51).
  - No nav item leads to the register. Only the InstitutionalAdmin's dashboard links to it.

AUDIENCE:
  - Trainees:
    - Dr Lerato Molefe: final year, with 3 STARs and later 15.
    - Dr Anele Dlamini: one of her STARs is revoked.
    - Dr Nomsa Mahlangu: appeals an Inadequate Progress decision.
    - Dr Sipho Ndlovu: has an annual review and a formative check-in.
  - CommitteeMembers:
    - Dr Thandi Zulu, the chair;
    - Dr David Naidoo, a member;
    - Dr John van Rensburg, the external member.
  - Programme staff:
    - Dr Refilwe Mokoena, SpecialityAdmin, who revokes;
    - Dr Kabelo Sithole, SubSpecialityAdmin;
    - Prof Nolwazi Mbatha, InstitutionalAdmin.
  - Frequency: after each sitting. Appeals are rare.
  - Stakes: high. Resolving an appeal cannot be undone, and a certificate leaves the system.
  - Viewports: desktop 1280×800 and phone 390×844.

SCREENS, in order:
  1. /portfolio/authorisations: My authorisations (Trainee).
     - One card per STAR in force, showing:
       - the EPA code and title, and the authorised level;
       - the issue date, and the expiry ("No expiry", or the date with "Expires in N days");
       - the committee's rationale;
       - "Download certificate", named by its EPA.
     - NEW: a "No longer in force" section listing revoked and expired STARs, each with its status, date and
       certificate.
     - States: none yet; one expiring; fifteen; after a revocation.
  2. /committee/my-reviews: My committee reviews (Trainee).
     - A list: Panel, Period, Type, State and Decision, with a View link named by its review.
     - The selected review's detail:
       - sits for, evidence window, type and state;
       - the decision history;
       - the agenda: EPA, window and outcome. The outcome is Decided "STAR #n.", Deferred "The committee's reason: …",
         or "Not decided at this review.";
       - while an appeal may be lodged, the appeal form: Appeal reason and Lodge appeal.
     - NEW:
       - Every decision, newest first. A replaced decision is marked "Replaced on appeal by the decision above.".
         Each shows its rationale, its conditions and who was present. Use ONE decision-history component, shared
         with screen 3's page.
       - An Appeals block: date lodged, reason, outcome in words, and date resolved.
     - States: none yet; a decision; the appeal form; under appeal; remitted; two reviews (annual and formative
       check-in); pre-graduation.
  3. The Appeals card on /committee/reviews/{ReviewId:int} (CommitteeMember).
     - A member's read-only view: "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and
       its external member, John van Rensburg."
     - The appeal body's resolve form:
       - Outcome, opening on "Select an outcome…", then "Dismissed: the decision stands" and "Remitted: the appeal
         body replaces the decision", with help that says what each does to the decision. There is no Upheld (D51).
       - Resolving with no outcome says "Choose an outcome." under the select.
       - Remitted reveals Replacement category ("Select a category…"), Replacement rationale, Replacement conditions
         (optional), and Present, with the chair and the resolver ticked and locked.
     - States:
       - the form;
       - no outcome chosen: "Choose an outcome.";
       - the member's view;
       - a remit refused for want of a quorum;
       - remitted: the replacement heads the history, with its conditions;
       - dismissed.
  4. /admin/entrustment-decisions: the STAR register (SpecialityAdmin, SubSpecialityAdmin, InstitutionalAdmin).
     - Filters: Trainee name and Status, with Apply.
     - A table: Trainee, EPA, Level, Issued, Expires and Status, and the actions Download and Revoke. Each action is
       named by trainee, EPA and issue date.
     - A revoke confirmation that names the EPA and the trainee and requires a reason.
     - States: none issued; issued; filtered; the revoke confirmation; revoked; superseded; a paused EPA, with "(no
       longer in use)" after its title.
  Also on the way, from other flows:
  - /access-denied, when a trainee types the committee's address for her own review (flow 01's page);
  - Decisions Due's "Revoked: re-decide" line, where a revocation leads (flow 08's page).

STEPS: 4.35, 4.36, 4.37, 4.39, 4.41, 4.42, 4.43, 4.44, 4.45, 4.46, 4.47, 4.48, 4.51, 5.7, 5.8, 6.22 and A.7.7.
  - They are pasted verbatim after this block (Role / Route / Do / Expect).
  - The Expect lines describe the product as replayed on 2026-09-26. Where a requirement below differs, the
    requirement wins.

STATES TO SHOW:
  - Every state listed under SCREENS.
  - For each page: loading (a skeleton under the header), the load error, and narrow (390 px).
  - Data volumes:
    - none;
    - typical: 3 STARs and one review;
    - heavy: 15 STARs plus a superseded one, 15 agenda lines, and two reviews in one period.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T307 (built, d03732d; keep it):
    - The Outcome select opens on "Select an outcome…". Nothing is resolved until an outcome is chosen ("Choose an
      outcome.").
    - Each outcome is named in words, with help text.
    - Remitted reveals "Replacement conditions".
    - There are two outcomes, "Dismissed: the decision stands" and "Remitted: the appeal body replaces the decision".
      Upheld is gone (D51).
    - A remit's conditions show as "Conditions: …" on the replacement.
    - The chosen outcome must be readable in full: today it is clipped at 1280 px once Remitted opens the second column
      (F-4.45c).
    - A refusal of Resolve appeal (the quorum) must be seen where the button was pressed; today it shows only at the
      head of the page (F-4.46a).
  - T308:
    - After she lodges an appeal, her list row reads Under appeal at once.
    - She sees every decision, with its conditions and who was present.
    - She sees her appeals: the date lodged, the reason, the outcome in words and the date resolved.
    - One component draws the decision history on her page and on the committee's.
  - T319:
    - My authorisations gains "No longer in force": revoked and expired STARs, each with its status, date and
      certificate. A superseded STAR stays off.
    - The revoke confirmation says only what is true: "The trainee is emailed." once that notice is built, and no
      promise before.
  - T264 and T260:
    - Revoke is an outline button in the row, named per row.
    - It opens a confirmation dialog that names the trainee and the EPA.
    - Confirm revocation is enabled as the reason is typed, not when the box loses focus.
    - The result takes the focus.
  - T324: a revocation names the person who revoked by name, never by user id. The certificate prints an id today.
  - T325: "Expires in N days" counts on the South African calendar. Any time is shown in SAST, with the zone.
  - T310: the certificate names the institution of the review that issued it. The PDF is not a screen here.

REQUIREMENTS EVERY WOMBAT PAGE MEETS (design/BRIEF.md § 6; the ones this flow tests):
  A1  Contrast in the tokens: the badges, the "Expires in" tint, and the success and warning alerts.
  A5  Every action reports its outcome where the focus lands. A destructive action confirms and names its target.
  A6  Loading, load-error and not-found states are designed. No action is offered before the record has loaded.
  A8  People by name; states and outcomes by label.
  A9  One clock, labelled SAST.
  A11 Offer only what the caller can do:
      - no Revoke on a revoked or superseded row;
      - no appeal form where no appeal may be lodged;
      - no resolve form for a member outside the appeal body.
  A12 The nav marks where you are.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Where do certificates live for the trainee: on My authorisations only, or also on the review that issued them?
  2. Does the trainee's review page reuse the committee's decision-history component exactly? (Recommended.)
  3. Should the speciality admins get a nav link to the STAR register, and the Trainee a nav link to My authorisations?
     Today the register is reached only by address or a dashboard card, and My authorisations only from a dashboard
     card.
  4. How does the trainee's list tell a formative check-in from an annual review? It has no Mode column today; only
     "No binding decision" and the View link's name mark it.

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
  - 2–3 variations.
  - Full fidelity for screens 1 and 4. Wireframes first for screens 2 and 3, then fidelity for the one I pick.
  - Name every design-system component you use, and mark anything else NEW.
  - Say which DESIGN.md rule a variation breaks.
  - Flag edge cases, and review each screen's accessibility against A1–A12 above.

ATTACHED (key screenshots first):
  - states/my-authorisations--expiring.png, my-reviews--detail.png, my-reviews--appealed.png, my-reviews--remitted.png,
    my-authorisations--after-revocation.png, entrustment-decisions--revoke-form.png, entrustment-decisions--superseded.png;
  - act-4/4.41-2-molefe-committee-page-denied.png.
  - A full-page capture draws the sidebar and top row part-way down a long page. That is how the capture stitched the
    sticky frame, not how the product looks.
```

## The journey

Each row is one runbook step. The Route column gives the step's page templates (`coverage.md` § Pages).

| Step | Route | Who does what | What they must be able to see |
|---|---|---|---|
| 4.35 | `/` → `/admin/entrustment-decisions` | Prof Mbatha opens the register from her dashboard's quick links. She downloads Dr Molefe's PAED-010 certificate, then filters by "Molefe". | Five Active rows, issued `D`. The certificate: institution, level, the issue and expiry dates, rationale, evidence, panel and chair. The filter leaves three rows. |
| 4.36 | `/admin/entrustment-decisions` | Dr Mokoena, arriving by typed address, revokes Dr Dlamini's PAED-002 with a reason. | A confirmation naming PAED-002 and Anele Dlamini, then "Entrustment decision for PAED-002 revoked.". The row reads Revoked and offers no Revoke. |
| 4.37 | `/admin/entrustment-decisions` → `/committee/decisions-due` | Dr Sithole filters by status Revoked, then Active. He downloads the revoked certificate and opens Decisions Due. | One Revoked row, then four Active. The certificate names the reviser (by id today: T324). "Revoked: re-decide", with Schedule. |
| 4.39 | `/` → `/portfolio/authorisations` | Dr Molefe opens My authorisations from her dashboard card and downloads PAED-001. | Three cards. PAED-010 reads "Expires in 20 days". Each download is named by its EPA. |
| 4.41 | `/committee/my-reviews` → `/committee/reviews/{ReviewId:int}` → `/access-denied` | Dr Molefe reads her review, then types the committee's address for it. | The decision and who was present. The agenda's three kinds of outcome. Then Access denied. |
| 4.42 | `/` → `/portfolio/authorisations` → `/committee/my-reviews` | Dr Dlamini reads what stands after the revocation. | One card, PAED-001. PAED-002 has simply gone (T319). Her agenda still reads PAED-002 Decided "STAR #5.", because it records what the sitting did. |
| 4.43 | `/committee/my-reviews` | Dr Mahlangu reads her Inadequate Progress decision and lodges an appeal. | Who was present; twelve lines Deferred and three Not decided; the Appeal reason form. Then "Appeal lodged." and Under appeal, in the list row too (T308). |
| 4.44 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Naidoo, a member, reads the appeal. | The Appeals card: the date, the reason and "(Open)", and who may resolve it. No form. |
| 4.45 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr van Rensburg, the external member, reads the resolve form, presses Resolve appeal with no outcome, chooses Remitted, and leaves. | The Outcome select, opening on "Select an outcome…" (Dismissed and Remitted, in words). "Choose an outcome." under it when none is chosen. The remit's fields, with an optional Replacement conditions. Present, with the chair and himself locked. Nothing changes until he submits (T307). |
| 4.46 | `/committee/reviews/{ReviewId:int}` | Dr Zulu remits with only herself present. | The quorum refusal. The review stays Under appeal, and the form keeps its values. |
| 4.47 | `/committee/reviews/{ReviewId:int}` | Dr Zulu remits with conditions and Dr Botha present. | "Appeal resolved." and Closed. The replacement heads the history, with "Conditions: …" (T307). The first decision reads "Replaced on appeal by the decision above.", and the appeal "(Remitted)". |
| 4.48 | `/committee/my-reviews` | Dr Mahlangu reads the outcome. | Closed; Satisfactory with Observations; who was present. After T308, also her appeal, as Remitted, and the decision it replaced. |
| 4.51 | `/committee/my-reviews` | Dr Ndlovu reads both of his reviews. | Two rows for the period: Ratified, "Outcome Deferred"; and the check-in, Closed, "No binding decision". |
| 5.7 | `/` → `/admin/entrustment-decisions` | Prof Mbatha filters by Molefe, first with status Active and then All. | Fifteen Active rows, each with Download and Revoke. With All, Act 4's PAED-010 STAR reads Superseded and offers Download only. |
| 5.8 | `/committee/my-reviews` → `/` → `/portfolio/authorisations` | Dr Molefe reads her pre-graduation review and downloads a certificate to keep. | Pre-graduation, Ratified, Graduate. Every EPA reads Decided, with its STAR number. The appeal form is offered. Fifteen cards, each reading "No expiry". |
| 6.22 | `/admin/epas` → `/admin/curricula` → `/admin/curricula/{Id:int}/items` → `/admin/entrustment-decisions` | Prof Mbatha, after the College pauses PAED-012, reads Dr Molefe's STARs. | Her fifteen are still Active. PAED-012's title ends "(no longer in use)" (T255): a pause leaves a STAR standing. |
| A.7.7 | `/` → `/committee/panels` → `/committee/decisions-due` → `/admin/entrustment-decisions` | Dr Mokoena works on her phone. | The register at 390 px, with its table scrolling inside its container (T226). |

The steps' notes say three more things the design must know:
- **The register has no nav link.** No nav item or dashboard card leads there for a speciality admin (Step 4.36's note;
  `coverage.md` § Reached only by address).
- **My authorisations requires the Trainee role.** A graduate loses the page at Step 5.22 (Step 5.8's note). The
  graduate's home is flow 13 (BRIEF § 7 B1, T311).
- **The revoke confirmation is not a dialog today.** It is an inline section below the table (observed:
  `states/entrustment-decisions--revoke-form.png`; Step 4.36's Actual).

## States to design

The states are from `scenario-paediatrics/states.md` § Portfolio, § Committee and § Access denied by page. Each is
`design/baseline/states/<name>.png`.

| Page | State | Screenshot | Reached |
|---|---|---|---|
| `/portfolio/authorisations` | None yet: "No active authorisations yet" | `my-authorisations--empty` | Dr Mahlangu, at Step 4.43 |
| | With an expiry: "Expires in 20 days" | `my-authorisations--expiring` | Dr Molefe, Step 4.39 |
| | After a revocation (design "No longer in force" here) | `my-authorisations--after-revocation` | Dr Dlamini, Step 4.42 |
| | Fifteen | `my-authorisations--fifteen` | Dr Molefe, Step 5.8 |
| | Loading | `my-authorisations--loading` | Held read |
| | Narrow | `my-authorisations--narrow` | Step 4.39 at 390 px |
| `/committee/my-reviews` | None yet: "No decisions yet" | `my-reviews--empty` | Dr Dlamini, at Step 3.50 |
| | The list and a review | `my-reviews--detail` | Dr Molefe, Step 4.41 |
| | The appeal form | `my-reviews--appeal-form` | Dr Mahlangu, Step 4.43, before lodging |
| | Under appeal (the row is stale: T308) | `my-reviews--appealed` | Step 4.43 |
| | The replacement decision (no trace of the appeal: T308) | `my-reviews--remitted` | Step 4.48 |
| | Annual and formative | `my-reviews--two` | Dr Ndlovu, Step 4.51 |
| | Pre-graduation | `my-reviews--pre-graduation` | Dr Molefe, Step 5.8 |
| | Loading | `my-reviews--loading` | Held read |
| | Narrow | `my-reviews--narrow` | Step 4.41 at 390 px |
| `/committee/reviews/{ReviewId:int}`, Appeals card (re-captured after T307) | Under appeal, a member | `review-detail--appeal-member` | Dr Naidoo, Step 4.44 |
| | No outcome chosen: "Choose an outcome." (a step capture) | `act-4/4.45-2-vanrensburg-choose-an-outcome` | Dr van Rensburg, Step 4.45 |
| | The resolve form, Remitted chosen | `review-detail--appeal-form` | Dr van Rensburg, Step 4.45 |
| | Remit without a quorum | `review-detail--remit-refused` | Dr Zulu, Step 4.46 |
| | Closed after a remit | `review-detail--remitted` | Step 4.47 |
| | Appeal dismissed | `review-detail--appeal-dismissed` | Scratch database (post-act 4) |
| | ~~Appeal upheld~~: no longer exists (T307, D51). The file is kept on disk; do not design it | `review-detail--appeal-upheld` | — |
| `/admin/entrustment-decisions` | None issued: "No entrustment decisions" | `entrustment-decisions--empty` | Prof Mbatha, at Step 4.2 |
| | Issued | `entrustment-decisions--issued` | Step 4.35 |
| | Filtered | `entrustment-decisions--filtered` | Step 4.35, "Molefe" |
| | Revoke form | `entrustment-decisions--revoke-form` | Dr Mokoena, Step 4.36, before confirming |
| | Revoked | `entrustment-decisions--revoked` | Step 4.36 |
| | Superseded | `entrustment-decisions--superseded` | Step 5.7, status All |
| | A paused EPA | `entrustment-decisions--paused-epa` | Step 6.22 |
| | Loading | `entrustment-decisions--loading` | Held read |
| | Narrow | `entrustment-decisions--narrow` | Dr Mokoena, Step A.7.7 |

That is 29 state captures and one step capture (`4.45-2`); `review-detail--appeal-upheld` no longer counts. Two states
are **new**, with no capture: My authorisations' "No longer in force" (T319) and the trainee's Appeals block (T308). The
Outcome select's empty first option (T307) is built now: `4.45-2` shows it, refused. Two related captures belong to
other flows: `decisions-due--revoked` (flow 08, Step 4.37) and the shell's access-denied page (flow 01;
`act-4/4.41-2-molefe-committee-page-denied.png`). The Coordinator is refused the register too (`states.md` § Access
denied, by page).

## Attach

Paths are relative to `design/baseline/`. Every file below was found on disk on 2026-09-26, and none shows a
registration link or a password. The key screenshots listed first were opened; open the rest before you upload them
(BRIEF § 3.3).

**First, the key screenshots:**
1. `states/my-authorisations--expiring.png`
2. `states/my-reviews--detail.png`
3. `states/my-reviews--appealed.png`
4. `states/my-reviews--remitted.png`
5. `states/my-authorisations--after-revocation.png`
6. `states/entrustment-decisions--revoke-form.png`
7. `states/entrustment-decisions--superseded.png`
8. `act-4/4.41-2-molefe-committee-page-denied.png`

**Then, as the chat asks for them:**
- **Screen 1:**
  - `states/my-authorisations--empty.png`
  - `states/my-authorisations--fifteen.png`
  - `states/my-authorisations--loading.png`
  - `states/my-authorisations--narrow.png`
- **Screen 2:**
  - `states/my-reviews--empty.png`
  - `states/my-reviews--appeal-form.png`
  - `states/my-reviews--two.png`
  - `states/my-reviews--pre-graduation.png`
  - `states/my-reviews--loading.png`
  - `states/my-reviews--narrow.png`
  - `act-4/4.48-1-mahlangu-remitted-outcome.png`
- **Screen 3** (re-captured on 2026-09-26 after T307 landed, d03732d; BRIEF § 10):
  - `act-4/4.45-1-vanrensburg-remit-form.png`
  - `act-4/4.45-2-vanrensburg-choose-an-outcome.png`
  - `act-4/4.46-1-zulu-remit-quorum-refused.png`
  - `act-4/4.47-1-zulu-appeal-remitted.png`
  - `act-4/4.44-1-naidoo-appeal-body-note.png`
  - `states/review-detail--appeal-form.png`
  - `states/review-detail--appeal-member.png`
  - `states/review-detail--remit-refused.png`
  - `states/review-detail--remitted.png`
  - `states/review-detail--appeal-dismissed.png`
- **Screen 4:**
  - `states/entrustment-decisions--empty.png`
  - `states/entrustment-decisions--issued.png`
  - `states/entrustment-decisions--filtered.png`
  - `states/entrustment-decisions--revoked.png`
  - `states/entrustment-decisions--paused-epa.png`
  - `states/entrustment-decisions--loading.png`
  - `states/entrustment-decisions--narrow.png`

**Do not attach:**
- `act-A/A.7.7-1-mokoena-home.png`: a SpecialityAdmin home, F06's (re-captured after T297 on 2026-09-26; BRIEF § 10).
- `states/review-detail--appeal-upheld.png`: an outcome that no longer exists (T307, D51). It is kept on disk as the
  record of the old form.

## Known problems this design must solve

| Task | What it means for the design | Evidence |
|---|---|---|
| **T307** (group 1, landed in d03732d; D51) | Built; keep it. **Outcome.** It opens on "Select an outcome…", then "Dismissed: the decision stands" and "Remitted: the appeal body replaces the decision", with help saying what each does. Resolving without a choice is refused in the form with "Choose an outcome.". **Remit.** Remitted reveals an optional "Replacement conditions" box. **Upheld.** Removed (D51): it closed the review with the decision in force, as Dismissed does. **Replacement card.** A remit's conditions show there as "Conditions: …". **Still to design:** the chosen outcome is clipped at 1280 px in the half-width select once Remitted opens the second column (F-4.45c), and the quorum refusal shows only at the head of the page, out of view of the card (F-4.46a). | Steps 4.45–4.47 (re-checked 2026-09-26); `act-4/4.45-1`, `4.45-2`, `4.46-1`, `4.47-1`; the screen 3 states above |
| **T308** | **The list row.** It reads Under appeal as soon as she lodges. **The detail.** It shows every decision as the committee's page does (newest first, "Replaced on appeal by the decision above.", rationale, Conditions and Present), from one shared component (T308 item 2). **Appeals block.** It gives the date lodged, the reason, the outcome in words and the date resolved. | `states/my-reviews--appealed.png`, `states/my-reviews--remitted.png`; Steps 4.43, 4.48 |
| **T319** (backend P2) | **No longer in force.** A section on My authorisations lists revoked and expired STARs, each with its status, date and a named Download; superseded STARs stay off (T319's recommendation). **The notice.** Revoking now sends a mail. The confirmation says so only once that mail exists. | `states/my-authorisations--after-revocation.png`; Step 4.36 Gap F-4.36a |
| **T264**, **T260** | **Revoke.** The trigger is an outline button with a per-row name, and a `ConfirmDialog` names the trainee and the EPA; today it is a red in-row button that opens an inline section. **Confirm revocation** is enabled as the reason is typed. | `states/entrustment-decisions--revoke-form.png` (observed); Step 4.36 note |
| **T324** (A8) | Wherever a revocation is shown, the reviser is named by name. The certificate prints "by f3e77734-…" today. | Step 4.37 Gap F-4.37a |
| **T325** (A9) | "Expires in N days" counts on the South African calendar. Today it counts from the UTC date (`MyAuthorisations.razor:39,132`, cited in T325). | `states/my-authorisations--expiring.png` |
| **T310** | The certificate is headed by the issuing panel's institution. It is a PDF, not a screen. Brief it separately if wanted. | T310 |
| **T322** (A1) | The badge tints (Active, Superseded, Deferred, Not decided), the "Expires in" pill and the success alert need 4.5:1 text. | `act-A/A.7.14-6-reviews-badges.png` (BRIEF § 6) |
| **T328** (A4) | Form controls use the body font. The revoke reason box renders in a monospace face (observed). | `states/entrustment-decisions--revoke-form.png` |
| **T309** | A formative check-in is typed as an "Annual progression" review. The trainee's list has no Mode column, so the design must mark a check-in some other way (question 4). | Step 4.51's Actual; Step 4.49 Gap F-4.49a |
| **T311** | A graduate loses My authorisations and My committee reviews (Step 5.22). That home is flow 13's; this flow only must not assume the page outlives the Trainee role. | BRIEF § 7 B1 |

This flow also covers BRIEF § 7 B5 (the trainee's decision history and appeals), B6 (STARs no longer in force), B8
(the register reached only by address) and B9 (the appeal outcome form).

## Questions the design must answer

1. **Certificates.** Do they live on My authorisations only, or also on the review that issued them? Today they are on
   My authorisations (Step 4.39) and the register (Step 4.35). The review's agenda gives only "STAR #n." (Step 4.41).
2. **The decision history.** Does the trainee's review page reuse the committee's decision-history component exactly?
   T308 recommends it: `ReviewDetail.razor:306-345` against `MyReviews.razor:65-86`.
3. **Nav links.** Should the speciality admins get a nav link to the STAR register, and the Trainee one to My
   authorisations? Both pages are listed in `coverage.md` § Reached only by address, and the Trainee's nav row is
   DESIGN.md:196. A nav change is a DESIGN.md change, because `NavMenuAuthorizationTests` parses its table (BRIEF § 4).
4. **Formative check-ins.** How does the trainee's list tell a check-in from an annual review? See Step 4.51 and T309.

## Acceptance

The flow is done when BRIEF § 9's checks hold:
- **The steps replay on a fresh database** (`tools/scenario-replay.ps1`):
  - Play Acts 1–3 and Act 4 through Step 4.34, then Steps 4.35–4.51.
  - Play Act 5 through Step 5.6, then Steps 5.7 and 5.8.
  - Play Act 6 through Step 6.21, then Step 6.22.
  - Play Step A.7.7.
  - Every Expect holds. Expect lines as amended by T307, T308 and T319 (Steps 4.36, 4.42, 4.43 and 4.45–4.48) replace
    the ones pasted here. Change any other Expect whose on-screen wording the redesign changes, in the same task (BRIEF
    § 9, item 7).
- **The 29 states are re-captured** into `design/baseline/states/`, together with the steps' captures:
  - Act 4: `4.35-1`, `4.36-1`, `4.37-1`, `4.39-1`, `4.41-1`, `4.41-2`, `4.42-1`, `4.43-1`, `4.44-1`, `4.45-1`,
    `4.45-2`, `4.46-1`, `4.47-1`, `4.48-1` and `4.51-1`;
  - Act 5: `5.7-1`, `5.7-2`, `5.8-1`, `5.8-2` and `5.8-3`;
  - Act 6: `6.22-3`;
  - Appendix: `A.7.7-4`.

  Compare them with the chosen artboards. `review-detail--appeal-dismissed` needs the scratch database in `states.md`.
  There is no `--appeal-upheld` to re-capture: T307 removed Upheld (D51).
- **`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj` is green,** without `--no-build`. That run includes:
  - T307's `ReviewDetailAppealFormTests`;
  - T308's `MyReviewsAppealTests`;
  - T319's bUnit test for "No longer in force";
  - `Design/*` and `Accessibility/*`;
  - T294's scenario guard.
- **A browser check passes at 1280 and 390 px** for every person in the flow:
  - Dr Molefe, Dr Dlamini, Dr Mahlangu and Dr Ndlovu;
  - Dr Zulu, Dr Naidoo and Dr van Rensburg;
  - Dr Mokoena, Dr Sithole and Prof Mbatha.

## Appendix: the runbook steps (paste after the ask)

Verbatim from `execution/knowledge/scenario-paediatrics/`: Role, Route, Do and Expect only. `D` is the replay day.

```
Step 4.35 — Prof Mbatha reads the issued STARs
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/entrustment-decisions
Do: Open Entrustment decisions from her dashboard's quick links. Download Dr Molefe's PAED-010 certificate. Filter by
  trainee "Molefe" and apply.
Expect:
  - **The list:** five rows, all Active, issued `D`.
    - Molefe: PAED-001 `5`, PAED-010 `4` expiring `D+20`, PAED-012 `5`.
    - Dlamini: PAED-001 `4`, PAED-002 `3b`.
    - Expires reads "—" where none was set.
  - **The certificate PDF** names:
    - Kgosi Kgari Teaching Hospital, "Statement of Awarded Responsibility", Lerato Molefe, PAED-010 and level `4`;
    - Issued on `D` and Expires on `D+20`, Status Active;
    - the rationale, and the evidence summary of the two snapshot lines;
    - Panel Paed Annual Review Panel, the review number, and Chair Thandi Zulu.
  - **The filter** leaves Molefe's three rows.

Step 4.36 — Dr Mokoena revokes one of Dr Dlamini's STARs
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /admin/entrustment-decisions
Do: Type the page's address. Choose Revoke on Dr Dlamini's PAED-002 decision. Give the reason "Staged on evidence about
  PAED-001, not PAED-002; to be decided again." and confirm the revocation.
Expect:
  - **The list:** the same five rows as Step 4.35, since all five registrars are in her speciality.
  - **The dialog** names PAED-002 and Anele Dlamini, and says "Revocation is immediate and irreversible. The trainee is
    notified." Confirm revocation is enabled once a reason is typed.
  - **After confirming:** "Entrustment decision for PAED-002 revoked." The row reads Revoked, with no Revoke button.
  - **The trainee is notified,** as the dialog promises: the application log holds a mail to Dr Dlamini about the
    revocation.

Step 4.37 — Dr Sithole reads the list and what must be decided again
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /admin/entrustment-decisions → /committee/decisions-due
Do: Type the entrustment decisions page's address. Filter Status to Revoked and apply, then to Active and apply.
  Download the revoked certificate. Then open Decisions due with "Every status".
Expect:
  - **Filtered:** Revoked leaves Dlamini's PAED-002, and Active leaves four rows.
  - **The revoked certificate** reads Status Revoked, with "REVOKED", the date, who revoked it (Dr Mokoena, by name,
    T142) and the reason.
  - **Decisions due:** Dlamini's PAED-002 reads "Revoked: re-decide". Its detail reads "STAR #n, issued for `<P>`, was
    revoked. Schedule a review to decide it again." It offers Schedule.
  - **By EPA:** PAED-002 reads Decided 0, Deferred 4 and To schedule 1.
  - **Count, default filter:** "71 of 75 decisions due in `<P>` shown."

Step 4.39 — Dr Molefe reads her STARs and downloads a certificate
Role: Trainee — Dr Lerato Molefe
Route: / → /portfolio/authorisations
Do: From the dashboard's My authorisations card, view her authorisations. Download the PAED-001 certificate.
Expect: Three cards, each issued on `D` with the committee's rationale: PAED-001 and PAED-012 at Authorised level `5`,
  and PAED-010 at `4`.
  - PAED-001 and 012 read "No expiry".
  - PAED-010 reads the date `D+20`, with the badge "Expires in 20 days".
  Each Download certificate is named by its EPA. The PAED-001 certificate downloads with Status Active and its evidence
  summary.

Step 4.41 — Dr Molefe reads her review, and not the committee's page
Role: Trainee — Dr Lerato Molefe
Route: /committee/my-reviews → /committee/reviews/{ReviewId:int} → /access-denied
Do: Open My committee reviews and view her review. Then type the committee's own address for the same review.
Expect:
  - **The list:** one row, with Period "`<P>` · `<Y>`-01-01 to `<Y>`-12-31", Type Annual progression, State Ratified and
    Decision Satisfactory Progress. Its View is named by the review.
  - **The detail** shows Sits for, Evidence window, Type and State. Its decision is headed "Satisfactory Progress", with
    the rationale and "Present when this decision was taken: …" naming all four.
  - **The agenda** has three columns:
    - Decided with "STAR #n." on PAED-001, 010 and 012;
    - Deferred with "The committee's reason: To be decided at her final review." on nine;
    - "Not decided at this review." on PAED-008, 009 and 013.
  - **The committee's page** lands on access-denied.

Step 4.42 — Dr Dlamini reads what stands after the revocation
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/authorisations → /committee/my-reviews
Do: View her authorisations, then her review.
Expect: One card, PAED-001 at `4`: the revoked PAED-002 is no longer listed. Her review reads Ratified, Satisfactory
  Progress. The agenda still reads Decided with STAR numbers on PAED-001 and 002, because it records what the sitting
  did.

Step 4.43 — Dr Mahlangu lodges an appeal
Role: Trainee — Dr Nomsa Mahlangu
Route: /committee/my-reviews
Do: View her ratified review and read it. Lodge an appeal with the reason "The single DOPS reflects the start of the
  year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration."
Expect:
  - **Before lodging:**
    - the decision reads "Inadequate Progress — Additional Training", with who was present;
    - the agenda shows twelve Deferred lines, each "The committee's reason: Not enough observed evidence yet this
      year.", and three "Not decided at this review.";
    - an Appeal reason form is offered.
  - **After lodging:** "Appeal lodged." The state reads Under appeal, the form is gone and the agenda stays in view.

Step 4.44 — Dr Naidoo sees who hears the appeal
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Mahlangu's review.
Expect: The list shows her review Under appeal. The Appeals card lists the appeal, with the date lodged, its reason and
  "(Open)". It offers him no form. It says "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu,
  and its external member, John van Rensburg." (T213, T237)

Step 4.45 — Dr van Rensburg, the external member, is offered the appeal
Role: CommitteeMember (external) — Dr John van Rensburg
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews, then Dr Mahlangu's review. Read the resolve form, and press Resolve appeal without choosing
  an outcome. Then choose Remitted to see what it asks, and leave without resolving.
Expect:
  - **The list:** the panel's five reviews.
  - **The Appeals card** offers him the resolve form with an Outcome select opening on "Select an outcome…", then
    "Dismissed: the decision stands" and "Remitted: the appeal body replaces the decision" (T307, D51: there is no
    Upheld). Its help text says Dismissed leaves the decision in force, Remitted replaces it, and either closes the
    review.
  - **Resolve appeal with no outcome** says "Choose an outcome." under the select, and resolves nothing.
  - **Remitted** reveals:
    - Replacement category, opening on "Select a category…";
    - Replacement rationale;
    - Replacement conditions, optional;
    - Present, with both Thandi Zulu (chair) and himself ticked and locked.
  - **Nothing changes** until he submits.

Step 4.46 — A remit without a quorum is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: On Dr Mahlangu's review, choose Remitted, the replacement category Satisfactory with Observations and a
  rationale. Tick nobody besides herself as present, and resolve.
Expect: Resolve appeal is refused with "A committee decision needs at least two panel members present: the chair and
  at least one other." (T165: a remit records its own quorate sitting). The review stays Under appeal.

Step 4.47 — Dr Zulu remits the decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Keep Remitted, Satisfactory with Observations, and the rationale "The stalled Mini-CEX was the assessor's delay;
  progress is adequate, with observed assessments to follow." Write the Replacement conditions "Two observed Mini-CEX
  and one DOPS before the next review." Tick Sarah Botha as present, and resolve the appeal.
Expect: "Appeal resolved." The state reads Closed (T250).
  - **Decision card:** the replacement heads it, "Satisfactory with Observations" with "Conditions: Two observed
    Mini-CEX and one DOPS before the next review." (T307) and "Present: Thandi Zulu (chair), Sarah Botha". The first
    decision stays below it, marked "Replaced on appeal by the decision above.".
  - **Appeals card:** the appeal reads "(Remitted)".
  - **No STAR changed:** the review issued none.

Step 4.48 — Dr Mahlangu reads the outcome
Role: Trainee — Dr Nomsa Mahlangu
Route: /committee/my-reviews
Do: Open My committee reviews and view the review.
Expect: The row reads State Closed and Decision Satisfactory with Observations. The detail's current decision is the
  replacement, with "Present when this decision was taken: Thandi Zulu (chair), Sarah Botha". No appeal form is
  offered.

Step 4.51 — Dr Ndlovu reads both reviews
Role: Trainee — Dr Sipho Ndlovu
Route: /committee/my-reviews
Do: Open My committee reviews.
Expect: Two rows for `<P>`. The annual review reads Ratified, Outcome Deferred. The check-in reads Closed, with Decision
  "No binding decision".

Step 5.7 — Prof Mbatha reads Dr Molefe's STARs
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/entrustment-decisions
Do: Open Entrustment decisions from her dashboard's quick links. Filter by the trainee name "Molefe", first with the
  status Active and then with All, applying the filters each time.
Expect: There are fifteen Active rows, one for each EPA from PAED-001 to PAED-015. Each row:
  - is at its EPA's exit level;
  - was issued on `D`, except PAED-001's and PAED-012's, which Act 4 issued;
  - has no expiry;
  - offers Download and Revoke.
  With All, Act 4's PAED-010 STAR, which Step 5.6 superseded, reads Superseded and offers Download only. She revokes
  nothing.

Step 5.8 — Dr Molefe reads her final review and keeps a STAR certificate
Role: Trainee — Dr Lerato Molefe
Route: /committee/my-reviews → / → /portfolio/authorisations
Do: Open My committee reviews and view the pre-graduation review. Then open My authorisations from her dashboard's card,
  and download the certificate for PAED-001.
Expect: The review is listed as Pre-graduation, Ratified, Graduate (programme complete). Its detail shows:
  - the review type;
  - the decision and its rationale;
  - who was present;
  - an agenda in which each EPA reads Decided, with its STAR number.
  The Lodge appeal form is offered, and she lodges no appeal. My authorisations holds 15 cards, each with its level, its
  issue date and "No expiry". The certificate downloads as `star-certificate-PAED-001-<id>-<8 hex digits>.pdf`.

Step 6.22 — Prof Mbatha sees the pause, and cannot lift it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items → /admin/entrustment-decisions
Do: Read the EPA list, then 11.1's items. Then open Entrustment decisions and filter by Dr Molefe's name. Revoke
  nothing.
Expect:
  - **The EPA list.** PAED-012 reads Inactive. No national EPA offers her Edit: only its College changes one (T211's
    rule; `UpdateEpa`).
  - **11.1's items.** PAED-012's row is marked "(inactive: not in force)", under the same notice. Every row reads "Set
    by the College".
  - **Dr Molefe's STARs.** Her fifteen from Act 5 still read Active, beside Act 4's PAED-010, Superseded (the status
    filter starts on All). The PAED-012 row names its EPA "PAED-012 — Communicating
    with and counselling patients, caregivers and healthcare teams (no longer in use)" (T255): a pause leaves a STAR
    standing.

Step A.7.7 — Dr Mokoena on her phone
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /committee/panels → /committee/decisions-due → /admin/entrustment-decisions
Do: At 390 px, open her dashboard, Decision panels, Decisions due and, by its address, Entrustment decisions.
Expect: The dashboard's coverage cards stack. The panels list and the decisions list scroll inside their containers
  (T226).
```
