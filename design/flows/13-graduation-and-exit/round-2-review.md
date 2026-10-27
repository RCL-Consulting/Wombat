# Flow 13 round 2: the review

Round 2 (the boards saved in `round-2/project/`: 168 boards, 77 at 1280, 78 at 390 and 8 at 768 (H08 has no 1280 twin), plus
`R2-0-spec`, `-spec-2`, `-spec-3`, `R2-0-steps` and `-steps-2`) drew Home (H), the staff export's entry points (X), Export
(E), Verify (V), the trainee profile and the end of a programme (P), Completed and closed (C) and the ended record pages (R),
with the Illustrations marked. Reviewed 2026-10-27 from three sides, each a Sonnet agent: **the code**
(`round-2-review-code.md`, K1-K27: `src/`, the ask, T312, T313), **the cast** (`round-2-review-cast.md`, C1-C13: the runbook,
the fixtures and the baseline captures) and **the design system** (`round-2-review-ds.md`, D1-D15: `bundle.css` v33, DESIGN.md,
`app.css`, measured in Chromium on a scratch copy). The three source files are kept as they are; this file merges them. The
integrator checked the must-fixes against the code before carrying them; what was narrowed or is not a fault is said where it
was. Severity: M must-fix, S should-fix, N nit. Round 1's M1-M7 are all fixed on the boards (K's closing table).

**The captures did not reach the canvas.** The Spec says so (`R2-0-spec`), as in flow 12; the cast reviewer opened eleven
from `design/baseline/act-5/` and found every bracket the runbook or a capture fills (C8). Round 3 fills each bracket the
cast table gives a value for; the operator re-attaches the captures or connects the upload folder. What holds, by all three:
every person, address, role, id, date, hash, count and order against FIXTURES and the runbook; Q1-Q27 as decided (Q1, Q3, Q9,
Q12, Q13, Q16, Q21, Q22, Q24, Q27 checked against code); the built words quoted as built, verbatim; the 15 Illustration chips
are exactly Spec 3's list; the 390 and 768 twins carry the same words as their 1280 boards (85 pairs); no raw colour, no
inline `style=`, no unknown class, no dangling id, one h1 per board; no page or table scrolls sideways at any width (but see
S24); every 390 control is 44 px; the dialogs, results and load errors follow flow 12's banked patterns.

## Must fix

