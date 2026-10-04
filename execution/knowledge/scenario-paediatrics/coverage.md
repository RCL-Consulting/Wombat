# Coverage: pages, endpoints and journeys

This file maps the runbook onto the product. It answers three questions:
- **Which steps play each page?** Every routable page file is listed with its `@page` templates, who may open it, and
  the steps whose `Route:` line names it.
- **What is not played, and why?** Pages, flows and states no step can reach, each with its reason.
- **What does each role do?** Every job each role does in the story, with its steps and pages. This index is what the
  GUI redesign is briefed from, and replaying a redesigned flow's steps is its acceptance check.

T294's test (`tests/Wombat.Web.Tests/Scenario/`) enforces the first two sections against the code:
- every `@page` template is named by a step's `Route:` line, or excused under `## Not played`;
- no `## Not played` row excuses a template a step plays;
- `## Pages` names every template, and only real ones.

The other sections are an index, kept by hand. The step lists below were generated from the `Route:` lines of the seven
act and appendix files on 2026-09-26: 324 steps, 68 page files, 80 templates, all 80 played. Step 6.14a (T300) was
added by hand afterwards, so the files now hold 325. Of the 11 endpoints Wombat.Web maps outside the router, steps name
9. T335 (flow 01, 2026-09-27) deleted the placeholder page and its template, so 67 page files and 79 templates remain.
T355 (flow 05, 2026-10-04) added one EPA's page under My progress, `/portfolio/progress/{EpaId:int}`, so 68 page
files and 80 templates. Its build closes, for its replay to confirm, five gaps the steps record: F-2.39a (T306, the
training year on Home, 2.39), T280 at 3.12 (no card is one link around its rows), F-A.7.3a (T328, the standing table's
rating links 44 px at 390, A.7.3), F-6.37a (T304, counts carried at a version move, 6.37, already closed by T304's own
replay) and F-A.7.6a (T323, each chart drawn at its own size and scrolled in its named region, A.7.6).

## Pages

"Who may open it" is the page's own authorisation attribute. A handler behind the page may narrow it further, by
institution, speciality or the record's own people; where that decides what a person meets, the cell says so.

