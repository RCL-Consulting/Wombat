using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Web.Components.Pages.CommitteeDecisions.Panels;

/// <summary>
/// The words of Decision panels and a panel's page (T364, flow 08: boards R2-1 and R2-2; Q5, Q6, Q8; C10–C13; round 3 M6,
/// M7, M8, F2, F5, S3, S4, S6, S7; T260, T329, T338). One place, so the pages and their tests say the same thing. No
/// pronoun: a person is named, or is "the chair".
/// </summary>
public static class PanelWords
{
    // ─── Decision panels ─────────────────────────────────────────────────────

    /// <summary>The list's h1 and tab.</summary>
    public const string ListTitle = "Decision panels";

    /// <summary>
    /// The list's subtitle: "… at Kgosi Kgari Teaching Hospital." once the institution is known; an Administrator, who
    /// belongs to none, reads the panels "by institution" until one is chosen.
    /// </summary>
    public static string ListSubtitle(string? institutionName, bool isAdministrator)
        => institutionName is not null
            ? $"The panels that take entrustment and progression decisions at {institutionName}."
            : isAdministrator
                ? "The panels that take entrustment and progression decisions, by institution."
                : "The panels that take entrustment and progression decisions.";

    /// <summary>Said while the list is read, in its status line (C11).</summary>
    public const string ListLoading = "Loading Decision panels.";

    /// <summary>The list's load error, fixed words; the failure goes to the log (C11, T338).</summary>
    public const string ListLoadFailed =
        "Could not load Decision panels. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The list's caption.</summary>
    public static string ListCaption(string? institutionName)
        => institutionName is null ? "Decision panels" : $"Decision panels at {institutionName}";

    /// <summary>The empty list's title.</summary>
    public const string NoPanels = "No decision panels";

    /// <summary>The Administrator's filter: its label, its first option, and the state before Show (F2).</summary>
    public const string InstitutionLabel = "Institution";

    public const string ChooseInstitutionOption = "Select institution";

    public const string ChooseInstitutionTitle = "Choose an institution";

    public const string ChooseInstitutionBody =
        "An administrator belongs to no institution, so the panels are shown for the one chosen above.";

    /// <summary>The Administrator's institutions could not be read (T338).</summary>
    public const string InstitutionsLoadFailed =
        "Could not load the institutions. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>
    /// Why New panel is missing for someone who manages panels but may create none (T260; T194), and the new-panel page's
    /// words in place of the form.
    /// </summary>
    /// <remarks>Roles, never a pronoun for them (T364, build review D6).</remarks>
    public const string NoneCreatable =
        "You cannot create a decision panel. A speciality or sub-speciality administrator creates the panels of the " +
        "speciality administered, at the administrator's own institution, once that institution has adopted a " +
        "curriculum in the speciality. An institutional administrator creates the institution's panels.";

    /// <summary>The Scope cell: the speciality a panel covers, or the whole institution; never the enum (round 3 M8).</summary>
    public static string ScopeCell(DecisionPanelScope scope, string? specialityName)
        => scope == DecisionPanelScope.Speciality
            ? specialityName is null ? "Speciality" : $"Speciality · {specialityName}"
            : WholeInstitution;

    /// <summary>An institution-wide panel's scope.</summary>
    public const string WholeInstitution = "Whole institution";

    /// <summary>What a panel decides for: the College committee it sits as, or "General panel".</summary>
    public static string DecidesFor(string? decisionBodyName) => decisionBodyName ?? GeneralPanel;

    public const string GeneralPanel = "General panel";

