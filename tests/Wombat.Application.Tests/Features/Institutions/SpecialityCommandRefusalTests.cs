using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Commands.CreateSpeciality;
using Wombat.Application.Features.Institutions.Commands.CreateSubSpeciality;
using Wombat.Application.Features.Institutions.Commands.UpdateSpeciality;
using Wombat.Application.Features.Institutions.Commands.UpdateSubSpeciality;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Institutions;

/// <summary>
/// T394 (flow 16, N1, Q1, T324): a speciality's and a sub-speciality's create and update refuse a taken name as the Name
/// field's refusal, in the built words, read before anything is staged; a sub-speciality's create takes its default ladder;
/// and the validators word an empty name and the caps in the label's words.
/// </summary>
/// <remarks>
/// The unique index's half (a name taken between the check and the save) needs a real server:
/// <c>SubSpecialitySaveRacePostgresTests</c> holds it for both (Integration).
/// </remarks>
public sealed class SpecialityCommandRefusalTests : IAsyncLifetime
{
    private const string SpecialityTaken = "A speciality with the same name already exists for this college.";
    private const string SubSpecialityTaken = "A sub-speciality with the same name already exists for this speciality.";

    private ApplicationDbContext _db = null!;
    private int _cpsaId;
    private int _cnsaId;
    private int _paediatricsId;
    private int _surgeryId;
    private int _neurologyId;
    private int _neonatologyId;
    private int _ladderId;

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

        var paediatrics = new Speciality { CollegeId = cpsa.Id, Name = "Paediatrics" };
        var surgery = new Speciality { CollegeId = cpsa.Id, Name = "Surgery" };
        var neurology = new Speciality { CollegeId = cnsa.Id, Name = "Neurology" };
        _db.Set<Speciality>().AddRange(paediatrics, surgery, neurology);
        await _db.SaveChangesAsync();

        var neonatology = new SubSpeciality { SpecialityId = paediatrics.Id, Name = "Neonatology" };
        _db.Set<SubSpeciality>().AddRange(
            neonatology,
            new SubSpeciality { SpecialityId = paediatrics.Id, Name = "Paediatrics" },
            new SubSpeciality { SpecialityId = neurology.Id, Name = "Adult Neurology" });
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        (_cpsaId, _cnsaId, _paediatricsId, _surgeryId, _neurologyId, _neonatologyId, _ladderId)
            = (cpsa.Id, cnsa.Id, paediatrics.Id, surgery.Id, neurology.Id, neonatology.Id, ladder.Id);
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    [Fact]
    public async Task CreateSpeciality_WithANameTheCollegeHolds_IsTheNameFieldsRefusal_AndStagesNothing()
    {
        var create = () => new CreateSpecialityCommandHandler(_db).Handle(
            new CreateSpecialityCommand(_cpsaId, " Paediatrics ", null, TestPrincipals.CollegeAdmin(_cpsaId)), CancellationToken.None);

        await ShouldBeRefusedUnderName(create, SpecialityTaken);
        (await _db.Set<Speciality>().CountAsync(speciality => speciality.Name == "Paediatrics")).Should().Be(1);
    }

    [Fact]
    public async Task CreateSpeciality_WithANameAnotherCollegeHolds_IsCreated()
    {
        var created = await new CreateSpecialityCommandHandler(_db).Handle(
            new CreateSpecialityCommand(_cpsaId, "Neurology", "Specialist training in Neurology.", TestPrincipals.CollegeAdmin(_cpsaId)),
            CancellationToken.None);

        created.Name.Should().Be("Neurology");
        created.CollegeId.Should().Be(_cpsaId);
    }

    [Fact]
    public async Task CreateSpeciality_TheNameIsComparedAsTheIndexCompares_SoAnotherCaseIsFree()
    {
        var created = await new CreateSpecialityCommandHandler(_db).Handle(
            new CreateSpecialityCommand(_cpsaId, "paediatrics", null, TestPrincipals.Administrator()), CancellationToken.None);

        created.Name.Should().Be("paediatrics", "the unique index is ordinal, so the check is too (N7)");
    }

