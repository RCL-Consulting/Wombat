using FluentAssertions;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T134. One answer to "does this type produce an entrustment rating, and what kind of evidence is
/// it?", replacing three that disagreed.
/// <para>
/// Two questions, and only one of them is the gate. <b>Rated</b> is what the type DECLARES — T126's
/// <c>rated_level_field</c>. <b>Category</b> is inferred from the key by a family map, and feeds
/// labels only. The gate was briefly a disjunction of the two, which meant a type could be counted
/// as rated on the strength of its NAME; a key called <c>cbd_checklist</c> would have entered a
/// committee's evidence denominator carrying no rating at all.
/// </para>
/// </summary>
public sealed class RatedActivityTypesTests
{
    private const string DeclaresRating = """
        {
          "version": 1,
          "rated_level_field": "overall_level",
          "sections": [
            {
              "key": "assessment",
              "title": "Assessment",
              "fields": [
                { "key": "overall_level", "type": "scale", "label": "Overall", "options": ["1", "2"], "scale_key": "O-R Scale" }
              ]
            }
          ]
        }
        """;

    private const string DeclaresNoRating = """
        {
          "version": 1,
          "sections": [
            {
              "key": "reflection",
              "title": "Reflection",
              "fields": [
                { "key": "what_i_learned", "type": "longtext", "label": "What I learned" }
              ]
            }
          ]
        }
        """;

    /// <summary>
    /// The family map classifies; it no longer decides what is rated. A known key with no declared
    /// pointer is categorised and is NOT rated — which is the whole change: rated-ness is something a
    /// type states about itself, not something inferred from what it is called.
    /// </summary>
    [Theory]
    [InlineData("mini_cex", WbaEvidenceSource.DirectObservation)]
    [InlineData("dops", WbaEvidenceSource.DirectObservation)]
    [InlineData("direct_observation", WbaEvidenceSource.DirectObservation)]
    [InlineData("cbd", WbaEvidenceSource.Conversation)]
    [InlineData("acat", WbaEvidenceSource.Conversation)]
    [InlineData("cca", WbaEvidenceSource.CaseAnalysis)]
    [InlineData("rca", WbaEvidenceSource.CaseAnalysis)]
    public void AKnownFamilyIsCategorisedButNotRatedOnItsKeyAlone(string key, WbaEvidenceSource expected)
    {
        var verdict = RatedActivityTypes.Classify(key, schemaJson: null);

        verdict.Category.Should().Be(expected);
        verdict.IsRated.Should().BeFalse("a key is a name, not a declaration");
    }

    [Theory]
    [InlineData("mini_cex_cpsa", WbaEvidenceSource.DirectObservation)]
    [InlineData("dops_cpsa", WbaEvidenceSource.DirectObservation)]
    [InlineData("cbd_cpsa", WbaEvidenceSource.Conversation)]
    [InlineData("direct_observation_cpsa", WbaEvidenceSource.DirectObservation)]
    public void TheSuffixedFormOfAFamilyIsCategorisedToo(string key, WbaEvidenceSource expected)
    {
        RatedActivityTypes.Classify(key, schemaJson: null).Category.Should().Be(expected);
    }

    /// <summary>
    /// D4 made "Case note review" an alias of CCA and D12 made "Directly observed clinical examination"
    /// the same instrument as Mini-CEX, so neither will ever be seeded and neither has a category.
    /// </summary>
    [Theory]
    [InlineData("case_note_review")]
    [InlineData("observed_clinical_exam")]
    public void InstrumentsTheCollegeRetiredHaveNoCategory(string key)
    {
        RatedActivityTypes.Classify(key, schemaJson: null).Category.Should().BeNull();
    }

    /// <summary>
    /// Filed under Conversation, arguably Case analysis. Pinned rather than corrected: re-filing it is
    /// [T122]'s DECISION 2 and belongs to the College, not to this refactor.
    /// </summary>
    [Fact]
    public void ChartStimulatedRecallStaysUnderConversation()
    {
        RatedActivityTypes.Classify("chart_stimulated_recall", null).Category
            .Should().Be(WbaEvidenceSource.Conversation);
    }

