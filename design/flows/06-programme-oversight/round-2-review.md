# Flow 06 round 2: the review

Round 2 (canvas version 1791178691-c49b, saved in `round-2/`, `7c2bdde1`) drew A at fidelity: Spec, Steps, Menus,
the four Homes, Programme trainees, the registrar page and Waiting for assessors with Send a reminder. It was checked
against the screenshots and flow 05's boards this time. Reviewed 2026-10-05 from three sides, each a Sonnet agent
reading the boards' text: **the code** (`src/`), **the cast** (the runbook and its T355 Actuals), and **the design
system** (`app.css`, DESIGN.md, states, accessibility). The canvas's own four open questions are answered or folded in
below. Severity: M must-fix, S should-fix, N nit.

## Decisions for the operator

| | Question | Recommendation |
|---|---|---|
| E1 | **Does "Opt out of digest emails" stop a staff reminder?** `DataRights.razor:161` says the opt-out stops three regular reminders and that "email about one particular thing is still sent". C4e (the ask's) made the reminder obey it. | **No:** a reminder about one request is that second kind. It is sent to an opted-out assessor; the refusals are deactivated, no email address, no such account, moved meanwhile. The account page's help is unchanged. |
| E2 | **The flat menu limit.** `NavItems.FlatLimit = 8` counts Home and the personal links, so the Coordinator's menu (Home, seven, My data rights) is 9 and would group; the ask's C10 said 8. | **Personal links are not counted:** they sit under their own rule. Up to 8 role links stay flat. DESIGN.md, `NavItems.For`, its comment and `NavMenuAuthorizationTests` change. |
| E3 | **Rows whose next move is a role, not a person.** `AwaitsReviewer` also counts a SpecialityAdmin's review of a teaching session, research output, QI project or reflective note, and the Coordinator's MSF release: the readers' own work, not an assessor's. | **List only rows whose next move names a person** (a `field:` nominee). The page is "Waiting for assessors"; role-held work is the reader's own queue. The Coordinator's Home card narrows the same way. |
| E4 | **Scope follows the acting role.** The read rules union every held role, so a Speciality admin who is also a Committee member would read the whole institution under "in Paediatrics". | **Scope by the acting role,** one query for the Home card and the page; the subtitle says what was read. The reader's own requests are left out. |
| E5 | **"Nothing filed in 30 days".** The digest counts any activity *created* about the registrar (drafts, cancelled and declined included), and lists a registrar admitted last week. | **Count activities that have left draft, and start the 30 days at the later of admission and D−30;** the digest and the page share the rule. |
| E6 | **The why-line.** Round 2 wrote "With Mohammed Patel · waiting 8 days" where flow 04 says "Waiting 8 days". | **Keep flow 04's "Waiting 8 days";** put "With Mohammed Patel" on its own line above it. |

Settled from the evidence (no decision needed): a second staff member is blocked from a same-day reminder too, by a
unique index (the canvas's question 3); the day after, the cell shows the last "Reminded …" line and the button again;
a withdrawn registrar's request is left waiting, as the runbook already plays it (A.5.10), so the canvas's question 4
goes.

## Findings

### The code

1. **M. The reminder needs new persistence.** A `ActivityTransition` would set `UpdatedOn` (`Activity.cs:146`) and
   restart the wait, so: a new table (who, when, the activity), with a migration, its `.Designer.cs` and the snapshot;
   a unique index on (activity, South African day); the sender's id pseudonymised by `ErasureExecutor` (as
   `ActivityTransition.ActorUserId`, `:143`); and a check that the state and `UpdatedOn` have not moved since the list
   was read, which is the "moved meanwhile" refusal.
2. **M. The recipient policy is Infrastructure's** (`Scheduling/ReminderRecipientPolicy.cs`, `internal static`): the
   build needs an Application-side interface. Its fourth reason, "no such account", is not drawn; draw it.
3. **M. The mail.** `AssessorPendingNudgeEmail.Build` hard-codes its subject, tags and "The following activities are
   waiting…", and prints `(int)TotalDays`, so a request under a day reads "waiting 0 days". The reminder reuses the
   template with its own tag ("assessor-reminder"), and the days say "less than a day" as flow 04 does.
4. **M. Role-held rows** (E3): "Any assessor at Kgosi Kgari Teaching Hospital" is false for every one; under E3 they
   are not listed, and w9 is redrawn as a nominee who cannot be reminded instead (deactivated).
5. **M. Scope** (E4): one role-explicit query; never the union. Today's tiles avoid the union with stamp filters.
6. **S. The registrar page's id.** flow 05's three queries take a user id and read the preferred profile, so an old
   profile's address would show the newer programme. The page refuses any profile that is not the user's preferred
   one, and checks the account exists (an erased profile keeps its institution under a pseudonym,
   `ErasureExecutor.cs:119`): not found.
7. **S. `EpaProgressTable` links every name** (`Shared/Progress/EpaProgressTable.razor`); it needs a parameter for
   names as text, and the Spec lists it as changed. `EntrustmentStandingPanel` already has `EpaHref`.
8. **S. The ended page.** flow 05's ended view is a private `EndedView` in `MyProgress.razor`, built from
   `EndedItemCard`, and its words say "your programme" (`ProgressWords.cs:98,103,159,194`). Draw the staff ended page
   from those cards, its words named NEW for staff ("No target after 2026-10-02" does not exist), and the build
   extracts `EndedView`.
9. **S. The roster's readers.** `CurriculumCoverageReader` gives per-registrar semester and year counts and an exempt
   count, not per-EPA figures: "Furthest short" and "Short on <EPA>" need a new reader over `QuotaProgressCalculator`.
   Exemption is computed (`QuotaWindowStatus.ExemptPartialPeriod`), so "the reason as stored" is "the reason", in
   words to write. The order today is by share met, then user id; after T298, then surname.
10. **S. Owner rows.** An activity opened from Waiting for assessors has no owner for these roles (`NavOwners`): add
    `ActivityView` rows for both admins and the Coordinator (Waiting for assessors lights), or say it stays unowned.
    The decision panel's owner row adds the Coordinator.
11. **S. The 5-day nudge is a literal in three places** (`AssessorPendingNudgeJob.cs`, its description, the DataRights
    help); the rule line makes a fourth. One setting, `DashboardThresholds.AssessorNudgeDays`. Removing
    `CoordinatorStallDays` touches its DTO, `CoordinatorDashboard.razor` and `RoleDashboard.cs`.
12. **S. The With filter** needs the nominee read for every candidate row (`ActivityHolders.Resolve`,
    `ReadUserField`); the row DTO gains its holder. Say so on the Spec.

### The cast

13. **M. Tie order.** At 2.33 every registrar is at 0, so surname order is Dlamini, du Plessis, Mahlangu, Molefe,
    Ndlovu. R2-Steps 2.33 and R2-Home's `ZERO` (c4, c5, a2, k2) draw training-year order. 3.52's order is right.
14. **M. The counts.** Du Plessis's CBD to Khumalo (R9, Step 3.21) stays Requested to the end of Act 3, so at 3.30
    the card reads **"3 waiting, 2 overdue"**; at 3.53 "2 waiting, 1 overdue"; at A.5.10 **"3 waiting, 1 overdue"**
    (the portfolio review, the CBD, Mahlangu's A.2.7 Mini-CEX to Khumalo at 6 days).
15. **M. The brackets the runbook fills.** Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-10-03, with
    Mohammed Patel. **Case-Based Discussion (Paediatrics)** (the stored name, not "CBD") · PAED-002 · 2026-09-29, with
    Fatima Khumalo. The Sub-speciality admin's sub-speciality is **Paediatrics** (`act-1-setup.md:332`).
16. **M. Programme trainees' figures at 3.52.** Mahlangu's PAED-002 (her DOPS, R11) and Ndlovu's (his Mini-CEX, R3)
    are "1 of 3"; "Short on PAED-002" lists du Plessis, Dlamini, Molefe at 0 first, then Mahlangu and Ndlovu.
    Dlamini's furthest short is PAED-002, 003, 005 (her PAED-004 is 1 of 3); Mahlangu's PAED-001, 003, 005.
17. **M. The new steps.** A.5.13 exists (devadmin's menu): number from A.5.14, in play order, routes as their `@page`
    (no query strings). At the appendix only Dlamini and Mahlangu are current, so "n of 5" is "n of 2". Dr Zulu waits
    on nothing after 3.33: the With filter's example is Mohammed Patel ("1 waiting, 1 overdue"), then Overdue only with
    Fatima Khumalo for no match. A refusal step names who plays it (an assessor ticks Opt out in My data rights, which
    under E1 no longer refuses) or uses a scratch database (deactivated, no email). After a reminder at 3.30, 3.31's
    Mini-CEX row reads "Reminded 2026-10-04 by Pieter Smit", and 3.32 says whether the nudge still mails a request
    reminded today (recommended: it does; the log then holds three stub mails, and 3.32 counts them).
18. **S. Steps the board missed:** 4.3 and 4.4 assert the admins' menus and Home cards; 3.33's Actual names "Targets
    this period" (now "Registrars behind").
19. **S. States at the wrong moment.** Dr Naidoo's line (c3) is Step 5.25's, `D−1` = 2026-10-03, over Act 5's roster
    (Molefe graduated, du Plessis withdrawn); the expiring invitation (k4) expires D+3 = 2026-10-07; Dr Molefe before
    graduation (r3) holds STARs on PAED-001, 010 and 012 and her final review is 2026-10-04; the heavy admin Home (a3)
    is invented (KGK-001 arrives when three are current): say so; 3.52b's "erased" and "another institution's" cases
    belong to the appendix.
20. **N.** "Last filed" reads 2026-10-04 for all five in a one-sitting replay. Ndlovu as the exempt stand-in: no step
    claims it. Entrustment decisions' trail: lit and no trail is the intended change (Q7); the steps say so.

### The design system

21. **M. Rows break between 641 and ~830 px.** `.roster-row` and `.coverage-row` keep their 10rem columns down to
    641, where a card is ~290 px wide. Collapse at 900 px (where the dashboard grid goes to one column), or with a
    container query.
22. **M. Send a reminder is 28 px at 390 on the registrar page:** the 44 px rule is scoped to `.waited-cell`. Scope it
    to `.reminder-action` and add it to app.css's phone list (~`:2816`).
23. **M. No pronouns.** "She gets one email…", "sends her no email", "waits for him", and the Spec's "a registrar's
    name to her page": use the name or "the assessor".
24. **M. `.roster-table`'s row header** takes `.clinic-table th`'s header styling (`app.css:319`): use
    `clinic-table--index` (which resets it, ~`:3520`) or declare the reset.
25. **M. `.filter-actions`** inside `.search-field` (column flex, `:282`) stacks Show and Clear: add `flex-direction:
    row; align-self: end`.
26. **M. The Coordinator's header action** changes DESIGN.md § Dashboard page ("every other role has none",
    `HomeFrame.ActionFor`, `HomeFrameTests`): the Spec lists the change.
27. **S. One foot rule:** the Spec says an empty card keeps its foot while its list is in the menu; c7, c8 and k1 draw
    none. Keep the foot, everywhere.
28. **S. Reuse, don't reinvent:** `.roster-name`/`.coverage-epa` → `.progress-row-link` (`:3616`); `.card-scope` →
    `.needs-you-rule` (`:2604`); `.figure-cell` → `.count-figure`/`.count-meta`; the labels →
    `.dashboard-metric-label`/`.progress-row-meta`; `.reminder-line` → `.waited-cell span` (`:2791`); the lists →
    `.list-unstyled`; `.section-error` exists (`:3499`); `.waiting-with` and `.with-cell-note` go. Keep only the two
    column grids.
29. **S. Link names** (T280): two links that read the same add their state, as DESIGN.md § List page says; draw one
    pair.
30. **S. The registrar page's missing states:** loading; the reminder's dialog, result, refusal and "Reminded" line;
    each section's own load error (This period, EPAs, trajectories, waiting, reviews); the page error with an h1
    ("Programme trainee") and its trail. Its margins stack to 48 px (`.index-section`, `.standing-panel` add their own):
    reset them in `.registrar-stack`. One heading scale for its sections.
31. **S. Words:** a card's count badge in words ("5 registrars", `BadgeWords`), not a bare "5"; the Filed checkbox
    without an orphan label; the Show select and the Show button not sharing a name (call the select "Waiting"); the
    apply-with-Show filter is a new list pattern: give DESIGN.md its rule text; the bars inside aria-hidden amend the
    progressbar rule: say so.
32. **S. The Spec's DESIGN.md list** also changes the staff target cards' paragraph ("Trainees in programme"), the
    "guard the eager reads" sentence ("Curriculum coverage"), and the owner rows (10).
33. **N.** Try again's focus goes to the frame's ActionResult region (T350), not a card heading; no inline layout
    styles in shipped markup; captions on every table and `data-label` on the Waiting State cell; one no-match
    wording and one clear button for both lists; empty states draw no filter form; "Registrars behind" lists everyone,
    ahead too: title it "Registrars" or say "fewest met first"; the subtitle "assessor, supervisor or reviewer" under
    "Waiting for assessors": keep the title, say "assessor" (E3 makes it true); an EPA everyone has met links to a
    no-match list: leave its name as text.

### States missing

The registrar page (30); Registrars behind past five rows ("3 more in Programme trainees."), and a heavy committee
Home; Programme trainees and Waiting for assessors as the Sub-speciality admin; the reminder in flight (confirming,
Cancel disabled); the ended registrar's past review link at 390.
