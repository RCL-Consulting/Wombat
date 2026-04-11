# Wombat — Source Map

> Machine-readable navigation guide for efficient agentic traversal.
> Generated: 2026-04-11. Root: repo root (`Wombat/`), referred to below as `/`.

---

## What is Wombat?

**Wombat** (Work-Based Assessment Tool) is an ASP.NET Core MVC web application for managing work-based assessments (WBA) for medical specialists using the EPA (Entrustable Professional Activities) framework. It supports multiple institutions, specialities, and subspecialities with configurable assessment forms.

---

## Quick orientation

| What you need | Where to look first |
|---|---|
| Project overview | `README.md` |
| DI wiring / startup | `Wombat.Web/Program.cs` |
| Database entities | `Wombat.Data/*.cs` (entity files in root of project) |
| EF entity config | `Wombat.Data/Configurations/Entities/` |
| EF DB context | `Wombat.Data/ApplicationDbContext.cs` |
| Repository interfaces | `Wombat.Application/Contracts/` |
| Repository implementations | `Wombat.Application/Repositories/` |
| Business/workflow services | `Wombat.Application/Services/` |
| View models (DTOs) | `Wombat.Common/Models/` |
| Roles & permissions | `Wombat.Common/Constants/Roles.cs`, `Permissions.cs` |
| MVC controllers | `Wombat.Web/Controllers/` |
| Razor views | `Wombat.Web/Views/<ControllerName>/` |
| Identity pages (login/register) | `Wombat.Web/Areas/Identity/Pages/Account/` |
| Email templates | `Wombat.Web/wwwroot/templates/` |
| Email sending | `Wombat.Web/Services/MailKitEmailSender.cs`, `EmailWorker.cs` |
| AutoMapper config | `Wombat.Application/Configurations/MapperConfig.cs` |
| Identity seeding | `Wombat.Web/Infrastructure/Identity/IdentitySeeder.cs` |

---

## Solution layout

```
Wombat.sln
Wombat.Common/          ← shared constants, enums, view models (no framework deps)
Wombat.Data/            ← EF Core entities, DbContext, migrations, EF configs
Wombat.Application/     ← repository interfaces + implementations, services, AutoMapper
Wombat.Web/             ← ASP.NET Core MVC host: controllers, views, areas, services
```

**Dependency direction:** `Wombat.Common ← Wombat.Data ← Wombat.Application ← Wombat.Web`

**Database:** PostgreSQL (via Npgsql). Originally MS SQL; readme reflects old config — current `Program.cs` uses `UseNpgsql`. Migrations are in `Wombat.Data/Migrations/`.

**Auth:** ASP.NET Core Identity with claim-based authorization policies. Roles control access; fine-grained permissions are expressed as claims.

**UI theme:** SB Admin 2 (Bootstrap 4). Vendor assets in `Wombat.Web/wwwroot/vendor/`.

---

## Wombat.Common (`Wombat.Common/`)

### Constants (`Constants/`)

| File | Contents |
|---|---|
| `Roles.cs` | `Role` enum + `RoleStrings` constants + `RoleHelper` (display names, enum↔string) + `RoleHierarchy` (rank-based assignment control) |
| `AssessmentRequestStatus.cs` | `AssessmentRequestStatus` enum: `Requested`, `Accepted`, `Declined`, `Cancelled`, `Completed`, `NotConducted`, `Expired`, `None` |
| `Permissions.cs` | Claims/permission constants (e.g. `ManageSpecialities`, `ManageUsers`, `RequestAssessment`, `HandleAssessmentRequests`, `ApproveTrainee`) |
| `Events.cs` | Event name constants for assessment workflow |
| `EnumExtensions.cs` | Extension to get `[Display(Name="...")]` from enum values |

### Roles (full list)
`Administrator`, `InstitutionalAdmin`, `SpecialityAdmin`, `SubSpecialityAdmin`, `Coordinator`, `CommitteeMember`, `Assessor`, `Trainee`, `PendingTrainee`

### Models (`Models/`) — View Models

