---
id: T310
title: The STAR certificate names the issuing institution from whichever trainee profile the database returns first, not from the review that issued it
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T310 — The STAR certificate names the issuing institution from whichever trainee profile the database returns first, not from the review that issued it

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is wrong only for a trainee with more than one profile, which the scenario never creates. But the certificate is the medico-legal record (DOMAIN.md:78), so when it is wrong, it is wrong on the one document that must be right.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings from the code read).

## Symptom

Not observable in the replay. Step 4.35's PAED-010 certificate correctly named Kgosi Kgari Teaching Hospital, because each scenario trainee has one profile. A trainee can have more than one: one user may hold several inactive profiles beside one active one, and admission refuses only an existing active profile. Take a trainee with an ended profile at institution A and a newer profile at B. Every certificate she downloads, from /admin/entrustment-decisions or My authorisations, is headed by whichever profile PostgreSQL returns first. A STAR issued by a panel at B can print A, or the reverse, and the heading can change between downloads.

## Root cause

`EntrustmentCertificatePdfService.cs:238-245`: under the comment "the issuing institution is where the trainee trained", it reads `TraineeProfiles.FirstOrDefaultAsync(p => p.UserId == decision.TraineeUserId)` with no `IsActive` filter and no `OrderBy`, then heads the certificate with that profile's institution (`:67`, `:263`). Several profiles per user are possible: the unique index on `TraineeProfiles.UserId` is filtered on `IsActive` (`TraineeProfileConfiguration.cs:14-16`), and `AdmitTrainee.cs:55-61` refuses only an active profile. The query already loads `decision.IssuedByCommitteeReview.Panel` (`:221-222`), and `DecisionPanel.InstitutionId` is a non-null int (`DecisionPanel.cs:16`; every panel runs at one institution, T182). That institution is the one that issued the STAR.

## What to build

Head the certificate with the issuing panel's institution, `decision.IssuedByCommitteeReview.Panel.InstitutionId`, and drop the `TraineeProfile` read. Correct the comment: the issuer is the panel's institution, not wherever the trainee trains now. No other reporting or entrustment reader uses this lookup (checked). The revoked banner in the same method prints `RevokedByUserId` raw (`:189`, F-4.37a, triaged under another area). If that task is open when this is taken, fix both in one change.

## Verification

- [ ] Infrastructure test (EntrustmentCertificateTextLayerTests): a trainee with an inactive profile at institution A and an active one at B. A STAR issued by a panel at A prints A, and one issued at B prints B, regardless of profile insertion order.
- [ ] Browser: Step 4.35's PAED-010 certificate still names Kgosi Kgari Teaching Hospital.

## Related

F-4.37a (same method: the revoker is printed by raw id), T182 (every panel runs at one institution), T153 (a trainee who has left an institution), T091 (EPAs are national, so the issuer is not the EPA's). Runbook Step 4.35. Reviewer suspect: 'The certificate names the institution from an arbitrary trainee profile' (step 4.35).
