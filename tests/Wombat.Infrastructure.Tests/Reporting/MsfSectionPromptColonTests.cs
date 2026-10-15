using FluentAssertions;
using Wombat.Infrastructure.Reporting;

namespace Wombat.Infrastructure.Tests.Reporting;

/// <summary>
/// T369 (T270): the portfolio PDF's MSF section prints a question's prompt as a label, with ": " after it only when the
/// prompt does not already end a sentence. The seed's rating prompt ends in a full stop, and printed "performance.: Avg
/// 4.0"; a comment prompt ending in a question mark printed "improve?:".
/// </summary>
public sealed class MsfSectionPromptColonTests
{
    [Theory]
    [InlineData("Rates the trainee's overall professional performance.", "Rates the trainee's overall professional performance.")]
    [InlineData("What should the trainee keep doing or improve?", "What should the trainee keep doing or improve?")]
    [InlineData("Already a label:", "Already a label:")]
    [InlineData("Clinical judgement", "Clinical judgement:")]
    [InlineData("Teaching (overall)", "Teaching (overall):")]
    [InlineData("Ends in a full stop and a space. ", "Ends in a full stop and a space.")]
    public void APrompt_GetsAColon_OnlyWhenItDoesNotEndASentence(string prompt, string printed)
        => MsfSectionComponent.Labelled(prompt).Should().Be(printed);
}
