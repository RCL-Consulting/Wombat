using System.Text.Json;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;

namespace Wombat.Web.Components.Pages.Admin.ActivityTypes;

/// <summary>The builder's tabs, in their order (T398, flow 17, Q1).</summary>
internal enum BuilderTab
{
    Details,
    Form,
    Workflow,
    Credit,
    Publish
}

/// <summary>
/// Where a refusal summary's line takes the person (T398, Q6, J1, S1): its tab, then the control at the place: a Details
/// input by id, a section's or a field's editor opened in place, or the tab's JSON editor. A line with none of them goes to
/// its tab alone.
/// </summary>
internal sealed record BuilderGoTo(BuilderTab Tab)
{
    public string? DetailsInputId { get; init; }

    public BuilderSectionModel? Section { get; init; }

    public BuilderFieldModel? Field { get; init; }

    public bool Json { get; init; }
}

/// <summary>One line of the builder's refusal summary: its words, prefixed by the page from the place, and where it goes.</summary>
internal sealed record BuilderSummaryLine(BuilderTab Tab, string Text, BuilderGoTo GoTo);

/// <summary>
/// A refused save or publish as the builder shows it (T398, Q6, J1–J3, S1–S4, S10): the summary's lines across the tabs,
/// each prefixed by its place ("Form:", "Workflow, move Cancel:"), the words at each place by its kind
/// (<c>build-contracts.md</c> § 3), and each tab's count. Built from the Domain's <see cref="RefusalPlace" />s and from the
/// page's own Details checks; the place is resolved against what the page holds now, so a line reaches the row typed.
/// </summary>
internal sealed class BuilderRefusal
{
    private readonly List<BuilderSummaryLine> _lines = [];

    public BuilderRefusal(string title)
    {
        Title = title;
    }

    /// <summary>"Nothing was saved." or "Nothing was published.".</summary>
    public string Title { get; }

    public IReadOnlyList<BuilderSummaryLine> Lines => _lines;

    /// <summary>The words under a Details control, by the control's id.</summary>
    public Dictionary<string, string> Details { get; } = new(StringComparer.Ordinal);

    /// <summary>The words under a section's head.</summary>
    public Dictionary<BuilderSectionModel, string> Sections { get; } = new(ReferenceEqualityComparer.Instance);

    /// <summary>The words at a field's row, or under its key while its editor is open.</summary>
    public Dictionary<BuilderFieldModel, string> Fields { get; } = new(ReferenceEqualityComparer.Instance);

    /// <summary>Whether the workflow's JSON is refused, and the words under it, if any.</summary>
    public bool WorkflowRefused { get; private set; }

    public string? WorkflowWords { get; private set; }

    public bool CreditRefused { get; private set; }

    public string? CreditWords { get; private set; }

    /// <summary>A tab's count: its lines in the summary (S2: "a tab's badge counts its lines").</summary>
    public int CountOn(BuilderTab tab) => _lines.Count(line => line.Tab == tab);

    public bool IsEmpty => _lines.Count == 0;

    /// <summary>A Details refusal under its control (S10's four words, the key taken), and its line.</summary>
    public void AddDetails(string inputId, string message)
    {
        Append(Details, inputId, message);
        _lines.Add(new BuilderSummaryLine(
            BuilderTab.Details, $"{ActivityTypeEditWords.DetailsPrefix} {message}", new BuilderGoTo(BuilderTab.Details) { DetailsInputId = inputId }));
    }

    /// <summary>A Details refusal no control holds: its line alone.</summary>
    public void AddDetailsLine(string message)
        => _lines.Add(new BuilderSummaryLine(BuilderTab.Details, $"{ActivityTypeEditWords.DetailsPrefix} {message}", new BuilderGoTo(BuilderTab.Details)));

    /// <summary>A section the page refuses before sending (a blank key).</summary>
    public void AddSection(BuilderSectionModel section, string sectionName, string message)
    {
        Append(Sections, section, message);
        _lines.Add(new BuilderSummaryLine(
            BuilderTab.Form, $"{ActivityTypeEditWords.FormSectionPrefix(sectionName)} {message}", new BuilderGoTo(BuilderTab.Form) { Section = section }));
    }

    /// <summary>A field the page refuses before sending (a blank key).</summary>
    public void AddField(BuilderSectionModel section, BuilderFieldModel field, string message)
    {
        Append(Fields, field, message);
        _lines.Add(new BuilderSummaryLine(
            BuilderTab.Form, $"{ActivityTypeEditWords.FormPrefix} {message}", new BuilderGoTo(BuilderTab.Form) { Section = section, Field = field }));
    }

