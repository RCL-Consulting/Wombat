using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.MultiSourceFeedback;
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
/// T238: a multi-source feedback campaign is started only about a current trainee (an active profile, on an account that
/// exists and still holds Trainee), and the campaign form's trainee picker offers exactly the trainees the create accepts.
/// </summary>
/// <remarks>
/// <para>
/// Until T238 the form listed every trainee profile at the caller's institution through the admin trainees list, which
/// shows completed and withdrawn profiles by design, and the create accepted any trainee whose latest profile was at the
/// caller's institution. So the form offered graduates and trainees who had withdrawn, and a crafted create named an
/// erased trainee's pseudonym, whose profile <c>ErasureExecutor</c> leaves active under an id no account holds.
/// </para>
/// <para>
/// Every refusal is followed by the save the audit pipeline makes from its catch, and the campaigns are counted through a
/// second context, so a campaign staged before the refusal could not hide.
/// </para>
/// </remarks>
public sealed class MsfCampaignSubjectsTests
{
    private const int Host = 1;
    private const int Other = 2;

    /// <summary>An active profile at the host, on an account that holds Trainee.</summary>
    private const string CurrentAtHost = "current-host";

    /// <summary>An active profile at the other institution.</summary>
    private const string CurrentElsewhere = "current-other";

    /// <summary>An erased trainee: their active profile at the host was rewritten to a pseudonym no account holds.</summary>
    private const string ErasedAtHost = "deleted_user_9f8e7d6c";

    /// <summary>An active profile at the host whose account no longer holds Trainee: it outlived its trainee.</summary>
    private const string NoLongerTraineeAtHost = "former-host";

    /// <summary>A trainee whose programme at the host has ended; they still hold Trainee, as a withdrawal leaves it.</summary>
    private const string LeftHost = "left-host";

    /// <summary>A trainee who moved: an ended profile at the host and a running one at the other institution.</summary>
    private const string MovedToOther = "moved-other";

    /// <summary>A trainee with an ended profile and a running one, both at the host: offered once, not twice.</summary>
    private const string RestartedAtHost = "restarted-host";

    private static readonly string[] EveryId =
    [
        CurrentAtHost, CurrentElsewhere, ErasedAtHost, NoLongerTraineeAtHost, LeftHost, MovedToOther, RestartedAtHost,
        "nobody-by-this-id"
    ];

    private const string NotRunByCaller =
        "A multi-source feedback campaign can only be run for a trainee in a programme at your own institution.";

    private const string NotCurrentTrainee =
        "A multi-source feedback campaign can only be run for a trainee in a programme now: someone whose trainee " +
        "profile is active and who still holds the Trainee role.";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    public static TheoryData<string> Callers => new()
    {
        "Coordinator of the host", "Coordinator elsewhere", "Administrator", "The subject, as Coordinator",
        "Coordinator who holds Trainee"
    };

    [Theory]
    [MemberData(nameof(Callers))]
    public async Task ThePicker_OffersExactlyTheTraineesTheCreateAccepts(string caller)
    {
        await using var db = await SeededDbAsync();
        var principal = Caller(caller);

        var offered = (await PickerAsync(db, principal)).Select(trainee => trainee.UserId).ToArray();

        offered.Should().OnlyHaveUniqueItems("a current trainee has one active profile");
        foreach (var userId in EveryId)
        {
            var accepted = await MsfCampaignRules.MayStartCampaignAboutAsync(
                db, Directory(), principal, userId, CancellationToken.None);

            offered.Contains(userId).Should().Be(accepted, $"{caller}, {userId}");
        }
    }

    [Fact]
    public async Task ThePicker_OffersTheHostsCurrentTrainees_OnceEach_AndNoneWhoseProfileOutlivedThem()
    {
        await using var db = await SeededDbAsync();

        var offered = await PickerAsync(db, TestPrincipals.Coordinator(Host));

        offered.Select(trainee => trainee.UserId).Should().BeEquivalentTo([CurrentAtHost, RestartedAtHost]);
        offered.Should().OnlyContain(trainee => trainee.IsActive, "each is offered on their running profile");
        offered.Single(trainee => trainee.UserId == CurrentAtHost).Email.Should().Be($"{CurrentAtHost}@test");

        (await PickerAsync(db, TestPrincipals.Coordinator(Other))).Select(trainee => trainee.UserId)
            .Should().BeEquivalentTo([CurrentElsewhere, MovedToOther]);
        (await PickerAsync(db, TestPrincipals.Administrator())).Select(trainee => trainee.UserId)
            .Should().BeEquivalentTo([CurrentAtHost, CurrentElsewhere, MovedToOther, RestartedAtHost]);
    }

    public static TheoryData<string, string> TraineesWhoAreNotCurrent()
    {
        var cases = new TheoryData<string, string>();
        foreach (var caller in new[] { "Coordinator of the host", "Administrator" })
        {
            foreach (var trainee in new[] { ErasedAtHost, NoLongerTraineeAtHost, LeftHost, "nobody-by-this-id" })
            {
                cases.Add(caller, trainee);
            }
        }

        return cases;
    }

