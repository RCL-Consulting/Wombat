# F11 From invitation to first sign-in and admission

An admin invites each person with the right role and scope. The invitee registers from the emailed link. A registrar
waits as PendingTrainee until the InstitutionalAdmin admits them with their programme dates. Every user meets this flow
once, and it is their first impression. It is flow 11 of 18 (`design/BRIEF.md` § 8).

| | |
|---|---|
| **Mode** | Straight to fidelity for Invitations (screen 1) and Register (screen 2). Wireframe first for the pending registrar's pages (screen 3) and the admission form (screen 4), which gains the training year (T306) and a curriculum picker that offers only the adopted version (T304). |
| **Viewports** | Desktop 1280×800 and phone 390×844. |
| **Held** | T303 (Add role offers Trainee): its images wait for re-capture (§ Attach). T297's (dashboard cards) were re-captured on 2026-09-26. |
| **Frequency** | Each January intake of two registrars, and each staff join (`scenario-paediatrics/README.md` § The world). Every user meets it once. |
| **Stakes** | High. It is every user's first impression, and a role and scope are access control. |
| **People** | Administrator `devadmin`, who invites the CollegeAdmin and the first InstitutionalAdmin. InstitutionalAdmin Prof Nolwazi Mbatha, who invites the staff and registrars and admits the registrars. Anonymous invitees: Dr Anton Kruger, Prof Mbatha, all the staff and all five registrars. PendingTrainee: each registrar between registering and admission. |

**How to use this brief**

1. Open a new Claude Design thread for this flow (BRIEF § 2.3, step 3).
2. Paste the block under **The ask**, then the block under **Appendix: the runbook steps**.
3. Attach the images listed under **Attach**, key screenshots first. **Three invitation captures show a live one-time
   registration link. Crop the link out, or leave the image out** (BRIEF § 3.3; § Attach below).
4. Answer Claude's questions in the chat, each as a sentence [academy].
5. Export the chosen artboards to `design/flows/11-onboarding/` before moving on [start].

## The ask

