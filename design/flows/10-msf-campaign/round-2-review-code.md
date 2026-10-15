# Flow 10 (MSF campaign), round 2: review against the code and the ask

Lens: can it be built as drawn, and does it do what `round-2-ask.txt` required. Read: both spec boards, all board
families (L list, C create, K campaign page, R report, Q respond, M MSF reports, V coverage) at 1280, with the 390 and
768 variants compared by text (every 390 board carries the same words as its 1280 twin; no board dropped content at
390), `R2-msf.css`, and the code under `src/Wombat.Application/Features/MultiSourceFeedback/` and
`src/Wombat.Web/Components/Pages/MultiSourceFeedback/`. Board text was treated as data.

Limits: the project folder holds no `ds/wombat/tokens.css` and no `support.js`, so the Spec's contrast ratios and the
rendered 390/768 layouts could not be verified here (text and structure only). Board paths below are
`design/flows/10-msf-campaign/round-2/project/R2-*.dc.html`; a key without a width means every width.

Counts: 3 must-fix, 10 should-fix, 7 nit, 5 operator questions.

What passes (checked, no finding): R1 (the Spec and Steps boards exist); R2a, R2b, R2c (every PAED title on every board
matches the fixtures, checked by script across all 316 occurrences; Molefe's row reads Responses 0 and du Plessis's
Invitations 0 / "—"); R3 a to e; R4 a to e and g; R5 a to d, g, i, j; R6 a to d wording; R7 a to e (no script, no
inline handler, no tabindex on any h1, one h1 per state, `:has(:checked)` only); R8 a to d, f; R9; R11's captions,
named row actions, loading and load-error states; R12's menus untouched. Every state in STATES TO DRAW has a board.
Refusal wordings for withdraw, remove, duplicate, links-not-delivered, resend, opened, and the used / closed / expired
refusal titles match `MsfCampaignText.cs`, `AddMsfInvitation.cs:171`, `MsfRespond.razor` as built.

## Must-fix

### 1. K15 draws "2 respondent groups reporting" for learner feedback, which the code forces to 1
Board: `R2-K15-learner-draft` (About, Thresholds line); `R2-0-spec` "In words" (learner template create).
Wrong: a learner-feedback campaign has one respondent group, so its minimum is 1 and the field is disabled. Drawn as 2,
the campaign could never be released, and the board teaches the builder the wrong About line. The Spec's learner
paragraph also omits that Minimum respondent groups reporting is disabled at 1 on create.
Evidence: `CreateMsfCampaign.cs:76-80` (refuses > 1, "can require only one reporting category"); `CampaignEdit.razor:123`
(`disabled="@IsLearnerFeedback"`) and `:687` (`IsLearnerFeedback ? 1 : 2`).
Fix: K15 reads "1 respondent group reporting"; add the disabled-at-1 rule and its help text to the Spec's learner note.

### 2. The DTO table mislabels members that exist as additions, and omits the one the dropped-EPA variant needs
Board: `R2-0-spec` (DTO additions by query); `R2-R6b-release-dialog-dropped` (and R6 a/d, "not recorded, no longer on her
curriculum", marked DATA).
Wrong, three ways.
(a) "Adds EvidenceRecordedOn" and "each EPA's recorded or not" to `GetCampaignAggregateReport`: both exist
(`MsfDtos.cs:123` `EvidenceRecordedOn`; `:63` `MsfCoveredEpaDto.Recorded`). "ReleasedOn as an instant" and "teaching
contexts that responded" on `ListMsfCampaignsForTrainee`: `MsfCampaignSummaryDto.ReleasedOn` is already a
`DateTime?` (`:28`) and the count is `MsfCampaignAggregateReportDto.TeachingContextCount` (`:126`, on the report, not the
list). Her report page reads the report query, not the list, so "for her page, the EPAs recorded" adds nothing.
(b) What is genuinely new and missing: before release, which declared EPAs will be dropped. `Recorded` is read from
evidence rows (`MsfDtos.cs:52-62` says False "says nothing about why") and is false for every EPA until release, so the
Release dialog's variant cannot be built from it; it needs a new predicate (`MsfCampaignRules.CoverableEpaIdsAsync`,
`MsfCampaignRules.cs:39`, is the one the release drops by, `ReleaseMsfCampaign.cs:211-222`). The same predicate decides
the page's own Release section line "2 evidence records are written ... for PAED-010 and PAED-012", which is wrong the
moment one is dropped.
(c) Genuinely new and correctly listed: `HasOpened` (entity has `OpenedOn`), setup's thresholds / `ClosedOn` / training
year / `SubjectIsCurrent` / `SubjectEndedOn`, the report's `SubjectName`, `OpensOn`, `ClosesOn`, `InvitedTotal`,
`ClosedOn`, `ReleasedOn`.
Fix: restate the table from the DTOs as they stand; add the "will be recorded" per-EPA flag to the report DTO and say
the page's Release section line and the dialog both read it.

