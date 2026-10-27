using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Trainees;

/// <summary>
/// T312: an ended programme keeps the list it ended with, recorded with the end, and its record reads that list, so a
/// catalogue change after a graduation (an EPA paused, an item added, a target changed) does not rewrite it. Replay
/// findings F-6.21a and F-6.40a: Dr Molefe's record lost PAED-012 while it was paused ("14 of 14"), and gained a KGK-001
/// card with "1 short" in every closed year once KGK added its own item.
/// </summary>
public sealed class EndedCurriculumListTests
{
    private const string Graduate = "trainee-1";
    private const string Registrar = "trainee-2";
    private const int Host = 1;
    private const int Elsewhere = 2;
    private const int CurriculumId = 10;
    private const int Cpsa = 42;

    /// <summary>The graduation day, as the replay's Act 5 records it.</summary>
    private static readonly DateOnly LastDay = new(2026, 9, 26);

    /// <summary>Noon UTC on the graduation day: 14:00 in South Africa, when the end is recorded.</summary>
    private static readonly DateTimeOffset Now = new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);

    /// <summary>A day the record is read on, after the catalogue changed.</summary>
    private static readonly DateOnly Later = new(2026, 10, 20);

    [Fact]
    public async Task AnEpaPausedAfterTheGraduation_StaysOnHerRecord_WithItsPeriods_AndInHerExitRule_WhileARunningTraineeLosesIt()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        await CompleteAsync(db);
        var standingBefore = await StandingAsync(db, Graduate);

        // The College pauses PAED-002 a week after she graduated (Act 6, Step 6.21's PAED-012).
        db.Epas.Single(epa => epa.Id == 2).Deactivate(new DateTime(2026, 10, 3, 8, 0, 0, DateTimeKind.Utc));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var record = (await TraineeQuotaProgressReader.ReadAsync(db, Graduate, Later, CancellationToken.None))!;
        record.Ended.Should().NotBeNull();
        record.Items.Select(item => item.EpaCode).Should().Equal("PAED-001", "PAED-002", "PAED-003");
        var paused = record.Items.Single(item => item.EpaCode == "PAED-002");
        paused.EpaInForce.Should().BeFalse("the card is headed \"(no longer in use)\", not dropped");
        paused.Periods!.Single(period => period.Name == "Semester 1, 2026")
            .Should().Match<QuotaWindowDto>(period => period.Count == 3 && period.IsMet);
        record.Paused.Should().BeEmpty("on the record the paused EPA is a card with its periods, not a footnote");

        var standingAfter = await StandingAsync(db, Graduate);
        standingAfter.Exit.Should().BeEquivalentTo(standingBefore.Exit, "15 of 15 stays 15 of 15 while an EPA is paused");
        standingAfter.Exit.EpaCount.Should().Be(3);
        standingAfter.Exit.AtExitLevel.Should().Be(3);
        standingAfter.Epas.Single(epa => epa.EpaCode == "PAED-002")
            .Should().Match<EpaStandingDto>(epa => !epa.EpaInForce && epa.Decision != null);

        // A registrar still in the programme on the same curriculum: T158, unchanged.
        var running = (await TraineeQuotaProgressReader.ReadAsync(db, Registrar, Later, CancellationToken.None))!;
        running.Ended.Should().BeNull();
        running.Items.Select(item => item.EpaCode).Should().Equal("PAED-001", "PAED-003");
        running.Paused.Select(item => item.EpaCode).Should().Equal("PAED-002");
        (await StandingAsync(db, Registrar)).Epas.Select(epa => epa.EpaCode).Should().Equal("PAED-001", "PAED-003");
    }

    [Fact]
    public async Task AnItemAddedAndATargetChangedAfterTheEnd_AreNotReadOnTheRecord()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        await CompleteAsync(db);

        // Act 6, Step 6.40: the institution adds its own item; and the College changes a target.
        db.Epas.Add(new Epa { Id = 9, SubSpecialityId = 1, OwningInstitutionId = Host, Code = "KGK-001", Title = "Our own extra" });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 109, CurriculumId = CurriculumId, EpaId = 9, OwningInstitutionId = Host, RequiredCount = 1,
            QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12, ScaleId = Cpsa
        });
        db.CurriculumItems.Single(item => item.Id == 101).RequiredCount = 5;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var record = (await TraineeQuotaProgressReader.ReadAsync(db, Graduate, Later, CancellationToken.None))!;
        record.Items.Select(item => item.EpaCode).Should().Equal("PAED-001", "PAED-002", "PAED-003");
        var first = record.Items.Single(item => item.EpaCode == "PAED-001");
        first.Target.Should().Be(3, "the target the programme ended with");
        first.Periods!.Should().OnlyContain(period => period.Target == 3);
        (await StandingAsync(db, Graduate)).Epas.Select(epa => epa.EpaCode).Should().Equal("PAED-001", "PAED-002", "PAED-003");

        // The running registrar is held to the catalogue as it stands.
        var running = (await TraineeQuotaProgressReader.ReadAsync(db, Registrar, Later, CancellationToken.None))!;
        running.Items.Select(item => item.EpaCode).Should().Equal("KGK-001", "PAED-001", "PAED-002", "PAED-003");
        running.Items.Single(item => item.EpaCode == "PAED-001").Target.Should().Be(5);
    }

    [Fact]
    public async Task TheEndedRecordsEpaPages_AreTheEpasOfTheListItEndedWith()
    {
        // Flow 13's R06: a paused EPA's page on an ended record reads its item from the list, with every period; an item
        // added after the end has no page on the record, and My activities links to none (EpasWithAPageAsync).
        await using var db = CreateDb();
        await SeedAsync(db);
        await CompleteAsync(db);
        db.Epas.Single(epa => epa.Id == 2).Deactivate(new DateTime(2026, 10, 3, 8, 0, 0, DateTimeKind.Utc));
        db.Epas.Add(new Epa { Id = 9, SubSpecialityId = 1, OwningInstitutionId = Host, Code = "KGK-001", Title = "Our own extra" });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 109, CurriculumId = CurriculumId, EpaId = 9, OwningInstitutionId = Host, RequiredCount = 1,
            QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var handler = new Wombat.Application.Features.Curricula.GetEpaProgressForTrainee.GetEpaProgressForTraineeQueryHandler(
            db, new FixedClock(Now));
        var self = TestPrincipals.InRoles([], Graduate, Host);

        var page = await handler.Handle(
            new Wombat.Application.Features.Curricula.GetEpaProgressForTrainee.GetEpaProgressForTraineeQuery(self, 2, Later),
            CancellationToken.None);
        page!.Ended.Should().NotBeNull();
        page.Item.EpaInForce.Should().BeFalse();
        page.Item.Periods!.Single(period => period.Name == "Semester 1, 2026").Count.Should().Be(3);

        (await handler.Handle(
                new Wombat.Application.Features.Curricula.GetEpaProgressForTrainee.GetEpaProgressForTraineeQuery(self, 9, Later),
                CancellationToken.None))
            .Should().BeNull("KGK-001 was added after the end");
        (await TraineeQuotaProgressReader.EpasWithAPageAsync(db, Graduate, CancellationToken.None))
            .Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public async Task CodesAndTitlesStayLive_OnTheRecord()
    {
        // Step 6.14: a corrected title still shows on a closed record; only the list and the targets are frozen.
        await using var db = CreateDb();
        await SeedAsync(db);
        await CompleteAsync(db);

        db.Epas.Single(epa => epa.Id == 3).Title = "Corrected title";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var record = (await TraineeQuotaProgressReader.ReadAsync(db, Graduate, Later, CancellationToken.None))!;
        record.Items.Single(item => item.EpaCode == "PAED-003").EpaTitle.Should().Be("Corrected title");
    }

    [Fact]
    public async Task HerStars_AreReadAsTheyStoodOnTheLastDay()
    {
        // A STAR that expired, was revoked or was superseded after the end still counts; one revoked before it does not,
        // and one issued after it at a review of another programme is not read.
        await using var db = CreateDb();
        await SeedAsync(db, withStars: false);
        var expiring = Star(db, 1, "5", new DateOnly(2026, 6, 1), expiresOn: new DateOnly(2026, 9, 30));
        var revokedAfter = Star(db, 2, "5", new DateOnly(2026, 6, 1));
        var superseded = Star(db, 3, "4", new DateOnly(2026, 6, 1));
        var revokedBefore = Star(db, 4, "4", new DateOnly(2026, 6, 1));
        await db.SaveChangesAsync();
        revokedBefore.Revoke("Concern raised", "chair-1", new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc));
        await db.SaveChangesAsync();
        await CompleteAsync(db);

        expiring.MarkExpired(new DateTime(2026, 10, 2, 8, 0, 0, DateTimeKind.Utc));
        revokedAfter.Revoke("Recorded in error", "chair-1", new DateTime(2026, 10, 2, 8, 0, 0, DateTimeKind.Utc));
        var successor = Star(db, 3, "3b", new DateOnly(2026, 10, 3), reviewId: ReviewOfALaterProgramme);
        superseded.SupersedeBy(successor);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var standing = await StandingAsync(db, Graduate);

        standing.Epas.Select(epa => (epa.EpaCode, epa.Decision?.LevelLabel)).Should().Equal(
            ("PAED-001", "5"), ("PAED-002", "5"), ("PAED-003", "4"), ("PAED-004", null));
        standing.Exit.AtExitLevel.Should().Be(3);
        standing.Exit.NotYetEpaCodes.Should().Equal("PAED-004");

        // T379 (flow 13, F6 and S8; build-contracts § 0): the record's STARs as Home and My progress count them. Issued: any
        // ratified decision, the one revoked before the end included; in force: the standing's own count, as the
        // programme left them.
        (await RecordStars.ReadAsync(db, TestPrincipals.Administrator(), Graduate, Later, CancellationToken.None))
            .Should().Be(new RecordStarsDto(AnyIssued: true, InForce: 3));
        (await RecordStars.ReadAsync(db, TestPrincipals.Administrator(), Registrar, Later, CancellationToken.None))
            .Should().Be(new RecordStarsDto(AnyIssued: false, InForce: 0), "no STAR was issued to the registrar");
    }

    /// <summary>
    /// T379 (flow 13, build-contracts § 0): how a programme ended, with the training year it ended in, read one way for the
    /// former trainee's Home, the ended profile and the staff export.
    /// </summary>
    [Fact]
    public async Task AnEndedProgramme_ReadsItsEndAndTheTrainingYearItEndedIn_AndARunningOneNone()
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        var running = await db.TraineeProfiles.AsNoTracking().SingleAsync(profile => profile.Id == 1);
        Wombat.Application.Features.Users.EndedProgrammes.Of(running).Should().BeNull("the programme runs");

        await CompleteAsync(db);
        var ended = await db.TraineeProfiles.AsNoTracking().SingleAsync(profile => profile.Id == 1);
        var end = Wombat.Application.Features.Users.EndedProgrammes.Of(ended)!;

        end.EndedOn.Should().Be(LastDay);
        end.Completed.Should().BeTrue();
        end.TrainingYearAtEnd.Should().Be(TraineeProfile.StageOn(ended.ProgrammeStartDate, LastDay));
    }

    [Fact]
    public async Task AStarTheCommitteeIssuedForTheProgramme_AfterItsLastDay_IsOnTheRecord_ButNotOneFromAnotherProgrammesReview()
    {
        // The operator's decision on T312's B5 (2026-10-27): a sitting convened for the programme may ratify after the last
        // day, and what it issued is the programme's; a review of another programme of the same trainee is not; and nothing
        // after the end, a revocation included, takes a STAR off the record.
        await using var db = CreateDb();
        await SeedAsync(db, withStars: false);
        await CompleteAsync(db);

        Star(db, 1, "5", new DateOnly(2026, 10, 5));
        var revokedSince = Star(db, 3, "4", new DateOnly(2026, 10, 5));
        Star(db, 2, "5", new DateOnly(2026, 12, 1), reviewId: ReviewOfALaterProgramme);
        Star(db, 4, "5", new DateOnly(2026, 10, 5), reviewId: ReviewOfAnEarlierProgramme);
        await db.SaveChangesAsync();
        revokedSince.Revoke("Recorded in error", "chair-1", new DateTime(2026, 10, 10, 8, 0, 0, DateTimeKind.Utc));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var standing = await StandingAsync(db, Graduate);

        standing.Epas.Select(epa => (epa.EpaCode, epa.Decision?.LevelLabel)).Should().Equal(
            ("PAED-001", "5"), ("PAED-002", null), ("PAED-003", "4"), ("PAED-004", null));
        standing.StarsAsEndedOn.Should().Be(LastDay);
    }

    [Fact]
    public async Task AStarSupersededAfterTheEnd_ByOneTheCommitteeIssuedForTheProgramme_ReadsAsTheSuccessor()
    {
        await using var db = CreateDb();
        await SeedAsync(db, withStars: false);
        var first = Star(db, 3, "4", new DateOnly(2026, 6, 1));
        await db.SaveChangesAsync();
        await CompleteAsync(db);

        first = await db.EntrustmentDecisions.SingleAsync(decision => decision.Id == first.Id);
        var successor = Star(db, 3, "5", new DateOnly(2026, 10, 3));
        first.SupersedeBy(successor);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var standing = await StandingAsync(db, Graduate);

        standing.Epas.Single(epa => epa.EpaCode == "PAED-003").Decision!.LevelLabel.Should().Be("5",
            "both are the programme's, and the reader takes the latest issued");
    }

    [Fact]
    public async Task ARunningProgrammesStanding_SaysNothingOfAnEnd()
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        var standing = await StandingAsync(db, Registrar);

        standing.StarsAsEndedOn.Should().BeNull();
    }

    [Fact]
    public async Task AWithdrawnRecord_GainsNoItemAddedAfterItsEnd()
    {
        // Act 6, Step 6.41: Dr du Plessis's withdrawn record, after KGK adds its own item (T312 review, R3).
        await using var db = CreateDb();
        await SeedAsync(db);
        await WithdrawalHandler(db)
            .Handle(new DeactivateTraineeProfileCommand(1, LastDay, TestPrincipals.Administrator()), CancellationToken.None);
        db.ChangeTracker.Clear();
        AddKgkItem(db);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var record = (await TraineeQuotaProgressReader.ReadAsync(db, Graduate, Later, CancellationToken.None))!;
        record.Ended.Should().NotBeNull();
        record.Items.Select(item => item.EpaCode).Should().Equal("PAED-001", "PAED-002", "PAED-003");
        (await StandingAsync(db, Graduate)).Exit.EpaCount.Should().Be(3);
    }

    [Fact]
    public async Task TheCompletedCardsCountLine_ReadsTheListItEndedWith()
    {
        // T312 review (B4): the completed card's count line is My progress's figure, so it reads the same list: an EPA paused
        // after the end keeps its line, and a target changed since is not read.
        await using var db = CreateDb();
        await SeedAsync(db);
        await CompleteAsync(db);
        db.Epas.Single(epa => epa.Id == 2).Deactivate(new DateTime(2026, 10, 3, 8, 0, 0, DateTimeKind.Utc));
        db.CurriculumItems.Single(item => item.Id == 102).RequiredCount = 5;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var key = (EpaId: 2, ObservedOn: new DateOnly(2026, 5, 12));
        var lines = await EpaCountLines.ReadAsync(db, Graduate, [key], Later, CancellationToken.None);

        var line = lines[key];
        line.EpaCode.Should().Be("PAED-002");
        (line.Window.Count, line.Window.Target, line.Window.IsMet).Should().Be((3, 3, true));

        // The registrar, still in the programme: the paused EPA has no line (T158).
        (await EpaCountLines.ReadAsync(db, Registrar, [key], Later, CancellationToken.None)).Should().BeEmpty();
    }

    [Fact]
    public async Task TheEndedRecordsMsfLines_AreTheEpasOfTheListItEndedWith()
    {
        // T312 review (R3): the MSF card lines read the same list as the cards: a pause after the end keeps the EPA's line,
        // and an item added after it gets none.
        await using var db = CreateDb();
        await SeedAsync(db);
        await CompleteAsync(db);
        db.Epas.Single(epa => epa.Id == 2).Deactivate(new DateTime(2026, 10, 3, 8, 0, 0, DateTimeKind.Utc));
        AddKgkItem(db);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var coverage = await new GetMsfCoverageForTraineeQueryHandler(db).Handle(
            new GetMsfCoverageForTraineeQuery(Graduate, TestPrincipals.Administrator(), AsOf: Later), CancellationToken.None);

        coverage!.Epas.Select(epa => epa.EpaCode).Should().Equal("PAED-001", "PAED-002", "PAED-003");

        var running = await new GetMsfCoverageForTraineeQueryHandler(db).Handle(
            new GetMsfCoverageForTraineeQuery(Registrar, TestPrincipals.Administrator(), AsOf: Later), CancellationToken.None);
        running!.Epas.Select(epa => epa.EpaCode).Should().Equal("KGK-001", "PAED-001", "PAED-003");
    }

    [Fact]
    public async Task AnItemOnTheListAProgrammeEndedWith_CannotBeRemoved_ButAnotherCan()
    {
        // T312 review (B1): its progress rows go with it, so a removal would leave the closed record a card reading "0 of N,
        // short". Refused before anything changes; an item no ended list names is removed as before.
        await using var db = CreateDb();
        await SeedAsync(db);
        await CompleteAsync(db);
        AddKgkItem(db);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var remove = () => new RemoveCurriculumItemCommandHandler(db).Handle(
            new RemoveCurriculumItemCommand(CurriculumId, 102, TestPrincipals.Administrator()), CancellationToken.None);

        await remove.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(RemoveCurriculumItemCommandHandler.EndedRecordsHoldTheItem(1));
        db.ChangeTracker.Clear();
        (await db.CurriculumItems.AnyAsync(item => item.Id == 102)).Should().BeTrue();
        (await db.CurriculumItemProgresses.AnyAsync(row => row.CurriculumItemId == 102)).Should().BeTrue();

        var removed = await new RemoveCurriculumItemCommandHandler(db).Handle(
            new RemoveCurriculumItemCommand(CurriculumId, 109, TestPrincipals.Administrator()), CancellationToken.None);
        removed.Items.Should().NotContain(item => item.Id == 109, "KGK-001 was added after the end: no ended list names it");
    }

    [Fact]
    public async Task AScaleOnlyTheListAProgrammeEndedWith_Pins_CannotBeDeleted_NorTheRungItsMinimaNameRemoved()
    {
        // T312 review (B2): every live item is re-pinned off the CPSA ladder after the end, so no foreign key holds it; the
        // closed record still names its minima as rungs of it.
        await using var db = CreateDb();
        await SeedAsync(db, withStars: false);
        await CompleteAsync(db);
        foreach (var item in db.CurriculumItems)
        {
            item.ScaleId = null;
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var delete = () => new DeleteEntrustmentScaleCommandHandler(db).Handle(
            new DeleteEntrustmentScaleCommand(Cpsa, TestPrincipals.Administrator()), CancellationToken.None);
        await delete.Should().ThrowAsync<InvalidOperationException>().WithMessage("*record of a programme that has ended*");
        db.ChangeTracker.Clear();

        var withoutTheTopRung = CpsaRungs.Take(5)
            .Select((label, index) => new EntrustmentLevelUpdate(4200 + index + 1, index + 1, label, null))
            .ToArray();
        var update = () => new UpdateEntrustmentScaleCommandHandler(db).Handle(
            new UpdateEntrustmentScaleCommand(Cpsa, "CPSA Paediatric Entrustment Scale v11.1", null, withoutTheTopRung,
                TestPrincipals.Administrator()),
            CancellationToken.None);
        await update.Should().ThrowAsync<InvalidOperationException>().WithMessage("*when a programme ended*requires level 6*");
        db.ChangeTracker.Clear();
        (await db.Set<EntrustmentScale>().AnyAsync(scale => scale.Id == Cpsa)).Should().BeTrue();
        (await db.Set<EntrustmentLevel>().CountAsync(level => level.ScaleId == Cpsa)).Should().Be(6);
    }

    private static void AddKgkItem(ApplicationDbContext db)
    {
        db.Epas.Add(new Epa { Id = 9, SubSpecialityId = 1, OwningInstitutionId = Host, Code = "KGK-001", Title = "Our own extra" });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 109, CurriculumId = CurriculumId, EpaId = 9, OwningInstitutionId = Host, RequiredCount = 1,
            QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12, ScaleId = Cpsa
        });
    }

    [Fact]
    public async Task ACompletion_KeepsTheListInTheSameSaveAsTheEnd()
    {
        var saves = new ProfileSaves();
        await using var db = CreateDb(saves);
        await SeedAsync(db);
        saves.Clear();

        await CompleteAsync(db);

        saves.Should().ContainSingle("the end and its list are one save").Which
            .Should().Be((true, true), "the end and its list are one save (EndRecorded, ListRecorded)");
        var profile = await db.TraineeProfiles.AsNoTracking().SingleAsync(entity => entity.Id == 1);
        profile.ReadEndedCurriculum()!.Should().BeEquivalentTo(
        [
            new EndedCurriculumItem(101, 1, false, 3, QuotaPeriod.Semester, 6, StageMap, Cpsa, QuotaPeriod.Semester, false),
            new EndedCurriculumItem(102, 2, false, 3, QuotaPeriod.Semester, 6, StageMap, Cpsa, QuotaPeriod.Semester, false),
            new EndedCurriculumItem(103, 3, false, 3, QuotaPeriod.Semester, 5, null, Cpsa, null, false)
        ]);
    }

    [Fact]
    public async Task AWithdrawal_KeepsTheListInTheSameSaveAsTheEnd_LeavingOutAnEpaPausedBeforeIt_AndAnotherInstitutionsItem()
    {
        var saves = new ProfileSaves();
        await using var db = CreateDb(saves);
        await SeedAsync(db);
        db.Epas.Add(new Epa { Id = 8, SubSpecialityId = 1, OwningInstitutionId = Elsewhere, Code = "ELS-001", Title = "Theirs" });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 108, CurriculumId = CurriculumId, EpaId = 8, OwningInstitutionId = Elsewhere, RequiredCount = 1,
            QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12
        });
        // Paused on the last day, before the end was recorded: it was no target when the programme ended.
        db.Epas.Single(epa => epa.Id == 3).Deactivate(new DateTime(2026, 9, 26, 7, 0, 0, DateTimeKind.Utc));
        await db.SaveChangesAsync();
        saves.Clear();

        await WithdrawalHandler(db)
            .Handle(new DeactivateTraineeProfileCommand(1, LastDay, TestPrincipals.Administrator()), CancellationToken.None);

        saves.Should().ContainSingle().Which.Should().Be((true, true), "the end and its list are one save (EndRecorded, ListRecorded)");
        var profile = await db.TraineeProfiles.AsNoTracking().SingleAsync(entity => entity.Id == 1);
        profile.ReadEndedCurriculum()!.Select(item => item.CurriculumItemId).Should().Equal(101, 102);
    }

    [Fact]
    public async Task ARefusedEnd_KeepsNoList()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var users = new Mock<IUserAdministrationService>();
        var email = new Mock<IEmailSender>();

        // A completion day still to come, a withdrawal before the programme started, and a caller from another
        // institution: each refused before anything is recorded, and the audit pipeline's save after the throw commits
        // nothing.
        var tomorrow = () => CompletionHandler(db, users, email).Handle(
            new CompleteTraineeProfileCommand(1, LastDay.AddDays(1), TestPrincipals.Administrator()), CancellationToken.None);
        var beforeStart = () => WithdrawalHandler(db).Handle(
            new DeactivateTraineeProfileCommand(1, new DateOnly(2022, 12, 31), TestPrincipals.Administrator()), CancellationToken.None);
        var outOfScope = () => CompletionHandler(db, users, email).Handle(
            new CompleteTraineeProfileCommand(1, LastDay, TestPrincipals.InstitutionalAdmin(Elsewhere)), CancellationToken.None);

        await tomorrow.Should().ThrowAsync<InvalidOperationException>();
        await beforeStart.Should().ThrowAsync<InvalidOperationException>();
        await outOfScope.Should().ThrowAsync<UnauthorizedAccessException>();
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var profile = await db.TraineeProfiles.AsNoTracking().SingleAsync(entity => entity.Id == 1);
        profile.IsActive.Should().BeTrue();
        profile.EndedCurriculumJson.Should().BeNull();
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private const string StageMap = """{"1": 3, "2": 4, "3": 5, "4": 6}""";

    private static readonly string[] CpsaRungs = ["1", "2", "3a", "3b", "4", "5"];

    private static async Task<EntrustmentStandingDto> StandingAsync(ApplicationDbContext db, string traineeUserId)
        => (await new GetEntrustmentStandingForTraineeQueryHandler(db).Handle(
            new GetEntrustmentStandingForTraineeQuery(traineeUserId, TestPrincipals.Administrator(), LastDay),
            CancellationToken.None))!;

    private static async Task CompleteAsync(ApplicationDbContext db)
    {
        await CompletionHandler(db, new Mock<IUserAdministrationService>(), new Mock<IEmailSender>())
            .Handle(new CompleteTraineeProfileCommand(1, LastDay, TestPrincipals.Administrator()), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    private static CompleteTraineeProfileCommandHandler CompletionHandler(
        ApplicationDbContext db,
        Mock<IUserAdministrationService> users,
        Mock<IEmailSender> email)
        => new(db, users.Object, email.Object, new CreditApplier(db), new TraineeCreditLock(db), new FixedClock(Now));

    private static DeactivateTraineeProfileCommandHandler WithdrawalHandler(ApplicationDbContext db)
        => new(db, new Mock<IUserAdministrationService>().Object, new CreditApplier(db), new TraineeCreditLock(db), new FixedClock(Now));

    /// <summary>A review of the graduate's programme: semester 1 of 2026, a period inside it (T312, B5).</summary>
    private const int ReviewOfTheProgramme = 30;

    /// <summary>A review of a later programme of the same trainee: its period begins after this one's last day.</summary>
    private const int ReviewOfALaterProgramme = 31;

    /// <summary>A review of an earlier programme: its period ended before this one started.</summary>
    private const int ReviewOfAnEarlierProgramme = 32;

    private static EntrustmentDecision Star(
        ApplicationDbContext db, int epaId, string rung, DateOnly issuedOn, DateOnly? expiresOn = null,
        int reviewId = ReviewOfTheProgramme)
    {
        var decision = EntrustmentDecision.Issue(
            Graduate, epaId, 4200 + Array.IndexOf(CpsaRungs, rung) + 1, issuedOn, expiresOn,
            committeeReviewId: reviewId, "chair-1", "Consistent across the period.", StarEvidence.One());
        db.EntrustmentDecisions.Add(decision);
        return decision;
    }

    /// <summary>
    /// Three national EPAs, two whose exit level is rung 5 and one rung 4, and a fourth for the STAR test; the graduate,
    /// three semester-1 2026 encounters on PAED-002, and her STARs at exit level; and a registrar still in the programme.
    /// </summary>
    private static async Task SeedAsync(ApplicationDbContext db, bool withStars = true)
    {
        db.Institutions.Add(new Institution { Id = Host, Name = "Kgosi Kgari Teaching Hospital" });
        db.Institutions.Add(new Institution { Id = Elsewhere, Name = "Elsewhere" });
        db.Colleges.Add(new College { Id = 1, Name = "CPSA", ShortCode = "CPSA" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = Cpsa, Name = "CPSA Paediatric Entrustment Scale v11.1" });
        for (var order = 1; order <= CpsaRungs.Length; order++)
        {
            db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 4200 + order, ScaleId = Cpsa, Order = order, Label = CpsaRungs[order - 1] });
        }

        db.Curricula.Add(new Curriculum
        {
            Id = CurriculumId, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "11.1",
            EffectiveFrom = new DateOnly(2023, 1, 1), IsActive = true
        });

        db.Epas.Add(new Epa { Id = 1, SubSpecialityId = 1, Code = "PAED-001", Title = "Clerk an acute admission" });
        db.Epas.Add(new Epa { Id = 2, SubSpecialityId = 1, Code = "PAED-002", Title = "Counsel a family" });
        db.Epas.Add(new Epa { Id = 3, SubSpecialityId = 1, Code = "PAED-003", Title = "Manage a ward" });
        db.CurriculumItems.Add(Item(101, 1, exitOrder: 6, StageMap, QuotaPeriod.Semester));
        db.CurriculumItems.Add(Item(102, 2, exitOrder: 6, StageMap, QuotaPeriod.Semester));
        db.CurriculumItems.Add(Item(103, 3, exitOrder: 5, stageMap: null, cadence: null));
        if (!withStars)
        {
            db.Epas.Add(new Epa { Id = 4, SubSpecialityId = 1, Code = "PAED-004", Title = "Lead a resuscitation" });
            db.CurriculumItems.Add(Item(104, 4, exitOrder: 5, stageMap: null, cadence: null));
        }

        foreach (var (id, userId) in new[] { (1, Graduate), (2, Registrar) })
        {
            db.TraineeProfiles.Add(new TraineeProfile
            {
                Id = id, UserId = userId, InstitutionId = Host, CurriculumId = CurriculumId,
                ProgrammeStartDate = new DateOnly(2023, 1, 15), ExpectedCompletionDate = new DateOnly(2027, 1, 15),
                IsActive = true
            });
        }

        foreach (var (id, from, to) in new[]
                 {
                     (ReviewOfTheProgramme, new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30)),
                     (ReviewOfALaterProgramme, new DateOnly(2026, 11, 1), new DateOnly(2027, 4, 30)),
                     (ReviewOfAnEarlierProgramme, new DateOnly(2021, 1, 1), new DateOnly(2021, 12, 31))
                 })
        {
            db.CommitteeReviews.Add(new CommitteeReview
            {
                Id = id, TraineeUserId = Graduate, PanelId = 1, AcademicYear = from.Year, Semester = 1,
                ReviewPeriodFrom = from, ReviewPeriodTo = to, ScheduledOn = to
            });
        }

        db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            CurriculumItemId = 102, TraineeUserId = Graduate, AcademicYear = 2026, Semester = 1,
            CountsSoFar = 3, MinimumLevelReachedCount = 3, LastObservedOn = new DateOnly(2026, 5, 12),
            LastObservedOnDeclared = true, LastUpdated = new DateTime(2026, 5, 12, 8, 0, 0, DateTimeKind.Utc)
        });

        if (withStars)
        {
            Star(db, 1, "5", new DateOnly(2026, 6, 1));
            Star(db, 2, "5", new DateOnly(2026, 6, 1));
            Star(db, 3, "4", new DateOnly(2026, 6, 1));
        }

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private static CurriculumItem Item(int id, int epaId, int exitOrder, string? stageMap, QuotaPeriod? cadence)
        => new()
        {
            Id = id, CurriculumId = CurriculumId, EpaId = epaId, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = exitOrder, MinimumLevelByStageJson = stageMap, WindowMonths = 12, ScaleId = Cpsa,
            DecisionCadence = cadence
        };

    private static ApplicationDbContext CreateDb(ProfileSaves? saves = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());
        if (saves is not null)
        {
            options.AddInterceptors(saves);
        }

        return new ApplicationDbContext(options.Options);
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    /// <summary>Each save that changes a trainee profile: whether it records the end, and whether it records the list.</summary>
    private sealed class ProfileSaves : SaveChangesInterceptor, IEnumerable<(bool EndRecorded, bool ListRecorded)>
    {
        private readonly List<(bool EndRecorded, bool ListRecorded)> _saves = [];

        public void Clear() => _saves.Clear();

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            Record(eventData);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Record(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void Record(DbContextEventData eventData)
        {
            var modified = eventData.Context!.ChangeTracker.Entries<TraineeProfile>()
                .Where(entry => entry.State == EntityState.Modified)
                .ToArray();
            if (modified.Length == 0)
            {
                return;
            }

            _saves.Add((
                modified.Any(entry => entry.Property(profile => profile.IsActive).IsModified),
                modified.Any(entry => entry.Property(profile => profile.EndedCurriculumJson).IsModified)));
        }

        public IEnumerator<(bool EndRecorded, bool ListRecorded)> GetEnumerator() => _saves.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