```
FLOW 11 — Invite each person with the right role and scope, register from the link, and be admitted

DIRECTION: <paste the aesthetic direction recorded as W-nnn in execution/DECISIONS.md (design/BRIEF.md § 2.3 step 0)>
DESIGN SYSTEM: Wombat, as set up for this project:
  - the :root tokens of app.css;
  - Fraunces on the wordmark only;
  - Lucide line icons;
  - the components named in CONSTRAINTS.
  The signed-in screens sit inside the shell settled in flow 01. Register has no shell. It is the account card (the
  sign-in page's layout), and it is static.

GOAL: An admin invites each person with the right role and scope, and can revoke or resend an invitation. The invitee
  registers from the emailed link. A registrar waits as a PendingTrainee until the InstitutionalAdmin admits them with
  their programme dates. Everyone lands on the right first page.
  What is wrong today:
  - The invitations table has 10 columns in a card about 630 px wide at 1280 px. It scrolls sideways, hiding the email
    and the role, and "Not delivered" breaks mid-word.
  - Revoke is a red button in each row.
  - After a refused issue or a revoke, the previous invitee's one-time link stays on screen.
  - A refused registration drops the names typed.
  - The training year is shown nowhere in admission.
  - Add role offers Trainee. This is being fixed.

AUDIENCE:
  - Administrator: the platform operator. Invites the CollegeAdmin and the first InstitutionalAdmin.
  - InstitutionalAdmin: Prof Nolwazi Mbatha, head of department. Invites the staff and the registrars, and admits the
    registrars.
  - Invitees, anonymous until they register:
    - consultants, a coordinator and programme leads;
    - an external examiner;
    - registrars.
  - PendingTrainee: a registrar who has registered but is not yet admitted.
  - Frequency: each January intake of two registrars, and each staff join. Every user meets it once.
  - Stakes: high. It is the first impression, and it sets who can see what.
  - Viewports: desktop 1280×800 and phone 390×844.

SCREENS, in order:
  1. /admin/invitations: Invitations (Administrator, InstitutionalAdmin).
     - The issue form: Email, Role, Institution (or College, for a CollegeAdmin), Speciality and Sub-speciality.
       - Role offers only what the caller may issue. The Administrator gets 8 roles, never Administrator. The
         InstitutionalAdmin gets 7, with no CollegeAdmin.
       - Speciality and Sub-speciality are enabled and required by the role's scope rule:
         - a Coordinator's speciality is optional;
         - a CommitteeMember takes no sub-speciality;
         - a SpecialityAdmin needs a speciality;
         - a SubSpecialityAdmin, an Assessor and a Trainee need both.
     - Refusals, in words, for example "Speciality administrators must be scoped to a speciality." and "A college is
       required for a college administrator."
     - The result of an issue:
       - "Invitation issued for <address>. Its email is being sent. Copy the link below — it is shown only once."
       - The one-time registration link, which expires in 14 days.
     - Active invitations: Email, Role, Institution, College, Speciality, Sub-speciality, Issued, Expires, Delivery, and
       the actions.
       - Delivery is words, not a badge: "Being sent"; "Sent"; "Not delivered."; or, after a second failure, "check the
         address".
       - A row not delivered offers Resend before Revoke. Each action is named by its row, for example "Revoke the
         Trainee invitation to ndlvou@…".
     - States: empty ("No active invitations"); the CollegeAdmin variant; the institution's form; refused; invalid
       address; issued, with its link; rows being sent; not delivered; check the address; sent; resent; revoked.
  2. /account/register: Register. Static; it is reached from the emailed link and posts to /account/register/submit.
     - Heading: "Complete registration". "Registering <address> as <role>." The email is shown, read-only.
     - Fields: First name, Last name, Password, Confirm password (each password with a Show toggle).
     - Register.
     - Refusals that bring the form back:
       - password too short: "Passwords must be at least 12 characters." (the first field takes the focus);
       - "The password confirmation does not match.";
       - details invalid.
     - Refusals shown with no form:
       - "This invitation has already been used.";
       - "This invitation has been revoked.";
       - "This invitation is invalid.";
       - "This invitation has expired.";
       - "A user with this email address already exists.";
       - "The invitation token is missing.";
       - "Registration could not be completed. Please try again."
     - Also at 390 px.
  3. A pending registrar's first pages (PendingTrainee).
     - Home: one card, "Awaiting admission": "You are registered and waiting to be admitted to a curriculum by your
       programme administrator.", with "Review your account →".
     - The nav: Home, My Account, Data Rights, Activities, My Activities and Logout.
     - My account: the role reads PendingTrainee.
     - Activities: the type picker, with 11 instruments.
     - My Activities: "No activities yet".
     - My Progress, typed: Access denied.
  4. /admin/trainees and /admin/trainees/edit: admission (InstitutionalAdmin).
     - Trainees:
       - Pending admission: name, email and institution, with "Admit to curriculum" named for the registrar.
       - Active profiles: name, curriculum, sub-speciality and expected completion, with Edit. NEW: a Training year
         column.
       - "No pending trainees found."
     - The admission form, titled "Trainee profile":
       - a summary of the registrar;
       - Curriculum, offering only the version the institution has adopted;
       - Programme start date, pre-filled with today;
       - Expected completion, optional. Its help: "Leave this empty to derive it from the curriculum window."
       - NEW: a read-only "Training year N", recomputed as the start date is typed;
       - Admit trainee.
     - After admitting, the profile page: "Trainee profile saved.", and an unknown id.
     - The registrar's open tab moves to sign-in: "Your session has ended. Please sign in again." (Admission changes
       her role.)
     - The users list and the user page as the registrars wait: PendingTrainee reads "System-managed", with no Remove.
  5. / — the InstitutionalAdmin's first Home, before anything is adopted:
     - Users: InstitutionalAdmin 1;
     - Specialities & sub-specialities;
     - Quick links: Users, Invitations, Curriculum adoptions and Entrustment decisions.

STEPS: 1.5, 1.7–1.11, 2.1–2.11, 2.16–2.19, 2.26–2.32 and 2.44.
  - They are pasted verbatim after this block (Role / Route / Do / Expect).
  - The Expect lines describe the product as replayed on 2026-09-26. Where a requirement below differs, the
    requirement wins.

STATES TO SHOW:
  - Every state listed under SCREENS.
  - For each signed-in list and form: loading (a skeleton under the header), the load error, and narrow (390 px).
  - Data volumes:
    - none;
    - typical: 9 staff invitations, then 5 registrars;
    - heavy: a January intake while staff invitations are still open, with some rows not delivered.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T303: on a user page, Trainee and PendingTrainee read "System-managed", with no Remove. Trainee is never offered
    under Add role. A person becomes a Trainee only by admission.
  - T323: at 1280 px, the invitations table shows Delivery and the row actions without sideways scrolling, and breaks
    no word.
  - T306: the admission form shows a read-only "Training year N" that updates as the start date is typed. Active
    profiles gains a Training year column.
  - T304: the admission form's Curriculum picker offers only the version the institution has adopted. Saving a
    profile never re-admits the registrar.
  - T317: a refused registration keeps the first and last names typed. It never keeps a password, and never puts the
    names in the address.
  - T315: the invitation token leaves the address bar once the page loads. Register is static, so this cannot depend
    on component code running after render.
  - T286: an invitation to an address that already has an account is refused at issue, on the invitations page. The
    register page's "already exists" refusal offers Sign in.
  - T324: validation messages use the field's label, never a property name (for example, "The TargetRole field is
    required.").
  - T264: Revoke is an outline button, named per row, with a confirmation that names the invitee. A refused issue or a
    revoke clears the previous invitee's link from the screen.

REQUIREMENTS EVERY WOMBAT PAGE MEETS (design/BRIEF.md § 6; the ones this flow tests):
  A1  Contrast in the tokens: the success and info alerts, and the input borders on the register card.
  A3  At 1280 px the wide table shows its actions. At 390 px nothing scrolls sideways except a table inside its own
      container.
  A4  Password toggles are 24 px or larger, expose aria-pressed and name their input.
  A5  Every action reports its outcome where the focus lands. A refusal keeps what was typed.
  A6  Loading, load-error and not-found states are designed.
  A8  People by name, roles by label. Home greets by email today.
  A11 Offer only what the caller can do: roles, scopes and curricula.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Should issuing an invitation be a form beside the table (as today), or a dialog or page of its own, so that the
     table has room?
  2. How is the one-time registration link shown, so that it is copied once and is not left on screen after a refused
     issue or a revoke?
  3. What does a registrar see between registering and admission, and does it tell her who admits her, and when?

CONSTRAINTS:
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side behaviour beyond a
  ≤10-line script module; no live validation; the phone nav toggle is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence.
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token or a NEW token with its value; spacing on the scale xs 4, sm 8, md 16, lg 24, xl 32,
  2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, primary action),
  Breadcrumbs, DataTable (.clinic-table in .table-container, PagerControls), FormField / FormActions (.form-container,
  .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid), StatePanel (loading / empty / error),
  Skeleton, Alert (success / info / warning / danger), ActionResult, ConfirmDialog (native <dialog>), badges (five tints),
  Icon, TrajectoryChart (hand-drawn SVG, no chart library).
Design these framework states explicitly (no source file shows them): the active nav item; field validation (invalid
  border plus a stripe, not colour alone; message under the field; the summary); the reconnect dialog (rejoining,
  retrying, failed, paused, resume-failed); the in-app error bar; access denied; not found; session ended.
Content: people by name, states and types by label, times in South African time with the zone shown; an out-of-scope
  record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (sidebar included); targets ≥ 24 px; focus
  moves to an action's result; every page works at 390 px with no sideways scroll.
Viewports: 1280×800 and 390×844.

ASK:
  - 2–3 variations.
  - Full fidelity for screens 1 and 2. Wireframes first for screens 3 and 4, then fidelity for the one I pick.
  - Name every design-system component you use, and mark anything else NEW.
  - Say which DESIGN.md rule a variation breaks.
  - Design Register as plain HTML with a form post: no script, and no live validation.
  - Flag edge cases, and review each screen's accessibility against the A-requirements above.

ATTACHED (key screenshots first):
  - states/invitations-list--institution.png, invitations-list--not-delivered.png, invitations-list--check-address.png;
  - states/register--form.png, register--password-short.png, register--used.png;
  - states/home--awaiting-admission.png, act-2/2.19-4-mahlangu-progress-denied.png;
  - states/pending-trainees-list--pending.png, trainee-profile-edit--admission.png, act-2/2.29-2-four-admitted.png;
  - states/home--institutional-admin-first.png.
  - A full-page capture draws the sidebar part-way down a long page, or ends it early. That is the capture, not the
    product.
```

