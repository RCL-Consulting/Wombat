using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Institutions.Queries.GetCollegeTree;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Institutions;

/// <summary>
/// T394 (flow 16, Q1, Q7): the College's tree in one read: each speciality by name with its sub-specialities by name, and
/// each one's default ladder by name; null for an unknown College and for another College's, so the page reads "College
/// unavailable"; an Administrator any College.
/// </summary>
public sealed class GetCollegeTreeTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private int _cpsaId;
    private int _cnsaId;
    private int _paediatricsId;

    public async Task InitializeAsync()
    {
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var ladder = new EntrustmentScale { Name = "CPSA Paediatric Entrustment Scale v11.1" };
        _db.EntrustmentScales.Add(ladder);
        var cpsa = new College { Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" };
        var cnsa = new College { Name = "College of Neurologists of South Africa", ShortCode = "CNSA" };
        _db.Colleges.AddRange(cpsa, cnsa);
        await _db.SaveChangesAsync();
        _cpsaId = cpsa.Id;
        _cnsaId = cnsa.Id;

        var surgery = new Speciality { CollegeId = cpsa.Id, Name = "Surgery", IsActive = false };
        var paediatrics = new Speciality { CollegeId = cpsa.Id, Name = "Paediatrics", Description = "Specialist training in Paediatrics." };
        var neurology = new Speciality { CollegeId = cnsa.Id, Name = "Neurology" };
        _db.Set<Speciality>().AddRange(surgery, paediatrics, neurology);
        await _db.SaveChangesAsync();
        _paediatricsId = paediatrics.Id;

        _db.Set<SubSpeciality>().AddRange(
            new SubSpeciality { SpecialityId = paediatrics.Id, Name = "Paediatrics", Description = "General paediatric specialist training programme.", DefaultEntrustmentScaleId = ladder.Id },
            new SubSpeciality { SpecialityId = paediatrics.Id, Name = "Neonatology", IsActive = false },
            new SubSpeciality { SpecialityId = neurology.Id, Name = "Adult Neurology" });
        await _db.SaveChangesAsync();
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    [Fact]
    public async Task TheCollegeAdmin_ReadsHisCollegesTree_SpecialitiesAndSubSpecialitiesByName_WithTheLaddersName()
    {
        var tree = await Handle(_cpsaId, TestPrincipals.CollegeAdmin(_cpsaId));

        tree.Should().NotBeNull();
        tree!.Name.Should().Be("College of Paediatricians of South Africa");
        tree.ShortCode.Should().Be("CPSA");
        tree.Specialities.Select(speciality => speciality.Name).Should().Equal("Paediatrics", "Surgery");
        tree.Specialities[1].IsActive.Should().BeFalse();
        tree.Specialities[1].SubSpecialities.Should().BeEmpty();

        var paediatrics = tree.Specialities[0];
        paediatrics.Id.Should().Be(_paediatricsId);
        paediatrics.Description.Should().Be("Specialist training in Paediatrics.");
        paediatrics.SubSpecialities.Select(sub => (sub.Name, sub.IsActive, sub.DefaultEntrustmentScaleName)).Should().Equal(
            ("Neonatology", false, null),
            ("Paediatrics", true, "CPSA Paediatric Entrustment Scale v11.1"));
        paediatrics.SubSpecialities.Should().OnlyContain(sub => sub.SpecialityId == _paediatricsId);
    }

    [Fact]
    public async Task AnotherCollegesTree_ReadsAsOneThatDoesNotExist()
        => (await Handle(_cnsaId, TestPrincipals.CollegeAdmin(_cpsaId))).Should().BeNull();

    [Fact]
    public async Task AnUnknownCollege_IsNull_EvenForTheAdministrator()
        => (await Handle(_cnsaId + 100, TestPrincipals.Administrator())).Should().BeNull();

    [Fact]
    public async Task TheAdministrator_ReadsAnyCollegesTree()
    {
        var tree = await Handle(_cnsaId, TestPrincipals.Administrator());

        tree!.Specialities.Should().ContainSingle().Which.SubSpecialities.Should().ContainSingle()
            .Which.Should().Match<CollegeTreeSubSpecialityDto>(sub =>
                sub.Name == "Adult Neurology" && sub.DefaultEntrustmentScaleId == null && sub.DefaultEntrustmentScaleName == null);
    }

    [Fact]
    public async Task AnInstitutionalAdmin_ReadsNoTree()
        => (await Handle(_cpsaId, TestPrincipals.InstitutionalAdmin(1))).Should().BeNull();

    private Task<CollegeTreeDto?> Handle(int collegeId, System.Security.Claims.ClaimsPrincipal principal)
        => new GetCollegeTreeQueryHandler(_db).Handle(new GetCollegeTreeQuery(collegeId, principal), CancellationToken.None);
}
