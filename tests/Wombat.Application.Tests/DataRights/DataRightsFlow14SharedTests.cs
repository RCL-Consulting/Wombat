using FluentAssertions;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Features.DataRights;
using Wombat.Domain.DataRights;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.DataRights;

/// <summary>
/// What flow 14's build lanes share (T387, build-contracts § 0): the request's narrowed rules, the records a correction
/// names, the decision notice and its address overload, and the labels.
/// </summary>
public sealed class DataRightsFlow14SharedTests
{
    // ─── The request's rules (Q8, Q14, F3) ────────────────────────────────────

    /// <summary>Q14: Withdraw is the submitted request's alone; until T387 an approved or rejected one could be withdrawn.</summary>
    [Fact]
    public void ADecidedRequest_CannotBeWithdrawnByItsRequester()
    {
        var approved = Request(DataRightsRequestType.Rectification);
        approved.Approve("coord-1", "Agreed.", DateTime.UtcNow);
        var rejected = Request(DataRightsRequestType.Access);
        rejected.Reject("coord-1", "No.", DateTime.UtcNow);

        approved.Invoking(request => request.Withdraw()).Should().Throw<InvalidOperationException>();
        rejected.Invoking(request => request.Withdraw()).Should().Throw<InvalidOperationException>();
    }

    /// <summary>F3: an erasure withdraws the person's other open requests, an approved Rectification among them.</summary>
    [Fact]
    public void AnErasureWithdraws_ASubmittedRequest_AndAnApprovedRectification_ButNothingSettled()
    {
        var submitted = Request(DataRightsRequestType.Access);
        var inHand = Request(DataRightsRequestType.Rectification);
        inHand.Approve("coord-1", "Agreed.", DateTime.UtcNow);
        var completed = Request(DataRightsRequestType.Export);
        completed.Approve("coord-1", "Agreed.", DateTime.UtcNow);
        completed.Complete(DateTime.UtcNow);

        submitted.WithdrawOnErasure();
        inHand.WithdrawOnErasure();

        submitted.Status.Should().Be(DataRightsRequestStatus.Withdrawn);
        inHand.Status.Should().Be(DataRightsRequestStatus.Withdrawn);
        completed.IsOpen.Should().BeFalse();
        completed.Invoking(request => request.WithdrawOnErasure()).Should().Throw<InvalidOperationException>();
    }

    /// <summary>Q8: nothing is placed under review; the status kept its stored numbers when UnderReview left.</summary>
    [Fact]
    public void TheStatuses_KeepTheNumbersTheyWereStoredUnder()
    {
        Enum.GetValues<DataRightsRequestStatus>().Select(status => (int)status).Should().Equal(0, 2, 3, 4, 5);
        DataRightsLabels.Statuses.Should().BeEquivalentTo(Enum.GetValues<DataRightsRequestStatus>());
        DataRightsLabels.OfferedTypes.Should().NotContain(DataRightsRequestType.Objection);
    }

    // ─── The records (Q26, S11) ───────────────────────────────────────────────

    [Theory]
    [InlineData(DataRightsRecords.AccountFirstName, "Account › First name")]
    [InlineData(DataRightsRecords.AccountLastName, "Account › Last name")]
    [InlineData(DataRightsRecords.ProgrammeStart, "Trainee profile › Programme start")]
    [InlineData("Other:Home address", "Other › Home address")]
    public void ARecord_ReadsByItsLabel(string targetType, string label)
    {
        DataRightsRecords.IsValid(targetType).Should().BeTrue();
        DataRightsRecords.Label(targetType).Should().Be(label);
    }

    [Fact]
    public void OthersName_HoldsAtMost194Characters_SoTheKeyFitsItsColumn()
    {
        DataRightsRecords.IsValid(DataRightsRecords.Other(new string('a', 194))).Should().BeTrue();
        DataRightsRecords.Other(new string('a', 194)).Length.Should().Be(200);
        DataRightsRecords.IsValid("Other:" + new string('a', 195)).Should().BeFalse();
        DataRightsRecords.IsValid("Other:").Should().BeFalse();
        DataRightsRecords.IsValid("Account.Email").Should().BeFalse("no page changes an email (Q25)");
    }

    // ─── The notice (Q5, F4) ──────────────────────────────────────────────────

    [Fact]
    public void TheNotice_NamesTheTypeTheDayAndTheOutcome_AndNeitherTheNoteNorTheDecider()
    {
        var message = DataRightsDecisionEmail.Build(
            "dlamini@kgk.wombat.local", DataRightsRequestType.Export, DataRightsNoticeOutcome.Approved,
            new DateOnly(2026, 10, 28), "https://wombat.test/account/data-rights");

        message.Subject.Should().Be(DataRightsDecisionEmail.DecidedSubject);
        message.TextBody.Should().Contain("Your Export request was approved on 2026-10-28.")
            .And.Contain("https://wombat.test/account/data-rights");
    }

    [Fact]
    public void AnErasuresConfirmation_CarriesNoLink()
    {
        var message = DataRightsDecisionEmail.Build(
            "ndlovu@kgk.wombat.local", DataRightsRequestType.Erasure, DataRightsNoticeOutcome.Erased,
            new DateOnly(2026, 10, 28), "https://wombat.test/account/data-rights");

        message.Subject.Should().Be(DataRightsDecisionEmail.ErasedSubject);
        message.TextBody.Should().NotContain("https://");
        message.HtmlBody.Should().NotContain("href");
    }

    /// <summary>F4: the erased account has no address, so the confirmation goes to the one read before; never logged.</summary>
    [Fact]
    public async Task TheAddressOverload_SendsToTheAddressGiven_AndLogsNoAddress()
    {
        var notices = new TestNotices(FakeUserDirectory.Empty);

        var sent = await notices.Notices.SendToAddressAsync("ndlovu@kgk.wombat.local", "user-9", "data-rights-erased",
            address => DataRightsDecisionEmail.Build(address, DataRightsRequestType.Erasure,
                DataRightsNoticeOutcome.Erased, new DateOnly(2026, 10, 28), string.Empty));
        var skipped = await notices.Notices.SendToAddressAsync(null, "user-9", "data-rights-erased",
            _ => throw new InvalidOperationException("Nothing is built without an address."));

        sent.Should().BeTrue();
        skipped.Should().BeFalse();
        notices.Sender.Sent.Should().ContainSingle().Which.To.Should().Be("ndlovu@kgk.wombat.local");
        notices.Logger.Entries.Should().NotContain(entry => entry.Message.Contains('@'));
    }

    private static DataRightsRequest Request(DataRightsRequestType type)
        => DataRightsRequest.Create("subject-1", "Subject One", type, "Because.", DateTime.UtcNow, 1);
}
