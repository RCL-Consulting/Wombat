using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Audit;
using Wombat.Domain.Audit;

namespace Wombat.Application.Tests.Audit;

public sealed class AuditPipelineBehaviorTests
{
    private readonly Mock<IAuditWriter> _writerMock = new();
    private readonly Mock<IAuditContextProvider> _contextMock = new();

    public AuditPipelineBehaviorTests()
    {
        _contextMock.Setup(c => c.UserId).Returns("user-1");
        _contextMock.Setup(c => c.UserDisplay).Returns("Test User");
        _contextMock.Setup(c => c.IpAddress).Returns("10.0.0.0/24");
        _contextMock.Setup(c => c.UserAgent).Returns("Test/1.0");
    }

    [Fact]
    public async Task Handle_Command_WritesSuccessAuditEntry()
    {
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object);
        var result = await behavior.Handle(new TestCommand("hello"), Next("ok"), CancellationToken.None);

        result.Should().Be("ok");
        capturedEntry.Should().NotBeNull();
        capturedEntry!.Action.Should().Be(nameof(TestCommand));
        capturedEntry.Success.Should().BeTrue();
        capturedEntry.ActorUserId.Should().Be("user-1");
        capturedEntry.Category.Should().Be(AuditCategory.Command);
    }

    [Fact]
    public async Task Handle_Query_SkipsAuditWrite()
    {
        var behavior = new AuditPipelineBehavior<TestQuery, string>(_writerMock.Object, _contextMock.Object);
        var result = await behavior.Handle(new TestQuery(), Next("query-result"), CancellationToken.None);

        result.Should().Be("query-result");
        _writerMock.Verify(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CommandThrows_WritesFailureEntryAndRethrows()
    {
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object);

        var act = async () => await behavior.Handle(
            new TestCommand("boom"),
            FailingNext<string>(),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        capturedEntry.Should().NotBeNull();
        capturedEntry!.Success.Should().BeFalse();
        capturedEntry.ErrorMessage.Should().Be("Boom!");

        // Not a refused save, so the ordinary write, which also commits whatever the handler left pending: the known
        // trap, kept on purpose (T201). Handlers avoid it by checking before they mutate.
        _writerMock.Verify(
            w => w.WriteDiscardingPendingChangesAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// T201. The handler's own save was refused, so its changes are still tracked on the context the writer shares.
    /// The row must be written without them, and the handler's exception, not a second one, must reach the caller.
    /// </summary>
    [Fact]
    public async Task Handle_HandlersOwnSaveRefused_WritesFailureEntryAloneAndRethrowsTheSameException()
    {
        var refused = new DbUpdateConcurrencyException("The campaign changed under this request.");
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteDiscardingPendingChangesAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object);

        var act = async () => await behavior.Handle(new TestCommand("save"), Throwing<string>(refused), CancellationToken.None);

        (await act.Should().ThrowAsync<DbUpdateConcurrencyException>()).Which.Should().BeSameAs(refused);

        capturedEntry.Should().NotBeNull();
        capturedEntry!.Success.Should().BeFalse();
        capturedEntry.ErrorMessage.Should().Be("The campaign changed under this request.");
        _writerMock.Verify(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// T201. Create/Update handlers translate a unique-index violation into an InvalidOperationException carrying the
    /// DbUpdateException. The refused insert is still tracked, so it is a refused save all the same.
    /// </summary>
    [Fact]
    public async Task Handle_HandlerTranslatedARefusedSave_WritesFailureEntryAloneAndRethrowsTheTranslation()
    {
        var translated = new InvalidOperationException(
            "An institution with the same name or short code already exists.",
            new DbUpdateException("duplicate key value violates unique constraint"));
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteDiscardingPendingChangesAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object);

        var act = async () => await behavior.Handle(new TestCommand("save"), Throwing<string>(translated), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(translated);

        capturedEntry.Should().NotBeNull();
        capturedEntry!.Success.Should().BeFalse();
        capturedEntry.ErrorMessage.Should().Be("An institution with the same name or short code already exists.");
        _writerMock.Verify(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// T201. ASP.NET Identity catches a concurrency conflict itself, and UserAdministrationService throws the failed
    /// result as a plain InvalidOperationException, so nothing says a save was refused. The refused user update is
    /// still tracked, and the ordinary write that re-sends it is refused in turn. The row must then be written alone,
    /// and the handler's exception, not the write's, must reach the caller.
    /// </summary>
    [Fact]
    public async Task Handle_OrdinaryFailureWriteRefused_WritesTheEntryAloneAndRethrowsTheHandlersException()
    {
        var handlers = new InvalidOperationException("Optimistic concurrency failure, object has been modified.");
        var writesRefusal = new DbUpdateConcurrencyException("The user changed under this request.");
        AuditEntry? attempted = null;
        AuditEntry? written = null;
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => attempted = e)
            .ThrowsAsync(writesRefusal);
        _writerMock
            .Setup(w => w.WriteDiscardingPendingChangesAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => written = e)
            .Returns(Task.CompletedTask);

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object);

        var act = async () => await behavior.Handle(new TestCommand("lock"), Throwing<string>(handlers), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(handlers);

        attempted.Should().NotBeNull("an exception that is not a refused save is written the ordinary way first");
        written.Should().BeSameAs(attempted, "the refused write's own entry is the one written alone");
        written!.Success.Should().BeFalse();
        written.ErrorMessage.Should().Be("Optimistic concurrency failure, object has been modified.");
    }

    /// <summary>
    /// T201. Only the handler's exception makes a failure row. When the success row's own write is refused the command's
    /// work has already committed, and a Success=false row would record a change that happened as one that did not.
    /// </summary>
    [Fact]
    public async Task Handle_SuccessWriteRefused_WritesNoFailureEntryAndSurfacesTheWritesException()
    {
        var writesRefusal = new DbUpdateException("value too long for type character varying(500)");
        var attempts = new List<AuditEntry>();
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => attempts.Add(e))
            .ThrowsAsync(writesRefusal);

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object);

        var act = async () => await behavior.Handle(new TestCommand("lock"), Next("ok"), CancellationToken.None);

        (await act.Should().ThrowAsync<DbUpdateException>()).Which.Should().BeSameAs(writesRefusal);

        attempts.Should().ContainSingle().Which.Success.Should().BeTrue();
        _writerMock.Verify(
            w => w.WriteDiscardingPendingChangesAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_AuditedCommand_WritesAuditEntry()
    {
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var behavior = new AuditPipelineBehavior<ExplicitAuditedRequest, string>(_writerMock.Object, _contextMock.Object);
        await behavior.Handle(new ExplicitAuditedRequest(), Next("ok"), CancellationToken.None);

        capturedEntry.Should().NotBeNull();
        capturedEntry!.Action.Should().Be(nameof(ExplicitAuditedRequest));
    }

    /// <summary>
    /// The stale-cookie case: /account/register/submit is anonymous, so the principal the pipeline
    /// sees is either empty or the previously registered user's. The handler resolves the real scope
    /// from the invitation and declares it, and the row must land there — which means the pipeline
    /// has to read the institution after the handler ran, not before. (T101)
    /// </summary>
    [Fact]
    public async Task Handle_HandlerDeclaresInstitution_StampsDeclaredScopeNotPrincipals()
    {
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var context = new DeclarableAuditContext(principalInstitutionId: 9);
        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, context);

        await behavior.Handle(new TestCommand("hello"), Declaring(context, 3, "ok"), CancellationToken.None);

        capturedEntry.Should().NotBeNull();
        capturedEntry!.InstitutionId.Should().Be(3);
    }

    [Fact]
    public async Task Handle_CommandThrowsAfterDeclaring_StampsDeclaredScope()
    {
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var context = new DeclarableAuditContext(principalInstitutionId: null);
        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, context);

        var act = async () => await behavior.Handle(
            new TestCommand("boom"),
            DeclaringThenFailing<string>(context, 3),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();

        capturedEntry.Should().NotBeNull();
        capturedEntry!.Success.Should().BeFalse();
        capturedEntry.InstitutionId.Should().Be(3);
    }

    [Fact]
    public async Task Handle_HandlerDeclaresNothing_StampsPrincipalsScope()
    {
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var context = new DeclarableAuditContext(principalInstitutionId: 9);
        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, context);

        await behavior.Handle(new TestCommand("hello"), Next("ok"), CancellationToken.None);

        capturedEntry.Should().NotBeNull();
        capturedEntry!.InstitutionId.Should().Be(9);
    }

    /// <summary>
    /// T205. An MSF respondent may be signed in to Wombat in the browser they answer from. Their submission's row must
    /// not name them: not their id, not their email, not their institution (which would put the row before that
    /// institution's admins), and not their user agent (which their own sign-in row carries beside their name). The
    /// address stays, truncated, as T101 kept it. A handler's declaration does not bring the actor back either.
    /// </summary>
    [Fact]
    public async Task Handle_AnonymousCommand_WritesASuccessRowThatNamesNobody()
    {
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var context = new DeclarableAuditContext(principalInstitutionId: 9);
        var behavior = new AuditPipelineBehavior<AnonymousTestCommand, string>(_writerMock.Object, context);

        await behavior.Handle(new AnonymousTestCommand(), Declaring(context, 3, "ok"), CancellationToken.None);

        capturedEntry.Should().NotBeNull();
        capturedEntry!.Success.Should().BeTrue();
        capturedEntry.Action.Should().Be(nameof(AnonymousTestCommand), "the command is still audited");
        ShouldNameNobody(capturedEntry);
    }

    [Fact]
    public async Task Handle_AnonymousCommandThrows_WritesAFailureRowThatNamesNobody()
    {
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var context = new DeclarableAuditContext(principalInstitutionId: 9);
        var behavior = new AuditPipelineBehavior<AnonymousTestCommand, string>(_writerMock.Object, context);

        var act = async () => await behavior.Handle(
            new AnonymousTestCommand(),
            DeclaringThenFailing<string>(context, 3),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        capturedEntry.Should().NotBeNull();
        capturedEntry!.Success.Should().BeFalse();
        ShouldNameNobody(capturedEntry);
    }

    /// <summary>T205: the refused-save path writes its row alone, and that row names nobody either.</summary>
    [Fact]
    public async Task Handle_AnonymousCommandsSaveRefused_WritesAFailureRowThatNamesNobody()
    {
        AuditEntry? capturedEntry = null;
        _writerMock
            .Setup(w => w.WriteDiscardingPendingChangesAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => capturedEntry = e)
            .Returns(Task.CompletedTask);

        var behavior = new AuditPipelineBehavior<AnonymousTestCommand, string>(_writerMock.Object, _contextMock.Object);
        _contextMock.Setup(c => c.InstitutionId).Returns(9);

        var act = async () => await behavior.Handle(
            new AnonymousTestCommand(),
            Throwing<string>(new DbUpdateException("duplicate key value violates unique constraint")),
            CancellationToken.None);

        await act.Should().ThrowAsync<DbUpdateException>();
        capturedEntry.Should().NotBeNull();
        ShouldNameNobody(capturedEntry!);
    }

    private static void ShouldNameNobody(AuditEntry entry)
    {
        entry.ActorUserId.Should().BeNull("the respondent's sign-in must not name them");
        entry.ActorDisplay.Should().BeNull();
        entry.ActorUserAgent.Should().BeNull("their user agent is on their own sign-in row, beside their name");
        entry.InstitutionId.Should().BeNull("an institution stamp would show the row to that institution's admins");
        entry.ActorIpAddress.Should().Be("10.0.0.0/24", "the truncated address is kept, as T101 kept it");
    }

    private static RequestHandlerDelegate<T> Next<T>(T value)
        => () => Task.FromResult(value);

    private static RequestHandlerDelegate<T> Declaring<T>(IAuditContextProvider context, int institutionId, T value)
        => () =>
        {
            context.DeclareInstitution(institutionId);
            return Task.FromResult(value);
        };

    private static RequestHandlerDelegate<T> DeclaringThenFailing<T>(IAuditContextProvider context, int institutionId)
        => () =>
        {
            context.DeclareInstitution(institutionId);
            throw new InvalidOperationException("Boom!");
        };

    /// <summary>
    /// Mirrors HttpAuditContextProvider: a declaration made during the dispatch overrides the
    /// principal-derived institution for the rest of it.
    /// </summary>
    private sealed class DeclarableAuditContext(int? principalInstitutionId) : IAuditContextProvider
    {
        private int? _declared;

        public string? UserId => "user-1";
        public string? UserDisplay => "Test User";
        public string? IpAddress => "10.0.0.0/24";
        public string? UserAgent => "Test/1.0";
        public int? InstitutionId => _declared ?? principalInstitutionId;
        public void DeclareInstitution(int institutionId) => _declared = institutionId;
        public void DeclareActor(string userId, string display) { }
    }

    private static RequestHandlerDelegate<T> FailingNext<T>()
        => () => throw new InvalidOperationException("Boom!");

    private static RequestHandlerDelegate<T> Throwing<T>(Exception exception)
        => () => throw exception;

    private sealed record TestCommand(string Payload) : IRequest<string>;
    private sealed record TestQuery : IRequest<string>;

    // Named without "Command" suffix but opts in via marker interface
    private sealed record ExplicitAuditedRequest : IRequest<string>, IAuditedCommand;

    private sealed record AnonymousTestCommand : IRequest<string>, IAnonymousAuditedCommand;
}
