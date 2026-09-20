---
id: T130
title: "The annual quota: a trainee cannot see what is expected of them this year"
status: queued
priority: P1
owner: agent
depends_on: [T129]
created: 2026-09-20
---

# T130 — The progress page reads "1 / 24" against a lifetime total nobody published

**Severity:** High. `knowledge/EPA-PROGRAMME.md` § 2 calls it *"the most visible gap in the
product"*, and it is the one question a trainee actually opens the page to answer.
**Surfaced:** 2026-09-20. The work is [T098] phase 3 and has been planned since 2026-09-19 —
but it lived as three paragraphs inside T098 and as Wave 4 of the programme document, never as
a task. § 2 says so in as many words: *"Phase 3 has no task file of its own. Write one before
starting."* This is that file.

## Symptom

Annexure A publishes a per-EPA frequency — so many encounters per semester or per year. The
product does not track against it. `PaediatricCatalogueSeeder.cs:356-361` multiplies the annual
quota by four programme years, so the progress page shows a trainee **"1 / 24"**: a lifetime
total against a number the College never published, on a page whose only real question is
*what is expected of me this year.*

Nothing buckets completions by period, because until [T119] there was no trustworthy date to
bucket on.

## Root cause

There is no period concept. `CurriculumItemProgress` counts completions for all time. The
per-EPA `currency` string that would distinguish *"Essential — each semester (six months)"*
from *"… annually (12 months)"* is carried in `paediatric-epa-v11.1.json` on every EPA and is
**deserialized by nothing**: `EpaSeed` (`PaediatricCatalogueSeeder.cs:397-405`) declares no
`currency` property. Same omission as `wbaTools` before [T122] was filed.

## What to build

Blast radius, already enumerated in § 2 — treat this as the starting map, not as gospel:

- A period column on `CurriculumItemProgress`, plus a new unique index on
  `CurriculumItemProgressConfiguration.cs:16`. **Hand-written migration with its `.Designer.cs`
  and a snapshot update** — see CLAUDE.md; without the Designer file `MigrateAsync()` skips it
  silently.
- A period resolver.
- `CreditApplier.cs:74-107` — credit into the right bucket.
- `RebuildCurriculumProgressCommand.cs:30-32` — replay into buckets. It was rewritten under
  [T119] and still has no production caller; check its atomicity before giving it one.
- Eight progress readers and two UI sites.
- Seed `PeriodKind` **explicitly** from the `currency` string rather than parsing it out of
  prose at read time. Add the property to `EpaSeed` and re-seed.

Nothing is live, so re-seeding and a destructive migration are both available. Prefer them to a
backfill that has to guess.

## Decisions this needs first

**From the College, via [T129]:** **D13** (which eleven months, and where the semester boundary
falls — it cannot be derived and is not in the repo) and **D14** (a registrar who starts
mid-year).

**From the maintainer, decidable today** — all three have a recommendation in § 3C:

- **D17** — `GetStage` (a 365-day block from `ProgrammeStartDate`) and the period (a fixed
  calendar window) disagree by construction. *Recommended: two named concepts, `Stage` for the
  Y1–Y4 curve and `Period` for the quota, with the difference documented.*
- **D18** — does `RequiredCount` become per-period, or gain a sibling? *Recommended: redefine
  it and delete the multiplication that produced "24".*
- **D19** — does phase 3 use `CurriculumItem.WindowMonths`? *Recommended: leave it, and say so
  here.* It is per-item currency, a different question from the quota period, and it is read by
  nothing in the credit path today — an implementer will find a period-shaped column already
  present and assume currency is enforced. **It is not.**

## Verification

- [ ] A trainee's progress page shows the published per-EPA frequency for the current period,
      not a lifetime multiple — checked in the browser against Annexure A for three EPAs of
      differing currency
- [ ] `EpaSeed` round-trips `currency`, and `PeriodKind` is seeded from it — checked by a seed
      round-trip test in `Wombat.Infrastructure.Tests` (see CLAUDE.md: a DSL property with no
      `Serialize` half is dropped at publish with no error)
- [ ] Credit lands in the period containing `ObservedOn`, not `CreatedOn` — checked by a
      `CreditApplier` test with an encounter dated into a previous period
- [ ] The migration has its `.Designer.cs` and the snapshot is updated — checked by
      `MigrateAsync()` applying it on a fresh database
- [ ] A rebuild reproduces the same bucket counts as incremental crediting — checked by
      `RebuildCurriculumProgressTests`
- [ ] Full suite green — `dotnet test` per project, no `--no-build`

## Related

[T098] phase 3; Wave 4 of `knowledge/EPA-PROGRAMME.md` § 4. Depends on [T129] for D13/D14.
[T119] (`observed_on`) is **done** and is what made this startable. [T131] is phase 4 and
depends on the period concept this task creates. C1 closed the period-anchor question on
2026-09-19: the fixed national academic year, subdivided into two semesters.

## Notes

- **Observed:** `WindowMonths` exists, is validated, is admin-editable, is carried through
  clone, and is read by nothing in the credit path. Its only consumer is `AdmitTrainee.cs:126`.
- **Observed:** the § 2 blast-radius list was written 2026-09-19 and cites line numbers. [T119]
  has landed since. Re-check each site rather than trusting the line number.
- **Inferred:** "eight progress readers" is § 2's count, not one this task re-derived.
