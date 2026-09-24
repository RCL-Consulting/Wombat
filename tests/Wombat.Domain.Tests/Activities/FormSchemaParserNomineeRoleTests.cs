using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Identity;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T102 fix 3. A <c>user</c> field's optional <c>role</c>: the role its nominee must hold.
/// </summary>
/// <remarks>
/// <para>
/// The value is authorization input — the nominee gate and the picker both read it — so the parser's job is to make
/// sure only one spelling of each role can ever be stored. The directory query matches on the role's stored name, so a
/// stored <c>"assessor"</c> that reached it un-normalised would match nobody, or worse, whatever a case-insensitive
/// collation later decided it meant.
/// </para>
/// <para>
/// Absent means Assessor, and absent must STAY absent through Parse+Serialize: every seed and every pinned version
/// omits it, and a canonical form that started materialising <c>"role":"Assessor"</c> would make the seed refresher
/// republish every seeded type on the next boot.
/// </para>
/// </remarks>
public sealed class FormSchemaParserNomineeRoleTests
{
    public static TheoryData<string> NominableRoles()
    {
        var data = new TheoryData<string>();
        foreach (var role in WombatRoles.Nominable)
        {
            data.Add(role);
        }

        return data;
    }

    // ---- Parse --------------------------------------------------------------------------------

    [Fact]
    public void Parse_RoleOnAUserField_IsRead()
    {
        var field = SingleUserField(FormSchemaParser.Parse(UserFieldSchema("\"role\": \"Coordinator\"")));

        Assert.Equal(WombatRoles.Coordinator, field.NomineeRole);
    }

    /// <summary>
    /// Absent is null on the record, not <see cref="WombatRoles.Assessor" />: the default is applied by
    /// <c>ActorFieldRules</c>, so the parse result can still say "this field did not say", which is what keeps the
    /// canonical form free of a materialised default.
    /// </summary>
    [Fact]
    public void Parse_UserFieldWithoutARole_LeavesTheRoleNull()
    {
        var field = SingleUserField(FormSchemaParser.Parse(UserFieldSchema(extraProperty: null)));

        Assert.Null(field.NomineeRole);
    }

    [Theory]
    [InlineData("\"role\": null")]
    [InlineData("\"role\": \"\"")]
    [InlineData("\"role\": \"   \"")]
    public void Parse_ANullOrBlankRole_IsTheSameAsNoRole(string roleProperty)
    {
        var field = SingleUserField(FormSchemaParser.Parse(UserFieldSchema(roleProperty)));

        Assert.Null(field.NomineeRole);
    }

    [Theory]
    [MemberData(nameof(NominableRoles))]
    public void Parse_EveryNominableRole_IsAccepted(string role)
    {
        var field = SingleUserField(FormSchemaParser.Parse(UserFieldSchema($"\"role\": \"{role}\"")));

        Assert.Equal(role, field.NomineeRole);
    }

    /// <summary>
    /// Every spelling of a role is stored as the one constant, so two drafts that differ only in case canonicalise to
    /// the same bytes and the directory query can match on the stored name without caring about case.
    /// </summary>
    [Theory]
    [InlineData("assessor", WombatRoles.Assessor)]
    [InlineData("ASSESSOR", WombatRoles.Assessor)]
    [InlineData("aSsEsSoR", WombatRoles.Assessor)]
    [InlineData("  assessor  ", WombatRoles.Assessor)]
    [InlineData("committeemember", WombatRoles.CommitteeMember)]
    [InlineData("COORDINATOR", WombatRoles.Coordinator)]
    [InlineData("institutionaladmin", WombatRoles.InstitutionalAdmin)]
    [InlineData("trainee", WombatRoles.Trainee)]
    public void Parse_Role_IsNormalisedToTheCanonicalConstant(string written, string canonical)
    {
        var field = SingleUserField(FormSchemaParser.Parse(UserFieldSchema($"\"role\": \"{written}\"")));

        Assert.Equal(canonical, field.NomineeRole, StringComparer.Ordinal);
    }

