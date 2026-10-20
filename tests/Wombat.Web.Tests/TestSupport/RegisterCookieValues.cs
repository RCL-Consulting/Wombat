using Wombat.Web.Security;

namespace Wombat.Web.Tests.TestSupport;

/// <summary>The register cookie's value in a <c>Set-Cookie</c> header, as a browser would send it back (T373, flow 11, S1).</summary>
internal static class RegisterCookieValues
{
    /// <summary>The value of <see cref="RegisterCookie.CookieName" /> in <paramref name="setCookie" />; empty when it is deleted.</summary>
    public static string ValueIn(string setCookie)
    {
        var prefix = $"{RegisterCookie.CookieName}=";
        var start = setCookie.IndexOf(prefix, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException($"No {RegisterCookie.CookieName} in: {setCookie}");
        }

        start += prefix.Length;
        var end = setCookie.IndexOf(';', start);
        return end < 0 ? setCookie[start..] : setCookie[start..end];
    }
}
