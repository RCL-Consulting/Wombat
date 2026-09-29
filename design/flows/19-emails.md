# F19 Wombat's emails: what each mail tells its reader, and where its link lands

Fifteen email templates carry Wombat outside the app: an invitation to register, a request for a colleague's feedback,
a nudge about waiting work, a weekly digest, a graduation. Nine of them have a sender in the code and six have none,
and all fifteen share one plain shell. This flow designs that shell once, then each message in it, in HTML and in plain text,
together with the notices that T319 and T320 will add.

Part of the GUI redesign briefed in `design/BRIEF.md` (W-008, T336). Written 2026-09-27.

| | |
|---|---|
| **People** | **Readers:** the invitees of Acts 1–2 (Dr Kruger, Prof Mbatha, nine staff and the five registrars); MSF respondents, who have no account (Dr Kagiso Motsepe, Dr Lindiwe Khoza and five other colleagues of Dr Molefe, Act 3); the assessors Dr Zulu, Dr Patel, Dr Khumalo and Dr Naidoo; the Coordinator Mr Pieter Smit; the trainees Dr Dlamini, Dr Mahlangu and Dr Ndlovu; the graduate Dr Molefe. **Causes:** the platform operator and Prof Mbatha (invitations), Mr Smit (MSF), the five scheduled jobs, and Mark complete |
| **Frequency and stakes** | **Invitations and feedback requests** go once per person, and the stakes are high. A mail that looks like spam, or does not say who is asking, stops onboarding or a campaign, and its link is a one-time secret. **Nudges** go daily and **the digest** weekly. Each is low stakes, but together they are the product's only voice outside the app. **Graduation** goes once, and today it says something untrue (T311) |
| **Mode** | **Wireframe first** for the shared shell (header, title, body, action, link fallback, facts, footer) and its plain-text twin. Then **fidelity** for each message in the chosen shell |
| **Templates** | `src/Wombat.Application/Common/Email/Templates/`: 15 `*Email.cs` files and the shell, `EmailTemplateBase.cs` |
| **Baseline** | `design/baseline/mail/`, which is gitignored. It holds `<Template>.png` (the HTML body at 600 px), `<Template>.html` and `<Template>.txt` for each template, plus three variants. Captured 2026-09-27 (T336); § The inventory says where each came from |
| **Held screenshots** | None. Every one-time token in the captures is replaced by a placeholder of the real token's length (§ Attach) |
| **Depends on** | F01's tokens and wordmark. A mail has no stylesheet, so each token becomes a literal value (§ Constraints) |

