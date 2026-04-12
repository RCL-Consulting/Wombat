# T010 — Web layout, navigation, auth, and the design system

**Phase:** 3 — Web
**Depends on:** T002 (can start in parallel with the Phase 2 tasks). The **only** task that has a right to edit `wwwroot/app.css` section headings or `Components/Shared/*` contracts.
**Blocks:** T011, T019

## Goal

Deliver the full Blazor chrome plus the shared design system that every later UI task leans on: the layout shell, the sidebar nav, the auth pages, role-gated routing, `app.css` (every section enumerated in `../DESIGN.md`), the shared component library, and the four canonical page shapes (list / detail / form / dashboard) rendered with convincing stub data. No feature pages — those come in T011 and T019 — but every visual building block those tasks need must exist and be verified against `../DESIGN.md`.

This task is what turns the current scaffolding (8-line `MainLayout.razor`, 37-line `app.css`, raw `<h1>` + `<table class="table">` list pages) into something that matches the ClinicAssist reference in shape. **Read `../DESIGN.md` before touching any file in this task.** That file is the contract; this file says what to build against it.

**Reference folders** (read-only, not committed):
- `ClinicAssist.NET_ref_DO_NOT_COMMIT/` — the design gold standard. Look at `src/ClinicAssist.Web/wwwroot/app.css`, `Components/Layout/MainLayout.razor.css`, `Components/Layout/NavMenu.razor.css`, and any page-level `.razor` file to see the quality target.
- `Wombat_ref_old_DO_NOT_COMMIT/` — the old Wombat. The GUI is rough (Bootstrap-coupled, no tokens), but it shipped. Consult its `Views/` folder for page structure on Wombat-specific workflows (EPAs, curricula, committee views, trainee profiles) that ClinicAssist doesn't have. Apply the ClinicAssist design system to the old Wombat page structure.

## What to do

### 1. Icons

Copy a minimal set of Lucide SVGs into `src/Wombat.Web/wwwroot/icons/` — one file per icon, named `check.svg`, `plus.svg`, `chevron-right.svg`, `trash.svg`, `pencil.svg`, `search.svg`, `arrow-up.svg`, `arrow-down.svg`, `x.svg`, `alert-triangle.svg`, `info.svg`, `user.svg`, `users.svg`, `home.svg`, `book.svg`, `inbox.svg`, `calendar.svg`, `file-text.svg`, `settings.svg`, `log-out.svg`. Twenty icons is plenty for v1; add more per-task as they come up, not speculatively.

Build `Components/Shared/Icon.razor` as:

```razor
@* Usage: <Icon Name="check" /> or <Icon Name="check" Size="18" Class="me-1" /> *@
<svg class="icon @Class" width="@Size" height="@Size" aria-hidden="true">
  <use href="/icons/@Name.svg#i"></use>
</svg>

@code {
  [Parameter, EditorRequired] public string Name { get; set; } = default!;
  [Parameter] public int Size { get; set; } = 16;
  [Parameter] public string Class { get; set; } = "";
}
```

(Each SVG file in `wwwroot/icons/` must declare `id="i"` on its root so the `<use>` reference resolves. Fix the Lucide exports once, then forget about it.) Nav icons are the **exception** — they use CSS background-image data URIs in `NavMenu.razor.css`, matching ClinicAssist's pattern, because hovering then doesn't repaint a bunch of `<svg>`s. **No Bootstrap Icons font.**

### 2. MainLayout

`Components/Layout/MainLayout.razor`:

```razor
@inherits LayoutComponentBase

<div class="page">
  <div class="sidebar">
    <NavMenu />
  </div>

  <main>
    <div class="top-row px-4 auth">
      <AuthorizeView>
        <Authorized>
          <a href="/account/profile">@context.User.Identity?.Name</a>
          <form action="/account/logout" method="post" style="display:inline">
            <AntiforgeryToken />
            <button type="submit" class="btn btn-outline btn-sm">Sign out</button>
          </form>
        </Authorized>
        <NotAuthorized>
          <a href="/account/login">Sign in</a>
        </NotAuthorized>
      </AuthorizeView>
    </div>

    <article class="content px-4">
      @Body
    </article>
  </main>
</div>

<div id="blazor-error-ui" data-nosnippet>
  An unhandled error has occurred.
  <a href="." class="reload">Reload</a>
  <span class="dismiss">🗙</span>
</div>
```

