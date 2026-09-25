---
id: T267
title: One concurrent close makes the MSF auto-close job's whole batch fail, so every other expired campaign closes an hour late
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T267 — One concurrent close makes the MSF auto-close job's whole batch fail, so every other expired campaign closes an hour late

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The other campaigns' `ClosedOn` moves by up to an hour. That can cross a semester boundary at the
end of June or December.
**Surfaced:** 2026-09-25, the T246 review. It predates T246.

## Symptom

`MsfCampaignAutoCloseJob` closes every expired campaign it found, then saves them in one `SaveChangesAsync`. If a
coordinator closes one of them between the read and the save, the `xmin` token rejects the whole save, and none of the
others close until the next hourly run.

## What to build

Save each campaign on its own, or catch the concurrency error, drop that campaign and save the rest. Log the skip.
Postgres test with an interceptor forcing the race.

## Verification

- [x] One concurrent close leaves every other expired campaign closed in the same run. Test.

## Related

T246, T214, T168.

---

## As built — 2026-09-25 (`26cc434`)

`MsfCampaignAutoCloseJob` closes and saves each expired campaign on its own, so one concurrent close no longer fails the
batch. A skip or a campaign left open is counted. Postgres race tests.

Browser on dev (scripted Chrome, master `3f08b26`; `pg_dump -n public` first, at `recovery/pre-t251-migration.dump`): campaigns 23 and 26 had their dates moved back by SQL, and the job was triggered. Run 85 logged "auto-closed 2 of 2
expired campaigns; skipped 0, left open 0, failed 0". Both read Under review with the same `ClosedOn`, and their
invitations were anonymised. The race itself cannot be driven from a browser; the Postgres tests cover it.
