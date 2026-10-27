# Review of a5596c52: the design system re-synced to flow 12

Read-only review against the code (Razor, `*Words.cs`/`*Text.cs`, `app.css` at HEAD). Method: byte compare of the bundle against `git show HEAD:...app.css`; a script that pulled every quoted string, bold lead, class and id out of the seven new READMEs and looked them up in `src/` and `app.css`, with each miss then read by hand; the Razor of UserDetail, UsersList, AssessorsList, AssessorTrainingSection, InstitutionEdit/Create, GroupMappings, InstitutionalAdminDashboard, ConfirmDialog read against the READMEs and previews.

Result: no must-fix. Two should-fix (overstated claims), four nits.

## Findings

1. **S, `design/system/components/Button/README.md` (the added "An action that is unavailable is never `disabled`" sentence) and `design/system/components/SsoMappings/README.md` (no Known gaps).** The claim is true of the last-way-in Remove…, but flow 12's own pages still use the native attribute on in-flight and busy controls, which the READMEs do not say. Flow 11's README records the same thing as "Kept by the build review (the race rule, R3)". Evidence:
   - `GroupMappings.razor:342` `disabled="@(_saving || (_deletingId is not null && _deletingId != mapping.Id))"` on every other row's Delete… while one runs.
   - `GroupMappings.razor:197` `disabled="@(_deletingId is not null)"` on Add mapping.
   - `InstitutionEdit.razor:121` `disabled="@Busy"` on Deactivate, and `:125` `disabled="@_deactivating"` on Save changes.
   - `ConfirmDialog.razor` Cancel is `disabled="@_confirming"` (already stated in that file's comment).
   Fix: in Button, end the sentence "…never `disabled` (the in-flight race rule still disables the other rows' Delete… on SSO mappings, Add mapping while a delete runs, and Deactivate/Save on her record while one runs)". Add a Known gaps line to SsoMappings and InstitutionRecord.

2. **S, `design/system/README.md` (index), the "Narrow widths" bullet: "every form's actions stack with the primary on top (`column-reverse`…)", and DESIGN.md's banner.** `InstitutionCreate.razor` (the Administrator's New institution, listed by the README as a flow 12 page, S12) is a plain `div.form-container` with `FormActions` Cancel then Save (`InstitutionCreate.razor:20-54`). It is in none of `.people-page`, `.user-page`, `.record-page`, so none of the `max-width: 640.98px` rules apply: no `column-reverse`, no 44px. The generic `.form-actions` has no phone rule outside the listed scopes (app.css column-reverse sites: `.change-password`, `.dialog-*`, `.committee-calendar`, `.dialog-card`, `.inv-page`, `.note-panel--form`, `.record-page`). Fix: add "except the Administrator's New institution form (T378's neighbour)" to the bullet, or list it under flow 12's known gaps; InstitutionRecord's README should also say `/new` is not under `.record-page`.

3. **N, `design/system/components/UserPage/preview.html` (the first two `<dialog>` blocks).** `aria-labelledby="pv-x"` and `"pv-y"` point at ids that do not exist (the h2s are `confirm-…-title`), and neither dialog has the real component's `aria-describedby`. The third dialog (`pv-lockw-title`) is right. The same dangling pattern is in earlier previews (ConfirmDialog, Invitations use `pv-*` that do exist, but ConfirmDialog/MyAccount etc. do not), so this is not new. Fix: point `aria-labelledby` at the h2's id and add `aria-describedby` to the `dialog-body` id.

4. **N, duplicate ids inside previews.** The 390 renditions repeat the first rendition's ids (`home-card-people`, `home-card-curriculum`, `home-card-institution` in InstitutionalAdminHome; `s-roles`, `s-sign`, `training-heading`, `add-role-*` in UserPage; `pf-q`, `pf-r`, `pf-s` in Users). `aria-labelledby`/`for` then resolve to the first copy. Preview-only; consistent with flow 11's `pv-active-heading-2` habit, which suffixed the second copy. Fix: suffix `-n` in the narrow copies.

