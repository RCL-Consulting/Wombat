# Flow 12 round 3: the text of every board

Generated from `round-3/project/R3-*.dc.html` (T376, build-contracts § 0): style, script, svg and helmet blocks stripped; headings, labels, aria-labels, placeholders, input values, captions and annotations kept. Board order is the canvas's. Design data: a caption or chip is never a shipped word.

######## BOARD R3-0-spec-2.dc.html — R3-0-spec-2 · round 3 · commands, rules, DESIGN.md, classes
## Flow 12 · round 3 · Spec, continued
### The commands, as written
// M1 — training only; institution from the person; no scope written public sealed record CreateOrUpdateAssessorProfileCommand( string UserId, AssessorTrainingStatus TrainingStatus, DateOnly? TrainingCompletedOn, string Qualifications, ClaimsPrincipal Principal) : IRequest; // handler: DemandUserAdministration, DemandNotCaller, DemandInScope(UserId); the person must hold Assessor // InstitutionId := person's InstitutionId; no UpdateScopeAsync; no security-stamp change // test: saving training for a Committee member who is also an Assessor leaves UserSpecialityScopes and the security stamp unchanged // M2 — Add role: the role, then the union of scopes, in one transaction public sealed record AddRoleToUserCommand( string UserId, string Role, int? SpecialityId, int? SubSpecialityId, ClaimsPrincipal Principal) : IRequest; // handler: InOneTransactionAsync { AddToRoleAsync; UpdateScopeAsync(held ∪ {SpecialityId, SubSpecialityId}) } // the role's need (F1): Assessor, Sub-speciality admin → a speciality and a sub-speciality in the RESULTING scope; // Speciality admin → a speciality; "may not be scoped to a sub-speciality" is an invitation rule, never applied to a held scope // the narrowing (F2): Any speciality + Speciality admin → the scope becomes that speciality for every role held, said before and after // a person with no institution (a College admin, U14): Add role offers no institution-scoped role; nothing is written // tests: the union; a failure after AddToRole rolls back both; Assessor first then Committee member (F1) passes, // Committee member first then Assessor passes, and the scope is the same // F7 public sealed record RemoveUserInstitutionalSignInCommand(string UserId, string Provider, ClaimsPrincipal Principal) : IRequest; // DemandUserAdministration, DemandNotCaller, DemandInScope; the last-way-in guard reused: // refused "The institutional sign-in is the account's only way in." when !HasPassword && logins.Count == 1 // F6 — read when the lock or Remove dialog opens, never on a page view public sealed record GetRequestsWaitingForUserQuery(string UserId, ClaimsPrincipal Principal) : IRequest<WaitingRequestsDto>; // WaitingRequestsDto(int Count, DateTimeOffset? OldestWaitingSince, IReadOnlyList<WaitingRow> FirstFive) // WaitingRow(TraineeName, ActivityTypeName, Reference /* PAED-015 */, WaitingSince, IsOverdue) // the person scope-checked; AwaitsReviewer and the nominee extraction in an Application service the nudge job shares
### Build rules
- F4 migration: drop AssessorProfiles.SpecialityId and SubSpecialityId; keep InstitutionId, written from the person at save.
- S5: ListSsoGroupMappings returns ProviderGone (the provider no longer in IOptions<SsoOptions>); DeleteSsoGroupMapping refuses a gone-provider mapping to anyone but the Administrator (a handler check and a test); roles already given remain.
- S7: one order everywhere, ORDER BY LastName.ToLower(), FirstName.ToLower(), Id, in the query, pinned by a test with “van Rensburg” and “du Plessis”.
- S6: ListUsers takes q (each word matched against first name, last name or email, so “thandi zulu”, “zulu thandi” and the address all find her), role (with No role), status, page and size, all in the query string; Home’s role links open a filtered Users; the user page’s Users crumb returns to it through an allow-listed return (as flow 11’s sign-out). Administrator and College admin in Role for the Administrator only. One paged SQL query, no N+1.
- S10: /admin/assessors/edit?id= is the PROFILE id: profile → UserId → /admin/users/{UserId}?section=training. Unknown, out-of-scope or missing → User unavailable. An unknown section opens nothing. For a person without Assessor, the page says once: “Thandi Zulu does not hold the Assessor role, so has no assessor training here.” (NEW).
- S12: the InstitutionCreate policy split off (Administrator only), so /admin/institutions/new is access denied for her; the Administrator’s page is flow 18’s.
- S13: Add role’s choices come from GetAdoptedDisciplinesForInstitutionQuery for the PERSON’s institution (P04: KGK; P27: the Demo Institution).
- S1, the scope cell: Institutional admin → the institution’s name; College admin → the College’s; otherwise the speciality names comma-joined, then “ / ” and the sub-speciality names; none → “Any speciality”. Two speciality ids are drawn (P34); Add role does not refuse a second speciality. PROPOSED.
- F9: PagerControls unchanged; this flow renders it only when total exceeds one page.
### DESIGN.md text
- § Form system, the note panel (F11, M9): “A form opened in place on a detail page (Add a role…, Record training) is a .detail-card.note-panel under its trigger, .note-panel--form spacing its own parts. Its trigger has aria-expanded, and aria-controls only while the panel exists; the panel is not a group, its fieldset is. A form in a note panel leaves with Cancel (a quiet button), a note with Keep. Radios with help: .choice-group of .form-check, the help a paragraph after the label, outside it, in aria-describedby (.choice-help-stack).”
- § Form system, read-outs (M8): “A value shown in a field’s place is FormField’s read-out: dl.form-group, its label the dt, its value dd.readonly-value, its help dd.field-help.”
- § Form system, refusals (S22): “A form with one refused field shows the refusal beside its field, focused, with no RefusalSummary: the stated exception. The record page’s ValidationSummary is removed; the first invalid field takes the focus.”
- § Table system (S2, S20, one scope): “One scope per person, in a column (Users) or the who line; exempt: Home’s People card and flow 11’s forms. A person with no role reads “No role” and, for an ended trainee profile only, “A completed or withdrawn registrar keeps an account and its records.” A stacked table carries role=table, rowgroup, row, columnheader, rowheader and cell, as DataTable’s Stack emits.”
- § Page-level patterns, Detail page (S2): “A person with no role: an ended profile says how and when it ended and what is still open (My progress, data rights); no profile says ‘‹name› holds no role. They can sign in to My account and My data rights.’”
- § Button system (S22): DESIGN.md:734 restated: “A destructive action is an outline trigger named for its row; its filled danger button is in the ConfirmDialog only.” This supersedes :3442’s filled Delete.
- § Dashboard page (S22): “A waiting line above the cards, only while something waits; flow 06’s oversight line is the precedent. Not a live region.”
### Each form’s parts, and what holds them
- Add a role: div.note-panel.note-panel--form (fieldset.choice-group.choice-help-stack, .form-grid of read-outs, help, .form-actions).
- Training: div.note-panel.note-panel--form (fieldset.choice-group, .form-group date, .form-group qualifications, .form-actions).
- Password: div.section-part (h3, help, .form-group with .password-field and rules, .form-actions).
- The record: form.form-container (.form-grid, dl.form-group Status, .form-actions).
- SSO: form.form-container (.form-grid ×2–3, .form-actions). Users’ filter: form.detail-card.people-filter (.search-grid, .filter-actions).
### Class table (S24)
Every class these boards add or retire
| Class
| Status
| Why, or what it reuses
| .user-page, .user-section, .section-part
| NEW
| the detail page as a column of sections and h3 parts
| .role-rows, .role-row, .role-row-label, .role-row-note
| NEW
| a role, then after a gap its Remove… or why none (T323)
| .no-role
| NEW
| the no-role sentence block
| .note-panel--form
| NEW modifier
| the system’s .note-panel, its gap the form’s (F11)
| .choice-help-stack, .choice-help
| NEW
| radios with help under each (M9); .choice-stack stays the MSF Kind radios’
| dl.form-group, dd
| NEW rule on .form-group
| the read-out (M8)
| .role-cell
| NEW
| one role a line in Users
| .no-role-cell
| NEW
| No role and its sentence in a cell
| .status-locked
| NEW
| the gap between the lock icon and “Locked out” in the badge
| .people-page
| NEW
| the list page’s column
| form.detail-card.people-filter
| NEW, listed once
| the filter card, role="search", “Filter ‹list›” (flows 06 and 12)
| .record-page
| NEW
| the record’s 52rem column
| .not-set-up
| NEW
| her SSO card
| .dashboard-grid .detail-list, dd, dd p
| NEW rules
| M7: the card’s list wraps
| a.dashboard-metric
| NEW rule on .dashboard-metric
| a count and its label as one link (S19)
| .dashboard-card-footer .field-help
| NEW rule
| the footer’s note on the left (S18)
| .user-section .actions-cell
| NEW rule
| the trigger at the start (S18)
| .person-address
| gone
| reuses .person-cell span
| .list-head
| gone
| reuses .list-section-title (flex)
| .card-link, .count-rows
| gone
| DashboardCard’s footer link and .dashboard-metric-row (S19)
| .action-reason
| gone
| reuses .field-help
| .inst-status
| gone
| dd > p in .detail-list
| .who-line, .who-line > div
| gone
| the who line is a dl.detail-list (S19)
| .choice-stack (ours)
| gone
| M9
| .disclosure-panel, .readback-pair, .status-readout, .lock-list, .waiting-request
| gone
| note panel; form-grid of read-outs; read-out; plain ul; Alert (S19)
| .captures, .old, the duplicate phone .form-control rule
| gone (canvas-only and redundant)
| S24
| .anno, .anno-line, .canvas-focus, .stage, .stage-label, .doc
| canvas only
| never app.css

######## BOARD R3-0-spec-3.dc.html — R3-0-spec-3 · round 3 · messages and data
## Flow 12 · round 3 · Spec, messages and data
### Every message, by kind
Built words kept; NEW beside what it replaces
| Kind
| Words
| Built or NEW
| Replaces
| Result
| Assessor added to Thandi Zulu. Thandi Zulu’s scope is now Paediatrics / Paediatrics. Any open session ends within a minute.
| NEW
| Role 'Assessor' added.
| Result
| Committee member added to Mohammed Patel. Mohammed Patel’s scope stays Paediatrics / Paediatrics. …
| NEW (F1)
| —
| Result
| Speciality admin added to Pieter Smit. Pieter Smit’s scope becomes Paediatrics, for every role he holds. …
| NEW (F2)
| —
| Result
| Assessor removed from Thandi Zulu. Any open session ends within a minute.
| NEW
| Role 'Assessor' removed.
| Result
| Password reset for Pieter du Plessis. Share the new password with Pieter du Plessis out-of-band.
| NEW
| Password reset. Share the new password with the user out-of-band.
| Result
| Mohammed Patel is locked out. Any open session ends within a minute. · Mohammed Patel is reactivated and can sign in again.
| NEW
| User locked out. · User reactivated.
| Result
| The ‹provider› sign-in was removed from Thandi Zulu. Thandi Zulu signs in with an email and a password.
| NEW (F7)
| —
| Result
| Training saved for Fatma Khumalo: Provisional (awaiting sign-off), completed 2026-09-20. · Training saved for Sarah Botha: Trained, no date.
| NEW
| Assessor profile saved.
| Result
| Kgosi Kgari Teaching Hospital saved. Its contact email is now hod.paediatrics@kgk.wombat.local. · … It has no contact email now.
| NEW
| Institution saved.
| Result
| Mapping added. People in ‹group› get Assessor in Paediatrics / Paediatrics at their next sign-in. · Mapping deleted. From each person’s next sign-in, ‹group› no longer gives Assessor.
| NEW
| Mapping added for group '…'. · Mapping deleted.
| Refusal
| The password was not reset. The new password needs: … · The new password does not meet the rules below.
| built
| —
| Refusal
| Speciality administrators must be scoped to a speciality. · The selected role requires speciality and sub-speciality scope.
| built
| —
| Refusal
| The institutional sign-in is the account’s only way in.
| built (flow 02)
| —
| Refusal
| Choose the training status. · Type the qualifications, as they should read.
| NEW
| The UserId field is required. · The Qualifications field is required.
| Refusal
| Type a short code. The record was not saved.
| NEW
| The Short code field is required.
| Refusal
| An institution with the same name or short code already exists.
| built
| —
| Refusal
| Enter a valid email address, for example name@example.org.
| PROPOSED
| no check today
| Refusal
| Only a Wombat administrator can map a group to Institutional admin.
| PROPOSED (Q19)
| —
| Hint
| Trainee is not offered: … · Sets a new password directly. … · Faculty-development lifecycle. … · Date the assessor completed …
| built
| —
| Hint
| Works across the institution, kept to ‹name›’s scope: ‹scope›. · Sits committee reviews, in ‹name›’s scope: ‹scope›. · Runs one sub-speciality’s committee work.
| NEW (nit)
| the unscoped RoleHelp
| Hint
| Pieter Smit’s scope becomes Paediatrics, for every role he holds.
| NEW (F2)
| —
| Hint
| A name in either order, part of one, or an address.
| NEW (S6)
| Part of a name or an address is enough.
| Hint
| Set by a Wombat administrator.
| NEW
| Set by a global administrator.
| Notice
| ‹name› signs in only through ‹provider›. A password cannot be set for them.
| NEW (F3)
| the reset field
| Notice
| N requests are waiting for ‹name›, the oldest since ‹…›. They stay with them; they do not move to anyone else, and no reminder is sent about them. (and “and N more.”)
| NEW (F6, S3)
| —
| Notice
| Remove dialogs, one sentence each (P36); ‹name› will hold no role. They can sign in to My account and My data rights.
| NEW (S3, S2)
| —
| Notice
| From each person’s next sign-in, belonging to ‹group› no longer gives Assessor. · The scope it gave stays.
| NEW (F8)
| … anyone who had Assessor only from this group loses it then.
| Notice
| Pieter du Plessis holds no role: they left the programme on 2026-10-18. … · Lerato Molefe … completed the programme on 2026-10-20. … · John van Rensburg holds no role. They can sign in to My account and My data rights.
| PROPOSED (Q13) · NEW (S2)
| This user has no roles.
| Notice
| Institutional sign-in is not set up for Kgosi Kgari Teaching Hospital. …
| NEW (T326)
| No SSO providers are configured. … (kept for the Administrator)
| Notice
| Could not load the users / this person / the assessors / the institution / the SSO mappings. Nothing has changed. …
| NEW
| the exception’s text
| Notice
| Thandi Zulu does not hold the Assessor role, so has no assessor training here.
| NEW (S10)
| —
| Empty
| No one matches “zzz”. … · Nobody holds the Assessor role yet. … · No group gives a role yet. Use Add a mapping to give one.
| NEW
| No users match the current filter. · No assessor profiles … · Add a mapping above …
| Label
| Lock out… · Save changes · Save training · Cancel (quiet, in a note panel) · Keep access · Keep the role · Keep the sign-in · Keep the mapping · Delete the ‹group› mapping · Remove sign-in · Reactivate user (kept)
| NEW except the last
| Lock out user · Save · Save assessor profile
### DATA the build must add
- ListUsers as one paged SQL query: the filter as S6, the order as S7, no N+1; the scope names, CollegeId and CollegeName, and the cell rule (S1); the latest ended trainee profile for no-role rows (S2).
- GetUserById: the scope names; the latest ended trainee profile (S2); the institutional sign-ins, AllowLocalPassword and HasPassword (F3); the training record joined; the invitations read removed (Q9).
- GetRequestsWaitingForUserQuery, on the dialog’s open (F6).
- GetAdoptedDisciplinesForInstitutionQuery for the person’s institution (S13).
- AddRoleToUserCommand with the union in one transaction (M2); the training-only command and F4’s migration (M1); RemoveUserInstitutionalSignInCommand (F7).
- Assessors: every Assessor holder joined to a record, with scope names; the redirect (S10).
- ListSsoGroupMappings’s ProviderGone; Create refusing Institutional admin to a non-Administrator; Delete of a gone-provider mapping the Administrator’s alone (S4, S5).
- Home: the distinct account count, the Pending trainee count, the adopted specialities and sub-specialities, the institution’s name and record; GetMyInstitutionNameQuery (S9).
- Her menu’s Entrustment decisions: an owner row, checked by ActiveNavItemTests (F10).
- Pending invitations: nothing, removed.

