# Flow 11 round 3: the text of every board

Generated from `round-3/project/R3-*.dc.html` (T373, build-contracts § 0): style, script, svg and helmet blocks stripped; headings, labels, aria-labels, aria-describedby reasons, captions and annotations kept. Board order is the canvas's. Design data: a caption or chip is never a shipped word.

######## BOARD R3-00-captures-1280.dc.html — R3-00 · the captures: all eleven read again
## Round 3 · the captures
All eleven arrived. Read again from C:\dev\Wombat\design\upload before drawing (2.26-1, 1.5-1, invitations-list--check-address, 2.8-1, 2.9-1, 1.9-1, 2.18-1, 2.28-4, 2.29-1, 2.29-3, A.7.9-4); each file's timestamp is the one round 2 read, so nothing in them changed.
Round 3 is a correction round: the boards are round 2's, renamed R3-, updated in place by the items F1–F10, M1–M11 and S1–S24. New boards: N06b (S10), N08a and N25 (S14), N24 and M12 (M1), N26 (S13), M04b (nit), M13 (S6), M14 (F1), M15 (F4), P07 (F3), P08 and P09 (S14). Dropped: M05 and P06 (F1). Round 1's and round 2's pages are untouched.
Marks: NEW PROPOSED DATA Illustration (now on the board's face as well as its title, F6) decided, T367. The blue ring (.canvas-focus) is the canvas's stand-in for the system's :focus-visible ring; no page draws a ring of its own.

######## BOARD R3-0-spec-2.dc.html — R3-0-spec (2 of 2): DESIGN.md, classes, mails, DATA
## R3-0-spec · flow 11 round 3 (2 of 2)
### DESIGN.md text
#### § The invitations list (round 2's text, with Last day)
The list is the page: Invitee (the row header, the address breaking after its @), Role and scope (the role's label over its scope; the institution or College too for the Administrator), Issued (the date over the time in SAST), Last day (the last South African calendar day the link works), Delivery, and the row's actions. Delivery is a state word with an icon: Being sent (clock, muted), Sent (circle-check, success), Not delivered and Check the address (circle-alert, warning, 600). Its explanation is a note row under its row (tr.has-note, then a second <tr> with td.history-note.history-note--warning across every column), and the row's Resend is described by it. Rows not delivered come first, newest first among them, then the rest newest first. A not-delivered row offers Resend before Revoke…; any other row Revoke… alone. An invitation that passes its last day unused moves to Expired, not used for 30 days on the same calendar, with Invite again. Nothing in the table holds a column to one line: Issued and Last day wrap, Delivery is .col-wrap, the actions .col-actions.
#### § Account / auth page (S15)
AuthLayout's one message slot becomes a message per field: a refusal sits under its field (.validation-message), the field marked and described by it, and the focus goes to the field. PasswordRules' refusal is the Password field's message, and the field is described by it and by the rules' list (G04). A refused confirmation empties both password fields, keeps the focus on Password, says "Type the password again in both fields.", and Password is described by the confirmation's message too (G05). The lockup and the purpose line head sign in and Register.
#### § Form system (S15, M11)
A required field's label ends in the required mark (span.required-mark, aria-hidden), its control carries aria-required="true", and "* marks a field you must fill in." stands above the form's first field; a group's legend adds a visually hidden "required". No "(optional)": a field's help says what leaving it empty does. A single possible value is read out as dl.form-group with dd.readonly-value. On Invite a person, a field the chosen role does not take is not drawn (never disabled there); elsewhere the form system's disabled rules stand. Resend's aria-disabled and the old link Alert text are gone with the OneTimeLink.
#### § ConfirmDialog (M8)
Below 641px a dialog's buttons are 44px and the row's width, stacked column-reverse: the action on top, the safe button last on screen and first in tab order. One rule in app.css: .dialog-card .form-actions .btn { min-height: 2.75rem } with .dialog-card .form-actions { flex-direction: column-reverse }; the top-layer dialog is outside any page wrapper, so the rule cannot be scoped to one. Flows 08's and 10's scoped copies fold into it.
#### § Page header and buttons (M9)
Below 641px every header action is 44px and the row's width (.header-container .actions-cell .btn), and every stand-alone button in a page column is 44px (.inv-page .btn).
#### § Table system (S16)
DataTable gains StackWide: it stacks below 900px with the explicit roles Stack gives. A note row is a second <tr> from the Row fragment. Row buttons are 44px from 900px down.
The Button system's and ConfirmDialog's restatements of round 2 are dropped (S15).
### Classes (S19)
Each round 2 class, reused or NEW
| Class
| Round 3
| tr.inv-has-note / tr.inv-note
| reuses tr.has-note / td.history-note (flow 03); NEW modifier .history-note--warning, because the explanation keeps the warning tint and stripe
| .consequence
| reuses .submit-check (SubmitCheck)
| .inv-section, .inv-section-head
| reuses .list-section, .list-section-title
| .admit-facts, .wait-facts
| reuses .detail-list
| .wait-steps
| reuses .account-steps
| .inv-role / .inv-scope
| reuses .person-cell (name over the scope)
| .field-label / .inv-readback
| reuses dl.form-group + dd.readonly-value (S18)
| .inv-radio*, the hand-sized legend
| reuses fieldset.form-group.choice-row, .form-check.review-type-choice, .review-type-help (S17)
| .result-block
| gone: ActionResult (.action-result) (S11)
| .inv-optional
| gone (F9)
| .reg-card-icon, .ty-value--wait, the auth card's three phone rules, the date-input font rule
| deleted (S19)
| .inv-when span
| kept: the date over the time (M6)
| .delivery, --sending, --sent, --warn
| NEW, because Delivery is a state word with an icon and no badge (Q15)
| .otl, .otl-row, .otl-help, .otl-said
| NEW, because the OneTimeLink has no system equivalent (S10)
| .inv-page, .inv-page--form, .inv-count
| NEW, because the four pages share a column of parts 24px apart and a plain-text count
| .wait-card, .wait-links
| NEW, because DashboardCard takes the control edge here (F8) and its links an arrow icon
| .ty-output
| NEW: the Training year <output>'s size
| .col-actions--single
| NEW, because .col-actions' 12rem made Current trainees scroll at 901px (M6)
| .skel-title, .skel-line, .skel-short, .loading-line
| NEW, because Skeleton's sizes were inline styles (S20)
| general rules below 641px
| NEW in app.css: .dialog-card .form-actions (M8), .header-container .actions-cell .btn (M9), .inv-page .btn (M9); from 900px .clinic-table--stack-wide .btn (S16). Flows 08's and 10's scoped copies (.committee-calendar, .msf-page) fold into them
### Mails (Q7, S12)
The invitation (S12). Subject: Your Wombat invitation. "You are invited to Wombat as Institutional admin at Kgosi Kgari Teaching Hospital." [the role's label, never its key; the scope in the page's words: "in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital", "at College of Paediatricians of South Africa"] "Register with this link; it works until 2026-11-02." [the link to /account/register/start?token=…] Register as InstitutionalAdmin … expires …
The admission (Q7, decided, T367). Subject: You are admitted in Wombat. "You have been admitted to the Paediatric EPA Curriculum (11.1), in training year 4." [or "Your programme starts on 2027-01-15."] "If you were signed in to Wombat, you have been signed out once: sign in again to log activities and follow your progress." [Sign in]. No other data. The admission's result claims no delivery.
### Words changed this round (beside the old)
"It works until 2026-11-02." It expires on 2026-11-02, in 14 days. (OneTimeLink, Register's note, G10, N17, N20)
Column "Last day" Expires
"Could not copy. Select the link and copy it yourself." NEW (N06b)
"Choose a role." NEW (N25)
"Nothing is adopted at Kgosi Kgari Teaching Hospital yet, so roles that need a speciality cannot be invited. Adopt a curriculum under Curriculum adoptions." NEW (N26)
"Left empty, it would be 2024-01-15, which has passed. Enter the expected completion." NEW (M15)
"Not admitted. Lerato Molefe already has an active trainee profile." This user already has an active trainee profile. (M09)
"Not admitted. Lerato Molefe's account is deactivated. Reactivate it under Users first." NEW (M13)
"This registrar belongs to no institution, so no curriculum can be offered." NEW (M04b)
"Last name: enter 100 characters or fewer." Enter your first name and last name, each of at most 100 characters, and a password. (G06)
"Loading the invitations." / "Loading what you can invite." / "Loading the trainees." / "Loading this registrar." NEW
"Training years turn every 365 days from the programme start." NEW (Admit's help, Current trainees' caption)
"Cancel" Back to invitations (Invite a person)
### Owner table and the accepted exceptions
/admin/invitations/new: Invitations lit (aria-current="true"), Home › Invitations › Invite a person.
/admin/trainees/admit: for the Institutional admin, Trainees lit (aria-current="true"), Home › Trainees › Admit ‹name›. For the Administrator, whose menu has no Trainees: nothing lit, the trail Home › Trainees › Admit ‹name› with an explicit crumb to /admin/trainees (M04b).
Inline links at 390 (19–21px, in running text: N15, N17, M04, M09, M13) are the accepted inline exception (WCAG 2.5.8's inline case), as flow 10.
### DATA the build must add
InvitationPreviewDto: the institution or College name, the speciality and sub-speciality names, ExpiresOn (the South African calendar date).
IssueInvitation: find-by-address and an unfiltered active-invitation lookup, with the typed refusal (M3).
ListExpiredUnusedInvitationsQuery, and the Invite again prefill.
The in-memory Not-delivered-first order.
Invitation.IssuedAsRole (F3; one migration).
Expiry on the South African calendar: the ProgrammeCalendar date, not the UTC date (F7).
The pending registrar's Home part: her institution, scope names, the curriculum adopted for her sub-speciality, the used invitation's issuer, IssuedAsRole and UsedOn.
PendingTraineeDto: scope names and Registered; deactivated accounts excluded (S6).
TraineeProfileDto.TrainingYear.
GetAdmissionCandidateQuery, and the start's training year and derived completion read (with F4's refusal).
The admission's mail and the ?admitted= result (S8).
wombat.js's clipboard call (with its failure, S10).
The /account/register/start path and its cookie (S1, S2).
The register POST's signed-in refusal (M4).
One transaction for provisioning and accept (F10).
No ResentOn (F2).

######## BOARD R3-0-spec.dc.html — R3-0-spec (1 of 2): the items, Illustration, M6, build rules
## R3-0-spec · flow 11 round 3 (1 of 2)
Each item by its number: what changed and where it is drawn, or why it is Spec only.
### The items
F1–F10, M1–M11, S1–S24
| Item
| What changed
| Where
| F1
| Admit reads out the curriculum adopted for the invited sub-speciality (dl, dd.readonly-value); Home names it; M05 and P06 dropped; P04's molefe@ gone; an SSO registrar alone gets a select
| M01–M03, M06, M07, M09, M13, M15 (read-out); M14 (the select, Illustration); P01–P03; P04 now ‹a registrar's name›
| F2
| No "Resent", no ResentOn; Issued is the current link's issue time
| L10 (10:23 SAST); Spec DATA line removed
| F3
| Invited by only when IssuedAsRole is InstitutionalAdmin, the name read now; none for an erased or deactivated issuer
| P01–P03 (shown); P04, P07 (none, Illustration); DATA list
| F4
| An empty completion whose derived date has passed is refused beside Expected completion
| M15 (Illustration)
| F5
| Sign out and continue posts the sign-out with returnTo=/account/register, accepted only when it equals that path; touches flow 02's sign-out endpoint
| G18 (a hidden returnTo); Build rules
| F6
| Illustration in the title and as a chip on the face
| every board in the Illustration list below
| F7
| "works until 2026-11-02"; the column "Last day"; expiry and Expired's 30 days on the South African calendar
| L01–L14 (Last day); N03, N06, N06b, N09, N21, L10 (OneTimeLink); G01–G06, G17 (Register's note); G10; N17, N20; the mail below
| F8
| The wait card is a DashboardCard with the control edge in DashboardFrame; no stripe
| P01–P05, P07
| F9
| "(optional)" gone; the star is the one convention; help says what empty does
| N05, N07 (Speciality's help); M01–M15 (Expected completion's help)
| F10
| Provisioning and accepting are one transaction, so "Nothing was saved." holds
| Spec only (G14 unchanged)
| M1
| A Trainee-holding admin sees the header and the T290 note only
| L17, T07 (redrawn); N24 (Invitations' words: user accounts) and M12 (Trainees' words: trainee profiles), 1280 and 390
| M2
| Home names the curriculum adopted for her invited sub-speciality; none when none is
| P01–P03; P05 has no Programme line (S5)
| M3
| Q10's and Q12's refusals typed; generic exception words; nothing of another institution in the audit row
| Spec only (Build rules)
| M4
| The register POST refuses a signed-in caller
| Spec only; G18's caption says it is reached by the page and by a stale tab's submit
| M5
| The OneTimeLink clears at the start of every action, refusal or navigation
| Spec only; N10, N11, L07 show no link
| M6
| Nothing scrolls sideways: Issued and Last day wrap date over time, Delivery .col-wrap, actions .col-actions; no width:1%, no nowrap, no min-width
| L01–L14, T01–T06; measured below; Current trainees changed to fit 901px (see M6's table)
| M7
| overflow-wrap:anywhere on results, notices and refusals; <wbr> after every @ in a sentence
| N21-390 (the 64-character address in the result); L13-390 (the 64-character address in a row); N15–N20
| M8
| ConfirmDialog's buttons 44px and column-reverse below 641px, by a general rule
| L06-390; DESIGN.md text below
| M9
| Header and stand-alone buttons 44px below 641px, by general rules
| every 390 board (Invite a person, Back to trainees, Try again)
| M10
| Load errors as a danger Alert in ActionResult; focus there after a failed retry
| L16, N23, T09, M11, P09
| M11
| Required marks: label star, aria-required, Role's legend with a hidden "required", the line above the first field
| N01–N26 (every Invite state); M01–M15 (Programme start; M14's curriculum select). No mark on Register
| S1
| A refused registration keeps token, codes and both names in the Data Protection cookie for 30 minutes; never in a URL
| Spec only
| S2
| The mail's link opens /account/register/start?token=… (PROPOSED), which sets the cookie and redirects
| Spec only; every OneTimeLink draws …/account/register/start?token=•••
| S3
| Times on the South African clock, labelled SAST; Registered and expired dates as dates
| No board change: every time on rounds 2 and 3 already reads SAST (now date over time in Issued)
| S4
| Addresses compared normalised; reads show the stored spelling
| Spec only
| S5
| An SSO registrar: no Programme line; Invited as "—"; Admit as F1's no-scope case
| P05; M14
| S6
| A deactivated pending account left off Waiting; Admit refuses it in words
| Waiting's caption (T01–T06); M13 (Illustration)
| S7
| G13 is a GET with neither cookie nor token; G16 a POST without the cookie
| G13, G16 captions
| S8
| Trainees reads ?admitted=‹profileId› and re-reads it in scope; the sentence never in the URL
| Spec only; T02's caption
| S9
| Trainee administration's refusals typed; any other fault logged and shown in M10's words
| Spec only
| S10
| The OneTimeLink inside ActionResult; Copy link; "Link copied." or "Could not copy. …" in the same live region; no inline ring
| N03, N06, N06b (the failure, Illustration), N09, N21, L10
| S11
| Results in ActionResult; .result-block gone
| L07, T02–T04, M09, M13
| S12
| The invitation mail: the role's label, the scope in the page's words, "works until"
| Spec only (mail section)
| S13
| Invite a person with nothing adopted
| N26 (Illustration)
| S14
| Missing boards
| P08, P09; N25 (no role); N08a (2.4 before a speciality: the hint is kept); M15 (F4)
| S15
| DESIGN.md: a message per field on the auth card; PasswordRules' refusal; G05's focus; "never disabled" narrowed; .readonly-value cited; restatements dropped
| DESIGN.md text below; G04, G05
| S16
| DataTable gains StackWide; a note row is a second <tr>; row buttons 44px from 900px down
| Spec only (boards already so)
| S17
| Role's radios named by their label; the description outside the label (.review-type-choice/.review-type-help); fieldset.form-group.choice-row
| N01–N26
| S18
| A single value as dl.form-group + dd.readonly-value; Training year an <output for="start"> under a real label
| N01–N26 (Institution); M01–M15 (Curriculum, Training year)
| S19
| CSS reuse
| the class table below; r3.css
| S20
| No inline style; Register's submit in FormActions; M08 the system's not-found panel
| every board (no style attribute on any page board); G01–G18; M08
| S21
| No maxlength; the 101-character last name refused
| G06
| S22
| Admit's refusals in the system's voice; M04's warning without role="alert"; loading lines that name what loads
| M09, M13; M04; L15, N22, T08, M10
| S23
| L13 read after 10:22: nothing from 09:19 or 09:22 still Being sent
| L13 (the cast's rows Sent against a mail server, the further rows Being sent within their hour)
| S24
| The Steps board
| R3-0-steps
### Illustration (F6)
Each of these carries the Illustration chip on its face and in its title, at 1280 and 390: L11 (against a mail server), L12 (composite), L13 (the heavy set), L14 (Q9), L16, L17, N06b, N13, N14, N15, N16, N17, N18, N20, N21, N23, N24, N25, N26, G06, G10, G11, G12, G13, G14, G15, G16, G17, P04, P05, P07, P08, P09, T06, T07, T09, M04, M04b, M06, M07, M09, M11, M12, M13, M14, M15.
### M6 · sideways scroll, measured
Measured in Chromium with the system's fonts loaded, each board's page at the viewport width given (the 1280 board below 641px, the 390 board at 390), the canvas's annotation pills hidden. Each cell: the page's sideways scroll · the widest table container's, in px.
Sideways scroll at each width (page · table, px)
| Board
| 1280
| 1100
| 1000
| 901
| 900
| 768
| 390
| L04, nine rows
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| L12, two not delivered
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| L13, the heavy set with the 64-character address
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| L05
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| L14
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| T01
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| T02
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| T03, four admitted
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| T04
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| T06, the Administrator
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
| 0 · 0
All 0. Getting there changed Current trainees: at 901px, where the table is still a table beside the sidebar (595px of column), it ran 98px wide. So, as built round 3 draws it: Programme start holds the date over "Training year N" (one column for two); Curriculum holds the curriculum over the sub-speciality (person-cell); a row's one button takes .col-actions--single (NEW: auto width, since .col-actions' 12rem is for three buttons); and the Administrator's institution sits under each name rather than in a column of its own (a column ran 8px over at 901). A sweep of all 199 round 3 boards at their own widths: 0 px sideways on every one.
### Build rules
M3, typed refusals at issue. IssueInvitation's refusals for Q10 and Q12 are a typed refusal: a reason (AccountExists, ActiveInvitationExists), the field (Email), and the holder's id and name only when the account or invitation is judged in the caller's scope. The exception's message is fixed generic words in every case; the page maps the reason to N15–N18's words. Nothing of another institution or College reaches the caller's audit row.
M4. POST /account/register/submit refuses a signed-in caller: back to G18's card; nothing accepted, no account created, no session replaced.
M5. The OneTimeLink is cleared at the start of every action, refusal or navigation: on Invite a person (its reset form too) and on the list (Revoke, Resend, Invite again). It is never restored.
S1. A refused registration keeps the token, the refusal codes and the two names in the Data Protection cookie for its 30 minutes; deleted on success; no password is stored. No refused redirect URL holds a name, a code or a token.
S2. The mail's link opens /account/register/start?token=… (PROPOSED), which sets the cookie and answers 303 to /account/register; the page and the GET never share a route.
Cookie (nit). SameSite=Lax, Data Protection, HttpOnly, Path=/account/register, 30 minutes; Secure follows the request's scheme, so the http replay registers.
S7. G13: a GET of /account/register with neither the cookie nor a token. G16: a POST of the form after the cookie lapsed.
S8. Admit redirects to /admin/trainees?admitted=‹profileId›; Trainees re-reads that profile in the caller's scope and writes the sentence (name, curriculum, training year, start). The URL carries the id only.
S9. Trainee administration's refusals are typed as M3's (NothingAdopted, NotPending, AlreadyAdmitted, Deactivated, CompletionPassed); any other fault is logged and shown in M10's words. Never exception.Message on a page.
F5. G18's button posts flow 02's sign-out endpoint with returnTo=/account/register; the endpoint accepts returnTo only when it equals that path (never an open redirect), and the person lands on Register with the card the cookie's token reads.
F10. Provisioning the account and accepting the invitation are one transaction, so G14's "Nothing was saved." holds.
S4. Addresses are compared normalised (trimmed, lower-cased) in Q10's check, the sweep (C3), the used-invitation reads and Expired's leaving rule; P01's Invited by and L14 show the stored spelling.
S3. Every time on the South African clock with SAST (ActivityMoments.When); Registered and the expired dates as dates (ActivityMoments.Date). Rounds 2 and 3 already read so: no board changed for it.

######## BOARD R3-0-steps.dc.html — R3-0-steps
## R3-0-steps · the 29 steps once built
### Journeys
Journeys, presses + fields typed
| Journey
| 1280
| 390
| Invite one person (Coordinator) from Home
| 4 + 1
| 4 + 1
| The nine in a row (2.2–2.7, both refusals), from Home. "Invite the next person" is the reset form's heading, not a button: no press
| 40 + 9
| 40 + 9
| Revoke and reissue (2.17) from the list
| 5 + 1
| 5 + 1
| Resend (2.26)
| 2 + 0
| 2 + 0
| Register (2.8)
| 1 + 4
| 1 + 4
| A refused registration, then success (2.9's first)
| 2 + 6
| 2 + 6
| Admit one (2.29) from Trainees
| 2 + 2
| 2 + 2
| Admit with the completion empty (2.30)
| 2 + 1
| 2 + 1
### The 29 Expects
Each step, its boards, and what its Expect says once built; today's words quoted where they change
| Step
| Boards
| Expect once built
| 1.5
| N02, N03, L02
| Today: Role defaults to "InstitutionalAdmin"; "A college is required for a college administrator." at the page's top. Once built: Invite a person; eight radios by label, none chosen; College admin shows College only, starred; the refusal (same words) under College, focused; then the built Issued words in the OneTimeLink, "It works until 2026-11-02.". Row: College admin over College of Paediatricians of South Africa, Being sent, Last day 2026-11-02.
| 1.7
| N04, L03
| Institutional admin chosen, not the default; Institution starred, offering Demo Institution and Kgosi Kgari Teaching Hospital; no speciality fields.
| 1.8
| G02
| Today: "Registering kruger@cmsa.wombat.local as CollegeAdmin." Once built: "This invitation is for kruger@cmsa.wombat.local, as College admin at College of Paediatricians of South Africa."; the note "… its link works until 2026-11-02.". Home unchanged.
| 1.9
| G07
| Today: "This invitation has already been used." Once built: h1 "Invitation already used", "This invitation has already been used: an account was registered from it.", Sign in.
| 1.10
| G03; X01
| "This invitation is for mbatha@kgk.wombat.local, as Institutional admin at Kgosi Kgari Teaching Hospital." Home's "Invite a person" (today: "which opens the invitations page") opens /admin/invitations/new (PROPOSED, Q1).
| 1.11
| L02
| Unchanged: "No active invitations".
| 2.1
| L01, N01
| Today: "Role offers InstitutionalAdmin (the default) …"; "Institution offers KGK only." Once built: no role chosen; seven radios by label; Institution read out as Kgosi Kgari Teaching Hospital; "* marks a field you must fill in.".
| 2.2
| N05, N06
| Coordinator: Speciality "Any speciality" with no star; its help says what empty does. The built Issued words; the OneTimeLink ("It works until 2026-11-02."); Copy link → "Link copied."; Open Invitations; the form reset under "Invite the next person". Row: Coordinator over Any speciality, 2026-10-19 over 09:19 SAST, Being sent.
| 2.3
| N07
| Speciality offers Paediatrics, with no star (optional for a committee member); no Sub-speciality. Rows Committee member over Paediatrics.
| 2.4
| N08a, N08
| Speciality starred, "Then choose its sub-speciality." until chosen (the hint stays); Sub-speciality starred, offering Paediatrics.
| 2.5
| N09
| Issued with Any speciality.
| 2.6
| N10
| "Speciality administrators must be scoped to a speciality." under Speciality, focused; no link on the page.
| 2.7
| N11, L04
| "The selected role requires speciality and sub-speciality scope." under Sub-speciality, focused; no link. Nine rows, 09:19 SAST, Being sent, Sithole first.
| 2.8
| G01
| Today: "Registering smit@kgk.wombat.local as Coordinator." Once built: "This invitation is for smit@kgk.wombat.local, as Coordinator at Kgosi Kgari Teaching Hospital."; the token gone by /account/register/start's redirect; Register in FormActions. The scope phrase's rule: "in ‹speciality›[ / ‹sub-speciality›]" only when the invitation has one, then "at ‹institution›".
| 2.9
| G04, G05
| Today: the refusal in the card's slot, the names dropped, the focus on First name. Once built: "The new password needs: At least 12 characters." under Password, the names kept, focus Password; then "The password confirmation does not match. Type the password again in both fields." under Confirm password, both emptied, focus Password (G05).
| 2.10
| G01–G03's pattern
| Each card names the address, the role's label, the scope phrase and the note's last day.
| 2.11
| G07
| As 1.9.
| 2.16
| N12, L05
| Five rows Trainee over Paediatrics / Paediatrics, 09:22 SAST, Being sent, ndlvou@ first.
| 2.17
| L06, L07, N19, L08, G08
| Today: "Invitation revoked." Once built: the dialog (Keep the invitation / Revoke invitation; at 390 44px, Keep last on screen); "Invitation revoked. The link sent to ndlvou@kgk.wombat.local no longer works: nobody can register from it." in ActionResult, no link; "Invite another address as Trainee, Paediatrics / Paediatrics"; the revoked link's card "Invitation revoked".
| 2.18
| P01
| Today: "Awaiting admission" / "…by your programme administrator." Once built: the DashboardCard with Institution, Programme on the Paediatric EPA Curriculum (11.1), Invited by Nolwazi Mbatha, Institutional admin, Who admits you, the four steps, Meanwhile.
| 2.19
| P02
| Menu and access denied unchanged; the card names mahlangu@kgk.wombat.local.
| 2.26
| L09, L10
| Today: "Not delivered. Resend emails a new link …" in one cell. Once built: Delivery "Not delivered" (the full stop goes with the split) and the note row "Resend emails a new link in place of the current one, which then stops working."; after Resend the built Resent words in the OneTimeLink; the row Issued 2026-10-19 over 10:23 SAST, no "Resent" (F2), Last day 2026-11-02, Being sent, only Revoke….
| 2.27
| G09, P03
| The first link: "Invitation link not recognised". The resent one: Home with the card.
| 2.28
| L01, T01
| Today: "Pending admission", "Admit to curriculum: ‹name›", "Active profiles", "No active trainee profiles found." Once built: "Waiting to be admitted" (5 waiting, by surname, each "Admit…"), "Current trainees" with "No current trainees".
| 2.29
| M01, M02, T02, T03
| Today: "Trainee profile" / "Admit a pending trainee into a curriculum.", Admit trainee landing on the profile (?id=) with Deactivate and Mark complete. Once built: "Admit Lerato Molefe"; the curriculum read out; Programme start starred, today then 2023-01-15; Training year 1 then 4; the result "Lerato Molefe admitted to the Paediatric EPA Curriculum (11.1), training year 4, from 2023-01-15." on Trainees above Dlamini's Admit…; then 3, 2, 1.
| 2.30
| M03, T04, T05
| Today: "No pending trainees found." Once built: "Left empty, it will be 2027-01-15."; the derived date in the result; "Nobody is waiting to be admitted."; his profile's "Trainee profile saved." unchanged; the list 2030-01-14.
| 2.31
| —
| Unchanged; Q7's mail besides.
| 2.32
| —
| Unchanged.
| 2.44
| X01
| Home's "Invite a person" opens /admin/invitations/new (PROPOSED, Q1), not the list; Q8's line is flow 12's.
Changed: 25 steps. Unchanged: 1.11, 2.31, 2.32 (and 2.19's menu and access denied).

######## BOARD R3-L01-none-1280.dc.html — L01 · 2.1, 2.28 none (Mbatha) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations
No active invitations
Issue an invitation to create the first onboarding link.

######## BOARD R3-L01-none-390.dc.html — L01 · 2.1, 2.28 none (Mbatha) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations
No active invitations
Issue an invitation to create the first onboarding link.

######## BOARD R3-L02-admin-none-1280.dc.html — L02 · 1.5, 1.11 none (the Administrator) · 1280
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
Home
Platform
Scheduled jobs
Audit log
SSO mappings
Data rights requests
Organisations
Institutions
Colleges
People
Users
Invitations
Catalogue
EPAs
Curricula
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
My data rights
[title: Demo Administrator] Demo Administrator
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations
No active invitations
Issue an invitation to create the first onboarding link.

######## BOARD R3-L02-admin-none-390.dc.html — L02 · 1.5, 1.11 none (the Administrator) · 390
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations
No active invitations
Issue an invitation to create the first onboarding link.

######## BOARD R3-L03-admin-two-1280.dc.html — L03 · 1.7 the Administrator's two rows · 1280
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
Home
Platform
Scheduled jobs
Audit log
SSO mappings
Data rights requests
Organisations
Institutions
Colleges
People
Users
Invitations
Catalogue
EPAs
Curricula
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
My data rights
[title: Demo Administrator] Demo Administrator
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 2 active
Every institution and College. Not delivered first, then newest first. A used or revoked invitation leaves this list.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| mbatha@kgk.wombat.local
| Institutional adminKgosi Kgari Teaching Hospital
| 2026-10-19‹time› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Institutional admin invitation to mbatha@kgk.wombat.local] Revoke…
| kruger@cmsa.wombat.local
| College adminCollege of Paediatricians of South Africa
| 2026-10-19‹time› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the College admin invitation to kruger@cmsa.wombat.local] Revoke…

######## BOARD R3-L03-admin-two-390.dc.html — L03 · 1.7 the Administrator's two rows · 390
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 2 active
Every institution and College. Not delivered first, then newest first. A used or revoked invitation leaves this list.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| mbatha@kgk.wombat.local
| Institutional adminKgosi Kgari Teaching Hospital
| 2026-10-19‹time› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Institutional admin invitation to mbatha@kgk.wombat.local] Revoke…
| kruger@cmsa.wombat.local
| College adminCollege of Paediatricians of South Africa
| 2026-10-19‹time› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the College admin invitation to kruger@cmsa.wombat.local] Revoke…

######## BOARD R3-L04-nine-1280.dc.html — L04 · 2.7 nine rows, Being sent; Issued and Last day wrap (M6) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 9 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| sithole@kgk.wombat.local
| Sub-speciality adminPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Sub-speciality admin invitation to sithole@kgk.wombat.local] Revoke…
| mokoena@kgk.wombat.local
| Speciality adminPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Speciality admin invitation to mokoena@kgk.wombat.local] Revoke…
| vanrensburg@sun.wombat.local
| Committee memberAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to vanrensburg@sun.wombat.local] Revoke…
| khumalo@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Assessor invitation to khumalo@kgk.wombat.local] Revoke…
| patel@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Assessor invitation to patel@kgk.wombat.local] Revoke…
| botha@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to botha@kgk.wombat.local] Revoke…
| naidoo@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to naidoo@kgk.wombat.local] Revoke…
| zulu@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to zulu@kgk.wombat.local] Revoke…
| smit@kgk.wombat.local
| CoordinatorAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Coordinator invitation to smit@kgk.wombat.local] Revoke…

