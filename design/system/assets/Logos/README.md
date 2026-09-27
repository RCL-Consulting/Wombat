# Logos

The Wombat mark, copied from `src/Wombat.Web/wwwroot/brand/`. Both are multi-colour SVGs with fixed inks (no `currentColor`): #2d6cdf to #16335a on the disc (the gradient runs top-left to bottom-right), #0e2747 for the burrow and the cross, white for the face. Show them as `<img>`, with `alt=""` beside the wordmark; never recolour, redraw or put them in `Icon`.

- `wombat-mark.svg`: the canonical mark, a 64-unit disc with transparent corners.
  - The sidebar's brand cell and the signed-out bar: 32px, beside the `wordmark` (Fraunces 1.6rem) in `nav-text-strong`, 10px apart, the whole cell the link home.
  - The phone bar: 28px, beside `wordmark-phone` (1.4rem), 8px apart.
  - The signed-out bar below 641px: 28px, beside the unchanged 1.6rem `wordmark`, 10px apart, with Sign in grown to 44px. MainLayout.razor.css shrinks `.brand-mark` everywhere below 641px but the word only in `.sidebar`. DESIGN.md § Logo & brand assets says the signed-out bar's mark is 32px, so this is probably a slip; scoping the rule to `.sidebar .brand-mark` would keep 32px and 1.6rem there. It is recorded as it ships.
  - The account card, on the sign-in page and the MSF respondent's only: 56px, beside `wordmark-large` (2.4rem) in `primary-color`.
- `wombat-tile.svg`: the same face on a full-bleed opaque square, used only to render the app-icon PNGs (see Favicons).

The asset store keeps a sanitised copy of each: the Inkscape editor metadata and the XML comments are removed, and the drawing is unchanged.
