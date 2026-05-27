# T063 — Decision panel authorization: widen handler + reconcile page authorize ✅ SHIPPED 2026-05-27

Two Act 2 findings on the Decision Panel surface:

- **A2-9** — Mbatha (`InstitutionalAdmin`) cannot create or manage Decision Panels. `CommitteeDecisionAuthorization.DemandPanelAdministration` (`src/Wombat.Application/Features/CommitteeDecisions/CommitteeDecisionAuthorization.cs:14`) only accepts `Administrator` / `SpecialityAdmin` / `SubSpecialityAdmin`. T056's institutional-admin sweep missed this surface.
- **A2-11** — `PanelEdit.razor`'s `[Authorize(Roles = "Coordinator,Administrator,SpecialityAdmin,SubSpecialityAdmin")]` lets a Coordinator into the page; the handler then rejects the save with "You are not allowed to manage committee panels." The two authorization sites are not in sync.

Both are small fixes that should be shipped together.

## Change

### Application layer

`src/Wombat.Application/Features/CommitteeDecisions/CommitteeDecisionAuthorization.cs`:

```csharp
public static void DemandPanelAdministration(ClaimsPrincipal principal)
{
    if (principal.IsInRole(WombatRoles.Administrator) ||
        principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
        principal.IsInRole(WombatRoles.SpecialityAdmin) ||
        principal.IsInRole(WombatRoles.SubSpecialityAdmin))
    {
        return;
    }

    throw new UnauthorizedAccessException("You are not allowed to manage committee panels.");
}
```

Then add a scope check for InstitutionalAdmin in each handler that calls `DemandPanelAdministration`:

```csharp
DemandPanelAdministration(principal);
if (principal.IsInstitutionalAdmin() && !principal.CanAccessInstitution(panel.InstitutionId))
{
    throw new UnauthorizedAccessException("You can only manage panels in your institution.");
}
```

Apply to:
- `CreateDecisionPanelCommandHandler` (verify the panel's resolved institution matches caller's institution).
- `UpdateDecisionPanelCommandHandler`.
- `DeleteDecisionPanelCommandHandler` (if exists).
- `GetDecisionPanelByIdQueryHandler` — return null on out-of-scope to avoid leaking other-institution ids (T056 pattern).
- `ListDecisionPanelsQueryHandler` — filter to own institution when caller is InstitutionalAdmin.

For `DecisionPanelScope.Speciality` panels, also verify the speciality belongs to the caller's institution.

### Web layer

`src/Wombat.Web/Components/Pages/CommitteeDecisions/PanelEdit.razor`:

Drop `Coordinator` from the page authorize roles and add `InstitutionalAdmin`:

```razor
@attribute [Authorize(Roles = "InstitutionalAdmin,Administrator,SpecialityAdmin,SubSpecialityAdmin")]
```

Same change on `PanelsList.razor` if the list page authorize is also Coordinator-inclusive (check).

Coordinator's actual privilege is `DemandReviewScheduling`, not `DemandPanelAdministration`. The page authorize should match the handler so the operator gets an immediate `/access-denied` instead of a misleading "Save" that fails.

If Coordinator should still be able to *see* panels for reference (read-only view of the list), keep them on `PanelsList.razor` but not on `PanelEdit.razor`.

## Tests

- New `Features/CommitteeDecisions/PanelScopeGuardTests.cs`:
  - InstitutionalAdmin from KGK creating a KGK panel → succeeds.
  - InstitutionalAdmin from KGK creating a panel scoped to Demo Institution → `UnauthorizedAccessException`.
  - InstitutionalAdmin updating own institution's panel → succeeds.
  - InstitutionalAdmin updating other institution's panel → exception.
  - Coordinator calling `DemandPanelAdministration` directly → still exception (no behaviour change).
- Update any existing committee tests that previously assumed Coordinator could administer panels.

## Browser verification

Sign in as Mbatha (InstitutionalAdmin):
- Navigate to `/committee/panels` → can see and edit KGK panel (id=1 from Act 2).
- Visit `/committee/panels/new`, fill the form with KGK Institution + Paediatrics Speciality, save → success.
- Attempt to load `/committee/panels/{otherInstId}` (some seeded panel from elsewhere — may need to seed one) → 404.

Sign in as Smit (Coordinator):
- Visit `/committee/panels/new` → redirected to `/access-denied`.
- Visit `/committee/panels` → can see panels (read-only) if PanelsList retains Coordinator; otherwise also `/access-denied`.

Sign in as Administrator: unchanged — full access to all institutions.

## Scenario doc

Mark A2-9 + A2-11 closed in the Act 2 findings summary. Update Step 2.8's `Actual:` to describe the corrected flow (Mbatha runs the panel creation; bootstrap admin substitution no longer needed).

## Out of scope

- Adding scope guards to the broader committee surface (review scheduling, decision recording, appeals). Those are governed by `DemandReviewScheduling` / `DemandPanelAccess` / `DemandChairAccess` etc. which appear correct; double-check during T063 but defer changes to a separate task if anything's off.
- Coordinator-as-panel-administrator. Coordinator role is operational support, not panel governance — keep the privilege boundary where it is.

## Definition of done

- Build clean, all suites pass.
- Scope-guard tests cover the four institutional-admin cases.
- Browser-verified as Mbatha, Smit, and Administrator.
- Scenario doc updated.

## Files touched

- `src/Wombat.Application/Features/CommitteeDecisions/CommitteeDecisionAuthorization.cs`
- `src/Wombat.Application/Features/CommitteeDecisions/CreateDecisionPanel.cs`
- `src/Wombat.Application/Features/CommitteeDecisions/UpdateDecisionPanel.cs`
- `src/Wombat.Application/Features/CommitteeDecisions/GetDecisionPanelById.cs`
- `src/Wombat.Application/Features/CommitteeDecisions/ListDecisionPanels.cs` (if it exists)
- `src/Wombat.Web/Components/Pages/CommitteeDecisions/PanelEdit.razor`
- `src/Wombat.Web/Components/Pages/CommitteeDecisions/PanelsList.razor`
- `tests/Wombat.Application.Tests/Features/CommitteeDecisions/PanelScopeGuardTests.cs` (new)
- `Rewrite/Tasks/T063-panel-authorization-reconciliation.md` (this file)
- `Rewrite/scenario-paediatrics.md`
- `Rewrite/current_state.md`

## Estimate

~1 hour. **Sonnet.**