## The journey

| Step | Route | Who does what | What they must be able to see |
|---|---|---|---|
| 1.5 | `/admin/invitations` | `devadmin` invites Dr Kruger as CollegeAdmin, pressing Issue once before choosing a College. | 8 roles, never Administrator. College replaces Institution. "A college is required for a college administrator.", then the issued status, the one-time link, and a row reading "Being sent". |
| 1.7 | `/admin/invitations` | `devadmin` invites Prof Mbatha as InstitutionalAdmin at KGK. | InstitutionalAdmin is the default. Speciality and Sub-speciality are disabled for this role. Two rows. |
| 1.8 | `/account/register` → `/` | Dr Kruger registers from his link. | "Registering kruger@… as CollegeAdmin.", with the address read-only. He lands on his Home, with one card, National catalogue. |
| 1.9 | `/account/register` | Anyone opens the used link. | "This invitation has already been used.", with no form (T285). |
| 1.10 | `/account/register` → `/` | Prof Mbatha registers. | Her Home: Users; Specialities & sub-specialities (it counts the national catalogue until T291 item 4 lands); Quick links; and an 18-item nav. |
| 1.11 | `/admin/invitations` → `/` | `devadmin` reloads the list. | "No active invitations": a used invitation leaves the list. |
| 2.1 | `/account/login` → `/` → `/admin/invitations` | Prof Mbatha opens Invitations and reads the form. | 7 roles, with no CollegeAdmin (T093). KGK is the only institution. |
| 2.2 | `/admin/invitations` | She invites Mr Smit as Coordinator. | Speciality optional, Sub-speciality disabled (T060). The status and the link. The form back at its defaults. |
| 2.3 | `/admin/invitations` | She invites three committee members, in Paediatrics. | Speciality offers Paediatrics alone (T092). Sub-speciality stays disabled. |
| 2.4 | `/admin/invitations` | She invites two assessors, in Paediatrics / Paediatrics. | Sub-speciality enabled, and filled by the chosen speciality. |
| 2.5 | `/admin/invitations` | She invites Dr van Rensburg, the external member. | Issued with no speciality. |
| 2.6 | `/admin/invitations` | She invites Dr Mokoena with no speciality, then with one. | "Speciality administrators must be scoped to a speciality.", and no link. Today the previous link stays on screen (F-2.6a, T264). |
| 2.7 | `/admin/invitations` | She invites Dr Sithole without a sub-speciality, then with one. | "The selected role requires speciality and sub-speciality scope." Then nine rows. |
| 2.8 | `/account/register` → `/` → `/account/logout` → `/account/login` | Mr Smit registers, then signs out. | His address and role named. The token cleared from the address bar (T315). His Home (re-captured after T297). |
| 2.9 | `/account/register` → `/` → `/account/logout` | Dr Patel's first two attempts are refused. | "Passwords must be at least 12 characters.", with the focus on the first field; then "The password confirmation does not match."; then his Home. |
| 2.10 | `/account/register` → `/` → `/account/logout` | The other seven staff register. | Each lands on Home, viewing as their role. |
| 2.11 | `/account/register` | Mr Smit opens his used link again. | "This invitation has already been used." |
| 2.16 | `/admin/invitations` | Prof Mbatha invites five registrars, one at a mistyped address. | Speciality and Sub-speciality both required for a Trainee. Five rows reading "Being sent". |
| 2.17 | `/admin/invitations` → `/account/register` | She revokes the mistyped invitation, issues the right one, then opens the revoked link. | Revoke named for its row (T239). "Invitation revoked." An address is never corrected in place (T283). "This invitation has been revoked." |
| 2.18 | `/account/register` → `/` → `/account/logout` | Four registrars register and wait. | "Registering <address> as Trainee.", then "Viewing as PendingTrainee" and the Awaiting admission card. |
| 2.19 | `/` → `/account/profile` → `/activities/mine` → `/activities/new` → `/portfolio/progress` → `/access-denied` → `/` | Dr Mahlangu explores before her admission. | The pending nav (T141); her role, PendingTrainee; "No activities yet"; the type picker with 11 instruments; Access denied on My Progress. |
| 2.26 | `/admin/invitations` | An hour on, Prof Mbatha resends Dr du Plessis's invitation. | "Not delivered. Resend emails a new link in place of the current one, which then stops working." Resend before Revoke. The new link, shown once. |
| 2.27 | `/account/register` → `/` → `/account/logout` | Dr du Plessis opens the old link, then registers from the new one. | "This invitation is invalid.", then the Awaiting admission card. |
| 2.28 | `/admin/invitations` → `/admin/users` → `/admin/users/{UserId}` → `/admin/trainees` | Prof Mbatha reviews the pending registrars. | Users lists 15. PendingTrainee reads "System-managed" (held, T303). Five registrars under Pending admission; "No active trainee profiles found." |
| 2.29 | `/admin/trainees` → `/admin/trainees/edit` → `/admin/trainees` | She admits four registrars with their start and completion dates. | Only the adopted curriculum offered (T091). The start date pre-filled. No training year today (T306). Each admission moves to the profile page. |
| 2.30 | `/admin/trainees` → `/admin/trainees/edit` → `/admin/trainees` | She admits Dr Ndlovu with no completion date, then corrects it. | The derived date and its help. "Trainee profile saved." "No pending trainees found." |
| 2.31 | `/` → `/account/session-ended` → `/account/login` → `/` | Dr Mahlangu's open tab ends; she signs in again. | "Your session has ended. Please sign in again." (T279). Then her Trainee Home (re-captured after T297). |
| 2.32 | `/account/login` → `/` → `/admin/invitations` → `/access-denied` → `/committee/panels` → `/committee/panels/new` → `/access-denied` | Mr Smit learns what his role offers. | His dashboard (re-captured after T297). Access denied on Invitations (T178). Decision Panels, read-only, with no nav link to it. Access denied on New panel. |
| 2.44 | `/` → `/admin/users` → `/admin/assessors` | Prof Mbatha reads her dashboard after onboarding. | Users by role, with no PendingTrainee line. Quick links. Fatima Khumalo's corrected name. The dashboard is designed in flow 12. |

