# MsfRespond

The MSF questionnaire (`/msf/respond?token=…`, `Pages/MultiSourceFeedback/MsfRespond.razor`; its states' titles in `MsfResponseRefusals`): **the page an invitation link opens for a respondent who has no account and never signs in**, in the sign-in layout's wide card (AuthLayout). Flow 10 (T369, 2026-10-19, 68327c9d; boards R3-Q1 to Q9; round 3 S9, N18; T202, T205, T280; build review D4) designed it. Until then it was the pre-restructure `--wide` card: an `<h2>` state at 2rem, a 56px mark, "Who sees your feedback" after the questions, every dead link one shape of alert, and a `maxlength="4000"` that cut a paste silently.

## Who reads it

Anyone holding a link (`[AllowAnonymous]`). Static server-rendered HTML with plain form posts, not a circuit (`[ExcludeFromInteractiveRouting]`), rate-limited per link, no antiforgery token (the link's token is the post's whole authority). It reads no script but `wombat.js`'s submit-once.

## The card

`div.account-form-container.account-form-container--wide.respond-card` (44rem; a flex column, its blocks 16px apart):

1. **The lockup** (`div.account-brand`): the mark at **44px** (`.account-brand-mark`, 2.75rem; round 2 N18) beside "Wombat" in `wordmark-large`.
2. **The card's one `<h1>` in every state**, with no `tabindex` (nothing moves the focus to it; T280): "Feedback on Lerato Molefe" ("Feedback on the teaching of Lerato Molefe" for learner feedback), or the state's title. The tab is the same words; "Error: " before them after a refused post.
3. **The questionnaire's state**: `p.respond-meta` "Default MSF · Last day to respond: 2026-11-02" (the South African day, T325); "You have been asked to give multi-source feedback on Lerato Molefe, a trainee you have worked with." ("… feedback on the teaching of Lerato Molefe, a trainee who has taught you.").
4. **Who sees your feedback** (Q1), before the first question, set apart on the info tint with a `secondary-color` edge (`div.respond-privacy`, `role="group"`, named by its title): `ul.respond-privacy-list`, `eye-off` "Your name and email address are never shown to Lerato Molefe." and `users` "Lerato Molefe sees the feedback only after the request has closed and a coordinator has reviewed and released it, grouped by respondent role." (learner feedback: "… together with the other learners' feedback."). What the invitation email promises, and no more (T202): no count, no other response, no threshold.
5. **The form** (`form.respond-form`, ruled off by a hairline, its questions 24px apart, `data-submit-once`; always in the tree, `hidden` when it holds nothing, so a post to a link that has just died is answered on this page): "Questions marked * must be answered." (`p.respond-required`, muted); **a rating** is one `fieldset.form-group` whose legend is the question with the required mark and a hidden "required", its help as a `p.page-subtitle` ("For each rating, compare Lerato Molefe with what you would expect of a trainee at their stage of training."), then `.scale-choices`: five `.form-check` rows, each 44px with a border, Well below expectations to Well above expectations, **the chosen one tinted `info-bg` with a `secondary-color` edge by CSS alone** (`:has(:checked)`; a 2px `CanvasText` frame in forced colours); **a comment** is a `label.respond-label` (1.1rem, 600) with "(optional)" (`span.respond-optional`, muted) or the mark, its help "Up to 4,000 characters. Your comments may be shown to Lerato Molefe word for word, so leave out anything that would identify you.", and a `textarea` of 5 rows **with no `maxlength`** (D4: the handler refuses an over-long one in words); then **Submit feedback** (primary).
6. **A refused post**: `div.error-summary` (`tabindex="-1"`, `autofocus`, since a full page load starts a screen reader at the top) holding a danger alert, "**Your feedback was not recorded.** …", named by the question at fault (`aria-describedby`), the answers kept.

## The states (S9: an Alert per state, under the h1)

| State | h1 | Alert |
|---|---|---|
| Recorded | "Thank you" | success, "Your feedback on Lerato Molefe has been recorded.", then "The link in your invitation has now been used, so it will not open the questionnaire again. You can close this page." |
| Link used | "Feedback link already used" | **info** (the respondent's answer was recorded) |
| Request closed | "Feedback request closed" | warning |
| Link expired, revoked, not recognised | "Feedback link expired", "… revoked", "… not recognised" | warning |
| A fault | "Something went wrong" | danger, then "You can close this page." |

Each dead link's alert is `role="alert"`, its words the refusal's own, then "You can close this page." The page's HTTP status is the Api's for the same refusal (404, 410, 400).

## Narrow widths

The card pads 24px below 641px; every rating row is 44px at every width; Submit feedback is the card's width below 641px. Nothing scrolls sideways at 390.

## Contrast

No new colour token. The privacy panel's words 11.84:1 on `info-bg`, its icons and edge `secondary-color` 4.55:1 on it; a chosen row's edge the same; muted lines 5.09:1 on the surface; a row's border `border-color` (decoration, the radio itself the mark).

## Anonymity

The page shows the respondent their own questionnaire and nothing else. Who sent a response is not recorded, even when they are signed in to Wombat in the same browser; the respondent's address is never on the page.

## Known gaps

- **Found re-syncing, fixed in step 8:** the rating question's legend rendered at body size and weight, `fieldset.form-group > legend` (0,1,2) outweighing `.respond-form legend` (0,1,1); the rule is now `.respond-form fieldset > legend`, as specific and later, so both questions read at 1.1rem/600.
- **Found re-syncing, fixed in step 8:** two sentences said "Please": they now read "Try again in a few minutes." and "Answer the questions below before submitting.". The rating's help says "their stage of training": singular they, kept.
