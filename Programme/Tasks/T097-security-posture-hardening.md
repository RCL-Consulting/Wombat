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
  `Programme/current_state.md`, alongside a public admin username in `deploy/README.md`.
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

- `Programme/INFRASTRUCTURE.md` is materially stale and actively misleading (env var names that
  would not bind, Postgres 16 vs the live 18.4, a `--migrate` step `deploy/README.md` forbids,
  a `REVOKE` that would re-break T096 archival). Not fixed here — needs its own task.
- `AllowedHosts: "*"`, and `ForwardedHeaders` trusting any proxy — both safe only because
  Kestrel binds loopback-only behind Caddy. Worth an explicit comment and a revisit if the
  bind address ever changes.

---

## 7. Post-deploy addendum (same session) — fingerprinted static assets required auth

Browser verification after the first deploy surfaced a **pre-existing** defect, unrelated to
the CSP but found by looking properly for the first time.

`src/Wombat.Infrastructure/Identity/AuthorizationPolicies.cs:31` sets a `FallbackPolicy` of
`RequireAuthenticatedUser()`. Every endpoint inherits it — including the fingerprinted
static-asset endpoints registered by `app.MapStaticAssets()`. So for an anonymous visitor:

- `/Components/Layout/ReconnectModal.abdmv1u4y3.razor.js` → **302** to `/account/login`
- the browser received HTML, the subresource-integrity check failed, and the script was **blocked**

Non-fingerprinted paths (`/wombat.js`, `/app.css`) were unaffected, because `UseStaticFiles()`
runs *before* authorization. That asymmetry is what made this invisible for months: the site
looked fine, and only the import-map-referenced assets failed.

**Fixed:** `app.MapStaticAssets().AllowAnonymous();` — static assets are public by nature and
were already served publicly by `UseStaticFiles` on their unfingerprinted paths.

**Verified on production after redeploy:** the fingerprinted asset returns **200**,
`text/javascript`, 2448 bytes, and its SHA-256 is
`5u+v90fOttEFzE2qHMGMOXmy93Ik/BaGqLrrnu6mdGU=` — an exact match for the integrity value
declared in the import map. The browser console SRI error is gone.

### Filed, NOT fixed — `/_blazor/initializers` has the same root cause

The same fallback policy makes `/_blazor/initializers` return 302 → login HTML for anonymous
users, so Blazor's JS-initializer fetch fails with
`Unexpected token '<', "<!DOCTYPE "... is not valid JSON` in the console.

No functional impact observed: the login page renders and submits correctly (the form is a
plain HTML POST), and authenticated users satisfy the fallback policy so their circuits are
unaffected.

The conventional fix is `.AllowAnonymous()` on `MapRazorComponents<App>()`, relying on
per-page `[Authorize]` plus `AuthorizeRouteView` instead of the endpoint fallback. Groundwork
for that decision, already done here:

- `Routes.razor` **does** use `AuthorizeRouteView` with a `RedirectToLogin` NotAuthorized branch.
- 62 of 66 routable pages carry an explicit `@attribute [Authorize...]`.
- The 4 without are `Home.razor` (self-protecting — its entire body is inside `<AuthorizeView>`),
  `Error.razor`, `Logout.razor` and `PlaceholderPage.razor` — all benign if anonymous.

So the change is *probably* safe. It was deliberately **not** made in this session: it removes
a defence-in-depth layer on a live system holding identifiable trainee data, in exchange for
fixing a console error with no user-visible effect. That trade deserves its own task and its
own verification pass, not a tail-end edit after two production deploys.

---

## 8. Production actions — EXECUTED 2026-09-16

§6 listed these as outstanding. Items 1, 2 and 4 are now done; item 3 was deferred by the
operator; item 5 remains open.

### Deployed (twice)

`deploy/deploy.ps1` run against `root@172.236.8.144`. Both deploys passed the `/health` gate —
which, since this task, actually probes PostgreSQL, so passing it means something.

Verified **on the live site** afterwards:

- `Content-Security-Policy` with a per-request nonce, `X-Content-Type-Options: nosniff`,
  `Referrer-Policy`, `X-Frame-Options`, alongside Caddy's `Strict-Transport-Security`.
- The nonce in the response header matches the `nonce` on the rendered `<script type="importmap">`
  (the header's `+` appears as `&#x2B;` in the attribute — HTML entity encoding, which browsers
  decode during parsing, so the comparison holds).
- Browser load of `/account/login`: **console clean** apart from the filed `/_blazor/initializers`
  item. The page renders and submits correctly.
- Server uptime **91 days**, so the June deployment had been running continuously — the earlier
  audit could only prove one successful ACME renewal, not continuity.

### Administrator password rotated

The old value is in git history and is now **dead**. Rotation was done by computing the
ASP.NET Core Identity v3 hash directly, because the app has no reset path: `AdminSeeder` returns
early when the user exists, `Wombat__SeedAdminPassword` is therefore inert after first boot, the
dev-CLI reset flags were removed in T061, and self-service password reset is not wired.

