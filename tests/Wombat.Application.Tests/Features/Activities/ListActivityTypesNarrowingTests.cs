using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T123 defect 3 — the activity-type picker narrows to tools whose rating ladder the subject's
/// curriculum could actually credit. Shipping T109's unshipped option 2.
/// </summary>
/// <remarks>
/// The scenario is the one the evidence run produced: a trainee moved onto the six-rung v11.1
/// curriculum was still offered the four legacy five-rung institution-scoped tools. Picking one is not
/// harmlessly uncredited — it half-credits, incrementing the volume counter while T109 refuses the
/// minimum — and no label can prevent that. Only not offering the tool can.
/// </remarks>
public sealed class ListActivityTypesNarrowingTests
{
    private const int LegacyScaleId = 2;
    private const int CpsaScaleId = 3;

    [Fact]
    public async Task ASixRungTrainee_IsNotOfferedAFiveRungTool()
    {
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "ndlovu", pinnedTo: CpsaScaleId);
        await db.SaveChangesAsync();

        var offered = await Offer(db, "ndlovu");

        offered.Should().Contain("mini_cex_cpsa", "its ladder is the one the curriculum pins");
        offered.Should().NotContain("mini_cex_paed", "its ladder is the five-rung legacy one");
        offered.Should().NotContain("msf_paed");
    }

    [Fact]
    public async Task AToolWhoseScaleKeyResolvesToNothing_StaysOnTheMenu()
    {
        // The four generic seeds declare "or_scale" and the seeded scale is named "O-R Scale" (T110).
        // A restriction that fires where the answer is unknown is the failure T108 guarded against.
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "ndlovu", pinnedTo: CpsaScaleId);
        await db.SaveChangesAsync();

        var offered = await Offer(db, "ndlovu");

        offered.Should().Contain("mini_cex_generic");
    }

    [Fact]
    public async Task AToolWithNoCreditRules_IsNeverNarrowed()
    {
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "ndlovu", pinnedTo: CpsaScaleId);
        await db.SaveChangesAsync();

        var offered = await Offer(db, "ndlovu");

        offered.Should().Contain("reflective_note", "it rates nothing, so it commits to no ladder");
    }

    [Fact]
    public async Task AToolRatingOnAScaleTheCreditRulesNeverName_IsNotNarrowedByIt()
    {
        // Mirroring the engine, not inventing a second rule: CreditApplier compares only the field a
        // directive names in minimum_level_field. A decorative scale field is not the type's ladder.
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "ndlovu", pinnedTo: CpsaScaleId);
        await db.SaveChangesAsync();

        var offered = await Offer(db, "ndlovu");

        offered.Should().Contain("unrated_extra_scale");
    }

    [Fact]
    public async Task ATraineeOnAnUnpinnedCurriculum_SeesEveryToolTheySawBefore()
    {
        // Curriculum 2 is 0/15 pinned on dev, so no legacy trainee's menu may change at all.
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "legacy", pinnedTo: null);
        await db.SaveChangesAsync();

        var offered = await Offer(db, "legacy");

        offered.Should().BeEquivalentTo(
            "mini_cex_cpsa", "mini_cex_paed", "msf_paed", "mini_cex_generic",
            "reflective_note", "unrated_extra_scale");
    }

    [Fact]
    public async Task APendingTraineeWithNoProfile_GetsTheFullClaimsScopedList_NotAnEmptyOne()
    {
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        await db.SaveChangesAsync();

        var offered = await Offer(db, "no-profile-at-all");

        offered.Should().HaveCount(6);
    }

    [Fact]
    public async Task NoSubject_AppliesNoNarrowingAtAll()
    {
        // The activity-type builder's live preview has no subject. It must see everything.
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "ndlovu", pinnedTo: CpsaScaleId);
        await db.SaveChangesAsync();

        var handler = new ListActivityTypesQueryHandler(db);
        var result = await handler.Handle(
            new ListActivityTypesQuery(Principal(), SubjectUserId: null), CancellationToken.None);

        result.Select(item => item.Key).Should().HaveCount(6);
    }

    [Fact]
    public async Task ACurriculumPinnedToTwoLaddersOffersToolsOnEither()
    {
        // CurriculumItem.ScaleId is per item and nothing compares an item's pin with its siblings',
        // so both sides of this predicate are sets.
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "mixed", pinnedTo: CpsaScaleId);
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 902, CurriculumId = 90, EpaId = 2, RequiredCount = 1,
            MinimumLevelOrder = 3, WindowMonths = 12, ScaleId = LegacyScaleId
        });
        await db.SaveChangesAsync();

        var offered = await Offer(db, "mixed");

        offered.Should().Contain("mini_cex_cpsa");
        offered.Should().Contain("mini_cex_paed");
    }

    /// <summary>
    /// T158. A deactivated EPA's item is not in force, so its ladder offers no tool: nothing filed with that tool could
    /// credit the item, and the EPA picker beside this one no longer offers its EPA. Reactivating brings the tool back.
    /// </summary>
    [Fact]
    public async Task ALadderOnlyADeactivatedEpasItemIsPinnedTo_OffersNoTool_UntilItIsReactivated()
    {
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "mixed", pinnedTo: CpsaScaleId);
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 902, CurriculumId = 90, EpaId = 2, RequiredCount = 1,
            MinimumLevelOrder = 3, WindowMonths = 12, ScaleId = LegacyScaleId
        });
        await db.SaveChangesAsync();

        (await Offer(db, "mixed")).Should().Contain("mini_cex_paed", "the legacy ladder is pinned by an item in force");

        (await db.Epas.SingleAsync(epa => epa.Id == 2)).IsActive = false;
        await db.SaveChangesAsync();

        var offered = await Offer(db, "mixed");
        offered.Should().NotContain("mini_cex_paed", "only the deactivated EPA's item is on the legacy ladder");
        offered.Should().NotContain("msf_paed");
        offered.Should().Contain("mini_cex_cpsa", "the item in force still pins the six-rung ladder");

        (await db.Epas.SingleAsync(epa => epa.Id == 2)).IsActive = true;
        await db.SaveChangesAsync();

        (await Offer(db, "mixed")).Should().Contain("mini_cex_paed");
    }

    [Fact]
    public async Task AnotherInstitutionsLocalCurriculumItemDoesNotDecideThisTraineesMenu()
    {
        // A national curriculum row is SHARED by every adopting institution and CurriculumItems is
        // unique on (CurriculumId, EpaId), so an institution-local item added by institution 99 is the
        // only row for its EPA. Unscoped, its ladder would narrow institution 2's trainee's picker.
        // CreditApplier and ResolveCreditableEpaIdsAsync both scope on OwningInstitutionId; so must this.
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "ndlovu", pinnedTo: null, institutionId: 2);
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 903, CurriculumId = 90, EpaId = 3, RequiredCount = 1,
            MinimumLevelOrder = 3, WindowMonths = 12,
            ScaleId = CpsaScaleId, OwningInstitutionId = 99
        });
        await db.SaveChangesAsync();

        var offered = await Offer(db, "ndlovu");

        offered.Should().Contain("mini_cex_paed",
            "institution 99's pin is not this trainee's ladder, so nothing may be narrowed by it");
        offered.Should().HaveCount(6);
    }

    [Fact]
    public async Task ThisInstitutionsOwnLocalItemDoesDecideTheMenu()
    {
        // The other half of the same predicate: a local item this trainee's institution added counts,
        // exactly as it counts for credit.
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "ndlovu", pinnedTo: null, institutionId: 2);
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 904, CurriculumId = 90, EpaId = 3, RequiredCount = 1,
            MinimumLevelOrder = 3, WindowMonths = 12,
            ScaleId = CpsaScaleId, OwningInstitutionId = 2
        });
        await db.SaveChangesAsync();

        var offered = await Offer(db, "ndlovu");

        offered.Should().Contain("mini_cex_cpsa");
        offered.Should().NotContain("mini_cex_paed");
    }

    /// <summary>
    /// T122 added no tool conjunct to the type menu. Tool keys on the types and allow-lists on the items must leave
    /// the offered set exactly as it is without them — including a keyed type that no item permits.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The design rejected dropping a keyed tool that no item permits. The EPA picker already falls back to T108's
    /// creditable set for that tool, and the write path refuses it with a message naming the instruments the
    /// curriculum accepts. Dropping it here instead would take a working instrument off the menu with no
    /// explanation, and would add a third copy of the profile-and-owner predicate to guard a configuration the
    /// seeded catalogue cannot produce. So this filter only ever decides WHICH tool is picked, by ladder, and the
    /// allow-list only ever narrows WHICH EPA it is filed against.
    /// </para>
    /// <para>
    /// Mini-CEX is forbidden on every item here, and three types carry the <c>mini_cex</c> key. Both ladder branches
    /// run: pinned, where T123 d3 narrows, and unpinned, where it narrows nothing. Each is compared with the same
    /// seed minus the T122 data, so the fact cannot pass just because the ladder filter happened to hide a keyed type.
    /// </para>
    /// </remarks>
    [Theory]
    [InlineData(null)]
    [InlineData(CpsaScaleId)]
    public async Task ToolKeysAndAllowLists_DoNotChangeTheTypeMenu(int? pinnedTo)
    {
        var withoutToolData = await OfferWithOrWithoutToolData(pinnedTo, withToolData: false);
        var withToolData = await OfferWithOrWithoutToolData(pinnedTo, withToolData: true);

        withToolData.Should().BeEquivalentTo(withoutToolData,
            "T122 narrows the EPA picker and gates the write path; it adds nothing to the type menu");
        withToolData.Should().Contain("mini_cex_cpsa",
            "no item permits Mini-CEX, and the menu still offers it: the EPA picker falls back and submit explains");
    }

    private static async Task<IReadOnlyList<string>> OfferWithOrWithoutToolData(int? pinnedTo, bool withToolData)
    {
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "ndlovu", pinnedTo);

        // A second item on the same ladder, so there is an item a keyed tool IS permitted on as well as one it is not.
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 905, CurriculumId = 90, EpaId = 2, RequiredCount = 1,
            MinimumLevelOrder = 3, WindowMonths = 12, ScaleId = pinnedTo
        });

        if (withToolData)
        {
            var toolKeys = new Dictionary<string, string?>
            {
                ["mini_cex_cpsa"] = "mini_cex",
                ["mini_cex_paed"] = "mini_cex",
                ["mini_cex_generic"] = "mini_cex",
                ["msf_paed"] = "msf",
                ["unrated_extra_scale"] = "dops",
                ["reflective_note"] = null
            };

            foreach (var type in db.ActivityTypes.Local)
            {
                type.WbaToolKey = toolKeys[type.Key];
            }

            // Mini-CEX is on neither list: forbidden on every item of the subject's curriculum.
            db.CurriculumItems.Local.Single(item => item.Id == 901).PermittedToolsJson = """["cbd","msf"]""";
            db.CurriculumItems.Local.Single(item => item.Id == 905).PermittedToolsJson = """["dops"]""";
        }

        await db.SaveChangesAsync();
        return await Offer(db, "ndlovu");
    }

    [Fact]
    public async Task TwoPastProfiles_TheMenuFollowsThePreferredProfile_NotTheLaterProgrammeStart()
    {
        // T185. The tool menu reads the profile credit reads: the preferred one, the highest id when none is active.
        // It took the latest programme start, so here it offered the legacy five-rung tools of profile 90's curriculum
        // while credit and the EPA picker read profile 95's, pinned to the CPSA ladder.
        await using var db = CreateDb();
        SeedLadders(db);
        SeedTypes(db);
        SeedTrainee(db, "ndlovu", pinnedTo: LegacyScaleId);
        db.Set<TraineeProfile>().Local.Single(profile => profile.Id == 90).IsActive = false;
        db.Set<Curriculum>().Add(new Curriculum
        {
            Id = 95, SubSpecialityId = 1, Name = "Earlier programme",
            Version = "11.0", EffectiveFrom = new DateOnly(2020, 1, 1), IsActive = true
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 951, CurriculumId = 95, EpaId = 1, RequiredCount = 6, MinimumLevelOrder = 6, WindowMonths = 12,
            ScaleId = CpsaScaleId
        });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 95, UserId = "ndlovu", CurriculumId = 95, InstitutionId = 2,
            ProgrammeStartDate = new DateOnly(2020, 1, 1), ExpectedCompletionDate = new DateOnly(2024, 1, 1),
            IsActive = false
        });
        await db.SaveChangesAsync();

        var offered = await Offer(db, "ndlovu");

        offered.Should().Contain("mini_cex_cpsa", "profile 95's curriculum pins the CPSA ladder");
        offered.Should().NotContain("mini_cex_paed", "profile 90's legacy ladder is not the trainee's programme");
    }

    private static async Task<IReadOnlyList<string>> Offer(ApplicationDbContext db, string subjectUserId)
    {
        var handler = new ListActivityTypesQueryHandler(db);
        var result = await handler.Handle(
            new ListActivityTypesQuery(Principal(), subjectUserId), CancellationToken.None);
        return result.Select(item => item.Key).ToList();
    }

    private static ClaimsPrincipal Principal() =>
        new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "viewer"), new Claim("institution_id", "2")],
            "test"));

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedLadders(ApplicationDbContext db)
    {
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = LegacyScaleId, Name = "Paed General Entrustment Scale" });
        foreach (var order in Enumerable.Range(1, 5))
        {
            db.Set<EntrustmentLevel>().Add(new EntrustmentLevel
            { Id = 200 + order, ScaleId = LegacyScaleId, Order = order, Label = $"Legacy {order}" });
        }

        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = CpsaScaleId, Name = "CPSA Paediatric Entrustment Scale v11.1" });
        var cpsa = new[] { "1", "2", "3a", "3b", "4", "5" };
        for (var order = 1; order <= cpsa.Length; order++)
        {
            db.Set<EntrustmentLevel>().Add(new EntrustmentLevel
            { Id = 300 + order, ScaleId = CpsaScaleId, Order = order, Label = cpsa[order - 1] });
        }
    }

    private static void SeedTypes(ApplicationDbContext db)
    {
        const string ratedCredit = """
            {"counts_for":[{"amount":1,"minimum_level_field":"overall_level","curriculum_item_match":{"epa_field":"epa_id"}}]}
            """;

        // Named by a credit directive: this IS the type's ladder.
        AddType(db, 17, "mini_cex_cpsa", RatedSchema("overall_level", "CPSA Paediatric Entrustment Scale v11.1"), ratedCredit);
        AddType(db, 11, "mini_cex_paed", RatedSchema("overall_level", "2"), ratedCredit);
        AddType(db, 14, "msf_paed", RatedSchema("overall_level", "2"), ratedCredit);

        // scale_key resolves to nothing — the T110 state.
        AddType(db, 1, "mini_cex_generic", RatedSchema("overall_level", "or_scale"), ratedCredit);

        // No credit rules at all.
        AddType(db, 5, "reflective_note", """
            {"version":1,"sections":[{"key":"d","title":"D","fields":[{"key":"note","type":"longtext","label":"Note"}]}]}
            """, null);

        // A scale field the credit rules never name. Decorative, not the type's ladder.
        AddType(db, 99, "unrated_extra_scale", """
            {"version":1,"sections":[{"key":"d","title":"D","fields":[
              {"key":"confidence","type":"scale","label":"Confidence","scale_key":"2"}]}]}
            """, """
            {"counts_for":[{"amount":1,"curriculum_item_match":{"epa_field":"epa_id"}}]}
            """);
    }

    private static string RatedSchema(string fieldKey, string scaleKey) => $$"""
        {"version":1,"sections":[{"key":"d","title":"D","fields":[
          {"key":"{{fieldKey}}","type":"scale","label":"Level","required":true,"scale_key":"{{scaleKey}}"}]}]}
        """;

    private static void AddType(ApplicationDbContext db, int id, string key, string schemaJson, string? creditRulesJson)
    {
        db.ActivityTypes.Add(new ActivityType
        {
            Id = id,
            Key = key,
            Name = key,
            Version = 1,
            IsActive = true,
            OwnerUserId = "admin",
            CreatedOn = DateTime.UtcNow,
            Scope = ActivityScope.Institution,
            ScopeId = 2,
            SchemaJson = schemaJson,
            CreditRulesJson = creditRulesJson
        });
    }

    private static void SeedTrainee(ApplicationDbContext db, string userId, int? pinnedTo, int institutionId = 2)
    {
        // Every item a fact here adds names one of these. An item is read only while its EPA is active (T158), so an
        // item whose EPA row is missing would be read as not in force, a state Postgres cannot hold.
        foreach (var epaId in new[] { 1, 2, 3 })
        {
            db.Epas.Add(new Epa { Id = epaId, SubSpecialityId = 1, Code = $"PAED-00{epaId}", Title = $"EPA {epaId}" });
        }

        db.Set<Curriculum>().Add(new Curriculum
        {
            Id = 90, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum",
            Version = "11.1", EffectiveFrom = new DateOnly(2026, 1, 1), IsActive = true
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 901, CurriculumId = 90, EpaId = 1, RequiredCount = 6,
            MinimumLevelOrder = 6, WindowMonths = 12, ScaleId = pinnedTo
        });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 90, UserId = userId, CurriculumId = 90, InstitutionId = institutionId,
            ProgrammeStartDate = new DateOnly(2026, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 12, 31),
            IsActive = true
        });
    }
}