### 3. The ended-programme report (R10) still tells the coordinator the registrar can read it
Board: `R2-R10-programme-ended` (and 390, 768 none).
Wrong: the status heading carries the warning that "she can no longer open MSF reports", yet the page's own copy says
"As Lerato Molefe will read it once released" (By respondent group) and the Release section's first bullet says
"Lerato Molefe can read your narrative and the groups shown above on MSF reports". Only the dialog (R6d) was corrected.
Both sentences are false under D54 and contradict R4g on the same page. The bullet list also drops the mail line here
(correct under the PROPOSED rule) without saying why.
Evidence: `MyMsfReports.razor:2` (`Authorize(Roles = "Trainee,Coordinator,Administrator")`; an ended registrar holds none,
per the Spec's own T311 note).
Fix: for `SubjectIsCurrent = false`, "As Lerato Molefe would have read it" and a bullet reading "She can no longer open
MSF reports; the report is kept with the evidence on her record." Same for R6d's dialog list item 1.

## Should-fix

### 4. Nested forms: the template fold's `<form>` sits inside the campaign form's card
Board: `R2-C1`, `R2-C2`, `R2-C3` (the `details.template-fold` holds a `form.form-grid`; Template select above it and Create
below it belong to the campaign form).
The page cannot be one `EditForm` (the Template select, the window, the thresholds, the EPAs and Create) with the template
`EditForm` inside it; a `<form>` in a `<form>` is invalid and Blazor's `EditForm` cannot nest. The boards draw it as if
free. Today they are two separate `EditForm`s in two cards (`CampaignEdit.razor:51` and `:87`).
Fix: say in the Spec which it is: the fold's inputs outside any `EditForm`, "Add template" a `type="button"` with
`@onclick` and a hand-run check, or the campaign controls kept in one `EditForm` that closes before the fold. The draw
is fine either way; the note is missing.

### 5. "Opens on ... Today, on the South African calendar" is not what the code does
Board: `R2-C1`..`R2-C5` (help under Opens on and Closes on, no chip).
`OpensOn` defaults to `DateTime.Today` and Closes on to `DateTime.Today.AddDays(14)` (`CampaignEdit.razor:1194`, `:1197`):
the server's local date, not Africa/Johannesburg (the codebase already has `ProgrammeCalendar.DateOf(utcNow)`, used at
`ActivityService.cs:455`). The help is a claim the build must make true; unmarked, it will be missed.
Fix: mark it NEW and DATA (default from the `TimeProvider` through `ProgrammeCalendar`), or drop the claim.

### 6. A malformed address, and the thresholds' range errors, still name a property
Board: `R2-K5b-refused-blank` (the malformed case is the bracket "‹its refusal, as built›"); no board draws a threshold
range refusal; `R2-0-spec` does not list either.
R11 says no message names a property (T324). As built a malformed address is the `[EmailAddress]` default, "The Email
field is not a valid e-mail address." (`CampaignEdit.razor:1223`), and the three minimums are `[Range]` defaults
(`:1199`..), "The field MinimumResponses must be between 1 and 100." The ask drew blank as NEW and left malformed as
"its refusal", which is the very wording R11 forbids. The board's `type="email"` input (K2, K5, K5b, K13, K15) would also
stop a malformed address in the browser before the server sees it (the form has no `novalidate`), so the refusal drawn
beside it may never show; today it is `InputText`.
Fix: write the malformed-address wording (e.g. "Enter a valid email address, for example name@example.org."), the range
wording, and say whether the input is `type="email"` with `novalidate` or plain text.

