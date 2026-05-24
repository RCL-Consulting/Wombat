# T055 — Publish button always visible + redirect after first save (ActivityType edit)

Two of the three items originally bundled in this task from `scenario-act1-fixes-plan.md` were real; the third turned out to be a false alarm.

## Items shipped

### 1. Publish button always visible (with disabled state + tooltip)

Before: the `Publish` button only rendered when `_editor.HasDraft == true`, alongside `Discard draft`. The button literally disappeared after a clean publish — confusing because Step 1.11.e of the scenario implied Publish was always available.

After (`ActivityTypeEdit.razor:13-18`):
- `Publish` renders unconditionally outside the `HasDraft` block.
- `disabled="@(_busy || !CanPublish)"` keeps it inert when there's no draft to publish.
- `title="@(CanPublish ? null : "Save a draft to publish.")"` explains *why* it's disabled without taking up screen space.
- `Discard draft` still gates on `HasDraft` — only renders when there's something to discard.
- New computed property `CanPublish => (_editor?.Id ?? 0) > 0 && _editor?.HasDraft == true` matches the previous show/hide condition exactly, so behaviour for the enable state is unchanged.

### 2. Redirect to `/admin/activity-types/{id}` after first save

Before: the first `Save draft` on a brand-new activity type kept the URL at `/admin/activity-types/new`. The page cached the new entity's id internally so `Publish` continued to work, but a browser refresh sent the user back to the empty form (losing their just-saved type).

After (`ActivityTypeEdit.razor:316,343-346`):
- Captures `wasNew = (_editor?.Id ?? 0) == 0` *before* sending `SaveActivityTypeDraftCommand`.
- After the command returns with the new `_editor.Id`, if `wasNew && _editor.Id > 0`, calls `Navigation.NavigateTo($"/admin/activity-types/{_editor.Id}", forceLoad: false)`.
- `forceLoad: false` keeps the navigation SPA-style so the user doesn't see a flash, and the `OnParametersSetAsync` cycle re-binds against the freshly-stamped id naturally.
- Added `@inject NavigationManager Navigation` at the top.

### 3. Dropped: "Create X" page title on 6 admin edit pages (false alarm)

The 2026-05-24 play-through reported the post-save page title still reading "Create X" on Institution, Speciality, Sub-speciality, EPA, Curriculum, and Activity Type edit pages. On closer inspection, 5 of those 6 pages already have the correct conditional `<PageTitle>@(IsNew ? "Create X" : "Edit X")</PageTitle>` and **direct navigation to `/admin/institutions/2` correctly shows "Edit Institution"** in the browser tab.

The play-through's stale-title screenshots were a Playwright snapshot-timing race: snapshots captured the page state in the brief window after the click triggered SPA navigation but before the Blazor re-render updated the title. Not a real bug. Activity-type edit uses a different `<PageTitle>` pattern (`@(_editor?.Name ?? "Activity Type")`) which is also correct.

No code change for item 3.

## Browser verification

`http://localhost:5080/admin/activity-types/new` as Administrator:

| Step | Button state |
|---|---|
| Fresh `/new` form | Save draft (enabled) · Publish (disabled, tooltip "Save a draft to publish.") |
| After filling Metadata + clicking Save draft | URL flips to `/admin/activity-types/21`; Save draft, Discard draft, Publish all enabled |
| After clicking Publish | Status banner "Published version 1."; Save draft enabled; Discard draft hidden; Publish disabled with tooltip again |

## Out of scope

- Same redirect pattern could be applied to other admin "create" surfaces (Forms, Trainee, Assessor) — they each have their own conventions; not changed.
- ActivityType's `<PageTitle>` could be brought into the `IsNew ? ...` pattern for consistency, but the current `_editor?.Name` is arguably nicer (shows "Mini-CEX" once loaded). No change.

## Definition of done

- Build clean.
- Web tests pass.
- Browser-verified end-to-end.

## Files touched

- `src/Wombat.Web/Components/Pages/Admin/ActivityTypes/ActivityTypeEdit.razor`
- `Rewrite/Tasks/T055-publish-button-and-post-save-redirect.md` (this file)
- `Rewrite/current_state.md`