`Components/Layout/MainLayout.razor.css` is ported from ClinicAssist verbatim (the 98 lines covering `.page`, `.sidebar`, `.top-row`, `@media (max-width: 640.98px)`, `@media (min-width: 641px)`, and `#blazor-error-ui`). The only tokens that change are the sidebar gradient variables — already defined in `:root` per `../DESIGN.md`.

### 3. NavMenu

`Components/Layout/NavMenu.razor` matches the nav item table in `../DESIGN.md § The NavMenu`. Each item is:

```razor
<div class="nav-item px-3">
  <NavLink class="nav-link" href="/admin/institutions">
    <span class="bi bi-institution-nav-menu" aria-hidden="true"></span>
    Institutions
  </NavLink>
</div>
```

wrapped in `<AuthorizeView Roles="…">` blocks so non-authorized users never see them server-side. The brand row at the top is just `<a class="navbar-brand" href="">Wombat</a>` with no logo glyph (logo arrives in a later task). Mobile collapse uses the `<input type="checkbox" class="navbar-toggler">` + CSS-only reveal trick from ClinicAssist's `NavMenu.razor.css` — no JS.

The initial nav set matches the table in `../DESIGN.md § The NavMenu`. Route paths to existing pages or to stubs under `Components/Pages/Placeholder/` — the stubs get replaced by T011, T019, etc., but every nav link must resolve to a real page so the smoke test passes.

Scoped styles go in `Components/Layout/NavMenu.razor.css`, again ported from ClinicAssist, including every nav icon's background-image data URI. Keep the class names (`nav-item`, `nav-link`, `nav-logout-button`, `nav-scrollable`, `navbar-toggler`, `bi bi-*-nav-menu`) — they match the ClinicAssist shape and muscle memory.

### 4. `app.css` — every section in `../DESIGN.md`

Produce `src/Wombat.Web/wwwroot/app.css` with all the sections listed in `../DESIGN.md § app.css section order`. In order, these are:

1. Design tokens (`:root`)
2. Base (body, h1–h5, `.page-subtitle`)
3. Header + search (`.header-container`, `.search-container`, `.search-input`, `.search-grid`, `.search-field`, `.search-hint`)
4. Tables (`.table-container`, `.clinic-table`, `.actions-cell`)
5. Buttons (`.btn`, `.btn-primary`, `.btn-success`, `.btn-danger`, `.btn-outline`, `.btn-sm`, `.btn-xs`; `:focus-visible` ring rules)
6. Forms (`.form-container`, `.form-grid`, `.form-grid--wide`, `.form-group`, `.full-width`, `.form-control`, `.form-select`, `.form-select-sm`, `.form-check`, `.form-check-input`, `.form-actions`)
7. Alerts (`.alert`, `.alert-danger`, `.alert-success`, `.alert-warning`, `.alert-info`)
8. Validation (`.validation-message`, `.validation-summary-errors`, `.input-validation-error`)
9. Cards (`.detail-card`, `.detail-card--compact`, `.detail-card--header`, `.detail-card--interactive`, `.detail-card--empty`, `.detail-card--empty-compact`)
10. Dashboard grid (`.dashboard-grid`, `.dashboard-span-2`, `.dashboard-span-3`, mobile collapse)
11. Details grid (`.details-grid` + `@media (max-width: 900px)` collapse)
12. Pager (`.pager`, `.pager-info`, `.pager-actions`, `.pager-page-size`, `.pager-page-size-label`, `.pager-page-size-select`)
13. Password toggle (`.password-wrapper`, `.password-toggle-btn`)
14. State panels / skeletons (`.state-panel-title`, `.state-panel-copy`, `.skeleton` + `@keyframes skeleton-pulse`)
15. Utilities (`.shadow`, `.text-center`, `.mb-3`, `.visually-hidden`)
16. Accessibility (`fieldset`, `fieldset legend`)

