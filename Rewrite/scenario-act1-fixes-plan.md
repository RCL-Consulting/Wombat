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

### T051 — Invitation form: capture First/Last name
Add `FirstName` + `LastName` columns (nullable text) to `Invitation`. Surface in the `InvitationsList` issue form. Pre-fill the accept-invitation form when present. Migration + Designer file (per CLAUDE.md hand-written migration rules).

**Effort:** ~2 hours. Small. Independent of T052. Suggested model: **Sonnet**.

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

### T055 — ActivityType Edit: Publish button always visible
Show `Publish` even when `_editor.HasDraft == false`, with `disabled="true"` and a `title` tooltip "Save a draft to publish." Removes the magic-disappears-after-publish surprise that Step 1.11.e's wording implied was unconditional.

**Effort:** ~15 minutes. Trivial. Could fold into T050 (just reword the step) if the team prefers doc fix over code fix. Suggested model: **Sonnet**.

## Sequencing and dependencies

```
T050 ──┐
       ├─→ scenario plays as-written (human play-through unblocked)
T051 ──┤
T052 ──┘
        ↓
T053   (UX win; independent of the above; can run in parallel)
        ↓
T054   (closes the only true feature gap)
        ↓
T055   (trivial; ship any time, or fold into T050)
```

Recommended order: **T050 → T055 (or fold) → T051 → T052 → T053 → T054.** T050 first regardless — costs an hour, unblocks the play-through, and lets a human run catch persistence/validation issues the audit can't see.

## Documented compromises (no code, doc only)

- **`/admin/invitations/new` does not exist** — the embedded-in-list form is the intentional pattern across admin lists (mirrors `/admin/curricula/{id}/items`'s "Add item" form). Doc updated to reflect this; no code change.
- **Field-type label `Long text` (with space)** — matches the project's display convention (`Multi-choice`, `Procedure reference`). Doc updated; no code change.
- **Activity-type list status across 3 columns (`Published` / `Draft` / `Status`)** — per-column breakdown is more readable than concatenation. Doc updated; no code change.

## Estimated total

| Task | Effort |
|---|---|
| T050 | 1h |
| T051 | 2h |
| T052 | 3h |
| T053 | 2h |
| T054 | 6–8h |
| T055 | 0.25h |
| **Total** | **~14–16h** across 5–6 commits |

## What this plan does *not* cover

- **Acts 2–5** are not drafted yet, so this audit only validates Act 1. Drafting Act 2 (the team-onboarding act) will exercise the invitation flow more heavily and may surface issues T051/T052 don't anticipate.
- **Persistence / validation / cross-page-flow correctness** of Act 1 itself — the Playwright audit only checks routes and form-field presence. A human play-through after T050 lands will catch what static inspection cannot.
- **The standing operational-deployment backlog item** carried from T016 — unrelated to this audit.

## Suggested handoff after this plan lands

If all six tasks ship, the Paediatrics scenario Act 1 is playable end-to-end against a clean dev install in ~2.5 hours of clicking, and Acts 2–5 can be drafted with confidence in the same step-format / gap-finding model. If only T050 ships, the scenario is playable with documented workarounds for the two structural gaps (invitation role, entrustment scale), which is the cheapest path to having a working runbook.
