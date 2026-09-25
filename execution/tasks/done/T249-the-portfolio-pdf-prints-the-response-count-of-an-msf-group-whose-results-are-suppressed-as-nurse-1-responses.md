---
id: T249
title: The portfolio PDF prints the response count of an MSF group whose results are suppressed, as "Nurse: 1 responses"
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T249 — The portfolio PDF prints the response count of an MSF group whose results are suppressed, as "Nurse: 1 responses"

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It is a count, not an answer, and it is ungrammatical.
**Surfaced:** 2026-09-25, the T225 review. It predates T225.

## Symptom

`MsfSectionComponent` prints the exact count for a group whose results are suppressed below the anonymity threshold.
The trainee's web copy (`MyMsfReports.razor`) leaves suppressed groups out. "1 responses" is also wrong.

## What to build

Print what the web copy shows: leave suppressed groups out, or say "fewer than N responses, not shown", once. Pluralise
the counts.

## Verification

- [x] PDF text tests: a suppressed group has no count, and "1 response" is singular.

## Related

T225, T217, T205, T169.

---

## As built — 2026-09-25 (`3775d5e`)

The portfolio PDF's MSF section prints no count for a group suppressed below its threshold, and one line: "Respondent
groups with fewer than N responses are not shown, to protect the respondents' anonymity." Counts are singular at one
("1 response", "1 rating"), and averages print the same on any host. PDF text tests pin the category threshold.

DESIGN.md records what can still be worked out from the total and the printed groups, and why it is accepted. **The
operator's call:** the stricter fix, dropping group counts from the PDF, is recorded there and not made.

Browser on dev (scripted Chrome, master `8e00e68`):
- **Campaign 16** (minimums 3/2/1; 2 Consultants and 1 Nurse, all answered): the PDF read "Total responses: 3",
  "Consultant: 2 responses", no Nurse, and the one "not shown" line.
- **Campaign 18:** "Consultant: 1 response … (1 rating)" and "Nurse: 1 response". Old campaign 14 reads "Allied health
  professional: 1 response".

**Found:** the coordinator's report page still says "from 1 responses". Filed as [T270].
