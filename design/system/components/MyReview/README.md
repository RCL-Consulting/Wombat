# MyReview

Her review (`/committee/my-reviews/{ReviewId}`, `Pages/CommitteeDecisions/MyReview.razor`; its words in `MyReviewWords.cs` and `Review/AppealWords.cs`; its script `wwwroot/js/review-appeal.js`): **one committee review as its trainee reads it**, once it is ratified: what was decided, her appeal, and what the committee did with each EPA. Flow 09 (T366, 2026-10-14, 49e7cf5e; boards R3-2; round 3 F1, F6, F8, M3, S1, S5, S9, S10, S12, S13; C6, C8; T308, T329; build review D2, D9, D14) made it a page of its own. Until then it was a "Review detail" card drawn under My committee reviews' list, with the newest decision only, an "Appeal reason" box with no help or counter, and no sight of the appeal once lodged (T308).

## Who reads it

**Only the review's own trainee, and only once it is ratified, under appeal or closed**, whatever role they act in (`GetMyCommitteeReviewQuery`, F8). The page admits an Administrator too, as the list does, but reads them nothing that is not their own; an Administrator reads a review at `/committee/reviews/{id}` (ReviewSitting). Anything else, an unknown id included, is flow 01's Page not found, word for word, drawn in place, lighting nothing (NotFound). Its owner is My committee reviews (NavMenu): the trail is Home › My committee reviews › "2026 S2 annual progression review" (Breadcrumbs).

## The page

- **The header**: the h1, the tab and the crumb are the period and the type, "2026 S2 annual progression review", "2026 S2 formative check-in" (C6); the subtitle the panel, "Paed Annual Review Panel".
- **One column at every width**, `div.review-page.my-review-page` (flow 07's flex column of cards 24px apart), each section a `detail-card` with its `h2.review-section-title`:

| Section | What |
|---|---|
| The facts | `section.detail-card.detail-card--compact` (named "About this review"), a `dl.detail-list`: Panel, Sits for ("2026 S2"), Evidence window ("2026-01-01 to 2026-12-31"), Type ("Annual progression review"), State, a badge on the text's line (`ReviewSitting.StateBadge`: Ratified and Closed green, Under appeal amber; `.my-review-page .detail-list dd .badge`, D12) |
| **Decisions** (`#decisions`) | DecisionHistory with `ForTrainee`, newest first, a replaced decision marked. A formative check-in: "No binding decision. A formative check-in records no decision and issues no STAR." (S13), and no Appeal or Agenda section |
| **Appeal** (`#appeal`) | her appeals as AppealEntry in `ul.appeal-list` ("Your appeal"); while the review is Ratified, the lodge block; once Closed, "This review is closed and cannot be appealed again." (`p.muted.appeal-closed`) under her entry |
| **Agenda** (`#agenda`) | read-only, `.clinic-table.agenda-table` in an `.agenda-table-container`, captioned "The EPAs this review was there to decide for 2026 S2, and what the committee did with each.": EPA (the row header, code over title), Window ("2026 S2", "2026"), Outcome (the agenda's badge; under a decided line "STAR #1." with no download, Q6; under "Not decided at this sitting" the decision's sentence, "To be decided at her final review.", T361, D53). No actions |

## The lodge block (C8, S9)

`div.lodge-block`, read width (48rem) at every width, a flex column 16px apart:

- **"What happens next"** (`h3.form-section-title`) over `ul.lodge-next`: who hears it, the appeal body read for her Ratified review, "Your appeal is heard by the panel's appeal body: today, Thandi Zulu (chair) and John van Rensburg (external)." (said truly when nobody on it can act now: "… None of them can act today: a panel administrator must seat a chair who can before it is heard."); what they may do, "They dismiss it, and the decision stands, or remit it, and take a new decision in its place. Either outcome is final: nothing reopens the review, and it cannot be appealed again." (D51); and every reader of the reason, "Who reads your reason: everyone who can open this review on the committee's side: the panel's members, the coordinators and institutional administrators of Kgosi Kgari Teaching Hospital, your programme's speciality and sub-speciality administrators, and Wombat's system administrators.".
- **"Why you are appealing"**, required (a visible `*` and a visually hidden "required"), a `textarea.form-control` (5 rows) with **no `maxlength`**, so a paste is never cut; its help `p.field-help#appeal-reason-help` "Up to 4,000 characters."; when refused, `p.validation-message#appeal-reason-message`; then **the counter**, a second `p.field-help#appeal-reason-count`, "153 of 4,000 characters", named after the help in the box's `aria-describedby` (FormField). It counts in the browser as she types (`review-appeal.js`'s `count`), so no keystroke is a round trip; the page renders the same words from what it holds.
- **Lodge appeal** (`btn-primary#lodge-appeal`), alone in a left-aligned `.form-actions`. Pressed blank, "Give your reason for appealing." under the box, which takes the focus; over 4,000, "Give your reason in 4,000 characters or fewer."; no dialog opens and nothing is sent.
- **The dialog** (ConfirmDialog as built, F6): "Lodge your appeal?", "Against the 2026 S2 annual progression review's decision, Inadequate Progress — Additional Training.", the appeal body's sentence ending "Either outcome is final.", who reads the reason; **Keep editing** (the safe button, first focus) and **Lodge appeal**. Keep editing or Escape returns the focus to Lodge appeal (`ReturnFocusId`, D6), the reason kept.
- **After the lodge, done or refused, the page reads the review again** (T260, S5): "Appeal lodged." heads the Appeal section in an `ActionResult` with the focus, the state reads Under appeal, her entry Open with who hears it, and the block is gone. A refusal sits in the same place in Wombat's words ("Only ratified reviews can be appealed.", a second tab, M3); an unexpected failure, "Your appeal could not be lodged. Nothing has changed. Try again, or come back in a few minutes.", logged.

## States (S12, T329)

- **Loading**: the header reads "Committee review", as flow 07's does; an always-present visually hidden status, "Loading your review."; three skeleton cards in a busy `.review-page.my-review-page`.
- **Load failed**: flow 07's "The review could not be loaded. Nothing on it has changed." with Try again, answering into the h1.
- **Not hers, or not yet ratified**: Page not found ("There is no page at this address." · "Check the address, or start again from Home." · Go to Home), PageHeader's `search` icon.

## Narrow widths

One column at every width. Below 641px Lodge appeal is 44px and the page's width (`.my-review-page .form-actions .btn`), and "Show the whole reason" 44px (AppealEntry). The agenda stacks under 760px of its own width (AgendaTable's container query). No sideways scroll at 768 or 390 (the replay's 4.41, 4.43, 4.48, 4.51).

## Contrast

No new colour token: text 12.63:1, muted 5.09:1, links 6.70:1 (the Spec's pairs); Lodge appeal 4.86:1; the badges as Badge's.

## Known gaps

- None the replay met. **Kept by the build review:** Lodge appeal is `aria-disabled` while the lodge runs, never natively disabled, and the dialog's Cancel is natively disabled while its confirm runs (D8, the busy-state convention of flow 07 and T234); the appeal body is "they", "theirs" and "None of them" (D13: a group, the boards' words).