| # | Side | Finding | Fix |
|---|---|---|---|
| M1 | code (K1), cast (C2) | **The refused-export Illustration X08 and Spec A3 rest on a false guard.** `MayReadAsync` returns true for an Administrator (`TraineeScopeResolver.cs:446-449`) and the export handler admits one, so "an Administrator is refused the export" cannot hold. X08 also draws a progress link beside it the review page never draws for an Administrator or an Institutional admin (`ReviewDetail.razor:218-229`: `RosterRoles` are the four programme roles). The Spec names no read that decides whether the three entry pages show the action. Real refusals exist and none is drawn: a review whose `TraineeElsewhere` is set, and an ended profile an Institutional admin can open for someone whose preferred profile is at another institution | Drop the Administrator case. Redraw X08 as one reachable refusal (`TraineeElsewhere`, or an ended profile at another institution), or mark the state unreachable on the Spec. The Spec states the gate as a data read: a `MayExport` bool (computed by `MayReadAsync` on the trainee's user id) on `CommitteeReviewDetailDto`, `ProgrammeTraineeDto` and `TraineeProfileDto`, so the action is absent where it would be refused; say what the Administrator and the Institutional admin see on the review page (F1) |
| M2 | code (K2) | **The take-back count cannot be "read again after the save".** The replay has already taken the credit back by then (`ProgrammeEndCredit.SaveAsync`), so a re-read returns 0 and "1 activity observed after that day no longer counts." cannot be built. `CompleteTraineeProfileCommand` and `DeactivateTraineeProfileCommand` are `IRequest` (void). Mark complete (P03) draws no take-back in its dialog or its result, though it shares the replay (a graduation recorded after the fact) | Both commands return the count they took back (`IRequest<int>` or a result type, from `ProgrammeEndCredit.SaveAsync`), and the result is built from it. The dialog's count stays a preview read, worded as a forecast. Draw Mark complete's dialog and result with the take-back too (F2) |
| M3 | code (K3) | **R06 draws a state the code does not have, and T312 as written does not build it.** A paused EPA on an ended record: built, `EpaProgress` shows the `PausedRow` alert, `Lines()` yields nothing before the ended branch (`:361-380`), Entrustment says only `PausedEntrustment`, with no STAR facts and no Open My authorisations (`:117-121`, `:143-147`); the ended card has no `EpaLabel` (`EndedRecord.razor:172-198`). T312 names the quota reader, the standing, MSF coverage and the PDF but not the EPA page or the ended card. Steps 6.21's "its page as today" contradicts R06 | Widen T312 or draw today's page (F3). Either way fix Steps 6.21's "as today" |
| M4 | code (K4) | **The staff export's trail needs the profile id and `GetExportSubjectQuery` does not exist yet.** The trail's middle link goes to `/admin/trainees/edit?id=` or `/programme/trainees/{profileId}`; the route carries only the user id, and the Spec's query row lists no id. Round 1 § 7 asked for it. A readmitted person's id must come from `PreferredProfiles` | The Spec's `GetExportSubjectQuery` carries `ProfileId` (the preferred profile, null when none); the middle crumbs go through `PageHeader.Trail` (the `AdmitRegistrar` pattern, `NavOwners.cs:220-222`) |
| M5 | cast (C1) | **A.7.12's twin is the wrong board.** V02 is titled "5.13 verified, three exports (the 390 twin is A.7.12)" and the Steps row says the same, but A.7.12 verifies Dr Molefe's newest export (`344dd52b…`, 22:56 SAST, "Once", `portfolio-344dd52bdded.pdf`), which is R2-V07's content | Make V07 at 390 the A.7.12 board, retitle V02's twin as 5.13's alone, fix the Steps row and V02's subtitle |

## Should fix

- **S1 One trail rule for the personal Export page** (K5, D10; E01, E07, E08): E01 draws no trail, E07 draws "Home › Export
  portfolio" (PROPOSED), and `TrailTo` returns null for a lit personal page (`NavOwners.cs:276`); the result is two
  `aria-current="page"` on E07. Decide once (F4) and draw E01 and E07 alike.
- **S2 The lit item is decided by route, not page type** (K6; E06, E09): a staff member who is also a former trainee is
  offered a personal Export portfolio, so exporting a registrar's portfolio would light her own item and drop the staff trail
  (`NavOwners.cs:221-225`). The Unavailable page (E09) must pass `Page=` for "nothing lit". Spec line, plus the
  staff-and-former-trainee case in the edge list.
- **S3 The two refusals the Domain words differently** (K7; P02, P07, P16): `EnsureCanEnd` says "completion date" and
  "deactivation date"; the Spec renames only the first. Change both nouns to "last day" (Domain and tests), add "The last day
  cannot be before the programme start date (2023-01-15)." as a NEW row with a board or a line, and say what an emptied field
  does (`EndDate` is a non-nullable `DateOnly`).
- **S4 The profile's read changes three ways the Spec omits** (K8; P01, P05, P12-P15): `GetTraineeProfileByIdQuery` throws
  for an unknown or other-institution id and the page prints `exception.Message`; P12/P13 need a null (CLAUDE.md's get-by-id
  rule) and a fixed-words catch; the DTO must carry the end's kind and day, the training year at the end, the last ratified
  decision and `MayExport` (M1); the `StatePanel` gets `LoadError` and `OnRetry`. The file is `TraineeProfileEdit.razor`.
- **S5 The "account" sentence has no read** (K9; P05, P11, P18): "holds no role now, and can still sign in to read the record,
  export the portfolio and download the STAR certificates" needs roles held, locked or erased (erasure ends a profile as a
  withdrawal, `TraineeProfile.cs:150-157`) and a STAR count. False for a graduate who holds Assessor (H03), a locked account,
  an erased one. A DATA row and a variant for each (F5).
- **S6 The Export page's own changes** (K10; E11, E13, E14, E15): the catch prints `exception.Message` for everything
  (`ExportPortfolio.razor:97-102`), so E11 (From after To, under From) needs `ValidationException` mapped to the field and E13
  fixed words; the Spec's "returns the file name" already exists (`PortfolioExportResult.FileName`), strike it; a caller with
  no profile (typing `/portfolio/export`) passes `MayReadAsync` as self and then has no subject: draw the Unavailable page
  (F6); the staff To default on an ended record ("Lerato Molefe's last day in the programme.") needs its hint word.
