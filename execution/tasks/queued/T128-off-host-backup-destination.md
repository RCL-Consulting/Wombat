---
id: T128
title: "The nightly backup is encrypted-capable but has nowhere to go, so it stays on one disk"
status: queued
priority: P1
created: 2026-09-20
---
# T128 — The nightly backup is encrypted-capable but has nowhere to go, so it stays on one disk

**Filed:** 2026-09-20
**Depends on:** an operator decision (destination + who holds the age private key)
**Blocks:** nothing in code — but it is the top operational risk in `Programme/HANDOVER.md`

## Symptom

`/usr/local/bin/wombat-backup.sh` exits **1** every night. Since 2026-09-20 that is correct
and intended: the hardened script refuses to ship an unencrypted bundle containing
`wombat.env` off-host, and refuses to call a local-only copy a backup.

```
[…] ERROR: WOMBAT_BACKUP_AGE_RECIPIENT unset — refusing to ship an UNENCRYPTED bundle containing wombat.env off-host.
[…] Backup complete LOCALLY ONLY — off-host copy did not happen. This is not a backup.
```

Every backup therefore still sits on `/dev/sda`, the same disk as the database it backs up.
One disk failure, theft or fire is simultaneously total data loss *and* total disclosure of
named doctors' competence records.

## What actually happened first (2026-09-20)

The hardened script had been in the repo since T097 (2026-09-16) and **was never deployed**.
The box ran the 2026-06-17 version for 94 days: database dump only, no `wombat.env`, no
DataProtection key ring, no `age`, no off-host — and it exited **0**, so cron reported
success every night and T097's "fails loudly instead" was true only of the repo file.

Root cause was structural: `deploy.ps1`/`deploy.sh` ship only `Wombat.Web` to
`/opt/wombat/app`, and `/usr/local/bin/*` was installed by hand once at first boot. Editing
the repo copy looked effective and was inert — the same shape as the seed-refresher problem
in `CLAUDE.md`.

**Both halves are closed.** The hardened script is installed (sha `95845015c6e9`, matching
the repo) and both deploy scripts now sync `/usr/local/bin/wombat-*.sh` on every run. What
remains is only the destination.

## What to build

Nothing in the application. Three operator steps:

1. **Generate an `age` keypair.** The private half must live on **neither** the Linode nor
   the dev box — otherwise the encryption protects against theft of the backup but not of
   either machine. A password manager or a printed copy in a different physical place.
2. **Pick a destination** and create `/etc/default/wombat-backup` (mode 600):
   ```
   WOMBAT_BACKUP_AGE_RECIPIENT=age1...
   WOMBAT_BACKUP_RCLONE_REMOTE=<remote>:<bucket>/wombat   # or WOMBAT_BACKUP_REMOTE for rsync
   ```
3. **`apt install age`**, plus `rclone` if that is the transport. Both are absent today.

### On the destination

`rsync` to the existing `rcl.co.za` box is the fastest option and the weakest. T097 already
noted it: same single operator, in Dallas, and it also relays all Wombat mail. A correlated
failure — lost credentials, a compromised operator account, a bad night for one provider —
takes both. Object storage via `rclone` (B2, S3, R2) is genuinely independent and is the
recommendation; it needs an account and a bucket, which is why this is a task and not a
session step.

## Verification

- [ ] `/etc/default/wombat-backup` exists, mode 600, and names a recipient plus a transport.
- [ ] `age` (and `rclone`, if used) resolve on the box.
- [ ] `/usr/local/bin/wombat-backup.sh` run by hand **exits 0** and logs
      `Backup complete (local + off-host)`.
- [ ] The `.tar.gz.age` artefact is present at the destination.
- [ ] **Restore rehearsed from the off-host copy, not the local one** — fetch it back,
      `age -d`, untar, and run `deploy/verify/restore-rehearsal.sh` against that dump. A
      backup nobody has restored is a hypothesis; one nobody has *retrieved* is worse.
- [ ] The private key is demonstrably not on either machine.

## Why it is needed now

Production holds the seeded admin and seed data only, so little is at risk **today** — the
2026-09-20 rehearsal restored cleanly with 10 roles, 1 user, 31 migrations. The reason not
to defer again is `Wombat__PseudonymSalt`: it can never be rotated, its loss permanently
breaks the linkability of every POPIA erasure pseudonym ever issued, and until 2026-09-20 it
was in no backup at all. Its only copy outside the server is `pwd_DO_NOT_COMMIT.txt` on one
dev machine.

This must be closed before the first real trainee is admitted, alongside deleting CLAUDE.md's
"nothing is live" section.

## Related

- `execution/tasks/done/T097-security-posture-hardening.md` §4 and its 2026-09-20 correction
- `Programme/INFRASTRUCTURE.md` § Backups
- `Programme/HANDOVER.md` § Backups — first-month item 1
- `deploy/verify/restore-rehearsal.sh`
