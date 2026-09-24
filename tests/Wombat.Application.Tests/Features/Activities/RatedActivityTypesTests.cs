using FluentAssertions;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T134. One answer to "does this type produce an entrustment rating, and what kind of evidence is
/// it?", replacing three that disagreed.
/// <para>
/// Two questions, and only one of them is the gate. <b>Rated</b> is what the type DECLARES — T126's
/// <c>rated_level_field</c>. <b>Category</b> feeds labels only. The gate was briefly a disjunction of
/// the two, which meant a type could be counted as rated on the strength of its NAME; a key called
/// <c>cbd_checklist</c> would have entered a committee's evidence denominator carrying no rating at all.
/// </para>
/// <para>
/// T144. The category is read from the instrument the type declares (<c>WbaToolKey</c>) and guessed
/// from the type's own key only when it declares none.
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
    /// The verification line of T144: an institution builds its own Mini-CEX in the builder, calls it
    /// something no list has heard of, and picks "Mini-CEX" as its instrument. It is Direct observation
    /// on the chart and on the sampling report, not a source of its own called <c>ward_round_review</c>.
    /// </summary>
    [Fact]
    public void ABuilderTypeKeyedMiniCexUnderAnUnfamiliarKeyIsDirectObservation()
    {
        var verdict = RatedActivityTypes.Classify("ward_round_review", wbaToolKey: "mini_cex", DeclaresRating);

        verdict.IsRated.Should().BeTrue();
        verdict.Category.Should().Be(WbaEvidenceSource.DirectObservation);
        verdict.SourceBucket.Should().Be("Direct observation");
    }

    /// <summary>
    /// Each categorised instrument of the College's vocabulary, under a key that matches no family, so
    /// only the tool key can be what classified it.
    /// </summary>
    [Theory]
    [InlineData("mini_cex", WbaEvidenceSource.DirectObservation)]
    [InlineData("dops", WbaEvidenceSource.DirectObservation)]
    [InlineData("direct_observation", WbaEvidenceSource.DirectObservation)]
    [InlineData("cbd", WbaEvidenceSource.Conversation)]
    [InlineData("chart_stimulated_recall", WbaEvidenceSource.Conversation)]
    [InlineData("cca", WbaEvidenceSource.CaseAnalysis)]
    [InlineData("rca", WbaEvidenceSource.CaseAnalysis)]
    public void AnInstrumentIsCategorisedByItsToolKey(string wbaToolKey, WbaEvidenceSource expected)
    {
        RatedActivityTypes.Classify("ward_round_review", wbaToolKey, DeclaresRating).Category
            .Should().Be(expected);
    }

    /// <summary>
    /// The instruments that are no category of rated evidence. A type keyed with one has no category even
    /// under a key that looks like a family: the declaration answers, and the name is not asked.
    /// </summary>
    [Theory]
    [InlineData("msf")]
    [InlineData("reflective_exercise")]
    [InlineData("clinical_audit")]
    [InlineData("portfolio_review")]
    [InlineData("learner_feedback")]
    public void AnInstrumentWithNoEvidenceCategoryHasNoneWhateverTheTypeIsCalled(string wbaToolKey)
    {
        var verdict = RatedActivityTypes.Classify("mini_cex_feedback", wbaToolKey, DeclaresRating);

        verdict.Category.Should().BeNull();
        verdict.SourceBucket.Should().Be("mini_cex_feedback", "an uncategorised type is still one source, its own");
    }

    /// <summary>
    /// The tool key outranks the type key when they disagree.
    /// </summary>
    [Fact]
    public void TheToolKeyWinsOverAFamilyLookingTypeKey()
    {
        RatedActivityTypes.Classify("cbd_ward_round", wbaToolKey: "mini_cex", DeclaresRating).Category
            .Should().Be(WbaEvidenceSource.DirectObservation);
    }

    /// <summary>
    /// A key that is not in the vocabulary is still a declaration, and the family match is only for a type
    /// that declares nothing. Guessing from the name here would let the name overrule what the type says.
    /// </summary>
    [Fact]
    public void AnUnknownToolKeyDoesNotFallBackToTheTypeKey()
    {
        var verdict = RatedActivityTypes.Classify("mini_cex_paed", wbaToolKey: "something_new", DeclaresRating);

        verdict.Category.Should().BeNull();
        verdict.SourceBucket.Should().Be("mini_cex_paed");
    }

    /// <summary>
    /// Read through <c>WbaTool.NormalizeKey</c>, as every other reader of the key is; a blank key is no key.
    /// </summary>
    [Fact]
    public void TheToolKeyIsNormalisedAndABlankOneIsNoKey()
    {
        RatedActivityTypes.Classify("ward_round_review", " Mini_CEX ", DeclaresRating).Category
            .Should().Be(WbaEvidenceSource.DirectObservation);
        RatedActivityTypes.Classify("dops_paed", "   ", DeclaresRating).Category
            .Should().Be(WbaEvidenceSource.DirectObservation, "a blank key is unkeyed, so the family match applies");
    }

    /// <summary>
    /// An unkeyed type falls back to the family match, which is what an operator-built type that never
    /// picked an instrument, and the seeded ACAT, still rely on. A known key with no declared pointer is
    /// categorised and is NOT rated: rated-ness is something a type states about itself.
    /// </summary>
    [Theory]
    [InlineData("mini_cex", WbaEvidenceSource.DirectObservation)]
    [InlineData("dops", WbaEvidenceSource.DirectObservation)]
    [InlineData("direct_observation", WbaEvidenceSource.DirectObservation)]
    [InlineData("cbd", WbaEvidenceSource.Conversation)]
    [InlineData("acat", WbaEvidenceSource.Conversation)]
    [InlineData("cca", WbaEvidenceSource.CaseAnalysis)]
    [InlineData("rca", WbaEvidenceSource.CaseAnalysis)]
    public void AnUnkeyedKnownFamilyIsCategorisedButNotRatedOnItsKeyAlone(string key, WbaEvidenceSource expected)
    {
        var verdict = RatedActivityTypes.Classify(key, wbaToolKey: null, schemaJson: null);

        verdict.Category.Should().Be(expected);
        verdict.IsRated.Should().BeFalse("a key is a name, not a declaration");
    }

    [Theory]
    [InlineData("mini_cex_paed", WbaEvidenceSource.DirectObservation)]
    [InlineData("dops_paed", WbaEvidenceSource.DirectObservation)]
    [InlineData("cbd_paed", WbaEvidenceSource.Conversation)]
    [InlineData("acat_paed", WbaEvidenceSource.Conversation)]
    [InlineData("direct_observation_paed", WbaEvidenceSource.DirectObservation)]
    [InlineData("cca_paed", WbaEvidenceSource.CaseAnalysis)]
    [InlineData("rca_paed", WbaEvidenceSource.CaseAnalysis)]
    [InlineData("chart_stimulated_recall_paed", WbaEvidenceSource.Conversation)]
    public void TheSuffixedFormOfAnUnkeyedFamilyIsCategorisedToo(string key, WbaEvidenceSource expected)
    {
        RatedActivityTypes.Classify(key, wbaToolKey: null, schemaJson: null).Category.Should().Be(expected);
    }

    /// <summary>
    /// The instruments with no evidence category are not families: an unkeyed type called <c>msf_*</c> gets
    /// no category from its name either.
    /// </summary>
    [Theory]
    [InlineData("msf_paed")]
    [InlineData("reflective_exercise_paed")]
    public void AnUnkeyedTypeNamedLikeAnUncategorisedInstrumentHasNoCategory(string key)
    {
        RatedActivityTypes.Classify(key, wbaToolKey: null, schemaJson: null).Category.Should().BeNull();
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
        RatedActivityTypes.Classify(key, wbaToolKey: null, schemaJson: null).Category.Should().BeNull();
        RatedActivityTypes.Classify("ward_round_review", key, schemaJson: null).Category.Should().BeNull();
    }

    /// <summary>
    /// Filed under Conversation, arguably Case analysis. Pinned rather than corrected: re-filing it belongs
    /// to the College. The instrument and the family agree, so it charts the same keyed or not.
    /// </summary>
    [Fact]
    public void ChartStimulatedRecallStaysUnderConversation()
    {
        RatedActivityTypes.Classify("ward_round_review", "chart_stimulated_recall", null).Category
            .Should().Be(WbaEvidenceSource.Conversation);
        RatedActivityTypes.Classify("chart_stimulated_recall", wbaToolKey: null, schemaJson: null).Category
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
    /// because a hard-coded list has heard of its name. Declaring no instrument, its evidence reads as its
    /// own key.
    /// </summary>
    [Fact]
    public void AnUnfamiliarUnkeyedTypeThatDeclaresARatingIsRated()
    {
        var verdict = RatedActivityTypes.Classify("ward_round_review", wbaToolKey: null, DeclaresRating);

        verdict.IsRated.Should().BeTrue();
        verdict.Category.Should().BeNull();
        verdict.SourceBucket.Should().Be("ward_round_review");
    }

    /// <summary>
    /// And a keyed instrument that declares a rating is rated for that reason, not for its name.
    /// </summary>
    [Fact]
    public void AKeyedInstrumentThatDeclaresARatingIsRatedAndCategorised()
    {
        var verdict = RatedActivityTypes.Classify("mini_cex_cpsa", "mini_cex", DeclaresRating);

        verdict.IsRated.Should().BeTrue();
        verdict.Category.Should().Be(WbaEvidenceSource.DirectObservation);
        verdict.SourceBucket.Should().Be("Direct observation");
    }

    [Fact]
    public void AnUnfamiliarKeyThatDeclaresNoRatingIsNotRated()
    {
        RatedActivityTypes.Classify("ward_round_review", wbaToolKey: null, DeclaresNoRating).IsRated.Should().BeFalse();
        RatedActivityTypes.Classify("ward_round_review", wbaToolKey: null, schemaJson: null).IsRated.Should().BeFalse();
        RatedActivityTypes.Classify("ward_round_review", "mini_cex", DeclaresNoRating).IsRated
            .Should().BeFalse("an instrument is a category, not a rating");
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
        RatedActivityTypes.Classify(key, wbaToolKey: null, DeclaresNoRating).IsRated.Should().BeFalse();
    }

    /// <summary>
    /// A stored schema that no longer parses declares nothing, so it rates nothing — whatever the type
    /// is called. The family arm used to rescue it; that was compatibility, not correctness. A type
    /// whose schema will not parse is a defect to fix, not evidence to count.
    /// </summary>
    [Fact]
    public void AnUnparseableSchemaIsNotRatedEvenUnderAKnownInstrument()
    {
        var verdict = RatedActivityTypes.Classify("mini_cex_cpsa", "mini_cex", "{ not json at all");

        verdict.IsRated.Should().BeFalse();
        verdict.Category.Should().Be(WbaEvidenceSource.DirectObservation, "the instrument still classifies");
    }

    [Fact]
    public void AnUnparseableSchemaUnderAnUnknownKeyIsNotRated()
    {
        RatedActivityTypes.Classify("ward_round_review", wbaToolKey: null, "{ not json at all").IsRated.Should().BeFalse();
    }

    /// <summary>
    /// A near-miss must not match: the prefix rule is "&lt;family&gt;_", not "starts with".
    /// </summary>
    [Fact]
    public void APrefixWithoutTheUnderscoreIsNotAMatch()
    {
        RatedActivityTypes.Classify("dopsomething", wbaToolKey: null, schemaJson: null).Category.Should().BeNull();
        RatedActivityTypes.Classify("cbdx", wbaToolKey: null, schemaJson: null).Category.Should().BeNull();
    }

    [Fact]
    public void AnEmptyOrNullKeyIsNotRated()
    {
        RatedActivityTypes.Classify(null, null, null).IsRated.Should().BeFalse();
        RatedActivityTypes.Classify("   ", null, null).IsRated.Should().BeFalse();
    }
}
