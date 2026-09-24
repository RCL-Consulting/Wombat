---
id: T205
title: MSF respondents have no page to answer on: the invitation link leads to a JSON endpoint on dev and to nothing in production
status: queued
priority: P1
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High. MSF is a v11.1 instrument on every EPA (D37), and no respondent can answer a campaign, so no MSF
evidence can be collected anywhere. Everything upstream (campaigns, invitations, release, evidence rows) works only
with test data.
**Surfaced:** 2026-09-24, the T202 review (finding 1, confirmed in code and deploy config).

## Symptom

- Wombat.Web has no respond route.
- `Wombat:MsfRespondUrl` points at the Api's JSON endpoint on dev (`src/Wombat.Web/appsettings.Development.json:11`,
  port 5090; `src/Wombat.Api/Endpoints/MsfRespond.cs`).
- `deploy/deploy.ps1:49` and `deploy.sh` publish only Wombat.Web, and `deploy/Caddyfile.wombat:14` sends everything to
  5080. `execution/architecture/INFRASTRUCTURE.md:145` points `Wombat__MsfRespondUrl` at the Web host, so production
  answers NotFound.
- T202 made the Api endpoint answer a readable 4xx for dead links, but a respondent still needs a page with the
  questionnaire.

## What to build

An anonymous respondent page in Wombat.Web, for example `/msf/respond/{token}`:
- It is reached from the invitation link, and is rate-limited like the Api endpoint.
- It shows the questionnaire (the template's questions and scale), whom the feedback is for, and the deadline.
- It submits the response through the same MediatR command the Api uses (`MsfCampaignRules`), and says plainly when a
  link is expired, used or revoked (T202's reasons).
- It has no layout chrome needing sign-in, and its CSP holds.

Then point `Wombat:MsfRespondUrl` at it on dev and in production, keep or retire the Api endpoint (decide), and fix
INFRASTRUCTURE.md and the deploy docs. Check the anonymity rules (the respondent sees nothing about other responses).

## Verification

- [ ] Browser: from a campaign opened on dev, take the invitation link from the dev email log (or the admin link) and
      open it signed out. The questionnaire renders, a response submits, the campaign's response count goes up, and the
      same link then says it has been used.
- [ ] An expired, used or revoked link gets its plain message, and no 500. Tests.
- [ ] The response path is anonymous, rate-limited and CSP-clean. Tests and a browser check.
- [ ] T202's fourth verification item (email link to a submitted response) is ticked here.

## Related

T121 (MSF), T132 (the respond URL setting), T202, T184, INFRASTRUCTURE.md.
