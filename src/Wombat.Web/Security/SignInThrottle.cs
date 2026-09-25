using System.Collections.Concurrent;
using System.Globalization;
using Wombat.Infrastructure.Http;

namespace Wombat.Web.Security;

/// <summary>
/// The sign-in throttle: ten failed password checks in five minutes from one client address, shared by the sign-in page,
/// the link-your-account page and the change-password page (T097, T149, T265). Only failures count (T156).
/// </summary>
/// <remarks>
/// <para>
/// Identity's lockout caps the guesses at one account. This caps them from one client, so a password spray across many
/// accounts is slowed too. Until T156 it was an ASP.NET rate-limiter policy: ten requests in five minutes from one /24,
/// successes included. A hospital's NAT or a carrier's shared /24 had ten sign-ins in five minutes between everyone on
/// it, and anyone on it could use them up. It now counts one address (<see cref="ClientAddress" />: an IPv6 client by its
/// /64), and a sign-in that succeeds costs nothing.
/// </para>
/// <para>
/// An attempt is counted when it begins, before its password is checked, and taken off the count when its password turns
/// out right (<see cref="SignInAttempt.Release" />). Counting only once a failure was known would let a client send any
/// number of guesses at once, all checked before the first was counted; this way no more than ten are ever in flight or
/// failed. An attempt nobody releases stays counted, so a fault part-way through a check counts as a failure, and so does
/// a refusal made without a check.
/// </para>
/// <para>
/// A fixed window per client: it starts with the client's first attempt and lasts five minutes, and a client that has
/// used up its ten is refused until it ends. Windows that have ended are swept out once a window, so the table holds only
/// the clients of the last ten minutes. It lives in the process, as the rate limiter's partitions did: a restart forgets
/// it.
/// </para>
/// </remarks>
public sealed class SignInThrottle
{
    /// <summary>Failed password checks one client may make in a <see cref="Window" />.</summary>
    public const int FailureLimit = 10;

    /// <summary>How long a client's count lasts, from its first attempt.</summary>
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, Tally> _tallies = new(StringComparer.Ordinal);
    private readonly TimeProvider _clock;
    private long _nextSweepTicks;

    public SignInThrottle(TimeProvider clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Begins a password check for the request's client: counted now, or refused because the client has used up its
    /// failures (<see cref="SignInAttempt.Refused" />).
    /// </summary>
    public SignInAttempt Begin(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var client = ClientAddress.Of(context);
        var now = _clock.GetUtcNow();
        SweepIfDue(now);

        while (true)
        {
            var tally = _tallies.GetOrAdd(client, static (_, ends) => new Tally(ends), now + Window);
            lock (tally)
            {
                if (tally.Retired)
                {
                    // Swept out between the lookup and the lock; the next lookup adds a fresh one.
                    continue;
                }

                if (now >= tally.Ends)
                {
                    tally.StartNewWindow(now + Window);
                }

                if (tally.Count >= FailureLimit)
                {
                    return SignInAttempt.RefusedUntil(tally.Ends - now);
                }

                tally.Count++;
                return new SignInAttempt(tally, tally.Generation);
            }
        }
    }

    /// <summary>The clients the table holds: those whose windows have not been swept out.</summary>
    internal int TrackedClients => _tallies.Count;

    /// <summary>
    /// Refuses the request as the throttle does: a <c>Retry-After</c> of the seconds left in the client's window, at least
    /// one.
    /// </summary>
    public static void SetRetryAfter(HttpResponse response, SignInAttempt refused)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(refused);

        var seconds = Math.Max(1, (int)Math.Ceiling(refused.RetryAfter.TotalSeconds));
        response.Headers.RetryAfter = seconds.ToString(CultureInfo.InvariantCulture);
    }

    private void SweepIfDue(DateTimeOffset now)
    {
        var due = Interlocked.Read(ref _nextSweepTicks);
        if (now.UtcTicks < due
            || Interlocked.CompareExchange(ref _nextSweepTicks, (now + Window).UtcTicks, due) != due)
        {
            return;
        }

        foreach (var (client, tally) in _tallies)
        {
            lock (tally)
            {
                if (now < tally.Ends)
                {
                    continue;
                }

                tally.Retired = true;
                _tallies.TryRemove(new KeyValuePair<string, Tally>(client, tally));
            }
        }
    }

    /// <summary>One client's count in its current window. Read and written under its own lock.</summary>
    internal sealed class Tally(DateTimeOffset ends)
    {
        public DateTimeOffset Ends { get; private set; } = ends;

        /// <summary>Failures recorded and checks in flight.</summary>
        public int Count { get; set; }

        /// <summary>
        /// Moves on with each new window, so an attempt that began in an earlier one and is released late is not taken off
        /// this one's count.
        /// </summary>
        public long Generation { get; private set; }

        /// <summary>Swept out of the table; a request that still holds it looks it up again.</summary>
        public bool Retired { get; set; }

        public void StartNewWindow(DateTimeOffset ends)
        {
            Ends = ends;
            Count = 0;
            Generation++;
        }
    }
}

/// <summary>One password check, counted against its client until it is released (<see cref="SignInThrottle" />).</summary>
public sealed class SignInAttempt
{
    private readonly SignInThrottle.Tally? _tally;
    private readonly long _generation;
    private int _released;

    internal SignInAttempt(SignInThrottle.Tally tally, long generation)
    {
        _tally = tally;
        _generation = generation;
    }

    private SignInAttempt(TimeSpan retryAfter)
    {
        Refused = true;
        RetryAfter = retryAfter;
    }

    /// <summary>The client has used up its failures: no password is to be checked.</summary>
    public bool Refused { get; }

    /// <summary>For a refusal, how long until the client's window ends.</summary>
    public TimeSpan RetryAfter { get; }

    internal static SignInAttempt RefusedUntil(TimeSpan retryAfter) => new(retryAfter);

    /// <summary>
    /// The password was right: the attempt is taken off the count, once. Only a success releases one. An attempt never
    /// released stays counted as a failure, and that includes a refusal made without checking a password: an account that
    /// signs in only through its institution, and the link page's refusals of an address it will not link. Releasing those
    /// would let the count say which addresses have accounts (T156 review).
    /// </summary>
    public void Release()
    {
        if (_tally is null || Interlocked.Exchange(ref _released, 1) == 1)
        {
            return;
        }

        lock (_tally)
        {
            if (!_tally.Retired && _tally.Generation == _generation && _tally.Count > 0)
            {
                _tally.Count--;
            }
        }
    }
}
