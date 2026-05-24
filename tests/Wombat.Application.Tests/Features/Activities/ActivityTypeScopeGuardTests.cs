using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Commands.DiscardActivityTypeDraft;
using Wombat.Application.Features.Activities.Commands.PublishActivityTypeDraft;
using Wombat.Application.Features.Activities.Commands.SaveActivityTypeDraft;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.ListActivityTypesAdmin;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

/// <summary>
/// T056.c scope-guard tests: InstitutionalAdmin sees only activity types whose scope (Global
/// or rooted in their institution) is accessible, and cannot write to Global-scoped types.
/// </summary>
public sealed class ActivityTypeScopeGuardTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private int _institutionAId;
    private int _institutionBId;
    private int _institutionAActivityTypeId;
    private int _institutionBActivityTypeId;
    private int _globalActivityTypeId;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        var institutionA = new Institution { Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow };
        var institutionB = new Institution { Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow };
        _db.Institutions.AddRange(institutionA, institutionB);
        await _db.SaveChangesAsync();

        var typeA = new ActivityType { Key = "type-a", Name = "TypeA", Scope = ActivityScope.Institution, ScopeId = institutionA.Id, IsActive = true, OwnerUserId = "u1", CreatedOn = DateTime.UtcNow };
        var typeB = new ActivityType { Key = "type-b", Name = "TypeB", Scope = ActivityScope.Institution, ScopeId = institutionB.Id, IsActive = true, OwnerUserId = "u1", CreatedOn = DateTime.UtcNow };
        var typeGlobal = new ActivityType { Key = "type-global", Name = "TypeGlobal", Scope = ActivityScope.Global, IsActive = true, OwnerUserId = "u1", CreatedOn = DateTime.UtcNow };
        _db.Set<ActivityType>().AddRange(typeA, typeB, typeGlobal);
        await _db.SaveChangesAsync();

        _institutionAId = institutionA.Id;
        _institutionBId = institutionB.Id;
        _institutionAActivityTypeId = typeA.Id;
        _institutionBActivityTypeId = typeB.Id;
        _globalActivityTypeId = typeGlobal.Id;
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ListActivityTypesAdmin_InstitutionalAdmin_SeesGlobalAndOwnInstitution()
    {
        var handler = new ListActivityTypesAdminQueryHandler(_db);
        var result = await handler.Handle(
            new ListActivityTypesAdminQuery(TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);

        result.Select(x => x.Id).Should().BeEquivalentTo(new[] { _institutionAActivityTypeId, _globalActivityTypeId });
    }

    [Fact]
    public async Task GetActivityTypeEditor_InstitutionalAdmin_OtherInstitution_Throws()
    {
        var handler = new GetActivityTypeEditorQueryHandler(_db);
        var act = () => handler.Handle(
            new GetActivityTypeEditorQuery(_institutionBActivityTypeId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveActivityTypeDraft_InstitutionalAdmin_RejectsOtherInstitutionScope()
    {
        var handler = new SaveActivityTypeDraftCommandHandler(_db);
        var act = () => handler.Handle(
            new SaveActivityTypeDraftCommand(
                _institutionBActivityTypeId,
                "type-b",
                "TypeB renamed",
                null,
                ActivityScope.Institution,
                _institutionBId,
                true,
                "{}",
                "{}",
                "{}",
                "[]",
                "actor",
                TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SaveActivityTypeDraft_InstitutionalAdmin_RejectsGlobalScope()
    {
        var handler = new SaveActivityTypeDraftCommandHandler(_db);
        var act = () => handler.Handle(
            new SaveActivityTypeDraftCommand(
                _globalActivityTypeId,
                "type-global",
                "TypeGlobal renamed",
                null,
                ActivityScope.Global,
                null,
                true,
                "{}",
                "{}",
                "{}",
                "[]",
                "actor",
                TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DiscardActivityTypeDraft_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var handler = new DiscardActivityTypeDraftCommandHandler(_db);
        var act = () => handler.Handle(
            new DiscardActivityTypeDraftCommand(_institutionBActivityTypeId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