######## BOARD R3-0-spec.dc.html — R3-0-spec · round 3 · Spec
## Flow 12 · round 3 · Spec
### The screenshots
Not attached to this message. I read all 17 from the copies made from C:\dev\Wombat\design\upload on renier-pc in round 2 of this session. They were not copied again this round.
Read in round 2 (10, the key captures): A.6.4-2-patel-locked, crop-first A.4.5-1-reset-refused, A.7.9-3-user-390, 2.12-2-users-list, A.7.9-2-users-390, 2.44-1-mbatha-home, 2.14-2-empty-save, 2.15-1-assessors-five, A.3.1-1-sso-no-provider, A.6.3-1-contact-saved.
Opened this round (7, round 2’s unopened, S16):
- 2.13-1-zulu-before: P02 holds. Today: “CommitteeMember” with a red Remove, and the lockout list showing the trainee and assessor lines for a Committee member (the nit’s case).
- 1.23-1-kgk-own-record: I01 holds. Today: “Edit institution”, paeds-admin@kgk.wombat.local, Back to home, Save.
- 5.17-3-mbatha-user-no-roles: P12 holds. Today: “This user has no roles.”, the Pending invitations card, the trainee line in Lockout for a person with no role.
- A.7.9-1-home-390: H05 holds. Confirms Trainee 2 in Act A. Today’s catalogue card there reads 3 specialities and 4 sub-specialities (T291 item 4); KGK’s adopted 1 and 1 is drawn.
- 2.12-3-users-filter-zulu: U02 holds. Today it filters per keystroke, with a clear button inside the field, and still shows the Institution column.
- states/users-list--loading: U11 holds. A 2026-09-26 frame, body read only: four skeleton bars under the header, no action.
- A.6.7-1-patel-reactivated: P17 holds. Today: “User reactivated.” at the page top; the lockout list still carries the trainee line for Patel (the nit).
All 17 were read; nothing is drawn from memory in place of any.
### Each item, and where it is drawn
F1–F11, M1–M10, S1–S24
| Item
| What changed
| Where
| F1
| Add role judges the role’s need against the resulting scope; Patel given Committee member, no refusal, scope unchanged.
| P28 (1280, 390); the handler test on Spec-2
| F2
| The narrowing said before the save and in the result.
| P29 form, P30 result (1280, 390); P06’s scope sentence
| F3
| An SSO-only account: no password part, the built refusal on Remove, no hint.
| P22 (1280, 390)
| F4
| SpecialityId and SubSpecialityId dropped from the profile.
| Spec only (Spec-2, build rules)
| F5
| Prerequisites.
| The block below; pills on O01–O10 and T03–T07
| F6
| GetRequestsWaitingForUserQuery on open; count, up to five rows, the oldest.
| P15 (N = 1), P31 (N = 2), P32 (N > 5); the query on Spec-2
| F7
| Remove another person’s institutional sign-in.
| P21 (dialog), P33 (result, 1280 and 390); the command on Spec-2
| F8
| The Delete dialog’s words.
| O07; O08’s result; messages on Spec-3
| F9
| No pager on ten or fifteen; U10 without “Page 1 of ‹10›”.
| U01, U04–U08 (none), U10, T11. Read note: F9 says “total > 10” yet no pager on fifteen; I drew the pager only above one page of 20. Say if the guard is total > 10.
| F10
| Entrustment decisions under Reviews in her menu.
| Every round 3 board of hers
| F11
| The note panel in place of the disclosure panel; Cancel quiet.
| P03, P04, P08, P27, P29, T03, T05, T06; DESIGN.md text on Spec-2
| M1
| Training split from scope.
| T03–T07 (no institution or speciality asked); command and test on Spec-2
| M2
| AddRoleToUserCommand with the union in one transaction.
| Spec-2
| M3
| P20–P22 as F3.
| P20 and P21 (Zulu, a password too), P22 (SSO-only)
| M4
| Prerequisite pills.
| O01–O10 “after T288 and T333”; T03–T07 “after T289”
| M5
| The Steps board’s 2.15 row by row.
| Steps
| M6
| The 390 password field.
| Every P and T board at 390; widths below
| M7
| The institution card wraps.
| H01–H05, H01b (64 characters, Illustration); table below
| M8
| Read-outs as dl.form-group, dd.readonly-value.
| I01–I04, I07, I08 Status; O04–O06 Provider and Institution, O04b scope; training read-out; P04, P27, P29 scope
| M9
| The note panel; .choice-group with .form-check; .choice-help-stack.
| P03, P04, P05 (closed), P08, P27, P28, P29, T03, T05, T06
| M10
| aria-controls only while the panel exists; no role="group"; help outside the label.
| The same boards; captions say it
| S1
| The scope cell’s rule.
| P34 (two ids, Illustration); rule on Spec-2
| S2
| Three no-role states.
| P09, P12 (ended profile); P35 (no profile, Illustration); DESIGN.md text on Spec-2
| S3
| A Remove dialog per role.
| P36 sheet at 1280 and 390; P06 redrawn
| S4
| The SSO form’s relabels.
| O04, O04b (new), O06
| S5
| Gone-provider mappings: Delete the Administrator’s alone.
| O03; handler check on Spec-2
| S6
| Either order, the address; Administrator and College admin for him; the query string.
| U02b “thandi zulu” (1280, 390); U14; Spec-2
| S7
| One order on every server.
| Spec-2 only
| S8
| Trainee notes without a subtitle.
| U13, P26
| S9
| Institution h1 while loading; subtitles without the name.
| I05, I06; U11, U12, T12, T13 (without the name until it loads)
| S10
| The redirect.
| Spec-2
| S11
| T10 without the note.
| T10
| S12
| InstitutionCreate split.
| Spec-2
| S13
| The person’s institution’s adopted set.
| P04, P27; Spec-2
| S14
| U14 and T14 Illustration, bracketed rows, the step named.
| U14 at 2.12 (18 rows), T14 at 2.15 (6)
| S15
| The Steps board.
| Steps
| S16
| The seven captures.
| First section above
| S17
| The radio’s label 44 px at 390.
| P03, P04, T06 (every radio, measured)
| S18
| No inline style.
| Every round 3 board: none (searched)
| S19
| System pieces.
| Waiting request as Alert (P06, P15, P31, P32); who line dl.detail-list (every P board); People counts as .dashboard-metric-row links, footer links (H01–H05)
| S20
| Stacked tables’ roles.
| Every stacked table here: role="table", rowgroup, row, columnheader, rowheader for the row’s name, cell
| S21
| The filter card, named “Filter people”; a hidden live count.
| Every U board
| S22
| DESIGN.md text.
| Spec-2
| S23
| Standing notices not live.
| H04, O03, P01, P26 (and U13)
| S24
| The class table.
| Spec-2
#### Nits, as passed
- RoleHelp variants for a scoped person: Coordinator “Works across the institution, kept to Thandi Zulu’s scope: Paediatrics.”; Committee member “Sits committee reviews, in Mohammed Patel’s scope: Paediatrics / Paediatrics.”; Sub-speciality admin “Runs one sub-speciality’s committee work.” (P03, P04, P28’s form). NEW.
- The lockout list by role. P15 and P14 (Patel, Assessor): 2 lines (reminders, assessor). P18 (Dlamini, Trainee): 2 (trainee, reminders). Zulu after 2.13: 3 (panel, reminders, assessor). Zulu before 2.13: 2. A person with no role: 1.
- Home’s counts and the adopted counts are DATA reads; the locked-account read is dropped (no locked line on Home).
- “Locked out” on these pages is the administrative lock (UserDeactivation.IsDeactivated) only; Identity’s self-lifting lockout after failed sign-ins is not shown as Locked out. PROPOSED.
- ListAllUsersAsync’s N+1 GetRolesAsync goes with S6’s paged query.
- Not started stays a status you can record. On the list, “Not recorded” in Training status means no record; Not started is a record. Botha’s no-date save: “Training saved for Sarah Botha: Trained, no date.”
- Mbatha’s own institutional sign-in: flow 02’s My account. I07 contact email cleared (board). I08 duplicate name or short code (board).
- Q9 deletes: the card, RevokePendingInvitationsForEmailCommand, UserDetailDto.PendingInvitations, UserPendingInvitationDto, the read in GetUserByIdQuery, the CSS state, and their tests.
- T11’s and U10’s bracketed rows: “order not shown” in the caption.
- I04’s address: ‹a malformed address›.
- The I and T notes: one note each on the Round 3 page. The Demo Institution’s id is 1.
- At 390 the canvas-only .anno chip wraps (white-space:normal).
- The breadcrumb’s Home (39 × 24) is the accepted inline exception, as in flows 03–11.
- O07’s danger button: “Delete the ‹group display name› mapping”.
- The canvas’s ds/ (tokens.css, bundle.css) saved with the round.
### Prerequisites (F5, M4)
- Land first, each its own task: T288 and T333 (the whole O family is drawn after them); T289 (the training section’s refusals, T03–T07: the training-only command M1 needs no T289, its refusals do).
- Built in this flow, drawn as fixed: T264’s users and SSO half; T326’s assessors and SSO items; T286’s external-logins half; Q16’s other halves: T291 items 4 and 8, T323, T324, T280, T325, T329, every DATA read, and Q4’s scope on Add role.
- At HEAD today all six tasks are queued.
### Illustrations
H01b · U03, U10, U12, U13, U14 · P06, P07, P08, P19, P20, P21, P22, P25, P26, P27, P28, P29, P30, P31, P32, P33, P34, P35, P36 · T01, T10, T11, T13, T14 · I03, I04, I06, I07, I08 · O03–O08, O04b, O10. Each carries the chip on its face and in its title.
### M6: the password field at 390
Measured on a static render of each board’s markup with the canvas’s tokens.css, bundle.css, f12r3.css and fonts (Chromium, Playwright), not in the canvas itself
| Board
| Input (w × h)
| Show toggle (w × h)
| P03
| 214 × 44
| 86 × 44
| P14
| 214 × 44
| 86 × 44
| P20
| 214 × 44
| 86 × 44
The rule: at 390 only .user-page .form-actions .btn and .user-page .section-part > .btn run full width; the toggle keeps the system’s 44 px height and its own width.
### M7 and the tables: px scrolled sideways
307 renders measured; the most any page scrolled sideways: 0 px. At 390, 88 boards checked for controls under 44 px (buttons, links styled as buttons, fields, selects, radio labels, dialog buttons; the breadcrumb’s Home excepted): 0 found.
Sideways scroll, in px; — where the board is not drawn at that width
| Board
| 1280
| 1100
| 1000
| 901
| 900
| 768
| 390
| H01-244-onboarded
| 0
| —
| 0
| H01b-long-email
| 0
| —
| 0
| H02-110-first
| 0
| —
| 0
| H03-121-adopted
| 0
| —
| 0
| H04-228-waiting
| 0
| —
| 0
| H05-a63-cancel
| 0
| —
| 0
| O03-left-mappings
| 0
| O05-added
| 0
| O06-refused-ia
| 0
| O08-deleted
| 0
| T02-214-five-unrecorded
| 0
| T08-215-five
| 0
| T10-zulu-removed
| 0
| T11-thirty
| 0
| T12-loading
| 0
| T13-load-error
| 0
| T14-admin-view
| 0
| U01-212-ten
| 0
| U02-212-zulu
| 0
| U02b-212-thandi-zulu
| 0
| U03-212-no-match
| 0
| U04-213-two-roles
| 0
| U05-228-fifteen-pending
| 0
| U06-244-fifteen
| 0
| U07-a114-fourteen
| 0
| U08-a64-patel-locked
| 0
| U09-no-role-filter
| 0
| U10-heavy
| 0
| U11-loading
| 0
| U12-load-error
| 0
| U13-trainee-note
| 0
| U14-admin-view
| 0

######## BOARD R3-0-steps.dc.html — R3-0-steps · round 3 · Steps
## Flow 12 · round 3 · Steps
Each Expect clause today’s runbook carries stays, with the new one beside it. Presses + fields typed.
The 15 steps
| Step
| Boards
| Today
| Expect once built
| 1.23
| I01
| Today: “Edit institution”, typed address, Save, Back to home, paeds-admin@kgk.wombat.local.
| Kept: KGK’s name, KGK, its contact email, Active; no Deactivate. New: titled “Kgosi Kgari Teaching Hospital”, reached from Home’s card; “Your institution's name, short code and contact email.”; Status a read-out over “Set by a Wombat administrator.”; Save changes and Cancel. /admin/institutions/new: access denied. Demo id (1): Page not found, unchanged.
| 2.12
| U01, U02, U02b, P01
| Today: Institution column, “CommitteeMember”, a per-keystroke Filter; own page’s pending invitations.
| Kept: ten rows, Zulu alone after the filter, the own-account note and Change your password. New: no Institution column; roles by label; each person’s one scope as invited (U01); “zulu” then Show: “People 1 of 10 match “zulu””, the focus on the heading; her subtitle “Your own account: your roles and how you sign in.”; no Pending invitations; Entrustment decisions in her menu.
| 2.13
| P02–P05, U04
| Today: a select, “Role 'Assessor' added.”, red Remove.
| Kept: the Trainee help; Assessor offered, never Administrator, College admin, Pending trainee or Trainee. New: Add a role… opens a note panel with described radios; Sub-speciality as text; “Assessor added to Thandi Zulu. Thandi Zulu’s scope is now Paediatrics / Paediatrics. …”; the scope arrives at 2.13. Users: both roles, Paediatrics / Paediatrics.
| 2.14
| T02–T07
| Today: stays on /admin/assessors/edit (the ?id= move was never built); “The UserId field is required.” and “The Qualifications field is required.”
| New: Assessors lists the five holders; Record training opens /admin/users/{id}?section=training; no institution or speciality asked; “Choose the training status.” and “Type the qualifications, as they should read.”; “Training saved for Fatma Khumalo: Provisional (awaiting sign-off), completed 2026-09-20.” The Note retired.
| 2.15
| T08
| Today: Institution, Speciality, Sub-speciality columns; “Provisional”.
| Botha Trained, Not recorded; Khumalo Provisional (awaiting sign-off), 2026-09-20; Naidoo Trained, 2020-10-20; Patel In training, Not recorded; Zulu Trained, 2018-10-20. Scope Paediatrics / Paediatrics; “Edit training for …”; van Rensburg not listed.
| 2.44
| H01, U06, T09
| Today: keys in a Users card; 2 and 2; Quick links.
| Kept: Invite a person. New: People by label with counts as links (Institutional admin 1, Speciality admin 1, Sub-speciality admin 1, Coordinator 1, Committee member 4, Assessor 5, Trainee 5); Curriculum 1 and 1 adopted; Your institution with Status; no waiting line, no Quick links; Fatima Khumalo.
| A.1.14
| U07
| Today: Users without Ndlovu.
| Kept: fourteen, no Sipho Ndlovu. New: the No role cell and its sentence.
| A.3.1
| O01
| Today: “No SSO providers are configured. …”, “Add a mapping above …”.
| New: “Institutional sign-in is not set up for Kgosi Kgari Teaching Hospital. …”; no form, no list.
| A.4.5
| P09–P11
| Today: page-top refusal; “Password reset. Share …”; “This user has no roles.”
| Kept: the six rules, Show, the refusal’s words, the field cleared. New: the refusal and result in Password; “Password reset for Pieter du Plessis. …”; the no-role sentence first appears here.
| A.5.5
| P23
| Today: Page not found; User unavailable.
| Unchanged, with Go to Users; Back to users gone.
| A.6.3
| I02, H05
| Today: “Institution saved.”; the field starts at paediatrics@kgk.wombat.local (A.6.1’s save).
| New: “Kgosi Kgari Teaching Hospital saved. Its contact email is now hod.paediatrics@kgk.wombat.local.” focused; Cancel to Home, its card reading the new address. (I01’s paeds-admin@ is 1.23’s.)
| A.6.4
| P14–P16, U08
| Today: four bullets, the trainee line among them, and no dialog (T264).
| Both go: the list reads two lines for Patel (reminders, assessor); Lock out… opens “Lock out Mohammed Patel?” with them and “1 request is waiting for Mohammed Patel, since 2026-10-12 12:35 SAST. It stays with them; it does not move to anyone else …”; “Mohammed Patel is locked out. …” focused.
| A.6.7
| P17
| Today: “User reactivated.”
| One press: “Mohammed Patel is reactivated and can sign in again.”
| A.7.9
| U01–U09 at 390, P18, H01–H05 at 390
| Today: Users scrolls 807 px sideways.
| Users stacked, 0 px sideways; the user page’s sections stack, the password toggle 86 × 44; Dlamini’s lockout two lines with the trainee line.
| A.7.14
| P15
| Today: white on danger on the page’s Lock out user.
| The danger pair is the dialog’s “Lock out Mohammed Patel” (5.95:1). Otherwise unchanged.
### Crossing steps whose words change
- 1.10: Home’s People and 0 and 0.
- 1.21: 1 and 1.
- 2.28: “Pending trainee” by label; Home’s waiting line (its Do gains “open Home”); Molefe’s page.
- 5.17: Molefe “completed the programme on 2026-10-20”.
- 5.10: ‹the step’s words that quote a role key or the user page› read by label.
- 2.37 and 3.52: ‹the step’s words› read by label where they quote a role key.
- A.5.15: devadmin’s “Lock out…” and its dialog in place of Lock out user.
- A.6.1: unchanged; the Administrator’s page is flow 18’s.
- Wherever an Expect quotes her menu: Entrustment decisions under Reviews (F10). The Delete step’s Expect, if one quotes it: F8’s words.
### Journeys, recomputed after S6, F11 and F9
At 1280 and 390
| Journey
| 1280
| 390
| Finding a person, “thandi zulu”, from Home
| 3 + 1 (Users, type, Show, Manage); by role from Home’s People: 2 + 0
| 4 + 1; by role 2 + 0
| Adding a role with its scope
| 3 + 0 (Add a role…, Assessor, Add role); abandoning it: 2 + 0 (Cancel in the note panel)
| 3 + 0
| Recording one assessor’s training
| 4 + 2 from Home (Assessors, Record training, status, Save; date, qualifications); 3 + 2 from the person’s page
| 5 + 2 / 3 + 2
| Resetting a password
| 1 + 1
| Locking out and reactivating
| 3 + 0
| Correcting the contact email
| 2 + 1 (Open Kgosi Kgari Teaching Hospital, the email, Save changes)
| 2 + 1
| Paging Users (F9)
| none at 10 or 15; Next 1 + 0 on the heavy set
| the same