    [Fact]
    public async Task UpdateSpeciality_ToANameASiblingHolds_IsTheNameFieldsRefusal_AndChangesNothing()
    {
        var update = () => new UpdateSpecialityCommandHandler(_db).Handle(
            new UpdateSpecialityCommand(_surgeryId, _cpsaId, "Paediatrics", "Changed.", false, TestPrincipals.CollegeAdmin(_cpsaId)),
            CancellationToken.None);

        await ShouldBeRefusedUnderName(update, SpecialityTaken);
        var surgery = await _db.Set<Speciality>().AsNoTracking().SingleAsync(speciality => speciality.Id == _surgeryId);
        (surgery.Name, surgery.Description, surgery.IsActive).Should().Be(("Surgery", (string?)null, true));
    }

    [Fact]
    public async Task UpdateSpeciality_ToACollegeThatDoesNotExist_IsRefusedAsTheCreateIs_AndChangesNothing()
    {
        // Code N-8: read as CreateSpeciality reads it, before anything is staged, not left to the foreign key.
        var update = () => new UpdateSpecialityCommandHandler(_db).Handle(
            new UpdateSpecialityCommand(_surgeryId, 9999, "Surgery", "Moved.", true, TestPrincipals.Administrator()),
            CancellationToken.None);

        await update.Should().ThrowAsync<InvalidOperationException>().WithMessage("College 9999 was not found.");
        _db.ChangeTracker.HasChanges().Should().BeFalse("nothing is staged for the audit pipeline to commit (T201)");
    }

    [Fact]
    public async Task UpdateSpeciality_KeepingItsOwnName_IsSaved()
    {
        var saved = await new UpdateSpecialityCommandHandler(_db).Handle(
            new UpdateSpecialityCommand(_paediatricsId, _cpsaId, "Paediatrics", "Specialist training.", true, TestPrincipals.CollegeAdmin(_cpsaId)),
            CancellationToken.None);

        saved.Description.Should().Be("Specialist training.");
    }

    [Fact]
    public async Task CreateSubSpeciality_WithANameTheSpecialityHolds_IsTheNameFieldsRefusal_AndStagesNothing()
    {
        var create = () => new CreateSubSpecialityCommandHandler(_db).Handle(
            new CreateSubSpecialityCommand(_paediatricsId, "Neonatology", null, _ladderId, TestPrincipals.CollegeAdmin(_cpsaId)),
            CancellationToken.None);

        await ShouldBeRefusedUnderName(create, SubSpecialityTaken);
        (await _db.Set<SubSpeciality>().CountAsync(sub => sub.Name == "Neonatology")).Should().Be(1);
    }

    [Fact]
    public async Task CreateSubSpeciality_WithADefault_StoresTheLadderChosenAtCreate()
    {
        var created = await new CreateSubSpecialityCommandHandler(_db).Handle(
            new CreateSubSpecialityCommand(_surgeryId, "Paediatric surgery", "Certificate training.", _ladderId, TestPrincipals.CollegeAdmin(_cpsaId)),
            CancellationToken.None);

        created.DefaultEntrustmentScaleId.Should().Be(_ladderId);
        (await _db.Set<SubSpeciality>().AsNoTracking().SingleAsync(sub => sub.Id == created.Id))
            .DefaultEntrustmentScaleId.Should().Be(_ladderId);
    }

    [Fact]
    public async Task CreateSubSpeciality_WithNoDefault_OffersEveryScale()
    {
        var created = await new CreateSubSpecialityCommandHandler(_db).Handle(
            new CreateSubSpecialityCommand(_neurologyId, "Paediatric Neurology", null, null, TestPrincipals.Administrator()),
            CancellationToken.None);

        created.DefaultEntrustmentScaleId.Should().BeNull();
    }

