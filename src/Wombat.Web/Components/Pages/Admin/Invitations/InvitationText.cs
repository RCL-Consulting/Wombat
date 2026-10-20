using System.Globalization;
using Microsoft.AspNetCore.Components;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Components.Pages.Admin.Invitations;

/// <summary>
/// What the invitations list and Invite a person say (T283; T373, flow 11): each invitation's mail, what an issue, a resend
/// or a revoke did, the roles' descriptions, and the refusals under their fields.
/// </summary>
public static class InvitationText
{
    // ─── The list (L01–L17) ──────────────────────────────────────────────────

    public const string ListSubtitle = "Invitations not yet used, with the state of each one's email.";

    /// <summary>The Administrator's caption (L03): every invitation is theirs to see.</summary>
    public const string ActiveCaptionEverywhere =
        "Every institution and College. Not delivered first, then newest first. A used or revoked invitation leaves this list.";

    /// <summary>
    /// An Institutional admin's caption (L04): their institution, and where an expired invitation goes. A row with no
    /// institution's name (a data fault: a College's invitation never reaches them) drops the place, never "At ."
    /// (T373 fix pass, P11).
    /// </summary>
    public static string ActiveCaptionAt(string? institutionName)
        => (string.IsNullOrWhiteSpace(institutionName) ? string.Empty : $"At {institutionName}. ") +
           "Not delivered first, then newest first. A used or revoked invitation leaves this list; " +
           "one that passes its last day unused moves to Expired, not used.";

    public const string ExpiredCaption =
        "Past their last day unused, in the last 30 days on the South African calendar, newest first. Each leaves this list " +
        "30 days after its last day, or once a new invitation goes to its address.";

    public const string LoadingList = "Loading the invitations.";

    /// <summary>A failed read of the list (L16, T329): never the exception's text.</summary>
    public const string ListLoadFailed =
        "Could not load the invitations. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>A resend or a revoke that failed for no reason of its own: logged, and said in these words (T329).</summary>
    public const string ActionFailed =
        "That could not be done just now. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>"4 active", "1 active" (L04).</summary>
    public static string ActiveCount(int count) => Invariant($"{count} active");

    /// <summary>"1 expired" (L14).</summary>
    public static string ExpiredCount(int count) => Invariant($"{count} expired");

