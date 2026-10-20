namespace Wombat.Application.Common.Email.Templates;

/// <summary>
/// Tells a registrar that they have been admitted (T373, flow 11, Q7, decided T367): to which curriculum, in which training
/// year or from which day, and that their open sessions ended once. Sent by the admission after its save, through
/// <c>AccountNotices</c>, so a refused admission mails nobody and a failed send never fails the admission.
/// </summary>
/// <remarks>
/// No other data (R3-0-spec-2, "Mails"): not the institution's or the admitter's name, not the expected completion. Until
/// T373 admission sent no mail, and the registrar learnt of it only when her session ended (T279).
/// </remarks>
public static class AdmissionEmail
{
    public const string Subject = "You are admitted in Wombat";

    public const string SignedOutOnce =
        "If you were signed in to Wombat, you have been signed out once: sign in again to log activities and follow your progress.";

    /// <param name="trainingYear">The training year on the day of the admission; null when the programme has not started.</param>
    /// <param name="signInUrl">The sign-in page under the configured base URL.</param>
    public static EmailMessage Build(
        string toEmail,
        string curriculumName,
        string curriculumVersion,
        int? trainingYear,
        DateOnly programmeStart,
        string signInUrl)
    {
        var admitted = Admitted(curriculumName, curriculumVersion, trainingYear, programmeStart);

        var html = EmailTemplateBase.WrapHtml(Subject, $"""
            <p>{Encode(admitted)}</p>
            <p>{Encode(SignedOutOnce)}</p>
            <p><a class="btn" href="{Encode(signInUrl)}">Sign in</a></p>
            """);

        var text = $"""
            {admitted}

            {SignedOutOnce}

            {signInUrl}
            """;

        return new EmailMessage(
            To: toEmail,
            Subject: Subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["trainee", "admitted"]);
    }

    /// <summary>
    /// "You have been admitted to the Paediatric EPA Curriculum (11.1), in training year 4." or, before the programme
    /// starts, "… (11.1). Your programme starts on 2027-01-15."
    /// </summary>
    public static string Admitted(string curriculumName, string curriculumVersion, int? trainingYear, DateOnly programmeStart)
        => trainingYear is { } year
            ? $"You have been admitted to the {curriculumName} ({curriculumVersion}), in training year {year}."
            : $"You have been admitted to the {curriculumName} ({curriculumVersion}). Your programme starts on {programmeStart:yyyy-MM-dd}.";

    private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);
}