    /// <summary>
    /// The Members cell: the count, then who sits by name, the chair and the external members marked: "4: Thandi Zulu
    /// (chair), David Naidoo, Sarah Botha, John van Rensburg (external)" (round 3 M8).
    /// </summary>
    public static string MembersCell(int count, IReadOnlyList<DecisionPanelMemberNameDto> members)
    {
        ArgumentNullException.ThrowIfNull(members);
        if (members.Count == 0)
        {
            return count.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        var names = members.Select(member => member.Role switch
        {
            DecisionPanelMemberRole.Chair => $"{member.Name} (chair)",
            DecisionPanelMemberRole.External => $"{member.Name} (external)",
            _ => member.Name
        });
        return $"{count}: {string.Join(", ", names)}";
    }

    // ─── Who decides each EPA ────────────────────────────────────────────────

    public const string RoutingTitle = "Who decides each EPA";

    public static string RoutingIntro(string institutionName)
        => $"Which panel takes each EPA's entrustment decision for a trainee of each programme at {institutionName}. An EPA " +
           "the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a panel sitting as it, one " +
           "covering the trainee's speciality before an institution-wide one, and to the general panels while none does.";

    public const string RoutingChooseInstitution = "Choose an institution to see which panel decides each EPA there.";

    public const string RoutingNone = "There is no committee routing to show for your institution.";

    public static string RoutingNoProgramme(string institutionName)
        => $"No programme is adopted or followed at {institutionName} yet.";

    /// <summary>Who decides each EPA could not be read (T329, T338).</summary>
    public const string RoutingLoadFailed =
        "Could not load who decides each EPA. Nothing has changed. Try again, or come back in a few minutes.";

    public static string RoutingCaption(string programme) => $"Who decides each EPA of {programme}";

    public const string NoPanelCovers = "No panel at this institution covers this programme.";

    /// <summary>
    /// One row of Who decides each EPA: its codes, as ranges ("PAED-001 to PAED-003, PAED-006 to PAED-015"), and who
    /// decides them, in one sentence.
    /// </summary>
    public sealed record RouteRow(string Codes, string DecidedBy);

    /// <summary>
    /// The rows of one programme: the EPAs that go to the same panels for the same reason share a row, and where no panel
    /// covers the programme every EPA shares one, "PAED-001 to PAED-015" (board R2-1, step 2.20). Rows read in the order of
    /// their first EPA.
    /// </summary>
    public static IReadOnlyList<RouteRow> RoutesOf(CommitteeRoutingProgrammeDto programme)
    {
        ArgumentNullException.ThrowIfNull(programme);

        return programme.Lines
            .GroupBy(line => line.Panels.Count == 0
                ? "none"
                : $"{line.DecisionBodyKey}|{line.FallsBackToGeneral}|{string.Join(",", line.Panels.Select(panel => panel.Id))}")
            .Select(group => (Codes: group.Select(line => line.EpaCode).ToArray(), Line: group.First()))
            .OrderBy(row => row.Codes.Min(StringComparer.Ordinal), StringComparer.Ordinal)
            .Select(row => new RouteRow(Ranges(row.Codes), DecidedBy(row.Line)))
            .ToArray();
    }

    private static string DecidedBy(CommitteeRoutingLineDto line)
    {
        if (line.Panels.Count == 0)
        {
            return NoPanelCovers;
        }

        var panels = string.Join(", ", line.Panels.Select(panel => panel.Name));
        if (line.DecisionBodyName is not { } body)
        {
            return panels;
        }

        // The fallback names the panels before it; "covering this programme": a panel sitting as the body may exist for
        // another speciality, and "no panel sits as it" would then be untrue.
        return line.FallsBackToGeneral
            ? $"{panels}. No panel covering this programme sits as the {body}, so " +
              (line.Panels.Count == 1 ? "the general panel decides." : "the general panels decide.")
            : $"{panels}, sitting as the {body}.";
    }

    /// <summary>
    /// Codes as ranges: three or more in a row of one prefix read "PAED-006 to PAED-015", two read "PAED-004, PAED-005",
    /// and a code with no number stands alone.
    /// </summary>
    public static string Ranges(IEnumerable<string> codes)
    {
        var parsed = codes
            .Distinct(StringComparer.Ordinal)
            .Select(code => (Code: code, Prefix: Prefix(code, out var number, out var width), Number: number, Width: width))
            .OrderBy(code => code.Prefix, StringComparer.Ordinal)
            .ThenBy(code => code.Number ?? int.MaxValue)
            .ThenBy(code => code.Code, StringComparer.Ordinal)
            .ToList();

        var parts = new List<string>();
        var index = 0;
        while (index < parsed.Count)
        {
            var end = index;
            while (end + 1 < parsed.Count &&
                   parsed[end].Number is int current &&
                   parsed[end + 1].Number == current + 1 &&
                   string.Equals(parsed[end + 1].Prefix, parsed[index].Prefix, StringComparison.Ordinal))
            {
                end++;
            }

            var length = end - index + 1;
            if (length >= 3)
            {
                parts.Add($"{parsed[index].Code} to {parsed[end].Code}");
            }
            else
            {
                parts.AddRange(parsed.Skip(index).Take(length).Select(code => code.Code));
            }

            index = end + 1;
        }

        return string.Join(", ", parts);
    }

    private static string Prefix(string code, out int? number, out int width)
    {
        var digits = 0;
        while (digits < code.Length && char.IsAsciiDigit(code[code.Length - 1 - digits]))
        {
            digits++;
        }

        width = digits;
        number = digits > 0 && digits < 9 ? int.Parse(code[^digits..], System.Globalization.CultureInfo.InvariantCulture) : null;
        return code[..^digits];
    }

    // ─── A panel's page ──────────────────────────────────────────────────────

    /// <summary>The page's h1 while it reads and when the read failed (C11).</summary>
    public const string PanelLoadingTitle = "Decision panel";

    /// <summary>A new panel's h1 and tab.</summary>
    public const string NewPanelTitle = "New panel";

    public const string NewPanelSubtitle =
        "A panel takes entrustment and progression decisions for the trainees it covers. Its name, scope, institution and " +
        "speciality cannot change once it is created.";

    public const string PanelLoading = "Loading the panel.";

    public const string PanelLoadFailed =
        "Could not load this panel. Nothing has changed. Try again, or come back in a few minutes.";

    public const string NewPanelLoadFailed =
        "Could not load the new-panel form. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>A panel's subtitle: "Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital" (Q6).</summary>
    public static string PanelSubtitle(DecisionPanelScope scope, string? specialityName, string? institutionName)
    {
        var parts = new List<string> { scope == DecisionPanelScope.Speciality ? "Speciality panel" : "Institution panel" };
        if (scope == DecisionPanelScope.Speciality && specialityName is not null)
        {
            parts.Add(specialityName);
        }

        if (institutionName is not null)
        {
            parts.Add(institutionName);
        }

        return string.Join(" · ", parts);
    }

    /// <summary>About this panel's Scope: "Speciality: Paediatrics", or the whole institution.</summary>
    public static string ScopeValue(DecisionPanelScope scope, string? specialityName)
        => scope == DecisionPanelScope.Speciality
            ? specialityName is null ? "Speciality" : $"Speciality: {specialityName}"
            : WholeInstitution;

    public const string AboutTitle = "About this panel";

    public const string SeatsTitle = "Who sits on it";

    public const string DecidesForTitle = "Decides for";

    /// <summary>Under About this panel (Q8).</summary>
    public const string PanelRules =
        "Its name, scope, institution and speciality cannot change once it is created. Its members can, even while its " +
        "reviews are open.";

    /// <summary>One open review, as About lists it and the lock names it: "Review #1 (2026 S2, scheduled)".</summary>
    public static string OpenReview(PanelOpenReviewDto review)
    {
        ArgumentNullException.ThrowIfNull(review);
        return $"Review #{review.Id} ({review.Period}, {review.StateInSentence})";
    }

    /// <summary>"and 4 more" after the first open review; null when it is the only one.</summary>
    public static string? MoreOpenReviews(int count) => count <= 1 ? null : $"and {count - 1} more";

    public const string None = "None";

    /// <summary>A review's link, named for its review (the visible words inside the name, S7).</summary>
    public static string OpenReviewLink(int reviewId) => $"Open review #{reviewId}";

    /// <summary>The page after a new panel was created (step 2.22).</summary>
    public static string Created(string panelName) => $"{panelName} was created.";

    public const string CreatedNext = "Its reviews can now be scheduled on Committee reviews.";

    /// <summary>
    /// After Save panel (C12, step 2.23): "Panel members updated. Sarah Botha is off the panel. Thandi Zulu chairs, with 1
    /// member and 1 external member."
    /// </summary>
    public static string MembersUpdated(IReadOnlyList<string> takenOff, string? chairName, int members, int externals)
    {
        ArgumentNullException.ThrowIfNull(takenOff);
        var sentences = new List<string> { "Panel members updated." };
        if (takenOff.Count > 0)
        {
            sentences.Add($"{Join(takenOff)} {(takenOff.Count == 1 ? "is" : "are")} off the panel.");
        }

        var seats = $"{Count(members, "member")} and {Count(externals, "external member")}";
        sentences.Add(chairName is null ? $"The panel has {seats}." : $"{chairName} chairs, with {seats}.");
        return string.Join(" ", sentences);
    }

    /// <summary>The seat count under the picker: "2 seats: the chair, 1 member and 0 external members." (S7).</summary>
    public static string SeatCount(bool hasChair, int members, int externals)
    {
        var seats = (hasChair ? 1 : 0) + members + externals;
        return $"{Count(seats, "seat")}: {(hasChair ? "the chair" : "no chair")}, {Count(members, "member")} and " +
               $"{Count(externals, "external member")}.";
    }

    /// <summary>
    /// The members who can no longer sit, named (round 3 M6, C12): "Sarah Botha can no longer sit on this panel, so is not
    /// listed below: … Saving takes Sarah Botha off the panel."
    /// </summary>
    public static string CannotSit(IReadOnlyList<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);
        var who = Join(names);
        var one = names.Count == 1;
        return $"{who} can no longer sit on this panel, so {(one ? "is" : "are")} not listed below: only an active committee " +
               $"member at its institution who is not a trainee can. Saving takes {who} off the panel.";
    }

