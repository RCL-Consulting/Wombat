# T361 step 6: the build's review

Three read-only reviewers on Sonnet read `t361` at `e07311d9` through the lenses in `review-lanes.md`, 2026-10-12. None
found a high-severity defect. Findings below, with the integrator's disposition; every "fix" goes to one fix pass on
`t361-fix`.

## R: the code, the data and authorization

| # | Sev. | Finding | Disposition |
|---|---|---|---|
| R1 | medium | T309 item 2 only partly met: a formative check-in reads "Annual progression" in `ReviewsSchedule.razor:120` (Type cell), `ReviewRowNames.cs:29` (row names) and flow 06's `RegistrarWords.cs:122` (review link); also the PDF, `CommitteeSectionComponent.cs:94`. | Fix all four with `IsFormative`; a test per caller. |
| R2 | low | The migration rewrites a deferred line on a still-open review to NotDecided; a STAR staged there issues while the line reads Not decided. | Fix in the migration: Deferred on a Scheduled or In progress review becomes Due; NotDecided otherwise. |
| R3 | low | Decisions due: `ReferenceOf`'s NotDecided case ignores E3's predicate, so the named review can differ from the one the status rests on. | Fix: the same predicate. |
| R4 | low | A direct Record with no rationale gets FluentValidation's default words, not the page's. | Fix: the validator's message is the page's sentence. |
| R5 | low | The Start preview builds the whole snapshot per read. | Leave: no leak; revisit only if slow. |

Sound: the preview's read path and its agreement with Start; Start's chair refusal; the migration's order, Designer,
snapshot and Down; `UndecidedNote` (trim, null, length, `[Redact]`, carried by a remitted appeal); Record and Ratify's
order of checks; `CloseAgenda`; E3 in `Standing`; no Deferred left in code; `GetCommitteeReviewById`'s scope; SA dates.

## D: the page, the design system and accessibility

| # | Sev. | Finding | Disposition |
|---|---|---|---|
| D1 | medium | The evidence reader's heading is h4 under the h2 Agenda, and the activity it embeds opens h2 About and h2 form sections. | Fix: the reader h3; `ActivityAbout` and `ActivityDetail` take a heading level, 4 here. |
| D2 | medium | Below 641 px: Stage pending decision, Cancel, "Show the other N snapshot lines", the snapshot and Present ticks, and the appeal's Resolve are under 44 px. | Fix in the phone block. |
| D3 | medium-low | Stacked agenda cells carry `data-label` but no rule prints it. | Fix: `td[data-label]::before` inside the container query. |
| D4 | low | The foot form and an open line on the same EPA both render `id="chart-jump-{epaId}"`. | Fix: prefix with `IdPrefix`. |

Sound: the bars, `aria-disabled` and `.is-unavailable`, the dialogs' safe buttons and focus, Remove's classes and names,
Stage's `aria-expanded`, stacking and focus clearance, no inline style, every class defined, the table roles, no
"defer" and no pronoun, ISO dates and SAST.

## G: the runbook, the cast and regressions

All six board-versus-code disagreements the runbook lane reported were judged right in the runbook (5.3, A.7.13, 4.38,
4.18) or honest (A.7.6, T309, now R1).

| # | Sev. | Finding | Disposition |
|---|---|---|---|
| G1 | medium | Stale Gap lines contradict their own Expects: F-4.46a (act 4), F-A.7.13a/b (appendix); coverage.md has no T361 "its build closes" paragraph. | Fix: the paragraph, and each Gap "closed by T361, for the replay to confirm". |
| G2 | medium | No step or state walks the review in progress at 390 or 768 (Stage, Remove, the dialogs, the Present ticks). | Fix: a states row "Narrow, in progress" at 4.16–4.19 and 4.24–4.25. |
| G3 | low-medium | states.md's "Ratify blocked" row cannot occur since D53; no row for a failed Refresh or a panel that cannot sit a quorum. | Fix: move the row to the unreachable table (or an SQL setup); add the two rows. |
| G4 | low-medium | Retained T358 Actuals under rewritten Expects describe the deferring page. | Fix: a one-line banner on acts 4, 5 and the appendix. |
| G5 | low | coverage.md says 332 steps; 331 since 4.21 went. | Fix. |
| G6 | low | A.7.6 names a link target that a ratified agenda does not have; item 20 is about the chart and progress links. | Fix: name those links. |
| G7 | low | The subtitle's training year is read at the period's end (E6), the standing on D; the runbook's clock note covers the standing only. | Fix the note; the rule stays (E6). |

Not verifiable from the repo: act 4's "54" presses and the 93 before the recount (80 checks out).

## After the fix pass

The six suites, then the squash onto master as one T361 commit. The replay (step 7) confirms T356, T273, T309 item 2,
T329's and T260's review halves and T324's decision form before any of those tasks closes.
