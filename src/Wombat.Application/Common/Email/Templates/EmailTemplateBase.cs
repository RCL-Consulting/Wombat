namespace Wombat.Application.Common.Email.Templates;

/// <summary>
/// Wraps a plain-text body in a minimal HTML email shell.
/// All templates produce both HTML and plain-text bodies.
/// </summary>
internal static class EmailTemplateBase
{
    // $$""" raw string: CSS braces stay literal; {{expr}} = interpolation.
    internal static string WrapHtml(string title, string bodyHtml) => $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>{{HtmlEncode(title)}}</title>
          <style>
            body { font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 0; }
            .wrapper { max-width: 600px; margin: 32px auto; background: #fff; border-radius: 6px; padding: 32px; }
            h1 { font-size: 1.25rem; color: #1a1a2e; margin-top: 0; }
            p { color: #333; line-height: 1.6; }
            a.btn { display: inline-block; margin-top: 16px; padding: 10px 20px; background: #2563eb; color: #fff; text-decoration: none; border-radius: 4px; }
            .footer { margin-top: 32px; font-size: 0.8rem; color: #888; }
          </style>
        </head>
        <body>
          <div class="wrapper">
            <h1>{{HtmlEncode(title)}}</h1>
            {{bodyHtml}}
            <p class="footer">This is an automated message from Wombat. Please do not reply to this email.</p>
          </div>
        </body>
        </html>
        """;

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
