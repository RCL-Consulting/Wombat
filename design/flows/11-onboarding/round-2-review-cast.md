# Flow 11 round 2: the cast and runbook review

Reviewed 2026-10-19 from the boards' text and markup in `round-2/project/` (87 boards at 1280, 85 at 390, 6 at 768; every
390 and 768 board was diffed against its 1280 board: the content is the same, only the menu differs; `canvas.json` for
the titles), against `round-2-ask.txt` (FIXTURES, CORRECTIONS), the runbook (`execution/knowledge/scenario-paediatrics/`:
README § Cast and § The world, `act-1-setup.md` Steps 1.5 to 1.11, `act-2-onboarding.md` Steps 2.1 to 2.44 with the T369
replay's Actual lines), `design/flows/11-onboarding.md` § 8, the 2.28 and 2.29 captures in `design/upload/baseline/`, and
the code where a board states a built order or rule. D = 2026-10-19, J = 2026-01-15, expiry 2026-11-02. Severity: M
must-fix, S should-fix, N nit.

## What holds

- **Every person, address, role, scope and institution checks out.** All 18 addresses are spelled as the cast has them
  (the deliberate typo `ndlvou@` only at 2.16, 2.17 and its revoked link; `ndlovu@` everywhere else); `vanrensburg@` is
  on `sun.wombat.local` and Kruger's on `cmsa.wombat.local`; no title anywhere ("names as stored"); `du Plessis` is
  lower-case throughout; the MSF cast's Mokwena never appears for Mokoena. Role labels are WombatRoleLabels on every board
  and every scope string (`Paediatrics / Paediatrics`, `Paediatrics`, `Any speciality`) follows the role: Smit
  Coordinator and van Rensburg Committee member with `Any speciality`; Zulu, Naidoo, Botha Committee member `Paediatrics`;
  Patel, Khumalo Assessor `Paediatrics / Paediatrics`; Mokoena Speciality admin `Paediatrics`; Sithole Sub-speciality
  admin `Paediatrics / Paediatrics`; the five registrars Trainee `Paediatrics / Paediatrics`.
- **Rows and order.** L04 draws the nine in reverse issue order, Sithole first, all 09:19 SAST (2.7 Actual: "all issued
  2026-10-19 09:19"); L05 the five at 09:22 with `ndlvou@` first; L07 four after the revoke; L08 five with `ndlovu@`
  first (09:22); L09 du Plessis alone, 09:22, Not delivered, Resend before Revoke…; L10 10:23, Being sent, only
  Revoke…, expiry 2026-11-02 (2.26 Actual); L03 Mbatha above Kruger (1.7 Actual: "above Kruger's"), times `‹time›`
  because the replay never recorded them. Counts in every caption equal their rows (9, 5, 4, 5, 1, 2, 19; 5 waiting,
  4, 1, 0; 1, 4, 5 current).
- **Dates and training years.** Molefe 2023-01-15 year 4 completion 2027-01-14; Dlamini 2024-01-15, 3, 2028-01-14; du
  Plessis 2025-01-15, 2, 2029-01-14; Mahlangu 2026-01-15, 1, 2030-01-14; Ndlovu 2026-01-15, year 1, derived 2027-01-15,
  then 2030-01-14: all equal 2.29, 2.30 and the ask, and each year follows from the start and D (Home's "Semester 2,
  2026" equals 2.18's Actual). M01 opens at 2026-10-19, "Training year 1"; M06 (2027-01-15, "Not started") and M07
  (2022-01-15, 4 years 9 months, "Training year 5") follow. Registered 2026-10-19 for all five. The Waiting and Current
  lists order by surname (Dlamini, du Plessis, Mahlangu, Molefe, Ndlovu), which the capture 2.28-4 and 2.29-3 show and
  `ListPendingTrainees` (service `OrderBy LastName`) and `ListTraineesForSpeciality.cs:97` give.
- **Counts across boards.** Nine staff (L04, L11, L13), five registrars (L05, T01), revoke and reissue (L05 to L08: five,
  four, five), the resend (L09, L10), "Waiting for admission: 5" (X01). The journey counts on R2-0-steps recompute: 4 + 1;
  40 + 9 (3 + 4·3 + 5·2 + 3 + 5 + 6 = 39, plus Invite a person from Home); 5 + 1; 2 + 0; 1 + 4; 2 + 6; 2 + 2; 2 + 1;
  10 + 9.
- **R2-0-steps covers all 29 steps § 8 lists** (1.5, 1.7 to 1.11, 2.1 to 2.11, 2.16 to 2.19, 2.26 to 2.32, 2.44), each with a
  board that exists, and the Expects it writes match what the boards draw except as the findings below say.
- **No board shows a live registration link or token.** Every link field reads `…/account/register?token=•••` (L10, N03,
  N06, N09, N21); no board carries a URL, a long token-like string or a password; the captures board says none of the
  eleven showed one.
- Fixed words equal the runbook's: the refusals at 2.6 and 2.7, "A college is required for a college administrator.", the
  six password rules, "This invitation has already been used.", "This invitation has been revoked.", "This invitation is
  invalid.", the revoke result, "Invitation issued for …", the resend words, the T290 notes, NothingAdopted, "This user
  already has an active trainee profile.", the Expected completion help.

## Findings

No must-fix: nothing on a board contradicts the cast. Eight should-fix and six nits.

| id | sev | board(s) | what is wrong | runbook evidence | fix |
|---|---|---|---|---|---|
| C1 | S | L13 (and 390, 768) | The heavy set draws states that cannot coexist. duplessis@ (09:22) and ndlvou@ (09:22) are Not delivered or Check the address, so the reading time is at least 10:22; yet molefe@, dlamini@, mahlangu@ (09:22) and the nine staff (09:19) read Being sent. A link is Being sent only for an hour (`Invitation.DeliveryOf`, `Invitation.cs:108-118`), so every 09:19 and 09:22 row at that moment is Not delivered. | Act 2 header "Being sent for its first hour and Not delivered after" (act-2:38-42); 2.26 Actual (09:22 row Not delivered only from 10:23). | Draw the 09:19 and 09:22 rows (and the three "Being sent" `‹time›` rows) as the states they would be at the reading time, or give every row of the set `‹time›`; keep Being sent only on rows issued within the hour. |
| C2 | S | R2-0-steps rows 1.10, 2.44 (and X01) | Both Expects say "Home unchanged" / "Unchanged", but Q1 retargets Home's "Invite a person" to `/admin/invitations/new` (X01, Spec routes). The runbook's Expect for 1.10 says the header action "opens the invitations page" and 2.44 says Home "offers Invite a person". Once Q1 is built both lines change, and a replay would record a false gap. | 1.10 Expect (act-1:233-235) and Actual ("Invite a person (/admin/invitations) in its header"); 2.44 Expect and Actual (act-2:877, 884). | Say on rows 1.10 and 2.44: "Invite a person opens Invite a person (PROPOSED, Q1; X01)", and drop "Home unchanged" for 1.10. |
| C3 | S | R2-0-steps rows 2.28, 2.29, 2.30 | Today's words are not quoted where the boards replace them, which the ask requires ("quoting today's words and giving the new ones"). Not quoted: the section names "Pending admission" and "Active profiles" (now "Waiting to be admitted", "Current trainees"); the button "Admit to curriculum: <name>" (now "Admit…" named "Admit <name>"); the empty "No active trainee profiles found." (the spec's message table has it, the Steps row does not); the admit form's h1/subtitle "Trainee profile" / "Admit a pending trainee into a curriculum." (now "Admit Lerato Molefe"); and 2.29's "Each admission moves to the profile's edit page (`?id=`)" with Deactivate and Mark complete, which Admit no longer does (it returns to Trainees). | 2.28 Expect and Actual (act-2:531-537, "Admit to curriculum: <name>"); 2.29 Expect (act-2:547-552); 2.30 Expect (act-2:568-570). | Add "Today: …" with these words to the three rows, so the replay's Expects can be rewritten from them. |
| C4 | S | R2-0-steps rows 2.10, 2.18 | 2.10 is "G01-G03 pattern" and 2.18 quotes "…as Trainee in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital." but no Register board draws any of the seven people of 2.10 or any of the four of 2.18. The only scoped card is G04 (Assessor, both scopes). The wording for a role with one scope (Zulu, Naidoo, Botha "in Paediatrics", Mokoena), none (van Rensburg: "as Committee member at Kgosi Kgari Teaching Hospital") and a Trainee is asserted in the Expect and drawn nowhere. | 2.10 Expect (act-2:192-195): "Each page names the address and the invited role"; 2.18 Expect (act-2:329). | State the rule once on the Spec board (scope phrase: "in <speciality>[ / <sub-speciality>]" only when the invitation has one, then "at <institution>") or add one Register board with a single-scope role. |
| C5 | S | R2-L10 against R2-0-steps 2.26 | L10's row reads "2026-10-19 10:23 SAST" then a "Resent" word (DATA: ResentOn). The ask names no such word, the 2.26 Actual does not have one, and the Steps Expect lists the row as "10:23 SAST, Being sent, expiring 2026-11-02, only Revoke…" without it. The board and the Expect disagree. | 2.26 Actual (act-2:499-505): "The row reads 'Being sent', issued 2026-10-19 10:23, expiring 2026-11-02 (14 days), and offers only Revoke." | Either name "Resent" in the Steps row and put it before the operator as PROPOSED (the Spec already lists ResentOn as DATA), or remove it from L10. |
| C6 | S | G10 to G17, N13, N14, T06 | The ask's rule is to label a state the replay never reaches "Illustration". The replay reaches none of expired, exists, not accepted, no token, fault, user not loaded, cookie lapsed, script blocked (G10 to G17), a blank or malformed Email (N13, N14) or the Administrator on Trainees (T06), and their titles carry no label. G06 and N15 to N21 do. The label also lives only in `canvas.json` titles: no board face says "Illustration" (a search of all 87 finds it only in R2-00's legend). | README § "How to play", ask line 10-11 and the states list ("Illustration" marked where the ask marks it). | Add "Illustration" to those titles; consider a chip on the board's face, as NEW/DATA/PROPOSED have. |
| C7 | S | R2-0-steps, "Issue the nine in a row" (40 + 9) | The count holds only if the form resets itself and "Invite the next person" is not a press. N06, N09 and N21 draw that button below the result and the reset form under it, and the Spec says "the form reset under 'Invite the next person'". If it is a press the count is 40 + 9 + 8 = 48 + 9. | 2.2 Actual (act-2:73-74): "the form back to its defaults" with no extra press. | Say in the journey note whether "Invite the next person" is a press (a skip-link to the reset form, uncounted) or a control. |
| C8 | S | R2-0-steps rows 2.4, 2.9 (and N08) | 2.4's Expect says Sub-speciality shows "Then choose its sub-speciality." until a speciality is chosen. No board draws an Assessor with no speciality: N08 has both chosen, and the hint is only a line in the Spec. 2.9's Expect also gives the focus only for the first refusal; G05's title says focus Password for the confirmation refusal and the Steps row omits it. | 2.4 Expect (act-2:97-98); 2.9 Actual (act-2:180-183). | Draw N08 before the speciality is chosen, or drop the hint from the Expect; state G05's focus on the 2.9 row. |
| C9 | N | R2-0-steps, last line | "Changed: 24 steps. Unchanged, saying so: 1.11, 2.31, 2.32, 2.44 (and 2.19's menu and access denied)." 24 + 4 = 28, not 29: 2.19 is changed (the card names mahlangu@, C2). | § 8: 29 steps. | "Changed: 25 steps." |
| C10 | N | L09, L12, L13 | The ask writes the Delivery text as "Not delivered." with its full stop; the boards draw the state word "Not delivered" and the note beside it (Q15's design). The runbook's cell text was "Not delivered. Resend emails …". | 2.26 Actual (act-2:501): "Not delivered. Resend emails a new link …". | Say on the Steps row that the full stop goes with the split (state word, note row). |
| C11 | N | G01 to G05, G17, G10, Spec | "Its link stops working on 2026-11-02" and the Expired column "2026-11-02" read as the link ending that day. It is usable through 2026-11-02: the list keeps rows with `ExpiresOn >= today` (`ListActiveInvitations.cs:61`) and a use is refused when `ExpiresOn < today` (`InvitationRules.cs:142`), so it stops working on 2026-11-03 and leaves Active then. The stub mail says "expires on 2026-11-02". | 1.5 Actual (act-1:147): "This link expires on 2026-11-02". | "Its link works until 2026-11-02" (or "stops working after 2026-11-02"); keep L14's "Expired" column for the date the invitation lapsed, and note the 30 days count from the first day it is not active. The ask's own wording; flag to the operator. |
| C12 | N | N14 | The malformed address is `ndlovu@kgk`, a value the ask does not give (it says only "a malformed address"), built from a cast name next to the cast's real typo `ndlvou@` (a well-formed address). A reader could take it for the 2.16 typo. | 2.16 (`ndlvou@kgk.wombat.local`, well-formed). | Use `‹a malformed address›`, or a value from flow 10's boards (`name@example` style). |
| C13 | N | P04, P06 | Both "Illustration" Homes use `molefe@kgk.wombat.local`, contradicting the cast: Molefe's invitation was issued by Mbatha (P04 says Administrator-issued) and KGK has one adopted version (P06 says two). P05 uses a bracket correctly. | README § Cast (Molefe: KGK registrar); 2.18 Actual. | `‹a registrar's address›` on P04 and P06, as P05 does. |
| C14 | N | M05 | With "Choose a curriculum" selected and nothing chosen, the page shows "Training year 4" and "in training year 4" and names "Paediatric EPA Curriculum (11.1)" in the consequence block. A year or a consequence cannot follow before a version is chosen. | 2.29 Expect: one version only ("Paediatric EPA Curriculum (11.1)"). | Draw the line and block empty until a version is chosen, or choose 11.1 and mark the second version as the illustration. |

## Checked and no finding

- Steps 1.8 to 1.10 and 2.8 to 2.11: the Expect each writes equals its board (G02, G03, G01, G04, G05, G07).
- The out-of-scope refusals (N16, N18) carry no name, date or link; the in-scope ones (N15, N17) give Pieter Smit and
  molefe@'s dates (2026-10-19, 2026-11-02).
- 2.17's journey, the dialog (L06), the result (L07) and the revoked link (G08) agree; N19 prefills Trainee,
  Paediatrics / Paediatrics with Email empty.
- T04 and T05: Ndlovu's 2027-01-15, then 2030-01-14; "Nobody is waiting to be admitted." replaces "No pending trainees
  found." (marked NEW).
- 2.19 (P02, 390 menu: Home, My data rights), 2.31, 2.32 and 1.11 are unchanged and say so.
