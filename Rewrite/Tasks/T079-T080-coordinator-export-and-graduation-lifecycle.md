# T079 + T080 — Coordinator portfolio export (F-5-5) + graduation lifecycle (F-5-4)

**Status:** ✅ Shipped 2026-06-01 (Opus). Two Act 5 findings.

## T079 — Coordinator cannot export portfolios (F-5-5)

`ExportPortfolioCommand.DemandExportAccess` admitted Administrator / InstitutionalAdmin /
SpecialityAdmin / SubSpecialityAdmin or the trainee themselves — **not Coordinator** — so the
scenario's Step 5.5 ("Coordinator reproduces the PDF") failed on authorisation. **Fix:** added
`Coordinator` to the allowed roles. +1 test (`Handle_CoordinatorCanExportTraineePortfolio`).
Live-verified: Smit (Coordinator) exported Molefe's portfolio — and got the **identical** content-hash
file as Mbatha's export (`portfolio-176a91aec2bc.pdf`), so Step 5.5's byte-for-byte reproducibility now
passes on both authorisation (T079) and determinism (T078).

## T080 — No trainee graduation/completion lifecycle (F-5-4)

Only a generic `Deactivate` existed (it leaves the `Trainee` role intact); there was no way to record a
graduation, no role transition, no "Completed" view, and no graduation email — a graduated trainee was
indistinguishable from a withdrawn one.

**Fix — a completion concept distinct from deactivation:**
- **Domain:** `TraineeProfile.CompletedOn` (`DateOnly?`, private set) + `Complete(completedOn)` — records
  the date and deactivates; throws if already inactive or if the date precedes the programme start.
  Migration `20260601172756_TraineeCompletion` (column only).
- **Command:** `CompleteTraineeProfileCommand` (scope-guarded via `CanAccessInstitution`) — calls
  `Complete`, then **removes the `Trainee` role** (there is no Alumnus role — the profile is archived),
  then sends a **`GraduationEmail`** to the graduate.
- **UI:** `TraineeProfileEdit` gains a "Completion date" field + "Mark complete" button (shown while
  active); the summary shows `Completed: <date>`. `PendingTraineesList` now splits **Active profiles**
  from a **Completed & closed profiles** section (Outcome = `Completed <date>` or `Withdrawn`).
- DTO + both trainee-list/get projections carry `CompletedOn`.

**Tests:** `TraineeProfileCompletionTests` (+3 Domain: records/deactivates, rejects inactive, rejects
pre-start) and `CompleteTraineeProfileCommandHandlerTests` (+2 Application: records + removes role +
emails; scope-guard rejects out-of-institution and mutates nothing). Domain 45→48, Application 265→268.
Infrastructure 10, Architecture 19, Web 42 green; Integration not run (Docker).

**Live-verified:** marked Molefe complete (2029-12-15) → profile `IsActive=false`, `CompletedOn` set,
her roles became **(none)** (Trainee removed), success banner reports the role removal + graduation
email, and she moved to the "Completed & closed" list. Snapshot `act5-complete` now reflects the proper
graduation (replacing the earlier `Deactivate` workaround).

**Note:** there is still no first-class `Graduate`/`Complete` **committee decision category** (F-5-1,
still open) — graduation is represented by the STARs + this profile-completion lifecycle, not a decision
enum value.