######## BOARD R3-H01-244-onboarded-1280.dc.html — H01 · 2.44 onboarded: the People card’s counts as links, the footer links, Status in the card; Entrustment decisions in the menu (F10) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Institutional admin · Semester 2, 2026
Invite a person
### People
1Institutional admin1Speciality admin1Sub-speciality admin1Coordinator4Committee member5Assessor5Trainee
15 accounts. DATA: distinct account countOpen Users
### Curriculum
1speciality adopted
1sub-speciality adopted
Paediatrics / Paediatrics, from the CPSA’s Paediatric EPA Curriculum (11.1). DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
paeds-admin@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H01-244-onboarded-390.dc.html — H01 · 2.44 onboarded: the People card’s counts as links, the footer links, Status in the card; Entrustment decisions in the menu (F10) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Institutional admin · Semester 2, 2026
Invite a person
### People
1Institutional admin1Speciality admin1Sub-speciality admin1Coordinator4Committee member5Assessor5Trainee
15 accounts. DATA: distinct account countOpen Users
### Curriculum
1speciality adopted
1sub-speciality adopted
Paediatrics / Paediatrics, from the CPSA’s Paediatric EPA Curriculum (11.1). DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
paeds-admin@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H01b-long-email-1280.dc.html — H01b · a 64-character contact address in the card: wraps, nothing sideways (M7) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Illustration
Institutional admin · Semester 2, 2026
Invite a person
### People
1Institutional admin1Speciality admin1Sub-speciality admin1Coordinator4Committee member5Assessor5Trainee
15 accounts. DATA: distinct account countOpen Users
### Curriculum
1speciality adopted
1sub-speciality adopted
Paediatrics / Paediatrics, from the CPSA’s Paediatric EPA Curriculum (11.1). DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
‹a-sixty-four-character-contact-address-incard›@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H01b-long-email-390.dc.html — H01b · a 64-character contact address in the card: wraps, nothing sideways (M7) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Illustration
Institutional admin · Semester 2, 2026
Invite a person
### People
1Institutional admin1Speciality admin1Sub-speciality admin1Coordinator4Committee member5Assessor5Trainee
15 accounts. DATA: distinct account countOpen Users
### Curriculum
1speciality adopted
1sub-speciality adopted
Paediatrics / Paediatrics, from the CPSA’s Paediatric EPA Curriculum (11.1). DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
‹a-sixty-four-character-contact-address-incard›@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H02-110-first-1280.dc.html — H02 · 1.10 her first sign-in: one account, 0 and 0 · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Institutional admin · Semester 2, 2026
Invite a person
### People
1Institutional admin
1 account: yours. DATA: distinct account countOpen Users
### Curriculum
0specialities adopted
0sub-specialities adopted
Nothing is adopted yet. Adopt the College’s curriculum to set up a programme. DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
paeds-admin@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H02-110-first-390.dc.html — H02 · 1.10 her first sign-in: one account, 0 and 0 · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Institutional admin · Semester 2, 2026
Invite a person
### People
1Institutional admin
1 account: yours. DATA: distinct account countOpen Users
### Curriculum
0specialities adopted
0sub-specialities adopted
Nothing is adopted yet. Adopt the College’s curriculum to set up a programme. DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
paeds-admin@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H03-121-adopted-1280.dc.html — H03 · 1.21 adopted before onboarding: 1 and 1 · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Institutional admin · Semester 2, 2026
Invite a person
### People
1Institutional admin
1 account: yours. DATA: distinct account countOpen Users
### Curriculum
1speciality adopted
1sub-speciality adopted
Paediatrics / Paediatrics, from the CPSA’s Paediatric EPA Curriculum (11.1). DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
paeds-admin@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H03-121-adopted-390.dc.html — H03 · 1.21 adopted before onboarding: 1 and 1 · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Institutional admin · Semester 2, 2026
Invite a person
### People
1Institutional admin
1 account: yours. DATA: distinct account countOpen Users
### Curriculum
1speciality adopted
1sub-speciality adopted
Paediatrics / Paediatrics, from the CPSA’s Paediatric EPA Curriculum (11.1). DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
paeds-admin@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H04-228-waiting-1280.dc.html — H04 · 2.28 five waiting: the line, not a live region (S23) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Institutional admin · Semester 2, 2026
Invite a person
5 registrars are waiting to be admitted. Nothing can be filed about them until then.
Open Trainees
### People
1Institutional admin1Speciality admin1Sub-speciality admin1Coordinator4Committee member5Assessor5Pending trainee
15 accounts. DATA: distinct account countOpen Users
### Curriculum
1speciality adopted
1sub-speciality adopted
Paediatrics / Paediatrics, from the CPSA’s Paediatric EPA Curriculum (11.1). DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
paeds-admin@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H04-228-waiting-390.dc.html — H04 · 2.28 five waiting: the line, not a live region (S23) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Institutional admin · Semester 2, 2026
Invite a person
5 registrars are waiting to be admitted. Nothing can be filed about them until then.
Open Trainees
### People
1Institutional admin1Speciality admin1Sub-speciality admin1Coordinator4Committee member5Assessor5Pending trainee
15 accounts. DATA: distinct account countOpen Users
### Curriculum
1speciality adopted
1sub-speciality adopted
Paediatrics / Paediatrics, from the CPSA’s Paediatric EPA Curriculum (11.1). DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
paeds-admin@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H05-a63-cancel-1280.dc.html — H05 · A.6.3 Cancel lands here: the card reads hod.paediatrics@ · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Institutional admin · Semester 2, 2026
Invite a person
### People
1Institutional admin1Speciality admin1Sub-speciality admin1Coordinator4Committee member5Assessor2Trainee
14 accounts. DATA: distinct account countOpen Users
### Curriculum
1speciality adopted
1sub-speciality adopted
Paediatrics / Paediatrics, from the CPSA’s Paediatric EPA Curriculum (11.1). DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
hod.paediatrics@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-H05-a63-cancel-390.dc.html — H05 · A.6.3 Cancel lands here: the card reads hod.paediatrics@ · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Home
Institutional admin · Semester 2, 2026
Invite a person
### People
1Institutional admin1Speciality admin1Sub-speciality admin1Coordinator4Committee member5Assessor2Trainee
14 accounts. DATA: distinct account countOpen Users
### Curriculum
1speciality adopted
1sub-speciality adopted
Paediatrics / Paediatrics, from the CPSA’s Paediatric EPA Curriculum (11.1). DATA: adopted counts
Open Curriculum adoptions
### Your institution
Name
Kgosi Kgari Teaching Hospital
Short code
KGK
Contact email
hod.paediatrics@kgk.wombat.local
Status
Active
Set by a Wombat administrator.
Open Kgosi Kgari Teaching Hospital

######## BOARD R3-U01-212-ten-1280.dc.html — U01 · 2.12 ten accounts; no pager on ten (F9); the filter card named “Filter people” (S21) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 10
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Fatma Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatma Khumalo]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U01-212-ten-768.dc.html — U01 · 2.12 ten accounts; no pager on ten (F9); the filter card named “Filter people” (S21) · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 10
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Fatma Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatma Khumalo]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U01-212-ten-390.dc.html — U01 · 2.12 ten accounts; no pager on ten (F9); the filter card named “Filter people” (S21) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 10
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Fatma Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatma Khumalo]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U02-212-zulu-1280.dc.html — U02 · 2.12 “zulu” then Show: the count in the heading and a hidden live count; focus on the heading · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
[value: zulu]
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 1 of 10 match “zulu”
1 of 10 people match “zulu”.
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U02-212-zulu-390.dc.html — U02 · 2.12 “zulu” then Show: the count in the heading and a hidden live count; focus on the heading · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
[value: zulu]
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 1 of 10 match “zulu”
1 of 10 people match “zulu”.
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U02b-212-thandi-zulu-1280.dc.html — U02b · “thandi zulu” then Show: a name in either order finds her (S6) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
PROPOSED: S6: either order, or the address
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
[value: thandi zulu]
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 1 of 10 match “thandi zulu”
1 of 10 people match “thandi zulu”.
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U02b-212-thandi-zulu-390.dc.html — U02b · “thandi zulu” then Show: a name in either order finds her (S6) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
PROPOSED: S6: either order, or the address
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
[value: thandi zulu]
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 1 of 10 match “thandi zulu”
1 of 10 people match “thandi zulu”.
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
| Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U03-212-no-match-1280.dc.html — U03 · no match (“zzz”) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Illustration
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
[value: zzz]
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 0 of 10 match “zzz”
No one matches “zzz”.
No one matches “zzz”
Nobody at Kgosi Kgari Teaching Hospital has “zzz” in their name or address. Press Clear to see all 10 people.

######## BOARD R3-U03-212-no-match-390.dc.html — U03 · no match (“zzz”) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Illustration
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
[value: zzz]
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 0 of 10 match “zzz”
No one matches “zzz”.
No one matches “zzz”
Nobody at Kgosi Kgari Teaching Hospital has “zzz” in their name or address. Press Clear to see all 10 people.

######## BOARD R3-U04-213-two-roles-1280.dc.html — U04 · 2.13 after three Add roles: scope Paediatrics / Paediatrics · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 10
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Fatma Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatma Khumalo]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U04-213-two-roles-390.dc.html — U04 · 2.13 after three Add roles: scope Paediatrics / Paediatrics · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 10
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Fatma Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatma Khumalo]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U05-228-fifteen-pending-1280.dc.html — U05 · 2.28 fifteen, five Pending trainee; no pager on fifteen (F9) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 15
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Pending trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
- Pending trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatma Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatma Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Pending trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| Lerato Molefemolefe@kgk.wombat.local
- Pending trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Lerato Molefe]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Sipho Ndlovundlovu@kgk.wombat.local
- Pending trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sipho Ndlovu]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U05-228-fifteen-pending-390.dc.html — U05 · 2.28 fifteen, five Pending trainee; no pager on fifteen (F9) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 15
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Pending trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
- Pending trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatma Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatma Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Pending trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| Lerato Molefemolefe@kgk.wombat.local
- Pending trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Lerato Molefe]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Sipho Ndlovundlovu@kgk.wombat.local
- Pending trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sipho Ndlovu]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U06-244-fifteen-1280.dc.html — U06 · 2.44 fifteen onboarded; Fatima Khumalo · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 15
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatima Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatima Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| Lerato Molefemolefe@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Lerato Molefe]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Sipho Ndlovundlovu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sipho Ndlovu]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U06-244-fifteen-390.dc.html — U06 · 2.44 fifteen onboarded; Fatima Khumalo · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 15
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatima Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatima Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| Lerato Molefemolefe@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Lerato Molefe]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Sipho Ndlovundlovu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sipho Ndlovu]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U07-a114-fourteen-1280.dc.html — U07 · A.1.14 fourteen: no Sipho Ndlovu; du Plessis and Molefe No role with the sentence · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 14
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatima Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatima Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| Lerato Molefemolefe@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Lerato Molefe]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U07-a114-fourteen-390.dc.html — U07 · A.1.14 fourteen: no Sipho Ndlovu; du Plessis and Molefe No role with the sentence · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 14
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatima Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatima Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| Lerato Molefemolefe@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Lerato Molefe]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U08-a64-patel-locked-1280.dc.html — U08 · A.6.4 Mohammed Patel locked: the Locked out badge · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 14
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatima Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatima Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| Lerato Molefemolefe@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Lerato Molefe]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Locked out
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U08-a64-patel-locked-390.dc.html — U08 · A.6.4 Mohammed Patel locked: the Locked out badge · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 14
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatima Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatima Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| Lerato Molefemolefe@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Lerato Molefe]
Manage
| David Naidoonaidoo@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Locked out
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage

######## BOARD R3-U09-no-role-filter-1280.dc.html — U09 · Role at “No role” then Show: du Plessis and Molefe · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
A completed or withdrawn registrar keeps an account and its records.
Status
Any status
Active
Locked out
Show
Clear
### People 2 of 14 hold no role
2 of 14 people hold no role.
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Pieter du Plessisduplessis@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Lerato Molefemolefe@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Lerato Molefe]
Manage

######## BOARD R3-U09-no-role-filter-390.dc.html — U09 · Role at “No role” then Show: du Plessis and Molefe · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
A completed or withdrawn registrar keeps an account and its records.
Status
Any status
Active
Locked out
Show
Clear
### People 2 of 14 hold no role
2 of 14 people hold no role.
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in.
| Name
| Roles
| Scope
| Status
| Actions
| Pieter du Plessisduplessis@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Lerato Molefemolefe@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Lerato Molefe]
Manage

######## BOARD R3-U10-heavy-1280.dc.html — U10 · the heavy set: about 200 accounts, rows 1–20, the pager with no “Page 1 of ‹10›” (F9) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Illustration
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People ‹about 200›
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in. Rows 1 to 20; the bracketed rows’ order not shown.
| Name
| Roles
| Scope
| Status
| Actions
| ‹person holding four roles›‹a-sixty-character-address-of-the-heavyset›@kgk.wombat.local
- Speciality admin
- Coordinator
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹person holding four roles›]
Manage
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatima Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatima Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| ‹further person 1›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 1›]
Manage
| ‹further person 2›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 2›]
Manage
| ‹further person 3›‹address›
- Trainee
| Paediatrics / Paediatrics
| Locked out
[aria-label: Manage ‹further person 3›]
Manage
| ‹further person 4›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 4›]
Manage
| ‹further person 5›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 5›]
Manage
| ‹further person 6›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 6›]
Manage
| ‹further person 7›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 7›]
Manage
| ‹further person 8›‹address›
- Trainee
| Paediatrics / Paediatrics
| Locked out
[aria-label: Manage ‹further person 8›]
Manage
| ‹further person 9›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 9›]
Manage
| ‹further person 10›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 10›]
Manage
| ‹further person 11›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 11›]
Manage
| ‹further person 12›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 12›]
Manage
| ‹further person 13›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 13›]
Manage
[aria-label: Pages of people]
Showing 1–20 of ‹about 200›
Rows per page
10
20
50
100
Previous
Next

