#!/usr/bin/env bash
#
# Post-deploy smoke test — crawls the authenticated surface.
#
# Logs in as the seeded admin (credentials read from /opt/wombat/config/wombat.env
# at runtime — nothing is hardcoded) and fetches every key page, reporting HTTP
# status, whether the response is authenticated, and the page title.
#
# Run on the server, as root. Optional first argument overrides the base URL:
#   Get-Content -Raw deploy/verify/smoke-test.sh | ssh root@<host> "tr -d '\r' | bash -s"
#
# Good looks like: every row HTTP 200 and AUTHED YES. An AUTHED NO means the login
# silently failed and the page fell back to the sign-in screen.
#
# Promoted 2026-09-20 from the 17-19 June first-boot scratch set.
set -uo pipefail
EMAIL=$(grep '^Wombat__SeedAdminEmail=' /opt/wombat/config/wombat.env | cut -d= -f2-)
PW=$(grep '^Wombat__SeedAdminPassword=' /opt/wombat/config/wombat.env | cut -d= -f2-)
BASE="${1:-https://wombat.rcl.co.za}"
JAR=$(mktemp)

HTML=$(curl -sS -c "$JAR" "$BASE/account/login")
TOK=$(printf '%s' "$HTML" | grep -oP 'name="__RequestVerificationToken"\s+value="\K[^"]+' | head -1)
curl -sS -o /dev/null -b "$JAR" -c "$JAR" \
  --data-urlencode "Email=$EMAIL" --data-urlencode "Password=$PW" \
  --data-urlencode "__RequestVerificationToken=$TOK" --data-urlencode "ReturnUrl=" \
  "$BASE/account/login/submit"

pages=( "/" "/admin/users" "/admin/colleges" "/admin/curricula" "/admin/epas" \
        "/admin/adoptions" "/admin/invitations" "/admin/audit" "/admin/jobs" \
        "/admin/institutions" "/account/manage" )

printf '%-24s %-6s %-7s %s\n' PAGE HTTP AUTHED TITLE
printf '%-24s %-6s %-7s %s\n' "------------------------" "----" "------" "-----"
for p in "${pages[@]}"; do
  body=$(curl -sS -b "$JAR" -w '\n__HTTP__%{http_code}' "$BASE$p")
  code=$(printf '%s' "$body" | grep -oP '__HTTP__\K[0-9]+')
  title=$(printf '%s' "$body" | grep -oiP '<title>\K[^<]+' | head -1)
  if printf '%s' "$title" | grep -qi 'sign in'; then authed=NO; else authed=YES; fi
  printf '%-24s %-6s %-7s %s\n' "$p" "${code:-?}" "$authed" "${title:-(none)}"
done
rm -f "$JAR"
echo DONE
