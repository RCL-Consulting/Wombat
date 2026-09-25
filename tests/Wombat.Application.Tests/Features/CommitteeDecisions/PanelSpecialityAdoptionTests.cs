using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// A Speciality-scoped panel is offered and created only for a speciality its institution has adopted, the rule the
/// institutional administrator's speciality list already follows. (T245)
/// </summary>
/// <remarks>
/// <para>
/// Before T245 a speciality administrator's form offered every speciality their claims named, and create accepted each:
/// a SpecialityAdmin at A whose claims named General Medicine, which A has not adopted, created a General Medicine panel
/// at A, which could never have a trainee. An institutional administrator's form offered only the adopted specialities
/// (<c>GetSpecialitiesListQuery</c>, T092), but create accepted any speciality from them too, and a global Administrator
/// was offered, and could create, any speciality at any institution. All now read one predicate,
/// <c>AdoptedSpecialities.IdsAt</c>, at the panel's own institution (the Administrator since the T245 review).
/// </para>
/// <para>
/// Every refused command is followed by a save and a cleared change tracker, as the audit pipeline does from its catch,
/// and the store is read back through a second context.
/// </para>
/// </remarks>
public sealed class PanelSpecialityAdoptionTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;

    private const int Paediatrics = 4;
    private const int Surgery = 5;
    private const int GeneralMedicine = 6;

    private const int GeneralPaediatrics = 11;
    private const int Neonatology = 12;
    private const int GeneralSurgery = 13;
    private const int GeneralInternalMedicine = 14;

    private const string Neonatal = "neonatal";

    private const string NotAdopted =
        "Your institution has adopted no curriculum in this speciality, so a panel for it would have no trainee to review.";

    private const string NotAdoptedAtChosenInstitution =
        "The institution you chose has adopted no curriculum in this speciality, so a panel for it would have no trainee " +
        "to review.";

    private static readonly FakeUserDirectory Seats = FakeUserDirectory
        .CommitteeMembersAt(InstitutionA, "chair", "member")
        .WithCommitteeMembers(InstitutionB, "chair", "member");

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ─── Create ──────────────────────────────────────────────────────────────

    public static TheoryData<string, int> SpecialitiesNotAdoptedAtA => new()
    {
        // The symptom (T194 step 5): a speciality administrator whose claims name a speciality A does not train.
        { "SpecialityAdmin of General Medicine", GeneralMedicine },
        { "SpecialityAdmin of Paediatrics and General Medicine", GeneralMedicine },
        { "SubSpecialityAdmin of General Internal Medicine", GeneralMedicine },
        // An adoption only elsewhere is not A's: B trains General Medicine.
        { "InstitutionalAdmin", GeneralMedicine },
        // An adoption A has let lapse is not one it has: A's General Surgery adoption is inactive.
        { "SpecialityAdmin of Surgery", Surgery },
        { "InstitutionalAdmin", Surgery }
    };

    [Theory]
    [MemberData(nameof(SpecialitiesNotAdoptedAtA))]
    public async Task ASpecialityPanel_ForASpecialityTheInstitutionHasNotAdopted_IsRefused_AndNothingIsWritten(
        string caller, int specialityId)
    {
        await using var db = await SeededDbAsync();

        var act = () => CreateAsync(db, Principal(caller), specialityId);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(NotAdopted);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredPanelsAsync()).Should().BeEmpty();
    }

    public static TheoryData<string> CallersWhoseSpecialityAIsAdopted => new()
    {
        "SpecialityAdmin of Paediatrics",
        "SpecialityAdmin of Paediatrics and General Medicine",
        "SubSpecialityAdmin of General Paediatrics",
        // Adopted is the speciality's, as the institutional administrator's list reads it: A adopted General Paediatrics,
        // not Neonatology, and a Neonatology administrator's panel covers Paediatrics, which A trains.
        "SubSpecialityAdmin of Neonatology",
        "InstitutionalAdmin"
    };

    [Theory]
    [MemberData(nameof(CallersWhoseSpecialityAIsAdopted))]
    public async Task ASpecialityPanel_ForASpecialityTheInstitutionHasAdopted_IsCreated(string caller)
    {
        await using var db = await SeededDbAsync();

        var panel = await CreateAsync(db, Principal(caller), Paediatrics);

        panel.InstitutionId.Should().Be(InstitutionA);
        (await StoredPanelsAsync()).Should().Equal((InstitutionA, (int?)Paediatrics));
    }

    [Fact]
    public async Task ASpecialityOutsideTheCallersReach_IsRefusedAsOutOfScope_BeforeItsAdoptionIsAsked()
    {
        // Paediatrics is adopted at A, but it is not a Surgery administrator's: the scope refusal, as before T245, which
        // says nothing about what A has adopted.
        await using var db = await SeededDbAsync();

        var act = () => CreateAsync(db, Principal("SpecialityAdmin of Surgery"), Paediatrics);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().Be("You can only manage panels in your institution.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredPanelsAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task WhoMayAct_IsSaidBeforeWhatTheInstitutionHasAdopted()
    {
        // A speciality administrator naming a College committee is told, as before T245, that the committee is not theirs
        // to name. An institutional administrator, who may name one, is told the speciality is not adopted, before the
        // committee's slot or the members are read.
        await using var db = await SeededDbAsync();
        db.DecisionBodies.Add(new DecisionBody { Key = Neonatal, Name = "Neonatal team Clinical Competency Committee" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var speciality = await RefusalAsync(() => CreateAsync(db, Principal("SpecialityAdmin of General Medicine"), GeneralMedicine, Neonatal));
        var institutional = await RefusalAsync(() => CreateAsync(db, Principal("InstitutionalAdmin"), GeneralMedicine, Neonatal));

        speciality.Should().BeOfType<UnauthorizedAccessException>()
            .Which.Message.Should().Be("Only an institutional administrator can say which College committee a panel sits as.");
        institutional.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be(NotAdopted);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredPanelsAsync()).Should().BeEmpty();
    }

    public static TheoryData<int, int> SpecialitiesAnInstitutionHasNotAdopted => new()
    {
        // T245 review: an Administrator names the institution, and is held to what it has adopted. B trains only General
        // Medicine; A has let Surgery lapse and never trained General Medicine.
        { InstitutionA, GeneralMedicine },
        { InstitutionA, Surgery },
        { InstitutionB, Paediatrics },
        { InstitutionB, Surgery }
    };

    [Theory]
    [MemberData(nameof(SpecialitiesAnInstitutionHasNotAdopted))]
    public async Task AnAdministratorsSpecialityPanel_ForASpecialityTheChosenInstitutionHasNotAdopted_IsRefused_AndNothingIsWritten(
        int institutionId, int specialityId)
    {
        await using var db = await SeededDbAsync();

        var act = () => CreateAsync(db, TestPrincipals.Administrator(), specialityId, institutionId: institutionId);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(NotAdoptedAtChosenInstitution);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredPanelsAsync()).Should().BeEmpty();
    }

    [Theory]
    [InlineData(InstitutionA, Paediatrics)]
    [InlineData(InstitutionB, GeneralMedicine)]
    public async Task AnAdministratorsSpecialityPanel_ForASpecialityTheChosenInstitutionHasAdopted_IsCreated(
        int institutionId, int specialityId)
    {
        // The adoptions read are the chosen institution's, not the caller's: an Administrator has none of their own.
        await using var db = await SeededDbAsync();

        await CreateAsync(db, TestPrincipals.Administrator(), specialityId, institutionId: institutionId);

        (await StoredPanelsAsync()).Should().Equal((institutionId, (int?)specialityId));
    }

    [Fact]
    public async Task AnInstitutionWidePanel_AsksNoAdoption()
    {
        // It covers no speciality. B trains General Medicine only, and A's institutional administrator's panel at A is
        // accepted whatever A has adopted.
        await using var db = await SeededDbAsync();

        await new CreateDecisionPanelCommandHandler(db, Seats).Handle(
            new CreateDecisionPanelCommand(
                "Hospital CCC",
                DecisionPanelScope.Institution,
                InstitutionA,
                SpecialityId: null,
                Members(),
                Principal("InstitutionalAdmin")),
            CancellationToken.None);

        (await StoredPanelsAsync()).Should().Equal((InstitutionA, (int?)null));
    }

    // ─── The form's offer ────────────────────────────────────────────────────

    public static TheoryData<string, int[]> Offers => new()
    {
        { "SpecialityAdmin of Paediatrics", [Paediatrics] },
        { "SpecialityAdmin of Paediatrics and General Medicine", [Paediatrics] },
        { "SubSpecialityAdmin of Neonatology", [Paediatrics] },
        { "SpecialityAdmin of General Medicine", [] },
        { "SubSpecialityAdmin of General Internal Medicine", [] },
        { "SpecialityAdmin of Surgery", [] },
        // The adopted specialities, from the one predicate its speciality list reads, named by the offer itself now.
        { "InstitutionalAdmin", [Paediatrics] }
    };

    [Theory]
    [MemberData(nameof(Offers))]
    public async Task TheFormOffers_ExactlyTheSpecialitiesCreateAccepts(string caller, int[] offered)
    {
        await using var db = await SeededDbAsync();

        var options = await OptionsAsync(db, Principal(caller));

        options.Specialities.Should().NotBeNull();
        options.Specialities!.Select(speciality => speciality.Id).Should().Equal(offered);
        foreach (var specialityId in new[] { Paediatrics, Surgery, GeneralMedicine })
        {
            var accepted = await AcceptsAsync(Principal(caller), specialityId);
            accepted.Should().Be(offered.Contains(specialityId), $"speciality {specialityId} is offered exactly when it is accepted");
        }
    }

    [Theory]
    [InlineData("SpecialityAdmin of General Medicine")]
    [InlineData("SubSpecialityAdmin of General Internal Medicine")]
    public async Task ASpecialityAdmin_WhoseSpecialitiesTheInstitutionHasNotAdopted_MayCreateNoPanel(string caller)
    {
        // The form then shows no form but says why (#panel-none-creatable), and the list offers no New panel.
        await using var db = await SeededDbAsync();

        var options = await OptionsAsync(db, Principal(caller));

        options.MayCreateInstitutionWide.Should().BeFalse();
        options.MayCreateAny.Should().BeFalse();
    }

    [Fact]
    public async Task TheInstitutionalAdminsOffer_IsTheirSpecialityList()
    {
        // One predicate: the speciality list the form used to read for them, and the offer that now names it.
        await using var db = await SeededDbAsync();
        var principal = Principal("InstitutionalAdmin");

        var options = await OptionsAsync(db, principal);
        var list = await new GetSpecialitiesListQueryHandler(db).Handle(new GetSpecialitiesListQuery(principal), CancellationToken.None);

        options.Specialities!.Select(speciality => speciality.Id).Should().Equal(list.Select(speciality => speciality.Id));
    }

    [Fact]
    public async Task AnAdministratorsOffer_WaitsForTheInstitution()
    {
        // An Administrator names the institution of every panel, so until they do no speciality is named (null), and both
        // scopes are theirs: the Speciality scope wherever a speciality has been adopted.
        await using var db = await SeededDbAsync();

        var options = await OptionsAsync(db, TestPrincipals.Administrator());

        options.Specialities.Should().BeNull();
        options.MayCreateInstitutionWide.Should().BeTrue();
        options.MayCreateAny.Should().BeTrue();
    }

    public static TheoryData<int, int[]> AdministratorOffers => new()
    {
        { InstitutionA, [Paediatrics] },
        { InstitutionB, [GeneralMedicine] }
    };

    [Theory]
    [MemberData(nameof(AdministratorOffers))]
    public async Task AnAdministratorsOffer_AtTheChosenInstitution_IsExactlyTheSpecialitiesCreateAcceptsThere(
        int institutionId, int[] offered)
    {
        // T245 review. Until then an Administrator was offered every speciality, whichever institution they chose, and
        // create accepted each: a General Medicine panel at A, which trains none.
        await using var db = await SeededDbAsync();

        var options = await OptionsAsync(db, TestPrincipals.Administrator(), institutionId);

        options.MayCreateInstitutionWide.Should().BeTrue();
        options.Specialities.Should().NotBeNull();
        options.Specialities!.Select(speciality => speciality.Id).Should().Equal(offered);
        foreach (var specialityId in new[] { Paediatrics, Surgery, GeneralMedicine })
        {
            var accepted = await AcceptsAsync(TestPrincipals.Administrator(), specialityId, institutionId);
            accepted.Should().Be(offered.Contains(specialityId), $"speciality {specialityId} is offered exactly when it is accepted");
        }
    }

    [Theory]
    [InlineData("InstitutionalAdmin")]
    [InlineData("SpecialityAdmin of Paediatrics and General Medicine")]
    public async Task AnInstitutionNamedByAnyoneButAnAdministrator_ChangesNothingTheyAreOffered(string caller)
    {
        // Their panels run at their own institution, A: naming B, which trains General Medicine, does not offer it.
        await using var db = await SeededDbAsync();

        var options = await OptionsAsync(db, Principal(caller), InstitutionB);

        options.Specialities!.Select(speciality => speciality.Id).Should().Equal(Paediatrics);
    }

    // ─── An existing panel ───────────────────────────────────────────────────

    [Fact]
    public async Task APanelWhoseSpecialityIsNoLongerAdopted_StaysOpenToItsAdministrators()
    {
        // A panel outlives an adoption: the trainees admitted under it still follow it (committee routing reads their
        // curricula beside the adopted ones), and the panel that reviews them keeps its administrators. Only a new panel
        // is asked for an adoption; update carries no speciality to ask about.
        await using var db = await SeededDbAsync();
        var panel = new DecisionPanel
        {
            Name = "Surgery CCC",
            Scope = DecisionPanelScope.Speciality,
            InstitutionId = InstitutionA,
            SpecialityId = Surgery,
            CreatedOn = DateTime.UtcNow,
            Members = [new DecisionPanelMember { UserId = "chair", Role = DecisionPanelMemberRole.Chair }]
        };
        db.DecisionPanels.Add(panel);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var principal = Principal("SpecialityAdmin of Surgery");
        (await new GetDecisionPanelByIdQueryHandler(db).Handle(new GetDecisionPanelByIdQuery(panel.Id, principal), CancellationToken.None))
            .Should().NotBeNull();

        await new UpdateDecisionPanelCommandHandler(db, Seats).Handle(
            new UpdateDecisionPanelCommand(panel.Id, Members(), principal), CancellationToken.None);

        await using var read = CreateDb();
        (await read.DecisionPanelMembers.Where(member => member.PanelId == panel.Id).CountAsync()).Should().Be(2);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static IReadOnlyList<DecisionPanelMemberInput> Members() =>
    [
        new DecisionPanelMemberInput("chair", DecisionPanelMemberRole.Chair),
        new DecisionPanelMemberInput("member", DecisionPanelMemberRole.Member)
    ];

    /// <param name="institutionId">The institution named on the command: an Administrator's choice; null for anyone else.</param>
    private static async Task<DecisionPanelDetailDto> CreateAsync(
        ApplicationDbContext db,
        ClaimsPrincipal principal,
        int specialityId,
        string? decisionBodyKey = null,
        int? institutionId = null)
        => await new CreateDecisionPanelCommandHandler(db, Seats).Handle(
            new CreateDecisionPanelCommand(
                "Programme CCC",
                DecisionPanelScope.Speciality,
                institutionId,
                specialityId,
                Members(),
                principal,
                decisionBodyKey),
            CancellationToken.None);

    private static async Task<Exception> RefusalAsync(Func<Task> act)
    {
        try
        {
            await act();
        }
        catch (Exception exception)
        {
            return exception;
        }

        throw new InvalidOperationException("The request was expected to be refused.");
    }

    /// <summary>Whether create accepts this speciality from this caller, each on a fresh store so one does not see another.</summary>
    private static async Task<bool> AcceptsAsync(ClaimsPrincipal principal, int specialityId, int? institutionId = null)
    {
        var tests = new PanelSpecialityAdoptionTests();
        await using var db = await tests.SeededDbAsync();
        try
        {
            await CreateAsync(db, principal, specialityId, institutionId: institutionId);
            return true;
        }
        catch (Exception exception) when (exception is InvalidOperationException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static async Task<DecisionPanelFormOptionsDto> OptionsAsync(
        ApplicationDbContext db, ClaimsPrincipal principal, int? institutionId = null)
        => await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
            new GetDecisionPanelFormOptionsQuery(principal, institutionId), CancellationToken.None);

    private static ClaimsPrincipal Principal(string caller) => caller switch
    {
        "InstitutionalAdmin" => TestPrincipals.InstitutionalAdmin(InstitutionA),
        "SpecialityAdmin of Paediatrics" => TestPrincipals.InRole(
            WombatRoles.SpecialityAdmin, "paeds-admin", InstitutionA, specialityId: Paediatrics),
        "SpecialityAdmin of General Medicine" => TestPrincipals.InRole(
            WombatRoles.SpecialityAdmin, "medicine-admin", InstitutionA, specialityId: GeneralMedicine),
        "SpecialityAdmin of Surgery" => TestPrincipals.InRole(
            WombatRoles.SpecialityAdmin, "surgery-admin", InstitutionA, specialityId: Surgery),
        "SpecialityAdmin of Paediatrics and General Medicine" => WithSpeciality(
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "two-speciality-admin", InstitutionA, specialityId: Paediatrics),
            GeneralMedicine),
        "SubSpecialityAdmin of General Paediatrics" => TestPrincipals.InRole(
            WombatRoles.SubSpecialityAdmin, "gen-paeds-admin", InstitutionA, subSpecialityId: GeneralPaediatrics),
        "SubSpecialityAdmin of Neonatology" => TestPrincipals.InRole(
            WombatRoles.SubSpecialityAdmin, "neonatology-admin", InstitutionA, subSpecialityId: Neonatology),
        "SubSpecialityAdmin of General Internal Medicine" => TestPrincipals.InRole(
            WombatRoles.SubSpecialityAdmin, "gim-admin", InstitutionA, subSpecialityId: GeneralInternalMedicine),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    /// <summary>A second speciality claim on the same sign-in: a user's claims may name several.</summary>
    private static ClaimsPrincipal WithSpeciality(ClaimsPrincipal principal, int specialityId)
    {
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(WombatClaimTypes.SpecialityId, specialityId.ToString()));
        return principal;
    }

    private async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = CreateDb();
        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Colleges.Add(new College { Id = 1, Name = "CMSA", ShortCode = "CMSA", IsActive = true });
        db.Specialities.AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery", IsActive = true },
            new Speciality { Id = GeneralMedicine, CollegeId = 1, Name = "General Medicine", IsActive = true });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true },
            new SubSpeciality { Id = Neonatology, SpecialityId = Paediatrics, Name = "Neonatology", IsActive = true },
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery", IsActive = true },
            new SubSpeciality { Id = GeneralInternalMedicine, SpecialityId = GeneralMedicine, Name = "General Internal Medicine", IsActive = true });
        db.Curricula.AddRange(
            new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" },
            new Curriculum { Id = 200, SubSpecialityId = GeneralSurgery, Name = "General Surgery", Version = "1" },
            new Curriculum { Id = 300, SubSpecialityId = GeneralInternalMedicine, Name = "General Internal Medicine", Version = "1" });
        db.Set<InstitutionCurriculumAdoption>().AddRange(
            // A trains General Paediatrics.
            new InstitutionCurriculumAdoption
            {
                Id = 1, InstitutionId = InstitutionA, CurriculumId = 100, SubSpecialityId = GeneralPaediatrics,
                AdoptedOn = new DateOnly(2026, 1, 1), IsActive = true
            },
            // A trained General Surgery once; the adoption is no longer active.
            new InstitutionCurriculumAdoption
            {
                Id = 2, InstitutionId = InstitutionA, CurriculumId = 200, SubSpecialityId = GeneralSurgery,
                AdoptedOn = new DateOnly(2025, 1, 1), IsActive = false
            },
            // B trains General Internal Medicine; A does not.
            new InstitutionCurriculumAdoption
            {
                Id = 3, InstitutionId = InstitutionB, CurriculumId = 300, SubSpecialityId = GeneralInternalMedicine,
                AdoptedOn = new DateOnly(2026, 1, 1), IsActive = true
            });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private static async Task SaveAndClearAsAuditPipelineWouldAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private async Task<IReadOnlyList<(int InstitutionId, int? SpecialityId)>> StoredPanelsAsync()
    {
        await using var read = CreateDb();
        return (await read.DecisionPanels.OrderBy(panel => panel.Id).ToListAsync())
            .Select(panel => (panel.InstitutionId, panel.SpecialityId))
            .ToArray();
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);
}
