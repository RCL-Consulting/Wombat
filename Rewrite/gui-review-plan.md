# GUI review plan — design-system audit

Polish pass over the Blazor UI now that the practical plan is closed (T035 shipped; T036 deferred — see `practical-plan.md`). Not new features — consistency, shared-component usage, and `Rewrite/DESIGN.md` compliance across ~65 pages and 15 shared components.

**Audience:** same hospital programme. The code works; this round makes it look and feel coherent and removes drift that would distract a user or embarrass a demo.

Companion docs:
- `Rewrite/DESIGN.md` — canonical design-system contract (rubric defers to it on conflicts)
- `src/Wombat.Web/wwwroot/app.css` — 885-line token/component anchor
- `Rewrite/current_state.md` — live session handoff

## Non-goals

- New pages, new routes, new flows
- Feature additions dressed as polish
- Changes to `DESIGN.md` or to tokens in `app.css` — those get their own task
- Restyling the whole app in one pass; clusters let us ship incrementally

## What "done" means

- No `class="bi bi-*"` anywhere in the codebase
- No Bootstrap class leakage (`btn btn-outline-*`, `class="table"`, `col-md-*`)
- Every page in every cluster loaded in a browser at least once this pass
- Shared-component drift (ad-hoc table vs `DataTable`, ad-hoc header vs `PageHeader`, etc.) either fixed or logged as a follow-up task
- Every cluster has a commit

## Review rubric

Each cluster checks every page against:

1. **Design-system compliance** — no `class="bi bi-*"`, no `btn btn-outline-*`, no `class="table"`, no `col-md-*`, no `<style>` blocks in page-level `.razor` files, no raw hex outside `:root`.
2. **Shared components** — `PageHeader` on lists/detail/forms; `DataTable` for tabular data; `FormField` for inputs; `StatePanel` (or `Skeleton` + `Alert`) for loading/empty/error; `Icon.razor` (not `<span class="bi...">`) for glyphs.
3. **State coverage** — loading, empty, and error states visible where async data loads.
4. **Accessibility** — labels bound to inputs, validation messages present, focus ring visible, keyboard navigation works for primary actions.
5. **Visual hierarchy** — consistent spacing tokens, page header + subtitle, action placement.
6. **Responsive** — `dashboard-grid` and `details-grid` behave at narrow widths.

Each cluster is **verified in a browser**, not just read. `Rewrite/DESIGN.md` is the canonical reference when the rubric and a page disagree.

---

## T037 — Consolidate NavMenu icons to `Icon.razor`

The icons render today, but via a bespoke mechanism: `NavMenu.razor.css` defines a `.bi-*-nav-menu` class per icon with an inline-SVG `background-image` data URI. Two parallel icon systems now exist in the app — `Icon.razor` everywhere else, this ad-hoc CSS here. Problems:

- The `bi-` prefix implies Bootstrap Icons, which this project does not use; false positive on the CLAUDE.md guidance and a maintenance hazard
- Strokes are hardcoded to `white` in the data URIs, so they can't follow `currentColor` on hover/active states
- Adding a new nav entry requires writing a second icon in a second place

Work:

- Replace every `<span class="bi bi-*-nav-menu">` with `<Icon Name="..." />`
- Add missing Lucide SVGs to `src/Wombat.Web/wwwroot/icons/` (shield, clock, key)
- Delete the `.bi` and `.bi-*-nav-menu` blocks from `NavMenu.razor.css`
- Add a minimal rule to space the `Icon` inside `.nav-link` so layout matches the current design
- Verify each authenticated role's nav renders in the browser; confirm hover/active states still visually distinguish the icon

**Effort:** ½ day. Runs first to make browser verification on every subsequent cluster meaningful.

## T038 — Trainee surface

Most user-facing cluster. Validates the `dashboard-grid` and card system end-to-end.

