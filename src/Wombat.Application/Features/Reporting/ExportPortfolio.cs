using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Domain.Reporting;

namespace Wombat.Application.Features.Reporting;

public sealed record ExportPortfolioCommand(
    string TraineeUserId,
    DateOnly? FromDate,
    DateOnly? ToDate,
    ClaimsPrincipal Principal) : IRequest<PortfolioExportResult>;

public sealed class ExportPortfolioCommandValidator : AbstractValidator<ExportPortfolioCommand>
{
    public ExportPortfolioCommandValidator()
    {
        RuleFor(command => command.TraineeUserId).NotEmpty();
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command)
            .Must(command => command.FromDate is null || command.ToDate is null || command.FromDate <= command.ToDate)
            .WithMessage("The start date must be before or equal to the end date.");
    }
}

public sealed class ExportPortfolioCommandHandler : IRequestHandler<ExportPortfolioCommand, PortfolioExportResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPortfolioPdfService _pdfService;

    public ExportPortfolioCommandHandler(IApplicationDbContext dbContext, IPortfolioPdfService pdfService)
    {
        _dbContext = dbContext;
        _pdfService = pdfService;
    }

    public async Task<PortfolioExportResult> Handle(ExportPortfolioCommand request, CancellationToken cancellationToken)
    {
        DemandExportAccess(request.Principal, request.TraineeUserId);

        var result = await _pdfService.GenerateAsync(
            new PortfolioExportRequest(request.TraineeUserId, request.FromDate, request.ToDate),
            cancellationToken);

        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        _dbContext.Set<PortfolioExport>().Add(new PortfolioExport
        {
            TraineeUserId = request.TraineeUserId,
            ExportedByUserId = userId,
            ExportedOn = DateTime.UtcNow,
            FilterFromDate = request.FromDate,
            FilterToDate = request.ToDate,
            ContentHash = result.ContentHash,
            FileName = result.FileName
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    private static void DemandExportAccess(ClaimsPrincipal principal, string traineeUserId)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
            principal.IsInRole(WombatRoles.SpecialityAdmin) ||
            principal.IsInRole(WombatRoles.SubSpecialityAdmin))
        {
            return;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.Equals(userId, traineeUserId, StringComparison.Ordinal))
        {
            return;
        }

        throw new UnauthorizedAccessException("You are not authorized to export this portfolio.");
    }
}
