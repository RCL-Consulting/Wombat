using Wombat.Domain.Activities;
using Wombat.Domain.Epas;
using Wombat.Domain.Activities.Credit;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Infrastructure.Persistence;

/// <summary>
/// Which seeder owns which activity type, and how each one's publish inputs are built.
/// </summary>
public enum ActivityTypeSeedSource
{
    /// <summary>The ten generic starter types seeded by <see cref="DataSeeder"/>.</summary>
    Generic,

    /// <summary>The CPSA paediatric instruments seeded by <see cref="PaediatricCatalogueSeeder"/>.</summary>
    PaediatricCollege
}

/// <summary>
/// How a seed's <c>DisplayFieldsJson</c> is derived. The two seeders disagree, and the difference
/// is load-bearing: applying one rule to all fourteen types would rewrite display fields for one
/// group or the other on the next boot.
/// </summary>
public enum DisplayFieldsRule
{
    /// <summary>The first three field keys of the parsed schema — what <see cref="DataSeeder"/> has always done.</summary>
    FirstThreeSchemaFields,

    /// <summary>An empty array — what <see cref="PaediatricCatalogueSeeder"/> has always passed.</summary>
    None
}

/// <summary>One seeded activity type, as the seeders and the refresher both see it.</summary>
/// <param name="WbaToolKey">
/// Which College-named instrument the type is (T122), or null when it is not one. Positional and required, so
/// every entry states it: a new seed that forgot it would compile, seed null, and under D21 credit every EPA on
/// every curriculum with nobody told. The seeders write it on CREATE only. On an existing database the T122
/// migration stamped it once, and a later change here reaches existing rows only through a new migration; the
/// seeders log a warning where a seeded type's key differs from this entry.
/// </param>
public sealed record ActivityTypeSeedEntry(
    string Key,
    string Name,
    string Description,
    ActivityScope Scope,
    ActivityTypeSeedSource Source,
    DisplayFieldsRule DisplayFields,
    string? WbaToolKey);

/// <summary>The four JSON payloads that <see cref="ActivityType.SaveDraft"/> takes.</summary>
public sealed record ActivityTypeSeedPayload(
    string SchemaJson,
    string WorkflowJson,
    string CreditRulesJson,
    string DisplayFieldsJson);

/// <summary>
/// The single source of truth for the seeded activity types: their keys, their identity, and the
/// per-seeder display-fields rule.
/// </summary>
/// <remarks>
/// <para>
/// Both <see cref="DataSeeder"/> and <see cref="PaediatricCatalogueSeeder"/> create types from this
/// list, and <see cref="ActivityTypeSeedRefresher"/> evolves the same list. Before T103 the two
/// seeders each carried their own copy of <c>SeedActorUserId</c> and their own display-fields rule;
/// a refresher that guessed either would either revert operator work or churn data on every boot,
/// so they are stated here once.
/// </para>
/// <para>
/// Note what is deliberately NOT here: <c>ScopeId</c>. Each seeder resolves its own scope (the demo
/// speciality vs the CPSA Paediatrics speciality) at creation time, and the refresher never touches
/// scope, name, description or <c>WbaToolKey</c> — it evolves the four versioned JSON payloads and
/// nothing else.
/// </para>
/// </remarks>
public static class ActivityTypeSeedCatalogue
{
    /// <summary>
    /// The actor recorded as <c>OwnerUserId</c>, draft author and publisher for everything seeded
    /// here. The refresher's ownership guard keys off this exact value.
    /// </summary>
    public const string SeedActorUserId = "seed-system";

