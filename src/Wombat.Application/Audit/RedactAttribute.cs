namespace Wombat.Application.Audit;

/// <summary>
/// Marks a command property as sensitive. The AuditPayloadSerializer replaces
/// the value with "[REDACTED]" before writing the SummaryJson to the audit log.
/// Apply to passwords, tokens, raw email bodies, and file contents.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class RedactAttribute : Attribute;
