using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Trainees;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T209 review: the trainees list says how, and on which day, each closed profile ended. A withdrawal's day is recorded
/// since T209; a profile deactivated before that has none, and says only "Withdrawn".
/// </summary>
public sealed class TraineesListOutcomeTests : TestContext
{
    public TraineesListOutcomeTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("instadmin@test");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "instadmin-1"));
    }

    [Fact]
    public void AClosedProfile_SaysHowItEnded_AndOnWhichDay()
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(
            Profile(1, "Thandi", completedOn: new DateOnly(2026, 6, 30), deactivatedOn: null),
            Profile(2, "Sipho", completedOn: null, deactivatedOn: new DateOnly(2026, 8, 20)),
            Profile(3, "Anna", completedOn: null, deactivatedOn: null)));

        var cut = RenderComponent<PendingTraineesList>();
        cut.WaitForState(() => cut.FindAll("caption").Any(caption => Text(caption) == "Completed and closed trainee profiles"));

        var table = cut.FindAll("table").Single(table => Text(table.QuerySelector("caption")!) == "Completed and closed trainee profiles");
        table.QuerySelectorAll("tbody tr")
            .Select(row => (Text(row.Children[0]), Text(row.Children[3])))
            .Should().Equal(
                ("Thandi Molefe", "Completed 2026-06-30"),
                ("Sipho Molefe", "Withdrawn 2026-08-20"),
                ("Anna Molefe", "Withdrawn"));
    }

    private static TraineeProfileDto Profile(int id, string firstName, DateOnly? completedOn, DateOnly? deactivatedOn) => new(
        id, $"trainee-{id}", $"trainee{id}@wombat.local", firstName, "Molefe",
        1, "CPSA Paediatrics", "11.1", 1, "Paediatrics", 1, "General Paediatrics",
        new DateOnly(2025, 1, 1), new DateOnly(2029, 1, 1),
        IsActive: false,
        CompletedOn: completedOn,
        DeactivatedOn: deactivatedOn);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender(params TraineeProfileDto[] profiles) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                GetInstitutionsListQuery => Array.Empty<InstitutionDto>(),
                ListPendingTraineesQuery => Array.Empty<PendingTraineeDto>(),
                ListTraineesForSpecialityQuery => profiles,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
