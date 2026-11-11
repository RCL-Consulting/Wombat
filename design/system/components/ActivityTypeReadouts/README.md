# ActivityTypeReadouts

The builder's read in words (`Pages/Admin/ActivityTypes/WorkflowReadout.razor`, `CreditReadout.razor`, `VersionsTable.razor`; words `ActivityTypeReadWords.cs`; the words computed by `GetActivityTypeEditorQuery`'s `ActivityTypeReadout`, and who moves a move by `Wombat.Domain/Activities/Workflow/ActorRuleWords.cs`) and the Workflow and Credit tabs that hold them over their JSON editors: **what the saved workflow, credit rules and versions mean, in words, so a reader never has to read JSON and a writer reads what Wombat understood.** Flow 17 (T398, 2026-11-10, 84bcffeb; boards R2-B03 to B05, R2-D02, R2-D03, R2-E03, R3-D01, D04, D05, E01; Q2, Q3, Q13, T16) made them. Until then the Workflow and Credit tabs were the JSON alone, and the versions were not shown.

## WorkflowReadout

`div.workflow-readout` (a flex column, 16px apart, no part's own margin; app.css's "Flow 17: the list and the read"):

- **The states on one line**: "Draft, where a new one starts · Requested · Completed, final · Declined · Cancelled" (the initial state "where a new one starts", a terminal one "final"), by label.
- **"Moves, as saved"**, a `DataTable StackWide` with that caption: **Move**, the row header, the move's label; then **From** ("Draft", or "Draft or Requested"), **To**, **Who moves it** in words ("The registrar, or whoever filed it", "The registrar", "The person Assessor names"; a rule it cannot word shown as written in `<code>`), **Checks** ("The whole form", "The mover's own fields", "Formats only": the move's `validation`, T105), **Note** ("Required" or "No").
- It joins the catalogue's **1024px stack** (`.workflow-readout .clinic-table--stack-wide`, app.css's "Flow 16: the catalogue, shared"; T398's sweep: Mini-CEX (Paediatrics)'s four moves need 578px at their narrowest, which the builder's card gave only from 936px). Stacked, each move is a block: its label, then "From:", "To:", "Who moves it:", "Checks:", "Note:".

## CreditReadout

`div.credit-readout`: one sentence a directive, "Counts 1 towards the curriculum item for the EPA in EPA, at the level Supervision required for this encounter records."; or, for none, "Credits nothing." and its `p.muted` line "An activity of this type counts towards no curriculum item, and its encounter date may not be after today."

## VersionsTable

`div.versions-readout`: the heading "Versions" at the level the panel asks (an h2 on a reader's Details, an h3 on a writer's Publish tab), then a `DataTable StackWide` captioned "Published versions": **Version** (the row header, "1"), **Published** (the South African day, "2026-11-10"), **By** ("Nolwazi Mbatha", "Wombat's catalogue" for the seeder; left out when no row names anyone, never an id; T239); or "Not published yet."; then `p.muted` "Activities filed under a version stay on it." (Q3).

## The Workflow and Credit tabs

Each is `section.detail-card.user-section[role="tabpanel"]`, h2 "Workflow" or "Credit", the read in words, then a writer's JSON editor: `label` "Workflow JSON" or "Credit rules JSON", `textarea.form-control.font-mono` (`spellcheck="false"`, its rows by its text), and the help "After each save this shows the rules as they were saved, laid out by Wombat, one property a line." (Q13): what was typed until a save, then the saved text laid out one property a line in the parser's order. The read is always **of the saved draft**, so until a save it says what is saved, not what is typed. A reader's whole view is the read, with no JSON (Q2); the panel is then focusable itself (`tabindex="0"`).

**A refusal at the JSON**: its words under it, `p.validation-message`, first in the textarea's `aria-describedby` with the help after, the textarea `aria-invalid`: "Move Cancel cannot be read: 'topic' (Topic) is not a User field, so it names nobody.", "Rule 1 reads its EPA from 'epa', but this form's EPA field is 'epa_id'. Use 'epa_id'.", "The workflow is not valid JSON. Check the brackets and commas near line 12." The JSON stays as typed. The summary's line ("Workflow, move Cancel: …") focuses the textarea.

## Rules

- **A `.razor` file never parses a DSL**: the words are the editor query's, computed from the saved draft (`ActivityTypeWorkflowReadDto`, `ActivityTypeCreditReadDto`); `HeadingLevel` places the parts under their panel's h2.

## Contrast

No new colour token. The table's head `text-color` on `header-bg` 11.36:1; the muted lines 5.09:1 on the surface.

## Known gaps

- `WorkflowReadout` and `CreditReadout` take a `HeadingLevel` they draw nothing with (the boards draw no heading of their own; kept for the contract).
