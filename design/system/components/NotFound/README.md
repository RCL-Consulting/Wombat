# NotFound

The page for an address no page claims: "Page not found", "There is no page at this address.", and Go to Home; it echoes no address and says nothing about records elsewhere.

Flow 01 (T335, b347e11c; T233; R2-NotFound-*; S3, S4). Source: `Components/Pages/NotFound.razor`, `/not-found`; a browser's 404 is rerun there with its status kept, and the router shows it for an unknown address inside a circuit.

## What the consumer provides

Nothing: it is a page.

## States

- **Signed in:** a PageHeader "Page not found" with the `search` icon in `secondary-color`, and a `.system-panel`: "There is no page at this address.", "Check the address, or start again from Home." in `muted-text`, and Go to Home (`.btn-primary`, `home` icon).
- **Signed out** (only typing `/not-found` reaches it: an unknown address sends a visitor to sign in first): the signed-out bar and a `.system-card` with the icon above the heading, "There is no page at this address." and Go to Home with no icon.

The shape is AccessDenied's (`.system-panel`, `.system-card`, 44px stacked buttons below 641px).

## Rules

- **It never echoes the address:** a crafted link would put its own words on a Wombat page (T285's class), and the address bar shows it already.
- **It says nothing about another institution's records.** A record out of scope is answered 404 so that it is not known to exist; a sentence about it would undo that.
- Tab and heading: "Page not found · Wombat".

## Contrast

The `search` icon in `secondary-color`: 4.61:1 on the page, 4.86:1 in the white signed-out card. Text pairs pass.
