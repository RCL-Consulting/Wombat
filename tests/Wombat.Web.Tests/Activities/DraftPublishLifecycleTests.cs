using FluentAssertions;
using Wombat.Domain.Activities;

namespace Wombat.Web.Tests.Activities;

public sealed class DraftPublishLifecycleTests
{
    [Fact]
    public void SaveDraft_ThenPublish_BumpsVersionAndStoresPublishedHistory()
    {
        var activityType = new ActivityType
        {
            Key = "hello_world",
            Name = "Hello World",
            OwnerUserId = "admin-1"
        };

        activityType.SaveDraft(
            """
            {
              "version": 1,
              "sections": [
                {
                  "key": "greeting",
                  "title": "Greeting",
                  "fields": [
                    { "key": "message", "type": "text", "label": "Your message", "required": true }
                  ]
                }
              ]
            }
            """,
            """
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "submitted", "label": "Submitted", "terminal": true }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject", "requires_fields": ["message"] }
              ]
            }
            """,
            """{ "counts_for": [] }""",
            """["message"]""",
            "admin-1");

        var published = activityType.PublishDraft("admin-1");

        activityType.Version.Should().Be(1);
        activityType.HasDraft.Should().BeFalse();
        activityType.SchemaJson.Should().NotBeNullOrWhiteSpace();
        published.Version.Should().Be(1);
        published.SchemaJson.Should().Be(activityType.SchemaJson);
        activityType.Versions.Should().ContainSingle();
    }
}
