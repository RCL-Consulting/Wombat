using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Tests.Identity;

/// <summary>
/// T339, flow 02 (C2, E11): Identity's password errors carry Wombat's six rule sentences, and the administrator's reset
/// card, which shows a refused reset's words, lists the rules broken in the one order under the one heading.
/// </summary>
public sealed class PasswordRuleWordsTests
{
    /// <summary>The rules AddInfrastructure sets.</summary>
    private static readonly PasswordOptions Rules = new()
    {
        RequireDigit = true,
        RequireLowercase = true,
        RequireUppercase = true,
        RequireNonAlphanumeric = true,
        RequiredLength = 12,
        RequiredUniqueChars = 4
    };

    [Fact]
    public async Task IdentitysPasswordValidator_RefusesInWombatsWords_WithIdentitysCodes()
    {
        var validator = new PasswordValidator<WombatIdentityUser>(new WombatIdentityErrorDescriber());
        var users = UserManagerWith(Rules);

        var result = await validator.ValidateAsync(users, new WombatIdentityUser(), "abcdefgh");

        result.Errors.Select(error => (error.Code, error.Description)).Should().BeEquivalentTo(new[]
        {
            ("PasswordTooShort", "At least 12 characters."),
            ("PasswordRequiresNonAlphanumeric", "A symbol, such as ! or #."),
            ("PasswordRequiresDigit", "A digit (0 to 9)."),
            ("PasswordRequiresUpper", "An upper-case letter.")
        });
    }

    /// <summary>
    /// The words above reach a person only if the host's Identity uses the describer: AddInfrastructure registers it
    /// (<c>AddErrorDescriber</c>), and this holds it there (the review of the t339 branch). The UserManager an
    /// administrator's reset goes through is resolved as the app resolves it, and its password validator refuses a weak
    /// password in Wombat's words.
    /// </summary>
    [Fact]
    public async Task TheHostsIdentity_UsesWombatsDescriber_SoAWeakResetIsRefusedInWombatsWords()
    {
        // No connection is opened: nothing is read from the database.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=t339_never_opened"
            })
            .Build();

        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddInfrastructure(configuration)
            .BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        scope.ServiceProvider.GetRequiredService<IdentityErrorDescriber>().Should().BeOfType<WombatIdentityErrorDescriber>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        users.ErrorDescriber.Should().BeOfType<WombatIdentityErrorDescriber>();

        var errors = new List<IdentityError>();
        foreach (var validator in users.PasswordValidators)
        {
            errors.AddRange((await validator.ValidateAsync(users, new WombatIdentityUser(), "abcdefgh")).Errors);
        }

        errors.Select(error => error.Description).Should().BeEquivalentTo(
            "At least 12 characters.", "A symbol, such as ! or #.", "A digit (0 to 9).", "An upper-case letter.");
        UserAdministrationService.ResetRefusal(errors).Should().Be(
            "The password was not reset. The new password needs: At least 12 characters. A digit (0 to 9). " +
            "An upper-case letter. A symbol, such as ! or #.");
    }

    [Fact]
    public void ARefusedReset_SaysItWasNotReset_ThenTheRulesBroken_InTheOneOrder()
    {
        var describer = new WombatIdentityErrorDescriber();

        // In the order Identity's validator finds them, which is not the order the rules are listed in.
        var message = UserAdministrationService.ResetRefusal(
        [
            describer.PasswordTooShort(12),
            describer.PasswordRequiresNonAlphanumeric(),
            describer.PasswordRequiresDigit(),
            describer.PasswordRequiresUpper()
        ]);

        message.Should().Be(
            "The password was not reset. The new password needs: At least 12 characters. A digit (0 to 9). " +
            "An upper-case letter. A symbol, such as ! or #.");
    }

    [Fact]
    public void ARefusedResetForAnythingButARule_SaysItWasNotReset_AndWhy()
        => UserAdministrationService.ResetRefusal([new IdentityErrorDescriber().ConcurrencyFailure()])
            .Should().Be("The password was not reset. Optimistic concurrency failure, object has been modified.");

    private static UserManager<WombatIdentityUser> UserManagerWith(PasswordOptions rules)
    {
        var services = new ServiceCollection().BuildServiceProvider();
        return new UserManager<WombatIdentityUser>(
            new NoStore(),
            Options.Create(new IdentityOptions { Password = rules }),
            new PasswordHasher<WombatIdentityUser>(),
            [],
            [],
            new UpperInvariantLookupNormalizer(),
            new WombatIdentityErrorDescriber(),
            services,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<UserManager<WombatIdentityUser>>.Instance);
    }

    private sealed class NoStore : IUserStore<WombatIdentityUser>
    {
        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(WombatIdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);

        public Task<string?> GetUserNameAsync(WombatIdentityUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);

        public Task SetUserNameAsync(WombatIdentityUser user, string? userName, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<string?> GetNormalizedUserNameAsync(WombatIdentityUser user, CancellationToken cancellationToken)
            => Task.FromResult(user.NormalizedUserName);

        public Task SetNormalizedUserNameAsync(WombatIdentityUser user, string? normalizedName, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<IdentityResult> CreateAsync(WombatIdentityUser user, CancellationToken cancellationToken)
            => Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> UpdateAsync(WombatIdentityUser user, CancellationToken cancellationToken)
            => Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> DeleteAsync(WombatIdentityUser user, CancellationToken cancellationToken)
            => Task.FromResult(IdentityResult.Success);

        public Task<WombatIdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
            => Task.FromResult<WombatIdentityUser?>(null);

        public Task<WombatIdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
            => Task.FromResult<WombatIdentityUser?>(null);
    }
}