######## BOARD R3-L04-nine-768.dc.html — L04 · 2.7 nine rows, Being sent; Issued and Last day wrap (M6) · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 9 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| sithole@kgk.wombat.local
| Sub-speciality adminPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Sub-speciality admin invitation to sithole@kgk.wombat.local] Revoke…
| mokoena@kgk.wombat.local
| Speciality adminPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Speciality admin invitation to mokoena@kgk.wombat.local] Revoke…
| vanrensburg@sun.wombat.local
| Committee memberAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to vanrensburg@sun.wombat.local] Revoke…
| khumalo@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Assessor invitation to khumalo@kgk.wombat.local] Revoke…
| patel@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Assessor invitation to patel@kgk.wombat.local] Revoke…
| botha@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to botha@kgk.wombat.local] Revoke…
| naidoo@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to naidoo@kgk.wombat.local] Revoke…
| zulu@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to zulu@kgk.wombat.local] Revoke…
| smit@kgk.wombat.local
| CoordinatorAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Coordinator invitation to smit@kgk.wombat.local] Revoke…

######## BOARD R3-L04-nine-390.dc.html — L04 · 2.7 nine rows, Being sent; Issued and Last day wrap (M6) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 9 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| sithole@kgk.wombat.local
| Sub-speciality adminPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Sub-speciality admin invitation to sithole@kgk.wombat.local] Revoke…
| mokoena@kgk.wombat.local
| Speciality adminPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Speciality admin invitation to mokoena@kgk.wombat.local] Revoke…
| vanrensburg@sun.wombat.local
| Committee memberAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to vanrensburg@sun.wombat.local] Revoke…
| khumalo@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Assessor invitation to khumalo@kgk.wombat.local] Revoke…
| patel@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Assessor invitation to patel@kgk.wombat.local] Revoke…
| botha@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to botha@kgk.wombat.local] Revoke…
| naidoo@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to naidoo@kgk.wombat.local] Revoke…
| zulu@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Committee member invitation to zulu@kgk.wombat.local] Revoke…
| smit@kgk.wombat.local
| CoordinatorAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Coordinator invitation to smit@kgk.wombat.local] Revoke…

