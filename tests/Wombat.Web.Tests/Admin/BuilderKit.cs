using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft;
using Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The builder's test kit (T398, flow 17, lane C): editors built by hand, as the FakeSender pattern of
/// <c>ActivityTypeBuilderAccessTests</c> builds them, and a sender that answers the editor read and the three commands.
/// The read in words (<see cref="ActivityTypeEditorDto.WorkflowRead" />) is lane A's to compute and lane B's to draw; here
/// it is built by hand and the tests ask only that it is placed.
/// </summary>
internal static class BuilderKit
{
    public const int KgkId = 23;
    public const int KgkInstitution = 7;

    /// <summary>KGK Teaching Session Log's form as the runbook builds it (1.27), Audience's key typed right.</summary>
    public const string KgkSchema = """
        {
          "version": 1,
          "observation_date_field": "delivered_on",
          "evidence_epa_field": "epa_id",
          "sections": [
            {
              "key": "session",
              "title": "Teaching session",
              "fields": [
                { "key": "topic", "type": "text", "label": "Topic", "required": true },
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "delivered_on", "type": "date", "label": "Date delivered", "required": true },
                { "key": "audience", "type": "choice", "label": "Audience", "required": true, "options": ["Interns", "Registrars"] },
                { "key": "objectives", "type": "longtext", "label": "Learning objectives", "required": true },
                { "key": "supervisor_user_id", "type": "user", "label": "Supervising consultant" }
              ]
            }
          ]
        }
        """;