    /// <summary>A member with no name to show: their account no longer exists.</summary>
    public const string UnnamedMember = "A member with no account";

    /// <summary>The seat picker's caption (Q5).</summary>
    public static string SeatsCaption(string? institutionName)
        => $"Seats on the panel: the active committee members at {institutionName ?? "the panel's institution"} who are " +
           "not trainees, by surname";

    /// <summary>A seat's legend, visually hidden: "Seat for Sarah Botha".</summary>
    public static string SeatFor(string name) => $"Seat for {name}";

    public const string NotSeated = "Not seated";

    public const string Member = "Member";

    public const string ExternalMember = "External member";

    /// <summary>The chair's row in the seat picker.</summary>
    public const string ChairChosenAbove = "chosen above";

    public const string ChairLabel = "Chair";

    public const string ChooseChairOption = "Choose a chair";

    public const string CandidatesHelp =
        "Only active committee members at the panel's institution who are not trainees are listed.";

    public const string ExternalsHelp =
        "External members are external examiners, who sit with the chair on the panel's appeal body. Only active committee " +
        "members at the panel's institution who are not trainees are listed: cross-institution externals are not yet " +
        "supported.";

    /// <summary>Scope, when only one is offered, said why (board R2-2, step 2.25).</summary>
    public const string ScopeSubSpecialityAdmin =
        "A sub-speciality administrator creates Speciality-scoped panels for the speciality of the sub-speciality.";