######## BOARD R3-L05-five-1280.dc.html — L05 · 2.16 five registrars, Being sent · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 5 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| ndlvou@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ndlvou@kgk.wombat.local] Revoke…
| mahlangu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to mahlangu@kgk.wombat.local] Revoke…
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
| dlamini@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to dlamini@kgk.wombat.local] Revoke…
| molefe@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to molefe@kgk.wombat.local] Revoke…

######## BOARD R3-L05-five-768.dc.html — L05 · 2.16 five registrars, Being sent · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 5 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| ndlvou@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ndlvou@kgk.wombat.local] Revoke…
| mahlangu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to mahlangu@kgk.wombat.local] Revoke…
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
| dlamini@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to dlamini@kgk.wombat.local] Revoke…
| molefe@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to molefe@kgk.wombat.local] Revoke…

######## BOARD R3-L05-five-390.dc.html — L05 · 2.16 five registrars, Being sent · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 5 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| ndlvou@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ndlvou@kgk.wombat.local] Revoke…
| mahlangu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to mahlangu@kgk.wombat.local] Revoke…
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
| dlamini@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to dlamini@kgk.wombat.local] Revoke…
| molefe@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to molefe@kgk.wombat.local] Revoke…

######## BOARD R3-L06-revoke-dialog-1280.dc.html — L06 · 2.17 the dialog: at 390 its buttons 44px, Keep last on screen, first in tab order (M8) · focus Keep the invitation · 1280
[aria-describedby: Revoke the Trainee invitation to ndlvou@ kgk.wombat.local (Paediatrics / Paediatrics)? The registration link already emailed to that address stops working at once: nobody can register from it. The invitation leaves this list. An address is never corrected in place: to invite the right one, issue a new invitation. Revoking cannot be undone.]
### Revoke this invitation?
Revoke the Trainee invitation to ndlvou@kgk.wombat.local (Paediatrics / Paediatrics)?
The registration link already emailed to that address stops working at once: nobody can register from it.
The invitation leaves this list. An address is never corrected in place: to invite the right one, issue a new invitation.
Revoking cannot be undone.
Keep the invitationRevoke invitation

######## BOARD R3-L06-revoke-dialog-390.dc.html — L06 · 2.17 the dialog: at 390 its buttons 44px, Keep last on screen, first in tab order (M8) · focus Keep the invitation · 390
[aria-describedby: Revoke the Trainee invitation to ndlvou@ kgk.wombat.local (Paediatrics / Paediatrics)? The registration link already emailed to that address stops working at once: nobody can register from it. The invitation leaves this list. An address is never corrected in place: to invite the right one, issue a new invitation. Revoking cannot be undone.]
### Revoke this invitation?
Revoke the Trainee invitation to ndlvou@kgk.wombat.local (Paediatrics / Paediatrics)?
The registration link already emailed to that address stops working at once: nobody can register from it.
The invitation leaves this list. An address is never corrected in place: to invite the right one, issue a new invitation.
Revoking cannot be undone.
Keep the invitationRevoke invitation

######## BOARD R3-L07-revoked-1280.dc.html — L07 · 2.17 revoked: ActionResult focused, no link (M5) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
Invitation revoked. The link sent to ndlvou@kgk.wombat.local no longer works: nobody can register from it.
Invite another address as Trainee, Paediatrics / Paediatrics
### Active invitations 4 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| mahlangu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to mahlangu@kgk.wombat.local] Revoke…
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
| dlamini@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to dlamini@kgk.wombat.local] Revoke…
| molefe@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to molefe@kgk.wombat.local] Revoke…

######## BOARD R3-L07-revoked-390.dc.html — L07 · 2.17 revoked: ActionResult focused, no link (M5) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
Invitation revoked. The link sent to ndlvou@kgk.wombat.local no longer works: nobody can register from it.
Invite another address as Trainee, Paediatrics / Paediatrics
### Active invitations 4 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| mahlangu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to mahlangu@kgk.wombat.local] Revoke…
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
| dlamini@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to dlamini@kgk.wombat.local] Revoke…
| molefe@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to molefe@kgk.wombat.local] Revoke…

######## BOARD R3-L08-five-ndlovu-1280.dc.html — L08 · 2.17 after ndlovu@ is issued: five again · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 5 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| ndlovu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ndlovu@kgk.wombat.local] Revoke…
| mahlangu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to mahlangu@kgk.wombat.local] Revoke…
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
| dlamini@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to dlamini@kgk.wombat.local] Revoke…
| molefe@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to molefe@kgk.wombat.local] Revoke…

######## BOARD R3-L08-five-ndlovu-390.dc.html — L08 · 2.17 after ndlovu@ is issued: five again · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 5 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| ndlovu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ndlovu@kgk.wombat.local] Revoke…
| mahlangu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to mahlangu@kgk.wombat.local] Revoke…
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
| dlamini@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to dlamini@kgk.wombat.local] Revoke…
| molefe@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to molefe@kgk.wombat.local] Revoke…

######## BOARD R3-L09-not-delivered-1280.dc.html — L09 · 2.26 before Resend: Not delivered and its note row · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 1 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to duplessis@kgk.wombat.local] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.

######## BOARD R3-L09-not-delivered-390.dc.html — L09 · 2.26 before Resend: Not delivered and its note row · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 1 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to duplessis@kgk.wombat.local] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.

######## BOARD R3-L10-resent-1280.dc.html — L10 · 2.26 after Resend: the OneTimeLink in ActionResult, focused; Issued 10:23 SAST (F2: no Resent) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
[aria-label: Result]
A new invitation link is being emailed to duplessis@kgk.wombat.local. The link it replaces no longer works. Copy the new link below — it is shown only once.
Registration link for duplessis@kgk.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again.] [readonly] [input: …/account/register/start?token=•••] Copy link
It works until 2026-11-02. It is shown here until your next action, then never again.
### Active invitations 1 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1910:23 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…

######## BOARD R3-L10-resent-390.dc.html — L10 · 2.26 after Resend: the OneTimeLink in ActionResult, focused; Issued 10:23 SAST (F2: no Resent) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Invitations not yet used, with the state of each one's email.
Invite a person
[aria-label: Result]
A new invitation link is being emailed to duplessis@kgk.wombat.local. The link it replaces no longer works. Copy the new link below — it is shown only once.
Registration link for duplessis@kgk.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again.] [readonly] [input: …/account/register/start?token=•••] Copy link
It works until 2026-11-02. It is shown here until your next action, then never again.
### Active invitations 1 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1910:23 SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…

######## BOARD R3-L11-sent-1280.dc.html — L11 · Against a mail server: the nine rows Sent · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Illustration: against a mail server
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 9 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| sithole@kgk.wombat.local
| Sub-speciality adminPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Sub-speciality admin invitation to sithole@kgk.wombat.local] Revoke…
| mokoena@kgk.wombat.local
| Speciality adminPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Speciality admin invitation to mokoena@kgk.wombat.local] Revoke…
| vanrensburg@sun.wombat.local
| Committee memberAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to vanrensburg@sun.wombat.local] Revoke…
| khumalo@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Assessor invitation to khumalo@kgk.wombat.local] Revoke…
| patel@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Assessor invitation to patel@kgk.wombat.local] Revoke…
| botha@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to botha@kgk.wombat.local] Revoke…
| naidoo@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to naidoo@kgk.wombat.local] Revoke…
| zulu@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to zulu@kgk.wombat.local] Revoke…
| smit@kgk.wombat.local
| CoordinatorAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Coordinator invitation to smit@kgk.wombat.local] Revoke…

######## BOARD R3-L11-sent-390.dc.html — L11 · Against a mail server: the nine rows Sent · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Illustration: against a mail server
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 9 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| sithole@kgk.wombat.local
| Sub-speciality adminPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Sub-speciality admin invitation to sithole@kgk.wombat.local] Revoke…
| mokoena@kgk.wombat.local
| Speciality adminPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Speciality admin invitation to mokoena@kgk.wombat.local] Revoke…
| vanrensburg@sun.wombat.local
| Committee memberAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to vanrensburg@sun.wombat.local] Revoke…
| khumalo@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Assessor invitation to khumalo@kgk.wombat.local] Revoke…
| patel@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Assessor invitation to patel@kgk.wombat.local] Revoke…
| botha@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to botha@kgk.wombat.local] Revoke…
| naidoo@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to naidoo@kgk.wombat.local] Revoke…
| zulu@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to zulu@kgk.wombat.local] Revoke…
| smit@kgk.wombat.local
| CoordinatorAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Coordinator invitation to smit@kgk.wombat.local] Revoke…

######## BOARD R3-L12-check-address-1280.dc.html — L12 · ndlvou@ at Check the address, duplessis@ Not delivered · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Illustration: composite
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 2 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| ndlvou@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Check the address
|
[aria-label: Resend the Trainee invitation to ndlvou@kgk.wombat.local] [aria-describedby: Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ndlvou@kgk.wombat.local] Revoke…
|
Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of the current one, which then stops working.
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to duplessis@kgk.wombat.local] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.

######## BOARD R3-L12-check-address-390.dc.html — L12 · ndlvou@ at Check the address, duplessis@ Not delivered · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Illustration: composite
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 2 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| ndlvou@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Check the address
|
[aria-label: Resend the Trainee invitation to ndlvou@kgk.wombat.local] [aria-describedby: Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ndlvou@kgk.wombat.local] Revoke…
|
Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of the current one, which then stops working.
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to duplessis@kgk.wombat.local] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.

######## BOARD R3-L13-heavy-1280.dc.html — L13 · the heavy set, read after 10:22 against a mail server (S23); the 64-character address in a row (M7) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Illustration: the heavy set, read at ‹a time after 10:22› SAST
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 19 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| ndlvou@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Check the address
|
[aria-label: Resend the Trainee invitation to ndlvou@kgk.wombat.local] [aria-describedby: Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ndlvou@kgk.wombat.local] Revoke…
|
Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of the current one, which then stops working.
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to duplessis@kgk.wombat.local] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time› SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to ‹a further registrar's address›] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time› SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to ‹a further registrar's address›] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.
| xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time within the hour› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local] Revoke…
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time within the hour› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time within the hour› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
| mahlangu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Trainee invitation to mahlangu@kgk.wombat.local] Revoke…
| dlamini@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Trainee invitation to dlamini@kgk.wombat.local] Revoke…
| molefe@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Trainee invitation to molefe@kgk.wombat.local] Revoke…
| sithole@kgk.wombat.local
| Sub-speciality adminPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Sub-speciality admin invitation to sithole@kgk.wombat.local] Revoke…
| mokoena@kgk.wombat.local
| Speciality adminPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Speciality admin invitation to mokoena@kgk.wombat.local] Revoke…
| vanrensburg@sun.wombat.local
| Committee memberAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to vanrensburg@sun.wombat.local] Revoke…
| khumalo@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Assessor invitation to khumalo@kgk.wombat.local] Revoke…
| patel@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Assessor invitation to patel@kgk.wombat.local] Revoke…
| botha@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to botha@kgk.wombat.local] Revoke…
| naidoo@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to naidoo@kgk.wombat.local] Revoke…
| zulu@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to zulu@kgk.wombat.local] Revoke…
| smit@kgk.wombat.local
| CoordinatorAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Coordinator invitation to smit@kgk.wombat.local] Revoke…

