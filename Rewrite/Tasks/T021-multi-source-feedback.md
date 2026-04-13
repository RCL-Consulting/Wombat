# T021 — Multi-source feedback (MSF/360)

**Phase:** 4 — Hardcoded domain features
**Depends on:** T006, T012
**Blocks:** T011, T016

## Goal

Implement multi-source feedback as a dedicated hardcoded workflow. This is one of the things that cannot be expressed as a generic activity type because of the anonymity requirements and the specific aggregation semantics. It lives alongside the activity platform, not inside it.

## Why this is not an activity type

A Mini-CEX is "one assessor rates one trainee, data is visible to both". MSF is "ten respondents rate one trainee, data is visible to the trainee only in aggregated form, individual respondents must not be de-anonymisable". That involves:

- Token-based respondent access (no login required for external respondents).
- Aggregation rules that refuse to display results until a minimum respondent count is reached.
- Permission rules that never let the trainee see individual responses.
- A coordinator review step between aggregation and trainee release.

None of these fit the Activity platform's grammar cleanly. Implementing them as schema extensions would bloat the DSL with MSF-specific concepts. Dedicated code is cleaner.

## What to do

1. **Domain** in `Wombat.Domain/MultiSourceFeedback/`:
   - `MsfCampaign` — aggregate root. Id, SubjectUserId (trainee), TemplateId, CreatedOn, OpensOn (`DateOnly`), ClosesOn (`DateOnly`), MinimumResponses (default 8), State (`Draft`, `Open`, `Closed`, `UnderReview`, `Released`, `Withdrawn`).
   - `MsfTemplate` — reusable template. Id, Name, SpecialityId?, Questions (list), IsActive. Think of this as a stripped-down activity form — but dedicated, because of anonymity.
   - `MsfQuestion` — child of template. Id, Order, Prompt, Type (`Scale`, `LongText`), ScaleId? (for scale questions), Required.
   - `MsfInvitation` — Id, CampaignId, RespondentEmail, RespondentCategory (choice: `peer_doctor`, `consultant`, `nurse`, `ahp`, `patient`, `other`), TokenHash, IssuedOn, ExpiresOn, RespondedOn?, RevokedOn?.
   - `MsfResponse` — Id, InvitationId, SubmittedOn (UTC). **No FK to user.** The only link is `InvitationId`, and the invitation is destroyed after response aggregation (see below).
   - `MsfResponseAnswer` — Id, ResponseId, QuestionId, ScaleValue?, LongText?.
2. **Anonymity protection measures**:
   - Once a campaign is `Closed` and `MsfResponse` aggregation is computed, the `MsfInvitation` rows are hashed-and-truncated: email is replaced with a hash, `RespondentEmail` column is nulled. This removes the link from response → person.
   - Response data is never displayed grouped by respondent — only by category, and only when the category has at least `MinimumCategoryResponses` (default 3) responses. Otherwise the category is suppressed (displayed as "insufficient responses").
   - The trainee sees only the aggregated view after the coordinator has reviewed and released.
   - No admin interface shows the email-to-response mapping at any time, ever.
3. **Workflow**:
   - Admin or coordinator creates an `MsfCampaign` for a trainee, picks a template, sets dates.
   - Trainee or coordinator adds respondent emails (with categories).
   - On `Open`, the system emails each respondent a token URL. Respondents fill in the form anonymously from the URL (no login).
   - On `ClosesOn`, the campaign auto-transitions to `Closed`. The aggregator runs.
   - Coordinator reviews the aggregated report. They can add a narrative and choose to release to the trainee.
   - Trainee sees the aggregated report with narrative.
4. **Public respondent endpoint**:
   - A small anonymous route (`/msf/respond?token=...`) that looks up the invitation by token hash, shows the form, accepts the response, marks the invitation as responded.
   - Rate-limit by IP and by token. Tokens are single-use.
   - This route is in `Wombat.Api` (minimal API) rather than the Blazor app, to keep it simple and stateless. A small anonymous Razor page would also work; match whatever is easiest to host under Caddy with a clean URL.
