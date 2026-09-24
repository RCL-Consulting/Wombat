# Wombat deployment

Target: Ubuntu LTS on Linode. Caddy + PostgreSQL + systemd.

**Live deployment:** `https://wombat.rcl.co.za` (Linode `172.236.8.144`, Ubuntu 26.04
LTS, 1 vCPU / 1 GB). On Ubuntu 26.04 the .NET 10 runtime, PostgreSQL (18), and Caddy
are all in the **default distro repos** — no Microsoft or Cloudsmith APT repos needed
(the commands below reflect that; older 24.04 notes are kept inline where they differ).
Secrets for this deployment are recorded in `pwd_DO_NOT_COMMIT.txt`.

## First-boot setup

Run these steps once when provisioning a new server. All commands run as root
unless noted.

### 1. OS prep

```bash
apt update && apt upgrade -y
hostnamectl set-hostname wombat-prod
adduser --system --group --no-create-home --shell /usr/sbin/nologin wombat
ufw allow 22 && ufw allow 80 && ufw allow 443 && ufw --force enable
```

### 2. Install .NET 10 runtime

On **Ubuntu 26.04** the ASP.NET Core 10 runtime is in the default repo:

```bash
apt update
apt install -y aspnetcore-runtime-10.0
```

(On Ubuntu 24.04, add the Microsoft APT repo first:
`wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb && dpkg -i packages-microsoft-prod.deb && apt update`,
then install the same package.)

### 3. PostgreSQL

```bash
apt install -y postgresql          # 18.x on Ubuntu 26.04 — do NOT pin 16: pg_dump 18 backups will not restore into a 16 cluster
sudo -u postgres createuser --pwprompt wombat   # enter a strong random password
sudo -u postgres createdb -O wombat wombat
# Verify:
psql -h 127.0.0.1 -U wombat -d wombat -c 'SELECT 1;'
```

**Audit append-only:** the `AuditLog` migration already installs a PostgreSQL trigger
(`audit_entries_immutable`) that raises an exception on any UPDATE/DELETE of
`AuditEntries` — for **all** roles, including the table owner. So the manual
`REVOKE UPDATE, DELETE ON "AuditEntries" FROM wombat;` mentioned in older docs is
**redundant and not applied here** — and applying it now would actively break archival.

**Resolved by T096** (migration `20260619065511_T096_AuditDeleteForArchival`): the trigger
still raises on every `UPDATE`, and on `DELETE` **unless** the transaction sets
`SET LOCAL wombat.allow_audit_delete = 'on'`. `AuditLogRetentionJob` opts in explicitly inside
its own transaction; nothing else does. The "latent conflict" this paragraph used to warn about
is gone. A table-level `REVOKE` cannot be bypassed by the GUC, which is why it must stay unapplied.

### 4. Config directory

```bash
mkdir -p /opt/wombat/config /opt/wombat/data/logs /opt/wombat/data/uploads
chown -R wombat:wombat /opt/wombat
```

**Do not copy `appsettings.Production.json` here.** This step used to say to, and it was
wrong: the app's ContentRoot is its `WorkingDirectory`, `/opt/wombat/app`, and
`appsettings.Production.json` already ships in the publish output. A copy under `config/` is
**read by nothing** — so editing it to change production behaviour silently does nothing,
which is worse than its absence. `deploy/verify/drift-check.sh` asserts it is not there.
(The live box never had one; the instruction was simply never executed.)

`/opt/wombat/config/` holds exactly one file: `wombat.env`.

Create `/opt/wombat/config/wombat.env` (mode 600):

```bash
cat > /opt/wombat/config/wombat.env <<'EOF'
ConnectionStrings__DefaultConnection=Host=127.0.0.1;Database=wombat;Username=wombat;Password=REDACTED
Email__SmtpHost=smtp.example.com
Email__SmtpPort=587
Email__SmtpUser=wombat@example.com
Email__SmtpPassword=REDACTED
Email__FromAddress=no-reply@example.com
Email__FromName=Wombat
Email__UseSsl=false          # true for implicit SSL on port 465 (this deployment uses 465 + true)
Wombat__BaseUrl=https://wombat.example.com
Wombat__MsfRespondUrl=https://wombat.example.com/msf/respond
Wombat__SeedAdminEmail=renier@rcl.co.za
Wombat__SeedAdminPassword=REDACTED
Wombat__PseudonymSalt=REDACTED
Wombat__DataProtectionKeysPath=/opt/wombat/data/keys
EOF
chmod 600 /opt/wombat/config/wombat.env
chown wombat:wombat /opt/wombat/config/wombat.env
```

**`Wombat__MsfRespondUrl`** — the MSF respondent page every invitation links to. Optional: unset, it
is `{Wombat__BaseUrl}/msf/respond`. Set, it must be on `Wombat__BaseUrl`'s scheme, host and port, or
opening a campaign refuses. Never the Api's `:5090`: that host is not deployed.