| File | Purpose |
|---|---|
| `AssessmentRequestVM.cs` | Assessment request view model |
| `AssessmentEventVM.cs` | Assessment event (history entry) |
| `AssessmentFormVM.cs` | Assessment form definition |
| `LoggedAssessmentVM.cs` | Completed (logged) assessment |
| `EPAVM.cs` | EPA (Entrustable Professional Activity) |
| `EPACurriculumVM.cs` | EPA ↔ subspeciality curriculum mapping |
| `EPAFormVM.cs` | EPA ↔ assessment form mapping |
| `OptionSetVM.cs`, `OptionSetsVM.cs` | Question option set |
| `OptionVM.cs` | Single option within an option set |
| `OptionCriterionVM.cs` | Criterion within an option |
| `OptionCriterionResponseVM.cs` | Trainee response to a criterion |
| `TextCriterionVM.cs` | Free-text question criterion |
| `STARApplicationVM.cs`, `STARApplicationFormVM.cs`, `STARItemVM.cs` | STAR (Situation-Task-Action-Result) application workflow |
| `SpecialityVM.cs`, `SubSpecialityVM.cs` | Speciality and subspeciality |
| `SpecialitySelectVM.cs`, `SubSpecialitySelectVM.cs`, `SubSpecialityOption.cs` | Select/dropdown helpers |
| `InstitutionVM.cs` | Institution |
| `WombatUserVM.cs` | User view model |
| `RegistrationInvitationVM.cs`, `InviteUserVM.cs`, `RegisterFromInviteVM.cs` | Invitation-based registration flow |
| `DashboardVM.cs` | Trainee/Assessor dashboard |
| `CoordinatorDashboardVM.cs` | Coordinator dashboard |
| `PortfolioVM.cs` | ePortfolio view model |
| `RescheduleVM.cs` | Reschedule request |
| `FormSelectVM.cs`, `SelectVM.cs` | Generic select helpers |
| `CheckBoxListItem.cs` | Checkbox list item helper |
| `Collection.cs` | Generic collection wrapper |
| `ErrorViewModel.cs` | Error page model |

---

## Wombat.Data (`Wombat.Data/`)

### EF Context
`ApplicationDbContext.cs` — extends `IdentityDbContext<WombatUser>`. Applies all entity configurations. Contains UTC datetime conversion for all `DateTime` columns.

### Entities (root of project)

| File | Key relationships / notes |
|---|---|
| `BaseEntity.cs` | Base class: `int Id`, `DateCreated`, `DateModified` |
| `WombatUser.cs` | Extends `IdentityUser`; has `InstitutionId`, `SpecialityId`, `SubSpecialityId`, role-specific fields |
| `Institution.cs` | Top-level organizational unit |
| `Speciality.cs` | Medical speciality; belongs to `Institution`; has `SubSpecialities`, `EPACurriculums` |
| `SubSpeciality.cs` | Subspeciality under a `Speciality`; has `EPACurriculums` |
| `EPA.cs` | Entrustable Professional Activity; belongs to `SubSpeciality` |
| `EPACurriculum.cs` | Many-to-many link: `EPA` ↔ `SubSpeciality` with training requirements |
| `EPAForm.cs` | Links `EPA` to `AssessmentForm` |
| `AssessmentForm.cs` | Configurable assessment form (questions); has `OptionSets` |
| `AssessmentRequest.cs` | Trainee → Assessor request; has `Status` (AssessmentRequestStatus), `LoggedAssessment` |
| `AssessmentEvent.cs` | Audit/history event on an assessment request |
| `LoggedAssessment.cs` | Completed assessment; linked 1:1 to `AssessmentRequest`; has `OptionCriterionResponses`, `STARApplication` |
| `OptionSet.cs` | Set of options for a question |
| `Option.cs` | A single option/answer choice |
| `OptionCriterion.cs` | Criterion/sub-item within an option |
| `OptionCriterionResponse.cs` | Trainee's response to a criterion on a completed assessment |
| `STARApplication.cs` | STAR narrative linked to a `LoggedAssessment` |
| `STARApplicationForm.cs` | STAR form template |
| `STARItem.cs` | Individual STAR form item/question |
| `STARResponse.cs` | Response to a STAR item |
| `RegistrationInvitation.cs` | Invitation token for new-user onboarding |