Each section header in the file must be `/* ── Name ──────────── */` so future edits don't shuffle. **Do not deviate from the class names.** They match the ClinicAssist reference on purpose — the palette values (and only the palette values) may change later without touching a single component.

Delete `src/Wombat.Web/wwwroot/app.css`'s current 37-line Blazor default and replace it wholesale.

### 5. Shared component library

All under `src/Wombat.Web/Components/Shared/`. Each one is small. Each one is covered by at least one rendering test in `Wombat.Web.Tests` (see below).

- **`Icon.razor`** — see step 1.
- **`PageHeader.razor`** — props `Title`, `Subtitle?`, `RenderFragment Actions?`. Emits `<div class="header-container"> <div> <h1>@Title</h1> <p class="page-subtitle">@Subtitle</p> </div> <div class="actions-cell">@Actions</div> </div>`.
- **`Breadcrumbs.razor`** — a small crumb trail rendered above `PageHeader` when present. Takes a list of `(label, href?)` tuples. `href == null` ⇒ current page, no anchor. Uses a plain `<nav aria-label="Breadcrumb">` with `<ol class="breadcrumbs">` (add a short `.breadcrumbs` block to `app.css` under utilities).
- **`DataTable.razor<TItem>`** — generic list shell. Parameters: `IReadOnlyList<TItem> Items`, `RenderFragment HeaderRow`, `RenderFragment<TItem> Row`, `RenderFragment? Empty`, `string? Caption`. Renders the `.table-container > .clinic-table` shape. Client-side sort and filter are **not** in scope for this task — that is v2. What we need first is a reusable shell so every list page stops hand-rolling its own.
- **`FormField.razor`** — props `Label`, `For?` (defaults to an auto `id`), `HelpText?`, `Required`, `FullWidth`, `RenderFragment ChildContent`. Emits `<div class="form-group @(FullWidth ? "full-width" : "")"> <label for="@For">@Label @if (Required) {<span aria-hidden="true">*</span><span class="visually-hidden">required</span>}</label> @ChildContent @if (HelpText is not null) {<small class="page-subtitle">@HelpText</small>} <ValidationMessage For="@_expression" /> </div>`. Make `For` resolve through a `CascadingValue` so the enclosing `<EditForm>` can wire it without repeat.
- **`ConfirmDialog.razor`** — props `Title`, `Body`, `ConfirmLabel`, `DangerAction`, `EventCallback OnConfirm`, `EventCallback OnCancel`. Renders a `<dialog>` element (native, polyfilled with `dialog.showModal()` via a tiny JS interop under `wwwroot/js/dialog.js`). Footer: `<button class="btn btn-outline">Cancel</button>` + `<button class="btn @(DangerAction ? "btn-danger" : "btn-primary")">@ConfirmLabel</button>`. Destructive actions (T011, T019 publish-new-version) open a ConfirmDialog before firing the command — do not tee this up inline.
- **`PagerControls.razor`** — props `Page`, `PageSize`, `TotalCount`, `EventCallback<int> OnPageChanged`, `EventCallback<int> OnPageSizeChanged`. Renders the `.pager` shape from `../DESIGN.md § Pager`. Default page sizes `10, 20, 50, 100`.
- **`StatePanel.razor`** — the loading/empty/error triad for list pages. Props `IsLoading`, `LoadError` (`string?`), `IsEmpty`, `EmptyTitle`, `EmptyBody`, `RenderFragment? EmptyActions`, `RenderFragment ChildContent`. Logic: loading → render N `Skeleton`s, error → `.alert .alert-danger`, empty → `.detail-card--empty`, otherwise → `ChildContent`.
- **`Skeleton.razor`** — props `Width`, `Height`, `Count`. Renders `Count` of `<div class="skeleton">` with the given inline size. Default count `1`.
- **`PasswordToggleButton.razor`** — copy from ClinicAssist; button absolutely-positioned inside `.password-wrapper`.
- **`Alert.razor`** — props `Kind` (`danger`/`success`/`warning`/`info`), `RenderFragment ChildContent`, `bool Dismissible`. Emits `.alert .alert-@Kind`. If `Dismissible`, include a right-aligned `×` button bound to an internal `bool _dismissed`.
- **`FormActions.razor`** — simple `<div class="form-actions">@ChildContent</div>` — exists so every form uses the same shape and grep finds them.

