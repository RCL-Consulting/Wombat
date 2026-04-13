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

var app = builder.Build();

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
