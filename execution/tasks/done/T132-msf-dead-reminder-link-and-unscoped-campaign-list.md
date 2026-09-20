---
id: T132
title: "Every MSF expiry reminder mails a dead, unclickable link"
status: done
priority: P1
owner: agent
depends_on: []
created: 2026-09-20
started: 2026-09-20
completed: 2026-09-20
---

# T132 — The expiry reminder mails the token hash, as a relative URL

**Severity:** High, and on [T121]'s critical path. MSF is the most-required instrument in the
v11.1 catalogue and the one the product cannot credit. A campaign that never reaches its
8-response minimum can never be released and can never credit — and the one mechanism for
chasing a non-responder sends them a link that cannot work.
**Surfaced:** 2026-09-20, looking for EPA work that is not gated on the College reply ([T129]).
Listed in `knowledge/EPA-PROGRAMME.md` § 2 as "MSF defects — READY", never filed.

## Symptom

`MsfInvitationExpiryReminderJob.cs:57`:

```csharp
var responseUrl = $"/msf/respond?token={Uri.EscapeDataString(invitation.TokenHash)}";
```

Two defects in one line.

1. **It sends the hash, not the token.** `TokenHash` is a hex SHA-256 of the token
   (`InvitationTokenService.HashToken`). `MsfCampaignRules.GetActiveInvitationByTokenAsync`
   verifies a submitted value by hashing it and comparing — so the hash arrives, gets hashed
   again, and matches nothing. Every reminder link resolves to an invalid token.
2. **It is a relative URL in an email.** `OpenMsfCampaign` builds its link from the configured
   absolute `Wombat:MsfRespondUrl` (`OpenMsfCampaign.cs:82-94`, which validates it is
   absolute). The reminder hardcodes `/msf/respond`, which is not clickable in a mail client.

So the reminder is not merely broken — it is broken twice, and neither failure is visible from
inside the product. The job logs `sent {Count} expiry reminders` and exits successfully.

## Root cause

The plaintext token is **not recoverable** at reminder time, by design: only the hash is
stored, hashed one-way. Whoever wrote the job reached for the nearest field that looked like a
token.

`OpenMsfCampaign.cs:55-56` is the working precedent — it generates a token, stores the hash,
and emails the plaintext, all in one pass. The reminder job never got the same treatment.

## Decision — how the reminder obtains a usable token

Since the plaintext cannot be recovered, the reminder must **re-issue**: generate a fresh
token, overwrite `TokenHash`, and mail the new link.

**The trade-off, stated plainly: this invalidates the link sent when the campaign opened.** A
respondent holding both emails and clicking the older one gets an invalid token. The reminder
email must therefore say that it replaces any earlier link.

*Rejected:* storing the plaintext token, which trades a dead link for a credential at rest.
*Rejected for now:* a "resend my link" page keyed by campaign and email — better UX, no
invalidation, but it is a new page and handler, and the reminder is still dead until it ships.
Worth doing later; this task does not block it.

## What to build

1. **One implementation of the respond URL.** `OpenMsfCampaign.GetRespondUrl()` validates
   `Wombat:MsfRespondUrl` is present and absolute. Extract it so the job and the command share
   it and cannot drift — the same reasoning that gives `ActorRuleMatcher` one implementation.
2. **Re-issue in the job.** Inject `IInvitationTokenService` and `IOptions<WombatOptions>`;
   generate, hash, assign, build the absolute URL from the plaintext, and **save** — the job
   currently calls no `SaveChangesAsync`, so a new hash would be lost without it.
3. **Say so in the email.** `MsfExpiryReminderEmail` must state that the link replaces any
   earlier one.

## Explicitly not in scope

**`ListMsfCampaignsForCoordinator` returns every campaign in every institution** — no
parameters, no principal, no filter, exposing `SubjectUserId` and response counts nationally.
It is real and it is confirmed, but `MsfCampaign` carries no institution, so scoping it means
the `TraineeProfile` resolver — and `ExportPortfolio.ResolveTraineeScopeAsync` is a deliberate
three-step resolver with a comment explaining why it is not one join. Copying it here would be
the **third** copy, which is precisely what [T113] says to avoid: *"Duplicating it would be a
third copy — extract it into one shared helper."*

**Folded into [T113]** as a third site, to be done when that helper is extracted. Also noted
there: no MSF command or query anywhere takes a `ClaimsPrincipal` — verified by grep across
`Features/MultiSourceFeedback/`, zero files.

## Verification

All checked 2026-09-20. Five new tests in
`tests/Wombat.Application.Tests/Scheduling/MsfInvitationExpiryReminderJobTests.cs`.

- [x] The reminder's URL carries a token that `VerifyToken` accepts against the stored hash —
      `ExecuteAsync_MailsATokenThatVerifiesAgainstTheStoredHash`
- [x] The URL is absolute and built from `Wombat:MsfRespondUrl` —
      `ExecuteAsync_MailsAnAbsoluteUrlBuiltFromConfiguration`
- [x] The new hash is persisted — `ExecuteAsync_PersistsTheReissuedHash`, which re-reads the
      invitation in a fresh scope
- [x] The token issued at open stops working after a reminder —
      `ExecuteAsync_RetiresTheTokenIssuedWhenTheCampaignOpened`, which also asserts the email says so
- [x] A missing `MsfRespondUrl` sends nothing and retires nothing —
      `ExecuteAsync_RefusesToSendWhenTheRespondUrlIsNotConfigured`. Added beyond the original list:
      resolving the URL after the loop would have burned every working token on a config error.
- [x] The respond URL has one implementation — `WombatOptionsExtensions.RequireMsfRespondUrl`,
      called by both `OpenMsfCampaignCommandHandler` and the job. `OpenMsfCampaign.GetRespondUrl`
      is deleted.
- [x] **Verified to fail against the pre-fix code.** Reverting line 76 to the old
      `TokenHash`-in-a-relative-URL form fails 4 of the 5. The fifth guards the config check,
      which that revert does not touch — correct, and worth stating so nobody reads it as a gap.
- [x] `dotnet build Wombat.sln -c Release` — 0 warnings, 0 errors
- [x] Suites green, no `--no-build`: Application **508**, Infrastructure **177**, Domain **69**,
      Web **103**, Architecture **23**. Total **880**, up from 875.

## Related

[T121] (MSF cannot credit an EPA — this is on its critical path), [T113] (inherits the
coordinator-list scope defect), [T129] (the College message; this task is deliberately
independent of it). § 2 of `knowledge/EPA-PROGRAMME.md` lists this row as READY.

## Notes

- **Observed:** `AddMsfInvitation.cs:57` writes `HashToken(GenerateToken())` — it hashes a
  token it immediately discards. Harmless, because `OpenMsfCampaign` overwrites the hash with
  one whose plaintext is actually mailed, but it reads as though a usable token was issued at
  invitation time. Worth a comment; not a defect.
- **Observed:** the job filters on `Campaign.State == Open` and unresponded, unrevoked,
  unanonymised invitations, which is correct. Only the URL is wrong.
