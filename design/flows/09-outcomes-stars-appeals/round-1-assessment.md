# Flow 09 round 1: assessment (T366 step 5)

Read-only, at HEAD `58f8dc87`. Boards: `design/flows/09-outcomes-stars-appeals/round-1/project/*.dc.html` (12, every
board's text read). `W/` = `src/Wombat.Web/Components/Pages/`, `CD/` = `W/CommitteeDecisions/`, `A/` =
`src/Wombat.Application/Features/`, `D/` = `src/Wombat.Domain/`. **Obs** = observed in code, the runbook or the boards;
**Inf** = inferred.

## 1. Summary

- **A, the opened row:** her review opens under its row in My committee reviews (`?review=41`, PROPOSED). The appeal
  block and the decision history sit inside the row, and the lodge confirms in a ConfirmDialog. The committee's
  Appeals section stays inline, redrawn with radios, a live count, a ConfirmDialog and the result in the section. The
  committee's address stays access denied. Steps: 2/5/2/2/4/10.
- **B, a page of its own:** the list links to `/committee/my-reviews/{id}` (PROPOSED), with the Appeal in a side
  column at 1280. The committee's form hides behind "Resolve this appeal". A trainee on the committee's address is
  redirected to her own page. Steps: 2/5/2/2/5/11.
- **C, a task of its own:** B's page, but lodging and resolving each happen on a form page of its own (`…/appeal`, both
  PROPOSED). Lodging confirms with a tick, not a dialog. Resolving shows a live "What will be recorded" summary, then a
  ConfirmDialog. Steps: 2/6/2/2/5/11.
- **P (one proposal):** My authorisations gets "In force" cards plus a "No longer in force" table (revoked and expired,
  with the reason PROPOSED, superseded left off). The register gets a ConfirmDialog for revoke, a revocation line
  under the row, "Active" renamed "In force", Issued linked to the review, stacked rows at 390, and an email
  PROPOSED.
- **Recommended pick: B, drawn in one column at every width** (its 390 order: header, Decisions, Appeal, Agenda). It
  is the only structure that gives her review an address and a trail without inventing an expanded-row table pattern
  (A) or two more routes and a stale-tab state (C).
- **Borrow:**
  - **A's committee section:** the resolve form inline, as today, not behind B's "Resolve this appeal". This saves a
    step on an already 7,800 px page.
  - **A's access-denied answer:** unchanged, not B's redirect, which is a shell change (`W/../Routes.razor:17-30`).
  - **C's "What happens next" wording** for the lodge block.
  - **C's "What will be recorded" sentence** as the body of the resolve ConfirmDialog (A F3 already nearly is).
- **All three share faults round 2 must fix:**
  - Wrong cast and data: PAED-001's title, Naidoo present at Mahlangu's first sitting, the first rationale missing,
    "Sits for" drawn as a date, wrong character counts.
  - Two claims the code does not support: the appeal body named before lodging, and the reason read "only by
    committee members".
  - The "cannot seat a quorum" and "nobody can resolve" states conflated.
  - The quorum line drawn as NEW when flow 07's exists.
  - The Appeals section drawn as adjacent to the decision history, which on flow 07's page is far above it.
- **No board says the attachments went missing** (§ 4 below). Still, PAED-001's invented title and the wrong attendee
  list show the screenshots were not used. Re-attach and name the folder before round 2.

## 2. Each variation against the code

### Shared by A, B and C (Obs unless marked)

- **"Under appeal" is a real state:** `CommitteeReviewState.UnderAppeal = 5` (`D/CommitteeDecisions/CommitteeReviewState.cs:9`),
  labelled "Under appeal" (`A/CommitteeDecisions/CommitteeDecisionWording.cs:105`). "Closed" is the label of `Final`
  (`:106`). Nothing new there.
- **New labels:** "In force" and "Replaced" on a decision are NEW. They are derived from `SupersedesDecisionId`
  (`CommitteeDecisionDtos.cs` `CommitteeDecisionDto`), not a domain status. Fine, but mark them new words.
