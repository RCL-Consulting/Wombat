# T012 — Email infrastructure

**Phase:** 3 — Web
**Depends on:** T005 (stubs an email sender)
**Blocks:** T015

## Goal

Replace the logging stub from T005 with a real MailKit-based sender backed by an in-process queue and a hosted worker. Templates for all transactional emails. No external provider lock-in.

## What to do

1. In `Wombat.Application/Common/Interfaces/`:
   - `IEmailSender` — `Task SendAsync(EmailMessage message, CancellationToken ct)`.
   - `EmailMessage` DTO — To, Subject, HtmlBody, TextBody, Cc (optional), Tags (for logging).
2. In `Wombat.Infrastructure/Email/`:
   - `MailKitEmailSender : IEmailSender` — constructs a `MailKit.Net.Smtp.SmtpClient`, uses the SMTP settings from `IOptions<EmailSettings>`, sends.
   - `EmailSettings` — SmtpHost, SmtpPort, SmtpUser, SmtpPassword, FromAddress, FromName, UseSsl, Timeout.
   - `EmailQueue` — a thin wrapper around `System.Threading.Channels.Channel<EmailMessage>`.
   - `EmailWorker : BackgroundService` — loops reading from the channel and invokes `MailKitEmailSender`. Retries with exponential backoff on transient failures. Logs failures after N retries and drops the message (do not block the queue).
   - `QueuedEmailSender : IEmailSender` — the thing Application code actually injects. Writes to the channel and returns immediately. The worker is the one that does the real send.
3. Wire the DI registration so that `IEmailSender` resolves to `QueuedEmailSender` (fast path) and the worker internally uses `MailKitEmailSender`. Mirror ClinicAssist's pattern exactly.
4. Templates under `Wombat.Infrastructure/Email/Templates/`:
   - `InvitationEmail` — the invitation link email.
   - `AssessmentRequestedEmail` — notifies assessor a new request has arrived.
   - `AssessmentAcceptedEmail` — notifies trainee.
   - `AssessmentDeclinedEmail` — notifies trainee.
   - `AssessmentCompletedEmail` — notifies trainee.
   - `StarDecisionEmail` — notifies trainee of approve/decline.
   - `PasswordResetEmail` — Identity's default, but use our template.
5. Template rendering: use a tiny custom renderer (Razor class library) or string interpolation with a helper — whichever ClinicAssist uses. Do not pull in a templating dependency if one is not already in use.
6. Hook the new sender into every place the stub was used (T005's invitation flow, the T008 assessment handlers, the T009 STAR handlers).
7. Configure SMTP in `appsettings.Development.json` pointing at MailHog (Docker) or Papercut for local dev. Real SMTP comes through environment variables in Production.

## Files created

- `src/Wombat.Application/Common/Interfaces/IEmailSender.cs`
- `src/Wombat.Application/Common/Email/EmailMessage.cs`
- `src/Wombat.Infrastructure/Email/{MailKitEmailSender,EmailSettings,EmailQueue,EmailWorker,QueuedEmailSender}.cs`
- `src/Wombat.Infrastructure/Email/Templates/*.cshtml` (or `.razor`)

## Verification

- [ ] `dotnet build` clean.
- [ ] With MailHog running locally, issue an invitation and confirm the email appears in MailHog with the right subject and a working link.
- [ ] Unit test: the worker retries a transient failure and eventually succeeds.
- [ ] Integration test (in `Wombat.Integration.Tests`): issuing an invitation enqueues exactly one message.
- [ ] If the SMTP server is unreachable, web requests are **not** slowed down — the message is still enqueued, the worker just fails in the background and logs.

## Notes & gotchas

- The queue is in-process. It is fine for one server. The day Wombat needs horizontal scale, replace the channel with a durable queue (Redis, RabbitMQ, Azure Queue) behind the same `IEmailSender`. Not a Phase 1 concern.
- If the app crashes with messages in the channel, they are lost. Document this in the task file; the mitigation is that invitations can be re-issued and assessment emails are cosmetic (the data is in the DB regardless).
- `MailKit` is an evolving package — pin the version in `Directory.Packages.props`, do not use `Microsoft.AspNetCore.Identity.UI.Services.IEmailSender`.
- Templates should render both HTML and text bodies. Some mail clients still don't render HTML.
- Logging: tag every email with its purpose (`Tags = ["invitation", "user-7"]`) so server logs are searchable.
