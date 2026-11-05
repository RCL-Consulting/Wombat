using System.Data.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wombat.Application.Common;
using Wombat.Application.Features.Colleges;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Colleges;

/// <summary>
/// T394 (flow 16, lane B; S10, N7, T272, T324): a College's name and short code are checked before the save, ordinally as
/// the unique indexes compare, and a taken one is refused under its field naming the College that holds it, before
/// anything is staged. The unique index's refusal is read as a taken value only for a race, by the same words; any other
/// refused save is left as the failure it is, which the page answers in its fixed sentence. The validators speak the
/// fields' words, never a property's name.
/// </summary>
public sealed class CollegeRefusalTests : IAsyncLifetime
{
    private readonly FailingSave _failingSave = new();
    private readonly string _store = Guid.NewGuid().ToString();
    private ApplicationDbContext _db = null!;
    private int _cpsaId;
    private int _cnsaId;

    public async Task InitializeAsync()
    {
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_store)
            .AddInterceptors(_failingSave)
            .Options);

        var cpsa = new College { Name = "College of Paediatricians of South Africa", ShortCode = "CPSA", CreatedOn = DateTime.UtcNow };
        var cnsa = new College { Name = "College of Neurologists of South Africa", ShortCode = "CNSA", CreatedOn = DateTime.UtcNow };
        _db.Colleges.AddRange(cpsa, cnsa);
        await _db.SaveChangesAsync();
        _cpsaId = cpsa.Id;
        _cnsaId = cnsa.Id;
        _db.ChangeTracker.Clear();
    }

    public Task DisposeAsync() => _db.DisposeAsync().AsTask();

    // R3-B04, 6.1 refused: the short code CPSA names its holder, under Short code.
    [Fact]
    public async Task Create_WithATakenShortCode_IsRefusedUnderShortCode_NamingItsHolder_AndStagesNothing()
    {
        var act = () => Create("College of Cardiologists of South Africa", "CPSA");

        var refusal = (await act.Should().ThrowAsync<FieldRefusalException>()).Which;
        refusal.Field.Should().Be(nameof(CreateCollegeCommand.ShortCode));
        refusal.Message.Should().Be("CPSA is already the short code of College of Paediatricians of South Africa. Choose another.");
        _db.ChangeTracker.Entries().Should().BeEmpty("the check runs before the College is staged (T201)");
        (await _db.Colleges.CountAsync()).Should().Be(2);
    }

    // R3-B05: a taken name, under Name.
    [Fact]
    public async Task Create_WithATakenName_IsRefusedUnderName_NamingItAsACollegesName()
    {
        var act = () => Create("College of Paediatricians of South Africa", "CPSA2");

        var refusal = (await act.Should().ThrowAsync<FieldRefusalException>()).Which;
        refusal.Field.Should().Be(nameof(CreateCollegeCommand.Name));
        refusal.Message.Should().Be("College of Paediatricians of South Africa is already a College's name. Choose another.");
        _db.ChangeTracker.Entries().Should().BeEmpty();
    }

    // N7: the pre-check compares as the unique index does, ordinally: another case is another value.
    [Fact]
    public async Task Create_WithTheSameShortCodeInAnotherCase_IsNotRefusedByTheCheck()
    {
        var created = await Create("College of Cardiologists of South Africa", "cpsa");

        created.ShortCode.Should().Be("cpsa");
    }

    [Fact]
    public async Task Update_KeepingItsOwnNameAndCode_IsNotRefused_ButTakingAnothersIs()
    {
        var kept = await Update(_cnsaId, "College of Neurologists of South Africa", "CNSA");
        kept.Name.Should().Be("College of Neurologists of South Africa");
        _db.ChangeTracker.Clear();

        var act = () => Update(_cnsaId, "College of Neurologists of South Africa", "CPSA");

        var refusal = (await act.Should().ThrowAsync<FieldRefusalException>()).Which;
        refusal.Field.Should().Be(nameof(UpdateCollegeCommand.ShortCode));
        refusal.Message.Should().Be("CPSA is already the short code of College of Paediatricians of South Africa. Choose another.");
        _db.ChangeTracker.Entries<College>().Should().OnlyContain(entry => entry.State == EntityState.Unchanged,
            "nothing is assigned before the check, so the audit pipeline's save commits nothing (T201)");
        await _db.SaveChangesAsync();
        (await _db.Colleges.AsNoTracking().SingleAsync(college => college.Id == _cnsaId)).ShortCode.Should().Be("CNSA");
    }

    // The race: the index refused a value no College held when it was checked. Read again, it names its holder now.
    [Fact]
    public async Task Create_RefusedByTheUniqueIndex_NamesTheHolderTheSaveRaced_UnderItsField()
    {
        _failingSave.Next = new DbUpdateException("duplicate", new FakeDbException("23505"));
        _failingSave.Before = () =>
        {
            // Another administrator's save that won the race, through a second context on the same store.
            using var other = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(_store)
                .Options);
            other.Colleges.Add(new College { Name = "College of Cardiologists of South Africa", ShortCode = "CCSA", CreatedOn = DateTime.UtcNow });
            other.SaveChanges();
        };

        var act = () => Create("College of Cardiologists of South Africa", "CCSA2");

        var refusal = (await act.Should().ThrowAsync<FieldRefusalException>()).Which;
        refusal.Field.Should().Be(nameof(CreateCollegeCommand.Name));
        refusal.Message.Should().Be("College of Cardiologists of South Africa is already a College's name. Choose another.");
        refusal.InnerException.Should().BeOfType<DbUpdateException>("the database's refusal stays underneath, so the pipeline drops the save");
    }

    // T272: any other refused save is not called a duplicate.
    [Fact]
    public async Task Create_RefusedForAnotherReason_IsNotCalledADuplicate()
    {
        _failingSave.Next = new DbUpdateException("something else", new FakeDbException("23503"));

        var act = () => Create("College of Cardiologists of South Africa", "CCSA");

        var thrown = (await act.Should().ThrowAsync<Exception>()).Which;
        thrown.Should().NotBeOfType<FieldRefusalException>();
        thrown.Message.Should().NotContain("already");
    }

    [Fact]
    public async Task Update_RefusedForAnotherReason_IsNotCalledADuplicate()
    {
        _failingSave.Next = new DbUpdateException("something else", new FakeDbException("23503"));

        var act = () => Update(_cpsaId, "College of Paediatricians of South Africa", "CPSA");

        var thrown = (await act.Should().ThrowAsync<Exception>()).Which;
        thrown.Should().NotBeOfType<FieldRefusalException>();
        thrown.Message.Should().NotContain("already");
    }

    // T324, N1: each refusal in its field's words, never "'Short Code' must not be empty.".
    [Fact]
    public void TheValidators_SpeakTheFieldsWords()
    {
        var create = new CreateCollegeCommandValidator().Validate(
            new CreateCollegeCommand(" ", "", new string('d', 1001), TestPrincipals.Administrator()));
        create.Errors.Select(error => (error.PropertyName, error.ErrorMessage)).Should().BeEquivalentTo(new[]
        {
            ("Name", "Enter a name."),
            ("ShortCode", "Enter a short code."),
            ("Description", "A description is at most 1,000 characters.")
        });

        var update = new UpdateCollegeCommandValidator().Validate(
            new UpdateCollegeCommand(1, new string('n', 201), new string('c', 33), null, true, TestPrincipals.Administrator()));
        update.Errors.Select(error => (error.PropertyName, error.ErrorMessage)).Should().BeEquivalentTo(new[]
        {
            ("Name", "A name is at most 200 characters."),
            ("ShortCode", "A short code is at most 32 characters.")
        });
    }

    private Task<CollegeDto> Create(string name, string shortCode)
        => new CreateCollegeCommandHandler(_db).Handle(
            new CreateCollegeCommand(name, shortCode, null, TestPrincipals.Administrator()), CancellationToken.None);

    private Task<CollegeDto> Update(int id, string name, string shortCode)
        => new UpdateCollegeCommandHandler(_db).Handle(
            new UpdateCollegeCommand(id, name, shortCode, null, true, TestPrincipals.Administrator()), CancellationToken.None);

    /// <summary>Refuses the next save with the exception the test sets, as the database would, after <see cref="Before" />.</summary>
    private sealed class FailingSave : SaveChangesInterceptor
    {
        public Exception? Next { get; set; }

        public Action? Before { get; set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Next is { } next)
            {
                Next = null;
                Before?.Invoke();
                Before = null;
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
