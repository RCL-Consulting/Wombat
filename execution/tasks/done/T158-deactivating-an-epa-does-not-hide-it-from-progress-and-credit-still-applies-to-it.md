---
id: T158
title: Deactivating an EPA does not hide it from progress, and credit still applies to it
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T158 — Deactivating an EPA takes it out of the picker, but not off the progress page, and not out of credit

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. No EPA in the v11.1 catalogue is deactivated and the College has not asked to retire one. It bites the
first time an administrator uses the Deactivate action: the EPA leaves the picker but stays on every progress surface,
and an activity already filed against it still credits it.
**Surfaced:** 2026-09-24, the EPA-stream survey, closing [T104]. This is T104's step 2, the one code change that outlived
the legacy data T104 was written about.

## Symptom

Observed at `431e69e`:

- The trainee's progress reader selects curriculum items by `CurriculumId` and owning institution only, with no
  `Epa.IsActive` predicate: `Features/Curricula/Quota/TraineeQuotaProgress.cs:165-168`. [T130] moved the query here
  from `GetCurriculumProgressForTrainee.cs:67-80`, the site T104 cited.
- The staff coverage reader has the same omission: `Features/Curricula/Quota/CurriculumCoverage.cs:93-95`.
- The EPA picker does filter. `ActivityReferenceDataService.cs:269-278` joins `Epa.IsActive`, and its comment at
  `:266-268` records that `CreditApplier` does not, calling the divergence "deliberate and one-directional".
- `DeactivateEpaCommandHandler` (`Features/Epas/GetEpas.cs:22-52`, `IsActive` set at `:52`) does not check for
  curriculum items that reference the EPA.

After a deactivation, then, a trainee can no longer file against the EPA but still sees its target and their shortfall
on `/portfolio/progress`. The staff dashboards still count it. An activity filed before the deactivation and completed
after it still credits it.

## What to build

Decide what deactivating an EPA means, record it in this file, then make every reader agree.

- **Readers (recommended): hide.** Items whose EPA is inactive are left out of `TraineeQuotaProgress`,
  `CurriculumCoverage` and any other reader that renders a target. That matches the picker. Grep every
  `Set<CurriculumItem>()` reader. Do not rely on the two named here being the only ones.
- **Credit (recommended): follow the picker.** An activity whose EPA is inactive at the moment of credit credits
  nothing. The transition stamps `CreditedItemCount = 0`, so [T108]'s warning explains it. Inferred: the natural home
  is `CreditTargetResolver`, the one implementation shared with the [T122] tool gate, so credit and gate cannot
  diverge. The alternative is to keep crediting, as the comment at `ActivityReferenceDataService.cs:266-268` argues.
  If that is chosen, say on the progress page why credit moved on a hidden item. Hiding the item while it keeps
  crediting is the one combination to avoid.
- `RebuildCurriculumProgress` replays through the same applier, so whichever credit rule is chosen applies on rebuild.
  Test that too.

## Verification

- [x] An item whose EPA is inactive is absent from the trainee's progress summary and from staff coverage. Handler
      tests on both readers; they would fail today.
- [x] The credit rule is recorded here and tested. Completing an activity against a now-inactive EPA either credits
      nothing and stamps `CreditedItemCount = 0`, or credits, as decided. Application test.
- [x] Reactivating the EPA restores the item on both readers. Test.
- [x] Browser, on dev: deactivate a PAED EPA as Administrator; the dev trainee's progress page no longer lists it;
      reactivate it and it returns.
- [x] Full suite green, no `--no-build`.

## Related

[T104] (closed 2026-09-24; this is its step 2), [T108] (the zero-credit stamp), [T130] (`TraineeQuotaProgress`),
[T122] (`CreditTargetResolver`).

---

## As built — 2026-09-24

**Decision:** a deactivated EPA leaves progress and takes no new credit, as it already leaves the pickers. Progress
readers and `CreditTargetResolver` skip items whose EPA is inactive.
- `CurriculumItemDto.EpaIsActive` feeds the curriculum editor. It shows "(inactive: not in force)" on the row, a notice,
  and "(inactive)" in the pickers.
- EpaEdit says what deactivation does, and confirms it.
- The rebuild page warns that a rebuild while an EPA is inactive removes that EPA's progress.

Known boundary, pinned by a test: an EPA reactivated after submission is not re-checked by the tool gate.

Browser on dev, 2026-09-24 (master `b0cb335`, the batch-C check): 
- PAED-014 (no evidence on it) was deactivated with the confirmation. The trainee's PAED-014 card disappeared, and the
  CBD picker went from 12 EPAs to 11. The admin row read "(inactive: not in force)" with the notice.
- Reactivated: the card and the picker came back, and `IsActive = t`.

**Filed:** [T195] (a curriculum item can name another owner's or sub-speciality's EPA) and [T196] (re-credit on
reactivation, a time-aware rebuild, the hand-on question, EpaEdit's leftovers).
