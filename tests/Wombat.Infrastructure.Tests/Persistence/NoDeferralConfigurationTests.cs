using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// T361 (D53): the agenda line carries no deferral, and the decision carries its one sentence about the EPAs it did not
/// decide, as long as Conditions may be. The model is read from the PostgreSQL provider's configuration without a
/// connection; <c>CommitteeAgendaPostgresTests</c> holds the migration to a real server.
/// </summary>
public sealed class NoDeferralConfigurationTests
{
    private static readonly IModel Model = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=unused")
            .Options)
        .GetService<IDesignTimeModel>().Model;

    [Fact]
    public void AnAgendaLine_HasNoDeferralReason_NorTheDeferralsCheck()
    {
        var line = Model.FindEntityType(typeof(CommitteeAgendaLine))!;

        line.FindProperty("DeferralReason").Should().BeNull();
        line.GetCheckConstraints().Select(check => check.Name)
            .Should().BeEquivalentTo("CK_CommitteeAgendaLines_WindowSemester", "CK_CommitteeAgendaLines_DecidedNamesItsStar");
    }

    [Fact]
    public void ADecision_HoldsItsSentenceAboutTheEpasNotDecided_AsLongAsItsConditions()
    {
        var decision = Model.FindEntityType(typeof(CommitteeDecision))!;
        var note = decision.FindProperty(nameof(CommitteeDecision.UndecidedNote))!;

        note.IsNullable.Should().BeTrue("the sentence is optional");
        note.GetMaxLength().Should().Be(CommitteeDecision.ConditionsMaxLength)
            .And.Be(decision.FindProperty(nameof(CommitteeDecision.Conditions))!.GetMaxLength());
    }
}
