# T398 (flow 17) build: code review of § 0 and lane A

Reviewed: `git diff d3c98db5~1..HEAD -- src tests` at `68e127d8` (§ 0 `d3c98db5`, lane A `562aae67` `ab9ed40d`, lane B's query part of
`69653e7f`), minus the Web pages. Note `08d1bbda~1` is § 0 itself, so the range above starts one commit earlier to include it.
Read against CLAUDE.md (Activity platform, Roles, jsonb, Editing a seed folder), `build-lanes.md` § Reviews and the fix pass,
`build-contracts.md` and `build-s0-notes.md`. Line numbers are the worktree's files at that head.

**Counts: 1 must, 3 should, 5 nit.**

Run (observed), `-c Release`, no `--no-build`, one at a time: Domain 895 passed; Application filtered to `ActivityType`,
`SystemManaged`, `PublishScaleBinding` 200 passed; Infrastructure filtered to `SeedRoundTrip`, `ActivityTypeSeedRefresher`,
`ActivityTypeSeedCatalogue` 293 passed; Integration (real PostgreSQL) `ActivityTypeEditorPostgres`, `ActivityTypesListPostgres`,
`ActivityTypeAdminScopePostgres` 3 passed. Nothing failed.

## What holds (checked, no finding)

- **T334/Q19, nothing widened.** `MayWrite` is the old rule with `!systemManaged &&` in front (`ActivityTypeAdminScope.cs:58-59`);
  `MayWriteAsync` likewise (`:124-133`). `MayRead`/`MayReadAsync` (`:68-94`) are the old `CanReadAsync` with `MayWrite`'s scope body
  (`ScopeAdmits`, `:100-117`, byte-for-byte the old body) in the third clause, so an Administrator, an InstitutionalAdmin of that
  institution and a CollegeAdmin of the owning College read exactly what they read before, system-managed or not. Every caller is
  updated: the three commands' guards (`SaveActivityTypeDraftCommand.cs:122,128`, `PublishActivityTypeDraftCommand.cs:42`,
  `DiscardActivityTypeDraftCommand.cs:34`), `ForCallerAsync` (`GetActivityTypeEditorQuery.cs:183`), `CanReadAsync` (`:291-297`), the list's
  `MayWrite` and `MayRead` (`ListActivityTypesAdminQuery.cs:90,102`). `grep` finds no other writer of `ActivityType` in
  Application, Api or Infrastructure than the three commands (and the seeders/refresher, which bypass the guard by design). The
  system-managed refusal comes first and in N10's words (`:357-362`); the scope refusals' words are unchanged.
- **P2.** `SaveDraft` and `PublishDraft` both go through `ParseAll` (`ActivityType.cs:106,132`), which collects the three parses and
  runs each cross-check only where the payloads it reads parsed (`:181-189`), and assigns nothing: `SaveDraft` computes all four
  staging strings first (`:108-111`) and assigns after; `PublishDraft` assigns after `ParseAll`. The save handler stages nothing before
  `SaveDraft` (the new entity is `Add`ed at `SaveActivityTypeDraftCommand.cs:171`, after it), so the audit pipeline's catch has nothing to
  commit; `SystemManagedActivityTypeTests` asserts an empty change tracker after three refusals. The refusal's message is the places'
  sentences joined, equal to the old single message when one place fails (`ActivityTypeDraftRefusedException.cs`, `ActorFieldRules.cs:177-181`,
  `EvidenceEpa.cs:63-69` keep the old `Distinct` by grouping on sentence in first-seen order).
- **Refusal places against the parsers.** Every `PlacedAt` sits in a `try` that has read the section, field, move or directive key, and
  `HasOwnPlace` stops an outer frame overriding an inner one (`SchemaParseException.cs:46-47`, same in the other two). A wrapped
  `ActorRuleParser` refusal in `editable_by` becomes a `SchemaParseException` as before (`FormSchemaParser.cs:427-429`) and is placed
  by the enclosing field. The two `ValueKind != Number` guards make a non-number property a parse refusal instead of an
  `InvalidOperationException` (an improvement, same sentence). Only the nominee-role sentence changed (S-1).
- **Indented serializer never stored; seeds' round trip unchanged.** `Serialize(x)` is `Serialize(x, indented: false)` and the compact
  branch is a plain `new Utf8JsonWriter(stream)` as before (`DslJson.cs:16-27`); the relaxed encoder and indentation exist only in the
  `indented` branch. The only `Serialize` calls on the store path are `SaveDraft`'s (`ActivityType.cs:108-110`, compact). The only
  `indented: true` calls are `ActivityTypeReadout.LaidOut*` (`ActivityTypeReadout.cs:108-124`), which feed DTO text. `SeedRoundTripTests`
  is untouched and green; `TheBuildersJson_IsLaidOut_AndParsesToTheStoredDraft` compares canonical to canonical.
