using FluentAssertions;
using Wombat.Application.Common.Options;
using Wombat.Domain.Activities;
using Wombat.Tests.Shared;
using static Wombat.Application.Tests.TestHelpers.AssessorReads;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// "Waiting for you" (T350, note 5; round 1, Q1, Q4, E5; round 2, E1): the one read the Activity inbox, the Assessor's
/// Home, the way on and the other-role line share. Every activity a move of which, one that leads on, the caller may make
/// now by the arms that are not the author's, less their own subject rows, oldest first; each with how long it has waited
/// and whether that is overdue, on one clock.
/// </summary>
public sealed class ListWaitingForYouQueryTests
{
    private static readonly DateTime Now = new(2026, 9, 30, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task TheRows_AreOldestFirst_ThenById()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddRequested(db, 1, Now.AddHours(-1));
        AddRequested(db, 2, Now.AddDays(-3));
        AddRequested(db, 3, Now.AddDays(-3));
        AddRequested(db, 4, Now.AddDays(-1));
        await db.SaveChangesAsync();

        var result = await WaitingAsync(db, now: Now);

        result.Items.Select(item => item.Id).Should().Equal([2, 3, 4, 1], "the one that has waited longest leads; a tie goes by id");
        result.Count.Should().Be(4);
        result.Oldest!.Id.Should().Be(2);
    }

    /// <summary>
    /// E5: the caller's own subject rows moved in from the dashboard. A Coordinator may record an MSF draft by her role,
    /// her own included; her own is not work waiting on her for someone else, and an assessor who is also a trainee finds
    /// his own requests on My activities.
    /// </summary>
    [Fact]
    public async Task TheCallersOwnSubjectRows_AreLeftOut_WhateverArmAdmitsThem()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddMsfDraft(db, 1, subject: "coordinator-1", Now.AddDays(-2));
        AddMsfDraft(db, 2, subject: TraineeId, Now.AddDays(-1));
        await db.SaveChangesAsync();

        var result = await WaitingAsync(db, CreatePrincipal("coordinator-1", "Coordinator"), now: Now);

