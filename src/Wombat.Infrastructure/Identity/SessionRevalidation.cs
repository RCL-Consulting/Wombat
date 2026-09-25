using Microsoft.AspNetCore.Identity;

namespace Wombat.Infrastructure.Identity;

/// <summary>
/// How soon a change to an account reaches the sessions already signed in to it: one minute. The sign-in cookie's security
/// stamp is checked against the account at most this long after the cookie was issued (<see cref="Configure" />, Identity's
/// <c>SecurityStampValidator</c>), and a Blazor circuit's sign-in is checked as often (Wombat.Web's
/// <c>SessionRevalidatingAuthenticationStateProvider</c>). (T279)
/// </summary>
/// <remarks>
/// <para>
/// Every change that should end a session changes the account's security stamp: an administrator's lock, an erasure, a
/// role added or removed, a move to another institution or a change of scope, a password changed or reset. A cookie or a
/// circuit whose stamp is no longer the account's is signed out at its next check. A check the stamp passes builds the
/// cookie's principal again from the account, so its roles and scope are never older than this either.
/// </para>
/// <para>
/// Until T279 no interval was set, so Identity's default of thirty minutes applied to the cookie, and nothing checked a
/// circuit at all. The lifecycle browser check (2026-09-25) saw an erased trainee's open session go on loading their pages
/// with the full Trainee navigation for about thirty minutes, and a circuit kept the claims it started with for as long as
/// its tab stayed open. Every gate that reads the caller's claims (their roles, institution and scope) trusted them for as
/// long.
/// </para>
/// <para>
/// <b>Why one minute</b>, the shortest of the one to five minutes T279 weighed. What it costs: a request that arrives more than a minute after
/// its cookie was issued reads the account's stamp, and on a pass builds the principal again (the account, its roles, its
/// scopes and whether it has a trainee record: a handful of indexed reads) and issues the cookie again. A Blazor Server
/// session makes few HTTP requests (a page load, the circuit's connection, the page's assets), so that is a few reads a
/// minute for each active user, and one read a minute for each open circuit. Wombat serves the trainees, assessors and
/// committees of a training programme, hundreds of people, not thousands at once, so the load is negligible. What five
/// minutes would buy is fewer of those reads; what it would cost is four more minutes in which a locked or erased account,
/// or someone who has just lost a role, keeps working through every gate that trusts the claims.
/// </para>
/// <para>
/// A circuit is checked this long after it started, and every interval after, so a change reaches it within one interval
/// of the circuit's start or its last check; the cookie it started from was itself checked no more than an interval before.
/// </para>
/// </remarks>
public static class SessionRevalidation
{
    /// <summary>The longest a cookie or a circuit goes unchecked against the account it signs in.</summary>
    public static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    /// <summary>The sign-in cookie's check: every host that adds Identity (AddInfrastructure) configures it with this.</summary>
    public static void Configure(SecurityStampValidatorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.ValidationInterval = Interval;
    }
}
