namespace Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;

/// <summary>
/// The delete's own refusal of a scale, written for the person who pressed (T392 build review P6): a scale in use, one
/// already gone, or one changed by another save meanwhile. Typed, so the scales list shows its words and shows any other
/// exception's text to nobody (T329). An <see cref="InvalidOperationException" />, as before; <paramref name="inner" /> keeps
/// a database refusal underneath, so the audit pipeline still discards the refused removal (T201).
/// </summary>
public sealed class EntrustmentScaleDeleteRefusedException(string message, Exception? inner = null)
    : InvalidOperationException(message, inner);