| Templates | Page | Who may open it | Steps |
|---|---|---|---|
| `/` | Home.razor | Any signed-in user | 1.1, 1.8, 1.10, 1.11, 1.21, 2.1, 2.8, 2.9, 2.10, 2.18, 2.19, 2.23, 2.25, 2.27, 2.31, 2.32, 2.33, 2.34, 2.35, 2.36, 2.37, 2.38, 2.39, 2.40, 2.44, 3.1, 3.12, 3.13, 3.15, 3.16, 3.17, 3.24, 3.26, 3.28, 3.30, 3.33, 3.34, 3.50, 3.51, 3.52, 3.53, 3.54, 4.1, 4.2, 4.3, 4.4, 4.35, 4.39, 4.42, 5.7, 5.8, 5.15, 5.21, 5.25, 5.28, 6.10, 6.38, A.1.1, A.2.1, A.4.1, A.4.6, A.4.7, A.5.3, A.5.10, A.6.1, A.6.2, A.6.8, A.7.3, A.7.4, A.7.5, A.7.6, A.7.7, A.7.8, A.7.9, A.7.10, A.7.11, A.7.14 |
| `/access-denied` | AccessDenied.razor | Anyone, signed in or not | 1.15, 2.19, 2.32, 4.5, 4.41, 5.22, 6.10, A.5.1, A.5.2, A.6.3 |
| `/account/change-password` | Account/ChangePassword.razor | Any signed-in user | A.4.2, A.4.6, A.7.14 |
| `/account/data-rights` | Profile/DataRights.razor | Any signed-in user | A.1.1, A.1.2, A.1.5, A.1.6, A.1.7, A.1.9, A.1.11, A.2.3, A.2.5, A.7.3 |
| `/account/forgot-password` | Account/ForgotPassword.razor | Anyone, signed in or not | A.4.4, A.7.12 |
| `/account/link-external` | Account/LinkExternalLogin.razor | Anyone, signed in or not | A.3.3 |
| `/account/login` | Account/Login.razor | Anyone not signed in; a signed-in visitor is sent Home | 1.1, 2.1, 2.8, 2.9, 2.10, 2.18, 2.23, 2.25, 2.27, 2.31, 2.32, 2.33, 2.34, 2.35, 2.36, 2.37, 2.39, 2.40, 4.3, 4.4, 5.15, 5.19, 5.20, 5.28, 6.10, A.1.13, A.3.2, A.3.3, A.4.3, A.4.4, A.4.6, A.4.7, A.5.4, A.6.5, A.6.8, A.7.12, A.7.14 |
| `/account/logout`, `/account/logout-confirm` | Account/Logout.razor | Any signed-in user; anyone else is sent to the sign-in page. A GET to either address draws the Sign out page and signs nobody out; its form posts to `/account/logout/submit` | A.4.7 |
| `/account/profile` | Account/Profile.razor | Any signed-in user | 2.19, 2.41, A.4.1, A.4.2, A.4.6, A.7.14 |
| `/account/register` | Account/Register.razor | Anyone, signed in or not | 1.8, 1.9, 1.10, 2.8, 2.9, 2.10, 2.11, 2.17, 2.18, 2.27 |
| `/activities/inbox` | Activities/ActivityInbox.razor | Any signed-in user | 3.4, 3.5, 3.11, 3.13, 3.15, 3.16, 3.17, 3.24, 3.26, 3.28, 3.33, 3.51, 5.25, 6.18, A.4.3, A.7.2 |
| `/activities/mine` | Activities/MyActivities.razor | Any signed-in user | 2.19, 3.2, 3.6, 3.20, 3.25, 3.48, 5.28, 6.19, 6.24, 6.41, A.2.7, A.7.3 |
| `/activities/new` | Activities/NewActivity.razor | Any signed-in user | 2.19, 2.42, 2.43, 3.1, 3.8, 3.9, 3.10, 3.12, 3.14, 3.18, 3.19, 3.20, 3.21, 3.22, 3.23, 3.25, 3.27, 3.29, 5.22, 5.24, 6.16, 6.19, 6.20, 6.27, 6.37, A.2.7, A.6.6, A.7.1 |
| `/activities/{ActivityId:int}` | Activities/ActivityView.razor | Any signed-in user; an activity opens only to its subject, its author, the people it names and their overseers, and reads "Activity unavailable" to anyone else | 3.2, 3.3, 3.4, 3.5, 3.6, 3.10, 3.11, 3.12, 3.13, 3.14, 3.15, 3.16, 3.17, 3.18, 3.19, 3.20, 3.21, 3.22, 3.23, 3.24, 3.25, 3.26, 3.27, 3.28, 3.29, 3.33, 4.17, 5.24, 5.25, 6.16, 6.18, 6.19, 6.24, A.2.7, A.5.6, A.7.1, A.7.2 |
| `/admin/activity-types` | Admin/ActivityTypes/ActivityTypesList.razor | Administrator, CollegeAdmin, InstitutionalAdmin; each row offers Edit where the caller may write the type and View elsewhere, and New activity type only to a caller with a scope to create in (T300) | 1.24, 1.26, 1.31, 6.14a |
| `/admin/activity-types/new`, `/admin/activity-types/{ActivityTypeId:int}` | Admin/ActivityTypes/ActivityTypeEdit.razor | Administrator, CollegeAdmin, InstitutionalAdmin; a type the caller may not write opens read-only, and Scope offers only the scopes the caller may write: an InstitutionalAdmin her institution, a CollegeAdmin his College's specialities and sub-specialities (T300, D52) | 1.25, 1.26, 1.27, 1.28, 1.29, 1.30, 6.14a, A.7.9, A.7.14 |
| `/admin/adoptions` | Admin/Adoptions/AdoptionsList.razor | Administrator, InstitutionalAdmin | 1.20, 6.32 |
| `/admin/assessors` | Admin/Assessors/AssessorsList.razor | Administrator, InstitutionalAdmin | 2.14, 2.15, 2.44 |
| `/admin/assessors/edit` | Admin/Assessors/AssessorProfileEdit.razor | Administrator, InstitutionalAdmin | 2.14 |
| `/admin/audit` | Admin/Audit/AuditList.razor | Administrator, InstitutionalAdmin | 3.55, 3.56, 3.57, 4.38 |
| `/admin/audit/{Id:guid}` | Admin/Audit/AuditDetail.razor | Administrator, InstitutionalAdmin | 3.56, 3.57, 4.38 |
| `/admin/colleges` | Admin/Colleges/CollegesList.razor | Administrator | 1.2, 1.3, 6.1, 6.2, 6.3 |
| `/admin/colleges/new`, `/admin/colleges/{Id:int}` | Admin/Colleges/CollegeEdit.razor | Administrator | 1.3, 6.1, 6.2, 6.10 |
| `/admin/colleges/{CollegeId:int}/specialities` | Admin/Institutions/SpecialitiesList.razor | Administrator, CollegeAdmin | 1.12, 6.3, 6.11 |
| `/admin/colleges/{CollegeId:int}/specialities/new`, `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` | Admin/Institutions/SpecialityEdit.razor | Administrator, CollegeAdmin | 6.3, 6.4, 6.11, 6.12 |
| `/admin/curricula` | Admin/Curricula/CurriculaList.razor | Administrator, CollegeAdmin, InstitutionalAdmin | 1.17, 1.19, 1.21, 6.13, 6.17, 6.22, 6.26, 6.29, 6.30, 6.31, 6.33, A.7.10 |
| `/admin/curricula/new`, `/admin/curricula/{Id:int}` | Admin/Curricula/CurriculumEdit.razor | Administrator, CollegeAdmin | 6.13, 6.29, 6.31 |
| `/admin/curricula/{Id:int}/items` | Admin/Curricula/CurriculumItemsEdit.razor | Administrator, CollegeAdmin, InstitutionalAdmin | 1.18, 1.21, 6.13, 6.17, 6.22, 6.26, 6.29, 6.30, 6.33, A.7.10 |
| `/admin/curriculum-progress` | Admin/CurriculumProgress/CurriculumProgressRebuild.razor | Administrator | 6.38 |
| `/admin/data-rights` | Admin/DataRights/RequestsList.razor | Administrator, Coordinator; a Coordinator sees his own institution's requests | A.1.3, A.1.8, A.1.12, A.7.5 |
| `/admin/data-rights/{Id:guid}` | Admin/DataRights/RequestDetail.razor | Administrator, Coordinator; a Coordinator decides his own institution's requests | A.1.3, A.1.8, A.1.12, A.5.7 |
| `/admin/entrustment-decisions` | Admin/EntrustmentDecisions/Index.razor | Administrator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin | 4.35, 4.36, 4.37, 5.7, 6.22, A.7.7 |
| `/admin/entrustment-scales` | Admin/EntrustmentScales/EntrustmentScalesList.razor | Administrator, InstitutionalAdmin | 1.4, 1.15, 1.22, 6.5, 6.6, 6.8, 6.9 |
| `/admin/entrustment-scales/new`, `/admin/entrustment-scales/{Id:int}` | Admin/EntrustmentScales/EntrustmentScaleEdit.razor | Administrator | 1.4, 6.5, 6.6, 6.10 |
| `/admin/epas` | Admin/Epas/EpasList.razor | Administrator, CollegeAdmin, InstitutionalAdmin | 1.16, 1.19, 1.21, 6.14, 6.17, 6.22, 6.23, 6.25, 6.28, A.7.10 |
| `/admin/epas/new`, `/admin/epas/{Id:int}` | Admin/Epas/EpaEdit.razor | Administrator, CollegeAdmin, InstitutionalAdmin | 6.14, 6.17, 6.23, 6.25, 6.28 |
| `/admin/institutions` | Admin/Institutions/InstitutionsList.razor | Administrator | 1.6, A.5.2, A.6.1, A.7.11 |
| `/admin/institutions/new`, `/admin/institutions/{Id:int}` | Admin/Institutions/InstitutionEdit.razor | Administrator, InstitutionalAdmin; only an Administrator creates or deactivates one, and an InstitutionalAdmin edits her own | 1.6, 1.23, A.5.5, A.6.1, A.6.3 |
| `/admin/invitations` | Admin/Invitations/InvitationsList.razor | Administrator, InstitutionalAdmin | 1.5, 1.7, 1.11, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.12, 2.16, 2.17, 2.26, 2.28, 2.32 |
| `/admin/jobs` | Admin/Jobs/ScheduledJobsList.razor | Administrator | 3.32, A.2.1, A.2.2, A.2.4, A.2.6, A.2.8, A.2.9, A.2.10, A.5.1, A.5.2, A.7.11 |
| `/admin/jobs/runs` | Admin/Jobs/ScheduledJobRunsList.razor | Administrator | A.2.10, A.7.11 |
| `/admin/specialities` | Admin/Institutions/MySpecialitiesRedirect.razor | Administrator, CollegeAdmin | 1.12, 6.11 |
| `/admin/specialities/{SpecialityId:int}/sub-specialities` | Admin/Institutions/SubSpecialitiesList.razor | Administrator, CollegeAdmin | 1.13, 6.4, 6.12 |
| `/admin/specialities/{SpecialityId:int}/sub-specialities/new`, `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` | Admin/Institutions/SubSpecialityEdit.razor | Administrator, CollegeAdmin | 1.14, 6.4, 6.7, 6.12 |
| `/admin/sso/group-mappings` | Admin/Sso/GroupMappings.razor | Administrator, InstitutionalAdmin | A.3.1 |
| `/admin/trainees` | Admin/Trainees/PendingTraineesList.razor | Administrator, InstitutionalAdmin | 2.28, 2.29, 2.30, 5.16, 5.17, 5.27, 6.34, 6.36, A.1.14, A.7.9 |
| `/admin/trainees/edit` | Admin/Trainees/TraineeProfileEdit.razor | Administrator, InstitutionalAdmin | 2.29, 2.30, 5.16, 5.17, 5.27, 6.34, 6.36, A.7.9 |
| `/admin/users` | Admin/Users/UsersList.razor | Administrator, InstitutionalAdmin | 2.12, 2.13, 2.28, 2.44, 5.10, A.1.14, A.4.5, A.5.1, A.6.4, A.6.9, A.7.9 |
| `/admin/users/{UserId}` | Admin/Users/UserDetail.razor | Administrator, InstitutionalAdmin | 2.12, 2.13, 2.28, 5.10, 5.17, A.4.5, A.5.5, A.6.4, A.6.7, A.6.9, A.7.9, A.7.14 |
| `/committee/decisions-due` | CommitteeDecisions/DecisionsDue.razor | Coordinator, Administrator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin | 4.1, 4.2, 4.3, 4.4, 4.5, 4.9, 4.10, 4.12, 4.34, 4.37, 5.1, 5.2, A.7.5, A.7.7, A.7.8 |
| `/committee/my-reviews` | CommitteeDecisions/MyReviews.razor | Trainee, Administrator | 4.41, 4.42, 4.43, 4.48, 4.51, 5.8, 5.22 |
| `/committee/panels` | CommitteeDecisions/PanelsList.razor | CommitteeMember, Coordinator, Administrator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin | 2.20, 2.23, 2.24, 2.25, 2.32, 2.35, 2.37, 4.13, 4.14, A.7.7 |
| `/committee/panels/new`, `/committee/panels/{PanelId:int}` | CommitteeDecisions/PanelEdit.razor | Administrator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin | 2.21, 2.22, 2.23, 2.25, 2.32, 4.13 |
| `/committee/reviews` | CommitteeDecisions/ReviewsSchedule.razor | Coordinator, Administrator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, CommitteeMember | 4.6, 4.7, 4.8, 4.9, 4.10, 4.11, 4.12, 4.15, 4.22, 4.26, 4.28, 4.30, 4.31, 4.32, 4.33, 4.44, 4.45, 4.49, 5.2, 5.3, 5.29, A.1.10, A.1.14, A.7.6, A.7.8, A.7.14 |
| `/committee/reviews/{ReviewId:int}` | CommitteeDecisions/ReviewDetail.razor | CommitteeMember, Coordinator, Administrator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin | 4.6, 4.7, 4.9, 4.10, 4.11, 4.15, 4.16, 4.17, 4.18, 4.19, 4.20, 4.21, 4.22, 4.23, 4.24, 4.25, 4.26, 4.27, 4.28, 4.29, 4.30, 4.31, 4.32, 4.33, 4.41, 4.44, 4.45, 4.46, 4.47, 4.49, 4.50, 5.2, 5.3, 5.4, 5.5, 5.6, A.1.10, A.1.14, A.7.6, A.7.13 |
| `/Error` | Error.razor | Anyone, signed in or not (static; T321) | A.5.8 |
| `/msf/campaigns` | MultiSourceFeedback/CampaignsList.razor | Coordinator, Administrator | 3.34, 3.38, 3.41, 3.46, 3.49, A.4.7, A.7.5 |
| `/msf/campaigns/new`, `/msf/campaigns/{CampaignId:int}` | MultiSourceFeedback/CampaignEdit.razor | Coordinator, Administrator | 3.34, 3.35, 3.36, 3.37, 3.38, 3.43, 3.44, A.7.5 |
| `/msf/coverage` | MultiSourceFeedback/ProgrammeCoverage.razor | Coordinator, Administrator | 3.49 |
| `/msf/my-reports`, `/msf/my-reports/{CampaignId:int}` | MultiSourceFeedback/MyMsfReports.razor | Trainee, Coordinator, Administrator; a trainee reads only her own released reports | 3.47, 5.22 |
| `/msf/reports/{CampaignId:int}` | MultiSourceFeedback/CampaignReport.razor | Coordinator, Administrator | 3.41, 3.44, 3.46 |
| `/msf/respond` | MultiSourceFeedback/MsfRespond.razor | Anyone, signed in or not (a static page) | 3.39, 3.40, 3.42, 3.45 |
| `/not-found` | NotFound.razor | Anyone, signed in or not | 1.23, A.5.4, A.5.5 |
| `/portfolio/authorisations` | Portfolio/MyAuthorisations.razor | Trainee | 4.39, 4.42, 5.8, 5.22 |
| `/portfolio/export`, `/portfolio/export/{TraineeUserId}` | Portfolio/ExportPortfolio.razor | Any signed-in user; the export admits a trainee to her own portfolio, and staff to a trainee they oversee | 5.9, 5.10, 5.12, 5.23 |
| `/portfolio/progress/{EpaId:int}` | Portfolio/EpaProgress.razor | Trainee, or a former trainee (trainee record); the caller's own curriculum's EPAs only, any other id "Page not found". Reached from My progress's index, Home's Furthest short and My authorisations rows, My activities' Credit links, a completed activity's Open My progress, and an ended record's EPA codes (T355) | 3.6, 3.7, 3.48, 5.26, 6.15, 6.19, 6.21, 6.24, 6.27, A.7.3 |
| `/portfolio/progress` | Portfolio/MyProgress.razor | Trainee, or a former trainee (trainee record) | 2.19, 2.39, 2.40, 3.7, 3.48, 4.40, 5.15, 5.19, 5.20, 5.26, 5.28, 6.15, 6.19, 6.21, 6.24, 6.27, 6.35, 6.37, 6.39, 6.40, 6.41, A.7.3, A.7.4 |
| `/portfolio/verify` | Portfolio/VerifyExport.razor | Anyone, signed in or not (a static page) | 5.13, 5.14, 5.23, A.7.12 |

