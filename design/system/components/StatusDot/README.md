# StatusDot

A 0.6rem coloured dot before a line of text, showing a service's status at a glance on the Administrator's System health card.

## What the consumer provides

`<li class="list-row"><span><span class="status-dot ok"></span>Database connection</span></li>`: the class `ok` (`success-color`), `warn` (`warning-color`) or `err` (`danger-color`), and the words after it. The dot is 0.6rem, round (50%, the one radius that is not a token), with a `space-sm` right margin.

## Rules (DESIGN.md § Status dots; § Dashboard page)

- The words carry the state; the dot repeats it. Never a dot alone.
- Used only in the System health card today.

## Contrast

A dot is a meaningful mark, so it needs 3:1: `ok` 5.88:1, `warn` 5.77:1, `err` 5.95:1 on `surface-color` (until T335 `ok` and `warn` were 2.87 and 2.57:1).

## Known gaps

The System health card's Email queue and Last nightly job rows are stubs that always show `warn`, labelled with task ids, "(T012)" and "(T024)" (T327).
