# Flow 10 round 2: the cast and runbook review

Reviewed 2026-10-15 from the boards' text and markup in `round-2/project/` (159 boards; every 390 and 768 board was
diffed against its 1280 board: the content is the same, only the menu differs), against `round-2-ask.txt` (FIXTURES),
the runbook (`execution/knowledge/scenario-paediatrics/`: README § Cast, `act-3-operations.md` 3.34 to 3.49,
`appendix-cross-cutting.md` A.4.7 and A.7.5, `states.md` 602 to 634) and the code where a board states a built order.
D = 2026-10-14. Severity: M must-fix, S should-fix, N nit. No screenshots exist for round 2; where a board adds a value
the ask does not give, it is flagged.

## What holds

- All fifteen EPA codes and titles (C3, C4, C5, V1, V3) equal the ask's seed list; PAED-015's title is whole.
- Names, addresses, groups and counts: Mokwena, Tau, Motsepe, Khoza, Khumalo, Botha, Sebego; 2/2/2/1 and All groups 7;
  1/2/2/1 and 6 of 7 at 3.43; Nurse 4.50 from 2 and Consultant 4.00 from 2 with the four comments verbatim; Peer
  doctor and Allied health professional suppressed and never drawn (R1, R3 to R7, R9 to R11, M3).
- The narrative is 106 characters (counted); 16:34 SAST is 14:34 UTC (3.43, 3.46, 3.47); coverage (1 of 5, 0 of 5,
  2 of 15, both semester sentences) equals 3.49; du Plessis 8/3/2 and PAED-015; Molefe year 4, du Plessis year 2.
- The withdraw body and result, the duplicate and blank refusals, the five `/msf/respond` refusals and the
  questionnaire (Q1 chooses Above, Mokwena's point) are the ask's or the source's words (`MsfRespond.razor:61, 82, 95`).
- The Steps board walks all 18 steps; each Expect, apart from the findings below, would hold once built, and the
  "unchanged" steps (3.40, 3.42, 3.45, 3.48, A.4.7) are.

## Findings

1. **M. R2-L2, L3, L4, L5, L6 (and their 390 boards; L7 rows 1 and 2): the list order contradicts its own caption.**
   The boards draw Lerato Molefe above Pieter du Plessis under "newest first". The list orders by `CreatedOn`
   descending (`ListMsfCampaignsForCoordinator.cs:40`) and du Plessis's campaign (#2) was created after Molefe's (#1).
   3.38's Actual reads "Pieter du Plessis, Draft ... Lerato Molefe, Open" (`act-3-operations.md:1012`) and A.7.5's
   "du Plessis Withdrawn; Molefe Released" (`appendix-cross-cutting.md:1267`). True: du Plessis first wherever both
   rows are drawn; in L7, du Plessis above Molefe among the semester-2 rows too.
2. **M. R2-K4 (3.36, after Remove): "Opening emails each of the 7 invitees ..." above a table reading All groups 6 and
   "6 addresses".** True: "each of the 6 invitees" (K3 before the removal and K2 after the re-add are 7).
3. **S. R2-K1, K2, K3, K4, K5, K5b, K13 (the add form): the respondent-group select is drawn Nurse, Peer doctor,
   Consultant, Allied health professional, Other, with Nurse selected.** 3.36's Expect and Actual
   (`act-3-operations.md:966, 975`) and the enum (`MsfRespondentCategory.cs`: PeerDoctor, Consultant, Nurse, Ahp, Other)
   give Peer doctor, Consultant, Nurse, Allied health professional, Other, which is also the order the Steps board
   writes for 3.36; the default is then Peer doctor. The journey count "10 + 16" (8 addresses typed, 8 groups chosen)
   depends on the default: say whether an add that keeps the default counts a field.
4. **S. R2-R3, R4, R5, R6a to R6d, R7, R7b, R9, R10, R11, M3, M4: Nurse is drawn above Consultant.** The built order is
   the enum's (`MsfAggregationService.cs:44`, `.OrderBy(group => group.Key)`: Peer doctor, Consultant, Nurse, Allied
   health), and 3.44's and 3.47's Actuals read "Consultant ... and Nurse" (`act-3-operations.md:1123, 1165`). The ask
   lists Nurse first only as a fixture list. The Steps board's 3.41 to 3.47 Expects therefore name an order the replay
   will not show (operator question 1).
5. **S. R2-R2 (the Close dialog) is drawn over the 3.41 page** (Open, "3 responses, of the 5 needed", one group
   reporting, the Nurse card only) while the dialog says "the 1 link not yet used" and "the 6 responses it has". Close
   is pressed at 3.44, after 3.43: the page behind must show 6 responses of the 5 needed, 2 respondent groups reporting,
   the Nurse and Consultant cards and "Suppressed: Peer doctor, Allied health professional."
6. **S. R2-R6b, R6c, R6d carry no "Illustration" chip.** R6a is a replay state; the three variants are not, and R6d
   also rests on a PROPOSED decision. The ask: each labelled Illustration where the replay does not reach it.
7. **S. R2-R6c: the dialog says "No evidence is recorded." over a report that still lists PAED-010 and PAED-012 under
   "Evidence for" and says "2 evidence records are written" under "When you release".** Draw it over R9's page (no
   Evidence list, no records line).
