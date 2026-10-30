using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Features.Users;
using Wombat.Domain.Curricula;
using Wombat.Domain.DataRights;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.DataRights.Queries;

/// <summary>
/// A request's page (T387, Q12, S14). Null for an unknown id and for one the caller may not read alike, so the page draws
/// one Request unavailable for both (Q13, Q17; T112: no oracle telling "no such request" from "another institution's").
/// Until T387 both threw the refusal message, which the page printed over its empty state.
/// </summary>
public sealed record GetDataRightsRequestByIdQuery(
    Guid Id,
    ClaimsPrincipal Principal) : IRequest<DataRightsRequestDetailDto?>;

public sealed class GetDataRightsRequestByIdQueryValidator : AbstractValidator<GetDataRightsRequestByIdQuery>
{
    public GetDataRightsRequestByIdQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetDataRightsRequestByIdQueryHandler : IRequestHandler<GetDataRightsRequestByIdQuery, DataRightsRequestDetailDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetDataRightsRequestByIdQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<DataRightsRequestDetailDto?> Handle(GetDataRightsRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<DataRightsRequest>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (entity is null || !DataRightsAuthorization.CanRead(request.Principal, entity))
        {
            return null;
        }

        var corrections = await _dbContext.Set<DataRightsRectification>()
            .AsNoTracking()
            .Where(c => c.RequestId == entity.Id)
            .OrderBy(c => c.AppliedOn)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);

        // Everyone the page names, read once (T387, build-lanes "names read once per page by its query"): the requester,
        // the decider and each recorder. An erased person is known by their erasure record, matched by the id they had or
        // by the pseudonym the erasure wrote over it (F3: DecidedByUserId and AppliedByUserId then carry the pseudonym).
        var staffIds = corrections.Select(c => c.AppliedByUserId)
            .Append(entity.DecidedByUserId)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var everyone = staffIds.Append(entity.RequesterUserId).Distinct(StringComparer.Ordinal).ToArray();
        var erasures = await DataRightsPeople.ErasuresOfAsync(_dbContext, everyone, cancellationToken);

        var requesterErasure = erasures.Of(entity.RequesterUserId);
        var contact = requesterErasure is null
            ? (await _users.GetContactsAsync([entity.RequesterUserId], cancellationToken)).GetValueOrDefault(entity.RequesterUserId)
            : null;
        var staffNames = await DataRightsPeople.StaffNamesAsync(_users, erasures, staffIds, cancellationToken);

        var institutionName = entity.InstitutionId is { } institutionId
            ? await _dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(institution => institution.Id == institutionId)
                .Select(institution => institution.Name)
                .SingleOrDefaultAsync(cancellationToken)
            : null;

        // The Correction section's record lines (S2, F6, S7) are a Rectification's alone, and an erased person has neither
        // a programme to correct nor a sign-in: their profile is the pseudonym's (T258), their logins removed (T149).
        var readsRecords = entity.Type == DataRightsRequestType.Rectification && requesterErasure is null;

        return new DataRightsRequestDetailDto(
            entity.Id,
            entity.RequesterUserId,
            // The live name (T387, F1, M1): a name corrected on My account shows at once; the pseudonym once erased.
            DataRightsPeople.RequesterName(entity, requesterErasure, contact),
            entity.RequestedOn,
            entity.Type,
            entity.Status,
            entity.Reason,
            entity.DecisionNote,
            entity.DecidedByUserId,
            entity.DecidedOn,
            entity.CompletedOn)
        {
            // The page's flags, from the rules as T387 changed them, never from roles (Q1, Q21, F9, N2).
            MayDecide = entity.Status == DataRightsRequestStatus.Submitted &&
                        DataRightsAuthorization.CanReview(request.Principal, entity),
            MayRecord = entity.Type == DataRightsRequestType.Rectification &&
                        entity.Status == DataRightsRequestStatus.Approved &&
                        DataRightsAuthorization.CanRectify(request.Principal, entity),
            MayDownload = DataRightsAuthorization.CanDownload(request.Principal, entity),
            OwnRequest = DataRightsAuthorization.OwnRequest(request.Principal, entity),

            // T387 (S1, N5, S8): the live address, none once erased; the erasure's South African day; the institution.
            RequesterEmail = DataRightsPeople.EmailOf(contact),
            ErasedOn = requesterErasure is null ? null : ProgrammeCalendar.DateOf(requesterErasure.ErasedOn),
            InstitutionName = institutionName,
            DecidedByName = entity.DecidedByUserId is { } decider ? staffNames[decider] : null,
            Corrections = corrections
                .Select(c => new DataRightsCorrectionDto(
                    c.Id,
                    c.TargetType,
                    // Read back through the parser, never as raw text: Postgres re-renders jsonb (CLAUDE.md; T387, S11).
                    ReadValue(c.FromValueJson),
                    ReadValue(c.ToValueJson),
                    c.AppliedByUserId,
                    staffNames[c.AppliedByUserId],
                    c.AppliedOn))
                .ToArray(),
            ProgrammeStart = readsRecords
                ? await ProgrammeStartAsync(entity.RequesterUserId, request.Principal, cancellationToken)
                : null,
            NameFromSignIn = readsRecords ? await NameFromSignInAsync(entity.RequesterUserId, cancellationToken) : null,
        };
    }

    /// <summary>
    /// The requester's programme start as a record (T387, S2): the preferred profile (one per trainee, the tie-break every
    /// reader of a trainee's programme uses), whether it has ended (flow 13's archived record), and whether the caller may
    /// open it. Opening it is the Trainees pages' own rule (<see cref="TraineeAdministrationRules" />) at the profile's
    /// institution: the Administrator, or an Institutional admin there; a Coordinator never (R3-C02's "you cannot").
    /// </summary>
    private async Task<ProgrammeStartRecordDto?> ProgrammeStartAsync(
        string requesterUserId,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var profile = await TraineeScopeResolver.PreferredProfiles(_dbContext)
            .AsNoTracking()
            .Where(candidate => candidate.UserId == requesterUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return null;
        }

        var mayOpen = TraineeAdministrationRules.MayAdministerTrainees(principal) &&
                      principal.CanAccessInstitution(profile.InstitutionId);
        return new ProgrammeStartRecordDto(profile.Id, EndedProgrammes.Of(profile) is not null, mayOpen);
    }

    /// <summary>
    /// The institution whose sign-in sets the requester's name (T387, F6, S7): ExternalLoginHandler writes both names at
    /// every institutional sign-in, so a correction made on My account would be undone at the next. The first sign-in, in
    /// How you sign in's order; null for none.
    /// </summary>
    private async Task<string?> NameFromSignInAsync(string requesterUserId, CancellationToken cancellationToken)
    {
        var methods = await _users.GetSignInMethodsAsync(requesterUserId, cancellationToken);
        return methods?.InstitutionalSignIns.FirstOrDefault()?.DisplayName;
    }

    /// <summary>
    /// A stored value as text. Total (T387 review N4): a value stored before T387 may be a JSON object (the old default
    /// "{}"), which is shown as stored rather than failing the whole page's read.
    /// </summary>
    private static string ReadValue(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string>(json) ?? string.Empty;
        }
        catch (JsonException)
        {
            return json;
        }
    }
}
