using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Wombat.Common.Constants;
using Wombat.Data;

namespace Wombat.Web.Infrastructure.Identity
{
    
    public static class IdentitySeeder
    {
        private static readonly string[] Roles = {
                Role.Administrator.ToStringValue(),
                Role.Assessor.ToStringValue(),
                Role.Coordinator.ToStringValue(),
                Role.Trainee.ToStringValue() };

        // --- DEV: seed full demo set, idempotent ---
        public static async Task SeedDevAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = sp.GetRequiredService<UserManager<WombatUser>>();

            await EnsureRolesAsync(roleMgr);

            await EnsureUserAsync(userMgr, "admin@localhost.com", Role.Administrator.ToStringValue(), "System", "Admin", 1, null, "P@ssw0rd");
            await EnsureUserAsync(userMgr, "assessor@localhost.com", Role.Assessor.ToStringValue(), "System", "Assessor", 2, null, "P@ssw0rd");
            await EnsureUserAsync(userMgr, "trainee@localhost.com", Role.Trainee.ToStringValue(), "System", "Trainee", 2, 1, "P@ssw0rd");
            await EnsureUserAsync(userMgr, "coordinator@localhost.com", Role.Coordinator.ToStringValue(), "System", "Coordinator", 2, null, "P@ssw0rd");
        }

        // --- PROD: bootstrap once if DB is empty, idempotent ---
        public static async Task SeedProdBootstrapAsync(IServiceProvider services, IConfiguration config)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = sp.GetRequiredService<UserManager<WombatUser>>();

            var anyUsers = false;
            try
            {
                anyUsers = await userMgr.Users.AnyAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "42P01") // relation does not exist
            {
                // Tables missing — migrate right here just in case, then re-check
                var db = sp.GetRequiredService<ApplicationDbContext>();
                db.Database.Migrate();
                anyUsers = await userMgr.Users.AnyAsync();
            }

            if (anyUsers) return;

            await EnsureRolesAsync(roleMgr);

            var email = config["Seed:AdminEmail"] ?? "admin@wombat.local";
            var pass = config["Seed:AdminPassword"]
                        ?? throw new InvalidOperationException("Missing Seed:AdminPassword (env var or user-secrets).");

            await EnsureUserAsync(userMgr, email, Role.Administrator.ToStringValue(), "System", "Admin", null, null, pass);
        }

        // --- helpers ---
        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleMgr)
        {
            foreach (var r in Roles)
                if (!await roleMgr.RoleExistsAsync(r))
                    await roleMgr.CreateAsync(new IdentityRole(r));
        }

        private static async Task EnsureUserAsync(
            UserManager<WombatUser> userMgr,
            string email,
            string role,
            string firstName,
            string lastName,
            int? institutionId,
            int? subSpecialityId,
            string password)
        {
            var user = await userMgr.FindByEmailAsync(email);
            if (user == null)
            {
                user = new WombatUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    Name = firstName,
                    Surname = lastName,
                    InstitutionId = institutionId,
                    SubSpecialityId = subSpecialityId,
                    ApprovalStatus = WombatUser.eApprovalStatus.Approved
                };

                var create = await userMgr.CreateAsync(user, password);
                if (!create.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}")));
            }

            if (!string.IsNullOrWhiteSpace(role) && !await userMgr.IsInRoleAsync(user, role))
            {
                var add = await userMgr.AddToRoleAsync(user, role);
                if (!add.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", add.Errors.Select(e => $"{e.Code}:{e.Description}")));
            }
        }
    }
}
