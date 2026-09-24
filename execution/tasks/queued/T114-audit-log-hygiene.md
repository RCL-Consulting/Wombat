---
id: T114
title: "Audit log hygiene: an unenforced size bound, an unstamped speciality, and one inconsistent SSO row"
status: queued
priority: P3
created: 2026-09-19
---
# T114 — Audit log hygiene: an unenforced size bound, an unstamped speciality, and one inconsistent SSO row

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Status:** open
**Surfaced:** 2026-09-19, while closing the audit half of T101.
**Severity:** Low — none of these is a disclosure. They are the loose ends left after T101 made the audit
log a scoped surface rather than a global one.

## Background

T101 changed the audit log from "anyone with the page can read every row" to "a row is readable by the
institution it was performed in". It did that by stamping `institutionId` in `AuditPipelineBehavior` (from
the new `IAuditContextProvider.InstitutionId`) and removing the `e.InstitutionId == null ||` catch-all
from `ListAuditEntriesQueryHandler` and `GetAuditEntryByIdQueryHandler`. It also added ~20 `[Redact]`
annotations, including for plaintext credentials that were previously written in the clear.

Three things were left.

## 1. The ~2 KB payload bound is a comment, not a mechanism

`AuditPayloadSerializer`'s doc comment says output is "bounded to ~2 KB by design". **Nothing truncates.**
`SaveActivityTypeDraftCommand.Draft{Schema,Workflow,CreditRules,DisplayFields}Json` routinely exceed it —
those are whole activity-type DSL documents, and they are deliberately NOT redacted because a type
definition is exactly what an admin would want to audit.

Decide: enforce the bound with truncation and a marker, or delete the claim from the comment. Do not
leave a doc comment asserting an invariant the code does not hold.

## 2. `AuditEntry.SpecialityId` is never stamped

The column exists and nothing writes it from the pipeline. A principal can hold **many** `speciality_id`
claims while the column is a single `int?`, so there is no honest value to write, and no query filters on
it today.

Decide: make it a collection, drop the column, or stamp it only from handlers that know a single
speciality. Leaving a permanently-null column that looks like a scope is the worst of the three.

## 3. One SSO audit row is unstamped while its sibling is not

`ExternalLoginHandler.cs:162` (`SsoAccountLinked`) omits `institutionId:` while `SsoLogin` at `:224`
passes it. Under T101's scoping the linked-account row is now Administrator-only and the login row is not,
for no reason anyone chose. Pass `institutionId:` on both.

Note the deliberate asymmetry that should NOT be "fixed": `LoginFailed` and `LoginLockedOut` in
`src/Wombat.Web/Program.cs` stay unstamped on purpose. Resolving an institution there would confirm the
account exists, which is precisely what their generic messages avoid. `Login` and `Logout` were stamped by
T101; the failure rows were not, and that is correct.

## Related

Fallout from [T101].
