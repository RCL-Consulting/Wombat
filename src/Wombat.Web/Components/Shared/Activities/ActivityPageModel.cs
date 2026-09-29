using System.Globalization;
using System.Text.Json;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Curricula;
using Wombat.Web.Navigation;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// Everything the activity page says about one activity, worked out once per load from the detail the server sent
/// (T342, flow 03; R3-C-Activity, R3-Spec § 1 and § 2): its name, the subtitle that names its people, the status card,
/// the About card, and the words its moves and dialogs use. Pure, so a test can hold every sentence to the Spec without
/// rendering the page.
/// </summary>
public sealed class ActivityPageModel
{
    /// <summary>"It is in nobody's inbox until you submit it." (C2: a draft is not private, so the page never says so.)</summary>
    public const string NobodysInbox = FilingWords.NobodysInboxSentence;

    private ActivityPageModel(ActivityDetailDto detail, string? viewerId)
    {
        Detail = detail;
        Activity = detail.Activity;
        ViewerId = viewerId;
        Workflow = PinnedWorkflows.TryParse(Activity.WorkflowJson);
        Schema = TryParseSchema(Activity.SchemaJson);
    }

    public ActivityDetailDto Detail { get; }

    public ActivityDto Activity { get; }

    public string? ViewerId { get; }

    public Workflow? Workflow { get; }

    public FormSchema? Schema { get; }

    /// <summary>The page's h1, tab title and last crumb: "Type · EPA · date", with " · nominee" on a collision (E2, E7).</summary>
    public string Heading => string.IsNullOrWhiteSpace(Detail.DisplayName) ? Activity.ActivityTypeName : Detail.DisplayName;

    /// <summary>The registrar the activity is about, by name; the create row's actor when the detail carries none.</summary>
    public string SubjectName
        => Detail.SubjectName
           ?? Activity.Transitions
               .FirstOrDefault(row => string.Equals(row.ActorUserId, Activity.SubjectUserId, StringComparison.Ordinal))
               ?.ActorName
           ?? "The registrar";

    /// <summary>The viewer is the registrar the activity is about.</summary>
    public bool ViewerIsSubject
        => ViewerId is not null && string.Equals(ViewerId, Activity.SubjectUserId, StringComparison.Ordinal);

    /// <summary>The viewer is its author: its subject or its creator, the actors <c>subject|creator</c> names.</summary>
    public bool ViewerIsAuthor
        => ViewerIsSubject ||
           (ViewerId is not null && string.Equals(ViewerId, Activity.CreatedByUserId, StringComparison.Ordinal));

    public ActivityHolderKind HolderKind => Detail.Holder?.Kind ?? FallbackHolderKind();

    /// <summary>The viewer holds the next move: their own draft or returned work, or a request made to them.</summary>
    public bool ViewerHolds => Detail.Holder is { IsViewer: true } holder
        ? holder.Kind is ActivityHolderKind.Author or ActivityHolderKind.Person
        : Detail.Holder is null && HolderKind == ActivityHolderKind.Author && ViewerIsAuthor;

    /// <summary>A record nothing more can happen to: finished, or closed at a dead end.</summary>
    public bool IsOver => HolderKind is ActivityHolderKind.Done or ActivityHolderKind.Closed;

    /// <summary>It is in its workflow's first state (a draft, or work returned to its author).</summary>
    public bool InInitialState
        => Workflow is null || string.Equals(Activity.CurrentState, Workflow.InitialState, StringComparison.Ordinal);

    /// <summary>The pinned credit rules credit nothing (an empty <c>counts_for</c>).</summary>
    public bool CreditsNothing => !FilingLateness.WarnsFor(Activity.CreditRulesJson);

    /// <summary>
    /// The move that filed it: the author's first move out of the initial state that leads on (<c>ActivityService.IsTheFiling</c>),
    /// or the create itself where the create is the filing (T148: a type born in <c>requested</c>, or born terminal), as
    /// <see cref="SectionAttributions" /> reads it (T342; the build review's G3). A born-logged journal club is then not
    /// "a draft", a cancelled born-requested request was submitted, and its lateness is on its create row.
    /// </summary>
    public ActivityTransitionDto? Filing
        => Workflow is null
            ? null
            : Ordered(Activity.Transitions).FirstOrDefault(row =>
                Workflow.LeftInitialStateLeadingOn(row.FromState, row.ToState, row.TransitionKey))
              ?? (CreateFiles(out var create) ? create : null);

