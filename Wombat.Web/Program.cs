/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Wombat.Application.Configurations;
using Wombat.Application.Contracts;
using Wombat.Application.Repositories;
using Wombat.Application.Services;
using Wombat.Common.Constants;
using Wombat.Data;
using Wombat.Services;
using Wombat.Web.Services;
using static Wombat.Data.WombatUser;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<WombatUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddHttpContextAccessor();

//builder.Services.AddTransient<IEmailSender>(s => new EmailSender("localhost", 25, "no-reply@rcl.co.za"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddHostedService<dbMigrator>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAssessmentFormRepository, AssessmentFormRepository>();
builder.Services.AddScoped<IEPARepository, EPARepository>();
builder.Services.AddScoped<IEnumCriteriaRepository, OptionCriteriaRepository>();
builder.Services.AddScoped<IOptionSetRepository, OptionSetRepository>();
builder.Services.AddScoped<ILoggedAssessmentRepository, LoggedAssessmentRepository>();
builder.Services.AddScoped<IOptionCriterionResponseRepository, OptionCriterionResponseRepository>();
builder.Services.AddScoped<ISpecialityRepository, SpecialityRepository>();
builder.Services.AddScoped<ISubSpecialityRepository, SubSpecialityRepository>();
builder.Services.AddScoped<IInstitutionRepository, InstitutionRepository>();
builder.Services.AddScoped<IAssessmentRequestRepository, AssessmentRequestRepository>();
builder.Services.AddScoped<IRegistrationInvitationRepository, RegistrationInvitationRepository>();
builder.Services.AddScoped<ISTARApplicationFormRepository, STARApplicationFormRepository>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<WombatUser>, CustomUserClaimsPrincipalFactory>();

builder.Services.AddAutoMapper(typeof(MapperConfig));

builder.Host.UseSerilog((ctx, lc) =>
    lc.WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Claims.ManageSpecialities, policy =>
        policy.RequireClaim(Claims.ManageSpecialities, "true"));

    options.AddPolicy(Claims.ManageSubspecialities, policy =>
        policy.RequireClaim(Claims.ManageSubspecialities, "true"));

    options.AddPolicy(Claims.ManageUsers, policy =>
        policy.RequireClaim(Claims.ManageUsers, "true"));

    options.AddPolicy(Claims.ManageRegistrationInvitations, policy =>
        policy.RequireClaim(Claims.ManageRegistrationInvitations, "true"));

    options.AddPolicy(Claims.ManageAssessmentForms, policy =>
        policy.RequireClaim(Claims.ManageAssessmentForms, "true"));

    options.AddPolicy(Claims.ManageOptionSets, policy =>
        policy.RequireClaim(Claims.ManageOptionSets, "true"));

    options.AddPolicy(Claims.ManageEPAs, policy =>
        policy.RequireClaim(Claims.ManageEPAs, "true"));

    // Add more as needed...
});

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<WombatUser>>();
}

app.Run();
