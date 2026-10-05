using System.Security.Claims;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Application.Tests.Scheduling;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Programme;

/// <summary>
/// The cast of Waiting for assessors and of Send a reminder (T358, flow 06, lane A1): Kgosi Kgari Teaching Hospital's
/// Paediatrics and Surgery, another hospital, the activity types <see cref="AssessorReads" /> seeds (a Mini-CEX that waits
/// for the assessor its field names; a teaching session a SpecialityAdmin accepts by role; an MSF the Coordinator
/// records by role), and the people, each an account as the recipient port reads it.
/// </summary>
internal static class ProgrammeWaitingFixture
{
    public const int Kgk = 10;
    public const int OtherHospital = 11;
    public const int Paediatrics = 100;
    public const int Surgery = 101;
    public const int PaediatricsSub = 1000;
    public const int SurgerySub = 1010;

    public const string Smit = "smit";
    public const string Mokoena = "mokoena";
    public const string Sithole = "sithole";
    public const string Naidoo = "naidoo";
    public const string Zulu = "zulu";
    public const string Patel = "patel";
    public const string Khumalo = "khumalo";
    public const string Mahlangu = "mahlangu";
    public const string DuPlessis = "duplessis";

    /// <summary>D: 2026-10-04 08:00 UTC, 10:00 in South Africa.</summary>
    public static readonly DateTime Now = new(2026, 10, 4, 8, 0, 0, DateTimeKind.Utc);

    public static readonly (string Id, string First, string Last)[] People =
    [
        (Smit, "Pieter", "Smit"),
        (Mokoena, "Refilwe", "Mokoena"),
        (Sithole, "Kabelo", "Sithole"),
        (Naidoo, "David", "Naidoo"),
        (Zulu, "Thandi", "Zulu"),
        (Patel, "Mohammed", "Patel"),
        (Khumalo, "Fatima", "Khumalo"),
        (Mahlangu, "Nomsa", "Mahlangu"),
        (DuPlessis, "Pieter", "du Plessis")
    ];

    public static ApplicationDbContext Seeded()
    {
        var db = AssessorReads.CreateDb();
        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" },
            new Institution { Id = OtherHospital, Name = "Other Hospital", ShortCode = "OTH" });
        db.Specialities.AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics" },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery" });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = PaediatricsSub, SpecialityId = Paediatrics, Name = "Paediatrics" },
            new SubSpeciality { Id = SurgerySub, SpecialityId = Surgery, Name = "General Surgery" });
        AssessorReads.SeedTypes(db);
        foreach (var (id, first, last) in People)
        {
            var user = NomineeSeed.AddUser(db, id, Kgk);
            user.FirstName = first;
            user.LastName = last;
        }

        db.SaveChanges();
        return db;
    }

    public static FakeUserDirectory Directory()
        => new([.. People.Select(person => (person.Id, $"{person.First} {person.Last}"))]);

    public static ClaimsPrincipal Coordinator(string userId = Smit) => TestPrincipals.InRole(WombatRoles.Coordinator, userId, Kgk);

    public static ClaimsPrincipal SpecialityAdmin(string userId = Mokoena)
        => TestPrincipals.InRole(WombatRoles.SpecialityAdmin, userId, Kgk, specialityId: Paediatrics);

    public static ClaimsPrincipal CommitteeMember(string userId = Naidoo) => TestPrincipals.InRole(WombatRoles.CommitteeMember, userId, Kgk);

    /// <summary>A request to <paramref name="assessor" /> (null: the field is empty), stamped to its programme.</summary>
    public static Activity AddRequest(
        ApplicationDbContext db,
        int id,
        string subject,
        string? assessor,
        DateTime updatedOn,
        int? institutionId = Kgk,
        int? specialityId = Paediatrics,
        int? subSpecialityId = PaediatricsSub,
        int typeId = AssessorReads.WbaTypeId,
        string state = "requested")
    {
        var activity = new Activity
        {
            Id = id,
            ActivityTypeId = typeId,
            SchemaVersion = 1,
            SubjectUserId = subject,
            CreatedByUserId = subject,
            CurrentState = state,
            DataJson = assessor is null ? "{}" : $$"""{ "assessor_user_id": "{{assessor}}" }""",
            InstitutionId = institutionId,
            SpecialityId = specialityId,
            SubSpecialityId = subSpecialityId,
            ObservedOn = DateOnly.FromDateTime(updatedOn),
            ObservedOnSource = ObservationDateSource.Declared,
            CreatedOn = updatedOn,
            UpdatedOn = updatedOn
        };
        activity.Transitions.Add(new ActivityTransition
        {
            FromState = state, ToState = state, TransitionKey = "create", ActorUserId = subject, OccurredOn = updatedOn
        });
        db.Activities.Add(activity);
        return activity;
    }

    public static Task<WaitingForAssessorsDto?> WaitingAsync(
        ApplicationDbContext db,
        ClaimsPrincipal principal,
        string actingRole,
        bool overdueOnly = false,
        string? withUserId = null,
        string? subjectUserId = null,
        int page = 1,
        int pageSize = 20,
        DateTime? now = null)
        => new ListWaitingForAssessorsQueryHandler(
                db,
                Directory(),
                new ReminderRecipients(db),
                Options.Create(new DashboardThresholds()),
                new AssessorReads.FixedClock(now ?? Now))
            .Handle(
                new ListWaitingForAssessorsQuery(principal, actingRole, overdueOnly, withUserId, subjectUserId, page, pageSize),
                CancellationToken.None);

    public static Task<SendActivityReminderResult> SendAsync(
        ApplicationDbContext db,
        RecordingEmailSender mail,
        ClaimsPrincipal principal,
        string actingRole,
        Activity activity,
        DateTime? now = null,
        string? expectedState = null,
        DateTime? expectedUpdatedOn = null)
        => new SendActivityReminderCommandHandler(
                db, Directory(), new ReminderRecipients(db), mail, new AssessorReads.FixedClock(now ?? Now))
            .Handle(
                new SendActivityReminderCommand(
                    principal, actingRole, activity.Id, expectedState ?? activity.CurrentState, expectedUpdatedOn ?? activity.UpdatedOn),
                CancellationToken.None);
}