    [Fact]
    public async Task CreateSubSpeciality_WithAScaleThatDoesNotExist_IsRefusedBeforeAnythingIsStaged()
    {
        var create = () => new CreateSubSpecialityCommandHandler(_db).Handle(
            new CreateSubSpecialityCommand(_surgeryId, "Paediatric surgery", null, _ladderId + 50, TestPrincipals.Administrator()),
            CancellationToken.None);

        (await create.Should().ThrowAsync<InvalidOperationException>()).Which.Should().NotBeOfType<FieldRefusalException>();
        _db.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSubSpeciality_ToANameASiblingHolds_IsTheNameFieldsRefusal_AndChangesNothing()
    {
        var update = () => new UpdateSubSpecialityCommandHandler(_db).Handle(
            new UpdateSubSpecialityCommand(_neonatologyId, _paediatricsId, "Paediatrics", null, false, _ladderId, TestPrincipals.CollegeAdmin(_cpsaId)),
            CancellationToken.None);

        await ShouldBeRefusedUnderName(update, SubSpecialityTaken);
        var neonatology = await _db.Set<SubSpeciality>().AsNoTracking().SingleAsync(sub => sub.Id == _neonatologyId);
        (neonatology.Name, neonatology.IsActive, neonatology.DefaultEntrustmentScaleId).Should().Be(("Neonatology", true, (int?)null));
    }

    [Fact]
    public async Task UpdateSubSpeciality_ToANameAnotherSpecialitysSubSpecialityHolds_IsSaved()
    {
        var saved = await new UpdateSubSpecialityCommandHandler(_db).Handle(
            new UpdateSubSpecialityCommand(_neonatologyId, _paediatricsId, "Adult Neurology", null, true, null, TestPrincipals.Administrator()),
            CancellationToken.None);

        saved.Name.Should().Be("Adult Neurology");
    }

    [Fact]
    public void TheValidators_WordAnEmptyName_AndTheCaps_InTheLabelsWords()
    {
        var name = new string('N', 201);
        var description = new string('D', 2001);
        var principal = TestPrincipals.Administrator();

        var createSpeciality = new CreateSpecialityCommandValidator();
        createSpeciality.TestValidate(new CreateSpecialityCommand(1, " ", null, principal))
            .ShouldHaveValidationErrorFor(command => command.Name).WithErrorMessage("Enter a name.");
        createSpeciality.TestValidate(new CreateSpecialityCommand(1, name, description, principal))
            .ShouldHaveValidationErrorFor(command => command.Name).WithErrorMessage("A name is at most 200 characters.");
        createSpeciality.TestValidate(new CreateSpecialityCommand(1, "N", description, principal))
            .ShouldHaveValidationErrorFor(command => command.Description).WithErrorMessage("A description is at most 2,000 characters.");
        createSpeciality.TestValidate(new CreateSpecialityCommand(1, "N", new string('D', 2000), principal))
            .ShouldNotHaveAnyValidationErrors();

        new UpdateSpecialityCommandValidator().TestValidate(new UpdateSpecialityCommand(1, 1, "", description, true, principal))
            .ShouldHaveValidationErrorFor(command => command.Name).WithErrorMessage("Enter a name.");
        new CreateSubSpecialityCommandValidator().TestValidate(new CreateSubSpecialityCommand(1, "", description, null, principal))
            .ShouldHaveValidationErrorFor(command => command.Description).WithErrorMessage("A description is at most 2,000 characters.");
        new UpdateSubSpecialityCommandValidator().TestValidate(new UpdateSubSpecialityCommand(1, 1, name, null, true, null, principal))
            .ShouldHaveValidationErrorFor(command => command.Name).WithErrorMessage("A name is at most 200 characters.");
    }

    private async Task ShouldBeRefusedUnderName(Func<Task> act, string words)
    {
        var refused = (await act.Should().ThrowAsync<FieldRefusalException>()).Which;
        refused.Field.Should().Be("Name");
        refused.Message.Should().Be(words);
        refused.InnerException.Should().BeNull("the holder was read before anything was saved");
        _db.ChangeTracker.HasChanges().Should().BeFalse("nothing is staged before a refusal (T201)");
    }
}