## Not played

No template is unplayed. Every one of the 80 templates is named by at least one step's `Route:` line, so this section
has no rows. A row added here must name, in backticks, a template that no step plays, with its reason (T294).

## Flows and states not played

Pages that are played but carry flows or states no step reaches. Each is either not built, needs something the replay
environment lacks, or has no reason to happen in the story. Where a state can still be reached from a step's database
state, `states.md` is where it belongs.

| Flow or state | Why no step plays it | Where it belongs |
|---|---|---|
| Institutional sign-in against a provider: `/account/sso-challenge/{providerKey}` to the identity provider and back to `/account/sso-callback`. This covers provisioning by group mapping, an unmatched group landing as PendingTrainee, Administrator never granted by SSO, the refusal of a deactivated or other-institution account, and the verified-email rules. | Needs an identity provider; `Sso:Providers` is empty on dev. The invariants are unit-tested (`SsoGroupMapperTests`, `SsoGroupMappingCommandTests`, `SsoScopeGuardTests`). A.3.2 and A.3.3 play the no-provider states. | A replay against a test IdP |
| Linking an account on `/account/link-external` with an institutional sign-in in progress, and its post, `/account/link-external/submit` (password check, throttle, lockout) | Needs an identity provider to start an external sign-in. A.3.3 plays only the expired state. | A replay against a test IdP |
| Adding and deleting SSO group mappings on `/admin/sso/group-mappings`, and T288's cross-institution refusal | The add form appears only when a provider is configured. A.3.1 plays the read-only, empty page. | T288; a replay against a test IdP |
| Self-service password reset by emailed link | Not built, by design: `/account/forgot-password` says to ask a Wombat administrator (A.4.4), and nothing sends the `PasswordResetEmail` template. The administrator's reset stands in (A.4.5). | Not built |
| Removing an institutional sign-in on My account: the Remove dialog with and without a password, its post `/account/external-logins/remove`, its refusals (a wrong password, the throttle, the last way in) and the lock a fifth wrong password causes | Needs an account with a linked institutional sign-in, and so an identity provider. The card's results are reachable as typed codes (`states.md` § Typed codes). | A replay against a test IdP |
| Change password for an account that signs in only through its institution: the page's reason and Back to My account in place of the form; My account's How you sign in with no Password row | Needs an SSO-provisioned account (no Wombat password); every cast account has one. | A replay against a test IdP |
| The lock a fifth wrong current password on Change password causes: the session ends and the sign-in page gives the lock notice (T339, E2) | A.4.2 plays one wrong current password; five would lock Dr Khumalo for 15 minutes and end her other session before A.4.3. | `states.md`, on a scratch database |
| Change password or Remove for an account someone else has already locked: refused on the page with the wait, the session kept (T339 review) | No step locks an account from outside while its owner is signed in. | `states.md`, on a scratch database |
| Change password refused by the sign-in throttle ("Too many attempts from this network. …") | Needs ten failed password checks from one address within five minutes; no step fails that often. | `states.md` (typed, or on a scratch database) |
| The sign-in page with scripts blocked (no Show toggle; signing in still works), and a signed-in visitor opening `/account/login` (sent Home) | No step blocks scripts or opens the sign-in page while signed in. | `states.md` |
| `/Error` reached through an unhandled exception | Outside Development only (`ErrorPages`, T321); the replay runs in Development, whose developer exception page answers instead. `Hosting/ErrorPageFlowTests` plays it; A.5.8 plays the page by its address. | Reported at A.5.8 |
| The five features the nav once linked as "Coming soon": Recent activities (Assessor), Stalled activities (Coordinator), Programme trainees (CommitteeMember, SpecialityAdmin, SubSpecialityAdmin), STAR review queue (SpecialityAdmin, SubSpecialityAdmin) and System (Administrator) | Not built, and not offered: the nav links to no unbuilt page, and the placeholder page is gone (T335, flow 01). Recent activities and System were dropped; stalled work and Programme trainees are flow 06's, the STAR review queue flow 09's. The menus are read at 3.31, 3.51–3.53 and A.5.9–A.5.13. | Flows 06 and 09 |
| The Coordinator's stalled-work triage: sending a reminder, or reassigning a request | No page offers it. The dashboard's "Stalled requests" rows link to each activity's page (T297), which offers neither (3.30, A.5.10). | Not built |
| Applying and completing an approved data-rights rectification | No page calls `ApplyRectificationCommand` or `CompleteRectificationRequestCommand` (T112, Still open). A.1.8 rejects the request instead. | Not built |
| An email when an activity is requested, completed, declined or returned | Nothing sends the `AssessmentRequested`, `AssessmentAccepted`, `AssessmentCompleted` or `AssessmentDeclined` templates. Steps 3.3, 5.24, A.2.7 and A.7.2 expect that nothing is sent. | Undecided |
| An assessor's training status (`AssessorProfile.TrainingStatus`) changing an assessment | Nothing on the activity path reads it (2.14 records it). | Undecided |
| An invitation's Delivery reading "Sent", or "check the address" after a second failure | The replay logs mail (`Email__SmtpHost` unset), so nothing reports a delivery or a refusal. Rows read "Being sent", then "Not delivered." | `states.md`, with an SMTP sink |
| The Administrator adopting for an institution through the institution picker on `/admin/adoptions` | The story's InstitutionalAdmin adopts for her own institution (1.20, 6.32). | `states.md` |
| Discard draft in the activity-type builder | The story publishes KGK's draft (1.30) and keeps the College's unpublished (6.14a), discarding neither. | `states.md` |
| Deactivating a College, speciality or sub-speciality | Nothing in Application or Infrastructure reads their `IsActive`, no decision says what it should stop, and the button does not ask first (T264). | A decision first |
| Removing a curriculum item (its ConfirmDialog, T222) | Every item measures registrars still in training. | `states.md`, on a scratch curriculum |
| An InstitutionalAdmin deactivating and reactivating a local EPA | The story pauses a national EPA (6.17, 6.23); the local variant was browser-checked in T196. | `states.md` |
| Rebuilding one trainee's progress | `RebuildCurriculumProgressCommand` takes a trainee, but no page offers it; 6.38 rebuilds everyone. | Not built |
| Scale edits that refuse or warn: removing a rung a pinned curriculum item needs, and a rename that unbinds a form bound by name (T253) | Only the unused CNSA scale is edited (6.5, 6.6). | `states.md` |
| The College adding nationally an EPA that an institution already holds as its own item | Undecided (T242, queued). The story avoids it: KGK's item names KGK-001, and PAED-016 goes onto 11.2 only. | T242 |
| A re-submission after a return, on a type that credits | No seeded type both credits and returns. 3.14–3.17 play the return on a reflective exercise, where lateness is never recorded. | A seed that credits and returns |
| A seeded procedure-log instrument | The CPSA catalogue has none; the generic `procedure_log` belongs to the Demo speciality. KGK's teaching log stands in (3.18, 3.19). | The College |
| The Random Case Analysis, Chart-Stimulated Recall and Clinical Audit instruments, filed | Offered to a registrar (2.42) but never filed. | A later replay |
| A learner-feedback campaign (`learner_feedback_cpsa` on PAED-015) | Outside every act's brief. | A later replay |
| MSF patient respondents | `CreateMsfTemplateCommandValidator` requires patient responses to be off, so the Patient group is never offered (3.36). | By design |
| An MSF report blocked from release (short of responses or groups); `/msf/respond` refusing an expired, revoked or unrecognised link | The story's campaign is released. Only "already used" (3.40) and "request closed" (3.45) are played. | `states.md` |
| The MSF auto-close and expiry-reminder jobs acting on a campaign | The appendix runs both with no open campaign (A.2.9). | `states.md` |
| An entrustment-only review; a panel sitting as the neonatal CCC; "Decided by another panel"; the sitting-order warning (T131 slice 5) | KGK has no neonatal panel, so PAED-004 and 005 fall back to the general panel (2.24, 4.14). | `states.md`, with a second panel |
| Agenda lines "Decided elsewhere" and "no longer decided", Defer after recording, and Remove of a STAR that no longer fits (T235) | Each needs two sittings in one window, a STAR revoked mid-review, or a curriculum change between recording and ratifying. | `states.md` |
| Appeal outcome Dismissed | The story's one appeal is remitted (4.47); 4.45 reads the other outcome on the form. There is no Upheld (T307, D51). | `states.md` |
| The review page's notes for a chair who can no longer act (T256), and for a trainee now elsewhere (T182, T213) | Nobody in the story loses the role, moves or is deactivated mid-review. | `states.md` |
| The Administrator on Decisions due and on a review (no ratify or appeal bypass, D46) | The Administrator acts only in Act 1, Act 6 and the appendix. | `states.md` |
| A graduate filing an activity about herself | Undecided. 5.22 opens the page, which admits her, and files nothing. | A decision first |
| Verifying a STAR certificate | `/portfolio/verify` checks portfolio exports only; a certificate's hash is recorded nowhere. | Not built |
| A graduate appealing her final review | 5.8 reads the form; Act 4 plays an appeal. | — |
| Mark complete or Deactivate refused for a day before the programme started | Only the after-today refusal is played (5.16). | `states.md` |
| "Run now" refused while the same job is running | Depends on timing (`ScheduledJobDispatcher.AlreadyRunning`). | `states.md` |
| An erasure refused because the person changed at the same moment (`ErasureExecutor.PersonChanged`) | Needs a concurrent write. | `states.md` |
| `/health` reporting Unhealthy | Needs PostgreSQL stopped mid-replay. A.6.10 plays the healthy state. | `states.md` |
| A pending trainee's pages, and `/msf/respond`, at 390 px | No pending trainee or open campaign exists when the appendix runs. | `states.md`, from Act 2's and Act 3's states |

