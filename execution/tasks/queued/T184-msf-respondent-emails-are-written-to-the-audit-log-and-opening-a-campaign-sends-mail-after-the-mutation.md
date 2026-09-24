---
id: T184
title: MSF: respondent emails are written to the audit log, and opening a campaign sends mail after the mutation
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
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

- [ ] The audit row for AddInvitation holds no email. Test.
- [ ] A failing send leaves the campaign unopened, with no token rotated. Test (save, then clear the tracker).
- [ ] The job and the command anonymise through one method. Test, or a grep in the as-built.

## Related

T113, T121, T132; the audit-trap memory.
