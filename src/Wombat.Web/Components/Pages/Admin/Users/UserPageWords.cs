namespace Wombat.Web.Components.Pages.Admin.Users;

/// <summary>
/// The user page's not-found words, as built (A.5.5, P23; T194's one answer for an unknown id and an out-of-scope one), in
/// one place since T376 (flow 12, build-contracts § 0): the user page (lane P) shows them for an id it cannot read, and the
/// old assessor form's route (lane T, S10) for a profile id it cannot follow, so a stale link reads the same wherever it
/// lands. The page's h1 and tab are "User" while it says so (S9: no name before a read).
/// </summary>
public static class UserPageWords
{
    /// <summary>The h1 and tab while no person is shown: loading, failed, or not found.</summary>
    public const string Heading = "User";

    /// <summary>The not-found panel's title.</summary>
    public const string UnavailableTitle = "User unavailable";

    /// <summary>The not-found panel's sentence: the same for an id that names nobody and one outside the caller's scope.</summary>
    public const string UnavailableBody = "The user could not be found or is outside your scope.";

    /// <summary>The not-found panel's way on (P23: "Back to users" in the header is gone).</summary>
    public const string GoToUsers = "Go to Users";
}