### 6. Auth pages

`Components/Pages/Account/Login.razor` — rewrite the existing primitive version to use `.account-form-container`:

```razor
@page "/account/login"
@attribute [AllowAnonymous]

<PageTitle>Sign in — Wombat</PageTitle>

<div class="account-form-container shadow">
  <h2>Sign in</h2>

  @if (!string.IsNullOrWhiteSpace(Error))
  {
    <Alert Kind="danger">@Error</Alert>
  }

  <form method="post" action="/account/login/submit">
    <AntiforgeryToken />
    <div class="mb-3">
      <label for="login-email">Email</label>
      <input id="login-email" class="form-control" type="email" name="Email" required autofocus />
    </div>
    <div class="mb-3 password-wrapper">
      <label for="login-password">Password</label>
      <input id="login-password" class="form-control" type="password" name="Password" required />
      <PasswordToggleButton For="login-password" />
    </div>
    <div class="form-check mb-3">
      <input id="login-remember" class="form-check-input" type="checkbox" name="RememberMe" value="true" />
      <label for="login-remember">Remember me</label>
    </div>
    <input type="hidden" name="ReturnUrl" value="@ReturnUrl" />
    <FormActions>
      <button type="submit" class="btn btn-primary">Sign in</button>
    </FormActions>
  </form>

  <p class="page-subtitle mt-3">Forgotten your password? <a href="/account/forgot-password">Reset it</a>.</p>
</div>
```

Same treatment for:

- `Logout.razor` — posts to `/account/logout`, redirects to `/account/login` (already wired in `Program.cs`; the page is the confirmation).
- `ChangePassword.razor` — `.form-container` + `.form-grid` + two password fields + confirm, validates against Identity.
- `Register.razor` — already created in T005; rewrite its HTML to use the `.account-form-container` + `FormField` + `Alert` pattern. Leave the `Register` form POST handler alone.
- `Profile.razor` — already exists; rewrite its HTML to use `PageHeader` + `.details-grid` (summary card + form card) pattern.

### 7. Error pages

- `Components/Pages/Error.razor` — use `PageHeader Title="Something went wrong"`, `Alert Kind="danger"`, "If this keeps happening, please contact your administrator."
- `Components/Pages/AccessDenied.razor` — new. `PageHeader Title="Access denied"`, body explains "You do not have permission to view this page." CTA: link back to `/`.
- `Components/Pages/NotFound.razor` — rewrite existing. `PageHeader Title="Page not found"`, `.detail-card--empty` body with a home link.
- `Components/Pages/Home.razor` — rewrite existing. Uses the **dashboard page shape** from `../DESIGN.md` as a dashboard placeholder: `PageHeader` with user name + role, `.dashboard-grid` with three `.detail-card` stubs labelled "Coming soon in T011". This is the canonical "auth works end-to-end" smoke page until T011 replaces it with the real dashboards.

### 8. `Program.cs` wiring in `Wombat.Web`

Copy the ClinicAssist pattern verbatim for:

