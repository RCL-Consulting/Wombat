---
id: T286
title: Account leftovers: a password change writes no audit row, institutional sign-in links cannot be listed or removed, and emails are unique only by check
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T286 — Account leftovers: a password change writes no audit row, institutional sign-in links cannot be listed or removed, and emails are unique only by check

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T265 and T155 reviews.

## Symptom and what to build

- **Audit.** A self-service password change writes no audit row, for success, refusal or lockout. Sign-in writes
  `Login`, `LoginFailed` and `LoginLockedOut`; write the same for the password change.
- **External logins.** No page lists them, and only an institution move or erasure removes one. List them to the account
  holder and an admin, allow removal, and decide whether a password reset drops them.
- **Unique emails.** `RequireUniqueEmail` is false and the `NormalizedEmail` index is not unique, so only the handler's
  check keeps emails unique. Add a unique partial index on `NormalizedEmail` (`WHERE "NormalizedEmail" IS NOT NULL`,
  because erasure nulls it) in a migration. Do not use `RequireUniqueEmail`: its validator would refuse erasure's null
  update.
- **Profile.** It shows "Change password" to accounts with `AllowLocalPassword == false`. Hide it.

## Verification

- [ ] Each, with tests. The index refuses a concurrent duplicate (Postgres test).

## Related

T265, T155, T258.

Notes, 2026-09-25 (the T285 review):
- **A link race answers 500.** In `ExternalLoginHandler.LinkAndSignInAsync`, two link requests that both pass
  `AddLoginAsync`'s internal check break the `AspNetUserLogins` key, and nothing catches the `DbUpdateException`. Catch
  it as `ProvisionNewUserAsync` does, discard, reload, and return `AlreadyLinked`.
- **A failed role leaves an account.** `InvitedUserProvisioner.ProvisionAsync` throws a bare exception if
  `AddToRoleAsync` fails after `CreateAsync` has saved the account, so the account stays without a role and the
  invitation stays unused.
- **Issuing never checks the address.** `IssueInvitation` should refuse an address that already has an account or that
  Identity will not accept, through T285's `GetAddressStatusAsync`.

Notes, 2026-09-25 (the mail chain's reviews):
- **Identity descriptions in exceptions.** Identity's error descriptions quote the address and are put into exception
  text that reaches audit rows and logs: `UserAdministrationService` (two places) and `InvitedUserProvisioner` (two
  places). `DevUserSeeder` does the same (dev only). Map them to fixed messages.
- **The audit display name.** `HttpAuditContextProvider` falls back to `ClaimTypes.Name`, which is the address.
- **A throwing logger** on the mail worker's retry or outcome path stops the worker.
- **`RevokeInvitation`'s check.** Through the command, a CollegeAdmin can revoke their College's CollegeAdmin
  invitation, although the comment says Administrator-only (T093). The page does not reach it. Apply T283's
  `IsAdministrator()` rule.

## Notes

- **T295 replay, 2026-09-26 (C51).** Notes, 2026-09-26 (the T295 replay, Step A.4.2): the replay confirms the audit item in the browser. Dr Khumalo made four tries: (a) a wrong current password, (b) a mismatched confirmation, (c) a password breaking four rules, and (d) a good one. They wrote no AuditEntries row between 13:47 and 13:51 UTC besides her Login and Logout. In the same part, the wrong sign-in at A.4.3 wrote LoginFailed, and the administrator's reset at A.4.5 wrote ResetUserPasswordCommand twice (false, then true) (F-A.4.2a; `design/baseline/act-A/A.4.2-1-incorrect.png`, `A.4.2-3-rules.png`). The endpoint (Program.cs:479-583, `ChangePasswordOutcome.SubmitPath`) is a minimal-API handler, not a MediatR command, so the audit pipeline never sees it. Write the rows from the endpoint with `IAuditWriter`, as the sign-in and sign-out endpoints do (Program.cs:280, :617), stamped with the institution read before any sign-out. This does not widen the task. Once it is fixed, add the rows to runbook Step A.4.2's Expect.
- **T295 replay, 2026-09-26 (C64).** Note, 2026-09-26 (T295 replay, steps 3.55-3.56): the audit display name, observed. In /admin/audit, command rows and Logout rows show the actor as 'ndlovu@kgk.wombat.local', while Login rows show 'Sipho Ndlovu', so one person appears under two names in the same list. In the detail of Ndlovu's refused CreateActivity, the Actor card labels the address 'Display name'. Screenshots: design/baseline/act-3/3.55-1-audit-kgk-from-sitting.png and 3.56-1-audit-failure-detail.png. This widens the item by one site. The Logout row (src/Wombat.Web/Program.cs:607) also reads ClaimTypes.Name, alongside HttpAuditContextProvider.cs:29. Login (Program.cs:230) and the SSO rows (ExternalLoginHandler.cs:435, :535, :770) write 'First Last'. Give every row the same 'First Last' display from one source, either a name claim issued at sign-in or the account itself, and never the address. Verify by replaying 3.55-3.56: every Ndlovu row, and the Actor card, reads his name.
- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26 (register--account-exists).** The 'Issuing never checks the address' item was observed. As Prof Mbatha on /admin/invitations, an Assessor invitation to botha@kgk.wombat.local, who already has an account, was accepted and handed to the mail worker ('Being sent'). The invitee finds out only on opening the link, where the preview refuses it: 'A user with this email address already exists.' (design/baseline/states/register--account-exists.png). IssueInvitationCommandHandler.Handle (IssueInvitation.cs:60-126) stores (:111) and mails (:117) with no address check. Only GetInvitationPreview.cs:45 and InvitedUserProvisioner.cs:57 ask GetAddressStatusAsync. This widens the item by the page the invitee lands on: Register.razor:21-25 shows the refusal alone, with no Sign in or reset link, so a person who already has an account is left at a dead end. Build: refuse at issue with the same InvitationRefusal.AccountExists, shown on /admin/invitations in the page's words, before anything is stored or mailed (the audit trap). Also give the register page's unavailable branch a Sign in link, because an account created after the issue still reaches it. Verify: a handler test that an issue to a taken address is refused and stores and mails nothing; a bUnit test that the unavailable register page offers Sign in; then replay the states row, whose Expect becomes the refusal on /admin/invitations.
