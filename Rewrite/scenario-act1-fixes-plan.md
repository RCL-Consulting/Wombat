# Scenario Act 1 fixes plan

The Playwright route-and-surface audit on 2026-05-24 (captured inline in `scenario-paediatrics.md`) found three hard gaps, three route mismatches, four UX-friction items, and three missing features across Act 1 of the Paediatrics scenario. This plan triages them.

## Triage

| Category | Count | Fix shape |
|---|---|---|
| Doc-only (scenario was written ahead of code; absorb the truth) | 7 | One pass through `scenario-paediatrics.md` |
| Small code fixes (UI + small auth surface) | 4 | One task each, all small |
| One real missing feature | 1 | New admin CRUD surface |
| Documented compromises (workaround cheaper than fix) | 3 | Note in doc only |

## Task list

### T050 — Scenario-doc corrections (docs-only)
Single pass through `Rewrite/scenario-paediatrics.md` to absorb the audit's findings so the runbook plays as-written:
- Step 1.1: drop "Viewing as Administrator" subtitle from expected wording.
- Step 1.2: full rewrite. Route → `/admin/invitations` (form embedded). Role → `InstitutionalAdmin`. Move *after* Phase 1.B (institution must exist before invitation).
- Step 1.5: link label is `Specialities`, not `Manage specialities`.
- Step 1.7: replace with workaround prose — "reuse the seeded scale; admin CRUD tracked as T054."
- Step 1.8: acknowledge the extra `Required knowledge and skills` field.
- Step 1.11.b: `LongText` → `Long text`.
- Step 1.11.c: replace JSON with the corrected schema already inline in the audit findings. Propagate the actor-DSL translation (`actor_roles: [...]` → `actor: "role:..."`; `actor_field: "x"` → `actor: "field:x"`; `initial` → `initial_state`; add top-level `version: 1`) to all 9 summarised types in Step 1.12.
- Step 1.11.e: clarify Publish renders alongside Save draft only when an unsaved draft exists.
- Step 1.13: column wording (`Published` / `Draft` / `Status` as three separate columns).

**Effort:** ~1 hour. Pure docs. Unblocks a human play-through immediately. Suggested model: **Sonnet** — mechanical edit, no judgement.

### T051 — Invitation form: capture First/Last name + surface registration URL + fix dev SMTP
Add `FirstName` + `LastName` columns (nullable text) to `Invitation`. Surface in the `InvitationsList` issue form. Pre-fill the accept-invitation form when present. Migration + Designer file (per CLAUDE.md hand-written migration rules).

**Additionally (from 2026-05-24 play-through, commit `d8a7557`):**
- **Surface the raw registration URL on the InvitationsList page after issuance.** `IssueInvitationCommand` already returns `IssuedInvitationResult.Token` in plaintext; `InvitationsList.IssueAsync` (lines 195–205) currently discards it. Capture it, render `http://{host}/account/register?token={token}` in a copy-to-clipboard chip on the just-issued row, and replace the misleading status message ("The stub sender logged the registration link") with something honest.
- **Align dev SMTP defaults to Papercut.** `src/Wombat.Web/appsettings.Development.json` has `Email:SmtpPort=1025` but Papercut SMTP listens on port 25 by default — every dev invitation email silently fails 3 retries and is dropped. Change the dev default to 25, or document the override `$env:Email__SmtpPort=25` in `Rewrite/INFRASTRUCTURE.md`. The UI surface change above is the more durable fix because it decouples the runbook from SMTP being configured at all.

**Effort:** ~3 hours (was 2; the URL-surface + SMTP tidy adds an hour). Suggested model: **Sonnet**.

### T052 — Invitation form: allow `Administrator` role + global scope
Re-add `Administrator` to the role combobox. When selected, Institution combobox becomes optional and Speciality / Sub-speciality dropdowns hide. Server-side guard: invitations with `role=Administrator` and `institutionId=null` accepted only when the issuing user holds the Administrator role themselves (already true for the bootstrap admin). Regression test asserts a non-Administrator cannot issue an Administrator invitation. Update CLAUDE.md's "Administrator role cannot be assigned via SSO" note to also call out the invitation rule.

**Effort:** ~3 hours. Touches auth rules. Suggested model: **Opus** — cost of getting auth wrong is high.

### T053 — Activity-type Metadata: Scope Id picker
Replace the raw `Scope Id` integer spinbutton on the activity-type Metadata tab with a context-aware picker. When Scope=Institution → list institutions; Speciality → list specialities scoped to what the editing user can see; SubSpeciality → likewise. When Scope=Global → hide the field entirely (Scope Id is meaningless).