**How to run this thread** (BRIEF § 2.3, step 4; source keys are BRIEF § 2.0's):
1. Run it once the pilot (F01) has settled the tokens and the type. It needs nothing else from the screen flows.
2. Stage the upload with `pwsh design/tools/stage_upload.ps1 -Flow 19` (once this file is committed: the script stages
   tracked briefs only). It keeps back in `design/upload/crop-first/` every file whose name holds invit, issued,
   resent, resend, reset or MsfExpiryReminder (BRIEF § 3.3): 14 files on 2026-09-27.
   - **The mail captures there** (`mail/InvitationEmail.*`, `mail/MsfInvitationEmail*`, `mail/MsfExpiryReminderEmail.png`,
     `mail/PasswordResetEmail.png`) carry only placeholders. Open each one and move it back.
   - **Two screen captures there show a full registration link** (observed): `act-2/2.16-1-registrars-invited.png`
     and `act-1/1.7-1-mbatha-invited.png`. Crop the link out before you upload either, or leave it out.
   - **The other three** (`states/invitations-list--not-delivered.png`, `act-3/3.43-2-link-resent.png` and
     `act-4/4.27-1-zulu-ratified-stars-issued.png`) show no link (observed). Open each and move it back.
3. Open a new thread. Attach the KEY SCREENSHOTS (§ Attach, first list), then paste **The ask**.
4. Paste **The runbook steps, verbatim** (the last section) as the next message: "Walk Claude through the journey and it
   will generate each screen in context" [academy].
5. Round 1 is the shell as wireframes ("Wireframe first when fidelity doesn't matter" [parrott]). Pick one, then ask
   for round 2.
6. Write every decision into the chat as a sentence; the chat travels in the handoff bundle [academy]. Export the chosen
   artboards and the HTML to `design/flows/19-emails/` before moving on: there is no version history [start].

## The ask

Paste this block as the thread's first message.

```text
FLOW 19 — Wombat's emails: every message a person gets from Wombat, as one family, in HTML and in plain text.

GOAL: Each mail tells its reader who is writing, why they are getting it, what happened or what is asked, and what
  to do next, with one action that lands on the page where it is done. It must read the same in a desktop client, on
  a phone and as plain text, and a one-time link must be obvious and copyable. What is wrong today:
  - every mail is the same plain card. It has no wordmark and no institution. Its button is a bright blue (#2563eb)
    that is not a Wombat colour, its font is Arial, and its only footer is "This is an automated message from Wombat.
    Please do not reply to this email." Nothing says why the reader is getting it or how to stop it;
  - four mails that ask the reader to act carry no link at all: the assessor's nudge, the draft reminder, the weekly
    digest and the graduation mail ("Please log in to Wombat");
  - the graduation mail claims "Your committee has ratified your final entrustment decisions", which Mark complete
    never checks (T311);
  - the invitation names the role by its code ("as InstitutionalAdmin", "as CollegeAdmin"). It does not say who
    invited the person, or to which institution or College;
  - the digest's section headings render larger than the mail's own title;
  - six templates are sent by nothing. Five belong to activity moves and committee outcomes that Wombat does not mail
    (T320), and one of those still uses the retired "STAR reflection" meaning. One belongs to a password reset that is
    not built. Three more have a sender that the story never triggers: the two STAR expiry notices and the MSF
    reminder;
  - a revoked STAR and a decided data-rights request are promised a mail on screen, and get none (T319).

AUDIENCE: Invitees (a College registrar, a head of department, consultants, registrars). MSF respondents: nurses,
  physiotherapists, peer registrars and consultants who have no Wombat account and may never have heard of it.
  Assessors, who are busy consultants. The programme coordinator. Registrars, and a graduate. They read in a desktop
  client (a 600 px reading pane), on a phone (390 px) and, some of them, in plain text only.

LAYOUT: One shell for every message, table-based and at most 600 px wide:
  - an inbox preheader (NEW);
  - a header with the wordmark, and the institution or College where there is one;
  - a title;
  - the body;
  - at most one primary action, with the link written out under it;
  - an optional facts block (label: value);
  - a footer that says why the reader got this mail. On a periodic mail the footer also says how to stop it.
  The plain-text twin follows the same order.

CONTENT (use these real scenario values):
  - Kgosi Kgari Teaching Hospital (KGK); the College of Paediatricians of South Africa (CPSA); Paediatric EPA
    Curriculum 11.1. EPAs are PAED-001 to PAED-015, for example PAED-010 "Leading and operating within a clinical
    team".
  - Invitation: Dr Lerato Molefe, invited by Prof Nolwazi Mbatha as a Trainee at KGK, link expires 2026-10-10.
  - MSF: "Feedback request: Lerato Molefe (Default MSF, 2026-09-26 to 2026-10-10)" to Dr Kagiso Motsepe, a peer
    doctor; last day to respond 2026-10-10; seven respondents; Dr Lindiwe Khoza's link resent.
  - Nudge: "Hi Thandi", Mini-CEX (Paediatrics) from Nomsa Mahlangu, waiting 8 days.
  - Draft reminder: "Hi Nomsa", Mini-CEX (Paediatrics), draft for 15 days.
  - Digest to Mr Pieter Smit: empty ("No items requiring attention this week."), or trainee at risk Sipho Ndlovu,
    campaign "Default MSF (campaign #1)", review "Anele Dlamini on 2026-12-07".
  - Graduation: Dr Molefe, programme complete on 2026-09-26.
  - STAR expiry: PAED-010 at level 4, expires 2026-10-16.
  - Activity moves: Dr Dlamini's Mini-CEX (Paediatrics) on PAED-001, requested of Dr David Naidoo, then completed;
    Dr Ndlovu's Mini-CEX declined by Dr Fatima Khumalo.
  - Links are on http://localhost:5080 in the baseline. In production they are on the configured base URL.

SCREENS (the messages), in order:
  1. The shell: desktop 600 px, phone 390 px, images off, and the plain-text twin. Show it with InvitationEmail (one
     action and a one-time link) and with the digest (lists and no single action).
  2. InvitationEmail: the first issue, and a Resend that replaces the link (NEW wording: "This replaces the link sent
     on …"). A staff role and a registrar. The role named by its label, the inviter, the institution or College, and
     the expiry.
  3. MsfInvitationEmail: MSF, and learner feedback (T164: "feedback on the teaching of …"). The resent link.
     MsfExpiryReminderEmail, sent two days before the last day.
  4. The nudges: AssessorPendingNudgeEmail and DraftNudgeEmail, with one item and with twelve, each item linked.
  5. CoordinatorDigestEmail: empty, typical and heavy (40 trainees at risk). Each list linked to its page, with the
     opt-out in the footer.
  6. GraduationEmail, in T311's wording, linked to My progress.
  7. The STAR notices: EntrustmentDecisionExpiringReminderEmail (20 days left) and EntrustmentDecisionExpiredEmail,
     each linked to My authorisations; NEW: a STAR revoked (T319).
  8. NEW, the notices T319 and T320 will add:
     - one activity-move mail that prints the pinned workflow's labels (Requested, to the assessor; Completed,
       Declined or Returned, to the trainee), replacing the four Assessment* templates;
     - a review outcome (ratified, and each STAR issued by EPA and level), replacing StarDecisionEmail;
     - an appeal lodged (to the appeal body's chair), and an appeal resolved (to the trainee);
     - a data-rights decision (Approved, Completed or Rejected, with no note);
     - an erasure carried out, with no link, because the reader can no longer sign in.
  9. PasswordResetEmail: only if Q7 keeps it, as a clearly marked future state (BRIEF § 7 B10).

STEPS: 1.5, 2.16, 2.17, 2.26, 3.3, 3.11, 3.32, 3.36, 3.43, 4.27, 4.36, 5.17, 5.18, A.1.3, A.1.12, A.2.3, A.2.4, A.2.6,
  A.2.8 and A.4.4 (20 steps), pasted verbatim in my next message (Role / Route / Do / Expect). They are the moments a
  mail is sent, or should be.

STATES TO SHOW: images off; a client that ignores <style> (so inline styles carry the design); a dark-mode client that
  inverts colours; a long name and a long address that must wrap ("m2-long-address-for-wrapping-checks-at-narrow-
  widths@paediatric-teaching-hospital-department-of-child-health.example.test"); an expired or replaced link, where
  the page the link opens says so (F10, F11). Data volumes: one item, typical, heavy.

REQUIREMENTS FROM KNOWN DEFECTS (each is testable):
  - T311: the graduation mail says only that the programme was marked complete on that day, and that her record stays
    in Wombat (My progress, her STAR certificates, the portfolio export). It links to My progress, and it has no
    "ratified" sentence.
  - T319: a revoked STAR mails the trainee once. The mail names the EPA, the level, the day and who revoked it, and
    links to My authorisations. It holds no part of the reason. A data-rights decision mails the requester once, with
    the request type, its date and the outcome, and no note. An erasure's confirmation carries no link.
  - T320: one activity-move template prints the pinned workflow's state and move labels, never keys. It replaces
    AssessmentRequested, Accepted, Completed and Declined. StarDecisionEmail is replaced by a review-outcome template
    and an appeal template in today's vocabulary: a STAR is a Statement of Awarded Responsibility, issued by
    ratification and never "declined".
  - T101, T319, T320: no free text written about a person goes into a mail: no decline note, revocation reason,
    decision note, rationale or form answer. The mail names the event, the instrument, the EPA and who acted, and
    links to the page where the text can be read.
  - T324: roles, states and instruments by their labels ("Institutional administrator", not "InstitutionalAdmin").
  - T325: every date is on the South African calendar. A time, if one is shown, carries its zone.
  - T240, D50: every periodic mail (the digest, the assessor's nudge, the draft reminder) says in its footer that it
    can be turned off under Data rights, "Opt out of digest emails", and links there. A one-off mail does not offer
    it, because it is always sent (the Data rights page's own help text says so).
  - Every mail that asks for an action links to the page where it is done: /activities/inbox, /activities/mine,
    /activities/{ActivityId:int}, /msf/respond, /account/register, /portfolio/authorisations, /portfolio/progress,
    /account/data-rights.
  - Contrast: the footer text is 3.5:1 today (#888 on white). Body text 4.5:1, a button 3:1 against its surround.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Whose voice is it? The Wombat wordmark alone, or the institution as sender ("Kgosi Kgari Teaching Hospital, via
     Wombat")? The data model holds an institution brand (logo, two colours), and nothing draws it.
  2. Images: is the header text only, or a hosted image of the mark? Many clients block remote images until asked.
  3. Reply: keep "Please do not reply", or give a person to reply to (the coordinator, the inviter)?
  4. Subjects: what does each subject carry for someone scanning an inbox? Today the MSF subjects name the trainee, the
     window and the due date; the nudges' and digest's subjects name nothing.
  5. Should an admission, a withdrawal (Deactivate, Step 5.27: "is not emailed") or a return for more detail
     be mailed? Today none is.
  6. Dates: the product prints yyyy-MM-dd; the certificate prints "16 October 2026". Which does a mail use?
  7. Password reset: design it as future, or leave it out and delete the template (T320 leaves the choice)?

CONSTRAINTS:
WOMBAT CONSTRAINTS FOR EMAIL (this flow's own; BRIEF.md § 5.1 governs the screens, and only its Content and
  Accessibility lines carry over)
Built by: C# string templates, one static Build(...) per message returning an EmailMessage (To, Subject, HtmlBody,
  TextBody, Tags). No template engine and no Razor. MailKit sends the HTML and the text as multipart/alternative.
  A design comes back as HTML that Claude Code rewrites as a string template, with every value HTML-encoded.
HTML: email-client HTML, not web HTML. Layout in nested tables (role="presentation"), 600 px wide at most and fluid
  below it. Inline styles; a <style> block may add a media query, but nothing may depend on it. No web fonts, no
  remote images unless Q2 decides one, no scripts, no forms, no background images, no CSS variables, no flexbox or
  grid. A button is a table cell holding a link, with the same link written out under it.
Colours: literal values taken from app.css :root, each named by the token it came from (F01's token sheet once the
  pilot lands). The font is a system stack.
Plain text: every message has a text twin that carries everything the HTML does, in the same order: the same words,
  each link on its own line, lists as "  - ". Design it too.
Links: every link is absolute, built from the configured base URL (Wombat:BaseUrl; MSF links from
  Wombat:MsfRespondUrl, which must sit on that host). A registration, MSF or reset link is a one-time secret: it
  appears only in its own recipient's mail.
Content: people by name; roles, states and instruments by their labels; dates on the South African calendar; no free
  text written about a person; an MSF respondent is never named to the trainee, and the trainee is named to the
  respondent (T202).
Tone: plain and specific. One action per mail, no marketing, no exclamation marks. Say what happened, who did it, what
  to do and by when.
Accessibility: lang="en" on the html element; text 4.5:1, a button 3:1 against its surround; a heading order a screen
  reader can walk; link text that says where it goes; readable with images off and as plain text.
Viewports: a 600 px desktop reading pane, a 390 px phone, and plain text.

ASK:
  - Round 1, WIREFRAME, 2–3 variations of the shell (screen 1), each shown with InvitationEmail and the digest, at
    600 px, at 390 px and as plain text, each with its reasoning. Stop for my pick.
  - Round 2, FULL FIDELITY in the picked shell: every message in SCREENS 2–9, as email-client HTML with its plain-text
    twin, and a table of every colour used and the app.css token it came from.
  - Mark everything NEW to Wombat. Review each message for contrast and reading order, and show it with images off.
  - Ask me before assuming anything the steps leave open.

ATTACHED: mail/InvitationEmail.png, mail/MsfInvitationEmail.png, mail/AssessorPendingNudgeEmail.png,
  mail/CoordinatorDigestEmail-items.png, mail/GraduationEmail.png, mail/EntrustmentDecisionExpiringReminderEmail.png,
  mail/AssessmentDeclinedEmail.png, mail/StarDecisionEmail.png, act-2/2.16-1-registrars-invited.png (its
  registration link cropped out), states/entrustment-decisions--revoke-form.png, states/data-rights--submitted.png.
  The mails show today's templates, defects included; they are not the target. Their links carry placeholders.
```

## The inventory

Every template in `src/Wombat.Application/Common/Email/Templates/`, checked against the code on 2026-09-27 (`grep
'<Template>.Build'` over `src`). All fifteen wrap their body in `EmailTemplateBase.WrapHtml` (`:10`), and each returns
a text twin. **Sent** means that something in `src` calls it. **In the story** means that the T295 replay's SMTP sink
received it: 34 mails on 2026-09-26, the story's day. The sink's `.eml` files are kept outside the repo, and each
baseline capture names its file.

| # | Template | Sent by | When | To | Subject today | Link | Sent | In the story | Baseline source |
|---|---|---|---|---|---|---|---|---|---|
| 1 | `InvitationEmail` | `IssueInvitation.cs:118`, `ResendInvitation.cs:148` | Issue invitation or Resend on `/admin/invitations` | the invitee | "Your Wombat invitation" | `/account/register?token=…`, built by `InvitationLinks.RegistrationUrl` (`IssueInvitation.cs:133-141`); expires in 14 days | yes | 17: Steps 1.5, 1.7, 2.2–2.7, 2.16 and 2.17 | Real: Step 2.16 to Dr Molefe as Trainee (sink `20260926-114607-029.eml`) |
| 2 | `MsfInvitationEmail` | `OpenMsfCampaign.cs:103`, `ResendMsfLinks.cs:133` | Open campaign; Resend links on `/msf/campaigns/{CampaignId:int}` | each respondent (no account) | "Feedback request: Lerato Molefe (Default MSF, 2026-09-26 to 2026-10-10)" | `/msf/respond?token=…` from `Wombat:MsfRespondUrl`, refused when unset (`WombatOptionsExtensions.cs:35`) | yes | 8: Step 3.36 (7) and 3.43 (1, resent) | Real: Step 3.36 to Dr Motsepe (`…124727-037`). Variant `-resent`: Step 3.43 to Dr Khoza (`…125233-044`), identical but for the token. Variant `-learner`: rendered, about Dr du Plessis's teaching (the story opens no learner campaign) |
| 3 | `MsfExpiryReminderEmail` | `MsfInvitationExpiryReminderJob.cs:126` | Daily at 08:00 UTC, once per respondent, two days before their last day | an unanswered respondent | "Reminder: feedback on Lerato Molefe (Default MSF) is due by 2026-10-10" | a new `/msf/respond` link; the first still works (T214) | yes | 0: every link was answered or resent before a reminder was due | Rendered: to Dr Khoza, due 2026-10-10 |
| 4 | `AssessorPendingNudgeEmail` | `AssessorPendingNudgeJob.cs:170` | Daily at 09:00 UTC, for work waiting more than 5 days on a `field:` nominee | the nominee | "Activities awaiting your assessment" | none | yes | 4: Steps 3.32 and A.2.8 | Real: Step 3.32, "Hi Thandi" (`…124322-036`) |
| 5 | `DraftNudgeEmail` | `ActivityDraftNudgeJob.cs:98` | Daily at 07:00 UTC, for drafts untouched 14 days | the draft's subject | "You have draft activities waiting" | none | yes | 1: Step A.2.8 | Real: Step A.2.8, "Hi Nomsa" (`…153640-049`) |
| 6 | `CoordinatorDigestEmail` | `WeeklyCoordinatorDigestJob.cs:130` | Mondays at 08:00 UTC, or Run now | each Coordinator, except one who opted out, is deactivated, has no institution or holds Trainee | "Your weekly Wombat digest" | none | yes | 3: Step A.2.6 to Mr Smit, and two to the Demo Institution's coordinator | Real: Step A.2.6, empty (`…153421-048`). Variant `-items`: rendered with all three lists |
| 7 | `GraduationEmail` | `CompleteTraineeProfile.cs:100` | Mark complete on `/admin/trainees/edit` | the graduate | "Congratulations on completing Paediatric EPA Curriculum" | none | yes | 1: Step 5.18 | Real: Step 5.18 (`…141525-045`) |
| 8 | `EntrustmentDecisionExpiringReminderEmail` | `EntrustmentDecisionExpiryJob.cs:93` | Daily at 03:30 UTC, for an active STAR within 30 days of its expiry | the trainee | "Entrustment decision expiring soon: PAED-010" | none | yes | 0: PAED-010's expiring STAR (Step 4.35, `D+20`) was superseded the same day, before the job's next run | Rendered: Dr Molefe, PAED-010, 2026-10-16, 20 days |
| 9 | `EntrustmentDecisionExpiredEmail` | `EntrustmentDecisionExpiryJob.cs:74` | The same job, once a STAR is past its expiry | the trainee | "Entrustment decision expired: PAED-010" | none | yes | 0: no STAR in the story reaches its expiry | Rendered: PAED-010, expired 2026-10-16 |
| 10 | `PasswordResetEmail` | nothing | Self-service reset is not built (Step A.4.4; BRIEF § 7 B10) | — | "Reset your Wombat password" | a reset link to a page that does not exist | no | — | Rendered with a placeholder link |
| 11 | `AssessmentRequestedEmail` | nothing (T320) | Would be Submit, Draft → Requested (Step 3.3) | the named assessor | "New assessment request" | `/activities/{ActivityId:int}` | no | — | Rendered: to Dr Naidoo, activity 1 |
| 12 | `AssessmentAcceptedEmail` | nothing; no CPSA workflow has an accepted state (T320) | — | the trainee | "Assessment request accepted" | `/activities/{ActivityId:int}` | no | — | Rendered: activity 1 |
| 13 | `AssessmentCompletedEmail` | nothing (T320) | Would be Complete (Step 3.5) | the trainee | "Assessment completed" | `/activities/{ActivityId:int}` | no | — | Rendered: to Dr Dlamini, activity 1 |
| 14 | `AssessmentDeclinedEmail` | nothing (T320) | Would be Decline (Step 3.11) | the trainee | "Assessment request declined" | none | no | — | Rendered: to Dr Ndlovu, with Dr Khumalo's note as the reason, which T320 forbids in mail |
| 15 | `StarDecisionEmail` | nothing; the retired "STAR reflection" meaning (T296, T320) | Would be Ratify (Step 4.27) | the trainee | "STAR reflection approved" | none | no | — | Rendered: approved, no comments |

**How the rendered ones were made.** The throwaway renderer loaded `Wombat.Application.dll` from `.scenario-app/bin`,
published on 2026-09-26 after the templates' last change (`16574700`, 2026-09-25). It called each `Build` with the
story's values above and was deleted afterwards. Each capture is the HTML body opened in Chrome at 600 px wide, as
the whole page.

**What every mail shares** (`EmailTemplateBase.cs`):
- **Structure.** A `<div class="wrapper">` card: max 600 px, white on #f4f4f4, radius 6 px, 32 px padding. Its `<h1>`
  repeats the subject, the body follows, and the footer is fixed.
- **Styles.** They sit in a `<style>` block in the head (`:17-24`): Arial; the title #1a1a2e at 1.25rem; body #333;
  `a.btn` #2563eb with white text; the footer #888 at 0.8rem.
- **What is missing.** No table layout, no preheader, no `lang` beyond `<html lang="en">`, no image, no institution.
- **The sender** is `EmailSettings.FromName`, "Wombat", at a configured address: `noreply@wombat.local` in the sink.
- **With `Email:SmtpHost` unset,** `LoggingEmailSender` writes the subject and text body to the log instead, with no
  address (T282).

## The journey

One line per step, in play order: who causes the mail, who reads it, and what it must let them see or do. The full
steps are in the last section.

| Step | Mail | Cause → reader | What the reader must be able to see or do |
|---|---|---|---|
| 1.5 | `InvitationEmail` | the operator → Dr Kruger | That the CPSA invites him to administer its catalogue in Wombat, the role by its label, the link, and when it expires |
| 2.16, 2.17 | `InvitationEmail` | Prof Mbatha → the five registrars | That KGK invites them as registrars, and who at KGK invited them. The mistyped address's link is revoked, and a mail to the right address follows |
| 2.26 | `InvitationEmail` (resent) | Prof Mbatha → Dr du Plessis | That this link replaces an earlier one, which no longer works |
| 3.3 | none today; the activity-move mail (T320) | Dr Dlamini → Dr Naidoo | That Dr Dlamini asks him to assess a Mini-CEX on PAED-001 of 2026-09-16, with a link to it |
| 3.11 | none today; the activity-move mail (T320) | Dr Khumalo → Dr Ndlovu | That his Mini-CEX was declined and by whom, with a link to read her note. No note in the mail |
| 3.32 | `AssessorPendingNudgeEmail` | the job → Dr Zulu, Dr Patel | What has waited on them, from whom and for how long, each item linked |
| 3.36 | `MsfInvitationEmail` | Mr Smit → seven colleagues | Whom the feedback is about, the questionnaire, the window, the last day, the one-time link, and what the trainee will and will not see |
| 3.43 | `MsfInvitationEmail` (resent) | Mr Smit → Dr Khoza | The same, with a link that works where the lost one would have |
| 4.27 | none today; the review-outcome mail (T320) | Dr Zulu → Dr Molefe | That her review was ratified, and which STARs were issued at which level, with a link to My authorisations |
| 4.36 | none today; the STAR-revoked mail (T319) | Dr Mokoena → Dr Dlamini | That her PAED-002 STAR was revoked, when and by whom, and where the certificate states the reason |
| 5.17, 5.18 | `GraduationEmail` | Prof Mbatha → Dr Molefe | Only what is true (T311), and where her record stays |
| A.1.3, A.1.12 | none today; the data-rights mails (T319) | Mr Smit, devadmin → Dr Dlamini, Dr Ndlovu | The decision on the request; for the erasure, a confirmation with no link |
| A.2.3, A.2.4, A.2.6 | `CoordinatorDigestEmail` | the job → Mr Smit | Once he opts out, no digest. After he opts back in, the week's lists, and how to opt out again |
| A.2.8 | `DraftNudgeEmail`, `AssessorPendingNudgeEmail` | the jobs → Dr Mahlangu, Dr Khumalo | The stale draft, and the request waiting 6 days, each linked |
| A.4.4 | `PasswordResetEmail` (not sent) | — | Nothing today: the reset page is a stub (Q7) |

## States to design

19 captures, in `design/baseline/mail/`. Each is a `.png` (the HTML body at 600 px), with its `.html` and `.txt` beside
it.

| Capture | What it shows | Source |
|---|---|---|
| `mail/InvitationEmail.png` | An invitation to register as Trainee, with the link written out | Real, Step 2.16 |
| `mail/MsfInvitationEmail.png` | An MSF request: the trainee, the questionnaire, the window, the last day, the anonymity promise | Real, Step 3.36 |
| `mail/MsfInvitationEmail-resent.png` | The resent link: identical to the first but for the token, so it does not say that it replaces anything | Real, Step 3.43 |
| `mail/MsfInvitationEmail-learner.png` | A learner-feedback request, "feedback on the teaching of Pieter du Plessis" (T164) | Rendered |
| `mail/MsfExpiryReminderEmail.png` | The reminder two days before the last day, with a new link and the two-links sentence (T214) | Rendered |
| `mail/AssessorPendingNudgeEmail.png` | One waiting activity, no link | Real, Step 3.32 |
| `mail/DraftNudgeEmail.png` | One stale draft, no link | Real, Step A.2.8 |
| `mail/CoordinatorDigestEmail.png` | The empty digest: "No items requiring attention this week." | Real, Step A.2.6 |
| `mail/CoordinatorDigestEmail-items.png` | The three lists, whose `<h2>` headings render larger than the title | Rendered |
| `mail/GraduationEmail.png` | The graduation mail with its untrue ratification sentence (T311) | Real, Step 5.18 |
| `mail/EntrustmentDecisionExpiringReminderEmail.png` | PAED-010 expires 2026-10-16, "(20 days from today)" | Rendered |
| `mail/EntrustmentDecisionExpiredEmail.png` | PAED-010 expired | Rendered |
| `mail/PasswordResetEmail.png` | A reset link to a page that does not exist | Rendered |
| `mail/AssessmentRequestedEmail.png` | "Anele Dlamini has requested your assessment for Mini-CEX (Paediatrics)" | Rendered |
| `mail/AssessmentAcceptedEmail.png` | A state no CPSA workflow has | Rendered |
| `mail/AssessmentCompletedEmail.png` | "David Naidoo has completed your assessment …", View feedback | Rendered |
| `mail/AssessmentDeclinedEmail.png` | The decline, carrying the note as "Reason:" | Rendered |
| `mail/StarDecisionEmail.png` | "Your STAR reflection has been approved by the committee." | Rendered |
| `act-5/5.18-1-graduation-email.png` | The graduation mail at 800 px, taken during the replay | Real, Step 5.18 |

These states have no capture, because nothing sends them yet. Design them from the requirements:
- **The notices T319 and T320 will add:** an activity move, a review outcome, an appeal lodged, an appeal resolved, a
  STAR revoked, a data-rights decision and an erasure confirmation.
- **An invitation resent** (Step 2.26). The template is the same as the first issue's, so the mail cannot say that it
  replaces an earlier link. The story's sink accepted every mail, so no invitation was resent to it.
- **A digest at volume,** and nudges with many items.
- **Every message at 390 px, with images off, and in a dark-mode client.**

## Attach

Every path is under `design/baseline/` and was checked to exist on 2026-09-27. **Every one-time token in `mail/` is
replaced** by `PLACEHOLDER-TOKEN-` padded with x to the real token's length (43 characters for a registration link, 59
for an MSF link), so each link keeps its real width (observed: `grep token= design/baseline/mail/*` finds only
placeholders). The captures in `act-1/`–`act-4/` and `states/` are screens: open each before uploading (BRIEF § 3.3).
Two of them show a full registration link (observed): `act-2/2.16-1-registrars-invited.png` and
`act-1/1.7-1-mbatha-invited.png`. **Crop the link out first, or leave them out.**