Pages:
- `Components/Pages/TraineeDashboard.razor`
- `Components/Pages/Portfolio/MyProgress.razor`
- `Components/Pages/Portfolio/MyAuthorisations.razor`
- `Components/Pages/Activities/MyActivities.razor`
- `Components/Pages/Portfolio/ExportPortfolio.razor`
- `Components/Pages/Portfolio/VerifyExport.razor`

**Effort:** 1.5 days.

## T039 — Committee flow

Densest recent work (T031–T033 all landed here).

Pages:
- `Components/Pages/Committee/PanelsList.razor`, `PanelEdit.razor`
- `Components/Pages/Committee/MyReviews.razor`, `ReviewsSchedule.razor`
- `Components/Pages/Committee/ReviewDetail.razor`
- `Components/Pages/CommitteeMemberDashboard.razor`

**Effort:** 1.5 days.

## T040 — Admin hierarchy

High volume, low-visibility polish. Run it as one sweep — pages are structurally similar (list + edit pairs).

Pages:
- Institutions (list, edit)
- Specialities, SubSpecialities (4 pages)
- Assessors (list, profile edit)
- Curricula (list, edit, items edit)
- EPAs (list, edit)
- Role-specific admin dashboards: `InstitutionalAdminDashboard`, `SpecialityAdminDashboard`, `SubSpecialityAdminDashboard`, `AdministratorDashboard`

**Effort:** 2 days.

## T041 — Activity platform

The schema-driven platform's admin-facing surface plus the trainee/assessor runtime.

Pages:
- `Components/Pages/Admin/ActivityTypes/ActivityTypesList.razor`, `ActivityTypeEdit.razor`
- `Components/Pages/Admin/Forms/FormsList.razor`, `FormEdit.razor`
- `Components/Pages/Activities/NewActivity.razor`, `ActivityView.razor`, `ActivityInbox.razor`
- `Components/Pages/AssessorDashboard.razor`, `CoordinatorDashboard.razor`

**Effort:** 2 days.

## T042 — Account & auth shell

Low-churn, small surface. Wrap-up cluster.

Pages:
- `Components/Pages/Account/Login.razor`, `Register.razor`, `ForgotPassword.razor`, `ChangePassword.razor`, `Profile.razor`
- `Components/Pages/AccessDenied.razor`, `Error.razor`, `NotFound.razor`
- `Components/Pages/Home.razor`, `PlaceholderPage.razor`
- Layout: `Components/Layout/MainLayout.razor`, `AuthLayout.razor`, `ReconnectModal.razor` (focus on the reconnect affordance)

**Effort:** 1 day.

---

## Sequencing

T037 first — cheap, and every other cluster's browser verification is more meaningful once nav icons render. After that, clusters are independent; reorder if a specific surface has pending user feedback. Default suggested order below; Trainee surface (T038) is the highest-visibility second step unless something else is pressing.

| Step | Task | Why this order |
|---|---|---|
| 1 | T037 — NavMenu hotfix | Known drift; unblocks visible verification elsewhere |
| 2 | T038 — Trainee surface | Most user-facing; exercises cards + dashboard-grid + trajectory chart |
| 3 | T039 — Committee flow | Densest recent work; where design drift is most likely |
| 4 | T040 — Admin hierarchy | Structurally similar pages; one sweep |
| 5 | T041 — Activity platform | Builder-facing; lower daily traffic |
| 6 | T042 — Account & auth shell | Low-churn wrap-up |

Total estimate: **~8 working days** for a single developer, including browser verification and commit overhead.

## Progress

| Task | Status | Commit |
|---|---|---|
| T037 — Consolidate NavMenu icons to `Icon.razor` | ✅ done (browser-verified Administrator role) | `1d25995` |
| T038 — Trainee surface | ✅ done (all 6 browser-verified, incl. seeded Trainee) | `88f5cf4` |
| T039 — Committee flow | active (CommitteeMember dev login seeded) | — |
| T040 — Admin hierarchy | pending | — |
| T041 — Activity platform | pending | — |
| T042 — Account & auth shell | pending | — |

## Active task

**T039 — Committee flow.** Model: Sonnet. Live state in `Rewrite/current_state.md`.
