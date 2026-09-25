# Handover — running Wombat

The T016 deliverable: what a stranger needs to take over the service. It covers the
running system, not the codebase — for the code, start at `CLAUDE.md`, then
`execution/STATE.md`.

`deploy/README.md` is the **operational manual** and stays authoritative for every
command. This file is the orientation around it: what exists, what it costs you to get
wrong, and what to do first.

---

## ⚠ Read this before anything else

**Wombat is deployed but not in service.** `wombat.rcl.co.za` is reachable and the local
dev database is populated, but both hold **scenario-execution data only** — rows produced
by replaying the `execution/knowledge/scenario-*.md` runbooks. There are no real trainees, no real
assessors and no real clinical records anywhere.

Two consequences:

1. **Backward compatibility is not a design constraint.** Destructive migrations, dropped
   columns and re-seeding from scratch are all on the table. `CLAUDE.md` has a section,
   *"Nothing is live — compatibility is not a constraint"*, that spells this out.
2. **The day Wombat takes on a real user, delete that section and re-read every decision
   it licensed.** Everything below marked *acceptable while nothing is live* flips at that
   moment. This is the single most important thing in this document.

---

## What is deployed, and where

| | |
|---|---|
| URL | `https://wombat.rcl.co.za` |
| Host | Linode `172.236.8.144` |
| OS | **Ubuntu 26.04 LTS** (the plan specified 24.04; the box was built on 26.04) |
| Size | 1 vCPU / **1 GB RAM** — half the documented minimum, mitigated with a 1 GB swapfile and `DOTNET_gcServer=0` (workstation GC) |
| Runtime | `aspnetcore-runtime-10.0`, from the **distro repo** |
| Database | PostgreSQL **18**, from the distro repo, local to the box |
| TLS / proxy | Caddy, distro repo, Let's Encrypt via TLS-ALPN-01 (auto-renewing) |
| Process manager | systemd unit `wombat` |
| Access | SSH as `root@172.236.8.144`, key `id_ed25519` |

On 26.04 the .NET runtime, PostgreSQL and Caddy are **all in the default Ubuntu repos** —
no Microsoft or Cloudsmith APT repo is needed. Do **not** pin PostgreSQL 16: a `pg_dump` 18
backup will not restore into a 16 cluster, so pinning silently invalidates every existing
backup.

**RAM is the scaling constraint.** Blazor Interactive Server holds a circuit per connected
user in memory. Resize before onboarding a real cohort, not after.

### Filesystem layout

```
/opt/wombat/app/              current release
/opt/wombat/app.prev/         previous release — this is the rollback
/opt/wombat/config/wombat.env every secret (mode 600, owner wombat:wombat)
/opt/wombat/config/appsettings.Production.json
/opt/wombat/data/logs/        Serilog file sink, rolling daily, 30 days
/opt/wombat/data/keys/        DataProtection key ring — see "Two things you cannot recover"
/opt/wombat/data/uploads/
/var/backups/wombat/          daily/ weekly/ monthly/
```

---

## Configuration

Everything secret lives in **`/opt/wombat/config/wombat.env`**, mode 600, owner `wombat`.
Nothing secret is in the repo, and nothing secret belongs in
`appsettings.Production.json`. The full key list is in `execution/architecture/INFRASTRUCTURE.md`
§ Environment file.

Rotation, for Phase 1, is the whole story: edit the env file, `systemctl restart wombat`.

The credentials for *this* deployment are recorded in **`pwd_DO_NOT_COMMIT.txt`** on the
owner's dev box. That file is gitignored and must stay that way — a secret has leaked into
this repo's committed history once already.

Three keys behave in ways that will surprise you:

- **`Wombat__PseudonymSalt` must never be rotated.** It generates the stable pseudonyms
  issued for POPIA/GDPR erasures. Rotating it breaks linkability across every export and
  erasure record already issued, and it cannot be reconstructed from anything.
- **`Wombat__DataProtectionKeysPath` is required in production.** The service user is
  homeless and `ProtectSystem=strict` makes the default `$HOME/.aspnet` location
  unwritable, so without it the key ring is regenerated on every start and **every user is
  logged out on each restart**.
- **`Wombat__SeedAdminPassword` is consumed only once**, the first time the admin user does
  not exist. `AdminSeeder` returns early afterwards, so editing it later does nothing.
  Rotate the live admin password **through the admin UI**. It was last rotated 2026-09-16
  under T097.

**Email is optional.** Omit the `Email__*` keys and the app runs, but invitations and
notifications will not send. The live box uses implicit SSL on **port 465**, which requires
`Email__UseSsl=true` — leaving the `false` default against port 465 produces a connection
that never completes. The SMTP account belongs to the Wombat owner (`renier@rcl.co.za`);
its credentials are in `pwd_DO_NOT_COMMIT.txt` and in the live env file.

