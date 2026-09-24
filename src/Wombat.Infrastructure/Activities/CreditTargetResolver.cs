using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// The one implementation of "which curriculum items would this activity credit". It is shared by the credit engine
/// and the EPA→tool gate (T122), so the gate can never refuse, or pass, a different item from the one credit lands on.
/// </summary>
/// <remarks>
/// <para>
/// Extracted verbatim from <see cref="CreditApplier" />, including its tracking query, so the engine's behaviour and
/// the rebuild's are unchanged. It is permission-agnostic on purpose. Tool permission is applied by
/// <see cref="ToolPermissionGate" /> on the write path and nowhere else. Putting it in here would make the engine
/// re-litigate a tool list at credit time, and a rebuild would then delete credit retroactively after an allow-list
/// edit, which is exactly what D20 rules out.
/// </para>
/// <para>
/// The EPA picker applies the same profile pick through <see cref="PickProfileAsync" />, so the three cannot choose
/// different curricula for a user who somehow holds two profiles. It applies the same
/// <see cref="CurriculumItemsInForce" /> predicate too, so an EPA it does not offer is one credit cannot land on (T158).
/// </para>
/// </remarks>
internal static class CreditTargetResolver
{
    /// <summary>
    /// The trainee profile credit accrues against: the trainee's preferred profile
    /// (<see cref="TraineeScopeResolver.PreferredProfiles" />), the active one, else the highest id.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Deliberately NOT filtered on IsActive. TraineeProfile.Complete() clears IsActive on graduation, so filtering
    /// here meant a graduated trainee earned no credit — harmless for live submissions (they no longer submit), but
    /// destructive under RebuildCurriculumProgress, which zeroes every progress row before replaying: alumni would
    /// come back with nothing.
    /// </para>
    /// <para>
    /// Until T185 this broke ties by the latest programme start, then the id, while the scope stamped on every
    /// activity, the portfolio export and the STAR curriculum took the preferred profile. The database allows one
    /// active profile per trainee, so the two disagreed only between past profiles: a trainee with no current profile
    /// whose later-created one started earlier. Credit then landed on one programme's curriculum while the export
    /// and the committee read the other's. There is one pick now, and this is it.
    /// </para>
    /// </remarks>
    public static async Task<TraineeProfile?> PickProfileAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        CancellationToken cancellationToken)
        => await TraineeScopeResolver.PreferredProfiles(dbContext)
            .AsNoTracking()
            .Where(profile => profile.UserId == traineeUserId)
            .FirstOrDefaultAsync(cancellationToken);

    public static async Task<TraineeContext?> ResolveTraineeAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        DateOnly observedOn,
        CancellationToken cancellationToken)
    {
        var profile = await PickProfileAsync(dbContext, traineeUserId, cancellationToken);
        if (profile is null)
        {
            return null;
        }

        return new TraineeContext(
            profile.CurriculumId,
            profile.InstitutionId,
            profile.GetStage(observedOn));
    }

    public static async Task<IReadOnlyList<CurriculumItem>> ResolveCurriculumItemsAsync(
        IApplicationDbContext dbContext,
        CurriculumItemMatchRule matchRule,
        JsonElement data,
        TraineeContext trainee,
        CancellationToken cancellationToken)
        => (await ResolveCurriculumItemsWithSourceAsync(dbContext, matchRule, data, trainee, cancellationToken)).Items;

    /// <summary>
    /// The items a directive matches, and the schema field whose value produced the match: the
    /// <c>curriculum_item_field</c>, the <c>epa_field</c>, or null for a literal <c>curriculum_item_id</c>.
    /// </summary>
    /// <remarks>
    /// The field is what a refusal message leads with. It is resolved here, by the same precedence that produced the
    /// items, rather than re-derived by the caller. The precedence includes the fall-through from an unparseable
    /// <c>curriculum_item_field</c> to <c>epa_field</c>.
    /// </remarks>
    public static async Task<(IReadOnlyList<CurriculumItem> Items, string? MatchedFieldKey)> ResolveCurriculumItemsWithSourceAsync(
        IApplicationDbContext dbContext,
        CurriculumItemMatchRule matchRule,
        JsonElement data,
        TraineeContext trainee,
        CancellationToken cancellationToken)
    {
        // Every match is confined to the trainee's adopted curriculum version (national core) plus their
        // own institution's local extras. This prevents credit leaking across curriculum versions or
        // onto another institution's local items that happen to share an EPA. (T091 phase 4.)
        //
        // And to items in force (T158). A deactivated EPA is not offered by the picker and is no target on any
        // progress page, so it takes no credit either: a completion while it is inactive matches nothing, and the
        // transition is stamped zero. Here rather than in CreditApplier, so the tool gate reads it the same way: an
        // inactive EPA is, to both, an EPA with no item on the trainee's curriculum.
        var scoped = dbContext.Set<CurriculumItem>()
            .InForce()
            .Where(entity => entity.CurriculumId == trainee.CurriculumId
                && (entity.OwningInstitutionId == null || entity.OwningInstitutionId == trainee.InstitutionId));

        var target = DescribeTarget(matchRule, data);
        switch (target.Kind)
        {
            case CreditTargetKind.LiteralItem:
                return (await scoped.Where(entity => entity.Id == target.Value).ToListAsync(cancellationToken), null);

            case CreditTargetKind.ItemField:
                return (await scoped.Where(entity => entity.Id == target.Value).ToListAsync(cancellationToken), target.SourceField);

            case CreditTargetKind.EpaField:
                return (await scoped.Where(entity => entity.EpaId == target.Value).ToListAsync(cancellationToken), target.SourceField);

            default:
                return ([], null);
        }
    }

    /// <summary>
    /// What a directive's target IS for this data, by the engine's precedence and nothing else: a literal
    /// <c>curriculum_item_id</c>; else a parseable <c>curriculum_item_field</c>; else a parseable <c>epa_field</c>
    /// (the fall-through included); else nothing.
    /// </summary>
    /// <remarks>
    /// Pure, so the tool gate can compare a directive's target before and after a move without reading the database,
    /// and by construction in the same terms credit resolves it. Two values that parse to the same id (<c>5</c> and
    /// <c>"5"</c>) describe the same target.
    /// </remarks>
    public static CreditTarget DescribeTarget(CurriculumItemMatchRule matchRule, JsonElement data)
    {
        if (matchRule.CurriculumItemId.HasValue)
        {
            return new CreditTarget(CreditTargetKind.LiteralItem, null, matchRule.CurriculumItemId.Value);
        }

        if (data.ValueKind == JsonValueKind.Object)
        {
            if (!string.IsNullOrWhiteSpace(matchRule.CurriculumItemField) &&
                TryGetInt32(data, matchRule.CurriculumItemField, out var curriculumItemId))
            {
                return new CreditTarget(CreditTargetKind.ItemField, matchRule.CurriculumItemField, curriculumItemId);
            }

            if (!string.IsNullOrWhiteSpace(matchRule.EpaField) &&
                TryGetInt32(data, matchRule.EpaField, out var epaId))
            {
                return new CreditTarget(CreditTargetKind.EpaField, matchRule.EpaField, epaId);
            }
        }

        return new CreditTarget(CreditTargetKind.None, null, 0);
    }

    public static bool TryGetInt32(JsonElement root, string fieldKey, out int value)
    {
        if (root.TryGetProperty(fieldKey, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
            {
                return true;
            }

            if (property.ValueKind == JsonValueKind.String &&
                int.TryParse(property.GetString(), CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
        }

        value = default;
        return false;
    }
}

/// <summary>How a directive's target was found. See <see cref="CreditTargetResolver.DescribeTarget" />.</summary>
internal enum CreditTargetKind
{
    None = 0,
    LiteralItem,
    ItemField,
    EpaField
}

/// <summary>
/// A directive's target for some data: how it was found, the schema field it came from (null for a literal item or
/// none), and the id it names (an item id or an EPA id, by <see cref="Kind" />).
/// </summary>
internal readonly record struct CreditTarget(CreditTargetKind Kind, string? SourceField, int Value);

/// <summary>What credit needs to know about the trainee an activity is about, on the day it happened.</summary>
internal sealed record TraineeContext(int CurriculumId, int InstitutionId, int? Stage);
