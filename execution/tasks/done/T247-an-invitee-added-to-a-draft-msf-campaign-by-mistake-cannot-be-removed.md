---
id: T247
title: An invitee added to a draft MSF campaign by mistake cannot be removed
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T247 — An invitee added to a draft MSF campaign by mistake cannot be removed

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The only remedy is to withdraw the campaign and create it again.
**Surfaced:** 2026-09-25, the T228 review (finding 2).

## What to build

A Remove on each draft invitee row, and a command to go with it:
- the T113 scope check before any change;
- only while the campaign is a draft (no link sent, no response);
- an audit row with the address redacted (T205);
- a ConfirmDialog and focus handling per DESIGN.md.

The invitee table shows counts per group (T217), so the control needs a way to reach one address while the campaign is a
draft. Decide how: a draft-only list of the addresses the coordinator typed.

## Verification

- [x] A draft invitee can be removed, and an open campaign's cannot. Handler and bUnit tests.

## Related

T228, T217, T205, T113.

---

## As built — 2026-09-25 (`65e0eb2`)

A draft campaign lists the addresses its coordinator typed ("Addresses invited"), with a Remove on each. The list is
shown only while the campaign is a draft, because no invitee holds a working link until it opens, and it goes once the
campaign opens (T217).
- **`RemoveMsfInvitation`** checks the T113 scope first and acts on drafts only. Its audit row carries ids and never
  an address (T205).
- **A refusal** is worded by the campaign's state.
- **The page** asks through a ConfirmDialog, then moves the focus to the result.

Handler, bUnit and Postgres tests.

Browser on dev (scripted Chrome, master `8e00e68`):
- **Campaign 19.** Three invitees were listed, and each Remove is named by its address. Cancel removed nothing.
- **Remove** read "nurse-1@example.test (Nurse) has been removed from this campaign…", with the focus on the result. The
  audit row has no "@". The address could then be added again.
- **Opened elsewhere:** the refusal says so, and no address is in the page.
- **Withdrawn elsewhere** (campaign 20): the refusal says the addresses are removed.
- **Removing the only invitee** disables Open with its reason.
- **Learner feedback** lists the teaching context.
- **Keyboard:** Tab, Enter and Tab work through the dialog.

**Found:** at 390px the address table's Remove sits off-screen until scrolled. Filed as [T270].