**First, with the ask (the KEY SCREENSHOTS):**
- `mail/InvitationEmail.png`
- `mail/MsfInvitationEmail.png`
- `mail/AssessorPendingNudgeEmail.png`
- `mail/CoordinatorDigestEmail-items.png`
- `mail/GraduationEmail.png`
- `mail/EntrustmentDecisionExpiringReminderEmail.png`
- `mail/AssessmentDeclinedEmail.png`
- `mail/StarDecisionEmail.png`
- `act-2/2.16-1-registrars-invited.png` (**crop its registration link out first**)
- `states/entrustment-decisions--revoke-form.png`
- `states/data-rights--submitted.png`

**Then, as the chat asks, by message:**
- The rest of the mails: `mail/MsfInvitationEmail-resent.png`, `mail/MsfInvitationEmail-learner.png`,
  `mail/MsfExpiryReminderEmail.png`, `mail/DraftNudgeEmail.png`, `mail/CoordinatorDigestEmail.png`,
  `mail/EntrustmentDecisionExpiredEmail.png`, `mail/PasswordResetEmail.png`, `mail/AssessmentRequestedEmail.png`,
  `mail/AssessmentAcceptedEmail.png`, `mail/AssessmentCompletedEmail.png`.
- The plain-text twins, pasted as text: `mail/InvitationEmail.txt`, `mail/MsfInvitationEmail.txt`,
  `mail/CoordinatorDigestEmail-items.txt`, `mail/GraduationEmail.txt`.
