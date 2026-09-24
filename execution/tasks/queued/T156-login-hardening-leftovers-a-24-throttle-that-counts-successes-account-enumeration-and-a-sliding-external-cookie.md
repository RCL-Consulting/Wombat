---
id: T156
title: Login hardening leftovers: a /24 throttle that counts successes, account enumeration, and a sliding external cookie
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T156 — Login hardening leftovers: a /24 throttle that counts successes, account enumeration, and a sliding external cookie


> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Each item is small; together they are the rest of the T149 review's section (b).
**Surfaced:** 2026-09-24, T149 adversarial review. None was introduced by T149.

## Items

1. **The login throttle is keyed on a /24 and counts successes.** `LoginRateLimitPolicy` (`Program.cs`) partitions on
   the client's /24, with a fixed window of 10 requests per 5 minutes. A hospital NAT or a carrier's shared /24 gets 10
   sign-ins per 5 minutes between everyone on it, and anyone can use them up. T149 put the SSO link endpoint on the same
   budget. Key on the full address, and count failures only.
2. **Forwarded headers are trusted from any peer.** `ForwardedHeaders` accepts X-Forwarded-For from anyone; it is safe
   only because Kestrel binds loopback behind Caddy. Set `KnownProxies` to the loopback addresses.
3. **Account enumeration.** The local login answers "This account uses institutional sign-in" for an SSO-only address
   and "Invalid email or password" for an unknown one. First-login provisioning shows Identity's "Username 'x' is
   already taken." when the email has an account at another institution.
4. **The external cookie slides.** `IdentityConstants.ExternalScheme` uses sliding expiry, so an IdP sign-in can be
   kept alive past its revocation at the provider. Set `SlidingExpiration = false`, and consider a `__Host-` prefix,
   since other `rcl.co.za` subdomains could plant cookies.
5. **Erasure is not transactional.** Each `UserManager` call saves on its own, so a failure midway leaves a
   half-erased account.

## Verification

- [ ] Each item fixed has a test, or a browser check recorded here.

## Related

T149, T097 (login throttling), T027 (SSO), T026 (erasure).
