# SeatPicker

Who sits on a decision panel, chosen **by person**, never in a multi-select (DESIGN.md § Form system, "The seat picker"). A pattern written in one place, `PanelEdit.razor`'s `Seats()`, and drawn on both New panel and a panel's page (DecisionPanelPage), under the heading "Who sits on it"; its words in `Panels/PanelWords.cs`. Flow 08 (T364, 2026-10-13, 622c5ad1; Q5; round 3 S3, S6, S7; M6; T257) made it. Until then the panel form had two native `<select multiple>`s, Members and External members, whose options had to be keyed by person (T257) and whose state a reader could not see at a glance.

## Markup

```html
<div class="form-group">                                   Chair
  <label for="panel-chair">Chair</label>
  <select id="panel-chair" class="form-select" aria-describedby="panel-chair-help …">
    <option value="">Choose a chair</option>
    <option>Sarah Botha (botha@kgk.wombat.local)</option> …   the name and the whole address
  </select>
  <div id="panel-chair-note" class="muted" role="status"></div>   T257's note, present from the first render
  <small class="page-subtitle" id="panel-chair-help">Only active committee members at the panel's institution who are not trainees are listed.</small>
</div>
<div id="panel-seats" class="choice-group panel-seats" role="group" aria-label="Seats">
  <div class="table-container shadow">
    <table class="clinic-table clinic-table--stack" role="table">
      <caption>Seats on the panel: the active committee members at Kgosi Kgari Teaching Hospital who are not trainees, by surname</caption>
      … one row a candidate, by surname:
      <th scope="row"><div class="person-cell">Sarah Botha<span>botha@kgk.wombat.local</span></div></th>
      <td><fieldset class="form-group choice-row">              keyed by the person (@key)
            <legend class="visually-hidden">Seat for Sarah Botha</legend>
            <div class="check-grid"> three .form-check radios: Not seated, Member, External member </div>
          </fieldset></td>
      … the chair's row: <td><p class="readonly-value">Chair <span class="muted">· chosen above</span></p></td>
    </table>
  </div>
  <p class="field-help" id="panel-seat-count" role="status">3 seats: the chair, 1 member and 1 external member.</p>
  <small class="page-subtitle">External members are external examiners, who sit with the chair on the panel's appeal body. …</small>
</div>
```

- **Chair** is a select of the people the panel may seat (`ListPanelMemberCandidatesQuery`, the rule its save enforces: active committee members at the panel's institution who do not hold Trainee, T165, T237), each "Sarah Botha (botha@kgk.wombat.local)".
- **The table** (`.panel-seats`, a flex column 8px apart) lists the same people by surname; the row header is `.person-cell` (the name over the whole address in `muted-text` at 0.9rem, which wraps anywhere). A seat is three radios in a `.check-grid` whose tracks are at least 9rem (`.panel-seats .choice-row .check-grid`), so they run across where there is room and wrap on a phone. The chair's row has no radios.
- **Choosing a chair takes them from their seat, and gives it back** (T257): the live note under Chair says so while the focus stays there, "Thandi Zulu is the chair now, so is no longer seated as a member."; choosing another chair puts the one before back, "Thandi Zulu is seated as a member again.".
- **The count** under the table, a `role="status"` line: "2 seats: the chair, 1 member and 0 external members." ("1 seat", "1 member", "1 external member"; "no chair" when none is chosen). Then the externals' help.
- **Nobody to seat**: in place of the table, "No active committee member at Kgosi Kgari Teaching Hospital who is not a trainee can sit yet." (`.muted`), or, for an Administrator's new panel with no institution yet, "Choose the institution first: a panel seats committee members from its own institution."
- **A member who can no longer sit** (erased, deactivated, moved, no longer a committee member, or holding Trainee) is in no control, so the form never sends someone the save refuses; the panel's page names them above the picker (DecisionPanelPage, M6).

## Refused (round 3 S6)

A panel refused for its seats (a panel of one: "A panel needs at least two members: the chair and at least one other, so that no decision is one person's.") marks the group **`.choice-group.is-invalid`**: the field's 4px danger stripe as an inset shadow, with no shift of its padding, and in a contrast theme a 4px `CanvasText` left border instead. The refusal stands at the group's top as its `.validation-message` (`#panel-seats-msg`), which the group names with `aria-describedby`; the form's summary says it as "Seats: A panel needs at least two members: …", a link that focuses the first radio. A missing chair is the Chair field's own: "Chair: Choose a chair.".

## Narrow widths

Below 641px the table stacks (`clinic-table--stack`), each person's row header losing the header cell's tint, edge and padding, so the name sits over its radios (`.committee-calendar .panel-seats .clinic-table--stack tbody th`, the seat picker's only: build review D5); every radio row and the Chair select are 44px (`.committee-calendar .form-check`, `.form-select`).

## Rules

- **Chosen by person, keyed by person**: each person's radio group is keyed by the user id, so a row reused for another person never shows their choice (`Design/MultiSelectOptionKeyTests` holds it). No page has a multi-select since flow 08.
- **A choice of one among a few, each a word, is radios** (DESIGN.md § Form system).
- **People by name as stored, with the whole address** where two people could share a name.

## Contrast

No new colour token. The name 11.36:1 and the address (`muted-text`) 4.57:1 on the row header's `header-bg`; a radio's edge `input-border` 3.80:1 on the surface; the invalid stripe `danger-color` 5.95:1 on the surface, the message 5.95:1.

## Known gaps

- Cross-institution external members are not supported, and the help says so.