    /// <summary>
    /// Whether the create was the filing: no move out of the initial state that leads on is its author's (T148), so the
    /// create, not a later move, filed it and filled the author's sections in.
    /// </summary>
    private bool CreateFiles(out ActivityTransitionDto? create)
    {
        create = Ordered(Activity.Transitions).FirstOrDefault(row => IsCreate(row.TransitionKey));
        if (create is null || Workflow is null)
        {
            return false;
        }

        var creator = create.ActorUserId;
        var data = StoredData();
        return !Workflow.TransitionsLeadingOn(Workflow.InitialState)
            .Any(transition => Matches(transition.Actor, creator, data));
    }

    /// <summary>The move that brought it into its current state; null for a draft with only its create row.</summary>
    public ActivityTransitionDto? LastMove
        => Ordered(Activity.Transitions).LastOrDefault(row =>
            !string.Equals(row.TransitionKey, Wombat.Domain.Activities.Workflow.Workflow.CreateTransitionKey, StringComparison.Ordinal));

    /// <summary>The nominee field's label ("Assessor", "Supervisor or mentor", "Reviewer"), or null for no nominee.</summary>
    public string? NomineeLabel
        => ActivityHolders.NomineeField(Workflow, Activity.CurrentState) is { } nomineeField && Schema is not null
            ? Schema.FieldLabel(nomineeField)
            : null;

    /// <summary>What the form's first section calls the author's part, lower-cased: "request", "reflection".</summary>
    public string AuthorPart
        => Schema?.Sections.FirstOrDefault()?.Title is { } title && !string.IsNullOrWhiteSpace(title)
            ? LowerFirstWords(title.Trim())
            : "activity";

    /// <summary>The type as a common noun, for "A reflective exercise credits nothing" ("reflective exercise").</summary>
    public string Noun => NounOf(Activity.ActivityTypeName);

    /// <summary>
    /// The page's subtitle: whose it is, and whom it goes to (E2; the boards' subtitles; the build review's D5).
    /// <list type="bullet">
    ///   <item>a draft never filed and then cancelled: "Pieter du Plessis's draft, cancelled";</item>
    ///   <item>a request, the review's too ("Review request"): "Sipho Ndlovu's request to Fatima Khumalo";</item>
    ///   <item>a reflection: "Sipho Ndlovu's reflection, for discussion with Sarah Botha";</item>
    ///   <item>any other part with a nominee: "Sipho Ndlovu's audit, with Sarah Botha";</item>
    ///   <item>a draft with nobody named: "Anele Dlamini's draft"; else "Sipho Ndlovu's journal club".</item>
    /// </list>
    /// </summary>
    public string Subtitle
    {
        get
        {
            var subject = Possessive(SubjectName);
            if (Filing is null && HolderKind == ActivityHolderKind.Closed)
            {
                return $"{subject} draft, {StateWord}";
            }

            if (Detail.NomineeName is { } nominee)
            {
                if (AuthorPart.Contains("request", StringComparison.Ordinal))
                {
                    return $"{subject} request to {nominee}";
                }

                return AuthorPart.Contains("reflection", StringComparison.Ordinal)
                    ? $"{subject} {AuthorPart}, for discussion with {nominee}"
                    : $"{subject} {AuthorPart}, with {nominee}";
            }

            return InInitialState && Filing is null ? $"{subject} draft" : $"{subject} {AuthorPart}";
        }
    }

    private string StateWord => Activity.CurrentStateLabel.ToLowerInvariant();

    /// <summary>
    /// The Cancel move the server offers this viewer here (C11), or null. Not only the author's: the demo types' cancel is
    /// <c>subject|field:assessor_user_id</c>, so their assessor may cancel too (the build review's G1). The server decides,
    /// so the page offers it to whoever it lists it for.
    /// </summary>
    public ActivityActionDto? CancelAction
        => Detail.AvailableActions.FirstOrDefault(action => action.IsAvailable && IsCancel(action));

    /// <summary>
    /// The status card carries Cancel while someone else holds the activity, or no one person does: the viewer has no bar
    /// of moves to put it in (C11). Otherwise it is last in the bar, on a draft or on a request the viewer holds.
    /// </summary>
    public bool CancelOnCard
        => CancelAction is not null &&
           HolderKind is ActivityHolderKind.Person or ActivityHolderKind.Waiting &&
           Detail.Holder is not { IsViewer: true };

    /// <summary>Cancel, last in the bar (C11), whoever it is offered to; null when the card carries it or there is none.</summary>
    public ActivityActionDto? BarCancel => CancelOnCard ? null : CancelAction;

    /// <summary>Whether <paramref name="action" /> withdraws the activity: the author's move to a dead end (C11).</summary>
    public static bool IsCancel(ActivityActionDto action)
        => action.TransitionKey is "cancel" or "withdraw";

