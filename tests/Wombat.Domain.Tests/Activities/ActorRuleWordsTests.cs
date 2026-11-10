using Wombat.Domain.Activities.Workflow;

namespace Wombat.Domain.Tests.Activities;

/// <summary>
/// T398 (flow 17, Q2, S9; R3-0-spec-2 § 3's ActorRuleWords row): the actor grammar in words, as the builder's read of a
/// workflow says who moves each move; anything it cannot word is the rule as written, flagged.
/// </summary>
public sealed class ActorRuleWordsTests
{
    private static readonly Dictionary<string, string> Labels = new(StringComparer.Ordinal)
    {
        ["assessor_user_id"] = "Assessor",
        ["supervisor"] = "Supervisor"
    };

    [Theory]
    [InlineData("subject", "The registrar")]
    [InlineData("creator", "Whoever filed it")]
    [InlineData("subject|creator", "The registrar, or whoever filed it")]
    [InlineData("field:assessor_user_id", "The person Assessor names")]
    [InlineData("subject|field:assessor_user_id", "The registrar, or the person Assessor names")]
    [InlineData("role:Coordinator|role:Administrator", "A Coordinator or an Administrator")]
    [InlineData("role:Coordinator|role:Administrator|role:CommitteeMember", "A Coordinator, an Administrator or a Committee member")]
    [InlineData("role:Assessor", "An Assessor")]
    [InlineData("role:InstitutionalAdmin", "An Institutional admin")]
    [InlineData("role:SpecialityAdmin+scope:speciality", "A Speciality admin of the activity's speciality")]
    [InlineData("role:SubSpecialityAdmin+scope:subspeciality", "A Sub-speciality admin of the activity's sub-speciality")]
    [InlineData("field:assessor_user_id+role:Assessor", "The person Assessor names, who is also an Assessor")]
    [InlineData("subject|role:SpecialityAdmin+scope:speciality", "The registrar, or a Speciality admin of the activity's speciality")]
    public void Describe_TheGrammar_InWords(string rule, string words)
    {
        var (described, worded) = ActorRuleWords.Describe(ActorRuleParser.Parse(rule), Label);

        Assert.True(worded);
        Assert.Equal(words, described);
    }

    /// <summary>What it cannot word is the rule as written, so the page shows it as code rather than guess.</summary>
    [Theory]
    [InlineData("field:ghost")]
    [InlineData("role:Wizard")]
    [InlineData("scope:institution")]
    [InlineData("role:Assessor+scope:institution")]
    [InlineData("scope:speciality")]
    [InlineData("subject|field:ghost")]
    [InlineData("role:Assessor+scope:speciality+scope:subspeciality")]
    public void Describe_WhatItCannotWord_IsTheRuleAsWritten(string rule)
    {
        var (described, worded) = ActorRuleWords.Describe(ActorRuleParser.Parse(rule), Label);

        Assert.False(worded);
        Assert.Equal(rule, described);
    }

    [Theory]
    [InlineData(TransitionValidation.All, "The whole form")]
    [InlineData(TransitionValidation.Owned, "The mover's own fields")]
    [InlineData(TransitionValidation.Draft, "Formats only")]
    public void Validation_InWords(TransitionValidation validation, string words)
        => Assert.Equal(words, ActorRuleWords.Validation(validation));

    private static string? Label(string key) => Labels.TryGetValue(key, out var label) ? label : null;
}