    public const string Workflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [ { "key": "draft", "label": "Draft" }, { "key": "logged", "label": "Logged", "terminal": true } ],
          "transitions": [ { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator", "validation": "all" } ]
        }
        """;

    public const string Credit = """{ "counts_for": [] }""";

    public static readonly DateTime SavedOn = new(2026, 11, 5, 9, 0, 0, DateTimeKind.Utc);

    public static readonly IReadOnlyList<ActivityTypeScopeChoiceDto> InstitutionScopes =
        [new(ActivityScope.Institution, [new ActivityTypeScopeTargetDto(KgkInstitution, "Kgosi Kgari Teaching Hospital")])];

    public static readonly ActivityTypeWorkflowReadDto WorkflowRead = new(
        [new ActivityTypeStateReadDto("draft", "Draft", true, false), new ActivityTypeStateReadDto("logged", "Logged", false, true)],
        [new ActivityTypeMoveReadDto("log", "Log", ["Draft"], "Logged", "The registrar, or whoever filed it", "The whole form", false)]);

    public static readonly ActivityTypeCreditReadDto CreditRead = new(true, []);

    /// <summary>KGK's type to its writer, Prof Mbatha: by default a draft saved, never published.</summary>
    public static ActivityTypeEditorDto Kgk(
        bool hasDraft = true,
        int publishedVersion = 0,
        bool isActive = true,
        string? draftSchema = null,
        string? publishedSchema = null,
        bool canWrite = true) => new ActivityTypeEditorDto(
            KgkId, "kgk_teaching_log", "KGK Teaching Session Log", "A registrar's log.", ActivityScope.Institution, KgkInstitution,
            isActive, null, publishedVersion, hasDraft,
            draftSchema ?? KgkSchema, Workflow, Credit, "[]",
            publishedVersion > 0 ? publishedSchema ?? KgkSchema : null,
            publishedVersion > 0 ? Workflow : null,
            publishedVersion > 0 ? Credit : null,
            "[]",
            "prof-mbatha", hasDraft ? "prof-mbatha" : null, hasDraft ? SavedOn : null,
            publishedVersion > 0 ? [new ActivityTypeVersionDto(1, SavedOn, "prof-mbatha") { PublishedByName = "Nolwazi Mbatha" }] : [],
            canWrite,
            canWrite ? InstitutionScopes : [],
            "Kgosi Kgari Teaching Hospital")
        {
            Owner = new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Institution, "Kgosi Kgari Teaching Hospital"),
            DraftSavedByName = hasDraft ? "Nolwazi Mbatha" : null,
            WorkflowRead = WorkflowRead,
            CreditRead = CreditRead
        };

    /// <summary>A new type on /new, as the editor read gives it, for a caller with these scopes.</summary>
    public static ActivityTypeEditorDto New(IReadOnlyList<ActivityTypeScopeChoiceDto> scopes, ActivityTypeOwnerDto? owner = null)
    {
        var first = scopes.FirstOrDefault();
        var target = first?.Targets.FirstOrDefault();
        return new ActivityTypeEditorDto(
            0, string.Empty, string.Empty, null, first?.Scope ?? ActivityScope.Global, target?.Id, true, null, 0, false,
            KgkSchema, Workflow, Credit, "[]", null, null, null, "[]", string.Empty, null, null, [],
            scopes.Count > 0, scopes, target?.Name)
        {
            Owner = owner ?? new ActivityTypeOwnerDto(ActivityTypeOwnerKind.Institution, target?.Name),
            WorkflowRead = WorkflowRead,
            CreditRead = CreditRead
        };
    }

    public static void SignIn(TestContext context, string role)
    {
        var auth = context.AddTestAuthorization();
        auth.SetAuthorized("caller@test");
        auth.SetRoles(role);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "prof-mbatha"));
        context.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public static IRenderedComponent<ActivityTypeEdit> Render(TestContext context, BuilderSender sender, int? id = KgkId, string? arriveAt = null)
    {
        context.Services.AddSingleton<IScopedSender>(sender);
        if (!context.Services.Any(service => service.ServiceType == typeof(IActivityReferenceDataService)))
        {
            context.Services.AddSingleton<IActivityReferenceDataService>(new StubActivityReferenceDataService());
        }

        if (arriveAt is not null)
        {
            context.Services.GetRequiredService<NavigationManager>().NavigateTo(arriveAt);
        }

        var cut = context.RenderComponent<ActivityTypeEdit>(parameters => parameters.Add(page => page.ActivityTypeId, id));
        if (sender.Loads)
        {
            cut.WaitForState(() => cut.FindAll(".tab-bar, #activity-type-no-scope, .state-panel-copy").Count > 0);
        }

        return cut;
    }

    public static IElement TabNamed(IRenderedFragment cut, string name)
        => cut.FindAll("[role=tab]").Single(tab => tab.TextContent.Trim().StartsWith(name, StringComparison.Ordinal));

    public static void Tab(IRenderedFragment cut, string name) => TabNamed(cut, name).Click();

    public static IElement Button(IRenderedFragment cut, string words)
        => cut.FindAll("button").First(button => button.TextContent.Trim() == words);

    public static IElement Named(IRenderedFragment cut, string ariaLabel)
        => cut.FindAll("button").Single(button => button.GetAttribute("aria-label") == ariaLabel);

    /// <summary>The focus calls by id the page has made (PageFocus.FocusByIdAsync).</summary>
    public static IReadOnlyList<string> FocusedIds(TestContext context)
        => context.JSInterop.Invocations
            .Where(invocation => invocation.Identifier == "wombat.focusById")
            .Select(invocation => invocation.Arguments[0]?.ToString() ?? string.Empty)
            .ToList();
}

/// <summary>Answers the builder's read and its three commands, and records each command.</summary>
internal sealed class BuilderSender : IScopedSender
{
    private readonly Task<ActivityTypeEditorDto?> _editor;

    public BuilderSender(ActivityTypeEditorDto? editor) : this(Task.FromResult(editor))
    {
    }

    public BuilderSender(Task<ActivityTypeEditorDto?> editor)
    {
        _editor = editor;
    }

    public bool Loads => _editor.IsCompleted;

    public List<SaveActivityTypeDraftCommand> Saves { get; } = [];

    public List<PublishActivityTypeDraftCommand> Publishes { get; } = [];

    public List<DiscardActivityTypeDraftCommand> Discards { get; } = [];

    /// <summary>What a save returns; by default the editor with the command's draft, saved.</summary>
    public Func<SaveActivityTypeDraftCommand, ActivityTypeEditorDto>? OnSave { get; set; }

    public Func<PublishActivityTypeDraftCommand, ActivityTypeEditorDto>? OnPublish { get; set; }

    public Func<DiscardActivityTypeDraftCommand, ActivityTypeEditorDto>? OnDiscard { get; set; }

    public Exception? SaveRefusal { get; set; }

    public Exception? PublishRefusal { get; set; }

    public Exception? DiscardRefusal { get; set; }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        object? response = request switch
        {
            GetActivityTypeEditorQuery => await _editor,
            GetWbaToolsQuery => (IReadOnlyList<WbaToolDto>)[new WbaToolDto("mini_cex", "Mini-CEX", null)],
            SaveActivityTypeDraftCommand save => Save(save),
            PublishActivityTypeDraftCommand publish => Publish(publish),
            DiscardActivityTypeDraftCommand discard => Discard(discard),
            _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
        };

        return (TResponse)response!;
    }

    public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    /// <summary>The type as the server holds it now: the read's, then each save's and publish's.</summary>
    private ActivityTypeEditorDto? _current;

    private ActivityTypeEditorDto Current => _current ?? _editor.Result!;

    private ActivityTypeEditorDto Save(SaveActivityTypeDraftCommand command)
    {
        Saves.Add(command);
        if (SaveRefusal is not null)
        {
            throw SaveRefusal;
        }

        _current = OnSave?.Invoke(command) ?? Current with
        {
            Id = command.ActivityTypeId ?? 42,
            Key = command.Key,
            Name = command.Name,
            Description = command.Description,
            IsActive = command.IsActive,
            HasDraft = true,
            DraftSchemaJson = command.DraftSchemaJson,
            DraftWorkflowJson = command.DraftWorkflowJson,
            DraftCreditRulesJson = command.DraftCreditRulesJson,
            StagingUpdatedOn = BuilderKit.SavedOn,
            DraftSavedByName = "Nolwazi Mbatha"
        };
        return _current;
    }

    private ActivityTypeEditorDto Publish(PublishActivityTypeDraftCommand command)
    {
        Publishes.Add(command);
        if (PublishRefusal is not null)
        {
            throw PublishRefusal;
        }

        var before = Current;
        _current = OnPublish?.Invoke(command) ?? before with
        {
            PublishedVersion = before.PublishedVersion + 1,
            HasDraft = false,
            PublishedSchemaJson = before.DraftSchemaJson,
            Versions = [.. before.Versions, new ActivityTypeVersionDto(before.PublishedVersion + 1, BuilderKit.SavedOn, "prof-mbatha") { PublishedByName = "Nolwazi Mbatha" }]
        };
        return _current;
    }

    private ActivityTypeEditorDto Discard(DiscardActivityTypeDraftCommand command)
    {
        Discards.Add(command);
        if (DiscardRefusal is not null)
        {
            throw DiscardRefusal;
        }

        return OnDiscard?.Invoke(command) ?? Current with { HasDraft = false, StagingUpdatedOn = null, DraftSavedByName = null };
    }
}
