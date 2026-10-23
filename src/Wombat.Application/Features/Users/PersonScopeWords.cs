using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Users;

/// <summary>
/// A person's one scope in the people pages' words (T376, flow 12, S1; build-contracts § 0): Users' Scope column, the user
/// page's who line and Add role's results, and the Assessors list. One place, so the three cannot drift. A person holds one
/// scope, whatever roles they hold (Q4), so the scope is the person's and never a role's.
/// </summary>
/// <remarks>
/// The rule, first match wins:
/// <list type="number">
/// <item>An Institutional admin reads as their institution's name ("Kgosi Kgari Teaching Hospital").</item>
/// <item>A College admin reads as their College's name ("College of Paediatricians of South Africa").</item>
/// <item>Anyone with no institution (the Administrator) reads "—": no scope applies.</item>
/// <item>Otherwise the speciality names, comma-joined, then " / " and the sub-speciality names, comma-joined:
/// "Paediatrics / Paediatrics", "Paediatrics", "Paediatrics, ‹second speciality›" (P34: two ids, as SSO can add).</item>
/// <item>No speciality at all reads "Any speciality" (Q18's words, flow 11).</item>
/// </list>
/// A person with no role reads by the same rule (du Plessis's "Paediatrics / Paediatrics" stays, U07).
/// </remarks>
public static class PersonScopeWords
{
    /// <summary>What a person with no speciality reads (Q18, flow 11's Invite a person).</summary>
    public const string AnySpeciality = "Any speciality";

    /// <summary>What a person with no institution reads in the Scope cell: no scope applies (U14's Administrator).</summary>
    public const string NoScope = "—";

    /// <summary>What an account with no institution reads where its institution is named (U14, the who line; one wording).</summary>
    public const string NoInstitution = "Global (no institution)";

    /// <summary>The scope in words, by the rule above, from names already read (<see cref="PersonScopeNames" />).</summary>
    public static string For(
        IReadOnlyCollection<string> roles,
        string? institutionName,
        string? collegeName,
        IReadOnlyCollection<string> specialityNames,
        IReadOnlyCollection<string> subSpecialityNames)
    {
        ArgumentNullException.ThrowIfNull(roles);
        ArgumentNullException.ThrowIfNull(specialityNames);
        ArgumentNullException.ThrowIfNull(subSpecialityNames);

        if (roles.Contains(WombatRoles.InstitutionalAdmin, StringComparer.Ordinal) && !string.IsNullOrWhiteSpace(institutionName))
        {
            return institutionName.Trim();
        }

        if (roles.Contains(WombatRoles.CollegeAdmin, StringComparer.Ordinal) && !string.IsNullOrWhiteSpace(collegeName))
        {
            return collegeName.Trim();
        }

        if (string.IsNullOrWhiteSpace(institutionName))
        {
            return NoScope;
        }

        return Disciplines(specialityNames, subSpecialityNames) ?? AnySpeciality;
    }

    /// <summary>
    /// "Paediatrics / Paediatrics": the speciality names comma-joined, then " / " and the sub-speciality names when there
    /// are any; null when there is no speciality. Names sort ordinally, ignoring case, so every page prints one order.
    /// </summary>
    public static string? Disciplines(IReadOnlyCollection<string> specialityNames, IReadOnlyCollection<string> subSpecialityNames)
    {
        var specialities = Sorted(specialityNames);
        if (specialities.Length == 0)
        {
            return null;
        }

        var subSpecialities = Sorted(subSpecialityNames);
        return subSpecialities.Length == 0
            ? string.Join(", ", specialities)
            : $"{string.Join(", ", specialities)} / {string.Join(", ", subSpecialities)}";
    }

    private static string[] Sorted(IReadOnlyCollection<string> names)
        => names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(name => name, StringComparer.Ordinal)
            .ToArray();
}

/// <summary>
/// The names <see cref="PersonScopeWords" /> needs for a set of people, read in four queries whatever the set's size
/// (T376, flow 12, S1; build-contracts § 0): their institutions, Colleges, specialities and sub-specialities. A page's query
/// loads it once for its rows (Users' page of 20, the Assessors list, one person's page), never per row and never in Razor.
/// </summary>
public sealed class PersonScopeNames
{
    private readonly IReadOnlyDictionary<int, string> _institutions;
    private readonly IReadOnlyDictionary<int, string> _colleges;
    private readonly IReadOnlyDictionary<int, string> _specialities;
    private readonly IReadOnlyDictionary<int, string> _subSpecialities;

