# ReferenceBlock

What a person quotes to whoever looks into a failure: a description list of the reference, in the monospace face, and the time with its zone, on `header-bg`; the error page shows it.

New in flow 01 (T335, b347e11c; T321). Source: `Components/Shared/ReferenceBlock.razor`.

## What the consumer provides

`<ReferenceBlock Reference="@reference" Time="@time" />`

- `Reference` (required): the reference as the log has it. On the error page, the request's W3C trace id, 32 hexadecimal characters (`4bf92f3577b34da6a3ce929d0e0e4736`), never the 55-character traceparent; the request's own id where there is no trace.
- `Time` (required): when, with its zone, on the South African clock: "2026-09-26 15:14 SAST".
- `Label`: what the reference is called, "Reference" by default.

## Markup

```html
<dl class="reference-block">
  <dt>Reference</dt>
  <dd><code>4bf92f3577b34da6a3ce929d0e0e4736</code></dd>
  <dt>Time</dt>
  <dd>2026-09-26 15:14 SAST</dd>
</dl>
```

## Look

- A two-column grid (label, value), 0.9375rem, `header-bg`, `radius-md`, padding `space-sm` `space-md`; labels in `muted-text`; the reference in `font-mono` at 0.875rem, wrapping anywhere rather than widening the page.
- Below 641px one column, each label over its value, padded 0.75rem, 2px between a label and its value and 8px between the pairs, so a 32-character id has the block's whole width; the panel around it pads 16px.

## Rules

- A description list, so a screen reader reads each label with its value.
- A time is always shown with its zone.

## Contrast

`muted-text` 4.57:1 and `text-color` 11.36:1 on `header-bg`.
