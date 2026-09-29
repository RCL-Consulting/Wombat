# Flow 03 · Round 3 checked item by item (2026-09-29)

Round 3 was drawn from `round-3-ask.txt`. Canvas version `1790668726-3bb2`, saved at `1790668927-bde4`, 88 files in
`round-3/project/`:
- three component boards (`R3-C-Log`, `R3-C-Activity`, `R3-C-Mine`), with 80 state wrappers at 1280 and 390;
- `R3-Spec` and `R3-Steps`;
- `flow03-r3.css`, loaded after round 2's `flow03.css`.

Each item was checked against the boards' text and markup (`round-3/project/`), and the round-2 phrasing it replaces was
searched for and found gone.

| Item | Holds | Where |
|---|---|---|
| C1 portfolio review, named reviewer | yes | `R3-C-Activity` awaiting-review: the Review request section in seed order ("Reviewer", "Portfolio export (file name)", "Notes for your reviewer"), "With Mohammed Patel since …", the result naming his inbox, "Credit: None: a portfolio review credits nothing". "Waiting for a reviewer" is gone. The fallback "Waiting for <state label>." is on the Spec only |
| C2 a draft is not private | yes | "It is in nobody's inbox until you submit it." on the result, Needs you, Home's card and the Spec. None of the three old phrases remain. The log's "Nobody else acts on it." is true and stays |
| C3 moves by their labels | yes | The teaching log's button "Log", its history row `mv('Log', 'Draft → Logged')`, result "Logged."; "Record Discussion", "Sign Off" on the Spec; "Submit to <name>" only where the next move is a `field:` rule's |
| C4 the greyed move | yes | `blocked-move` redrawn on an invented type (Entrustment level); new `refused-assessor` state with the field message; "(not on the current list)"; "(can no longer be named)" gone |
| C5 cast and data | yes | RCA on PAED-003, CSR on PAED-012; no PAED-006 or PAED-007 row; the 17-EPA list is Sipho Ndlovu's on 11.2; the re-filed PAED-002 to Sarah Botha is Completed beside the returned reflection; the reflection returned 2026-09-29 everywhere; the 40 rows labelled invented; no "[EPA title]" |
| C6 seed order and labels | yes | Mini-CEX EPA, Assessor, Date observed, Clinical setting, Presenting problem, Case complexity; "Reflecting on"; DOPS "Procedure complexity", with no "Assessment" section; A.7.1's focus order in seed order on Steps |
| C7 not found or not yours | yes | "Activity unavailable" · "This activity does not exist, or you cannot open it." on both widths; Step 3.4 on Steps |
| C8 focus targets | yes | The summary links are `#{{it.target}}-in` against inputs `id="{{f.id}}-in"`; the note panel's link targets `note-in` |
| C9 the programme hint as a predicted refusal | yes | `hint-programme` sets the field's own message, so `aria-invalid="true"` (`R3-C-Log` renderVals, the comment at C9) |
| C10 missing boards | yes | `assessor-note-open` (focus in the textarea, Decline `aria-expanded`; "Decline requires a note." on the refused board); `draft-cancel-dialog` ("Keep the draft"); `submitting` ("Submitting…") |
| C11 one wording | yes | Needs you worded the same on Home's card and My activities; "Closed" and "Done"; headlines end with a full stop; the check line names the state and the person; the Cancel rule is on the Spec; "was to fill this in" |
| C12 "your programme" to its registrar only | yes | Both wordings on the Spec and on Step 3.9 |
| C13 a place for the old summary | yes | `completed-paused` draws the paused EPA's credit warning under Credit in About; "Form: Version n" when a move's reason names it (Spec) |
| C14 locked is not empty | yes | `flow03-r3.css`: `.form-section-lockhead` on `--header-bg` with the lock and the owner, the body on the page ground |
| C15 the Steps board | yes | 2.43, 3.3, 3.4, 3.6, 3.11, 3.21, 3.23, 3.25, 3.27–3.29, 5.24, 6.16 and A.6.6 added; "Create · — → Draft"; Home's Recent activities stays (the rules box); the hint rule stated |
| E1–E9 | yes | E7 is visible in My activities: "… · 2026-09-09 · Sarah Botha" and "… · Fatima Khumalo" |

**The design is final for step F.** The build items are in `round-2-review.md` § For the build; `flow03.css` and
`flow03-r3.css` are the proposal, to be rebuilt in `app.css` on the scale and with the reuse listed there.
