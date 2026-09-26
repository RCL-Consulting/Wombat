---
id: T329
title: New activity and New MSF campaign crash the circuit when a read fails; EPAs, committee review, panels and the builder misstate a wait or a failure
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T329 — New activity and New MSF campaign crash the circuit when a read fails; EPAs, committee review, panels and the builder misstate a wait or a failure

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. When a read fails, New activity, the trainee's filing page, ends the circuit. The page dies behind the 'An unhandled error has occurred. Reload' bar and must be reloaded, while every other page shows its alert. Nothing typed is lost, because the read comes before the form. The rest are Low. A skeleton that never ends says 'still loading' about a read that failed. A blank review page says nothing. The routing card says there is no routing when routing exists. The builder calls an existing type new and offers a save that can only be refused.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings S-shell--error-banner, S-new-activity--loading, S-epas-list--load-error, S-review-detail--loading, S-panels-list--loading, S-activity-type-edit--loading, S-activity-type-edit--not-found).

## Symptom

Found by the T295 states sweep, 2026-09-26, holding a read with ACCESS EXCLUSIVE locks on a scratch database (states.md § Holding a read).
- New activity (shell--error-banner.png, Dr Dlamini): 61 s after Activities is pressed, the bar 'An unhandled error has occurred. Reload' appears and the circuit ends. The page is left with an empty 'Activity type: Select…' picker. While the read waits, the page shows the header and an empty picker, with no skeleton (new-activity--loading.png; states.md calls this row 'Loading (no skeleton)').
- New MSF campaign (/msf/campaigns/new): by code read, the page ends its circuit the same way if its template or trainee list fails. Not captured.
- EPAs (epas-list--load-error.png, Dr Kruger): after the Epas query timed out (confirmed in the log), the list stayed a skeleton, even after the locks were released. The alert never shows.
- Committee review (review-detail--loading.png, Dr Zulu): while the review is read, the page shows its header and blank space, with no skeleton and no loading text.
- Decision panels (panels-list--loading.png, Dr Botha): while the list loads, the 'Who decides each EPA' card already says 'There is no committee routing to show for your institution.' Routing for curricula 11.1 and 11.2 appeared once the hold was released.
- Activity-type builder (activity-type-edit--loading.png, activity-type-edit--not-found.png): an existing type is titled 'New activity type' while it loads, and so is an unknown id. Save draft is enabled in both cases.
- Invitations and SSO group mappings (by code read, not captured): after a failed read, the card under the top alert says 'No active invitations. Issue an invitation to create the first onboarding link.' (or 'No group mappings').

## Root cause

