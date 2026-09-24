---
id: T164
title: Learner feedback (PAED-015) cannot be recorded
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T164 — Learner feedback, EPA 15's twelfth instrument, has no way to be recorded

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low-Medium. It is the one v11.1 instrument with neither a seed nor a task besides [T154]'s two. PAED-015's
allow-list names it, and EPA 15 lists "structured feedback from learners" as an information source, so a registrar on
EPA 15 cannot produce part of the evidence the College asks for.
**Surfaced:** 2026-09-24, the EPA-stream survey. [T120] deferred it to "the MSF task". [T121] shipped without it, and
`EPA-PROGRAMME.md` § 5 item 4 said it was "designed nowhere" (at `431e69e`).

**Waits on operator decision D35** (`EPA-PROGRAMME.md` § 3D): an MSF template, or a separate token-based form.
Recommendation: **an MSF template with a new `Learner` respondent category.** It reuses the anonymity, thresholds and
token issuance that would otherwise be rebuilt badly.

## Symptom

Observed at `431e69e`:

- The vocabulary key exists: `Persistence/Seeds/paediatric-epa-v11.1.json:93-99` (`learner_feedback`, "Kept distinct
  from MSF").
- PAED-015's allow-list names it: `20260923105040_T122_WbaToolAllowLists.cs:58`.
- No seed folder exists for it under `Activities/Seeds/`. `MsfRespondentCategory` (`MsfRespondentCategory.cs:3-11`) has
  PeerDoctor, Consultant, Nurse, Ahp, Patient and Other, but no Learner.
- EPA 15 adds a precondition: "Feedback from at least two assessors across at least two teaching contexts, before
  entrustment is granted" (`tasks/done/T098-data/epa-detail.json:212`; Annexure A `annexure-a.json:194`).
  `MinimumCategoryResponses` (`MsfCampaign.cs:13`) counts responses per respondent category. It cannot express a
  per-teaching-context threshold.

## What to build

After D35, and assuming its recommendation:

1. `MsfRespondentCategory.Learner`, and a learner-feedback MSF template: a College questionnaire if one is supplied,
   otherwise a clearly labelled interim one.
2. The released evidence row. It is MSF-shaped (`counts_for []`, per D8), keyed `learner_feedback` so the [T122]
   allow-list and the committee can tell it from MSF. Either a `learner_feedback_cpsa` type or `msf_cpsa` with a
   distinguishing tool key; decide and record it.
3. **The teaching-context threshold needs its own design.** An invitation or response would need a teaching-context
   attribute, plus a campaign rule of "at least two contexts, at least two respondents". How the College counts "two
   assessors across two teaching contexts" is unknown: two occasions, or two respondent groups. The survey proposed
   it as a question for the College. Until it is answered, report the count and do not gate on it.

## Verification

- [ ] D35 is recorded as decided in `EPA-PROGRAMME.md` § 3D, with the choice made.
- [ ] A learner-feedback campaign can be created, answered by a Learner respondent and released, and its evidence row
      carries `learner_feedback`. Application tests and a browser run on dev.
- [ ] PAED-015's evidence surfaces (the committee snapshot and the activity list) show it as learner feedback, not MSF.
      Browser check.
- [ ] The teaching-context count is shown, or the task records why it is deferred. Test, or a note here.
- [ ] Full suite green, no `--no-build`.

## Related

D35, D8 (MSF credits nothing), D11 (category minimum), [T120], [T121], [T122], [T154] (the other two unbuilt
instruments), [T168] (MSF coverage per EPA).
