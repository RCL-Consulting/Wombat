# Flow 18 build: the review

The build on `t400` at `07920cc9` (§ 0, lanes A, B, C, the sweep and its fix `5b9bcf53`, lane W `88ee3636`) was
reviewed 2026-11-11 from two sides, each a Sonnet agent: **the code** (`build-review-code.md`: 0 must, S-1–S-2, N-1–N-8;
the full Integration suite 541/541 on PostgreSQL, the once-flaky `EvidenceEpaMigrationPostgresTests` passing) and **the
pages and runbook** (`build-review-pages.md`: M1, S1–S2, 13 nits, the NEW words in 26 groups each with its "replaces"
cell; Web.Tests 4950/4950). The two files are kept as they are; this one merges them. Each must-fix and should-fix below
was checked by the integrator before it was carried.

## What holds

- The migration is the table and its two indexes, nothing else; the Designer and snapshot agree (code).
- Mail outcomes are recorded at the four points, never throw, never hold an address or body; one queue reader (code).
- P1 as built: a framework `InvalidOperationException` stores "The action failed." (checked by a scratch program); the
  audit's institution filter cannot widen an institution's administrator's scope; `SummaryJson` is the Administrator's
  only; Run now's disabled check sits under the lock before any run row (code).
- Every shipped word is on a board or in the NEW list; the dialogs, the focus after each action, `aria-required`, the
  accessible names, badges, SAST times and the pager's order hold; 16 changed steps match the code, and the Accounts
  counts 9, 11 and 24 trace (pages).
- The sweep's four open points and lane W's three are fine as built (pages).

## Must-fix

| # | From | Defect | Checked | Fix |
|---|---|---|---|---|
| M1 | pages M1 | A.2.9 expects `scheduled-job-run-retention: "no old runs to delete."`; the job now logs "no old runs or mail outcomes to delete." Lane W did not list A.2.9. | `appendix-cross-cutting.md:680`, `:685`; `ScheduledJobRunRetentionJob.cs:49` | Rewrite A.2.9's Expect (both lines) to the new sentence. |

## Should-fix

| # | From | Defect | Checked | Fix |
|---|---|---|---|---|
| S1 | code S-1 | Home still reads accounts, jobs and mail after the database probe fails, so a database that stops answering makes Home wait out each connection timeout (about a minute) before it says so. | `GetAdministratorDashboardSummaryQuery.cs:82-90` | Skip the dependent reads when the probe fails; they read "Not read". |
| S2 | code S-2 | `AuditFailureText` keeps any `UnauthorizedAccessException`'s text, as P1 literally says; the framework throws that type for file access with a path in its text. Latent: no command does file I/O. | `AuditFailureText.cs:56-57` | **F1, the operator's.** Recommended: apply the Wombat throw-site test to it too. |
| S3 | pages S1 | `states.md:145` says Home reads "The other 7 last succeeded." at A.2.2; the digest has not run yet (A.2.4), so the code renders "The other 6 last succeeded." and the digest's "has not run yet". | `AdministratorHomeWords.cs:128-139` | Correct the row. |
| S4 | pages S2 | DESIGN.md's flow 18 text has no paragraph for Scheduled jobs and Run history as pages (schedule words, Last/Next run cells, the filters and their address, the h2 "Runs", the count line's focus). | DESIGN.md flow 18 | Add it. |

## Nits

Twenty-one in the two files (code 8, pages 13), carried to the fix pass as one list. The ones that matter: DESIGN.md's
institutions bullet says "visually hidden name" where Edit uses `aria-label`; DESIGN.md's payload ceiling is 28rem of
content; Home reads "All 0 jobs last succeeded." with no job definitions; a hand-made `?page=` past the end shows a
misleading card (Run history, the audit log), and the audit list does not clamp `Page` (an `int.MaxValue` page overflows
the offset); Run history throws on `from=0001-01-01` or `to=9999-12-31`; `GetScheduledJobStatusQueryHandler.cs:62` does
not set the UTC kind; the dashboard and Run now take a principal but only the pages check `IsAdministrator()`; the
pipeline logs cancellation at Error; a concurrent create clashing on the name is refused under Short code; retention
loads every expired row; `RecordAsync` is uncancellable.

## The NEW words

`build-review-pages.md`'s last section: 26 groups, each with its "replaces" cell (C5), to go into `build-contracts.md`
§ 5 beside rows 1–8.

## For the operator

- **F1 (S2).** P1 keeps an `UnauthorizedAccessException`'s words as a refusal. Recommended: keep them only when Wombat's
  own code threw it, as for `InvalidOperationException`; a framework one stores "The action failed." with its full text
  in the log. The alternative is P1 as written, with the latent path leak.