**Never rotate `Wombat__PseudonymSalt`** — it is used to generate stable pseudonyms
for erased users. Rotating it breaks linkability across exports and erasure records.

**`Wombat__DataProtectionKeysPath`** — directory where ASP.NET Core DataProtection keys
are persisted (auth cookies + antiforgery tokens). Required under systemd: the service
user is homeless and `ProtectSystem=strict` makes the default `$HOME/.aspnet` location
unwritable, so without this keys would be ephemeral and every restart would log users
out. Point it inside `ReadWritePaths` (i.e. under `/opt/wombat/data`). Back it up.

**Email (`Email__*`) is optional for Phase 1.** If omitted, the app still runs but
invitations/notifications won't send. Add the SMTP keys and `systemctl restart wombat`
to enable email later — no redeploy needed.

### 5. Deploy the application

From your dev machine (first deploy only — subsequent deploys use `deploy.sh`):

```bash
rm -rf publish/          # dotnet publish does NOT clean its output dir
dotnet publish src/Wombat.Web/Wombat.Web.csproj -c Release -o publish/
rsync -az --delete publish/ wombat-prod:/opt/wombat/app/
```

`publish/` is repo-root build output, gitignored, and both deploy scripts now clear it
before publishing. Without the wipe a file dropped from the project lingers there and
ships — `rsync --delete` would catch it, but `deploy.ps1` tars the directory wholesale
and the server extracts into a fresh `app.new`, so the stale file survives the rotation.
**Never run the `rsync` line on its own**: `publish/` may hold binaries from an older
commit.

### 6. Database migration and seeding

No manual step needed: the service applies all EF migrations and seeds roles + the
bootstrap admin **automatically on startup** (next step), using the connection string
from `wombat.env` via systemd's `EnvironmentFile`.

> Do **not** run `sudo -u wombat dotnet Wombat.Web.dll --migrate` by hand expecting it
> to pick up `wombat.env` — a one-shot process doesn't inherit the service environment,
> and a bash `. wombat.env` mis-splits the connection string on its `;`. Let systemd
> (step 7) start the service, which loads the env correctly and migrates + seeds on boot.
> Watch `journalctl -u wombat -f` to confirm migrations apply and the admin is seeded.

### 7. systemd service

```bash
cp deploy/wombat.service /etc/systemd/system/wombat.service
systemctl daemon-reload
systemctl enable --now wombat
# Verify:
journalctl -u wombat -f
```

Add a sudoers rule so the deploy script can restart the service without a password:

```bash
echo 'wombat ALL=(ALL) NOPASSWD: /bin/systemctl restart wombat' \
    > /etc/sudoers.d/wombat-restart
chmod 440 /etc/sudoers.d/wombat-restart
```

### 8. Caddy

On **Ubuntu 26.04** Caddy is in the default repo:

```bash
apt install -y caddy
```

