---
id: T234
title: A plain Save on the EPA edit page drops the keyboard focus to the page body
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T234 — A plain Save on the EPA edit page drops the keyboard focus to the page body

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It affects keyboard and screen-reader users only.
**Surfaced:** 2026-09-25, the G1 browser check (T196's EpaEdit).

## Symptom

As collegeadmin, open `/admin/epas/16`, focus Save and press Enter. The status reads "EPA saved.", but
`document.activeElement` is BODY. The reactivation path (tick Active, then Save) ends the same way, for instadmin too.
This contradicts the page's comment that "Every other action leaves its own button, and the focus with it".

## Cause (inferred)

`disabled="@_saving"` disables the focused button while it saves, and a disabled element loses the focus.

## What to build

Keep the focus: move it to the status region (`.action-result`, as the Deactivate path does), or do not disable the
focused button, and guard re-entry with the flag as T202 does. Check other pages that disable their own submit button
while saving; fix them the same way, or file what you find.

## Verification

- [x] After Save, and after a reactivating Save, the focus is on the status region. bUnit (`VerifyFocusAsyncInvoke`) and
      browser.

## Related

T196, T202, T217.

Note, 2026-09-25 (the G2 browser check): a refused "Save profile" on `/admin/trainees/edit` also leaves the focus on BODY
(Save is disabled while it runs). Deactivate and Mark complete move the focus to the result; Save does not.

---

## As built — 2026-09-25 (`2b1b035`)

A save keeps the keyboard focus. The pressed button stays enabled, with `aria-disabled` while it runs and re-entry
guarded, and the focus then moves to the result region (`ActionResult`). This covers EpaEdit (Save, Deactivate,
reactivating), the trainee profile, the committee review page's actions, the institution and profile pages, curriculum
items and invitations. bUnit `ActionFocusTests` and `VerifyFocusAsyncInvoke` tests.

Browser on dev (scripted Chrome, master `675a53d`):
- **EpaEdit** (as collegeadmin and instadmin): the focus lands on "EPA saved.", "EPA deactivated.", "EPA reactivated…" or
  "EPA saved and deactivated.". A duplicate code is refused with the focus kept on Save.
- **Review 10** (as the chair): Stage, Remove, Defer, Reinstate, Record and Ratify each moved the focus to their result.
  Double Enter on Record and on Ratify wrote one audit row each.
- **Institution, profile, curriculum item and invitation saves** focus their results. A refused activity submit keeps
  the focus on Submit.

**Found:** changing a password crashes the page (it predates T234; the password is changed, but the circuit errors).
It is confirmed and raised to P2 as [T265]. **Filed from the review:** [T264] (destructive actions, pagers).
