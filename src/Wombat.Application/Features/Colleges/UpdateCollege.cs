using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Colleges;

public sealed record UpdateCollegeCommand(
    int Id,
    string Name,
    string ShortCode,
    string? Description,
    bool IsActive,
    ClaimsPrincipal Principal) : IRequest<CollegeDto>;

public sealed class UpdateCollegeCommandValidator : AbstractValidator<UpdateCollegeCommand>
{
    public UpdateCollegeCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        // T394 (flow 16, T324, N1): each failure in its field's words, never a property's name.
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(CollegeHolders.NameRequired)
            .MaximumLength(200).WithMessage(CollegeHolders.NameTooLong);
        RuleFor(command => command.ShortCode)
            .NotEmpty().WithMessage(CollegeHolders.ShortCodeRequired)
            .MaximumLength(32).WithMessage(CollegeHolders.ShortCodeTooLong);
        RuleFor(command => command.Description).MaximumLength(1000).WithMessage(CollegeHolders.DescriptionTooLong);
    }
}

public sealed class UpdateCollegeCommandHandler : IRequestHandler<UpdateCollegeCommand, CollegeDto>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateCollegeCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CollegeDto> Handle(UpdateCollegeCommand request, CancellationToken cancellationToken)
    {
        // Editing the College record itself is an Administrator action (a CollegeAdmin manages the catalogue,
        // not the College's own metadata/active flag).
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only global administrators may update colleges.");
        }

        var college = await _dbContext.Set<College>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"College {request.Id} was not found.");

        // T394 (S10, T201): checked before anything is assigned, since the audit pipeline commits whatever a refused
        // handler changed.
        var name = request.Name.Trim();
        var shortCode = request.ShortCode.Trim();
        if (await CollegeHolders.RefusalAsync(_dbContext, college.Id, name, shortCode, null, cancellationToken) is { } taken)
        {
            throw taken;
        }

        college.Name = name;
        college.ShortCode = shortCode;
        college.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        college.IsActive = request.IsActive;

        await CollegeHolders.SaveAsync(_dbContext, request.Id, name, shortCode, cancellationToken);

        return new CollegeDto(college.Id, college.Name, college.ShortCode, college.Description, college.IsActive, college.CreatedOn);
    }
}
