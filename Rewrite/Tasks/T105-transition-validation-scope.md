# T105 — Every transition validates the whole schema in Submit mode, so a half-filled draft cannot be cancelled

**Status:** open
**Surfaced:** 2026-09-17, mapping the transition pipeline for T070.
**Severity:** Medium — a trainee can create a draft they can never dispose of, and the workaround shaped
the CPSA seeds in a way that hides the assessor's required fields from the validator.

## Symptom

A trainee starts a CPSA Mini-CEX, fills in two of the six required request fields, saves, and then wants to
abandon it. Pressing **Cancel** throws a validation error demanding every required field. The only way out
of a partly-filled draft is to complete it.

The same applies to an assessor's **Decline**: `decline` re-validates the entire schema, including fields
the assessor has no business filling in.

## Root cause

`ActivityService.cs:133-137` runs one validation call for **every** transition key:

```csharp
_schemaValidator.Validate(schema, mergedDataJson, SchemaValidationMode.Submit, transition.RequiresFields);
```

`SchemaValidationMode.Submit` is unconditional — there is no exemption for `cancel`, `decline` or any other
disposal transition. `SchemaValidator.cs:30-41` then walks every section and field of the whole schema
(skipping only those failing `show_if`), and `:58-59` computes
`isRequired = (mode == Submit && field.Required) || additionallyRequiredFieldKeys.Contains(field.Key)`.

Two things follow that are worth stating plainly, because both were assumed otherwise:

- **`requires_fields` widens validation; it never narrows it.** It is folded into the same full-schema
  Submit pass as an extra required-key set. It is *not* a scope.
- **`requires_fields` has zero effect in `WorkflowEvaluator`.** The evaluator
  (`WorkflowEvaluator.cs:13-37`) never reads the property; its only runtime consumer is the validator call
  above.

By contrast the draft path uses `SchemaValidationMode.Draft` (`ActivityService.cs:97` and `:46`).

## Knock-on effect already baked into the seeds

`CpsaWbaSeedTests.cs:150-163` records the workaround verbatim: the four assessor fields
(`overall_level`, `strengths`, `improvements`, `plan`) deliberately declare **no** `required: true`, because
a required assessor field would also block `decline` and `cancel`. They are gated by `requires_fields` on
`complete` instead.

That is a sound response to the constraint, but it means the schema itself no longer says which fields are
mandatory — the workflow does. The older seeds did not know this: `mini_cex/schema.json` marks **14 of 14**
fields `required: true`, which is why its `accept`, `decline` and `cancel` transitions are unusable today.

## Desired behaviour

A transition should declare its validation scope. Sketch, to be decided:

- Disposal transitions (those reaching a state with no outgoing transitions, or an explicit
  `validation: "none" | "draft" | "submit"` property on the transition) validate in **Draft** mode or skip
  validation entirely.
- Progress transitions keep Submit mode.
- With that in place, assessor fields can carry honest `required: true` flags again and `requires_fields`
  goes back to meaning "additionally required for this step".

This is a workflow-DSL addition, so it wants the same treatment as T070's `editable_by`: parse **and**
serialise (`ActivityType.SaveDraft` round-trips through both, so a missing `Serialize` half is dropped
silently at publish), a builder UI affordance, and a round-trip test.

## Verification

- A trainee creates a CPSA Mini-CEX with only one field filled and cancels it successfully.
- An assessor declines a submitted activity with a note and no ratings.
- `complete` still enforces every required request field plus the four assessor fields.
- The legacy `mini_cex` type's `accept` / `decline` / `cancel` become usable.

## Related

Shapes how [T070] declares field ownership, and how the ten remaining v11.1 tools should be authored —
worth deciding before those seeds are written. Same DSL surface as T103's round-trip guard.