- `AddBlazorServerInteractive` (or `AddInteractiveServerComponents` in .NET 10, whichever name the reference uses).
- `AddAuthentication().AddIdentityCookies()` with the correct cookie settings.
- `AddIdentityCore<WombatUser>().AddRoles<…>().AddEntityFrameworkStores<…>().AddSignInManager().AddDefaultTokenProviders()`.
- `AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())` — **this is the non-negotiable switch that makes every page authenticated by default**. Pages that need to be anonymous declare `[AllowAnonymous]` explicitly.
- `AddAuthenticationStateProvider<ServerAuthenticationStateProvider>()` — copy ClinicAssist's exact registration so the state flows into Blazor components.
- `AddScoped<IScopedSender, ScopedSender>()` — the Wombat/ClinicAssist shim that wraps `IServiceScopeFactory` + `ISender`. Copy verbatim.
- Npgsql `ApplicationDbContext` registration with the connection string from `ConnectionStrings:DefaultConnection`.
- Middleware order: `UseStaticFiles()` → `UseRouting()` → `UseAuthentication()` → `UseAuthorization()` → `MapRazorComponents<App>().AddInteractiveServerRenderMode()`.
- Identity endpoints: `MapPost("/account/login/submit", …)` and `MapPost("/account/logout", …)` implemented as minimal APIs that call `SignInManager` and redirect. Do **not** use the scaffolded Identity UI.

### 9. Tests

New project: `tests/Wombat.Web.Tests/` (xUnit + `bunit` for Blazor component rendering — add the `bunit` package in `Directory.Packages.props`). Tests:

- `DesignSystemSmokeTests` — bUnit renders every shared component (`PageHeader`, `DataTable`, `FormField`, `ConfirmDialog`, `PagerControls`, `StatePanel`, `Alert`, `Skeleton`) and asserts the root element has the expected class from `../DESIGN.md` (e.g. `.header-container`, `.clinic-table`, `.form-group`, `.pager`, `.alert.alert-danger`, `.skeleton`). This is the guardrail that the class vocabulary does not silently drift.
- `PageShapeSmokeTests` — bUnit renders a stub list page, detail page, form page, dashboard page using the patterns in `../DESIGN.md § Page-level patterns`. Asserts each renders its root container class. Purpose: future tasks that deviate from the shape fail the test.
- `NavMenuAuthorizationTests` — renders `NavMenu` under a fake `AuthenticationStateProvider` with each role claim and asserts the correct set of `<NavLink>` children appears. One test per row in the nav item table.

### 10. Delete old primitive pages

The following existing pages were written as the minimum to exercise MediatR flows. Rewrite each to use `PageHeader` + `DataTable` or `.form-container` as appropriate:

- `Components/Pages/Admin/Epas/EpasList.razor`
- `Components/Pages/Admin/Epas/EpaEdit.razor`
- `Components/Pages/Admin/Curricula/CurriculaList.razor`
- `Components/Pages/Admin/Curricula/CurriculumEdit.razor`
- `Components/Pages/Admin/Curricula/CurriculumItemsEdit.razor`
- `Components/Pages/Admin/Forms/FormsList.razor` / `FormEdit.razor`
- `Components/Pages/Admin/Institutions/*`
- `Components/Pages/Admin/Invitations/InvitationsList.razor`
- `Components/Pages/Admin/Trainees/*`
- `Components/Pages/Admin/Assessors/*`
- `Components/Pages/Account/Profile.razor`
- `Components/Pages/Account/Register.razor`

**Do not touch MediatR wiring** — the handlers and commands are already verified; only the Razor markup changes. `git diff --stat` on each file should show only the `.razor` template (and sometimes a tiny `@code` tweak to pass data to `PageHeader`). This is the step that actually proves the design system works — a rewrite is only useful if the existing pages adopt it.

## Files created

