using System.Security.Claims;
using MediatR;

namespace Wombat.Application.Features.Institutions.Commands.CreateSubSpeciality;

/// <param name="DefaultEntrustmentScaleId">
/// The ladder its trainees' committee STARs are limited to, chosen at create (T394, flow 16, Q1); null for none, every
/// scale offered.
/// </param>
public sealed record CreateSubSpecialityCommand(
    int SpecialityId,
    string Name,
    string? Description,
    int? DefaultEntrustmentScaleId,
    ClaimsPrincipal Principal) : IRequest<SubSpecialityDto>;