- **Editor read's null and callers.** The only two callers of `GetActivityTypeEditorQuery` are the builder (null becomes Unavailable)
  and `NewActivity.razor:309-310` (null is turned into the page's own load failure, caught at `:319`). The three commands pass the
  entity they already hold into `ForCallerAsync`, never null. `NewActivity` reads `PublishedSchemaJson ?? DraftSchemaJson`
  (`:209-213`), so the filing page is untouched by the reader change.
- **Names read once.** One `UserDisplayNames.ResolveAsync` per `ForCallerAsync` (`GetActivityTypeEditorQuery.cs:192-198`), the seeder
  filtered out before the lookup, null saver filtered by `ResolveAsync`; the unit test asserts one lookup holding exactly the two ids and
  none for a seeder-only type.
- **The list's owner and flags.** `Owner`, `SystemManaged`, `WrittenBy` come from the rows and the one `ScopeTargetsAsync` read; no query
  per row; `CallerCollegeAsync` two small reads for a College admin only. `Readable` is `MayRead` on the row's own columns.
- **Seed ownership moved to Domain, refresher unchanged.** `SeedActorUserId` is now a `const` alias of `SeederUserId` (`"seed-system"`,
  same value); the refresher asks the same four guards in the same order (`ActivityTypeSeedRefresher.cs:295,317,334`), with
  `MaxBy(Version)` equal to the old `OrderByDescending(...).First()`; its existing tests pass.
- **The claim lane W could not confirm: "Wombat's catalogue".** Every seeder publishes with `ActivityTypeSeedCatalogue.SeedActorUserId`
  (`DataSeeder.cs:483`, `PaediatricCatalogueSeeder.cs:449`, `ActivityTypeSeedRefresher.cs:370`), which is `ActivityTypeSeedOwnership.SeederUserId`
  `"seed-system"` (`ActivityTypeSeedCatalogue.cs:93`), the exact string `IsSeeder` compares ordinally (`GetActivityTypeEditorQuery.cs:228-229`),
  and `PublishDraft` stamps `PublishedByUserId = actorUserId.Trim()` (`ActivityType.cs:148`). The earlier migrations (T122, T162) also write the literal
  `seed-system`. So a seeded version's publisher reads "Wombat's catalogue". Confirmed by reading the path and by
  `ActivityTypeEditorReadTests.TheSaverAndEachPublisher_AreNamed_InOneLookup` and the PostgreSQL test (both green).
- **The PostgreSQL tests** translate the preview's nested `Contains` subqueries, the College short-code navigation, and the list; they ran on a real server.

## Must

### M-1. The preview's EPAs (and so another institution's own EPAs) are read for any reader, by the type's scope, not the caller's

`GetActivityTypeEditorQuery.cs:222` fills `PreviewEpaOptions` unconditionally in `ForCallerAsync`, from the type's own scope and target
(`PreviewEpaOptionsAsync`, `:239-283`). The reader gate in front of it is `MayRead`, whose first clause admits *anyone* to an active,
published type of any scope (`ActivityTypeAdminScope.cs:75`, kept deliberately: "a trainee on /activities/new"). The builder page renders
that list in the Form tab's live preview for readers too (`ActivityTypeEdit.razor:546-549`; `ReaderTabs` include Form,
`ActivityTypeEdit.razor.cs:84`).

Failure scenario: an InstitutionalAdmin of Kgosi Kgari opens `/admin/activity-types/{id}` for Baragwanath's active, published
Institution-scope type (ids are sequential integers; the list does not show it to her, the editor does). The editor returns
`PreviewEpaOptions` for scope Institution, id Baragwanath: Baragwanath's *own* EPAs (`OwningInstitutionId == Baragwanath`, e.g. `BARA-001
Another's own`) and every EPA on the curricula Baragwanath adopted, which the preview then draws as a dropdown. Before T398 the preview
read the caller's own claims, so this was never possible. EPAs an institution owns are institution-scoped everywhere else in the product
(CLAUDE.md § InstitutionalAdmin scope-aware powers: out of scope is 404, not leaked). The tests prove the type's-scope filter
(`ActivityTypeEditorReadTests.cs:229-236`, `ActivityTypeEditorPostgresTests.cs:105-107`) only with the type's own institution as the caller;
none reads another institution's published type as a reader.

Verified by reading the path: `MayReadAsync` true on `(isActive && publishedVersion > 0)` (`:91`) -> `ForBuilder` true -> `ForCallerAsync` ->
`PreviewEpaOptionsAsync(dbContext, activityType.Scope, activityType.ScopeId)` with no principal.

Fix: compute the options only when the caller is admitted to the type's scope (the rule `ScopeAdmits` already holds, i.e. a writer *or* a
reader of its own scope), and return `[]` (or null, "the form loads its own") otherwise; or restrict the Institution case to national EPAs
plus the caller's own institution's. Add a test: an InstitutionalAdmin of institution A reading institution B's published type gets no
B-owned EPA.

## Should

### S-1. The nominee-role refusal now tells the author to type a value the parser rejects

