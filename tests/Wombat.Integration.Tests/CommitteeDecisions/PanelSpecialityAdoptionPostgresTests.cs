using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.CommitteeDecisions;

/// <summary>
/// T245 on a real PostgreSQL server: the panel form offers, and panel create accepts, a speciality panel only for a
/// speciality the institution has adopted, read through the one adoption join (<c>AdoptedSpecialities.IdsAt</c>) at the
/// panel's own institution, an Administrator's chosen one included (T245 review).
/// </summary>
/// <remarks>
/// The handler tests run on EF InMemory, which evaluates the join in memory; this is the check that it translates, and
/// that an adoption at another institution, or one no longer active, counts for nothing on the server either. Isolated the
/// way <c>DecisionPanelTraineeSeatPostgresTests</c> is: a migrated schema of its own, dropped in a finally and again on
/// dispose.
/// </remarks>
public sealed class PanelSpecialityAdoptionPostgresTests : IAsyncLifetime
{
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_OnlyASpecialityTheInstitutionHasAdopted_IsOfferedOrCreated()
    {
        var schema = await _schemas.CreateAsync();

        try
        {
            int institutionA, institutionB, paediatrics, surgery, generalMedicine;
            await using (var arrange = NewContext(schema))
            {
                await arrange.Database.MigrateAsync();

                var a = new Institution { Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow };
                var b = new Institution { Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow };
                var college = new College { Name = "CMSA", ShortCode = "CMSA", IsActive = true };
                var paeds = new Speciality { College = college, Name = "Paediatrics", IsActive = true };
                var surg = new Speciality { College = college, Name = "Surgery", IsActive = true };
                var medicine = new Speciality { College = college, Name = "General Medicine", IsActive = true };
                var generalPaediatrics = Curriculum(new SubSpeciality { Speciality = paeds, Name = "General Paediatrics", IsActive = true });
                var generalSurgery = Curriculum(new SubSpeciality { Speciality = surg, Name = "General Surgery", IsActive = true });
                var internalMedicine = Curriculum(new SubSpeciality { Speciality = medicine, Name = "General Internal Medicine", IsActive = true });
                arrange.AddRange(a, b, college, paeds, surg, medicine, generalPaediatrics, generalSurgery, internalMedicine);
                await arrange.SaveChangesAsync();
                arrange.AddRange(
                    // A trains General Paediatrics; it trained General Surgery once; only B trains General Medicine.
                    Adoption(a, generalPaediatrics, isActive: true),
                    Adoption(a, generalSurgery, isActive: false),
                    Adoption(b, internalMedicine, isActive: true));
                await arrange.SaveChangesAsync();

                (institutionA, institutionB, paediatrics, surgery, generalMedicine) = (a.Id, b.Id, paeds.Id, surg.Id, medicine.Id);
            }

            var specialityAdmin = SpecialityAdmin(institutionA, paediatrics, surgery, generalMedicine);

            await using (var db = NewContext(schema))
            {
                var admins = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
                    new GetDecisionPanelFormOptionsQuery(specialityAdmin), CancellationToken.None);
                var institutional = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
                    new GetDecisionPanelFormOptionsQuery(InstitutionalAdmin(institutionA)), CancellationToken.None);

                var administratorAtA = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
                    new GetDecisionPanelFormOptionsQuery(Administrator(), institutionA), CancellationToken.None);
                var administratorAtB = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
                    new GetDecisionPanelFormOptionsQuery(Administrator(), institutionB), CancellationToken.None);

                admins.Specialities!.Select(speciality => speciality.Id).Should().Equal(paediatrics);
                institutional.Specialities!.Select(speciality => speciality.Id).Should().Equal(paediatrics);
                administratorAtA.Specialities!.Select(speciality => speciality.Id).Should().Equal(paediatrics);
                administratorAtB.Specialities!.Select(speciality => speciality.Id).Should().Equal(generalMedicine);
            }

            var users = FakeUserDirectory.CommitteeMembersAt(institutionA, "chair", "member");
            foreach (var refused in new[] { surgery, generalMedicine })
            {
                await using var db = NewContext(schema);
                var create = () => CreateAsync(db, users, specialityAdmin, refused);

                (await create.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
                    "Your institution has adopted no curriculum in this speciality, so a panel for it would have no trainee to review.");

                // As the audit pipeline would, from its catch: the refusal came before anything was added.
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
            }

            // An Administrator at A is refused General Medicine, which only B trains.
            await using (var db = NewContext(schema))
            {
                var create = () => CreateAsync(db, users, Administrator(), generalMedicine, institutionA);

                (await create.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(
                    "The institution you chose has adopted no curriculum in this speciality, so a panel for it would have no " +
                    "trainee to review.");

                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
            }

            await using (var db = NewContext(schema))
            {
                (await db.DecisionPanels.CountAsync()).Should().Be(0);
                await CreateAsync(db, users, specialityAdmin, paediatrics);
            }

            await using (var read = NewContext(schema))
            {
                (await read.DecisionPanels.Select(panel => new { panel.InstitutionId, panel.SpecialityId }).ToListAsync())
                    .Should().ContainSingle().Which.Should().Be(new { InstitutionId = institutionA, SpecialityId = (int?)paediatrics });
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static Task<DecisionPanelDetailDto> CreateAsync(
        ApplicationDbContext db, FakeUserDirectory users, ClaimsPrincipal principal, int specialityId, int? institutionId = null)
        => new CreateDecisionPanelCommandHandler(db, users).Handle(
            new CreateDecisionPanelCommand(
                "Programme CCC",
                DecisionPanelScope.Speciality,
                institutionId,
                specialityId,
                [new("chair", DecisionPanelMemberRole.Chair), new("member", DecisionPanelMemberRole.Member)],
                principal),
            CancellationToken.None);

    private static Curriculum Curriculum(SubSpeciality subSpeciality)
        => new() { SubSpeciality = subSpeciality, Name = subSpeciality.Name, Version = "1", EffectiveFrom = new DateOnly(2024, 1, 1) };

    private static InstitutionCurriculumAdoption Adoption(Institution institution, Curriculum curriculum, bool isActive)
        => new()
        {
            Institution = institution,
            Curriculum = curriculum,
            SubSpecialityId = curriculum.SubSpecialityId,
            AdoptedOn = new DateOnly(2024, 1, 1),
            IsActive = isActive
        };

    private static ClaimsPrincipal SpecialityAdmin(int institutionId, params int[] specialityIds)
        => new(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "speciality-admin"),
                new Claim(ClaimTypes.Role, WombatRoles.SpecialityAdmin),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            }.Concat(specialityIds.Select(id => new Claim(WombatClaimTypes.SpecialityId, id.ToString(CultureInfo.InvariantCulture)))),
            "test"));

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "administrator"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            "test"));

    private static ClaimsPrincipal InstitutionalAdmin(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, $"inst-admin-{institutionId}"),
                new Claim(ClaimTypes.Role, WombatRoles.InstitutionalAdmin),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "test"));

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);
}
