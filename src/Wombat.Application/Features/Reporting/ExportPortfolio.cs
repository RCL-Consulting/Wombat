using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
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
    /// <summary>
    /// One message for every refusal — out of scope, wrong role, no such user. The route parameter is
    /// a user id the caller already holds, so refusing tells them nothing they did not type; what a
    /// varied message would add is confirmation of which trainees exist and where they train.
    /// </summary>
    private const string RefusalMessage = "You are not authorized to export this portfolio.";

    private readonly IApplicationDbContext _dbContext;
    private readonly IPortfolioPdfService _pdfService;

    public ExportPortfolioCommandHandler(IApplicationDbContext dbContext, IPortfolioPdfService pdfService)
    {
        _dbContext = dbContext;
        _pdfService = pdfService;
    }

    public async Task<PortfolioExportResult> Handle(ExportPortfolioCommand request, CancellationToken cancellationToken)
    {
        await DemandExportAccessAsync(request.Principal, request.TraineeUserId, cancellationToken);

        var result = await _pdfService.GenerateAsync(
            new PortfolioExportRequest(request.TraineeUserId, request.FromDate, request.ToDate, request.Principal),
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

    /// <summary>
    /// The export is the trainee's entire clinical record in one file — every activity's DataJson,
    /// rendered verbatim, transitions and all. Oversight of it is therefore scoped exactly as the
    /// single-activity read gate scopes oversight of one assessment: holding an oversight role is not
    /// enough, the role has to be held over THIS trainee. (T101)
    /// </summary>
    /// <remarks>
    /// Before T101 this method returned early for InstitutionalAdmin, SpecialityAdmin,
    /// SubSpecialityAdmin and Coordinator on the role alone, with no institution, speciality or
    /// sub-speciality comparison anywhere, so a Coordinator in institution A could export the
    /// complete portfolio of any trainee in institution B by editing the route id. That leaked
    /// strictly more than the single-activity hole this task is named for; do not reintroduce a bare
    /// role check.
    ///
    /// Refusal stays an <see cref="UnauthorizedAccessException"/> rather than the null-to-404 the
    /// get-by-id reads use. 404 exists there to stop an integer id in the address bar being
    /// incremented into an enumeration of other institutions' activities; here the route parameter is
    /// an opaque user id the caller must already possess, and because every refusal — including one
    /// for a user id that matches nobody — carries the single <see cref="RefusalMessage"/>, no
    /// refusal confirms that a trainee exists. That leaves the house convention for a command, which
    /// is to throw, and it is what the Razor page already renders as a flat "not authorized" alert.
    /// </remarks>
    private async Task DemandExportAccessAsync(
        ClaimsPrincipal principal,
        string traineeUserId,
        CancellationToken cancellationToken)
    {
        // The trainee themselves, a global Administrator, or someone who oversees the programme the trainee is on.
        // A trainee with no profile has no organisational home, so no scoped role can be held over them, and an
        // unknown user id lands in the same place. The ladder, and the tie-break that picks the trainee's profile, are
        // TraineeScopeResolver's (T113): the same answer as the scope ActivityService stamps on each activity this PDF
        // is assembled from, so a caller who may export the bundle may read each of its assessments one by one.
        // Not the other way round for everyone: this ladder puts the trainee rung first (T185), and the per-activity
        // gate does not yet, so a Trainee who also holds an oversight role opens a peer's activities singly but is
        // refused their export. TraineeScopeResolver.ActsAsTrainee records that gap.
        if (!await TraineeScopeResolver.MayReadAsync(_dbContext, principal, traineeUserId, cancellationToken))
        {
            throw new UnauthorizedAccessException(RefusalMessage);
        }
    }
}
