# F10 An MSF campaign from set-up to release, and the anonymous answer

**Who and why:** a registrar's multi-source feedback (MSF) is one campaign a semester covering many EPAs (D9): the
Coordinator writes a questionnaire once, starts a campaign for the registrar, invites colleagues by address and
respondent group, opens it, resends a link that was not delivered, closes it and releases the report with a narrative.
Colleagues answer from an emailed link on `/msf/respond`, the only page people without an account ever see, and the
registrar then reads the released report, grouped by respondent role, with no respondent named. Anonymity must hold,
and a release writes one evidence record per EPA on her portfolio (D8: it counts towards no target). Six pages carry
it. None has been touched since flow 01 gave them the shell, sentence-case titles and the lit owner (T331); the step
captures are from flow 09's replay (2026-10-14), the state captures from 2026-09-26, on the old frame.

**Built 2026-10-15 (T369, step 6),** from round 3 (`10-msf-campaign/round-3-check.md`): the proposal P; the report RA
with RB's group header and RC's Released one-liner; MSF reports MA, her report on a page of its own; F1–F11. New MSF
campaign is a page of its own (`M/NewCampaign.razor`), and so is her report (`M/MyMsfReport.razor`). F2's cast: a third
Nurse, Sister Thandi Nkosi, and a third Consultant, Dr Johan van Wyk, both invited at 3.36 and answering at 3.42 (8 of
9 respond): Consultant 4.00 from 3, Nurse 4.33 from 3, Peer doctor and Allied health professional suppressed. §§ 1–7
below are the brief as round 1 was sent it, and describe the pages before the build, except § 5, whose rows say what
the build closed; § 8 is re-synced to the runbook as built. The build's own record is `10-msf-campaign/build-*.md`.

The journeys as built (presses + fields typed; a select left as it is counts no field; the same at 1280 and 390 unless
noted):

| Journey | Steps | Count |
|---|---|---|
| Create the questionnaire and the campaign, from Home's Start an MSF campaign: Add template; Trainee and Minimum responses; two EPAs ticked; Create campaign | 3.34–3.35 | 5 + 2 |
| Invite nine and remove one: nine adds, Remove…, Remove invitee, one add (ten addresses, one per add; the group is kept between adds: seven of the nine adds change it, and the last, so eight group changes) | 3.36 | 12 + 18 |
| Open | 3.37 | 1 + 0 |
| Withdraw from the list: Withdraw…, Withdraw campaign | 3.38 | 2 + 0 |
| A respondent answers: a point, the comment, Submit feedback | 3.39 | 2 + 1 |
| Resend, from the list: Manage, Resend 1 link | 3.43 | 2 + 0 |
| Close, from the list: View report, Close campaign…, Close campaign | 3.44 | 3 + 0 |
| Release, from the list: Review and release, the narrative, Release to trainee…, Release to Lerato Molefe | 3.46 | 3 + 1 |
| Dr Molefe reads her report, from Home: MSF reports, View | 3.47 | 2 + 0 (3 + 0 at 390, Menu first) |
| Mr Smit reads coverage, from the list: MSF coverage | 3.49 | 1 + 0 |

Restated 2026-10-15 for T369 (the brief of 2026-09-26 predates flows 01–09). Paths to screenshots are under
`design/baseline/` (gitignored). Code is cited at `cc80d5b3`; `W/` is `src/Wombat.Web/Components/Pages/`, `M/` is
`W/MultiSourceFeedback/`, `A/` is `src/Wombat.Application/Features/`, `MSF/` is `A/MultiSourceFeedback/`, `Nav/` is
`src/Wombat.Web/Navigation/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **F** in BRIEF.md § 8: the List, Form and Detail shapes are expected to hold. Two pages are less settled than the F suggests and get variations (Mode). |
| Mode | **Straight to fidelity**, kept from the old brief, read as BRIEF.md § 2.4 says: an F flow whose structural round may be a single proposal to confirm. Round 1 is at full fidelity: ONE proposal for the campaign list, the campaign page (create and manage), MSF coverage and `/msf/respond` (on its own card: static and anonymous, DESIGN.md § Anonymous static page), to confirm or correct; and 2–3 variations for the two pages the audit found open: the coordinator's report (where Close campaign and Release to trainee sit, with no confirmation today though neither can be undone), and the registrar's MSF reports (a narrow list card beside the chosen report, `M/MyMsfReports.razor:23–92`, where flow 09 gave My committee reviews a page per review). Round 2: the pick with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768 (§ 11, flow 06) |
| Variations | One proposal for four pages; 2–3 for the report and for the registrar's MSF reports in round 1; the chosen one in round 2 |
| People | Coordinator: Mr Pieter Smit (KGK). Anonymous respondents, colleagues of Dr Molefe with no account: Sister Grace Mokwena and Sister Palesa Tau (nurses), Dr Kagiso Motsepe and Dr Lindiwe Khoza (registrars, peer doctors), Dr Fatima Khumalo and Dr Sarah Botha (consultants), Ms Naledi Sebego (physiotherapist); Dr Khoza never answers. Trainees: Dr Lerato Molefe (training year 4; the campaign's subject), Dr Pieter du Plessis (year 2; a campaign withdrawn as a draft), Dr Anele Dlamini (year 3; typing Dr Molefe's report id). SpecialityAdmin: Dr Refilwe Mokoena, who asks for du Plessis's withdrawal (`README.md` § Cast) |
| Runbook steps | 18, pasted verbatim in § 8: 3.34–3.49 (Act 3, Phase 3.G), A.4.7 and A.7.5 (appendix). 3.48 lands on flows 03 and 05's pages; A.4.7 passes through flow 02's sign-out page |
| Pages (`coverage.md` templates) | `/msf/campaigns` (`M/CampaignsList.razor`, 269 lines), `/msf/campaigns/new` and `/msf/campaigns/{CampaignId:int}` (`M/CampaignEdit.razor`, 1245: one page for both, its shared words in `M/MsfCampaignText.cs`, 150), `/msf/reports/{CampaignId:int}` (`M/CampaignReport.razor`, 462), `/msf/respond` (`M/MsfRespond.razor`, 458: static, `[ExcludeFromInteractiveRouting]`, `AuthLayout`), `/msf/my-reports` and `/msf/my-reports/{CampaignId:int}` (`M/MyMsfReports.razor`, 144: the list and, beside it, the report chosen), `/msf/coverage` (`M/ProgrammeCoverage.razor`, 180; words in `M/MsfProgrammeCoverageText.cs`, 99). Arrived at from `/` (the Coordinator Home's header action "Start an MSF campaign", `Nav/HomeFrame.cs:52`, flow 06's) and the menus. Checked at `cc80d5b3`: `git log` shows no change to any of them since `b347e11c` (flow 01). |
| Menus (`Nav/NavItems.cs`) | MSF campaigns is in the Coordinator's flat menu (`:102`, `:175`); the campaign page, the report and MSF coverage light it for the Coordinator, under the trail Home › MSF campaigns › the page (`Nav/NavOwners.cs:129–132`, T331). The Administrator is admitted to all four Coordinator pages (`[Authorize(Roles = "Coordinator,Administrator")]`) but has no item for them (`:142–148`) and nothing lit, trail Home. MSF reports is in the Trainee's menu (`:93`, `:179`) and lights itself, no trail; its page also admits the Coordinator and the Administrator (`M/MyMsfReports.razor:3`, T269), whom no menu offers it. MSF coverage has no item: an outline link in the list's header (`M/CampaignsList.razor:17`; DESIGN.md:655–657). `/msf/respond` is outside the rule: nothing lit, no trail (`Nav/NavOwners.cs:185`). The PendingTrainee's menu is Home alone (D55, `:182`). A registrar whose programme ends loses the Trainee role (D54) and with it MSF reports (Step 5.22, flow 13). |
| Components drawn at runtime | `PageHeader` (one title, "MSF campaign", on create and on every campaign, `M/CampaignEdit.razor:12–14`; MSF coverage adds a "Back to campaigns" header button beside its trail, `M/ProgrammeCoverage.razor:16`), `StatePanel` (no page passes `OnRetry`), `DataTable` (the list, captioned, no `Stack`, `M/CampaignsList.razor:48`; the registrar's list, no caption, no `Stack`, `M/MyMsfReports.razor:27`), raw `table.clinic-table` (the invitee counts `M/CampaignEdit.razor:215–254`, the draft's addresses `:266–300`, coverage's two tables `M/ProgrammeCoverage.razor:59–139`), `section.detail-card` in `.details-grid`, `dl.details-list` (the campaign card; the report's Summary is `<p><strong>` lines, `M/CampaignReport.razor:36–42`), `FormField`, `FormActions`, `FieldHelp`, `EditForm` with `DataAnnotationsValidator` and `ValidationSummary` (four forms), `Alert` (every result and refusal), `ConfirmDialog` (Withdraw on the list and on the campaign page, Remove invitee; none for Close campaign or Release to trainee), `BadgeFor.MsfCampaign` (the campaign page only; the list and the report print the state as text), `InFlight` (Resend only), a hand-rolled `div.action-result` (not `ActionResult`) on the three Coordinator pages that act, `ul.workflow-action-reasons` (Open's reason), `.check-grid` (the EPAs), `.scale-choices` and `.account-form-container--wide` (`/msf/respond`). Queries: `ListMsfCampaignsForCoordinator`, `ListMsfTemplates`, `ListMsfCampaignSubjects`, `GetMsfCampaignSetup`, `GetCampaignAggregateReport`, `GetMsfResponseForm`, `ListMsfCampaignsForTrainee`, `GetMsfProgrammeCoverage`; commands `CreateMsfTemplate`, `CreateMsfCampaign`, `AddMsfInvitation`, `RemoveMsfInvitation`, `OpenMsfCampaign`, `ResendMsfLinks`, `CloseMsfCampaign`, `ReleaseMsfCampaign`, `WithdrawMsfCampaign`, `SubmitMsfResponse` (all `MSF/`). Mail: `MsfInvitationEmail` (open and resend) and `MsfExpiryReminderEmail` (a job), flow 19's. |
| Held | None. Every step capture is from the T366 replay (2026-10-14), on the shell as built. Every `states/` capture of these pages (`campaigns-list--*`, `campaign-edit--*`, `campaign-report--*`, `msf-respond--*`, `my-msf-reports--*`, `programme-coverage--*`) is from 2026-09-26, before flow 01: a flat Title Case sidebar, an email in the top bar, the old alert colours (checked by opening `campaign-edit--draft` and `msf-respond--used`). The step captures stand in for them where there is one (§ 3) |
| Depends on | Flow 01, built (`b347e11c`): the shell, the trail, the lit owner on these pages (T331), the tokens' contrast (T322), `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again, access denied and not found. Flow 02 (`f50dffb2`): the auth card's heading is its `<h1>` (not yet on `/msf/respond`); the sign-out page A.4.7 crosses. Flow 03 (`725237ee`) and flow 04 (`06aa51d7`): row actions named per row; My activities' MSF rows. Flow 05 (`b020c942`): ISO dates (D1), stacked rows below 641 px, My progress's MSF line. Flow 06 (`85d5a508`): the Coordinator's Home and its "Start an MSF campaign" action. Flow 07 (`44de180b`): the review page's MSF coverage panel. Flow 08 (`622c5ad1`): one action a row. Flow 09 (`49e7cf5e`): a review page of its own per item (My committee reviews), `ConfirmDialog` opening on Cancel. The design system is **version 23**, re-synced to flow 09 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 10`. Open each screenshot. The script puts any
   capture whose name contains "invit" or "resent" in `design/upload/crop-first/` (`act-3/3.36-1-remove-invitee-dialog.png`,
   `act-3/3.36-2-seven-invitees.png`, `act-3/3.43-2-link-resent.png`, `states/campaign-edit--resent.png`): each was
   opened on 2026-10-15 and shows a campaign page with `wombat.local` addresses, no link and no token, so they go as
   they are. Nothing needs a crop: the tallest captures (2,000–2,100 px) go whole and the canvas sees them scaled.