    public const string ScopeSpecialityAdmin =
        "A speciality administrator creates Speciality-scoped panels for the speciality.";

    public const string ScopeInstitutionOnly =
        "The institution has adopted no curriculum in a speciality, so only an institution-wide panel can be created.";

    /// <summary>The scope radios' words, never the enum's name.</summary>
    public static string ScopeChoice(DecisionPanelScope scope)
        => scope == DecisionPanelScope.Institution ? "Institution" : "Speciality";

    public const string ChooseSpecialityOption = "Choose a speciality";

    /// <summary>The seat picker has nobody to offer at the panel's institution.</summary>
    public static string NobodyMaySit(string? institutionName)
        => $"No active committee member at {institutionName ?? "the panel's institution"} who is not a trainee can sit yet.";

    public const string ChooseInstitutionFirst =
        "Choose the institution first: a panel seats committee members from its own institution.";

    /// <summary>Who the chair now is, said where the focus stays, on Chair (T257, step 2.21).</summary>
    public static string NowChair(string name, bool wasExternal)
        => $"{name} is the chair now, so is no longer seated as {(wasExternal ? "an external member" : "a member")}.";

    /// <summary>The chair before, put back in the seat choosing them as chair took them from (T257).</summary>
    public static string SeatedAgain(string name, bool asExternal)
        => $"{name} is seated as {(asExternal ? "an external member" : "a member")} again.";

