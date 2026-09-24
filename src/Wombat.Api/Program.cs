using Microsoft.EntityFrameworkCore;
using Wombat.Api.Endpoints;
using Wombat.Application;
using Wombat.Infrastructure;
using Wombat.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMsfResponseRateLimiter();

// Every error answers with a problem-details body rather than an empty one. A fault is a 500 that names no exception;
// a refusal meant for a respondent is answered by the respond endpoint itself (MsfRespondEndpoint). (T202)
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseRateLimiter();

app.MapGet("/health", () => "ok");
app.MapMsfRespondEndpoint();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

public partial class Program;
