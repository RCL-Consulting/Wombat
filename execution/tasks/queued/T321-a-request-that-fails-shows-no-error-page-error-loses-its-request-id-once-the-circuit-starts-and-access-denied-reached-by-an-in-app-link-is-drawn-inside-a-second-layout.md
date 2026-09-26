---
id: T321
title: A request that fails shows no error page, /Error loses its request id once the circuit starts, and Access denied reached by an in-app link is drawn inside a second layout
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T321 — A request that fails shows no error page, /Error loses its request id once the circuit starts, and Access denied reached by an in-app link is drawn inside a second layout

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. In production an unhandled failure in a request answers a bare 500 with no body, so the browser shows its own error screen. The person has no request id to quote, and the log has none to find. The page meant for this cannot keep its id even when reached. The nested layout draws two navigation sidebars, two top rows and two Sign out buttons on every in-app link to a page the role may not open, which duplicates the landmarks a screen reader navigates by. None of this is a disclosure: production shows no stack trace.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-A.5.8a, F-A.6.3c).

## Symptom

- Step A.5.8 (Dr Dlamini, signed in, types /Error): the prerendered HTML carries 'Request ID: 00-05f81b4e…-00', and the interactive render replaces it with nothing. The page ends at 'If this keeps happening, please contact your administrator.', with nothing to quote. Screenshots: design/baseline/act-A/A.5.8-1-error-signed-in.png, design/baseline/states/error--signed-in.png.
- No failure leads to /Error at all (A.5.8's Note). An unhandled exception in a request gives the developer exception page on dev and an empty 500 in production. Signed out, /Error redirects to sign-in (A.5.8-2-error-signed-out.png). So even once the handler is wired, a failure on a page open to someone not signed in (the sign-in post, the MSF respondent's page) would be sent to a sign-in form.
- Step A.6.3 (Prof Mbatha): Back to institutions on KGK's edit page renders Access denied in place at /admin/institutions, inside a second copy of MainLayout: two sidebars, two top rows and two Sign out buttons. Playwright's `getByRole('button', {name: 'Sign out'})` matched 2. Screenshot: design/baseline/act-A/A.6.3-4-back-to-institutions-denied.png. A typed address goes to /access-denied?ReturnUrl=… instead, and is drawn once (A.5.1, A.5.2).

## Root cause

- `src/Wombat.Web/Program.cs:127-161` has no `UseExceptionHandler`; only `src/Wombat.Api/Program.cs:22` has one. WebApplication adds the developer page in Development. Elsewhere nothing catches, and Kestrel answers 500 with no body. T233 closed the same gap for 404 (`Navigation/NotFoundPages.cs:54`, `UseStatusCodePagesWithReExecute`) but not for 500.
- `src/Wombat.Web/Components/Pages/Error.razor:23-24` reads `Activity.Current?.Id ?? HttpContext?.TraceIdentifier` in OnInitialized.
  - It has no `[ExcludeFromInteractiveRouting]`, so for a signed-in user `App.razor:84-88` renders it InteractiveServer. It runs again in the circuit, where the cascaded HttpContext and `Activity.Current` are null, and `ShowRequestId` turns false.
  - It has no `[AllowAnonymous]`, so the fallback policy sends a visitor who is not signed in to sign in.
- No log line carries the id. No `IncludeScopes` is set in appsettings or Program.cs, so the console logger writes no request scope, and an id a person quoted could not be found in the journal. (Inferred from configuration; the journal was not checked.)
- `src/Wombat.Web/Components/Routes.razor:8-10` wraps `<AccessDenied />` in `<LayoutView Layout="typeof(Layout.MainLayout)">` inside AuthorizeRouteView's NotAuthorized. AuthorizeRouteView already renders NotAuthorized content in its `DefaultLayout`, MainLayout (:4), so the page gets MainLayout twice. Only in-circuit navigation reaches this branch. A full load is refused by endpoint authorization and redirected to /access-denied, which is drawn once. The LayoutView dates from 0caee6a, before T010, and was never a choice. The cause is inferred from the code and the framework's AuthorizeRouteView; no fix has proven it.

## What to build

- **Wire the exception handler.** Outside Development, Web's pipeline calls `UseExceptionHandler("/Error", createScopeForErrors: true)`. Place it after `SecurityHeadersMiddleware`, so the page carries the CSP header and its nonce, and confirm that the handler's response reset keeps them, as T233 did for /not-found. Only a browser's GET for HTML gets the page, as for 404; every other request keeps a bare 500.
- **Make Error.razor static and open to everyone.** Mark it `[ExcludeFromInteractiveRouting]` and `[AllowAnonymous]`:
  - static, it renders once, inside the failed request, so the id it shows is that request's;
  - it is open to a visitor who is not signed in and to an MSF respondent, and shows nothing about any account;
  - the rerun still answers 500.
  - Typed directly, it may say there is nothing to report.
- **Log the id the page shows.** Turn on scopes for the console logger, or log the id explicitly with the exception, so an administrator can search the journal for the id a person quotes.
- **Out of scope:** failures inside a circuit, which show MainLayout.razor:30's 'An unhandled error has occurred' bar. Note whether that bar should carry an id too.
- **Draw Access denied once.** In Routes.razor, remove the inner LayoutView and render `<AccessDenied />` directly, so AuthorizeRouteView's DefaultLayout draws it once. The alternative is to navigate to /access-denied?ReturnUrl=…, so both paths land on the same address; pick one and record it.
- **Update the runbook.** A.5.8's Expect changes: a failure leads here, the page shows signed out too, and its id stays on screen. Update A.6.3's Actual as well.

## Verification

- [ ] Integration test modelled on tests/Wombat.Integration.Tests/Hosting/NotFoundPageFlowTests.cs: a test endpoint that throws answers 500 with the 'Something went wrong' page, the CSP header, the page's nonce and a request id. The same holds signed out. A POST, or a request that does not accept HTML, gets no page.
- [ ] Test with a captured logger: the id on the page appears in the logged failure.
- [ ] Test that Error.razor's route renders statically for a signed-in user (no interactive root), so its id survives.
- [ ] bUnit test on Routes: a page the signed-in user may not open renders Access denied with one nav and one Sign out button.
- [ ] Browser, replaying A.6.3 as Prof Mbatha: Back to institutions shows Access denied with one sidebar and one Sign out. Replace design/baseline/act-A/A.6.3-4-back-to-institutions-denied.png.
- [ ] Browser, replaying A.5.8 signed in and signed out, at 390 px: the request id is still on screen after the page settles. Update the runbook's A.5.8 Expect.

## Related

T233 (the same gap for 404: NotFoundPages), T181 and T265 (static pages for visitors who are not signed in, and ExcludeFromInteractiveRouting), T205 (the MSF respondent page is static), F-A.6.3b (Back to institutions leads an InstitutionalAdmin to a page that refuses her; filed with T291's class), runbook steps A.5.1, A.5.2, A.5.8 and A.6.3, and states error--signed-in.
