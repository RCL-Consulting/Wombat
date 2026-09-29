# F10 An MSF campaign from set-up to release

A Coordinator runs a registrar's multi-source feedback (MSF) campaign, colleagues answer it anonymously, and the
registrar reads the released report. It includes `/msf/respond`, the only page that people without an account ever
see. It is flow 10 of 18 (`design/BRIEF.md` § 8).

| | |
|---|---|
| **Mode** | Straight to fidelity. The List, Form and Detail shapes hold (BRIEF § 4). Design the respondent's page, `/msf/respond`, on its own card: it is static and anonymous (DESIGN.md § Anonymous static page). |
| **Viewports** | Desktop 1280×800 and phone 390×844. |
| **Held** | None of this flow's pages is changed by a group-1 task. Two step captures land on the Coordinator's dashboard, F06's (re-captured after T297 on 2026-09-26), and are left out (§ Attach). |
| **Frequency** | One campaign per registrar per period: about 16 returned questionnaires per registrar a year (EPA-PROGRAMME.md:203–205, D9). |
| **Stakes** | Medium. Anonymity must hold, and a released report becomes evidence on the registrar's portfolio. |
| **People** | Mr Pieter Smit (Coordinator). Anonymous respondents: Sister Grace Mokwena, Sister Palesa Tau, Dr Kagiso Motsepe, Dr Fatima Khumalo, Dr Sarah Botha and Ms Naledi Sebego; Dr Lindiwe Khoza never answers (Steps 3.36, 3.39, 3.42). Dr Lerato Molefe (Trainee). |

**How to use this brief**

1. Open a new Claude Design thread for this flow (BRIEF § 2.3, step 3).
2. Paste the block under **The ask**, then the block under **Appendix: the runbook steps**.
3. Attach the images listed under **Attach**, key screenshots first. Open each one before you upload it (BRIEF § 3.3).
4. Answer Claude's questions in the chat, each as a sentence [academy].
5. Export the chosen artboards to `design/flows/10-msf-campaign/` before moving on [start].

## The ask

