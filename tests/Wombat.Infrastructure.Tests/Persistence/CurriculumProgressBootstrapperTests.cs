using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Audit;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// T130: the startup rebuild that refills curriculum progress after the migration empties it.
/// </summary>
/// <remarks>
/// <para>
/// The T130 migration deletes every progress row on every existing database, because a lifetime tally cannot
/// be split into semesters. Without this component every trainee would read zero, while every completion's
/// <c>CreditedItemCount</c> still said it had credited, until someone found the rebuild button.
/// </para>
/// <para>
/// It runs inside startup, so its failure modes matter as much as its success: it must fire exactly when the
/// table is empty AND something has credited, never otherwise, never take the app down, and still let a
/// shutdown's cancellation through. The rebuild itself is tested in
/// <c>RebuildCurriculumProgressTests</c>; here it is a recorded request.
/// </para>
/// </remarks>
public sealed class CurriculumProgressBootstrapperTests
{
    [Fact]
    public async Task AnEmptyProgressTable_WithACreditedCompletion_IsRebuiltOnce_ForEveryone_AsAnAdministrator()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTransitions.Add(Transition(creditedItemCount: 2));
        await dbContext.SaveChangesAsync();

        var expected = new RebuildCurriculumProgressResult(
            ActivitiesReplayed: 3,
            CreditApplications: 4,
            ProgressRowsWritten: 2,
            ProgressRowsRemoved: 0,
            TransitionsStamped: 3,
            ActivitiesCredited: 3);
        var sender = new RecordingSender { Result = expected };
        using var shutdown = new CancellationTokenSource();

        var result = await CreateBootstrapper(dbContext, sender).RunAsync(shutdown.Token);

        var command = sender.Requests.Should().ContainSingle().Which
            .Should().BeOfType<RebuildCurriculumProgressCommand>().Subject;

        // Null means every trainee. The migration emptied the whole table, so a per-trainee rebuild would
        // leave everybody else on zero.
        command.TraineeUserId.Should().BeNull();

        // The rebuild refuses anyone who is not a global Administrator, and it checks with the BCL's
        // IsInRole, which reads the identity's RoleClaimType. A principal built with a custom role claim type
        // would be refused on every boot, logged, and swallowed, so it is asserted the way the handler reads it.
        command.Principal.IsInRole(WombatRoles.Administrator).Should().BeTrue();
        command.Principal.Identity!.IsAuthenticated.Should().BeTrue();
        command.Principal.Identity.Name.Should().Be(CurriculumProgressBootstrapper.SystemActorName);

        sender.Tokens.Should().Equal(shutdown.Token);
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task TheStartupRebuildIsAuditedUnderItsSystemName_NotAsNobody()
    {
        // There is no HTTP request at startup, so without a declared actor the audit row for a rewrite of every
        // trainee's progress would name nobody, indistinguishable from an anonymous command. The review of T130
        // found exactly that; the declaration is what the audit pipeline reads.
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTransitions.Add(Transition(creditedItemCount: 1));
        await dbContext.SaveChangesAsync();
        var auditContext = new RecordingAuditContext();

        await CreateBootstrapper(dbContext, new RecordingSender(), auditContext: auditContext).RunAsync();

        auditContext.UserId.Should().Be(CurriculumProgressBootstrapper.SystemActorName);
        auditContext.UserDisplay.Should().Be(CurriculumProgressBootstrapper.SystemActorName);
    }

    /// <summary>
    /// The normal state of every running system. A rebuild here would re-score every trainee on every boot.
    /// </summary>
    [Fact]
    public async Task AProgressTableThatHoldsARow_IsLeftAlone()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTransitions.Add(Transition(creditedItemCount: 1));
        dbContext.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            CurriculumItemId = 4000,
            TraineeUserId = "trainee-1",
            AcademicYear = 2026,
            Semester = 1,
            CountsSoFar = 1,
            LastObservedOn = new DateOnly(2026, 3, 10),
            LastUpdated = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();
        var sender = new RecordingSender();

        var result = await CreateBootstrapper(dbContext, sender).RunAsync();

