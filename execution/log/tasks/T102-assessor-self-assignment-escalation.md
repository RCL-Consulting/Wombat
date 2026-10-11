---
id: T102
title: "A `user`-typed field accepts any user id, unchecked for role or scope"
status: done
priority: P1
created: 2026-09-17
started: 2026-09-24
completed: 2026-09-24
---
# T102 — A `user`-typed field accepts any user id, unchecked for role or scope

**Status:** open — **fix 1 shipped 2026-09-17 with T070**; fixes 2 and 3 remain. See Progress at the foot.
**Retitled:** 2026-09-20. The original title — *"A trainee can name themselves as their own assessor
and self-award entrustment credit"* — described the path `ThrowIfActorFieldNamesSubject` now closes.
The filename keeps the old slug on purpose: `T102` is the durable handle, and `EPA-PROGRAMME.md:571`
cites the path.
**Surfaced:** 2026-09-17, adversarial review of the T070 plan.
**Severity:** High (integrity of the assessment record) — defeats the formative/summative separation T031
established and the "never from a single form" principle v11.1 states explicitly.

## Symptom

> **The self-naming half of this is closed.** `ActivityService.ThrowIfActorFieldNamesSubject` (T070,
> `c33c14b`) rejects, at create and on the patch merge, any data in which a `field:` actor rule resolves
> to the activity's own `SubjectUserId`. What remains is the *other* user id: a trainee can still name
> an arbitrary person the picker never offered, who holds no `Assessor` role and may sit in another
> institution. The account below is the original framing, kept because gaps 2 and 3 are unchanged.

On a CPSA WBA the trainee fills in `assessor_user_id` at creation. Nothing validates that value. A trainee
who submits **their own** user id becomes the activity's bound assessor, satisfies
`actor: "field:assessor_user_id"`, can take the `complete` transition themselves, and `CreditApplier`
awards the curriculum credit — with `overall_level` set to whatever they chose.

## Root cause — three gaps that line up

1. **No server-side membership check on `user`-typed fields.** `SchemaValidator.cs:77` routes
   `FieldType.User` to `ValidateStringField` (`:117-123`) — length and regex only. The options-membership
   branch at `SchemaValidator.cs:160-163` is inert because `user` fields carry no inline `options`.
2. **The picker is the only restriction, and it is client-side.**
   `ActivityReferenceDataService.GetAssessorOptionsAsync` (`:104-127`) emits users in the `Assessor` role,
   institution-filtered — but that shapes the dropdown; it never re-validates the submitted value.
3. **The actor rule trusts the data.** `WorkflowEvaluator.cs:47` resolves `field:assessor_user_id` by
   string-comparing `DataJson["assessor_user_id"]` to the caller's `ClaimTypes.NameIdentifier`
   (`:78-82`, `:84-107`). Whoever's id is in that field *is* the assessor, by definition.

Nothing in the chain asks whether that person holds the `Assessor` role, is a different human from the
subject, or is even in the same institution.

## Why it matters more after T070

T070 makes field write-ownership **data-driven** off the same `assessor_user_id` field: sections marked
`editable_by: "field:assessor_user_id"` become writable by whoever that field names. A self-named trainee
would then also inherit write access to the entrustment and feedback sections by design rather than by
oversight. The failure mode is unchanged in kind but larger in blast radius, and the seeds harden around it.

## Fix — layered, cheapest first

1. **Reject `assessor_user_id` equal to `SubjectUserId`** in `ActivityService.CreateDraftAsync` and on the
   patch merge. One comparison, closes the self-assessment case outright.
2. **Validate `user`-typed field values server-side** against the same query that builds the picker — the
   submitted id must resolve to a user the caller may legitimately nominate (role + scope). This is the real
   fix; it generalises to any admin-built tool with a `user` field, which is the platform premise.
3. Consider whether a `user` field should declare its required role in the schema DSL (a `role` property),
   so the builder can express "pick a supervisor" vs "pick a peer" and the validator has something to check
   against. This is a DSL addition — decide alongside T070's `editable_by`.

## Decision required

Is fix 1 enough for now, or does 2 land with T070? Recommendation: **1 immediately** (a few lines, removes
the headline abuse), **2 as its own task** before the remaining ten v11.1 tools are seeded, since every one
of them will carry a `user` field.

## Verification

- [x] **A trainee who names themself is rejected at create**, with fix 1's message (T070;
  `Create_SubjectNamesThemselfAsTheAssessor_IsRejected`, and fix 1 still speaks first — paths-and-regressions tests).
- [x] **After fix 2, a user the picker never offered is rejected server-side.** Changed from "outside the caller's
  institution": the institution judged is the **activity's stamped one (the subject's)**, never the caller's, with no
  Administrator bypass (see As built). Evidence: `NomineeGateTests` (37 cases: each eligibility condition failing alone,
  mutation-pinned, M01–M16 all killed); in the browser on dev, a forged option value on `/activities/new` was refused at
  create with "that person cannot be named here" and no draft was left behind.
- [x] **The legitimate path is unaffected.** Browser, dev, activity 16: the trainee named Demo Assessor, saved; an
  admin removed his Assessor role; the trainee's unchanged submit was **refused by name** (the hand-on); re-picking Demo
  Committee passed; Committee then lost the Assessor role too and **still completed it, crediting 1 item** — an unchanged
  nominee is never re-judged on the assessor's own move. Roles restored afterwards.
