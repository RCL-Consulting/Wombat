# T012 — Email infrastructure

**Phase:** 5 — Web & messaging
**Depends on:** T005 (stubs an email sender)
**Blocks:** T015
**Status:** Done

## Goal

Replace the logging stub from T005 with a real MailKit-based sender backed by an in-process queue and a hosted worker. Templates for all transactional emails. No external provider lock-in.

## What was done

1. **Application layer:**
   - Updated `IEmailSender` interface: `Task SendAsync(EmailMessage message, CancellationToken ct)` (replaces old `string toEmail, string subject, string body` signature).
   - Created `EmailMessage` DTO in `Application/Common/Email/` — `To`, `Subject`, `HtmlBody`, `TextBody`, `Cc` (optional), `Tags` (for logging).
   - Added `BaseUrl` to `WombatOptions` so handlers can build absolute links for emails.

2. **Email templates in `Application/Common/Email/Templates/`:**
   - `EmailTemplateBase` — minimal HTML email shell using `$$"""` raw strings (CSS braces literal, `{{expr}}` for interpolation).
   - `InvitationEmail` — the invitation link email.
   - `AssessmentRequestedEmail` — notifies assessor a new request has arrived.
   - `AssessmentAcceptedEmail` — notifies trainee.
   - `AssessmentDeclinedEmail` — notifies trainee (optional reason).
   - `AssessmentCompletedEmail` — notifies trainee.
   - `StarDecisionEmail` — notifies trainee of approve/decline (optional comments).
   - `PasswordResetEmail` — Identity password reset with our template.
   - **Note:** Templates live in Application (not Infrastructure) so handlers can call them without violating layer boundaries. They are pure string-interpolation helpers with no dependencies.

3. **Infrastructure email stack in `Infrastructure/Email/`:**
   - `EmailSettings` — SmtpHost, SmtpPort, SmtpUser, SmtpPassword, FromAddress, FromName, UseSsl, TimeoutSeconds. Section name `"Email"`.
   - `EmailQueue` — thin wrapper around `Channel<EmailMessage>` (unbounded, single-reader).
   - `QueuedEmailSender : IEmailSender` — the thing Application code injects. Writes to the channel and returns immediately.
   - `ISmtpSender` — interface for testability; implemented by `MailKitEmailSender`.
   - `MailKitEmailSender : ISmtpSender` — constructs `SmtpClient`, authenticates, sends, disconnects. Uses `IOptions<EmailSettings>`.
   - `EmailWorker : BackgroundService` — loops reading from the channel and invokes `ISmtpSender` via scoped DI. Retries with exponential backoff on transient failures (MaxRetries=3, delays 2s/4s). Logs and drops after retries exhausted. Configurable delay factory for zero-delay unit tests.
   - `LoggingEmailSender` — updated to match new `EmailMessage` interface; used as fallback when `Email:SmtpHost` is not configured.
   - `AssemblyInfo.cs` — `InternalsVisibleTo("Wombat.Application.Tests")` for `EmailWorker.ExecutePublicAsync` test access.

4. **DI wiring in `DependencyInjection.cs`:**
   - `IOptions<EmailSettings>` bound to `"Email"` config section.
   - `EmailQueue` registered as singleton.
   - `ISmtpSender` → `MailKitEmailSender` registered as transient.
   - When `Email:SmtpHost` is configured: `IEmailSender` → `QueuedEmailSender` + `EmailWorker` hosted service.
   - When `Email:SmtpHost` is blank: `IEmailSender` → `LoggingEmailSender` (no worker).

5. **Caller updates:**
   - `IssueInvitation` handler — uses `InvitationEmail.Build(...)` template, absolute URL via `WombatOptions.BaseUrl`.
   - `OpenMsfCampaign` handler — uses `EmailMessage` directly with inline MSF-specific body.
   - `MsfRespondEndpointFlowTests.CapturingEmailSender` — updated to new `EmailMessage` interface.

6. **Configuration:**
   - `appsettings.Development.json` — Email section pointing at MailHog (`localhost:1025`), `BaseUrl: "http://localhost:5080"`.
   - `appsettings.json` — Email section with empty `SmtpHost` (logging fallback), empty `BaseUrl`.

7. **Package:** MailKit 4.15.1 pinned in `Directory.Packages.props` (versions < 4.11 had CVE GHSA-g7hc-96xr-gvvx on MimeKit).

## Files created

