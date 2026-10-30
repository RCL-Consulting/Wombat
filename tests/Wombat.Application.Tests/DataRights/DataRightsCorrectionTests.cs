using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.Curricula;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.DataRights;

/// <summary>
/// Recording a correction on an approved Rectification and completing it (T316; T387, lane K, Q26, S11, R3-C01 to C10):
/// the record's key, the values as typed and stored as JSON strings, the recorder, the typed refusals, and the completion's
/// notice after the commit.
/// </summary>
public sealed class DataRightsCorrectionTests
{
    private const string RequesterId = "nomsa";
    private const string PieterId = "pieter";

    /// <summary>
    /// T387 (S11; CLAUDE.md's jsonb rule): a value is stored as the JSON of the string typed and read back with the parser,
    /// so what was typed comes back, compared canonical-to-canonical, never as raw text. "Mahlngu ›" is not ASCII: the
    /// serializer escapes it and Postgres would render it unescaped, and either text reads back the same.
    /// </summary>
    [Fact]
    public async Task Apply_StoresEachValueAsTheJsonOfTheStringTyped_AndTheRequestReadsItBack()
    {
        await using var db = CreateDb();
        var request = await AddApprovedRectificationAsync(db);
        var users = new FakeUserDirectory((PieterId, "Pieter Smit"));

        var recorded = await new ApplyRectificationCommandHandler(db, users).Handle(
            new ApplyRectificationCommand(request.Id, DataRightsRecords.AccountLastName, "Mahlngu ›", "Mahlangu", Coordinator()),
            CancellationToken.None);

        var stored = await db.Set<DataRightsRectification>().SingleAsync();
        Canonical(stored.FromValueJson).Should().Be(Canonical(JsonSerializer.Serialize("Mahlngu ›")));
        JsonSerializer.Deserialize<string>(stored.FromValueJson).Should().Be("Mahlngu ›");
        JsonSerializer.Deserialize<string>(stored.ToValueJson).Should().Be("Mahlangu");
        stored.AppliedByUserId.Should().Be(PieterId);
        stored.TargetType.Should().Be(DataRightsRecords.AccountLastName);

        recorded.FromValue.Should().Be("Mahlngu ›");
        recorded.AppliedByUserId.Should().Be(PieterId);
        recorded.AppliedByName.Should().Be("Pieter Smit");

        // What Postgres hands back is its own rendering, here the same string with the character unescaped.
        stored.FromValueJson = "\"Mahlngu ›\"";
        await db.SaveChangesAsync();
        var directory = new Mock<IUserAdministrationService>();
        directory.Setup(u => u.GetDisplayNamesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Returns((IReadOnlyCollection<string> ids, CancellationToken token) => users.GetDisplayNamesAsync(ids, token));
        directory.Setup(u => u.GetContactsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, UserContact>());
        directory.Setup(u => u.GetSignInMethodsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountSignInMethods?)null);
        var page = await new GetDataRightsRequestByIdQueryHandler(db, directory.Object).Handle(
            new GetDataRightsRequestByIdQuery(request.Id, Coordinator()), CancellationToken.None);
        page!.Corrections.Should().ContainSingle().Which.FromValue.Should().Be("Mahlngu ›");
    }

    /// <summary>T387 (Q26, S11, R3-C04): the record's key, Other's 194, and the values not blank, each in the page's words.</summary>
    [Theory]
    [InlineData(DataRightsRecords.AccountFirstName, "a", "b", null, null)]
    [InlineData(DataRightsRecords.ProgrammeStart, "2026-01-12", "2026-02-02", null, null)]
    [InlineData("Other:Home address", "a", "b", null, null)]
    [InlineData("", "a", "b", "TargetType", "Choose the record.")]
    [InlineData("Account.Email", "a", "b", "TargetType", "Choose the record.")]
    [InlineData("Other:", "a", "b", "TargetType", "Name the record.")]
    [InlineData("Other:   ", "a", "b", "TargetType", "Name the record.")]
    [InlineData(DataRightsRecords.AccountLastName, "   ", "b", "FromValue", "Enter the value before.")]
    [InlineData(DataRightsRecords.AccountLastName, "a", "", "ToValue", "Enter the value after.")]
    public void Apply_TheValidator_HoldsTheRecordAndTheValues(
        string targetType, string from, string to, string? property, string? message)
    {
        var result = new ApplyRectificationCommandValidator().Validate(
            new ApplyRectificationCommand(Guid.NewGuid(), targetType, from, to, Coordinator()));

        if (property is null)
        {
            result.IsValid.Should().BeTrue();
            return;
        }

        result.Errors.Should().ContainSingle().Which.Should().Match<FluentValidation.Results.ValidationFailure>(
            failure => failure.PropertyName == property && failure.ErrorMessage == message);
    }

