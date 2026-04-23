using FluentAssertions;
using System.Security.Claims;
using System.Text.Json;
using Wombat.Application.Audit;

namespace Wombat.Application.Tests.Audit;

public sealed class AuditPayloadSerializerTests
{
    [Fact]
    public void Serialize_PlainCommand_IncludesAllProperties()
    {
        var command = new PlainCommand("alice@test.com", 42);

        var json = AuditPayloadSerializer.Serialize(command);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("email").GetString().Should().Be("alice@test.com");
        doc.RootElement.GetProperty("count").GetInt32().Should().Be(42);
    }

    [Fact]
    public void Serialize_CommandWithRedactedProperty_ReplacesWithRedacted()
    {
        var command = new CommandWithSecret("alice@test.com", "super-secret-token");

        var json = AuditPayloadSerializer.Serialize(command);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("email").GetString().Should().Be("alice@test.com");
        doc.RootElement.GetProperty("token").GetString().Should().Be("[REDACTED]");
    }

    [Fact]
    public void Serialize_CommandWithMultipleRedactedProperties_RedactsAll()
    {
        var command = new CommandWithTwoSecrets("user@x.com", "pw123", "tok456");

        var json = AuditPayloadSerializer.Serialize(command);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("email").GetString().Should().Be("user@x.com");
        doc.RootElement.GetProperty("password").GetString().Should().Be("[REDACTED]");
        doc.RootElement.GetProperty("token").GetString().Should().Be("[REDACTED]");
    }

    [Fact]
    public void Serialize_EmptyCommand_ReturnsEmptyObject()
    {
        var json = AuditPayloadSerializer.Serialize(new EmptyCommand());
        json.Should().Be("{}");
    }

    [Fact]
    public void Serialize_CommandWithClaimsPrincipal_ReplacesWithPrincipalMarker()
    {
        // ClaimsPrincipal has a Claims[].Subject cycle that blew up System.Text.Json
        // before this was handled explicitly. The actor identity is captured separately
        // on AuditEntry so a placeholder here is the correct summary.
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Name, "alice"),
        }, "test"));
        var command = new CommandWithPrincipal("hello", principal);

        var json = AuditPayloadSerializer.Serialize(command);
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("message").GetString().Should().Be("hello");
        doc.RootElement.GetProperty("actor").GetString().Should().Be("[PRINCIPAL]");
    }

    // Test DTOs — use property form so [Redact] is on a property, not a ctor param
    private sealed record PlainCommand(string Email, int Count);

    private sealed class CommandWithSecret
    {
        public CommandWithSecret(string email, string token) { Email = email; Token = token; }
        public string Email { get; }
        [Redact] public string Token { get; }
    }

    private sealed class CommandWithTwoSecrets
    {
        public CommandWithTwoSecrets(string email, string password, string token) { Email = email; Password = password; Token = token; }
        public string Email { get; }
        [Redact] public string Password { get; }
        [Redact] public string Token { get; }
    }

    private sealed record EmptyCommand;

    private sealed record CommandWithPrincipal(string Message, ClaimsPrincipal Actor);
}
