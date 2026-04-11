# T026 — Data subject rights (POPIA / GDPR)

**Phase:** 6 — Cross-cutting operations
**Depends on:** T018, T021, T022, T023, T025
**Blocks:** T016

## Goal

Make Wombat compliant with the data-subject rights the institutions we care about (South African POPIA and European GDPR) actually enforce: the right to access, to rectify, to export, to object, and to erasure. This is not a "click a button and we delete everything" feature — several of these rights have statutory exceptions for medical training records — but the system must handle each request with a clear, auditable, and correct workflow.

## Rights to implement

| Right | What Wombat does | Exceptions |
|-------|-----------------|------------|
| **Access** | Any user can see their own data through the existing UI; plus a one-click "download everything we hold about me" export. | None. |
| **Rectification** | Users can correct their own profile fields directly. Activities, once in a terminal workflow state, can only be corrected via a SpecialityAdmin with a rectification note. | None. |
| **Export (portability)** | A machine-readable export of all personal data (JSON + PDF). | None. |
| **Objection** | Users can opt out of non-essential processing (e.g. analytics, optional MSF campaigns). | Cannot object to processing required for the training programme. |
| **Erasure** | On approved request, the user's profile is pseudonymised and activity authorship is replaced with a stable pseudonym. | Committee decisions, ratified assessments, audit entries, and regulatory submissions are **retained** with pseudonymised references. Document why. |

## What to do

1. **Domain** in `Wombat.Domain/DataRights/`:
   - `DataRightsRequest` — Id, RequesterUserId, RequesterDisplayName (denormalized), RequestedOn (UTC), Type (enum: `Access`, `Rectification`, `Export`, `Objection`, `Erasure`), Status (`Submitted`, `UnderReview`, `Approved`, `Rejected`, `Completed`, `Withdrawn`), Reason (longtext), DecisionNote (longtext, set when approved/rejected), DecidedByUserId?, DecidedOn?, CompletedOn?.
   - `DataRightsRectification` — child. Id, RequestId, TargetType (string), TargetId (`Guid`), FromValueJson, ToValueJson, AppliedOn?.
   - `DataRightsErasureRecord` — Id, RequestId, UserId, Pseudonym (stable opaque string, e.g. `deleted_user_7f3a`), ErasedOn, RetentionReasonsJson (array of reason codes explaining what was kept and why).
2. **Workflow**:
   - User submits a request from `Profile/DataRights.razor`.
   - Coordinator or Administrator reviews in `Admin/DataRights/RequestsList.razor`.
   - Reviewer approves or rejects with a note.
   - On approval, the system executes the request (see below).
   - User sees status updates on their own profile page.
3. **Access request** — synthesises a readable report: profile, all activities (via the portfolio export from T023), MSF aggregate reports, committee decisions, audit entries where the user is the subject. Returned as a ZIP with a PDF + a JSON dump.
4. **Rectification request** — free-text request describing the correction. Reviewer applies the correction manually through the admin UI with a linked `DataRightsRectification` child recording before/after values and the reason.
5. **Export request** — same as access but explicitly machine-readable JSON, schema-versioned.
6. **Objection** — toggles flags on the user record (`OptOutOfOptionalProcessing`, `OptOutOfDigestEmails`, etc.). Scheduled jobs and email features honour the flags.
7. **Erasure request — the hard one**:
   - Generate a stable pseudonym: `deleted_user_` + first 8 hex chars of SHA-256(salted user id). Salt stored in `appsettings` (never committed). The pseudonym is deterministic per user but unlinkable without the salt.
   - Replace all `UserId` references with a special `ErasedUserId` stub that points to the pseudonym. Do **not** actually `DELETE` rows — break the FK chain through rewrite, preserving structural integrity.
   - Fields to clear on the `AspNetUsers` row: Email, NormalizedEmail, UserName (→ pseudonym), PhoneNumber, PasswordHash, SecurityStamp, ConcurrencyStamp, LockoutEnd, TwoFactorEnabled (→ false), and any profile fields holding PII (first name, last name, display name, avatar blob, bio).
   - On activities: the trainee field on activities they were the subject of becomes the pseudonym. Their *authorship* of assessments they performed on other trainees becomes the pseudonym. The activity content is not altered.
   - On committee decisions: the decision itself is retained. Panel-member attribution by the erased user becomes the pseudonym. Document this loudly in `CUSTOMIZATION.md` — committee decisions survive erasure because they are regulatory records.
   - On audit log (T025): **retain unchanged**. Audit entries are legally permissible under legitimate-interest and legal-obligation bases. The `ActorDisplay` denormalized at write time remains the original value at that time; this is intentional and documented.
   - On MSF responses (T021): already anonymous. No action needed.
   - On reference tokens (sessions, refresh tokens, 2FA devices): destroyed.
   - Institution scope associations removed.
   - Avatar image files deleted from disk (they are separate blobs, not database rows).
   - Write a `DataRightsErasureRecord` with a list of retention reason codes (`committee_decision`, `audit_log`, `regulatory_submission`, `ratified_assessment_record`).
