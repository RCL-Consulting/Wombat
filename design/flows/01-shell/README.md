# Flow 01 · The shell — the design record

The pilot's design (T335), kept beside its brief (`../01-shell.md`) so that it outlives the canvas.

- **The canvas:** https://claude.ai/artifact/R86QvLEyyfKx98fT4MENcD (private), "Wombat · Flow 01 · The shell", built on the
  Wombat design system (W-009).
- **Round 1** (`round-1/`): three structural variations as wireframes.
  - A: a grouped sidebar with one acting role.
  - B: top-bar sections showing all roles at once.
  - C: Home is the map, with breadcrumbs everywhere.

  The overview (`Main.dc.html`) and each variation's spec (`A-Spec`, `B-Spec`, `C-Spec`) hold the navigation per role,
  the switching rules, the click counts and the reasoning. The shared states are in `S-Outcomes`, `S-Failures` and
  `S-Overlays`. The per-variation screens stay on the canvas.
- **The pick** (2026-09-27, the operator): **A**, the grouped sidebar with one acting role, with breadcrumbs on detail
  pages taken from C. Decided at the same time:
  - Stalled Activities and Programme Trainees are designed in flow 06, and the STAR Review Queue in flow 09.
  - Recent Activities and System are dropped.
  - A deep link switches only to a role the person holds.
  - Round 2 chose Source Sans 3 for body text (served from Google Fonts on the canvas; the build would self-host it
    under the OFL, decision D11). Dark mode is recorded as later, and reduced motion as in (review S17).
- **Round 2** (`round-2/`): A at full fidelity.
  - The shell at 1280 and 390 px.
  - The landing frame, with a two-role user, loading and load error.
  - Detail pages with breadcrumbs.
  - Access denied, not found and the error page, each signed in, signed out and at 390 px.
  - The reconnect dialog and the error bar.
  - The token sheet (`R2-Tokens.dc.html`).
  - `canvas.json` is the canvas's index at that version.

- **The round-2 review** (`round-2-review.md`) is a verdict of accept with changes: 22 should-fixes, the decisions
  D1–D11 for the operator, the round-3 message for the canvas, and the build plan for step F.

These files are HTML mockups written by the design tool. They load Google Fonts, which Wombat's CSP forbids, so they are
a picture of the design, not code to copy. The build (pilot step F) implements them in Razor and `app.css` under
`DESIGN.md`, as amended.
