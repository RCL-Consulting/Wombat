# SubmitCheck

The check line (`Components/Shared/Activities/SubmitCheck.razor`): one line above a form's action bar that says, before the move is pressed, what it will do: which state the activity lands in and whose inbox it goes to, by name. On Log an activity and on the author's own draft on the activity's page. Flow 03 (T342, 2026-09-29, 725237ee; C11) made it, from round 1's variation C.

## What the consumer provides

`<SubmitCheck Lead="@words.CheckLead" Text="@words.CheckText(handOffName, creditsNothing, daysLate)" />`

- `Lead` (required): "When you submit:", or for another first move "When you log it:" (`FilingWords.CheckLead`).
- `Text` (required): the rest, from `FilingWords.CheckText`, the one wording both pages use.

It renders `<p class="submit-check"><strong>When you submit:</strong> it goes to …</p>`.

## The words

- **To a person named:** "it goes to Fatima Khumalo's Activity inbox and stays Requested until Fatima Khumalo acts on it."
- **Nobody named yet:** "it goes to the Activity inbox of the assessor you name, and stays Requested until that assessor acts on it." (the hand-off field's label, lower-cased: "supervisor or mentor", "reviewer").
- **Finished once filed:** "it is Logged at once, and credits nothing. Nobody else acts on it." (", and credits nothing" only when the type credits nothing).
- **No named hand-off:** "it is <state> until the next step is taken."
- **Late:** " Filed today, 20 days after the encounter: it will be recorded as late." is added when a filing today would be more than 14 days after the encounter (the count Log an activity and the draft page both take from `FilingLateness`).

The state is always its label, the person always their name: never "they", never a past tense. As the hand-off picker changes, the line changes with it.

## Look

Body text on `info-bg`, a 4px `secondary-color` stripe down its left, `radius-md`, 12px by 16px, no margin (the column's gap spaces it). It sits directly above `.form-actions--moves`, which drops its top rule, so the line leads into the buttons.

## Rules (DESIGN.md § Form system, "The activity form"; § Page-level patterns, "Record page with a workflow")

- **Plain text, not a live region.** It is read in its place, above the button; a change to it is not announced.
- Only where the next move is the author's filing: never on a request someone else holds.

## Contrast

`text-color` on `info-bg`, 11.84:1; the stripe `secondary-color` on `info-bg`, 4.55:1.

## Known gaps

- A draft of a type whose move is Log still says "It is in nobody's inbox until you submit it." elsewhere on the page (T348).
