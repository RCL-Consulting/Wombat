using System.Security.Claims;
using FluentAssertions;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.DataRights;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Tests.DataRights;

/// <summary>
/// T112 — who may act on a data-rights request.
/// </summary>
/// <remarks>
/// <para>
/// The feature gated on role alone in six hand-written copies, so any Coordinator in any institution
/// could list, open, download, approve and <b>erase</b> any other institution's requests. A subject
/// access report is everything the product holds about one person, and erasure is irreversible.
/// </para>
/// <para>
/// These tests exist because the first cut of T112 had NO coverage of the institution comparison: an
/// adversarial review deleted the conjunction from <c>CanAct</c> and all 449 Application tests still
/// passed. Every pre-existing reviewer test used an Administrator principal, which short-circuits
/// before the comparison is reached. If this file is ever deleted, the security property goes back to
/// being unobservable.
/// </para>
/// </remarks>
public sealed class DataRightsAuthorizationTests
{
    private const int HomeInstitution = 1;
    private const int OtherInstitution = 2;

    // ─── The conjunction itself ──────────────────────────────────────────────

    [Fact]
    public void ACoordinatorMayReviewTheirOwnInstitutionsRequest()
        => DataRightsAuthorization
            .CanReview(Coordinator(HomeInstitution), Request(HomeInstitution))
            .Should().BeTrue();

    /// <summary>The headline defect: role alone was enough, in any institution.</summary>
    [Fact]
    public void ACoordinatorMayNotReviewAnotherInstitutionsRequest()
        => DataRightsAuthorization
            .CanReview(Coordinator(OtherInstitution), Request(HomeInstitution))
            .Should().BeFalse();

    [Fact]
    public void ACoordinatorWithNoInstitutionClaimMayReviewNothing()
        => DataRightsAuthorization
            .CanReview(Coordinator(null), Request(HomeInstitution))
            .Should().BeFalse();

    /// <summary>
    /// An unplaceable request is Administrator-only. Erasure cannot be undone, so a missing stamp
    /// withholds the power rather than spreading it to every Coordinator.
    /// </summary>
    [Fact]
    public void AnUnstampedRequestIsReviewableByNoScopedCoordinator()
        => DataRightsAuthorization
            .CanReview(Coordinator(HomeInstitution), Request(institutionId: null))
            .Should().BeFalse();

    /// <summary>
    /// Both sides are <c>int?</c>, so a naive <c>==</c> would make an unstamped request match a
    /// claimless caller.
    /// </summary>
    [Fact]
    public void AnUnstampedRequestDoesNotMatchAClaimlessCoordinator()
        => DataRightsAuthorization
            .CanReview(Coordinator(null), Request(institutionId: null))
            .Should().BeFalse();

    [Fact]
    public void AGlobalAdministratorMayReviewAnyInstitutionsRequest()
        => DataRightsAuthorization
            .CanReview(Administrator(), Request(OtherInstitution))
            .Should().BeTrue();

    [Fact]
    public void AGlobalAdministratorMayReviewAnUnstampedRequest()
        => DataRightsAuthorization
            .CanReview(Administrator(), Request(institutionId: null))
            .Should().BeTrue();

    // ─── Role sets stay distinct ─────────────────────────────────────────────

    [Fact]
    public void ACoordinatorMayNotRectify()
        => DataRightsAuthorization
            .CanRectify(Coordinator(HomeInstitution), Request(HomeInstitution))
            .Should().BeFalse();

    [Fact]
    public void ASpecialityAdminMayRectifyInTheirOwnInstitution()
        => DataRightsAuthorization
            .CanRectify(SpecialityAdmin(HomeInstitution), Request(HomeInstitution))
            .Should().BeTrue();

    [Fact]
    public void ASpecialityAdminMayNotRectifyAnotherInstitutionsRequest()
        => DataRightsAuthorization
            .CanRectify(SpecialityAdmin(OtherInstitution), Request(HomeInstitution))
            .Should().BeFalse();

    [Fact]
    public void ASpecialityAdminMayNotReview()
        => DataRightsAuthorization
            .CanReview(SpecialityAdmin(HomeInstitution), Request(HomeInstitution))
            .Should().BeFalse();

    [Fact]
    public void ATraineeMayNeitherReviewNorRectify()
    {
        var trainee = Build("trainee-1", HomeInstitution, WombatRoles.Trainee);

        DataRightsAuthorization.CanReview(trainee, Request(HomeInstitution)).Should().BeFalse();
        DataRightsAuthorization.CanRectify(trainee, Request(HomeInstitution)).Should().BeFalse();
    }

