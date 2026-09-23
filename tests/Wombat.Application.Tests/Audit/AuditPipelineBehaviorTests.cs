using FluentAssertions;
using MediatR;
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

    private sealed record TestCommand(string Payload) : IRequest<string>;
    private sealed record TestQuery : IRequest<string>;

    // Named without "Command" suffix but opts in via marker interface
    private sealed record ExplicitAuditedRequest : IRequest<string>, IAuditedCommand;
}
