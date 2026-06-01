namespace Wombat.Application.Common.Email.Templates;

public static class GraduationEmail
{
    public static EmailMessage Build(string toEmail, string traineeName, string programmeName, DateOnly completedOn)
    {
        var subject = $"Congratulations on completing {programmeName}";
        var encodedName = System.Net.WebUtility.HtmlEncode(traineeName);
        var encodedProgramme = System.Net.WebUtility.HtmlEncode(programmeName);

        var html = EmailTemplateBase.WrapHtml(subject, $"""
            <p>Dear {encodedName},</p>
            <p>Congratulations on completing your training programme,
               <strong>{encodedProgramme}</strong>, as of {completedOn:yyyy-MM-dd}.</p>
            <p>Your committee has ratified your final entrustment decisions and your portfolio of
               evidence is available in Wombat. We wish you every success in the next stage of your career.</p>
            """);

        var text = $"""
            Dear {traineeName},

            Congratulations on completing your training programme, {programmeName}, as of {completedOn:yyyy-MM-dd}.

            Your committee has ratified your final entrustment decisions and your portfolio of evidence is
            available in Wombat. We wish you every success in the next stage of your career.
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["graduation"]);
    }
}
