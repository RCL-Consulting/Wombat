using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// T248: the campaign form's trainee picker (<see cref="ListMsfCampaignSubjectsQuery" />) offers exactly whom the create
/// command accepts, because both ask one predicate (<see cref="MsfCampaignRules.MayStartCampaignAbout" />) of a current
/// trainee (T238's rule); and creating a questionnaire template, or a campaign, takes the right to run campaigns
/// (<see cref="MsfCampaignRules.EnsureRunsCampaigns" />).
/// </summary>
/// <remarks>
/// <para>
/// <c>MsfCampaignSubjectsTests</c> (T238) checks the picker against the rule. These check it against the create command
/// itself, the real handler, for every kind of caller the campaign page can be reached by or a crafted request sent as:
/// an InstitutionalAdmin, a Coordinator at no institution, and an Administrator or a plain Trainee as well as the
/// coordinators.
/// </para>
/// <para>
/// Every refusal is followed by a save and a cleared tracker, then read back: the audit pipeline saves the request's
/// context from its catch, so anything a refused command had staged would be committed by its own refusal.
/// </para>
/// </remarks>
public sealed class MsfCampaignSubjectPickerTests
{
    private const int Host = 1;
    private const int Elsewhere = 2;

    /// <summary>In training at the host: an active profile there, and an account that holds Trainee.</summary>
    private const string CurrentAtHost = "current-host";

    /// <summary>In training at the other institution.</summary>
    private const string CurrentElsewhere = "current-elsewhere";

    /// <summary>Trained at the host, and is in training at the other institution now.</summary>
    private const string Moved = "moved";

    /// <summary>Completed at the host: a past profile only. The account still holds Trainee.</summary>
    private const string Completed = "completed";

    /// <summary>An active profile at the host whose user no longer holds Trainee.</summary>
    private const string RoleRemoved = "role-removed";

    /// <summary>An erased trainee's profile: an active profile at the host under a pseudonym that names no account.</summary>
    private const string Pseudonym = "erased-3f9a";

    /// <summary>An account that holds Trainee, with no profile.</summary>
    private const string NotAdmitted = "not-admitted";

    /// <summary>A coordinator at the host who is also in training there; their sign-in carries no Trainee claim.</summary>
    private const string CoordinatorInTraining = "coordinator-in-training";

    private static readonly string[] EveryCandidate =
        [CurrentAtHost, CurrentElsewhere, Moved, Completed, RoleRemoved, Pseudonym, NotAdmitted, CoordinatorInTraining, "nobody"];

    private readonly string _databaseName = Guid.NewGuid().ToString();

    public static TheoryData<string> Callers => new()
    {
        "Coordinator@Host", "Coordinator@Elsewhere", "Administrator", "CoordinatorInTraining", "Coordinator+Trainee",
        "Administrator+Trainee", "InstitutionalAdmin@Host", "Coordinator@Nowhere", "Trainee"
    };

