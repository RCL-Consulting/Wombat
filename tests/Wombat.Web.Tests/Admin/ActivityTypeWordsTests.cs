using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The words both activity-type pages print (T398, flow 17, build-contracts § 0): the owner, the scope, T300's notice,
/// the release's lines, Unavailable, the version and the draft (R2-A01, R2-B06, R2-B09, R3-C07).
/// </summary>
public sealed class ActivityTypeWordsTests
{
    [Theory]
    [InlineData(ActivityTypeOwnerKind.College, "CPSA", "CPSA")]
    [InlineData(ActivityTypeOwnerKind.College, "DEMO-C", "DEMO-C")]
    [InlineData(ActivityTypeOwnerKind.Institution, "Kgosi Kgari Teaching Hospital", "Kgosi Kgari Teaching Hospital")]
    [InlineData(ActivityTypeOwnerKind.Platform, null, "The platform")]
    [InlineData(ActivityTypeOwnerKind.College, null, "Its College")]
    [InlineData(ActivityTypeOwnerKind.Institution, null, "Its institution")]
    public void SetBy_NamesTheOwner_NeverByAnEnum(ActivityTypeOwnerKind kind, string? name, string expected)
        => ActivityTypeWords.SetBy(new ActivityTypeOwnerDto(kind, name)).Should().Be(expected);

    [Theory]
    [InlineData(ActivityScope.Speciality, "Paediatrics", "Speciality · Paediatrics")]
    [InlineData(ActivityScope.SubSpeciality, "Paediatrics / Neonatology", "Sub-speciality · Paediatrics / Neonatology")]
    [InlineData(ActivityScope.Institution, "Kgosi Kgari Teaching Hospital", "Institution")]
    [InlineData(ActivityScope.Global, null, "Global")]
    [InlineData(ActivityScope.Speciality, null, "Speciality")]
    public void ScopeDetail_IsTheScope_WithItsDisciplineByName(ActivityScope scope, string? target, string expected)
        => ActivityTypeWords.ScopeDetail(scope, target).Should().Be(expected);

    [Theory]
    [InlineData(ActivityScope.Speciality, "Paediatrics",
        "Set by the College that owns Paediatrics. You can read this activity type here, but not change it.")]
    [InlineData(ActivityScope.SubSpeciality, null, "Set by the College. You can read this activity type here, but not change it.")]
    [InlineData(ActivityScope.Institution, "Kgosi Kgari Teaching Hospital",
        "Set by Kgosi Kgari Teaching Hospital. You can read this activity type here, but not change it.")]
    [InlineData(ActivityScope.Institution, null, "Set by its institution. You can read this activity type here, but not change it.")]
    [InlineData(ActivityScope.Global, null, "Set by a global administrator. You can read this activity type here, but not change it.")]
    public void ReadOnlyNotice_IsT300sThree_WordForWord(ActivityScope scope, string? target, string expected)
        => ActivityTypeWords.ReadOnlyNotice(scope, target).Should().Be(expected);

    [Fact]
    public void TheReleasesLines_NameTheRelease()
    {
        ActivityTypeWords.WrittenByLine("the MSF release").Should().Be("Written by the MSF release.");
        ActivityTypeWords.ReleaseNotice("the learner-feedback release").Should().Be(
            "Written by the learner-feedback release. Its form changes only with a release of Wombat. Nobody is offered it when filing an activity.");
    }

    [Fact]
    public void Unavailable_IsTheSystemsThreeLines()
    {
        ActivityTypeWords.Unavailable.Heading.Should().Be("Activity type unavailable");
        ActivityTypeWords.Unavailable.Body.Should().Be("This activity type does not exist, or you cannot open it.");
        ActivityTypeWords.Unavailable.GoTo.Should().Be("Go to Activity types");
    }

    [Theory]
    [InlineData(1, "Version 1")]
    [InlineData(12, "Version 12")]
    [InlineData(0, "Not published")]
    public void Published_IsTheVersion_NeverV0(int version, string expected)
        => ActivityTypeWords.Published(version).Should().Be(expected);

    [Fact]
    public void Draft_IsTheSouthAfricanDayOfTheSave_OrNone()
    {
        ActivityTypeWords.Draft(new DateTime(2026, 11, 5, 9, 0, 0, DateTimeKind.Utc)).Should().Be("Saved 2026-11-05");
        ActivityTypeWords.Draft(new DateTime(2026, 11, 4, 22, 30, 0, DateTimeKind.Utc))
            .Should().Be("Saved 2026-11-05", "22:30 UTC is already the next day in South Africa");
        ActivityTypeWords.Draft(null).Should().Be("None");
    }
}
