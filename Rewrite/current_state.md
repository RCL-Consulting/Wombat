# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T031 — Formative-only committee review mode.** Model: Sonnet.

First Block 2 task. Small extension to `CommitteeReview` — add an `IsFormative` boolean so committees can run interim progress check-ins that close without producing a binding `CommitteeDecision`. Ratification step is skipped for formative reviews. See scope in `Rewrite/practical-plan.md` §T031. ½–1 day.

## Last completed

**T030 — STAR certificate PDF + trainee "My authorisations" panel.**

- `EntrustmentCertificatePdfService` (QuestPDF, single-page A4 with header, decision body, evidence summary, signature block, revocation overlay if revoked, SHA-256 filename hash) lives in `src/Wombat.Infrastructure/Reporting/` alongside `PortfolioPdfService`. Interface `IEntrustmentCertificatePdfService` sits in Application so handlers can depend on the abstraction.
- `DownloadEntrustmentCertificateCommand` (application) with authorisation covering: Administrator / InstitutionalAdmin / SpecialityAdmin / SubSpecialityAdmin / Coordinator (all pass), trainee on own decision, and any chair/member of the issuing panel. Other roles get `UnauthorizedAccessException`.
- Trainee page `/portfolio/authorisations` — one card per `Active` decision with level, issue, expiry (yellow badge if within 30 days), rationale, and a "Download certificate" button that streams the PDF via the existing `wombatFileDownload` JS helper.
- TraineeDashboard gains a "My authorisations" card linking to the page. New `award.svg` Lucide icon added for it.
- Admin page `/admin/entrustment-decisions` (InstitutionalAdmin+) — table with trainee + status filters, per-row Download, and Revoke button that opens an in-place confirmation with a mandatory revocation reason. Linked from InstitutionalAdminDashboard Quick links.
- `ListEntrustmentDecisionsForAdminQuery` added (scope check: admin roles only; applies optional trainee + status filters).
- DI: `IEntrustmentCertificatePdfService` → `EntrustmentCertificatePdfService` registered alongside the portfolio PDF service.
- 6 new application handler tests covering authorisation (trainee-self allowed, other-trainee denied, admin allowed, issuing chair allowed, unrelated assessor denied) and missing-decision handling. PDF service is faked in tests; real-PDF integration is smoke-tested manually.

## Plan this session works against

`Rewrite/practical-plan.md` — the pragmatic post-rewrite plan. Four blocks, nine tasks (T028–T036). Block 1 now complete with T028–T030. Block 2 opens with T031.

## Block 2 sequence

1. T031 — formative-only committee review mode (active)
2. T032 — sampling-concentration warning on review detail

## Test status at handoff

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- Domain tests — 33/33 pass
- Application tests — 134/134 pass (6 new for certificate download)
- Architecture tests — 19/19 pass
- Web tests — 33/33 pass
- Infrastructure tests — pre-existing parallel-run flakiness on `SeedParseTests.AllSeedJsonFiles_ParseCleanly` (passes in isolation)
- Integration tests — require Docker / Testcontainers per CLAUDE.md (not run locally)

## Known T030 limitations

- **PDF visual QA was build-time only.** QuestPDF produced output; no manual browser render walkthrough in this session. Worth an eye on a generated certificate before T031 wraps.
- **Admin scope-filtering is role-based, not institution-scoped.** InstitutionalAdmin currently sees decisions across all institutions. If that becomes a policy concern, the admin query needs an institution claim filter — not blocking T031.
- **Chair signature is a line + "on file" label, not a cryptographic signature.** Matches the T029 spec; cryptographic signing is deferred indefinitely.

## What remains (operational, not code — carried forward from T016)

- Execute `deploy/README.md` first-boot checklist against a real Linode server
- Configure DNS, TLS certificate (Caddy auto-provisions via ACME)
- Set production secrets in `/opt/wombat/config/wombat.env`
- Run `--seed` to provision the admin user and seeded activity types
- Revoke UPDATE/DELETE on AuditEntries table after first migration

## Companion reference docs

- `EPA Book/evaluation.md` — 92-requirement book scorecard (reference, not todo list)
- `EPA Book/critique.md` — literature-backed reasoning for practical-plan compromises
- `Rewrite/book-fidelity-plan.md` — superseded; kept only because `critique.md` cites it

## Last verified commits

- `10f7e55` — T030 (STAR certificate PDF + authorisations UI)
- `21f7959` — docs: record T029 commit hash in current_state handoff
- `91ff841` — T029 (EntrustmentDecision aggregate / STAR)
- `dc506d1` — T028 (rename `star_reflection` → `reflective_note`)
- `bf583ee` — MailKit 4.16.0 security bump (closes GHSA-9j88-vvj5-vhgr)
- `864ad3b` — T016 (rewrite-complete baseline)
