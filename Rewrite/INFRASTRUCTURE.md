# Infrastructure — Linode deployment

Wombat will run on a Linode VPS. The layout mirrors ClinicAssist.NET's production setup. Everything in this document is "what the server should look like when task T015 is done".

## Target

- **One Linode VPS**, Ubuntu 24.04 LTS, 2GB RAM minimum (4GB recommended if Blazor circuits grow).
- **One managed PostgreSQL database** — either Linode Managed Postgres or a local Postgres on the same VPS. Local Postgres is fine for Phase 1; migrate to managed later if load justifies it.
- **DNS** — an `A` record pointing `wombat.<yourdomain>` at the VPS IP.
- **Email** — SMTP credentials for an external provider (Postmark, SendGrid, Mailgun, or a self-hosted Postfix). The app only needs SMTP host/port/user/pass, nothing custom.

## Server layout

```
/opt/wombat/
├── app/                  <-- published binaries (dotnet publish output)
│   ├── Wombat.Web.dll
│   └── ...
├── data/                 <-- writable runtime data
│   ├── logs/             <-- serilog rolling files
│   └── uploads/          <-- if needed
└── config/
    ├── appsettings.Production.json
    └── wombat.env        <-- environment variables, loaded by systemd (mode 600)
```

Owner: a dedicated `wombat` system user with no shell login. `sudo adduser --system --group --no-create-home --shell /usr/sbin/nologin wombat`.

## Systemd unit

`/etc/systemd/system/wombat.service`:

```ini
[Unit]
Description=Wombat Web
After=network.target postgresql.service
Wants=postgresql.service

[Service]
Type=notify
WorkingDirectory=/opt/wombat/app
ExecStart=/usr/bin/dotnet /opt/wombat/app/Wombat.Web.dll
Restart=always
RestartSec=5
User=wombat
Group=wombat
EnvironmentFile=/opt/wombat/config/wombat.env
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5080
KillSignal=SIGINT
TimeoutStopSec=30
SyslogIdentifier=wombat
ProtectSystem=strict
ReadWritePaths=/opt/wombat/data
PrivateTmp=true
NoNewPrivileges=true

[Install]
WantedBy=multi-user.target
```

Match ClinicAssist's unit file where it differs; do not invent differences.

## Caddy

`/etc/caddy/Caddyfile` stanza:

```
wombat.example.com {
    encode zstd gzip
    reverse_proxy 127.0.0.1:5080 {
        header_up Host {host}
        header_up X-Forwarded-Proto {scheme}
        flush_interval -1
    }
    log {
        output file /var/log/caddy/wombat.log
    }
}
```

`flush_interval -1` is required for SignalR / Blazor Server streaming responses. Do not forget it.

Caddy handles TLS automatically via Let's Encrypt. No manual cert management.

## Database

Two options:

1. **Local Postgres 16** on the same VPS — simpler, cheaper, fine for Phase 1.
2. **Linode Managed Postgres** — pay for it when uptime matters more than $.

For Phase 1, use local Postgres:

```bash
sudo apt install postgresql-16
sudo -u postgres createuser --pwprompt wombat
sudo -u postgres createdb -O wombat wombat
```

Connection string in `wombat.env`:

```
Wombat__ConnectionStrings__Default=Host=127.0.0.1;Database=wombat;Username=wombat;Password=...
```

## Environment file

`/opt/wombat/config/wombat.env` (mode 600, owner wombat:wombat):

```
Wombat__ConnectionStrings__Default=Host=127.0.0.1;Database=wombat;Username=wombat;Password=REDACTED
Wombat__Email__SmtpHost=smtp.example.com
Wombat__Email__SmtpPort=587
Wombat__Email__SmtpUser=wombat@example.com
Wombat__Email__SmtpPassword=REDACTED
Wombat__Email__FromAddress=no-reply@example.com
Wombat__Email__FromName=Wombat
Wombat__BaseUrl=https://wombat.example.com
Wombat__SeedAdminEmail=renier@rcl.co.za
Wombat__SeedAdminPassword=REDACTED
Wombat__PseudonymSalt=REDACTED
```

The double-underscore syntax is ASP.NET Core's convention for nesting. Never commit this file.

