using System.Text.Json;
using FluentAssertions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Activities.Schema;
using Wombat.Infrastructure.Reporting;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// T199: an activity's EPA, person and campaign fields print in the portfolio export as what they name, and a value that
/// names nothing the export found prints as the page would show it, never as an account id.
/// </summary>
/// <remarks>
/// <c>PortfolioPdfServiceTests.TheActivitiesSection_NamesAnEpaAPersonAndACampaign_NeverByTheirIds</c> reads the names from
/// the database and renders them; these hold the rule for each kind, including its fallback.
/// </remarks>
public sealed class ActivitiesSectionReferenceFieldTests
{
    private static readonly PortfolioFieldReferences References = new(
        new Dictionary<int, string> { [3] = "PAED-003 — Resuscitation" },
        new Dictionary<string, string>(StringComparer.Ordinal) { ["assessor-1"] = "Thandi Zulu" },
        new Dictionary<int, string> { [7] = "Annual MSF (Campaign #7)" });

    [Fact]
    public void AnEpaField_PrintsTheEpa_WhetherStoredAsTextOrAsANumber()
    {
        Text(MiniCex("epa_id"), """{ "epa_id": "3" }""", "mini_cex_cpsa").Should().Be("PAED-003 — Resuscitation");
        Text(MsfRecord("epa_id"), """{ "epa_id": 3 }""", MsfEvidenceKinds.MsfActivityTypeKey).Should().Be("PAED-003 — Resuscitation");
    }

    [Fact]
    public void AnEpaTheExportDidNotFind_PrintsAsStored()
    {
        // As a value no option declares prints as stored (T191): the export has nothing truer to say about it.
        Text(MiniCex("epa_id"), """{ "epa_id": "99" }""", "mini_cex_cpsa").Should().Be("99");
    }

    [Fact]
    public void APersonField_PrintsTheirName_OrUnknownPerson_NeverTheirAccountId()
    {
        Text(MiniCex("assessor_user_id"), """{ "assessor_user_id": "assessor-1" }""", "mini_cex_cpsa").Should().Be("Thandi Zulu");
        Text(MiniCex("assessor_user_id"), """{ "assessor_user_id": "gone-7f3a" }""", "mini_cex_cpsa")
            .Should().Be(PortfolioFieldReferences.UnknownPerson);
        Text(MiniCex("assessor_user_id"), """{ "assessor_user_id": "" }""", "mini_cex_cpsa").Should().BeNull("nobody was named");
    }

    [Theory]
    [InlineData(MsfEvidenceKinds.MsfActivityTypeKey)]
    [InlineData(MsfEvidenceKinds.LearnerFeedbackActivityTypeKey)]
    public void AnEvidenceRecordsCampaign_PrintsAsTheMsfSectionHeadsIt_OrByItsNumber(string evidenceTypeKey)
    {
        Text(MsfRecord("campaign_id"), """{ "campaign_id": 7 }""", evidenceTypeKey).Should().Be("Annual MSF (Campaign #7)");
        Text(MsfRecord("campaign_id"), """{ "campaign_id": 8 }""", evidenceTypeKey).Should().Be("Campaign #8");
    }

    [Fact]
    public void ANumberFieldCalledCampaignId_OnAnyOtherType_IsJustANumber()
    {
        // Only a release's evidence record names a campaign there; a builder type's field of that name is the author's.
        Text(MsfRecord("campaign_id"), """{ "campaign_id": 7 }""", "institution_quality_project").Should().Be("7");
        Text(MsfRecord("campaign_id"), """{ "campaign_id": 7 }""", activityTypeKey: null).Should().Be("7");
    }

    private static string? Text(FormField field, string dataJson, string? activityTypeKey)
    {
        using var document = JsonDocument.Parse(dataJson);
        return ActivitiesSectionComponent.FieldValueText(
            field, document.RootElement, EntrustmentRungLookup.Empty, References, activityTypeKey);
    }

    private static FormField MiniCex(string key) => SeedField("mini_cex_cpsa", key);

    private static FormField MsfRecord(string key) => SeedField(MsfEvidenceKinds.MsfActivityTypeKey, key);

    private static FormField SeedField(string seed, string key)
        => FormSchemaParser.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", seed, "schema.json")))
            .Sections
            .SelectMany(section => section.Fields)
            .Single(field => field.Key == key);
}
