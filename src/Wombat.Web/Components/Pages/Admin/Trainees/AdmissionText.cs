using Wombat.Application.Features.Trainees;

namespace Wombat.Web.Components.Pages.Admin.Trainees;

/// <summary>
/// The words of Trainees and Admit ‹name› (T373, flow 11, T01–T10, M01–M15), in one place, so a page and its tests read
/// the same sentences. Straight apostrophes; the boards' "…" only on "Admit…".
/// </summary>
public static class AdmissionText
{
    /// <summary>A date as every page prints one: ISO, in the invariant culture whatever the host's (T373 fix pass, P12).</summary>
    public static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    // ── Trainees ───────────────────────────────────────────────────────────────

    public const string TraineesSubtitle = "Admit registrars who are waiting, and keep current trainees' programmes.";

    public const string WaitingHeading = "Waiting to be admitted";

    public const string WaitingCaption =
        "Registered from a Trainee invitation, by surname. A deactivated account is not listed. Nothing can be filed about " +
        "them until they are admitted.";

    public const string WaitingCaptionAdministrator =
        "Registered from a Trainee invitation at any institution, by surname. A deactivated account is not listed. Nothing " +
        "can be filed about them until they are admitted.";

    public const string NobodyWaiting = "Nobody is waiting to be admitted.";

    public const string NobodyWaitingBody = "A registrar appears here after registering from a Trainee invitation.";

    public const string CurrentHeading = "Current trainees";

    public const string CurrentCaption =
        "Admitted to a curriculum, by surname. Training years turn every 365 days from the programme start.";

    public const string NoCurrent = "No current trainees";

    public const string NoCurrentBody = "A registrar appears here once admitted.";

    public const string ClosedHeading = "Completed and closed";

    public const string TraineesLoading = "Loading the trainees.";

    public const string TraineesLoadFailed =
        "Could not load the trainees. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>A section's count beside its title: "5 waiting", "1 waiting".</summary>
    public static string Waiting(int count) => $"{count} waiting";

    /// <summary>"1 current", "4 current".</summary>
    public static string Current(int count) => $"{count} current";

    /// <summary>"Training year 4", under the programme start.</summary>
    public static string TrainingYear(int year) => $"Training year {year}";

    /// <summary>
    /// S8's result (T02–T04), its bold first sentence: "Lerato Molefe admitted to the Paediatric EPA Curriculum (11.1),
    /// training year 4, from 2023-01-15." Before the programme starts no training year is named.
    /// </summary>
    public static string Admitted(AdmissionResultDto result)
    {
        var where = $"{result.FullName} admitted to the {result.CurriculumName} ({result.CurriculumVersion})";
        return result.TrainingYear is { } year
            ? $"{where}, training year {year}, from {Iso(result.ProgrammeStart)}."
            : $"{where}, from {Iso(result.ProgrammeStart)}, when the programme starts.";
    }

    /// <summary>
    /// The result's second sentence, in plain weight, when the completion was derived: "Expected completion 2027-01-15,
    /// derived from the curriculum window: change it under Edit."; else null.
    /// </summary>
    public static string? AdmittedDerived(AdmissionResultDto result) => result.CompletionWasDerived
        ? $"Expected completion {Iso(result.ExpectedCompletion)}, derived from the curriculum window: change it under Edit."
        : null;

    // ── Admit ‹name› ───────────────────────────────────────────────────────────

    /// <summary>The page while it loads, when its read failed, and for the T290 note (check rule 2): no name before the read.</summary>
    public const string AdmitARegistrar = "Admit a registrar";

    public const string AdmitSubtitle = "Admit a registrar who is waiting into the institution's curriculum.";

    public const string NotFoundHeading = "Registrar not found";

    public const string NotFoundBody = "No registrar waiting for admission has this link.";

    public const string NotFoundNext = "Those waiting are listed on Trainees.";

    public const string GoToTrainees = "Go to Trainees";

    public const string AdmitLoading = "Loading this registrar.";

    public const string AdmitLoadFailed =
        "Could not load this registrar. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>An admission that failed for a reason that is not a refusal: logged, and said in these fixed words (S9, T329).</summary>
    public const string AdmitFailed =
        "Could not admit this registrar. Nothing has changed. Try again, or come back in a few minutes.";

