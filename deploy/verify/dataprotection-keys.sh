#!/usr/bin/env bash
#
# DataProtection key ring — diagnoses the failure that logs everyone out.
#
# The service user is homeless and ProtectSystem=strict makes the default
# $HOME/.aspnet unwritable, so without Wombat__DataProtectionKeysPath pointing
# inside ReadWritePaths the key ring is regenerated on every start and every
# session cookie and antiforgery token is invalidated on restart.
#
# Run on the server, as root:
#   Get-Content -Raw deploy/verify/dataprotection-keys.sh | ssh root@<host> "tr -d '\r' | bash -s"
#
# Good looks like: key-*.xml under /opt/wombat/data/keys, and the writability
# probe answering yes. Keys found anywhere else means the path is misconfigured.
#
# Promoted 2026-09-20 from the 17-19 June first-boot scratch set (was check-dp.sh).
set -uo pipefail
echo "wombat passwd entry:"; getent passwd wombat
echo
echo "HOME as wombat sees it:"; sudo -u wombat bash -lc 'echo HOME=$HOME; ls -ld $HOME 2>/dev/null || echo "(home not accessible)"'
echo
echo "DataProtection-Keys dirs anywhere on disk:"
find / -type d -name 'DataProtection-Keys' 2>/dev/null || true
echo
echo "any key-*.xml files:"
find / -type f -name 'key-*.xml' 2>/dev/null | head -20 || true
echo
echo "is /opt/wombat/data writable by wombat? (ReadWritePaths target):"
sudo -u wombat bash -lc 'touch /opt/wombat/data/.wtest && echo yes && rm -f /opt/wombat/data/.wtest' 2>&1 || echo no
echo DONE
