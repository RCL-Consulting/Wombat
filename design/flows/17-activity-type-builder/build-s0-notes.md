# T398 § 0: what the lanes must know beyond `build-contracts.md` § 0

§ 0 is `d3c98db5` on `t398`. Counts after it: Domain 840, Application 4419, Infrastructure 1035, Architecture 52, Web 4703,
Integration 530; build 0 warnings.

## Signatures
- `ActivityTypeAdminScope.MayWrite(principal, scope, scopeId, owningCollegeId, bool systemManaged)`;
  `MayWriteAsync(db, principal, scope, scopeId, bool systemManaged, ct)`.
- `MayRead(principal, scope, scopeId, owningCollegeId, bool isActive, int publishedVersion)`;
  `MayReadAsync(db, principal, scope, scopeId, isActive, publishedVersion, ct)` (ignore `SystemManaged`: Q19).
- `ActivityTypeScopeGuard.EnsureCallerCanWriteAsync(db, principal, ActivityType? activityType, scope, scopeId, ct)`;
  Save passes `existing` (null for a new type) on both its calls.
- `OwnerOf(ActivityScope, ScopeTarget)` and `ScopeTargetAsync(db, scope, scopeId, ct)` are **internal** (ScopeTarget is
  internal; Application has no InternalsVisibleTo): test them through the queries.
- Scale picker: `EntrustmentScaleOption(Value, SeedKey, Name)` (`.Label` became `.Name`).

## Words
- N10: `$"Nothing was saved. {type.Name} is written by {WrittenBy}, so it cannot be changed here."`; a system-managed key
  not in `SystemManagedReleases` reads "a release of Wombat" (NEW, § 5).
- `ActivityTypeWords` (NEW for § 5): `SetBy` with a null name reads "Its College" / "Its institution";
  `Unavailable.Heading/.Body/.GoTo`; `Draft(DateTime?)` gives "None" for null (lane B: pass `DraftUpdatedOn` or check
  `HasDraft`); `ReadOnlyNotice` holds T300's notice word for word.
- `ScopeDetail` follows the boards (Institution → "Institution", Global → "Global", a vanished target → "Speciality",
  never "#id"). § 0 did not switch the list's `FormatScope` (lane B) or the builder's `StoredScopeText` (lane C).

## Left to the lanes
- Lane A: the parse exceptions' `IActivityTypeRefusal` and their fallback-place test; the editor DTO's new members and
  every version's `PublishedByName`; an unknown or unreadable id becomes null, so these tests change on purpose:
  `ActivityTypeScopeGuardTests`' `AnIdThatNamesNoType…` and draft-only cases, and the Q19 test's institution case.
- Lane B: the list DTO's new members; replace the empty `.workflow-readout, .credit-readout, .versions-readout {}` rule
  under lane B's app.css heading (it exists only for `DefinedClassTests`).
- Lane W: 6.14a's Expect changes: `msf_cpsa` reads View, not Edit, for the College admin and the Administrator; a save on
  it is refused in N10's words.

## Departures § 0 took (accepted by the integrator)
- Q19's test: an institution's admin reads an active `msf_cpsa` but never an inactive one (they never read an inactive
  College type); the College admin and the Administrator still read and list an inactive one.
- `SystemManagedActivityTypeTests.ATypeMadeInTheBuilder_IsNotSystemManaged…` now asserts the command carries no
  `SystemManaged` and the editor's is `init`-only.
