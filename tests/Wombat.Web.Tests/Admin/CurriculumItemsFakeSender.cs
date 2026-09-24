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

    public static readonly IReadOnlyList<EpaDto> Epas =
    [
        Epa(1, "PAED-001"),
        Epa(2, "PAED-002"),
        Epa(3, "PAED-003")
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

    public List<UpdateCurriculumItemCommand> Updates { get; } = [];

    public List<AddCurriculumItemCommand> Adds { get; } = [];

    public Exception? UpdateFailure { get; init; }

    public Exception? AddFailure { get; init; }

    public static CurriculumItemDto Item(
        int id,
        int epaId,
        int requiredCount,
        QuotaPeriod period,
        string? permittedToolsJson,
        int minimumLevelOrder = 3,
        string? stageMinimaJson = null,
        EntrustmentScaleDto? scale = null)
    {
        var epa = Epas.Single(candidate => candidate.Id == epaId);
        return new CurriculumItemDto(id, epaId, epa.Code, epa.Title, requiredCount, period, minimumLevelOrder, 12, null,
            stageMinimaJson, permittedToolsJson, scale?.Id, scale?.Name);
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        object response = request switch
        {
            GetCurriculumByIdQuery => Curriculum(),
            ListEpasForSubSpecialityQuery => Epas,
            GetEntrustmentScalesListQuery => Scales,
            GetWbaToolsQuery => Vocabulary,
            UpdateCurriculumItemCommand update => Update(update),
            AddCurriculumItemCommand add => Add(add),
            _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
        };

        return Task.FromResult((TResponse)response);
    }

    public Task Send(IRequest request, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    private CurriculumDto Curriculum()
        => new(CurriculumId, 2, SubSpecialityId, "Paediatrics", "General Paediatrics", "CMSA", "Paediatrics v11.1", "11.1",
            new DateOnly(2026, 1, 1), null, true, true, _items, SubSpecialityDefaultScaleId);

    // What the handler would store: the command's values, normalised as the handler normalises them.
    private CurriculumDto Update(UpdateCurriculumItemCommand command)
    {
        Updates.Add(command);
        if (UpdateFailure is not null)
        {
            throw UpdateFailure;
        }

        _items = _items
            .Select(item => item.Id == command.ItemId
                ? item with
                {
                    MinimumLevelOrder = command.MinimumLevelOrder,
                    MinimumLevelByStageJson = CurriculumItem.NormalizeStageOverridesJson(command.MinimumLevelByStageJson),
                    ScaleId = command.ScaleId,
                    ScaleName = Scales.FirstOrDefault(scale => scale.Id == command.ScaleId)?.Name,
                    PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson(command.PermittedToolKeys)
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

        return Curriculum();
    }

    private static EpaDto Epa(int id, string code)
        => new(id, SubSpecialityId, "General Paediatrics", "CMSA", code, $"{code} title", null, null, EpaCategory.Core, true,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private static EntrustmentScaleDto Scale(int id, string name, params string[] labels)
        => new(id, name, null, labels
            .Select((label, index) => new EntrustmentLevelDto(id * 10 + index, index + 1, label, null))
            .ToList());
}
