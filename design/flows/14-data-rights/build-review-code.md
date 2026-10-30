# T387 build review: code, data, scope, authorization (flow 14)

Diff `c47bc6e0..54ce6df2` (`src`, `tests`), branch `t387`, read against `build-lanes.md`, `build-contracts.md` and the brief's
Q1-Q28 / F1-F10. Read-only; no suite was run. Each claim was read in the code.

**Result: 0 must-fix, 3 should-fix, 8 nits.** Authorization and scope hold on every command and read (checked list at the
end). The five open points are judged below.

## Should-fix

### S1. An erasure rewrites the requester's name on their requests, but leaves what they typed and the values they had corrected

- **Where:** `Wombat.Infrastructure/DataRights/ErasureExecutor.cs:377-400` (rewrites `RequesterDisplayName`, `DecidedByUserId`,
  `AppliedByUserId`, withdraws open requests; nothing touches `Reason`, `DecisionNote` or `DataRightsRectification.FromValueJson` /
  `ToValueJson`); shown by `RequestDetail.razor:284` (`@request.Reason`) and `:369-370` (`correction.FromValue` / `ToValue`);
  read by `GetDataRightsRequestById.cs:121-130`.
- **What is wrong:** F3 says "every request of the person rewritten", and the pseudonym replaces the name column. But a person's earlier
  Completed Rectification of `Account.FirstName` / `Account.LastName` stores their old and new name as the correction's before/after, and
  every request keeps the free-text reason (people write "Please erase Sipho Ndlovu's data"). After the erasure the page headed
  "deleted_user_0ae1d64b" shows the real name in the Correction table and Reason. The confirmation mail promises "your personal data has been
  erased".
- **Failure scenario:** registrar corrects "Sipho" to "Siphiwe" (Completed), later asks for erasure and is erased. A Coordinator opens the
  Rectification: the pseudonymous request lists Sipho -> Siphiwe.
- **Fix:** in the executor's request block, for the person's own requests replace `Reason` and the correction values of name records
  (`Account.FirstName`/`LastName`, or all records) with a fixed "Erased" text, or have the detail query withhold them for an erased requester
  (`ErasedOn is not null`). Needs a decision if the reason is a retained record (add a retention reason); if kept on purpose, say so in D-notes
  and in the mail. Not drawn by any board, so lane W / the operator decides.

### S2. Record a correction does not touch the request row, so a concurrent Complete or erasure does not refuse it (open point 2)

- **Where:** `Commands/ApplyRectification.cs:94-117` (checks `Approved`, then `Add`s a `DataRightsRectification` and saves; the request row is
  not in the unit of work, so its `xmin` is never compared). `CompleteRectificationRequest.cs:50,75` does carry the token.
- **What is wrong:** F8 put `xmin` on the request so that a stale page cannot act on a changed state. Complete, Approve, Reject and Withdraw
  honour it; Apply is the one write that reads the status and does not. A row-level `FOR KEY SHARE` taken by the FK insert does not change
  `xmin`.
- **Failure scenario:** tab A shows the form, tab B presses Complete request (requester is mailed "Read what was corrected"); tab A records a
  correction. It lands on a Completed request, after the mail, and My data rights' "Corrected: ..." changes after the fact. The erasure race is
  harmless (a correction on a Withdrawn request) but the same.
- **Fix:** make the save carry the request's token: `_dbContext.Set<DataRightsRequest>().Entry(dataRightsRequest).Property(r => r.Reason).IsModified = true`
  (a no-op UPDATE of the row, refused if `xmin` moved), catch `DbUpdateConcurrencyException` and throw
  `DataRightsRefusedException(NotAnApprovedRectification, ..., inner)` exactly as Complete does. Test: `DataRightsRequestRacePostgresTests` gains
  an Apply-after-Complete case.
- Severity: low (two staff, or two tabs, in one second), but it is the one write F8 missed.

### S3. Refusal paths on the request page that no test reaches (open point 4)

