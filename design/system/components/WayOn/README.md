# WayOn

The way on (`Components/Shared/Activities/WayOn.razor`): after an assessor's move on the activity page, the next activity waiting for them, with "Open the next" and the way back to their list. It sits straight after the move's result, outside its live region. Flow 04 (T350, 2026-09-30, 06aa51d7; round 1, Q6; round 2, C1, C10 d; R3) made it; until then a move's result was the end of the page's story, and the assessor went back to the inbox by the sidebar.

## What the consumer provides

`<WayOn Next="@_next" BackLabel="@($"Back to {ownerLabel}")" BackHref="@ownerHref" />`

- `Next`: the first row still waiting for the reader once the activity just moved is left out (`ListWaitingForYouQuery`, read again after the move); null when nothing waits.
- `BackLabel` and `BackHref`: the acting role's list the activity page sits under, by the owner table (`NavOwners.OwnerFor(typeof(ActivityView), role)`): "Back to Activity inbox" for an Assessor; "Back to Home" (`/`) where the role has no owner.

The page draws it only after a move the reader made as someone other than the activity's author. A move made as the author keeps flow 03's result ("It is in Fatima Khumalo's Activity inbox."). A read that fails is logged and costs only the count and the way on, never the result.

## The result above it

The page's ActionResult (`#activity-result`, which takes the focus) holds only the move's sentence in bold and what is left (`WaitingWords.MoreForYou`): "**Completed.** 1 more waits for you.", "**Discussed.** Nothing else waits for you.", "**Returned to Sipho Ndlovu.** Nothing else waits for you." (a Return names the act and whose it is now, `WaitingWords.ReturnedTo`; the status card below still says Draft). A move into a final state reads as that state.

## Markup

- Something waits: `section.way-on` (`aria-label="The next activity waiting for you"`, a column 16px apart, 24px above the status card) holding the next row (`WaitingList`, `WithSince`, its link out of the tab order), "Waiting less than a day, since 2026-10-03 17:22 SAST.", then a `.form-actions` row: **Open the next** (`a.btn.btn-primary`, `aria-label="Open the next: Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-10-02, from Pieter du Plessis"`) and **Back to Activity inbox** (`a.btn.btn-outline`).
- Nothing waits: `p.way-on-none` holding one `a.btn.btn-outline` "Go to Home".

No automatic advance (Q6): the assessor chooses. The row's link is out of the tab order, so from the focused result the next Tab is Open the next (C10 d); a pointer and a screen reader's browse mode still have it.

**Below 641px** the buttons stack, each 44px and the column's width, Open the next on top.

## Rules (DESIGN.md § Page-level patterns, "Record page with a workflow", R3)

- The live region says the result and the count only; the row and its buttons are not announced.
- "Back to" names the list by the owner table, never a remembered referrer.

## Contrast

As WaitingList's row; the buttons as Button's: `on-fill` on `secondary-color` 4.86:1, an outline button's words and edge 4.86:1 on its surface fill.

## Known gaps

- An assessor who created the request on a registrar's behalf, and rates it through the `field:` arm, is its author to the page, so gets flow 03's inbox sentence and no way on (T351).
- The way on reads the whole waiting list for its first row and a count, once per move (T351).
