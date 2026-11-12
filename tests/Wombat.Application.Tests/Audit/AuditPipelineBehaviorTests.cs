using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Audit;
using Wombat.Application.Features.MultiSourceFeedback;
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
        // EF Core's own exception is no refusal (T400, K10, P1): the row stores the fixed words, the log the text.
        capturedEntry.ErrorMessage.Should().Be(AuditFailureText.ActionFailed);
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

    /// <summary>
    /// T400 (flow 18, K10, P1): a validator's refusal is stored as its messages, each once, in order, never as
    /// FluentValidation's "Validation failed: -- Property: … Severity: Error" text written for a log.
    /// </summary>
    [Fact]
    public async Task Handle_ValidationRefusal_StoresTheValidatorsMessagesOnly()
    {
        var refusal = new FluentValidation.ValidationException(
        [
            new FluentValidation.Results.ValidationFailure("Note", "Decline requires a note."),
            new FluentValidation.Results.ValidationFailure("Rationale", "Type the rationale."),
            new FluentValidation.Results.ValidationFailure("Note", "Decline requires a note."),
        ]);
        var logger = new RecordingLogger();
        var written = CaptureWrite();

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object, logger);
        var act = async () => await behavior.Handle(new TestCommand("x"), Throwing<string>(refusal), CancellationToken.None);

        (await act.Should().ThrowAsync<FluentValidation.ValidationException>()).Which.Should().BeSameAs(refusal);
        written().ErrorMessage.Should().Be("Decline requires a note. Type the rationale.");
        refusal.Message.Should().StartWith("Validation failed:", "the exception the caller gets is unchanged");
        logger.Errors.Should().BeEmpty("a refusal is the reader's, not the log's");
    }

    /// <summary>T400 (K10, P1, F1): a refusal of authority Wombat's code threw is stored in its own words.</summary>
    [Fact]
    public async Task Handle_UnauthorizedAccess_StoresItsWords()
    {
        var logger = new RecordingLogger();
        var written = CaptureWrite();

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object, logger);
        var act = async () => await behavior.Handle(
            new TestCommand("x"),
            Throwing<string>(new UnauthorizedAccessException("Only global administrators may create institutions.")),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        written().ErrorMessage.Should().Be("Only global administrators may create institutions.");
        logger.Errors.Should().BeEmpty();
    }

    /// <summary>
    /// T400 (K10, P1): an <see cref="InvalidOperationException" /> the framework throws ("Sequence contains no elements.")
    /// is no refusal: the row stores "The action failed.", and the exception goes to the log whole, with the row's id, so
    /// the two can be matched. The caller still gets it as thrown.
    /// </summary>
    [Fact]
    public async Task Handle_FrameworksInvalidOperation_StoresTheActionFailed_AndLogsTheExceptionWithTheRowsId()
    {
        var logger = new RecordingLogger();
        var written = CaptureWrite();

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object, logger);
        var act = async () => await behavior.Handle(
            new TestCommand("x"),
            () => Task.FromResult(Enumerable.Empty<string>().First()),
            CancellationToken.None);

        var thrown = (await act.Should().ThrowAsync<InvalidOperationException>()).Which;
        thrown.Message.Should().Contain("Sequence contains no elements");

        var entry = written();
        entry.ErrorMessage.Should().Be("The action failed.");
        var logged = logger.Errors.Should().ContainSingle().Subject;
        logged.Exception.Should().BeSameAs(thrown);
        logged.Message.Should().Contain(nameof(TestCommand)).And.Contain(entry.Id.ToString());
    }

    /// <summary>T400 (K10, P1): an <see cref="InvalidOperationException" /> thrown by Wombat's code is a refusal.</summary>
    [Fact]
    public async Task Handle_WombatsInvalidOperation_IsARefusal_KeptInItsWords()
    {
        var logger = new RecordingLogger();
        var written = CaptureWrite();

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object, logger);
        var act = async () => await behavior.Handle(new TestCommand("x"), FailingNext<string>(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        written().ErrorMessage.Should().Be("Boom!");
        logger.Errors.Should().BeEmpty();
    }

    [Fact]
    public void FailureText_ByKind()
    {
        AuditFailureText.Of(new ArgumentException("Value cannot be null.")).Should().Be("The action failed.");
        AuditFailureText.Of(new DbUpdateException("duplicate key value")).Should().Be("The action failed.");
        AuditFailureText.Of(new InvalidOperationException("never thrown")).Should().Be(
            "The action failed.", "an exception never thrown has no throw site in Wombat's code");
        AuditFailureText.Of(Caught(() => Enumerable.Empty<int>().Single())).Should().Be("The action failed.");
        AuditFailureText.Of(Caught(() => throw new InvalidOperationException("Decline requires a note."))).Should().Be(
            "Decline requires a note.");
        AuditFailureText.Of(Caught(() => throw new MsfRefusedException("The campaign is closed."))).Should().Be(
            "The campaign is closed.", "a typed refusal derives from InvalidOperationException");
        AuditFailureText.Of(new FluentValidation.ValidationException("Validation failed: nothing listed")).Should().Be(
            "The action failed.", "a validation exception that lists no failure has no words for a reader");
    }

    /// <summary>
    /// The operator's F1 (the build review's S2): an <see cref="UnauthorizedAccessException" /> is a refusal only when Wombat's
    /// code threw it. The framework throws the same type for file access, with the path in its text, and that is no refusal.
    /// </summary>
    [Fact]
    public void FailureText_UnauthorizedAccess_ByWhoThrewIt()
    {
        AuditFailureText.Of(Caught(() => throw new UnauthorizedAccessException("Only global administrators may create institutions.")))
            .Should().Be("Only global administrators may create institutions.", "Wombat's code threw it");

        var frameworks = Caught(() => File.ReadAllText(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory)));
        frameworks.Should().BeOfType<UnauthorizedAccessException>("the framework refuses to open a directory as a file");
        AuditFailureText.Of(frameworks).Should().Be("The action failed.", "its text names a path, for a developer");

        AuditFailureText.Of(new UnauthorizedAccessException("never thrown")).Should().Be(
            "The action failed.", "an exception never thrown has no throw site in Wombat's code");
    }

    /// <summary>The build review's N-5: a cancelled command stores "The action failed." but is no error in the log.</summary>
    [Fact]
    public async Task Handle_Cancelled_StoresTheActionFailed_WithNoErrorLogged()
    {
        var logger = new RecordingLogger();
        var written = CaptureWrite();

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object, logger);
        var act = async () => await behavior.Handle(
            new TestCommand("x"), Throwing<string>(new OperationCanceledException()), CancellationToken.None);

        await act.Should().ThrowAsync<OperationCanceledException>();
        written().ErrorMessage.Should().Be("The action failed.");
        logger.Errors.Should().BeEmpty();
    }

    /// <summary>F1 through the pipeline: a framework's refusal of file access stores "The action failed." and is logged.</summary>
    [Fact]
    public async Task Handle_FrameworksUnauthorizedAccess_StoresTheActionFailed_AndLogsIt()
    {
        var logger = new RecordingLogger();
        var written = CaptureWrite();

        var behavior = new AuditPipelineBehavior<TestCommand, string>(_writerMock.Object, _contextMock.Object, logger);
        var act = async () => await behavior.Handle(
            new TestCommand("x"),
            () => Task.FromResult(File.ReadAllText(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory))),
            CancellationToken.None);

        var thrown = (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which;

        written().ErrorMessage.Should().Be("The action failed.");
        logger.Errors.Should().ContainSingle().Which.Exception.Should().BeSameAs(thrown);
    }

    private static Exception Caught(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            return exception;
        }

        throw new InvalidOperationException("Nothing was thrown.");
    }

    /// <summary>Captures the failure row written the ordinary way.</summary>
    private Func<AuditEntry> CaptureWrite()
    {
        AuditEntry? captured = null;
        _writerMock
            .Setup(w => w.WriteAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((e, _) => captured = e)
            .Returns(Task.CompletedTask);
        return () => captured ?? throw new InvalidOperationException("No row was written.");
    }

    private sealed class RecordingLogger : Microsoft.Extensions.Logging.ILogger<AuditPipelineBehavior<TestCommand, string>>
    {
        public List<(string Message, Exception? Exception)> Errors { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;

        public void Log<TState>(
            Microsoft.Extensions.Logging.LogLevel logLevel,
            Microsoft.Extensions.Logging.EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel >= Microsoft.Extensions.Logging.LogLevel.Error)
            {
                Errors.Add((formatter(state, exception), exception));
            }
        }
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
