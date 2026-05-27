# T064 — AssessorProfileEdit: post-save URL flip + assessor-user dropdown narrowing

`src/Wombat.Web/Components/Pages/Admin/Assessors/AssessorProfileEdit.razor` has two UX warts surfaced in the 2026-05-27 Act 2 play-through (Finding **A2-6**):

- **A2-6.a** — After Save, the URL stays at `/admin/assessors/edit` rather than flipping to `/admin/assessors/edit?id={profileId}`. A page refresh therefore loses the just-saved entity. Same shape as the T055 fix on `ActivityTypeEdit.razor` (which flipped from `/new` to `/{id}`).
- **A2-6.b** — The Assessor user `<select>` does NOT narrow to remaining unprofiled users after each save. The operator can pick an already-profiled user and the handler will (presumably) reject — but the form should prevent the mistake at picker time.

## Change

### Post-save URL flip (T055 pattern)

Locate `SaveAsync` in `AssessorProfileEdit.razor`. After the successful `CreateAssessorProfileCommand` / `UpdateAssessorProfileCommand` call, `NavigateTo($"/admin/assessors/edit?id={result.Id}", forceLoad: true)` for the create path. The `forceLoad: true` matches T057's fix — `<PageTitle>` does not re-evaluate on same-component SPA navigation, so a full reload guarantees both `<h1>` and document.title flip correctly.

Use `forceLoad: false` only if the page already re-derives state from the new `?id=` query. Mirror exactly what T055 did on `ActivityTypeEdit.razor` for consistency.

### Dropdown narrowing

In `OnInitializedAsync` (or wherever the assessor-user list is loaded), filter out users who already have an `AssessorProfile`. Two reasonable approaches:

1. **Filter in the existing query.** Extend `ListAssessorUsersQuery` with an `ExcludeUserIds` parameter populated from a quick `GetExistingAssessorUserIdsQuery`. Pre-load the existing user-ids first, then exclude.
2. **Filter client-side.** Load all assessor-role users, then load existing profiles, then exclude in the Razor `@code` block.

Pick (1) for cleanliness — the filtering belongs in the query, not the view. Add a `GetAssessorUserIdsAlreadyProfiledQuery` returning `IReadOnlyList<string>`.

When the form loads in edit mode (`?id=...`), the current profile's user must remain selectable (otherwise the picker's value won't match any option). Pass `IncludeUserIds: [currentProfile.UserId]` or similar.

## Tests

- New bUnit test in `tests/Wombat.Web.Tests/Admin/Assessors/` (create folder if absent): renders `AssessorProfileEdit.razor` with a seeded pair of users where one is already profiled. Assert the profiled user is NOT in the picker's options, the unprofiled one IS, and the picker has the placeholder `"Select assessor"` option.
- Edit-mode test: render with `?id={existingProfileId}`, assert the existing user IS selectable + pre-selected.
- Existing query tests for `ListAssessorUsersQuery` — add coverage for the new exclude parameter.

## Browser verification

- Fresh `/admin/assessors/edit` page: Assessor user dropdown shows 5 KGK Paed assessor-role users (Zulu, Naidoo, Botha, Patel, Khumalo). Save a profile for Zulu. Verify the URL flips to `/admin/assessors/edit?id={n}`.
- Refresh the page at the new URL: form pre-fills with Zulu's data.
- Visit `/admin/assessors/edit` again (no id): assessor dropdown should now NOT show Zulu.

## Out of scope

- Cascading Speciality → Sub-speciality picker (today's UI disables Sub-speciality unless a Speciality is chosen; that's fine).
- Bulk-create assessors. Defer.
- The training-status enum vs date question — that's T065.

## Definition of done

- Build clean, all suites pass.
- Two new bUnit tests pass.
- Browser-verified post-save URL flip + dropdown narrowing.
- Scenario doc Act 2 Step 2.4 Actual/Gap revised; A2-6.a and A2-6.b marked closed.

## Files touched

- `src/Wombat.Web/Components/Pages/Admin/Assessors/AssessorProfileEdit.razor`
- `src/Wombat.Application/Features/Assessors/ListAssessorUsers.cs` — extend with exclude/include parameters.
- `src/Wombat.Application/Features/Assessors/GetAssessorUserIdsAlreadyProfiled.cs` — new (or fold into the existing query as a second list).
- `tests/Wombat.Web.Tests/Admin/Assessors/*` — new bUnit tests.
- `tests/Wombat.Application.Tests/Features/Assessors/*` — extend ListAssessorUsersQuery tests.
- `Rewrite/Tasks/T064-assessor-profile-edit-polish.md` (this file)
- `Rewrite/scenario-paediatrics.md`
- `Rewrite/current_state.md`

## Estimate

~30 minutes. **Sonnet.**
