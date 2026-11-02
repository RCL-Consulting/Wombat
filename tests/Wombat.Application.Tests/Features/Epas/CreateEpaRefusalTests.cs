using System.Data.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wombat.Application.Common;
using Wombat.Application.Features.Epas;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Epas;

/// <summary>
/// T392 (flow 15, lane C): an EPA's refusals belong to their fields (F4). An institution's own EPA only in a discipline the
/// institution has actively adopted, on a create and on a save that moves it (F9, Q12; T291's command half), refused under
/// Sub-speciality before anything is staged (T201); a duplicate code only when the code's unique index refused the save
/// (T272), under Code; any other refused save left as the failure it is, which the page answers in its fixed words.
/// </summary>
public sealed class CreateEpaRefusalTests : IAsyncLifetime
{
    private readonly FailingSave _failingSave = new();
    private ApplicationDbContext _db = null!;
    private int _institutionId;
    private int _adoptedSubSpecialityId;
    private int _otherSubSpecialityId;
    private int _ownEpaId;
    private int _nationalEpaId;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(_failingSave)
            .Options;
        _db = new ApplicationDbContext(options);

        var college = new College { Name = "CPSA", ShortCode = "CPSA", IsActive = true, CreatedOn = DateTime.UtcNow };
        _db.Colleges.Add(college);
        await _db.SaveChangesAsync();

        var speciality = new Speciality { CollegeId = college.Id, Name = "Paediatrics", IsActive = true };
        _db.Specialities.Add(speciality);
        await _db.SaveChangesAsync();

        var paediatrics = new SubSpeciality { SpecialityId = speciality.Id, Name = "Paediatrics", IsActive = true };
        var neonatology = new SubSpeciality { SpecialityId = speciality.Id, Name = "Neonatology", IsActive = true };
        _db.SubSpecialities.AddRange(paediatrics, neonatology);
        var institution = new Institution { Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow };
        _db.Institutions.Add(institution);
        await _db.SaveChangesAsync();

        var curriculum = new Curriculum { SubSpecialityId = paediatrics.Id, Name = "Paediatric EPA Curriculum", Version = "11.1", EffectiveFrom = new DateOnly(2026, 1, 1) };
        _db.Curricula.Add(curriculum);
        await _db.SaveChangesAsync();
        _db.Set<InstitutionCurriculumAdoption>().Add(new InstitutionCurriculumAdoption
        {
            InstitutionId = institution.Id,
            CurriculumId = curriculum.Id,
            SubSpecialityId = paediatrics.Id,
            AdoptedOn = new DateOnly(2026, 10, 30),
            IsActive = true
        });

        var national = new Epa { SubSpecialityId = paediatrics.Id, Code = "PAED-001", Title = "Emergency care", IsActive = true };
        var own = new Epa { SubSpecialityId = paediatrics.Id, OwningInstitutionId = institution.Id, Code = "KGK-001", Title = "Outreach", IsActive = true };
        _db.Epas.AddRange(national, own);
        await _db.SaveChangesAsync();

