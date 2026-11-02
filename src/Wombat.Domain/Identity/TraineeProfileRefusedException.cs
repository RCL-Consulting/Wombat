namespace Wombat.Domain.Identity;

/// <summary>
/// A trainee profile's own refusal, written for the person who pressed (T392 build review P6): an end on a day outside the
/// programme, an end or a change of a profile that has already ended. Typed, so the profile page shows its words and shows
/// any other exception's text to nobody (T329). An <see cref="InvalidOperationException" />, as the refusals were before, so
/// the audit pipeline and every caller that caught one read it as before.
/// </summary>
public sealed class TraineeProfileRefusedException(string message) : InvalidOperationException(message);
