# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T023 — Portfolio PDF export** (next task on the critical path after T012) — **Model: Opus**

T012 is now implemented. Next session: start `Tasks/T023-portfolio-pdf-export.md`.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → ~~T011~~ → ~~T012~~ → T023 → T024 → T025 → T026 → T027 → T013 → T014 → T015 → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

T012 completed:
- Updated `IEmailSender` interface: `SendAsync(EmailMessage message, CancellationToken ct)` (replaces old `string toEmail, string subject, string body` signature)
- Created `EmailMessage` DTO in `Application/Common/Email/` (To, Subject, HtmlBody, TextBody, Cc, Tags)
- Created email templates in `Application/Common/Email/Templates/` (7 templates: Invitation, AssessmentRequested/Accepted/Declined/Completed, StarDecision, PasswordReset) — note: moved to Application (not Infrastructure) so handlers can call them without violating layer boundaries. Templates use `$$"""` raw string literals (CSS braces are literal; `{{expr}}` for interpolation).
- Created Infrastructure email stack:
  - `EmailSettings` — SmtpHost, SmtpPort, SmtpUser, SmtpPassword, FromAddress, FromName, UseSsl, TimeoutSeconds
  - `EmailQueue` — `System.Threading.Channels.Channel<EmailMessage>` wrapper
  - `QueuedEmailSender : IEmailSender` — writes to channel and returns immediately
  - `ISmtpSender` — internal interface for testability; implemented by `MailKitEmailSender`
  - `MailKitEmailSender` — real SMTP via MailKit 4.15.1 (pinned; versions < 4.11 had CVE GHSA-g7hc-96xr-gvvx)
  - `EmailWorker : BackgroundService` — reads from channel, retries with exponential backoff (MaxRetries=3), configurable delay factory (zero-delay for tests)
  - `AssemblyInfo.cs` with `InternalsVisibleTo("Wombat.Application.Tests")`
- Updated `WombatOptions` to add `BaseUrl` (used by invitation handler to build absolute registration links)
- Updated `IssueInvitation` handler to use `InvitationEmail.Build(...)` template
- Updated `OpenMsfCampaign` handler to use `EmailMessage` directly (MSF-specific inline body)
- Updated `LoggingEmailSender` to match new interface (used as fallback when `Email:SmtpHost` is not configured)
- DI: `IEmailSender` resolves to `QueuedEmailSender` when `Email:SmtpHost` is configured; falls back to `LoggingEmailSender` otherwise. `EmailWorker` registered only when SMTP is configured.
- `appsettings.Development.json` — Email section pointing at MailHog (localhost:1025)
- `appsettings.json` — Email section with empty SmtpHost (logging fallback)
- Fixed `MsfRespondEndpointFlowTests.CapturingEmailSender` to use new `EmailMessage` interface
- Tests:
  - 2 Application tests (IssueInvitationCommandHandlerTests): one-email-sent, body contains role + expiry date
  - 3 Application tests (EmailWorkerRetryTests): retry on transient failure, drop after MaxRetries, multi-message ordering
- Verification:
  - `dotnet build Wombat.sln -c Release` — clean (0 warnings, 0 errors)
  - `dotnet test tests/Wombat.Application.Tests` — 47 passed
  - `dotnet test tests/Wombat.Web.Tests` — 33 passed
- Verification caveats:
  - Live MailHog test not run — requires MailHog running locally + live DB
  - Integration tests not run — require Docker/PostgreSQL
