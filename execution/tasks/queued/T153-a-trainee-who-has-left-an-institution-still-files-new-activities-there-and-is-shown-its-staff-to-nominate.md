---
id: T153
title: A trainee who has left an institution still files new activities there, and is shown its staff to nominate
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T153 — A trainee who has left an institution still files new activities there, and is shown its staff to nominate

**Severity:** Low. A privacy leak of one institution's staff names and emails to someone who used to train there, and
activities stamped to a programme the subject has left.
**Surfaced:** 2026-09-24, T102 review round 2 (upheld 3/3). Pre-existing in the stamp rule (T101); T102 made it visible
by resolving the create-page picker the same way the stamp is resolved.

## Symptom

`SubjectScopeResolver` (formerly `ActivityService.ResolveSubjectScopeAsync`) prefers the active trainee profile, then
the highest-id inactive one, then the Identity row. A trainee whose only profile is an inactive one at institution A,
and whose Identity institution is now B, is stamped to A on every new activity. Since T102 the create page's nominee
picker resolves the same way, so it lists A's assessors (name and email), and the server accepts only A's.

## What to build

A decision first. The review suggested treating "has left" as a create-time fact: have the resolver report when its
answer came from an inactive profile at an institution the subject no longer belongs to, and refuse the create (before
any mutation) with a message that says so, mirrored on the create page. Changing the stamp rule alone would split it
from `CreditTargetResolver`, the T108 EPA picker and the T122 gate, which all follow the profile.

## Verification

- [ ] A subject with an inactive profile at A and an Identity institution B is refused at create, and the picker lists
  nobody — tests.

## Related

T102, T101, T108, T122.