    /// <summary>
    /// "Cancel request…" while another person holds it, "Cancel this draft…" on a draft, including a returned one (C11).
    /// </summary>
    public string CancelLabel => IsDraft ? "Cancel this draft…" : "Cancel request…";

    /// <summary>
    /// It is a draft: in its initial state, where the author's own move files it. A type born in <c>requested</c> starts
    /// as a request, not a draft, so its assessor's Cancel says "request" (G1, G3).
    /// </summary>
    public bool IsDraft => InInitialState && !CreateFiles(out _);

    /// <summary>The Cancel dialog's words (R3-Spec § 1, Confirm).</summary>
    public (string Title, string Body, string Keep, string Go) CancelDialog
        => IsDraft
            ? ("Cancel this draft?", "A cancelled draft cannot be reopened, and it credits nothing.", "Keep the draft", "Cancel draft")
            : ("Cancel this request?",
               (Detail.Holder is { Kind: ActivityHolderKind.Person, IsViewer: true }
                   ? "It leaves your Activity inbox. "
                   : Detail.Holder is { Kind: ActivityHolderKind.Person, Name: { } name }
                       ? $"It leaves {name}'s Activity inbox. "
                       : string.Empty) + "A cancelled request cannot be reopened, and it credits nothing.",
               "Keep the request",
               "Cancel request");

    /// <summary>The status card (Q3, R3-Spec § 1 "Status card").</summary>
    public ActivityStatusView Status(string? ratedLabel)
    {
        var holder = Detail.Holder;
        var badgeClass = "badge " + BadgeFor.ActivityState(Activity.CurrentState, HolderKind == ActivityHolderKind.Done);
        var badge = Activity.CurrentStateLabel;
        var meta = HolderKind is ActivityHolderKind.Person or ActivityHolderKind.Waiting &&
                   FilingLateness.Label(Filing?.DaysAfterEncounter) is not null
            ? $"Filed {Filing!.DaysAfterEncounter} days after the encounter: recorded as late."
            : null;

        switch (HolderKind)
        {
            case ActivityHolderKind.Author:
                if (ViewerHolds)
                {
                    if (Detail.Returned is { } returned)
                    {
                        return new ActivityStatusView(
                            ActivityStatusTone.Yours, badgeClass, badge,
                            $"With you. {returned.ByName} returned it on {ActivityMoments.When(returned.ReturnedOn)}.",
                            Quote: string.IsNullOrWhiteSpace(returned.Note) ? null : returned.Note, QuoteBy: returned.ByName,
                            Body: $"Change your {AuthorPart} and submit it again.");
                    }

                    return new ActivityStatusView(
                        ActivityStatusTone.Yours, badgeClass, badge, "With you. Not submitted yet.",
                        Body: $"Finish the {AuthorPart} and submit it. {NobodysInbox}");
                }

                var author = holder?.Name ?? SubjectName;
                return new ActivityStatusView(
                    ActivityStatusTone.Others, badgeClass, badge,
                    Detail.Returned is { } back
                        ? $"With {author}. {back.ByName} returned it on {ActivityMoments.When(back.ReturnedOn)}."
                        : $"With {author}. Not submitted yet.",
                    Quote: Detail.Returned?.Note, QuoteBy: Detail.Returned?.ByName,
                    Body: $"It is in nobody's inbox until {author} submits it.");

            case ActivityHolderKind.Person when holder is { IsViewer: true }:
                return new ActivityStatusView(
                    ActivityStatusTone.Yours, badgeClass, badge,
                    holder.Since is { } asked
                        ? $"Your move. {SubjectName} asked you on {ActivityMoments.When(asked)}."
                        : "Your move.",
                    Body: YourMoves(),
                    Meta: meta);

            case ActivityHolderKind.Person:
            case ActivityHolderKind.Waiting:
            {
                var name = holder is { Kind: ActivityHolderKind.Person, Name: { } person } ? person : null;
                var headline = name is null
                    ? $"Waiting for {Activity.CurrentStateLabel}."
                    : holder?.Since is { } since
                        ? $"With {name} since {ActivityMoments.When(since)}."
                        : $"With {name}.";
                var cancel = CancelOnCard ? CancelAction : null;
                string? body = ViewerIsAuthor
                    ? cancel is not null && name is not null
                        ? $"Nothing for you to do. You can cancel the request until {name} acts on it."
                        : cancel is not null
                            ? "Nothing for you to do. You can cancel the request until someone acts on it."
                            : "Nothing for you to do."
                    : null;
                return new ActivityStatusView(
                    ActivityStatusTone.Others, badgeClass, badge, headline, Body: body, Meta: meta,
                    Action: cancel is null ? ActivityStatusAction.None : ActivityStatusAction.Cancel,
                    ActionLabel: cancel is null ? null : CancelLabel);
            }

            case ActivityHolderKind.Closed:
            {
                var move = LastMove;
                var who = MoverName(move);
                var when = move is null ? null : ActivityMoments.When(move.OccurredOn);
                var headline = move is null
                    ? "Closed."
                    : $"Closed. {who} {StateWord} it on {when}.";
                var declined = string.Equals(Activity.CurrentState, "declined", StringComparison.Ordinal);
                if (declined)
                {
                    return new ActivityStatusView(
                        ActivityStatusTone.Declined, badgeClass, badge, headline,
                        Quote: string.IsNullOrWhiteSpace(move?.Note) ? null : move!.Note, QuoteBy: move?.ActorName,
                        Body: ViewerIsSubject
                            ? "It credits nothing, and nothing more can happen to it. To be assessed on this encounter, file it again and name someone else."
                            : "It credits nothing, and nothing more can happen to it.",
                        // E5: File it again, only on a declined request and only for its registrar.
                        Action: ViewerIsSubject ? ActivityStatusAction.FileAgain : ActivityStatusAction.None,
                        ActionLabel: ViewerIsSubject ? "File it again, to someone else" : null,
                        ActionHref: ViewerIsSubject ? $"/activities/new?from={Activity.Id}" : null);
                }

                return new ActivityStatusView(
                    ActivityStatusTone.Others, badgeClass, badge, headline,
                    Quote: string.IsNullOrWhiteSpace(move?.Note) ? null : move!.Note, QuoteBy: move?.ActorName,
                    Body: Filing is null
                        ? "It was never submitted, and it credits nothing."
                        : "It credits nothing, and nothing more can happen to it.");
            }

            default:
                return DoneStatus(badgeClass, badge, ratedLabel);
        }
    }

