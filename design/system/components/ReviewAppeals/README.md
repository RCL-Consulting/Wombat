# ReviewAppeals

The committee review's **Appeals** section (`#appeals`, `.review-appeals`; written in `Pages/CommitteeDecisions/ReviewDetail.razor`'s `AppealsSection` and `ResolveForm`, its words in `Review/ReviewWords.cs` and `CommitteeDecisionWording`): the appeals lodged against the review, and for the appeal body that can act now, the form that resolves one. A part of the page (ReviewSitting), not a component. Flow 09 (T366, 2026-10-14, 49e7cf5e; board R3-3; round 3 F5, F6, F7, M8; C12b, C13, C14b, C14c; T213, T237, T307, T323) rebuilt it. Until then the resolve form came first, its Outcome a closed select that cut "Remitted: the appeal body replaces the decision" at 390 (T323), the appeals a one-line list after it, and a reader had no way to the section but scrolling to the page's end.

## Who reads it

Everyone who reads the committee review (ReviewSitting). **The resolve form** is offered only to the appeal body that can act now, and only while the review is under appeal (`CallerResolvesAppeals`, T213, T237). Everyone else reads who can resolve it.

## Getting there: "Go to the appeal" (F5)

While the review is under appeal **every reader's sitting bar carries "Go to the appeal"** (SittingBar; `SittingAct.GoToAppeal`): an outline small button (`#sitting-go-to-appeal`) at the bar's right end from 641px, under the state line at full width and 44px below. It moves the focus through `PageFocus` to the Appeals h2 (`#appeals-heading`, `tabindex="-1"`), which the browser brings into view; never an `href="#appeals"`, which `<base href="/">` would make a navigation to Home. The replay measured the jump saving 7,129px of scroll at 1280 and 13,033px at 390 (4.44).

## The section

`section.detail-card.review-appeals#appeals`, its `h2.review-section-title` "Appeals", then in order:

1. **The result**, "Appeal resolved." (`ActionResult`, the page's one result, taking the focus once drawn; ReviewSitting), or a refusal from the server when the review read again no longer offers the form.
2. **The appeals**: "No appeal has been lodged against this review." (muted), else `ul.appeal-list` of AppealEntry, "Appeal by Nomsa Mahlangu" (T308).
3. **The resolve form** (below), or under appeal for everyone else **the who-may line**, `p.appeals-who#appeal-body-note` (muted): "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John van Rensburg." It names only those who can act now (`AppealBody`), so a reader who holds a seat and cannot act now (a Coordinator in the External seat) reads it and is not named in it (F7, C13); when none can, "Only the appeal body, the panel's chair or an external member, can resolve the appeal, and none of them …".

## The resolve form (`form.resolve-form`, named "Resolve the appeal")

**One column at read width** (48rem; its `.form-grid` and outcome grid one track, T323), so the Outcome and the replacement read whole at every width; 24px below the entries.

- **Outcome** (required): a `fieldset.form-group#appeal-outcome` whose legend is "Outcome", **two radios** in flow 08's `.choice-group` (`.is-invalid` when refused), one a line in `.check-grid.resolve-outcomes`: "Dismissed: the decision stands" and "Remitted: the appeal body replaces the decision" (D51: no Upheld). **Neither is chosen at first** (T307). Its help, `small.page-subtitle`: "Dismissed leaves the committee's decision in force. Remitted replaces it with a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing reopens it." Pressed with neither: "Choose an outcome." under the group, the focus on its first radio, nothing sent (FormField: options that are sentences are radios).
- **Remitted** reveals, each full width: **Replacement category** (required, opening on "Select a category…"; on an entrustment-only review none, and in its place `p.muted#remit-entrustment-only-note` saying the remit changes no entrustment decision, C17), **Replacement rationale** (required), **Replacement conditions** (optional, T307), and **Present** (required): "Tick every panel member at this sitting. A committee decision needs at least two panel members present: the chair and at least one other.", every member who may sit, the chair and the resolver ticked and locked (T165). What was typed for a replacement is sent only with Remitted.
- **`div.decision-actions`** (DecisionForm's row): **Resolve appeal** (in the available state `btn-primary#resolve-appeal`, `type="submit"`), and beside it in `div.decision-actions-text` **the quorum line**, `p.quorum-line#appeal-quorum-line` (`aria-live="polite"`, always present, filled while Remitted is chosen): "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one other.", "2 present: the chair and 1 other. The quorum is met." Resolve appeal names it (`aria-describedby`) while Remitted is chosen. A panel that cannot seat a quorum shows its shortfall instead (`p.move-reasons#remit-reason`, "**Resolve appeal:** …"), Resolve appeal then `aria-disabled` with `.is-unavailable`, never natively disabled, and pressed it moves the focus to the reason (C12b).
- **A refusal after the actions** (M8), 16px under the row: a remit short of a category, a rationale or a quorum is refused on the page, with nothing sent, led "Resolve appeal was refused:" so it does not repeat the line: "Resolve appeal was refused: A committee decision needs at least two panel members present: the chair and at least one other.", taking the focus, everything chosen and typed kept (4.46). A failure that is no refusal: "The appeal could not be resolved just now. Try again, or come back in a few minutes.", logged (build review R1).

## The two dialogs (ConfirmDialog as built; C14b, C14c, F6)

- **Remit**: "Remit Nomsa Mahlangu's appeal?" · "Satisfactory with Observations replaces Inadequate Progress — Additional Training." ("The appeal body's decision replaces the committee's." on an entrustment-only review) · "Present: Thandi Zulu (chair), Sarah Botha" · "This closes the 2026 S2 review. Nothing reopens it, and no STAR changes." · **Go back** · **Remit the appeal**.
- **Dismiss**: "Dismiss Nomsa Mahlangu's appeal?" · "Inadequate Progress — Additional Training stays in force." · "This closes the 2026 S2 review. Nothing reopens it." · **Go back** · **Dismiss the appeal**.
- **Go back** has the first focus; it or Escape returns the focus to Resolve appeal with everything typed kept (`ReturnFocusId="resolve-appeal"`, D6). Done, "Appeal resolved." heads the section with the focus (the replay's D15, G7), the review reads Closed, the replacement heads the Decision section (DecisionHistory), and the form is gone.

## Narrow widths

No phone rule of its own: flow 07's give the section what it needs below 641px (Resolve appeal 44px at the row's width through `.decision-actions`, the radio and tick rows 2.75rem through `.review-page .check-grid`). The replay measured each radio's label row 24px tall at 1280 and 768 and 44 to 48px at 390, the labels whole (D16).

## Contrast

No new colour token. The quorum line `text-color` 12.63:1, the who-may line and help `muted-text` 5.09:1; the reason as `move-reasons`; Resolve appeal 4.86:1; the invalid group's stripe `danger-color`, as FormField's.

## Known gaps

- None the replay met (4.44 to 4.47). **Kept by the build review** (D8): while another action on the page runs, Resolve appeal is natively disabled (the busy-state convention of flow 07 and T234); while its own runs it keeps the focus, `aria-disabled`.
