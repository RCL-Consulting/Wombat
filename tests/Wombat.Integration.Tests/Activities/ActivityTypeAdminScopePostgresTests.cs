using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.ListActivityTypesAdmin;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;

namespace Wombat.Integration.Tests.Activities;

/// <summary>
/// T300 on a real PostgreSQL server: who may write an activity type, read by the builder's list, its editor and its Scope
/// picker through <c>ActivityTypeAdminScope</c>.
/// </summary>
/// <remarks>
/// <para>
/// The unit suite (<c>ActivityTypeScopeGuardTests</c>) runs on EF InMemory, which evaluates each query in memory. Here each
/// must translate: the list's one read of its rows' Colleges (two array <c>Contains</c>, <c>= ANY</c>), the Scope picker's
/// targets narrowed by a captured role flag and claim, and a sub-speciality's name joined to its speciality's by string
/// concatenation. The figures are Steps 1.24 to 1.26 in small: a College instrument, an institution's own type, and a
/// Global one, read by an Administrator, an InstitutionalAdmin and the College's CollegeAdmin.
/// </para>
/// <para>
/// Isolated the way <c>DashboardWaitingPostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>),
/// registered before it is created and dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class ActivityTypeAdminScopePostgresTests : IAsyncLifetime
{
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheListTheEditorAndThePicker_OnPostgres_GiveTheGuardsVerdict()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await using (var db = NewContext(schema))
            {
                await db.Database.MigrateAsync();
            }

            int kalafong, baragwanath, cpsa, paediatrics, neonatology, otherSpeciality, otherSub;
            int globalType, kalafongType, baragwanathType, cpsaType, cpsaSubType, otherType, otherSubType;
            await using (var db = NewContext(schema))
            {
                var kgk = new Institution { Name = "T300 Kalafong", ShortCode = "T300KGK", IsActive = true, CreatedOn = DateTime.UtcNow };
                var chbah = new Institution { Name = "T300 Baragwanath", ShortCode = "T300CHB", IsActive = true, CreatedOn = DateTime.UtcNow };
                var paediatricians = new College { Name = "T300 College of Paediatricians", ShortCode = "T300CPSA", IsActive = true };
                var physicians = new College { Name = "T300 College of Physicians", ShortCode = "T300CP", IsActive = true };
                var paeds = new Speciality { College = paediatricians, Name = "T300 Paediatrics", IsActive = true };
                var neonates = new SubSpeciality { Speciality = paeds, Name = "Neonatology", IsActive = true };
                var medicine = new Speciality { College = physicians, Name = "T300 Internal Medicine", IsActive = true };
                var cardiology = new SubSpeciality { Speciality = medicine, Name = "Cardiology", IsActive = true };
                db.AddRange(kgk, chbah, paediatricians, physicians, paeds, neonates, medicine, cardiology);
                await db.SaveChangesAsync();
                (kalafong, baragwanath, cpsa) = (kgk.Id, chbah.Id, paediatricians.Id);
                (paediatrics, neonatology, otherSpeciality, otherSub) = (paeds.Id, neonates.Id, medicine.Id, cardiology.Id);

                globalType = AddType(db, "t300_global", ActivityScope.Global, null);
                kalafongType = AddType(db, "t300_kgk", ActivityScope.Institution, kalafong);
                baragwanathType = AddType(db, "t300_chbah", ActivityScope.Institution, baragwanath);
                cpsaType = AddType(db, "t300_mini_cex_cpsa", ActivityScope.Speciality, paediatrics);
                cpsaSubType = AddType(db, "t300_neonatal_cpsa", ActivityScope.SubSpeciality, neonatology);
                otherType = AddType(db, "t300_physicians", ActivityScope.Speciality, otherSpeciality);
                otherSubType = AddType(db, "t300_cardiology", ActivityScope.SubSpeciality, otherSub);
            }

            var verdicts = new Dictionary<string, (ClaimsPrincipal Principal, int[] Writes)>
            {
                ["an Administrator"] = (Principal([WombatRoles.Administrator], kalafong),
                    [globalType, kalafongType, baragwanathType, cpsaType, cpsaSubType, otherType, otherSubType]),
                ["an InstitutionalAdmin of Kalafong"] = (Principal([WombatRoles.InstitutionalAdmin], kalafong), [kalafongType]),
                // A CollegeAdmin carries an institution claim too (T113); it makes him no InstitutionalAdmin.
                ["a CollegeAdmin of the College of Paediatricians"] = (Principal([WombatRoles.CollegeAdmin], kalafong, cpsa),
                    [cpsaType, cpsaSubType])
            };
            int[] ours = [globalType, kalafongType, baragwanathType, cpsaType, cpsaSubType, otherType, otherSubType];

            foreach (var (who, (principal, writes)) in verdicts)
            {
                await using var db = NewContext(schema);

                var list = await new ListActivityTypesAdminQueryHandler(db).Handle(new ListActivityTypesAdminQuery(principal), CancellationToken.None);
                list.CanCreate.Should().BeTrue($"{who} has a scope to create in");
                list.Items.Where(item => ours.Contains(item.Id) && item.CanWrite).Select(item => item.Id)
                    .Should().BeEquivalentTo(writes, $"the list tells {who} what the guard admits");

                foreach (var typeId in ours.Where(id => list.Items.Any(item => item.Id == id)))
                {
                    var editor = await new GetActivityTypeEditorQueryHandler(db).Handle(new GetActivityTypeEditorQuery(typeId, principal), CancellationToken.None);
                    editor.CanWrite.Should().Be(writes.Contains(typeId), $"the editor tells {who} what the guard admits on type {typeId}");
                }

                // The picker is exactly the pairs the guard admits among every target the database holds.
                var offered = await ActivityTypeAdminScope.WritableScopesAsync(db, principal, CancellationToken.None);
                var offeredPairs = offered
                    .SelectMany(choice => choice.Scope == ActivityScope.Global
                        ? [(choice.Scope, (int?)null)]
                        : choice.Targets.Select(target => (choice.Scope, (int?)target.Id)))
                    .ToList();
                var admitted = new List<(ActivityScope, int?)>();
                foreach (var pair in await EveryPairAsync(db))
                {
                    if (await ActivityTypeAdminScope.MayWriteAsync(db, principal, pair.Scope, pair.ScopeId, CancellationToken.None))
                    {
                        admitted.Add(pair);
                    }
                }

                offeredPairs.Should().BeEquivalentTo(admitted, $"{who} is offered exactly the scopes the guard admits them to");
            }

            await using (var db = NewContext(schema))
            {
                var college = await ActivityTypeAdminScope.WritableScopesAsync(db, verdicts["a CollegeAdmin of the College of Paediatricians"].Principal, CancellationToken.None);
                college.Select(choice => choice.Scope).Should().Equal(ActivityScope.Speciality, ActivityScope.SubSpeciality);
                college[1].Targets.Select(target => target.Name).Should().Equal("T300 Paediatrics / Neonatology");

                var institution = await ActivityTypeAdminScope.WritableScopesAsync(db, verdicts["an InstitutionalAdmin of Kalafong"].Principal, CancellationToken.None);
                institution.Should().ContainSingle().Which.Targets.Select(target => target.Name).Should().Equal("T300 Kalafong");

                var editor = await new GetActivityTypeEditorQueryHandler(db).Handle(
                    new GetActivityTypeEditorQuery(cpsaSubType, verdicts["an InstitutionalAdmin of Kalafong"].Principal), CancellationToken.None);
                editor.ScopeTargetName.Should().Be("T300 Paediatrics / Neonatology");

                // The list names every row's target in one read per kind (T300 review, T291 item 5): the institutions'
                // names, and the sub-specialities' joined to their specialities', each by an array Contains.
                var everyName = new Dictionary<int, string?>
                {
                    [globalType] = null,
                    [kalafongType] = "T300 Kalafong",
                    [baragwanathType] = "T300 Baragwanath",
                    [cpsaType] = "T300 Paediatrics",
                    [cpsaSubType] = "T300 Paediatrics / Neonatology",
                    [otherType] = "T300 Internal Medicine",
                    [otherSubType] = "T300 Internal Medicine / Cardiology"
                };
                var administratorList = await new ListActivityTypesAdminQueryHandler(db).Handle(
                    new ListActivityTypesAdminQuery(verdicts["an Administrator"].Principal), CancellationToken.None);
                administratorList.Items.Where(item => ours.Contains(item.Id)).ToDictionary(item => item.Id, item => item.ScopeTargetName)
                    .Should().Equal(everyName);

                // Another College's discipline is named to a CollegeAdmin too.
                var collegeList = await new ListActivityTypesAdminQueryHandler(db).Handle(
                    new ListActivityTypesAdminQuery(verdicts["a CollegeAdmin of the College of Paediatricians"].Principal), CancellationToken.None);
                collegeList.Items.Single(item => item.Id == otherSubType).ScopeTargetName.Should().Be("T300 Internal Medicine / Cardiology");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static async Task<List<(ActivityScope Scope, int? ScopeId)>> EveryPairAsync(ApplicationDbContext db)
    {
        var pairs = new List<(ActivityScope, int?)> { (ActivityScope.Global, null) };
        pairs.AddRange((await db.Institutions.Select(entity => entity.Id).ToListAsync()).Select(id => (ActivityScope.Institution, (int?)id)));
        pairs.AddRange((await db.Specialities.Select(entity => entity.Id).ToListAsync()).Select(id => (ActivityScope.Speciality, (int?)id)));
        pairs.AddRange((await db.SubSpecialities.Select(entity => entity.Id).ToListAsync()).Select(id => (ActivityScope.SubSpeciality, (int?)id)));
        return pairs;
    }

    private static int AddType(ApplicationDbContext db, string key, ActivityScope scope, int? scopeId)
    {
        var type = new ActivityType
        {
            Key = key,
            Name = key,
            Scope = scope,
            ScopeId = scopeId,
            Version = 1,
            IsActive = true,
            SchemaJson = "{}",
            WorkflowJson = "{}",
            CreditRulesJson = """{ "counts_for": [] }""",
            OwnerUserId = "system",
            CreatedOn = DateTime.UtcNow
        };
        db.ActivityTypes.Add(type);
        db.SaveChanges();
        return type.Id;
    }

    private static ClaimsPrincipal Principal(string[] roles, int institutionId, int? collegeId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "t300-caller"),
            new(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        if (collegeId is int college)
        {
            claims.Add(new Claim(WombatClaimTypes.CollegeId, college.ToString(CultureInfo.InvariantCulture)));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);
}
