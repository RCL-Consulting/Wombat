using System.Globalization;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;

namespace Wombat.Web.Components.Pages.Admin.ActivityTypes;

/// <summary>
/// The words both activity-type pages print (T398, flow 17, build-contracts § 0): the list (lane B) and the builder (lane
/// C) name an owner, a scope, a release, a version and a draft alike. Boards R2-A01, R2-B06, R2-B09, R3-C07; T300's notice
/// is moved here from <c>ActivityTypeEdit.razor</c>, word for word. Words no board draws are marked NEW.
/// </summary>
public static class ActivityTypeWords
{
    /// <summary>
    /// Whose type it is, the owner line's and the list's Set by cell's first line (R2-A01, R3-C07): the College by its short
    /// code ("CPSA"), the institution by its name ("Kgosi Kgari Teaching Hospital"), "The platform" for a Global type (T12).
    /// A target that no longer exists has no name: "Its College" or "Its institution" (NEW).
    /// </summary>
    public static string SetBy(ActivityTypeOwnerDto owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        return owner.Kind switch
        {
            ActivityTypeOwnerKind.College => owner.Name ?? "Its College",
            ActivityTypeOwnerKind.Institution => owner.Name ?? "Its institution",
            _ => "The platform"
        };
    }

    /// <summary>
    /// The scope under the owner: "Speciality · Paediatrics", "Sub-speciality · Paediatrics / Neonatology", "Institution"
    /// (the institution is the owner, so not named twice), "Global" (R2-A01, R3-C07). The target is never named by its id:
    /// a speciality that no longer exists reads "Speciality".
    /// </summary>
    public static string ScopeDetail(ActivityScope scope, string? targetName) => scope switch
    {
        ActivityScope.Speciality => WithTarget("Speciality", targetName),
        ActivityScope.SubSpeciality => WithTarget("Sub-speciality", targetName),
        ActivityScope.Institution => "Institution",
        _ => "Global"
    };

    /// <summary>
    /// Whose type a reader is looking at, and that they read it here and change nothing (T300; moved from
    /// <c>ActivityTypeEdit.razor</c>, word for word). A Speciality or SubSpeciality type is its College's (T091), a Global
    /// one the Administrator's, an Institution one its institution's.
    /// </summary>
    public static string ReadOnlyNotice(ActivityScope scope, string? targetName)
    {
        var whose = scope switch
        {
            ActivityScope.Speciality or ActivityScope.SubSpeciality => targetName is null
                ? "Set by the College."
                : $"Set by the College that owns {targetName}.",
            ActivityScope.Institution => targetName is null ? "Set by its institution." : $"Set by {targetName}.",
            _ => "Set by a global administrator."
        };

        return $"{whose} You can read this activity type here, but not change it.";
    }

    /// <summary>The list's line under a system-managed type's name (R2-A01, Q9): "Written by the MSF release.".</summary>
    public static string WrittenByLine(string writtenBy) => $"Written by {writtenBy}.";

    /// <summary>
    /// The builder's standing notice on a system-managed type (R2-B06, Q9, T22): "Written by the MSF release. Its form
    /// changes only with a release of Wombat. Nobody is offered it when filing an activity.".
    /// </summary>
    public static string ReleaseNotice(string writtenBy)
        => $"{WrittenByLine(writtenBy)} Its form changes only with a release of Wombat. Nobody is offered it when filing an activity.";

    /// <summary>The builder's Unavailable (R2-B09): an id that names no type, or one the caller may not open.</summary>
    public static class Unavailable
    {
        public const string Heading = "Activity type unavailable";

        public const string Body = "This activity type does not exist, or you cannot open it.";

        public const string GoTo = "Go to Activity types";
    }

    /// <summary>The published version: "Version 1", or "Not published" (R2-A01; "v0" gone).</summary>
    public static string Published(int version)
        => version > 0 ? string.Create(CultureInfo.InvariantCulture, $"Version {version}") : "Not published";

    /// <summary>
    /// The draft: "Saved 2026-11-05", the South African day it was saved (<see cref="ProgrammeCalendar.DateOf" />), or
    /// "None" (R2-A01, R2-A05).
    /// </summary>
    public static string Draft(DateTime? savedOnUtc)
        => savedOnUtc is DateTime saved
            ? $"Saved {ProgrammeCalendar.DateOf(saved).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
            : "None";

    private static string WithTarget(string scope, string? targetName) => targetName is null ? scope : $"{scope} · {targetName}";
}