`FormSchemaParser.cs:404-407` still matches the typed role against the keys (`WombatRoles.Nominable`, ordinal-ignore-case: "InstitutionalAdmin",
"CommitteeMember"), but the sentence now lists the labels (`Use one of: Institutional admin, Coordinator, Committee member, Assessor,
Trainee.`, N1/T324; the test was rewritten to expect it, `FormSchemaParserNomineeRoleTests.cs:193-209`). "Committee member" and
"Institutional admin" do not match any key, so following the message returns the same refusal.

Failure scenario: an author editing the Form JSON (the builder's laid-out JSON editor) types `"role": "Supervisor"`, reads "Use one of ...
Committee member ...", types `"role": "Committee member"`, and is refused again, now with no way to learn the accepted spelling. The form's
role picker is unaffected; the JSON path is.

Fix: either accept the label as well (match `WombatRoleLabels.For(role)` ordinal-ignore-case, keeping the stored value the key), or keep the
keys in the sentence. The first matches the decision's intent and costs one extra comparison; add the label case to the theory.

### S-2. A reader is shown names and draft metadata of people and drafts it has no scope over

`ForCallerAsync` resolves `DraftSavedByName` and every version's `PublishedByName` for every caller `MayRead` admits
(`GetActivityTypeEditorQuery.cs:192-220`, with the Web showing the strip at `ActivityTypeEdit.razor.cs:268-279` and `:279`). So the same
cross-institution reader as M-1 sees "Saved 2026-11-05 by Nolwazi Mbatha" and the publishers' display names for another institution's or
College's type (before T398 only the ids were in the DTO). `Map` also still sends `DraftDisplayFieldsJson` as `StagingDisplayFieldsJson ??
DisplayFieldsJson` (`:321`) to a reader whose three payload strings were replaced by the published ones (`:206-208`): the reader sees
published JSON beside a draft's display fields. The page does not print the latter, but the DTO is inconsistent with "a reader never sees the
writers' draft".

Fix: for a caller who is not a writer, set `DraftSavedByName` null (and `HasDraft` stays the page's "a draft exists" flag only if wanted),
resolve publisher names only when the caller's scope admits the type, and use `DisplayFieldsJson` for a reader as for the payloads. Test:
reader versus writer on a type with a draft.

### S-3. A system-managed type that already holds a draft can never be discarded, and the refresher skips it forever

T334 refuses save, publish and discard on a system-managed type to everyone (`ActivityTypeScopeGuard`, `:357-362`;
`SavingPublishingAndDiscardingASystemManagedTypeInTheBuilder_IsRefused_ToEveryone` builds exactly this state and asserts the discard is
refused). The seed refresher skips a type with a draft in flight (`ActivityTypeSeedRefresher.cs:317-329`, `SkippedDraftInFlight`) and does
not discard it. A draft left on `msf_cpsa` or `learner_feedback_cpsa` by a pre-T334 builder save (the doc comment at
`ActivityTypeAdminScope.cs:43-48` says the builder could do this) therefore blocks the release's seed refresh for good, and the only way out
is a database edit. Nothing is live, but the replay corpus is the test corpus, and the new refusal removed the one in-product way out.

Fix: let the Discard path (not Save/Publish) pass for a system-managed type when it has a draft, or have the refresher discard a draft whose
`StagingUpdatedByUserId` is not the seeder on a system-managed type; at least log it in the refresher and say so in HANDOFF. Add a test for the
chosen route.

## Nit

- **N-1.** `Description` has a 2000-character column (`ActivityTypeConfiguration.cs:14`) but `SaveActivityTypeDraftCommandValidator` checks
  Key and Name only (`SaveActivityTypeDraftCommand.cs:72-83`); a long description is a database failure, the exact class S10 closes for the
  other two.
- **N-2.** The system-managed refusal is asked before the scope check (`ActivityTypeAdminScope.cs:357`) and interpolates `activityType.Name`, so an
  out-of-scope caller who reaches the command with the id of an inactive system-managed type learns its name; the handler's remarks say scope
  guards run first so an outsider learns nothing. Not reachable from the pages (they pass only ids the caller was shown), so nit.
- **N-3.** `PublishActivityTypeDraftCommand.cs:48-52` parses the staged schema and asks the scale-binding question before `PublishDraft`, so a
  staged draft wrong on a tab *and* binding a deleted scale reports only one of them, and a staged schema that no longer parses throws a bare
  `SchemaParseException` instead of the aggregate. A saved draft always parsed at save, so only a pre-rule draft can hit it.
- **N-4.** `ActorFieldRules.PublishProblems` keeps one place per distinct sentence (`:262-265`), so two user fields declaring the same options
  yield one `UserFieldHasOptions` place (the first's); the second field's row gets no at-place message, only the summary.
- **N-5.** For a type nobody has published (a Global one any reader may open) a non-writer is shown the draft, not the published payload
  (`GetActivityTypeEditorQuery.cs:206-208`, `?? editor.DraftSchemaJson`). Acknowledged in the comment; noted so the "reader gets the published
  payloads" claim is read with this exception.