    /// <summary>
    /// A role on any other field type reads as a restriction nothing enforces. No stored version carries one, so the
    /// parser can refuse it outright without costing a pinned version its parse.
    /// </summary>
    [Theory]
    [InlineData("\"type\": \"text\"")]
    [InlineData("\"type\": \"longtext\"")]
    [InlineData("\"type\": \"number\"")]
    [InlineData("\"type\": \"date\"")]
    [InlineData("\"type\": \"datetime\"")]
    [InlineData("\"type\": \"choice\", \"options\": [\"a\", \"b\"]")]
    [InlineData("\"type\": \"scale\", \"options\": [\"1\", \"2\"]")]
    [InlineData("\"type\": \"epa\"")]
    [InlineData("\"type\": \"checkbox\"")]
    [InlineData("\"type\": \"signature\"")]
    public void Parse_RoleOnANonUserField_IsRefused(string typeAndOptions)
    {
        var json = $$"""
            {
              "version": 1,
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "supervisor", {{typeAndOptions}}, "label": "Supervisor", "role": "Assessor" }
                  ]
                }
              ]
            }
            """;

        var exception = Assert.Throws<SchemaParseException>(() => FormSchemaParser.Parse(json));

        Assert.Contains("'supervisor'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("only a User field names a person", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The two national roles carry no institution, so a nominee holding only them could never be at the trainee's
    /// institution; PendingTrainee is not admitted to anything. A type requiring any of them is unfillable, which the
    /// parser says at save rather than leaving to the first refused submit. Case variants are refused too — the
    /// allow-list comparison is case-insensitive in both directions.
    /// </summary>
    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.CollegeAdmin)]
    [InlineData(WombatRoles.PendingTrainee)]
    [InlineData("administrator")]
    [InlineData("COLLEGEADMIN")]
    [InlineData("pendingtrainee")]
    public void Parse_ARoleNoNomineeCanHold_IsRefused(string role)
    {
        var exception = Assert.Throws<SchemaParseException>(
            () => FormSchemaParser.Parse(UserFieldSchema($"\"role\": \"{role}\"")));

        Assert.Contains("'assessor_user_id'", exception.Message, StringComparison.Ordinal);
        Assert.Contains($"'{role}'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("not a role a nominee can hold", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The speciality and sub-speciality admins are scoped more narrowly than the institution, and the institution is all
    /// the nominee directory checks: an admin of another speciality at the trainee's institution would pass both the
    /// picker and the gate. Refused until the directory also matches the activity's stamped speciality, and refused in
    /// every spelling, since the allow-list comparison is case-insensitive.
    /// </summary>
    [Theory]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    [InlineData("specialityadmin")]
    [InlineData("SUBSPECIALITYADMIN")]
    public void Parse_ARoleScopedNarrowerThanTheInstitution_IsRefused(string role)
    {
        var exception = Assert.Throws<SchemaParseException>(
            () => FormSchemaParser.Parse(UserFieldSchema($"\"role\": \"{role}\"")));

        Assert.Contains("'assessor_user_id'", exception.Message, StringComparison.Ordinal);
        Assert.Contains($"'{role}'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("not a role a nominee can hold", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// An unknown role, including the actor-rule spelling an author might carry across from the Workflow tab, is
    /// refused, and the message lists the roles that would be accepted so the author can repair it.
    /// </summary>
    [Theory]
    [InlineData("Supervisor")]
    [InlineData("role:Assessor")]
    [InlineData("Assessors")]
    public void Parse_AnUnknownRole_IsRefusedAndTheMessageListsTheNominableRoles(string role)
    {
        var exception = Assert.Throws<SchemaParseException>(
            () => FormSchemaParser.Parse(UserFieldSchema($"\"role\": \"{role}\"")));

        Assert.Contains($"'{role}'", exception.Message, StringComparison.Ordinal);
        foreach (var nominable in WombatRoles.Nominable)
        {
            Assert.Contains(nominable, exception.Message, StringComparison.Ordinal);
        }

        Assert.DoesNotContain(WombatRoles.CollegeAdmin, exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(WombatRoles.PendingTrainee, exception.Message, StringComparison.Ordinal);

        // Ordinal substring: this also rules out SubSpecialityAdmin, which contains it.
        Assert.DoesNotContain(WombatRoles.SpecialityAdmin, exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"role\": 7")]
    [InlineData("\"role\": [\"Assessor\"]")]
    [InlineData("\"role\": { \"name\": \"Assessor\" }")]
    [InlineData("\"role\": true")]
    public void Parse_ARoleThatIsNotAString_IsRefused(string roleProperty)
    {
        var exception = Assert.Throws<SchemaParseException>(
            () => FormSchemaParser.Parse(UserFieldSchema(roleProperty)));

        Assert.Contains("'role' must be a string", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The nominable set is exactly All minus the three roles no nominee can be judged against and the two scoped more
    /// narrowly than the institution the directory checks. Pinned so that adding a role to
    /// <see cref="WombatRoles.All" /> forces a decision about whether a user field may require it.
    /// </summary>
    [Fact]
    public void NominableRoles_AreEveryInstitutionScopedRole()
    {
        var expected = WombatRoles.All
            .Except(
                [
                    WombatRoles.Administrator,
                    WombatRoles.CollegeAdmin,
                    WombatRoles.PendingTrainee,
                    WombatRoles.SpecialityAdmin,
                    WombatRoles.SubSpecialityAdmin
                ],
                StringComparer.Ordinal)
            .Order(StringComparer.Ordinal);

        Assert.Equal(expected, WombatRoles.Nominable.Order(StringComparer.Ordinal));
        Assert.Contains(WombatRoles.Assessor, WombatRoles.Nominable);
    }

    // ---- Serialize ----------------------------------------------------------------------------

    [Fact]
    public void Serialize_Role_IsEmitted()
    {
        var serialized = FormSchemaParser.Serialize(FormSchemaParser.Parse(UserFieldSchema("\"role\": \"CommitteeMember\"")));

        Assert.Contains("\"role\":\"CommitteeMember\"", serialized, StringComparison.Ordinal);
    }

    /// <summary>
    /// What is stored is the canonical constant, not what the author typed — so the published version, and every
    /// activity pinned to it, carries one spelling.
    /// </summary>
    [Fact]
    public void Serialize_Role_IsEmittedInItsCanonicalCase()
    {
        var serialized = FormSchemaParser.Serialize(FormSchemaParser.Parse(UserFieldSchema("\"role\": \"committeemember\"")));

        Assert.Contains("\"role\":\"CommitteeMember\"", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("committeemember", serialized, StringComparison.Ordinal);
    }

    /// <summary>
    /// The seed corpus and every pinned version omit <c>role</c>. If Serialize materialised the default, every seeded
    /// type with a user field would canonicalise differently from its stored version and be republished at boot.
    /// </summary>
    [Fact]
    public void Serialize_UserFieldWithoutARole_EmitsNoRoleProperty()
    {
        var serialized = FormSchemaParser.Serialize(FormSchemaParser.Parse(UserFieldSchema(extraProperty: null)));

        Assert.DoesNotContain("\"role\"", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialize_Role_RoundTripsModuloWhitespace()
    {
        var json = UserFieldSchema("\"role\": \"Coordinator\"");

        var serialized = FormSchemaParser.Serialize(FormSchemaParser.Parse(json));

        Assert.Equal(ActivityTestData.NormalizeJson(json), ActivityTestData.NormalizeJson(serialized));
    }

    /// <summary>
    /// The trap CLAUDE.md names: SaveDraft stores Serialize(Parse(json)), so a Parse half with no Serialize half is
    /// dropped at publish — the field would silently fall back to requiring an Assessor.
    /// </summary>
    [Theory]
    [MemberData(nameof(NominableRoles))]
    public void Serialize_Role_SurvivesASecondRoundTrip(string role)
    {
        var once = FormSchemaParser.Serialize(FormSchemaParser.Parse(UserFieldSchema($"\"role\": \"{role.ToLowerInvariant()}\"")));
        var twice = FormSchemaParser.Serialize(FormSchemaParser.Parse(once));

        Assert.Equal(once, twice);
        Assert.Equal(role, SingleUserField(FormSchemaParser.Parse(twice)).NomineeRole);
    }

    // ---- helpers ------------------------------------------------------------------------------

    /// <summary>A schema with one user field, <c>assessor_user_id</c>, and optionally one extra property on it.</summary>
    private static string UserFieldSchema(string? extraProperty)
    {
        var extra = extraProperty is null ? string.Empty : $", {extraProperty}";

        return $$"""
            {
              "version": 1,
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "title", "type": "text", "label": "Title", "required": true },
                    { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true{{extra}} }
                  ]
                }
              ]
            }
            """;
    }

    private static FormField SingleUserField(FormSchema schema)
        => Assert.Single(schema.Sections.SelectMany(section => section.Fields), field => field.Type == FieldType.User);
}
