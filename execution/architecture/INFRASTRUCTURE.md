# Infrastructure — Linode deployment

Wombat runs on a Linode VPS. The layout mirrors ClinicAssist.NET's production setup.

> **⚠ Read `deploy/README.md` first.** That file documents the deployment **as actually built
> and running**; this file is the design rationale. Where the two disagree, `deploy/README.md`
> wins. This document was corrected against the live server on 2026-09-16 (T097) after an audit
> found several instructions here that would silently produce a broken box — in particular
> environment-variable names that do not bind to anything the application reads.

## Target

- **One Linode VPS.** Specified: Ubuntu 24.04 LTS, 2GB RAM minimum (4GB recommended if Blazor
  circuits grow). **As built:** Ubuntu **26.04 LTS**, 1 vCPU / **1GB** RAM — half the specified
  minimum — mitigated with a 1GB swapfile and `DOTNET_gcServer=0` (workstation GC). It has run
  for 90+ days at that size. Resize before onboarding a real cohort: Blazor Interactive Server
  holds a circuit per connected user in memory, so RAM is the scaling constraint.
- **Packages come from the Ubuntu 26.04 distro repos** — `aspnetcore-runtime-10.0`,
  `postgresql` (18.x) and `caddy` are all packaged. **No Microsoft or Cloudsmith APT repo is
  needed**, which is a significant simplification over what this document originally specified.
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

**Forwarded headers are believed from loopback only (T156).** Caddy reaches Kestrel over loopback
(`ASPNETCORE_URLS=http://127.0.0.1:5080`) and names the client in `X-Forwarded-For` and the scheme in
`X-Forwarded-Proto`. The app believes them from `127.0.0.1` and `::1` alone (`KnownProxies`; an IPv4 address mapped into
IPv6 counts as the IPv4 one). From any other peer they are ignored, so binding Kestrel beyond loopback, or putting another
proxy in front of it, needs `KnownProxies` changed in `Program.cs` too, or every client is counted as the proxy.

**The sign-in throttle (T097, T149, T265; T156).** Ten failed password checks in five minutes from one client address,
shared by the sign-in page, the link-your-account page and the change-password page. An IPv4 client is its whole address,
an IPv6 client its /64 (`ClientAddress`, the same client the MSF respondent limits count). A success costs nothing: a
check is counted when it begins, so no more than ten are ever in flight, and taken off the count only when its password
is right. Every other outcome stays counted, a refusal made without a check included (an account that signs in only
through its institution, or an address the link page will not link), so the count cannot say which addresses have
accounts. A refusal is a redirect with a `Retry-After` of the seconds left in the window: back to the sign-in or
change-password form, and from the link page to the sign-in page, ending the institutional sign-in in progress, which
would expire before the window did (the external cookie, below). The count lives in the process, so
a restart forgets it. Identity's lockout (five failures, fifteen minutes) caps the guesses at one account; this caps them
from one client. Audit rows keep the address's /24 (IPv6 /48), never the whole of it.

Every path goes to Wombat.Web on 5080, which is all the MSF respondent page needs (`/msf/respond`, T205): it is a page
of Wombat.Web, anonymous, static (no Blazor circuit) and rate-limited in the app, so Caddy carries no route, header or
exemption for it. The access log (`/var/log/caddy/wombat.log`) records each request's full URI, so a respondent's
unused link (`/msf/respond?token=…`) is readable there, as a registration link (`/account/register?token=…`) always has
been. Both die once used; filtering `token` out of the log is not done. The page sends `Referrer-Policy: no-referrer`,
so the link is not also copied into the `Referer` of the stylesheets and scripts it loads (the app's own policy,
`strict-origin-when-cross-origin`, sends the full address on a same-origin request). If filtering is ever wanted, it
must cover `request>uri` and `request>headers>Referer` both: the registration page still sends its link as a Referer.
The page also sends `X-Robots-Tag: noindex, nofollow`: a leaked link must not put a trainee's name in a search engine.

Caddy handles TLS automatically via Let's Encrypt. No manual cert management.

## Database

Two options:

1. **Local Postgres** on the same VPS — simpler, cheaper, fine for Phase 1. **This is what runs.**
2. **Linode Managed Postgres** — pay for it when uptime matters more than $.

