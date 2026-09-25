namespace Wombat.Domain.Epas;

public sealed class Epa
{
    public int Id { get; set; }
    public int SubSpecialityId { get; set; }

    /// <summary>
    /// Null for a national (College-owned) EPA — the catalogue core. Set to an institution id for an
    /// institution-local supplementary EPA: institutions may add local extras to a discipline but can never
    /// edit the national core (T091, phase 3).
    /// </summary>
    public int? OwningInstitutionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The key a seeder finds this row by, such as <c>cpsa:paediatrics:epa:PAED-001</c>, or null for an EPA an
    /// administrator made (T221). Written when a seeder creates the row and never changed after: no command writes it, so
    /// neither a new code nor a move to another sub-speciality can lose it.
    /// </summary>
    public string? SeedKey { get; init; }

    /// <summary>
    /// The practice domain this EPA belongs to, as published by the owning College — for example
    /// the College of Paediatricians' five domains: "Ambulatory, Emergency and Critical Care in
    /// Children", "Care for the Newborn", "Chronic, Developmental and Preventive Care",
    /// "Leadership, Population and Systems Health", "Ethics, Communication, Education and
    /// Palliative Care". Null when the College publishes no domain grouping.
    /// </summary>
    /// <remarks>
    /// Deliberately a free-text label rather than an enum: domains are defined per College and
    /// differ between disciplines, so a fixed set would have to change whenever a new College
    /// adopts Wombat. Contrast <see cref="EpaCategory"/>, which is a Wombat-wide concept.
    /// </remarks>
    public string? Domain { get; set; }

    public string? Description { get; set; }
    public string? RequiredKnowledgeSkills { get; set; }
    public EpaCategory Category { get; set; } = EpaCategory.Core;

    /// <summary>
    /// Whether the EPA is in force now: offered by the pickers, a target on progress pages, and a place credit can land.
    /// Change it through <see cref="Deactivate" /> and <see cref="Reactivate" />, which keep <see cref="DeactivatedOn" />
    /// in step; the database refuses the two disagreeing (<c>CK_Epas_DeactivatedOn</c>).
    /// </summary>
    /// <remarks>
    /// Init-only (T196 review), so a handler holding a stored EPA cannot set the flag without the moment and meet the
    /// check only at save. An object initializer still can, which is how a test builds an inactive EPA with no recorded
    /// pause (<see cref="InForceAt" />); nothing in the product constructs one.
    /// </remarks>
    public bool IsActive
    {
        get => _isActive;
        init => _isActive = value;
    }

    // EF reads and writes the flag through this field (its convention for a property with a matching backing field).
    private bool _isActive = true;

    /// <summary>
    /// When the current deactivation began (UTC), or null while the EPA is active (T196, D48).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Deactivating pauses credit; it does not cancel it. A completion credits the EPA's items only while the EPA is in
    /// force at the moment of credit (<see cref="InForceAt" />), and reactivating credits every completion filed during
    /// the pause. So the credit a completion ends up with depends on one pause only, the current one: every earlier
    /// pause was closed by a reactivation that credited what it held. That is why one timestamp is the whole history the
    /// rule needs, and why no table of active periods is kept.
    /// </para>
    /// <para>
    /// It is also why deactivating an EPA that is already inactive keeps the first timestamp: moving it later would
    /// count completions that were paused.
    /// </para>
    /// </remarks>
    public DateTime? DeactivatedOn { get; private set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public Wombat.Domain.Institutions.SubSpeciality SubSpeciality { get; set; } = null!;
    public ICollection<Wombat.Domain.Curricula.CurriculumItem> CurriculumItems { get; set; } = [];
    /// <summary>
    /// Takes the EPA out of force from <paramref name="utcNow" />. A no-op on an EPA that is already inactive, which keeps
    /// the moment its pause began (see <see cref="DeactivatedOn" />).
    /// </summary>
    /// <returns>True when this call deactivated it.</returns>
    public bool Deactivate(DateTime utcNow)
    {
        if (!IsActive)
        {
            return false;
        }

        _isActive = false;
        DeactivatedOn = utcNow;
        return true;
    }

    /// <summary>
    /// Puts the EPA back in force and ends its pause. The caller credits the completions filed during the pause
    /// (<see cref="DeactivatedOn" />, read before this call), because the credit rule no longer tells them apart.
    /// </summary>
    /// <returns>True when this call reactivated it.</returns>
    public bool Reactivate()
    {
        if (IsActive)
        {
            return false;
        }

        _isActive = true;
        DeactivatedOn = null;
        return true;
    }

    /// <summary>
    /// Whether a completion credited at <paramref name="moment" /> may credit this EPA's items: the EPA is active, or the
    /// moment falls before its current pause began. The same rule as <c>CurriculumItemsInForce.InForceAt</c>, which is
    /// its query form.
    /// </summary>
    /// <remarks>
    /// An inactive EPA with no <see cref="DeactivatedOn" /> is in force at no moment. The database cannot hold one, but a
    /// test fixture that constructs one with <see cref="IsActive" /> false does, and it reads as the pre-T196 rule did.
    /// </remarks>
    public bool InForceAt(DateTime moment)
        => IsActive || (DeactivatedOn is { } pausedFrom && moment < pausedFrom);
}