- `src/Wombat.Web/wwwroot/app.css` (rewritten — every section in `../DESIGN.md`)
- `src/Wombat.Web/wwwroot/icons/*.svg` (≈20 Lucide icons)
- `src/Wombat.Web/wwwroot/js/dialog.js` (tiny interop for `<dialog>.showModal`)
- `src/Wombat.Web/Components/Layout/MainLayout.razor` (rewritten)
- `src/Wombat.Web/Components/Layout/MainLayout.razor.css` (ported from ClinicAssist)
- `src/Wombat.Web/Components/Layout/NavMenu.razor` (new)
- `src/Wombat.Web/Components/Layout/NavMenu.razor.css` (ported from ClinicAssist)
- `src/Wombat.Web/Components/Shared/Icon.razor`
- `src/Wombat.Web/Components/Shared/PageHeader.razor`
- `src/Wombat.Web/Components/Shared/Breadcrumbs.razor`
- `src/Wombat.Web/Components/Shared/DataTable.razor`
- `src/Wombat.Web/Components/Shared/FormField.razor`
- `src/Wombat.Web/Components/Shared/FormActions.razor`
- `src/Wombat.Web/Components/Shared/ConfirmDialog.razor`
- `src/Wombat.Web/Components/Shared/PagerControls.razor`
- `src/Wombat.Web/Components/Shared/StatePanel.razor`
- `src/Wombat.Web/Components/Shared/Skeleton.razor`
- `src/Wombat.Web/Components/Shared/Alert.razor`
- `src/Wombat.Web/Components/Shared/PasswordToggleButton.razor`
- `src/Wombat.Web/Components/Pages/Account/Login.razor` (rewritten)
- `src/Wombat.Web/Components/Pages/Account/Logout.razor`
- `src/Wombat.Web/Components/Pages/Account/ChangePassword.razor`
- `src/Wombat.Web/Components/Pages/Account/Profile.razor` (rewritten)
- `src/Wombat.Web/Components/Pages/Account/Register.razor` (rewritten)
- `src/Wombat.Web/Components/Pages/{Error,AccessDenied,NotFound,Home}.razor`
- All Admin pages under `src/Wombat.Web/Components/Pages/Admin/**` rewritten to use the new component library (markup only)
- `tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`
- `tests/Wombat.Web.Tests/Design/DesignSystemSmokeTests.cs`
- `tests/Wombat.Web.Tests/Design/PageShapeSmokeTests.cs`
- `tests/Wombat.Web.Tests/Navigation/NavMenuAuthorizationTests.cs`

## Verification

- [ ] `dotnet build Wombat.sln -c Release` clean with zero warnings.
- [ ] `dotnet test tests/Wombat.Web.Tests -c Release` green.
- [ ] `grep -R 'class="table"' src/Wombat.Web/Components` returns nothing (no Bootstrap `.table` survivors).
- [ ] `grep -RE '#[0-9a-fA-F]{6}' src/Wombat.Web/Components src/Wombat.Web/wwwroot/app.css | grep -v ':root' | grep -v 'data:image/svg'` returns nothing (no raw hex outside `:root` and icon data URIs).
- [ ] `grep -R 'class="btn-outline-' src/Wombat.Web/Components` returns nothing (no Bootstrap outline variants).
- [ ] `grep -R '<i class="bi bi-' src/Wombat.Web/Components` returns nothing (no Bootstrap Icons font usage).
- [ ] Manual walkthrough:
  1. Unauthenticated visit to `/` redirects to `/account/login`. Login page renders inside `.account-form-container`, not inside the main layout.
  2. Log in as a seeded admin. Land on `/` (Home). The sidebar is visible, the top-row shows the user name + "Sign out" button, the body has a `PageHeader` with role + three stub dashboard cards in `.dashboard-grid`.
  3. Click every nav link. Each either renders a real page (T005 Invitations, T003 Institutions, etc. — rewritten per step 10) or a placeholder page that still sits inside `MainLayout` and reads "Coming soon in T011/T019".
  4. Every rewritten list page (Epas, Curricula, Invitations, Institutions, Specialities, SubSpecialities, Trainees, Assessors) now shows: `PageHeader` at top, optional `.search-container`, either `.clinic-table` inside `.table-container` (with data) or `.detail-card--empty` (without). No primitive `<table class="table">` survives. Visual diff against the ClinicAssist reference shows the same shape.
  5. Every rewritten edit page uses `.form-container` + `.form-grid` + `FormField` + `FormActions`. Validation errors render as `.validation-message` under the field and `.validation-summary-errors` at the top.
  6. Attempt to navigate to an admin-only page as a Trainee — redirected to `/AccessDenied`, which renders inside `MainLayout` using `PageHeader` + `.alert.alert-warning`.
  7. Sign-out clears the cookie and redirects to `/account/login`. The top-row flips to "Sign in".
  8. Resize the window to `<= 640px`. Sidebar collapses behind the `.navbar-toggler` checkbox. The nav opens and closes.
  9. Focus an input. Hit `Tab` through the page. Focus rings are visible on buttons, links, and form controls using `--focus-ring`.
  10. Submit a malformed form. Both the field-level `.validation-message` and the form-level `.validation-summary-errors` appear. Re-submitting with valid data clears them.
