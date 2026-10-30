# Flow 14 build: the review (T387 step 6)

The build on `t387` (§ 0, lanes D, K, M, P and the runbook lane W merged; HEAD `374b55c9`) was reviewed 2026-10-30 from
three sides, each a read-only Sonnet agent: **the code** (`build-review-code.md`, S1–S3 and N1–N8: correctness, data,
scope and authorization), **the pages** (`build-review-pages.md`, P1–P11: the boards, the design system,
accessibility) and **the runbook** (`build-review-runbook.md`, R1–R9). The three files are kept as they are; this one
merges them. Severity: M must-fix, S should-fix, N nit. No reviewer ran the six suites or the scripts.

One should-fix was the operator's to decide, and was decided before the fix pass: **code S1** (2026-10-30), an approved
erasure scrubs the person's own text on every one of their data-rights requests, the reason and every correction's
before and after values, to "Erased with the person's data on ‹day›."; the staff's decision note is kept.

## Must fix

- **P1** DESIGN.md's banner of redesigned flows ends at flow 13; flow 14's amendments are in the body only.

## Should fix (all taken into the fix pass)

- **Code S1** an erasure rewrites the requester's name on their requests, but leaves what they typed (the reason) and
  the values a correction recorded (a corrected name), so a pseudonymous request can name the person.
- **Code S2** Record a correction does not put the request row in its save, so its `xmin` never refuses a correction
  that races Complete request (or the erasure that withdraws it).
- **Code S3** the request page's refusal branches no test reaches: the forecast null or throwing, Complete refused by the
  command, Record's `ValidationException` mapping and `NotAnApprovedRectification`.
- **P2** Show, Clear filters, a page turn and a change of size put the queue back into its loading skeleton, dropping the
  page-size select's focus and a typed but unapplied filter; Users answers in place.
- **P3** the DESIGN.md amendments Spec 4 and `round-3-check.md`'s build note 7 name are mostly not written (N17, § Table
  system, § Alerts, § Detail page and the title rule, the queue's and the request's own sections), and `:3426` still says
  the crumb is the id "once it has loaded".
- **P4** a decision note over 4,000 characters is refused as "A decision note is required."; the note has no counter.
- **P5** the erasure dialog opens on Cancel only if the browser's first focus says so (inferred): action mode puts no
  `autofocus` on Cancel, and a scrolling body is a focusable area in Chromium.
- **R1** "Erasure with nothing else open" names a registrar in a running programme, whose result reads D09, not D22.
- **R2** the bulk, second-institution and staff-with-a-review-elsewhere recipes give no way to build the state.
- **R3** "After the erasure" describes the board's filtered queue but the step shows five rows.
- **R4** "The person's earlier requests rewritten" produces a different mix of rows from K02's.

## Nits