    public const string WhoHeading = "Who is being admitted";

    public const string ProgrammeHeading = "Programme";

    public const string RequiredMarkLine = "* marks a field you must fill in.";

    public const string ChooseCurriculum = "Choose a curriculum";

    public const string ChooseCurriculumRefusal = "Choose a curriculum.";

    public const string StartRequired = "Enter the programme start.";

    public const string CompletionHelp = "Leave this empty to derive it from the curriculum window.";

    public const string StartLongAgo = "This start is more than four years ago. Check the date before you admit.";

    public const string BackToTrainees = "Back to trainees";

    public const string NotAdmitted = "Not admitted.";

    public const string OpenTrainees = "Open Trainees";

    public const string Lead = "When you admit:";

    public static string Admit(string name) => $"Admit {name}";

    /// <summary>F1's read-out help: "Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality."</summary>
    public static string AdoptedFor(string? institution, string? invitedAs)
        => $"Adopted at {institution} for {invitedAs}, the invited sub-speciality.";

    /// <summary>M14's help, for a registrar invited to no sub-speciality.</summary>
    public static string ChooseFromAdopted(string? institution)
        => $"This registrar was invited to no sub-speciality, so choose from the curricula {institution} has adopted. Admitting sets the registrar's scope from it.";

    /// <summary>The Training year output: "Training year 4", or M06's "Not started: the programme starts on 2027-01-15".</summary>
    public static string TrainingYearOutput(int? year, DateOnly start)
        => year is { } value ? TrainingYear(value) : $"Not started: the programme starts on {Iso(start)}";

    public static string TrainingYearHelp(DateOnly today)
        => $"Training years turn every 365 days from the programme start, counted to {Iso(today)}. It follows the date when you change it.";

    /// <summary>M03: "Left empty, it will be 2027-01-15."</summary>
    public static string LeftEmpty(DateOnly derived) => $"Left empty, it will be {Iso(derived)}.";

    /// <summary>
    /// M04, check rule 3: none adopted for the invited sub-speciality while other curricula are adopted. The institution-wide
    /// sentence (<see cref="NothingAdoptedAtAll" />) is for an institution that has adopted none.
    /// </summary>
    public static string NothingAdoptedForScope(string? institution, string? invitedAs, string name)
        => $"No curriculum is adopted at {institution} for {invitedAs}, so {name} cannot be admitted.";

    /// <summary>Today's sentence (<c>TraineeProfileEdit.razor:292–294</c> at <c>69a7df20</c>), with its link's words.</summary>
    public const string NothingAdoptedAtAllBefore = "This institution has adopted no curriculum yet, so no trainee can be admitted. Adopt one under ";

    public const string NothingAdoptedAtAllLink = "Curriculum adoptions";

    public const string NothingAdoptedAtAllAfter = ", then admit the trainee.";

    public const string NothingAdoptedAtAll = NothingAdoptedAtAllBefore + NothingAdoptedAtAllLink + NothingAdoptedAtAllAfter;

    /// <summary>The three lines of "When you admit:" (M01, M06's variant).</summary>
    public static IReadOnlyList<string> WhenYouAdmit(string name, string? curriculum, int? trainingYear, DateOnly start)
    {
        var on = curriculum is null ? "a Trainee" : $"a Trainee on the {curriculum}";
        return trainingYear is { } year
            ?
            [
                $"{name} becomes {on}, in training year {year}.",
                $"Any session {name} has open ends once, and {name} signs in again as a Trainee.",
                $"An email tells {name} so."
            ]
            :
            [
                $"{name} becomes {on}, whose programme starts on {Iso(start)}. Nothing can be filed before then.",
                $"Any session {name} has open ends once, and {name} signs in again as a Trainee.",
                $"An email tells {name} the day the programme starts."
            ];
    }

    public static string AlreadyAdmitted(string name) => $"{name} already has an active trainee profile.";

    public static string DeactivatedBefore(string name) => $"{name}'s account is deactivated. Reactivate it under ";

    public const string DeactivatedLink = "Users";

    public const string DeactivatedAfter = " first.";
}