For Phase 1, use local Postgres. On Ubuntu 26.04 the distro package is **PostgreSQL 18**:

```bash
sudo apt install postgresql          # 18.x on 26.04 — do NOT pin postgresql-16
sudo -u postgres createuser --pwprompt wombat
sudo -u postgres createdb -O wombat wombat
```

> **Version matters for restores.** The live cluster is 18.x and backups are taken with
> `pg_dump` 18. A dump from 18 will **not** restore into a 16 cluster, so a rebuild that
> installs 16 fails at exactly the moment you need it to work.
>
> **It matters for error codes too (T243).** On 18, a delete refused by an `ON DELETE RESTRICT`
> foreign key reports SQLSTATE `23001`, where 16 reports `23503`. The dev server was 16.10 on
> 2026-09-25, so code and tests go through `PostgresErrors.IsForeignKeyViolation`, which accepts
> both (ARCHITECTURE.md § Testing). `deploy/verify/drift-check.sh` prints the server's version
> and counts a major other than 18 as drift.

Connection string in `wombat.env` — note the name, which is what ASP.NET Core's
`GetConnectionString("DefaultConnection")` actually reads:

```
ConnectionStrings__DefaultConnection=Host=127.0.0.1;Database=wombat;Username=wombat;Password=...
```

> **Not** `Wombat__ConnectionStrings__Default`, which this document specified until 2026-09-16.
> That name binds to nothing: `WombatOptions` has no `ConnectionStrings` property, so the app
> would fall through to whatever `appsettings.json` provides and silently target the wrong
> database. (T097 removed the committed dev fallback, so it now fails fast instead.)

### After a migration that removes personal data

A migration that drops or nulls a column does not take the old values off the disk. `DROP COLUMN`
only hides the column: each value stays in its row until the row is next written. An `UPDATE`
leaves the old row version behind as a dead row, and a plain `VACUUM` frees that space without
overwriting it. Only a table rewrite removes the old bytes, so run one after the deploy that applies
the migration:

```bash
sudo -u postgres psql -d wombat -c 'VACUUM FULL "MsfInvitations";'   # T207
```

`VACUUM FULL` locks the table while it runs; these tables are small. Two copies are out of its reach:

- **The WAL.** Old segments are recycled after the next checkpoints.
- **Dumps taken before the deploy.** They age out with the backup rotation (§ Backups), which keeps
  them up to six months.

| Migration | Table | What it removed |
|---|---|---|
| T207 (`T207_DropMsfRespondentEmailHash`) | `MsfInvitations` | The unsalted SHA-256 of each anonymised MSF respondent's address. The migration rewrites every row that held one, then drops the column. |

### After T163: MSF respondent links mailed before the deploy stop working

Since T163 a respondent link's token begins with a selector, which the database holds in the clear under a unique index
(`MsfInvitations.TokenSelector`), so the public respondent page reads one row per request instead of every invitation.
The migration gives no invitation already stored a selector, so **every link mailed before the deploy is answered "not
recognised"**. Nothing is live, so this was accepted rather than keeping the old scan for old links (W-007).

A link is issued only when a campaign opens and by the expiry reminder. The reminder replaces a link at most once, and
only a link issued **before** the first reminder day (two days before the respondent's last day to respond;
`MsfInvitation.IsReminderDue`). So an unanswered respondent of a campaign open at the deploy falls into one of two groups:

- **Link issued before the first reminder day:** dead until the reminder mails a new one, from that day on.
- **Link issued on or after it** (the campaign opened late in its window, or the reminder already went out): no reminder
  will come, so this respondent can never answer. The only remedy is to withdraw the campaign (which is final) and open
  a new one for the trainee.

List them before deploying; `reminded` is false for the second group:

```bash
sudo -u postgres psql -d wombat <<'SQL'
SELECT c."Id" AS campaign, i."Id" AS invitation, c."ClosesOn",
       i."IssuedOn" < ((LEAST(c."ClosesOn", i."ExpiresOn") - 2)::timestamp AT TIME ZONE 'UTC') AS reminded
FROM "MsfInvitations" i JOIN "MsfCampaigns" c ON c."Id" = i."CampaignId"
WHERE c."State" = 1                                   -- Open
  AND i."RespondedOn" IS NULL AND i."RevokedOn" IS NULL AND COALESCE(i."RespondentEmail", '') <> ''
ORDER BY c."Id", i."Id";
SQL
```