---

## Deploying and rolling back

```powershell
./deploy/deploy.ps1                      # Windows; defaults to root@172.236.8.144
./deploy/deploy.ps1 -Remote user@host
```

```bash
./deploy/deploy.sh [user@host]           # Linux/macOS/WSL/Git-Bash; needs rsync
```

Both publish locally in Release, rotate `/opt/wombat/app` to `app.prev`, ship the new
binaries, restart the service and confirm `/health`.

**Migrations apply automatically at startup** via `Database.MigrateAsync()`, using the
connection string systemd loads from `wombat.env`. You do not run `dotnet ef` against
production. The corollary: **a bad migration takes the app down on restart, not on
deploy.** Read every generated migration before shipping it, and prefer additive ones.

Rollback of the binaries:

```bash
ssh root@172.236.8.144 "rm -rf /opt/wombat/app && mv /opt/wombat/app.prev /opt/wombat/app && systemctl restart wombat"
```

Rollback of a **destructive migration** is a restore from the nightly dump. There is no
down-migration path in production. That is the reason to prefer additive migrations.

---

## Backups, and the one thing to verify first

`/usr/local/bin/wombat-backup.sh` runs nightly at 02:00 from `/etc/cron.d/wombat-backup`.
It bundles **three** things, because a database-only backup restores to a box that boots
and then cannot honour its own erasure records:

1. the `pg_dump -Fc` database dump,
2. `wombat.env` — which carries the unrotatable `Wombat__PseudonymSalt`,
3. `/opt/wombat/data/keys` — the DataProtection key ring.

Retention: 14 daily, 4 weekly (Sundays), 6 monthly (the 1st).

The bundle is **`age`-encrypted before it leaves the box** (it contains `wombat.env`) to a
recipient whose private key lives on neither machine, then shipped off-host. Configure in
`/etc/default/wombat-backup` (mode 600): `WOMBAT_BACKUP_AGE_RECIPIENT`, plus one of
`WOMBAT_BACKUP_RCLONE_REMOTE` or `WOMBAT_BACKUP_REMOTE`. The script **exits non-zero if the
off-host leg is unconfigured or `age` is missing**, so cron mails you, and it refuses to
ship `wombat.env` unencrypted.

