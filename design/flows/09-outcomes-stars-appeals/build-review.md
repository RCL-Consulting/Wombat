# Flow 09 build review (T366 step 6)

Three read-only reviewers on Sonnet reviewed the branch `t366` after the three page lanes merged (T `a7fe22c5`, S
`bfa3859f`, C `4c5cff9b`) and, for G, the runbook lane (`9b720692`): **R** covered the code, the data and authorization;
**D** the pages, the design system and accessibility; **G** the runbook, the cast and regressions. Severity: H high,
M medium, L low. Each finding was traced in the code by its reviewer; P marks one the reviewer could not fully trace.
**No high finding.** The fix column says what the fix pass (`t366-fix`) does; "Kept" and "Replay" say why not.

## R: the code, the data and authorization

**Sound:** her review's read (own trainee, Ratified or later, every role; unknown or other is null); the committee's read
unchanged for every role; one name lookup a page; `LodgeAppeal`'s refusals before the mutation and its 4,000-character
limit in the validator; the list's order; the register's `IssuingReviewReadable` judged per viewer in one call, its
successor read and reviser names with bounded queries; the revoke's refusals before `Revoke`, T367's notice after the
save; the certificate's access unchanged; `ConfirmDialog` unchanged for its existing callers; dates from the South
African helpers.

| # | Sev | Finding | Fix |
|---|---|---|---|
| R1 | M | `ReviewDetail.razor` `ExecuteAsync` shows `RefusalText.Of(exception)` for every exception; the resolve path now leads it "Resolve appeal was refused:", so a database or concurrency failure prints framework text as a refusal | Classify as the register does: a handler's refusal (`ValidationException`, `UnauthorizedAccessException`, `CommitteeRefusedException`, the known domain sentences) shows its words; anything else a fixed sentence, logged. Same as D3 |
| R2 | L (P) | The page-side remit check and the handler's "the resolver is present" rule may diverge for a resolver who cannot sit | A test: an External resolver whose `MaySit` is false is refused on the page, or passes to the handler, and either way nothing is lost |
| R3 | L | A revoke reason over 1,000 characters shows FluentValidation's default wording | `WithMessage` in Wombat's words, as the lodge has; with D2 |
| R4 | L | `StarWords.At` hard-codes +2 h | Through `ProgrammeCalendar` / `ActivityMoments` |
| R5 | L | `InitialFocusId` is focused before the dialog's body may have rendered | Render first (a flag focused in `OnAfterRenderAsync`), then focus |
| R6 | L | Two tabs lodging at once: the loser's `DbUpdateConcurrencyException` reads "could not be lodged" though the appeal exists | Map the concurrency failure on Lodge to a re-read and the refusal "Only ratified reviews can be appealed." |

## D: the pages, the design system and accessibility

**Sound:** no non-negotiable broken; every class defined, the new ones only in the three Flow 09 sections; phone rules
scoped; the round-3 bans absent; one h1 a page; tables captioned and scoped; row actions carry their visible words;
the jump through `PageFocus`; the revoke dialog's first focus and blank refusal; the counter a `.field-help`; the
filtered count's status; the shipped words as the Spec fixed them; ISO dates; no Upheld, no deferral; the `.star-grid`,
the stacked tables, the one-column resolve form; DESIGN.md agrees with the code but for D2.