- NewActivity.razor:139-152: OnInitializedAsync has no try. FilingLateness.ProgrammeStartAsync catches its own failure (FilingLateness.cs:108-136). Then ListActivityTypesQuery (:146-149) throws into the renderer, and Blazor ends the circuit (MainLayout.razor:30, #blazor-error-ui). LoadSelectedTypeAsync (:178-197) sends GetActivityTypeEditorQuery (:192) with no try either. It runs from the picker's @bind:after (:34) and from the ?type= deep link (:151, :154-176). The page renders no StatePanel, so it has no loading state. A scan of every page's OnInitializedAsync and OnParametersSetAsync found no other page that reads unguarded, except the next item.
- CampaignEdit.razor:515-523: on /msf/campaigns/new, OnParametersSetAsync awaits LoadTemplatesAsync (:720) and LoadTraineesAsync (:739), and neither read is in a try.
- EpasList.razor:17: IsLoading is `_epas is null || _subSpecialities is null || _specialities is null || _institutions is null`, with no error guard. The catch (:75-78) sets only _errorMessage, so a failure in any of the four reads leaves the skeleton forever. Every other list either conjoins `string.IsNullOrWhiteSpace(_errorMessage)` or sets its list in the catch. CurriculaList.razor:91-95 sets `_curricula = []`, so Curricula, which the sweep also named, is not affected.
- ReviewDetail.razor:113 gates the whole body on `_review is not null`. LoadAsync (:1069-1098) keeps no loading flag, and the page has no StatePanel.
- PanelsList.razor:184 starts `_routingLoading` false. It turns true only in LoadRoutingAsync (:246-262), which runs after the form options and the panel list have been read (:186-231). Until then `_routing is null` renders the 'no committee routing' line (:99-104).
- ActivityTypeEdit.razor:434: PageTitleText is 'New activity type' unless `_editor.Id > 0`. An unknown id's read throws 'The activity type could not be found.' (GetActivityTypeEditorQuery.cs:97-101). Save draft (:21) is disabled only by OtherInFlight. If it is pressed while _editor is null, SaveDraftCoreAsync (:610-650) sends a create with an empty key, name and workflow. ActivityType.SaveDraft (ActivityType.cs:96) refuses that, and the page prints ArgumentException's developer text. Nothing is created, because the entity is added only after SaveDraft (SaveActivityTypeDraftCommand.cs:98-125).
- InvitationsList.razor:121 and GroupMappings.razor:116 pass `LoadError="@(null)"` to StatePanel and print the failure in their top alert (:22-25). A failed read therefore also draws the empty state.

## What to build

Hold every page to one StatePanel contract. The header renders from the first render. A skeleton shows while the page's first read runs, and the alert shows if it fails. No action that needs the loaded record is offered until the record has loaded.
- New activity: read the type list inside a try, and render the picker inside a StatePanel. It shows a skeleton while the list is read and the alert on failure, with the picker left out. A failed read of the chosen type (LoadSelectedTypeAsync, including the ?type= preselect) shows an alert and leaves the picker usable.
- New MSF campaign: do the same for the template and trainee reads on /msf/campaigns/new.
- EpasList: add the error guard to IsLoading, as the other lists do, or keep a _loading flag and clear it in finally.
- ReviewDetail: keep a loading flag and put the body inside a StatePanel with a skeleton. Show the first read's failure there. ActionResult keeps the refusals of actions.
- PanelsList: for a caller who is not an Administrator, the routing card shows its skeleton from the first render. Either initialise `_routingLoading` from `!_isAdministrator`, or show the skeleton while `_loading`.
- ActivityTypeEdit: take the title from the route. It is 'New activity type' only on /new. On /{id} it is a neutral 'Activity type' until the editor loads, then 'Edit {name}'. Save draft and Publish are not offered until the editor has loaded, and not after its read has failed. If T300 lands first, gate them on `_editor?.CanWrite == true`, which covers both cases.
- InvitationsList and GroupMappings: pass the load failure to StatePanel's LoadError so the empty state is not drawn under it. Keep the refusals of actions in the top alert.
- The alert's wording is T272's sentence for a database failure. This task only makes sure each page shows the alert.
- states.md: rewrite the 'Unhandled error banner' row (states.md:105). It reaches the bar through this crash, so it needs another way in, or a record that no page crashes on a read any more. Rewrite New activity's 'Loading (no skeleton)' row (:244) too.

## Verification

- [ ] bUnit, with a sender whose read throws. NewActivity renders a danger alert, and no exception escapes the renderer: test both the type list and the chosen type's read after a selection. /msf/campaigns/new does the same when its template or trainee read throws. EpasList renders the alert, not the skeleton. InvitationsList renders the alert and not 'No active invitations'.
- [ ] bUnit, with a read that never completes. NewActivity and ReviewDetail render a skeleton. The routing card on PanelsList, for an InstitutionalAdmin, renders its skeleton and never the 'no committee routing' line. ActivityTypeEdit on /admin/activity-types/5 renders neither 'New activity type' nor Save draft. On an unknown id it renders neither of them after the alert either.
- [ ] Browser, with states.md's lock hold on a scratch database: retake new-activity--loading, epas-list--load-error, review-detail--loading, panels-list--loading, activity-type-edit--loading and activity-type-edit--not-found. New activity, held past 65 s, shows its alert, and the circuit stays alive (no error bar). Update states.md rows 105 and 244.

## Related

T272 (the wording of the alert: raw EF text, and RefusalText's case for database failures), T299 (ActivityView's missing h1 in the same loading and load-error states), T300 (the builder's buttons gated on CanWrite), T321 (the circuit's unhandled-error bar, which it leaves out of scope), T270 (MyMsfReports, where one field holds both the list's load error and a selection's refusal). states.md § Holding a read, and rows 105, 244, 331 and 392.
