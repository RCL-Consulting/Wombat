using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Identity;

public sealed class AdminSeeder
{
    private readonly UserManager<WombatIdentityUser> _userManager;
    private readonly IOptions<WombatOptions> _options;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(
        UserManager<WombatIdentityUser> userManager,
        IOptions<WombatOptions> options,
        ILogger<AdminSeeder> logger)
    {
        _userManager = userManager;
        _options = options;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var options = _options.Value;

        if (string.IsNullOrWhiteSpace(options.SeedAdminEmail) || string.IsNullOrWhiteSpace(options.SeedAdminPassword))
        {
            _logger.LogInformation("Administrator bootstrap skipped because Wombat:SeedAdminEmail or Wombat:SeedAdminPassword is not configured.");
            return;
        }

        var existingUser = await _userManager.FindByEmailAsync(options.SeedAdminEmail);
        if (existingUser != null)
        {
            return;
        }

        var adminUser = new WombatIdentityUser
        {
            UserName = options.SeedAdminEmail,
            Email = options.SeedAdminEmail,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(adminUser, options.SeedAdminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to create seed administrator: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(adminUser, WombatRoles.Administrator);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to assign seed administrator role: {errors}");
        }

        _logger.LogInformation("Seeded administrator user {Email}.", adminUser.Email);
    }
}