- **Wrong EPA title:** PAED-001 is drawn as "Managing the acutely ill child" on every board. The seed says
  "Providing paediatric emergency care to children" (`src/Wombat.Infrastructure/Persistence/Seeds/paediatric-epa-v11.1.json`).
  Neither the brief nor the ask gives that title, and screenshots 4.39 and 4.41 show it. **Inf:** the canvas did not
  read the screenshots.
- **Wrong attendees:** every board puts David Naidoo among those present for Mahlangu's first decision. Step 4.31
  records "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)"
  (`execution/knowledge/scenario-paediatrics/act-4-annual-review.md:883-905`; outcome state "Mahlangu 3 then 2", `:1364`).
  The brief's own example (§ 1, "Present when this decision was taken") already says so.
- **Missing rationale:** the first decision's rationale is never drawn. It is "One DOPS in the year and a Mini-CEX
  that stalled; a supervised support plan and re-review in six months." (Step 4.31). DecisionHistory must show it.
- **"Sits for" drawn as a date:** every board puts "2026-10-13" under it. In the code it is the period, "2026 S2"
  (`CD/MyReviews.razor:60`, `PeriodLabel`). The sitting's date is `ScheduledOn`, which today's detail does not print.
- **Wrong character count:** the appeal reason reads "179 of 4,000 characters". Mahlangu's reason is 153 characters.
  The 4,000 limit itself is real (`A/CommitteeDecisions/LodgeAppeal.cs:26`).
- **The appeal body before lodging is not available:** "Who hears it: Thandi Zulu (chair) and John van Rensburg
  (external)" is drawn on the lodge form, where the review is Ratified. `AppealBody` is filled only while the review is
  UnderAppeal (`A/CommitteeDecisions/GetCommitteeReviewById.cs:249-256`; DTO doc `CommitteeDecisionDtos.cs:441-452`).
  It is empty before lodging and after closing. Showing it at lodge time needs a query change, extending
  `AppealBodyOf` to Ratified (fold into T308). The names should also be read as "who can hear it now".
- **Who reads the reason is understated:** "Who reads your reason: Every committee member who can open this review" is
  false. `/committee/reviews/{id}` admits CommitteeMember, Coordinator, Administrator, InstitutionalAdmin,
  SpecialityAdmin and SubSpecialityAdmin (`CD/ReviewDetail.razor:2`). The read ladder admits:
  - the panel members who may sit;
  - the coordinators of the institution;
  - InstitutionalAdmins, and speciality and sub-speciality administrators of her programme;
  - every Administrator (`A/CommitteeDecisions/CommitteeDecisionAuthorization.cs:691-711`, `:756-767`).

  The reason prints for every such reader (`CD/ReviewDetail.razor:1452-1458`). The words must say so: "Everyone who
  reads this review on the committee's side reads your reason: the panel, and the programme's coordinators and
  administrators."
- **Names on the appeal entries need a lookup:** "Appeal by Nomsa Mahlangu" and "resolved … by Thandi Zulu" need one.
  `CommitteeAppealDto` carries `LodgedByUserId` and `ResolvedByUserId` only (`CommitteeDecisionDtos.cs` `CommitteeAppealDto`;
  `D/CommitteeDecisions/CommitteeAppeal.cs:27-34`). The data exists and the resolver is stored, so this is a small mapper
  change (fold into T308).
- **Two different states drawn as one** (A F5, B F6, C F1, "A panel that cannot seat a quorum"):
  - **No appeal body can act:** `AppealBody` is empty, and the line reads "Only the appeal body, the panel's chair or
    an external member, can resolve the appeal, and none of them can act now. A panel administrator must seat a chair
    who can." It names nobody (`CD/ReviewDetail.razor:1050-1056`). The boards' "…: the panel's chair, Thandi Zulu, and
    its external member, John van Rensburg, and none of them can act now." is not the string.
  - **The panel cannot seat a quorum** (`PanelShortfall`, `CommitteeDecisionDtos.cs:421-438`): this blocks Remitted
    only. Resolve appeal is unavailable with "Resolve appeal: {shortfall}" (`CD/ReviewDetail.razor:1424-1440`), and
    Dismissed stays available. B's "no Resolve this appeal" and A's "Resolve appeal unavailable" for this state are
    wrong.
