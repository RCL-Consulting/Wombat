using MediatR;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The curriculum item editor's server, as the page sees it: the queries it loads and the commands it sends, which are
/// recorded rather than run. Shared by the tool-list tests (T122) and the minima tests (T125).
/// </summary>
internal sealed class CurriculumItemsFakeSender : IScopedSender
{
    public const int CurriculumId = 5;
    public const int SubSpecialityId = 7;

    // Ordered by Name, as GetWbaToolsQuery returns it.
    public static readonly IReadOnlyList<WbaToolDto> Vocabulary =
    [
        new("cbd", "Case-based discussion", null),
        new("dops", "DOPS", null),
        new("mini_cex", "Mini-CEX", null),
        new("msf", "Multi-source feedback", null)
    ];

    /// <summary>What <see cref="GetDecisionBodiesQuery" /> answers: the one body the catalogue names (T131).</summary>
    public static readonly IReadOnlyList<DecisionBodyDto> DecisionBodies =
    [
        new("neonatal", "Neonatal team Clinical Competency Committee")
    ];

    public static readonly IReadOnlyList<EpaDto> Epas =
    [
        Epa(1, "PAED-001"),
        Epa(2, "PAED-002"),
        Epa(3, "PAED-003")
    ];

    /// <summary>Two more of the sub-speciality's EPAs, which no default item names: somewhere to add to (T222).</summary>
    public static readonly IReadOnlyList<EpaDto> MoreEpas =
    [
        Epa(4, "PAED-004"),
        Epa(5, "PAED-005")
    ];

    /// <summary>The v11.1 ladder: level 3 is split, so Order 6 is the College's rung "5" (T100).</summary>
    public static readonly EntrustmentScaleDto CpsaScale = Scale(901, "CPSA Paediatric Entrustment Scale v11.1", "1", "2", "3a", "3b", "4", "5");

    /// <summary>The legacy five-rung ladder, whose labels are prose.</summary>
    public static readonly EntrustmentScaleDto OrScale = Scale(900, "O-R Scale",
        "Observe only", "Direct supervision", "Indirect supervision", "Independent", "Supervises others");

    private List<CurriculumItemDto> _items;

    /// <param name="items">The curriculum's items; the tool-list set when omitted.</param>
    public CurriculumItemsFakeSender(IEnumerable<CurriculumItemDto>? items = null)
    {
        _items = items?.ToList() ??
        [
            Item(11, 1, 3, QuotaPeriod.Semester, """["cbd","mini_cex"]"""),
            Item(12, 2, 3, QuotaPeriod.AcademicYear, null),
            Item(13, 3, 2, QuotaPeriod.Semester, """["legacy_tool","mini_cex"]""")
        ];
    }

    // GetEntrustmentScalesListQuery orders by name.
    public IReadOnlyList<EntrustmentScaleDto> Scales { get; init; } = [];

    public int? SubSpecialityDefaultScaleId { get; init; }

    /// <summary>
    /// Whether the caller may change the curriculum itself, as <c>CurriculumAdminScope.ForCaller</c> answers it: true for
    /// the College or an Administrator, false for an institution reading a curriculum it adopted (T211).
    /// </summary>
    public bool CanEditCurriculum { get; init; } = true;

    public List<RemoveCurriculumItemCommand> Removes { get; } = [];

    public List<UpdateCurriculumItemCommand> Updates { get; } = [];

    public List<AddCurriculumItemCommand> Adds { get; } = [];

    public Exception? UpdateFailure { get; init; }

    public Exception? AddFailure { get; init; }

    /// <summary>A refusal the Remove command answers with (T222).</summary>
    public Exception? RemoveFailure { get; init; }

    /// <summary>
    /// Whether an Add stores the item it was sent and an Update moves the item to the EPA it was sent, as the handlers
    /// store them, so the curriculum the page redraws from holds the EPAs the commands put on it (T222). Off by default:
    /// the older tests check what was sent, not what the list shows after. On, the commands also refuse as the handlers
    /// do: an Add or Update naming an EPA another item holds, and an Update or Remove of an item the curriculum no longer
    /// holds (T222 review).
    /// </summary>
    public bool StoresItems { get; init; }

    /// <summary>How many times the page read the curriculum (<see cref="GetCurriculumByIdQuery" />).</summary>
    public int CurriculumReads { get; private set; }

    /// <summary>Puts an item on the curriculum behind the page's back, as another tab or admin would (T222 review).</summary>
    public void AddElsewhere(int itemId, int epaId)
        => _items = [.. _items, Item(itemId, epaId, 1, QuotaPeriod.AcademicYear, null)];

