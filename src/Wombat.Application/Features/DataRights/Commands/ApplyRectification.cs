using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights.Commands;

/// <summary>
/// Records one correction on an approved Rectification (T316; T387, Q2, Q26, S11): which of the requester's records was
/// changed (a <see cref="DataRightsRecords" /> key), its value before and after as typed, and who recorded it. The data
/// itself is changed on the page that owns it; this is the request's record that it was.
/// </summary>
/// <remarks>
/// <c>FromValue</c> and <c>ToValue</c> are redacted from the audit summary. The AuditPipelineBehavior audits every
/// request whose type name ends in "Command" and AuditPayloadSerializer writes its properties into SummaryJson; these two
/// ARE the personal data being corrected — the before value most of all, since it is the wrong or objected-to value the
/// subject asked to have changed. DataRightsRectification already persists both, under the data rights feature's own
/// access control, so the audit copy added nothing but a second, ungated one that a rectification can never reach.
/// RequestId and TargetType stay in the clear: they identify what was rectified without restating it. (T101)
/// The values arrive as typed and are stored as JSON strings (<c>JsonSerializer.Serialize(string)</c>), read back with
/// <c>Deserialize&lt;string&gt;</c> (T387, S11). Until T387 the command took JSON and a Guid target id.
/// </remarks>
public sealed record ApplyRectificationCommand(
    Guid RequestId,
    string TargetType,
    [property: Redact] string FromValue,
    [property: Redact] string ToValue,
    ClaimsPrincipal Principal) : IRequest<DataRightsCorrectionDto>;

public sealed class ApplyRectificationCommandValidator : AbstractValidator<ApplyRectificationCommand>
{
    public ApplyRectificationCommandValidator()
    {
        RuleFor(command => command.RequestId).NotEmpty();

        // One of DataRightsRecords' keys (T387, Q26, S11): a fixed record, or Other with a name of at most 194 characters,
        // which with "Other:" fills the column's 200. The messages are the page's own words for the same checks (R3-C04),
        // so the property names are what a page maps; it checks first and never sends these.
        RuleFor(command => command.TargetType)
            .Must(DataRightsRecords.IsValid)
            .WithMessage(command => TargetTypeMessage(command.TargetType));

        // Not blank after trim: the JSON of an empty string is not empty, so the check is on the value as typed (S11).
        RuleFor(command => command.FromValue)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Enter the value before.");
        RuleFor(command => command.ToValue)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Enter the value after.");
        RuleFor(command => command.Principal).NotNull();
    }

    private static string TargetTypeMessage(string? targetType)
    {
        if (targetType is null || !targetType.StartsWith(DataRightsRecords.OtherPrefix, StringComparison.Ordinal))
        {
            return "Choose the record.";
        }

        return string.IsNullOrWhiteSpace(DataRightsRecords.OtherName(targetType))
            ? "Name the record."
            : $"Name the record in {DataRightsRecords.OtherNameMaxLength} characters or fewer.";
    }
}

public sealed class ApplyRectificationCommandHandler : IRequestHandler<ApplyRectificationCommand, DataRightsCorrectionDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public ApplyRectificationCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<DataRightsCorrectionDto> Handle(ApplyRectificationCommand request, CancellationToken cancellationToken)
    {
        var actorUserId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        var dataRightsRequest = await _dbContext.Set<DataRightsRequest>()
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken)
            ?? throw new InvalidOperationException("Data rights request not found.");
        // T112: the gate runs AFTER the load, because it is the REQUEST's institution being checked,
        // not merely the caller's role. Refusal and not-found are both thrown before anything is
        // mutated, so a caller out of scope changes nothing.
        DataRightsAuthorization.DemandRectificationAccess(request.Principal, dataRightsRequest);

        if (dataRightsRequest.Type != DataRightsRequestType.Rectification ||
            dataRightsRequest.Status != DataRightsRequestStatus.Approved)
        {
            throw new DataRightsRefusedException(
                DataRightsRefusal.NotAnApprovedRectification,
                "A correction is recorded only on an approved Rectification request.");
        }

        // The recorder by name, read before anything is staged, so nothing that can fail runs after the save (T201). The
        // page reads the request again after a record; this answers what was recorded, and by whom.
        var names = await _users.GetDisplayNamesAsync([actorUserId], cancellationToken);
        var recorderName = names.TryGetValue(actorUserId, out var name) && !string.IsNullOrWhiteSpace(name) ? name : actorUserId;

        var utcNow = DateTime.UtcNow;
        var rectification = DataRightsRectification.Create(
            request.RequestId,
            request.TargetType,
            JsonSerializer.Serialize(request.FromValue),
            JsonSerializer.Serialize(request.ToValue),
            actorUserId,
            utcNow);

        _dbContext.Set<DataRightsRectification>().Add(rectification);

        // The request row goes into the save too, unchanged, so its xmin token (T387, F8) refuses a correction that races
        // Complete request in another tab, or the erasure of its requester (review S2): until the review, the insert alone
        // never compared the token, and a correction could land on a request completed, and mailed, a moment before.
        _dbContext.Set<DataRightsRequest>().Entry(dataRightsRequest).Property(r => r.Status).IsModified = true;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // As Complete request does: no longer an approved Rectification. Carried as the inner exception, so the audit
            // pipeline discards the refused correction and writes its row alone (T201).
            throw new DataRightsRefusedException(
                DataRightsRefusal.NotAnApprovedRectification,
                "A correction is recorded only on an approved Rectification request.",
                exception);
        }

        return new DataRightsCorrectionDto(
            rectification.Id,
            rectification.TargetType,
            request.FromValue,
            request.ToValue,
            rectification.AppliedByUserId,
            recorderName,
            rectification.AppliedOn);
    }
}
