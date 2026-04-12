using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Email;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found. Configure ConnectionStrings:DefaultConnection before starting the app.");
        }

        services.Configure<WombatOptions>(configuration.GetSection(WombatOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentity<WombatIdentityUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 12;
            options.Password.RequiredUniqueChars = 4;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddClaimsPrincipalFactory<WombatUserClaimsPrincipalFactory>()
        .AddDefaultTokenProviders();

        services.AddAuthentication()
            .AddIdentityCookies();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.LoginPath = "/account/login";
            options.AccessDeniedPath = "/access-denied";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });

        services.AddWombatAuthorization();
        services.AddScoped<IEmailSender, LoggingEmailSender>();
        services.AddScoped<IInvitedUserProvisioner, InvitedUserProvisioner>();
        services.AddScoped<IUserAdministrationService, UserAdministrationService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<ISchemaValidator, SchemaValidator>();
        services.AddScoped<IWorkflowEvaluator, WorkflowEvaluator>();
        services.AddScoped<ICreditApplier, CreditApplier>();

        services.AddScoped<RoleSeeder>();
        services.AddScoped<AdminSeeder>();
        services.AddScoped<DataSeeder>();

        return services;
    }
}
