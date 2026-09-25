using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Which decision panels one caller may create, change or open: the answer
/// <see cref="CommitteeDecisionAuthorization.PanelReachAsync" /> gives, read by the gate
/// (<see cref="CommitteeDecisionAuthorization.MayAdministerPanelAsync" />) and by the panel form's offer
/// (<see cref="GetDecisionPanelFormOptionsQuery" />). (T182, T131 slice 3, T194)
/// </summary>
/// <param name="EveryInstitution">A global Administrator: every panel, at every institution.</param>
/// <param name="InstitutionId">The one institution anyone else manages panels at, or null when they manage none.</param>
/// <param name="EveryPanelAtInstitution">An InstitutionalAdmin: every panel at <paramref name="InstitutionId" />.</param>
/// <param name="SpecialityIds">
/// A Speciality or SubSpecialityAdmin: the specialities whose Speciality-scoped panels at <paramref name="InstitutionId" />
/// they manage (their own speciality, or the one each of their sub-specialities belongs to). Never the institution-wide
/// panel.
/// </param>
internal sealed record PanelAdministrationReach(
    bool EveryInstitution,
    int? InstitutionId,
    bool EveryPanelAtInstitution,
    IReadOnlySet<int> SpecialityIds)
{
    /// <summary>A global Administrator's reach.</summary>
    public static PanelAdministrationReach EveryPanel { get; } = new(true, null, false, new HashSet<int>());

    /// <summary>The reach of someone who manages no panel: no institution of their own.</summary>
    public static PanelAdministrationReach None { get; } = new(false, null, false, new HashSet<int>());

    /// <summary>Whether a panel of this scope, covering this speciality, at this institution is within reach.</summary>
    public bool Admits(int institutionId, DecisionPanelScope scope, int? specialityId)
    {
        if (EveryInstitution)
        {
            return true;
        }

        if (InstitutionId != institutionId)
        {
            return false;
        }

        if (EveryPanelAtInstitution)
        {
            return true;
        }

        return scope == DecisionPanelScope.Speciality &&
               specialityId is int speciality &&
               SpecialityIds.Contains(speciality);
    }

    /// <summary>
    /// Whether every panel at some institution is within reach, the institution-wide one and every speciality's: a global
    /// Administrator's, or an InstitutionalAdmin's at their own. Otherwise at most the Speciality-scoped panels of
    /// <see cref="SpecialityIds" /> are.
    /// </summary>
    public bool ManagesEveryPanel
        => EveryInstitution || (InstitutionId is not null && EveryPanelAtInstitution);
}
