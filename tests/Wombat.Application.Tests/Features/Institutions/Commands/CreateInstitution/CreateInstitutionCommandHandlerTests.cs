using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Institutions.Commands.CreateInstitution;
using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;
using Wombat.Domain.Institutions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Institutions.Commands.CreateInstitution;

public sealed class CreateInstitutionCommandHandlerTests
{
    [Fact]
    public async Task Handle_PersistsInstitutionAndReturnsDto()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        var handler = new CreateInstitutionCommandHandler(dbContext);

        var result = await handler.Handle(
            new CreateInstitutionCommand("Colleges of Medicine", "CMSA", "admin@example.test", TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Colleges of Medicine");
        result.ShortCode.Should().Be("CMSA");
        result.ContactEmail.Should().Be("admin@example.test");

        var savedInstitution = await dbContext.Institutions.SingleAsync();
        savedInstitution.Name.Should().Be("Colleges of Medicine");
        savedInstitution.ShortCode.Should().Be("CMSA");
        savedInstitution.ContactEmail.Should().Be("admin@example.test");
        savedInstitution.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_RejectsCreationByInstitutionalAdmin()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        var handler = new CreateInstitutionCommandHandler(dbContext);

        var act = () => handler.Handle(
            new CreateInstitutionCommand("Other University", "OU", null, TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        (await dbContext.Institutions.AnyAsync()).Should().BeFalse();
    }

    /// <summary>
    /// T400 (flow 18, R3-I09, T378): a short code another institution holds is refused under Short code, a name under
    /// Name, in the record's words, before anything is staged, so the audit's failure row carries the refusal alone.
    /// </summary>
    [Theory]
    [InlineData("Another Hospital", "KGK", InstitutionRecordField.ShortCode)]
    [InlineData("Kgosi Kgari Teaching Hospital", "NEW", InstitutionRecordField.Name)]
    [InlineData(" Kgosi Kgari Teaching Hospital ", " KGK ", InstitutionRecordField.ShortCode)]
    public async Task Handle_RefusesADuplicateUnderItsField_BeforeStagingAnything(
        string name, string shortCode, InstitutionRecordField field)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        dbContext.Institutions.Add(new Institution { Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" });
        await dbContext.SaveChangesAsync();
        var handler = new CreateInstitutionCommandHandler(dbContext, TimeProvider.System);

        var act = () => handler.Handle(
            new CreateInstitutionCommand(name, shortCode, null, TestPrincipals.Administrator()), CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<InstitutionUpdateRefusedException>()).Which;
        refusal.Reason.Should().Be(InstitutionUpdateRefusal.Duplicate);
        refusal.Field.Should().Be(field);
        refusal.Message.Should().Be("An institution with the same name or short code already exists.");
        InstitutionUpdateRefusals.IsRefusal(refusal).Should().BeTrue();
        dbContext.ChangeTracker.Entries<Institution>().Should().OnlyContain(entry => entry.State == EntityState.Unchanged,
            "nothing is staged before a refusal (AuditPipelineBehavior commits a failed handler's staged rows, T201)");
        (await dbContext.Institutions.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_StampsTheMomentByTheClock()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var now = new DateTime(2026, 11, 10, 22, 30, 0, DateTimeKind.Utc);

        await using var dbContext = new ApplicationDbContext(options);
        var handler = new CreateInstitutionCommandHandler(dbContext, new FixedClock(now));

        var result = await handler.Handle(
            new CreateInstitutionCommand("Kgosi Kgari Teaching Hospital", "KGK", null, TestPrincipals.Administrator()),
            CancellationToken.None);

        result.CreatedOn.Should().Be(now);
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
