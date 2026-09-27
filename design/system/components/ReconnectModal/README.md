# ReconnectModal

The reconnect dialog: a native modal `<dialog>` the Blazor runtime drives when a page's connection to the server drops, showing one of six states at a time (Reconnecting, Could not reconnect, Connection lost, Page paused, Could not resume, Reloading), which cannot be dismissed.

Flow 01 (T335, b347e11c; T330) rewrote it from the .NET template's. Source: `Components/Layout/ReconnectModal.razor`, `.razor.css` and `.razor.js`; `App.razor` renders it on every page.

## What the consumer provides

Nothing: no page places it. The runtime finds `dialog#components-reconnect-modal`, puts a `components-reconnect-*` class on it for the state and raises `components-reconnect-state-changed`; the script opens and closes it, keeps its role, name, description and live region in step, and runs its buttons.

## States (DESIGN.md § The reconnect dialog)

Each is one `div.reconnect-state` element carrying a `-visible` class, shown by one rule when the dialog carries the matching class.

| State | Dialog class | Heading (h2.reconnect-title) and 20px icon | Sentence | Button |
|---|---|---|---|---|
| Rejoining | `components-reconnect-show` | Reconnecting, loader-circle turning, `secondary-color` | "The connection to Wombat dropped. Reconnecting now.", and the bar | none |
| Retrying | `-show -retrying` | Could not reconnect, clock, `warning-color` | `data-attempt="waiting"`: "Trying again in 5 seconds." (the count written a second at a time); `"started"`: "Trying again now.", and the bar | none |
| Failed | `components-reconnect-failed` | Connection lost, circle-alert, `danger-color` | "Wombat cannot be reached. Try again when your connection is back. If the page cannot be restored, it reloads, and anything not yet saved on it is lost." | Try again (refresh-cw) |
| Paused | `components-reconnect-paused` | Page paused, pause, `secondary-color` | "This page is paused. Resume to carry on." | Resume |
| Resume failed | `components-reconnect-resume-failed` (set by the script) | Could not resume, circle-alert | Failed's sentence | Try again (refresh-cw) |
| Rejected | `components-reconnect-rejected` | Reloading, loader-circle turning | "Reloading the page…" | none |

- **Could not reconnect is one state, its line replaced.** `data-attempt` on the dialog picks the line (`data-reconnect-line`), so the heading, and the focus on it, stay put from the first retry to the last.
- **Focus, name and role.** The shown state's heading (`tabindex="-1"`, no ring) takes the focus; the dialog is named by that heading and described by its sentence; its role is `alertdialog` for Connection lost and Could not resume. One polite live region inside the dialog says a new state's sentence once, never the count.
- **The buttons** are `.btn .btn-primary`, found by `data-reconnect-action` (`retry` or `resume`); while an attempt runs the button carries `aria-disabled="true"`. Try again reconnects, else resumes, else reloads; Resume resumes, else reloads.
- **It cannot be dismissed:** Esc is refused.

## Look

- 400px wide (`25rem`), never closer than `space-md` to the screen's sides (358px at 390px); `surface-color`, `radius-xl`, `shadow-dialog`, padding `space-lg` (1.25rem below 641px); at least 12.5rem tall from 641px, so it does not jump between states. The backdrop is `scrim`.
- The heading is 1.25rem/600 with its icon; the bar is 4px, a `secondary-color` fill running on a `header-bg` track; the button sits at the right, and below 641px fills the width at 44px.
- **Its icons are inlined** as Lucide paths, not drawn from `/icons/`: it shows when the server cannot be reached, so it cannot fetch an icon then.
- **Motion:** it arrives after 0.3s, so a blip shows nothing, then slides up and fades in; the spinner turns and the bar runs. Under `prefers-reduced-motion: reduce` all of it stops, the dialog opaque at rest and the bar's fill standing where the design draws it.

## Contrast

Text 12.63:1 on `surface-color`; the bar 4.37:1 on its track; the icons: `secondary-color` 4.86:1, `warning-color` 5.77:1, `danger-color` 5.95:1 on white.

## Known gaps

No "Try now": the runtime has no call that skips its countdown.
