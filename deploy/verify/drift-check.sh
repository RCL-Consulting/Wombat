#!/usr/bin/env bash
#
# Drift check — does the server actually run what this repo says it does?
#
# **This one is different from its siblings.** The other scripts in deploy/verify/ are
# piped to the server and run there. This one runs **locally** and reaches out over SSH,
# because a comparison needs both sides.
#
#   ./deploy/verify/drift-check.sh [user@host]     # default: root@172.236.8.144
#
# Why it exists: on 2026-09-20 /usr/local/bin/wombat-backup.sh turned out to be the
# 2026-06-17 version — T097 had rewritten deploy/wombat-backup.sh three months earlier and
# nothing deployed it, so the docs described a control the box did not have, and cron
# reported success every night. Nothing would have caught that but a comparison.
#
# Good looks like: every row MATCH, exit 0. A DRIFT row prints the diff, so you can judge
# whether it is a stray comment or a missing control. Exit 1 if anything differs.
#
# Comparison is CRLF-insensitive: a Windows checkout is CRLF and the deploy strips it.
set -uo pipefail

REMOTE="${1:-root@172.236.8.144}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SSH_OPTS=(-o BatchMode=yes -o ConnectTimeout=15)

# local-repo-path : deployed-path : how it gets there
PAIRS=(
    "deploy/wombat-backup.sh:/usr/local/bin/wombat-backup.sh:deploy script"
    "deploy/wombat-health.sh:/usr/local/bin/wombat-health.sh:deploy script"
    "deploy/wombat.service:/etc/systemd/system/wombat.service:BY HAND"
    "deploy/Caddyfile.wombat:/etc/caddy/Caddyfile:BY HAND"
    "src/Wombat.Web/appsettings.Production.json:/opt/wombat/app/appsettings.Production.json:publish output"
)

# Paths that must NOT exist. A file here is read by nothing, so editing it to change
# production behaviour silently does nothing — worse than its absence.
SHOULD_NOT_EXIST=(
    "/opt/wombat/config/appsettings.Production.json:the app's ContentRoot is /opt/wombat/app (systemd WorkingDirectory), so a copy in config/ is never loaded"
)

echo "Drift check against $REMOTE"
echo "repo: $REPO_ROOT"
echo

# One round trip for every remote hash, rather than one ssh per file.
REMOTE_PATHS=""
for p in "${PAIRS[@]}"; do
    REMOTE_PATHS="$REMOTE_PATHS $(printf '%s' "$p" | cut -d: -f2)"
done

