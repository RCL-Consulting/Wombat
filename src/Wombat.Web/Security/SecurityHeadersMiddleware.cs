using System.Security.Cryptography;

namespace Wombat.Web.Security;

/// <summary>
/// Emits the response security headers Wombat's security contract promises: a nonce-backed
/// Content-Security-Policy plus the standard hardening headers.
/// </summary>
/// <remarks>
/// The nonce exists because <c>&lt;ImportMap /&gt;</c> renders an inline
/// <c>&lt;script type="importmap"&gt;</c> block, which <c>script-src 'self'</c> alone would
/// block — taking Blazor's module loading down with it. Every request gets a fresh nonce,
/// published on <see cref="HttpContext.Items"/> under <see cref="NonceItemKey"/> for
/// <c>App.razor</c> to stamp onto that tag.
/// </remarks>
public sealed class SecurityHeadersMiddleware
{
    /// <summary>Key under which the per-request CSP nonce is published on <c>HttpContext.Items</c>.</summary>
    public const string NonceItemKey = "csp-nonce";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var nonce = GenerateNonce();
        context.Items[NonceItemKey] = nonce;

        var headers = context.Response.Headers;

        // Stop browsers second-guessing declared content types — the classic vector for
        // turning an uploaded or exported file into executable script.
        headers["X-Content-Type-Options"] = "nosniff";

        // Don't leak portfolio / trainee ids in the Referer when a user follows an
        // external link out of the app.
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Belt-and-braces alongside CSP frame-ancestors, for older browsers.
        headers["X-Frame-Options"] = "SAMEORIGIN";

        // style-src keeps 'unsafe-inline' because ~51 components carry style="..."
        // attributes, which CSP counts as inline styles. script-src — the directive that
        // actually mitigates XSS — takes no such exemption.
        headers["Content-Security-Policy"] = string.Join("; ", new[]
        {
            "default-src 'self'",
            $"script-src 'self' 'nonce-{nonce}'",
            "style-src 'self' 'unsafe-inline'",
            "img-src 'self' data:",
            "font-src 'self'",
            // 'self' covers the same-origin WebSocket that carries the Blazor Server circuit.
            "connect-src 'self'",
            "object-src 'none'",
            "base-uri 'self'",
            "form-action 'self'",
            "frame-ancestors 'self'",
        });

        await _next(context);
    }

    private static string GenerateNonce()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
