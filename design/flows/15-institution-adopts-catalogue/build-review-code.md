# T392 (flow 15) build: code review of the Application halves

Reviewed: `git diff 9d4b2a2f..HEAD -- src tests` on `t392` (`ee98ba1c`), Application, Domain and the two seeders; the pages are another
reviewer's. Line numbers are the worktree's files at that HEAD. Read against `build-contracts.md`, the brief § 6, round-2-review
and round-3-check decisions.

**Counts: 0 must, 4 should, 6 nit.** No defect that lets an Institutional admin of A see or act on B was found; every new query
and command was traced for it (see "Checked and sound").

Run, both `-c Release`, no `--no-build`: Application tests for Adoptions, CurriculumInstitutionState, EpaScopeGuard,
CreateEpaRefusal, TraineeCurriculumChoices and CurriculumStartsHeldBack, 66 passed; `CurriculumAdminScopePostgresTests`, 3 passed
on PostgreSQL (Q9's new arm translates to SQL, ended profile included).

## Must

None.

## Should

### S-1. `EpaReadScope` and the discipline picker read claims without checking the role

`src/Wombat.Application/Features/Epas/GetEpas.cs:211-212` (`ForAsync`), `:238-239` (`Apply`), `:378` (`DisciplinesAsync`).
`ForAsync` uses `GetCollegeId()` and `GetInstitutionId()` with no `IsCollegeAdmin()` or `IsInstitutionalAdmin()` test. Everything
else in this build reads each claim only with its role (`CurriculumAdminScope.Openable`'s comment: every signed-in user carries an
institution claim (T113), "and it makes nobody an InstitutionalAdmin"; `CanAccessInstitution`, `CanAccessCollege`).

Failure scenario: the old list had this shape, but the old by-id read did not (it ended in the role-gated
`CanAccessInstitution`). Now that by-id "follows the list's rule" (Q2, Q19), a CollegeAdmin of College C whose account carries the
usual institution claim for institution A reads, by id, every A-owned local EPA (the second arm of `Apply`), with `NamesOwner`
true. The rule for items is that a CollegeAdmin reads none of an institution's own (`CurriculumAdminScope` class remarks), and
`EpaScopeGuardTests.GetEpaById_CollegeAdmin_AnInstitutionsOwnEpa_ReturnsNull` passes only because its principal has no institution
claim. Likewise any other role that reaches the query (Assessor, Coordinator, Trainee: institution claim, no college claim)
resolves to the "institution" branch and reads A's adopted national EPAs and own EPAs. The pages' policies mask most of this, but
the query is the second line of defence and no longer holds alone. `DisciplinesAsync` has the mirror: an account with a stale
college claim and the IA role is offered the College's disciplines while `CreatesOwn` is true, so Save is refused with the F9
sentence the picker should have prevented.

Fix: in `ForAsync` take `collegeId = principal.IsCollegeAdmin() ? principal.GetCollegeId() : null` and
`institutionId = principal.IsInstitutionalAdmin() ? principal.GetInstitutionId() : null` (the `Openable` form); the same in
`DisciplinesAsync`. Add a test with institution and college claims on a CollegeAdmin and on an Assessor.

### S-2. F6 "no Add on a superseded version" is a page rule only; the command still adds

`src/Wombat.Application/Features/Curricula/ManageCurriculumItems.cs` (`AddCurriculumItemCommandHandler`, the
`EnsureOwnerAdoptedAsync` call) accepts "an adoption now or before" (`CurriculumAdminScope.cs:310-333`). `MayAddItem`
(`GetCurricula.cs:194-198`) is read by the pages only.

Failure scenario: an Institutional admin opens 11.1's items page while 11.1 is her active adoption (Add offered); a colleague
adopts 11.2; she presses Save on the add form. The command succeeds and writes an own item onto a superseded version that no new
admission joins and that Q9 now keeps open for the registrars still on it, which changes their measure mid-programme (F6 calls
that "a College-level question"). The same by a direct call. The brief records "the command allows it" as the *rejected*
alternative, so the decision and the code disagree. Update and Remove rightly stay allowed.

Fix: for an own item on Add only, require the institution's *active* adoption of that version (a check beside
`EnsureOwnerAdoptedAsync`, thrown before anything is staged: `UnauthorizedAccessException`, or a `FieldRefusalException` if the
page should say a sentence). Test: Add on a superseded version refused with nothing saved; Update and Remove of an existing own
item there still accepted.