    private ActivityStatusView DoneStatus(string badgeClass, string badge, string? ratedLabel)
    {
        var move = LastMove;
        string headline;
        if (move is null)
        {
            headline = "Done.";
        }
        else if (IsAuthorsOwn(move))
        {
            // The log's own move: "Done. Logged on …" (the author made it; nobody else acts on it).
            headline = $"Done. {Activity.CurrentStateLabel} on {ActivityMoments.When(move.OccurredOn)}.";
        }
        else
        {
            headline = $"Done. {MoverName(move)} {DonePhrase(move)} on {ActivityMoments.When(move.OccurredOn)}.";
        }

        if (CreditsNothing)
        {
            return new ActivityStatusView(
                ActivityStatusTone.Done, badgeClass, badge, headline,
                Body: move is not null && IsAuthorsOwn(move)
                    ? $"{Capitalise(Article(Noun))} credits nothing, and nobody else acts on it."
                    : $"{Capitalise(Article(Noun))} credits nothing. Nothing more happens to it.");
        }

        var rated = ratedLabel is null ? string.Empty : $"Rated {ratedLabel}. ";
        var code = Detail.EpaCode;
        string credit;
        if (Detail.EpaInForce == false)
        {
            credit = code is null ? "Its credit waits while its EPA is paused." : $"Its credit to {code} waits while the EPA is paused.";
        }
        else if (move?.CreditedItemCount is int count)
        {
            credit = count == 0
                ? "It counted towards no curriculum requirement."
                : $"Credited {CreditOutcome.Label(count)}{(code is null ? string.Empty : $" to {code}")}.";
        }
        else
        {
            credit = string.Empty;
        }

        var body = (rated + credit).Trim();
        return new ActivityStatusView(
            ActivityStatusTone.Done, badgeClass, badge, headline,
            Body: body.Length == 0 ? null : body,
            // The registrar's next look is at their progress (Step 3.6).
            Action: ViewerIsSubject ? ActivityStatusAction.OpenProgress : ActivityStatusAction.None,
            ActionLabel: ViewerIsSubject ? "Open My progress" : null,
            ActionHref: ViewerIsSubject ? "/portfolio/progress" : null);
    }