## Reached only by address

Pages a role is admitted to, played in the story, that its menu and dashboard do not link to. The redesign should decide
whether each deserves a link.

| Page | Who is admitted but has no link | Steps |
|---|---|---|
| `/portfolio/export/{TraineeUserId}` | Every member of staff who may export a trainee's portfolio: no page links to it | 5.10, 5.12 |
| `/admin/entrustment-decisions` | SpecialityAdmin and SubSpecialityAdmin; the InstitutionalAdmin reaches it only from her dashboard's quick links | 4.36, 4.37, A.7.7 |
| `/admin/institutions/{Id:int}` | InstitutionalAdmin, for her own institution | 1.23, A.6.3 |
| `/committee/panels` | Coordinator: the page admits him, but his menu has no Decision panels (DESIGN.md) | 2.32 |
| `/account/logout-confirm`, `/account/logout` | Everyone: only the error page's Sign out links to the confirmation address (on the replay no failure leads there, A.5.8), and nothing links to `/account/logout`; everywhere else the account row's Sign out posts to `/account/logout/submit` and signs out at once | A.4.7 |
| `/Error` | Everyone: on the replay (Development) no failure leads to it | A.5.8 |
| `/portfolio/authorisations` | Trainee: reached from Home's My authorisations card (its Open My authorisations; its STAR rows open the EPA page) and from an EPA page's Entrustment, not from the menu, which lights My progress on it (T355) | 4.39, 4.42, 5.8 |

## Endpoints

Routes Wombat.Web maps outside the router. A step names one in its `Route:` line where the visit is part of the journey.

