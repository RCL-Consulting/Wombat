using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;

namespace Wombat.Web.Components.Pages.Admin.EntrustmentScales;

/// <summary>
/// The words of a scale's page (T394, flow 16, lane B; boards R3-C02–C05, C11–C16, C20, R2-C17–C19b), in one place, so the
/// page and its tests read the same sentences. The refusals of a save are <see cref="EntrustmentScaleRefusalWords" />'s
/// (the command's own words, composed from its typed refusal) and the field refusals <see cref="EntrustmentScaleBindings" />'s.
/// </summary>
public static class EntrustmentScaleWords
{
    /// <summary>R2-C18b, C19b: the h1 while a scale is read, or when its read failed (T7).</summary>
    public const string HeadingWhileUnread = "Entrustment scale";

    /// <summary>R3-C03a: the create page's h1 and tab, as every create board draws it.</summary>
    public const string CreateHeading = "Create entrustment scale";

    public const string LoadingScale = "Loading this entrustment scale.";

    public const string ScaleLoadFailed =
        "Could not load this entrustment scale. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>R2-C17 (Q7).</summary>
    public const string UnavailableHeading = "Entrustment scale unavailable";

    public const string UnavailableBody = "This entrustment scale does not exist, or you cannot open it.";

    public const string GoToEntrustmentScales = "Go to Entrustment scales";

    /// <summary>R3-C02 (S15).</summary>
    public const string RecordSubtitle = "A ladder shared by every College and institution.";

    /// <summary>R3-C03a.</summary>
    public const string CreateSubtitle = "A ladder of at least two rungs, shared by every College and institution.";

    public const string DetailsHeading = "Details";
    public const string LevelsHeading = "Levels";
    public const string NameLabel = "Name";
    public const string DescriptionLabel = "Description";

    /// <summary>R3-C02: what a rung's label and order are, above the table.</summary>
    public const string LevelsIntro =
        "A rung is read by its label everywhere (\"3a\", \"5\"). Its order is its rank, lowest first, and everything that " +
        "compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is " +
        "shown only on this page.";

    /// <summary>R3-C02 and N12: the table's caption.</summary>
    public static string Caption(string? scaleName)
        => string.IsNullOrWhiteSpace(scaleName) ? "Levels of the new scale, lowest first" : $"Levels of {scaleName}, lowest first";

    public const string OrderHeader = "Order";
    public const string LabelHeader = "Label";
    public const string DescriptionHeader = "Description";

    /// <summary>Q16 (T395), above Add level on a scale in use.</summary>
    public const string InUseLine = "Rungs can be reordered or removed only while nothing uses this scale.";

    public const string AddLevel = "Add level";
    public const string Cancel = "Cancel";
    public const string Save = "Save";
    public const string Saving = "Saving…";
    public const string CreateScale = "Create scale";
    public const string Creating = "Creating…";

    // ---- a rung's controls (R3-C03a, C04): by its order for its Label, by its label otherwise, its order while empty ----

    public static string LabelName(int order) => $"Label of the rung in order {order}";

    /// <summary>A rung as its row's controls name it: by its label, or "the rung in order 2" while it has none.</summary>
    public static string Rung(int order, string? label)
        => string.IsNullOrWhiteSpace(label) ? $"the rung in order {order}" : label.Trim();

    public static string DescriptionName(int order, string? label) => $"Description of {Rung(order, label)}";

    // ---- results and failures ----

    /// <summary>R3-C04, by the ?created token.</summary>
    public const string Created = "Entrustment scale created.";

    /// <summary>R3-C05.</summary>
    public const string Saved = "Entrustment scale saved.";

    /// <summary>R2-0-spec-2 § 9's failure sentence.</summary>
    public const string SaveFailed =
        "The entrustment scale could not be saved. Nothing has changed. Try again, or come back in a few minutes.";

    public const string CreateFailed =
        "The entrustment scale could not be created. Nothing has changed. Try again, or come back in a few minutes.";
}