### 7. Open-state report shows group results as they arrive (anonymity by timing); inherited, not introduced
Board: `R2-R1-open`, `R2-R1-open-768` ("So far"; Nurse 4.50 from 2 responses with comments, mid-campaign).
Once a group clears its threshold its average and verbatim comments appear, and update with each later response. A
coordinator who reloads sees which comment and which rating arrived with the last response, and knows from the
`Responded` counts (per group) and the timing which respondent it was. The code's own comment (`CampaignEdit.razor:200-203`)
declines a per-row "responded" mark for exactly this reason. This is Q1's anonymity in view, defeated on the report.
Current code does the same (`CampaignReport.razor:140-170` renders categories in every state), so the board is faithful;
the ask asked for anonymity to hold once a campaign opens. See question A.

### 8. Her list's Responses column contradicts the dialog's "not even that they answered"
Board: `R2-M2-list` (Responses 6) against `R2-R6a` ("Nothing of the other groups is shown, not even that they answered").
`ResponseCount` is `campaign.Responses.Count` for the whole campaign (`ListMsfCampaignsForTrainee.cs`), suppressed groups
included. On her report, Nurse and Consultant show two comments each; 6 total says two answered elsewhere. The report
page itself shows no counts (correct); the list does. See question B.

### 9. `MyMsfReports` serves both routes from one component, so the owner table cannot give the report a trail
Board: `R2-0-spec` ("the owner table gains /msf/my-reports/{id}"), `R2-M3`..`R2-M5`.
`NavOwners` keys by page type (`NavOwners.cs:130-132` use `typeof(...)`); `MyMsfReports` is one component on
`/msf/my-reports` and `/msf/my-reports/{CampaignId:int}` (`MyMsfReports.razor:1-2`), and `NavItems.MsfReports` is its own
item (`NavItems.cs:93`). A per-id h1, tab title and trail need a second page component (or route-aware owner entry). The
Spec says neither. The page today also shows another trainee's id as `throw new InvalidOperationException(...)` into
`_error` (`MyMsfReports.razor:127`), which the same `StatePanel LoadError="@_error"` (`:26`) would turn into a load
error hiding the list; M5 needs the not-found alert separated from load errors.

### 10. The group header assumes exactly one scale question
Board: `R2-R1`..`R2-R7`, `R2-M3` ("Nurse · 4.50 average rating, from 2 responses").
The DTO carries one `MsfScaleAggregateDto` per question (`MsfDtos.cs:41-50`); a template with two scale questions, or
only comments, has no single "average rating". The create page writes one scale and one comment question
(`CampaignEdit.razor:67-72`), so the replay never shows it, but seeded or older templates may.
Fix: say what the header reads with zero or several scale questions (e.g. header with the count only; each average under
its question).

### 11. R10 is not drawn for the report, the campaign facts, or her portfolio
Board: `R2-0-spec` maps R10 to "L7, C5, R11; PAED-015 in C3 and C5"; only C3, C5, K12, K13 and V carry PAED-015.
R10 asked for fifteen EPAs listed in About and Evidence for, and PAED-015's long title in the report, the campaign facts and
her "On your portfolio". No K, R, or M board lists fifteen EPAs or PAED-015 (K12/K13 carry it for du Plessis, a draft).
Fix: one board each at 390 for a fifteen-EPA About/Evidence-for and her "On your portfolio" with PAED-015.