## States to design

The states are from `scenario-paediatrics/states.md` § Account and sign-in, § Home, § Activities and § Institution
administration. Each is `design/baseline/states/<name>.png`. "Scratch" means the scratch database that `states.md`
describes.

| Page | States (screenshot names) |
|---|---|
| `/admin/invitations` | `invitations-list--empty`, `--college-admin`, `--refused` (Step 1.5); `--issued` (Step 1.5, **shows a link**); `--institution` (Step 2.1); `--invalid` (email `not-an-address`); `--being-sent` (Step 2.16, **shows a link**); `--revoked` (Step 2.17); `--not-delivered` (Step 2.26, before Resend); `--resent` (Step 2.26, **shows a link**); `--sent` and `--check-address` (scratch, with an SMTP sink); `--loading`; `--narrow` |
| `/account/register` | `register--token-missing`; `--form` (Step 1.8); `--password-short` and `--confirmation` (Step 2.9); `--details-invalid`; `--used` (Step 1.9); `--revoked` (Step 2.17); `--invalid` (Step 2.27); `--expired` and `--account-exists` (scratch); `--general-refusal` (held read); `--narrow` |
| `/` | `home--awaiting-admission` (Step 2.18); `home--institutional-admin-first` (Step 1.10) |
| `/account/profile`, `/activities/new`, `/activities/mine` | `profile--pending-trainee`; `new-activity--pending-trainee`; `my-activities--empty` (all at Step 2.19) |
| `/admin/users`, `/admin/users/{UserId}` | `users-list--pending` (Step 2.28); `user-detail--pending-trainee` (Step 2.28, **held: T303**) |
| `/admin/trainees` | `pending-trainees-list--pending` (Step 2.28); `--admitted` (Step 2.29); `--none-pending` (Step 2.30); `--loading`; `--narrow` |
| `/admin/trainees/edit` | `trainee-profile-edit--admission` (Step 2.29, before saving); `--active` (after it); `--saved` (Step 2.30); `--not-found` (typed `?id=999999`); `--narrow` |
| `/account/login` | `login--session-ended` (Step 2.31) |

That is 44 captures. No replay reaches Register's skeleton: the page is static, so its read runs before the page is
sent (`states.md` § States no local replay reaches). The pending trainee's pages were captured at desktop width only
(`coverage.md` § Flows and states not played), so design their 390 px states from scratch. The training-year line
(T306) and the issue-time refusal for an address that already has an account (T286) are new states with no capture.

## Attach

Paths are relative to `design/baseline/`. Every file below was found on disk on 2026-09-26. Open each one before you
upload it (BRIEF § 3.3).

**First, the key screenshots.** Each of these was opened; none shows a registration link.
1. `states/invitations-list--institution.png`
2. `states/invitations-list--not-delivered.png`
3. `states/invitations-list--check-address.png`
4. `states/register--form.png`
5. `states/register--password-short.png`
6. `states/register--used.png`
7. `states/home--awaiting-admission.png`
8. `act-2/2.19-4-mahlangu-progress-denied.png`
9. `states/pending-trainees-list--pending.png`
10. `states/trainee-profile-edit--admission.png`
11. `act-2/2.29-2-four-admitted.png`
12. `states/home--institutional-admin-first.png`

**Then, as the chat asks for them:**
- **Screen 1:**
  - `states/invitations-list--empty.png`
  - `states/invitations-list--college-admin.png`
  - `states/invitations-list--refused.png` (opened; no link)
  - `states/invitations-list--invalid.png`
  - `states/invitations-list--revoked.png` (opened; no link)
  - `states/invitations-list--sent.png` (opened; no link)
  - `states/invitations-list--loading.png`
  - `states/invitations-list--narrow.png` (opened; no link)
- **Screen 2:**
  - `states/register--token-missing.png`
  - `states/register--confirmation.png`
  - `states/register--details-invalid.png`
  - `states/register--revoked.png`
  - `states/register--invalid.png`
  - `states/register--expired.png`
  - `states/register--account-exists.png`
  - `states/register--general-refusal.png`
  - `states/register--narrow.png`
- **Screen 3:**
  - `states/profile--pending-trainee.png`
  - `states/new-activity--pending-trainee.png`
  - `states/my-activities--empty.png`
- **Screen 4:**
  - `states/users-list--pending.png`
  - `states/pending-trainees-list--admitted.png`
  - `states/pending-trainees-list--none-pending.png`
  - `states/pending-trainees-list--loading.png`
  - `states/pending-trainees-list--narrow.png`
  - `states/trainee-profile-edit--active.png`
  - `states/trainee-profile-edit--saved.png`
  - `states/trainee-profile-edit--not-found.png`
  - `states/trainee-profile-edit--narrow.png`
  - `states/login--session-ended.png`

