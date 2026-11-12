namespace Wombat.Application.Features.Audit;

/// <summary>
/// The audit log's Institution filter (T400, flow 18, K15, T19): every row, the rows with no institution, or one
/// institution's. An <c>int?</c> cannot say the three apart (null would be both "all" and "none"). The Administrator's
/// alone: the query ignores it for an institution's administrator, pinned to theirs (T101).
/// </summary>
public readonly record struct AuditInstitutionFilter
{
    private AuditInstitutionFilter(AuditInstitutionFilterKind kind, int? institutionId)
    {
        Kind = kind;
        InstitutionId = institutionId;
    }

    public AuditInstitutionFilterKind Kind { get; }

    /// <summary>The institution, for <see cref="AuditInstitutionFilterKind.One" />; null otherwise.</summary>
    public int? InstitutionId { get; }

    /// <summary>Every row the caller may read.</summary>
    public static AuditInstitutionFilter All => new(AuditInstitutionFilterKind.All, null);

    /// <summary>The rows with no institution: the Administrator's own, the jobs', the MSF respondents', a failed sign-in.</summary>
    public static AuditInstitutionFilter NoInstitution => new(AuditInstitutionFilterKind.None, null);

    /// <summary>One institution's rows.</summary>
    public static AuditInstitutionFilter One(int institutionId) => new(AuditInstitutionFilterKind.One, institutionId);
}

/// <summary>The three kinds of <see cref="AuditInstitutionFilter" />.</summary>
public enum AuditInstitutionFilterKind
{
    All,
    None,
    One,
}
