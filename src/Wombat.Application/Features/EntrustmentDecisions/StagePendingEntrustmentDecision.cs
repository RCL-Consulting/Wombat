using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.EntrustmentDecisions;

public sealed record StagePendingEntrustmentDecisionCommand(
    int ReviewId,
    int? PendingId,
    int EpaId,
    int AuthorisedLevelId,
    DateOnly IssuedOn,
    DateOnly? ExpiresOn,
    string Rationale,
    IReadOnlyList<EntrustmentEvidenceLinkInput> EvidenceLinks,
    ClaimsPrincipal Principal) : IRequest<PendingEntrustmentDecisionDto>;

public sealed class StagePendingEntrustmentDecisionCommandValidator : AbstractValidator<StagePendingEntrustmentDecisionCommand>
{
    public StagePendingEntrustmentDecisionCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.EpaId).GreaterThan(0);
        RuleFor(command => command.AuthorisedLevelId).GreaterThan(0);
        RuleFor(command => command.Rationale).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command)
            .Must(command => !command.ExpiresOn.HasValue || command.ExpiresOn.Value > command.IssuedOn)
            .WithMessage("An expiry date must be after the issue date.");
        RuleForEach(command => command.EvidenceLinks).ChildRules(link =>
        {
            link.RuleFor(l => l.SourceLabel).NotEmpty().MaximumLength(200);
            link.RuleFor(l => l.Summary).MaximumLength(2000);
        });
    }
}

public sealed class StagePendingEntrustmentDecisionCommandHandler
    : IRequestHandler<StagePendingEntrustmentDecisionCommand, PendingEntrustmentDecisionDto>
{
    private readonly IApplicationDbContext _dbContext;

    public StagePendingEntrustmentDecisionCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PendingEntrustmentDecisionDto> Handle(StagePendingEntrustmentDecisionCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(r => r.Panel)
                .ThenInclude(p => p.Members)
            .SingleOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        if (review.IsFormative)
        {
            throw new InvalidOperationException("Formative reviews cannot issue entrustment decisions.");
        }

        if (review.State is not CommitteeReviewState.InProgress and not CommitteeReviewState.Decided)
        {
            throw new InvalidOperationException("Pending entrustment decisions may only be staged on an in-progress or decided review.");
        }

        EntrustmentDecisionAuthorization.DemandChairAccess(request.Principal, review.Panel);
        var actorUserId = EntrustmentDecisionAuthorization.GetRequiredUserId(request.Principal);

        _ = await _dbContext.Set<Epa>().SingleOrDefaultAsync(e => e.Id == request.EpaId, cancellationToken)
            ?? throw new InvalidOperationException("The specified EPA could not be found.");
        var level = await _dbContext.Set<EntrustmentLevel>().SingleOrDefaultAsync(l => l.Id == request.AuthorisedLevelId, cancellationToken)
            ?? throw new InvalidOperationException("The specified entrustment level could not be found.");

        // T076 / F-4D-1: when the trainee's programme (sub-speciality) declares a default entrustment
        // scale, the STAR must be granted on that scale — reject a level from any other scale.
        var programScaleId = await _dbContext.Set<TraineeProfile>()
            .Where(profile => profile.UserId == review.TraineeUserId)
            .Select(profile => profile.Curriculum.SubSpeciality.DefaultEntrustmentScaleId)
            .FirstOrDefaultAsync(cancellationToken);
        if (programScaleId.HasValue && level.ScaleId != programScaleId.Value)
        {
            throw new InvalidOperationException("The authorised level must belong to the programme's entrustment scale.");
        }

        var evidenceJson = EntrustmentDecisionMappings.SerializeEvidenceLinks(request.EvidenceLinks ?? Array.Empty<EntrustmentEvidenceLinkInput>());

        PendingEntrustmentDecision pending;
        if (request.PendingId.HasValue)
        {
            pending = await _dbContext.Set<PendingEntrustmentDecision>()
                .SingleOrDefaultAsync(p => p.Id == request.PendingId.Value && p.ReviewId == request.ReviewId, cancellationToken)
                ?? throw new InvalidOperationException("The pending entrustment decision could not be found for this review.");

            pending.Update(request.AuthorisedLevelId, request.IssuedOn, request.ExpiresOn, request.Rationale, evidenceJson);
        }
        else
        {
            pending = PendingEntrustmentDecision.Stage(
                request.ReviewId,
                request.EpaId,
                request.AuthorisedLevelId,
                request.IssuedOn,
                request.ExpiresOn,
                request.Rationale,
                evidenceJson,
                actorUserId,
                DateTime.UtcNow);
            _dbContext.Set<PendingEntrustmentDecision>().Add(pending);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var stored = await _dbContext.Set<PendingEntrustmentDecision>()
            .AsNoTracking()
            .Include(p => p.Epa)
            .Include(p => p.AuthorisedLevel)
            .SingleAsync(p => p.Id == pending.Id, cancellationToken);

        return stored.ToDto();
    }
}
