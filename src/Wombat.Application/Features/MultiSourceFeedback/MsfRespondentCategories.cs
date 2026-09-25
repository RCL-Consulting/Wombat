using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>What each respondent group is called on a page and in the portfolio. (T217, T225)</summary>
/// <remarks>
/// An option is shown by its label, never by the key it stores (DESIGN.md § Form system, T191): the campaign form offered
/// "PeerDoctor" and "Ahp". The report's category headings, the trainee's copy of it and the portfolio PDF printed the key
/// until T225.
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
