using System.Globalization;
using Wombat.Application.Features.Dashboards.InstitutionalAdmin;

namespace Wombat.Web.Components.Pages.Dashboards;

/// <summary>
/// Her Home's words (T376, flow 12, boards R3-H01–H05; Q7, Q8): the waiting line, People, Curriculum and Your institution.
/// Every count has its singular.
/// </summary>
public static class InstitutionalAdminHomeWords
{
    public const string PeopleTitle = "People";
    public const string CurriculumTitle = "Curriculum";
    public const string InstitutionTitle = "Your institution";
    public const string OpenUsers = "Open Users";
    public const string OpenTrainees = "Open Trainees";
    public const string OpenAdoptions = "Open Curriculum adoptions";

    /// <summary>Under Status: she reads it, a Wombat administrator sets it (Q3; Spec 3's Hint, NEW).</summary>
    public const string StatusSetBy = "Set by a Wombat administrator.";

    /// <summary>With nothing adopted (H02, 1.10).</summary>
    public const string NothingAdopted = "Nothing is adopted yet. Adopt the College's curriculum to set up a programme.";

    /// <summary>
    /// The waiting line's first sentence (Q8, H04), bold: "5 registrars are waiting to be admitted." The rest is
    /// <see cref="WaitingRest" />.
    /// </summary>
    public static string WaitingLead(int count)
        => count == 1
            ? "1 registrar is waiting to be admitted."
            : $"{Count(count)} registrars are waiting to be admitted.";

    /// <summary>The waiting line after its first sentence (D55: nothing is filed about a registrar not yet admitted).</summary>
    public const string WaitingRest = "Nothing can be filed about them until then.";

    /// <summary>People's footer note (K24): "15 accounts.", "1 account: yours." (H02).</summary>
    public static string Accounts(int count, bool onlyYours)
        => count == 1
            ? onlyYours ? "1 account: yours." : "1 account."
            : $"{Count(count)} accounts.";

    /// <summary>Curriculum's figures' labels (H01, H02): "speciality adopted", "specialities adopted".</summary>
    public static string SpecialitiesAdopted(int count) => count == 1 ? "speciality adopted" : "specialities adopted";

    public static string SubSpecialitiesAdopted(int count) => count == 1 ? "sub-speciality adopted" : "sub-specialities adopted";

    /// <summary>One adoption's line (H01): "Paediatrics / Paediatrics, from the CPSA's Paediatric EPA Curriculum (11.1)."</summary>
    public static string Adoption(AdoptedCurriculumItem item)
        => $"{item.SpecialityName} / {item.SubSpecialityName}, from the {item.CollegeShortCode}'s {item.CurriculumName} ({item.CurriculumVersion}).";

    /// <summary>The institution card's link (H01): "Open Kgosi Kgari Teaching Hospital".</summary>
    public static string OpenInstitution(string name) => $"Open {name}";

    /// <summary>Status in the card (H01).</summary>
    public static string Status(bool isActive) => isActive ? "Active" : "Inactive";

    /// <summary>A contact email not recorded (I07's cleared address).</summary>
    public const string NoContactEmail = "None recorded";

    private static string Count(int count) => count.ToString(CultureInfo.InvariantCulture);
}