### S-3. The new institution-state queries are tested only on the in-memory provider

`tests/Wombat.Application.Tests/Features/Adoptions/GetAdoptableCurriculaTests.cs:132`, `Curricula/CurriculumInstitutionStateTests.cs:173`
and `Epas/EpaScopeGuardTests.cs:32` use `UseInMemoryDatabase`; only Q9's `Openable` arm has a PostgreSQL test. Not proven to
translate: `GetAdoptableCurricula.cs` (a correlated `Items.Count(...)` inside an anonymous projection that also builds an
`AdoptableCurriculumDto`, with `activeIds.Contains` in the filter), `GetCurricula.cs` `CurriculumInstitutionStates.ReadAsync`
(`GroupBy` then `Select` over `TraineeProfile`) and `GetEpas.cs` `EpaReads.ListAsync` (a correlated `FirstOrDefault` on `Institution`
inside a projection). The repo's own rule is that the in-memory provider is not evidence for SQL (that is why
`CurriculumAdminScopePostgresTests` exists).

Fix: extend the Postgres tests with one run each of `GetAdoptableCurriculaQuery`, `GetCurriculaListQuery` (an IA, and the
Administrator with an institution), `GetEpaListQuery` and `GetEpaPageQuery`.

### S-4. The dialog's consent is read at one moment and the supersession is made at another

`src/Wombat.Application/Features/Adoptions/AdoptCurriculum.cs` (the `existingActive` read and supersede). The list says what
adopting would supersede ("the 3 registrars on 11.1 stay on it"), and the command supersedes whatever is active at the save. If
another admin adopted a third version between the page's read and the Adopt, the command succeeds, supersedes the third version,
and the result's `SupersededVersion`/`SupersededRegistrarCount` say so, but the person consented to words about a different version
and registrar count. No data is wrong; the confirmation was. Rated should because Q6 makes the dialog the consent.

Fix: an optional `ExpectedSupersededCurriculumId` on `AdoptCurriculumCommand`; if given and different from the active adoption
found, throw `AdoptionRefusedException` with a new `Stale` reason before anything is staged, so the page reloads and asks again.
At the least, confirm the page words its result from `AdoptionDto.Superseded*`, never from the list's row.

## Nit

- **N-1. `CurriculumDto.CanEditInPlace` is dead.** Declared `CurriculumDto.cs:15`, filled positionally at `CurriculumMappings.cs:40, 70`
  and in `CurriculumRow.Projection` (`true`), read nowhere in `src` or `tests` (grep of both trees). `MayAddItem` replaced its only
  use. Remove it with its `EnsureCurriculumCanBeEditedInPlace()` guard (`CurriculumMappings.cs:143`) and the constructor
  arguments, or note why it stays.
- **N-2. Two XML-doc comments were broken by typos.** `GetCurricula.cs:13` starts `//// <summary>` and `:265` starts `// <summary>`
  (the second belongs to `CurriculumRow`; its closing `/// </summary>` is orphaned). Both lose their documentation and may warn
  (CS1587/CS1570); use `/// <summary>`.
- **N-3. Item order can differ between a read and a command's returned curriculum.** `CurriculumMappings.cs:74-76` orders
  `Epa.Code` in memory (the culture comparer); `CurriculumRow.Projection` orders in SQL (the database collation). Round-3
  decision 3 says every read and every command's result agree; they do for `KGK-001` against `PAED-015`, but codes differing
  only in case or punctuation may not. Use an ordinal comparer in memory, or state the assumption in
  `TheItems_AreTheNationalOnesInEpaOrder_ThenEachInstitutionsOwn`.
- **N-4. `CurriculumChoiceDto.IsActiveAdoption`'s doc says "false on the admission's choices, which are all active".** The
  admission path never sets it, so a page that read it there would draw every choice as not adopted
  (`TraineeCurriculumChoices.cs`, the record). Fill it on the admission path too, or say "do not read it on the admission".
- **N-5. `EpaFieldMessages.NotAdopted` names a discipline from an unscoped read.** `CreateEpa.cs` reads the sub-speciality by id
  with no scope and the refusal quotes its speciality and sub-speciality names to any IA. Catalogue names are national and not
  secret, so this is not a leak, but `FieldRefusalException`'s remark "it names nobody out of the caller's scope" is not strictly
  true for a discipline the institution has not adopted; reword the remark.
