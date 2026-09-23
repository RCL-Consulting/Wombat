using FluentAssertions;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Infrastructure.Activities;

namespace Wombat.Infrastructure.Tests.Activities;

/// <summary>
/// The encounter date an activity asserts (T119), and the calendar its fallback uses (T130).
/// </summary>
public sealed class ObservationDateResolverTests
{
    [Fact]
    public void AnUndatedActivityFiledJustAfterMidnightInSouthAfrica_IsDatedTheSouthAfricanDay_NotTheUtcOne()
    {
        // 23:30 UTC on 30 June is 01:30 on 1 July in South Africa. The progress page reads "today" in South Africa,
        // so with a UTC fallback this encounter would land in semester 1 while the page was already showing
        // semester 2, and it would be missing from "this semester" for the two hours either side of every boundary.
        var activity = new Activity { CreatedOn = new DateTime(2026, 6, 30, 23, 30, 0, DateTimeKind.Utc) };

        var (observedOn, source) = ObservationDateResolver.Resolve(activity, SchemaWithoutADateField(), "{}");

        observedOn.Should().Be(new DateOnly(2026, 7, 1));
        source.Should().Be(ObservationDateSource.CreatedOn);
    }

    [Fact]
    public void ADeclaredDateIsTakenAsTyped_WhateverTheFilingTime()
    {
        var activity = new Activity { CreatedOn = new DateTime(2026, 6, 30, 23, 30, 0, DateTimeKind.Utc) };

        var (observedOn, source) = ObservationDateResolver.Resolve(
            activity, SchemaWithADateField(), """{ "observed_on": "2026-06-30" }""");

        observedOn.Should().Be(new DateOnly(2026, 6, 30));
        source.Should().Be(ObservationDateSource.Declared);
    }

    private static FormSchema SchemaWithoutADateField() => FormSchemaParser.Parse("""
        { "version": 1, "sections": [ { "key": "s", "title": "S", "fields": [ { "key": "note", "type": "text", "label": "Note" } ] } ] }
        """);

    private static FormSchema SchemaWithADateField() => FormSchemaParser.Parse("""
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "sections": [ { "key": "s", "title": "S", "fields": [ { "key": "observed_on", "type": "date", "label": "Observed on" } ] } ]
        }
        """);
}
