using System.IO.Compression;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Reporting;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.DataRights;

/// <summary>
/// T335, flow 01: the acting role is stored with the account (D1), so it is the person's data, and their access report
/// says what is stored. Erasure clears it (<c>ErasureTransactionPostgresTests</c>).
/// </summary>
public sealed class AccessReportProfileTests
{
    [Theory]
    [InlineData("Assessor")]
    [InlineData(null)]
    public async Task TheProfile_CarriesTheStoredActingRole(string? actingRole)
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Users.Add(new WombatIdentityUser
        {
            Id = "t335-subject", UserName = "t.zulu@kgk.test", Email = "t.zulu@kgk.test", FirstName = "Thandi",
            LastName = "Zulu", ActingRole = actingRole
        });
        await db.SaveChangesAsync();

        var export = await new AccessReportBuilder(db, new NoPortfolio()).BuildAsync("t335-subject", CancellationToken.None);

        using var zip = new ZipArchive(new MemoryStream(export.ZipBytes), ZipArchiveMode.Read);
        await using var json = zip.GetEntry("data-export.json")!.Open();
        using var document = await JsonDocument.ParseAsync(json);
        var profile = document.RootElement.GetProperty("profile");
        profile.GetProperty("firstName").GetString().Should().Be("Thandi", "guard: the profile is the subject's");
        if (actingRole is null)
        {
            // The report leaves every null out (WhenWritingNull), as it does an account's missing institution.
            profile.TryGetProperty("actingRole", out _).Should().BeFalse();
        }
        else
        {
            profile.GetProperty("actingRole").GetString().Should().Be(actingRole);
        }
    }

    /// <summary>The PDF is best-effort in the report, and not what this test reads.</summary>
    private sealed class NoPortfolio : IPortfolioPdfService
    {
        public Task<PortfolioExportResult> GenerateAsync(PortfolioExportRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException("The PDF is not part of this test.");
    }
}
