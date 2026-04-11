using System.Text.Json;

namespace Wombat.Domain.Tests.Activities;

internal static class ActivityTestData
{
    public const string ValidSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "details",
              "title": "Details",
              "fields": [
                {
                  "key": "title",
                  "type": "text",
                  "label": "Title",
                  "required": true
                },
                {
                  "key": "score",
                  "type": "number",
                  "label": "Score",
                  "required": false,
                  "validation": {
                    "min": 1,
                    "max": 5
                  }
                }
              ]
            }
          ]
        }
        """;

    public const string ValidWorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Submitted" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject" },
            { "key": "complete", "from": "submitted", "to": "completed", "actor": "role:Assessor", "requires_note": true, "requires_fields": ["title"] }
          ]
        }
        """;

    public const string ValidCreditRulesJson = """
        {
          "counts_for": [
            {
              "curriculum_item_match": {
                "epa_field": "epa_id"
              },
              "amount": 1,
              "minimum_level_field": "score"
            }
          ]
        }
        """;

    public static string NormalizeJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement);
    }
}
