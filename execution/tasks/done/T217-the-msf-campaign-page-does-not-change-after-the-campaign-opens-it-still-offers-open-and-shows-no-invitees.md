---
id: T217
title: The MSF campaign page does not change after the campaign opens: it still offers Open and shows no invitees
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T217 — The MSF campaign page does not change after the campaign opens: it still offers Open and shows no invitees

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. A coordinator sees stale controls; the handlers refuse a second open.
**Surfaced:** 2026-09-25, the T163 browser check (not new; identical in a 2026-09-24 screenshot).

## Symptom

After "Campaign opened…", `/msf/campaigns/{id}` still reads "Add invitees, then open the campaign to send anonymous
response links.", with "Open campaign" enabled and no invitee list.

## What to build

Show the campaign's state and its invitees (counts or anonymised rows, per MSF anonymity) once opened. Hide Open, and
offer only what the state allows (Close, Withdraw).

## Verification

- [x] An open campaign's page shows its state and invitees, and no Open. bUnit and browser.

## Related

T205, T206, T202.

---

## As built — 2026-09-25 (`f152a15`, `f0e592f`)

Once a campaign opens, `/msf/campaigns/{id}` shows its state and its invitees as counts per respondent group, with a
Responded column. Open and the invitee form go, and the page offers only what the state allows: Withdraw and View
report while open, Review and release while under review, View report once released, and nothing once withdrawn. A
withdraw disables the other actions while it runs. A refused add re-reads the campaign, and moves the focus to the
refusal if the form has gone.

Browser on dev (scripted Chrome, master `95b65f4`):, as coordinator:
- **Campaign 10.** The picker shows labels, and the counts updated with each add. After Open the badge reads "Open",
  the columns are Respondent group, Invited and Responded, there is no Open and no form, Withdraw and View report remain,
  and the focus is on the result. Three mails reached the sink.
- **Campaign 11, withdrawn through the dialog with a double-clicked confirm:** "Withdrawn", no actions, and one status
  with no "already withdrawn". SQL state 5.
- **Campaigns 4 (under review), 1 (released) and 8 (withdrawn):** each offers only what its state allows.
- **Two tabs on campaign 12.** Tab A's add was refused with `role=alert` "Invitations can only be added while a campaign
  is in draft." It then showed "Open", and the focus was on the refusal. SQL: one invitation.
- **390px and 1280px:** no horizontal scroll.

**Found and fixed:** after an add, the focus fell to the page body, because a new form model rebuilt the form. The email
is now cleared in place and takes the focus. bUnit test, mutation-checked.

**Filed:** [T228] (P2): one address can be invited twice. Also [T224] (a Coordinator who is the subject) and [T225]
(page polish). The 390px edge gutter is added to [T226].