        _institutionId = institution.Id;
        _adoptedSubSpecialityId = paediatrics.Id;
        _otherSubSpecialityId = neonatology.Id;
        _ownEpaId = own.Id;
        _nationalEpaId = national.Id;
        _db.ChangeTracker.Clear();
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Create_AnOwnEpaInADisciplineNotAdopted_IsRefusedUnderSubSpeciality_WithNothingStaged()
    {
        var act = () => new CreateEpaCommandHandler(_db).Handle(
            new CreateEpaCommand(_otherSubSpecialityId, "KGK-002", "Neonatal outreach", null, null, EpaCategory.Elective, Admin()),
            CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<FieldRefusalException>()).Which;
        refusal.Field.Should().Be(nameof(CreateEpaCommand.SubSpecialityId));
        refusal.Message.Should().Be("Your institution has not adopted Paediatrics / Neonatology, so an EPA cannot be added to it.");
        _db.ChangeTracker.Entries<Epa>().Should().NotContain(entry => entry.State == EntityState.Added,
            "the audit pipeline commits what a refused handler staged (T201)");
        (await _db.Epas.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Create_AnOwnEpaInAnAdoptedDiscipline_IsSaved_AsHerInstitutions()
    {
        var created = await new CreateEpaCommandHandler(_db).Handle(
            new CreateEpaCommand(_adoptedSubSpecialityId, "KGK-002", "Outreach two", null, null, EpaCategory.Elective, Admin()),
            CancellationToken.None);

        created.OwningInstitutionId.Should().Be(_institutionId);
        created.OwningInstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        created.MayChange.Should().BeTrue();
        created.DisciplineName.Should().Be("Paediatrics / Paediatrics");
    }

    [Fact]
    public async Task Update_ThatMovesAnOwnEpaIntoADisciplineNotAdopted_IsRefusedUnderSubSpeciality_WithNothingChanged()
    {
        var act = () => UpdateHandler().Handle(
            new UpdateEpaCommand(_ownEpaId, _otherSubSpecialityId, "KGK-001", "Renamed", null, null, EpaCategory.Elective, true, Admin()),
            CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<FieldRefusalException>()).Which;
        refusal.Field.Should().Be(nameof(UpdateEpaCommand.SubSpecialityId));
        refusal.Message.Should().Be("Your institution has not adopted Paediatrics / Neonatology, so an EPA cannot be added to it.");
        _db.ChangeTracker.Entries<Epa>().Should().OnlyContain(entry => entry.State == EntityState.Unchanged,
            "the refusal is found before anything is changed (T201)");
    }

    [Fact]
    public async Task Update_OfAnOwnEpaLeftInADisciplineNoLongerAdopted_ThatKeepsItsDiscipline_IsSaved()
    {
        // F9: an own EPA left in a discipline no longer adopted keeps Edit, so a save that does not move it is accepted.
        var own = await _db.Epas.SingleAsync(epa => epa.Id == _ownEpaId);
        own.SubSpecialityId = _otherSubSpecialityId;
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        var result = await UpdateHandler().Handle(
            new UpdateEpaCommand(_ownEpaId, _otherSubSpecialityId, "KGK-001", "Renamed", null, null, EpaCategory.Elective, true, Admin()),
            CancellationToken.None);

        result.Epa.Title.Should().Be("Renamed");
        result.Epa.SubSpecialityId.Should().Be(_otherSubSpecialityId, "its discipline is unchanged");
        result.Epa.DisciplineName.Should().Be("Paediatrics / Neonatology");
    }

    [Fact]
    public async Task Update_ByAnAdministrator_NamesTheInstitutionThatHasNotAdopted()
    {
        var act = () => UpdateHandler().Handle(
            new UpdateEpaCommand(_ownEpaId, _otherSubSpecialityId, "KGK-001", "Outreach", null, null, EpaCategory.Elective, true, TestPrincipals.Administrator()),
            CancellationToken.None);

        (await act.Should().ThrowAsync<FieldRefusalException>()).Which.Message
            .Should().Be("Kgosi Kgari Teaching Hospital has not adopted Paediatrics / Neonatology, so an EPA cannot be added to it.");
    }

    [Fact]
    public async Task Create_RefusedByTheCodesUniqueIndex_IsADuplicate_UnderCode()
    {
        _failingSave.Next = new DbUpdateException("duplicate", new FakeDbException("23505"));

        var act = () => new CreateEpaCommandHandler(_db).Handle(
            new CreateEpaCommand(_adoptedSubSpecialityId, "KGK-001", "Outreach again", null, null, EpaCategory.Elective, Admin()),
            CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<FieldRefusalException>()).Which;
        refusal.Field.Should().Be(nameof(CreateEpaCommand.Code));
        refusal.Message.Should().Be("An EPA with the same code already exists for this sub-speciality.");
        refusal.InnerException.Should().BeOfType<DbUpdateException>("the database's refusal stays underneath, so the pipeline drops the save");
    }

    [Fact]
    public async Task Create_RefusedForAnotherReason_IsNotCalledADuplicate()
    {
        // T272: until T392 every refused save read as a duplicate code.
        _failingSave.Next = new DbUpdateException("something else", new FakeDbException("23503"));

        var act = () => new CreateEpaCommandHandler(_db).Handle(
            new CreateEpaCommand(_adoptedSubSpecialityId, "KGK-009", "Outreach nine", null, null, EpaCategory.Elective, Admin()),
            CancellationToken.None);

        var thrown = (await act.Should().ThrowAsync<Exception>()).Which;
        thrown.Should().NotBeOfType<FieldRefusalException>();
        thrown.Message.Should().NotContain("same code");
    }

    [Fact]
    public async Task Update_RefusedByTheCodesUniqueIndex_IsADuplicate_UnderCode()
    {
        var second = new Epa { SubSpecialityId = _adoptedSubSpecialityId, Code = "PAED-002", Title = "Second", IsActive = true };
        _db.Epas.Add(second);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
        _failingSave.Next = new DbUpdateException("duplicate", new FakeDbException("23505"));

        var act = () => UpdateHandler().Handle(
            new UpdateEpaCommand(_nationalEpaId, _adoptedSubSpecialityId, "PAED-002", "Emergency care", null, null, EpaCategory.Core, true, TestPrincipals.Administrator()),
            CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<FieldRefusalException>()).Which;
        refusal.Field.Should().Be(nameof(UpdateEpaCommand.Code));
        refusal.Message.Should().Be("An EPA with the same code already exists for this sub-speciality.");
    }

    private System.Security.Claims.ClaimsPrincipal Admin() => TestPrincipals.InstitutionalAdmin(_institutionId);

    private UpdateEpaCommandHandler UpdateHandler()
        => new(_db, new CreditApplier(_db), new EpaCreditLock(_db), new TraineeCreditLock(_db));

    /// <summary>Refuses the next save with the exception the test sets, as the database would.</summary>
    private sealed class FailingSave : SaveChangesInterceptor
    {
        public Exception? Next { get; set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Next is { } next)
            {
                Next = null;
                throw next;
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

    private sealed class FakeDbException(string sqlState) : DbException("refused for the test")
    {
        public override string? SqlState => sqlState;
    }
}