**`Wombat__PseudonymSalt`** — used by the erasure executor (T026) to generate deterministic pseudonyms for erased users (`deleted_user_<hex>`). This is a deployment secret. **Do not rotate it** — rotating the salt breaks pseudonym stability across exports and makes previously-issued pseudonyms unlinkable to erasure records.

## Deployment process

Two paths depending on taste:

### Simple (Phase 1)

1. On dev machine: `dotnet publish src/Wombat.Web -c Release -o publish/`
2. `rsync -az --delete publish/ wombat@vps:/opt/wombat/app/`
3. `ssh wombat@vps "sudo systemctl restart wombat"`
4. Tail logs: `journalctl -u wombat -f`

Wrap that in a `deploy.sh` script committed to the repo. That is the whole deploy pipeline. Add CI later if needed.

### Fancy (later)

- GitHub Actions on push to `main`: build, test, publish artifact, scp to server, restart service.
- Staging environment on a second subdomain and a second systemd unit.

Defer the fancy path. Task T015 delivers the simple path only.

## Audit log retention and cold storage

Audit entries are **never deleted** from the system by admins or automated cleanup. The lifecycle is:

| Age | Location | Action |
|-----|----------|--------|
| 0–2 years | `AuditEntries` (live table) | Queryable from the admin UI. Append-only; PostgreSQL trigger prevents UPDATE/DELETE. |
| 2–7 years | `AuditEntryArchives` | Moved by `AuditLogRetentionJob` (daily 03:00 UTC). Not surfaced in the admin UI by default; query directly if needed. |
| 7+ years | Cold storage (Linode Object Storage) | Exported as gzipped JSONL files and removed from the database. |

### Cold storage export (7-year transition)

When entries in `AuditEntryArchives` reach 7 years old, a cron job (to be configured separately, not part of the application) should:

1. Export rows with `OccurredAt < NOW() - INTERVAL '7 years'` as `audit-YYYY.jsonl.gz`.
2. Upload to a private Linode Object Storage bucket: `s3://wombat-audit-cold/{year}/`.
3. DELETE the exported rows from `AuditEntryArchives`.

The export cron is **not** part of the application binary — it is a server-level script (`/usr/local/bin/wombat-audit-cold.sh`) that connects to PostgreSQL directly. This keeps cold storage logic out of the app's attack surface.

### Append-only enforcement

The migration that creates `AuditEntries` also installs a PostgreSQL trigger (`audit_entries_immutable`) that raises an exception on any UPDATE or DELETE. Additionally, revoke mutation privileges from the app user after migration:

```sql
REVOKE UPDATE, DELETE ON "AuditEntries" FROM wombat;
```

Run this once, post-migration. It does not need to be re-applied on upgrades.

### Retention window

The default is 2 years active + 5 years archive = 7 years total. Regulators may require longer retention. If an institution's governing body specifies a different window, adjust the `AuditLogRetentionJob` configuration (the 2-year cutoff is the only parameter). The cold-storage script uses the archive table as its source and can run as often as needed.

## SSO (OIDC) provider configuration

Wombat supports institutional single sign-on via OpenID Connect. Providers are configured in `appsettings` (or, for secrets, in environment variables). Adding a new provider is a config edit + app restart — no redeployment.

### appsettings structure

```json
"Sso": {
  "Providers": [
    {
      "Key": "uct",
      "DisplayName": "University of Cape Town",
      "InstitutionId": 1,
      "Authority": "https://login.microsoftonline.com/<tenant-id>/v2.0",
      "ClientId": "<from Azure AD app registration>",
      "ClientSecret": "<from environment variable>",
      "Scopes": ["openid", "profile", "email"],
      "GroupsClaim": "groups",
      "EnableFederatedLogout": false
    }
  ]
}
```

### Environment variables for secrets

Client secrets must not be stored in `appsettings.Production.json`. Use the environment file:

```
Sso__Providers__0__ClientSecret=REDACTED
```

The double-underscore `__` with array index `0` maps to `Sso:Providers[0]:ClientSecret`.

### Provider-specific notes

