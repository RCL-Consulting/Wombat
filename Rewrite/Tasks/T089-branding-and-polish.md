# T089 — Branding & polish (logo, palette, favicon, a11y nits)

**Status:** in progress (2026-06-08, Opus)

## Why
DESIGN.md shipped the palette as explicit TBD placeholders and the brand as a text-only "Wombat"
wordmark ("no icon until a logo exists"); `wwwroot` had no favicon/app-icons/manifest. The Appendix v2
re-run also surfaced two minor polish items (missing `arrow-left.svg` → 404; unlabeled nav toggler).
User chose a **full-identity** pass: refined navy/blue, logo designed from scratch (Concept C — wombat
face in a circle with a burrow motif).

## What changed
- **Palette** (`app.css :root` + DESIGN.md): `--secondary-color` `#3498db`→**`#2d6cdf`** (refined action
  blue, verified **4.86:1** white-on-`.btn-primary`, WCAG AA); added **`--accent-color: #3498db`**. Primary
  ink, sidebar gradient, semantics unchanged.
- **Logo** (`wwwroot/brand/`): `wombat-mark.svg` (round mark, transparent corners — canonical vector logo)
  and `wombat-tile.svg` (full-bleed opaque square for OS tiles). Wired into the nav brand lockup
  (`NavMenu.razor` + `.razor.css`) and the login card (`Login.razor` + `.account-brand` in app.css).
- **Favicon / app-icons / manifest** (`wwwroot/`): `favicon.svg`, `favicon.ico` (16/32/48),
  `apple-touch-icon.png` (180), `icon-192.png`, `icon-512.png`, `site.webmanifest`. Head links + theme-color
  added to `Components/App.razor`. Rasters generated from `wombat-tile.svg` (Playwright render → Pillow ICO).
- **Polish nits:** added `wwwroot/icons/arrow-left.svg` (Lucide; fixes the 3 Back-link 404s); nav toggler
  got `aria-label="Toggle navigation menu"` + `aria-expanded`.

## Verification
- Solution builds clean; Web bUnit + Domain/Application/Architecture suites green.
- Live screenshots: login card lockup, sidebar brand lockup, favicon in tab, Back-link icon renders.

## Notes
- Mark/tile colours are duplicated in the SVGs (not `:root`-driven, since they are standalone assets). To
  re-skin, regenerate the four raster files from the tile and keep them in sync with `:root` — documented in
  DESIGN.md § "Logo & brand assets".
- Concept-preview scratch lives under `.brand-preview/` (not committed).