######## BOARD R3-U10-heavy-768.dc.html — U10 · the heavy set: about 200 accounts, rows 1–20, the pager with no “Page 1 of ‹10›” (F9) · Illustration · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Illustration
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People ‹about 200›
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in. Rows 1 to 20; the bracketed rows’ order not shown.
| Name
| Roles
| Scope
| Status
| Actions
| ‹person holding four roles›‹a-sixty-character-address-of-the-heavyset›@kgk.wombat.local
- Speciality admin
- Coordinator
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹person holding four roles›]
Manage
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatima Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatima Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| ‹further person 1›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 1›]
Manage
| ‹further person 2›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 2›]
Manage
| ‹further person 3›‹address›
- Trainee
| Paediatrics / Paediatrics
| Locked out
[aria-label: Manage ‹further person 3›]
Manage
| ‹further person 4›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 4›]
Manage
| ‹further person 5›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 5›]
Manage
| ‹further person 6›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 6›]
Manage
| ‹further person 7›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 7›]
Manage
| ‹further person 8›‹address›
- Trainee
| Paediatrics / Paediatrics
| Locked out
[aria-label: Manage ‹further person 8›]
Manage
| ‹further person 9›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 9›]
Manage
| ‹further person 10›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 10›]
Manage
| ‹further person 11›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 11›]
Manage
| ‹further person 12›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 12›]
Manage
| ‹further person 13›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 13›]
Manage
[aria-label: Pages of people]
Showing 1–20 of ‹about 200›
Rows per page
10
20
50
100
Previous
Next

######## BOARD R3-U10-heavy-390.dc.html — U10 · the heavy set: about 200 accounts, rows 1–20, the pager with no “Page 1 of ‹10›” (F9) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Illustration
Everyone with an account at Kgosi Kgari Teaching Hospital: the roles each holds, their scope, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People ‹about 200›
At Kgosi Kgari Teaching Hospital, by last name: each person's roles, one scope for all of them, and whether they can sign in. Rows 1 to 20; the bracketed rows’ order not shown.
| Name
| Roles
| Scope
| Status
| Actions
| ‹person holding four roles›‹a-sixty-character-address-of-the-heavyset›@kgk.wombat.local
- Speciality admin
- Coordinator
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹person holding four roles›]
Manage
| Sarah Bothabotha@kgk.wombat.local
- Committee member
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Anele Dlaminidlamini@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Anele Dlamini]
Manage
| Pieter du Plessisduplessis@kgk.wombat.local
| No roleA completed or withdrawn registrar keeps an account and its records.
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Pieter du Plessis]
Manage
| Fatima Khumalokhumalo@kgk.wombat.local
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatima Khumalo]
Manage
| Nomsa Mahlangumahlangu@kgk.wombat.local
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Nomsa Mahlangu]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| ‹further person 1›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 1›]
Manage
| ‹further person 2›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 2›]
Manage
| ‹further person 3›‹address›
- Trainee
| Paediatrics / Paediatrics
| Locked out
[aria-label: Manage ‹further person 3›]
Manage
| ‹further person 4›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 4›]
Manage
| ‹further person 5›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 5›]
Manage
| ‹further person 6›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 6›]
Manage
| ‹further person 7›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 7›]
Manage
| ‹further person 8›‹address›
- Trainee
| Paediatrics / Paediatrics
| Locked out
[aria-label: Manage ‹further person 8›]
Manage
| ‹further person 9›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 9›]
Manage
| ‹further person 10›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 10›]
Manage
| ‹further person 11›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 11›]
Manage
| ‹further person 12›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 12›]
Manage
| ‹further person 13›‹address›
- Trainee
| Paediatrics / Paediatrics
| Active
[aria-label: Manage ‹further person 13›]
Manage
[aria-label: Pages of people]
Showing 1–20 of ‹about 200›
Rows per page
10
20
50
100
Previous
Next

######## BOARD R3-U11-loading-1280.dc.html — U11 · loading: the subtitle without the institution’s name until it loads (S9) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at your institution: the roles each holds, their scope, and whether they can sign in.
Loading the users.

######## BOARD R3-U11-loading-390.dc.html — U11 · loading: the subtitle without the institution’s name until it loads (S9) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Everyone with an account at your institution: the roles each holds, their scope, and whether they can sign in.
Loading the users.

######## BOARD R3-U12-load-error-1280.dc.html — U12 · a failed read; the subtitle without the name (S9) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Illustration
Everyone with an account at your institution: the roles each holds, their scope, and whether they can sign in.
Could not load the users. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-U12-load-error-390.dc.html — U12 · a failed read; the subtitle without the name (S9) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Users
Illustration
Everyone with an account at your institution: the roles each holds, their scope, and whether they can sign in.
Could not load the users. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-U13-trainee-note-1280.dc.html — U13 · Trainee beside an admin role: the note, no subtitle (S8), not a live region (S23) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
‹a person holding Trainee and Institutional admin›
Sign out
## Users
Illustration
You hold the Trainee role, so you cannot view or change user accounts, including your own.

######## BOARD R3-U13-trainee-note-390.dc.html — U13 · Trainee beside an admin role: the note, no subtitle (S8), not a live region (S23) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
‹a person holding Trainee and Institutional admin›
Sign out
## Users
Illustration
You hold the Trainee role, so you cannot view or change user accounts, including your own.

######## BOARD R3-U14-admin-view-1280.dc.html — U14 · the Administrator at 2.12: Institution column; Administrator and College admin in Role (S6); the ‹Demo account› rows bracketed (S14) · Illustration · 1280
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
## Users
Illustration
Every account in Wombat: the institution, roles and scope of each, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Administrator
College admin
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 18
Every institution, by last name: each person’s institution, roles, scope and status. The ‹Demo account› rows’ order not shown.
| Name
| Institution
| Roles
| Scope
| Status
| Actions
| ‹College admin name›collegeadmin@wombat.local
| Global (no institution)
- College admin
| ‹College›
| Active
[aria-label: Manage ‹College admin name›]
Manage
| Demo Administratordevadmin@wombat.local
| Global (no institution)
- Administrator
| —
| Active
[aria-label: Manage Demo Administrator]
Manage
| Sarah Bothabotha@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Fatma Khumalokhumalo@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatma Khumalo]
Manage
| Anton Krugerkruger@cmsa.wombat.local
| Global (no institution)
- College admin
| College of Paediatricians of South Africa
| Active
[aria-label: Manage Anton Kruger]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| David Naidoonaidoo@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage
| ‹Demo account 1›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 1›]
Manage
| ‹Demo account 2›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 2›]
Manage
| ‹Demo account 3›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 3›]
Manage
| ‹Demo account 4›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 4›]
Manage
| ‹Demo account 5›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 5›]
Manage

######## BOARD R3-U14-admin-view-768.dc.html — U14 · the Administrator at 2.12: Institution column; Administrator and College admin in Role (S6); the ‹Demo account› rows bracketed (S14) · Illustration · 768
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
## Users
Illustration
Every account in Wombat: the institution, roles and scope of each, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Administrator
College admin
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 18
Every institution, by last name: each person’s institution, roles, scope and status. The ‹Demo account› rows’ order not shown.
| Name
| Institution
| Roles
| Scope
| Status
| Actions
| ‹College admin name›collegeadmin@wombat.local
| Global (no institution)
- College admin
| ‹College›
| Active
[aria-label: Manage ‹College admin name›]
Manage
| Demo Administratordevadmin@wombat.local
| Global (no institution)
- Administrator
| —
| Active
[aria-label: Manage Demo Administrator]
Manage
| Sarah Bothabotha@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Fatma Khumalokhumalo@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatma Khumalo]
Manage
| Anton Krugerkruger@cmsa.wombat.local
| Global (no institution)
- College admin
| College of Paediatricians of South Africa
| Active
[aria-label: Manage Anton Kruger]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| David Naidoonaidoo@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage
| ‹Demo account 1›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 1›]
Manage
| ‹Demo account 2›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 2›]
Manage
| ‹Demo account 3›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 3›]
Manage
| ‹Demo account 4›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 4›]
Manage
| ‹Demo account 5›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 5›]
Manage

######## BOARD R3-U14-admin-view-390.dc.html — U14 · the Administrator at 2.12: Institution column; Administrator and College admin in Role (S6); the ‹Demo account› rows bracketed (S14) · Illustration · 390
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
## Users
Illustration
Every account in Wombat: the institution, roles and scope of each, and whether they can sign in.
Invite a person
[aria-label: Filter people]
Name or email
A name in either order, part of one, or an address.
Role
Any role
Administrator
College admin
Institutional admin
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Pending trainee
Trainee
No role
Status
Any status
Active
Locked out
Show
Clear
### People 18
Every institution, by last name: each person’s institution, roles, scope and status. The ‹Demo account› rows’ order not shown.
| Name
| Institution
| Roles
| Scope
| Status
| Actions
| ‹College admin name›collegeadmin@wombat.local
| Global (no institution)
- College admin
| ‹College›
| Active
[aria-label: Manage ‹College admin name›]
Manage
| Demo Administratordevadmin@wombat.local
| Global (no institution)
- Administrator
| —
| Active
[aria-label: Manage Demo Administrator]
Manage
| Sarah Bothabotha@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Paediatrics
| Active
[aria-label: Manage Sarah Botha]
Manage
| Fatma Khumalokhumalo@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Fatma Khumalo]
Manage
| Anton Krugerkruger@cmsa.wombat.local
| Global (no institution)
- College admin
| College of Paediatricians of South Africa
| Active
[aria-label: Manage Anton Kruger]
Manage
| Nolwazi Mbathambatha@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Institutional admin
| Kgosi Kgari Teaching Hospital
| Active
[aria-label: Manage Nolwazi Mbatha]
Manage
| Refilwe Mokoenamokoena@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Speciality admin
| Paediatrics
| Active
[aria-label: Manage Refilwe Mokoena]
Manage
| David Naidoonaidoo@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Paediatrics
| Active
[aria-label: Manage David Naidoo]
Manage
| Mohammed Patelpatel@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Assessor
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Mohammed Patel]
Manage
| Kabelo Sitholesithole@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Sub-speciality admin
| Paediatrics / Paediatrics
| Active
[aria-label: Manage Kabelo Sithole]
Manage
| Pieter Smitsmit@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Coordinator
| Any speciality
| Active
[aria-label: Manage Pieter Smit]
Manage
| John van Rensburgvanrensburg@sun.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Any speciality
| Active
[aria-label: Manage John van Rensburg]
Manage
| Thandi Zuluzulu@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
- Committee member
| Paediatrics
| Active
[aria-label: Manage Thandi Zulu]
Manage
| ‹Demo account 1›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 1›]
Manage
| ‹Demo account 2›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 2›]
Manage
| ‹Demo account 3›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 3›]
Manage
| ‹Demo account 4›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 4›]
Manage
| ‹Demo account 5›‹address›
| Demo Institution
- ‹role›
| ‹…›
| Active
[aria-label: Manage ‹Demo account 5›]
Manage

######## BOARD R3-P01-212-own-390.dc.html — P01 · 2.12 her own page: the note not a live region (S23); the who line a detail list (S19) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Nolwazi Mbatha
## Nolwazi Mbatha
Your own account: your roles and how you sign in.
[aria-label: Account]
Email
mbatha@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Kgosi Kgari Teaching Hospital
This is your own account, so you cannot change its roles, lockout or password here. Another administrator can change your roles or lockout. Change your password from your account.
### Roles and scope
- Institutional admin
### Sign-in and access
#### Institutional sign-in
No institutional sign-in is linked. Nolwazi Mbatha signs in with an email and a password.

######## BOARD R3-P02-213-zulu-before-390.dc.html — P02 · 2.13 Thandi Zulu before: lockout without the assessor line (she is not one yet) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P03-213-add-opened-1280.dc.html — P03 · 2.13 Add a role… opened: a note panel (F11), focus on the first radio; aria-controls now, no role="group", help outside each label (M10) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
Add a role…
Role *
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
Sub-speciality admin
Runs one sub-speciality's committee work.
Coordinator
Works across the institution, kept to Thandi Zulu's scope: Paediatrics.
Assessor
Assesses the registrars who ask them, at this institution.
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
Cancel
Add role
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P03-213-add-opened-390.dc.html — P03 · 2.13 Add a role… opened: a note panel (F11), focus on the first radio; aria-controls now, no role="group", help outside each label (M10) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
Add a role…
Role *
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
Sub-speciality admin
Runs one sub-speciality's committee work.
Coordinator
Works across the institution, kept to Thandi Zulu's scope: Paediatrics.
Assessor
Assesses the registrars who ask them, at this institution.
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
Cancel
Add role
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P04-213-assessor-chosen-1280.dc.html — P04 · 2.13 Assessor chosen: Speciality and Sub-speciality read-outs as dl.form-group (M8), the person’s institution’s adopted set (S13) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
Add a role…
Role *
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
Sub-speciality admin
Runs one sub-speciality's committee work.
Coordinator
Works across the institution, kept to Thandi Zulu's scope: Paediatrics.
Assessor
Assesses the registrars who ask them, at this institution.
Speciality
Paediatrics
Already held by Thandi Zulu.
Sub-speciality
Paediatrics
The only sub-speciality Kgosi Kgari Teaching Hospital has adopted.
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
Cancel
Add role
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P04-213-assessor-chosen-390.dc.html — P04 · 2.13 Assessor chosen: Speciality and Sub-speciality read-outs as dl.form-group (M8), the person’s institution’s adopted set (S13) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
Add a role…
Role *
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
Sub-speciality admin
Runs one sub-speciality's committee work.
Coordinator
Works across the institution, kept to Thandi Zulu's scope: Paediatrics.
Assessor
Assesses the registrars who ask them, at this institution.
Speciality
Paediatrics
Already held by Thandi Zulu.
Sub-speciality
Paediatrics
The only sub-speciality Kgosi Kgari Teaching Hospital has adopted.
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
Cancel
Add role
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P05-213-assessor-added-1280.dc.html — P05 · 2.13 Assessor added: the panel gone, the trigger closed with no aria-controls (M10); the result focused · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Assessor added to Thandi Zulu. Thandi Zulu's scope is now Paediatrics / Paediatrics. Any open session ends within a minute.
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
No assessor training is recorded for Thandi Zulu yet.
[aria-label: Record training for Thandi Zulu]
Record training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P05-213-assessor-added-768.dc.html — P05 · 2.13 Assessor added: the panel gone, the trigger closed with no aria-controls (M10); the result focused · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Assessor added to Thandi Zulu. Thandi Zulu's scope is now Paediatrics / Paediatrics. Any open session ends within a minute.
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
No assessor training is recorded for Thandi Zulu yet.
[aria-label: Record training for Thandi Zulu]
Record training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P05-213-assessor-added-390.dc.html — P05 · 2.13 Assessor added: the panel gone, the trigger closed with no aria-controls (M10); the result focused · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Assessor added to Thandi Zulu. Thandi Zulu's scope is now Paediatrics / Paediatrics. Any open session ends within a minute.
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
No assessor training is recorded for Thandi Zulu yet.
[aria-label: Record training for Thandi Zulu]
Record training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P06-remove-dialog-1280.dc.html — P06 · Remove… on Assessor: the waiting line an Alert (S19), “does not move to anyone else” (S3); the scope sentence says what it means · Illustration · 1280
Illustration
### Remove Assessor from Thandi Zulu?
Thandi Zulu can no longer be named as an activity's assessor, and leaves the Assessors list. Any open session ends within a minute.
‹n› requests waiting for Thandi Zulu stay with them; they do not move to anyone else, and no reminder is sent about them. DATA: on open
Thandi Zulu's assessor training is kept, and shows again if Assessor is given back.
Thandi Zulu's scope stays Paediatrics / Paediatrics, for Committee member, the role still held (F2).
Keep the role
Remove Assessor

