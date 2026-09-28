# F12 The institution's people: users, roles, assessors, locks and resets

The InstitutionalAdmin keeps her institution's people. She reviews its users, adds a second role, records assessor
training, resets a password, locks someone out and lets them back in, and keeps her own institution's record and
nobody else's. Role grants and locks are access control. It is flow 12 of 18 (`design/BRIEF.md` § 8).

| | |
|---|---|
| **Mode** | Straight to fidelity for the lists (Users, Assessors). Wireframe first for the user page, which brings roles, scope, invitations, lock and reset into one place, and for the InstitutionalAdmin dashboard. |
| **Viewports** | Desktop 1280×800 and phone 390×844. |
| **Held** | Nothing. T303 (Add role offered Trainee) landed in 4824d62 and its captures were retaken on 2026-09-26 (§ Attach), as were T302's after 41be531. Brief every screen, the user page's role section included. |
| **Frequency** | Monthly: rotations, staff joining, lockouts. |
| **Stakes** | High. A role grant or a lock decides who can see and do what. |
| **People** | Prof Nolwazi Mbatha (InstitutionalAdmin, KGK). Her subjects: Dr Thandi Zulu (gains Assessor), Dr Mohammed Patel (locked out, then reactivated) and Dr Pieter du Plessis (password reset). Also Dr Anele Dlamini, whose account she opens on her phone (Step A.7.9), and Dr Sipho Ndlovu, whose data was erased (Step A.1.14). |

**How to use this brief**

1. Open a new Claude Design thread for this flow (BRIEF § 2.3, step 3).
2. Paste the block under **The ask**, then the block under **Appendix: the runbook steps**.
3. Attach the images listed under **Attach**, key screenshots first. Open each one before you upload it (BRIEF § 3.3).
   None of this flow's captures shows a password (`act-A/A.4.5-2-reset.png` was checked, BRIEF § 3.3).
4. Answer Claude's questions in the chat, each as a sentence [academy].
5. Export the chosen artboards to `design/flows/12-people-admin/` before moving on [start].

## The ask

