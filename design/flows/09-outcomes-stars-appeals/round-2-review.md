# Flow 09 round 2: the review

Round 2 (canvas version 1791962299-ac6d, saved in `round-2/`, `1221c005`) drew B and P at fidelity: Spec, Steps, My
committee reviews (1280, 768, 390), her review page (1280 a–c, 768, 390 a–b), the committee's Appeals section (the
order board, 1280 a–c, 390 a–b), My authorisations (1280 a–b, 390) and the STAR register (1280 a–c, 768, 390). Reviewed
2026-10-14 from three sides, each a Sonnet agent reading the boards' text: **the code** (`src/`), **the cast** (the
runbook, the seed and the baseline) and **the design system** (`app.css`, DESIGN.md, BRIEF.md, accessibility). Every
must-fix below was checked against the code or the runbook before it was carried; what did not hold is said where it
was dropped. Severity: M must-fix, S should-fix, N nit. Code is cited at `1221c005`.

## Decisions for the operator

| | Question | Recommendation |
|---|---|---|
| F1 | **The order of My committee reviews.** Today it is `OrderByDescending(ScheduledOn)` only (`ListReviewsForTrainee.cs:43`); every fixture review is scheduled 2026-10-13, so the captures (4.51, 5.8) show the tie in stored order, oldest first. The boards draw Molefe #7 over #1 under "newest first" but Ndlovu #5 over #6: inconsistent, and nothing decided it. | **Newest first, consistently,** with `ThenByDescending(Id)` (a build change): #7 over #1, #6 over #5; 4.51's and 5.8's Expects say so. |
| F2 | **The expired STAR has no replay data:** PAED-010 #2 is superseded by #18 before its 2026-11-02 expiry. | **Test it as a `states.md` row** on a scratch database with the clock moved past 2026-11-02; no journey step, and the boards keep it as "Illustration". |
| F3 | **Revoke the STAR: disabled until typed, or refused when blank.** C21 drew it enabled as the reason is typed. DESIGN.md never disables a button natively (§ Button system) and points no refusal at a disabled control; it also needs `@bind:event="oninput"`, a round trip a keystroke, which the client-side counter exists to avoid. The lodge already refuses on blank. | **Always enabled; a blank reason is refused under the box,** "Give a reason for the revocation.", the focus on the box. |
| F4 | **The download wording against label-in-name.** C19's "Download the certificate for PAED-001" on a card whose visible text is "Download certificate" does not contain the visible label (WCAG 2.5.3). Today's cards already read "Download certificate for PAED-001" (`MyAuthorisations.razor:58`; DESIGN.md § Table system, "each STAR's 'Download certificate for PAED-003'"). | **Keep DESIGN.md's pattern:** "Download certificate for PAED-001", "Download certificate for PAED-002 (revoked)". The register keeps T239's row names ("Download the certificate of Anele Dlamini's PAED-002 decision, issued 2026-10-13"), which contain its visible "Download"; C19's "one wording" means My authorisations' two tables. |
| F5 | **"Go to the appeal"** (C9, PROPOSED): Appeals is the committee page's last section, ~7,800 px down at 1280 and further at 390. | **Adopt,** in the reader's sitting bar while Under appeal, focusing the Appeals h2 through `PageFocus` (never a bare `href="#appeals"`: `<base href="/">` makes it a navigation to Home). |
| F6 | **Where Keep editing and Go back put the focus.** C14a says the box; the Spec says the opener. | **The opener,** as the native modal `<dialog>` already does (Lodge appeal, Resolve appeal); the text is kept, the box one Shift+Tab away. |
| F7 | **C13 cannot happen to a committee member.** A seat counts only while its holder may sit (`WorksOnReview`, `CommitteeDecisionAuthorization.cs:485–513`, T279), so Dr van Rensburg with a lapsed seat gets "The committee review could not be found among the reviews you can view.", not the member's view. That view of an appeal-body member who cannot act exists only for someone who also reads through another rung (Coordinator at the panel's institution, InstitutionalAdmin, Administrator). | **Keep T279;** redraw C13's case as a Coordinator at Kgosi Kgari who also holds the External seat and cannot act now, and say on the Spec that a lapsed member alone is refused as not found. |
| F8 | **Who may open `/committee/my-reviews/{id}`.** The list admits Trainee and Administrator (`MyReviews.razor:2`); the review ladder lets an Administrator read every review (`:705–708`), so the page, calling `GetCommitteeReviewByIdQuery` alone, would show an Administrator any review in any state, with a lodge block. | **Only the review's own trainee, and only ratified or later, for every role;** anything else is flow 01's not found. An Administrator reads a review at the committee's address. |
| F9 | **Show or Apply filters** on the register. C24 kept Apply filters; flow 08's F2 settled Show for every list (`PanelsList.razor:58`). | **Show,** as flow 08; the two older lists (Audit, Data rights) are a follow-up. |

