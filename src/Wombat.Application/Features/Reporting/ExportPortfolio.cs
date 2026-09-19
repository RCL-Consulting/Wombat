using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
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
        if (principal.IsAdministrator())
        {
            return;
        }

        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(callerUserId) &&
            string.Equals(callerUserId, traineeUserId, StringComparison.Ordinal))
        {
            return;
        }

        // A trainee with no profile has no organisational home, so no scoped role can be held over
        // them: only a global Administrator or the trainee themselves get through, and both have
        // already been answered above. An unknown user id lands here too, by the same route.
        var scope = await ResolveTraineeScopeAsync(traineeUserId, cancellationToken);
        if (scope is not null && IsScopedOverseerOf(scope, principal))
        {
            return;
        }

        throw new UnauthorizedAccessException(RefusalMessage);
    }

    /// <summary>
    /// Programme oversight: the roles that supervise a trainee may export that trainee's portfolio,
    /// each at the level of the tree they are scoped to. Mirrors
    /// <c>ActivityService.IsScopedOverseerOf</c>, which gates the individual activities this PDF is
    /// assembled from. (T101)
    /// </summary>
    private static bool IsScopedOverseerOf(TraineeScope scope, ClaimsPrincipal principal)
    {
        // EVERY arm requires the institution, the speciality ones included: a Speciality is
        // College-owned and therefore a NATIONAL id, so IsInSpeciality on its own would let one
        // hospital's SpecialityAdmin export the complete portfolio of every paediatric trainee in the
        // country. Mirrors ActivityService.IsScopedOverseerOf.
        if (principal.GetInstitutionId() != scope.InstitutionId)
        {
            return false;
        }

        if (principal.IsInstitutionalAdmin() ||
            principal.IsInRole(WombatRoles.Coordinator) ||
            principal.IsInRole(WombatRoles.CommitteeMember))
        {
            return true;
        }

        if (scope.SpecialityId is int specialityId &&
            principal.IsInRole(WombatRoles.SpecialityAdmin) &&
            principal.IsInSpeciality(specialityId))
        {
            return true;
        }

        return scope.SubSpecialityId is int subSpecialityId &&
               principal.IsInRole(WombatRoles.SubSpecialityAdmin) &&
               principal.IsInSubSpeciality(subSpecialityId);
    }

    /// <summary>
    /// The trainee's organisational home, read from their <see cref="TraineeProfile"/>: the active
    /// profile, then the most recent by id.
    /// </summary>
    /// <remarks>
    /// That tie-break is taken deliberately from <c>ActivityService.ResolveSubjectScopeAsync</c>,
    /// which stamps the same three ids onto every activity at creation. The two must not drift: a
    /// caller who may read each of a trainee's assessments one by one must be the same caller who may
    /// export them as a bundle, in both directions. Change them together.
    /// </remarks>
    private async Task<TraineeScope?> ResolveTraineeScopeAsync(
        string traineeUserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(traineeUserId))
        {
            return null;
        }

        // Resolved one level at a time rather than as a single join through
        // TraineeProfile -> Curriculum -> SubSpeciality. Those navigations are required, so one query
        // would be an INNER join and a missing curriculum row would take the institution down with
        // it — refusing the trainee's own institutional admin because of an unrelated gap. Each level
        // degrades on its own; a null speciality simply matches no speciality admin.
        var profile = await _dbContext.Set<TraineeProfile>()
            .Where(entity => entity.UserId == traineeUserId)
            .OrderByDescending(entity => entity.IsActive)
            .ThenByDescending(entity => entity.Id)
            .Select(entity => new { entity.InstitutionId, entity.CurriculumId })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var subSpecialityId = await _dbContext.Set<Curriculum>()
            .Where(entity => entity.Id == profile.CurriculumId)
            .Select(entity => (int?)entity.SubSpecialityId)
            .FirstOrDefaultAsync(cancellationToken);

        var specialityId = subSpecialityId is null
            ? null
            : await _dbContext.Set<SubSpeciality>()
                .Where(entity => entity.Id == subSpecialityId.Value)
                .Select(entity => (int?)entity.SpecialityId)
                .FirstOrDefaultAsync(cancellationToken);

        return new TraineeScope(profile.InstitutionId, specialityId, subSpecialityId);
    }

    private sealed record TraineeScope(int InstitutionId, int? SpecialityId, int? SubSpecialityId);
}