- `src/Wombat.Application/Common/Email/EmailMessage.cs`
- `src/Wombat.Application/Common/Email/Templates/EmailTemplateBase.cs`
- `src/Wombat.Application/Common/Email/Templates/InvitationEmail.cs`
- `src/Wombat.Application/Common/Email/Templates/AssessmentRequestedEmail.cs`
- `src/Wombat.Application/Common/Email/Templates/AssessmentAcceptedEmail.cs`
- `src/Wombat.Application/Common/Email/Templates/AssessmentDeclinedEmail.cs`
- `src/Wombat.Application/Common/Email/Templates/AssessmentCompletedEmail.cs`
- `src/Wombat.Application/Common/Email/Templates/StarDecisionEmail.cs`
- `src/Wombat.Application/Common/Email/Templates/PasswordResetEmail.cs`
- `src/Wombat.Infrastructure/Email/EmailSettings.cs`
- `src/Wombat.Infrastructure/Email/EmailQueue.cs`
- `src/Wombat.Infrastructure/Email/QueuedEmailSender.cs`
- `src/Wombat.Infrastructure/Email/ISmtpSender.cs`
- `src/Wombat.Infrastructure/Email/MailKitEmailSender.cs`
- `src/Wombat.Infrastructure/Email/EmailWorker.cs`
- `src/Wombat.Infrastructure/AssemblyInfo.cs`
- `tests/Wombat.Application.Tests/Features/Invitations/IssueInvitationCommandHandlerTests.cs`
- `tests/Wombat.Application.Tests/Email/EmailWorkerRetryTests.cs`

## Files modified

- `src/Wombat.Application/Common/Interfaces/IEmailSender.cs`
- `src/Wombat.Application/Common/Options/WombatOptions.cs`
- `src/Wombat.Application/Features/Invitations/IssueInvitation.cs`
- `src/Wombat.Application/Features/MultiSourceFeedback/OpenMsfCampaign.cs`
- `src/Wombat.Infrastructure/Email/LoggingEmailSender.cs`
- `src/Wombat.Infrastructure/DependencyInjection.cs`
- `src/Wombat.Infrastructure/Wombat.Infrastructure.csproj`
- `src/Wombat.Web/appsettings.json`
- `src/Wombat.Web/appsettings.Development.json`
- `tests/Wombat.Integration.Tests/MultiSourceFeedback/MsfRespondEndpointFlowTests.cs`
- `Directory.Packages.props`

## Verification

- [x] `dotnet build Wombat.sln -c Release` — clean (0 warnings, 0 errors).
- [ ] With MailHog running locally, issue an invitation and confirm the email appears in MailHog with the right subject and a working link. *(requires live DB + MailHog)*
- [x] Unit test: the worker retries a transient failure and eventually succeeds. (EmailWorkerRetryTests — 3 tests)
- [ ] Integration test (in `Wombat.Integration.Tests`): issuing an invitation enqueues exactly one message. *(deferred — requires Docker for Testcontainers)*
- [ ] If the SMTP server is unreachable, web requests are **not** slowed down — the message is still enqueued, the worker just fails in the background and logs. *(architecture guarantees this — `QueuedEmailSender` writes to channel; `EmailWorker` retries independently)*
- [x] Application tests: 47 passed (including 2 new IssueInvitation + 3 new EmailWorker tests).
- [x] Web tests: 33 passed (no regressions).

## Notes & gotchas

- The queue is in-process. It is fine for one server. The day Wombat needs horizontal scale, replace the channel with a durable queue (Redis, RabbitMQ, Azure Queue) behind the same `IEmailSender`. Not a Phase 1 concern.
- If the app crashes with messages in the channel, they are lost. Invitations can be re-issued and assessment emails are cosmetic (the data is in the DB regardless).
- Templates live in `Application/Common/Email/Templates/` (not Infrastructure) so Application handlers can build `EmailMessage` instances without an Infrastructure reference. This is a deliberate departure from the original task spec.
- `MailKit` 4.15.1 pinned in `Directory.Packages.props`. Earlier versions had CVE GHSA-g7hc-96xr-gvvx on the `MimeKit` transitive dependency.
- Templates render both HTML and text bodies. The HTML shell uses `$$"""` raw string literals to avoid CSS brace escaping issues.
- Logging: every email is tagged with its purpose (`Tags = ["invitation", "role:Trainee"]`) so server logs are searchable.
