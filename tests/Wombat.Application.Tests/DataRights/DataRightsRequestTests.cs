using FluentAssertions;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Tests.DataRights;

public sealed class DataRightsRequestTests
{
    [Fact]
    public void Create_SetsInitialState()
    {
        var utcNow = DateTime.UtcNow;
        var request = DataRightsRequest.Create("user-1", "Test User", DataRightsRequestType.Erasure, "Please erase my data.", utcNow);

        request.Id.Should().NotBeEmpty();
        request.RequesterUserId.Should().Be("user-1");
        request.RequesterDisplayName.Should().Be("Test User");
        request.Type.Should().Be(DataRightsRequestType.Erasure);
        request.Status.Should().Be(DataRightsRequestStatus.Submitted);
        request.RequestedOn.Should().Be(utcNow);
    }

    [Fact]
    public void Approve_FromSubmitted_SetsApproved()
    {
        var request = DataRightsRequest.Create("user-1", "Test User", DataRightsRequestType.Access, "I want my data.", DateTime.UtcNow);
        var decisionTime = DateTime.UtcNow;

        request.Approve("admin-1", "Approved per policy.", decisionTime);

        request.Status.Should().Be(DataRightsRequestStatus.Approved);
        request.DecidedByUserId.Should().Be("admin-1");
        request.DecisionNote.Should().Be("Approved per policy.");
        request.DecidedOn.Should().Be(decisionTime);
    }

    [Fact]
    public void Reject_FromSubmitted_SetsRejected()
    {
        var request = DataRightsRequest.Create("user-1", "Test User", DataRightsRequestType.Erasure, "Erase me.", DateTime.UtcNow);

        request.Reject("admin-1", "Active training obligation.", DateTime.UtcNow);

        request.Status.Should().Be(DataRightsRequestStatus.Rejected);
    }

    [Fact]
    public void Complete_FromApproved_SetsCompleted()
    {
        var request = DataRightsRequest.Create("user-1", "Test User", DataRightsRequestType.Access, "Data.", DateTime.UtcNow);
        request.Approve("admin-1", "OK.", DateTime.UtcNow);

        request.Complete(DateTime.UtcNow);

        request.Status.Should().Be(DataRightsRequestStatus.Completed);
        request.CompletedOn.Should().NotBeNull();
    }

    [Fact]
    public void Complete_FromNonApproved_Throws()
    {
        var request = DataRightsRequest.Create("user-1", "Test User", DataRightsRequestType.Access, "Data.", DateTime.UtcNow);

        var act = () => request.Complete(DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Withdraw_FromSubmitted_SetsWithdrawn()
    {
        var request = DataRightsRequest.Create("user-1", "Test User", DataRightsRequestType.Export, "Export.", DateTime.UtcNow);

        request.Withdraw();

        request.Status.Should().Be(DataRightsRequestStatus.Withdrawn);
    }

    [Fact]
    public void Withdraw_FromCompleted_Throws()
    {
        var request = DataRightsRequest.Create("user-1", "Test User", DataRightsRequestType.Access, "Data.", DateTime.UtcNow);
        request.Approve("admin-1", "OK.", DateTime.UtcNow);
        request.Complete(DateTime.UtcNow);

        var act = () => request.Withdraw();

        act.Should().Throw<InvalidOperationException>();
    }
}
