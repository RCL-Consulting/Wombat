using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// The checks on the College decision body a panel sits as, shared by creating a panel and by
/// <see cref="SetDecisionPanelBodyCommand" />, and the one mapping of a panel to its form's DTO. (T131 slice 3)
/// </summary>
internal static class DecisionPanelBodies
{
    /// <summary>The refusal for a key that names no decision body.</summary>
    internal const string UnknownBody = "Choose a College committee from the list.";

    /// <summary>
    /// The decision body <paramref name="decisionBodyKey" /> names, or null when it is blank (a general panel). Refuses
    /// a key that names no body, and a body another panel at this institution already sits as for the same speciality
    /// (or for the whole institution), naming that panel. Reads only, so a caller runs it before its first mutation.
    /// </summary>
    /// <param name="dbContext">The store.</param>
    /// <param name="decisionBodyKey">The key asked for, as typed.</param>
    /// <param name="institutionId">The panel's institution.</param>
    /// <param name="specialityId">The speciality the panel covers, or null for an institution-wide panel.</param>
    /// <param name="panelId">The panel being changed, which may keep its own body; null for a new panel.</param>
    /// <param name="cancellationToken">Cancels the reads.</param>
    /// <exception cref="InvalidOperationException">The key names no body, or the body is taken.</exception>
    public static async Task<DecisionBody?> DemandAsync(
        IApplicationDbContext dbContext,
        string? decisionBodyKey,
        int institutionId,
        int? specialityId,
        int? panelId,
        CancellationToken cancellationToken)
    {
        var key = DecisionBody.NormalizeKey(decisionBodyKey);
        if (key is null)
        {
            return null;
        }

        var body = await dbContext.Set<DecisionBody>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Key == key, cancellationToken)
            ?? throw new InvalidOperationException(UnknownBody);

        // The unique index on (institution, speciality, body) holds against a race; this names the panel in the way.
        var taken = await TakenRefusalAsync(dbContext, body, institutionId, specialityId, panelId, cancellationToken);
        if (taken is not null)
        {
            throw new InvalidOperationException(taken);
        }

        return body;
    }

    /// <summary>
    /// The refusal naming the panel that already sits as <paramref name="body" /> for this institution and speciality
    /// (or for the whole institution when <paramref name="specialityId" /> is null), other than
    /// <paramref name="panelId" />; null when the slot is free. Reads only: nothing here saves.
    /// </summary>
    /// <remarks>
    /// <see cref="DemandAsync" /> asks it before a mutation. A handler also asks it after its save is refused
    /// (<see cref="Microsoft.EntityFrameworkCore.DbUpdateException" />), because the unique index on (institution,
    /// speciality, body) is what holds against a race: two InstitutionalAdmins tagging two institution-wide panels at the
    /// same moment both pass the check, the index refuses the second save, and without this the page printed EF's own
    /// text ("An error occurred while saving the entity changes…"). The handler wraps the database's exception in this
    /// message, so the audit pipeline still sees a refused save and writes its failure row without re-sending the
    /// refused change (T201). The refused change is still tracked then, which a read does not flush. (T131 slice 3)
    /// </remarks>
    public static async Task<string?> TakenRefusalAsync(
        IApplicationDbContext dbContext,
        DecisionBody body,
        int institutionId,
        int? specialityId,
        int? panelId,
        CancellationToken cancellationToken)
    {
        var holder = await dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .Where(panel => panel.InstitutionId == institutionId &&
                            panel.SpecialityId == specialityId &&
                            panel.DecisionBodyKey == body.Key &&
                            (panelId == null || panel.Id != panelId))
            .OrderBy(panel => panel.Id)
            .Select(panel => panel.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return holder is null
            ? null
            : $"{holder} already sits as the {body.Name} " +
              $"{(specialityId is null ? "for the whole institution" : "for this speciality")}. An institution has one " +
              "such panel, and one per speciality: change that panel instead.";
    }

    /// <summary>The panel form's DTO. <paramref name="decisionBodyName" /> is the name of the body it sits as, if any.</summary>
    public static DecisionPanelDetailDto ToDetailDto(DecisionPanel panel, string? decisionBodyName)
        => new(
            panel.Id,
            panel.Name,
            panel.Scope,
            panel.InstitutionId,
            panel.SpecialityId,
            panel.Members
                .OrderBy(member => member.Role)
                .ThenBy(member => member.UserId, StringComparer.Ordinal)
                .Select(member => new DecisionPanelMemberDto(member.Id, member.UserId, member.Role))
                .ToArray(),
            panel.DecisionBodyKey,
            panel.DecisionBodyKey is null ? null : decisionBodyName);

    /// <summary>The name of the body a panel sits as, or null for a general panel.</summary>
    public static async Task<string?> NameOfAsync(
        IApplicationDbContext dbContext,
        string? decisionBodyKey,
        CancellationToken cancellationToken)
        => decisionBodyKey is null
            ? null
            : await dbContext.Set<DecisionBody>()
                .AsNoTracking()
                .Where(body => body.Key == decisionBodyKey)
                .Select(body => body.Name)
                .FirstOrDefaultAsync(cancellationToken);
}
