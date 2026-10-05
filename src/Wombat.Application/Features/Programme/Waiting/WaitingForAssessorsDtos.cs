using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;

namespace Wombat.Application.Features.Programme.Waiting;

/// <summary>
/// One reminder a member of staff sent about a waiting request (T358, flow 06; C4): "Reminded 2026-10-04 by Pieter Smit".
/// </summary>
/// <param name="SentOn">When (UTC).</param>
/// <param name="SentOnDay">The South African day it fell on, which the same-day block counts by.</param>
/// <param name="SentByName">Who sent it, by name (<c>UserDisplayNames</c>).</param>
public sealed record ActivityReminderDto(DateTime SentOn, DateOnly SentOnDay, string SentByName);

/// <summary>
/// What Waiting for assessors is asked for (T358; D3): Overdue only, one nominee (With), and one registrar (the registrar
/// page's section). Each null or false asks nothing.
/// </summary>
public sealed record WaitingForAssessorsFilter(bool OverdueOnly = false, string? WithUserId = null, string? SubjectUserId = null);

/// <summary>A name the With filter offers: a waiting row's nominee (T358, review 12).</summary>
public sealed record NomineeOptionDto(string UserId, string Name);

/// <summary>
/// What waits for a named assessor in one programme scope, read as one role (T358, flow 06; Q3, E3, E4): Home's card and
/// the page are this one read (<see cref="WaitingForAssessorsReader" />).
/// </summary>
/// <param name="Scope">The scope read, which the subtitle names (E4).</param>
/// <param name="Items">The page of the match, oldest first by <c>UpdatedOn</c>, then id.</param>
/// <param name="MatchCount">How many match the filters: the heading's "3 waiting".</param>
/// <param name="MatchOverdueCount">How many of the match are overdue: the heading's ", 2 overdue".</param>
/// <param name="TotalCount">How many wait before any filter but the registrar's: the no-match heading's "of 2".</param>
/// <param name="TotalOverdueCount">How many of those are overdue: the card's stripe.</param>
/// <param name="Nominees">Every waiting row's nominee once, by surname then first name: the With filter's names.</param>
/// <param name="DueDays"><c>DashboardThresholds.AssessorDueDays</c>: "Overdue once it has waited 7 days."</param>
/// <param name="NudgeDays"><c>DashboardThresholds.AssessorNudgeDays</c>: "Its assessor is emailed after 5."</param>
/// <param name="Page">The page returned, from 1, held to the last page there is.</param>
/// <param name="PageSize">The rows a page holds.</param>
/// <param name="MayRemind">
/// Whether the role read as may send a reminder: one of <see cref="ProgrammeScope.WaitingRoles" />. A Committee member
/// reads a registrar's waiting requests on the registrar page and sends none (D5).
/// </param>
public sealed record WaitingForAssessorsDto(
    ProgrammeScopeDto Scope,
    IReadOnlyList<ActivitySummaryDto> Items,
    int MatchCount,
    int MatchOverdueCount,
    int TotalCount,
    int TotalOverdueCount,
    IReadOnlyList<NomineeOptionDto> Nominees,
    int DueDays,
    int NudgeDays,
    int Page,
    int PageSize,
    bool MayRemind)
{
    /// <summary>
    /// What was asked, as read (T358, lane A1): the heading's ", with Mohammed Patel" and the no-match line's "what was
    /// asked" read it, so the page's words cannot disagree with the rows it was given.
    /// </summary>
    public WaitingForAssessorsFilter Filter { get; init; } = new();
}

/// <summary>
/// Whom a reminder about one waiting request may be written to (T358, flow 06; E1; review 2): the one rule the list's
/// "No reminder: …" line and the command's refusal share, so the row says before anyone presses what the command would
/// answer.
/// </summary>
/// <remarks>
/// <para>
/// In the order the refusals are asked: no account (the nominee field names someone erased or deleted), deactivated (an
/// administrator's lock or an erasure, never a brute-force lockout, which lifts itself), and no email address.
/// </para>
/// <para>
/// Never the opt-out of digest emails (E1): a reminder is "email about one particular thing", which the account page says
/// is still sent. The periodic reminders' rule (<c>ReminderRecipientPolicy</c>) is the same three and the opt-out; it
/// stays Infrastructure's and unchanged.
/// </para>
/// </remarks>
public static class ReminderRecipientRules
{
    /// <summary>Why a reminder may not be sent to <paramref name="recipient" />, or null when it may.</summary>
    public static ReminderOutcome? RefusalFor(ReminderRecipientDto? recipient) => recipient switch
    {
        null => ReminderOutcome.NoAccount,
        { IsDeactivated: true } => ReminderOutcome.Deactivated,
        _ when string.IsNullOrWhiteSpace(recipient.Email) => ReminderOutcome.NoEmail,
        _ => null
    };
}
