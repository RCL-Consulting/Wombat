using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Activities;

/// <summary>
/// T102 on a real PostgreSQL server: the nominee directory, the one query behind both the <c>user</c> field's picker
/// (<see cref="ActivityReferenceDataService.GetNomineeOptionsAsync" />) and the write path's check
/// (<see cref="ActivityService.CreateDraftAsync" />).
/// </summary>
/// <remarks>
/// <para>
/// None of this is visible to the unit suites. They run on EF InMemory, which evaluates the query in memory: the role
/// conjunction is never translated to SQL, a <c>DateTimeOffset</c> is stored exactly as written, and string equality is
/// .NET's ordinal. On Npgsql the conjunction must translate to correlated subqueries (or the query throws, or worse,
/// evaluates on the client), <c>LockoutEnd</c> is a <c>timestamptz</c> with microsecond precision and an
/// <c>infinity</c>, and id equality is the column collation's. The deactivation threshold exists precisely because
/// <see cref="DateTimeOffset.MaxValue" /> may not come back as written.
/// </para>
/// <para>
/// The schema helpers are copied from <c>WbaToolAllowListPostgresTests</c>. Each test builds
/// <see cref="ApplicationDbContext" /> directly on a schema of its own (<c>SearchPath = it_&lt;guid&gt;</c>, with nothing
/// else on the path), so no statement can reach <c>public</c>. Every schema is registered BEFORE it is created. Each
/// test drops its schemas in a <c>finally</c>, and <see cref="DisposeAsync" /> repeats the drop as a backstop. Nothing
/// that can fail runs in <see cref="InitializeAsync" />.
/// </para>
/// </remarks>
public sealed class NomineeDirectoryPostgresTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private const int InstitutionId = 10;
    private const int OtherInstitutionId = 20;

    private const string SubjectId = "trainee-subject";

    // One condition fails per decoy; the eligible ones differ only in what should not matter.
    private const string Eligible = "nominee-eligible";
    private const string EligibleWithExtraRole = "nominee-extra-role";
    private const string BrieflyLocked = "nominee-briefly-locked";
    private const string LockoutExpired = "nominee-lockout-expired";
    private const string BothRoles = "nominee-both-roles";
    private const string CaseSensitiveEligible = "Nominee-Case";
    private const string CaseSensitiveDecoy = "nominee-case";
    private const string DeactivatedByEf = "nominee-deactivated";
    private const string DeactivatedAsInfinity = "nominee-infinity";
    private const string DeactivatedTruncated = "nominee-truncated-max";
    private const string OtherInstitution = "nominee-other-institution";
    private const string NoInstitution = "nominee-no-institution";
    private const string CoordinatorOnly = "nominee-coordinator";
    private const string CommitteeOnly = "nominee-committee-only";
    private const string NoRoles = "nominee-no-roles";

    /// <summary>Everyone the directory must list for an <c>Assessor</c> field at <see cref="InstitutionId" />.</summary>
    private static readonly string[] ExpectedAssessorNominees =
        [Eligible, EligibleWithExtraRole, BrieflyLocked, LockoutExpired, BothRoles, CaseSensitiveEligible];

    /// <summary>Every seeded id the directory must NOT list for an <c>Assessor</c> field, the subject included.</summary>
    private static readonly string[] SeededButNotListed =
        [SubjectId, CaseSensitiveDecoy, DeactivatedByEf, DeactivatedAsInfinity, DeactivatedTruncated, OtherInstitution, NoInstitution, CoordinatorOnly, CommitteeOnly, NoRoles];

    /// <summary>
    /// Ids that name nobody, or name a real user only under a looser comparison than ordinal. PostgreSQL's
    /// <c>text</c> keeps trailing spaces significant (only <c>char(n)</c> pads), and the default collation is
    /// deterministic, so each of these must miss.
    /// </summary>
    private static readonly string[] Variants =
        [Eligible.ToUpperInvariant(), Eligible + " ", " " + Eligible, "NOMINEE-CASE", "nobody-at-all"];

    private readonly List<string> _schemas = [];
    private string _baseConnectionString = null!;

    public Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => DropSchemasAsync();

    [Fact]
    public async Task GetNomineeOptions_OnPostgres_ListsExactlyTheEligible_WithEveryConditionEvaluatedInOneServerSideQuery()
    {
        // The directory is one IQueryable: the institution, subject and lockout filters plus one EXISTS-within-EXISTS per
        // required role. If any of it could not translate, EF would throw here; if it were split into a user fetch and
        // a role fetch joined in memory, the listing would be more than one command. Both are asserted on what reached
        // the server, not inferred from the result.
        try
        {
            var schema = await MigratedSchemaAsync();
            await SeedMatrixAsync(schema);

            var commands = new CommandLog();
            await using (var db = NewContext(schema, commands))
            {
                var options = await new ActivityReferenceDataService(db).GetNomineeOptionsAsync(ExistingActivityScope([WombatRoles.Assessor]));

                options.Select(option => option.Value).Should().BeEquivalentTo(ExpectedAssessorNominees);
                options.Single(option => option.Value == Eligible).Label.Should().Be($"First {Eligible} ({Eligible}@test.local)");
            }

            var listing = commands.Texts.Should().ContainSingle("one round trip: nothing is fetched and joined on the client").Subject;
            listing.Should().Contain("FROM \"AspNetUsers\"");
            listing.Should().Contain("\"AspNetUserRoles\"").And.Contain("\"AspNetRoles\"");
            Count(listing, @"\bEXISTS\b").Should().Be(2, "one user-role subquery holding one role subquery, for the one required role");
            Count(listing, "\"NormalizedName\" = ").Should().Be(1);
            listing.Should().Contain("\"LockoutEnd\" IS NULL", "the deactivation test runs in SQL, not after materialisation");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task GetNomineeOptions_OnPostgres_RequiresEveryRole_AsOneSubqueryPairPerRole()
    {
        // Conjunction, not union: a user holding either role alone is not listed. Two roles mean two independent
        // EXISTS pairs over the same user, never one subquery testing IN (…), which would be a disjunction.
        try
        {
            var schema = await MigratedSchemaAsync();
            await SeedMatrixAsync(schema);

            var commands = new CommandLog();
            await using (var db = NewContext(schema, commands))
            {
                var options = await new ActivityReferenceDataService(db).GetNomineeOptionsAsync(
                    ExistingActivityScope([WombatRoles.Assessor, WombatRoles.CommitteeMember]));

                options.Select(option => option.Value).Should().Equal(BothRoles);
            }

            var listing = commands.Texts.Should().ContainSingle().Subject;
            Count(listing, @"\bEXISTS\b").Should().Be(4);
            Count(listing, "\"NormalizedName\" = ").Should().Be(2);
            listing.Should().NotContain(" IN (", "a single role subquery with IN would admit a user holding either role");

            await using (var db = NewContext(schema))
            {
                (await new ActivityReferenceDataService(db).GetNomineeOptionsAsync(ExistingActivityScope([WombatRoles.CommitteeMember])))
                    .Select(option => option.Value).Should().BeEquivalentTo([BothRoles, CommitteeOnly],
                        "guard: the committee-only decoy fails the conjunction, not the committee role on its own");
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task GetNomineeOptions_OnTheCreatePage_OnPostgres_ResolvesTheSubjectsInstitution_AndListsTheSameSet()
    {
        // The create page has no stamp yet: the institution is resolved from the subject, as the create will stamp it.
        // This subject has no trainee profile, so the resolver falls back to their Identity row on the server.
        try
        {
            var schema = await MigratedSchemaAsync();
            await SeedMatrixAsync(schema);

            await using var db = NewContext(schema);
            var options = await new ActivityReferenceDataService(db).GetNomineeOptionsAsync(
                new NomineeOptionScope(SubjectId, [WombatRoles.Assessor], ForExistingActivity: false, ActivityInstitutionId: null, StoredValue: null));

            options.Select(option => option.Value).Should().BeEquivalentTo(ExpectedAssessorNominees);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task LockoutEnd_WrittenAsDateTimeOffsetMaxValue_IsStoredAsInfinity_AndReadsBackAsMaxValue()
    {
        // What an administrator's lock and an erasure write. UserDeactivation compares against a far-future threshold
        // rather than MaxValue itself so that any of three round trips would work: exact, truncated to microseconds,
        // or infinity. This pins which one Npgsql actually does, so a driver upgrade that changes it is noticed.
        try
        {
            var schema = await MigratedSchemaAsync();

            await using (var db = NewContext(schema))
            {
                NomineeSeed.AddUser(db, DeactivatedByEf, InstitutionId, DateTimeOffset.MaxValue, WombatRoles.Assessor);
                await db.SaveChangesAsync();
            }

            var stored = await QueryAsync(
                schema,
                """SELECT "LockoutEnd"::text, isfinite("LockoutEnd") FROM "AspNetUsers" WHERE "Id" = $1""",
                reader => (Text: reader.GetString(0), Finite: reader.GetBoolean(1)),
                DeactivatedByEf);

            stored.Should().ContainSingle().Which.Should().Be(("infinity", false),
                "Npgsql maps DateTimeOffset.MaxValue to timestamptz 'infinity' rather than truncating it to microseconds");

            await using (var db = NewContext(schema))
            {
                var readBack = await db.Users.AsNoTracking()
                    .Where(user => user.Id == DeactivatedByEf)
                    .Select(user => user.LockoutEnd)
                    .SingleAsync();

                readBack.Should().Be(DateTimeOffset.MaxValue, "infinity reads back as MaxValue, so the round trip is exact");
                readBack.Should().BeOnOrAfter(new DateTimeOffset(9000, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    "whatever the round trip, the stored value must still be at or past UserDeactivation.Threshold");
            }
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task CreateDraft_OnPostgres_AcceptsEveryListedNominee_AndRefusesEverySeededIdItDoesNotList_BeforeAnyWrite()
    {
        // Picker and gate are one query, so on the real server as in memory, "listed" and "accepted" must be the same
        // set. Each attempt is its own request on its own context. A refusal must leave the context clean, because the
        // audit pipeline saves it from its catch, and must leave no row on the server.
        try
        {
            var schema = await MigratedSchemaAsync();
            await SeedMatrixAsync(schema);
            var activityTypeId = await SeedActivityTypeAsync(schema);

            IReadOnlyList<string> listed;
            await using (var db = NewContext(schema))
            {
                listed = (await new ActivityReferenceDataService(db).GetNomineeOptionsAsync(ExistingActivityScope([WombatRoles.Assessor])))
                    .Select(option => option.Value)
                    .ToList();
            }

            listed.Should().BeEquivalentTo(ExpectedAssessorNominees, "guard: parity over an empty or wrong list would prove nothing");

            foreach (var nominee in listed)
            {
                await using var db = NewContext(schema);
                var created = await Service(db).CreateDraftAsync(CreateInput(activityTypeId, nominee));

                ReadString(created.DataJson, "assessor_user_id").Should().Be(nominee, "a listed nominee must be accepted as written");
                created.InstitutionId.Should().Be(InstitutionId);
            }

            foreach (var nominee in SeededButNotListed.Concat(Variants))
            {
                var message = await ShouldBeRefusedAsync(schema, service => service.CreateDraftAsync(CreateInput(activityTypeId, nominee)));

                if (nominee != SubjectId)
                {
                    // The subject is refused earlier, by the rule that an actor field may never name the subject.
                    message.Should().Contain("cannot be named here", $"'{nominee}' must be refused by the nominee gate");
                }
            }

            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "Activities" """))
                .Should().Be(ExpectedAssessorNominees.Length, "exactly the listed nominees' creates reached the server");
            (await ScalarAsync<long>(schema, """SELECT COUNT(DISTINCT "DataJson" ->> 'assessor_user_id') FROM "Activities" """))
                .Should().Be(ExpectedAssessorNominees.Length);
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    [Fact]
    public async Task CreateDraft_OnPostgres_NamesARefusedPersonOnlyWhenTheyBelongToTheActivitysInstitution()
    {
        // The refusal must not confirm anything about another institution's people, nor match an id loosely to find a
        // name. A deactivated or role-less colleague at the trainee's own institution is named; anyone else is
        // "that person".
        try
        {
            var schema = await MigratedSchemaAsync();
            await SeedMatrixAsync(schema);
            var activityTypeId = await SeedActivityTypeAsync(schema);

            (await ShouldBeRefusedAsync(schema, service => service.CreateDraftAsync(CreateInput(activityTypeId, CoordinatorOnly))))
                .Should().StartWith($"Assessor: First {CoordinatorOnly} cannot be named here.");
            (await ShouldBeRefusedAsync(schema, service => service.CreateDraftAsync(CreateInput(activityTypeId, DeactivatedAsInfinity))))
                .Should().StartWith($"Assessor: First {DeactivatedAsInfinity} cannot be named here.");

            var crossInstitution = await ShouldBeRefusedAsync(schema, service => service.CreateDraftAsync(CreateInput(activityTypeId, OtherInstitution)));
            crossInstitution.Should().StartWith("Assessor: that person cannot be named here.");
            crossInstitution.Should().NotContain(OtherInstitution);

            // "nominee-case" is a real, ineligible user; "NOMINEE-CASE" is nobody, and must not borrow either name.
            (await ShouldBeRefusedAsync(schema, service => service.CreateDraftAsync(CreateInput(activityTypeId, "NOMINEE-CASE"))))
                .Should().StartWith("Assessor: that person cannot be named here.");
            (await ShouldBeRefusedAsync(schema, service => service.CreateDraftAsync(CreateInput(activityTypeId, CaseSensitiveDecoy))))
                .Should().StartWith($"Assessor: First {CaseSensitiveDecoy} cannot be named here.");
        }
        finally
        {
            await DropSchemasAsync();
        }
    }

    // ---- fixture ----------------------------------------------------------------------------------------------

    /// <summary>
    /// The subject and one user per condition, through the shared seed helper, so roles carry <c>NormalizedName</c> as
    /// Identity writes them. Two deactivations are then rewritten by raw SQL into the other forms a stored
    /// <c>LockoutEnd</c> could take: <c>infinity</c> literally, and <see cref="DateTimeOffset.MaxValue" /> truncated to
    /// PostgreSQL's microseconds.
    /// </summary>
    private async Task SeedMatrixAsync(string schema)
    {
        await using (var db = NewContext(schema))
        {
            NomineeSeed.AddUser(db, SubjectId, InstitutionId, WombatRoles.Trainee, WombatRoles.Assessor);

            NomineeSeed.AddUser(db, Eligible, InstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, EligibleWithExtraRole, InstitutionId, WombatRoles.Assessor, WombatRoles.Coordinator);
            NomineeSeed.AddUser(db, BrieflyLocked, InstitutionId, DateTimeOffset.UtcNow.AddMinutes(15), WombatRoles.Assessor);
            NomineeSeed.AddUser(db, LockoutExpired, InstitutionId, DateTimeOffset.UtcNow.AddDays(-1), WombatRoles.Assessor);
            NomineeSeed.AddUser(db, BothRoles, InstitutionId, WombatRoles.Assessor, WombatRoles.CommitteeMember);
            NomineeSeed.AddUser(db, CaseSensitiveEligible, InstitutionId, WombatRoles.Assessor);

            // Differs from an eligible id only in case. Its user name must differ, because Identity's user-name index
            // is on the upper-cased name.
            var caseDecoy = NomineeSeed.AddUser(db, CaseSensitiveDecoy, InstitutionId, WombatRoles.Coordinator);
            caseDecoy.UserName = "case-decoy@test.local";
            caseDecoy.NormalizedUserName = "CASE-DECOY@TEST.LOCAL";
            caseDecoy.Email = "case-decoy@test.local";
            caseDecoy.NormalizedEmail = "CASE-DECOY@TEST.LOCAL";

            NomineeSeed.AddUser(db, DeactivatedByEf, InstitutionId, DateTimeOffset.MaxValue, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, DeactivatedAsInfinity, InstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, DeactivatedTruncated, InstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, OtherInstitution, OtherInstitutionId, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, NoInstitution, institutionId: null, WombatRoles.Assessor);
            NomineeSeed.AddUser(db, CoordinatorOnly, InstitutionId, WombatRoles.Coordinator);
            NomineeSeed.AddUser(db, CommitteeOnly, InstitutionId, WombatRoles.CommitteeMember);
            NomineeSeed.AddUser(db, NoRoles, InstitutionId);

            await db.SaveChangesAsync();
        }

        (await ExecuteAsync(schema, """UPDATE "AspNetUsers" SET "LockoutEnd" = 'infinity'::timestamptz WHERE "Id" = $1""", DeactivatedAsInfinity))
            .Should().Be(1);
        (await ExecuteAsync(schema, """UPDATE "AspNetUsers" SET "LockoutEnd" = TIMESTAMPTZ '9999-12-31 23:59:59.999999+00' WHERE "Id" = $1""", DeactivatedTruncated))
            .Should().Be(1);

        (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "AspNetRoles" WHERE "NormalizedName" = UPPER("Name")"""))
            .Should().Be(4, "guard: Assessor, Trainee, Coordinator and CommitteeMember, each with its normalised name");
    }

    /// <summary>
    /// A CPSA-shaped request form: the trainee names their assessor, who alone may complete it. No instrument key and no
    /// credit, so the T122 gate has nothing to judge and cannot be the reason for any refusal here.
    /// </summary>
    private async Task<int> SeedActivityTypeAsync(string schema)
    {
        const string SchemaJson = """
            {
              "version": 1,
              "observation_date_field": "observed_on",
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true },
                    { "key": "observed_on", "type": "date", "label": "Date observed", "required": true }
                  ]
                },
                {
                  "key": "assessment",
                  "title": "Assessment",
                  "editable_by": "field:assessor_user_id",
                  "fields": [
                    { "key": "overall_level", "type": "number", "label": "Supervision required" }
                  ]
                }
              ]
            }
            """;

        const string WorkflowJson = """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
                { "key": "completed", "label": "Completed", "terminal": true },
                { "key": "cancelled", "label": "Cancelled" }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
                { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "requires_fields": ["overall_level"] },
                { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject|creator" }
              ]
            }
            """;

        const string CreditRulesJson = """{ "counts_for": [] }""";
        const string DisplayFieldsJson = """["assessor_user_id"]""";
        var publishedOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        await using var db = NewContext(schema);
        var activityType = new ActivityType
        {
            Key = "nominee_probe",
            Name = "Nominee probe",
            Scope = ActivityScope.Institution,
            ScopeId = InstitutionId,
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditRulesJson,
            DisplayFieldsJson = DisplayFieldsJson,
            WbaToolKey = null,
            OwnerUserId = "admin-1",
            CreatedOn = publishedOn
        };
        activityType.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = SchemaJson,
            WorkflowJson = WorkflowJson,
            CreditRulesJson = CreditRulesJson,
            DisplayFieldsJson = DisplayFieldsJson,
            PublishedByUserId = "admin-1",
            PublishedOn = publishedOn
        });

        db.ActivityTypes.Add(activityType);
        await db.SaveChangesAsync();
        return activityType.Id;
    }

    private static NomineeOptionScope ExistingActivityScope(IReadOnlyList<string> roles)
        => new(SubjectId, roles, ForExistingActivity: true, ActivityInstitutionId: InstitutionId, StoredValue: null);

    private static CreateActivityInput CreateInput(int activityTypeId, string nominee)
        => new(
            activityTypeId,
            SubjectId,
            SubjectId,
            JsonSerializer.Serialize(new Dictionary<string, string> { ["assessor_user_id"] = nominee, ["observed_on"] = "2026-03-10" }),
            new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, SubjectId)], "test")));

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator());

    /// <summary>
    /// Runs one refused request on its own context, asserts it left nothing for the audit save to commit, performs
    /// that save, and returns the refusal message. The <c>ToolPermissionGateTests</c> template, on Npgsql.
    /// </summary>
    private async Task<string> ShouldBeRefusedAsync(string schema, Func<ActivityService, Task> act)
    {
        await using var db = NewContext(schema);
        var service = Service(db);

        var attempt = async () => await act(service);
        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();

        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => $"{entry.Metadata.ClrType.Name}: {entry.State}")
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        (await db.SaveChangesAsync()).Should().Be(0);

        return thrown.Which.Message;
    }

    private static string? ReadString(string dataJson, string key)
    {
        using var document = JsonDocument.Parse(dataJson);
        return document.RootElement.TryGetProperty(key, out var value) ? value.GetString() : null;
    }

    private static int Count(string text, string pattern)
        => Regex.Matches(text, pattern.StartsWith(@"\b", StringComparison.Ordinal) ? pattern : Regex.Escape(pattern)).Count;

    // ---- schema helpers, copied from WbaToolAllowListPostgresTests ---------------------------------------------

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await CreateSchemaAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        return schema;
    }

    private async Task<int> ExecuteAsync(string schema, string sql, params object[] values)
    {
        await using var connection = await OpenAsync(schema);
        await using var command = Command(connection, sql, values);
        return await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ScalarAsync<T>(string schema, string sql, params object[] values)
    {
        await using var connection = await OpenAsync(schema);
        await using var command = Command(connection, sql, values);
        var value = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }

    private async Task<List<T>> QueryAsync<T>(string schema, string sql, Func<NpgsqlDataReader, T> map, params object[] values)
    {
        await using var connection = await OpenAsync(schema);
        await using var command = Command(connection, sql, values);
        await using var reader = await command.ExecuteReaderAsync();

        var results = new List<T>();
        while (await reader.ReadAsync())
        {
            results.Add(map(reader));
        }

        return results;
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

    private async Task<NpgsqlConnection> OpenAsync(string schema)
    {
        var connection = new NpgsqlConnection(SchemaConnectionString(schema));
        await connection.OpenAsync();
        return connection;
    }

    /// <summary>A new, empty schema, registered for dropping before it exists so that no failure can leak it.</summary>
    private async Task<string> CreateSchemaAsync()
    {
        var schema = $"it_{Guid.NewGuid():N}";
        _schemas.Add(schema);

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA \"{schema}\"";
        await command.ExecuteNonQueryAsync();

        return schema;
    }

    /// <summary>Drops every schema this test created. Called from each test's finally and again from DisposeAsync.</summary>
    private async Task DropSchemasAsync()
    {
        if (_schemas.Count == 0)
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        foreach (var schema in _schemas.ToList())
        {
            // Belt and braces: this class only ever drops a schema it named itself.
            if (schema.StartsWith("it_", StringComparison.Ordinal))
            {
                await using var drop = connection.CreateCommand();
                drop.CommandText = $"DROP SCHEMA IF EXISTS \"{schema}\" CASCADE";
                await drop.ExecuteNonQueryAsync();
            }

            _schemas.Remove(schema);
        }
    }

    private ApplicationDbContext NewContext(string schema, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(SchemaConnectionString(schema));
        if (interceptors.Length > 0)
        {
            options.AddInterceptors(interceptors);
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>
    /// The schema and nothing else on the search path, so an unqualified name can only ever resolve inside it.
    /// </summary>
    private string SchemaConnectionString(string schema)
        => new NpgsqlConnectionStringBuilder(_baseConnectionString)
        {
            SearchPath = schema,
            Pooling = false
        }.ConnectionString;

    /// <summary>The same resolution order as <c>MsfRespondEndpointFlowTests</c>.</summary>
    private static string ResolveBaseConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable("WOMBAT_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var secretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            WombatWebUserSecretsId,
            "secrets.json");

        if (File.Exists(secretsPath))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(secretsPath));
            if (document.RootElement.TryGetProperty("ConnectionStrings:DefaultConnection", out var property)
                && property.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(property.GetString()))
            {
                return property.GetString()!;
            }
        }

        return "Host=localhost;Port=5432;Database=wombat;Username=postgres;Password=postgres";
    }

    /// <summary>Every query EF sends through one context, so "one round trip, all in SQL" is asserted on the wire.</summary>
    private sealed class CommandLog : DbCommandInterceptor
    {
        public List<string> Texts { get; } = [];

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Texts.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            Texts.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}
