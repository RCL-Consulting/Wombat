using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Users.Queries.GetUserById;
using Wombat.Application.Features.Users.Queries.ListUsers;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The Users pages offer someone who holds Trainee nothing, whatever role admits them to the pages, and say why: the list
/// lists nobody and the user page opens nobody, their own account included, and neither reads anything. On anyone's own
/// account the user page offers no role, lockout or password change, and says why. (T278)
/// </summary>
/// <remarks>
/// These run the real handlers against an in-memory store, as <c>PanelPagesTraineeFirstTests</c> does: the rule under test
/// is the handlers' (<c>UserAdministrationRules</c>), which the pages ask before they send anything. Each rule has a
/// control: the same caller without Trainee, or on someone else's account, is offered what the rule takes away.
/// </remarks>
public sealed class UsersPagesSelfAndTraineeTests : TestContext
{
    private const int InstitutionA = 1;
    private const string Self = "registrar-a";
    private const string Other = "colleague-a";

    private const string TraineeAdministersNoUser =
        "You hold the Trainee role, so you cannot view or change user accounts, including your own.";

    private const string OwnAccountNote =
        "This is your own account, so you cannot change its roles, lockout or password here. Another administrator can " +
        "change your roles or lockout.";

    // The subtitles say what each page offers, so they change with what it withholds (review of T278).
    private const string ListSubtitle =
        "Manage roles, passwords, and access for all users across the institutions you administer.";

    private const string UserSubtitle = "Roles, password reset, lockout, and pending-invitation cleanup for a single user.";
    private const string OwnAccountSubtitle = "Your own account's summary, roles and pending invitations.";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly TestAuthorizationContext _auth;
    private readonly HandlerSender _sender;

    public UsersPagesSelfAndTraineeTests()
    {
        _auth = this.AddTestAuthorization();
        _sender = new HandlerSender(CreateDb);
        Services.AddSingleton<IScopedSender>(_sender);
        Seed();
    }

    public static TheoryData<string> UserAdministrationRoles => new()
    {
        WombatRoles.Administrator,
        WombatRoles.InstitutionalAdmin
    };

