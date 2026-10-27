# F13 The programme ends: the portfolio, its verification, completion, withdrawal and the former trainee's record

**Who and why:** a registrar's programme ends, by completion or by withdrawal, and the record that leaves Wombat has to
be right, reachable and verifiable. Dr Lerato Molefe exports her portfolio, staff reproduce it byte for byte, and a
credentialing officer with no account checks the PDF by its hash. Prof Mbatha records Dr Molefe's completion and Dr du
Plessis's withdrawal; each loses the Trainee role (D54) and keeps a read-only record through the `trainee_record` claim
(T252). The former trainee comes back for her record, her STAR certificates and her portfolio. Pages: Export portfolio
(her own and the staff route), Verify (static, signed out), the trainee profile with Mark complete and Deactivate,
Trainees' Completed and closed list, the former trainee's Home and menu, and the record pages she keeps (My progress
and an EPA's page, flow 05's; My activities and an activity, flow 03's). None of Export, Verify, the end of a
programme or the former trainee's Home has been redesigned: flow 01 gave them the shell and Home's frame, flow 05 drew
My progress's ended record (R5), flow 11 moved admission off the profile page and drew Trainees, flow 12 drew the user
page's no-role section (S2, Q13). The step captures are from flow 12's replay (2026-10-26), on the shell as built; most
`states/` captures of Export, Verify and the profile's dialogs are from 2026-09-26, on the old frame.

Restated 2026-10-27 for T379 (the brief of 2026-09-26 predates flows 01–12, D54 and D55: it had Dr du Plessis keep the
Trainee role and his trainee dashboard, and asked whether a graduate may file, which D55 settled). Paths to screenshots
are under `design/baseline/` (gitignored). Code is cited at `82b108a6`; `W/` is `src/Wombat.Web/Components/Pages/`, `P/`
is `W/Portfolio/`, `Tr/` is `W/Admin/Trainees/`, `A/` is `src/Wombat.Application/Features/`, `Nav/` is
`src/Wombat.Web/Navigation/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **W** in BRIEF.md § 8: the former trainee's Home and menu (BRIEF § 7 B1) and the staff export's entry points (B2) do not exist yet; Export, Verify and the end of a programme exist and are expected to change in their parts, not their place. |
| Frequency and stakes | Once per registrar after four years, plus leavers; Verify whenever a PDF is received. Very high: the PDF and the STAR certificates are the record that leaves Wombat (T310, citing DOMAIN.md:78); an end of programme cannot be undone. |
| Mode | **Kept from the old brief, read as flows 11 and 12 read it** (BRIEF.md § 2.4). **Wireframe first** for the former trainee's Home together with her menu (Q1, Q2, Q3: what Home leads with, which pages her menu and Home offer, whether My authorisations admits her), and for the staff export's entry points together with the export page's staff header (Q4: from the trainee profile, the committee review page, the registrar's programme page and/or the user page): 2–3 STRUCTURES each. **Straight to fidelity** for Export portfolio (2–3 variations, own and staff: the header naming whose, the dates' defaults, a refusal with no form; Q5), Verify (2–3 variations of the static page: the field's help, the verified answer with several exports, the not-a-hash refusal; Q6), and the end of a programme on the trainee profile with its two dialogs (2–3 variations: where the last day and the two ends sit apart from Save profile, the refusal under its field, the ended record; Q7). **One proposal at fidelity each**, to confirm or correct, for Trainees' Completed and closed list (Q8) and the ended record pages a former trainee keeps (My progress and an EPA's page as flow 05 built them, My activities and an activity: only what changes for her, Q2, Q9). Round 2: the picks with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768 (§ 11, flow 06) |
| Variations | 2–3 structures (wireframes) for the former trainee's Home and menu, and for the staff export's entry points; 2–3 variations (fidelity) for Export, Verify and the end of a programme; one proposal each for Completed and closed and the ended record pages; the chosen ones in round 2 |
| People | Trainee → former trainee: Dr Lerato Molefe (KGK, Paediatrics / Paediatrics, Paediatric EPA Curriculum (11.1); started `J−3y` = 2023-01-15; training year 4; 15 STARs, one per EPA PAED-001 to PAED-015, nine at level 5 and six at 4, issued `D`, no expiry; completed `D`, no role since, D54). Trainee → former trainee: Dr Pieter du Plessis (started 2025-01-15, training year 2; withdrawn with last day `D−2`, no role since, D54/T290; his Mini-CEX on PAED-002 observed `D−1`, completed, credit taken back). InstitutionalAdmin: Prof Nolwazi Mbatha (KGK). Coordinator: Mr Pieter Smit (KGK). Assessor: Dr David Naidoo (5.25, acting as Committee member before and after). Anonymous: a credentialing officer at Dr Molefe's next hospital (no account). Other current trainees: Dr Anele Dlamini (year 3), Dr Nomsa Mahlangu and Dr Sipho Ndlovu (year 1). `D` is the replay day (2026-10-26 in the T376 replay); `J` the latest 15 January on or before it |
| Runbook steps | 26, pasted verbatim in § 8: 5.9–5.29 (Act 5), 6.21, 6.40 and 6.41 (Act 6), A.7.4 and A.7.12 (appendix). 5.11 and 5.18 read the PDF and the email (flows 20 and 19's outputs); 5.19 crosses flow 02's session-ended sign-in; 5.22 crosses flow 01's access denied; 5.24–5.26 are flows 03, 04 and 05's pages played by a registrar about to leave; 5.29 is flow 08's Schedule a review |
| Pages (`coverage.md` templates) | `/portfolio/export` and `/portfolio/export/{TraineeUserId}` (one file, `P/ExportPortfolio.razor`, 114 lines, a bare `[Authorize]`; the command gates the staff route, `A/Reporting/ExportPortfolio.cs`), `/portfolio/verify` (`P/VerifyExport.razor`, 121, `[AllowAnonymous]`, `[ExcludeFromInteractiveRouting]`, a GET form; `A/Reporting/VerifyExport.cs`), `/admin/trainees` (`Tr/PendingTraineesList.razor`, 348: Waiting to be admitted and Current trainees, flow 11's; Completed and closed, this flow's, `:211–240`), `/admin/trainees/edit` (`Tr/TraineeProfileEdit.razor`, 464: an active profile's form with the last day, Deactivate and Mark complete; an ended profile read-only since T305; `?id=`), `/admin/users/{UserId}` (`W/Admin/Users/UserDetail.razor`, flow 12's; its no-role section reached at 5.17 and 5.27), `/portfolio/progress` (`P/MyProgress.razor`, 314, `TraineeOrFormerTrainee`, flow 05's) and `/portfolio/progress/{EpaId:int}` (`P/EpaProgress.razor`, 415, the same policy, flow 05's), `/` (`W/Home.razor`, 108: for no role, the default branch `:57–79`, "Your training record" or "No role assigned"), `/portfolio/authorisations` (`P/MyAuthorisations.razor`, 314, `Roles = "Trainee"`, flow 09's), `/committee/my-reviews` (`W/CommitteeDecisions/MyReviews.razor`, `Roles = "Trainee,Administrator"`, flow 09's), `/msf/my-reports` (`W/MultiSourceFeedback/MyMsfReports.razor`, `Roles = "Trainee,Coordinator,Administrator"`, flow 10's), `/activities/new` (`W/Activities/NewActivity.razor`, `Roles = ActivityFilingRules.FilingRole`, Trainee only since D55, flow 03's), `/activities/mine` (`W/Activities/MyActivities.razor`, a bare `[Authorize]`, flow 03's), `/activities/{ActivityId:int}` (`W/Activities/ActivityView.razor`, flows 03 and 04's), `/committee/reviews` (`W/CommitteeDecisions/ReviewsSchedule.razor`, flow 08's) with `/committee/reviews/new` (5.29), and, as candidate entry points for the staff export (Q4), `/committee/reviews/{ReviewId:int}` (`W/CommitteeDecisions/ReviewDetail.razor`, flows 07 and 09's; CommitteeMember, Coordinator, the admins) and `/programme/trainees/{ProfileId:int}` (`W/Programme/ProgrammeTraineeDetail.razor`, flow 06's; CommitteeMember, Speciality and Sub-speciality admins, Coordinator; it reads an ended profile, `A/Programme/Trainees/GetProgrammeTrainee.cs:98–109`). Since `b347e11c` (flow 01): Export and Verify untouched; the profile page `95d4e950`/`a27983fd` (T304/T305: read-only once ended), `78e95df6` (D54: Deactivate says the role goes), `69a17c70` (flow 11: admission moved to `/admin/trainees/admit`); Trainees `f3471780` (T290), `69a17c70`, `27ff8779` (flow 11); Home `06aa51d7` (flow 04's other-role line only); My progress and the EPA page `b020c942` (flow 05, R5), `85d5a508` (flow 06); My authorisations `e9b29081` (T367), `49e7cf5e` (flow 09); NewActivity `fb6e465e` (D55); MyActivities `fb6e465e`, `78e95df6`, `cc80d5b3`; the user page `f50dffb2` (flow 02), `e53d4ae7` (flow 12). |
| Menus (`Nav/NavItems.cs`) | A Trainee's (`:180`): Home, Log an activity, My activities, MSF reports, My committee reviews, Export portfolio; then, under the rule, the personal links My progress and My data rights (`:212–214`). A former trainee holds no role, so no "Acting as" head (DESIGN.md:556): Home, then My progress and My data rights, and nothing else (observed 5.21, A.7.4). Export portfolio, My authorisations and My activities each admit her when typed (Export and My activities by `[Authorize]`; My authorisations does not, `P/MyAuthorisations.razor:2`), and no link leads to any of them. My authorisations is owned by My progress for the Trainee only (`Nav/NavOwners.cs:180`); the EPA page's "Open My authorisations" shows only to a holder of Trainee (`P/EpaProgress.razor:146`, `:298`; DESIGN.md:3652). The staff export is owned by nothing a staff role lights: the trail is Home › Export portfolio and nothing is lit for Prof Mbatha (observed, `5.10-1`). Verify is outside the rule ("an anonymous static page", `Nav/NavOwners.cs:203`): the signed-out bar, the brand and Sign in. The trainee profile lights Trainees, trail Home › Trainees › Trainee profile, with "Back to trainees" beside it (`Tr/TraineeProfileEdit.razor:13–17`). |
| Components drawn at runtime | `PageHeader` (Export's fixed "Export portfolio" / "Generate a PDF export of the trainee's portfolio.", `P/ExportPortfolio.razor:10`; Verify's "Verify portfolio export" / "Check whether a PDF was produced by this system.", `P/VerifyExport.razor:20`; the profile's "Trainee profile" with the subtitle "Update curriculum and completion details for this trainee." or, ended, "The record of a programme that has ended."), `ActionResult` with `Alert`s (Export's result at the page's top; Verify's under the form, focused on load after a press, `FocusOnLoad`; the profile's every result and refusal at the top, the last-day field naming the refusal by `aria-describedby`), `FormField` with native `<input type="date">` (Export) and `InputDate` (the profile), `FormActions` (the profile: Cancel, Deactivate, Mark complete, Save profile in one row, `:117–129`), `ConfirmDialog` ×2 (Deactivate, Mark complete; `:137–150`), `.details-grid` of `section.detail-card`s with `<h3>` titles (the profile's Trainee summary and Profile details, `:41`, `:56`; Verify's result card, `P/VerifyExport.razor:55–65`), `StatePanel` (the profile, `LoadError="@(null)"`, `:36`; none on Export), `DataTable` (Completed and closed, captioned "Completed and closed trainee profiles", no `StackWide`, `Tr/PendingTraineesList.razor:221`), `DashboardCard` (Home's "Your training record", `W/Home.razor:65–70`), `InFlight` (Export's "Generating..."), the `wombatFileDownload` script (Export). Queries and commands: `ExportPortfolioCommand` (with `TraineeScopeResolver.MayReadAsync`, one refusal "You are not authorized to export this portfolio."), `VerifyExportQuery`, `GetTraineeProfileByIdQuery`, `GetTraineeCurriculumChoicesQuery`, `CompleteTraineeProfileCommand` (removes Trainee, sends the graduation email), `DeactivateTraineeProfileCommand` (removes Trainee since D54, `A/Trainees/DeactivateTraineeProfile.cs:97`; no mail), the trainee list queries, `GetActiveDecisionsForTraineeQuery` and `DownloadEntrustmentCertificateCommand` (My authorisations). The scheduled job `portfolio-export-cleanup` deletes export records older than 90 days (`src/Wombat.Infrastructure/Scheduling/Jobs/PortfolioExportCleanupJob.cs:27`). |
| Held | None. Every step capture is from the T376 replay (2026-10-26), on the shell as built. The `states/` captures of Export (`export-portfolio--*`), Verify (`verify-export--*`) and the profile's dialogs, future-day refusal, narrow and not-found (`trainee-profile-edit--complete-dialog`, `--future-refused`, `--deactivate-dialog`, `--narrow`, `--not-found`, `--active`) are from 2026-09-26, before flow 01 (a flat Title Case sidebar with Logout, an email in the top bar: checked by opening `export-portfolio--refused`); `my-progress--exit-met`, `--withdrawn` and `--narrow-former` (2026-09-26) predate flow 05. `home--no-role` and `home--narrow-former-trainee` (2026-09-27) are on the shell. `home--trainee-ended` (2026-10-04) shows a state D54 retired (a withdrawn registrar keeping Trainee); it is not attached. The step captures stand in where there is one (§ 3) |
| Depends on | Flow 01, built (`b347e11c`): the shell, the no-role sidebar (no head), Home's frame (no subtitle for no role), access denied ("You cannot open this page"), not found, `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again, the signed-out bar on a static page. Flow 02 (`f50dffb2`): the session-ended sign-in (5.19). Flow 03 (`725237ee`) and D55 (`fb6e465e`): only a registrar whose programme is running files; an activity's page saying it counted towards nothing. Flow 05 (`b020c942`): My progress and the EPA page, the ended record (R5), ISO dates (D1), stacked rows below 641 px. Flow 06 (`85d5a508`): the registrar's programme page. Flow 07/09 (`44de180b`, `49e7cf5e`): the committee review page, My committee reviews, My authorisations. Flow 08 (`622c5ad1`): Schedule a review offers current trainees only. Flow 11 (`69a17c70`): Trainees, Admit ‹name›, `StackWide`, refusals under their field. Flow 12 (`e53d4ae7`): the user page's "‹name› holds no role: they completed the programme on ‹date›." (S2, Q13), Users' "No role". D54 (`f3471780`, `78e95df6`): completion and withdrawal both take Trainee away. The design system is **version 33**, re-synced to flow 12 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `powershell -File design/tools/stage_upload.ps1 -Flow 13`. Open each screenshot. The
   script puts any capture whose name contains "invit", "issued", "resent", "resend" or "reset" in
   `design/upload/crop-first/`; § 4 names none, so nothing should land there. No capture here shows a password; the
   verify captures show a hash and user ids, which are scenario data and go as they are.
2. **Start a new canvas** from the main app's Design page, "Wombat 13", on the Wombat design system v33
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not an earlier
   flow's canvas. Write its `tokens.css` by hand from app.css's `:root` before round 1 (§ 11, flow 09: the system serves
   only `tokens.json`).
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, the mark's asset id as flow 12 copied it), and write its `/_blob/` URL into § 1 and
   `design/flows/13-graduation-and-exit/round-1-ask.txt`. The canvas is https://claude.ai/artifact/SujH2iB2snNe7VANE4SxdK
   and its mark `/_blob/f4e05985da677926678d5d52de67cf4f` (2026-10-27).
4. **Send § 1 and § 8 as ONE message** (`design/flows/13-graduation-and-exit/round-1-ask.txt`), with the key screenshots
   in § 4. Read `project/canvas.json`'s version before and after: a send can draw nothing. Read round 1's notes for a line
   saying it never received the attachments (§ 11, flow 06); re-attach and name the folder before round 2 if so. Say
   again that every person, address and value comes from § 1 and § 8 (flow 09's round 1 invented people).
5. **Pick a structure for the former trainee's Home and menu and for the staff export's entry points, a variation for
   Export, Verify and the end of a programme, and confirm or correct Completed and closed and the ended record pages,**
   say why in a sentence, and answer § 6's questions one sentence each; the operator's are marked. A question whose
   answer rests on an open task names it, and the task lands before the build (§ 11, flow 05): here T312 (Q10). Ask for
   round 2 at fidelity, with its Spec and Steps boards.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/12-people-admin/round-2-review.md` did:
   one reviewer on the code (and the ask), one on the cast and the runbook, one on the design system; the round's
   check reads the boards' stacked rows against DESIGN.md as well as the ask (§ 11, flow 11). Put the decisions to the
   operator, then ask for one correction round.
7. **Record every round** in `design/flows/13-graduation-and-exit/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 13 — My programme ends: I keep my record, I export my portfolio and anyone can check it; staff close it right

This is flow 13 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands),
  06 (watching the programme), 07 (the review sitting), 08 (panels, what is due, the schedule), 09 (STARs, the
  register, appeals), 10 (MSF campaigns), 11 (invitation to admission) and 12 (the institution's people) are designed
  and built; every signed-in page here sits inside the shell, and Verify in the signed-out bar. Round 1: 2–3
  STRUCTURES as wireframes for the former trainee's Home together with her menu, and 2–3 for where staff reach a
  trainee's portfolio export together with the export page's staff header; 2–3 variations at FULL FIDELITY each for
  Export portfolio, for Verify, and for ending a programme on the trainee profile with its two dialogs; ONE proposal at
  fidelity each for Trainees' Completed and closed list and for the ended record pages a former trainee keeps, to
  confirm or correct. I pick before round 2.
  The runbook steps follow below in this same message. The brand mark is at /_blob/f4e05985da677926678d5d52de67cf4f: use it, never a drawn disc.
  Every person, address, role, date, hash and count comes from this message and the steps below; invent none.

GOAL:
  A former trainee (a graduate, or a registrar who withdrew; she holds no role, D54, and keeps a trainee record) must
  be able to:
    - see at once that her programme has ended, how and on which day, and that her record is kept, read-only;
    - reach everything that record holds: My progress and each EPA's page, her STAR certificates, her activities, and
      her portfolio export, from Home and her menu, never by typing an address;
    - export her portfolio and check the new file on Verify.
  Staff who oversee a registrar (Institutional admin, Coordinator, committee and speciality admins) must be able to:
    - export a named registrar's portfolio from that registrar's own pages, told whose it is before and after;
    - be refused with one sentence and no form for a portfolio they may not export, the same for an unknown one.
  Anyone holding a portfolio PDF, with no account, must be able to:
    - check that Wombat produced it, by the file's SHA-256 typed or pasted in either case, told how to compute it;
    - read, for a genuine file, when it was first generated, how many times it was exported and when last, the filter,
      and the file name, with people as ids, never names; and be told plainly when a value is not a hash at all.
  The Institutional admin must be able to:
    - record a completion (a graduation) or a withdrawal on a chosen last day, after reading what each does and
      confirming it, refused beside the field for a day after today, and land on the ended record;
    - read every ended programme apart from the current ones, and open one as the record it is, never as an edit.
  What is wrong today (the attached screenshots show each; on the shell as built, 2026-10-26):
  - The former trainee's Home is one card, "Your training record": "You hold no role at the moment. Your training record
    is kept, read-only: your progress in each period, and your portfolio to export." with one link, Open My progress
    (5.21). It never says that or when her programme ended, and nothing links to the export it mentions.
  - Her menu is Home, My progress and My data rights. Export portfolio admits her but only by its typed address (5.23);
    My activities likewise (5.28); My authorisations, the only page that downloads her STAR certificates, refuses her
    with "You cannot open this page" (5.22), and the EPA page hides its "Open My authorisations" from her.
  - The staff export is reached only by typing /portfolio/export/<user id> (5.10, 5.12). It reads "Export portfolio /
    Generate a PDF export of the trainee's portfolio." before and after, naming nobody; nothing is lit in the menu; a
    refused id still draws the form and refuses only after Export PDF. Its dates default to the last twelve months on
    the server's calendar, so every step resets From to the programme start by hand.
  - Verify reads "Enter the hash from the PDF footer" in its field, though the PDF prints no hash (5.13). The same
    hash in upper case, as Windows' Get-FileHash prints it, reads "No matching export found" (5.13). Three identical
    exports answer with whichever row comes first. The answer prints a UTC time; its card is an <h3> under the <h1>.
    The export records it checks are deleted after 90 days by a scheduled job, so a genuine PDF stops verifying.
  - The trainee profile puts the last day, Deactivate and Mark complete in the same form and the same button row as
    Save profile (Cancel, Deactivate, Mark complete, Save profile, 5.16). A day after today is refused at the page's
    top ("The completion date cannot be after today (2026-10-26).") with the field unmarked. The page is titled
    "Trainee profile", never the person; "Back to trainees" repeats the trail; Email and Name are bold run-in labels.
  - Completed and closed lists each ended profile with an Edit (named "Edit Lerato Molefe"), though the record cannot
    be edited; its columns differ from Current trainees' and it does not stack at 390.
  - My progress and the EPA page for a completed programme follow the live catalogue: an EPA the College pauses after
    she graduated drops out of her record (15 of 15 becomes 14 of 14, 6.21), and an item KGK adds after it appears on
    it with a shortfall in every closed year (6.40); the PDF reads the same.

AUDIENCE:
  - Former trainee: Dr Lerato Molefe (graduated `D`, training year 4, 15 STARs) and Dr Pieter du Plessis (withdrew,
    last day `D−2`, training year 2, no STAR). No role (D54); the menu has no "Acting as" head. Rarely: after the end,
    a few times (an application elsewhere, a credentialing office asks for the PDF). Desktop and phone (A.7.4).
  - Trainee: Dr Molefe before she graduates (5.9, 5.15), Dr du Plessis before he leaves (5.24, 5.26).
  - InstitutionalAdmin: Prof Nolwazi Mbatha, Kgosi Kgari Teaching Hospital (KGK). Records each end; exports a
    registrar's portfolio for a committee or an employer.
  - Coordinator: Mr Pieter Smit (KGK): reproduces the export from the review he has open (5.12); schedules reviews
    (5.29).
  - Assessor: Dr David Naidoo, who rates du Plessis's last Mini-CEX (5.25).
  - Anonymous: a credentialing officer at Dr Molefe's next hospital, with the PDF and no account (5.13, 5.14, A.7.12).
  - Frequency: once per registrar after four years, plus leavers. Stakes: very high. The PDF and the STAR
    certificates are the record that leaves Wombat; an end cannot be undone. Desktop 1280×800 and phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - Export portfolio (/portfolio/export, her own; /portfolio/export/{TraineeUserId}, staff): From date and To date (the
    last twelve months), Export PDF ("Generating..." while it runs; a second press sends nothing); done "Portfolio
    exported successfully. The download should start automatically." focused, and portfolio-<12 hex digits>.pdf
    downloads; refused "You are not authorized to export this portfolio." (one sentence for an unknown id, another
    institution's trainee, and a role that does not oversee her); "The start date must be before or equal to the end
    date.". Who may export: the trainee herself (also after the end) and anyone who oversees her at her institution
    (Institutional admin, a Speciality or Sub-speciality admin of her discipline, a Coordinator, a Committee member).
    The same data on the same dates gives the same bytes (no generation time in the PDF), so exports by her, Prof
    Mbatha and Mr Smit are byte-identical (5.10, 5.12); after the end, a new file says "The trainee completed the
    programme on 26 October 2026." in its Progress per EPA, so its name differs (5.23).
  - Verify (/portfolio/verify; static, signed out or in; a GET form, Verify loads /portfolio/verify?hash=…&check=1 and the
    page checks as it renders; "Verify portfolio export" / "Check whether a PDF was produced by this system."): Content
    hash (SHA-256), required; "Export verified": "This PDF was generated by Wombat on 2026-10-26 20:51 UTC.", "Trainee
    ID: 2b5abc2f-9e37-4714-bb7a-8e48fa11f699", "Exported by: …" (ids, never names: the page must not disclose a name),
    "Filter: 2023-01-15 to 2026-10-26", "File: portfolio-aba6ce7943c2.pdf"; "No matching export found. This hash does not
    correspond to any PDF generated by this system."; spaces only: "Enter the content hash to check."; a failed check:
    "The export could not be checked just now. Please try again in a few minutes."; never an error page. The result
    takes the focus after a press, not on arrival from a link. The file name carries only the first 12 hex digits.
  - The trainee profile (/admin/trainees/edit?id=…; trail Home › Trainees › Trainee profile; Back to trainees).
    Active: Trainee summary (Email, Name, Status Active); Profile details: Curriculum*, Programme start date*, Expected
    completion date ("Leave this empty to derive it from the curriculum window."), Last day in the programme (opens on
    today, South African calendar; help "Mark complete records it as the graduation day, archives the profile and
    removes the Trainee role. Deactivate records it as the day the trainee left without completing, and also removes
    the Trainee role (D54). It cannot be after today, and it cannot be changed afterwards. Either way, the period it
    falls in holds no target unless it is in that period's last month, and encounters observed after it count towards
    nothing in this programme, even those already counted."); Cancel, Deactivate, Mark complete, Save profile.
    Mark complete asks "Mark this programme complete?" / "Record 2026-10-26 as Lerato Molefe's graduation day? The
    profile is archived, the Trainee role is removed and a graduation email is sent. Encounters observed after that day
    will count towards nothing in this programme, including any already counted. The day cannot be changed afterwards,
    and this cannot be undone." (Cancel / Mark complete) → "Trainee marked complete. The Trainee role has been removed
    and a graduation email sent.". Deactivate asks "Deactivate this trainee profile?" / "Deactivate Pieter du Plessis's
    profile, with 2026-10-24 as their last day in the programme? The Trainee role is removed; they keep their own
    record, read-only. Encounters observed after that day will count towards nothing in this programme, including any
    already counted. The day cannot be changed afterwards, and a deactivated profile cannot be made active again."
    (Cancel / Deactivate) → "Trainee profile deactivated. Their last day in the programme is recorded as 2026-10-24, and
    the Trainee role has been removed." (no mail). Refused: "The completion date cannot be after today (2026-10-26).".
    Ended (T305): subtitle "The record of a programme that has ended."; Status Completed with "Completed: 2026-10-26",
    or Inactive with "Left the programme: 2026-10-24"; "This programme has ended, so its record is archived and cannot
    be changed." over Curriculum, Programme start date and Expected completion date as text; no input and no button.
    Unknown id: "Profile unavailable" / "The trainee profile could not be loaded.". Mark complete checks no committee
    decision: a Graduate decision is not required (T166's gating, undecided).
  - Trainees (/admin/trainees): Waiting to be admitted and Current trainees (flow 11's, "3 current" after both ends:
    Dlamini, Mahlangu, Ndlovu); "Completed and closed": Name, Curriculum, Sub-speciality, Outcome ("Completed
    2026-10-26", "Withdrawn 2026-10-24"), Edit.
  - The user page of someone with no role (flow 12's; draw only what links to this flow): "Lerato Molefe holds no role:
    they completed the programme on 2026-10-26. They can still sign in to read their training record (My progress) and
    their data rights." (or "they left the programme on 2026-10-24"); Users' Roles cell "No role" over "A completed or
    withdrawn registrar keeps an account and its records.".
  - The former trainee (no role; the trainee_record claim): Home, no subtitle, one card "Your training record" (above);
    menu Home, My progress, My data rights. Admitted: My progress and each EPA's page (TraineeOrFormerTrainee), Export
    portfolio (her own), My activities and her activities (her own, read-only: "Rated 3b. It counted towards no
    curriculum requirement.", Credit None for one observed after her last day), My data rights, My account. Refused
    with access denied ("You cannot open this page", "Your account holds no role that opens this page.", "If you need
    it for your work, ask your institution's Wombat administrator.", Go to Home): My committee reviews, My
    authorisations, MSF reports, New activity (D55: only a registrar whose programme is running files). Her open
    session ends within a minute of the end ("Your session has ended. Sign in again.", flow 02's). A former trainee who
    later holds a staff role gets that role's Home and keeps My progress as a personal link.
  - My progress, ended (flow 05's R5; draw only what changes): notice "You completed your programme on 2026-10-26. This
    page is your record of it and is read-only: no target applies to you any more. A period your programme ended in
    before that period's last month has no target, and no period after it is listed." (or "Your programme ended on
    2026-10-24. …"); "Your programme": Started, Completed (or Ended), Training year "4 when your programme ended"; each
    EPA's periods newest first, the last "no target (your programme ended part-way through) · n recorded"; no bars; the
    standing "You completed your programme on 2026-10-26, in training year 4 …" with the exit rule met. An EPA's page
    the same, its trajectory; a paused EPA headed "(no longer in use)".
  - Schedule a review (flow 08's, 5.29) offers only current trainees, each with the training year.
  - Data volumes: none (a registrar with no export; a closed list empty); typical (three byte-identical exports of one
    file, two closed profiles, 15 STARs); heavy (a closed list of 40 over several intakes, a record with 17 EPA cards,
    an export of 200 activities, a hash pasted with spaces and a line break, a 60-character name).

WHAT FLOW 13 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell (flow 01: the no-role sidebar, Home's frame, access denied, not found, the signed-out bar); the
    menus' items (propose, do not draw as settled); My progress and the EPA page as flow 05 built them, beyond what
    names the end or links the record; Trainees' Waiting and Current lists and Admit (flow 11); the user page (flow 12);
    the committee review page and My authorisations' own layout (flows 07, 09); session ended (flow 02); the PDF and
    the emails (flows 20 and 19); the rules above (an end cannot be undone and its day cannot be changed; both ends take
    the Trainee role, D54; only a running programme files, D55; one refusal for any export the caller may not make; an
    out-of-scope record is not found; Verify stays static and shows ids, never names).
  May, for these pages: the former trainee's Home and which pages her Home and menu offer; where staff reach the
    export; Export's and Verify's whole layout, headers, defaults, help and answers; where the last day and the two ends
    sit on the profile, the dialogs' words, where the result and the refusal show; the ended record's title and shape;
    the Completed and closed list; stacking at 390; the words. Mark PROPOSED any rule change (a page admitting a new
    caller, a new route, a new action, a new refusal, data a page does not read today): those are mine. The runbook's
    Expects change with the words.
  Say which of these each variation changes, and why.

SCREENS, in order:
  1. The former trainee's Home and menu: Dr Molefe after graduating (5.21), Dr du Plessis after withdrawing (5.28), a
     former trainee who later holds Assessor (her Assessor Home, My progress kept), an account with neither a role nor
     a record ("No role assigned"), each at 1280 and 390 (A.7.4).
  2. The staff export's entry points: where Prof Mbatha reaches Dr Molefe's export (the trainee profile, active and
     ended; the user page's no-role section; Q4), where Mr Smit reaches it (the committee review page of 5.2's review;
     the registrar's programme page); each link named for the person.
  3. Export portfolio: her own before and after (5.9); staff, naming Dr Molefe, before and after (5.10, 5.12); a
     former trainee's own (5.23); refused, one sentence and no form (an unknown id, Dr Dlamini typing Dr Molefe's id);
     From after To refused; generating; a failed export; loading; 390 px.
  4. Verify (signed out): blank; verified, with three exports of the file (5.13); verified after the end, a second
     file (5.23); upper case accepted; no match (5.14); spaces only (5.14); not a hash (a 12-digit file-name prefix;
     PROPOSED words); could not check; 390 px with the ids and the hash wrapping (A.7.12).
  5. Ending a programme (InstitutionalAdmin): an active profile with its end controls (5.16); the Mark complete dialog;
     a day after today refused, beside the field, the typed day kept (5.16); completed, the ended record (5.17); the
     Deactivate dialog and the withdrawn record (5.27); an unknown id; a load error; 390 px.
  6. Completed and closed on Trainees: two ended (5.27), none, forty; 390 px.
  7. The ended record pages: My progress completed (5.20) and withdrawn (5.28), with whatever links the record (Q2);
     an EPA's page ended; My activities and the after-the-end Mini-CEX (5.28); 6.21's paused EPA and 6.40's later item,
     as T312 would draw them (Q10).

STEPS: 5.9–5.29, 6.21, 6.40, 6.41, A.7.4 and A.7.12 (26). Pasted verbatim below (Role / Route / Do / Expect). In them,
  D is the day the story is replayed, J the latest 15 January on or before it. Where an Expect quotes today's wording
  or layout, that is today's page, not a requirement on the new one. 5.11 and 5.18 are the PDF and the email (flows 20
  and 19: do not draw them); 5.19 is flow 02's session ended; 5.22's access denied is flow 01's; 5.24–5.26 are flows
  03, 04 and 05's pages; 5.29 is flow 08's (keep them; draw only what is on these pages).

STATES TO SHOW:
  Every state named in SCREENS; loading for every signed-in page (the header and a skeleton, no action offered until
  the read is in); a load error (a sentence and Try again, never an exception's text); each confirmation, its cancel,
  and the result after it with the focus on it; a refusal beside the field that caused it, keeping what was typed;
  Verify's every answer as a static page (no client behaviour); 390 px for every page; the heavy volumes.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T311: a former trainee's Home says how and when her programme ended ("You completed your programme on
    2026-10-26." / "Your programme ended on 2026-10-24."), never "No role assigned"; it links her record: My progress,
    her STAR certificates (My authorisations, when she holds any), her activities and Export portfolio, each a page that
    admits her. My authorisations admits the trainee record (PROPOSED: TraineeOrFormerTrainee) and its Download works;
    the EPA page's "Open My authorisations" shows to her. Her menu offers what the design chooses (Q2). MSF reports and
    My committee reviews stay role-only unless decided (Q3). "No role assigned" stays for an account with neither.
  - T314: the staff export names the trainee in its header ("Export Lerato Molefe's portfolio"); her own says "your
    portfolio"; it is linked from her pages for a caller the export admits, named for her; an unknown id and an
    out-of-scope id get one refusal before any form, naming nobody (PROPOSED: a gated name query).
  - T313 and T287's verify item: a hash in any letter case and with surrounding spaces verifies; a value that is not 64
    hexadecimal digits gets its own message; several exports of one file answer with the first generation, the count
    and the last export (ORDER BY ExportedOn, Id); the records are kept for the life of the trainee's record (the 90-day
    job goes); the field's help tells how to compute the file's SHA-256 (sha256sum, certutil -hashfile <file> SHA256,
    Get-FileHash), not "from the PDF footer".
  - T312 (lands first as its own task, Q10): an ended record keeps the EPA list and targets it ended with: an EPA paused
    afterwards stays on it, headed "(no longer in use)"; an item added afterwards does not appear; the PDF follows.
  - T325 (A9): Export's default dates and every "today" on the South African calendar (it reads the server's date
    today); Verify's time in South African time with its zone, or labelled UTC by decision (Q6).
  - T329/T272's family: Export prints an exception's own text for any failure but a refusal; the profile prints it for
    a failed load, save or end, and draws "Profile unavailable" under the failure (no Try again); no page shows an
    exception's text.
  - T280: one <h1>, then <h2> in order (Verify's result and the profile's two cards are <h3> under the <h1>); a table's
    caption does not repeat its heading ("Completed and closed trainee profiles" under "Completed and closed").
  - T323: the closed list stacks at 390 (StackWide), nothing scrolls sideways; at 1280 a 64-digit hash and a user id
    wrap inside their column.
  - T264 / A5: each end confirms in a ConfirmDialog naming the person and the day, its trigger an outline button apart
    from Save profile; the result takes the focus where the record now shows; a refusal beside its field, keeping the
    typed day.
  - A11: offer only what the caller can do: no Edit on an ended record; no export link to a caller the export refuses;
    no end control on an ended profile (done, T305).
  - Done since the old brief, keep them: T305 (an ended profile is read-only: no inputs, no Save profile, its subtitle
    and line); D54/T290 (withdrawal takes Trainee too; the withdrawn registrar is a former trainee, not a trainee with
    an ended dashboard); D55 (a former trainee files nothing; New activity refuses her); T281 (an encounter after the
    last day credits nothing, taken back in the end's save); T252 (My progress and the EPA page admit the record; the
    ended record's periods); T297 (dashboard states by label); T303 (Trainee never offered by Add role); flow 12's S2
    (the user page's no-role sentence).

QUESTIONS THE DESIGN MUST ANSWER:
  1. The former trainee's Home: what it leads with (the end and its day, her record, her certificates, the export), and
     how it differs for a graduate and a registrar who withdrew.
  2. Her menu and links: which of My progress, Export portfolio, My authorisations and My activities are menu items
     (personal links under the rule) and which are links from Home and My progress.
  3. (Mine) My authorisations admits the trainee record (T311); MSF reports and My committee reviews stay role-only?
  4. The staff export's entry points: on which pages ("Export Lerato Molefe's portfolio" on the trainee profile, the
     committee review page, the registrar's programme page, the user page's no-role section), and for whom.
  5. Export: the header for her and for staff, the dates' defaults (programme start to today, or to the last day),
     the refusal before the form, where the result shows.
  6. Verify: the field's help, the verified answer's words with several exports, the time's zone, not-a-hash.
  7. The end of a programme on the profile: the last day and the two ends as their own section apart from Save
     profile; the dialogs' words; the refusal under the field; the page titled by the person; Back to trainees.
  8. Completed and closed: its columns, a View (not Edit) named for the person, a pager, stacking.
  9. The ended record pages: what names the end and links the record (certificates, export) on My progress and an
     EPA's page, and My activities without Log an activity.
  10. (Mine) T312 lands before the build as its own task, and the record freezes at its last day?
  11. (Mine) Verifying a STAR certificate (not built, BRIEF § 7 B10): leave it out?
  12. (Mine) The graduation email's false ratification sentence (T311 item 4, flow 19's output): folded into this build?
  13. (Mine) Mark complete with no ratified Graduate decision: say on the end section what the last committee decision
      was (DATA), with no gate?
  14. (Mine) A correction to an archived record (T305's open question): none drawn here?
  15. (Mine) A former trainee who returns for a second programme (T374): out of this flow?
  16. (Mine) What the build folds in.

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
  Since flow 02 also: the auth card, PasswordField with its Show toggle and PasswordRules. Since flow 03:
  RefusalSummary, DataTable's stacked rows (Stack). Since flow 05: ISO dates, the training year's wording, the ended
  record (R5). Since flow 06: the role Homes; filters applied with Show. Since flow 07: the container-query stack.
  Since flow 08: one action a row, a choice of a few kinds as radios (choice-group), a form page of its own, the
  pager's focus on the list's heading. Since flow 09: a page of its own per list row; a dialog with a field. Since flow
  10: a create page of its own reached from Home's header action. Since flow 11: the list is the page and its create
  form a page of its own; DataTable's StackWide (stacked from 900 px down); a role chosen from described radios; the
  required mark; only the chosen role's fields drawn; refusals under their field, focused. Since flow 12: a detail
  page in one column of <h2> sections with no details-grid (the user page); a form in a note panel opened in place
  (Add a role…); a destructive action confirmed, its trigger an outline button.
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
  ConfirmDialog. Verify shows people as ids, never names. A date is ISO (YYYY-MM-DD) on every screen.

ASK:
  - Round 1: 2–3 structures as wireframes for screen 1 (Dr Molefe's Home and menu at 5.21, Dr du Plessis's at 5.28,
    and at 390, A.7.4) and for screen 2 (the export reached by Prof Mbatha at 5.10 and Mr Smit at 5.12, with the export
    page's staff header), each with its reasoning and its step count; 2–3 variations at full fidelity each for screen 3
    (5.9, 5.10, 5.23, refused), screen 4 (5.13, 5.14, A.7.12) and screen 5 (5.16, 5.17, 5.27); ONE proposal at fidelity
    each for screen 6 (5.27) and screen 7 (5.20, 5.28), to confirm or correct. At 1280 and 390 px. With them, two
    boards of their own: a Spec board (every component each variation uses, NEW marked; the DESIGN.md rules each
    changes; everything PROPOSED; the data a page would need that it does not read today, marked DATA) and a Steps
    board (each variation's journeys counted, presses + fields typed, at 1280 and 390, and which of the 26 Expects below
    it would change). Stop and wait for my pick.
  - Round 2: the chosen designs with every screen and state above, at 1280 and 390 px, every confirmation, and where
    the focus lands after each action; with two boards of their own:
    - a Spec board: every NEW and changed component, its classes, the DESIGN.md text each changes, the tokens, the
      data each page needs that it does not read today (marked DATA), and every message by kind (result, refusal,
      hint, notice, empty) with its words;
    - a Steps board: each of the 26 runbook steps below, and what its Expect will say once built (quoting today's words
      and giving the new ones; "unchanged" where so), with each journey's count (presses + fields typed) at 1280 and
      390: a former trainee downloading a STAR certificate from Home; exporting her portfolio; staff exporting a named
      registrar's; verifying a file; marking a programme complete; recording a withdrawal.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (§ The NavMenu's personal links; § Dashboard page (no role); § Form page; § Detail page and the title rule;
    § Table system; § Button system's destructive action; § Alerts; § Account / auth page's static pages).
  - Mark PROPOSED anything not built: a page admitting a new caller, a confirmation, a new route, a new action, a
    refusal, a rule change.
  - Flag edge cases: a former trainee with no STAR (du Plessis); one who later holds Assessor; an account with neither a
    role nor a record; a 64-digit hash pasted with spaces or a line break; a 12-digit prefix; a staff caller who also
    oversees at another institution; a registrar whose profile was moved before the end; a last day in the last month
    of a period; the end recorded on the programme's first day; an export with From after To; two people with one name
    in the closed list.
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, every row
    action named for its row, focus after every action and confirmation, refusals tied to their fields
    (aria-describedby), targets ≥ 24 px (44 at 390), Verify usable with no script.

ATTACHED: act-5/5.21-1-molefe-home-graduate.png, act-5/5.22-2-molefe-authorisations-denied.png,
  act-5/5.10-1-mbatha-staff-export.png, act-5/5.13-1-verifier-genuine-verified.png,
  act-5/5.13-2-verifier-uppercase-hash.png, act-5/5.16-1-mbatha-complete-dialog-future-day.png,
  act-5/5.16-2-mbatha-future-day-refused.png, act-5/5.17-1-mbatha-marked-complete.png,
  act-5/5.27-1-mbatha-deactivate-dialog.png, act-5/5.27-3-mbatha-trainees-closed.png,
  act-5/5.20-1-molefe-progress-completed.png and act-A/A.7.4-1-home-390.png. Every other capture dated 2026-09-26 is
  an older frame (a Title Case sidebar with Logout, an email in the top bar): read its body only. No attached capture
  shows a password.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 5.9 | `/portfolio/export` | Dr Molefe (Trainee) | Export portfolio from the menu; the dates set by hand from the last twelve months to `J−3y`–`D`; "Portfolio exported successfully. …" focused; `portfolio-<12 hex>.pdf`. **The defaults on the server's calendar (T325); "the trainee's portfolio" for her own** |
| 5.10 | `/admin/users` → `/admin/users/{UserId}` → `/portfolio/export/{TraineeUserId}` | Prof Mbatha | the same file, byte-identical. **Reached only by typing the id; the page names nobody; nothing lit (T314)** |
| 5.11 | none (the PDF) | Prof Mbatha | not a screen: flow 20's (header, footer, sections) |
| 5.12 | `/portfolio/export/{TraineeUserId}` | Mr Smit | the same file. **Typed; no link from the review he has open (T314)** |
| 5.13 | `/portfolio/verify` | Anonymous | static, the signed-out bar; "Export verified", UTC, ids, the filter, the file. **"from the PDF footer" (T287); upper case refused (T313); which of three exports answers is chance (T313)** |
| 5.14 | `/portfolio/verify` | Anonymous | "No matching export found. …"; "Enter the content hash to check."; never an error page |
| 5.15 | `/portfolio/progress` | Dr Molefe (Trainee) | her running record (flow 05's); nothing says it has ended |
| 5.16 | `/admin/trainees` → `/admin/trainees/edit` | Prof Mbatha | the last day opening on `D` with its help; the dialog naming `J+1y−1d`; the refusal. **The refusal at the page's top, the field unmarked; the ends in Save profile's row** |
| 5.17 | `/admin/trainees/edit` → `/admin/trainees` → `/admin/users` → `/admin/users/{UserId}` | Prof Mbatha | the result focused; the ended record read-only (T305); Completed and closed; the user page's no-role sentence (flow 12's). **"Edit" on the closed row; "Trainee profile", not her name** |
| 5.18 | none (the email) | System | flow 19's. **"Your committee has ratified your final entrustment decisions" (T311 item 4)** |
| 5.19 | `/portfolio/progress` → `/account/session-ended` → `/account/login` | Dr Molefe | flow 02's session ended |
| 5.20 | `/account/login` → `/account/login/submit` → `/portfolio/progress` | Dr Molefe (former) | the ended record (flow 05's R5). **Nothing links her certificates or her export** |
| 5.21 | `/` | Dr Molefe (former) | no "Acting as"; Home, My progress, My data rights. **"Your training record" names no end, links only My progress (T311)** |
| 5.22 | `/committee/my-reviews`, `/portfolio/authorisations`, `/msf/my-reports`, `/activities/new` → `/access-denied` | Dr Molefe (former) | access denied (flow 01's); New activity refused (D55). **My authorisations refused: her certificates unreachable (T311)** |
| 5.23 | `/portfolio/export` → `/portfolio/verify` | Dr Molefe (former) | a new file that says she completed; both verify. **Export not in her menu or Home (T311)** |
| 5.24 | `/activities/new` → `/activities/{ActivityId:int}` | Dr du Plessis (Trainee) | flow 03's: a Mini-CEX dated `D−1`, Requested |
| 5.25 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Naidoo | flow 04's: rated `3b`, Completed, "1 item" |
| 5.26 | `/portfolio/progress` → `/portfolio/progress/{EpaId:int}` | Dr du Plessis (Trainee) | flow 05's: counted, n |
| 5.27 | `/admin/trainees` → `/admin/trainees/edit` → `/admin/trainees` | Prof Mbatha | the Deactivate dialog naming `D−2`; the result; Inactive, "Left the programme"; "Withdrawn `D−2`" in Completed and closed; the credit taken back (T281); no mail |
| 5.28 | `/` → `/portfolio/progress` → `/activities/mine` | Dr du Plessis (former) | Home's card, My progress "Ended", the Mini-CEX credited None. **My activities reached only by address; Home names no end (T311)** |
| 5.29 | `/committee/reviews` → `/committee/reviews/new` → `/committee/reviews` | Mr Smit | flow 08's: only current trainees |
| 6.21 | `/portfolio/progress` → `/portfolio/progress/{EpaId:int}` | Dr Molefe (former) | **PAED-012 gone from her completed record while paused; 15 of 15 becomes 14 of 14 (T312)** |
| 6.40 | `/portfolio/progress` | Dr Molefe (former) | **a KGK-001 card added after she graduated, "0 of 1, 1 short" in each closed year (T312)** |
| 6.41 | `/portfolio/progress` → `/activities/mine` | Dr du Plessis (former) | still ended; the Mini-CEX credited None; **KGK-001 on his record too (T312)** |
| A.7.4 | `/` → `/portfolio/progress` | Dr Molefe (former), 390 px | Home and the record fit; no "Acting as". **Home names no end (T311)** |
| A.7.12 | `/account/login` → `/account/forgot-password` → `/portfolio/verify` | Anonymous, 390 px | the verify result fits, the ids wrapping |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Home and the role dashboards, §
Activities, § Portfolio and § Institution administration. The step captures are from the T376 replay (2026-10-26), on
the shell as built. The `states/` captures of Export, Verify and the profile's dialogs are from 2026-09-26, on the old
frame: use the step capture where there is one, and the old one for its body only.

| Page | State | Screenshot | Reached at |
|---|---|---|---|
| `/` | Former trainee, graduated | `act-5/5.21-1-molefe-home-graduate.png` (`states/home--no-role.png`) | Step 5.21 |
| `/` | Former trainee, withdrew | `act-5/5.28-1-duplessis-home-ended.png` | Step 5.28 |
| `/` | Former trainee, 390 px | `act-A/A.7.4-1-home-390.png`, `act-A/A.7.4-2-menu-open-390.png` (`states/home--narrow-former-trainee.png`) | Step A.7.4 |
| `/` | Neither a role nor a record ("No role assigned") | — | Not captured; no cast member |
| `/portfolio/export` | Own, before export | `states/export-portfolio--own.png` (old) | Step 5.9 |
| `/portfolio/export` | Exported | `act-5/5.9-1-molefe-exported.png` (`states/export-portfolio--exported.png`, old) | Step 5.9 |
| `/portfolio/export/{TraineeUserId}` | Staff, exported | `act-5/5.10-1-mbatha-staff-export.png`, `act-5/5.12-1-smit-reproduces-export.png` (`states/export-portfolio--staff.png`, old) | Steps 5.10, 5.12 |
| `/portfolio/export/{TraineeUserId}` | Refused | `states/export-portfolio--refused.png` (old) | Scratch (post-appendix), Dr Dlamini |
| `/portfolio/export` | A former trainee's own | `act-5/5.23-1-graduate-exported.png` | Step 5.23 |
| `/portfolio/export` | 390 px | `states/export-portfolio--narrow.png` (old) | Step 5.23 at 390 |
| `/portfolio/verify` | Blank | `states/verify-export--blank.png` (old) | Step 5.13 |
| `/portfolio/verify` | Verified | `act-5/5.13-1-verifier-genuine-verified.png` (`states/verify-export--verified.png`, old) | Step 5.13 |
| `/portfolio/verify` | Upper case, refused today | `act-5/5.13-2-verifier-uppercase-hash.png` | Step 5.13 |
| `/portfolio/verify` | No match | `act-5/5.14-1-verifier-tampered-no-match.png` (`states/verify-export--no-match.png`, old) | Step 5.14 |
| `/portfolio/verify` | Spaces only | `act-5/5.14-2-verifier-spaces-only.png` (`states/verify-export--empty.png`, old) | Step 5.14 |
| `/portfolio/verify` | The second file verified | `act-5/5.23-2-graduate-file-verified.png` | Step 5.23 |
| `/portfolio/verify` | Could not check | `states/verify-export--check-failed.png` (old) | A held read |
| `/portfolio/verify` | 390 px | `act-A/A.7.12-3-verify-390.png` (`states/verify-export--narrow.png`, old) | Step A.7.12 |
| `/admin/trainees/edit` | Active, the end controls | `act-5/5.16-2-mbatha-future-day-refused.png` (`states/trainee-profile-edit--active.png`, old) | Step 5.16 |
| `/admin/trainees/edit` | Mark complete dialog | `act-5/5.16-1-mbatha-complete-dialog-future-day.png` (`states/trainee-profile-edit--complete-dialog.png`, old) | Step 5.16 |
| `/admin/trainees/edit` | A future day refused | `act-5/5.16-2-mbatha-future-day-refused.png` (`states/trainee-profile-edit--future-refused.png`, old) | Step 5.16 |
| `/admin/trainees/edit` | Completed | `act-5/5.17-1-mbatha-marked-complete.png` (`states/trainee-profile-edit--completed.png`) | Step 5.17 |
| `/admin/trainees/edit` | Deactivate dialog | `act-5/5.27-1-mbatha-deactivate-dialog.png` (`states/trainee-profile-edit--deactivate-dialog.png`, old) | Step 5.27 |
| `/admin/trainees/edit` | Withdrawn | `act-5/5.27-2-mbatha-deactivated.png` (`states/trainee-profile-edit--deactivated.png`) | Step 5.27 |
| `/admin/trainees/edit` | Unknown id | `states/trainee-profile-edit--not-found.png` (old) | Typed `?id=999999` |
| `/admin/trainees/edit` | 390 px | `states/trainee-profile-edit--narrow.png` (old) | Step A.7.9 |
| `/admin/trainees` | Completed and closed, one | `act-5/5.17-2-mbatha-trainees-completed.png` | Step 5.17 |
| `/admin/trainees` | Completed and closed, two | `act-5/5.27-3-mbatha-trainees-closed.png` (`states/pending-trainees-list--closed.png`) | Step 5.27 |
| `/admin/users/{UserId}` | No role, completed (flow 12's) | `act-5/5.17-3-mbatha-user-no-roles.png` (`states/user-detail--no-roles.png`) | Step 5.17 |
| `/admin/users/{UserId}` | No role, withdrawn (flow 12's) | `act-5/5.27-4-mbatha-duplessis-user-no-role.png` | Step 5.27 |
| `/portfolio/progress` | Running, exit rule met | `act-5/5.15-1-molefe-progress-running.png` (`states/my-progress--exit-met.png`, old) | Step 5.15 |
| `/portfolio/progress` | Completed, read-only | `act-5/5.20-1-molefe-progress-completed.png` (`states/my-progress--completed.png`) | Step 5.20 |
| `/portfolio/progress/{EpaId:int}` | An EPA, completed | `act-5/5.20-2-molefe-epa-page-completed.png` | Step 5.20 |
| `/portfolio/progress` | Withdrawn, read-only | `act-5/5.28-2-duplessis-progress-withdrawn.png` (`states/my-progress--withdrawn.png`, old) | Step 5.28 |
| `/portfolio/progress` | 390 px, former trainee | `act-A/A.7.4-3-progress-390.png`, `act-A/A.7.4-4-epa-page-390.png` (`states/my-progress--narrow-former.png`, old) | Step A.7.4 |
| `/portfolio/progress` | During a pause (T312's evidence) | `act-6/6.21-1-molefe-progress.png`, `act-6/6.21-2-molefe-paed012-page-paused.png` | Step 6.21 |
| `/portfolio/progress` | After the rebuild (T312's evidence) | `act-6/6.40-1-molefe-progress.png`, `act-6/6.41-1-duplessis-progress.png` | Steps 6.40, 6.41 |
| `/activities/mine` | Credited None after leaving | `act-5/5.28-3-duplessis-my-activities-none.png` (`states/my-activities--credited-none.png`) | Step 5.28 |
| `/activities/{ActivityId:int}` | Counted towards nothing | `act-5/5.28-4-duplessis-activity-credits-nothing.png` (`states/activity-view--after-programme-end.png`) | Step 5.28 |
| `/access-denied` | My authorisations refused | `act-5/5.22-2-molefe-authorisations-denied.png` | Step 5.22 |
| `/access-denied` | My committee reviews, MSF reports, New activity refused | `act-5/5.22-1-molefe-my-reviews-denied.png`, `act-5/5.22-3-molefe-msf-reports-denied.png`, `act-5/5.22-5-molefe-new-activity-denied.png` | Step 5.22 |
| `/committee/reviews/new` | Current trainees only (flow 08's) | `act-5/5.29-1-smit-current-trainees-only.png` (`states/reviews-schedule--current-only.png`) | Step 5.29 |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- Anywhere: a load error on Export (none designed: no read before the press), on the profile (prints the exception's
  text over "Profile unavailable"), and on the former trainee's Home (none: it reads only her claims).
- Never built: the former trainee's Home naming her end (T311); My authorisations as a former trainee; any staff link to
  the export (T314); the export page naming whose (T314); a refusal before the form; Verify's not-a-hash answer and its
  several-exports answer (T313); the end controls apart from Save profile; a refusal under the last-day field; a View
  on a closed row; the closed list stacked or paged; an ended record frozen at its last day (T312).
- An account with neither a role nor a record ("No role assigned"): no cast member reaches it.
- `home--trainee-ended` (a withdrawn registrar keeping Trainee and its dashboard's ended line) is a state D54 retired;
  `TraineeDashboard`'s ended branch (`W/Dashboards/TraineeDashboard.razor:341–344`) is no longer reached by any cast
  member (inferred).

**Belongs to other flows:** flow 01: access denied, Home's frame, the no-role sidebar. Flow 02: session ended (5.19,
`act-5/5.19-1…`). Flow 03: New activity and the activity form (5.24, `act-5/5.24-*`). Flow 04: the inbox and rating
(5.25, `act-5/5.25-*`). Flow 05: the running record (5.15, 5.26, `act-5/5.26-*`). Flow 08: Schedule a review (5.29).
Flow 09: My committee reviews and My authorisations' own layout (5.8). Flow 11: Trainees' Waiting and Current lists,
Admit. Flow 12: the user page and Users (`act-5/5.10-0…`, `-0b…`, `act-5/5.17-3…` to `-5…`, `act-5/5.27-4…`). Flow 14:
the former trainee's data rights (A.1.6). Flow 18: the jobs list that names `portfolio-export-cleanup` (A.2.1, A.2.9).
Flow 19: the graduation email (5.18, `act-5/5.18-1…`). Flow 20: the PDF (5.11, `act-5/5.11-*`), the STAR certificate
(T310).

## 4. Attach

Paths are relative to `design/baseline/`. The twelve key screenshots were opened on 2026-10-27 or are read from their
step's Actual; none shows a password or a one-time link. The verify captures show a hash and user ids, which are
scenario data and the page's own design. In every tall capture at 1280 the sidebar's colour stops at 800 px: an
artefact of the capture.

**Key screenshots (attach these first, with § 1):**
1. `act-5/5.21-1-molefe-home-graduate.png`: the former trainee's one card, no end named, one link
2. `act-5/5.22-2-molefe-authorisations-denied.png`: her STAR certificates refused
3. `act-5/5.10-1-mbatha-staff-export.png`: the staff export by address, naming nobody, nothing lit
4. `act-5/5.13-1-verifier-genuine-verified.png`: Verify's answer, UTC, ids, an <h3> card
5. `act-5/5.13-2-verifier-uppercase-hash.png`: the genuine hash in upper case, "No matching export found"
6. `act-5/5.16-1-mbatha-complete-dialog-future-day.png`: the Mark complete dialog over the form
7. `act-5/5.16-2-mbatha-future-day-refused.png`: the refusal at the page's top; four buttons in one row
8. `act-5/5.17-1-mbatha-marked-complete.png`: the ended record, "Trainee profile", Back to trainees
9. `act-5/5.27-1-mbatha-deactivate-dialog.png`: the Deactivate dialog
10. `act-5/5.27-3-mbatha-trainees-closed.png`: Completed and closed with Edit on each ended row
11. `act-5/5.20-1-molefe-progress-completed.png`: the completed record (flow 05's), no link to certificates or export
12. `act-A/A.7.4-1-home-390.png`: the former trainee's Home at 390

**Home and the menu:** `act-5/5.28-1-duplessis-home-ended.png`, `act-A/A.7.4-2-menu-open-390.png`,
`act-5/5.22-1-molefe-my-reviews-denied.png`, `act-5/5.22-3-molefe-msf-reports-denied.png`,
`act-5/5.22-5-molefe-new-activity-denied.png`.

**Export:** `act-5/5.9-1-molefe-exported.png`, `act-5/5.12-1-smit-reproduces-export.png`,
`act-5/5.23-1-graduate-exported.png`, and (old frame) `states/export-portfolio--own.png`,
`states/export-portfolio--refused.png`, `states/export-portfolio--narrow.png`.

**Verify:** `act-5/5.14-1-verifier-tampered-no-match.png`, `act-5/5.14-2-verifier-spaces-only.png`,
`act-5/5.23-2-graduate-file-verified.png`, `act-A/A.7.12-3-verify-390.png`, and (old frame)
`states/verify-export--blank.png`, `states/verify-export--check-failed.png`.

**Ending a programme:** `act-5/5.27-2-mbatha-deactivated.png`, `act-5/5.17-2-mbatha-trainees-completed.png`,
`act-5/5.17-3-mbatha-user-no-roles.png`, `act-5/5.27-4-mbatha-duplessis-user-no-role.png`, and (old frame)
`states/trainee-profile-edit--not-found.png`, `states/trainee-profile-edit--narrow.png`.

**The record:** `act-5/5.15-1-molefe-progress-running.png`, `act-5/5.20-2-molefe-epa-page-completed.png`,
`act-5/5.28-2-duplessis-progress-withdrawn.png`, `act-5/5.28-3-duplessis-my-activities-none.png`,
`act-5/5.28-4-duplessis-activity-credits-nothing.png`, `act-A/A.7.4-3-progress-390.png`,
`act-A/A.7.4-4-epa-page-390.png`, and, as T312's evidence only, never a target, `act-6/6.21-1-molefe-progress.png`,
`act-6/6.21-2-molefe-paed012-page-paused.png`, `act-6/6.40-1-molefe-progress.png`, `act-6/6.41-1-duplessis-progress.png`.

That is 46 files.

**Do not attach:** `states/home--trainee-ended.png` (2026-10-04, a state D54 retired) and
`act-A/A.4.6-4-my-authorisations-ended.png` (2026-10-13, Dr du Plessis still acting as Trainee before D54: a stale
capture; A.4.6's other captures keep their old names but show the former trainee's Home, flow 02's step); the old-frame `states/` captures a step capture replaces (`home--no-role`, `--narrow-former-trainee`;
`export-portfolio--exported`, `--staff`; `verify-export--verified`, `--no-match`, `--empty`, `--narrow`;
`trainee-profile-edit--active`, `--complete-dialog`, `--future-refused`, `--completed`, `--deactivate-dialog`,
`--deactivated`; `pending-trainees-list--closed`; `user-detail--no-roles`; `my-progress--exit-met`, `--completed`,
`--withdrawn`, `--narrow-former`; `my-activities--credited-none`; `activity-view--after-programme-end`;
`reviews-schedule--current-only`); the PDF and email captures (`act-5/5.11-*`, `act-5/5.18-1…`: flows 20 and 19's);
the other flows' step captures (`act-5/5.10-0…`, `-0b…`, `act-5/5.17-4…`, `-5…`, `act-5/5.19-1…`, `act-5/5.22-4…`,
`act-5/5.24-*`, `act-5/5.25-*`, `act-5/5.26-*`, `act-5/5.29-1…`, `act-A/A.7.12-1…`, `-2…`).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `82b108a6`.
"Observed" is seen in a capture or a replay's Actual; "code" is read in the source and not replayed; "inferred" is
neither.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T311** (P2, queued; its Home, menu, My authorisations and email items) | **Home:** the no-role branch draws "Your training record" for a holder of the claim (`W/Home.razor:57–79`), naming no end and linking only My progress (observed 5.21, 5.28, A.7.4, A.1.6, A.4.6); T311's root cause about a withdrawn registrar keeping Trainee is void since D54. `TraineeHomeWords.Ended` already words the ended line. **Menu:** the personal links are My progress and My data rights (`Nav/NavItems.cs:212–214`); Export portfolio is a Trainee item only (`:180`), though the page admits her (`P/ExportPortfolio.razor:3`, observed 5.23). **My authorisations** is `Roles = "Trainee"` (`P/MyAuthorisations.razor:2`; observed 5.22), though its queries admit her as herself; the EPA page hides "Open My authorisations" from her (`P/EpaProgress.razor:146`, `:298`; DESIGN.md:3652). **The email** (`src/Wombat.Application/Common/Email/Templates/GraduationEmail.cs:15`, `:24`) claims a ratification Mark complete never checks (observed 5.18; flow 19's output). | `act-5/5.21-1…`, `act-5/5.22-2…`, `act-A/A.7.4-1…` |
| **T314** (P3, queued) | A fixed header (`P/ExportPortfolio.razor:10`); the staff route resolves no name (`:62–69`); a refusal only after the press, under a drawn form (`:101–104`; observed old frame, `export-portfolio--refused`); no page links `/portfolio/export/{id}` (observed 5.10, 5.12); nothing lit, trail Home › Export portfolio (observed 5.10). The command's one refusal (`A/Reporting/ExportPortfolio.cs:35`) stays. | `act-5/5.10-1…`, `states/export-portfolio--refused.png` |
| **T313** (P2, queued) and **T287**'s verify item (P3, queued) | An exact, case-sensitive match with no order (`A/Reporting/VerifyExport.cs:26–30`; observed 5.13: upper case "No matching export found"); the page sends `Hash.Trim()` as typed (`P/VerifyExport.razor:103`); no format check, so a 12-digit prefix reads "No matching export found"; the 90-day cleanup deletes the records (`src/Wombat.Infrastructure/Scheduling/Jobs/PortfolioExportCleanupJob.cs:27–39`; code, A.2.9's job list); the placeholder "Enter the hash from the PDF footer" (`P/VerifyExport.razor:27`; observed 5.13) for a PDF that prints none (it cannot: the hash is of the whole file). Removing the job changes A.1/A.2's job lists (flow 18's steps A.2.1, A.2.9). | `act-5/5.13-1…`, `-2…` |
| **T312** (P2, queued; lands first, Q10) | An ended record reads the live catalogue: a pause drops PAED-012 from her completed record and her standing (observed 6.21: 14 of 14); KGK-001 added after she graduated appears with a shortfall in each closed year and "1 with no decision" (observed 6.40, 6.41); the PDF reads the same. A data rule across the progress readers, the standing and the PDF, not a screen: the design draws its outcome. | `act-6/6.21-1…`, `act-6/6.40-1…` |
| **T325** (A9; the export and verify half) | Export's defaults use the server's `DateTime.Today` (`P/ExportPortfolio.razor:55–56`; observed 5.9's From 2025-10-26, correct only because the server's date agreed); Verify prints "… on 2026-10-26 20:51 UTC" (`P/VerifyExport.razor:57`; observed 5.13). The profile's last day already defaults to the South African today (`Tr/TraineeProfileEdit.razor:459`). | `act-5/5.13-1…` |
| **T329 / T272's family** (code) | Export prints `exception.Message` for any failure but a refusal (`P/ExportPortfolio.razor:105–108`) and has no load state. The profile prints the exception's text for a failed load, save or end (`Tr/TraineeProfileEdit.razor:268`, `:322`, `:400`), and its `StatePanel` passes `LoadError="@(null)"` (`:36`), so a failed load reads the text over "Profile unavailable" with no Try again. | `states/trainee-profile-edit--not-found.png` |
| **T280** (headings, captions; code) | Verify's result is an `<h3>` "Export verified" under the `<h1>` (`P/VerifyExport.razor:56`); the profile's cards are `<h3>` (`Tr/TraineeProfileEdit.razor:41`, `:56`); the closed list's caption "Completed and closed trainee profiles" repeats its `<h2>` (`Tr/PendingTraineesList.razor:220–221`). Verify carries four inline `style`s (`P/VerifyExport.razor:41`, `:47`, `:55`, `:69`), against DESIGN.md's no-inline-style rule. | `act-5/5.13-1…` |
| **T323** (A3; the closed list) | Completed and closed has no `StackWide` (`Tr/PendingTraineesList.razor:221`), so at 390 it does not stack as Current trainees does (inferred; A.7.9 checked Current only). | `act-5/5.27-3…` |
| **New: the closed row's Edit** (observed; code) | Each ended profile offers "Edit", named "Edit ‹name›" (`Tr/PendingTraineesList.razor:237`), opening a record that cannot be edited (T305); its columns (Curriculum, Sub-speciality, Outcome) differ from Current trainees' (Curriculum and sub-speciality, Programme start, Expected completion); no pager (inferred: a closed list only grows). A11. Q8. | `act-5/5.27-3…` |
| **New: the profile's end controls** (observed; code) | The last day sits in the profile's form, and Deactivate and Mark complete in Save profile's row (Cancel, Deactivate, Mark complete, Save profile, `Tr/TraineeProfileEdit.razor:105–129`; observed 5.16): two irreversible ends beside an ordinary save. A future day's refusal is an alert at the page's top, the field named by `aria-describedby` but not marked invalid (`:25–28`, `:110–112`; observed `5.16-2`). The page is titled "Trainee profile" (`:13`), never the person; "Back to trainees" repeats the trail (`:15`), which flow 12 dropped on the user page; Email and Name are bold run-in labels (observed `5.17-1`). Q7. | `act-5/5.16-2…`, `act-5/5.17-1…` |
| **New: the former trainee's pages reached only by address** (observed) | My activities admits her (`W/Activities/MyActivities.razor:2`, bare `[Authorize]`) and lists her own (observed 5.28, 6.41), but neither her menu nor Home links it; nor Export portfolio (5.23). Q2. | `act-5/5.28-3…` |
| **New: the staff export's candidates** (code) | A Coordinator cannot open the trainee profile (`AdministratorOrInstitutionalAdmin`), so T314's two links leave Mr Smit only the committee review page (`W/CommitteeDecisions/ReviewDetail.razor:2`), and a review exists only once scheduled. The registrar's programme page (`W/Programme/ProgrammeTraineeDetail.razor:2`, flow 06's) admits the Coordinator, the committee and the speciality admins and reads an ended profile (`A/Programme/Trainees/GetProgrammeTrainee.cs:98–109`), but not the Institutional admin. Q4. | — |
| **T166's gating** (operator decision, EPA-PROGRAMME § 4) | Mark complete checks no ratified Graduate decision; the end section shows none either. Q13. | `act-5/5.16-1…` |

**Dropped from the old brief (done since, or decided):**
- T305 (`95d4e950`, `a27983fd`): an ended profile is read-only, its details as text under "This programme has ended, so
  its record is archived and cannot be changed.", no inputs, no Save profile (observed 5.17, 5.27). A rule now; T305's
  open question (no correction path, STATE § Open questions) is Q14.
- D54 (T290, `f3471780`, `78e95df6`): a withdrawal takes the Trainee role away, as a completion does. The old brief's
  "Dr du Plessis keeps the Trainee role", his trainee dashboard's ended line and "Deactivate (a withdrawal, which keeps
  it)" are void; both are former trainees with the same Home (observed 5.28, A.4.6).
- D55 (`fb6e465e`; T153's create half): only a registrar whose programme is running files; New activity refuses a
  former trainee (observed 5.22) and the dashboard offers no filing. The old brief's Q2 is answered. T153's remaining
  notes (T113's) are not this flow's.
- T281 (done): an encounter after the last day credits nothing and is taken back in the end's save (observed 5.27, 5.28).
- T252 (done) and flow 05 (`b020c942`, R5): My progress and the EPA page admit the record and draw the ended periods,
  ISO dates, no bars (observed 5.20, 5.28, A.7.4).
- T297 (done): dashboard states by label; T303 (done): Add role never offers Trainee; flow 12's S2: the user page's
  no-role sentence (observed 5.17, 5.27).
- T238 (done): Schedule a review offers only current trainees (observed 5.29).
- T310 (P3, queued): the STAR certificate's issuing institution is a PDF's fault: flow 20's, not drawn here.
- The old brief's LAYOUT line (the shell is flow 01's) and its Q2 (filing after the end, D55).

## 6. Questions for the operator

Each with a recommendation. The canvas answers 1, 2 and 4–9 in its variations; 3 and 10–16 are the operator's.

1. **Q1. The former trainee's Home** (5.21, 5.28, A.7.4). *Recommendation:* one emphasised card headed by the end
   ("You completed your programme on 2026-10-26." / "Your programme ended on 2026-10-24."), with "Your record is kept,
   read-only.", then four links named for what they hold: My progress, Your STAR certificates (15; only when she holds
   any), Your activities, Export your portfolio. No header action, no subtitle (flow 01's no-role frame). A former
   trainee who holds a staff role keeps that role's Home.
2. **Q2. Her menu and links.** *Recommendation:* the personal links become My progress, Export portfolio and My data
   rights for a holder of the record (PROPOSED: `NavItems.PersonalFor`); My authorisations and My activities are reached
   from Home and from My progress (a "Your record" line under its notice), owned by My progress, as My authorisations is
   for a Trainee. The EPA page's "Open My authorisations" shows to her.
3. **Q3. (The operator's) My authorisations, MSF reports and My committee reviews.** *Recommendation:* My authorisations
   admits the record (`TraineeOrFormerTrainee`, T311 item 2; its queries already admit her); MSF reports and My
   committee reviews stay role-only (DESIGN.md:236), their content being on the PDF.
4. **Q4. The staff export's entry points.** *Recommendation:* a header action "Export Lerato Molefe's portfolio" on the
   trainee profile (active and ended), on the committee review page (for the trainee under review), and on the
   registrar's programme page; each offered only where `MayReadAsync` admits the caller; not on the user page (flow
   12's, and Users is not a trainee list).
5. **Q5. Export.** *Recommendation:* "Export your portfolio" for her, "Export Lerato Molefe's portfolio" for staff
   (the trail from the page it was opened from); the name read before the form by a query gated as the command, one
   refusal with no form; From defaults to the programme start and To to today (South African) or the last day for an
   ended record (PROPOSED; 5.9's "last twelve months" changes); the result under the form, focused, naming the file.
6. **Q6. Verify.** *Recommendation:* the field's help "The SHA-256 of the PDF file: 64 characters, 0–9 and a–f, in
   either case. Compute it with sha256sum, certutil -hashfile <file> SHA256 or Get-FileHash."; verified: "Wombat
   produced this PDF. It was first generated on 2026-10-26 22:51 SAST and has been exported 3 times, most recently on
   …" with Trainee and Exported first by as ids, the filter and the file name, as an `<h2>` section; not a hash: "That is
   not a SHA-256 hash: it has 64 characters, 0–9 and a–f. The file name carries only the first 12." (PROPOSED words);
   SAST with the zone (A9), not UTC.
7. **Q7. The end of a programme.** *Recommendation:* the page titled by the person ("Lerato Molefe's programme"),
   Back to trainees gone; the profile's fields with Save profile, then an `<h2>` section "End this programme" with the
   last day (help shortened to the two ends' meaning) and Mark complete and Deactivate… as outline buttons, each in its
   ConfirmDialog; the refusal under the field, focused, the typed day kept; the result at the ended record's top,
   focused; the ended record a details list with the outcome first.
8. **Q8. Completed and closed.** *Recommendation:* Name, Curriculum and sub-speciality, Programme start, Outcome
   ("Completed 2026-10-26" / "Withdrawn 2026-10-24"), one View named "View Lerato Molefe's record" (PROPOSED words);
   newest end first; StackWide; a pager at 50 (flow 08's); a caption that does not repeat the heading.
9. **Q9. The ended record pages.** *Recommendation:* My progress's notice gains the record's links (STAR certificates,
   Export your portfolio); nothing else of flow 05's page changes; My activities shows no Log an activity (built) and
   the after-the-end row "Credit None" (built).
10. **Q10. (The operator's) T312.** *Recommendation:* land T312 first as its own task (P2, a data rule across the
    readers and the PDF), freezing the list at the last day; 6.21's and 6.40's Expects change with it; the design
    draws the frozen record ("PAED-012 … (no longer in use)" kept; no KGK-001).
11. **Q11. (The operator's) Verifying a STAR certificate** (B10). *Recommendation:* leave it out; the certificate is
    in the portfolio PDF, which verifies.
12. **Q12. (The operator's) The graduation email.** *Recommendation:* fold T311 item 4 into this build (drop the
    ratification sentence, link My progress); flow 19 keeps the email's design.
13. **Q13. (The operator's) Mark complete and the committee's decision.** *Recommendation:* the end section reads the
    last ratified decision ("Last committee decision: Graduate (programme complete), ratified 2026-10-26", DATA,
    PROPOSED), and gates nothing (T166's gating stays undecided).
14. **Q14. (The operator's) Correcting an archived record** (T305's open question). *Recommendation:* none drawn here;
    a correction goes through data rights (flow 14) until decided.
15. **Q15. (The operator's) A second programme** (T374). *Recommendation:* out of this flow; the closed row offers no
    re-admission.
16. **Q16. (The operator's) What the build folds in.** *Recommendation:* land T312 first as its own task; fold T311
    (Home, menu, My authorisations, the EPA page's link, the email's sentence), T314, T313 with T287's verify item (the
    cleanup job removed, A.2.1/A.2.9's Expects changed), and the T325, T329, T280 and T323 halves of these pages, with
    the closed list's View and the profile's end section. T310 (the PDF) stays flow 20's; T287's sign-in items and
    T166 stay their own.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps are 5.9–5.29, 6.21, 6.40, 6.41, A.7.4 and A.7.12;
  § 9): every Expect must hold, the refusals' exact words included. **Rewrite the steps that change:** 5.9 (the
  defaults, "your portfolio"); 5.10 and 5.12 (reached from her pages, named); 5.13 and 5.14 (the help, the answer's
  words, upper case accepted, the count); 5.16, 5.17 and 5.27 (the end section, the title, the refusal under the field,
  View on the closed row); 5.18 (the email, if Q12); 5.20, 5.21, 5.22, 5.23 and 5.28 (Home, menu, My authorisations
  admitted, Export in the menu); 6.21, 6.40 and 6.41 (once T312 lands); A.7.4; search the whole runbook (BRIEF.md § 9
  item 7) for the words that change, including 5.8, A.1.6, A.2.1, A.2.9 and A.4.6, which cross these pages. A new page
  or route needs a step (`Scenario/ScenarioRunbookTests`). A new step takes a letter when its number is taken (§ 11,
  flow 06). Record passwords only in `pwd_DO_NOT_COMMIT.txt`.
- **Checks the design adds:** 5.21, Home names the end and its four links each open a page that admits her; 5.22, My
  authorisations admits her and downloads PAED-001's certificate; 5.10, the export reached from Dr Molefe's profile and
  named for her; 5.12, from her review; 5.13, the upper-case hash verifies, "exported 3 times", SAST; a 12-digit prefix
  gets the format message; 5.16, the refusal under the field, focused, the day kept; the closed list's View; 390 and
  768 with nothing scrolling sideways; no exception's text on any failed read.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include:
  - `Portfolio/VerifyExportPageTests`, `Hosting/VerifyExportPageHostingTests` (Verify stays static), the export page's
    and the profile page's own tests, with new ones for the named header, the refusal before the form, the end section
    and the closed list's View; the handler tests T313 and T314 bring; T311's Home test (the claim, the end, the links,
    "No role assigned" for neither);
  - `Navigation/NavMenuAuthorizationTests` (the record's personal links), `DashboardLinkAuthorizationTests` (every link
    on the former trainee's Home opens a page that admits her), `ActiveNavItemTests`; `Hosting/PageTitleTests`;
  - `Accessibility/RowNamesTests`, `ActionFocusTests`, `AccessibleNamesTests`, `IdReferencesTests`;
  - `Design/*`, including `NarrowLayoutTests`, `DefinedClassTests`, `ContrastTests`; the scenario guard in `Scenario/`.
- **Re-capture** the states in § 3 (the old-frame ones, and the scratch ones on scratch copies, as `states.md` says)
  and the steps' own captures, keeping their names; `check_baseline_paths.py` at missing 0. Compare them with the
  chosen artboards.
- **Browser check** at 1280, 768 and 390 px as Dr Molefe (trainee, then former trainee), Dr du Plessis (former), Prof
  Mbatha, Mr Smit, and signed out on Verify.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only), as flow 12's replay
left them (T376, 2026-10-26; the Gaps it recorded are 5.10's and 5.12's T314, 5.13's T313 and T287, 5.21's, 5.22's,
5.23's and A.7.4's T311, and 6.21's and 6.40's T312). `D` is the replay day and `J` the latest 15 January on or before
it (README § How to play).

```text
Step 5.9 — Dr Molefe exports her portfolio
Role: Trainee — Dr Lerato Molefe
Route: /portfolio/export
Do: Open Export portfolio from the menu. Set the from date to her programme start, `J−3y`, and the to date to `D`, and
  export the PDF.
Expect: The page opens with the last twelve months filled in. After the export it says "Portfolio exported
  successfully. The download should start automatically.", and a file named `portfolio-<12 hex digits>.pdf` downloads.
  Record the file name: Steps 5.10 and 5.12 must produce the same one.

Step 5.10 — Prof Mbatha exports Dr Molefe's portfolio by its address
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId} → /portfolio/export/{TraineeUserId}
Do: Find Dr Molefe in Users (type "Molefe" in Name or email and press Show) and open her. Take her user id from the
  path of the page's address (before any `?`: Manage adds `?back=`), then type the export address with that id. Set the dates to `J−3y` and `D`, and export.
Expect: The export succeeds as in Step 5.9. The downloaded file has the same name as Dr Molefe's and is identical to it
  byte for byte. Compare the two with `fc /b`, or by their SHA-256.

Step 5.11 — Prof Mbatha reads the portfolio
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: n/a
Do: Open the PDF from Step 5.10 and read it through.
Expect: Every page carries the same header and footer:
  - header: Kgosi Kgari Teaching Hospital, "Portfolio Export", Lerato Molefe, Paediatric EPA Curriculum, "Paediatrics —
    Paediatrics" (speciality and sub-speciality), and "Period: `J−3y` to `D`";
  - footer: "Page n of m | Generated by Wombat", with no generation time.
  The sections follow in this order:
  - a Summary, with activities by type and the number of committee reviews;
  - "Statements of Awarded Responsibility (STARs)", reading "15 active entrustment decisions on record." and giving each
    EPA's rung by its label;
  - "Progress per EPA", read on `D` in training year 4;
  - "Committee Decisions", listing each ratified review. The final one reads Pre-graduation review, Ratified, "Graduate
    (programme complete)", with its rationale and the four members present by name;
  - her activities by type, with rungs shown by their labels and people by their names;
  - the MSF campaign;
  - "Appendix — Audit Trail".

Step 5.12 — Mr Smit reproduces the export
Role: Coordinator — Mr Pieter Smit
Route: /portfolio/export/{TraineeUserId}
Do: Type the export address with Dr Molefe's user id, which Prof Mbatha sends him. Set the same two dates, and export.
Expect: The export is allowed, because a coordinator of her institution oversees her (T079, T101). The file has the
  same name as those from Steps 5.9 and 5.10, and is identical to both byte for byte.

Step 5.13 — A verifier checks the genuine PDF
Role: Anonymous — a credentialing officer at Dr Molefe's new hospital
Route: /portfolio/verify
Do: In a private window, without signing in, open the verification page. Compute the SHA-256 of the PDF that Dr Molefe
  sent (`certutil -hashfile <file> SHA256`, or `sha256sum`, both of which print it in lower case), enter it and press
  Verify. Do the same for Mr Smit's copy.
Expect: The page works signed out, as a static page, under the signed-out bar: the brand and Sign in, and no menu.
  Verifying loads the page's own address with the hash in it. The
  page reads "Export verified" and shows:
  - when Wombat generated the file, in UTC;
  - the trainee and the exporter as user ids, never as names (DESIGN: the anonymous page must not disclose a name);
  - "Filter: `J−3y` to `D`";
  - the file name that all three downloads carry.
  Mr Smit's copy has the same SHA-256 and gets the same answer.

Step 5.14 — The verifier checks a tampered copy
Role: Anonymous — a credentialing officer at Dr Molefe's new hospital
Route: /portfolio/verify
Do: Change one byte in a copy of the PDF, compute the copy's SHA-256 and verify it. Then press Verify with only spaces
  in the field.
Expect: The tampered copy gets "No matching export found. This hash does not correspond to any PDF generated by this
  system." The press with only spaces gets "Enter the content hash to check." Neither leads to an error page.

Step 5.15 — Dr Molefe leaves My progress open
Role: Trainee — Dr Lerato Molefe
Route: /account/login → /account/login/submit → / → /portfolio/progress
Do: In her own browser, sign in and open My progress. Leave the tab open through Step 5.19.
Expect: The page shows her programme as still running:
  - "This period", with "1 of 10" over "EPAs met this semester" and "0 of 5" over "EPAs met in 2026";
  - Your EPAs, each EPA's count for the window containing `D` with its bar, and every STAR "At or above", at 5 or 4;
  - "Entrustment against Annexure A", "15 at or above · 0 below · 0 with no decision, of 15 EPAs", with the exit rule
    met as in Step 5.6: "15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of 6). Every
    EPA is at its exit level.";
  - the period before each window, and her rating trajectories, on each EPA's page (T355), not on My progress.
  Nothing says that her programme has ended.

Step 5.16 — Prof Mbatha tries to record the story's graduation day
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit
Do: Open Dr Molefe's profile from Current trainees (Edit). Set the last day in the programme to the story's graduation day,
  `J+1y−1d`. Press Mark complete and confirm.
Expect: The last-day field opens on `D`. Among the rest, its help says that:
  - Mark complete records the graduation day, archives the profile and removes the Trainee role;
  - the day cannot be after today;
  - the day cannot be changed afterwards.
  The confirmation names `J+1y−1d` (as yyyy-MM-dd) as her graduation day. It says that the Trainee role will be
  removed, a graduation email will be sent, encounters after that day will count towards nothing, and none of this can
  be undone. Once
  confirmed, the request is refused with "The completion date cannot be after today (`D`)." (`D` shown as yyyy-MM-dd),
  which is read with the field. The profile is still Active.

Step 5.17 — Prof Mbatha marks Dr Molefe's programme complete on `D`
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees/edit → /admin/trainees → /admin/users → /admin/users/{UserId}
Do: Set the last day to `D`, press Mark complete and confirm. Then go back to Trainees, and open Dr Molefe in Users.
  On her page, press Add a role…, read it and press Cancel.
Expect: The page says "Trainee marked complete. The Trainee role has been removed and a graduation email sent."
  - The summary reads Status Completed, Completed `D`.
  - The last-day field, Deactivate and Mark complete are gone. The page's subtitle reads "The record of a programme
    that has ended.", and the profile's details read as a list under "This programme has ended, so its record is
    archived and cannot be changed.": Curriculum Paediatric EPA Curriculum (11.1), Programme start date `J−3y`,
    Expected completion date `J+1y−1d`. There are no inputs and no Save profile; Back to trainees stays (T305).
  - On Trainees she is no longer under Current trainees. She is listed under "Completed and closed" with the outcome
    "Completed `D`".
  - Her user page's Roles and scope says "**Lerato Molefe holds no role: they completed the programme on `D`.** They can
    still sign in to read their training record (My progress) and their data rights." (T376, S2, Q13; D54), and its Add
    a role… offers the six staff roles and no Trainee (T303). On Users her Roles cell reads "No role" over "A completed
    or withdrawn registrar keeps an account and its records."
  - No credit is taken back, because nothing of hers is observed after `D` (T281; Step 5.27 shows a take-back).

Step 5.18 — Dr Molefe is emailed
Role: System — the graduation email sent by Mark complete
Route: n/a
Do: Read the application log for the mail sent in Step 5.17.
Expect: There is one email to `molefe@kgk.wombat.local` with the subject "Congratulations on completing Paediatric EPA
  Curriculum". It congratulates her on completing the programme as of `D`, and says that her committee has ratified
  her final entrustment decisions and that her portfolio is available in Wombat.

Step 5.19 — Dr Molefe's open session ends
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress → /account/session-ended → /account/login
Do: Go back to the tab left open at Step 5.15 and wait up to a minute.
Expect: Removing her role changed her account's security stamp. The session check, which runs once a minute, ends the
  session: the tab reloads to the sign-in page, which says "Your session has ended. Sign in again.", an information
  notice. Nothing she had open still acts as a Trainee.

Step 5.20 — Dr Molefe signs in again and reads her record
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /account/login → /account/login/submit → /portfolio/progress
Do: Sign in from the page that Step 5.19 left her on. She is returned to My progress. Read it.
Expect: Her trainee record admits her to the page (T252, `TraineeOrFormerTrainee`).
  - An information notice reads "You completed your programme on `D`. This page is your record of it and is read-only:
    no target applies to you any more.", followed by D49's rule. Every date on the page is ISO, `YYYY-MM-DD` (T355, D1).
  - A "Your programme" card gives her start date, Completed `D`, and training year 4 as the year the programme ended.
  - Each EPA's card, its code a link to its page, lists its periods, newest first. The period holding `D` reads "no
    target (your programme ended part-way through) · n recorded", and the earlier periods read as met or short.
  - There are no progress bars.
  - The standing reads the year her programme ended in, "You completed your programme on `D`, in training year 4 …",
    with the exit rule met.
  - Her trajectories are on each EPA's page, which reads as her record does: every period listed, and no "n more by"
    (T355).

Step 5.21 — What her home page and menu offer a graduate
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /
Do: Open Home and read the menu.
Expect: The sidebar has no "Acting as" head, since she holds no role, and the menu offers Home, then My progress and My
  data rights under the rule, and nothing else (DESIGN § The NavMenu, D8, T252). The top bar names her, "Lerato Molefe",
  with Sign out. Home says "You completed your programme on `D`, so no target applies to you any more." and points her
  to My progress, read-only, as the trainee dashboard's Your targets card does for an ended programme (T252 As built:
  "Home says the same"; dates ISO, T355).

Step 5.22 — What a graduate can no longer open
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /committee/my-reviews → /access-denied → /portfolio/authorisations → /access-denied → /msf/my-reports → /access-denied → /activities/new → /access-denied
Do: Type the addresses of My committee reviews, My authorisations and MSF reports in turn. Then type the address of the
  new-activity page.
Expect: Each of the first three reads "You cannot open this page", "Your account holds no role that opens this page."
  and "If you need it for your work, ask your institution's Wombat administrator.", with Go to Home: those pages still
  require the Trainee role (DESIGN § The NavMenu).
  Nothing offers her an activity to file: there is no menu link and no dashboard action, and the new-activity page
  reads the same "You cannot open this page" when she types its address. Only a registrar whose programme is running
  files (D55); the create refuses anyone else too.

Step 5.23 — Dr Molefe exports her portfolio as a graduate
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/export → /portfolio/verify
Do: Type the export address, set the dates to `J−3y` and `D`, and export. Then verify the new file's SHA-256
  on the verification page.
Expect: The export page is not in her menu, but it admits her, because the portfolio is her own (`MayReadAsync`). The
  file has a new name. Its "Progress per EPA" now says "The trainee completed the programme on <`D`>", so the content
  differs from Steps 5.9 to 5.12. The new file verifies, with the same trainee, the same filter and its own
  file name. The earlier file still verifies too.

Step 5.24 — Dr du Plessis asks for a Mini-CEX on a shift after his post ended
Role: Trainee — Dr Pieter du Plessis
Route: /activities/new → /activities/new?type=mini_cex_cpsa → /activities/{ActivityId:int}
Do: File a Mini-CEX (Paediatrics) on PAED-002 with:
  - encounter date `D−1`;
  - Dr Naidoo as the assessor;
  - a clinical setting, a presenting problem and a case complexity.
  Submit it.
Expect: The EPA list offers PAED-002, because its tool list includes the Mini-CEX. There is no late-filing warning.
  Submit to David Naidoo reads "Submitted. It is now Requested. It is in David Naidoo's Activity inbox.", and the
  activity is in his inbox. No email is sent: Wombat mails nobody when an activity moves (Step 3.3).

Step 5.25 — Dr Naidoo rates it
Role: Assessor — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: He has acted as a Committee member since Step 4.5: choose Switch to Assessor in the sidebar, then open Dr du
  Plessis's Mini-CEX from Activity inbox and rate it `3b`, PAED-002's minimum in training year 2. Write what was done
  well, what to develop and the agreed plan, and complete it. Then choose Switch to Committee member in the sidebar.
Expect: Before the switch, his Committee member's Home carries, under the header, the line "1 activity waits for you
  in the Activity inbox: Mini-CEX (Paediatrics) · PAED-002 · `D−1`, from Pieter du Plessis, waiting less than a day.",
  with Open it (T350). After it, Home's "Waiting for you" is badged "1 waiting", and the inbox lists the Mini-CEX as
  "Mini-CEX (Paediatrics) · PAED-002 · `D−1`", from Pieter du Plessis, Requested. He rates it on the rung picker. After
  Complete the result reads "Completed. Nothing else waits for you.", with Go to Home. The activity reads Completed and
  is read-only to both of them. It credits PAED-002 in the semester that holds `D−1` (checked in Step 5.26), and My
  activities reads it credited "1 item". Nobody is emailed.

Step 5.26 — Dr du Plessis sees it counted
Role: Trainee — Dr Pieter du Plessis
Route: /portfolio/progress → /portfolio/progress/{EpaId:int}
Do: Open My progress, then PAED-002 from its index, and read its count for this period.
Expect: This period's count for PAED-002 includes the `D−1` Mini-CEX: its row reads "n of 3 this semester", and its page
  the same count, with "Last encounter `D−1`" and "Training year 2: level 3b, the minimum each encounter is judged
  against and your STAR's target." Record the count as n.

Step 5.27 — Prof Mbatha records his withdrawal
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Open Dr du Plessis's profile from Current trainees (Edit) and set the last day in the programme to `D−2`. Press Deactivate and confirm.
Expect: The confirmation names `D−2` as his last day. It says that the Trainee role is removed and he keeps his own
  record, that encounters after that day will count towards nothing, including any already counted, and that neither
  the day nor the profile can be changed back. After confirming:
  - the page says "Trainee profile deactivated. Their last day in the programme is recorded as `D−2`, and the Trainee
    role has been removed.";
  - the summary reads Status Inactive, "Left the programme: `D−2`", and the profile's details read-only, as on Step
    5.17, with no Save profile (T305);
  - on Trainees he is listed under "Completed and closed", with the outcome "Withdrawn `D−2`";
  - in the same save, the credit from the `D−1` Mini-CEX is taken back (T281);
  - the Trainee role is taken away, as a completion takes it (T290), and he is not emailed.

Step 5.28 — Dr du Plessis's record after he left
Role: Former trainee (no role; a trainee record) — Dr Pieter du Plessis
Route: /account/login → /account/login/submit → / → /portfolio/progress → /activities/mine
Do: Sign in, then read Home, My progress and My activities.
Expect: Each page records that his programme ended on `D−2`, every date ISO (T355, D1):
  - He holds no role (T290: withdrawal takes Trainee away, as completion does), so the phone bar shows no "Acting as"
    and the menu holds Home, My progress and My data rights. Home shows the former trainee's card, "Your training
    record", pointing to My progress, as Dr Molefe's does at Step 5.21 (T252; that it names no end day is T311).
  - My progress opens with "Your programme ended on `D−2`. This page is your record of it and is read-only…". Its "Your
    programme" card reads Ended `D−2`. PAED-002's period holding `D−2` reads "no target (your programme ended part-way
    through) · n−1 recorded".
  - My activities still lists the `D−1` Mini-CEX as Completed, its Credit now "None": the take-back's replay re-stamped
    its completion (`ProgrammeEndCredit`). The filing is kept, but it counts towards nothing.

Step 5.29 — Neither ended programme can be put before the panel
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews
Do: Press Schedule a review, choose the Paed Annual Review Panel and read the Trainee list. Then Cancel.
Expect: The list offers only Dr Dlamini, Dr Mahlangu and Dr Ndlovu, each with the training year. A graduate and a
  registrar who has withdrawn are not current trainees (T238). Cancel returns to Committee reviews; nothing is
  scheduled.

Step 6.21 — Dr Molefe's record during the pause
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress → /portfolio/progress/{EpaId:int}
Do: Read her record. Then open PAED-012's page by its address (`/portfolio/progress/{EpaId}`, PAED-012's id from Step
  6.15): her record does not link it while it is paused.
Expect: The record opens with its notice that she completed her programme and that the page is read-only.
  - It lists no PAED-012 card while PAED-012 is paused, as every progress page leaves out an EPA not in force (T158).
  - Her "Entrustment against Annexure A" panel has no PAED-012 row either: the standing reads only items in force.
  - No chart is on My progress (T355). PAED-012's page is headed "PAED-012 — … (no longer in use)", and its rating
    trajectory, headed "Rating trajectory (no longer in use)", still charts her ratings on it (T255).
  - PAED-006's card carries the title corrected in Step 6.14. Her other cards and periods are as Act 5 left them.

Step 6.40 — Dr Molefe's record after the rebuild
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: /portfolio/progress
Do: Read her record, card by card, against what she read in Act 5 (Step 5.20).
Expect:
  - Her fifteen national cards are back, PAED-012 among them, and each period reads what Act 5 recorded: the rebuild
    replays a completed profile's evidence as it was (Act 5's handoff).
  - PAED-006's card carries the corrected title. There is no PAED-016 card: her profile is on 11.1.
  - A KGK-001 card has appeared under "Once a year", counting none of her encounters: each closed year of her
    programme reads "0 of 1, 1 short". KGK's own item on 11.1 is read live, into a record that has ended.

Step 6.41 — Dr du Plessis's record after the rebuild
Role: Former trainee (no role; a trainee record) — Dr Pieter du Plessis
Route: /portfolio/progress → /activities/mine
Do: Read his record and his activities.
Expect:
  - My progress still opens with the notice that his programme ended on the day Act 5 recorded, "Your programme ended on
    <that day, as `YYYY-MM-DD`>. This page is your record of it and is read-only: …" (T355, D1).
  - PAED-002's period holding that day still reads "no target (your programme ended part-way through)", with the count
    Act 5 Step 5.28 read: the Mini-CEX observed after his last day still counts towards nothing (T281).
  - A KGK-001 card has appeared on his record too, as on Dr Molefe's.
  - My activities lists that Mini-CEX as Completed, its Credit None.

Step A.7.4 — Dr Molefe's record on her phone
Role: Former trainee (no role; a trainee record) — Dr Lerato Molefe
Route: / → /portfolio/progress
Do: At 390 px, sign in, read Home, and open My progress from the menu.
Expect: Home says that she completed her programme and points her to My progress, as at Step 5.21 (T252). The phone bar
  shows no "Acting as", since she holds no role, and the menu holds Home, My progress and My data rights, then her name
  and Sign out at its foot. My progress shows her read-only record of past periods (T252), its cards fitting the width;
  her charts are on each EPA's page, not on My progress (T355).

Step A.7.12 — The anonymous pages on a phone
Role: Anonymous — a verifier
Route: /account/login → /account/forgot-password → /portfolio/verify
Do: At 390 px, open the sign-in page and the forgot-password page. Then verify Dr Molefe's portfolio PDF from Act 5 by
  its hash.
Expect: The sign-in card fits the width. Every field, toggle, button and link on the sign-in and forgot pages is
  44 px tall; Sign in is full width; the h1 is 1.5rem. The verify page's result fits the width.
```
