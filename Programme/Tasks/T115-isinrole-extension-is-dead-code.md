# T115 — `ClaimsPrincipalExtensions.IsInRole` never executes, and every `role:` rule depends on which one runs

**Status:** open
**Surfaced:** 2026-09-19, adversarial review during T101.
**Severity:** Low today, Medium the day T027's OIDC path is switched on.

## Symptom

`src/Wombat.Application/Common/Extensions/ClaimsPrincipalExtensions.cs` defines:

```csharp
public static bool IsInRole(this ClaimsPrincipal principal, string role)
    => principal.Claims.Any(claim =>
        claim.Type == ClaimTypes.Role &&
        string.Equals(claim.Value, role, StringComparison.Ordinal));
```

**This method never runs in instance-call position.** C# considers extension methods only when no
applicable instance method exists, and `ClaimsPrincipal.IsInRole(string)` is applicable. So every
`principal.IsInRole(...)` call site in the repo — `IsAdministrator()`, `IsCollegeAdmin()`,
`IsInstitutionalAdmin()`, `ActorRuleMatcher`, `ExportPortfolio`, and everything T101 added — binds to the
**BCL** method. `grep -rn "ClaimsPrincipalExtensions\."` finds no static call site either.

## Why it matters

The two differ in one respect that is currently invisible. The extension hard-codes `ClaimTypes.Role`;
the BCL method resolves against `ClaimsIdentity.RoleClaimType`, which is configurable per identity.

Today they agree: `WombatUserClaimsPrincipalFactory` adds no role claims of its own, the test helpers use
`ClaimTypes.Role`, and the default `RoleClaimType` matches. **T027's OIDC path is exactly where a
non-default `RoleClaimType` appears** — an IdP that emits `roles` or a namespaced claim type. At that
point the dead extension would have behaved differently from the live BCL call, and the authorization
surface would shift under a change nobody associated with roles.

## Why it is worth fixing now rather than then

The whole `role:` half of the actor grammar and all of T101's scoped-oversight arms rest on this
resolution. A security boundary should not depend on an overload-resolution subtlety that no comment in
the repo mentions and that at least one reviewer has already misread.

## Options

1. **Delete the extension.** Simplest. Everything already uses the BCL method; deleting it removes the
   ambiguity and makes the `RoleClaimType` dependency explicit and greppable.
2. **Rename it** (e.g. `HasRoleClaim`) so it is callable and distinct, and use it deliberately wherever an
   ordinal `ClaimTypes.Role` match is what is actually wanted.

Prefer 1 unless a call site genuinely needs the hard-coded behaviour. Whichever is chosen, add a line to
the T027 SSO notes recording that `RoleClaimType` must map to `ClaimTypes.Role`, or roles silently stop
matching.

## Related

Surfaced by the [T101] review panel. Touches [T027] (OIDC) when that is activated.
