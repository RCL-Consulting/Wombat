# T122 — Every CPSA tool can credit every CPSA EPA, because the EPA→tool mapping is parsed by nothing

**Status:** open
**Surfaced:** 2026-09-19, auditing v11.1 completeness after the first evidence run (T118).
**Severity:** Medium-High — not wrong *data*, but unenforced *rules*. A trainee can satisfy PAED-005
("Providing neonatal care in intensive and high-care settings", which v11.1 permits **CBD, DOPS and MSF**
only) entirely with Mini-CEXs, and the product will call the curriculum item complete. The College
published a tool allow-list per EPA and Wombat ignores it.

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
   so the transcription in `Rewrite/Tasks/T098-data/annexure-a.json` (which stays verbatim and untouched)
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

- A `cbd_cpsa` filed against PAED-001 (which permits CBD) submits and credits exactly as it does today.
- A `mini_cex_cpsa` filed against PAED-005 (CBD, DOPS, MSF) is refused at submit with a message naming
  those three, and PAED-005 was never offered in that tool's EPA picker in the first place.
- A curriculum-2 trainee is unaffected in every respect: no allow-lists, no tool keys on the legacy types,
  no narrowing, no refusals.
- An activity type with no `WbaToolKey` credits every EPA in the subject's curriculum, as today.
- Booting against the existing dev database backfills fifteen allow-lists and four tool keys, and leaves a
  CollegeAdmin's locally added item untouched.
- Cloning curriculum 3 to a new version carries all fifteen allow-lists.
- A second boot changes nothing — the canonical form round-trips and the backfill is `is null`-gated.

## Related

T098 phase 2b. Sits beside [T123], which narrows the *tool* list per trainee while this narrows the *EPA*
list per tool; the two compose, and both must share the fall-through-permissively discipline or they will
empty a picker between them. Independent of T098 phase 3 (the annual quota) and of `observed_on`, neither
of which this task touches. Makes the MSF gap (`src/Wombat.Domain/MultiSourceFeedback/`, no `EpaId`, no
credit path, `CreditApplier.ApplyAsync` takes an `Activity`) visible on all fifteen EPAs rather than
invisible on all fifteen.