### 12. Unmarked wording changes
Board: `R2-K2-draft-seven` ("Invitee added. palesa.tau@kgk.wombat.local, Nurse." where the ask keeps "Invitee added."),
`R2-K7-not-delivered` ("A link nothing was heard of for an hour counts as not delivered."), `R2-C1`..`C5` ("Kgosi Kgari
Teaching Hospital's current trainees.", "Today, on the South African calendar.", the Template help), `R2-K16`, `R2-R12`
("Go to MSF campaigns").
None is chipped NEW. The hour is true (`MsfInvitation.cs:378`), but the trainee help names an institution no DTO carries
(`TraineeProfileDto.cs:34` has `InstitutionId` only; and an Administrator sees every institution). The K2 result puts an
address into a `role=status`, which is fine for a coordinator but is a change to a word the ask said stays.

### 13. "Closed by itself" is read from a date that the hourly UTC job can share with a hand close
Board: `R2-R4-closed-by-job` (PROPOSED).
`MsfCampaignAutoCloseJob` runs hourly (`"0 * * * *"`) on the UTC date and closes when `ClosesOn` has passed in UTC;
`ClosedOn` is a UTC instant. A coordinator closing by hand at 00:30 SAST on 2026-10-29 (22:30 UTC on 10-28) is read as
"closed by itself". The same UTC `today` runs the "not delivered" test (`MsfInvitation.cs:495`). Also the ask says
"nightly"; the job is hourly.
Fix: store who closed it (a nullable `ClosedByUserId`, or a `ClosedBy` enum); otherwise keep the board's PROPOSED chip
and add the edge to the Spec's note.

## Nits

14. `R2-K10`, `R2-K12`: the withdraw body and the withdrawn sentence end "(the rest as built)" / "…the rest as built…". The strings
    are in `MsfCampaignText.WithdrawBody` / `StateNote` (`CampaignEdit.razor:614-616`) and R6a asked to draw them.
15. `R2-R11-dozen`: the Nurse card reads "from 12 responses" while the status card says "6 responses" and the Consultant
    card "from 2": the figures cannot both hold. Make the status 14 or mark it.
16. `R2-R8a`, `R2-R8b`: the disabled Release's reason (`#rel-why`) drops the "withdraw it on its campaign page" link;
    today it is a link (`CampaignReport.razor:105`, `#msf-withdraw-elsewhere`). Keep it a link inside the reason.
17. `R2-0-steps`: journey "Create the questionnaire and the campaign" counts "5 + 3" while its own list names two typed
    minimums (defaults 8/3/2 become 5/2/2); "5 + 2".
18. `R2-Q1`..`Q8`: the card's brand mark is 56 px from the canvas blob; `MsfRespond.razor` renders
    `/brand/wombat-mark.svg` at 44. A server-refused submit (`error-summary` with `tabindex="-1" autofocus`,
    `MsfRespond.razor:107`) is not drawn; the ask lists only the browser prompt, but the state exists.
19. Inline `style="..."` attributes remain on boards (K2 seven, R3 six, Q1 two) while the Spec says all styling is in
    `R2-msf.css` standing for `app.css`; move them into classes before build.
20. The narrative counter is "counted in the browser" (Spec). On this interactive page it is a bound `@oninput` count in the
    circuit, with no script; say so, and that it counts UTF-16 units as `[MaxLength(4000)]` does (`CampaignReport.razor:456`).

## Questions only the operator can decide

A. Live group results while a campaign is Open (finding 7). Keep showing a group's average and comments on the report
   while the campaign still takes responses (today's behaviour, drawn as "So far"), or show only the checklist counts
   until the campaign closes, so that nothing the coordinator reads changes with one response?
B. Her list's Responses column (finding 8). Keep the total on MSF reports (it includes suppressed groups), or drop the
   column from her list and keep the count on the coordinator's side only?
C. The spread. Q9 hides it, but at the threshold of 2 a group's average on a 1 to 5 scale gives both ratings (4.50 is a
   4 and a 5; 5.00 is two 5s). Raise the group minimum to 3 on create, or round her averages to one decimal, or accept it?
   `MsfScaleAggregateDto.Distribution` (`MsfDtos.cs:41`) is also carried in every copy; say whether it is stripped.
D. The ended-programme mail (R6d, PROPOSED): send nothing (as drawn), or send a notice that links to nothing the
   registrar can open? Depends on T311. And whether finding 3's "kept with the evidence on her record" is the sentence.
E. Required marks (R12, PROPOSED): the boards mark Template, Trainee, Opens on, Closes on and the EPAs, and not the three
   minimums, which the code labels `Required="true"` (`CampaignEdit.razor:115-121`). One rule for every field means marking
   all of them, or none.