2. **Start a new canvas** from the main app's Design page, "Wombat 10", on the Wombat design system v23
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not an earlier flow's canvas. Write its `tokens.css` by hand
   from app.css's `:root` before round 1 (§ 11, flow 09: the system serves only `tokens.json`).
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`). Put the returned `/_blob/` URL in § 1 and in
   `design/flows/10-msf-campaign/round-1-ask.txt`, where they read `/_blob/d5b5890c1eb0efaad13d0d347d1ebf4c` until then.
4. **Send § 1 and § 8 as ONE message** (`design/flows/10-msf-campaign/round-1-ask.txt`), with the key screenshots in
   § 4. Read `project/canvas.json`'s version before and after: a send can draw nothing. Read round 1's notes for a line
   saying it never received the attachments (§ 11, flow 06); re-attach and name the folder before round 2 if so. Round
   1 of flow 09 drew invented people: say again that every person and value comes from § 1 and § 8.
5. **Confirm or correct the proposal, pick a variation for each of the two open pages,** say why in a sentence, and
   answer § 6's questions one sentence each; the operator's are marked. A question whose answer rests on an open task
   names it, and the task lands before the build (§ 11, flow 05). Ask for round 2.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/09-outcomes-stars-appeals/round-2-review.md`
   did: one reviewer on the code (and the ask), one on the cast and the runbook, one on the design system. Put the
   decisions to the operator, then ask for one correction round.
