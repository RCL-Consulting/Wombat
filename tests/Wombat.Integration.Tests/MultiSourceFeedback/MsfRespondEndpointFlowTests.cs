using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Wombat.Api.Endpoints;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T021 and T121 end to end: the Api host on a real PostgreSQL server, from an opened campaign through anonymous
/// responses to the released report and the per-EPA evidence it leaves on the trainee's record.
/// </summary>
/// <remarks>
/// <para>
/// The host runs on a schema of its own (<c>SearchPath = it_&lt;guid&gt;</c>, nothing else on the path), so no statement
/// can reach <c>public</c>. xUnit 2 does not call <see cref="DisposeAsync" /> after a failed
/// <see cref="InitializeAsync" />, so everything in setup that can fail after the schema exists runs inside a
/// <c>try</c> whose <c>catch</c> tears the host down and drops the schema before rethrowing (T140). Before that, every
/// failed setup left an <c>it_</c> schema behind in whatever database the suite pointed at.
/// </para>
/// <para>
/// The Api host migrates at startup but seeds nothing; only the Web host seeds. So setup replays the Web host's
/// catalogue seeding against the Api host's own services, and admits the subject onto the College's curriculum
/// by name (T140).
/// </para>
/// </remarks>
public sealed class MsfRespondEndpointFlowTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    /// <summary>The College's v11.1 curriculum, as <c>PaediatricCatalogueSeeder</c> names it.</summary>
    private const string PaediatricCurriculumName = "Paediatric EPA Curriculum";

    /// <summary>The institution <c>DataSeeder</c> creates.</summary>
    private const string DemoInstitutionShortCode = "DEMO";

    private readonly string _schemaName = $"it_{Guid.NewGuid():N}";
    private ApiFactory? _factory;
    private HttpClient? _client;
    private string? _baseConnectionString;

    /// <summary>
    /// T121: the campaign now has to name EPAs from the subject's own curriculum, so the subject has to
    /// be a real admitted trainee rather than a bare user id.
    /// </summary>
    private int _institutionId;
    private int _coveredEpaId;
    private ClaimsPrincipal _coordinator = null!;

    private ApiFactory Factory
        => _factory ?? throw new InvalidOperationException("The Api host is not running: InitializeAsync did not complete.");

    private HttpClient Client
        => _client ?? throw new InvalidOperationException("The Api host is not running: InitializeAsync did not complete.");

    private string BaseConnectionString
        => _baseConnectionString ?? throw new InvalidOperationException("The base connection string was never resolved.");

    private string SchemaConnectionString
    {
        get
        {
            var builder = new NpgsqlConnectionStringBuilder(BaseConnectionString)
            {
                SearchPath = _schemaName,
                Pooling = false
            };

            return builder.ConnectionString;
        }
    }

    public async Task InitializeAsync()
    {
        _baseConnectionString = ResolveBaseConnectionString();

        try
        {
            await CreateSchemaAsync();

            _factory = new ApiFactory(SchemaConnectionString, "http://localhost/msf/respond");

            // Starts the host, and the host migrates the schema before it serves anything.
            _client = _factory.CreateClient();

            await SeedCatalogueAsync();
            await AdmitSubjectAsync();
        }
        catch (Exception setupFailure)
        {
            // The only chance to clean up: xUnit 2 will not call DisposeAsync once this method has thrown. A failed
            // teardown must not hide the setup failure that caused it, so both are reported.
            try
            {
                await TearDownAsync();
            }
            catch (Exception teardownFailure)
            {
                throw new AggregateException(
                    "InitializeAsync failed, and so did the teardown that should have dropped its schema.",
                    setupFailure,
                    teardownFailure);
            }

            throw;
        }
    }

    public Task DisposeAsync() => TearDownAsync();

    /// <summary>
    /// Stops the host and drops the schema. Safe to call more than once, and at any point in setup: every step
    /// checks what exists, and the drop runs even when stopping the host throws.
    /// </summary>
    private async Task TearDownAsync()
    {
        try
        {
            _client?.Dispose();
            _client = null;

            if (_factory is not null)
            {
                var factory = _factory;
                _factory = null;
                await factory.DisposeAsync();
            }
        }
        finally
        {
            await DropSchemaAsync();
        }
    }

    private async Task CreateSchemaAsync()
    {
        await using var connection = new NpgsqlConnection(BaseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA \"{_schemaName}\"";
        await command.ExecuteNonQueryAsync();
    }

    private async Task DropSchemaAsync()
    {
        if (string.IsNullOrWhiteSpace(_baseConnectionString))
        {
            return;
        }

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        await using var drop = connection.CreateCommand();
        drop.CommandText = $"DROP SCHEMA IF EXISTS \"{_schemaName}\" CASCADE";
        await drop.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// The catalogue seeding the Web host's startup runs after it migrates: <c>DataSeeder</c>, then
    /// <c>PaediatricCatalogueSeeder</c>, each resolved from the Api host's own container and each in its own scope,
    /// as <c>AcademicPeriodQuotaPostgresTests.SeededSchemaAsync</c> does.
    /// </summary>
    /// <remarks>
    /// The Api host's <c>Program.cs</c> migrates and seeds nothing, so without this a fresh schema holds no
    /// institution, no curriculum and no <c>msf_cpsa</c> type, and the release would have nothing to record
    /// evidence with. (T140)
    /// </remarks>
    private async Task SeedCatalogueAsync()
    {
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<DataSeeder>().SeedAsync();
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<PaediatricCatalogueSeeder>().SeedAsync();
        }
    }

    /// <summary>
    /// Admits "trainee-1" onto the College's paediatric curriculum and builds the coordinator who runs their campaign.
    /// </summary>
    /// <remarks>
    /// Before T121 the subject was a bare string and nothing resolved it. The campaign now narrows its
    /// EPA coverage to the subject's curriculum, the create is refused outside the caller's institution,
    /// and the release fans out activities scope-stamped from the subject's profile - so all three need a
    /// real profile to exercise. The curriculum is chosen by name, not as "the first one with a national item":
    /// <c>msf_cpsa</c> is scoped to the paediatric discipline, so a subject on any other curriculum would have
    /// their evidence refused at release.
    /// </remarks>
    private async Task AdmitSubjectAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var curriculumId = await dbContext.Curricula
            .Where(entity => entity.Name == PaediatricCurriculumName)
            .Select(entity => entity.Id)
            .SingleAsync();

        // One of the College's own items: national, so it is on this curriculum for every adopting institution.
        _coveredEpaId = await dbContext.CurriculumItems
            .Where(item => item.CurriculumId == curriculumId && item.OwningInstitutionId == null)
            .OrderBy(item => item.Epa.Code)
            .Select(item => item.EpaId)
            .FirstAsync();

        _institutionId = await dbContext.Institutions
            .Where(entity => entity.ShortCode == DemoInstitutionShortCode)
            .Select(entity => entity.Id)
            .SingleAsync();

        dbContext.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = "trainee-1",
            InstitutionId = _institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(3)),
            IsActive = true
        });

        await dbContext.SaveChangesAsync();

        // Built with the role and institution claim types the app issues, and with ClaimsIdentity told
        // which claim carries a role - ClaimsPrincipal.IsInRole is the BCL instance method and reads
        // RoleClaimType, so an identity built without it matches no role: and `role:Coordinator` is what
        // lets the release record evidence at all.
        _coordinator = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "coordinator-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, _institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));
    }

    [Fact]
    public async Task OpenRespondCloseRelease_Flow_WorksAgainstLiveApiAndPreservesAnonymity()
    {
        var template = await SendAsync(new CreateMsfTemplateCommand(
            "Annual MSF",
            null,
            false,
            [
                new CreateMsfTemplateQuestionItem("Rates the trainee's overall professional performance.", MsfQuestionType.Scale, null, true),
                new CreateMsfTemplateQuestionItem("What should the trainee keep doing or improve?", MsfQuestionType.LongText, null, false)
            ]));

        var campaign = await SendAsync(new CreateMsfCampaignCommand(
            "trainee-1",
            template.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            4,
            2,
            2,
            [_coveredEpaId],
            "coordinator-1",
            _coordinator));

        var invitees = CreateInvitees(campaign.Id);
        foreach (var invitee in invitees)
        {
            await SendAsync(new AddMsfInvitationCommand(campaign.Id, invitee.Email, invitee.Category, _coordinator));
        }

        await SendAsync(new OpenMsfCampaignCommand(campaign.Id, _coordinator));

        Factory.EmailSender.Messages.Should().HaveCount(8);
        Factory.EmailSender.Messages.Should().OnlyContain(message => message.TextBody.Contains("http://localhost/msf/respond?token=", StringComparison.Ordinal));

        // Two consultants and two nurses: two categories at the floor of two responses each, which is what the
        // release gate (D11) asks of this campaign. Chosen by address rather than as "the first four messages",
        // because the open handler sends in whatever order the invitations load, and Postgres promises none.
        var respondentEmails = invitees
            .Where(invitee => invitee.Category is MsfRespondentCategory.Consultant or MsfRespondentCategory.Nurse)
            .Select(invitee => invitee.Email)
            .ToHashSet(StringComparer.Ordinal);
        var respondentMessages = Factory.EmailSender.Messages
            .Where(message => respondentEmails.Contains(message.To))
            .ToList();
        respondentMessages.Should().HaveCount(4, "guard: two consultants and two nurses are invited");

        foreach (var message in respondentMessages)
        {
            var token = ExtractToken(message.TextBody);

            var formResponse = await Client.GetAsync($"/msf/respond?token={Uri.EscapeDataString(token)}");
            formResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var form = await formResponse.Content.ReadFromJsonAsync<MsfResponseFormDto>();
            form.Should().NotBeNull();
            form!.Questions.Should().HaveCount(2);

            var submitResponse = await Client.PostAsJsonAsync(
                $"/msf/respond?token={Uri.EscapeDataString(token)}",
                new MsfRespondSubmission
                {
                    Answers = form.Questions.Select(question => question.Type == MsfQuestionType.Scale
                        ? new MsfRespondAnswerRequest { QuestionId = question.QuestionId, ScaleValue = 4, LongText = null }
                        : new MsfRespondAnswerRequest { QuestionId = question.QuestionId, ScaleValue = null, LongText = "Consistent and helpful." })
                        .ToList()
                });

            submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await dbContext.MsfResponses.CountAsync()).Should().Be(4);
            (await dbContext.MsfInvitations.CountAsync(invitation => invitation.RespondedOn != null)).Should().Be(4);
        }

        var closedReport = await SendAsync(new CloseMsfCampaignCommand(campaign.Id, _coordinator));
        closedReport.TotalResponses.Should().Be(4);
        closedReport.State.Should().Be(MsfCampaignState.UnderReview);
        closedReport.Categories.Should().Contain(category => !category.IsSuppressed);
        closedReport.Categories.Should().OnlyContain(category =>
            category.Questions.All(question =>
                question.Comments.All(comment => !comment.Contains('@', StringComparison.Ordinal))));

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var invitations = await dbContext.MsfInvitations.OrderBy(invitation => invitation.Id).ToListAsync();
            invitations.Should().OnlyContain(invitation => invitation.RespondentEmail == null);
            invitations.Should().OnlyContain(invitation => !string.IsNullOrWhiteSpace(invitation.RespondentEmailHash));

            // A coordinator reviews between close and release, so the two are normally days apart. Here they run
            // seconds apart, on one UTC day, and evidence dated from the release would pass the ObservedOn check
            // below. Moving the close two days back separates all three candidate dates: the release (today), the
            // close (two days ago) and the schedule (a week ahead). Nothing on the release path reads a response's
            // timestamp, so the responses keep theirs.
            var closedAt = await dbContext.MsfCampaigns
                .Where(entity => entity.Id == campaign.Id)
                .Select(entity => entity.ClosedOn)
                .SingleAsync();
            closedAt.Should().NotBeNull("guard: CloseMsfCampaign stamps ClosedOn");

            var backdated = await dbContext.MsfCampaigns
                .Where(entity => entity.Id == campaign.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(entity => entity.ClosedOn, closedAt!.Value.AddDays(-2)));
            backdated.Should().Be(1);
        }

        await SendAsync(new ReleaseMsfCampaignCommand(
            campaign.Id, "coordinator-1", "Released after coordinator review.", 4, _coordinator));

        // T113: the coordinator list is confined in SQL through the preferred-profile set, so this is where the
        // NOT EXISTS it translates to is exercised against PostgreSQL rather than the in-memory provider.
        (await SendAsync(new ListMsfCampaignsForCoordinatorQuery(_coordinator)))
            .Should().ContainSingle(summary => summary.Id == campaign.Id);
        (await SendAsync(new ListMsfCampaignsForCoordinatorQuery(CoordinatorElsewhere())))
            .Should().NotContain(summary => summary.Id == campaign.Id);
        (await SendAsync(new GetCampaignAggregateReportQuery(campaign.Id, CoordinatorElsewhere())))
            .Should().BeNull();

        var traineeReports = await SendAsync(new ListMsfCampaignsForTraineeQuery("trainee-1", Trainee()));
        traineeReports.Should().ContainSingle(report => report.Id == campaign.Id && report.ResponseCount == 4);

        var releasedReport = (await SendAsync(new GetCampaignAggregateReportQuery(campaign.Id, Trainee())))!;
        releasedReport.State.Should().Be(MsfCampaignState.Released);
        releasedReport.CoordinatorNarrative.Should().Be("Released after coordinator review.");
        releasedReport.ReadyForRelease.Should().BeTrue();
        releasedReport.Categories.Should().OnlyContain(category =>
            category.Questions.All(question =>
                question.Comments.All(comment => !comment.Contains('@', StringComparison.Ordinal))));

        // T121: the release leaves one terminal msf_cpsa activity per covered EPA behind, and that is the
        // only thing that ever connected a campaign to a curriculum.
        releasedReport.CoveredEpas.Should().ContainSingle(epa => epa.EpaId == _coveredEpaId);
        releasedReport.EvidenceRecordedOn.Should().NotBeNull();

        var activities = await SendAsync(new ListActivitiesBySubjectQuery("trainee-1", _coordinator));
        var evidence = activities.Should().ContainSingle(activity => activity.ActivityTypeKey == "msf_cpsa").Subject;
        evidence.CurrentState.Should().Be("recorded");

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var activity = await dbContext.Activities
                .Include(entity => entity.Transitions)
                .SingleAsync(entity => entity.Id == evidence.Id);

            // Read as values, never as text: Postgres discards the submitted jsonb bytes and renders its own
            // (`"epa_id": 12`, with a space), so a substring of the serializer's output never matches what is stored.
            using var data = JsonDocument.Parse(activity.DataJson);
            data.RootElement.GetProperty("epa_id").GetInt32().Should().Be(_coveredEpaId);
            data.RootElement.GetProperty("overall_level").GetInt32().Should().Be(4);
            activity.DataJson.Should().NotContain("@example.test");

            // T121: the evidence is dated the day the window actually shut (ClosedOn): not the day it was scheduled
            // to shut (ClosesOn), which would stamp a future ObservedOn on a campaign closed early, and not the day
            // it was released. Both guards hold by construction: the campaign closed a week ahead of its schedule,
            // and its close was moved two days before its release.
            var closedOn = await dbContext.MsfCampaigns
                .Where(entity => entity.Id == campaign.Id)
                .Select(entity => entity.ClosedOn)
                .SingleAsync();
            closedOn.Should().NotBeNull();

            var closedOnDate = DateOnly.FromDateTime(closedOn!.Value);
            closedOnDate.Should().BeBefore(campaign.ClosesOn, "guard: the campaign is closed ahead of its schedule");
            closedOnDate.Should().BeBefore(
                DateOnly.FromDateTime(releasedReport.EvidenceRecordedOn!.Value),
                "guard: the campaign closed on an earlier day than it was released");
            activity.ObservedOn.Should().Be(closedOnDate);
            data.RootElement.GetProperty("observed_on").GetString()
                .Should().Be(closedOnDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            // D8: MSF consumes none of Annexure A's 55 encounters, so `counts_for` is empty, the applier
            // is never reached and the stamp stays null - T108's "credit was never evaluated".
            var record = activity.Transitions.Single(entity => entity.TransitionKey == "record");
            record.CreditedItemCount.Should().BeNull();
            (await dbContext.CurriculumItemProgresses.CountAsync()).Should().Be(0);

            // The fixture removes the scheduler, and its first act on starting is to write its job definitions, so
            // an empty table says no background job ran beside this flow.
            (await dbContext.ScheduledJobDefinitions.CountAsync())
                .Should().Be(0, "guard: the test host runs no scheduler");
        }
    }

    private static IReadOnlyList<(string Email, MsfRespondentCategory Category)> CreateInvitees(int campaignId)
        => new (string Email, MsfRespondentCategory Category)[]
        {
            ($"consultant-1-{campaignId}@example.test", MsfRespondentCategory.Consultant),
            ($"consultant-2-{campaignId}@example.test", MsfRespondentCategory.Consultant),
            ($"nurse-1-{campaignId}@example.test", MsfRespondentCategory.Nurse),
            ($"nurse-2-{campaignId}@example.test", MsfRespondentCategory.Nurse),
            ($"other-1-{campaignId}@example.test", MsfRespondentCategory.Other),
            ($"other-2-{campaignId}@example.test", MsfRespondentCategory.Other),
            ($"other-3-{campaignId}@example.test", MsfRespondentCategory.Other),
            ($"peer-1-{campaignId}@example.test", MsfRespondentCategory.PeerDoctor)
        };

    /// <summary>The campaign's subject, signed in, with the claims the app issues.</summary>
    private ClaimsPrincipal Trainee()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "trainee-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Trainee),
                new Claim(WombatClaimTypes.InstitutionId, _institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>A coordinator at an institution the subject does not train at.</summary>
    private ClaimsPrincipal CoordinatorElsewhere()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "coordinator-elsewhere"),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, (_institutionId + 10_000).ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private static string ExtractToken(string body)
    {
        var match = Regex.Match(body, @"token=([^&\s]+)", RegexOptions.CultureInvariant);
        match.Success.Should().BeTrue("the email body should contain a responder token");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }

    private async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(request);
    }

    private async Task SendAsync(IRequest request)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(request);
    }

    private static string ResolveBaseConnectionString()
    {
        var environmentConnectionString = Environment.GetEnvironmentVariable("WOMBAT_TEST_CONNECTION");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        var secretsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Microsoft",
            "UserSecrets",
            WombatWebUserSecretsId,
            "secrets.json");

        if (File.Exists(secretsPath))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(secretsPath));
            if (document.RootElement.TryGetProperty("ConnectionStrings:DefaultConnection", out var property)
                && property.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(property.GetString()))
            {
                return property.GetString()!;
            }
        }

        return "Host=localhost;Port=5432;Database=wombat;Username=postgres;Password=postgres";
    }

    private sealed class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;
        private readonly string _respondUrl;

        public ApiFactory(string connectionString, string respondUrl)
        {
            _connectionString = connectionString;
            _respondUrl = respondUrl;
        }

        public CapturingEmailSender EmailSender { get; } = new();

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
            builder.UseSetting("Wombat:MsfRespondUrl", _respondUrl);
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEmailSender>();
                services.AddSingleton<IEmailSender>(EmailSender);

                // The scheduler finds every daily and hourly job due on a fresh schema, and its first tick fires them
                // unawaited while setup is still seeding: most schemas the old fixture leaked held eight job runs.
                // Nothing in this flow wants them. They would only write beside the test, and outlive the host into
                // the schema's drop. Single, not RemoveAll: if the registration changes shape, say so here.
                services.Remove(services.Single(descriptor =>
                    descriptor.ServiceType == typeof(IHostedService)
                    && descriptor.ImplementationType == typeof(ScheduledJobHost)));
            });
        }
    }

    public sealed class CapturingEmailSender : IEmailSender
    {
        public List<EmailMessage> Messages { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
