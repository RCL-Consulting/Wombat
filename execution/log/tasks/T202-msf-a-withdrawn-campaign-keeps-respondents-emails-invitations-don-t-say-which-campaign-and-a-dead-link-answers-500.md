---
id: T202
title: MSF: a withdrawn campaign keeps respondents' emails, invitations don't say which campaign, and a dead link answers 500
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T202 — MSF: a withdrawn campaign keeps respondents' emails, invitations don't say which campaign, and a dead link answers 500

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium: privacy, plus the respondent's experience.
**Surfaced:** 2026-09-24, the T184 review (findings 1 and 3, confirmed in code).

## Symptom

1. `MsfCampaign.Withdraw` (`src/Wombat.Domain/MultiSourceFeedback/MsfCampaign.cs:160`) never anonymises, so a
   withdrawn campaign keeps every respondent's email indefinitely. Close anonymises; withdraw does not.
2. An invitation identifies its campaign only by the template name ("MSF request: {Template.Name}"). Two campaigns on
   the same template send identical-looking invitations.
3. `Wombat:MsfRespondUrl` points at the Api's JSON endpoint (`src/Wombat.Api/Endpoints/MsfRespond.cs`), and the Api has
   no exception handler. A dead or used link throws `InvalidOperationException` and the respondent gets a bare 500.
   **Confirm how production routes the respond URL:** whether respondents get a page at all needs checking against
   T121 and T132.
4. The "Open campaign" button is not disabled while the open runs, so a double-click races two opens. Every respondent
   then gets two invitations, one of them dead.

## What to build

`Withdraw` anonymises through T184's shared routine. The invitation names the trainee's campaign (template, window,
and the trainee's name or initials, whatever MSF anonymity allows). A dead link gets a friendly page (or a 4xx with a
readable body) that explains it. The Open button is disabled while in flight.

## Verification

- [x] A withdrawn campaign's invitations are anonymised. Domain test.
- [x] A used link answers with a readable refusal, not 500. Api test.
- [x] Open cannot be sent twice from the page. bUnit.
- [ ] ~~The respondent's path, from email link to submitted response, works end to end on dev. Browser.

## Related

T184, T121, T132, T163.

---

## As built — 2026-09-24

- `MsfCampaign.Withdraw` anonymises through T184's routine.
- The invitation names the trainee, the template and one true deadline (the earlier of ClosesOn and the expiry), and
  refuses to open without the trainee's name.
- The Api respond endpoint answers each refusal with a readable 4xx: used, unknown, closed, expired 410, revoked 410.
  A fault answers a problem-details 500 with no message.
- The Open button is disabled while in flight.
- Tests throughout, mutation-checked.

**Verification item 4 (email link to a submitted response) moves to [T205]**: respondents have no page to answer
on. Also filed: [T206] (the expiry reminder never sends; there is no withdraw action; multi-tab invites) and
[T207] (the unsalted email hash).
