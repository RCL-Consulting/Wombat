# Flow 11 build review: the runbook and the cast

Reviewer: read-only, Sonnet. Worktree `t373` at `7c5fed98`. Scope: `act-1-setup.md`, `act-2-onboarding.md`, the sweeps in
Acts 5, 6 and the appendix, `coverage.md`, `states.md`, against the built pages and mails. Paths below are under
`execution/knowledge/scenario-paediatrics/` unless stated.

Summary: **0 must-fix, 5 should-fix, 4 nit.** Every quoted heading, button, caption, column header, sentence, mail line and
tag in the 29 rewritten Expects matches the built code (`InvitationText.cs`, `OneTimeLink.razor`, `InvitationsList.razor`,
`InviteAPerson.razor`, `RegisterText.cs`, `Register.razor`, `RegisterOutcome.cs`, `AdmissionText.cs`, `AdmitRegistrar.razor`,
`PendingTraineesList.razor`, `TraineeDashboard.razor`, `InvitationEmail.cs`, `AdmissionEmail.cs`, `InvitationRules.cs`,
`InvitationScopeWords.cs`). The cast, addresses, training years (4, 3, 2, 1, 1), `ndlvou@` (2.16 and 2.17 only, plus the
states rows that revoke it), the journey counts and the coverage rows hold.

## What was checked and holds

- **Wording (item 1):** the list (subtitle, captions, columns, counts, "No active invitations", Revoke dialog, result and
  OneTimeLink sentences), Invite a person (subtitle, required line, eight/seven radios in order, scope fields, the four
  refusals and where each sits), Register (heading, notice, help, note, every card), Trainees (subtitle, headings, both
  captions, counts, empty states, columns), Admit (title, subtitle, trail owner, help lines, "When you admit:", result
  sentence, derived-completion sentence), the wait card, `Your Wombat invitation` and `You are admitted in Wombat` texts and
  tags (`invitation`; `trainee, admitted`).
- **Cast (item 2):** 9 staff then 5 registrars; "9 active" (Smit, Zulu, Naidoo, Botha, Patel, Khumalo, van Rensburg, Mokoena,
  Sithole, newest first in the order the Do lines issue them); 11 then 18 invitation stubs; "5 active", then "4 active" after
  the revoke, then 5 again; Users 15; Waiting in surname order (Dlamini, du Plessis, Mahlangu, Molefe, Ndlovu); Waiting 5, 4,
  1, none; Current 4 then 5. Scope phrases equal `InvitationScopeWords` output. `Issued D` over "hh:mm SAST" equals
  `ActivityMoments.When`. D+14 is the SA day plus 14 (`Invitation.LinkExpiresOn`).
- **Performable (item 3):** all routes exist (`/admin/invitations/new`, `/account/register/start` as a mapped 303 endpoint,
  `/admin/trainees/admit?userId=`); the result lands on Trainees with `?admitted=`; Invite again and Invite another address
  prefill as the Expects say; journey counts in 2.2, 2.8, 2.17, 2.26, 2.29, 2.30 and the 40 of 2.7 recount correctly (2.7's
  40 excludes Open Invitations, see C8).
- **Later acts (item 4):** no step opens `/account/register?token=`; nothing greps a stub by `role:` (the only `role:` tags
  left are inside `Actual` lines); the only absolute stub counts (Act 3, 4, 5, 6) sit in `Actual` lines, and the Expects there
  count relative or by subject, so the five extra admission stubs break nothing; "Active profiles", "Pending admission" and
  "Completed & closed profiles" survive only in `Actual` lines; Acts 5 and 6 and A.7.9 name Current trainees and "Completed
  and closed" as built; A.1.14 is unaffected. The Administrator's menu has no Trainees, and no step needs it there.