- **S7 Built subtitles dropped silently** (K11; R01, R02, R05, R10, P01-P04, P06-P10, P16, P17, P19, R07): My progress'
  ended subtitle ("Your record of the programme you ended: what your curriculum held you to in each period.",
  `MyProgress.razor:101`), the active profile's ("Update curriculum and completion details for this trainee.", `:282`) and My
  activities' ("Everything you have filed or logged, newest encounter first.", `MyActivities.razor:20`, which fills the
  bracket and which the NEW subtitle drops the ordering from). Draw them, or mark each removal NEW with the old words beside it.
- **S8 The record line and Home's STAR line share one unstated rule** (K12, K16; R01, R02, R05, H01-H07): "My authorisations
  (only with a STAR)" needs a STAR count on My progress, whose standing loads in its own `try`; `EndedRecord` is shared with the
  staff page and held by a golden-markup test, so the line is added by `MyProgress.razor` around it, not for staff. "Holder of a
  STAR" is undefined for all-revoked or expired STARs. Home's query must name the profile (preferred), the kind (completed or
  withdrawn), the training year at the end and refuse a current Trainee in the no-role branch (the claim is issued for any
  profile, `WombatUserClaimsPrincipalFactory.cs:67-70`). A DATA row and the rule (F6).
- **S9 The Spec's "Where" column** (K13): eight paths do not exist (`Features/Portfolio` is `Features/Reporting`;
  `Features/Committee` is `CommitteeDecisions`; `Features/Progress` is `Features/Trainees/ProgrammeEndCredit`, `internal`;
  `Features/Dashboard` is `Dashboards`; `Jobs` is `Infrastructure/Scheduling/Jobs`; `Admin/Trainees/Edit.razor` is
  `TraineeProfileEdit.razor`; `EpaPage` is `EpaProgress.razor`). Home's former-trainee branch is static markup in
  `Home.razor`'s `default:` case, so H05/H06 need a new component under `DashboardFrame` (`DashboardLinkAuthorizationTests`
  renders it). Removing the cleanup job also removes its registration (`DependencyInjection.cs`) and its
  `ScheduledJobDefinition` row (T313).
- **S10 The not-a-hash refusal is a family** (K14; V05, V06): 12 hex digits, 64 characters with a non-hex letter, 65 or more,
  and non-hex short text each need their own sentence; "The 12 in a file's name are only its start." fits only the first.
  Write three variants (count; first bad character; the 12-digit prefix), draw V06 as the 12-digit case only.
- **S11 The closed list** (K15, N2; C01-C05): it is unbounded and filtered in memory today, so "20 a page" is a paged query
  and `PagerControls` here is new; the Administrator's list names no institution under a name (the Current list does,
  `PendingTraineesList.razor:166,175`), so the same name at two institutions is not told apart; the "1 ended" / "2 ended"
  count has no `AdmissionText` word; an erased person appears as "Withdrawn ‹pseudonym›". Add the cue, the word, and one line
  for erasure (F7).
- **S12 The email in Mark complete's dialog** (K17; P03, P17, P19): "sent to molefe@kgk.wombat.local" is new, with no NEW mark
  and no read (the DTO already carries `Email`). Mark it NEW and list the read, or drop it (F8).
- **S13 The result's Verify link** (K18; E03, E05, E06, E08): `/portfolio/verify` is static (`[ExcludeFromInteractiveRouting]`),
  so the link inside the success Alert must be a plain `<a>` (full navigation). One Spec line.
- **S14 P18's "after T312" chip and DATA row misplace the dependency** (K19; P18): an ended profile is read-only, so its
  `CurriculumId` is final and the curriculum label is built today; T312 freezes items and targets. Remove the chip and the row,
  or say what P18 shows that T312 changes.
- **S15 R10 draws two states that cannot hold at once** (C3): 17 EPA cards under a standing that reads "15 of 15 … (level 5: 9
  of 9 · level 4: 6 of 6)". Bracket the standing's counts or say which.