### EF Configurations (`Configurations/Entities/`)
One file per entity with complex config: `AssessmentFormConfiguration`, `EPAConfiguration`, `EPACurriculumConfiguration`, `EPAFormConfiguration`, `InstitutionConfiguration`, `OptionConfiguration`, `OptionCriterionConfiguration`, `OptionSetConfiguration`, `RoleSeedConfiguration` (seeds ASP.NET roles), `SpecialityConfiguration`, `SubSpecialityConfiguration`

### Migrations (`Migrations/`)
Standard EF Core migrations against PostgreSQL. Run `update-database` locally or use idempotent script for Azure.

---

## Wombat.Application (`Wombat.Application/`)

### Repository Interfaces (`Contracts/`)

| Interface | Covers |
|---|---|
| `IGenericRepository<T>` | Base CRUD interface |
| `IAssessmentFormRepository` | Assessment form queries |
| `IAssessmentRequestRepository` | Assessment request queries + workflow helpers |
| `IAssessmentEventRepository` | Assessment event history |
| `IEPARepository` | EPA + curriculum queries |
| `IEnumCriteriaRepository` | Option criteria |
| `IOptionSetRepository` | Option sets |
| `IOptionCriterionResponseRepository` | Criterion responses |
| `ILoggedAssessmentRepository` | Logged/completed assessments |
| `ISpecialityRepository` | Specialities |
| `ISubSpecialityRepository` | Subspecialities |
| `IInstitutionRepository` | Institutions |
| `IRegistrationInvitationRepository` | Registration invitations |
| `ISTARApplicationFormRepository` | STAR application forms |
| `IAssessmentWorkflowService` | Assessment workflow orchestration |
| `INotificationService` | Email notification dispatch |
| `IUserContextService` | Current user context (institution, speciality, role) |

### Repository Implementations (`Repositories/`)
Direct counterparts to each interface above: `GenericRepository`, `AssessmentFormRepository`, `AssessmentRequestRepository`, `AssessmentEventRepository`, `EPARepository`, `OptionCriteriaRepository`, `OptionSetRepository`, `OptionCriterionResponseRepository`, `LoggedAssessmentRepository`, `SpecialityRepository`, `SubSpecialityRepository`, `InstitutionRepository`, `RegistrationInvitationRepository`, `STARApplicationFormRepository`

### Services (`Services/`)

| File | Purpose |
|---|---|
| `AssessmentWorkflowService.cs` | Core workflow: accept/decline/cancel/reschedule/complete assessment requests; sends notifications |
| `NotificationService.cs` | Renders HTML email templates and queues emails via `INotificationService` |
| `UserContextService.cs` | Provides current user's institution, speciality, subspeciality, and role context |
| `CustomUserClaimsPrincipalFactory.cs` | Adds custom claims (permissions) to the user's principal on login |

### AutoMapper (`Configurations/`)
`MapperConfig.cs` — all entity ↔ view model mappings in one profile.

### Extensions (`Extensions/`)
`AssessmentRequestExtensions.cs` — helpers for assessment request status transitions and display.

---

## Wombat.Web (`Wombat.Web/`)

### MVC Controllers (`Controllers/`)

| Controller | Routes | Primary role access |
|---|---|---|
| `HomeController.cs` | `/Home/*` | Landing page; role-specific dashboards (trainee, assessor, coordinator, admin) |
| `AssessmentRequestsController.cs` | `/AssessmentRequests/*` | Trainee creates requests; assessor accepts/declines/completes |
| `AssessmentFormsController.cs` | `/AssessmentForms/*` | Admin configures assessment forms |
| `LoggedAssessmentsController.cs` | `/LoggedAssessments/*` | View, log, submit, and manage assessments; portfolio views |
| `CoordinatorsController.cs` | `/Coordinators/*` | Coordinator review of pending assessments |
| `EPAsController.cs` | `/EPAs/*` | EPA management |
| `InstitutionsController.cs` | `/Institutions/*` | Institution CRUD |
| `SpecialitiesController.cs` | `/Specialities/*` | Speciality CRUD |
| `SubSpecialitiesController.cs` | `/SubSpecialities/*` | Subspeciality CRUD |
| `OptionSetsController.cs` | `/OptionSets/*` | Option set configuration |
| `RegistrationInvitationsController.cs` | `/RegistrationInvitations/*` | Invite and manage user onboarding |
| `STARApplicationFormsController.cs` | `/STARApplicationForms/*` | STAR form configuration |
| `WombatUsersController.cs` | `/WombatUsers/*` | User management (admin) |