```
FLOW 12 — Keep my institution's people: their roles, assessor training, passwords and access

DIRECTION: <paste the aesthetic direction recorded as W-nnn in execution/DECISIONS.md (design/BRIEF.md § 2.3 step 0)>
DESIGN SYSTEM: Wombat, as set up for this project:
  - the :root tokens of app.css;
  - Fraunces on the wordmark only;
  - Lucide line icons;
  - the components named in CONSTRAINTS.
  These screens sit inside the shell settled in flow 01. Do not redesign the shell here.

GOAL: The InstitutionalAdmin:
  - reviews her institution's users;
  - gives a consultant a second role;
  - records each assessor's training;
  - resets a registrar's password;
  - locks a consultant out and lets him back in;
  - keeps her own institution's record, and sees no other institution's.
  Role grants and locks are access control. A record outside her institution is "not found", never "forbidden".
  What is wrong today:
  - Lock out user is a red button that acts at once, with no confirmation. The Remove buttons beside each role are red
    too, and sit against the role's name.
  - At 1280 px, the Account summary clips a long email, and the Pending invitations table clips inside its narrow card.
  - On her own institution's record she can untick Active and press Deactivate (being fixed).
  - The assessors' empty state claims that a profile makes an assessor selectable.
  - The SSO page tells her to edit application settings, and points at a form that is not there.

AUDIENCE:
  - InstitutionalAdmin Prof Nolwazi Mbatha, head of department at a teaching hospital, at a desk and on her phone.
  - The people she manages: consultants, a coordinator, programme leads and registrars.
  - Frequency: monthly.
  - Stakes: high.
  - Viewports: desktop 1280×800 and phone 390×844.

SCREENS, in order:
  1. / — the InstitutionalAdmin's dashboard.
     - Cards:
       - Users, with a count per role;
       - Specialities & sub-specialities, counting what the institution has adopted;
       - Quick links: Users, Invitations, Curriculum adoptions and Entrustment decisions.
     - Also at 390 px.
  2. /admin/users: Users.
     - A filter by name or email.
     - A table: name, email, institution, roles and status, with Manage named per row.
     - States: KGK's staff (10 rows); filtered ("zulu"); no match ("No users match the current filter."); with pending
       registrars; after an erasure (the erased registrar is gone); loading; 390 px.
  3. /admin/users/{UserId}: one user, titled by the person's name.
     - Account summary: name, email, institution and status.
     - Roles:
       - each role with Remove;
       - Trainee and PendingTrainee read "System-managed", with no Remove;
       - Add role, offering InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Coordinator and Assessor. Never
         Administrator, CollegeAdmin, PendingTrainee or Trainee.
       - The result, for example "Role 'Assessor' added."
     - Reset password: it sets a password directly, and the user is not emailed. It is refused with the rules the
       password breaks; when done, the field is cleared.
     - Lockout:
       - what a lock does until the user is reactivated (sits on no panel; is not a current trainee; is sent no
         reminders; cannot be named as an assessor), and that their records stay;
       - Lock out user, then "User locked out." with the status Locked out;
       - Reactivate user.
     - Pending invitations to this email: a table, and Revoke.
     - Her own account: her roles with no Remove, and no Add role, Reset or Lockout. It shows the note "This is your
       own account, so you cannot change its roles, lockout or password here. Another administrator can change your
       roles or lockout." and a "Change your password" link.
     - States: her own; another's; role added; no roles (a graduate); reset refused; reset done; locked; reactivated;
       unavailable (out of scope: "User unavailable" / "The user could not be found or is outside your scope."); pending
       invitations; loading; 390 px.
  4. /admin/assessors and /admin/assessors/edit: assessor training.
     - The list: name, institution, speciality / sub-speciality, training status, and training completed (a date or
       "Not recorded"), with Edit named per assessor.
     - The profile form:
       - Assessor user;
       - Institution (hers only);
       - Speciality and Sub-speciality;
       - Qualifications;
       - Training status: Not started, In training, Provisional (awaiting sign-off) or Trained;
       - the completion date, asked only for Provisional or Trained.
     - States: none; five; new; provisional (with the date); required field empty; saved ("Assessor profile saved.");
       unknown id; 390 px.
  5. /admin/institutions/{Id:int}: her own institution's record.
     - She edits the name, short code and contact email.
     - Status is shown as text. There is no Active box, no Deactivate, and no link to the Institutions list, which is
       the Administrator's.
     - Another institution's id: Page not found.
  6. /admin/sso/group-mappings: SSO group mappings, with no identity provider configured.
     - It says, in her terms, that institutional sign-in is not set up, and whom to ask.
     - Current mappings: empty.
     - Nothing can be changed.

STEPS: 1.23, 2.12–2.15, 2.44, A.1.14, A.3.1, A.4.5, A.5.5, A.6.3, A.6.4, A.6.7, A.7.9 and A.7.14.
  - They are pasted verbatim after this block (Role / Route / Do / Expect).
  - The Expect lines describe the product as replayed on 2026-09-26. Where a requirement below differs, the
    requirement wins.
  - Step A.6.3's Deactivate and untick-Active refusals ceased to exist with T302; its Expect was rewritten for it.

STATES TO SHOW:
  - Every state listed under SCREENS.
  - For each page: loading (a skeleton under the header), the load error, and narrow (390 px).
  - Data volumes:
    - none;
    - typical: 15 users, 5 assessors;
    - heavy: 200 users with pagination ("Showing 1–50 of N"), a person with four roles, and an email of 60 characters.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T302 (landed): an InstitutionalAdmin sees her institution's Status as text, "Set by a global administrator.", with
    no Active box and no Deactivate, whether it is active or not. The "deactivate refused" and "untick and save" states
    no longer exist.
  - T303 (landed): Trainee reads "System-managed" on the user page and is never offered under Add role.
  - T264:
    - Lock out asks first, in a confirmation that names the person.
    - Destructive triggers in a row or card (a role's Remove, Revoke all pending invitations) are outline buttons,
      named per row.
    - A destructive button is not offered on a record that is already inactive.
  - T323:
    - Long emails and GUIDs wrap inside their card at 1280 px.
    - The Pending invitations table does not clip.
    - Each role's Remove has a gap before it.
  - T326:
    - The assessors' empty state says what a profile records. It does not claim that a profile makes someone
      selectable (the picker needs only the role).
    - The SSO page does not point at a form that does not render, and does not tell an InstitutionalAdmin to edit
      application settings. She is told to ask a Wombat administrator.
  - T289: the assessor profile form refuses anyone who holds Trainee, and offers only users at her own institution. A
    save moves to the saved profile's page, and the new form offers only assessors without a profile.
  - T288: once a provider exists, a mapping can grant roles only at her institution. There is no institution to pick
    for her.
  - T321: Access denied reached from an in-app link is drawn once, not inside a second layout.
  - T322: white on the danger button, the focus ring and input borders reach AA (3.82, 2.99 and 1.49 today).
  - T324: validation names the field's label: "Assessor user is required.", not "The UserId field is required."
  - T286: the user page lists the person's institutional sign-in links, and lets her remove one.
  - T317: Reset password enables at the real minimum of 12 characters, not 8, and its refusal reads as sentences.

REQUIREMENTS EVERY WOMBAT PAGE MEETS (design/BRIEF.md § 6; the ones this flow tests):
  A1  Contrast in the tokens: the danger button, the focus ring and input borders.
  A3  At 1280 px, emails and GUIDs wrap and tables show their actions. At 390 px, cards stack and buttons wrap.
  A5  Every action reports its outcome where the focus lands. Destructive actions confirm and name their target.
  A6  Loading, load-error and not-found states are designed.
  A7  Access denied is drawn once.
  A8  People by name; roles and statuses by label.
  A10 The copy says what the page does for this viewer.
  A11 Offer only what the caller can do.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Is the user page one page with sections (account; roles and scope; invitations; security, meaning password, lock
     and sign-in links), or tabs?
  2. Should assessor training live on the user page, instead of on a separate Assessors list?
  3. Should the InstitutionalAdmin have a link to her own institution's record? Today she reaches it only by typing
     its address.

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

ASK:
  - 2–3 variations.
  - Full fidelity for the lists (screens 2 and 4). Wireframes first for the dashboard (screen 1) and the user page
    (screen 3), then fidelity for the one I pick.
  - Name every design-system component you use, and mark anything else NEW.
  - Say which DESIGN.md rule a variation breaks.
  - Flag edge cases, and review each screen's accessibility against the A-requirements above.

ATTACHED (key screenshots first):
  - states/users-list--staff.png, user-detail--locked.png;
  - act-A/A.7.14-5-danger-button.png;
  - states/assessor-profile-edit--provisional.png, assessors-list--empty.png, group-mappings--no-provider.png,
    user-detail--pending-invitations.png, home--institutional-admin.png.
  - A full-page capture draws the sidebar part-way down a long page, or ends it early. That is the capture, not the
    product.
```

