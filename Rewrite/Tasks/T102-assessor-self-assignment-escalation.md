# T102 — A trainee can name themselves as their own assessor and self-award entrustment credit

**Status:** open
**Surfaced:** 2026-09-17, adversarial review of the T070 plan.
**Severity:** High (integrity of the assessment record) — defeats the formative/summative separation T031
established and the "never from a single form" principle v11.1 states explicitly.

## Symptom

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

- A trainee who selects themselves in the assessor picker (or posts their own id directly) is rejected at
  create with a validation message, not at transition time.
- After fix 2: posting the id of a user who is not an Assessor, or is outside the caller's institution, is
  rejected server-side even though the picker never offered them.
- The legitimate path is unaffected: trainee names a real assessor, assessor completes, credit applies.

## Related

T070 (makes the field load-bearing for write permission), T101 (same family — activity authorization).