- **An appeal body member who cannot act now** (B F6): "the who-may line names him with the rest" is false. The line
  names only the members who can act (`PanelSeat.AppealBodyAt`, `A/CommitteeDecisions/PanelSeat.cs:140-154`; T237).
- **QuorumCount is drawn as NEW, but flow 07's quorum line exists:** `p.quorum-line`, `aria-live="polite"`
  (`CD/Review/DecisionForm.razor:130`), with the words from `ReviewWords.Quorum`: "Only the chair is ticked. A committee
  decision needs …" / "N present: the chair and N others. The quorum is met." (`CD/Review/ReviewWords.cs:644-646`). The
  boards invent three wordings ("1 present: needs the chair and at least one other", "Present: 1 · needs …", "1 present:
  needs one more"). Reuse flow 07's line and words.
- **Ticks:** the chair and the resolver are ticked and locked (`CD/ReviewDetail.razor:1030-1041`), as the boards draw.
- **Quorum, Inf:** when the external member resolves, the chair plus himself already make 2, so the quorum refusal can
  only meet a chair resolving alone, as at 4.46. Worth one line on the boards.
- **Placement of the Appeals section:** A's intro says "The section stays where it is, under the decision history",
  and every committee board draws Decisions directly above Appeals. On a decided review the Decision section comes
  first and the Appeals section last, after:
  - AgendaTable;
  - Standing, MSF coverage, Snapshot and Trajectory (`CD/ReviewDetail.razor:79-100`).

  So "the replacement decision is above" is thousands of pixels above, and Dr Naidoo's "2 steps" hide a 7,800 px
  scroll. Draw the real order. The fix is one of these, and the operator decides:
  - PROPOSE an in-page link from the bar to `#appeals` while Under appeal (SittingBar is flow 07's);
  - move Appeals under Decisions while Under appeal (a change to flow 07's page, which the brief forbids without the
    operator).
- **Confirmed rules:**
  - No appeal window: `CommitteeReview.LodgeAppeal` checks only Ratified and no open appeal (`D/CommitteeDecisions/CommitteeReview.cs`,
    refusals at about `:625`, `:630`).
  - Remitted carries `UndecidedNote` and changes no STAR, and either outcome is final (`CommitteeAppealOutcome.cs:96-106`).
  - Quorum is 2, with the rule string at `CommitteeReview.cs:17-21`.
  - The refusals the boards quote ("An unresolved appeal already exists for this review.", "Choose an outcome.") are
    real.
  - The boards' prefix "The appeal was not resolved." before the quorum rule is new wording. It is fine, but mark it.
- **No mail is drawn for lodging or resolving.** Correct: T320 is flow 19's, and no board promises an appeal mail.

### A, the opened row

- **NEW patterns:** an expanded row spanning columns, and a disclosure per row (DESIGN § Table system). `DataTable`
  has neither. At 390 this nests AgendaTable's container-query stack inside a stacked row: **Inf**, fragile; flow 07's
  replay found a stacked caption shrinking (BRIEF § 11).
- **A new name string:** F5's "Open the 2026 S2 formative check-in before Paed Annual Review Panel" is not
  `ReviewRowNames`'s "the 2026 S2 formative review before …" (`CD/ReviewRowNames.cs:33-38`).
- **`?review=` also admits the Administrator:** `MyReviews` admits Administrator (`CD/MyReviews.razor:2`). Harmless,
  but the query parameter must ignore an id that is not the caller's.
- **Row and detail agreeing (T308 item 1) is drawn correctly.**
- **Access denied is unchanged:** honest about the brief's "may not touch the shell", but it does not answer the
  problem the brief raised.

### B, a page of its own

- **The redirect is a shell change:** "/committee/reviews/41 → /committee/my-reviews/41 … a redirect in the review
  page's guard, not the shell" is wrong about the mechanism. The `[Authorize(Roles=…)]` at `CD/ReviewDetail.razor:2`
  stops a Trainee before the page runs, and `Routes.razor:17-30` draws `<AccessDenied />` in place. A redirect must
  live in the shell's NotAuthorized branch or AccessDenied, which is flow 01's. B does flag it as the operator's call.
- **Two reviews in one period share an h1** (**Inf**, a real collision): "2026 S2 review" is the same for Dr Ndlovu's
  annual review and his check-in (4.51). The h1 and tab need the type: "2026 S2 annual progression review",
  "2026 S2 formative check-in".
- **The side column is a new detail-page pattern** (DESIGN § Detail page). A 4,000-character reason and the lodge form
  in a narrow column are cramped. Recommend B's 390 order at every width.
- **F6's STAR numbers are impossible:** "STAR #9" and "STAR #23" cannot occur. Act 4 issued #1–#5 and Act 5 thirteen
  more, and PAED-001's STAR at exit level is Act 4's. Use ‹n›.
- **Certificate downloads on Decided lines are feasible** (the line's STAR id and the trainee may download any status:
  `A/EntrustmentDecisions/DownloadEntrustmentCertificate.cs:18-31`, `:68-73`). They are still not recommended (Q6).
- **"Resolve this appeal" costs a step** (5/11 against A's 4/10) on the page's last section.

### C, a task of its own

- **F6 uses Anele Dlamini for an entrustment-only remit.** Her review is Annual progression, Satisfactory Progress
  (Step 4.42), and no step appeals an entrustment-only review. Draw it without a cast name.
- **Two more PROPOSED routes,** each with a stale state (F5 handles the committee's). The lodge page would need
  `DemandOwnRatifiedReview`'s refusal drawn too, for an appeal already lodged in another tab.
- **The tick that replaces a dialog** is a field that has to be validated (F3's "Tick that you understand …").
- **"Remit it, and take a new decision with at least one other panel member"** is right only with the chair present.
  Keep the QuorumRule words.
- **F2 draws "Present: Thandi Zulu (chair) · you" with the 4.46 data,** and its summary agrees. Fine.
- **ResolveSummary is NEW.** It updates on clicks only, as flagged.

## 3. The STAR pages' proposal

### My authorisations (P-authorisations)

- **What it proposes:**
  - A subtitle "The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates."
  - "In force" cards with ISO dates.
  - "No longer in force" (PROPOSED), a captioned table of EPA / Level / Status / Since / Why / Certificate, for revoked
    and expired only, with superseded left off (T319's recommendation).
  - The reason on the page, PROPOSED (Q8), with a fallback wording.
  - Downloads named per EPA.
  - Download errors under their button.
  - "Expires in" counted on the SA calendar.
  - The menu unchanged (it lights My progress; the trail is drawn as a proposal).
- **Faults:**
  - **No rationale on the cards** (Obs). Step 4.39 expects each card "with the committee's rationale", and F1's cards
    have none, yet F3 says "the rationale … as F1".
  - **The fifteen-card case (F3) uses a second card form** ("Level 5 · issued … · No expiry"). PAED-012's line drops
    Issued. Pick one card form.
  - **"No longer in force" on an empty list:** F1 shows the section with "None."; F5 hides it "when empty and nothing
    is in force". Pick one; recommend hiding it when empty.
  - **The backend is not there yet:**
    - the query reads Active only (`A/EntrustmentDecisions/GetActiveDecisionsForTrainee.cs:49`);
    - the DTO carries `RevokedByUserId`, not a name (`EntrustmentDecisionDtos.cs`), and needs a lookup;
    - an expired STAR has no stored date of its own (`D/EntrustmentDecisions/EntrustmentDecision.cs:141-159` sets
      only the status), so its "Since" is `ExpiresOn`, which the board does use.
  - **Download refusal wording:** "The certificate could not be made. Try again in a moment." is new. The real null
    result is "not found"; draw both.
  - **Dates and badge:** ISO and the SA-calendar count are correct requirements (`W/Portfolio/MyAuthorisations.razor:30`,
    `:36`, `:39`, `:132`). The badge shows at 0–30 days.
  - **Download naming:** "Download the certificate for PAED-010" differs from today's "Download certificate for
    PAED-001". Pick one and say it in the runbook.

### The register (P-register)

- **What it proposes:**
  - Revoke as an outline button named for its row, which opens a ConfirmDialog with the focus in the reason box.
  - Confirm enabled as the reason is typed (`oninput`, flagged).
  - A refusal inside the dialog that keeps the reason.
  - The result alert above the table, focused.
  - A revocation line under the revoked row: date, time SAST, reviser by name, reason.
  - A superseded row naming its successor.
  - Issued linking to the issuing review (`IssuedByCommitteeReviewId` exists).
  - A "N STARs shown" count and Clear filters.
  - Two empty states (none issued; filtered to nothing).
  - Rows stacked at 390.
  - The email paragraph PROPOSED, without the reason, with a link to My authorisations.
- **"The trainee is notified.":** the false sentence (`W/Admin/EntrustmentDecisions/Index.razor:106`; nothing sent,
  `A/EntrustmentDecisions/RevokeEntrustmentDecision.cs:35-78`) is replaced. The dialog's new consequences sentence
  ("The EPA reads 'Revoked: re-decide' on Decisions due, and her review's agenda keeps its line.") is true.
- **Faults:**
  - **"She has been emailed." (F3) claims delivery.** T319 makes a failed send log and never fail the command, so the
    result can be true while no mail went out. Drop it from the result, and keep the dialog's statement of what is
    sent.
  - **"Active" renamed "In force"** (PROPOSED). The certificate prints "Status Active" (Step 4.35), the Status filter
    and the runbook (4.35, 4.37, 5.7, 6.22) say Active, and the domain says Active. Recommend keeping "Active" in the
    register; "In force" and "No longer in force" stay section titles on My authorisations only.
  - **Vocabulary:** the h1 stays "Entrustment decisions" (the menu, flow 06), while the caption and count say
    "STARs". Say both once ("Issued STARs (entrustment decisions)") or keep to one.
  - **Missing names in the DTO:** the reviser's name in the row needs a lookup (`ListEntrustmentDecisionsForAdmin`
    fills `TraineeName` only). The successor's issue date for "Superseded on ‹D› by …" needs `SupersededByDecisionId`'s
    row, which is there but not mapped.
  - **F4's order breaks "newest first"** (**Inf**). It puts Act 4's PAED-012 between Act 5 rows, but the query orders
    by `IssuedOn` desc only (`ListEntrustmentDecisionsForAdmin.cs:105`).
  - **Off-by-one count:** the revocation reason is 69 characters, not 68.
  - **The Trainee filter already binds `oninput`** (`Index.razor:27`), one round trip a keystroke for a filter applied
    by a button. Bind on change.
  - **The Issued link is "plain text otherwise" for readers who cannot open the review.** That needs a per-row read
    check. **Inf:** register scope and review-read scope coincide for these roles. Confirm this, or always link.
  - **At 390 (F5) the active row drops Issued and Expires;** draw every field. The revocation line truncates with
    "…"; a stacked row must wrap, not cut.

## 4. Attachments

- **No board says the attachments were not received.** A grep of every `.dc.html` for attach, screenshot, receiv,
  capture and upload found nothing (Obs).
- **The canvas did use the mark:** `/_blob/19eee4d64e6ebc1818aa6f9007654872` appears on 11 boards.
- **The evidence says the screenshots were not read** (Inf):
  - PAED-001's title is invented (it is in the 4.39 and 4.41 captures, not in the text);
  - Naidoo is placed at Mahlangu's sitting (4.43 shows three);
  - the "Sits for" value is wrong (4.41 shows "2026 S2").
- **Before round 2:** re-attach the key screenshots and name the folder (`C:\dev\Wombat\design\upload`), as flow 06
  did.

## 5. Recommended answers to § 6

1. B's page of its own at `/committee/my-reviews/{id}` (PROPOSED route, owned by My committee reviews, its h1 naming
   the period and type). A trainee on the committee's address keeps flow 01's access denied unchanged, and the runbook's
   4.41 keeps testing the denial (the redirect is a shell change; leave it to the operator).
2. One `DecisionHistory` component lifted from `DecisionForm`'s read branch (`CD/Review/DecisionForm.razor:142-185`),
   showing the trainee exactly what the committee sees: category, "Replaced on appeal by the decision above.",
   rationale, "About the EPAs not decided", Conditions and Present, newest first.
3. One `AppealEntry` on both sides, with:
   - "Your appeal" or "Appeal by {name}";
   - its state badge in words (Open, Dismissed, Remitted);
   - Lodged {date};
   - while open, heard by {the appeal body that can act now};
   - once resolved, Resolved {date} by {name};
   - the reason, collapsed past six lines.

   Her form sits in the Appeal block after Decisions, offered only while the review is Ratified.
4. The lodge block says who can hear it now (a query change), that either outcome is final and nothing reopens the
   review, and who reads the reason in truthful words (§ 2). A ConfirmDialog follows (A's F2), not C's tick, and the
   result takes the focus in the block.
5. A ConfirmDialog for Resolve, whose body is C's "what will be recorded" sentence. Flow 07's live quorum line
   (`ReviewWords.Quorum`, `aria-live`) sits beside Resolve appeal. The refusal shows under the button with the focus,
   keeping what was typed, and "Appeal resolved." heads the Appeals section with the focus.
6. Certificates stay on My authorisations only. The agenda's "STAR #n." records what the sitting did, and a download
   there of a since-revoked or superseded STAR would mislead; what a line shows belongs to T363.
7. **(Operator's)** Send the mail, as T319 recommends: no reason, the reviser by name, a link to My authorisations.
   The dialog says it will be sent, and the result does not claim delivery. This rests on T319's revocation half, which
   lands first.
8. **(Operator's)** Revoked and expired only, superseded left off. Show the reason on the page: she may already
   download the certificate that prints it, the page is under her own access control, and only the mail stays
   reason-free.
9. **(Operator's)** Keep no deadline in Wombat for now, and add a § 3F question to the College on an appeal window.
   Keep the reason readable by every reader of the committee's page, but say who those readers are, truthfully, on the
   lodge form.
10. **(Operator's)** Keep My authorisations under My progress, with no menu item: flow 05 decided it, the Home card and
    the EPA page lead there, and a new item changes DESIGN.md and `NavMenuAuthorizationTests`.
11. **(Operator's)** Which tasks to fold in, and which land first:
    - **Land first, as its own task:** T319's revocation half: the one-off notice path, the revocation mail, and the
      trainee's query widened to revoked and expired with the reviser's name. Q7 and Q8 rest on it (§ 11, flow 05).
      T319's data-rights half stays T319's.
    - **Fold in:**
      - T308 whole, plus `AppealBody` read while Ratified and the names on appeals;
      - T260 and T264 (the register's halves);
      - T324 (the register's Status labels, the appeal form's "The Reason field is required.", the certificate's
        reviser by name, a small loader fix that Step 4.37 expects);
      - T325 (My authorisations' "today" only);
      - T328 (the reason boxes' body font);
      - T329 and T272 (these pages' load and download errors).
    - **Stay their own flows':** T310, T311, T320, T362, T363 and T365.

## 6. Corrections round 2 must carry (on B as picked)

- **C1.** PAED-001 is "Providing paediatric emergency care to children" on every board. Use the seed's titles for
  every EPA drawn.
- **C2.** Mahlangu's first decision: "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)", with
  the rationale "One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six
  months." Naidoo was not present (Step 4.31).
- **C3.** "Sits for" reads the period ("2026 S2"). If the sitting's date is wanted, label it "Sat on" and say it is
  NEW.
- **C4.** The counters must be true: Mahlangu's reason is 153 of 4,000 characters, and Mokoena's is 69 of 1,000.
- **C5.** One column at every width (B's 390 order: header, Decisions, Appeal, Agenda). No side column.
- **C6.** The h1 and tab name the period and the review's type: "2026 S2 annual progression review", "2026 S2
  formative check-in". Draw Dr Ndlovu's two (4.51) to show they differ. The trail is My committee reviews › {h1}.
- **C7.** A trainee on `/committee/reviews/{id}` sees flow 01's access denied, unchanged. Remove the redirect, or keep
  it only as a PROPOSED shell change for the operator.
- **C8.** The lodge block's "who hears it" is labelled PROPOSED data (`AppealBody` is filled only Under appeal today,
  `GetCommitteeReviewById.cs:249-256`). Its "who reads your reason" names every reader of the committee's page: the
  panel, and the programme's coordinators and administrators.
- **C9.** The committee's Appeals section is drawn where it is: the page's last section, after the agenda, standing,
  MSF coverage, snapshot and trajectory, with the decision history at the top. Propose (do not settle) an in-page
  link to `#appeals` while Under appeal. "The replacement decision is above" stays true, so leave it.
- **C10.** The resolve form inline, as in A, not behind "Resolve this appeal". It shows only to `CallerResolvesAppeals`,
  while Under appeal.
- **C11.** The quorum line is flow 07's `p.quorum-line` with `ReviewWords.Quorum`'s words, live, beside Resolve
  appeal. Not a NEW QuorumCount, and no invented wording.
- **C12.** Draw two states apart:
  - (a) Nobody on the appeal body can act: no form, and the line naming nobody ("Only the appeal body, the panel's
    chair or an external member, can resolve the appeal, and none of them can act now. A panel administrator must
    seat a chair who can.").
  - (b) The panel cannot seat a quorum (`PanelShortfall`): the form is offered and Dismissed works; with Remitted
    chosen, Resolve appeal is unavailable with "Resolve appeal: {shortfall}" beside it.
- **C13.** An appeal body member who cannot act now reads the member's view, and the who-may line leaves him out (T237).
- **C14.** The resolve ConfirmDialog's body is C's "What will be recorded" sentence; Go back returns the focus to
  Resolve appeal. The lodge ConfirmDialog is A's F2, with "Keep editing" keeping the text.
- **C15.** Appeal entries show names ("Appeal by Nomsa Mahlangu", "Resolved 2026-10-13 by Thandi Zulu"). Mark the
  name lookup as a data change folded from T308. The ‹hh:mm› times, if shown, are in SAST with the zone.
- **C16.** Decided lines keep "STAR #n." with no download (Q6). Remove F6's "STAR #9"/"#23" and use ‹n›.
- **C17.** Draw the entrustment-only remit without a cast name, labelled an illustration. Draw it on both sides (no
  category, the trainee's two sentences).
- **C18.** My authorisations cards carry the committee's rationale (Step 4.39), in one card form for three and for
  fifteen. "No longer in force" is hidden when empty.
- **C19.** No longer in force: Status (a badge), Since (the revocation date, or for an expired STAR its expiry date),
  "Revoked by Refilwe Mokoena" plus the reason (if Q8 says so), and Download named "Download the certificate for
  PAED-002 (revoked)". One Download wording across both pages.
- **C20.** The register keeps "Active" (the certificate, the filter and the domain say it). Revoked, Expired and
  Superseded stay as labels.
- **C21.** The revoke dialog says what is sent, if Q7 sends it. The result reads "Anele Dlamini's PAED-002 STAR is
  revoked." with no "She has been emailed".
- **C22.** The revocation line names the reviser and the date (SAST time, with the zone) and wraps at 390, never cut
  with "…". At 390 every stacked row shows all its fields, Issued and Expires included.
- **C23.** The register stays newest issued first. Fix F4's order, and draw the superseded row's line naming its
  successor.
- **C24.** The Trainee filter binds on change, applied with Apply filters (no keystroke round trips). The register has
  one caption and one wording for its subject: say "STARs" and "entrustment decisions" once together.
- **C25.** Every page: loading (header plus skeleton, no action), a load error with Try again, one h1 then h2, every
  table captioned, and 768 for the list and the register.
- **C26.** Steps: recount for B as corrected, at 1280 and 390, for the six journeys, plus Dr Mokoena's revocation
  (4.36) and Dr Dlamini's My authorisations (4.42).
