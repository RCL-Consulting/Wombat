# Icons

Wombat's 25 Lucide icons (MIT), copied from `src/Wombat.Web/wwwroot/icons/`. Each file is a 24-unit `<svg id="i">` holding bare paths with no stroke or fill attributes: app.css's `.icon` class draws them (fill none, stroke `currentColor`, stroke-width 2, round caps and joins) when `Icon.razor` references one with `<use href="/icons/{name}.svg#i">`. So a tile shown here as a plain `<img>` renders its paths filled black, which is not how the app draws it; the Icon component's preview shows them as drawn. Their ink is the text colour around them.

In use (20): alert-triangle, arrow-down, arrow-left, award, book, calendar, check, clock, file-text, home, inbox, info, key, log-out, plus, search, settings, shield, user, users.

Shipped but unused (5): arrow-up, chevron-right, pencil, trash, x.

Add an icon by copying its Lucide SVG into `wwwroot/icons/` and giving its root `id="i"`. Never use the Bootstrap Icons font, emoji or pictures instead.
