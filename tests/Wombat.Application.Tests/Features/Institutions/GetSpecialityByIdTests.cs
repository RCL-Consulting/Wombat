using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Institutions.Queries.GetSpecialityById;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Institutions;

public sealed class GetSpecialityByIdTests
{
    [Fact]
    public async Task ReturnsSpeciality_ForOwningCollegeAdmin_NullForOthers()
    {
        await using var dbContext = CreateDbContext();
        var college = new College { Name = "C", ShortCode = "C" };
        dbContext.Set<College>().Add(college);
        await dbContext.SaveChangesAsync();
        var speciality = new Speciality { CollegeId = college.Id, Name = "Paediatrics" };
        dbContext.Set<Speciality>().Add(speciality);
        await dbContext.SaveChangesAsync();

        var handler = new GetSpecialityByIdQueryHandler(dbContext);

        (await handler.Handle(new GetSpecialityByIdQuery(speciality.Id, TestPrincipals.Administrator()), CancellationToken.None))
            !.CollegeId.Should().Be(college.Id);
        (await handler.Handle(new GetSpecialityByIdQuery(speciality.Id, TestPrincipals.CollegeAdmin(college.Id)), CancellationToken.None))
            .Should().NotBeNull();
        (await handler.Handle(new GetSpecialityByIdQuery(speciality.Id, TestPrincipals.CollegeAdmin(college.Id + 999)), CancellationToken.None))
            .Should().BeNull();
        (await handler.Handle(new GetSpecialityByIdQuery(999999, TestPrincipals.Administrator()), CancellationToken.None))
            .Should().BeNull();
    }

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