## The journey

| Step | Route | Who does what | What they must be able to see |
|---|---|---|---|
| 1.23 | `/admin/institutions/{Id:int}` → `/not-found` | Prof Mbatha opens KGK's record by typing its address, then the Demo Institution's id. | KGK's name, short code, contact email and status. Save, and nothing that would deactivate KGK. No link to the Institutions list. The other id: Page not found. |
| 2.12 | `/admin/invitations` → `/admin/users` → `/admin/users/{UserId}` | She reviews KGK's users and opens her own account. | Ten rows, KGK's only (T056). The filter. Her own page: roles with no Remove, the own-account note, and "Change your password" (T278). |
| 2.13 | `/admin/users` → `/admin/users/{UserId}` | She adds Assessor to Dr Zulu, Dr Naidoo and Dr Botha. | Add role's offer: no Trainee, with a help line saying why (T303, re-captured). The Lockout card's list (T284). "Role 'Assessor' added." Both roles in the list. |
| 2.14 | `/admin/assessors` → `/admin/assessors/edit` → `/admin/assessors` | She creates five assessor profiles. | "No assessor profiles". KGK's five Assessors offered, and KGK alone. The date asked only for Provisional or Trained. "Assessor profile saved." |
| 2.15 | `/admin/assessors` | She reads the list. | Five rows, with statuses and dates ("Not recorded" where none). Edit named per assessor. Dr van Rensburg absent: he is not an Assessor. |
| 2.44 | `/` → `/admin/users` → `/admin/assessors` | She reads her dashboard after onboarding. | Users per role, and the adopted count, which is wrong until T291 item 4 lands. Quick links. Dr Khumalo's corrected name. |
| A.1.14 | `/admin/users` → `/admin/trainees` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | She looks for Dr Ndlovu after his erasure. | He is gone from users and trainees. His reviews appear under a `deleted_user_…` pseudonym. |
| A.3.1 | `/admin/sso/group-mappings` | She opens SSO Mappings; no provider is configured. | That no provider is configured; no form; "No group mappings". Nothing can be changed (T326 for the copy). |
| A.4.5 | `/admin/users` → `/admin/users/{UserId}` | She resets Dr du Plessis's password: 10 characters, then a valid one. | That the user is not emailed. The rules a password breaks. The field cleared on success. |
| A.5.5 | `/admin/institutions/{Id:int}` → `/not-found` → `/admin/users/{UserId}` | She opens another institution's record, then another institution's user, by id. | Page not found; then "User unavailable". Neither page confirms that the record exists. |
| A.6.3 | `/admin/institutions/{Id:int}` → `/` | She changes KGK's contact email, reads Status, and looks at Back to home and Cancel. | "Institution saved.", with the focus. Status as text, "Active" and "Set by a global administrator.": no Active box and no Deactivate (T302). "Back to home" and Cancel both lead to `/` (T291 item 7, landed with T302; re-checked 2026-09-26). |
| A.6.4 | `/admin/users` → `/admin/users/{UserId}` | She locks Dr Patel out while he is signed in elsewhere. | What a lock does. "Locked out", and Reactivate user, with the focus on the result. Today nothing asks first (T264). |
| A.6.7 | `/admin/users/{UserId}` | She reactivates Dr Patel. | Active again, and Lock out user offered again. The focus on the result. |
| A.7.9 | `/` → `/admin/users` → `/admin/users/{UserId}` → `/admin/trainees` → `/admin/trainees/edit` → `/admin/activity-types/{ActivityTypeId:int}` | She works on her phone. | The user page's cards stacked, and its buttons wrapped. The trainee profile and the builder belong to flows 11 and 17. |
| A.7.14 | `/account/login` → `/` → … → `/admin/users/{UserId}` → … | She checks her pages' contrast. | Every pair at WCAG 2.1 AA: text 4.5:1, and control boundaries and the focus ring 3:1. Today several fail (BRIEF § 6 A1). |