`ActionFocusTests` rows are `CanBeRefused: false` with the page tests as the cover, but these branches of `RequestDetail.razor` have none
(`DataRightsRequestPageTests.cs` was read in full for `NoLongerApproved`, `CompleteFailed`, `ForecastFailed`, `_forecast = null`):

- **Forecast null or throwing** (`:627-641`: `ForecastFailed`, `ShowStateChanged`, the Withdrawn / decided arms). The test file only sets `_forecast`
  to a value.
- **Complete refused by the command** (`:900-909`): `NotAnApprovedRectification` (`NoLongerApproved`), and any other exception (`CompleteFailed`);
  `NoCorrectionRecorded` from the command (the page test covers only the client-side check).
- **Record refused by the command** (`:818-857`): the `ValidationException` mapping by property (`TargetType`/`FromValue`/`ToValue` to the field
  errors, and the unmapped fall-through to `CorrectionFailed`) and `NotAnApprovedRectification`. `AFailedRecord_...` covers the generic arm only.
- Reject and Approve share `DecisionRefusedAsync`, so their refusals are covered by the Approve tests (`:415-499`).
- My data rights and the queue are covered (Submit, Withdraw decided-meanwhile and generic, Save, load failures).

Fix: four or five bUnit cases using the existing `_command` hook, one per branch above. `RequestDetail`'s forecast-null branch is the one that
decides whether an erasure dialog can open on a stale page, so it matters most.

## Nits

- **N1. Apply does not refuse the two name records for a requester with an institutional sign-in (open point 1).** Defect: no, severity nit. The
  command records that a correction happened; it changes no account (names are changed on My account / by the provider). A Coordinator could
  only bypass the disabled radios by forging a circuit message, and the result is a stored record, not a changed name. If the operator wants the
  rule stated once, put it in the handler next to `DataRightsRecords.IsValid` (`GetSignInMethodsAsync`), refusing with a field error on `TargetType`.
- **N2. Scope-before-Withdrawn (open point 3): correct, not a defect.** `ApproveDataRightsRequest.cs:85-86` and `Reject...cs:63-64` run
  `DemandReviewAccess` before `DemandSubmitted`. The plan's order would tell an out-of-scope Coordinator "withdrawn", a state tell about a foreign
  request. The order is the safer one; the plan line should be amended by lane W. The in-scope requester (refused as requester) is told nothing of the state
  either, which is right.
- **N3. Last-Administrator counts active Administrators only (open point 5): correct.** `ApproveDataRightsRequest.cs:106-125` excludes
  `IsDeactivated` (a locked/erased account runs nothing) and compares the id. Two small points: the message says "only Administrator" where it
  means "only active Administrator" (an operator with a deactivated second admin may be confused; reword to "only active Administrator"), and
  two Administrators erasing each other in the same second can both pass the check (each reads two active admins; the check is not under a lock).
  Narrow, and `xmin` does not help because they are different rows.
- **N4. `ReadValue` throws on a legacy correction value.** `GetDataRightsRequestById.cs:176` `Deserialize<string>` on a pre-T387 row whose value is
  the old default `"{}"` or a JSON object throws `JsonException`, and the whole request page becomes the load failure. Nothing is live (CLAUDE.md) and no
  page recorded one before T387, so a nit; a `try` returning the raw text would make it total.
- **N5. Unknown id and foreign id differ in the command's exception** (`InvalidOperationException("Data rights request not found.")` vs
  `UnauthorizedAccessException`) at `Approve...cs:~78/85`, `Reject`, `Apply`, `Complete`. The pages map both to the same fixed words, so nothing reaches a
  user, but the caller's own audit row (`errorMessage`) differs. T056 says 404-not-403 on reads, which the queries do; for commands the pattern is
  pre-existing.
- **N6. Queue order has no tie-break.** `ListDataRightsRequests.cs:261` `OrderByDescending(RequestedOn)` then `Skip/Take`: two requests with an equal
  timestamp can swap across a page boundary. Add `.ThenBy(r => r.Id)`.