8. **Hard-reject conditions**: an erasure request is auto-flagged for reviewer attention when:
   - The user has an unratified committee review in progress (complete or withdraw the review first).
   - The user is the sole signatory on an open workflow (reassign first).
   - The user is a named panel member on a scheduled future review (reassign first).
   - The request is from an account with active training obligations; the reviewer must confirm the institution's legal basis (withdrawal from programme, etc.) before approving.
9. **CQRS** in `Wombat.Application/Features/DataRights/`:
   - Commands: `SubmitDataRightsRequestCommand`, `WithdrawDataRightsRequestCommand`, `ApproveDataRightsRequestCommand`, `RejectDataRightsRequestCommand`, `ExecuteErasureCommand` (internal, invoked by the approval handler), `ApplyRectificationCommand`.
   - Queries: `ListDataRightsRequestsQuery`, `GetDataRightsRequestByIdQuery`, `GetMyDataRightsRequestsQuery`.
10. **Blazor pages** under `Wombat.Web/Components/Pages/`:
    - `Profile/DataRights.razor` (user self-service)
    - `Admin/DataRights/RequestsList.razor`
    - `Admin/DataRights/RequestDetail.razor`
11. **Audit** (T025): every data-rights action writes an audit entry in category `DataRights`.

## Files created

- `src/Wombat.Domain/DataRights/**`
- `src/Wombat.Infrastructure/Persistence/Configurations/DataRights/**`
- `src/Wombat.Infrastructure/Persistence/Migrations/*DataRights.cs`
- `src/Wombat.Application/Features/DataRights/**`
- `src/Wombat.Infrastructure/DataRights/ErasureExecutor.cs`
- `src/Wombat.Infrastructure/DataRights/AccessReportBuilder.cs`
- `src/Wombat.Web/Components/Pages/Profile/DataRights.razor`
- `src/Wombat.Web/Components/Pages/Admin/DataRights/**`
- `tests/Wombat.Application.Tests/DataRights/**`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` — erasure tests confirm every table is handled (or an exception is thrown on approval if a new table was added without updating the executor).
- [ ] Manual: create a demo user, add activities, MSF response, a committee decision. Submit an erasure request. Approve. Confirm:
  - The user's profile fields are cleared.
  - The user can no longer log in.
  - Activities still exist, subject/author replaced with the pseudonym.
  - The committee decision still exists, with the pseudonym in place of the original panel member.
  - The audit log shows the full original username in entries written before the erasure, and shows `DataRights` category entries for the request and the execution.
  - The `DataRightsErasureRecord` lists the retention reasons.
- [ ] Access request returns a downloadable ZIP with a complete JSON + a PDF summary.
- [ ] Rejection writes a decision note visible to the requester.
- [ ] The erasure executor throws a coverage-test exception when a new `UserId`-referencing table is added without being handled — i.e., write a reflection-based test that enumerates every `UserId` FK in the model and fails if the executor doesn't touch it.

## Notes & gotchas

- Erasure is the second-most-sensitive operation in the system (committee decisions are first). Every change to the domain that adds a new `UserId` FK must update `ErasureExecutor`. Enforce this with the reflection-based coverage test — it is the only defence against silent regressions.
- The salt for the pseudonym hash is a deployment secret. Document in `INFRASTRUCTURE.md`. Rotating the salt breaks pseudonym stability across exports — do not rotate it.
- Do not use the word "GDPR" in error messages visible to users outside the EU, and do not use "POPIA" outside South Africa. Use "data protection law" as neutral phrasing.
- South African POPIA has slightly different language around "data subject" and "operator" than GDPR "data subject" and "processor". The implementation is the same; the policy text differs. Keep the policy text in `CUSTOMIZATION.md` so institutions localize it per jurisdiction.
- The "right to object" is weak in a mandatory training context. Most processing is not optional. Do not promise more than the law actually grants — the UI should list the *specific* processing the user can object to, not offer a blanket opt-out.
- Do not let a user submit an erasure request while they have active committee reviews. Block in the UI with an explanation, and in the command handler as a safety net.
- An erasure request from a *minor* (trainees are typically adults, but not always — under-18 medical students exist in some jurisdictions) has additional requirements. If Wombat needs to support this, add a separate task. For now, scope the feature to adult users and document the exclusion.
- This task closes the loop on a commitment made in T021 and T025: MSF anonymity and audit retention are compatible with erasure because the data is either already anonymised or legitimately retained.
