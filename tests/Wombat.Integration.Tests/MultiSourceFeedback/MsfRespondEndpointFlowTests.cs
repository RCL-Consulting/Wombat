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

namespace Wombat.Integration.Tests.MultiSourceFeedback;

public sealed class MsfRespondEndpointFlowTests : IAsyncLifetime
{
    private const string WombatWebUserSecretsId = "fd2ea5f4-1ee7-4c92-87f8-4f9dc5f6d0d7";

    private readonly string _schemaName = $"it_{Guid.NewGuid():N}";
    private ApiFactory _factory = null!;
    private HttpClient _client = null!;
    private string _baseConnectionString = null!;

    /// <summary>
    /// T121: the campaign now has to name EPAs from the subject's own curriculum, so the subject has to
    /// be a real admitted trainee rather than a bare user id.
    /// </summary>
    private int _institutionId;
    private int _coveredEpaId;
    private ClaimsPrincipal _coordinator = null!;

    private string SchemaConnectionString
    {
        get
        {
            var builder = new NpgsqlConnectionStringBuilder(_baseConnectionString)
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

        await using var connection = new NpgsqlConnection(_baseConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE SCHEMA IF NOT EXISTS \"{_schemaName}\"";
        await command.ExecuteNonQueryAsync();

        _factory = new ApiFactory(SchemaConnectionString, "http://localhost/msf/respond");
        _client = _factory.CreateClient();

        await AdmitSubjectAsync();
    }

    /// <summary>
    /// Admits "trainee-1" onto a seeded curriculum and builds the coordinator who runs their campaign.
    /// </summary>
    /// <remarks>
    /// Before T121 the subject was a bare string and nothing resolved it. The campaign now narrows its
    /// EPA coverage to the subject's curriculum, the create is refused outside the caller's institution,
    /// and the release fans out activities scope-stamped from the subject's profile - so all three need a
    /// real profile to exercise.
    /// </remarks>
    private async Task AdmitSubjectAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var curriculum = await dbContext.Curricula
            .Include(entity => entity.Items)
            .Where(entity => entity.Items.Any(item => item.OwningInstitutionId == null))
            .OrderBy(entity => entity.Id)
            .FirstAsync();

        _institutionId = await dbContext.Institutions.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();
        _coveredEpaId = curriculum.Items
            .Where(item => item.OwningInstitutionId == null)
            .OrderBy(item => item.Id)
            .Select(item => item.EpaId)
            .First();

        dbContext.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = "trainee-1",
            InstitutionId = _institutionId,
            CurriculumId = curriculum.Id,
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

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();

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
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            4,
            2,
            2,
            [_coveredEpaId],
            "coordinator-1",
            _coordinator));

        foreach (var invitee in CreateInvitees(campaign.Id))
        {
            await SendAsync(new AddMsfInvitationCommand(campaign.Id, invitee.Email, invitee.Category));
        }

        await SendAsync(new OpenMsfCampaignCommand(campaign.Id));

        _factory.EmailSender.Messages.Should().HaveCount(8);
        _factory.EmailSender.Messages.Should().OnlyContain(message => message.TextBody.Contains("http://localhost/msf/respond?token=", StringComparison.Ordinal));

        foreach (var message in _factory.EmailSender.Messages.Take(4))
        {
            var token = ExtractToken(message.TextBody);

            var formResponse = await _client.GetAsync($"/msf/respond?token={Uri.EscapeDataString(token)}");
            formResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var form = await formResponse.Content.ReadFromJsonAsync<MsfResponseFormDto>();
            form.Should().NotBeNull();
            form!.Questions.Should().HaveCount(2);

            var submitResponse = await _client.PostAsJsonAsync(
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

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await dbContext.MsfResponses.CountAsync()).Should().Be(4);
            (await dbContext.MsfInvitations.CountAsync(invitation => invitation.RespondedOn != null)).Should().Be(4);
        }

        var closedReport = await SendAsync(new CloseMsfCampaignCommand(campaign.Id));
        closedReport.TotalResponses.Should().Be(4);
        closedReport.State.Should().Be(MsfCampaignState.UnderReview);
        closedReport.Categories.Should().Contain(category => !category.IsSuppressed);
        closedReport.Categories.Should().OnlyContain(category =>
            category.Questions.All(question =>
                question.Comments.All(comment => !comment.Contains('@', StringComparison.Ordinal))));

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var invitations = await dbContext.MsfInvitations.OrderBy(invitation => invitation.Id).ToListAsync();
            invitations.Should().OnlyContain(invitation => invitation.RespondentEmail == null);
            invitations.Should().OnlyContain(invitation => !string.IsNullOrWhiteSpace(invitation.RespondentEmailHash));
        }

        await SendAsync(new ReleaseMsfCampaignCommand(
            campaign.Id, "coordinator-1", "Released after coordinator review.", 4, _coordinator));

        var traineeReports = await SendAsync(new ListMsfCampaignsForTraineeQuery("trainee-1"));
        traineeReports.Should().ContainSingle(report => report.Id == campaign.Id && report.ResponseCount == 4);

        var releasedReport = await SendAsync(new GetCampaignAggregateReportQuery(campaign.Id));
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

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var activity = await dbContext.Activities
                .Include(entity => entity.Transitions)
                .SingleAsync(entity => entity.Id == evidence.Id);

            activity.DataJson.Should().Contain($"\"epa_id\":{_coveredEpaId}");
            activity.DataJson.Should().Contain("\"overall_level\":4");
            activity.DataJson.Should().NotContain("@example.test");
            activity.ObservedOn.Should().Be(campaign.ClosesOn);

            // D8: MSF consumes none of Annexure A's 55 encounters, so `counts_for` is empty, the applier
            // is never reached and the stamp stays null - T108's "credit was never evaluated".
            var record = activity.Transitions.Single(entity => entity.TransitionKey == "record");
            record.CreditedItemCount.Should().BeNull();
            (await dbContext.CurriculumItemProgresses.CountAsync()).Should().Be(0);
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

    private static string ExtractToken(string body)
    {
        var match = Regex.Match(body, @"token=([^&\s]+)", RegexOptions.CultureInvariant);
        match.Success.Should().BeTrue("the email body should contain a responder token");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }

    private async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        return await sender.Send(request);
    }

    private async Task SendAsync(IRequest request)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
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
