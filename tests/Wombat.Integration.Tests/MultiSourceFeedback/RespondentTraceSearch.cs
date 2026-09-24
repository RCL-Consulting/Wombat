using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Npgsql;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// Searches every row of every table in one schema for anything computable from a respondent's address alone: the
/// address, and its unsalted SHA-256, SHA-512, SHA-1 and MD5 digests of the address as typed, trimmed, upper- or
/// lower-cased, as hex or base64. (T207)
/// </summary>
/// <remarks>
/// Until T207 anonymising an invitation kept an unsalted SHA-256 of the upper-cased address, which anyone holding the
/// invited addresses could recompute and match to its row. So the search is not "no column named for a hash" but "no
/// value anyone could compute from the address". Each row is read as its whole text (<c>t::text</c>), so a new column
/// or a new table is searched without this list changing.
/// </remarks>
internal static class RespondentTraceSearch
{
    /// <summary>
    /// The rows, as <c>table: row</c>, that hold a trace of <paramref name="address" />. The connection's search path
    /// must be the schema under test and nothing else.
    /// </summary>
    public static async Task<List<string>> TracesAsync(string schemaConnectionString, string address)
    {
        var traces = TracesOf(address);
        var found = new List<string>();

        await using var connection = new NpgsqlConnection(schemaConnectionString);
        await connection.OpenAsync();

        var tables = new List<string>();
        await using (var list = new NpgsqlCommand(
            """
            SELECT table_name FROM information_schema.tables
            WHERE table_schema = current_schema() AND table_type = 'BASE TABLE'
            """, connection))
        await using (var reader = await list.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
        }

        tables.Should().Contain("MsfInvitations", "guard: the search reaches the invitations");

        foreach (var table in tables)
        {
            var quoted = "\"" + table.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
            await using var rows = new NpgsqlCommand($"SELECT t::text FROM {quoted} AS t", connection);
            await using var reader = await rows.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = reader.GetString(0);
                if (traces.Any(trace => row.Contains(trace, StringComparison.OrdinalIgnoreCase)))
                {
                    found.Add($"{table}: {row}");
                }
            }
        }

        return found;
    }

    private static List<string> TracesOf(string address)
    {
        var forms = new[]
        {
            address,
            address.Trim(),
            address.Trim().ToUpperInvariant(),
            address.Trim().ToLowerInvariant()
        }.Distinct(StringComparer.Ordinal).ToList();

        var traces = new List<string>(forms);
        foreach (var form in forms)
        {
            var bytes = Encoding.UTF8.GetBytes(form);
            byte[][] digests = [SHA256.HashData(bytes), SHA512.HashData(bytes), SHA1.HashData(bytes), MD5.HashData(bytes)];
            foreach (var digest in digests)
            {
                traces.Add(Convert.ToHexString(digest));
                traces.Add(Convert.ToBase64String(digest));
            }
        }

        return traces;
    }
}