        result.Should().BeNull();
        sender.Requests.Should().BeEmpty();
    }

    /// <summary>
    /// A fresh install, or a system where nothing has ever credited: an empty table is the right answer.
    /// <c>CreditedItemCount</c> is three-valued (T108), and neither null (never evaluated) nor 0 (evaluated,
    /// matched nothing) means a row is missing.
    /// </summary>
    [Fact]
    public async Task AnEmptyProgressTable_WithNoCreditedCompletion_IsLeftAlone()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTransitions.AddRange(
            Transition(creditedItemCount: null),
            Transition(creditedItemCount: 0));
        await dbContext.SaveChangesAsync();
        var sender = new RecordingSender();

        var result = await CreateBootstrapper(dbContext, sender).RunAsync();

        result.Should().BeNull();
        sender.Requests.Should().BeEmpty();
    }

    /// <summary>
    /// The kill switch (<c>Wombat__RebuildEmptyCurriculumProgress=false</c>). Off means no rebuild, but the
    /// empty table must still be announced, or the switch would turn a visible gap into a silent one.
    /// </summary>
    [Fact]
    public async Task TheKillSwitch_StopsTheRebuild_ButSaysOneWasDue()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTransitions.Add(Transition(creditedItemCount: 1));
        await dbContext.SaveChangesAsync();
        var sender = new RecordingSender();
        var logger = new CapturingLogger<CurriculumProgressBootstrapper>();

        var result = await CreateBootstrapper(dbContext, sender, rebuildEnabled: false, logger).RunAsync();

        result.Should().BeNull();
        sender.Requests.Should().BeEmpty();
        logger.Warnings.Should().ContainSingle().Which.Values["Count"].Should().Be(1);
    }

    /// <summary>
    /// A startup component that throws takes the whole app down. The rebuild rolls itself back on failure,
    /// and the manual rebuild page still works, so the right response is to log and carry on.
    /// </summary>
    [Fact]
    public async Task ARebuildThatFails_IsLogged_AndDoesNotFailStartup()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTransitions.Add(Transition(creditedItemCount: 1));
        await dbContext.SaveChangesAsync();
        var failure = new InvalidOperationException("The replay could not complete.");
        var sender = new RecordingSender { Failure = failure };
        var logger = new CapturingLogger<CurriculumProgressBootstrapper>();

        var run = () => CreateBootstrapper(dbContext, sender, rebuildEnabled: true, logger).RunAsync();

        var result = await run.Should().NotThrowAsync();
        result.Subject.Should().BeNull();
        sender.Requests.Should().ContainSingle();
        logger.Errors.Should().ContainSingle().Which.Exception.Should().BeSameAs(failure);
    }

    /// <summary>
    /// The one exception it must not swallow. A shutdown during startup cancels the rebuild, and turning that
    /// into "rebuild failed, carry on" would let the host continue booting into a process that is stopping.
    /// </summary>
    [Fact]
    public async Task ACancelledRebuild_PropagatesTheCancellation()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ActivityTransitions.Add(Transition(creditedItemCount: 1));
        await dbContext.SaveChangesAsync();
        var sender = new RecordingSender { Failure = new OperationCanceledException() };

        var run = () => CreateBootstrapper(dbContext, sender).RunAsync();

        await run.Should().ThrowAsync<OperationCanceledException>();
        sender.Requests.Should().ContainSingle();
    }

    private static CurriculumProgressBootstrapper CreateBootstrapper(
        ApplicationDbContext dbContext,
        ISender sender,
        bool rebuildEnabled = true,
        CapturingLogger<CurriculumProgressBootstrapper>? logger = null,
        RecordingAuditContext? auditContext = null)
        => new(
            dbContext,
            sender,
            auditContext ?? new RecordingAuditContext(),
            Options.Create(new WombatOptions { RebuildEmptyCurriculumProgress = rebuildEnabled }),
            logger ?? new CapturingLogger<CurriculumProgressBootstrapper>());

    /// <summary>Records what the bootstrapper declares to the audit context; there is no HTTP request at startup.</summary>
    private sealed class RecordingAuditContext : IAuditContextProvider
    {
        public string? UserId { get; private set; }
        public string? UserDisplay { get; private set; }
        public string? IpAddress => null;
        public string? UserAgent => null;
        public int? InstitutionId => null;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) => (UserId, UserDisplay) = (userId, display);
    }

    /// <summary>
    /// A completion's transition. The bootstrapper reads nothing but <c>CreditedItemCount</c>, and the in-memory
    /// provider enforces no foreign key, so no activity row is needed behind it.
    /// </summary>
    private static ActivityTransition Transition(int? creditedItemCount) => new()
    {
        ActivityId = 1,
        FromState = "submitted",
        ToState = "completed",
        TransitionKey = "complete",
        ActorUserId = "assessor-1",
        OccurredOn = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc),
        CreditedItemCount = creditedItemCount
    };

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>An <see cref="ISender" /> that records what it was asked to send and answers with a canned result.</summary>
    private sealed class RecordingSender : ISender
    {
        public List<object> Requests { get; } = [];

        public List<CancellationToken> Tokens { get; } = [];

        public RebuildCurriculumProgressResult Result { get; init; } = new(0, 0, 0, 0, 0);

        public Exception? Failure { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            Tokens.Add(cancellationToken);

            // A faulted task rather than a synchronous throw: that is how a real async handler fails.
            return Failure is not null
                ? Task.FromException<TResponse>(Failure)
                : Task.FromResult((TResponse)(object)Result);
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException();

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