(On Ubuntu 24.04, add the Cloudsmith Caddy repo first — see Caddy's install docs.)

Install `deploy/Caddyfile.wombat` as `/etc/caddy/Caddyfile` (it is the full file, not a
fragment — adjust the domain for a different deployment), then:

```bash
cp deploy/Caddyfile.wombat /etc/caddy/Caddyfile
caddy validate --config /etc/caddy/Caddyfile --adapter caddyfile
systemctl reload caddy
```

DNS must already point at the server (it does for `wombat.rcl.co.za`); Caddy then
obtains a Let's Encrypt cert automatically (TLS-ALPN-01) within a few seconds.

Caddy obtains a Let's Encrypt certificate automatically. DNS must point at the
server before this step, or the ACME challenge will fail.

### 9. Health check cron

> **Steps 9 and 10 bootstrap these scripts; every later deploy re-syncs them.** Since
> 2026-09-20 `deploy.ps1` and `deploy.sh` copy `deploy/wombat-*.sh` to `/usr/local/bin/` on
> every run. Before that they did not, and nothing else did either — which is how the T097
> backup rewrite sat undeployed for three months while these docs described it as live.
>
> `wombat.service`, `Caddyfile.wombat` and `appsettings.Production.json` are **still**
> install-once-by-hand. After editing any of them, re-run the relevant step, and check drift:
>
> ```bash
> ssh root@<host> 'sha256sum /usr/local/bin/wombat-*.sh /etc/systemd/system/wombat.service'
> sha256sum deploy/wombat-*.sh deploy/wombat.service
> ```

```bash
cp deploy/wombat-health.sh /usr/local/bin/wombat-health.sh
chmod +x /usr/local/bin/wombat-health.sh
echo '* * * * * root /usr/local/bin/wombat-health.sh >> /var/log/wombat-health.log 2>&1' \
    > /etc/cron.d/wombat-health
```

### 10. Backup cron

```bash
cp deploy/wombat-backup.sh /usr/local/bin/wombat-backup.sh
chmod +x /usr/local/bin/wombat-backup.sh
echo '0 2 * * * root /usr/local/bin/wombat-backup.sh >> /var/log/wombat-backup.log 2>&1' \
    > /etc/cron.d/wombat-backup
```

Verify the first backup restores cleanly:

```bash
pg_restore -h 127.0.0.1 -U wombat -d postgres \
    --create --clean /var/backups/wombat/daily/wombat-$(date +%Y-%m-%d).dump || true
# run a quick query against the restored DB, then drop it
```

## Subsequent deploys

From your dev machine:

```bash
# Linux / macOS / WSL / Git-Bash (needs rsync + bash):
./deploy/deploy.sh [user@host]
```

```powershell
# Native Windows PowerShell (uses tar + scp; no rsync needed):
./deploy/deploy.ps1                       # defaults to root@172.236.8.144
./deploy/deploy.ps1 -Remote user@host     # e.g. staging
```

Both: clear `publish/`, publish locally (Release), rotate the previous release on the
server (`/opt/wombat/app` -> `app.prev`), ship the new binaries, restart the service
(which auto-applies EF migrations on startup via systemd's env), and confirm `/health`.
`deploy.sh` rsyncs; `deploy.ps1` ships a tarball over scp.

`deploy.ps1` stages its tarball in the **OS temp directory**, not in the repo, and deletes
it after a successful upload. It used to write 44 MB to `deploy/.remote/` — a folder of
otherwise-tracked scripts is the wrong home for build output, even gitignored.

## Verification scripts

`deploy/verify/` holds five read-only checks for the live box. None hardcodes a
credential — the two that need one read it from `/opt/wombat/config/wombat.env` at
runtime. None of them writes to the production database; the two that need a database
restore into a throwaway and drop it afterwards.

| Script | Answers |
|---|---|
| `restore-rehearsal.sh` | **Is the nightly dump actually restorable?** Restores the newest dump into a throwaway DB, counts roles/users/migrations, drops it. This is the rehearsal §10 describes — run it rather than assume it. |
| `audit-trigger.sh` | **Does the T096 append-only guarantee still hold?** Asserts UPDATE always rejected, DELETE rejected by default, DELETE permitted only under `wombat.allow_audit_delete`. |
| `dataprotection-keys.sh` | **Will a restart log everyone out?** Checks the key ring lives under `/opt/wombat/data/keys` and that the service user can write there. |
| `smoke-test.sh` | **Does the authenticated surface work?** Logs in as the seeded admin and crawls eleven pages, reporting HTTP status, authenticated-or-not, and title. |
| `login-cookie.sh` | **Is the auth cookie `Secure` behind Caddy?** The forwarded-headers property `smoke-test.sh` does not assert. |
| `drift-check.sh` | **Does the server run what this repo says it does?** Hashes every deployed artifact against its repo original, asserts the paths that must *not* exist, and checks file modes. **Runs locally, not piped** — a comparison needs both sides. |

`drift-check.sh` is the exception — run it **locally**, and it exits non-zero on any drift:

```bash
./deploy/verify/drift-check.sh [user@host]      # default root@172.236.8.144
```

The other four are piped to the server rather than installed, so they need no execute bit:

```powershell
Get-Content -Raw deploy/verify/restore-rehearsal.sh | ssh root@172.236.8.144 "tr -d '\r' | bash -s"
```

```bash
ssh root@172.236.8.144 'bash -s' < deploy/verify/restore-rehearsal.sh
```

`smoke-test.sh` and `login-cookie.sh` take an optional base URL as the first argument,
defaulting to `https://wombat.rcl.co.za`.

These were promoted on 2026-09-20 out of an untracked scratch folder from the June
first-boot session. Thirteen sibling scripts were deleted as superseded by the checklist
above — **two of them had the database password, the seed admin password, the SMTP
password and the unrotatable `Wombat__PseudonymSalt` hardcoded in plaintext.** If you
write another ad-hoc server script, read secrets from `wombat.env`; do not paste them in.

## Rollback

```bash
ssh wombat-prod "rm -rf /opt/wombat/app && mv /opt/wombat/app.prev /opt/wombat/app && sudo systemctl restart wombat"
```

For database migrations: if a migration is destructive, restore from the nightly
dump. Prefer additive-only migrations to keep rollback simple.

## Useful commands

```bash
# Tail application logs
journalctl -u wombat -f

# Last 100 lines
journalctl -u wombat -n 100 --no-pager

# HTTP logs (Caddy)
tail -f /var/log/caddy/wombat.log

# Service status
systemctl status wombat

# Manual health check
curl -i http://127.0.0.1:5080/health
```
