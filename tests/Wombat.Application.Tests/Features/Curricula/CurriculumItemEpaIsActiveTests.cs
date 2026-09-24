using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T158: every path that hands the curriculum editor its items says whether each item's EPA is active, so the editor
/// can mark an item that is not in force.
/// </summary>
/// <remarks>
/// The editor reads items as records and lists a retired EPA's item with the rest. Since T158 that item is on no
/// progress page and takes no credit, so a projection that reported it as active would show the admin a live target
/// that no trainee sees. Three projections build the DTO: the list, the single read, and the one every item command
/// returns.
/// </remarks>
public sealed class CurriculumItemEpaIsActiveTests
{
    private const int CurriculumId = 3000;
    private const int ActiveEpaId = 5000;
    private const int RetiredEpaId = 5001;
    private const int ActiveItemId = 7000;
    private const int RetiredItemId = 7001;

    [Fact]
    public async Task TheSingleRead_MarksTheRetiredEpasItem()
    {
        await using var db = await SeededAsync();

        var curriculum = await new GetCurriculumByIdQueryHandler(db).Handle(
            new GetCurriculumByIdQuery(CurriculumId, TestPrincipals.Administrator()), CancellationToken.None);

        AssertMarked(curriculum!.Items);
    }

    [Fact]
    public async Task TheList_MarksTheRetiredEpasItem()
    {
        await using var db = await SeededAsync();

        var curricula = await new GetCurriculaListQueryHandler(db).Handle(
            new GetCurriculaListQuery(TestPrincipals.Administrator()), CancellationToken.None);

        AssertMarked(curricula.Single().Items);
    }

    [Fact]
    public async Task WhatAnItemCommandReturns_MarksTheRetiredEpasItem()
    {
        await using var db = await SeededAsync();

        var curriculum = await new UpdateCurriculumItemCommandHandler(db).Handle(
            new UpdateCurriculumItemCommand(
                CurriculumId, ActiveItemId, ActiveEpaId, 3, QuotaPeriod.Semester, 3, 12, null, null, null,
                TestPrincipals.Administrator()),
            CancellationToken.None);

        AssertMarked(curriculum.Items);
    }

    private static void AssertMarked(IReadOnlyList<CurriculumItemDto> items)
    {
        items.Single(item => item.Id == ActiveItemId).EpaIsActive.Should().BeTrue();
        items.Single(item => item.Id == RetiredItemId).EpaIsActive.Should().BeFalse(
            "its EPA is deactivated, so the item is not in force and the editor must say so");
    }

    private static async Task<ApplicationDbContext> SeededAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var college = new College { Id = 1, Name = "CPSA", ShortCode = "CPSA" };
        var speciality = new Speciality { Id = 1, Name = "Paediatrics", CollegeId = 1, College = college };
        var subSpeciality = new SubSpeciality { Id = 1, Name = "Paediatrics", SpecialityId = 1, Speciality = speciality };
        db.Colleges.Add(college);
        db.Specialities.Add(speciality);
        db.SubSpecialities.Add(subSpeciality);

        db.Epas.AddRange(
            new Epa { Id = ActiveEpaId, Code = "PAED-001", Title = "In force", SubSpecialityId = 1 },
            new Epa { Id = RetiredEpaId, Code = "PAED-002", Title = "Retired", SubSpecialityId = 1, IsActive = false });

        var curriculum = new Curriculum
        {
            Id = CurriculumId,
            SubSpecialityId = 1,
            SubSpeciality = subSpeciality,
            Name = "Paediatric EPA Curriculum",
            Version = "11.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        };
        curriculum.Items.Add(new CurriculumItem
        {
            Id = ActiveItemId, EpaId = ActiveEpaId, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 3, WindowMonths = 12
        });
        curriculum.Items.Add(new CurriculumItem
        {
            Id = RetiredItemId, EpaId = RetiredEpaId, RequiredCount = 1, QuotaPeriod = QuotaPeriod.AcademicYear,
            MinimumLevelOrder = 3, WindowMonths = 12
        });
        db.Curricula.Add(curriculum);
        await db.SaveChangesAsync();

        return db;
    }
}
