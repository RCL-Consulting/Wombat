namespace Wombat.Application.Audit;

/// <summary>
/// A command whose audit row must not say who sent it: no actor, no actor's institution, no user agent. The row still
/// records the command, when it ran, whether it succeeded and the client's truncated address. (T205)
/// </summary>
/// <remarks>
/// <para>
/// For a command whose sender is promised anonymity. The one such command is an MSF respondent's submission
/// (<c>SubmitMsfResponseCommand</c>). A respondent is a stranger holding an emailed link, but the page they answer on is
/// served from the signed-in app's own origin, so a consultant who opens their link in the browser where they use Wombat
/// arrives signed in: <c>[AllowAnonymous]</c> admits a caller without a sign-in, it does not remove one. Without this
/// marker, the pipeline filled the row from that sign-in: their user id, their email and their institution, which put the
/// row in front of that institution's admins at <c>/admin/audit</c>, a few milliseconds before the response it submitted.
/// </para>
/// <para>
/// The row carries no institution, so it is read by a global Administrator only (T101). The user agent goes with the
/// actor: beside the user agent on the respondent's own sign-in row, it would name them again. The address stays, as T101
/// kept it on this row ("from where"), truncated as on every row.
/// </para>
/// <para>
/// Declaring an actor or an institution (<see cref="IAuditContextProvider" />) does not reach such a row either.
/// </para>
/// </remarks>
public interface IAnonymousAuditedCommand : IAuditedCommand;
