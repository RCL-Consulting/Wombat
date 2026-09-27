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

- **Decisions D1–D11** (2026-09-27): the operator accepted every recommendation ("accept all"). Two are recorded as
  project rules: W-010 (the GET role switch) and W-011 (OFL fonts; dark mode later). The rest shape the design:
  - D1: the role is remembered per account, stored with the account.
  - D3: the brand cell stays at the head of the sidebar.
  - D4: the active item's alpha is .32.
  - D5: breadcrumbs follow the owning list.
  - D6: access denied renders in place and names neither the page nor who may open it.
  - D7: the error bar's Dismiss is kept, with no reference.
  - D8: no "Acting as" heading without a role; My progress is a personal link.
  - D9: the phone menu's account row sits at its foot.
  - D10: "Page · Wombat" in sentence case on every page.
- **Round 3** (the same canvas, round-2 boards revised, seven added: `R2-Rules`, `R2-Sidebar-Scroll`,
  `R2-Phone-Zulu-Open`, `R2-Phone-Bars`, `R2-Error-Typed`, `R2-Reconnect-Narrow`, `R2-ErrorBar-Narrow`) applies the
  review's 30 fixes; `round-3/` holds it.
- **Round 3 accepted** (2026-09-27): every one of the 30 fixes is on the boards, checked on the text and the markup. It
  is the design step F builds. The canvas also corrected the review: #62779F was the blend at the old alpha (.37); at .32
  the swatch is #556C98, and white on it is 5.27:1. Three small things carry into the build, not into another round:
  - Rows 4 px apart with a 2 px ring at a 2 px offset make the ring touch the next row's box without overlapping it.
    That holds, because no row but the current one has a fill.
  - The reconnect and error-bar boards list their rules but not the runbook wording they change. The build rewrites
    those runbook steps anyway.
  - `R2-Rules` is headed "round 2". Its content is round 3's.

- **After round 3** (2026-09-27): the canvas's boards drew the mark as a CSS disc, because a canvas does not copy a
  design system's logos (BRIEF § 11). The real mark was copied into the canvas's assets, and canvas version
  `1790508781-630f` shows it (`/_blob/3ddf69ec3b0c226b928d7ee0c43990dd`, 32 px; 28 px on a phone). `round-3/` keeps the
  boards as accepted, with the disc; nothing else changed.
- **Built** as `b347e11c` (T335 step F), replayed on a fresh database with no regression (step G), and put into the
  design system (`064cde00`).

These files are HTML mockups written by the design tool. They load Google Fonts, which Wombat's CSP forbids, so they are
a picture of the design, not code to copy. The build (pilot step F) implements them in Razor and `app.css` under
`DESIGN.md`, as amended.
