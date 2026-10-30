using FluentValidation;
using MediatR;
using System.Security.Claims;
using Wombat.Application.Audit;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights.Commands;

/// <remarks>
/// <c>Reason</c> is redacted from the audit summary. The AuditPipelineBehavior audits every request
/// whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson; this is the data subject stating, in their own words and up to 4000 characters, why
/// they want their data erased or corrected — health, harassment, a dispute. Copying it into a
/// general-purpose admin table is the opposite of what the request asks for. <c>Type</c> stays in
/// the clear so the log still shows which right was exercised and when. (T101)
/// </remarks>
public sealed record SubmitDataRightsRequestCommand(
    DataRightsRequestType Type,
    [property: Redact] string Reason,
    ClaimsPrincipal Principal) : IRequest<DataRightsRequestDto>;

public sealed class SubmitDataRightsRequestCommandValidator : AbstractValidator<SubmitDataRightsRequestCommand>
{
    public SubmitDataRightsRequestCommandValidator()
    {
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
        // Objection is not a request a person makes (T387, Q8): My data rights offers four types, and so does the command.
        RuleFor(command => command.Type)
            .Must(type => DataRightsLabels.OfferedTypes.Contains(type))
            .WithMessage("Choose the type of request.");
    }
}

public sealed class SubmitDataRightsRequestCommandHandler : IRequestHandler<SubmitDataRightsRequestCommand, DataRightsRequestDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public SubmitDataRightsRequestCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<DataRightsRequestDto> Handle(SubmitDataRightsRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        // The account's "First Last" (T387, F1): until T387 the sign-in's name claim, which is the address, so the stored
        // name was an address. The pages name a living requester live; this stored name is what the record keeps, and the
        // erasure writes the pseudonym over it (F3). The address only for an account with no name; the claims only for an
        // account the directory does not hold.
        var account = await _users.GetByIdAsync(userId, cancellationToken);
        var accountName = account is null ? string.Empty : $"{account.FirstName} {account.LastName}".Trim();
        var displayName = accountName.Length > 0
            ? accountName
            : account is { Email: { Length: > 0 } email }
                ? email
                : request.Principal.FindFirst(ClaimTypes.Name)?.Value
                  ?? request.Principal.FindFirst(ClaimTypes.Email)?.Value
                  ?? userId;

        // No committee review refuses an erasure request (T258 review). T026 refused one while a review of the requester
        // was scheduled, in progress, decided or under appeal, because an erasure then left the review running under the
        // pseudonym. Since T258 approving the erasure ends what is open (ErasureExecutor withdraws an open review) and
        // keeps what is settled (a review under appeal stays answerable), so the refusal only contradicted it: a request
        // made before a review was scheduled withdrew it, and one made a day after was refused for the same review.
        // Whether the person may be erased is the approver's decision.
        var utcNow = DateTime.UtcNow;
        var entity = DataRightsRequest.Create(
            userId,
            displayName,
            request.Type,
            request.Reason,
            utcNow,
            // T112: the submitter IS the data subject, so their own claim is the authoritative answer
            // to "where does this request belong" — no lookup, and nothing to go stale. Null when they
            // carry no institution claim, which leaves the request Administrator-only.
            request.Principal.GetInstitutionId());

        _dbContext.Set<DataRightsRequest>().Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    private static DataRightsRequestDto ToDto(DataRightsRequest entity) => new(
        entity.Id,
        entity.RequesterUserId,
        entity.RequesterDisplayName,
        entity.RequestedOn,
        entity.Type,
        entity.Status,
        entity.Reason,
        entity.DecisionNote,
        entity.DecidedByUserId,
        entity.DecidedOn,
        entity.CompletedOn);
}