- **N7. A pre-migration correction has `AppliedByUserId = ""`** (migration default). `StaffNamesAsync` is called with "" and the row renders
  an empty recorder. Nothing is live; if kept, show "Not recorded".
- **N8. No bound on a correction's values.** `ApplyRectification.cs:47-53` checks blank only; Blazor's 32 KB message limit is the real cap. A
  `MaximumLength(1000)` on both values and a check that `Other` names contain no control characters would match the 194-character rule.

## Checked and holding

- **Authorization / scope (T056):** `DataRightsAuthorization` one role set; `CanReview` = in scope and not the requester, the Administrator included
  (Q21/F9); `CanRectify` = `CanReview`; `CanRead` = in scope or requester; `CanDownload` = Administrator or requester. A null `InstitutionId` matches
  nobody but the Administrator. Every command loads, then demands; the three reads return null for an unknown and a foreign id alike; the forecast
  is gated as the command and returns null for a non-Erasure, a non-Submitted and the requester. List filters by the caller's institution, and a
  Coordinator with none sees nothing.
- **Erasure (F3):** the request block runs before the save that precedes the account update, inside the one transaction; all requests of the person get
  the pseudonym; `DecidedByUserId` and `AppliedByUserId` are rewritten; other open requests (Submitted and Approved, including an approved
  Rectification) are withdrawn and counted, the approving one excluded by id; all are tracked, so a concurrent change is `xmin`'s
  `PersonChanged`. The forecast's counts use the same predicates (open reviews as trainee, campaigns not Released/Withdrawn, other open requests).
  F5: reviews are named at the caller's institution (the Administrator's all), counted elsewhere; campaigns named everywhere as approved.
- **Concurrency and the audit pipeline (T201):** the token is mapped as `CommitteeReviewConfiguration` does. Approve/Reject detach the request on
  `DbUpdateConcurrencyException`, re-read untracked, and throw `Withdrawn` / `DecidedMeanwhile` with the cause inside (so `IsRefusedSave` discards
  pending changes); if the request is still Submitted the raw exception is rethrown and is also a refused save. The executor clears the tracker
  on every failure; Complete and Withdraw carry the exception inside. Every refusal in Approve is before `entity.Approve`, so nothing is staged for
  the pipeline to commit. Detaching is enough because Approve stages only the request (an Erasure's other rows are the executor's, cleared there).
- **Mail:** sent only after the commit, never on a refusal; the Erasure notice uses the address read before `ExecuteAsync`; `SendToAddressAsync` logs
  the id and kind, never the address; `DataRightsDecisionEmail` carries the type, the day and (except Erased) a link, no note and no decider; HTML parts are encoded.
- **Migration `T387_DataRightsFlow14`:** `xmin` mapped with no DDL (comment and no `AddColumn`); status 1 moved to 0 first; `AppliedOn` backfilled
  from the request's decision moment, else `now()`, before `NOT NULL`; `AppliedByUserId` default "" and 450; `TargetId` and
  `OptOutOfOptionalProcessing` dropped; Designer's `[Migration]`/`[DbContext]` present and its model equals the snapshot (diff is the method name only).
- **`ApplyRectification`:** fixed keys or `Other:` with 1-194 characters and no edge whitespace; values stored with `JsonSerializer.Serialize(string)` and
  read back through the parser (canonical-to-canonical, per CLAUDE.md's jsonb rule); recorder name read before anything is staged.
- **Stored names (F1):** live name, then the stored column, pseudonym after erasure (by id or by pseudonym); Submit stores "First Last", else the address.
- **Web:** `IScopedSender` on all three pages; no Domain `using` in any `.razor`; no `ex.Message` printed (every catch logs and shows fixed words or a
  `DataRightsRefusedException.Reason` mapping); no raw hex, inline style, arrow glyph or `ToLocalTime()` in the diff.