```
FLOW 10 — Run a registrar's multi-source feedback campaign from set-up to release, and answer one anonymously

DIRECTION: <paste the aesthetic direction recorded as W-nnn in execution/DECISIONS.md (design/BRIEF.md § 2.3 step 0)>
DESIGN SYSTEM: Wombat, as set up for this project:
  - the :root tokens of app.css;
  - Fraunces on the wordmark only;
  - Lucide line icons;
  - the components named in CONSTRAINTS.
  The signed-in screens sit inside the shell settled in flow 01. The respondent's page has no shell. It is the
  account card, widened for a questionnaire (the sign-in page's layout, with no nav).

GOAL: The Coordinator:
  - builds a questionnaire;
  - creates a campaign for a registrar;
  - invites colleagues by address and respondent group;
  - opens the campaign;
  - resends a link that was not delivered;
  - closes the campaign and releases the report, with a narrative.
  Colleagues answer anonymously from an emailed link. The registrar reads the released report, and her progress shows
  the EPAs it covered. /msf/respond is the only page that people without an account ever see.
  What is wrong today:
  - One release shows two clocks: "12:53" unlabelled on the trainee's list, and "10:53 UTC" on the coordinator's
    report.
  - The nav lights no item on the report and coverage pages.
  - The report says "from 1 responses".
  - The draft's address table breaks addresses mid-word.
  - A withdrawn draft shows a Responded column.
  - New campaign loses the live connection when a read fails, instead of showing an error.

AUDIENCE:
  - Coordinator: Mr Pieter Smit, at a desk and on his phone.
  - Anonymous respondents: nurses, registrars, consultants and a physiotherapist. They never sign in, and they may
    open the link on a phone.
  - Trainee: Dr Lerato Molefe.
  - Frequency: one campaign per registrar per period, about 16 returned questionnaires per registrar a year.
  - Stakes: medium. Anonymity must hold.
  - Viewports: desktop 1280×800 and phone 390×844.

SCREENS, in order:
  1. /msf/campaigns: the campaign list.
     - Header: New campaign, and an "MSF coverage" link.
     - Rows: trainee, template, state and window, with actions named by state:
       - Draft: Manage and Withdraw;
       - Open: Manage, View report and Withdraw;
       - Released: View campaign and View report;
       - Withdrawn: View campaign.
     - Withdraw confirms in a dialog that names the trainee, the template and the closing date, and says it cannot be
       undone.
     - States: empty ("No MSF campaigns"); open and draft; the withdraw dialog; withdrawn; released.
  2. /msf/campaigns/new, then /msf/campaigns/{CampaignId:int}: a campaign.
     - Create page:
       - A Quick template card, shown while no template exists: kind, name, one rating question and one comment
         question.
       - A Create campaign card:
         - Trainee: the institution's current trainees, by name and address;
         - Template;
         - opens and closes, defaulting to today and +14 days;
         - minimum responses (5) and minimum per group (2);
         - minimum reporting groups (2), with help saying two is the College's answer;
         - "Evidence for these EPAs": a checkbox for each of her curriculum's 15 EPAs.
     - Campaign page:
       - Details: trainee, template, state and response window.
       - Invitees counted by respondent group: Invited, plus Responded once the campaign is open, and an All groups
         total.
       - While it is a draft, and only then:
         - "Addresses invited", each with Remove. Remove opens a dialog that names the address and the group.
         - "Add an invitee": an address and a respondent group (Peer doctor, Consultant, Nurse, Allied health
           professional or Other; never Patient or Learner).
         - Open campaign, disabled with its reason until someone is invited.
       - Once it is open:
         - the respondents' last day;
         - "N links are still being sent";
         - a warning when a link was not delivered, with "Resend N links". It never says whose link.
       - Once it is closed: a note that the addresses have been removed, and "Review and release".
     - States: new (no template yet, template created, a trainee chosen); draft with nobody invited; draft with
       invitees; the remove dialog; opened; not delivered; resent; closed; released; the withdraw dialog; withdrawn;
       unavailable.
  3. /msf/respond: the respondent's page. It is static and anonymous, on its own card, with no nav.
     - Heading: "Feedback on Lerato Molefe", with the template and the last day to respond.
     - A statement that:
       - the respondent's name and address are never shown to her;
       - she sees the feedback only once the request has closed and been released, grouped by respondent role.
     - The rating: five points, from Well below expectations to Well above expectations, and required.
     - The comment: optional, up to 4000 characters, with a warning that it may reach her word for word.
     - Submit feedback.
     - States: the form; the rating left out (the browser's own prompt); thank you; link already used; request closed;
       link expired; link not recognised; something went wrong. Each also at 390 px.
  4. /msf/reports/{CampaignId:int}: the coordinator's report.
     - Summary: template, kind, state, total responses, the minimums, and reporting groups (n of 2 required).
     - Evidence for: the EPAs, and a note that one record per EPA is written on release. Once released, it says when.
     - A card for each respondent group that answered: the rating's average "from n responses" and the comments, or
       "Insufficient responses in this category. Results are suppressed."
     - Actions:
       - Close campaign, while it is open.
       - Once it is under review: a narrative; "Supervision level this feedback supports" (Not stated, or a rung of
         the six-rung ladder); and Release to trainee.
       - Once it is released: what was stored, with no form.
     - States: open; under review; blocked ("Release needs 5 responses; 1 have come back."); names no EPA; released;
       unavailable.
  5. /msf/my-reports and /msf/my-reports/{CampaignId:int}: the registrar's reports.
     - A list: template, when released, and the number of responses.
     - One report: the narrative, and only the groups that met the threshold, each with its average and comments. No
       group's count.
     - States: none released; the list; a report; another trainee's id (her own list, with nothing selected).
  6. /msf/coverage: MSF coverage, a planning aid for the Coordinator.
     - For each programme and semester, how many trainees it covers.
     - By EPA: "n of 5 trainees covered". By trainee: "n of 15 EPAs covered".
     - No badge, tint or bar: coverage is not a target.
     - States: loaded; nobody to show.

STEPS: 3.34–3.49, A.4.7 and A.7.5.
  - They are pasted verbatim after this block (Role / Route / Do / Expect).
  - The Expect lines describe the product as replayed on 2026-09-26. Where a requirement below differs, the
    requirement wins.
  - Step 3.48 checks the release's effect on My Activities and My progress. Those pages belong to flows 03 and 05:
    keep their words, but do not design them here.
  - Step A.4.7 passes through the sign-out confirmation page, which is flow 02's.

STATES TO SHOW:
  - Every state listed under SCREENS.
  - For each signed-in page: loading (a skeleton under the header), the load error, and narrow (390 px).
  - Data volumes:
    - none;
    - typical: 7 invitees in 4 groups, and 6 responses;
    - heavy: many campaigns across 5 trainees, and a group with a dozen comments.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T325: one clock, labelled. The trainee's list and the coordinator's report show the same release time, in SAST
    with the zone.
  - T331: the nav marks MSF Campaigns on the report page and the coverage page.
  - T329: a failed read on New campaign shows the page's load-error alert, not a dead page.
  - T270:
    - "from 1 response" is singular when there is one;
    - the draft's address table fits at 390 px;
    - a withdrawn draft has no Responded column.
  - T322: alert and badge text reaches 4.5:1. This covers the suppressed-group warning, the state badges and the
    success alerts.
  - Observed, not filed:
    - At 1280 px, the draft's address column breaks addresses mid-word.
    - "Add an invitee" labels the group "Category", where both tables say "Respondent group".
  - Anonymity, a product decision:
    - Once a campaign opens, no page lists who was invited or who responded.
    - The trainee's report shows no group's count, and nothing of a group whose results were suppressed.

REQUIREMENTS EVERY WOMBAT PAGE MEETS (design/BRIEF.md § 6; the ones this flow tests):
  A1  Contrast in the tokens.
  A3  At 390 px nothing scrolls sideways except a table inside its own container. At 1280 px addresses wrap between
      words.
  A5  Every action reports its outcome where the focus lands. Destructive actions (Remove, Withdraw) confirm and name
      their target.
  A6  Loading, load-error and not-found ("Campaign unavailable", "Report unavailable") states are designed.
  A9  One clock, labelled SAST.
  A10 The copy says what the page does for this viewer.
  A12 The nav marks where you are.

QUESTIONS THE DESIGN MUST ANSWER:
  1. The report and the campaign page never list who responded; they count by group. How does the design keep that
     rule visible to the coordinator, so that he does not go looking for names?
  2. What does the respondent see first: who they are rating and why, or the questions?
  3. Should MSF coverage stay a link from the campaign list, or become a tab of it?

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
  - 2–3 variations, at full fidelity.
  - Name every design-system component you use, and mark anything else NEW.
  - Say which DESIGN.md rule a variation breaks.
  - Design the respondent's page as plain HTML with a form post: no script, no live validation.
  - Flag edge cases, and review each screen's accessibility against the A-requirements above.

ATTACHED (key screenshots first):
  - states/campaign-edit--new.png, campaign-edit--draft-empty.png, campaign-edit--draft.png,
    campaign-edit--not-delivered.png;
  - states/msf-respond--form.png, msf-respond--thank-you.png, msf-respond--used.png, msf-respond--narrow.png;
  - states/campaign-report--under-review.png, campaign-report--blocked.png;
  - states/my-msf-reports--report.png, programme-coverage--loaded.png;
  - act-3/3.47-1-molefe-report.png, act-3/3.46-1-report-released.png.
  - A full-page capture draws the sidebar part-way down a long page, or ends it early. That is the capture, not the
    product.
```

