namespace Wombat.Application.Features.Activities;

/// <summary>
/// The release of Wombat that writes each system-managed activity type (T162; T398, flow 17, Q9, T334), in words: the
/// list's "Written by the MSF release." and the builder's release notice, and the guard's refusal (N10). Keyed by the
/// type's key, as <c>ActivityTypeSeedCatalogue</c> declares the two system-managed types.
/// </summary>
public static class SystemManagedReleases
{
    /// <summary>
    /// "the MSF release" for <c>msf_cpsa</c>, "the learner-feedback release" for <c>learner_feedback_cpsa</c>; null for
    /// any other key.
    /// </summary>
    public static string? WrittenBy(string key) => key switch
    {
        "msf_cpsa" => "the MSF release",
        "learner_feedback_cpsa" => "the learner-feedback release",
        _ => null
    };
}
