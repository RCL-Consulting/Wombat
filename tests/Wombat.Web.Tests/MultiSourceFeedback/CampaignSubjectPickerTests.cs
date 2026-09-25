using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// T224: the campaign form's trainee picker offers nobody the create command refuses whoever the subject is. Never the
/// caller themselves, whatever role brought them to the page, and nobody at all to a caller who holds Trainee
/// (<see cref="MsfCampaignRules.IsKeptFromCampaignsAbout" />). The picker is also what the EPA list trusts: it reads the
/// curriculum of a trainee the picker offered, and of no one else.
/// </summary>
public sealed class CampaignSubjectPickerTests : TestContext
{
    private const string Registrar = "trainee-1";
    private const string Classmate = "trainee-2";

    private readonly TestAuthorizationContext _auth;
    private readonly ReferenceData _referenceData = new();

    public CampaignSubjectPickerTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("caller@test");

        Services.AddSingleton<IScopedSender>(new FakeSender());
        Services.AddSingleton<IActivityReferenceDataService>(_referenceData);
    }

    [Fact]
    public void ACoordinator_IsOfferedEveryTraineeTheListReturns()
    {
        SignIn("coordinator-1", WombatRoles.Coordinator);

        OfferedSubjects(RenderComponent<CampaignEdit>()).Should().Equal(Registrar, Classmate);
    }

    [Theory]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.Administrator)]
    public void TheCaller_IsNeverOfferedAsTheSubject_WhateverRoleBroughtThemHere(string role)
    {
        SignIn(Registrar, role);

        OfferedSubjects(RenderComponent<CampaignEdit>()).Should().Equal(Classmate);
    }

    /// <summary>
    /// A caller who holds Trainee is shown no picker at all: the create form is not there for them (T224 review), and the
    /// page says why (<see cref="RunsNoCampaignsPageTests" />). Nobody is listed for them and no curriculum is read.
    /// </summary>
    [Theory]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.Administrator)]
    public void ACallerWhoHoldsTrainee_IsOfferedNobody_AndNoCurriculumIsRead(string role)
    {
        SignIn(Registrar, role, WombatRoles.Trainee);
        var cut = RenderComponent<CampaignEdit>();

        cut.FindAll("#msf-subject").Should().BeEmpty();
        OfferedSubjects(cut).Should().BeEmpty();
        _referenceData.Asked.Should().BeEmpty();
    }

    /// <summary>
    /// The subject exclusion holds by itself in the picker: a value edited into the select naming the caller is not a
    /// trainee the picker offered, so no curriculum is read for it. (T224)
    /// </summary>
    [Fact]
    public void AForgedChoiceOfTheCaller_ReadsNoCurriculum()
    {
        SignIn(Registrar, WombatRoles.Coordinator);
        var cut = RenderComponent<CampaignEdit>();

        cut.Find("#msf-subject").Change(Registrar);
        _referenceData.Asked.Should().BeEmpty();

        cut.Find("#msf-subject").Change(Classmate);
        _referenceData.Asked.Should().Equal(Classmate);
    }

    private void SignIn(string userId, params string[] roles)
    {
        _auth.SetRoles(roles);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private static string[] OfferedSubjects(IRenderedComponent<CampaignEdit> cut)
        => cut.FindAll("#msf-subject option")
            .Select(option => option.GetAttribute("value") ?? string.Empty)
            .Where(value => value.Length > 0)
            .ToArray();

    private static TraineeProfileDto Trainee(string userId, string firstName)
        => new(1, userId, $"{userId}@test", firstName, "Mokoena", 2, "Paediatrics", "11.1", 3, "Paediatrics", 4,
            "General Paediatrics", new DateOnly(2026, 1, 1), new DateOnly(2029, 12, 31), true);

    private sealed class ReferenceData : StubActivityReferenceDataService
    {
        public List<string> Asked { get; } = [];

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetSubjectCurriculumEpaOptionsAsync(
            string subjectUserId, string? permittedToolKey = null, CancellationToken cancellationToken = default)
        {
            Asked.Add(subjectUserId);
            return Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([new("101", "PAED-001 Resuscitate a critically ill child")]);
        }
    }

    private sealed class FakeSender : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                ListMsfTemplatesQuery => (IReadOnlyList<MsfTemplateDto>)[new MsfTemplateDto(1, "Default MSF", null, false, true, [])],
                ListTraineesForSpecialityQuery => (IReadOnlyList<TraineeProfileDto>)
                    [Trainee(Registrar, "Thandi"), Trainee(Classmate, "Sipho")],
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
