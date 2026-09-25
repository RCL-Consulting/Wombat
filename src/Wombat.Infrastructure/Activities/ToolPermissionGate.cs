using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// The write-path half of the EPA→tool allow-list (T122, D20): refuses an activity whose credit target is an
/// EPA its instrument may not credit.
/// </summary>
/// <remarks>
/// <para>
/// Reads only, and throws. Every caller awaits it BEFORE its first mutation. The audit pipeline's catch saves the
/// request's shared DbContext, so a refusal thrown after a mutation would commit that mutation. The curriculum items
/// the resolver loads are tracked but never modified, so the audit save writes nothing for them.
/// </para>
/// <para>
/// It answers with the credit engine's own resolver (<see cref="CreditTargetResolver" />). That covers the same
/// profile, the same curriculum and owner scoping, the same string-or-number parse, and the same fall-through from
/// <c>curriculum_item_field</c> to <c>epa_field</c>. So an item the gate passes is exactly an item credit can land on,
/// now or once a deactivated EPA is reactivated (see below).
/// It is the authority where the picker is not: a literal <c>curriculum_item_id</c> rule, a rule mixing
/// <c>curriculum_item_field</c> with <c>epa_field</c>, or an <c>epa_field</c> that is not an <c>epa</c>-typed field.
/// The picker narrows none of those. Since T137 the last two can no longer be saved or published
/// (<c>EvidenceEpa.EnsureCreditAgrees</c>), but a version pinned before then can still carry them, so the gate keeps
/// covering them.
/// </para>
/// <para>
/// What it judges is <see cref="GatedTargets(string?, FormSchema)" />: each credit directive's target, or, for a type whose
/// rules credit nothing, the EPA its schema says the activity is evidence for (T154). An unrated instrument (the
/// reflective exercise, the clinical audit, the portfolio review; D6, D7) credits nothing, but since T137 it is stamped
/// as evidence for the EPA it is filed against, and the committee reads that. The College's list for an EPA names the
/// instruments that are evidence for it, so an unrated instrument is held to the list exactly as a rated one is: the
/// same predicate, the same create / changed-target / author's-hand-on rule (<c>ActivityService.DirectivesToJudge</c>),
/// the same message. The EPA picker narrows the same field (<c>CreditRuleFields.ResolveNarrowedEpaFieldKeys</c>).
/// </para>
/// <para>
/// What it deliberately lets through:
/// <list type="bullet">
///   <item>A type with no <c>WbaToolKey</c>, or an item with no tool list: unrestricted (D21).</item>
///   <item>A type whose pinned rules credit nothing (<c>counts_for: []</c>) and whose schema names no evidence EPA
///   (<c>evidence_epa_field</c>). It can credit no item and is filed against no EPA, so there is nothing to judge.</item>
///   <item>A subject with no trainee profile, or an EPA with no item on their curriculum. Nothing would be credited
///   today, and refusing would turn this gate into a curriculum-membership check that neither D20 nor D21 decided.
///   The boundary this leaves is recorded: a profile created or re-pointed after submission is not re-checked
///   when the activity completes, because credit never re-litigates (D20).</item>
/// </list>
/// </para>
/// <para>
/// What it does NOT let through, since T196: an instrument the list of a deactivated EPA forbids. A deactivated EPA pauses
/// credit (D48), and reactivating it credits every completion filed during the pause, so the gate judges the item the
/// activity will be credited under whether or not its EPA is in force now. Being out of force is never itself a refusal
/// (D48 rejected that as a curriculum-membership check): a permitted instrument files, and its credit waits for the
/// reactivation. Before T196 the gate read only items in force, so an EPA deactivated at the hand-on and reactivated
/// before completion credited a tool its list forbids (the T158 boundary, now closed).
/// </para>
/// </remarks>
internal static class ToolPermissionGate
{
    /// <summary>
    /// How many refused items the message names before it summarises the rest. A rule set crediting many items can
    /// refuse many at once, and the message is also the audit row's error text, which has a bounded column.
    /// </summary>
    internal const int MaxItemsNamed = 3;

