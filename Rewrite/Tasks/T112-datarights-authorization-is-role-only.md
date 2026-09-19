# T112 — Data-rights handlers are gated by role alone, and one skips authorization entirely

**Status:** open
**Surfaced:** 2026-09-19, mapping the read boundary for T101.
**Severity:** High (confidentiality) — a subject access report is, by construction, everything the
product holds about one person. It is the single richest object in the system, and today any
`Coordinator` in any institution can download anyone's.

## Symptom

Three distinct defects in the same feature folder, `src/Wombat.Application/Features/DataRights/`:

1. **Role-only gates, no scope.** `DownloadAccessReport.DemandAccess` (~`:52-64`) allows `Administrator`
   or `Coordinator`, or the requester themselves — with **no institution comparison**. The same shape
   repeats in `GetDataRightsRequestById`, `ListDataRightsRequests`, `ApproveDataRightsRequest` and
   `RejectDataRightsRequest`; `ApplyRectification` and `CompleteRectificationRequest` use
   `Administrator || SpecialityAdmin`, also unscoped.
2. **`ListDataRightsRequests` skips the check when the principal is null.** The handler reads
   `if (request.Principal is not null) DemandReviewAccess(...)` — so a caller who passes no principal is
   authorized by omission rather than refused. Every other handler in the repo treats a null principal
   as a failure.
3. **The list leaks the population even if the download is fixed.** `ListDataRightsRequests` returns
   every request in every institution unfiltered, so closing only `DownloadAccessReport` would leave the
   names and request types visible.

`AccessReportBuilder` includes `DataJson` for every activity where the subject is subject **or** creator,
so the report carries clinical narrative, not just metadata.

## Why it is not a contained fix

`DataRightsRequest` (`src/Wombat.Domain/DataRights/DataRightsRequest.cs`) carries **no institution
stamp** — only `RequesterUserId`. So scoping needs one of:

- **Stamp at submission**, the treatment T101 gave `Activity` (`Activity.InstitutionId` et al., stamped in
  `ActivityService.CreateDraftAsync`, backfilled by migration). Clean, and consistent with T101 — but it
  is a Domain change plus a migration.
- **Resolve through the profile**, as `ExportPortfolio.ResolveTraineeScopeAsync` now does. This has no
  answer for a requester who is an **assessor or administrative staff** rather than a trainee: they have
  no `TraineeProfile`, and data-rights requests are explicitly not limited to trainees.

The second reason it is not contained: deciding who the data-rights officer *is*. POPIA-style subject
access is usually a named institutional role, not "any Coordinator". That is a product decision, not a
code one, and it should be made before the plumbing.

## Recommended shape

1. Fix defect 2 immediately and separately — a null principal must refuse, not pass. One line, no design
   required.
2. Decide the officer role and its scope.
3. Stamp `DataRightsRequest.InstitutionId` at submission, mirroring T101's `Activity` treatment, with a
   migration that backfills from the requester's profile where one exists and leaves null otherwise.
4. Route every handler in the folder through one shared predicate, the way T101 routed the activity
   surface through `ActivityService.IsReadableBy` / `ActivityReadScope.WhereReadableBy`.

## Confirmed reachable

The T101 review panel traced the whole chain and confirmed it end to end, so this is not a paper
finding:

- `RequestsList.razor:2` is `[Authorize(Roles = "Administrator,Coordinator")]` and `:61` emits
  `/admin/data-rights/@req.Id` — the unscoped list hands the caller every institution's request GUIDs.
- `Program.cs:417-437` is a plain `MapGet(...).RequireAuthorization()`, i.e. `curl`-able, not a Blazor
  circuit.
- `AccessReportBuilder.cs:62-75` selects `DataJson = a.DataJson` verbatim for every activity where the
  user is subject **or** creator, and `:151-165` bundles the full portfolio PDF.
- The same bare role check lets a Coordinator approve another institution's **Erasure** request
  (`ApproveDataRightsRequest.cs:57` → `ErasureExecutor.cs:43-59`), which is destructive, not merely a
  disclosure.

It is the exact pattern the comment at `ExportPortfolio.cs:83-88` now says must not be reintroduced.

## Related

Same defect class as [T101], found while closing it. [T101] fixed the activity surface and the portfolio
export; this is the third door and was deliberately deferred because it needs a product decision.
