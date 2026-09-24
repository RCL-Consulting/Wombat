using System.Security.Cryptography;
using System.Text;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Tests.MultiSourceFeedback;

/// <summary>
/// Every form a respondent's address could be kept in without a key: the address itself, and its unsalted digests.
/// (T207)
/// </summary>
/// <remarks>
/// Until T207 an anonymised invitation kept an unsalted SHA-256 of the upper-cased address, which anyone holding the
/// invited addresses could recompute and match to its row. So the check is not "no column named for a hash" but "no
/// value anyone could compute from the address alone": each common digest, of each normalisation a writer would pick,
/// as hex (either case, compared ignoring case) and as base64.
/// </remarks>
internal static class RespondentAddressTraces
{
    public static IReadOnlyList<string> Of(string address)
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

    /// <summary>Asserts that no string the invitation holds contains the address or any unsalted digest of it.</summary>
    public static void AssertNoneKept(MsfInvitation invitation, string address)
    {
        var stored = typeof(MsfInvitation).GetProperties()
            .Where(property => property.PropertyType == typeof(string) && property.CanRead)
            .Select(property => (Name: property.Name, Value: (string?)property.GetValue(invitation)))
            .Where(pair => pair.Value is not null)
            .ToList();

        foreach (var trace in Of(address))
        {
            foreach (var (name, value) in stored)
            {
                Assert.False(
                    value!.Contains(trace, StringComparison.OrdinalIgnoreCase),
                    $"{name} keeps a trace of {address}: {value}");
            }
        }
    }
}
