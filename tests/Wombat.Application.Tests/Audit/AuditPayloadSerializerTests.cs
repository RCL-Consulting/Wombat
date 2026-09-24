using FluentAssertions;
using MediatR;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Wombat.Application.Audit;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;

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

    /// <summary>
    /// A respondent's address is written nowhere in the audit summary of the command that invites them. Closing a
    /// campaign anonymises the invitations, not the audit trail, so an address written here outlived it. (T184)
    /// </summary>
    [Fact]
    public void Serialize_AddMsfInvitationCommand_RedactsTheRespondentEmail()
    {
        var command = new AddMsfInvitationCommand(
            7, "nurse-1@example.test", MsfRespondentCategory.Nurse, new ClaimsPrincipal());

        var json = AuditPayloadSerializer.Serialize(command);
        var doc = JsonDocument.Parse(json);

        json.Should().NotContain("nurse-1@example.test");
        doc.RootElement.GetProperty("respondentEmail").GetString().Should().Be("[REDACTED]");
        doc.RootElement.GetProperty("campaignId").GetInt32().Should().Be(7);
        doc.RootElement.GetProperty("respondentCategory").GetInt32().Should().Be((int)MsfRespondentCategory.Nurse);
    }

    /// <summary>
    /// Every MSF request the audit pipeline records redacts every property that identifies a respondent: an address,
    /// or the single-use token that stands in for one. A new MSF command carrying either fails here until it is marked.
    /// (T101, T184)
    /// </summary>
    [Fact]
    public void EveryAuditedMsfRequest_RedactsEveryRespondentIdentifier()
    {
        // The pipeline's own predicate (AuditPipelineBehavior.IsCommand): a "Command" name, or the opt-in marker.
        var audited = typeof(AddMsfInvitationCommand).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(AddMsfInvitationCommand).Namespace)
            .Where(type => typeof(IBaseRequest).IsAssignableFrom(type) && !type.IsAbstract)
            .Where(type => type.Name.EndsWith("Command", StringComparison.Ordinal)
                           || typeof(IAuditedCommand).IsAssignableFrom(type))
            .ToList();

        var identifiers = audited
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.Name.Contains("Email", StringComparison.OrdinalIgnoreCase)
                                   || property.Name.Contains("Token", StringComparison.OrdinalIgnoreCase))
                .Select(property => (Type: type, Property: property)))
            .ToList();

        // Not vacuous: the two known identifiers are among those checked.
        identifiers.Select(pair => $"{pair.Type.Name}.{pair.Property.Name}").Should().Contain(
        [
            $"{nameof(AddMsfInvitationCommand)}.{nameof(AddMsfInvitationCommand.RespondentEmail)}",
            $"{nameof(SubmitMsfResponseCommand)}.{nameof(SubmitMsfResponseCommand.Token)}"
        ]);

        identifiers
            .Where(pair => pair.Property.GetCustomAttribute<RedactAttribute>() is null)
            .Select(pair => $"{pair.Type.Name}.{pair.Property.Name}")
            .Should().BeEmpty("a respondent identifier written to the audit log undoes MSF's anonymity");
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