- **N-6. `GetAdoptableCurriculaQuery` returns the whole open catalogue to an Administrator for any `InstitutionId`, existing or
  not** (`GetAdoptableCurricula.cs`, the Administrator branch), as `GetCurriculaListQuery` does. Harmless, but an id naming no
  institution would better read nothing, as the scoped caller's other id does.

## Checked and sound

- **Order of refusals, nothing staged first.** `AdoptCurriculumCommandHandler`: scope throw, then the curriculum read (NotFound),
  `IsActive` (HeldBack), the existing-active read (AlreadyAdopted), the superseded and registrar reads, and only then
  `existingActive.IsActive = false` and `Add`. Test `Handle_AHeldBackVersion_IsRefused_WithNothingWritten`. `CreateEpa`:
  not-found, authorization, the F9 read, then `Add`. `UpdateEpa`: F9 after both authorizations, before the reactivation reads and the
  first assignment (the EPA hold is disposed by the throw). `UpdateTraineeProfile`: the move refusal is still read before any
  mutation.
- **Held back.** Adopt refuses `!IsActive` with the typed `AdoptionRefusal.HeldBack`; `CreateCurriculumCommandHandler` and
  `CloneAsNewVersion` store `false` (the source keeps its flag; `CurriculumStartsHeldBackTests`); both seeders set `IsActive = true`
  explicitly (a comment only, the same value as the property default).
- **Adoption day.** `ProgrammeCalendar.DateOf(_timeProvider.GetUtcNow().UtcDateTime)`; `Handle_DatesTheAdoptionByTheSouthAfricanDay`
  pins the late-evening case.
- **Q9's arm to SQL.** One correlated EXISTS holding the own-item EXISTS and the profile EXISTS (no `IsActive` on the profile, so an
  ended one keeps the version open); the PostgreSQL test is green. `OpenToInstitution` shares `OpenTo`, so list and by-id stay one rule.
- **Counts' scope.** `RegistrarCount`, `AdoptedRegistrarCount` and `SupersededRegistrarCount` all filter `InstitutionId` and
  `IsActive` and group by `CurriculumId`; none uses `AdoptionId`. The institution is the caller's own claim (IA) or the
  Administrator's chosen id; a scoped caller naming another id gets `[]` (`AScopedCaller_NamingAnotherInstitution_ReadsNothing`).
- **SupersededOn ordering.** `(AdoptedOn, Id)` in `CurriculumInstitutionStates.For` and in `GetTraineeCurriculumChoices`, both
  using the next adoption in the discipline after the version's latest; same-day going back is tested in both.
- **Administrator versus IA.** `OwnInstitution` is null for an Administrator even with the IA role, so her All curricula and items
  page carry no institution's state; `MayAddItem` for an IA needs her own institution's active adoption; `Adopt`, the EPA
  commands and the item commands keep their `CanAccessInstitution`/`CanAccessCollege` checks before reading the target.
- **F9.** Create refuses an unadopted discipline for an own EPA only (never a national one); Update only when `SubSpecialityId`
  changes (`CreateEpaRefusalTests`: create, move, keep in place, the Administrator naming the institution).
- **Audit pipeline (T201).** `FieldRefusalException` is an `InvalidOperationException`; the unique-violation rethrow carries the
  `DbUpdateException` as `inner`, so `IsRefusedSave` finds it and the pipeline discards the refused save; refusals thrown before
  staging have nothing to discard. The `when SqlState == 23505` filter narrows T272 correctly (any other save failure reaches the
  page as unexpected).
- **`CurriculumItemOnEndedRecordsException`.** Same message as before, still an `InvalidOperationException`, thrown at the same
  point before the removal; `Records` is carried for the page.

## Test coverage against the brief's list

Covered: held-back refusal with nothing written, the SA day, another institution's IA on Adopt, going back, Newer/Older with the Id
tie, the registrar and own-items-not-carried counts, the Administrator needing an institution, a scoped caller naming another
institution, `SupersededOn` with same-day going back, `MayAddItem`, F9 on both commands, T272, create and clone held back, Q9 on
PostgreSQL, `FieldRefusal` on the page side.
Not covered: role-less claims on `EpaReadScope` (S-1); Add on a superseded version (S-2); SQL translation of the new queries
(S-3); the stale dialog (S-4); that the seeded curricula are open after both seeders run (their value did not change, low risk).