    /// <remarks>
    /// The generic Mini-CEX, DOPS and CBD carry their instrument keys although no generic curriculum has a tool list
    /// (T122). They ARE those instruments, and unkeyed they would be unrestricted on every EPA of every curriculum
    /// that does have one, for anyone who can file them. ACAT is not a College-named instrument, so under D21 it
    /// stays unkeyed and unrestricted.
    /// </remarks>
    public static IReadOnlyList<ActivityTypeSeedEntry> Entries { get; } =
    [
        new("mini_cex", "Mini-CEX", "Mini clinical evaluation exercise.",
            ActivityScope.Speciality, ActivityTypeSeedSource.Generic, DisplayFieldsRule.FirstThreeSchemaFields, WbaToolKey: "mini_cex"),
        new("dops", "DOPS", "Direct observation of procedural skills.",
            ActivityScope.Speciality, ActivityTypeSeedSource.Generic, DisplayFieldsRule.FirstThreeSchemaFields, WbaToolKey: "dops"),
        new("cbd", "Case-based Discussion", "Structured case-based discussion.",
            ActivityScope.Speciality, ActivityTypeSeedSource.Generic, DisplayFieldsRule.FirstThreeSchemaFields, WbaToolKey: "cbd"),
        new("acat", "ACAT", "Acute care assessment tool.",
            ActivityScope.Speciality, ActivityTypeSeedSource.Generic, DisplayFieldsRule.FirstThreeSchemaFields, WbaToolKey: null),
        new("reflective_note", "Reflective Note", "Structured reflective note using the situation-task-action-result frame.",
            ActivityScope.Speciality, ActivityTypeSeedSource.Generic, DisplayFieldsRule.FirstThreeSchemaFields, WbaToolKey: null),
        new("procedure_log", "Procedure Log", "Self-logged procedural experience.",
            ActivityScope.Speciality, ActivityTypeSeedSource.Generic, DisplayFieldsRule.FirstThreeSchemaFields, WbaToolKey: null),
        new("research_output", "Research Output", "Publication, poster, or presentation evidence.",
            ActivityScope.Speciality, ActivityTypeSeedSource.Generic, DisplayFieldsRule.FirstThreeSchemaFields, WbaToolKey: null),
        new("teaching_session", "Teaching Session", "Teaching activity delivered by the trainee.",
            ActivityScope.Speciality, ActivityTypeSeedSource.Generic, DisplayFieldsRule.FirstThreeSchemaFields, WbaToolKey: null),
        new("qi_project", "QI Project", "Quality-improvement project with fixed PDSA sections.",
            ActivityScope.Speciality, ActivityTypeSeedSource.Generic, DisplayFieldsRule.FirstThreeSchemaFields, WbaToolKey: null),
        new("journal_club", "Journal Club", "Journal club attendance or presentation log.",
            ActivityScope.Speciality, ActivityTypeSeedSource.Generic, DisplayFieldsRule.FirstThreeSchemaFields, WbaToolKey: null),

        // The keys are <family>_cpsa, and both halves matter — see PaediatricCatalogueSeeder for why.
        new("mini_cex_cpsa", "Mini-CEX (Paediatrics)",
            "Mini clinical evaluation exercise - direct observation of a focused clinical encounter, followed by immediate feedback.",
            ActivityScope.Speciality, ActivityTypeSeedSource.PaediatricCollege, DisplayFieldsRule.None, WbaToolKey: "mini_cex"),
        new("dops_cpsa", "DOPS (Paediatrics)",
            "Direct observation of procedural skills - technique, patient interaction and safety.",
            ActivityScope.Speciality, ActivityTypeSeedSource.PaediatricCollege, DisplayFieldsRule.None, WbaToolKey: "dops"),
        new("cbd_cpsa", "Case-Based Discussion (Paediatrics)",
            "Structured discussion of a case the trainee has managed, exploring clinical reasoning and decision-making.",
            ActivityScope.Speciality, ActivityTypeSeedSource.PaediatricCollege, DisplayFieldsRule.None, WbaToolKey: "cbd"),
        new("direct_observation_cpsa", "Direct Observation (Paediatrics)",
            "Observation of the trainee in routine practice - ward rounds, handover and family meetings.",
            ActivityScope.Speciality, ActivityTypeSeedSource.PaediatricCollege, DisplayFieldsRule.None, WbaToolKey: "direct_observation"),

        // System-written, never hand-filed. One row is created per EPA a released MSF campaign covers
        // (T121). A trainee CAN still create a stray draft from /activities/new — nothing in the product
        // expresses "system-managed" — but both its sections declare
        // `editable_by: role:Coordinator|role:Administrator`, so every field they submit is dropped at
        // creation, and the `record` transition carries the same rule, so the empty draft can never
        // become evidence. Hiding it from the picker needs a SystemManaged flag on ActivityType, which
        // belongs with [T118] findings 6 and 7.
        new("msf_cpsa", "Multi-Source Feedback (Paediatrics)",
            "The per-EPA evidence record a released multi-source feedback campaign leaves behind.",
            ActivityScope.Speciality, ActivityTypeSeedSource.PaediatricCollege, DisplayFieldsRule.None, WbaToolKey: "msf")
    ];

