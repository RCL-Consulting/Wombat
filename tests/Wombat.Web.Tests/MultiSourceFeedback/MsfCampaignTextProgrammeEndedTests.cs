using System.Globalization;
using FluentAssertions;
using Wombat.Web.Components.Pages.MultiSourceFeedback;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// T369 (flow 10, build-contracts § 0; K14, R10): the sentence the campaign page and the report show once the registrar's
/// programme has ended. It names her each time, never a pronoun, and writes the day in ISO whatever the culture.
/// </summary>
public sealed class MsfCampaignTextProgrammeEndedTests
{
    [Fact]
    public void TheSentence()
    {
        MsfCampaignText.ProgrammeEnded("Lerato Molefe", new DateOnly(2026, 10, 20)).Should().Be(
            "Lerato Molefe's programme ended on 2026-10-20. A release still records the feedback as evidence on Lerato " +
            "Molefe's record, but Lerato Molefe can no longer open MSF reports to read it.");
    }

    [Fact]
    public void TheDay_IsIso_WhateverTheCulture()
    {
        var before = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ar-SA");
            MsfCampaignText.ProgrammeEnded("Lerato Molefe", new DateOnly(2026, 10, 20)).Should().Contain("ended on 2026-10-20.");
        }
        finally
        {
            CultureInfo.CurrentCulture = before;
        }
    }
}
