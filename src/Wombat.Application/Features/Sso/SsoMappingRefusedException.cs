namespace Wombat.Application.Features.Sso;

/// <summary>Why a mapping's create or delete was refused (T376, flow 12: the SSO mappings page's words by reason, never by text).</summary>
public enum SsoMappingRefusal
{
    /// <summary>The provider is not set up, or serves another institution (T288).</summary>
    ProviderNotTheInstitutions,

    /// <summary>Administrator is never given by a mapping: it always needs a manual assignment.</summary>
    AdministratorNotMappable,

    /// <summary>Trainee and Pending trainee are admission's alone (T333).</summary>
    AdmissionRoleNotMappable,

    /// <summary>Institutional admin, mapped by anyone but the Administrator (T376, Q19, PROPOSED).</summary>
    InstitutionalAdminNotMappable,

    /// <summary>A role a mapping cannot give at an institution: College admin (a College's, not an institution's), or no role at all.</summary>
    RoleNotMappable,

    /// <summary>A speciality or sub-speciality the role takes none of: Institutional admin's, or a sub-speciality for a role kept to a speciality (T376 build review, B3).</summary>
    NotScopable,

    /// <summary>A speciality or sub-speciality the institution has not adopted, or a sub-speciality of another speciality (T376 build review, B3).</summary>
    NotAdopted,

    /// <summary>The role needs a speciality, or a speciality and sub-speciality, and the mapping names none (T376 build review, B3).</summary>
    ScopeNeeded,

    /// <summary>The mapping is not there, or is outside the caller's scope: the two read the same (404 words).</summary>
    NotFound,

    /// <summary>The mapping's provider is no longer set up for its institution, and only the Administrator deletes it (T376, S5).</summary>
    ProviderGone
}

/// <summary>The field of the mapping form a refusal sits under (T376, S22: beside its field, never a summary).</summary>
public enum SsoMappingField
{
    /// <summary>No one field: the page shows the refusal in its result.</summary>
    None,
    Provider,
    Role,
    Speciality,
    SubSpeciality
}

/// <summary>
/// A refusal of a mapping's create or delete, meant for the administrator who asked (T376, flow 12, O06, S5). The page maps
/// <see cref="Reason" /> and <see cref="Field" /> to where the words go; the words are the message, which names nobody.
/// </summary>
/// <remarks>
/// Derived from <see cref="InvalidOperationException" />, as <c>InvitationIssueRefusedException</c> is, so nothing that
/// catches a refusal changes and the audit row keeps its message. A page shows this type's words, and treats any other
/// <see cref="InvalidOperationException" /> as an unexpected failure: a fixed sentence, logged (T329).
/// </remarks>
public sealed class SsoMappingRefusedException : InvalidOperationException
{
    public SsoMappingRefusedException(SsoMappingRefusal reason, SsoMappingField field, string message)
        : base(message)
    {
        Reason = reason;
        Field = field;
    }

    public SsoMappingRefusal Reason { get; }

    public SsoMappingField Field { get; }
}

/// <summary>Which exceptions from the SSO mappings commands are refusals, whose words the page shows (T376, T329).</summary>
public static class SsoMappingRefusals
{
    /// <summary>
    /// A scope refusal (<see cref="UnauthorizedAccessException" />, in the handler's words, the T278/T290 note among them)
    /// or the mapping's own (<see cref="SsoMappingRefusedException" />). Anything else is an unexpected failure.
    /// </summary>
    public static bool IsRefusal(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is UnauthorizedAccessException or SsoMappingRefusedException;
    }
}