        result.Items.Select(item => item.Id).Should().Equal(2);
    }

    /// <summary>
    /// A <c>role:</c> arm admits the caller (the MSF's record, by Coordinator), and a <c>field:</c> arm naming them does;
    /// the author's arms never do (T342, B6): a draft the assessor raised for a trainee is his to submit as its creator, so
    /// it is not waiting on him as its assessor.
    /// </summary>
    [Fact]
    public async Task ARoleArmAdmits_AFieldArmNamingThemAdmits_AndTheAuthorsArmsDoNot()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddMsfDraft(db, 1, subject: TraineeId, Now.AddDays(-1));
        AddRequested(db, 2, Now.AddDays(-2));
        // A reflection the assessor created for the trainee, still a draft: its submit is "subject|creator".
        db.Activities.Add(new Activity
        {
            Id = 3, ActivityTypeId = ReflectiveTypeId, SchemaVersion = 1, SubjectUserId = TraineeId,
            CreatedByUserId = AssessorId, CurrentState = "draft", DataJson = NamesTheAssessor,
            CreatedOn = Now.AddDays(-3), UpdatedOn = Now.AddDays(-3)
        });
        await db.SaveChangesAsync();

        (await WaitingAsync(db, CreatePrincipal(AssessorId, "Assessor", "Coordinator"), now: Now))
            .Items.Select(item => item.Id).Should().Equal([2, 1], "the request by its field, the MSF by the role; not the draft");
        (await WaitingAsync(db, CreatePrincipal(AssessorId, "Assessor"), now: Now))
            .Items.Select(item => item.Id).Should().Equal([2], "without the role, only the field");
    }

    /// <summary>
    /// E1: overdue at 7 × 24 h since <c>UpdatedOn</c>, exactly, and not a minute before, so a whole-day count of 7 always
    /// carries it; the count of overdue rows is the badge's.
    /// </summary>
    [Fact]
    public async Task ARow_IsOverdueAtExactlySevenTimesTwentyFourHours_AndNotAMinuteBefore()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddRequested(db, 1, Now.AddDays(-7));
        AddRequested(db, 2, Now.AddDays(-7).AddMinutes(1));
        await db.SaveChangesAsync();

        var result = await WaitingAsync(db, now: Now);

        result.Items.Select(item => (item.Id, item.IsOverdue, item.WaitedDays)).Should().Equal((1, true, 7), (2, false, 6));
        result.OverdueCount.Should().Be(1);
    }

    /// <summary>Spec § 7: whole days, rounded down, as the nudge counts them; under a day is 0.</summary>
    [Fact]
    public async Task TheWait_IsWholeDaysRoundedDown_AndUnderADayIsZero()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddRequested(db, 1, Now.AddDays(-3).AddMinutes(1));
        AddRequested(db, 2, Now.AddHours(-23).AddMinutes(-59));
        AddRequested(db, 3, Now.AddHours(-24));
        await db.SaveChangesAsync();

        var result = await WaitingAsync(db, now: Now);

        result.Items.Select(item => (item.Id, item.WaitedDays)).Should().Equal((1, 2), (3, 1), (2, 0));
    }

    [Fact]
    public async Task TheDueDays_AreReadFromTheThresholds_AndDecideOverdue()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        AddRequested(db, 1, Now.AddDays(-3));
        await db.SaveChangesAsync();

        var result = await WaitingAsync(db, now: Now, thresholds: new DashboardThresholds { AssessorDueDays = 3 });

        result.DueDays.Should().Be(3);
        result.Items.Single().IsOverdue.Should().BeTrue();
        (await WaitingAsync(db, now: Now)).Items.Single().IsOverdue.Should().BeFalse("the default is 7");
    }

    /// <summary>
    /// Note 5: two of a registrar's requests that share type, EPA and date are named with their nominee on her own lists
    /// (E7). Here the nominee is the reader, so the name never carries it; the page tells the two apart instead (note 9).
    /// </summary>
    [Fact]
    public async Task TheName_NeverCarriesTheReadersOwnName_AndTheRowSaysWhoseItIs()
    {
        await using var db = CreateDb();
        SeedCpsaTypes(db);
        foreach (var id in new[] { 1, 2 })
        {
            var activity = AddFiled(db, id, CpsaMiniCexTypeId, "requested", TraineeId, NamesTheAssessor, Now.AddHours(-id),
                Move("submit", "draft", "requested", TraineeId, Now.AddHours(-id)));
            activity.ObservedOn = new DateOnly(2026, 9, 20);
            activity.ObservedOnSource = ObservationDateSource.Declared;
        }

        await db.SaveChangesAsync();
        var directory = new FakeUserDirectory((AssessorId, "Mohammed Patel"), (TraineeId, "Anele Dlamini"));

        var result = await WaitingAsync(db, directory: directory, now: Now);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(item =>
        {
            item.DisplayName.Should().StartWith("Mini-CEX (Paediatrics)").And.NotContain("Mohammed Patel");
            item.DisplayNameHasNominee.Should().BeFalse();
            item.SubjectName.Should().Be("Anele Dlamini");
            item.DecidedOn.Should().BeNull("only the decided read says when it was decided");
        });
        directory.Lookups.Should().ContainSingle("the names are read in one lookup");
    }

    [Fact]
    public async Task NothingWaiting_IsAnEmptyRead_WithTheDueDays()
    {
        await using var db = CreateDb();
        SeedTypes(db);
        await db.SaveChangesAsync();

        var result = await WaitingAsync(db, now: Now);

        result.Items.Should().BeEmpty();
        result.Oldest.Should().BeNull();
        result.OverdueCount.Should().Be(0);
        result.DueDays.Should().Be(7);
    }

    /// <summary>A legacy Mini-CEX requested of the assessor, which he may accept: waiting on him since <paramref name="since" />.</summary>
    private static void AddRequested(Wombat.Infrastructure.Persistence.ApplicationDbContext db, int id, DateTime since)
        => AddFiled(db, id, WbaTypeId, "requested", TraineeId, NamesTheAssessor, since,
            Move("submit", "draft", "requested", TraineeId, since));

    /// <summary>An MSF row in draft, which a Coordinator records by her role.</summary>
    private static void AddMsfDraft(Wombat.Infrastructure.Persistence.ApplicationDbContext db, int id, string subject, DateTime since)
        => AddFiled(db, id, MsfTypeId, "draft", subject, "{}", since);
}
