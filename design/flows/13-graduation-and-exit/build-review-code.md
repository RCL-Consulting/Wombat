# T379 build review: code, data, scope, authorization

Diff `master...t379` (`src`), branch `t379`, HEAD `95d3f765`, read against `build-lanes.md`, `build-contracts.md`, `round-2-review.md` (F1–F11)
and the brief's Q1–Q27. Read-only; no suite was run. Each claim was read in the code.

**Result: 0 must-fix, 2 should-fix, 7 nits.** Authorization and scope hold on every new read and command (checked list at the end).

## Should-fix

### B1. "The last committee decision" can name a formative review that was only closed

- **Where:** `Wombat.Application/Features/CommitteeDecisions/LastRatifiedDecision.cs:38` (`entity.RatifiedOn != null`); its comment, `:25`
  ("a withdrawn or formative one never was"); `Wombat.Domain/CommitteeDecisions/CommitteeReview.cs:530` (`Close` sets `RatifiedOn = utcNow`
  for a formative review; `Ratify` is `:575`).
- **What is wrong:** the reader treats a non-null `RatifiedOn` as "ratified", but `CommitteeReview.Close` (formative only) stamps `RatifiedOn` and
  `RatifiedByUserId` too. A formative review has no decision, so its `Category` is null and it reads as an entrustment-only review.
- **Failure scenario:** a registrar's last summative review was ratified 2026-06-01 (Graduate); a formative review is closed 2026-09-15. End this
  programme says "The last committee decision: Mid-year review, ratified 2026-09-15." (`TraineeProfileWords.LastDecision`, the no-category arm),
  naming a review that ratified nothing and hiding the summative one. Gates nothing (Q13), so the harm is a wrong sentence on a page an admin
  reads before ending a programme.
- **Fix:** add `&& !entity.IsFormative` to the `Where` (and keep the comment true). Test: a closed formative review newer than a ratified summative one;
  the summative one is named.

### B2. The take-back forecast's day is read from a profile that can have changed, and a stale `_today` is kept for the whole page life

- **Where:** `TraineeProfileEdit.razor` `OnParametersSetAsync` (`_today = QuotaCalendar.Today(Clock)` once per parameter set) and `AskAsync`
  (`TraineeProfileWords.LastDayRefusal(_endDate, _profile.ProgrammeStartDate, _today)`); command check `TraineeProfile.EndDayRefusal` via
  `EnsureCanEnd` (`TraineeProfile.cs:206–240`).
- **What is wrong:** nothing lets a refused day through, because `Complete`/`Deactivate` run the same `EndDayRefusal` again and the page maps their
  `InvalidOperationException` to the same field (`EndAsync` catch). But a page left open past SAST midnight keeps yesterday's `_today`: the
  press-time check refuses today's date ("cannot be after today (yesterday)") though the command would accept it, and the default `_endDate`
  stays yesterday. The page's refusal then states a wrong bound.
- **Failure scenario:** an admin opens the profile at 23:50, returns at 00:10, types the new day: refused with yesterday's date named; reloading is
  the only way out.
- **Fix:** take `QuotaCalendar.Today(Clock)` inside `AskAsync` (as the command does) instead of reading the cached field; keep `_today` only for the
  default. Low risk; severity is S only because the words state a false bound.

## Nits

- **B3.** `CompleteTraineeProfile.cs:126` builds the link from `(_baseUrl ?? string.Empty).TrimEnd('/')`: with `Wombat:BaseUrl` unset the email carries a
  relative `/portfolio/progress`. Same pattern as `AccountNotices.LinkTo` and `InvitationLinks`, so no regression, and BaseUrl is operator configuration
  (not user input), HTML-encoded in the HTML part (`GraduationEmail.cs`), so no open redirect or injection. The text part is raw but plain text.
- **B4.** `GetTraineeProfileByIdQuery.cs:106–108`: for an ended, role-less, unlocked account `RecordStars.ReadAsync` runs `EntrustmentStandingReader.ReadAsync`
  (the full standing) only to use `AnyIssued`; `InForce` is discarded. An `AnyAsync` on `EntrustmentDecision` would do. One extra read on one page.
- **B5.** `VerifyExport.cs:101–105` loads every row of the hash (`ToListAsync`) to take first, last and count. The lookup itself is sound: `ContentHash`
  is compared untransformed to a lower-cased value and `PortfolioExportConfiguration.cs:19` indexes it, so no scan. A hash has a handful of rows; a
  `Count`/`Min`/`Max` plus one `FirstOrDefault` would be tidier.
- **B6.** `VerifyExport.cs:78–87` drops `Rune.IsWhiteSpace` runes only; a pasted zero-width space (U+200B) or BOM (U+FEFF) is not whitespace, so the
  refusal names an invisible `Character`. Refusal order (blank, not-hex before length, then 12-digit prefix, then length) and case folding are right.
- **B7.** `ProgrammeEndCredit.ObservedTakenBackAsync` counts activities with any transition of `CreditedItemCount > 0`; an activity whose credit was later
  reversed is still counted, so "will stop counting" and the result's count can overstate. The forecast and the commands share the rule exactly (the
  one method), so they cannot disagree; the rule is T281's and unchanged.
