---
id: T122
title: "Every CPSA tool can credit every CPSA EPA, because the EPA→tool mapping is parsed by nothing"
status: done
priority: P1
created: 2026-09-19
started: 2026-09-23
completed: 2026-09-23
---
# T122 — Every CPSA tool can credit every CPSA EPA, because the EPA→tool mapping is parsed by nothing

**Status:** done 2026-09-23 — see **As built** below.
**Surfaced:** 2026-09-19, auditing v11.1 completeness after the first evidence run (T118).
**Severity:** Medium-High — not wrong *data*, but unenforced *rules*. A trainee can satisfy PAED-005
("Providing neonatal care in intensive and high-care settings", which v11.1 permits **CBD, DOPS and MSF**
only) entirely with Mini-CEXs, and the product will call the curriculum item complete. The College
published a tool allow-list per EPA and Wombat ignores it.


> **Note from [T130], 2026-09-23.** `PaediatricCatalogueSeeder.EpaSeed` is now `internal` and carries
> `ObservationsPerSemester`; `Wombat.Infrastructure.Tests/Persistence/PaediatricCatalogueQuotaSeedTests.cs` holds a guard
> that fails on any catalogue key neither deserialized nor allow-listed, and `wbaTools` is on its allow-list until
> this task moves it. For correcting existing rows, T130 chose a **one-off, guarded migration UPDATE plus a startup
> warning**, not an update pass in `EnsureCurriculumAsync`: a reconcile on every boot silently reverts an
> administrator who set a value on purpose. The same argument applies to `PermittedToolsJson`.

## Symptom

`src/Wombat.Infrastructure/Persistence/Seeds/paediatric-epa-v11.1.json` carries `wbaTools` on all fifteen
EPAs — the transcribed `tools` column of Annexure A. Nothing in `src/` reads it:

- `PaediatricCatalogueSeeder.EpaSeed` (`PaediatricCatalogueSeeder.cs:397-405`) declares eight
  `JsonPropertyName` members and `wbaTools` is not one of them, so `System.Text.Json` drops it silently.
- `BuildCurriculumItem` (`:350-367`) therefore cannot write it, and `CurriculumItem` has nowhere to put it.
- The only reader in the repository is a test —
  `tests/Wombat.Application.Tests/Features/Epas/PaediatricCatalogueSeedTests.cs:151`, which asserts that
  every EPA names MSF. It reads the JSON file off disk, not the database.

So the credit path never sees it. `CreditApplier.ResolveCurriculumItemsAsync` (`CreditApplier.cs:246-285`)
matches on `EpaId` alone; the tool that produced the encounter is not a term in the predicate at any point.
`mini_cex_cpsa`, `dops_cpsa`, `cbd_cpsa` and `direct_observation_cpsa` all declare the identical credit rule
(`Seeds/<key>/credit.json`: `epa_field: "epa_id"`, `minimum_level_field: "overall_level"`), so all four are
interchangeable against all fifteen EPAs.

## What v11.1 actually says, and what it does not

Annexure A's `tools` cell is **free text** — an allow-list of instrument names with a count in parentheses:

```
PAED-005  "CBD, DOPS, MSF (3)"
PAED-007  "Mini-CEX, DOPS, Directly observed clinical examination, MSF (4)"
PAED-010  "MSF, Direct observation (2)"
```

Two things follow, and an implementer must not invent either of them:

1. **The number in parentheses is the count of names, not a quota.** There is no per-tool sub-quota
   anywhere in the source document. The per-annum frequency (`observationsPerYear`, Annexure A's
   `frequency` column) is the only volume rule, and it is T098 phase 3's problem, not this task's. The
   allow-list is purely permissive: it says which instruments may credit this EPA, never how many of each.
2. **Fourteen distinct names appear across the fifteen EPAs**, of which Wombat seeds four:
   `mini_cex_cpsa`, `dops_cpsa`, `cbd_cpsa`, `direct_observation_cpsa`. The other ten are unbuilt
   (`current_state.md:104-108`). MSF is the worst of them — required by all fifteen EPAs, with
   `src/Wombat.Domain/MultiSourceFeedback/` holding no `EpaId` and no credit path at all. **Enforcing the
   mapping does not create that gap; it makes it explicit**, which is the right direction. Until MSF can
   credit, every EPA's allow-list contains one entry the product cannot satisfy — and that will be visible
   rather than invisible.

