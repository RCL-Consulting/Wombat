# T111 — The trainee dashboard's "Request an assessment" link preselects nothing

**Status:** DONE 2026-09-19
**Surfaced:** 2026-09-18, while scoping T109.
**Severity:** Low — cosmetic, but on the trainee's most-used surface.

## Symptom

`TraineeDashboard.razor:121` links to:

```razor
<a href="/activities/new?type=mini_cex" class="btn btn-outline">Request an assessment</a>
```

`NewActivity.razor` declares **no** `[SupplyParameterFromQuery]` property. The `type` parameter is silently
ignored and the picker opens unset, so the trainee lands on a form that looks like it should already know
what they clicked and doesn't.

## What shipped, and the two things the task did not know

`NewActivity.razor` now declares `[SupplyParameterFromQuery(Name = "type")]` and resolves the key against
`_activityTypes` — the list it already loads, which is scope-filtered **and, since [T123] defect 3,
ladder-filtered**. That is what keeps the link from becoming an authorization statement: a key the caller
may not use is simply absent and resolves to nothing. A server-side key lookup would bypass both filters,
which is exactly what the note below forbids.

**1. The `@bind:after` trap.** `:29` wires `LoadSelectedTypeAsync` to the element's *change event*.
Assigning `_selectedActivityTypeId` in code selects the right `<option>` and renders **no form** — a page
that looks like it loaded the type and shows nothing, which is worse than doing nothing at all. The
handler is awaited explicitly.

**2. `mini_cex` was the wrong key, so fixing this page alone would have changed nothing on screen.**
`DataSeeder.cs:265` scopes all ten generic seeds to the demo "General Medicine" speciality, so
`ListActivityTypesQuery`'s speciality filter removes `mini_cex` from a paediatric trainee's list before the
page ever sees it. The dashboard link would have resolved to nothing for every real trainee and the task
would have been marked done with the button behaving exactly as before.

`TraineeDashboard.razor:121` now links to `/activities/new` with no query string. The dashboard has no
basis for choosing a tool — it does not know the trainee's speciality tooling, and hard-coding a demo seed
key on a shared surface was the actual defect. **Nothing the user sees changes**, because the parameter was
being discarded anyway; the URL has simply stopped claiming something it could not do. The capability is
now real and tested, for a deep link that *does* know the tool.

**Noted, not fixed:** "Log an activity" and "Request an assessment" now visibly go to the same place. They
already did. Wombat has no request-an-assessment flow — a trainee creates the activity and names the
assessor — so the second button labels an action the product does not have. That is a product question,
not this task's.

## The sweep is done

The task asked whether any other page links with a query parameter its target does not read. Swept across
`src/Wombat.Web` against all 13 `[SupplyParameterFromQuery]` declarations: **`TraineeDashboard.razor:121`
was the only one.** `VerifyExport.razor:48,55-60` is the in-repo template for the pattern and reads its
parameter correctly.

## Notes from when it was filed

- Confirmed by grep: `SupplyParameterFromQuery` appears nowhere in `NewActivity.razor`.
- `mini_cex` is an activity type **key**, not an id, while the picker binds `_selectedActivityTypeId` (an
  `int`) — so the fix has to resolve key → id against the list the page already loads from
  `ListActivityTypesQuery`, not assume the query string carries an id.
- A key that does not resolve (not published, not in the caller's scope, or renamed) must leave the picker
  unset rather than throw. The link is a convenience; it is not an authorization statement, and it must not
  become one.
- Worth checking whether any other page links with a query parameter the target does not read — this was
  found by accident, not by a sweep.

## Related

Found while scoping `T109`. Unrelated to the credit path; filed separately so it does not ride along on a
change to the credit engine.