######## BOARD R3-L13-heavy-768.dc.html — L13 · the heavy set, read after 10:22 against a mail server (S23); the 64-character address in a row (M7) · 768 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Illustration: the heavy set, read at ‹a time after 10:22› SAST
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 19 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| ndlvou@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Check the address
|
[aria-label: Resend the Trainee invitation to ndlvou@kgk.wombat.local] [aria-describedby: Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ndlvou@kgk.wombat.local] Revoke…
|
Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of the current one, which then stops working.
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to duplessis@kgk.wombat.local] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time› SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to ‹a further registrar's address›] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time› SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to ‹a further registrar's address›] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.
| xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time within the hour› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local] Revoke…
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time within the hour› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time within the hour› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
| mahlangu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Trainee invitation to mahlangu@kgk.wombat.local] Revoke…
| dlamini@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Trainee invitation to dlamini@kgk.wombat.local] Revoke…
| molefe@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Trainee invitation to molefe@kgk.wombat.local] Revoke…
| sithole@kgk.wombat.local
| Sub-speciality adminPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Sub-speciality admin invitation to sithole@kgk.wombat.local] Revoke…
| mokoena@kgk.wombat.local
| Speciality adminPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Speciality admin invitation to mokoena@kgk.wombat.local] Revoke…
| vanrensburg@sun.wombat.local
| Committee memberAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to vanrensburg@sun.wombat.local] Revoke…
| khumalo@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Assessor invitation to khumalo@kgk.wombat.local] Revoke…
| patel@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Assessor invitation to patel@kgk.wombat.local] Revoke…
| botha@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to botha@kgk.wombat.local] Revoke…
| naidoo@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to naidoo@kgk.wombat.local] Revoke…
| zulu@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to zulu@kgk.wombat.local] Revoke…
| smit@kgk.wombat.local
| CoordinatorAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Coordinator invitation to smit@kgk.wombat.local] Revoke…

######## BOARD R3-L13-heavy-390.dc.html — L13 · the heavy set, read after 10:22 against a mail server (S23); the 64-character address in a row (M7) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Illustration: the heavy set, read at ‹a time after 10:22› SAST
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations 19 active
At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this list; one that passes its last day unused moves to Expired, not used.
| Invitee
| Role and scope
| Issued
| Last day
| Delivery
| Actions
| ndlvou@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Check the address
|
[aria-label: Resend the Trainee invitation to ndlvou@kgk.wombat.local] [aria-describedby: Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ndlvou@kgk.wombat.local] Revoke…
|
Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of the current one, which then stops working.
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to duplessis@kgk.wombat.local] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to duplessis@kgk.wombat.local] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time› SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to ‹a further registrar's address›] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time› SAST
| 2026-11-02
| Not delivered
|
[aria-label: Resend the Trainee invitation to ‹a further registrar's address›] [aria-describedby: Resend emails a new link in place of the current one, which then stops working.] Resend [aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
|
Resend emails a new link in place of the current one, which then stops working.
| xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time within the hour› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local] Revoke…
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time within the hour› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
| ‹a further registrar's address›
| TraineePaediatrics / Paediatrics
| 2026-10-19‹time within the hour› SAST
| 2026-11-02
| Being sent
|
[aria-label: Revoke the Trainee invitation to ‹a further registrar's address›] Revoke…
| mahlangu@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Trainee invitation to mahlangu@kgk.wombat.local] Revoke…
| dlamini@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Trainee invitation to dlamini@kgk.wombat.local] Revoke…
| molefe@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Trainee invitation to molefe@kgk.wombat.local] Revoke…
| sithole@kgk.wombat.local
| Sub-speciality adminPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Sub-speciality admin invitation to sithole@kgk.wombat.local] Revoke…
| mokoena@kgk.wombat.local
| Speciality adminPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Speciality admin invitation to mokoena@kgk.wombat.local] Revoke…
| vanrensburg@sun.wombat.local
| Committee memberAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to vanrensburg@sun.wombat.local] Revoke…
| khumalo@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Assessor invitation to khumalo@kgk.wombat.local] Revoke…
| patel@kgk.wombat.local
| AssessorPaediatrics / Paediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Assessor invitation to patel@kgk.wombat.local] Revoke…
| botha@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to botha@kgk.wombat.local] Revoke…
| naidoo@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to naidoo@kgk.wombat.local] Revoke…
| zulu@kgk.wombat.local
| Committee memberPaediatrics
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Committee member invitation to zulu@kgk.wombat.local] Revoke…
| smit@kgk.wombat.local
| CoordinatorAny speciality
| 2026-10-1909:19 SAST
| 2026-11-02
| Sent
|
[aria-label: Revoke the Coordinator invitation to smit@kgk.wombat.local] Revoke…

######## BOARD R3-L14-expired-1280.dc.html — L14 · Expired, not used (Q9), read on ‹a date before 2026-12-02› · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Illustration: Q9
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations
No active invitations
Issue an invitation to create the first onboarding link.
### Expired, not used 1 expired NEW: Q9
Past their last day unused, in the last 30 days on the South African calendar, newest first. Each leaves this list 30 days after its last day, or once a new invitation goes to its address.
| Invitee
| Role and scope
| Issued
| Last day
| Actions
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
|
[aria-label: Invite duplessis@kgk.wombat.local again as Trainee, Paediatrics / Paediatrics] Invite again

######## BOARD R3-L14-expired-390.dc.html — L14 · Expired, not used (Q9), read on ‹a date before 2026-12-02› · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Illustration: Q9
Invitations not yet used, with the state of each one's email.
Invite a person
### Active invitations
No active invitations
Issue an invitation to create the first onboarding link.
### Expired, not used 1 expired NEW: Q9
Past their last day unused, in the last 30 days on the South African calendar, newest first. Each leaves this list 30 days after its last day, or once a new invitation goes to its address.
| Invitee
| Role and scope
| Issued
| Last day
| Actions
| duplessis@kgk.wombat.local
| TraineePaediatrics / Paediatrics
| 2026-10-1909:22 SAST
| 2026-11-02
|
[aria-label: Invite duplessis@kgk.wombat.local again as Trainee, Paediatrics / Paediatrics] Invite again

######## BOARD R3-L15-loading-1280.dc.html — L15 · loading: "Loading the invitations." and a skeleton; no action (S22) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Invitations not yet used, with the state of each one's email.
Loading the invitations.

######## BOARD R3-L15-loading-390.dc.html — L15 · loading: "Loading the invitations." and a skeleton; no action (S22) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Invitations not yet used, with the state of each one's email.
Loading the invitations.

######## BOARD R3-L16-load-error-1280.dc.html — L16 · a failed read: a danger Alert in ActionResult (M10); focus on it after a failed retry too · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Illustration
Invitations not yet used, with the state of each one's email.
Could not load the invitations. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-L16-load-error-390.dc.html — L16 · a failed read: a danger Alert in ActionResult (M10); focus on it after a failed retry too · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Illustration
Invitations not yet used, with the state of each one's email.
Could not load the invitations. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-L17-t290-1280.dc.html — L17 · a Trainee-holding admin: the header and T290's note only (M1) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Invitations
Illustration
Invitations not yet used, with the state of each one's email.
You hold the Trainee role, so you cannot view or change user accounts, including your own.

######## BOARD R3-L17-t290-390.dc.html — L17 · a Trainee-holding admin: the header and T290's note only (M1) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Invitations
Illustration
Invitations not yet used, with the state of each one's email.
You hold the Trainee role, so you cannot view or change user accounts, including your own.

######## BOARD R3-N01-empty-1280.dc.html — N01 · 2.1 no role chosen; the institution read out (S18); * marks (M11) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N01-empty-390.dc.html — N01 · 2.1 no role chosen; the institution read out (S18); * marks (M11) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N02-admin-college-refused-1280.dc.html — N02 · 1.5 College admin, refused beside College · focus College · 1280
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
Home
Platform
Scheduled jobs
Audit log
SSO mappings
Data rights requests
Organisations
Institutions
Colleges
People
Users
Invitations
Catalogue
EPAs
Curricula
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
My data rights
[title: Demo Administrator] Demo Administrator
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: kruger@cmsa.wombat.local]
Role * required
[aria-describedby: Keeps a College's specialities, EPAs and curricula. Choose the College.] [checked]
College admin
Keeps a College's specialities, EPAs and curricula. Choose the College.
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
College *
[aria-describedby: A college is required for a college administrator.] [aria-invalid]
[selected] Choose a College
College of Paediatricians of South Africa
Demo College
A college is required for a college administrator.
CancelIssue invitation

######## BOARD R3-N02-admin-college-refused-390.dc.html — N02 · 1.5 College admin, refused beside College · focus College · 390
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: kruger@cmsa.wombat.local]
Role * required
[aria-describedby: Keeps a College's specialities, EPAs and curricula. Choose the College.] [checked]
College admin
Keeps a College's specialities, EPAs and curricula. Choose the College.
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
College *
[aria-describedby: A college is required for a college administrator.] [aria-invalid]
[selected] Choose a College
College of Paediatricians of South Africa
Demo College
A college is required for a college administrator.
CancelIssue invitation

######## BOARD R3-N03-admin-kruger-issued-1280.dc.html — N03 · 1.5 Kruger issued: the OneTimeLink, the form reset under Invite the next person · 1280
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
Home
Platform
Scheduled jobs
Audit log
SSO mappings
Data rights requests
Organisations
Institutions
Colleges
People
Users
Invitations
Catalogue
EPAs
Curricula
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
My data rights
[title: Demo Administrator] Demo Administrator
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
[aria-label: Result]
Invitation issued for kruger@cmsa.wombat.local. Its email is being sent. Copy the link below — it is shown only once.
Registration link for kruger@cmsa.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.] [readonly] [input: …/account/register/start?token=•••] Copy link
It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.
Open Invitations
### Invite the next person
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Keeps a College's specialities, EPAs and curricula. Choose the College.]
College admin
Keeps a College's specialities, EPAs and curricula. Choose the College.
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
CancelIssue invitation

######## BOARD R3-N03-admin-kruger-issued-390.dc.html — N03 · 1.5 Kruger issued: the OneTimeLink, the form reset under Invite the next person · 390
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
[aria-label: Result]
Invitation issued for kruger@cmsa.wombat.local. Its email is being sent. Copy the link below — it is shown only once.
Registration link for kruger@cmsa.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.] [readonly] [input: …/account/register/start?token=•••] Copy link
It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.
Open Invitations
### Invite the next person
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Keeps a College's specialities, EPAs and curricula. Choose the College.]
College admin
Keeps a College's specialities, EPAs and curricula. Choose the College.
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
CancelIssue invitation

######## BOARD R3-N04-admin-institutional-1280.dc.html — N04 · 1.7 Institutional admin: the Institution picker, required · 1280
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
Home
Platform
Scheduled jobs
Audit log
SSO mappings
Data rights requests
Organisations
Institutions
Colleges
People
Users
Invitations
Catalogue
EPAs
Curricula
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
My data rights
[title: Demo Administrator] Demo Administrator
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: mbatha@kgk.wombat.local]
Role * required
[aria-describedby: Keeps a College's specialities, EPAs and curricula. Choose the College.]
College admin
Keeps a College's specialities, EPAs and curricula. Choose the College.
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.] [checked]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution *
Choose an institution
Demo Institution
[selected] Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N04-admin-institutional-390.dc.html — N04 · 1.7 Institutional admin: the Institution picker, required · 390
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: mbatha@kgk.wombat.local]
Role * required
[aria-describedby: Keeps a College's specialities, EPAs and curricula. Choose the College.]
College admin
Keeps a College's specialities, EPAs and curricula. Choose the College.
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.] [checked]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution *
Choose an institution
Demo Institution
[selected] Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N05-coordinator-1280.dc.html — N05 · 2.2 Coordinator: Speciality has no star, its help says what empty does (F9) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: smit@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.] [checked]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
[aria-describedby: Leave it at Any speciality for the whole institution.]
[selected] Any speciality
Paediatrics
Leave it at Any speciality for the whole institution.
CancelIssue invitation

######## BOARD R3-N05-coordinator-390.dc.html — N05 · 2.2 Coordinator: Speciality has no star, its help says what empty does (F9) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: smit@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.] [checked]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
[aria-describedby: Leave it at Any speciality for the whole institution.]
[selected] Any speciality
Paediatrics
Leave it at Any speciality for the whole institution.
CancelIssue invitation

######## BOARD R3-N06-smit-issued-1280.dc.html — N06 · 2.2 Smit issued, Copy link pressed: "Link copied." in the live region · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
[aria-label: Result]
Invitation issued for smit@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown only once.
Registration link for smit@kgk.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one. Link copied.] [readonly] [input: …/account/register/start?token=•••] [aria-describedby: Link copied.] Copied
Link copied.
It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.
Open Invitations
### Invite the next person
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N06-smit-issued-390.dc.html — N06 · 2.2 Smit issued, Copy link pressed: "Link copied." in the live region · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
[aria-label: Result]
Invitation issued for smit@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown only once.
Registration link for smit@kgk.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one. Link copied.] [readonly] [input: …/account/register/start?token=•••] [aria-describedby: Link copied.] Copied
Link copied.
It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.
Open Invitations
### Invite the next person
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N06b-copy-failed-1280.dc.html — N06b · S10 the copy failed: "Could not copy. Select the link and copy it yourself." (NEW) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
[aria-label: Result]
Invitation issued for smit@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown only once.
Registration link for smit@kgk.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one. Could not copy. Select the link and copy it yourself. NEW] [readonly] [input: …/account/register/start?token=•••] [aria-describedby: Could not copy. Select the link and copy it yourself. NEW] Copy link
Could not copy. Select the link and copy it yourself. NEW
It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.
Open Invitations
### Invite the next person
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N06b-copy-failed-390.dc.html — N06b · S10 the copy failed: "Could not copy. Select the link and copy it yourself." (NEW) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
[aria-label: Result]
Invitation issued for smit@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown only once.
Registration link for smit@kgk.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one. Could not copy. Select the link and copy it yourself. NEW] [readonly] [input: …/account/register/start?token=•••] [aria-describedby: Could not copy. Select the link and copy it yourself. NEW] Copy link
Could not copy. Select the link and copy it yourself. NEW
It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.
Open Invitations
### Invite the next person
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N07-committee-1280.dc.html — N07 · 2.3 Committee member with Paediatrics · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: zulu@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.] [checked]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
[aria-describedby: Leave it at Any speciality for the whole institution.]
Any speciality
[selected] Paediatrics
Leave it at Any speciality for the whole institution.
CancelIssue invitation

######## BOARD R3-N07-committee-390.dc.html — N07 · 2.3 Committee member with Paediatrics · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: zulu@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.] [checked]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
[aria-describedby: Leave it at Any speciality for the whole institution.]
Any speciality
[selected] Paediatrics
Leave it at Any speciality for the whole institution.
CancelIssue invitation

######## BOARD R3-N08-assessor-1280.dc.html — N08 · 2.4 Assessor: Speciality, then Sub-speciality, both starred · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: patel@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.] [checked]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N08-assessor-390.dc.html — N08 · 2.4 Assessor: Speciality, then Sub-speciality, both starred · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: patel@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.] [checked]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N08a-assessor-before-1280.dc.html — N08a · 2.4 Assessor before a speciality: "Then choose its sub-speciality." (S14) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: patel@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.] [checked]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
[aria-describedby: Then choose its sub-speciality.]
[selected] Choose a speciality
Paediatrics
Then choose its sub-speciality.
CancelIssue invitation

######## BOARD R3-N08a-assessor-before-390.dc.html — N08a · 2.4 Assessor before a speciality: "Then choose its sub-speciality." (S14) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: patel@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.] [checked]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
[aria-describedby: Then choose its sub-speciality.]
[selected] Choose a speciality
Paediatrics
Then choose its sub-speciality.
CancelIssue invitation