- **S16 Steps rewrites** (C4, C5, C6, C7, C13; steps boards): 5.8 is "Unchanged" but its Note (last point she can download from
  My authorisations) is false once Q3 lands: "Expect unchanged; Note rewritten". The row "then the steps that open the
  profile" lists 2.39 and 2.40, which do not open it (they are the Trainee's Home and My progress, unchanged); the steps that do
  are 2.30, 6.34, 6.36 and A.7.9 (each an active profile: h1 "‹name›'s programme", no Back to trainees, the End section, the
  header action), and A.7.9's two Expect lines change (closed list stacked, View 44 px; three fields plus the last day and two
  triggers). A.2.1 and A.2.9 also rewrite their nine-job list, Actual ("All nine had run"), title, the cleanup bullet, the
  90-day Note and Gap F-A.2.9a. After T312, 6.21's Do and bullets invert (15 of 15, level 5: 9 of 9, F-6.21a closes), 6.40 and
  6.41 become 15 EPAs with no KGK-001 (F-6.40a closes), and R05 omits PAED-006's corrected title (6.14). The summary card loses
  "Name:" and "Trainee summary" (5.17, 5.27, A.1.14); A.1.14's Expect never describes the closed list; the closed order flips to
  latest-first at A.1.14, 5.27, 6.34. The full list of steps whose Expect, Do, Note or Gap the build rewrites is in the cast
  review.
