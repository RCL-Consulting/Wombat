using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Queries.ListUsers;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T061: minimal smoke test that the Users list renders rows + filters them client-side.
/// </summary>
public sealed class UsersListSmokeTests : TestContext
{
    [Fact]
    public void RendersUserRows_AndFiltersByName()
    {
        var users = new List<UserSummaryDto>
        {
            new("u-1", "smit@kgk", "Sam", "Smit", 1, "KGK", new[] { WombatRoles.Coordinator }, false),
            new("u-2", "naidoo@kgk", "Priya", "Naidoo", 1, "KGK", new[] { WombatRoles.Assessor }, false)
        };

        Services.AddSingleton<IScopedSender>(new FakeSender(users));
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@example.com");
        auth.SetRoles(WombatRoles.Administrator);

        var cut = RenderComponent<UsersList>();
        cut.WaitForState(() => cut.Markup.Contains("Sam"));

        cut.Markup.Should().Contain("Sam");
        cut.Markup.Should().Contain("Priya");

        var filterInput = cut.Find("#users-filter");
        filterInput.Input("Smit");

        cut.WaitForState(() => !cut.Markup.Contains("Priya"));
        cut.Markup.Should().Contain("Sam");
        cut.Markup.Should().NotContain("Priya");
    }

    private sealed class FakeSender : IScopedSender
    {
        private readonly IReadOnlyList<UserSummaryDto> _users;

        public FakeSender(IReadOnlyList<UserSummaryDto> users)
        {
            _users = users;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is ListUsersQuery)
            {
                return Task.FromResult((TResponse)(object)(IReadOnlyList<UserSummaryDto>)_users);
            }
            throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