**Rolling the migration back** (`Down`) drops every selector, and the pre-T163 build accepts only a 43-character token,
so every link mailed after the deploy dies too. Restoring the pre-deploy `pg_dump` (the planned rollback) does the same
to those links, and brings back the ones mailed before.

### After T214: a link a reminder replaced keeps working until the last day to respond

Since T214 the reminder keeps the link it replaces (`MsfInvitations.PreviousTokenSelector`, under a unique index, and
`PreviousTokenHash`), and that link takes the respondent's one response until their last day to respond. Answering
through either link, and closing or withdrawing the campaign, retires it.

A second check, `CK_MsfInvitations_PreviousLinkUnanswered`, keeps a previous link off an answered invitation. The reminder
job stores a new link only after it has mailed it, so an answer, a close or a withdrawal can commit in between. The
job's store is then refused, by that check or by the campaign's `xmin`. It logs a warning, `…while its reminder was being
sent, so the reminder's link was not stored`, and goes on to the next respondent. That respondent's mail carries a link
that opens nothing. They have answered, or their campaign has been closed or withdrawn. The job mails nobody else of a
campaign it finds closed or withdrawn. Any other refused store stops the run, as before T214. A close or withdrawal that
read the campaign before a reminder was stored is refused the same way, and says the campaign changed; close or withdraw
it again.

- **Nothing to do at deploy.** Every invitation already stored gets no previous link, so a link a reminder replaced
  before the deploy stays retired. Nor does T214 revive a link mailed before T163: the reminder keeps only a link that
  carries a selector.
- **One wrong sentence, for campaigns opened before T163 only.** The reminder tells every respondent that their original
  link "also works until" the last day. For an invitation mailed before T163 it does not: that link has no selector, and
  § After T163 says the reminder's link is their only one. Nothing is live (W-007), so the email is not written around
  those rows. Close or withdraw such a campaign, or accept the sentence until it closes.
- **Rolling back** (`Down`, or restoring the pre-deploy dump) retires every previous link again. `Down` leaves the
  current links as they are.

### After T251: what became of each MSF link's mail

Since T251 the mail worker reports each MSF link's mail onto its invitation (`MsfInvitations.SentOn`,
`DeliveryFailedOn`, and `DeliveryLinkSelector`, the link the report is about; `CK_MsfInvitations_DeliveryOutcome` keeps
sent and dropped apart). An open campaign's page counts the links not delivered and offers Resend. A link counts as not
delivered when its mail was dropped after three attempts, when it was still queued as the app stopped (the queue is in
process; a stop in order reports it), or when nothing was heard of it for an hour (the app crashed, or stopped while
that mail was being sent).

- **Nothing is backfilled.** Whether a link mailed before the deploy arrived is not known. So every unanswered link of a
  campaign open at the deploy that was issued more than an hour before it reads as **not delivered**, and its page offers
  Resend. Only scenario data is affected (W-007). Resending mails each respondent a new link and keeps the old one
  working as the previous link, so a respondent who did receive it loses nothing.
- **A deploy restarts the app.** Mail still queued at the restart is reported dropped, and its campaign offers Resend. The
  log line is `Email 3f9a1c2b7d4e (tags: msf-invite, campaign:5) abandoned unsent: the app is shutting down.` The one
  mail being sent at that moment is not reported (T251 review): it may have reached the mail server, so its link reads
  as still being sent for an hour, then as not delivered, and a Resend keeps it working as the previous link. Its log
  line is `Email … (tags: …) cut off by the app's shutdown; whether it was sent is not known.`