5. **N, `src/Wombat.Application/Features/Assessors/AssessorTrainingWords.cs:54`.** The XML doc still says `"Training saved for Fatma Khumalo:"`. The re-sync corrected DESIGN.md's copy of the typo (the index README says "corrected") but not this source line; the cast's name is Fatima Khumalo. Fix: one-word edit in the code comment (not a design-system file).

6. **N, `design/system/components/Users/README.md`, "Narrow widths": "Manage 44px from 900px".** True (`.clinic-table--stack-wide .btn` at `max-width: 900px`, app.css:6233) but reads as "above 900px". Fix: "at 900px and below".

## Held (checked, found right)

**1. bundle.css**
- Lines 24 to 6796 equal `git show HEAD:src/Wombat.Web/wwwroot/app.css` from `body {` on (6771 lines) exactly, followed by two blank lines before the `/* ══ MainLayout.razor.css` banner (no diff after CR stripping).
- The `:root` extras block (lines 19 to 22) is closed; the bundle is not nested.
- The layout part (line 6797 to the end) is identical to the `a5596c52^` bundle's layout part.
- The header comment names flows 01 to 12 and `master@4566f133`.

**2. New READMEs, strings.** About 150 quoted strings checked by lookup. The only misses were example substitutions (Thandi Zulu, Fatima Khumalo, Kgosi Kgari Teaching Hospital) for `{name}` and `{institution}` placeholders, and prose phrases. Confirmed at the source:
- UsersListWords: HeadingCount (including "1 of 14 are locked out", "2 of 14 hold no role"), LiveCount, NoMatchTitle and NoMatchBody, Empty, the role order (Pending trainee before Trainee, Administrator and College admin first for the Administrator).
- UserPageText and UserAdministrationRules: NoRoleLead, AlreadyHeld, Narrowing, ScopeStays, TrainingKept, LockBody, LastWayIn, LockedNow, AdministratorNotLockable, OwnAccountNote, RoleHelp, TraineeRoleNotOffered, WaitingLead and AndMore, WaitingUnread, SignsInOnlyThrough.
- AssessorTrainingWords: the statuses and their order, TypeQualifications, StatusHelp, CompletedOnHelp, OwnTraining, NotAnAssessor, SavedLead/SavedWhat.
- AssessorsText: EmptyTitle and EmptyBody, NoRecordDate "—", Not recorded.
- SsoMappingWords: every sentence, including DeleteTitle, ScopeStays, GoneRowNote, the refusal words and "Only a Wombat administrator can map…".
- InstitutionEdit: RecordSubtitle, LoadFailed, "Set by a Wombat administrator."
- Validator words for the short code and email.
- InstitutionalAdminHomeWords: the waiting line (WaitingLead and WaitingRest), Accounts and "1 account: yours.", NothingAdopted, OpenAdoptions, the adopted counts' labels, NoContactEmail.
- DashboardFrame "Could not load your Home."

**2b. Classes and ids.** Every class and id in backticks exists in `app.css` or the Razor (`#users-trainee-note`, `#user-own-account-note`, `#remove-role-{role}` via `Slug`, `#add-role-toggle`, `#add-role-panel`, `#lock-out-trigger`, `#lockout-effects`, `#training-*`, `#home-card-*`, `#home-waiting-line`, `#people-heading`). Source paths exist (Users, UserPage, AssessorTraining, Assessors, SsoMappings, InstitutionRecord, InstitutionalAdminHome, `PeopleLinks.cs`, `PersonScopeWords.cs`, `ListUsersQuery.cs`, `GetRequestsWaitingForUser…`).

**2c. Behavioural claims, read in the Razor**
- Focus: the heading after Show, Clear and a page turn on Users; the first radio when Add a role… opens and when `?section=training` opens the panel; Cancel returns to the trigger; Try again focuses the heading, Name (record) or the trigger (training); the dialogs' `ReturnFocusId`; the safe button first.
- Rows and panels: `aria-controls` only while the panel exists; `aria-disabled` plus `.is-unavailable` and `aria-describedby` on the last way in; one result at a time (`_pageResultVersion`); the old route `AssessorProfileEdit` redirects (owner row at `NavOwners.cs:167`).
- Pager and sizes: the pager only when `MatchCount > PageSize` (Users) or `TotalCount > PageSize` (Assessors); `DefaultPageSize` 20 and sizes 10/20/50/100.
- 390 rules: `.user-page .form-actions`, `.note-panel--form .form-actions` and `.record-page .form-actions` are `column-reverse` with Cancel first in the DOM. The Remove… role row stacks. Radio labels are 2.75rem (user page only). `.people-page .btn`, `.form-control` and `.form-select` are 2.75rem. The filter's buttons use `.filter-actions` and its phone rule. `.dashboard-grid a.dashboard-metric` is 44px and the footers wrap.

