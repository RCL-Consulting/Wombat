# AccessDenied

The page a person sees for a page none of their roles opens: signed in, why (in terms of the roles they hold, never the page or who could open it) and Go to Home; signed out, sign in and come back to the page asked for.

Flow 01 (T335, b347e11c; R2-Denied-*; D6, S1, S2) rebuilt it. Source: `Components/Pages/AccessDenied.razor`, `/access-denied`; the words are `Navigation/SystemPageText.cs`.

## What the consumer provides

Nothing: it is a page. It is reached two ways and drawn once either way: a full load of a refused page is sent to `/access-denied` by the sign-in cookie's AccessDeniedPath, and in-app navigation to one renders it in place, at the refused page's address.

## Signed in

A PageHeader "You cannot open this page" with the `ban` icon in `danger-color` (`IconTone="danger"`), and one `.system-panel`:

- "Your role (Trainee) does not open this page." or "None of your roles (Committee member, Assessor) opens this page." (the roles by label, in precedence order), or "Your account holds no role that opens this page."
- In `muted-text`: "If you need it for your work, ask your institution's Wombat administrator.", or "… ask the platform administrator." for an Institutional admin, a College admin or an Administrator (their institution's administrator would be themselves or nobody).
- Go to Home, `.btn-primary` with the `home` icon.

Nothing is lit in the nav, there is no trail (`Page="typeof(AccessDenied)"`), and no switch of role is offered: access is the union of the roles held, so no other role could open it.

## Signed out

Only typing the address reaches it. Under MainLayout's signed-out bar, a centred `.system-card`: the PageHeader "Sign in to open this page" with the `lock` icon above it and no rule; "After you sign in, Wombat brings you back to the page you asked for." when `ReturnUrl` is a path on this site (else "Sign in to carry on."); and Sign in, `.btn-primary`, with no icon, to `/account/login?ReturnUrl=…`.

## The system-page shape (DESIGN.md § System pages)

- `.system-panel`: `surface-color`, a `border-color` edge, `radius-lg`, `shadow-raised`, a 12px column gap, padding `space-lg`, at most 40rem.
- `.system-card-page` > `.system-card`: centred 80px under the bar (56px of its own and the article's 24px), 580px wide at most, `radius-xl`, padding `space-xl`.
- Below 641px the buttons stand one under another, each the panel's width and 44px tall; the signed-out card starts 16px under the bar and pads 24px.
- "Go to Home" is the way back everywhere. The tab is the heading's words: "You cannot open this page · Wombat".
- It reads no database, and neither does its shell.

## Contrast

The icon is a meaningful mark, 3:1 or more: the `ban` in `danger-color` 5.65:1 on the page; the `lock` in `secondary-color` 4.86:1 in the white card. Text pairs pass.
