using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Institutions.Commands.CreateInstitution;
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
}