    private static ClaimsPrincipal Caller(string caller) => caller switch
    {
        "Coordinator@Host" => TestPrincipals.Coordinator(Host),
        "Coordinator@Elsewhere" => TestPrincipals.Coordinator(Elsewhere),
        "Administrator" => TestPrincipals.Administrator(),
        "CoordinatorInTraining" => TestPrincipals.Coordinator(Host, userId: CoordinatorInTraining),
        "Coordinator+Trainee" => TestPrincipals.InRoles([WombatRoles.Coordinator, WombatRoles.Trainee], "registrar-1", Host),
        "Administrator+Trainee" => TestPrincipals.InRoles([WombatRoles.Administrator, WombatRoles.Trainee], "registrar-2", null),
        "InstitutionalAdmin@Host" => TestPrincipals.InstitutionalAdmin(Host),
        "Coordinator@Nowhere" => TestPrincipals.InRole(WombatRoles.Coordinator, "coordinator-nowhere", institutionId: null),
        "Trainee" => TestPrincipals.Trainee(CurrentAtHost, Host),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    // ─── The picker is the create ────────────────────────────────────────────

    /// <summary>
    /// For every caller, the picker holds exactly the candidates the create command accepts. Each candidate is created
    /// about in turn with the real handler.
    /// </summary>
    [Theory]
    [MemberData(nameof(Callers))]
    public async Task ThePicker_OffersExactlyWhomTheCreateAccepts(string caller)
    {
        await using var db = await SeedAsync();
        var principal = Caller(caller);

        var offered = (await PickerAsync(db, principal)).Select(subject => subject.UserId).ToArray();

        var accepted = new List<string>();
        foreach (var candidate in EveryCandidate)
        {
            var thrown = await Record.ExceptionAsync(() => CreateAsync(db, candidate, principal));
            if (thrown is null)
            {
                accepted.Add(candidate);
            }
            else
            {
                thrown.Should().BeOfType<UnauthorizedAccessException>($"{candidate} is refused by the scope gate, not later");
            }
        }

        offered.Should().BeEquivalentTo(accepted);
        offered.Should().OnlyHaveUniqueItems();
    }

    /// <summary>
    /// What each caller is offered, spelled out, so the parity above is not two empty lists agreeing. The host's
    /// coordinator is offered the trainees in training there, never one who has moved away, completed, lost Trainee or
    /// been erased; the other institution's coordinator the two in training there; an Administrator everyone in training;
    /// a coordinator in training everyone the host's coordinator is offered but themselves; and nobody else anyone.
    /// </summary>
    [Fact]
    public async Task EachCaller_IsOfferedTheCurrentTraineesTheyRunCampaignsFor()
    {
        await using var db = await SeedAsync();

        (await OfferedAsync(db, "Coordinator@Host")).Should().BeEquivalentTo([CoordinatorInTraining, CurrentAtHost]);
        (await OfferedAsync(db, "Coordinator@Elsewhere")).Should().BeEquivalentTo([CurrentElsewhere, Moved]);
        (await OfferedAsync(db, "Administrator")).Should().BeEquivalentTo([CoordinatorInTraining, CurrentAtHost, CurrentElsewhere, Moved]);
        (await OfferedAsync(db, "CoordinatorInTraining")).Should().Equal(CurrentAtHost);

        foreach (var nobody in new[] { "Coordinator+Trainee", "Administrator+Trainee", "InstitutionalAdmin@Host", "Coordinator@Nowhere", "Trainee" })
        {
            (await OfferedAsync(db, nobody)).Should().BeEmpty(nobody);
        }
    }

    /// <summary>
    /// The picker names each trainee by their account's name and email, read for just the offered trainees in one lookup
    /// (<see cref="IUserAdministrationService.GetContactsAsync" />, T248 review), and orders them by surname.
    /// </summary>
    [Fact]
    public async Task ThePicker_NamesEachTraineeFromTheirAccount_BySurname()
    {
        await using var db = await SeedAsync();

        var offered = await PickerAsync(db, Caller("Administrator"));

        offered.Select(subject => (subject.UserId, subject.FirstName, subject.LastName, subject.Email))
            .Should().Contain((CurrentAtHost, "Thandi", "Mokoena", $"{CurrentAtHost}@test"));
        offered.Select(subject => subject.LastName).Should().Equal("Buthelezi", "Dlamini", "Mokoena", "Naidoo");
    }

    // ─── The create's refusals ───────────────────────────────────────────────

    /// <summary>
    /// A caller who runs no campaign by their roles, an InstitutionalAdmin or a Coordinator with no institution, is told
    /// whom campaigns are run by, whatever subject they name (T248 review): the reason is their own roles, so it says
    /// nothing about the id. Until the review they were given the subject refusal, as if the trainee were the problem.
    /// </summary>
    [Theory]
    [InlineData("InstitutionalAdmin@Host")]
    [InlineData("Coordinator@Nowhere")]
    public async Task TheCreate_RefusesACallerWhoRunsNoCampaigns_ByTheirRoles_WhateverTheSubject_BeforeAnythingIsStored(string caller)
    {
        await using var db = await SeedAsync();

        foreach (var subject in new[] { CurrentAtHost, Completed, "nobody" })
        {
            var refusal = await Record.ExceptionAsync(() => CreateAsync(db, subject, Caller(caller)));
            refusal.Should().BeOfType<UnauthorizedAccessException>(subject)
                .Which.Message.Should().Be(MsfCampaignRules.RunsCampaignsRoles, subject);
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        await using var fresh = CreateDb();
        (await fresh.MsfCampaigns.AsNoTracking().CountAsync()).Should().Be(0);
    }

    // ─── Creating a template ─────────────────────────────────────────────────

    [Theory]
    [InlineData("Coordinator+Trainee")]
    [InlineData("Administrator+Trainee")]
    [InlineData("Trainee")]
    public async Task ACallerWhoHoldsTrainee_CannotCreateATemplate_AndNothingIsStored(string caller)
    {
        await using var db = CreateDb();

        var refusal = await Record.ExceptionAsync(() => CreateTemplateAsync(db, Caller(caller)));

        refusal.Should().BeOfType<UnauthorizedAccessException>()
            .Which.Message.Should().Be(MsfCampaignRules.TraineeRunsNoCampaigns);
        await NoTemplateIsStoredAfterTheAuditSaveAsync(db);
    }

    [Theory]
    [InlineData("InstitutionalAdmin@Host")]
    [InlineData("Coordinator@Nowhere")]
    public async Task ACallerWhoRunsNoCampaigns_CannotCreateATemplate_AndNothingIsStored(string caller)
    {
        await using var db = CreateDb();

        var refusal = await Record.ExceptionAsync(() => CreateTemplateAsync(db, Caller(caller)));

        refusal.Should().BeOfType<UnauthorizedAccessException>()
            .Which.Message.Should().Be(MsfCampaignRules.RunsCampaignsRoles);
        await NoTemplateIsStoredAfterTheAuditSaveAsync(db);
    }

    [Theory]
    [InlineData("Coordinator@Host")]
    [InlineData("Administrator")]
    public async Task ACallerWhoRunsCampaigns_CreatesATemplate(string caller)
    {
        await using var db = CreateDb();

        var created = await CreateTemplateAsync(db, Caller(caller));

        await using var fresh = CreateDb();
        (await fresh.Set<MsfTemplate>().AsNoTracking().SingleAsync()).Id.Should().Be(created.Id);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>
    /// The refused command's context is saved, as the audit pipeline saves it from its catch, and its tracker cleared;
    /// then the store is read from that context and from a fresh one.
    /// </summary>
    private async Task NoTemplateIsStoredAfterTheAuditSaveAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        (await db.Set<MsfTemplate>().AsNoTracking().CountAsync()).Should().Be(0);

        await using var fresh = CreateDb();
        (await fresh.Set<MsfTemplate>().AsNoTracking().CountAsync()).Should().Be(0);
        (await fresh.Set<MsfQuestion>().AsNoTracking().CountAsync()).Should().Be(0);
    }

    private static Task<MsfTemplateDto> CreateTemplateAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => new CreateMsfTemplateCommandHandler(db).Handle(
            new CreateMsfTemplateCommand(
                "Default MSF",
                null,
                false,
                [new CreateMsfTemplateQuestionItem("Overall", MsfQuestionType.Scale, null, true)],
                principal),
            CancellationToken.None);

    private async Task<string[]> OfferedAsync(ApplicationDbContext db, string caller)
        => (await PickerAsync(db, Caller(caller))).Select(subject => subject.UserId).ToArray();

    private static Task<IReadOnlyList<TraineeProfileDto>> PickerAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => new ListMsfCampaignSubjectsQueryHandler(db, Accounts())
            .Handle(new ListMsfCampaignSubjectsQuery(principal), CancellationToken.None);

    private static Task<MsfCampaignSummaryDto> CreateAsync(ApplicationDbContext db, string subjectUserId, ClaimsPrincipal principal)
        => new CreateMsfCampaignCommandHandler(db, new ActivityReferenceDataService(db), Accounts()).Handle(
            new CreateMsfCampaignCommand(
                subjectUserId,
                TemplateId,
                new DateOnly(2026, 10, 1),
                new DateOnly(2026, 10, 15),
                MinimumResponses: 1,
                MinimumCategoryResponses: 1,
                MinimumRespondentCategories: 1,
                EpaIds: [],
                CreatedByUserId: principal.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                principal),
            CancellationToken.None);

    /// <summary>
    /// The accounts: everyone with a profile but the pseudonym, and the trainee with none. Each holds Trainee but the one
    /// whose role was removed, who is an Assessor now.
    /// </summary>
    private static FakeUserDirectory Accounts()
        => new FakeUserDirectory()
            .With(Account(CoordinatorInTraining, "Ayanda", "Buthelezi", Host, WombatRoles.Trainee, WombatRoles.Coordinator))
            .With(Account(CurrentAtHost, "Thandi", "Mokoena", Host, WombatRoles.Trainee))
            .With(Account(CurrentElsewhere, "Sipho", "Dlamini", Elsewhere, WombatRoles.Trainee))
            .With(Account(Moved, "Lerato", "Naidoo", Elsewhere, WombatRoles.Trainee))
            .With(Account(Completed, "Pieter", "Botha", Host, WombatRoles.Trainee))
            .With(Account(RoleRemoved, "Nomsa", "Zulu", Host, WombatRoles.Assessor))
            .With(Account(NotAdmitted, "Kagiso", "Molefe", Host, WombatRoles.Trainee));

    private static UserIdentityDetails Account(string userId, string firstName, string lastName, int institutionId, params string[] roles)
        => new(userId, $"{userId}@test", firstName, lastName, institutionId, [], [], roles);

    private const int TemplateId = 40;
    private const int CurriculumId = 100;

    private async Task<ApplicationDbContext> SeedAsync()
    {
        var db = CreateDb();

        db.Specialities.Add(new Speciality { Id = 10, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 11, SpecialityId = 10, Name = "General Paediatrics", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = CurriculumId, SubSpecialityId = 11, Name = "Paediatric EPA Curriculum", Version = "11.1" });

        db.Set<TraineeProfile>().AddRange(
            Profile(CurrentAtHost, Host, isActive: true),
            Profile(CurrentElsewhere, Elsewhere, isActive: true),
            Profile(Moved, Host, isActive: false),
            Profile(Moved, Elsewhere, isActive: true),
            Profile(Completed, Host, isActive: false),
            Profile(RoleRemoved, Host, isActive: true),
            Profile(Pseudonym, Host, isActive: true),
            Profile(CoordinatorInTraining, Host, isActive: true));

        db.Set<MsfTemplate>().Add(new MsfTemplate
        {
            Id = TemplateId,
            Name = "Annual MSF",
            Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
        });

        await db.SaveChangesAsync();
        return db;
    }

    private static TraineeProfile Profile(string userId, int institutionId, bool isActive)
        => new()
        {
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = isActive
        };

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);
}