> **Live state, verified and fixed 2026-09-20.**
>
> For 94 days none of the paragraph above was true of this box. `/usr/local/bin/wombat-backup.sh`
> was still the **2026-06-17** version (1924 B vs the repo's 6321): database dump only, no
> `wombat.env`, no key ring, no `age`, no off-host — and it **exited 0**, so cron reported
> success every night and T097's "fails loudly instead" was true only of the repo file.
> Root cause: the cron scripts had **no deployment path**. Both halves are now closed —
> the hardened script is installed (sha `95845015c6e9`, matching the repo) and
> `deploy.ps1`/`deploy.sh` sync `/usr/local/bin/wombat-*.sh` on every deploy.
>
> **The nightly job now exits 1, on purpose.** Confirmed by running it: it writes the
> three-part bundle (`database.dump` + `wombat.env` + `keys/`, 104 KB) and then refuses to
> ship it, because `/etc/default/wombat-backup` does not exist. `age` and `rclone` are not
> installed. **So there is still no off-host backup** — everything sits on `/dev/sda` beside
> the database. Expect a nightly cron mail until [T128] is done; that is the control
> working, not a fault.
>
> **What is genuinely fine:** the dumps restore. Rehearsed 2026-09-20 against
> `wombat-2026-09-20.dump` — 444 TOC entries, clean restore, 10 roles / 1 user /
> 31 migrations. Production holds the seeded admin and seed data only, so little is at risk
> *today*. See `execution/tasks/queued/T128-off-host-backup-destination.md`.

**Rehearse a restore.** A backup nobody has restored is a hypothesis. There is a script
for it — `deploy/verify/restore-rehearsal.sh` restores the newest dump into a throwaway
database, counts roles/users/migrations, and drops it, without touching production:

```bash
ssh root@172.236.8.144 'bash -s' < deploy/verify/restore-rehearsal.sh
```

### Two things you cannot recover

Losing `Wombat__PseudonymSalt` permanently breaks every erasure pseudonym already issued.
Losing `/opt/wombat/data/keys` logs every user out on restore. Both are inside the nightly
bundle — which is exactly why the bundle, not the bare dump, is the artefact that matters.

---

## Logs, health, monitoring

```bash
journalctl -u wombat -f              # application (Serilog console sink)
journalctl -u wombat -n 100 --no-pager
tail -f /var/log/caddy/wombat.log    # HTTP
tail -f /var/log/wombat-health.log
tail -f /var/log/wombat-backup.log
systemctl status wombat
curl -i http://127.0.0.1:5080/health
```

Serilog also writes to `/opt/wombat/data/logs/`, rolling daily, kept 30 days.

`/usr/local/bin/wombat-health.sh` probes `/health` every minute from cron; after **three**
consecutive failures it restarts the service and emails via `msmtp`. Since T097 the
endpoint actually checks the database (`AddDbContextCheck`) — before that it was
liveness-only and returned 200 whenever the process was alive, while a PostgreSQL outage or
an exhausted pool left the app unusable and **both the restart cron and the deploy gate
trusted it**. If you add checks, keep the response body terse: `/health` is anonymous and
publicly reachable.

---

## Where the live truth lives

This file goes stale. These do not:

| File | What it is |
|---|---|
| `execution/STATE.md` | **Read this first, every session.** The live handoff: active task, last verified commit, blockers. Newest session at the top. |
| `deploy/README.md` | The operational manual — first-boot checklist, deploys, rollback, useful commands. |
| `deploy/verify/*.sh` | Six read-only checks for the live box: restore rehearsal, audit append-only trigger, DataProtection keys, authenticated smoke test, auth-cookie `Secure` flag, and **`drift-check.sh`** — does the server run what the repo says? None hardcodes a credential. |
| `execution/architecture/INFRASTRUCTURE.md` | The server contract: layout, systemd unit, Caddy, env file, backups, SSO, audit retention. |
| `CLAUDE.md` | Architecture, conventions, footguns, and the "nothing is live" section to delete when that changes. |
| `execution/tasks/<lane>/T0xx-*.md` | One file per unit of work, including every filed-but-unfixed defect. |

---

## Known limitations

Structural, and true as of 2026-09-20:

- **No real users.** See the warning at the top. Everything in the database is scenario
  data.
- **There is no off-host backup** — see the backup section. The nightly job now fails loudly
  rather than silently, but everything still sits on one disk. Top risk; tracked as
  **[T128]**, and expect a cron mail every night until it is closed.
- **Server-side files can drift from the repo.** The cron scripts used to have no deployment
  path at all, which is how T097's backup rewrite sat undeployed for three months. Both
  deploy scripts now sync `/usr/local/bin/wombat-*.sh`, but `wombat.service` and the
  Caddyfile are **still** install-once-by-hand. **Run `./deploy/verify/drift-check.sh`
  before trusting any claim in this document about server behaviour** — it hashes every
  deployed artifact against its repo original and exits non-zero on any difference. Last
  run clean on 2026-09-20.
- **1 vCPU / 1 GB**, half the documented minimum. Fine for scenario replay; resize before a
  real cohort.
- **SSO is built but not activated.** T027 shipped the OIDC wiring, the group-to-role
  mapper and `/admin/sso/group-mappings`. Enabling a provider is **config plus a restart,
  not a redeploy** — it needs an institution's authority, client id/secret and group-to-role
  map in `wombat.env`. The Administrator role can never be assigned via SSO, by design.
- **Integration tests need a PostgreSQL server** (`WOMBAT_TEST_CONNECTION`, else Wombat.Web's user secrets), one
  throwaway `it_<guid>` schema per test, and are not run on the deploy box, so they have never gated a production
  deploy. Dev's server is PostgreSQL 16; production runs 18 (T243).
- **Migrations apply at startup**, so a bad one fails the service rather than the deploy.
- **Filed, unfixed defects live in `execution/tasks/`.** That folder, not this file, is the
  register — `execution/STATE.md` names which are in flight.

---

## First month of real use — do these in order

1. **Configure the off-host encrypted backup leg — [T128].** Nothing else on this list
   matters if the box dies. Generate an `age` keypair whose private half lives on neither
   machine, `apt install age`, create `/etc/default/wombat-backup`, then rehearse a restore
   **from the off-host copy** with `deploy/verify/restore-rehearsal.sh`. The local restore
   already passes; retrieval is the untested half.
2. **Resize the Linode** before the first real cohort. RAM is the constraint.
3. **Delete the "Nothing is live" section from `CLAUDE.md`** the day a real trainee is
   admitted, and re-read every decision it licensed — destructive migrations and re-seeding
   stop being free at that moment.
4. **Rotate the seeded admin password through the admin UI** and confirm a named human owns
   the Administrator role. It cannot be granted via SSO.
5. **Activate SSO** if the first institution has an IdP — config only, no redeploy.
6. **Confirm email end to end** from the live box (port 465, `Email__UseSsl=true`) before
   the first invitation goes to a real address.