- **B8.** `TraineeProfileEdit.razor` `EndAsync`: a failure after `hold.CommitAsync` (the role removal, `Complete…:115`/`Deactivate…:108`) throws out of the
  command with the end already saved. The page then shows the exception as a last-day refusal if it is an `InvalidOperationException`, or "Nothing has
  changed" if not. The email is queued (`QueuedEmailSender`), so it is the role write only. Pre-existing ordering; a re-read in the catch would tell the truth.
- **B9.** `LastRatifiedDecision` reads the trainee's reviews at any institution: a trainee moved between institutions shows the old institution's
  category and review type to the new institution's admin. Names nothing, gates nothing, and the profile read is scope-checked; stated for completeness.

## Checked and holding

- **`GetExportSubjectQuery`:** `MayReadAsync` is asked first (the export command's own check), an unknown id and an erased pseudonym return null (contacts
  missing), the own route with no profile returns null (F6), the Administrator is never refused (`MayReadAsync` is true for them), and the staff route
  for a trainee-first caller is refused by `ActsAsTrainee`. Name falls back to the address only when the person has no name; an erased person never
  reaches it.
- **`MayExport` on the three DTOs:** `GetCommitteeReviewByIdQuery:47`, `GetProgrammeTraineeQuery`, `GetTraineeProfileByIdQuery` all call
  `TraineeScopeResolver.MayReadAsync`, the command's own check; the commands that answer with the review answer false and the page keeps what it read
  (`ReviewDetail.razor`). The erased person's header action is suppressed on the profile (`IsErased`), and elsewhere lands on the Unavailable page.
- **`GetTakeBackForecastQuery`:** `DemandTraineeAdministration`, then `CanAccessInstitution`, unknown and out-of-scope alike throw the same
  `InvalidOperationException` (indistinguishable, better than the end commands, which differ). Returns a count and one date; the `ScopeExemptHandlers`
  entry states exactly that and the stale-entry test covers it. It counts by `ProgrammeEndCredit.ObservedTakenBackAsync`, the rule
  `CountTakenBackAsync` (and so the commands' `ProgrammeEndResult(TakenBack)`) uses.
- **End commands:** `ProgrammeEndResult` is read before the end is recorded, under the same `HoldForEndAsync`; `SaveAsync` replays when the count is above
  zero as before; the day is checked by one Domain rule (`EndDayRefusal`) on the press and in both commands, so no race lets a refused day through
  (B2 is about wording only). `EndDayRefusal`'s words name the bound's day on both sides, ISO and invariant culture.
- **`ListEndedProfilesQuery`:** trainee-first and role check before any read; an Institutional admin is filtered by `InstitutionId` in SQL, an admin with
  no institution gets an empty page, the Administrator sees all with institution names (one batched read). Count, then `ORDER BY COALESCE(CompletedOn,
  DeactivatedOn, MinValue) DESC, Id DESC` with `Skip/Take` in SQL; page clamped, sizes whitelisted; contacts read once for the page (no N+1); an erased
  person is listed under the pseudonym with an empty address.
- **`GetTraineeProfileById`:** null for an unknown and an out-of-scope id alike; trainee-first before the read; an erased person's name is the pseudonym and
  the address empty; the account section is absent for the erased.
- **`GetFormerTraineeHomeQuery`:** reads the caller's own preferred profile only (the user id is the principal's); null for a running profile, no profile
  or an end with no recorded day.
- **`My authorisations`:** the `TraineeOrFormerTrainee` policy admits only the Trainee role or the record claim; `GetDecisionsForTraineeQuery` and
  `DownloadEntrustmentCertificateCommand` judge by `MayReadAsync` (own record, else oversight) or the issuing chair, so a graduate reaches her own STARs and
  no one else's (`DownloadEntrustmentCertificateTests` holds both).
- **`ListActivitiesBySubject`:** `SubjectEndedOn` and `ObservedAfterEnd` are read only when the caller is the subject; `PreferredProfiles ... !IsActive`
  is the preferred (active-else-latest) profile, so a re-admitted trainee's running programme ends nothing.
- **`VerifyExport`:** parsing drops all whitespace, lower-cases, refuses non-hex before counting, names a 12-digit prefix; the page echoes the hash through
  Razor's encoding; the response carries ids only, as before (an erased person's pseudonym after `ErasureExecutor` rewrites `PortfolioExport`).
- **Migration `20261027115106_T379_PortfolioExportCleanupRetired`:** two `DELETE`s on `ScheduledJobRuns` and `ScheduledJobDefinitions` by `"Key"` (both tables
  and the column exist in the snapshot); empty `Down`; the Designer's model is the T312 Designer's and the snapshot is untouched (`git diff --stat` shows
  no change to it); the job's class, registration and test are gone and no source reference to `portfolio-export-cleanup` remains.
- **`GraduationEmail`:** name, programme and URL are HTML-encoded in the HTML part; the link is `BaseUrl` plus a constant path.
- **`LastRatifiedDecision`:** orders by `RatifiedOn` then `Id`, takes the review's current decision (a remitted appeal's replacement), and reads one review
  with its decisions; apart from B1 the choice of review is right.
