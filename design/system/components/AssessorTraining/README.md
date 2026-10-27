# AssessorTraining

The user page's **Assessor training** section (`Pages/Admin/Users/AssessorTrainingSection.razor`; its words in `Wombat.Application/Features/Assessors/AssessorTrainingWords.cs`, its read `GetAssessorTraining`, its save `CreateOrUpdateAssessorProfile`, training only): a holder of Assessor's training status, completion date and qualifications, read out and edited in a note panel. Flow 12 (T376, 2026-10-23, e53d4ae7; boards R3-T03 to T07; Q2, Q18, M1, S10, F11; T289) made it a section of the person's page. Until then training was a page of its own, `/admin/assessors/edit`, a form that also asked an institution and a speciality and rewrote the person's whole scope (M1); that route now leads here.

## What the page provides

UserPage mounts it between Roles and scope and Sign-in and access:

`<AssessorTrainingSection UserId="…" PersonName="Thandi Zulu" HoldsAssessor="…" Open="…" OnActed="…" PageResultVersion="…" />`

- `HoldsAssessor`: the section shows only while the person holds Assessor. A removed Assessor's record is kept and the section hidden, shown again with the role (Q18).
- `Open`: the page was opened at `?section=training` (`PeopleLinks.UserTraining`, the Assessors list's Record training and Edit training): the panel opens and its first radio takes the focus, once.
- `OnActed` and `PageResultVersion`: one result on the page at a time; the section clears the page's result when it acts, and its own when the page acts (build review P9).

## Anatomy

`section.detail-card.user-section` named by `h2#training-heading` "Assessor training"; `div#training-result.action-result` (hidden while empty, `.user-section .action-result:empty`); then:

- **With a record**: `dl.detail-list` of Training status, Training completed and Qualifications, each `dd.readonly-value` ("Trained", "2018-10-26" or "Not recorded", "MBChB, MMed (Paed), FC Paed (SA)"), then **Edit training**. **With none**: **Record training** alone. The trigger is `.btn-outline` in `.actions-cell`, named "Edit training for Thandi Zulu", `aria-expanded`, `aria-controls` only while the panel exists.
- **The panel** (F11; FormField § A form in a note panel): `div#training-panel.detail-card.note-panel.note-panel--form`:
  - **Training status \***: `fieldset.form-group.choice-row.choice-group`, four `.form-check` radios, Not started, In training, Provisional (awaiting sign-off), Trained, each described by the group's help, a `p.page-subtitle` after the radios: "Faculty-development lifecycle. In-training assessors may assess under supervision." The legend carries the mark and a hidden " required".
  - **Training completed**: a date, only once a finished status (Provisional or Trained) is chosen, its help "Date the assessor completed assessor training. Leave blank if unrecorded."
  - **Qualifications \***: a two-row textarea, `aria-required`.
  - `FormActions`: **Cancel** (`.btn-quiet`; closes the panel, the focus back on the trigger) and **Save training** (primary; `aria-disabled` while its save runs, never disabled).
- It asks no institution or speciality: an assessor's discipline is the person's one scope (M1).

## States

- **Refused** (S22): "Choose the training status." under the radios and "Type the qualifications, as they should read." under the textarea, each field marked on its inputs (`aria-invalid` on every radio and on the textarea, never on the fieldset), the group by `.choice-group.is-invalid`, the focus on the first refused field (the first radio); no summary. The page's check and the validator say the same words.
- **Saved**: in the section, focused: "**Training saved for Fatima Khumalo:** Provisional (awaiting sign-off), completed 2026-09-26." ("… Trained, no date." for a finished status with none; "… In training." for the others); the panel closes and the record is read out.
- **Her own page**: the record read out, the standing info note "You cannot record your own assessor training. Another administrator can." and no trigger.
- **Not an Assessor, opened at `?section=training`** (S10): said once, in the section's place, and focused: "Refilwe Mokoena does not hold the Assessor role, so has no assessor training here."
- **Loading**: "Loading the assessor training."; **failed**: "Could not load the assessor training. Nothing has changed. Try again, or come back in a few minutes." with Try again, which focuses the trigger. A failed save: "The training was not saved. Nothing has changed. …".

## Narrow widths

Below 641px each radio's label is its 44px target, every control 44px, and Save training sits on top of Cancel, both the panel's width (`.user-page .form-actions`, `column-reverse`).

## Contrast

No new colour token; the read-out's terms `muted-text` 5.09:1, its values `text-color`.
