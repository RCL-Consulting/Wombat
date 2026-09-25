namespace Wombat.Web.Components.Shared;

/// <summary>
/// What a list's row actions name, where the row's own words can repeat (DESIGN.md § Button system; T239). A column of
/// identical "View" or "Open" buttons is indistinguishable to a screen reader, so each action's <c>aria-label</c> carries
/// its visible label and names its row. Where a list's key column is unique (an institution's name, an EPA's code), the
/// page names the row by it. Where it is not, the row is named by what it shows, and this makes the names differ.
/// </summary>
public static class RowNames
{
    /// <summary>
    /// Each row's name, by <paramref name="key" />, and no two the same: <paramref name="name" />, and where two rows
    /// would share one, each <paramref name="tieBreakers" /> suffix in turn, added to every row still sharing a name. A
    /// tie-breaker that says the same of every row sharing a name tells them nothing, so it is not added to them. Rows
    /// that every tie-breaker leaves sharing a name are numbered in list order, "(1 of 2)". A row whose name no other row
    /// has keeps it plain.
    /// </summary>
    /// <remarks>
    /// A tie-breaker repeats what the row shows where it can (the email beside a name, a review's type), so a sighted
    /// user and a screen-reader user tell the rows apart by the same words. The numbering is the last resort, and it too
    /// is something every user can see: the rows' order on the list. Until the T239 review the last tie-breaker was
    /// trusted to separate the rows, and on four lists it could not (two drafts saved in one minute, two panels of one
    /// name).
    /// </remarks>
    public static IReadOnlyDictionary<TKey, string> Distinct<TItem, TKey>(
        IEnumerable<TItem> rows,
        Func<TItem, TKey> key,
        Func<TItem, string> name,
        params Func<TItem, string>[] tieBreakers)
        where TKey : notnull
    {
        var list = rows.ToList();
        var names = list.ToDictionary(key, name);

        foreach (var tieBreaker in tieBreakers)
        {
            var shared = Shared(list, key, names);
            if (shared.Count == 0)
            {
                return names;
            }

            foreach (var group in shared)
            {
                var suffixes = group.Select(tieBreaker).ToList();
                if (suffixes.Distinct(StringComparer.Ordinal).Count() == 1)
                {
                    continue;
                }

                for (var index = 0; index < group.Count; index++)
                {
                    names[key(group[index])] += suffixes[index];
                }
            }
        }

        foreach (var group in Shared(list, key, names))
        {
            for (var index = 0; index < group.Count; index++)
            {
                names[key(group[index])] += $" ({index + 1} of {group.Count})";
            }
        }

        return names;
    }

    /// <summary>The rows that share a name, a group for each name, each in list order.</summary>
    private static List<List<TItem>> Shared<TItem, TKey>(
        List<TItem> list, Func<TItem, TKey> key, Dictionary<TKey, string> names)
        where TKey : notnull
        => list.GroupBy(row => names[key(row)], StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.ToList())
            .ToList();
}
