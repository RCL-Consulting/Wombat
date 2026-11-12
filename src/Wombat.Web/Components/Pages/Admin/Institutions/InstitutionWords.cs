using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;

namespace Wombat.Web.Components.Pages.Admin.Institutions;

/// <summary>
/// The words of the institutions list, the create page and the Administrator's record (T400, flow 18, lane A; boards
/// R2-I01–I06, I17–I21, R3-I07–I16, I22; R3-0-spec-2 § 9), in one place so the pages and their tests read the same.
/// </summary>
/// <remarks>
/// The institution's own administrator's record keeps flow 12's words (<c>InstitutionEdit</c>'s constants), and the
/// refusals are the commands' own (<c>UpdateInstitutionCommandValidator</c>).
/// </remarks>
public static class InstitutionWords
{
    // ─── The list (R2-I01–I06) ──────────────────────────────────────────────

    public const string ListHeading = "Institutions";

    /// <summary>Q5's subtitle (T12).</summary>
    public const string ListSubtitle = "The institutions that hold Wombat's people and records.";

    public const string Caption = "Institutions";

    public const string LoadingInstitutions = "Loading the institutions.";

    /// <summary>The house load error (R2-I05; T329): fixed words, the failure logged.</summary>
    public const string InstitutionsLoadFailed =
        "Could not load the institutions. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The empty list, as built (no board draws it: one institution exists from the first boot).</summary>
    public const string EmptyTitle = "No institutions yet";

    public const string EmptyBody = "Create the first institution to start configuring scope and onboarding.";

    /// <summary>A contact email the institution does not have (R2-I06).</summary>
    public const string NoContactEmail = "None";

    public const string Active = "Active";

    public const string Inactive = "Inactive";

    public const string Edit = "Edit";

    // ─── The create page (R3-I07–I10) ───────────────────────────────────────

    public const string CreateHeading = "Create institution";

    /// <summary>T12.</summary>
    public const string CreateSubtitle =
        "A new institution is active from the moment it is saved, so its people can be invited.";

    public const string NameHelp = "Up to 200 characters.";

    public const string ShortCodeHelp = "Up to 32 characters.";

    public const string ContactEmailHelp = "Optional.";

    public const string Cancel = "Cancel";

    public const string Save = "Save";

    public const string Saving = "Saving...";

    // ─── The Administrator's record (R3-I11–I16, I22; R2-I17–I20) ───────────

    /// <summary>The h1 and tab of an unknown or unreadable id, for both roles (Q17, T10).</summary>
    public const string UnavailableHeading = "Institution unavailable";

    public const string UnavailableBody = "This institution does not exist, or you cannot open it.";

    public const string GoToInstitutions = "Go to Institutions";

    public const string GoToHome = "Go to Home";

    public const string StatusHeading = "Status";

    /// <summary>The Status sentences (T12).</summary>
    public const string ActiveSentence = "A new invitation can name it.";

    public const string InactiveSentence =
        "No new invitation can name it. Its people, their records and the invitations already sent are kept.";

    /// <summary>The triggers, each opening its question (T11, M2); "…" because each opens a dialog.</summary>
    public const string DeactivateTrigger = "Deactivate…";

    public const string ReactivateTrigger = "Reactivate…";

    public const string DeactivateConfirm = "Deactivate";

    public const string ReactivateConfirm = "Reactivate";

    /// <summary>The deactivation question's body, as built (<c>InstitutionEdit.razor</c>).</summary>
    public const string DeactivateBody =
        "No new invitation can be issued for it until it is reactivated. Its people, their records and the invitations " +
        "already sent are kept. You can reactivate it on this page.";

    /// <summary>T11.</summary>
    public const string ReactivateBody = "A new invitation can name it again. Its people and their records are as they were.";

    /// <summary>T23, as built.</summary>
    public const string DeactivateFailed =
        "Could not deactivate the institution. Nothing has changed. Try again, or come back in a few minutes.";

    public const string ReactivateFailed =
        "Could not reactivate the institution. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The record's subtitle, "KGK · Active" (N2).</summary>
    public static string RecordSubtitle(string shortCode, bool isActive) => $"{shortCode} · {Status(isActive)}";

    public static string Status(bool isActive) => isActive ? Active : Inactive;

    public static string DeactivateTitle(string name) => $"Deactivate {name}?";

