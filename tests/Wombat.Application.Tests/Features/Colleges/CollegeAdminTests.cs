using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Colleges;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Colleges;

public sealed class CollegeAdminTests
{
    [Fact]
    public async Task Create_AsAdministrator_PersistsCollege()
    {
        await using var dbContext = CreateDbContext();

        var result = await new CreateCollegeCommandHandler(dbContext).Handle(
            new CreateCollegeCommand("College of Paediatricians", "FCPaed", "Paediatrics", TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Name.Should().Be("College of Paediatricians");
        (await dbContext.Set<College>().SingleAsync()).ShortCode.Should().Be("FCPaed");
    }

    [Fact]
    public async Task Create_AsNonAdministrator_Throws()
    {
        await using var dbContext = CreateDbContext();

        var act = () => new CreateCollegeCommandHandler(dbContext).Handle(
            new CreateCollegeCommand("X", "X", null, TestPrincipals.CollegeAdmin(1)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetById_CollegeAdmin_OtherCollege_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var college = new College { Name = "A", ShortCode = "A" };
        dbContext.Set<College>().Add(college);
        await dbContext.SaveChangesAsync();

        var handler = new GetCollegeByIdQueryHandler(dbContext);

        (await handler.Handle(new GetCollegeByIdQuery(college.Id, TestPrincipals.CollegeAdmin(college.Id)), CancellationToken.None))
            .Should().NotBeNull();
        (await handler.Handle(new GetCollegeByIdQuery(college.Id, TestPrincipals.CollegeAdmin(college.Id + 999)), CancellationToken.None))
            .Should().BeNull();
    }

    [Fact]
    public async Task List_CollegeAdmin_SeesOnlyOwnCollege()
    {
        await using var dbContext = CreateDbContext();
        var a = new College { Name = "A", ShortCode = "A" };
        var b = new College { Name = "B", ShortCode = "B" };
        dbContext.Set<College>().AddRange(a, b);
        await dbContext.SaveChangesAsync();

        var list = await new GetCollegesListQueryHandler(dbContext)
            .Handle(new GetCollegesListQuery(TestPrincipals.CollegeAdmin(a.Id)), CancellationToken.None);

        list.Should().ContainSingle().Which.Id.Should().Be(a.Id);
    }

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
