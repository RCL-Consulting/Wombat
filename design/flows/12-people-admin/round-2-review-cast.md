# Flow 12 round 2: the cast and runbook review

Reviewed 2026-10-23 from the boards' text and markup in `round-2/project/` (three Spec and Steps boards, 78 boards at 1280,
78 at 390, 8 at 768; every 390 board was diffed line by line against its 1280 board: the content is the same, only the
menu differs; `canvas.json` for the titles), against `round-2-ask.txt` (FIXTURES, DECISIONS, CORRECTIONS), the brief
`design/flows/12-people-admin.md` § 1, the runbook (`execution/knowledge/scenario-paediatrics/`: README § Cast and § The
world, `act-1-setup.md` Steps 1.1, 1.10, 1.21, 1.23, `act-2-onboarding.md` Steps 2.2 to 2.7, 2.12 to 2.15, 2.28, 2.37, 2.41,
2.44, `act-5-graduation.md` Steps 5.10, 5.17, 5.27, 5.28, `appendix-cross-cutting.md` Steps A.1.14, A.3.1, A.4.5, A.5.5,
A.5.15, A.6.1 to A.6.9, A.7.9, with the T373 replay's Actual lines) and the captures in `design/baseline/` (2.12-2, 2.14-2,
2.44-1, A.6.4-1, A.6.4-2). D = 2026-10-20, J = 2026-01-15. Severity: M must-fix, S should-fix, N nit.

## What holds