    /// <summary>Removes an item from the curriculum behind the page's back, as another tab or admin would (T222 review).</summary>
    public void RemoveElsewhere(int itemId)
        => _items = _items.Where(item => item.Id != itemId).ToList();

    /// <summary>Holds each Remove until <see cref="Release" /> (T222, as T206's Withdraw).</summary>
    public bool HoldRemoves { get; init; }

    /// <summary>
    /// Holds the first EPA picker query after a Remove has answered until <see cref="Release" />: the page's refresh of
    /// its pickers, which is still part of the Remove (T222).
    /// </summary>
    public bool HoldPickersAfterARemove { get; init; }

    private TaskCompletionSource? _held;

    public void Release() => (_held ?? throw new InvalidOperationException("Nothing was held.")).SetResult();

    /// <summary>The curriculum's items as the fake holds them now, for a picker answer worked out from them.</summary>
    public IReadOnlyList<CurriculumItemDto> CurrentItems => _items;

    public static CurriculumItemDto Item(
        int id,
        int epaId,
        int requiredCount,
        QuotaPeriod period,
        string? permittedToolsJson,
        int minimumLevelOrder = 3,
        string? stageMinimaJson = null,
        EntrustmentScaleDto? scale = null,
        bool epaIsActive = true,
        QuotaPeriod? decisionCadence = null,
        string? decisionBodyKey = null,
        bool decisionIsOpportunistic = false,
        int? owningInstitutionId = null,
        bool canEdit = true,
        string? owningInstitutionName = null)
    {
        var epa = Epas.Concat(MoreEpas).Single(candidate => candidate.Id == epaId);
        return new CurriculumItemDto(id, epaId, epa.Code, epa.Title, requiredCount, period, minimumLevelOrder, 12, null,
            stageMinimaJson, permittedToolsJson, epaIsActive, owningInstitutionId, owningInstitutionName, decisionCadence,
            decisionBodyKey, BodyName(decisionBodyKey), decisionIsOpportunistic, scale?.Id, scale?.Name)
        {
            // What CurriculumAdminScope.ForCaller answers for the caller (T211); an editable item unless a test says not.
            CanEdit = canEdit
        };
    }

    private static string? BodyName(string? key)
        => key is null ? null : DecisionBodies.FirstOrDefault(body => body.Key == key)?.Name ?? key;

    /// <summary>
    /// What <see cref="ListCurriculumItemEpaOptionsQuery" /> answers for every picker; <see cref="Epas" />, all active,
    /// when not set. <see cref="EpaOptionsFor" /> overrides it per picker.
    /// </summary>
    public IReadOnlyList<EpaDto> EpaList { get; init; } = Epas;

    /// <summary>
    /// What <see cref="ListCurriculumItemEpaOptionsQuery" /> answers for a picker, by its item id (null for the Add
    /// form), or null to answer <see cref="EpaList" />. The rule itself is the Application's (T195); this is its answer.
    /// </summary>
    public Func<int?, IReadOnlyList<EpaDto>?> EpaOptionsFor { get; init; } = _ => null;

    /// <summary>The EPA picker queries the page sent, in order (T195).</summary>
    public List<ListCurriculumItemEpaOptionsQuery> EpaOptionQueries { get; } = [];

    /// <summary>A refusal the edit row's picker query answers with, as the handler does for a caller who may not save the item.</summary>
    public Exception? EditOptionsFailure { get; init; }

