# T083 — Data-rights export "Download" returns 404 (endpoint never mapped)

**Status:** 🔴 OPEN — found 2026-06-04 in Appendix A.1.1 play-through. **HIGH** (GDPR deliverable unreachable).

## Problem (F-A1-2)

A trainee's Access/Export data-rights request can be submitted and admin-approved → the request flips to
`Completed`, and the trainee's `/account/data-rights` page shows a **Download** link
(`/account/data-rights/download/{id}` — `DataRights.razor:107`). **Clicking it 404s** — the export
cannot be downloaded.

Root cause: there is **no HTTP endpoint** mapped for `/account/data-rights/download/{id}`. `Program.cs`
maps `/account/login/submit`, `/account/register/submit`, the SSO routes, `/account/logout`, and
`/dashboard/switch/{role}` — but **not** the data-rights download. The `DownloadAccessReportQuery`
handler (`Wombat.Application/Features/DataRights/Queries/DownloadAccessReport.cs`) and
`AccessReportBuilder` (`Wombat.Infrastructure/DataRights`) exist, but **nothing in `Wombat.Web`
references `DownloadAccessReportQuery` or `AccessExportResult`** (grep: zero matches) — the query is
never dispatched over HTTP.

So Access/Export requests complete successfully but the export artifact (the GDPR Art. 15/20
deliverable) is **undeliverable**.

**Live-verified (2026-06-04):** approved Mahlangu's Export (status → `Completed`); the `Download` link
both via in-page `fetch` and via click returns **404** (`chrome-error` page on navigation).

## Fix outline

- Add `app.MapGet("/account/data-rights/download/{id:guid}", ...)` in `Program.cs` (authenticated):
  resolve the current user, send `DownloadAccessReportQuery(id, userId)` (the handler/validator already
  enforce ownership), and return the `AccessExportResult` as a file (`Results.File(bytes, contentType,
  fileName)`). Mirror the auth pattern of the existing `MapPost` account endpoints.
- Confirm `AccessExportResult` carries content-type + filename (ZIP per `AccessReportBuilder`).
- Add an endpoint/integration test: approve an Export → GET the download route → 200 + ZIP bytes; and a
  negative test that another user gets 404/403.

## Notes

The handler builds the report **on-demand at download time** (by design — no async export worker), so the
only missing piece is the HTTP wiring. Erasure download is N/A (erasure has no artifact).
