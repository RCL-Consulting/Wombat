# MyAuthorisations

My authorisations (`/portfolio/authorisations`, `Pages/Portfolio/MyAuthorisations.razor`; its words in `Shared/StarWords.cs`): **the trainee's STARs** (Statements of Awarded Responsibility), the ones in force as cards and the ones no longer in force in a table, each with its certificate. Flow 09 (T366, 2026-10-14, 49e7cf5e, with T367, e9b29081; boards R3-5; round 3 F2, F4, M7, S1, S2, S3, S9, S13; C18, C19; T272, T325) rebuilt it. Until then it was a `.details-grid` (1fr 2fr) of the active STARs only, its dates long-form ("3 November 2026") and "Expires in N days" counted on the UTC day; a revoked or expired STAR vanished from the page (T367), and a download's failure printed above the page.

## Who reads it

The Trainee and, since flow 13 (T379, Q3; R3-R08), a former trainee: the policy is My progress's, `TraineeOrFormerTrainee` (until then `[Authorize(Roles = "Trainee")]`, which refused a graduate her own certificates, T311). Each reads her own STARs (`GetDecisionsForTraineeQuery`: Active, Revoked and Expired, never Superseded). A graduate reaches it from her Home's record card (FormerTraineeHome), My progress's "Your record:" line (MyProgress) and the EPA page's "Open My authorisations". Its owner is My progress, a personal link (`UnderAPersonalLink`), so for a former trainee who holds no role too: the trail is Home › My progress › My authorisations (NavMenu, Breadcrumbs). Home's My authorisations card (DashboardCard) links here.

## The page

`div.my-authorisations` (the wrapper every phone rule here is scoped to). The header: "My authorisations", subtitle "The Statements of Awarded Responsibility (STAR) the committee has issued you, and their certificates."

- **In force** (`section.stars-section`, `h2.list-section-title#stars-in-force`): **one card a STAR, by EPA code, in an equal-column grid** (`.star-grid`: `auto-fill` tracks of `min(20rem, 100%)`, 24px apart; one column at 390, as many as fit at 768 and 1280, S9; a card may shrink below its longest word, so PAED-015's long title wraps in it). Each card is `article.detail-card.star-card` (a flex column 8px apart):
  - `h3` (1.05rem): the EPA by name, "PAED-010 — Leading and operating within a clinical team" (EpaLabel, the paused mark after a deactivated EPA, T255);
  - a `dl.detail-list`: **Authorised level** ("4"), **Issued** ("2026-10-14"), **Expires**: the date, and from 30 days before it, beside it an amber badge on the text's line, "Expires in 20 days" ("Expires in 1 day", "Expires today"), counted on the South African calendar, a STAR being valid through its date (S3, T325); or "No expiry" (C18);
  - the rationale (`p.star-rationale`, muted);
  - at the card's foot whatever the rationale's length (`.star-card .form-actions`, `margin-top: auto`): **Download certificate** (`btn-primary`, `download`), named "Download certificate for PAED-010" (F4: its visible words inside its name), its result under it.
  - None in force, while some STAR is no longer in force: the heading stays, over "No STAR is in force now. When a committee review ratifies a decision authorising you to perform an EPA, it will appear here." (`p.muted`, S2).
- **No longer in force** (`h2#stars-ended`), only when there is one (C18): a hand-written `.clinic-table.clinic-table--stack.clinic-table--stack-wide`, captioned "STARs that are no longer in force: revoked, or past their expiry":

| Column | What |
|---|---|
| EPA | the row's header, the EPA by name (EpaLabel) |
| Level | "3b" |
| Status | the badge (`BadgeFor.EntrustmentDecision`): Revoked red, Expired amber |
| Since (`.col-fit`) | the day it stopped being in force: a revocation's South African day, or the day after an expiry date |
| Why | in the reviser's name and words (Q8, T367): "Revoked by Refilwe Mokoena: Staged on evidence about PAED-001, not PAED-002; to be decided again."; "Its expiry date, 2026-11-02, passed." |
| (Actions) | **Download** (`btn-outline btn-sm`, `download`), named "Download certificate for PAED-002 (revoked)" ("… (expired)"), its result under it |

## The download (C19, T234, T272)

- While a certificate is made, its button says "Generating…", `aria-disabled`, named "Download certificate for PAED-010, generating", and keeps the focus; every other Download is natively disabled.
- **Done**: the browser's download says what it did; the focus stays on the button, and the page shows no result.
- **Refused or failed**: its result sits under its own button (`div.download-result`, 0.9rem; 8px under an actions cell) in an `ActionResult` that takes the focus: "The certificate could not be found." or "The certificate could not be made just now." with **Try again** (`refresh-cw`), which moves the focus back to its Download (build review D1). Never an exception's text.

## States (C25, T329)

- **Loading**: the header at once, an always-present visually hidden status "Loading your authorisations.", two card skeletons (14rem), no Download.
- **Load failed**: "Could not load your authorisations. Nothing has changed. Try again, or come back in a few minutes." with Try again, the failure logged.
- **No STAR at all**: the empty card, "No active authorisations yet", "When a committee review ratifies a decision authorising you to perform an EPA, it will appear here. You can then download a STAR certificate for your records."
- **No STAR at all, for a reader who holds no Trainee role** (flow 13, T379, P2; Spec 3's Empty row): the empty card's title is "No STAR was issued to you, so there is no certificate to download." (`FormerTraineeRecordWords.NoStarIssued`, the words of her Home's plain STARs row) and nothing under it: the title says it all, so StatePanel draws no copy block. R3-R09 drew the built words; the build follows the Spec. Measured on the replay (Pieter du Plessis, 1280 and 390).

## Narrow widths

- **One card a row at 390**, as many as fit at 768 and 1280; **No longer in force stacks at 900px and below** (both stack classes; DataTable), the EPA heading its labelled cells.
- Below 641px every button on the page is 44px, a failed download's Try again among them (D7), and the cards' Download is the card's full width.
- No sideways scroll at 1280, 768 or 390 (the replay's 4.39, 4.42).

## Contrast

No new colour token. Body text 12.63:1 and the rationale (`muted-text`) 5.09:1 on the surface; the row header 11.36:1 on `header-bg`; Download 4.86:1; "Expires in 20 days" as Badge's amber.

## Known gaps

- **Found by the replay, not filed:** the certificate's evidence summary prints its times in UTC.
- **Kept by the build review** (D8): while one certificate is made, the other Downloads are natively disabled (the busy-state convention of flow 07 and T234).
