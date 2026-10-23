# Flow 12 round 3: the check

Round 3 (canvas "Wombat 12", saved in `round-3/project/`: 178 files, 174 boards `R3-*.dc.html` on page `r3` of `canvas.json`, plus
`f12r3.css` and `f12r2.css`) was checked item by item against `round-3-ask.txt` and `round-2-review.md` (F1-F11, M1-M10, S1-S24,
nits, "What round 3's ask must do"), by a Sonnet agent for the main session, 2026-10-23. Method: every board's HTML with style,
script and svg stripped (class names, roles, aria attributes and text read); the Spec, Spec 2, Spec 3 and Steps read in full; the
P, U, T, H, I and O boards read at 1280 (the 390 and 768 twins checked by script and spot reads); `f12r3.css` read whole; `grep` over
all boards for `style=` (0), retired classes (0) and class names used against `app.css`; the built code (`PagerControls.razor`,
`app.css:2314` `.note-panel`, `app.css:5850` `.choice-stack`) opened where the ask cites it. **Not done: the rendered measurements.**
`ds/wombat/tokens.css` and `components/bundle.css` are not in `round-3/project/` (nor in round 2's), so the boards cannot be drawn
faithfully here; the Spec's "0 px sideways" tables and "88 boards, 0 controls under 44 px" are taken as reported, not re-measured.

## Verdict

Tally (rows below): **Done 78, Partly 1, Not done 1, Regressed 0**.

**Round 3 is final and fit to build from.** Every must-fix, should-fix and F decision is drawn or written on the Spec; no invented
person, address, role, scope, date, count or order; Fatma until 2.41 and Fatima from 2.44; KGK's contact email is the one in force at
each board (paeds-admin@ on I01 and H01-H04; hod.paediatrics@ on I02 and H05). Nothing left needs another round: every leftover is a
build note (see "Left"). The Spec says whether the captures arrived: "Not attached to this message", read from
`C:\dev\Wombat\design\upload`, all 17 named, seven opened this round, "nothing is drawn from memory".

## Left

1. **ds/ not saved** (nit "Save the canvas's ds/ with the round"). Not done. Housekeeping: copy `tokens.css` and
   `components/bundle.css` from the design system into `round-3/project/ds/wombat/`. Not a defect of the design.
2. **F9's guard.** Decided as "total > 10", yet "no pager on fifteen"; the boards and Spec 2 draw "renders it only when total exceeds one
   page" (page size 20). `PagerControls` defaults `PageSize = 20` and has a size select (10/20/50/100), so the right test is
   `TotalCount > PageSize`, not 10 (at size 10 a list of 15 needs it). Build note: guard on `TotalCount > PageSize`; say so in the handoff.
3. **Narrow-desktop word splits (carried from flow 11).** Users' Name cell (`th scope="row" class="col-wrap"`, name over address) is
   `overflow-wrap:anywhere`; flow 11's check found that splits addresses mid-word at 901-1280. Not measured here (no bundle.css).
   Build note: keep the `<wbr>` after `@`, and do not copy `.col-wrap` onto the row header unless replay at 901, 1000, 1100 and 390
   shows no mid-word break.
4. Nits, build or replay: P22 (SSO-only Assessor) draws no "Assessor training" section though the person holds Assessor (P20 does);
   P04/P29 carry "Already held by Thandi Zulu." and "The only speciality Kgosi Kgari Teaching Hospital has adopted.", which Spec 3's
   message table does not list; T05's `fieldset aria-invalid` (not a supported attribute on a group); Botha's no-date result has a
   Spec line but no board; the `dl.form-group` read-out rules must be written into `app.css`.

## F1-F11

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| F1 Add role after a sub-speciality | P28 (1280, 390); Spec-2 handler test | Done | P28 Patel "Committee member added to Mohammed Patel. Mohammed Patel's scope stays Paediatrics / Paediatrics."; no refusal; Spec-2 "Assessor first then Committee member (F1) passes". Illustration chip. |
| F2 Narrowing said before and after | P29 form, P30 result; P06 | Done | P29 `alert-info` "Pieter Smit's scope becomes Paediatrics, for every role he holds." (NEW), Paediatrics as `dd.readonly-value`; P30 repeats it, who line "Paediatrics"; P06 "scope stays Paediatrics / Paediatrics, for Committee member, the role still held (F2)". |
| F3 SSO-only Password part | P22 (1280, 390); P20, P21 | Done | P22 "signs in only through ‹provider›. A password cannot be set for them.", no reset field; Remove refused "The institutional sign-in is the account's only way in." with no hint; DATA pill "AllowLocalPassword, HasPassword". |
| F4 Drop SpecialityId/SubSpecialityId | Spec-2 build rules | Done | "F4 migration: drop AssessorProfiles.SpecialityId and SubSpecialityId; keep InstitutionId, written from the person at save." |
| F5 Prerequisites | Spec; pills on O01-O10, T03-T07 | Done | Spec "Prerequisites (F5, M4)" block; O pills "Prerequisite: after T288 and T333"; T03-T07 "Prerequisite: after T289"; "At HEAD today all six tasks are queued." |
| F6 Waiting-request query | P15 (N=1), P31 (N=2), P32 (N>5); Spec-2 | Done | P31 "2 requests are waiting for Mohammed Patel, the oldest since ‹...›", rows in brackets; P32 five rows then "and 3 more."; `GetRequestsWaitingForUserQuery(UserId, Principal)` and `WaitingRequestsDto` on Spec-2; "on open" DATA pills. |
| F7 Remove another person's sign-in | P20, P21, P33 (1280, 390) | Done | P21 "Remove the ‹provider› sign-in from Thandi Zulu?" buttons "Keep the sign-in" / "Remove sign-in"; P33 "The ‹provider› sign-in was removed from Thandi Zulu." focused (`action-result` tabindex -1, `role=status`); `RemoveUserInstitutionalSignInCommand` on Spec-2; Mbatha's own: Spec line, flow 02. |
| F8 Delete dialog's words | O07, O08; Spec-3 | Done | O07 "From each person's next sign-in, belonging to ‹group display name› no longer gives Assessor." + "The scope it gave stays."; old "anyone who had Assessor only from this group loses it" gone. |
| F9 Pager guard | U01, U04-U08 (none), U10, T11; Spec-2 | Partly | No pager on 10/15 and U10 "Showing 1-20 of ‹about 200›" with no "Page 1 of ‹10›". But Spec 1 says "Say if the guard is total > 10", Spec-2 writes "only when total exceeds one page": decision and drawing disagree. See Left 2 (build note). |
| F10 Entrustment decisions in her menu | every board of hers | Done | "Entrustment decisions" under Reviews on every H, U, P, T, I, O board of Mbatha (absent only on dialogs and Administrator boards: O02, P27, T14, U14); owner row + `ActiveNavItemTests` on Spec-3 DATA. |
| F11 Note panel | P03, P04, P08, P27, P29, T03, T05, T06 | Done | `div.detail-card.note-panel.note-panel--form`; `.btn-quiet` "Cancel" then primary; Spec-2 DESIGN.md "A form in a note panel leaves with Cancel (a quiet button), a note with Keep." `.disclosure-panel` on 0 boards. `.note-panel` exists at `app.css:2314`. |

## M1-M10

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| M1 Training split from scope | T03-T07; Spec-2 | Done | Command `(UserId, TrainingStatus, TrainingCompletedOn, Qualifications, Principal)`, "no UpdateScopeAsync; no security-stamp change", test "leaves UserSpecialityScopes and the security stamp unchanged"; T03-T07 ask only status, date, qualifications; "the training-only command M1 needs no T289, its refusals do". |
| M2 Add role's write | Spec-2 | Done | `AddRoleToUserCommand(UserId, Role, SpecialityId?, SubSpecialityId?, Principal)`, `InOneTransactionAsync { AddToRoleAsync; UpdateScopeAsync(held ∪ {...}) }`; tests union, rollback, Assessor-first; no-institution person: "Add role offers no institution-scoped role; nothing is written". |
| M3 P21-P22 as F3 | P20, P21, P22 | Done | Zulu (P20/P21) holds a password, Remove… allowed; P22 SSO-only words, no field, no "Reset a password first". |
| M4 Prerequisite pills | O01-O10, T03-T07 | Done | `anno` pills on all O boards and T03-T07 (both widths), not page content. |
| M5 Steps 2.15 | Steps | Done | "Botha Trained, Not recorded; Khumalo Provisional (awaiting sign-off), 2026-09-20; Naidoo Trained, 2020-10-20; Patel In training, Not recorded; Zulu Trained, 2018-10-20"; T08/T09 match. |
| M6 390 password field | every P and T at 390; Spec | Done | `@media (max-width:640.98px)` full width only on `.user-page .form-actions .btn` and `.section-part > .btn`; Spec table P03/P14/P20 input 214 x 44, Show 86 x 44. (Reported, not re-measured.) |
| M7 Home card wraps | H01-H05, H01b | Done | `.dashboard-grid .detail-list{grid-template-columns:minmax(0,max-content) minmax(0,1fr)}` + `dd{overflow-wrap:anywhere;min-width:0}`; 0 inline styles; H01b ‹a-sixty-four-character-contact-address-incard›@kgk.wombat.local, Illustration; Spec table 0 at 1280/1100/390. |
| M8 Read-outs | I01-I08, O04-O06, P04, P27, P29, training | Done | `dl.form-group > dt + dd.readonly-value` (P29 Speciality, I Status, O04 Provider/Institution); `.field-label`, `.status-readout` on 0 boards. |
| M9 Note panel, choice-group | P03-P05, P08, P27-P29, T03, T05, T06 | Done | Radios `fieldset.choice-group.choice-help-stack` of `.form-check`; `.choice-stack` on 0 boards and f12r3.css comment "the shipped .choice-stack ... untouched"; `.choice-stack` exists at `app.css:5850`. |
| M10 Disclosure wiring | same boards | Done | P03 trigger `aria-expanded="true" aria-controls="ar-panel"` and `#ar-panel` exists; P05 closed trigger has no `aria-controls`; `role="group"` on 0 panels; help `<p class="choice-help">` outside the `<label>`, `aria-describedby` kept. |

## S1-S24

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| S1 Scope cell rule | P34; Spec-2 | Done | Rule on Spec-2 ("Institutional admin → the institution's name; College admin → the College's; otherwise ..."), P34 "Paediatrics, ‹second speciality›" Illustration; "Add role does not refuse a second speciality. PROPOSED." U01 Mbatha "Kgosi Kgari Teaching Hospital", Smit "Any speciality". |
| S2 Three no-role states | U07, U09, P09, P12, P35; Spec-2 | Done | Ended: "Pieter du Plessis holds no role: they left the programme on 2026-10-18." / "... completed the programme on 2026-10-20."; none: P35 "John van Rensburg holds no role. They can sign in to My account and My data rights." (NEW, Illustration); Users cell sentence for ended only; DESIGN.md text on Spec-2. |
| S3 Remove dialog per role | P36 (1280, 390), P06 | Done | Six dialogs: Committee member (chair stall, T256), Speciality admin, Sub-speciality admin, Coordinator, Institutional admin (‹another Institutional admin›), last role "will hold no role. ..."; P06 "stay with them; they do not move to anyone else". |
| S4 SSO form | O04, O04b, O06 | Done | O06 title "handler refusal (a posted role the select does not offer)"; O04 "Speciality and Sub-speciality appear once a role that needs them is chosen", O04b shows them; Group display name starred. |
| S5 Gone-provider mappings | O03; Spec-2 | Done | "Read only ... Only a Wombat administrator can delete them."; no Delete; build rule S5 (handler check, test, ProviderGone, roles remain). |
| S6 Users filter | U02b, U14; Spec-2 | Done | U02b "thandi zulu" "1 of 10 match" + hidden count; hint "A name in either order, part of one, or an address."; Role select Administrator/College admin on U14 only; query-string and allow-listed return in Spec-2. |
| S7 One order | Spec-2 | Done | "ORDER BY LastName.ToLower(), FirstName.ToLower(), Id ... test with “van Rensburg” and “du Plessis”". |
| S8 Trainee notes, no subtitle | U13, P26 | Done | P26 h1 "User", only "You hold the Trainee role, so you cannot view or change user accounts, including your own."; no subtitle. |
| S9 Institution name while loading | I05, I06, U11, U12, T12, T13 | Done | `<title>Institution · Wombat`, h1 "Institution"; subtitles "Everyone with an account at your institution: ..." (without the name, said on Spec). |
| S10 Redirect | Spec-2 | Done | "?id= is the PROFILE id: profile → UserId → /admin/users/{UserId}?section=training"; unknown/out-of-scope/missing → User unavailable; unknown section opens nothing; absence sentence NEW. |
| S11 T10 | T10 | Done | Four rows, no "not listed" note. |
| S12 InstitutionCreate | Spec-2 | Done | "InstitutionCreate policy split off (Administrator only) ... /admin/institutions/new is access denied for her". |
| S13 Person's institution's set | P04, P27; Spec-2 | Done | P27 "Adopted by Demo Institution, this person's institution (S13)"; `GetAdoptedDisciplinesForInstitutionQuery`. |
| S14 U14, T14 | U14 (1280, 768, 390), T14 (1280, 390) | Done | Both Illustration; U14 18 rows, 5 ‹Demo account› + collegeadmin bracketed; T14 six holders, ‹Demo Institution assessor›; steps named (2.12, 2.15). |
| S15 Steps board | Steps | Done | See Steps section. |
| S16 The seven captures | Spec | Done | Seven listed with what each shows (P02, I01, P12, H05, U02, U11, P17 "hold"). |
| S17 Radio label target | P03, P04, T06 at 390 | Done | `.choice-help-stack .form-check label{min-height:2.75rem;display:flex;align-items:center}` below 641; Spec "88 boards ... 0 found". |
| S18 No inline style | all | Done | `style=` in 0 of 174 boards (scripted); `.user-section .actions-cell`, `.dashboard-card-footer .field-help` rules in f12r3.css. |
| S19 System pieces | P06, P15, P31, P32; every P; H01-H05 | Done | Waiting request `div.alert.alert-warning` with no `role`; who line `dl.detail-list aria-label="Account"`; Home counts `a.dashboard-metric` in `.dashboard-metric-row`; `.waiting-request`, `.who-line`, `.card-link`, `.count-rows` on 0 boards. |
| S20 Stacked-table roles | every stacked table | Done | `role="table"`, `rowgroup`, `row`, `columnheader`, `th scope="row" role="rowheader"`, `td role="cell"`. |
| S21 Filter card | every U board | Done | `form.detail-card.people-filter role="search" aria-label="Filter people"`; heading `tabindex="-1"`; `p.visually-hidden role="status"` "1 of 10 people match “zulu”." |
| S22 DESIGN.md text | Spec-2 | Done | Note panel, one-scope exemptions, read-out, destructive trigger (":734 ... supersedes :3442"), waiting line (flow 06 precedent), single-field refusal and ValidationSummary removed. |
| S23 Not live regions | H04, O03, P01, P26, U13 | Done | No `role`/`aria-live` on those notices; results only `role=status`/`alert` (P05, P07, P10, ...). |
| S24 Class table | Spec-2 | Done | Row for each NEW/gone class; retired (`.choice-stack` ours, `.captures`, `.old`, `.disclosure-panel`, `.status-readout`, ...) on 0 boards; every class used is NEW in the table or in `app.css` (scripted). |

## Steps board

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| 2.15 row by row | Steps | Done | As M5. |
| 5.28 off the list; no-role sentence first at A.4.5 | Steps | Done | Crossing list: 1.10, 1.21, 2.28, 5.17, 5.10, 2.37/3.52, A.5.15, A.6.1; A.4.5 "the no-role sentence first appears here". |
| A.6.4 Today/Expect | Steps | Done | Today "four bullets, the trainee line among them, and no dialog (T264)"; Expect "Both go: ... two lines for Patel ... 1 request is waiting ... since 2026-10-12 12:35 SAST". |
| A.5.15, 5.10, A.6.1 crossing | Steps | Done | "A.6.1: unchanged; the Administrator's page is flow 18's." |
| 2.14 Note retired, 2.37/3.52, scope at 2.13 | Steps | Done | 2.14 Today "stays on /admin/assessors/edit (the ?id= move was never built); “The UserId field is required.” ..."; 2.13 "the scope arrives at 2.13". |
| 2.28 "open Home" | Steps | Done | "2.28: ... Home's waiting line (its Do gains “open Home”)". |
| A.6.3 field | Steps | Done | "the field starts at paediatrics@kgk.wombat.local (A.6.1's save) ... (I01's paeds-admin@ is 1.23's.)" |
| 2.12 scope | Steps | Done | "each person's one scope as invited (U01)". |
| A.1.14 | Steps | Done | "New: the No role cell and its sentence." |
| F10 / F8 wording | Steps | Done | "Wherever an Expect quotes her menu: Entrustment decisions under Reviews (F10). The Delete step's Expect, if one quotes it: F8's words." |
| Journey counts recomputed | Steps | Done | Table "Journeys, recomputed after S6, F11 and F9" at 1280 and 390 (e.g. "Adding a role ... 3 + 0; abandoning it: 2 + 0 (Cancel in the note panel)"). |

## Nits and ALSO

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| RoleHelp for a scoped person | P03, P04, P28 | Done | "Works across the institution, kept to Thandi Zulu's scope: Paediatrics."; no "Choose a speciality only to keep them to it." on Zulu's boards (kept for unscoped Smit, correct). |
| Lockout list by role | P15, P18; Spec | Done | P15 Patel 2 lines, P18 Dlamini trainee + reminders; Spec counts each (Zulu 3, no role 1). |
| Home counts / locked read | H01-H05; Spec | Done | DATA pills; "the locked-account read is dropped". |
| "Locked out" vs Identity lockout | Spec | Done | "administrative lock (UserDeactivation.IsDeactivated) only ... PROPOSED." |
| ListAllUsersAsync N+1 | Spec | Done | One line on Spec. |
| Training small text | Spec | Done | "Not started stays a status you can record ... “Not recorded” ... no record"; Botha's "Training saved for Sarah Botha: Trained, no date." (text only, no board). |
| Three undrawn cases | I07, I08; Spec | Done | I07 "It has no contact email now."; I08 "An institution with the same name or short code already exists." on the field; Mbatha's own sign-in: flow 02 line. |
| Q9 deletions list | Spec | Done | "the card, RevokePendingInvitationsForEmailCommand, UserDetailDto.PendingInvitations, UserPendingInvitationDto, the read in GetUserByIdQuery, the CSS state, and their tests". |
| T11/U10 placeholders | T11, U10 | Done | Captions "The bracketed rows' order not shown." |
| I04 address | I04 | Done | `value="‹a malformed address›"`. |
| Families row note | Spec | Done | "The Demo Institution's id is 1." |
| 390 `.anno` | I, O | Done | `@media (max-width:640.98px){.anno{white-space:normal}}`. |
| Breadcrumb Home exception | Spec | Done | "(39 x 24) is the accepted inline exception, as in flows 03-11." |
| O07 danger label | O07 | Done | "Delete the ‹group display name› mapping". |
| Save ds/ | project folder | Not done | No `ds/` folder; every board links `ds/wombat/tokens.css` and `components/bundle.css`, which are not saved. |
| Primary on top at 390 | all 390 forms and dialogs | Done | `column-reverse` on `.dialog-card`, `.note-panel--form`, `.user-page`, `.record-page .form-actions`; DOM is secondary first (P06 "Keep the role" then "Remove Assessor"), so the primary is on top. |
| Form gap named | Spec-2 | Done | "Each form's parts, and what holds them": `div.note-panel.note-panel--form`, `div.section-part`, `form.form-container`, `form.detail-card.people-filter`; `.note-panel` is a flex column. |
| Re-measure tables, 44 px | Spec | Done | Seven-width table for 30 boards, 0 px; "88 boards ... 0 found". Reported, not re-measured here. |
| No password drawn; no impossible states | P03, P10 | Done | `input#pw type=password value=""`, P10 invalid and empty; reset field absent on P22. |

## General requirements of the ask

| Item | Board(s) | Status | Evidence |
|---|---|---|---|
| Round 3 page, R3- prefix, others untouched | `canvas.json` | Done | Pages r1, r2, r3; boards on r3 carry R3-. |
| Spec first says whether captures arrived | R3-0-spec | Done | "Not attached to this message. I read all 17 from the copies made from C:\dev\Wombat\design\upload ... nothing is drawn from memory". |
| Every F/M/S item with where drawn | R3-0-spec | Done | "F1-F11, M1-M10, S1-S24" table with Where column. |
| Illustration label in title and chip | all | Done | Spec list; `anno--ill` and "Illustration" in titles (checked U03, U10, U12-U14, P06-P08, P19-P36, T01, T10, T11, T13, T14, I03, I04, I06-I08, O03-O08, O10, H01b). |
| DATA table restated | Spec-3 | Done | "DATA the build must add": every item the ask lists, incl. "Pending invitations: nothing, removed." |
