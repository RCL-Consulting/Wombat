using System.Globalization;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// The two hard bounds on an encounter date (T160, from T119's design): not after today, for every type; and not before
/// the trainee's programme began, only for a type that can credit (<see cref="EncounterDatePolicy.CanCredit" />). Which
/// writes are put to it is the caller's decision (<c>ActivityService</c>); this class only judges.
/// </summary>
/// <remarks>
/// <para>
/// It judges the date the stamp would write: <see cref="ObservationDateResolver" /> over the PINNED schema's
/// <c>observation_date_field</c>, so a hidden field, a malformed value or a schema with no pointer is judged exactly as
/// the stamp treats it. A date nobody stated falls back to the day the activity was created and is never refused.
/// </para>
/// <para>
/// "Today" is the South African calendar date (<see cref="Wombat.Domain.Curricula.ProgrammeCalendar" />), the calendar
/// a clinician types the encounter date in. On a UTC "today" a registrar filing at 01:00 on the day of the encounter
/// would be refused for dating it tomorrow.
/// </para>
/// <para>
/// The programme start is read from the profile credit uses (<see cref="CreditTargetResolver.PickProfileAsync" />): a
/// date before it gets no stage from <c>TraineeProfile.GetStage</c>, which silently drops a completion to the flat
/// minimum instead of a stage minimum. That is a harm only to credit, so a type whose pinned rules credit nothing (a
/// research output, a journal club, a reflective exercise) gets the future check only, and may be dated from before the
/// trainee was admitted. So does a subject with no trainee profile. A refusal is a field error in
/// <c>SchemaValidator</c>'s shape, against the date field, so the form's author reads it as one.
/// </para>
/// <para>
/// The refusal's words are neutral ("The date cannot be after today"), never "the encounter date" (T197): the date field
/// is whatever the type calls it (a portfolio review's "Review period to", an MSF release's "Feedback window closed"),
/// and the message is always shown behind that field's own label (<c>ActivityService.ThrowIfInvalid</c>).
/// </para>
/// <para>
/// There is no bound at the programme's end. An encounter observed after the profile's last day credits nothing on it
/// (T281), and that is <c>CreditApplier</c>'s to apply, not this gate's to refuse. The end can be recorded after the
/// fact (T209), on activities already filed, so a refusal here would land on whoever next moves one: an assessor
/// completing it, who cannot change the date, or an author pushed to type a date inside the programme, which would then
/// credit. The activity is kept, and its completion is stamped as crediting nothing, which the activity page warns of.
/// </para>
/// <para>
/// Lateness is not this class's business: D15 never refuses a late filing (<see cref="EncounterDatePolicy" />).
/// Every read is awaited before the caller's first mutation, and the profile is read untracked, so a refusal leaves
/// the request's DbContext clean for the audit pipeline's save.
/// </para>
/// </remarks>
internal static class EncounterDateGate
{
    /// <summary>
    /// Whether the encounter date the stamp would write differs between two payloads: another stated date, or a date
    /// stated on one side only. The values are compared as the resolver parses them, never as stored text.
    /// </summary>
    public static bool Changed(Activity activity, FormSchema schema, string storedDataJson, string newDataJson)
        => ObservationDateResolver.Resolve(activity, schema, storedDataJson) !=
           ObservationDateResolver.Resolve(activity, schema, newDataJson);

    /// <param name="creditRulesJson">
    /// The credit rules of the version the activity is pinned to. The programme-start bound applies only when they can
    /// credit (<see cref="EncounterDatePolicy.CanCredit" />). Null on the system-written path (an MSF release), which gets
    /// the future check only whatever its type: a record nobody typed has nobody who could correct it.
    /// </param>
    public static async Task<IReadOnlyList<ActivityValidationErrorDto>> ValidateAsync(
        IApplicationDbContext dbContext,
        Activity activity,
        FormSchema schema,
        string dataJson,
        DateOnly today,
        string? creditRulesJson,
        CancellationToken cancellationToken)
    {
        var (encounteredOn, source) = ObservationDateResolver.Resolve(activity, schema, dataJson);
        if (source != ObservationDateSource.Declared || string.IsNullOrWhiteSpace(schema.ObservationDateField))
        {
            return [];
        }

        var field = schema.ObservationDateField;

        if (encounteredOn > today)
        {
            return
            [
                new ActivityValidationErrorDto(
                    field,
                    $"The date cannot be after today ({Format(today)}).",
                    "after_today")
            ];
        }

        if (!EncounterDatePolicy.CanCredit(creditRulesJson))
        {
            return [];
        }

        var profile = await CreditTargetResolver.PickProfileAsync(dbContext, activity.SubjectUserId, cancellationToken);
        if (profile is not null && encounteredOn < profile.ProgrammeStartDate)
        {
            return
            [
                new ActivityValidationErrorDto(
                    field,
                    $"The date cannot be before the trainee's programme started ({Format(profile.ProgrammeStartDate)}).",
                    "before_programme")
            ];
        }

        return [];
    }

    private static string Format(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