- The HTML, if it asks for the markup: `mail/InvitationEmail.html`, `mail/CoordinatorDigestEmail-items.html`.
- Where the mails are caused and promised: `act-1/1.7-1-mbatha-invited.png` (crop its link out first),
  `states/invitations-list--not-delivered.png`,
  `act-3/3.32-1-nudge-run.png`, `act-3/3.43-1-link-not-delivered.png`, `act-3/3.43-2-link-resent.png`,
  `act-4/4.27-1-zulu-ratified-stars-issued.png`, `act-4/4.36-2-mokoena-dlamini-paed002-revoked.png`,
  `act-5/5.17-1-mbatha-marked-complete.png`, `act-A/A.2.3-1-smit-digest-opt-out.png`, `states/data-rights--rejected.png`
  (its help text promises the mail T320 says is not sent), `act-A/A.4.4-1-forgot-password-stub.png`.

**Do not attach:** `act-1/1.5-2-kruger-invited.png`, `states/invitations-list--resent.png` and the other invitations
captures taken just after Issue or Resend, beyond the two above once cropped. Each shows a live registration link
(BRIEF § 3.3). The staging script leaves this paragraph's files out.

## Known problems this design must solve

| Task | Priority | What it means for the design | Evidence |
|---|---|---|---|
| T311 | P2 | The graduation mail drops "Your committee has ratified your final entrustment decisions" and links to My progress (`GraduationEmail.cs:15-16`, `:24-25`) | `mail/GraduationEmail.png` |
| T319 | P2 | A revoked STAR and a data-rights decision are promised a mail and get none (`Admin/EntrustmentDecisions/Index.razor:106`, `Profile/DataRights.razor:262`). Two new templates, plus an erasure confirmation with no link. No reason or note in the mail | `states/entrustment-decisions--revoke-form.png`, `states/data-rights--submitted.png` |
| T320 | P2 | No activity move and no committee outcome mails anyone, although the Data rights help text says they do (`Profile/DataRights.razor:163`). Five dead templates: four Assessment* and StarDecision. One workflow-labelled move template, a review-outcome template and an appeal template replace them | `mail/Assessment*.png`, `mail/StarDecisionEmail.png`, `act-3/3.3-1-requested.png` |
| T324 | P3 | The invitation prints the role's code ("as InstitutionalAdmin"; `InvitationEmail.cs:19` and `:26`, from `targetRole`) | `mail/InvitationEmail.png` shows "Trainee"; Step 1.7's reads "InstitutionalAdmin" |
| T325 | P3 | The expiry job reads "today" from UTC (`EntrustmentDecisionExpiryJob.cs:33`), so a Run now between midnight and 02:00 SAST counts "N days from today" from yesterday. Every job's schedule is in UTC (07:00, 08:00, 09:00, 03:30 UTC: 09:00, 10:00, 11:00 and 05:30 SAST) | code read |
| T274 | P3 | The STAR expiry notices reach locked accounts, and one failing recipient stops a mailing job's run. A footer that says why the reader got the mail must be true of every reader | code read |
| T240, D50 | done | The digest, both nudges and the draft reminder honour "Opt out of digest emails"; one-off mails do not. None of them says so in the mail | `act-A/A.2.3-1-smit-digest-opt-out.png` |
| Not filed | — | **Four mails that ask for action have no link:** the nudge, the draft reminder, the digest and the graduation mail ("Please log in to Wombat") | `mail/AssessorPendingNudgeEmail.png`, `mail/DraftNudgeEmail.png` |
| Not filed | — | **The digest names a campaign by questionnaire and id,** not by whose it is ("Default MSF (campaign #1)"; `WeeklyCoordinatorDigestJob.cs:269`) | `mail/CoordinatorDigestEmail-items.png` |
| Not filed | — | **The digest's `<h2>` sections render larger than the `<h1>` title.** The shell styles `h1` and `p` only (`EmailTemplateBase.cs:20-21`) | `mail/CoordinatorDigestEmail-items.png` |
| Not filed | — | **The resent invitation and the resent MSF link are word for word the first mail.** Neither says that it replaces a link, although Resend makes the old one stop working (Step 2.26's page says so) | `mail/MsfInvitationEmail-resent.png` |
| Not filed (code read) | — | **The STAR expiry reminder repeats daily.** The job reminds any active STAR within 30 days whose last reminder was before today (`EntrustmentDecisionExpiryJob.cs:54`), so a STAR is reminded every day of its last 30 (inference from the code; no test covers the job, and the story never ran it inside a window). Decide the cadence, and design the subject for it | code read |
| Not filed (code read) | — | **An invitation's link is relative when `Wombat:BaseUrl` is unset** (`IssueInvitation.cs:139`), while an MSF open is refused then (`WombatOptionsExtensions.cs:35`). Production sets it (`deploy/verify/drift-check.sh:143`) | code read |
| Not filed | — | **The footer fails contrast.** #888 on white is 3.5:1 (computed), for 0.8rem text | `mail/InvitationEmail.png` |
| Not filed | — | **The text twins wrap by hand.** The graduation text breaks a sentence mid-line ("portfolio of evidence is" / "available in Wombat"; `GraduationEmail.cs:24-25`) | `mail/GraduationEmail.txt` |

## Questions the design must answer

1. **Whose voice is it?** The Wombat wordmark alone, or the institution as the sender ("Kgosi Kgari Teaching Hospital,
   via Wombat")? `InstitutionBrand` (a logo and two colours; `Domain/Institutions/InstitutionBrand.cs`) exists, and no
   mail or page draws it.
2. **Images.** Is the header text only, or a hosted image of the mark from the base URL? Remote images are often
   blocked until the reader allows them (inference), so the mail must stand without one.
3. **Reply.** Keep "Please do not reply", or give a person to reply to: the coordinator for an MSF request, the inviter
   for an invitation?
4. **Subjects.** What does each subject carry for someone scanning an inbox? The MSF subjects name the trainee, the
   window and the due date (T202, T206); the nudges' and the digest's name nothing.
5. **Silent moments.** Should an admission (PendingTrainee → Trainee), a withdrawal (Deactivate, Step 5.27: "he keeps
   the Trainee role and is not emailed") or a return for more detail be mailed? Today none is; T320's recommendation
   covers the return, not the other two.
6. **Dates.** Mails print yyyy-MM-dd; the STAR certificate prints "16 October 2026". One rule for mail.
7. **Password reset.** Design it as a marked future state, or leave it out and delete the template (T320 item 3 leaves
   the choice)?

## Notes

- **A mail is not a page.** None of BRIEF § 5.1's Blazor, CSP or component rules applies inside a mail client. Only its
  Content and Accessibility lines do. Every link a mail carries lands on a page that those rules govern: the register
  page and `/msf/respond` are static pages (F11, F10).
- **Delivery is tracked for two templates.** An invitation and an MSF link carry a delivery key, so the invitations list
  and the campaign page can say "Being sent", "Sent" or "Not delivered" (T251, T283). That state is designed on those
  screens (F11, F10), not in the mail.
- **Who is skipped** is decided by `ReminderRecipientPolicy` (T240, D50) and each job's own rules. It is not the
  design's to change, but the footer must describe it truly.

## Acceptance

After Claude Code builds the chosen design (BRIEF § 9), the flow is done when all of these hold:
- **The tests pass.** Run `dotnet test` on the Application and Web test projects, never with --no-build (CLAUDE.md §
  Testing). They include:
  - `MsfInvitationEmailTests`;
  - the mailing jobs' tests (`ActivityDraftNudgeJobTests`, `AssessorPendingNudgeJobTests`,
    `MsfInvitationExpiryReminderJobTests`, `WeeklyCoordinatorDigestJobTests`);
  - `IssueInvitationCommandHandlerTests`;
  - the new tests T311, T319 and T320 name.
- **Each template has a test** that its HTML and text carry the same facts and the same link, that the link is
  absolute on the configured base URL, and that no free-text field reaches either body.
- **The mails replay.** Replay the steps that send them, against an SMTP sink (`tools/scenario-replay.ps1 start <db>
  <port> smtp=keep`, with a sink on port 25):
  - 1.5, 1.7, 2.16, 2.17 and 2.26;
  - 3.32, 3.36 and 3.43;
  - 5.17 and 5.18;
  - A.2.3 to A.2.8;
  - once T319 and T320 land, 3.3, 3.5, 3.11, 4.27, 4.36, 4.43 to 4.48, A.1.3, A.1.8 and A.1.12.
  Every Expect holds. Where the design changes a mail's wording, the steps' Expect lines change in the same task (BRIEF §
  9, item 7). Expect to rewrite 1.5 (the invitation's wording), 3.32 and A.2.8 (the greeting and the list), and 5.18
  (T311).
- **The baseline is re-captured.** The 19 captures above are retaken into `design/baseline/mail/`, from the sink where
  the story sends the mail and rendered otherwise, and compared with the chosen artboards. Check each at 600 px, at 390
  px, with images off, and as its `.txt`.

## The runbook steps, verbatim

Paste this as the second message. It is each step's Role, Route, Do and Expect, copied from
`execution/knowledge/scenario-paediatrics/`. `D` is the replay day: 2026-09-26.

```text
Step 1.5 — Invite Dr Kruger as the CPSA's CollegeAdmin
Role: Administrator — the platform operator
Route: /admin/invitations
Do: Under Issue invitation, enter `kruger@cmsa.wombat.local` and choose the role CollegeAdmin. Press Issue invitation
  before choosing a College. Then choose `College of Paediatricians of South Africa` and issue the invitation.
Expect: Before the issue, Active invitations reads "No active invitations".
  - The Role picker offers the eight roles an invitation can carry, and never Administrator.
  - Choosing CollegeAdmin replaces the Institution picker with a College picker, which offers the Demo College and the
    CPSA. Speciality and Sub-speciality stay disabled.
  - The first press is refused: "A college is required for a college administrator."
  - The second press reads "Invitation issued for kruger@cmsa.wombat.local. Its email is being sent. Copy the link below
    — it is shown only once." (T283's wording). The registration link is shown beneath it, and it expires in 14 days.
  - The new row reads CollegeAdmin, with College `College of Paediatricians of South Africa`, no institution, an expiry
    of `D+14` and a Delivery of "Being sent".
  - The application log holds a stub email "Your Wombat invitation", tagged `invitation, role:CollegeAdmin`. It invites
    the reader to register as CollegeAdmin, with the same link and its expiry date. The log never names the address
    (T282), so the role tells this mail from Mbatha's.

Step 2.16 — Mbatha invites the five registrars, one to a mistyped address
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Invite `molefe@`, `dlamini@`, `duplessis@` and `mahlangu@kgk.wombat.local` as Trainee at KGK, Paediatrics /
  Paediatrics. Copy each link. For Dr Ndlovu she types `ndlvou@kgk.wombat.local`.
Expect: A Trainee invitation needs both speciality and sub-speciality, and the Sub-speciality select is enabled for it.
  Five rows read Trainee, KGK, Paediatrics, Paediatrics, and each Delivery reads "Being sent".

Step 2.17 — Mbatha revokes the mistyped invitation and issues the right one
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations → /account/register
Do: Revoke the `ndlvou@` row and issue a Trainee invitation to `ndlovu@kgk.wombat.local` with the same scope. Then open
  the revoked link, as anyone holding it could.
Expect: The row's button is named "Revoke the Trainee invitation to ndlvou@kgk.wombat.local" (T239). Pressing it shows
  "Invitation revoked." and the row is gone, with no link shown (see Step 2.6's note). An address is never corrected in
  place: the invitation names who may register (T283). A new row reads `ndlovu@`. The revoked link shows no form, only
  "This invitation has been revoked."

Step 2.26 — Mbatha resends Dr du Plessis's invitation
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: At least an hour after Step 2.16, open Invitations and press Resend on du Plessis's row. Copy the new link.
Expect: His is the only row left. Its Delivery reads "Not delivered. Resend emails a new link in place of the current
  one, which then stops working.", and it offers Resend before Revoke. Afterwards the page reads "A new invitation link
  is being emailed to duplessis@kgk.wombat.local. The link it replaces no longer works. Copy the new link below — it is
  shown only once.", with the link below it. The row reads "Being sent", offers no Resend and expires 14 days from now.

Step 3.3 — Dr Dlamini completes the request and submits it
Role: Trainee — Dr Anele Dlamini
Route: /activities/{ActivityId:int}
Do: Type the presenting problem "Two-year-old with bronchiolitis and rising work of breathing" and submit.
Expect: The focused result reads "Submitted. It is now Requested. It is in David Naidoo's Activity inbox." The status
  card, badged Requested, reads "With David Naidoo since …" and "Nothing for you to do. You can cancel the request until
  David Naidoo acts on it.", and its one action is Cancel request…, quiet. The page is read-only to her: Request reads
  "Filled in by Anele Dlamini, `D`", and Entrustment and Feedback are locked, "David Naidoo fills this in". The history
  adds Submit, Draft → Requested. It carries no lateness note, because ten days is on time (D15). No email is sent:
  Wombat mails nobody when an activity moves, so Dr Naidoo learns of the request from his inbox.

Step 3.11 — Dr Khumalo declines the request, with a reason
Role: Assessor — Dr Fatima Khumalo
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Ndlovu's Mini-CEX. Press Decline and send it with the reason left empty. Then send it with the reason "I was
  not on the ward that day; Dr Botha observed this encounter. Please send it to her."
Expect: The status card reads "Your move. Sipho Ndlovu asked you on …". Decline opens the note panel under the actions,
  "Decline this request", and moves the focus into "Reason for Sipho Ndlovu", with "Sipho Ndlovu reads it on the
  activity's page. It is kept with the activity's history." under it. Sent empty with Decline with this note, it is
  refused: the panel stays open, with the note as typed, and its summary reads "Not declined." and "Reason for Sipho
  Ndlovu: Decline requires a note."; the activity stays Requested. Keep the request would close the panel and hand the
  focus back to Decline. Sent with the reason, the result reads "Declined." The status card, badged Declined, reads
  "Closed. You declined it on …", quotes her reason, and reads "It credits nothing, and nothing more can happen to it."
  The page is read-only, with no move left. The history's Decline row, Requested → Declined, is credited "—", with her
  note on a row of its own under it. Her inbox no longer lists the request.

Step 3.32 — The daily nudge reminds the assessors
Role: Administrator — devadmin@wombat.local
Route: /admin/jobs
Do: Run `assessor-pending-nudge` now.
Expect: The job's last run updates. The application log holds two stub emails, "Activities awaiting your assessment".
  The log names no address (T282); each greets its assessor by first name:
  - "Hi Thandi", listing Mini-CEX (Paediatrics) from Nomsa Mahlangu, waiting 8 days;
  - "Hi Mohammed", listing Portfolio and Logbook Review (Paediatrics) from Pieter du Plessis, waiting 8 days.
  The run's summary line reads "assessors nudged 2 (activities 2)" and skips nobody. Nothing else has waited five days,
  so nobody else is nudged.

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

Step 3.43 — Mr Smit resends the undelivered link
Role: Coordinator — Mr Pieter Smit
Route: /msf/campaigns/{CampaignId:int}
Do: At least an hour after the open, open the campaign and resend.
Expect: The counts read Responded: Peer doctor 1, Consultant 2, Nurse 2 and Allied health professional 1, and All
  groups 7 invited, 6 responded. A warning reads "1 link was not delivered. Resend sends each of these respondents a new
  link; this page never says who they are.", with a "Resend 1 link" button. After the resend, the page reads "1 new link
  is being sent." and "1 link is still being sent…", and the warning is gone. The log holds one more "Feedback request:
  Lerato Molefe …" stub email. It names no address; it is Dr Khoza's, the only link not answered.

Step 4.27 — Dr Zulu ratifies, and the STARs are issued
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Ratify the decision.
Expect: "Decision ratified." The state reads Ratified, and Ratify is gone.
  - **Agenda:**
    - PAED-001, 010 and 012 read Decided, each with "STAR #n.";
    - the nine deferred lines keep their reasons;
    - PAED-008, 009 and 013 read Not decided, "The review was ratified without deciding it.".
  - **Pending:** "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each
    one." (T212)
  - **Standing:** "2 at or above · 1 below · 12 with no decision, of 15 EPAs". PAED-001 and 012 at `5` meet year 4's
    target of `5`, and PAED-010 at `4` is below it. The exit rule reads "2 of 15 EPAs at their exit level by STAR
    decision".
  - **What ratifying locks:** nothing on the review can now be staged, deferred, removed or recorded.

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

Step 5.17 — Prof Mbatha marks Dr Molefe's programme complete on `D`
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees/edit → /admin/trainees → /admin/users/{UserId}
Do: Set the last day to `D`, press Mark complete and confirm. Then go back to Trainees, and open Dr Molefe in Users.
Expect: The page says "Trainee marked complete. The Trainee role has been removed and a graduation email sent."
  - The summary reads Status Completed, Completed `D`.
  - The last-day field, Deactivate and Mark complete are gone.
  - On Trainees she is no longer under Active profiles. She is listed under Completed & closed profiles with the outcome
    "Completed `D`".
  - Her user page says "This user has no roles.", and its Add role offers no Trainee (T303).
  - No credit is taken back, because nothing of hers is observed after `D` (T281; Step 5.27 shows a take-back).

Step 5.18 — Dr Molefe is emailed
Role: System — the graduation email sent by Mark complete
Route: n/a
Do: Read the application log for the mail sent in Step 5.17.
Expect: There is one email to `molefe@kgk.wombat.local` with the subject "Congratulations on completing Paediatric EPA
  Curriculum". It congratulates her on completing the programme as of `D`, and says that her committee has ratified
  her final entrustment decisions and that her portfolio is available in Wombat.

Step A.1.3 — Mr Smit approves the export from KGK's queue
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights → /admin/data-rights/{Id:guid}
Do: Open Data rights requests from the menu. Narrow it to type Export and status Submitted, apply the filters, and
  review Dr Dlamini's request. Press Approve with no decision note. Then approve it with the note "Identity confirmed;
  export released to the data subject."
Expect: The queue holds only KGK's requests (T112), and the filters change it only when they are applied. On the
  request's page Data rights requests stays lit, and the trail reads Home › Data rights requests › the request's id
  once it has loaded (R2-Rules § 3; DESIGN.md's owner table).
  - The row names the requester by the address she signs in with, and its Review action is named for the row (T239).
  - The detail page shows the requester, her user id, when it was submitted, its type, its status and her reason.
  - Approving with no note is refused: "A decision note is required."
  - Once approved, the request reads Completed at once, because an export is built when it is downloaded. A Decision
    card appears (decided by, as a user id; decided on; the note; completed on), and Approve and Reject are gone.

Step A.1.12 — devadmin approves the erasure
Role: Administrator — devadmin
Route: /admin/data-rights → /admin/data-rights/{Id:guid}
Do: Narrow the queue to Erasure and apply the filter, review Dr Ndlovu's request, and approve it with the note "Identity
  confirmed. Programme records are kept under a pseudonym."
Expect: The Administrator's queue holds every institution's requests.
  - Once approved, the request reads Completed at once, with its Decision card. The approval and the erasure are one
    transaction (T258).
  - No confirmation is asked before this irreversible action (noted on T264).
  - The queue and the request still name the requester by the address he signed in with: the erasure keeps the request
    as it was submitted (`ErasureExecutor`).

Step A.2.3 — Mr Smit opts out of digest emails
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Tick "Opt out of digest emails" and save his preferences.
Expect: "Processing preferences saved."

Step A.2.4 — devadmin runs the weekly digest, and Mr Smit is skipped
Role: Administrator — devadmin
Route: /admin/jobs
Do: Run weekly-coordinator-digest now, then read the application log.
Expect: "Job 'weekly-coordinator-digest' dispatched."; the row's last run is now, and it reads Succeeded. The log holds
  "WeeklyCoordinatorDigestJob: digests sent n; coordinators skipped: …", with at least one counted as "opted out of
  digest emails". No "Your weekly Wombat digest" mail greets Pieter.

Step A.2.6 — devadmin runs the digest again, and Mr Smit gets it
Role: Administrator — devadmin
Route: /admin/jobs
Do: Run weekly-coordinator-digest now, then read the log.
Expect: A stub mail "Your weekly Wombat digest" begins "Hi Pieter,". It covers KGK only (T117). With nothing to list
  it says "No items requiring attention this week." Otherwise it lists:
  - under "Trainees at risk", any current KGK trainee who has filed nothing in the last 30 days by the real clock
    (T284): never Dr Molefe or Dr du Plessis, whose programmes have ended, or Dr Ndlovu, who has been erased;
  - any MSF campaign of his waiting on its review;
  - any committee review scheduled in the coming week.

Step A.2.8 — devadmin runs the two reminders against aged work
Role: Administrator — devadmin
Route: /admin/jobs
Do: Age the draft by 15 days and the request by 6 (see the Note). Then run activity-draft-nudge and
  assessor-pending-nudge now, and read the log.
Expect: The log holds a stub mail "You have draft activities waiting" beginning "Hi Nomsa,", listing "Mini-CEX
  (Paediatrics) — draft for 15 days". It also holds "Activities awaiting your assessment", beginning "Hi Fatima,",
  which lists "Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 6 days". Each job logs one line counting whom it
  reminded and whom it skipped, and why (T151, T240).

Step A.4.4 — Dr du Plessis has forgotten his password
Role: Anonymous — Dr Pieter du Plessis
Route: /account/login → /account/forgot-password → /account/login
Do: Follow "Forgotten your password?" from the sign-in page.
Expect: A "Forgotten password" page: "A Wombat administrator can set a new password for you." then three steps ("Ask
  your Wombat administrator for a new password: the one at your institution or, if your account belongs to no
  institution, the platform's administrator. They give it to you themselves; Wombat does not email it." "Sign in with
  it." "Choose your own password on My account, under Change password."), and Back to sign in. There is no field, and
  no email is sent.
```
