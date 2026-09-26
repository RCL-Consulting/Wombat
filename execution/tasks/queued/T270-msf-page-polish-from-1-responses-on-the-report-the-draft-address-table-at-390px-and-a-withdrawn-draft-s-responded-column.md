---
id: T270
title: MSF page polish: "from 1 responses" on the report, the draft address table at 390px, and a withdrawn draft's Responded column
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T270 — MSF page polish: "from 1 responses" on the report, the draft address table at 390px, and a withdrawn draft's Responded column

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low, cosmetic.
**Surfaced:** 2026-09-25, the MSF chain 2 browser check.

## Symptom

- `/msf/reports/{id}` reads "Average: 4.00 from 1 responses." (campaign 18 on dev). The PDF was fixed by T249; the
  page was not.
- At 390px, a learner-feedback draft's "Addresses invited" table is 428px in a 306px container, so Remove sits
  off-screen until scrolled. "Teaching context" and its cells break mid-word.
- A campaign withdrawn while still a draft (campaign 20) shows its counts table with a "Responded 0" column.

## What to build

Pluralise the report's counts (one helper shared with T249's PDF). Let the address table stack or wrap its columns at
phone width, with Remove always visible. Show no Responded column for a campaign that never opened. bUnit and CSS
tests; browser at 390px.

## Verification

- [ ] Each item, with a test, and one browser pass at 390px.

## Related

T247, T249, T225, T266.

Note, 2026-09-25 (the T269 review): `MyMsfReports.razor` shows a refusal twice: in the top danger alert (no `Role`) and
again as the StatePanel's `LoadError`, which replaces the "Released reports" list. The wording ("…not available to the
current trainee.") also reaches a Coordinator who types another report's id.

Note, 2026-09-25 (the MSF chain 3 browser check): the trainee's learner-feedback report reads "Rates the trainee's
teaching overall.: 4.00". The page adds a colon after question text that already ends in a full stop.

Note, 2026-09-25 (the T284 review): on a draft whose trainee is no longer current, `CampaignEdit` still enables Add
invitee and Open campaign, which are then refused. Disable both with a visible reason (a `SubjectIsCurrent` flag on
`MsfCampaignSetupDto`), keep Withdraw working, and move the focus to `.action-result` after a refusal that disables the
pressed button.

## Notes

- **T295 replay, 2026-09-26 (C86).** Note, 2026-09-26 (the T295 replay, step 3.47, F-3.47a): the same stray colon on the 360° MSF report. Dr Molefe's report reads "Rates the trainee's overall professional performance.: 4.00" (screenshot `design/baseline/act-3/3.47-2-molefe-report.png`). Both report kinds come from one line, `MyMsfReports.razor:75` (`@question.Prompt: @question.Scale.Average`). This widens the item: the portfolio PDF's MSF section does the same at `MsfSectionComponent.cs:165` (`"{question.Prompt}: "`) and `:174` (`"{question.Prompt}:"`). Use one helper that adds the colon only when the prompt does not already end in punctuation, in both the page and the PDF, and cover it with a bUnit test and a PDF text test.
- **T295 replay, 2026-09-26 (C87).** Note, 2026-09-26 (the T295 replay, step 3.50, state `my-msf-reports--foreign`, F-3.50a): confirmed. Signed in as Dr Dlamini, `/msf/my-reports/1` (another trainee's campaign) shows "The selected report is not available to the current trainee." twice: once as the danger alert, and again as the Released-reports StatePanel's load error, in place of her own (empty) list. The runbook's states.md expects "her empty list, nothing selected" (screenshot `design/baseline/states/my-msf-reports--foreign.png`). The cause is the one this task's T269 note gives: `MyMsfReports.razor:18-20` and `:26` both read `_error`, which is set at `:137` from the throw at about `:127`. Keep the list's load error apart from the selection's refusal, so the list still renders under a refused selection. This does not widen the task.
- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26 (states/campaign-report--blocked.png, --no-epa.png).** The report's other counts are not pluralised either. BlockedReason (CampaignReport.razor:296-303) reads 'Release needs 5 responses; 1 have come back.' Its second branch has the same shape: '{N} respondent groups to report; {M} cleared the {K}-response threshold'. The helper this task shares with T249's PDF should phrase these too ('1 has come back', '1 respondent group'). bUnit: a campaign with one response, and one with one surviving group.
