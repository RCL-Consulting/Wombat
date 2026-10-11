---
id: T205
title: MSF respondents have no page to answer on: the invitation link leads to a JSON endpoint on dev and to nothing in production
status: done
priority: P1
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T205 — MSF respondents have no page to answer on: the invitation link leads to a JSON endpoint on dev and to nothing in production

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

- [x] Browser: from a campaign opened on dev, take the invitation link from the dev email log (or the admin link) and
      open it signed out. The questionnaire renders, a response submits, the campaign's response count goes up, and the
      same link then says it has been used.
- [x] An expired, used or revoked link gets its plain message, and no 500. Tests.
- [x] The response path is anonymous, rate-limited and CSP-clean. Tests and a browser check.
- [x] T202's fourth verification item (email link to a submitted response) is ticked here.

## Related

T121 (MSF), T132 (the respond URL setting), T202, T184, INFRASTRUCTURE.md.

---

## As built — 2026-09-24

`/msf/respond?token=…` is an anonymous, **server-rendered** page, not interactive, so the submit passes through the rate
limit and needs no Blazor circuit.
- It uses the Api endpoint's query and command. The Api stays as the integration endpoint, with one refusal-to-status
  mapping (404 not recognised; 410 used, expired, revoked or closed; 400 incomplete; a 500 names nothing).
- **Rate limits:** 10 a minute per link per address, and 60 a minute per address.
- A malformed token is refused before any database read.
- `IAnonymousAuditedCommand`: the respondent is never named on the audit row, even when signed in.
- `MsfRespondUrl` defaults to `{BaseUrl}/msf/respond` and is validated.
- A question with no scale uses the interim 5-point default (D47). Ratings outside the scale, and comments over 4000
  characters, are refused.

Browser on dev (scripted Chrome, `5f9db98`, a local SMTP sink capturing the mail): 
- Campaign 6's email reads "Feedback request: Demo Trainee (Default MSF, 2026-09-24 to 2026-10-08)", with a
  `localhost:5080/msf/respond?token=` link and "The last day to respond is 2026-10-08."
- Signed out, the page renders "Feedback on Demo Trainee" with the five labels. The submit gives "Thank you".
- The same link again answers 410 "Feedback link already used", and a mangled token 404.
- The audit row has no actor, display name or user agent, and a redacted token and answers.

T202's fourth verification item (email link to a submitted response) is met here.
