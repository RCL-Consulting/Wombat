# Alert

A message block in one of four kinds (`success`, `info`, `warning`, `danger`): body text on the kind's tint, the kind's colour on a 4px left edge and on an icon the stylesheet draws, and an ARIA role that follows the kind unless the caller names one; with `ActionResult` and `RefusalText`, it is how every page reports what an action did.

## What the consumer provides

`<Alert Kind="danger" Id="login-error" Role="…" Dismissible="true">…</Alert>`

- `Kind`: `success`, `info` (the default), `warning` or `danger`. It renders `div.alert.alert-{Kind}`. **Info is for a notice**: something the person should know that refused nothing (flow 02: "Your session has ended. Sign in again.", "You have signed out.", change password's "This account signs in through your institution, so it has no password to change here."). Danger is for a refusal only.
- The content: the message in the product's words. A caught refusal goes through `RefusalText.Of(exception)`, never `exception.Message`, so a validator's refusal reads as its sentences, not as "Validation failed: -- Members: … Severity: Error".
- `Role`, only where the kind's default is wrong. Defaults: `danger` is `role="alert"` (read at once), `warning` and `success` are `role="status"`, `info` has none. A refusal shown as a warning takes `Role="alert"`; a notice that arrives with the page takes `Kind="info" Role="status"` (the sign-in page's notices, change password's institution-only notice); a warning or success that is standing page content, there on every visit, takes `Role=""`, which renders none.
- `Id`, when a field must name the alert with `aria-describedby`: an alert already on the page when it loads is not reliably announced, so the field that takes the focus names it (sign-in, and every field a refusal names).
- **Any other attribute** lands on the alert's own element (`CaptureUnmatchedValues`, T339): `tabindex="-1"` and `autofocus` for a message that takes the focus as a static page loads (the sign-in page's refusal, and its notice beside the institutions' buttons; the link page's refusal; change password's refusal). SignInCard has the focus rule.
- `Dismissible`: puts the content in a `div` beside a `.btn .btn-outline .btn-xs` × inside an `.actions-cell`.
- **An action of its own:** lay the words and the action out in `div.alert-row`: the words in `span.alert-row-text` (flex `1 1 16rem`), the button or link after them, wrapping under the words where they need the width. Home's load error ("**Could not load your Home.** Nothing has changed. Try again, or come back in a few minutes." with Try again) and the acting-role switch's result (with Switch back).

## Look (DESIGN.md § Alerts, validation, empty states)

- `.alert`: `text-color` words, a 1px edge in the kind's colour thickened to 4px down the left, `radius-md`, padding 0.75rem 1rem 0.75rem 3rem, `space-lg` below.
- **Inside a card or a dialog that spaces its blocks with a gap, the alert has no margin of its own**, so a result keeps the container's rhythm: the auth card (its children's margins are zeroed), `.change-password .alert`, `.my-account-card .alert` and `.dialog-form .alert`, all 16px between blocks; and since flow 03 Log an activity's `.form-column > .alert` (24px between blocks; its paragraphs 4px apart). A new container with a gap does the same.
- `.alert::before`: the kind's 20px Lucide icon, a mask filled with the kind's colour, 1rem in and on the first line: circle-check (success), info (info), triangle-alert (warning), circle-alert (danger). No page writes the icon.
- Tints and edges: success `success-bg`/`success-color`; info `info-bg`/`secondary-color`; warning `warning-bg`/`warning-color`; danger `danger-bg`/`danger-color`.
- In a Windows contrast theme the icon is filled with `CanvasText`.

## ActionResult

`<ActionResult @ref="_result">` wraps a page's result alerts in `div.action-result[tabindex=-1]`. The page calls `FocusAfterRender()` once the action has answered, done or refused, so the result is read and the focus never falls to the body; `FocusOnLoad` does the same for a result that arrives with a page load (it renders `autofocus`). The region shows the `focus-ring` outline when focused.

## Rules

- Never hand-write `<div class="alert …">`: it skips the role default.
- **A semantic colour is never an alert's words** (T322): the kind is told by the tint, the edge and the icon.
- An action that is done moves the focus to its result; a refused action leaves the focus where it was, with its refusal as a `danger` Alert.
- **A refusal that names fields is not an Alert** (flow 03, T342): it is a RefusalSummary, which takes the focus and links each line to its field. A refusal that names no field stays a `danger` Alert above the page's cards.
- **A result on a record page** (flow 03, the activity page): an ActionResult at the head of the page body, `id="activity-result"`, its success Alert `role="status"`, taking the focus after every move: the move's sentence in bold, then, plain, whose inbox it is in. "**Submitted. It is now Requested.** It is in David Naidoo's Activity inbox." A result handed over from another page (Log an activity's filing) arrives with the page, `FocusOnLoad`, and wins over the h1. A standing notice (File it again's "Copied from your request to Fatima Khumalo, which was declined. …") is `Kind="info"` with no role.
- A load error says that nothing changed and offers the read again; it draws nothing the read would have filled. Home's DashboardFrame, My account (StatePanel with `OnRetry`, T339) and flow 03's four activity pages do; every other StatePanel `LoadError` prints the page's own string with no Try again (StatePanel's Known gaps).
- A refusal that arrives by URL travels as a code and the page chooses the sentence, so a crafted link cannot put words on Wombat's page. No sentence says "Please" (flow 02, C5).

## Contrast

Words 11.23 to 11.89:1 on every tint. Edges and icons: success 5.23:1, warning 5.43:1, danger 5.56:1, info 4.55:1 on their tints. Until T335 each kind's colour was its words on its own tint (success 2.55, warning 2.42, danger 3.57:1).

## Known gaps

A dismissible alert's × wraps under content wider than the row (the `.actions-cell` wraps), as the committee review's sampling warnings do (seen in a static render of the stylesheet).
