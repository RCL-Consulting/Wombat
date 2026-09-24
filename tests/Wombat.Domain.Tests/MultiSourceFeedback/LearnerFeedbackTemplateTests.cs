using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Domain.Tests.MultiSourceFeedback;

/// <summary>
/// Learner feedback as an MSF template kind (T164, D35): who answers which questionnaire, and how the teaching contexts
/// its learners answered from are counted.
/// </summary>
public sealed class LearnerFeedbackTemplateTests
{
    /// <summary>
    /// Both enums are stored as their numbers, so a value is appended and nothing is renumbered: a renumbered category
    /// would re-label every invitation already stored.
    /// </summary>
    [Fact]
    public void TheStoredNumbersAreAppendedToNeverRenumbered()
    {
        Assert.Equal([0, 1, 2, 3, 4, 5, 6], Enum.GetValues<MsfRespondentCategory>().Select(category => (int)category));
        Assert.Equal(4, (int)MsfRespondentCategory.Patient);
        Assert.Equal(5, (int)MsfRespondentCategory.Other);
        Assert.Equal(6, (int)MsfRespondentCategory.Learner);

        // Every template stored before T164 is multi-source feedback.
        Assert.Equal(0, (int)MsfTemplateKind.Msf);
        Assert.Equal(1, (int)MsfTemplateKind.LearnerFeedback);
        Assert.Equal(MsfTemplateKind.Msf, new MsfTemplate().Kind);
    }

    [Fact]
    public void ALearnerFeedbackTemplate_AcceptsLearnersAndNobodyElse()
    {
        var template = new MsfTemplate { Kind = MsfTemplateKind.LearnerFeedback, AllowPatientResponses = true };

        Assert.Equal([MsfRespondentCategory.Learner], template.AcceptedCategories());
    }

    [Fact]
    public void AnMsfTemplate_AcceptsEveryoneButALearner_AndAPatientOnlyWhereAllowed()
    {
        Assert.Equal(
            [
                MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.Consultant, MsfRespondentCategory.Nurse,
                MsfRespondentCategory.Ahp, MsfRespondentCategory.Patient, MsfRespondentCategory.Other
            ],
            new MsfTemplate { Kind = MsfTemplateKind.Msf, AllowPatientResponses = true }.AcceptedCategories());

        Assert.Equal(
            [
                MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.Consultant, MsfRespondentCategory.Nurse,
                MsfRespondentCategory.Ahp, MsfRespondentCategory.Other
            ],
            new MsfTemplate { Kind = MsfTemplateKind.Msf, AllowPatientResponses = false }.AcceptedCategories());
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("  Ward   round ", "Ward round")]
    [InlineData("Student\ttutorial", "Student tutorial")]
    public void AContext_IsStoredTrimmed_WithEachRunOfWhitespaceOneSpace(string? typed, string? stored)
        => Assert.Equal(stored, MsfTeachingContexts.Normalize(typed));

    [Fact]
    public void TwoSpellingsOfOneContext_CountOnce_AndBlanksCountAsNone()
    {
        Assert.Equal(
            ["Student tutorial", "WARD ROUND"],
            MsfTeachingContexts.Distinct(["ward round", "Ward  round", "Student tutorial", null, "  ", "WARD ROUND"]));

        // The answer does not depend on the order the responses were read in.
        Assert.Equal(
            MsfTeachingContexts.Distinct(["ward round", "Ward round"]),
            MsfTeachingContexts.Distinct(["Ward round", "ward round"]));
    }

    [Fact]
    public void TheContextsAnsweredFrom_AreTheRespondingLearners_NotEveryoneInvited()
    {
        var campaign = new MsfCampaign();
        Respond(campaign, "Ward round");
        Respond(campaign, "Student tutorial");
        campaign.Invitations.Add(new MsfInvitation { RespondentCategory = MsfRespondentCategory.Learner, TeachingContext = "Clinic" });

        Assert.Equal(["Student tutorial", "Ward round"], campaign.RespondedTeachingContexts());
    }

    [Fact]
    public void AResponseWhoseInvitationWasNotLoaded_IsRefused_NotCountedAsNoContext()
    {
        var campaign = new MsfCampaign();
        campaign.Responses.Add(new MsfResponse { Invitation = null! });

        var refusal = Assert.Throws<InvalidOperationException>(() => campaign.RespondedTeachingContexts());

        Assert.Contains("invitation must be loaded", refusal.Message, StringComparison.Ordinal);
    }

    private static void Respond(MsfCampaign campaign, string context)
    {
        var invitation = new MsfInvitation { RespondentCategory = MsfRespondentCategory.Learner, TeachingContext = context };
        var response = new MsfResponse { Invitation = invitation };
        invitation.Responses.Add(response);
        campaign.Invitations.Add(invitation);
        campaign.Responses.Add(response);
    }
}