## States to design

The states are from `scenario-paediatrics/states.md` § Home and § Institution administration. Each is
`design/baseline/states/<name>.png`.

| Page | States (screenshot names) |
|---|---|
| `/` | `home--institutional-admin` (Step 2.44); `home--narrow-institutional-admin` (Step A.7.9) |
| `/admin/users` | `users-list--staff`, `--filtered`, `--no-match` (Step 2.12); `--loading`; `--narrow` (Step A.7.9) |
| `/admin/users/{UserId}` | `user-detail--own` (Step 2.12); `--other` and `--role-added` (Step 2.13); `--no-roles` (Step 5.17); `--reset-refused` and `--reset` (Step A.4.5); `--locked` (Step A.6.4); `--reactivated` (Step A.6.7); `--unavailable` (Step A.5.5); `--pending-invitations` (scratch: Dr Botha invited again); `--loading`; `--narrow` (Step A.7.9). Every one with an Add role card was re-captured after T303 (2026-09-26) |
| `/admin/assessors` | `assessors-list--empty` (Step 2.14); `--five` (Step 2.15); `--loading`; `--narrow` |
| `/admin/assessors/edit` | `assessor-profile-edit--new`, `--provisional`, `--invalid`, `--saved` (Step 2.14); `--not-found` (typed `?id=999999`); `--narrow` |
| `/admin/institutions/{Id:int}` | `institution-edit--own` (Step 1.23, re-captured after T302); `--deactivate-refused` (Step A.6.3: that state ceased with T302, and the file now holds her page after the save, Status as text); `--not-found` (Step 1.23, the Demo Institution) |
| `/admin/sso/group-mappings` | `group-mappings--no-provider` (Step A.3.1); `--narrow` |

That is 34 captures. Two users-list states belong to other flows and are useful here too: `users-list--pending`
(flow 11, Step 2.28) and `users-list--after-erasure` (flow 14, Step A.1.14). No replay reaches four states:
- the SSO add form, its mapping rows and Delete, which need a configured provider (`states.md` § States no local replay
  reaches);
- the "holds Trainee" notes on Users and a user (DESIGN.md:1494–1498), since no cast member holds Trainee beside a
  staff role.

Design both from the text.

## Attach

Paths are relative to `design/baseline/`. Every file below was found on disk on 2026-09-26. Open each one before you
upload it (BRIEF § 3.3).

**First, the key screenshots:**
1. `states/users-list--staff.png`
2. `states/user-detail--locked.png`
3. `act-A/A.7.14-5-danger-button.png`
4. `states/assessor-profile-edit--provisional.png`
5. `states/assessors-list--empty.png`
6. `states/group-mappings--no-provider.png`
7. `states/user-detail--pending-invitations.png`
8. `states/home--institutional-admin.png`

**Then, as the chat asks for them:**
- **Screen 1:**
  - `states/home--narrow-institutional-admin.png`
- **Screen 2:**
  - `states/users-list--filtered.png`
  - `states/users-list--no-match.png`
  - `states/users-list--loading.png`
  - `states/users-list--narrow.png`
- **Screen 3:**
  - `states/user-detail--own.png`
  - `states/user-detail--reactivated.png`
  - `states/user-detail--unavailable.png`
  - `states/user-detail--loading.png`
  - `act-A/A.6.4-1-patel-before-lock.png`
