# FilterBar

A list's filters, applied with Show: a real form above the list, its labelled fields in a `.search-grid`, then Show and, once a filter is set, Clear filters; nothing changes until Show, and the address carries what was asked. A pattern, not a component: each page writes the form. Flow 06 (T358, 2026-10-05, 85d5a508; round 3 items 25, 31, 33; D3, D9; R2-Trainees, R2-Waiting) made it, on Programme trainees and Waiting for assessors (DESIGN.md § Page-level patterns, "List page", "A list's filters are applied with Show").

## Markup

```html
<form class="search-container" method="get" aria-label="Filter Waiting for assessors">
  <div class="search-grid">
    <div class="search-field">
      <label for="f-show">Waiting</label>
      <select id="f-show" name="show" class="form-select">…</select>
    </div>
    <div class="search-field">
      <div class="form-check"><input id="f-filed" class="form-check-input" type="checkbox"><label for="f-filed">Nothing filed in 30 days</label></div>
    </div>
    <div class="search-field filter-actions">
      <button type="submit" class="btn btn-primary">Show</button>
      <a class="btn btn-outline" href="/programme/waiting">Clear filters</a>   (once any filter is set)
    </div>
  </div>
</form>
```

- **The form** is a `<form class="search-container">` named for its list ("Filter Programme trainees", "Filter Waiting for assessors"), so Enter is Show. Its fields are `.search-field`s in a `.search-grid` (220px tracks, `minmax(min(220px, 100%), 1fr)`), each a labelled control; a checkbox carries its label once, beside it.
- **`.filter-actions`** is the last `.search-field`: Show (`btn-primary`) and Clear filters (`btn-outline`), side by side on the fields' bottom line (a wrapping row, `align-self: end`, `space-sm` apart). Below 641px each button is 44px and the row's width, one under the other.
- **No field acts on change.** No live filtering, and nothing on the page moves while a field is set.

## Show, Clear filters and the address (D3)

- **Show** navigates to the page's own address with the filters as its query: `?short=2&year=4&filed=true` (Programme trainees), `?show=overdue&with=<id>` (Waiting for assessors). The page reads them as query parameters and reads the list again whenever they change, so the address, Back and a shared link all show what was asked. An address that is already the one shown reads again in place.
- **Clear filters** is a link to the bare route.
- **The page number is not in the address**: Show starts from the first page.

## The answer (D9)

- **The list's heading counts the answer** in the list's own words, and takes the focus after Show and after a page turn (`h2.list-section-title`, `tabindex="-1"`): with no filter, the count ("5 current registrars", "3 waiting, 2 overdue"); with one, its own sentence ("1 of 5 current registrars has filed nothing in 30 days", "1 waiting, 1 overdue, with Mohammed Patel"); with several, "2 of 5 current registrars match these filters", and what was asked in the rule line. Every count has its singular.
- **No match is not empty.** The heading is "0 of 5 current registrars" ("0 of 2 waiting"), over an empty-state card: "No registrar matches these filters." ("No request matches these filters."), what was asked ("Overdue only, with Fatima Khumalo."), and Clear filters. One pattern and one button word for every list (review 33).
- **Nothing to filter, no form.** When the list holds nothing at all, the page draws its empty state and no form. While it loads or has failed, the form stands.

## Rules

- A select that picks a kind of row is named for what it filters ("Waiting": All, Overdue only), never "Show", which is the button's (review 31).
- A filter's options are what the list could answer: Training year lists the years a current registrar is in, With each waiting row's nominee by surname.
- The form is a GET form whose fields carry the query's keys, so Show works before the circuit starts (Waiting for assessors; Programme trainees' does not yet, ProgrammeTrainees' known gaps).

## Contrast

Labels `text-color` 12.63:1; each control's edge `input-border` 3.80:1; Show's words `on-fill` 4.86:1 on `secondary-color`; Clear filters 4.86:1 on the surface.