5. **CQRS** in `Wombat.Application/Features/MultiSourceFeedback/`:
   - Commands: `CreateMsfTemplate`, `CreateMsfCampaign`, `AddMsfInvitation`, `OpenMsfCampaign`, `CloseMsfCampaign`, `ReleaseMsfCampaign`, `WithdrawMsfCampaign`.
   - Queries: `GetCampaignAggregateReport` (applies the suppression rules), `ListMsfCampaignsForTrainee`, `ListMsfCampaignsForCoordinator`.
6. **Blazor pages**:
   - `MultiSourceFeedback/CampaignsList.razor` (coordinator view)
   - `MultiSourceFeedback/CampaignEdit.razor` (create + add invitees)
   - `MultiSourceFeedback/CampaignReport.razor` (coordinator review + release)
   - `MultiSourceFeedback/MyMsfReports.razor` (trainee view of released reports only)
   - Plus the anonymous `/msf/respond` page in `Wombat.Api`.
7. **Migration**: `MultiSourceFeedback`. Include `Designer.cs`.
8. **Tests**:
   - Anonymity invariant: after aggregation, no query can return respondent email or user id associated with a response.
   - Suppression: a category with fewer than `MinimumCategoryResponses` does not appear in the aggregate report.
   - Token validation: expired, revoked, already-used tokens are rejected.
   - Campaign auto-closes at `ClosesOn` (exercised by the scheduled job from T024).

## Files created

- `src/Wombat.Domain/MultiSourceFeedback/**`
- `src/Wombat.Infrastructure/Persistence/Configurations/MultiSourceFeedback/**`
- `src/Wombat.Infrastructure/Persistence/Migrations/*MultiSourceFeedback.cs`
- `src/Wombat.Application/Features/MultiSourceFeedback/**`
- `src/Wombat.Web/Components/Pages/MultiSourceFeedback/**`
- `src/Wombat.Api/Endpoints/MsfRespond.cs`

## Verification

- [x] `dotnet build` clean.
- [x] `dotnet test` — anonymity tests and suppression tests green.
- [ ] Manual: coordinator creates a campaign for a demo trainee, adds 8 respondents across 3 categories, opens it. Emails land in MailHog. Open 4 of the tokens, submit responses. Close the campaign. Aggregator runs. Coordinator releases. Trainee sees the aggregated view. **Confirm individual respondent identity is not visible anywhere in the UI or API.**
- [ ] Attempt an admin "god view" to see individual responses — should fail because no such view exists.

## Session notes

- 2026-04-13: Implemented the core MSF slice across Domain/Application/Infrastructure/Web/API.
- Added the dedicated aggregate model, persistence configuration, and EF migration `20260413095239_MultiSourceFeedback`.
- Added aggregate-report generation with category suppression and invitation anonymisation on close.
- Added anonymous token-backed response endpoints under `Wombat.Api/Endpoints/MsfRespond.cs` with rate limiting.
- Added coordinator/trainee pages under `src/Wombat.Web/Components/Pages/MultiSourceFeedback/`.
- Added application tests covering anonymity, suppression, expired/revoked/used token rejection, and response submission persistence.
- Deliberate scope note: the anonymous respondent surface is currently API-first JSON rather than a rendered HTML page, and the coordinator UI is a minimal bootstrap flow rather than a polished picker-driven workflow.
- Remaining work for full task completion: manual walkthrough against a live local database plus final decision on whether T012 lands before MailHog-backed MSF verification.

## Notes & gotchas

- Anonymity is a legal commitment, not a feature. Breaking it damages trust permanently. Write the tests first.
- Respondent emails are PII and are deleted after aggregation. This is a deliberate data minimisation choice. Document it in `CUSTOMIZATION.md` or a privacy note.
- "Patient" category surveys are optional; some institutions use them, others find them problematic. Make the category optional at template level.
- Do not let the coordinator export the raw response table. The aggregate is the only supported output.
- Retention: aggregated reports are kept indefinitely (they're part of the portfolio). Individual responses are deleted after aggregation. This is explicitly different from activities, where raw data lives forever.