**Crop the link out first, or leave these out.** Each shows a full `/account/register?token=…` link that still works:
- `states/invitations-list--issued.png` (observed: Dr Kruger's link);
- `states/invitations-list--being-sent.png` (observed: Dr Dlamini's link);
- `states/invitations-list--resent.png` (observed, BRIEF § 3.3).

Do not attach the act-1 and act-2 invitation step captures either (`1.5-2`, `1.7-1`, `2.2-1`, `2.3-1`, `2.4-1`,
`2.5-1`, `2.6-*`, `2.7-*`, `2.16-1`, `2.17-*`, `2.26-*`). They were taken just after an issue or a resend, or under a
link left on screen (F-2.6a). Treat each as showing a link unless you have opened it (BRIEF § 3.3).

**Hold until re-captured after the group-1 fix lands; do not brief from these:**
- T303:
  - `act-2/2.28-3-molefe-user.png`
  - `states/user-detail--pending-trainee.png`

**Re-captured after T297 landed (2026-09-26), so no longer held:** `act-2/2.31-2-mahlangu-trainee-home.png` and the
other first-Home captures of the five affected roles, `2.8-2`, `2.9-3`, `2.10-4`, `2.10-6`, `2.10-7` and `2.32-1`. Each
was taken from the end-of-Act-2 snapshot, not the step's own moment: Dr Mokoena's and Dr Sithole's Homes (`2.10-6`,
`2.10-7`) already count the five registrars admitted later in the act (BRIEF § 10).

## Known problems this design must solve

| Task | What it means for the design | Evidence |
|---|---|---|
| **T303** (group 1, being fixed) | **The user page.** Trainee reads "System-managed", with no Remove, beside PendingTrainee. **Add role.** It never offers Trainee. Admission is the only way in. | Held captures above; Step 2.28 |
| **T323** (A3) | **The table at 1280 px.** Delivery and the row actions show without sideways scrolling, and no word breaks. Today the card is about 630 px wide beside the form, and the table scrolls to hide Email and Role. "Not delivered" breaks as "delivere / d." and the check-address text runs one word per line (observed). | `states/invitations-list--check-address.png`, `states/invitations-list--not-delivered.png` |
| **T306** (BRIEF § 7 B4) | **Admission form.** A read-only "Training year N (from the programme start)", recomputed as the start date is typed. **Active profiles.** A Training year column. | `act-2/2.29-2-four-admitted.png`, `states/trainee-profile-edit--admission.png` |
| **T304** (P2) | **The Curriculum picker.** It offers only the version the institution has adopted; after a re-adoption today, it offers the old one too. **Saving.** A profile save never re-admits the registrar, and it reports any credit it replays (BRIEF § 10). | T304; Step 2.29's note |
| **T317** | A refused registration comes back with the first and last names as typed. The passwords never come back, and the names never go in the address (T317 recommends a short-lived cookie bound to the token). | Step 2.9; `states/register--password-short.png` |
| **T315** (backend) | The token must leave the address bar. Register is static, so `OnAfterRenderAsync` never runs; T315 clears it through `wwwroot/wombat.js`, or a cookie and redirect. The design adds no behaviour that needs component code after load. | Step 2.8's Expect |
| **T286** | **At issue.** An invitation to an address that already has an account is refused on the invitations page, before anything is stored or mailed. **On Register.** The "A user with this email address already exists." page offers Sign in. | `states/register--account-exists.png`; T286's note of 2026-09-26 |
| **T264** | **Revoke.** It is an outline button (red today), named per row, and confirms naming the invitee. **The link.** A refused issue or a revoke clears the previous invitee's link from the screen (F-2.6a, `act-2-onboarding.md:125`). | `states/invitations-list--not-delivered.png`, `states/invitations-list--resent.png` |
| **T324** (A8) | Validation messages use field labels. `InvitationsList` (`TargetRole`) and `TraineeProfileEdit` print property names today. | T324 |
| **T322** (A1), **T328** (A4) | **The register card.** Its input borders reach 3:1. Its "Show" toggles are 24 px or larger, expose `aria-pressed` and name their input. **Date inputs** use the body font; they render in a monospace face today (observed). | `states/register--form.png`, `states/trainee-profile-edit--admission.png`; `states/login--narrow.png` (BRIEF § 6 A4) |
| **Observed, not filed** | **Code names.** Roles print as code names ("InstitutionalAdmin", "PendingTrainee", "Viewing as PendingTrainee"). **Greeting.** Home greets by email, not by name (BRIEF § 6 A8). **Status.** The admission form's summary reads "Status: Active" for a registrar who is not yet admitted, meaning the account's status. **Pending admission.** The table clips the email, and its "Admit to curriculum" action is out of view at 1280 px. | `states/home--awaiting-admission.png`, `states/trainee-profile-edit--admission.png`, `act-2/2.29-2-four-admitted.png` |

## Questions the design must answer

1. **Where issuing lives.** Should issuing an invitation be a form beside the table (as today), or a dialog or a page
   of its own, so that the table has room? The 10-column table shares 1280 px with the form
   (`states/invitations-list--check-address.png`). DESIGN.md § The invitations list (from line 1115) fixes the
   Delivery wording, not the layout.
2. **The one-time link.** How is it shown, so that it is copied once and is not left on screen after a refused issue
   or a revoke? The link is shown only on the page load that issued it (`states/invitations-list--being-sent.png`),
   and today it outlives a refusal (`act-2-onboarding.md:125`, F-2.6a).
3. **The wait.** What does a registrar see between registering and admission? Does it tell her who admits her, and
   when? Today she sees one card naming "your programme administrator", with no name and no date
   (`states/home--awaiting-admission.png`; Step 2.18).

## Notes

- **Register is static** (DESIGN.md § Account / auth page, from line 1856; BRIEF § 5.2):
  - no live validation;
  - the form posts to `/account/register/submit`;
  - a refusal comes back as a full page load with an `?error=` code (T285).
- **The link-sending states come from a scratch instance with an SMTP sink.** The replay logs mail instead of sending
  it, so its rows read "Being sent" and then "Not delivered". Both `invitations-list--sent` and `--check-address` came
  from the scratch instance (`coverage.md` § Flows and states not played; `states.md`).

## Acceptance

The flow is done when BRIEF § 9's checks hold:
- **The steps replay on a fresh database** (`tools/scenario-replay.ps1`):
  - Play Act 1 through Step 1.11, the flow's steps 1.5 and 1.7–1.11 included.
  - Play Act 2 through Step 2.44.
  - Every Expect holds, including 2.28's "System-managed" once T303 lands. Change any Expect whose on-screen wording
    the redesign changes, in the same task (BRIEF § 9, item 7). Record the passwords chosen only in
    `pwd_DO_NOT_COMMIT.txt` (README § Passwords).
- **The 44 states are re-captured,** with the steps' captures, into `design/baseline/`:
  - Crop or mask the one-time link in every capture taken just after an issue or a resend.
  - `invitations-list--sent`, `--check-address`, `register--expired` and `--account-exists` need the scratch database in
    `states.md`.
  - Compare the captures with the chosen artboards.
- **`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj` is green,** without `--no-build`. That run includes:
  - `Design/*` and `Accessibility/*`;
  - `Navigation/NavMenuAuthorizationTests`, for the pending registrar's nav;
  - T294's scenario guard.
- **A browser check passes at 1280 and 390 px** for:
  - `devadmin`;
  - Prof Mbatha;
  - an invitee on the static register page, signed out;
  - a PendingTrainee;
  - a newly admitted Trainee.

## Appendix: the runbook steps (paste after the ask)

Verbatim from `execution/knowledge/scenario-paediatrics/`: Role, Route, Do and Expect only. `D` is the replay day and
`J` the January anchor (README § How to play).

```
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

Step 1.7 — Invite Prof Mbatha as KGK's InstitutionalAdmin
Role: Administrator — the platform operator
Route: /admin/invitations
Do: Invite `mbatha@kgk.wombat.local` as InstitutionalAdmin at `Kgosi Kgari Teaching Hospital`, with no speciality.
Expect: InstitutionalAdmin is the form's default role. The Institution picker offers the Demo Institution and KGK.
  Speciality and Sub-speciality stay disabled for this role. The status line and the link read as in Step 1.5.
  Active invitations now holds two rows: Kruger's, and Mbatha's, which reads InstitutionalAdmin at `Kgosi Kgari Teaching
  Hospital` with no College. The log holds a second stub email, tagged `role:InstitutionalAdmin`, which invites the
  reader to register as InstitutionalAdmin.

Step 1.8 — Dr Kruger registers
Role: Anonymous — Dr Anton Kruger, invited
Route: /account/register → /
Do: In a fresh browser session, open the link from the CollegeAdmin stub email in the application log (the link Step
  1.5 showed). Enter:
  - First name `Anton` and Last name `Kruger`;
  - a password, twice, that meets the policy: at least 12 characters, with an upper-case and a lower-case letter, a
    digit and a symbol.
  Press Register. Write the password to `pwd_DO_NOT_COMMIT.txt`.
Expect: The page reads "Registering kruger@cmsa.wombat.local as CollegeAdmin.", and the address is shown but cannot be
  edited. Register signs Kruger in and lands on Home, which reads "Welcome, kruger@cmsa.wombat.local" and "Viewing as
  CollegeAdmin". The dashboard has one card, National catalogue, with Specialities, EPAs and Curricula. The nav reads
  Home, My Account, Data Rights, Specialities, EPAs, Curricula, Logout.

Step 1.9 — A used link is refused
Role: Anonymous — anyone who holds Kruger's link
Route: /account/register
Do: In another fresh session, open Kruger's link again.
Expect: The page reads "This invitation has already been used." and offers no form. The page refuses up front whatever
  no input could put right (T285).

Step 1.10 — Prof Mbatha registers
Role: Anonymous — Prof Nolwazi Mbatha, invited
Route: /account/register → /
Do: Register as in Step 1.8, from the InstitutionalAdmin stub email in the log, with First name `Nolwazi` and Last name
  `Mbatha`. Write her password to `pwd_DO_NOT_COMMIT.txt`.
Expect: The page reads "Registering mbatha@kgk.wombat.local as InstitutionalAdmin." Register lands her on Home, which
  reads "Viewing as InstitutionalAdmin". The dashboard has three cards:
  - Users, which lists InstitutionalAdmin 1;
  - Specialities & sub-specialities, which reads 0 and 0, because KGK has adopted nothing yet;
  - Quick links: Users, Invitations, Curriculum adoptions and Entrustment decisions.
  The nav reads Home, My Account, Data Rights, Curriculum Adoptions, EPAs, Curricula, Activity Types, Entrustment
  Scales, Trainees, Assessors, Invitations, Users, SSO Mappings, Audit Log, Decision Panels, Committee Reviews,
  Decisions Due, Logout.

Step 1.11 — Both invitations are spent
Role: Administrator — the platform operator
Route: /admin/invitations → /
Do: Reload the invitations list, then go Home.
Expect: Active invitations reads "No active invitations", because a used invitation leaves the list. The dashboard's
  count of registered users is two higher than in Step 1.1.

Step 2.1 — Prof Mbatha opens the invitations page
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /admin/invitations
Do: Sign in and open Invitations from the nav. Read what the issue form offers before issuing anything.
Expect: Active invitations is empty: "No active invitations". Kruger's CollegeAdmin invitation is the College's, not
  KGK's, and Mbatha's own has been used. Role offers InstitutionalAdmin (the default), SpecialityAdmin,
  SubSpecialityAdmin, Coordinator, CommitteeMember, Assessor and Trainee, but not CollegeAdmin, which only an
  Administrator issues (T093). Institution offers KGK only. Speciality is disabled for the default role.

Step 2.2 — Mbatha invites Mr Smit as Coordinator
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue an invitation to `smit@kgk.wombat.local` as Coordinator at KGK, with speciality and sub-speciality left blank.
  Copy the link.
Expect: For a Coordinator, Speciality is optional and Sub-speciality stays disabled (T060). The page reads "Invitation
  issued for smit@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown only once." (T283's
  wording). Below it the link says it expires in 14 days. Active invitations gains a row: Coordinator, KGK, no
  speciality, expiring `D+14`, Delivery "Being sent". The form goes back to its defaults, and the log holds a stub
  "Your Wombat invitation" tagged `role:Coordinator`.

Step 2.3 — Mbatha invites Dr Zulu, Dr Naidoo and Dr Botha as committee members
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue one invitation each to `zulu@`, `naidoo@` and `botha@kgk.wombat.local` as CommitteeMember at KGK, with
  speciality Paediatrics and no sub-speciality. Copy each link before issuing the next.
Expect: Speciality offers Paediatrics alone: KGK's adopted specialities (T092). Sub-speciality stays disabled, because a
  committee member may not be scoped to one. Three rows read CommitteeMember, KGK, Paediatrics, with the sub-speciality
  blank.

Step 2.4 — Mbatha invites Dr Patel and Dr Khumalo as assessors
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue an invitation each to `patel@` and `khumalo@kgk.wombat.local` as Assessor at KGK, with speciality Paediatrics
  and sub-speciality Paediatrics.
Expect: For an Assessor the Sub-speciality select is enabled. Once Paediatrics is chosen as the speciality, it offers
  that speciality's sub-speciality, Paediatrics. Two rows read Assessor, KGK, Paediatrics, Paediatrics.

Step 2.5 — Mbatha invites Dr van Rensburg as an external committee member
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue an invitation to `vanrensburg@sun.wombat.local` as CommitteeMember at KGK, with speciality and sub-speciality
  blank.
Expect: It is issued with no speciality (T060). The row reads CommitteeMember, KGK, with both scope columns blank. The
  address's domain does not matter: the invitation scopes him to KGK.

Step 2.6 — Mbatha invites Dr Mokoena as the programme's SpecialityAdmin
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue `mokoena@kgk.wombat.local` as SpecialityAdmin at KGK with no speciality. Then issue it again with speciality
  Paediatrics.
Expect: The first is refused: "Speciality administrators must be scoped to a speciality." (T060). Nothing is issued and
  no link is shown. The second is issued. Sub-speciality stays disabled for this role, and the row reads SpecialityAdmin,
  KGK, Paediatrics.

Step 2.7 — Mbatha invites Dr Sithole as the SubSpecialityAdmin
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: Issue `sithole@kgk.wombat.local` as SubSpecialityAdmin at KGK with speciality Paediatrics and no sub-speciality.
  Then issue it again with sub-speciality Paediatrics.
Expect: The first is refused: "The selected role requires speciality and sub-speciality scope." The second is issued.
  Active invitations now holds nine rows: Smit, Zulu, Naidoo, Botha, Patel, Khumalo, van Rensburg, Mokoena and Sithole,
  each with the role and scope above.

Step 2.8 — Mr Smit registers from his link
Role: Anonymous — Mr Pieter Smit, holding his invitation link
Route: /account/register → / → /account/logout → /account/login
Do: Open the link and enter first name Pieter, last name Smit and a password that meets the rules. Confirm it and
  register. Read the landing page, then sign out.
Expect: The page reads "Registering smit@kgk.wombat.local as Coordinator.", with the email filled in and not editable.
  The token is cleared from the address bar once the page loads. Registering signs him in and lands on Home: "Welcome,
  smit@kgk.wombat.local", "Viewing as Coordinator". Signing out returns him to the sign-in page.

Step 2.9 — Dr Patel's first two attempts are refused
Role: Anonymous — Dr Mohammed Patel, holding his invitation link
Route: /account/register → / → /account/logout
Do: Register with a 10-character password that mixes upper case, lower case, a digit and a symbol. Then use a
  12-character password with a different confirmation. Then use a 12-character password confirmed correctly. Sign out.
Expect: The first attempt returns to the form with "Passwords must be at least 12 characters." The first field takes the
  focus and no account is created (T285). The second returns with "The password confirmation does not match." The
  third registers him and lands on "Viewing as Assessor".

Step 2.10 — The other seven staff register
Role: Anonymous — Dr Zulu, Dr Naidoo, Dr Botha, Dr Khumalo, Dr van Rensburg, Dr Mokoena and Dr Sithole
Route: /account/register → / → /account/logout
Do: Each registers from their own link with the cast's name and signs out. Dr Khumalo types her first name as "Fatma"
  by mistake (she corrects it in Step 2.41).
Expect: Each page names the address and the invited role. Each person lands on Home viewing as that role. Zulu, Naidoo,
  Botha and van Rensburg view as CommitteeMember, Khumalo as Assessor, Mokoena as SpecialityAdmin and Sithole as
  SubSpecialityAdmin.

Step 2.11 — A used link cannot be used again
Role: Anonymous — Mr Pieter Smit
Route: /account/register
Do: Open his invitation link a second time.
Expect: No form is shown, only "This invitation has already been used." (T285's words).

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

Step 2.18 — Four registrars register and are pending
Role: Anonymous — Dr Molefe, Dr Dlamini, Dr Mahlangu and Dr Ndlovu
Route: /account/register → / → /account/logout
Do: Each registers from their own link. Molefe, Dlamini and Ndlovu sign out. Dr Mahlangu stays signed in with Home open
  in her tab, which Step 2.31 uses. Dr du Plessis cannot register: he says no email reached him.
Expect: Each page reads "Registering <address> as Trainee." Each registrar lands on "Viewing as PendingTrainee", because
  a Trainee invitation registers as PendingTrainee until admission. Home shows one card, "Awaiting admission": "You are
  registered and waiting to be admitted to a curriculum by your programme administrator." It has a "Review your account
  →" link.

Step 2.19 — What a registrar sees before admission
Role: PendingTrainee — Dr Nomsa Mahlangu
Route: / → /account/profile → /activities/mine → /activities/new → /portfolio/progress → /access-denied → /
Do: Read the nav. Follow Review your account, then open My Activities and Activities, and file nothing. Type the My
  Progress address.
Expect: The nav reads Home, My Account, Data Rights, Activities, My Activities and Logout. It has no MSF Reports, My
  Committee Reviews, My Progress or Export Portfolio, because those pages do not admit a pending trainee (T141). My
  account lists her role as PendingTrainee. My Activities reads "No activities yet". Activities opens the type picker
  with the eleven instruments of Step 2.42: her invitation's Paediatrics scope selects them, and with no curriculum yet
  no ladder narrows them. My Progress shows Access denied ("You do not have permission to view this page.") with Back
  to home.

Step 2.26 — Mbatha resends Dr du Plessis's invitation
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations
Do: At least an hour after Step 2.16, open Invitations and press Resend on du Plessis's row. Copy the new link.
Expect: His is the only row left. Its Delivery reads "Not delivered. Resend emails a new link in place of the current
  one, which then stops working.", and it offers Resend before Revoke. Afterwards the page reads "A new invitation link
  is being emailed to duplessis@kgk.wombat.local. The link it replaces no longer works. Copy the new link below — it is
  shown only once.", with the link below it. The row reads "Being sent", offers no Resend and expires 14 days from now.

Step 2.27 — Dr du Plessis registers from the new link
Role: Anonymous — Dr Pieter du Plessis
Route: /account/register → / → /account/logout
Do: Open the first link, then the resent one. Register from the resent link and sign out.
Expect: The first link shows no form, only "This invitation is invalid.": the resend replaced the link's hash, so the old
  token matches no invitation. The resent link registers him and lands on "Viewing as PendingTrainee" with the Awaiting
  admission card.

Step 2.28 — The pending registrars, as Mbatha sees them
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations → /admin/users → /admin/users/{UserId} → /admin/trainees
Do: Open Invitations. Read the users list and open Dr Molefe's page. Then open Trainees.
Expect: Active invitations reads "No active invitations". Users lists 15, with the five registrars as PendingTrainee. On
  Molefe's page, PendingTrainee reads "System-managed" and has no Remove. Trainees' Pending admission lists the five with their emails and KGK, each with an
  "Admit to curriculum" named for the registrar. Active profiles reads "No active trainee profiles found."

Step 2.29 — Mbatha admits four registrars
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Admit each with its programme start and expected completion. Molefe starts `J−3y` and completes `J+1y−1d`. Dlamini
  starts `J−2y` and completes `J+2y−1d`. Du Plessis starts `J−1y` and completes `J+3y−1d`. Mahlangu starts `J` and
  completes `J+4y−1d`.
Expect: The form reads "Trainee profile" / "Admit a pending trainee into a curriculum.". Curriculum offers only
  "Paediatric EPA Curriculum (11.1)", KGK's adopted version (T091). The start date is pre-filled with today. There is no
  training-year field, because the year is derived. Each admission moves to the profile's edit page (`?id=`). That page
  shows Status Active and "Update curriculum and completion details for this trainee.", with Last day in the programme,
  Deactivate and Mark complete, none of them used here. The four appear under Active profiles with the curriculum,
  Paediatrics and the completion date entered.

Step 2.30 — Dr Ndlovu's completion date is derived, then corrected
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/trainees → /admin/trainees/edit → /admin/trainees
Do: Admit Ndlovu with start `J` and the completion date left empty. Then set completion to `J+4y−1d` on his profile and
  save the profile.
Expect: Left empty, the completion date is derived as `J` + 12 months, the longest item window on v11.1. The help text
  says so ("Leave this empty to derive it from the curriculum window."). The save reads "Trainee profile saved.", and
  the list shows `J+4y−1d`. Pending admission reads "No pending trainees found."

Step 2.31 — Dr Mahlangu's open session ends, and she signs back in as a Trainee
Role: Trainee — Dr Nomsa Mahlangu
Route: / → /account/session-ended → /account/login → /
Do: Her tab from Step 2.18 has stayed open through her admission. Once it leaves Home, she signs in again.
Expect: Within a minute of her admission the tab moves to the sign-in page, which reads "Your session has ended. Please
  sign in again." Admission changes her role, and a role change ends open sessions (T279). Signed in again, she sees
  "Viewing as Trainee" and the trainee dashboard of Step 2.39.

Step 2.32 — Mr Smit, Coordinator
Role: Coordinator — Mr Pieter Smit
Route: /account/login → / → /admin/invitations → /access-denied → /committee/panels → /committee/panels/new → /access-denied
Do: Sign in and read the dashboard and nav. Type the invitations address. Type the Decision Panels address, then the
  new-panel address.
Expect: The dashboard reads "No stalled requests.", "No invitations expiring soon." and a Quick action, "Start an MSF
  campaign". The nav adds Data Rights Requests, MSF Campaigns, Committee Reviews, Decisions Due and Stalled Activities.
  It has no Invitations, and the invitations page shows Access denied (T178). It has no Decision Panels either, though
  that page admits him: it lists the panel with no New panel and no Edit column. The panel form shows Access denied.

Step 2.44 — Prof Mbatha's dashboard after onboarding
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/assessors
Do: Open Home. Then open Users and Assessors.
Expect: The page reads "Viewing as InstitutionalAdmin". Users reads InstitutionalAdmin 1, SpecialityAdmin 1,
  SubSpecialityAdmin 1, Coordinator 1, CommitteeMember 4, Assessor 5 and Trainee 5, with no PendingTrainee line.
  Specialities & sub-specialities reads 1 and 1, what KGK has adopted. Quick links reads Users, Invitations, Curriculum
  adoptions and Entrustment decisions. The users and assessors lists both name Fatima Khumalo (Step 2.41).
```