    /// <summary>T387 (S11): Other's name holds 194 characters, the column's 200 less "Other:".</summary>
    [Fact]
    public void Apply_TheValidator_HoldsOthersNameTo194Characters()
    {
        var validator = new ApplyRectificationCommandValidator();

        validator.Validate(new ApplyRectificationCommand(
                Guid.NewGuid(), DataRightsRecords.Other(new string('x', 194)), "a", "b", Coordinator()))
            .IsValid.Should().BeTrue();
        validator.Validate(new ApplyRectificationCommand(
                Guid.NewGuid(), DataRightsRecords.OtherPrefix + new string('x', 195), "a", "b", Coordinator()))
            .Errors.Should().ContainSingle().Which.ErrorMessage.Should().Be("Name the record in 194 characters or fewer.");
    }

    /// <summary>T387 (C10): a correction on a request that is not an approved Rectification is refused, typed, nothing stored.</summary>
    [Fact]
    public async Task Apply_OnASubmittedRectification_IsRefusedTyped_AndStoresNothing()
    {
        await using var db = CreateDb();
        var request = AddRectification(db);
        await db.SaveChangesAsync();

        var act = () => new ApplyRectificationCommandHandler(db, FakeUserDirectory.Empty).Handle(
            new ApplyRectificationCommand(request.Id, DataRightsRecords.AccountLastName, "a", "b", Coordinator()),
            CancellationToken.None);

        (await act.Should().ThrowAsync<DataRightsRefusedException>())
            .Which.Reason.Should().Be(DataRightsRefusal.NotAnApprovedRectification);
        (await db.Set<DataRightsRectification>().CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// T316, T387 (R3-C03): Complete request with no correction recorded is refused in the board's words, typed, and the
    /// request stays Approved; nobody is mailed.
    /// </summary>
    [Fact]
    public async Task Complete_WithNoCorrectionRecorded_IsRefused_AndMailsNobody()
    {
        await using var db = CreateDb();
        var request = await AddApprovedRectificationAsync(db);
        var notices = new TestNotices(RequesterDirectory());

        var act = () => new CompleteRectificationRequestCommandHandler(db, notices.Notices).Handle(
            new CompleteRectificationRequestCommand(request.Id, Coordinator()), CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<DataRightsRefusedException>()).Which;
        refusal.Reason.Should().Be(DataRightsRefusal.NoCorrectionRecorded);
        refusal.Message.Should().Be("Record at least one correction before completing the request.");
        (await db.Set<DataRightsRequest>().SingleAsync()).Status.Should().Be(DataRightsRequestStatus.Approved);
        notices.Sender.Sent.Should().BeEmpty();
    }

    /// <summary>T387: Complete on a request that is not an approved Rectification is refused, typed, before anything else.</summary>
    [Fact]
    public async Task Complete_OnASubmittedRectification_IsRefusedTyped()
    {
        await using var db = CreateDb();
        var request = AddRectification(db);
        await db.SaveChangesAsync();
        var notices = new TestNotices(RequesterDirectory());

        var act = () => new CompleteRectificationRequestCommandHandler(db, notices.Notices).Handle(
            new CompleteRectificationRequestCommand(request.Id, Coordinator()), CancellationToken.None);

        (await act.Should().ThrowAsync<DataRightsRefusedException>())
            .Which.Reason.Should().Be(DataRightsRefusal.NotAnApprovedRectification);
        notices.Sender.Sent.Should().BeEmpty();
    }

    /// <summary>
    /// T387 (C06, T2): completing sends the requester the Completed notice after the commit: the type, the South African
    /// day and the link to My data rights; never who completed it, nor the values.
    /// </summary>
    [Fact]
    public async Task Complete_WithACorrection_CompletesIt_AndSendsTheCompletedNotice()
    {
        await using var db = CreateDb();
        var request = await AddApprovedRectificationAsync(db);
        db.Set<DataRightsRectification>().Add(DataRightsRectification.Create(
            request.Id, DataRightsRecords.AccountLastName, "\"Mahlngu\"", "\"Mahlangu\"", PieterId, DateTime.UtcNow));
        await db.SaveChangesAsync();
        var notices = new TestNotices(RequesterDirectory());

        var result = await new CompleteRectificationRequestCommandHandler(db, notices.Notices).Handle(
            new CompleteRectificationRequestCommand(request.Id, Coordinator()), CancellationToken.None);

        result.Status.Should().Be(DataRightsRequestStatus.Completed);
        var mail = notices.Sender.Sent.Should().ContainSingle().Which;
        mail.To.Should().Be("mahlangu@kgk.wombat.local");
        mail.Subject.Should().Be(DataRightsDecisionEmail.CompletedSubject);
        var day = ProgrammeCalendar.DateOf(result.CompletedOn!.Value).ToString("yyyy-MM-dd");
        mail.TextBody.Should().Contain($"Your Rectification request was completed on {day}.")
            .And.Contain("https://wombat.test/account/data-rights")
            .And.NotContain("Mahlngu").And.NotContain("Pieter");
    }

    /// <summary>
    /// T387 (F1): the stored name is the account's "First Last", read from the directory; until T387 it was the sign-in's
    /// name claim, the address. An account with no name stores its address.
    /// </summary>
    [Theory]
    [InlineData("Nomsa", "Mahlangu", "Nomsa Mahlangu")]
    [InlineData("", "", "mahlangu@kgk.wombat.local")]
    public async Task Submit_StoresTheAccountsFirstAndLastName(string first, string last, string stored)
    {
        await using var db = CreateDb();
        var users = new FakeUserDirectory().With(
            new UserIdentityDetails(RequesterId, "mahlangu@kgk.wombat.local", first, last, 1, [], [], [WombatRoles.Trainee]));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, RequesterId),
                new Claim(ClaimTypes.Name, "mahlangu@kgk.wombat.local"),
                new Claim(WombatClaimTypes.InstitutionId, "1")
            ], "test"));

        var result = await new SubmitDataRightsRequestCommandHandler(db, users).Handle(
            new SubmitDataRightsRequestCommand(DataRightsRequestType.Rectification, "My surname is misspelt.", principal),
            CancellationToken.None);

        result.RequesterDisplayName.Should().Be(stored);
        (await db.Set<DataRightsRequest>().SingleAsync()).RequesterDisplayName.Should().Be(stored);
    }

    /// <summary>T387 (Q8): Objection is not a request a person makes; the four offered types are.</summary>
    [Fact]
    public void Submit_TheValidator_RefusesObjection_AndTakesTheFourOfferedTypes()
    {
        var validator = new SubmitDataRightsRequestCommandValidator();

        validator.Validate(new SubmitDataRightsRequestCommand(DataRightsRequestType.Objection, "Reason", Coordinator()))
            .Errors.Should().ContainSingle().Which.PropertyName.Should().Be("Type");
        foreach (var type in DataRightsLabels.OfferedTypes)
        {
            validator.Validate(new SubmitDataRightsRequestCommand(type, "Reason", Coordinator())).IsValid.Should().BeTrue();
        }
    }

    /// <summary>
    /// T387 (Q14): only a request waiting for a decision is withdrawn; one decided meanwhile is refused, typed, and nothing
    /// changes. Until T387 an approved Rectification, or a rejected request, could be withdrawn by the command.
    /// </summary>
    [Theory]
    [InlineData(DataRightsRequestStatus.Approved)]
    [InlineData(DataRightsRequestStatus.Rejected)]
    [InlineData(DataRightsRequestStatus.Withdrawn)]
    public async Task Withdraw_ARequestNoLongerSubmitted_IsRefusedAsDecidedMeanwhile(DataRightsRequestStatus status)
    {
        await using var db = CreateDb();
        var request = AddRectification(db);
        switch (status)
        {
            case DataRightsRequestStatus.Approved: request.Approve(PieterId, "Approved.", DateTime.UtcNow); break;
            case DataRightsRequestStatus.Rejected: request.Reject(PieterId, "No.", DateTime.UtcNow); break;
            default: request.Withdraw(); break;
        }

        await db.SaveChangesAsync();
        var requester = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, RequesterId)], "test"));

        var act = () => new WithdrawDataRightsRequestCommandHandler(db).Handle(
            new WithdrawDataRightsRequestCommand(request.Id, requester), CancellationToken.None);

        (await act.Should().ThrowAsync<DataRightsRefusedException>())
            .Which.Reason.Should().Be(DataRightsRefusal.DecidedMeanwhile);
        db.ChangeTracker.HasChanges().Should().BeFalse("the refusal is raised before anything is staged");
        (await db.Set<DataRightsRequest>().AsNoTracking().SingleAsync()).Status.Should().Be(status);
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static DataRightsRequest AddRectification(ApplicationDbContext db)
    {
        var request = DataRightsRequest.Create(
            RequesterId, "Nomsa Mahlangu", DataRightsRequestType.Rectification, "My surname", DateTime.UtcNow.AddHours(-1), 1);
        db.Set<DataRightsRequest>().Add(request);
        return request;
    }

    private static async Task<DataRightsRequest> AddApprovedRectificationAsync(ApplicationDbContext db)
    {
        var request = AddRectification(db);
        request.Approve(PieterId, "Approved.", DateTime.UtcNow.AddMinutes(-30));
        await db.SaveChangesAsync();
        return request;
    }

    private static IUserAdministrationService RequesterDirectory() => new FakeUserDirectory().With(
        new UserIdentityDetails(RequesterId, "mahlangu@kgk.wombat.local", "Nomsa", "Mahlangu", 1, [], [], [WombatRoles.Trainee]));

    private static string Canonical(string json) => JsonSerializer.Serialize(JsonDocument.Parse(json).RootElement);

    private static ClaimsPrincipal Coordinator() => new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, PieterId),
            new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
            new Claim(WombatClaimTypes.InstitutionId, "1")
        ], "test"));
}