| Provider | Notes |
|----------|-------|
| **Microsoft Entra ID** | Groups claim emits object IDs by default, not display names. Map by object ID in `SsoGroupRoleMappings`; keep display names for admin UX. Enable "Group claims" in the app registration's Token Configuration. |
| **Google Workspace** | No native groups claim. Use Google Directory API to populate groups, or map by domain/OU. |
| **Shibboleth** | SAML at the wire level. Point an OIDC bridge (Keycloak, Auth0, or a SAML-to-OIDC adapter) at the Shibboleth IdP. Wombat talks to the bridge over OIDC. Do not implement SAML directly. |

### Security invariants

- `state` and `nonce` parameters are enforced by ASP.NET Core's OIDC handler.
- Valid issuer list is strictly from configured authorities.
- Clock skew tolerance: 2 minutes.
- **Administrator role cannot be assigned via SSO.** Even if the mapping table maps a group to Administrator, the `SsoGroupMapper` logs a warning and skips it. Administrator requires explicit manual assignment.
- SSO-provisioned users have `AllowLocalPassword = false` — they cannot set a local password and are refused by the local login form.
- Break-glass: at least two local-password Administrator accounts must exist independent of SSO.

### Callback URLs

Each provider gets its own callback path: `/signin-oidc-{key}` (e.g. `/signin-oidc-uct`). Register this as the redirect URI in the IdP's app registration.

### Group-to-role mapping

Managed by administrators at `/admin/sso/group-mappings`. Each mapping links a provider + external group ID to a Wombat role, institution, and optional speciality/sub-speciality scope. Changes take effect on the next login for each user.

## Backups

- **Database**: nightly `pg_dump` to `/var/backups/wombat/`, then `rsync` off-host (or Linode Object Storage). Retain 14 daily, 4 weekly, 6 monthly.
- **Config**: `/opt/wombat/config/` backed up daily, same destination. Secrets in this file mean the backup destination must be private.
- **Uploads**: if the app writes user files, back those up the same way.

A simple cron:

```
0 2 * * * /usr/local/bin/wombat-backup.sh >> /var/log/wombat-backup.log 2>&1
```

Script contents live in the repo under `deploy/wombat-backup.sh`.

## Secrets management

- No secrets in the repo.
- No secrets in `appsettings.Production.json`.
- Secrets live in `/opt/wombat/config/wombat.env` on the server, mode 600, owner wombat.
- Rotation: edit the env file, `systemctl restart wombat`. That's the whole rotation story for Phase 1.
- **Exception:** `Wombat__PseudonymSalt` must **never** be rotated — see the Environment file section above.

## Observability

- `journalctl -u wombat` for application logs (Serilog Console sink).
- `/opt/wombat/data/logs/` for Serilog file sink (rolling daily, keep 30 days).
- `/var/log/caddy/wombat.log` for HTTP logs.
- Optional: a Seq instance on a subdomain, pointed at by Serilog's Seq sink. Nice to have; not required for Phase 1.

## Health check

Expose `/health` in `Wombat.Web` and have Caddy probe it (or a simple cron + curl). If it returns non-200 for two consecutive minutes, restart the service and email you. Don't get clever; a 10-line shell script is fine.

## Rollback

- Keep the last two published binary directories: `/opt/wombat/app.prev/` is the previous release.
- Rollback = `rm -rf /opt/wombat/app && mv /opt/wombat/app.prev /opt/wombat/app && systemctl restart wombat`.
- For database migrations, rollback is manual — if a migration is destructive, restore from the nightly dump. Prefer additive migrations.

## First-boot checklist

When setting up a fresh VPS:

1. Fresh Ubuntu 24.04, apply all updates, set hostname.
2. Create `wombat` system user.
3. Install .NET 10 SDK (from Microsoft's APT repo) or just the runtime if you're publishing self-contained from dev.
4. Install Postgres 16, create the database and user.
5. Install Caddy from the official APT repo.
6. Copy `appsettings.Production.json` and `wombat.env` to `/opt/wombat/config/`.
7. Publish from dev and rsync to `/opt/wombat/app/`.
8. Install the systemd unit, `systemctl daemon-reload`, `systemctl enable --now wombat`.
9. Install the Caddyfile stanza, `systemctl reload caddy`.
10. Run DB migrations: `dotnet Wombat.Web.dll --migrate` (wire this up as a one-shot mode, same as ClinicAssist's `dbMigrator`).
11. Visit `https://wombat.example.com`, log in as the seeded admin, issue an invitation.

That checklist is the verification for T015. Put it in the task file too.