## The journey

| Step | Route | Who does what | What they must be able to see |
|---|---|---|---|
| 3.34 | `/` → `/msf/campaigns` → `/msf/campaigns/new` | Mr Smit starts a campaign and, with no template yet, adds the Quick template "Default MSF". | "No MSF campaigns", with MSF coverage and New campaign in the header. "Template created.", and the Template select offering it. |
| 3.35 | `/msf/campaigns/new` → `/msf/campaigns/{CampaignId:int}` | He creates Dr Molefe's campaign: the defaults, the three minimums, and evidence for PAED-010 and PAED-012. | Only KGK's five trainees offered. All 15 EPAs, once a trainee is chosen. The College's answer (D11) in the help. Draft, with "Nobody has been invited yet.", and Open campaign disabled with its reason (T225). |
| 3.36 | `/msf/campaigns/{CampaignId:int}` | He adds seven colleagues by address and group, removes one entered under the wrong group, and adds her again. | "Invitee added." each time, with the address cleared. The five groups. A Remove dialog naming the address and the group. The counts by group. Open campaign enabled. |
| 3.37 | `/msf/campaigns/{CampaignId:int}` | He opens the campaign. | "Campaign opened; links are being sent.", the last day, and "7 links are still being sent…". Counts only, with Responded 0 and no address (T217, T247). |
| 3.38 | `/msf/campaigns/new` → `/msf/campaigns/{CampaignId:int}` → `/msf/campaigns` | He creates a second campaign, for Dr du Plessis, then withdraws it from the list. | Two rows with actions named by state (T225). The Withdraw dialog naming the trainee, the template and the closing date. Withdrawn, with only View campaign. |
| 3.39 | `/msf/respond` | Three respondents answer from their links, signed in to nothing. | "Feedback on Lerato Molefe", the last day, and the anonymity statement. A required five-point rating (D47) and an optional comment with its warning. "Thank you". |
| 3.40 | `/msf/respond` | Dr Motsepe opens his link again. | "Feedback link already used", with no questionnaire. |
| 3.41 | `/msf/campaigns` → `/msf/reports/{CampaignId:int}` | Mr Smit reads the report while the campaign is open. | State Open and the totals against the minimums. The Nurse card (4.50 from 2). Peer doctor suppressed. Close campaign, but not Release. |
| 3.42 | `/msf/respond` | Three more answer; Dr Khoza never does. | "Thank you" each time. |
| 3.43 | `/msf/campaigns/{CampaignId:int}` | He resends the link that was not delivered. | Responded counts by group, and "1 link was not delivered…; this page never says who they are." with "Resend 1 link". Then "1 new link is being sent.". |
| 3.44 | `/msf/reports/{CampaignId:int}` → `/msf/campaigns/{CampaignId:int}` | He closes the campaign, then opens its page. | "Campaign closed and anonymised for review.", Under review, two groups shown and two suppressed. Release enabled, with the narrative and the supervision level (D10). The campaign page says the addresses are gone. |
| 3.45 | `/msf/respond` | Dr Khoza opens the resent link after the close. | "Feedback request closed", with no questionnaire. |
| 3.46 | `/msf/reports/{CampaignId:int}` → `/msf/campaigns` | He writes the narrative, leaves the level at Not stated, and releases. | "Report released to the trainee.", with what was stored and no form (T246). When the evidence was recorded. The list row reads Released, with no Withdraw. |
| 3.47 | `/msf/my-reports` → `/msf/my-reports/{CampaignId:int}` | Dr Molefe reads her released report. | One row, with 6 responses. The narrative, Nurse 4.50 and Consultant 4.00 with their comments. Nothing of the suppressed groups, and no counts (T249). |
| 3.48 | `/activities/mine` → `/portfolio/progress` | She checks her record and her progress (flows 03 and 05). | Two MSF rows, credited "—" (D8). "2 of 15 EPAs covered by a released campaign…", which counts towards no target. |
| 3.49 | `/msf/campaigns` → `/msf/coverage` | Mr Smit reads the programme's MSF coverage. | Each semester's trainee count. PAED-010 and PAED-012 at "1 of 5 trainees covered". Molefe "2 of 15 EPAs covered". No badge, tint or bar (D8, D9). |
| A.4.7 | `/account/logout-confirm` → `/` → … → `/msf/campaigns` | He signs out through the confirmation page, then opens MSF Campaigns by its address. | The Sign out page, with no nav. Asked to sign in, then brought back to MSF Campaigns. |
| A.7.5 | `/` → `/msf/campaigns` → `/msf/campaigns/{CampaignId:int}` → `/committee/decisions-due` → `/admin/data-rights` | He works on his phone. | The campaign page keeps its gutter (T226), and the invitee counts table scrolls inside its card. |

