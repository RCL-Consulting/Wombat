using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T122: the builder's "This tool is" picker — the only surface that says which instrument an activity type is.
/// </summary>
/// <remarks>
/// <para>
/// <c>SaveActivityTypeDraftCommand</c> writes every metadata column on every save, so the page must send the
/// stored key back even when the admin never opened the Metadata tab. A save that sent null would withdraw the
/// type's instrument, and under D21 null is unrestricted: every curriculum item's tool list would stop applying
/// to that form, and nothing would say so.
/// </para>
/// <para>
/// A stored key the vocabulary no longer holds must still be listed. A select whose value has no matching option
/// shows its first option, so an orphan would read as "Not a WBA instrument" while the row still said otherwise.
/// </para>
/// </remarks>
public sealed class ActivityTypeToolPickerTests : TestContext
{
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            { "key": "s", "title": "Details", "fields": [ { "key": "epa_id", "type": "epa", "label": "EPA" } ] }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [ { "key": "draft", "label": "Draft" }, { "key": "done", "label": "Done", "terminal": true } ],
          "transitions": [ { "key": "submit", "from": "draft", "to": "done", "actor": "subject" } ]
        }
        """;

    // Ordered by Name, as GetWbaToolsQuery returns it.
    private static readonly IReadOnlyList<WbaToolDto> Vocabulary =
    [
        new("cbd", "Case-based discussion", null),
        new("dops", "DOPS", null),
        new("mini_cex", "Mini-CEX", null),
        new("msf", "Multi-source feedback", null)
    ];

    public ActivityTypeToolPickerTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    [Fact]
    public void TheMetadataTab_OffersNotAWbaInstrumentFirst_ThenTheVocabulary()
    {
        var cut = RenderMetadata(new FakeSender(storedToolKey: null));

        var options = Options(cut);

        // "Not a WBA instrument" is the empty value, so choosing it saves null (D21), and it comes first so a
        // new type defaults to it.
        options.First().Should().Be((string.Empty, "Not a WBA instrument"));
        options.Skip(1).Should().Equal(Vocabulary.Select(tool => (tool.Key, tool.Name)),
            "one option per instrument, value = key, label = the College's name, and nothing else");
    }

    [Fact]
    public void ATypeWithNoInstrument_ShowsNotAWbaInstrument()
    {
        var cut = RenderMetadata(new FakeSender(storedToolKey: null));

        cut.Find("#type-wba-tool").GetAttribute("value").Should().BeEmpty();
    }

    [Fact]
    public void TheStoredKey_IsSelected_AndNothingExtraIsListed()
    {
        var cut = RenderMetadata(new FakeSender(storedToolKey: "dops"));

        cut.Find("#type-wba-tool").GetAttribute("value").Should().Be("dops");
        Options(cut).Should().HaveCount(1 + Vocabulary.Count, "a key the vocabulary holds is not an orphan");
    }

    [Fact]
    public void AnOrphanStoredKey_IsListedAsAnExtraOption_AndSelected()
    {
        // So it can be seen, and withdrawn. Without the extra option the select would show "Not a WBA
        // instrument" for a type the write path still treats as legacy_tool.
        var cut = RenderMetadata(new FakeSender(storedToolKey: "legacy_tool"));

        cut.Find("#type-wba-tool").GetAttribute("value").Should().Be("legacy_tool");

        var options = Options(cut);
        options.Should().HaveCount(2 + Vocabulary.Count);
        options.Should().ContainSingle(option => option.Value == "legacy_tool")
            .Which.Label.Should().Contain("legacy_tool").And.Contain("not a known instrument");
    }

    [Fact]
    public void ChoosingAnInstrument_SendsItsKeyOnSave()
    {
        var sender = new FakeSender(storedToolKey: null);
        var cut = RenderMetadata(sender);

        cut.Find("#type-wba-tool").Change("mini_cex");
        SaveDraft(cut);

        sender.Saves.Should().ContainSingle()
            .Which.WbaToolKey.Should().Be("mini_cex");
    }

    [Fact]
    public void ChoosingNotAWbaInstrument_SendsNullOnSave()
    {
        // Null, not "": the command normalises and validates against the vocabulary, and an empty string is
        // not a key. This is also how an orphan is withdrawn.
        var sender = new FakeSender(storedToolKey: "legacy_tool");
        var cut = RenderMetadata(sender);

        cut.Find("#type-wba-tool").Change(string.Empty);
        SaveDraft(cut);

        sender.Saves.Should().ContainSingle()
            .Which.WbaToolKey.Should().BeNull();
    }

    [Fact]
    public void ASaveFromTheFormTab_SendsTheStoredKey_WithoutTheMetadataTabEverOpening()
    {
        // The everyday edit: an admin adds a field and saves. They never see the picker, and the page must still
        // send the stored instrument back, or that save silently makes the type unrestricted.
        var sender = new FakeSender(storedToolKey: "dops");
        var cut = RenderPage(sender);

        cut.FindAll("#type-wba-tool").Should().BeEmpty("the default tab is Form, not Metadata");

        SaveDraft(cut);

        sender.Saves.Should().ContainSingle()
            .Which.WbaToolKey.Should().Be("dops");
    }

    [Fact]
    public void AnUnchangedOrphan_IsSentBackAsStored()
    {
        // The page does not quietly withdraw a key it cannot name. The server decides what to do with it, and
        // says so; an editor that dropped it would change the type's restriction without being asked.
        var sender = new FakeSender(storedToolKey: "legacy_tool");
        var cut = RenderMetadata(sender);

        SaveDraft(cut);

        sender.Saves.Should().ContainSingle()
            .Which.WbaToolKey.Should().Be("legacy_tool");
    }

    // ---- helpers ----

    private IRenderedComponent<ActivityTypeEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ActivityTypeEdit>(parameters => parameters.Add(page => page.ActivityTypeId, 4));
        cut.WaitForState(() => cut.FindAll(".tab-bar-tab").Count > 0);

        return cut;
    }

    private IRenderedComponent<ActivityTypeEdit> RenderMetadata(FakeSender sender)
    {
        var cut = RenderPage(sender);

        cut.FindAll(".tab-bar-tab")
            .First(tab => tab.TextContent.Trim() == "Metadata")
            .Click();
        cut.WaitForState(() => cut.FindAll("#type-wba-tool").Count == 1);

        return cut;
    }

    private static IReadOnlyList<(string Value, string Label)> Options(IRenderedComponent<ActivityTypeEdit> cut)
        => cut.FindAll("#type-wba-tool option")
            .Select(option => (option.GetAttribute("value") ?? string.Empty, option.TextContent.Trim()))
            .ToList();

    private static void SaveDraft(IRenderedComponent<ActivityTypeEdit> cut)
        => cut.FindAll("button")
            .First(button => button.TextContent.Trim() == "Save draft")
            .Click();

    private sealed class FakeSender : IScopedSender
    {
        private readonly ActivityTypeEditorDto _editor;

        public FakeSender(string? storedToolKey)
        {
            _editor = new ActivityTypeEditorDto(
                4,
                "dops_cpsa",
                "DOPS (CPSA)",
                null,
                ActivityScope.Global,
                null,
                true,
                storedToolKey,
                1,
                false,
                SchemaJson,
                WorkflowJson,
                "{}",
                "[]",
                SchemaJson,
                WorkflowJson,
                "{}",
                "[]",
                "admin-1",
                null,
                null,
                [],
                true,
                [new ActivityTypeScopeChoiceDto(ActivityScope.Global, [])],
                null);
        }

        public List<SaveActivityTypeDraftCommand> Saves { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                GetActivityTypeEditorQuery => _editor,
                GetInstitutionsListQuery => (IReadOnlyList<InstitutionDto>)[],
                GetSpecialitiesListQuery => (IReadOnlyList<SpecialityDto>)[],
                GetSubSpecialitiesListQuery => (IReadOnlyList<SubSpecialityDto>)[],
                GetWbaToolsQuery => Vocabulary,
                SaveActivityTypeDraftCommand save => Save(save),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private ActivityTypeEditorDto Save(SaveActivityTypeDraftCommand command)
        {
            Saves.Add(command);
            return _editor with { WbaToolKey = command.WbaToolKey, HasDraft = true };
        }
    }
}