######## BOARD R3-N09-vanrensburg-issued-1280.dc.html — N09 · 2.5 van Rensburg issued (Any speciality) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
[aria-label: Result]
Invitation issued for vanrensburg@sun.wombat.local. Its email is being sent. Copy the link below — it is shown only once.
Registration link for vanrensburg@sun.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.] [readonly] [input: …/account/register/start?token=•••] Copy link
It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.
Open Invitations
### Invite the next person
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N09-vanrensburg-issued-390.dc.html — N09 · 2.5 van Rensburg issued (Any speciality) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
[aria-label: Result]
Invitation issued for vanrensburg@sun.wombat.local. Its email is being sent. Copy the link below — it is shown only once.
Registration link for vanrensburg@sun.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.] [readonly] [input: …/account/register/start?token=•••] Copy link
It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.
Open Invitations
### Invite the next person
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N10-refused-speciality-1280.dc.html — N10 · 2.6 refused beside Speciality; no link (M5) · focus Speciality · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: mokoena@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.] [checked]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
[aria-describedby: Speciality administrators must be scoped to a speciality.] [aria-invalid]
[selected] Choose a speciality
Paediatrics
Speciality administrators must be scoped to a speciality.
CancelIssue invitation

######## BOARD R3-N10-refused-speciality-390.dc.html — N10 · 2.6 refused beside Speciality; no link (M5) · focus Speciality · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: mokoena@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.] [checked]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
[aria-describedby: Speciality administrators must be scoped to a speciality.] [aria-invalid]
[selected] Choose a speciality
Paediatrics
Speciality administrators must be scoped to a speciality.
CancelIssue invitation

######## BOARD R3-N11-refused-sub-1280.dc.html — N11 · 2.7 refused beside Sub-speciality; no link (M5) · focus Sub-speciality · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: sithole@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.] [checked]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
[aria-describedby: The selected role requires speciality and sub-speciality scope.] [aria-invalid]
[selected] Choose a sub-speciality
Paediatrics
The selected role requires speciality and sub-speciality scope.
CancelIssue invitation

######## BOARD R3-N11-refused-sub-390.dc.html — N11 · 2.7 refused beside Sub-speciality; no link (M5) · focus Sub-speciality · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: sithole@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.] [checked]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
[aria-describedby: The selected role requires speciality and sub-speciality scope.] [aria-invalid]
[selected] Choose a sub-speciality
Paediatrics
The selected role requires speciality and sub-speciality scope.
CancelIssue invitation

######## BOARD R3-N12-trainee-1280.dc.html — N12 · 2.16 Trainee · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: molefe@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N12-trainee-390.dc.html — N12 · 2.16 Trainee · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: molefe@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N13-email-blank-1280.dc.html — N13 · Email left blank: NEW words (T324) · focus Email · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: Enter the person's email address. NEW] [aria-invalid]
Enter the person's email address. NEW
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.] [checked]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
[aria-describedby: Leave it at Any speciality for the whole institution.]
[selected] Any speciality
Paediatrics
Leave it at Any speciality for the whole institution.
CancelIssue invitation

######## BOARD R3-N13-email-blank-390.dc.html — N13 · Email left blank: NEW words (T324) · focus Email · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: Enter the person's email address. NEW] [aria-invalid]
Enter the person's email address. NEW
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.] [checked]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
[aria-describedby: Leave it at Any speciality for the whole institution.]
[selected] Any speciality
Paediatrics
Leave it at Any speciality for the whole institution.
CancelIssue invitation

######## BOARD R3-N14-malformed-1280.dc.html — N14 · a malformed address · focus Email · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: Enter a valid email address, for example name@ example.org.] [aria-invalid] [input: ‹a malformed address›]
Enter a valid email address, for example name@example.org.
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N14-malformed-390.dc.html — N14 · a malformed address · focus Email · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: Enter a valid email address, for example name@ example.org.] [aria-invalid] [input: ‹a malformed address›]
Enter a valid email address, for example name@example.org.
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N15-t286-in-scope-1280.dc.html — N15 · T286 in scope: one link, "open Pieter Smit under Users" · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: smit@ kgk.wombat.local already has a Wombat account (Pieter Smit), so it cannot be invited. To give Pieter Smit another role, open Pieter Smit under Users . NEW] [aria-invalid] [input: smit@kgk.wombat.local]
smit@kgk.wombat.local already has a Wombat account (Pieter Smit), so it cannot be invited. To give Pieter Smit another role, open Pieter Smit under Users. NEW
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.] [checked]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
[aria-describedby: Leave it at Any speciality for the whole institution.]
[selected] Any speciality
Paediatrics
Leave it at Any speciality for the whole institution.
CancelIssue invitation

######## BOARD R3-N15-t286-in-scope-390.dc.html — N15 · T286 in scope: one link, "open Pieter Smit under Users" · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: smit@ kgk.wombat.local already has a Wombat account (Pieter Smit), so it cannot be invited. To give Pieter Smit another role, open Pieter Smit under Users . NEW] [aria-invalid] [input: smit@kgk.wombat.local]
smit@kgk.wombat.local already has a Wombat account (Pieter Smit), so it cannot be invited. To give Pieter Smit another role, open Pieter Smit under Users. NEW
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.] [checked]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
[aria-describedby: Leave it at Any speciality for the whole institution.]
[selected] Any speciality
Paediatrics
Leave it at Any speciality for the whole institution.
CancelIssue invitation

######## BOARD R3-N16-t286-out-of-scope-1280.dc.html — N16 · T286 outside the caller's scope: no name, no link · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: An account with this address already exists. Ask a Wombat administrator to add the role. NEW] [aria-invalid] [input: kruger@cmsa.wombat.local]
An account with this address already exists. Ask a Wombat administrator to add the role. NEW
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.] [checked]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
[aria-describedby: Leave it at Any speciality for the whole institution.]
[selected] Any speciality
Paediatrics
Leave it at Any speciality for the whole institution.
CancelIssue invitation

######## BOARD R3-N16-t286-out-of-scope-390.dc.html — N16 · T286 outside the caller's scope: no name, no link · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: An account with this address already exists. Ask a Wombat administrator to add the role. NEW] [aria-invalid] [input: kruger@cmsa.wombat.local]
An account with this address already exists. Ask a Wombat administrator to add the role. NEW
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.] [checked]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
[aria-describedby: Leave it at Any speciality for the whole institution.]
[selected] Any speciality
Paediatrics
Leave it at Any speciality for the whole institution.
CancelIssue invitation

######## BOARD R3-N17-q10-in-scope-1280.dc.html — N17 · Q10 in scope: the dates, Invitations linked · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: molefe@ kgk.wombat.local already has an active invitation, issued 2026-10-19 09:22 SAST, which works until 2026-11-02. Resend it or revoke it under Invitations . NEW] [aria-invalid] [input: molefe@kgk.wombat.local]
molefe@kgk.wombat.local already has an active invitation, issued 2026-10-19 09:22 SAST, which works until 2026-11-02. Resend it or revoke it under Invitations. NEW
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N17-q10-in-scope-390.dc.html — N17 · Q10 in scope: the dates, Invitations linked · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: molefe@ kgk.wombat.local already has an active invitation, issued 2026-10-19 09:22 SAST, which works until 2026-11-02. Resend it or revoke it under Invitations . NEW] [aria-invalid] [input: molefe@kgk.wombat.local]
molefe@kgk.wombat.local already has an active invitation, issued 2026-10-19 09:22 SAST, which works until 2026-11-02. Resend it or revoke it under Invitations. NEW
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N18-q10-out-of-scope-1280.dc.html — N18 · Q10 outside the caller's scope: no date · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: This address already has an active invitation from another institution. Ask a Wombat administrator. NEW] [aria-invalid] [input: ‹an address another institution invited›]
This address already has an active invitation from another institution. Ask a Wombat administrator. NEW
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N18-q10-out-of-scope-390.dc.html — N18 · Q10 outside the caller's scope: no date · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[aria-describedby: This address already has an active invitation from another institution. Ask a Wombat administrator. NEW] [aria-invalid] [input: ‹an address another institution invited›]
This address already has an active invitation from another institution. Ask a Wombat administrator. NEW
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N19-prefilled-revoke-1280.dc.html — N19 · 2.17 from Invite another address…: Email empty · focus Email · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
Role and scope filled from the revoked invitation to ndlvou@kgk.wombat.local. Type the right address. NEW
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N19-prefilled-revoke-390.dc.html — N19 · 2.17 from Invite another address…: Email empty · focus Email · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
Role and scope filled from the revoked invitation to ndlvou@kgk.wombat.local. Type the right address. NEW
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N20-prefilled-expired-1280.dc.html — N20 · from Invite again (Q9): address, role and scope filled · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration: Q9
One person, one role. The registration link is shown here once, and emailed to them.
Filled from the invitation to duplessis@kgk.wombat.local, whose last day was 2026-11-02. Check it, then issue. NEW
* marks a field you must fill in.
Email *
[input: duplessis@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N20-prefilled-expired-390.dc.html — N20 · from Invite again (Q9): address, role and scope filled · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration: Q9
One person, one role. The registration link is shown here once, and emailed to them.
Filled from the invitation to duplessis@kgk.wombat.local, whose last day was 2026-11-02. Check it, then issue. NEW
* marks a field you must fill in.
Email *
[input: duplessis@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.] [checked]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality *
Choose a speciality
[selected] Paediatrics
Sub-speciality *
Choose a sub-speciality
[selected] Paediatrics
CancelIssue invitation

######## BOARD R3-N21-long-address-1280.dc.html — N21 · a 64-character address: the result and the label break after the @ (M7) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
[aria-label: Result]
Invitation issued for xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown only once.
Registration link for xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.] [readonly] [input: …/account/register/start?token=•••] Copy link
It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.
Open Invitations
### Invite the next person
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N21-long-address-390.dc.html — N21 · a 64-character address: the result and the label break after the @ (M7) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
[aria-label: Result]
Invitation issued for xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown only once.
Registration link for xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx@kgk.wombat.local
[aria-describedby: It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.] [readonly] [input: …/account/register/start?token=•••] Copy link
It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.
Open Invitations
### Invite the next person
* marks a field you must fill in.
Email *
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N22-loading-1280.dc.html — N22 · loading: "Loading what you can invite." · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
Loading what you can invite.

######## BOARD R3-N22-loading-390.dc.html — N22 · loading: "Loading what you can invite." · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
One person, one role. The registration link is shown here once, and emailed to them.
Loading what you can invite.

######## BOARD R3-N23-load-error-1280.dc.html — N23 · a failed read: the danger Alert in ActionResult (M10) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
Could not load what you can invite. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-N23-load-error-390.dc.html — N23 · a failed read: the danger Alert in ActionResult (M10) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
Could not load what you can invite. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-N24-t290-1280.dc.html — N24 · a Trainee-holding admin: the header and Invitations' T290 note, before any read (M1) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
You hold the Trainee role, so you cannot view or change user accounts, including your own.

######## BOARD R3-N24-t290-390.dc.html — N24 · a Trainee-holding admin: the header and Invitations' T290 note, before any read (M1) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
You hold the Trainee role, so you cannot view or change user accounts, including your own.

######## BOARD R3-N25-no-role-1280.dc.html — N25 · Issue with no role: "Choose a role." under the legend, the first radio focused (S14) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: smit@kgk.wombat.local]
[aria-describedby: Choose a role. NEW]
Role * required
Choose a role. NEW
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N25-no-role-390.dc.html — N25 · Issue with no role: "Choose a role." under the legend, the first radio focused (S14) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: smit@kgk.wombat.local]
[aria-describedby: Choose a role. NEW]
Role * required
Choose a role. NEW
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
CancelIssue invitation

######## BOARD R3-N26-nothing-adopted-1280.dc.html — N26 · nothing adopted (S13): Issue stays for the roles that need no speciality · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: patel@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.] [checked]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
Nothing is adopted at Kgosi Kgari Teaching Hospital yet, so roles that need a speciality cannot be invited. Adopt a curriculum under Curriculum adoptions. NEW
CancelIssue invitation

