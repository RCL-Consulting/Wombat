# T097 — Security posture hardening: make the documented controls real

## Status: COMPLETE (code) — production actions outstanding, see §6

## Why this exists

A full-repo audit on 2026-09-16 found that three security controls `CLAUDE.md` lists as
shipped did not exist in the code, and that credentials had leaked into tracked files.
The documentation asserted a posture the application did not have — the worst kind of
drift, because it stops anyone from looking.

Found (each verified by grep/curl against the running app before fixing):

| Claim in `CLAUDE.md` | Reality before T097 |
|---|---|
| "rate-limited login" | No rate limiter anywhere in `Wombat.Web`. The only one in the solution covers `MsfRespond` on the **Api** host, which is not the host serving `wombat.rcl.co.za`. |
| "CSP with nonce-backed `script-src`" | No CSP in the codebase at all. The only policy on the wire was the framework default `frame-ancestors 'self'`. |
| "`X-Content-Type-Options: nosniff`" | Not set by the app or by Caddy. Absent from every live response. |

Plus, not previously documented anywhere:

- `lockoutOnFailure: false` at `Program.cs` with **no `options.Lockout` block at all** — failed
  logins were audited but never counted, so no account could ever lock.
- The seeded production Administrator password in plaintext in tracked, pushed
  `Rewrite/current_state.md`, alongside a public admin username in `deploy/README.md`.
  Combined with the two items above, that is valid-credential replay against a login
  that cannot throttle or lock, on a publicly reachable form.
- A plaintext dev connection string committed at `src/Wombat.Web/appsettings.json`, which
  ships in the publish output and would silently override the documented startup fail-fast
  if the deployment env var were ever dropped.
- `deploy/wombat-backup.sh` wrote every dump to the same filesystem as the database. The
  off-host step existed only as a comment. Neither `wombat.env` (holding the explicitly
  non-rotatable `Wombat__PseudonymSalt`) nor the DataProtection key ring was backed up at all.
- `AddHealthChecks()` registered **zero** checks, so `/health` returned 200 while PostgreSQL
  was unreachable — and both the per-minute restart cron and the deploy gate trust it.
- Three published NuGet advisories failing `NuGetAudit` on a fresh restore.

## 1. Login throttling (two independent layers)

- `src/Wombat.Infrastructure/DependencyInjection.cs` — added an `options.Lockout` block:
  `AllowedForNewUsers = true`, `MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = 15 min`.
- `src/Wombat.Web/Program.cs` — login now passes `lockoutOnFailure: true`, and a new
  `result.IsLockedOut` branch audits `LoginLockedOut` while returning a message that does
  **not** confirm the account exists.
- Added `AddRateLimiter` with a fixed-window policy (10 attempts / 5 min, partitioned by
  truncated client IP) applied to `/account/login/submit` via `.RequireRateLimiting`.
  `UseRateLimiter` sits **before** `UseAntiforgery`, so attempts are counted even when they
  fail antiforgery — an attacker without a token is still throttled.

The two layers cover different attacks: Identity lockout stops a single account being
hammered; the IP limiter stops a password-spray spread thinly across many accounts, and
stops an attacker locking a real user out by burning their attempts for them.

## 2. Security headers + nonce-backed CSP

New `src/Wombat.Web/Security/SecurityHeadersMiddleware.cs`, registered immediately after
`UseForwardedHeaders` so static files and error responses carry the headers too.

Sets `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin`,
`X-Frame-Options: SAMEORIGIN`, and a CSP.

**Why the nonce is needed:** `<ImportMap />` renders an inline `<script type="importmap">`.
A plain `script-src 'self'` blocks it and takes Blazor's module loading down with it — which
is exactly why the original design called for a nonce. The middleware mints a fresh nonce per
request onto `HttpContext.Items`, and `App.razor` stamps it on the `ImportMap` tag.

`style-src` retains `'unsafe-inline'` because ~51 components carry `style="..."` attributes,
which CSP counts as inline styles. `script-src` — the directive that actually mitigates XSS —
takes no such exemption.

Note: ASP.NET Core appends its own `Content-Security-Policy: frame-ancestors 'self'` for Blazor
form handling, so **two** CSP headers appear on the wire. Browsers enforce multiple policies as
an intersection, and the framework's adds no restriction beyond ours, so this is harmless.

## 3. Credential hygiene