- [x] **Fix 3:** `role` parses, normalises, serialises, round-trips through `SeedRoundTripTests` and the builder
  (`BuilderNomineeRoleTests`), and is refused on a non-user field or a non-nominable role. The builder's "Names a"
  select shows for a User field only (browser, Mini-CEX (Paediatrics) Form tab).
- [x] **Picker = gate.** `NomineePickerGateParityTests`: over a 13-user matrix, every listed id is accepted and every
  unlisted one refused, on create and on a re-pick. `NomineeDirectoryPostgresTests` (6, real PostgreSQL) pin the SQL,
  the role conjunction, ordinal ids and the lockout threshold (`DateTimeOffset.MaxValue` round-trips as `infinity`).

## Related

T070 (makes the field load-bearing for write permission), T101 (same family — activity authorization).

---

## Progress — 2026-09-17: fix 1 landed with T070

Fix 1 (the narrow guard) shipped as part of T070, because T070 makes `assessor_user_id` decide **write
ownership** as well as transition rights — a reviewer demonstrated the complete path: subject names
themself in `draft`, matches `field:assessor_user_id` from `requested` on, rates themself, takes their own
`complete`, and `CreditApplier` awards the credit.

`ActivityService.ThrowIfActorFieldNamesSubject` now refuses — at create **and** on the patch merge — any
data in which a field referenced by a `field:` actor rule (anywhere in the workflow's transitions or state
`editable_by`, or the schema's section/field `editable_by`) resolves to the activity's `SubjectUserId`.
It is driven off the parsed rules rather than a hard-coded field name, so it covers admin-built types too.

Tests: `Create_SubjectNamesThemselfAsTheAssessor_IsRejected` and
`Transition_SubjectRetargetsTheAssessorFieldToThemself_IsRejected`, both verified to fail against the
pre-fix code.

**Fixes 2 and 3 remain open and this task stays open.** Nothing yet validates that a submitted
`user`-typed value names someone who actually holds the required role, or who is inside the caller's
scope — `SchemaValidator` still routes `FieldType.User` to plain string validation. A trainee can still
nominate an arbitrary user id that the picker never offered; they simply cannot nominate *themself*.
Decide this before the remaining ten v11.1 tools are seeded, since every one carries a `user` field.

---

## As built — 2026-09-24: fixes 2 and 3

**The rule.** A *nominee field* is every `user` field plus every field a `field:` rule names (`ActorFieldRules`, the
one walker of declared actor rules). Its value must be the exact id of a user who holds every role the field requires
(`role`, default `Assessor`), belongs to the **activity's stamped institution**, is **not deactivated** (an admin lock
or an erasure; a brute-force lockout does not count — `UserDeactivation`), and is not the subject. A **changed** value
is judged on every write, whoever makes it, including a withdrawal (a `field:` value grants read, inbox and nudges in
every state). An **unchanged** value is judged only at the author's hand-on — the D20 clause, factored out of T122's
`DirectivesToJudge` into `UnchangedFieldsHandedOn` and shared. `NomineeGate` runs after the T122 gate and before the
first mutation, on create, transition and the staged MSF path.

**Picker = gate.** `NomineeDirectory` is the one query; `GetNomineeOptionsAsync` replaced `GetAssessorOptionsAsync`.
A writable user field gets the list; a locked one gets only the stored person's label (`GetUserOptionAsync`). Only a
value **stored** on the activity is ever labelled (`ActivityForm.StoredDataJson`), never the working copy.

**Publish checks** (`ActorFieldRules.EnsurePublishable`, in `SaveDraft` and `PublishDraft`, not the parser): duplicate
section or field keys, a `field:` rule naming no field or a non-user field, and `options`/`catalogue` on a user field.

**Deleted:** `UpdateActivityDraftCommand`, its input and `IActivityService.UpdateDraftAsync` — no caller, and it skipped
fix 1, T070's writable filter and the state gate. T106 item 1 and T127 are annotated.

**Also:** the nudge job reads the pinned version; `SubjectScopeResolver` is shared by the stamp and the create-page
picker; the builder proposes schema-wide unique default keys; busy guards on NewActivity and ActivityView.

**Review history.** Design: a 6-reader map, then a 5-lens critique (24 findings, 21 upheld) that added the hand-on
clause, "deactivated" instead of "locked out", the publish checks and read-only labels. Implementation review round 1
(6 lenses, 3 refuters each) found the working-copy label leak, a Release-build break, a remount DbContext race and
stale docs, all fixed. 274 tests from six worktree agents, each area mutation-checked.

**Filed:** [T149] (P1) SSO link endpoint is an unthrottled password oracle and SSO ignores lockout; [T150] sampling
counts assessors on drafts and cancels; [T151] nudges reach deactivated and opted-out nominees; [T152] a supervisor from
another institution cannot be named; [T153] a trainee who has left an institution still files there and sees its staff.

**Round 2** (6 lenses, 3 refuters each; 6 minor upheld, none major): a refused, unlisted pick showed as "Select…"
while the page kept sending it (now a neutral local option); `SpecialityAdmin`/`SubSpecialityAdmin` as a `role` were
checked only against the institution (removed from the nominable set); one vacuous test; no tests for the busy guards,
the per-load scope or the stale-load discard; and [T153].

**Round 3** (3 lenses, 3 refuters each) came back with no code defect: two findings, both the same wording error (the
nominable roles were called "institution-scoped", false for Assessor and Trainee, whose invitations carry a
speciality). Reworded in `WombatRoles.Nominable`, CUSTOMIZATION.md and D23: only the institution is matched, never the
nominee's speciality, so cross-discipline naming within an institution is accepted by design. The review loop is dry.
