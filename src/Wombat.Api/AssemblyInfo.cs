using System.Runtime.CompilerServices;

// T202 review. MsfRespondEndpoint.Describe maps every refusal a respondent can meet to a 4xx; the integration suite asks
// it of every value, so a refusal added later without an answer fails a test instead of answering 500.
[assembly: InternalsVisibleTo("Wombat.Integration.Tests")]
