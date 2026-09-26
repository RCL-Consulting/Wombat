---
id: T315
title: The invitation token stays in /account/register's address bar: the call that clears it runs in OnAfterRenderAsync, which a signed-out visitor's static page never runs
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T315 — The invitation token stays in /account/register's address bar: the call that clears it runs in OnAfterRenderAsync, which a signed-out visitor's static page never runs

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The token is a bearer credential for 14 days (Invitation.LinkLifetimeDays). Until the invitee registers, anyone who reopens the link from the address bar or the history of a shared ward computer can choose the password for that address and role. The product meant to clear it, and the clearing code is dead.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-2.8a).

## Symptom

Open an invitation link (runbook Step 2.8, and every register link at 2.9, 2.10, 2.17 and 2.18). The page reads "Registering <address> as <role>." The address bar still shows /account/register?token=<token> after the page has loaded, and keeps it until Register is pressed (F-2.8a; design/baseline/act-2/2.8-1-smit-register-form.png). Step 2.8's Expect says "The token is cleared from the address bar once the page loads." A refusal (Step 2.9) reloads the page as /account/register?token=<token>&error=PasswordTooShort, so the token is back in the bar and in another history entry.

## Root cause

Register.razor:139-148 clears the token with JS.InvokeVoidAsync("wombat.clearInvitationTokenFromUrl") in OnAfterRenderAsync. Since T181, App.razor:84-88 (PageRenderMode) gives a visitor who has not signed in no render mode. Every page such a visitor reaches is static server-side HTML with no circuit, so OnAfterRenderAsync never runs and JS interop has nothing to call. DESIGN.md § Account / auth page states the rule (lines 1887-1891) and already lists this call as dead signed out (lines 1919-1921). No task picked it up. RegisterPageTests.cs:39 runs bUnit's JSInterop in Loose mode, so the call passes silently in tests. RegisterOutcome.Url (RegisterOutcome.cs:89-104) also prepends token= to every refusal's redirect.

## What to build

- Clear the token by the means a static page has: `wombat.js`, which App.razor loads on every page. Mark the register form (for example `data-clear-invitation-token`). At load, wombat.js calls the existing `wombat.clearInvitationTokenFromUrl` when the marker is present, the same pattern `data-submit-once` uses. The hidden Token field (Register.razor:74) keeps the token for the post. The `?error=` codes stay in the address.
- A refused registration reloads the page with a full load, so the same script clears the token again. Nothing extra is needed for Step 2.9.
- Remove the OnAfterRenderAsync, the `_urlCleaned` field and the `IJSRuntime` inject from Register.razor. Remove the register half of DESIGN's "Two predate the rule and are dead signed out" sentence.
- A stronger variant, acceptable if the reviewer prefers it: the GET with `?token=` puts the token in a short-lived cookie (HttpOnly, SameSite=Strict, Secure, path /account/register, protected with Data Protection) and redirects to /account/register. The page and its refusals then never carry the token. With either approach, a visit the browser has already written to its global history cannot be removed from the page; the invitation's single use and 14-day life are the bound on that.
- Not in scope: `PasswordToggleButton`, which the same DESIGN sentence names as dead on the sign-in, register and link pages.

## Verification

- [ ] bUnit (RegisterPageTests): the form carries the marker on a first visit and after a refusal, and the component no longer injects IJSRuntime or overrides OnAfterRenderAsync.
- [ ] Integration (AccountRefusalFlowTests' register tests) still pass: the endpoint still reads the token from the form, and a refusal still returns to the usable form.
- [ ] Browser check, runbook Step 2.8 on a fresh register link: once the page has loaded, the address bar reads /account/register with no token; Register still signs Smit in and lands on Home 'Viewing as Coordinator'.
- [ ] Browser check, runbook Step 2.9: after the PasswordTooShort refusal the address reads /account/register?error=PasswordTooShort, and the third attempt registers Patel.
- [ ] DESIGN.md § Account / auth page no longer lists the register page's clearing as dead, and the next replay of Step 2.8 records no F-2.8a gap.

## Related

T181 (static pages for signed-out visitors), T285 (refusals as codes), T265; DESIGN.md § Account / auth page; runbook Steps 2.8, 2.9, 2.10, 2.17, 2.18; F-2.8a.
