# Flow 10 round 1: assessment (T369 step 5)

Read-only, at HEAD `ded43219`. Canvas "Wombat 10" (https://claude.ai/artifact/CPUXS577gX2yo79AryKmVE), version
1792060099-c3ec. Boards: `design/flows/10-msf-campaign/round-1/project/*.dc.html` (27, every board's text read) and
`canvas.json`. `W/` = `src/Wombat.Web/Components/Pages/`, `M/` = `W/MultiSourceFeedback/`, `A/` =
`src/Wombat.Application/Features/`, `MSF/` = `A/MultiSourceFeedback/`, `D/` = `src/Wombat.Domain/`. **Obs** = observed in code, the runbook or the
boards; **Inf** = inferred. Lines cited "(brief)" are the restated brief's own citations at `cc80d5b3`, unchanged since.

## 1. Summary

- **The proposal (P) holds, with corrections.** These are its parts:
  - **The list:** badges for the state, "Trainee" for "Subject", stacked rows at 390.
  - **The create page:** "New MSF campaign", the questionnaire first, with "Write a new questionnaire" folded, and the
    thresholds relabelled by respondent group.
  - **The campaign page:** "MSF campaign for Lerato Molefe". One status card holds the badge, the state's sentence,
    the counts, the links and the actions. The draft's addresses are rows, with Remove beside each.
  - **`/msf/respond`:** an `<h1>`, then one lead line and two privacy bullets before the question.
  - **Coverage:** no "Back to campaigns" button.
  - **Dialogs:** Withdraw and Remove renamed on their safe side; Close and Release new, each marked PROPOSED.

  The corrections are mostly data the DTOs do not carry yet, and one wrong count. Section 3 lists them.
- **The report: pick RA** (status first, release last), one column at every width. Borrow:
  - RB's group header (name and average on one line);
  - RC's one-line summary for the Released state.

  Do not take RB's side column (flow 09 rejected it, C5) or RC's table of groups. RC splits the comments from their
  group's average, adds an Invited column the report does not read, and carries the spread (Q9).
- **The registrar: pick MA**, her report on a page of its own at the route that exists today
  (`/msf/my-reports/{CampaignId:int}`), so no new route. Borrow MB-390's handling of another trainee's id: one alert
  above her own list. Change its words to "not found" (§ 5).
- **Anonymity holds on every board** (Obs):
  - no address once a campaign opens;
  - counts by group only;
  - the registrar sees no group's count and nothing of a suppressed group;
  - the release dialog says so.

  The one pressure point is RC's spread (Q9).
- **The canvas left out what the ask required** (Obs):
  - no reasoning for any variation;
  - no step counts;
  - no list of components with NEW marked;
  - no DESIGN.md rule named;
  - PROPOSED marked only on the two new dialogs and RC's spread.

  Round 2 must carry a spec board and a steps board, as flow 09's round 2 did (`R2-0-spec`, `R2-0-steps`).
- **The attachments arrived** (§ 6). No board says otherwise, and the boards draw names and titles found only in the
  captures. One count is wrong (P1, Responded 3 at 3.38), and ten EPA titles are placeholders.

## 2. What was drawn

27 boards in 11 groups (`canvas.json` notes t1–t11). Every board uses the copied mark
(`/_blob/d5b5890c1eb0efaad13d0d347d1ebf4c`, the canvas's only asset) and the design system's `tokens.css` and
`bundle.css`.

| Group | Boards | State drawn |
|---|---|---|
| P · Screen 1, the list | P1-List (1280), P1-List-390 | After Withdraw (3.38), the result alert above the list; Released and Withdrawn rows (3.46) at 390 |
| P · Screen 2, create | P2-Create (1280), P2-Create-390 | "Template created.", Molefe chosen, 5/2/2, PAED-010 and PAED-012 ticked (3.34–3.35); at 390 no template yet, the fold open, Create disabled with its reason |
| P · Screen 2, the campaign | P2-Draft, P2-Open, P2-Review (1280), P2-Open-390 | Draft with seven invitees, the add form's result (3.36); Open with a link not delivered (3.43); Under review (3.44); at 390 after the resend |
| Confirmations | P-Dialogs | Withdraw (du Plessis), Remove (Tau), Close campaign (PROPOSED), Release to Lerato Molefe (PROPOSED), each with where the focus starts and goes |
| P · Screen 4, respond | P4-Respond (1280, Above chosen), P4-Respond-390, P4-Thanks-390, P4-Used-390 | 3.39, 3.40 |
| P · Screen 6, coverage | P6-Coverage (1280), P6-Coverage-390 | 3.49 |
| Screen 3, RA | RA (1280, under review, just closed), RA-390 (released), RA-Open (1280, open, Close) | 3.41, 3.44, 3.46 |
| Screen 3, RB | RB (1280), RB-390 | Under review, just closed (3.44) |
| Screen 3, RC | RC (1280, released), RC-390 (under review) | 3.44, 3.46 |
| Screen 5, MA | MA-List, MA-Report (1280), MA-Report-390 | 3.47 |
| Screen 5, MB | MB (1280), MB-390 | 3.47; Dlamini on Molefe's id |

**Not drawn in round 1** (the ask asked for P's states at 3.34–3.38 and 3.43–3.44 only, so these are round 2's):
- loading and load errors;
- unavailable;
- the list empty and under review;
- the campaign released and withdrawn;
- no EPA ticked, a refused add, a draft whose trainee is no longer current;
- the report blocked from release, or naming no EPA;
- `/msf/respond` closed, expired, not recognised, the fault and the browser's prompt;
- coverage empty;
- the registrar's empty list;
- learner feedback;
- the heavy volumes;
- 768.

## 3. The proposal against the code

### Screen 1, the campaign list (P1)

- **Confirm:**
  - "Trainee" for "Subject" (`M/CampaignsList.razor:51`, brief);
  - the state as `BadgeFor.MsfCampaign`'s badge, with tints that match
    (`W/../Shared/BadgeFor.cs:117-124`: Open submitted, Under review accepted, Released completed, Withdrawn
    declined);
  - row actions named for their row, as today;
  - the result above the table, then the table;
  - the caption saying counts only;
  - `clinic-table--stack-wide` (in app.css) for T323.
- **Wrong count (Obs):** at 3.38 Dr Molefe's row reads "Responded 3". Nobody answers until 3.39, so it is 0.
- **Data the list does not carry:**
  - "Training year 4" under the name: `MsfCampaignSummaryDto` has no training year (`MSF/MsfDtos.cs:17-37`). It is a
    small mapper change, or drop it.
  - At 390 the Released row reads "Released 2026-10-14 16:34 SAST" in State and "Closed 2026-10-14" under a cell
    labelled "Closed". `ReleasedOn` is in the DTO; `ClosedOn` is not (`D/MultiSourceFeedback/MsfCampaign.cs:56` has
    it).
  - A column headed "Closes" at 1280 whose cell reads "Closed" on some rows is two facts in one column. Recommend
    "Closes" with `ClosesOn` on every row, and the release time on the report, not the list.
- **The caption names one institution**, "Every campaign for a trainee at Kgosi Kgari Teaching Hospital". The
  Administrator is admitted and sees every institution's campaigns. Use "Every campaign you run…", or name the
  institution only for a Coordinator.
- **Header order differs:** at 1280, MSF coverage then New campaign; at 390 the other way round. Pick one (New
  campaign first, as flow 06's Home puts its action).
- **"Responded: — never opened"** on the withdrawn draft is fine: it answers T270's "no Responded on a campaign that
  never opened" for the list.

### Screen 2, creating (P2-Create)

- **Confirm (Q6):**
  - its own title, "New MSF campaign", trail Home › MSF campaigns › New MSF campaign;
  - the Questionnaire card first: the Template select, with the questions as its help (`ListMsfTemplates` returns
    `Questions`, `MSF/ListMsfTemplates.cs:31`);
  - "Write a new questionnaire" as a disclosure, open when there is none;
  - then Trainee and window, Thresholds, and the EPAs as a fieldset with a live "2 of 15 ticked".

  Each tick is already a round trip, so the count costs nothing.
- **The one-label rule:** "Minimum responses in a respondent group" and "Minimum respondent groups reporting" settle
  "Category" (§ 5 Titles row). The runbook's 3.35 Expect changes with them.
- **"Template created. Default MSF is chosen as this campaign's template."** is a behaviour change. `CreateTemplateAsync`
  reloads the list and sets only the status (`M/CampaignEdit.razor:830-832`). Mark it NEW: select the new template.
- **Create disabled until a questionnaire exists** ("Create campaign: write a questionnaire first.",
  `aria-disabled`) is NEW. It is flow 01's disabled-with-reason pattern, as Open campaign uses. Fine.
- **Ten EPA titles are placeholders**, "[title from the curriculum]". The four drawn are right. Use the seed's
  (`paediatric-epa-v11.1.json`):
  - 004 Managing common neonatal conditions
  - 005 Providing neonatal care in intensive and high-care settings
  - 006 Managing long-term health conditions (LTHCs)
  - 007 Maintaining and promoting the health and well-being of children
  - 009 Providing care for an adolescent patient
  - 011 Managing population health challenges
  - 013 Managing child protection, legal and ethical aspects of children at risk
  - 014 Providing palliative and end-of-life care

  The same holds on P6.
- **Asterisks:** Opens on and Closes on carry them at 1280 but not at 390. Pick one.
- **Help text:** "The respondents' last day to answer, on the South African calendar" is right only once T325's
  respondent half lands (`MSF/MsfCampaignRules.cs:704`, brief). Fold it in (Q11).
- **Defaults:** the default dates must come from the SA calendar, not `DateTime.Today` (`M/CampaignEdit.razor:1194`,
  `:1197`, brief).

### Screen 2, a campaign (P2-Draft, P2-Open, P2-Review, P2-Open-390)

- **Confirm (Q5):**
  - its own h1, "MSF campaign for Lerato Molefe", with the template, kind and window as the subtitle;
  - one status card: badge, state sentence, counts, a "Links" part with the not-delivered warning and Resend in the
    warning's row (`alert-row` exists), and the actions;
  - the facts in "About this campaign".

  The draft's addresses become rows that wrap after "@" (`<wbr>`), with Remove beside each and named for its address
  and group (T270, T280). The add form's group reads "Respondent group" (T270). "Opening emails each of the 7
  invitees…" states the consequence.
- **Data the setup DTO does not carry** (`MSF/GetMsfCampaignSetup.cs:29-130`): `MsfCampaignSetupDto` has the name,
  template, kind, state, window, counts, the draft's addresses and the two link numbers. It lacks:
  - the thresholds ("5 responses, 2 in a group, 2 groups reporting");
  - the EPAs ("Evidence for PAED-010, PAED-012");
  - the training year ("Lerato Molefe, training year 4");
  - the closing date ("Closed to responses on 2026-10-14": `ClosedOn` is a UTC `DateTime`; show the SA date).

  These are mapper additions. Mark them as data changes.
- **"6 of 7 invitees have responded."** is a total the counts table already shows. It names nobody. Fine.
- **The 1280 open board uses `detail-card--warning`** for the whole status card, and the 390 board uses
  `detail-card--emphasis`. One undelivered link should not tint the whole card. Keep `--emphasis`, and let the alert
  carry the warning.
- **Under review (P2-Review)** keeps the counts with Responded. Step A.7.5 expects the released campaign's count table
  to fit its card. Good. The trigger for "Review and release" is primary, and Withdraw is an outline button. Good.
- **One h1 per campaign:** two campaigns for one trainee in two semesters share an h1 (**Inf**, the collision flow 09's
  C6 found). The subtitle's window tells them apart. Accept it, or add the window to the h1. Ask in round 2.
- **Not drawn:**
  - the released and withdrawn pages (a withdrawn draft has no Responded column: T270, `M/CampaignEdit.razor:573`);
  - "7 links are still being sent" (3.37);
  - the draft whose trainee is no longer current (T270's `SubjectIsCurrent`);
  - a refused add (T228's duplicate, a malformed address) beside the field, keeping the typed address;
  - learner feedback's teaching context per invitee.

### Confirmations (P-Dialogs)

- **Withdraw and Remove:** the built wording, with the safe button renamed "Keep the campaign" and "Keep the invitee".
  `ConfirmDialog` takes `CancelLabel` (`W/../Shared/ConfirmDialog.razor:109`), so renaming them costs nothing.
  Confirm. Draw Withdraw's under-review wording ("It has closed, and its report has not been released. …") too.
- **Close campaign (PROPOSED):** confirm the shape: three consequences and "Closing cannot be undone". Its numbers need
  data the report does not read:
  - "the 1 link not yet used": `MsfCampaignAggregateReportDto` has `TotalResponses` but no invited count
    (`MSF/MsfDtos.cs:117-136`);
  - the h1's trainee name and the closing date: the report DTO carries `SubjectUserId` only and no window.

  Add `SubjectName`, `OpensOn`, `ClosesOn` and the invited total to the report DTO, or read `GetMsfCampaignSetup`
  beside it. Say which in round 2.
- **Release to trainee (PROPOSED):** confirm:
  - it names the groups shown;
  - it says nothing of the others is shown, "not even that they answered";
  - it says how many evidence records are written, and for which EPAs;
  - it states the level;
  - it says that narrative and level are final.

  Add three lines:
  - a line for an EPA no longer on her curriculum ("PAED-0nn: not recorded, no longer on her curriculum");
  - for a campaign naming no EPA, "No evidence is recorded";
  - Q8's sentence, if the operator sends a mail.
- **Focus:** each dialog opens on its safe button, and the result takes the focus. This matches flow 09's
  `ConfirmDialog` opening on Cancel and returning focus to the opener. Confirm.

### Screen 4, `/msf/respond` (P4)

- **Confirm (Q2):**
  - the `<h1>` (T280; flow 02's auth card);
  - the meta line;
  - one lead sentence;
  - the two privacy statements as a list labelled "Who sees your feedback";
  - "Questions marked * must be answered.";
  - the rating as one fieldset whose legend is the question, with "Compare … at their stage of training" as its help
    (it is moved, not lost);
  - the comment's warning above its box;
  - "Submit feedback".

  Thank you and Link already used are each an h1, a sentence and "You can close this page." The words are the built
  ones.
- **Static page:** the `.is-chosen` tint on the chosen radio must be CSS only (`:has(:checked)`). The page has no
  script and no live state (`M/MsfRespond.razor:1-6`, brief). Say so in round 2.
- **P4-Thanks's `h1 tabindex="-1"`** is harmless on a static page, but nothing moves focus there. Drop it.
- **Not drawn:** closed, expired, not recognised, the fault, the browser's required prompt, and learner feedback's
  "feedback on the teaching of …".

### Screen 6, MSF coverage (P6)

- **Confirm:**
  - "Back to campaigns" gone, so the trail does the work (brief § 5 Titles row);
  - the programme heading and semester lines as built;
  - both grids each in a captioned, focusable `role="region"` that scrolls within itself at 390 (T323's exception);
  - "1 of 5 trainees covered" in bold only;
  - no badge, tint or bar (D8, D9);
  - "Coverage is not a target." added to the subtitle.
- **The trainees and their order are right:** the five KGK trainees, ordered by name
  (`MSF/GetMsfProgrammeCoverage.cs:196`).
- **The placeholders:** the same eight EPA titles as P2-Create.
- **Not drawn:** "No trainees to show", loading, an Administrator's view with more than one programme.

## 4. The report: RA, RB, RC

All three share the same data, words and dialogs. Each puts the trainee's name and window in the h1 and subtitle, with
"Open its campaign page", which needs the data in § 3's Close item. The release time is in SAST (T325), and the plural
is "from 2 responses". None draws "from 1 response" (T270). The narrative counter "106 of 4,000 characters" is right:
the narrative is 106 characters (Obs). The steps are equal across the three: Close is 2 (button, confirm), Release is 3
(type the narrative, Release, confirm).

- **RA, status first, release last.** One column. The status card holds:
  - the badge;
  - a sentence heading ("Ready to release: every threshold is met." / "Still taking responses, until 2026-10-28.");
  - a checklist ("6 responses, of the 5 needed."; "2 respondent groups reporting, of the 2 needed."; the group rule);
  - Evidence for;
  - Close while Open, with "Release is offered once the campaign has closed."

  Then come the groups as cards, the suppressed groups named in one compact card, and the release form last, with
  "When you release:" in three lines and a link to withdraw on the campaign page. Released: "As released" (narrative,
  level) and "Recorded on the portfolio: 2026-10-14 16:34 SAST". It reads in the order the decision is made.
- **RB, the decision beside the evidence.** At 1280 the groups take the left and a 22 rem column holds status and
  release. It stacks at 390. The side column is a new detail-page pattern, and flow 09 rejected one for its appeal form
  (round-1 assessment C5). A 4,000-character narrative in 22 rem is cramped.
- **RC, one table of groups.** It puts these in one table:
  - Invited and Responded per group (the report DTO has no invited counts: a data change);
  - the average;
  - "How the ratings fell" (PROPOSED, Q9);
  - Shown or Suppressed.

  The comments sit in a separate section, away from their group's figure. Released collapses to one summary line.

**Recommend RA.** It keeps one column (flow 09's rule), puts the thresholds as a checklist where a blocked release
explains itself (Q3), and puts the release form after everything it releases. Borrow:
- **RB's group header:** the name and its average on one line (`group-head`), which shortens each card. Keep RA's
  sentence for the suppressed groups over RB's per-group badge rows: "Suppressed" as a badge reads as a state.
- **RC's summary line** for the Released state's status card ("Released · 2026-10-14 16:34 SAST · 6 responses, with 5
  needed · 2 respondent groups reporting, with 2 needed"), in place of RA-390's repeated checklist.

**Not for round 2:** RC's Invited column (it repeats the campaign page) and the spread, unless the operator says yes to
Q9.

**RA's faults round 2 fixes:**
- **The blocked state is not drawn.** Draw it: an `is-short` line and the disabled Release with its reason beside it,
  in both of today's wordings: "Release needs 5 responses; 1 has come back. …" and "Release needs 2 respondent groups
  to report; 1 cleared the 2-response threshold. …" (T270's singular).
- **The released time** is `EvidenceRecordedOn`, which is null when the campaign names no EPA. "Released to Lerato
  Molefe on …" needs `ReleasedOn` on the report DTO (it is on the summary DTO only).
- **RA-Open's suppressed card** says "its rating and comments are not shown", which is right before release. Keep the
  coordinator's sentence and the registrar's sentence apart.

## 5. The registrar's MSF reports: MA, MB

- **MA, a page of its own.**
  - **The list:** a captioned table (Report, Released in SAST, Responses, View named for its row).
  - **Her report:** at `/msf/my-reports/{CampaignId:int}`, with the trail Home › MSF reports › "Default MSF, released
    2026-10-14". It shows the narrative first, then "What your colleagues said": each shown group with its average and
    comments, nothing else. Last, "On your portfolio" links PAED-010 and PAED-012 (`MsfCoveredEpaDto.EpaId` exists)
    with "Recorded as evidence on … SAST".
  - **The route exists** (`M/MyMsfReports.razor`, brief § Pages). This changes what it draws, not a new route. The
    nav owner (`Nav/NavOwners.cs`) gains a trail for it: MSF reports lights itself today with no trail.
- **MB, beside the list, mended.** The list becomes a column of row links, with the chosen one marked, beside the report
  card in `details-grid`. That cures the "Respons" cut. The report gets an h2 of its own, and the refusal is shown once.
- **Recommend MA:**
  - it answers Q4 as flow 09 answered My committee reviews (a page per item);
  - it gives the report its own h1 and title, where today it has none (§ 5);
  - its 390 needs no list above the report.

  The steps are equal: MSF reports, then View (2). **Borrow from MB-390:** for another trainee's id, her own list with
  one alert above it, focused (T270: "said once").
- **MB-390's words fail the content rule.** "That report is not one of yours." confirms the report exists, and an
  out-of-scope record is "not found", never "forbidden" (§ 1 Constraints). Use "That report could not be found. Your
  released reports are listed below." For Dr Dlamini, "No released reports" below it is right: she has none.
- **Check for round 2:**
  - MA's 390 evidence links are 44 px blocks: good.
  - The report's "average rating" shows two decimals and no count: right (T249).
  - Draw an EPA whose evidence was not recorded (`Recorded` false): leave it out of "On your portfolio", or mark it "not
    recorded".
  - The release time comes from the list's `ReleasedOn`, or from the report DTO if § 4's change is made.
  - Draw a load error with Try again (`M/MyMsfReports.razor:137` prints the exception today).

## 6. Attachments and invented data

- **No board says the attachments were missing.** A grep of every `.dc.html` for attach, screenshot, receiv, capture
  and upload finds nothing (Obs).
- **The boards draw what only the captures hold** (Obs that it is not in the ask; Inf that it came from the captures):
  - PAED-001, -002, -003 and -008's titles, all exactly as seeded;
  - Mahlangu and Ndlovu in coverage's By trainee table.

  None of these is in `round-1-ask.txt`. The attachments arrived. No re-attach is needed, though naming the folder
  again in round 2 does no harm.
- **Invented or wrong:**
  - P1 Responded 3 at 3.38 (it is 0);
  - ten EPA titles left as "[title]" placeholders (better than invented, but round 2 must fill them);
  - "Training year 4/2" and "Closed on" (true values, but no DTO carries them: § 3).
- **Checked and right:**
  - every cast name, address and group;
  - the counts 2/2/2/1 and 7, and 7/6 at 3.43;
  - every comment, word for word;
  - Nurse 4.50 and Consultant 4.00 (Mokwena 4 and Tau 5; Khumalo 4 and Botha 4);
  - RC's spread (Nurse one Well above and one Above, Consultant two Above);
  - Peer doctor 1 of 2 and Allied health professional 1 of 1;
  - the narrative;
  - D = 2026-10-14 and D+14 = 2026-10-28;
  - "16:34 SAST" (14:34 UTC);
  - the five KGK trainees and their years (`README.md` § Cast).
- **Line endings:** the saved boards are LF, as flow 09's are.

## 7. Recommended answers to § 6

1. **Anonymity, visible.** One caption on every count table, built: "Counted by respondent group. This page never lists
   who was invited or which of them responded." Add the list's caption line. Nothing else: no per-row marks, and no
   repeated banner. The release dialog's "nothing of the other groups is shown, not even that they answered" carries
   it to the decision.
2. **The respondent's first view.** Whom and why first, short, as P4 draws it: the h1 with the trainee's name, the
   last day, one lead line and two privacy bullets, then the question. The "compare at their stage" note becomes the
   fieldset's help.
3. **The report.** RA, with the corrections in § 4:
   - Close sits in the status card while the campaign is open;
   - Release is the last section, an outline button opening the PROPOSED ConfirmDialog;
   - a blocked release says why in the checklist and beside the disabled button;
   - the results take the focus at the top.
4. **The registrar's reports.** MA: a page of its own at the existing `/msf/my-reports/{CampaignId:int}`, with MB-390's
   single "not found" alert over her list for another trainee's id.
5. **The campaign page while open.** P2-Open's one status card: "6 of 7 invitees have responded.", the counts table, a
   Links part with the not-delivered warning and "Resend 1 link" in its row, and "N links are still being sent" as
   info. The resend result is in the Links part, focused.
6. **The create page.** The questionnaire first, as a select with "Write a new questionnaire" folded (open when none);
   the fifteen EPAs as a fieldset with the live count; Create disabled with its reason until a template exists.
7. **(The operator's) Coverage and the Administrator.** Recommend keeping coverage as the list header's outline link:
   a tab is a new pattern, and changes DESIGN.md's menu table and `NavMenuAuthorizationTests`. Recommend no menu item
   for the Administrator: flow 09 gave My authorisations none, and an Administrator does not run campaigns day to day.
   - *Alternatives:* a tab of the list; an "MSF campaigns" item in the Administrator's menu.
8. **(The operator's) Telling the registrar.** Recommend a mail on release through T367's one-off notice path
   (`A/../Common/Email/AccountNotices.cs`). It holds no feedback, only "Your MSF report from … has been released", with
   a link to MSF reports. The release dialog says it will be sent, and the result does not claim delivery. No Home card
   for now: the trainee Home is flow 05's.
   - *Alternatives:* a Home card (flow 05's page), both, or neither (leave it to T320 and flow 19).
9. **(The operator's) A rating's spread.** Recommend neither reader sees it. For a group of two it adds nothing to the
   average, and for a larger group it tells a coordinator who knows the invitees more about who chose what. The
   registrar must never see it (T249).
   - *Alternatives:* the coordinator only, for groups of five or more; the coordinator always (RC).
10. **(The operator's) A campaign that outlives the programme.** Recommend warn, and keep the release.
    - The feedback was given while she trained, and the evidence belongs on her record, which stays readable
      (`trainee_record`, T252). It credits nothing (D8).
    - The campaign page and the report say "Lerato Molefe's programme ended on ‹date›: a release records evidence on
      her closed record, and she cannot read it on MSF reports (T311)."
    - The release dialog repeats it.
    - This needs a `SubjectIsCurrent` flag, T270's own (T284 review), on both DTOs.
    - *Alternatives:* refuse the release and allow withdraw only (a domain rule in `MSF/MsfCampaignRules.cs:99-103`,
      brief); keep it silent, as now.
11. **(The operator's) The tasks.** Recommend folding into the build T270, T325 and the MSF halves of T329, T280, T324
    and T323, with:
    - T325's respondent half (the link judged on the SA date, `MSF/MsfCampaignRules.cs:704`);
    - the DTO additions in §§ 3–5.

    None needs to land first. They are this flow's pages and mappers, and no domain rule moves. If Q8 is a mail, it
    rides on T367's existing path inside the build. If Q10 is "refuse", that is a rule change and should land first as
    its own task. T272's foreign key, T153, T320, T334, T276 and T311 stay their own flows'.

## 8. Draft must-fixes for round 2's ask (on P confirmed, RA and MA picked)

- **R1.** A spec board and a steps board, as flow 09's round 2:
  - every component used, with NEW marked;
  - every DESIGN.md rule changed (§ Table system, § Form system, § Detail page, § The MSF campaign page, § Anonymous
    static page);
  - every PROPOSED item (the Close and Release dialogs, the release mail if Q8, the new template selected after
    "Template created.", Create disabled until a template exists, the data each board reads that a DTO lacks);
  - each journey's step count at 1280 and 390.
- **R2.** The cast's numbers:
  - at 3.38, Responded 0 on Molefe's row;
  - every EPA title from the seed (§ 3's list).
- **R3.** The list: "Closes" with `ClosesOn` on every row, the release time off the list (or a labelled column), one
  header order, a caption that does not name one institution for the Administrator, and the training year marked as a
  data change or dropped.
- **R4.** The campaign page:
  - the status card `--emphasis` in every state;
  - the facts (thresholds, EPAs, training year, closed on) marked as data the setup DTO gains;
  - draw 3.37 ("7 links are still being sent"), released, withdrawn (no Responded column), unavailable, and loading;
  - a refused add beside the field, keeping the address;
  - the draft whose trainee is no longer current, with Add invitee and Open disabled and their reason;
  - learner feedback's draft.
- **R5.** The report on RA:
  - the blocked state in both wordings, with the singulars ("1 has come back", "from 1 response");
  - names no EPA;
  - Under review after the nightly job closed it ("Closed by itself on …");
  - released with RC's summary line and RB's group header;
  - unavailable and loading;
  - the 390 and 768 of each;
  - `ReleasedOn` and the report's name, window and invited total marked as data the report DTO gains.
- **R6.** The dialogs:
  - Withdraw's under-review wording;
  - Release with the "not recorded" EPA line and the no-EPA line;
  - Q8's sentence, if a mail;
  - Q10's warning, if warned;
  - each dialog's cancel returning the focus to its opener.
- **R7.** `/msf/respond`:
  - closed (3.45), expired, not recognised, the fault, the browser's required prompt;
  - learner feedback's questionnaire;
  - each at 390 and 1280;
  - the chosen radio's tint as CSS only;
  - no `tabindex` on the h1.
- **R8.** MSF reports on MA:
  - empty;
  - another trainee's id, with "That report could not be found…" over her own list, said once;
  - an EPA not recorded;
  - loading and a load error with Try again;
  - 390 and 768.
- **R9.** Coverage: "No trainees to show", loading, a load error, two programmes (the Administrator); the real titles.
- **R10.** The heavy volumes:
  - ten campaigns in the list at 390;
  - a group of a dozen comments of up to 4,000 characters (wrapping, never cut);
  - fifteen EPAs ticked;
  - PAED-015's 119-character label in the report, the campaign facts and the evidence list.
- **R11.** Every page: one h1 then h2s; every table captioned; Try again in place of an exception's text (T329, T272);
  no message naming a property (T324); 768 for the list, the draft's rows and the report (§ 11, flow 06).
- **R12.** Q7–Q10 drawn as the operator decides; nothing else of flows 01–09 drawn as changed.
