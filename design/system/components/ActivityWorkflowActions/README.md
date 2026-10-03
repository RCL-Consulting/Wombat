# ActivityWorkflowActions

The activity page's action bar and its note panel (`Components/Shared/Activities/ActivityWorkflowActions.razor`): the moves the server offers this reader, the move that leads on first and filled, Save draft, Discard changes, the other moves, and Cancel last and quiet; a move that needs a note opens a panel under the bar. Log an activity's own bar is the same rule in the same classes (`.form-actions--moves`), written on its page. Flow 03 (T342, 2026-09-29, 725237ee; E3, E4, C3, C4, C10, C11, A6, A10) redesigned it; until then every move was a primary, and while one ran all were disabled, the pressed one too, so the focus fell to the page. Flow 04 (T350, 2026-09-30, 06aa51d7; note 4, note 11, round 1 E1, E2, E4) put the move labels in sentence case, made the note panel one pattern for every move that needs a note (`NotePanelWords`), and gave Discard changes its reason. The note panel is part of this component, not one of its own: it is the bar's, opened by the bar's toggle, and closes back to it.

## What the consumer provides

```razor
<ActivityWorkflowActions Actions="_actions" Running="@_running" OnTransitionRequested="HandleTransitionAsync"
                         CancelAction="@model.BarCancel" CancelLabel="@model.CancelLabel" OnCancelRequested="RequestCancelAsync"
                         CanSaveDraft="@CanSaveDraft" OnSaveDraft="SaveDraftAsync" CanDiscard="…" HasPendingChanges="…"
                         OnDiscard="DiscardChanges" HandsTo="HandsTo" SubjectName="@model.SubjectName" NoteRefusal="@_noteRefusal"
                         AuthorPart="@model.AuthorPart" CurrentStateLabel="@model.Activity.CurrentStateLabel" />
```