- **A host with no `Email:SmtpHost`** (`appsettings.json`'s default) logs each mail instead of sending it
  (`LoggingEmailSender`) and runs no mail worker, so nothing is ever reported. Every MSF link there reads as still being
  sent for an hour, then as not delivered, and a Resend only logs another. That is true, since nobody was mailed. Dev
  sets `localhost` (the SMTP sink on port 25) and production sets its relay, so neither is affected; a host that
  should send mail and shows this has lost its `Email__SmtpHost`.
- **An address that the mail server always refuses** (a mistyped domain) stays not delivered however often it is resent.
  The page never says whose link it is, so the remedy is to withdraw the campaign and invite again. Since T282 the log
  does not name the address either: `Email … (tags: msf-invite, campaign:5) failed after 3 attempts. Message dropped.`,
  with the server's reply beneath it as the exception and the address in it replaced by `[recipient]` (`5.1.2
  <[recipient]>: Recipient address rejected: Domain not found`). The campaign tag and the reason are what to go on.
- **Rolling back** (`Down`) drops the three columns and the check; nothing else reads them.

### Reading the mail log (T282)

Every line about a mail names it by a reference drawn for that mail (twelve hex digits) and its tags, never by its
address or its subject. The same reference is on the line that queued it (`Debug`), on each failed attempt (`Warning`),
and on its outcome: `Email … (tags: …) sent on attempt N.` (`Information`), `… failed after 3 attempts. Message
dropped.` (`Error`), `… cut off by the app's shutdown`, `… abandoned after attempt N` or `… abandoned unsent`
(`Warning`), or `The outcome of email … could not be recorded.` (`Error`). So `journalctl -u wombat | grep 3f9a1c2b7d4e`
follows one mail, and `grep campaign:5` one campaign's mail. Two mails to one person carry two references, so the log
cannot say that two mails went to the same person; that is the point, since an MSF reminder goes only to those who have
not answered. An exception about a mail is logged with the mail's addresses replaced by `[recipient]`: each as given, with
its domain in either form (an international domain goes to the server, and comes back quoted, as `xn--…`), and its local
part where a server quotes that alone (`<[recipient]>... User unknown`).

### After T283: what became of each account invitation's mail, and the jobs' mail

Since T283 the mail worker reports each account invitation's mail onto the invitation (`Invitations.SentOn`,
`DeliveryFailedOn`, and `DeliveryFailures`, how many of its mails the mail server was offered and the worker then gave
up on, which leaves out a mail abandoned still queued as the app stopped; `CK_Invitations_DeliveryOutcome` keeps sent and
dropped apart). `/admin/invitations` has a Delivery column, and a row not delivered offers Resend, which
emails a new link and retires the old one. The rules are T251's: dropped after three attempts, still queued as the app
stopped, or nothing heard for an hour.

- **Nothing is backfilled.** Every invitation still open at the deploy that was issued more than an hour before it
  reads as **not delivered**, and its row offers Resend. Only scenario data is affected (W-007). A resend retires the
  old link: an invitee who did receive the first mail must use the new one.
- **An address the mail server always refuses** stays not delivered however often it is resent. From the second failure
  the row says to check the address; the remedy is to revoke the invitation and issue a new one to the right address.
- **A host with no `Email:SmtpHost`** reports nothing, so every invitation there reads as being sent for an hour, then
  as not delivered, as MSF links do (§ After T251).
- **The nudges and the weekly digest** log a second line per run once the worker has reported on every mail the run
  handed over: `ActivityDraftNudgeJob mail of the run started 2026-09-28 07:00:00Z: sent 38, not delivered 2.` (a
  warning when any was not delivered). A run whose mail is not all reported (no mail worker, or a stop mid-send) is
  logged with `not reported before the job ran again N` when the same job next runs (a "Run now" while the last run's
  mail is still being retried says so too), or with `never reported N` as the app stops. Which recipients were not
  delivered is in the worker's own lines, by mail reference, never by address (T282).
- **Rolling back** (`Down`) drops the three columns and the two checks; nothing else reads them.

## Environment file

`/opt/wombat/config/wombat.env` (mode 600, owner wombat:wombat):

```
ConnectionStrings__DefaultConnection=Host=127.0.0.1;Database=wombat;Username=wombat;Password=REDACTED
Email__SmtpHost=smtp.example.com
Email__SmtpPort=587
Email__SmtpUser=wombat@example.com
Email__SmtpPassword=REDACTED
Email__FromAddress=no-reply@example.com
Email__FromName=Wombat
Email__UseSsl=false
Wombat__BaseUrl=https://wombat.example.com
Wombat__MsfRespondUrl=https://wombat.example.com/msf/respond
Wombat__AllowSelfRegistration=false
Wombat__SeedAdminEmail=admin@example.com
Wombat__SeedAdminPassword=REDACTED
Wombat__PseudonymSalt=REDACTED
Wombat__DataProtectionKeysPath=/opt/wombat/data/keys
DOTNET_gcServer=0
```

