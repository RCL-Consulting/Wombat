using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Commands.SaveActivityDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetFileAgainSource;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T342, flow 03's backend (lane A2): the move's hand-off and result on the activity page (B2, C3, E4), saving a draft
/// without a move (B5, E3), filing a declined request again (B10, E5), the viewer-aware programme wording (C12), the
/// picker's groups as the list query serves them (B8), and the locked assessor's refusal (C4, E2). Over the shipped
/// <c>mini_cex_cpsa</c> and <c>reflective_exercise_cpsa</c> seeds and the KGK teaching log of the runbook's Act 1.
/// </summary>
/// <remarks>
/// Every refusal is checked against the audit trap, as <see cref="EncounterDateBoundsTests" /> does: nothing is left dirty
/// in the change tracker, and the audit pipeline's save of the same context writes nothing.
/// </remarks>
public sealed class FilingServiceTests
{
    private const string TraineeId = "trainee-1";
    private const string AssessorId = "assessor-1";
    private const string LockedAssessorId = "assessor-locked";
    private const string CoordinatorId = "coordinator-1";
    private const int InstitutionId = 10;
    private const int CurriculumId = 3000;

    private const int MiniCexTypeId = 1;
    private const int TeachingLogTypeId = 2;
    private const int ReflectionTypeId = 3;

    /// <summary>On the trainee's curriculum, and its list names the Mini-CEX.</summary>
    private const int MiniCexEpaId = 5000;

    /// <summary>On the trainee's curriculum, and its list names only the CBD.</summary>
    private const int CbdOnlyEpaId = 5001;

    /// <summary>On the trainee's curriculum and names the Mini-CEX, but paused (D48).</summary>
    private const int PausedEpaId = 5002;