    /// <summary>The About card's rows (§ Page-level patterns, "Record page with a workflow"; C13).</summary>
    public IReadOnlyList<ActivityAboutRow> About()
    {
        var rows = new List<ActivityAboutRow> { new("Registrar", SubjectName) };

        // The stored encounter date, marked when nobody stated one (T161, D28): the date every list, the trajectory and
        // the PDF place this activity on, so the page says what they say, on a draft too.
        var encounter = new ActivityAboutRow(
            "Encounter", EncounterDate.Label(Activity.ObservedOn, Activity.ObservedOnDeclared), Id: "activity-encounter-date");

        // An author's own draft, never filed: its form below says the rest, so About is who and when (R3 dlAbout).
        if (InInitialState && Filing is null && HolderKind == ActivityHolderKind.Author)
        {
            rows.Add(new ActivityAboutRow("Started", ActivityMoments.Date(Activity.CreatedOn)));
            rows.Add(encounter);
            rows.Add(CreditRow());
            return rows;
        }

        if (Detail.NomineeName is { } nominee)
        {
            rows.Add(new ActivityAboutRow(NomineeLabel ?? "Assessor", nominee));
        }

        if (Detail.EpaCode is { } code)
        {
            rows.Add(new ActivityAboutRow("EPA", string.IsNullOrWhiteSpace(Detail.EpaTitle) ? code : $"{code} — {Detail.EpaTitle}"));
        }

        rows.Add(encounter);

        if (Filing is { } filing)
        {
            var filed = ActivityMoments.Date(filing.OccurredOn);
            rows.Add(new ActivityAboutRow(
                "Filed",
                FilingLateness.Label(filing.DaysAfterEncounter) is null
                    ? filed
                    : $"{filed}, {filing.DaysAfterEncounter} days after the encounter (late)"));
        }

        // C13: the form's version, only where a move's reason names it (T107's stranded move).
        if (Detail.AvailableActions.Any(action =>
                action.UnavailableReason?.Contains("version", StringComparison.OrdinalIgnoreCase) == true))
        {
            rows.Add(new ActivityAboutRow("Form", $"Version {Activity.SchemaVersion}"));
        }

        rows.Add(CreditRow());
        return rows;
    }

    private ActivityAboutRow CreditRow()
    {
        if (CreditsNothing)
        {
            return new ActivityAboutRow("Credit", $"None: {Article(Noun)} credits nothing");
        }

        switch (HolderKind)
        {
            case ActivityHolderKind.Closed:
                return new ActivityAboutRow("Credit", "None");
            case ActivityHolderKind.Done:
            {
                var terminal = LastMove;
                var warnings = new List<string>();
                if (Detail.EpaInForce == false)
                {
                    warnings.Add(PausedWarning);
                }
                else if (terminal?.CreditedItemCount == 0 ||
                         Activity.Transitions.Any(row => row.CreditedItemCount == 0))
                {
                    // T108: a completion that credited nothing. T281: the page cannot know the programme's end (an
                    // assessor may not read the profile), so the words name every cause.
                    warnings.Add(CreditedNothingWarning);
                }

                if (Activity.Transitions.Any(row => row.CreditScaleMismatchCount > 0))
                {
                    warnings.Add(AcrossScalesWarning);
                }

                return new ActivityAboutRow("Credit", CreditOutcome.Label(terminal?.CreditedItemCount), warnings);
            }

            default:
                var finish = Workflow?.States.FirstOrDefault(state => state.Terminal)?.Label;
                return new ActivityAboutRow(
                    "Credit",
                    finish is null ? "None until it is finished" : $"None until it is {finish.Trim().ToLowerInvariant()}");
        }
    }

    /// <summary>T196, D48: an EPA paused after its completion; the credit waits for its reactivation.</summary>
    public const string PausedWarning = "This activity's EPA is paused: its credit waits.";

    /// <summary>T108 and T281, in today's words.</summary>
    public const string CreditedNothingWarning =
        "It counted towards no curriculum requirement. The EPA it was recorded against is most likely not part of this " +
        "trainee's curriculum, or was not in use at the time, or the encounter is dated after the trainee's programme ended. " +
        "The record is kept. If it should have counted, raise it with the programme administrator.";

    /// <summary>T109, in today's words.</summary>
    public const string AcrossScalesWarning =
        "It counted towards the required number of observations, but not towards the supervision level. The rating was " +
        "recorded on a different entrustment scale from the one this trainee's curriculum requirement uses, so the two " +
        "levels are not comparable. Please raise it with the programme administrator: the curriculum item's scale may need " +
        "to be corrected.";

    /// <summary>
    /// The stored rating as a person reads it ("4"), through the rungs of the rated field's ladder; the stored value itself
    /// when the ladder did not load; null when nothing is rated.
    /// </summary>
    public string? RatedLabel(IReadOnlyList<EntrustmentRung>? rungs)
    {
        if (Schema?.RatedLevelField is not { } field || ReadString(Activity.DataJson, field) is not { } stored)
        {
            return null;
        }

        return int.TryParse(stored, NumberStyles.Integer, CultureInfo.InvariantCulture, out var order) &&
               rungs?.FirstOrDefault(rung => rung.Order == order) is { } rung
            ? rung.Label
            : stored;
    }

