using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wombat.Application.Audit;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Reporting;
using Wombat.Application.Scheduling;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Email;
using Wombat.Infrastructure.MultiSourceFeedback;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Reporting;
using Wombat.Infrastructure.Scheduling;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Scheduling.Jobs;

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
        services.Configure<DashboardThresholds>(configuration.GetSection(DashboardThresholds.SectionName));
        services.Configure<SsoOptions>(configuration.GetSection(SsoOptions.SectionName));

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddScoped<IAuditContextProvider, HttpAuditContextProvider>();

        services.AddIdentity<WombatIdentityUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 12;
            options.Password.RequiredUniqueChars = 4;
            options.SignIn.RequireConfirmedAccount = false;

            // Per-account brute-force protection. The login endpoint passes
            // lockoutOnFailure: true, so these bounds are what actually apply.
            // Enabled for every new user, including the seeded administrator.
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddClaimsPrincipalFactory<WombatUserClaimsPrincipalFactory>()
        .AddDefaultTokenProviders();

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
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddSingleton<EmailQueue>();
        services.AddTransient<ISmtpSender, MailKitEmailSender>();
        var smtpHost = configuration[$"{EmailSettings.SectionName}:SmtpHost"];
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            services.AddScoped<IEmailSender, LoggingEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, QueuedEmailSender>();
            services.AddHostedService<EmailWorker>();
        }

        // What the mail worker reports about a mail that asks (T251): an MSF link's is recorded onto its invitation.
        services.AddScoped<IEmailDeliveryObserver, MsfLinkDeliveryRecorder>();
        services.AddScoped<IInvitedUserProvisioner, InvitedUserProvisioner>();
        services.AddScoped<SsoGroupMapper>();
        services.AddScoped<ExternalLoginHandler>();
        services.AddScoped<IUserAdministrationService, UserAdministrationService>();
        // The clock the activity write path judges the encounter date's "today" by (T160). TryAdd: a host that already
        // registers one keeps it.
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IActivityReferenceDataService, ActivityReferenceDataService>();
        services.AddScoped<ISchemaValidator, SchemaValidator>();
        services.AddScoped<IWorkflowEvaluator, WorkflowEvaluator>();
        services.AddScoped<IFieldPermissionEvaluator, FieldPermissionEvaluator>();
        services.AddScoped<ICreditApplier, CreditApplier>();
        // T230: a completion's credit and its EPA's deactivation or reactivation are serialised on the EPA's row.
        services.AddScoped<IEpaCreditLock, EpaCreditLock>();

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        services.AddScoped<IPortfolioPdfService, PortfolioPdfService>();
        services.AddScoped<IEntrustmentCertificatePdfService, EntrustmentCertificatePdfService>();
        services.AddScoped<IErasureExecutor, ErasureExecutor>();
        services.AddScoped<IAccessReportBuilder, AccessReportBuilder>();
        services.AddScoped<IObjectionFlagUpdater, ObjectionFlagService>();
        services.AddScoped<IObjectionFlagReader, ObjectionFlagService>();

        services.AddScoped<RoleSeeder>();
        services.AddScoped<AdminSeeder>();
        services.AddScoped<DataSeeder>();
        services.AddScoped<PaediatricCatalogueSeeder>();
        services.AddScoped<ActivityTypeSeedRefresher>();
        services.AddScoped<CurriculumProgressBootstrapper>();
        services.AddScoped<DevUserSeeder>();

        services.AddScheduledJob<ActivityDraftNudgeJob>();
        services.AddScheduledJob<AssessorPendingNudgeJob>();
        services.AddScheduledJob<MsfCampaignAutoCloseJob>();
        services.AddScheduledJob<MsfInvitationExpiryReminderJob>();
        services.AddScheduledJob<WeeklyCoordinatorDigestJob>();
        services.AddScheduledJob<PortfolioExportCleanupJob>();
        services.AddScheduledJob<AuditLogRetentionJob>();
        services.AddScheduledJob<ScheduledJobRunRetentionJob>();
        services.AddScheduledJob<EntrustmentDecisionExpiryJob>();

        services.AddSingleton<IScheduledJobRegistry>(provider =>
        {
            var registry = new ScheduledJobRegistry();
            foreach (var job in provider.GetServices<IScheduledJob>())
            {
                registry.Register(job);
            }
            return registry;
        });
        services.AddSingleton<ScheduledJobLocks>();
        services.AddSingleton<IScheduledJobDispatcher, ScheduledJobDispatcher>();
        services.AddHostedService<ScheduledJobHost>();

        return services;
    }
}