Code N1 (Apply does not refuse the two name records for an institutional sign-in), N2 (scope before Withdrawn: correct),
N3 (the last-Administrator message says "only" for "only active"; two Administrators erasing each other at once), N4
(`ReadValue` throws on a legacy value), N5 (unknown and foreign ids differ in the command's exception), N6 (the queue has
no tie-break), N7 (a pre-migration correction's empty recorder), N8 (no bound on a correction's values); pages P6
(refusals not dropped on change; the record radios not `aria-invalid`), P7 (a stacked row header keeps a rule), P8 (two
"to the second" implementations), P9 (the page words read an Infrastructure constant), P10 (the inline programme link is
20px and unnamed in DESIGN.md; `.request-id` breaks a fitting id), P11 (the request page does not re-read on a new `Id`);
runbook R5 (the no-salt row starts with no Erasure), R6 (A.7.3's "every cell but Submitted"), R7 (the Outcome's replay
check reads as current), R8 (the request page's 390 order), R9 (the mail steps cannot verify a recipient).

## Held (checked and fine)

Authorization and scope on every command and read (`DataRightsAuthorization`'s one role set; the three reads null for an
unknown and a foreign id alike; the forecast gated as the command; the queue by institution, nothing for a Coordinator
with none); the erasure's transaction and its request block (all tracked, inside the save before the account); the
concurrency token and the audit pipeline (T201); the mails after the commit, never on a refusal, the erasure's to the
address read first; migration `T387_DataRightsFlow14` with its Designer and snapshot; `ApplyRectification`'s keys and
canonical jsonb values; the stored names. The pages' words against the boards word for word (bar F7's reminder clause and
the note's full stop, both right); one h1, titles, tokens, no inline style, `Icon`, `IScopedSender`, no exception text,
SAST; refusals under fields; results focused; 44px at 390 bar the inline link; the badges. The runbook's quoted Expects
against the code, A.1.8a end to end, the Outcome's counts, the 332 steps and 42 not-yet-captured rows.

## The fix pass (`t387`, 2026-10-30)

Each finding was checked against the code first; all held.

- **Fixed:**
  - **S1** `ErasureExecutor`'s request block writes `DataRightsLabels.ErasedText(erasedOn)` ("Erased with the person's
    data on 2026-10-30.", the erasure's South African day) into the `Reason` of every request of the person, the
    approving one included, and into `FromValueJson`/`ToValueJson` of every correction on those requests (a JSON string,
    read back canonically), all tracked in the same save (xmin). `DecisionNote` and `TargetType` are kept, and another
    person's request the erased one decided keeps its reason and values. `ErasureEndsOpenRecordsPostgresTests` adds a
    correction "Sipho" to "Siphiwe" on the person's own Rectification and asserts every reason and both values scrubbed,
    the note kept, the control request untouched; `ErasureCoverageTests` pins the four writes and that no `DecisionNote`
    is written. The request page shows the scrubbed reason and values as it shows any (no change needed).
    CUSTOMIZATION.md § 5 says what erasure does to requests (and its stale "asks for no confirmation" line is replaced
    by the dialog); A.1.12's Expect, the kept-request state and the Outcome's SQL read the scrubbed reason. A.1.14 reads
    no request field.
  - **S2** `ApplyRectification` marks the request's `Status` modified, so the save is an UPDATE carrying `xmin`; a
    `DbUpdateConcurrencyException` is refused as `NotAnApprovedRectification` with the cause inside, as Complete does.
    `DataRightsRequestRacePostgresTests` adds a correction saved after another context completed the request: refused,
    nothing recorded, the request Completed.
  - **S3** eleven `DataRightsRequestPageTests` cases: the forecast throwing and null (still decidable: ForecastFailed, no
    dialog, the note kept; withdrawn meanwhile: Request's refusal; decided meanwhile: Decision's), Complete refused by
    the command (`NotAnApprovedRectification`, read again; `NoCorrectionRecorded`, beside the button, focus kept; any
    other: CompleteFailed), Record's validator mapped by property (FromValue and ToValue under their fields, TargetType
    under Other's name or under the record, an unmapped property the fixed failure) and `NotAnApprovedRectification`.
  - **P1** the banner lists flow 14 with the sections it amended. No commit hash, as flows 11–13 carry none.
  - **P2** `RequestsList.LoadAsync(showLoading)`, true for the first read and Try again only; the fields are reset from
    the filter only then and on Clear. A queue test holds a change of size's read: the form, table and pager stay, no
    loading line, no focus call.
  - **P3** DESIGN.md: § Form system (N17, the refusal first, and a refusal dropped on change; the decision note among the
    counters), § Table system (the three data-rights tables), § Alerts (the standing erased notice, the live name in
    results), § Button system (Cancel's autofocus), two new sections under Page-level patterns ("Data rights requests" and
    "A data rights request", with the title rule, the one-column record page and the inline-link exemption), the crumb
    line corrected, and the section-order row.
  - **P4** the note has a counter (`decision-note-count`, counted in the browser by `review-appeal.js` as My data rights'
    reason is), named last in `aria-describedby`; over 4,000 characters it is refused before any send, "Give your note in
    4,000 characters or fewer.", and the validator's refusal is mapped by length. Tests for 4,001 (refused, the counter's
    excess) and 4,000 (sent).
  - **P5** a real defect (inferred, as the reviewer says; to confirm in the browser): `ConfirmDialog`'s action mode puts
    `autofocus` on Cancel unless `InitialFocusId` names a field, so the dialog focusing steps pick Cancel first. Every
    other dialog already opened there, so nothing else moves; two `ConfirmDialogFieldModeTests` cases.
  - **R1** D22's recipe names Dr du Plessis (no open review, campaign, request or programme) and says why a registrar in
    a programme reads D09.
  - **R2** the bulk recipe is one `INSERT … SELECT … FROM generate_series(1, 45)` over the Demo dev accounts and two KGK
    consultants (every offered type, five statuses, one with no institution); the foreign id is the Demo coordinator's
    Export; D13's is Dr Mahlangu given Assessor and one of her reviews moved, open, to a Demo panel (both in SQL); the
    60-character address is a Prof Mbatha invitation.
  - **R3** "After the erasure" chooses Erasure and presses Show again, so the capture is R3-Q07's.
  - **R4** K02's recipe makes Access Withdrawn (by the erasure), Export Completed and a Rectification Rejected beside the
    Erasure, and the reason reads the scrubbed text.
  - **Nits:** N3 (the command's message says "only active Administrator"; the page's board words unchanged), N4
    (`ReadValue` returns the stored text for a value that is not a JSON string), N6 (`ThenByDescending(Id)`), P6 (each
    refusal goes on its field's change; the record radios `aria-invalid` while refused; tested), P7 (no rule under a
    stacked row header at 900px; a stylesheet test), P9 (`DataRightsRefusalSentences.PersonChanged` in Application; the
    executor's constant points at it), P10 (`.request-id` loses `word-break: break-all`; the inline link named in
    DESIGN.md), P11 (`OnParametersSetAsync` reads again when `Id` changes), R5 (the no-salt row submits an Erasure first),
    R6 ("every text cell but Submitted"), R7 (the replay check "predates T387"), R8 (the request page's 390 row says
    Approve above Reject), R9 (a sentence in A.1.12's Note). `build-contracts.md` § 6 lists the words shipped that no
    board draws. The flows' verbatim copies re-synced (`sync_verbatim_steps.py`).
- **Left:**
  - **N1** the command records that a correction happened and changes no account; refusing the name records for an
    institutional sign-in is a rule the operator may want, not a defect (a forged circuit message stores a record only).
  - **N2** correct as built; the plan line is history.
  - **N3's race** two Administrators erasing each other in one second: a lock across two users' rows for a case that
    needs two Administrators acting at once; noted.
  - **N5** pre-existing across the feature; no page shows either message.
  - **N7** nothing is live and no page recorded a correction before T387 (CLAUDE.md § Nothing is live).
  - **N8** Blazor's message limit caps the values; a bound needs words on the page and a validator rule, more than a nit.
  - **P8** a refactor of two words classes for no behaviour change; both produce the same text.
