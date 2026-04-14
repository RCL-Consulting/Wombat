# Wombat deployment

Target: Ubuntu 24.04 LTS on Linode. Caddy + PostgreSQL + systemd.

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

```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt update
apt install -y aspnetcore-runtime-10.0
```

### 3. PostgreSQL

```bash
apt install -y postgresql-16
sudo -u postgres createuser --pwprompt wombat   # enter a strong random password
sudo -u postgres createdb -O wombat wombat
# Verify:
psql -h 127.0.0.1 -U wombat -d wombat -c 'SELECT 1;'
```

After first migration, revoke mutation rights on the audit table (append-only enforcement):

```bash
sudo -u postgres psql -d wombat -c 'REVOKE UPDATE, DELETE ON "AuditEntries" FROM wombat;'
```

### 4. Config directory

```bash
mkdir -p /opt/wombat/config /opt/wombat/data/logs /opt/wombat/data/uploads
chown -R wombat:wombat /opt/wombat
```

Copy `appsettings.Production.json` from the repo:

```bash
cp appsettings.Production.json /opt/wombat/config/
chown wombat:wombat /opt/wombat/config/appsettings.Production.json
```

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
Wombat__BaseUrl=https://wombat.example.com
Wombat__SeedAdminEmail=renier@rcl.co.za
Wombat__SeedAdminPassword=REDACTED
Wombat__PseudonymSalt=REDACTED
EOF
chmod 600 /opt/wombat/config/wombat.env
chown wombat:wombat /opt/wombat/config/wombat.env
```

**Never rotate `Wombat__PseudonymSalt`** — it is used to generate stable pseudonyms
for erased users. Rotating it breaks linkability across exports and erasure records.

### 5. Deploy the application

From your dev machine (first deploy only — subsequent deploys use `deploy.sh`):

```bash
dotnet publish src/Wombat.Web/Wombat.Web.csproj -c Release -o publish/
rsync -az --delete publish/ wombat-prod:/opt/wombat/app/
```

### 6. Database migration and seeding

```bash
ssh wombat-prod "sudo -u wombat dotnet /opt/wombat/app/Wombat.Web.dll --migrate"
ssh wombat-prod "sudo -u wombat dotnet /opt/wombat/app/Wombat.Web.dll --seed"
```

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

```bash
apt install -y debian-keyring debian-archive-keyring apt-transport-https
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' \
    | gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' \
    | tee /etc/apt/sources.list.d/caddy-stable.list
apt update && apt install -y caddy
```

Append the stanza from `deploy/Caddyfile.wombat` into `/etc/caddy/Caddyfile`,
replacing `wombat.example.com` with the real domain. Then:

```bash
systemctl reload caddy
```

Caddy obtains a Let's Encrypt certificate automatically. DNS must point at the
server before this step, or the ACME challenge will fail.

### 9. Health check cron

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
./deploy/deploy.sh [user@host]
```

The script: publishes locally, rotates the previous release on the server,
rsyncs the new binaries, runs migrations, restarts the service, and
confirms the health check.

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