- **States (item 5):** the stand-in SQL matches the schema (`"Invitations"."ExpiresOn"` is a `date`, so
  `(now() AT TIME ZONE 'Africa/Johannesburg')::date - 1` is right; `"UserSpecialityScopes"`, `"UserSubSpecialityScopes"`,
  `"InstitutionCurriculumAdoptions"."IsActive"` exist). `IssuedAsRole` is reached through the story (Mbatha) and the
  Administrator stand-in (waiting2); the issuer-gone row works because a lock-out sets `IsDeactivated`
  (`UserDeactivation`, `PendingAdmissionReader`). N26 carries the needs-confirmation line; M04's other sentence is flagged
  inline.
- **Coverage (item 6):** `/admin/invitations/new` steps (1.5, 1.7, 2.1–2.7, 2.16, 2.17, 2.44) and `/admin/invitations` steps
  match the Route lines; `/admin/trainees/admit` (2.29, 2.30); `/admin/trainees/edit` loses 2.29; 88 templates; the
  endpoint row for `/account/register/start` lists exactly the steps whose Route names it.

## Findings

### C1 — should-fix: states.md:`/admin/trainees/admit` "A start long ago" names the wrong training year

`states.md` (row "A start long ago", `states/admit-registrar--long-ago.png`): "a start of `J−5y` typed and then put back:
'Training year 5' (or 6)". The page's output is `TraineeProfile.StageOn` = `daysElapsed / 365 + 1`
(`Wombat.Domain/Identity/TraineeProfile.cs:197–204`; `AdmissionDates.For`). `J−5y` to `D` is at least 1826 days, so the
output is "Training year 6", or 7 near the next 15 January. The same file's Step 2.29 has `J−3y` giving "Training year 4".
A replay agent will read "Training year 6" and record a gap.
Fix: "'Training year 6' (or 7 in the last days before the next 15 January)".

### C2 — should-fix: README § The browser says nothing about the clipboard, and 1.5 and 2.2 expect "Link copied."

`act-1-setup.md:139–146` and `act-2-onboarding.md` Step 2.2 expect Copy link to say "Link copied." and the button to read
"Copied". `wombat.js` `copyText` returns false (and the page says "Could not copy. Select the link and copy it yourself.")
whenever `navigator.clipboard.writeText` is refused. A Playwright context launched as README § The browser describes
(`chromium.launch({ channel: 'chrome' })`, default context) has no `clipboard-write` grant, so the first Copy link press in
1.5 fails the Expect. Only 1.5 mentions the refused wording, and states.md reaches it by blocking the permission, so the
default replay should grant it.
Fix: add to `README.md:76` (the browser bullet): "Grant the context `clipboard-read` and `clipboard-write` for the replay's
origin (`context.grantPermissions`), or Copy link answers 'Could not copy…' (Step 1.5 shows both wordings; `states.md`'s Copy
refused row blocks it on purpose)."

### C3 — should-fix: Step 2.9's Note counts one refusal; its Do plays two

`act-2-onboarding.md:209–210`: "A refused registration and then the success are 2 presses and 6 fields". The Do (three
attempts: a 10-character password, a mismatched confirmation, a correct one) is 3 presses and 8 fields (4 typed, then two
passwords again twice, since the names are kept). Either count the Do as written, or say "One refused registration and then
the success are 2 presses and 6 fields (the Do plays two refusals: 3 and 8)". A journey count is a redesign acceptance
number, so it should match the steps it is measured on.

### C4 — should-fix: Step 2.29 says the help "adds" the derived date; for a passed date it replaces the first sentence

