using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Colleges;

public sealed record CreateCollegeCommand(
    string Name,
    string ShortCode,
    string? Description,
    ClaimsPrincipal Principal) : IRequest<CollegeDto>;

public sealed class CreateCollegeCommandValidator : AbstractValidator<CreateCollegeCommand>
{
    public CreateCollegeCommandValidator()
    {
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

public sealed class CreateCollegeCommandHandler : IRequestHandler<CreateCollegeCommand, CollegeDto>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateCollegeCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CollegeDto> Handle(CreateCollegeCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only global administrators may create colleges.");
        }

        // T394 (S10, T201): a taken name or short code is refused, naming its holder, before the College is staged, since
        // the audit pipeline commits whatever a refused handler staged.
        var name = request.Name.Trim();
        var shortCode = request.ShortCode.Trim();
        if (await CollegeHolders.RefusalAsync(_dbContext, null, name, shortCode, null, cancellationToken) is { } taken)
        {
            throw taken;
        }

        var college = new College
        {
            Name = name,
            ShortCode = shortCode,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedOn = DateTime.UtcNow
        };

        _dbContext.Set<College>().Add(college);

        await CollegeHolders.SaveAsync(_dbContext, null, name, shortCode, cancellationToken);

        return new CollegeDto(college.Id, college.Name, college.ShortCode, college.Description, college.IsActive, college.CreatedOn);
    }
}