**Effort:** ~2 hours. UI-only. Suggested model: **Sonnet**.

### T054 — Admin CRUD for `EntrustmentScale` + `EntrustmentLevel`
New surface from scratch:
- Routes: `/admin/entrustment-scales`, `/admin/entrustment-scales/new`, `/admin/entrustment-scales/{id}`.
- List + edit pages following the institutions pattern (PageHeader, table, FormField, Save/Cancel).
- MediatR commands: `CreateEntrustmentScale`, `UpdateEntrustmentScale`, `ArchiveEntrustmentScale`. No hard delete — existing forms and reviews reference scales by FK.
- Nested editor for levels (add / remove / reorder by `Order`).
- FluentValidation: at least 2 levels, contiguous `Order` from 1, unique `Order` and `Label` per scale, `CatalogueKey` unique across active scales when present.
- Web tests (bUnit) for the editor; Application tests for the handlers.
- Add nav-menu entry between "Activity Types" and "Scheduled Jobs".

**Effort:** ~6–8 hours. Largest task; new surface. Sequence **after** T051+T052 to avoid touching admin code twice. Suggested model: **Opus** — needs architectural judgement to match existing admin patterns.

### T056 — InstitutionalAdmin role-power audit (raised by play-through)
The 2026-05-24 play-through showed that an `InstitutionalAdmin` user is locked out of every admin page except `/admin/entrustment-decisions`. Pages for EPAs, Curricula, Activity Types, Invitations, Audit, Jobs, Trainees, Assessors, Forms, SSO, and the edit pages for Institutions / Specialities / Sub-specialities are all gated on `[Authorize(Policy = "Administrator")]` or `[Authorize(Roles = WombatRoles.Administrator)]`. This breaks the Paediatrics scenario's premise that Prof Mbatha (as InstitutionalAdmin) owns the rest of the setup after Phase 1.B.

Two acceptable resolutions, pick one:

**Option A — grant institution-scoped admin powers to `InstitutionalAdmin`.**
- Update every `[Authorize(Policy = "Administrator")]` on pages that should be available within an institution to `[Authorize(Policy = "AdministratorOrInstitutionalAdmin")]` (new combined policy).
- Add handler-side scope guards: any command/query touching `InstitutionId`-bearing entities checks that the caller's `InstitutionalAdmin` scope matches the target. Queries filter by scope; commands reject with `ForbidResult` when scopes don't match.
- Architecture tests assert that every handler reachable from an InstitutionalAdmin-allowed page calls a scope-guard helper.
- Web tests cover the access flips (InstitutionalAdmin sees own institution's EPAs but not others').

**Option B — accept the model and revise the scenario.**
- Rewrite `scenario-paediatrics.md` so the bootstrap admin runs all Act 1 phases; Mbatha is created in Act 2 as a non-admin user (Coordinator?) who only consumes the curriculum + activity types. Cast table updates to make the bootstrap admin a recurring character through Acts 1–5.
- Reduces operator surprise for hospitals running multi-institution deployments — they would never expect a single "InstitutionalAdmin" to define EPAs and curriculum anyway; that's a programme-level activity.

**Recommended:** Option A, gated on a discussion with whoever owns the role model. Option B is the cheaper revert if A is too invasive.

**Effort:** Option A ~12–16h (touches ~25 pages + handlers + tests). Option B ~3h (scenario doc rewrite). Suggested model: **Opus** either way — auth model decisions are load-bearing.

### T055 — ActivityType Edit: post-save housekeeping bundle
Three small UI fixes that surfaced in the 2026-05-24 play-through, grouped because they all touch admin edit pages:

1. **Publish button always visible.** Show `Publish` even when `_editor.HasDraft == false`, with `disabled="true"` and a `title` tooltip "Save a draft to publish." Removes the magic-disappears-after-publish surprise.
2. **Redirect to `/admin/activity-types/{id}` on first Save draft.** Today the URL stays at `/admin/activity-types/new` after the first save; the page caches the new id internally so Publish still works, but a refresh sends the user back to the blank form. Either redirect or update the URL via `NavigationManager.NavigateTo($"/admin/activity-types/{id}", forceLoad: false)`.
3. **Fix the "Create X" page title after save** across Institution, Speciality, Sub-speciality, EPA, Curriculum, and Activity Type edit pages. The `<PageTitle>` should be conditional on whether the entity has an id (e.g. `@(_id > 0 ? "Edit X" : "Create X")`). Six pages, six one-line edits.

