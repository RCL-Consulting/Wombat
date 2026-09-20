using FluentAssertions;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T134. One answer to "does this type produce an entrustment rating, and what kind of evidence is
/// it?", replacing three that disagreed. The gate is a disjunction — declared (T126's
/// <c>rated_level_field</c>) OR a known family — and both arms are load-bearing today.
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

    [Theory]
    [InlineData("mini_cex", WbaEvidenceSource.DirectObservation)]
    [InlineData("dops", WbaEvidenceSource.DirectObservation)]
    [InlineData("direct_observation", WbaEvidenceSource.DirectObservation)]
    [InlineData("cbd", WbaEvidenceSource.Conversation)]
    [InlineData("acat", WbaEvidenceSource.Conversation)]
    [InlineData("cca", WbaEvidenceSource.CaseAnalysis)]
    [InlineData("rca", WbaEvidenceSource.CaseAnalysis)]
    public void AKnownFamilyIsRatedByItsKeyAlone(string key, WbaEvidenceSource expected)
    {
        // No schema at all: this is the arm that keeps the operator-built *_paed types counted, since
        // they are in no seeder and can never be given a pointer while T133 stands.
        var verdict = RatedActivityTypes.Classify(key, schemaJson: null);

        verdict.IsRated.Should().BeTrue();
        verdict.Category.Should().Be(expected);
    }

    [Theory]
    [InlineData("mini_cex_cpsa", WbaEvidenceSource.DirectObservation)]
    [InlineData("dops_cpsa", WbaEvidenceSource.DirectObservation)]
    [InlineData("cbd_cpsa", WbaEvidenceSource.Conversation)]
    [InlineData("direct_observation_cpsa", WbaEvidenceSource.DirectObservation)]
    [InlineData("mini_cex_paed", WbaEvidenceSource.DirectObservation)]
    public void TheSuffixedFormOfAFamilyMatchesToo(string key, WbaEvidenceSource expected)
    {
        // The defect T134 fixed: these matched nothing under an exact-key test, so a v11.1 trainee's
        // committee sampling report said there was no rated evidence at all.
        RatedActivityTypes.Classify(key, schemaJson: null).Category.Should().Be(expected);
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
    /// The declaration arm on its own: a tool an institution built under a name no family knows, which
    /// declares a rating. It counts, and its own key becomes its source bucket so two of them count as
    /// one source rather than as none.
    /// </summary>
    [Fact]
    public void AnUnfamiliarKeyThatDeclaresARatingIsRated()
    {
        var verdict = RatedActivityTypes.Classify("ward_round_review", DeclaresRating);

        verdict.IsRated.Should().BeTrue();
        verdict.Category.Should().BeNull();
        verdict.SourceBucket.Should().Be("ward_round_review");
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
    /// A stored schema that no longer parses is a real defect, but it is not a committee report's to
    /// raise. The family arm still answers, which is what makes this fix robust to the state of any
    /// particular database.
    /// </summary>
    [Fact]
    public void AnUnparseableSchemaUnderAKnownFamilyIsStillRated()
    {
        var verdict = RatedActivityTypes.Classify("mini_cex_cpsa", "{ not json at all");

        verdict.IsRated.Should().BeTrue();
        verdict.Category.Should().Be(WbaEvidenceSource.DirectObservation);
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
        RatedActivityTypes.Classify("dopsomething", null).IsRated.Should().BeFalse();
        RatedActivityTypes.Classify("cbdx", null).IsRated.Should().BeFalse();
    }

    [Fact]
    public void AnEmptyOrNullKeyIsNotRated()
    {
        RatedActivityTypes.Classify(null, null).IsRated.Should().BeFalse();
        RatedActivityTypes.Classify("   ", null).IsRated.Should().BeFalse();
    }
}