    /// <summary>The rated field's ladder, by its <c>scale_key</c>; null when the form rates nothing.</summary>
    public string? RatedScaleKey
        => Schema?.RatedLevelField is { } key
            ? Schema.Sections.SelectMany(section => section.Fields)
                .FirstOrDefault(candidate => string.Equals(candidate.Key, key, StringComparison.Ordinal))?.ScaleKey
            : null;

    /// <summary>A field's label, as the form and the refusals name it.</summary>
    public string FieldLabel(string fieldKey) => Schema?.FieldLabel(fieldKey) ?? fieldKey;

    /// <summary>
    /// A field's label, or null for a key the form does not hold: what <see cref="FieldRefusals.Split" /> cuts a refusal by,
    /// so a key the schema lacks gets no line of its own key.
    /// </summary>
    public string? FieldLabelOrNull(string fieldKey)
        => Schema is not null &&
           Schema.Sections.Any(section => section.Fields.Any(field => string.Equals(field.Key, fieldKey, StringComparison.Ordinal)))
            ? Schema.FieldLabel(fieldKey)
            : null;

    /// <summary>The <c>user</c> field that names who acts in the current state (the assessor's), or null.</summary>
    public string? NomineeFieldKey => ActivityHolders.NomineeField(Workflow, Activity.CurrentState);

    private static readonly ActorRule AuthorRule = ActorRuleParser.Parse("subject|creator");

    /// <summary>
    /// Each section's "Filled in by Sipho Ndlovu, 2026-09-29" (T342, Spec § 2; ActivityForm's <c>SectionAttributions</c>),
    /// by section key: the last person who moved the activity out of a state they could fill the section in, with the
    /// date of that move on the South African calendar.
    /// </summary>
    /// <remarks>
    /// A section is written by whom its <c>editable_by</c> names, within whom the state's <c>editable_by</c> names (both
    /// <c>subject|creator</c> when unset, as <c>FieldPermissionEvaluator</c> reads them). The page judges the rules it can
    /// from what it holds: <c>subject</c> and <c>creator</c> by id, <c>field:</c> by the stored value. A <c>role:</c> or
    /// <c>scope:</c> term needs the mover's claims, which the page does not have, so it matches anyone. Leaving the state is
    /// what finished the section: a draft's submit, an assessor's complete; or the create, where the create is the filing. A cancel is not filling anything in, so a
    /// cancelled draft names its author without a date (R3-C-Activity). A section nobody moved on from carries nothing;
    /// the form shows the attribution only on a filled section anyway.
    /// </remarks>
    public IReadOnlyDictionary<string, string> SectionAttributions()
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (Schema is null || Workflow is null)
        {
            return result;
        }

        var data = StoredData();
        var moves = Ordered(Activity.Transitions).Where(row => !IsCreate(row.TransitionKey)).ToList();

        // The create is the filing when no move out of the initial state that leads on is its author's (a type born in
        // requested, or born terminal; T148): then it, not a later move, is what filled the author's sections in.
        var createFiles = CreateFiles(out var create);

        foreach (var section in Schema.Sections)
        {
            var sectionRule = section.EditableBy ?? AuthorRule;
            bool FilledBy(ActivityTransitionDto row)
            {
                var stateRule = Workflow.States
                    .FirstOrDefault(state => string.Equals(state.Key, row.FromState, StringComparison.Ordinal))?.EditableBy ?? AuthorRule;
                return Matches(sectionRule, row.ActorUserId, data) && Matches(stateRule, row.ActorUserId, data);
            }

            var filling = moves.LastOrDefault(row => !IsCancelKey(row.TransitionKey) && FilledBy(row));
            if (filling is null && createFiles && Matches(sectionRule, create!.ActorUserId, data))
            {
                filling = create;
            }

            if (filling is not null)
            {
                result[section.Key] = $"Filled in by {filling.ActorName ?? "someone"}, {ActivityMoments.Date(filling.OccurredOn)}";
            }
            else if (moves.LastOrDefault(row => IsCancelKey(row.TransitionKey) && FilledBy(row)) is { ActorName: { } name })
            {
                result[section.Key] = $"Filled in by {name}";
            }
        }