7. **Record every round** in `design/flows/10-msf-campaign/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 10 — Run a registrar's multi-source feedback campaign from set-up to release, and answer one anonymously

This is flow 10 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands),
  06 (watching the programme), 07 (the review sitting), 08 (panels, what is due, the schedule) and 09 (STARs, the
  register, appeals) are designed and built; the signed-in pages sit inside the shell. Straight to fidelity: round 1
  is ONE proposal, at full fidelity, for the campaign list, the campaign page (create and manage), MSF coverage and the
  respondent's page, for me to confirm or correct, plus 2–3 variations for the two pages whose shape is open: the
  coordinator's report (with Close and Release) and the registrar's MSF reports. I pick before round 2. The runbook
  steps follow below in this same message. The brand mark is at /_blob/d5b5890c1eb0efaad13d0d347d1ebf4c: use it, never a drawn disc. Every person,
  address, count and comment comes from this message and the steps below; invent none.

GOAL: The Coordinator must be able to:
    - write a questionnaire once (a rating and a comment), and start a campaign for a current registrar: the window,
      three minimums, and the EPAs it is evidence for;
    - invite colleagues by address and respondent group, remove one added by mistake while it is a draft, and open it;
    - follow it while it is open, by group and never by name: how many were invited, how many answered, and how many
      links were not delivered, and resend those;
    - read the report, close the campaign, and release it to the registrar with a narrative and an optional
      supervision level, knowing that neither close nor release can be undone; or withdraw a campaign started in
      error;
    - see across the programme which registrars a released campaign has covered, per EPA and semester.
  A colleague with no account must be able to open the link from the invitation, see whom the feedback is about and
  what she will and will not see, answer, and be told it was recorded; or be told plainly why the link no longer works.
  The registrar must be able to find her released report and read it: the narrative, and each group that cleared the
  threshold, with nothing of a group that did not.
  What is wrong today (the attached screenshots show each; all on the shell as built, 2026-10-14):
  - Close campaign and Release to trainee have no confirmation, though closing removes every respondent's address for
    good and releasing writes evidence to her portfolio; their result is a page-top alert (3.44, 3.46).
  - The report's summary is seven bold-label lines; the actions card sits in a third column under the summary; each
    group is a card of its own with "Average: 4.50 from 2 responses." ("from 1 responses" with one); the release time
    reads "2026-10-14 14:34 UTC" (3.44, 3.46).
  - The registrar's MSF reports is a narrow list card beside the report: at 1280 its table is cut ("Respons"), her
    release time reads "2026-10-14 16:34" with no zone (the coordinator's page said 14:34 UTC), each rating reads
    "Rates the trainee's overall professional performance.: 4.00" (a colon after a full stop), and the report has no
    heading of its own (3.47). Another trainee's id shows its refusal twice, in place of her own list.
  - The draft's address table breaks every address mid-word at 1280, and its Remove buttons sit far from them; the add
    form names the group "Category" where both tables say "Respondent group" (3.36).
  - The campaign list scrolls sideways at 390, its actions off-screen (A.7.5); its State is plain text where the
    campaign page shows a badge; "Subject" there, "Trainee" everywhere else; the closing date wraps mid-date at 1280.
  - The create page and every campaign share one title, "MSF campaign", and one subtitle. The create page's dates
    default from the server's calendar, and a failed read of its templates or trainees ends the live connection (no
    error shown). A campaign withdrawn as a draft shows a Responded column.
  - /msf/respond has no <h1> (each state is an <h2>), and puts two paragraphs and a note above the first question.
  - Every failed read prints the exception's own text.

AUDIENCE:
  - Coordinator: Mr Pieter Smit, Kgosi Kgari Teaching Hospital (KGK), Paediatrics; at a desk and on his phone. He is not
    a clinician, so he leaves the supervision level Not stated.
  - Anonymous respondents, no account, may open the link on a phone: Sister Grace Mokwena and Sister Palesa Tau
    (nurses), Dr Kagiso Motsepe and Dr Lindiwe Khoza (registrars: peer doctors), Dr Fatima Khumalo and Dr Sarah Botha
    (consultants), Ms Naledi Sebego (physiotherapist: allied health professional). Dr Khoza never answers.
  - Trainees: Dr Lerato Molefe (training year 4; her campaign covers PAED-010 and PAED-012), Dr Pieter du Plessis
    (year 2; a campaign withdrawn as a draft), Dr Anele Dlamini (year 3; types Dr Molefe's report id).
  - KGK follows the College's (CPSA) curriculum 11.1: fifteen EPAs, PAED-001 to PAED-015, MSF an instrument on every
    one. Frequency: one campaign per registrar per period, about 16 returned questionnaires per registrar a year (D9).
    Stakes: medium. Anonymity must hold; a release is evidence on her record. Desktop 1280×800 and phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - The campaign list (Coordinator, Administrator; "Create and review multi-source feedback campaigns."): the
    campaigns of the caller's institution's trainees, columns Subject, Template (with its kind, "Multi-source
    feedback"), State, Invitations, Responses, Closes, and actions named by state: Draft "Manage" and Withdraw; Open
    "Manage", "View report" and Withdraw; Under review "View campaign", "Review and release" and Withdraw; Released
    "View campaign" and "View report"; Withdrawn "View campaign". Each named for its row: "Manage: the campaign for
    Lerato Molefe (Default MSF, closing 2026-10-28)". Header: "MSF coverage" (outline) and New campaign. Empty: "No
    MSF campaigns" / "Create the first MSF campaign to start respondent outreach.".
  - Withdraw (list and campaign page): a dialog "Withdraw this campaign?", "Withdraw the campaign for Pieter du Plessis
    (Default MSF, closing 2026-10-28)? It will take no more responses: any link already sent stops working, and every
    respondent's email address is removed. A withdrawn campaign is never released to the trainee, and withdrawing
    cannot be undone." (one under review: "It has closed, and its report has not been released. …"), Withdraw
    campaign / Cancel. Done: "The campaign for … has been withdrawn. Its respondents' links no longer work, and their
    email addresses have been removed.".
  - Creating (`/msf/campaigns/new`): a Quick template card, shown on the create page only: Kind (Multi-source feedback
    or Learner feedback), Template name "Default MSF", Scale question "Rates the trainee's overall professional
    performance.", Comment question "What should the trainee keep doing or improve?", Add template → "Template
    created.". A Create campaign card: Trainee (the institution's current trainees, "Lerato Molefe
    (molefe@kgk.wombat.local)"), Template, Opens on and Closes on (today and +14 days), Minimum responses (default 8),
    Minimum category responses (default 3), Minimum reporting categories (default 2; help "How many respondent groups
    must clear the threshold above before the report can be released. Two is the College's answer: eight peer doctors
    alone are not multi-source feedback."), and "Evidence for these EPAs *", a checkbox per EPA of her curriculum once
    she is chosen ("Select a trainee first." before; help "One campaign covers many EPAs and runs once a semester. Each
    one selected here becomes a separate evidence record on the trainee's portfolio when the report is released.").
    None ticked: "Select at least one EPA this campaign is evidence for.".
  - A campaign's page: Trainee, Template (with kind), State (badge), Response window, and one sentence for the state:
    Draft "Add invitees, then open the campaign to send anonymous response links."; Open "Open. Each respondent is
    emailed a link, which gives 2026-10-28 as their last day to respond. The campaign closes by itself once that day
    has passed; to close it sooner, use Close campaign on its report."; Under review "Closed to responses, and every
    respondent's email address has been removed. Review the responses on its report, and release it to the trainee
    there. If it should never be released, withdraw it here."; Released "The report has been released to the
    trainee."; Withdrawn "Withdrawn. It takes no responses, any link sent no longer works, …".
    Invitees, counted by respondent group (Peer doctor, Consultant, Nurse, Allied health professional, Other; never
    Patient; Learner only on learner feedback): Invited, and Responded once it has opened, with an All groups total;
    "Counted by respondent group. This page never lists who was invited or which of them responded." ("Once the
    campaign opens, …" on a draft). Nobody: "Nobody has been invited yet." / "Nobody was invited.".
    On a draft only: "Addresses invited" ("Listed only while the campaign is a draft, so that an address added by
    mistake can be removed. No invitee holds a working link until the campaign opens."), each address with its group
    and Remove; Remove asks "Remove this invitee?", "Remove palesa.tau@kgk.wombat.local (Allied health professional)
    from this campaign? They hold no working link, and will not be sent one when the campaign opens. While the campaign
    is a draft, they can be added again."; done: "… has been removed from this campaign, and will not be emailed a link
    when it opens.". "Add an invitee": Respondent email, the group, Add invitee → "Invitee added.", the address cleared
    and focused for the next. Open campaign, disabled with "Open campaign: add at least one invitee first. Opening the
    campaign emails each invitee a link to respond." until someone is invited; opened: "Campaign opened; links are
    being sent.".
    While open: "7 links are still being sent. Reload this page to see whether they were delivered."; a warning "1 link
    was not delivered. Resend sends each of these respondents a new link; this page never says who they are." with
    "Resend 1 link" → "1 new link is being sent.". A link nothing was heard of for an hour counts as not delivered.
    Actions by state: Draft Withdraw campaign, Open campaign; Open Withdraw campaign, View report; Under review Withdraw
    campaign, Review and release; Released View report; Withdrawn nothing.
    Not found or out of scope: "Campaign unavailable" / "The selected campaign could not be loaded.".
  - The report (`/msf/reports/{id}`; "Review aggregate results and release them to the trainee."): Summary: Template,
    Kind, State, Total responses, Minimum responses, Minimum category responses, "Reporting categories: 1 of 2
    required". Evidence for: "PAED-010 — Leading and operating within a clinical team", "PAED-012 — Communicating with
    and counselling patients, caregivers and healthcare teams", and "One evidence record per EPA is written to the
    trainee's portfolio when the report is released." (once released: "Recorded on the portfolio: 2026-10-14 14:34
    UTC"; none named: "This campaign names no EPA, so releasing it will record no evidence on the trainee's
    portfolio."). Coordinator actions: Open → Close campaign → "Campaign closed and anonymised for review."; Under
    review → Narrative (up to 4,000 characters), "Supervision level this feedback supports" (Not stated, 1, 2, 3a, 3b,
    4, 5; help "Optional, and blank is a perfectly good answer. Your own judgement as the releasing clinician, informed
    by the aggregate below - never an average of respondent scores."), Release to trainee → "Report released to the
    trainee."; Released → the narrative and the level as stored, no form. Blocked: Release disabled under "Release
    needs 5 responses; 1 have come back. If it should never be released, withdraw it on its campaign page." (or "Release
    needs 2 respondent groups to report; 1 cleared the 2-response threshold. Feedback that survives from too few groups
    is not multi-source feedback."). A card per group that answered: the rating's average "from n responses" and the
    comments, verbatim; a group under its threshold: "Insufficient responses in this category. Results are
    suppressed."; a group that did not answer has no card. Unavailable: "Report unavailable".
  - /msf/respond (anonymous, static HTML, form post; the sign-in card widened, no nav): "Feedback on Lerato Molefe",
    "Default MSF · Last day to respond: 2026-10-28", "You have been asked to give multi-source feedback on Lerato
    Molefe, a trainee you have worked with. Your name and email address are never shown to Lerato Molefe. They see the
    feedback only after the request has closed and a coordinator has reviewed and released it, grouped by respondent
    role.", "For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of
    training.", "Questions marked * must be answered."; the rating, required, five radios from Well below expectations
    through Below, Meets and Above to Well above expectations (D47); the comment, optional, "Up to 4000 characters. Your
    comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you."; Submit
    feedback. Done: "Thank you", "Your feedback on Lerato Molefe has been recorded.", "The link in your invitation has
    now been used, so it will not open the questionnaire again. You can close this page.". Refusals, each a title, a
    sentence and "You can close this page.": "Feedback link already used" ("This feedback link has already been used: a
    response was submitted through it, and each link takes one response."), "Feedback request closed" ("This feedback
    request has closed and is no longer accepting responses."), "Feedback link expired", "Feedback link not
    recognised". A fault: "Something went wrong" ("… If you had submitted the form, opening your link again will say
    whether your feedback was recorded."). A rating left out is the browser's own required prompt. Learner feedback
    asks "feedback on the teaching of Lerato Molefe, a trainee who has taught you" and groups learners together.
  - MSF reports (Trainee; "Released aggregate feedback only. Individual responses are never shown."): her released
    reports, newest first: Template (with kind), Released, Responses, View ("View the Default MSF report, released
    …"); empty "No released reports" / "Released multi-source feedback will appear here.". The report: Template,
    Narrative, and only the groups that cleared the threshold, each its rating's average and its comments; no group's
    count, nothing of a suppressed group, not even that it answered; for learner feedback the number of teaching
    contexts, never their names. Another trainee's id: her own list, nothing selected (today the refusal "The selected
    report is not available to the current trainee." twice, in place of the list).
  - MSF coverage (Coordinator, Administrator; "How many of each programme's trainees a released multi-source feedback
    campaign covered, per EPA and semester."): per programme ("Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching
    Hospital"), each semester "5 trainees, whose programme had started by 30 June 2026"; By EPA, a row per EPA and a
    column per semester, "1 of 5 trainees covered" (in bold when above 0); By trainee, "2 of 15 EPAs covered". No badge,
    tint or bar: coverage is not a target (D8, D9). Nobody: "No trainees to show".
  - Elsewhere, kept as flows 03 and 05 built them: My activities lists each released EPA's record ("Multi-Source
    Feedback (Paediatrics)", Recorded, credit "—"); My progress reads "Multi-source feedback: 2 of 15 EPAs covered by a
    released campaign that closed this semester. MSF is tracked on its own and counts towards no target.".
  - Data volumes: none (no template, no campaign); typical (one questionnaire, two campaigns, seven invitees in four
    groups, six responses, two groups shown and two suppressed); heavy (a campaign for each of five registrars in each
    of two semesters, a group with a dozen comments of up to 4,000 characters, all fifteen EPAs ticked).

WHAT FLOW 10 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell (flow 01: the trail, the acting role, access denied, not found); the menus' items (propose a
    change, do not draw it as settled); the sign-in card's frame (flow 02) beyond widening it for the questionnaire;
    My activities and My progress (flows 03 and 05); the Coordinator's Home (flow 06); the rules above (who runs a
    campaign: a Coordinator at the trainee's institution or an Administrator, never about themselves, never someone who
    holds Trainee; only a current trainee for a new campaign, an invitee or an open; the thresholds and suppression;
    one response per link; that no page lists an address or who answered once a campaign opens; that the registrar
    sees no group's count and nothing of a suppressed group).
  May, for these pages: their whole layout (where the actions sit, the report's summary as a list or a line, the
    groups' cards, the registrar's report on a page of its own or beside the list, the create page's two cards, the
    draft's addresses as rows that stack), the order and grouping of parts, titles and subtitles (the create page and a
    campaign need not share one), where each result and refusal shows and where focus goes, confirmations, stacking at
    390, and the words. Mark PROPOSED any rule change (a confirmation that is not built, a mail sent, what a reader
    sees of a rating's spread, a new route): those are mine. The runbook's Expects change with the words.
  Say which of these each variation changes, and why.

SCREENS, in order:
  1. The campaign list: none (3.34); a draft and an open campaign (3.38); the withdraw dialog (3.38); a withdrawn and a
     released row (3.38, 3.46); under review; loading; a load error; 390 px (A.7.5).
  2. The campaign page. Creating: no template yet, then "Template created." (3.34); a trainee chosen, fifteen EPAs
     (3.35); no EPA ticked; a template or trainee read that fails. A campaign: draft, nobody invited, Open disabled with
     its reason (3.35); draft with seven invitees and their addresses, Remove's dialog (3.36); opened, links being sent
     (3.37); a link not delivered, then resent (3.43); under review, the addresses gone (3.44); released; withdrawn (du
     Plessis, a draft); the withdraw dialog; unavailable; loading; 390 px (A.7.5). Learner feedback's draft (a teaching
     context per learner) in words.
  3. The report: open, one group shown and one suppressed (3.41); under review, ready, two shown and two suppressed
     (3.44); the close and release confirmations (NEW); released, as stored (3.46); blocked from release; names no EPA;
     unavailable; loading; 390 px.
  4. /msf/respond, on its own card, no nav: the questionnaire (3.39); the rating left out (the browser's prompt);
     thank you (3.39, 3.42); link already used (3.40); request closed (3.45); link expired; link not recognised;
     something went wrong; each at 390 px. Learner feedback's questionnaire in words.
  5. MSF reports, the registrar's: none released; the list and her report (3.47); another trainee's id; loading; a load
     error; 390 px.
  6. MSF coverage: loaded (3.49); nobody to show; loading; 390 px (3.49).

STEPS: 3.34–3.49, A.4.7 and A.7.5. Pasted verbatim below (Role / Route / Do / Expect). In them, D is the day the story
  is replayed. Where an Expect quotes today's wording or layout, that is today's page, not a requirement on the new
  one. Step 3.48 checks the release on My activities and My progress (flows 03 and 05): keep their words, do not
  design them here. Step A.4.7 passes through the sign-out page (flow 02's).

STATES TO SHOW:
  Every state named in SCREENS; loading for every signed-in page (the header and a skeleton, no action offered until the
  read is in); a load error (a sentence and Try again, never an exception's text); each confirmation, its cancel, and
  the result after it with focus on it; a refusal beside the control that caused it, keeping what was typed; 390 px for
  every page; the heavy volumes.

REQUIREMENTS FROM KNOWN DEFECTS:
  - A5 (BRIEF.md § 6): every action reports its outcome where the focus goes and where the person is working; a
    destructive or final action (Withdraw, Remove, Close campaign, Release to trainee) confirms and names its target,
    and its trigger is an outline button.
  - T325 (A9): one clock. The release time reads the same on both pages, in South African time with the zone
    ("2026-10-14 16:34 SAST"); the create page's default dates and a link's last day are the South African date.
  - T270: "from 1 response" (and "1 has come back", "1 respondent group"); the draft's addresses fit at 390 and wrap
    between words at 1280, Remove always beside its address; no Responded column on a campaign that never opened; no
    colon after a question that ends in a full stop; a refused id on MSF reports leaves her own list in place, said
    once; on a draft whose trainee is no longer current, Add invitee and Open campaign are disabled with their reason.
  - T329, T272: a failed read on the create page, or any page here, is an alert with Try again; no exception's or
    framework's text on any page; nothing ends the live connection.
  - T280: every table captioned, every row action named for its row with no two alike; headings in order: one <h1>
    (on /msf/respond too, as flow 02 gave the sign-in card), then <h2>.
  - T323 (A3): nothing at 390 scrolls sideways except a wide table inside its own region (coverage's grids); the list
    and the registrar's list stack.
  - T324: no message names a model property ("The Email field is required."); groups and states by their labels.
  - Content (BRIEF.md § 6): sentence case; people by name as stored; ISO dates (D1); one label for the respondent group
    everywhere (the add form's "Category" and the report's "Minimum category responses", "Reporting categories").
  - Anonymity, a product rule: once a campaign opens no page lists an address or who answered; counts by group only;
    the registrar sees no group's count and nothing of a suppressed group.
  - Done since the old brief, keep them: MSF campaigns lit on the report and coverage pages, with the trail (T331);
    the tokens' contrast for alerts and badges (T322); the campaign page's gutter at 390 (T226); "Category" in Remove's
    dialog already reads the group's label (T225).

QUESTIONS THE DESIGN MUST ANSWER:
  1. Anonymity, visible: the campaign page and the report count by group and never list names. How does the design
     keep that rule in view, so the coordinator does not go looking for names?
  2. The respondent's first view: who they are rating and why, or the question? Today three paragraphs and a note come
     first.
  3. The report: where Close and Release sit; their confirmations (what each says cannot be undone); how the summary,
     the evidence, the thresholds and the groups read together; where a blocked release says why.
  4. The registrar's MSF reports: the report beside the list as today, or a page of its own (a PROPOSED route reached
     from the list, as flow 09 gave My committee reviews)?
  5. The campaign page while open: how the counts, the links being sent and the links not delivered read as one
     status, and where Resend sits.
  6. The create page: the Quick template card beside Create campaign, or a step before it; fifteen EPAs to tick.
  7. (Mine) MSF coverage: a link from the list's header as today, or a tab of the list? And does the Administrator,
     admitted to every campaign page, get a menu item?
  8. (Mine) The registrar is told nothing when her report is released: no mail, no Home card. Tell her, and how?
  9. (Mine) A rating is reported as an average. The report also holds how many chose each point. Does the coordinator,
     or the registrar, see that spread?
  10. (Mine) A campaign whose registrar's programme ends (graduated or withdrew) while it is open or under review can
      still be closed and released, and the release writes evidence on her ended record, which she can no longer open
      (she loses MSF reports with the Trainee role). Keep, refuse the release, or warn on the page?

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
  TrajectoryChart's two drawings with its visible table. Since flow 06: the committee and coordinator Homes, the
  registrar list with its filters. Since flow 07: ReviewSitting, SittingBar, AgendaTable (its container-query stack),
  AgendaLineForm, EvidenceReader, DecisionForm, ReviewReturnLink. Since flow 08: one action a row, the seat
  picker, a choice of a few kinds as radios (choice-group), the trainee line, a form page of its own (Schedule a
  review). Since flow 09: a review page of its own per list row (My committee reviews), the shared decision history,
  "No longer in force", the register's stacked rows with a revoked row's line.
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
Also: each page has exactly one h1, and its headings are in order. A confirmation is flow 01's native <dialog>
  ConfirmDialog. /msf/respond is plain HTML with a form post: no script, no live validation, the browser's own required
  check.

ASK:
  - Round 1, at full fidelity on the design system's tokens: ONE proposal for screens 1, 2, 4 and 6 (Mr Smit's list,
    create page and campaign page at 3.34–3.38 and 3.43–3.44; Sister Mokwena's questionnaire, thank you and Dr
    Motsepe's used link at 3.39–3.40; the coverage at 3.49), and 2–3 variations for screen 3 (Mr Smit's report at
    3.41, 3.44 and 3.46, with Close and Release and their confirmations) and screen 5 (Dr Molefe's MSF reports at
    3.47). Each variation with its reasoning and its step count, at 1280 and 390 px. Stop and wait for my pick.
  - Round 2: the chosen design with every screen and state above, at 1280 and 390 px, each role's view, every
    confirmation, and where the focus lands after each action.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (§ Table system; § Form system; § Page-level patterns: List page, Detail page, and the title rule; § The
    MSF campaign page; § Multi-source feedback coverage and § The programme's MSF coverage; § Anonymous static page).
  - Mark PROPOSED anything not built: a confirmation, a new route, a mail, a rule change.
  - Flag edge cases: learner feedback (one group, learners, a teaching context per learner, no supervision level, the
    report counting contexts); a campaign naming no EPA; a release that drops an EPA no longer on her curriculum
    ("— not recorded"); a draft whose trainee is no longer current; a link resent while the first still works; a
    campaign closed by the nightly job, not by hand; a 4,000-character comment; the 120-character EPA title
    ("PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and
    research"); a group whose threshold is one, which then shows a lone answer.
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, every row
    action named for its row, badges read as words, focus after every action and confirmation; targets ≥ 24 px (44 at
    390); /msf/respond's radios as one fieldset with its legend.

ATTACHED: act-3/3.36-2-seven-invitees.png, act-3/3.43-1-link-not-delivered.png, act-3/3.44-1-report-under-review.png,
  act-3/3.46-1-report-released.png, act-3/3.47-1-molefe-report.png, act-3/3.39-1-mokwena-questionnaire.png,
  act-3/3.40-1-link-already-used.png, act-3/3.35-0-create-form.png, act-3/3.46-2-list-released.png,
  act-A/A.7.5-2-campaigns-390.png and act-3/3.49-1-msf-coverage.png. The two report pages and the draft are 2,000–2,100
  px, which you will see scaled. In every tall capture at 1280 the sidebar's colour stops at 800 px: that is the
  capture, not the page. Other captures dated 2026-09-26 are an older frame (a Title Case sidebar, an email in the top
  bar, older alert colours): read their bodies only.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 3.34 | `/` → `/msf/campaigns` → `/msf/campaigns/new` | Mr Smit | "No MSF campaigns", MSF coverage and New campaign in the header; the Quick template card (pre-filled) beside Create campaign; "Template created.", the select offering Default MSF |
| 3.35 | `/msf/campaigns/new` → `/msf/campaigns/{CampaignId:int}` | Mr Smit | KGK's five current trainees only; fifteen EPAs once Molefe is chosen; the D11 help; he types 5/2/2 over the defaults 8/3/2; Draft, "Nobody has been invited yet.", Open disabled with its reason (T225) |
| 3.36 | `/msf/campaigns/{CampaignId:int}` | Mr Smit | "Invitee added." each time, the address cleared and focused; the five groups (labelled "Category" in the form); Remove's dialog naming address and group; counts 2/2/2/1, All groups 7; the addresses breaking mid-word |
| 3.37 | `/msf/campaigns/{CampaignId:int}` | Mr Smit | "Campaign opened; links are being sent.", Open; the last day; "7 links are still being sent…"; counts only, Responded 0, no address (T217, T247) |
| 3.38 | `/msf/campaigns/new` → `/msf/campaigns/{CampaignId:int}` → `/msf/campaigns` | Mr Smit | two rows, actions named by state (T225); the Withdraw dialog naming trainee, template and closing date; Withdrawn with only View campaign |
| 3.39 | `/msf/respond` | Sister Mokwena, Sister Tau, Dr Motsepe | signed in to nothing: "Feedback on Lerato Molefe", the last day, the anonymity statement, the five-point rating (D47), the comment's warning; "Thank you" |
| 3.40 | `/msf/respond` | Dr Motsepe | "Feedback link already used", no questionnaire (HTTP 410) |
| 3.41 | `/msf/campaigns` → `/msf/reports/{CampaignId:int}` | Mr Smit | Open, 3 against the minimums, 1 of 2 groups; Nurse "4.50 from 2 responses."; Peer doctor suppressed; Close only |
| 3.42 | `/msf/respond` | Dr Khumalo, Dr Botha, Ms Sebego | "Thank you" each; Dr Khoza never opens hers |
| 3.43 | `/msf/campaigns/{CampaignId:int}` | Mr Smit | Responded by group, 7/6; "1 link was not delivered… never says who they are.", "Resend 1 link"; "1 new link is being sent." |
| 3.44 | `/msf/reports/{CampaignId:int}` → `/msf/campaigns/{CampaignId:int}` | Mr Smit | Close (no confirmation): "Campaign closed and anonymised for review.", Under review, two shown, two suppressed; the narrative and level (D10); the campaign page says the addresses are gone |
| 3.45 | `/msf/respond` | Dr Khoza | "Feedback request closed", no questionnaire |
| 3.46 | `/msf/reports/{CampaignId:int}` → `/msf/campaigns` | Mr Smit | Release (no confirmation): "Report released to the trainee.", as stored with no form (T246); "Recorded on the portfolio: … UTC" (T325); the row Released, no Withdraw |
| 3.47 | `/msf/my-reports` → `/msf/my-reports/{CampaignId:int}` | Dr Molefe | one row, 6 responses, released "16:34" unlabelled; the narrative, Consultant 4.00 and Nurse 4.50 with comments, ".: 4.00" (T270); nothing of the suppressed groups, no counts (T249) |
| 3.48 | `/activities/mine` → `/portfolio/progress` → `/portfolio/progress/{EpaId:int}` | Dr Molefe | flows 03 and 05's: two MSF rows, "—" (D8); "2 of 15 EPAs covered…"; "Multi-source feedback is not plotted." (D36) |
| 3.49 | `/msf/campaigns` → `/msf/coverage` | Mr Smit | each semester's trainee count; PAED-010 and PAED-012 "1 of 5 trainees covered"; Molefe "2 of 15 EPAs covered"; no badge, tint or bar (D8, D9) |
| A.4.7 | `/account/logout-confirm` → `/` → … → `/msf/campaigns` | Mr Smit | flow 02's sign-out page; asked to sign in, then brought back to MSF campaigns |
| A.7.5 | `/` → `/msf/campaigns` → `/msf/campaigns/{CampaignId:int}` → `/committee/decisions-due` → `/admin/data-rights` | Mr Smit at 390 | the list scrolling sideways, its actions off-screen; the released campaign keeping its gutter (T226), its counts table fitting its card |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Multi-source feedback. The step
captures are from the T366 replay (2026-10-14), on the shell as built. Every `states/` capture named below is from
2026-09-26, on the old frame (a flat Title Case sidebar, an email in the top bar, the alert colours before T322; on
`/msf/respond`, the card centred and the old warning colour): use the step capture where there is one, and the old one
for its body only.

| Page | State | Screenshot | Reached at |
|---|---|---|---|
| `/msf/campaigns` | None: "No MSF campaigns" | `act-3/3.34-1-campaigns-empty.png` (`states/campaigns-list--empty.png`, old) | Step 3.34 |
| `/msf/campaigns` | A draft and an open campaign | `act-3/3.38-1-list-open-and-draft.png` (`states/campaigns-list--open-draft.png`, old) | Step 3.38 |
| `/msf/campaigns` | The withdraw dialog | `act-3/3.38-2-withdraw-dialog.png` (`states/campaigns-list--withdraw-dialog.png`, old) | Step 3.38 |
| `/msf/campaigns` | Withdrawn | `act-3/3.38-3-withdrawn.png` (`states/campaigns-list--withdrawn.png`, old) | Step 3.38 |
| `/msf/campaigns` | Released | `act-3/3.46-2-list-released.png` (`states/campaigns-list--released.png`, old) | Step 3.46 |
| `/msf/campaigns` | Signed back in (A.4.7) | `act-A/A.4.7-4-back-on-msf-campaigns.png` | Step A.4.7 |
| `/msf/campaigns` | Loading | `states/campaigns-list--loading.png` (old frame) | A held read |
| `/msf/campaigns` | 390 px (scrolls sideways) | `act-A/A.7.5-2-campaigns-390.png` (`states/campaigns-list--narrow.png`, old) | Step A.7.5 |
| `/msf/campaigns/new` | No template yet | `states/campaign-edit--no-template.png` (old frame) | Step 3.34, before the template |
| `/msf/campaigns/new` | Template created | `act-3/3.34-2-template-created.png` (`states/campaign-edit--template-created.png`, old) | Step 3.34 |
| `/msf/campaigns/new` | The form, nobody chosen | `act-3/3.35-0-create-form.png` | Step 3.35 |
| `/msf/campaigns/new` | A trainee chosen, fifteen EPAs | `states/campaign-edit--new.png` (old frame) | Step 3.35, before Create |
| `/msf/campaigns/{CampaignId:int}` | Draft, nobody invited | `act-3/3.35-1-molefe-campaign-draft.png` (`states/campaign-edit--draft-empty.png`, old) | Step 3.35 |
| `/msf/campaigns/{CampaignId:int}` | The remove dialog | `act-3/3.36-1-remove-invitee-dialog.png` (`states/campaign-edit--remove-dialog.png`, old) | Step 3.36 |
| `/msf/campaigns/{CampaignId:int}` | Draft with seven invitees | `act-3/3.36-2-seven-invitees.png` (`states/campaign-edit--draft.png`, old) | Step 3.36 |
| `/msf/campaigns/{CampaignId:int}` | Opened | `act-3/3.37-1-campaign-opened.png` (`states/campaign-edit--opened.png`, old) | Step 3.37 |
| `/msf/campaigns/{CampaignId:int}` | The withdraw dialog, cancelled | `states/campaign-edit--withdraw-dialog.png` (old frame) | After Step 3.37 |
| `/msf/campaigns/{CampaignId:int}` | A link not delivered | `act-3/3.43-1-link-not-delivered.png` (`states/campaign-edit--not-delivered.png`, old) | Step 3.43 |
| `/msf/campaigns/{CampaignId:int}` | Resent | `act-3/3.43-2-link-resent.png` (`states/campaign-edit--resent.png`, old) | Step 3.43 |
| `/msf/campaigns/{CampaignId:int}` | Under review, the addresses gone | `act-3/3.44-2-campaign-closed.png` (`states/campaign-edit--closed.png`, old) | Step 3.44 |
| `/msf/campaigns/{CampaignId:int}` | Released | `act-A/A.7.5-3-campaign-390.png` at 390 (`states/campaign-edit--released.png`, old) | After Step 3.46; Step A.7.5 |
| `/msf/campaigns/{CampaignId:int}` | Withdrawn as a draft | `states/campaign-edit--withdrawn.png` (old frame: "Nobody was invited.") | After Step 3.38 |
| `/msf/campaigns/{CampaignId:int}` | Unavailable | `states/campaign-edit--unavailable.png` (old frame) | Typed `/msf/campaigns/999999` |
| `/msf/campaigns/{CampaignId:int}` | 390 px | `act-A/A.7.5-3-campaign-390.png` (`states/campaign-edit--narrow.png`, old) | Step A.7.5 |
| `/msf/reports/{CampaignId:int}` | Open, a group suppressed | `act-3/3.41-1-report-open.png` (`states/campaign-report--open.png`, old) | Step 3.41 |
| `/msf/reports/{CampaignId:int}` | Under review, ready | `act-3/3.44-1-report-under-review.png` (`states/campaign-report--under-review.png`, old) | Step 3.44 |
| `/msf/reports/{CampaignId:int}` | Released, as stored | `act-3/3.46-1-report-released.png` (`states/campaign-report--released.png`, old) | Step 3.46 |
| `/msf/reports/{CampaignId:int}` | Blocked from release | `states/campaign-report--blocked.png` (old frame) | Scratch (post-appendix): one response |
| `/msf/reports/{CampaignId:int}` | Names no EPA | `states/campaign-report--no-epa.png` (old frame) | Scratch (post-appendix) |
| `/msf/reports/{CampaignId:int}` | Unavailable | `states/campaign-report--unavailable.png` (old frame) | Typed `/msf/reports/999999` |
| `/msf/reports/{CampaignId:int}` | Loading | `states/campaign-report--loading.png` (old frame) | A held read |
| `/msf/reports/{CampaignId:int}` | 390 px | `states/campaign-report--narrow.png` (old frame) | Step 3.46 at 390 |
| `/msf/reports/{CampaignId:int}` | The owner lit, the trail | `states/shell--owner-lit.png` (2026-09-27, flow 01's) | Step 3.41 |
| `/msf/respond` | The questionnaire | `act-3/3.39-1-mokwena-questionnaire.png` (`states/msf-respond--form.png`, old) | Step 3.39 |
| `/msf/respond` | The rating left out (the browser's prompt) | `states/msf-respond--required.png` (old) | Step 3.39, Submit with no rating |
| `/msf/respond` | Thank you | `act-3/3.39-2-mokwena-thank-you.png` (`states/msf-respond--thank-you.png`, old) | Steps 3.39, 3.42 |
| `/msf/respond` | Link already used | `act-3/3.40-1-link-already-used.png` (`states/msf-respond--used.png`, old) | Step 3.40 |
| `/msf/respond` | Request closed | `act-3/3.45-1-request-closed.png` (`states/msf-respond--closed.png`, old) | Step 3.45 |
| `/msf/respond` | Link expired | `states/msf-respond--expired.png` (old) | Scratch (post-appendix) |
| `/msf/respond` | Link not recognised | `states/msf-respond--not-recognised.png` (old) | Typed `?token=not-a-link` |
| `/msf/respond` | Something went wrong | `states/msf-respond--fault.png` (old) | A held read |
| `/msf/respond` | 390 px | `states/msf-respond--narrow.png` (old) | Step 3.39 at 390 |
| `/msf/my-reports` | None released | `states/my-msf-reports--empty.png` (old frame) | Step 3.25 |
| `/msf/my-reports` | The list | `act-3/3.47-0-my-reports-list.png` (`states/my-msf-reports--list.png`, old) | Step 3.47 |
| `/msf/my-reports/{CampaignId:int}` | Her report | `act-3/3.47-1-molefe-report.png` (`states/my-msf-reports--report.png`, old) | Step 3.47 |
| `/msf/my-reports/{CampaignId:int}` | Another trainee's id (the refusal twice: T270) | `states/my-msf-reports--foreign.png` (old frame) | Dr Dlamini, after Step 3.46 |
| `/msf/my-reports` | Loading | `states/my-msf-reports--loading.png` (old frame) | A held read |
| `/msf/my-reports` | 390 px | `states/my-msf-reports--narrow.png` (old frame) | Step 3.47 at 390 |
| `/msf/coverage` | Loaded | `act-3/3.49-1-msf-coverage.png` (`states/programme-coverage--loaded.png`, old) | Step 3.49 |
| `/msf/coverage` | Nobody to show | `states/programme-coverage--empty.png` (old frame) | Step 2.8 |
| `/msf/coverage` | Loading | `states/programme-coverage--loading.png` (old frame) | A held read |
| `/msf/coverage` | 390 px | `act-3/3.49-2-msf-coverage-390.png` (`states/programme-coverage--narrow.png`, old) | Step 3.49 at 390 |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- Anywhere: a load error on any of these pages (none is captured; the create page's ends its circuit, T329); a
  refused add (an address the campaign already invites, T228; a malformed address); no EPA ticked at create; a draft
  whose trainee is no longer current (T284); a campaign under review in the list's row; a campaign closed by the
  auto-close job; the expiry reminder's second link (the first still works, T214).
- Learner feedback, never played: its Quick template wording ("Learner feedback (interim questionnaire)"), one group
  (Learner) with a teaching context per invitee, a report counting "Teaching contexts that responded", no supervision
  level; the respondent's "feedback on the teaching of …".
- "Feedback link revoked": nothing sets a link's revoked time (`states.md` § States no local replay reaches).
- Not built: a confirmation for Close or Release; a report page of its own for the registrar; Try again on any page.

**Belongs to other flows:** flow 01: the shell, the owner lit (`states/shell--owner-lit.png`). Flow 02: the sign-out
page (`act-A/A.4.7-1…`, `-3…`, `-5…`). Flows 03 and 05: My activities and My progress after the release
(`act-3/3.48-*`, `states/my-activities--msf.png`, `states/my-progress--msf.png`, `states/my-progress--msf-error.png`,
`states/epa-progress--msf.png`). Flow 06: the Coordinator's Home (`act-A/A.4.7-2-cancel-back-on-dashboard.png`,
`act-A/A.7.5-1-home-390.png`). Flow 07: the review page's MSF coverage panel and "Starting now leaves out N feedback
campaigns". Flow 13: the graduate refused MSF reports (Step 5.22). Flow 14: an erased respondent's address on an open
campaign (T276). Flow 17: the builder editing `msf_cpsa` (T334). Flow 18: the audit log's "Msf" rows (Step 3.53).
Flow 19: the invitation, the resent link and the expiry reminder (`mail/MsfInvitationEmail*`,
`mail/MsfExpiryReminderEmail.*`). Flow 20: the portfolio PDF's MSF section (T249, T270's colon).

## 4. Attach

Paths are relative to `design/baseline/`. Every key screenshot below was opened and checked on 2026-10-15; none shows a
link, a token or a password (the addresses are the cast's `wombat.local` ones). Four go through `crop-first/` by name
only (How to run, step 1). In every tall capture at 1280 the sidebar's colour stops at 800 px: an artefact of the
capture.

**Key screenshots (attach these first, with § 1):**
1. `act-3/3.36-2-seven-invitees.png`: the draft: counts, the addresses breaking mid-word, "Category", Open enabled
2. `act-3/3.43-1-link-not-delivered.png`: open: Responded by group, the not-delivered warning, Resend 1 link
3. `act-3/3.44-1-report-under-review.png`: the report: summary lines, the actions card, two groups shown, two suppressed
4. `act-3/3.46-1-report-released.png`: released, as stored; "Recorded on the portfolio: … UTC"
5. `act-3/3.47-1-molefe-report.png`: the registrar's list card beside her report; "16:34", ".: 4.00", the cut column
6. `act-3/3.39-1-mokwena-questionnaire.png`: the respondent's page
7. `act-3/3.40-1-link-already-used.png`: a refusal on the respondent's card
8. `act-3/3.35-0-create-form.png`: the create page: Quick template beside Create campaign, the defaults 8/3/2
9. `act-3/3.46-2-list-released.png`: the list: Withdrawn and Released rows, the date wrapping
10. `act-A/A.7.5-2-campaigns-390.png`: the list at 390, scrolling sideways
11. `act-3/3.49-1-msf-coverage.png`: MSF coverage

**The campaign list:** `act-3/3.34-1-campaigns-empty.png`, `act-3/3.38-1-list-open-and-draft.png`,
`act-3/3.38-2-withdraw-dialog.png`, `act-3/3.38-3-withdrawn.png`, `act-A/A.4.7-4-back-on-msf-campaigns.png`,
`states/campaigns-list--loading.png` (old frame).

**The campaign page:** `act-3/3.34-2-template-created.png`, `act-3/3.35-1-molefe-campaign-draft.png`,
`act-3/3.36-1-remove-invitee-dialog.png`, `act-3/3.37-1-campaign-opened.png`, `act-3/3.43-2-link-resent.png`,
`act-3/3.44-2-campaign-closed.png`, `act-A/A.7.5-3-campaign-390.png`, and, for their bodies only (old frame),
`states/campaign-edit--no-template.png`, `states/campaign-edit--new.png`, `states/campaign-edit--withdrawn.png`,
`states/campaign-edit--unavailable.png`.

**The report:** `act-3/3.41-1-report-open.png`, and (old frame) `states/campaign-report--blocked.png`,
`states/campaign-report--no-epa.png`, `states/campaign-report--narrow.png`.

**The respondent's page:** `act-3/3.39-2-mokwena-thank-you.png`, `act-3/3.45-1-request-closed.png`, and (old frame)
`states/msf-respond--required.png`, `states/msf-respond--expired.png`, `states/msf-respond--not-recognised.png`,
`states/msf-respond--fault.png`, `states/msf-respond--narrow.png`.

**The registrar's MSF reports:** `act-3/3.47-0-my-reports-list.png`, and (old frame) `states/my-msf-reports--empty.png`,
`states/my-msf-reports--foreign.png`, `states/my-msf-reports--narrow.png`.

**MSF coverage:** `act-3/3.49-2-msf-coverage-390.png`, `states/programme-coverage--empty.png` (old frame).

That is 42 files.

**Do not attach:** `act-A/A.4.7-2-cancel-back-on-dashboard.png` and `act-A/A.7.5-1-home-390.png` (the Coordinator's Home, flow 06's);
`act-A/A.4.7-1-sign-out-page.png`, `act-A/A.4.7-3-signed-out.png` and `act-A/A.4.7-5-typed-logout.png` (flow 02's);
`act-3/3.48-*` (flows 03 and 05's); the thank-you pages `act-3/3.39-3…`, `-4…` and `act-3/3.42-*` (the same image as `3.39-2`);
every old-frame `states/` capture that a step capture above stands in for.

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `cc80d5b3`.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T270** (P3, queued) | **Closed in the build (T369):** every count has its singular ("from 3 responses", "1 has come back"); a draft's addresses are a list, each whole over its group with Remove… beside it; a campaign that never opened shows Responses "—" and no Responded column; the question once, never ".:" (her page and the PDF); a refused id lands on her list with one alert; a draft whose trainee is not current has Add invitee and Open aria-disabled with their reasons (`SubjectIsCurrent`). "Average: 4.00 from 1 responses." (`M/CampaignReport.razor:161`), and the blocked reason's "1 have come back" (`:296–303`); the draft's address table at 390 (428 px in a 306 px container, Remove off-screen) and its mid-word breaks at 1280 (`col-wrap` on the address, `M/CampaignEdit.razor:270`, `:283`); a withdrawn draft's Responded column (`ShowsResponses` is "not a draft", `:573`); the stray colon (`M/MyMsfReports.razor:75`; the PDF's `src/Wombat.Infrastructure/Reporting/MsfSectionComponent.cs:165`, `:174` are flow 20's); a refused id on MSF reports shown twice and in place of her list (`M/MyMsfReports.razor:18–20`, `:26`, `:127`, `:137`); on a draft whose trainee is no longer current, Add invitee and Open stay enabled and are refused (T284 review: a `SubjectIsCurrent` flag on `MsfCampaignSetupDto`). | `act-3/3.36-2…`, `act-3/3.47-1…`, `states/my-msf-reports--foreign.png`; Steps 3.36, 3.47 |
| **T325** (P3, queued; A9) | **Closed in the build (T369):** every moment "yyyy-MM-dd hh:mm SAST" on both pages; the create page's window and the respondent's last day, and the auto-close job, on the South African calendar. The registrar's list prints `ReleasedOn.ToLocalTime()` unlabelled (`M/MyMsfReports.razor:39`, `:45`); the report prints "… UTC" (`M/CampaignReport.razor:75`); the create page's defaults are `DateTime.Today` (`M/CampaignEdit.razor:1194`, `:1197`); a respondent's link is judged on the UTC date (`MSF/MsfCampaignRules.cs:704`), so it works up to two hours past its South African last day. | `act-3/3.46-1…` against `act-3/3.47-1…`; Steps 3.46, 3.47 |
| **T329** (P2, queued), **T272** (P3, queued) | **Closed in the build (T369), T329's MSF half:** every failed read is fixed words with Try again, the failure to the log; the create page's reads are in a try. T272's foreign key stays backend. The create page awaits its template and trainee reads outside any try (`M/CampaignEdit.razor:515–524`, `:720`, `:739`): a failed read ends the circuit. Every other failed read prints `exception.Message` (`M/CampaignsList.razor:160`, `:234`, `:250`; `M/CampaignReport.razor:271`, `:322`, `:353`; `M/MyMsfReports.razor:137`; `M/ProgrammeCoverage.razor:173`); no page passes `OnRetry`. T272's MSF half (no foreign key on `MsfQuestion.ScaleId`) is backend. | — |
| **T280** (P3, queued) | **Closed in the build (T369):** each row's actions named by its row; every table captioned; h1 → h2 → h3 on each page; `/msf/respond`'s card heading its one h1. `/msf/campaigns` has duplicate accessible names across rows; MSF reports' table has no caption (`M/MyMsfReports.razor:27`); the Coordinator pages go h1 → h3 → h4 (`M/CampaignEdit.razor:50`, `:84`, `:204`; `M/CampaignReport.razor:35`); `/msf/respond` has no `<h1>`: each state is an `<h2>` (`M/MsfRespond.razor:44`, `:52`, `:58`, `:67`; DESIGN.md:373–374 keeps it so), where flow 02 made the auth card's heading its `<h1>`. | `act-3/3.39-1…` |
| **T324** (P3, queued) | **Closed in the build (T369):** every form message in Wombat's words, the field never named by its property. The forms' `DataAnnotations` messages name the property ("The Email field is required.", `M/CampaignEdit.razor:1224–1226`, `:306–308`). | — |
| **T323** (A3; observed, not filed) | **Closed in the build (T369):** the campaign list stacks below 641 px (A.7.5); her list stacks at 900 px and below, with no Responses column (F3); the Closes date never wraps. At 390 the campaign list scrolls sideways in its box with its actions off-screen (`DataTable` with no `Stack`, `M/CampaignsList.razor:48`); at 1280 the registrar's list is cut inside its card ("Respons", `M/MyMsfReports.razor:23–52`), and the list's Closes date wraps mid-date. | `act-A/A.7.5-2…`, `act-3/3.47-1…`, `act-3/3.46-2…` |
| **A5** (BRIEF § 6; observed, not filed) | **Closed in the build (T369):** Close campaign… and Release to trainee… confirm in a ConfirmDialog naming the campaign; each result is an `ActionResult`, focused; a refusal shows beside its button. Close campaign and Release to trainee act on one press with no `ConfirmDialog` (`M/CampaignReport.razor:132`, `:136`; Steps 3.44 and 3.46's Actuals: "no confirmation dialog"), though closing anonymises for good (`MSF/CloseMsfCampaign.cs`) and releasing writes evidence that nothing takes back (`MSF/ReleaseMsfCampaign.cs:124–150`). Their results are page-top alerts in a hand-rolled `.action-result` (`:18–28`), as on the list and the campaign page; `ActionResult` is not used. | `act-3/3.44-1…`, `act-3/3.46-1…` |
| Titles and words (observed, not filed) | **Closed in the build (T369):** "New MSF campaign" is a page of its own (`M/NewCampaign.razor`), each campaign "MSF campaign for {name}"; Trainee, Respondent group and "respondent groups reporting" throughout; the state a badge on both; no Back to campaigns on MSF coverage. The create page and every campaign share the title "MSF campaign" and one subtitle (`M/CampaignEdit.razor:12–14`); the list says "Subject" (`M/CampaignsList.razor:51`) where every other page says "Trainee"; the add form says "Category" (`M/CampaignEdit.razor:315`) and the report "Minimum category responses", "Reporting categories" (`M/CampaignReport.razor:41–42`) where the tables say "Respondent group"; the list prints the state as text (`M/CampaignsList.razor:68`), the campaign page as a badge (`M/CampaignEdit.razor:181`); MSF coverage has a "Back to campaigns" button beside its own trail (`M/ProgrammeCoverage.razor:16`). | `act-3/3.36-2…`, `act-3/3.46-2…`, `act-3/3.49-1…` |
| The registrar's report (observed, not filed) | **Closed in the build (T369):** her report is a page of its own, `/msf/my-reports/{CampaignId:int}` (`M/MyMsfReport.razor`), its h1 "{template}, released {date}"; a release mails her (Q8, `MsfReportReleasedEmail`); her Home is unchanged. No heading of its own ("Selected report", an h3 in a card beside the list, `M/MyMsfReports.razor:54–91`), no focus move on View, no loading state of its own; nothing on her Home or in a mail says a report was released (`MSF/ReleaseMsfCampaign.cs` sends nothing; the trainee Home has no MSF card). Question 4, question 8. | `act-3/3.47-1…` |
| **D54, D55 and the release** (observed, and one inference) | **Closed in the build (T369), Q10 kept as now with a warning:** the campaign page and the report say once her programme has ended that she can no longer open MSF reports, and no release mail goes (F4). Observed: create, add invitee and open ask for a current trainee (`MSF/MsfCampaignRules.cs:108–122`, T284); close, resend, release and withdraw do not, deliberately (`:99–103`: "a close, release or withdraw finishes it"). The release writes `msf_cpsa` through `ActivityService.StageCompletedAsync` (`MSF/ReleaseMsfCampaign.cs:242`), which does not ask D55's `HasCurrentProgrammeAsync` (`src/Wombat.Infrastructure/Activities/ActivityService.cs:91`, `:176`, `:465`: the create and the filing move only). The registrar who has ended loses the Trainee role (D54) and so MSF reports (`M/MyMsfReports.razor:3`; Step 5.22). Inferred, not replayed: a campaign whose registrar graduates or withdraws while it is open can be released onto her ended record, its evidence dated by the close (possibly after her last day), and she cannot open the report. The MSF type credits nothing (D8), so T281's rule changes no count. No runbook step reaches it. Question 10. | — |
| **T153** (P3, queued) | A campaign has no institution of its own: its scope follows the trainee's current profile, so it would move with a trainee who changes institution. Backend; not this flow's unless the operator folds it in. | — |
| **T320** (P2, queued) | **Partly closed in the build (T369):** a release now mails the registrar (Q8, T367's `AccountNotices`); the rest stays flow 19's. Mail is sent only on open and resend (and the reminder job); nothing tells the registrar of a release. Flow 19's, unless question 8 asks for a mail. | — |
| **T334** (P2, queued), **T276** (P3, queued), **T311** (P2, queued) | The builder can edit `msf_cpsa` (flow 17's); an erased person's address stays on another's open campaign (flow 14's); a graduate loses MSF reports with the Trainee role (flow 13's; DESIGN keeps it role-only). Name them; do not design them here. | — |
| Anonymity (T217, T247, T249, T251; DESIGN.md § The MSF campaign page, from :1956) | **Held in the build (T369):** no group's results before the close (F1), a group reports from 3 responses (F2), `Distribution` dropped (Q9); the suppressed line names groups, never counts them. Once a campaign opens, `GetMsfCampaignSetup` lists no address (`MsfCampaignSetupDto.DraftInvitees` on a draft only); the counts are by group; not-delivered links are a number. The registrar's report drops suppressed groups (`M/MyMsfReports.razor:66`) and shows no count. A design that adds a respondent list, a per-invitee mark, or a group's count for the registrar breaks the rule. The aggregate also carries each point's count (`MSF/MsfDtos.cs:41`, `Distribution`), shown on no page: question 9. | Steps 3.37, 3.43, 3.47 |
| The respondent's page (observed) | **Held in the build (T369):** still static, no script; one h1 per state, its Alert under it; "Who sees your feedback" before the rating. Static by design (`M/MsfRespond.razor:1–6`): no circuit, a form post, a rate limit per link, `Referrer-Policy: no-referrer`, not indexed, not stored (`:370–382`); a refused post focuses its summary as the page loads (`:103–110`). Three paragraphs and a note precede the first question (`:68–101`). | `act-3/3.39-1…` |

**Dropped from the old brief (fixed since):** the unlit nav on the report and coverage pages (T331, `b347e11c`: MSF
campaigns lights for the Coordinator, with the trail Home › MSF campaigns › the page); the alert and badge contrast
(T322, flow 01's tokens); "minimum responses (5) and minimum per group (2)" as defaults (they are 8 and 3; the runbook
types 5 and 2). The old brief's DESIGN.md line references (994, 1990, 238–241) are now :1956, :4102 and :655.

## 6. Questions the design must answer

1. **Anonymity, visible:** how the campaign page and the report keep "counted by group, never named" in view, so the
   coordinator does not go looking for names.
2. **The respondent's first view** (3.39): who they are rating and why, or the question first.
3. **The report** (3.41, 3.44, 3.46): where Close and Release sit; their confirmations; how summary, evidence,
   thresholds and groups read together; where a blocked release says why.
4. **The registrar's MSF reports** (3.47): beside the list as today, or a page of its own (a PROPOSED route, as flow
   09's My committee reviews).
5. **The campaign page while open** (3.37, 3.43): the counts, links being sent and links not delivered as one status;
   where Resend sits.
6. **The create page** (3.34, 3.35): the Quick template card beside Create campaign, or a step before it; fifteen EPAs.
7. **(The operator's) MSF coverage and the Administrator's menu:** coverage a header link as today, or a tab of the list
   (a DESIGN.md change: `NavMenuAuthorizationTests` parses the menu table)? The Administrator, admitted to every
   campaign page, has no item: keep?
8. **(The operator's) Telling the registrar of a release:** nothing tells her today. A mail (T320's family, flow 19),
   a Home card, both, or neither?
9. **(The operator's) A rating's spread:** the report shows an average; it also holds each point's count. Does the
   coordinator see it, the registrar, or neither (a small group's spread can say who chose what)?
10. **(The operator's) A campaign that outlives its registrar's programme** (D54, D55): may it still be closed and
    released onto her ended record, which she can no longer open? Keep as now, refuse the release (withdraw only), or
    warn on the campaign page and the report?
11. **(The operator's) T270, T325, T329, T280, T324 and T323's MSF halves:** folded into the build, or any landed first?
    T272's MSF foreign key, T153, T320, T334, T276 and T311 stay their own flows' unless the operator says otherwise.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps are Act 3's Phase 3.G and the appendix; § 9):
  every Expect must hold, the refusals' exact words included. **Rewrite the steps that change:** 3.36 (the group's
  label, the addresses' layout); 3.41, 3.44 and 3.46 ("from 2 responses", the confirmations, the release time in
  SAST); 3.47 (the report's place, the colon, the time); 3.39 (the page's order and its `<h1>`); 3.35 (the create
  page's title, if it changes); A.7.5 (the list at 390); search the whole runbook (BRIEF.md § 9 item 7) for the words
  that change. A new page or route needs a step (`Scenario/ScenarioRunbookTests`). A new step takes a letter when its
  number is taken (§ 11, flow 06).
- **Checks the design adds:** 3.44 and 3.46, a ConfirmDialog naming the campaign, the result focused; 3.47, one
  labelled SAST time on both pages and no stray colon; 3.36, addresses whole at 1280 and stacked at 390 with Remove
  beside each; A.7.5 and 3.47 at 390, nothing scrolls sideways; a failed read on New campaign shows an alert with Try
  again (T329, bUnit); `/msf/respond` with one `<h1>` and still no script; every page checked at 768 as well; every
  table captioned.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include:
  - `MultiSourceFeedback/*` (among them `CampaignReportActionsTests`, `CampaignStatePageTests`,
    `CampaignsListWithdrawTests`, `MsfRespondPageTests`, `MsfPageLabelTests`, `ProgrammeCoveragePageTests`),
    `Hosting/MsfRespondPageHostingTests`, and the integration suite's `MultiSourceFeedback/MsfReportPageFlowTests`, with
    new ones for each confirmation, the plural, the clock and the create page's load error;
  - `Navigation/NavMenuAuthorizationTests`, `ActiveNavItemTests`; `Hosting/PageTitleTests`;
  - `Accessibility/RowNamesTests`, `ActionFocusTests`, `AccessibleNamesTests`, `IdReferencesTests`;
  - `Design/*`, including `NarrowLayoutTests`, `DefinedClassTests`, `ContrastTests`; the scenario guard in `Scenario/`.
- **Re-capture** the states in § 3 (the old-frame ones, and the scratch ones on scratch copies, as `states.md` says)
  and the steps' own captures, keeping their names; `check_baseline_paths.py` at missing 0. Compare them with the
  chosen artboards.
- **Browser check** at 1280, 768 and 390 px as each person in § 1: Mr Smit; an anonymous respondent in a browser signed
  in to nothing, where the page must work as plain HTML; Dr Molefe; Dr Dlamini (another trainee's id).

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as flow 09's replay left it (T366, 2026-10-14; every one of these steps "Gap: none" but 3.47's
known T270 and T325). `D` is the replay day.

```text
Step 3.34 — Mr Smit creates the MSF questionnaire
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/new
Do: Open MSF campaigns and start a new campaign. No questionnaire exists yet, so add the one the page offers as it
  stands: kind Multi-source feedback, name "Default MSF", with its scale question and its comment question.
Expect:
  - **The list:** "MSF campaigns", "Create and review multi-source feedback campaigns.", with New campaign and then MSF
    coverage in its header; "No MSF campaigns" / "Create the first MSF campaign to start respondent outreach."
  - **The create page** is "New MSF campaign" (its tab too), under the trail Home › MSF campaigns › New MSF campaign
    with MSF campaigns lit. Its subtitle says creating it makes a draft and nobody is emailed until it opens. Four
    cards, each an h2: Questionnaire, Trainee and window, Thresholds and Evidence.
  - **No questionnaire yet (T369, C1):** the Template select reads "No template yet", with "Write the first one below;
    it is chosen here once it is added."; the fold "Write a new questionnaire" is open, pre-filled (Multi-source
    feedback, "Default MSF", "Rates the trainee's overall professional performance.", "What should the trainee keep
    doing or improve?"). Create campaign is aria-disabled, with "Create campaign: write a questionnaire first." beside
    it.
  - **Add template:** "**Template created.** Default MSF is chosen as this campaign's template.", focused. The fold
    closes, the select reads "Default MSF (Multi-source feedback)", chosen, with its two questions under it, and Create
    campaign is offered.

Step 3.35 — Mr Smit creates a campaign for Dr Molefe
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/new → /msf/campaigns/{CampaignId:int}
Do: Fill in the campaign:
  - trainee Dr Lerato Molefe, with template Default MSF as it is already chosen;
  - opens on `D` and closes on `D+14`, the defaults;
  - Minimum responses 5; Minimum responses in a respondent group 3 and Minimum respondent groups reporting 2, the
    defaults;
  - evidence for PAED-010 and PAED-012.
  Create it.
Expect:
  - **The form:** the Trainee select offers KGK's five current trainees by name and address, and nobody else. The
    thresholds open on 8 / 3 / 2. "Minimum responses in a respondent group" says a group with fewer is suppressed and
    asks at least 3 for multi-source feedback (T369, F2); the help beside "Minimum respondent groups reporting" says
    that two is the College's answer (D11). The window's help says both days are on the South African calendar (T325).
  - **Evidence:** once a trainee is chosen, "Evidence for these EPAs" lists all 15 EPAs of her curriculum, since MSF is
    on every one (D37), with a count over them: "2 of 15 ticked" once the two are ticked.
  - **Created:** the campaign's own page, "MSF campaign for Lerato Molefe", subtitle "Default MSF · Multi-source
    feedback · `D` to `D+14`". Its status card: Draft, "Add invitees, then open the campaign to send anonymous response
    links.", "Nobody has been invited yet." Open campaign is aria-disabled, with "Open campaign: add at least one
    invitee first. …" beside it (T225), and Withdraw campaign… is offered. About this campaign reads Trainee "Lerato
    Molefe, training year 4", Template "Default MSF (Multi-source feedback)", Thresholds "5 responses; 3 in a respondent
    group; 2 respondent groups reporting" and "Evidence for PAED-010 and PAED-012", each EPA by code and title.

Step 3.36 — Mr Smit invites nine colleagues, and removes one added under the wrong group
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: Add each colleague by address and respondent group, in this order:
  - Sister Grace Mokwena, `grace.mokwena@kgk.wombat.local`, as a Nurse;
  - Sister Palesa Tau, `palesa.tau@kgk.wombat.local`, by mistake as an Allied health professional;
  - the registrars Dr Kagiso Motsepe, `kagiso.motsepe@kgk.wombat.local`, and Dr Lindiwe Khoza,
    `lindiwe.khoza@kgk.wombat.local`, as Peer doctors;
  - Dr Khumalo, `khumalo@kgk.wombat.local`, and Dr Botha, `botha@kgk.wombat.local`, as Consultants;
  - the physiotherapist Ms Naledi Sebego, `naledi.sebego@kgk.wombat.local`, as an Allied health professional;
  - Sister Thandi Nkosi, `nkosi@kgk.wombat.local`, as a Nurse;
  - Dr Johan van Wyk, `vanwyk@kgk.wombat.local`, as a Consultant.
  Then remove Sister Tau's row and add her again as a Nurse.
Expect:
  - **The add form** ("Add an invitee", "* marks a field you must fill in."): Respondent email, and a Respondent group
    select offering Peer doctor, Consultant, Nurse, Allied health professional and Other, in that order, Peer doctor
    chosen; it offers no Patient and no Learner. The group is kept from one add to the next.
  - **Each add** reads "Invitee added." alone (T369, F11), clears the address and puts the focus back in it.
  - **The addresses:** "Addresses invited", listed only while the campaign is a draft. Under a count ("9 addresses"),
    each address whole over its group, with a Remove… beside it named for its address and group ("Remove
    palesa.tau@kgk.wombat.local (Allied health professional)").
  - **Remove** asks "Remove this invitee?", naming the address and group; its buttons are Keep the invitee and Remove
    invitee. Confirmed, the result above the rows, focused, says "palesa.tau@kgk.wombat.local (Allied health
    professional) has been removed from this campaign, and will not be emailed a link when it opens."
  - **In the end** the counts, captioned that once the campaign opens the page never lists who was invited, read Peer
    doctor 2, Consultant 3, Nurse 3, Allied health professional 1 and All groups 9, with no Responded column. "9
    addresses" lists the nine. Open campaign is offered, with "Opening emails each of the 9 invitees a link to respond,
    with `D+14` as their last day." beside it.

Step 3.37 — Mr Smit opens the campaign
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: Open the campaign.
Expect: The page reads "Campaign opened; links are being sent.", focused. The status card reads Open and "0 of 9
  invitees have responded." Its note names `D+14` as the respondents' last day, says the campaign closes by itself once
  that day has passed, and that Close campaign on its report closes it sooner. Under "Invitees by respondent group" the
  invitees are now counted only, with a Responded column reading 0, and no address is listed (T217, T247). Beside it,
  the Links part reads "9 links are still being sent. Reload this page to see whether they were delivered." The
  actions are View report and Withdraw campaign…. The application log holds nine "Feedback request: Lerato Molefe
  (Default MSF, …)" emails, each with a `/msf/respond` link.

Step 3.38 — Mr Smit withdraws a campaign started in error
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/new → /msf/campaigns/{CampaignId:int} → /msf/campaigns
Do: Create a second campaign, for Dr Pieter du Plessis: Default MSF, the defaults, evidence for PAED-015. Then
  Dr Mokoena says his MSF belongs to next semester, so withdraw it from the campaign list.
Expect: The list, "Campaigns (2)", is captioned "MSF campaigns, newest first. Counted by respondent group: this list
  never shows who was invited or who responded." Each row is headed by its trainee, with its links named by its state
  (T225) and by the campaign:
  - Pieter du Plessis first: Default MSF (Multi-source feedback), Draft, Invitations 0, Responses "—" (it never opened,
    T270), closing `D+14`: "Manage" and "Withdraw…";
  - then Lerato Molefe: Open, Invitations 9, Responses 0: "Manage", "View report" and "Withdraw…".
  Withdraw asks "Withdraw this campaign?", names "Pieter du Plessis (Default MSF, closing `D+14`)", and says withdrawing
  cannot be undone; its buttons are Keep the campaign and Withdraw campaign. Confirmed, it reads "The campaign for
  Pieter du Plessis (…) has been withdrawn. Its respondents' links no longer work, and their email addresses have been
  removed.", focused. The row then reads Withdrawn, with only "View campaign".

Step 3.39 — Three respondents answer from their links
Role: Anonymous — Sister Grace Mokwena, Sister Palesa Tau and Dr Kagiso Motsepe (MSF respondents)
Route: /msf/respond
Do: Each respondent opens their own link from the log, in a browser signed in to nothing, and answers:
  - Sister Mokwena: Above expectations, "Calm and kind with parents at night; hands over clearly".
  - Sister Tau: Well above expectations, "Leads neonatal resuscitations and explains her decisions to the nurses".
  - Dr Motsepe: Meets expectations, "A supportive senior; could delegate more".
Expect: The card's one h1 is "Feedback on Lerato Molefe" (T280), its tab the same, over "Default MSF · Last day to
  respond: `D+14`" and the line asking for multi-source feedback on a trainee they have worked with. "Who sees your
  feedback" comes before the rating: their name and address are never shown to her, and she sees the feedback only
  after it has closed and been released, grouped by respondent role. The rating offers five points, from Well below
  expectations to Well above expectations (D47), and is required. The comment is marked "(optional)", with help that
  reads "Up to 4,000 characters." and warns that it may reach her word for word. Each submit ends on the h1 "Thank you"
  and a success Alert, "Your feedback on Lerato Molefe has been recorded."

Step 3.40 — A used link is refused
Role: Anonymous — Dr Kagiso Motsepe (MSF respondent)
Route: /msf/respond
Do: Open the same link again.
Expect: The card's h1 reads "Feedback link already used", with an info Alert saying the link has already been used
  and each link takes one response. No questionnaire is shown.

Step 3.41 — Mr Smit reads the report while the campaign is open
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns → /msf/reports/{CampaignId:int}
Do: Open Dr Molefe's report from the campaign list.
Expect: "MSF report for Lerato Molefe", subtitle "Default MSF · Multi-source feedback · `D` to `D+14`" with "Open its
  campaign page". The status card reads Open, "Still taking responses, until `D+14`.", and "The responses are read once
  the campaign has closed and every address has been removed." (T369, F1). Its checklist reads "3 responses, of the 5
  needed.", "0 respondent groups reporting, of the 2 needed." (no group has 3 yet) and "A respondent group reports once
  3 of it have responded; a group with fewer is suppressed." "Evidence for" lists PAED-010 and PAED-012, and says one
  record per EPA is written when the report is released. No group card and no suppressed line are shown, whatever has
  come back. Close campaign… is offered, with its help; Release is not.

Step 3.42 — Five more respondents answer, and one never does
Role: Anonymous — Dr Fatima Khumalo, Dr Sarah Botha, Ms Naledi Sebego, Sister Thandi Nkosi and Dr Johan van Wyk (MSF respondents)
Route: /msf/respond
Do: The five answer, each from their own link as in Step 3.39; Dr Khoza does not open hers.
  - Dr Khumalo: Above expectations, "Sound judgement in clinic; teaches the interns well".
  - Dr Botha: Above expectations, "Ready to run a ward; should document escalation decisions more fully".
  - Ms Sebego: Above expectations, "Refers early and listens to the therapists".
  - Sister Nkosi: Above expectations, "Escalates early and keeps the nursing notes in step".
  - Dr van Wyk: Above expectations, "Thorough on ward rounds; presents concisely".
Expect: Each submit ends on the h1 "Thank you" and "Your feedback on Lerato Molefe has been recorded." Eight of the
  nine links have now been used; Step 3.43 reads the counts.

Step 3.43 — Mr Smit resends the undelivered link
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: At least an hour after the open, open the campaign and resend.
Expect: The status card reads "8 of 9 invitees have responded." The counts read Invited and Responded: Peer doctor 2
  and 1, Consultant 3 and 3, Nurse 3 and 3, Allied health professional 1 and 1, and All groups 9 and 8. In the Links
  part, a warning reads "1 link was not delivered. Resend sends each of these respondents a new link; this page never
  says who they are.", with the "Resend 1 link" button inside it, and "A link nothing was heard of for an hour counts as
  not delivered." under it. After the resend, the Links part reads "1 new link is being sent.", focused, and "1 link is
  still being sent. Reload this page to see whether it was delivered.", and the warning is gone. The log holds one more
  "Feedback request: Lerato Molefe …" stub email. It names no address; it is Dr Khoza's, the only link not answered.

Step 3.44 — Mr Smit closes the campaign
Role: Coordinator — Mr Pieter Smit
Route: /msf/reports/{CampaignId:int} → /msf/campaigns/{CampaignId:int}
Do: On the report, press Close campaign… and confirm. Then open its campaign page.
Expect:
  - **The dialog** (A5): "Close this campaign now?", "Close the campaign for Lerato Molefe (Default MSF, closing
    `D+14`) before its last day?", then "It takes no more responses: the 1 link not yet used stops working.", "Every
    respondent's email address is removed, so no link can be resent.", "The report keeps the 8 responses it has, for
    you to review and release." and "Closing cannot be undone."; its buttons Keep it open and Close campaign.
  - **Closed:** "Campaign closed and anonymised for review. Every respondent's email address has been removed.",
    focused. The status card reads Under review, "Ready to release: every threshold is met.", with "8 responses, of
    the 5 needed." and "2 respondent groups reporting, of the 2 needed." met.
  - **By respondent group**, "As Lerato Molefe will read it once released." and how ratings are averaged, then the
    groups that reported, in the groups' order: "Consultant · 4.00 average rating, from 3 responses" above "Nurse · 4.33
    average rating, from 3 responses". Each card gives the comment question once, "What should the trainee keep doing
    or improve?", and its three comments.
  - **The suppressed line:** "Suppressed: Peer doctor, Allied health professional. Fewer than 3 responses each, so
    their ratings and comments are not shown here. Lerato Molefe sees nothing of them, not even that they answered."
    Named, never counted.
  - **Release to Lerato Molefe**, after the groups: what a release does (she reads the narrative and the groups shown;
    "2 evidence records are written to Lerato Molefe's portfolio, for PAED-010 and PAED-012."; she is sent an email
    with a link to MSF reports; it cannot be undone), then the Narrative with its counter, "0 of 4,000 characters", and
    the optional "Supervision level this feedback supports", which offers Not stated and the six rungs (D10). Release
    to trainee… is offered.
  - **The campaign page:** Under review, "Closed to responses, and every respondent's email address has been
    removed.", the counts by group with their Responded column (9 and 8), and About's "Closed to responses: on `D`". It
    offers "Review and release" and Withdraw campaign….

Step 3.45 — A link opened after the close is refused
Role: Anonymous — Dr Lindiwe Khoza (MSF respondent)
Route: /msf/respond
Do: Open the resent link.
Expect: The card's h1 reads "Feedback request closed", with a warning Alert saying the request has closed and is no
  longer accepting responses. No questionnaire is shown.

Step 3.46 — Mr Smit releases the report to Dr Molefe
Role: Coordinator — Mr Pieter Smit
Route: /msf/reports/{CampaignId:int} → /msf/campaigns
Do: Write the narrative "Consistently strong feedback from nursing and consultant colleagues; keep documenting
  escalation decisions". Leave the supervision level Not stated, since he is not a clinician. Press Release to
  trainee… and confirm.
Expect:
  - **The counter** reads "106 of 4,000 characters" once the narrative is typed.
  - **The dialog** (A5): "Release this report to Lerato Molefe?": she can read it on MSF reports, the narrative and the
    ratings and comments of the Consultant and Nurse groups, nothing of the other groups; "2 evidence records are
    written to Lerato Molefe's portfolio: PAED-010 and PAED-012."; "Supervision level: Not stated."; "Lerato Molefe is
    sent an email saying a report has been released, with a link to MSF reports. The email holds none of the
    feedback."; and that releasing cannot be undone. Its buttons are Not yet and Release to Lerato Molefe.
  - **Released:** "Report released to Lerato Molefe.", focused. The status card reads Released, "The report has been
    released to the trainee.", and one line: "`D` hh:mm SAST · 8 responses, with 5 needed · 2 respondent groups
    reporting, with 2 needed" (T325). "Evidence for" lists PAED-010 and PAED-012, "Recorded on the portfolio: `D` hh:mm
    SAST". The groups read "As Lerato Molefe reads it.", the Consultant card first, and the suppressed line without its
    last sentence. "As released" shows the narrative and Supervision level "Not stated" as stored, with no form (T246).
  - **The mail:** the log holds one more stub email, "MSF report released: Default MSF": "Your multi-source feedback
    report, Default MSF, was released to you on `D` by Pieter Smit.", that it holds none of the feedback, and the link
    to `/msf/my-reports` (T369, Q8). The stub line prints no recipient (T282); it is Dr Molefe's.
  - **The list:** the row reads Released, Invitations 9, Responses 8, with "View campaign" and "View report" and no
    Withdraw.

Step 3.47 — Dr Molefe reads her released report
Role: Trainee — Dr Lerato Molefe
Route: /msf/my-reports → /msf/my-reports/{CampaignId:int}
Do: Open MSF reports, then view the report.
Expect:
  - **MSF reports:** "Released aggregate feedback only. Individual responses are never shown."; "Your released
    reports", a table captioned "Your released MSF reports, newest first", with the columns Report and Released and no
    Responses (T369, F3). One row: Default MSF over Multi-source feedback, Released "`D` hh:mm SAST", the same moment the
    coordinator's report gives (T325), and View, named "View the Default MSF report, released `D`".
  - **Her report** is a page of its own at `/msf/my-reports/{CampaignId:int}`: h1 and tab "Default MSF, released `D`",
    subtitle "Multi-source feedback · Released `D` hh:mm SAST", under the trail Home › MSF reports › Default MSF,
    released `D`, with MSF reports lit.
  - **Its sections:** Narrative, the coordinator's words. "What your colleagues said": "Grouped by respondent role; no
    one is named.", how ratings are averaged, and that comments are word for word; then two groups only, "Consultant ·
    4.00 average rating" first and "Nurse · 4.33 average rating", each with the comment question once and its three
    comments, and no stray ".:" (T270). "On your portfolio": "Recorded as evidence on `D` hh:mm SAST:", with PAED-010
    and PAED-012, each a link to its page under My progress.
  - **Nothing of the others:** nothing of the peer doctors or the physiotherapist, not even that they answered, and no
    group's count (T249).

Step 3.48 — Dr Molefe's record and progress show her evidence and the feedback
Role: Trainee — Dr Lerato Molefe
Route: /activities/mine → /portfolio/progress → /portfolio/progress/{EpaId:int}
Do: Open My activities, then My progress, then PAED-001, PAED-010 and PAED-012 from its index.
Expect: My activities holds her six Completed WBAs and two Multi-Source Feedback (Paediatrics) rows. The MSF rows are
  PAED-010 and PAED-012, encounter date `D` (the day the campaign closed), Done, Recorded, credit "—" as text (D8). Each
  WBA's Credit, "1 item", is a link to its EPA's page (T355). My progress reads:
  - "1 of 10" over "EPAs met this semester", and training year 4;
  - "Multi-source feedback: 2 of 15 EPAs covered by a released campaign that closed this semester. MSF is tracked on its
    own and counts towards no target.";
  - PAED-001's row: "3 of 3 this semester", "Target met for Semester 2, 2026.";
  - PAED-012's row "2 of 3 this semester" and PAED-010's "1 of 3 this semester", no row carrying an MSF line.
  PAED-001's page reads "At the minimum level when observed: 2 of 3" and "Training year 4: level 5, the minimum each
  encounter is judged against and your STAR's target."; its trajectory reads "3 ratings in the 2026 academic year, from
  Thandi Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below." The pages of PAED-010 and PAED-012 each read
  "MSF in Semester 2, 2026: covered by a released campaign that closed on `D`." first among their MSF lines, chart their
  ratings, and say under the chart "Multi-source feedback is not plotted.": no chart plots an MSF point (D36).

Step 3.49 — Mr Smit reads the programme's MSF coverage
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns → /msf/coverage
Do: Open MSF coverage from the campaign list.
Expect: "MSF coverage", under the trail Home › MSF campaigns › MSF coverage, with no Back to campaigns (T369); its
  subtitle ends "Coverage is not a target." One programme: "Paediatric EPA Curriculum 11.1 at Kgosi Kgari Teaching
  Hospital".
  - Under it, a line for each semester: "Semester 1, 2026: 5 trainees, whose programme had started by 30 June 2026"
    and "Semester 2, 2026: 5 trainees, whose programme had started by 31 December 2026".
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
  "You have signed out." MSF campaigns then asks him to sign in, and after signing in he is brought back to it, the
  list as Step 3.46 left it (Pieter du Plessis's Withdrawn, Dr Molefe's Released). The audit log records a Logout.

Step A.7.5 — Mr Smit on his phone
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions due and the data-rights queue. On Decisions
  due, set Status to "Every status", press Show, and open By EPA.
Expect: Home's header action, Start an MSF campaign, is a row of its own below the header's rule, 44px tall and the page's
  width, and the cards stack in one column, each row's link a 44px block (T358, R2-Home k6). MSF campaigns stacks its rows
  (T323): Pieter du Plessis's first, then Dr Molefe's, each a block with every field shown under its column's name, each
  action a 44px block; nothing scrolls sideways. The campaign page keeps its gutter (T226), and its invitee table (counts by
  respondent group: the page never lists an address) fits its card. Decisions due's filters stack, with Show and Clear
  filters each 44px tall; By EPA's fold summary is 44px, and opened it reads one block per EPA; each decision is a block, its
  one action a 44px button the block's width; the pager's buttons are 44px (T364). The queue's filters stack above its table.
```