`act-2-onboarding.md:638–640`: Expected completion has "Leave this empty to derive it from the curriculum window."; "while it
is empty its help adds what it would be ('Left empty, it would be ‹J−2y›, which has passed. Enter the expected
completion.' for Molefe's start)". `AdmitRegistrar.razor:212–216`: when the derived date has passed (Molefe J−3y, Dlamini
J−2y; du Plessis only if `D` is after `J`) the help is `TraineeAdmissionRefusals.CompletionPassed(gone)` alone, with no
"Leave this empty…" sentence; only a future derived date gets both ("Leave this empty… Left empty, it will be …", which is
what 2.30 correctly says). A strict replay checks for both sentences on Molefe's page and fails.
Fix: "...the help is 'Leave this empty to derive it from the curriculum window.'; once the start is typed and the date it would
give has passed, that help is replaced by 'Left empty, it would be ‹J−2y›, which has passed. Enter the expected
completion.' (Molefe, Dlamini); otherwise it adds 'Left empty, it will be ‹date›.'"

### C5 — should-fix (product copy, quoted faithfully): the Trainee-not-offered help names a button that no longer exists

Steps 2.13 (the Expect around `act-2-onboarding.md:254`) and 2.28 (`:606`) quote "...from Trainees with 'Admit to
curriculum'." That is the code's text (`UserAdministrationRules.cs:44` and `:50`), but flow 11 replaced that button with
"Admit…" on Waiting to be admitted (`PendingTraineesList.razor`), and the page title is Trainees. The replay passes, but the
sentence now sends a person to a control that is not there. Fix in code and in the two Expects together, for example
"from Trainees with Admit…" (and the `TraineeNote` at `:44`); or file a task and keep the runbook as is, with a Note.

### C6 — nit: states.md:166 locks Prof Mbatha and never says to reactivate her

"Awaiting admission, the issuer gone": "devadmin then locks Prof Mbatha out". Every later "Prof Mbatha" scratch row on the
same `wombat_states` database then cannot sign in. The preamble says a scratch is dropped afterwards, but not whether one
database serves all rows. Add "then Reactivate user" (the button is `Reactivate user` on her page,
`UserDetail.razor:148`), or "on a scratch database of its own".

### C7 — nit: states.md "Nothing adopted for the invited scope" (M04) is not marked needs-confirmation

The row's own parenthesis says the "No curriculum is adopted at ‹institution› for ‹sub-speciality›" sentence needs another
curriculum adopted, and the SQL shown reaches only the "no curriculum yet" sentence. N26's row says "Needs-confirmation:";
M04's should open its parenthesis the same way, so a replay does not report the unreachable sentence as a miss.

### C8 — nit: Step 2.7's count of 40 presses assumes a Copy link on every issue; only 2.2 and 2.3 say to copy

`act-2-onboarding.md:164–165`. The 40 includes Copy link for Smit, the three committee members, Patel, Khumalo,
van Rensburg, Mokoena and Sithole (it excludes Open Invitations, which 2.7's Do also presses). Steps 2.4, 2.5 and 2.6 do not
say to copy. Registration happens from the log's links, so the copy is optional. Say "(counting a Copy link for each, and
not Open Invitations)".

### C9 — nit: Step 2.29's "(4, 3, 2 and 1)" is not the order Current trainees lists them in

`act-2-onboarding.md:648–649`: Current trainees is by surname (Dlamini 3, du Plessis 2, Mahlangu 1, Molefe 4). The
parenthesis reads as the row order. Say "(Dlamini 3, du Plessis 2, Mahlangu 1, Molefe 4)".

## Items I looked for and did not find

- No Expect still quotes the old forms ("Registering X as CollegeAdmin", "Share this registration link", "It expires in 14
  days", `role:` tags, "Active profiles", "Pending admission", "Admit to curriculum:" as a row button) outside an `Actual`
  line.
- No `Route:` line names a path the router lacks; `/account/register/start` is in the endpoint table.
- `ndlvou@` appears in 2.16, 2.17, their `Actual` lines, the Act 2 outcome SQL comment, and the two states rows that revoke
  and prefill from it. `ndlovu@` is used from 2.17 on, and the signed-in-elsewhere row correctly opens Ndlovu's link.
- The last day `2026-11-02` appears only in `Actual` lines (D = 2026-10-19); every Expect says `D+14`.
