using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Reporting;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Reporting;

/// <summary>
/// The export check (T379, flow 13, S10, T313): a hash is read as certutil and Get-FileHash print it, in either case, with
/// spaces and line breaks in it; what cannot be a SHA-256 hash is refused with its reason before anything is looked up;
/// and a hash exported more than once is answered with its first generation, its count and its last export.
/// </summary>
public sealed class VerifyExportQueryHandlerTests
{
    private const string Hash = "aba6ce7943c20681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995";

    [Fact]
    public async Task AHashExportedOnce_IsVerified_AsThatExport()
    {
        await using var db = CreateDb();
        db.Set<PortfolioExport>().Add(Export(new DateTime(2026, 10, 26, 20, 56, 0, DateTimeKind.Utc), "trainee-1"));
        await db.SaveChangesAsync();

        var answer = await Check(db, Hash);

        answer.Refusal.Should().BeNull();
        var export = answer.Export!;
        export.Count.Should().Be(1);
        export.FirstExportedOn.Should().Be(new DateTime(2026, 10, 26, 20, 56, 0, DateTimeKind.Utc));
        export.LastExportedOn.Should().Be(export.FirstExportedOn);
        export.TraineeUserId.Should().Be("trainee-1");
        export.FirstExportedByUserId.Should().Be("trainee-1");
        export.FilterFromDate.Should().Be(new DateOnly(2023, 1, 15));
        export.FilterToDate.Should().Be(new DateOnly(2026, 10, 26));
        export.FileName.Should().Be("portfolio-aba6ce7943c2.pdf");
        export.ContentHash.Should().Be(Hash);
    }

    /// <summary>
    /// Step 5.13: three identical exports. The first is the generation and its exporter the first; the last is the latest
    /// by moment, then by id, whichever order the rows were written in.
    /// </summary>
    [Fact]
    public async Task ThreeIdenticalExports_AreOneAnswer_TheFirstGeneration_TheCount_AndTheLast()
    {
        await using var db = CreateDb();
        var first = new DateTime(2026, 10, 26, 20, 51, 0, DateTimeKind.Utc);
        var last = new DateTime(2026, 10, 26, 20, 52, 0, DateTimeKind.Utc);
        db.Set<PortfolioExport>().AddRange(
            Export(last, "smit"),
            Export(first, "trainee-1"),
            Export(first.AddSeconds(30), "admin"));
        await db.SaveChangesAsync();

        var export = (await Check(db, Hash)).Export!;

        export.Count.Should().Be(3);
        export.FirstExportedOn.Should().Be(first);
        export.FirstExportedByUserId.Should().Be("trainee-1", "the first exporter, not the last");
        export.LastExportedOn.Should().Be(last);
    }

    [Theory]
    [InlineData("ABA6CE7943C20681A22F7BBFD89A51B9E19FF005892E318806C28DD215A6C995")]
    [InlineData("  aba6ce7943c20681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995 ")]
    [InlineData("ab a6 ce 79 43 c2 06 81 a2 2f 7b bf d8 9a 51 b9\ne1 9f f0 05 89 2e 31 88 06 c2 8d d2 15 a6 c9 95")]
    [InlineData("ABA6CE7943C20681A22F7BBFD89A51B9\r\n\tE19FF005892E318806C28DD215A6C995")]
    [InlineData("﻿aba6ce7943c20681a22f7bbfd89a51b9​e19ff005892e318806c28dd215a6c995​")]
    public async Task UpperCase_Spaces_AndLineBreaks_Verify(string typed)
    {
        await using var db = CreateDb();
        db.Set<PortfolioExport>().Add(Export(new DateTime(2026, 10, 26, 20, 51, 0, DateTimeKind.Utc), "trainee-1"));
        await db.SaveChangesAsync();

        var answer = await Check(db, typed);

        answer.Refusal.Should().BeNull();
        answer.Export.Should().NotBeNull("Get-FileHash prints upper case and certutil's older form spaces it (T313)");
    }

