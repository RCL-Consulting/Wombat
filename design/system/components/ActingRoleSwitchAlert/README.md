# ActingRoleSwitchAlert

What a switch of the acting role did, said once under the header of the page it landed on: "You are now acting as Assessor.", an info alert in an ActionResult that takes the focus, with "Switch back to {role}" away from Home.

New in flow 01 (T335, b347e11c). Source: `Components/Shared/ActingRoleSwitchAlert.razor`; the switch is `Navigation/ActingRoleSwitch.cs`.

## What the consumer provides

Nothing: PageHeader renders it, so every page with a header can say it. It reads the one-time word a switch left (a short-lived, protected, HttpOnly cookie that `App.razor` takes and `Routes` cascades) and shows only when that word's new role is the acting role.

## Markup

```html
<div class="action-result" tabindex="-1" autofocus>
  <div class="alert alert-info" role="status">
    <div class="alert-row">
      <span class="alert-row-text">You are now acting as Assessor.</span>
      <a class="btn btn-outline btn-sm" href="/dashboard/switch/CommitteeMember?returnUrl=…" data-enhance-nav="false">Switch back to Committee member</a>
    </div>
  </div>
</div>
```

## Rules (DESIGN.md § Acting role)

- **A person acts in one role at a time.** The acting role chooses only what the frame shows (the navigation, Home's dashboard, the head); access is the union of the roles held and never changes with it.
- **The switch is one GET address**, `/dashboard/switch/{role}?returnUrl=<local path>` (W-010), so an email's link can switch too. A role the person does not hold is ignored; a return address that is not a path on this site lands on Home.
- **Switch back** returns to this same page, path and query, in the role the person was acting as. Not on Home: a switch with no return address lands there, and the sidebar's own switch is the way back.
- **Every switch link is a full page load** (`data-enhance-nav="false"`).
- **Said once, focused.** Its ActionResult takes the focus as the page loads (`autofocus`, and a focus call once the circuit renders), ahead of the h1 that `FocusOnNavigate` focuses. A resumed circuit does not say it again.
- The role is named by its sentence-case label (`WombatRoleLabels`): "Committee member", never "CommitteeMember".

## Contrast

Words 11.84:1 on `info-bg`; its edge and icon 4.55:1; Switch back, filled with the surface, 4.86:1.
