# Flow 06 round 3: the check

Round 3 (canvas version 1791182119-de2f, saved in `round-3/`, `020af6e4`) was checked item by item against
`round-3-ask.txt` by one Sonnet reader of the boards' text and markup, 2026-10-05. **The design is final.** No fourth
round: what remains is wording and board detail the build writes itself (CLAUDE.md § Multi-agent workflows).

## Verdict

- E1–E6 and the settled reminder rules: all done.
- Corrections: 29 done; 5 partial (8, 17, 18, 30, 33); 23 (pronouns) not done.
- States to add: all done (the registrar page's eight, the heavy committee Home, the Sub-speciality admin's lists, the
  reminder in flight, the ended registrar's past review at 390, the 768 board).

## Settled here (the integrator's reading of the operator's decisions)

- **r6's "No reminder: the programme ended on 2026-10-02" goes.** E1 allows four refusals only, and the ask settled
  that a withdrawn registrar's request stays waiting (A.5.10). Such a request is listed and remindable like any other.

## What the build carries (not a design question)

1. **No pronouns in any shipped word** (about 17 board captions and notes still use she/her/he/him/his). The build
   writes every string with the person's name, "the registrar" or "the assessor".
2. **The ended registrar page** (r6) is built from flow 05's ended view (`EndedItemCard`, extracted from
   `MyProgress.razor`), not the live table r6 draws; its words are written for staff (correction 8).
3. **The steps** (17, 18), written into the runbook in the build:
   - A.5.14 plays the other-institution and erased cases only; "an earlier profile of Nomsa Mahlangu's" does not exist
     in the corpus (a profile not the preferred one is covered by a test instead).
   - A.5.16's heading takes the no-match pattern when nobody matches ("No current registrar has filed nothing in 30
     days" is wrong; in a one-sitting replay both current registrars have filed, so the list is empty), and every count
     sentence has its singular and plural ("1 of 2 current registrars has", "2 of 2 … have").
   - 4.3 and 4.4 are the runbook's own steps (`act-4-annual-review.md`); their Expects change only where A changes
     them: the menus gain Programme trainees, Waiting for assessors and Entrustment decisions ("no Entrustment
     decisions in her menu" flips), and Home's cards are Waiting for assessors, Registrars and Targets by EPA. Their
     Decisions due clauses stay.
4. **The registrar page's loading state** (r8) keeps the trail and the header while the body loads (30).
5. **No inline styles** in shipped markup: skeleton sizes are classes (33).
6. **Board detail:** A.5.10's Mini-CEX is PAED-002 · 2026-10-02 to Fatima Khumalo (A.2.7); w9's rows sorted oldest
   first; the remaining brackets (Last filed, an exit level, a cadence, a review's name) are read from the data.

## Files

- `round-1/`, `round-2/`, `round-3/`: the canvas's boards per round (https://claude.ai/artifact/97WQWEuzMYHcKngPnNdHkV).
- `round-1-ask.txt`, `round-2-ask.txt`, `round-3-ask.txt`: what was sent.
- `round-2-review.md`: the three-sided review and the operator's decisions E1–E6.
- This file: the close.