    /// <summary>
    /// Every place of a refusal from the Domain (the three parsers, the two cross-checks, P2's aggregate), worded at its
    /// place by its kind; a kind with no words of its own is its summary line alone.
    /// </summary>
    public void AddPlaces(
        IEnumerable<RefusalPlace> places,
        BuilderSchemaModel schema,
        string workflowJson,
        ActivityTypeWorkflowReadDto? workflowRead)
    {
        foreach (var place in places)
        {
            switch (place.Tab)
            {
                case RefusalTab.Details:
                    AddDetailsLine(place.Sentence);
                    break;
                case RefusalTab.Form:
                    AddFormPlace(place, schema);
                    break;
                case RefusalTab.Workflow:
                    AddWorkflowPlace(place, schema, workflowJson, workflowRead);
                    break;
                default:
                    AddCreditPlace(place);
                    break;
            }
        }
    }

    private void AddFormPlace(RefusalPlace place, BuilderSchemaModel schema)
    {
        var tab = new BuilderGoTo(BuilderTab.Form);
        switch (place.Kind)
        {
            case RefusalKind.DuplicateFieldKey when place.FieldKeys.FirstOrDefault() is { } key:
            {
                var holders = FieldsWithKey(schema, key);
                foreach (var (_, holder) in holders)
                {
                    var others = holders.Where(other => !ReferenceEquals(other.Field, holder)).Select(other => Name(other.Field)).ToList();
                    Append(Fields, holder, ActivityTypeEditWords.DuplicateFieldKey(key, others));
                }

                // The line goes to the last holder: the field most likely just given the key (R3-C15-after: Audience).
                var last = holders.LastOrDefault();
                _lines.Add(new BuilderSummaryLine(
                    BuilderTab.Form,
                    $"{ActivityTypeEditWords.FormPrefix} {place.Sentence}",
                    last.Field is null ? tab : tab with { Section = last.Section, Field = last.Field }));
                return;
            }

            case RefusalKind.DuplicateSectionKey when place.SectionKey is { } key:
            {
                var holders = schema.Sections.Where(section => string.Equals(section.Key.Trim(), key, StringComparison.Ordinal)).ToList();
                foreach (var section in holders)
                {
                    var others = holders.Where(other => !ReferenceEquals(other, section)).Select(Name).ToList();
                    Append(Sections, section, ActivityTypeEditWords.DuplicateSectionKey(key, others));
                }

                var last = holders.LastOrDefault();
                _lines.Add(new BuilderSummaryLine(
                    BuilderTab.Form,
                    $"{(last is null ? ActivityTypeEditWords.FormPrefix : ActivityTypeEditWords.FormSectionPrefix(Name(last)))} {place.Sentence}",
                    last is null ? tab : tab with { Section = last }));
                return;
            }

            case RefusalKind.EmptySection when SectionWithKey(schema, place.SectionKey) is { } section:
                Append(Sections, section, ActivityTypeEditWords.EmptySection);
                _lines.Add(new BuilderSummaryLine(
                    BuilderTab.Form,
                    $"{ActivityTypeEditWords.FormSectionPrefix(Name(section))} {place.Sentence}",
                    tab with { Section = section }));
                return;

            case RefusalKind.UserFieldHasOptions when FieldsWithKey(schema, place.FieldKeys.FirstOrDefault()).FirstOrDefault() is { Field: not null } holder:
                Append(Fields, holder.Field, ActivityTypeEditWords.UserFieldHasOptions);
                _lines.Add(new BuilderSummaryLine(
                    BuilderTab.Form,
                    $"{ActivityTypeEditWords.FormPrefix} {place.Sentence}",
                    tab with { Section = holder.Section, Field = holder.Field }));
                return;
        }

        // Any other kind: its summary line alone, which still goes to its field or its section where it has one.
        var field = FieldsWithKey(schema, place.FieldKeys.FirstOrDefault()).FirstOrDefault();
        if (field.Field is not null)
        {
            _lines.Add(new BuilderSummaryLine(
                BuilderTab.Form, $"{ActivityTypeEditWords.FormPrefix} {place.Sentence}", tab with { Section = field.Section, Field = field.Field }));
            return;
        }

        if (SectionWithKey(schema, place.SectionKey) is { } placeSection)
        {
            _lines.Add(new BuilderSummaryLine(
                BuilderTab.Form,
                $"{ActivityTypeEditWords.FormSectionPrefix(Name(placeSection))} {place.Sentence}",
                tab with { Section = placeSection }));
            return;
        }

        _lines.Add(new BuilderSummaryLine(BuilderTab.Form, $"{ActivityTypeEditWords.FormPrefix} {place.Sentence}", tab));
    }