######## BOARD R3-N26-nothing-adopted-390.dc.html — N26 · nothing adopted (S13): Issue stays for the roles that need no speciality · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Invitations
Invite a person
## Invite a person
Illustration
One person, one role. The registration link is shown here once, and emailed to them.
* marks a field you must fill in.
Email *
[input: patel@kgk.wombat.local]
Role * required
[aria-describedby: Runs an institution in Wombat: invites and admits people, adopts curricula.]
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
[aria-describedby: Runs one speciality's committee work: panels, reviews and decisions due.]
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
[aria-describedby: Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.]
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
[aria-describedby: Works across the institution. Choose a speciality only to keep them to it.]
Coordinator
Works across the institution. Choose a speciality only to keep them to it.
[aria-describedby: Sits committee reviews. Choose a speciality only to keep them to it.]
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
[aria-describedby: Assesses the registrars who ask them, at this institution.] [checked]
Assessor
Assesses the registrars who ask them, at this institution.
[aria-describedby: A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.]
Trainee
A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.
Institution
Kgosi Kgari Teaching Hospital
Speciality
Nothing is adopted at Kgosi Kgari Teaching Hospital yet, so roles that need a speciality cannot be invited. Adopt a curriculum under Curriculum adoptions. NEW
CancelIssue invitation

######## BOARD R3-G01-smit-1280.dc.html — G01 · 2.8 Smit's form; Register in FormActions (S20); no marks (Q6) · 1280
Wombat
Work-based assessment for specialist training.
## Complete registration
This invitation is for smit@kgk.wombat.local, as Coordinator at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: smit@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
Last name
Password
[aria-describedby: The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-label: Show password] Show
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-label: Show confirm password] Show
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G01-smit-390.dc.html — G01 · 2.8 Smit's form; Register in FormActions (S20); no marks (Q6) · 390
Wombat
Work-based assessment for specialist training.
## Complete registration
This invitation is for smit@kgk.wombat.local, as Coordinator at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: smit@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
Last name
Password
[aria-describedby: The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-label: Show password] Show
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-label: Show confirm password] Show
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G02-kruger-1280.dc.html — G02 · 1.8 Kruger's form · 1280
Wombat
Work-based assessment for specialist training.
## Complete registration
This invitation is for kruger@cmsa.wombat.local, as College admin at College of Paediatricians of South Africa. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: kruger@cmsa.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
Last name
Password
[aria-describedby: The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-label: Show password] Show
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-label: Show confirm password] Show
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G02-kruger-390.dc.html — G02 · 1.8 Kruger's form · 390
Wombat
Work-based assessment for specialist training.
## Complete registration
This invitation is for kruger@cmsa.wombat.local, as College admin at College of Paediatricians of South Africa. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: kruger@cmsa.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
Last name
Password
[aria-describedby: The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-label: Show password] Show
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-label: Show confirm password] Show
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G03-mbatha-1280.dc.html — G03 · 1.10 Mbatha's form · 1280
Wombat
Work-based assessment for specialist training.
## Complete registration
This invitation is for mbatha@kgk.wombat.local, as Institutional admin at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: mbatha@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
Last name
Password
[aria-describedby: The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-label: Show password] Show
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-label: Show confirm password] Show
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G03-mbatha-390.dc.html — G03 · 1.10 Mbatha's form · 390
Wombat
Work-based assessment for specialist training.
## Complete registration
This invitation is for mbatha@kgk.wombat.local, as Institutional admin at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: mbatha@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
Last name
Password
[aria-describedby: The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-label: Show password] Show
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-label: Show confirm password] Show
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G04-password-refused-1280.dc.html — G04 · 2.9 first: under Password, which is described by it and the rules; names kept · focus Password · 1280
Wombat
Work-based assessment for specialist training.
## Complete registration
This invitation is for patel@kgk.wombat.local, as Assessor in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: patel@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
[input: Mohammed]
Last name
[input: Patel]
Password
[aria-describedby: The new password needs: At least 12 characters. The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-invalid] [aria-label: Show password] Show
The new password needs: At least 12 characters.
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-label: Show confirm password] Show
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G04-password-refused-390.dc.html — G04 · 2.9 first: under Password, which is described by it and the rules; names kept · focus Password · 390
Wombat
Work-based assessment for specialist training.
## Complete registration
This invitation is for patel@kgk.wombat.local, as Assessor in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: patel@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
[input: Mohammed]
Last name
[input: Patel]
Password
[aria-describedby: The new password needs: At least 12 characters. The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-invalid] [aria-label: Show password] Show
The new password needs: At least 12 characters.
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-label: Show confirm password] Show
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G05-confirm-refused-1280.dc.html — G05 · 2.9 second: under Confirm password; both emptied; Password described by it too · focus Password · 1280
Wombat
Work-based assessment for specialist training.
## Complete registration
This invitation is for patel@kgk.wombat.local, as Assessor in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: patel@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
[input: Mohammed]
Last name
[input: Patel]
Password
[aria-describedby: The password confirmation does not match. Type the password again in both fields. NEW The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-label: Show password] Show
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-describedby: The password confirmation does not match. Type the password again in both fields. NEW] [aria-invalid] [aria-label: Show confirm password] Show
The password confirmation does not match. Type the password again in both fields. NEW
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G05-confirm-refused-390.dc.html — G05 · 2.9 second: under Confirm password; both emptied; Password described by it too · focus Password · 390
Wombat
Work-based assessment for specialist training.
## Complete registration
This invitation is for patel@kgk.wombat.local, as Assessor in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: patel@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
[input: Mohammed]
Last name
[input: Patel]
Password
[aria-describedby: The password confirmation does not match. Type the password again in both fields. NEW The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-label: Show password] Show
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-describedby: The password confirmation does not match. Type the password again in both fields. NEW] [aria-invalid] [aria-label: Show confirm password] Show
The password confirmation does not match. Type the password again in both fields. NEW
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G06-details-invalid-1280.dc.html — G06 · a 101-character last name typed and refused (S21; no maxlength) · focus Last name · 1280 · Illustration
Wombat
Work-based assessment for specialist training.
## Complete registration
Illustration
This invitation is for patel@kgk.wombat.local, as Assessor in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: patel@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
[input: Mohammed]
Last name
[aria-describedby: Last name: enter 100 characters or fewer. NEW] [aria-invalid] [input: ‹a last name of 101 characters›]
Last name: enter 100 characters or fewer. NEW
Password
[aria-describedby: The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-label: Show password] Show
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-label: Show confirm password] Show
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G06-details-invalid-390.dc.html — G06 · a 101-character last name typed and refused (S21; no maxlength) · focus Last name · 390 · Illustration
Wombat
Work-based assessment for specialist training.
## Complete registration
Illustration
This invitation is for patel@kgk.wombat.local, as Assessor in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: patel@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
[input: Mohammed]
Last name
[aria-describedby: Last name: enter 100 characters or fewer. NEW] [aria-invalid] [input: ‹a last name of 101 characters›]
Last name: enter 100 characters or fewer. NEW
Password
[aria-describedby: The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.] [aria-label: Show password] Show
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
[aria-label: Show confirm password] Show
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G07-used-1280.dc.html — G07 · 1.9, 2.11 used: Sign in · 1280
Wombat
Work-based assessment for specialist training.
## Invitation already used
This invitation has already been used: an account was registered from it. NEW
If it was yours, sign in with that account.
Sign in
Forgotten your password?

######## BOARD R3-G07-used-390.dc.html — G07 · 1.9, 2.11 used: Sign in · 390
Wombat
Work-based assessment for specialist training.
## Invitation already used
This invitation has already been used: an account was registered from it. NEW
If it was yours, sign in with that account.
Sign in
Forgotten your password?

######## BOARD R3-G08-revoked-1280.dc.html — G08 · 2.17 ndlvou@'s revoked link · 1280
Wombat
Work-based assessment for specialist training.
## Invitation revoked
This invitation has been revoked.
Ask the person who invited you for a new invitation.

######## BOARD R3-G08-revoked-390.dc.html — G08 · 2.17 ndlvou@'s revoked link · 390
Wombat
Work-based assessment for specialist training.
## Invitation revoked
This invitation has been revoked.
Ask the person who invited you for a new invitation.

######## BOARD R3-G09-invalid-1280.dc.html — G09 · 2.27 the link a resend replaced · 1280
Wombat
Work-based assessment for specialist training.
## Invitation link not recognised
This invitation is invalid.
The link may be incomplete, or a newer invitation email may have replaced it. Open the link in the most recent invitation email, or ask the person who invited you for a new invitation. NEW

######## BOARD R3-G09-invalid-390.dc.html — G09 · 2.27 the link a resend replaced · 390
Wombat
Work-based assessment for specialist training.
## Invitation link not recognised
This invitation is invalid.
The link may be incomplete, or a newer invitation email may have replaced it. Open the link in the most recent invitation email, or ask the person who invited you for a new invitation. NEW

######## BOARD R3-G10-expired-1280.dc.html — G10 · expired: the day after its last day on the SA calendar (F7) · 1280 · Illustration
Wombat
Work-based assessment for specialist training.
## Invitation expired
Illustration
This invitation has expired: its link worked until 2026-11-02. NEW
Ask the person who invited you for a new invitation.

######## BOARD R3-G10-expired-390.dc.html — G10 · expired: the day after its last day on the SA calendar (F7) · 390 · Illustration
Wombat
Work-based assessment for specialist training.
## Invitation expired
Illustration
This invitation has expired: its link worked until 2026-11-02. NEW
Ask the person who invited you for a new invitation.

######## BOARD R3-G11-exists-1280.dc.html — G11 · the address already has an account: Sign in · 1280 · Illustration
Wombat
Work-based assessment for specialist training.
## Account already exists
Illustration
A user with this email address already exists.
Sign in with that account.
Sign in
Forgotten your password?

######## BOARD R3-G11-exists-390.dc.html — G11 · the address already has an account: Sign in · 390 · Illustration
Wombat
Work-based assessment for specialist training.
## Account already exists
Illustration
A user with this email address already exists.
Sign in with that account.
Sign in
Forgotten your password?

######## BOARD R3-G12-not-accepted-1280.dc.html — G12 · the address not accepted · 1280 · Illustration
Wombat
Work-based assessment for specialist training.
## Address not accepted
Illustration
This email address cannot be used for an account. Ask your administrator for an invitation to another address.

######## BOARD R3-G12-not-accepted-390.dc.html — G12 · the address not accepted · 390 · Illustration
Wombat
Work-based assessment for specialist training.
## Address not accepted
Illustration
This email address cannot be used for an account. Ask your administrator for an invitation to another address.

######## BOARD R3-G13-no-token-1280.dc.html — G13 · a GET of /account/register with neither the cookie nor a token (S7) · 1280 · Illustration
Wombat
Work-based assessment for specialist training.
## Invitation link incomplete
Illustration
The invitation token is missing.
Open the link from the invitation email again. If the email split it over two lines, copy all of it into the address bar. NEW

######## BOARD R3-G13-no-token-390.dc.html — G13 · a GET of /account/register with neither the cookie nor a token (S7) · 390 · Illustration
Wombat
Work-based assessment for specialist training.
## Invitation link incomplete
Illustration
The invitation token is missing.
Open the link from the invitation email again. If the email split it over two lines, copy all of it into the address bar. NEW

######## BOARD R3-G14-fault-1280.dc.html — G14 · could not be completed; one transaction, so nothing was saved (F10); Try again reopens the form from the cookie · 1280 · Illustration
Wombat
Work-based assessment for specialist training.
## Registration not completed
Illustration
Registration could not be completed. Nothing was saved. NEW
The invitation is still open in this browser for 30 minutes from when you opened its link.
Try again

######## BOARD R3-G14-fault-390.dc.html — G14 · could not be completed; one transaction, so nothing was saved (F10); Try again reopens the form from the cookie · 390 · Illustration
Wombat
Work-based assessment for specialist training.
## Registration not completed
Illustration
Registration could not be completed. Nothing was saved. NEW
The invitation is still open in this browser for 30 minutes from when you opened its link.
Try again

######## BOARD R3-G15-user-not-loaded-1280.dc.html — G15 · the user not loaded after registration: Sign in · 1280 · Illustration
Wombat
Work-based assessment for specialist training.
## Registered, but not signed in
Illustration
The invited user could not be loaded after registration.
Your account was created. Sign in with this email address and the password you chose. PROPOSED: next step
Sign in

######## BOARD R3-G15-user-not-loaded-390.dc.html — G15 · the user not loaded after registration: Sign in · 390 · Illustration
Wombat
Work-based assessment for specialist training.
## Registered, but not signed in
Illustration
The invited user could not be loaded after registration.
Your account was created. Sign in with this email address and the password you chose. PROPOSED: next step
Sign in

######## BOARD R3-G16-cookie-lapsed-1280.dc.html — G16 · a POST of the form after the cookie's 30 minutes (S7) · 1280 · Illustration
Wombat
Work-based assessment for specialist training.
## Invitation timed out
Illustration
This page held the invitation for 30 minutes, and that time has passed. Nothing was saved. NEW
Try again: open the link in the invitation email once more.

######## BOARD R3-G16-cookie-lapsed-390.dc.html — G16 · a POST of the form after the cookie's 30 minutes (S7) · 390 · Illustration
Wombat
Work-based assessment for specialist training.
## Invitation timed out
Illustration
This page held the invitation for 30 minutes, and that time has passed. Nothing was saved. NEW
Try again: open the link in the invitation email once more.