######## BOARD R3-P06-remove-dialog-390.dc.html — P06 · Remove… on Assessor: the waiting line an Alert (S19), “does not move to anyone else” (S3); the scope sentence says what it means · Illustration · 390
Illustration
### Remove Assessor from Thandi Zulu?
Thandi Zulu can no longer be named as an activity's assessor, and leaves the Assessors list. Any open session ends within a minute.
‹n› requests waiting for Thandi Zulu stay with them; they do not move to anyone else, and no reminder is sent about them. DATA: on open
Thandi Zulu's assessor training is kept, and shows again if Assessor is given back.
Thandi Zulu's scope stays Paediatrics / Paediatrics, for Committee member, the role still held (F2).
Keep the role
Remove Assessor

######## BOARD R3-P07-removed-1280.dc.html — P07 · Assessor removed: the result focused; Assessor training hidden, kept · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Assessor removed from Thandi Zulu. Any open session ends within a minute.
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P07-removed-390.dc.html — P07 · Assessor removed: the result focused; Assessor training hidden, kept · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Assessor removed from Thandi Zulu. Any open session ends within a minute.
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P08-speciality-refused-1280.dc.html — P08 · two adopted specialities: Speciality admin with none chosen, refused under Speciality, focused · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Pieter Smit
## Pieter Smit
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
smit@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Any speciality
### Roles and scope
- Coordinator
[aria-label: Remove Coordinator from Pieter Smit]
Remove…
Add a role…
Role *
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
Assessor
Assesses the registrars who ask them, at this institution.
Speciality *
Choose a speciality
Paediatrics
‹second adopted speciality›
Speciality administrators must be scoped to a speciality.
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
Cancel
Add role
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Pieter Smit signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Pieter Smit]
Lock out…

######## BOARD R3-P08-speciality-refused-390.dc.html — P08 · two adopted specialities: Speciality admin with none chosen, refused under Speciality, focused · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Pieter Smit
## Pieter Smit
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
smit@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Any speciality
### Roles and scope
- Coordinator
[aria-label: Remove Coordinator from Pieter Smit]
Remove…
Add a role…
Role *
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
Assessor
Assesses the registrars who ask them, at this institution.
Speciality *
Choose a speciality
Paediatrics
‹second adopted speciality›
Speciality administrators must be scoped to a speciality.
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
Cancel
Add role
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Pieter Smit signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Pieter Smit]
Lock out…

######## BOARD R3-P09-a45-duplessis-768.dc.html — P09 · A.4.5 Pieter du Plessis, no role (an ended profile): lockout list one line · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Pieter du Plessis
## Pieter du Plessis
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
duplessis@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Pieter du Plessis holds no role: they left the programme on 2026-10-18. They can still sign in to read their training record (My progress) and their data rights. DATA: the ended trainee profile
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Pieter du Plessis signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Pieter du Plessis]
Lock out…

######## BOARD R3-P09-a45-duplessis-390.dc.html — P09 · A.4.5 Pieter du Plessis, no role (an ended profile): lockout list one line · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Pieter du Plessis
## Pieter du Plessis
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
duplessis@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Pieter du Plessis holds no role: they left the programme on 2026-10-18. They can still sign in to read their training record (My progress) and their data rights. DATA: the ended trainee profile
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Pieter du Plessis signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Pieter du Plessis]
Lock out…

######## BOARD R3-P10-a45-reset-refused-390.dc.html — P10 · A.4.5 a 10-character password refused · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Pieter du Plessis
## Pieter du Plessis
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
duplessis@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Pieter du Plessis holds no role: they left the programme on 2026-10-18. They can still sign in to read their training record (My progress) and their data rights. DATA: the ended trainee profile
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
The password was not reset. The new password needs: At least 12 characters. An upper-case letter. A symbol, such as ! or #.
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password does not meet the rules below.
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Pieter du Plessis signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Pieter du Plessis]
Lock out…

######## BOARD R3-P11-a45-reset-done-390.dc.html — P11 · A.4.5 reset: the result naming him, focused · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Pieter du Plessis
## Pieter du Plessis
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
duplessis@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Pieter du Plessis holds no role: they left the programme on 2026-10-18. They can still sign in to read their training record (My progress) and their data rights. DATA: the ended trainee profile
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Password reset for Pieter du Plessis. Share the new password with Pieter du Plessis out-of-band.
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Pieter du Plessis signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Pieter du Plessis]
Lock out…

######## BOARD R3-P12-517-molefe-no-role-390.dc.html — P12 · 5.17 Lerato Molefe after completing: no role, “completed” · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Lerato Molefe
## Lerato Molefe
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Lerato Molefe holds no role: they completed the programme on 2026-10-20. They can still sign in to read their training record (My progress) and their data rights. DATA: the ended trainee profile
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Lerato Molefe signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Lerato Molefe]
Lock out…

######## BOARD R3-P13-228-molefe-pending-390.dc.html — P13 · 2.28 Lerato Molefe as a pending registrar · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Lerato Molefe
## Lerato Molefe
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
molefe@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Pending traineeSystem-managed: waiting to be admitted. Open Trainees
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Lerato Molefe signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Lerato Molefe]
Lock out…

######## BOARD R3-P14-a64-patel-before-390.dc.html — P14 · A.6.4 Mohammed Patel before the lock: two lines (no panel, no trainee line) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Mohammed Patel
## Mohammed Patel
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
patel@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from Mohammed Patel]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training status
In training
Training completed
Not recorded
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Mohammed Patel]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Mohammed Patel signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Mohammed Patel]
Lock out…

######## BOARD R3-P15-a64-lock-dialog-1280.dc.html — P15 · A.6.4 the dialog: two effects as they read for an Assessor, the waiting request as an Alert, “does not move to anyone else” · 1280
### Lock out Mohammed Patel?
Mohammed Patel can no longer sign in, and any open session ends within a minute. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
1 request is waiting for Mohammed Patel, since 2026-10-12 12:35 SAST. It stays with them; it does not move to anyone else, and no reminder is sent about it. DATA: GetRequestsWaitingForUserQuery, on open
- Pieter du Plessis · Portfolio and Logbook Review (Paediatrics) · PAED-015 · waiting since 2026-10-12 12:35 SAST · overdue
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
Keep access
Lock out Mohammed Patel

######## BOARD R3-P15-a64-lock-dialog-390.dc.html — P15 · A.6.4 the dialog: two effects as they read for an Assessor, the waiting request as an Alert, “does not move to anyone else” · 390
### Lock out Mohammed Patel?
Mohammed Patel can no longer sign in, and any open session ends within a minute. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
1 request is waiting for Mohammed Patel, since 2026-10-12 12:35 SAST. It stays with them; it does not move to anyone else, and no reminder is sent about it. DATA: GetRequestsWaitingForUserQuery, on open
- Pieter du Plessis · Portfolio and Logbook Review (Paediatrics) · PAED-015 · waiting since 2026-10-12 12:35 SAST · overdue
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
Keep access
Lock out Mohammed Patel

######## BOARD R3-P16-a64-locked-768.dc.html — P16 · A.6.4 locked: the result in Lockout, focused; Status Locked out · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Mohammed Patel
## Mohammed Patel
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
patel@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Locked out
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from Mohammed Patel]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training status
In training
Training completed
Not recorded
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Mohammed Patel]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Mohammed Patel signs in with an email and a password.
#### Lockout
Mohammed Patel is locked out. Any open session ends within a minute.
This user is currently locked out and cannot sign in.
Reactivate user

######## BOARD R3-P16-a64-locked-390.dc.html — P16 · A.6.4 locked: the result in Lockout, focused; Status Locked out · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Mohammed Patel
## Mohammed Patel
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
patel@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Locked out
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from Mohammed Patel]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training status
In training
Training completed
Not recorded
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Mohammed Patel]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Mohammed Patel signs in with an email and a password.
#### Lockout
Mohammed Patel is locked out. Any open session ends within a minute.
This user is currently locked out and cannot sign in.
Reactivate user

######## BOARD R3-P17-a67-reactivated-390.dc.html — P17 · A.6.7 reactivated in one press · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Mohammed Patel
## Mohammed Patel
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
patel@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from Mohammed Patel]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training status
In training
Training completed
Not recorded
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Mohammed Patel]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Mohammed Patel signs in with an email and a password.
#### Lockout
Mohammed Patel is reactivated and can sign in again.
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Mohammed Patel]
Lock out…

######## BOARD R3-P18-a79-dlamini-1280.dc.html — P18 · A.7.9 Anele Dlamini: two lines (the trainee line and reminders; no panel, no assessor line) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Anele Dlamini
## Anele Dlamini
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
dlamini@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- TraineeSystem-managed
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Anele Dlamini signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are not a current trainee. No committee review or feedback campaign can be started about them, a draft feedback campaign about them cannot be opened, and the programme's lists of its trainees leave them out: the speciality, sub-speciality and committee dashboards' trainee targets and counts, feedback coverage, decisions due, and the weekly digest's inactive trainees.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Anele Dlamini]
Lock out…

######## BOARD R3-P18-a79-dlamini-390.dc.html — P18 · A.7.9 Anele Dlamini: two lines (the trainee line and reminders; no panel, no assessor line) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Anele Dlamini
## Anele Dlamini
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
dlamini@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- TraineeSystem-managed
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Anele Dlamini signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are not a current trainee. No committee review or feedback campaign can be started about them, a draft feedback campaign about them cannot be opened, and the programme's lists of its trainees leave them out: the speciality, sub-speciality and committee dashboards' trainee targets and counts, feedback coverage, decisions due, and the weekly digest's inactive trainees.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Anele Dlamini]
Lock out…

######## BOARD R3-P19-four-roles-1280.dc.html — P19 · four roles and a 60-character address: nothing cut · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- ‹person holding four roles›
## ‹person holding four roles›
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
‹a-sixty-character-address-of-the-heavyset›@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Speciality admin
[aria-label: Remove Speciality admin from ‹person holding four roles›]
Remove…
- Coordinator
[aria-label: Remove Coordinator from ‹person holding four roles›]
Remove…
- Committee member
[aria-label: Remove Committee member from ‹person holding four roles›]
Remove…
- Assessor
[aria-label: Remove Assessor from ‹person holding four roles›]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
No assessor training is recorded for ‹person holding four roles› yet.
[aria-label: Record training for ‹person holding four roles›]
Record training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. ‹person holding four roles› signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out ‹person holding four roles›]
Lock out…

######## BOARD R3-P19-four-roles-390.dc.html — P19 · four roles and a 60-character address: nothing cut · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- ‹person holding four roles›
## ‹person holding four roles›
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
‹a-sixty-character-address-of-the-heavyset›@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Speciality admin
[aria-label: Remove Speciality admin from ‹person holding four roles›]
Remove…
- Coordinator
[aria-label: Remove Coordinator from ‹person holding four roles›]
Remove…
- Committee member
[aria-label: Remove Committee member from ‹person holding four roles›]
Remove…
- Assessor
[aria-label: Remove Assessor from ‹person holding four roles›]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
No assessor training is recorded for ‹person holding four roles› yet.
[aria-label: Record training for ‹person holding four roles›]
Record training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. ‹person holding four roles› signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out ‹person holding four roles›]
Lock out…

######## BOARD R3-P20-signin-listed-1280.dc.html — P20 · Thandi Zulu, who also holds a password: an institutional sign-in with Remove… allowed (M3) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training status
Trained
Training completed
2018-10-20
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Thandi Zulu]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
- ‹provider›Institutional sign-in.
[aria-label: Remove the ‹provider› sign-in from Thandi Zulu]
Remove…
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P20-signin-listed-390.dc.html — P20 · Thandi Zulu, who also holds a password: an institutional sign-in with Remove… allowed (M3) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training status
Trained
Training completed
2018-10-20
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Thandi Zulu]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
- ‹provider›Institutional sign-in.
[aria-label: Remove the ‹provider› sign-in from Thandi Zulu]
Remove…
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P21-signin-dialog-1280.dc.html — P21 · Remove… on Thandi Zulu’s sign-in (she holds a password): an action-mode ConfirmDialog (F7) · Illustration · 1280
Illustration
### Remove the ‹provider› sign-in from Thandi Zulu?
Thandi Zulu can no longer sign in through ‹provider›. Signing in with the email and a password still works.
Keep the sign-in
Remove sign-in

######## BOARD R3-P21-signin-dialog-390.dc.html — P21 · Remove… on Thandi Zulu’s sign-in (she holds a password): an action-mode ConfirmDialog (F7) · Illustration · 390
Illustration
### Remove the ‹provider› sign-in from Thandi Zulu?
Thandi Zulu can no longer sign in through ‹provider›. Signing in with the email and a password still works.
Keep the sign-in
Remove sign-in

######## BOARD R3-P22-signin-last-1280.dc.html — P22 · an SSO-only account (F3): Password says no password can be set, no field; Remove… refused in the built words, no hint · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- ‹a person who signs in only through ‹provider››
## ‹a person who signs in only through ‹provider››
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
‹address›
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from ‹a person who signs in only through ‹provider››]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
‹a person who signs in only through ‹provider›› signs in only through ‹provider›. A password cannot be set for them. NEW: F3 DATA: AllowLocalPassword, HasPassword
#### Institutional sign-in
- ‹provider›Institutional sign-in.
[aria-label: Remove the ‹provider› sign-in from ‹a person who signs in only through ‹provider››]
Remove…
The institutional sign-in is the account's only way in.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out ‹a person who signs in only through ‹provider››]
Lock out…

######## BOARD R3-P22-signin-last-390.dc.html — P22 · an SSO-only account (F3): Password says no password can be set, no field; Remove… refused in the built words, no hint · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- ‹a person who signs in only through ‹provider››
## ‹a person who signs in only through ‹provider››
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
‹address›
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from ‹a person who signs in only through ‹provider››]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
‹a person who signs in only through ‹provider›› signs in only through ‹provider›. A password cannot be set for them. NEW: F3 DATA: AllowLocalPassword, HasPassword
#### Institutional sign-in
- ‹provider›Institutional sign-in.
[aria-label: Remove the ‹provider› sign-in from ‹a person who signs in only through ‹provider››]
Remove…
The institutional sign-in is the account's only way in.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out ‹a person who signs in only through ‹provider››]
Lock out…

######## BOARD R3-P23-a55-unavailable-390.dc.html — P23 · A.5.5 User unavailable (also for an Administrator’s account by id) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- User
## User
User unavailable
The user could not be found or is outside your scope.
Go to Users

######## BOARD R3-P24-loading-390.dc.html — P24 · loading · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- User
## User
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
Loading this person.

######## BOARD R3-P25-load-error-390.dc.html — P25 · a failed read · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- User
## User
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
Could not load this person. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-P26-trainee-note-1280.dc.html — P26 · the Trainee’s note: no subtitle (S8), not a live region (S23) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
‹a person holding Trainee and Institutional admin›
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- User
## User
Illustration
You hold the Trainee role, so you cannot view or change user accounts, including your own.

######## BOARD R3-P26-trainee-note-390.dc.html — P26 · the Trainee’s note: no subtitle (S8), not a live region (S23) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
‹a person holding Trainee and Institutional admin›
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- User
## User
Illustration
You hold the Trainee role, so you cannot view or change user accounts, including your own.

######## BOARD R3-P27-admin-dev-trainee-1280.dc.html — P27 · the Administrator’s view of the dev trainee: Add a role reads the person’s institution’s adopted set (S13) · Illustration · 1280
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- ‹dev trainee’s name›
## ‹dev trainee’s name›
Illustration
Roles, sign-in and lockout for one person, with assessor training for an Assessor.
[aria-label: Account]
Email
trainee@wombat.local
Institution
Demo Institution
Status
Active
Scope
‹…›
### Roles and scope
- TraineeSystem-managed
Add a role…
Role *
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
Sub-speciality admin
Runs one sub-speciality's committee work.
Coordinator
Works across the institution, kept to ‹dev trainee’s name›'s scope: ‹…›.
Committee member
Sits committee reviews, in ‹dev trainee’s name›'s scope: ‹…›.
Assessor
Assesses the registrars who ask them, at this institution.
Speciality
‹Demo Institution’s adopted speciality›
Adopted by Demo Institution, this person's institution (S13).
Sub-speciality
‹Demo Institution’s adopted sub-speciality›
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
Cancel
Add role
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. ‹dev trainee’s name› signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are not a current trainee. No committee review or feedback campaign can be started about them, a draft feedback campaign about them cannot be opened, and the programme's lists of its trainees leave them out: the speciality, sub-speciality and committee dashboards' trainee targets and counts, feedback coverage, decisions due, and the weekly digest's inactive trainees.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out ‹dev trainee’s name›]
Lock out…

