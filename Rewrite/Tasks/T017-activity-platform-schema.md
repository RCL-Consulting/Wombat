# T017 — Activity platform: schema, aggregates, storage

**Phase:** 2 — Activity platform (new)
**Depends on:** T004, T006
**Blocks:** T018, T019, T020, T021, T022, T023, T025

## Goal

Lay the foundation of the no-code activity platform. This task delivers the domain aggregates, the schema DSL, the workflow DSL, the credit rules DSL, EF configurations, and migrations — but no UI and no engine yet. The engine is T018; the UI is T019. Splitting lets the domain be solid and tested before the moving parts land on top.

## What to do

1. **Domain aggregates** in `Wombat.Domain/Activities/`:
   - `ActivityType` — aggregate root. Id, Key (stable string), Name, Description, Scope (`Global`/`Institution`/`Speciality`/`SubSpeciality`), ScopeId (nullable), SchemaJson, WorkflowJson, CreditRulesJson, Version (int), IsActive, OwnerUserId, CreatedOn. Has domain method `PublishNewVersion(newSchemaJson, newWorkflowJson, newCreditRulesJson)` that bumps version and validates before accepting.
   - `Activity` — aggregate root. Id, ActivityTypeId, SchemaVersion (pinned), SubjectUserId, CreatedByUserId, CurrentState, DataJson, EpaId (nullable), CurriculumItemId (nullable), CreatedOn, UpdatedOn. Domain method `ApplyTransition(transitionKey, actorUserId, newDataJson, note)` — records the event, updates state + data.
   - `ActivityTransition` — child of `Activity`. Id, ActivityId, FromState, ToState, TransitionKey, ActorUserId, OccurredOn, Note, SnapshotJson.
   - `ActivityPermissionRule` — child of `ActivityType`. Id, ActivityTypeId, TransitionKey, ActorRuleJson, FieldRequirementJson.
2. **Schema DSL** in `Wombat.Domain/Activities/Schema/`:
   - `FormSchema` record — `int Version`, `IReadOnlyList<FormSection> Sections`.
   - `FormSection` record — Key, Title, ShowIf (optional expression), Fields.
   - `FormField` record — Key, Type (enum), Label, HelpText, Required, Options (for choice types), ScaleKey (for scale type), Validation (record of Min/Max/Regex/Length), ShowIf.
   - `FieldType` enum — `Text`, `LongText`, `Number`, `Date`, `DateTime`, `Choice`, `MultiChoice`, `Scale`, `User`, `Epa`, `File`, `Checkbox`, `Rating`, `Markdown`.
   - `FormSchemaParser` — parses the `SchemaJson` string into `FormSchema`. Throws `SchemaParseException` on malformed input. All parsing lives in Domain; there is no C# code in Application or Infrastructure that reads `SchemaJson` directly.
3. **Workflow DSL** in `Wombat.Domain/Activities/Workflow/`:
   - `Workflow` record — Version, InitialState, States (list), Transitions (list).
   - `WorkflowState` record — Key, Label, Terminal (bool).
   - `WorkflowTransition` record — Key, From (list of states), To, Actor (parsed actor rule), RequiresNote, RequiresFields.
   - `ActorRule` — discriminated union: `SubjectUser`, `CreatorUser`, `NamedRole(string)`, `ScopeMatch(string)`, `Combined(list)`. String form e.g. `"role:Assessor+subject_assessor"` parses to `Combined([NamedRole("Assessor"), /* custom */])`. Keep the parser simple; document the grammar.
   - `WorkflowParser` — parses `WorkflowJson` into `Workflow`. Validates that every transition's `From`/`To` references a declared state, initial state exists, no unreachable states.
4. **Credit rules DSL** in `Wombat.Domain/Activities/Credit/`:
   - `CreditRules` record — list of `CreditDirective`.
   - `CreditDirective` record — `CurriculumItemMatchRule`, `int Amount`, `string? MinimumLevelField`, `string? MinimumLevelFixed`.
   - Parser throws on unknown fields.
5. **EF configurations** in `Wombat.Infrastructure/Persistence/Configurations/Activities/`:
   - `ActivityTypeConfiguration` — jsonb columns for `SchemaJson`, `WorkflowJson`, `CreditRulesJson`. Unique index on `(Key, Version)`. Index on `(Scope, ScopeId)`.
   - `ActivityConfiguration` — jsonb column for `DataJson`. Indexes listed in `ARCHITECTURE.md` Activity layer section. GIN on `DataJson`; expression indexes on `(DataJson->>'epa_id')` and `(DataJson->>'assessor_user_id')`; composite on `(ActivityTypeId, CurrentState, SubjectUserId)`.
   - `ActivityTransitionConfiguration` — FK to Activity, cascade delete with the parent.
   - `ActivityPermissionRuleConfiguration` — FK to ActivityType, cascade delete.
6. **EF migration**: `ActivitiesPlatform`. Include `Designer.cs`. Verify the jsonb columns and GIN indexes land correctly — Npgsql handles jsonb natively but needs `Database.Migrate()` to have executed `CREATE EXTENSION` if any is required (none for jsonb; yes for `pg_trgm` if later added).
7. **Domain unit tests** under `tests/Wombat.Domain.Tests/Activities/`:
   - `FormSchemaParserTests` — valid, missing field key, unknown field type, invalid section shape.
   - `WorkflowParserTests` — valid, missing initial state, transition to undeclared state, unreachable state.
   - `CreditRulesParserTests` — valid, malformed directive.
   - `ActivityApplyTransitionTests` — pure domain: apply transition mutates state and records event; applying an undeclared transition throws.
   - Round-trip tests: sample JSON → parsed → serialize → original (modulo insignificant whitespace).

## Files created

- `src/Wombat.Domain/Activities/*.cs` (all the aggregates + children + parsers)
- `src/Wombat.Infrastructure/Persistence/Configurations/Activities/*.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*ActivitiesPlatform.cs` (+ Designer.cs)
- `tests/Wombat.Domain.Tests/Activities/**`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test tests/Wombat.Domain.Tests` — all parser and transition tests green.
- [ ] `dotnet ef database update` applies the migration successfully against a local Postgres.
- [ ] Inserting a sample `ActivityType` row with a valid `SchemaJson` / `WorkflowJson` / `CreditRulesJson` round-trips through the parser without error.

## Notes & gotchas

- The parsers live in **Domain**. This is unusual — parsing usually feels like infrastructure — but the rules the parsers enforce are domain invariants, and putting them in Domain keeps Application handlers thin. Do not move them.
- jsonb columns are mapped with `.HasColumnType("jsonb")`. Do not map them as `text`; you lose every Postgres jsonb feature.
- The `SchemaVersion` on `Activity` is pinned at creation. A later version bump on the `ActivityType` does not invalidate existing activities — they render against their pinned schema forever.
- Do not try to enforce schema conformance via SQL `CHECK` constraints. jsonb is not strict enough for that and the parser is the authority.
- The `ActorRule` grammar will grow. Keep it tiny for now: `subject`, `creator`, `role:<name>`, `scope:same_speciality`, `+` for conjunction, `|` for disjunction. Anything more is YAGNI until a seeded activity type actually needs it.