| # | Sev | Finding | Fix |
|---|---|---|---|
| D1 | M | My authorisations' Try again removes the alert and its button, so the focus falls to the page | Each Download button gets an id; the retry moves the focus to it (`PageFocus`) |
| D2 | M | The revoke box has `maxlength="1000"` and `@bind:event="oninput"`: a long paste is cut silently, the validator can never fire, and every keystroke is a round trip, against DESIGN.md § Form system's counter | No `maxlength`; the counter counted in the browser by `review-appeal.js` (generalised); over 1,000 refused under the box in Wombat's words (R3) |
| D3 | M | As R1; and `LodgeAppeal` wraps every `InvalidOperationException` as a refusal | R1's fix; `LodgeAppeal` wraps only the domain's own state rules (the two sentences it throws) |
| D4 | L (P) | Stacked note row: `td.history-note` doubles its row's padding at ≤900 px | `padding: 0` on the stacked note cell |
| D5 | L | "Show the whole reason" is measured once; a resize or a late font can leave it wrong | Re-measure on resize (`ResizeObserver` in the module) |
| D6 | L (P) | Keep editing and Go back rely on the browser returning the focus; Safari and Firefox on macOS do not focus a clicked button | `ConfirmDialog` records the opener (`document.activeElement`, else a caller-given id) at `ShowAsync` and focuses it on close by Cancel or Escape |
| D7 | L | A failed download's Try again inside No longer in force is 28 px at 390 | Add `.my-authorisations .download-result .btn` to the 44 px rule |
| D8 | L | Native `disabled` on busy actions (other Downloads, Resolve appeal while another action runs, Cancel while confirming) | **Kept:** the busy-state convention of flow 07 and T234; DESIGN.md already names it |
| D9 | L | The dialog's `aria-describedby` body also wraps the revoke field, so the label and counter are read twice | The describing paragraphs get their own id; the field sits outside it |
| D10 | L | Clear filters twice at once when nothing matches | One: the empty state's sentence without its own button |
| D11 | L | My committee reviews' rows have no row header | Period a `th scope="row"` |
| D12 | L | `.detail-list dd .badge` is unscoped | Scoped to `.my-review-page` and `.my-authorisations` |
| D13 | L | "They", "theirs", "None of them" for the appeal body | **Kept:** a group, the boards' words; no person is named by a pronoun |
| D14 | P | `review-appeal.js` is the repo's first dynamic import | **Replay:** `/js/review-appeal.js` 200 and the counter working, under the CSP |
| D15 | P | The result's focus after Remit or Dismiss depends on the render arriving after the dialog closes | **Replay** (4.47) |
| D16 | P | The radios' targets at 1280 and 768 | **Replay:** measure them |

## G: the runbook, the cast and regressions

**Sound:** every Expect of the 17 steps against the code (titles, names, results, refusals, dialogs, counts); the cast
(reviews #1–#7, STARs #1–#18, #18 superseding #2, 18 rows at A.7.7, D+20 = 2026-11-02); no other step dismisses or
revokes; the old words survive only in Actual history; the new route's coverage row; the new states rows; the six gap
closures; `check_verbatim_steps.py` 424 steps 0 problems, `check_flow_completeness.py` 331 steps none missing.

| # | Sev | Finding | Fix |
|---|---|---|---|
| G1 | L | 4.41's Period cell quoted with a "·"; the page now puts the window on its own line | The Expect says the window is under the label |
| G2 | L | Act 4's outcome check says "the act wrote no mail (29)"; 4.36 now mails Dr Dlamini | Say 4.36 writes one mail; the counts are T364's history |
| G3 | L | `coverage.md:139` says 84 templates; 86 | 86 |
| G4 | L | The empty register, empty My committee reviews and My authorisations' None yet are placed at steps that do not visit those pages | Each gets a step that visits the page, or a scratch instruction |
| G5 | L | `EntrustmentDecisionExpiryJob` reads the UTC date; My authorisations the South African one, so from SAST midnight to the 03:30 UTC run a STAR expired yesterday reads In force, and the states row's SQL fallback fails between 00:00 and 02:00 SAST | The job on the South African calendar (`ProgrammeCalendar.DateOf`); the states row's fallback D−2 |
| G6 | L | The certificate prints "Revoked on {d MMMM yyyy}" from the UTC date | ISO, on the South African calendar |
| G7 | L (P) | The focus after a resolve, once the dialogs are removed | **Replay**, with D15 |

## From the runbook lane (observed, not fixed by it)

- The decision history's Present line ends without a full stop; the remit dialog's with one. **Fix:** one form (no full
  stop, as the history has it, which 4.47's Expect quotes).
- The revoked certificate's long-form date: G6.
- Native `disabled` while busy: D8, kept.
- The expired STAR only on a scratch database: as F2 decided; G5 makes its fallback hold.

## Fixed (the fix pass, `t366-fix`)

**Fixed:** R1, D3 and R6 `4eff913f`; R2 `b3b6d59f` (a test: the page counts only who may sit, the handler refuses the
resolver, nothing typed is lost; no code changed); R4 and D5 `e91fa0cf`; R5 and D6 `d0c89cd1`; R3 and D2 `b74176b8`;
D1 `f981813f`; D9 `234b1a14`; D10 `a08a79e9`; D11 `71e064b8`; D4, D7 and D12 `da54be63`; G5 `be625647` (the job, and
`EntrustmentDecision.MarkExpired`'s guard, which read the UTC day too), its states row's fallback `dc40d4c9`; G6
`24aba589`; G1, G2, G3, G4 and the Present line `dc40d4c9`. Not done, as the table says: D8 and D13 (Kept), D14, D15,
D16 and G7 (Replay).
