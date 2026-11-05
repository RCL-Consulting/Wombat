using System.Globalization;
using Wombat.Application.Features.Curricula;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The reach of a College's change, in words (T394, flow 16, Q2, Q21's EV2, S16, T8; build-contracts § 0): one sentence
/// for a curriculum version, one for an EPA, read by the curriculum's page, the items page, an item's page and an EPA's
/// page, so they cannot word it differently. Counts and institution names, never a registrar's name; every count has its
/// singular.
/// </summary>
/// <remarks>
/// The boards' words (R3-D06, F05, F09, E05, E07, E11) are the plural and the "none" forms; the singulars, the empty
/// registrar count and an own EPA's "none" are NEW (build-contracts § 6).
/// </remarks>
public static class CatalogueReachWords
{
    /// <summary>A version no institution trains against (R3-D05).</summary>
    public const string NoAdoption = "No institution has adopted this version yet.";

    /// <summary>
    /// A version's reach: "Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it."
    /// (R3-D06), then, where an institution has superseded it with registrars still on it, "Still used by 1 institution that
    /// has superseded it, …; 2 registrars in training on it." (T8); or <see cref="NoAdoption" />.
    /// </summary>
    public static string ForCurriculum(CurriculumReachDto reach)
    {
        ArgumentNullException.ThrowIfNull(reach);
        if (!reach.Reaches)
        {
            return NoAdoption;
        }

        var sentences = new List<string>(2);
        if (reach.AdoptedBy.Count > 0)
        {
            sentences.Add($"Adopted by {Institutions(reach.AdoptedBy.Count)}, {JoinAnd(reach.AdoptedBy)}; {OnIt(reach.RegistrarCount)}.");
        }

        if (reach.SupersededBy.Count > 0)
        {
            var who = reach.SupersededBy.Count == 1
                ? "1 institution that has superseded it"
                : Invariant($"{reach.SupersededBy.Count} institutions that have superseded it");
            sentences.Add($"Still used by {who}, {JoinAnd(reach.SupersededBy)}; {OnIt(reach.SupersededRegistrarCount)}.");
        }

        return string.Join(' ', sentences);
    }

    /// <summary>
    /// A national EPA's reach: "PAED-012 is on 1 curriculum adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3
    /// registrars in training are measured against it." (R3-E05, E07), or "PAED-016 is on no curriculum an institution has
    /// adopted, so no registrar is measured against it yet." (R3-E11). <paramref name="code" /> is the EPA's code.
    /// </summary>
    public static string ForNationalEpa(string code, EpaReachDto reach)
    {
        ArgumentNullException.ThrowIfNull(reach);
        if (reach.CurriculumCount == 0)
        {
            return $"{code} is on no curriculum an institution has adopted, so no registrar is measured against it yet.";
        }

        return $"{code} is on {Curricula(reach.CurriculumCount)} adopted by {Institutions(reach.Institutions.Count)}, " +
               $"{JoinAnd(reach.Institutions)}; {MeasuredAgainstIt(reach.RegistrarCount)}.";
    }

    /// <summary>
    /// An institution's own EPA's reach, through its owner's own items (Spec-2 § 9, no board): "KGK-001 is Kgosi Kgari
    /// Teaching Hospital's own item on 1 curriculum; 2 registrars in training are measured against it.", or NEW for none:
    /// "KGK-001 is on no curriculum Kgosi Kgari Teaching Hospital trains against, so no registrar is measured against it yet."
    /// </summary>
    public static string ForOwnEpa(string code, string ownerName, EpaReachDto reach)
    {
        ArgumentNullException.ThrowIfNull(reach);
        if (reach.CurriculumCount == 0)
        {
            return $"{code} is on no curriculum {ownerName} trains against, so no registrar is measured against it yet.";
        }

        return $"{code} is {ownerName}'s own item on {Curricula(reach.CurriculumCount)}; {MeasuredAgainstIt(reach.RegistrarCount)}.";
    }

    /// <summary>"Kgosi Kgari Teaching Hospital", "A and B", "A, B and C": names joined as the boards join them.</summary>
    public static string JoinAnd(IReadOnlyList<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);
        return names.Count switch
        {
            0 => string.Empty,
            1 => names[0],
            _ => $"{string.Join(", ", names.Take(names.Count - 1))} and {names[^1]}"
        };
    }

    private static string Institutions(int count) => count == 1 ? "1 institution" : Invariant($"{count} institutions");

    private static string Curricula(int count) => count == 1 ? "1 curriculum" : Invariant($"{count} curricula");

    private static string OnIt(int registrars) => registrars switch
    {
        0 => "no registrar in training on it",
        1 => "1 registrar in training on it",
        _ => Invariant($"{registrars} registrars in training on it")
    };

    private static string MeasuredAgainstIt(int registrars) => registrars switch
    {
        0 => "no registrar in training is measured against it",
        1 => "1 registrar in training is measured against it",
        _ => Invariant($"{registrars} registrars in training are measured against it")
    };

    private static string Invariant(FormattableString value) => value.ToString(CultureInfo.InvariantCulture);
}
