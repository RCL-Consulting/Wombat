---
id: T184
title: MSF: respondent emails are written to the audit log, and opening a campaign sends mail after the mutation
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The first undoes MSF's anonymisation in the one log that is kept longest. The second is the audit
trap: a failed send commits a half-opened campaign.
**Surfaced:** 2026-09-24, the T113 implementation and review notes. Pre-existing.

## Symptom

1. `AddMsfInvitationCommand.RespondentEmail` is not marked `[Redact]`, so every respondent's address is written into
   the audit log. The close-time anonymisation clears it from the invitations, but not from the audit trail.
2. `OpenMsfCampaignCommandHandler` sends the invitation emails after `campaign.Open()`. If a send throws,
   `AuditPipelineBehavior` saves the scoped DbContext from its `catch`, and that commits the open with some tokens
   already rotated.
3. `MsfCampaignAutoCloseJob` has its own copy of `AnonymizeInvitations`, which can drift from the command's.

## What to build

Redact the email (and any other respondent identifier on the MSF commands), and check the audit writer's redaction
test covers it. Move every throwable step of Open before the first mutation, or send after the save through an outbox
or a post-commit step. Share one anonymise routine between the job and the command.

## Verification

- [x] The audit row for AddInvitation holds no email. Test.
- [x] A failing send leaves the campaign unopened, with no token rotated. Test (save, then clear the tracker).
- [x] The job and the command anonymise through one method. Test, or a grep in the as-built.

## Related

T113, T121, T132; the audit-trap memory.

---

## As built — 2026-09-24

- `AddMsfInvitationCommand.RespondentEmail`, and the other respondent identifiers on MSF requests, are redacted in the
  audit log. The redaction test covers them.
- `OpenMsfCampaign` does every throwable step before `campaign.Open()`. A failed send leaves the campaign unopened with
  no token rotated (proved with a save and a tracker clear). A race between two opens is proved on Postgres: both mail,
  only the first open's tokens are stored.
- One anonymise routine is shared by Close and `MsfCampaignAutoCloseJob`.
- The Open success message no longer mentions "the logging email sender".

**Verification**
- Tests: Domain, Application, Web (2 bUnit) and Integration (`MsfOpenCampaignRacePostgresTests`). Every mutant was
  caught.
- **The browser checks were not run.** The MSF pages admit only Coordinator or Administrator. The verifying agent was
  blocked by the permission classifier from reading the dev admin credential, and was not allowed to work around it.
  The operator can run them in a minute:
  - In a new draft campaign, add an invitee. Its `/admin/audit` row reads `"respondentEmail":"[REDACTED]"`.
  - Open the campaign. The message reads "Campaign opened, and each respondent has been emailed a link to respond."
  - Close it. Each invitation has a null email and a hash, and `AnonymizedOn` equals `ClosedOn`.
- Dev's 15 `AddMsfInvitationCommand` audit rows written before T184 still hold addresses (scenario data). Redaction
  applies to new rows.

**Filed from the review:**
- [T201] a failed handler save loses its audit row;
- [T202] withdraw does not anonymise, the invitation names no campaign, a dead link answers 500, and Open can be
  double-sent.
