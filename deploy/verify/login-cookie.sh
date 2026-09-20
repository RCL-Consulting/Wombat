#!/usr/bin/env bash
#
# Login + auth-cookie check — the one thing the smoke test does not assert.
#
# Verifies that the Identity.Application cookie is issued WITH the Secure flag when
# the app sits behind Caddy. That depends on forwarded-headers handling: get it
# wrong and the cookie goes out non-Secure over what the app thinks is plain HTTP.
# Credentials are read from /opt/wombat/config/wombat.env at runtime.
#
# Run on the server, as root. Optional first argument overrides the base URL:
#   Get-Content -Raw deploy/verify/login-cookie.sh | ssh root@<host> "tr -d '\r' | bash -s"
#
# Good looks like: "AUTH COOKIE: issued WITH Secure flag". Anything else is a
# regression in the proxy or forwarded-headers configuration.
#
# Promoted 2026-09-20 from the 17-19 June first-boot scratch set (was login-test.sh).
set -euo pipefail
EMAIL=$(grep '^Wombat__SeedAdminEmail=' /opt/wombat/config/wombat.env | cut -d= -f2-)
PW=$(grep '^Wombat__SeedAdminPassword=' /opt/wombat/config/wombat.env | cut -d= -f2-)
BASE="${1:-https://wombat.rcl.co.za}"
JAR=$(mktemp)

echo "=== GET login (through Caddy/HTTPS) ==="
HTML=$(curl -sS -c "$JAR" "$BASE/account/login")
TOK=$(printf '%s' "$HTML" | grep -oP 'name="__RequestVerificationToken"\s+value="\K[^"]+' | head -1)
echo "antiforgery token length: ${#TOK}"

echo "=== POST credentials ==="
HDRS=$(curl -sS -D - -o /dev/null -b "$JAR" -c "$JAR" \
  --data-urlencode "Email=$EMAIL" \
  --data-urlencode "Password=$PW" \
  --data-urlencode "__RequestVerificationToken=$TOK" \
  --data-urlencode "ReturnUrl=" \
  "$BASE/account/login/submit")
echo "$HDRS" | grep -iE '^HTTP/|^location:' | sed 's/[[:space:]]*$//'
if echo "$HDRS" | grep -qi 'set-cookie:.*Identity\.Application'; then
  if echo "$HDRS" | grep -i 'set-cookie:.*Identity\.Application' | grep -qi 'secure'; then
    echo 'AUTH COOKIE: issued WITH Secure flag  (forwarded-headers behind Caddy => OK)'
  else
    echo 'AUTH COOKIE: issued but NOT Secure'
  fi
else
  echo 'AUTH COOKIE: none issued (login failed)'
fi

echo "=== authenticated GET / ==="
HOME=$(curl -sS -b "$JAR" "$BASE/")
echo "home bytes: ${#HOME}"
echo "$HOME" | grep -qiE 'logout|sign ?out|/account/logout' && echo 'home: signed-in (logout present)' || echo 'home: no logout marker'
echo "$HOME" | grep -qiE 'Administration|/admin' && echo 'home: admin nav present' || echo 'home: no admin nav'
rm -f "$JAR"
echo DONE