The double-underscore syntax is ASP.NET Core's convention for nesting. Never commit this file.

**Names corrected 2026-09-16.** `Email` and `ConnectionStrings` are **top-level** configuration
sections, not children of `Wombat` — `EmailSettings.SectionName` is `"Email"`. The old
`Wombat__Email__*` names bound to nothing, so email silently did not work.

- **`Email__UseSsl`** — set `true` for implicit-SSL submission on **port 465**. The live
  deployment uses 465 with `UseSsl=true`. Leaving it at the `false` default while using 465
  produces a connection that never completes.
- **`Wombat__DataProtectionKeysPath`** — **required in production.** The service user is homeless
  and `ProtectSystem=strict` makes the default `$HOME/.aspnet` unwritable, so without this the
  DataProtection key ring is regenerated on every start and every user is logged out on each
  restart. **Back this directory up** (T097's backup script now includes it).
- **`Wombat__SeedAdminPassword`** — only consumed the first time the admin user does not exist;
  `AdminSeeder` returns early if it does. Changing it later has no effect. Rotate the live
  password through the admin UI, not by editing this value.
- **`Wombat__MsfRespondUrl`** — optional; **leave it unset.** The absolute address of the MSF
  respondent page on this host. Unset, it is `{Wombat__BaseUrl}/msf/respond`, for this deployment
  `https://wombat.rcl.co.za/msf/respond` (T205). Set, it must be on `Wombat__BaseUrl`'s scheme, host
  and port, or opening a campaign and the expiry reminder refuse, saying why: a wrong value (the Api's
  `:5090`, `http` for an `https` site, another host) used to open the campaign without complaint and
  mail every respondent a link nobody could use. With neither set, opening a campaign refuses to send
  anything (T132). Every invitation and every expiry reminder links to `{MsfRespondUrl}?token=…`.
  `deploy/verify/drift-check.sh` prints both values from the server's `wombat.env`.
  The page is Wombat.Web's own (T205): anonymous, rendered as plain server-side HTML with form
  posts (no circuit, no script needed), and limited to 10 requests a minute per link per client
  address, and 60 a minute per client address whatever the links (an IPv6 client by its /64). A
  link that is used, expired, revoked or of a closed request gets a page saying so, with a 404 or
  410. `deploy/verify/smoke-test.sh` probes it anonymously.
  - Dev (`appsettings.Development.json`): `http://localhost:5080/msf/respond`, the web app.
  - **Never point it at the Api host.** `Wombat.Api` answers `/msf/respond` too, as JSON: it is
    the integration endpoint, sending the same query and command with the same rate limit and
    refusal statuses. It is not deployed (`deploy.ps1` and `deploy.sh` publish Wombat.Web only),
    and a respondent sent there gets JSON, not a questionnaire. Until T205 dev pointed there
    (port 5090) and production had no page at all.

### SSO (optional, not currently configured)

Nothing SSO-related is set on this deployment, so no OIDC handler is registered. To enable a
provider, add an indexed block and restart — config only, no redeploy:

```
Sso__Providers__0__Key=myuni
Sso__Providers__0__DisplayName=My University
Sso__Providers__0__InstitutionId=1
Sso__Providers__0__Authority=https://login.example.edu
Sso__Providers__0__ClientId=REDACTED
Sso__Providers__0__ClientSecret=REDACTED
Sso__Providers__0__GroupsClaim=groups
Sso__Providers__0__Scopes__0=openid
Sso__Providers__0__Scopes__1=profile
Sso__Providers__0__Scopes__2=email
Sso__Providers__0__EmailVerifiedClaim=email_verified
```

`EmailVerifiedClaim` is the claim by which the provider says its `email` claim is verified. It defaults to
`email_verified` (OIDC Core § 5.1), so it is needed only for a provider that says it another way; Entra ID's is below.

**`Wombat__PseudonymSalt`** — used by the erasure executor (T026) to generate deterministic pseudonyms for erased users (`deleted_user_<hex>`). This is a deployment secret. **Do not rotate it** — rotating the salt breaks pseudonym stability across exports and makes previously-issued pseudonyms unlinkable to erasure records.

An erasure is one database transaction (T258): every step or none, and the request is left as it was, to be approved
again. Every Identity write's result is checked too (T156), because the user store reports a refusal as a result, not an
exception: a concurrency conflict on the account is refused as `ErasureExecutor.PersonChanged`, as one on a review or a
campaign is, and any other refusal fails the erasure naming the step. See CUSTOMIZATION.md § Data subject rights.

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

The export cron is **not** part of the application binary — it is a server-level script
(`/usr/local/bin/wombat-audit-cold.sh`) that connects to PostgreSQL directly. This keeps cold
storage logic out of the app's attack surface.

> **Status: not written, not installed.** No such script exists in `deploy/` or on the server.
> Nothing depends on it yet — the oldest audit entries date from 2026, so the 7-year transition
> is not due until 2033. Written up here as a design note, not as something you can rely on.

### Append-only enforcement

The migration that creates `AuditEntries` installs a PostgreSQL trigger
(`audit_entries_immutable`). **Since T096 (migration `20260619065511_T096_AuditDeleteForArchival`)
it no longer blocks everything:**

- Every `UPDATE` still raises.
- `DELETE` raises **unless** the transaction has set the custom GUC
  `SET LOCAL wombat.allow_audit_delete = 'on'`.

`AuditLogRetentionJob` opts in explicitly inside its own transaction; nothing else does. That
was necessary because the original blanket trigger made the archival job impossible — it would
have started failing silently the first time rows aged past the 2-year cutoff.

> **🚫 Do NOT run `REVOKE UPDATE, DELETE ON "AuditEntries" FROM wombat;`**
> This document recommended it until 2026-09-16. It is redundant (the trigger enforces the
> invariant) and **actively harmful** — a table-level revoke cannot be bypassed by the GUC, so
> it re-breaks archival exactly as T096 fixed it. It is not applied on the live server.

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
      "EmailVerifiedClaim": "xms_edov",
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
| **Microsoft Entra ID** | Groups claim emits object IDs by default, not display names. Map by object ID in `SsoGroupRoleMappings`; keep display names for admin UX. Enable "Group claims" in the app registration's Token Configuration. Entra sends no `email_verified`: add the `xms_edov` optional claim (email domain owner verified) in Token Configuration and set `EmailVerifiedClaim` to `xms_edov`. Without it no email from Entra counts as verified, so Entra can neither provision nor link (see the invariants below). `xms_edov` is documented as a boolean and has been seen as the string `"1"`; both read as verified. **Needs confirmation:** check one real Entra ID token's `xms_edov` before the first Entra institution goes live. |
| **Google Workspace** | No native groups claim. Use Google Directory API to populate groups, or map by domain/OU. |
| **Shibboleth** | SAML at the wire level. Point an OIDC bridge (Keycloak, Auth0, or a SAML-to-OIDC adapter) at the Shibboleth IdP. Wombat talks to the bridge over OIDC. Do not implement SAML directly. |

### Security invariants

- `state` and `nonce` parameters are enforced by ASP.NET Core's OIDC handler.
- Valid issuer list is strictly from configured authorities.
- Clock skew tolerance: 2 minutes.
- **Administrator role cannot be assigned via SSO.** Even if the mapping table maps a group to Administrator, the `SsoGroupMapper` logs a warning and skips it. Administrator requires explicit manual assignment.
- SSO-provisioned users have `AllowLocalPassword = false` — they cannot set a local password and are refused by the local login form.
- **An email the provider does not verify is never written (T155).** The provider must assert its `EmailVerifiedClaim`
  as true (`true` in any case, or `1`), in every such claim it sends. Only then does a sign-in copy a changed email to the account, as its email and user name, in one
  validated write that rotates the security stamp. An address another account holds, as its email or its user name, is
  refused before anything changes, and so is anything Identity's validators refuse. Both accounts stay as they were, the
  sign-in goes ahead, and the refusal is logged and audited (`SsoEmailSyncRefused`). A change that is written is audited
  as `SsoEmailChanged`. A refused Identity update is reloaded before anything else saves, because the audit writer's save
  and the group mapper's would otherwise commit it.
- **A first sign-in provisions an account only from a verified email that no account holds (T155).** Otherwise it is
  refused (`SsoProvisioningRefused`, stamped with the provider's institution), and the person is told to ask for an
  invitation. The unverified refusal comes before any lookup, so it reads the same whether or not an account holds the
  address.
- **The external cookie does not slide, and is a `__Host-` cookie (T156).** It holds a provider's sign-in from the
  callback until the account is signed in or linked, and ends five minutes after the callback however often the link
  page reads it: until T156 each read past the halfway mark issued it again, so a sign-in could be kept alive past its
  revocation at the provider. Its name is `__Host-Identity.External`, always `Secure`, `Path=/`, no `Domain`, so another
  `rcl.co.za` subdomain cannot plant one. A browser treats `http://localhost` as a secure origin, so development over
  plain HTTP keeps working (observed in Chrome). The application and antiforgery cookies keep their names.
- **A wrong password, an unknown address and an institutional account read the same (T156).** All three get "Invalid
  email or password." (with a pointer at the institution's button where the page offers one) and the same unstamped
  `LoginFailed` row; no password is checked for the institutional account. A first sign-in whose new account Identity
  refuses is told so in Wombat's words, never Identity's, which quote the address: a taken address as the in-use check
  says, anything else as "could not create". **The lockout reply still differs:** Identity answers a locked account
  before it checks a password, so an account an administrator has deactivated gets "Too many failed sign-in attempts…"
  at the first try, and an active local account gets it after five wrong guesses, while an unknown or institutional
  address never does. That is left as it was: whether a locked-out person is told so is its own decision.
- **Only a verified email is matched to an account for linking (T155).** A link outlives a password change, so matching
  an unverified email would let anyone who can set their own address at the provider, and has learnt the account's
  password once, keep a way in. The callback offers no link for an unverified email (it is refused as above), and the
  link endpoint, which any external cookie reaches without the callback, refuses it before it looks up an account or
  checks a password. A provider that verifies nothing therefore neither provisions nor links; it still signs in the
  accounts already linked to it.
- Break-glass: at least two local-password Administrator accounts must exist independent of SSO.

### Callback URLs

Each provider gets its own callback path: `/signin-oidc-{key}` (e.g. `/signin-oidc-uct`). Register this as the redirect URI in the IdP's app registration.

### Group-to-role mapping

Managed by administrators at `/admin/sso/group-mappings`. Each mapping links a provider + external group ID to a Wombat role, institution, and optional speciality/sub-speciality scope. Changes take effect on the next login for each user.

## Backups

`deploy/wombat-backup.sh` bundles **three** things nightly, because a database-only backup
restores to a box that boots and then cannot honour its own erasure records:

1. **Database** — `pg_dump -Fc`. Retain 14 daily, 4 weekly (Sunday), 6 monthly (1st).
2. **`/opt/wombat/config/wombat.env`** — holds `Wombat__PseudonymSalt`, which is **never
   rotatable**. Losing it permanently breaks the linkability of every POPIA erasure pseudonym
   already issued. It cannot be reconstructed from anything.
3. **`/opt/wombat/data/keys`** — the DataProtection key ring. Losing it invalidates every
   session cookie and antiforgery token on restore.

The bundle is **`age`-encrypted before it leaves the box** (it contains `wombat.env`), to a
recipient whose private key lives on **neither** machine, then shipped off-host via `rclone` or
`rsync`.

> **A backup on the same disk as the database is not a backup.** One disk failure, theft or fire
> is simultaneously total data loss *and* total disclosure of named doctors' competence records.
> Until T097 the off-host step existed only as a comment in the script and had never run.

The script **exits non-zero** if the off-host leg is unconfigured or `age` is missing, so cron
mails you. It refuses to ship `wombat.env` unencrypted. Configure in `/etc/default/wombat-backup`
(mode 600): `WOMBAT_BACKUP_AGE_RECIPIENT`, plus one of `WOMBAT_BACKUP_RCLONE_REMOTE` or
`WOMBAT_BACKUP_REMOTE`.

> **This described the repo file rather than the server until 2026-09-20.** For 94 days
> `/usr/local/bin/wombat-backup.sh` was the 2026-06-17 version (1924 B vs 6321 B): database
> dump only, no `wombat.env`, no key ring, no `age`, no off-host, exit 0. The cron scripts
> had **no deployment path** — `deploy.ps1`/`deploy.sh` ship only `Wombat.Web` to
> `/opt/wombat/app`, and `/usr/local/bin/*` was installed by hand once at first boot — so
> T097's rewrite never left the repo.
>
> **Both fixed:** the hardened script is installed (sha `95845015c6e9`) and both deploy
> scripts now sync `/usr/local/bin/wombat-*.sh` every run. The nightly job consequently
> **exits 1** until `/etc/default/wombat-backup` exists — correct behaviour, tracked as
> **T128**, and there is still no off-host copy.
>
> `wombat.service`, `Caddyfile.wombat` and `appsettings.Production.json` remain
> install-once-by-hand. Before trusting a claim about server behaviour, `sha256sum` the
> deployed file against the repo.

Cron — installed to `/etc/cron.d/wombat-backup`, where the **user field is mandatory**:

```
0 2 * * * root /usr/local/bin/wombat-backup.sh >> /var/log/wombat-backup.log 2>&1
```

**Rehearse a restore.** A backup nobody has restored is a hypothesis, not a backup.

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

`/health` is probed every minute by `deploy/wombat-health.sh` (cron); after 3 consecutive
failures it restarts the service and emails you via `msmtp`.

**Since T097 the endpoint actually checks the database** (`AddDbContextCheck`). Before that it
was liveness-only — `AddHealthChecks()` with zero registered checks — so it returned 200
whenever the process was alive. A PostgreSQL outage, a bad connection string, or an exhausted
pool left `/health` reporting "Healthy" while the app was unusable, and **both the restart cron
and the deploy gate trusted it**. If you add further checks, keep the public response body
terse: the endpoint is anonymous and reachable at `https://wombat.rcl.co.za/health`.

## Rollback

- Keep the last two published binary directories: `/opt/wombat/app.prev/` is the previous release.
- Rollback = `rm -rf /opt/wombat/app && mv /opt/wombat/app.prev /opt/wombat/app && systemctl restart wombat`.
- For database migrations, rollback is manual — if a migration is destructive, restore from the nightly dump. Prefer additive migrations.

## First-boot checklist

When setting up a fresh VPS:

1. Fresh **Ubuntu 26.04**, apply all updates, set hostname. Add a **1GB swapfile** if the plan
   has 1GB RAM.
2. Create `wombat` system user.
3. `apt install aspnetcore-runtime-10.0` — from the **distro repos**. No Microsoft APT repo.
4. `apt install postgresql` (**18.x**), create the database and user.
5. `apt install caddy` — distro repo; no Cloudsmith repo needed.
6. Copy `wombat.env` to `/opt/wombat/config/` (mode 600). Create `/opt/wombat/data/keys`,
   owned by `wombat`.
7. Publish from dev and ship to `/opt/wombat/app/` — use `deploy/deploy.ps1` (Windows) or
   `deploy/deploy.sh`.
8. Install the systemd unit, `systemctl daemon-reload`, `systemctl enable --now wombat`.
9. Install the Caddyfile stanza, `systemctl reload caddy`.
10. **Migrations need no separate step.** The service applies them on startup via
    `Database.MigrateAsync()`, reading config from systemd's `EnvironmentFile`.
    > 🚫 Do **not** hand-run `dotnet Wombat.Web.dll --migrate` expecting it to pick up
    > `wombat.env`. The flag exists, but a one-shot process does not inherit the service
    > environment, and sourcing the file from bash mis-splits the connection string on its
    > `;`. This document prescribed that step until 2026-09-16; it was the actual bug fixed
    > on 2026-06-19.
11. Install the health and backup crons to `/etc/cron.d/` (**user field required**), and
    configure `/etc/default/wombat-backup` so the off-host backup leg actually runs.
12. Visit `https://wombat.example.com`, log in as the seeded admin, issue an invitation.
13. **Create a second local-password Administrator** before enabling SSO — see the break-glass
    invariant above. Only one admin is seeded.

That checklist is the verification for T015. Put it in the task file too.
