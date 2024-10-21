using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Wombat.Application.Configurations;
using Wombat.Application.Contracts;
using Wombat.Data;
using Wombat.Application.Repositories;
using Wombat.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<WombatUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IEmailSender>(s => new EmailSender("localhost", 25, "no-reply@rcl.co.za"));

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
builder.Services.AddAutoMapper(typeof(MapperConfig));

builder.Host.UseSerilog((ctx, lc) =>
    lc.WriteTo.Console()
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllersWithViews();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
