# AppealEntry

One appeal against a committee review, the same on both review pages (`Pages/CommitteeDecisions/Review/AppealEntry.razor`; every sentence in `Review/AppealWords.cs`; its script `wwwroot/js/review-appeal.js`). Flow 09 (T366, 2026-10-14, 49e7cf5e; contract § 1; boards R3-2, R3-3; C12a, C15, S10; T308; build review D5) made it. Until then the committee's page listed an appeal as one line, its date, its reason and its outcome word in brackets ("(Open)"), after the resolve form, with no name and no one who hears it; and the trainee's page did not show her appeal at all once lodged (T308).

## What the consumer provides

The page wraps the entries in `ul.appeal-list` (a flex column 16px apart, no bullets): `<AppealEntry @key="appeal.Id" Appeal="appeal" Review="review" ForTrainee="true" />`.

- `Appeal` (required): the appeal drawn.
- `Review` (required): its review, for the appeal body that "Heard by …" names while the appeal is open.
- `ForTrainee`: the trainee's own page heads it as hers and says a resolved outcome is final.

## Markup

`li.appeal-entry#appeal-{id}`: a bordered block (1px `border-color`, `radius-lg`, 16px by 24px), a flex column 8px apart.

- **The head**, `h3.appeal-entry-head` (1.05rem, a wrapping row): "Your appeal" on her page, "Appeal by Nomsa Mahlangu" on the committee's (T308), then its badge (`BadgeFor.Appeal`; Badge): **Open** amber (`badge-accepted`), **Dismissed** and **Remitted** green (`badge-completed`).
- **The meta**, `p.appeal-entry-meta` (muted 0.9rem): "Lodged 2026-10-14", then " · Resolved 2026-10-14 by Thandi Zulu" once resolved (C15), on the South African calendar (`ActivityMoments.Date`).
- **What it came to**, `p.appeal-entry-outcome`:
  - open, the appeal body that can act now: "Heard by Thandi Zulu (chair) and John van Rensburg (external)."; nothing when nobody can (C12a);
  - resolved, the committee's "Dismissed: the decision stands." or "Remitted: the appeal body replaced the decision. The replacement decision is above."; hers "Dismissed: the decision stands, and it is final." or "Remitted: the appeal body replaced the decision. The decision above is theirs, and it is final.".
- **The reason**: `p.appeal-entry-label` (weight 600) "Reason", then `p.appeal-entry-reason#appeal-{id}-reason` (`pre-wrap`, wrapping anywhere), **clamped to six lines** (`.appeal-entry-reason--clamped`: `-webkit-line-clamp` and `line-clamp` 6).
- **"Show the whole reason"** (`button.btn.btn-sm.btn-quiet.appeal-entry-more`, `aria-controls` the reason, `aria-expanded="false"`), drawn **only when the clamped reason overflows**, measured after render by `review-appeal.js` (`overflows`), and again whenever the reason's box resizes or the fonts load (`watchOverflow`, a `ResizeObserver`; D5). Pressed, the clamp is lifted, the button goes, and the reason (`tabindex="-1"`) takes the focus. A script that cannot run never draws the button.

## Rules (DESIGN.md § Committee agenda, "The decision history and the appeal entry, on both review pages")

- **No Upheld** (D51): an appeal is dismissed or remitted, and either is final. Neither is a failure, so both are green; the word says which.
- **"Theirs" and "None of them" for the appeal body are kept** (build review D13): a group, the boards' words; no person is named by a pronoun.
- The trainee reads "your"; everyone else reads her by name.

## Narrow widths

Below 641px "Show the whole reason" is 44px (its own rule, on both pages).

## Contrast

No new colour token. The reason and outcome `text-color` 12.63:1, the meta `muted-text` 5.09:1 on the surface; the entry's 1px `border-color` edge is a grouping, not a boundary a control needs; the badges as Badge's.

## Known gaps

- None the replay met (4.43, 4.44, 4.47, 4.48).
