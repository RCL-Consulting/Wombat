# Prompt for executing T010 — Web layout, navigation, auth, and the design system

Copy the block below and use it as the opening prompt for an agent session.

---

You are working on the Wombat rewrite — a Blazor Interactive Server app backed by PostgreSQL. Your job is to execute task T010, which delivers the shared design system, layout shell, navigation, auth pages, shared component library, and rewrites every existing primitive admin page to use the new components.

## Bootstrapping — read these files first, in this order

1. `Rewrite/current_state.md` — project status and last verified commit
2. `Rewrite/DESIGN.md` — **the canonical visual contract** (tokens, layout grid, buttons, tables, forms, cards, dashboard grid, alerts, skeletons, pager, icons, page patterns, `app.css` section order, non-negotiables). This is the single source of truth for every class name, every CSS custom property, and every component shape. Do not deviate from it.
3. `Rewrite/Tasks/T010-web-layout-auth.md` — the task file. It has ten numbered steps, a file list, verification checks (including five `grep` rules that must return zero matches), and a manual walkthrough. Follow it end-to-end.
4. `Rewrite/ARCHITECTURE.md` — layer rules, CQRS conventions, `IScopedSender` requirement
5. `Rewrite/CUSTOMIZATION.md` — the Activity platform context (you won't build it in T010, but you need to understand the nav items and page structure it implies)

## Reference folders (read-only, not committed)

- `ClinicAssist.NET_ref_DO_NOT_COMMIT/` — **the design gold standard**. Key files to study:
  - `src/ClinicAssist.Web/wwwroot/app.css` (~570 lines) — port the structure and class names, replace the palette with DESIGN.md's `:root` tokens
  - `src/ClinicAssist.Web/Components/Layout/MainLayout.razor` and `MainLayout.razor.css`
  - `src/ClinicAssist.Web/Components/Layout/NavMenu.razor` and `NavMenu.razor.css`
  - Any page-level `.razor` file to see the rendering quality target
  - `CLAUDE.md` — footguns list (Bootstrap Icons, `IScopedSender`, `btn-outline` vs `btn-outline-primary`)
- `Wombat_ref_old_DO_NOT_COMMIT/` — the old Wombat. The GUI is rough, but it shipped. Consult its `Views/` folder for page structure on Wombat-specific workflows (EPAs, curricula, committee views, trainee profiles) that ClinicAssist doesn't have. Apply the ClinicAssist design system to the old Wombat page structure.

## Execution order

Follow T010's ten steps in order:

1. **Icons** — copy ~20 Lucide SVGs into `wwwroot/icons/`, build `Icon.razor`
2. **MainLayout** — rewrite `MainLayout.razor` + create `MainLayout.razor.css` (port from ClinicAssist)
3. **NavMenu** — create `NavMenu.razor` + `NavMenu.razor.css` with role-gated `<AuthorizeView>` blocks per DESIGN.md's nav item table
4. **`app.css`** — delete the 37-line Blazor default, replace with the full 16-section design system per DESIGN.md § app.css section order, porting values from ClinicAssist's app.css
5. **Shared components** — build all 12 components under `Components/Shared/` (Icon, PageHeader, Breadcrumbs, DataTable, FormField, FormActions, ConfirmDialog, PagerControls, StatePanel, Skeleton, Alert, PasswordToggleButton)
6. **Auth pages** — rewrite Login, Logout, ChangePassword, Register, Profile using the new components
7. **Error pages** — Error, AccessDenied, NotFound, Home (dashboard stub)
8. **`Program.cs` wiring** — Identity, `IScopedSender`, `FallbackPolicy = RequireAuthenticatedUser`, middleware order (copy ClinicAssist's pattern)
9. **Tests** — new `tests/Wombat.Web.Tests/` project with bUnit: `DesignSystemSmokeTests`, `PageShapeSmokeTests`, `NavMenuAuthorizationTests`
10. **Rewrite existing primitive pages** — every admin page listed in T010 step 10 (EPAs, Curricula, Forms, Institutions, Invitations, Trainees, Assessors) gets its markup replaced to use PageHeader + DataTable/FormField/FormActions. **Do not touch MediatR handlers or commands** — only the `.razor` template changes.

## Critical rules

- **No Bootstrap classes.** No `class="table"`, no `btn-outline-primary`, no `col-md-*`. Wombat has its own token-driven `app.css`.
- **No raw hex colours outside `:root`.** All colours route through CSS custom properties.
- **No `<i class="bi bi-*">`.** Bootstrap Icons font is not loaded. Use `Icon.razor`.
- **No inline `<style>` blocks** in page-level `.razor` files. Styles go in `app.css` or `.razor.css` isolation files for layout components only.
- **No MudBlazor, no Radzen, no jQuery.**
- **Use `IScopedSender`** (not `ISender`) in all Blazor interactive components.
- **`DateOnly`** for calendar dates, **`DateTime`** only for timestamps.
- **MediatR v12 max** — do not upgrade.

## Verification (must all pass before marking T010 done)

1. `dotnet build Wombat.sln -c Release` — zero warnings
2. `dotnet test tests/Wombat.Web.Tests -c Release` — green
3. `dotnet test tests/Wombat.Architecture.Tests -c Release` — green (layer boundaries intact)
4. Five grep checks from T010 must return zero matches:
   - `grep -R 'class="table"' src/Wombat.Web/Components`
   - `grep -RE '#[0-9a-fA-F]{6}' src/Wombat.Web/Components src/Wombat.Web/wwwroot/app.css | grep -v ':root' | grep -v 'data:image/svg'`
   - `grep -R 'class="btn-outline-' src/Wombat.Web/Components`
   - `grep -R '<i class="bi bi-' src/Wombat.Web/Components`
5. The manual walkthrough in T010's Verification section (10 steps) should be plausible from the code — you won't run a browser, but the markup structure should clearly support each step.

## When you're done

- Update `Rewrite/current_state.md`: mark T010 complete, note the commit hash, record any follow-up items discovered
- Check the T010 box in `Rewrite/PLAN.md`
- Commit with message `T010: web layout, navigation, auth, and the design system`

The goal is that after this task, the app looks and behaves like ClinicAssist (only recoloured) and every future UI task (T011 dashboards, T019 activity builder) has a solid foundation to build on instead of re-deriving styles ad hoc.