**3. Previews**
- Every class used resolves in `bundle.css` apart from the previews' own `pv-cap`, `pv-narrow` and `pv-stage`. Each preview defines the ones it uses; `pv-stage` appears only in the two previews that draw a dialog.
- No raw hex colour anywhere in the seven previews (style blocks or markup).
- Head and structure match flow 11's Invitations preview: `@dsCard` comment, charset, own `<style>`, no links (the system injects the bundle).
- Markup matches the Razor: `header-container`, `dl.detail-list[aria-label=Account]`, `ul.role-rows`, `.actions-cell`, the note panel with `choice-help-stack`, the Sign-in `.section-part`s, and `dialog` with `.form-container`, `.dialog-body`, `.form-actions`.
- Dialog button order has the safe button first in the DOM in all four dialogs (Remove, two Lock outs, Delete).
- The Users role filter's "none" value, the `status=active|locked` values and the Home card markup match.
- The Home preview omits `DashboardFrame`'s header, which `RoleDashboard` draws; the preview shows it.

**4. Amended shared READMEs**
- DataTable, PagerControls and FilterBar are true: the StackWide uses on Users, Assessors and GroupMappings (one `MappingTable` for both lists), the `.col-wrap` floor of 7rem, the `overflow-wrap`, "People" as the list title.
- Badge: `status-locked` has `gap: var(--space-xs)`, and Icon's default size is 16 (14 on Users).
- DashboardCard, FormField, DetailCard and PageHeader are true (`OwnerHref` exists; the `.home-institution-list` markup and CSS match).
- NavMenu counts, recomputed from `NavItems.cs`: Institutional admin 15 plus Home equals 16, Administrator 16, and Entrustment decisions in her Reviews group (`NavItems.cs:155`). The owner rows for InstitutionEdit, InstitutionCreate (Administrator only), UserDetail and AssessorProfileEdit match `NavOwners.cs:146-167`.
- StatePanel: the five pages' fixed sentences, their focus targets and the "User unavailable" empty state with Go to Users.
- ConfirmDialog: the four dialogs' sentences and button names (`SsoMappingWords.DeleteKeep` and `DeleteConfirm`, `UserPageText.KeepTheRole` and `KeepAccess`, `RemoveSignIn`).
- Icon and Icons README: 64 icons, 59 named plus 5 unnamed, which equals `design-system.json`'s 64 `order` entries; each list's count matches its length.

**5. tokens.json**
- It parses.
- `git diff 4829f210 HEAD -- app.css` has one hunk (`@@ -6460,3 +6460,420 @@`, appended People admin sections); `:root` is not in the diff, so no token value should change, and none did (only `meta.ref`, `synced`, the notes and seven source-map rows).
- The source-map paths exist.

**6. README index and DESIGN.md**
- The index paragraph is true: commits `e53d4ae7`, `b8ed8fd6`, `964a2c4e` and `5f9bb9bb` exist; "five People admin sections" is the five `/* Flow 12: … */ … /* end Flow 12: … */` blocks; "328 steps, 283 with no gap" matches HANDOFF and STATE; the 44px list and the Icon sentence match.
- DESIGN.md's one-line change (Fatma to Fatima) is right; the banner already names flow 12 (lines 60-63).
- The stacked-row rule (primary on top at 390) holds for the user page, the training form, her record and the three dialogs; the exception is finding 2.

**7. Icons.** Every `Icon Name="…"` in the flow 12 Razor (arrow-right, building-2, key-round, lock, plus, search, trash, user-plus, plus PageHeader's variable icon and `DashboardCard Icon="users|book|building-2"`) is in `assetGroups.Icons.order`. Nothing is missing, and no upload is needed.
