using Wombat.Domain.DataRights;

namespace Wombat.Application.Features.DataRights;

public sealed record DataRightsRequestDto(
    Guid Id,
    string RequesterUserId,
    string RequesterDisplayName,
    DateTime RequestedOn,
    DataRightsRequestType Type,
    DataRightsRequestStatus Status,
    string Reason,
    string? DecisionNote,
    string? DecidedByUserId,
    DateTime? DecidedOn,
    DateTime? CompletedOn);

public sealed record DataRightsRequestSummaryDto(
    Guid Id,
    string RequesterDisplayName,
    DateTime RequestedOn,
    DataRightsRequestType Type,
    DataRightsRequestStatus Status);

public sealed record DataRightsRectificationDto(
    Guid Id,
    Guid RequestId,
    string TargetType,
    Guid TargetId,
    string FromValueJson,
    string ToValueJson,
    DateTime? AppliedOn);

public sealed record DataRightsErasureRecordDto(
    Guid Id,
    Guid RequestId,
    string UserId,
    string Pseudonym,
    DateTime ErasedOn,
    string RetentionReasonsJson);

public sealed record PagedDataRightsResult(
    IReadOnlyList<DataRightsRequestSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record AccessExportResult(
    byte[] ZipBytes,
    string FileName);
