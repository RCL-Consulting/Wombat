using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

public sealed record EpaDto(
    int Id,
    int SubSpecialityId,
    string SubSpecialityName,
    string CollegeName,
    string Code,
    string Title,
    string? Description,
    string? RequiredKnowledgeSkills,
    EpaCategory Category,
    bool IsActive,
    DateTime CreatedOn)
{
    /// <summary>
    /// The institution whose own EPA this is, or null for a national EPA, the College's (T091; T392, Q3).
    /// </summary>
    public int? OwningInstitutionId { get; init; }

    /// <summary>
    /// The owning institution's name, for an institution's own EPA: the page names whose trainees deactivating it stops
    /// measuring (Q11, "… for Kgosi Kgari Teaching Hospital's trainees"). Null for a national EPA.
    /// </summary>
    public string? OwningInstitutionName { get; init; }

    /// <summary>
    /// Whether the EPA's own-mark names its owner ("Kgosi Kgari Teaching Hospital's own EPA") rather than saying "Your
    /// institution's own EPA": true for a caller whose reads span institutions, as the curriculum items' mark is decided
    /// (<c>CurriculumAdminScope.NamesItemOwners</c>, DESIGN.md § Table system, T211, T222). The query decides, never the page
    /// from the caller's roles.
    /// </summary>
    public bool NamesOwner { get; init; }

    /// <summary>
    /// Whether the caller may change the EPA: <see cref="UpdateEpaCommand" />'s own rule (a national EPA its College's
    /// CollegeAdmin or an Administrator; an institution's own EPA that institution's InstitutionalAdmin or an Administrator),
    /// never the caller's roles (T392, S4). The list offers Edit and the EPA's page its form only where it is true; elsewhere
    /// the row says "Set by the College." and the page is the EPA's record (Q2).
    /// </summary>
    public bool MayChange { get; init; }

    /// <summary>The discipline the EPA belongs to, as the pages write it: "Paediatrics / Paediatrics" (speciality / sub-speciality).</summary>
    public string DisciplineName { get; init; } = string.Empty;
}

/// <summary>Whose EPA list a caller reads, which picks the list's words (T392, R2-0-spec-2 § 8).</summary>
public enum EpaListReader
{
    /// <summary>An Administrator: every College's EPAs and every institution's own.</summary>
    Administrator,

    /// <summary>A CollegeAdmin: the College's national EPAs.</summary>
    College,

    /// <summary>An InstitutionalAdmin: the national EPAs of the disciplines the institution has adopted, and its own.</summary>
    Institution
}

/// <summary>The EPA list's page (T392, flow 15, lane C): its rows, read by the list's rule, and what the page may offer.</summary>
/// <param name="Epas">The EPAs the caller reads, each with its owner, discipline and "may change".</param>
/// <param name="Reader">Whose list it is, for the page's words.</param>
/// <param name="MayCreate">
/// Whether Create EPA can be completed (Q12, S4): true for an Administrator and a CollegeAdmin, and for an InstitutionalAdmin
/// only while the institution has actively adopted a discipline to add an EPA to.
/// </param>
/// <param name="InstitutionName">The InstitutionalAdmin's institution, which the subtitle names; null for the others.</param>
public sealed record EpaListDto(IReadOnlyList<EpaDto> Epas, EpaListReader Reader, bool MayCreate, string? InstitutionName);

/// <summary>A discipline an EPA may be created in or moved to: "Paediatrics / Paediatrics".</summary>
public sealed record EpaDisciplineDto(int SubSpecialityId, string Name);

/// <summary>
/// An EPA's page (T392, flow 15, lane C), read once: the EPA (null for a create, and for an id the caller cannot open, which
/// is the page's Unavailable, Q2, Q19), and what the form offers.
/// </summary>
/// <param name="Epa">The EPA by the list's rule, or null.</param>
/// <param name="Disciplines">
/// The sub-speciality picker's options: the disciplines the caller's create or move would be accepted in (the College's, an
/// institution's actively adopted ones, every one for an Administrator), and the EPA's own discipline where it is not among
/// them, since an own EPA left in a discipline no longer adopted keeps it (F9).
/// </param>
/// <param name="CreatesOwn">Whether an EPA this caller creates is the institution's own, not national (<see cref="CreateEpaCommand" />'s rule).</param>
/// <param name="InstitutionName">The institution a created own EPA belongs to, which the create page names; null otherwise.</param>
/// <param name="MayCreate">As <see cref="EpaListDto.MayCreate" />.</param>
public sealed record EpaPageDto(
    EpaDto? Epa,
    IReadOnlyList<EpaDisciplineDto> Disciplines,
    bool CreatesOwn,
    string? InstitutionName,
    bool MayCreate);