    /// <summary><see cref="Epas" /> with the named ones inactive, as the EPA list would return them after a deactivation.</summary>
    public static IReadOnlyList<EpaDto> EpasWithInactive(params int[] inactiveIds)
        => Epas.Select(epa => inactiveIds.Contains(epa.Id) ? epa with { IsActive = false } : epa).ToList();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request is RemoveCurriculumItemCommand heldRemove && HoldRemoves)
        {
            _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _held.Task.ContinueWith(_ => (TResponse)(object)Remove(heldRemove), TaskScheduler.Default);
        }

        if (request is ListCurriculumItemEpaOptionsQuery heldOptions && HoldPickersAfterARemove && Removes.Count > 0 && _held is null)
        {
            _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _held.Task.ContinueWith(_ => (TResponse)(object)EpaOptions(heldOptions), TaskScheduler.Default);
        }

        object response = request switch
        {
            GetCurriculumByIdQuery => ReadCurriculum(),
            ListCurriculumItemEpaOptionsQuery options => EpaOptions(options),
            GetEntrustmentScalesListQuery => Scales,
            GetWbaToolsQuery => Vocabulary,
            GetDecisionBodiesQuery => DecisionBodies,
            UpdateCurriculumItemCommand update => Update(update),
            AddCurriculumItemCommand add => Add(add),
            RemoveCurriculumItemCommand remove => Remove(remove),
            _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
        };

        return Task.FromResult((TResponse)response);
    }

    public Task Send(IRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    private IReadOnlyList<EpaDto> EpaOptions(ListCurriculumItemEpaOptionsQuery query)
    {
        EpaOptionQueries.Add(query);
        if (query.ItemId is not null && EditOptionsFailure is not null)
        {
            throw EditOptionsFailure;
        }

        return EpaOptionsFor(query.ItemId) ?? EpaList;
    }

    private CurriculumDto ReadCurriculum()
    {
        CurriculumReads++;
        return Curriculum();
    }

    // The handlers' refusals, in their words, where the fake stores what the commands send.
    private void EnsureHeld(int itemId)
    {
        if (StoresItems && _items.All(item => item.Id != itemId))
        {
            throw new InvalidOperationException("The requested curriculum item was not found.");
        }
    }

    private void EnsureEpaNotYetOn(int epaId, int? exceptItemId)
    {
        if (StoresItems && _items.Any(item => item.Id != exceptItemId && item.EpaId == epaId))
        {
            throw new InvalidOperationException("This curriculum already contains the selected EPA.");
        }
    }

    private CurriculumDto Curriculum()
        => new(CurriculumId, 2, SubSpecialityId, "Paediatrics", "General Paediatrics", "CMSA", "Paediatrics v11.1", "11.1",
            new DateOnly(2026, 1, 1), null, true, true, _items, SubSpecialityDefaultScaleId)
        {
            CanEditCurriculum = CanEditCurriculum
        };

    private CurriculumDto Remove(RemoveCurriculumItemCommand command)
    {
        Removes.Add(command);
        if (RemoveFailure is not null)
        {
            throw RemoveFailure;
        }

        EnsureHeld(command.ItemId);

        _items = _items.Where(item => item.Id != command.ItemId).ToList();
        return Curriculum();
    }

    // What the handler would store: the command's values, normalised as the handler normalises them.
    private CurriculumDto Update(UpdateCurriculumItemCommand command)
    {
        Updates.Add(command);
        if (UpdateFailure is not null)
        {
            throw UpdateFailure;
        }

        EnsureHeld(command.ItemId);
        EnsureEpaNotYetOn(command.EpaId, command.ItemId);

        var epa = StoresItems ? Epas.Concat(MoreEpas).Single(candidate => candidate.Id == command.EpaId) : null;
        _items = _items
            .Select(item => item.Id == command.ItemId
                ? item with
                {
                    EpaId = epa?.Id ?? item.EpaId,
                    EpaCode = epa?.Code ?? item.EpaCode,
                    EpaTitle = epa?.Title ?? item.EpaTitle,
                    MinimumLevelOrder = command.MinimumLevelOrder,
                    MinimumLevelByStageJson = CurriculumItem.NormalizeStageOverridesJson(command.MinimumLevelByStageJson),
                    ScaleId = command.ScaleId,
                    ScaleName = Scales.FirstOrDefault(scale => scale.Id == command.ScaleId)?.Name,
                    PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson(command.PermittedToolKeys),
                    DecisionCadence = command.DecisionCadence,
                    DecisionBodyKey = DecisionBody.NormalizeKey(command.DecisionBodyKey),
                    DecisionBodyName = BodyName(DecisionBody.NormalizeKey(command.DecisionBodyKey)),
                    DecisionIsOpportunistic = command.DecisionIsOpportunistic
                }
                : item)
            .ToList();
        return Curriculum();
    }

    private CurriculumDto Add(AddCurriculumItemCommand command)
    {
        Adds.Add(command);
        if (AddFailure is not null)
        {
            throw AddFailure;
        }

        EnsureEpaNotYetOn(command.EpaId, null);

        if (StoresItems)
        {
            _items =
            [
                .. _items,
                Item(_items.Select(item => item.Id).DefaultIfEmpty(10).Max() + 1, command.EpaId, command.RequiredCount,
                    command.QuotaPeriod, CurriculumItem.NormalizePermittedToolsJson(command.PermittedToolKeys), command.MinimumLevelOrder)
            ];
        }

        return Curriculum();
    }

    public static EpaDto Epa(int id, string code)
        => new(id, SubSpecialityId, "General Paediatrics", "CMSA", code, $"{code} title", null, null, EpaCategory.Core, true,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private static EntrustmentScaleDto Scale(int id, string name, params string[] labels)
        => new(id, name, null, labels
            .Select((label, index) => new EntrustmentLevelDto(id * 10 + index, index + 1, label, null))
            .ToList());
}