    private static readonly DateTimeOffset Now = new(2026, 9, 29, 8, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = ProgrammeCalendar.DateOf(Now.UtcDateTime);
    private static readonly DateOnly ProgrammeStart = new(2026, 1, 15);

    // ---- the move's words on the activity page (B2, C3, E4) --------------------------------------------------------

    [Fact]
    public async Task ASubmit_HandsTheRequestToTheNamedAssessor_AndReadsSubmittedItIsNowRequested()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));

        var detail = await DetailAsync(options, draft.Id, TraineeId);

        var submit = detail.AvailableActions.Single(action => action.TransitionKey == "submit");
        submit.Label.Should().Be("Submit");
        submit.HandOffFieldKey.Should().Be("assessor_user_id");
        submit.HandsToName.Should().Be($"First {AssessorId}");
        submit.TargetStateLabel.Should().Be("Requested");
        submit.TargetIsTerminal.Should().BeFalse();
        submit.ResultSentence.Should().Be("Submitted. It is now Requested.");

        var cancel = detail.AvailableActions.Single(action => action.TransitionKey == "cancel");
        cancel.HandOffFieldKey.Should().BeNull();
        cancel.HandsToName.Should().BeNull();
        cancel.ResultSentence.Should().Be("Cancelled.");
    }

    [Fact]
    public async Task ADraftNamingNobodyYet_HasTheFieldButNoName()
    {
        // The page, holding the value being typed, resolves the name from the picker's options itself.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(assessorId: null));

        var submit = (await DetailAsync(options, draft.Id, TraineeId)).AvailableActions
            .Single(action => action.TransitionKey == "submit");

        submit.HandOffFieldKey.Should().Be("assessor_user_id");
        submit.HandsToName.Should().BeNull();
    }

    [Fact]
    public async Task TheTeachingLogsLog_HandsItToNobody_ThoughItNamesASupervisingConsultant_AndReadsLogged()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, TeachingLogTypeId, TeachingLog(supervisorId: AssessorId));

        var log = (await DetailAsync(options, draft.Id, TraineeId)).AvailableActions
            .Single(action => action.TransitionKey == "log");

        log.Label.Should().Be("Log");
        log.HandOffFieldKey.Should().BeNull();
        log.HandsToName.Should().BeNull("the Supervising consultant receives nothing");
        log.TargetStateLabel.Should().Be("Logged");
        log.TargetIsTerminal.Should().BeTrue();
        log.ResultSentence.Should().Be("Logged.");
    }

    // ---- save a draft without a move (B5, E3) ----------------------------------------------------------------------

    [Fact]
    public async Task SaveDraft_WritesTheCallersFields_AndRestampsTheDate_WithNoHistoryRowAndNoMove()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId, Today.AddDays(-5)));

        var saved = await SaveDraftAsync(options, draft.Id, TraineeId, $$"""
            { "presenting_problem": "Wheeze, day two", "observed_on": "{{Iso(Today.AddDays(-4))}}" }
            """);

        saved.CurrentState.Should().Be("draft");
        var stored = await StoredAsync(options, draft.Id);
        stored.CurrentState.Should().Be("draft");
        stored.ObservedOn.Should().Be(Today.AddDays(-4));
        stored.UpdatedOn.Should().Be(Now.UtcDateTime);
        stored.Transitions.Should().ContainSingle("a save is no move: the create row stays the only one");
        Field(stored.DataJson, "presenting_problem").Should().Be("Wheeze, day two");
        Field(stored.DataJson, "setting").Should().Be("ward", "a field the save did not send is kept");
    }

    [Fact]
    public async Task SaveDraft_ChecksFormatsOnly_SoAHalfFilledDraftIsKept()
    {
        // `draft` validation: a required field left empty is no refusal until the submit.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));

        await SaveDraftAsync(options, draft.Id, TraineeId, """{ "presenting_problem": null }""");

        Field((await StoredAsync(options, draft.Id)).DataJson, "presenting_problem").Should().BeNull();
    }

    [Fact]
    public async Task SaveDraft_RefusesAFutureDate_AsAFieldErrorOnTheDate_AndChangesNothing()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId, Today.AddDays(-5)));

        var refusal = await RefusalAsync(options, service => service.SaveDraftAsync(
            SaveInput(draft.Id, TraineeId, $$"""{ "observed_on": "{{Iso(Today.AddDays(1))}}" }""")));

        refusal.Message.Should().Be($"Date observed: The date cannot be after today ({Iso(Today)}).");
        refusal.Should().BeOfType<ActivityFieldsRefusedException>().Which.FieldKeys.Should().Equal("observed_on");
        (await StoredAsync(options, draft.Id)).ObservedOn.Should().Be(Today.AddDays(-5));
    }

    [Fact]
    public async Task SaveDraft_RefusesADateBeforeTheProgramme_InTheRegistrarsOwnWords()
    {
        // C12: the registrar reads "your programme".
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));

        var refusal = await RefusalAsync(options, service => service.SaveDraftAsync(
            SaveInput(draft.Id, TraineeId, $$"""{ "observed_on": "{{Iso(ProgrammeStart.AddDays(-1))}}" }""")));

        refusal.Message.Should().Be("Date observed: The date cannot be before your programme started (2026-01-15).");
    }

    [Fact]
    public async Task SaveDraft_RefusesALockedAssessor_ByName_OnTheAssessorField()
    {
        // C4, E2: the Spec's words, as a field message on Assessor.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));

        var refusal = await RefusalAsync(options, service => service.SaveDraftAsync(
            SaveInput(draft.Id, TraineeId, $$"""{ "assessor_user_id": "{{LockedAssessorId}}" }""")));

        refusal.Message.Should().Be(
            $"Assessor: First {LockedAssessorId} cannot be named as an assessor. Choose someone else.");
        refusal.Should().BeOfType<ActivityFieldsRefusedException>().Which.FieldKeys.Should().Equal("assessor_user_id");
        Field((await StoredAsync(options, draft.Id)).DataJson, "assessor_user_id").Should().Be(AssessorId);
    }

    [Fact]
    public async Task SaveDraft_DoesNotJudgeAnUnchangedNominee_ASaveIsNoHandOn()
    {
        // The assessor was named while eligible and has since been locked. The save leaves them; the submit judges them.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));
        await LockAsync(options, AssessorId);

        await SaveDraftAsync(options, draft.Id, TraineeId, """{ "presenting_problem": "Still wheezy" }""");

        var refusal = await RefusalAsync(options, service => service.TransitionAsync(
            new TransitionActivityInput(draft.Id, "submit", TraineeId, Principal(TraineeId), null, null)));
        refusal.Message.Should().Be(
            $"Assessor: First {AssessorId} cannot be named as an assessor. Choose someone else.");
    }

    [Fact]
    public async Task SaveDraft_RefusesAnEpaTheInstrumentMayNotBeFiledOn()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));

        var refusal = await RefusalAsync(options, service => service.SaveDraftAsync(
            SaveInput(draft.Id, TraineeId, $$"""{ "epa_id": {{CbdOnlyEpaId}} }""")));

        refusal.Should().BeOfType<ActivityFieldsRefusedException>().Which.FieldKeys.Should().Contain("epa_id");
        (await StoredAsync(options, draft.Id)).EpaId.Should().Be(MiniCexEpaId);
    }

    [Fact]
    public async Task SaveDraft_MovesTheEpaStamp_WithTheSavedEpa()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, ReflectionTypeId, Reflection(MiniCexEpaId));

        await SaveDraftAsync(options, draft.Id, TraineeId, $$"""{ "epa_id": {{CbdOnlyEpaId}} }""");

        (await StoredAsync(options, draft.Id)).EpaId.Should().Be(CbdOnlyEpaId, "a reflection is no instrument (D21)");
    }

    [Fact]
    public async Task SaveDraft_RefusesAFieldTheCallerCannotWrite()
    {
        // The registrar's own rating: the Entrustment section is the assessor's.
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));

        var refusal = await RefusalAsync(options, service => service.SaveDraftAsync(
            SaveInput(draft.Id, TraineeId, """{ "overall_level": 5 }""")));

        refusal.Should().BeOfType<ActivityFieldsRefusedException>().Which.FieldKeys.Should().Equal("overall_level");
        refusal.Message.Should().EndWith("you cannot change this while the activity is Draft.");
    }

    [Fact]
    public async Task SaveDraft_IsRefused_OnceTheRequestIsHandedOn()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));
        await TransitionAsync(options, draft.Id, "submit", TraineeId);

        var refusal = await RefusalAsync(options, service => service.SaveDraftAsync(
            SaveInput(draft.Id, TraineeId, """{ "presenting_problem": "Changed my mind" }""")));

        refusal.Message.Should().Be("You cannot change this activity while it is Requested.");
        Field((await StoredAsync(options, draft.Id)).DataJson, "presenting_problem").Should().Be("Bronchiolitis");
    }

    [Fact]
    public async Task SaveDraft_TheAssessorCannotSaveTheRegistrarsDraft()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));

        (await RefusalAsync(options, service => service.SaveDraftAsync(
            SaveInput(draft.Id, AssessorId, """{ "presenting_problem": "Rewritten" }""")))).Message
            .Should().Be("You cannot change this activity while it is Draft.");

        // Once it is theirs to act on, their own fields only: the registrar's request is still hers.
        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        var refusal = await RefusalAsync(options, service => service.SaveDraftAsync(
            SaveInput(draft.Id, AssessorId, """{ "presenting_problem": "Rewritten" }""")));
        refusal.Should().BeOfType<ActivityFieldsRefusedException>().Which.FieldKeys.Should().Equal("presenting_problem");
        Field((await StoredAsync(options, draft.Id)).DataJson, "presenting_problem").Should().Be("Bronchiolitis");
    }

    [Fact]
    public async Task SaveDraft_ByAStranger_ReadsAsNotFound()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));

        (await RefusalAsync(options, service => service.SaveDraftAsync(
            SaveInput(draft.Id, CoordinatorId, """{ "presenting_problem": "x" }""")))).Message
            .Should().Be("The activity could not be found.");
    }

    [Fact]
    public async Task TheSaveDraftCommand_GoesThroughTheService_AndRedactsThePatchFromTheAudit()
    {
        var service = new Mock<IActivityService>(MockBehavior.Strict);
        var principal = Principal(TraineeId);
        service.Setup(mock => mock.SaveDraftAsync(
                It.Is<SaveActivityDraftInput>(input =>
                    input.ActivityId == 7 && input.ActorUserId == TraineeId && input.DataPatchJson == "{}" &&
                    ReferenceEquals(input.Principal, principal)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActivityDto)null!);

        await new SaveActivityDraftCommandHandler(service.Object).Handle(
            new SaveActivityDraftCommand(7, TraineeId, principal, "{}"), CancellationToken.None);

        service.VerifyAll();
        typeof(SaveActivityDraftCommand).GetProperty(nameof(SaveActivityDraftCommand.DataPatchJson))!
            .GetCustomAttributes(typeof(Wombat.Application.Audit.RedactAttribute), inherit: false)
            .Should().ContainSingle();
    }

    // ---- the programme start, worded for its reader (C12) ----------------------------------------------------------

    [Fact]
    public async Task ADateBeforeTheProgramme_FiledBySomeoneElse_ReadsTheTraineesProgramme()
    {
        var options = await SeededAsync();

        var refusal = await RefusalAsync(options, service => service.CreateDraftAsync(new CreateActivityInput(
            MiniCexTypeId, TraineeId, CoordinatorId, MiniCex(AssessorId, ProgrammeStart.AddDays(-1)), Principal(CoordinatorId))));

        refusal.Message.Should().Be("Date observed: The date cannot be before the trainee's programme started (2026-01-15).");
    }

    [Fact]
    public void TheWordingHelper_GivesTheHintAndTheRefusalForEachReader()
    {
        ProgrammeStartWording.Refusal(readerIsSubject: true, ProgrammeStart)
            .Should().Be("The date cannot be before your programme started (2026-01-15).");
        ProgrammeStartWording.Refusal(readerIsSubject: false, ProgrammeStart)
            .Should().Be("The date cannot be before the trainee's programme started (2026-01-15).");
        ProgrammeStartWording.Hint(readerIsSubject: true, ProgrammeStart)
            .Should().Be("This date is before your programme started (2026-01-15), and will not be accepted.");
        ProgrammeStartWording.Hint(readerIsSubject: false, ProgrammeStart)
            .Should().Be("This date is before the trainee's programme started (2026-01-15), and will not be accepted.");
    }

    // ---- file it again (B10, E5) -----------------------------------------------------------------------------------

    [Fact]
    public async Task ADeclinedRequest_IsCopied_WithoutItsAssessor()
    {
        var options = await SeededAsync();
        var declined = await DeclinedMiniCexAsync(options);

        var copy = await FileAgainAsync(options, declined, TraineeId);

        copy.Should().NotBeNull();
        copy!.SourceActivityId.Should().Be(declined);
        copy.ActivityTypeId.Should().Be(MiniCexTypeId);
        copy.ActivityTypeKey.Should().Be("mini_cex_cpsa");
        copy.CopiedFieldKeys.Should().Equal("epa_id", "observed_on", "setting", "presenting_problem", "complexity");
        copy.EpaDropped.Should().BeFalse();
        copy.DeclinedByName.Should().Be("Fatima Khumalo");
        copy.DeclinedOn.Should().Be(Now.UtcDateTime);

        Field(copy.DataJson, "assessor_user_id").Should().BeNull("she names someone else");
        Field(copy.DataJson, "presenting_problem").Should().Be("Bronchiolitis");
        Field(copy.DataJson, "observed_on").Should().Be(Iso(Today.AddDays(-3)));
        JsonDocument.Parse(copy.DataJson).RootElement.GetProperty("epa_id").GetInt32().Should().Be(MiniCexEpaId);
    }

    [Fact]
    public async Task FileAgain_DropsAPausedEpa_AndSaysSo()
    {
        var options = await SeededAsync();
        var declined = await DeclinedMiniCexAsync(options, epaId: PausedEpaId, pauseAfterFiling: true);

        var copy = await FileAgainAsync(options, declined, TraineeId);

        copy!.EpaDropped.Should().BeTrue();
        copy.CopiedFieldKeys.Should().NotContain("epa_id");
        Field(copy.DataJson, "epa_id").Should().BeNull();
    }

    [Fact]
    public async Task FileAgain_DropsAKeyTheCurrentVersionLacks()
    {
        var options = await SeededAsync();
        var declined = await DeclinedMiniCexAsync(options);
        await PublishWithoutComplexityAsync(options);

        var copy = await FileAgainAsync(options, declined, TraineeId);

        copy!.CopiedFieldKeys.Should().NotContain("complexity");
        Field(copy.DataJson, "complexity").Should().BeNull();
        copy.CopiedFieldKeys.Should().Contain("presenting_problem");
    }

    [Fact]
    public async Task FileAgain_IsOnlyForTheSubject()
    {
        var options = await SeededAsync();
        var declined = await DeclinedMiniCexAsync(options);

        (await FileAgainAsync(options, declined, AssessorId)).Should().BeNull();
        (await FileAgainAsync(options, declined, CoordinatorId)).Should().BeNull();
    }

    [Fact]
    public async Task FileAgain_IsOnlyForADeclinedRequest_NotACancelledOneOrOneStillOpen()
    {
        var options = await SeededAsync();

        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));
        (await FileAgainAsync(options, draft.Id, TraineeId)).Should().BeNull("a draft");

        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        (await FileAgainAsync(options, draft.Id, TraineeId)).Should().BeNull("a request still with its assessor");

        await TransitionAsync(options, draft.Id, "cancel", TraineeId);
        (await StoredAsync(options, draft.Id)).CurrentState.Should().Be("cancelled", "guard");
        (await FileAgainAsync(options, draft.Id, TraineeId)).Should().BeNull("a cancel is her own choice (E5)");

        (await FileAgainAsync(options, 999_999, TraineeId)).Should().BeNull("no such activity");
    }

    [Fact]
    public async Task FileAgain_IsNotOfferedForACompletedAssessment()
    {
        var options = await SeededAsync();
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId));
        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        await TransitionAsync(options, draft.Id, "complete", AssessorId,
            """{ "overall_level": 3, "strengths": "Clear.", "improvements": "Earlier escalation.", "plan": "Repeat." }""");

        (await FileAgainAsync(options, draft.Id, TraineeId)).Should().BeNull();
    }

    // ---- the picker's groups, served (B8) --------------------------------------------------------------------------

    [Fact]
    public async Task TheTypeList_CarriesEachTypesShape_AndWhetherItCreditsNothing()
    {
        var options = await SeededAsync();
        await using var db = new ApplicationDbContext(options);

        var types = await new ListActivityTypesQueryHandler(db).Handle(
            new ListActivityTypesQuery(Principal(TraineeId)), CancellationToken.None);

        types.Select(type => (type.Key, type.Shape, type.CreditsNothing)).Should().BeEquivalentTo(
        [
            ("mini_cex_cpsa", ActivityTypeShape.Rated, false),
            ("kgk_teaching_log", ActivityTypeShape.LoggedByYou, true),
            ("reflective_exercise_cpsa", ActivityTypeShape.DiscussedOrReviewed, true)
        ]);
    }

    // ---- helpers ---------------------------------------------------------------------------------------------------

    private static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string MiniCex(string? assessorId, DateOnly? observedOn = null, int epaId = MiniCexEpaId) => $$"""
        {
          "epa_id": {{epaId}},
          "assessor_user_id": {{(assessorId is null ? "null" : $"\"{assessorId}\"")}},
          "observed_on": "{{Iso(observedOn ?? Today.AddDays(-3))}}",
          "setting": "ward",
          "presenting_problem": "Bronchiolitis",
          "complexity": "moderate"
        }
        """;

    private static string TeachingLog(string? supervisorId) => $$"""
        {
          "topic": "Fluids in DKA",
          "epa_id": {{MiniCexEpaId}},
          "delivered_on": "{{Iso(Today.AddDays(-1))}}",
          "audience": "Interns",
          "objectives": "Calculate the deficit.",
          "supervisor_user_id": {{(supervisorId is null ? "null" : $"\"{supervisorId}\"")}}
        }
        """;

    private static string Reflection(int epaId) => $$"""
        {
          "epa_id": {{epaId}},
          "assessor_user_id": "{{AssessorId}}",
          "observed_on": "{{Iso(Today.AddDays(-2))}}",
          "prompt": "challenging_case",
          "what_happened": "A difficult conversation."
        }
        """;

    private static string? Field(string dataJson, string key)
    {
        using var document = JsonDocument.Parse(dataJson);
        return document.RootElement.TryGetProperty(key, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => value.GetString(),
                _ => value.GetRawText()
            }
            : null;
    }

    private static SaveActivityDraftInput SaveInput(int activityId, string actorId, string patch)
        => new(activityId, actorId, Principal(actorId), patch);

    private static async Task<ActivityDto> CreateAsync(DbContextOptions<ApplicationDbContext> options, int typeId, string dataJson)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).CreateDraftAsync(new CreateActivityInput(typeId, TraineeId, TraineeId, dataJson, Principal(TraineeId)));
    }

    private static async Task<ActivityDto> TransitionAsync(
        DbContextOptions<ApplicationDbContext> options,
        int activityId,
        string transitionKey,
        string actorId,
        string? patch = null,
        string? note = null)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).TransitionAsync(
            new TransitionActivityInput(activityId, transitionKey, actorId, Principal(actorId), patch, note));
    }

    private static async Task<ActivityDto> SaveDraftAsync(
        DbContextOptions<ApplicationDbContext> options, int activityId, string actorId, string patch)
    {
        await using var db = new ApplicationDbContext(options);
        return await Service(db).SaveDraftAsync(SaveInput(activityId, actorId, patch));
    }

    private static async Task<ActivityDetailDto> DetailAsync(DbContextOptions<ApplicationDbContext> options, int activityId, string viewerId)
    {
        await using var db = new ApplicationDbContext(options);
        return (await Service(db).GetDetailAsync(activityId, Principal(viewerId)))!;
    }

    /// <summary>A Mini-CEX request to the assessor, declined by her with a reason. Returns its id.</summary>
    private static async Task<int> DeclinedMiniCexAsync(
        DbContextOptions<ApplicationDbContext> options,
        int epaId = MiniCexEpaId,
        bool pauseAfterFiling = false)
    {
        var draft = await CreateAsync(options, MiniCexTypeId, MiniCex(AssessorId, epaId: epaId));
        await TransitionAsync(options, draft.Id, "submit", TraineeId);
        await TransitionAsync(options, draft.Id, "decline", AssessorId, note: "I was not there that day.");

        if (pauseAfterFiling)
        {
            await using var db = new ApplicationDbContext(options);
            (await db.Epas.SingleAsync(epa => epa.Id == epaId)).Deactivate(Now.UtcDateTime);
            await db.SaveChangesAsync();
        }

        (await StoredAsync(options, draft.Id)).CurrentState.Should().Be("declined", "guard");
        return draft.Id;
    }

    private static async Task<FileAgainSourceDto?> FileAgainAsync(
        DbContextOptions<ApplicationDbContext> options, int sourceId, string callerId)
    {
        await using var db = new ApplicationDbContext(options);
        var users = new Mock<IUserAdministrationService>();
        users.Setup(mock => mock.GetDisplayNamesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<string> ids, CancellationToken _) =>
                ids.Where(id => id == AssessorId).ToDictionary(id => id, _ => "Fatima Khumalo"));

        return await new GetFileAgainSourceQueryHandler(
                db, new FieldPermissionEvaluator(), new ActivityReferenceDataService(db), users.Object)
            .Handle(new GetFileAgainSourceQuery(sourceId, Principal(callerId)), CancellationToken.None);
    }

    /// <summary>Publishes version 2 of the Mini-CEX, whose request no longer asks for Case complexity.</summary>
    private static async Task PublishWithoutComplexityAsync(DbContextOptions<ApplicationDbContext> options)
    {
        await using var db = new ApplicationDbContext(options);
        var type = await db.ActivityTypes.Include(entity => entity.Versions).SingleAsync(entity => entity.Id == MiniCexTypeId);
        var schema = type.SchemaJson!.Replace("\"key\": \"complexity\"", "\"key\": \"case_complexity\"", StringComparison.Ordinal);
        schema.Should().NotBe(type.SchemaJson, "guard: the key really is renamed");

        type.SchemaJson = schema;
        type.Version = 2;
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = type.Id,
            Version = 2,
            SchemaJson = schema,
            WorkflowJson = type.WorkflowJson!,
            CreditRulesJson = type.CreditRulesJson!,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = Now.UtcDateTime
        });
        await db.SaveChangesAsync();
    }

    private static async Task LockAsync(DbContextOptions<ApplicationDbContext> options, string userId)
    {
        await using var db = new ApplicationDbContext(options);
        (await db.Users.SingleAsync(user => user.Id == userId)).LockoutEnd = DateTimeOffset.MaxValue;
        await db.SaveChangesAsync();
    }

    private static async Task<InvalidOperationException> RefusalAsync(
        DbContextOptions<ApplicationDbContext> options,
        Func<ActivityService, Task> act)
    {
        await using var db = new ApplicationDbContext(options);

        var attempt = async () => await act(Service(db));
        var thrown = await attempt.Should().ThrowAsync<InvalidOperationException>();

        db.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => $"{entry.Metadata.ClrType.Name}: {entry.State}")
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");
        (await db.SaveChangesAsync()).Should().Be(0);

        return thrown.Which;
    }

    private static async Task<Activity> StoredAsync(DbContextOptions<ApplicationDbContext> options, int activityId)
    {
        await using var db = new ApplicationDbContext(options);
        return await db.Activities.AsNoTracking().Include(entity => entity.Transitions).SingleAsync(entity => entity.Id == activityId);
    }

    private static async Task<DbContextOptions<ApplicationDbContext>> SeededAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);

        NomineeSeed.AddUser(db, TraineeId, InstitutionId, WombatRoles.Trainee);
        NomineeSeed.AddUser(db, AssessorId, InstitutionId, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, LockedAssessorId, InstitutionId, DateTimeOffset.MaxValue, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, CoordinatorId, InstitutionId, WombatRoles.Coordinator);

        db.WbaTools.AddRange(new WbaTool { Key = "mini_cex", Name = "Mini-CEX" }, new WbaTool { Key = "cbd", Name = "CBD" });
        db.Epas.AddRange(
            new Epa { Id = MiniCexEpaId, SubSpecialityId = 1, Code = "PAED-001", Title = "An EPA the Mini-CEX may be filed on" },
            new Epa { Id = CbdOnlyEpaId, SubSpecialityId = 1, Code = "PAED-002", Title = "An EPA only the CBD may be filed on" },
            new Epa { Id = PausedEpaId, SubSpecialityId = 1, Code = "PAED-003", Title = "An EPA to be paused" });
        db.CurriculumItems.AddRange(
            Item(4000, MiniCexEpaId, """["mini_cex","cbd"]"""),
            Item(4001, CbdOnlyEpaId, """["cbd"]"""),
            Item(4002, PausedEpaId, """["mini_cex"]"""));
        db.TraineeProfiles.Add(new TraineeProfile
        {
            Id = 1,
            UserId = TraineeId,
            InstitutionId = InstitutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = ProgrammeStart,
            ExpectedCompletionDate = ProgrammeStart.AddYears(4),
            IsActive = true
        });

        db.ActivityTypes.Add(PublishedType(
            MiniCexTypeId, "mini_cex_cpsa", ReadSeed("mini_cex_cpsa", "schema.json"), ReadSeed("mini_cex_cpsa", "workflow.json"),
            ReadSeed("mini_cex_cpsa", "credit.json"), "mini_cex"));
        db.ActivityTypes.Add(PublishedType(
            TeachingLogTypeId, "kgk_teaching_log", FilingPickerAndMoveWordsTests.TeachingLogSchema,
            FilingPickerAndMoveWordsTests.TeachingLogWorkflow, """{"counts_for": []}""", wbaToolKey: null));
        db.ActivityTypes.Add(PublishedType(
            ReflectionTypeId, "reflective_exercise_cpsa", ReadSeed("reflective_exercise_cpsa", "schema.json"),
            ReadSeed("reflective_exercise_cpsa", "workflow.json"), ReadSeed("reflective_exercise_cpsa", "credit.json"),
            wbaToolKey: null));

        await db.SaveChangesAsync();
        return options;
    }

    private static CurriculumItem Item(int id, int epaId, string permittedToolsJson)
        => new()
        {
            Id = id,
            CurriculumId = CurriculumId,
            EpaId = epaId,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3,
            WindowMonths = 12,
            PermittedToolsJson = permittedToolsJson
        };

    private static ActivityType PublishedType(
        int id, string key, string schemaJson, string workflowJson, string creditJson, string? wbaToolKey)
    {
        var type = new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Scope = ActivityScope.Global,
            Version = 1,
            WbaToolKey = wbaToolKey,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            OwnerUserId = "system",
            CreatedOn = Now.UtcDateTime
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            ActivityTypeId = id,
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditJson,
            DisplayFieldsJson = "[]",
            PublishedByUserId = "system",
            PublishedOn = Now.UtcDateTime
        });
        return type;
    }

    private static string ReadSeed(string folder, string file)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", folder, file));

    private static ActivityService Service(ApplicationDbContext db)
        => new(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator(),
            new FixedClock(Now));

    private static ClaimsPrincipal Principal(string userId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], "test"));

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
