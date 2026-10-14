# Flow 09 round 3: the check

Round 3 (canvas "Wombat 09" version 1791966935-56f8, saved in `round-3/`, its text in `round-3/boards-text.md`) was
checked item by item against `round-3-ask.txt` by the main session, from the boards' text and their source, 2026-10-14.
Round 3 is a page of its own ("Round 3"), its boards `R3-`; rounds 1 and 2 are untouched. The Spec carries a table of
what changed and where. **The design is final.** No fourth round: what remains is two flags the build or the replay
settles (CLAUDE.md § Multi-agent workflows).

The canvas was published without its tokens stylesheet until version 1791966935-56f8 (the operator saw "bland"
previews); the boards were not changed by that fix, only the file they link.

## Verdict

- **F1–F9: all done.**
  - F1: every My committee reviews board (1280, 768, 390) lists Ndlovu's #6 (Formative check-in) above #5 and Molefe's
    #7 (Pre-graduation) above #1; the caption says "scheduled date, then review number, both descending".
  - F2: the expired STAR is drawn only under "Illustration: an expired STAR" (My authorisations 1280 a, 390).
  - F3: "Give a reason for the revocation." on the blank-pressed frame (register 1280 b, 390 b); the button is drawn
    enabled with the box empty. No "enables as" anywhere.
  - F4: the cards' names "Download certificate for PAED-001/010/012", the table's "Download certificate for PAED-002
    (revoked)", "(expired)" on the illustration; the register keeps its row names.
  - F5: "Go to the appeal" on both order boards and all five Appeals boards; the Spec places it at the bar's right end
    at 1280 and under the state line, full width, 44 px at 390, and estimates 7,800 / 18,000 px saved.
  - F6: Keep editing returns the focus to Lodge appeal (review 1280 b, 390 b; Steps 4.43).
  - F7, M2: C13's reader is "‹name›, a Coordinator at Kgosi Kgari Teaching Hospital who also holds the panel's External
    seat", with the Coordinator's menu; the lapsed member's refusal is Spec only.
  - F8: Not found is flow 01's ("There is no page at this address.") on review 390 c and 1280 c.
  - F9: Show on every register board; "Apply filters" appears nowhere.
- **M1–M11: all done.**
  - M1: the names are "…, Pre-graduation" and "…, Annual progression" on Molefe's pair, and "View the 2026 S2 formative
    review before Paed Annual Review Panel"; "pre-graduation review" is in no name.
  - M3, M4: "Only ratified reviews can be appealed." and "Only active entrustment decisions can be revoked." (reason
    kept, dialog open); "An unresolved appeal already exists…" and "…could not be found among…" are drawn nowhere.
  - M5, M8: ConfirmDialog, the appeal form's quorum line and its refusal are listed CHANGED, with their DESIGN.md
    sentences.
  - M6: the dialog's open, blank-refused and refused states; the per-keystroke flag is gone.
  - M7: F4's names on all four My authorisations boards.
  - M9: `.issued-review` is inline-flex, `min-height: 24px`, and 44 px below 641 px.
  - M10: A.7.7 reads "· 18 shown" over 18 stacked rows: Molefe's 16 (#7's thirteen, PAED-012, PAED-001, PAED-010
    Superseded) and Dlamini's 2.
  - M11: 4.35's certificate clause and 4.39's PAED-001 clause are word for word; 4.37's revoked certificate is kept.
- **S1–S13: all done.**
  - S1: at 390, her pre-graduation, formative and Ndlovu's annual pages; on the register, none, five, Molefe, Revoked,
    fifteen Active, All with the Superseded, paused, the refused revoke, loading. Fifteen Active at 1280 (register c);
    My authorisations at 768.
  - S2: "every STAR revoked or expired" at 1280 and 390. S3: "Expires today", "Expires in 1 day", Since 2026-11-03,
    "Its expiry date, 2026-11-02, passed." S4: "Superseded by STAR #18, issued 2026-10-13."
  - S5: "Then the page reads the review again" follows the refused lodge. S6: "… unless her account is deactivated."
  - S7: "Issued as plain text" (register 1280 a). S8: tables hand-written with both stack classes; FilterBar a pattern.
  - S9: none of `.row-actions`, `.review-facts`, `.star-facts`, `tr.has-line`, `.star-row-line` or a textarea font rule;
    the cards are `.star-grid`; every phone rule is scoped to `.my-review-page`, `.my-authorisations`, `.appeal-entry-more`
    or `.entrustment-list`.
  - S10: both `-webkit-line-clamp: 6` and `line-clamp: 6`; the overflow is measured after render. S11: the visually
    hidden "3 shown". S12: the DESIGN.md list. S13: review #2's and Act 5's rationales, the formative sentence NEW.
- **Nits: all done** ("Resolve appeal was refused:", "Entrustment decisions only, replaced on appeal", "Open review #2",
  the order line, board 3 at 390's caption).
- **Steps board:** every revised row as asked. 4.35 places the Institutional admin's route through Home's quick links
  ("no menu item for her"), as Q10 decided; "the menu marks the page" sits on 4.36 and 4.37, the speciality admins'.
  C26 recounted: 4.43 and 4.36 gain the blank press, 4.44 the jump.

## Flags, settled or carried

- **The sitting bar's "· 2 STARs issued"**: settled from the code. `ReviewWords.cs:98-100` already writes
  "{outcome} · N STARs issued" / "· no STAR issued".
- **The Outcome select at 390** may cut "Remitted: the appeal body replaces the decision" in the closed native select.
  Carried to the build: check it at 390 in the replay; radios (flow 08's choice-group) are the fallback.
- **F5's 390 saving** is an estimate; the replay measures it (A.7).

## For the build

- The Spec's "Data the build must add" list, plus round 2's (`round-2-review.md`).
- **T367 lands first** (the revocation mail, the widened trainee query).
- The proposed app.css "Outcomes, STARs and appeals" section is in the boards' helmet CSS, after "The committee
  calendar".
- The runbook Gaps these steps close: F-4.36a, F-4.37a, F-4.43a, F-4.45c, F-4.48a, and T260's entrustment half.
