using FluentAssertions;
using Wombat.Domain.Curricula;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Navigation;

namespace Wombat.Web.Tests.Shared;

/// <summary>
/// T400 (flow 18, K7, Q7; build-contracts § 0): <see cref="SastTime" />, the one place Web turns a UTC moment into South
/// African words, its day <see cref="ProgrammeCalendar.DateOf" />'s, and the two older helpers that now call it.
/// </summary>
public sealed class SastTimeTests
{
    private static readonly DateTime LateEvening = new(2026, 11, 10, 22, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime Afternoon = new(2026, 11, 10, 15, 1, 41, 861, DateTimeKind.Utc);

    [Fact]
    public void HalfPastTenUtc_IsHalfPastMidnightSast_OnTheNextDay()
    {
        SastTime.Minute(LateEvening).Should().Be("2026-11-11 00:30 SAST");
        SastTime.Second(LateEvening).Should().Be("2026-11-11 00:30:00 SAST");
        SastTime.Clock(LateEvening).Should().Be("00:30");
        SastTime.Day(LateEvening).Should().Be("2026-11-11");
        SastTime.Day(LateEvening).Should().Be(ProgrammeCalendar.DateOf(LateEvening).ToString("yyyy-MM-dd"),
            "a time and its day never disagree");
    }

    [Fact]
    public void EachForm_ReadsTheSouthAfricanClock()
    {
        SastTime.Minute(Afternoon).Should().Be("2026-11-10 17:01 SAST");
        SastTime.Second(Afternoon).Should().Be("2026-11-10 17:01:41 SAST");
        SastTime.Clock(Afternoon).Should().Be("17:01");
        SastTime.Day(Afternoon).Should().Be("2026-11-10");
    }

    [Fact]
    public void TheStoredMoment_IsUtcToTheMillisecond()
        => SastTime.StoredUtc(Afternoon).Should().Be("2026-11-10 15:01:41.861 UTC");

    [Fact]
    public void AMomentOfAnyKind_IsReadAsUtc_NeverTheServersZone()
    {
        var unspecified = DateTime.SpecifyKind(Afternoon, DateTimeKind.Unspecified);
        var local = DateTime.SpecifyKind(Afternoon, DateTimeKind.Local);

        SastTime.Minute(unspecified).Should().Be("2026-11-10 17:01 SAST");
        SastTime.Minute(local).Should().Be("2026-11-10 17:01 SAST");
    }

    [Fact]
    public void AnOffsetMoment_IsReadOnTheSouthAfricanClock()
    {
        var inLondon = new DateTimeOffset(2026, 9, 26, 13, 14, 0, TimeSpan.Zero);
        var inNewYork = new DateTimeOffset(2026, 9, 26, 9, 14, 0, TimeSpan.FromHours(-4));

        SastTime.Minute(inLondon).Should().Be("2026-09-26 15:14 SAST");
        SastTime.Minute(inNewYork).Should().Be("2026-09-26 15:14 SAST");
    }

    [Fact]
    public void ATypedSouthAfricanTime_IsTheUtcMomentTwoHoursEarlier_AndBack()
    {
        var typed = new DateTime(2026, 11, 11, 0, 30, 0, DateTimeKind.Unspecified);

        var utc = SastTime.FromSastInput(typed);

        utc.Should().Be(LateEvening);
        utc.Kind.Should().Be(DateTimeKind.Utc);
        var shown = SastTime.ToSastInput(utc);
        shown.Should().Be(typed);
        shown.Kind.Should().Be(DateTimeKind.Unspecified);
    }

    [Fact]
    public void ATypedTime_TaggedUtcByTheInput_IsStillReadAsSouthAfrican()
    {
        // AuditList.razor's datetime-local tags what was typed as UTC today (:185–186); the helper reads the clock it shows.
        var typed = new DateTime(2026, 11, 11, 0, 30, 0, DateTimeKind.Utc);

        SastTime.FromSastInput(typed).Should().Be(LateEvening);
    }

    [Fact]
    public void TheOlderHelpers_SayWhatTheyAlwaysSaid_ThroughIt()
    {
        ErrorPages.TimeOf(new DateTimeOffset(2026, 9, 26, 13, 14, 0, TimeSpan.Zero)).Should().Be("2026-09-26 15:14 SAST");
        ActivityListWords.DateOf(LateEvening).Should().Be("2026-11-11");
        ActivityListWords.DateOf(Afternoon).Should().Be("2026-11-10");
    }
}
