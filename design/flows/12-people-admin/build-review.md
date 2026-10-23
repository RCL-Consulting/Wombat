# Flow 12 build: the review (T376 step 6)

The build on `t376` (§ 0 `de324a6e`; lanes T, I, U, P merged; the runbook lane `1d254b02`, `8d841789`; merge `aeb29eda`)
was reviewed 2026-10-23 from three sides, each a read-only Sonnet agent: **the code** (`build-review-code.md`, B1–B9:
correctness, data, scope and authorization), **the pages** (`build-review-pages.md`, P1–P16: the boards, the design
system, accessibility) and **the runbook and cast** (`build-review-runbook.md`, R1–R7). The three files are kept as they
are; this one merges them. Severity: M must-fix, S should-fix, N nit. Six suites green on `t376` before the review:
Domain 830, Application 4108, Infrastructure 1033, Architecture 52, Web 4131, Integration 510.

Checked by the integrator before carrying (observed): the merge's resolutions (every lane's `app.css` section and
DESIGN.md paragraph present once); B1's query reads no institution (as the reviewer quotes). The rest are carried on the
reviewers' evidence, for the fix pass to confirm before changing code.

## Must fix

- **P1** DESIGN.md's "Redesigned so far" banner stops at flow 11: name flow 12 (T376), as each flow's banner line does.

## Should fix (all taken into the fix pass)

- **B1** `GetRequestsWaitingForUser`: filter the activities by the person's institution for a non-Administrator, so an
  assessor moved between institutions shows the new admin nothing of the old institution's trainees.
- **B2** `AddRoleToUser`: read the held scope and compute the resulting scope inside the transaction (M2's guarantee).
- **B3** `CreateSsoGroupMapping`: check a mapping's speciality and sub-speciality as Add role does (adopted by the
  institution, the pair consistent, what the role needs: `AdoptedDisciplines`, `RoleTakesSubSpeciality`,
  `RoleNeedsSpeciality`).
- **P2** Users' Institution and Scope cells (`UsersList.razor:158`, `:176`): no mid-word split at 901–1100 px (a floor, as
  flow 11's Delivery column, or no `.col-wrap`); the replay measures it.
- **P3** the Users row header's `break-word` rule must be scoped to Users, not reach SSO's group ids
  (`GroupMappings.razor:327`).
- **P4** the remove-sign-in dialog's "a password still works" only when the account has a usable password; with two
  sign-ins and no password, say the other sign-in still works (`UserPageText.cs:243`).
- **P5** the dozen sentences on no board: each marked NEW in `build-contracts.md`'s words table, or replaced by a board's.
- **P6** a rows-per-page change keeps the focus on the select on Assessors too, as on Users (one rule for both pagers).
- **P7** GroupMappings' Try again: `FocusAfterRetry`, as every other page's.
- **P8** `.dashboard-grid .detail-list` scoped to her Home, so the Trainee's Home is unchanged.
- **P9** the user page shows one result at a time: Save training clears the page's result and the page's actions clear
  the section's (the contract gains the reset P asked for).
- **R1** `states.md:594`, `:605`: Entrustment decisions from her menu (F10), not Home's Quick links.
- **R2** 1.6, `states.md:786`, `coverage.md`: the Create institution form's refusals in Wombat's words (F-1.6b closed).
- **R3** A.6.9 (`appendix-cross-cutting.md:1132`): devadmin's own page shows the Institutional sign-in section.

## Nits (taken where they cost a line)

B4 (`UpdateInstitution` calls every `DbUpdateException` a duplicate: check the constraint name), B5 (Home's count reads
the whole institution and asks no trainee-first check), B6 (`ListAssessors` pages in memory), B7 (`GetAdoptedDisciplines`
asks no trainee-first check), B8 (the in-memory default sort is ordinal), B9 (`AwaitingNominees` scans `DataJson` as
text); P10–P16 as listed in `build-review-pages.md`; R4 (no step quotes "1 of 14 are locked out"), R5 (2.13's 390 count
not on the Steps board), R6 (5.10 reads the user id from an address that now carries `?back=`), R7 (`states.md:8`'s "67
page files").

## Held (checked and fine)

The F4 migration with its Designer and snapshot; nothing reads the dropped columns; the nudge job's behaviour; the
deleted queries had no other readers; T288 and T333 not regressed; one h1, h2 sections, captions, row actions named per
row; destructive triggers outline, the filled danger button only in ConfirmDialog; no raw hex, inline style or Bootstrap;
no undefined class; F9's guard `TotalCount > PageSize`; every quoted Expect is what the code prints; the cast and counts;
the step order (2.14's route, 2.15's redirect, Act 2's outcome SQL); Users and the user page agree on Locked out
(`IsDeactivated`); her own page shows Institutional sign-in, as P01 draws and 2.12 says.

## The fix pass (`t376-fix`, 2026-10-23)

Each finding was checked against the code first; all held.

- **Fixed:** P1 (the banner names flow 12); B1 (an Institutional admin reads only her institution's waiting requests);
  B2 (inside the transaction the role is written first, which holds the account's row, then the scope is read again
  and the union recomputed; a refusal only that read finds rolls the role back); B3 (a mapping's speciality and
  sub-speciality checked as Add a role… checks them, typed refusals under Speciality and Sub-speciality); P2 (Users'
  Institution and Scope `th.col-wrap`, a 7rem floor); P3 (`.group-id` breaks anywhere); P4 (the dialog by account);
  P5 (`build-contracts.md` § 6); P6 (Assessors' size change reads in place, the focus kept on the select); P7
  (`FocusAfterRetry` on SSO mappings); P8 (`.home-institution-list`, her card only); P9 (`PageResultVersion`: the
  page's action clears the section's result); R1–R3; nits B4 (only a unique violation is a duplicate), B7
  (trainee-first), P10, P14, P15, R4–R7. The flows' verbatim copies of 1.6, 5.10 and A.6.9 and the appendix line
  references were re-synced (0 problems).
- **Left:** B5 (a trainee-first on Home's summary would fail her Home, not refuse a read; the page gates it), B6, B8,
  B9 (fine at this size), P11 (a legacy redirect), P12 (no pager, so no Rows per page, once a large size fits the
  matches: for the handoff), P13 (an inline link in a sentence, WCAG 2.5.8's exception), P16 (flow 18's).
