# Flow 04 · Round 1 — the review and the pick

Round 1 was drawn on 2026-09-30 from `round-1-ask.txt` (§ 1 and § 8 as one message). Canvas version `1790764233-784e`:
26 boards saved in `round-1/project/`: `Main` (the three structures, their step counts and the canvas's own pick),
`R1-Steps`, `R1-Spec`, A1–A5, B1–B3, C1–C6, and the assessor's activity page drawn once: the component board
`R1-C-Activity` with six states, shown by P1–P8. This time the canvas gave step counts. The reviewer checked them
against the code and the runbook, and they hold where the structure holds.

## The shared parts (P1–P8, `R1-C-Activity`), common to all three

1. **The rung picker** (Q5): the rung row gets radios, six abreast at every width (about 50 px each at 390). The
   chosen rung is filled and marked "chosen" in words, and its descriptor sits under the row. Beneath it is "What each
   rung means", open at 1280 until a rung is chosen and folded on a phone. Each radio is `aria-describedby` its own
   descriptor. The legend is the field's label with the red `*`, and the College's help sits under it.
2. **One note panel** for Decline and Return. It is headed "Decline this request" or "Return this reflection", its note
   is labelled "Note for Sipho Ndlovu" with the form's red `*`, and both moves share the same help. Its send button is
   danger for Decline and primary for Return, and its keep button is "Keep the request" or "Keep the reflection". A
   refusal reads "Not declined. It is still Requested." with the field's line, and the panel stays open (T299, T349).
3. **Moves in sentence case**: Complete, Decline, Record discussion, Sign off, Return, on the bar and in the history.
4. **On a phone, the rating first** (the ask's question 7; § 6's question 7 is the year minimum, below). For a reader
   with a section to fill, the filled Request folds into one line (`request-fold`: who filled it in, a gist, Show).
   About moves under the bar. At 1280 the order is flow 03's.
5. **The way on** (Q6), NEW `WayOn`, sits inside the move's `ActionResult`. It holds "1 more waits for you", the next
   item's row, **Open the next** (named for its item) and Back to Activity inbox. With nothing left it reads "Nothing
   else waits for you." and offers Go to Home. C replaces this with an automatic advance.
6. **Smaller fixes**: Discard changes, greyed, now says why ("nothing to discard yet"). The Request names people without
   their email. "Activity unavailable" links to the acting role's list: Activity inbox for an Assessor.

## The three variations

| | A · Two pages, one list | B · Home is the inbox | C · A queue you work through |
|---|---|---|---|
| Home (Assessor) | "Waiting for you": the inbox's first five rows from one read, in one order and one set of words (NEW `WaitingList`), counted "3 · 1 overdue". Then Recent decisions, five and dated, with "All your decisions" | The whole list, 25 a page, then "Decided by you", paged. Activity inbox leaves the Assessor's menu, and `/activities/inbox` sends an Assessor to `/` | Two figures (waiting, overdue), the oldest named, "Start with the oldest", and Recent decisions |
| Activity inbox | Two sections: Waiting for you (Activity, EPA, State, Waiting) and Decided by you (Activity, Decision, Decided, Credit), 20 a page, no cut-off. Stacked at 390 | Gone for the Assessor | Grouped: Overdue, then Waiting. My decisions is a new page and menu item, with a search |
| Two-role Home (Q3) | One line on the other view's Home naming the count, whether any is overdue and the oldest, with Open it (no switch) and Switch to Assessor | A card of the waiting rows on the committee Home | A count on the sidebar's switch, on every page |
| Overdue (Q4) | A badge beside the state, oldest first, and the count | As A | As A, plus a group heading the inbox |
| After a move (Q6) | `WayOn`: Open the next · Back to Activity inbox | The same, with Back to Home | The next item opens by itself, and the result names what was just done. NEW `QueueNav` in the header |
| Rate from Home, 1280 | 3 presses | 3 | 3 if it is the oldest, else 4 |
| Two-role consultant rates an overdue request, 1280 / 390 | 3 / 3 | 3 / 3 | 5 / 7 |
| Botha returns, then records the discussion | 5 (today 11) | 5 | 9 |

## Reading them against the code and the brief

- **C breaks the "may not" list, and it is the slowest where the brief hurts most.** Its automatic advance moves the
  result onto another activity's page. Flow 03 fixed "the result's words and focus" and the status card ("Done. You
  completed it on …", "Rated 3a. Credited 1 item to PAED-002."). Under C the rater never reads either for the record
  they just rated. C admits this (C5's note, "Risk"). `QueueNav` adds a header action to flow 03's page, which is also
  on the "may not" list.
- **C's count on the switch** means running the inbox's actionable read (`ActivityWaiting.LoadActionableAsync`, the act
  gate on every candidate row) in `MainLayout` on every page for every two-role user. Inside a circuit the count also
  goes stale until the next full load. A two-role consultant still needs 5 presses where A and B need 3.
- **B would unpick more of flow 03 than the canvas says.** The canvas's case against B (Main, "Recommendation") is that
  "the Coordinator's and committee's pages link to" the inbox. None do: the only link to `/activities/inbox` is the
  Assessor's own card (`AssessorDashboard.razor:39`), and the Speciality admin's card is left unlinked on purpose
  (`SpecialityAdminDashboard.razor:9`). B's real costs:
  - Flow 03's built copy tells the registrar where her request went: "It is in Fatima Khumalo's Activity inbox."
    (`FilingWords.cs:136`, `:144`; `SubmitCheck`; the Cancel dialog's "It leaves … Activity inbox",
    `ActivityPageModel.cs:210–212`). The runbook quotes it in Steps 3.3, 3.10, 3.12, 3.14, 3.22, 3.25 and 3.29.
  - The owner table lights Activity inbox over the activity page (`NavOwners.cs:92`).
  - Thirteen of the 17 steps route through `/activities/inbox`, and A.4.3's return address lands there.
  - The inbox is also the Speciality admin's and the Coordinator's, through `role:` arms (`ActivityInbox.razor:11–12`,
    G6). B's redirect would have to depend on the acting role.
  - B's Home becomes a paged list page. DESIGN.md § Dashboard page makes each dashboard a card composition with no
    `PageHeader` of its own, and puts no pager on it.
  - B3's card of rows on the committee Home is the merged view the brief's question 3 rules out (W-010: "a line or a
    count, not a merged view").
- **A is buildable from what exists, with four new reads:**
  - **The one list.** NEW `ListWaitingForYouQuery` (the canvas's name) over `LoadActionableAsync(…, ActorArms.NotAuthor)`,
    oldest first by `UpdatedOn` (`GetAssessorDashboardSummaryQuery.cs:72`), carrying the inbox's EPA and date
    (`ListActivitiesByActorInboxQuery`). Home takes the first five, the inbox all of them. Today the inbox keeps
    `ActivityWaiting`'s newest first (`ActivityWaiting.cs:103`), which is T297's disagreement. The two reads also
    differ in one filter: Home drops the caller's own subject rows (`GetAssessorDashboardSummaryQuery.cs:71`) and the
    inbox does not. One read settles it.
  - **Decided by you, paged.** Today's decisions loop (`:84–134`) reads 50 at a time and stops at ten. A page with a
    total ("1–5 of 12") must walk everything the caller moved last. That is small for an assessor. Its Credit column is
    the inbox's latest-credit rule (T108). The definition does not change: moved last by you, and finished or with no
    move left (T297).
  - **The way on** is one more waiting read after a move, less the current activity.
  - **The other-role line** is a waiting read on the committee dashboard for a holder of Assessor. The act gate reads
    claims, not the acting role, so it finds her rows. Opening the activity from it is today's rule: an activity opened
    outside the acting role's menu lights nothing and its trail is Home › the activity (`NavOwners.cs:88–94`, DESIGN.md
    § The NavMenu). So the Spec's open question has its answer.
- **"Waiting since" has one clock, and it is `UpdatedOn`, not "its latest Submit" as the Spec proposes.** Home's
  Overdue (`GetAssessorDashboardSummaryQuery.cs:62`, `:151`), the Coordinator's stall and the nudge
  (`AssessorPendingNudgeJob.cs:58`, `:103`) all read it. Step 3.30 ages it by SQL, which is how 3.33's Overdue appears
  at all. A Submit clock would lose that Overdue in every replay.
  - The status card's "asked you on …" is the newest move (`ActivityHolderDto.Since`, `ActivityDtos.cs:367`). In use
    the two are the same moment. On the aged replay they are not: a row reads "Waiting 8 days, since 2026-09-22" above a
    card reading "asked you on 2026-09-30".
  - The nudge counts whole days, `(int)(now − UpdatedOn).TotalDays`. So A1's "Waiting 1 day" for an item 18 hours old
    reads "0 days" by the nudge's count. Round 2 must word a wait under a day.
- **A5's Switch to Assessor carries `returnUrl=/activities/inbox`.** DESIGN.md § The NavMenu makes every switch
  `ActingRoleSwitch.Url(role)` with no return address, landing on Home. A second switch in the body, with a return
  address, is a rule change for no gain: "Open it" already takes her to the work. The way on then says "Back to
  Activity inbox" to someone acting as Committee member, a page outside her menu. It should follow the owner table:
  the acting role's list, else Home.
- **The fidelity page keeps to the "may" list,** with five corrections:
  - **The picker's round trips.** The Spec says "no round trip per rung". A choice still has to reach the server
    (`ActivityForm.razor:214`, `@onchange` → `UpdateValue`). That is one round trip per press, which is explicit and
    fine; only the fill can be CSS alone.
  - **The legend's length.** The College's descriptors are seeded, and each runs to about 200 characters
    (`paediatric-epa-v11.1.json:26`). Six of them open at 1280 is a long block above Feedback, and round 2 must draw the
    real ones.
  - **The move labels need no map and no DSL label.** `WorkflowTransition.LabelFor` title-cases the key; sentence-casing
    it there fixes the bar, the history and the reasons for every type, the registrar's view included. A DSL `label`
    would need a `Serialize` half and `SeedRoundTripTests` (CLAUDE.md, "Editing a seed folder").
  - **The request fold's "gist"** ("Emergency unit · Moderate · …") picks fields from a runtime schema, and nothing says
    which ones. Keep the section's title, "Filled in by …" and Show.
  - **The fold applies to more than it should.** The component moves About under the bar at 390 in every state (P7's
    completed page included), where the Spec limits it to a reader with a section to fill.
- **The way on inside `role="status"`** makes the live region read the whole next row: its name, "from …", two badges
  and the wait. Keep the live region to "Completed. 1 more waits for you." and put the row and its buttons straight
  after it. The focus still lands on the result, and Tab reaches Open the next.
- **What the canvas got wrong about the cast:**
  - **The committee menu.** A5, B3 and C6 draw it as "Decisions due, Committee reviews". A Committee member is offered
    Committee reviews and Decision panels (`NavItems.cs:151`, DESIGN.md § The NavMenu), never Decisions due (T131).
  - **Dr Patel's decisions are not his.** By the runbook he completed Dr Mahlangu's DOPS (R11) and Dr Molefe's DOPS on
    PAED-001 (R14, rung 4), and nothing else. He never declined or signed off anything: Dr du Plessis's review is still
    waiting at A.6.8. Dr Dlamini's CBD is Dr Zulu's (R18), and Dr Mahlangu's PAED-004 Mini-CEX is Dr Zulu's (R21).
    Decisions dated June and July come before every record, because Act 3 files the first.
  - **Dr Zulu's stalled request** is dated `D−3` (2026-09-27) and aged to 8 days, not 2026-09-16 and 9 days.
  - **Smaller slips.** Dr Ndlovu's R2 is dated 2026-09-10, where P3 has 09-09. The reflection's option reads "A critical
    incident". Placeholders stand where the runbook and the seed have the words: Dr Dlamini's presenting problem (Step
    3.3), the descriptors, and the feedback labels "What was done well", "Areas for development", "Agreed plan"
    (`mini_cex_cpsa/schema.json:91–103`).
  - Filler is fine for the 12-decision volume state. Every state a step names must carry that step's rows.

## Question 7: show the rater the registrar's year minimum?

Step 3.5 notes that year 3's minimum on PAED-001 is rung 4, and the page does not show it to Dr Naidoo. The options:

- **(a)** Never show it to the rater. This is today's behaviour.
- **(b)** Show it before the rating, in About or beside the picker: "Year 3's minimum on PAED-001: 4".
- **(c)** Show it only after Complete, in the status card: "Rated 4, at year 3's minimum".

**Recommendation: (a).** The field's own help says the rung is "the supervision this activity actually required on this
occasion - not a judgement of the trainee's worth". A minimum shown before the choice invites rating to the standard,
the anchoring the College's scale is built to avoid. Shown after, it teaches the rater the minimum for next time and
does the same thing one encounter later. The rater needs nothing from it to rate. The minimum belongs to the registrar's
progress and the committee's review, where it already is. For the same reason, on the assessor's side T346's semester
count ("1 of 3 this semester") should not appear on the completed card either.

## Recommendation

**A.** It makes T297 structural: one read, one component and one order on Home and in the inbox. It keeps the menu and
the shell flow 01 settled, and the words flow 03 gave the registrar. It answers question 1 without a new nav item. It
tells a two-role consultant on the Home she opens to, with one press to the work, without switching. **No borrowing is
needed.** The canvas offers B's two-role card, but that is the merged view W-010 rules out. A5's line, naming the
oldest, is enough, because the way on carries her through the rest. Take A with these corrections: the line's switch
goes, the way on's "Back to" follows the owner table, the waiting clock is `UpdatedOn`, and the live region is kept
short.

**Watch in round 2:**

- **The cast.** Every step's state carries the runbook's rows (the cast notes above). The real descriptors and feedback
  labels.
- **States not yet drawn.** A5 at 390, with the right committee menu. The inbox empty while decisions exist ("Inbox
  clear" above Decided by you), and the registrar-only empty state kept. The awaiting review page (Dr Patel's Sign off
  and Return). Declined, returned and discussed from the assessor's side. Completed on a paused EPA. "Completing…".
  Loading and error on Home, the inbox and the page.
- **The flagged edge cases.** 25 rows, every row overdue, and two requests with one name.
- **Wording.** A wait under a day. The count as words with no "0" badge.
- **Scope.** The committee Home's line is a slot in flow 07's page, so agree its place with flow 07.

## Decisions (put to the operator)

| # | Question | Recommendation |
|---|---|---|
| Pick | Which structure? | A, with the corrections above; no borrowing |
| Q1 | Decisions beyond ten | Decided by you, a second section of the Activity inbox: newest first, 20 a page, all of them; Activity, Decision, Decided (SAST), Credit. Home's card keeps five, each dated, with "All your decisions". A decision stays "moved last by you, and finished or with no move left" (T297) |
| Q2 | Is the inbox the assessor's Home? | Keep both. Home's "Waiting for you" is the inbox's first five rows from the same read, in the same order and words (`WaitingList`), with "Open Activity inbox" at its foot. Home adds the decisions |
| Q3 | A two-role consultant | One line on the other view's Home, only when work waits there. It names how many, whether any is overdue, and the oldest, with "Open it" or "Open the oldest", which opens the activity without switching. The line has no switch: the sidebar's stays the only one. The committee side's words are flow 07's |
| Q4 | Overdue | A badge beside the state, never in its place; oldest first; and the count as words ("3 waiting, 1 overdue") on Home and in the inbox. The rule line says "more than 7 days". One clock, `UpdatedOn`, as Overdue, the stall and the nudge read it. No Overdue group |
| Q5 | The rater's rung picker | Radios in the rung row, the College's labels, six abreast at every width. The chosen rung's descriptor goes under the row. "What each rung means" is open at 1280 until a rung is chosen and folded at 390, with the seeded descriptors |
| Q6 | The way on | In the result: "1 more waits for you", the next row, Open the next (named for it), and Back to the acting role's list, else Home. With nothing left: "Nothing else waits for you." and Go to Home. No automatic advance. The live region holds the sentence only |
| Q7 | Show the rater the year minimum? | No: not before the rating, not after. T346's semester count is left off the assessor's completed card too |
| E1 | Moves' labels | Sentence case from `WorkflowTransition.LabelFor`, for every type and both views: bar, history, reasons. No DSL label |
| E2 | The note panels | One pattern: "Decline this request" or "Return this reflection" / "… review"; "Note for Sipho Ndlovu" with the red `*`; danger only for a move that ends the activity; "Keep the request" or "Keep the reflection". Steps 3.11 and 3.15 re-quoted |
| E3 | The rater's page on a phone | Only while the reader has a section to fill: filled sections fold (title, "Filled in by …", Show, no gist), and About goes under the bar |
| E4 | Small fixes | Activity unavailable's link goes to the acting role's list, else Home. The Request's user fields read the name, not `NomineeDirectory`'s "name (email)" option label. Discard changes says why it is greyed |
| E5 | The waiting read and the replay | One read (`ListWaitingForYouQuery`) for Home and the inbox, oldest first, leaving out the caller's own subject rows in both. Step 3.30's SQL also ages the Submit row, so the page's "asked you on" agrees with the row's "since" |
| R1 | DESIGN.md § Dashboard page | The Assessor's cards become "Waiting for you" (first five, count in words, no "0" badge) and "Recent decisions" (five, dated, linked). A Home may carry one line about another held role's waiting work |
| R2 | DESIGN.md § List page | A list page may hold two headed, counted sections, the second paged. The inbox stacks at 390, and Waiting replaces Updated. `ActivityLink`'s second line gains "from <registrar>" in the assessor's lists |
| R3 | DESIGN.md § Record page with a workflow | The result may carry the way on. On a phone, a reader with a section to fill gets the fold and About under the bar. Activity unavailable's link follows the acting role |
| R4 | DESIGN.md § Form system, "The activity form" | A writer of a scale chooses on the rung row, not a select (`ActivityForm.razor:210`). Moves are in sentence case. One note-panel pattern |

## The operator's answer (2026-09-30)

**Accept all**: A, "Two pages, one list", with its four corrections (no switch in the other-role line; the way on's
"Back to" follows the owner table; the waiting clock is `UpdatedOn`; the live region holds the sentence only) and no
borrowing; Q1–Q7 as recommended (Q7: the registrar's year minimum is not shown to the rater, before or after rating);
E1–E5; and R1–R4. Round 2's ask is `round-2-ask.txt`, which also carries the review's "check before round 2" fixes: the
committee menu, one waiting clock and its words under a day, the cast's real rows, the seeded rung descriptors and
feedback labels, the phone fold only while the reader has a section to fill, and two claims dropped (no round trip per
rung; other roles' pages linking to the inbox).
