using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Forms;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Epas;
using Wombat.Domain.Forms;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Forms;

/// <summary>
/// T056.c scope-guard tests for Forms. InstitutionalAdmin sees their own institution's forms
/// (and Global forms read-only) and cannot edit Global or other-institution forms.
/// </summary>
public sealed class AssessmentFormScopeGuardTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private int _institutionAId;
    private int _institutionBId;
    private int _institutionBFormId;
    private int _globalFormId;
    private int _scaleId;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        var institutionA = new Institution { Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow };
        var institutionB = new Institution { Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow };
        _db.Institutions.AddRange(institutionA, institutionB);

        var scale = new EntrustmentScale { Name = "Default scale" };
        scale.Levels.Add(new EntrustmentLevel { Order = 1, Label = "L1" });
        scale.Levels.Add(new EntrustmentLevel { Order = 2, Label = "L2" });
        _db.Set<EntrustmentScale>().Add(scale);
        await _db.SaveChangesAsync();

        var formA = new AssessmentForm { Name = "FormA", InstitutionId = institutionA.Id, ScaleId = scale.Id, CanDelete = true, IsActive = true };
        var formB = new AssessmentForm { Name = "FormB", InstitutionId = institutionB.Id, ScaleId = scale.Id, CanDelete = true, IsActive = true };
        var formGlobal = new AssessmentForm { Name = "FormGlobal", ScaleId = scale.Id, CanDelete = true, IsActive = true };
        _db.Set<AssessmentForm>().AddRange(formA, formB, formGlobal);
        await _db.SaveChangesAsync();

        _institutionAId = institutionA.Id;
        _institutionBId = institutionB.Id;
        _institutionBFormId = formB.Id;
        _globalFormId = formGlobal.Id;
        _scaleId = scale.Id;
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ListForms_InstitutionalAdmin_SeesOwnAndGlobal()
    {
        var handler = new GetAssessmentFormsListQueryHandler(_db);
        var result = await handler.Handle(
            new GetAssessmentFormsListQuery(TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);

        // FormA + FormGlobal; FormB excluded.
        result.Select(f => f.Name).Should().BeEquivalentTo(new[] { "FormA", "FormGlobal" });
    }

    [Fact]
    public async Task GetFormById_InstitutionalAdmin_OtherInstitution_ReturnsNull()
    {
        var handler = new GetAssessmentFormByIdQueryHandler(_db);
        var result = await handler.Handle(
            new GetAssessmentFormByIdQuery(_institutionBFormId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateForm_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var handler = new UpdateAssessmentFormCommandHandler(_db);
        var act = () => handler.Handle(
            new UpdateAssessmentFormCommand(_institutionBFormId, "FormB renamed", _institutionBId, null, null, _scaleId, true, true, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateForm_InstitutionalAdmin_RejectsGlobalForm()
    {
        var handler = new UpdateAssessmentFormCommandHandler(_db);
        var act = () => handler.Handle(
            new UpdateAssessmentFormCommand(_globalFormId, "FormGlobal renamed", null, null, null, _scaleId, true, true, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task CreateForm_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var handler = new CreateAssessmentFormCommandHandler(_db);
        var act = () => handler.Handle(
            new CreateAssessmentFormCommand("New form", _institutionBId, null, null, _scaleId, true, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