    public static string ReactivateTitle(string name) => $"Reactivate {name}?";

    /// <summary>The created result (T11, R3-I11).</summary>
    public static string Created(string name) => $"{name} created.";

    public static string Deactivated(string name) => $"{name} deactivated. A new invitation can no longer name it.";

    public static string Reactivated(string name) => $"{name} reactivated. A new invitation can name it again.";

    /// <summary>A failure's first sentence, which the result bolds (R3-I22), and the rest.</summary>
    public static (string Lead, string Remainder) SplitFirstSentence(string message)
    {
        var end = message.IndexOf(". ", StringComparison.Ordinal);
        return end < 0 ? (message, string.Empty) : (message[..(end + 1)], message[(end + 2)..]);
    }
}

/// <summary>
/// The three fields of an institution's record, as the create page and the record hold them, with the page's own checks
/// and the command's refusals placed under their fields (T376's pattern, T378; R3-I08–I10).
/// </summary>
internal sealed class InstitutionFields
{
    private static readonly string[] Order = [InstitutionEdit.NameId, InstitutionEdit.ShortCodeId, InstitutionEdit.EmailId];

    public string Name { get; set; } = string.Empty;

    public string ShortCode { get; set; } = string.Empty;

    public string? ContactEmail { get; set; }

    /// <summary>The id of a field's refusal, which the field names first in its <c>aria-describedby</c>.</summary>
    public static string MessageId(string fieldId) => $"{fieldId}-msg";

    public static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>
    /// The page's own checks, in the validator's words (T324): each refusal into <paramref name="errors" /> by its field's
    /// id; the first refused field's id, or null when all pass. Nothing is sent while one is refused.
    /// </summary>
    public string? Check(IDictionary<string, string> errors)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            errors[InstitutionEdit.NameId] = UpdateInstitutionCommandValidator.NameMissing;
        }
        else if (Name.Trim().Length > 200)
        {
            errors[InstitutionEdit.NameId] = UpdateInstitutionCommandValidator.NameTooLong;
        }

        if (string.IsNullOrWhiteSpace(ShortCode))
        {
            errors[InstitutionEdit.ShortCodeId] = UpdateInstitutionCommandValidator.ShortCodeMissing;
        }
        else if (ShortCode.Trim().Length > 32)
        {
            errors[InstitutionEdit.ShortCodeId] = UpdateInstitutionCommandValidator.ShortCodeTooLong;
        }

        if (EmptyToNull(ContactEmail) is { } email
            && (email.Length > 320 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email)))
        {
            errors[InstitutionEdit.EmailId] = UpdateInstitutionCommandValidator.ContactEmailMalformed;
        }

        return FirstInvalid(errors);
    }

    /// <summary>
    /// A command's refusal of a field, placed under it: a name or short code another institution holds (the record's own
    /// refusal, by its field, never by its text), or the validator's messages by property. The first refused field's id;
    /// null when the exception refuses no field, which the page then shows in its result.
    /// </summary>
    public static string? FieldOf(Exception exception, IDictionary<string, string> errors)
    {
        switch (exception)
        {
            case InstitutionUpdateRefusedException { Field: not InstitutionRecordField.None } refusal
                when IdOf(refusal.Field) is { } fieldId:
                errors[fieldId] = refusal.Message;
                return fieldId;
            case FluentValidation.ValidationException validation when validation.Errors.Any():
                foreach (var failure in validation.Errors)
                {
                    var failed = failure.PropertyName switch
                    {
                        "Name" => InstitutionEdit.NameId,
                        "ShortCode" => InstitutionEdit.ShortCodeId,
                        _ => InstitutionEdit.EmailId
                    };
                    if (!errors.ContainsKey(failed))
                    {
                        errors[failed] = failure.ErrorMessage;
                    }
                }

                return FirstInvalid(errors);
            default:
                return null;
        }
    }

    private static string? IdOf(InstitutionRecordField field) => field switch
    {
        InstitutionRecordField.Name => InstitutionEdit.NameId,
        InstitutionRecordField.ShortCode => InstitutionEdit.ShortCodeId,
        InstitutionRecordField.ContactEmail => InstitutionEdit.EmailId,
        _ => null
    };

    private static string? FirstInvalid(IDictionary<string, string> errors) => Order.FirstOrDefault(errors.ContainsKey);
}
