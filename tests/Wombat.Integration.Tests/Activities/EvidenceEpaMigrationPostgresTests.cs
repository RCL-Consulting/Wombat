using Wombat.Tests.Shared;
using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Activities;

/// <summary>
/// T137 on a real PostgreSQL server: the migration that gives existing stored schemas the <c>evidence_epa_field</c>
/// pointer and stamps <c>Activities.EpaId</c> through each activity's pinned version, and the trainee's list query, whose
/// LEFT join and correlated credit subquery the InMemory provider never translates.
/// </summary>
/// <remarks>
/// <para>
/// The migration's SQL is exercised nowhere else: the unit suites run on EF InMemory, which never runs a migration, and
/// on a fresh database the UPDATEs touch nothing, because the seeders run afterwards. The only databases they ever act on
/// are existing ones, so they are rehearsed here on a schema stopped at the last pre-T137 migration and filled in its
/// shape by raw SQL.
/// </para>
/// <para>
/// The schema helpers follow <c>WbaToolAllowListPostgresTests</c>: each test works in a schema of its own
/// (<c>SearchPath = it_&lt;guid&gt;</c>, nothing else on the path), registered before it is created and dropped in a
/// <c>finally</c> and again from <see cref="DisposeAsync" />.
/// </para>
/// </remarks>
public sealed class EvidenceEpaMigrationPostgresTests : IAsyncLifetime
{
    private const string LastMigrationBeforeT137 = "20260923105040_T122_WbaToolAllowLists";
    private const string SeedOwnerUserId = "seed-system";
    private const string TraineeId = "trainee-1";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task Migration_StampsEveryResolvableEpaThroughThePinnedVersion_AndOnlyThose()
    {
        try
        {
            var fixture = await ArrangePreT137DatabaseAsync();

            await MigrateToLatestAsync(fixture.Schema);

            var stamped = await EpaIdsAsync(fixture.Schema);

            stamped[fixture.Activities["mini-cex, epa as a string"]].Should().Be(fixture.HistoryEpaId);
            stamped[fixture.Activities["mini-cex, epa as a number"]].Should().Be(fixture.WardRoundEpaId);
            stamped[fixture.Activities["msf, which credits nothing"]].Should().Be(fixture.HistoryEpaId,
                "only the pointer can supply an EPA for a type that credits nothing");
            stamped[fixture.Activities["mini-cex, an id no epa has"]].Should().BeNull("the column never holds an id a join cannot resolve");
            stamped[fixture.Activities["mini-cex, not a number"]].Should().BeNull("and the guard stops the cast failing the migration");
            stamped[fixture.Activities["mini-cex, no epa yet"]].Should().BeNull();
            stamped[fixture.Activities["mini-cex, a sign and whitespace"]].Should().Be(fixture.HistoryEpaId,
                "the resolver's int.TryParse accepts a leading + and surrounding whitespace, so the migration does too");
            stamped[fixture.Activities["mini-cex, leading zeros"]].Should().Be(fixture.WardRoundEpaId);
            stamped[fixture.Activities["mini-cex, a no-break space"]].Should().BeNull(
                "NumberStyles.Integer allows only ASCII whitespace, so the resolver refuses it too");
            stamped[fixture.Activities["mini-cex, negative"]].Should().BeNull();
            stamped[fixture.Activities["operator type, epa under another key"]].Should().Be(fixture.HistoryEpaId,
                "its credit reads the_epa, an epa field, so the pointer is determined whatever its key");
            stamped[fixture.Activities["operator type, credit reads another epa field"]].Should().Be(fixture.WardRoundEpaId,
                "the pointer follows the field credit reads, not the field keyed epa_id");
            stamped[fixture.Activities["operator type, epa_id is text"]].Should().BeNull();
            stamped[fixture.Activities["operator type, credits a literal item"]].Should().BeNull(
                "a pointer beside an item-targeted directive could name another EPA than the one credited");
            stamped[fixture.Activities["operator type, sole epa field, credits nothing"]].Should().Be(fixture.HistoryEpaId);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task Migration_GivesThePointerOnlyToSchemasThatCanCarryIt_InEveryStoredColumn()
    {
        try
        {
            var fixture = await ArrangePreT137DatabaseAsync();

            await MigrateToLatestAsync(fixture.Schema);

            var pointers = await PointersAsync(fixture.Schema);

            pointers["mini_cex_cpsa"].Should().Be(new Pointers("epa_id", "epa_id", null));
            pointers["msf_cpsa"].Should().Be(new Pointers("epa_id", "epa_id", null));
            pointers["op_draft"].Should().Be(new Pointers(null, null, "epa_id"),
                "a draft in flight qualifies on its own staged credit, and its published version has no EPA field");
            pointers["op_other_key"].Should().Be(new Pointers("the_epa", "the_epa", null),
                "credit reads the_epa, an epa field: the one value EnsureCreditAgrees would accept");
            pointers["op_credit_reads_second_epa"].Should().Be(new Pointers("second_epa", "second_epa", null),
                "not epa_id, which credit does not read");
            pointers["op_sole_epa_no_credit"].Should().Be(new Pointers("related_epa", "related_epa", null),
                "a form that credits nothing and has one epa field is about that EPA");
            pointers["op_text_epa_id"].Should().Be(new Pointers(null, null, null));
            pointers["op_literal_item"].Should().Be(new Pointers(null, null, null),
                "EnsureCreditAgrees refuses a pointer beside an item-targeted directive");
            pointers["op_item_field_and_epa_field"].Should().Be(new Pointers(null, null, null));
            pointers["op_credit_reads_two_epa_fields"].Should().Be(new Pointers(null, null, null));
            pointers["op_credit_reads_a_number"].Should().Be(new Pointers(null, null, null),
                "the pointer must name an epa field");
            pointers["op_two_epas_no_credit"].Should().Be(new Pointers(null, null, null), "which one is not determined");

            // Every version the migration gave a pointer is one the app's own publish rule accepts.
            await using var db = NewContext(fixture.Schema);
            var patched = (await db.ActivityTypeVersions.AsNoTracking().ToListAsync())
                .Where(version => FormSchemaParser.Parse(version.SchemaJson).EvidenceEpaField is not null)
                .ToList();

            patched.Should().HaveCount(5, "mini_cex_cpsa, msf_cpsa, op_other_key, op_credit_reads_second_epa and op_sole_epa_no_credit");
            foreach (var version in patched)
            {
                var agrees = () => EvidenceEpa.EnsureCreditAgrees(
                    FormSchemaParser.Parse(version.SchemaJson),
                    Wombat.Domain.Activities.Credit.CreditRulesParser.Parse(version.CreditRulesJson));
                agrees.Should().NotThrow();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The seed folders gained the same pointer, so a seeded type the migration patched is already in sync with its
    /// folder: the refresher republishes nothing, and no in-flight activity is left on a version without the pointer.
    /// </summary>
    [Fact]
    public async Task AfterTheMigration_TheSeedRefresherFindsThePatchedSeedTypesInSync()
    {
        try
        {
            var fixture = await ArrangePreT137DatabaseAsync();
            await MigrateToLatestAsync(fixture.Schema);

            await using var db = NewContext(fixture.Schema);
            var results = await new ActivityTypeSeedRefresher(
                    db,
                    NullLogger<ActivityTypeSeedRefresher>.Instance,
                    Options.Create(new WombatOptions { RefreshSeededActivityTypes = true }))
                .RefreshAsync();

            results.Where(result => result.Key is "mini_cex_cpsa" or "msf_cpsa")
                .Should().OnlyContain(result => result.Outcome == ActivityTypeSeedRefreshOutcome.Unchanged)
                .And.HaveCount(2);

            (await db.ActivityTypeVersions.CountAsync()).Should().Be(
                await db.ActivityTypes.CountAsync(), "one version per type, and none was added");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// Down strips the pointer from EVERY schema, since the parser before T137 refuses an unknown root property, and
    /// clears the stamp it wrote.
    /// </summary>
    [Fact]
    public async Task Down_RemovesEveryPointerAndEveryStamp()
    {
        try
        {
            var fixture = await ArrangePreT137DatabaseAsync();
            await MigrateToLatestAsync(fixture.Schema);

            await using (var db = NewContext(fixture.Schema))
            {
                await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT137);
            }

            (await ScalarAsync<long>(fixture.Schema, """SELECT COUNT(*) FROM "Activities" WHERE "EpaId" IS NOT NULL"""))
                .Should().Be(0);
            (await ScalarAsync<long>(fixture.Schema, """
                SELECT (SELECT COUNT(*) FROM "ActivityTypeVersions" WHERE "SchemaJson" ? 'evidence_epa_field')
                     + (SELECT COUNT(*) FROM "ActivityTypes" WHERE "SchemaJson" ? 'evidence_epa_field')
                     + (SELECT COUNT(*) FROM "ActivityTypes" WHERE "StagingSchemaJson" ? 'evidence_epa_field')
                """)).Should().Be(0);
            (await ScalarAsync<long>(fixture.Schema, """SELECT COUNT(*) FROM pg_indexes WHERE indexname = 'IX_Activities_EpaId' AND schemaname = current_schema()"""))
                .Should().Be(0);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The trainee's list on PostgreSQL: the LEFT join to the EPA, and the latest-evaluated credit outcome as a
    /// correlated subquery. InMemory evaluates both in memory and would pass a query the server cannot run.
    /// </summary>
    [Fact]
    public async Task TheTraineesList_ProjectsTheEpaTheEncounterDateAndTheCreditOutcome()
    {
        try
        {
            var fixture = await ArrangePreT137DatabaseAsync();
            await MigrateToLatestAsync(fixture.Schema);

            var credited = fixture.Activities["mini-cex, epa as a string"];
            var uncredited = fixture.Activities["mini-cex, epa as a number"];
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(fixture.Schema))
            {
                await InsertTransitionAsync(connection, credited, "complete", new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc), 0);
                await InsertTransitionAsync(connection, credited, "rebuild", new DateTime(2026, 3, 11, 9, 0, 0, DateTimeKind.Utc), 2);
                await InsertTransitionAsync(connection, credited, "note", new DateTime(2026, 3, 12, 9, 0, 0, DateTimeKind.Utc), null);
                await InsertTransitionAsync(connection, uncredited, "complete", new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc), 0);
            }

            await using var db = NewContext(fixture.Schema);
            var rows = (await new ListActivitiesBySubjectQueryHandler(db, FakeUserDirectory.Empty).Handle(
                new ListActivitiesBySubjectQuery(
                    TraineeId,
                    new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, TraineeId)], "test"))),
                CancellationToken.None)).Items;

            rows.Should().HaveCount(fixture.Activities.Count);

            var creditedRow = rows.Single(row => row.Id == credited);
            creditedRow.EpaCode.Should().Be("PAED-001");
            creditedRow.EpaTitle.Should().Be("Take a history");
            creditedRow.ObservedOn.Should().Be(new DateOnly(2026, 3, 10));
            creditedRow.ObservedOnDeclared.Should().BeTrue();
            creditedRow.CreditedItemCount.Should().Be(2, "the latest move that evaluated credit decides; a later one that evaluated nothing does not");

            rows.Single(row => row.Id == uncredited).CreditedItemCount.Should().Be(0);
            rows.Single(row => row.Id == fixture.Activities["mini-cex, no epa yet"]).Should().Match<Wombat.Application.Features.Activities.Dtos.ActivitySummaryDto>(
                row => row.EpaCode == null && row.CreditedItemCount == null);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the pre-T137 database ----------------------------------------------------------------------------------------

    private sealed record Fixture(string Schema, int HistoryEpaId, int WardRoundEpaId, IReadOnlyDictionary<string, int> Activities);

    private sealed record Pointers(string? Published, string? Version, string? Staged);

    /// <summary>
    /// A schema stopped at T122, holding the two seeded types as they stood before T137 (their seed folders minus the
    /// pointer), operator types that pin each branch and guard of the pointer UPDATE, and activities pinned to each.
    /// </summary>
    private async Task<Fixture> ArrangePreT137DatabaseAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            await db.GetService<IMigrator>().MigrateAsync(LastMigrationBeforeT137);
            (await db.Database.GetPendingMigrationsAsync()).Should().ContainSingle(migration => migration.EndsWith("_T137_ActivityEvidenceEpa", StringComparison.Ordinal));
        }

        var activities = new Dictionary<string, int>(StringComparer.Ordinal);

        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);

        var collegeId = await InsertAsync(connection,
            """INSERT INTO "Colleges" ("Name", "ShortCode", "CreatedOn", "IsActive") VALUES ('CPSA', 'CPSA', TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE) RETURNING "Id" """);
        var specialityId = await InsertAsync(connection,
            """INSERT INTO "Specialities" ("CollegeId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, collegeId);
        var subSpecialityId = await InsertAsync(connection,
            """INSERT INTO "SubSpecialities" ("SpecialityId", "Name", "IsActive") VALUES ($1, 'Paediatrics', TRUE) RETURNING "Id" """, specialityId);

        var history = await InsertAsync(connection,
            """INSERT INTO "Epas" ("SubSpecialityId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, 'PAED-001', 'Take a history', 0, TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """,
            subSpecialityId);
        var wardRound = await InsertAsync(connection,
            """INSERT INTO "Epas" ("SubSpecialityId", "Code", "Title", "Category", "CreatedOn", "IsActive") VALUES ($1, 'PAED-002', 'Lead a ward round', 0, TIMESTAMPTZ '2026-01-01 00:00:00+00', TRUE) RETURNING "Id" """,
            subSpecialityId);

        var miniCex = await InsertSeedTypeWithoutPointerAsync(connection, "mini_cex_cpsa");
        var msf = await InsertSeedTypeWithoutPointerAsync(connection, "msf_cpsa");

        const string CreditsFromEpaId = """{"counts_for":[{"curriculum_item_match":{"epa_field":"epa_id"},"amount":1}]}""";
        const string CreditsNothing = """{"counts_for":[]}""";
        const string NoEpaSchema = """{"version":1,"sections":[{"key":"s","title":"S","fields":[{"key":"notes","type":"text","label":"Notes","required":false}]}]}""";

        const string EpaIdSchema = """{"version":1,"sections":[{"key":"s","title":"S","fields":[{"key":"epa_id","type":"epa","label":"EPA","required":false}]}]}""";
        const string TwoEpaFieldsSchema = """{"version":1,"sections":[{"key":"s","title":"S","fields":[{"key":"epa_id","type":"epa","label":"EPA","required":false},{"key":"second_epa","type":"epa","label":"Other","required":false}]}]}""";

        var otherKey = await InsertTypeAsync(connection, "op_other_key",
            """{"version":1,"sections":[{"key":"s","title":"S","fields":[{"key":"the_epa","type":"epa","label":"EPA","required":false},{"key":"epa_id","type":"text","label":"Legacy","required":false}]}]}""",
            """{"counts_for":[{"curriculum_item_match":{"epa_field":"the_epa"},"amount":1}]}""");
        var readsSecond = await InsertTypeAsync(connection, "op_credit_reads_second_epa",
            TwoEpaFieldsSchema,
            """{"counts_for":[{"curriculum_item_match":{"epa_field":"second_epa"},"amount":1}]}""");
        var textEpaId = await InsertTypeAsync(connection, "op_text_epa_id",
            """{"version":1,"sections":[{"key":"s","title":"S","fields":[{"key":"epa_id","type":"text","label":"EPA code","required":false}]}]}""",
            CreditsNothing);
        var literalItem = await InsertTypeAsync(connection, "op_literal_item",
            EpaIdSchema,
            """{"counts_for":[{"curriculum_item_match":{"curriculum_item_id":42},"amount":1}]}""");
        await InsertTypeAsync(connection, "op_item_field_and_epa_field",
            EpaIdSchema,
            """{"counts_for":[{"curriculum_item_match":{"epa_field":"epa_id","curriculum_item_field":"item_ref"},"amount":1}]}""");
        await InsertTypeAsync(connection, "op_credit_reads_two_epa_fields",
            TwoEpaFieldsSchema,
            """{"counts_for":[{"curriculum_item_match":{"epa_field":"epa_id"},"amount":1},{"curriculum_item_match":{"epa_field":"second_epa"},"amount":1}]}""");
        await InsertTypeAsync(connection, "op_credit_reads_a_number",
            """{"version":1,"sections":[{"key":"s","title":"S","fields":[{"key":"epa_id","type":"epa","label":"EPA","required":false},{"key":"epa_ref","type":"number","label":"EPA ref","required":false}]}]}""",
            """{"counts_for":[{"curriculum_item_match":{"epa_field":"epa_ref"},"amount":1}]}""");
        var soleEpa = await InsertTypeAsync(connection, "op_sole_epa_no_credit",
            """{"version":1,"sections":[{"key":"s","title":"S","fields":[{"key":"related_epa","type":"epa","label":"Related EPA","required":false}]}]}""",
            CreditsNothing);
        await InsertTypeAsync(connection, "op_two_epas_no_credit", TwoEpaFieldsSchema, CreditsNothing);
        await InsertTypeAsync(connection, "op_draft", NoEpaSchema, CreditsNothing,
            stagedSchemaJson: """{"version":1,"sections":[{"key":"s","title":"S","fields":[{"key":"epa_id","type":"epa","label":"EPA","required":false}]}]}""",
            stagedCreditRulesJson: CreditsFromEpaId);

        activities["mini-cex, epa as a string"] = await InsertActivityAsync(connection, miniCex, $$"""{"epa_id":"{{history}}"}""");
        activities["mini-cex, epa as a number"] = await InsertActivityAsync(connection, miniCex, $$"""{"epa_id":{{wardRound}}}""");
        activities["mini-cex, an id no epa has"] = await InsertActivityAsync(connection, miniCex, """{"epa_id":999999}""");
        activities["mini-cex, not a number"] = await InsertActivityAsync(connection, miniCex, """{"epa_id":"PAED-001"}""");
        activities["mini-cex, no epa yet"] = await InsertActivityAsync(connection, miniCex, """{}""");
        activities["mini-cex, a sign and whitespace"] = await InsertActivityAsync(connection, miniCex, $$"""{"epa_id":" \t+{{history}} "}""");
        activities["mini-cex, leading zeros"] = await InsertActivityAsync(connection, miniCex, $$"""{"epa_id":"0000000000{{wardRound}}"}""");
        activities["mini-cex, a no-break space"] = await InsertActivityAsync(connection, miniCex, $$"""{"epa_id":"\u00a0{{history}}"}""");
        activities["mini-cex, negative"] = await InsertActivityAsync(connection, miniCex, $$"""{"epa_id":"-{{history}}"}""");
        // As the release writes it: recorded, by the releasing coordinator. An msf_cpsa draft its own subject filed is one
        // T162's migration deletes, and this schema is migrated through T162.
        activities["msf, which credits nothing"] = await InsertActivityAsync(
            connection, msf, $$"""{"epa_id":{{history}}}""", state: "recorded", createdByUserId: "coordinator-1");
        activities["operator type, epa under another key"] = await InsertActivityAsync(connection, otherKey, $$"""{"the_epa":{{history}},"epa_id":"{{history}}"}""");
        activities["operator type, credit reads another epa field"] = await InsertActivityAsync(connection, readsSecond, $$"""{"epa_id":{{history}},"second_epa":{{wardRound}}}""");
        activities["operator type, epa_id is text"] = await InsertActivityAsync(connection, textEpaId, $$"""{"epa_id":"{{history}}"}""");
        activities["operator type, credits a literal item"] = await InsertActivityAsync(connection, literalItem, $$"""{"epa_id":{{history}}}""");
        activities["operator type, sole epa field, credits nothing"] = await InsertActivityAsync(connection, soleEpa, $$"""{"related_epa":{{history}}}""");

        return new Fixture(schema, history, wardRound, activities);
    }

    /// <summary>A seeded type as it stood before T137: its seed folder's canonical payloads, minus the pointer.</summary>
    private static async Task<int> InsertSeedTypeWithoutPointerAsync(NpgsqlConnection connection, string key)
    {
        var entry = ActivityTypeSeedCatalogue.Entries.Single(candidate => candidate.Key == key);
        var payload = await ActivityTypeSeedCatalogue.ReadCanonicalAsync(entry, CancellationToken.None);

        var schemaJson = payload.SchemaJson.Replace("\"evidence_epa_field\":\"epa_id\",", string.Empty, StringComparison.Ordinal);
        schemaJson.Should().NotContain("evidence_epa_field", "guard: the fixture must be in the pre-T137 shape");

        return await InsertTypeAsync(connection, key, schemaJson, payload.CreditRulesJson, displayFieldsJson: payload.DisplayFieldsJson, ownerUserId: SeedOwnerUserId);
    }

    private static async Task<int> InsertTypeAsync(
        NpgsqlConnection connection,
        string key,
        string schemaJson,
        string creditRulesJson,
        string? stagedSchemaJson = null,
        string? stagedCreditRulesJson = null,
        string displayFieldsJson = "[]",
        string ownerUserId = "admin-legacy")
    {
        const string WorkflowJson = """{"version":1,"initial_state":"draft","states":[{"key":"draft","label":"Draft"},{"key":"done","label":"Done","terminal":true}],"transitions":[{"key":"submit","from":"draft","to":"done","actor":"subject","validation":"all"}]}""";

        var workflowJson = key is "mini_cex_cpsa" or "msf_cpsa"
            ? (await ActivityTypeSeedCatalogue.ReadCanonicalAsync(
                ActivityTypeSeedCatalogue.Entries.Single(candidate => candidate.Key == key), CancellationToken.None)).WorkflowJson
            : WorkflowJson;

        var typeId = await InsertAsync(connection,
            """
            INSERT INTO "ActivityTypes"
                ("Key", "Name", "Scope", "Version", "SchemaJson", "WorkflowJson", "CreditRulesJson", "DisplayFieldsJson",
                 "StagingSchemaJson", "StagingWorkflowJson", "StagingCreditRulesJson", "StagingDisplayFieldsJson",
                 "OwnerUserId", "CreatedOn", "IsActive")
            VALUES ($1, $1, 0, 1, $2::jsonb, $3::jsonb, $4::jsonb, $5::jsonb,
                    $6::jsonb, CASE WHEN $6 IS NULL THEN NULL ELSE $3::jsonb END, $7::jsonb, CASE WHEN $6 IS NULL THEN NULL ELSE '[]'::jsonb END,
                    $8, TIMESTAMPTZ '2026-06-19 00:00:00+00', TRUE)
            RETURNING "Id"
            """,
            key, schemaJson, workflowJson, creditRulesJson, displayFieldsJson,
            (object?)stagedSchemaJson ?? DBNull.Value, (object?)stagedCreditRulesJson ?? DBNull.Value, ownerUserId);

        await ExecuteAsync(connection,
            """
            INSERT INTO "ActivityTypeVersions"
                ("ActivityTypeId", "Version", "SchemaJson", "WorkflowJson", "CreditRulesJson", "DisplayFieldsJson", "PublishedByUserId", "PublishedOn")
            VALUES ($1, 1, $2::jsonb, $3::jsonb, $4::jsonb, $5::jsonb, $6, TIMESTAMPTZ '2026-06-19 00:00:00+00')
            """,
            typeId, schemaJson, workflowJson, creditRulesJson, displayFieldsJson, ownerUserId);

        return typeId;
    }

    private static Task<int> InsertActivityAsync(
        NpgsqlConnection connection,
        int activityTypeId,
        string dataJson,
        string state = "draft",
        string createdByUserId = TraineeId)
        => InsertAsync(connection,
            """
            INSERT INTO "Activities"
                ("ActivityTypeId", "SchemaVersion", "SubjectUserId", "CreatedByUserId", "CurrentState", "DataJson",
                 "CreatedOn", "UpdatedOn", "ObservedOn", "ObservedOnSource")
            VALUES ($1, 1, $2, $4, $5, $3::jsonb,
                    TIMESTAMPTZ '2026-03-10 08:00:00+00', TIMESTAMPTZ '2026-03-10 08:00:00+00', DATE '2026-03-10', 0)
            RETURNING "Id"
            """,
            activityTypeId, TraineeId, dataJson, createdByUserId, state);

    private static Task<int> InsertTransitionAsync(NpgsqlConnection connection, int activityId, string key, DateTime occurredOn, int? creditedItemCount)
        => ExecuteAsync(connection,
            """
            INSERT INTO "ActivityTransitions"
                ("ActivityId", "FromState", "ToState", "TransitionKey", "ActorUserId", "OccurredOn", "SnapshotJson", "CreditedItemCount")
            VALUES ($1, 'draft', 'done', $2, 'assessor-1', $3, '{}'::jsonb, $4)
            """,
            activityId, key, occurredOn, (object?)creditedItemCount ?? DBNull.Value);

    // ---- reads ---------------------------------------------------------------------------------------------------------

    private async Task<Dictionary<int, int?>> EpaIdsAsync(string schema)
    {
        await using var db = NewContext(schema);
        return await db.Activities.AsNoTracking().ToDictionaryAsync(activity => activity.Id, activity => activity.EpaId);
    }

    private async Task<Dictionary<string, Pointers>> PointersAsync(string schema)
    {
        await using var db = NewContext(schema);
        var types = await db.ActivityTypes.AsNoTracking().Include(type => type.Versions).ToListAsync();

        return types.ToDictionary(
            type => type.Key,
            type => new Pointers(
                Pointer(type.SchemaJson),
                Pointer(type.Versions.Single().SchemaJson),
                Pointer(type.StagingSchemaJson)),
            StringComparer.Ordinal);

        // Read with the product's own parser, so a pointer the migration wrote in a shape the app cannot read fails here.
        static string? Pointer(string? schemaJson)
            => string.IsNullOrWhiteSpace(schemaJson) ? null : FormSchemaParser.Parse(schemaJson).EvidenceEpaField;
    }

    // ---- schema lifecycle (as WbaToolAllowListPostgresTests) ------------------------------------------------------------

    private async Task MigrateToLatestAsync(string schema)
    {
        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);

    private static async Task<int> InsertAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private static async Task<int> ExecuteAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        return await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ScalarAsync<T>(string schema, string sql)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }

    /// <summary>Positional parameters ($1, $2, …), so no value is ever spliced into SQL text.</summary>
    private static NpgsqlCommand Command(NpgsqlConnection connection, string sql, object[] values)
    {
        var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        return command;
    }
}
