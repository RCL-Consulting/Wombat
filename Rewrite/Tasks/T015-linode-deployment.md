# T015 — Linode deployment

**Phase:** 4 — Quality & ship
**Depends on:** T012, T014
**Blocks:** T016

## Goal

Ship the rewritten Wombat to a Linode VPS: Ubuntu 24.04, PostgreSQL, Caddy, systemd. Reach the point where `https://wombat.<domain>` serves the real app with TLS.

## What to do

Execute `INFRASTRUCTURE.md`'s "First-boot checklist" against a real (or rebuilt) VPS. Every step lands in a commit of a new folder `deploy/` in the repo.

1. **Server prep**
   - Fresh Ubuntu 24.04 LTS Linode.
   - `apt update && apt upgrade -y`.
   - Set hostname: `hostnamectl set-hostname wombat-prod`.
   - Create `wombat` system user (no shell, no home).
   - UFW firewall: allow 22, 80, 443 only.
2. **.NET runtime**
   - Install .NET 10 ASP.NET Core runtime from the Microsoft APT repo. Do **not** install the SDK on the server; publishing happens from dev.
3. **PostgreSQL**
   - Install Postgres 16 from the default Ubuntu repo.
   - Create `wombat` role with a strong random password. Save the password into the env file only.
   - Create `wombat` database owned by the `wombat` role.
   - Test: `psql -h 127.0.0.1 -U wombat -d wombat -c 'SELECT 1;'`.
4. **Config**
   - Create `/opt/wombat/config/` owned by `wombat:wombat`.
   - Copy `appsettings.Production.json` from the repo.
   - Create `/opt/wombat/config/wombat.env` per INFRASTRUCTURE.md. Mode 600. Fill in real values.
5. **Publish & deploy**
   - From dev: `dotnet publish src/Wombat.Web -c Release -o publish/`.
   - `rsync -az --delete publish/ wombat-prod:/opt/wombat/app/`.
   - Write `deploy/deploy.sh` that does the publish + rsync + restart, committed to the repo.
6. **Database migration**
   - `ssh wombat-prod "sudo -u wombat dotnet /opt/wombat/app/Wombat.Web.dll --migrate"`.
   - Then `--seed` once (admin + roles only in Production).
7. **systemd**
   - Install `/etc/systemd/system/wombat.service` per INFRASTRUCTURE.md.
   - `systemctl daemon-reload && systemctl enable --now wombat`.
   - Check `journalctl -u wombat -f` for a clean startup.
8. **Caddy**
   - Install Caddy from the official APT repo.
   - Install the Caddyfile stanza per INFRASTRUCTURE.md. **Include `flush_interval -1`** — Blazor Server will not work without it.
   - `systemctl reload caddy`.
   - Verify TLS cert is issued (Let's Encrypt, automatic).
9. **Health check**
   - Add a `/health` endpoint to `Wombat.Web` if T014 didn't already.
   - Script `/usr/local/bin/wombat-health.sh` runs curl against `/health` every minute from cron. On three consecutive failures, restart the service and email the admin.
10. **Backups**
    - Install `/usr/local/bin/wombat-backup.sh` per INFRASTRUCTURE.md.
    - Cron: daily at 02:00.
    - Verify the first backup: restore into a throwaway database, run a simple query, drop it.
11. **Commit everything**
    - `deploy/deploy.sh`
    - `deploy/wombat.service`
    - `deploy/Caddyfile.wombat`
    - `deploy/wombat-backup.sh`
    - `deploy/wombat-health.sh`
    - `deploy/README.md` — one-page "how to deploy Wombat" doc with commands in order.

## Files created (in repo)

- `deploy/deploy.sh`
- `deploy/wombat.service`
- `deploy/Caddyfile.wombat`
- `deploy/wombat-backup.sh`
- `deploy/wombat-health.sh`
- `deploy/README.md`

## Verification

- [ ] `https://wombat.<domain>` returns a TLS-secured login page.
- [ ] The seeded admin can log in.
- [ ] Issuing an invitation sends a real email through the configured SMTP provider.
- [ ] `systemctl status wombat` says `active (running)`.
- [ ] `journalctl -u wombat --since "10 minutes ago"` shows no errors.
- [ ] Restarting the service takes the site down for <5 seconds.
- [ ] The nightly backup cron runs successfully once (check log).

## Notes & gotchas

- **`flush_interval -1` in Caddy** — forgetting this breaks SignalR / Blazor Server in subtle ways (streaming responses get buffered, UI hangs on the first interaction). This is the most common deployment bug for Blazor Server on Caddy.
- **systemd `Type=notify`** requires the app to call `IHostApplicationLifetime.ApplicationStarted` (which ASP.NET Core does by default) plus the `Microsoft.Extensions.Hosting.Systemd` package to signal readiness. Add the package if not already present.
- **Secrets in env file** — never commit. Check mode is 600. Check owner is wombat:wombat. Verify it is not readable by other users.
- **The first cold start is slow** — .NET 10 on small Linode instances can take 10–20 seconds to boot. Set `TimeoutStartSec=60` in the systemd unit to avoid false failures.
- **DNS propagation** — if the `A` record was created recently, Caddy may fail to get a cert on the first try. Wait 10 minutes and retry.
- **Publish runtime identifier** — decide whether to publish framework-dependent (requires .NET runtime on the server) or self-contained (bigger binary, no server-side runtime needed). ClinicAssist uses framework-dependent; copy that.
