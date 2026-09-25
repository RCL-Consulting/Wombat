using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Web.Components.Pages.CommitteeDecisions;

/// <summary>The lines of one instrument under one EPA, in encounter order.</summary>
public sealed record EvidenceInstrumentGroup(string InstrumentName, IReadOnlyList<CommitteeEvidenceDto> Lines);

/// <summary>Every line about one EPA, grouped by instrument.</summary>
public sealed record EvidenceEpaGroup(
    int EpaId,
    string EpaCode,
    string? EpaTitle,
    IReadOnlyList<EvidenceInstrumentGroup> Instruments)
{
    public int LineCount => Instruments.Sum(instrument => instrument.Lines.Count);
}

/// <summary>
/// A frozen evidence snapshot as the committee page shows it (T167).
/// </summary>
/// <param name="ByEpa">The lines about one EPA each, grouped by EPA in code order and then by instrument.</param>
/// <param name="AcrossEpas">
/// The lines that are about no single EPA: an MSF campaign's report, which covers several (its per-EPA records are
/// ordinary activity lines under each EPA), and an activity whose type names no EPA.
/// </param>
/// <param name="FrozenBeforeLinesNamedTheirEpa">
/// Activity lines frozen before T167, which recorded no EPA, instrument, rating or encounter date. They are listed as
/// they were written, not under a heading that would claim they are about no EPA.
/// </param>
public sealed record EvidenceSnapshotView(
    IReadOnlyList<EvidenceEpaGroup> ByEpa,
    IReadOnlyList<CommitteeEvidenceDto> AcrossEpas,
    IReadOnlyList<CommitteeEvidenceDto> FrozenBeforeLinesNamedTheirEpa)
{
    /// <summary>Whether <see cref="AcrossEpas" /> holds an MSF campaign's report.</summary>
    public bool AcrossEpasHoldsACampaignReport
        => AcrossEpas.Any(line => line.SourceType == CommitteeEvidenceSourceType.MsfCampaign);

    /// <summary>Whether <see cref="AcrossEpas" /> holds an activity, which is then one whose type names no EPA.</summary>
    public bool AcrossEpasHoldsAnActivity
        => AcrossEpas.Any(line => line.SourceType == CommitteeEvidenceSourceType.Activity);

    /// <summary>Whether the view holds no line at all.</summary>
    public bool IsEmpty => ByEpa.Count == 0 && AcrossEpas.Count == 0 && FrozenBeforeLinesNamedTheirEpa.Count == 0;
}

/// <summary>
/// Groups a frozen snapshot by EPA and then by instrument, keeping every line in every state (T135): the panel reads a
/// run of declines as evidence too, so nothing is filtered here.
/// </summary>
public static class EvidenceSnapshotGroups
{
    public static EvidenceSnapshotView Build(IEnumerable<CommitteeEvidenceDto> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var lines = items.ToArray();

        var frozenBefore = lines
            .Where(line => line.FrozenBeforeLinesNamedTheirEpa)
            .OrderBy(line => line.SourceRecordedOn)
            .ThenBy(line => line.Id)
            .ToArray();

        var described = lines.Where(line => !line.FrozenBeforeLinesNamedTheirEpa).ToArray();

        var byEpa = described
            .Where(line => line.SourceType == CommitteeEvidenceSourceType.Activity && line.EpaId is not null)
            .GroupBy(line => line.EpaId!.Value)
            .Select(group =>
            {
                var first = group.First();
                var instruments = group
                    .GroupBy(line => InstrumentOf(line), StringComparer.Ordinal)
                    .OrderBy(instrument => instrument.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(instrument => new EvidenceInstrumentGroup(
                        instrument.Key,
                        instrument
                            .OrderBy(line => line.ObservedOn)
                            .ThenBy(line => line.ActivityId)
                            .ToArray()))
                    .ToArray();

                return new EvidenceEpaGroup(group.Key, first.EpaCode ?? $"EPA {group.Key}", first.EpaTitle, instruments);
            })
            .OrderBy(group => group.EpaCode, StringComparer.Ordinal)
            .ThenBy(group => group.EpaId)
            .ToArray();

        var acrossEpas = described
            .Where(line => line.SourceType != CommitteeEvidenceSourceType.Activity || line.EpaId is null)
            .OrderBy(line => line.SourceType)
            .ThenBy(line => line.ObservedOn)
            .ThenBy(line => line.SourceRecordedOn)
            .ThenBy(line => line.Id)
            .ToArray();

        return new EvidenceSnapshotView(byEpa, acrossEpas, frozenBefore);
    }

    /// <summary>
    /// The lines a staged entrustment decision may name, grouped as <see cref="Build" /> groups them, with the chosen
    /// EPA's group first (D38, T131).
    /// </summary>
    /// <remarks>
    /// Only lines the staging handler accepts (<see cref="CommitteeEvidenceDto.CanGroundADecision" />), so the picker
    /// offers nothing the gate refuses: never a supervisor report. The chosen EPA's lines come first as a hint, never as
    /// a filter; a decision may rest on a line about any EPA. With no EPA chosen, or none of its lines in the snapshot,
    /// the groups keep <see cref="Build" />'s code order.
    /// </remarks>
    public static EvidenceSnapshotView BuildPicker(IEnumerable<CommitteeEvidenceDto> items, int chosenEpaId)
    {
        ArgumentNullException.ThrowIfNull(items);

        var view = Build(items.Where(line => line.CanGroundADecision));
        var byEpa = view.ByEpa
            .OrderBy(group => group.EpaId == chosenEpaId ? 0 : 1)
            .ToArray();

        return view with { ByEpa = byEpa };
    }

    /// <summary>What the picker says beside an EPA line's label: its rung, encounter date and state.</summary>
    public static string PickerDetail(CommitteeEvidenceDto line)
    {
        ArgumentNullException.ThrowIfNull(line);

        var parts = new List<string> { RatingText(line) };
        if (line.ObservedOn is DateOnly observedOn)
        {
            var day = observedOn.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            parts.Add(line.ObservedOnDeclared == false ? $"{day} (filed)" : day);
        }

        // The state by its label, frozen with the snapshot (T220).
        if (!string.IsNullOrWhiteSpace(line.SourceStateLabel))
        {
            parts.Add(line.SourceStateLabel);
        }

        return string.Join(" · ", parts);
    }

    /// <summary>The rating column's text: the rung, else whether the instrument rates at all.</summary>
    public static string RatingText(CommitteeEvidenceDto line)
    {
        ArgumentNullException.ThrowIfNull(line);

        if (!string.IsNullOrWhiteSpace(line.RatingLabel))
        {
            return line.RatingLabel;
        }

        return line.IsRatedInstrument == true ? "Not recorded" : "Unrated";
    }

    private static string InstrumentOf(CommitteeEvidenceDto line)
        => string.IsNullOrWhiteSpace(line.InstrumentName) ? "Unnamed instrument" : line.InstrumentName;
}
