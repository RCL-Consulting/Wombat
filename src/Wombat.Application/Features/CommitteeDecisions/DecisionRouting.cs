using System.Diagnostics.CodeAnalysis;
using Wombat.Application.Common.Security;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Which panel takes an EPA's entrustment decision for a trainee: the one routing rule (T131, Decision 3).
/// </summary>
/// <remarks>
/// <para>
/// <b>Eligibility</b> (<see cref="IsEligible" />): a panel may review a trainee when it runs at the trainee's institution
/// and is either institution-wide or covers the trainee's speciality. The institution half is T182's; the speciality half
/// is T194 item 2, folded in here, because before it a Paediatrics panel accepted a Surgery trainee at the same hospital.
/// A speciality id is national (T091), so it counts only together with the institution, which this checks first.
/// Scheduling asks it (<see cref="CommitteeTraineeScope.MayScheduleFor" />), and so does everything below.
/// </para>
/// <para>
/// <b>Routing</b> (<see cref="RoutesTo(CurriculumItem, DecisionPanel, TraineeScope?, IEnumerable{DecisionPanel})" />): an
/// item whose EPA the College gives to a named committee (<see cref="CurriculumItem.DecisionBodyKey" />, EPAs 4 and 5 to the
/// neonatal CCC) goes to an eligible panel sitting as that body (<see cref="DecisionPanel.DecisionBodyKey" />), one that
/// covers the trainee's speciality before an institution-wide one. It goes to the eligible general panels only when no
/// such panel exists, which is the brief's fallback: an institution that has not set up a neonatal panel still decides
/// EPAs 4 and 5. An item with no body goes to the eligible general panels, never to a body's panel: the neonatal CCC
/// decides the EPAs the College gives it and nothing else.
/// </para>
/// <para>
/// The unique index on (institution, speciality, body) leaves at most one institution-wide panel and one panel per
/// speciality sitting as a body, so a body-tagged item routes to exactly one panel wherever any carries its tag. Ties that
/// only an unconstrained store could produce go to the lowest id, so the answer is still one panel.
/// </para>
/// <para>
/// Pure: it reads only what it is handed. The committee routing card (<see cref="GetCommitteeRoutingQuery" />) asks it of
/// every panel at an institution, and the review agenda (slice 4) will ask it when a line is added, so what the card says
/// and what the agenda does cannot drift apart.
/// </para>
/// </remarks>
public static class DecisionRouting
{
    /// <summary>
    /// Whether <paramref name="panel" /> may review <paramref name="trainee" />: at the trainee's institution, and
    /// institution-wide or covering the trainee's speciality. False for a trainee with no profile (null).
    /// </summary>
    public static bool IsEligible(DecisionPanel panel, [NotNullWhen(true)] TraineeScope? trainee)
    {
        ArgumentNullException.ThrowIfNull(panel);

        if (trainee is null || panel.InstitutionId != trainee.InstitutionId)
        {
            return false;
        }

        return panel.Scope switch
        {
            DecisionPanelScope.Institution => true,
            DecisionPanelScope.Speciality => panel.SpecialityId is int specialityId && trainee.SpecialityId == specialityId,
            _ => false
        };
    }

    /// <summary>
    /// Whether <paramref name="item" />'s entrustment decision for <paramref name="trainee" /> is taken by
    /// <paramref name="panel" />.
    /// </summary>
    /// <param name="item">The curriculum item; only its <see cref="CurriculumItem.DecisionBodyKey" /> is read.</param>
    /// <param name="panel">The panel asked about.</param>
    /// <param name="trainee">Where the trainee trains, or null when they hold no profile.</param>
    /// <param name="bodyPanels">
    /// At least every panel at the trainee's institution that sits as a decision body. Others are ignored, and so is
    /// anything ineligible, so a caller may hand over all of an institution's panels.
    /// </param>
    public static bool RoutesTo(
        CurriculumItem item,
        DecisionPanel panel,
        TraineeScope? trainee,
        IEnumerable<DecisionPanel> bodyPanels)
    {
        ArgumentNullException.ThrowIfNull(item);
        return RoutesTo(item.DecisionBodyKey, panel, trainee, bodyPanels);
    }

    /// <summary>
    /// <see cref="RoutesTo(CurriculumItem, DecisionPanel, TraineeScope?, IEnumerable{DecisionPanel})" /> for an item read
    /// as a projection: <paramref name="decisionBodyKey" /> is the item's <see cref="CurriculumItem.DecisionBodyKey" />.
    /// </summary>
    public static bool RoutesTo(
        string? decisionBodyKey,
        DecisionPanel panel,
        TraineeScope? trainee,
        IEnumerable<DecisionPanel> bodyPanels)
    {
        ArgumentNullException.ThrowIfNull(panel);
        ArgumentNullException.ThrowIfNull(bodyPanels);

        if (!IsEligible(panel, trainee))
        {
            return false;
        }

        var bodyKey = DecisionBody.NormalizeKey(decisionBodyKey);
        if (bodyKey is null)
        {
            return IsGeneral(panel);
        }

        // The panel itself is always a candidate, so a caller that hands over too few body panels cannot make a body's
        // own panel refuse its EPAs.
        var bodyPanel = BodyPanelFor(bodyKey, trainee, bodyPanels.Append(panel));
        return bodyPanel is null ? IsGeneral(panel) : bodyPanel.Id == panel.Id;
    }

    /// <summary>
    /// The panel sitting as <paramref name="decisionBodyKey" /> that decides for <paramref name="trainee" />, or null when
    /// no eligible panel sits as it, and the item falls back to the general panels. A panel covering the trainee's
    /// speciality comes before an institution-wide one.
    /// </summary>
    public static DecisionPanel? BodyPanelFor(
        string? decisionBodyKey,
        TraineeScope? trainee,
        IEnumerable<DecisionPanel> bodyPanels)
    {
        ArgumentNullException.ThrowIfNull(bodyPanels);

        var bodyKey = DecisionBody.NormalizeKey(decisionBodyKey);
        if (bodyKey is null || trainee is null)
        {
            return null;
        }

        return bodyPanels
            .Where(candidate =>
                string.Equals(DecisionBody.NormalizeKey(candidate.DecisionBodyKey), bodyKey, StringComparison.Ordinal) &&
                IsEligible(candidate, trainee))
            .OrderBy(candidate => candidate.Scope == DecisionPanelScope.Speciality ? 0 : 1)
            .ThenBy(candidate => candidate.Id)
            .FirstOrDefault();
    }

    /// <summary>A panel that sits as no decision body: the trainee's general committee.</summary>
    public static bool IsGeneral(DecisionPanel panel)
    {
        ArgumentNullException.ThrowIfNull(panel);
        return DecisionBody.NormalizeKey(panel.DecisionBodyKey) is null;
    }
}