REMOTE_DATA=$(ssh "${SSH_OPTS[@]}" "$REMOTE" "for f in $REMOTE_PATHS; do
    if [ -f \"\$f\" ]; then
        printf '%s\t%s\t%s\n' \"\$f\" \"\$(tr -d '\r' < \"\$f\" | sha256sum | cut -c1-16)\" \"\$(stat -c %a \"\$f\")\"
    else
        printf '%s\tABSENT\t-\n' \"\$f\"
    fi
done" 2>&1) || { echo "ssh to $REMOTE failed:"; echo "$REMOTE_DATA"; exit 1; }

DRIFT=0
MISSING=0
printf '  %-46s %-14s %s\n' DEPLOYED-PATH INSTALLED-BY RESULT
printf '  %-46s %-14s %s\n' "----------------------------------------------" "--------------" "------"

for p in "${PAIRS[@]}"; do
    local_rel="$(printf '%s' "$p" | cut -d: -f1)"
    remote_path="$(printf '%s' "$p" | cut -d: -f2)"
    via="$(printf '%s' "$p" | cut -d: -f3)"
    local_abs="$REPO_ROOT/$local_rel"

    if [ ! -f "$local_abs" ]; then
        printf '  %-46s %-14s %s\n' "$remote_path" "$via" "NO REPO FILE ($local_rel)"
        MISSING=$((MISSING + 1))
        continue
    fi

    local_hash=$(tr -d '\r' < "$local_abs" | sha256sum | cut -c1-16)
    remote_hash=$(printf '%s' "$REMOTE_DATA" | awk -F'\t' -v f="$remote_path" '$1 == f { print $2 }' | head -1)

    if [ -z "$remote_hash" ]; then
        printf '  %-46s %-14s %s\n' "$remote_path" "$via" "NO ANSWER"
        DRIFT=$((DRIFT + 1))
    elif [ "$remote_hash" = "ABSENT" ]; then
        printf '  %-46s %-14s %s\n' "$remote_path" "$via" "ABSENT ON SERVER"
        DRIFT=$((DRIFT + 1))
    elif [ "$local_hash" = "$remote_hash" ]; then
        printf '  %-46s %-14s %s\n' "$remote_path" "$via" "match"
    else
        printf '  %-46s %-14s %s\n' "$remote_path" "$via" "** DRIFT ** repo=$local_hash server=$remote_hash"
        DRIFT=$((DRIFT + 1))
    fi
done

# Files that must not exist.
echo
echo "Paths that must NOT exist:"
for s in "${SHOULD_NOT_EXIST[@]}"; do
    path="$(printf '%s' "$s" | cut -d: -f1)"
    why="$(printf '%s' "$s" | cut -d: -f2-)"
    if ssh "${SSH_OPTS[@]}" "$REMOTE" "test -f '$path'" 2>/dev/null; then
        printf '  %-46s ** PRESENT — DELETE IT **\n' "$path"
        printf '  %-46s   %s\n' "" "$why"
        DRIFT=$((DRIFT + 1))
    else
        printf '  %-46s absent (correct)\n' "$path"
    fi
done

# Deployed binaries must not be writable by anyone but their owner. tar from a Windows
# checkout carries no Unix modes, so an extract can land 666 on every DLL — which lets any
# local user swap one and get code execution as the service user on the next restart.
echo
echo "Application directory permissions:"
PERMS=$(ssh "${SSH_OPTS[@]}" "$REMOTE" '
    n=$(find /opt/wombat/app -type f -perm -o+w 2>/dev/null | wc -l)
    t=$(find /opt/wombat/app -type f 2>/dev/null | wc -l)
    printf "%s %s" "$n" "$t"' 2>/dev/null)
WW=$(printf '%s' "$PERMS" | cut -d' ' -f1)
TOT=$(printf '%s' "$PERMS" | cut -d' ' -f2)
if [ "${WW:-0}" -gt 0 ] 2>/dev/null; then
    printf '  %-46s ** %s of %s files are WORLD-WRITABLE **\n' "/opt/wombat/app" "$WW" "$TOT"
    printf '  %-46s   fix: chmod -R go-w /opt/wombat/app\n' ""
    DRIFT=$((DRIFT + 1))
else
    printf '  %-46s no world-writable files (%s checked)\n' "/opt/wombat/app" "${TOT:-?}"
fi

# Secrets must stay tight even when everything else is fine.
echo
echo "Secret file modes:"
ssh "${SSH_OPTS[@]}" "$REMOTE" '
for f in /opt/wombat/config/wombat.env /opt/wombat/data/keys; do
    if [ -e "$f" ]; then printf "  %-46s %s %s\n" "$f" "$(stat -c %a "$f")" "$(stat -c %U:%G "$f")"
    else printf "  %-46s ABSENT\n" "$f"; fi
done' 2>&1

# The link every MSF invitation carries (T205). Printed, not judged: good is BaseUrl https://<this host>, and
# MsfRespondUrl either unset (it is then {BaseUrl}/msf/respond) or exactly that. The app refuses to open a campaign when
# MsfRespondUrl is off BaseUrl's origin, so a bad value shows up there too, but only once someone tries.
echo
echo "MSF respondent link settings (wombat.env):"
ssh "${SSH_OPTS[@]}" "$REMOTE" '
for k in Wombat__BaseUrl Wombat__MsfRespondUrl; do
    v=$(grep "^$k=" /opt/wombat/config/wombat.env 2>/dev/null | tail -1 | cut -d= -f2-)
    printf "  %-46s %s\n" "$k" "${v:-(unset)}"
done' 2>&1

# Executable bit on the cron scripts — a 644 wombat-backup.sh silently stops backing up.
echo
echo "Cron script permissions:"
printf '%s' "$REMOTE_DATA" | awk -F'\t' '$1 ~ /^\/usr\/local\/bin\// {
    status = ($3 ~ /^[1357]/) ? "executable" : "** NOT EXECUTABLE **"
    printf "  %-46s %s (%s)\n", $1, status, $3
}'

# The cron entries themselves are created by README commands, not by any repo file.
echo
echo "Cron entries:"
ssh "${SSH_OPTS[@]}" "$REMOTE" '
for f in /etc/cron.d/wombat-health /etc/cron.d/wombat-backup; do
    if [ -f "$f" ]; then printf "  %-32s %s\n" "$f" "$(cat "$f")"; else printf "  %-32s ABSENT\n" "$f"; fi
done' 2>&1

# The PostgreSQL major version (T243). deploy/README.md and INFRASTRUCTURE.md say 18.x: the backups are pg_dump 18,
# which will not restore into a 16 cluster, and 18 changed an error code the app can read. A delete refused by an
# ON DELETE RESTRICT key is 23001 (restrict_violation) on 18 and 23503 on 16; PostgresErrors.IsForeignKeyViolation
# accepts both. The dev server was 16.10 on 2026-09-25, and the integration suite runs on whichever server
# WOMBAT_TEST_CONNECTION or the user secrets name, so a suite that is green on dev says nothing about 18. To run it on
# production's version, point WOMBAT_TEST_CONNECTION at a postgres:18 container (ARCHITECTURE.md § Testing).
#
# Counted in PG_PROBLEM, not DRIFT, which counts files, so the summary says which of the two is wrong. A query that
# fails says which step failed: ssh exits 255 when it cannot connect, and sudo and psql exit with their own status and
# print their own error, which is shown under the row.
EXPECTED_PG_MAJOR=18
PG_PROBLEM=0
PG_FAILED=""
echo
echo "PostgreSQL server:"
PG_ERR_FILE=$(mktemp)
PG_VERSION=$(ssh "${SSH_OPTS[@]}" "$REMOTE" "sudo -n -u postgres psql -XAtc 'SHOW server_version'" 2>"$PG_ERR_FILE")
PG_STATUS=$?
PG_VERSION=$(printf '%s\n' "$PG_VERSION" | tr -d '\r' | sed '/^$/d' | head -1)
PG_ERROR=$(tr -d '\r' < "$PG_ERR_FILE")
rm -f "$PG_ERR_FILE"
if [ "$PG_STATUS" -eq 255 ]; then
    PG_FAILED="ssh to $REMOTE failed"
elif [ "$PG_STATUS" -ne 0 ]; then
    PG_FAILED="sudo -u postgres psql exited $PG_STATUS"
elif [ -z "$PG_VERSION" ]; then
    PG_FAILED="psql answered with nothing"
fi
if [ -n "$PG_FAILED" ]; then
    printf '  %-46s ** NO ANSWER: %s **\n' "server_version" "$PG_FAILED"
    printf '%s\n' "$PG_ERROR" | sed '/^$/d' | head -3 | sed 's/^/    /'
    PG_PROBLEM=1
elif [ "${PG_VERSION%%.*}" = "$EXPECTED_PG_MAJOR" ]; then
    printf '  %-46s %s (major %s, as the repo says)\n' "server_version" "$PG_VERSION" "$EXPECTED_PG_MAJOR"
else
    printf '  %-46s ** %s: the repo says %s.x **\n' "server_version" "$PG_VERSION" "$EXPECTED_PG_MAJOR"
    PG_PROBLEM=1
fi

# Show what actually differs — a hash tells you there is a problem, not what it is.
if [ "$DRIFT" -gt 0 ]; then
    for p in "${PAIRS[@]}"; do
        local_rel="$(printf '%s' "$p" | cut -d: -f1)"
        remote_path="$(printf '%s' "$p" | cut -d: -f2)"
        local_abs="$REPO_ROOT/$local_rel"
        [ -f "$local_abs" ] || continue
        local_hash=$(tr -d '\r' < "$local_abs" | sha256sum | cut -c1-16)
        remote_hash=$(printf '%s' "$REMOTE_DATA" | awk -F'\t' -v f="$remote_path" '$1 == f { print $2 }' | head -1)
        [ "$remote_hash" = "ABSENT" ] && continue
        [ -z "$remote_hash" ] && continue
        [ "$local_hash" = "$remote_hash" ] && continue
        echo
        echo "=== diff: $local_rel (-) vs $remote_path (+) ==="
        diff <(tr -d '\r' < "$local_abs") <(ssh "${SSH_OPTS[@]}" "$REMOTE" "tr -d '\r' < $remote_path" 2>/dev/null) | sed 's/^/  /'
    done
fi

echo
if [ "$DRIFT" -eq 0 ] && [ "$MISSING" -eq 0 ] && [ "$PG_PROBLEM" -eq 0 ]; then
    echo "NO DRIFT — the server runs what the repo says."
    exit 0
fi
if [ "$DRIFT" -gt 0 ] || [ "$MISSING" -gt 0 ]; then
    echo "DRIFT: $DRIFT file(s) differ or are missing on the server; $MISSING repo file(s) not found."
    echo "Cron scripts are re-synced by deploy.ps1/deploy.sh. Everything marked BY HAND needs"
    echo "the matching step in deploy/README.md re-run."
fi
if [ -n "$PG_FAILED" ]; then
    echo "POSTGRESQL: the server's version could not be read ($PG_FAILED); see the row above."
elif [ "$PG_PROBLEM" -gt 0 ]; then
    echo "POSTGRESQL: the server runs $PG_VERSION, and the repo says $EXPECTED_PG_MAJOR.x (INFRASTRUCTURE.md § Database)."
fi
exit 1
