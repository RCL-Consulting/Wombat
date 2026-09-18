# T111 — The trainee dashboard's "Request an assessment" link preselects nothing

**Status:** open
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

## Notes for whoever picks it up

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
