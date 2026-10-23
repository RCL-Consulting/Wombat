using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Institutions;

/// <summary>
/// T376 (flow 12, I03, I04, I07, I08): her institution's record refuses beside the field that is wrong, by a typed
/// field, never by the message's text, and in Wombat's words (T324).
/// </summary>
public sealed class InstitutionRecordRefusalTests
{
    private const int Kgk = 2;

    [Fact]
    public async Task AShortCodeAnotherInstitutionHolds_IsRefusedUnderShortCode_InTheBuiltWords_AndNothingChanges()
    {
        await using var db = CreateDb();

        var act = () => Update(db, "Kgosi Kgari Teaching Hospital", "DEMO", null);

        var refusal = (await act.Should().ThrowAsync<InstitutionUpdateRefusedException>()).Which;
        refusal.Reason.Should().Be(InstitutionUpdateRefusal.Duplicate);
        refusal.Field.Should().Be(InstitutionRecordField.ShortCode);
        refusal.Message.Should().Be("An institution with the same name or short code already exists.");
        refusal.Message.Should().NotContain("Demo", "the refusal names no other institution");
        (await Stored(db)).ShortCode.Should().Be("KGK");
    }

    [Fact]
    public async Task ANameAnotherInstitutionHolds_IsRefusedUnderName()
    {
        await using var db = CreateDb();

        var act = () => Update(db, " Demo Institution ", "KGK", null);

        (await act.Should().ThrowAsync<InstitutionUpdateRefusedException>()).Which.Field.Should().Be(InstitutionRecordField.Name);
    }

    [Fact]
    public async Task KeepingItsOwnNameAndShortCode_IsNoClash_AndTheEmailSaves()
    {
        await using var db = CreateDb();

        var saved = await Update(db, "Kgosi Kgari Teaching Hospital", "KGK", " hod.paediatrics@kgk.wombat.local ");

        saved.ContactEmail.Should().Be("hod.paediatrics@kgk.wombat.local");
    }

    [Fact]
    public async Task AClearedEmail_IsStoredAsNone()
    {
        await using var db = CreateDb();

        var saved = await Update(db, "Kgosi Kgari Teaching Hospital", "KGK", "  ");

        saved.ContactEmail.Should().BeNull("I07: \"It has no contact email now.\"");
    }

    [Fact]
    public async Task AnUnknownInstitution_IsATypedRefusal_OnNoField()
    {
        await using var db = CreateDb();

        var act = () => new UpdateInstitutionCommandHandler(db).Handle(
            new UpdateInstitutionCommand(404, "X", "X", null, TestPrincipals.Administrator()), CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<InstitutionUpdateRefusedException>()).Which;
        refusal.Reason.Should().Be(InstitutionUpdateRefusal.NotFound);
        refusal.Field.Should().Be(InstitutionRecordField.None);
    }

    // ─── The validator's words (T324) ───────────────────────────────────────

    [Fact]
    public void AnEmptyShortCode_IsRefusedInWombatsWords()
    {
        new UpdateInstitutionCommandValidator().TestValidate(Command("KGK hospital", "", null))
            .ShouldHaveValidationErrorFor(command => command.ShortCode)
            .WithErrorMessage("Type a short code. The record was not saved.");
    }

    [Fact]
    public void AnEmptyName_IsRefusedInWombatsWords()
    {
        new UpdateInstitutionCommandValidator().TestValidate(Command("", "KGK", null))
            .ShouldHaveValidationErrorFor(command => command.Name)
            .WithErrorMessage(UpdateInstitutionCommandValidator.NameMissing);
    }

    [Fact]
    public void AMalformedEmail_IsRefused_AndNoEmailIsAllowed()
    {
        var validator = new UpdateInstitutionCommandValidator();

        validator.TestValidate(Command("KGK hospital", "KGK", "not-an-address"))
            .ShouldHaveValidationErrorFor(command => command.ContactEmail)
            .WithErrorMessage("Enter a valid email address, for example name@example.org.");
        validator.TestValidate(Command("KGK hospital", "KGK", null)).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(Command("KGK hospital", "KGK", "")).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void NoRefusal_NamesAProperty()
    {
        var result = new UpdateInstitutionCommandValidator().TestValidate(Command("", "", "x"));

        result.Errors.Select(error => error.ErrorMessage).Should().NotContain(message =>
            message.Contains("'Name'") || message.Contains("'Short Code'") || message.Contains("Contact Email"));
    }

    private static UpdateInstitutionCommand Command(string name, string shortCode, string? email)
        => new(Kgk, name, shortCode, email, TestPrincipals.InstitutionalAdmin(Kgk));

    private static Task<Wombat.Application.Features.Institutions.InstitutionDto> Update(
        ApplicationDbContext db, string name, string shortCode, string? email)
        => new UpdateInstitutionCommandHandler(db).Handle(Command(name, shortCode, email), CancellationToken.None);

    private static async Task<Institution> Stored(ApplicationDbContext db)
        => await db.Institutions.AsNoTracking().SingleAsync(entity => entity.Id == Kgk);

    private static ApplicationDbContext CreateDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Institutions.AddRange(
            new Institution { Id = 1, Name = "Demo Institution", ShortCode = "DEMO" },
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", ContactEmail = "paeds-admin@kgk.wombat.local" });
        db.SaveChanges();
        return db;
    }
}