    /// <param name="judgeDirective">
    /// Which of the pinned version's <see cref="GatedTargets(string?, FormSchema)" /> to judge, by index; null judges
    /// every one (the create). A move judges only the targets it changed, or whose target the mover could still correct,
    /// so a refusal never names a target that did not change or a field the mover cannot write.
    /// </param>
    public static async Task EnsurePermittedAsync(
        IApplicationDbContext dbContext,
        string? wbaToolKey,
        string? creditRulesJson,
        FormSchema schema,
        string subjectUserId,
        DateOnly observedOn,
        string dataJson,
        CancellationToken cancellationToken,
        Func<int, bool>? judgeDirective = null)
    {
        var toolKey = WbaTool.NormalizeKey(wbaToolKey);
        if (toolKey is null)
        {
            return;
        }

        var targets = GatedTargets(creditRulesJson, schema);
        if (targets.Count == 0)
        {
            return;
        }

        // observedOn only feeds the trainee's stage, which item resolution never reads. It is passed so the resolver
        // keeps one signature for the engine and the gate.
        var trainee = await CreditTargetResolver.ResolveTraineeAsync(dbContext, subjectUserId, observedOn, cancellationToken);
        if (trainee is null)
        {
            return;
        }

        using var document = JsonDocument.Parse(dataJson);

        // Refuse when ANY matched item is not permitted: the engine credits every item a rule matches, and an unrated
        // instrument's one target is the EPA it is evidence for.
        var refusals = new List<(CurriculumItem Item, string? MatchedFieldKey)>();
        var seen = new HashSet<int>();
        for (var index = 0; index < targets.Count; index++)
        {
            if (judgeDirective is not null && !judgeDirective(index))
            {
                continue;
            }

            // Every item the target names, in force or not (D48, T196). An EPA that is deactivated pauses credit, and
            // reactivating it credits what was filed meanwhile, so its item is where this activity's credit will land.
            // Judged here, the instrument cannot reach it through a pause. Not in force is never itself a refusal: that
            // would make this a curriculum-membership check, which D48 rejected.
            var (items, matchedFieldKey) = await CreditTargetResolver.ResolveCurriculumItemsWithSourceAsync(
                dbContext, targets[index], document.RootElement, trainee, inForceAt: null, cancellationToken);

            foreach (var item in items.Where(item => seen.Add(item.Id)))
            {
                if (ToolPermission.Evaluate(CurriculumItem.ParsePermittedTools(item.PermittedToolsJson), toolKey)
                    == ToolPermissionVerdict.NotPermitted)
                {
                    refusals.Add((item, matchedFieldKey));
                }
            }
        }

        if (refusals.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(await DescribeAsync(dbContext, toolKey, schema, refusals, cancellationToken));
    }

    /// <summary>
    /// What the gate judges for one pinned version, in an order the callers index by: every credit directive's match rule,
    /// or, when the rules credit nothing, the schema's <c>evidence_epa_field</c> as an <c>epa_field</c> target (T154).
    /// Empty when there is neither, and when the rules do not parse (see <see cref="TryParseRules" />).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The evidence EPA is a target only when nothing is credited. A type that credits by EPA must credit through the
    /// evidence field (<c>EvidenceEpa.EnsureCreditAgrees</c>), so for it the two are already the same target; one that
    /// credits a fixed or picked item is judged on the item, as before. Adding the pointer beside its directives would
    /// judge a pre-T137 pinned version on an EPA it never credited.
    /// </para>
    /// <para>
    /// Not a default for <c>epa_field</c>: the credit engine never reads this target, so an unrated instrument still
    /// credits nothing (D7). It is the list's other reading, "which instruments are evidence for this EPA", applied to
    /// the one EPA an unrated activity names.
    /// </para>
    /// </remarks>
    internal static IReadOnlyList<CurriculumItemMatchRule> GatedTargets(string? creditRulesJson, FormSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        if (!TryParseRules(creditRulesJson, out var rules))
        {
            return [];
        }

        if (rules.CountsFor.Count > 0)
        {
            return rules.CountsFor.Select(directive => directive.CurriculumItemMatchRule).ToList();
        }

        return string.IsNullOrWhiteSpace(schema.EvidenceEpaField)
            ? []
            : [new CurriculumItemMatchRule(schema.EvidenceEpaField, CurriculumItemId: null, CurriculumItemField: null)];
    }

    /// <summary>
    /// One sentence per refused item, up to <see cref="MaxItemsNamed" /> and then a count of the rest, led by the
    /// label of the field that produced the match (nothing for a literal <c>curriculum_item_id</c>), naming the
    /// instrument and the ones the curriculum accepts for that EPA.
    /// </summary>
    /// <remarks>
    /// Catalogue data only: EPA codes and titles and instrument names. The message lands in the audit row, because
    /// the audit pipeline records a failed command's exception message, and nothing clinical belongs there. It
    /// follows <c>ThrowIfActorFieldNamesSubject</c> in naming the field by its label, not its key: the page shows
    /// the message as one alert, and "epa_id:" means nothing to a registrar.
    /// </remarks>
    private static async Task<string> DescribeAsync(
        IApplicationDbContext dbContext,
        string toolKey,
        FormSchema schema,
        IReadOnlyList<(CurriculumItem Item, string? MatchedFieldKey)> refusals,
        CancellationToken cancellationToken)
    {
        var epaIds = refusals.Select(refusal => refusal.Item.EpaId).Distinct().ToArray();
        var epas = await dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epaIds.Contains(epa.Id))
            .Select(epa => new { epa.Id, epa.Code, epa.Title })
            .ToDictionaryAsync(epa => epa.Id, cancellationToken);

        var keys = refusals
            .SelectMany(refusal => CurriculumItem.ParsePermittedTools(refusal.Item.PermittedToolsJson))
            .Append(toolKey)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var names = await dbContext.Set<WbaTool>()
            .AsNoTracking()
            .Where(tool => keys.Contains(tool.Key))
            .ToDictionaryAsync(tool => tool.Key, tool => tool.Name, StringComparer.Ordinal, cancellationToken);

        string NameOf(string key) => names.TryGetValue(key, out var name) ? name : key;

        var sentences = refusals.Take(MaxItemsNamed).Select(refusal =>
        {
            var epa = epas.TryGetValue(refusal.Item.EpaId, out var found)
                ? $"{found.Code} — {found.Title}"
                : string.Format(CultureInfo.InvariantCulture, "EPA {0}", refusal.Item.EpaId);

            var accepted = JoinWithOr(CurriculumItem.ParsePermittedTools(refusal.Item.PermittedToolsJson)
                .Select(NameOf)
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToList());

            var sentence = $"{NameOf(toolKey)} cannot be used as evidence for {epa}. The curriculum accepts {accepted} for this EPA.";

            if (refusal.MatchedFieldKey is null)
            {
                return sentence;
            }

            return $"{schema.FieldLabel(refusal.MatchedFieldKey)}: {sentence}";
        });

        var message = string.Join(" ", sentences);
        var unnamed = refusals.Count - MaxItemsNamed;
        return unnamed <= 0
            ? message
            : $"{message} {NameOf(toolKey)} cannot be used for {unnamed} more {(unnamed == 1 ? "EPA" : "EPAs")} this activity would credit either.";
    }

    private static string JoinWithOr(IReadOnlyList<string> names)
        => names.Count switch
        {
            0 => "no instrument",
            1 => names[0],
            _ => $"{string.Join(", ", names.Take(names.Count - 1))} or {names[^1]}"
        };

    /// <summary>
    /// Fails open on a blank or unparseable rule set. A published version's rules always parse, because saving a
    /// draft round-trips them through the parser, and an unparseable set would fail at credit anyway. Refusing here
    /// would turn a malformed rule block into a filing error on every EPA.
    /// </summary>
    private static bool TryParseRules(string? creditRulesJson, out CreditRules rules)
    {
        rules = null!;
        if (string.IsNullOrWhiteSpace(creditRulesJson))
        {
            return false;
        }

        try
        {
            rules = CreditRulesParser.Parse(creditRulesJson);
            return true;
        }
        catch (CreditRulesParseException)
        {
            return false;
        }
    }
}
