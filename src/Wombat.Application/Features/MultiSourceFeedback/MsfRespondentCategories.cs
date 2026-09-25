using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>What each respondent group is called on a page. (T217)</summary>
/// <remarks>
/// An option is shown by its label, never by the key it stores (DESIGN.md § Form system, T191): the campaign form offered
/// "PeerDoctor" and "Ahp". The report's category headings and the portfolio PDF still print the key.
/// </remarks>
public static class MsfRespondentCategories
{
    public static string Describe(MsfRespondentCategory category)
        => category switch
        {
            MsfRespondentCategory.PeerDoctor => "Peer doctor",
            MsfRespondentCategory.Consultant => "Consultant",
            MsfRespondentCategory.Nurse => "Nurse",
            MsfRespondentCategory.Ahp => "Allied health professional",
            MsfRespondentCategory.Patient => "Patient",
            MsfRespondentCategory.Learner => "Learner",
            _ => "Other"
        };
}
