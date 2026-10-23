using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Assessors;

/// <summary>
/// Records one assessor's training (T376, flow 12, M1, F4): the status, its completion date and the qualifications, and
/// nothing else. The institution is the person's own, written onto the profile at every save; the command asks for no
/// institution, speciality or sub-speciality and writes no scope, since an assessor's discipline is the person's scope
/// (Q12), kept on the user page (Add a role). Until T376 the save took the form's scope ids and passed them to
/// <c>UpdateScopeAsync</c>, which replaces the person's whole scope and changes their security stamp: saving training for a
/// Committee member who was also an Assessor would have stripped her speciality and signed her out.
/// </summary>
public sealed record CreateOrUpdateAssessorProfileCommand(
    string UserId,
    AssessorTrainingStatus TrainingStatus,
    DateOnly? TrainingCompletedOn,
    string Qualifications,
    ClaimsPrincipal Principal) : IRequest<AssessorProfileDto>;

public sealed class CreateOrUpdateAssessorProfileCommandValidator : AbstractValidator<CreateOrUpdateAssessorProfileCommand>
{
    public CreateOrUpdateAssessorProfileCommandValidator()
    {
        // The words are the training section's (T376, lane T, T05), so a refusal never names a property (T324).
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Qualifications)
            .NotEmpty().WithMessage(AssessorTrainingWords.TypeQualifications)
            .MaximumLength(4000).WithMessage("Keep the qualifications to 4,000 characters.");
        RuleFor(command => command.TrainingStatus).IsInEnum().WithMessage(AssessorTrainingWords.ChooseStatus);
        // Note: a completion date is only meaningful for Provisional/Trained; rather than
        // rejecting a stale value (the edit form hides the field for other statuses), the
        // handler normalizes it away. See CreateOrUpdateAssessorProfileCommandHandler.
    }
}

public sealed class CreateOrUpdateAssessorProfileCommandHandler : IRequestHandler<CreateOrUpdateAssessorProfileCommand, AssessorProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public CreateOrUpdateAssessorProfileCommandHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    /// <summary>The refusal to record your own training (T376, M1: the Users surface changes nobody's own account, T278).</summary>
    public const string OwnTrainingRefused = "You cannot record your own assessor training.";

    /// <summary>The refusal for an id that names nobody the caller may change (T289; T290's one refusal for both).</summary>
    public const string AssessorNotInScope = "You do not have permission to manage this assessor.";

    /// <summary>The refusal for an account that belongs to no institution, so has nowhere to record a profile (T376, M1).</summary>
    public const string AssessorHasNoInstitution =
        "This person belongs to no institution, so no assessor training can be recorded.";

    /// <summary>The refusal for a person who does not hold Assessor.</summary>
    public const string NotAnAssessor = "Only users in the Assessor role can have an assessor profile.";

    /// <remarks>
    /// <para>
    /// <b>A trainee first</b> (<see cref="UserAdministrationRules.DemandUserAdministration" />, T278), asked before
    /// anything is looked up; then <b>not the caller</b> (<see cref="UserAdministrationRules.DemandNotCaller" />); then
    /// <b>in scope</b> (<see cref="UserAdministrationRules.DemandInScope" />, T289): an id outside the caller's institution
    /// is refused as one that names nobody.
    /// </para>
    /// <para>
    /// <b>No scope is written</b> (M1): no <c>UpdateScopeAsync</c>, so neither the person's scope nor their security stamp
    /// changes. The profile's institution is stamped from the person's own (F4), so a person moved by Users carries their
    /// profile with them at the next save; T289's refusal of a form value naming another institution has nothing left to
    /// refuse, since no form names one.
    /// </para>
    /// </remarks>
    public async Task<AssessorProfileDto> Handle(CreateOrUpdateAssessorProfileCommand request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);
        UserAdministrationRules.DemandNotCaller(request.Principal, request.UserId, OwnTrainingRefused);

        var user = UserAdministrationRules.DemandInScope(
            request.Principal,
            await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken),
            AssessorNotInScope);

        // Typed (T376, lane T): the section words each from its reason, never from this message.
        if (user.InstitutionId is not int institutionId)
        {
            throw new AssessorTrainingRefusedException(AssessorTrainingRefusal.NoInstitution, AssessorHasNoInstitution);
        }

        if (!user.Roles.Contains(WombatRoles.Assessor, StringComparer.Ordinal))
        {
            throw new AssessorTrainingRefusedException(AssessorTrainingRefusal.NotAnAssessor, NotAnAssessor);
        }

        var institution = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .SingleOrDefaultAsync(entity => entity.Id == institutionId, cancellationToken)
            ?? throw new InvalidOperationException("The assessor's institution could not be found.");

        var profile = await _dbContext.Set<AssessorProfile>()
            .SingleOrDefaultAsync(entity => entity.UserId == request.UserId, cancellationToken);

        if (profile is null)
        {
            profile = new AssessorProfile
            {
                UserId = request.UserId
            };

            _dbContext.Set<AssessorProfile>().Add(profile);
        }

        profile.Qualifications = request.Qualifications.Trim();
        profile.InstitutionId = institutionId;
        profile.TrainingStatus = request.TrainingStatus;
        profile.TrainingCompletedOn =
            request.TrainingStatus is AssessorTrainingStatus.Provisional or AssessorTrainingStatus.Trained
                ? request.TrainingCompletedOn
                : null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AssessorProfileDto(
            profile.Id,
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            profile.Qualifications,
            institution.Id,
            institution.Name,
            profile.TrainingStatus,
            profile.TrainingCompletedOn);
    }
}
