using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights.Queries;

/// <summary>
/// How the data-rights reads name a person (T387, F1, S1, N5, N13): the queue and a request's page read the same rule, so
/// they cannot name one person two ways.
/// </summary>
/// <remarks>
/// <para>
/// A living account is named live, "First Last" from the directory (<c>GetContactsAsync</c>), so a name corrected on My
/// account shows at once (M1). Until T387 the pages printed the name stored at submission, which was the sign-in claim:
/// the address, most often.
/// </para>
/// <para>
/// An erased person is named by their pseudonym, with no address. They are known by their erasure record
/// (<see cref="DataRightsErasureRecord" />, one per erased person), matched by the id they had (a request keeps its
/// <c>RequesterUserId</c>) or by the pseudonym the erasure wrote over a decider's or a recorder's id (F3). Their account
/// still exists with its names blanked, so it is never asked for a name.
/// </para>
/// </remarks>
internal static class DataRightsPeople
{
    /// <summary>The erasure records of everyone in <paramref name="ids" />, by the id they had or by their pseudonym, in one query.</summary>
    public static async Task<DataRightsErasures> ErasuresOfAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return DataRightsErasures.None;
        }

        var wanted = ids.Distinct(StringComparer.Ordinal).ToArray();
        var records = await dbContext.Set<DataRightsErasureRecord>()
            .AsNoTracking()
            .Where(record => wanted.Contains(record.UserId) || wanted.Contains(record.Pseudonym))
            .ToListAsync(cancellationToken);
        return new DataRightsErasures(records);
    }

    /// <summary>
    /// The requester as every sentence names them: the pseudonym once erased; else the live "First Last"; else (an account
    /// with no name, or none found) the name stored at submission.
    /// </summary>
    public static string RequesterName(DataRightsRequest request, DataRightsErasureRecord? erasure, UserContact? contact)
    {
        if (erasure is not null)
        {
            return erasure.Pseudonym;
        }

        var live = contact is null ? string.Empty : $"{contact.FirstName} {contact.LastName}".Trim();
        return live.Length > 0 ? live : request.RequesterDisplayName;
    }

    /// <summary>The live address (S1); null when none was read, which is so for an erased person, who is never asked about.</summary>
    public static string? EmailOf(UserContact? contact)
        => string.IsNullOrWhiteSpace(contact?.Email) ? null : contact.Email;

    /// <summary>
    /// The deciders' and recorders' names, by the id the request holds: the pseudonym for one erased (it is then the id
    /// itself, F3), the live "First Last" otherwise, in one directory read; the id itself for an account the directory
    /// does not know or that has no name, so a row is never left unnamed.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, string>> StaffNamesAsync(
        IUserAdministrationService users,
        DataRightsErasures erasures,
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken)
    {
        var living = ids.Where(id => erasures.Of(id) is null).Distinct(StringComparer.Ordinal).ToArray();
        var live = living.Length == 0
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : await users.GetDisplayNamesAsync(living, cancellationToken);

        return ids.Distinct(StringComparer.Ordinal).ToDictionary(
            id => id,
            id => erasures.Of(id)?.Pseudonym
                  ?? (live.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name) ? name : id),
            StringComparer.Ordinal);
    }
}

/// <summary>The erasure records a read found, looked up by the id a person had or by their pseudonym (T387).</summary>
internal sealed class DataRightsErasures
{
    public static readonly DataRightsErasures None = new([]);

    private readonly IReadOnlyList<DataRightsErasureRecord> _records;

    public DataRightsErasures(IReadOnlyList<DataRightsErasureRecord> records) => _records = records;

    /// <summary>The erasure of the person <paramref name="id" /> names, by their old id or their pseudonym; null for one never erased.</summary>
    public DataRightsErasureRecord? Of(string id)
        => _records
            .Where(record => string.Equals(record.UserId, id, StringComparison.Ordinal) ||
                             string.Equals(record.Pseudonym, id, StringComparison.Ordinal))
            .OrderBy(record => record.ErasedOn)
            .FirstOrDefault();
}