- **Every person, address, role and institution checks out.** All addresses are spelled as the cast has them
  (`vanrensburg@` on `sun.wombat.local`, Kruger's on `cmsa.wombat.local`, the rest on `kgk.wombat.local`); no title anywhere;
  `du Plessis` lower-case; role labels, never keys. Each scope follows its invitation: Smit and van Rensburg
  `Any speciality` (2.2, 2.5), Zulu, Naidoo, Botha `Paediatrics` until 2.13 and `Paediatrics / Paediatrics` after, Mokoena
  `Paediatrics` (2.6), Sithole, Patel, Khumalo and the five registrars `Paediatrics / Paediatrics` (2.4, 2.7, 2.28 "Invited
  as").
- **Fatma until 2.41, Fatima from 2.44.** Fatma on U01 to U05, T02, T05 to T08 and U14; Fatima on U06 to U08, U10, T09 to T11
  and T14. No Fatima before 2.41.
- **Counts and order per step.** Ten at 2.12 and 2.13 (U01 to U04: Botha, Khumalo, Mbatha, Mokoena, Naidoo, Patel, Sithole,
  Smit, van Rensburg, Zulu, as the 2.12 capture has them); fifteen with five Pending trainee at 2.28 (U05, H04); fifteen at
  2.44 with Trainee 5 (U06, H01: 1/1/1/1/4/5/5 equals 2.44's Actual); fourteen at A.1.14, A.6.4 and A.7.9 (U07, U08; Dlamini
  and Mahlangu Trainee, du Plessis and Molefe No role); Home at A.6.3 Cancel: 14 accounts, Trainee 2 (H05). The surname
  order puts Dlamini before du Plessis and Mokoena before Molefe, as A.1.14's Trainees list does; the Assessors list is
  Botha, Khumalo, Naidoo, Patel, Zulu (2.15's Actual).
- **Who holds which roles.** 2.12 one role each (U01); 2.13 Zulu, Naidoo, Botha "Committee member / Assessor" (U04,
  runbook 2.13 Actual: "CommitteeMember, Assessor"); Add role offers exactly Institutional admin, Speciality admin,
  Sub-speciality admin, Coordinator, Assessor to a Committee member (P03) and the same minus Coordinator plus Committee member
  to Smit (P08), as 2.13 and 2.28 record.
- **Training saved at 2.14.** Status, date and qualifications equal the runbook: Zulu Trained 2018-10-20 (D-8y), Naidoo Trained
  2020-10-20 (D-6y), Botha Trained with no date ("Not recorded"), Patel In training ("Not recorded"), Khumalo Provisional
  2026-09-20 (D-1mo), qualifications "MBChB, MMed (Paed), FC Paed (SA)" throughout. T08's rows are in the right order against
  those values.
- **Dates and endings.** du Plessis "left the programme on 2026-10-18" is true (5.27: last day D-2 = 2026-10-18; A.1.14's
  Trainees: "Withdrawn 2026-10-18"); Molefe "completed the programme on 2026-10-20" is true (5.17; A.1.14 "Completed
  2026-10-20"). Both hold no role at A.4.5 and 5.17 ("This user has no roles."), and their scope rows survive the loss of
  the role (no code removes them).
- **Patel at A.6.4.** One waiting request, du Plessis's Portfolio and Logbook Review (Paediatrics) · PAED-015, waiting since
  2026-10-12 12:35 SAST, overdue: equals A.6.8's Actual and the inbox capture; his training (In training, Not recorded)
  equals 2.14; the lock list for him has no trainee line (Q17) and Dlamini's has it (P18).
- **KGK's contact email.** `paeds-admin@kgk.wombat.local` on H01 to H04 and I01 (1.23, 2.44: before A.6.1); `hod.paediatrics@`
  on I02 and H05 (A.6.3 and after). See C9 for the one value between them.
- **Access.** No board for Mbatha carries the Demo Institution, `devadmin`, Kruger or any other institution's row, address or
  name; O01, O03 to O08 name only KGK; P23 (A.5.5) names no one and uses the same page as an unknown id. The Administrator's
  boards (U14, P27, T14, O02) alone carry Institution, and none says "her institution".
- **Words.** The built words equal the runbook and captures: the own-account note, the T303 Add role help, the reset help, the
  six rules, the refusal "The password was not reset. The new password needs: At least 12 characters. An upper-case letter.
  A symbol, such as ! or #." with the field named "The new password does not meet the rules below.", "User unavailable" /
  "The user could not be found or is outside your scope.", "Set by a global administrator." (replaced and marked NEW), the
  lock list's four built lines, and A.3.1's Administrator sentence.
- **Journey counts recompute.** Find 3 + 1, role link 2 + 0; Add role 3 + 0; record training 4 + 2 from Home (Assessors,
  Record training, status, Save; qualifications and date typed) and 3 + 2 from Add role's result; reset 1 + 1; lock and
  reactivate 3 + 0; correct the email 2 + 1.
- **The 15 steps of the ask** are all on R2-0-steps, each with a board that exists. Every Illustration the ask names carries
  the word on its face and in its title (U03, U10, U12, U13, P06 to P08, P19 to P22, P25 to P27, T01, T10, T11, T13, I03,
  I04, I06, O03 to O08, O10, H07), except U14 and T14 (C1).
- No board draws a password: every reset field is empty.

## Findings

One must-fix. Eight should-fix and six nits.

| id | sev | board(s) | what is wrong | runbook evidence | fix |
|---|---|---|---|---|---|
| C1 | M | R2-0-steps row 2.15 | The Expect's dates are in the wrong order for the names beside them. It reads "Completed 2018-10-20, 2020-10-20, Not recorded, Not recorded, 2026-09-20 by last name (Botha, Khumalo, Naidoo, Patel, Zulu)": that pairs Botha with 2018-10-20 and Khumalo with 2020-10-20. By last name the five are Not recorded, 2026-09-20, 2020-10-20, Not recorded, 2018-10-20. T08 draws them correctly, so the board and the Expect disagree, and a replay written from the Expect records a false gap. | 2.15 Actual: "Botha Trained 'Not recorded', Khumalo (Fatma) Provisional 2026-09-20, Naidoo Trained 2020-10-20, Patel In training 'Not recorded', Zulu Trained 2018-10-20". | Write the Expect row by row: Botha Trained Not recorded; Khumalo Provisional (awaiting sign-off) 2026-09-20; Naidoo Trained 2020-10-20; Patel In training Not recorded; Zulu Trained 2018-10-20. |
| C2 | S | R2-U14, R2-T14 (and 768) | The Administrator's lists are not the replay's. U14 shows Demo Administrator, Kruger, the ten KGK staff and the dev trainee, as if those were every account. The runbook seeds eight dev accounts: `trainee@`, `assessor@`, `committee@`, `committee2@`, `coordinator@`, `instadmin@wombat.local` at the Demo Institution, `collegeadmin@wombat.local` (a CPSA College admin) and `devadmin@`. Five Demo accounts and `collegeadmin@` are missing, so the "every institution" caption is false. T14 reads "Assessors at every institution 5", but `assessor@wombat.local` holds Assessor at the Demo Institution, so the count under Q2's "every Assessor holder" is 6. The state U14 draws (Fatma, ten staff, no registrars, Kruger present) is never one devadmin reads: the only Users read by devadmin is A.6.9, with every registrar present. U14 uses Fatma and T14 Fatima, with no step named on either. | act-1 "Eight dev accounts (`DevUserSeeder`)"; 1.1 "counts the seeded accounts (8 …)"; A.6.2 (25 rows, 22 with a role); A.6.9 Actual ("Demo Administrator \| devadmin@wombat.local \| Global \| Administrator"). | Mark U14 and T14 "Illustration" and bracket the missing rows (`‹Demo account›` ×5, `collegeadmin@wombat.local` as Global (no institution), College admin), or redraw U14 at A.6.9's state. T14 reads 6 with a `‹Demo Institution assessor›` row. Name the moment on both. |
| C3 | S | R2-0-steps rows 5.28 (crossing list) and A.4.5 | 5.28 is listed as a step whose words change: "du Plessis's page, '… left the programme on 2026-10-18 …' (P09)". 5.28 is du Plessis signing in himself (Home, My progress, My activities); Mbatha never opens his admin page there. The page first reaches the replay at A.4.5, which already has its row. Listing 5.28 would have the replay look for an admin page that step never opens. 5.27 (the withdrawal) shows no user page either. | 5.28 Route `/account/login → / → /portfolio/progress → /activities/mine`; 5.27 Route `/admin/trainees → /admin/trainees/edit → /admin/trainees`; A.4.5 Actual ("Manage Pieter du Plessis … Roles 'This user has no roles.'"). | Drop 5.28 from the crossing list (its Home card "Your training record" is flow 05's and unchanged). Say on A.4.5's row that the no-role sentence first appears there. |
| C4 | S | R2-0-steps row A.6.4 | Today's words omit what the new Expect removes. The runbook's A.6.4 Expect has four bullets, one of them "he is not a current trainee", and "No confirmation is asked first (T264)"; the Steps row's Today quotes neither. The new Expect says "three effects (no trainee line)" and a dialog, so both lines of the runbook change and neither is flagged. A.7.14 and A.5.15 (devadmin's "Lock out user", now "Lock out…" and a dialog) are not marked as carrying the same change except through A.7.14's row. | A.6.4 Expect and Actual ("not a current trainee (with what that leaves out)"; "Lock out user, no dialog"). | Add to Today: the four-bullet list with the trainee line and "no dialog (T264)"; say in the Expect that both go. Add to the crossing list: A.5.15 now plays "Lock out…" and its dialog for the Administrator. |
| C5 | S | R2-0-steps rows 2.13, 2.14 and the crossing list | The scope moves from 2.14 to 2.13 and the runbook says so in three places, none flagged. Note (c) says Add role's Assessor writes the sub-speciality at 2.13. The runbook's 2.14 Note says "Saving a profile sets the user's scope to the profile's (T279: open sessions end)"; its 2.37 Note says Zulu, Naidoo and Botha "hold the Paediatrics sub-speciality only through their assessor profiles (Step 2.14)"; 3.52 repeats it. Under S1 there is no profile that sets a scope (Q12), so all three notes go. The crossing list names none of 2.14's Note, 2.37 or 3.52. | 2.14 Note; 2.37 Note ("Fixed by T290" carries the sentence); act-3 3.52 line 1412 "came with the assessor profile (Step 2.14)". | Add 2.37 and 3.52 to the crossing list and say on the 2.14 row that its Note is retired (the scope arrives at 2.13, PROPOSED (c)). |
| C6 | S | R2-0-steps row 2.14 | Today's words quote an unbuilt behaviour as built. Today lists "page moves to `?id=`", but the replay's Actual says "Each save … stayed at /admin/assessors/edit" and the Gap F-2.14a is still open. Today also drops "The Qualifications field is required." beside "The UserId field is required.", which the capture 2.14-2 shows twice (the alert and under the field). T05 replaces both, so a reader cannot see that the second old refusal is also changing. | 2.14 Actual (Gap F-2.14a, F-2.14d); capture `2.14-2-empty-save.png`. | Today: "stays on /admin/assessors/edit (the `?id=` move was never built)"; both old refusals quoted. |
| C7 | S | R2-0-steps crossing list; 2.28, 5.10, A.6.1 | Three steps whose words change are not listed or lack the move that shows them. (a) 2.28's Do never opens Home, so H04's waiting line cannot be checked there: the Do needs "open Home". (b) 5.10 finds Molefe by typing in Users' filter and opening Manage ("filter 'Molefe'"): the filter now needs Show and Manage is named for her, as at 2.12 and A.6.9. (c) A.6.1 plays the Administrator on the same route `/admin/institutions/{Id:int}` the boards retitle by the institution's name, and says "Institution saved."; the Spec's message list marks "Institution saved." as replaced, and nothing says whether the Administrator keeps the old page and result (flow 18) or gets the new ones. | 2.28 Route `/admin/invitations → /admin/users → /admin/users/{UserId} → /admin/trainees`; 5.10 Do and Actual; A.6.1 Actual ("Edit institution", Status box, Deactivate, "Institution saved."). | Add 5.10 and A.6.1 to the crossing list; for A.6.1 say "unchanged: the Administrator's page is flow 18's" or give the new words. Add "open Home" to 2.28's Do. |
| C8 | S | R2-0-spec (Did the captures arrive?), P02, P12, P17, U02, I01 | The Spec says seven captures were "copied but not opened": `2.13-1-zulu-before`, `1.23-1-kgk-own-record`, `5.17-3-mbatha-user-no-roles`, `A.7.9-1-home-390`, `2.12-3-users-filter-zulu`, `states/users-list--loading`, `A.6.7-1-patel-reactivated`. Five boards lean on them (P02 on 2.13-1, I01 on 1.23-1, P12 on 5.17-3, U02 on 2.12-3, P17 on A.6.7-1). The words on those boards equal the runbook's Actual, but the ask said "Read them before drawing". | Ask: "Read them before drawing: the words in them are the data for today's pages." | Open the seven and say so in round 3's Spec, or state that the five boards were drawn from the runbook's Actual lines. |
| C9 | S | Ask (HER INSTITUTION'S RECORD), R2-0-steps row A.6.3, R2-I02 | The ask says the contact email "reads paeds-admin@kgk.wombat.local from 1.1 until A.6.3". A.6.1 (devadmin, before A.6.3) sets it to `paediatrics@kgk.wombat.local`; at A.6.3 the form therefore opens on `paediatrics@`, not `paeds-admin@`. No board draws A.6.3's before state (I02 is the saved result), so no board is wrong, but the A.6.3 Expect, written from the ask, would say the wrong starting value. | A.6.1 Do and Actual ("Contact email set to paediatrics@kgk.wombat.local and saved"); A.6.3 Do ("set its contact email to hod.paediatrics@…"). | State on the A.6.3 row that the field starts at `paediatrics@kgk.wombat.local` (A.6.1), and that I01's `paeds-admin@` is 1.23's. |
| C10 | N | R2-T11, R2-U10 | The placeholder rows sit after the last cast row on page 1. T11's fifteen `‹further assessor›` rows follow Zulu on "Page 1 of 2", but the list is by last name, so anything after Zulu belongs on page 2 and the fifteen before her. U10 puts its further people after Mbatha with Mokoena and Naidoo (on the page by name) absent, which is possible, but its four-role person comes first with no name to place it. | 2.15 / 2.12 ordering ("by last name"). | Put the placeholders between the cast rows they could sort between, or say "order not shown" in the caption. |
| C11 | N | R2-I04 | The malformed address is `hod.paediatrics.kgk.wombat.local`, a value the ask does not give (it says only "a malformed contact email"), built from the cast's real new address with the @ dropped. It reads as a typo of A.6.3's value. | A.6.3: `hod.paediatrics@kgk.wombat.local`. | Use `‹a malformed address›`, or a flow 10 or 11 value. |
| C12 | N | R2-U01 to U08, R2-P01 | Mbatha's Scope cell and who-line read "Kgosi Kgari Teaching Hospital", the institution, where Q4 says the one scope is a speciality / sub-speciality. The ask allows it ("as round 1 drew it"); the 2.12 capture has no Scope column and the runbook says nothing about hers. | 2.12 capture; 2.12 Expect. | Mark the cell PROPOSED on the Spec (an Institutional admin holds no speciality scope row), or write "Whole institution". |
| C13 | N | R2-0-spec (Families), R2-I01 to I06, R2-T family | The Spec's Families table lists "notes: /new and the Demo id" for I, and the ask asks for the redirect note on Assessors. No board face carries any of these notes: they exist only as Routes rows and the Steps row 1.23. The Demo Institution's id is 1 (A.5.5 Actual: `/admin/institutions/1`), and KGK's is 2; the ask brackets it `‹…›`. | 1.23 Actual; A.5.5 Actual. | Either add one small note board to I and T, or drop "notes" from the Families row. Write id 1 where it is quoted. |
| C14 | N | R2-0-steps row 2.12 | Scope is quoted as "Scope 'Paediatrics'" for all ten rows. It reads `Paediatrics` for Zulu, Naidoo, Botha and Mokoena, `Paediatrics / Paediatrics` for Khumalo, Patel and Sithole, `Any speciality` for Smit and van Rensburg, and the institution for Mbatha (U01). A replay written from the row would fail on seven of ten. | U01 rows; 2.2 to 2.7. | Say "each person's one scope as invited (U01)". |
| C15 | N | R2-0-steps row 2.12, A.1.14 | Both rows mark the filter as "Show" and the Role filter's "No role" sentence, but A.1.14's Expect says only "du Plessis and Molefe 'No role' with the sentence", while the runbook's A.1.14 Expect has no No role cell at all (it predates Q13 and U07). The row should say the Expect gains it. | A.1.14 Expect and Actual (14 rows, no No role wording). | Add "(new: the No role cell and its sentence)" to the A.1.14 Expect. |

## Checked and no finding

- Home at 1.10, 1.21, 2.28, 2.44 and A.6.3: counts (0 and 0, then 1 and 1), "1 account: yours", "15 accounts" (2.28 and 2.44), "14 accounts"
  (A.6.3), People labels and numbers, the waiting line only at 2.28, no "Locked out" line anywhere (C18), and no
  invented "no training recorded" line at 2.44 (C4); the menu equals 1.10's Expect for her and 1.1's for the Administrator.
- 2.12 to 2.15: own page (P01), Zulu before and after, the Add role radios with flow 11's words, the five holders before any
  record (T02), the saves and refusals, the five recorded (T08), the corrected name (T09).
- A.4.5 (P09 to P11), A.5.5 (P23: same page as an unknown id, trail Home › Users › User, tab "User"), A.6.3 (I02, H05),
  A.6.4 and A.6.7 (P14 to P17, U08), A.7.9 (P18, the four effects with the trainee line).
- The Trainee's notes (U13, P26), the loading and load-error boards, and the Administrator's SSO card (O02): no cast value in
  them.
- 390 and 768 boards: same content as their 1280 boards (no value differs).
