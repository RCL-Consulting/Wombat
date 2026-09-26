---
id: T330
title: The reconnect modal shows 'Rejoining the server' and 'Rejoin failed' at once while retrying, and resume-failed says retry beside a Resume button
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T330 — The reconnect modal shows 'Rejoining the server' and 'Rejoin failed' at once while retrying, and resume-failed says retry beside a Resume button

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Nothing is lost. During an outage the modal contradicts itself on every retry after the first, and in resume-failed the text and the button disagree.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings S-shell--reconnect-retrying, S-shell--reconnect-resume-failed).

## Symptom

- shell--reconnect-retrying.png (Dr Zulu, app stopped): while retrying, the modal shows 'Rejoining the server...' above 'Rejoin failed... trying again in 5 seconds.' A MutationObserver on #components-reconnect-modal logged 'components-reconnect-show', and 15 ms later 'components-reconnect-show components-reconnect-retrying'. That class list stayed for every retry that followed.
- shell--reconnect-resume-failed.png (paused with Blazor.pauseCircuit(), then the app stopped): 'Failed to resume the session. Please retry or reload the page.' sits above one button, labelled 'Resume'.

## Root cause

- The Blazor runtime (blazor.web.js 10.0.9, the custom modal's update(), read in the NuGet cache) adds components-reconnect-retrying when the attempt is greater than 1, and never removes components-reconnect-show. Only show(), hide(), failed() and rejected() clear the classes.
- src/Wombat.Web/Components/Layout/ReconnectModal.razor.css:10 displays .components-reconnect-first-attempt-visible under .components-reconnect-show, and no rule hides it under .components-reconnect-retrying. The component's three files are the .NET 10 template's, unchanged.
- ReconnectModal.razor:24-28: the resume-failed paragraph says 'retry', but the state's only button (components-resume-button) reads 'Resume'. It runs resume() (ReconnectModal.razor.js:48-57), which retries Blazor.resumeCircuit() and reloads when the circuit is gone. So the button is the retry, and only the words disagree.

## What to build

- CSS: add `#components-reconnect-modal.components-reconnect-retrying .components-reconnect-first-attempt-visible { display: none; }` after the display:block list, so that each state shows one line.
- Copy: in resume-failed, make the text and the button agree. For example, 'Failed to resume the session. Try again, or reload the page.' with a 'Try again' button for that state, wired to resume(). Or keep 'Resume' and write 'Press Resume to try again, or reload the page.' Keep the reconnect-failed state's 'Retry' consistent with whichever is chosen.
- Colours stay on tokens (DESIGN.md).

## Verification

- [ ] A design test in the DesignSystemSmokeTests style reads ReconnectModal.razor.css and asserts that a retrying rule hides .components-reconnect-first-attempt-visible. A markup test asserts that the resume-failed text names the label of the button shown with it.
- [ ] Browser, on a scratch app, the states.md reconnect rows (states.md:100-104). Rejoining still shows only 'Rejoining the server...'. Retrying shows only 'Rejoin failed... trying again in N seconds.' In resume-failed, the text and the button agree. Retake shell--reconnect-rejoining, shell--reconnect-retrying and shell--reconnect-resume-failed.

## Related

states.md reconnect rows (100-104). T321 (the unhandled-error bar, the shell's other failure state). T328 (stylesheet leftovers; it does not cover this component).