######## BOARD R3-G17-script-blocked-1280.dc.html — G17 · script blocked: no Show toggles; the token still gone (the redirect needs no script) · 1280 · Illustration
Wombat
Work-based assessment for specialist training.
## Complete registration
Illustration
This invitation is for smit@kgk.wombat.local, as Coordinator at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: smit@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
Last name
Password
[aria-describedby: The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.]
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G17-script-blocked-390.dc.html — G17 · script blocked: no Show toggles; the token still gone (the redirect needs no script) · 390 · Illustration
Wombat
Work-based assessment for specialist training.
## Complete registration
Illustration
This invitation is for smit@kgk.wombat.local, as Coordinator at Kgosi Kgari Teaching Hospital. NEW: words DATA: scope names
Email
[aria-describedby: The address this invitation is for. It cannot be changed here.] [readonly] [input: smit@kgk.wombat.local]
The address this invitation is for. It cannot be changed here.
First name
Last name
Password
[aria-describedby: The new password needs: At least 12 characters. At least 4 different characters. A digit (0 to 9). An upper-case letter. A lower-case letter. A symbol, such as ! or #.]
The new password needs:
At least 12 characters.
At least 4 different characters.
A digit (0 to 9).
An upper-case letter.
A lower-case letter.
A symbol, such as ! or #.
Confirm password
Register
Not expecting this invitation? Ignore it: its link works until 2026-11-02. DATA: ExpiresOn, SA calendar

######## BOARD R3-G18-signed-in-elsewhere-1280.dc.html — G18 · Q11: reached by the page AND by a stale tab's submit (M4); the button posts sign-out with returnTo=/account/register (F5) · 1280
Wombat
Work-based assessment for specialist training.
## Signed in as someone else
You are signed in as Pieter Smit. This invitation is for patel@kgk.wombat.local. NEW
To register it, sign out first. Your own account is not changed.
Sign out and continue
Go to Home

######## BOARD R3-G18-signed-in-elsewhere-390.dc.html — G18 · Q11: reached by the page AND by a stale tab's submit (M4); the button posts sign-out with returnTo=/account/register (F5) · 390
Wombat
Work-based assessment for specialist training.
## Signed in as someone else
You are signed in as Pieter Smit. This invitation is for patel@kgk.wombat.local. NEW
To register it, sign out first. Your own account is not changed.
Sign out and continue
Go to Home

######## BOARD R3-P01-molefe-1280.dc.html — P01 · 2.18 Molefe: DashboardCard, the control edge, no stripe (F8) · 1280
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
Acting as Pending trainee
Home
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## Home
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Programme
Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1) DATA: the invited sub-speciality's curriculum
Invited by
Nolwazi Mbatha, Institutional admin, on 2026-10-19 DATA: IssuedAsRole, UsedOn
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital. If it is taking long, ask Nolwazi Mbatha.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at molefe@kgk.wombat.local, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P01-molefe-390.dc.html — P01 · 2.18 Molefe: DashboardCard, the control edge, no stripe (F8) · 390
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
## Home
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Programme
Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1) DATA: the invited sub-speciality's curriculum
Invited by
Nolwazi Mbatha, Institutional admin, on 2026-10-19 DATA: IssuedAsRole, UsedOn
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital. If it is taking long, ask Nolwazi Mbatha.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at molefe@kgk.wombat.local, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P02-mahlangu-1280.dc.html — P02 · 2.19 Mahlangu; at 390 her menu open · 1280
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
Acting as Pending trainee
Home
My data rights
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
## Home
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Programme
Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1) DATA: the invited sub-speciality's curriculum
Invited by
Nolwazi Mbatha, Institutional admin, on 2026-10-19 DATA: IssuedAsRole, UsedOn
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital. If it is taking long, ask Nolwazi Mbatha.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at mahlangu@kgk.wombat.local, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P02-mahlangu-390.dc.html — P02 · 2.19 Mahlangu; at 390 her menu open · 390
[checked]
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
Acting as Pending trainee
Home
My data rights
[title: Nomsa Mahlangu] Nomsa Mahlangu
Sign out
## Home
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Programme
Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1) DATA: the invited sub-speciality's curriculum
Invited by
Nolwazi Mbatha, Institutional admin, on 2026-10-19 DATA: IssuedAsRole, UsedOn
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital. If it is taking long, ask Nolwazi Mbatha.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at mahlangu@kgk.wombat.local, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P03-duplessis-1280.dc.html — P03 · 2.27 du Plessis, from the resent link · 1280
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
Acting as Pending trainee
Home
My data rights
[title: Pieter du Plessis] Pieter du Plessis
Sign out
## Home
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Programme
Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1) DATA: the invited sub-speciality's curriculum
Invited by
Nolwazi Mbatha, Institutional admin, on 2026-10-19 DATA: IssuedAsRole, UsedOn
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital. If it is taking long, ask Nolwazi Mbatha.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at duplessis@kgk.wombat.local, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P03-duplessis-390.dc.html — P03 · 2.27 du Plessis, from the resent link · 390
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
## Home
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Programme
Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1) DATA: the invited sub-speciality's curriculum
Invited by
Nolwazi Mbatha, Institutional admin, on 2026-10-19 DATA: IssuedAsRole, UsedOn
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital. If it is taking long, ask Nolwazi Mbatha.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at duplessis@kgk.wombat.local, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P04-admin-issued-1280.dc.html — P04 · an Administrator-issued invitation: no Invited by, no ask (F3) · 1280 · Illustration
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
Acting as Pending trainee
Home
My data rights
[title: ‹a registrar's name›] ‹a registrar's name›
Sign out
## Home
Illustration
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Programme
Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1) DATA: the invited sub-speciality's curriculum
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at ‹a registrar's address›, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P04-admin-issued-390.dc.html — P04 · an Administrator-issued invitation: no Invited by, no ask (F3) · 390 · Illustration
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
## Home
Illustration
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Programme
Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1) DATA: the invited sub-speciality's curriculum
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at ‹a registrar's address›, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P05-sso-1280.dc.html — P05 · SSO-provisioned: no Programme, no Invited by, no ask (S5) · 1280 · Illustration
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
Acting as Pending trainee
Home
My data rights
[title: ‹a registrar's name›] ‹a registrar's name›
Sign out
## Home
Illustration
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at ‹a registrar's address›, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P05-sso-390.dc.html — P05 · SSO-provisioned: no Programme, no Invited by, no ask (S5) · 390 · Illustration
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
## Home
Illustration
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at ‹a registrar's address›, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P07-issuer-gone-1280.dc.html — P07 · the issuer erased or deactivated: no Invited by line (F3) · 1280 · Illustration
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
Acting as Pending trainee
Home
My data rights
[title: ‹a registrar's name›] ‹a registrar's name›
Sign out
## Home
Illustration
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Programme
Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1) DATA: the invited sub-speciality's curriculum
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at ‹a registrar's address›, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P07-issuer-gone-390.dc.html — P07 · the issuer erased or deactivated: no Invited by line (F3) · 390 · Illustration
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
## Home
Illustration
Pending trainee · Semester 2, 2026
### Awaiting admission
You are registered, and waiting to be admitted to the Paediatrics programme at Kgosi Kgari Teaching Hospital. Until then, nothing can be filed by you or about you.
Institution
Kgosi Kgari Teaching Hospital
Programme
Paediatrics / Paediatrics, on the Paediatric EPA Curriculum (11.1) DATA: the invited sub-speciality's curriculum
Who admits you
An Institutional admin at Kgosi Kgari Teaching Hospital.
#### When you are admitted
Your programme start date is set, and with it your training year.
You are signed out once, and sign in again as a Trainee.
Then, once your programme has started, you can log activities and follow your progress.
An email tells you, at ‹a registrar's address›, that it has happened. decided, T367
#### Meanwhile
You can check your account and your data rights.
Review your accountOpen My data rights

######## BOARD R3-P08-loading-1280.dc.html — P08 · loading (DashboardFrame's) · 1280 · Illustration
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
Acting as Pending trainee
Home
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## Home
Illustration
Pending trainee · Semester 2, 2026

######## BOARD R3-P08-loading-390.dc.html — P08 · loading (DashboardFrame's) · 390 · Illustration
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
## Home
Illustration
Pending trainee · Semester 2, 2026

######## BOARD R3-P09-load-error-1280.dc.html — P09 · a failed read (DashboardFrame's built words, M10's Alert) · focus on it · 1280 · Illustration
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
Acting as Pending trainee
Home
My data rights
[title: Lerato Molefe] Lerato Molefe
Sign out
## Home
Illustration
Pending trainee · Semester 2, 2026
Could not load your Home. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-P09-load-error-390.dc.html — P09 · a failed read (DashboardFrame's built words, M10's Alert) · focus on it · 390 · Illustration
Wombat
Acting as Pending trainee
MenuClose
[aria-label: Main]
## Home
Illustration
Pending trainee · Semester 2, 2026
Could not load your Home. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-T01-five-waiting-1280.dc.html — T01 · 2.28 five waiting, nobody admitted · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 5 waiting
Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name
| Email
| Invited as
| Registered DATA
| Actions
| Anele Dlamini
| dlamini@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Anele Dlamini] Admit…
| Pieter du Plessis
| duplessis@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Pieter du Plessis] Admit…
| Nomsa Mahlangu
| mahlangu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Nomsa Mahlangu] Admit…
| Lerato Molefe
| molefe@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Lerato Molefe] Admit…
| Sipho Ndlovu
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees
No current trainees
A registrar appears here once admitted.
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T01-five-waiting-768.dc.html — T01 · 2.28 five waiting, nobody admitted · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 5 waiting
Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name
| Email
| Invited as
| Registered DATA
| Actions
| Anele Dlamini
| dlamini@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Anele Dlamini] Admit…
| Pieter du Plessis
| duplessis@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Pieter du Plessis] Admit…
| Nomsa Mahlangu
| mahlangu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Nomsa Mahlangu] Admit…
| Lerato Molefe
| molefe@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Lerato Molefe] Admit…
| Sipho Ndlovu
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees
No current trainees
A registrar appears here once admitted.
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T01-five-waiting-390.dc.html — T01 · 2.28 five waiting, nobody admitted · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 5 waiting
Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name
| Email
| Invited as
| Registered DATA
| Actions
| Anele Dlamini
| dlamini@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Anele Dlamini] Admit…
| Pieter du Plessis
| duplessis@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Pieter du Plessis] Admit…
| Nomsa Mahlangu
| mahlangu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Nomsa Mahlangu] Admit…
| Lerato Molefe
| molefe@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Lerato Molefe] Admit…
| Sipho Ndlovu
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees
No current trainees
A registrar appears here once admitted.
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T02-one-admitted-1280.dc.html — T02 · 2.29 Molefe admitted: ActionResult directly above Dlamini's Admit… (S8: ?admitted=) · focus on it · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 4 waiting
Lerato Molefe admitted to the Paediatric EPA Curriculum (11.1), training year 4, from 2023-01-15.
Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name
| Email
| Invited as
| Registered DATA
| Actions
| Anele Dlamini
| dlamini@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Anele Dlamini] Admit…
| Pieter du Plessis
| duplessis@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Pieter du Plessis] Admit…
| Nomsa Mahlangu
| mahlangu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Nomsa Mahlangu] Admit…
| Sipho Ndlovu
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees 1 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Lerato Molefe
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T02-one-admitted-390.dc.html — T02 · 2.29 Molefe admitted: ActionResult directly above Dlamini's Admit… (S8: ?admitted=) · focus on it · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 4 waiting
Lerato Molefe admitted to the Paediatric EPA Curriculum (11.1), training year 4, from 2023-01-15.
Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name
| Email
| Invited as
| Registered DATA
| Actions
| Anele Dlamini
| dlamini@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Anele Dlamini] Admit…
| Pieter du Plessis
| duplessis@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Pieter du Plessis] Admit…
| Nomsa Mahlangu
| mahlangu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Nomsa Mahlangu] Admit…
| Sipho Ndlovu
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees 1 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Lerato Molefe
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T03-four-admitted-1280.dc.html — T03 · 2.29 four admitted, Ndlovu waiting; Mahlangu's result above Ndlovu's Admit… · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 1 waiting
Nomsa Mahlangu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15.
Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name
| Email
| Invited as
| Registered DATA
| Actions
| Sipho Ndlovu
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees 4 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Anele Dlamini
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2024-01-15Training year 3
| 2028-01-14
| [aria-label: Edit Anele Dlamini's profile] Edit
| Pieter du Plessis
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2025-01-15Training year 2
| 2029-01-14
| [aria-label: Edit Pieter du Plessis's profile] Edit
| Nomsa Mahlangu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Nomsa Mahlangu's profile] Edit
| Lerato Molefe
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T03-four-admitted-768.dc.html — T03 · 2.29 four admitted, Ndlovu waiting; Mahlangu's result above Ndlovu's Admit… · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 1 waiting
Nomsa Mahlangu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15.
Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name
| Email
| Invited as
| Registered DATA
| Actions
| Sipho Ndlovu
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees 4 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Anele Dlamini
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2024-01-15Training year 3
| 2028-01-14
| [aria-label: Edit Anele Dlamini's profile] Edit
| Pieter du Plessis
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2025-01-15Training year 2
| 2029-01-14
| [aria-label: Edit Pieter du Plessis's profile] Edit
| Nomsa Mahlangu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Nomsa Mahlangu's profile] Edit
| Lerato Molefe
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T03-four-admitted-390.dc.html — T03 · 2.29 four admitted, Ndlovu waiting; Mahlangu's result above Ndlovu's Admit… · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 1 waiting
Nomsa Mahlangu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15.
Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name
| Email
| Invited as
| Registered DATA
| Actions
| Sipho Ndlovu
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees 4 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Anele Dlamini
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2024-01-15Training year 3
| 2028-01-14
| [aria-label: Edit Anele Dlamini's profile] Edit
| Pieter du Plessis
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2025-01-15Training year 2
| 2029-01-14
| [aria-label: Edit Pieter du Plessis's profile] Edit
| Nomsa Mahlangu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Nomsa Mahlangu's profile] Edit
| Lerato Molefe
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T04-none-waiting-1280.dc.html — T04 · 2.30 Ndlovu admitted, completion derived; nobody waiting · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Sipho Ndlovu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15. Expected completion 2027-01-15, derived from the curriculum window: change it under Edit. NEW
Nobody is waiting to be admitted. NEW
A registrar appears here after registering from a Trainee invitation.
### Current trainees 5 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Anele Dlamini
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2024-01-15Training year 3
| 2028-01-14
| [aria-label: Edit Anele Dlamini's profile] Edit
| Pieter du Plessis
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2025-01-15Training year 2
| 2029-01-14
| [aria-label: Edit Pieter du Plessis's profile] Edit
| Nomsa Mahlangu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Nomsa Mahlangu's profile] Edit
| Lerato Molefe
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
| Sipho Ndlovu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2027-01-15
| [aria-label: Edit Sipho Ndlovu's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T04-none-waiting-768.dc.html — T04 · 2.30 Ndlovu admitted, completion derived; nobody waiting · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Sipho Ndlovu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15. Expected completion 2027-01-15, derived from the curriculum window: change it under Edit. NEW
Nobody is waiting to be admitted. NEW
A registrar appears here after registering from a Trainee invitation.
### Current trainees 5 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Anele Dlamini
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2024-01-15Training year 3
| 2028-01-14
| [aria-label: Edit Anele Dlamini's profile] Edit
| Pieter du Plessis
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2025-01-15Training year 2
| 2029-01-14
| [aria-label: Edit Pieter du Plessis's profile] Edit
| Nomsa Mahlangu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Nomsa Mahlangu's profile] Edit
| Lerato Molefe
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
| Sipho Ndlovu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2027-01-15
| [aria-label: Edit Sipho Ndlovu's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T04-none-waiting-390.dc.html — T04 · 2.30 Ndlovu admitted, completion derived; nobody waiting · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Sipho Ndlovu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15. Expected completion 2027-01-15, derived from the curriculum window: change it under Edit. NEW
Nobody is waiting to be admitted. NEW
A registrar appears here after registering from a Trainee invitation.
### Current trainees 5 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Anele Dlamini
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2024-01-15Training year 3
| 2028-01-14
| [aria-label: Edit Anele Dlamini's profile] Edit
| Pieter du Plessis
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2025-01-15Training year 2
| 2029-01-14
| [aria-label: Edit Pieter du Plessis's profile] Edit
| Nomsa Mahlangu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Nomsa Mahlangu's profile] Edit
| Lerato Molefe
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
| Sipho Ndlovu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2027-01-15
| [aria-label: Edit Sipho Ndlovu's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T05-after-profile-save-1280.dc.html — T05 · 2.30 back from his profile (flow 13's, unchanged): 2030-01-14 · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted. NEW
A registrar appears here after registering from a Trainee invitation.
### Current trainees 5 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Anele Dlamini
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2024-01-15Training year 3
| 2028-01-14
| [aria-label: Edit Anele Dlamini's profile] Edit
| Pieter du Plessis
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2025-01-15Training year 2
| 2029-01-14
| [aria-label: Edit Pieter du Plessis's profile] Edit
| Nomsa Mahlangu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Nomsa Mahlangu's profile] Edit
| Lerato Molefe
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
| Sipho Ndlovu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Sipho Ndlovu's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T05-after-profile-save-390.dc.html — T05 · 2.30 back from his profile (flow 13's, unchanged): 2030-01-14 · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted
Nobody is waiting to be admitted. NEW
A registrar appears here after registering from a Trainee invitation.
### Current trainees 5 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Anele Dlamini
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2024-01-15Training year 3
| 2028-01-14
| [aria-label: Edit Anele Dlamini's profile] Edit
| Pieter du Plessis
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2025-01-15Training year 2
| 2029-01-14
| [aria-label: Edit Pieter du Plessis's profile] Edit
| Nomsa Mahlangu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Nomsa Mahlangu's profile] Edit
| Lerato Molefe
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
| Sipho Ndlovu
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Sipho Ndlovu's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T06-admin-view-1280.dc.html — T06 · the Administrator (no menu item, nothing lit): the institution under each name · 1280 · Illustration
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
Home
Platform
Scheduled jobs
Audit log
SSO mappings
Data rights requests
Organisations
Institutions
Colleges
People
Users
Invitations
Catalogue
EPAs
Curricula
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
My data rights
[title: Demo Administrator] Demo Administrator
Sign out
[aria-label: Breadcrumb]
Home
Trainees
## Trainees
Illustration
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 1 waiting
Registered from a Trainee invitation at any institution, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name and institution
| Email
| Invited as
| Registered DATA
| Actions
| Sipho NdlovuKgosi Kgari Teaching Hospital
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees 4 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name and institution
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Anele DlaminiKgosi Kgari Teaching Hospital
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2024-01-15Training year 3
| 2028-01-14
| [aria-label: Edit Anele Dlamini's profile] Edit
| Pieter du PlessisKgosi Kgari Teaching Hospital
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2025-01-15Training year 2
| 2029-01-14
| [aria-label: Edit Pieter du Plessis's profile] Edit
| Nomsa MahlanguKgosi Kgari Teaching Hospital
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Nomsa Mahlangu's profile] Edit
| Lerato MolefeKgosi Kgari Teaching Hospital
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T06-admin-view-390.dc.html — T06 · the Administrator (no menu item, nothing lit): the institution under each name · 390 · Illustration
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
## Trainees
Illustration
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 1 waiting
Registered from a Trainee invitation at any institution, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name and institution
| Email
| Invited as
| Registered DATA
| Actions
| Sipho NdlovuKgosi Kgari Teaching Hospital
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees 4 current
Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.
| Name and institution
| Curriculum and sub-speciality
| Programme start NEW DATA: TrainingYear
| Expected completion
| Actions
| Anele DlaminiKgosi Kgari Teaching Hospital
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2024-01-15Training year 3
| 2028-01-14
| [aria-label: Edit Anele Dlamini's profile] Edit
| Pieter du PlessisKgosi Kgari Teaching Hospital
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2025-01-15Training year 2
| 2029-01-14
| [aria-label: Edit Pieter du Plessis's profile] Edit
| Nomsa MahlanguKgosi Kgari Teaching Hospital
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2026-01-15Training year 1
| 2030-01-14
| [aria-label: Edit Nomsa Mahlangu's profile] Edit
| Lerato MolefeKgosi Kgari Teaching Hospital
| Paediatric EPA Curriculum (11.1)Paediatrics
| 2023-01-15Training year 4
| 2027-01-14
| [aria-label: Edit Lerato Molefe's profile] Edit
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T07-t290-1280.dc.html — T07 · a Trainee-holding admin: the header and T290's note only (M1) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Illustration
Admit registrars who are waiting, and keep current trainees' programmes.
You hold the Trainee role, so you cannot view or change trainee profiles, including your own.

######## BOARD R3-T07-t290-390.dc.html — T07 · a Trainee-holding admin: the header and T290's note only (M1) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Trainees
Illustration
Admit registrars who are waiting, and keep current trainees' programmes.
You hold the Trainee role, so you cannot view or change trainee profiles, including your own.

######## BOARD R3-T08-loading-1280.dc.html — T08 · loading: "Loading the trainees." · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
Loading the trainees.

######## BOARD R3-T08-loading-390.dc.html — T08 · loading: "Loading the trainees." · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
Loading the trainees.

######## BOARD R3-T09-load-error-1280.dc.html — T09 · a failed read: the danger Alert in ActionResult (M10) · focus on it, after a failed retry too · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Illustration
Admit registrars who are waiting, and keep current trainees' programmes.
Could not load the trainees. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-T09-load-error-390.dc.html — T09 · a failed read: the danger Alert in ActionResult (M10) · focus on it, after a failed retry too · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Trainees
Illustration
Admit registrars who are waiting, and keep current trainees' programmes.
Could not load the trainees. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-T10-cancelled-1280.dc.html — T10 · Cancel on Admit Lerato Molefe: back, focus on her Admit… · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 5 waiting
Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name
| Email
| Invited as
| Registered DATA
| Actions
| Anele Dlamini
| dlamini@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Anele Dlamini] Admit…
| Pieter du Plessis
| duplessis@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Pieter du Plessis] Admit…
| Nomsa Mahlangu
| mahlangu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Nomsa Mahlangu] Admit…
| Lerato Molefe
| molefe@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Lerato Molefe] Admit…
| Sipho Ndlovu
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees
No current trainees
A registrar appears here once admitted.
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-T10-cancelled-390.dc.html — T10 · Cancel on Admit Lerato Molefe: back, focus on her Admit… · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
## Trainees
Admit registrars who are waiting, and keep current trainees' programmes.
### Waiting to be admitted 5 waiting
Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about them until they are admitted.
| Name
| Email
| Invited as
| Registered DATA
| Actions
| Anele Dlamini
| dlamini@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Anele Dlamini] Admit…
| Pieter du Plessis
| duplessis@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Pieter du Plessis] Admit…
| Nomsa Mahlangu
| mahlangu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Nomsa Mahlangu] Admit…
| Lerato Molefe
| molefe@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Lerato Molefe] Admit…
| Sipho Ndlovu
| ndlovu@kgk.wombat.local
| Paediatrics / Paediatrics
| 2026-10-19
| [aria-label: Admit Sipho Ndlovu] Admit…
### Current trainees
No current trainees
A registrar appears here once admitted.
### Completed and closed
DATA: flow 13’s list, unchanged

######## BOARD R3-M01-opened-1280.dc.html — M01 · 2.29 opened: today, Training year 1; the curriculum read out (F1, S18) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2026-10-19]
Training year NEW
Training year 1
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 1.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M01-opened-390.dc.html — M01 · 2.29 opened: today, Training year 1; the curriculum read out (F1, S18) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2026-10-19]
Training year NEW
Training year 1
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 1.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M02-start-chosen-1280.dc.html — M02 · 2.29 2023-01-15 chosen: Training year 4 on the date's change; 2027-01-14 · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2023-01-15]
Training year NEW
Training year 4
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.] [input: 2027-01-14]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 4.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M02-start-chosen-390.dc.html — M02 · 2.29 2023-01-15 chosen: Training year 4 on the date's change; 2027-01-14 · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2023-01-15]
Training year NEW
Training year 4
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.] [input: 2027-01-14]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 4.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M03-ndlovu-empty-1280.dc.html — M03 · 2.30 Ndlovu, the completion empty: what it will be · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Sipho Ndlovu
## Admit Sipho Ndlovu
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
ndlovu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2026-01-15]
Training year NEW
Training year 1
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window. Left empty, it will be 2027-01-15. DATA: derived date]
Leave this empty to derive it from the curriculum window. Left empty, it will be 2027-01-15. DATA: derived date
When you admit: NEW: SubmitCheck reused
Sipho Ndlovu becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 1.
Any session Sipho Ndlovu has open ends once, and Sipho Ndlovu signs in again as a Trainee.
An email tells Sipho Ndlovu so. decided, T367
CancelAdmit Sipho Ndlovu

######## BOARD R3-M03-ndlovu-empty-390.dc.html — M03 · 2.30 Ndlovu, the completion empty: what it will be · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Sipho Ndlovu
## Admit Sipho Ndlovu
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
ndlovu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2026-01-15]
Training year NEW
Training year 1
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window. Left empty, it will be 2027-01-15. DATA: derived date]
Leave this empty to derive it from the curriculum window. Left empty, it will be 2027-01-15. DATA: derived date
When you admit: NEW: SubmitCheck reused
Sipho Ndlovu becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 1.
Any session Sipho Ndlovu has open ends once, and Sipho Ndlovu signs in again as a Trainee.
An email tells Sipho Ndlovu so. decided, T367
CancelAdmit Sipho Ndlovu

######## BOARD R3-M04-nothing-adopted-1280.dc.html — M04 · nothing adopted for the invited sub-speciality: a standing warning, no form · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum.
This institution has adopted no curriculum yet, so no trainee can be admitted. Adopt one under Curriculum adoptions, then admit the trainee.
Back to trainees

######## BOARD R3-M04-nothing-adopted-390.dc.html — M04 · nothing adopted for the invited sub-speciality: a standing warning, no form · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum.
This institution has adopted no curriculum yet, so no trainee can be admitted. Adopt one under Curriculum adoptions, then admit the trainee.
Back to trainees

######## BOARD R3-M04b-no-institution-1280.dc.html — M04b · the Administrator: a registrar with no institution · 1280 · Illustration
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
Home
Platform
Scheduled jobs
Audit log
SSO mappings
Data rights requests
Organisations
Institutions
Colleges
People
Users
Invitations
Catalogue
EPAs
Curricula
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
My data rights
[title: Demo Administrator] Demo Administrator
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit ‹a registrar's name›
## Admit ‹a registrar's name›
Illustration
Admit a registrar who is waiting into the institution's curriculum.
This registrar belongs to no institution, so no curriculum can be offered. NEW
Back to trainees

######## BOARD R3-M04b-no-institution-390.dc.html — M04b · the Administrator: a registrar with no institution · 390 · Illustration
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit ‹a registrar's name›
## Admit ‹a registrar's name›
Illustration
Admit a registrar who is waiting into the institution's curriculum.
This registrar belongs to no institution, so no curriculum can be offered. NEW
Back to trainees

######## BOARD R3-M06-future-start-1280.dc.html — M06 · a start in the future: Not started (C4) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2027-01-15]
Training year NEW
Not started: the programme starts on 2027-01-15
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), whose programme starts on 2027-01-15. Nothing can be filed before then.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe the day the programme starts. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M06-future-start-390.dc.html — M06 · a start in the future: Not started (C4) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2027-01-15]
Training year NEW
Not started: the programme starts on 2027-01-15
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), whose programme starts on 2027-01-15. Nothing can be filed before then.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe the day the programme starts. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M07-five-years-1280.dc.html — M07 · a start more than four years back: Training year 5 and a warning (NEW) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[aria-describedby: This start is more than four years ago. Check the date before you admit. NEW] [input: 2022-01-15]
This start is more than four years ago. Check the date before you admit. NEW
Training year NEW
Training year 5
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 5.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M07-five-years-390.dc.html — M07 · a start more than four years back: Training year 5 and a warning (NEW) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[aria-describedby: This start is more than four years ago. Check the date before you admit. NEW] [input: 2022-01-15]
This start is more than four years ago. Check the date before you admit. NEW
Training year NEW
Training year 5
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 5.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M08-not-found-1280.dc.html — M08 · a userId not found or not waiting: the system's not-found panel (S20) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Registrar not found
## Registrar not found
No registrar waiting for admission has this link. PROPOSED
Those waiting are listed on Trainees.
Go to Trainees

######## BOARD R3-M08-not-found-390.dc.html — M08 · a userId not found or not waiting: the system's not-found panel (S20) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Registrar not found
## Registrar not found
No registrar waiting for admission has this link. PROPOSED
Those waiting are listed on Trainees.
Go to Trainees

######## BOARD R3-M09-already-admitted-1280.dc.html — M09 · a second Admit from a stale tab (S22) · focus on the refusal · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
Not admitted. Lerato Molefe already has an active trainee profile. Open Trainees NEW
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2023-01-15]
Training year NEW
Training year 4
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.] [input: 2027-01-14]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 4.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M09-already-admitted-390.dc.html — M09 · a second Admit from a stale tab (S22) · focus on the refusal · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
Not admitted. Lerato Molefe already has an active trainee profile. Open Trainees NEW
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2023-01-15]
Training year NEW
Training year 4
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.] [input: 2027-01-14]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 4.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M10-loading-1280.dc.html — M10 · loading: "Loading this registrar." · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Admit a registrar who is waiting into the institution's curriculum.
Loading this registrar.

######## BOARD R3-M10-loading-390.dc.html — M10 · loading: "Loading this registrar." · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Admit a registrar who is waiting into the institution's curriculum.
Loading this registrar.

######## BOARD R3-M11-load-error-1280.dc.html — M11 · a failed read (M10) · focus on it · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Could not load this registrar. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-M11-load-error-390.dc.html — M11 · a failed read (M10) · focus on it · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Could not load this registrar. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-M12-t290-1280.dc.html — M12 · a Trainee-holding admin: the header and Trainees' T290 note, before any read (M1) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
You hold the Trainee role, so you cannot view or change trainee profiles, including your own.

######## BOARD R3-M12-t290-390.dc.html — M12 · a Trainee-holding admin: the header and Trainees' T290 note, before any read (M1) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
You hold the Trainee role, so you cannot view or change trainee profiles, including your own.

######## BOARD R3-M13-deactivated-1280.dc.html — M13 · S6 a deactivated pending account · focus on the refusal · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
Not admitted. Lerato Molefe's account is deactivated. Reactivate it under Users first. NEW
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2023-01-15]
Training year NEW
Training year 4
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.] [input: 2027-01-14]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 4.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M13-deactivated-390.dc.html — M13 · S6 a deactivated pending account · focus on the refusal · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
Not admitted. Lerato Molefe's account is deactivated. Reactivate it under Users first. NEW
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2023-01-15]
Training year NEW
Training year 4
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.] [input: 2027-01-14]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 4.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M14-no-scope-1280.dc.html — M14 · F1's one select: an SSO registrar with no invited scope; Invited as "—" (S5) · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit ‹a registrar's name›
## Admit ‹a registrar's name›
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
‹a registrar's address›
Institution
Kgosi Kgari Teaching Hospital
Invited as
— DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum *
[aria-describedby: This registrar was invited to no sub-speciality, so choose from the curricula Kgosi Kgari Teaching Hospital has adopted. Admitting sets the registrar's scope from it. NEW]
[selected] Choose a curriculum
Paediatric EPA Curriculum (11.1)
This registrar was invited to no sub-speciality, so choose from the curricula Kgosi Kgari Teaching Hospital has adopted. Admitting sets the registrar's scope from it. NEW
Programme start *
[input: 2026-10-19]
Training year NEW
Training year 1
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
‹a registrar's name› becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 1.
Any session ‹a registrar's name› has open ends once, and ‹a registrar's name› signs in again as a Trainee.
An email tells ‹a registrar's name› so. decided, T367
CancelAdmit ‹a registrar's name›

######## BOARD R3-M14-no-scope-390.dc.html — M14 · F1's one select: an SSO registrar with no invited scope; Invited as "—" (S5) · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit ‹a registrar's name›
## Admit ‹a registrar's name›
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
‹a registrar's address›
Institution
Kgosi Kgari Teaching Hospital
Invited as
— DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum *
[aria-describedby: This registrar was invited to no sub-speciality, so choose from the curricula Kgosi Kgari Teaching Hospital has adopted. Admitting sets the registrar's scope from it. NEW]
[selected] Choose a curriculum
Paediatric EPA Curriculum (11.1)
This registrar was invited to no sub-speciality, so choose from the curricula Kgosi Kgari Teaching Hospital has adopted. Admitting sets the registrar's scope from it. NEW
Programme start *
[input: 2026-10-19]
Training year NEW
Training year 1
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Leave this empty to derive it from the curriculum window.]
Leave this empty to derive it from the curriculum window.
When you admit: NEW: SubmitCheck reused
‹a registrar's name› becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 1.
Any session ‹a registrar's name› has open ends once, and ‹a registrar's name› signs in again as a Trainee.
An email tells ‹a registrar's name› so. decided, T367
CancelAdmit ‹a registrar's name›

######## BOARD R3-M15-derived-passed-1280.dc.html — M15 · F4: start 2023-01-15, completion empty, after Admit: refused beside Expected completion · focus on it · 1280 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
Home
People
Invitations
Trainees
Assessors
Users
Curriculum
Curriculum adoptions
Curricula
EPAs
Activity types
Entrustment scales
Reviews
Decisions due
Committee reviews
Decision panels
Access and audit
SSO mappings
Audit log
My data rights
[title: Nolwazi Mbatha] Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2023-01-15]
Training year NEW
Training year 4
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Left empty, it would be 2024-01-15, which has passed. Enter the expected completion. NEW Enter the expected completion: left empty, it would be 2024-01-15, which has passed. NEW] [aria-invalid]
Left empty, it would be 2024-01-15, which has passed. Enter the expected completion. NEW
Enter the expected completion: left empty, it would be 2024-01-15, which has passed. NEW
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 4.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-M15-derived-passed-390.dc.html — M15 · F4: start 2023-01-15, completion empty, after Admit: refused beside Expected completion · focus on it · 390 · Illustration
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
[aria-label: Breadcrumb]
Home
Trainees
Admit Lerato Molefe
## Admit Lerato Molefe
Illustration
Admit a registrar who is waiting into the institution's curriculum. PROPOSED: route /admin/trainees/admit
### Who is being admitted
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Invited as
Paediatrics / Paediatrics DATA
Registered
2026-10-19 DATA
### Programme
* marks a field you must fill in.
Curriculum
Paediatric EPA Curriculum (11.1)
Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.
Programme start *
[input: 2023-01-15]
Training year NEW
Training year 4
Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.
Expected completion
[aria-describedby: Left empty, it would be 2024-01-15, which has passed. Enter the expected completion. NEW Enter the expected completion: left empty, it would be 2024-01-15, which has passed. NEW] [aria-invalid]
Left empty, it would be 2024-01-15, which has passed. Enter the expected completion. NEW
Enter the expected completion: left empty, it would be 2024-01-15, which has passed. NEW
When you admit: NEW: SubmitCheck reused
Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 4.
Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.
An email tells Lerato Molefe so. decided, T367
CancelAdmit Lerato Molefe

######## BOARD R3-X01-for-flow-12-1280.dc.html — X01 · for flow 12: Q8's line and Q1's retarget (PROPOSED)
## For flow 12 · PROPOSED · not a Home redesign
Q8: a line for flow 12 to place on the Institutional admin's Home while anyone waits. No mail.
### DATA: flow 12’s card
Waiting for admission: 5Opens Trainees
Q1: Home's header action "Invite a person" (HomeFrame.cs:51) opens /admin/invitations/new instead of the list (1.10, 2.44). Only its target changes.
