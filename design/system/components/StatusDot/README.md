# StatusDot

A 0.6rem coloured dot before a line of text, showing a service's status at a glance on the Administrator's System health card.

## What the consumer provides

`<span class="status-dot ok"></span>Database connection`: the class `ok` (`success-color`), `warn` (`warning-color`) or `err` (`danger-color`), and the words after it. The dot has a `space-sm` right margin and `radius-round`.

## Rules (DESIGN.md § Status dots; § Dashboard page)

- The words carry the state; the dot repeats it. Never a dot alone.
- Used only in the System health card today.

## Contrast

**Fails 3:1 for a meaningful mark, kept as the source has it:** `ok` 2.87:1 and `warn` 2.57:1 on `surface-color`. `err` passes at 3.82:1.

## Known gaps

The System health card's Email queue and Last nightly job rows are stubs that always show `warn`, labelled with task ids (T327).
