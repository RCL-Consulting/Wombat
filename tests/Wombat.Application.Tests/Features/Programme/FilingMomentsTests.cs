using FluentAssertions;
using Wombat.Application.Features.Programme.Filing;
using static Wombat.Application.Tests.Features.Programme.ProgrammeCast;

namespace Wombat.Application.Tests.Features.Programme;

/// <summary>
/// T358 (flow 06, E5): when an activity was filed, and "Nothing filed in 30 days". Filed is having left draft, by the move
/// the filing's lateness is judged by (T160), or the create when the create was the filing; the 30 days start at the later
/// of admission and today − 30.
/// </summary>
public sealed class FilingMomentsTests
{
    private static readonly DateTime TenDaysAgo = DAt10.AddDays(-10);
    private static readonly DateTime FortyDaysAgo = DAt10.AddDays(-40);

    [Fact]
    public async Task ADraft_IsNotFiled_AndACancelledDraftNeither()
    {
        var (db, _) = Build();
        await using var _db = db;
        AddActivity(db, DraftType, Mahlangu, TenDaysAgo, "draft");
        AddActivity(db, DraftType, Mahlangu, TenDaysAgo, "cancelled", moves: [("draft", "cancelled", "cancel", TenDaysAgo.AddHours(1))]);
        db.SaveChanges();

        var filed = await FilingMoments.LastFiledAsync(db, [Mahlangu], CancellationToken.None);

        filed.Should().BeEmpty("a draft, and a draft withdrawn before it was submitted, were never filed");
    }

    [Fact]
    public async Task ADraftBornRequest_IsFiledWhenSent()
    {
        var (db, _) = Build();
        await using var _db = db;
        var sent = TenDaysAgo.AddDays(3);
        AddActivity(db, DraftType, Mahlangu, TenDaysAgo, "submitted", moves: [("draft", "submitted", "submit", sent)]);
        db.SaveChanges();

        var filed = await FilingMoments.LastFiledAsync(db, [Mahlangu], CancellationToken.None);

        filed.Should().Equal(new Dictionary<string, DateTime> { [Mahlangu] = sent });
    }

    [Fact]
    public async Task ATypeBornRequested_IsFiledAtItsCreate_EvenWhenItIsCancelledLater()
    {
        var (db, _) = Build();
        await using var _db = db;
        AddActivity(db, RequestedType, Ndlovu, FortyDaysAgo, "cancelled", moves: [("requested", "cancelled", "cancel", TenDaysAgo)]);
        AddActivity(db, LoggedType, Dlamini, TenDaysAgo, "logged");
        db.SaveChanges();

        var filed = await FilingMoments.LastFiledAsync(db, [Ndlovu, Dlamini], CancellationToken.None);

        filed.Should().Equal(new Dictionary<string, DateTime>
        {
            [Ndlovu] = FortyDaysAgo,
            [Dlamini] = TenDaysAgo
        }, "a request is filed by its create, and so is a record born terminal; a later cancel files nothing");
    }

    // T358, build review R2: the create was read as the filing when the registrar, not its creator, holds the move out of
    // draft, so a draft someone else opened about the registrar read as filed at its create. A create is the filing only
    // when the type's initial state is no draft: born requested, or born terminal.
    [Fact]
    public async Task ADraftSomeoneElseCreatedAboutTheRegistrar_IsNotFiled_TillTheRegistrarSubmitsIt()
    {
        const int SubjectSubmitsType = 5;
        var (db, _) = Build();
        await using var _db = db;
        AddType(db, SubjectSubmitsType, "subject_submits", "Subject submits", DraftWorkflow.Replace(
            "\"key\": \"submit\", \"from\": \"draft\", \"to\": \"submitted\", \"actor\": \"subject|creator\"",
            "\"key\": \"submit\", \"from\": \"draft\", \"to\": \"submitted\", \"actor\": \"subject\"",
            StringComparison.Ordinal));
        AddActivity(db, SubjectSubmitsType, Mahlangu, FortyDaysAgo, "draft", createdBy: "zulu");
        var sent = TenDaysAgo.AddDays(2);
        AddActivity(db, SubjectSubmitsType, Ndlovu, FortyDaysAgo, "submitted", createdBy: "zulu",
            moves: [("draft", "submitted", "submit", sent)]);
        db.SaveChanges();

        var filed = await FilingMoments.LastFiledAsync(db, [Mahlangu, Ndlovu], CancellationToken.None);

        filed.Should().Equal(new Dictionary<string, DateTime> { [Ndlovu] = sent },
            "a draft is not filed whoever created it; the registrar's submit files it");
    }

