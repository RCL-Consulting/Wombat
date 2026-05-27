# T060 — Invitation validator: allow Coordinator + external CommitteeMember with null Speciality

`InvitationRules.ValidateScope` (`src/Wombat.Application/Features/Invitations/InvitationRules.cs:41`) rejects any `Coordinator` / `CommitteeMember` / `SpecialityAdmin` invitation that lacks `SpecialityId`. The Paediatrics scenario Act 2 play-through (2026-05-27, Finding **A2-1**) needs:

- **Coordinator** with no speciality — a department-wide programme coordinator who supports trainees across multiple specialities.
- **CommitteeMember** with no speciality — an external examiner (e.g. Dr van Rensburg from Stellenbosch) who sits on the panel for THIS programme but is not tied to a local speciality.

`SpecialityAdmin` should keep the speciality requirement — the role is, by definition, an administrator of a specific speciality.

## Change

`src/Wombat.Application/Features/Invitations/InvitationRules.cs` — split the combined rule into two narrower rules:

```csharp
return targetRole switch
{
    WombatRoles.InstitutionalAdmin when specialityId.HasValue || subSpecialityId.HasValue
        => "Institutional administrators may only be scoped to an institution.",
    WombatRoles.SpecialityAdmin when !specialityId.HasValue
        => "Speciality administrators must be scoped to a speciality.",
    WombatRoles.SpecialityAdmin or WombatRoles.Coordinator or WombatRoles.CommitteeMember when subSpecialityId.HasValue
        => "The selected role may not be scoped to a sub-speciality.",
    WombatRoles.SubSpecialityAdmin or WombatRoles.Assessor or WombatRoles.Trainee when !specialityId.HasValue || !subSpecialityId.HasValue
        => "The selected role requires speciality and sub-speciality scope.",
    _ => null
};
```

After the change:
- Coordinator with `(institution)`, no speciality → accepted.
- Coordinator with `(institution, speciality)` → accepted.
- CommitteeMember with `(institution)`, no speciality → accepted.
- CommitteeMember with `(institution, speciality)` → accepted.
- SpecialityAdmin without speciality → still rejected with a more specific message.
- All three: sub-speciality still forbidden.

## Tests

`tests/Wombat.Application.Tests/Features/Invitations/InvitationRulesTests.cs` (or equivalent file in the existing Invitations test folder — locate first):

- New: Coordinator with null speciality → returns null (accepted).
- New: CommitteeMember with null speciality → returns null.
- Existing: SpecialityAdmin with null speciality → still rejected (update assertion if the message changes).
- Existing: Coordinator with sub-speciality → still rejected.

If no `InvitationRulesTests.cs` exists yet, add one. The class is internal-static so test access may need `[InternalsVisibleTo]` (check first — likely already configured given the pattern elsewhere).

## Browser verification

Sign in as Mbatha (InstitutionalAdmin). Issue:
1. Invitation `coord-test@kgk.wombat.local`, role `Coordinator`, institution KGK, speciality blank, sub-speciality blank → expect success.
2. Invitation `ext-test@kgk.wombat.local`, role `CommitteeMember`, institution KGK, speciality blank → expect success.
3. Invitation `sa-test@kgk.wombat.local`, role `SpecialityAdmin`, institution KGK, speciality blank → expect "Speciality administrators must be scoped to a speciality."

Revoke all three after verification.

## Scenario doc

Update Step 2.1 and Step 2.2's `Actual:` / `Gap:` lines once shipped, marking A2-1 closed and reverting the speciality assignments back to the original "leave blank" prescription.

## Out of scope

- Re-exposing the `Administrator` role on the invitation form. That's T052 — separate task.
- First/Last name capture on the invitation form. That's T051.b.
- Loosening sub-speciality nullability for any role. The current rule (Assessor/Trainee/SubSpecialityAdmin require both speciality and sub-speciality) is correct.

## Definition of done

- Build clean, all suites pass.
- New tests cover the Coordinator and CommitteeMember null-speciality cases.
- Browser-verified the three cases above.
- Scenario doc Act 2 updated.

## Files touched

- `src/Wombat.Application/Features/Invitations/InvitationRules.cs`
- `tests/Wombat.Application.Tests/Features/Invitations/*` (new or extended test class)
- `Rewrite/scenario-paediatrics.md` (Step 2.1 + 2.2 Actual/Gap revert)
- `Rewrite/Tasks/T060-invitation-validator-scope-relaxation.md` (this file)
- `Rewrite/current_state.md`

## Estimate

~30 minutes. **Sonnet.**