| Endpoint | What it does | Steps |
|---|---|---|
| `/dashboard/switch/{role}` | Switches the acting role to another role the user holds, stored with the account, and says so once on the page it lands on (`?returnUrl=`, local only); a role not held writes nothing. The sidebar's "Switch to …" and "Change role" use it | 2.34, 3.5, 3.13, 3.15, 3.17, 3.26, 3.28, 3.33, 3.52, 4.5, 5.25, A.5.3 |
| `/account/login/submit` | The sign-in form's post | 5.15, 5.20, 5.28, 6.10 (every sign-in posts it) |
| `/account/logout/submit` | The sign-out forms' post (the account row's Sign out in the top bar and the phone menu's foot, and the Sign out page's): requires the antiforgery token, signs out, records a Logout, and lands on the sign-in page with "You have signed out." (T339) | 2.8, 2.9, 2.10, 2.18, 2.27, 2.34, A.4.7 |
| `/account/session-ended` | Where a tab goes, by a full page load, once its sign-in has ended (a role change, a completed programme, an erasure, a password change, a lock) | 2.31, 5.19, A.1.13, A.4.3, A.6.5 |
| `/account/data-rights/download/{id:guid}` | Downloads a completed data-rights export, to its data subject or a global Administrator | A.1.4, A.1.5 |
| `/account/sso-challenge/{providerKey}` | Starts an institutional sign-in | A.3.2 (no provider configured) |
| `/account/sso-callback` | Where the identity provider returns | A.3.3 (no sign-in in progress) |
| `/health` | The public health check, database included | A.6.10 |
| `/account/register/submit` | The registration form's post | Not named; every registration in 1.8, 1.10, 2.8–2.10, 2.18 and 2.27 posts it |
| `/account/change-password/submit` | The change-password form's post: a change lands on My account with "Password updated."; a refusal returns to the page; the fifth wrong current password locks the account and signs it out to the sign-in page (T339, E2); an account already locked by someone else is refused on the page with the wait, and stays signed in | Not named; A.4.2 and A.4.6 post it |
| `/account/profile/submit` | My account's name form's post: saves the name, issues the sign-in cookie again so the top bar names the person as saved, and returns to My account with "Name saved." or the refusal | 2.41, A.4.1, A.7.14 |
| `/account/external-logins/remove` | My account's Remove dialog's post: removes one institutional sign-in, asking for the password when the account has one, and never the last way in; a GET returns to My account (T339) | Not played: needs a linked institutional sign-in (see above) |
| `/account/link-external/submit` | Links an institutional identity to an account by its password | Not played: needs an identity provider (see above) |

## Journeys by role

Each job is phrased as the person's goal. Steps are in play order. Pages are the templates the steps visit, sign-in and
Home aside unless the job is about them. A person holding two roles (Dr Zulu, Dr Naidoo and Dr Botha are CommitteeMember
and Assessor) appears under each role for the jobs done in it.

### Administrator (the platform operator, `devadmin`)

| Job | Steps | Pages |
|---|---|---|
| See the platform at a glance: database health, how many users, the maintenance links | 1.1, 1.11, A.6.2 | `/account/login`, `/` |
| Confirm a College's record | 1.2, 1.3 | `/admin/colleges`, `/admin/colleges/{Id:int}` |
| Add a second national College, and correct a College's description | 6.1, 6.2 | `/admin/colleges`, `/admin/colleges/new`, `/admin/colleges/{Id:int}` |
| Give a College with no CollegeAdmin its first speciality and sub-speciality | 6.3, 6.4 | `/admin/colleges/{CollegeId:int}/specialities`, `/admin/colleges/{CollegeId:int}/specialities/new`, `/admin/colleges/{CollegeId:int}/specialities/{Id:int}`, `/admin/specialities/{SpecialityId:int}/sub-specialities`, `/admin/specialities/{SpecialityId:int}/sub-specialities/new`, `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` |
| Read an entrustment ladder's rungs in order | 1.4 | `/admin/entrustment-scales`, `/admin/entrustment-scales/{Id:int}` |
| Create an entrustment scale, and add a level to it | 6.5, 6.6 | `/admin/entrustment-scales`, `/admin/entrustment-scales/new`, `/admin/entrustment-scales/{Id:int}` |
| Make a scale a sub-speciality's default | 6.7 | `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` |
| Delete a scale, and be told what still uses it | 6.8 | `/admin/entrustment-scales` |
| Create an institution | 1.6 | `/admin/institutions`, `/admin/institutions/new`, `/admin/institutions/{Id:int}` |
| Keep an institution's record up to date | A.6.1 | `/admin/institutions`, `/admin/institutions/{Id:int}` |
| Invite the first person to run a College, and the first to run an institution | 1.5, 1.7, 1.11 | `/admin/invitations` |
| Remind assessors of work waiting on them, now | 3.32, A.2.8 | `/admin/jobs` |
| Run a scheduled job now and read what it did | A.2.4, A.2.6, A.2.9 | `/admin/jobs` |
| Pause a scheduled job and resume it | A.2.2 | `/admin/jobs` |
| See every scheduled job and its run history | A.2.1, A.2.10 | `/admin/jobs`, `/admin/jobs/runs` |
| Read the audit trail across institutions, payloads included | 3.57 | `/admin/audit`, `/admin/audit/{Id:guid}` |
| Rebuild every trainee's curriculum progress from their evidence | 6.38 | `/admin/curricula`, `/admin/curriculum-progress` |
| Approve a registrar's request to erase their data | A.1.12 | `/admin/data-rights`, `/admin/data-rights/{Id:guid}` |
| Look at my own account | A.6.9 | `/admin/users`, `/admin/users/{UserId}` |
| Find no System page: the menu offers nothing unbuilt | A.5.13 | `/` |
| Do the platform's work on a phone | A.7.11 | `/`, `/admin/jobs`, `/admin/jobs/runs`, `/admin/institutions` |

### CollegeAdmin (Dr Anton Kruger, CPSA)

| Job | Steps | Pages |
|---|---|---|
| Accept my invitation to run the College's catalogue | 1.8 | `/account/register`, `/` |
| Check my College's specialities, sub-specialities and their default ladder | 1.12, 1.13, 1.14 | `/admin/specialities`, `/admin/colleges/{CollegeId:int}/specialities`, `/admin/specialities/{SpecialityId:int}/sub-specialities`, `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` |
| Check the national EPAs | 1.16 | `/admin/epas` |
| Check a curriculum version and its items' targets | 1.17, 1.18 | `/admin/curricula`, `/admin/curricula/{Id:int}/items` |
| Find which pages are the Administrator's: the scales and the College's own record | 1.15, 6.10 | `/admin/entrustment-scales`, `/admin/colleges/{Id:int}`, `/admin/entrustment-scales/new`, `/access-denied` |
| Correct a speciality's description | 6.11 | `/admin/specialities`, `/admin/colleges/{CollegeId:int}/specialities`, `/admin/colleges/{CollegeId:int}/specialities/{Id:int}` |
| Add a sub-speciality, with its default ladder | 6.12 | `/admin/colleges/{CollegeId:int}/specialities/{Id:int}`, `/admin/specialities/{SpecialityId:int}/sub-specialities`, `/admin/specialities/{SpecialityId:int}/sub-specialities/new`, `/admin/specialities/{SpecialityId:int}/sub-specialities/{Id:int}` |
| Start a curriculum for a new sub-speciality and hold it back from adoption | 6.13 | `/admin/curricula`, `/admin/curricula/new`, `/admin/curricula/{Id:int}`, `/admin/curricula/{Id:int}/items` |
| Correct an EPA's wording | 6.14 | `/admin/epas`, `/admin/epas/{Id:int}` |
| Draft an activity type for a College discipline, and read the College's instruments as their author | 6.14a | `/admin/activity-types`, `/admin/activity-types/new`, `/admin/activity-types/{ActivityTypeId:int}` |
| Take an EPA out of use while the College revises it, then restore it | 6.17, 6.23 | `/admin/epas`, `/admin/epas/{Id:int}`, `/admin/curricula`, `/admin/curricula/{Id:int}/items` |
| Add a new national EPA | 6.28 | `/admin/epas`, `/admin/epas/new`, `/admin/epas/{Id:int}` |
| Publish a new curriculum version: clone the current one, change its items, release it | 6.29, 6.30, 6.31 | `/admin/curricula`, `/admin/curricula/{Id:int}`, `/admin/curricula/{Id:int}/items` |
| Keep the catalogue on a phone | A.7.10 | `/`, `/admin/epas`, `/admin/curricula`, `/admin/curricula/{Id:int}/items` |

