using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Colleges;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Web.Components.Pages.Admin.Invitations;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T283: the invitations list says what became of each invitation's email, offers Resend where it was not delivered,
/// shows the new link a resend made, and after a second failure says to check the address.
/// </summary>
/// <remarks>
/// Until T283 an invitation issued while the mail server was down read as issued and nothing more. The page told the
/// administrator that "email delivery is configured separately", and the invitee never heard of it. Focus after a resend,
/// done or refused, is <c>ActionFocusTests</c>' "InvitationsList Resend".
/// </remarks>
public sealed class InvitationsListDeliveryTests : TestContext
{
    private static readonly DateTime Issued = new(2029, 3, 7, 9, 30, 0, DateTimeKind.Utc);

    public InvitationsListDeliveryTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new WombatOptions { BaseUrl = "https://wombat.test" }));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void EachRow_SaysWhatBecameOfItsEmail_AndFromTheSecondFailure_ToCheckTheAddress()
    {
        var cut = Render(new FakeSender(
            Row(1, "sent@hospital.test", InvitationDelivery.Sent),
            Row(2, "sending@hospital.test", InvitationDelivery.BeingSent),
            Row(3, "dropped-once@hospital.test", InvitationDelivery.NotDelivered, failures: 1),
            Row(4, "dropped-twice@hospital.test", InvitationDelivery.NotDelivered, failures: 2, checkAddress: true)));

        cut.FindAll("thead th").Select(Text).Should().Contain("Delivery");
        Delivery(cut, 1).Should().Be("Sent");
        Delivery(cut, 2).Should().Be("Being sent");
        cut.Find("#invitation-delivery-2").ClassList.Should().Contain("muted", "nothing is wrong yet");
        Delivery(cut, 3).Should().Be(
            "Not delivered. Resend emails a new link in place of the current one, which then stops working.");
        Delivery(cut, 4).Should().Be(
            "Not delivered. Its email has failed 2 times, so the mail server may be refusing this address. Check the " +
            "address; if it is wrong, revoke this invitation and issue a new one to the right address. Resend emails a " +
            "new link in place of the current one, which then stops working.");
    }

    [Fact]
    public void OnlyARowNotDelivered_OffersResend_NamedByItsRow_AndDescribedByWhy()
    {
        var cut = Render(new FakeSender(
            Row(1, "sent@hospital.test", InvitationDelivery.Sent),
            Row(2, "sending@hospital.test", InvitationDelivery.BeingSent),
            Row(3, "dropped@hospital.test", InvitationDelivery.NotDelivered, failures: 1)));

        var resend = cut.FindAll("button").Where(button => Text(button) == "Resend").Should().ContainSingle().Which;
        resend.GetAttribute("aria-label").Should().Be("Resend the Coordinator invitation to dropped@hospital.test");
        resend.GetAttribute("aria-describedby").Should().Be("invitation-delivery-3");
        resend.ClassList.Should().Contain(["btn", "btn-sm", "btn-outline"]);
        resend.Closest(".actions-cell").Should().NotBeNull();

        cut.FindAll("button").Count(button => Text(button) == "Revoke").Should().Be(3, "every row can still be revoked");
    }

    [Fact]
    public void AResend_ShowsTheNewLink_SaysTheOldOneNoLongerWorks_AndTheRowIsBeingSentAgain()
    {
        var sender = new FakeSender(Row(3, "dropped@hospital.test", InvitationDelivery.NotDelivered, failures: 1))
        {
            AfterResend = [Row(3, "dropped@hospital.test", InvitationDelivery.BeingSent, failures: 1)]
        };
        var cut = Render(sender);

        cut.FindAll("button").Single(button => Text(button) == "Resend").Click();

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-success").TextContent.Should().Contain("dropped@hospital.test"));
        sender.Resent.Should().Equal(3);
        Text(cut.Find(".action-result .alert-success")).Should().Be(
            "A new invitation link is being emailed to dropped@hospital.test. The link it replaces no longer works. Copy the " +
            "new link below — it is shown only once.");
        cut.Find(".action-result code").TextContent.Should().Be("https://wombat.test/account/register?token=token-new");
        Text(cut.Find(".action-result .alert-info")).Should().Contain("the Delivery column below says whether that arrived");
        Delivery(cut, 3).Should().Be("Being sent");
        cut.FindAll("button").Should().NotContain(button => Text(button) == "Resend");
    }

    [Fact]
    public void ARefusedResend_IsShown_AndTheListIsReadAgain()
    {
        // The mail arrived after all, while the page was open: the resend is refused, and the row says so.
        var sender = new FakeSender(Row(3, "dropped@hospital.test", InvitationDelivery.NotDelivered, failures: 1))
        {
            Refusal = ResendInvitationCommandHandler.InvitationChanged,
            AfterResend = [Row(3, "dropped@hospital.test", InvitationDelivery.Sent, failures: 1)]
        };
        var cut = Render(sender);

        cut.FindAll("button").Single(button => Text(button) == "Resend").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(ResendInvitationCommandHandler.InvitationChanged));
        cut.Find(".action-result .alert-danger").GetAttribute("role").Should().Be("alert");
        cut.FindAll(".action-result code").Should().BeEmpty("no link was made");
        Delivery(cut, 3).Should().Be("Sent");

        // The row no longer offers Resend, so the button that had the focus is gone: the refusal takes it (T234).
        cut.FindAll("button").Should().NotContain(button => Text(button) == "Resend");
        cut.WaitForAssertion(() => JSInterop.Invocations
            .Where(invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus")
            .Should().ContainSingle());
    }

    [Fact]
    public void AnIssuedInvitationsLink_SaysItIsAlsoBeingEmailed_AndWhereToSeeWhetherItArrived()
    {
        var cut = Render(new FakeSender());

        cut.Find("#invitation-email").Change("registrar@hospital.test");
        cut.Find("form button[type=submit]").Closest("form")!.Submit();

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-success"));
        Text(cut.Find(".action-result .alert-success")).Should().Be(
            "Invitation issued for registrar@hospital.test. Its email is being sent. Copy the link below — it is shown only once.");
        Text(cut.Find(".action-result .alert-info")).Should().Contain(
            "It is also being emailed to the invitee, and the Delivery column below says whether that arrived.");
        Text(cut.Find(".action-result .alert-info")).Should().NotContain("configured separately");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<InvitationsList> Render(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<InvitationsList>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private static string Delivery(IRenderedFragment cut, int invitationId) => Text(cut.Find($"#invitation-delivery-{invitationId}"));

    private static ActiveInvitationDto Row(
        int id, string email, InvitationDelivery delivery, int failures = 0, bool checkAddress = false)
        => new(id, email, WombatRoles.Coordinator, 4, "Groote Schuur Hospital", null, null, 5, "Paediatrics", null, null,
            Issued, new DateOnly(2029, 3, 21), delivery, failures, checkAddress);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender(params ActiveInvitationDto[] rows) : IScopedSender
    {
        private IReadOnlyList<ActiveInvitationDto> _rows = rows;

        /// <summary>What the list reads once a resend has answered, done or refused.</summary>
        public IReadOnlyList<ActiveInvitationDto>? AfterResend { get; init; }

        /// <summary>When set, a resend is refused with this.</summary>
        public string? Refusal { get; init; }

        public List<int> Resent { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object? answer = request switch
            {
                GetInstitutionsListQuery => new[] { new InstitutionDto(4, "Groote Schuur Hospital", "GSH", null, true, Issued) },
                GetCollegesListQuery => Array.Empty<CollegeDto>(),
                GetSpecialitiesListQuery => Array.Empty<SpecialityDto>(),
                GetSubSpecialitiesListQuery => Array.Empty<SubSpecialityDto>(),
                ListActiveInvitationsQuery => _rows,
                IssueInvitationCommand => new IssuedInvitationResult(9, "token-issued"),
                ResendInvitationCommand resend => Resend(resend),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)answer!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        private IssuedInvitationResult Resend(ResendInvitationCommand command)
        {
            Resent.Add(command.InvitationId);
            _rows = AfterResend ?? _rows;
            if (Refusal is not null)
            {
                throw new InvalidOperationException(Refusal);
            }

            return new IssuedInvitationResult(command.InvitationId, "token-new");
        }
    }
}
