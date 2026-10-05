using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Persistence;

/// <summary>
/// T358 (flow 06, lane A0): the reminder's table as the model maps it, and the admission day. The model is read from the
/// PostgreSQL provider's configuration without a connection; <c>ActivityReminderPostgresTests</c> holds the index to a
/// real server.
/// </summary>
public sealed class ActivityReminderConfigurationTests
{
    private static readonly IModel Model = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=unused")
            .Options)
        .GetService<IDesignTimeModel>().Model;

    [Fact]
    public void TheReminder_IsItsOwnTable_WithTheSameDayBlockAsAUniqueIndex()
    {
        var reminder = Model.FindEntityType(typeof(ActivityReminder))!;

        reminder.GetTableName().Should().Be("ActivityReminders");

        var sameDay = reminder.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(ActivityReminder.ActivityId), nameof(ActivityReminder.SentOnDay)]));
        sameDay.IsUnique.Should().BeTrue("one reminder per request per South African day, whoever sends it");

        reminder.GetIndexes().Should().Contain(index =>
            !index.IsUnique &&
            index.Properties.Select(property => property.Name).SequenceEqual(new[] { nameof(ActivityReminder.ActivityId), nameof(ActivityReminder.SentOn) }));

        reminder.FindProperty(nameof(ActivityReminder.SentByUserId))!.GetMaxLength().Should().Be(450);
        reminder.FindProperty(nameof(ActivityReminder.AssessorUserId))!.GetMaxLength().Should().Be(450);
        reminder.FindProperty(nameof(ActivityReminder.SentByUserId))!.IsNullable.Should().BeFalse();
        reminder.FindProperty(nameof(ActivityReminder.AssessorUserId))!.IsNullable.Should().BeFalse();
        reminder.FindProperty(nameof(ActivityReminder.SentOn))!.GetColumnType().Should().Be("timestamp with time zone");
        reminder.FindProperty(nameof(ActivityReminder.SentOnDay))!.GetColumnType().Should().Be("date");
    }

    /// <summary>C4c: a reminder goes with its activity, and neither side can reach the other through a navigation.</summary>
    [Fact]
    public void TheReminder_CascadesWithItsActivity_AndNoNavigationJoinsThem()
    {
        var reminder = Model.FindEntityType(typeof(ActivityReminder))!;

        var key = reminder.GetForeignKeys().Should().ContainSingle().Subject;
        key.PrincipalEntityType.ClrType.Should().Be(typeof(Activity));
        key.Properties.Select(property => property.Name).Should().Equal(nameof(ActivityReminder.ActivityId));
        key.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
        key.DependentToPrincipal.Should().BeNull("a reminder carries the activity's id, not the activity");
        key.PrincipalToDependent.Should().BeNull("an activity never loads, or saves, its reminders");
    }

    [Fact]
    public void TheAdmissionDay_IsARequiredDate()
    {
        var admittedOn = Model.FindEntityType(typeof(TraineeProfile))!.FindProperty(nameof(TraineeProfile.AdmittedOn))!;

        admittedOn.IsNullable.Should().BeFalse();
        admittedOn.GetColumnType().Should().Be("date");
    }
}