    // ─── The users list ──────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void TheUsersList_ListsATraineeWhoAdministersUsersNobody_ReadsNothing_AndSaysWhy(string role)
    {
        SignInAs(role, holdsTrainee: true);

        var cut = RenderComponent<UsersList>();
        cut.WaitForState(() => cut.FindAll("#users-trainee-note").Count == 1);

        cut.Find("#users-trainee-note").TextContent.Trim().Should().Be(TraineeAdministersNoUser);
        cut.Find("#users-trainee-note").GetAttribute("role").Should().BeNull("standing page content is not announced on every load");
        cut.FindAll("table").Should().BeEmpty(role);
        cut.FindAll("#users-filter").Should().BeEmpty(role);
        cut.FindAll("a.btn").Select(link => link.TextContent.Trim()).Should().NotContain("Manage");
        cut.FindAll(".alert-danger").Should().BeEmpty("the page does not send what the handler would refuse");
        cut.FindAll(".page-subtitle").Should().BeEmpty("a subtitle promising roles, passwords and access would say what it withholds");
        _sender.Received.Should().BeEmpty("nothing about anyone is read for them");
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void TheControl_TheUsersList_ListsTheSameCallerWithoutTrainee_TheInstitutionsUsers(string role)
    {
        SignInAs(role, holdsTrainee: false);

        var cut = RenderComponent<UsersList>();
        cut.WaitForState(() => cut.FindAll("table").Count == 1);

        cut.FindAll("#users-trainee-note").Should().BeEmpty(role);
        cut.FindAll("a.btn").Where(link => link.TextContent.Trim() == "Manage").Should().HaveCount(2, role);
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be(ListSubtitle, role);
    }

    // ─── The user page ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(WombatRoles.Administrator, Other)]
    [InlineData(WombatRoles.Administrator, Self)]
    [InlineData(WombatRoles.InstitutionalAdmin, Other)]
    [InlineData(WombatRoles.InstitutionalAdmin, Self)]
    public void TheUserPage_OpensNobodyForATraineeWhoAdministersUsers_TheirOwnAccountIncluded_AndSaysWhy(string role, string userId)
    {
        // Before T278 their own account offered Remove on the Trainee role, and a colleague's offered Lock out.
        SignInAs(role, holdsTrainee: true);

        var cut = RenderComponent<UserDetail>(parameters => parameters.Add(page => page.UserId, userId));
        cut.WaitForState(() => cut.FindAll("#user-trainee-note").Count == 1);

        cut.Find("#user-trainee-note").TextContent.Trim().Should().Be(TraineeAdministersNoUser);
        cut.Find("#user-trainee-note").GetAttribute("role").Should().BeNull();
        cut.FindAll("button").Should().BeEmpty($"{role} on {userId}: nothing is offered");
        cut.FindAll(".state-panel-title").Should().BeEmpty("the reason is the Trainee role, and the note says so");
        cut.Find("h1").TextContent.Trim().Should().Be("User", "no name is read for them");
        cut.FindAll(".page-subtitle").Should().BeEmpty("a subtitle promising roles, password reset and lockout would say what it withholds");
        _sender.Received.Should().BeEmpty("the user is not read for them");
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void TheControl_TheUserPage_OffersTheSameCallerWithoutTrainee_EveryChange_OnSomeoneElse(string role)
    {
        SignInAs(role, holdsTrainee: false);

        var cut = RenderComponent<UserDetail>(parameters => parameters.Add(page => page.UserId, Other));
        cut.WaitForState(() => Buttons(cut).Contains("Lock out user"));

        cut.FindAll("#user-trainee-note, #user-own-account-note").Should().BeEmpty(role);
        Buttons(cut).Should().Contain(["Remove", "Add role", "Reset password", "Lock out user"], role);
        cut.FindAll("#add-role-select").Should().ContainSingle();
        cut.FindAll("#reset-password-input").Should().ContainSingle();
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be(UserSubtitle, role);
    }

    [Theory]
    [MemberData(nameof(UserAdministrationRoles))]
    public void TheirOwnAccount_OffersNoRole_Lockout_OrPasswordChange_AndSaysWhy(string role)
    {
        SignInAs(role, holdsTrainee: false);

        var cut = RenderComponent<UserDetail>(parameters => parameters.Add(page => page.UserId, Self));
        cut.WaitForState(() => cut.FindAll("#user-own-account-note").Count == 1);

        var note = cut.Find("#user-own-account-note");
        note.TextContent.Should().StartWith(OwnAccountNote);
        note.GetAttribute("role").Should().BeNull("standing page content is not announced on every load");
        note.QuerySelector("a")!.GetAttribute("href").Should().Be("/account/change-password", "a password is changed there");

        // Their roles are still shown, each without a Remove, and without the "System-managed" that means something else.
        cut.FindAll(".stack-list li span:first-child").Select(span => span.TextContent.Trim())
            .Should().Equal(WombatRoles.Coordinator, role);
        cut.Markup.Should().NotContain("System-managed");
        Buttons(cut).Should().NotContain(["Remove", "Add role", "Reset password", "Lock out user", "Reactivate user"], role);
        cut.FindAll("#add-role-select").Should().BeEmpty(role);
        cut.FindAll("#reset-password-input").Should().BeEmpty(role);
        cut.Markup.Should().NotContain("Global administrators cannot be locked out",
            "until T278 their own account's lockout card said so, whatever their role");

        // Revoking the invitations addressed to them changes nothing about the account, so it is still offered.
        Buttons(cut).Should().Contain("Revoke all pending invitations");

        // The subtitle names what is there, not the role, password and lockout changes it withholds.
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be(OwnAccountSubtitle, role);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static IReadOnlyList<string> Buttons(IRenderedFragment cut)
        => cut.FindAll("button").Select(button => button.TextContent.Trim()).ToArray();

    /// <summary>
    /// The registrar at A in <paramref name="role" />, with Trainee beside it when <paramref name="holdsTrainee" />. A global
    /// Administrator carries no institution.
    /// </summary>
    private void SignInAs(string role, bool holdsTrainee)
    {
        _auth.SetAuthorized("registrar@test");
        _auth.SetRoles(holdsTrainee ? [WombatRoles.Trainee, role] : [role]);

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, Self) };
        if (role != WombatRoles.Administrator)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, InstitutionA.ToString()));
        }

        _auth.SetClaims(claims.ToArray());

        // The registrar's own record carries the roles they are signed in with, so their own page lists them.
        _sender.Users.Add(new UserIdentityDetails(
            Self, $"{Self}@a.test", "Rene", "Registrar", InstitutionA, [], [],
            holdsTrainee ? [WombatRoles.Coordinator, WombatRoles.Trainee, role] : [WombatRoles.Coordinator, role]));
    }

    private void Seed()
    {
        using var db = CreateDb();
        db.Institutions.Add(new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Invitation>().Add(new Invitation
        {
            Email = $"{Self}@a.test",
            TokenHash = "t278",
            TargetRole = WombatRoles.Assessor,
            InstitutionId = InstitutionA,
            IssuedByUserId = "admin",
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14)
        });
        db.SaveChanges();

        _sender.Users.Add(new UserIdentityDetails(
            Other, "colleague@a.test", "Chris", "Chair", InstitutionA, [], [], [WombatRoles.CommitteeMember]));
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    /// <summary>
    /// Sends the pages' reads to the real handlers, each on a fresh context over the one store, as a circuit's scoped
    /// sender would, and records each request. No command is sent in these tests.
    /// </summary>
    private sealed class HandlerSender(Func<ApplicationDbContext> createDb) : IScopedSender
    {
        public UserStore Users { get; } = new();

        public List<object> Received { get; } = [];

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            await using var db = createDb();

            object? answer = request switch
            {
                ListUsersQuery query => await new ListUsersQueryHandler(Users, db).Handle(query, cancellationToken),
                GetUserByIdQuery query => await new GetUserByIdQueryHandler(Users, db).Handle(query, cancellationToken),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return (TResponse)answer!;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }

    /// <summary>A user store that answers the reads; every change throws, since no test here sends one.</summary>
    private sealed class UserStore : IUserAdministrationService
    {
        private readonly Dictionary<string, UserIdentityDetails> _users = new(StringComparer.Ordinal);

        public void Add(UserIdentityDetails user) => _users[user.UserId] = user;

        public Task<UserIdentityDetails?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.TryGetValue(userId, out var user) ? user : null);

        public Task<IReadOnlyList<UserIdentityDetails>> ListUsersInRoleAsync(string role, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_users.Values.Where(user => user.Roles.Contains(role)).ToArray());

        public Task<IReadOnlyList<UserIdentityDetails>> ListAllUsersAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<UserIdentityDetails>>(_users.Values.ToArray());

        public Task UpdateNamesAsync(string userId, string firstName, string lastName, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task UpdateScopeAsync(string userId, int institutionId, IReadOnlyCollection<int> specialityIds, IReadOnlyCollection<int> subSpecialityIds, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task PromotePendingTraineeAsync(string userId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task AddRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task RemoveRoleAsync(string userId, string role, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task ResetPasswordAsync(string userId, string newPassword, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task SetLockoutAsync(string userId, bool locked, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