### InstitutionalAdmin (Prof Nolwazi Mbatha, KGK)

| Job | Steps | Pages |
|---|---|---|
| Accept my invitation to run my institution | 1.10 | `/account/register`, `/` |
| Adopt the College's curriculum so that we can admit registrars | 1.19, 1.20, 1.21 | `/admin/curricula`, `/admin/epas`, `/admin/adoptions`, `/admin/curricula/{Id:int}/items`, `/` |
| Adopt a newer curriculum version | 6.32 | `/admin/adoptions` |
| Read the entrustment ladders | 1.22, 6.9 | `/admin/entrustment-scales` |
| Keep my institution's own record, and nobody else's | 1.23, A.5.5, A.6.3 | `/admin/institutions/{Id:int}`, `/not-found`, `/admin/users/{UserId}`, `/access-denied` |
| Check the College's assessment instruments | 1.24, 1.25 | `/admin/activity-types`, `/admin/activity-types/{ActivityTypeId:int}` |
| Build and publish an activity type of our own | 1.26, 1.27, 1.28, 1.29, 1.30, 1.31 | `/admin/activity-types`, `/admin/activity-types/new`, `/admin/activity-types/{ActivityTypeId:int}` |
| Invite staff, each with the right role and scope | 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7 | `/admin/invitations` |
| Invite registrars, replace a mistyped invitation, and resend one whose email was lost | 2.16, 2.17, 2.26 | `/admin/invitations`, `/account/register` |
| Review my institution's users, and my own account | 2.12, 2.28, 2.44 | `/admin/invitations`, `/admin/users`, `/admin/users/{UserId}`, `/` |
| Give a consultant a second role | 2.13 | `/admin/users`, `/admin/users/{UserId}` |
| Record each assessor's training status | 2.14, 2.15 | `/admin/assessors`, `/admin/assessors/edit` |
| Admit registrars to the curriculum, with their programme dates | 2.28, 2.29, 2.30 | `/admin/trainees`, `/admin/trainees/edit` |
| Form the review panel, and see who decides each EPA | 2.20, 2.21, 2.22, 2.24 | `/committee/panels`, `/committee/panels/new`, `/committee/panels/{PanelId:int}` |
| Change which College committee a panel sits as | 4.13 | `/committee/panels`, `/committee/panels/{PanelId:int}` |
| Read my institution's audit trail | 3.55, 3.56, 4.38 | `/admin/audit`, `/admin/audit/{Id:guid}` |
| See what the committee must decide this period, and what is scheduled | 4.2, 4.12, 4.34 | `/committee/decisions-due`, `/committee/reviews` |
| Read a decided review before the chair ratifies it | 4.26 | `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| Read the STARs the committee issued, and download a certificate | 4.35, 5.7 | `/`, `/admin/entrustment-decisions` |
| Export a registrar's portfolio | 5.10, 5.11 | `/admin/users`, `/admin/users/{UserId}`, `/portfolio/export/{TraineeUserId}` |
| Record a registrar's graduation | 5.16, 5.17 | `/admin/trainees`, `/admin/trainees/edit`, `/admin/users/{UserId}` |
| Record a registrar leaving the programme | 5.27 | `/admin/trainees`, `/admin/trainees/edit` |
| See an EPA the College has paused, and a graduate's STARs on it | 6.22 | `/admin/epas`, `/admin/curricula`, `/admin/curricula/{Id:int}/items`, `/admin/entrustment-decisions` |
| Add an EPA and a curriculum item of our own | 6.25, 6.26, 6.33 | `/admin/epas`, `/admin/epas/new`, `/admin/epas/{Id:int}`, `/admin/curricula`, `/admin/curricula/{Id:int}/items` |
| Move a registrar to a new curriculum version | 6.34, 6.36 | `/admin/trainees`, `/admin/trainees/edit` |
| See what a registrar's erasure left | A.1.14 | `/admin/users`, `/admin/trainees`, `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| Check our SSO group mappings | A.3.1 | `/admin/sso/group-mappings` |
| Reset a registrar's password | A.4.5 | `/admin/users`, `/admin/users/{UserId}` |
| Lock a consultant out, and let him back in | A.6.4, A.6.7 | `/admin/users`, `/admin/users/{UserId}` |
| Find the Administrator's pages closed to me | A.5.2 | `/admin/institutions`, `/admin/jobs`, `/access-denied` |
| Do my administration on a phone, and check its contrast | A.7.9, A.7.14 | `/`, `/admin/users`, `/admin/users/{UserId}`, `/admin/trainees`, `/admin/trainees/edit`, `/admin/activity-types/{ActivityTypeId:int}`, `/account/login`, `/account/profile`, `/account/profile/submit`, `/account/change-password`, `/committee/reviews` |

### SpecialityAdmin (Dr Refilwe Mokoena, Paediatrics)

| Job | Steps | Pages |
|---|---|---|
| Accept my invitation | 2.10 | `/account/register`, `/` |
| Add a consultant to my programme's review panel | 2.23 | `/committee/panels`, `/committee/panels/{PanelId:int}` |
| Read my programme's dashboard: trainees, work awaiting review, target coverage | 2.38, 3.53 | `/` |
| See what the committee must decide in my speciality | 4.3 | `/committee/decisions-due` |
| Schedule a registrar's review from what is due | 4.9 | `/committee/decisions-due`, `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| Revoke a STAR issued in error | 4.36 | `/admin/entrustment-decisions` |
| Find no Programme trainees or STAR review queue yet (flows 06 and 09) | 3.53, A.5.12 | `/` |
| Review my account | 2.41 | `/account/profile` |
| Do my programme's work on a phone | A.7.7 | `/`, `/committee/panels`, `/committee/decisions-due`, `/admin/entrustment-decisions` |

### SubSpecialityAdmin (Dr Kabelo Sithole, Paediatrics)

| Job | Steps | Pages |
|---|---|---|
| Accept my invitation | 2.10 | `/account/register`, `/` |
| See which panels I may create | 2.25 | `/committee/panels`, `/committee/panels/new` |
| Read my sub-speciality's dashboard | 2.38, 3.54 | `/` |
| See what the committee must decide in my sub-speciality | 4.4 | `/committee/decisions-due` |
| Schedule a registrar's review from what is due | 4.10 | `/committee/decisions-due`, `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| Find a revoked STAR and what must be decided again | 4.37 | `/admin/entrustment-decisions`, `/committee/decisions-due` |
| Review my account | 2.41 | `/account/profile` |
| Do my work on a phone | A.7.8 | `/`, `/committee/reviews`, `/committee/decisions-due` |

### Coordinator (Mr Pieter Smit, KGK)