## States to design

The states are from `scenario-paediatrics/states.md` § Multi-source feedback. Each is `design/baseline/states/<name>.png`.
The "Scratch" rows need the scratch database that `states.md` describes.

| Page | States (screenshot suffixes) |
|---|---|
| `/msf/campaigns` (`campaigns-list--`) | `empty` (Step 3.34); `open-draft` and `withdraw-dialog` (Step 3.38); `withdrawn` (Step 3.38, confirmed); `released` (Step 3.46); `loading`; `narrow` (Step A.7.5) |
| `/msf/campaigns/new` (`campaign-edit--`) | `no-template` (Step 3.34, before the template); `template-created` (Step 3.34); `new` (Step 3.35, a trainee chosen, 15 EPAs offered) |
| `/msf/campaigns/{CampaignId:int}` (`campaign-edit--`) | `draft-empty` (Step 3.35, Open disabled with its reason); `remove-dialog` and `draft` (Step 3.36); `opened` (Step 3.37); `withdraw-dialog` (cancelled, no change); `not-delivered` and `resent` (Step 3.43); `closed` (Step 3.44); `withdrawn` (du Plessis); `released` (after Step 3.46); `unavailable` (typed `/msf/campaigns/999999`); `narrow` (Step A.7.5) |
| `/msf/reports/{CampaignId:int}` (`campaign-report--`) | `open` (Step 3.41, a group suppressed); `under-review` (Step 3.44); `released` (Step 3.46); `blocked` (scratch: 1 response); `no-epa` (scratch); `unavailable`; `loading`; `narrow` |
| `/msf/respond` (`msf-respond--`) | `form` (Step 3.39); `required` (the browser's prompt); `thank-you`; `used` (Step 3.40); `closed` (Step 3.45); `expired` (scratch); `not-recognised` (typed token); `fault` (held read, 35 s); `narrow` |
| `/msf/my-reports` and `/{CampaignId:int}` (`my-msf-reports--`) | `empty`; `list` and `report` (Step 3.47); `foreign` (Dr Dlamini typing Dr Molefe's id: her own empty list); `loading`; `narrow` |
| `/msf/coverage` (`programme-coverage--`) | `loaded` (Step 3.49); `empty` (Step 2.8: "No trainees to show"); `loading`; `narrow` |

That is 49 captures. `states.md` lists one respondent refusal that no replay reaches, "Feedback link revoked": nothing
sets a link's revoked time (`states.md` § States no local replay reaches). There is no capture of the create page's
load error (T329); design it.

## Attach

Paths are relative to `design/baseline/`. Every file below was found on disk on 2026-09-26. The MSF captures show no
registration or response link. Open each one before you upload it (BRIEF § 3.3).

**First, the key screenshots:**
1. `states/campaign-edit--new.png`
2. `states/campaign-edit--draft-empty.png`
3. `states/campaign-edit--draft.png`
4. `states/campaign-edit--not-delivered.png`
5. `states/msf-respond--form.png`
6. `states/msf-respond--thank-you.png`
7. `states/msf-respond--used.png`
8. `states/campaign-report--under-review.png`
9. `states/campaign-report--blocked.png`
10. `states/my-msf-reports--report.png`
11. `states/programme-coverage--loaded.png`
12. `states/msf-respond--narrow.png`
13. `act-3/3.47-1-molefe-report.png`
14. `act-3/3.46-1-report-released.png`

Items 13 and 14 are a pair: the same release on two clocks (T325).

**Then, as the chat asks for them:**
- **Screen 1:**
  - `states/campaigns-list--empty.png`
  - `states/campaigns-list--open-draft.png`
  - `states/campaigns-list--withdraw-dialog.png`
  - `states/campaigns-list--withdrawn.png`
  - `states/campaigns-list--released.png`
  - `states/campaigns-list--loading.png`
  - `states/campaigns-list--narrow.png`
- **Screen 2:**
  - `states/campaign-edit--no-template.png`
  - `states/campaign-edit--template-created.png`
  - `states/campaign-edit--remove-dialog.png`
  - `states/campaign-edit--opened.png`
  - `states/campaign-edit--resent.png`
  - `states/campaign-edit--closed.png`
  - `states/campaign-edit--released.png`
  - `states/campaign-edit--withdraw-dialog.png`
  - `states/campaign-edit--withdrawn.png`
  - `states/campaign-edit--unavailable.png`
  - `states/campaign-edit--narrow.png`
- **Screen 3:**
  - `states/msf-respond--required.png`
  - `states/msf-respond--closed.png`
  - `states/msf-respond--expired.png`
  - `states/msf-respond--not-recognised.png`
  - `states/msf-respond--fault.png`
- **Screen 4:**
  - `states/campaign-report--open.png`
  - `states/campaign-report--no-epa.png`
  - `states/campaign-report--released.png`
  - `states/campaign-report--unavailable.png`
  - `states/campaign-report--loading.png` (the unlit nav, T331)
  - `states/campaign-report--narrow.png`
- **Screen 5:**
  - `states/my-msf-reports--empty.png`
  - `states/my-msf-reports--list.png`
  - `states/my-msf-reports--foreign.png`
  - `states/my-msf-reports--loading.png`
  - `states/my-msf-reports--narrow.png`
- **Screen 6:**
  - `states/programme-coverage--empty.png`
  - `states/programme-coverage--loading.png` (the unlit nav, T331)
  - `states/programme-coverage--narrow.png`

**Do not attach:**
- `act-A/A.4.7-2-cancel-back-on-dashboard.png` and `act-A/A.7.5-1-home-390.png`: both show the Coordinator's
  dashboard, F06's (re-captured after T297 on 2026-09-26; BRIEF § 10).

## Known problems this design must solve

| Task | What it means for the design | Evidence |
|---|---|---|
| **T325** (A9) | One release reads "2026-09-27 16:27" under Released (server-local, no zone) on the trainee's list, and "Recorded on the portfolio: 2026-09-27 14:27 UTC" on the report. Show one SAST time, labelled, everywhere. New campaign's default dates also come from the server's date. | `act-3/3.47-1-molefe-report.png` against `act-3/3.46-1-report-released.png` |
| **T331** (A12) | The nav lights no item on `/msf/reports/{CampaignId:int}` or `/msf/coverage`. Mark MSF Campaigns there. | `states/campaign-report--loading.png`, `states/programme-coverage--loading.png`; the same unlit nav in `states/campaign-report--under-review.png` (observed) |
| **T329** (P2, A6) | New campaign ends the circuit when a read fails. Design its load-error state: the header, the alert, and no form. | T329 |
| **T270** | **Plurals.** "from 1 responses" becomes "from 1 response". **Address table.** At 390 px it was 428 px in a 306 px container, so Remove sat off-screen; it must stack or wrap, with Remove always visible. **Withdrawn drafts.** A campaign withdrawn while still a draft shows no Responded column. The replay's withdrawn draft had nobody invited, so `states/campaign-edit--withdrawn.png` shows only "Nobody was invited." (observed). | T270 (dev campaigns 18 and 20) |
| **T270's notes** | **Refusals.** My MSF reports shows a refusal twice, once in the top alert and once in the list's load error. **Question text.** The trainee's report adds a colon after question text that already ends in a full stop. | T270, notes of 2026-09-25 |
| **T322** (A1) | The warning alert ("Insufficient responses…"), the success alerts and the state badges fall below 4.5:1. | `states/campaign-report--under-review.png`; BRIEF § 6's failing pairs |
| **Observed, not filed** | At 1280 px the draft's email column is narrow and breaks addresses mid-word, and the Remove buttons are not visibly named per row. The add form labels the group "Category" while both tables say "Respondent group". | `states/campaign-edit--draft.png` |
| **Anonymity** (T217, T247, T249; DESIGN.md § The MSF campaign page, from line 994) | Once a campaign opens, no page lists an address or who responded. Invitees are counted by group. The trainee's report shows no group's count and nothing of a suppressed group. A design that adds a respondent list, or a per-invitee "responded" mark, breaks the product's rule. | Steps 3.37, 3.43, 3.47; `states/campaign-edit--opened.png` |

## Questions the design must answer

1. **Anonymity, visible.** The report never lists who responded; the campaign page counts by group only
   (`states/campaign-edit--opened.png`; DESIGN.md § The MSF campaign page, "never listed once the campaign has
   opened"). How does the design keep that rule visible to the coordinator, so that he does not go looking for names?
2. **The respondent's first view.** What comes first: who they are rating and why, or the questions?
   `states/msf-respond--form.png` puts two paragraphs above the first question today.
3. **MSF coverage.** Should it stay a link from the campaign list's header (DESIGN.md:238–241, § The NavMenu), or
   become a tab of the list? A tab changes the page shape and `NavMenuAuthorizationTests` does not cover it
   (inference).

## Notes

- **`/msf/respond` is static for everyone.** It is `[ExcludeFromInteractiveRouting]` (`MsfRespond.razor`; `App.razor`
  remarks, BRIEF § 5.2). It has:
  - no live validation;
  - a plain form post;
  - a rate limit per link;
  - `Referrer-Policy: no-referrer` (DESIGN.md § Anonymous static page, :1990).
  Its narrow state was captured by opening Sister Mokwena's link at 390 px (`states.md`).
- **Not played in the runbook** (`coverage.md` § Flows and states not played):
  - a learner-feedback campaign;
  - patient respondents, which are excluded by design (`CreateMsfTemplateCommandValidator`);
  - the auto-close and expiry-reminder jobs acting on a campaign.

## Acceptance

The flow is done when BRIEF § 9's checks hold:
- **The steps replay on a fresh database** (`tools/scenario-replay.ps1`):
  - Play Acts 1–2 and Act 3 through Step 3.33, then Steps 3.34–3.49.
  - Play the appendix through to Steps A.4.7 and A.7.5.
  - Every Expect holds. Change any Expect whose on-screen wording the redesign changes (for example, "from 1
    response"), in the same task (BRIEF § 9, item 7).
- **The 49 states are re-captured** into `design/baseline/states/`, with the steps' captures `3.34-1` to `3.49-1`,
  `A.4.7-3`, `A.4.7-4`, `A.7.5-2` and `A.7.5-3`. The scratch states (`campaign-report--blocked`, `--no-epa` and
  `msf-respond--expired`) need the scratch database in `states.md`. Compare them with the chosen artboards.
- **`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj` is green,** without `--no-build`. That run includes
  `Design/*`, `Accessibility/*`, `Navigation/*` (for T331) and T294's scenario guard.
- **A browser check passes at 1280 and 390 px** for:
  - Mr Smit;
  - an anonymous respondent, in a browser signed in to nothing, where the page must work as plain HTML;
  - Dr Molefe.

## Appendix: the runbook steps (paste after the ask)

Verbatim from `execution/knowledge/scenario-paediatrics/`: Role, Route, Do and Expect only. `D` is the replay day.

```
Step 3.34 — Mr Smit creates the MSF questionnaire
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/new
Do: Open MSF campaigns and start a new campaign. The Template select is empty, so add the Quick template as it stands:
  kind Multi-source feedback, name "Default MSF", with its scale question and its comment question.
Expect: The list reads "No MSF campaigns", with MSF coverage and New campaign in its header. The create page shows a
  Quick template card and a Create campaign card. Adding the template reads "Template created.", and the Template
  select now offers Default MSF.

Step 3.35 — Mr Smit creates a campaign for Dr Molefe
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/new → /msf/campaigns/{CampaignId:int}
Do: Fill in the campaign:
  - trainee Dr Lerato Molefe and template Default MSF;
  - opens on `D` and closes on `D+14`, the defaults;
  - minimum responses 5, minimum category responses 2 and minimum reporting categories 2;
  - evidence for PAED-010 and PAED-012.
  Create it.
Expect: The Trainee select offers KGK's five current trainees by name and address, and nobody else. Once a trainee is
  chosen, "Evidence for these EPAs" lists all 15 EPAs of her curriculum, since MSF is on every one (D37). The help
  beside the reporting categories says that two is the College's answer (D11). Creating it opens the campaign: Draft,
  Lerato Molefe, Default MSF (Multi-source feedback), window `D` to `D+14`, and "Nobody has been invited yet." Open
  campaign is shown disabled, with "Open campaign: add at least one invitee first…" below it (T225).

Step 3.36 — Mr Smit invites seven colleagues, and removes one added under the wrong group
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: Add each colleague by address and respondent group:
  - Sister Grace Mokwena, `grace.mokwena@kgk.wombat.local`, as a Nurse;
  - Sister Palesa Tau, `palesa.tau@kgk.wombat.local`, by mistake as an Allied health professional;
  - the registrars Dr Kagiso Motsepe, `kagiso.motsepe@kgk.wombat.local`, and Dr Lindiwe Khoza,
    `lindiwe.khoza@kgk.wombat.local`, as Peer doctors;
  - Dr Khumalo, `khumalo@kgk.wombat.local`, and Dr Botha, `botha@kgk.wombat.local`, as Consultants;
  - the physiotherapist Ms Naledi Sebego, `naledi.sebego@kgk.wombat.local`, as an Allied health professional.
  Then remove Sister Tau's row and add her again as a Nurse.
Expect: Each add reads "Invitee added." and clears the address for the next one. The group select offers Peer doctor,
  Consultant, Nurse, Allied health professional and Other; it offers no Patient and no Learner. Remove asks "Remove this
  invitee?", naming the address and group. Confirmed, it says she "has been removed from this campaign, and will not be
  emailed a link when it opens." In the end the counts read Peer doctor 2, Consultant 2, Nurse 2, Allied health
  professional 1 and All groups 7, with no Responded column. "Addresses invited" lists the seven, each with Remove, and
  Open campaign is enabled.

Step 3.37 — Mr Smit opens the campaign
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: Open the campaign.
Expect: The page reads "Campaign opened; links are being sent." and State: Open. The note names `D+14` as the
  respondents' last day, and says the campaign can be closed sooner from its report. "7 links are still being sent.
  Reload this page to see whether they were delivered." The invitees are now counted only, with a Responded column
  reading 0, and no address is listed (T217, T247). The actions are Withdraw campaign and View report. The application
  log holds seven "Feedback request: Lerato Molefe (Default MSF, …)" emails, each with a `/msf/respond` link.

Step 3.38 — Mr Smit withdraws a campaign started in error
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/new → /msf/campaigns/{CampaignId:int} → /msf/campaigns
Do: Create a second campaign, for Dr Pieter du Plessis: Default MSF, the defaults, evidence for PAED-015. Then
  Dr Mokoena says his MSF belongs to next semester, so withdraw it from the campaign list.
Expect: The list shows two rows, each with links named by its state (T225):
  - Lerato Molefe, Open: "Manage", "View report" and Withdraw;
  - Pieter du Plessis, Draft: "Manage" and Withdraw.
  Withdraw asks "Withdraw this campaign?", names "Pieter du Plessis (Default MSF, closing `D+14`)", and says withdrawing
  cannot be undone. Confirmed, it reads "The campaign for Pieter du Plessis (…) has been withdrawn. Its respondents'
  links no longer work, and their email addresses have been removed." The row then reads Withdrawn, with only "View
  campaign".

Step 3.39 — Three respondents answer from their links
Role: Anonymous — Sister Grace Mokwena, Sister Palesa Tau and Dr Kagiso Motsepe (MSF respondents)
Route: /msf/respond
Do: Each respondent opens their own link from the log, in a browser signed in to nothing, and answers:
  - Sister Mokwena: Above expectations, "Calm and kind with parents at night; hands over clearly".
  - Sister Tau: Well above expectations, "Leads neonatal resuscitations and explains her decisions to the nurses".
  - Dr Motsepe: Meets expectations, "A supportive senior; could delegate more".
Expect: The page reads "Feedback on Lerato Molefe", Default MSF, and last day to respond `D+14`. It says their name and
  address are never shown to her, and that she sees the feedback only after it has closed and been released, grouped by
  respondent role. The rating offers five points, from Well below expectations to Well above expectations (D47), and is
  required. The comment is optional, with help warning that it may reach her word for word. Each submit ends on "Thank
  you" and "Your feedback on Lerato Molefe has been recorded."

Step 3.40 — A used link is refused
Role: Anonymous — Dr Kagiso Motsepe (MSF respondent)
Route: /msf/respond
Do: Open the same link again.
Expect: The page reads "Feedback link already used" and shows no questionnaire.

Step 3.41 — Mr Smit reads the report while the campaign is open
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns → /msf/reports/{CampaignId:int}
Do: Open Dr Molefe's report from the campaign list.
Expect: The report reads State Open, total responses 3, minimum responses 5, minimum category responses 2, and
  reporting categories 1 of 2 required. "Evidence for" lists PAED-010 and PAED-012, and says one record per EPA is
  written when the report is released. A Nurse card gives the rating's average, 4.50 from 2 responses, and both
  comments. A Peer doctor card reads "Insufficient responses in this category. Results are suppressed." A group that
  has not answered has no card. Close campaign is offered; Release is not.

Step 3.42 — Three more respondents answer, and one never does
Role: Anonymous — Dr Fatima Khumalo, Dr Sarah Botha and Ms Naledi Sebego (MSF respondents)
Route: /msf/respond
Do: The three answer; Dr Khoza does not open her link.
  - Dr Khumalo: Above expectations, "Sound judgement in clinic; teaches the interns well".
  - Dr Botha: Above expectations, "Ready to run a ward; should document escalation decisions more fully".
  - Ms Sebego: Above expectations, "Refers early and listens to the therapists".
Expect: Each submit ends on "Thank you" and "Your feedback on Lerato Molefe has been recorded." Six of the seven links
  have now been used; Step 3.43 reads the counts.

Step 3.43 — Mr Smit resends the undelivered link
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: At least an hour after the open, open the campaign and resend.
Expect: The counts read Responded: Peer doctor 1, Consultant 2, Nurse 2 and Allied health professional 1, and All
  groups 7 invited, 6 responded. A warning reads "1 link was not delivered. Resend sends each of these respondents a new
  link; this page never says who they are.", with a "Resend 1 link" button. After the resend, the page reads "1 new link
  is being sent." and "1 link is still being sent…", and the warning is gone. The log holds one more "Feedback request:
  Lerato Molefe …" stub email. It names no address; it is Dr Khoza's, the only link not answered.

Step 3.44 — Mr Smit closes the campaign
Role: Coordinator — Mr Pieter Smit
Route: /msf/reports/{CampaignId:int} → /msf/campaigns/{CampaignId:int}
Do: On the report, close the campaign. Then open its campaign page.
Expect: The report reads "Campaign closed and anonymised for review." and State: Under review:
  - total responses 6; reporting categories 2 of 2 required;
  - the Nurse card (4.50 from 2) and the Consultant card (4.00 from 2) show their comments;
  - Peer doctor and Allied health professional are suppressed.
  Release to trainee is enabled. Beside it are the narrative and the optional "Supervision level this feedback
  supports", which offers Not stated and the six rungs (D10). The campaign page says the campaign is closed to
  responses and every respondent's address has been removed. It offers Withdraw campaign and "Review and release".

Step 3.45 — A link opened after the close is refused
Role: Anonymous — Dr Lindiwe Khoza (MSF respondent)
Route: /msf/respond
Do: Open the resent link.
Expect: The page reads "Feedback request closed" and shows no questionnaire.

Step 3.46 — Mr Smit releases the report to Dr Molefe
Role: Coordinator — Mr Pieter Smit
Route: /msf/reports/{CampaignId:int} → /msf/campaigns
Do: Write the narrative "Consistently strong feedback from nursing and consultant colleagues; keep documenting
  escalation decisions". Leave the supervision level Not stated, since he is not a clinician. Release.
Expect: The report reads "Report released to the trainee." and State Released. The actions card shows the narrative
  and "Not stated" as stored, with no form (T246). "Evidence for" lists PAED-010 and PAED-012, with the time they were
  recorded on the portfolio. In the list, the row reads Released, with "View campaign" and "View report" and no
  Withdraw.

Step 3.47 — Dr Molefe reads her released report
Role: Trainee — Dr Lerato Molefe
Route: /msf/my-reports → /msf/my-reports/{CampaignId:int}
Do: Open MSF reports, then view the report.
Expect: One row: Default MSF (Multi-source feedback), released just now, 6 responses. The report shows the narrative
  and two groups only, Nurse (4.50) and Consultant (4.00), each with its comments. It shows nothing of the peer doctor
  or the physiotherapist, not even that they answered, and no group's count (T249).

Step 3.48 — Dr Molefe's record and progress show her evidence and the feedback
Role: Trainee — Dr Lerato Molefe
Route: /activities/mine → /portfolio/progress
Do: Open My activities, then My progress.
Expect: My activities holds her six Completed WBAs and two Multi-Source Feedback (Paediatrics) rows. The MSF rows are
  PAED-010 and PAED-012, encounter date `D` (the day the campaign closed), Recorded, credit "—" (D8). My progress reads:
  - semester targets "1 of 10 EPAs met this semester", and training year 4;
  - multi-source feedback "2 of 15 EPAs covered by a released campaign that closed this semester. MSF is tracked on its
    own and counts towards no target.";
  - PAED-001: "3 of 3 this semester", "Target met for Semester 2, 2026. At the minimum level when observed: 2 of 3.",
    minimum now 5;
  - PAED-012 "2 of 3" and PAED-010 "1 of 3", each with "MSF in Semester 2, 2026: covered by a released campaign that
    closed on …".
  The trajectory charts PAED-001 (3 observations from 3 distinct assessors), PAED-010 and PAED-012, and plots no MSF
  point (D36).

Step 3.49 — Mr Smit reads the programme's MSF coverage
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns → /msf/coverage
Do: Open MSF coverage from the campaign list.
Expect: One programme: "Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching Hospital".
  - Semester 1, 2026 reads "5 trainees, whose programme had started by 30 June 2026". Semester 2, 2026 reads
    "5 trainees, whose programme had started by 31 December 2026".
  - By EPA, in Semester 2, 2026: PAED-010 and PAED-012 read "1 of 5 trainees covered", in bold. Every other cell reads
    "0 of 5 trainees covered".
  - By trainee: Lerato Molefe reads "2 of 15 EPAs covered" in Semester 2, 2026; everyone else reads 0 of 15.
  There is no badge, tint or bar, because coverage is not a target (D8, D9). The withdrawn campaign counts for nothing.

Step A.4.7 — Mr Smit signs out through the confirmation page
Role: Coordinator — Mr Pieter Smit
Route: /account/logout-confirm → / → /account/logout → / → /account/logout-confirm → /account/logout/submit → /account/login → /msf/campaigns → /account/login → /msf/campaigns
Do: Open the sign-out confirmation by its address and press Cancel. Type `/account/logout` and press Cancel. Open the
  confirmation again and sign out. Then open MSF campaigns by its address.
Expect: A "Sign out" page, with no nav: "You are signed in as Pieter Smit (smit@kgk.wombat.local). Signing out ends
  your session in this browser.", with Cancel and Sign out. Cancel returns him to his dashboard, still signed in. A
  typed GET /account/logout draws the same page and signs nobody out (T317). Sign out lands on the sign-in page with
  "You have signed out." MSF campaigns then asks him to sign in, and after signing in he is brought back to it. The
  audit log records a Logout.

Step A.7.5 — Mr Smit on his phone
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions due and the data-rights queue.
Expect: The campaign page keeps its gutter (T226), and its invitee table (counts by respondent group: the page never
  lists an address) scrolls within its card. Decisions due's summary scrolls sideways, and a keyboard can scroll it, as
  a labelled, focusable region (DESIGN.md's decisions-due contract). The queue's filters stack above its table.
```