- Scrubbed every password literal from tracked docs (`current_state.md`,
  `act3-rebuild-scratch.md`, `scenario-paediatrics.md`), replacing each with a pointer to
  the gitignored `pwd_DO_NOT_COMMIT.txt`. **This removes them from the working tree only —
  they remain in git history.** See §6.
- Removed the plaintext dev connection string from `appsettings.json`, replacing it with a
  note explaining why a committed fallback is dangerous. Local dev already resolves it from
  `user-secrets`; the existing startup throw is now the real behaviour rather than a claim.
- `appsettings.Development.json` still carries `SeedAdminPassword` — **left deliberately**:
  it is a local-dev placeholder needed for seeding, and only binds under
  `ASPNETCORE_ENVIRONMENT=Development`. It must never be the value used in production.

## 4. Backups that survive the host

`deploy/wombat-backup.sh` rewritten to bundle the database dump **plus** `wombat.env` **plus**
the DataProtection key ring, encrypt the bundle with `age` to a recipient whose private key
lives on neither machine, and ship it off-host via `rclone` or `rsync`.

The critical behavioural change: if the off-host leg is **not configured, or `age` is missing,
the script exits non-zero** so cron mails the operator. It refuses to ship an unencrypted
bundle containing `wombat.env`. A silently-skipped off-host step is how this stayed broken
for months; it now fails loudly instead.

Configuration lives in `/etc/default/wombat-backup` (mode 600):
`WOMBAT_BACKUP_AGE_RECIPIENT`, and one of `WOMBAT_BACKUP_RCLONE_REMOTE` / `WOMBAT_BACKUP_REMOTE`.

## 5. Health check that can fail

`AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("database")` — `/health` now probes
PostgreSQL. Required a new `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore`
package reference (architecture tests re-run and green after the addition, per `CLAUDE.md`).

Also pinned three transitive packages with published advisories, which `NuGetAudit`
(warnings-as-errors in Release) surfaced on the fresh restore: `Microsoft.OpenApi` → 2.12.2
(GHSA-v5pm-xwqc-g5wc, high), `SSH.NET` → 2026.0.0 (GHSA-q939-rpr3-3284, high), `AngleSharp`
→ 1.8.1 (GHSA-pgww-w46g-26qg, moderate). Each pin stays inside its parent's major version.
Enabled `CentralPackageTransitivePinningEnabled` to make the pins effective.

## Verification

- `dotnet build Wombat.sln -c Release` → **0 warnings, 0 errors** (warnings-as-errors on).
- Domain **50**, Application **314**, Architecture **19**, Web **43** — all pass, 0 failed, 0 skipped.
- Ran the app locally and confirmed on the wire:
  - `X-Content-Type-Options`, `Referrer-Policy`, `X-Frame-Options` and the full CSP all present.
  - The CSP nonce in the response header **matches** the `nonce` attribute on the rendered
    `<script type="importmap">` tag — so the policy is enforceable without breaking Blazor.
  - 12 consecutive POSTs to `/account/login/submit`: attempts 1–10 pass through to antiforgery
    (400), attempt **11 trips the limiter** and redirects. Limiter counts pre-antiforgery as designed.

## 6. NOT done — requires the operator, on production

These are deliberately out of scope for a code change and must be done against the live host:

1. **Rotate the production Administrator password.** Treat the old one as disclosed. Scrubbing
   the working tree does not remove it from git history on the origin.
2. **Decide on git history.** Rotation makes a history rewrite optional. `origin` is a private
   self-hosted server (`rcl.co.za:10648`), not a public forge, which bounds the exposure — but
   note that host is a second single-operator box, in Dallas, that also relays all Wombat mail.
3. **Configure and test the off-host backup leg** — pick a destination, generate the `age`
   keypair, keep the private half off both machines, and rehearse a restore.
4. **Deploy.** None of the above code is live until `deploy/deploy.ps1` runs. The CSP and the
   rate limiter in particular should be smoke-tested on production immediately after.
5. Consider adding a pre-commit secret scanner (gitleaks/trufflehog) so §3 cannot recur.

## Follow-ups filed separately

- `Rewrite/INFRASTRUCTURE.md` is materially stale and actively misleading (env var names that
  would not bind, Postgres 16 vs the live 18.4, a `--migrate` step `deploy/README.md` forbids,
  a `REVOKE` that would re-break T096 archival). Not fixed here — needs its own task.
- `AllowedHosts: "*"`, and `ForwardedHeaders` trusting any proxy — both safe only because
  Kestrel binds loopback-only behind Caddy. Worth an explicit comment and a revisit if the
  bind address ever changes.