    public const string DecidesForHelpNew =
        "A panel sitting as a College committee decides the EPAs the College gives that committee (the neonatal CCC decides " +
        "EPAs 4 and 5), and nothing else. Where no panel sits as it, the general panels decide them.";

    public const string DecidesForHelp =
        DecidesForHelpNew + " It cannot change while a review before the panel is scheduled, in progress or awaiting " +
        "ratification.";

    public const string DecidesForReadOnly =
        "Only an institutional administrator can change which College committee a panel sits as.";

    /// <summary>The lock, said first (F5, step 4.13), before the refusal's own sentence.</summary>
    public const string CannotChangeNow = "It cannot change now.";

    /// <summary>Save committee's result.</summary>
    public static string CommitteeSaved(string? decisionBodyName)
        => decisionBodyName is null ? "The panel is a general panel." : $"The panel sits as the {decisionBodyName}.";

    /// <summary>Decides for, not saved: the summary's title, saying what the panel still is (C13).</summary>
    public static string CommitteeNotSaved(string? savedBodyName)
        => savedBodyName is null
            ? "Not saved. The panel is still a general panel."
            : $"Not saved. The panel still sits as the {savedBodyName}.";

    /// <summary>The race's short clause, the summary's link (C13; S4: a short clause, not the paragraph).</summary>
    public static string RaceClause(int reviewId) => $"Decides for: review #{reviewId} now sits before this panel";

    /// <summary>The rest of the race's summary line, after its link.</summary>
    public const string RaceExplained =
        ". What a review decides, and its agenda, follow the College committee the panel sat as when it was scheduled, so " +
        "the panel keeps what it sits as until its open reviews are ratified, or closed if formative.";

    /// <summary>Beside the select, refused by the race (C13).</summary>
    public static string RaceBeside(int reviewId, string period, string state)
        => $"Not saved: review #{reviewId} ({period}, {state}) now sits before this panel. The select shows what is saved.";

    /// <summary>Beside the select, any other refusal: the handler's own words.</summary>
    public static string NotSavedBecause(string refusal) => $"Not saved: {refusal}";

    // ─── The panel form's refusals (C7, T324, S4) ────────────────────────────

    public const string NotCreated = "The panel was not created.";

    public const string MembersNotSaved = "The panel's members were not saved.";

    public const string KeptBelow = "Everything you chose is kept below.";

    public const string ChooseAChair = "Choose a chair.";

    /// <summary>An unexpected failure of a save: fixed words; the failure goes to the log (T338).</summary>
    public const string SaveFailed =
        "The panel could not be saved just now. Nothing has changed. Try again, or come back in a few minutes.";

    public const string Cancel = "Cancel";

    public const string CreatePanel = "Create panel";

    public const string SavePanel = "Save panel";

    public const string SaveCommittee = "Save committee";

    public const string Saving = "Saving…";

    public const string GoToPanels = "Go to Decision panels";

    /// <summary>"1 seat", "2 seats", "0 members" (S7).</summary>
    public static string Count(int count, string noun)
        => $"{count} {noun}{(count == 1 ? string.Empty : "s")}";

    /// <summary>"Sarah Botha", "Sarah Botha and David Naidoo", "A, B and C".</summary>
    public static string Join(IReadOnlyList<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);
        return names.Count switch
        {
            0 => string.Empty,
            1 => names[0],
            _ => $"{string.Join(", ", names.Take(names.Count - 1))} and {names[^1]}"
        };
    }
}
