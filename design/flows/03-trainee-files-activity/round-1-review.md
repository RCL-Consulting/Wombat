# Flow 03 · Round 1 — the review and the pick

Round 1 was drawn on 2026-09-29 from `round-1-ask.txt` (§ 1 and § 8 as one message). Canvas version `1790663484-624c`:
27 boards saved in `round-1/project/`: `Main` (the shared parts) and three variations. The canvas gave no step counts,
so the counts below are the reviewer's, taken from the boards.

## The shared parts (`Main`), common to all three

1. A result region at the head of the body that takes the focus ("Submitted. It is now Requested." plus who will find
   it). This fixes T299's silent move.
2. A refusal: a summary that links each refused field, and a message at the field. The mark drops when the value
   changes, and the field's hints come back (T299, T263).
3. Date hints under the field, checked when the field is left, not per keystroke; only on instruments that credit.
4. A section someone else fills in: a dashed frame with no inputs and the owner's name ("Fatima Khumalo fills this
   in"); once filled, the values as text, solid, with who and when. The ladder is a read-only row with the chosen
   rung's descriptor. Descriptors exist in the data (`EntrustmentLevel.Description`).
5. One action bar: one filled button (the move that moves it on), Save draft, and Cancel last and quiet, behind a
   ConfirmDialog. Pressed, a button is aria-disabled.
6. The note panel stays open on a refusal with the note as typed, and is named for its reader ("Note to Sipho
   Ndlovu"). No more "Transition note" or "Apply".
7. A move its mover cannot make: shown greyed with its reason (A, C), or hidden and explained (B).
8. History: SAST with the zone; a note on its own full-width line; each move stacked as a block below 641 px.

## The three variations

| | A · Pick, then file; status on top | B · One form; the activity as a thread | C · Guided filing; next step first |
|---|---|---|---|
| Log an activity | A grouped picker first: rated (7), discussed or reviewed (3), logged by you (1). Each is a link to `?type=`, the address that exists. Then one form. | The type is a select inside the form (native optgroups), and changing it redraws the form. The assessor's sections are not drawn on a new filing. | A four-step stepper: what, the encounter (EPA, date and assessor as radio lists), the request, check and submit. Continue at step 2 saves a draft. |
| Activity page | A status card on top ("Who has it now", the state, what is next, the page's one action) replaces the summary card. Then About (registrar, assessor, EPA, encounter, filed, credit), the sections, and the history table. | The sections, then a thread: a Now card, then every move newest first, with notes in full. The history table goes. | A next-step card, a facts list, the sections collapsed, and the history collapsed to its latest move. |
| Declined | The reason quoted in the status card, and "File it again, to someone else" (NEW `/activities/new?from={id}`). | The same, in the thread's Now card. | The same; File it again opens step 2. |
| My activities | "Needs you" on top, then all activities with a "Who has it" column. The registrar's inbox goes. | A state filter with a Show button. The registrar's inbox stays in her menu, narrowed to what she must act on. | Three tabs as links: Needs you, With others, Finished. The registrar's inbox goes. |
| File a rated request | 2 presses on 2 pages (pick, Submit) | 1 page (choose in the select, Submit) | 4 pages, 4 presses (Continue ×3, Submit) |
| Re-submit a returned reflection | My activities or Home › Needs you › the page › Submit | Inbox › the page › Submit | My activities › the page › Edit › step 3 › step 4 › Submit |

## Reading them against the code and the brief

- **C is the weakest for this job.** A registrar files 25 a semester, often on a phone: four round trips a filing,
  and an abandoned filing leaves a draft at step 2. Its fixed "encounter" step pulls the EPA, the date and the assessor
  out of a runtime schema. Only the date is declared as such (`observation_date_field`); an assessor is any `user`
  field, and a type may have none (the teaching log) or several. Its radio lists do solve the cut-off select, but A
  solves that too, by printing the chosen EPA in full under the select.
- **B's type-as-a-field** redraws the form on change and promises to keep values "when the new type allows them",
  which is fragile across schemas. Its narrowed inbox changes T297's rule. Its thread on the activity page reads well,
  but it drops the table that F07's chair scans.
- **A** keeps the address that exists (`?type=`), the table F07 needs, and T297's agreement (Home's card equals Needs
  you). Its status card answers "who has it now" (T320) in one place. It is buildable from what the workflow already
  knows. "Who has it" means the actors who can make the next move, which the ActorRuleMatcher answers, and it degrades
  to the state's label when no single person holds it.
- **Watch in round 2:** A's h1 carries type, EPA, date and assessor (four parts, long at 390 px). A2's "Submit to
  Fatima Khumalo" depends on one nominee field; with none or several it must read "Submit".

## Recommendation

**A**, with two borrowings: from B, the history behind a native `<details>` ("All 3 moves") at 390 px; from C, the
check line before Submit ("It goes to Fatima Khumalo's Activity inbox … it will be recorded as late"), under A's
action bar.

## Decisions (put to the operator)

| # | Question | Recommendation |
|---|---|---|
| Pick | Which structure? | A, with the two borrowings above |
| Q1 | The instrument as its own first step? | Yes: A1's grouped picker of links. The groups come from the type's shape (a scale field makes it rated; a creator's move to a terminal state makes it logged), not from a hand list |
| Q2 | Showing the assessor's sections | Main panel 4: dashed, no inputs, the owner's name; once filled, text with who and when |
| Q3 | Who has it now; the history | A's status card on top, carrying the page's one action, replaces the summary card and the state under the h1. The history table stays, with each note on a full-width line, stacked at 390 px |
| Q4 | The College's labels | Rungs as a read-only row of six; the chosen rung shows its descriptor. Instruments by their full label |
| Q5 | A move its mover cannot make | Greyed, with its reason beside it (DESIGN.md's rule as it stands). No seeded workflow reaches it |
| Q6 | The registrar's own inbox | It goes. My activities' "Needs you" and Home's card list exactly the same rows (T297's rule moves there). `/activities/inbox` stays the assessor's (flow 04). Step 3.16's route changes |
| E1 | "File it again, to someone else" on a declined request | Yes: NEW `/activities/new?from={id}`, copying the type, EPA, date and request, with the assessor empty; nothing is saved until she saves |
| E2 | The activity page's h1 | Type · EPA · date (enough for T280). The assessor goes in the subtitle ("Sipho Ndlovu's request to Fatima Khumalo"), not the h1 |
| E3 | Cancel | Quiet, last, and behind a ConfirmDialog ("Cancel this draft? It cannot be reopened, and it credits nothing.") |

## The operator's answer (2026-09-29)

**Accept all**: A with both borrowings; Q1–Q6 and E1–E3 as recommended. Round 2's ask is `round-2-ask.txt`, which
adds five corrections: the Submit label with no single nominee field; "who has it" when no one person does; what
"Needs you" holds; the programme hint's changed words; and the loading, error and not-found boards.

*For the build:* today's actionable read (`ActivityWaiting.LoadActionableAsync`) takes any state the caller can move
out of, so a request the registrar can only cancel may count as hers. Correction 3 says it does not belong in "Needs
you"; check which it is before building the list (3.16-2 shows only the returned reflection).