######## BOARD R3-P27-admin-dev-trainee-390.dc.html — P27 · the Administrator’s view of the dev trainee: Add a role reads the person’s institution’s adopted set (S13) · Illustration · 390
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- ‹dev trainee’s name›
## ‹dev trainee’s name›
Illustration
Roles, sign-in and lockout for one person, with assessor training for an Assessor.
[aria-label: Account]
Email
trainee@wombat.local
Institution
Demo Institution
Status
Active
Scope
‹…›
### Roles and scope
- TraineeSystem-managed
Add a role…
Role *
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
Sub-speciality admin
Runs one sub-speciality's committee work.
Coordinator
Works across the institution, kept to ‹dev trainee’s name›'s scope: ‹…›.
Committee member
Sits committee reviews, in ‹dev trainee’s name›'s scope: ‹…›.
Assessor
Assesses the registrars who ask them, at this institution.
Speciality
‹Demo Institution’s adopted speciality›
Adopted by Demo Institution, this person's institution (S13).
Sub-speciality
‹Demo Institution’s adopted sub-speciality›
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
Cancel
Add role
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. ‹dev trainee’s name› signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are not a current trainee. No committee review or feedback campaign can be started about them, a draft feedback campaign about them cannot be opened, and the programme's lists of its trainees leave them out: the speciality, sub-speciality and committee dashboards' trainee targets and counts, feedback coverage, decisions due, and the weekly digest's inactive trainees.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out ‹dev trainee’s name›]
Lock out…

######## BOARD R3-P28-f1-patel-committee-1280.dc.html — P28 · F1: Patel (Assessor, Paediatrics / Paediatrics) given Committee member: no refusal, the scope unchanged · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Mohammed Patel
## Mohammed Patel
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
patel@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Committee member added to Mohammed Patel. Mohammed Patel's scope stays Paediatrics / Paediatrics. Any open session ends within a minute.
- Assessor
[aria-label: Remove Assessor from Mohammed Patel]
Remove…
- Committee member
[aria-label: Remove Committee member from Mohammed Patel]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training status
In training
Training completed
Not recorded
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Mohammed Patel]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Mohammed Patel signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Mohammed Patel]
Lock out…

######## BOARD R3-P28-f1-patel-committee-390.dc.html — P28 · F1: Patel (Assessor, Paediatrics / Paediatrics) given Committee member: no refusal, the scope unchanged · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Mohammed Patel
## Mohammed Patel
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
patel@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
Committee member added to Mohammed Patel. Mohammed Patel's scope stays Paediatrics / Paediatrics. Any open session ends within a minute.
- Assessor
[aria-label: Remove Assessor from Mohammed Patel]
Remove…
- Committee member
[aria-label: Remove Committee member from Mohammed Patel]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training status
In training
Training completed
Not recorded
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Mohammed Patel]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Mohammed Patel signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Mohammed Patel]
Lock out…

######## BOARD R3-P29-f2-smit-form-1280.dc.html — P29 · F2: Smit choosing Speciality admin: the narrowing said before the save · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Pieter Smit
## Pieter Smit
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
smit@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Any speciality
### Roles and scope
- Coordinator
[aria-label: Remove Coordinator from Pieter Smit]
Remove…
Add a role…
Role *
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
Assessor
Assesses the registrars who ask them, at this institution.
Speciality
Paediatrics
The only speciality Kgosi Kgari Teaching Hospital has adopted.
Pieter Smit's scope becomes Paediatrics, for every role he holds. NEW: F2
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
Cancel
Add role
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Pieter Smit signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Pieter Smit]
Lock out…

######## BOARD R3-P29-f2-smit-form-390.dc.html — P29 · F2: Smit choosing Speciality admin: the narrowing said before the save · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Pieter Smit
## Pieter Smit
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
smit@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Any speciality
### Roles and scope
- Coordinator
[aria-label: Remove Coordinator from Pieter Smit]
Remove…
Add a role…
Role *
Institutional admin
Runs an institution in Wombat: invites and admits people, adopts curricula.
Speciality admin
Runs one speciality's committee work: panels, reviews and decisions due.
Sub-speciality admin
Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.
Committee member
Sits committee reviews. Choose a speciality only to keep them to it.
Assessor
Assesses the registrars who ask them, at this institution.
Speciality
Paediatrics
The only speciality Kgosi Kgari Teaching Hospital has adopted.
Pieter Smit's scope becomes Paediatrics, for every role he holds. NEW: F2
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
Cancel
Add role
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Pieter Smit signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Pieter Smit]
Lock out…

######## BOARD R3-P30-f2-smit-result-1280.dc.html — P30 · F2: the result repeats the narrowing; the who line reads Paediatrics · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Pieter Smit
## Pieter Smit
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
smit@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics
### Roles and scope
Speciality admin added to Pieter Smit. Pieter Smit's scope becomes Paediatrics, for every role he holds. Any open session ends within a minute.
- Coordinator
[aria-label: Remove Coordinator from Pieter Smit]
Remove…
- Speciality admin
[aria-label: Remove Speciality admin from Pieter Smit]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Pieter Smit signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Pieter Smit]
Lock out…

######## BOARD R3-P30-f2-smit-result-390.dc.html — P30 · F2: the result repeats the narrowing; the who line reads Paediatrics · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Pieter Smit
## Pieter Smit
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
smit@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics
### Roles and scope
Speciality admin added to Pieter Smit. Pieter Smit's scope becomes Paediatrics, for every role he holds. Any open session ends within a minute.
- Coordinator
[aria-label: Remove Coordinator from Pieter Smit]
Remove…
- Speciality admin
[aria-label: Remove Speciality admin from Pieter Smit]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Pieter Smit signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Pieter Smit]
Lock out…

######## BOARD R3-P31-f6-two-waiting-1280.dc.html — P31 · F6: N = 2, the count, both rows and the oldest; the second row bracketed · Illustration · 1280
Illustration
### Lock out Mohammed Patel?
Mohammed Patel can no longer sign in, and any open session ends within a minute. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
2 requests are waiting for Mohammed Patel, the oldest since ‹the older of the two›. They stay with them; they do not move to anyone else, and no reminder is sent about them. DATA: GetRequestsWaitingForUserQuery, on open
- Pieter du Plessis · Portfolio and Logbook Review (Paediatrics) · PAED-015 · waiting since 2026-10-12 12:35 SAST · overdue
- ‹trainee› · ‹activity type› · PAED-‹…› · waiting since ‹date› ‹time› SAST · ‹overdue or not›
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
Keep access
Lock out Mohammed Patel

######## BOARD R3-P32-f6-more-than-five-1280.dc.html — P32 · F6: N > 5: five rows and “and 3 more” · Illustration · 1280
Illustration
### Lock out Mohammed Patel?
Mohammed Patel can no longer sign in, and any open session ends within a minute. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
8 requests are waiting for Mohammed Patel, the oldest since ‹the oldest waiting-since›. They stay with them; they do not move to anyone else, and no reminder is sent about them. DATA: GetRequestsWaitingForUserQuery, on open
- Pieter du Plessis · Portfolio and Logbook Review (Paediatrics) · PAED-015 · waiting since 2026-10-12 12:35 SAST · overdue
- ‹trainee› · ‹activity type› · PAED-‹…› · waiting since ‹date› ‹time› SAST · ‹overdue or not›
- and 3 more.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
Keep access
Lock out Mohammed Patel

######## BOARD R3-P33-f7-signin-removed-1280.dc.html — P33 · F7: Thandi Zulu’s ‹provider› sign-in removed: the result in Sign-in and access, naming her, focused · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training status
Trained
Training completed
2018-10-20
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Thandi Zulu]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
The ‹provider› sign-in was removed from Thandi Zulu. Thandi Zulu signs in with an email and a password.
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P33-f7-signin-removed-390.dc.html — P33 · F7: Thandi Zulu’s ‹provider› sign-in removed: the result in Sign-in and access, naming her, focused · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training status
Trained
Training completed
2018-10-20
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Thandi Zulu]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
The ‹provider› sign-in was removed from Thandi Zulu. Thandi Zulu signs in with an email and a password.
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-P34-s1-two-ids-1280.dc.html — P34 · S1: a scope of two speciality ids, comma-joined (PROPOSED cell rule) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- ‹a person with two speciality ids›
## ‹a person with two speciality ids›
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
‹address›
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics, ‹second speciality›
### Roles and scope
- Committee member
[aria-label: Remove Committee member from ‹a person with two speciality ids›]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. ‹a person with two speciality ids› signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out ‹a person with two speciality ids›]
Lock out…

######## BOARD R3-P34-s1-two-ids-390.dc.html — P34 · S1: a scope of two speciality ids, comma-joined (PROPOSED cell rule) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- ‹a person with two speciality ids›
## ‹a person with two speciality ids›
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
‹address›
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics, ‹second speciality›
### Roles and scope
- Committee member
[aria-label: Remove Committee member from ‹a person with two speciality ids›]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. ‹a person with two speciality ids› signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out ‹a person with two speciality ids›]
Lock out…

######## BOARD R3-P35-s2-no-profile-1280.dc.html — P35 · S2: no role and no trainee profile (a staff member whose last role was removed) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- John van Rensburg
## John van Rensburg
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
vanrensburg@sun.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Any speciality
### Roles and scope
John van Rensburg holds no role. They can sign in to My account and My data rights. NEW: S2
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. John van Rensburg signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out John van Rensburg]
Lock out…

######## BOARD R3-P35-s2-no-profile-390.dc.html — P35 · S2: no role and no trainee profile (a staff member whose last role was removed) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- John van Rensburg
## John van Rensburg
Illustration
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
vanrensburg@sun.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Any speciality
### Roles and scope
John van Rensburg holds no role. They can sign in to My account and My data rights. NEW: S2
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. John van Rensburg signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out John van Rensburg]
Lock out…

######## BOARD R3-P36-s3-remove-sheet-1280.dc.html — P36 · S3: a Remove dialog per role, each one sentence; the last role says what is left (S2) · Illustration · 1280
Illustration
### Remove Committee member from David Naidoo?
David Naidoo leaves every committee panel and can no longer sit reviews. If David Naidoo chairs a review, its ratification waits until the panel has a chair again (T256). Any open session ends within a minute.
Keep the role
Remove Committee member
### Remove Speciality admin from Refilwe Mokoena?
Refilwe Mokoena no longer runs the Paediatrics committee's work: panels, reviews and decisions due. Any open session ends within a minute.
Refilwe Mokoena will hold no role. They can sign in to My account and My data rights.
Keep the role
Remove Speciality admin
### Remove Sub-speciality admin from Kabelo Sithole?
Kabelo Sithole no longer runs the Paediatrics / Paediatrics committee's work. Any open session ends within a minute.
Kabelo Sithole will hold no role. They can sign in to My account and My data rights.
Keep the role
Remove Sub-speciality admin
### Remove Coordinator from Pieter Smit?
Pieter Smit can no longer run MSF campaigns or chase requests waiting for assessors; a campaign already under way can still be finished. Any open session ends within a minute.
Pieter Smit will hold no role. They can sign in to My account and My data rights.
Keep the role
Remove Coordinator
### Remove Institutional admin from ‹another Institutional admin›?
‹another Institutional admin› can no longer invite, admit or manage people, or adopt curricula, at Kgosi Kgari Teaching Hospital. Any open session ends within a minute.
Keep the role
Remove Institutional admin
### Remove Committee member from John van Rensburg?
John van Rensburg leaves every committee panel and can no longer sit reviews. Any open session ends within a minute.
John van Rensburg will hold no role. They can sign in to My account and My data rights.
Keep the role
Remove Committee member

######## BOARD R3-P36-s3-remove-sheet-390.dc.html — P36 · S3: a Remove dialog per role, each one sentence; the last role says what is left (S2) · Illustration · 390
Illustration
### Remove Committee member from David Naidoo?
David Naidoo leaves every committee panel and can no longer sit reviews. If David Naidoo chairs a review, its ratification waits until the panel has a chair again (T256). Any open session ends within a minute.
Keep the role
Remove Committee member
### Remove Speciality admin from Refilwe Mokoena?
Refilwe Mokoena no longer runs the Paediatrics committee's work: panels, reviews and decisions due. Any open session ends within a minute.
Refilwe Mokoena will hold no role. They can sign in to My account and My data rights.
Keep the role
Remove Speciality admin
### Remove Sub-speciality admin from Kabelo Sithole?
Kabelo Sithole no longer runs the Paediatrics / Paediatrics committee's work. Any open session ends within a minute.
Kabelo Sithole will hold no role. They can sign in to My account and My data rights.
Keep the role
Remove Sub-speciality admin
### Remove Coordinator from Pieter Smit?
Pieter Smit can no longer run MSF campaigns or chase requests waiting for assessors; a campaign already under way can still be finished. Any open session ends within a minute.
Pieter Smit will hold no role. They can sign in to My account and My data rights.
Keep the role
Remove Coordinator
### Remove Institutional admin from ‹another Institutional admin›?
‹another Institutional admin› can no longer invite, admit or manage people, or adopt curricula, at Kgosi Kgari Teaching Hospital. Any open session ends within a minute.
Keep the role
Remove Institutional admin
### Remove Committee member from John van Rensburg?
John van Rensburg leaves every committee panel and can no longer sit reviews. Any open session ends within a minute.
John van Rensburg will hold no role. They can sign in to My account and My data rights.
Keep the role
Remove Committee member

######## BOARD R3-T01-none-390.dc.html — T01 · no Assessor holder yet (before 2.10) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Illustration
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
Nobody holds the Assessor role yet
An assessor’s training record holds their training status, the date they completed it and their qualifications. Give someone the Assessor role from their page in Users; they are listed here, and their training is recorded on their page.

######## BOARD R3-T02-214-five-unrecorded-1280.dc.html — T02 · 2.14 the five Assessor holders, nothing recorded · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital 5
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Not recorded
| —
[aria-label: Record training for Sarah Botha]
Record training
| Fatma Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Not recorded
| —
[aria-label: Record training for Fatma Khumalo]
Record training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Not recorded
| —
[aria-label: Record training for David Naidoo]
Record training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| Not recorded
| —
[aria-label: Record training for Mohammed Patel]
Record training
| Thandi Zuluzulu@kgk.wombat.local
| Paediatrics / Paediatrics
| Not recorded
| —
[aria-label: Record training for Thandi Zulu]
Record training

######## BOARD R3-T02-214-five-unrecorded-390.dc.html — T02 · 2.14 the five Assessor holders, nothing recorded · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital 5
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Not recorded
| —
[aria-label: Record training for Sarah Botha]
Record training
| Fatma Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Not recorded
| —
[aria-label: Record training for Fatma Khumalo]
Record training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Not recorded
| —
[aria-label: Record training for David Naidoo]
Record training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| Not recorded
| —
[aria-label: Record training for Mohammed Patel]
Record training
| Thandi Zuluzulu@kgk.wombat.local
| Paediatrics / Paediatrics
| Not recorded
| —
[aria-label: Record training for Thandi Zulu]
Record training

