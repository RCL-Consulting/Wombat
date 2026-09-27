# Logos

The Wombat mark, copied from `src/Wombat.Web/wwwroot/brand/`. Both are multi-colour SVGs with fixed inks (no `currentColor`): #2d6cdf to #16335a on the disc (the gradient runs top-left to bottom-right), #0e2747 for the burrow and the cross, white for the face. Show them as `<img>`, with `alt=""` beside the wordmark; never recolour, redraw or put them in `Icon`.

- `wombat-mark.svg`: the canonical mark, a 64-unit disc with transparent corners. The sidebar lockup shows it at 40px beside the `wordmark`; account cards at 56px beside `wordmark-large`.
- `wombat-tile.svg`: the same face on a full-bleed opaque square, used only to render the app-icon PNGs (see Favicons).

The asset store keeps a sanitised copy of each: the Inkscape editor metadata and the XML comments are removed, and the drawing is unchanged.
