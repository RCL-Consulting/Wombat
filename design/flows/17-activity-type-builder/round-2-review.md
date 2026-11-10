# Flow 17 round 2: the review

Round 2 (`round-2/`, 145 boards: Spec 1–3 and Steps, 69 page boards at 1280 with their 390 and 768 twins, three 390/768
boards without a base by design; `f17r2.css`) drew structure A at full fidelity: A the list, B reading a type, C a new
type and its form, D the workflow and credit, E Publish, Discard and deactivating. The canvas read all 30 captures and
the nine seeds from its own `project/ref/` and listed them first on R2-0-spec.

Reviewed 2026-11-10 from three sides, each a Sonnet agent: **the code** (`round-2-review-code.md`, `4adda465`: 0 must,
J1–J15, N1–N14), **the cast and words** (`round-2-review-cast.md`, `5dc373c7`: C1–C16, 3 must; all 64 numbered states
drawn) and **the design system** (`round-2-review-ds.md`, `747fd118`: M1–M2, 10 should, 6 nit). The three files are kept
as they are; this one merges them. Each must-fix below was checked by the integrator before it was carried. Severity: M
must, S should, N nit.

## What holds

- The twins match their base on text and attributes (code and cast: 73 of 73; three have no base by design).
- Every Spec line-number claim about today's code checks out (code); every decision Q1–Q16 holds in the boards and no
  drawn state is impossible (code); cast, FIXTURES, menus and the lit item hold; no gendered word (cast).
- By script: no undefined class, no inline style, no hex, one h1 a board, no heading skip; the tabs pattern and DataTable
  stack markup are right; every contrast figure on the Spec is right (ds).

## Must-fix

| # | From | Defect | Checked | Fix |
|---|---|---|---|---|
| M1 | ds M1 | Required fields carry the star (`required-mark`, `aria-hidden`) but no `aria-required="true"` and no "* marks a field you must fill in." line: 39 boards, 89 controls (Details on 22 files, every in-place editor). Only the preview has both. | `R2-C01`: 2 `required-mark`, 0 `aria-required` | Draw both on every form with a required control (DESIGN.md's form system). |
| M2 | ds M2 | Accessible names miss the visible words (WCAG 2.5.3): a tab with its "1 problem" chip is `aria-label="Form"` (12–13 files); C19's "Hide fields"/"Show fields" are named "Hide the fields of section X" and flip name and `aria-expanded` together. | 13 files with `aria-label="Form"` and `.tab-problem` | Drop the tab's `aria-label` (the chip is in its name, or `aria-describedby` alone); C19: visible words first in the name, a fixed name with `aria-expanded` doing the state. |
| M3 | cast C1 | The Steps board counts the Preview choice, a radio, as a press, against its own rule that a radio is a field. | R2-0-steps | 1.27 at 390 15 + 26 (not 17 + 24); 1.26–1.30 at 390 22 + 31 (not 24 + 29); A.7.9 3 + 1 (not 4 + 0). |
| M4 | cast C2 | 1.30 is pressed from the Credit tab and a save keeps the tab (T2), but R2-E02 draws Details selected; its Expect reads the Key on Details and the versions on Publish, two more presses. | `R2-E02`: `tab-details` `aria-selected="true"` | Redraw E02 on Credit with the result above the tabs; count 1.30's Expect's tab presses (Q20). |
| M5 | cast C3 | Five changed `states.md` rows have no Steps row: 902, 904, 906, 911, 912 (the ask omitted them too). | cast review's evidence | Add the five rows with their changed Expect. |

## Should-fix (carried to round 3)

- **Build feasibility (code).** J1 RefusalSummary is wired to flow 03's field ids and cannot switch tabs: a builder
  variant, or a parameter for each line's tab and target. J2 refusals are one joined string with 59 text-only throw
  sites: "carries its place" needs a list of (place, sentence), not one place. J3 nothing tells a refusal from a failure
  (both `InvalidOperationException`, printed by `RefusalText`): a refusal type. J6 `aria-disabled` Publish is not inert
  (`PublishAsync` has no `CanPublish` guard): add it. J8 the Publish tab's warnings need a stated source: under Q16 the
  saved draft. J9 `ConfirmDialog` has no success button but E05 draws `btn-success` (also ds): add a confirm kind, or
  draw `btn-primary`. J11 `ActivityForm` takes no EPA list; state the rule for Institution and Global types. J12 actor
  rules in words need a full grammar. J13 Details' refusals (blank, too long, key taken) undrawn. J14 an indented
  `Serialize` changes every caller, and the page never reloads the JSON after a save: a separate layout call, applied at
  load and after save. J15 T271's scale resolution is a service change. J4 D04's two credit sentences cannot both come
  from the code. J5, J7, J10 are questions below.
- **Words and states (cast).** 1.27's fields recount to 23, and its count assumes Add field opens the new row's editor
  (else + 5 presses: draw which). R2-A12 shows Draft "None" after R2-E12 saved one. Chips: A11, B11, E01, E03, D05 carry
  none though no step reaches them; E08 none where E04 has one. Spec lacks two NEW sentences (C07 "None: a Global type
  belongs to…", C20 "No fields in this section yet."). C19 folds sections 2–6 against T23's "all start open". No board
  for 1.24's cleared 22-row list. Learner Feedback's help texts undrawn beside MSF's.
- **Design system (ds).** `.tab-problem` duplicates `.badge.badge-declined`: use the badge. C3 copies nine rules: add
  `.activity-types-page` to the 1024px selector list instead. T4's 363px arithmetic ignores that `.actions-cell` wraps
  (the outline stays, for its other reasons; correct the Spec). The tab bar leaving the card has no Spec or DESIGN.md
  line, and `ContrastTests` reads `.is-active`: update the test with C1. Tab hover is invisible (`--hover-bg` equals the
  page ground). The stripe states have no forced-colors fallback. C19's placeholder ids hold spaces and brackets.

## Nits

Code N1–N14, cast's five and ds's six stay in their files; the round-3 ask takes them as a list.

## Questions for the operator

- **Q17 (J7). Publish with Active newly unticked on a published type.** Q16's save-first writes `IsActive` straight
  through, so Publish would deactivate without Q15's question. *Recommend:* Publish asks Q15's question first when
  Active was newly unticked, then saves, then asks about warnings if any; Cancel on the first leaves everything unsaved.
- **Q18 (J5). A setting that points at a deleted field.** R2-C18 draws a refusal; the code drops the pointer at save on
  purpose (T133), and the Publish warnings already say "This form will stop recording which field carries the encounter
  date." *Recommend:* keep T133; redraw C18 as the select reading "None" after the save, with that warning on the Publish
  tab, and drop the NEW refusal sentence.
- **Q19 (J10). Who reads an inactive system-managed type.** Q9 makes MSF and Learner Feedback read-only for everyone,
  but the read rule as written would lock an inactive one out. *Recommend:* Q9 changes writing only; reading follows the
  ordinary rule, so an inactive system-managed type stays readable (View) to the roles that read it today. One existing
  test inverts with T334.
- **Q20 (M4, T2). Where Publish lands.** *Recommend:* on the tab it was pressed from, the result above the tabs, as a
  save does (T2); the Steps board counts 1.30's Expect's reads as the presses they cost.