| Job | Steps | Pages |
|---|---|---|
| Accept my invitation | 2.8, 2.11 | `/account/register`, `/`, `/account/logout/submit`, `/account/login` |
| Learn what my dashboard offers, and which pages are not mine | 2.32 | `/`, `/admin/invitations`, `/committee/panels`, `/committee/panels/new`, `/access-denied` |
| See which requests have stalled | 3.30, 3.31, A.5.10 | `/`, `/not-found` |
| Set up an MSF questionnaire and a campaign for a registrar | 3.34, 3.35 | `/msf/campaigns`, `/msf/campaigns/new`, `/msf/campaigns/{CampaignId:int}` |
| Invite the respondents, and fix one added under the wrong group | 3.36 | `/msf/campaigns/{CampaignId:int}` |
| Open a campaign, and resend a link that was not delivered | 3.37, 3.43 | `/msf/campaigns/{CampaignId:int}` |
| Withdraw a campaign started in error | 3.38 | `/msf/campaigns/new`, `/msf/campaigns/{CampaignId:int}`, `/msf/campaigns` |
| Read an MSF report, close the campaign and release it to the registrar | 3.41, 3.44, 3.46 | `/msf/campaigns`, `/msf/reports/{CampaignId:int}`, `/msf/campaigns/{CampaignId:int}` |
| See which registrars the programme's MSF has covered | 3.49 | `/msf/campaigns`, `/msf/coverage` |
| See what the committee must decide this period | 4.1, 5.1 | `/`, `/committee/decisions-due` |
| Schedule the annual reviews, and be stopped from a second for the same period | 4.6, 4.7, 4.8 | `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| Read the ratified schedule and a review's outcome | 4.33 | `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| Schedule a formative check-in | 4.49, A.1.10 | `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| Schedule a registrar's pre-graduation review | 5.2 | `/committee/decisions-due`, `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| Reproduce a registrar's portfolio export | 5.12 | `/portfolio/export/{TraineeUserId}` |
| Check that an ended programme cannot be put before the panel | 5.29 | `/committee/reviews` |
| Decide a registrar's data-rights request: approve an export, reject a correction | A.1.3, A.1.8, A.5.7 | `/admin/data-rights`, `/admin/data-rights/{Id:guid}` |
| Find that I cannot download a registrar's data export | A.1.4 | `/account/data-rights/download/{id:guid}` |
| Stop, then restart, my weekly digest email | A.2.3, A.2.5 | `/account/data-rights` |
| Sign out, and come back to the page I asked for | A.4.7 | `/account/logout-confirm`, `/`, `/account/logout`, `/account/logout/submit`, `/account/login`, `/msf/campaigns` |
| Review my account | 2.41 | `/account/profile` |
| Do my work on a phone | A.7.5 | `/`, `/msf/campaigns`, `/msf/campaigns/{CampaignId:int}`, `/committee/decisions-due`, `/admin/data-rights` |

### CommitteeMember (Dr Thandi Zulu, chair; Dr David Naidoo, Dr Sarah Botha; Dr John van Rensburg, external)