8. **S. R2-R6d: the dialog says "No email is sent ..." while the release section behind it still reads "Lerato Molefe is
   sent an email ..." and the status card has no ended-programme warning.** Draw it over R10's page, which has both
   right.
9. **S. R2-R8a and R9: the replay does reach these** (`states.md:627` blocked, `:628` names no EPA): a scratch campaign
   for Dr Anele Dlamini, evidence for PAED-012, one invitee, answered and closed. The boards are labelled Illustration and
   drawn for Lerato Molefe with PAED-010 and PAED-012 (R8a also says "1 response" against a Molefe campaign that has
   7 invited). The cast-true version is "MSF report for Anele Dlamini", PAED-012, 1 response; thresholds for it are
   not in the ask (‹n›). Operator question 3.
10. **S. R2-K15 (learner feedback draft): values invented on an Illustration board.** "3" invitees ("Opening emails each
    of the 3 invitees", "Learner / All groups 3"), the thresholds "5 responses; 2 in a respondent group; 2 respondent
    groups reporting", "Evidence for PAED-010 and PAED-012" and the last day "2026-10-28" are not in the ask, which
    brackets what it does not give. Use ‹n›, ‹thresholds›, ‹EPAs› and ‹closes›.
11. **N. R2-R6b, R7b, M4: three conventions for the dropped EPA.** R6b: "1 evidence record ... PAED-010. PAED-0nn: not
    recorded" beside a backdrop listing both; R7b: "PAED-0nn — ‹title›: not recorded"; M4's strip names PAED-012 while
    its list shows PAED-010 alone. The ask uses PAED-0nn. Use one (PAED-0nn, or PAED-012 throughout since the
    campaign names PAED-010 and PAED-012) and make R6b's backdrop match.
12. **N. R2-K2 (and the Steps board, 3.36): "Invitee added. palesa.tau@kgk.wombat.local, Nurse."** The ask keeps
    "Invitee added." among today's words, and 3.36's Expect and the Steps board say each add reads "Invitee added.".
    The extra address and group are new wording with no NEW chip (operator question 4).
13. **N. R2-K7, K8: "A link nothing was heard of for an hour counts as not delivered."** Not in the ask. It is built
    behaviour (`Invitation.cs:88`; the runbook note at `act-3-operations.md:59`) but is new copy on the page with no
    NEW chip; mark it or drop it.
14. **N. R2-K12: "Withdrawn. It takes no responses, any link sent no longer works, ‹the rest as built›."** The ask gives
    only the withdraw result's sentence ("Its respondents' links no longer work, and their email addresses have been
    removed."); 3.38 is the source for the fill.
15. **N. R2-K13: the Illustration strip says Pieter du Plessis's programme ended "while the campaign was a draft".** His
    draft is campaign #2, withdrawn at 3.38 and unable to be in this state. Keep him for the ask's wording, but say
    "a draft campaign for a registrar whose programme has since ended".
16. **N. R2-0-steps, 3.44:** the runbook's Expect has the narrative and "Supervision level this feedback supports (Not
    stated and the six rungs, D10)" beside Release; the new Expect drops them, though R3 draws them ("Not stated, 1, 2,
    3a, 3b, 4, 5"). Add: "the narrative and the supervision level (Not stated, 1, 2, 3a, 3b, 4, 5) stand beside it".
17. **N. R2-0-steps, the Create journey ("5 + 3"):** it counts from Home's Start an MSF campaign (`HomeFrame.cs:52`, straight
    to the new page). 3.34's route in the runbook is Home, the list, New campaign; say the count starts at Home's action
    or add the extra press.

## Not drawn, which the ask lists

- "Signed back in (A.4.7, the list as loaded)" has no board of its own; L5 stands for it. Say so on the Spec.
- Learner feedback's template, questionnaire and report are in words only, as the ask permits.

## Operator questions

1. **Group order on every report:** Nurse then Consultant (as drawn) or the built order (Peer doctor, Consultant, Nurse,
   Allied health professional)? Recommend the built order; if the operator wants another, the build changes.
2. **Campaign list order:** keep `CreatedOn` descending (du Plessis above Molefe at 3.38 and A.7.5)? Recommend yes.
3. **R8a and R9:** redraw for Dr Dlamini and PAED-012 as `states.md:627, 628` reach them, or keep them as labelled
   illustrations for Molefe?
4. **The add result:** "Invitee added." alone, or with the address and group?
