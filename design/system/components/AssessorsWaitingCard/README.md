# AssessorsWaitingCard

Home's Waiting for assessors card (`Components/Shared/Programme/AssessorsWaitingCard.razor`): the page's own first five, so the card and the page its foot opens cannot disagree; on both speciality admins' Homes (spanning three) and the Coordinator's (spanning two). Flow 06 (T358, 2026-10-05, 85d5a508; Q3, E3, E6; R2-Home a1–a5, k1–k4; round 3 items 11, 27) made it; until then the admins' card was "Pending reviews", a count that linked nowhere, and the Coordinator's "Stalled requests", ten rows each its type and its trainee beside "d MMM".

## What the consumer provides

`<AssessorsWaitingCard Waiting="@Summary?.Waiting" IsLoading="IsLoading" Span="2" />`

- `Waiting`: Waiting for assessors' first page of five (`WaitingForAssessorsDto`); null when the role gives no scope, which reads as empty.
- `IsLoading`: the title over the frame's skeleton.
- `Span`: 3 on the admins' Home, 2 on the Coordinator's.

## The card (a DashboardCard)

- **Title** "Waiting for assessors" (`clock`, `#card-waiting-assessors`), a section named by it.
- **Badge** in flow 04's words, "3 waiting, 2 overdue" ("2 waiting"; `WaitingWords.StaffCount`, `badge-submitted`: work waiting on someone); none when nothing waits.
- **Stripe**: the emphasis one, or the warning one in its place when any row is overdue (`Warning`, never both).
- **The rule line**: "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5.", both numbers the settings'.
- **The rows**: WaitingList's staff reading (`WithNominee`): the link ("Portfolio and Logbook Review (Paediatrics) · PAED-015 · 2026-10-04" over "from Pieter du Plessis"), the state's badge with "Overdue" beside it, "With Mohammed Patel" on its own line, then "Waiting 8 days" (E6). **No reminder from Home**: that is the page's. Past five, "9 more wait in Waiting for assessors." ("1 more waits …").
- **The foot**: "Open Waiting for assessors" (`btn-sm btn-outline`, `/programme/waiting`), empty included, since the page is in the menu (item 27).
- **Empty**: `p.card-empty` "Nothing is waiting for an assessor.", then the foot.

## The Coordinator's Home (DESIGN.md § Dashboard page, "The oversight Homes")

- **The header action** "Start an MSF campaign" (`plus`, a primary button, `/msf/campaigns/new`; `HomeFrame.ActionFor`, Q6, review 26), there from the first render, before the cards' read returns; below 641px its own 44px row under the rule (`.home-action`). It replaced the "Quick action" card.
- **Waiting for assessors** (spanning two), in place of "Stalled requests".
- **Nothing filed in 30 days** (`clipboard-list`, `#card-nothing-filed`): its badge "5 registrars" in the draft tone (`BadgeTone`, D10); its rule line always, since the 30 days' rule is the card's whole meaning: "Current registrars with nothing filed (a draft is not filed) in the last 30 days. A registrar admitted less than 30 days ago is not listed." (E5); the rows RegistrarRoster's `FiledMeta` reading; the foot "Open in Programme trainees" (`/programme/trainees?filed=true`), empty included; empty, "Every current registrar has filed something in the last 30 days."
- **Invitations nearing expiry** (`calendar`, `#card-invitations`): each a `.list-row`, the address with " · Trainee" (the role by its label, muted) beside "expires 2026-10-07" (ISO, T325); its rule line with rows only, "Expiring in the next 3 days. Only an institutional admin can resend an invitation."; no row action and no foot; spanning the row once it has a row (`Span` 3, read `Summary?.…`, the eager-read guard); empty, "No invitations expiring soon."

## Rules

- **One read, one order, one set of words** with the page (`ListWaitingForAssessorsQuery`'s first five).
- Every row is its own link to the activity behind it; the card is never one link around them (T280).
- A card's parameters read while the summary is null are written `Summary?.…`: the stripe, the badge, the span (`HomeFrameTests`).

## Contrast

As WaitingList's; the warning stripe `warning-color` 5.77:1 on the surface; the badge's words 11.84:1 on `info-bg`, its edge 4.55:1.
