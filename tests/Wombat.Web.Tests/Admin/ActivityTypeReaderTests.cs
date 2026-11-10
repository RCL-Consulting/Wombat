using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Identity;
using Wombat.Web.Tests.Activities;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, Q9, Q12, T1, T271 item 2, T300, N3; R2-B01–B06, B09–B11, R3-B07): a reader's page, a system-managed
/// type's, Unavailable, loading and the owner line.
/// </summary>
public sealed class ActivityTypeReaderTests : TestContext
{
    private const string ScaleSchema = """
        {
          "version": 1,
          "rated_level_field": "overall_level",
          "sections": [ { "key": "assessment", "title": "Entrustment", "fields": [
            { "key": "overall_level", "type": "scale", "label": "Supervision required for this encounter", "required": true, "scale_key": "seed:cpsa:scale:v11.1" } ] } ]
        }
        """;

    public ActivityTypeReaderTests() => BuilderKit.SignIn(this, WombatRoles.InstitutionalAdmin);

    [Fact]
    public void AReader_SeesTheTitleAsTheName_TheNotice_TheOwnerLine_AndTheStrip()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(Mini()));

        cut.Find("h1").TextContent.Should().Be("Mini-CEX (Paediatrics)");
        cut.Find(".page-subtitle").TextContent.Should().Contain("What a registrar fills in, who moves it on, and what it counts towards.")
            .And.Contain("CPSA · Speciality · Paediatrics", "the owner line by the College's short code (settled here)");
        cut.Find("#activity-type-read-only").TextContent.Should().Be(
            "Set by the College that owns Paediatrics. You can read this activity type here, but not change it.");
        cut.Find(".builder-strip").TextContent.Should().Contain("Version 1, 2026-11-05, Wombat's catalogue");
        cut.FindAll("#builder-save, #builder-publish, #builder-discard, dialog").Should().BeEmpty();
        cut.Find("label[for=show-form]").TextContent.Should().Be("The form");
    }

    [Fact]
    public void AReader_ReadsTheScaleByItsName_ASeedKeyResolved()
    {
        Services.AddSingleton<IActivityReferenceDataService>(new Scales());
        var cut = BuilderKit.Render(this, new BuilderSender(Mini()));

        BuilderKit.Named(cut, "View field Supervision required for this encounter").Click();

        cut.FindAll(".field-editor-inplace dt").Single(dt => dt.TextContent == "Entrustment scale").NextElementSibling!.TextContent
            .Should().Be("CPSA Paediatric Entrustment Scale v11.1", "T271 item 2: a seed: key resolves to its scale");
        cut.Find("#panel-form").TextContent.Should().Contain("Supervision required for this encounter (overall_level)");
    }

    [Fact]
    public void AWriter_PicksTheScaleByName_TheSeedKeyKeptAsStored()
    {
        Services.AddSingleton<IActivityReferenceDataService>(new Scales());
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk(draftSchema: ScaleSchema)));

        BuilderKit.Named(cut, "Edit field Supervision required for this encounter").Click();

        cut.Find("#field-scale").GetAttribute("value").Should().Be("seed:cpsa:scale:v11.1");
        cut.FindAll("#field-scale option").Select(option => (option.GetAttribute("value"), option.TextContent.Trim()))
            .Should().Contain(("seed:cpsa:scale:v11.1", "CPSA Paediatric Entrustment Scale v11.1"));
    }

    [Fact]
    public void ASystemManagedType_IsReadOnly_WithTheReleasesNotice()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(Mini() with { SystemManaged = true, WrittenBy = "the MSF release", Name = "Multi-Source Feedback (Paediatrics)" }));

        cut.Find("#activity-type-read-only").TextContent.Should().Be(
            "Written by the MSF release. Its form changes only with a release of Wombat. Nobody is offered it when filing an activity.");
        cut.Find("#activity-type-read-only").HasAttribute("role").Should().BeFalse("a standing notice is no live region");
    }

    [Fact]
    public void AnIdNoneMayRead_IsUnavailable_NeverForbidden()
    {
        var cut = BuilderKit.Render(this, new BuilderSender((ActivityTypeEditorDto?)null), id: 404);

        cut.Find("h1").TextContent.Should().Be("Activity type unavailable");
        cut.Find(".state-panel-copy").TextContent.Should().Be("This activity type does not exist, or you cannot open it.");
        cut.Find(".detail-card--empty a").GetAttribute("href").Should().Be("/admin/activity-types");
        cut.Find(".detail-card--empty a").TextContent.Should().Be("Go to Activity types");
        cut.FindAll(".page-subtitle, .tab-bar, button#builder-save").Should().BeEmpty();
        TabTitle.Of(this, cut).Should().Be("Activity type unavailable · Wombat");
    }

    [Fact]
    public void WhileItLoads_ItSaysSo()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(new TaskCompletionSource<ActivityTypeEditorDto?>().Task));

        cut.Find(".loading-line").TextContent.Should().Be("Loading this activity type.");
        cut.Find(".loading-line").GetAttribute("role").Should().Be("status");
        cut.Find("h1").TextContent.Should().Be("Activity type");
    }

    [Fact]
    public void AWritersTitle_IsEditTheName_AndNoBackToList()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        cut.Find("h1").TextContent.Should().Be("Edit KGK Teaching Session Log");
        cut.Markup.Should().NotContain("Back to list");
        cut.Find(".page-subtitle").TextContent.Should().Contain("Kgosi Kgari Teaching Hospital · Institution");
        cut.Find(".builder-strip").TextContent.Should().Contain("Not published").And.Contain("Saved 2026-11-05 by Nolwazi Mbatha");
    }

    private static ActivityTypeEditorDto Mini() => BuilderKit.Kgk(hasDraft: false, publishedVersion: 1, canWrite: false, publishedSchema: ScaleSchema) with
    {
        Name = "Mini-CEX (Paediatrics)",
        Key = "mini_cex_cpsa",
        Scope = Wombat.Domain.Activities.ActivityScope.Speciality,
        ScopeId = 5,
        ScopeTargetName = "Paediatrics",
        Owner = new ActivityTypeOwnerDto(ActivityTypeOwnerKind.College, "CPSA"),
        Versions = [new ActivityTypeVersionDto(1, BuilderKit.SavedOn, "seed-system") { PublishedByName = "Wombat's catalogue" }]
    };

    private sealed class Scales : StubActivityReferenceDataService
    {
        public override Task<IReadOnlyList<EntrustmentScaleOption>> GetEntrustmentScaleOptionsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<EntrustmentScaleOption>>(
                [new EntrustmentScaleOption("12", "cpsa:scale:v11.1", "CPSA Paediatric Entrustment Scale v11.1"), new EntrustmentScaleOption("3", null, "O-R Scale")]);
    }
}
