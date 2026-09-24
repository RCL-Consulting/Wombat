using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Epas;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// Which EPA an activity is evidence for (T137): the pinned schema's pointer, read the way credit reads it, kept only
/// when the EPA exists.
/// </summary>
public sealed class EvidenceEpaResolverTests
{
    private const int KnownEpaId = 17;

    [Theory]
    [InlineData("""{ "epa_id": 17 }""", 17)]
    [InlineData("""{ "epa_id": "17" }""", 17)]
    [InlineData("""{ "epa_id": " 17 " }""", 17)]
    public void Declared_ReadsAnIntegerTheWayCreditDoes(string dataJson, int expected)
    {
        EvidenceEpaResolver.Declared(SchemaWithPointer(), dataJson).Should().Be(expected);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{ "epa_id": null }""")]
    [InlineData("""{ "epa_id": "" }""")]
    [InlineData("""{ "epa_id": "PAED-001" }""")]
    [InlineData("""{ "epa_id": 17.5 }""")]
    [InlineData("""{ "epa_id": [17] }""")]
    [InlineData("""{ "other_epa": 17 }""")]
    [InlineData("[]")]
    [InlineData("not json")]
    public void Declared_IsNullForAnythingThatIsNotAnIntegerUnderThePointer(string dataJson)
    {
        EvidenceEpaResolver.Declared(SchemaWithPointer(), dataJson).Should().BeNull();
    }

    /// <summary>
    /// No pointer, no EPA, even when the data carries one: which field is the EPA is the schema's statement to make,
    /// not something to guess from a key name.
    /// </summary>
    [Fact]
    public void Declared_IsNullWhenTheSchemaDeclaresNoPointer()
    {
        EvidenceEpaResolver.Declared(SchemaWithoutPointer(), """{ "epa_id": 17 }""").Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_KeepsAnEpaThatExists()
    {
        await using var db = CreateDb();

        (await EvidenceEpaResolver.ResolveAsync(db, SchemaWithPointer(), """{ "epa_id": "17" }""", CancellationToken.None))
            .Should().Be(KnownEpaId);
    }

    /// <summary>
    /// An id naming no EPA is dropped, so the column never holds an id a join cannot resolve.
    /// </summary>
    [Fact]
    public async Task ResolveAsync_DropsAnIdThatNamesNoEpa()
    {
        await using var db = CreateDb();

        (await EvidenceEpaResolver.ResolveAsync(db, SchemaWithPointer(), """{ "epa_id": 9999 }""", CancellationToken.None))
            .Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_IsNullWithoutAPointer_EvenForAnEpaThatExists()
    {
        await using var db = CreateDb();

        (await EvidenceEpaResolver.ResolveAsync(db, SchemaWithoutPointer(), """{ "epa_id": 17 }""", CancellationToken.None))
            .Should().BeNull();
    }

    private static ApplicationDbContext CreateDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Epas.Add(new Epa { Id = KnownEpaId, SubSpecialityId = 1, Code = "PAED-001", Title = "Take a history", IsActive = true });
        db.SaveChanges();
        return db;
    }

    private static FormSchema SchemaWithPointer() => FormSchemaParser.Parse("""
        {
          "version": 1,
          "evidence_epa_field": "epa_id",
          "sections": [ { "key": "s", "title": "S", "fields": [ { "key": "epa_id", "type": "epa", "label": "EPA" } ] } ]
        }
        """);

    private static FormSchema SchemaWithoutPointer() => FormSchemaParser.Parse("""
        { "version": 1, "sections": [ { "key": "s", "title": "S", "fields": [ { "key": "epa_id", "type": "epa", "label": "EPA" } ] } ] }
        """);
}