    // ─── The data subject ────────────────────────────────────────────────────

    [Fact]
    public void TheRequesterMayReadTheirOwnRequest()
    {
        var request = Request(HomeInstitution, requesterUserId: "subject-1");
        var act = () => DataRightsAuthorization.DemandReadAccess(Build("subject-1", HomeInstitution), request);

        act.Should().NotThrow();
    }

    /// <summary>
    /// The export is narrower than the metadata read. Scoping the request row does not scope the
    /// bundle it releases — the report is assembled by person, so it can carry rows stamped to other
    /// institutions that a reviewer cannot open individually. The subject collects their own; the
    /// only download link in the product is on their own profile page.
    /// </summary>
    [Fact]
    public void AReviewerInScopeMayReadTheRequestButNotExportTheBundle()
    {
        var request = Request(HomeInstitution, requesterUserId: "subject-1");
        var coordinator = Coordinator(HomeInstitution);

        var read = () => DataRightsAuthorization.DemandReadAccess(coordinator, request);
        var export = () => DataRightsAuthorization.DemandExportAccess(coordinator, request);

        read.Should().NotThrow();
        export.Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void TheRequesterMayExportTheirOwnBundle()
    {
        var request = Request(HomeInstitution, requesterUserId: "subject-1");
        var act = () => DataRightsAuthorization.DemandExportAccess(Build("subject-1", HomeInstitution), request);

        act.Should().NotThrow();
    }

    [Fact]
    public void AStrangerMayNeitherReadNorExport()
    {
        var request = Request(HomeInstitution, requesterUserId: "subject-1");
        var stranger = Build("stranger-1", HomeInstitution);

        var read = () => DataRightsAuthorization.DemandReadAccess(stranger, request);
        var export = () => DataRightsAuthorization.DemandExportAccess(stranger, request);

        read.Should().Throw<UnauthorizedAccessException>();
        export.Should().Throw<UnauthorizedAccessException>();
    }

    /// <summary>
    /// Every refusal carries the same words, so the message cannot distinguish "not yours" from
    /// "wrong institution" from "no such request".
    /// </summary>
    [Fact]
    public void EveryRefusalCarriesTheSameMessage()
    {
        var request = Request(HomeInstitution, requesterUserId: "subject-1");

        var outOfScope = Record.Exception(() =>
            DataRightsAuthorization.DemandReviewAccess(Coordinator(OtherInstitution), request));
        var notYours = Record.Exception(() =>
            DataRightsAuthorization.DemandExportAccess(Build("stranger-1", HomeInstitution), request));

        outOfScope.Should().BeOfType<UnauthorizedAccessException>();
        notYours.Should().BeOfType<UnauthorizedAccessException>();
        outOfScope!.Message.Should().Be(notYours!.Message);
        outOfScope.Message.Should().Be(DataRightsAuthorization.RefusalMessage);
    }

    // ─── The list gate ───────────────────────────────────────────────────────

    [Fact]
    public void CanReviewAnythingAnswersRoleOnly()
    {
        // Deliberately does NOT decide WHICH requests — the query filters by institution separately,
        // so that a reviewer with no institution claim gets an empty list rather than everyone's.
        DataRightsAuthorization.CanReviewAnything(Coordinator(null)).Should().BeTrue();
        DataRightsAuthorization.CanReviewAnything(Administrator()).Should().BeTrue();
        DataRightsAuthorization.CanReviewAnything(Build("trainee-1", HomeInstitution, WombatRoles.Trainee))
            .Should().BeFalse();
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private static DataRightsRequest Request(int? institutionId, string requesterUserId = "subject-1")
        => DataRightsRequest.Create(
            requesterUserId,
            "Subject One",
            DataRightsRequestType.Erasure,
            "Please erase my record.",
            DateTime.UtcNow,
            institutionId);

    private static ClaimsPrincipal Administrator()
        => Build("admin-1", null, WombatRoles.Administrator);

    private static ClaimsPrincipal Coordinator(int? institutionId)
        => Build("coord-1", institutionId, WombatRoles.Coordinator);

    private static ClaimsPrincipal SpecialityAdmin(int? institutionId)
        => Build("spec-admin-1", institutionId, WombatRoles.SpecialityAdmin);

    private static ClaimsPrincipal Build(string userId, int? institutionId = null, string? role = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        if (role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