The full vocabulary, in first-appearance order: DOPS, Mini-CEX, CCA, CBD, MSF, RCA, Clinical audit,
Reflective exercise, Direct observation, Chart-stimulated recall, Case note review, Directly observed
clinical examination, Learner feedback, Portfolio and logbook review.

## Design

### Where the mapping lives: `CurriculumItem.PermittedToolsJson`

Annexure A's row is `(EPA, frequency, Y1–Y4 minima, tools)`. Three of those four cells already live on
`CurriculumItem`: `RequiredCount`, `MinimumLevelOrder` + `MinimumLevelByStageJson`, and the `ScaleId` that
T109 added to say which ladder the minima are on. **The tools cell is the fourth cell of the same published
row**, and splitting it onto `Epa` would put half of one table in a different entity with a different
lifetime.

That lifetime difference is the decisive argument, not the aesthetic one. `Epa` rows are keyed
`(SubSpecialityId, Code)` (`EpaConfiguration.cs:22-24`) and are *shared between curriculum versions* — a
v11.2 curriculum would point its items at the same fifteen `Epa` rows. A tool list on `Epa` is therefore
retroactive: editing it for v11.2 rewrites what v11.1 permitted, for every trainee still pinned to v11.1.
`CurriculumItem` is per version, and `Curriculum.CloneAsNewVersion` (`Curriculum.cs:31-46`) copies items
forward explicitly. This is the same reasoning T109 used to put `ScaleId` on the item rather than the EPA,
and the same reasoning that gives T091's institution-local extras their own allow-list without touching
national core.

**Not a join table.** `CurriculumItemPermittedTool(CurriculumItemId, WbaToolId)` is the relational purist's
answer and it buys FK integrity on the tool id. It costs a fourth table in the credit path's hot query, its
own migration, its own admin plumbing and its own cascade rules, for a set of at most fourteen short keys
that is always read as a unit and never queried from the other side. `MinimumLevelByStageJson`
(`CurriculumItem.cs:22`) is the house precedent for exactly this shape.

**The jsonb rule applies.** Per `CLAUDE.md`: never compare a stored `jsonb` against serializer output as a
raw string — Postgres re-renders its own text. Canonicalise both sides. `NormalizeStageOverridesJson`
(`CurriculumItem.cs:112-126`) is the pattern to copy: parse, sort, re-serialise, and store the canonical
form. Store the keys sorted so a seed-vs-stored comparison is stable.

### How a tool is identified: a seeded `WbaTools` table plus `ActivityType.WbaToolKey`

- `WbaTools` — a small seeded reference table: `Id`, `Key` (stable, lowercase, e.g. `mini_cex`, `cbd`,
  `msf`, `chart_stimulated_recall`), `Name` (the annexure's display name), `Description` (nullable).
- `ActivityType.WbaToolKey` — a new nullable column on `ActivityType` naming which instrument this type
  *is*. Null means "not a recognised WBA instrument", which is true of `reflective_note`, `procedure_log`,
  `journal_club`, `qi_project`, `research_output`, `teaching_session` and every type an institution builds
  without saying otherwise.
- `CurriculumItem.PermittedToolsJson` stores **keys**, not ids. The catalogue seed file is national data
  that boots dev, production and any future institution's instance; a `WbaTools.Id` sequence is per
  deployment and would make the same JSON mean different things in different databases.

**Why not identify a tool by the activity-type key, or by its prefix?** The prefix convention already
exists — `GetEpaTrajectoryForTraineeQuery.SourceByActivityFamily` (`:77-90`) matches a family by exact key
or `"<family>_"` prefix, and `PaediatricCatalogueSeeder`'s own remarks (`:76-90`) explain that the suffix
in `mini_cex_cpsa` is load-bearing for precisely that reason. But that same file records why it is not good
enough: *"an institution that builds its own rated tool through the Activity builder under an unfamiliar key
will not chart. The real fix is a flag on ActivityType marking it as producing an entrustment rating."*
`WbaToolKey` **is** that flag. Making the mapping depend on it retires the hard-coded family list rather
than adding a second copy of it that can drift from the first.

It also avoids the hazard T109 spent two sections on: `scale_key` binds a schema to a scale **by exact
name**, so renaming a scale silently unbinds every schema that names it — which is why
`UpdateEntrustmentScaleCommandHandler` and `DeleteEntrustmentScaleCommandHandler` now have to refuse those
operations outright. Do not build a second name-binding. A tool key is chosen once from a list and never
inferred from a string the operator was free to type.

**Why not an FK from `CurriculumItem` to `ActivityTypes`?** Activity types are per deployment,
operator-creatable, and scope-bound to one institution or speciality. A national curriculum item cannot
reference one institution's row. `mini_cex_paed` (`Scope = Institution, ScopeId = 2`, operator-built — see
T104) is the general case, and the seeded CPSA four are the exception, not the rule.

