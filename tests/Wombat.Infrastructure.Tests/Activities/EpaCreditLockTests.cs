using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Trainees;
using Wombat.Infrastructure;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// T230's wiring, and T281's. The races themselves need two PostgreSQL connections and are driven in
/// <c>EpaCreditRacePostgresTests</c> and <c>ProgrammeEndCreditRacePostgresTests</c>; this is what those tests cannot see:
/// that the host hands the handlers the real locks, and that on any other provider a hold is harmless.
/// </summary>
public sealed class EpaCreditLockTests
{
    [Fact]
    public void TheHost_HandsTheEpaHandlersTheRealLock()
    {
        // Nothing in the Web suites composes AddInfrastructure (it needs a database), so an unregistered lock would first
        // show as a failure to build UpdateEpaCommandHandler on the EPA edit page. No connection is opened here.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=t230_never_opened"
            })
            .Build();

        var services = new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IEpaCreditLock>().Should().BeOfType<EpaCreditLock>();
        scope.ServiceProvider.GetRequiredService<IRequestHandler<UpdateEpaCommand, UpdateEpaResult>>()
            .Should().BeOfType<UpdateEpaCommandHandler>();
        scope.ServiceProvider.GetRequiredService<IRequestHandler<DeactivateEpaCommand>>()
            .Should().BeOfType<DeactivateEpaCommandHandler>();

        // T281: the trainee's lock, and the two handlers that record a programme's end with it.
        scope.ServiceProvider.GetRequiredService<ITraineeCreditLock>().Should().BeOfType<TraineeCreditLock>();
        scope.ServiceProvider.GetRequiredService<IRequestHandler<CompleteTraineeProfileCommand>>()
            .Should().BeOfType<CompleteTraineeProfileCommandHandler>();
        scope.ServiceProvider.GetRequiredService<IRequestHandler<DeactivateTraineeProfileCommand>>()
            .Should().BeOfType<DeactivateTraineeProfileCommandHandler>();
    }

    [Fact]
    public async Task OnAnotherProvider_AHoldOpensNothing_AndCommitsAndDisposesQuietly()
    {
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        var epaCreditLock = new EpaCreditLock(db);

        await using (var change = await epaCreditLock.HoldForChangeAsync(1, CancellationToken.None))
        {
            db.Database.CurrentTransaction.Should().BeNull();
            await change.CommitAsync(CancellationToken.None);
        }

        await using (var credit = await epaCreditLock.HoldForCreditAsync([1, 2], CancellationToken.None))
        {
            db.Database.CurrentTransaction.Should().BeNull();
            await credit.CommitAsync(CancellationToken.None);
        }

        var traineeCreditLock = new TraineeCreditLock(db);

        await using (var end = await traineeCreditLock.HoldForEndAsync(1, CancellationToken.None))
        {
            db.Database.CurrentTransaction.Should().BeNull();
            await end.CommitAsync(CancellationToken.None);
        }

        await using (var credit = await traineeCreditLock.HoldForCreditAsync(["trainee-1"], CancellationToken.None))
        {
            db.Database.CurrentTransaction.Should().BeNull();
            await credit.CommitAsync(CancellationToken.None);
        }
    }
}
