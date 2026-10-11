---
id: T112
title: "Data-rights handlers are gated by role alone, and one skips authorization entirely"
status: done
priority: P1
created: 2026-09-19
---
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

---

## Shipped — 2026-09-19

Scoped the existing holders; **did not** choose the holders. Which roles ought to hold a statutory
data-rights power is a product question, and answering it in a security commit would have silently
changed who can do what. The role sets are preserved exactly: `Coordinator` for review/decision,
`SpecialityAdmin` for rectification, `Administrator` globally. **That question is still open** — see
"Still open" below.

### What changed

- **`DataRightsRequest.InstitutionId`**, nullable, stamped at submission from the submitting
  principal's own claim (the submitter *is* the data subject, so no lookup and nothing to go stale).
  Migration `20260919111354_T112_DataRightsInstitutionScope` backfills from `AspNetUsers."InstitutionId"`,
  which is the same column the `institution_id` claim is issued from.
- **`DataRightsAuthorization`** replaces six hand-written, role-only gates with one predicate that
  conjoins the request's institution. A null stamp matches no scoped reviewer — erasure is
  irreversible, so an unplaceable request withholds the power rather than spreading it.
- **`ListDataRightsRequestsQuery`**'s `Principal` was optional *and last*, and the handler read
  `if (request.Principal is not null)` — a caller passing nothing was authorized by omission. It is now
  required, ahead of the paging parameters, and the query filters by institution (count included, or it
  would leak the other institution's volume).
- **The export is narrower than the metadata read.** Scoping the request row does not scope the bundle
  it releases: the access report is assembled by *person*, so it can carry rows stamped to other
  institutions — an assessor who covered a rotation elsewhere, a trainee who transferred. Those rows are
  the subject's personal data and belong in *their* report, but a reviewer in scope for the request
  cannot open them individually. `DemandExportAccess` therefore admits only the data subject and a
  global Administrator. Nothing is lost: the only download link in the product is on the subject's own
  profile page (`Profile/DataRights.razor:107`). The reviewer approves; the subject collects.

### The review caught that none of this was tested

An adversarial review deleted the institution conjunction from `CanAct` and **all 449 Application tests
still passed**. Every pre-existing reviewer-path test used an `Administrator` principal, which
short-circuits before the comparison is ever reached, and the first batch of new tests exercised the
list query's LINQ filter rather than the gate.

`DataRightsAuthorizationTests` and three handler-level tests now cover it. Re-running the same mutation
fails **8** tests, including one asserting `IErasureExecutor` was never called — a refusal that still
erased would be worse than no gate.

### Still open

- **Who the data-rights officer should be.** "Any Coordinator" is unlikely to be the right answer for a
  statutory function, and `InstitutionalAdmin` — admitted by the activity read gate — is not admitted
  here. Needs a product decision, then a small change to the two role arrays.
- **`SpecialityAdmin` rectification is institution-scoped but not speciality-scoped**, unlike both T101
  gates. Latent: `ApplyRectificationCommand` and `CompleteRectificationRequestCommand` have no caller
  outside the feature folder today. Close it when a UI is built for them.
- **`ErasureExecutor` is scoped to the person, not the row.** Approving an erasure pseudonymises every
  row belonging to that person, including ones stamped to institutions the approver cannot read. That is
  inherent to erasing a person rather than a record, and cannot be fixed by scoping the request; it needs
  a decision about whether a cross-institution erasure requires a global Administrator.
- **Privileged reads leave no audit row.** `AuditPipelineBehavior` audits `*Command` only, so
  `GetDataRightsRequestByIdQuery` is unaudited. See [T114].
