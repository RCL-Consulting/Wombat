using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Security;

/// <summary>
/// T156: the sign-in throttle counts failed password checks from one client address, ten in five minutes. Until T156 it
/// was a rate-limiter policy on the client's /24 that counted every request, successes included. The endpoints that use
/// it are driven against the whole app in the integration suite (<c>SignInThrottleFlowTests</c>).
/// </summary>
public sealed class SignInThrottleTests
{
    private const string Client = "203.0.113.7";

    private readonly MovableClock _clock = new();
    private readonly SignInThrottle _throttle;

    public SignInThrottleTests()
    {
        _throttle = new SignInThrottle(_clock);
    }

    [Fact]
    public void TenFailures_RefuseTheEleventh_UntilTheWindowEnds()
    {
        Fail(Client, SignInThrottle.FailureLimit);

        _clock.MoveForward(TimeSpan.FromMinutes(2));
        var refused = _throttle.Begin(From(Client));
        refused.Refused.Should().BeTrue();
        refused.RetryAfter.Should().Be(TimeSpan.FromMinutes(3), "the window runs five minutes from the client's first attempt");

        _clock.MoveForward(TimeSpan.FromMinutes(3));
        _throttle.Begin(From(Client)).Refused.Should().BeFalse("the window has ended, and a new one begins");
    }

    [Fact]
    public void ASignInThatSucceeds_CostsNothing()
    {
        // A morning's sign-ins behind one hospital's address.
        for (var signIn = 0; signIn < 50; signIn++)
        {
            var attempt = _throttle.Begin(From(Client));
            attempt.Refused.Should().BeFalse($"sign-in {signIn + 1} succeeds and is not counted");
            attempt.Release();
        }

        Fail(Client, SignInThrottle.FailureLimit);
        _throttle.Begin(From(Client)).Refused.Should().BeTrue("ten failures are still ten");
    }

    [Fact]
    public void TheWholeAddressIsCounted_NotItsSlash24()
    {
        Fail(Client, SignInThrottle.FailureLimit);

        _throttle.Begin(From(Client)).Refused.Should().BeTrue();
        _throttle.Begin(From("203.0.113.8")).Refused.Should().BeFalse("a neighbour on the same /24 has failures of its own");
        _throttle.Begin(From("::ffff:203.0.113.7")).Refused.Should().BeTrue("the same IPv4 client, through a dual-stack socket");
    }

    [Fact]
    public void AnIPv6Client_IsCountedByItsSlash64_WhichItCanMoveWithinFreely()
    {
        for (var failure = 1; failure <= SignInThrottle.FailureLimit; failure++)
        {
            // A new address in the same /64 for every guess.
            _throttle.Begin(From($"2001:db8:1:2::{failure:x}")).Refused.Should().BeFalse();
        }

        _throttle.Begin(From("2001:db8:1:2:ffff::1")).Refused.Should().BeTrue();
        _throttle.Begin(From("2001:db8:1:3::1")).Refused.Should().BeFalse("another /64 is another client");
    }

    [Fact]
    public void ChecksStillInFlight_AreCounted_SoNoMoreThanTenGuessesAreEverOut()
    {
        // Ten guesses sent at once, none answered yet.
        var inFlight = Enumerable.Range(0, SignInThrottle.FailureLimit).Select(_ => _throttle.Begin(From(Client))).ToList();
        inFlight.Should().OnlyContain(attempt => !attempt.Refused);

        _throttle.Begin(From(Client)).Refused.Should().BeTrue("an eleventh waits for one of the ten to be answered");

        inFlight[0].Release();
        _throttle.Begin(From(Client)).Refused.Should().BeFalse("the one that succeeded made room");
    }

    [Fact]
    public void ReleasingAnAttemptTwice_TakesItOffTheCountOnce()
    {
        Fail(Client, SignInThrottle.FailureLimit - 1);
        var attempt = _throttle.Begin(From(Client));

        attempt.Release();
        attempt.Release();

        _throttle.Begin(From(Client)).Refused.Should().BeFalse();
        _throttle.Begin(From(Client)).Refused.Should().BeTrue("nine failures, one released twice, and one more is ten");
    }

    [Fact]
    public void AnAttemptReleasedAfterItsWindowWasSweptOut_IsNotTakenOffTheNextWindowsCount()
    {
        var late = _throttle.Begin(From(Client));

        _clock.MoveForward(SignInThrottle.Window);
        Fail(Client, SignInThrottle.FailureLimit);

        late.Release();

        _throttle.Begin(From(Client)).Refused.Should().BeTrue("the late release belonged to the window before");
    }

    [Fact]
    public void AnAttemptReleasedAfterItsWindowStartedAgain_IsNotTakenOffTheNextWindowsCount()
    {
        // The sweep runs once a window, so a client whose window ends between two sweeps starts its next one in place.
        _throttle.Begin(From("198.51.100.1")).Release();
        _clock.MoveForward(TimeSpan.FromMinutes(1));
        var late = _throttle.Begin(From(Client));
        _clock.MoveForward(TimeSpan.FromMinutes(4));
        _throttle.Begin(From("198.51.100.1")).Release();
        _clock.MoveForward(TimeSpan.FromMinutes(1));

        Fail(Client, SignInThrottle.FailureLimit);
        _throttle.TrackedClients.Should().Be(2, "guard: the client's window started again in place, not swept out");

        late.Release();

        _throttle.Begin(From(Client)).Refused.Should().BeTrue("the late release belonged to the window before");
    }

    [Fact]
    public void ARefusal_SaysHowLongToWait_InWholeSecondsRoundedUp()
    {
        Fail(Client, SignInThrottle.FailureLimit);
        _clock.MoveForward(TimeSpan.FromSeconds(100.2));

        var refused = _throttle.Begin(From(Client));
        var response = new DefaultHttpContext().Response;
        SignInThrottle.SetRetryAfter(response, refused);

        response.Headers.RetryAfter.ToString().Should().Be("200");
    }

    [Fact]
    public void WindowsThatHaveEnded_AreSweptOut()
    {
        for (var client = 1; client <= 20; client++)
        {
            Fail($"198.51.100.{client}", 1);
        }

        _throttle.TrackedClients.Should().Be(20);

        _clock.MoveForward(SignInThrottle.Window);
        _throttle.Begin(From(Client)).Release();

        _throttle.TrackedClients.Should().Be(1, "only the client of the current window is held");
    }

    [Fact]
    public void RequestsWithoutAnAddress_AreCountedTogether()
    {
        Fail(null, SignInThrottle.FailureLimit);

        _throttle.Begin(From(null)).Refused.Should().BeTrue();
    }

    private void Fail(string? address, int times)
    {
        for (var failure = 0; failure < times; failure++)
        {
            _throttle.Begin(From(address)).Refused.Should().BeFalse($"guard: failure {failure + 1} is checked");
        }
    }

    private static DefaultHttpContext From(string? address)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = address is null ? null : IPAddress.Parse(address);
        return context;
    }

    private sealed class MovableClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 9, 25, 8, 0, 0, TimeSpan.Zero);

        public void MoveForward(TimeSpan by) => _now += by;

        public override DateTimeOffset GetUtcNow() => _now;
    }
}