### Views (`Views/`)

Each controller has a matching subfolder. Key view patterns:

**AssessmentRequests/**
- `Index.cshtml` — list of requests
- `Create.cshtml` / `CreateForEPA.cshtml` — trainee creates a request
- `Details.cshtml` — view request
- `Edit.cshtml` — edit pending request
- `AcceptRequest.cshtml` / `DeclineRequest.cshtml` / `CancelRequest.cshtml` — workflow actions
- `RescheduleRequest.cshtml` — reschedule

**LoggedAssessments/**
- `MyAssessments.cshtml` — trainee's own assessments
- `StartAssessment.cshtml` / `LogAssessmentFor.cshtml` / `LogRequestedAssessment.cshtml` / `CreateFromList.cshtml` — assessor logging paths
- `SubmitAssessment.cshtml` — final submission
- `Details.cshtml` / `DetailsFromRequest.cshtml` — view a logged assessment
- `PortfolioIndex.cshtml` / `PortfolioByEPA.cshtml` / `PortfolioByTrainee.cshtml` — portfolio views
- `AssessorsIndex.cshtml` — assessor-specific listing

**EPAs/**
- `Index.cshtml` — EPA list
- `EPA.cshtml` — EPA detail/form
- `EPACurriculum.cshtml` — curriculum config
- `Create.cshtml` / `Edit.cshtml` / `Details.cshtml`

**Home/**
- `Landing.cshtml` — public landing page
- `Index.cshtml` — authenticated landing (redirects by role)
- `TraineeHome.cshtml` / `AssessorHome.cshtml` / `Coordinator.cshtml` / `Administrator.cshtml` — role-specific dashboards
- Partials: `_Calendar.cshtml`, `_EPAProgressTracker.cshtml`, `_PortfolioBuilder.cshtml`, `_ScheduleAssessment.cshtml`, `_AcceptedRequests.cshtml`, `_DeclinedRequests.cshtml`, `_UpcomingAssessmentCalendar.cshtml`

**Shared/**
- `_Layout.cshtml` — master layout (sb-admin-2)
- `_Sidebar.cshtml` / `_Topbar.cshtml` — navigation chrome
- `_LoginPartial.cshtml` — auth header widget
- `_OptionCriterionListPartial.cshtml` / `_OptionSetSelect.cshtml` — reusable form partials
- `_DeleteConfirmationScript.cshtml` — reusable delete confirm JS
- `DisplayTemplates/` — `DateOnly.cshtml`, `DateTime.cshtml`, `NullableDateTime.cshtml`
- `EditorTemplates/` — `CheckBoxListItem.cshtml`, `EPACurriculumVM.cshtml`, `EPAFormVM.cshtml`, `OptionCriterionVM.cshtml`, `OptionVM.cshtml`, `STARItemVM.cshtml`, `SubSpecialityVM.cshtml`

### Identity Area (`Areas/Identity/Pages/Account/`)
Scaffolded ASP.NET Identity Razor Pages:
- `Login.cshtml` / `Logout.cshtml` — authentication
- `Register.cshtml` — registration (used via invitation flow)
- `Manage/` — `ChangePassword`, `Email`, `Index` (profile), `PersonalData`, `DeletePersonalData`, `TwoFactorAuthentication`, and related pages

### Services (`Services/`)

| File | Purpose |
|---|---|
| `MailKitEmailSender.cs` | Sends emails via MailKit SMTP; implements `IEmailSender` |
| `SmtpEmailSender.cs` | Alternative SMTP sender |
| `EmailWorker.cs` | `IHostedService` background worker; drains the email `Channel<>` |
| `EmailSettings.cs` | Config model for SMTP settings |
| `dbMigrator.cs` | `IHostedService` that runs EF migrations on startup |

### Infrastructure (`Infrastructure/`)

| File | Purpose |
|---|---|
| `GlobalDateTimeDisplayMetadataProvider.cs` | Ensures `DateTime` fields render consistently via display templates |
| `Identity/IdentitySeeder.cs` | Seeds default roles and users (dev: seeded accounts; prod: bootstrap admin from config) |

### Email Templates (`wwwroot/templates/`)
HTML email templates rendered by `NotificationService`:
`AssessmentRequest.html`, `AssessmentAccepted.html`, `AssessmentDeclined.html`, `AssessmentCancelled.html`, `AssessmentRescheduled.html`, `LoggedAssessment.html`, `RegistrationInvitation.html`, `RequestCommentAdded.html`, `EmailConfirmation.html`

### Static Assets (`wwwroot/`)

| Path | Contents |
|---|---|
| `css/site.css` | App-specific styles |
| `css/sb-admin-2.css` | SB Admin 2 theme |
| `js/site.js` / `custom.js` | App-specific JS |
| `js/sb-admin-2.js` | Theme JS |
| `vendor/bootstrap/` | Bootstrap 4 |
| `vendor/jquery/` | jQuery |
| `vendor/jquery-easing/` | jQuery easing plugin |
| `images/` | Logos, user avatars, branding |
| `pdf/logo.jpg` | Logo asset for PDF generation |
| `templates/` | HTML email templates |

### CI/CD
`.github/workflows/WombatWeb20240504122946.yml` — Azure App Service deployment workflow.

---

## Assessment Workflow (key state machine)

```
[Trainee creates request]
        ↓
   Requested
        ↓ (Assessor accepts)        ↓ (Assessor declines)   ↓ (Trainee cancels)
     Accepted                       Declined                Cancelled
        ↓ (Assessor completes)
     Completed   →  LoggedAssessment created
        ↓ (Trainee marks not conducted)
   NotConducted
        ↓ (time expires)
      Expired
```

Assessment events are logged to `AssessmentEvent` for each transition.
`AssessmentWorkflowService.cs` owns all transitions and notification dispatch.

---

## Authorization model

Authorization uses both **roles** and **claims**:

- **Roles** control coarse access (controller-level `[Authorize(Roles="...")]`)
- **Claims** control fine-grained features (policy-based `[Authorize(Policy="...")]`)
- `CustomUserClaimsPrincipalFactory` injects claims into the user's identity at login
- `UserContextService` provides the current user's institutional/speciality context to controllers and views

### Claim policies (defined in `Program.cs`)
`ManageSpecialities`, `ManageSubspecialities`, `ManageUsers`, `ManageRegistrationInvitations`, `ManageAssessmentForms`, `ManageOptionSets`, `ManageEPAs`, `ApproveTrainee`, `HandleAssessmentRequests`, `RequestAssessment`

---

## Key technical notes

- **Database:** PostgreSQL via Npgsql (despite README saying MS SQL — `Program.cs` is authoritative)
- **Migrations:** In `Wombat.Data/Migrations/`. Run via `update-database` or startup `dbMigrator` service
- **Email:** Async channel-based queue (`Channel<>` → `EmailWorker`); templates are HTML files in `wwwroot/templates/`
- **AutoMapper:** Single profile in `Wombat.Application/Configurations/MapperConfig.cs`; all VM ↔ entity mappings here
- **DateTime handling:** `ApplicationDbContext` converts all `DateTime` values to UTC via a `ValueConverter` — store UTC, display local
- **Identity seeding:** Dev environment seeds 4 demo users (admin, coordinator, assessor, trainee) all with `P@ssw0rd`; prod bootstraps from config
- **Default route:** `{controller=Home}/{action=Landing}/{id?}` — landing page is `HomeController.Landing`
- **License:** GNU AGPL-3.0
