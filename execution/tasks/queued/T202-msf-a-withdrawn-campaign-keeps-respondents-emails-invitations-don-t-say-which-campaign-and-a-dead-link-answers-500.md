---
id: T202
title: MSF: a withdrawn campaign keeps respondents' emails, invitations don't say which campaign, and a dead link answers 500
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

- [ ] A withdrawn campaign's invitations are anonymised. Domain test.
- [ ] A used link answers with a readable refusal, not 500. Api test.
- [ ] Open cannot be sent twice from the page. bUnit.
- [ ] The respondent's path, from email link to submitted response, works end to end on dev. Browser.

## Related

T184, T121, T132, T163.
