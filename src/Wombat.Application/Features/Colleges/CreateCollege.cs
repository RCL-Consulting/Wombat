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
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.ShortCode).NotEmpty().MaximumLength(32);
        RuleFor(command => command.Description).MaximumLength(1000);
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

        var college = new College
        {
            Name = request.Name.Trim(),
            ShortCode = request.ShortCode.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedOn = DateTime.UtcNow
        };

        _dbContext.Set<College>().Add(college);

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