    /// <summary>Whether the invitation's current link was not delivered, so its row offers Resend.</summary>
    public static bool OffersResend(ActiveInvitationDto invitation)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        return invitation.Delivery == InvitationDelivery.NotDelivered;
    }

    /// <summary>
    /// The Delivery cell's state word (Q15: a word with an icon, no badge): Being sent, Sent, Not delivered, or Check the
    /// address once enough mails failed (<see cref="ActiveInvitationDto.SuggestCheckingAddress" />). "Not delivered" lost its
    /// full stop when its explanation moved to the note row (Steps 2.26).
    /// </summary>
    public static string Delivery(ActiveInvitationDto invitation)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        return invitation.SuggestCheckingAddress ? "Check the address" : Delivery(invitation.Delivery);
    }

    /// <summary>The state word for a link's mail.</summary>
    public static string Delivery(InvitationDelivery delivery) => delivery switch
    {
        InvitationDelivery.Sent => "Sent",
        InvitationDelivery.BeingSent => "Being sent",
        _ => "Not delivered"
    };

    /// <summary>The Delivery cell's icon (Q15): clock while being sent, circle-check once sent, circle-alert otherwise.</summary>
    public static string DeliveryIcon(InvitationDelivery delivery) => delivery switch
    {
        InvitationDelivery.Sent => "circle-check",
        InvitationDelivery.BeingSent => "clock",
        _ => "circle-alert"
    };

    /// <summary>The Delivery cell's class: muted while being sent, the success icon once sent, 600 and warning otherwise.</summary>
    public static string DeliveryClass(InvitationDelivery delivery) => delivery switch
    {
        InvitationDelivery.Sent => "delivery delivery--sent",
        InvitationDelivery.BeingSent => "delivery delivery--sending",
        _ => "delivery delivery--warn"
    };

    /// <summary>
    /// What a row not delivered says first in its note row, once enough of the invitation's mails have failed that the
    /// address may be what is wrong (<see cref="ActiveInvitationDto.SuggestCheckingAddress" />); null before then.
    /// </summary>
    /// <remarks>
    /// A mail server that always refuses an address (a mistyped domain) leaves it not delivered however often it is resent,
    /// and the product cannot tell that from a mail server that was down. Only an administrator can: by reading the
    /// address. A wrong address is not corrected in place, since the invitation names who may register.
    /// </remarks>
    public static string? CheckAddress(ActiveInvitationDto invitation)
    {
        ArgumentNullException.ThrowIfNull(invitation);
        if (!invitation.SuggestCheckingAddress)
        {
            return null;
        }

        var failures = invitation.DeliveryFailures.ToString(CultureInfo.InvariantCulture);
        return $"Its email has failed {failures} times, so the mail server may be refusing this address. " +
               "Check the address; if it is wrong, revoke this invitation and issue a new one to the right address.";
    }

    /// <summary>
    /// What a row not delivered says last in its note row, after any <see cref="CheckAddress" />: that Resend retires the
    /// current link. The OneTimeLink tells the administrator who issued it that the link may be shared another way; this is
    /// where anyone about to press Resend on that row (the row's Resend names this note) learns that it would stop a shared
    /// link working. (T283 review)
    /// </summary>
    public const string ResendRetiresTheLink =
        "Resend emails a new link in place of the current one, which then stops working.";

    /// <summary>A row's note (L09, L12): <see cref="CheckAddress" /> when it applies, then <see cref="ResendRetiresTheLink" />.</summary>
    public static string Note(ActiveInvitationDto invitation)
        => CheckAddress(invitation) is { } check ? $"{check} {ResendRetiresTheLink}" : ResendRetiresTheLink;

    /// <summary>
    /// A row's second line under the role (L03, L04; Q18): the speciality over its sub-speciality, "Any speciality" for a
    /// Coordinator or Committee member kept to none, the place for a role that takes no speciality; and for the
    /// Administrator, who sees every institution and College, the place too ("Paediatrics / Paediatrics at Kgosi Kgari
    /// Teaching Hospital").
    /// </summary>
    public static string? ScopeLine(
        string targetRole,
        string? institutionName,
        string? collegeName,
        string? specialityName,
        string? subSpecialityName,
        bool withPlace)
    {
        var disciplines = InvitationScopeWords.Disciplines(specialityName, subSpecialityName)
            ?? (targetRole is WombatRoles.Coordinator or WombatRoles.CommitteeMember ? AnySpeciality : null);
        var place = string.IsNullOrWhiteSpace(institutionName) ? collegeName : institutionName;

        if (disciplines is null)
        {
            return place;
        }

        return withPlace && !string.IsNullOrWhiteSpace(place) ? $"{disciplines} at {place}" : disciplines;
    }

    /// <summary>"Trainee, Paediatrics / Paediatrics", or the label alone: what "Invite … as" and "Invite again … as" say.</summary>
    public static string RoleWithScope(string targetRole, string? scopeLine)
        => scopeLine is null ? WombatRoleLabels.For(targetRole) : $"{WombatRoleLabels.For(targetRole)}, {scopeLine}";

    /// <summary>"Issued": the South African date over its time, "2026-10-19" over "09:22 SAST" (S3, T325).</summary>
    public static (string Date, string Time) IssuedParts(DateTime issuedOnUtc)
    {
        var when = ActivityMoments.When(issuedOnUtc);
        var space = when.IndexOf(' ', StringComparison.Ordinal);
        return space < 0 ? (when, string.Empty) : (when[..space], when[(space + 1)..]);
    }

    /// <summary>A date as every page prints one: ISO.</summary>
    public static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    // ─── The revoke (L06, L07) ───────────────────────────────────────────────

    public const string RevokeTitle = "Revoke this invitation?";
    public const string RevokeKeep = "Keep the invitation";
    public const string RevokeConfirm = "Revoke invitation";
    public const string RevokeStopsTheLink =
        "The registration link already emailed to that address stops working at once: nobody can register from it.";
    public const string RevokeLeavesTheList =
        "The invitation leaves this list. An address is never corrected in place: to invite the right one, issue a new invitation.";
    public const string RevokeCannotBeUndone = "Revoking cannot be undone.";

    // ─── Issue and resend: the OneTimeLink (S10) ─────────────────────────────

    /// <summary>
    /// What follows an issue's bold "Invitation issued for ‹address›." (N03), which the page writes with the address's break
    /// (M7). One copy of each result's sentences (T373 fix pass, R2): the helpers that wrote a whole result as one string,
    /// which no page could use since the address needs its break in markup, went.
    /// </summary>
    public const string IssuedRest = "Its email is being sent. Copy the link below — it is shown only once.";

    /// <summary>What follows a resend's "A new invitation link is being emailed to ‹address›." (L10).</summary>
    public const string ResentRest = "The link it replaces no longer works. Copy the new link below — it is shown only once.";

    /// <summary>The link's help after a resend (L10).</summary>
    public static string ResentLinkHelp(DateOnly expiresOn)
        => $"It works until {Iso(expiresOn)}. It is shown here until your next action, then never again.";

    /// <summary>
    /// The link's help after an issue (N03, F7): until when it works, that it is shown once, and what to do if the mail
    /// does not arrive. Until T373 the link's alert said it "expires in 14 days".
    /// </summary>
    public static string IssuedLinkHelp(DateOnly expiresOn)
        => ResentLinkHelp(expiresOn) + " If the email is not delivered, share this link another way, or resend it from " +
           "Invitations, which emails a new link in place of this one.";

    public const string CopyLink = "Copy link";
    public const string Copied = "Copied";
    public const string LinkCopied = "Link copied.";
    public const string CopyFailed = "Could not copy. Select the link and copy it yourself.";
    public const string OpenInvitations = "Open Invitations";

    // ─── Invite a person (N01–N26) ───────────────────────────────────────────

    public const string InviteSubtitle = "One person, one role. The registration link is shown here once, and emailed to them.";
    public const string RequiredMarkLine = "* marks a field you must fill in.";
    public const string NextPersonHeading = "Invite the next person";
    public const string LoadingForm = "Loading what you can invite.";

    /// <summary>A failed read of the form's choices (N23, T329).</summary>
    public const string FormLoadFailed =
        "Could not load what you can invite. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>An issue that failed for no reason of its own: logged, and said in these words (T329).</summary>
    public const string IssueFailed =
        "The invitation could not be issued just now. Nothing was saved. Try again, or come back in a few minutes.";

    public const string AnySpeciality = "Any speciality";
    public const string ChooseCollege = "Choose a College";
    public const string ChooseInstitution = "Choose an institution";
    public const string ChooseSpeciality = "Choose a speciality";
    public const string ChooseSubSpeciality = "Choose a sub-speciality";

    /// <summary>Speciality's help for a Coordinator or Committee member, who may be kept to none (F9, Q18).</summary>
    public const string AnySpecialityHelp = "Leave it at Any speciality for the whole institution.";

    /// <summary>Speciality's help, until the sub-speciality is chosen, for a role that needs it too (N08a; R4).</summary>
    public const string ThenSubSpecialityHelp = "Then choose its sub-speciality.";

    /// <summary>Nothing adopted (N26, S13), before its link to Curriculum adoptions.</summary>
    public static string NothingAdopted(string institutionName)
        => $"Nothing is adopted at {institutionName} yet, so roles that need a speciality cannot be invited. Adopt a curriculum under";

    /// <summary>The roles Invite a person offers, in its order (N01, N02): College admin to the Administrator only.</summary>
    public static readonly IReadOnlyList<string> Roles =
    [
        WombatRoles.CollegeAdmin,
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.Coordinator,
        WombatRoles.CommitteeMember,
        WombatRoles.Assessor,
        WombatRoles.Trainee
    ];

    /// <summary>Each role's description under its radio (N01, N02; S17).</summary>
    public static string RoleHelp(string role) => role switch
    {
        WombatRoles.CollegeAdmin => "Keeps a College's specialities, EPAs and curricula. Choose the College.",
        WombatRoles.InstitutionalAdmin => "Runs an institution in Wombat: invites and admits people, adopts curricula.",
        WombatRoles.SpecialityAdmin => "Runs one speciality's committee work: panels, reviews and decisions due.",
        WombatRoles.SubSpecialityAdmin => "Runs one sub-speciality's committee work. Choose the speciality, then the sub-speciality.",
        WombatRoles.Coordinator => "Works across the institution. Choose a speciality only to keep them to it.",
        WombatRoles.CommitteeMember => "Sits committee reviews. Choose a speciality only to keep them to it.",
        WombatRoles.Assessor => "Assesses the registrars who ask them, at this institution.",
        WombatRoles.Trainee => "A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "A role Invite a person does not offer.")
    };

    /// <summary>A role's radio id: "role-institutional-admin".</summary>
    public static string RoleInputId(string role) => "role-" + string.Concat(role.Select((character, index) =>
        char.IsUpper(character) && index > 0 ? "-" + char.ToLowerInvariant(character) : char.ToLowerInvariant(character).ToString()));

    /// <summary>The roles that need a speciality: Speciality is starred for them (N08, N10).</summary>
    public static bool NeedsSpeciality(string? role)
        => role is WombatRoles.SpecialityAdmin or WombatRoles.SubSpecialityAdmin or WombatRoles.Assessor or WombatRoles.Trainee;

    /// <summary>The roles that may be kept to a speciality, or left at Any speciality (F9, Q18).</summary>
    public static bool MayHaveSpeciality(string? role)
        => role is WombatRoles.Coordinator or WombatRoles.CommitteeMember;

    /// <summary>The roles that need a sub-speciality as well (N08, N11).</summary>
    public static bool NeedsSubSpeciality(string? role)
        => role is WombatRoles.SubSpecialityAdmin or WombatRoles.Assessor or WombatRoles.Trainee;

    // ─── The refusals under Email (N15–N18) ─────────────────────────────────

    /// <summary>N15 after its address, which the page writes with a break after its @ (M7).</summary>
    public static string AccountHereAfterAddress(string name)
        => $" already has a Wombat account ({name}), so it cannot be invited. To give {name} another role,";

    public static string OpenUnderUsers(string name) => $"open {name} under Users";

    /// <summary>N15 for an account in scope with no name, an erased or nameless one (T373 fix pass, B6).</summary>
    public const string AccountHereAfterAddressNoName =
        " already has a Wombat account, so it cannot be invited. To give the account another role,";

    /// <summary>N15's link for an account with no name.</summary>
    public const string OpenItUnderUsers = "open it under Users";

    /// <summary>N16: an account outside the caller's scope, named by nothing.</summary>
    public const string AccountElsewhere = "An account with this address already exists. Ask a Wombat administrator to add the role.";

    /// <summary>N17 after its address.</summary>
    public static string ActiveHereAfterAddress(DateTime issuedOnUtc, DateOnly lastDay)
        => $" already has an active invitation, issued {ActivityMoments.When(issuedOnUtc)}, which works until " +
           $"{Iso(lastDay)}. Resend it or revoke it under";

    /// <summary>N18: an invitation at another institution, named by nothing.</summary>
    public const string ActiveElsewhere =
        "This address already has an active invitation from another institution. Ask a Wombat administrator.";

    /// <summary>N18's College case.</summary>
    public const string ActiveAtACollege = "This address already has an active invitation from a College. Ask a Wombat administrator.";

    /// <summary>
    /// <paramref name="email" /> with a <c>&lt;wbr&gt;</c> after its @ (M7), so a long address breaks there first: in markup,
    /// never in a stored string, and never in an attribute.
    /// </summary>
    public static RenderFragment Address(string email) => builder =>
    {
        var at = email.IndexOf('@', StringComparison.Ordinal);
        if (at < 0)
        {
            builder.AddContent(0, email);
            return;
        }

        builder.AddContent(0, email[..(at + 1)]);
        builder.AddMarkupContent(1, "<wbr>");
        builder.AddContent(2, email[(at + 1)..]);
    };

    private static string Invariant(FormattableString value) => value.ToString(CultureInfo.InvariantCulture);
}
