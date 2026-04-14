namespace Wombat.Application.Audit;

/// <summary>
/// Marker interface for commands that opt in to audit logging explicitly.
/// Commands named *Command are audited automatically by the pipeline behaviour.
/// Implement this interface for audited commands whose names don't follow that convention.
/// </summary>
public interface IAuditedCommand;