- [ ] bUnit tests assert the class vocabulary for every shared component (step 9).
- [ ] `bin/Debug/net10.0/Wombat.Web.styles.css` does not duplicate any `.btn`/`.detail-card`/`.form-control` rules — only layout and NavMenu are scoped.
- [ ] `../DESIGN.md` reads as accurate: if any class name here differs from it, update `DESIGN.md` in the same commit.

## Notes & gotchas

- **`IScopedSender` not `ISender`.** `ISender` captures the top-level scope and causes DbContext lifecycle bugs on subsequent calls within the same Blazor circuit. Copy ClinicAssist's shim verbatim. This is the single most common mistake in ClinicAssist's history; do not repeat it.
- **`AuthenticationStateProvider` wiring is finicky.** Copy ClinicAssist's `Program.cs` block for `AddAuthenticationStateProvider` and friends exactly. Getting this wrong means `<AuthorizeView>` silently renders `<NotAuthorized>` for everyone.
- **Do not use the scaffolded Identity UI.** It does not play nicely with Blazor Interactive Server and it brings in a huge surface. Hand-roll the six pages under `Components/Pages/Account/`.
- **`FallbackPolicy = RequireAuthenticatedUser`** makes every page authenticated by default. To mark a page anonymous, add `@attribute [AllowAnonymous]` on the `@page` line. Login, Register (via invitation token), NotFound, Error, AccessDenied are the only anonymous pages.
- **`flush_interval -1` in Caddy** is required for Blazor Server (signal-R tunnelled via the reverse proxy). Add a note to T015 when it gets picked up.
- **Palette is still TBD.** Ship with the ClinicAssist placeholder values in `:root` so the app renders correctly. A palette-swap PR can happen any time before launch by editing `:root` only. Architecture tests (T013) should spot-check that no component file hard-codes a hex value.
- **Do not introduce new global classes ad-hoc.** If a later task needs a new class, add it to `../DESIGN.md` first in the same PR.
- **Do not fork `.clinic-table` styles per feature.** If a list needs something weird, add a modifier class like `.clinic-table--dense` to `app.css` and document it in `DESIGN.md`.
- **`<dialog>` accessibility**: the native element handles focus trap and escape-to-close for free. Do not re-implement those. The interop module is ten lines max.
- **`bunit` + Interactive Server**: `bunit` tests render components statically, not through the SignalR circuit, so they do not catch lifetime bugs. They catch markup drift — which is what they are for here. Circuit behaviour is covered by integration tests in later tasks.

## Why this task is now bigger than it was

The earlier version of this task said "copy ClinicAssist's pattern" and listed the components by name. That turned out to be insufficient — the agent that executed a halfway pass shipped an 8-line `MainLayout.razor`, a 37-line `app.css` full of Blazor defaults, and primitive `<h1>` + `<table class="table">` list pages. Every subsequent UI task was going to have to re-derive the design system on the fly, and the rewrite was slipping back toward the same inconsistent HTML the old Wombat was trying to get away from.

This version of T010 names every shared component, names every `app.css` section, names every class, names every rewrite target, and hangs the entire contract off `../DESIGN.md`. A session that follows it end-to-end should land a build that looks and behaves like ClinicAssist (only recoloured) at the end of its run, not a build that still needs a "visual polish" pass.
