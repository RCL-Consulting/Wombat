using Bunit;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Wombat.Application.Common;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, Q6, J1–J3, S1–S4, S10, P1): a refused save is a summary at the top of the result region, one line a
/// place, each prefixed by the page from the place and each a link that switches the tab and focuses the place; the words
/// again at the place; the tab's count. A failure is the fixed sentence; never an exception's text.
/// </summary>
public sealed class ActivityTypeRefusalTests : TestContext
{
    private const string DuplicateSentence =
        "Field key 'topic' is used by 2 fields ('Topic' in 'Teaching session', 'Audience' in 'Teaching session'); give each field its own key.";

    public ActivityTypeRefusalTests() => BuilderKit.SignIn(this, WombatRoles.InstitutionalAdmin);

    private static string WithAudienceKeyedTopic => BuilderKit.KgkSchema.Replace("\"key\": \"audience\"", "\"key\": \"topic\"");

    [Fact]
    public void ADuplicateKey_IsOneSummaryLine_AfterForm_AndItsWordsAtBothFields()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(draftSchema: WithAudienceKeyedTopic))
        {
            SaveRefusal = new ActivityTypeDraftRefusedException(
                [new RefusalPlace(RefusalTab.Form, RefusalKind.DuplicateFieldKey, DuplicateSentence) { FieldKeys = ["topic", "topic"], SectionKey = "session" }])
        };
        var cut = BuilderKit.Render(this, sender);

        BuilderKit.Button(cut, "Save draft").Click();

        var summary = cut.Find("#refusal-summary");
        summary.GetAttribute("role").Should().Be("alert");
        summary.QuerySelector("strong")!.TextContent.Should().Be("Nothing was saved.");
        summary.TextContent.Should().Contain("Everything you typed is kept.");
        summary.QuerySelectorAll("li").Select(li => li.TextContent.Trim()).Should().Equal($"Form: {DuplicateSentence}");
        cut.Find("#tab-form").TextContent.Trim().Should().Be("Form 1 problem");

        var refused = cut.FindAll("li.field-row.is-refused");
        refused.Select(row => row.QuerySelector(".validation-message")!.TextContent.Trim()).Should().Equal(
            "Key 'topic' is also the key of Audience. Give each field its own key.",
            "Key 'topic' is also the key of Topic. Give each field its own key.");
    }

    [Fact]
    public void ALinesPress_SwitchesTheTab_OpensTheRowsEditor_AndFocusesItsKey()
    {
        var sender = new BuilderSender(BuilderKit.Kgk(draftSchema: WithAudienceKeyedTopic))
        {
            SaveRefusal = new ActivityTypeDraftRefusedException(
                [new RefusalPlace(RefusalTab.Form, RefusalKind.DuplicateFieldKey, DuplicateSentence) { FieldKeys = ["topic", "topic"] }])
        };
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Details");
        BuilderKit.Button(cut, "Save draft").Click();
        BuilderKit.TabNamed(cut, "Details").GetAttribute("aria-selected").Should().Be("true", "a refusal keeps the tab (R3-C15-before)");

        cut.Find("#refusal-summary a").Click();

        BuilderKit.TabNamed(cut, "Form").GetAttribute("aria-selected").Should().Be("true");
        cut.Find("li.field-editor-inplace h4").TextContent.Should().Be("Field Audience", "the last holder: the field just keyed (R3-C15-after)");
        var key = cut.Find("#field-key");
        key.GetAttribute("aria-invalid").Should().Be("true");
        key.GetAttribute("aria-describedby").Should().StartWith("field-key-refusal");
        cut.Find("#field-key-refusal").TextContent.Should().Be("Key 'topic' is also the key of Topic. Give each field its own key.");
        BuilderKit.FocusedIds(this).Should().EndWith("field-key");
    }

    [Fact]
    public void AnEmptySection_IsRefusedAtItsHead_AfterFormSection()
    {
        var sender = new BuilderSender(BuilderKit.Kgk())
        {
            SaveRefusal = new ActivityTypeDraftRefusedException(
                [new RefusalPlace(RefusalTab.Form, RefusalKind.EmptySection, "Section 'session' must contain at least one field.") { SectionKey = "session" }])
        };
        var cut = BuilderKit.Render(this, sender);

        BuilderKit.Button(cut, "Save draft").Click();

        cut.Find("#refusal-summary li").TextContent.Trim()
            .Should().Be("Form, section Teaching session: Section 'session' must contain at least one field.");
        var head = cut.Find(".outline-section-head.is-refused");
        head.QuerySelector(".validation-message")!.TextContent.Should().Be("This section has no fields. Add a field, or delete the section.");

        // The fix pass (S2): the summary's line opens the section's editor at its key, which is marked and described
        // by the head's words, as a field's key is.
        cut.Find("#refusal-summary a").Click();
        var key = cut.Find("#section-key");
        key.GetAttribute("aria-invalid").Should().Be("true");
        var describedBy = key.GetAttribute("aria-describedby")!;
        cut.Find($"#{describedBy}").TextContent.Should().Be("This section has no fields. Add a field, or delete the section.");
    }

    [Fact]
    public void AFieldRuleOnAMove_IsRefusedUnderTheWorkflowJson_AfterWorkflowMove()
    {
        const string sentence = "Transition 'cancel' actor names 'field:topic', but 'topic' is not a User field; a field: rule must name a User field.";
        var sender = new BuilderSender(BuilderKit.Kgk())
        {
            SaveRefusal = new ActivityTypeDraftRefusedException(
                [new RefusalPlace(RefusalTab.Workflow, RefusalKind.FieldRuleNotUserField, sentence) { MoveKey = "cancel", NamedField = "topic" }])
        };
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Workflow");

        BuilderKit.Button(cut, "Save draft").Click();

        cut.Find("#refusal-summary li").TextContent.Trim().Should().Be($"Workflow, move Cancel: {sentence}");
        cut.Find("#tab-workflow").TextContent.Trim().Should().Be("Workflow 1 problem");
        var json = cut.Find("#workflow-json");
        json.GetAttribute("aria-invalid").Should().Be("true");
        json.GetAttribute("aria-describedby").Should().Be("workflow-json-refusal workflow-json-help");
        cut.Find("#workflow-json-refusal").TextContent.Should().Be("Move Cancel cannot be read: 'topic' (Topic) is not a User field, so it names nobody.");
    }

    [Fact]
    public void ACreditRuleOnTheWrongEpaField_IsRefusedUnderTheCreditJson_AfterCredit()
    {
        const string sentence = "Credit rule 1 reads its EPA from field 'epa', but the form's EPA field is 'epa_id'. The EPA an activity is filed against and the EPA it credits must be the same field.";
        var sender = new BuilderSender(BuilderKit.Kgk())
        {
            SaveRefusal = new ActivityTypeDraftRefusedException(
                [new RefusalPlace(RefusalTab.Credit, RefusalKind.CreditEpaFieldDisagrees, sentence) { DirectiveNumber = 1, NamedField = "epa", ExpectedField = "epa_id" }])
        };
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Tab(cut, "Credit");

        BuilderKit.Button(cut, "Save draft").Click();

        cut.Find("#refusal-summary li").TextContent.Trim().Should().Be($"Credit: {sentence}", "S4: never \"Credit, rule 1:\"");
        cut.Find("#credit-json-refusal").TextContent.Should().Be("Rule 1 reads its EPA from 'epa', but this form's EPA field is 'epa_id'. Use 'epa_id'.");
    }

    [Theory]
    [InlineData(RefusalTab.Workflow, 7L, "Workflow: The workflow is not valid JSON. Check the brackets and commas near line 7.")]
    [InlineData(RefusalTab.Credit, null, "Credit: The credit rules are not valid JSON. Check the brackets and commas.")]
    public void MalformedJson_IsWordedByThePage(RefusalTab tab, long? line, string expected)
    {
        var sender = new BuilderSender(BuilderKit.Kgk())
        {
            SaveRefusal = new ActivityTypeDraftRefusedException(
                [new RefusalPlace(tab, RefusalKind.MalformedJson, "'}' is invalid after a value. LineNumber: 6 | BytePositionInLine: 2.") { JsonLine = line }])
        };
        var cut = BuilderKit.Render(this, sender);

        BuilderKit.Button(cut, "Save draft").Click();

        cut.Find("#refusal-summary li").TextContent.Trim().Should().Be(expected);
        cut.Markup.Should().NotContain("BytePositionInLine", "never the reader's own text");
    }

    [Fact]
    public void AParseExceptionWithNoPlace_IsItsTabsLine()
    {
        var sender = new BuilderSender(BuilderKit.Kgk()) { SaveRefusal = new WorkflowParseException("Workflow must declare at least one state.") };
        var cut = BuilderKit.Render(this, sender);

        BuilderKit.Button(cut, "Save draft").Click();

        cut.Find("#refusal-summary li").TextContent.Trim().Should().Be("Workflow: Workflow must declare at least one state.");
    }

    [Fact]
    public void TheKeyTaken_IsRefusedUnderKey_AndOnDetails()
    {
        var sender = new BuilderSender(BuilderKit.New(BuilderKit.InstitutionScopes))
        {
            SaveRefusal = new FieldRefusalException("Key", "An activity type with key 'kgk_teaching_log' already exists.")
        };
        var cut = BuilderKit.Render(this, sender, id: null);
        cut.Find("#type-key").Change("kgk_teaching_log");
        cut.Find("#type-name").Change("KGK Teaching Session Log");

        BuilderKit.Button(cut, "Save draft").Click();

        cut.Find("#refusal-summary li").TextContent.Trim().Should().Be("Details: An activity type with key 'kgk_teaching_log' already exists.");
        cut.Find("#tab-details").TextContent.Trim().Should().Be("Details 1 problem");
        cut.Find("#type-key").GetAttribute("aria-describedby").Should().Be("type-key-refusal type-key-help");
        cut.Find("#type-key-refusal").TextContent.Should().Be("An activity type with key 'kgk_teaching_log' already exists.");
    }

    [Fact]
    public void DetailsFourWords_AreThePagesOwn_BeforeAnythingIsSent()
    {
        var sender = new BuilderSender(BuilderKit.New(BuilderKit.InstitutionScopes));
        var cut = BuilderKit.Render(this, sender, id: null);

        BuilderKit.Button(cut, "Save draft").Click();

        sender.Saves.Should().BeEmpty();
        cut.FindAll("#refusal-summary li").Select(li => li.TextContent.Trim()).Should().Equal("Details: Enter a key.", "Details: Enter a name.");

        cut.Find("#type-key").Change(new string('k', 101));
        cut.Find("#type-name").Change(new string('n', 201));
        BuilderKit.Button(cut, "Save draft").Click();

        cut.Find("#type-key-refusal").TextContent.Should().Be("A key is at most 100 characters.");
        cut.Find("#type-name-refusal").TextContent.Should().Be("A name is at most 200 characters.");
    }

    [Fact]
    public void AValidationRefusal_OfTwoProperties_IsEachUnderItsField()
    {
        var sender = new BuilderSender(BuilderKit.Kgk())
        {
            SaveRefusal = new ValidationException([new ValidationFailure("Key", "Enter a key."), new ValidationFailure("Name", "Enter a name.")])
        };
        var cut = BuilderKit.Render(this, sender);

        BuilderKit.Button(cut, "Save draft").Click();

        cut.Find("#tab-details").TextContent.Trim().Should().Be("Details 2 problems");
    }

    [Fact]
    public void TheGuardsSentence_IsShownInTheRegion_WithNoPlace()
    {
        const string guard = "You do not have permission to modify activity types in that speciality.";
        var sender = new BuilderSender(BuilderKit.Kgk()) { SaveRefusal = new UnauthorizedAccessException(guard) };
        var cut = BuilderKit.Render(this, sender);

        BuilderKit.Button(cut, "Save draft").Click();

        cut.Find(".action-result .alert-danger").TextContent.Trim().Should().Be(guard);
        cut.FindAll("#refusal-summary").Should().BeEmpty();
    }

    [Fact]
    public void AFailure_IsTheFixedSentence_NeverTheExceptionsText()
    {
        var sender = new BuilderSender(BuilderKit.Kgk()) { SaveRefusal = new InvalidOperationException("duplicate key value violates unique constraint") };
        var cut = BuilderKit.Render(this, sender);

        BuilderKit.Button(cut, "Save draft").Click();

        var alert = cut.Find(".action-result .alert-danger");
        alert.QuerySelector("strong")!.TextContent.Should().Be("The draft could not be saved.");
        alert.TextContent.Trim().Should().Be("The draft could not be saved. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("unique constraint");
    }

    [Fact]
    public void AFieldLeftWithNoKey_IsRefusedByThePage_AtItsRow()
    {
        var sender = new BuilderSender(BuilderKit.Kgk());
        var cut = BuilderKit.Render(this, sender);
        BuilderKit.Named(cut, "Edit field Topic").Click();
        cut.Find("#field-key").Change(string.Empty);

        BuilderKit.Button(cut, "Save draft").Click();

        sender.Saves.Should().BeEmpty("no schema can be written without the key");
        cut.Find("#refusal-summary li").TextContent.Trim().Should().Be("Form: Enter a field key.");
        cut.Find("#field-key-refusal").TextContent.Should().Be("Enter a field key.");
    }
}
