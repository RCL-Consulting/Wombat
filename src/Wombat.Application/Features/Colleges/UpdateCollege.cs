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
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.ShortCode).NotEmpty().MaximumLength(32);
        RuleFor(command => command.Description).MaximumLength(1000);
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

        college.Name = request.Name.Trim();
        college.ShortCode = request.ShortCode.Trim();
        college.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        college.IsActive = request.IsActive;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw new InvalidOperationException("A college with the same name or short code already exists.", exception);
        }

        return new CollegeDto(college.Id, college.Name, college.ShortCode, college.Description, college.IsActive, college.CreatedOn);
    }
}
