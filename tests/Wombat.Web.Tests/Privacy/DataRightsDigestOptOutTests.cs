using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Profile;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Privacy;

/// <summary>
/// T240: the Data rights page says what "Opt out of digest emails" stops.
/// </summary>
/// <remarks>
/// Since T151 and T240 the flag stops three periodic reminders, of which only the coordinator's is called a digest: the
/// draft reminder and the reminder of activities waiting on an assessor stop too. A trainee who ticked the box, or an
/// assessor who did, would not otherwise know it, so the checkbox is described by help text naming all three, and what
/// still arrives. Until T240 the two preference checkboxes were wrapped in classes app.css does not define, with no id
/// and no <c>&lt;label for&gt;</c>; they now follow DESIGN.md's checkbox rule.
/// </remarks>
public sealed class DataRightsDigestOptOutTests : TestContext
{
    private readonly RecordingSender _sender = new();

    public DataRightsDigestOptOutTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IScopedSender>(_sender);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void TheDigestOptOut_IsDescribedByWhatItStops_AndWhatItDoesNot()
    {
        var cut = RenderComponent<DataRights>();

        var checkbox = cut.Find("#data-rights-digest-emails");
        checkbox.GetAttribute("aria-describedby").Should().Be("data-rights-digest-emails-help");

        var help = Text(cut.Find("#data-rights-digest-emails-help"));
        help.Should().Contain("the weekly coordinator digest")
            .And.Contain("the reminder of drafts left untouched for 14 days")
            .And.Contain("the reminder of activities that have waited more than 5 days for you to assess")
            .And.Contain("Email about one particular thing is still sent");
    }

    /// <summary>
    /// T358, review 11: the nudge's days are one setting, <c>DashboardThresholds.AssessorNudgeDays</c>, which this help reads
    /// where it wrote 5 by hand; the words are otherwise unchanged (E1).
    /// </summary>
    [Fact]
    public void TheNudgesDays_AreReadFromTheSetting()
    {
        Services.Configure<Wombat.Application.Common.Options.DashboardThresholds>(thresholds => thresholds.AssessorNudgeDays = 3);

        var cut = RenderComponent<DataRights>();

        Text(cut.Find("#data-rights-digest-emails-help"))
            .Should().Contain("the reminder of activities that have waited more than 3 days for you to assess");
    }

    [Fact]
    public void BothPreferences_AreLabelledCheckboxes_AndEveryReferenceResolves()
    {
        var cut = RenderComponent<DataRights>();

        foreach (var id in new[] { "data-rights-optional-processing", "data-rights-digest-emails" })
        {
            var checkbox = cut.Find($"#{id}");
            checkbox.GetAttribute("type").Should().Be("checkbox");
            checkbox.ParentElement!.ClassList.Should().Contain("form-check");
            cut.FindAll($"label[for='{id}']").Should().NotBeEmpty();
        }

        AccessibleNames.NameOf(cut, cut.Find("#data-rights-digest-emails")).Should().Contain("Opt out of digest emails");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void TickingTheDigestOptOut_AndSaving_SendsIt()
    {
        var cut = RenderComponent<DataRights>();

        cut.Find("#data-rights-digest-emails").Change(true);
        cut.FindAll("button").Single(button => Text(button) == "Save preferences").Click();

        var command = _sender.Received.OfType<UpdateObjectionFlagsCommand>().Should().ContainSingle().Which;
        command.OptOutOfDigestEmails.Should().BeTrue();
        command.OptOutOfOptionalProcessing.Should().BeFalse();
    }

    private static string Text(IElement element)
        => string.Join(' ', element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed class RecordingSender : IScopedSender
    {
        public List<object> Received { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            object answer = request switch
            {
                GetObjectionFlagsQuery => new ObjectionFlagsDto(OptOutOfOptionalProcessing: false, OptOutOfDigestEmails: false),
                GetMyDataRightsRequestsQuery => Array.Empty<DataRightsRequestSummaryDto>(),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };
            return Task.FromResult((TResponse)answer);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            return request is UpdateObjectionFlagsCommand
                ? Task.CompletedTask
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}