    [Fact]
    public async Task AWellFormedHashNoExportHas_IsNotFound_AndNotRefused()
    {
        await using var db = CreateDb();
        db.Set<PortfolioExport>().Add(Export(new DateTime(2026, 10, 26, 20, 51, 0, DateTimeKind.Utc), "trainee-1"));
        await db.SaveChangesAsync();

        var answer = await Check(db, "5c171ec5" + Hash[8..]);

        answer.Refusal.Should().BeNull();
        answer.Export.Should().BeNull();
    }

    /// <summary>T313: an export is never deleted by age, so a file exported long ago still verifies.</summary>
    [Fact]
    public async Task AnExportAYearOld_StillVerifies()
    {
        await using var db = CreateDb();
        db.Set<PortfolioExport>().Add(Export(new DateTime(2025, 10, 1, 8, 0, 0, DateTimeKind.Utc), "trainee-1"));
        await db.SaveChangesAsync();

        (await Check(db, Hash)).Export.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" \r\n ")]
    public async Task NothingButSpaces_IsRefusedAsBlank(string typed)
    {
        await using var db = CreateDb();

        var answer = await Check(db, typed);

        answer.Refusal.Should().Be(VerifyExportRefusal.Blank);
        answer.Export.Should().BeNull();
    }

    /// <summary>S10, V06: the 12 hexadecimal digits in a file's name are the start of its hash, not the hash.</summary>
    [Fact]
    public async Task AFileNamesTwelveDigits_AreRefusedAsTheFileNamesPrefix()
    {
        await using var db = CreateDb();
        db.Set<PortfolioExport>().Add(Export(new DateTime(2026, 10, 26, 20, 51, 0, DateTimeKind.Utc), "trainee-1"));
        await db.SaveChangesAsync();

        var answer = await Check(db, "ABA6CE7943C2");

        answer.Refusal.Should().Be(VerifyExportRefusal.FileNamePrefix);
        answer.Length.Should().Be(12);
        answer.Export.Should().BeNull("a prefix is refused, never matched against the stored hashes");
    }

    [Theory]
    [InlineData("aba6ce79", 8)]
    [InlineData(Hash + "0", 65)]
    [InlineData("ab a6 ce", 6)]
    public async Task AWrongLength_IsRefused_WithTheLengthAsCounted(string typed, int length)
    {
        await using var db = CreateDb();

        var answer = await Check(db, typed);

        answer.Refusal.Should().Be(VerifyExportRefusal.WrongLength);
        answer.Length.Should().Be(length, "counted without the spaces, which are accepted");
    }

    /// <summary>
    /// A character that is no hexadecimal digit is named, as typed, before any length is counted: a hash with a letter O
    /// for a zero has the right length and is still not a hash.
    /// </summary>
    [Theory]
    [InlineData("aba6ce7943c2O681a22f7bbfd89a51b9e19ff005892e318806c28dd215a6c995", "O")]
    [InlineData("SHA256 hash of portfolio.pdf:", "S")]
    [InlineData("abcg", "g")]
    public async Task ACharacterThatIsNoHexadecimalDigit_IsRefused_AndNamed(string typed, string character)
    {
        await using var db = CreateDb();

        var answer = await Check(db, typed);

        answer.Refusal.Should().Be(VerifyExportRefusal.NotHexadecimal);
        answer.Character.Should().Be(character);
    }

    private static Task<VerifyExportAnswer> Check(ApplicationDbContext db, string typed)
        => new VerifyExportQueryHandler(db).Handle(new VerifyExportQuery(typed), CancellationToken.None);

    private static PortfolioExport Export(DateTime exportedOn, string exportedBy) => new()
    {
        TraineeUserId = "trainee-1",
        ExportedByUserId = exportedBy,
        ExportedOn = exportedOn,
        FilterFromDate = new DateOnly(2023, 1, 15),
        FilterToDate = new DateOnly(2026, 10, 26),
        ContentHash = Hash,
        FileName = "portfolio-aba6ce7943c2.pdf"
    };

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