- **Screen 4:**
  - `states/assessors-list--five.png`
  - `states/assessors-list--loading.png`
  - `states/assessors-list--narrow.png`
  - `states/assessor-profile-edit--new.png`
  - `states/assessor-profile-edit--invalid.png` (T324's property name)
  - `states/assessor-profile-edit--saved.png`
  - `states/assessor-profile-edit--not-found.png`
  - `states/assessor-profile-edit--narrow.png`
- **Screen 5:**
  - `states/institution-edit--not-found.png`
  - `act-A/A.5.5-1-demo-institution-not-found.png`
  - Back to institutions → Access denied (T321's second layout) no longer exists: the link went with T302 (T291
    item 7), so her Back and Cancel lead home, and no step reaches it. Flow 01 designs Access denied.
  - The re-captured institution record (T302, 2026-09-26; Step A.6.3's captures re-taken by the T335 replay):
    `states/institution-edit--own.png`, `act-1/1.23-1-kgk-own-record.png` (Step 1.23, before any change),
    `act-A/A.6.3-1-contact-saved.png` and `states/institution-edit--deactivate-refused.png` (after her save: Status as
    text, Cancel and Save), and `act-A/A.6.3-2-cancel-home.png` (her Home, where Back to home and Cancel lead). The
    ceased state `institution-edit--deactivate-refused` kept its file name (BRIEF § 10).
- **Screen 6:**
  - `states/group-mappings--narrow.png`

**Re-captured after T303 landed (4824d62, 2026-09-26), so no longer held:**
- T303:
  - `states/user-detail--no-roles.png`
  - `states/user-detail--reset-refused.png`
  - `states/user-detail--reset.png`
  - `states/user-detail--narrow.png`
  - `states/user-detail--other.png`
  - `states/user-detail--role-added.png`
  - `act-A/A.7.9-3-user-390.png`
  - also, because T303's help line under Add role changed them: `states/user-detail--locked.png`, `--reactivated.png`
    and `--pending-invitations.png`, and `act-A/A.6.4-1`, `A.6.4-2`, `A.6.7-1`, `A.4.5-1` and `A.4.5-2`.

`--other` and `--role-added` come from a fresh replay of Act 2, so they show Step 2.13's own moment; `--no-roles` is
from the end of Act 5; the rest are from a post-actA copy (BRIEF § 10). A held Trainee now reads "System-managed" with no
Remove (`--reset`, `--narrow`). Every Roles row still runs the role's name into its note or its Remove with no gap
("TraineeSystem-managed", F-2.28b, observed): T323's Remove item, below, covers both.

T302's six held captures were re-captured on 2026-09-26 after it landed (41be531) and are listed under Screen 5 above.

`states/user-detail--reset-refused.png` is also T323's evidence of an address clipped at 1280 px, and still shows it
after the re-capture ("duplessis@kgk.wombat.l", observed). `states/user-detail--pending-invitations.png` shows the same
clip.

## Known problems this design must solve

| Task | What it means for the design | Evidence |
|---|---|---|
| **T302** (group 1, landed in 41be531) | **Her own institution.** She sees Status as text, with no Active box and no Deactivate. The deactivate-refused and untick-and-save states no longer exist. **Related, T291 item 7, landed with T302.** "Back to home" and Cancel go to Home for her, not to the Administrator's list. **Item 8, still open:** she is still shown the create form if she types its address; she should never be. | Screen 5's re-captured captures; `act-1/1.23-4-institutional-admin-create-form.png` (re-captured 2026-09-26: the form, now with "Back to home") (Step 1.23's capture of the old "Back to institutions" link was removed by the T335 replay: the link no longer exists) |
| **T303** (group 1, landed in 4824d62) | **Trainee.** It is "System-managed" on the user page, with no Remove, and never offered under Add role. | The re-captured images above; Steps 2.13 and 2.28 |
| **T264** | **Lock out user.** It asks first, naming the person, and its trigger is an outline button; today it is red and acts at once. **In-row destructive buttons.** A role's Remove and "Revoke all pending invitations" are red today (observed). They become outline buttons with per-row names. **Inactive records.** A destructive button is not offered on a record that is already inactive. | `act-A/A.7.14-5-danger-button.png`, `states/user-detail--locked.png`, `states/user-detail--pending-invitations.png`; Step A.6.4 |
| **T323** (A3) | **Long values.** Emails and GUIDs wrap inside their card at 1280 px (`overflow-wrap: anywhere` on the details list). **Pending invitations.** The table fits its card or moves to a wider column. **Remove.** Each role's button, and the "System-managed" note, has a gap before it; today "Assessor" runs into its button and "Trainee" into its note (observed; F-2.28b). | `states/user-detail--reset-refused.png`, `states/user-detail--pending-invitations.png`; Step A.4.5 Gap F-A.4.5c |
| **T326** (A10) | **Assessors' empty state.** It says what a profile records (training status and date, qualifications); the nominee picker needs only the role. **SSO page.** With no provider, it does not point at a form ("Add a mapping above…"), and it tells an InstitutionalAdmin to ask a Wombat administrator instead of naming `Sso:Providers`. | `states/assessors-list--empty.png`, `states/group-mappings--no-provider.png`; Step 2.14 Gap F-2.14c |
| **T289** (P2) | **The assessor profile.** It refuses anyone who holds Trainee, and requires the user to be at her institution. **After a save.** The page moves to the saved profile (`?id=`), and the new form's picker offers only assessors without a profile (Step 2.14 Gap F-2.14a, F-2.14b). | T289; Step 2.14 |
| **T288** (P2) | **SSO mappings, once a provider exists.** A mapping grants roles only at the provider's institution, which is also hers. There is no free choice of institution for her. Not reachable on the replay. | T288; `coverage.md` § Flows and states not played |
| **T321** (A7) | Access denied reached from an in-app link is drawn once, in one layout. | None now: Back to institutions, the link that reached it, went with T302 (Screen 5). Flow 01 designs Access denied |
| **T322** (A1) | White on the danger button is 3.82 today; the focus ring on the page 2.99; input borders 1.49. Each must reach AA. | `act-A/A.7.14-5-danger-button.png` (re-taken after T335's tokens: white on it is 5.95 now); BRIEF § 6's failing pairs before T335 |
| **T324** (A8) | The assessor form's empty save reads "The UserId field is required." Name the field "Assessor user". | `states/assessor-profile-edit--invalid.png`; Step 2.14 Gap F-2.14d |
| **T286** | The user page lists the person's institutional sign-in links, to the account holder and to an admin, with Remove. It must also be decided whether a password reset drops them. | T286 (External logins) |
| **T317** | Reset password enables at 12 characters, Identity's minimum, not 8. Its refusal reads as sentences joined by spaces, not "12 characters.; Passwords …". | Step A.4.5's note and Gap F-A.4.5a, F-A.4.5b |
| **T291 item 4** | The dashboard's Specialities & sub-specialities card counts what KGK has adopted (1 and 1), not the national catalogue (2 and 2 today). | `states/home--institutional-admin.png` (observed: 2 and 2); Step 2.44's note |
| **Observed, not filed** | **Greeting.** Home greets by email ("Welcome, mbatha@kgk.wombat.local"). **Code names.** Roles print as code names ("InstitutionalAdmin", "CommitteeMember") on the dashboard and the user page (BRIEF § 6 A8). | `states/home--institutional-admin.png`, `states/user-detail--locked.png` |

This flow also answers BRIEF § 7 B8 for `/admin/institutions/{Id:int}`, a page reached only by address.

## Questions the design must answer

1. **Sections or tabs.** Is the user page one page with sections (account; roles and scope; invitations; security) or
   tabs? Today it is a grid of five cards (`states/user-detail--locked.png`): Account summary, Roles, Reset password,
   Lockout and Pending invitations. T286 would add a sixth, for sign-in links. DESIGN.md:1494–1505 fixes what the page
   shows on one's own account.
2. **Assessor training.** Should it live on the user page, instead of on a separate Assessors list? The profile is one
   per Assessor, and saving it sets the user's scope (Step 2.14's note; T279), so it is part of that user's access.
3. **Her own institution.** Should the InstitutionalAdmin have a link to her institution's record?
   `/admin/institutions/{Id:int}` is in `coverage.md` § Reached only by address (Steps 1.23 and A.6.3); her nav and
   Quick links do not offer it (`states/home--institutional-admin.png`).

## Notes

- **Out-of-scope ids are a 404, not a 403** (CLAUDE.md § InstitutionalAdmin scope-aware powers):
  - the other institution's record reads Page not found;
  - another institution's user reads "User unavailable" (`states/user-detail--unavailable.png`; Step A.5.5).
  The design must not add a "you do not have access" variant for these.
- **Reset and lock end open sessions** (T279; Steps A.6.4, A.6.5). The page need not show that live; the session
  states belong to flow 02.

## Acceptance

The flow is done when BRIEF § 9's checks hold:
- **The steps replay on a fresh database** (`tools/scenario-replay.ps1`):
  - Play Act 1 through Step 1.23, and Act 2 through Step 2.44, the flow's steps 2.12–2.15 included.
  - Play the rest of Acts 2–6.
  - Play the appendix through Steps A.1.14, A.3.1, A.4.5, A.5.5, A.6.3, A.6.4, A.6.7, A.7.9 and A.7.14.
  - Every Expect holds, with A.6.3's rewritten by T302 and 2.13's Add role list by T303. Change any Expect whose
    on-screen wording the redesign changes, in the same task (BRIEF § 9, item 7). Record the reset password only in
    `pwd_DO_NOT_COMMIT.txt`.
- **The 34 states are re-captured.** `institution-edit--deactivate-refused` names a state that no longer exists; its
  file holds her page after the save (BRIEF § 10).
  - Take the steps' captures too:
    - Act 1: `1.23-1` and `1.23-3`;
    - Act 2: `2.12-2`, `2.12-3`, `2.13-1`, `2.13-2`, `2.14-1` to `2.14-4`, `2.15-1`, `2.44-1`, `2.44-2` and `2.44-3`;
    - Appendix: `A.3.1-1`, `A.4.5-1`, `A.4.5-2`, `A.5.5-*`, `A.6.3-*`, `A.6.4-*`, `A.6.7-1`, `A.7.9-1`, `A.7.9-2`,
      `A.7.9-3` and `A.7.14-5`.
  - `user-detail--pending-invitations` needs the scratch database in `states.md`.
  - Compare the captures with the chosen artboards.
- **`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj` is green,** without `--no-build`. That run includes:
  - `Design/*`, including `RowActionMarkupTests` for the per-row names;
  - `Accessibility/*`, including `ActionFocusTests` for the lock and reset results;
  - T322's planned `Design/ContrastTests`;
  - T294's scenario guard.
- **A browser check passes at 1280 and 390 px** as Prof Mbatha. It includes a contrast pass as in Step A.7.14, with
  every pair at AA.

## Appendix: the runbook steps (paste after the ask)

Verbatim from `execution/knowledge/scenario-paediatrics/`: Role, Route, Do and Expect only. `D` is the replay day.

```
Step 1.23 — KGK's own record
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /not-found
Do: Open KGK's record at `/admin/institutions/{InstitutionId}` by typing its address, since nothing in her nav leads
  there. Read it and change nothing. Then open the Demo Institution's id the same way.
Expect: KGK's page is headed "Edit institution" and shows KGK's name, the short code `KGK`, the contact email, and
  that KGK is active. Save is offered for the name, short code and contact email.
  She is offered nothing that would deactivate KGK, neither a Deactivate nor an Active she can untick, and no link
  (Back or Cancel) to the Institutions list: deactivating an institution and listing them belong to an Administrator
  alone (`DeactivateInstitutionCommand`; DESIGN.md § Table system, T211).
  The Demo Institution's id sends her to Page not found, not "You cannot open this page". The product never confirms
  that a record outside her institution exists (CLAUDE.md § InstitutionalAdmin scope-aware powers).

Step 2.12 — Mbatha reviews KGK's users
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/invitations → /admin/users → /admin/users/{UserId}
Do: Open Invitations. Then read the user list, filter it by "zulu" and clear the filter. Open Manage on her own
  row.
Expect: Active invitations reads "No active invitations": all nine staff invitations have been used. The user list has
  ten rows: Mbatha and the nine staff, each at KGK with the invited role and status Active. No Demo account,
  `devadmin` or Dr Kruger appears: an InstitutionalAdmin lists only her institution's users (T056). The filter leaves
  Zulu alone. Her own page's subtitle reads "Your own account's summary, roles and pending invitations." A note says
  "This is your own account, so you cannot change its roles, lockout or password here. Another administrator can change
  your roles or lockout.", followed by a Change your password link. It offers no Remove, Add role, Reset password or
  Lockout (T278).

Step 2.13 — Mbatha adds Assessor to Dr Zulu, Dr Naidoo and Dr Botha
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: For each of the three, open Manage, choose Assessor under Add role and press Add role.
Expect: Before the change, Roles lists CommitteeMember with Remove. Add role offers InstitutionalAdmin, SpecialityAdmin,
  SubSpecialityAdmin, Coordinator and Assessor, never Administrator, CollegeAdmin, PendingTrainee or Trainee (T303).
  Its help reads "Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit to
  curriculum'.", on every user's page.
  A Lockout card lists what a lock does (T284), and Pending invitations reads "No active invitations are outstanding for
  this email.". After the change the page reads "Role 'Assessor' added.", Roles lists both, and Add role no longer offers
  Assessor. The users list's Roles column shows both.

Step 2.14 — Mbatha creates five assessor profiles
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/assessors → /admin/assessors/edit → /admin/assessors
Do: Create a profile for each assessor at KGK, Paediatrics / Paediatrics, with qualifications "MBChB, MMed (Paed), FC
  Paed (SA)". Zulu is Trained, completed `D−8 years`. Naidoo is Trained, `D−6 years`. Botha is Trained with the date
  left blank. Patel is In training. Khumalo is Provisional (awaiting sign-off), `D−1 month`.
Expect: The list first reads "No assessor profiles". The form's Assessor user offers the five KGK Assessors, and not van
  Rensburg or any Demo account. Institution offers KGK alone. Sub-speciality is enabled once a speciality is chosen.
  Training status offers Not started, In training, Provisional (awaiting sign-off) and Trained, and the completion date
  is asked for only when the status is Provisional or Trained. Each save reads "Assessor profile saved.", and the page
  moves to `?id=` for the saved profile. The next new form's picker leaves out everyone already profiled.

Step 2.15 — The assessor list and who is not on it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/assessors
Do: Read the list.
Expect: Five rows, each at KGK in Paediatrics / Paediatrics. Training status reads Trained for Zulu, Naidoo and Botha, In
  training for Patel and Provisional for Khumalo. Training completed reads `D−8 years`, `D−6 years`, "Not recorded",
  "Not recorded" and `D−1 month` respectively. Each row has an Edit named for its assessor. Van Rensburg is neither listed
  nor offered, because he does not hold the Assessor role.

Step 2.44 — Prof Mbatha's dashboard after onboarding
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/assessors
Do: Open Home. Then open Users and Assessors.
Expect: Home reads "Institutional admin · Semester N, YYYY" and offers "Invite a person". Users reads InstitutionalAdmin
  1, SpecialityAdmin 1, SubSpecialityAdmin 1, Coordinator 1, CommitteeMember 4, Assessor 5 and Trainee 5, with no
  PendingTrainee line.
  Specialities & sub-specialities reads 1 and 1, what KGK has adopted. Quick links reads Users, Invitations, Curriculum
  adoptions and Entrustment decisions. The users and assessors lists both name Fatima Khumalo (Step 2.41).

Step A.1.14 — What the erasure left, as KGK sees it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/trainees → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Look for Dr Ndlovu among KGK's users, its trainees and its committee reviews. Open the check-in from A.1.10.
Expect: No Dr Ndlovu remains among KGK's users or trainees (T026, T258).
  - Committee reviews lists his reviews under a pseudonym, `deleted_user_…`, that names nobody: Act 4's ratified
    review and closed check-in as they were, and A.1.10's check-in as Withdrawn.
  - The check-in reads Withdrawn, dated today, with the reason "Withdrawn because the trainee's personal data was erased
    at their request. Nothing more is decided at this review.", and offers no action.
  - His activities and progress stay under the same pseudonym; the account itself is checked in the outcome's SQL.

Step A.3.1 — Prof Mbatha opens SSO Mappings with no provider configured
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/sso/group-mappings
Do: Open SSO mappings from the menu.
Expect: A card says that no SSO providers are configured, and to add them to the `Sso:Providers` section of the
  application settings and restart. No form for adding a mapping is offered. Current mappings is empty ("No group
  mappings"), although its empty state still says to add a mapping above. Nothing on the page can be changed.

Step A.4.5 — Prof Mbatha resets his password
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: Open Dr du Plessis's account. Set a new password of 10 characters, then one that meets every rule. Record the
  second in `pwd_DO_NOT_COMMIT.txt`, as the password to give him out of band.
Expect: The Reset password card says it sets a password directly and that the user is not emailed. Its field has the
  Show toggle ("Show new password") and the six rules under it, and Reset password is enabled once anything is typed.
  The 10-character password is refused with "The password was not reset. The new password needs:" and the rules it
  breaks, in the six rules' order. The second is accepted, and the field is cleared. The audit log records the reset
  with the password redacted (T101).

Step A.5.5 — Another institution's records, by id
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /not-found → /admin/users/{UserId}
Do: Open the Demo Institution's page by its id. Then open the page of the dev trainee `trainee@wombat.local` by that
  account's id.
Expect: The institution shows Page not found, not "You cannot open this page". The user shows "User unavailable" ("The
  user could not be found or is outside your scope."). Neither page confirms that the record exists (CLAUDE.md: 404, not
  403).

Step A.6.3 — Prof Mbatha edits her own institution
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /
Do: Open KGK's page by its address (no nav link leads there). Set its contact email to
  `hod.paediatrics@kgk.wombat.local` and save. Read Status, then look at Back to home and Cancel.
Expect: "Institution saved." takes the focus. Status reads "Active" as text, with "Set by a global administrator."
  beneath: there is no Active box and no Deactivate, because an institution's state is the Administrator's alone
  (T302). "Back to home" and Cancel both lead to `/`. SQL:
  `SELECT "Id","ContactEmail","IsActive" FROM "Institutions" WHERE "ShortCode"='KGK'` gives
  `2|hod.paediatrics@kgk.wombat.local|t`.

Step A.6.4 — Prof Mbatha locks Dr Patel out
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/users/{UserId}
Do: With Dr Patel signed in on another browser, open his account, read the Lockout card, and lock him out.
Expect: Once locked, his status reads "Locked out" and the card offers Reactivate user. The focus moves to the result.
  No confirmation is asked first (T264). Before the lock, the card lists what it does until he is reactivated, and says
  his records stay as they are (T284):
  - he sits on no committee panel;
  - he is not a current trainee;
  - he is sent no reminders or digests;
  - he cannot be named as an activity's assessor.

Step A.6.7 — Prof Mbatha reactivates Dr Patel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users/{UserId}
Do: Reactivate Dr Patel.
Expect: His status reads Active, the card offers Lock out user again, and the focus moves to the result.

Step A.7.9 — Prof Mbatha on her phone
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/users → /admin/users/{UserId} → /admin/trainees → /admin/trainees/edit → /admin/activity-types/{ActivityTypeId:int}
Do: At 390 px, open Users and Dr Dlamini's account, Trainees and Dr Dlamini's profile, then the KGK Teaching Session
  Log in the builder.
Expect: The user page's cards stack, and its buttons wrap rather than overflow. The trainee profile form stacks its
  fields. The builder's editor and live preview stack in one column (DESIGN.md § Builder layout, T266), and its tab bar
  stays usable.

Step A.7.14 — Prof Mbatha's pages, checked for contrast
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/profile/submit → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient, and its current item (white on the .32 fill);
  - muted text on the page background;
  - the status badges on a committee review opened from Committee reviews (Dr Molefe's review 7; the list itself shows
    each state as plain text);
  - a success alert (save My account's name, from the name in the top bar, unchanged: "Name saved.") and a danger
    alert (Change password with a confirmation that differs, which checks no password and changes nothing);
  - a password field's Show toggle, pressed (white on the action blue, 4.86:1);
  - white on the primary, danger (Lock out user on Dr Patel's page, not pressed) and success (Publish in the builder,
    not pressed; with no draft it is disabled, and a disabled control is exempt) buttons;
  - the focus ring on white and on the page background;
  - an input's border.
Expect: Every pair meets WCAG 2.1 AA: text 4.5:1, large text 3:1, and 3:1 for a control's boundary and the focus
  ring. Muted text passes on the page background since T086.
```
