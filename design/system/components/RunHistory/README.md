# RunHistory

Run history (`/admin/jobs/runs`, `Pages/Admin/Jobs/ScheduledJobRunsList.razor`; its words `JobWords.cs`; its address `RunHistoryAddress.cs` and `Components/Shared/JobLinks.cs`; the read `GetScheduledJobRunsQuery`, `ScheduledJobRunDto`; the names through `DataRightsPeople.NamesOfAsync`): **every run of the eight jobs, newest first, kept for 90 days, filtered by job, status and South African day, the filter in the page's own address.** Flow 18 (T400, 2026-11-12, `34b5077e`, with `565b978a`: the Job select's options keyed, Started one unit, the key unbroken from 1151px and Ran by's floor from 1200px; boards R3-R01 to R09, R2-R03, R08, R10, R11; T1, T17, T18, K7, K19, M2, N1, N2, S4, S7, C1) rebuilt it. Until then it showed every run in UTC with a Finished column, an error cut at 80 characters with its rest in a tooltip, a user id for who ran it, and its read's exception text.

## Who reads it

The Administrator alone (`[Authorize(Policy = "Administrator")]`). It lights Scheduled jobs (NavOwners), whose header action opens it; the trail is Home › Scheduled jobs › Run history, and there is no Back. Home's and a failed run's **Its runs** open it filtered to that job's failed runs (`JobLinks.RunsOf(key, "Failed")`).

## The page

- **The header**: h1 and tab "Run history"; "Every run of the eight jobs, newest first, kept for 90 days."
- **The column**: `<div class="people-page job-runs-page">`, `aria-busy` while it reads.
- **The filter** (FilterBar; the audit log's form): `form.search-container.people-filter`, `role="search"`, named "Filter the runs", on `.search-grid`: **Job**, a select of the stored jobs' keys with "All jobs" first, so it cannot miss (a key the address names but no job stores is added, shown selected; each option `@key`ed by its job, `565b978a`: unkeyed, the diff rewrote the address's option to the first stored key, and the select showed activity-draft-nudge over another job's rows); **Status** (All, Running, Succeeded, Failed); **From (a South African day)** and **To (a South African day)**, `type="date"` (a day runs from its 00:00 SAST, 22:00 UTC the day before, K7); then `.filter-actions`: **Show** (primary, `aria-disabled` until the first read is in) and **Clear filters** (outline) only while a filter is applied.
- **Nothing filters until Show**, and Show writes the page's own address, which is the page's state: `?job=&status=&from=&to=&page=`, with `size=` only where the page size is not 50 (`RunHistoryAddress`), so Its runs, Back and a shared link open the same rows. The page reads its address whenever it changes, and leaves out a value it cannot read (a status none of the three, a day not `yyyy-MM-dd`, a page below 1, a size the pager does not offer).
- **Under the filter, always drawn**: `h2.list-section-title` "Runs" (`tabindex="-1"`), then **the count line** (`p[tabindex="-1"]`, no live region, N2): "21 runs.", "1 run.", "2,800 runs.", "No runs match these filters."
- **The list**: a `DataTable` with `StackWide`, captioned "Runs, newest first": **Job**, the row header, the key alone (S7); **Started**, the South African day and the hour to the second, one `span.nowrap` ("2026-11-12 05:30:00 SAST", `565b978a`); **Duration** (`.col-fit--wide`): "7 ms", "1.2 s", "2 min 5 s", "—" while it runs; **Status**, the badge (Running amber, Succeeded green, Failed red, "Unknown" otherwise) and a failed run's error **whole** under it (`span.text-sm`, up to its 2,000 characters, breaking anywhere rather than widening the table, T1); **Ran by**, the person's name now ("Demo Administrator"), an erased person's pseudonym, or "Scheduler".
- **The pager** (PagerControls, M2): in its own order, info, Previous and Next, Per page; 50 a page; drawn whenever a row is ("Showing 1–21 of 21"). A page past the last (a hand-made address) reads the last.
- **The focus** (N1): Show focuses the count line; a page turn focuses "Runs"; Clear filters and "Clear the filters" focus Job; a change of page size leaves the focus on its select.
- **No match is not empty** (T17, R2-R03): under the count line, a `.detail-card--empty` "No runs match" over "The filters are kept above. Clear them to see all 21." ("… to see the one run."), with **Clear the filters** (primary).
- **No run at all** (R2-R08): "No job has run yet" over "The scheduler runs each job at its time; Scheduled jobs shows when each runs next.", with no count line and no pager; Show then focuses "Runs".

## States

- **Loading**: `p.loading-line[role="status"]` "Loading the run history." over a skeleton card, under "Runs"; the filter stands, Show `aria-disabled`.
- **Load failed**: "Could not load the run history. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel), which focuses the h1 once it reads.

## Narrow widths

**At 1024px and below the list stacks** (C1: `.job-runs-page` in the seven selector lists of app.css's flow 16 shared 1024px query), a readability choice, not an overflow fix (S8: the table needs 573 to 579px, which a container has from 887px). **While a row shows an error, Status asks for 30% of the table** (a share, not a floor; without it the error's max-content took the column to 463px at 1280 and broke every key). **From 1151px a key does not break at its hyphens** (`white-space: nowrap` on the row header: the narrowest table, "msf-invitation-expiry-reminder" 246px beside Started's 202px, needs 774px of the container's 828); from 1025 to 1150px it breaks there again, which keeps the table in its 702 to 827px container with a 200-character error. **From 1200px Ran by's header holds 9rem**, so "Demo Administrator" stays on one line (the narrowest table 826px of 877). The replay had found, at 1280 with an error, "entrustment-decision-" over "expiry", every Started over two lines and "Demo Administrator" over two. Below 641px the filter's controls and Show are 44px, Show the row's width.

## Contrast

No new colour token. The stacked labels and the empty card's copy `muted-text`, 5.09:1 on the surface; the badges are the five tints (Badge).

## Known gaps

- No page fault found re-syncing. The pager is a plain block, with no `Label`: it is not a `nav` landmark (DESIGN.md § Pager, "Its name").
