using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Audit;
using Wombat.Application.Common.Options;
using Wombat.Domain.Audit;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T155 on real PostgreSQL, with the real <see cref="AuditWriter" />: an SSO sign-in writes only an email its provider
/// verified, never onto another account's address, and a refused write is not committed by any later save in the request.
/// </summary>
/// <remarks>
/// <para>
/// The handler's cases are tested over the in-memory store (<c>SsoLinkAndSignInTests</c>). This is the one place the
/// real audit writer runs with it: that writer saves the request's context, so whatever a refused Identity update left
/// tracked is committed by the refusal's own audit row unless the handler put it back first. Every account is read in a
/// scope of its own, from the database.
/// </para>
/// <para>
/// Isolated the way <c>SsoErasurePostgresTests</c> is: a migrated schema of its own, dropped in a finally and on dispose.
/// </para>
/// </remarks>
public sealed class SsoEmailSyncPostgresTests : IAsyncLifetime
{
    private const string ProviderKey = "kgk";

    private readonly TestSchemas _schemas = new();

    /// <summary>The provider's institution, created in each test's schema before the provider's options are read.</summary>
    private int _institutionId;

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task AVerifiedEmailNamingAnotherAccount_LeavesBothAccountsAsTheyWere()
    {
        await WithSchemaAsync(async root =>
        {
            var naidoo = await CreateLinkedAsync(root, "naidoo@kgk.test", "idp-subject-1");
            var venter = await CreateLinkedAsync(root, "venter@kgk.test", "idp-subject-2");

            var result = await SignInAsync(root, "venter@kgk.test", "idp-subject-1");

            result.Succeeded.Should().BeTrue(result.ErrorMessage);
            var storedNaidoo = await StoredAsync(root, naidoo.Id);
            storedNaidoo.Email.Should().Be("naidoo@kgk.test");
            storedNaidoo.UserName.Should().Be("naidoo@kgk.test");
            storedNaidoo.NormalizedEmail.Should().Be("NAIDOO@KGK.TEST");
            storedNaidoo.NormalizedUserName.Should().Be("NAIDOO@KGK.TEST");
            storedNaidoo.SecurityStamp.Should().Be(naidoo.SecurityStamp);

            var storedVenter = await StoredAsync(root, venter.Id);
            storedVenter.Email.Should().Be("venter@kgk.test");
            storedVenter.NormalizedUserName.Should().Be("VENTER@KGK.TEST");
            storedVenter.ConcurrencyStamp.Should().Be(venter.ConcurrencyStamp);

            (await AuditActionsAsync(root)).Should().Contain(["SsoEmailSyncRefused", "SsoLogin"]);
        });
    }

    [Fact]
    public async Task AVerifiedEmailIdentityRefuses_IsNotCommittedByTheRefusalsOwnAuditRow()
    {
        // An apostrophe is a valid address but not a valid user name, so Identity refuses the write after it has set the
        // values on the tracked account. The audit writer's save comes next.
        await WithSchemaAsync(async root =>
        {
            var naidoo = await CreateLinkedAsync(root, "naidoo@kgk.test", "idp-subject-1");

            var result = await SignInAsync(root, "o'naidoo@kgk.test", "idp-subject-1");

            result.Succeeded.Should().BeTrue(result.ErrorMessage);
            var stored = await StoredAsync(root, naidoo.Id);
            stored.Email.Should().Be("naidoo@kgk.test");
            stored.UserName.Should().Be("naidoo@kgk.test");
            stored.NormalizedEmail.Should().Be("NAIDOO@KGK.TEST");
            stored.SecurityStamp.Should().Be(naidoo.SecurityStamp);
            (await AuditActionsAsync(root)).Should().Contain("SsoEmailSyncRefused", "the audit row was saved, and carried nothing else");
        });
    }

    [Fact]
    public async Task AVerifiedEmailNoOneHolds_IsWritten()
    {
        await WithSchemaAsync(async root =>
        {
            var naidoo = await CreateLinkedAsync(root, "naidoo@kgk.test", "idp-subject-1");

            var result = await SignInAsync(root, "n.naidoo@kgk.test", "idp-subject-1");

            result.Succeeded.Should().BeTrue(result.ErrorMessage);
            var stored = await StoredAsync(root, naidoo.Id);
            stored.Email.Should().Be("n.naidoo@kgk.test");
            stored.UserName.Should().Be("n.naidoo@kgk.test");
            stored.NormalizedEmail.Should().Be("N.NAIDOO@KGK.TEST");
            stored.NormalizedUserName.Should().Be("N.NAIDOO@KGK.TEST");
            (await AuditActionsAsync(root)).Should().Contain("SsoEmailChanged");
        });
    }

    // ---- helpers --------------------------------------------------------------------------------------------------

    private async Task WithSchemaAsync(Func<ServiceProvider, Task> test)
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
            services.AddIdentity<WombatIdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddHttpContextAccessor();
            services.AddSingleton<IAuthenticationService, NullAuthenticationService>();
            services.AddScoped<IAuditWriter, AuditWriter>();
            services.Configure<SsoOptions>(options => options.Providers.Add(new SsoProviderOptions
            {
                Key = ProviderKey,
                DisplayName = "KGK sign-in",
                InstitutionId = _institutionId,
                GroupsClaim = "groups"
            }));
            services.AddScoped<SsoGroupMapper>();
            services.AddScoped<ExternalLoginHandler>();

            await using var root = services.BuildServiceProvider();

            await using (var migrationScope = root.CreateAsyncScope())
            {
                var db = migrationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.MigrateAsync();

                var institution = new Institution { Name = "Kgosi Kgari", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow };
                db.Institutions.Add(institution);
                await db.SaveChangesAsync();
                _institutionId = institution.Id;
            }

            await test(root);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private async Task<WombatIdentityUser> CreateLinkedAsync(ServiceProvider root, string email, string subject)
    {
        await using var scope = root.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var user = new WombatIdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = email,
            InstitutionId = _institutionId
        };
        (await users.CreateAsync(user)).Succeeded.Should().BeTrue();
        (await users.AddLoginAsync(user, new UserLoginInfo(ProviderKey, subject, "KGK sign-in"))).Succeeded.Should().BeTrue();
        return user;
    }

    /// <summary>A sign-in through the provider in a request scope of its own, asserting a verified email.</summary>
    private static async Task<ExternalLoginHandler.ExternalLoginResult> SignInAsync(ServiceProvider root, string email, string subject)
    {
        await using var scope = root.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext { RequestServices = scope.ServiceProvider };

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(ClaimTypes.NameIdentifier, subject),
            new("email_verified", "true")
        };
        var login = new ExternalLoginInfo(new ClaimsPrincipal(new ClaimsIdentity(claims, ProviderKey)), ProviderKey, subject, "KGK sign-in");

        return await scope.ServiceProvider.GetRequiredService<ExternalLoginHandler>().HandleCallbackAsync(login, null, null);
    }

    private static async Task<WombatIdentityUser> StoredAsync(ServiceProvider root, string userId)
    {
        await using var scope = root.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Users.AsNoTracking().SingleAsync(user => user.Id == userId);
    }

    private static async Task<List<string>> AuditActionsAsync(ServiceProvider root)
    {
        await using var scope = root.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()
            .Set<AuditEntry>().AsNoTracking().Select(entry => entry.Action).ToListAsync();
    }

    private sealed class NullAuthenticationService : IAuthenticationService
    {
        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
    }
}
