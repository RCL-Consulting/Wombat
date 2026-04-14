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

    private static RequestHandlerDelegate<T> Next<T>(T value)
        => () => Task.FromResult(value);

    private static RequestHandlerDelegate<T> FailingNext<T>()
        => () => throw new InvalidOperationException("Boom!");

    private sealed record TestCommand(string Payload) : IRequest<string>;
    private sealed record TestQuery : IRequest<string>;

    // Named without "Command" suffix but opts in via marker interface
    private sealed record ExplicitAuditedRequest : IRequest<string>, IAuditedCommand;
}