- `Actions` (required): the moves the server decided this reader may see (`ActivityDetailDto.AvailableActions`), each an `ActivityActionDto` with its label, whether it needs a note, why it is unavailable, and what it hands on (`HandOffFieldKey`, `HandsToName`, `TargetStateLabel`, `ResultSentence`).
- `Running`: the key of the move running now (or `__save-draft`); null when none.
- `CancelAction` and `CancelLabel`: Cancel, when the bar carries it ("Cancel this draft…" on a draft, returned work included; "Cancel request…" on a request the reader holds); null when the status card carries it (ActivityStatus).
- `CanSaveDraft` (the author's own draft, or returned work, with fields to write: E3), `CanDiscard` (the form below has fields this reader may write) and `HasPendingChanges`.
- `HandsTo`: the name the move's button hands the activity to, as the form's picker holds it now, saved or not; by default the stored nominee.
- `SubjectName` (whom the note is for) and `NoteRefusal` (a refusal of the note move).
- `AuthorPart` (flow 04): what the form's first section calls the author's part (`ActivityPageModel.AuthorPart`: "request", "reflection", "review request"); its first word names the note panel's heading and Keep (`NotePanelWords.PartOf`; "activity" when there is none).
- `CurrentStateLabel` (flow 04): the state the activity is in, for a refused note's title ("Not declined. It is still Requested.").

## The bar

`div.activity-moves` (a column, 16px apart) holding `div.form-actions.form-actions--moves`: left-aligned, no top rule (the check line leads into it), 12px between buttons.

1. **The move that leads on**, filled (`.btn-primary`). Its label is the move's own, in sentence case (`WorkflowTransition.LabelFor`: "Submit", "Complete", "Record discussion", "Sign off", "Log"), with " to <name>" only when the move hands the activity to the person a filled user field names: "Submit to David Naidoo" (E4, C3). A stored nominee the directory no longer offers ("(not on the current list)") is named nowhere, and the button reads "Submit" alone.
2. **Save draft**, outlined (E3; `SaveActivityDraftCommand`: it writes no history row and no credit).
3. **Discard changes**, outlined. Until something is typed it is `.is-unavailable` and `aria-disabled="true"`, and says why beside it: "Nothing to discard yet." in `ul.move-reasons.move-reasons--beside > li#why-discard`, in the bar's row, which the button names with `aria-describedby` (flow 04, note 11).
4. **Any other move**, outlined. A move that needs a note (Decline, Return) is a toggle: `aria-expanded`, and `aria-controls="note-panel"` only while the panel it names is on the page. Moves that need a note come after those that do not.
5. **Cancel**, last: `.btn .btn-quiet .btn-quiet--danger` (underlined `danger-color` words on no ground), pushed to the far end. It opens the page's ConfirmDialog.

Then, when any move is unavailable, `ul.move-reasons` under the buttons (the row's full width, 0.9rem, body text).

**Below 641px** the bar stacks: each button the column's width and 44px, the move on top, Cancel last and no longer pushed; Discard's reason sits under its button.

## A move that cannot be completed here

Shown, never hidden, and never natively disabled: `.btn.is-unavailable` (the not-allowed cursor), `aria-disabled="true"`, named by its reason with `aria-describedby`; pressing it sends nothing. It stays in the tab order, so its reason can be reached and read. The reason starts with the move's name: "**Sign off:** Needs Overall level, which you cannot fill in here." (T107, C4). `aria-disabled` without `.is-running` means "cannot"; with it, "running".

## While a move runs

The pressed button keeps the focus, reads its first word's -ing form with the rest kept ("Submitting…", "Completing…", "Declining…", "Returning…", "Recording discussion…", "Signing off…", "Logging…"; "Working…" only for a first word it cannot form; "Saving…" for Save draft; `FilingWords.Running`), carries `aria-disabled="true"` and `.is-running` (the progress cursor), and a visually hidden status, always on the page, says the label less its ellipsis with a full stop ("Submitting.", "Recording discussion."); every button it would race is disabled (T234, C10).

## The note panel

One pattern for every move that needs a note (flow 04, note 4; round 1, E2), its words `NotePanelWords`'. `section.detail-card.note-panel#note-panel` under the bar, named by its title (`aria-labelledby="note-panel-title"`), a 4px `secondary-color` stripe, its blocks 16px apart:

- `h2#note-panel-title.note-panel-title` (1.1rem): the move and the author's part, "Decline this request", "Return this reflection", "Return this review".
- The note: its label, "Note for Sipho Ndlovu", ending in the form's `.required-mark` " *" (`aria-hidden`; the textarea's `aria-required` says "required" once); its help, `p.field-help#note-help`, "Sipho Ndlovu reads it on the activity's page. It is kept with the activity's history."; `textarea#note-in.form-control` (four rows), described by the help. **The focus moves into it** when the panel opens.
- Its own bar (`.form-actions--moves`): the send, "Decline with this note" or "Return with this note", `.btn-danger` only for a move that ends the activity (a decline; `NotePanelWords.SendClass`), else `.btn-primary` (a return, which hands it back); then the quiet keep, "Keep the request", "Keep the reflection", "Keep the review", which closes the panel and hands the focus back to its toggle. Below 641px they stack at 44px, the send on top.
- **A refused note** keeps the panel open with the note as typed: the RefusalSummary `#note-summary`, its title the move not made and the state it is still in (`RefusalWords.ForNote`: "Not declined. It is still Requested.", "Not returned. It is still Awaiting discussion."), its one line "Note for Sipho Ndlovu: Decline requires a note." a link to `#note-in`; the note `.input-validation-error`, `aria-invalid="true"`, with `p.validation-message#note-msg` "Decline requires a note." under it, and described by the help and that message (never the summary, which took the focus and was read).

## Cancel's dialog

A ConfirmDialog in its action mode, the danger action, its safe button saying what it keeps (`CancelLabel`, flow 03):

- A draft: "Cancel this draft?" · "A cancelled draft cannot be reopened, and it credits nothing." · **Keep the draft** · Cancel draft.
- A request: "Cancel this request?" · "It leaves David Naidoo's Activity inbox. A cancelled request cannot be reopened, and it credits nothing." ("your Activity inbox" to its holder) · **Keep the request** · Cancel request.

The focus starts on the keep button, goes back to the opener on Keep, and to the result once cancelled.

## Results

The page shows a move's result in its ActionResult at the head (`id="activity-result"`), which takes the focus: its sentence in bold, then whose inbox it is in, plain. "**Submitted. It is now Requested.** It is in David Naidoo's Activity inbox.", "**Completed.**", "**Declined.**", "**Logged.**", "**Cancelled.**", "**Returned to Sipho Ndlovu.** Nothing else waits for you." (a return, made by the assessor; T350 build review, D5). After a move made as someone other than the author, the result's tail says what is left for the reader ("**Completed.** 1 more waits for you.") and WayOn follows it, outside it. Save draft: "**Draft saved. It has not been submitted.** It is in nobody's inbox until you submit it."; nothing changed: "**Nothing to save.** Nothing has changed since it was last saved." A move the server no longer offers: "That action is not available here. Reload the page to see what you can do now."

## Rules (DESIGN.md § Form system, "The action bar"; § Page-level patterns, "Record page with a workflow")

- One filled move per bar; everything else outlined; `.btn-danger` only where the move ends the activity, in the note panel or the dialog.
- A move is named by its own label (flow 03, E4), in sentence case since flow 04 ("Record discussion", "Sign off"; round 1, E1): on its button, in its reason, its running label, its result and the history.
- Cancel is offered to whomever the server offers it, not only the author: the demo types' cancel is `subject|field:assessor_user_id`, so their assessor has it last in the bar (G1).

## Contrast

A quiet button `link-color` 6.70:1 on the surface (`danger-color` 5.95:1, 5.65:1 on the page); the move reasons `text-color`; the note panel's stripe `secondary-color`, 4.86:1 on the surface.

## Known gaps

- `WorkflowTransition.LabelFor` lowers a one-letter or mixed-case word (`return_to_A` is "Return to a", `resubmit_PDFs` "Resubmit pdfs"), and a double separator leaves a double space; only a builder's own keys reach it (T352).
- The greyed move's reason reads "which you cannot fill in here", where the boards said "which only the assessor fills in" (T347).
- A draft save can race a move from another tab: an activity has no concurrency token (T344).
