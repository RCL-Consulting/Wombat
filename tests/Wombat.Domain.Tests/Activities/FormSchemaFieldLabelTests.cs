using Wombat.Domain.Activities.Schema;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T172: <see cref="FormSchema.FieldLabel" /> is the one place a field key becomes the words the form shows. Every
/// refusal and disabled-action reason that names a field goes through it.
/// </summary>
public sealed class FormSchemaFieldLabelTests
{
    [Fact]
    public void ADeclaredField_IsNamedByItsLabel()
    {
        var schema = FormSchemaParser.Parse("""
            {
              "version": 1,
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "presenting_problem", "type": "text", "label": "Presenting problem", "required": true }
                  ]
                }
              ]
            }
            """);

        Assert.Equal("Presenting problem", schema.FieldLabel("presenting_problem"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AFieldWithABlankLabel_IsNamedByItsKey(string blank)
    {
        // The parser refuses a blank label; a schema built in code does not pass through it.
        var schema = Schema(Field("presenting_problem", blank));

        Assert.Equal("presenting_problem", schema.FieldLabel("presenting_problem"));
    }

    [Fact]
    public void AKeyTheSchemaDoesNotDeclare_IsNamedByItself()
    {
        var schema = Schema(Field("presenting_problem", "Presenting problem"));

        Assert.Equal("unlabelled_note", schema.FieldLabel("unlabelled_note"));
    }

    [Fact]
    public void ALabel_IsNamedWithoutItsSurroundingSpace()
    {
        var schema = Schema(Field("presenting_problem", "  Presenting problem "));

        Assert.Equal("Presenting problem", schema.FieldLabel("presenting_problem"));
    }

    [Fact]
    public void AFieldInALaterSection_IsFound()
    {
        var schema = new FormSchema(1,
        [
            new FormSection("request", "Request", null, [Field("presenting_problem", "Presenting problem")], null),
            new FormSection("feedback", "Feedback", null, [Field("strengths", "What was done well")], null)
        ]);

        Assert.Equal("What was done well", schema.FieldLabel("strengths"));
    }

    [Fact]
    public void ARepeatedLabel_IsNamedWithTheSectionItSitsUnder()
    {
        // The shape qi_project ships: the same four labels under each PDSA cycle. "Plan" alone would not say which.
        var schema = FormSchemaParser.Parse("""
            {
              "version": 1,
              "sections": [
                {
                  "key": "project",
                  "title": "Project",
                  "fields": [
                    { "key": "project_title", "type": "text", "label": "Project title", "required": true }
                  ]
                },
                {
                  "key": "pdsa_1",
                  "title": "PDSA cycle 1",
                  "fields": [
                    { "key": "pdsa_1_plan", "type": "longtext", "label": "Plan", "required": true },
                    { "key": "pdsa_1_do", "type": "longtext", "label": "Do", "required": true }
                  ]
                },
                {
                  "key": "pdsa_2",
                  "title": "PDSA cycle 2",
                  "fields": [
                    { "key": "pdsa_2_plan", "type": "longtext", "label": "Plan" },
                    { "key": "pdsa_2_do", "type": "longtext", "label": "Do" }
                  ]
                }
              ]
            }
            """);

        Assert.Equal("Plan (PDSA cycle 1)", schema.FieldLabel("pdsa_1_plan"));
        Assert.Equal("Do (PDSA cycle 2)", schema.FieldLabel("pdsa_2_do"));
        Assert.Equal("Project title", schema.FieldLabel("project_title"));
    }

    [Fact]
    public void LabelsThatReadTheSame_AreRepeated_WhateverTheirCaseOrSpacing()
    {
        var schema = new FormSchema(1,
        [
            new FormSection("pdsa_1", "PDSA cycle 1", null, [Field("pdsa_1_plan", "Plan")], null),
            new FormSection("pdsa_2", " PDSA cycle 2 ", null, [Field("pdsa_2_plan", " plan ")], null)
        ]);

        Assert.Equal("Plan (PDSA cycle 1)", schema.FieldLabel("pdsa_1_plan"));
        Assert.Equal("plan (PDSA cycle 2)", schema.FieldLabel("pdsa_2_plan"));
    }

    [Fact]
    public void ARepeatedLabel_InASectionWithNoTitle_IsNamedByItsLabelAlone()
    {
        // The parser refuses a blank title; a schema built in code does not pass through it. "Plan ()" would say nothing.
        var schema = new FormSchema(1,
        [
            new FormSection("pdsa_1", " ", null, [Field("pdsa_1_plan", "Plan")], null),
            new FormSection("pdsa_2", "PDSA cycle 2", null, [Field("pdsa_2_plan", "Plan")], null)
        ]);

        Assert.Equal("Plan", schema.FieldLabel("pdsa_1_plan"));
        Assert.Equal("Plan (PDSA cycle 2)", schema.FieldLabel("pdsa_2_plan"));
    }

    private static FormSchema Schema(FormField field)
        => new(1, [new FormSection("main", "Main", null, [field], null)]);

    private static FormField Field(string key, string label)
        => new(key, FieldType.Text, label, null, false, [], null, null, null, null, null, null);
}