    /// <summary>
    /// The seed-owned types of <paramref name="source" /> whose stored <c>WbaToolKey</c> is not the one their entry
    /// declares (T122). For a startup warning only; nothing here writes.
    /// </summary>
    /// <remarks>
    /// The seeders stamp the key when they create a type and never again. That is deliberate: an administrator
    /// may withdraw or change a seeded type's instrument in the builder, and a reconcile on every boot would silently
    /// undo them. So a difference is announced at every startup instead, until someone decides which is right. A
    /// type an operator has taken over (another owner) is theirs, and is not reported.
    /// </remarks>
    public static IReadOnlyList<(ActivityTypeSeedEntry Entry, string? StoredWbaToolKey)> FindWbaToolKeyDrift(
        IEnumerable<ActivityType> storedTypes,
        ActivityTypeSeedSource source)
    {
        var entries = For(source).ToDictionary(entry => entry.Key, StringComparer.Ordinal);

        return storedTypes
            .Where(activityType => string.Equals(activityType.OwnerUserId, SeedActorUserId, StringComparison.Ordinal))
            .Where(activityType => entries.ContainsKey(activityType.Key))
            .Select(activityType => (Entry: entries[activityType.Key], Stored: WbaTool.NormalizeKey(activityType.WbaToolKey)))
            .Where(pair => !string.Equals(pair.Stored, WbaTool.NormalizeKey(pair.Entry.WbaToolKey), StringComparison.Ordinal))
            .Select(pair => (pair.Entry, pair.Stored))
            .ToList();
    }

    public static IEnumerable<ActivityTypeSeedEntry> For(ActivityTypeSeedSource source)
        => Entries.Where(entry => entry.Source == source);

    /// <summary>
    /// Reads a seed folder from disk and returns the four publish inputs, already canonicalised the
    /// way <see cref="ActivityType.SaveDraft"/> would store them.
    /// </summary>
    public static async Task<ActivityTypeSeedPayload> ReadCanonicalAsync(
        ActivityTypeSeedEntry entry,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var schemaJson = await ReadSeedFileAsync(entry.Key, "schema.json", cancellationToken);
        var workflowJson = await ReadSeedFileAsync(entry.Key, "workflow.json", cancellationToken);
        var creditRulesJson = await ReadSeedFileAsync(entry.Key, "credit.json", cancellationToken);

        return Canonicalise(schemaJson, workflowJson, creditRulesJson, BuildDisplayFieldsJson(entry, schemaJson));
    }

    /// <summary>
    /// Parses and re-serialises all four payloads, producing the exact strings
    /// <see cref="ActivityType.SaveDraft"/> would persist.
    /// </summary>
    /// <remarks>
    /// This is the only comparison form that survives a round trip through the database. The four
    /// columns are <c>jsonb</c>: PostgreSQL discards the submitted byte string, keeps a parsed tree,
    /// and renders its own text on read (keys reordered, <c>", "</c> separators inserted). Comparing
    /// a stored string against a freshly serialised one would therefore differ every time, and the
    /// refresher would bump the version on every boot, forever. Both sides go through here instead.
    /// </remarks>
    public static ActivityTypeSeedPayload Canonicalise(
        string schemaJson,
        string workflowJson,
        string creditRulesJson,
        string displayFieldsJson)
        => new(
            FormSchemaParser.Serialize(FormSchemaParser.Parse(schemaJson)),
            WorkflowParser.Serialize(WorkflowParser.Parse(workflowJson)),
            CreditRulesParser.Serialize(CreditRulesParser.Parse(creditRulesJson)),
            ActivityType.NormalizeDisplayFieldsJson(displayFieldsJson));

    public static string BuildDisplayFieldsJson(ActivityTypeSeedEntry entry, string schemaJson)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.DisplayFields == DisplayFieldsRule.None)
        {
            return "[]";
        }

        var fields = FormSchemaParser.Parse(schemaJson)
            .Sections
            .SelectMany(section => section.Fields)
            .Select(field => field.Key)
            .Take(3)
            .ToArray();

        return System.Text.Json.JsonSerializer.Serialize(fields);
    }

    public static async Task<string> ReadSeedFileAsync(
        string activityKey,
        string fileName,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", activityKey, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Seed file '{fileName}' for activity type '{activityKey}' was not found.", path);
        }

        return await File.ReadAllTextAsync(path, cancellationToken);
    }
}