######## BOARD R3-T03-214-zulu-open-1280.dc.html — T03 · 2.14 ?section=training: the note panel open, focus on the first radio (M9, M10); no institution or speciality asked (M1) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Prerequisite: after T289
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
[aria-label: Record training for Thandi Zulu]
Record training
Training status *
Not started
In training
Provisional (awaiting sign-off)
Trained
Faculty-development lifecycle. In-training assessors may assess under supervision.
Qualifications *
MBChB, MMed (Paed), FC Paed (SA)
Cancel
Save training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-T03-214-zulu-open-390.dc.html — T03 · 2.14 ?section=training: the note panel open, focus on the first radio (M9, M10); no institution or speciality asked (M1) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Prerequisite: after T289
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
[aria-label: Record training for Thandi Zulu]
Record training
Training status *
Not started
In training
Provisional (awaiting sign-off)
Trained
Faculty-development lifecycle. In-training assessors may assess under supervision.
Qualifications *
MBChB, MMed (Paed), FC Paed (SA)
Cancel
Save training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-T04-214-zulu-saved-1280.dc.html — T04 · 2.14 Zulu saved: Trained, 2018-10-20 · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Prerequisite: after T289
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training saved for Thandi Zulu: Trained, completed 2018-10-20.
Training status
Trained
Training completed
2018-10-20
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Thandi Zulu]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-T04-214-zulu-saved-390.dc.html — T04 · 2.14 Zulu saved: Trained, 2018-10-20 · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Thandi Zulu
## Thandi Zulu
Prerequisite: after T289
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
zulu@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Committee member
[aria-label: Remove Committee member from Thandi Zulu]
Remove…
- Assessor
[aria-label: Remove Assessor from Thandi Zulu]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training saved for Thandi Zulu: Trained, completed 2018-10-20.
Training status
Trained
Training completed
2018-10-20
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Thandi Zulu]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They sit on no committee panel.
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Thandi Zulu]
Lock out…

######## BOARD R3-T05-214-khumalo-refused-1280.dc.html — T05 · 2.14 Fatma Khumalo, an empty save: both refusals, the group marked, focus on the first radio · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Fatma Khumalo
## Fatma Khumalo
Prerequisite: after T289
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
khumalo@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from Fatma Khumalo]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
[aria-label: Record training for Fatma Khumalo]
Record training
Training status *
Not started
In training
Provisional (awaiting sign-off)
Trained
Choose the training status.
Faculty-development lifecycle. In-training assessors may assess under supervision.
Qualifications *
Type the qualifications, as they should read.
Cancel
Save training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Fatma Khumalo signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Fatma Khumalo]
Lock out…

######## BOARD R3-T05-214-khumalo-refused-390.dc.html — T05 · 2.14 Fatma Khumalo, an empty save: both refusals, the group marked, focus on the first radio · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Fatma Khumalo
## Fatma Khumalo
Prerequisite: after T289
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
khumalo@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from Fatma Khumalo]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
[aria-label: Record training for Fatma Khumalo]
Record training
Training status *
Not started
In training
Provisional (awaiting sign-off)
Trained
Choose the training status.
Faculty-development lifecycle. In-training assessors may assess under supervision.
Qualifications *
Type the qualifications, as they should read.
Cancel
Save training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Fatma Khumalo signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Fatma Khumalo]
Lock out…

######## BOARD R3-T06-214-khumalo-filled-1280.dc.html — T06 · 2.14 Provisional (awaiting sign-off) chosen: the date asked, 2026-09-20; each radio’s label 44 px at 390 (S17) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Fatma Khumalo
## Fatma Khumalo
Prerequisite: after T289
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
khumalo@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from Fatma Khumalo]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
[aria-label: Record training for Fatma Khumalo]
Edit training
Training status *
Not started
In training
Provisional (awaiting sign-off)
Trained
Faculty-development lifecycle. In-training assessors may assess under supervision.
Training completed
[value: 2026-09-20]
Date the assessor completed assessor training. Leave blank if unrecorded.
Qualifications *
MBChB, MMed (Paed), FC Paed (SA)
Cancel
Save training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Fatma Khumalo signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Fatma Khumalo]
Lock out…

######## BOARD R3-T06-214-khumalo-filled-390.dc.html — T06 · 2.14 Provisional (awaiting sign-off) chosen: the date asked, 2026-09-20; each radio’s label 44 px at 390 (S17) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Fatma Khumalo
## Fatma Khumalo
Prerequisite: after T289
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
khumalo@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from Fatma Khumalo]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
[aria-label: Record training for Fatma Khumalo]
Edit training
Training status *
Not started
In training
Provisional (awaiting sign-off)
Trained
Faculty-development lifecycle. In-training assessors may assess under supervision.
Training completed
[value: 2026-09-20]
Date the assessor completed assessor training. Leave blank if unrecorded.
Qualifications *
MBChB, MMed (Paed), FC Paed (SA)
Cancel
Save training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Fatma Khumalo signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Fatma Khumalo]
Lock out…

######## BOARD R3-T07-214-khumalo-saved-1280.dc.html — T07 · 2.14 saved: the result naming her, focused · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Fatma Khumalo
## Fatma Khumalo
Prerequisite: after T289
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
khumalo@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from Fatma Khumalo]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training saved for Fatma Khumalo: Provisional (awaiting sign-off), completed 2026-09-20.
Training status
Provisional (awaiting sign-off)
Training completed
2026-09-20
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Fatma Khumalo]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Fatma Khumalo signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Fatma Khumalo]
Lock out…

######## BOARD R3-T07-214-khumalo-saved-390.dc.html — T07 · 2.14 saved: the result naming her, focused · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Users
- ›
- Fatma Khumalo
## Fatma Khumalo
Prerequisite: after T289
Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.
[aria-label: Account]
Email
khumalo@kgk.wombat.local
Institution
Kgosi Kgari Teaching Hospital
Status
Active
Scope
Paediatrics / Paediatrics
### Roles and scope
- Assessor
[aria-label: Remove Assessor from Fatma Khumalo]
Remove…
Add a role…
Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.
### Assessor training
Training saved for Fatma Khumalo: Provisional (awaiting sign-off), completed 2026-09-20.
Training status
Provisional (awaiting sign-off)
Training completed
2026-09-20
Qualifications
MBChB, MMed (Paed), FC Paed (SA)
[aria-label: Edit training for Fatma Khumalo]
Edit training
### Sign-in and access
#### Password
Sets a new password directly. The user is not emailed; share the password securely out-of-band.
New password
[aria-label: Show new password]
Show
The new password needs:
- At least 12 characters.
- At least 4 different characters.
- A digit (0 to 9).
- An upper-case letter.
- A lower-case letter.
- A symbol, such as ! or #.
Reset password
#### Institutional sign-in
No institutional sign-in is linked. Fatma Khumalo signs in with an email and a password.
#### Lockout
Locking out the user prevents sign-in immediately. Until they are reactivated:
- They are sent no activity reminders or weekly digests.
- They cannot be named as an activity's assessor.
Their records stay as they are, and a review or feedback campaign already under way can still be finished. Reactivating clears the lockout.
[aria-label: Lock out Fatma Khumalo]
Lock out…

######## BOARD R3-T08-215-five-1280.dc.html — T08 · 2.15 five recorded, by last name · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital 5
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatma Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatma Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training
| Thandi Zuluzulu@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2018-10-20
[aria-label: Edit training for Thandi Zulu]
Edit training

######## BOARD R3-T08-215-five-768.dc.html — T08 · 2.15 five recorded, by last name · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital 5
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatma Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatma Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training
| Thandi Zuluzulu@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2018-10-20
[aria-label: Edit training for Thandi Zulu]
Edit training

######## BOARD R3-T08-215-five-390.dc.html — T08 · 2.15 five recorded, by last name · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital 5
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatma Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatma Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training
| Thandi Zuluzulu@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2018-10-20
[aria-label: Edit training for Thandi Zulu]
Edit training

######## BOARD R3-T09-244-fatima-390.dc.html — T09 · 2.44 Fatima Khumalo · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital 5
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatima Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatima Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training
| Thandi Zuluzulu@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2018-10-20
[aria-label: Edit training for Thandi Zulu]
Edit training

######## BOARD R3-T10-zulu-removed-1280.dc.html — T10 · after Zulu’s Assessor is removed: four rows; no extra note (S11) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Illustration
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital 4
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatima Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatima Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training

######## BOARD R3-T10-zulu-removed-390.dc.html — T10 · after Zulu’s Assessor is removed: four rows; no extra note (S11) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Illustration
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital 4
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatima Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatima Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training

######## BOARD R3-T11-thirty-1280.dc.html — T11 · thirty holders: rows 1–20 and the pager · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Illustration
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital ‹30›
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page. The bracketed rows’ order not shown.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatima Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatima Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training
| Thandi Zuluzulu@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2018-10-20
[aria-label: Edit training for Thandi Zulu]
Edit training
| ‹further assessor 1›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 1›]
Edit training
| ‹further assessor 2›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 2›]
Edit training
| ‹further assessor 3›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 3›]
Edit training
| ‹further assessor 4›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 4›]
Edit training
| ‹further assessor 5›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 5›]
Edit training
| ‹further assessor 6›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 6›]
Edit training
| ‹further assessor 7›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 7›]
Edit training
| ‹further assessor 8›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 8›]
Edit training
| ‹further assessor 9›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 9›]
Edit training
| ‹further assessor 10›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 10›]
Edit training
| ‹further assessor 11›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 11›]
Edit training
| ‹further assessor 12›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 12›]
Edit training
| ‹further assessor 13›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 13›]
Edit training
| ‹further assessor 14›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 14›]
Edit training
| ‹further assessor 15›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 15›]
Edit training
[aria-label: Pages of people]
Showing 1–20 of ‹30›
Rows per page
10
20
50
100
Previous
Next

######## BOARD R3-T11-thirty-768.dc.html — T11 · thirty holders: rows 1–20 and the pager · Illustration · 768
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Illustration
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital ‹30›
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page. The bracketed rows’ order not shown.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatima Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatima Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training
| Thandi Zuluzulu@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2018-10-20
[aria-label: Edit training for Thandi Zulu]
Edit training
| ‹further assessor 1›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 1›]
Edit training
| ‹further assessor 2›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 2›]
Edit training
| ‹further assessor 3›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 3›]
Edit training
| ‹further assessor 4›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 4›]
Edit training
| ‹further assessor 5›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 5›]
Edit training
| ‹further assessor 6›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 6›]
Edit training
| ‹further assessor 7›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 7›]
Edit training
| ‹further assessor 8›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 8›]
Edit training
| ‹further assessor 9›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 9›]
Edit training
| ‹further assessor 10›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 10›]
Edit training
| ‹further assessor 11›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 11›]
Edit training
| ‹further assessor 12›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 12›]
Edit training
| ‹further assessor 13›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 13›]
Edit training
| ‹further assessor 14›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 14›]
Edit training
| ‹further assessor 15›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 15›]
Edit training
[aria-label: Pages of people]
Showing 1–20 of ‹30›
Rows per page
10
20
50
100
Previous
Next

######## BOARD R3-T11-thirty-390.dc.html — T11 · thirty holders: rows 1–20 and the pager · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Illustration
Everyone at Kgosi Kgari Teaching Hospital who holds the Assessor role, with the assessor training recorded for each.
### Assessors at Kgosi Kgari Teaching Hospital ‹30›
At Kgosi Kgari Teaching Hospital, by last name: each holder of the Assessor role, and the training recorded on their page. The bracketed rows’ order not shown.
| Name
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatima Khumalokhumalo@kgk.wombat.local
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatima Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training
| Thandi Zuluzulu@kgk.wombat.local
| Paediatrics / Paediatrics
| Trained
| 2018-10-20
[aria-label: Edit training for Thandi Zulu]
Edit training
| ‹further assessor 1›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 1›]
Edit training
| ‹further assessor 2›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 2›]
Edit training
| ‹further assessor 3›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 3›]
Edit training
| ‹further assessor 4›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 4›]
Edit training
| ‹further assessor 5›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 5›]
Edit training
| ‹further assessor 6›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 6›]
Edit training
| ‹further assessor 7›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 7›]
Edit training
| ‹further assessor 8›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 8›]
Edit training
| ‹further assessor 9›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 9›]
Edit training
| ‹further assessor 10›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 10›]
Edit training
| ‹further assessor 11›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 11›]
Edit training
| ‹further assessor 12›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 12›]
Edit training
| ‹further assessor 13›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 13›]
Edit training
| ‹further assessor 14›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 14›]
Edit training
| ‹further assessor 15›‹address›
| Paediatrics / Paediatrics
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹further assessor 15›]
Edit training
[aria-label: Pages of people]
Showing 1–20 of ‹30›
Rows per page
10
20
50
100
Previous
Next

######## BOARD R3-T12-loading-1280.dc.html — T12 · loading: the subtitle without the name (S9) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Everyone at your institution who holds the Assessor role, with the assessor training recorded for each.
Loading the assessors.

######## BOARD R3-T12-loading-390.dc.html — T12 · loading: the subtitle without the name (S9) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Everyone at your institution who holds the Assessor role, with the assessor training recorded for each.
Loading the assessors.

######## BOARD R3-T13-load-error-1280.dc.html — T13 · a failed read: the subtitle without the name (S9) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Illustration
Everyone at your institution who holds the Assessor role, with the assessor training recorded for each.
Could not load the assessors. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-T13-load-error-390.dc.html — T13 · a failed read: the subtitle without the name (S9) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## Assessors
Illustration
Everyone at your institution who holds the Assessor role, with the assessor training recorded for each.
Could not load the assessors. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-T14-admin-view-1280.dc.html — T14 · the Administrator by address at 2.15: six holders, assessor@wombat.local among them (S14) · Illustration · 1280
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
## Assessors
Illustration
Everyone who holds the Assessor role, at every institution, with the assessor training recorded for each.
### Assessors at every institution 6
Every institution, by last name: each holder of the Assessor role and their training. The bracketed row’s order not shown.
| Name
| Institution
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatma Khumalokhumalo@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatma Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training
| Thandi Zuluzulu@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
| Paediatrics / Paediatrics
| Trained
| 2018-10-20
[aria-label: Edit training for Thandi Zulu]
Edit training
| ‹Demo Institution assessor›assessor@wombat.local
| Demo Institution
| ‹…›
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹Demo Institution assessor›]
Edit training

######## BOARD R3-T14-admin-view-390.dc.html — T14 · the Administrator by address at 2.15: six holders, assessor@wombat.local among them (S14) · Illustration · 390
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
## Assessors
Illustration
Everyone who holds the Assessor role, at every institution, with the assessor training recorded for each.
### Assessors at every institution 6
Every institution, by last name: each holder of the Assessor role and their training. The bracketed row’s order not shown.
| Name
| Institution
| Scope
| Training status
| Training completed
| Actions
| Sarah Bothabotha@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
| Paediatrics / Paediatrics
| Trained
| Not recorded
[aria-label: Edit training for Sarah Botha]
Edit training
| Fatma Khumalokhumalo@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
| Paediatrics / Paediatrics
| Provisional (awaiting sign-off)
| 2026-09-20
[aria-label: Edit training for Fatma Khumalo]
Edit training
| David Naidoonaidoo@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
| Paediatrics / Paediatrics
| Trained
| 2020-10-20
[aria-label: Edit training for David Naidoo]
Edit training
| Mohammed Patelpatel@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
| Paediatrics / Paediatrics
| In training
| Not recorded
[aria-label: Edit training for Mohammed Patel]
Edit training
| Thandi Zuluzulu@kgk.wombat.local
| Kgosi Kgari Teaching Hospital
| Paediatrics / Paediatrics
| Trained
| 2018-10-20
[aria-label: Edit training for Thandi Zulu]
Edit training
| ‹Demo Institution assessor›assessor@wombat.local
| Demo Institution
| ‹…›
| ‹status›
| ‹date or Not recorded›
[aria-label: Edit training for ‹Demo Institution assessor›]
Edit training