    [Fact]
    public async Task AReturnAndReSubmission_IsOneFiling_AtTheFirst()
    {
        var (db, _) = Build();
        await using var _db = db;
        var first = FortyDaysAgo.AddDays(1);
        AddActivity(
            db,
            DraftType,
            Molefe,
            FortyDaysAgo,
            "submitted",
            moves:
            [
                ("draft", "submitted", "submit", first),
                ("submitted", "draft", "return", TenDaysAgo),
                ("draft", "submitted", "submit", TenDaysAgo.AddHours(2))
            ]);
        db.SaveChanges();

        var filed = await FilingMoments.LastFiledAsync(db, [Molefe], CancellationToken.None);

        filed[Molefe].Should().Be(first, "the re-submission after a supervisor's return is not a second filing (T160)");
    }

    [Fact]
    public async Task ARecordedMsf_Counts()
    {
        var (db, _) = Build();
        await using var _db = db;
        AddActivity(
            db, MsfType, DuPlessis, TenDaysAgo, "recorded", createdBy: "smit", dataJson: "{}",
            moves: [("draft", "recorded", "record", TenDaysAgo)]);
        db.SaveChanges();

        var filed = await FilingMoments.LastFiledAsync(db, [DuPlessis], CancellationToken.None);

        filed[DuPlessis].Should().Be(TenDaysAgo);
    }

    [Fact]
    public async Task TheLatestFilingIsRead_AndOnlyForTheRegistrarsAsked()
    {
        var (db, _) = Build();
        await using var _db = db;
        AddActivity(db, LoggedType, Dlamini, FortyDaysAgo, "logged");
        AddActivity(db, LoggedType, Dlamini, TenDaysAgo, "logged");
        AddActivity(db, LoggedType, Molefe, TenDaysAgo, "logged");
        db.SaveChanges();

        var filed = await FilingMoments.LastFiledAsync(db, [Dlamini], CancellationToken.None);

        filed.Should().Equal(new Dictionary<string, DateTime> { [Dlamini] = TenDaysAgo });
    }

    [Theory]
    [InlineData(10, false)]
    [InlineData(29, false)]
    [InlineData(30, true)]
    [InlineData(31, true)]
    public void ARegistrarWithNothingFiled_IsListedOnlyOnce30DaysHavePassedSinceAdmission(int admittedDaysAgo, bool listed)
        => FilingMoments.NothingFiled(D.AddDays(-admittedDaysAgo), lastFiledOn: null, D).Should().Be(listed);

    [Fact]
    public void AFilingInTheWindow_ByTheSouthAfricanDay_KeepsARegistrarOffTheList()
    {
        var admitted = D.AddDays(-200);
        var windowStart = D.AddDays(-FilingMoments.WindowDays);

        // 22:30 UTC on the day before the window is already the window's first day in South Africa.
        var lateEvening = windowStart.AddDays(-1).ToDateTime(new TimeOnly(22, 30), DateTimeKind.Utc);
        var dayBefore = windowStart.AddDays(-1).ToDateTime(new TimeOnly(8, 0), DateTimeKind.Utc);

        FilingMoments.NothingFiled(admitted, lateEvening, D).Should().BeFalse();
        FilingMoments.NothingFiled(admitted, dayBefore, D).Should().BeTrue();
        FilingMoments.NothingFiled(admitted, DAt10, D).Should().BeFalse();
    }
}