    private void AddWorkflowPlace(
        RefusalPlace place, BuilderSchemaModel schema, string workflowJson, ActivityTypeWorkflowReadDto? workflowRead)
    {
        var json = new BuilderGoTo(BuilderTab.Workflow) { Json = true };
        if (place.Kind == RefusalKind.MalformedJson)
        {
            var words = ActivityTypeEditWords.MalformedJson(workflow: true, place.JsonLine);
            WorkflowRefused = true;
            WorkflowWords = Joined(WorkflowWords, words);
            _lines.Add(new BuilderSummaryLine(BuilderTab.Workflow, $"{ActivityTypeEditWords.WorkflowPrefix} {words}", json));
            return;
        }

        var move = place.MoveKey is { } moveKey ? MoveLabel(workflowJson, moveKey, workflowRead) : null;
        var prefix = move is null ? ActivityTypeEditWords.WorkflowPrefix : ActivityTypeEditWords.WorkflowMovePrefix(move);
        _lines.Add(new BuilderSummaryLine(BuilderTab.Workflow, $"{prefix} {place.Sentence}", json));

        string? atPlace = (place.Kind, move, place.NamedField) switch
        {
            (RefusalKind.FieldRuleNotUserField, { } label, { } named)
                => ActivityTypeEditWords.FieldRuleNotUserField(label, named, LabelOf(schema, named)),
            (RefusalKind.FieldRuleNamesNoField, { } label, { } named)
                => ActivityTypeEditWords.FieldRuleNamesNoField(label, named),
            _ => null
        };

        if (atPlace is not null)
        {
            WorkflowRefused = true;
            WorkflowWords = Joined(WorkflowWords, atPlace);
        }
    }

    private void AddCreditPlace(RefusalPlace place)
    {
        var json = new BuilderGoTo(BuilderTab.Credit) { Json = true };
        if (place.Kind == RefusalKind.MalformedJson)
        {
            var words = ActivityTypeEditWords.MalformedJson(workflow: false, place.JsonLine);
            CreditRefused = true;
            CreditWords = Joined(CreditWords, words);
            _lines.Add(new BuilderSummaryLine(BuilderTab.Credit, $"{ActivityTypeEditWords.CreditPrefix} {words}", json));
            return;
        }

        _lines.Add(new BuilderSummaryLine(BuilderTab.Credit, $"{ActivityTypeEditWords.CreditPrefix} {place.Sentence}", json));
        if (place is { Kind: RefusalKind.CreditEpaFieldDisagrees, DirectiveNumber: { } rule, NamedField: { } named, ExpectedField: { } expected })
        {
            CreditRefused = true;
            CreditWords = Joined(CreditWords, ActivityTypeEditWords.CreditEpaFieldDisagrees(rule, named, expected));
        }
    }

    /// <summary>
    /// A move by its label as the typed JSON gives it, else as the saved read names it, else its key in words ("cancel"
    /// reads "Cancel"). The JSON is read as JSON only, never parsed as a workflow: the refused one may not be one.
    /// </summary>
    internal static string MoveLabel(string workflowJson, string moveKey, ActivityTypeWorkflowReadDto? workflowRead)
    {
        try
        {
            using var document = JsonDocument.Parse(workflowJson);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("transitions", out var transitions) &&
                transitions.ValueKind == JsonValueKind.Array)
            {
                foreach (var transition in transitions.EnumerateArray())
                {
                    if (transition.ValueKind == JsonValueKind.Object &&
                        transition.TryGetProperty("key", out var key) &&
                        key.ValueKind == JsonValueKind.String &&
                        string.Equals(key.GetString(), moveKey, StringComparison.Ordinal) &&
                        transition.TryGetProperty("label", out var label) &&
                        label.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(label.GetString()))
                    {
                        return label.GetString()!.Trim();
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON: the read's label or the key in words.
        }

        return workflowRead?.Moves.FirstOrDefault(m => string.Equals(m.Key, moveKey, StringComparison.Ordinal))?.Label
            ?? InWords(moveKey);
    }

    private static string InWords(string key)
    {
        var words = key.Replace('_', ' ').Replace('-', ' ').Trim();
        return words.Length == 0 ? key : char.ToUpperInvariant(words[0]) + words[1..];
    }

    private static List<(BuilderSectionModel Section, BuilderFieldModel Field)> FieldsWithKey(BuilderSchemaModel schema, string? key)
        => key is null
            ? []
            : schema.Sections
                .SelectMany(section => section.Fields.Select(field => (Section: section, Field: field)))
                .Where(pair => string.Equals(pair.Field.Key.Trim(), key, StringComparison.Ordinal))
                .ToList();

    private static BuilderSectionModel? SectionWithKey(BuilderSchemaModel schema, string? key)
        => key is null ? null : schema.Sections.FirstOrDefault(section => string.Equals(section.Key.Trim(), key, StringComparison.Ordinal));

    private static string? LabelOf(BuilderSchemaModel schema, string key)
        => FieldsWithKey(schema, key).Select(pair => pair.Field.Label.Trim()).FirstOrDefault(label => label.Length > 0);

    internal static string Name(BuilderSectionModel section)
        => string.IsNullOrWhiteSpace(section.Title) ? section.Key.Trim() : section.Title.Trim();

    internal static string Name(BuilderFieldModel field)
        => string.IsNullOrWhiteSpace(field.Label) ? field.Key.Trim() : field.Label.Trim();

    private static void Append<TKey>(Dictionary<TKey, string> words, TKey key, string message)
        where TKey : notnull
        => words[key] = words.TryGetValue(key, out var before) ? $"{before} {message}" : message;

    private static string Joined(string? before, string words) => before is null ? words : $"{before} {words}";
}