**Effort:** ~1 hour. Was 15 min; the two new items push it up. Suggested model: **Sonnet**.

## Sequencing and dependencies

```
T050 done (commit 96104a1)
2026-05-24 play-through done (commit d8a7557) — surfaced T056, bumped T051/T055 scope
T055 done (commit 6eaef56) — Publish always visible + post-save SPA redirect
T053 done (commit 4aeaa3d) — context-aware Scope Id picker
T054 done (commit ef02268) — EntrustmentScale admin CRUD
        ↓
T056.a  done (Option A foundations + Institutions/Speciality/SubSpec cluster)
T056.b  done (EPAs + Curricula cluster)
T056.c  done (ActivityTypes + Forms cluster)
T056.d  pending (Trainees + Assessors + Invitations + EntrustmentScales cluster)
T056.e  pending (Audit + SSO + NavMenu refresh + scenario doc revert)
        ↓
T051    (invitation form: name capture + URL surface + dev SMTP tidy)
        ↓
T052    (re-expose Administrator role on invitation)
```

**Remaining order: T056.b → T056.c → T056.d → T056.e → T051 → T052.** T056 turned out to be too large to land in one session and is being landed in five clusters; each cluster ships a coherent feature group's scope guards plus tests. T051/T052 stay queued behind T056.e because they touch invitation surfaces that T056.d will already have modified.

## Documented compromises (no code, doc only)

- **`/admin/invitations/new` does not exist** — the embedded-in-list form is the intentional pattern across admin lists (mirrors `/admin/curricula/{id}/items`'s "Add item" form). Doc updated to reflect this; no code change.
- **Field-type label `Long text` (with space)** — matches the project's display convention (`Multi-choice`, `Procedure reference`). Doc updated; no code change.
- **Activity-type list status across 3 columns (`Published` / `Draft` / `Status`)** — per-column breakdown is more readable than concatenation. Doc updated; no code change.

## Estimated total

| Task | Effort | Status |
|---|---|---|
| T050 | 1h | ✅ done (commit `96104a1`) |
| T053 | 2h | ✅ done (commit `4aeaa3d`) |
| T054 | 6–8h | ✅ done (commit `ef02268`) |
| T055 | 1h | ✅ done (commit `6eaef56`) |
| T056.a | ~3h | ✅ done (commit `41def8a` — foundations + Institutions/Speciality/SubSpec cluster; 14 handlers + 14 pages + 9 tests) |
| T056.b | ~3h | ✅ done (commit `9e3bc0a` — EPAs + Curricula cluster; 13 handlers + 8 pages + 10 tests) |
| T056.c | ~3h | ✅ done (ActivityTypes + Forms cluster; 13 handlers + 5 pages + 10 tests) |
| T056.d | ~3–4h | ⏳ pending (Trainees + Assessors + Invitations + EntrustmentScales cluster) |
| T056.e | ~3–4h | ⏳ pending (Audit + SSO + NavMenu refresh + scenario doc revert) |
| T051 | 3h | ⏳ pending (after T056.e) |
| T052 | 3h | ⏳ pending (after T056.e) |
| **Total remaining** | **~18–22h** | T056.b–e dominates |

## What this plan does *not* cover

- **Acts 2–5** are not drafted yet, so this audit only validates Act 1. Drafting Act 2 (the team-onboarding act) will exercise the invitation flow more heavily and may surface issues T051/T052 don't anticipate.
- **Persistence / validation / cross-page-flow correctness** of Act 1 itself — the Playwright audit only checks routes and form-field presence. A human play-through after T050 lands will catch what static inspection cannot.
- **The standing operational-deployment backlog item** carried from T016 — unrelated to this audit.

## Suggested handoff after this plan lands

If all six tasks ship, the Paediatrics scenario Act 1 is playable end-to-end against a clean dev install in ~2.5 hours of clicking, and Acts 2–5 can be drafted with confidence in the same step-format / gap-finding model.

**As of 2026-05-24:** T050, T053, T054, T055 shipped; Step 1.7 reverted to canonical create-scale prescription. T056.a shipped (foundations + Institutions/Speciality/SubSpec cluster). Mbatha can now edit her own institution + manage specialities + sub-specialities, but the remaining admin pages (EPAs, Curricula, ActivityTypes, Forms, Trainees, Assessors, Invitations, EntrustmentScales, Audit, SSO) still require Administrator. T056.b–e will progressively close the remaining gaps. Phases 1.C–1.F currently run under the bootstrap admin as a documented workaround inside the scenario doc.