| Job | Steps | Pages |
|---|---|---|
| Accept my invitation | 2.10 | `/account/register`, `/` |
| See how the programme's registrars stand against this period's targets | 2.33, 2.35, 2.37, 3.52 | `/account/login`, `/`, `/committee/panels` |
| Switch between my committee and assessor dashboards, to rate and back | 2.34, 3.13, 3.33, 3.52 | `/`, `/dashboard/switch/{role}`, `/account/logout/submit`, `/account/login` |
| Check who sits on my panel and which EPAs it decides | 4.14 | `/committee/panels` |
| Find that scheduling is not mine | 4.5 | `/committee/decisions-due`, `/access-denied` |
| List my panel's reviews and open one | 4.11 | `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| (Chair) Read a registrar's evidence, standing and MSF before the sitting | 4.15, 5.3 | `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| (Chair) Start a review and stage STARs on its evidence | 4.16, 4.17, 4.18, 4.19, 4.20, 4.28, 5.3, 5.4 | `/committee/reviews/{ReviewId:int}`, `/activities/{ActivityId:int}` |
| (Chair) Defer what the committee will not decide, with a reason | 4.21, 4.28, 4.30, 4.31, 4.32 | `/committee/reviews/{ReviewId:int}` |
| (Chair) Record the committee's decision with a quorum present | 4.23, 4.24, 4.25, 4.29, 4.30, 4.31, 4.32, 5.5 | `/committee/reviews/{ReviewId:int}` |
| (Chair) Ratify the decision and issue the STARs | 4.27, 4.29, 4.30, 4.31, 4.32, 5.6 | `/committee/reviews/{ReviewId:int}` |
| (Member) Follow a sitting without the chair's controls | 4.22, 4.44 | `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| (External member) See the appeal I may hear | 4.45 | `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |
| (Chair) Resolve an appeal, with a quorum for the replacement decision | 4.46, 4.47 | `/committee/reviews/{ReviewId:int}` |
| (Chair) Hold and close a formative check-in | 4.50 | `/committee/reviews/{ReviewId:int}` |
| Find no Programme trainees yet (flow 06) | 3.52, A.5.11 | `/` |
| Review and correct my account | 2.41, A.4.1 | `/account/profile`, `/account/profile/submit` |
| Read a review on a phone, and with a screen reader | A.7.6, A.7.13 | `/`, `/committee/reviews`, `/committee/reviews/{ReviewId:int}` |

### Assessor (Dr Mohammed Patel, Dr Fatima Khumalo; and Dr Zulu, Dr Naidoo and Dr Botha as assessors)

| Job | Steps | Pages |
|---|---|---|
| Accept my invitation, getting the password rules right | 2.9, 2.10 | `/account/register`, `/`, `/account/logout/submit`, `/account/login` |
| See my empty assessor dashboard on first sign-in | 2.36 | `/account/login`, `/` |
| Rate a Mini-CEX a registrar sent me | 3.5, 3.13, 3.26, 3.33, 5.25 | `/dashboard/switch/{role}`, `/`, `/activities/inbox`, `/activities/{ActivityId:int}` |
| Rate the other WBAs sent to me: a DOPS, a CBD, a CCA, a direct observation | 3.24, 3.26, 3.28 | `/`, `/dashboard/switch/{role}`, `/activities/inbox`, `/activities/{ActivityId:int}` |
| Be kept out of a request that does not name me | 3.4 | `/activities/inbox`, `/activities/{ActivityId:int}` |
| Decline a request that is not mine to rate, saying why | 3.11 | `/activities/inbox`, `/activities/{ActivityId:int}` |
| Return a reflection for more detail, then record the discussion | 3.15, 3.17 | `/dashboard/switch/{role}`, `/`, `/activities/inbox`, `/activities/{ActivityId:int}` |
| Complete an assessment on an EPA the College has paused | 6.18 | `/activities/inbox`, `/activities/{ActivityId:int}` |
| Read my assessor dashboard: what waits for me and what I decided | 3.33, 3.51 | `/`, `/dashboard/switch/{role}`, `/activities/inbox` |
| Find no Recent activities: it was dropped | 3.51, A.5.9 | `/` |
| Change my password, and have my other session end | A.4.2, A.4.3 | `/account/profile`, `/account/change-password`, `/account/session-ended`, `/account/login`, `/activities/inbox` |
| Be locked out, and sign in again once reactivated | A.6.5, A.6.8 | `/account/session-ended`, `/account/login`, `/` |
| Complete an assessment on my phone | A.7.2 | `/activities/inbox`, `/activities/{ActivityId:int}` |
| Review my account | 2.41 | `/account/profile` |

### Trainee (Dr Molefe to Step 5.17; Dr Dlamini, Dr du Plessis, Dr Mahlangu and Dr Ndlovu)

| Job | Steps | Pages |
|---|---|---|
| Sign in again as a Trainee once I am admitted | 2.31 | `/`, `/account/session-ended`, `/account/login` |
| See my targets, training year and minimum levels | 2.39, 2.40 | `/account/login`, `/`, `/portfolio/progress` |
| See which instruments I may file, on which EPAs, and whom I may name | 2.42, 2.43, 3.23 | `/activities/new` |
| Review my account | 2.41 | `/account/profile` |
| Ask a consultant for a Mini-CEX: save a draft, then submit it | 3.1, 3.2, 3.3 | `/`, `/activities/new`, `/activities/mine`, `/activities/{ActivityId:int}` |
| Ask for the other WBAs: a CBD, a DOPS, a CCA, a direct observation | 3.21, 3.23, 3.25, 3.27, 3.29 | `/activities/new`, `/activities/{ActivityId:int}`, `/activities/mine` |
| File an encounter on the right date: a future day and a day before my programme are refused, and a late filing is warned about | 3.8, 3.9, 3.10 | `/activities/new`, `/activities/{ActivityId:int}` |
| File an encounter again after the assessor declined it | 3.12 | `/`, `/activities/{ActivityId:int}`, `/activities/new` |
| Submit a reflection, and submit it again when it is returned | 3.14, 3.16 | `/activities/new`, `/`, `/activities/inbox`, `/activities/{ActivityId:int}` |
| Log a teaching session I gave | 3.18, 3.19 | `/activities/new`, `/activities/{ActivityId:int}` |
| Ask for a portfolio review | 3.22 | `/activities/new`, `/activities/{ActivityId:int}` |
| Abandon a draft started on the wrong instrument | 3.20 | `/activities/new`, `/activities/{ActivityId:int}`, `/activities/mine` |
| Follow my activities and what each credited | 3.6, 3.48 | `/activities/mine`, `/activities/{ActivityId:int}`, `/portfolio/progress/{EpaId:int}` |
| See my progress against this period's targets, and each EPA's own page: its count, level, STAR, chart and activities | 3.7, 3.48, 5.15, 5.26 | `/portfolio/progress`, `/portfolio/progress/{EpaId:int}` |
| Read my dashboard | 3.50 | `/` |
| Read my released MSF report | 3.47 | `/msf/my-reports`, `/msf/my-reports/{CampaignId:int}` |
| Read my STARs and download a certificate | 4.39, 4.42, 5.8 | `/`, `/portfolio/authorisations` |
| Read my standing against the College's exit levels | 4.40 | `/portfolio/progress` |
| Read my committee review and its outcome | 4.41, 4.42, 4.48, 4.51, 5.8 | `/committee/my-reviews`, `/committee/reviews/{ReviewId:int}`, `/access-denied` |
| Appeal a committee decision | 4.43 | `/committee/my-reviews` |
| Export my portfolio | 5.9 | `/portfolio/export` |
| Leave the programme part-way: an encounter after my last day stops counting, and I keep my record | 5.24, 5.26, 5.28, 6.41 | `/activities/new`, `/activities/{ActivityId:int}`, `/portfolio/progress`, `/`, `/activities/mine` |
| See an EPA paused and restored in my progress and pickers | 6.15, 6.16, 6.19, 6.20, 6.24 | `/portfolio/progress`, `/portfolio/progress/{EpaId:int}`, `/activities/new`, `/activities/{ActivityId:int}`, `/activities/mine` |
| See my institution's own EPA among my targets | 6.27 | `/portfolio/progress`, `/portfolio/progress/{EpaId:int}`, `/activities/new` |
| Read my progress after moving to a new curriculum version | 6.35, 6.37, 6.39 | `/portfolio/progress`, `/activities/new` |
| Set my processing preferences, and ask for and download a copy of my data | A.1.1, A.1.2, A.1.5 | `/`, `/account/data-rights`, `/account/data-rights/download/{id:guid}` |
| Ask for access and withdraw it; ask for a correction and read the decision | A.1.6, A.1.7, A.1.9 | `/account/data-rights` |
| Ask for my personal data to be erased | A.1.11, A.1.13 | `/account/data-rights`, `/account/session-ended`, `/account/login` |
| Leave a draft and a request waiting, and be reminded | A.2.7 | `/activities/new`, `/activities/{ActivityId:int}`, `/activities/mine` |
| Sign in with a password an administrator set, and choose my own | A.4.6 | `/account/login`, `/`, `/account/profile`, `/account/change-password` |
| Meet the pages that are not mine: an administrator's page, another's activity, a forged switch, an unknown address, the error page | A.5.1, A.5.3, A.5.4, A.5.6, A.5.8 | `/admin/users`, `/admin/jobs`, `/access-denied`, `/dashboard/switch/{role}`, `/not-found`, `/account/login`, `/activities/{ActivityId:int}`, `/Error` |
| Name an assessor when one is locked out | A.6.6 | `/activities/new` |
| File an assessment with the keyboard alone, and use my pages on a phone | A.7.1, A.7.3 | `/activities/new`, `/activities/{ActivityId:int}`, `/`, `/portfolio/progress`, `/portfolio/progress/{EpaId:int}`, `/activities/mine`, `/account/data-rights` |

### PendingTrainee (each registrar between registration and admission)

| Job | Steps | Pages |
|---|---|---|
| Register from my invitation and wait to be admitted | 2.18, 2.27 | `/account/register`, `/`, `/account/logout/submit`, `/account/login` |
| See what I can do before admission | 2.19 | `/`, `/account/profile`, `/activities/mine`, `/activities/new`, `/portfolio/progress`, `/access-denied` |

### Former trainee (Dr Lerato Molefe after Step 5.17: no role, a trainee record)

| Job | Steps | Pages |
|---|---|---|
| Be emailed that my programme is complete | 5.18 | None: the email is read in the application log |
| Be signed out when my programme is marked complete, and told why | 5.19 | `/portfolio/progress`, `/account/session-ended`, `/account/login` |
| Read my read-only record of the programme | 5.20, 6.21, 6.40 | `/account/login`, `/account/login/submit`, `/portfolio/progress`, `/portfolio/progress/{EpaId:int}` |
| See what Home and the menu offer me now | 5.21 | `/` |
| Find the pages that still need the Trainee role | 5.22 | `/committee/my-reviews`, `/portfolio/authorisations`, `/msf/my-reports`, `/access-denied`, `/activities/new` |
| Export my portfolio as a graduate, and check it verifies | 5.23 | `/portfolio/export`, `/portfolio/verify` |
| Read my record on a phone | A.7.4 | `/`, `/portfolio/progress` |

### Anonymous visitor

| Job | Steps | Pages |
|---|---|---|
| (Invitee) Register from the link in my invitation | 1.8, 1.10, 2.8, 2.9, 2.10, 2.18, 2.27 | `/account/register`, `/`, `/account/logout/submit`, `/account/login` |
| (Invitee) Open a link that was used, revoked or replaced | 1.9, 2.11, 2.17, 2.27 | `/account/register` |
| (MSF respondent) Give anonymous feedback on a registrar from my emailed link | 3.39, 3.42 | `/msf/respond` |
| (MSF respondent) Open my link after I answered, or after the campaign closed | 3.40, 3.45 | `/msf/respond` |
| (Verifier) Check that a portfolio PDF is one Wombat produced, and spot a tampered copy | 5.13, 5.14, A.7.12 | `/portfolio/verify` |
| (Anyone) Look for an institutional sign-in, and open its pages with none in progress | A.3.2, A.3.3 | `/account/login`, `/account/sso-challenge/{providerKey}`, `/account/sso-callback`, `/account/link-external` |
| (Anyone) Get back in after forgetting my password | A.4.4 | `/account/login`, `/account/forgot-password` |
| (Anyone) Sign in and verify on a phone | A.7.12 | `/account/login`, `/account/forgot-password`, `/portfolio/verify` |
| (Uptime monitor) Check that the service and its database are up | A.6.10 | `/health` |