- **S17 Brackets to fill** (C8; R01-R08, X08, Spec 1): the runbook and the captures give a value for each, and the "capture
  5.20-2 did not arrive, so not drawn" notes on R03, R04 and R06 go. Round 3 fills them from the cast review's table, row by
  row (PAED-010 and PAED-012 Semester 2, 2026 counts 1 and 2; the "0 of 3, 3 short" semesters; the year-4 standing sentence;
  PAED-001's title and target; Entrustment level 5 and Issued 2026-10-26; "No STAR yet."; My activities' 7 rows and
  subtitle; R08's 15 titles, levels and rationales). `‹name as stored›` is "Demo Administrator" (X08 goes with M1). Not
  fillable stay bracketed: H03, H07, P16-P19, E17, E18, C04, C05, R10, V04's `5c171ec5‹…›`.
- **S18 R07 has no trail and no caption** (D1; R07 at 1280, 768, 390): the Spec's route table owns `/activities/mine` under My
  progress with "Home › My progress › the page" (R08, R09 draw it), but R07 draws none, and its table is the one table in the
  flow with no `<caption>` (the Spec says every table is captioned). Draw the trail; add a caption that does not repeat the h2,
  or say the shipped page has none and add it in the build note.
- **S19 The last-day refusal against DESIGN.md** (D2): not a fault. DESIGN.md lines 827-832 describe the built behaviour (the
  refusal an Alert in `.action-result`, the focus there), and Q7 (operator, 2026-10-27) moves the refusal under its field, which
  P02 draws. Round 3 marks on P02 and on the Spec that it amends that DESIGN.md rule ("a refusal found before any dialog stays
  beside its field with the focus in it, `role="alert"` so it is read at once"); the build amends DESIGN.md. Wording only.
- **S20 Verify's answer is a hand-built card** (D3; V02, V03, V07, V09, V10): the success is `section.verify-answer` with its own
  stripe, outside `ActionResult`, while V04 and V08 use `ActionResult`; the section is also a region landmark that exists only
  after a press. Decide once (F9).
- **S21 `.verify-details` is a second `.detail-list`** (D4; V02-V10): same grid and weights; `.hash-value` already wraps the
  hash. Use `.detail-list` (with `dd{min-width:0}` in the system if a long value needs it) and drop the class, or say in the NEW
  table why not.
- **S22 `.closed-when` is `.person-cell`** (D5; C01-C05): the same flex column with a muted second line, already used by the
  Curriculum cell of the same table. Reuse it; drop `.closed-when` from the NEW table.
- **S23 Withdrawn and Inactive are drawn as `badge-draft`** (D6; C02, C04, C05, P11): the class means draft, DESIGN.md's Badges
  list has no tone for them, and P11's grey "Inactive" badge sits beside P01's plain-word "Active". Decide the tone and name
  (F10); draw Active, Inactive and Completed alike in the summary.
- **S24 C04 scrolls 29 px at 901** (D7): measured: the table container is 593 px and the table 622 px, caused by 18
  `‹Completed or Withdrawn›` placeholder badges (`white-space:nowrap`); the real words fit at 901 with no slack, so a longer
  name or a two-word outcome would scroll the closed list between 900 and 1000 px. Draw real words in the Illustration; give the
  Programme column `.col-wrap` and `overflow-wrap:anywhere` on its cell, as flow 11 did for Users; say the "no sideways scroll"
  claim is measured with real words.

## Nits

- Illustration E17's annotation says the header reads "the registrar's institution, never the caller's other one"; the header
  comes from the subject query (M4), not the ladder (K N1).
- An activity opened from My activities by a former trainee has no owner for her, so its trail is "Home › the activity": a
  Steps line (K N3).
- A holder of Trainee acting as another role is offered My progress and My data rights only (Q2): Export portfolio is in the
  menu only while acting as Trainee, as today; an edge line (K N4).
- V05 and V06 "the field focused" needs `autofocus` on a static SSR page, unlike `ActionResult`'s `FocusOnLoad`; one line
  (K N5).
- Name the header slot each X board uses: the Programme page has no `Actions` slot today and the review page puts the link in
  `SubtitleContent` (K N6).
- P05 and P11 read "Completed Completed: 2026-10-26" (the built pair); consider one row, optional (K N7).
- R07's Mini-CEX row lacks the "to David Naidoo" sub-line that 5.28 and the capture have (C9).
- E09-E15, H05, H06, P06, P12-P15, V08-V10, X03-X07 are build states no step plays and are not marked Illustration, as asked;
  each needs a `states.md` row when built, and X03 and X04 have no replay step (C10).
- Illustrations mix a fixture's value with a bracketed person: E17's start date, E18 and P19's `molefe@` and "Training year 4"
  under a 60-character name, E16's 200 activities (the replay holds 8), C04's 38 identical curricula; bracket them (C11).
- Canvas notes use "she", "her", "his" (E01, E03, E04, E07, E08, H03, P01, P03, R01-R06, R09); the product stores no pronoun:
  say "Lerato Molefe's" or "their" (C12).
- Move the NEW rules into one `f13r2.css` as flow 12 did and keep the canvas chrome apart; 168 copies of one `<style>` block
  hold 33 NEW rules (D9).
- E14 and E15 (loading, failed) drop the trail E04 draws; P14 and P15 keep theirs: draw Home › Trainees › Export portfolio (D11).
- The end's two triggers are named by their visible word only; add `aria-describedby` to the note beside each, or an
  `aria-label` starting with the visible word (D12).
- `h2#end-h` has `tabindex="-1"` with no focus use; `.end-help` is used nowhere and `.end-decision` is a no-op (D13).
- The hash field's `aria-describedby` reads the refusal last (V06: `hash-h hash-cmds hash-e`): put the refusal id first on V05
  and V06 and say so in the DESIGN.md text, optional (D14).
- The pager's page-size select is 28 px at 390, pre-existing in every flow's pager: add `.pager .form-select{min-height:2.75rem}`
  at 390 to the system, or state the exception (D8).
- Accepted, no change: the 24 px record and Home links at 1280 (D15), the breadcrumb Home link, Mark complete's confirm being
  `btn-primary` (Q24).
- Held (K): Q27 for an Administrator acting alone (no Trainees or Programme trainees item) gives "Home › Export portfolio" by
  `OwnerFor`; not drawn, nit level.

## Decisions for the operator

The operator's Q1-Q27 (`round-1-assessment.md` § 10) stand as given; nothing below reopens them.

| | Question | Recommendation | Alternatives |
|---|---|---|---|
| F1 | **Where does a staff export link live, and who sees it?** (M1) The action appears only where `MayExport` is true; the Administrator and the Institutional admin see no progress link on the review page | **A `MayExport` bool on the three DTOs**; the export action is anchored to the header, not to the progress link; X08 redrawn as a `TraineeElsewhere` review (or an ended profile at another institution), or marked unreachable | Hide the action from the Administrator and the Institutional admin on the review page, and say so on the Spec |
| F2 | **The take-back count** (M2) | **Both commands return the count taken back**; the result is built from it; Mark complete's dialog and result carry the take-back as Deactivate's do | Draw Mark complete without one and state that a completion never takes credit back (false for a graduation recorded late, T281) |
| F3 | **T312's scope** (M3) | **T312 grows to cover R05, R06 and P18's states** (the ended EPA page for a paused EPA, `EndedItemCard`'s `EpaLabel`, the standing's count), since Q10 lands T312 first | R05, R06 and P18 draw today's page, and Steps 6.21 stays "as today" |
| F4 | **The personal Export page's trail** (S1) | **No trail on E01 and E07**, as My progress (a lit personal link draws none); drop Q2's parenthesis | A one-step trail "Home › Export portfolio" on both, with `TrailTo` and `PageHeader` special-cased and a DESIGN.md sentence |
| F5 | **The account sentence** (S5) | **Show it only when the account holds no role and is neither locked nor erased**; the STAR clause only when a STAR exists; a variant for "holds Assessor now" | Drop the sentence from the ended profile and keep it on Home |
| F6 | **What "holds a STAR" means, and the no-profile caller** (S6, S8) | **Any ratified STAR decision, in force or not**, so the My authorisations link shows whenever the page has something to list (its "no longer in force" group); Home's count says "in force". A caller with no profile at `/portfolio/export` gets the Unavailable page | In force only; a caller with no profile sees an empty form |
| F7 | **The closed list for an Administrator and for an erased person** (S11) | **Institution under each name, as the Current list**; the count word added to `AdmissionText`; an erased person listed as "Withdrawn" under the pseudonym, with one Spec line | Omit erased profiles from the closed list |
| F8 | **The address in Mark complete's dialog** (S12) | **Keep it**, marked NEW, read from the DTO's `Email` | Drop it, as a PII line a dialog does not need |
| F9 | **Verify's answer** (S20) | **Put it in `ActionResult` (`FocusOnLoad`)** as one container: the success Alert's first sentence, the `dl` beneath | Keep the card, with DESIGN.md naming it the page-load result's one exception and `.verify-answer` the stripe's only class |
| F10 | **Withdrawn and Inactive tones** (S23) | **Add both to DESIGN.md § Badges** (Completed green, Withdrawn and Inactive grey, a named alias of the grey tone, not `badge-draft`); Active a plain word everywhere in the summary | All three plain words in the summary and the list |
| F11 | **Domain nouns and the before-start refusal** (S3) | **Both nouns become "last day"** (Domain and tests); the before-start refusal is a NEW row with its own board or line | Keep the two nouns and draw each refusal with its own word |

## What round 3's ask must do

- **Restate F1-F11 as decided**, beside Q1-Q27, and draw what each decides (F1: the reachable refusal; F2: Mark complete's
  take-back; F3: R05, R06, P18; F4: E01 and E07 alike; F5: the sentence's variants; F7: the Administrator's closed list; F9:
  Verify's answer; F10: the three outcome tones).
- **Fix M1-M5 and S1-S24 board by board**, citing the id; the nits as one list, no agent each. Redraw X08 (M1); P03's dialog
  and result (M2); R05, R06 and P18 per F3; the trail on every staff export board (M4); V02's and V07's 390 twins (M5); R07
  (S18); C04 with real words (S24).
- **Drop the Administrator case** and draw a reachable refusal, or mark X08 unreachable; **name the read** the three entry pages
  use to show or hide the export action (M1).
- **The Spec**: the `MayExport` bool; `GetExportSubjectQuery` with `ProfileId`; both commands' result type; the profile read's
  null and its added fields (S4); the Export page's mapping, the no-profile caller, the staff hint (S6); the not-a-hash
  variants (S10); the DATA rows for the account sentence, the STAR rule and Home's query (S5, S8); the "Where" column corrected
  (S9); the paged closed query (S11); every NEW class a row and `.verify-details` and `.closed-when` out (S21, S22); the
  DESIGN.md text P02 amends (S19) and the badge words (S23).
- **The Steps board** as S16 and M5; recompute the journey counts after any trail change; list the build-state boards that need
  a `states.md` row (C10).
- **Fill every bracket** the cast table gives a value for (S17), keep every fixture, word and order the reviews found true,
  invent nothing; the operator re-attaches the captures or connects the upload folder, and the Spec says whether they arrived.

## Who raised what

| Here | Code review | Cast review | Design review |
|---|---|---|---|
| M1 | K1 | C2 | |
| M2 | K2 | | |
| M3 | K3 | | |
| M4 | K4 | | |
| M5 | | C1 | |
| S1 | K5 | | D10 |
| S2-S14 | K6-K19 (S2 = K6, S3 = K7, S4 = K8, S5 = K9, S6 = K10, S7 = K11, S8 = K12, K16, S9 = K13, S10 = K14, S11 = K15, S12 = K17, S13 = K18, S14 = K19) | | |
| S15-S17 | | C3, C4-C7 and C13, C8 | |
| S18-S24 | | | D1-D7 (S19 = D2 narrowed to wording) |
| Nits | K N1-N7 | C9-C12 | D8, D9, D11-D15 |

## The operator's decisions (2026-10-27)

The operator took F1–F11 as recommended ("yes, all as recommended"), with every must-fix, should-fix and nit above as
round 3's ask requires them. T312's scope grows to cover R05, R06 and P18 (F3) before the build. Round 3's ask is
`round-3-ask.txt`; the captures go with it (attached, or the upload folder connected).