    private PersonScopeNames(
        IReadOnlyDictionary<int, string> institutions,
        IReadOnlyDictionary<int, string> colleges,
        IReadOnlyDictionary<int, string> specialities,
        IReadOnlyDictionary<int, string> subSpecialities)
    {
        _institutions = institutions;
        _colleges = colleges;
        _specialities = specialities;
        _subSpecialities = subSpecialities;
    }

    /// <summary>Reads the names every one of <paramref name="people" /> needs.</summary>
    public static async Task<PersonScopeNames> LoadAsync(
        IApplicationDbContext dbContext,
        IEnumerable<UserIdentityDetails> people,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(people);

        var all = people.ToArray();
        var institutionIds = all.Where(p => p.InstitutionId.HasValue).Select(p => p.InstitutionId!.Value).Distinct().ToArray();
        var collegeIds = all.Where(p => p.CollegeId.HasValue).Select(p => p.CollegeId!.Value).Distinct().ToArray();
        var specialityIds = all.SelectMany(p => p.SpecialityIds).Distinct().ToArray();
        var subSpecialityIds = all.SelectMany(p => p.SubSpecialityIds).Distinct().ToArray();

        var institutions = institutionIds.Length == 0
            ? new Dictionary<int, string>()
            : await dbContext.Set<Institution>().AsNoTracking()
                .Where(entity => institutionIds.Contains(entity.Id))
                .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);
        var colleges = collegeIds.Length == 0
            ? new Dictionary<int, string>()
            : await dbContext.Set<College>().AsNoTracking()
                .Where(entity => collegeIds.Contains(entity.Id))
                .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);
        var specialities = specialityIds.Length == 0
            ? new Dictionary<int, string>()
            : await dbContext.Set<Speciality>().AsNoTracking()
                .Where(entity => specialityIds.Contains(entity.Id))
                .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);
        var subSpecialities = subSpecialityIds.Length == 0
            ? new Dictionary<int, string>()
            : await dbContext.Set<SubSpeciality>().AsNoTracking()
                .Where(entity => subSpecialityIds.Contains(entity.Id))
                .ToDictionaryAsync(entity => entity.Id, entity => entity.Name, cancellationToken);

        return new PersonScopeNames(institutions, colleges, specialities, subSpecialities);
    }

    /// <summary>The person's institution's name, or null for none (the page writes <see cref="PersonScopeWords.NoInstitution" />).</summary>
    public string? InstitutionName(UserIdentityDetails person)
        => person.InstitutionId is int id && _institutions.TryGetValue(id, out var name) ? name : null;

    /// <summary>The person's College's name (a College admin's), or null.</summary>
    public string? CollegeName(UserIdentityDetails person)
        => person.CollegeId is int id && _colleges.TryGetValue(id, out var name) ? name : null;

    /// <summary>The person's speciality names, in the order <see cref="PersonScopeWords.Disciplines" /> prints them.</summary>
    public IReadOnlyList<string> SpecialityNames(UserIdentityDetails person)
        => Names(person.SpecialityIds, _specialities);

    /// <summary>The person's sub-speciality names.</summary>
    public IReadOnlyList<string> SubSpecialityNames(UserIdentityDetails person)
        => Names(person.SubSpecialityIds, _subSpecialities);

    /// <summary>The person's scope in words (<see cref="PersonScopeWords.For" />).</summary>
    public string Scope(UserIdentityDetails person)
    {
        ArgumentNullException.ThrowIfNull(person);
        return PersonScopeWords.For(
            person.Roles,
            InstitutionName(person),
            CollegeName(person),
            SpecialityNames(person),
            SubSpecialityNames(person));
    }

    private static IReadOnlyList<string> Names(IEnumerable<int> ids, IReadOnlyDictionary<int, string> lookup)
        => ids
            .Select(id => lookup.TryGetValue(id, out var name) ? name : null)
            .Where(name => name is not null)
            .Select(name => name!)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(name => name, StringComparer.Ordinal)
            .ToArray();
}