Settled from the evidence (no decision needed):

- **The cast holds:** every EPA title and code against the seed; levels; STARs #1–#18 and #18 superseding #2; reviews
  #1, #2, #4–#7; training years; windows; the Present lists; the reason counts (153, 69); C26's counts recompute at
  1280 and 390; the contrast table recomputes (no new token).
- **The appeal rules as drawn are the code's:** Ratified only (`CommitteeReview.cs:623`); one open appeal; Final after
  resolving; Dismissed takes no attendance; Remitted replaces the decision, carries the undecided note and changes no
  STAR; the appeal body is the chair and external who may sit, no Administrator bypass (`PanelSeat.cs:146–153`); quorum
  2, and a chair alone is the only remit the quorum refuses (C11's note holds).
- **Words that exist as drawn:** the state labels and badge tints, `AppealOutcomeLabel` and its help, the review-type
  labels (C6's h1s), the AppealBodyNote's three sentences, `PanelShortfall`'s second branch (C12b), "Choose an
  outcome.", "Appeal lodged.", "No appeal has been lodged against this review.", "Revoked: re-decide".
- **Routes and the menu:** `ReviewDetail` refuses a Trainee at the route (C7 unchanged); a formative review closes to
  Final, so her page shows no Appeal section; `NavOwners` needs one row for the new page; My authorisations already sits
  under My progress (Q10 as built).
- **The register:** `IssuedOn desc, Id desc` (`ListEntrustmentDecisionsForAdmin.cs:105`) gives the drawn order; Revoke
  only on Active; the status badges exist.
- **The row names on My committee reviews** are `ReviewRowNames`' (`Base` prefixes "formative" only; a shared name adds
  the type as a tie-breaker to both rows): Molefe's two read "View the 2026 S2 review before Paed Annual Review Panel,
  Annual progression" and "…, Pre-graduation". The boards' "pre-graduation review" is redrawn to that; no code change.

**Data the build must add** (nothing below is returned today):

| Board shows | Gap | Size |
|---|---|---|
| The lodge block's "heard by …" while Ratified; the lodge dialog's names | `AppealBodyOf` reads the appeal body only Under appeal (`GetCommitteeReviewById.cs:252–256`); read it for the trainee's own Ratified review; re-read the review after Lodge (`WithNamesFrom` copies the empty body) | S |
| "Kgosi Kgari Teaching Hospital" in the lodge block (C8c) | The panel's institution name on the detail DTO | S |
| "Appeal by {name}", "Resolved … by {name}" | `LodgedByName`, `ResolvedByName` on `CommitteeAppealDto` (T308) | S |
| The appeal entry's badge (Open, Dismissed, Remitted) | A `BadgeFor` mapping for an appeal | S |
| My authorisations' revoked and expired rows, the reviser by name, Since | T367: the trainee query widened (`GetActiveDecisionsForTrainee.cs:49` is Active only); `RevokedByName`; Since from `RevokedOn` (SA date) or from `ExpiresOn` | M (T367) |
| "Expires in N days" on the South African calendar | `ProgrammeCalendar` in place of `DateTime.UtcNow` (`MyAuthorisations.razor:39, 132`) | S |
| The register's line "Revoked … at hh:mm SAST by {name}" | `RevokedByName`; `ActivityMoments.When` exists | S |
| The superseded line | No `SupersededOn` is stored; the successor's `IssuedOn` (S6 below words it so) | S |
| Issued linking to a review the viewer may open | `IssuingReviewReadable` through `CommitteeReviewReadAccess.ReadableAsync`; plain text otherwise | M |
| The revoked certificate's reviser by name (4.37) | A name lookup in `EntrustmentCertificatePdfService` (it prints the user id, `:189`; T324) | S |
| The download refusal and failure words | `MyAuthorisations` prints `exception.Message` (`:121`); T272's words | S |
| The quorum line on the appeal form; the refusal under Resolve appeal | UI only (M8) | S |
| Her review page's sections | Already in `CommitteeReviewDetailDto` | none |

## Must fix

| # | Side | Finding | Fix |
|---|---|---|---|
| M1 | cast, code | My committee reviews' order contradicts itself: Molefe #7 over #1 "newest first", Ndlovu #5 over #6; the build sorts ties in stored order | As F1: newest first on every board, `ThenByDescending(Id)`; 4.51 and 5.8 say so |
| M2 | code | C13's board (3 a) draws Dr van Rensburg, a committee member with a lapsed seat, reading the member's view; T279 refuses him the review | As F7: redraw as a Coordinator holding the External seat who cannot act; the who-may line leaves him out |
| M3 | code, design | "A lodge refused" (2 b) shows "An unresolved appeal already exists for this review." from a second tab. A stale lodge meets `State != Ratified` first (`CommitteeReview.cs:623–626`); the second sentence cannot be reached through the page | Draw "Only ratified reviews can be appealed."; keep the other as an unreachable guard in tests. (The ask listed both; the fault was the ask's) |
| M4 | code | "A revoke refused" (6 b) shows "The entrustment decision could not be found among the decisions you may revoke." for a second tab. A second revoke finds the decision and `Revoke` throws "Only active entrustment decisions can be revoked." (`EntrustmentDecision.cs:122`); the not-found sentence is an unknown id or one out of scope | Draw "Only active entrustment decisions can be revoked."; the not-found sentence stays a test, not a board |
| M5 | code, design | The Spec lists ConfirmDialog "used as built" for Revoke. Action mode has a `BodyContent` slot, but no way to put the first focus in a field (Cancel carries autofocus) and `ConfirmAsync` always closes after `OnConfirm` (`ConfirmDialog.razor:125–143`), so "a refusal inside the dialog, the reason kept" cannot happen | List ConfirmDialog as CHANGED: initial focus on a named field, and `OnConfirm` able to keep the dialog open on a refusal it reports; the DESIGN.md dialog paragraph says so. Lodge, Remit and Dismiss are as built (their refusals land on the page). Size M |
| M6 | design | "Revoke the STAR enables as the reason is typed" breaks DESIGN.md's button rule and costs a round trip a keystroke | As F3: always enabled, blank refused under the box; board 6 b (and 390) and 4.36 |
| M7 | design | The cards' Download: visible "Download certificate", name "Download the certificate for PAED-001": the visible text is not in the name (WCAG 2.5.3, A) | As F4: "Download certificate for PAED-001", "… for PAED-002 (revoked)" |
| M8 | code | The appeal form's quorum line and its refusal "under Resolve appeal" are drawn as flow 07's, reused, unchanged. Neither is built there: `p.quorum-line` is in `DecisionForm.razor:130` only, and a resolve refusal renders at the Appeals section's head (`ReviewDetail.razor:1372`, `AppealsPlace`) | The Spec names both CHANGED: the quorum line from `ReviewWords.Quorum` on the appeal form; the refusal in a place after the form's actions. Size S each |
| M9 | design | `.issued-review` is a block link of 21.6 px (0.9rem × 1.5), alone in its cell, at every width: under the 24 px floor flow 08 held (`1ec7a352`) | Inline-block, 24 px tall; 44 px at 390 like the row's buttons |
| M10 | cast | A.7.7's board (6, 390) captions "19 shown" and draws about 17; the runbook has 18 rows (Molefe 16, Dlamini 2; A.7.7 Actual) | "18 shown", 18 stacked rows |
| M11 | cast | The Steps board drops the certificate checks, the only place a certificate is verified: 4.35 (the PAED-010 certificate: Kgosi Kgari Teaching Hospital, "Statement of Awarded Responsibility", Lerato Molefe, the level, Issued, Expires, Status Active, the rationale, the evidence, the panel, the review, the chair) and 4.39 (the PAED-001 certificate: Status Active, Mini-CEX #12 and CBD #13). 4.37's certificate clause is kept (the lens said otherwise; it holds) | Restore them word for word from today's Expects |

## Should fix

- **States missing at 390** (design S12, cast S6): her review page for the pre-graduation review (5.8), the formative
  check-in and Dr Ndlovu's annual review; on the register: none issued, five issued, filtered to Molefe, filtered to
  Revoked, fifteen Active ("· 15 shown", 5.7: no board draws it at any width), All with the Superseded line, a paused
  EPA, the refused revoke, loading. And My authorisations at 768 (the cards' grid below).
- **A trainee with only revoked or expired STARs** (code S9): "In force" with its empty sentence, then the table; no
  board draws it.
- **Expiry** (code S4, S5): "Expires in N days" counts on the South African calendar (T325) and reads "Expires today" /
  "Expires in 1 day"; an expired STAR is valid through `ExpiresOn`, so Since is the day after it (2026-11-03) and the
  Why reads "Its expiry date, 2026-11-02, passed." The 03:30 UTC job's sub-day
  lag is noted, not drawn.
- **The superseded line** (code S7): no supersession date is stored; "Superseded by STAR #18, issued 2026-10-13." reads
  the successor's own date without a new column.
- **The lodge refused leaves a stale page** (design S14): after the refusal the page reads the review again, so the
  form gives way to Under appeal and the open entry; draw it.
- **The revoke dialog's promise** (code S13): T367 skips a deactivated account, and a graduate's link opens access
  denied (flow 13's). "Anele Dlamini is sent an email" is worded so it stays true when the send is skipped.
- **Issued's review link** (code S8) shows as a link only where the viewer may open the review, plain text otherwise
  (the data row above); the Spec's flag becomes the rule.
- **DataTable** (code S11, design S13c): the component has `Stack` only (`DataTable.razor:35–38`). Either it gains
  `StackWide`, listed CHANGED, or the three tables are hand-written as flows 06 and 08's are; the Spec says which.
  "FilterBar" is a pattern (`search-container`), not a component.
- **CSS reuse** (design S1–S5, S7): `div.actions-cell`, not `.row-actions`; `.detail-list`, not `.review-facts` and
  `.star-facts`; flow 03's `.has-note` / `.history-note` pair generalised, not `tr.has-line` / `.star-row-line`; drop
  `textarea.form-control { font-family }` (the global `font: inherit` does it) and use `.field-help` for the counter;
  scope the phone rule to `.review-appeals` and the trainee's page, never the whole committee page (`.outcomes` there
  would change its agenda and decision forms), and drop the two rules flow 07 already has; a grid for the cards
  (`.details-grid` is `1fr 2fr`), one column at 390, checked at 768.
- **"Show the whole reason"** (design S9): the trigger is measured (`scrollHeight > clientHeight` after render), not a
  character count; the unprefixed `line-clamp` beside the prefixed one.
- **The register's filtered count** (design S10) is announced: a visually hidden `role="status"` carries "3 shown".
- **DESIGN.md text** (design S13): § Badges (the appeal's three), § Button system (the dialog, M5), § Table system (the
  tables as built), § Page-level patterns (My committee reviews' State a badge, View named by the review), §
  Accessibility (label in name on cards, the filtered-count status), the app.css section's place and classes; loading
  and failed heads read "Committee review" on both pages: say so.
- **The Steps board's lost and lagging checks** (cast S1, S2, S5, and M4 downgraded here: 4.44's "Heard by the appeal
  body" is vague, not wrong): 4.35's Expires "—", the trail, and "STAR id descending" for one day's ties; 4.36's
  dialog in the Do; 4.37's Decisions due "Revoked: re-decide", its sentence and one Schedule, By EPA PAED-002 (Decided
  0, Not decided 4, To schedule 1), "71 of 75" in place of "Decisions due unchanged"; 4.39's Home card; 4.41's list
  columns, no Deferred, the committee's sentence on each of the twelve lines; 4.43's route list › page › list and its
  Do (blank press, typing, Lodge, the dialog's Lodge); 4.44 the appeal body by name; 4.45's option words, the help, no
  Upheld, conditions optional; 4.46's Do reads the quorum line first; 4.47's carried "About the EPAs not decided" and
  the SQL check; 4.51's route to both pages; 5.7's rows at their levels, no expiry, the Superseded row's Download only,
  the new order; 5.8's names present, the lodge block unused, each card's level; 6.22's and A.7.7's EPA list, 11.1's
  items and "Set by the College", A.7.7's Home, panels, Decisions due and menu. The Gaps that close (F-4.36a,
  F-4.37a, F-4.43a, F-4.45c, F-4.48a, T260's entrustment half) are listed.
- **Placeholders** (cast S3, S4): review #2's rationale is "Steady progress in year 3; two EPAs entrusted at this
  sitting." (4.42-2); Act 5's STAR rationales "PAED-0nn entrusted at its exit level; the panel recommends completion."
  (5.8-3). The revocation time stays ‹hh:mm›: its step asserts the shape and `RevokedOn` + 2 h, as 4.50 does. The
  formative page's "A formative check-in records no decision and issues no STAR." is marked NEW.

## Nits

- The Spec's order line reads "header, the review's facts, Decision"; the facts are the header's, and the heading is
  Decisions where two show (4.48) (code N1).
- The remit form offers seven categories today (Graduate among them); the boards draw six (code N2).
- `PanelShortfall`'s first branch (the chair cannot sit) is undrawn (code N4).
- The trainee's not found is flow 01's: `NavigationManager.NotFound()`, the ladder's refusal mapped to it (code N6);
  `ReviewLinks` gains the trainee's address (N7).
- The counter script survives re-renders and ships as a module under the CSP nonce; `maxlength` never cuts a paste
  silently (code N8). Dates shown from UTC columns convert on the SA calendar (`ActivityMoments`) (N9).
- The quorum line and the refusal say one sentence twice in two live regions (design N2): vary the refusal's lead.
- Board 3 390 a's caption speaks of 1280 (design N1); "—" against "No expiry" on the two pages (N3); "Issued … at
  review #2" names no destination (N3); two identical "Entrustment decisions only" h3s (N5); the lodge dialog repeats
  the readers sentence (N6); icon names from `Icon.razor` (N7); the flex `ul` checked with VoiceOver (N8).
- 4.45's "himself ticked and locked" and 4.46's chair-only tick read as contradictory without a clause (cast N1);
  C26's Mahlangu count is the happy path (N2); "the replacement decision is above" and "the decision above is theirs"
  (N3).
