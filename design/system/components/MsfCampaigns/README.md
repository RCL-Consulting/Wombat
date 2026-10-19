# MsfCampaigns

MSF campaigns, the campaign list (`/msf/campaigns`, `Pages/MultiSourceFeedback/CampaignsList.razor`; its words in `MsfCampaignText.cs`, its addresses in `Shared/MsfLinks.cs`): **every campaign the coordinator runs**, newest first, counted and never listed by person, each row linking to what its page lets the coordinator do in its state. Flow 10 (T369, 2026-10-19, 68327c9d; boards R3-L1 to L9; round 3 L1–L9, M1, M10; T206, T225, T270, T280, T323, T329; build review D2, D11) rebuilt it. Until then New campaign came after MSF coverage, the list was a `DataTable` stacked only below 641px (its seven columns split names from 641 to 900px), a withdraw's result was plain, and a failed read printed the exception's text.

## Who reads it

Coordinator and Administrator (`[Authorize(Roles = "Coordinator,Administrator")]`), each the campaigns in their scope (`ListMsfCampaignsForCoordinatorQuery`). The Coordinator's menu offers it as "MSF campaigns", which owns every campaign page: New MSF campaign, a campaign's page, its report and MSF coverage (NavMenu). Someone who holds Trainee, whatever role brought them, runs no campaign (`MsfCampaignRules.RunsNoCampaigns`, T224): the page opens with a standing warning (`#msf-runs-no-campaigns`, `Role=""`) in the words a create refusal gives, and offers no New campaign, no list and no empty card. A list leaves out every campaign about the caller (T269).

## The page

- **The header** (`.msf-header`): "MSF campaigns", the subtitle "Create and review multi-source feedback campaigns.", and its actions **New campaign** (primary, `plus`, `/msf/campaigns/new`) **then MSF coverage** (outline, `#msf-coverage-link`, only for a caller it shows a programme to) (L1). Below 641px they stack, each 44px and the page's width.
- **The column** (`.msf-page`): its sections 24px apart; the page's one `ConfirmDialog` is rendered inside it, so its phone rules reach it (M10).
- **The result** (`ActionResult`, `#msf-campaigns-result`, focused once the dialog has closed): "**The campaign for Lerato Molefe (Default MSF, closing 2026-11-02) has been withdrawn.** Its respondents' links no longer work, and their email addresses have been removed." (its first sentence bold, as on the campaign page; L4, build review D11), or for one closed, "… Its report will never be released to the trainee."; refused, a danger alert "**Not withdrawn.** …".
- **The list** (`section.list-section`): `h2.list-section-title` "Campaigns (2)", a plain count, over a hand-written `.clinic-table.clinic-table--stack.clinic-table--stack-wide` in a `.table-container.shadow` (D2; DataTable), captioned "MSF campaigns, newest first. Counted by respondent group: this list never shows who was invited or who responded." **Newest first by creation** (M1):

| Column | What |
|---|---|
| Trainee | **the row's header**, `th.trainee-cell`: the name alone, "Lerato Molefe" |
| Template | "Default MSF", and under it its kind, `span.kind-line` "Multi-source feedback" |
| State | the badge (`BadgeFor.MsfCampaign`): Draft grey, Open blue, Closed and Under review amber, Released green, Withdrawn red; "Under review", never the enum's "UnderReview" |
| Invitations | the count, "9" |
| Responses | the count, or **"—" until the campaign has opened** (`HasOpened`, T270): a draft, and a draft withdrawn, have no responses |
| Closes (`.nowrap`) | "2026-11-02" |
| (Actions) | `div.actions-cell` of small outline buttons: the campaign page's link, the report's link, Withdraw… |

- **The row's links are named by what their page lets the coordinator do** in the row's state (T225; `CampaignLinkLabel`, `ReportLinkLabel`): Draft "Manage" and no report link; Open "Manage" and "View report"; Closed or Under review "View campaign" and "Review and release"; Released "View campaign" and "View report"; Withdrawn "View campaign" alone. **Withdraw…** on every campaign not yet released (`MsfCampaignRules.IsWithdrawable`, T206). Each is named for its row, its visible label first: "Manage: the campaign for Lerato Molefe (Default MSF, closing 2026-11-02)", "Withdraw: the campaign for …"; where two rows would read the same, `RowNames.Distinct` adds the state (T280).

## Withdraw (ConfirmDialog, DangerAction)

"Withdraw this campaign?" · "Withdraw the campaign for Lerato Molefe (Default MSF, closing 2026-11-02)? It will take no more responses: any link already sent stops working, and every respondent's email address is removed. A withdrawn campaign is never released to the trainee, and withdrawing cannot be undone." (for one closed and under review: "It has closed, and its report has not been released. A withdrawn campaign is never released to the trainee and is never evidence on their record, and withdrawing cannot be undone.") · **Keep the campaign** · Withdraw campaign (L3). The command is sent the state the dialog was worded by and refuses a withdraw confirmed for an open campaign once it has closed (T199). Withdraw… is never natively disabled (T234): a second press while one runs sends nothing.

## States (L8, L9, T329)

- **Loading**: the header at once, "Campaigns" over four row skeletons (3rem), said once, "Loading MSF campaigns.".
- **Load failed**: "Could not load MSF campaigns. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel), never the exception's text, which goes to the log.
- **None**: the empty card, "No MSF campaigns", "Create the first MSF campaign to start respondent outreach.".

## Narrow widths

**Stacked at 900px and below** (both stack classes): each row a block, the trainee heading its labelled cells; a row's buttons are 44px from 900px (`.msf-page .clinic-table--stack-wide td .actions-cell .btn`) and, below 641px, a full-width column (A.7.5). No sideways scroll at 1280, 768 or 390.

## Contrast

No new colour token. Body text 12.63:1 and the kind line (`muted-text`) 5.09:1 on the surface; the row header 11.36:1 on `header-bg`; the buttons 4.86:1; the badges as Badge's.

## Anonymity

The list counts invitations and responses for the whole campaign, never by person and never by group: who was invited or who responded is on no list (the caption says so).