######## BOARD R3-I01-123-kgk-1280.dc.html — I01 · 1.23 KGK’s record: paeds-admin@; Status a dl.form-group read-out (M8) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Your institution's name, short code and contact email.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
[value: KGK]
Contact email
[value: paeds-admin@kgk.wombat.local]
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I01-123-kgk-390.dc.html — I01 · 1.23 KGK’s record: paeds-admin@; Status a dl.form-group read-out (M8) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Your institution's name, short code and contact email.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
[value: KGK]
Contact email
[value: paeds-admin@kgk.wombat.local]
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I02-a63-saved-1280.dc.html — I02 · A.6.3 saved from paediatrics@ (A.6.1’s) to hod.paediatrics@: the result focused · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Your institution's name, short code and contact email.
Kgosi Kgari Teaching Hospital saved. Its contact email is now hod.paediatrics@kgk.wombat.local.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
[value: KGK]
Contact email
[value: hod.paediatrics@kgk.wombat.local]
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I02-a63-saved-390.dc.html — I02 · A.6.3 saved from paediatrics@ (A.6.1’s) to hod.paediatrics@: the result focused · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Your institution's name, short code and contact email.
Kgosi Kgari Teaching Hospital saved. Its contact email is now hod.paediatrics@kgk.wombat.local.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
[value: KGK]
Contact email
[value: hod.paediatrics@kgk.wombat.local]
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I03-short-emptied-1280.dc.html — I03 · Short code emptied: refused under it, focused; no summary (S22) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Illustration
Your institution's name, short code and contact email.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
Type a short code. The record was not saved.
Contact email
[value: paeds-admin@kgk.wombat.local]
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I03-short-emptied-390.dc.html — I03 · Short code emptied: refused under it, focused; no summary (S22) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Illustration
Your institution's name, short code and contact email.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
Type a short code. The record was not saved.
Contact email
[value: paeds-admin@kgk.wombat.local]
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I04-mail-malformed-1280.dc.html — I04 · a malformed contact email: refused under it, what was typed kept · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Illustration
Your institution's name, short code and contact email.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
[value: KGK]
Contact email
[value: ‹a malformed address›]
Enter a valid email address, for example name@example.org.
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I04-mail-malformed-390.dc.html — I04 · a malformed contact email: refused under it, what was typed kept · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Illustration
Your institution's name, short code and contact email.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
[value: KGK]
Contact email
[value: ‹a malformed address›]
Enter a valid email address, for example name@example.org.
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I05-loading-1280.dc.html — I05 · loading: h1 and tab “Institution” until the name loads (S9) · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Institution
## Institution
Your institution's name, short code and contact email.
Loading the institution.

######## BOARD R3-I05-loading-390.dc.html — I05 · loading: h1 and tab “Institution” until the name loads (S9) · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Institution
## Institution
Your institution's name, short code and contact email.
Loading the institution.

######## BOARD R3-I06-load-error-1280.dc.html — I06 · a failed read: no form; h1 and tab “Institution” (S9) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Institution
## Institution
Illustration
Your institution's name, short code and contact email.
Could not load the institution. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-I06-load-error-390.dc.html — I06 · a failed read: no form; h1 and tab “Institution” (S9) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Institution
## Institution
Illustration
Your institution's name, short code and contact email.
Could not load the institution. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-I07-mail-cleared-1280.dc.html — I07 · the contact email cleared and saved: the result says so · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Illustration
Your institution's name, short code and contact email.
Kgosi Kgari Teaching Hospital saved. It has no contact email now.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
[value: KGK]
Contact email
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I07-mail-cleared-390.dc.html — I07 · the contact email cleared and saved: the result says so · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Illustration
Your institution's name, short code and contact email.
Kgosi Kgari Teaching Hospital saved. It has no contact email now.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
[value: KGK]
Contact email
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I08-duplicate-1280.dc.html — I08 · a short code another institution holds: refused on its field · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Illustration
Your institution's name, short code and contact email.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
[value: ‹a short code another institution holds›]
An institution with the same name or short code already exists.
Contact email
[value: paeds-admin@kgk.wombat.local]
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-I08-duplicate-390.dc.html — I08 · a short code another institution holds: refused on its field · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
[aria-label: Breadcrumb]
- Home
- ›
- Kgosi Kgari Teaching Hospital
## Kgosi Kgari Teaching Hospital
Illustration
Your institution's name, short code and contact email.
[aria-label: Kgosi Kgari Teaching Hospital]
* marks a field you must fill in.
Name *
[value: Kgosi Kgari Teaching Hospital]
Short code *
[value: ‹a short code another institution holds›]
An institution with the same name or short code already exists.
Contact email
[value: paeds-admin@kgk.wombat.local]
Status
Active
Set by a Wombat administrator.
Cancel
Save changes

######## BOARD R3-O01-a31-her-none-1280.dc.html — O01 · A.3.1 no provider: in her terms · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
Prerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
### Institutional sign-in is not set up
Institutional sign-in is not set up for Kgosi Kgari Teaching Hospital. A Wombat administrator sets it up; ask them if your hospital signs staff in through its own system.
Until then, your people sign in with an email and a password, and there is nothing to change on this page.

######## BOARD R3-O01-a31-her-none-390.dc.html — O01 · A.3.1 no provider: in her terms · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
Prerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
### Institutional sign-in is not set up
Institutional sign-in is not set up for Kgosi Kgari Teaching Hospital. A Wombat administrator sets it up; ask them if your hospital signs staff in through its own system.
Until then, your people sign in with an email and a password, and there is nothing to change on this page.

######## BOARD R3-O02-admin-none-1280.dc.html — O02 · the Administrator, no provider: the setting’s name kept · 1280
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
## SSO mappings
Prerequisite: after T288 and T333
Map external identity provider groups to Wombat roles. Changes take effect on the next login for each user.
No SSO providers are configured. Add providers to the Sso:Providers section in application settings and restart.

######## BOARD R3-O02-admin-none-390.dc.html — O02 · the Administrator, no provider: the setting’s name kept · 390
Wombat
Acting as Administrator
MenuClose
[aria-label: Main]
Acting as Administrator
- Home
Platform
- Scheduled jobs
- Audit log
- SSO mappings
- Data rights requests
Organisations
- Institutions
- Colleges
People
- Users
- Invitations
Catalogue
- EPAs
- Curricula
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- My data rights
Demo Administrator
Sign out
## SSO mappings
Prerequisite: after T288 and T333
Map external identity provider groups to Wombat roles. Changes take effect on the next login for each user.
No SSO providers are configured. Add providers to the Sso:Providers section in application settings and restart.

######## BOARD R3-O03-left-mappings-1280.dc.html — O03 · mappings whose provider is gone: read only; Delete is the Administrator’s alone (S5) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
### Institutional sign-in is not set up
Institutional sign-in is not set up for Kgosi Kgari Teaching Hospital. A Wombat administrator sets it up; ask them if your hospital signs staff in through its own system.
Until then, your people sign in with an email and a password, and there is nothing to change on this page.
### Mappings left from a removed provider 1
Read only. These name a provider no longer set up for Kgosi Kgari Teaching Hospital, so they give no new role; roles they already gave remain. Only a Wombat administrator can delete them.
| External group
| Provider
| Wombat role and scope
| Actions
| ‹group display name›‹group ID›
| ‹removed provider›
| Committee memberPaediatrics
Read only: its provider is no longer set up.

######## BOARD R3-O03-left-mappings-390.dc.html — O03 · mappings whose provider is gone: read only; Delete is the Administrator’s alone (S5) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
### Institutional sign-in is not set up
Institutional sign-in is not set up for Kgosi Kgari Teaching Hospital. A Wombat administrator sets it up; ask them if your hospital signs staff in through its own system.
Until then, your people sign in with an email and a password, and there is nothing to change on this page.
### Mappings left from a removed provider 1
Read only. These name a provider no longer set up for Kgosi Kgari Teaching Hospital, so they give no new role; roles they already gave remain. Only a Wombat administrator can delete them.
| External group
| Provider
| Wombat role and scope
| Actions
| ‹group display name›‹group ID›
| ‹removed provider›
| Committee memberPaediatrics
Read only: its provider is no longer set up.

######## BOARD R3-O04-provider-form-1280.dc.html — O04 · a provider for KGK: Provider and Institution as read-outs (M8); Speciality and Sub-speciality only once a role that needs them is chosen (S4) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
### Add a mapping
* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.
Provider
‹provider›
The one provider set up for Kgosi Kgari Teaching Hospital.
External group ID *
Group display name *
Wombat role *
Choose a role
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.
Institution
Kgosi Kgari Teaching Hospital
Add mapping
### Current mappings 0
No group gives a role yet. Use Add a mapping to give one.

######## BOARD R3-O04-provider-form-390.dc.html — O04 · a provider for KGK: Provider and Institution as read-outs (M8); Speciality and Sub-speciality only once a role that needs them is chosen (S4) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
### Add a mapping
* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.
Provider
‹provider›
The one provider set up for Kgosi Kgari Teaching Hospital.
External group ID *
Group display name *
Wombat role *
Choose a role
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.
Institution
Kgosi Kgari Teaching Hospital
Add mapping
### Current mappings 0
No group gives a role yet. Use Add a mapping to give one.

######## BOARD R3-O04b-assessor-chosen-1280.dc.html — O04b · Assessor chosen: Speciality and Sub-speciality appear, as read-outs · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
### Add a mapping
* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.
Provider
‹provider›
The one provider set up for Kgosi Kgari Teaching Hospital.
External group ID *
[value: ‹group ID›]
Group display name *
[value: ‹group display name›]
Wombat role *
Choose a role
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.
Institution
Kgosi Kgari Teaching Hospital
Speciality
Paediatrics
Sub-speciality
Paediatrics
The only ones Kgosi Kgari Teaching Hospital has adopted.
Add mapping
### Current mappings 0
No group gives a role yet. Use Add a mapping to give one.

######## BOARD R3-O04b-assessor-chosen-390.dc.html — O04b · Assessor chosen: Speciality and Sub-speciality appear, as read-outs · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
### Add a mapping
* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.
Provider
‹provider›
The one provider set up for Kgosi Kgari Teaching Hospital.
External group ID *
[value: ‹group ID›]
Group display name *
[value: ‹group display name›]
Wombat role *
Choose a role
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.
Institution
Kgosi Kgari Teaching Hospital
Speciality
Paediatrics
Sub-speciality
Paediatrics
The only ones Kgosi Kgari Teaching Hospital has adopted.
Add mapping
### Current mappings 0
No group gives a role yet. Use Add a mapping to give one.

######## BOARD R3-O05-added-1280.dc.html — O05 · a mapping added: the result focused, the row listed · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
Mapping added. People in ‹group display name› get Assessor in Paediatrics / Paediatrics at their next sign-in.
### Add a mapping
* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.
Provider
‹provider›
The one provider set up for Kgosi Kgari Teaching Hospital.
External group ID *
Group display name *
Wombat role *
Choose a role
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.
Institution
Kgosi Kgari Teaching Hospital
Add mapping
### Current mappings 1
At Kgosi Kgari Teaching Hospital. Each applies at a person’s next sign-in.
| External group
| Provider
| Wombat role and scope
| Actions
| ‹group display name›‹group ID›
| ‹provider›
| AssessorPaediatrics / Paediatrics
[aria-label: Delete the mapping of ‹group display name› to Assessor]
Delete…

######## BOARD R3-O05-added-390.dc.html — O05 · a mapping added: the result focused, the row listed · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
Mapping added. People in ‹group display name› get Assessor in Paediatrics / Paediatrics at their next sign-in.
### Add a mapping
* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.
Provider
‹provider›
The one provider set up for Kgosi Kgari Teaching Hospital.
External group ID *
Group display name *
Wombat role *
Choose a role
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.
Institution
Kgosi Kgari Teaching Hospital
Add mapping
### Current mappings 1
At Kgosi Kgari Teaching Hospital. Each applies at a person’s next sign-in.
| External group
| Provider
| Wombat role and scope
| Actions
| ‹group display name›‹group ID›
| ‹provider›
| AssessorPaediatrics / Paediatrics
[aria-label: Delete the mapping of ‹group display name› to Assessor]
Delete…

######## BOARD R3-O06-refused-ia-1280.dc.html — O06 · handler refusal (a posted role the select does not offer): Institutional admin, refused beside Wombat role (S4) · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
### Add a mapping
* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.
Provider
‹provider›
The one provider set up for Kgosi Kgari Teaching Hospital.
External group ID *
[value: ‹group ID›]
Group display name *
[value: ‹group display name›]
Wombat role *
Choose a role
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Only a Wombat administrator can map a group to Institutional admin.
Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.
Institution
Kgosi Kgari Teaching Hospital
Add mapping
### Current mappings 1
At Kgosi Kgari Teaching Hospital. Each applies at a person’s next sign-in.
| External group
| Provider
| Wombat role and scope
| Actions
| ‹group display name›‹group ID›
| ‹provider›
| AssessorPaediatrics / Paediatrics
[aria-label: Delete the mapping of ‹group display name› to Assessor]
Delete…

######## BOARD R3-O06-refused-ia-390.dc.html — O06 · handler refusal (a posted role the select does not offer): Institutional admin, refused beside Wombat role (S4) · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
### Add a mapping
* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.
Provider
‹provider›
The one provider set up for Kgosi Kgari Teaching Hospital.
External group ID *
[value: ‹group ID›]
Group display name *
[value: ‹group display name›]
Wombat role *
Choose a role
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Only a Wombat administrator can map a group to Institutional admin.
Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.
Institution
Kgosi Kgari Teaching Hospital
Add mapping
### Current mappings 1
At Kgosi Kgari Teaching Hospital. Each applies at a person’s next sign-in.
| External group
| Provider
| Wombat role and scope
| Actions
| ‹group display name›‹group ID›
| ‹provider›
| AssessorPaediatrics / Paediatrics
[aria-label: Delete the mapping of ‹group display name› to Assessor]
Delete…

######## BOARD R3-O07-delete-dialog-1280.dc.html — O07 · Delete…: F8’s words; the danger button names its mapping · Illustration · 1280
IllustrationPrerequisite: after T288 and T333
### Delete this mapping?
Delete the mapping of ‹group display name› to Assessor (Paediatrics / Paediatrics)?
From each person's next sign-in, belonging to ‹group display name› no longer gives Assessor.
The scope it gave stays.
Keep the mapping
Delete the ‹group display name› mapping

######## BOARD R3-O07-delete-dialog-390.dc.html — O07 · Delete…: F8’s words; the danger button names its mapping · Illustration · 390
IllustrationPrerequisite: after T288 and T333
### Delete this mapping?
Delete the mapping of ‹group display name› to Assessor (Paediatrics / Paediatrics)?
From each person's next sign-in, belonging to ‹group display name› no longer gives Assessor.
The scope it gave stays.
Keep the mapping
Delete the ‹group display name› mapping

######## BOARD R3-O08-deleted-1280.dc.html — O08 · deleted: the result focused, the list empty · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
Mapping deleted. From each person's next sign-in, ‹group display name› no longer gives Assessor.
### Add a mapping
* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.
Provider
‹provider›
The one provider set up for Kgosi Kgari Teaching Hospital.
External group ID *
Group display name *
Wombat role *
Choose a role
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.
Institution
Kgosi Kgari Teaching Hospital
Add mapping
### Current mappings 0
No group gives a role yet. Use Add a mapping to give one.

######## BOARD R3-O08-deleted-390.dc.html — O08 · deleted: the result focused, the list empty · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
Mapping deleted. From each person's next sign-in, ‹group display name› no longer gives Assessor.
### Add a mapping
* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen.
Provider
‹provider›
The one provider set up for Kgosi Kgari Teaching Hospital.
External group ID *
Group display name *
Wombat role *
Choose a role
Speciality admin
Sub-speciality admin
Coordinator
Committee member
Assessor
Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted.
Institution
Kgosi Kgari Teaching Hospital
Add mapping
### Current mappings 0
No group gives a role yet. Use Add a mapping to give one.

######## BOARD R3-O09-loading-1280.dc.html — O09 · loading · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
Prerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
Loading the SSO mappings.

######## BOARD R3-O09-loading-390.dc.html — O09 · loading · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
Prerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
Loading the SSO mappings.

######## BOARD R3-O10-load-error-1280.dc.html — O10 · a failed read · Illustration · 1280
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
Could not load the SSO mappings. Nothing has changed. Try again, or come back in a few minutes.
Try again

######## BOARD R3-O10-load-error-390.dc.html — O10 · a failed read · Illustration · 390
Wombat
Acting as Institutional admin
MenuClose
[aria-label: Main]
Acting as Institutional admin
- Home
People
- Invitations
- Trainees
- Assessors
- Users
Curriculum
- Curriculum adoptions
- Curricula
- EPAs
- Activity types
- Entrustment scales
Reviews
- Decisions due
- Committee reviews
- Decision panels
- Entrustment decisions
Access and audit
- SSO mappings
- Audit log
- My data rights
Nolwazi Mbatha
Sign out
## SSO mappings
IllustrationPrerequisite: after T288 and T333
Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in.
Could not load the SSO mappings. Nothing has changed. Try again, or come back in a few minutes.
Try again