        return result;
    }

    private static bool IsCancelKey(string transitionKey) => transitionKey is "cancel" or "withdraw";

    private static bool IsCreate(string transitionKey)
        => string.Equals(transitionKey, Wombat.Domain.Activities.Workflow.Workflow.CreateTransitionKey, StringComparison.Ordinal);

    private bool Matches(ActorRule rule, string actorUserId, IReadOnlyDictionary<string, string> data) => rule switch
    {
        SubjectUserActorRule => string.Equals(actorUserId, Activity.SubjectUserId, StringComparison.Ordinal),
        CreatorUserActorRule => string.Equals(actorUserId, Activity.CreatedByUserId, StringComparison.Ordinal),
        FieldUserActorRule field => data.TryGetValue(field.Field, out var named) && string.Equals(named, actorUserId, StringComparison.Ordinal),
        CombinedActorRule { CombinationKind: ActorRuleCombinationKind.All } all => all.Rules.All(inner => Matches(inner, actorUserId, data)),
        CombinedActorRule any => any.Rules.Any(inner => Matches(inner, actorUserId, data)),
        _ => true
    };

    // The stored data's string values by key, for the field: rules.
    private IReadOnlyDictionary<string, string> StoredData()
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(Activity.DataJson) ? "{}" : Activity.DataJson);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.String && property.Value.GetString() is { } value)
                    {
                        values[property.Name] = value;
                    }
                }
            }
        }
        catch (JsonException)
        {
        }

        return values;
    }

    /// <summary>The name of whoever made <paramref name="move" />: "You" to the viewer.</summary>
    private string MoverName(ActivityTransitionDto? move)
        => move is null
            ? "Someone"
            : ViewerId is not null && string.Equals(move.ActorUserId, ViewerId, StringComparison.Ordinal)
                ? "You"
                : move.ActorName ?? "Someone";

    private bool IsAuthorsOwn(ActivityTransitionDto move)
        => string.Equals(move.ActorUserId, Activity.SubjectUserId, StringComparison.Ordinal) ||
           string.Equals(move.ActorUserId, Activity.CreatedByUserId, StringComparison.Ordinal);

    // "completed it", "recorded the discussion", "signed it off": the move in words, from the state it ended in.
    private string DonePhrase(ActivityTransitionDto move) => move.TransitionKey switch
    {
        "record_discussion" => "recorded the discussion",
        "sign_off" => "signed it off",
        _ => $"{move.ToStateLabel.Trim().ToLowerInvariant()} it"
    };

    // The viewer's own moves in a sentence: "Complete it, or decline it with a note Sipho Ndlovu will read." Each move is a
    // verb phrase by its key (MovePhrase), never its label with " it" after it, which read "Record discussion it".
    private string? YourMoves()
    {
        var moves = Detail.AvailableActions
            .Where(action => action.IsAvailable && !IsCancel(action))
            .Select(action => MovePhrase(action.TransitionKey) +
                              (action.RequiresNote ? $" with a note {SubjectName} will read" : string.Empty))
            .ToList();
        return moves.Count == 0 ? null : Capitalise(string.Join(", or ", moves)) + ".";
    }

    /// <summary>
    /// A move as a verb phrase, by its key: "complete it", "record the discussion", "sign it off". Every seeded move is
    /// named; a builder's own move is "act on it", since its label need not be a verb (the runbook lane's finding, T342).
    /// </summary>
    public static string MovePhrase(string transitionKey) => transitionKey switch
    {
        "complete" => "complete it",
        "decline" => "decline it",
        "record_discussion" => "record the discussion",
        "return" => "return it",
        "sign_off" => "sign it off",
        "verify" => "verify it",
        "approve" => "approve it",
        "accept" => "accept it",
        "reject" => "reject it",
        "review" => "review it",
        "record" => "record it",
        "submit" => "submit it",
        "revise" => "revise it",
        _ => "act on it"
    };

    private ActivityHolderKind FallbackHolderKind()
    {
        if (Workflow is null)
        {
            return ActivityHolderKind.Waiting;
        }

        if (Workflow.States.FirstOrDefault(state => string.Equals(state.Key, Activity.CurrentState, StringComparison.Ordinal))?.Terminal == true)
        {
            return ActivityHolderKind.Done;
        }

        if (!Workflow.HasOutgoingTransition(Activity.CurrentState))
        {
            return ActivityHolderKind.Closed;
        }

        return InInitialState ? ActivityHolderKind.Author : ActivityHolderKind.Waiting;
    }

    public static ActivityPageModel From(ActivityDetailDto detail, string? viewerId)
    {
        ArgumentNullException.ThrowIfNull(detail);
        return new ActivityPageModel(detail, viewerId);
    }

    /// <summary>
    /// A result as the result region shows it (Spec § 1, "Result"; the build review's D7): the move's own sentence, bold,
    /// and whose inbox it is in, plain. "Submitted. It is now Requested. It is in Fatima Khumalo's Activity inbox." is
    /// ("Submitted. It is now Requested.", "It is in Fatima Khumalo's Activity inbox."); a message with no inbox sentence
    /// is all title.
    /// </summary>
    public static (string Title, string? Inbox) SplitResult(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var at = message.IndexOf(" It is in ", StringComparison.Ordinal);
        return at > 0 ? (message[..at], message[(at + 1)..]) : (message, null);
    }

    /// <summary>The history oldest first, as the page's table reads it (A7).</summary>
    public static IReadOnlyList<ActivityTransitionDto> Ordered(IEnumerable<ActivityTransitionDto> transitions)
        => transitions.OrderBy(row => row.OccurredOn).ThenBy(row => row.Id).ToList();

    /// <summary>
    /// "reflective exercise" from "Reflective Exercise (Paediatrics)": the bracketed discipline dropped, each capitalised
    /// word lower-cased, and an acronym ("KGK", "DOPS") kept as it is.
    /// </summary>
    public static string NounOf(string typeName)
    {
        var name = typeName ?? string.Empty;
        var bracket = name.IndexOf(" (", StringComparison.Ordinal);
        if (bracket > 0)
        {
            name = name[..bracket];
        }

        return LowerFirstWords(name.Trim());
    }

    /// <summary>"a reflective exercise", "an audit", "a KGK teaching session log".</summary>
    public static string Article(string noun)
        => noun.Length > 0 && "aeiou".Contains(noun[0]) ? $"an {noun}" : $"a {noun}";

    public static string Possessive(string name) => $"{name}'s";

    private static string LowerFirstWords(string text)
        => string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(word =>
            word.Length > 1 && char.IsUpper(word[0]) && word.Skip(1).All(character => !char.IsUpper(character))
                ? char.ToLowerInvariant(word[0]) + word[1..]
                : word));

    private static string Capitalise(string text)
        => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    private static FormSchema? TryParseSchema(string? schemaJson)
    {
        if (string.IsNullOrWhiteSpace(schemaJson))
        {
            return null;
        }

        try
        {
            return FormSchemaParser.Parse(schemaJson);
        }
        catch (Exception exception) when (exception is SchemaParseException or JsonException or ArgumentException)
        {
            return null;
        }
    }

    private static string? ReadString(string? dataJson, string key)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty(key, out var value))
            {
                return null;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => string.IsNullOrWhiteSpace(value.GetString()) ? null : value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                _ => null
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>The status card's stripe: whose move it is (R3-Spec § 4, ActivityStatus).</summary>
public enum ActivityStatusTone
{
    /// <summary>The viewer's move (<c>.activity-status--yours</c>, the emphasis stripe).</summary>
    Yours,

    /// <summary>Someone else's move, or closed by a cancel.</summary>
    Others,

    /// <summary>Finished.</summary>
    Done,

    /// <summary>Declined.</summary>
    Declined
}

/// <summary>The card's one action.</summary>
public enum ActivityStatusAction
{
    None,

    /// <summary>"File it again, to someone else" (E1, E5): a link to Log an activity with <c>?from=</c>.</summary>
    FileAgain,

    /// <summary>"Open My progress", on a completed activity, for its registrar.</summary>
    OpenProgress,

    /// <summary>"Cancel request…", quiet, behind the ConfirmDialog (C11).</summary>
    Cancel
}

/// <summary>What the status card says. Every headline ends with a full stop (C11).</summary>
public sealed record ActivityStatusView(
    ActivityStatusTone Tone,
    string BadgeClass,
    string Badge,
    string Headline,
    string? Quote = null,
    string? QuoteBy = null,
    string? Body = null,
    string? Meta = null,
    ActivityStatusAction Action = ActivityStatusAction.None,
    string? ActionLabel = null,
    string? ActionHref = null)
{
    public string ToneClass => Tone switch
    {
        // The viewer's move reuses the emphasis card's stripe (T6); the frame is every card's hairline (T8).
        ActivityStatusTone.Yours => "detail-card activity-status detail-card--emphasis",
        ActivityStatusTone.Done => "detail-card activity-status activity-status--done",
        ActivityStatusTone.Declined => "detail-card activity-status activity-status--declined",
        _ => "detail-card activity-status activity-status--others"
    };
}

/// <summary>A row of the About card, with the credit warnings that sit under its value (C13).</summary>
public sealed record ActivityAboutRow(string Label, string Value, IReadOnlyList<string>? Warnings = null, string? Id = null);

/// <summary>Moments as the activity pages state them: South African time, with its zone (A7, B13).</summary>
public static class ActivityMoments
{
    /// <summary>"2026-09-29 09:12 SAST", from a stored UTC moment.</summary>
    public static string When(DateTime utc)
        => ErrorPages.TimeOf(new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)));

    /// <summary>The South African calendar date of a stored UTC moment, "2026-09-29".</summary>
    public static string Date(DateTime utc)
        => ProgrammeCalendar.DateOf(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