    [Theory]
    [MemberData(nameof(TraineesWhoAreNotCurrent))]
    public async Task ACampaignAboutSomeoneWhoIsNotACurrentTrainee_IsRefusedAtCreate_BeforeAnyWrite(string caller, string trainee)
    {
        await using var db = await SeededDbAsync();
        var principal = Caller(caller);

        var refusal = await Record.ExceptionAsync(() => CreateAsync(db, principal, trainee));

        // Anyone but an Administrator gets the one refusal a trainee elsewhere gets, which confirms nothing about the id;
        // an Administrator runs every institution's campaigns, and is told what is wrong.
        refusal.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(
            caller == "Administrator" ? NotCurrentTrainee : NotRunByCaller);

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        (await CampaignCountAsync()).Should().Be(0, "the refusal comes before anything is written");

        (await PickerAsync(db, principal)).Select(offered => offered.UserId).Should().NotContain(trainee);

        // The control: the same caller starts a campaign about a current trainee at the host.
        (await CreateAsync(db, principal, CurrentAtHost)).SubjectUserId.Should().Be(CurrentAtHost);
        (await CampaignCountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ACoordinator_IsRefusedATraineeElsewhere_InTheSameWordsAsOneWhoIsNotCurrent()
    {
        await using var db = await SeededDbAsync();

        foreach (var trainee in new[] { CurrentElsewhere, MovedToOther })
        {
            var refusal = await Record.ExceptionAsync(() => CreateAsync(db, TestPrincipals.Coordinator(Host), trainee));

            refusal.Should().BeOfType<UnauthorizedAccessException>()
                .Which.Message.Should().Be(NotRunByCaller, trainee);
        }

        await db.SaveChangesAsync();
        (await CampaignCountAsync()).Should().Be(0);
        (await CreateAsync(db, TestPrincipals.Coordinator(Other), MovedToOther)).SubjectUserId.Should().Be(MovedToOther);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static ClaimsPrincipal Caller(string caller) => caller switch
    {
        "Coordinator of the host" => TestPrincipals.Coordinator(Host),
        "Coordinator elsewhere" => TestPrincipals.Coordinator(Other),
        "Administrator" => TestPrincipals.Administrator(),
        "The subject, as Coordinator" => TestPrincipals.Coordinator(Host, userId: CurrentAtHost),
        "Coordinator who holds Trainee" => TestPrincipals.InRoles(
            [WombatRoles.Coordinator, WombatRoles.Trainee], RestartedAtHost, Host),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    /// <summary>
    /// Every trainee's account but the erased one's, which no account holds; the one whose profile outlived the role holds
    /// Assessor only.
    /// </summary>
    private static FakeUserDirectory Directory()
        => FakeUserDirectory
            .Trainees(CurrentAtHost, CurrentElsewhere, LeftHost, MovedToOther, RestartedAtHost)
            .With(new UserIdentityDetails(
                NoLongerTraineeAtHost, $"{NoLongerTraineeAtHost}@test", "Former", "Trainee", Host, [], [],
                [WombatRoles.Assessor]));

    private static Task<IReadOnlyList<Wombat.Application.Features.Trainees.TraineeProfileDto>> PickerAsync(
        ApplicationDbContext db, ClaimsPrincipal principal)
        => new ListMsfCampaignSubjectsQueryHandler(db, Directory())
            .Handle(new ListMsfCampaignSubjectsQuery(principal), CancellationToken.None);

    private static async Task<MsfCampaignSummaryDto> CreateAsync(ApplicationDbContext db, ClaimsPrincipal principal, string subject)
        => await new CreateMsfCampaignCommandHandler(db, new ActivityReferenceDataService(db), Directory()).Handle(
            new CreateMsfCampaignCommand(
                subject,
                TemplateId,
                new DateOnly(2026, 9, 1),
                new DateOnly(2026, 9, 21),
                MinimumResponses: 1,
                MinimumCategoryResponses: 1,
                MinimumRespondentCategories: 1,
                EpaIds: [],
                CreatedByUserId: principal.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                principal),
            CancellationToken.None);

    /// <summary>Every campaign stored, read through a second context so nothing tracked can mask a refused create.</summary>
    private async Task<int> CampaignCountAsync()
    {
        await using var db = CreateDb();
        return await db.MsfCampaigns.AsNoTracking().CountAsync();
    }

    private const int TemplateId = 40;

    private async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = CreateDb();

        db.Institutions.AddRange(
            new Institution { Id = Host, Name = "Host", ShortCode = "H", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = Other, Name = "Other", ShortCode = "O", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.Add(new Speciality { Id = 10, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 11, SpecialityId = 10, Name = "General Paediatrics", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = 11, Name = "Paediatric EPA Curriculum", Version = "11.1" });
        db.Set<MsfTemplate>().Add(new MsfTemplate
        {
            Id = TemplateId,
            Name = "Annual MSF",
            Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
        });

        AddProfile(db, 1, CurrentAtHost, Host, isActive: true);
        AddProfile(db, 2, CurrentElsewhere, Other, isActive: true);
        AddProfile(db, 3, ErasedAtHost, Host, isActive: true);
        AddProfile(db, 4, NoLongerTraineeAtHost, Host, isActive: true);
        AddProfile(db, 5, LeftHost, Host, isActive: false);
        AddProfile(db, 6, MovedToOther, Host, isActive: false);
        AddProfile(db, 7, MovedToOther, Other, isActive: true);
        AddProfile(db, 8, RestartedAtHost, Host, isActive: false);
        AddProfile(db, 9, RestartedAtHost, Host, isActive: true);

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private static void AddProfile(ApplicationDbContext db, int id, string userId, int institutionId, bool isActive)
        => db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = id,
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = 100,
            ProgrammeStartDate = new DateOnly(2024, 1, 1),
            ExpectedCompletionDate = new DateOnly(2028, 1, 1),
            IsActive = isActive
        });

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);
}