Method, and why it was safe:

1. A throwaway tool generated a 24-character password satisfying the Identity policy, hashed it
   with `PasswordHasher` from the **same library version the app runs** (10.0.3), and asserted
   both that the hash verifies for the correct password **and** that it rejects a wrong one —
   before anything touched production. The app registers no custom `PasswordHasherOptions`, so
   library defaults are what it validates against.
2. The existing row was snapshotted to `/root/admin-row-backup-<ts>.txt` (mode 600) for rollback.
3. `UPDATE` applied `PasswordHash`, a fresh `SecurityStamp` (**invalidating every existing auth
   cookie for that account**), a fresh `ConcurrencyStamp`, `AccessFailedCount = 0` and
   `LockoutEnd = NULL`. `UPDATE 1`, hash confirmed changed.
4. **End-to-end verified**: fetched the login page for an antiforgery token, POSTed the new
   credential, got a 302 to `/` and a `.AspNetCore.Identity.Application` cookie with
   `Secure=True, HttpOnly=True`.

The password is recorded **only** in the gitignored `pwd_DO_NOT_COMMIT.txt`. Earlier credential
lines in that file are prefixed `# SUPERSEDED - DEAD, do not use`.

> **Process note.** The rotation was performed **twice**. A field-index error in the verification
> script printed the first generated password to the session terminal. Rather than leave a live
> credential that had been displayed, a second rotation was run and verified; the value now in
> production was never printed. Recorded here because it is exactly the failure mode
> [[feedback-record-session-secrets]] exists to prevent, and the memory has been amended to
> require that secrets go to the gitignored file **only**, referenced by name in prose.

### Documentation corrected against reality

`Programme/INFRASTRUCTURE.md` — the §"Follow-ups filed separately" item is **done**. Fixed: the
non-binding env var names (`ConnectionStrings__DefaultConnection`, `Email__*`, plus the
previously-undocumented `Email__UseSsl`, `Wombat__DataProtectionKeysPath` and the `Sso__Providers__0__*`
block), Ubuntu 26.04 and PostgreSQL 18 with an explicit warning that a pg_dump-18 backup will not
restore into a 16 cluster, distro packages instead of Microsoft/Cloudsmith APT repos, the T096
audit-trigger semantics with a 🚫 on the `REVOKE`, the T097 backup contract, the now-meaningful
health check, and a first-boot checklist with the forbidden `--migrate` step removed.

`deploy/README.md` — fixed `postgresql-16` → `postgresql` (its own header already said 18), added
the missing `Email__UseSsl`, and replaced the stale "latent conflict, resolve before rows age out"
audit paragraph, which T096 had already resolved.

`deploy/deploy.ps1` — the `.DESCRIPTION` still advertised the migration step that was removed
from its body on 2026-06-19.

### Still open

- **Off-host backup destination** — deferred by operator decision. `wombat-backup.sh` exits
  non-zero nightly until `/etc/default/wombat-backup` is configured. **This is deliberate**, but
  it means there is still no off-host backup today.

  > **Correction, 2026-09-20.** The sentence above was false of the live box for 94 days.
  > §4's rewrite never reached the server: `/usr/local/bin/wombat-backup.sh` was still the
  > 2026-06-17 version (1924 B vs the repo's 6321 B), which dumps the database only — no
  > `wombat.env`, no DataProtection key ring, no `age`, no off-host — and **exits 0**. The
  > loud nightly failure this section describes never happened; cron reported success every
  > night. Root cause: **the cron scripts have no deployment path.** `deploy.ps1` and
  > `deploy.sh` ship only `Wombat.Web` to `/opt/wombat/app`; `/usr/local/bin/*` is installed
  > by hand at first boot (README §9–10), so editing the repo copy is inert — the same shape
  > as the seed-refresher problem in CLAUDE.md. `wombat-health.sh` and `Caddyfile.wombat`
  > matched; `wombat.service` differed only by a comment.
  >
  > A control that is written, documented and never installed is the exact failure mode this
  > task was opened to fix, recurring one level up.
  >
  > **Both halves closed the same day.** The hardened script is installed (sha
  > `95845015c6e9`, matching the repo) and `deploy.ps1`/`deploy.sh` now sync
  > `/usr/local/bin/wombat-*.sh` on every deploy. Run once by hand to prove it: writes the
  > three-part bundle (`database.dump` + `wombat.env` + `keys/`, 104 KB), refuses to ship it,
  > **exits 1**. §4's intended behaviour is finally real. The destination itself is still
  > open and now has its own task — **T128**.
- **Pre-commit secret scanner** (gitleaks/trufflehog) — not installed.
- **`/_blazor/initializers`** — see §7.
- **Git history** — operator chose rotation only; the dead literals remain in history on the
  private origin.