    [Fact]
    public void TheThreeLabelsAreTheStringsTheChartAlreadyPrints()
    {
        // Byte-identical to what GetEpaTrajectoryForTraineeQuery emitted, so no tooltip or
        // accessibility table changes wording.
        WbaEvidenceSource.DirectObservation.Label().Should().Be("Direct observation");
        WbaEvidenceSource.Conversation.Label().Should().Be("Conversation");
        WbaEvidenceSource.CaseAnalysis.Label().Should().Be("Case analysis");
    }

    /// <summary>
    /// The declaration is the gate, whatever the type is called. This is what closed the trajectory's
    /// KNOWN LIMITATION: an institution's own rated tool charts because it says it is rated, not
    /// because a hard-coded list has heard of its name. Its evidence reads as its own key until
    /// [T122] gives it a tool key to classify by.
    /// </summary>
    [Fact]
    public void AnUnfamiliarKeyThatDeclaresARatingIsRated()
    {
        var verdict = RatedActivityTypes.Classify("ward_round_review", DeclaresRating);

        verdict.IsRated.Should().BeTrue();
        verdict.Category.Should().BeNull();
        verdict.SourceBucket.Should().Be("ward_round_review");
    }

    /// <summary>
    /// And a KNOWN family that declares a rating is rated for that reason, not for its name.
    /// </summary>
    [Fact]
    public void AKnownFamilyThatDeclaresARatingIsRatedAndCategorised()
    {
        var verdict = RatedActivityTypes.Classify("mini_cex_cpsa", DeclaresRating);

        verdict.IsRated.Should().BeTrue();
        verdict.Category.Should().Be(WbaEvidenceSource.DirectObservation);
        verdict.SourceBucket.Should().Be("Direct observation");
    }

    [Fact]
    public void AnUnfamiliarKeyThatDeclaresNoRatingIsNotRated()
    {
        RatedActivityTypes.Classify("ward_round_review", DeclaresNoRating).IsRated.Should().BeFalse();
        RatedActivityTypes.Classify("ward_round_review", schemaJson: null).IsRated.Should().BeFalse();
    }

    [Theory]
    [InlineData("reflective_note")]
    [InlineData("journal_club")]
    [InlineData("procedure_log")]
    [InlineData("qi_project")]
    [InlineData("research_output")]
    [InlineData("teaching_session")]
    public void TheSixUnratedSeedsAreNotRated(string key)
    {
        RatedActivityTypes.Classify(key, DeclaresNoRating).IsRated.Should().BeFalse();
    }

    /// <summary>
    /// A stored schema that no longer parses declares nothing, so it rates nothing — whatever the type
    /// is called. The family arm used to rescue it; that was compatibility, not correctness. A type
    /// whose schema will not parse is a defect to fix, not evidence to count.
    /// </summary>
    [Fact]
    public void AnUnparseableSchemaIsNotRatedEvenUnderAKnownFamily()
    {
        var verdict = RatedActivityTypes.Classify("mini_cex_cpsa", "{ not json at all");

        verdict.IsRated.Should().BeFalse();
        verdict.Category.Should().Be(WbaEvidenceSource.DirectObservation, "the key still classifies");
    }

    [Fact]
    public void AnUnparseableSchemaUnderAnUnknownKeyIsNotRated()
    {
        RatedActivityTypes.Classify("ward_round_review", "{ not json at all").IsRated.Should().BeFalse();
    }

    /// <summary>
    /// A near-miss must not match: the prefix rule is "&lt;family&gt;_", not "starts with".
    /// </summary>
    [Fact]
    public void APrefixWithoutTheUnderscoreIsNotAMatch()
    {
        RatedActivityTypes.Classify("dopsomething", null).Category.Should().BeNull();
        RatedActivityTypes.Classify("cbdx", null).Category.Should().BeNull();
    }

    [Fact]
    public void AnEmptyOrNullKeyIsNotRated()
    {
        RatedActivityTypes.Classify(null, null).IsRated.Should().BeFalse();
        RatedActivityTypes.Classify("   ", null).IsRated.Should().BeFalse();
    }
}
