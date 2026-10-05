# Flow 06 build: the review (T358 step 6)

Three read-only Sonnet reviewers on `t358` at `4811e8bb` (`review-lanes.md`): **R** the code, the data and authorization;
**D** the pages, the design system and accessibility; **G** the runbook, the cast and regressions. No high finding.
**Stopping line** (agreed before the fix pass): one fix pass (`t358-fix`) for every real defect and every doc that now
says something false; the rest recorded here, and nothing filed that a later task already holds.

## Fixed in the pass (`t358-fix`)

1. **D1 (medium, confirmed). A not-found page lights its owner.** `/programme/trainees/{ProfileId}` for an out-of-scope
   or erased registrar draws Page not found inline, but `NavMenu` lights by the routed page (`NavOwners.Lit`), so
   Programme trainees reads `aria-current`. `PageHeader`'s `Page="typeof(NotFound)"` reaches only the trail. The same
   holds for Programme trainees' own not-found and flow 05's not-found EPA page (My progress lit). Fix once in the shell:
   a page that draws not found tells the shell (a scoped holder `NavMenu` reads before the route); a test mounts the
   menu with each of the three and asserts nothing is lit. Step A.5.14's Expect then holds as written.
2. **R2 (low, plausible). A draft someone else created counts as filed.** `FilingMoments.CreateIsTheFiling` treats a
   create as the filing when the subject cannot move it out of its initial state; a draft created about the registrar
   by someone else then reads as filed at `CreatedOn`. A create is the filing only when the type's initial state is not
   a draft (born `requested` or terminal), as CLAUDE.md's SchemaValidator note defines a create that is the filing.
3. **R3 (low, confirmed).** `ListWaitingForAssessorsQuery` gets a validator bounding `PageSize` 1..100 and `Page` ≥ 1, as
   `ListProgrammeTraineesQuery` has.
4. **G1 (medium-low, confirmed). The Nothing filed rule line misstates the rule.** "…in the last 30 days, or since
   admission if that is later" reads as a window from admission; the code lists a registrar only once admitted at least
   30 days ago (E5's "later of admission and today − 30" means the window is not yet open). New words: "Current
   registrars with nothing filed (a draft is not filed) in the last 30 days. A registrar admitted less than 30 days ago
   is not listed." Steps 2.32 and the states that quote it follow.
5. **G2 (low-medium).** Steps 4.36 and 4.37 say Entrustment decisions is reached by typing its address and is in no
   menu: now "Open Entrustment decisions from the menu", lit on its page, no trail (as A.7.7).
6. **G3 (low).** A.2.6's Expect names who the digest lists: add that a draft is not a filing and a registrar admitted
   under 30 days ago is not listed.
7. **G4 (low).** `NavItems`' remark counts the grouped menus under the old count (17, 16): now 16 and 15.
8. **G5 (low).** 3.54b's EPAs bullet reads as an order: "among them", in code order.

## Recorded, not fixed

- **R1 (medium-low, not reachable today).** A reminder's history and the same-day block are keyed by request, not by the
  nominee it went to. While a request waits nobody may change its named assessor (`round-2-review.md` § Root cause, the
  field is locked), so the case cannot occur; it becomes real with Reassign. Noted on **T359**: a reassignment must
  scope `LastReminder` and `RemindedToday` to the current holder, or decide the rule is per request.
- **R4 (low).** The Homes read every current registrar's activities (Nothing filed) and every waiting candidate to show
  five. Slow only at scale; nothing is live. Revisit if a Home is slow.
- **R5 (low, cosmetic).** An erased sender or assessor shows as the pseudonym on a waiting row, as transitions do
  elsewhere; the row is marked no such account.
- **D3 (low).** The coverage bar's width is an inline style, the built `progress-bar-fill` pattern DESIGN.md already
  names (flow 05's index and EPA page do the same).
- **G, 3.54b's trajectories:** two charts (PAED-004, and Dr Mahlangu's DOPS on PAED-002) are right; the boards' "one
  trajectory" was stale. A.5.15 (Dr Patel's portfolio review) and A.5.10's order (portfolio review, Mini-CEX at 6 days,
  CBD) checked against the corpus: right.

## Sound (the reviewers' summary)

Scope is one role's, never the union, held to the institution; an out-of-scope, non-preferred, erased or unknown id is
not found; the command stages nothing before its checks, leaves the activity untouched, sends to an opted-out assessor,
blocks the same South African day (race caught, Postgres-tested) and pseudonymises on erasure; the migration, Designer,
snapshot, backfill and unique index agree; every new clock is South African; the roster shares My progress's tally and
ties by surname. The pages' words carry no "n / m", "inactive", pronoun or role key; dates ISO; five NEW classes only;
the reminder's dialog, focus and failure path as designed. The runbook's figures, orders and setups hold against the
corpus; flow 04's and flow 05's callers are unchanged; My progress's ended view is golden-tested byte-equal.