### Where it is enforced: one predicate, two call sites — the T108 shape

A shared, pure evaluator in `src/Wombat.Application/Features/Activities/Services/` next to
`CreditRuleFields.cs` — T108 put that file there after adversarial review caught a `.razor` reaching into
Domain, and mirror-the-engine logic belongs one layer from the engine, not in a component:

```
ToolPermission.Evaluate(IReadOnlyList<string>? permittedToolKeys, string? activityTypeToolKey)
    -> NotRestricted | Permitted | NotPermitted
```

`NotRestricted` when the curriculum item declares no allow-list **or** the activity type declares no
`WbaToolKey`. This is the load-bearing default and it is the same discipline T109's `Unpinned` basis
established: curriculum 2's fifteen items have no allow-list, every institution-built curriculum has none,
and every builder-made type has no tool key. **All of them behave exactly as they do today. Nothing that
credits today stops crediting because of this change.**

Two callers:

1. **`ActivityReferenceDataService.GetEpaOptionsAsync`** (`:80-146`). `EpaOptionScope` gains `ToolKey`;
   when the narrowing path is already active (T108's three gates), intersect the creditable EPA id set with
   the items whose allow-list admits this tool. The stored-value union (`WithStoredValueAsync:213-249`)
   must keep working unchanged — an `epa` field is a `<select>` and dropping a stored value erases recorded
   evidence rather than narrowing a choice.
2. **`ActivityService`**, beside the existing `ThrowIfInvalid(_schemaValidator.Validate(...))` calls
   (`CreateDraftAsync:109`, `TransitionAsync:203-207`). The schema validator is a pure Domain service with
   no database, so the check cannot live inside it; `ActivityService` already does the EF lookups and
   already turns `ActivityValidationErrorDto` into a field-keyed message (`ThrowIfInvalid:672-684`). Emit
   the error against the `epa` field key so it renders next to the picker that caused it, naming the
   permitted instruments.

Refusing at submit is safe for the assessor. The Entrustment and Feedback sections of every CPSA schema are
`editable_by: field:assessor_user_id` (`Seeds/mini_cex_cpsa/schema.json`), so the EPA is chosen and frozen
by the trainee before the assessor ever opens the form; a refusal lands on the person who can still fix it.

### 🔶 DECISION 1 (maintainer) — the enforcement point

| Option | What it does | Cost |
|---|---|---|
| **a. Filter the EPA picker only** | `GetEpaOptionsAsync` stops offering EPAs this tool may not credit | Cheapest; the plumbing exists. **Unenforced**: an activity created through `Wombat.Api`, an in-flight draft filed before the mapping landed, or a type matching on `curriculum_item_field` rather than `epa_field` all walk straight past it |
| **b. Reject at credit time** | `CreditApplier` refuses the item match when the tool is not permitted | Refuses at the one moment nobody can act: the encounter happened, the assessor rated it, and the activity is terminal. It also drops into T108's zero-credit banner, whose copy points the reader at *"an administrator about your curriculum"* — misleading here, because the curriculum is correct and the tool was wrong |
| **c. Credit but warn** | A fourth `LevelComparisonBasis` beside T109's `ScaleMismatch`: count volume, refuse the minimum | Honest that the encounter happened, but it lets a portfolio fill with volume no College rule recognises, and it needs a new counter, a new stamp and a new banner |
| **d. Reject at submit, picker agrees** | The predicate above, wired to both call sites | One predicate, two callers, a message the person can act on. Needs the write-path check as well as the picker |

**I would choose (d), with `CreditApplier` deliberately *not* re-litigating.** An allow-list can be edited
after an encounter was filed and completed; refusing credit retroactively would delete evidence a trainee
legitimately collected under the rule in force at the time. The gate belongs on the write path, and the
engine credits what the write path let through — the same reasoning T109 used for *"it never throws"*
(`CreditApplier.ApplyAsync` runs after `ApplyTransition` has already mutated state, before the single
`SaveChangesAsync` at `ActivityService.cs:229`; a throw there rolls back a completed, schema-valid
assessment).

Explicitly **not** (a) alone. T108 already shipped a "this credited nothing" banner, so a silent no-credit
path exists — and a defect that resolves into that banner is a defect that tells the reader the wrong thing.

### 🔶 DECISION 2 (College / CPSA content owner) — the tool vocabulary

Annexure A's fourteen names contain pairs that may denote one instrument:

- **"Direct observation"** (PAED-002, 006, 009, 010, 011, 013, 015) vs **"Directly observed clinical
  examination"** (PAED-007 only).
- **"CCA"** vs **"Case note review"** (PAED-006 only) — both arguably chart-based case analysis, and
  `GetEpaTrajectoryForTraineeQuery:86-88` already files `cca`, `rca` and `case_note_review` under one
  "Case analysis" label.
- **"Reflective exercise"** (PAED-001, 003, 008, 014), which Wombat seeds as `reflective_note` — a
  trainee-authored type with `counts_for: []` that cannot credit anything at all.

Seed all fourteen verbatim, one row per published name, or collapse to a canonical set with aliases?

**Consequence of seeding verbatim, stated plainly:** the seeded `direct_observation_cpsa` tool declares
`direct_observation`, so it would be permitted on PAED-006 and PAED-009 and **refused on PAED-007**, whose
cell says "Directly observed clinical examination". That may be exactly what the College means, or it may
be an artefact of one document written by several hands.

**I would seed all fourteen verbatim** and ask the CPSA content owner to merge explicitly, because a merge
Wombat invents is invisible and unauditable, while a merge the College makes is recorded in the catalogue
file. The mapping's whole value is that it can be checked against the published annexure line by line.

### 🔶 DECISION 3 (maintainer) — what an unrecognised tool may do

A `WbaToolKey` of null means "not a recognised instrument", and under the design above that is
`NotRestricted`: an institution's home-built rated tool credits every EPA in the curriculum, exactly as
today. The alternative reading is that an EPA with an allow-list admits *only* listed tools, so an
unrecognised tool is refused.

**I would keep it permissive**, for the reason T108 and T109 both landed on: a restriction that fires where
the answer is unknown produces an empty picker or a silent refusal, and both are worse failures than the
one being fixed. Revisit once the builder offers the tool picker and institutions have had a release to set
it.

## The seeder work — the half that silently does nothing if you skip it

1. `EpaSeed` (`:397-405`) gains
   `[property: JsonPropertyName("wbaTools")] IReadOnlyList<string> WbaTools`.
2. `BuildCurriculumItem` (`:350-367`) writes the canonical `PermittedToolsJson`.
3. **A backfill pass, or steps 1–2 change nothing on any existing database.** `EnsureCurriculumAsync`
   skips any item whose `EpaId` is already present (`:310-319`), so on dev and on production — both of
   which already hold curriculum 3 — the fifteen items exist and are never rebuilt. There is **no
   refresher for the catalogue JSON**: `ActivityTypeSeedRefresher` (T103) evolves activity-type seed
   folders only. Copy the shape of T109's pin at `:333-347` verbatim: build `seededEpaIds` from the
   catalogue, then
   `foreach (var item in curriculum.Items.Where(e => e.OwningInstitutionId is null && e.PermittedToolsJson is null && seededEpaIds.Contains(e.EpaId)))`.
   Scope to `seededEpaIds` for the same reason T109 gives — a CollegeAdmin's own added national item was
   authored against a tool set this seeder cannot know, and asserting one would be a lie with teeth.
4. `EnsureActivityTypesAsync` (`:392-427`) `continue`s past any key that already exists, so the four
   seeded CPSA types will never receive their `WbaToolKey` on an existing database either. Add an
   idempotent pass that sets `WbaToolKey` where it is null for the seeded keys. The refresher cannot do
   it: `ActivityTypeSeedCatalogue`'s own remarks (`:63-66`) state that it "never touches scope, name or
   description — it evolves the four versioned JSON payloads and nothing else", and `WbaToolKey` is a
   column on `ActivityType`, not one of the four payloads.
5. `Curriculum.CloneAsNewVersion` (`:34-46`) must copy `PermittedToolsJson`, or every new curriculum
   version silently loses its mapping one version at a time. That is word for word the hazard the existing
   comment at `:42-45` describes for `ScaleId`.
6. The seed file's `wbaTools` changes from display names to tool keys, and `WbaTools.Name` carries the
   display name. `PaediatricCatalogueSeedTests.cs:151` asserts `"MSF"` is present on every EPA; it becomes
   an assertion on `"msf"`, and the audit against the published annexure moves to an explicit name↔key map
   so the transcription in `execution/tasks/done/T098-data/annexure-a.json` (which stays verbatim and untouched)
   remains checkable against the seed.

## Migration

`CurriculumItems.PermittedToolsJson jsonb NULL`, `ActivityTypes.WbaToolKey text NULL`, and the `WbaTools`
table. Model it on `20260918074040_T109_EntrustmentScalePinning` — Designer file **and**
`ApplicationDbContextModelSnapshot` update, per `CLAUDE.md`, or `MigrateAsync()` skips it without a word.

**The migration backfills nothing**, deliberately and for T109's reason: a wrong allow-list is not
symmetric with an absent one. Absent means "unrestricted, exactly as today"; wrong means a tool the College
does permit is refused at submit, and the trainee cannot tell why. Values arrive by provenance only, from
the seeder that reads the published catalogue, and from an administrator who chooses them deliberately.

## Admin surfaces

`CurriculumItemsEdit.razor` gains a **Tools** cell and a checkbox group over `WbaTools`, defaulting to
none-selected = unrestricted. The plumbing is the same list T109 walked and it is short:
`CurriculumItemDto` (`CurriculumDto.cs:29`), `AddCurriculumItemCommand` / `UpdateCurriculumItemCommand`
(`ManageCurriculumItems.cs:19,31`), both handlers (`:143-154`, `:207-215`), `CurriculumMappings.cs:38`, and
both `GetCurricula` projections (`:75`, `:111`). National core stays read-only to an InstitutionalAdmin
exactly as it is now.

The activity-type builder gains a **"This tool is"** picker over `WbaTools` writing
`ActivityType.WbaToolKey`, with an explicit "Not a WBA instrument" option that is the default. Unsetting it
must be allowed: an administrator who becomes unsure must be able to withdraw the assertion, which is the
rule T109 adopted for unpinning a scale.

## Verification

Rewritten 2026-09-23. The original list assumed a boot-time backfill, "four tool keys" and "curriculum 3", all of which
changed; see **As built**. Each item names its evidence.

- [x] **A tool the list permits submits and credits as before.** A `cbd`-keyed type against an item listing `cbd` goes
      create → submit → complete and credits one row — `ToolPermissionGateTests`. In the browser, the CBD picker on
      dev offers PAED-005.
- [x] **A Mini-CEX against PAED-005 is refused, and PAED-005 was never offered.** The dev Mini-CEX picker offers exactly
      PAED-001–004, 006–008, 012 and 013. Submitting a pre-T122 Mini-CEX draft on PAED-005 (activity 13, staged with the
      old build) was refused in the browser with "EPA: Mini-CEX cannot be used as evidence for PAED-005 — Providing
      neonatal care in intensive and high-care settings. The curriculum accepts CBD, DOPS or MSF for this EPA." The
      activity kept its state and its one transition, and the audit row recorded the refusal. Repairing the EPA in the
      same submit passed (the gate reads the merged data).
- [x] **A curriculum without lists is unaffected.** Dev curriculum 1 (IM Core) has a null list. A keyed type on a
      curriculum with no lists narrows nothing and is refused nothing — `EpaOptionCreditScopeTests` (d),
      `ToolPermissionGateTests` (D21 cases).
- [x] **A type with no `WbaToolKey` credits every EPA, as before** (D21) — `ToolPermissionGateTests`,
      `ToolPermissionTests`. Also unrestricted: a null, empty or malformed list; a subject with no profile; an EPA with no
      item.
- [x] **An existing database is stamped once, by the migration.** On dev: 15 allow-lists on curriculum 2, 8 tool keys
      (the five CPSA seeds plus the generic Mini-CEX, DOPS and CBD), 12 vocabulary rows, and zero startup warnings.
      Decoys stay null: a local item, another version, a code outside the catalogue, an operator type on a seed key —
      `WbaToolAllowListPostgresTests`. A CollegeAdmin's own national item keeps a null list —
      `PaediatricCatalogueToolSeedTests`.
- [x] **Cloning a curriculum carries the lists** — `CurriculumCloneTests`.
- [x] **A second boot changes nothing.** On dev: zero warnings, and fingerprints of every item's list, every type's key
      and every vocabulary row are identical before and after. On Postgres: zero `PaediatricCatalogueSeeder` warnings
      after migrate-then-boot, including on a populated pre-T122 database — `WbaToolAllowListPostgresTests`.
- [x] **Credit never re-checks the list (D20).** In the browser: activity 13 was submitted while PAED-001 permitted
      Mini-CEX, then Mini-CEX was unticked on PAED-001 in the admin editor. The assessor's complete still credited "1 item".
      `/admin/curriculum-progress` then rebuilt the tallies byte for byte, keeping activity 11 (Mini-CEX, PAED-011, whose
      list excludes it) and every PAED-001 Mini-CEX — `ToolPermissionGateRuleShapeTests` (rebuild).
- [x] **A forbidden draft can still be withdrawn.** Activity 14 (Mini-CEX, PAED-010) was cancelled in the browser; the
      move into a dead end is exempt — `ToolPermissionGateTests`.
- [x] **The final build, re-verified in the browser after the gate rule was reshaped.** Activity 15 (a Mini-CEX draft on
      PAED-001, with Mini-CEX then removed from PAED-001's list) was refused at submit: "…The curriculum accepts CBD,
      CCA, Clinical audit, DOPS, MSF, RCA or Reflective exercise for this EPA". It was then cancelled. A third boot logged
      zero warnings.
- [x] **The admin surfaces.**
  - The curriculum item editor shows a Tools column and a Tools fieldset per item. In the browser, unticking and then
    restoring Mini-CEX on PAED-001 worked, and an unchanged save of PAED-005 kept its list, window, target, pin and minima.
  - The builder shows "This tool is: Mini-CEX" for `mini_cex_cpsa`. A save kept the key, and discarding the draft left it
    — `CurriculumItemsToolListTests`, `ActivityTypeToolPickerTests`, `CurriculumItemPermittedToolsTests`,
    `SaveActivityTypeDraftWbaToolKeyTests`.
- [x] **Every refusal leaves the DbContext clean (the audit trap)** — every refusal test in the classes above asserts no
      Added/Modified/Deleted entries and a zero-row audit save.

## As built (2026-09-23)

**Decisions.** D20 and D21 are closed as recommended (EPA-PROGRAMME § 3C). D4 and D12 are applied. The one open College
question, whether "clinical observed interaction" merges Mini-CEX with Direct observation, is § 3F question 5; the two
are seeded as separate instruments.

**Model.**
- `WbaTools` holds 12 keys, in `Wombat.Domain.Epas.WbaTool`. `ActivityType.WbaToolKey` is live and unversioned: the
  builder writes it on save, not publish, and Discard does not undo it.
- `CurriculumItem.PermittedToolsJson` is jsonb holding a sorted key array; null means any instrument and `[]` is never
  stored. `Curriculum.CloneAsNewVersion` carries it.
- `Workflow.HasOutgoingTransition` is the one dead-end test for field writes. `Workflow.CanReachTerminal` is the
  gate's reachability test.

**Where it is enforced.**
- `ToolPermission.Evaluate` is the one predicate. `ToolPermissionGate` uses `CreditTargetResolver`, which was extracted
  verbatim from `CreditApplier`, so the gate and the engine resolve the same items.
- The gate runs in these places:
  - **create:** always, whoever files;
  - **`UpdateDraftAsync`:** when the credit target changes and credit can still follow from the current state;
  - **`TransitionAsync`:** when credit can still follow the move AND either the target changes, or the target is
    unchanged but a refusal is something the mover can act on;
  - **`StageCompletedAsync`:** never, since it refuses crediting types already.
- **Judged per credit directive, on D20's own reason.** A changed target is judged wherever credit can still follow.
  An unchanged target is re-checked only when the author hands it on while still able to correct it. That needs four
  things:
  - the mover is the subject or the creator;
  - nobody else has acted yet;
  - the mover can write that directive's field now (`IFieldPermissionEvaluator`);
  - the move hands the target on: credit can follow without coming back through the state it left
    (`Workflow.CanReachTerminal`), or the mover loses write access to the field, which is checked on a detached probe
    activity.
  A literal `curriculum_item_id` directive is judged only at create.
  `CreditTargetResolver.DescribeTarget` is the one statement of the engine's precedence, and both the engine and the
  gate use it.
- Consequences: a pre-T122 draft is refused when the trainee submits it. An assessor's completion, a trainee's
  sign-off after assessment, a resubmission after a decline, a withdrawal into a holding state and a reassign are
  never refused for an unchanged target. "Changed" means the engine would resolve the target differently, so `"6"`
  and `6` count as unchanged.
- The picker intersects T108's creditable set with the gate and never empties.

**Divergences from the design in this file, and why.**
1. **A one-off guarded migration UPDATE, not an `is null` boot pass, and not "backfills nothing."** This is the T130 note
   at the top of this file. Null means both "never set" and "an admin cleared it", so a boot reconcile would undo a
   deliberate clear. Afterwards the seeders warn and never write. The migration's frozen arrays are pinned to a
   hand-typed snapshot, never to the live catalogue.
2. **The generic `mini_cex`, `dops` and `cbd` are keyed** (the design said "no tool keys on the legacy types"). They are
   those instruments, and unkeyed they would be unrestricted on every EPA with a list. No generic curriculum has a list,
   so a curriculum-2-style trainee is unaffected. `acat` is not a College name and stays null.
3. **The catalogue keeps Annexure A verbatim.** Per EPA, `annexureTools` holds the cell and `wbaTools` holds the keys.
   A root `wbaToolVocabulary` records `annexureNames` and the alias notes, so the D4 alias lives where a reader of the
   annexure looks. `PaediatricCatalogueToolSeedTests` resolves one into the other and fails on an unclaimed name.
4. **The refusal is one page-level sentence per refused item, led by the field's label**, not an error beside the
   picker. No per-field error plumbing exists. The `ThrowIfActorFieldNamesSubject` precedent names the label, not
   `epa_id`.
5. **Adjacent fix required by this task's own write.** `SaveActivityTypeDraftCommandHandler` added a new type before
   its scope guard, and wrote metadata before the DSL parse. Through the audit pipeline's catch, a refused create
   committed a blank Global type that squatted the key, and a malformed draft committed a rename. The handler now
   checks everything first, and `ActivityType.SaveDraft` parses all four payloads before assigning any —
   `SaveActivityTypeDraftWbaToolKeyTests`, `ActivityTypeSaveDraftAtomicityTests`.
6. **`CurriculumItemsEdit` has separate edit and add models.** They used to share one, so opening a row silently
   filled the Add form.
7. **Not done, filed instead.**
   - Classifying rated sources by `WbaToolKey` ([T144]).
   - The inert legacy `FormEpaLink` screen ([T145]).
   - A crediting type from another discipline can credit a trainee's curriculum ([T146]). The design critique proposed
     fixing this here, but on dev [T123] d3 already hides the rated generics from a CPSA-pinned trainee, and the rule
     needs its own decision.
   - CampaignEdit's dangling `label for` ([T147]).
8. **The known boundary (D20).** The gate evaluates the curriculum the subject is on at the gated write. A subject with
   no profile passes, and a profile created or re-pointed before completion is not re-checked. Pinned by
   `ToolPermissionGateTests`.
9. **No changes needed:**
   - `UpdateDraftAsync` still has no production caller (T106 item 1); it is gated on a target change regardless.
   - The scenario runbooks: `scenario-paediatrics.md` files a DOPS against PAED-010 with the operator-built `dops_paed`,
     which is unkeyed and so unrestricted.
   - DESIGN.md gained the checkbox-group pattern (`.check-grid`).

**Found by the test agents and fixed.** `ToolPermission.Evaluate` judged an all-blank list non-empty and refused
every instrument. It was latent, because every caller passes parsed lists. The menu's profile pick also lacked the
resolver's `Id` tie-break.

**Found by the adversarial review and fixed.** The review ran six finders, deduplicated the findings, then gave each
three refuters.
- **An author's recall into the draft phase was gated like a submission** (builder shapes only). The trainee could not
  pull back a request whose EPA had been closed, while the assessor's unchanged completion credited it. Fixed with
  reachability: a move counts as a submission only if credit can follow it without passing back through the initial
  state.
- **A refusal naming many items could overflow the audit row's 2,000-character error text.** The audit save then
  failed and masked the refusal. The gate now names at most three items, and `AuditEntry.Create` truncates to the
  column for every command.
- **One builder shape stays refused on purpose:** an author's `cancel` OUT OF THE DRAFT into a state the assessor can
  reopen straight into assessment. There, credit can follow with no further author move. Pinned by a test.

**A second review round reshaped the rule.** It ran three finders with three refuters each, and confirmed 7 findings.
Five showed that recognising "the author's submission" from actor-rule shapes cannot be made right:
- a trainee's sign-off after assessment was re-gated, which could strand the assessed encounter (major);
- withdrawals into holding states, and author moves that stay with the assessor, were refused;
- a `role:Trainee` submit, the scenario runbooks' shape, was never gated;
- a legacy-shaped resubmission and a draft-initial one were treated differently.
The rule became "an unchanged target is checked at create and at the handover, and never again". Each shape has a
test, and the rule was mutation-checked.
The other two findings were documentation gaps, both fixed:
- the three-item cap was misdescribed;
- nothing said that a builder key change on a seeded type parks a draft, which the refresher then skips.
The save command also now runs both scope guards before any key check.

**A third review round finished the job.** It ran two finders with three refuters each, and confirmed 8 findings:
- **Still syntax-bound (major, builder-only):** the handover's named-person exemption still read the actor rule's
  syntax. A legacy `accept` with a fallback approver (`field:…|role:Coordinator`) was re-checked after create and
  stranded, and an assessor could pick a draft up unchecked.
- The final rule stops reading syntax and states D20's reason directly: refuse only a mover who can act on the
  refusal, and only before anyone else has acted. Its three conditions are each mutation-pinned (5, 5 and 1 tests fail
  when one is removed).
- **Pre-existing bug, filed as [T148] (P2):** Submit on `/activities/new` for a requested-born type sends the only
  author transition left, `cancel`, so the encounter is withdrawn as it is filed.
- The rest were test and comment wording that described superseded rules; all fixed.

**A fourth review round (two finders, three refuters each) confirmed 11 findings.** Three changed the rule, and the
rest were doc wording:
- **The mover's identity was never checked (major).** The rule never asked who was making the move, so an assessor
  allowed to correct the EPA was refused at an unchanged completion.
- **Every directive was judged (major).** A multi-directive rule was judged on every directive, so a refusal could
  name a field the mover cannot write.
- **Loop-back handovers slipped through.** A handover whose credit path comes back through the draft went unchecked.
The gate is now judged per directive with the mover-is-the-author condition, and each fix is mutation-pinned.
**A fifth round came back nearly dry.** It had two finders, one on the rule's principle and one on engine parity
after `CreditTargetResolver` moved onto `DescribeTarget`, each with three refuters. Engine parity found nothing, and
one minor builder-only finding survived on a 2–1 vote: a withdrawal out of the draft into a holding state the author
cannot edit counts as a hand-on. It is kept as a documented, pinned residual. The refusal lands on the author, who
can act on it, so the principle holds, and any narrower test would read actor-rule syntax again. Across the five
rounds the confirmed findings went 2 → 7 → 8 → 11 → 1, with severity falling; the loop was stopped there.

**Tests.** 1,555 unit tests, up from 1,195, plus 6 new Postgres tests. New classes:
- Domain: `WbaToolTests`, `CurriculumItemPermittedToolsTests`, `WorkflowDeadEndTests`, `WorkflowReachabilityTests`,
  `ActivityTypeSaveDraftAtomicityTests`, `AuditEntryErrorMessageTests`.
- Application: `ToolPermissionTests`, `ToolPermissionGateTests` (44), `ToolPermissionGateRuleShapeTests` (21),
  `SaveActivityTypeDraftWbaToolKeyTests`, `CurriculumItemPermittedToolsTests`, `GetWbaToolsQueryTests`. Additions to
  `CurriculumCloneTests` and `ListActivityTypesNarrowingTests`.
- Infrastructure: `PaediatricCatalogueToolSeedTests` (37); 30 additions to `EpaOptionCreditScopeTests`.
- Web (38 new): `WbaToolKeyThreadingTests`, `CurriculumItemsToolListTests`, `ActivityTypeToolPickerTests`; additions to
  `EpaPickerScopeTests`.
- Integration: `WbaToolAllowListPostgresTests`.

Several agents mutation-checked their classes by breaking src temporarily and restoring it; every mutant was caught.

## Related

T098 phase 2b. Sits beside [T123], which narrows the *tool* list per trainee while this narrows the *EPA*
list per tool; the two compose, and both must share the fall-through-permissively discipline or they will
empty a picker between them. Independent of T098 phase 3 (the annual quota) and of `observed_on`, neither
of which this task touches. Makes the MSF gap (`src/Wombat.Domain/MultiSourceFeedback/`, no `EpaId`, no
credit path, `CreditApplier.ApplyAsync` takes an `Activity`) visible on all fifteen EPAs rather than
invisible on all fifteen.
