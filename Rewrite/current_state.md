# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T038 — Trainee surface (GUI review cluster 2).** Model: Sonnet.

Browser-verify and polish: `TraineeDashboard`, `Portfolio/MyProgress`, `Portfolio/MyAuthorisations`, `Activities/MyActivities`, `Portfolio/ExportPortfolio`, `Portfolio/VerifyExport`. Apply the rubric in `Rewrite/gui-review-plan.md`. **Local dev DB is currently broken** — see "Open blocker" below — start by unblocking that, since this task needs the app running in a browser.

## This session at a glance

Practical plan closed. **T036 (accreditor-specific export template) deferred indefinitely** — WBA is new locally, no accreditor format spec is forthcoming, and a speculative generic template would likely be rewritten when a real spec lands. T023's portfolio PDF covers the trainee-facing export in the meantime.

New plan: `Rewrite/gui-review-plan.md`. Design-system audit across ~65 pages + 15 shared components, six clusters (T037–T042), ~8 working days.

T037 shipped — NavMenu's bespoke `.bi-*-nav-menu` background-image CSS replaced with `Icon.razor`. Added Lucide SVGs (shield, clock, key). Deleted the duplicate icon mechanism. Build + 270 tests green. **Browser verification of T037 was blocked** because the dev server fails on startup with `ActivityTypes."Title"` not existing — the T028 data migration's UPDATE references a column the local DB doesn't have. Pre-existing infra issue, unrelated to T037; recorded under "Open blocker" so the next session picks it up first.

## Open blocker

Dev server (`dotnet run --project src/Wombat.Web/Wombat.Web.csproj`) crashes during `MigrateAsync()` with:

```
42703: column "Title" of relation "ActivityTypes" does not exist
SQL: UPDATE "ActivityTypes" SET "Key" = 'reflective_note', "Title" = 'Reflective Note' WHERE "Key" = 'star_reflection';
```

Source: T028 (commit `dc506d1`) data-migration step. Either the migration is wrong or the local DB is at a schema state from before `Title` was added (or it lives under a different name like `Name`/`DisplayName` here). Diagnose by reading the T028 migration, the `ActivityType` entity + its EF configuration, and whichever earlier migration introduced the title column. Decide whether to amend the migration, write a corrective hand-written migration, or rebuild the local DB.

This blocks browser verification on every GUI-review cluster until resolved.

## Last completed

**T037 — Consolidate NavMenu icons to `Icon.razor`.**

- `NavMenu.razor` — replaced 31 `<span class="bi bi-*-nav-menu">` invocations with `<Icon Name="..." />` (home, user, shield, calendar, file-text, book, inbox, users, alert-triangle, settings, clock, key, log-out).
- `NavMenu.razor.css` — deleted the `.bi` and 10 `.bi-*-nav-menu` background-image blocks. Added a single `.nav-item ::deep .icon` rule sizing icons to 1.25rem with the previous left/right margins; `flex-shrink: 0` prevents collapse on narrow nav.
- New SVGs: `wwwroot/icons/shield.svg`, `clock.svg`, `key.svg` (Lucide, single `<svg id="i">` matching the existing convention).
- Side benefit: nav icons now follow `currentColor` from `.nav-link` color, so they recolour on hover/active instead of staying hardcoded white.
- **Verification: build clean, 270/270 tests pass (Domain 45, Application 168, Architecture 19, Web 38). Browser verification not performed — blocked by the pre-existing dev-server migration error described above.**

## Plan this session works against

`Rewrite/gui-review-plan.md` — design-system audit of ~65 pages + 15 shared components. Cluster 1 (T037) active.

`Rewrite/practical-plan.md` — closed: T035 done, T036 deferred indefinitely.

## GUI review sequence

1. ✅ T037 — Consolidate NavMenu icons to Icon.razor (browser verification deferred)
2. T038 — Trainee surface (active — 1.5 d, blocked on dev DB)
3. T039 — Committee flow (1.5 d)
4. T040 — Admin hierarchy (2 d)
5. T041 — Activity platform (2 d)
6. T042 — Account & auth shell (1 d)

## Block 4 / practical-plan sequence (closed)

1. ✅ T035 — Assessor training status field
2. 🚫 T036 — Accreditor-specific export template (deferred — WBA new locally, no accreditor spec)

## Test status at handoff

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- Domain tests — 45/45 pass
- Application tests — 168/168 pass (3 new for training status round-trip)
- Architecture tests — 19/19 pass
- Web tests — 38/38 pass
- Infrastructure tests — `SeedParseTests` pre-existing parallel-run flakiness; passes in isolation
- Integration tests — Docker-gated; not run locally

## Known T035 compromises

- **No validation that the date is plausible.** The date picker accepts any `DateOnly?` — a far-future date would be stored as-is. The field is information-only and admin-only, so the risk is cosmetic; add a sanity guard if a programme starts filling in future placeholder dates.
- **No browser-level verification this session.** List/edit wiring is a direct column-through, identical in shape to existing profile fields; trusted from the Application-layer round-trip tests.

## What remains (operational, not code — carried forward from T016)

- Execute `deploy/README.md` first-boot checklist against a real Linode server
- Configure DNS, TLS certificate (Caddy auto-provisions via ACME)
- Set production secrets in `/opt/wombat/config/wombat.env`
- Run `--seed` to provision the admin user and seeded activity types
- Revoke UPDATE/DELETE on AuditEntries table after first migration

## Companion reference docs

- `EPA Book/evaluation.md` — 92-requirement book scorecard (reference, not todo list)
- `EPA Book/critique.md` — literature-backed reasoning for practical-plan compromises
- `Rewrite/book-fidelity-plan.md` — superseded; kept only because `critique.md` cites it

## Last verified commits

- `1d25995` — T037 (consolidate NavMenu icons to Icon.razor; browser verification deferred)
- `ba7c7d8` — docs: defer T036, open GUI review plan (T037–T042)
- `9d60cd7` — T035 (assessor training status field)
- `dd18b66` — T034 (EPA core/elective + stage-indexed supervision levels)
- `9910ba1` — T033 (per-trainee per-EPA trajectory chart)
- `2e02a1e` — T032 (sampling-concentration warning on review detail)
- `ac4fdb9` — T031 (formative-only committee review mode)
- `c9b00d0` — docs: record T030 commit hash
- `10f7e55` — T030 (STAR certificate PDF + authorisations UI)
- `21f7959` — docs: record T029 commit hash in current_state handoff
- `91ff841` — T029 (EntrustmentDecision aggregate / STAR)
- `dc506d1` — T028 (rename `star_reflection` → `reflective_note`)
- `bf583ee` — MailKit 4.16.0 security bump (closes GHSA-9j88-vvj5-vhgr)
- `864ad3b` — T016 (rewrite-complete baseline)
